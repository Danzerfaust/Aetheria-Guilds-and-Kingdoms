using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SOAGuildsAndKingdoms.src.quests
{
    /// <summary>
    /// Represents a guild quest definition from the guild_quests table
    /// </summary>
    public class Quest
    {
        /// <summary>
        /// Database primary key
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Recurrence type determining period-locking behavior
        /// </summary>
        [JsonConverter(typeof(JsonStringEnumConverter<QuestRecurrenceType>))]
        public QuestRecurrenceType RecurrenceType { get; set; }

        /// <summary>
        /// Display title for the quest
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Detailed description shown in the quest UI
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// List of objectives that must be completed
        /// </summary>
        public List<QuestObjective> Objectives { get; set; } = [];

        /// <summary>
        /// Rewards granted upon quest completion
        /// </summary>
        public List<QuestReward> Rewards { get; set; } = [];

        /// <summary>
        /// Date/time when quest becomes available (format depends on UsesIngameTime)
        /// </summary>
        public string StartsAt { get; set; } = string.Empty;

        /// <summary>
        /// Date/time when quest expires (format depends on UsesIngameTime)
        /// </summary>
        public string ExpiresAt { get; set; } = string.Empty;

        /// <summary>
        /// Whether dates use in-game time (true) or real-world time (false)
        /// </summary>
        public bool UsesIngameTime { get; set; }

        /// <summary>
        /// Whether the quest repeats
        /// </summary>
        public bool Repeat { get; set; }

        /// <summary>
        /// Rank the quest is for (weeklies)
        /// </summary>
        public string Rank { get; set; } = "D";

        /// <summary>
        /// Unix timestamp when the quest was created
        /// </summary>
        public long CreatedAt { get; set; }

        /// <summary>
        /// Generates the period key for this quest based on its recurrence type and dates
        /// </summary>
        public string GeneratePeriodKey()
        {
            return QuestPeriodKeyGenerator.GeneratePeriodKey(RecurrenceType, StartsAt, ExpiresAt, UsesIngameTime);
        }

        /// <summary>
        /// Serializes objectives to JSON for database storage
        /// </summary>
        public string SerializeRequirements()
        {
            var requirements = new QuestRequirementsJson { Objectives = Objectives };
            return JsonSerializer.Serialize(requirements, QuestJsonContext.Default.QuestRequirementsJson);
        }

        /// <summary>
        /// Deserializes objectives from JSON database storage
        /// </summary>
        public static List<QuestObjective> DeserializeRequirements(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return [];

            try
            {
                var requirements = JsonSerializer.Deserialize(json, QuestJsonContext.Default.QuestRequirementsJson);
                return requirements?.Objectives ?? [];
            }
            catch
            {
                return [];
            }
        }

        /// <summary>
        /// Serializes rewards to JSON for database storage
        /// </summary>
        public string SerializeRewards()
        {
            var rewardsWrapper = new QuestRewardsJson { Rewards = Rewards };
            return JsonSerializer.Serialize(rewardsWrapper, QuestJsonContext.Default.QuestRewardsJson);
        }

        /// <summary>
        /// Deserializes rewards from JSON database storage
        /// </summary>
        public static List<QuestReward> DeserializeRewards(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return [];

            try
            {
                var rewardsWrapper = JsonSerializer.Deserialize(json, QuestJsonContext.Default.QuestRewardsJson);
                return rewardsWrapper?.Rewards ?? [];
            }
            catch
            {
                return [];
            }
        }
    }

    /// <summary>
    /// Quest recurrence types that determine period-locking behavior
    /// </summary>
    public enum QuestRecurrenceType
    {
        /// <summary>
        /// Daily quest - period key based on specific day
        /// </summary>
        Daily,

        /// <summary>
        /// Weekly quest - period key based on week's Sunday date
        /// </summary>
        Weekly,

        /// <summary>
        /// Monthly quest - period key based on year and month
        /// </summary>
        Monthly,

        /// <summary>
        /// Seasonal/event quest - period key based on start and end dates
        /// </summary>
        Seasonal
    }

    /// <summary>
    /// JSON wrapper for quest requirements (objectives array)
    /// </summary>
    public class QuestRequirementsJson
    {
        [JsonPropertyName("objectives")]
        public List<QuestObjective> Objectives { get; set; } = [];
    }

    /// <summary>
    /// JSON wrapper for quest rewards array
    /// </summary>
    public class QuestRewardsJson
    {
        [JsonPropertyName("rewards")]
        public List<QuestReward> Rewards { get; set; } = [];
    }

    [JsonSourceGenerationOptions(
        PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonSerializable(typeof(QuestRequirementsJson))]
    [JsonSerializable(typeof(QuestRewardsJson))]
    [JsonSerializable(typeof(QuestProgressJson))]
    [JsonSerializable(typeof(Quest))]
    [JsonSerializable(typeof(List<QuestObjective>))]
    [JsonSerializable(typeof(List<QuestReward>))]
    public partial class QuestJsonContext : JsonSerializerContext
    {
    }
}
