using System;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;
using Vintagestory.API.Config;

namespace SOAGuildsAndKingdoms.src.quests.blocks
{
    public class QuestBoardBlock : Block
    {
        private WorldInteraction[] interactions = [];

        protected bool isWallBoard;

        public override void OnLoaded(ICoreAPI api)
        {
            base.OnLoaded(api);

            isWallBoard = Variant["attachment"] == "wall";

            interactions = ObjectCacheUtil.GetOrCreate(api, $"questBoardInteraction-{Variant["questType"]}", () =>
            {
                return new WorldInteraction[] { new()
                    {
                        ActionLangCode = $"soaguildsandkingdoms:quests-view-{Variant["questType"]}",
                        MouseButton = EnumMouseButton.Right,
                    }
                };
            });
        }

        public override WorldInteraction[] GetPlacedBlockInteractionHelp(IWorldAccessor world, BlockSelection selection, IPlayer forPlayer)
        {
            return interactions.Append(base.GetPlacedBlockInteractionHelp(world, selection, forPlayer));
        }

        public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
        {
            if (world.Side == EnumAppSide.Client)
            {
                if (world.Api is not ICoreClientAPI capi) return false;

                var isWeeklyBoard = Variant["questType"] == "daily-weekly";

                var modSystem = capi.ModLoader.GetModSystem<SOAGuildsAndKingdomsModSystem>();
                if (modSystem == null) return false;

                if (isWeeklyBoard)
                {
                    int chunkX = guilds.LandClaim.FloorDiv(blockSel.Position.X, guilds.LandClaim.ChunkSize);
                    int chunkZ = guilds.LandClaim.FloorDiv(blockSel.Position.Z, guilds.LandClaim.ChunkSize);
                    var owningGuild = modSystem.GetChunkOwner(chunkX, chunkZ);

                    // If the board isn't in a claim, don't open the dialog
                    if (owningGuild == null)
                    {
                        capi.ShowChatMessage(Lang.Get("soaguildsandkingdoms:quest-board-not-in-claim"));
                        return false;
                    }

                    // If the player doesn't have interact permission for the owning guild, don't open the quest dialog
                    if (!owningGuild.HasInteractPermission)
                    {
                        capi.ShowChatMessage(Lang.Get("soaguildsandkingdoms:quest-board-wrong-claim"));
                        return false;
                    }
                }

                gui.dialogs.QuestDialog.CloseCurrentDialog();
                var questDialog = new gui.dialogs.QuestDialog(capi, modSystem, Variant["questType"]);
                questDialog.TryOpen();

                return true;
            }

            return base.OnBlockInteractStart(world, byPlayer, blockSel);
        }

        public override bool TryPlaceBlock(IWorldAccessor world, IPlayer byPlayer, ItemStack itemstack, BlockSelection blockSel, ref string failureCode)
        {
            BlockPos supportingPos = blockSel.Position.AddCopy(blockSel.Face.Opposite);
            Block supportingBlock = world.BlockAccessor.GetBlock(supportingPos);

            if (blockSel.Face.IsHorizontal && (supportingBlock.CanAttachBlockAt(world.BlockAccessor, this, supportingPos, blockSel.Face) || supportingBlock.GetAttributes(world.BlockAccessor, supportingPos)?.IsTrue("partialAttachable") == true))
            {
                Block wallblock = world.BlockAccessor.GetBlock(CodeWithParts("wall", blockSel.Face.Opposite.Code));

                if (!wallblock.CanPlaceBlock(world, byPlayer, blockSel, ref failureCode))
                {
                    return false;
                }

                world.BlockAccessor.SetBlock(wallblock.BlockId, blockSel.Position);

                return true;
            }

            if (!CanPlaceBlock(world, byPlayer, blockSel, ref failureCode))
            {
                return false;
            }

            BlockFacing[] horVer = SuggestedHVOrientation(byPlayer, blockSel);
            AssetLocation blockCode = CodeWithParts(horVer[0].Code);
            Block block = world.BlockAccessor.GetBlock(blockCode);
            world.BlockAccessor.SetBlock(block.BlockId, blockSel.Position);

            BlockPos targetPos = blockSel.DidOffset ? blockSel.Position.AddCopy(blockSel.Face.Opposite) : blockSel.Position;
            double dx = byPlayer.Entity.Pos.X - (targetPos.X + blockSel.HitPosition.X);
            double dz = (float)byPlayer.Entity.Pos.Z - (targetPos.Z + blockSel.HitPosition.Z);
            float angleHor = (float)Math.Atan2(dx, dz);

            float deg45 = GameMath.PIHALF / 2;
            float roundRad = ((int)Math.Round(angleHor / deg45)) * deg45;

            return true;
        }

        public override void OnBlockBroken(IWorldAccessor world, BlockPos pos, IPlayer? byPlayer, float dropQuantityMultiplier = 1)
        {
            if (world.Side == EnumAppSide.Server)
            {
                if (byPlayer?.WorldData.CurrentGameMode == EnumGameMode.Creative)
                {
                    base.OnBlockBroken(world, pos, byPlayer, dropQuantityMultiplier);
                    return;
                }

                Block block = world.BlockAccessor.GetBlock(CodeWithParts("ground", "north"));
                block ??= world.BlockAccessor.GetBlock(CodeWithParts("wall", "north"));
                ItemStack[] dropStacks = [new ItemStack(block)];

                if (dropStacks != null)
                {
                    foreach (ItemStack stack in dropStacks)
                    {
                        if (stack != null)
                        {
                            world.SpawnItemEntity(stack, pos.ToVec3d().Add(0.5, 0.5, 0.5));
                        }
                    }
                }
            }

            SpawnBlockBrokenParticles(pos, byPlayer);
            world.BlockAccessor.SetBlock(0, pos);
            if (!world.Side.IsServer()) return;
        }
    }
}
