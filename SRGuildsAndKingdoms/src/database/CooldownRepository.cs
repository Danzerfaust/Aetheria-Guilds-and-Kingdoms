using System;
using System.Runtime.CompilerServices;
using Microsoft.Data.Sqlite;
using Vintagestory.API.Common;
using Vintagestory.API.Server;

namespace SRGuildsAndKingdoms.src.database
{
	// Token: 0x020000B1 RID: 177
	[NullableContext(1)]
	[Nullable(0)]
	public class CooldownRepository
	{
		// Token: 0x0600081B RID: 2075 RVA: 0x000389BF File Offset: 0x00036BBF
		public CooldownRepository(ICoreServerAPI serverApi, GuildDatabase database)
		{
			this.serverApi = serverApi;
			this.database = database;
		}

		// Token: 0x0600081C RID: 2076 RVA: 0x000389D8 File Offset: 0x00036BD8
		public void SetCooldown(string playerUid, DateTime expiresAt)
		{
			try
			{
				SqliteConnection connection = this.database.Connection;
				using (SqliteCommand command = new SqliteCommand("\n                    INSERT INTO guild_cooldowns (player_uid, expires_at)\n                    VALUES (@playerUid, @expiresAt)\n                    ON CONFLICT(player_uid) DO UPDATE SET expires_at = excluded.expires_at;", connection))
				{
					command.Parameters.AddWithValue("@playerUid", playerUid);
					command.Parameters.AddWithValue("@expiresAt", new DateTimeOffset(expiresAt).ToUnixTimeSeconds());
					command.ExecuteNonQuery();
					ILogger logger = this.serverApi.Logger;
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(58, 2);
					defaultInterpolatedStringHandler.AppendLiteral("[CooldownRepository] Set cooldown for player '");
					defaultInterpolatedStringHandler.AppendFormatted(playerUid);
					defaultInterpolatedStringHandler.AppendLiteral("' until ");
					defaultInterpolatedStringHandler.AppendFormatted<DateTime>(expiresAt, "yyyy-MM-dd HH:mm:ss");
					defaultInterpolatedStringHandler.AppendLiteral(" UTC");
					logger.Debug(defaultInterpolatedStringHandler.ToStringAndClear());
				}
			}
			catch (Exception ex)
			{
				this.serverApi.Logger.Error("[CooldownRepository] Failed to set cooldown: " + ex.Message);
			}
		}

		// Token: 0x0600081D RID: 2077 RVA: 0x00038AE4 File Offset: 0x00036CE4
		public DateTime? GetCooldown(string playerUid)
		{
			DateTime? dateTime;
			try
			{
				SqliteConnection connection = this.database.Connection;
				using (SqliteCommand command = new SqliteCommand("SELECT expires_at FROM guild_cooldowns WHERE player_uid = @playerUid;", connection))
				{
					command.Parameters.AddWithValue("@playerUid", playerUid);
					object result = command.ExecuteScalar();
					if (result == null || result == DBNull.Value)
					{
						dateTime = null;
						dateTime = dateTime;
					}
					else
					{
						DateTime expiresAt = DateTimeOffset.FromUnixTimeSeconds(Convert.ToInt64(result)).DateTime;
						if (expiresAt <= DateTime.UtcNow)
						{
							this.ClearCooldown(playerUid);
							dateTime = null;
						}
						else
						{
							dateTime = new DateTime?(expiresAt);
						}
					}
				}
			}
			catch (Exception ex)
			{
				this.serverApi.Logger.Error("[CooldownRepository] Failed to get cooldown: " + ex.Message);
				dateTime = null;
			}
			return dateTime;
		}

		// Token: 0x0600081E RID: 2078 RVA: 0x00038BD8 File Offset: 0x00036DD8
		public bool IsOnCooldown(string playerUid, out TimeSpan remainingTime)
		{
			remainingTime = TimeSpan.Zero;
			DateTime? expiresAt = this.GetCooldown(playerUid);
			if (expiresAt == null)
			{
				return false;
			}
			DateTime now = DateTime.UtcNow;
			if (expiresAt.Value <= now)
			{
				return false;
			}
			remainingTime = expiresAt.Value - now;
			return true;
		}

		// Token: 0x0600081F RID: 2079 RVA: 0x00038C30 File Offset: 0x00036E30
		public bool ClearCooldown(string playerUid)
		{
			bool result;
			try
			{
				SqliteConnection connection = this.database.Connection;
				using (SqliteCommand command = new SqliteCommand("DELETE FROM guild_cooldowns WHERE player_uid = @playerUid;", connection))
				{
					command.Parameters.AddWithValue("@playerUid", playerUid);
					int num = command.ExecuteNonQuery();
					if (num > 0)
					{
						this.serverApi.Logger.Debug("[CooldownRepository] Cleared cooldown for player '" + playerUid + "'");
					}
					result = (num > 0);
				}
			}
			catch (Exception ex)
			{
				this.serverApi.Logger.Error("[CooldownRepository] Failed to clear cooldown: " + ex.Message);
				result = false;
			}
			return result;
		}

		// Token: 0x04000354 RID: 852
		private readonly ICoreServerAPI serverApi;

		// Token: 0x04000355 RID: 853
		private readonly GuildDatabase database;
	}
}
