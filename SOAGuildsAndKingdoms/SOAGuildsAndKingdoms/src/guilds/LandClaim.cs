using System;
using System.Text.Json.Serialization;
using Vintagestory.API.Config;

namespace SOAGuildsAndKingdoms.src.guilds
{
    /// <summary>
    /// Land claim represented in VintageStory chunk coordinates.
    /// Vintage Story uses 16x16 block chunks horizontally; this class represents
    /// a single claimed chunk at (ChunkX, ChunkZ).
    /// Each claim now represents exactly one 16x16 chunk instead of a radius-based area.
    /// </summary>
    [JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
    [JsonDerivedType(typeof(LandClaim), "LandClaim")]
    [JsonDerivedType(typeof(GuildHomeClaim), "GuildHomeClaim")]
    [JsonDerivedType(typeof(OutpostClaim), "OutpostClaim")]
    public class LandClaim
    {
        // Vintage Story chunk size (blocks per chunk on X/Z)
        public const int ChunkSize = GlobalConstants.ChunkSize;

        // Chunk coordinates - each claim represents exactly one chunk
        public int ChunkX { get; set; }
        public int ChunkZ { get; set; }

        public string? ClaimedByUid { get; set; }
        public DateTime Timestamp { get; set; }

        // Convenience chunk bounds (this claim covers exactly one chunk) - made virtual for GuildHomeClaim
        public virtual int MinChunkX => ChunkX;
        public virtual int MaxChunkX => ChunkX;
        public virtual int MinChunkZ => ChunkZ;
        public virtual int MaxChunkZ => ChunkZ;

        // Derived block bounds (inclusive block coordinates) that this chunk covers - made virtual for GuildHomeClaim
        public virtual int MinBlockX => ChunkX * ChunkSize;
        public virtual int MaxBlockX => ((ChunkX + 1) * ChunkSize) - 1;
        public virtual int MinBlockZ => ChunkZ * ChunkSize;
        public virtual int MaxBlockZ => ((ChunkZ + 1) * ChunkSize) - 1;

        /// <summary>
        /// Checks chunk-aligned intersection. Since each claim is exactly one chunk,
        /// this returns true only if both claims are for the same chunk.
        /// Made virtual so GuildHomeClaim can override with 2x2 logic.
        /// </summary>
        public virtual bool Intersects(LandClaim other)
        {
            if (other == null) return false;
            return ChunkX == other.ChunkX && ChunkZ == other.ChunkZ;
        }

        /// <summary>
        /// Returns true if this claim contains the chunk coordinate (chunkX, chunkZ).
        /// Since each claim is exactly one chunk, this is a simple equality check.
        /// Made virtual so GuildHomeClaim can override with 2x2 logic.
        /// </summary>
        public virtual bool ContainsChunk(int chunkX, int chunkZ)
        {
            return ChunkX == chunkX && ChunkZ == chunkZ;
        }

        /// <summary>
        /// Returns true if the block coordinate (blockX, blockZ) lies within this claim.
        /// Made virtual so GuildHomeClaim can override with 2x2 logic.
        /// </summary>
        public virtual bool ContainsBlockCoord(int blockX, int blockZ)
        {
            return blockX >= MinBlockX && blockX <= MaxBlockX &&
                   blockZ >= MinBlockZ && blockZ <= MaxBlockZ;
        }

        /// <summary>
        /// Create a LandClaim for a specific chunk.
        /// </summary>
        public static LandClaim CreateFromChunk(int chunkX, int chunkZ, string? claimedByUid = null)
        {
            return new LandClaim
            {
                ChunkX = chunkX,
                ChunkZ = chunkZ,
                ClaimedByUid = claimedByUid,
                Timestamp = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Create a LandClaim for the chunk that contains the given block coordinates.
        /// </summary>
        public static LandClaim CreateFromBlockPosition(int blockX, int blockZ, string? claimedByUid = null)
        {
            int chunkX = FloorDiv(blockX, ChunkSize);
            int chunkZ = FloorDiv(blockZ, ChunkSize);
            return CreateFromChunk(chunkX, chunkZ, claimedByUid);
        }

        /// <summary>
        /// Returns block bounds as a tuple (minX, maxX, minZ, maxZ).
        /// Made virtual so GuildHomeClaim can override.
        /// </summary>
        public virtual (int minX, int maxX, int minZ, int maxZ) ToBlockBounds() => (MinBlockX, MaxBlockX, MinBlockZ, MaxBlockZ);

        // Helper: floor division that handles negative coordinates like Vintage Story expects for chunk coords.
        public static int FloorDiv(int a, int b)
        {
            // Use Math.Floor on double to ensure correct floor division for negative numbers
            return (int)Math.Floor(a / (double)b);
        }
    }
}