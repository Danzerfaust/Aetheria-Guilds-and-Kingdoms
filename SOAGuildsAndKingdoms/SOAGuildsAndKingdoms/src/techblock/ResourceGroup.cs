using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace SOAGuildsAndKingdoms.src.techblock
{
    /// <summary>
    /// Represents a group of resources that can collectively contribute to a single requirement
    /// For example: "Flux" group could include both "borax" and "lime" where either can contribute to the total
    /// </summary>
    public class ResourceGroup
    {
        /// <summary>
        /// Display name for the resource group (e.g., "Flux Materials", "Metal Ore", "Wood Products")
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; set; } = null!;

        /// <summary>
        /// Total amount required for this resource group
        /// </summary>
        [JsonPropertyName("amountRequired")]
        public int AmountRequired { get; set; }

        /// <summary>
        /// List of resource patterns that can contribute to this group
        /// Each pattern supports wildcards and comma-separated values like regular resource requirements
        /// Examples: ["crushed-borax", "crushed-lime"], ["ore-*"], ["clay-red,clay-blue"]
        /// </summary>
        [JsonPropertyName("resourcePatterns")]
        public List<string> ResourcePatterns { get; set; } = new List<string>();

        /// <summary>
        /// Checks if an item matches any of the resource patterns in this group
        /// </summary>
        public bool DoesItemMatch(string itemName)
        {
            if (string.IsNullOrEmpty(itemName) || ResourcePatterns == null || ResourcePatterns.Count == 0)
                return false;

            // Remove namespace prefix (everything before ':')
            var colonIndex = itemName.IndexOf(':');
            if (colonIndex >= 0 && colonIndex < itemName.Length - 1)
            {
                itemName = itemName.Substring(colonIndex + 1);
            }

            return ResourcePatterns.Any(pattern => ResourceMatcher.DoesItemMatchResource(itemName, pattern));
        }

        /// <summary>
        /// Gets all items from available items that match any pattern in this resource group
        /// </summary>
        public Dictionary<string, int> GetMatchingItems(Dictionary<string, int> availableItems)
        {
            if (availableItems == null || ResourcePatterns == null || ResourcePatterns.Count == 0)
                return new Dictionary<string, int>();

            var matchingItems = new Dictionary<string, int>();

            foreach (var item in availableItems)
            {
                if (DoesItemMatch(item.Key))
                {
                    matchingItems[item.Key] = item.Value;
                }
            }

            return matchingItems;
        }

        /// <summary>
        /// Calculates the total quantity of items that match this resource group
        /// </summary>
        public int GetTotalMatchingQuantity(Dictionary<string, int> availableItems)
        {
            return GetMatchingItems(availableItems).Sum(item => item.Value);
        }

        /// <summary>
        /// Validates that this resource group has valid configuration
        /// </summary>
        public bool Validate()
        {
            if (string.IsNullOrWhiteSpace(Name))
                return false;

            if (AmountRequired <= 0)
                return false;

            if (ResourcePatterns == null || ResourcePatterns.Count == 0)
                return false;

            // Validate each pattern
            return ResourcePatterns.All(pattern => ResourceMatcher.IsValidResourceRequirement(pattern));
        }
    }
}
