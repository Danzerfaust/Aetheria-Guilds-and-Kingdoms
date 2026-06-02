using System;

namespace SOAGuildsAndKingdoms.src.guilds
{
    /// <summary>
    /// Special land claim representing an outpost - a 1x1 chunk claim that doesn't need to be adjacent to existing claims.
    /// Outposts allow guilds to establish remote territory for strategic purposes without the adjacency restriction.
    /// </summary>
    public class OutpostClaim : LandClaim
    {
        /// <summary>
        /// Mark this as an outpost claim type
        /// </summary>
        public bool IsOutpost { get; set; } = true;

        /// <summary>
        /// Optional name/description for the outpost
        /// </summary>
        public string OutpostName { get; set; } = "";

        public OutpostClaim()
        {
            // Default constructor for serialization
            IsOutpost = true;
        }

        /// <summary>
        /// Creates a new outpost claim at the specified chunk coordinates
        /// </summary>
        public OutpostClaim(int chunkX, int chunkZ, string claimedByUid, string outpostName = "")
        {
            IsOutpost = true;
            ChunkX = chunkX;
            ChunkZ = chunkZ;
            ClaimedByUid = claimedByUid;
            OutpostName = outpostName;
            Timestamp = DateTime.UtcNow;
        }

        /// <summary>
        /// Create an OutpostClaim for a specific chunk.
        /// </summary>
        public static OutpostClaim CreateFromChunk(int chunkX, int chunkZ, string? claimedByUid = null, string outpostName = "")
        {
            return new OutpostClaim(chunkX, chunkZ, claimedByUid ?? "", outpostName);
        }

        /// <summary>
        /// Create an OutpostClaim for the chunk that contains the given block coordinates.
        /// </summary>
        public static OutpostClaim CreateFromBlockPosition(int blockX, int blockZ, string? claimedByUid = null, string outpostName = "")
        {
            int chunkX = FloorDiv(blockX, ChunkSize);
            int chunkZ = FloorDiv(blockZ, ChunkSize);
            return CreateFromChunk(chunkX, chunkZ, claimedByUid, outpostName);
        }
    }
}