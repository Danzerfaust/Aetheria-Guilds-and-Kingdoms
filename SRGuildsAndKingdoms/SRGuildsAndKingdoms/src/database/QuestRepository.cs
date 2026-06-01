using Microsoft.Data.Sqlite;
using SRGuildsAndKingdoms.src.quests;
using System;
using System.Collections.Generic;
using Vintagestory.API.Server;

namespace SRGuildsAndKingdoms.src.database
{
    /// <summary>
    /// Repository for guild quest operations
    /// </summary>
    public class QuestRepository(ICoreServerAPI serverApi, GuildDatabase database)
    {
        private readonly ICoreServerAPI serverApi = serverApi;
        private readonly GuildDatabase database = database;

        #region Quest Definition CRUD

        /// <summary>
        /// Creates a new quest definition
        /// </summary>
        /// <returns>The ID of the created quest</returns>
        public int CreateQuest(Quest quest)
        {
            ArgumentNullException.ThrowIfNull(quest);

            const string sql = @"
                INSERT INTO guild_quests (recurrence_type, title, description, requirements, rewards, starts_at, expires_at, uses_ingame_time, repeat, created_at, rank)
                VALUES (@recurrenceType, @title, @description, @requirements, @rewards, @startsAt, @expiresAt, @usesIngameTime, @repeat, @createdAt, @rank);
                SELECT last_insert_rowid();";

            try
            {
                using var command = new SqliteCommand(sql, database.Connection);
                command.Parameters.AddWithValue("@recurrenceType", quest.RecurrenceType.ToString().ToLowerInvariant());
                command.Parameters.AddWithValue("@title", quest.Title);
                command.Parameters.AddWithValue("@description", quest.Description);
                command.Parameters.AddWithValue("@requirements", quest.SerializeRequirements());
                command.Parameters.AddWithValue("@rewards", quest.SerializeRewards());
                command.Parameters.AddWithValue("@startsAt", quest.StartsAt);
                command.Parameters.AddWithValue("@expiresAt", quest.ExpiresAt);
                command.Parameters.AddWithValue("@usesIngameTime", quest.UsesIngameTime ? 1 : 0);
                command.Parameters.AddWithValue("@repeat", quest.Repeat ? 1 : 0);
                command.Parameters.AddWithValue("@createdAt", DateTimeOffset.Now.ToUnixTimeSeconds());
                command.Parameters.AddWithValue("@rank", quest.Rank);

                quest.Id = Convert.ToInt32(command.ExecuteScalar());
                serverApi.Logger.Debug($"[QuestRepository] Created quest '{quest.Title}' with ID {quest.Id}");
                return quest.Id;
            }
            catch (Exception ex)
            {
                serverApi.Logger.Error($"[QuestRepository] Failed to create quest: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Gets a quest by ID
        /// </summary>
        public Quest? GetQuest(int questId)
        {
            const string sql = @"
                SELECT id, recurrence_type, title, description, requirements, rewards, starts_at, expires_at, uses_ingame_time, repeat, created_at, rank
                FROM guild_quests
                WHERE id = @questId;";

            try
            {
                using var command = new SqliteCommand(sql, database.Connection);
                command.Parameters.AddWithValue("@questId", questId);

                using var reader = command.ExecuteReader();
                if (reader.Read())
                {
                    return ReadQuest(reader);
                }
                return null;
            }
            catch (Exception ex)
            {
                serverApi.Logger.Error($"[QuestRepository] Failed to get quest {questId}: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Updates an existing quest definition (only allowed before starts_at)
        /// </summary>
        public bool UpdateQuest(Quest quest)
        {
            ArgumentNullException.ThrowIfNull(quest);

            const string sql = @"
                UPDATE guild_quests
                SET recurrence_type = @recurrenceType,
                    title = @title,
                    description = @description,
                    requirements = @requirements,
                    rewards = @rewards,
                    starts_at = @startsAt,
                    expires_at = @expiresAt,
                    uses_ingame_time = @usesIngameTime,
                    repeat = @repeat,
                    rank = @rank
                WHERE id = @questId;";

            try
            {
                using var command = new SqliteCommand(sql, database.Connection);
                command.Parameters.AddWithValue("@questId", quest.Id);
                command.Parameters.AddWithValue("@recurrenceType", quest.RecurrenceType.ToString().ToLowerInvariant());
                command.Parameters.AddWithValue("@title", quest.Title);
                command.Parameters.AddWithValue("@description", quest.Description);
                command.Parameters.AddWithValue("@requirements", quest.SerializeRequirements());
                command.Parameters.AddWithValue("@rewards", quest.SerializeRewards());
                command.Parameters.AddWithValue("@startsAt", quest.StartsAt);
                command.Parameters.AddWithValue("@expiresAt", quest.ExpiresAt);
                command.Parameters.AddWithValue("@usesIngameTime", quest.UsesIngameTime ? 1 : 0);
                command.Parameters.AddWithValue("@repeat", quest.Repeat ? 1 : 0);
                command.Parameters.AddWithValue("@rank", quest.Rank);

                int affected = command.ExecuteNonQuery();
                return affected > 0;
            }
            catch (Exception ex)
            {
                serverApi.Logger.Error($"[QuestRepository] Failed to update quest {quest.Id}: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Deletes a quest definition and all associated player progress
        /// </summary>
        public bool DeleteQuest(int questId)
        {
            const string sql = "DELETE FROM guild_quests WHERE id = @questId;";

            try
            {
                using var command = new SqliteCommand(sql, database.Connection);
                command.Parameters.AddWithValue("@questId", questId);

                int affected = command.ExecuteNonQuery();
                if (affected > 0)
                {
                    serverApi.Logger.Debug($"[QuestRepository] Deleted quest {questId}");
                }
                return affected > 0;
            }
            catch (Exception ex)
            {
                serverApi.Logger.Error($"[QuestRepository] Failed to delete quest {questId}: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Gets ALL quests from the database (active, expired, and future)
        /// Used for admin quest management
        /// </summary>
        public List<Quest> GetAllQuests()
        {
            const string sql = @"
                SELECT id, recurrence_type, title, description, requirements, rewards, starts_at, expires_at, uses_ingame_time, repeat, created_at, rank
                FROM guild_quests
                ORDER BY created_at DESC;";

            return ExecuteQuestQuery(sql);
        }

        #endregion

        #region Quest Queries

        /// <summary>
        /// Gets all active quests (started and not expired) for real-world time quests
        /// </summary>
        /// <param name="currentDate">Current date to check against (uses Eastern Time if null)</param>
        public List<Quest> GetActiveRealTimeQuests(DateTime? currentDate = null)
        {
            var date = currentDate ?? QuestTimeHelper.TodayEastern;
            var dateStr = QuestPeriodKeyGenerator.FormatDate(date);

            const string sql = @"
                SELECT id, recurrence_type, title, description, requirements, rewards, starts_at, expires_at, uses_ingame_time, repeat, created_at, rank
                FROM guild_quests
                WHERE uses_ingame_time = 0
                  AND starts_at <= @currentDate
                  AND expires_at >= @currentDate
                ORDER BY recurrence_type, expires_at;";

            return ExecuteQuestQuery(sql, cmd => cmd.Parameters.AddWithValue("@currentDate", dateStr));
        }

        /// <summary>
        /// Gets all active quests for in-game time
        /// </summary>
        /// <param name="currentIngameDate">Current in-game date</param>
        public List<Quest> GetActiveIngameTimeQuests(GameDate currentIngameDate)
        {
            var dateStr = QuestPeriodKeyGenerator.FormatInGameDate(currentIngameDate);

            // If year is 1 (representing VS year 0), convert to "0000" for database comparison
            // This is needed because database stores IGT dates as "0000-MM-DD" but DateTime uses year 1
            if (currentIngameDate.Year == 1)
            {
                dateStr = "0000" + dateStr[4..];
            }

            const string sql = @"
                SELECT id, recurrence_type, title, description, requirements, rewards, starts_at, expires_at, uses_ingame_time, repeat, created_at, rank
                FROM guild_quests
                WHERE uses_ingame_time = 1
                  AND starts_at <= @currentDate
                  AND expires_at >= @currentDate
                ORDER BY recurrence_type, expires_at;";

            return ExecuteQuestQuery(sql, cmd => cmd.Parameters.AddWithValue("@currentDate", dateStr));
        }

        /// <summary>
        /// Gets all active quests (both real-time and in-game time)
        /// </summary>
        public List<Quest> GetAllActiveQuests(GameDate? currentIngameDate = null)
        {
            var quests = GetActiveRealTimeQuests();

            if (currentIngameDate != null)
            {
                quests.AddRange(GetActiveIngameTimeQuests((GameDate)currentIngameDate));
            }

            return quests;
        }

        /// <summary>
        /// Gets quests by recurrence type
        /// </summary>
        public List<Quest> GetQuestsByRecurrenceType(QuestRecurrenceType recurrenceType)
        {
            const string sql = @"
                SELECT id, recurrence_type, title, description, requirements, rewards, starts_at, expires_at, uses_ingame_time, repeat, created_at, rank
                FROM guild_quests
                WHERE recurrence_type = @recurrenceType
                ORDER BY starts_at DESC;";

            return ExecuteQuestQuery(sql, cmd =>
                cmd.Parameters.AddWithValue("@recurrenceType", recurrenceType.ToString().ToLowerInvariant()));
        }

        /// <summary>
        /// Gets quests available for a player to start (active, not already started, not period-locked)
        /// </summary>
        public List<Quest> GetAvailableQuestsForPlayer(string playerUid, GameDate? currentIngameDate = null)
        {
            var allActive = GetAllActiveQuests(currentIngameDate);
            var available = new List<Quest>();

            foreach (var quest in allActive)
            {
                available.Add(quest);
            }

            return available;
        }

        /// <summary>
        /// Checks if a specific quest is available for a player to start
        /// </summary>
        public bool IsQuestAvailableForPlayer(string playerUid, Quest quest)
        {
            // Check if player already has this specific quest active
            if (HasActiveQuest(playerUid, quest.Id))
                return false;

            var periodKey = quest.GeneratePeriodKey();

            // Weeklies are always available as long as they are not active and they have not been completed for the current period
            if (quest.RecurrenceType == QuestRecurrenceType.Weekly && !WasQuestCompletedInPeriod(playerUid, quest.Id, periodKey)) return true;

            // Check if player is period-locked for this recurrence type
            if (IsPlayerPeriodLocked(playerUid, quest.RecurrenceType, periodKey))
                return false;

            return true;
        }

        /// <summary>
        /// Gets all expired repeating quests that need to be renewed
        /// </summary>
        /// <param name="currentDate">Current date to check against (uses Eastern Time if null)</param>
        public List<Quest> GetExpiredRepeatingQuests(DateTime? currentDate = null)
        {
            var date = currentDate ?? QuestTimeHelper.TodayEastern;
            var dateStr = QuestPeriodKeyGenerator.FormatDate(date);

            const string sql = @"
                SELECT id, recurrence_type, title, description, requirements, rewards, starts_at, expires_at, uses_ingame_time, repeat, created_at, rank
                FROM guild_quests
                WHERE repeat = 1
                  AND expires_at < @currentDate
                ORDER BY expires_at ASC;";

            return ExecuteQuestQuery(sql, cmd => cmd.Parameters.AddWithValue("@currentDate", dateStr));
        }

        #endregion

        #region Player Quest Progress

        /// <summary>
        /// Starts a quest for a player (inserts into guild_member_quests)
        /// </summary>
        /// <returns>True if quest was started successfully</returns>
        public bool StartQuest(string playerUid, int questId)
        {
            var quest = GetQuest(questId);
            if (quest == null)
            {
                serverApi.Logger.Warning($"[QuestRepository] Cannot start quest {questId}: quest not found");
                return false;
            }

            // Verify quest is available for this player
            if (!IsQuestAvailableForPlayer(playerUid, quest))
            {
                serverApi.Logger.Warning($"[QuestRepository] Quest {questId} is not available for player {playerUid}");
                return false;
            }

            // Generate period key for this quest
            var periodKey = quest.GeneratePeriodKey();

            const string sql = @"
                INSERT INTO guild_member_quests (player_uid, quest_id, status, progress, recurrence_type, period_key, started_at)
                VALUES (@playerUid, @questId, 'active', @progress, @recurrenceType, @periodKey, @startedAt);";

            try
            {
                using var command = new SqliteCommand(sql, database.Connection);
                command.Parameters.AddWithValue("@playerUid", playerUid);
                command.Parameters.AddWithValue("@questId", questId);
                command.Parameters.AddWithValue("@progress", "{}");
                command.Parameters.AddWithValue("@recurrenceType", quest.RecurrenceType.ToString().ToLowerInvariant());
                command.Parameters.AddWithValue("@periodKey", periodKey);
                command.Parameters.AddWithValue("@startedAt", DateTimeOffset.Now.ToUnixTimeSeconds());

                command.ExecuteNonQuery();
                serverApi.Logger.Debug($"[QuestRepository] Player {playerUid} started quest {questId} with period key {periodKey}");
                return true;
            }
            catch (SqliteException ex) when (ex.SqliteErrorCode == 19) // UNIQUE constraint
            {
                serverApi.Logger.Warning($"[QuestRepository] Player {playerUid} already has quest {questId} or is period-locked");
                return false;
            }
            catch (Exception ex)
            {
                serverApi.Logger.Error($"[QuestRepository] Failed to start quest: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Gets a player's progress on a specific quest ID and period key
        /// </summary>
        public PlayerQuestProgress? GetPlayerQuestProgress(string playerUid, int questId, string periodKey)
        {
            const string sql = @"
                SELECT player_uid, quest_id, status, progress, recurrence_type, period_key, started_at, completed_at
                FROM guild_member_quests
                WHERE player_uid = @playerUid AND quest_id = @questId AND period_key = @periodKey;";

            try
            {
                using var command = new SqliteCommand(sql, database.Connection);
                command.Parameters.AddWithValue("@playerUid", playerUid);
                command.Parameters.AddWithValue("@questId", questId);
                command.Parameters.AddWithValue("@periodKey", periodKey);

                using var reader = command.ExecuteReader();
                if (reader.Read())
                {
                    return ReadPlayerQuestProgress(reader);
                }
                return null;
            }
            catch (Exception ex)
            {
                serverApi.Logger.Error($"[QuestRepository] Failed to get player quest progress: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Gets all active quests for a player (excludes expired quests)
        /// </summary>
        /// <param name="playerUid">The player's UID</param>
        /// <param name="currentIngameDate">Current in-game date for filtering IGT quests (null = skip IGT expiration check)</param>
        public List<PlayerQuestProgress> GetPlayerActiveQuests(string playerUid, GameDate? currentIngameDate = null)
        {
            var currentDateStr = QuestPeriodKeyGenerator.FormatDate(QuestTimeHelper.TodayEastern);

            // Build in-game date string if provided
            string? ingameDateStr = null;
            if (currentIngameDate.HasValue)
            {
                ingameDateStr = QuestPeriodKeyGenerator.FormatInGameDate(currentIngameDate.Value);
                // If year is 1 (representing VS year 0), convert to "0000" for database comparison
                if (currentIngameDate.Value.Year == 1)
                {
                    ingameDateStr = "0000" + ingameDateStr[4..];
                }
            }

            // Filter out expired quests based on their time mode
            const string sql = @"
                SELECT gmq.player_uid, gmq.quest_id, gmq.status, gmq.progress, gmq.recurrence_type, gmq.period_key, gmq.started_at, gmq.completed_at
                FROM guild_member_quests gmq
                INNER JOIN guild_quests gq ON gmq.quest_id = gq.id
                WHERE gmq.player_uid = @playerUid 
                  AND gmq.status = 'active'
                  AND (
                      (gq.uses_ingame_time = 0 AND gq.expires_at >= @currentDate)
                      OR (gq.uses_ingame_time = 1 AND (@ingameDate IS NULL OR gq.expires_at >= @ingameDate))
                  );";

            return ExecutePlayerProgressQuery(sql, cmd =>
            {
                cmd.Parameters.AddWithValue("@playerUid", playerUid);
                cmd.Parameters.AddWithValue("@currentDate", currentDateStr);
                cmd.Parameters.AddWithValue("@ingameDate", ingameDateStr ?? (object)DBNull.Value);
            });
        }

        /// <summary>
        /// Gets all completed quests for a player
        /// </summary>
        public List<PlayerQuestProgress> GetPlayerCompletedQuests(string playerUid)
        {
            const string sql = @"
                SELECT player_uid, quest_id, status, progress, recurrence_type, period_key, started_at, completed_at
                FROM guild_member_quests
                WHERE player_uid = @playerUid AND status = 'completed'
                ORDER BY completed_at DESC;";

            return ExecutePlayerProgressQuery(sql, cmd =>
            {
                cmd.Parameters.AddWithValue("@playerUid", playerUid);
            });
        }

        /// <summary>
        /// Updates a player's quest progress
        /// </summary>
        public bool UpdatePlayerQuestProgress(PlayerQuestProgress progress)
        {
            ArgumentNullException.ThrowIfNull(progress);

            const string sql = @"
                UPDATE guild_member_quests
                SET progress = @progress
                WHERE player_uid = @playerUid AND quest_id = @questId AND status = 'active';";

            try
            {
                using var command = new SqliteCommand(sql, database.Connection);
                command.Parameters.AddWithValue("@playerUid", progress.PlayerUid);
                command.Parameters.AddWithValue("@questId", progress.QuestId);
                command.Parameters.AddWithValue("@progress", progress.SerializeProgress());

                int affected = command.ExecuteNonQuery();
                return affected > 0;
            }
            catch (Exception ex)
            {
                serverApi.Logger.Error($"[QuestRepository] Failed to update quest progress: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Completes a quest for a player (updates status to completed)
        /// </summary>
        /// <returns>True if quest was completed successfully</returns>
        public bool CompleteQuest(string playerUid, int questId)
        {
            const string sql = @"
                UPDATE guild_member_quests
                SET status = 'completed',
                    completed_at = @completedAt
                WHERE player_uid = @playerUid AND quest_id = @questId AND status = 'active';";

            try
            {
                using var command = new SqliteCommand(sql, database.Connection);
                command.Parameters.AddWithValue("@playerUid", playerUid);
                command.Parameters.AddWithValue("@questId", questId);
                command.Parameters.AddWithValue("@completedAt", DateTimeOffset.Now.ToUnixTimeSeconds());

                int affected = command.ExecuteNonQuery();
                if (affected > 0)
                {
                    serverApi.Logger.Debug($"[QuestRepository] Player {playerUid} completed quest {questId}");
                }
                return affected > 0;
            }
            catch (Exception ex)
            {
                serverApi.Logger.Error($"[QuestRepository] Failed to complete quest: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Abandons a quest for a player (deletes the row)
        /// </summary>
        public bool AbandonQuest(string playerUid, int questId)
        {
            const string sql = @"
                DELETE FROM guild_member_quests
                WHERE player_uid = @playerUid AND quest_id = @questId AND status = 'active';";

            try
            {
                using var command = new SqliteCommand(sql, database.Connection);
                command.Parameters.AddWithValue("@playerUid", playerUid);
                command.Parameters.AddWithValue("@questId", questId);

                int affected = command.ExecuteNonQuery();
                if (affected > 0)
                {
                    serverApi.Logger.Debug($"[QuestRepository] Player {playerUid} abandoned quest {questId}");
                }
                return affected > 0;
            }
            catch (Exception ex)
            {
                serverApi.Logger.Error($"[QuestRepository] Failed to abandon quest: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Removes all quest progress for a player by period key (admin command utility)
        /// </summary>
        /// <returns>Number of rows deleted</returns>
        public int RemovePlayerQuestProgressByPeriodKey(string playerUid, string periodKey)
        {
            const string sql = @"
                DELETE FROM guild_member_quests
                WHERE player_uid = @playerUid AND period_key = @periodKey;";

            try
            {
                using var command = new SqliteCommand(sql, database.Connection);
                command.Parameters.AddWithValue("@playerUid", playerUid);
                command.Parameters.AddWithValue("@periodKey", periodKey);

                int affected = command.ExecuteNonQuery();
                if (affected > 0)
                {
                    serverApi.Logger.Notification($"[QuestRepository] Removed {affected} quest progress entries for player {playerUid} with period key {periodKey}");
                }
                return affected;
            }
            catch (Exception ex)
            {
                serverApi.Logger.Error($"[QuestRepository] Failed to remove quest progress: {ex.Message}");
                throw;
            }
        }

        #endregion

        #region Period Locking Queries

        /// <summary>
        /// Checks if a player already has an active instance of a specific quest
        /// </summary>
        public bool HasActiveQuest(string playerUid, int questId)
        {
            const string sql = @"
                SELECT 1 FROM guild_member_quests
                WHERE player_uid = @playerUid AND quest_id = @questId AND status = 'active'
                LIMIT 1;";

            try
            {
                using var command = new SqliteCommand(sql, database.Connection);
                command.Parameters.AddWithValue("@playerUid", playerUid);
                command.Parameters.AddWithValue("@questId", questId);

                return command.ExecuteScalar() != null;
            }
            catch (Exception ex)
            {
                serverApi.Logger.Error($"[QuestRepository] Failed to check active quest: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Checks if a player is period-locked for a specific recurrence type and period
        /// </summary>
        public bool IsPlayerPeriodLocked(string playerUid, QuestRecurrenceType recurrenceType, string periodKey)
        {
            const string sql = @"
                SELECT 1 FROM guild_member_quests
                WHERE player_uid = @playerUid 
                  AND recurrence_type = @recurrenceType 
                  AND period_key = @periodKey
                LIMIT 1;";

            try
            {
                using var command = new SqliteCommand(sql, database.Connection);
                command.Parameters.AddWithValue("@playerUid", playerUid);
                command.Parameters.AddWithValue("@recurrenceType", recurrenceType.ToString().ToLowerInvariant());
                command.Parameters.AddWithValue("@periodKey", periodKey);

                return command.ExecuteScalar() != null;
            }
            catch (Exception ex)
            {
                serverApi.Logger.Error($"[QuestRepository] Failed to check period lock: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Gets all period keys a player has completed for a given recurrence type
        /// </summary>
        public List<string> GetPlayerCompletedPeriodKeys(string playerUid, QuestRecurrenceType recurrenceType)
        {
            const string sql = @"
                SELECT DISTINCT period_key FROM guild_member_quests
                WHERE player_uid = @playerUid 
                  AND recurrence_type = @recurrenceType
                  AND status = 'completed'
                  AND period_key IS NOT NULL;";

            var keys = new List<string>();

            try
            {
                using var command = new SqliteCommand(sql, database.Connection);
                command.Parameters.AddWithValue("@playerUid", playerUid);
                command.Parameters.AddWithValue("@recurrenceType", recurrenceType.ToString().ToLowerInvariant());

                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    keys.Add(reader.GetString(0));
                }
                return keys;
            }
            catch (Exception ex)
            {
                serverApi.Logger.Error($"[QuestRepository] Failed to get completed period keys: {ex.Message}");
                return keys;
            }
        }

        /// <summary>
        /// Gets all completed quest IDs with their period keys for a player
        /// </summary>
        public List<(int questId, string periodKey)> GetPlayerCompletedQuestsByPeriod(string playerUid)
        {
            const string sql = @"
                SELECT quest_id, period_key FROM guild_member_quests
                WHERE player_uid = @playerUid 
                  AND status = 'completed'
                  AND period_key IS NOT NULL;";

            var completedQuests = new List<(int, string)>();

            try
            {
                using var command = new SqliteCommand(sql, database.Connection);
                command.Parameters.AddWithValue("@playerUid", playerUid);

                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    int questId = reader.GetInt32(0);
                    string periodKey = reader.GetString(1);
                    completedQuests.Add((questId, periodKey));
                }
                return completedQuests;
            }
            catch (Exception ex)
            {
                serverApi.Logger.Error($"[QuestRepository] Failed to get completed quests by period: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Checks if a player completed a specific quest in a specific period
        /// </summary>
        public bool WasQuestCompletedInPeriod(string playerUid, int questId, string periodKey)
        {
            const string sql = @"
                SELECT 1 FROM guild_member_quests
                WHERE player_uid = @playerUid 
                  AND quest_id = @questId
                  AND period_key = @periodKey
                  AND status = 'completed'
                LIMIT 1;";

            try
            {
                using var command = new SqliteCommand(sql, database.Connection);
                command.Parameters.AddWithValue("@playerUid", playerUid);
                command.Parameters.AddWithValue("@questId", questId);
                command.Parameters.AddWithValue("@periodKey", periodKey);

                return command.ExecuteScalar() != null;
            }
            catch (Exception ex)
            {
                serverApi.Logger.Error($"[QuestRepository] Failed to check quest completion in period: {ex.Message}");
                throw;
            }
        }

        #endregion

        #region Cleanup Operations

        /// <summary>
        /// Cleans up expired quest progress (deletes active quests for expired quest definitions)
        /// Does NOT delete the quest definitions themselves
        /// </summary>
        /// <param name="currentDate">Current real-world date (null = use Eastern Time)</param>
        /// <param name="currentIngameDate">Current in-game date (null = skip in-game time quests)</param>
        /// <returns>Number of rows deleted</returns>
        public int CleanupExpiredQuestProgress(DateTime? currentDate = null, GameDate? currentIngameDate = null)
        {
            var realDate = currentDate ?? QuestTimeHelper.TodayEastern;
            var realDateStr = QuestPeriodKeyGenerator.FormatDate(realDate);

            int totalDeleted = 0;

            // Cleanup real-time quests
            const string sqlRealTime = @"
                DELETE FROM guild_member_quests
                WHERE status = 'active'
                  AND quest_id IN (
                      SELECT id FROM guild_quests
                      WHERE uses_ingame_time = 0 AND expires_at < @currentDate
                  );";

            try
            {
                using (var command = new SqliteCommand(sqlRealTime, database.Connection))
                {
                    command.Parameters.AddWithValue("@currentDate", realDateStr);
                    totalDeleted += command.ExecuteNonQuery();
                }

                // Cleanup in-game time quests if date provided
                if (currentIngameDate.HasValue)
                {
                    var ingameDateStr = QuestPeriodKeyGenerator.FormatInGameDate(currentIngameDate.Value);

                    // If year is 1 (representing VS year 0), convert to "0000" for database comparison
                    if (currentIngameDate.Value.Year == 1)
                    {
                        ingameDateStr = "0000" + ingameDateStr[4..];
                    }

                    const string sqlIngameTime = @"
                        DELETE FROM guild_member_quests
                        WHERE status = 'active'
                          AND quest_id IN (
                              SELECT id FROM guild_quests
                              WHERE uses_ingame_time = 1 AND expires_at < @currentDate
                          );";

                    using var command = new SqliteCommand(sqlIngameTime, database.Connection);
                    command.Parameters.AddWithValue("@currentDate", ingameDateStr);
                    totalDeleted += command.ExecuteNonQuery();
                }

                if (totalDeleted > 0)
                {
                    serverApi.Logger.Notification($"[QuestRepository] Cleaned up {totalDeleted} expired quest progress entries");
                }

                return totalDeleted;
            }
            catch (Exception ex)
            {
                serverApi.Logger.Error($"[QuestRepository] Failed to cleanup expired quests: {ex.Message}");
                throw;
            }
        }

        #endregion

        #region Private Helpers

        private List<Quest> ExecuteQuestQuery(string sql, Action<SqliteCommand>? configureCommand = null)
        {
            var quests = new List<Quest>();

            try
            {
                using var command = new SqliteCommand(sql, database.Connection);
                configureCommand?.Invoke(command);

                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    quests.Add(ReadQuest(reader));
                }
            }
            catch (Exception ex)
            {
                serverApi.Logger.Error($"[QuestRepository] Quest query failed: {ex.Message}");
                throw;
            }

            return quests;
        }

        private List<PlayerQuestProgress> ExecutePlayerProgressQuery(string sql, Action<SqliteCommand>? configureCommand = null)
        {
            var progressList = new List<PlayerQuestProgress>();

            try
            {
                using var command = new SqliteCommand(sql, database.Connection);
                configureCommand?.Invoke(command);

                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    progressList.Add(ReadPlayerQuestProgress(reader));
                }
            }
            catch (Exception ex)
            {
                serverApi.Logger.Error($"[QuestRepository] Player progress query failed: {ex.Message}");
                throw;
            }

            return progressList;
        }

        private static Quest ReadQuest(SqliteDataReader reader)
        {
            return new Quest
            {
                Id = reader.GetInt32(0),
                RecurrenceType = Enum.Parse<QuestRecurrenceType>(reader.GetString(1), ignoreCase: true),
                Title = reader.GetString(2),
                Description = reader.GetString(3),
                Objectives = Quest.DeserializeRequirements(reader.GetString(4)),
                Rewards = Quest.DeserializeRewards(reader.GetString(5)),
                StartsAt = reader.GetString(6),
                ExpiresAt = reader.GetString(7),
                UsesIngameTime = reader.GetInt32(8) == 1,
                Repeat = reader.GetInt32(9) == 1,
                CreatedAt = reader.GetInt64(10),
                Rank = reader.GetString(11)
            };
        }

        private static PlayerQuestProgress ReadPlayerQuestProgress(SqliteDataReader reader)
        {
            return new PlayerQuestProgress
            {
                PlayerUid = reader.GetString(0),
                QuestId = reader.GetInt32(1),
                Status = Enum.Parse<PlayerQuestStatus>(reader.GetString(2), ignoreCase: true),
                ObjectiveProgress = PlayerQuestProgress.DeserializeProgress(reader.IsDBNull(3) ? null : reader.GetString(3)),
                RecurrenceType = Enum.Parse<QuestRecurrenceType>(reader.GetString(4), ignoreCase: true),
                PeriodKey = reader.IsDBNull(5) ? null : reader.GetString(5),
                StartedAt = reader.GetInt64(6),
                CompletedAt = reader.IsDBNull(7) ? null : reader.GetInt64(7)
            };
        }

        #endregion
    }
}
