using SRGuildsAndKingdoms.src.network;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SRGuildsAndKingdoms.src.quests
{
    /// <summary>
    /// Represents a single objective within a quest
    /// </summary>
    public class QuestObjective
    {
        /// <summary>
        /// Unique identifier for this objective within the quest
        /// </summary>
        [JsonPropertyName("id")]
        public int Id { get; set; }

        /// <summary>
        /// Type of objective (kill, turn_in, etc.)
        /// </summary>
        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        /// <summary>
        /// Number of items/kills required to complete this objective
        /// </summary>
        [JsonPropertyName("count")]
        public int Count { get; set; }

        /// <summary>
        /// For kill objectives: wildcard patterns for accepted entity codes
        /// Example: ["game:drifter-*", "game:wolf-*"]
        /// </summary>
        [JsonPropertyName("acceptedTargets")]
        public List<string>? AcceptedTargets { get; set; }

        /// <summary>
        /// For turn_in objectives: accepted item codes with optional NBT data
        /// </summary>
        [JsonPropertyName("acceptedItems")]
        public List<QuestAcceptedItemDto>? AcceptedItems { get; set; }
    }

    /// <summary>
    /// Well-known objective types
    /// </summary>
    public static class QuestObjectiveType
    {
        /// <summary>
        /// Kill a certain number of entities
        /// </summary>
        public const string Kill = "kill";

        /// <summary>
        /// Turn in (consume) a certain number of items
        /// </summary>
        public const string TurnIn = "turn_in";
    }
}
