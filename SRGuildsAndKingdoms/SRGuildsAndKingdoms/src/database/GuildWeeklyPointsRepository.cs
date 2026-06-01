using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using Vintagestory.API.Server;
using SRGuildsAndKingdoms.src.utils;

namespace SRGuildsAndKingdoms.src.database
{
    /// <summary>
    /// Repository for managing weekly guild GRS points tracking
    /// </summary>
    public class GuildWeeklyPointsRepository
    {
        private readonly ICoreServerAPI serverApi;
        private readonly GuildDatabase guildDatabase;

        public GuildWeeklyPointsRepository(ICoreServerAPI serverApi, GuildDatabase guildDatabase)
        {
            this.serverApi = serverApi;
            this.guildDatabase = guildDatabase;
        }

        public int GetWeeklyPointsEarned(int guildId, string weekKey)
        {
            try
            {
                const string sql = "SELECT points_earned FROM guild_weekly_points WHERE guild_id = @guildId AND week_key = @weekKey;";
                var connection = guildDatabase.Connection;
                using var command = new SqliteCommand(sql, connection);
                command.Parameters.AddWithValue("@guildId", guildId);
                command.Parameters.AddWithValue("@weekKey", weekKey);

                var result = command.ExecuteScalar();
                return result != null ? Convert.ToInt32(result) : 0;
            }
            catch (Exception ex)
            {
                serverApi.Logger.Error($"[GuildWeeklyPointsRepository] Failed to get weekly points for guild {guildId}, week {weekKey}: {ex.Message}");
                return 0;
            }
        }

        public bool AddWeeklyPoints(int guildId, string weekKey, int points, long weekStartUnix)
        {
            try
            {
                const string sql = @"
                    INSERT INTO guild_weekly_points (guild_id, week_key, points_earned, week_start_unix)
                    VALUES (@guildId, @weekKey, @points, @weekStartUnix)
                    ON CONFLICT(guild_id, week_key) DO UPDATE SET points_earned = points_earned + @points;";

                var connection = guildDatabase.Connection;
                using var command = new SqliteCommand(sql, connection);
                command.Parameters.AddWithValue("@guildId", guildId);
                command.Parameters.AddWithValue("@weekKey", weekKey);
                command.Parameters.AddWithValue("@points", points);
                command.Parameters.AddWithValue("@weekStartUnix", weekStartUnix);

                command.ExecuteNonQuery();
                return true;
            }
            catch (Exception ex)
            {
                serverApi.Logger.Error($"[GuildWeeklyPointsRepository] Failed to add weekly points for guild {guildId}, week {weekKey}: {ex.Message}");
                return false;
            }
        }

        public int CleanupOldWeeklyData(string cutoffWeekKey)
        {
            try
            {
                const string sql = "DELETE FROM guild_weekly_points WHERE week_key < @cutoffWeekKey;";
                var connection = guildDatabase.Connection;
                using var command = new SqliteCommand(sql, connection);
                command.Parameters.AddWithValue("@cutoffWeekKey", cutoffWeekKey);

                return command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                serverApi.Logger.Error($"[GuildWeeklyPointsRepository] Failed to cleanup old weekly data before week {cutoffWeekKey}: {ex.Message}");
                return 0;
            }
        }

        public Dictionary<string, int> GetWeeklyPointsHistory(int guildId, int numberOfWeeks)
        {
            var history = new Dictionary<string, int>();

            try
            {
                const string sql = @"
                    SELECT week_key, points_earned
                    FROM guild_weekly_points
                    WHERE guild_id = @guildId
                    ORDER BY week_key DESC
                    LIMIT @limit;";

                var connection = guildDatabase.Connection;
                using var command = new SqliteCommand(sql, connection);
                command.Parameters.AddWithValue("@guildId", guildId);
                command.Parameters.AddWithValue("@limit", numberOfWeeks);

                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var weekKey = reader.GetString(0);
                    var pointsEarned = reader.GetInt32(1);
                    history[weekKey] = pointsEarned;
                }
            }
            catch (Exception ex)
            {
                serverApi.Logger.Error($"[GuildWeeklyPointsRepository] Failed to get weekly points history for guild {guildId}: {ex.Message}");
            }

            return history;
        }

        public bool WouldExceedWeeklyLimit(int guildId, string weekKey, int limit, int pointsToAdd)
        {
            if (limit <= 0) return false;

            var currentPoints = GetWeeklyPointsEarned(guildId, weekKey);
            return (currentPoints + pointsToAdd) > limit;
        }
    }
}
