using System;
using System.Collections.Generic;
using System.Linq;
using Vintagestory.API.Common;

namespace SOAGuildsAndKingdoms.src.techblock
{
    /// <summary>
    /// Manages technology progress for all guilds
    /// Note: Tech data is now stored directly in the Guild object (Guild.TechProgress)
    /// This manager provides convenience methods for accessing tech data
    /// </summary>
    public class GuildTechManager(ICoreAPI api, System.Func<string, SOAGuildsAndKingdoms.src.guilds.Guild>? getGuildFunc = null, System.Action<string>? markDirtyAction = null)
    {
        private readonly ICoreAPI api = api ?? throw new ArgumentNullException(nameof(api));

        /// <summary>
        /// Gets tech data for a guild directly from the Guild object
        /// For client-side compatibility, creates a GuildTechData wrapper
        /// </summary>
        public GuildTechData GetGuildTechData(string guildId)
        {
            if (string.IsNullOrWhiteSpace(guildId))
                throw new ArgumentException("GuildId cannot be null or empty", nameof(guildId));

            // Try to get guild from provided function (server-side)
            if (getGuildFunc != null)
            {
                var guild = getGuildFunc(guildId);
                if (guild != null)
                {
                    return new GuildTechData
                    {
                        GuildId = guildId,
                        TechProgress = guild.TechProgress
                    };
                }
            }

            // Client-side or guild not found: return empty data
            return new GuildTechData { GuildId = guildId };
        }

        /// <summary>
        /// Updates progress for a specific tech block
        /// Automatically marks guild as dirty for database persistence
        /// </summary>
        public void UpdateTechProgress(string guildId, int techBlockId, Action<GuildTechProgress> updateAction)
        {
            if (getGuildFunc != null)
            {
                var guild = getGuildFunc(guildId);
                if (guild != null)
                {
                    var progress = guild.GetOrCreateTechProgress(techBlockId);
                    updateAction(progress);
                    markDirtyAction?.Invoke(guildId);
                    return;
                }
            }

            // Fallback for client-side or when guild not found
            var guildData = GetGuildTechData(guildId);
            var techProgress = guildData.GetOrCreateProgress(techBlockId);
            updateAction(techProgress);
        }

        /// <summary>
        /// Calculates the resource scaling percentage based on guild member count
        /// Below 5 players: 0% scaling
        /// 5+ players: +2% for each player above 4
        /// (5 players = 2%, 6 players = 4%, 7 players = 6%, etc.)
        /// </summary>
        /// <param name="memberCount">Number of members in the guild</param>
        /// <returns>Scaling factor as a decimal (e.g., 1.04 for 4% increase)</returns>
        public decimal GetResourceScaling(int memberCount)
        {
            if (memberCount < 5)
                return 1.0m;

            // +2% for each player above 4
            decimal scalingPercentage = (memberCount - 4) * 6;
            return 1.0m + (scalingPercentage / 100m);
        }

        /// <summary>
        /// Gets the scaled resource requirements for a tech based on guild size
        /// </summary>
        /// <param name="guildId">The guild name/ID</param>
        /// <param name="baseRequirements">The base resource requirements</param>
        /// <returns>Scaled resource requirements</returns>
        public Dictionary<string, int> GetScaledRequirements(string guildId, Dictionary<string, int> baseRequirements)
        {
            var guild = getGuildFunc?.Invoke(guildId);
            if (guild == null)
                return new Dictionary<string, int>(baseRequirements);

            decimal scaling = GetResourceScaling(guild.Members.Count);
            //decimal scaling = GetResourceScaling(30);
            var scaledRequirements = new Dictionary<string, int>();

            foreach (var req in baseRequirements)
            {
                scaledRequirements[req.Key] = (int)Math.Ceiling(req.Value * scaling);
            }

            return scaledRequirements;
        }

        /// <summary>
        /// Marks a technology as unlocked for a guild
        /// If guild has more than 10 members, sets up personal unlock requirements
        /// </summary>
        /// <param name="guildId">The guild name/ID</param>
        /// <param name="techBlockId">The tech block ID to unlock</param>
        /// <param name="techBlock">Optional: The tech block object for calculating personal requirements</param>
        public void UnlockTech(string guildId, int techBlockId, TechBlock techBlock = null)
        {
            var guild = getGuildFunc?.Invoke(guildId);

            UpdateTechProgress(guildId, techBlockId, progress =>
            {
                progress.IsUnlocked = true;
                progress.UnlockedTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            });

            // Check if personal unlocks are required (guild size > 10)
            if (guild != null && guild.Members.Count > 10)
            {
                guild.TechRequiresPersonalUnlock[techBlockId] = true;

                // Initialize personal unlock tracking for all current members
                if (techBlock != null)
                {
                    foreach (var memberUid in guild.Members.Keys)
                    {
                        InitializePersonalUnlock(guild, memberUid, techBlockId, techBlock);
                    }
                }

                // Mark dirty again for personal unlock data
                markDirtyAction?.Invoke(guildId);

                api.Logger.Notification($"Guild {guildId} unlocked tech {techBlockId} (requires personal unlock for {guild.Members.Count} members)");
            }
            else
            {
                if (guild != null)
                {
                    guild.TechRequiresPersonalUnlock[techBlockId] = false;
                }
                api.Logger.Notification($"Guild {guildId} unlocked tech {techBlockId}");
            }
        }

        /// <summary>
        /// Initializes personal unlock tracking for a player
        /// </summary>
        private void InitializePersonalUnlock(SOAGuildsAndKingdoms.src.guilds.Guild guild, string playerUid, int techId, TechBlock techBlock)
        {
            var playerProgress = guild.GetOrCreatePlayerProgress(playerUid);
            var personalUnlock = playerProgress.GetOrCreateUnlock(techId);
            personalUnlock.RequiresPersonalUnlock = true;
            personalUnlock.IsPersonallyUnlocked = false;
        }

        /// <summary>
        /// Contributes resources to a player's personal unlock
        /// </summary>
        /// <param name="guildId">The guild name/ID</param>
        /// <param name="playerUid">The player's unique ID</param>
        /// <param name="techId">The tech block ID</param>
        /// <param name="resourceGroupName">Name of the resource group</param>
        /// <param name="amount">Amount to contribute</param>
        /// <param name="techBlock">The tech block for checking completion</param>
        /// <returns>True if personal unlock is now complete</returns>
        public bool ContributeToPersonalUnlock(string guildId, string playerUid, int techId, string resourceGroupName, int amount, TechBlock techBlock)
        {
            var guild = getGuildFunc?.Invoke(guildId);
            if (guild == null) return false;

            var playerProgress = guild.GetOrCreatePlayerProgress(playerUid);
            var personalUnlock = playerProgress.GetOrCreateUnlock(techId);

            if (!personalUnlock.RequiresPersonalUnlock || personalUnlock.IsPersonallyUnlocked)
                return personalUnlock.IsPersonallyUnlocked;

            // Simplified: mark as complete after any contribution
            personalUnlock.IsPersonallyUnlocked = true;
            markDirtyAction?.Invoke(guildId);

            api.Logger.Notification($"Player {playerUid} completed personal unlock for tech {techId} in guild {guildId}");

            return true;
        }

        /// <summary>
        /// Sets up personal unlock requirement for a new member joining a guild
        /// Called when a player joins a guild that has techs requiring personal unlocks
        /// </summary>
        /// <param name="guildId">The guild name/ID</param>
        /// <param name="playerUid">The new member's unique ID</param>
        /// <param name="techBlocks">List of all tech blocks</param>
        public void InitializePersonalUnlocksForNewMember(string guildId, string playerUid, List<TechBlock> techBlocks)
        {
            var guild = getGuildFunc?.Invoke(guildId);
            if (guild == null) return;

            bool anyInitialized = false;

            // Find all techs that require personal unlocks
            foreach (var kvp in guild.TechRequiresPersonalUnlock)
            {
                if (kvp.Value && guild.IsTechUnlocked(kvp.Key))
                {
                    var techBlock = techBlocks.FirstOrDefault(tb => tb.Id == kvp.Key);
                    if (techBlock != null)
                    {
                        InitializePersonalUnlock(guild, playerUid, kvp.Key, techBlock);
                        anyInitialized = true;
                    }
                }
            }

            if (anyInitialized)
            {
                markDirtyAction?.Invoke(guildId);
            }
        }

        /// <summary>
        /// Submits resources toward a technology research
        /// </summary>
        public void SubmitResources(string guildId, int techBlockId, Dictionary<string, int> resources)
        {
            UpdateTechProgress(guildId, techBlockId, progress =>
            {
                foreach (var resource in resources)
                {
                    if (progress.ResourcesSubmitted.ContainsKey(resource.Key))
                    {
                        progress.ResourcesSubmitted[resource.Key] += resource.Value;
                    }
                    else
                    {
                        progress.ResourcesSubmitted[resource.Key] = resource.Value;
                    }
                }
            });
        }

        /// <summary>
        /// Checks if a guild has prerequisites unlocked for a tech
        /// </summary>
        public bool HasPrerequisites(string guildId, TechBlock tech, List<TechBlock> allTechs)
        {
            var guildData = GetGuildTechData(guildId);
            return tech.IsAvailableForGuild(guildData, allTechs);
        }

        /// <summary>
        /// Gets all unlocked tech IDs for a guild
        /// </summary>
        public List<int> GetUnlockedTechs(string guildId)
        {
            var guildData = GetGuildTechData(guildId);
            return guildData.TechProgress
                .Where(kvp => kvp.Value.IsUnlocked)
                .Select(kvp => kvp.Key)
                .ToList();
        }
    }
}
