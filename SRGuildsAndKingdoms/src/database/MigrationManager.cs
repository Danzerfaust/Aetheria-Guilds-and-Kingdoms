using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Microsoft.Data.Sqlite;
using SRGuildsAndKingdoms.src.guilds;
using SRGuildsAndKingdoms.src.techblock;
using Vintagestory.API.Common;
using Vintagestory.API.Server;

namespace SRGuildsAndKingdoms.src.database
{
	// Token: 0x020000B6 RID: 182
	[NullableContext(1)]
	[Nullable(0)]
	public class MigrationManager
	{
		// Token: 0x06000860 RID: 2144 RVA: 0x0003C460 File Offset: 0x0003A660
		public MigrationManager(ICoreServerAPI serverApi, GuildDatabase database)
		{
			this.serverApi = serverApi;
			this.database = database;
			this.dataFolder = serverApi.GetOrCreateDataPath("ModData/SRGuildsAndKingdoms");
			this.guildsJsonPath = Path.Combine(this.dataFolder, "guilds.json");
			this.cooldownJsonPath = Path.Combine(this.dataFolder, "guild_cooldowns.json");
			this.zoneWhitelistJsonPath = Path.Combine(this.dataFolder, "zone_whitelist.json");
		}

		// Token: 0x06000861 RID: 2145 RVA: 0x0003C4D4 File Offset: 0x0003A6D4
		public bool NeedsMigration()
		{
			bool result;
			try
			{
				bool hasGuildsJson = File.Exists(this.guildsJsonPath);
				bool hasCooldownJson = File.Exists(this.cooldownJsonPath);
				bool hasWhitelistJson = File.Exists(this.zoneWhitelistJsonPath);
				if (!hasGuildsJson && !hasCooldownJson && !hasWhitelistJson)
				{
					result = false;
				}
				else
				{
					bool isDatabaseEmpty = this.IsDatabaseEmpty();
					if (hasGuildsJson && !isDatabaseEmpty)
					{
						this.serverApi.Logger.Warning("[SRGuildsAndKingdoms:MigrationManager] Both JSON and DB exist - delete guilds.db if you wanna remigrate, otherwise delete jsons");
						result = false;
					}
					else
					{
						ILogger logger = this.serverApi.Logger;
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(93, 3);
						defaultInterpolatedStringHandler.AppendLiteral("[SRGuildsAndKingdoms:MigrationManager] Migration needed for: guilds=");
						defaultInterpolatedStringHandler.AppendFormatted<bool>(hasGuildsJson);
						defaultInterpolatedStringHandler.AppendLiteral(", cooldowns=");
						defaultInterpolatedStringHandler.AppendFormatted<bool>(hasCooldownJson);
						defaultInterpolatedStringHandler.AppendLiteral(", whitelists=");
						defaultInterpolatedStringHandler.AppendFormatted<bool>(hasWhitelistJson);
						logger.Notification(defaultInterpolatedStringHandler.ToStringAndClear());
						result = (hasGuildsJson || hasCooldownJson || hasWhitelistJson);
					}
				}
			}
			catch (Exception ex)
			{
				this.serverApi.Logger.Error("[SRGuildsAndKingdoms:MigrationManager] Error checking migration status: " + ex.Message);
				result = false;
			}
			return result;
		}

		// Token: 0x06000862 RID: 2146 RVA: 0x0003C5E0 File Offset: 0x0003A7E0
		public MigrationResult MigrateFromJson()
		{
			MigrationResult result = new MigrationResult
			{
				StartTime = DateTime.UtcNow
			};
			MigrationResult result2;
			try
			{
				this.serverApi.Logger.Notification("[SRGuildsAndKingdoms:MigrationManager] ========================================");
				this.serverApi.Logger.Notification("[SRGuildsAndKingdoms:MigrationManager] Starting migration from JSON to SQLite");
				this.serverApi.Logger.Notification("[SRGuildsAndKingdoms:MigrationManager] ========================================");
				this.serverApi.Logger.Notification("[SRGuildsAndKingdoms:MigrationManager] Step 1: Creating backups of JSON files...");
				this.CreateBackups(result);
				if (File.Exists(this.guildsJsonPath))
				{
					this.serverApi.Logger.Notification("[SRGuildsAndKingdoms:MigrationManager] Step 2: Migrating guild data...");
					this.MigrateGuilds(result);
				}
				else
				{
					this.serverApi.Logger.Notification("[SRGuildsAndKingdoms:MigrationManager] Step 2: Skipped - no guilds.json found");
				}
				if (File.Exists(this.cooldownJsonPath))
				{
					this.serverApi.Logger.Notification("[SRGuildsAndKingdoms:MigrationManager] Step 3: Migrating cooldown data...");
					this.MigrateCooldowns(result);
				}
				else
				{
					this.serverApi.Logger.Notification("[SRGuildsAndKingdoms:MigrationManager] Step 3: Skipped - no guild_cooldowns.json found");
				}
				if (File.Exists(this.zoneWhitelistJsonPath))
				{
					this.serverApi.Logger.Notification("[SRGuildsAndKingdoms:MigrationManager] Step 4: Migrating zone whitelist data...");
					this.MigrateZoneWhitelists(result);
				}
				else
				{
					this.serverApi.Logger.Notification("[SRGuildsAndKingdoms:MigrationManager] Step 4: Skipped - no zone_whitelist.json found");
				}
				this.serverApi.Logger.Notification("[SRGuildsAndKingdoms:MigrationManager] Step 5: Validating migration...");
				this.ValidateMigration(result);
				result.EndTime = DateTime.UtcNow;
				result.Success = !result.Errors.Any<string>();
				this.serverApi.Logger.Notification("[SRGuildsAndKingdoms:MigrationManager] ========================================");
				ILogger logger = this.serverApi.Logger;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(70, 1);
				defaultInterpolatedStringHandler.AppendLiteral("[SRGuildsAndKingdoms:MigrationManager] Migration completed in ");
				defaultInterpolatedStringHandler.AppendFormatted<double>(result.Duration.TotalSeconds, "F2");
				defaultInterpolatedStringHandler.AppendLiteral(" seconds");
				logger.Notification(defaultInterpolatedStringHandler.ToStringAndClear());
				ILogger logger2 = this.serverApi.Logger;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(48, 1);
				defaultInterpolatedStringHandler2.AppendLiteral("[SRGuildsAndKingdoms:MigrationManager] Success: ");
				defaultInterpolatedStringHandler2.AppendFormatted<bool>(result.Success);
				logger2.Notification(defaultInterpolatedStringHandler2.ToStringAndClear());
				ILogger logger3 = this.serverApi.Logger;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler3 = new DefaultInterpolatedStringHandler(56, 1);
				defaultInterpolatedStringHandler3.AppendLiteral("[SRGuildsAndKingdoms:MigrationManager] Guilds migrated: ");
				defaultInterpolatedStringHandler3.AppendFormatted<int>(result.GuildsMigrated);
				logger3.Notification(defaultInterpolatedStringHandler3.ToStringAndClear());
				ILogger logger4 = this.serverApi.Logger;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler4 = new DefaultInterpolatedStringHandler(59, 1);
				defaultInterpolatedStringHandler4.AppendLiteral("[SRGuildsAndKingdoms:MigrationManager] Cooldowns migrated: ");
				defaultInterpolatedStringHandler4.AppendFormatted<int>(result.CooldownsMigrated);
				logger4.Notification(defaultInterpolatedStringHandler4.ToStringAndClear());
				ILogger logger5 = this.serverApi.Logger;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler5 = new DefaultInterpolatedStringHandler(65, 1);
				defaultInterpolatedStringHandler5.AppendLiteral("[SRGuildsAndKingdoms:MigrationManager] Zone whitelists migrated: ");
				defaultInterpolatedStringHandler5.AppendFormatted<int>(result.ZoneWhitelistsMigrated);
				logger5.Notification(defaultInterpolatedStringHandler5.ToStringAndClear());
				if (result.Errors.Any<string>())
				{
					ILogger logger6 = this.serverApi.Logger;
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler6 = new DefaultInterpolatedStringHandler(59, 1);
					defaultInterpolatedStringHandler6.AppendLiteral("[SRGuildsAndKingdoms:MigrationManager] Errors encountered: ");
					defaultInterpolatedStringHandler6.AppendFormatted<int>(result.Errors.Count);
					logger6.Error(defaultInterpolatedStringHandler6.ToStringAndClear());
					foreach (string error in result.Errors)
					{
						this.serverApi.Logger.Error("[SRGuildsAndKingdoms:MigrationManager]   - " + error);
					}
				}
				if (result.Warnings.Any<string>())
				{
					ILogger logger7 = this.serverApi.Logger;
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler7 = new DefaultInterpolatedStringHandler(49, 1);
					defaultInterpolatedStringHandler7.AppendLiteral("[SRGuildsAndKingdoms:MigrationManager] Warnings: ");
					defaultInterpolatedStringHandler7.AppendFormatted<int>(result.Warnings.Count);
					logger7.Warning(defaultInterpolatedStringHandler7.ToStringAndClear());
					foreach (string warning in result.Warnings)
					{
						this.serverApi.Logger.Warning("[SRGuildsAndKingdoms:MigrationManager]   - " + warning);
					}
				}
				this.serverApi.Logger.Notification("[SRGuildsAndKingdoms:MigrationManager] ========================================");
				result2 = result;
			}
			catch (Exception ex)
			{
				result.Success = false;
				result.EndTime = DateTime.UtcNow;
				result.Errors.Add("Migration failed with exception: " + ex.Message);
				this.serverApi.Logger.Error("[SRGuildsAndKingdoms:MigrationManager] Migration failed: " + ex.Message);
				this.serverApi.Logger.Error("[SRGuildsAndKingdoms:MigrationManager] Stack trace: " + ex.StackTrace);
				result2 = result;
			}
			return result2;
		}

		// Token: 0x06000863 RID: 2147 RVA: 0x0003CABC File Offset: 0x0003ACBC
		private void CreateBackups(MigrationResult result)
		{
			string backupFolder = Path.Combine(this.dataFolder, "json_backups");
			Directory.CreateDirectory(backupFolder);
			string timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
			if (File.Exists(this.guildsJsonPath))
			{
				string backupPath = Path.Combine(backupFolder, "guilds_" + timestamp + ".json");
				File.Copy(this.guildsJsonPath, backupPath, true);
				result.BackupFiles.Add(backupPath);
				this.serverApi.Logger.Notification("[SRGuildsAndKingdoms:MigrationManager] Created backup: " + backupPath);
			}
			if (File.Exists(this.cooldownJsonPath))
			{
				string backupPath2 = Path.Combine(backupFolder, "guild_cooldowns_" + timestamp + ".json");
				File.Copy(this.cooldownJsonPath, backupPath2, true);
				result.BackupFiles.Add(backupPath2);
				this.serverApi.Logger.Notification("[SRGuildsAndKingdoms:MigrationManager] Created backup: " + backupPath2);
			}
			if (File.Exists(this.zoneWhitelistJsonPath))
			{
				string backupPath3 = Path.Combine(backupFolder, "zone_whitelist_" + timestamp + ".json");
				File.Copy(this.zoneWhitelistJsonPath, backupPath3, true);
				result.BackupFiles.Add(backupPath3);
				this.serverApi.Logger.Notification("[SRGuildsAndKingdoms:MigrationManager] Created backup: " + backupPath3);
			}
		}

		// Token: 0x06000864 RID: 2148 RVA: 0x0003CC04 File Offset: 0x0003AE04
		private void MigrateGuilds(MigrationResult result)
		{
			try
			{
				string json = File.ReadAllText(this.guildsJsonPath);
				JsonSerializerOptions options = new JsonSerializerOptions
				{
					DefaultIgnoreCondition = JsonIgnoreCondition.Never,
					PropertyNameCaseInsensitive = true
				};
				options.TypeInfoResolver = JsonTypeInfoResolver.Combine(new IJsonTypeInfoResolver[]
				{
					new DefaultJsonTypeInfoResolver(),
					new MigrationManager.LandClaimPolymorphicResolver()
				});
				Dictionary<string, Guild> guilds = JsonSerializer.Deserialize<Dictionary<string, Guild>>(json, options);
				if (guilds == null || guilds.Count == 0)
				{
					result.Warnings.Add("No guilds found in guilds.json");
				}
				else
				{
					ILogger logger = this.serverApi.Logger;
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(63, 1);
					defaultInterpolatedStringHandler.AppendLiteral("[SRGuildsAndKingdoms:MigrationManager] Loaded ");
					defaultInterpolatedStringHandler.AppendFormatted<int>(guilds.Count);
					defaultInterpolatedStringHandler.AppendLiteral(" guilds from JSON");
					logger.Notification(defaultInterpolatedStringHandler.ToStringAndClear());
					new GuildRepository(this.serverApi, this.database);
					SqliteConnection connection = this.database.Connection;
					using (SqliteTransaction transaction = connection.BeginTransaction())
					{
						try
						{
							foreach (KeyValuePair<string, Guild> guildKvp in guilds)
							{
								Guild guild = guildKvp.Value;
								try
								{
									if (!guild.Roles.ContainsKey("Leader"))
									{
										guild.Roles["Leader"] = new GuildRole
										{
											Description = "Leader",
											Permissions = (GuildPermission.Invite | GuildPermission.Promote | GuildPermission.Kick | GuildPermission.ManageRoles | GuildPermission.BreakAndPlaceBlocks | GuildPermission.InteractBlocks),
											Hierarchy = 1
										};
									}
									if (!guild.Roles.ContainsKey("Member"))
									{
										guild.Roles["Member"] = new GuildRole
										{
											Description = "Member",
											Permissions = (GuildPermission.BreakAndPlaceBlocks | GuildPermission.InteractBlocks),
											Hierarchy = 100
										};
									}
									foreach (GuildHomeClaim claim in guild.Claims.OfType<GuildHomeClaim>())
									{
										if (claim.HomeChunks == null || claim.HomeChunks.Count == 0)
										{
											claim.GenerateHomeChunks();
										}
										claim.IsGuildHome = true;
									}
									foreach (OutpostClaim outpostClaim in guild.Claims.OfType<OutpostClaim>())
									{
										outpostClaim.IsOutpost = true;
									}
									this.InsertGuildDirect(connection, transaction, guild);
									int guildsMigrated = result.GuildsMigrated;
									result.GuildsMigrated = guildsMigrated + 1;
									ILogger logger2 = this.serverApi.Logger;
									DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(75, 3);
									defaultInterpolatedStringHandler2.AppendLiteral("[SRGuildsAndKingdoms:MigrationManager] Migrated guild: ");
									defaultInterpolatedStringHandler2.AppendFormatted(guild.Name);
									defaultInterpolatedStringHandler2.AppendLiteral(" (");
									defaultInterpolatedStringHandler2.AppendFormatted<int>(guild.Members.Count);
									defaultInterpolatedStringHandler2.AppendLiteral(" members, ");
									defaultInterpolatedStringHandler2.AppendFormatted<int>(guild.Claims.Count);
									defaultInterpolatedStringHandler2.AppendLiteral(" claims)");
									logger2.Debug(defaultInterpolatedStringHandler2.ToStringAndClear());
								}
								catch (Exception ex)
								{
									result.Errors.Add("Failed to migrate guild '" + guild.Name + "': " + ex.Message);
									this.serverApi.Logger.Error("[SRGuildsAndKingdoms:MigrationManager] Error migrating guild '" + guild.Name + "': " + ex.Message);
								}
							}
							transaction.Commit();
							ILogger logger3 = this.serverApi.Logger;
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler3 = new DefaultInterpolatedStringHandler(68, 1);
							defaultInterpolatedStringHandler3.AppendLiteral("[SRGuildsAndKingdoms:MigrationManager] Successfully migrated ");
							defaultInterpolatedStringHandler3.AppendFormatted<int>(result.GuildsMigrated);
							defaultInterpolatedStringHandler3.AppendLiteral(" guilds");
							logger3.Notification(defaultInterpolatedStringHandler3.ToStringAndClear());
						}
						catch (Exception ex2)
						{
							transaction.Rollback();
							throw new Exception("Transaction failed during guild migration: " + ex2.Message, ex2);
						}
					}
				}
			}
			catch (Exception ex3)
			{
				result.Errors.Add("Guild migration failed: " + ex3.Message);
				throw;
			}
		}

		// Token: 0x06000865 RID: 2149 RVA: 0x0003D088 File Offset: 0x0003B288
		private void InsertGuildDirect(SqliteConnection connection, SqliteTransaction transaction, Guild guild)
		{
			long guildId;
			using (SqliteCommand command = new SqliteCommand("\n                INSERT INTO guilds (name, description, display_color, secondary_color, points, created_at, updated_at)\n                VALUES (@name, @description, @displayColor, @secondaryColor, @points, @createdAt, @updatedAt);\n                SELECT last_insert_rowid();", connection, transaction))
			{
				command.Parameters.AddWithValue("@name", guild.Name);
				command.Parameters.AddWithValue("@description", guild.Description);
				command.Parameters.AddWithValue("@displayColor", guild.DisplayColor);
				command.Parameters.AddWithValue("@secondaryColor", guild.SecondaryColor);
				command.Parameters.AddWithValue("@points", guild.Points);
				command.Parameters.AddWithValue("@createdAt", DateTimeOffset.UtcNow.ToUnixTimeSeconds());
				command.Parameters.AddWithValue("@updatedAt", DateTimeOffset.UtcNow.ToUnixTimeSeconds());
				guildId = (long)command.ExecuteScalar();
			}
			foreach (GuildMember member in guild.Members.Values)
			{
				using (SqliteCommand command2 = new SqliteCommand("\n                    INSERT INTO guild_members (guild_id, player_uid, role, joined_at, last_seen)\n                    VALUES (@guildId, @playerUid, @role, @joinedAt, @lastSeen);", connection, transaction))
				{
					command2.Parameters.AddWithValue("@guildId", guildId);
					command2.Parameters.AddWithValue("@playerUid", member.PlayerUid);
					command2.Parameters.AddWithValue("@role", member.Role);
					command2.Parameters.AddWithValue("@joinedAt", DateTimeOffset.UtcNow.ToUnixTimeSeconds());
					command2.Parameters.AddWithValue("@lastSeen", new DateTimeOffset(member.LastSeen).ToUnixTimeSeconds());
					command2.ExecuteNonQuery();
				}
			}
			foreach (KeyValuePair<string, GuildRole> roleKvp in guild.Roles)
			{
				using (SqliteCommand command3 = new SqliteCommand("\n                    INSERT INTO guild_roles (guild_id, role_name, description, permissions, hierarchy)\n                    VALUES (@guildId, @roleName, @description, @permissions, @hierarchy);", connection, transaction))
				{
					command3.Parameters.AddWithValue("@guildId", guildId);
					command3.Parameters.AddWithValue("@roleName", roleKvp.Key);
					command3.Parameters.AddWithValue("@description", roleKvp.Value.Description);
					command3.Parameters.AddWithValue("@permissions", (int)roleKvp.Value.Permissions);
					command3.Parameters.AddWithValue("@hierarchy", roleKvp.Value.Hierarchy);
					command3.ExecuteNonQuery();
				}
			}
			foreach (GuildInvite invite in guild.PendingInvites)
			{
				using (SqliteCommand command4 = new SqliteCommand("\n                    INSERT INTO guild_invites (guild_id, inviter_uid, invitee_uid, created_at, expires_at)\n                    VALUES (@guildId, @inviterUid, @inviteeUid, @createdAt, @expiresAt);", connection, transaction))
				{
					command4.Parameters.AddWithValue("@guildId", guildId);
					command4.Parameters.AddWithValue("@inviterUid", invite.InviterUid);
					command4.Parameters.AddWithValue("@inviteeUid", invite.InviteeUid);
					command4.Parameters.AddWithValue("@createdAt", new DateTimeOffset(invite.Timestamp).ToUnixTimeSeconds());
					command4.Parameters.AddWithValue("@expiresAt", new DateTimeOffset(invite.ExpiresAt).ToUnixTimeSeconds());
					command4.ExecuteNonQuery();
				}
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
				using (SqliteCommand command5 = new SqliteCommand("\n                    INSERT INTO land_claims (guild_id, chunk_x, chunk_z, claim_type, claimed_by_uid, claimed_at, metadata)\n                    VALUES (@guildId, @chunkX, @chunkZ, @claimType, @claimedByUid, @claimedAt, @metadata);", connection, transaction))
				{
					command5.Parameters.AddWithValue("@guildId", guildId);
					command5.Parameters.AddWithValue("@chunkX", claim.ChunkX);
					command5.Parameters.AddWithValue("@chunkZ", claim.ChunkZ);
					command5.Parameters.AddWithValue("@claimType", claimType);
					command5.Parameters.AddWithValue("@claimedByUid", claim.ClaimedByUid ?? "");
					command5.Parameters.AddWithValue("@claimedAt", new DateTimeOffset(claim.Timestamp).ToUnixTimeSeconds());
					command5.Parameters.AddWithValue("@metadata", metadata ?? DBNull.Value);
					command5.ExecuteNonQuery();
				}
			}
			foreach (KeyValuePair<int, GuildTechProgress> techProgressKvp in guild.TechProgress)
			{
				int techId = techProgressKvp.Key;
				GuildTechProgress progress = techProgressKvp.Value;
				bool requiresPersonalUnlock = guild.TechRequiresPersonalUnlock.GetValueOrDefault(techId, false);
				using (SqliteCommand command6 = new SqliteCommand("\n                    INSERT INTO guild_tech_progress (guild_id, tech_id, is_unlocked, requires_personal_unlock, unlocked_at)\n                    VALUES (@guildId, @techId, @isUnlocked, @requiresPersonalUnlock, @unlockedAt);", connection, transaction))
				{
					command6.Parameters.AddWithValue("@guildId", guildId);
					command6.Parameters.AddWithValue("@techId", techId);
					command6.Parameters.AddWithValue("@isUnlocked", (progress.IsUnlocked > false) ? 1 : 0);
					command6.Parameters.AddWithValue("@requiresPersonalUnlock", (requiresPersonalUnlock > false) ? 1 : 0);
					command6.Parameters.AddWithValue("@unlockedAt", progress.UnlockedTimestamp ?? DBNull.Value);
					command6.ExecuteNonQuery();
					foreach (KeyValuePair<string, int> contributionKvp in progress.ResourceGroupsSubmitted)
					{
						using (SqliteCommand contributionCommand = new SqliteCommand("\n                        INSERT INTO guild_tech_contributions (guild_id, tech_id, resource_group, amount_submitted)\n                        VALUES (@guildId, @techId, @resourceGroup, @amountSubmitted);", connection, transaction))
						{
							contributionCommand.Parameters.AddWithValue("@guildId", guildId);
							contributionCommand.Parameters.AddWithValue("@techId", techId);
							contributionCommand.Parameters.AddWithValue("@resourceGroup", contributionKvp.Key);
							contributionCommand.Parameters.AddWithValue("@amountSubmitted", contributionKvp.Value);
							contributionCommand.ExecuteNonQuery();
						}
					}
				}
			}
			foreach (KeyValuePair<string, PlayerTechProgress> playerProgressKvp in guild.PlayerTechProgress)
			{
				string playerUid = playerProgressKvp.Key;
				foreach (KeyValuePair<int, PersonalTechUnlock> unlockKvp in playerProgressKvp.Value.PersonalUnlocks)
				{
					int techId2 = unlockKvp.Key;
					PersonalTechUnlock unlock = unlockKvp.Value;
					using (SqliteCommand command7 = new SqliteCommand("\n                        INSERT INTO guild_member_tech_progress (guild_id, player_uid, tech_id, is_unlocked, unlocked_at)\n                        VALUES (@guildId, @playerUid, @techId, @isUnlocked, @unlockedAt);", connection, transaction))
					{
						command7.Parameters.AddWithValue("@guildId", guildId);
						command7.Parameters.AddWithValue("@playerUid", playerUid);
						command7.Parameters.AddWithValue("@techId", techId2);
						command7.Parameters.AddWithValue("@isUnlocked", (unlock.IsPersonallyUnlocked > false) ? 1 : 0);
						command7.Parameters.AddWithValue("@unlockedAt", DBNull.Value);
						command7.ExecuteNonQuery();
					}
				}
			}
		}

		// Token: 0x06000866 RID: 2150 RVA: 0x0003DA9C File Offset: 0x0003BC9C
		private void MigrateCooldowns(MigrationResult result)
		{
			try
			{
				Dictionary<string, DateTime> cooldowns = JsonSerializer.Deserialize<Dictionary<string, DateTime>>(File.ReadAllText(this.cooldownJsonPath), null);
				if (cooldowns == null || cooldowns.Count == 0)
				{
					result.Warnings.Add("No cooldowns found in guild_cooldowns.json");
				}
				else
				{
					SqliteConnection connection = this.database.Connection;
					using (SqliteTransaction transaction = connection.BeginTransaction())
					{
						try
						{
							foreach (KeyValuePair<string, DateTime> cooldownKvp in cooldowns)
							{
								if (cooldownKvp.Value > DateTime.UtcNow)
								{
									using (SqliteCommand command = new SqliteCommand("\n                                INSERT INTO guild_cooldowns (player_uid, expires_at)\n                                VALUES (@playerUid, @expiresAt);", connection, transaction))
									{
										command.Parameters.AddWithValue("@playerUid", cooldownKvp.Key);
										command.Parameters.AddWithValue("@expiresAt", new DateTimeOffset(cooldownKvp.Value).ToUnixTimeSeconds());
										command.ExecuteNonQuery();
										int cooldownsMigrated = result.CooldownsMigrated;
										result.CooldownsMigrated = cooldownsMigrated + 1;
									}
								}
							}
							transaction.Commit();
							ILogger logger = this.serverApi.Logger;
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(99, 1);
							defaultInterpolatedStringHandler.AppendLiteral("[SRGuildsAndKingdoms:MigrationManager] Successfully migrated ");
							defaultInterpolatedStringHandler.AppendFormatted<int>(result.CooldownsMigrated);
							defaultInterpolatedStringHandler.AppendLiteral(" cooldowns (expired cooldowns skipped)");
							logger.Notification(defaultInterpolatedStringHandler.ToStringAndClear());
						}
						catch (Exception ex)
						{
							transaction.Rollback();
							throw new Exception("Transaction failed during cooldown migration: " + ex.Message, ex);
						}
					}
				}
			}
			catch (Exception ex2)
			{
				result.Errors.Add("Cooldown migration failed: " + ex2.Message);
				throw;
			}
		}

		// Token: 0x06000867 RID: 2151 RVA: 0x0003DCC8 File Offset: 0x0003BEC8
		private void MigrateZoneWhitelists(MigrationResult result)
		{
			try
			{
				Dictionary<string, HashSet<string>> whitelists = JsonSerializer.Deserialize<Dictionary<string, HashSet<string>>>(File.ReadAllText(this.zoneWhitelistJsonPath), null);
				if (whitelists == null || whitelists.Count == 0)
				{
					result.Warnings.Add("No zone whitelists found in zone_whitelist.json");
				}
				else
				{
					SqliteConnection connection = this.database.Connection;
					using (SqliteTransaction transaction = connection.BeginTransaction())
					{
						try
						{
							int zoneId = 1;
							foreach (KeyValuePair<string, HashSet<string>> whitelistKvp in from kvp in whitelists
							orderby kvp.Key
							select kvp)
							{
								string zoneName = whitelistKvp.Key;
								HashSet<string> playerUids = whitelistKvp.Value;
								foreach (string playerUid in playerUids)
								{
									using (SqliteCommand command = new SqliteCommand("\n                                INSERT INTO zone_whitelists (zone_id, player_uid)\n                                VALUES (@zoneId, @playerUid);", connection, transaction))
									{
										command.Parameters.AddWithValue("@zoneId", zoneId);
										command.Parameters.AddWithValue("@playerUid", playerUid);
										command.ExecuteNonQuery();
										int zoneWhitelistsMigrated = result.ZoneWhitelistsMigrated;
										result.ZoneWhitelistsMigrated = zoneWhitelistsMigrated + 1;
									}
								}
								ILogger logger = this.serverApi.Logger;
								DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(78, 3);
								defaultInterpolatedStringHandler.AppendLiteral("[SRGuildsAndKingdoms:MigrationManager] Migrated zone '");
								defaultInterpolatedStringHandler.AppendFormatted(zoneName);
								defaultInterpolatedStringHandler.AppendLiteral("' -> ID ");
								defaultInterpolatedStringHandler.AppendFormatted<int>(zoneId);
								defaultInterpolatedStringHandler.AppendLiteral(" with ");
								defaultInterpolatedStringHandler.AppendFormatted<int>(playerUids.Count);
								defaultInterpolatedStringHandler.AppendLiteral(" player(s)");
								logger.Notification(defaultInterpolatedStringHandler.ToStringAndClear());
								zoneId++;
							}
							transaction.Commit();
							ILogger logger2 = this.serverApi.Logger;
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(84, 1);
							defaultInterpolatedStringHandler2.AppendLiteral("[SRGuildsAndKingdoms:MigrationManager] Successfully migrated ");
							defaultInterpolatedStringHandler2.AppendFormatted<int>(result.ZoneWhitelistsMigrated);
							defaultInterpolatedStringHandler2.AppendLiteral(" zone whitelist entries");
							logger2.Notification(defaultInterpolatedStringHandler2.ToStringAndClear());
							result.Warnings.Add("Zone whitelists migrated with auto-generated IDs. Please verify zone IDs match your guild-config.json ProtectedZones configuration.");
						}
						catch (Exception ex)
						{
							transaction.Rollback();
							throw new Exception("Transaction failed during zone whitelist migration: " + ex.Message, ex);
						}
					}
				}
			}
			catch (Exception ex2)
			{
				result.Errors.Add("Zone whitelist migration failed: " + ex2.Message);
				throw;
			}
		}

		// Token: 0x06000868 RID: 2152 RVA: 0x0003DFC8 File Offset: 0x0003C1C8
		private void ValidateMigration(MigrationResult result)
		{
			try
			{
				SqliteConnection connection = this.database.Connection;
				using (SqliteCommand command = new SqliteCommand("SELECT COUNT(*) FROM guilds;", connection))
				{
					int dbGuildCount = Convert.ToInt32(command.ExecuteScalar());
					if (dbGuildCount != result.GuildsMigrated)
					{
						List<string> warnings = result.Warnings;
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(51, 2);
						defaultInterpolatedStringHandler.AppendLiteral("Guild count mismatch: Expected ");
						defaultInterpolatedStringHandler.AppendFormatted<int>(result.GuildsMigrated);
						defaultInterpolatedStringHandler.AppendLiteral(", found ");
						defaultInterpolatedStringHandler.AppendFormatted<int>(dbGuildCount);
						defaultInterpolatedStringHandler.AppendLiteral(" in database");
						warnings.Add(defaultInterpolatedStringHandler.ToStringAndClear());
					}
					else
					{
						ILogger logger = this.serverApi.Logger;
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(73, 1);
						defaultInterpolatedStringHandler2.AppendLiteral("[SRGuildsAndKingdoms:MigrationManager] Validation: Guild count matches (");
						defaultInterpolatedStringHandler2.AppendFormatted<int>(dbGuildCount);
						defaultInterpolatedStringHandler2.AppendLiteral(")");
						logger.Notification(defaultInterpolatedStringHandler2.ToStringAndClear());
					}
				}
				using (SqliteCommand command2 = new SqliteCommand("SELECT COUNT(*) FROM guild_cooldowns;", connection))
				{
					int dbCooldownCount = Convert.ToInt32(command2.ExecuteScalar());
					if (dbCooldownCount != result.CooldownsMigrated)
					{
						List<string> warnings2 = result.Warnings;
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler3 = new DefaultInterpolatedStringHandler(54, 2);
						defaultInterpolatedStringHandler3.AppendLiteral("Cooldown count mismatch: Expected ");
						defaultInterpolatedStringHandler3.AppendFormatted<int>(result.CooldownsMigrated);
						defaultInterpolatedStringHandler3.AppendLiteral(", found ");
						defaultInterpolatedStringHandler3.AppendFormatted<int>(dbCooldownCount);
						defaultInterpolatedStringHandler3.AppendLiteral(" in database");
						warnings2.Add(defaultInterpolatedStringHandler3.ToStringAndClear());
					}
					else
					{
						ILogger logger2 = this.serverApi.Logger;
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler4 = new DefaultInterpolatedStringHandler(76, 1);
						defaultInterpolatedStringHandler4.AppendLiteral("[SRGuildsAndKingdoms:MigrationManager] Validation: Cooldown count matches (");
						defaultInterpolatedStringHandler4.AppendFormatted<int>(dbCooldownCount);
						defaultInterpolatedStringHandler4.AppendLiteral(")");
						logger2.Notification(defaultInterpolatedStringHandler4.ToStringAndClear());
					}
				}
				using (SqliteCommand command3 = new SqliteCommand("SELECT COUNT(*) FROM zone_whitelists;", connection))
				{
					int dbWhitelistCount = Convert.ToInt32(command3.ExecuteScalar());
					if (dbWhitelistCount != result.ZoneWhitelistsMigrated)
					{
						List<string> warnings3 = result.Warnings;
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler5 = new DefaultInterpolatedStringHandler(60, 2);
						defaultInterpolatedStringHandler5.AppendLiteral("Zone whitelist count mismatch: Expected ");
						defaultInterpolatedStringHandler5.AppendFormatted<int>(result.ZoneWhitelistsMigrated);
						defaultInterpolatedStringHandler5.AppendLiteral(", found ");
						defaultInterpolatedStringHandler5.AppendFormatted<int>(dbWhitelistCount);
						defaultInterpolatedStringHandler5.AppendLiteral(" in database");
						warnings3.Add(defaultInterpolatedStringHandler5.ToStringAndClear());
					}
					else
					{
						ILogger logger3 = this.serverApi.Logger;
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler6 = new DefaultInterpolatedStringHandler(82, 1);
						defaultInterpolatedStringHandler6.AppendLiteral("[SRGuildsAndKingdoms:MigrationManager] Validation: Zone whitelist count matches (");
						defaultInterpolatedStringHandler6.AppendFormatted<int>(dbWhitelistCount);
						defaultInterpolatedStringHandler6.AppendLiteral(")");
						logger3.Notification(defaultInterpolatedStringHandler6.ToStringAndClear());
					}
				}
				this.serverApi.Logger.Notification("[SRGuildsAndKingdoms:MigrationManager] Validation complete");
			}
			catch (Exception ex)
			{
				result.Warnings.Add("Validation failed: " + ex.Message);
				this.serverApi.Logger.Warning("[SRGuildsAndKingdoms:MigrationManager] Validation encountered errors: " + ex.Message);
			}
		}

		// Token: 0x06000869 RID: 2153 RVA: 0x0003E2FC File Offset: 0x0003C4FC
		private bool IsDatabaseEmpty()
		{
			bool result;
			try
			{
				SqliteConnection connection = this.database.Connection;
				using (SqliteCommand command = new SqliteCommand("SELECT COUNT(*) FROM guilds;", connection))
				{
					result = (Convert.ToInt32(command.ExecuteScalar()) == 0);
				}
			}
			catch
			{
				result = true;
			}
			return result;
		}

		// Token: 0x04000366 RID: 870
		private readonly ICoreServerAPI serverApi;

		// Token: 0x04000367 RID: 871
		private readonly GuildDatabase database;

		// Token: 0x04000368 RID: 872
		private readonly string dataFolder;

		// Token: 0x04000369 RID: 873
		private readonly string guildsJsonPath;

		// Token: 0x0400036A RID: 874
		private readonly string cooldownJsonPath;

		// Token: 0x0400036B RID: 875
		private readonly string zoneWhitelistJsonPath;

		// Token: 0x02000173 RID: 371
		[NullableContext(0)]
		private class LandClaimPolymorphicResolver : IJsonTypeInfoResolver
		{
			// Token: 0x06000C21 RID: 3105 RVA: 0x000492F0 File Offset: 0x000474F0
			[NullableContext(1)]
			[return: Nullable(2)]
			public JsonTypeInfo GetTypeInfo(Type type, JsonSerializerOptions options)
			{
				if (type == typeof(List<LandClaim>))
				{
					return JsonTypeInfo.CreateJsonTypeInfo<List<LandClaim>>(options);
				}
				if (type == typeof(Guild))
				{
					return JsonTypeInfo.CreateJsonTypeInfo<Guild>(options);
				}
				return null;
			}
		}
	}
}
