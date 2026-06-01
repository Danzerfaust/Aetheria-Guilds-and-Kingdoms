using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SRGuildsAndKingdoms.src.techblock
{
    /// <summary>
    /// Tracks a guild's progress on a specific technology block
    /// </summary>
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class GuildTechProgress
    {
        /// <summary>
        /// The tech block ID this progress relates to
        /// </summary>
        public int TechBlockId { get; set; }

        /// <summary>
        /// Whether this guild has unlocked this technology
        /// </summary>
        public bool IsUnlocked { get; set; } = false;

        /// <summary>
        /// Resources this guild has submitted toward unlocking this tech
        /// Key: Resource requirement string, Value: Amount submitted
        /// </summary>
        public Dictionary<string, int> ResourcesSubmitted { get; set; } = new Dictionary<string, int>();

        /// <summary>
        /// Resource groups this guild has submitted toward unlocking this tech
        /// Key: Resource group ID, Value: Amount submitted
        /// </summary>
        [JsonPropertyName("resourceGroupsSubmitted")]
        public Dictionary<string, int> ResourceGroupsSubmitted { get; set; } = new Dictionary<string, int>();

        /// <summary>
        /// Timestamp when this tech was unlocked (null if not unlocked)
        /// </summary>
        public long? UnlockedTimestamp { get; set; }

        /// <summary>
        /// Gets the amount submitted for a specific resource group
        /// </summary>
        public int GetResourceGroupSubmitted(string groupName)
        {
            return ResourceGroupsSubmitted.GetValueOrDefault(groupName, 0);
        }
    }

    /// <summary>
    /// Contains all technology progress for a single guild
    /// </summary>
    public class GuildTechData
    {
        private static readonly JsonSerializerOptions SerializeOptions = new()
        {
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.Never
        };

        private static readonly JsonSerializerOptions DeserializeOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        /// <summary>
        /// Guild identifier
        /// </summary>
        public string GuildId { get; set; } = null!;

        /// <summary>
        /// Dictionary of tech block progress (TechBlockId -> Progress)
        /// </summary>
        public Dictionary<int, GuildTechProgress> TechProgress { get; set; } = new Dictionary<int, GuildTechProgress>();

        /// <summary>
        /// Gets or creates progress for a specific tech block
        /// </summary>
        public GuildTechProgress GetOrCreateProgress(int techBlockId)
        {
            if (!TechProgress.ContainsKey(techBlockId))
            {
                TechProgress[techBlockId] = new GuildTechProgress { TechBlockId = techBlockId };
            }
            return TechProgress[techBlockId];
        }

        /// <summary>
        /// Checks if a tech is unlocked for this guild
        /// </summary>
        public bool IsTechUnlocked(int techBlockId)
        {
            return TechProgress.TryGetValue(techBlockId, out var progress) && progress.IsUnlocked;
        }

        /// <summary>
        /// Serializes the guild tech data to JSON string
        /// </summary>
        public string ToJson()
        {
            return JsonSerializer.Serialize(this, SerializeOptions);
        }

        /// <summary>
        /// Deserializes guild tech data from JSON string
        /// </summary>
        public static GuildTechData? FromJson(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return null;

            try
            {
                return JsonSerializer.Deserialize<GuildTechData>(json, DeserializeOptions);
            }
            catch (JsonException ex)
            {
                throw new InvalidOperationException($"Failed to deserialize GuildTechData: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Saves guild tech data directly to byte array for network transmission or storage
        /// </summary>
        public byte[] ToBytes()
        {
            var json = ToJson();
            return System.Text.Encoding.UTF8.GetBytes(json);
        }

        /// <summary>
        /// Loads guild tech data from byte array
        /// </summary>
        public static GuildTechData? FromBytes(byte[] data)
        {
            if (data == null || data.Length == 0)
                return null;

            var json = System.Text.Encoding.UTF8.GetString(data);
            return FromJson(json);
        }
    }
}
