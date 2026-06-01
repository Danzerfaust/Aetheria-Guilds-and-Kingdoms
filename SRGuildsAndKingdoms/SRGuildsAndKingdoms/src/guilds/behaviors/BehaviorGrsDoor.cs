using System;
using System.Collections.Generic;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.GameContent;

namespace SRGuildsAndKingdoms.src.guilds.behaviors
{
    public class BlockBehaviorGrsDoor(Block block) : BlockBehavior(block)
    {
        private const string RANK_VARIANT_NAME = "rank";
        public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel, ref EnumHandling handling)
        {
            handling = EnumHandling.PreventSubsequent;

            bool interactResult = HandleBlockInteract(world, byPlayer);

            if (interactResult)
            {
                base.OnBlockInteractStart(world, byPlayer, blockSel, ref handling);
            }

            return interactResult;
        }

        private bool HandleBlockInteract(IWorldAccessor world, IPlayer byPlayer)
        {
            // If no VS character system or rank variant is missing, allow interaction
            var characterSystem = world.Api.ModLoader.GetModSystem<CharacterSystem>();
            if (characterSystem == null) return true;

            string? rank = block.Variant[RANK_VARIANT_NAME];

            if (rank == null || rank == "")
            {
                return true;
            }

            var rankHierarchy = new Dictionary<string, int>
            {
                { "S", 4 },
                { "A", 3 },
                { "B", 2 },
                { "C", 1 }
            };

            // Get the door's rank level
            if (!rankHierarchy.TryGetValue(rank.ToUpper(), out int doorRankLevel))
            {
                // Unknown rank, allow interaction
                return true;
            }

            // Check if player has any rank that is high enough to open this door
            foreach (var kvp in rankHierarchy)
            {
                if (kvp.Value >= doorRankLevel)
                {
                    if (characterSystem.HasTrait(byPlayer, $"guild-rank-{kvp.Key}"))
                    {
                        // Player has a rank that can open this door
                        return true;
                    }
                }
            }

            // Player doesn't have required rank, show error on client
            if (world.Side == EnumAppSide.Client)
            {
                (world.Api as ICoreClientAPI)?.TriggerIngameError(this, "locked", Lang.Get("srguildsandkingdoms:grsdoor-locked"));
            }

            return false;
        }
    }
}
