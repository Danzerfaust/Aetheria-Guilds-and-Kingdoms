using System;
using System.Collections.Generic;
using System.Linq;

namespace SOAGuildsAndKingdoms.src.guilds
{
    /// <summary>
    /// Special land claim representing a guild's home territory - a 2x2 chunk area.
    /// This is typically the first claim established by a guild and serves as their central base.
    /// </summary>
    public class GuildHomeClaim : LandClaim
    {
        /// <summary>
        /// The 2x2 area of chunks that make up the guild home
        /// </summary>
        public List<LandClaim> HomeChunks { get; set; } = new();

        /// <summary>
        /// The center chunk coordinates around which the home was established
        /// </summary>
        public int CenterChunkX { get; set; }
        public int CenterChunkZ { get; set; }

        /// <summary>
        /// Mark this as a guild home claim type
        /// </summary>
        public bool IsGuildHome { get; set; } = true;

        // Override base properties to reflect the 2x2 area bounds
        public override int MinChunkX => HomeChunks.Count > 0 ? HomeChunks.Min(c => c.ChunkX) : CenterChunkX;
        public override int MaxChunkX => HomeChunks.Count > 0 ? HomeChunks.Max(c => c.ChunkX) : CenterChunkX;
        public override int MinChunkZ => HomeChunks.Count > 0 ? HomeChunks.Min(c => c.ChunkZ) : CenterChunkZ;
        public override int MaxChunkZ => HomeChunks.Count > 0 ? HomeChunks.Max(c => c.ChunkZ) : CenterChunkZ;

        // Override block bounds to cover the entire 2x2 area
        public override int MinBlockX => MinChunkX * ChunkSize;
        public override int MaxBlockX => ((MaxChunkX + 1) * ChunkSize) - 1;
        public override int MinBlockZ => MinChunkZ * ChunkSize;
        public override int MaxBlockZ => ((MaxChunkZ + 1) * ChunkSize) - 1;

        public GuildHomeClaim()
        {
            // Default constructor for serialization
            IsGuildHome = true;
        }

        /// <summary>
        /// Creates a new guild home claim centered on the specified chunk coordinates
        /// </summary>
        public GuildHomeClaim(int centerChunkX, int centerChunkZ, string claimedByUid)
        {
            IsGuildHome = true;
            CenterChunkX = centerChunkX;
            CenterChunkZ = centerChunkZ;
            ClaimedByUid = claimedByUid;
            Timestamp = DateTime.UtcNow;

            // Set the base class properties to the center chunk (for compatibility)
            ChunkX = centerChunkX;
            ChunkZ = centerChunkZ;

            // Generate the 2x2 chunk area
            GenerateHomeChunks();
        }

        /// <summary>
        /// Generates the 2x2 chunk area for the guild home
        /// </summary>
        public void GenerateHomeChunks()
        {
            HomeChunks.Clear();

            for (int dx = 0; dx <= 1; dx++)
            {
                for (int dz = 0; dz <= 1; dz++)
                {
                    int chunkX = CenterChunkX + dx;
                    int chunkZ = CenterChunkZ + dz;

                    var chunk = new LandClaim
                    {
                        ChunkX = chunkX,
                        ChunkZ = chunkZ,
                        ClaimedByUid = ClaimedByUid,
                        Timestamp = Timestamp
                    };

                    HomeChunks.Add(chunk);
                }
            }
        }

        /// <summary>
        /// Checks if this guild home intersects with another claim
        /// </summary>
        public override bool Intersects(LandClaim other)
        {
            if (other == null) return false;

            if (other is GuildHomeClaim otherHome)
            {
                // Check if any chunks in this home intersect with any chunks in the other home
                return HomeChunks.Any(thisChunk =>
                    otherHome.HomeChunks.Any(otherChunk => thisChunk.Intersects(otherChunk)));
            }

            // Check if the other claim intersects with any of our home chunks
            return HomeChunks.Any(chunk => chunk.Intersects(other));
        }

        /// <summary>
        /// Returns true if this guild home contains the specified chunk coordinate
        /// </summary>
        public override bool ContainsChunk(int chunkX, int chunkZ)
        {
            return HomeChunks.Any(chunk => chunk.ContainsChunk(chunkX, chunkZ));
        }

        /// <summary>
        /// Returns true if the block coordinate lies within the guild home area
        /// </summary>
        public override bool ContainsBlockCoord(int blockX, int blockZ)
        {
            return HomeChunks.Any(chunk => chunk.ContainsBlockCoord(blockX, blockZ));
        }

        /// <summary>
        /// Gets all individual chunk claims that make up this guild home
        /// </summary>
        public IEnumerable<LandClaim> GetIndividualChunks()
        {
            return HomeChunks;
        }

        /// <summary>
        /// Returns the number of chunks in this guild home (always 4 for a 2x2 area)
        /// </summary>
        public int ChunkCount => HomeChunks.Count;

        /// <summary>
        /// Creates a guild home claim from the specified center chunk coordinates
        /// </summary>
        public static GuildHomeClaim CreateFromCenterChunk(int centerChunkX, int centerChunkZ, string claimedByUid)
        {
            return new GuildHomeClaim(centerChunkX, centerChunkZ, claimedByUid);
        }

        /// <summary>
        /// Creates a guild home claim from block position (will determine center chunk)
        /// </summary>
        public static new GuildHomeClaim CreateFromBlockPosition(int blockX, int blockZ, string claimedByUid)
        {
            int centerChunkX = FloorDiv(blockX, ChunkSize);
            int centerChunkZ = FloorDiv(blockZ, ChunkSize);
            return CreateFromCenterChunk(centerChunkX, centerChunkZ, claimedByUid);
        }

        /// <summary>
        /// Returns block bounds as a tuple covering the entire 2x2 area
        /// </summary>
        public override (int minX, int maxX, int minZ, int maxZ) ToBlockBounds()
            => (MinBlockX, MaxBlockX, MinBlockZ, MaxBlockZ);
    }
}