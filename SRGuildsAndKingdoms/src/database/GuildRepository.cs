using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Microsoft.Data.Sqlite;
using SRGuildsAndKingdoms.src.guilds;
using SRGuildsAndKingdoms.src.techblock;
using Vintagestory.API.Common;
using Vintagestory.API.Server;

namespace SRGuildsAndKingdoms.src.database
{
	// Token: 0x020000B4 RID: 180
	[NullableContext(1)]
	[Nullable(0)]
	public class GuildRepository
	{
		// Token: 0x06000833 RID: 2099 RVA: 0x00039D04 File Offset: 0x00037F04
		public GuildRepository(ICoreServerAPI serverApi, GuildDatabase database)
		{
		}

		// Token: 0x06000834 RID: 2100 RVA: 0x00039D30 File Offset: 0x00037F30
		public void LoadAllGuilds()
		{
			try
			{
				this.serverApi.Logger.Notification("[SRGuildsAndKingdoms] Loading guilds from db...");
				this.guildCache.Clear();
				this.dirtyGuilds.Clear();
				foreach (Guild guild in this.LoadGuildsFromDatabase())
				{
					this.guildCache[guild.Name] = guild;
				}
				this.cacheLoaded = true;
				ILogger logger = this.serverApi.Logger;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(44, 1);
				defaultInterpolatedStringHandler.AppendLiteral("[SRGuildsAndKingdoms] Loaded ");
				defaultInterpolatedStringHandler.AppendFormatted<int>(this.guildCache.Count);
				defaultInterpolatedStringHandler.AppendLiteral(" guilds from db");
				logger.Notification(defaultInterpolatedStringHandler.ToStringAndClear());
			}
			catch (Exception ex)
			{
				this.serverApi.Logger.Error("[SRGuildsAndKingdoms] Failed to load guilds: " + ex.Message);
				this.serverApi.Logger.Error("[SRGuildsAndKingdoms] Stack trace: " + ex.StackTrace);
				throw;
			}
		}

		// Token: 0x06000835 RID: 2101 RVA: 0x00039E5C File Offset: 0x0003805C
		[return: Nullable(2)]
		public Guild GetGuild(string name)
		{
			this.EnsureCacheLoaded();
			if (string.IsNullOrWhiteSpace(name))
			{
				return null;
			}
			Guild guild;
			this.guildCache.TryGetValue(name, out guild);
			return guild;
		}

		// Token: 0x06000836 RID: 2102 RVA: 0x00039E8C File Offset: 0x0003808C
		[return: Nullable(2)]
		public Guild GetGuildByMember(string playerUid)
		{
			this.EnsureCacheLoaded();
			if (string.IsNullOrWhiteSpace(playerUid))
			{
				return null;
			}
			return this.guildCache.Values.FirstOrDefault((Guild g) => g.Members.ContainsKey(playerUid));
		}

		// Token: 0x06000837 RID: 2103 RVA: 0x00039ED7 File Offset: 0x000380D7
		public List<Guild> GetAllGuilds()
		{
			this.EnsureCacheLoaded();
			return this.guildCache.Values.ToList<Guild>();
		}

		// Token: 0x06000838 RID: 2104 RVA: 0x00039EF0 File Offset: 0x000380F0
		public void CreateGuild(Guild guild)
		{
			ArgumentNullException.ThrowIfNull(guild, "guild");
			if (string.IsNullOrWhiteSpace(guild.Name))
			{
				throw new ArgumentException("Guild name cannot be empty");
			}
			this.EnsureCacheLoaded();
			if (this.guildCache.ContainsKey(guild.Name))
			{
				throw new InvalidOperationException("Guild '" + guild.Name + "' already exists");
			}
			try
			{
				SqliteConnection connection = this.database.Connection;
				using (SqliteTransaction transaction = connection.BeginTransaction())
				{
					try
					{
						using (SqliteCommand insertCommand = new SqliteCommand("\n                        INSERT INTO guilds (name, description, display_color, secondary_color, points, created_at, updated_at)\n                        VALUES (@name, @description, @displayColor, @secondaryColor, @points, @createdAt, @updatedAt);\n                        SELECT last_insert_rowid();", connection))
						{
							insertCommand.Transaction = transaction;
							insertCommand.Parameters.AddWithValue("@name", guild.Name);
							insertCommand.Parameters.AddWithValue("@description", guild.Description);
							insertCommand.Parameters.AddWithValue("@displayColor", guild.DisplayColor);
							insertCommand.Parameters.AddWithValue("@secondaryColor", guild.SecondaryColor);
							insertCommand.Parameters.AddWithValue("@points", guild.Points);
							insertCommand.Parameters.AddWithValue("@createdAt", DateTimeOffset.UtcNow.ToUnixTimeSeconds());
							insertCommand.Parameters.AddWithValue("@updatedAt", DateTimeOffset.UtcNow.ToUnixTimeSeconds());
							guild.DatabaseId = new int?(Convert.ToInt32(insertCommand.ExecuteScalar()));
							int guildId = guild.DatabaseId.Value;
							this.UpsertGuildMembers(connection, transaction, guildId, guild);
							this.UpsertGuildRoles(connection, transaction, guildId, guild);
							this.UpsertGuildInvites(connection, transaction, guildId, guild);
							this.UpsertLandClaims(connection, transaction, guildId, guild);
							this.UpsertTechProgress(connection, transaction, guildId, guild);
							transaction.Commit();
							ILogger logger = this.serverApi.Logger;
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(57, 2);
							defaultInterpolatedStringHandler.AppendLiteral("[GuildRepository] Created new guild '");
							defaultInterpolatedStringHandler.AppendFormatted(guild.Name);
							defaultInterpolatedStringHandler.AppendLiteral("' with database ID: ");
							defaultInterpolatedStringHandler.AppendFormatted<int?>(guild.DatabaseId);
							logger.Notification(defaultInterpolatedStringHandler.ToStringAndClear());
						}
					}
					catch (Exception ex)
					{
						transaction.Rollback();
						this.serverApi.Logger.Error("[GuildRepository] Transaction rolled back during guild creation: " + ex.Message);
						throw;
					}
				}
			}
			catch (Exception ex2)
			{
				this.serverApi.Logger.Error("[GuildRepository] Failed to create guild '" + guild.Name + "': " + ex2.Message);
				throw;
			}
			this.guildCache[guild.Name] = guild;
		}

		// Token: 0x06000839 RID: 2105 RVA: 0x0003A1E4 File Offset: 0x000383E4
		public void UpdateGuild(Guild guild)
		{
			ArgumentNullException.ThrowIfNull(guild, "guild");
			this.EnsureCacheLoaded();
			if (!this.guildCache.ContainsKey(guild.Name))
			{
				throw new InvalidOperationException("Guild '" + guild.Name + "' does not exist");
			}
			this.guildCache[guild.Name] = guild;
			this.dirtyGuilds.Add(guild.Name);
		}

		// Token: 0x0600083A RID: 2106 RVA: 0x0003A254 File Offset: 0x00038454
		public void DeleteGuild(string guildName)
		{
			if (string.IsNullOrWhiteSpace(guildName))
			{
				throw new ArgumentException("Guild name cannot be empty");
			}
			this.EnsureCacheLoaded();
			if (!this.guildCache.ContainsKey(guildName))
			{
				return;
			}
			this.guildCache.Remove(guildName);
			this.dirtyGuilds.Add(guildName);
		}

		// Token: 0x0600083B RID: 2107 RVA: 0x0003A2A3 File Offset: 0x000384A3
		public void MarkDirty(string guildName)
		{
			if (!string.IsNullOrWhiteSpace(guildName) && this.guildCache.ContainsKey(guildName))
			{
				this.dirtyGuilds.Add(guildName);
			}
		}

		// Token: 0x0600083C RID: 2108 RVA: 0x0003A2C8 File Offset: 0x000384C8
		public void RenameGuild(Guild guild, string newName)
		{
			ArgumentNullException.ThrowIfNull(guild, "guild");
			if (string.IsNullOrWhiteSpace(newName))
			{
				throw new ArgumentException("New guild name cannot be empty");
			}
			this.EnsureCacheLoaded();
			if (!this.guildCache.ContainsKey(guild.Name))
			{
				throw new InvalidOperationException("Guild '" + guild.Name + "' not found in cache");
			}
			if (this.guildCache.ContainsKey(newName))
			{
				throw new InvalidOperationException("Guild name '" + newName + "' is already taken");
			}
			string oldName = guild.Name;
			this.guildCache.Remove(oldName);
			guild.Name = newName;
			this.guildCache[newName] = guild;
			this.dirtyGuilds.Remove(oldName);
			this.dirtyGuilds.Add(newName);
		}

		// Token: 0x0600083D RID: 2109 RVA: 0x0003A390 File Offset: 0x00038590
		public void CommitChanges()
		{
			if (!this.cacheLoaded)
			{
				this.serverApi.Logger.Warning("[SRGuildsAndKingdoms] Cannot save guild changes: cache not loaded");
				return;
			}
			if (this.dirtyGuilds.Count == 0)
			{
				return;
			}
			try
			{
				SqliteConnection connection = this.database.Connection;
				using (SqliteTransaction transaction = connection.BeginTransaction())
				{
					try
					{
						foreach (string guildName in this.dirtyGuilds.ToList<string>())
						{
							Guild guild;
							if (this.guildCache.TryGetValue(guildName, out guild))
							{
								this.UpsertGuild(connection, transaction, guild);
							}
							else
							{
								this.DeleteGuildFromDatabase(connection, transaction, guildName);
							}
						}
						transaction.Commit();
						int savedCount = this.dirtyGuilds.Count;
						this.dirtyGuilds.Clear();
						ILogger logger = this.serverApi.Logger;
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(48, 1);
						defaultInterpolatedStringHandler.AppendLiteral("[SRGuildsAndKingdoms] Saved ");
						defaultInterpolatedStringHandler.AppendFormatted<int>(savedCount);
						defaultInterpolatedStringHandler.AppendLiteral(" guild changes to db");
						logger.Notification(defaultInterpolatedStringHandler.ToStringAndClear());
					}
					catch (Exception ex)
					{
						transaction.Rollback();
						this.serverApi.Logger.Error("[SRGuildsAndKingdoms] Transaction rolled back due to error: " + ex.Message);
						throw;
					}
				}
			}
			catch (Exception ex2)
			{
				this.serverApi.Logger.Error("[SRGuildsAndKingdoms] Failed to commit changes: " + ex2.Message);
				this.serverApi.Logger.Error("[SRGuildsAndKingdoms] Stack trace: " + ex2.StackTrace);
				throw;
			}
		}

		// Token: 0x0600083E RID: 2110 RVA: 0x0003A57C File Offset: 0x0003877C
		public List<Guild> GetLeaderboard(int limit = 10)
		{
			List<Guild> result;
			try
			{
				SqliteConnection connection = this.database.Connection;
				using (SqliteCommand command = new SqliteCommand("\n                    SELECT id, name, description, display_color, secondary_color, \n                           points, created_at, updated_at\n                    FROM guilds\n                    ORDER BY points DESC, name ASC\n                    LIMIT @limit;", connection))
				{
					command.Parameters.AddWithValue("@limit", limit);
					List<Guild> guilds = new List<Guild>();
					using (SqliteDataReader reader = command.ExecuteReader())
					{
						while (reader.Read())
						{
							int guildId = reader.GetInt32(0);
							string guildName = reader.GetString(1);
							Guild cachedGuild;
							if (this.guildCache.TryGetValue(guildName, out cachedGuild))
							{
								guilds.Add(cachedGuild);
							}
							else
							{
								Guild guild = this.LoadGuildById(connection, guildId);
								if (guild != null)
								{
									guilds.Add(guild);
								}
							}
						}
						result = guilds;
					}
				}
			}
			catch (Exception ex)
			{
				this.serverApi.Logger.Error("[SRGuildsAndKingdoms] Failed to get leaderboard: " + ex.Message);
				result = new List<Guild>();
			}
			return result;
		}

		// Token: 0x0600083F RID: 2111 RVA: 0x0003A684 File Offset: 0x00038884
		private void EnsureCacheLoaded()
		{
			if (!this.cacheLoaded)
			{
				this.LoadAllGuilds();
			}
		}

		// Token: 0x06000840 RID: 2112 RVA: 0x0003A694 File Offset: 0x00038894
		private List<Guild> LoadGuildsFromDatabase()
		{
			List<Guild> guilds = new List<Guild>();
			SqliteConnection connection = this.database.Connection;
			List<Guild> result;
			using (SqliteCommand command = new SqliteCommand("\n                SELECT id, name, description, display_color, secondary_color, \n                       points, created_at, updated_at\n                FROM guilds;", connection))
			{
				using (SqliteDataReader reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						int guildId = reader.GetInt32(0);
						Guild guild = new Guild
						{
							DatabaseId = new int?(guildId),
							Name = reader.GetString(1),
							Description = reader.GetString(2),
							DisplayColor = reader.GetInt32(3),
							SecondaryColor = reader.GetInt32(4),
							Points = reader.GetInt32(5)
						};
						this.LoadGuildMembers(connection, guildId, guild);
						this.LoadGuildRoles(connection, guildId, guild);
						this.LoadGuildInvites(connection, guildId, guild);
						this.LoadLandClaims(connection, guildId, guild);
						this.LoadTechProgress(connection, guildId, guild);
						guilds.Add(guild);
					}
					result = guilds;
				}
			}
			return result;
		}

		// Token: 0x06000841 RID: 2113 RVA: 0x0003A7AC File Offset: 0x000389AC
		[return: Nullable(2)]
		private Guild LoadGuildById(SqliteConnection connection, int guildId)
		{
			Guild result;
			using (SqliteCommand command = new SqliteCommand("\n                SELECT id, name, description, display_color, secondary_color, \n                       points, created_at, updated_at\n                FROM guilds\n                WHERE id = @guildId;", connection))
			{
				command.Parameters.AddWithValue("@guildId", guildId);
				using (SqliteDataReader reader = command.ExecuteReader())
				{
					if (!reader.Read())
					{
						result = null;
					}
					else
					{
						Guild guild = new Guild
						{
							DatabaseId = new int?(guildId),
							Name = reader.GetString(1),
							Description = reader.GetString(2),
							DisplayColor = reader.GetInt32(3),
							SecondaryColor = reader.GetInt32(4),
							Points = reader.GetInt32(5)
						};
						this.LoadGuildMembers(connection, guildId, guild);
						this.LoadGuildRoles(connection, guildId, guild);
						this.LoadGuildInvites(connection, guildId, guild);
						this.LoadLandClaims(connection, guildId, guild);
						this.LoadTechProgress(connection, guildId, guild);
						result = guild;
					}
				}
			}
			return result;
		}

		// Token: 0x06000842 RID: 2114 RVA: 0x0003A8A8 File Offset: 0x00038AA8
		private void LoadGuildMembers(SqliteConnection connection, int guildId, Guild guild)
		{
			using (SqliteCommand command = new SqliteCommand("\n                SELECT player_uid, role, joined_at, last_seen, points_contribution\n                FROM guild_members\n                WHERE guild_id = @guildId;", connection))
			{
				command.Parameters.AddWithValue("@guildId", guildId);
				using (SqliteDataReader reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						GuildMember member = new GuildMember
						{
							PlayerUid = reader.GetString(0),
							Role = reader.GetString(1),
							LastSeen = DateTimeOffset.FromUnixTimeSeconds(reader.GetInt64(3)).DateTime,
							PointsContribution = reader.GetInt32(4)
						};
						guild.Members[member.PlayerUid] = member;
					}
				}
			}
		}

		// Token: 0x06000843 RID: 2115 RVA: 0x0003A974 File Offset: 0x00038B74
		private void LoadGuildRoles(SqliteConnection connection, int guildId, Guild guild)
		{
			using (SqliteCommand command = new SqliteCommand("\n                SELECT role_name, description, permissions, hierarchy\n                FROM guild_roles\n                WHERE guild_id = @guildId;", connection))
			{
				command.Parameters.AddWithValue("@guildId", guildId);
				using (SqliteDataReader reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						GuildRole role = new GuildRole
						{
							Description = reader.GetString(1),
							Permissions = (GuildPermission)reader.GetInt32(2),
							Hierarchy = reader.GetInt32(3)
						};
						guild.Roles[reader.GetString(0)] = role;
					}
				}
			}
		}

		// Token: 0x06000844 RID: 2116 RVA: 0x0003AA28 File Offset: 0x00038C28
		private void LoadGuildInvites(SqliteConnection connection, int guildId, Guild guild)
		{
			using (SqliteCommand command = new SqliteCommand("\n                SELECT inviter_uid, invitee_uid, created_at, expires_at\n                FROM guild_invites\n                WHERE guild_id = @guildId;", connection))
			{
				command.Parameters.AddWithValue("@guildId", guildId);
				using (SqliteDataReader reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						GuildInvite invite = new GuildInvite
						{
							InviterUid = reader.GetString(0),
							InviteeUid = reader.GetString(1),
							GuildName = guild.Name,
							Timestamp = DateTimeOffset.FromUnixTimeSeconds(reader.GetInt64(2)).DateTime,
							ExpiresAt = DateTimeOffset.FromUnixTimeSeconds(reader.GetInt64(3)).DateTime
						};
						guild.PendingInvites.Add(invite);
					}
				}
			}
		}

		// Token: 0x06000845 RID: 2117 RVA: 0x0003AB08 File Offset: 0x00038D08
		private void LoadLandClaims(SqliteConnection connection, int guildId, Guild guild)
		{
			using (SqliteCommand command = new SqliteCommand("\n                SELECT chunk_x, chunk_z, claim_type, claimed_by_uid, claimed_at, metadata\n                FROM land_claims\n                WHERE guild_id = @guildId;", connection))
			{
				command.Parameters.AddWithValue("@guildId", guildId);
				using (SqliteDataReader reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						int chunkX = reader.GetInt32(0);
						int chunkZ = reader.GetInt32(1);
						string claimType = reader.GetString(2);
						string claimedByUid = reader.GetString(3);
						DateTime claimedAt = DateTimeOffset.FromUnixTimeSeconds(reader.GetInt64(4)).DateTime;
						string metadata = reader.IsDBNull(5) ? null : reader.GetString(5);
						LandClaim landClaim;
						if (!(claimType == "guild_home"))
						{
							if (!(claimType == "outpost"))
							{
								landClaim = new LandClaim
								{
									ChunkX = chunkX,
									ChunkZ = chunkZ,
									ClaimedByUid = claimedByUid,
									Timestamp = claimedAt
								};
							}
							else
							{
								landClaim = this.CreateOutpostClaim(chunkX, chunkZ, claimedByUid, claimedAt, metadata);
							}
						}
						else
						{
							landClaim = this.CreateGuildHomeClaim(chunkX, chunkZ, claimedByUid, claimedAt, metadata);
						}
						LandClaim claim = landClaim;
						guild.Claims.Add(claim);
					}
				}
			}
		}

		// Token: 0x06000846 RID: 2118 RVA: 0x0003AC60 File Offset: 0x00038E60
		private GuildHomeClaim CreateGuildHomeClaim(int chunkX, int chunkZ, string claimedByUid, DateTime claimedAt, [Nullable(2)] string metadata)
		{
			int centerChunkX = chunkX;
			int centerChunkZ = chunkZ;
			if (!string.IsNullOrEmpty(metadata))
			{
				try
				{
					Dictionary<string, int> metadataObj = JsonSerializer.Deserialize<Dictionary<string, int>>(metadata, null);
					if (metadataObj != null)
					{
						int cx;
						if (metadataObj.TryGetValue("center_chunk_x", out cx))
						{
							centerChunkX = cx;
						}
						int cz;
						if (metadataObj.TryGetValue("center_chunk_z", out cz))
						{
							centerChunkZ = cz;
						}
					}
				}
				catch
				{
				}
			}
			return new GuildHomeClaim(centerChunkX, centerChunkZ, claimedByUid)
			{
				Timestamp = claimedAt
			};
		}

		// Token: 0x06000847 RID: 2119 RVA: 0x0003ACD0 File Offset: 0x00038ED0
		private OutpostClaim CreateOutpostClaim(int chunkX, int chunkZ, string claimedByUid, DateTime claimedAt, [Nullable(2)] string metadata)
		{
			string outpostName = "";
			if (!string.IsNullOrEmpty(metadata))
			{
				try
				{
					Dictionary<string, string> metadataObj = JsonSerializer.Deserialize<Dictionary<string, string>>(metadata, null);
					string name;
					if (metadataObj != null && metadataObj.TryGetValue("outpost_name", out name))
					{
						outpostName = (name ?? "");
					}
				}
				catch
				{
				}
			}
			return new OutpostClaim(chunkX, chunkZ, claimedByUid, outpostName)
			{
				Timestamp = claimedAt
			};
		}

		// Token: 0x06000848 RID: 2120 RVA: 0x0003AD38 File Offset: 0x00038F38
		private void LoadTechProgress(SqliteConnection connection, int guildId, Guild guild)
		{
			using (SqliteCommand command = new SqliteCommand("\n                SELECT tech_id, is_unlocked, requires_personal_unlock, unlocked_at\n                FROM guild_tech_progress\n                WHERE guild_id = @guildId;", connection))
			{
				command.Parameters.AddWithValue("@guildId", guildId);
				using (SqliteDataReader reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						int techId = reader.GetInt32(0);
						GuildTechProgress progress = new GuildTechProgress
						{
							TechBlockId = techId,
							IsUnlocked = (reader.GetInt32(1) == 1),
							UnlockedTimestamp = (reader.IsDBNull(3) ? null : new long?(reader.GetInt64(3)))
						};
						guild.TechProgress[techId] = progress;
						guild.TechRequiresPersonalUnlock[techId] = (reader.GetInt32(2) == 1);
						this.LoadTechContributions(connection, guildId, techId, progress);
					}
					this.LoadPlayerTechProgress(connection, guildId, guild);
				}
			}
		}

		// Token: 0x06000849 RID: 2121 RVA: 0x0003AE30 File Offset: 0x00039030
		private void LoadTechContributions(SqliteConnection connection, int guildId, int techId, GuildTechProgress progress)
		{
			using (SqliteCommand command = new SqliteCommand("\n                SELECT resource_group, amount_submitted\n                FROM guild_tech_contributions\n                WHERE guild_id = @guildId AND tech_id = @techId;", connection))
			{
				command.Parameters.AddWithValue("@guildId", guildId);
				command.Parameters.AddWithValue("@techId", techId);
				using (SqliteDataReader reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						progress.ResourceGroupsSubmitted[reader.GetString(0)] = reader.GetInt32(1);
					}
				}
			}
		}

		// Token: 0x0600084A RID: 2122 RVA: 0x0003AED4 File Offset: 0x000390D4
		private void LoadPlayerTechProgress(SqliteConnection connection, int guildId, Guild guild)
		{
			using (SqliteCommand command = new SqliteCommand("\n                SELECT player_uid, tech_id, is_unlocked, unlocked_at\n                FROM guild_member_tech_progress\n                WHERE guild_id = @guildId;", connection))
			{
				command.Parameters.AddWithValue("@guildId", guildId);
				using (SqliteDataReader reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						string playerUid = reader.GetString(0);
						int techId = reader.GetInt32(1);
						guild.GetOrCreatePlayerProgress(playerUid).GetOrCreateUnlock(techId).IsPersonallyUnlocked = (reader.GetInt32(2) == 1);
					}
				}
			}
		}

		// Token: 0x0600084B RID: 2123 RVA: 0x0003AF74 File Offset: 0x00039174
		private void UpsertGuild(SqliteConnection connection, SqliteTransaction transaction, Guild guild)
		{
			int guildId = this.GetOrCreateGuildId(connection, transaction, guild);
			this.UpdateGuildBasicInfo(connection, transaction, guildId, guild);
			this.UpsertGuildMembers(connection, transaction, guildId, guild);
			this.UpsertGuildRoles(connection, transaction, guildId, guild);
			this.UpsertGuildInvites(connection, transaction, guildId, guild);
			this.UpsertLandClaims(connection, transaction, guildId, guild);
			this.UpsertTechProgress(connection, transaction, guildId, guild);
		}

		// Token: 0x0600084C RID: 2124 RVA: 0x0003AFC8 File Offset: 0x000391C8
		private int GetOrCreateGuildId(SqliteConnection connection, SqliteTransaction transaction, Guild guild)
		{
			if (guild.DatabaseId != null)
			{
				return guild.DatabaseId.Value;
			}
			int value;
			using (SqliteCommand selectCommand = new SqliteCommand("SELECT id FROM guilds WHERE name = @name COLLATE NOCASE;", connection))
			{
				selectCommand.Transaction = transaction;
				selectCommand.Parameters.AddWithValue("@name", guild.Name);
				object result = selectCommand.ExecuteScalar();
				if (result != null)
				{
					guild.DatabaseId = new int?(Convert.ToInt32(result));
					value = guild.DatabaseId.Value;
				}
				else
				{
					using (SqliteCommand insertCommand = new SqliteCommand("\n                INSERT INTO guilds (name, description, display_color, secondary_color, points, created_at, updated_at)\n                VALUES (@name, @description, @displayColor, @secondaryColor, @points, @createdAt, @updatedAt);\n                SELECT last_insert_rowid();", connection))
					{
						insertCommand.Transaction = transaction;
						insertCommand.Parameters.AddWithValue("@name", guild.Name);
						insertCommand.Parameters.AddWithValue("@description", guild.Description);
						insertCommand.Parameters.AddWithValue("@displayColor", guild.DisplayColor);
						insertCommand.Parameters.AddWithValue("@secondaryColor", guild.SecondaryColor);
						insertCommand.Parameters.AddWithValue("@points", 0);
						insertCommand.Parameters.AddWithValue("@createdAt", DateTimeOffset.UtcNow.ToUnixTimeSeconds());
						insertCommand.Parameters.AddWithValue("@updatedAt", DateTimeOffset.UtcNow.ToUnixTimeSeconds());
						guild.DatabaseId = new int?(Convert.ToInt32(insertCommand.ExecuteScalar()));
						ILogger logger = this.serverApi.Logger;
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(57, 2);
						defaultInterpolatedStringHandler.AppendLiteral("[GuildRepository] Created new guild '");
						defaultInterpolatedStringHandler.AppendFormatted(guild.Name);
						defaultInterpolatedStringHandler.AppendLiteral("' with database ID: ");
						defaultInterpolatedStringHandler.AppendFormatted<int?>(guild.DatabaseId);
						logger.Debug(defaultInterpolatedStringHandler.ToStringAndClear());
						value = guild.DatabaseId.Value;
					}
				}
			}
			return value;
		}

		// Token: 0x0600084D RID: 2125 RVA: 0x0003B1EC File Offset: 0x000393EC
		private void UpdateGuildBasicInfo(SqliteConnection connection, SqliteTransaction transaction, int guildId, Guild guild)
		{
			using (SqliteCommand command = new SqliteCommand("\n                UPDATE guilds \n                SET description = @description,\n                    display_color = @displayColor,\n                    secondary_color = @secondaryColor,\n                    points = @points,\n                    updated_at = @updatedAt\n                WHERE id = @guildId;", connection))
			{
				command.Transaction = transaction;
				command.Parameters.AddWithValue("@guildId", guildId);
				command.Parameters.AddWithValue("@description", guild.Description);
				command.Parameters.AddWithValue("@displayColor", guild.DisplayColor);
				command.Parameters.AddWithValue("@secondaryColor", guild.SecondaryColor);
				command.Parameters.AddWithValue("@points", guild.Points);
				command.Parameters.AddWithValue("@updatedAt", DateTimeOffset.UtcNow.ToUnixTimeSeconds());
				command.ExecuteNonQuery();
			}
		}

		// Token: 0x0600084E RID: 2126 RVA: 0x0003B2D8 File Offset: 0x000394D8
		private void UpsertGuildMembers(SqliteConnection connection, SqliteTransaction transaction, int guildId, Guild guild)
		{
			HashSet<string> existingUids = new HashSet<string>();
			using (SqliteCommand selectCommand = new SqliteCommand("SELECT player_uid FROM guild_members WHERE guild_id = @guildId;", connection))
			{
				selectCommand.Transaction = transaction;
				selectCommand.Parameters.AddWithValue("@guildId", guildId);
				using (SqliteDataReader reader = selectCommand.ExecuteReader())
				{
					while (reader.Read())
					{
						existingUids.Add(reader.GetString(0));
					}
				}
			}
			HashSet<string> currentUids = guild.Members.Keys.ToHashSet<string>();
			List<string> removedUids = existingUids.Except(currentUids).ToList<string>();
			if (removedUids.Count > 0)
			{
				foreach (string uid in removedUids)
				{
					using (SqliteCommand deleteCommand = new SqliteCommand("DELETE FROM guild_members WHERE guild_id = @guildId AND player_uid = @playerUid;", connection))
					{
						deleteCommand.Transaction = transaction;
						deleteCommand.Parameters.AddWithValue("@guildId", guildId);
						deleteCommand.Parameters.AddWithValue("@playerUid", uid);
						deleteCommand.ExecuteNonQuery();
					}
				}
			}
			foreach (GuildMember member in guild.Members.Values)
			{
				using (SqliteCommand upsertCommand = new SqliteCommand("\n                    INSERT INTO guild_members (guild_id, player_uid, role, joined_at, last_seen, points_contribution)\n                    VALUES (@guildId, @playerUid, @role, @joinedAt, @lastSeen, @pointsContribution)\n                    ON CONFLICT(guild_id, player_uid) DO UPDATE SET\n                        role = excluded.role,\n                        last_seen = excluded.last_seen,\n                        points_contribution = excluded.points_contribution;", connection))
				{
					upsertCommand.Transaction = transaction;
					upsertCommand.Parameters.AddWithValue("@guildId", guildId);
					upsertCommand.Parameters.AddWithValue("@playerUid", member.PlayerUid);
					upsertCommand.Parameters.AddWithValue("@role", member.Role);
					upsertCommand.Parameters.AddWithValue("@joinedAt", DateTimeOffset.UtcNow.ToUnixTimeSeconds());
					upsertCommand.Parameters.AddWithValue("@lastSeen", new DateTimeOffset(member.LastSeen).ToUnixTimeSeconds());
					upsertCommand.Parameters.AddWithValue("@pointsContribution", member.PointsContribution);
					upsertCommand.ExecuteNonQuery();
				}
			}
		}

		// Token: 0x0600084F RID: 2127 RVA: 0x0003B5B4 File Offset: 0x000397B4
		private void UpsertGuildRoles(SqliteConnection connection, SqliteTransaction transaction, int guildId, Guild guild)
		{
			using (SqliteCommand deleteCommand = new SqliteCommand("DELETE FROM guild_roles WHERE guild_id = @guildId;", connection))
			{
				deleteCommand.Transaction = transaction;
				deleteCommand.Parameters.AddWithValue("@guildId", guildId);
				deleteCommand.ExecuteNonQuery();
			}
			foreach (KeyValuePair<string, GuildRole> roleKvp in guild.Roles)
			{
				using (SqliteCommand insertCommand = new SqliteCommand("\n                    INSERT INTO guild_roles (guild_id, role_name, description, permissions, hierarchy)\n                    VALUES (@guildId, @roleName, @description, @permissions, @hierarchy);", connection))
				{
					insertCommand.Transaction = transaction;
					insertCommand.Parameters.AddWithValue("@guildId", guildId);
					insertCommand.Parameters.AddWithValue("@roleName", roleKvp.Key);
					insertCommand.Parameters.AddWithValue("@description", roleKvp.Value.Description);
					insertCommand.Parameters.AddWithValue("@permissions", (int)roleKvp.Value.Permissions);
					insertCommand.Parameters.AddWithValue("@hierarchy", roleKvp.Value.Hierarchy);
					insertCommand.ExecuteNonQuery();
				}
			}
		}

		// Token: 0x06000850 RID: 2128 RVA: 0x0003B714 File Offset: 0x00039914
		private void UpsertGuildInvites(SqliteConnection connection, SqliteTransaction transaction, int guildId, Guild guild)
		{
			using (SqliteCommand deleteCommand = new SqliteCommand("DELETE FROM guild_invites WHERE guild_id = @guildId;", connection))
			{
				deleteCommand.Transaction = transaction;
				deleteCommand.Parameters.AddWithValue("@guildId", guildId);
				deleteCommand.ExecuteNonQuery();
			}
			foreach (GuildInvite invite in guild.PendingInvites)
			{
				using (SqliteCommand insertCommand = new SqliteCommand("\n                    INSERT INTO guild_invites (guild_id, inviter_uid, invitee_uid, created_at, expires_at)\n                    VALUES (@guildId, @inviterUid, @inviteeUid, @createdAt, @expiresAt);", connection))
				{
					insertCommand.Transaction = transaction;
					insertCommand.Parameters.AddWithValue("@guildId", guildId);
					insertCommand.Parameters.AddWithValue("@inviterUid", invite.InviterUid);
					insertCommand.Parameters.AddWithValue("@inviteeUid", invite.InviteeUid);
					insertCommand.Parameters.AddWithValue("@createdAt", new DateTimeOffset(invite.Timestamp).ToUnixTimeSeconds());
					insertCommand.Parameters.AddWithValue("@expiresAt", new DateTimeOffset(invite.ExpiresAt).ToUnixTimeSeconds());
					insertCommand.ExecuteNonQuery();
				}
			}
		}

		// Token: 0x06000851 RID: 2129 RVA: 0x0003B87C File Offset: 0x00039A7C
		private void UpsertLandClaims(SqliteConnection connection, SqliteTransaction transaction, int guildId, Guild guild)
		{
			using (SqliteCommand deleteCommand = new SqliteCommand("DELETE FROM land_claims WHERE guild_id = @guildId;", connection))
			{
				deleteCommand.Transaction = transaction;
				deleteCommand.Parameters.AddWithValue("@guildId", guildId);
				deleteCommand.ExecuteNonQuery();
			}
			foreach (LandClaim claim in guild.Claims)
			{
				string text;
				if (!(claim is GuildHomeClaim))
				{
					if (!(claim is OutpostClaim))
					{
						text = "regular";
					}
					else
					{
						text = "outpost";
					}
				}
				else
				{
					text = "guild_home";
				}
				string claimType = text;
				string metadata = null;
				GuildHomeClaim guildHome = claim as GuildHomeClaim;
				if (guildHome != null)
				{
					metadata = JsonSerializer.Serialize(new
					{
						center_chunk_x = guildHome.CenterChunkX,
						center_chunk_z = guildHome.CenterChunkZ
					}, null);
				}
				else
				{
					OutpostClaim outpost = claim as OutpostClaim;
					if (outpost != null)
					{
						metadata = JsonSerializer.Serialize(new
						{
							outpost_name = outpost.OutpostName
						}, null);
					}
				}
				using (SqliteCommand insertCommand = new SqliteCommand("\n                    INSERT INTO land_claims (guild_id, chunk_x, chunk_z, claim_type, claimed_by_uid, claimed_at, metadata)\n                    VALUES (@guildId, @chunkX, @chunkZ, @claimType, @claimedByUid, @claimedAt, @metadata)\n                    ON CONFLICT(chunk_x, chunk_z) DO UPDATE SET\n                        guild_id = excluded.guild_id,\n                        claim_type = excluded.claim_type,\n                        claimed_by_uid = excluded.claimed_by_uid,\n                        claimed_at = excluded.claimed_at,\n                        metadata = excluded.metadata;", connection))
				{
					insertCommand.Transaction = transaction;
					insertCommand.Parameters.AddWithValue("@guildId", guildId);
					insertCommand.Parameters.AddWithValue("@chunkX", claim.ChunkX);
					insertCommand.Parameters.AddWithValue("@chunkZ", claim.ChunkZ);
					insertCommand.Parameters.AddWithValue("@claimType", claimType);
					insertCommand.Parameters.AddWithValue("@claimedByUid", claim.ClaimedByUid ?? "");
					insertCommand.Parameters.AddWithValue("@claimedAt", new DateTimeOffset(claim.Timestamp).ToUnixTimeSeconds());
					insertCommand.Parameters.AddWithValue("@metadata", metadata ?? DBNull.Value);
					insertCommand.ExecuteNonQuery();
				}
			}
		}

		// Token: 0x06000852 RID: 2130 RVA: 0x0003BABC File Offset: 0x00039CBC
		private void UpsertTechProgress(SqliteConnection connection, SqliteTransaction transaction, int guildId, Guild guild)
		{
			using (SqliteCommand deleteCommand = new SqliteCommand("DELETE FROM guild_tech_progress WHERE guild_id = @guildId;", connection))
			{
				deleteCommand.Transaction = transaction;
				deleteCommand.Parameters.AddWithValue("@guildId", guildId);
				deleteCommand.ExecuteNonQuery();
			}
			foreach (KeyValuePair<int, GuildTechProgress> techProgressKvp in guild.TechProgress)
			{
				int techId = techProgressKvp.Key;
				GuildTechProgress progress = techProgressKvp.Value;
				bool requiresPersonalUnlock = guild.TechRequiresPersonalUnlock.GetValueOrDefault(techId, false);
				using (SqliteCommand insertCommand = new SqliteCommand("\n                    INSERT INTO guild_tech_progress (guild_id, tech_id, is_unlocked, requires_personal_unlock, unlocked_at)\n                    VALUES (@guildId, @techId, @isUnlocked, @requiresPersonalUnlock, @unlockedAt);", connection))
				{
					insertCommand.Transaction = transaction;
					insertCommand.Parameters.AddWithValue("@guildId", guildId);
					insertCommand.Parameters.AddWithValue("@techId", techId);
					insertCommand.Parameters.AddWithValue("@isUnlocked", (progress.IsUnlocked > false) ? 1 : 0);
					insertCommand.Parameters.AddWithValue("@requiresPersonalUnlock", (requiresPersonalUnlock > false) ? 1 : 0);
					insertCommand.Parameters.AddWithValue("@unlockedAt", progress.UnlockedTimestamp ?? DBNull.Value);
					insertCommand.ExecuteNonQuery();
					foreach (KeyValuePair<string, int> contributionKvp in progress.ResourceGroupsSubmitted)
					{
						using (SqliteCommand contributionCommand = new SqliteCommand("\n                        INSERT INTO guild_tech_contributions (guild_id, tech_id, resource_group, amount_submitted)\n                        VALUES (@guildId, @techId, @resourceGroup, @amountSubmitted);", connection))
						{
							contributionCommand.Transaction = transaction;
							contributionCommand.Parameters.AddWithValue("@guildId", guildId);
							contributionCommand.Parameters.AddWithValue("@techId", techId);
							contributionCommand.Parameters.AddWithValue("@resourceGroup", contributionKvp.Key);
							contributionCommand.Parameters.AddWithValue("@amountSubmitted", contributionKvp.Value);
							contributionCommand.ExecuteNonQuery();
						}
					}
				}
			}
			this.UpsertPlayerTechProgress(connection, transaction, guildId, guild);
		}

		// Token: 0x06000853 RID: 2131 RVA: 0x0003BD6C File Offset: 0x00039F6C
		private void UpsertPlayerTechProgress(SqliteConnection connection, SqliteTransaction transaction, int guildId, Guild guild)
		{
			using (SqliteCommand deleteCommand = new SqliteCommand("DELETE FROM guild_member_tech_progress WHERE guild_id = @guildId;", connection))
			{
				deleteCommand.Transaction = transaction;
				deleteCommand.Parameters.AddWithValue("@guildId", guildId);
				deleteCommand.ExecuteNonQuery();
			}
			foreach (KeyValuePair<string, PlayerTechProgress> playerProgressKvp in guild.PlayerTechProgress)
			{
				string playerUid = playerProgressKvp.Key;
				foreach (KeyValuePair<int, PersonalTechUnlock> unlockKvp in playerProgressKvp.Value.PersonalUnlocks)
				{
					int techId = unlockKvp.Key;
					PersonalTechUnlock unlock = unlockKvp.Value;
					using (SqliteCommand insertCommand = new SqliteCommand("\n                        INSERT INTO guild_member_tech_progress (guild_id, player_uid, tech_id, is_unlocked, unlocked_at)\n                        VALUES (@guildId, @playerUid, @techId, @isUnlocked, @unlockedAt);", connection))
					{
						insertCommand.Transaction = transaction;
						insertCommand.Parameters.AddWithValue("@guildId", guildId);
						insertCommand.Parameters.AddWithValue("@playerUid", playerUid);
						insertCommand.Parameters.AddWithValue("@techId", techId);
						insertCommand.Parameters.AddWithValue("@isUnlocked", (unlock.IsPersonallyUnlocked > false) ? 1 : 0);
						insertCommand.Parameters.AddWithValue("@unlockedAt", DBNull.Value);
						insertCommand.ExecuteNonQuery();
					}
				}
			}
		}

		// Token: 0x06000854 RID: 2132 RVA: 0x0003BF50 File Offset: 0x0003A150
		private void DeleteGuildFromDatabase(SqliteConnection connection, SqliteTransaction transaction, string guildName)
		{
			using (SqliteCommand command = new SqliteCommand("DELETE FROM guilds WHERE name = @name COLLATE NOCASE;", connection))
			{
				command.Transaction = transaction;
				command.Parameters.AddWithValue("@name", guildName);
				command.ExecuteNonQuery();
				this.serverApi.Logger.Debug("[GuildRepository] Deleted guild '" + guildName + "' from database (cascaded to all related data)");
			}
		}

		// Token: 0x0400035D RID: 861
		private readonly ICoreServerAPI serverApi = serverApi;

		// Token: 0x0400035E RID: 862
		private readonly GuildDatabase database = database;

		// Token: 0x0400035F RID: 863
		private readonly Dictionary<string, Guild> guildCache = new Dictionary<string, Guild>();

		// Token: 0x04000360 RID: 864
		private readonly HashSet<string> dirtyGuilds = new HashSet<string>();

		// Token: 0x04000361 RID: 865
		private bool cacheLoaded;
	}
}
