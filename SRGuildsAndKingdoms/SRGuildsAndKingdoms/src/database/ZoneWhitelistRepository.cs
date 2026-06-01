using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using Vintagestory.API.Server;

namespace SRGuildsAndKingdoms.src.database
{
    /// <summary>
    /// Repository for managing zone whitelist data using zone IDs.
    /// Zone whitelist data are written directly (no caching in repository) since they're rare.
    /// </summary>
    public class ZoneWhitelistRepository
    {
        private readonly ICoreServerAPI serverApi;
        private readonly GuildDatabase database;

        public ZoneWhitelistRepository(ICoreServerAPI serverApi, GuildDatabase database)
        {
            this.serverApi = serverApi;
            this.database = database;
        }

        /// <summary>
        /// Adds a player to a zone's whitelist by zone ID
        /// </summary>
        /// <param name="zoneId">The zone's unique ID (from ProtectedZone.Id in config)</param>
        /// <param name="playerUid">The player's unique ID</param>
        public bool AddPlayerToZone(int zoneId, string playerUid)
        {
            if (zoneId < 0 || string.IsNullOrWhiteSpace(playerUid))
                return false;

            try
            {
                var connection = database.Connection;

                const string sql = @"
                    INSERT INTO zone_whitelists (zone_id, player_uid)
                    VALUES (@zoneId, @playerUid)
                    ON CONFLICT(zone_id, player_uid) DO NOTHING;";

                using var command = new SqliteCommand(sql, connection);
                command.Parameters.AddWithValue("@zoneId", zoneId);
                command.Parameters.AddWithValue("@playerUid", playerUid);

                int rowsAffected = command.ExecuteNonQuery();

                if (rowsAffected > 0)
                {
                    serverApi.Logger.Debug($"[SRGuildsAndKingdoms:ZoneWhitelistRepository] Added player '{playerUid}' to zone ID {zoneId}");
                }

                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                serverApi.Logger.Error($"[SRGuildsAndKingdoms:ZoneWhitelistRepository] Failed to add player to zone: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Removes a player from a zone's whitelist by zone ID
        /// </summary>
        public bool RemovePlayerFromZone(int zoneId, string playerUid)
        {
            if (zoneId < 0 || string.IsNullOrWhiteSpace(playerUid))
                return false;

            try
            {
                var connection = database.Connection;

                const string sql = @"
                    DELETE FROM zone_whitelists 
                    WHERE zone_id = @zoneId AND player_uid = @playerUid;";

                using var command = new SqliteCommand(sql, connection);
                command.Parameters.AddWithValue("@zoneId", zoneId);
                command.Parameters.AddWithValue("@playerUid", playerUid);

                int rowsAffected = command.ExecuteNonQuery();

                if (rowsAffected > 0)
                {
                    serverApi.Logger.Debug($"[SRGuildsAndKingdoms:ZoneWhitelistRepository] Removed player '{playerUid}' from zone ID {zoneId}");
                }

                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                serverApi.Logger.Error($"[SRGuildsAndKingdoms:ZoneWhitelistRepository] Failed to remove player from zone: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Checks if a player is whitelisted for a specific zone by zone ID
        /// </summary>
        public bool IsPlayerWhitelisted(int zoneId, string playerUid)
        {
            if (zoneId < 0 || string.IsNullOrWhiteSpace(playerUid))
                return false;

            try
            {
                var connection = database.Connection;

                const string sql = @"
                    SELECT COUNT(*) FROM zone_whitelists 
                    WHERE zone_id = @zoneId AND player_uid = @playerUid;";

                using var command = new SqliteCommand(sql, connection);
                command.Parameters.AddWithValue("@zoneId", zoneId);
                command.Parameters.AddWithValue("@playerUid", playerUid);

                var count = Convert.ToInt32(command.ExecuteScalar());
                return count > 0;
            }
            catch (Exception ex)
            {
                serverApi.Logger.Error($"[SRGuildsAndKingdoms:ZoneWhitelistRepository] Failed to check whitelist: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Gets all zone IDs a player is whitelisted for
        /// </summary>
        public List<int> GetWhitelistedZones(string playerUid)
        {
            var zones = new List<int>();

            if (string.IsNullOrWhiteSpace(playerUid))
                return zones;

            try
            {
                var connection = database.Connection;

                const string sql = "SELECT zone_id FROM zone_whitelists WHERE player_uid = @playerUid;";

                using var command = new SqliteCommand(sql, connection);
                command.Parameters.AddWithValue("@playerUid", playerUid);

                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    zones.Add(reader.GetInt32(0));
                }
            }
            catch (Exception ex)
            {
                serverApi.Logger.Error($"[SRGuildsAndKingdoms:ZoneWhitelistRepository] Failed to get whitelisted zones: {ex.Message}");
            }

            return zones;
        }

        /// <summary>
        /// Gets all players whitelisted for a specific zone by zone ID
        /// </summary>
        public List<string> GetWhitelistedPlayers(int zoneId)
        {
            var players = new List<string>();

            if (zoneId < 0)
                return players;

            try
            {
                var connection = database.Connection;

                const string sql = "SELECT player_uid FROM zone_whitelists WHERE zone_id = @zoneId;";

                using var command = new SqliteCommand(sql, connection);
                command.Parameters.AddWithValue("@zoneId", zoneId);

                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    players.Add(reader.GetString(0));
                }
            }
            catch (Exception ex)
            {
                serverApi.Logger.Error($"[SRGuildsAndKingdoms:ZoneWhitelistRepository] Failed to get whitelisted players: {ex.Message}");
            }

            return players;
        }

        /// <summary>
        /// Clears all players from a zone's whitelist by zone ID
        /// </summary>
        public int ClearZone(int zoneId)
        {
            if (zoneId < 0)
                return 0;

            try
            {
                var connection = database.Connection;

                const string sql = "DELETE FROM zone_whitelists WHERE zone_id = @zoneId;";

                using var command = new SqliteCommand(sql, connection);
                command.Parameters.AddWithValue("@zoneId", zoneId);

                int rowsDeleted = command.ExecuteNonQuery();

                if (rowsDeleted > 0)
                {
                    serverApi.Logger.Debug($"[SRGuildsAndKingdoms:ZoneWhitelistRepository] Cleared {rowsDeleted} player(s) from zone ID {zoneId}");
                }

                return rowsDeleted;
            }
            catch (Exception ex)
            {
                serverApi.Logger.Error($"[SRGuildsAndKingdoms:ZoneWhitelistRepository] Failed to clear zone: {ex.Message}");
                return 0;
            }
        }

        /// <summary>
        /// Gets all zone IDs that have whitelisted players
        /// </summary>
        public List<int> GetAllZoneIds()
        {
            var zones = new List<int>();

            try
            {
                var connection = database.Connection;

                const string sql = "SELECT DISTINCT zone_id FROM zone_whitelists;";

                using var command = new SqliteCommand(sql, connection);
                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    zones.Add(reader.GetInt32(0));
                }
            }
            catch (Exception ex)
            {
                serverApi.Logger.Error($"[SRGuildsAndKingdoms:ZoneWhitelistRepository] Failed to get zone IDs: {ex.Message}");
            }

            return zones;
        }

        /// <summary>
        /// Gets the total number of whitelisted players across all zones (distinct)
        /// </summary>
        public int GetTotalWhitelistedPlayersCount()
        {
            try
            {
                var connection = database.Connection;

                const string sql = "SELECT COUNT(DISTINCT player_uid) FROM zone_whitelists;";

                using var command = new SqliteCommand(sql, connection);
                return Convert.ToInt32(command.ExecuteScalar());
            }
            catch (Exception ex)
            {
                serverApi.Logger.Error($"[SRGuildsAndKingdoms:ZoneWhitelistRepository] Failed to get total player count: {ex.Message}");
                return 0;
            }
        }

        /// <summary>
        /// Gets all whitelist data (for cache loading and admin/debugging purposes)
        /// Returns: Dictionary&lt;zoneId, List&lt;playerUid&gt;&gt;
        /// </summary>
        public Dictionary<int, List<string>> GetAllWhitelists()
        {
            var whitelists = new Dictionary<int, List<string>>();

            try
            {
                var connection = database.Connection;

                const string sql = "SELECT zone_id, player_uid FROM zone_whitelists ORDER BY zone_id, player_uid;";

                using var command = new SqliteCommand(sql, connection);
                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    var zoneId = reader.GetInt32(0);
                    var playerUid = reader.GetString(1);

                    if (!whitelists.ContainsKey(zoneId))
                    {
                        whitelists[zoneId] = new List<string>();
                    }

                    whitelists[zoneId].Add(playerUid);
                }
            }
            catch (Exception ex)
            {
                serverApi.Logger.Error($"[SRGuildsAndKingdoms:ZoneWhitelistRepository] Failed to get all whitelists: {ex.Message}");
            }

            return whitelists;
        }
    }
}
