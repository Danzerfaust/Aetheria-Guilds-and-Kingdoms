using SOAGuildsAndKingdoms.src.database;
using SOAGuildsAndKingdoms.src.guilds;
using SOAGuildsAndKingdoms.src.techblock;
using System;
using System.Collections.Generic;
using Vintagestory.API.Server;

namespace SOAGuildsAndKingdoms.src.examples
{
    /// <summary>
    /// Example usage of the SQLite-based guild tech system
    /// </summary>
    public class GuildTechUsageExamples
    {
        private ICoreServerAPI sapi;
        private GuildManager guildManager;
        private GuildTechManager techManager;
        private GuildRepository repository;

        public void Initialize(ICoreServerAPI sapi, GuildManager guildManager, GuildRepository repository)
        {
            this.sapi = sapi;
            this.guildManager = guildManager;
            this.repository = repository;
            this.techManager = new GuildTechManager(
                sapi,
                guildName => guildManager.GetGuild(guildName),
                guildName => repository.MarkDirty(guildName)
            );
        }

        /// <summary>
        /// Example 1: Accessing and modifying tech progress via Guild object
        /// Tech data is stored in the Guild object and persisted to SQLite automatically
        /// </summary>
        public void Example1_AccessTechViaGuild()
        {
            string guildName = "The Iron Legion";

            // Get guild from GuildManager (loads from SQLite if not cached)
            var guild = guildManager.GetGuild(guildName);
            if (guild == null)
            {
                sapi.Logger.Warning($"Guild '{guildName}' not found");
                return;
            }

            // Get or create tech progress for tech block ID 1
            var progress = guild.GetOrCreateTechProgress(1);
            progress.IsUnlocked = true;
            progress.UnlockedTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            // Mark guild as dirty - will be saved to SQLite on next world save (~5 minutes)
            repository.MarkDirty(guildName);

            // Verify tech is unlocked
            bool isUnlocked = guild.TechProgress.TryGetValue(1, out var tech) && tech.IsUnlocked;
            sapi.Logger.Notification($"Tech 1 unlocked: {isUnlocked}");
        }

        /// <summary>
        /// Example 2: Using GuildTechManager with automatic persistence
        /// GuildTechManager now works with Guild objects and marks them dirty automatically
        /// </summary>
        public void Example2_UsingTechManager()
        {
            string guildName = "The Copper Alliance";

            // Unlock a tech using GuildTechManager
            // This automatically marks the guild as dirty for database persistence
            techManager.UnlockTech(guildName, 1);

            // Submit resources for a tech
            var resources = new Dictionary<string, int>
            {
                { "plank-*", 50 },
                { "clay-blue", 20 }
            };
            techManager.SubmitResources(guildName, 2, resources);

            // Get unlocked techs
            var unlockedTechs = techManager.GetUnlockedTechs(guildName);
            sapi.Logger.Notification($"Guild has {unlockedTechs.Count} unlocked techs");

            // Changes are automatically saved to SQLite on next world save event
        }

        /// <summary>
        /// Example 3: JSON serialization for network transmission
        /// </summary>
        public void Example3_NetworkTransmission()
        {
            string guildId = "guild_003";
            var guildData = techManager.GetGuildTechData(guildId);

            // Serialize to JSON string (for debugging or API)
            string json = guildData.ToJson();

            // Deserialize from JSON
            var deserializedData = GuildTechData.FromJson(json);

            // Or use bytes for efficient network transmission
            byte[] bytes = guildData.ToBytes();
            var fromBytes = GuildTechData.FromBytes(bytes);
        }

        /// <summary>
        /// Example 4: Checking tech requirements and progress via Guild object
        /// </summary>
        public void Example4_CheckingRequirements(List<TechBlock> allTechBlocks)
        {
            string guildName = "The Bronze Collective";
            var guild = guildManager.GetGuild(guildName);
            if (guild == null) return;

            // Get a specific tech block
            var techBlock = allTechBlocks.Find(t => t.Id == 5);

            // Check if guild has prerequisites
            bool hasPrereqs = techManager.HasPrerequisites(guildName, techBlock, allTechBlocks);

            // Get available items from guild inventory (example)
            var availableItems = new Dictionary<string, int>
            {
                { "plank-oak", 100 },
                { "plank-birch", 50 },
                { "clay-blue", 30 }
            };

            // Get progress for this tech from Guild object
            var progress = guild.GetOrCreateTechProgress(techBlock.Id);

            // Check if guild can research with available items
            bool canResearch = techBlock.CanResearchWithItems(availableItems, progress);

            if (canResearch && hasPrereqs)
            {
                sapi.Logger.Notification("Guild can research this tech!");
            }
        }

        /// <summary>
        /// Example 5: Complete research workflow with SQLite persistence
        /// </summary>
        public void Example5_CompleteResearchWorkflow(TechBlock tech, Dictionary<string, int> playerInventory)
        {
            string guildName = "Crescent Coven";
            var guild = guildManager.GetGuild(guildName);
            if (guild == null) return;

            var progress = guild.GetOrCreateTechProgress(tech.Id);

            // Check if already unlocked
            if (progress.IsUnlocked)
            {
                sapi.Logger.Notification("Tech already unlocked!");
                return;
            }

            // Check requirements
            foreach (var group in tech.ResourceGroups)
            {
                var groupName = group.Name;
                var requiredAmount = group.AmountRequired;
                var alreadySubmitted = progress.GetResourceGroupSubmitted(groupName);
                var stillNeeded = requiredAmount - alreadySubmitted;

                if (stillNeeded <= 0)
                    continue; // This requirement already met

                // Check if player has matching items
                var available = group.GetTotalMatchingQuantity(playerInventory);

                if (available >= stillNeeded)
                {
                    // Submit the required amount
                    var toSubmit = new Dictionary<string, int> { { groupName, stillNeeded } };
                    techManager.SubmitResources(guildName, tech.Id, toSubmit);

                    // Remove items from player inventory (you'd implement this)
                    // RemoveItemsFromInventory(playerInventory, groupName, stillNeeded);
                }
                else
                {
                    sapi.Logger.Notification($"Need {stillNeeded} more of {groupName}");
                    return;
                }
            }

            // All requirements met - unlock the tech!
            // This marks the guild as dirty and will be saved to SQLite automatically
            techManager.UnlockTech(guildName, tech.Id);
            sapi.Logger.Notification($"Tech {tech.Text} unlocked and will be saved to database!");
        }

        /// <summary>
        /// Example 6: Automatic DB persistence with MarkDirty
        /// </summary>
        public void Example6_AutomaticPersistence()
        {
            // No manual save needed! Guild changes are tracked via MarkGuildDirty()
            // and committed to SQLite automatically on world save events

            string guildName = "The Titanium Order";
            var guild = guildManager.GetGuild(guildName);
            if (guild == null) return;

            // Make changes to guild
            guild.Points += 100;
            var tech = guild.GetOrCreateTechProgress(10);
            tech.IsUnlocked = true;

            // Mark as dirty - will be saved automatically
            repository.MarkDirty(guildName);

            // Changes will be committed to SQLite within ~5 minutes (next world save)
            // Or immediately on server shutdown
            sapi.Logger.Notification("Guild marked dirty - changes will be saved automatically");
        }

        /// <summary>
        /// Example 7: Bulk operations with SQLite-backed guilds
        /// </summary>
        public void Example7_BulkOperations()
        {
            // Get all guilds from repository (loaded from SQLite)
            var allGuilds = repository.GetAllGuilds();

            foreach (var guild in allGuilds)
            {
                try
                {
                    // Perform some operation on each guild
                    // Example: Award bonus points to all guilds
                    guild.Points += 10;

                    // Mark as dirty to save changes
                    repository.MarkDirty(guild.Name);

                    sapi.Logger.Notification($"Processed guild {guild.Name}");
                }
                catch (Exception ex)
                {
                    sapi.Logger.Error($"Failed to process guild {guild.Name}: {ex.Message}");
                }
            }

            // All changes will be saved to SQLite on next world save
            sapi.Logger.Notification($"Processed {allGuilds.Count} guilds - changes pending save");
        }
    }
}
