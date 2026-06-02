using SOAGuildsAndKingdoms.src.database;
using System;
using System.Collections.Generic;
using System.Linq;
using Vintagestory.API.Server;

namespace SOAGuildsAndKingdoms.src.config
{
    /// <summary>
    /// Manages zone whitelist data with in-memory cache and database persistence
    /// </summary>
    /// <remarks>
    /// Initializes the zone whitelist manager
    /// </remarks>
    /// <param name="api">Server API instance</param>
    /// <param name="repository">Zone whitelist repository for database operations</param>
    public class ZoneWhitelistManager(ICoreServerAPI api, ZoneWhitelistRepository repository)
    {
        // Fast lookup cache: zoneId -> set of player UIDs
        private readonly Dictionary<int, HashSet<string>> zoneWhitelists = [];
        private bool cacheLoaded = false;
        private SOAGuildsAndKingdomsModSystem? modSystem;

        /// <summary>
        /// Adds a player to a zone's whitelist by zone ID
        /// </summary>
        /// <param name="zoneId">Zone's unique ID (from ProtectedZone.Id in config)</param>
        /// <param name="playerUid">Player's unique ID</param>
        /// <returns>True if added successfully, false if already whitelisted</returns>
        public bool AddPlayerToZone(int zoneId, string playerUid)
        {
            EnsureCacheLoaded();

            if (zoneId < 0 || string.IsNullOrWhiteSpace(playerUid))
                return false;

            if (!zoneWhitelists.ContainsKey(zoneId))
            {
                zoneWhitelists[zoneId] = new HashSet<string>();
            }

            bool added = zoneWhitelists[zoneId].Add(playerUid);

            if (added)
            {
                repository.AddPlayerToZone(zoneId, playerUid);
                modSystem?.NetworkHandler?.BroadcastGuildConfigToAll();
            }

            return added;
        }

        /// <summary>
        /// Removes a player from a zone's whitelist by zone ID
        /// </summary>
        /// <param name="zoneId">Zone's unique ID</param>
        /// <param name="playerUid">Player's unique ID</param>
        /// <returns>True if removed successfully, false if not found</returns>
        public bool RemovePlayerFromZone(int zoneId, string playerUid)
        {
            EnsureCacheLoaded();

            if (zoneId < 0 || string.IsNullOrWhiteSpace(playerUid))
                return false;

            if (!zoneWhitelists.ContainsKey(zoneId))
                return false;

            bool removed = zoneWhitelists[zoneId].Remove(playerUid);

            if (removed)
            {
                repository.RemovePlayerFromZone(zoneId, playerUid);

                // Clean up empty zone entries in cache
                if (zoneWhitelists[zoneId].Count == 0)
                {
                    zoneWhitelists.Remove(zoneId);
                }

                modSystem?.NetworkHandler?.BroadcastGuildConfigToAll();
            }

            return removed;
        }

        /// <summary>
        /// Checks if a player is whitelisted for a specific zone by zone ID
        /// </summary>
        /// <param name="zoneId">Zone's unique ID</param>
        /// <param name="playerUid">Player's unique ID</param>
        /// <returns>True if player is whitelisted</returns>
        public bool IsPlayerWhitelisted(int zoneId, string playerUid)
        {
            EnsureCacheLoaded();

            if (zoneId < 0 || string.IsNullOrWhiteSpace(playerUid))
                return false;

            return zoneWhitelists.ContainsKey(zoneId) &&
                   zoneWhitelists[zoneId].Contains(playerUid);
        }

        /// <summary>
        /// Gets all zone IDs a player is whitelisted for
        /// </summary>
        /// <param name="playerUid">Player's unique ID</param>
        /// <returns>List of zone IDs</returns>
        public List<int> GetWhitelistedZones(string playerUid)
        {
            EnsureCacheLoaded();

            if (string.IsNullOrWhiteSpace(playerUid))
                return new List<int>();

            return zoneWhitelists
                .Where(kvp => kvp.Value.Contains(playerUid))
                .Select(kvp => kvp.Key)
                .ToList();
        }

        /// <summary>
        /// Gets all players whitelisted for a specific zone by zone ID
        /// </summary>
        /// <param name="zoneId">Zone's unique ID</param>
        /// <returns>List of player UIDs</returns>
        public List<string> GetWhitelistedPlayers(int zoneId)
        {
            EnsureCacheLoaded();

            if (zoneId < 0)
                return new List<string>();

            if (!zoneWhitelists.ContainsKey(zoneId))
                return new List<string>();

            return zoneWhitelists[zoneId].ToList();
        }

        /// <summary>
        /// Gets the number of whitelisted players for a specific zone
        /// </summary>
        /// <param name="zoneId">Zone's unique ID</param>
        /// <returns>Count of whitelisted players</returns>
        public int GetWhitelistedPlayersCount(int zoneId)
        {
            EnsureCacheLoaded();

            if (zoneId < 0)
                return 0;

            if (!zoneWhitelists.ContainsKey(zoneId))
                return 0;

            return zoneWhitelists[zoneId].Count;
        }

        /// <summary>
        /// Clears all players from a zone's whitelist by zone ID
        /// </summary>
        /// <param name="zoneId">Zone's unique ID</param>
        /// <returns>Number of players removed</returns>
        public int ClearZone(int zoneId)
        {
            EnsureCacheLoaded();

            if (zoneId < 0)
                return 0;

            if (!zoneWhitelists.ContainsKey(zoneId))
                return 0;

            int count = zoneWhitelists[zoneId].Count;
            zoneWhitelists.Remove(zoneId);

            if (count > 0)
            {
                repository.ClearZone(zoneId);
                modSystem?.NetworkHandler?.BroadcastGuildConfigToAll();
            }

            return count;
        }

        /// <summary>
        /// Gets all zone IDs that have whitelisted players
        /// </summary>
        /// <returns>List of zone IDs</returns>
        public List<int> GetAllZoneIds()
        {
            EnsureCacheLoaded();
            return zoneWhitelists.Keys.ToList();
        }

        /// <summary>
        /// Gets the total number of whitelisted players across all zones
        /// </summary>
        public int GetTotalWhitelistedPlayersCount()
        {
            EnsureCacheLoaded();
            return zoneWhitelists.Values.SelectMany(s => s).Distinct().Count();
        }

        /// <summary>
        /// Loads whitelist data from database into cache.
        /// Called automatically on first access, but can be called manually during startup.
        /// </summary>
        public void Load()
        {
            try
            {
                api.Logger.Notification("[ZoneWhitelist] Loading zone whitelists from database...");

                zoneWhitelists.Clear();

                var allWhitelists = repository.GetAllWhitelists();

                foreach (var kvp in allWhitelists)
                {
                    zoneWhitelists[kvp.Key] = new HashSet<string>(kvp.Value);
                }

                cacheLoaded = true;
                api.Logger.Notification($"[ZoneWhitelist] Loaded {zoneWhitelists.Count} zone whitelist(s) from database");

                modSystem = api.ModLoader.GetModSystem<SOAGuildsAndKingdomsModSystem>();
            }
            catch (Exception ex)
            {
                api.Logger.Error($"[ZoneWhitelist] Failed to load whitelist data: {ex.Message}");
                zoneWhitelists.Clear();
                cacheLoaded = true; // Mark as loaded to prevent repeated failures
            }
        }

        /// <summary>
        /// Ensures the cache is loaded before accessing data
        /// </summary>
        private void EnsureCacheLoaded()
        {
            if (!cacheLoaded)
            {
                Load();
            }
        }
    }
}
