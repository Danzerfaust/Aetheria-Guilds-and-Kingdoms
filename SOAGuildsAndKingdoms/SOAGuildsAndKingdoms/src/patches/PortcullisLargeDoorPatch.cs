using System;
using System.Collections.Generic;
using System.Reflection;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;
using System.Linq;

namespace SOAGuildsAndKingdoms.src.patches
{
    /// <summary>
    /// Patch to add rank checks to custom portcullis "rank" variant doors.
    /// Also adds auto-close.
    /// 
    /// Million null checks here because this feels flimsy :-(
    /// </summary>
    public class PortcullisLargeDoorPatch
    {
        private const string RANK_VARIANT_NAME = "rank";
        private const int AUTO_CLOSE_DELAY_MS = 1000;

        private static readonly Dictionary<BlockPos, long> activeTimers = [];

        private static readonly Dictionary<BlockPos, (BlockPos controllerPos, long timestamp)> cellControllerCache = new();

        public static bool PrefixController(
            object __instance,
            IWorldAccessor world,
            IPlayer byPlayer,
            BlockSelection blockSel,
            ref EnumHandling handling,
            ref bool __result)
        {
            try
            {
                if (__instance == null || world == null || byPlayer == null || blockSel == null) return true;

                var blockProperty = __instance.GetType().GetProperty("Block");
                if (blockProperty == null) return true;

                var block = blockProperty.GetValue(__instance) as Block;
                if (block == null) return true;

                if (!CheckRankRequirement(world, byPlayer, block))
                {
                    handling = EnumHandling.PreventSubsequent;
                    __result = false;
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                world.Api.Logger.Error($"[PortcullisRankPatch] Exception in PrefixController: {ex.Message}");
                return true;
            }
        }

        public static bool PrefixCell(
            object __instance,
            IWorldAccessor world,
            IPlayer byPlayer,
            BlockSelection blockSel,
            ref bool __result)
        {
            try
            {
                if (__instance == null || world == null || byPlayer == null || blockSel == null || blockSel.Position == null) return true;

                var cellType = __instance.GetType();
                if (cellType == null) return true;
                var cellToAnchorField = cellType.GetField("CellToAnchor", BindingFlags.Public | BindingFlags.Static);
                if (cellToAnchorField == null) return true;

                var cellToAnchor = cellToAnchorField.GetValue(null) as System.Collections.IDictionary;
                if (cellToAnchor == null) return true;

                BlockPos? controllerPos = null;
                if (cellToAnchor.Contains(blockSel.Position))
                {
                    controllerPos = cellToAnchor[blockSel.Position] as BlockPos;
                }

                if (controllerPos == null) return true;

                var cellPosKey = blockSel.Position.Copy();
                if (cellPosKey == null) return true;
                cellControllerCache[cellPosKey] = (controllerPos.Copy(), DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());

                if (world.BlockAccessor == null) return true;

                var controllerBlock = world.BlockAccessor.GetBlock(controllerPos);
                if (controllerBlock == null) return true;

                if (!CheckRankRequirement(world, byPlayer, controllerBlock))
                {
                    __result = false;
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                world.Api.Logger.Error($"[PortcullisRankPatch] Exception in PrefixCell: {ex.Message}");
                return true;
            }
        }

        private static bool CheckRankRequirement(IWorldAccessor world, IPlayer byPlayer, Block block)
        {
            if (world == null || world.Api == null || byPlayer == null || block == null) return true;

            var characterSystem = world.Api.ModLoader?.GetModSystem<CharacterSystem>();
            if (characterSystem == null) return true;

            string? rank = null;
            if (block.Variant != null && block.Variant.ContainsKey(RANK_VARIANT_NAME))
            {
                rank = block.Variant[RANK_VARIANT_NAME];
            }

            if (string.IsNullOrEmpty(rank)) return true;

            var rankHierarchy = new Dictionary<string, int>
            {
                { "S", 4 },
                { "A", 3 },
                { "B", 2 },
                { "C", 1 }
            };

            if (!rankHierarchy.TryGetValue(rank.ToUpper(), out int doorRankLevel))
            {
                return true;
            }

            foreach (var kvp in rankHierarchy)
            {
                if (kvp.Value >= doorRankLevel)
                {
                    if (characterSystem.HasTrait(byPlayer, $"guild-rank-{kvp.Key}"))
                    {
                        return true;
                    }
                }
            }

            if (world.Side == EnumAppSide.Client)
            {
                (world.Api as ICoreClientAPI)?.TriggerIngameError(
                    "portcullis-rank-locked",
                    "locked",
                    Lang.Get("soaguildsandkingdoms:grsdoor-locked")
                );
            }

            return false;
        }

        public static void PostfixController(
            object __instance,
            IWorldAccessor world,
            IPlayer byPlayer,
            BlockSelection blockSel,
            bool __result)
        {
            try
            {
                if (__instance == null || world == null || byPlayer == null || blockSel == null) return;

                if (!__result || world.Side != EnumAppSide.Server) return;

                var blockProperty = __instance.GetType().GetProperty("Block");
                if (blockProperty == null) return;

                var block = blockProperty.GetValue(__instance) as Block;
                if (block == null) return;

                string? rank = null;
                if (block.Variant != null && block.Variant.ContainsKey(RANK_VARIANT_NAME))
                {
                    rank = block.Variant[RANK_VARIANT_NAME];
                }

                if (string.IsNullOrEmpty(rank)) return;

                var posProperty = __instance.GetType().GetProperty("Pos");
                if (posProperty == null) return;

                var controllerPos = posProperty.GetValue(__instance) as BlockPos;
                if (controllerPos == null) return;

                var isOpenedField = __instance.GetType().GetField("opened", BindingFlags.NonPublic | BindingFlags.Instance);
                if (isOpenedField == null) return;

                bool isOpened = (bool)(isOpenedField.GetValue(__instance) ?? false);

                if (isOpened)
                {
                    ScheduleAutoClose(world, __instance, controllerPos);
                }
                else
                {
                    var posKey = controllerPos.Copy();
                    if (posKey != null && activeTimers.TryGetValue(posKey, out long existingTimerId))
                    {
                        try
                        {
                            world.UnregisterCallback(existingTimerId);
                        }
                        catch
                        {
                        }
                        activeTimers.Remove(posKey);
                    }
                }
            }
            catch (Exception ex)
            {
                world.Api.Logger.Error($"[PortcullisRankPatch] Exception in PostfixController: {ex.Message}");
            }
        }

        public static void PostfixCell(
            object __instance,
            IWorldAccessor world,
            IPlayer byPlayer,
            BlockSelection blockSel,
            bool __result)
        {
            try
            {
                if (__instance == null || world == null || world.BlockAccessor == null || byPlayer == null || blockSel == null || blockSel.Position == null) return;

                if (!__result || world.Side != EnumAppSide.Server)
                {
                    return;
                }

                var cellPosKey = blockSel.Position.Copy();
                if (cellPosKey == null) return;

                if (!cellControllerCache.TryGetValue(cellPosKey, out var cached))
                {
                    return;
                }

                var controllerPos = cached.controllerPos;
                if (controllerPos == null) return;

                if (world.BlockAccessor == null) return;

                var controllerBlock = world.BlockAccessor.GetBlock(controllerPos);
                if (controllerBlock == null) return;

                string? rank = null;
                if (controllerBlock.Variant != null && controllerBlock.Variant.ContainsKey(RANK_VARIANT_NAME))
                {
                    rank = controllerBlock.Variant[RANK_VARIANT_NAME];
                }

                if (string.IsNullOrEmpty(rank)) return;

                var timestamp = cached.timestamp;

                var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                var keysToRemove = cellControllerCache.Where(kvp => now - kvp.Value.timestamp > 5000).Select(kvp => kvp.Key).ToList();
                foreach (var key in keysToRemove)
                {
                    cellControllerCache.Remove(key);
                }

                cellControllerCache.Remove(cellPosKey);

                var controllerBe = world.BlockAccessor.GetBlockEntity(controllerPos);
                if (controllerBe == null) return;

                var behaviorsField = controllerBe.GetType().GetField("Behaviors", BindingFlags.Public | BindingFlags.Instance);
                if (behaviorsField == null) return;

                var behaviors = behaviorsField.GetValue(controllerBe) as System.Collections.IList;
                if (behaviors == null) return;

                object? doorBehavior = null;
                foreach (var behavior in behaviors)
                {
                    if (behavior == null || behavior.GetType() == null) continue;

                    var behaviorTypeName = behavior.GetType().Name;
                    if (behaviorTypeName != null && behaviorTypeName.Contains("LargeDoor"))
                    {
                        doorBehavior = behavior;
                        break;
                    }
                }

                if (doorBehavior == null) return;

                var isOpenedField = doorBehavior.GetType().GetField("opened", BindingFlags.NonPublic | BindingFlags.Instance);
                if (isOpenedField == null) return;

                bool isOpened = (bool)(isOpenedField.GetValue(doorBehavior) ?? false);

                if (isOpened)
                {
                    ScheduleAutoClose(world, doorBehavior, controllerPos);
                }
                else
                {
                    var posKey = controllerPos.Copy();
                    if (posKey != null && activeTimers.TryGetValue(posKey, out long existingTimerId))
                    {
                        try
                        {
                            world.UnregisterCallback(existingTimerId);
                        }
                        catch
                        {
                        }
                        activeTimers.Remove(posKey);
                    }
                }
            }
            catch (Exception ex)
            {
                world.Api.Logger.Error($"[PortcullisRankPatch] Exception in PostfixCell: {ex.Message}");
            }
        }

        private static void ScheduleAutoClose(IWorldAccessor world, object doorBehaviorInstance, BlockPos controllerPos)
        {
            try
            {
                if (world == null || doorBehaviorInstance == null || controllerPos == null) return;

                var posKey = controllerPos.Copy();
                if (posKey == null) return;

                if (activeTimers.TryGetValue(posKey, out long existingTimerId))
                {
                    try
                    {
                        world.UnregisterCallback(existingTimerId);
                    }
                    catch
                    {
                    }
                    activeTimers.Remove(posKey);
                }

                long timerId = world.RegisterCallback((dt) =>
                {
                    try
                    {
                        if (world == null || world.BlockAccessor == null || doorBehaviorInstance == null || posKey == null) return;

                        var controllerBe = world.BlockAccessor.GetBlockEntity(posKey);
                        if (controllerBe == null) return;

                        activeTimers.Remove(posKey);

                        var behaviorType = doorBehaviorInstance.GetType();
                        if (behaviorType == null) return;

                        var isOpenedField = behaviorType.GetField("opened", BindingFlags.NonPublic | BindingFlags.Instance);
                        if (isOpenedField == null) return;

                        var openedValue = isOpenedField.GetValue(doorBehaviorInstance);
                        if (openedValue == null) return;

                        bool isOpened = (bool)openedValue;
                        if (!isOpened) return;

                        var toggleMethod = behaviorType.GetMethod("ToggleLargeDoorState", BindingFlags.Public | BindingFlags.Instance);
                        if (toggleMethod != null)
                        {
                            try
                            {
                                toggleMethod.Invoke(doorBehaviorInstance, [null, false]);
                            }
                            catch
                            {
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        world.Api.Logger.Error($"[PortcullisRankPatch] Exception during auto-close: {ex.Message}");
                    }
                }, AUTO_CLOSE_DELAY_MS);

                activeTimers[posKey] = timerId;
            }
            catch (Exception ex)
            {
                world.Api.Logger.Error($"[PortcullisRankPatch] Exception in ScheduleAutoClose: {ex.Message}");
            }
        }
    }
}
