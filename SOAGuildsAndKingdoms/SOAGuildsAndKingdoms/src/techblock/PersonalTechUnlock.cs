using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SOAGuildsAndKingdoms.src.techblock
{
    /// <summary>
    /// Tracks an individual player's progress towards unlocking a specific tech
    /// that requires personal contribution (for guilds > 10 members)
    /// </summary>
    public class PersonalTechUnlock
    {
        /// <summary>
        /// The tech block ID this personal unlock is for
        /// </summary>
        [JsonPropertyName("techId")]
        public int TechId { get; set; }

        /// <summary>
        /// Whether the player has completed their personal unlock requirement
        /// </summary>
        [JsonPropertyName("isPersonallyUnlocked")]
        public bool IsPersonallyUnlocked { get; set; }

        /// <summary>
        /// Whether this tech requires a personal unlock (set at time of guild unlock)
        /// </summary>
        [JsonPropertyName("requiresPersonalUnlock")]
        public bool RequiresPersonalUnlock { get; set; }
    }

    /// <summary>
    /// Tracks all personal tech unlock progress for a single player within a guild
    /// </summary>
    public class PlayerTechProgress
    {
        /// <summary>
        /// The player's unique identifier
        /// </summary>
        [JsonPropertyName("playerUid")]
        public string PlayerUid { get; set; } = null!;

        /// <summary>
        /// Dictionary of tech ID to personal unlock status
        /// </summary>
        [JsonPropertyName("personalUnlocks")]
        public Dictionary<int, PersonalTechUnlock> PersonalUnlocks { get; set; } = new();

        /// <summary>
        /// Gets or creates a personal unlock entry for a specific tech
        /// </summary>
        public PersonalTechUnlock GetOrCreateUnlock(int techId)
        {
            if (!PersonalUnlocks.ContainsKey(techId))
            {
                PersonalUnlocks[techId] = new PersonalTechUnlock { TechId = techId };
            }
            return PersonalUnlocks[techId];
        }

        /// <summary>
        /// Checks if the player has personally unlocked a tech
        /// </summary>
        public bool IsPersonallyUnlocked(int techId)
        {
            if (!PersonalUnlocks.TryGetValue(techId, out var unlock))
                return false;

            // If personal unlock not required, consider it unlocked
            if (!unlock.RequiresPersonalUnlock)
                return true;

            return unlock.IsPersonallyUnlocked;
        }
    }
}
