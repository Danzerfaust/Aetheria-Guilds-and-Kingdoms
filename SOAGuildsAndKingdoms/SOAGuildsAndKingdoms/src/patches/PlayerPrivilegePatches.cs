using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Server;

namespace SOAGuildsAndKingdoms.src.patches
{
    /// <summary>
    /// Server-side patch for WorldMap.TestBlockAccess to enforce guild permissions and protected zones
    /// This is the authoritative check that prevents unauthorized block interactions
    /// </summary>
    public class WorldMapTestBlockAccessPatch
    {
        public static bool Prefix(IPlayer player, BlockSelection blockSel, EnumBlockAccessFlags accessType, out string claimant, ref EnumWorldAccessResponse __result)
        {
            claimant = "";
            __result = EnumWorldAccessResponse.Granted;

            if (player?.Entity?.World?.Api == null || blockSel?.Position == null)
                return true;

            var api = player.Entity.World.Api;
            var modSystem = api.ModLoader?.GetModSystem<SOAGuildsAndKingdomsModSystem>();
            if (modSystem == null) return true;

            // Allow creative mode players to bypass all restrictions
            if (player?.WorldData?.CurrentGameMode == EnumGameMode.Creative)
                return true;

            int chunkX = SOAGuildsAndKingdoms.src.guilds.LandClaim.FloorDiv(blockSel.Position.X, SOAGuildsAndKingdoms.src.guilds.LandClaim.ChunkSize);
            int chunkZ = SOAGuildsAndKingdoms.src.guilds.LandClaim.FloorDiv(blockSel.Position.Z, SOAGuildsAndKingdoms.src.guilds.LandClaim.ChunkSize);

            // SERVER-SIDE CHECK
            if (api.Side == EnumAppSide.Server && player is IServerPlayer serverPlayer)
            {
                // Check protected zones first (they take precedence over guild claims)
                var guildManager = modSystem.GetGuildManager();
                var config = guildManager?.GetConfigManager()?.GetConfig();
                var spawnPos = (api as ICoreServerAPI)?.World.DefaultSpawnPosition.AsBlockPos;

                if (config != null && spawnPos != null && config.IsWithinProtectedZone(blockSel.Position.X, blockSel.Position.Z, spawnPos))
                {
                    var zone = config.GetProtectedZoneAt(blockSel.Position.X, blockSel.Position.Z, spawnPos);

                    // Check if player is whitelisted for this zone
                    if (zone != null && modSystem.GetZoneWhitelistManager()?.IsPlayerWhitelisted(zone.Id, serverPlayer.PlayerUID) == true)
                    {
                        return true; // Allow whitelisted players full access
                    }

                    // For non-whitelisted players in protected zones:
                    // - Allow "Use" actions (opening inventories, using doors, etc.)
                    // - Block "BuildOrBreak" actions (placing/breaking blocks)
                    if (zone != null)
                    {
                        // Allow interaction/use actions for everyone
                        if (accessType == EnumBlockAccessFlags.Use)
                        {
                            return true; // Allow using/interacting with blocks (chests, doors, etc.)
                        }

                        // Block build/break actions for non-whitelisted players
                        if (accessType == EnumBlockAccessFlags.BuildOrBreak)
                        {
                            claimant = zone.Name;
                            serverPlayer.SendMessage(GlobalConstants.GeneralChatGroup,
                                $"You cannot build or break blocks in protected zone: {zone.Name}",
                                EnumChatType.Notification);
                            __result = EnumWorldAccessResponse.LandClaimed;
                            return false;
                        }
                    }
                }

                // Check guild land claims
                bool guildAllows = false;

                if (accessType == EnumBlockAccessFlags.Use)
                {
                    guildAllows = modSystem.CheckGuildUsePrivilege(serverPlayer, blockSel.Position);
                }
                else if (accessType == EnumBlockAccessFlags.BuildOrBreak)
                {
                    guildAllows = modSystem.CheckGuildBuildPrivilege(serverPlayer, blockSel.Position);
                }

                if (!guildAllows)
                {
                    // Get the owning guild via reflection
                    var owningGuild = modSystem.GetType()
                        .GetMethod("GetChunkOwningGuild", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                        ?.Invoke(modSystem, new object[] { chunkX, chunkZ });

                    if (owningGuild != null)
                    {
                        var guildName = owningGuild.GetType().GetProperty("Name")?.GetValue(owningGuild)?.ToString();
                        if (!string.IsNullOrEmpty(guildName))
                        {
                            claimant = guildName;
                            __result = EnumWorldAccessResponse.LandClaimed;
                            return false; // Block the action
                        }
                    }
                }
            }
            // CLIENT-SIDE CHECK
            else if (api.Side == EnumAppSide.Client)
            {
                // Check protected zones first (they take precedence over guild claims)
                var protectedZoneInfo = modSystem.CheckProtectedZone(blockSel.Position.X, blockSel.Position.Z);
                if (protectedZoneInfo.HasValue && protectedZoneInfo.Value.isProtected)
                {
                    // Allow whitelisted players to build/break in protected zones
                    if (player != null && protectedZoneInfo.Value.whitelistedPlayers.Contains(player.PlayerUID))
                    {
                        return true;
                    }

                    // Allow "Use" actions (opening inventories, using doors, etc.) for everyone
                    if (accessType == EnumBlockAccessFlags.Use)
                    {
                        // Continue with original method - allow interaction
                    }
                    // Block "BuildOrBreak" actions for non-whitelisted players (server will validate whitelist)
                    else if (accessType == EnumBlockAccessFlags.BuildOrBreak)
                    {
                        claimant = protectedZoneInfo.Value.zoneName;
                        __result = EnumWorldAccessResponse.LandClaimed;
                        return false; // Block the action - server will validate whitelist
                    }
                }

                // Check guild land claims
                var owningGuild = modSystem.GetChunkOwner(chunkX, chunkZ);

                if (owningGuild != null)
                {
                    claimant = owningGuild.Name;

                    // If player is not a member of the owning guild, deny access
                    if (!owningGuild.IsPlayerMember)
                    {
                        __result = EnumWorldAccessResponse.LandClaimed;
                        return false; // Block the action on client side
                    }

                    // Player is a member - check permissions based on action type
                    bool hasPermission = false;

                    if (accessType == EnumBlockAccessFlags.BuildOrBreak)
                    {
                        // Check if player's role has BreakAndPlaceBlocks permission (synced from server)
                        hasPermission = owningGuild.HasBreakPlacePermission;
                    }
                    else if (accessType == EnumBlockAccessFlags.Use)
                    {
                        // Check if player's role has InteractBlocks permission (synced from server)
                        hasPermission = owningGuild.HasInteractPermission;
                    }

                    if (!hasPermission)
                    {
                        __result = EnumWorldAccessResponse.LandClaimed;
                        return false; // Block the action
                    }
                }
            }

            return true; // Continue with original method
        }
    }
}
