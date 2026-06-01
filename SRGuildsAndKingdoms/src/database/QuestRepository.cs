using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Microsoft.Data.Sqlite;
using SRGuildsAndKingdoms.src.quests;
using Vintagestory.API.Common;
using Vintagestory.API.Server;

namespace SRGuildsAndKingdoms.src.database
{
	// Token: 0x020000BE RID: 190
	[NullableContext(1)]
	[Nullable(0)]
	public class QuestRepository
	{
		// Token: 0x06000900 RID: 2304 RVA: 0x00040A3B File Offset: 0x0003EC3B
		public QuestRepository(ICoreServerAPI serverApi, GuildDatabase database)
		{
		}

		// Token: 0x06000901 RID: 2305 RVA: 0x00040A54 File Offset: 0x0003EC54
		public int CreateQuest(Quest quest)
		{
			ArgumentNullException.ThrowIfNull(quest, "quest");
			int id;
			try
			{
				using (SqliteCommand command = new SqliteCommand("\n                INSERT INTO guild_quests (recurrence_type, title, description, requirements, rewards, starts_at, expires_at, uses_ingame_time, repeat, created_at)\n                VALUES (@recurrenceType, @title, @description, @requirements, @rewards, @startsAt, @expiresAt, @usesIngameTime, @repeat, @createdAt);\n                SELECT last_insert_rowid();", this.database.Connection))
				{
					command.Parameters.AddWithValue("@recurrenceType", quest.RecurrenceType.ToString().ToLowerInvariant());
					command.Parameters.AddWithValue("@title", quest.Title);
					command.Parameters.AddWithValue("@description", quest.Description);
					command.Parameters.AddWithValue("@requirements", quest.SerializeRequirements());
					command.Parameters.AddWithValue("@rewards", quest.SerializeRewards());
					command.Parameters.AddWithValue("@startsAt", quest.StartsAt);
					command.Parameters.AddWithValue("@expiresAt", quest.ExpiresAt);
					command.Parameters.AddWithValue("@usesIngameTime", (quest.UsesIngameTime > false) ? 1 : 0);
					command.Parameters.AddWithValue("@repeat", (quest.Repeat > false) ? 1 : 0);
					command.Parameters.AddWithValue("@createdAt", DateTimeOffset.Now.ToUnixTimeSeconds());
					quest.Id = Convert.ToInt32(command.ExecuteScalar());
					ILogger logger = this.serverApi.Logger;
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(43, 2);
					defaultInterpolatedStringHandler.AppendLiteral("[QuestRepository] Created quest '");
					defaultInterpolatedStringHandler.AppendFormatted(quest.Title);
					defaultInterpolatedStringHandler.AppendLiteral("' with ID ");
					defaultInterpolatedStringHandler.AppendFormatted<int>(quest.Id);
					logger.Debug(defaultInterpolatedStringHandler.ToStringAndClear());
					id = quest.Id;
				}
			}
			catch (Exception ex)
			{
				this.serverApi.Logger.Error("[QuestRepository] Failed to create quest: " + ex.Message);
				throw;
			}
			return id;
		}

		// Token: 0x06000902 RID: 2306 RVA: 0x00040C6C File Offset: 0x0003EE6C
		[NullableContext(2)]
		public Quest GetQuest(int questId)
		{
			Quest result;
			try
			{
				using (SqliteCommand command = new SqliteCommand("\n                SELECT id, recurrence_type, title, description, requirements, rewards, starts_at, expires_at, uses_ingame_time, repeat, created_at\n                FROM guild_quests\n                WHERE id = @questId;", this.database.Connection))
				{
					command.Parameters.AddWithValue("@questId", questId);
					using (SqliteDataReader reader = command.ExecuteReader())
					{
						if (reader.Read())
						{
							result = QuestRepository.ReadQuest(reader);
						}
						else
						{
							result = null;
						}
					}
				}
			}
			catch (Exception ex)
			{
				ILogger logger = this.serverApi.Logger;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(40, 2);
				defaultInterpolatedStringHandler.AppendLiteral("[QuestRepository] Failed to get quest ");
				defaultInterpolatedStringHandler.AppendFormatted<int>(questId);
				defaultInterpolatedStringHandler.AppendLiteral(": ");
				defaultInterpolatedStringHandler.AppendFormatted(ex.Message);
				logger.Error(defaultInterpolatedStringHandler.ToStringAndClear());
				throw;
			}
			return result;
		}

		// Token: 0x06000903 RID: 2307 RVA: 0x00040D50 File Offset: 0x0003EF50
		public bool UpdateQuest(Quest quest)
		{
			ArgumentNullException.ThrowIfNull(quest, "quest");
			bool result;
			try
			{
				using (SqliteCommand command = new SqliteCommand("\n                UPDATE guild_quests\n                SET recurrence_type = @recurrenceType,\n                    title = @title,\n                    description = @description,\n                    requirements = @requirements,\n                    rewards = @rewards,\n                    starts_at = @startsAt,\n                    expires_at = @expiresAt,\n                    uses_ingame_time = @usesIngameTime,\n                    repeat = @repeat\n                WHERE id = @questId;", this.database.Connection))
				{
					command.Parameters.AddWithValue("@questId", quest.Id);
					command.Parameters.AddWithValue("@recurrenceType", quest.RecurrenceType.ToString().ToLowerInvariant());
					command.Parameters.AddWithValue("@title", quest.Title);
					command.Parameters.AddWithValue("@description", quest.Description);
					command.Parameters.AddWithValue("@requirements", quest.SerializeRequirements());
					command.Parameters.AddWithValue("@rewards", quest.SerializeRewards());
					command.Parameters.AddWithValue("@startsAt", quest.StartsAt);
					command.Parameters.AddWithValue("@expiresAt", quest.ExpiresAt);
					command.Parameters.AddWithValue("@usesIngameTime", (quest.UsesIngameTime > false) ? 1 : 0);
					command.Parameters.AddWithValue("@repeat", (quest.Repeat > false) ? 1 : 0);
					result = (command.ExecuteNonQuery() > 0);
				}
			}
			catch (Exception ex)
			{
				ILogger logger = this.serverApi.Logger;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(43, 2);
				defaultInterpolatedStringHandler.AppendLiteral("[QuestRepository] Failed to update quest ");
				defaultInterpolatedStringHandler.AppendFormatted<int>(quest.Id);
				defaultInterpolatedStringHandler.AppendLiteral(": ");
				defaultInterpolatedStringHandler.AppendFormatted(ex.Message);
				logger.Error(defaultInterpolatedStringHandler.ToStringAndClear());
				throw;
			}
			return result;
		}

		// Token: 0x06000904 RID: 2308 RVA: 0x00040F30 File Offset: 0x0003F130
		public bool DeleteQuest(int questId)
		{
			bool result;
			try
			{
				using (SqliteCommand command = new SqliteCommand("DELETE FROM guild_quests WHERE id = @questId;", this.database.Connection))
				{
					command.Parameters.AddWithValue("@questId", questId);
					int num = command.ExecuteNonQuery();
					if (num > 0)
					{
						ILogger logger = this.serverApi.Logger;
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(32, 1);
						defaultInterpolatedStringHandler.AppendLiteral("[QuestRepository] Deleted quest ");
						defaultInterpolatedStringHandler.AppendFormatted<int>(questId);
						logger.Debug(defaultInterpolatedStringHandler.ToStringAndClear());
					}
					result = (num > 0);
				}
			}
			catch (Exception ex)
			{
				ILogger logger2 = this.serverApi.Logger;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(43, 2);
				defaultInterpolatedStringHandler2.AppendLiteral("[QuestRepository] Failed to delete quest ");
				defaultInterpolatedStringHandler2.AppendFormatted<int>(questId);
				defaultInterpolatedStringHandler2.AppendLiteral(": ");
				defaultInterpolatedStringHandler2.AppendFormatted(ex.Message);
				logger2.Error(defaultInterpolatedStringHandler2.ToStringAndClear());
				throw;
			}
			return result;
		}

		// Token: 0x06000905 RID: 2309 RVA: 0x00041028 File Offset: 0x0003F228
		public List<Quest> GetAllQuests()
		{
			return this.ExecuteQuestQuery("\n                SELECT id, recurrence_type, title, description, requirements, rewards, starts_at, expires_at, uses_ingame_time, repeat, created_at\n                FROM guild_quests\n                ORDER BY created_at DESC;", null);
		}

		// Token: 0x06000906 RID: 2310 RVA: 0x00041038 File Offset: 0x0003F238
		public List<Quest> GetActiveRealTimeQuests(DateTime? currentDate = null)
		{
			DateTime date = currentDate ?? QuestTimeHelper.TodayEastern;
			string dateStr = QuestPeriodKeyGenerator.FormatDate(date);
			return this.ExecuteQuestQuery("\n                SELECT id, recurrence_type, title, description, requirements, rewards, starts_at, expires_at, uses_ingame_time, repeat, created_at\n                FROM guild_quests\n                WHERE uses_ingame_time = 0\n                  AND starts_at <= @currentDate\n                  AND expires_at >= @currentDate\n                ORDER BY recurrence_type, expires_at;", delegate(SqliteCommand cmd)
			{
				cmd.Parameters.AddWithValue("@currentDate", dateStr);
			});
		}

		// Token: 0x06000907 RID: 2311 RVA: 0x00041088 File Offset: 0x0003F288
		public List<Quest> GetActiveIngameTimeQuests(GameDate currentIngameDate)
		{
			QuestRepository.<>c__DisplayClass9_0 CS$<>8__locals1 = new QuestRepository.<>c__DisplayClass9_0();
			CS$<>8__locals1.dateStr = QuestPeriodKeyGenerator.FormatInGameDate(currentIngameDate);
			if (currentIngameDate.Year == 1)
			{
				QuestRepository.<>c__DisplayClass9_0 CS$<>8__locals2 = CS$<>8__locals1;
				string str = "0000";
				string dateStr = CS$<>8__locals1.dateStr;
				CS$<>8__locals2.dateStr = str + dateStr.Substring(4, dateStr.Length - 4);
			}
			return this.ExecuteQuestQuery("\n                SELECT id, recurrence_type, title, description, requirements, rewards, starts_at, expires_at, uses_ingame_time, repeat, created_at\n                FROM guild_quests\n                WHERE uses_ingame_time = 1\n                  AND starts_at <= @currentDate\n                  AND expires_at >= @currentDate\n                ORDER BY recurrence_type, expires_at;", delegate(SqliteCommand cmd)
			{
				cmd.Parameters.AddWithValue("@currentDate", CS$<>8__locals1.dateStr);
			});
		}

		// Token: 0x06000908 RID: 2312 RVA: 0x000410F0 File Offset: 0x0003F2F0
		public List<Quest> GetAllActiveQuests(GameDate? currentIngameDate = null)
		{
			List<Quest> quests = this.GetActiveRealTimeQuests(null);
			if (currentIngameDate != null)
			{
				quests.AddRange(this.GetActiveIngameTimeQuests(currentIngameDate.Value));
			}
			return quests;
		}

		// Token: 0x06000909 RID: 2313 RVA: 0x0004112C File Offset: 0x0003F32C
		public List<Quest> GetQuestsByRecurrenceType(QuestRecurrenceType recurrenceType)
		{
			return this.ExecuteQuestQuery("\n                SELECT id, recurrence_type, title, description, requirements, rewards, starts_at, expires_at, uses_ingame_time, repeat, created_at\n                FROM guild_quests\n                WHERE recurrence_type = @recurrenceType\n                ORDER BY starts_at DESC;", delegate(SqliteCommand cmd)
			{
				cmd.Parameters.AddWithValue("@recurrenceType", recurrenceType.ToString().ToLowerInvariant());
			});
		}

		// Token: 0x0600090A RID: 2314 RVA: 0x00041160 File Offset: 0x0003F360
		public List<Quest> GetAvailableQuestsForPlayer(string playerUid, GameDate? currentIngameDate = null)
		{
			List<Quest> allActiveQuests = this.GetAllActiveQuests(currentIngameDate);
			List<Quest> available = new List<Quest>();
			foreach (Quest quest in allActiveQuests)
			{
				available.Add(quest);
			}
			return available;
		}

		// Token: 0x0600090B RID: 2315 RVA: 0x000411BC File Offset: 0x0003F3BC
		public bool IsQuestAvailableForPlayer(string playerUid, Quest quest)
		{
			if (this.HasActiveQuest(playerUid, quest.Id))
			{
				return false;
			}
			string periodKey = quest.GeneratePeriodKey();
			return !this.IsPlayerPeriodLocked(playerUid, quest.RecurrenceType, periodKey);
		}

		// Token: 0x0600090C RID: 2316 RVA: 0x000411F4 File Offset: 0x0003F3F4
		public List<Quest> GetExpiredRepeatingQuests(DateTime? currentDate = null)
		{
			DateTime date = currentDate ?? QuestTimeHelper.TodayEastern;
			string dateStr = QuestPeriodKeyGenerator.FormatDate(date);
			return this.ExecuteQuestQuery("\n                SELECT id, recurrence_type, title, description, requirements, rewards, starts_at, expires_at, uses_ingame_time, repeat, created_at\n                FROM guild_quests\n                WHERE repeat = 1\n                  AND expires_at < @currentDate\n                ORDER BY expires_at ASC;", delegate(SqliteCommand cmd)
			{
				cmd.Parameters.AddWithValue("@currentDate", dateStr);
			});
		}

		// Token: 0x0600090D RID: 2317 RVA: 0x00041244 File Offset: 0x0003F444
		public bool StartQuest(string playerUid, int questId)
		{
			Quest quest = this.GetQuest(questId);
			if (quest == null)
			{
				ILogger logger = this.serverApi.Logger;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(54, 1);
				defaultInterpolatedStringHandler.AppendLiteral("[QuestRepository] Cannot start quest ");
				defaultInterpolatedStringHandler.AppendFormatted<int>(questId);
				defaultInterpolatedStringHandler.AppendLiteral(": quest not found");
				logger.Warning(defaultInterpolatedStringHandler.ToStringAndClear());
				return false;
			}
			if (!this.IsQuestAvailableForPlayer(playerUid, quest))
			{
				ILogger logger2 = this.serverApi.Logger;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(53, 2);
				defaultInterpolatedStringHandler2.AppendLiteral("[QuestRepository] Quest ");
				defaultInterpolatedStringHandler2.AppendFormatted<int>(questId);
				defaultInterpolatedStringHandler2.AppendLiteral(" is not available for player ");
				defaultInterpolatedStringHandler2.AppendFormatted(playerUid);
				logger2.Warning(defaultInterpolatedStringHandler2.ToStringAndClear());
				return false;
			}
			string periodKey = quest.GeneratePeriodKey();
			bool result;
			try
			{
				using (SqliteCommand command = new SqliteCommand("\n                INSERT INTO guild_member_quests (player_uid, quest_id, status, progress, recurrence_type, period_key, started_at)\n                VALUES (@playerUid, @questId, 'active', @progress, @recurrenceType, @periodKey, @startedAt);", this.database.Connection))
				{
					command.Parameters.AddWithValue("@playerUid", playerUid);
					command.Parameters.AddWithValue("@questId", questId);
					command.Parameters.AddWithValue("@progress", "{}");
					command.Parameters.AddWithValue("@recurrenceType", quest.RecurrenceType.ToString().ToLowerInvariant());
					command.Parameters.AddWithValue("@periodKey", periodKey);
					command.Parameters.AddWithValue("@startedAt", DateTimeOffset.Now.ToUnixTimeSeconds());
					command.ExecuteNonQuery();
					ILogger logger3 = this.serverApi.Logger;
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler3 = new DefaultInterpolatedStringHandler(57, 3);
					defaultInterpolatedStringHandler3.AppendLiteral("[QuestRepository] Player ");
					defaultInterpolatedStringHandler3.AppendFormatted(playerUid);
					defaultInterpolatedStringHandler3.AppendLiteral(" started quest ");
					defaultInterpolatedStringHandler3.AppendFormatted<int>(questId);
					defaultInterpolatedStringHandler3.AppendLiteral(" with period key ");
					defaultInterpolatedStringHandler3.AppendFormatted(periodKey);
					logger3.Debug(defaultInterpolatedStringHandler3.ToStringAndClear());
					result = true;
				}
			}
			catch (SqliteException ex2) when (ex2.SqliteErrorCode == 19)
			{
				ILogger logger4 = this.serverApi.Logger;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler4 = new DefaultInterpolatedStringHandler(64, 2);
				defaultInterpolatedStringHandler4.AppendLiteral("[QuestRepository] Player ");
				defaultInterpolatedStringHandler4.AppendFormatted(playerUid);
				defaultInterpolatedStringHandler4.AppendLiteral(" already has quest ");
				defaultInterpolatedStringHandler4.AppendFormatted<int>(questId);
				defaultInterpolatedStringHandler4.AppendLiteral(" or is period-locked");
				logger4.Warning(defaultInterpolatedStringHandler4.ToStringAndClear());
				result = false;
			}
			catch (Exception ex)
			{
				this.serverApi.Logger.Error("[QuestRepository] Failed to start quest: " + ex.Message);
				throw;
			}
			return result;
		}

		// Token: 0x0600090E RID: 2318 RVA: 0x0004151C File Offset: 0x0003F71C
		[return: Nullable(2)]
		public PlayerQuestProgress GetPlayerQuestProgress(string playerUid, int questId, string periodKey)
		{
			PlayerQuestProgress result;
			try
			{
				using (SqliteCommand command = new SqliteCommand("\n                SELECT player_uid, quest_id, status, progress, recurrence_type, period_key, started_at, completed_at\n                FROM guild_member_quests\n                WHERE player_uid = @playerUid AND quest_id = @questId AND period_key = @periodKey;", this.database.Connection))
				{
					command.Parameters.AddWithValue("@playerUid", playerUid);
					command.Parameters.AddWithValue("@questId", questId);
					command.Parameters.AddWithValue("@periodKey", periodKey);
					using (SqliteDataReader reader = command.ExecuteReader())
					{
						if (reader.Read())
						{
							result = QuestRepository.ReadPlayerQuestProgress(reader);
						}
						else
						{
							result = null;
						}
					}
				}
			}
			catch (Exception ex)
			{
				this.serverApi.Logger.Error("[QuestRepository] Failed to get player quest progress: " + ex.Message);
				throw;
			}
			return result;
		}

		// Token: 0x0600090F RID: 2319 RVA: 0x000415F8 File Offset: 0x0003F7F8
		public List<PlayerQuestProgress> GetPlayerActiveQuests(string playerUid, GameDate? currentIngameDate = null)
		{
			QuestRepository.<>c__DisplayClass17_0 CS$<>8__locals1 = new QuestRepository.<>c__DisplayClass17_0();
			CS$<>8__locals1.playerUid = playerUid;
			CS$<>8__locals1.currentDateStr = QuestPeriodKeyGenerator.FormatDate(QuestTimeHelper.TodayEastern);
			CS$<>8__locals1.ingameDateStr = null;
			if (currentIngameDate != null)
			{
				CS$<>8__locals1.ingameDateStr = QuestPeriodKeyGenerator.FormatInGameDate(currentIngameDate.Value);
				if (currentIngameDate.Value.Year == 1)
				{
					QuestRepository.<>c__DisplayClass17_0 CS$<>8__locals2 = CS$<>8__locals1;
					string str = "0000";
					string ingameDateStr = CS$<>8__locals1.ingameDateStr;
					CS$<>8__locals2.ingameDateStr = str + ingameDateStr.Substring(4, ingameDateStr.Length - 4);
				}
			}
			return this.ExecutePlayerProgressQuery("\n                SELECT gmq.player_uid, gmq.quest_id, gmq.status, gmq.progress, gmq.recurrence_type, gmq.period_key, gmq.started_at, gmq.completed_at\n                FROM guild_member_quests gmq\n                INNER JOIN guild_quests gq ON gmq.quest_id = gq.id\n                WHERE gmq.player_uid = @playerUid \n                  AND gmq.status = 'active'\n                  AND (\n                      (gq.uses_ingame_time = 0 AND gq.expires_at >= @currentDate)\n                      OR (gq.uses_ingame_time = 1 AND (@ingameDate IS NULL OR gq.expires_at >= @ingameDate))\n                  );", delegate(SqliteCommand cmd)
			{
				cmd.Parameters.AddWithValue("@playerUid", CS$<>8__locals1.playerUid);
				cmd.Parameters.AddWithValue("@currentDate", CS$<>8__locals1.currentDateStr);
				cmd.Parameters.AddWithValue("@ingameDate", CS$<>8__locals1.ingameDateStr ?? DBNull.Value);
			});
		}

		// Token: 0x06000910 RID: 2320 RVA: 0x00041694 File Offset: 0x0003F894
		public List<PlayerQuestProgress> GetPlayerCompletedQuests(string playerUid)
		{
			return this.ExecutePlayerProgressQuery("\n                SELECT player_uid, quest_id, status, progress, recurrence_type, period_key, started_at, completed_at\n                FROM guild_member_quests\n                WHERE player_uid = @playerUid AND status = 'completed'\n                ORDER BY completed_at DESC;", delegate(SqliteCommand cmd)
			{
				cmd.Parameters.AddWithValue("@playerUid", playerUid);
			});
		}

		// Token: 0x06000911 RID: 2321 RVA: 0x000416C8 File Offset: 0x0003F8C8
		public bool UpdatePlayerQuestProgress(PlayerQuestProgress progress)
		{
			ArgumentNullException.ThrowIfNull(progress, "progress");
			bool result;
			try
			{
				using (SqliteCommand command = new SqliteCommand("\n                UPDATE guild_member_quests\n                SET progress = @progress\n                WHERE player_uid = @playerUid AND quest_id = @questId AND status = 'active';", this.database.Connection))
				{
					command.Parameters.AddWithValue("@playerUid", progress.PlayerUid);
					command.Parameters.AddWithValue("@questId", progress.QuestId);
					command.Parameters.AddWithValue("@progress", progress.SerializeProgress());
					result = (command.ExecuteNonQuery() > 0);
				}
			}
			catch (Exception ex)
			{
				this.serverApi.Logger.Error("[QuestRepository] Failed to update quest progress: " + ex.Message);
				throw;
			}
			return result;
		}

		// Token: 0x06000912 RID: 2322 RVA: 0x00041798 File Offset: 0x0003F998
		public bool CompleteQuest(string playerUid, int questId)
		{
			bool result;
			try
			{
				using (SqliteCommand command = new SqliteCommand("\n                UPDATE guild_member_quests\n                SET status = 'completed',\n                    completed_at = @completedAt\n                WHERE player_uid = @playerUid AND quest_id = @questId AND status = 'active';", this.database.Connection))
				{
					command.Parameters.AddWithValue("@playerUid", playerUid);
					command.Parameters.AddWithValue("@questId", questId);
					command.Parameters.AddWithValue("@completedAt", DateTimeOffset.Now.ToUnixTimeSeconds());
					int num = command.ExecuteNonQuery();
					if (num > 0)
					{
						ILogger logger = this.serverApi.Logger;
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(42, 2);
						defaultInterpolatedStringHandler.AppendLiteral("[QuestRepository] Player ");
						defaultInterpolatedStringHandler.AppendFormatted(playerUid);
						defaultInterpolatedStringHandler.AppendLiteral(" completed quest ");
						defaultInterpolatedStringHandler.AppendFormatted<int>(questId);
						logger.Debug(defaultInterpolatedStringHandler.ToStringAndClear());
					}
					result = (num > 0);
				}
			}
			catch (Exception ex)
			{
				this.serverApi.Logger.Error("[QuestRepository] Failed to complete quest: " + ex.Message);
				throw;
			}
			return result;
		}

		// Token: 0x06000913 RID: 2323 RVA: 0x000418AC File Offset: 0x0003FAAC
		public bool AbandonQuest(string playerUid, int questId)
		{
			bool result;
			try
			{
				using (SqliteCommand command = new SqliteCommand("\n                DELETE FROM guild_member_quests\n                WHERE player_uid = @playerUid AND quest_id = @questId AND status = 'active';", this.database.Connection))
				{
					command.Parameters.AddWithValue("@playerUid", playerUid);
					command.Parameters.AddWithValue("@questId", questId);
					int num = command.ExecuteNonQuery();
					if (num > 0)
					{
						ILogger logger = this.serverApi.Logger;
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(42, 2);
						defaultInterpolatedStringHandler.AppendLiteral("[QuestRepository] Player ");
						defaultInterpolatedStringHandler.AppendFormatted(playerUid);
						defaultInterpolatedStringHandler.AppendLiteral(" abandoned quest ");
						defaultInterpolatedStringHandler.AppendFormatted<int>(questId);
						logger.Debug(defaultInterpolatedStringHandler.ToStringAndClear());
					}
					result = (num > 0);
				}
			}
			catch (Exception ex)
			{
				this.serverApi.Logger.Error("[QuestRepository] Failed to abandon quest: " + ex.Message);
				throw;
			}
			return result;
		}

		// Token: 0x06000914 RID: 2324 RVA: 0x0004199C File Offset: 0x0003FB9C
		public int RemovePlayerQuestProgressByPeriodKey(string playerUid, string periodKey)
		{
			int result;
			try
			{
				using (SqliteCommand command = new SqliteCommand("\n                DELETE FROM guild_member_quests\n                WHERE player_uid = @playerUid AND period_key = @periodKey;", this.database.Connection))
				{
					command.Parameters.AddWithValue("@playerUid", playerUid);
					command.Parameters.AddWithValue("@periodKey", periodKey);
					int affected = command.ExecuteNonQuery();
					if (affected > 0)
					{
						ILogger logger = this.serverApi.Logger;
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(78, 3);
						defaultInterpolatedStringHandler.AppendLiteral("[QuestRepository] Removed ");
						defaultInterpolatedStringHandler.AppendFormatted<int>(affected);
						defaultInterpolatedStringHandler.AppendLiteral(" quest progress entries for player ");
						defaultInterpolatedStringHandler.AppendFormatted(playerUid);
						defaultInterpolatedStringHandler.AppendLiteral(" with period key ");
						defaultInterpolatedStringHandler.AppendFormatted(periodKey);
						logger.Notification(defaultInterpolatedStringHandler.ToStringAndClear());
					}
					result = affected;
				}
			}
			catch (Exception ex)
			{
				this.serverApi.Logger.Error("[QuestRepository] Failed to remove quest progress: " + ex.Message);
				throw;
			}
			return result;
		}

		// Token: 0x06000915 RID: 2325 RVA: 0x00041A9C File Offset: 0x0003FC9C
		public bool HasActiveQuest(string playerUid, int questId)
		{
			bool result;
			try
			{
				using (SqliteCommand command = new SqliteCommand("\n                SELECT 1 FROM guild_member_quests\n                WHERE player_uid = @playerUid AND quest_id = @questId AND status = 'active'\n                LIMIT 1;", this.database.Connection))
				{
					command.Parameters.AddWithValue("@playerUid", playerUid);
					command.Parameters.AddWithValue("@questId", questId);
					result = (command.ExecuteScalar() != null);
				}
			}
			catch (Exception ex)
			{
				this.serverApi.Logger.Error("[QuestRepository] Failed to check active quest: " + ex.Message);
				throw;
			}
			return result;
		}

		// Token: 0x06000916 RID: 2326 RVA: 0x00041B40 File Offset: 0x0003FD40
		public bool IsPlayerPeriodLocked(string playerUid, QuestRecurrenceType recurrenceType, string periodKey)
		{
			bool result;
			try
			{
				using (SqliteCommand command = new SqliteCommand("\n                SELECT 1 FROM guild_member_quests\n                WHERE player_uid = @playerUid \n                  AND recurrence_type = @recurrenceType \n                  AND period_key = @periodKey\n                LIMIT 1;", this.database.Connection))
				{
					command.Parameters.AddWithValue("@playerUid", playerUid);
					command.Parameters.AddWithValue("@recurrenceType", recurrenceType.ToString().ToLowerInvariant());
					command.Parameters.AddWithValue("@periodKey", periodKey);
					result = (command.ExecuteScalar() != null);
				}
			}
			catch (Exception ex)
			{
				this.serverApi.Logger.Error("[QuestRepository] Failed to check period lock: " + ex.Message);
				throw;
			}
			return result;
		}

		// Token: 0x06000917 RID: 2327 RVA: 0x00041C00 File Offset: 0x0003FE00
		public List<string> GetPlayerCompletedPeriodKeys(string playerUid, QuestRecurrenceType recurrenceType)
		{
			List<string> keys = new List<string>();
			List<string> result;
			try
			{
				using (SqliteCommand command = new SqliteCommand("\n                SELECT DISTINCT period_key FROM guild_member_quests\n                WHERE player_uid = @playerUid \n                  AND recurrence_type = @recurrenceType\n                  AND status = 'completed'\n                  AND period_key IS NOT NULL;", this.database.Connection))
				{
					command.Parameters.AddWithValue("@playerUid", playerUid);
					command.Parameters.AddWithValue("@recurrenceType", recurrenceType.ToString().ToLowerInvariant());
					using (SqliteDataReader reader = command.ExecuteReader())
					{
						while (reader.Read())
						{
							keys.Add(reader.GetString(0));
						}
						result = keys;
					}
				}
			}
			catch (Exception ex)
			{
				this.serverApi.Logger.Error("[QuestRepository] Failed to get completed period keys: " + ex.Message);
				throw;
			}
			return result;
		}

		// Token: 0x06000918 RID: 2328 RVA: 0x00041CE4 File Offset: 0x0003FEE4
		public bool WasQuestCompletedInPeriod(string playerUid, int questId, string periodKey)
		{
			bool result;
			try
			{
				using (SqliteCommand command = new SqliteCommand("\n                SELECT 1 FROM guild_member_quests\n                WHERE player_uid = @playerUid \n                  AND quest_id = @questId\n                  AND period_key = @periodKey\n                  AND status = 'completed'\n                LIMIT 1;", this.database.Connection))
				{
					command.Parameters.AddWithValue("@playerUid", playerUid);
					command.Parameters.AddWithValue("@questId", questId);
					command.Parameters.AddWithValue("@periodKey", periodKey);
					result = (command.ExecuteScalar() != null);
				}
			}
			catch (Exception ex)
			{
				this.serverApi.Logger.Error("[QuestRepository] Failed to check quest completion in period: " + ex.Message);
				throw;
			}
			return result;
		}

		// Token: 0x06000919 RID: 2329 RVA: 0x00041D98 File Offset: 0x0003FF98
		public int CleanupExpiredQuestProgress(DateTime? currentDate = null, GameDate? currentIngameDate = null)
		{
			string realDateStr = QuestPeriodKeyGenerator.FormatDate(currentDate ?? QuestTimeHelper.TodayEastern);
			int totalDeleted = 0;
			int result;
			try
			{
				using (SqliteCommand command = new SqliteCommand("\n                DELETE FROM guild_member_quests\n                WHERE status = 'active'\n                  AND quest_id IN (\n                      SELECT id FROM guild_quests\n                      WHERE uses_ingame_time = 0 AND expires_at < @currentDate\n                  );", this.database.Connection))
				{
					command.Parameters.AddWithValue("@currentDate", realDateStr);
					totalDeleted += command.ExecuteNonQuery();
				}
				if (currentIngameDate != null)
				{
					string ingameDateStr = QuestPeriodKeyGenerator.FormatInGameDate(currentIngameDate.Value);
					if (currentIngameDate.Value.Year == 1)
					{
						string str = "0000";
						string text = ingameDateStr;
						ingameDateStr = str + text.Substring(4, text.Length - 4);
					}
					using (SqliteCommand command2 = new SqliteCommand("\n                        DELETE FROM guild_member_quests\n                        WHERE status = 'active'\n                          AND quest_id IN (\n                              SELECT id FROM guild_quests\n                              WHERE uses_ingame_time = 1 AND expires_at < @currentDate\n                          );", this.database.Connection))
					{
						command2.Parameters.AddWithValue("@currentDate", ingameDateStr);
						totalDeleted += command2.ExecuteNonQuery();
					}
				}
				if (totalDeleted > 0)
				{
					ILogger logger = this.serverApi.Logger;
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(60, 1);
					defaultInterpolatedStringHandler.AppendLiteral("[QuestRepository] Cleaned up ");
					defaultInterpolatedStringHandler.AppendFormatted<int>(totalDeleted);
					defaultInterpolatedStringHandler.AppendLiteral(" expired quest progress entries");
					logger.Notification(defaultInterpolatedStringHandler.ToStringAndClear());
				}
				result = totalDeleted;
			}
			catch (Exception ex)
			{
				this.serverApi.Logger.Error("[QuestRepository] Failed to cleanup expired quests: " + ex.Message);
				throw;
			}
			return result;
		}

		// Token: 0x0600091A RID: 2330 RVA: 0x00041F54 File Offset: 0x00040154
		private List<Quest> ExecuteQuestQuery(string sql, [Nullable(new byte[]
		{
			2,
			1
		})] Action<SqliteCommand> configureCommand = null)
		{
			List<Quest> quests = new List<Quest>();
			try
			{
				using (SqliteCommand command = new SqliteCommand(sql, this.database.Connection))
				{
					if (configureCommand != null)
					{
						configureCommand(command);
					}
					using (SqliteDataReader reader = command.ExecuteReader())
					{
						while (reader.Read())
						{
							quests.Add(QuestRepository.ReadQuest(reader));
						}
					}
				}
			}
			catch (Exception ex)
			{
				this.serverApi.Logger.Error("[QuestRepository] Quest query failed: " + ex.Message);
				throw;
			}
			return quests;
		}

		// Token: 0x0600091B RID: 2331 RVA: 0x00042008 File Offset: 0x00040208
		private List<PlayerQuestProgress> ExecutePlayerProgressQuery(string sql, [Nullable(new byte[]
		{
			2,
			1
		})] Action<SqliteCommand> configureCommand = null)
		{
			List<PlayerQuestProgress> progressList = new List<PlayerQuestProgress>();
			try
			{
				using (SqliteCommand command = new SqliteCommand(sql, this.database.Connection))
				{
					if (configureCommand != null)
					{
						configureCommand(command);
					}
					using (SqliteDataReader reader = command.ExecuteReader())
					{
						while (reader.Read())
						{
							progressList.Add(QuestRepository.ReadPlayerQuestProgress(reader));
						}
					}
				}
			}
			catch (Exception ex)
			{
				this.serverApi.Logger.Error("[QuestRepository] Player progress query failed: " + ex.Message);
				throw;
			}
			return progressList;
		}

		// Token: 0x0600091C RID: 2332 RVA: 0x000420BC File Offset: 0x000402BC
		private static Quest ReadQuest(SqliteDataReader reader)
		{
			return new Quest
			{
				Id = reader.GetInt32(0),
				RecurrenceType = Enum.Parse<QuestRecurrenceType>(reader.GetString(1), true),
				Title = reader.GetString(2),
				Description = reader.GetString(3),
				Objectives = Quest.DeserializeRequirements(reader.GetString(4)),
				Rewards = Quest.DeserializeRewards(reader.GetString(5)),
				StartsAt = reader.GetString(6),
				ExpiresAt = reader.GetString(7),
				UsesIngameTime = (reader.GetInt32(8) == 1),
				Repeat = (reader.GetInt32(9) == 1),
				CreatedAt = reader.GetInt64(10)
			};
		}

		// Token: 0x0600091D RID: 2333 RVA: 0x00042178 File Offset: 0x00040378
		private static PlayerQuestProgress ReadPlayerQuestProgress(SqliteDataReader reader)
		{
			return new PlayerQuestProgress
			{
				PlayerUid = reader.GetString(0),
				QuestId = reader.GetInt32(1),
				Status = Enum.Parse<PlayerQuestStatus>(reader.GetString(2), true),
				ObjectiveProgress = PlayerQuestProgress.DeserializeProgress(reader.IsDBNull(3) ? null : reader.GetString(3)),
				RecurrenceType = Enum.Parse<QuestRecurrenceType>(reader.GetString(4), true),
				PeriodKey = (reader.IsDBNull(5) ? null : reader.GetString(5)),
				StartedAt = reader.GetInt64(6),
				CompletedAt = (reader.IsDBNull(7) ? null : new long?(reader.GetInt64(7)))
			};
		}

		// Token: 0x040003A8 RID: 936
		private readonly ICoreServerAPI serverApi = serverApi;

		// Token: 0x040003A9 RID: 937
		private readonly GuildDatabase database = database;
	}
}
