using Microsoft.Data.Sqlite;
using System;
using Vintagestory.API.Server;

namespace SOAGuildsAndKingdoms.src.database
{
    /// <summary>
    /// Repository for managing guild cooldown data
    /// Cooldowns are written directly (no caching) since they're rare
    /// </summary>
    public class CooldownRepository
    {
        private readonly ICoreServerAPI serverApi;
        private readonly GuildDatabase database;

        public CooldownRepository(ICoreServerAPI serverApi, GuildDatabase database)
        {
            this.serverApi = serverApi;
            this.database = database;
        }

        /// <summary>
        /// Sets a cooldown for a player
        /// </summary>
        public void SetCooldown(string playerUid, DateTime expiresAt)
        {
            try
            {
                var connection = database.Connection;

                const string sql = @"
                    INSERT INTO guild_cooldowns (player_uid, expires_at)
                    VALUES (@playerUid, @expiresAt)
                    ON CONFLICT(player_uid) DO UPDATE SET expires_at = excluded.expires_at;";

                using var command = new SqliteCommand(sql, connection);
                command.Parameters.AddWithValue("@playerUid", playerUid);
                command.Parameters.AddWithValue("@expiresAt", new DateTimeOffset(expiresAt).ToUnixTimeSeconds());
                command.ExecuteNonQuery();

                serverApi.Logger.Debug($"[CooldownRepository] Set cooldown for player '{playerUid}' until {expiresAt:yyyy-MM-dd HH:mm:ss} UTC");
            }
            catch (Exception ex)
            {
                serverApi.Logger.Error($"[CooldownRepository] Failed to set cooldown: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets the cooldown expiry time for a player
        /// </summary>
        public DateTime? GetCooldown(string playerUid)
        {
            try
            {
                var connection = database.Connection;

                const string sql = "SELECT expires_at FROM guild_cooldowns WHERE player_uid = @playerUid;";

                using var command = new SqliteCommand(sql, connection);
                command.Parameters.AddWithValue("@playerUid", playerUid);

                var result = command.ExecuteScalar();
                if (result == null || result == DBNull.Value)
                    return null;

                var expiresAtUnix = Convert.ToInt64(result);
                var expiresAt = DateTimeOffset.FromUnixTimeSeconds(expiresAtUnix).DateTime;

                // Clean up expired cooldown
                if (expiresAt <= DateTime.UtcNow)
                {
                    ClearCooldown(playerUid);
                    return null;
                }

                return expiresAt;
            }
            catch (Exception ex)
            {
                serverApi.Logger.Error($"[CooldownRepository] Failed to get cooldown: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Checks if a player is currently on cooldown
        /// </summary>
        public bool IsOnCooldown(string playerUid, out TimeSpan remainingTime)
        {
            remainingTime = TimeSpan.Zero;

            var expiresAt = GetCooldown(playerUid);
            if (expiresAt == null)
                return false;

            var now = DateTime.UtcNow;
            if (expiresAt.Value <= now)
                return false;

            remainingTime = expiresAt.Value - now;
            return true;
        }

        /// <summary>
        /// Clears a player's cooldown
        /// </summary>
        public bool ClearCooldown(string playerUid)
        {
            try
            {
                var connection = database.Connection;

                const string sql = "DELETE FROM guild_cooldowns WHERE player_uid = @playerUid;";

                using var command = new SqliteCommand(sql, connection);
                command.Parameters.AddWithValue("@playerUid", playerUid);

                int rowsAffected = command.ExecuteNonQuery();

                if (rowsAffected > 0)
                {
                    serverApi.Logger.Debug($"[CooldownRepository] Cleared cooldown for player '{playerUid}'");
                }

                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                serverApi.Logger.Error($"[CooldownRepository] Failed to clear cooldown: {ex.Message}");
                return false;
            }
        }
    }
}
