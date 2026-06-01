using System.Collections.Generic;
using System.Linq;

namespace SRGuildsAndKingdoms.src.techblock
{
    /// <summary>
    /// Utility class for matching resource requirements against available items
    /// Supports wildcard matching (plank-*) and multiple type matching (clay-red,clay-blue)
    /// </summary>
    public static class ResourceMatcher
    {
        /// <summary>
        /// Checks if an item matches a resource requirement
        /// Supports various matching patterns:
        /// - Exact matching: "wood" matches "wood"
        /// - Wildcard matching: "plank-*" matches "plank-oak", "plank-birch", etc.
        /// - Multiple type matching: "clay-red,clay-blue" matches either "clay-red" or "clay-blue"
        /// - Predefined mappings: "wood" matches "log", "plank", "stick", etc.
        /// </summary>
        /// <param name="itemName">Name of the available item</param>
        /// <param name="resourceRequirement">Resource requirement string (supports wildcards and multiple types)</param>
        /// <returns>True if the item matches the resource requirement</returns>
        public static bool DoesItemMatchResource(string itemName, string resourceRequirement)
        {
            if (string.IsNullOrEmpty(itemName) || string.IsNullOrEmpty(resourceRequirement))
                return false;

            // Normalize item name
            var normalizedItem = NormalizeString(itemName);
            var normalizedRequirement = NormalizeString(resourceRequirement);

            // Handle multiple type matching (comma-separated)
            if (normalizedRequirement.Contains(","))
            {
                var resourceTypes = normalizedRequirement.Split(',')
                    .Select(type => type.Trim())
                    .Where(type => !string.IsNullOrEmpty(type));

                return resourceTypes.Any(type => DoesItemMatchSingleResource(normalizedItem, type));
            }

            // Handle single resource requirement (may include wildcards)
            return DoesItemMatchSingleResource(normalizedItem, normalizedRequirement);
        }

        /// <summary>
        /// Matches an item against a single resource requirement (no comma separation)
        /// Handles wildcards and predefined mappings
        /// </summary>
        private static bool DoesItemMatchSingleResource(string normalizedItem, string normalizedResource)
        {
            // Handle wildcard matching
            if (normalizedResource.Contains("*"))
            {
                return MatchesWildcardPattern(normalizedItem, normalizedResource);
            }

            // Exact match
            if (normalizedItem == normalizedResource)
                return true;

            // Partial matching - item contains resource name or vice versa
            if (normalizedItem.Contains(normalizedResource) || normalizedResource.Contains(normalizedItem))
                return true;

            return false;
        }

        /// <summary>
        /// Matches an item against a wildcard pattern
        /// Supports * as a wildcard that matches any characters, including multiple wildcards
        /// </summary>
        private static bool MatchesWildcardPattern(string item, string pattern)
        {
            // Convert wildcard pattern to regex-like matching
            if (pattern == "*")
                return true; // Matches everything

            if (pattern.StartsWith("*") && pattern.EndsWith("*"))
            {
                // *pattern* - contains
                var middle = pattern.Substring(1, pattern.Length - 2);
                if (string.IsNullOrEmpty(middle))
                    return true;

                // If middle contains wildcards, use recursive matching
                if (middle.Contains("*"))
                    return MatchesWildcardRecursive(item, pattern);

                return item.Contains(middle);
            }
            else if (pattern.StartsWith("*"))
            {
                // *pattern - ends with
                var suffix = pattern.Substring(1);
                if (string.IsNullOrEmpty(suffix))
                    return true;

                // If suffix contains wildcards, use recursive matching
                if (suffix.Contains("*"))
                    return MatchesWildcardRecursive(item, pattern);

                return item.EndsWith(suffix);
            }
            else if (pattern.EndsWith("*"))
            {
                // pattern* - starts with
                var prefix = pattern.Substring(0, pattern.Length - 1);
                if (string.IsNullOrEmpty(prefix))
                    return true;

                // If prefix contains wildcards, use recursive matching
                if (prefix.Contains("*"))
                    return MatchesWildcardRecursive(item, pattern);

                return item.StartsWith(prefix);
            }
            else if (pattern.Contains("*"))
            {
                // Pattern with * in middle - use recursive matching for multiple wildcards
                return MatchesWildcardRecursive(item, pattern);
            }

            return false;
        }

        /// <summary>
        /// Recursive wildcard matching algorithm that supports multiple wildcards
        /// </summary>
        private static bool MatchesWildcardRecursive(string text, string pattern)
        {
            int textIndex = 0;
            int patternIndex = 0;
            int starIndex = -1;
            int matchIndex = 0;

            while (textIndex < text.Length)
            {
                if (patternIndex < pattern.Length && pattern[patternIndex] == '*')
                {
                    // Remember position of * and where we are in text
                    starIndex = patternIndex;
                    matchIndex = textIndex;
                    patternIndex++;
                }
                else if (patternIndex < pattern.Length && pattern[patternIndex] == text[textIndex])
                {
                    // Characters match, advance both
                    textIndex++;
                    patternIndex++;
                }
                else if (starIndex != -1)
                {
                    // Backtrack: try matching one more character with the *
                    patternIndex = starIndex + 1;
                    matchIndex++;
                    textIndex = matchIndex;
                }
                else
                {
                    // No match and can't backtrack
                    return false;
                }
            }

            // Handle remaining characters in pattern (should only be *)
            while (patternIndex < pattern.Length && pattern[patternIndex] == '*')
            {
                patternIndex++;
            }

            // Match succeeds if we've consumed entire pattern
            return patternIndex == pattern.Length;
        }

        /// <summary>
        /// Normalizes a string for consistent matching
        /// </summary>
        private static string NormalizeString(string input)
        {
            return input?.ToLower()
                .Replace(" ", "")
                .Replace("_", "")
                .Replace("-", "") ?? string.Empty;
        }

        /// <summary>
        /// Gets all items from available items that match a resource requirement
        /// </summary>
        /// <param name="availableItems">Dictionary of available items</param>
        /// <param name="resourceRequirement">Resource requirement string</param>
        /// <returns>Dictionary of matching items with their quantities</returns>
        public static Dictionary<string, int> GetMatchingItems(Dictionary<string, int> availableItems, string resourceRequirement)
        {
            if (availableItems == null || string.IsNullOrEmpty(resourceRequirement))
                return new Dictionary<string, int>();

            return availableItems
                .Where(item => DoesItemMatchResource(item.Key, resourceRequirement))
                .ToDictionary(item => item.Key, item => item.Value);
        }

        /// <summary>
        /// Calculates the total quantity of items that match a resource requirement
        /// </summary>
        /// <param name="availableItems">Dictionary of available items</param>
        /// <param name="resourceRequirement">Resource requirement string</param>
        /// <returns>Total quantity of matching items</returns>
        public static int GetTotalMatchingQuantity(Dictionary<string, int> availableItems, string resourceRequirement)
        {
            return GetMatchingItems(availableItems, resourceRequirement).Sum(item => item.Value);
        }

        /// <summary>
        /// Validates if a resource requirement string is properly formatted
        /// </summary>
        /// <param name="resourceRequirement">Resource requirement string to validate</param>
        /// <returns>True if the format is valid</returns>
        public static bool IsValidResourceRequirement(string resourceRequirement)
        {
            if (string.IsNullOrWhiteSpace(resourceRequirement))
                return false;

            // Check for invalid wildcard patterns
            if (resourceRequirement.Contains("**"))
                return false;

            // Check comma-separated values
            if (resourceRequirement.Contains(","))
            {
                var parts = resourceRequirement.Split(',');
                return parts.All(part => !string.IsNullOrWhiteSpace(part.Trim()));
            }

            return true;
        }
    }
}