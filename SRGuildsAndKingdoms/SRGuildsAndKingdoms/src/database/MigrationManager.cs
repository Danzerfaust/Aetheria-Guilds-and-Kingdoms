using SRGuildsAndKingdoms.src.guilds;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Vintagestory.API.Server;

namespace SRGuildsAndKingdoms.src.database
{
    /// <summary>
    /// Migrate guild data from JSON files to SQLite database
    /// 
    /// Can get rid of this in the next version, once we've migrated
    /// </summary>
    public class MigrationManager
    {
        private readonly ICoreServerAPI serverApi;
        private readonly GuildDatabase database;
        private readonly string dataFolder;
        private readonly string guildsJsonPath;
        private readonly string cooldownJsonPath;
        private readonly string zoneWhitelistJsonPath;

        public MigrationManager(ICoreServerAPI serverApi, GuildDatabase database)
        {
            this.serverApi = serverApi;
            this.database = database;
            this.dataFolder = serverApi.GetOrCreateDataPath("ModData/SRGuildsAndKingdoms");
            this.guildsJsonPath = Path.Combine(dataFolder, "guilds.json");
            this.cooldownJsonPath = Path.Combine(dataFolder, "guild_cooldowns.json");
            this.zoneWhitelistJsonPath = Path.Combine(dataFolder, "zone_whitelist.json");
        }

        /// <summary>
        /// Checks if migration is needed (JSON files exist and database is empty)
        /// </summary>
        public bool NeedsMigration()
        {
            try
            {
                // Check if JSON files exist
                bool hasGuildsJson = File.Exists(guildsJsonPath);
                bool hasCooldownJson = File.Exists(cooldownJsonPath);
                bool hasWhitelistJson = File.Exists(zoneWhitelistJsonPath);

                if (!hasGuildsJson && !hasCooldownJson && !hasWhitelistJson)
                {
                    return false;
                }

                // Check if database is empty
                bool isDatabaseEmpty = IsDatabaseEmpty();

                if (hasGuildsJson && !isDatabaseEmpty)
                {
                    serverApi.Logger.Warning("[SRGuildsAndKingdoms:MigrationManager] Both JSON and DB exist - delete guilds.db if you wanna remigrate, otherwise delete jsons");
                    return false;
                }

                serverApi.Logger.Notification($"[SRGuildsAndKingdoms:MigrationManager] Migration needed for: guilds={hasGuildsJson}, cooldowns={hasCooldownJson}, whitelists={hasWhitelistJson}");
                return hasGuildsJson || hasCooldownJson || hasWhitelistJson;
            }
            catch (Exception ex)
            {
                serverApi.Logger.Error($"[SRGuildsAndKingdoms:MigrationManager] Error checking migration status: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Performs the migration from JSON to SQLite
        /// </summary>
        public MigrationResult MigrateFromJson()
        {
            var result = new MigrationResult
            {
                StartTime = DateTime.UtcNow
            };

            try
            {
                serverApi.Logger.Notification("[SRGuildsAndKingdoms:MigrationManager] ========================================");
                serverApi.Logger.Notification("[SRGuildsAndKingdoms:MigrationManager] Starting migration from JSON to SQLite");
                serverApi.Logger.Notification("[SRGuildsAndKingdoms:MigrationManager] ========================================");

                // Step 1: Create backups
                serverApi.Logger.Notification("[SRGuildsAndKingdoms:MigrationManager] Step 1: Creating backups of JSON files...");
                CreateBackups(result);

                // Step 2: Load and migrate guilds
                if (File.Exists(guildsJsonPath))
                {
                    serverApi.Logger.Notification("[SRGuildsAndKingdoms:MigrationManager] Step 2: Migrating guild data...");
                    MigrateGuilds(result);
                }
                else
                {
                    serverApi.Logger.Notification("[SRGuildsAndKingdoms:MigrationManager] Step 2: Skipped - no guilds.json found");
                }

                // Step 3: Migrate cooldowns
                if (File.Exists(cooldownJsonPath))
                {
                    serverApi.Logger.Notification("[SRGuildsAndKingdoms:MigrationManager] Step 3: Migrating cooldown data...");
                    MigrateCooldowns(result);
                }
                else
                {
                    serverApi.Logger.Notification("[SRGuildsAndKingdoms:MigrationManager] Step 3: Skipped - no guild_cooldowns.json found");
                }

                // Step 4: Migrate zone whitelists
                if (File.Exists(zoneWhitelistJsonPath))
                {
                    serverApi.Logger.Notification("[SRGuildsAndKingdoms:MigrationManager] Step 4: Migrating zone whitelist data...");
                    MigrateZoneWhitelists(result);
                }
                else
                {
                    serverApi.Logger.Notification("[SRGuildsAndKingdoms:MigrationManager] Step 4: Skipped - no zone_whitelist.json found");
                }

                // Step 5: Validate migration
                serverApi.Logger.Notification("[SRGuildsAndKingdoms:MigrationManager] Step 5: Validating migration...");
                ValidateMigration(result);

                result.EndTime = DateTime.UtcNow;
                result.Success = !result.Errors.Any();

                serverApi.Logger.Notification("[SRGuildsAndKingdoms:MigrationManager] ========================================");
                serverApi.Logger.Notification($"[SRGuildsAndKingdoms:MigrationManager] Migration completed in {result.Duration.TotalSeconds:F2} seconds");
                serverApi.Logger.Notification($"[SRGuildsAndKingdoms:MigrationManager] Success: {result.Success}");
                serverApi.Logger.Notification($"[SRGuildsAndKingdoms:MigrationManager] Guilds migrated: {result.GuildsMigrated}");
                serverApi.Logger.Notification($"[SRGuildsAndKingdoms:MigrationManager] Cooldowns migrated: {result.CooldownsMigrated}");
                serverApi.Logger.Notification($"[SRGuildsAndKingdoms:MigrationManager] Zone whitelists migrated: {result.ZoneWhitelistsMigrated}");

                if (result.Errors.Any())
                {
                    serverApi.Logger.Error($"[SRGuildsAndKingdoms:MigrationManager] Errors encountered: {result.Errors.Count}");
                    foreach (var error in result.Errors)
                    {
                        serverApi.Logger.Error($"[SRGuildsAndKingdoms:MigrationManager]   - {error}");
                    }
                }

                if (result.Warnings.Any())
                {
                    serverApi.Logger.Warning($"[SRGuildsAndKingdoms:MigrationManager] Warnings: {result.Warnings.Count}");
                    foreach (var warning in result.Warnings)
                    {
                        serverApi.Logger.Warning($"[SRGuildsAndKingdoms:MigrationManager]   - {warning}");
                    }
                }

                serverApi.Logger.Notification("[SRGuildsAndKingdoms:MigrationManager] ========================================");

                return result;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.EndTime = DateTime.UtcNow;
                result.Errors.Add($"Migration failed with exception: {ex.Message}");

                serverApi.Logger.Error($"[SRGuildsAndKingdoms:MigrationManager] Migration failed: {ex.Message}");
                serverApi.Logger.Error($"[SRGuildsAndKingdoms:MigrationManager] Stack trace: {ex.StackTrace}");

                return result;
            }
        }

        private void CreateBackups(MigrationResult result)
        {
            var backupFolder = Path.Combine(dataFolder, "json_backups");
            Directory.CreateDirectory(backupFolder);

            var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");

            if (File.Exists(guildsJsonPath))
            {
                var backupPath = Path.Combine(backupFolder, $"guilds_{timestamp}.json");
                File.Copy(guildsJsonPath, backupPath, true);
                result.BackupFiles.Add(backupPath);
                serverApi.Logger.Notification($"[SRGuildsAndKingdoms:MigrationManager] Created backup: {backupPath}");
            }

            if (File.Exists(cooldownJsonPath))
            {
                var backupPath = Path.Combine(backupFolder, $"guild_cooldowns_{timestamp}.json");
                File.Copy(cooldownJsonPath, backupPath, true);
                result.BackupFiles.Add(backupPath);
                serverApi.Logger.Notification($"[SRGuildsAndKingdoms:MigrationManager] Created backup: {backupPath}");
            }

            if (File.Exists(zoneWhitelistJsonPath))
            {
                var backupPath = Path.Combine(backupFolder, $"zone_whitelist_{timestamp}.json");
                File.Copy(zoneWhitelistJsonPath, backupPath, true);
                result.BackupFiles.Add(backupPath);
                serverApi.Logger.Notification($"[SRGuildsAndKingdoms:MigrationManager] Created backup: {backupPath}");
            }
        }

        private void MigrateGuilds(MigrationResult result)
        {
            try
            {
                var json = File.ReadAllText(guildsJsonPath);

                var options = new JsonSerializerOptions
                {
                    DefaultIgnoreCondition = JsonIgnoreCondition.Never,
                    PropertyNameCaseInsensitive = true
                };

                // Configure polymorphic deserialization for LandClaim hierarchy
                options.TypeInfoResolver = JsonTypeInfoResolver.Combine(
                    new DefaultJsonTypeInfoResolver(),
                    new LandClaimPolymorphicResolver()
                );

                var guilds = JsonSerializer.Deserialize<Dictionary<string, Guild>>(json, options);

                if (guilds == null || guilds.Count == 0)
                {
                    result.Warnings.Add("No guilds found in guilds.json");
                    return;
                }

                serverApi.Logger.Notification($"[SRGuildsAndKingdoms:MigrationManager] Loaded {guilds.Count} guilds from JSON");

                // Create repository for inserting guilds
                var repository = new GuildRepository(serverApi, database);

                var connection = database.Connection;
                using var transaction = connection.BeginTransaction();

                try
                {
                    foreach (var guildKvp in guilds)
                    {
                        var guild = guildKvp.Value;

                        try
                        {
                            // Ensure guild has default roles
                            if (!guild.Roles.ContainsKey("Leader"))
                            {
                                guild.Roles["Leader"] = new GuildRole
                                {
                                    Description = "Leader",
                                    Permissions = GuildPermission.Invite | GuildPermission.Promote | GuildPermission.Kick |
                                                 GuildPermission.ManageRoles | GuildPermission.BreakAndPlaceBlocks |
                                                 GuildPermission.InteractBlocks,
                                    Hierarchy = 1
                                };
                            }

                            if (!guild.Roles.ContainsKey("Member"))
                            {
                                guild.Roles["Member"] = new GuildRole
                                {
                                    Description = "Member",
                                    Permissions = GuildPermission.BreakAndPlaceBlocks | GuildPermission.InteractBlocks,
                                    Hierarchy = 100
                                };
                            }

                            // Regenerate home chunks for GuildHomeClaims if needed
                            foreach (var claim in guild.Claims.OfType<GuildHomeClaim>())
                            {
                                if (claim.HomeChunks == null || claim.HomeChunks.Count == 0)
                                {
                                    claim.GenerateHomeChunks();
                                }
                                claim.IsGuildHome = true;
                            }

                            // Ensure outpost claims have correct flag
                            foreach (var claim in guild.Claims.OfType<OutpostClaim>())
                            {
                                claim.IsOutpost = true;
                            }

                            // Insert guild directly using SQL (bypass cache during migration)
                            InsertGuildDirect(connection, transaction, guild);
                            result.GuildsMigrated++;

                            serverApi.Logger.Debug($"[SRGuildsAndKingdoms:MigrationManager] Migrated guild: {guild.Name} ({guild.Members.Count} members, {guild.Claims.Count} claims)");
                        }
                        catch (Exception ex)
                        {
                            result.Errors.Add($"Failed to migrate guild '{guild.Name}': {ex.Message}");
                            serverApi.Logger.Error($"[SRGuildsAndKingdoms:MigrationManager] Error migrating guild '{guild.Name}': {ex.Message}");
                        }
                    }

                    transaction.Commit();
                    serverApi.Logger.Notification($"[SRGuildsAndKingdoms:MigrationManager] Successfully migrated {result.GuildsMigrated} guilds");
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    throw new Exception($"Transaction failed during guild migration: {ex.Message}", ex);
                }
            }
            catch (Exception ex)
            {
                result.Errors.Add($"Guild migration failed: {ex.Message}");
                throw;
            }
        }

        private void InsertGuildDirect(Microsoft.Data.Sqlite.SqliteConnection connection, Microsoft.Data.Sqlite.SqliteTransaction transaction, Guild guild)
        {
            // This is a simplified version of GuildRepository.UpsertGuild
            // We insert directly to avoid cache complexity during migration

            // 1. Insert guild basic info
            const string guildSql = @"
                INSERT INTO guilds (name, description, display_color, secondary_color, points, created_at, updated_at)
                VALUES (@name, @description, @displayColor, @secondaryColor, @points, @createdAt, @updatedAt);
                SELECT last_insert_rowid();";

            long guildId;
            using (var command = new Microsoft.Data.Sqlite.SqliteCommand(guildSql, connection, transaction))
            {
                command.Parameters.AddWithValue("@name", guild.Name);
                command.Parameters.AddWithValue("@description", guild.Description);
                command.Parameters.AddWithValue("@displayColor", guild.DisplayColor);
                command.Parameters.AddWithValue("@secondaryColor", guild.SecondaryColor);
                command.Parameters.AddWithValue("@points", guild.Points);
                command.Parameters.AddWithValue("@createdAt", DateTimeOffset.UtcNow.ToUnixTimeSeconds());
                command.Parameters.AddWithValue("@updatedAt", DateTimeOffset.UtcNow.ToUnixTimeSeconds());

                guildId = (long)command.ExecuteScalar()!;
            }

            // 2. Insert members
            foreach (var member in guild.Members.Values)
            {
                const string memberSql = @"
                    INSERT INTO guild_members (guild_id, player_uid, role, joined_at, last_seen)
                    VALUES (@guildId, @playerUid, @role, @joinedAt, @lastSeen);";

                using var command = new Microsoft.Data.Sqlite.SqliteCommand(memberSql, connection, transaction);
                command.Parameters.AddWithValue("@guildId", guildId);
                command.Parameters.AddWithValue("@playerUid", member.PlayerUid);
                command.Parameters.AddWithValue("@role", member.Role);
                command.Parameters.AddWithValue("@joinedAt", DateTimeOffset.UtcNow.ToUnixTimeSeconds());
                command.Parameters.AddWithValue("@lastSeen", new DateTimeOffset(member.LastSeen).ToUnixTimeSeconds());
                command.ExecuteNonQuery();
            }

            // 3. Insert roles
            foreach (var roleKvp in guild.Roles)
            {
                const string roleSql = @"
                    INSERT INTO guild_roles (guild_id, role_name, description, permissions, hierarchy)
                    VALUES (@guildId, @roleName, @description, @permissions, @hierarchy);";

                using var command = new Microsoft.Data.Sqlite.SqliteCommand(roleSql, connection, transaction);
                command.Parameters.AddWithValue("@guildId", guildId);
                command.Parameters.AddWithValue("@roleName", roleKvp.Key);
                command.Parameters.AddWithValue("@description", roleKvp.Value.Description);
                command.Parameters.AddWithValue("@permissions", (int)roleKvp.Value.Permissions);
                command.Parameters.AddWithValue("@hierarchy", roleKvp.Value.Hierarchy);
                command.ExecuteNonQuery();
            }

            // 4. Insert invites
            foreach (var invite in guild.PendingInvites)
            {
                const string inviteSql = @"
                    INSERT INTO guild_invites (guild_id, inviter_uid, invitee_uid, created_at, expires_at)
                    VALUES (@guildId, @inviterUid, @inviteeUid, @createdAt, @expiresAt);";

                using var command = new Microsoft.Data.Sqlite.SqliteCommand(inviteSql, connection, transaction);
                command.Parameters.AddWithValue("@guildId", guildId);
                command.Parameters.AddWithValue("@inviterUid", invite.InviterUid);
                command.Parameters.AddWithValue("@inviteeUid", invite.InviteeUid);
                command.Parameters.AddWithValue("@createdAt", new DateTimeOffset(invite.Timestamp).ToUnixTimeSeconds());
                command.Parameters.AddWithValue("@expiresAt", new DateTimeOffset(invite.ExpiresAt).ToUnixTimeSeconds());
                command.ExecuteNonQuery();
            }

            // 5. Insert land claims
            foreach (var claim in guild.Claims)
            {
                string claimType = claim switch
                {
                    GuildHomeClaim => "guild_home",
                    OutpostClaim => "outpost",
                    _ => "regular"
                };

                string? metadata = null;
                if (claim is GuildHomeClaim guildHome)
                {
                    metadata = JsonSerializer.Serialize(new { center_chunk_x = guildHome.CenterChunkX, center_chunk_z = guildHome.CenterChunkZ });
                }
                else if (claim is OutpostClaim outpost)
                {
                    metadata = JsonSerializer.Serialize(new { outpost_name = outpost.OutpostName });
                }

                const string claimSql = @"
                    INSERT INTO land_claims (guild_id, chunk_x, chunk_z, claim_type, claimed_by_uid, claimed_at, metadata)
                    VALUES (@guildId, @chunkX, @chunkZ, @claimType, @claimedByUid, @claimedAt, @metadata);";

                using var command = new Microsoft.Data.Sqlite.SqliteCommand(claimSql, connection, transaction);
                command.Parameters.AddWithValue("@guildId", guildId);
                command.Parameters.AddWithValue("@chunkX", claim.ChunkX);
                command.Parameters.AddWithValue("@chunkZ", claim.ChunkZ);
                command.Parameters.AddWithValue("@claimType", claimType);
                command.Parameters.AddWithValue("@claimedByUid", claim.ClaimedByUid ?? "");
                command.Parameters.AddWithValue("@claimedAt", new DateTimeOffset(claim.Timestamp).ToUnixTimeSeconds());
                command.Parameters.AddWithValue("@metadata", (object?)metadata ?? DBNull.Value);
                command.ExecuteNonQuery();
            }

            // 6. Insert tech progress
            foreach (var techProgressKvp in guild.TechProgress)
            {
                var techId = techProgressKvp.Key;
                var progress = techProgressKvp.Value;
                var requiresPersonalUnlock = guild.TechRequiresPersonalUnlock.GetValueOrDefault(techId, false);

                const string techProgressSql = @"
                    INSERT INTO guild_tech_progress (guild_id, tech_id, is_unlocked, requires_personal_unlock, unlocked_at)
                    VALUES (@guildId, @techId, @isUnlocked, @requiresPersonalUnlock, @unlockedAt);";

                using var command = new Microsoft.Data.Sqlite.SqliteCommand(techProgressSql, connection, transaction);
                command.Parameters.AddWithValue("@guildId", guildId);
                command.Parameters.AddWithValue("@techId", techId);
                command.Parameters.AddWithValue("@isUnlocked", progress.IsUnlocked ? 1 : 0);
                command.Parameters.AddWithValue("@requiresPersonalUnlock", requiresPersonalUnlock ? 1 : 0);
                command.Parameters.AddWithValue("@unlockedAt", (object?)progress.UnlockedTimestamp ?? DBNull.Value);
                command.ExecuteNonQuery();

                // Insert tech contributions
                foreach (var contributionKvp in progress.ResourceGroupsSubmitted)
                {
                    const string contributionSql = @"
                        INSERT INTO guild_tech_contributions (guild_id, tech_id, resource_group, amount_submitted)
                        VALUES (@guildId, @techId, @resourceGroup, @amountSubmitted);";

                    using var contributionCommand = new Microsoft.Data.Sqlite.SqliteCommand(contributionSql, connection, transaction);
                    contributionCommand.Parameters.AddWithValue("@guildId", guildId);
                    contributionCommand.Parameters.AddWithValue("@techId", techId);
                    contributionCommand.Parameters.AddWithValue("@resourceGroup", contributionKvp.Key);
                    contributionCommand.Parameters.AddWithValue("@amountSubmitted", contributionKvp.Value);
                    contributionCommand.ExecuteNonQuery();
                }
            }

            // 7. Insert player tech progress
            foreach (var playerProgressKvp in guild.PlayerTechProgress)
            {
                var playerUid = playerProgressKvp.Key;
                var playerProgress = playerProgressKvp.Value;

                foreach (var unlockKvp in playerProgress.PersonalUnlocks)
                {
                    var techId = unlockKvp.Key;
                    var unlock = unlockKvp.Value;



                    const string playerProgressSql = @"
                        INSERT INTO guild_member_tech_progress (guild_id, player_uid, tech_id, is_unlocked, unlocked_at)
                        VALUES (@guildId, @playerUid, @techId, @isUnlocked, @unlockedAt);";

                    using var command = new Microsoft.Data.Sqlite.SqliteCommand(playerProgressSql, connection, transaction);
                    command.Parameters.AddWithValue("@guildId", guildId);
                    command.Parameters.AddWithValue("@playerUid", playerUid);
                    command.Parameters.AddWithValue("@techId", techId);
                    command.Parameters.AddWithValue("@isUnlocked", unlock.IsPersonallyUnlocked ? 1 : 0);
                    command.Parameters.AddWithValue("@unlockedAt", DBNull.Value);
                    command.ExecuteNonQuery();
                }
            }
        }

        private void MigrateCooldowns(MigrationResult result)
        {
            try
            {
                var json = File.ReadAllText(cooldownJsonPath);
                var cooldowns = JsonSerializer.Deserialize<Dictionary<string, DateTime>>(json);

                if (cooldowns == null || cooldowns.Count == 0)
                {
                    result.Warnings.Add("No cooldowns found in guild_cooldowns.json");
                    return;
                }

                var connection = database.Connection;
                using var transaction = connection.BeginTransaction();

                try
                {
                    foreach (var cooldownKvp in cooldowns)
                    {
                        // Only migrate cooldowns that haven't expired
                        if (cooldownKvp.Value > DateTime.UtcNow)
                        {
                            const string sql = @"
                                INSERT INTO guild_cooldowns (player_uid, expires_at)
                                VALUES (@playerUid, @expiresAt);";

                            using var command = new Microsoft.Data.Sqlite.SqliteCommand(sql, connection, transaction);
                            command.Parameters.AddWithValue("@playerUid", cooldownKvp.Key);
                            command.Parameters.AddWithValue("@expiresAt", new DateTimeOffset(cooldownKvp.Value).ToUnixTimeSeconds());
                            command.ExecuteNonQuery();

                            result.CooldownsMigrated++;
                        }
                    }

                    transaction.Commit();
                    serverApi.Logger.Notification($"[SRGuildsAndKingdoms:MigrationManager] Successfully migrated {result.CooldownsMigrated} cooldowns (expired cooldowns skipped)");
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    throw new Exception($"Transaction failed during cooldown migration: {ex.Message}", ex);
                }
            }
            catch (Exception ex)
            {
                result.Errors.Add($"Cooldown migration failed: {ex.Message}");
                throw;
            }
        }

        private void MigrateZoneWhitelists(MigrationResult result)
        {
            try
            {
                var json = File.ReadAllText(zoneWhitelistJsonPath);
                var whitelists = JsonSerializer.Deserialize<Dictionary<string, HashSet<string>>>(json);

                if (whitelists == null || whitelists.Count == 0)
                {
                    result.Warnings.Add("No zone whitelists found in zone_whitelist.json");
                    return;
                }

                var connection = database.Connection;
                using var transaction = connection.BeginTransaction();

                try
                {
                    // Auto-generate zone IDs based on order (starting from 1)
                    // Zone names can be looked up from config using the zone ID
                    int zoneId = 1;
                    foreach (var whitelistKvp in whitelists.OrderBy(kvp => kvp.Key))
                    {
                        var zoneName = whitelistKvp.Key;
                        var playerUids = whitelistKvp.Value;

                        foreach (var playerUid in playerUids)
                        {
                            const string sql = @"
                                INSERT INTO zone_whitelists (zone_id, player_uid)
                                VALUES (@zoneId, @playerUid);";

                            using var command = new Microsoft.Data.Sqlite.SqliteCommand(sql, connection, transaction);
                            command.Parameters.AddWithValue("@zoneId", zoneId);
                            command.Parameters.AddWithValue("@playerUid", playerUid);
                            command.ExecuteNonQuery();

                            result.ZoneWhitelistsMigrated++;
                        }

                        serverApi.Logger.Notification($"[SRGuildsAndKingdoms:MigrationManager] Migrated zone '{zoneName}' -> ID {zoneId} with {playerUids.Count} player(s)");
                        zoneId++;
                    }

                    transaction.Commit();
                    serverApi.Logger.Notification($"[SRGuildsAndKingdoms:MigrationManager] Successfully migrated {result.ZoneWhitelistsMigrated} zone whitelist entries");
                    result.Warnings.Add($"Zone whitelists migrated with auto-generated IDs. Please verify zone IDs match your guild-config.json ProtectedZones configuration.");
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    throw new Exception($"Transaction failed during zone whitelist migration: {ex.Message}", ex);
                }
            }
            catch (Exception ex)
            {
                result.Errors.Add($"Zone whitelist migration failed: {ex.Message}");
                throw;
            }
        }

        private void ValidateMigration(MigrationResult result)
        {
            try
            {
                var connection = database.Connection;

                // Count guilds in database
                const string countGuildsSql = "SELECT COUNT(*) FROM guilds;";
                using (var command = new Microsoft.Data.Sqlite.SqliteCommand(countGuildsSql, connection))
                {
                    var dbGuildCount = Convert.ToInt32(command.ExecuteScalar());

                    if (dbGuildCount != result.GuildsMigrated)
                    {
                        result.Warnings.Add($"Guild count mismatch: Expected {result.GuildsMigrated}, found {dbGuildCount} in database");
                    }
                    else
                    {
                        serverApi.Logger.Notification($"[SRGuildsAndKingdoms:MigrationManager] Validation: Guild count matches ({dbGuildCount})");
                    }
                }

                // Count cooldowns in database
                const string countCooldownsSql = "SELECT COUNT(*) FROM guild_cooldowns;";
                using (var command = new Microsoft.Data.Sqlite.SqliteCommand(countCooldownsSql, connection))
                {
                    var dbCooldownCount = Convert.ToInt32(command.ExecuteScalar());

                    if (dbCooldownCount != result.CooldownsMigrated)
                    {
                        result.Warnings.Add($"Cooldown count mismatch: Expected {result.CooldownsMigrated}, found {dbCooldownCount} in database");
                    }
                    else
                    {
                        serverApi.Logger.Notification($"[SRGuildsAndKingdoms:MigrationManager] Validation: Cooldown count matches ({dbCooldownCount})");
                    }
                }

                // Count zone whitelists in database
                const string countWhitelistsSql = "SELECT COUNT(*) FROM zone_whitelists;";
                using (var command = new Microsoft.Data.Sqlite.SqliteCommand(countWhitelistsSql, connection))
                {
                    var dbWhitelistCount = Convert.ToInt32(command.ExecuteScalar());

                    if (dbWhitelistCount != result.ZoneWhitelistsMigrated)
                    {
                        result.Warnings.Add($"Zone whitelist count mismatch: Expected {result.ZoneWhitelistsMigrated}, found {dbWhitelistCount} in database");
                    }
                    else
                    {
                        serverApi.Logger.Notification($"[SRGuildsAndKingdoms:MigrationManager] Validation: Zone whitelist count matches ({dbWhitelistCount})");
                    }
                }

                serverApi.Logger.Notification("[SRGuildsAndKingdoms:MigrationManager] Validation complete");
            }
            catch (Exception ex)
            {
                result.Warnings.Add($"Validation failed: {ex.Message}");
                serverApi.Logger.Warning($"[SRGuildsAndKingdoms:MigrationManager] Validation encountered errors: {ex.Message}");
            }
        }

        private bool IsDatabaseEmpty()
        {
            try
            {
                var connection = database.Connection;
                const string sql = "SELECT COUNT(*) FROM guilds;";
                using var command = new Microsoft.Data.Sqlite.SqliteCommand(sql, connection);
                var count = Convert.ToInt32(command.ExecuteScalar());
                return count == 0;
            }
            catch
            {
                return true;
            }
        }

        // Custom type resolver for polymorphic land claims (same as GuildManager)
        private class LandClaimPolymorphicResolver : IJsonTypeInfoResolver
        {
            public JsonTypeInfo? GetTypeInfo(Type type, JsonSerializerOptions options)
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

    /// <summary>
    /// Result of a migration operation
    /// </summary>
    public class MigrationResult
    {
        public bool Success { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public TimeSpan Duration => EndTime - StartTime;

        public int GuildsMigrated { get; set; }
        public int CooldownsMigrated { get; set; }
        public int ZoneWhitelistsMigrated { get; set; }

        public List<string> BackupFiles { get; set; } = new();
        public List<string> Errors { get; set; } = new();
        public List<string> Warnings { get; set; } = new();

        public string Summary => $"Success: {Success}, Guilds: {GuildsMigrated}, Cooldowns: {CooldownsMigrated}, Zone Whitelists: {ZoneWhitelistsMigrated}, Duration: {Duration.TotalSeconds:F2}s, Errors: {Errors.Count}, Warnings: {Warnings.Count}";
    }
}
