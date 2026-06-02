using System.Text.Json.Serialization;

namespace SOAGuildsAndKingdoms.src.quests
{
    /// <summary>
    /// Represents a reward granted upon quest completion
    /// </summary>
    public class QuestReward
    {
        /// <summary>
        /// Item or currency code (e.g., "game:fruit-redapple", "game:grspoints")
        /// Special codes like "game:grspoints" are handled by reward-granting logic
        /// </summary>
        [JsonPropertyName("code")]
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// Optional NBT data for the item as Base64 string (null for currencies or items without NBT)
        /// </summary>
        [JsonPropertyName("nbt")]
        public string? Nbt { get; set; }

        /// <summary>
        /// Amount of items or currency to award
        /// </summary>
        [JsonPropertyName("amount")]
        public int Amount { get; set; }
    }

    /// <summary>
    /// Well-known reward codes
    /// </summary>
    public static class QuestRewardCodes
    {
        /// <summary>
        /// Guild Ranking Score points (special code, not a real item)
        /// </summary>
        public const string GrsPoints = "game:grspoints";
    }
}
