using SRGuildsAndKingdoms.src.guilds;
using System;
using System.Collections.Generic;
using System.Linq;
using Vintagestory.API.Server;
using Vintagestory.GameContent;

namespace SRGuildsAndKingdoms.src.player
{
    /// <summary>
    /// Manages synchronization of guild research traits to player characters
    /// Automatically grants and revokes recipe-unlocking traits based on guild tech progress
    /// </summary>
    public class GuildTraitManager
    {
        private readonly ICoreServerAPI serverApi;
        private readonly SRGuildsAndKingdomsModSystem modSystem;
        private CharacterSystem? characterSystem;

        public GuildTraitManager(ICoreServerAPI api, SRGuildsAndKingdomsModSystem modSystem)
        {
            this.serverApi = api;
            this.modSystem = modSystem;

            // Get the CharacterSystem from the recipe blocker mod
            this.characterSystem = api.ModLoader.GetModSystem<CharacterSystem>();

            if (characterSystem == null)
            {
                //api.Logger.Warning("[GuildTraitManager] CharacterSystem not found - trait syncing will not work!");
            }
        }

        /// <summary>
        /// Synchronize all traits for a player based on their guild's tech progress
        /// </summary>
        public void SyncPlayerTraits(IServerPlayer player)
        {
            serverApi.Logger.Debug($"[GuildTraitManager] === Starting trait sync for player {player?.PlayerName} ===");

            if (characterSystem == null)
            {
                serverApi.Logger.Error("[GuildTraitManager] CharacterSystem is null! Cannot sync traits.");
                return;
            }

            if (player == null)
            {
                serverApi.Logger.Error("[GuildTraitManager] Player is null! Cannot sync traits.");
                return;
            }

            var guildManager = modSystem.GetGuildManager();
            if (guildManager == null)
            {
                serverApi.Logger.Error("[GuildTraitManager] GuildManager is null! Cannot sync traits.");
                return;
            }

            // Get player's guild
            var guild = guildManager.GetGuildByMember(player.PlayerUID);

            if (guild == null)
            {
                //serverApi.Logger.Debug($"[GuildTraitManager] Player {player.PlayerName} has no guild - removing all guild-granted traits");
                // Player has no guild - remove all guild-granted traits
                RemoveAllGuildTraits(player);
                //serverApi.Logger.Debug($"[GuildTraitManager] === Finished trait sync for player {player.PlayerName} (no guild) ===");
                return;
            }

            //serverApi.Logger.Debug($"[GuildTraitManager] Player {player.PlayerName} is in guild '{guild.Name}'");

            // Get all traits the player SHOULD have based on guild tech
            var requiredTraits = GetRequiredTraitsForGuild(guild);
            //serverApi.Logger.Debug($"[GuildTraitManager] Required traits for guild '{guild.Name}': [{string.Join(", ", requiredTraits)}] (Count: {requiredTraits.Count})");

            // Get all traits the player currently has (guild-granted ones)
            var currentGuildTraits = GetPlayerGuildTraits(player);
            //serverApi.Logger.Debug($"[GuildTraitManager] Current guild traits on player: [{string.Join(", ", currentGuildTraits)}] (Count: {currentGuildTraits.Count})");

            // Add missing traits
            int grantedCount = 0;
            foreach (var trait in requiredTraits)
            {
                if (!currentGuildTraits.Contains(trait))
                {
                    //serverApi.Logger.Notification($"[GuildTraitManager] Player {player.PlayerName} is MISSING trait '{trait}' - attempting to grant...");
                    GrantTrait(player, trait);
                    grantedCount++;
                }
                else
                {
                    //serverApi.Logger.Debug($"[GuildTraitManager] Player {player.PlayerName} already has trait '{trait}'");
                }
            }

            // Remove extra traits (tech was lost or player switched guilds)
            int revokedCount = 0;
            foreach (var trait in currentGuildTraits)
            {
                if (!requiredTraits.Contains(trait))
                {
                    //serverApi.Logger.Notification($"[GuildTraitManager] Player {player.PlayerName} has EXTRA trait '{trait}' - revoking...");
                    RevokeTrait(player, trait);
                    revokedCount++;
                }
            }

            //serverApi.Logger.Notification($"[GuildTraitManager] === Finished trait sync for player {player.PlayerName}: Granted {grantedCount}, Revoked {revokedCount} ===");
        }

        /// <summary>
        /// Synchronize traits for all members of a guild (called when guild researches new tech)
        /// </summary>
        public void SyncGuildMemberTraits(Guild guild)
        {
            if (guild == null)
                return;

            foreach (var memberUid in guild.Members.Keys)
            {
                var player = serverApi.World.PlayerByUid(memberUid) as IServerPlayer;
                if (player != null)
                {
                    SyncPlayerTraits(player);
                }
            }
        }

        /// <summary>
        /// Get all traits that should be granted based on guild's unlocked techs
        /// </summary>
        private HashSet<string> GetRequiredTraitsForGuild(Guild guild)
        {
            //serverApi.Logger.Debug($"[GuildTraitManager] Getting required traits for guild '{guild.Name}'");
            var traits = new HashSet<string>();
            var techBlocks = modSystem.TechBlocks;

            //serverApi.Logger.Debug($"[GuildTraitManager] Total tech blocks available: {techBlocks.Count}");
            //serverApi.Logger.Debug($"[GuildTraitManager] Guild '{guild.Name}' has {guild.TechProgress.Count} tech progress entries");

            int unlockedCount = 0;
            foreach (var techProgress in guild.TechProgress.Values)
            {
                var techBlock = techBlocks.FirstOrDefault(tb => tb.Id == techProgress.TechBlockId);
                var techName = techBlock?.Text ?? $"Unknown (ID: {techProgress.TechBlockId})";

                //serverApi.Logger.Debug($"[GuildTraitManager]   Tech '{techName}' (ID: {techProgress.TechBlockId}) - Unlocked: {techProgress.IsUnlocked}");

                if (!techProgress.IsUnlocked)
                    continue;

                unlockedCount++;

                if (techBlock == null)
                {
                    //serverApi.Logger.Warning($"[GuildTraitManager]   Tech ID {techProgress.TechBlockId} is unlocked but not found in tech blocks config!");
                    continue;
                }

                if (techBlock.GrantedTraits == null || techBlock.GrantedTraits.Count == 0)
                {
                    //serverApi.Logger.Debug($"[GuildTraitManager]   Tech '{techBlock.Text}' has no granted traits");
                    continue;
                }

                //serverApi.Logger.Notification($"[GuildTraitManager]   Tech '{techBlock.Text}' grants traits: [{string.Join(", ", techBlock.GrantedTraits)}]");
                foreach (var trait in techBlock.GrantedTraits)
                {
                    if (!string.IsNullOrWhiteSpace(trait))
                    {
                        traits.Add(trait);
                        //serverApi.Logger.Debug($"[GuildTraitManager]     Added trait '{trait}' to required list");
                    }
                }
            }

            var guildManager = modSystem.GetGuildManager();
            var configManager = guildManager?.GetConfigManager();
            string? guildRankClass = configManager?.GetConfig()?.GetGuildRankClass(guild.Points);

            if (guildRankClass != null)
            {
                traits.Add($"guild-rank-{guildRankClass}");
            }

            //serverApi.Logger.Notification($"[GuildTraitManager] Guild '{guild.Name}' has {unlockedCount} unlocked techs, requiring {traits.Count} total traits");
            return traits;
        }

        /// <summary>
        /// Get all guild-granted traits currently on a player
        /// </summary>
        private HashSet<string> GetPlayerGuildTraits(IServerPlayer player)
        {
            //serverApi.Logger.Debug($"[GuildTraitManager] Checking current guild traits for player {player.PlayerName}");
            var guildTraits = new HashSet<string>();

            if (characterSystem == null)
            {
                //serverApi.Logger.Error("[GuildTraitManager] CharacterSystem is null, cannot get player traits");
                return guildTraits;
            }

            // Filter for guild-granted traits by checking against known tech-granted traits
            var techBlocks = modSystem.TechBlocks;
            //serverApi.Logger.Debug($"[GuildTraitManager] Checking against {techBlocks.Count} tech blocks for possible guild traits");

            foreach (var tech in techBlocks)
            {
                if (tech.GrantedTraits == null || tech.GrantedTraits.Count == 0)
                    continue;

                foreach (var trait in tech.GrantedTraits)
                {
                    bool hasTrait = characterSystem.HasTrait(player, trait);
                    //serverApi.Logger.Debug($"[GuildTraitManager]   Checking trait '{trait}' from tech '{tech.Text}': {(hasTrait ? "HAS" : "MISSING")}");

                    if (hasTrait)
                    {
                        guildTraits.Add(trait);
                    }
                }
            }

            var guildManager = modSystem.GetGuildManager();
            var configManager = guildManager?.GetConfigManager();
            var classThresholds = configManager?.GetConfig()?.ClassThresholds?.Select(x => x.Key);

            if (classThresholds != null && classThresholds.Any())
            {
                foreach (var rankClass in classThresholds)
                {
                    string rankTrait = $"guild-rank-{rankClass}";
                    if (characterSystem.HasTrait(player, rankTrait))
                    {
                        guildTraits.Add(rankTrait);
                    }
                }
            }

            //serverApi.Logger.Debug($"[GuildTraitManager] Player {player.PlayerName} currently has {guildTraits.Count} guild-granted traits");
            return guildTraits;
        }

        /// <summary>
        /// Grant a trait to a player
        /// </summary>
        private void GrantTrait(IServerPlayer player, string traitCode)
        {
            //serverApi.Logger.Debug($"[GuildTraitManager] GrantTrait called for player {player?.PlayerName}, trait '{traitCode}'");

            if (characterSystem == null)
            {
                //serverApi.Logger.Error("[GuildTraitManager] CharacterSystem is null, cannot grant trait");
                return;
            }

            if (string.IsNullOrWhiteSpace(traitCode))
            {
                //serverApi.Logger.Error("[GuildTraitManager] TraitCode is null or empty, cannot grant trait");
                return;
            }

            try
            {
                // Check if player already has this trait
                bool alreadyHasTrait = characterSystem.HasTrait(player, traitCode);
                //serverApi.Logger.Debug($"[GuildTraitManager] Player {player.PlayerName} already has trait '{traitCode}': {alreadyHasTrait}");

                if (!alreadyHasTrait)
                {
                    var playerEntity = player.Entity;
                    if (playerEntity == null)
                    {
                        //serverApi.Logger.Error($"[GuildTraitManager] Player entity is null for {player.PlayerName}");
                        return;
                    }

                    // Access the player's WatchedAttributes to store the trait
                    var watchedAttributes = playerEntity.WatchedAttributes;
                    if (watchedAttributes == null)
                    {
                        //serverApi.Logger.Error($"[GuildTraitManager] WatchedAttributes is null for player {player.PlayerName}");
                        return;
                    }

                    // Get current extraTraits array (this is where HasTrait looks for extra traits)
                    var currentTraits = watchedAttributes.GetStringArray("extraTraits")?.ToList() ?? new List<string>();
                    //serverApi.Logger.Debug($"[GuildTraitManager] Player {player.PlayerName} currently has {currentTraits.Count} extraTraits: [{string.Join(", ", currentTraits)}]");

                    // Add the new trait if not already present
                    if (!currentTraits.Contains(traitCode))
                    {
                        currentTraits.Add(traitCode);
                        watchedAttributes.SetStringArray("extraTraits", currentTraits.ToArray());

                        // Mark the attribute as dirty so it syncs
                        watchedAttributes.MarkPathDirty("extraTraits");
                        playerEntity.WatchedAttributes.MarkAllDirty();

                        //serverApi.Logger.Notification($"[GuildTraitManager] ✓ Successfully granted trait '{traitCode}' to player {player.PlayerName}");
                        //serverApi.Logger.Debug($"[GuildTraitManager] Updated extraTraits array now has {currentTraits.Count} traits: [{string.Join(", ", currentTraits)}]");
                    }
                    else
                    {
                        //serverApi.Logger.Debug($"[GuildTraitManager] Trait '{traitCode}' already in player's extraTraits array");
                    }

                    // Verify it was added
                    bool verifyHasTrait = characterSystem.HasTrait(player, traitCode);
                    //serverApi.Logger.Notification($"[GuildTraitManager] Verification: Player has trait '{traitCode}': {verifyHasTrait}");

                    if (!verifyHasTrait)
                    {
                        //serverApi.Logger.Warning($"[GuildTraitManager] Trait was added but HasTrait still returns false!");
                    }
                }
                else
                {
                    //serverApi.Logger.Debug($"[GuildTraitManager] Skipping grant - player {player.PlayerName} already has trait '{traitCode}'");
                }
            }
            catch (Exception ex)
            {
                serverApi.Logger.Error($"[GuildTraitManager] Failed to grant trait '{traitCode}' to player {player.PlayerName}: {ex.Message}");
                serverApi.Logger.Error($"[GuildTraitManager] Stack trace: {ex.StackTrace}");
            }
        }

        /// <summary>
        /// Revoke a trait from a player
        /// </summary>
        private void RevokeTrait(IServerPlayer player, string traitCode)
        {
            if (characterSystem == null || string.IsNullOrWhiteSpace(traitCode))
                return;

            try
            {
                // Check if player has this trait before trying to remove it
                if (characterSystem.HasTrait(player, traitCode))
                {
                    var playerEntity = player.Entity;
                    if (playerEntity == null)
                    {
                        //serverApi.Logger.Error($"[GuildTraitManager] Player entity is null for {player.PlayerName}");
                        return;
                    }

                    // Access the player's WatchedAttributes
                    var watchedAttributes = playerEntity.WatchedAttributes;
                    if (watchedAttributes == null)
                    {
                        //serverApi.Logger.Error($"[GuildTraitManager] WatchedAttributes is null for player {player.PlayerName}");
                        return;
                    }

                    // Get the extraTraits array
                    var currentTraits = watchedAttributes.GetStringArray("extraTraits")?.ToList();
                    if (currentTraits == null || currentTraits.Count == 0)
                    {
                        //serverApi.Logger.Warning($"[GuildTraitManager] No extraTraits array found for player {player.PlayerName}");
                        return;
                    }

                    // Remove the trait
                    if (currentTraits.Remove(traitCode))
                    {
                        watchedAttributes.SetStringArray("extraTraits", currentTraits.ToArray());

                        // Mark the attribute as dirty so it syncs
                        watchedAttributes.MarkPathDirty("extraTraits");
                        playerEntity.WatchedAttributes.MarkAllDirty();

                        //serverApi.Logger.Debug($"[GuildTraitManager] Revoked trait '{traitCode}' from player {player.PlayerName}");
                    }
                    else
                    {
                        //serverApi.Logger.Warning($"[GuildTraitManager] Trait '{traitCode}' not found in player {player.PlayerName}'s extraTraits array");
                    }
                }
            }
            catch (Exception ex)
            {
                //serverApi.Logger.Error($"[GuildTraitManager] Failed to revoke trait '{traitCode}' from player {player.PlayerName}: {ex.Message}");
            }
        }

        /// <summary>
        /// Remove all guild-granted traits from a player (when they leave guild)
        /// </summary>
        private void RemoveAllGuildTraits(IServerPlayer player)
        {
            var currentGuildTraits = GetPlayerGuildTraits(player);
            foreach (var trait in currentGuildTraits)
            {
                RevokeTrait(player, trait);
            }
        }
    }
}
