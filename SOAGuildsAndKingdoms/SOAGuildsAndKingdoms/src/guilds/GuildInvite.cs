using System;

namespace SOAGuildsAndKingdoms.src.guilds
{
    public class GuildInvite
    {
        public string InviterUid { get; set; } = null!;
        public string InviteeUid { get; set; } = null!;
        public string GuildName { get; set; } = null!;
        public DateTime Timestamp { get; set; }
        public DateTime ExpiresAt { get; set; }

        /// <summary>
        /// Check if this invite has expired
        /// </summary>
        public bool IsExpired()
        {
            return DateTime.UtcNow > ExpiresAt;
        }

        /// <summary>
        /// Get remaining time before expiry in seconds
        /// </summary>
        public double GetRemainingSeconds()
        {
            return (ExpiresAt - DateTime.UtcNow).TotalSeconds;
        }
    }
}
