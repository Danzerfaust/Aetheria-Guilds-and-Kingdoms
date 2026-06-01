using Microsoft.Data.Sqlite;
using SRGuildsAndKingdoms.src.guilds;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Vintagestory.API.Server;

namespace SRGuildsAndKingdoms.src.database
{
    /// <summary>
    /// Repository for guild ops
    /// </summary>
    public class GuildRepository(ICoreServerAPI serverApi, GuildDatabase database)
    {
        private readonly ICoreServerAPI serverApi = serverApi;
        private readonly GuildDatabase database = database;

        private readonly Dictionary<string, Guild> guildCache = [];
        private readonly HashSet<string> dirtyGuilds = [];
        private bool cacheLoaded = false;

        /// <summary>
        /// Load guilds from database and into cache
        /// </summary>
        public void LoadAllGuilds()
        {
            try
            {
                serverApi.Logger.Notification("[SRGuildsAndKingdoms] Loading guilds from db...");

                guildCache.Clear();
                dirtyGuilds.Clear();

                var guilds = LoadGuildsFromDatabase();

                foreach (var guild in guilds)
                {
                    guildCache[guild.Name] = guild;
                }

                cacheLoaded = true;
                serverApi.Logger.Notification($"[SRGuildsAndKingdoms] Loaded {guildCache.Count} guilds from db");
            }
            catch (Exception ex)
            {
                serverApi.Logger.Error($"[SRGuildsAndKingdoms] Failed to load guilds: {ex.Message}");
                serverApi.Logger.Error($"[SRGuildsAndKingdoms] Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        /// <summary>
        /// Gets a guild by name
        /// </summary>
        public Guild? GetGuild(string name)
        {
            EnsureCacheLoaded();

            if (string.IsNullOrWhiteSpace(name))
                return null;

            guildCache.TryGetValue(name, out var guild);
            return guild;
        }

        /// <summary>
        /// Gets a guild by member UID
        /// </summary>
        public Guild? GetGuildByMember(string playerUid)
        {
            EnsureCacheLoaded();

            if (string.IsNullOrWhiteSpace(playerUid))
                return null;

            return guildCache.Values.FirstOrDefault(g => g.Members.ContainsKey(playerUid));
        }

        /// <summary>
        /// Gets all guilds
        /// </summary>
        public List<Guild> GetAllGuilds()
        {
            EnsureCacheLoaded();
            return [.. guildCache.Values];
        }

        /// <summary>
        /// Gets the internal database ID for a guild by name
        /// </summary>
        /// <param name="guildName">The name of the guild</param>
        /// <returns>The guild's database ID, or -1 if not found</returns>
        public int GetGuildIdByName(string guildName)
        {
            EnsureCacheLoaded();

            if (string.IsNullOrWhiteSpace(guildName))
                return -1;

            if (guildCache.TryGetValue(guildName, out var guild))
            {
                return guild.DatabaseId ?? -1;
            }

            return -1;
        }

        /// <summary>
        /// Creates a new guild
        /// </summary>
        public void CreateGuild(Guild guild)
        {
            ArgumentNullException.ThrowIfNull(guild);

            if (string.IsNullOrWhiteSpace(guild.Name))
                throw new ArgumentException("Guild name cannot be empty");

            EnsureCacheLoaded();

            if (guildCache.ContainsKey(guild.Name))
                throw new InvalidOperationException($"Guild '{guild.Name}' already exists");

            // Insert the guild directly into the database so it gets a DatabaseId immediately
            try
            {
                var connection = database.Connection;
                using var transaction = connection.BeginTransaction();

                try
                {
                    // Insert new guild and get ID
                    const string insertSql = @"
                        INSERT INTO guilds (name, description, display_color, secondary_color, points, created_at, updated_at)
                        VALUES (@name, @description, @displayColor, @secondaryColor, @points, @createdAt, @updatedAt);
                        SELECT last_insert_rowid();";

                    using var insertCommand = new SqliteCommand(insertSql, connection);
                    insertCommand.Transaction = transaction;
                    insertCommand.Parameters.AddWithValue("@name", guild.Name);
                    insertCommand.Parameters.AddWithValue("@description", guild.Description);
                    insertCommand.Parameters.AddWithValue("@displayColor", guild.DisplayColor);
                    insertCommand.Parameters.AddWithValue("@secondaryColor", guild.SecondaryColor);
                    insertCommand.Parameters.AddWithValue("@points", guild.Points);
                    insertCommand.Parameters.AddWithValue("@createdAt", DateTimeOffset.UtcNow.ToUnixTimeSeconds());
                    insertCommand.Parameters.AddWithValue("@updatedAt", DateTimeOffset.UtcNow.ToUnixTimeSeconds());

                    guild.DatabaseId = Convert.ToInt32(insertCommand.ExecuteScalar());

                    // Insert guild data (members, roles, etc.)
                    int guildId = guild.DatabaseId.Value;
                    UpsertGuildMembers(connection, transaction, guildId, guild);
                    UpsertGuildRoles(connection, transaction, guildId, guild);
                    UpsertGuildInvites(connection, transaction, guildId, guild);
                    UpsertLandClaims(connection, transaction, guildId, guild);
                    UpsertTechProgress(connection, transaction, guildId, guild);

                    transaction.Commit();

                    serverApi.Logger.Notification($"[GuildRepository] Created new guild '{guild.Name}' with database ID: {guild.DatabaseId}");
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    serverApi.Logger.Error($"[GuildRepository] Transaction rolled back during guild creation: {ex.Message}");
                    throw;
                }
            }
            catch (Exception ex)
            {
                serverApi.Logger.Error($"[GuildRepository] Failed to create guild '{guild.Name}': {ex.Message}");
                throw;
            }

            // Add to cache after successful database insert
            guildCache[guild.Name] = guild;
            // Don't mark dirty since we just saved it
        }

        /// <summary>
        /// Updates a guild
        /// </summary>
        public void UpdateGuild(Guild guild)
        {
            ArgumentNullException.ThrowIfNull(guild);

            EnsureCacheLoaded();

            if (!guildCache.ContainsKey(guild.Name))
                throw new InvalidOperationException($"Guild '{guild.Name}' does not exist");

            guildCache[guild.Name] = guild;
            dirtyGuilds.Add(guild.Name);
        }

        /// <summary>
        /// Deletes a guild by name
        /// </summary>
        public void DeleteGuild(string guildName)
        {
            if (string.IsNullOrWhiteSpace(guildName))
                throw new ArgumentException("Guild name cannot be empty");

            EnsureCacheLoaded();

            if (!guildCache.ContainsKey(guildName))
                return;

            guildCache.Remove(guildName);
            dirtyGuilds.Add(guildName);
        }

        /// <summary>
        /// Marks a guild as dirty (needs to be saved)
        /// </summary>
        public void MarkDirty(string guildName)
        {
            if (!string.IsNullOrWhiteSpace(guildName) && guildCache.ContainsKey(guildName))
            {
                dirtyGuilds.Add(guildName);
            }
        }

        /// <summary>
        /// Renames a guild
        /// </summary>
        public void RenameGuild(Guild guild, string newName)
        {
            ArgumentNullException.ThrowIfNull(guild);

            if (string.IsNullOrWhiteSpace(newName))
                throw new ArgumentException("New guild name cannot be empty");

            EnsureCacheLoaded();

            if (!guildCache.ContainsKey(guild.Name))
                throw new InvalidOperationException($"Guild '{guild.Name}' not found in cache");

            if (guildCache.ContainsKey(newName))
                throw new InvalidOperationException($"Guild name '{newName}' is already taken");

            string oldName = guild.Name;

            // Update cache key
            guildCache.Remove(oldName);
            guild.Name = newName;
            guildCache[newName] = guild;

            dirtyGuilds.Remove(oldName);
            dirtyGuilds.Add(newName);
        }

        /// <summary>
        /// Commits all dirty guilds to db
        /// </summary>
        public void CommitChanges()
        {
            if (!cacheLoaded)
            {
                serverApi.Logger.Warning("[SRGuildsAndKingdoms] Cannot save guild changes: cache not loaded");
                return;
            }

            if (dirtyGuilds.Count == 0)
            {
                return;
            }

            try
            {
                var connection = database.Connection;
                using var transaction = connection.BeginTransaction();

                try
                {
                    foreach (var guildName in dirtyGuilds.ToList()) // clone
                    {
                        if (guildCache.TryGetValue(guildName, out var guild))
                        {
                            UpsertGuild(connection, transaction, guild);
                        }
                        else
                        {
                            DeleteGuildFromDatabase(connection, transaction, guildName);
                        }
                    }

                    transaction.Commit();
                    var savedCount = dirtyGuilds.Count;
                    dirtyGuilds.Clear();

                    serverApi.Logger.Notification($"[SRGuildsAndKingdoms] Saved {savedCount} guild changes to db");
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    serverApi.Logger.Error($"[SRGuildsAndKingdoms] Transaction rolled back due to error: {ex.Message}");
                    throw;
                }
            }
            catch (Exception ex)
            {
                serverApi.Logger.Error($"[SRGuildsAndKingdoms] Failed to commit changes: {ex.Message}");
                serverApi.Logger.Error($"[SRGuildsAndKingdoms] Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        /// <summary>
        /// Gets top {limit} leaderboard sorted by guild points (GRS)
        /// </summary>
        public List<Guild> GetLeaderboard(int limit = 10)
        {
            try
            {
                var connection = database.Connection;

                const string sql = @"
                    SELECT id, name, description, display_color, secondary_color, 
                           points, created_at, updated_at
                    FROM guilds
                    ORDER BY points DESC, name ASC
                    LIMIT @limit;";

                using var command = new SqliteCommand(sql, connection);
                command.Parameters.AddWithValue("@limit", limit);

                var guilds = new List<Guild>();
                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    var guildId = reader.GetInt32(0);
                    var guildName = reader.GetString(1);

                    if (guildCache.TryGetValue(guildName, out var cachedGuild))
                    {
                        guilds.Add(cachedGuild);
                    }
                    else
                    {
                        var guild = LoadGuildById(connection, guildId);
                        if (guild != null)
                        {
                            guilds.Add(guild);
                        }
                    }
                }

                return guilds;
            }
            catch (Exception ex)
            {
                serverApi.Logger.Error($"[SRGuildsAndKingdoms] Failed to get leaderboard: {ex.Message}");
                return [];
            }
        }

        #region Private Helper Methods

        /// <summary>
        /// Just make sure cache is loaded
        /// </summary>
        private void EnsureCacheLoaded()
        {
            if (!cacheLoaded)
            {
                LoadAllGuilds();
            }
        }

        /// <summary>
        /// Loads all guilds from database (to be cached on startup)
        /// </summary>
        private List<Guild> LoadGuildsFromDatabase()
        {
            var guilds = new List<Guild>();
            var connection = database.Connection;

            // First, load all guild records
            const string sqlGuilds = @"
                SELECT id, name, description, display_color, secondary_color, 
                       points, created_at, updated_at
                FROM guilds;";

            using var command = new SqliteCommand(sqlGuilds, connection);
            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                var guildId = reader.GetInt32(0);
                var guild = new Guild
                {
                    DatabaseId = guildId, // Set database ID
                    Name = reader.GetString(1),
                    Description = reader.GetString(2),
                    DisplayColor = reader.GetInt32(3),
                    SecondaryColor = reader.GetInt32(4),
                    Points = reader.GetInt32(5),
                };

                // Load related data for this guild
                LoadGuildMembers(connection, guildId, guild);
                LoadGuildRoles(connection, guildId, guild);
                LoadGuildInvites(connection, guildId, guild);
                LoadLandClaims(connection, guildId, guild);
                LoadTechProgress(connection, guildId, guild);

                guilds.Add(guild);
            }

            return guilds;
        }

        private Guild? LoadGuildById(SqliteConnection connection, int guildId)
        {
            const string sql = @"
                SELECT id, name, description, display_color, secondary_color, 
                       points, created_at, updated_at
                FROM guilds
                WHERE id = @guildId;";

            using var command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("@guildId", guildId);

            using var reader = command.ExecuteReader();

            if (!reader.Read())
                return null;

            var guild = new Guild
            {
                DatabaseId = guildId, // Set database ID
                Name = reader.GetString(1),
                Description = reader.GetString(2),
                DisplayColor = reader.GetInt32(3),
                SecondaryColor = reader.GetInt32(4),
                Points = reader.GetInt32(5),
            };

            // Load related data
            LoadGuildMembers(connection, guildId, guild);
            LoadGuildRoles(connection, guildId, guild);
            LoadGuildInvites(connection, guildId, guild);
            LoadLandClaims(connection, guildId, guild);
            LoadTechProgress(connection, guildId, guild);

            return guild;
        }

        private void LoadGuildMembers(SqliteConnection connection, int guildId, Guild guild)
        {
            const string sql = @"
                SELECT player_uid, role, joined_at, last_seen, points_contribution
                FROM guild_members
                WHERE guild_id = @guildId;";

            using var command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("@guildId", guildId);

            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                var member = new GuildMember
                {
                    PlayerUid = reader.GetString(0),
                    Role = reader.GetString(1),
                    LastSeen = DateTimeOffset.FromUnixTimeSeconds(reader.GetInt64(3)).DateTime,
                    PointsContribution = reader.GetInt32(4)
                };
                guild.Members[member.PlayerUid] = member;
            }
        }

        private void LoadGuildRoles(SqliteConnection connection, int guildId, Guild guild)
        {
            const string sql = @"
                SELECT role_name, description, permissions, hierarchy
                FROM guild_roles
                WHERE guild_id = @guildId;";

            using var command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("@guildId", guildId);

            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                var role = new GuildRole
                {
                    Description = reader.GetString(1),
                    Permissions = (GuildPermission)reader.GetInt32(2),
                    Hierarchy = reader.GetInt32(3)
                };
                guild.Roles[reader.GetString(0)] = role;
            }
        }

        private void LoadGuildInvites(SqliteConnection connection, int guildId, Guild guild)
        {
            const string sql = @"
                SELECT inviter_uid, invitee_uid, created_at, expires_at
                FROM guild_invites
                WHERE guild_id = @guildId;";

            using var command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("@guildId", guildId);

            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                var invite = new GuildInvite
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

        private void LoadLandClaims(SqliteConnection connection, int guildId, Guild guild)
        {
            const string sql = @"
                SELECT chunk_x, chunk_z, claim_type, claimed_by_uid, claimed_at, metadata
                FROM land_claims
                WHERE guild_id = @guildId;";

            using var command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("@guildId", guildId);

            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                var chunkX = reader.GetInt32(0);
                var chunkZ = reader.GetInt32(1);
                var claimType = reader.GetString(2);
                var claimedByUid = reader.GetString(3);
                var claimedAt = DateTimeOffset.FromUnixTimeSeconds(reader.GetInt64(4)).DateTime;
                var metadata = reader.IsDBNull(5) ? null : reader.GetString(5);

                LandClaim claim = claimType switch
                {
                    "guild_home" => CreateGuildHomeClaim(chunkX, chunkZ, claimedByUid, claimedAt, metadata),
                    "outpost" => CreateOutpostClaim(chunkX, chunkZ, claimedByUid, claimedAt, metadata),
                    _ => new LandClaim
                    {
                        ChunkX = chunkX,
                        ChunkZ = chunkZ,
                        ClaimedByUid = claimedByUid,
                        Timestamp = claimedAt
                    }
                };

                guild.Claims.Add(claim);
            }
        }

        private GuildHomeClaim CreateGuildHomeClaim(int chunkX, int chunkZ, string claimedByUid, DateTime claimedAt, string? metadata)
        {
            int centerChunkX = chunkX;
            int centerChunkZ = chunkZ;

            if (!string.IsNullOrEmpty(metadata))
            {
                try
                {
                    var metadataObj = JsonSerializer.Deserialize<Dictionary<string, int>>(metadata);
                    if (metadataObj != null)
                    {
                        if (metadataObj.TryGetValue("center_chunk_x", out int cx))
                            centerChunkX = cx;
                        if (metadataObj.TryGetValue("center_chunk_z", out int cz))
                            centerChunkZ = cz;
                    }
                }
                catch { }
            }

            return new GuildHomeClaim(centerChunkX, centerChunkZ, claimedByUid) { Timestamp = claimedAt };
        }

        private OutpostClaim CreateOutpostClaim(int chunkX, int chunkZ, string claimedByUid, DateTime claimedAt, string? metadata)
        {
            string outpostName = "";

            if (!string.IsNullOrEmpty(metadata))
            {
                try
                {
                    var metadataObj = JsonSerializer.Deserialize<Dictionary<string, string>>(metadata);
                    if (metadataObj != null && metadataObj.TryGetValue("outpost_name", out string? name))
                    {
                        outpostName = name ?? "";
                    }
                }
                catch { }
            }

            return new OutpostClaim(chunkX, chunkZ, claimedByUid, outpostName) { Timestamp = claimedAt };
        }

        private void LoadTechProgress(SqliteConnection connection, int guildId, Guild guild)
        {
            // Load guild tech progress
            const string sqlTechProgress = @"
                SELECT tech_id, is_unlocked, requires_personal_unlock, unlocked_at
                FROM guild_tech_progress
                WHERE guild_id = @guildId;";

            using var command = new SqliteCommand(sqlTechProgress, connection);
            command.Parameters.AddWithValue("@guildId", guildId);

            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                var techId = reader.GetInt32(0);
                var progress = new techblock.GuildTechProgress
                {
                    TechBlockId = techId,
                    IsUnlocked = reader.GetInt32(1) == 1,
                    UnlockedTimestamp = reader.IsDBNull(3) ? null : reader.GetInt64(3)
                };

                guild.TechProgress[techId] = progress;
                guild.TechRequiresPersonalUnlock[techId] = reader.GetInt32(2) == 1;

                // Load contributions for this tech
                LoadTechContributions(connection, guildId, techId, progress);
            }

            // Load player tech progress
            LoadPlayerTechProgress(connection, guildId, guild);
        }

        private void LoadTechContributions(SqliteConnection connection, int guildId, int techId, techblock.GuildTechProgress progress)
        {
            const string sql = @"
                SELECT resource_group, amount_submitted
                FROM guild_tech_contributions
                WHERE guild_id = @guildId AND tech_id = @techId;";

            using var command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("@guildId", guildId);
            command.Parameters.AddWithValue("@techId", techId);

            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                progress.ResourceGroupsSubmitted[reader.GetString(0)] = reader.GetInt32(1);
            }
        }

        private void LoadPlayerTechProgress(SqliteConnection connection, int guildId, Guild guild)
        {
            const string sql = @"
                SELECT player_uid, tech_id, is_unlocked, unlocked_at
                FROM guild_member_tech_progress
                WHERE guild_id = @guildId;";

            using var command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("@guildId", guildId);

            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                var playerUid = reader.GetString(0);
                var techId = reader.GetInt32(1);

                var playerProgress = guild.GetOrCreatePlayerProgress(playerUid);
                var unlock = playerProgress.GetOrCreateUnlock(techId);
                unlock.IsPersonallyUnlocked = reader.GetInt32(2) == 1;
            }
        }

        private void UpsertGuild(SqliteConnection connection, SqliteTransaction transaction, Guild guild)
        {
            // Get or create guild ID
            int guildId = GetOrCreateGuildId(connection, transaction, guild);

            // Update guild basic info
            UpdateGuildBasicInfo(connection, transaction, guildId, guild);

            // Upsert related data
            UpsertGuildMembers(connection, transaction, guildId, guild);
            UpsertGuildRoles(connection, transaction, guildId, guild);
            UpsertGuildInvites(connection, transaction, guildId, guild);
            UpsertLandClaims(connection, transaction, guildId, guild);
            UpsertTechProgress(connection, transaction, guildId, guild);
        }

        private int GetOrCreateGuildId(SqliteConnection connection, SqliteTransaction transaction, Guild guild)
        {
            // If guild already has a database ID, use it (for updates and renames)
            if (guild.DatabaseId.HasValue)
            {
                return guild.DatabaseId.Value;
            }

            // Otherwise, try to find by name (for backward compatibility or first-time saves)
            const string selectSql = "SELECT id FROM guilds WHERE name = @name COLLATE NOCASE;";
            using var selectCommand = new SqliteCommand(selectSql, connection);
            selectCommand.Transaction = transaction;
            selectCommand.Parameters.AddWithValue("@name", guild.Name);

            var result = selectCommand.ExecuteScalar();
            if (result != null)
            {
                guild.DatabaseId = Convert.ToInt32(result);
                return guild.DatabaseId.Value;
            }

            // Insert new guild
            const string insertSql = @"
                INSERT INTO guilds (name, description, display_color, secondary_color, points, created_at, updated_at)
                VALUES (@name, @description, @displayColor, @secondaryColor, @points, @createdAt, @updatedAt);
                SELECT last_insert_rowid();";

            using var insertCommand = new SqliteCommand(insertSql, connection);
            insertCommand.Transaction = transaction;
            insertCommand.Parameters.AddWithValue("@name", guild.Name);
            insertCommand.Parameters.AddWithValue("@description", guild.Description);
            insertCommand.Parameters.AddWithValue("@displayColor", guild.DisplayColor);
            insertCommand.Parameters.AddWithValue("@secondaryColor", guild.SecondaryColor);
            insertCommand.Parameters.AddWithValue("@points", 0); // Default points for new guilds
            insertCommand.Parameters.AddWithValue("@createdAt", DateTimeOffset.UtcNow.ToUnixTimeSeconds());
            insertCommand.Parameters.AddWithValue("@updatedAt", DateTimeOffset.UtcNow.ToUnixTimeSeconds());

            guild.DatabaseId = Convert.ToInt32(insertCommand.ExecuteScalar());
            serverApi.Logger.Debug($"[GuildRepository] Created new guild '{guild.Name}' with database ID: {guild.DatabaseId}");
            return guild.DatabaseId.Value;
        }

        private void UpdateGuildBasicInfo(SqliteConnection connection, SqliteTransaction transaction, int guildId, Guild guild)
        {
            const string sql = @"
                UPDATE guilds 
                SET description = @description,
                    display_color = @displayColor,
                    secondary_color = @secondaryColor,
                    points = @points,
                    updated_at = @updatedAt
                WHERE id = @guildId;";

            using var command = new SqliteCommand(sql, connection);
            command.Transaction = transaction;
            command.Parameters.AddWithValue("@guildId", guildId);
            command.Parameters.AddWithValue("@description", guild.Description);
            command.Parameters.AddWithValue("@displayColor", guild.DisplayColor);
            command.Parameters.AddWithValue("@secondaryColor", guild.SecondaryColor);
            command.Parameters.AddWithValue("@points", guild.Points);
            command.Parameters.AddWithValue("@updatedAt", DateTimeOffset.UtcNow.ToUnixTimeSeconds());

            command.ExecuteNonQuery();
        }

        private void UpsertGuildMembers(SqliteConnection connection, SqliteTransaction transaction, int guildId, Guild guild)
        {
            // Get existing member UIDs from the database
            var existingUids = new HashSet<string>();
            const string selectSql = "SELECT player_uid FROM guild_members WHERE guild_id = @guildId;";
            using (var selectCommand = new SqliteCommand(selectSql, connection))
            {
                selectCommand.Transaction = transaction;
                selectCommand.Parameters.AddWithValue("@guildId", guildId);
                using var reader = selectCommand.ExecuteReader();
                while (reader.Read())
                {
                    existingUids.Add(reader.GetString(0));
                }
            }

            // Delete only members that were actually removed from the guild
            var currentUids = guild.Members.Keys.ToHashSet();
            var removedUids = existingUids.Except(currentUids).ToList();
            if (removedUids.Count > 0)
            {
                foreach (var uid in removedUids)
                {
                    const string deleteSql = "DELETE FROM guild_members WHERE guild_id = @guildId AND player_uid = @playerUid;";
                    using var deleteCommand = new SqliteCommand(deleteSql, connection);
                    deleteCommand.Transaction = transaction;
                    deleteCommand.Parameters.AddWithValue("@guildId", guildId);
                    deleteCommand.Parameters.AddWithValue("@playerUid", uid);
                    deleteCommand.ExecuteNonQuery();
                }
            }

            // Upsert current members (insert or update)
            foreach (var member in guild.Members.Values)
            {
                const string upsertSql = @"
                    INSERT INTO guild_members (guild_id, player_uid, role, joined_at, last_seen, points_contribution)
                    VALUES (@guildId, @playerUid, @role, @joinedAt, @lastSeen, @pointsContribution)
                    ON CONFLICT(guild_id, player_uid) DO UPDATE SET
                        role = excluded.role,
                        last_seen = excluded.last_seen,
                        points_contribution = excluded.points_contribution;";

                using var upsertCommand = new SqliteCommand(upsertSql, connection);
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

        private void UpsertGuildRoles(SqliteConnection connection, SqliteTransaction transaction, int guildId, Guild guild)
        {
            // Delete all existing roles
            const string deleteSql = "DELETE FROM guild_roles WHERE guild_id = @guildId;";
            using (var deleteCommand = new SqliteCommand(deleteSql, connection))
            {
                deleteCommand.Transaction = transaction;
                deleteCommand.Parameters.AddWithValue("@guildId", guildId);
                deleteCommand.ExecuteNonQuery();
            }

            // Insert current roles
            foreach (var roleKvp in guild.Roles)
            {
                const string insertSql = @"
                    INSERT INTO guild_roles (guild_id, role_name, description, permissions, hierarchy)
                    VALUES (@guildId, @roleName, @description, @permissions, @hierarchy);";

                using var insertCommand = new SqliteCommand(insertSql, connection);
                insertCommand.Transaction = transaction;
                insertCommand.Parameters.AddWithValue("@guildId", guildId);
                insertCommand.Parameters.AddWithValue("@roleName", roleKvp.Key);
                insertCommand.Parameters.AddWithValue("@description", roleKvp.Value.Description);
                insertCommand.Parameters.AddWithValue("@permissions", (int)roleKvp.Value.Permissions);
                insertCommand.Parameters.AddWithValue("@hierarchy", roleKvp.Value.Hierarchy);
                insertCommand.ExecuteNonQuery();
            }
        }

        private void UpsertGuildInvites(SqliteConnection connection, SqliteTransaction transaction, int guildId, Guild guild)
        {
            // Delete all existing invites
            const string deleteSql = "DELETE FROM guild_invites WHERE guild_id = @guildId;";
            using (var deleteCommand = new SqliteCommand(deleteSql, connection))
            {
                deleteCommand.Transaction = transaction;
                deleteCommand.Parameters.AddWithValue("@guildId", guildId);
                deleteCommand.ExecuteNonQuery();
            }

            // Insert current invites
            foreach (var invite in guild.PendingInvites)
            {
                const string insertSql = @"
                    INSERT INTO guild_invites (guild_id, inviter_uid, invitee_uid, created_at, expires_at)
                    VALUES (@guildId, @inviterUid, @inviteeUid, @createdAt, @expiresAt);";

                using var insertCommand = new SqliteCommand(insertSql, connection);
                insertCommand.Transaction = transaction;
                insertCommand.Parameters.AddWithValue("@guildId", guildId);
                insertCommand.Parameters.AddWithValue("@inviterUid", invite.InviterUid);
                insertCommand.Parameters.AddWithValue("@inviteeUid", invite.InviteeUid);
                insertCommand.Parameters.AddWithValue("@createdAt", new DateTimeOffset(invite.Timestamp).ToUnixTimeSeconds());
                insertCommand.Parameters.AddWithValue("@expiresAt", new DateTimeOffset(invite.ExpiresAt).ToUnixTimeSeconds());
                insertCommand.ExecuteNonQuery();
            }
        }

        private void UpsertLandClaims(SqliteConnection connection, SqliteTransaction transaction, int guildId, Guild guild)
        {
            const string deleteSql = "DELETE FROM land_claims WHERE guild_id = @guildId;";
            using (var deleteCommand = new SqliteCommand(deleteSql, connection))
            {
                deleteCommand.Transaction = transaction;
                deleteCommand.Parameters.AddWithValue("@guildId", guildId);
                deleteCommand.ExecuteNonQuery();
            }

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

                const string insertSql = @"
                    INSERT INTO land_claims (guild_id, chunk_x, chunk_z, claim_type, claimed_by_uid, claimed_at, metadata)
                    VALUES (@guildId, @chunkX, @chunkZ, @claimType, @claimedByUid, @claimedAt, @metadata)
                    ON CONFLICT(chunk_x, chunk_z) DO UPDATE SET
                        guild_id = excluded.guild_id,
                        claim_type = excluded.claim_type,
                        claimed_by_uid = excluded.claimed_by_uid,
                        claimed_at = excluded.claimed_at,
                        metadata = excluded.metadata;";

                using var insertCommand = new SqliteCommand(insertSql, connection);
                insertCommand.Transaction = transaction;
                insertCommand.Parameters.AddWithValue("@guildId", guildId);
                insertCommand.Parameters.AddWithValue("@chunkX", claim.ChunkX);
                insertCommand.Parameters.AddWithValue("@chunkZ", claim.ChunkZ);
                insertCommand.Parameters.AddWithValue("@claimType", claimType);
                insertCommand.Parameters.AddWithValue("@claimedByUid", claim.ClaimedByUid ?? "");
                insertCommand.Parameters.AddWithValue("@claimedAt", new DateTimeOffset(claim.Timestamp).ToUnixTimeSeconds());
                insertCommand.Parameters.AddWithValue("@metadata", (object?)metadata ?? DBNull.Value);
                insertCommand.ExecuteNonQuery();
            }
        }

        private void UpsertTechProgress(SqliteConnection connection, SqliteTransaction transaction, int guildId, Guild guild)
        {
            // Delete existing tech progress
            const string deleteSql = "DELETE FROM guild_tech_progress WHERE guild_id = @guildId;";
            using (var deleteCommand = new SqliteCommand(deleteSql, connection))
            {
                deleteCommand.Transaction = transaction;
                deleteCommand.Parameters.AddWithValue("@guildId", guildId);
                deleteCommand.ExecuteNonQuery();
            }

            // Insert current tech progress
            foreach (var techProgressKvp in guild.TechProgress)
            {
                var techId = techProgressKvp.Key;
                var progress = techProgressKvp.Value;
                var requiresPersonalUnlock = guild.TechRequiresPersonalUnlock.GetValueOrDefault(techId, false);

                const string insertSql = @"
                    INSERT INTO guild_tech_progress (guild_id, tech_id, is_unlocked, requires_personal_unlock, unlocked_at)
                    VALUES (@guildId, @techId, @isUnlocked, @requiresPersonalUnlock, @unlockedAt);";

                using var insertCommand = new SqliteCommand(insertSql, connection);
                insertCommand.Transaction = transaction;
                insertCommand.Parameters.AddWithValue("@guildId", guildId);
                insertCommand.Parameters.AddWithValue("@techId", techId);
                insertCommand.Parameters.AddWithValue("@isUnlocked", progress.IsUnlocked ? 1 : 0);
                insertCommand.Parameters.AddWithValue("@requiresPersonalUnlock", requiresPersonalUnlock ? 1 : 0);
                insertCommand.Parameters.AddWithValue("@unlockedAt", (object?)progress.UnlockedTimestamp ?? DBNull.Value);
                insertCommand.ExecuteNonQuery();

                // Insert tech contributions
                foreach (var contributionKvp in progress.ResourceGroupsSubmitted)
                {
                    const string insertContributionSql = @"
                        INSERT INTO guild_tech_contributions (guild_id, tech_id, resource_group, amount_submitted)
                        VALUES (@guildId, @techId, @resourceGroup, @amountSubmitted);";

                    using var contributionCommand = new SqliteCommand(insertContributionSql, connection);
                    contributionCommand.Transaction = transaction;
                    contributionCommand.Parameters.AddWithValue("@guildId", guildId);
                    contributionCommand.Parameters.AddWithValue("@techId", techId);
                    contributionCommand.Parameters.AddWithValue("@resourceGroup", contributionKvp.Key);
                    contributionCommand.Parameters.AddWithValue("@amountSubmitted", contributionKvp.Value);
                    contributionCommand.ExecuteNonQuery();
                }
            }

            // Upsert player tech progress
            UpsertPlayerTechProgress(connection, transaction, guildId, guild);
        }

        private void UpsertPlayerTechProgress(SqliteConnection connection, SqliteTransaction transaction, int guildId, Guild guild)
        {
            // Delete existing player tech progress
            const string deleteSql = "DELETE FROM guild_member_tech_progress WHERE guild_id = @guildId;";
            using (var deleteCommand = new SqliteCommand(deleteSql, connection))
            {
                deleteCommand.Transaction = transaction;
                deleteCommand.Parameters.AddWithValue("@guildId", guildId);
                deleteCommand.ExecuteNonQuery();
            }

            // Insert current player tech progress
            foreach (var playerProgressKvp in guild.PlayerTechProgress)
            {
                var playerUid = playerProgressKvp.Key;
                var playerProgress = playerProgressKvp.Value;

                foreach (var unlockKvp in playerProgress.PersonalUnlocks)
                {
                    var techId = unlockKvp.Key;
                    var unlock = unlockKvp.Value;

                    const string insertSql = @"
                        INSERT INTO guild_member_tech_progress (guild_id, player_uid, tech_id, is_unlocked, unlocked_at)
                        VALUES (@guildId, @playerUid, @techId, @isUnlocked, @unlockedAt);";

                    using var insertCommand = new SqliteCommand(insertSql, connection);
                    insertCommand.Transaction = transaction;
                    insertCommand.Parameters.AddWithValue("@guildId", guildId);
                    insertCommand.Parameters.AddWithValue("@playerUid", playerUid);
                    insertCommand.Parameters.AddWithValue("@techId", techId);
                    insertCommand.Parameters.AddWithValue("@isUnlocked", unlock.IsPersonallyUnlocked ? 1 : 0);
                    insertCommand.Parameters.AddWithValue("@unlockedAt", DBNull.Value); // Not tracking unlock time for personal unlocks
                    insertCommand.ExecuteNonQuery();
                }
            }
        }

        private void DeleteGuildFromDatabase(SqliteConnection connection, SqliteTransaction transaction, string guildName)
        {
            const string sql = "DELETE FROM guilds WHERE name = @name COLLATE NOCASE;";
            using var command = new SqliteCommand(sql, connection);
            command.Transaction = transaction;
            command.Parameters.AddWithValue("@name", guildName);
            command.ExecuteNonQuery();

            serverApi.Logger.Debug($"[GuildRepository] Deleted guild '{guildName}' from database (cascaded to all related data)");
        }

        #endregion
    }
}
