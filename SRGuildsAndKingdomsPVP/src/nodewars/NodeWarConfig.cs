namespace SRGuildsAndKingdomsPVP.src.nodewars
{
    /// <summary>
    /// Configuration settings for a node war
    /// </summary>
    public class NodeWarConfig
    {
        /// <summary>
        /// Minimum number of guild members required in the zone to start capturing
        /// </summary>
        public int MinPlayersToCapture { get; set; } = 3;

        /// <summary>
        /// Total capture points needed to win the node
        /// </summary>
        public double CapturePointsNeeded { get; set; } = 1000.0;

        /// <summary>
        /// Base capture points gained per second when requirements are met
        /// </summary>
        public double PointsPerSecondBase { get; set; } = 1.0;

        /// <summary>
        /// Additional capture points awarded for each kill in the zone
        /// </summary>
        public double PointsPerKill { get; set; } = 50.0;

        /// <summary>
        /// Points deducted for each death in the zone
        /// </summary>
        public double PointsPerDeath { get; set; } = -10.0;

        /// <summary>
        /// Whether multiple guilds can contest the same node simultaneously
        /// </summary>
        public bool AllowContesting { get; set; } = true;

        /// <summary>
        /// Multiplier applied to capture rate when the node is contested
        /// </summary>
        public double ContestedMultiplier { get; set; } = 0.5;

        /// <summary>
        /// Bonus multiplier per additional player beyond the minimum (diminishing returns)
        /// </summary>
        public double ExtraPlayerBonus { get; set; } = 0.1;

        /// <summary>
        /// Maximum duration of the node war in seconds (0 = no time limit)
        /// </summary>
        public double MaxDurationSeconds { get; set; } = 3600.0; // 1 hour

        /// <summary>
        /// Whether to enable overtime when time expires during contested capture
        /// </summary>
        public bool EnableOvertime { get; set; } = true;

        /// <summary>
        /// Seconds of uncontested control needed to win in overtime
        /// </summary>
        public double OvertimeControlSeconds { get; set; } = 60.0;
    }
}
