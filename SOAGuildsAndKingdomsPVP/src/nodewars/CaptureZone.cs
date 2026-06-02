using Vintagestory.API.MathTools;

namespace SOAGuildsAndKingdomsPVP.src.nodewars
{
    /// <summary>
    /// Represents a capture zone within a node
    /// Players must stand in these zones to accumulate capture points
    /// </summary>
    public class CaptureZone
    {
        /// <summary>
        /// Unique identifier for this capture zone
        /// </summary>
        public string ZoneId { get; set; }

        /// <summary>
        /// Display name of the capture zone
        /// </summary>
        public string ZoneName { get; set; }

        /// <summary>
        /// Center position of the capture zone
        /// </summary>
        public Vec3d Center { get; set; }

        /// <summary>
        /// Radius of the capture zone in blocks
        /// </summary>
        public int Radius { get; set; }

        /// <summary>
        /// Point multiplier for this zone (default: 1.0)
        /// Higher values make this zone more valuable
        /// </summary>
        public double PointMultiplier { get; set; }

        /// <summary>
        /// Whether this capture zone is currently active
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Description of the capture zone
        /// </summary>
        public string Description { get; set; }

        public CaptureZone()
        {
            ZoneId = string.Empty;
            ZoneName = string.Empty;
            Center = new Vec3d(0, 0, 0);
            Radius = 10;
            PointMultiplier = 1.0;
            IsActive = true;
            Description = string.Empty;
        }

        public CaptureZone(string zoneId, string zoneName, Vec3d center, int radius)
        {
            ZoneId = zoneId;
            ZoneName = zoneName;
            Center = center;
            Radius = radius;
            PointMultiplier = 1.0;
            IsActive = true;
            Description = string.Empty;
        }

        /// <summary>
        /// Check if a position is within this capture zone
        /// </summary>
        public bool IsPositionInZone(Vec3d position)
        {
            return Center.DistanceTo(position) <= Radius;
        }
    }
}
