using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using Vintagestory.API.MathTools;

namespace SRGuildsAndKingdoms.src.techblock
{
    /// <summary>
    /// Represents a technology research block in the tech progression system
    /// Contains information about position, appearance, level, and unlock requirements
    /// </summary>
    public class TechBlock
    {
        /// <summary>
        /// Unique identifier for this technology block
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Calculated position for rendering (set dynamically)
        /// </summary>
        public Vec2d Position { get; set; } = null!;

        /// <summary>
        /// Display text shown on the technology block
        /// </summary>
        public string Text { get; set; } = null!;

        /// <summary>
        /// Technology level/tier (determines positioning and prerequisites)
        /// </summary>
        public int Level { get; set; } = 1;

        /// <summary>
        /// Age/Era this technology belongs to (Stone, Bronze, Iron, etc.)
        /// </summary>
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public TechAge Age { get; set; } = TechAge.Stone;

        /// <summary>
        /// Resource groups required to unlock this technology
        /// Allows multiple resource types to contribute to the same requirement
        /// Example: A "Flux" group could include both borax and lime
        /// </summary>
        [JsonPropertyName("resourceGroups")]
        public List<ResourceGroup> ResourceGroups { get; set; } = new List<ResourceGroup>();

        /// <summary>
        /// Optional description or flavor text for this technology
        /// </summary>
        public string Description { get; set; } = "";

        /// <summary>
        /// List of technology IDs that this technology unlocks when researched
        /// Used to automatically generate connection lines in the tech tree
        /// </summary>
        public List<int> UnlocksIds { get; set; } = new List<int>();

        /// <summary>
        /// Traits granted to all guild members when this tech is unlocked
        /// These traits can be used to unlock recipes via the recipe blocker system
        /// </summary>
        [JsonPropertyName("grantedTraits")]
        public List<string> GrantedTraits { get; set; } = new List<string>();

        /// <summary>
        /// Checks if a guild can research this tech given their available items and progress
        /// </summary>
        public bool CanResearchWithItems(Dictionary<string, int> availableItems, GuildTechProgress progress = null)
        {
            if (progress?.IsUnlocked == true)
                return false; // Already unlocked

            if (ResourceGroups == null || ResourceGroups.Count == 0)
                return false;

            if (availableItems == null || availableItems.Count == 0)
                return false;

            // Check ResourceGroups
            foreach (var group in ResourceGroups)
            {
                var submitted = progress?.GetResourceGroupSubmitted(group.Name) ?? 0;
                var stillNeeded = group.AmountRequired - submitted;

                if (stillNeeded > 0)
                {
                    var totalAvailable = group.GetTotalMatchingQuantity(availableItems);
                    if (totalAvailable < stillNeeded)
                        return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Checks if a guild can research this tech given their available items, progress, and age restrictions
        /// </summary>
        /// <param name="availableItems">Dictionary of available item codes and quantities</param>
        /// <param name="progress">Current guild progress for this tech</param>
        /// <param name="config">Tech blocks configuration (checks if age is enabled)</param>
        /// <returns>True if the tech can be researched</returns>
        public bool CanResearchWithItems(Dictionary<string, int> availableItems, GuildTechProgress progress, TechBlocksConfig config)
        {
            // First check if the age is enabled
            if (config != null && !config.IsAgeEnabled(Age))
            {
                return false;
            }

            // Then check resource requirements
            return CanResearchWithItems(availableItems, progress);
        }

        /// <summary>
        /// Checks if prerequisites are met for a guild
        /// </summary>
        public bool IsAvailableForGuild(GuildTechData guildData, List<TechBlock> allTechBlocks)
        {
            // Find all techs that unlock this one
            var prerequisites = allTechBlocks.Where(tb => tb.UnlocksIds.Contains(Id)).ToList();

            if (!prerequisites.Any())
                return true; // No prerequisites

            // All prerequisites must be unlocked
            return prerequisites.All(prereq => guildData.IsTechUnlocked(prereq.Id));
        }

        /// <summary>
        /// Checks if prerequisites are met for a guild and if the tech age is enabled
        /// </summary>
        /// <param name="guildData">Guild's tech progress data</param>
        /// <param name="allTechBlocks">All tech blocks in the system</param>
        /// <param name="config">Tech blocks configuration (checks if age is enabled)</param>
        /// <returns>True if tech is available for research</returns>
        public bool IsAvailableForGuild(GuildTechData guildData, List<TechBlock> allTechBlocks, TechBlocksConfig config)
        {
            // First check if the age is enabled
            if (config != null && !config.IsAgeEnabled(Age))
            {
                return false;
            }

            // Then check prerequisites
            return IsAvailableForGuild(guildData, allTechBlocks);
        }

        /// <summary>
        /// Calculates the personal unlock requirements (5% of guild requirements)
        /// for use when guild size > 10
        /// </summary>
        /// <returns>Dictionary of ResourceGroup name to required amount</returns>
        public Dictionary<string, int> GetPersonalRequirements()
        {
            var personalReqs = new Dictionary<string, int>();

            if (ResourceGroups == null)
                return personalReqs;

            foreach (var group in ResourceGroups)
            {
                // Calculate 5% of guild requirement, round up to ensure at least 1
                int personalAmount = (int)System.Math.Ceiling(group.AmountRequired * 0.05);
                personalReqs[group.Name] = personalAmount;
            }

            return personalReqs;
        }

        /// <summary>
        /// Validates all resource requirements in this tech block
        /// </summary>
        public bool ValidateResourceRequirements()
        {
            // Validate resource groups
            if (ResourceGroups != null && ResourceGroups.Count > 0)
            {
                if (!ResourceGroups.All(group => group.Validate()))
                    return false;
            }

            return true;
        }
    }

    /// <summary>
    /// Represents different technological ages/eras in the tech progression system
    /// </summary>
    public enum TechAge
    {
        Stone,
        Copper,
        Bronze,
        OtherBronze,
        Iron,
        MeteoricIron,
        Steel,
        MeteoricSteel
    }
}