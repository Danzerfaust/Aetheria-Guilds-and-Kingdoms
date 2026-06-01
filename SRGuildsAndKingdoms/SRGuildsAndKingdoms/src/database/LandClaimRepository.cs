using SRGuildsAndKingdoms.src.guilds;
using System;
using System.Collections.Generic;
using Vintagestory.API.Server;

namespace SRGuildsAndKingdoms.src.database
{
    /// <summary>
    /// Specialized repository for optimized land claim queries
    /// </summary>
    public class LandClaimRepository(ICoreServerAPI serverApi, GuildRepository guildRepository)
    {

        // Spatial index: (chunkX, chunkZ) -> guild name
        private readonly Dictionary<(int chunkX, int chunkZ), string> chunkToGuildIndex = [];

        // Reverse index: guild name -> list of chunk coordinates
        private readonly Dictionary<string, List<(int chunkX, int chunkZ)>> guildToChunksIndex = [];

        /// <summary>
        /// Builds the spatial indexes from all guilds' land claims
        /// Call this after loading guilds into the repository
        /// </summary>
        public void RebuildIndexes()
        {
            try
            {
                chunkToGuildIndex.Clear();
                guildToChunksIndex.Clear();

                var allGuilds = guildRepository.GetAllGuilds();

                foreach (var guild in allGuilds)
                {
                    var chunkList = new List<(int, int)>();

                    foreach (var claim in guild.Claims)
                    {
                        // Handle polymorphic claims
                        if (claim is GuildHomeClaim guildHome)
                        {
                            // Guild home is 2x2 chunks
                            foreach (var homeChunk in guildHome.GetIndividualChunks())
                            {
                                var coords = (homeChunk.ChunkX, homeChunk.ChunkZ);
                                chunkToGuildIndex[coords] = guild.Name;
                                chunkList.Add(coords);
                            }
                        }
                        else
                        {
                            // Regular claim or outpost (single chunk)
                            var coords = (claim.ChunkX, claim.ChunkZ);
                            chunkToGuildIndex[coords] = guild.Name;
                            chunkList.Add(coords);
                        }
                    }

                    if (chunkList.Count > 0)
                    {
                        guildToChunksIndex[guild.Name] = chunkList;
                    }
                }
            }
            catch (Exception ex)
            {
                serverApi.Logger.Error($"[LandClaimRepository] Failed to build indexes: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Gets the guild that owns a specific chunk
        /// </summary>
        /// <param name="chunkX">Chunk X coordinate</param>
        /// <param name="chunkZ">Chunk Z coordinate</param>
        /// <returns>Guild name if chunk is claimed, null otherwise</returns>
        public string? GetGuildOwningChunk(int chunkX, int chunkZ)
        {
            var coords = (chunkX, chunkZ);
            return chunkToGuildIndex.TryGetValue(coords, out var guildName) ? guildName : null;
        }

        /// <summary>
        /// Gets all chunks claimed by a specific guild
        /// </summary>
        /// <param name="guildName">Name of the guild</param>
        /// <returns>List of chunk coordinates</returns>
        public List<(int chunkX, int chunkZ)> GetGuildClaims(string guildName)
        {
            return guildToChunksIndex.TryGetValue(guildName, out var chunks)
                ? new List<(int, int)>(chunks)
                : new List<(int, int)>();
        }

        /// <summary>
        /// Checks if a chunk is claimed by any guild
        /// </summary>
        /// <param name="chunkX">Chunk X coordinate</param>
        /// <param name="chunkZ">Chunk Z coordinate</param>
        /// <returns>True if chunk is claimed</returns>
        public bool IsChunkClaimed(int chunkX, int chunkZ)
        {
            return chunkToGuildIndex.ContainsKey((chunkX, chunkZ));
        }

        /// <summary>
        /// Adds a chunk claim to the indexes
        /// Call this when a guild claims new land
        /// </summary>
        /// <param name="guildName">Name of the guild</param>
        /// <param name="chunkX">Chunk X coordinate</param>
        /// <param name="chunkZ">Chunk Z coordinate</param>
        public void AddClaimToIndex(string guildName, int chunkX, int chunkZ)
        {
            var coords = (chunkX, chunkZ);
            chunkToGuildIndex[coords] = guildName;

            if (!guildToChunksIndex.ContainsKey(guildName))
            {
                guildToChunksIndex[guildName] = new List<(int, int)>();
            }

            guildToChunksIndex[guildName].Add(coords);
        }

        /// <summary>
        /// Removes a chunk claim from the indexes
        /// Call this when a guild unclaims land
        /// </summary>
        /// <param name="chunkX">Chunk X coordinate</param>
        /// <param name="chunkZ">Chunk Z coordinate</param>
        public void RemoveClaimFromIndex(int chunkX, int chunkZ)
        {
            var coords = (chunkX, chunkZ);

            if (chunkToGuildIndex.TryGetValue(coords, out var guildName))
            {
                chunkToGuildIndex.Remove(coords);

                if (guildToChunksIndex.TryGetValue(guildName, out var chunkList))
                {
                    chunkList.Remove(coords);

                    // Clean up empty guild entries
                    if (chunkList.Count == 0)
                    {
                        guildToChunksIndex.Remove(guildName);
                    }
                }
            }
        }

        /// <summary>
        /// Updates the indexes when a guild changes its name
        /// </summary>
        /// <param name="oldName">Old guild name</param>
        /// <param name="newName">New guild name</param>
        public void UpdateGuildName(string oldName, string newName)
        {
            if (guildToChunksIndex.TryGetValue(oldName, out var chunkList))
            {
                // Update reverse index
                guildToChunksIndex.Remove(oldName);
                guildToChunksIndex[newName] = chunkList;

                // Update forward index
                foreach (var coords in chunkList)
                {
                    chunkToGuildIndex[coords] = newName;
                }
            }
        }

        /// <summary>
        /// Removes all claims for a guild from the indexes
        /// Call this when a guild is deleted or disbanded
        /// </summary>
        /// <param name="guildName">Name of the guild</param>
        public void RemoveGuildFromIndex(string guildName)
        {
            if (guildToChunksIndex.TryGetValue(guildName, out var chunkList))
            {
                // Remove all chunks from forward index
                foreach (var coords in chunkList)
                {
                    chunkToGuildIndex.Remove(coords);
                }

                // Remove from reverse index
                guildToChunksIndex.Remove(guildName);
            }
        }

        /// <summary>
        /// Gets statistics about claimed chunks
        /// </summary>
        /// <returns>Total claimed chunks and number of guilds with claims</returns>
        public (int totalClaimedChunks, int guildsWithClaims) GetStatistics()
        {
            return (chunkToGuildIndex.Count, guildToChunksIndex.Count);
        }

        /// <summary>
        /// Finds chunks within a radius of a point (for neighbor checks, etc.)
        /// </summary>
        /// <param name="centerX">Center chunk X</param>
        /// <param name="centerZ">Center chunk Z</param>
        /// <param name="radius">Radius in chunks</param>
        /// <returns>List of claimed chunks with their owning guild names</returns>
        public List<(int chunkX, int chunkZ, string guildName)> GetClaimsInRadius(int centerX, int centerZ, int radius)
        {
            var results = new List<(int, int, string)>();

            for (int x = centerX - radius; x <= centerX + radius; x++)
            {
                for (int z = centerZ - radius; z <= centerZ + radius; z++)
                {
                    var coords = (x, z);
                    if (chunkToGuildIndex.TryGetValue(coords, out var guildName))
                    {
                        results.Add((x, z, guildName));
                    }
                }
            }

            return results;
        }
    }
}
