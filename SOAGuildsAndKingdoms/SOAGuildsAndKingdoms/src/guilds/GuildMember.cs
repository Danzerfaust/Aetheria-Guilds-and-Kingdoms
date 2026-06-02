using System;

namespace SOAGuildsAndKingdoms.src.guilds
{
    public class GuildMember
    {
        public string PlayerUid { get; set; } = null!;
        public string Role { get; set; } = null!;
        public DateTime LastSeen { get; set; } = DateTime.UtcNow;
        public int PointsContribution { get; set; } = 0;
    }
}
