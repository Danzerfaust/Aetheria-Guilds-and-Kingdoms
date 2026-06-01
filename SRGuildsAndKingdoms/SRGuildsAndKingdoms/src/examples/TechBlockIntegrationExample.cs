using SRGuildsAndKingdoms.src.techblock;
using System;
using System.Collections.Generic;
using Vintagestory.API.Server;

namespace SRGuildsAndKingdoms.src.examples
{
    /// <summary>
    /// Example integration of tech blocks system into the mod
    /// Shows how to initialize and use the tech system in your ModSystem
    /// </summary>
    public class TechBlockIntegrationExample
    {
        private ICoreServerAPI serverApi;
        private TechBlocksConfig techConfig;
        private GuildTechManager techManager;

        /// <summary>
        /// Example: Initialize tech system on server start
        /// Call this from your ModSystem.StartServerSide method
        /// </summary>
        public void InitializeTechSystem(ICoreServerAPI api)
        {
            serverApi = api;

            try
            {
                // Load tech blocks configuration
                api.Logger.Notification("Loading tech blocks configuration...");
                techConfig = TechBlocksConfig.LoadFromFile(api);

                // Validate the configuration
                if (!techConfig.Validate(api))
                {
                    api.Logger.Error("Tech blocks configuration validation failed!");
                    return;
                }

                // Initialize the tech manager
                techManager = new GuildTechManager(api);

                api.Logger.Notification($"Tech system initialized with {techConfig.TechBlocks.Count} technologies");
            }
            catch (Exception ex)
            {
                api.Logger.Error($"Failed to initialize tech system: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Example: Handle player attempting to contribute resources to a tech
        /// </summary>
        public void OnPlayerContributeToTech(string guildId, string playerName, int techBlockId, Dictionary<string, int> playerInventory)
        {
            try
            {
                // Get the tech block
                var techBlock = techConfig.TechBlocks.Find(t => t.Id == techBlockId);
                if (techBlock == null)
                {
                    serverApi.Logger.Warning($"Tech block {techBlockId} not found");
                    return;
                }

                // Get guild's progress
                var guildData = techManager.GetGuildTechData(guildId);
                var progress = guildData.GetOrCreateProgress(techBlockId);

                // Check if already unlocked
                if (progress.IsUnlocked)
                {
                    serverApi.Logger.Notification($"Tech '{techBlock.Text}' is already unlocked for guild {guildId}");
                    return;
                }

                // Check prerequisites
                if (!techManager.HasPrerequisites(guildId, techBlock, techConfig.TechBlocks))
                {
                    serverApi.Logger.Notification($"Prerequisites not met for tech '{techBlock.Text}'");
                    return;
                }

                // Check if player has required resources
                if (!techBlock.CanResearchWithItems(playerInventory, progress))
                {
                    serverApi.Logger.Notification($"Player {playerName} doesn't have required resources");
                    return;
                }

                // Calculate resources to contribute
                var contributed = new Dictionary<string, int>();
                bool allRequirementsMet = true;

                foreach (var group in techBlock.ResourceGroups)
                {
                    var groupName = group.Name;
                    var requiredAmount = group.AmountRequired;
                    var alreadySubmitted = progress.GetResourceGroupSubmitted(groupName);
                    var stillNeeded = requiredAmount - alreadySubmitted;

                    if (stillNeeded > 0)
                    {
                        var available = group.GetTotalMatchingQuantity(playerInventory);
                        var toContribute = Math.Min(available, stillNeeded);

                        if (toContribute > 0)
                        {
                            contributed[groupName] = toContribute;
                            // Here you would remove items from player inventory
                            // RemoveItemsFromPlayer(playerName, groupName, toContribute);
                        }

                        if (toContribute < stillNeeded)
                        {
                            allRequirementsMet = false;
                        }
                    }
                }

                // Submit the contributed resources
                if (contributed.Count > 0)
                {
                    techManager.SubmitResources(guildId, techBlockId, contributed);
                    serverApi.Logger.Notification($"Player {playerName} contributed resources to '{techBlock.Text}'");
                }

                // Check if tech is now complete
                if (allRequirementsMet)
                {
                    techManager.UnlockTech(guildId, techBlockId);
                    serverApi.Logger.Event($"Guild {guildId} unlocked technology: {techBlock.Text}");

                    // Notify all guild members
                    NotifyGuildMembers(guildId, $"Technology '{techBlock.Text}' has been unlocked!");
                }
            }
            catch (Exception ex)
            {
                serverApi.Logger.Error($"Error contributing to tech: {ex.Message}");
            }
        }

        /// <summary>
        /// Example: Get available techs for a guild (for UI display)
        /// </summary>
        public List<TechBlockInfo> GetAvailableTechsForGuild(string guildId)
        {
            var result = new List<TechBlockInfo>();
            var guildData = techManager.GetGuildTechData(guildId);

            foreach (var techBlock in techConfig.TechBlocks)
            {
                var progress = guildData.GetOrCreateProgress(techBlock.Id);
                var hasPrereqs = techManager.HasPrerequisites(guildId, techBlock, techConfig.TechBlocks);

                result.Add(new TechBlockInfo
                {
                    TechBlock = techBlock,
                    IsUnlocked = progress.IsUnlocked,
                    IsAvailable = hasPrereqs && !progress.IsUnlocked,
                    Progress = progress,
                    ProgressPercentage = CalculateProgressPercentage(techBlock, progress)
                });
            }

            return result;
        }

        /// <summary>
        /// Calculate completion percentage for a tech block
        /// </summary>
        private double CalculateProgressPercentage(TechBlock techBlock, GuildTechProgress progress)
        {
            if (progress.IsUnlocked)
                return 100.0;

            if (techBlock.ResourceGroups == null || techBlock.ResourceGroups.Count == 0)
                return 0.0;

            double totalRequired = 0;
            double totalSubmitted = 0;

            foreach (var group in techBlock.ResourceGroups)
            {
                totalRequired += group.AmountRequired;
                totalSubmitted += progress.GetResourceGroupSubmitted(group.Name);
            }

            return totalRequired > 0 ? (totalSubmitted / totalRequired) * 100.0 : 0.0;
        }

        /// <summary>
        /// Example: Reload tech blocks configuration
        /// Useful for hot-reloading config changes without restarting server
        /// </summary>
        public void ReloadTechConfig()
        {
            try
            {
                techConfig = TechBlocksConfig.LoadFromFile(serverApi);

                if (techConfig.Validate(serverApi))
                {
                    serverApi.Logger.Notification($"Reloaded {techConfig.TechBlocks.Count} tech blocks");
                }
                else
                {
                    serverApi.Logger.Error("Failed to validate reloaded tech config");
                }
            }
            catch (Exception ex)
            {
                serverApi.Logger.Error($"Failed to reload tech config: {ex.Message}");
            }
        }

        /// <summary>
        /// Placeholder for notifying guild members
        /// </summary>
        private void NotifyGuildMembers(string guildId, string message)
        {
            // Implementation depends on your guild system
            // Example: serverApi.SendMessageToGroup(guildId, message);
        }
    }

    /// <summary>
    /// Helper class for displaying tech information in UI
    /// </summary>
    public class TechBlockInfo
    {
        public TechBlock TechBlock { get; set; }
        public bool IsUnlocked { get; set; }
        public bool IsAvailable { get; set; }
        public GuildTechProgress Progress { get; set; }
        public double ProgressPercentage { get; set; }
    }
}
