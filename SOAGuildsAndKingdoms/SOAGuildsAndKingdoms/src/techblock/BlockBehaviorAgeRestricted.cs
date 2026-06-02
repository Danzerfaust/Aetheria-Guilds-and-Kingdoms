using System;
using System.Linq;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.API.Util;

namespace SOAGuildsAndKingdoms.src.techblock
{
    /// <summary>
    /// Block behavior that restricts mining based on the current world age configuration
    /// </summary>
    public class BlockBehaviorAgeRestricted : BlockBehavior
    {
        private TechAge requiredAge;
        private string requiredTrait;
        private ICoreAPI api;

        public BlockBehaviorAgeRestricted(Block block) : base(block)
        {
        }

        public override void Initialize(JsonObject properties)
        {
            base.Initialize(properties);

            // Parse the required age from the block's properties
            var ageString = properties["requiredAge"].AsString("Stone");
            if (!Enum.TryParse<TechAge>(ageString, true, out requiredAge))
            {
                requiredAge = TechAge.Stone;
            }

            // Parse the required trait from the block's properties
            requiredTrait = properties["requiredTrait"].AsString(null);
        }

        public override void OnLoaded(ICoreAPI api)
        {
            base.OnLoaded(api);
            this.api = api;
        }

        /// <summary>
        /// Checks if a player has the required trait to access this resource
        /// </summary>
        private bool PlayerHasRequiredTrait(IPlayer player)
        {
            // If no trait is required, allow access
            if (string.IsNullOrWhiteSpace(requiredTrait))
                return true;

            // Check the player's traits directly from their entity's watched attributes
            var watchedAttributes = player?.Entity?.WatchedAttributes;
            if (watchedAttributes == null)
                return false;

            // Get the player's extra traits array (where guild-granted traits are stored)
            var extraTraits = watchedAttributes.GetStringArray("extraTraits");
            if (extraTraits == null || extraTraits.Length == 0)
                return false; // No traits = can't mine

            // Check if the player has the required trait
            return extraTraits.Contains(requiredTrait);
        }

        public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel, ref EnumHandling handling)
        {
            // Allow creative mode players to bypass age restrictions
            if (byPlayer?.WorldData?.CurrentGameMode == EnumGameMode.Creative)
            {
                return base.OnBlockInteractStart(world, byPlayer, blockSel, ref handling);
            }

            // Get mod system reference
            var modSystem = api?.ModLoader.GetModSystem<SOAGuildsAndKingdomsModSystem>();

            // Check if the required age is enabled globally
            if (modSystem?.TechBlocksConfig != null)
            {
                if (!modSystem.TechBlocksConfig.IsAgeEnabled(requiredAge))
                {
                    // Age not enabled - prevent mining
                    if (world.Side == EnumAppSide.Client)
                    {
                        (api as ICoreClientAPI)?.ShowChatMessage(
                            $"This ore cannot be mined until the {requiredAge} Age is enabled."
                        );
                    }

                    handling = EnumHandling.PreventDefault;
                    return false;
                }
            }

            // Check if player has the required trait (guild tech unlock)
            if (!PlayerHasRequiredTrait(byPlayer))
            {
                // Show message on client side for immediate feedback
                if (world.Side == EnumAppSide.Client)
                {
                    var message = !string.IsNullOrWhiteSpace(requiredTrait)
                        ? $"Your guild hasn't researched the required technology ({requiredTrait}) to mine this resource."
                        : "Your guild hasn't researched the required technology to mine this resource.";
                    (api as ICoreClientAPI)?.ShowChatMessage(message);
                }
                // Also send error on server side for security
                else if (world.Side == EnumAppSide.Server)
                {
                    var serverPlayer = byPlayer as IServerPlayer;
                    if (serverPlayer != null)
                    {
                        var message = !string.IsNullOrWhiteSpace(requiredTrait)
                            ? $"Your guild hasn't researched the required technology ({requiredTrait}) to mine this resource."
                            : "Your guild hasn't researched the required technology to mine this resource.";
                        serverPlayer.SendIngameError("age_restricted", message);
                    }
                }

                handling = EnumHandling.PreventDefault;
                return false;
            }

            return base.OnBlockInteractStart(world, byPlayer, blockSel, ref handling);
        }

        public override void OnBlockBroken(IWorldAccessor world, BlockPos pos, IPlayer byPlayer, ref EnumHandling handling)
        {
            // Allow creative mode players to bypass age restrictions
            if (byPlayer?.WorldData?.CurrentGameMode == EnumGameMode.Creative)
            {
                base.OnBlockBroken(world, pos, byPlayer, ref handling);
                return;
            }

            // Get mod system reference
            var modSystem = api?.ModLoader.GetModSystem<SOAGuildsAndKingdomsModSystem>();

            // Double-check on block break as well
            if (modSystem?.TechBlocksConfig != null)
            {
                if (!modSystem.TechBlocksConfig.IsAgeEnabled(requiredAge))
                {
                    // Age not enabled - prevent breaking
                    handling = EnumHandling.PreventDefault;
                    return;
                }
            }

            // Check if player has the required trait on both client and server
            if (!PlayerHasRequiredTrait(byPlayer))
            {
                handling = EnumHandling.PreventDefault;
                return;
            }

            base.OnBlockBroken(world, pos, byPlayer, ref handling);
        }

        public override bool OnBlockInteractStep(float secondsUsed, IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel, ref EnumHandling handling)
        {
            // Allow creative mode players to bypass age restrictions
            if (byPlayer?.WorldData?.CurrentGameMode == EnumGameMode.Creative)
            {
                return base.OnBlockInteractStep(secondsUsed, world, byPlayer, blockSel, ref handling);
            }

            // Get mod system reference
            var modSystem = api?.ModLoader.GetModSystem<SOAGuildsAndKingdomsModSystem>();

            // Check during mining animation as well
            if (modSystem?.TechBlocksConfig != null)
            {
                if (!modSystem.TechBlocksConfig.IsAgeEnabled(requiredAge))
                {
                    handling = EnumHandling.PreventDefault;
                    return false;
                }
            }

            // Check if player has the required trait on both client and server
            if (!PlayerHasRequiredTrait(byPlayer))
            {
                handling = EnumHandling.PreventDefault;
                return false;
            }

            return base.OnBlockInteractStep(secondsUsed, world, byPlayer, blockSel, ref handling);
        }
    }
}
