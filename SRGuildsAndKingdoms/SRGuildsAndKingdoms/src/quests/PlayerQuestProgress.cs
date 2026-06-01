using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SRGuildsAndKingdoms.src.quests
{
    /// <summary>
    /// Represents a player's progress on an active or completed quest
    /// </summary>
    public class PlayerQuestProgress
    {
        /// <summary>
        /// Player's unique identifier
        /// </summary>
        public string PlayerUid { get; set; } = string.Empty;

        /// <summary>
        /// Quest database ID
        /// </summary>
        public int QuestId { get; set; }

        /// <summary>
        /// Current status of this quest for the player
        /// </summary>
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public PlayerQuestStatus Status { get; set; } = PlayerQuestStatus.Active;

        /// <summary>
        /// Progress on each objective (objective ID -> current count)
        /// </summary>
        public Dictionary<int, ObjectiveProgress> ObjectiveProgress { get; set; } = [];

        /// <summary>
        /// Denormalized recurrence type for period-locking queries
        /// </summary>
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public QuestRecurrenceType RecurrenceType { get; set; }

        /// <summary>
        /// Period key for period-locking (only set when completed)
        /// </summary>
        public string? PeriodKey { get; set; }

        /// <summary>
        /// Unix timestamp when the player started this quest
        /// </summary>
        public long StartedAt { get; set; }

        /// <summary>
        /// Unix timestamp when the player completed this quest (null if not completed)
        /// </summary>
        public long? CompletedAt { get; set; }

        /// <summary>
        /// Checks if all objectives are complete (but quest not yet submitted)
        /// </summary>
        public bool AreAllObjectivesComplete(List<QuestObjective> questObjectives)
        {
            foreach (var objective in questObjectives)
            {
                if (!ObjectiveProgress.TryGetValue(objective.Id, out var progress))
                    return false;

                if (progress.Current < objective.Count)
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Gets the current progress for a specific objective
        /// </summary>
        public int GetObjectiveProgress(int objectiveId)
        {
            return ObjectiveProgress.TryGetValue(objectiveId, out var progress) ? progress.Current : 0;
        }

        /// <summary>
        /// Adds progress to a specific objective, capping at the required count
        /// </summary>
        /// <returns>The actual amount added (may be less if capped)</returns>
        public int AddObjectiveProgress(int objectiveId, int amount, int maxCount)
        {
            if (!ObjectiveProgress.TryGetValue(objectiveId, out var progress))
            {
                progress = new ObjectiveProgress();
                ObjectiveProgress[objectiveId] = progress;
            }

            int remaining = maxCount - progress.Current;
            int actualAdded = Math.Min(amount, remaining);

            if (actualAdded > 0)
            {
                progress.Current += actualAdded;
            }

            return actualAdded;
        }

        /// <summary>
        /// Serializes objective progress to JSON for database storage
        /// </summary>
        public string SerializeProgress()
        {
            var progressJson = new QuestProgressJson { Objectives = ObjectiveProgress };
            return JsonSerializer.Serialize(progressJson, QuestJsonContext.Default.QuestProgressJson);
        }

        /// <summary>
        /// Deserializes objective progress from JSON database storage
        /// </summary>
        public static Dictionary<int, ObjectiveProgress> DeserializeProgress(string? json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return [];

            try
            {
                var progressJson = JsonSerializer.Deserialize(json, QuestJsonContext.Default.QuestProgressJson);
                return progressJson?.Objectives ?? [];
            }
            catch
            {
                return [];
            }
        }
    }

    /// <summary>
    /// Progress tracking for a single objective
    /// </summary>
    public class ObjectiveProgress
    {
        /// <summary>
        /// Current count towards completion
        /// </summary>
        [JsonPropertyName("current")]
        public int Current { get; set; }
    }

    /// <summary>
    /// JSON wrapper for objective progress serialization
    /// </summary>
    public class QuestProgressJson
    {
        [JsonPropertyName("objectives")]
        public Dictionary<int, ObjectiveProgress> Objectives { get; set; } = [];
    }

    /// <summary>
    /// Status of a player's quest
    /// </summary>
    public enum PlayerQuestStatus
    {
        /// <summary>
        /// Quest is in progress
        /// </summary>
        Active,

        /// <summary>
        /// Quest has been completed and rewards claimed
        /// </summary>
        Completed
    }
}
