SOAGuildsAndKingdoms\SOAGuildsAndKingdoms\src\guilds\LandClaim.cs
using System;

namespace SOAGuildsAndKingdoms.src.guilds
{
    /// <summary>
    /// Simple rectangular land claim centered on (CenterX, CenterZ) with a radius (in blocks).
    /// Stored as center+radius to keep serialization small and simple.
    /// </summary>
    public class LandClaim
    {
        public int CenterX { get; set; }
        public int CenterZ { get; set; }
        public int Radius { get; set; }
        public string ClaimedByUid { get; set; }
        public DateTime Timestamp { get; set; }

        // Convenience properties for overlap checks
        public int MinX => CenterX - Radius;
        public int MaxX => CenterX + Radius;
        public int MinZ => CenterZ - Radius;
        public int MaxZ => CenterZ + Radius;

        public bool Intersects(LandClaim other)
        {
            if (other == null) return false;
            // Rectangular intersection test
            return !(MaxX < other.MinX || MinX > other.MaxX || MaxZ < other.MinZ || MinZ > other.MaxZ);
        }
    }
}