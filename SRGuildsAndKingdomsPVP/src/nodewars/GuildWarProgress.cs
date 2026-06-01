using System;

namespace SRGuildsAndKingdomsPVP.src.nodewars
{
    /// <summary>
    /// Tracks a guild's progress in capturing a node
    /// </summary>
    public class GuildWarProgress
    {
        /// <summary>
        /// Unique identifier of the guild
        /// </summary>
        public string GuildUid { get; set; }

        /// <summary>
        /// Display name of the guild
        /// </summary>
        public string GuildName { get; set; }

        /// <summary>
        /// Current capture points accumulated by this guild
        /// </summary>
        public double CapturePoints { get; set; }

        /// <summary>
        /// Number of guild members currently in the node zone
        /// </summary>
        public int PlayersInZone { get; set; }

        /// <summary>
        /// Total kills by this guild's members in the node war
        /// </summary>
        public int Kills { get; set; }

        /// <summary>
        /// Total deaths of this guild's members in the node war
        /// </summary>
        public int Deaths { get; set; }

        /// <summary>
        /// Last time this progress was updated (real-world UTC time)
        /// </summary>
        public DateTime LastUpdateTime { get; set; }

        /// <summary>
        /// Time when this guild first met capture requirements (for overtime)
        /// </summary>
        public DateTime? FirstCaptureTime { get; set; }

        public GuildWarProgress()
        {
            GuildUid = string.Empty;
            GuildName = string.Empty;
            CapturePoints = 0.0;
            PlayersInZone = 0;
            Kills = 0;
            Deaths = 0;
            LastUpdateTime = DateTime.MinValue;
            FirstCaptureTime = null;
        }

        public GuildWarProgress(string guildUid, string guildName)
        {
            GuildUid = guildUid;
            GuildName = guildName;
            CapturePoints = 0.0;
            PlayersInZone = 0;
            Kills = 0;
            Deaths = 0;
            LastUpdateTime = DateTime.MinValue;
            FirstCaptureTime = null;
        }
    }
}
