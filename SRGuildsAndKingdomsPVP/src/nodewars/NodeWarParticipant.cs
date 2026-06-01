using System;

namespace SRGuildsAndKingdomsPVP.src.nodewars
{
    /// <summary>
    /// Represents a player participating in a node war
    /// </summary>
    public class NodeWarParticipant
    {
        /// <summary>
        /// Unique identifier of the player
        /// </summary>
        public string PlayerUid { get; set; }

        /// <summary>
        /// Display name of the player
        /// </summary>
        public string PlayerName { get; set; }

        /// <summary>
        /// Guild UID the player is fighting for
        /// </summary>
        public string GuildUid { get; set; }

        /// <summary>
        /// Whether the player is actively participating
        /// </summary>
        public bool IsParticipating { get; set; }

        /// <summary>
        /// ID of the node war this player is participating in
        /// </summary>
        public string? CurrentNodeWarId { get; set; }

        /// <summary>
        /// Personal kill count in the current node war
        /// </summary>
        public int Kills { get; set; }

        /// <summary>
        /// Personal death count in the current node war
        /// </summary>
        public int Deaths { get; set; }

        /// <summary>
        /// Time when the player joined the node war (real-world UTC time)
        /// </summary>
        public DateTime JoinTime { get; set; }

        public NodeWarParticipant()
        {
            PlayerUid = string.Empty;
            PlayerName = string.Empty;
            GuildUid = string.Empty;
            IsParticipating = false;
            CurrentNodeWarId = null;
            Kills = 0;
            Deaths = 0;
            JoinTime = DateTime.MinValue;
        }

        public NodeWarParticipant(string playerUid, string playerName, string guildUid, string nodeWarId)
        {
            PlayerUid = playerUid;
            PlayerName = playerName;
            GuildUid = guildUid;
            IsParticipating = true;
            CurrentNodeWarId = nodeWarId;
            Kills = 0;
            Deaths = 0;
            JoinTime = DateTime.MinValue;
        }
    }
}
