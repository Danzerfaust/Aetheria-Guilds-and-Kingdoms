using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Microsoft.Data.Sqlite;
using Vintagestory.API.Common;
using Vintagestory.API.Server;

namespace SRGuildsAndKingdoms.src.database
{
	// Token: 0x020000BF RID: 191
	[NullableContext(1)]
	[Nullable(0)]
	public class ZoneWhitelistRepository
	{
		// Token: 0x0600091E RID: 2334 RVA: 0x00042234 File Offset: 0x00040434
		public ZoneWhitelistRepository(ICoreServerAPI serverApi, GuildDatabase database)
		{
			this.serverApi = serverApi;
			this.database = database;
		}

		// Token: 0x0600091F RID: 2335 RVA: 0x0004224C File Offset: 0x0004044C
		public bool AddPlayerToZone(int zoneId, string playerUid)
		{
			if (zoneId < 0 || string.IsNullOrWhiteSpace(playerUid))
			{
				return false;
			}
			bool result;
			try
			{
				SqliteConnection connection = this.database.Connection;
				using (SqliteCommand command = new SqliteCommand("\n                    INSERT INTO zone_whitelists (zone_id, player_uid)\n                    VALUES (@zoneId, @playerUid)\n                    ON CONFLICT(zone_id, player_uid) DO NOTHING;", connection))
				{
					command.Parameters.AddWithValue("@zoneId", zoneId);
					command.Parameters.AddWithValue("@playerUid", playerUid);
					int num = command.ExecuteNonQuery();
					if (num > 0)
					{
						ILogger logger = this.serverApi.Logger;
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(73, 2);
						defaultInterpolatedStringHandler.AppendLiteral("[SRGuildsAndKingdoms:ZoneWhitelistRepository] Added player '");
						defaultInterpolatedStringHandler.AppendFormatted(playerUid);
						defaultInterpolatedStringHandler.AppendLiteral("' to zone ID ");
						defaultInterpolatedStringHandler.AppendFormatted<int>(zoneId);
						logger.Debug(defaultInterpolatedStringHandler.ToStringAndClear());
					}
					result = (num > 0);
				}
			}
			catch (Exception ex)
			{
				this.serverApi.Logger.Error("[SRGuildsAndKingdoms:ZoneWhitelistRepository] Failed to add player to zone: " + ex.Message);
				result = false;
			}
			return result;
		}

		// Token: 0x06000920 RID: 2336 RVA: 0x00042350 File Offset: 0x00040550
		public bool RemovePlayerFromZone(int zoneId, string playerUid)
		{
			if (zoneId < 0 || string.IsNullOrWhiteSpace(playerUid))
			{
				return false;
			}
			bool result;
			try
			{
				SqliteConnection connection = this.database.Connection;
				using (SqliteCommand command = new SqliteCommand("\n                    DELETE FROM zone_whitelists \n                    WHERE zone_id = @zoneId AND player_uid = @playerUid;", connection))
				{
					command.Parameters.AddWithValue("@zoneId", zoneId);
					command.Parameters.AddWithValue("@playerUid", playerUid);
					int num = command.ExecuteNonQuery();
					if (num > 0)
					{
						ILogger logger = this.serverApi.Logger;
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(77, 2);
						defaultInterpolatedStringHandler.AppendLiteral("[SRGuildsAndKingdoms:ZoneWhitelistRepository] Removed player '");
						defaultInterpolatedStringHandler.AppendFormatted(playerUid);
						defaultInterpolatedStringHandler.AppendLiteral("' from zone ID ");
						defaultInterpolatedStringHandler.AppendFormatted<int>(zoneId);
						logger.Debug(defaultInterpolatedStringHandler.ToStringAndClear());
					}
					result = (num > 0);
				}
			}
			catch (Exception ex)
			{
				this.serverApi.Logger.Error("[SRGuildsAndKingdoms:ZoneWhitelistRepository] Failed to remove player from zone: " + ex.Message);
				result = false;
			}
			return result;
		}

		// Token: 0x06000921 RID: 2337 RVA: 0x00042454 File Offset: 0x00040654
		public bool IsPlayerWhitelisted(int zoneId, string playerUid)
		{
			if (zoneId < 0 || string.IsNullOrWhiteSpace(playerUid))
			{
				return false;
			}
			bool result;
			try
			{
				SqliteConnection connection = this.database.Connection;
				using (SqliteCommand command = new SqliteCommand("\n                    SELECT COUNT(*) FROM zone_whitelists \n                    WHERE zone_id = @zoneId AND player_uid = @playerUid;", connection))
				{
					command.Parameters.AddWithValue("@zoneId", zoneId);
					command.Parameters.AddWithValue("@playerUid", playerUid);
					result = (Convert.ToInt32(command.ExecuteScalar()) > 0);
				}
			}
			catch (Exception ex)
			{
				this.serverApi.Logger.Error("[SRGuildsAndKingdoms:ZoneWhitelistRepository] Failed to check whitelist: " + ex.Message);
				result = false;
			}
			return result;
		}

		// Token: 0x06000922 RID: 2338 RVA: 0x00042510 File Offset: 0x00040710
		public List<int> GetWhitelistedZones(string playerUid)
		{
			List<int> zones = new List<int>();
			if (string.IsNullOrWhiteSpace(playerUid))
			{
				return zones;
			}
			try
			{
				SqliteConnection connection = this.database.Connection;
				using (SqliteCommand command = new SqliteCommand("SELECT zone_id FROM zone_whitelists WHERE player_uid = @playerUid;", connection))
				{
					command.Parameters.AddWithValue("@playerUid", playerUid);
					using (SqliteDataReader reader = command.ExecuteReader())
					{
						while (reader.Read())
						{
							zones.Add(reader.GetInt32(0));
						}
					}
				}
			}
			catch (Exception ex)
			{
				this.serverApi.Logger.Error("[SRGuildsAndKingdoms:ZoneWhitelistRepository] Failed to get whitelisted zones: " + ex.Message);
			}
			return zones;
		}

		// Token: 0x06000923 RID: 2339 RVA: 0x000425E0 File Offset: 0x000407E0
		public List<string> GetWhitelistedPlayers(int zoneId)
		{
			List<string> players = new List<string>();
			if (zoneId < 0)
			{
				return players;
			}
			try
			{
				SqliteConnection connection = this.database.Connection;
				using (SqliteCommand command = new SqliteCommand("SELECT player_uid FROM zone_whitelists WHERE zone_id = @zoneId;", connection))
				{
					command.Parameters.AddWithValue("@zoneId", zoneId);
					using (SqliteDataReader reader = command.ExecuteReader())
					{
						while (reader.Read())
						{
							players.Add(reader.GetString(0));
						}
					}
				}
			}
			catch (Exception ex)
			{
				this.serverApi.Logger.Error("[SRGuildsAndKingdoms:ZoneWhitelistRepository] Failed to get whitelisted players: " + ex.Message);
			}
			return players;
		}

		// Token: 0x06000924 RID: 2340 RVA: 0x000426B0 File Offset: 0x000408B0
		public int ClearZone(int zoneId)
		{
			if (zoneId < 0)
			{
				return 0;
			}
			int result;
			try
			{
				SqliteConnection connection = this.database.Connection;
				using (SqliteCommand command = new SqliteCommand("DELETE FROM zone_whitelists WHERE zone_id = @zoneId;", connection))
				{
					command.Parameters.AddWithValue("@zoneId", zoneId);
					int rowsDeleted = command.ExecuteNonQuery();
					if (rowsDeleted > 0)
					{
						ILogger logger = this.serverApi.Logger;
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(78, 2);
						defaultInterpolatedStringHandler.AppendLiteral("[SRGuildsAndKingdoms:ZoneWhitelistRepository] Cleared ");
						defaultInterpolatedStringHandler.AppendFormatted<int>(rowsDeleted);
						defaultInterpolatedStringHandler.AppendLiteral(" player(s) from zone ID ");
						defaultInterpolatedStringHandler.AppendFormatted<int>(zoneId);
						logger.Debug(defaultInterpolatedStringHandler.ToStringAndClear());
					}
					result = rowsDeleted;
				}
			}
			catch (Exception ex)
			{
				this.serverApi.Logger.Error("[SRGuildsAndKingdoms:ZoneWhitelistRepository] Failed to clear zone: " + ex.Message);
				result = 0;
			}
			return result;
		}

		// Token: 0x06000925 RID: 2341 RVA: 0x0004279C File Offset: 0x0004099C
		public List<int> GetAllZoneIds()
		{
			List<int> zones = new List<int>();
			try
			{
				SqliteConnection connection = this.database.Connection;
				using (SqliteCommand command = new SqliteCommand("SELECT DISTINCT zone_id FROM zone_whitelists;", connection))
				{
					using (SqliteDataReader reader = command.ExecuteReader())
					{
						while (reader.Read())
						{
							zones.Add(reader.GetInt32(0));
						}
					}
				}
			}
			catch (Exception ex)
			{
				this.serverApi.Logger.Error("[SRGuildsAndKingdoms:ZoneWhitelistRepository] Failed to get zone IDs: " + ex.Message);
			}
			return zones;
		}

		// Token: 0x06000926 RID: 2342 RVA: 0x00042850 File Offset: 0x00040A50
		public int GetTotalWhitelistedPlayersCount()
		{
			int result;
			try
			{
				SqliteConnection connection = this.database.Connection;
				using (SqliteCommand command = new SqliteCommand("SELECT COUNT(DISTINCT player_uid) FROM zone_whitelists;", connection))
				{
					result = Convert.ToInt32(command.ExecuteScalar());
				}
			}
			catch (Exception ex)
			{
				this.serverApi.Logger.Error("[SRGuildsAndKingdoms:ZoneWhitelistRepository] Failed to get total player count: " + ex.Message);
				result = 0;
			}
			return result;
		}

		// Token: 0x06000927 RID: 2343 RVA: 0x000428D0 File Offset: 0x00040AD0
		public Dictionary<int, List<string>> GetAllWhitelists()
		{
			Dictionary<int, List<string>> whitelists = new Dictionary<int, List<string>>();
			try
			{
				SqliteConnection connection = this.database.Connection;
				using (SqliteCommand command = new SqliteCommand("SELECT zone_id, player_uid FROM zone_whitelists ORDER BY zone_id, player_uid;", connection))
				{
					using (SqliteDataReader reader = command.ExecuteReader())
					{
						while (reader.Read())
						{
							int zoneId = reader.GetInt32(0);
							string playerUid = reader.GetString(1);
							if (!whitelists.ContainsKey(zoneId))
							{
								whitelists[zoneId] = new List<string>();
							}
							whitelists[zoneId].Add(playerUid);
						}
					}
				}
			}
			catch (Exception ex)
			{
				this.serverApi.Logger.Error("[SRGuildsAndKingdoms:ZoneWhitelistRepository] Failed to get all whitelists: " + ex.Message);
			}
			return whitelists;
		}

		// Token: 0x040003AA RID: 938
		private readonly ICoreServerAPI serverApi;

		// Token: 0x040003AB RID: 939
		private readonly GuildDatabase database;
	}
}
