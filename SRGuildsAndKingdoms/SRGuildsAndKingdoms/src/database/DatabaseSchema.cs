using Microsoft.Data.Sqlite;
using System;
using Vintagestory.API.Server;

namespace SRGuildsAndKingdoms.src.database
{
    public class DatabaseSchema
    {
        private readonly ICoreServerAPI serverApi;
        private const int CurrentSchemaVersion = 9;

        public DatabaseSchema(ICoreServerAPI serverApi)
        {
            this.serverApi = serverApi;
        }

        /// <summary>
        /// Init the database schema and applies migrations if needed
        /// </summary>
        public void InitializeSchema(SqliteConnection connection)
        {
            try
            {
                CreateSchemaVersionTable(connection);

                int currentVersion = GetCurrentSchemaVersion(connection);

                if (currentVersion == 0)
                {
                    serverApi.Logger.Notification("[SRGuildsAndKingdoms] Initializing database schema...");
                    CreateAllTables(connection);
                    SetSchemaVersion(connection, CurrentSchemaVersion);
                    serverApi.Logger.Notification($"[SRGuildsAndKingdoms] Schema initialized to version {CurrentSchemaVersion}");
                }
                else if (currentVersion < CurrentSchemaVersion)
                {
                    serverApi.Logger.Notification($"[SRGuildsAndKingdoms] Migrating schema from version {currentVersion} to {CurrentSchemaVersion}");
                    ApplyMigrations(connection, currentVersion);
                    SetSchemaVersion(connection, CurrentSchemaVersion);
                    serverApi.Logger.Notification($"[SRGuildsAndKingdoms] Schema migration complete");
                }
                else if (currentVersion > CurrentSchemaVersion)
                {
                    serverApi.Logger.Error($"[SRGuildsAndKingdoms] Database schema version {currentVersion} is newer than expected {CurrentSchemaVersion}. Please update the mod.");
                    throw new InvalidOperationException($"Database schema version mismatch: {currentVersion} > {CurrentSchemaVersion}");
                }
            }
            catch (Exception ex)
            {
                serverApi.Logger.Error($"[SRGuildsAndKingdoms] Failed to initialize schema: {ex.Message}");
                serverApi.Logger.Error($"[SRGuildsAndKingdoms] Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        private void CreateSchemaVersionTable(SqliteConnection connection)
        {
            const string sql = @"
                CREATE TABLE IF NOT EXISTS schema_version (
                    version INTEGER PRIMARY KEY,
                    applied_at INTEGER NOT NULL
                );";

            using var command = new SqliteCommand(sql, connection);
            command.ExecuteNonQuery();
        }

        private int GetCurrentSchemaVersion(SqliteConnection connection)
        {
            const string sql = "SELECT COALESCE(MAX(version), 0) FROM schema_version;";
            using var command = new SqliteCommand(sql, connection);
            var result = command.ExecuteScalar();
            return Convert.ToInt32(result);
        }

        private void SetSchemaVersion(SqliteConnection connection, int version)
        {
            const string sql = "INSERT INTO schema_version (version, applied_at) VALUES (@version, @appliedAt);";
            using var command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("@version", version);
            command.Parameters.AddWithValue("@appliedAt", DateTimeOffset.UtcNow.ToUnixTimeSeconds());
            command.ExecuteNonQuery();
        }

        private void CreateAllTables(SqliteConnection connection)
        {
            using var transaction = connection.BeginTransaction();
            try
            {
                // Guilds table
                ExecuteNonQuery(connection, @"
                    CREATE TABLE guilds (
                        id INTEGER PRIMARY KEY AUTOINCREMENT,
                        name TEXT NOT NULL UNIQUE COLLATE NOCASE,
                        description TEXT NOT NULL DEFAULT '',
                        display_color INTEGER NOT NULL,
                        secondary_color INTEGER NOT NULL,
                        points INTEGER NOT NULL DEFAULT 0,
                        created_at INTEGER NOT NULL,
                        updated_at INTEGER NOT NULL
                    );", transaction);

                ExecuteNonQuery(connection, "CREATE INDEX idx_guilds_name ON guilds(name COLLATE NOCASE);", transaction);
                ExecuteNonQuery(connection, "CREATE INDEX idx_guilds_points ON guilds(points DESC);", transaction);
                ExecuteNonQuery(connection, "CREATE INDEX idx_guilds_created_at ON guilds(created_at);", transaction);

                // Guild members table
                ExecuteNonQuery(connection, @"
                    CREATE TABLE guild_members (
                        guild_id INTEGER NOT NULL,
                        player_uid TEXT NOT NULL,
                        role TEXT NOT NULL,
                        joined_at INTEGER NOT NULL,
                        last_seen INTEGER NOT NULL,
                        PRIMARY KEY (guild_id, player_uid),
                        FOREIGN KEY (guild_id) REFERENCES guilds(id) ON DELETE CASCADE
                    );", transaction);

                ExecuteNonQuery(connection, "CREATE INDEX idx_guild_members_player_uid ON guild_members(player_uid);", transaction);
                ExecuteNonQuery(connection, "CREATE INDEX idx_guild_members_guild_id ON guild_members(guild_id);", transaction);
                ExecuteNonQuery(connection, "CREATE INDEX idx_guild_members_role ON guild_members(guild_id, role);", transaction);

                // Guild roles table
                ExecuteNonQuery(connection, @"
                    CREATE TABLE guild_roles (
                        guild_id INTEGER NOT NULL,
                        role_name TEXT NOT NULL,
                        description TEXT NOT NULL DEFAULT '',
                        permissions INTEGER NOT NULL DEFAULT 0,
                        hierarchy INTEGER NOT NULL DEFAULT 999,
                        PRIMARY KEY (guild_id, role_name),
                        FOREIGN KEY (guild_id) REFERENCES guilds(id) ON DELETE CASCADE
                    );", transaction);

                ExecuteNonQuery(connection, "CREATE INDEX idx_guild_roles_guild_id ON guild_roles(guild_id);", transaction);

                // Guild invites table
                ExecuteNonQuery(connection, @"
                    CREATE TABLE guild_invites (
                        id INTEGER PRIMARY KEY AUTOINCREMENT,
                        guild_id INTEGER NOT NULL,
                        inviter_uid TEXT NOT NULL,
                        invitee_uid TEXT NOT NULL,
                        created_at INTEGER NOT NULL,
                        expires_at INTEGER NOT NULL,
                        FOREIGN KEY (guild_id) REFERENCES guilds(id) ON DELETE CASCADE
                    );", transaction);

                ExecuteNonQuery(connection, "CREATE INDEX idx_guild_invites_invitee_uid ON guild_invites(invitee_uid);", transaction);
                ExecuteNonQuery(connection, "CREATE INDEX idx_guild_invites_expires_at ON guild_invites(expires_at);", transaction);
                ExecuteNonQuery(connection, "CREATE INDEX idx_guild_invites_guild_id ON guild_invites(guild_id);", transaction);

                // Land claims table
                ExecuteNonQuery(connection, @"
                    CREATE TABLE land_claims (
                        id INTEGER PRIMARY KEY AUTOINCREMENT,
                        guild_id INTEGER NOT NULL,
                        chunk_x INTEGER NOT NULL,
                        chunk_z INTEGER NOT NULL,
                        claim_type TEXT NOT NULL CHECK(claim_type IN ('regular', 'guild_home', 'outpost')),
                        claimed_by_uid TEXT NOT NULL,
                        claimed_at INTEGER NOT NULL,
                        metadata TEXT,
                        FOREIGN KEY (guild_id) REFERENCES guilds(id) ON DELETE CASCADE,
                        UNIQUE (chunk_x, chunk_z)
                    );", transaction);

                ExecuteNonQuery(connection, "CREATE INDEX idx_land_claims_guild_id ON land_claims(guild_id);", transaction);
                ExecuteNonQuery(connection, "CREATE INDEX idx_land_claims_chunk_coords ON land_claims(chunk_x, chunk_z);", transaction);
                ExecuteNonQuery(connection, "CREATE INDEX idx_land_claims_type ON land_claims(guild_id, claim_type);", transaction);

                // Guild tech progress table
                ExecuteNonQuery(connection, @"
                    CREATE TABLE guild_tech_progress (
                        guild_id INTEGER NOT NULL,
                        tech_id INTEGER NOT NULL,
                        is_unlocked INTEGER NOT NULL DEFAULT 0,
                        requires_personal_unlock INTEGER NOT NULL DEFAULT 0,
                        unlocked_at INTEGER,
                        PRIMARY KEY (guild_id, tech_id),
                        FOREIGN KEY (guild_id) REFERENCES guilds(id) ON DELETE CASCADE
                    );", transaction);

                ExecuteNonQuery(connection, "CREATE INDEX idx_guild_tech_progress_guild_id ON guild_tech_progress(guild_id);", transaction);
                ExecuteNonQuery(connection, "CREATE INDEX idx_guild_tech_progress_unlocked ON guild_tech_progress(guild_id, is_unlocked);", transaction);

                // Guild tech contributions table
                ExecuteNonQuery(connection, @"
                    CREATE TABLE guild_tech_contributions (
                        guild_id INTEGER NOT NULL,
                        tech_id INTEGER NOT NULL,
                        resource_group TEXT NOT NULL,
                        amount_submitted INTEGER NOT NULL DEFAULT 0,
                        PRIMARY KEY (guild_id, tech_id, resource_group),
                        FOREIGN KEY (guild_id, tech_id) REFERENCES guild_tech_progress(guild_id, tech_id) ON DELETE CASCADE
                    );", transaction);

                ExecuteNonQuery(connection, "CREATE INDEX idx_guild_tech_contributions_guild_tech ON guild_tech_contributions(guild_id, tech_id);", transaction);

                // Guild member tech progress table (personal unlock status)
                ExecuteNonQuery(connection, @"
                    CREATE TABLE guild_member_tech_progress (
                        guild_id INTEGER NOT NULL,
                        player_uid TEXT NOT NULL,
                        tech_id INTEGER NOT NULL,
                        is_unlocked INTEGER NOT NULL DEFAULT 0,
                        unlocked_at INTEGER,
                        PRIMARY KEY (guild_id, player_uid, tech_id),
                        FOREIGN KEY (guild_id, tech_id) REFERENCES guild_tech_progress(guild_id, tech_id) ON DELETE CASCADE
                    );", transaction);

                ExecuteNonQuery(connection, "CREATE INDEX idx_guild_member_tech_progress_player ON guild_member_tech_progress(player_uid);", transaction);
                ExecuteNonQuery(connection, "CREATE INDEX idx_guild_member_tech_progress_guild_tech ON guild_member_tech_progress(guild_id, tech_id);", transaction);

                // Guild cooldowns table
                ExecuteNonQuery(connection, @"
                    CREATE TABLE guild_cooldowns (
                        player_uid TEXT PRIMARY KEY,
                        expires_at INTEGER NOT NULL
                    );", transaction);

                ExecuteNonQuery(connection, "CREATE INDEX idx_guild_cooldowns_expires_at ON guild_cooldowns(expires_at);", transaction);

                // Zone whitelists table
                // zone_id references ProtectedZone.Id from the config file
                // Zone names are looked up from config at runtime
                ExecuteNonQuery(connection, @"
                    CREATE TABLE zone_whitelists (
                        zone_id INTEGER NOT NULL,
                        player_uid TEXT NOT NULL,
                        PRIMARY KEY (zone_id, player_uid)
                    );", transaction);

                ExecuteNonQuery(connection, "CREATE INDEX idx_zone_whitelists_zone ON zone_whitelists(zone_id);", transaction);
                ExecuteNonQuery(connection, "CREATE INDEX idx_zone_whitelists_player ON zone_whitelists(player_uid);", transaction);

                // Nodes table
                // Stores strategic node locations that guilds can capture/control
                // x and z are absolute world coordinates (not chunk coordinates)
                ExecuteNonQuery(connection, @"
                    CREATE TABLE nodes (
                        id INTEGER PRIMARY KEY AUTOINCREMENT,
                        name TEXT NOT NULL UNIQUE,
                        x INTEGER NOT NULL,
                        z INTEGER NOT NULL,
                        radius INTEGER NOT NULL,
                        created_at INTEGER NOT NULL
                    );", transaction);

                ExecuteNonQuery(connection, "CREATE INDEX idx_nodes_name ON nodes(name);", transaction);
                ExecuteNonQuery(connection, "CREATE INDEX idx_nodes_location ON nodes(x, z);", transaction);

                // Capture zones table
                // Stores capture zones that are part of node wars
                // node_name references nodes.name (using name instead of id for easier management)
                ExecuteNonQuery(connection, @"
                    CREATE TABLE capture_zones (
                        id INTEGER PRIMARY KEY AUTOINCREMENT,
                        node_name TEXT NOT NULL,
                        zone_id TEXT NOT NULL,
                        zone_name TEXT NOT NULL,
                        center_x REAL NOT NULL,
                        center_y REAL NOT NULL,
                        center_z REAL NOT NULL,
                        radius INTEGER NOT NULL,
                        point_multiplier REAL NOT NULL DEFAULT 1.0,
                        is_active INTEGER NOT NULL DEFAULT 1,
                        description TEXT,
                        created_at INTEGER NOT NULL,
                        UNIQUE (node_name, zone_id)
                    );", transaction);

                ExecuteNonQuery(connection, "CREATE INDEX idx_capture_zones_node ON capture_zones(node_name);", transaction);
                ExecuteNonQuery(connection, "CREATE INDEX idx_capture_zones_active ON capture_zones(node_name, is_active);", transaction);

                // Node wars table
                // Stores active and historical node war data
                // status: 'Scheduled', 'Active', 'Completed', 'Cancelled'
                ExecuteNonQuery(connection, @"
                    CREATE TABLE node_wars (
                        id INTEGER PRIMARY KEY AUTOINCREMENT,
                        node_id TEXT NOT NULL,
                        status TEXT NOT NULL CHECK(status IN ('Scheduled', 'Active', 'Completed', 'Cancelled')),
                        start_time INTEGER NOT NULL,
                        end_time INTEGER,
                        signup_deadline INTEGER,
                        max_guilds INTEGER NOT NULL DEFAULT 0,
                        capture_points_needed REAL NOT NULL DEFAULT 10000.0,
                        controlling_guild_uid TEXT,
                        controlling_guild_name TEXT,
                        previous_controlling_guild_uid TEXT,
                        created_at INTEGER NOT NULL,
                        updated_at INTEGER NOT NULL
                    );", transaction);

                ExecuteNonQuery(connection, "CREATE INDEX idx_node_wars_node_id ON node_wars(node_id);", transaction);
                ExecuteNonQuery(connection, "CREATE INDEX idx_node_wars_status ON node_wars(status);", transaction);
                ExecuteNonQuery(connection, "CREATE INDEX idx_node_wars_active ON node_wars(node_id, status) WHERE status IN ('Scheduled', 'Active');", transaction);

                // Guild war signups table
                // Tracks which guilds have signed up for scheduled wars
                ExecuteNonQuery(connection, @"
                    CREATE TABLE guild_war_signups (
                        id INTEGER PRIMARY KEY AUTOINCREMENT,
                        war_id INTEGER NOT NULL,
                        guild_uid TEXT NOT NULL,
                        guild_name TEXT NOT NULL,
                        signup_by_player_uid TEXT NOT NULL,
                        signup_time INTEGER NOT NULL,
                        members_online INTEGER NOT NULL DEFAULT 0,
                        total_members INTEGER NOT NULL DEFAULT 0,
                        is_confirmed INTEGER NOT NULL DEFAULT 1,
                        FOREIGN KEY (war_id) REFERENCES node_wars(id) ON DELETE CASCADE,
                        UNIQUE (war_id, guild_uid)
                    );", transaction);

                ExecuteNonQuery(connection, "CREATE INDEX idx_guild_war_signups_war ON guild_war_signups(war_id);", transaction);
                ExecuteNonQuery(connection, "CREATE INDEX idx_guild_war_signups_guild ON guild_war_signups(guild_uid);", transaction);

                // Guild war progress table
                // Tracks each guild's progress during an active war
                ExecuteNonQuery(connection, @"
                    CREATE TABLE guild_war_progress (
                        id INTEGER PRIMARY KEY AUTOINCREMENT,
                        war_id INTEGER NOT NULL,
                        guild_uid TEXT NOT NULL,
                        guild_name TEXT NOT NULL,
                        capture_points REAL NOT NULL DEFAULT 0.0,
                        players_in_zone INTEGER NOT NULL DEFAULT 0,
                        kills INTEGER NOT NULL DEFAULT 0,
                        deaths INTEGER NOT NULL DEFAULT 0,
                        last_update INTEGER NOT NULL,
                        FOREIGN KEY (war_id) REFERENCES node_wars(id) ON DELETE CASCADE,
                        UNIQUE (war_id, guild_uid)
                    );", transaction);

                ExecuteNonQuery(connection, "CREATE INDEX idx_guild_war_progress_war ON guild_war_progress(war_id);", transaction);
                ExecuteNonQuery(connection, "CREATE INDEX idx_guild_war_progress_guild ON guild_war_progress(guild_uid);", transaction);
                ExecuteNonQuery(connection, "CREATE INDEX idx_guild_war_progress_points ON guild_war_progress(war_id, capture_points DESC);", transaction);

                // Guild quests table
                // recurrence_type: 'daily', 'weekly', 'monthly', 'seasonal' - period-locking behavior
                //
                // requirements: JSON with objectives array, each objective has a type ('turn_in', 'kill', 'craft', etc.)
                // example requirements for a "kill 15 drifters and bring back 15 cupronickel nails and strips" quest:
                //  {
                //    "objectives": [
                //      {
                //        "id": 0,
                //        "type": "kill",
                //        "acceptedTargets": ["game:drifter-*"],
                //        "count": 15,
                //      },
                //      {
                //        "id": 1,
                //        "type": "turn_in",
                //        "acceptedItems": ["metalnailsandstrips-cupronickel"],
                //        "count": 15,
                //      }
                //    ]
                //  }
                //
                // rewards: JSON with items to award upon completion
                // rewards[].code is the item/block code (e.g., "game:fruit-redapple")
                // rewards[].amount is the quantity to award
                // rewards[].nbt is optional Base64-encoded NBT data for the item (null if no NBT)
                // Special codes: "game:grspoints" for guild ranking points (no NBT needed)
                // example rewards for giving GRS points and a temporal gear:
                // {
                //   "rewards": [
                //     { "code": "game:grspoints", "amount": 100, "nbt": null },
                //     { "code": "game:gear-temporal", "amount": 1, "nbt": "AQAAAAtkdXJhYmlsaXR5..." }
                //   ]
                // }
                //
                // starts_at and expires_at are TEXT dates in ISO 8601 format, but the interpretation depends on uses_ingame_time:
                //   - Real-world: "2026-04-21"
                //   - In-game: "0001-04-08"
                // uses_ingame_time determines which format to interpret (0 = real-world, 1 = in-game)
                // For daily/weekly/monthly quests, use real-world time to allow for consistent resets regardless of in-game time progression
                // seasonal quests are the only ones that should use ingame time to allow for seasonal events that follow the in-game calendar instead of real-world time
                // Quest requirements/rewards can be edited before starts_at, but are locked once active
                ExecuteNonQuery(connection, @"
                    CREATE TABLE guild_quests (
                        id INTEGER PRIMARY KEY AUTOINCREMENT,
                        recurrence_type TEXT NOT NULL CHECK(recurrence_type IN ('daily', 'weekly', 'monthly', 'seasonal')),
                        title TEXT NOT NULL,
                        description TEXT NOT NULL,
                        requirements TEXT NOT NULL,
                        rewards TEXT NOT NULL,
                        starts_at TEXT NOT NULL,
                        expires_at TEXT NOT NULL,
                        uses_ingame_time INTEGER NOT NULL DEFAULT 0,
                        repeat INTEGER NOT NULL DEFAULT 0,
                        rank TEXT NOT NULL DEFAULT 'D',
                        created_at INTEGER NOT NULL
                    );", transaction);

                ExecuteNonQuery(connection, "CREATE INDEX idx_guild_quests_expires_at ON guild_quests(expires_at);", transaction);
                ExecuteNonQuery(connection, "CREATE INDEX idx_guild_quests_recurrence ON guild_quests(recurrence_type);", transaction);
                ExecuteNonQuery(connection, "CREATE INDEX idx_guild_quests_active ON guild_quests(recurrence_type, expires_at);", transaction);
                ExecuteNonQuery(connection, "CREATE INDEX idx_guild_quests_time_mode ON guild_quests(uses_ingame_time);", transaction);

                // Guild member quests table for progress/completion tracking
                // status: 'active' = in progress, 'completed' = finished
                // progress: JSON tracking quest objectives by ID
                //   Example: { "objectives": { "0": { "current": 7 }, "1": { "current": 10 } } }
                // recurrence_type: Denormalized from guild_quests for period-locking queries
                // period_key: Only populated when status='completed', used for period-locking
                //   - daily: "daily_2026_04_21" (year_month_day)
                //   - weekly: "weekly_2026_04_03" (year_month_day, Sunday of that week)
                //   - monthly: "monthly_2026_04" (year_month)
                //   - seasonal (real-world): "seasonal_2026_04_01_to_2026_06_30"
                //   - seasonal (in-game): "seasonal_0001_04_01_to_0001_06_15"
                //
                // When a player abandons a quest, delete row
                ExecuteNonQuery(connection, @"
                    CREATE TABLE guild_member_quests (
                        id INTEGER PRIMARY KEY AUTOINCREMENT,
                        player_uid TEXT NOT NULL,
                        quest_id INTEGER NOT NULL,
                        status TEXT NOT NULL DEFAULT 'active' CHECK(status IN ('active', 'completed')),
                        progress TEXT,
                        recurrence_type TEXT NOT NULL,
                        period_key TEXT,
                        started_at INTEGER NOT NULL,
                        completed_at INTEGER,
                        FOREIGN KEY (quest_id) REFERENCES guild_quests(id) ON DELETE CASCADE
                    );", transaction);

                ExecuteNonQuery(connection, "CREATE INDEX idx_member_quests_player ON guild_member_quests(player_uid);", transaction);
                ExecuteNonQuery(connection, "CREATE INDEX idx_member_quests_quest ON guild_member_quests(quest_id);", transaction);
                ExecuteNonQuery(connection, "CREATE INDEX idx_member_quests_status ON guild_member_quests(player_uid, status);", transaction);
                ExecuteNonQuery(connection, "CREATE INDEX idx_member_quests_active ON guild_member_quests(quest_id, status);", transaction);

                // Guild weekly points table
                // Tracks GRS points earned per week by each guild for weekly limit enforcement
                // week_key: ISO week format "YYYY-Wnn" (e.g., "2024-W52"), weeks start Sunday 12am EST
                // points_earned: Total GRS points earned by the guild during this week
                // week_start_unix: Unix timestamp of the Sunday 12am EST when the week started (for reference)
                ExecuteNonQuery(connection, @"
                    CREATE TABLE guild_weekly_points (
                        guild_id INTEGER NOT NULL,
                        week_key TEXT NOT NULL,
                        points_earned INTEGER NOT NULL DEFAULT 0,
                        week_start_unix INTEGER NOT NULL,
                        PRIMARY KEY (guild_id, week_key),
                        FOREIGN KEY (guild_id) REFERENCES guilds(id) ON DELETE CASCADE
                    );", transaction);

                ExecuteNonQuery(connection, "CREATE INDEX idx_weekly_points_guild ON guild_weekly_points(guild_id);", transaction);
                ExecuteNonQuery(connection, "CREATE INDEX idx_weekly_points_week ON guild_weekly_points(week_key);", transaction);

                ExecuteNonQuery(connection, @"
                    CREATE TABLE guild_events (
                        id INTEGER PRIMARY KEY AUTOINCREMENT,
                        name TEXT NOT NULL,
                        description TEXT,
                        max_players INTEGER NOT NULL DEFAULT 1,
                        start_date TEXT NOT NULL,
                        end_date TEXT NOT NULL,
                        location_x INTEGER,
                        location_y INTEGER,
                        location_z INTEGER,
                        created_at INTEGER NOT NULL
                    );", transaction);

                ExecuteNonQuery(connection, "CREATE INDEX idx_guild_events_start_date ON guild_events(start_date);", transaction);
                ExecuteNonQuery(connection, "CREATE INDEX idx_guild_events_end_date ON guild_events(end_date);", transaction);

                ExecuteNonQuery(connection, @"
                    CREATE TABLE guild_event_registrations (
                        event_id INTEGER NOT NULL,
                        registree_uid TEXT NOT NULL,
                        registration_date TEXT NOT NULL,
                        PRIMARY KEY (event_id, registree_uid),
                        FOREIGN KEY (event_id) REFERENCES guild_events(id) ON DELETE CASCADE
                    );", transaction);

                ExecuteNonQuery(connection, "CREATE INDEX idx_guild_event_registrations_event ON guild_event_registrations(event_id);", transaction);
                ExecuteNonQuery(connection, "CREATE INDEX idx_guild_event_registrations_registree ON guild_event_registrations(registree_uid);", transaction);

                transaction.Commit();
                serverApi.Logger.Notification("[SRGuildsAndKingdoms] All tables created successfully");
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                serverApi.Logger.Error($"[SRGuildsAndKingdoms] Failed to create tables: {ex.Message}");
                throw;
            }
        }

        private void ApplyMigrations(SqliteConnection connection, int fromVersion)
        {
            using var transaction = connection.BeginTransaction();
            try
            {
                if (fromVersion < 2)
                {
                    serverApi.Logger.Notification("[SRGuildsAndKingdoms] Applying migration to version 2: Adding repeat column to guild_quests");
                    ExecuteNonQuery(connection, "ALTER TABLE guild_quests ADD COLUMN repeat INTEGER NOT NULL DEFAULT 0;", transaction);
                }

                if (fromVersion < 3)
                {
                    serverApi.Logger.Notification("[SRGuildsAndKingdoms] Applying migration to version 3: Adding points_contribution column to guild_members and removing composite PRIMARY KEY from guild_member_quests");

                    // Add points_contribution to guild_members
                    ExecuteNonQuery(connection, "ALTER TABLE guild_members ADD COLUMN points_contribution INTEGER NOT NULL DEFAULT 0;", transaction);

                    // Calculate and populate points_contribution from completed quests
                    serverApi.Logger.Notification("[SRGuildsAndKingdoms] Calculating points_contribution for existing members...");
                    ExecuteNonQuery(connection, @"
                        UPDATE guild_members
                        SET points_contribution = (
                            SELECT COALESCE(SUM(json_extract(reward.value, '$.amount')), 0)
                            FROM guild_member_quests gmq
                            JOIN guild_quests gq ON gmq.quest_id = gq.id
                            CROSS JOIN json_each(gq.rewards, '$.rewards') AS reward
                            WHERE gmq.player_uid = guild_members.player_uid
                              AND gmq.status = 'completed'
                              AND json_extract(reward.value, '$.code') = 'game:grspoints'
                        );", transaction);

                    // Recreate guild_member_quests without composite PRIMARY KEY
                    ExecuteNonQuery(connection, @"
                        CREATE TABLE guild_member_quests_new (
                            id INTEGER PRIMARY KEY AUTOINCREMENT,
                            player_uid TEXT NOT NULL,
                            quest_id INTEGER NOT NULL,
                            status TEXT NOT NULL DEFAULT 'active' CHECK(status IN ('active', 'completed')),
                            progress TEXT,
                            recurrence_type TEXT NOT NULL,
                            period_key TEXT,
                            started_at INTEGER NOT NULL,
                            completed_at INTEGER,
                            FOREIGN KEY (quest_id) REFERENCES guild_quests(id) ON DELETE CASCADE
                        );", transaction);

                    // Copy data from old table to new table
                    ExecuteNonQuery(connection, @"
                        INSERT INTO guild_member_quests_new (player_uid, quest_id, status, progress, recurrence_type, period_key, started_at, completed_at)
                        SELECT player_uid, quest_id, status, progress, recurrence_type, period_key, started_at, completed_at
                        FROM guild_member_quests;", transaction);

                    // Drop old table
                    ExecuteNonQuery(connection, "DROP TABLE guild_member_quests;", transaction);

                    // Rename new table to original name
                    ExecuteNonQuery(connection, "ALTER TABLE guild_member_quests_new RENAME TO guild_member_quests;", transaction);

                    // Recreate indexes
                    ExecuteNonQuery(connection, "CREATE INDEX idx_member_quests_player ON guild_member_quests(player_uid);", transaction);
                    ExecuteNonQuery(connection, "CREATE INDEX idx_member_quests_quest ON guild_member_quests(quest_id);", transaction);
                    ExecuteNonQuery(connection, "CREATE INDEX idx_member_quests_status ON guild_member_quests(player_uid, status);", transaction);
                    ExecuteNonQuery(connection, "CREATE INDEX idx_member_quests_active ON guild_member_quests(quest_id, status);", transaction);

                    // Recreate the period lock unique index
                    ExecuteNonQuery(connection, @"
                        CREATE UNIQUE INDEX idx_period_lock
                        ON guild_member_quests(player_uid, recurrence_type, period_key) 
                        WHERE period_key IS NOT NULL;", transaction);
                }

                if (fromVersion < 4)
                {
                    serverApi.Logger.Notification("[SRGuildsAndKingdoms] Applying migration to version 4: Adding nodes table");

                    // Create nodes table
                    ExecuteNonQuery(connection, @"
                        CREATE TABLE nodes (
                            id INTEGER PRIMARY KEY AUTOINCREMENT,
                            name TEXT NOT NULL UNIQUE,
                            x INTEGER NOT NULL,
                            z INTEGER NOT NULL,
                            radius INTEGER NOT NULL,
                            created_at INTEGER NOT NULL
                        );", transaction);

                    ExecuteNonQuery(connection, "CREATE INDEX idx_nodes_name ON nodes(name);", transaction);
                    ExecuteNonQuery(connection, "CREATE INDEX idx_nodes_location ON nodes(x, z);", transaction);
                }

                if (fromVersion < 5)
                {
                    serverApi.Logger.Notification("[SRGuildsAndKingdoms] Applying migration to version 5: Adding capture_zones table");

                    // Create capture zones table
                    ExecuteNonQuery(connection, @"
                        CREATE TABLE capture_zones (
                            id INTEGER PRIMARY KEY AUTOINCREMENT,
                            node_name TEXT NOT NULL,
                            zone_id TEXT NOT NULL,
                            zone_name TEXT NOT NULL,
                            center_x REAL NOT NULL,
                            center_y REAL NOT NULL,
                            center_z REAL NOT NULL,
                            radius INTEGER NOT NULL,
                            point_multiplier REAL NOT NULL DEFAULT 1.0,
                            is_active INTEGER NOT NULL DEFAULT 1,
                            description TEXT,
                            created_at INTEGER NOT NULL,
                            UNIQUE (node_name, zone_id)
                        );", transaction);

                    ExecuteNonQuery(connection, "CREATE INDEX idx_capture_zones_node ON capture_zones(node_name);", transaction);
                    ExecuteNonQuery(connection, "CREATE INDEX idx_capture_zones_active ON capture_zones(node_name, is_active);", transaction);
                }

                if (fromVersion < 6)
                {
                    serverApi.Logger.Notification("[SRGuildsAndKingdoms] Applying migration to version 6: Adding node_wars tables");

                    // Create node wars table
                    ExecuteNonQuery(connection, @"
                        CREATE TABLE node_wars (
                            id INTEGER PRIMARY KEY AUTOINCREMENT,
                            node_id TEXT NOT NULL,
                            status TEXT NOT NULL CHECK(status IN ('Scheduled', 'Active', 'Completed', 'Cancelled')),
                            start_time INTEGER NOT NULL,
                            end_time INTEGER,
                            signup_deadline INTEGER,
                            max_guilds INTEGER NOT NULL DEFAULT 0,
                            capture_points_needed REAL NOT NULL DEFAULT 10000.0,
                            controlling_guild_uid TEXT,
                            controlling_guild_name TEXT,
                            previous_controlling_guild_uid TEXT,
                            created_at INTEGER NOT NULL,
                            updated_at INTEGER NOT NULL
                        );", transaction);

                    ExecuteNonQuery(connection, "CREATE INDEX idx_node_wars_node_id ON node_wars(node_id);", transaction);
                    ExecuteNonQuery(connection, "CREATE INDEX idx_node_wars_status ON node_wars(status);", transaction);
                    ExecuteNonQuery(connection, "CREATE INDEX idx_node_wars_active ON node_wars(node_id, status) WHERE status IN ('Scheduled', 'Active');", transaction);

                    // Create guild war signups table
                    ExecuteNonQuery(connection, @"
                        CREATE TABLE guild_war_signups (
                            id INTEGER PRIMARY KEY AUTOINCREMENT,
                            war_id INTEGER NOT NULL,
                            guild_uid TEXT NOT NULL,
                            guild_name TEXT NOT NULL,
                            signup_by_player_uid TEXT NOT NULL,
                            signup_time INTEGER NOT NULL,
                            members_online INTEGER NOT NULL DEFAULT 0,
                            total_members INTEGER NOT NULL DEFAULT 0,
                            is_confirmed INTEGER NOT NULL DEFAULT 1,
                            FOREIGN KEY (war_id) REFERENCES node_wars(id) ON DELETE CASCADE,
                            UNIQUE (war_id, guild_uid)
                        );", transaction);

                    ExecuteNonQuery(connection, "CREATE INDEX idx_guild_war_signups_war ON guild_war_signups(war_id);", transaction);
                    ExecuteNonQuery(connection, "CREATE INDEX idx_guild_war_signups_guild ON guild_war_signups(guild_uid);", transaction);

                    // Create guild war progress table
                    ExecuteNonQuery(connection, @"
                        CREATE TABLE guild_war_progress (
                            id INTEGER PRIMARY KEY AUTOINCREMENT,
                            war_id INTEGER NOT NULL,
                            guild_uid TEXT NOT NULL,
                            guild_name TEXT NOT NULL,
                            capture_points REAL NOT NULL DEFAULT 0.0,
                            players_in_zone INTEGER NOT NULL DEFAULT 0,
                            kills INTEGER NOT NULL DEFAULT 0,
                            deaths INTEGER NOT NULL DEFAULT 0,
                            last_update INTEGER NOT NULL,
                            FOREIGN KEY (war_id) REFERENCES node_wars(id) ON DELETE CASCADE,
                            UNIQUE (war_id, guild_uid)
                        );", transaction);

                    ExecuteNonQuery(connection, "CREATE INDEX idx_guild_war_progress_war ON guild_war_progress(war_id);", transaction);
                    ExecuteNonQuery(connection, "CREATE INDEX idx_guild_war_progress_guild ON guild_war_progress(guild_uid);", transaction);
                    ExecuteNonQuery(connection, "CREATE INDEX idx_guild_war_progress_points ON guild_war_progress(war_id, capture_points DESC);", transaction);
                }

                if (fromVersion < 7)
                {
                    serverApi.Logger.Notification("[SRGuildsAndKingdoms] Applying migration to version 7: Adding rank column to guild_quests, removing daily/weekly quests, and removing period lock index");

                    ExecuteNonQuery(connection, "DELETE FROM guild_quests WHERE recurrence_type IN ('daily', 'weekly');", transaction);
                    serverApi.Logger.Notification("[SRGuildsAndKingdoms] Removed all daily and weekly quests from guild_quests table");

                    ExecuteNonQuery(connection, "ALTER TABLE guild_quests ADD COLUMN rank TEXT NOT NULL DEFAULT 'D';", transaction);

                    ExecuteNonQuery(connection, "DROP INDEX IF EXISTS idx_period_lock;", transaction);
                    serverApi.Logger.Notification("[SRGuildsAndKingdoms] Removed idx_period_lock unique index");
                }

                if (fromVersion < 8)
                {
                    serverApi.Logger.Notification("[SRGuildsAndKingdoms] Applying migration to version 8: Adding guild_weekly_points table for weekly GRS points tracking");

                    ExecuteNonQuery(connection, @"
                        CREATE TABLE guild_weekly_points (
                            guild_id INTEGER NOT NULL,
                            week_key TEXT NOT NULL,
                            points_earned INTEGER NOT NULL DEFAULT 0,
                            week_start_unix INTEGER NOT NULL,
                            PRIMARY KEY (guild_id, week_key),
                            FOREIGN KEY (guild_id) REFERENCES guilds(id) ON DELETE CASCADE
                        );", transaction);

                    ExecuteNonQuery(connection, "CREATE INDEX idx_weekly_points_guild ON guild_weekly_points(guild_id);", transaction);
                    ExecuteNonQuery(connection, "CREATE INDEX idx_weekly_points_week ON guild_weekly_points(week_key);", transaction);
                }

                if (fromVersion < 9)
                {
                    serverApi.Logger.Notification("[SRGuildsAndKingdoms] Applying migration to version 9: Adding guild_events and guild_event_registrations tables");

                    ExecuteNonQuery(connection, @"
                        CREATE TABLE guild_events (
                            id INTEGER PRIMARY KEY AUTOINCREMENT,
                            name TEXT NOT NULL,
                            description TEXT,
                            max_players INTEGER NOT NULL DEFAULT 1,
                            start_date TEXT NOT NULL,
                            end_date TEXT NOT NULL,
                            location_x INTEGER,
                            location_y INTEGER,
                            location_z INTEGER,
                            created_at INTEGER NOT NULL
                        );", transaction);

                    ExecuteNonQuery(connection, "CREATE INDEX idx_guild_events_start_date ON guild_events(start_date);", transaction);
                    ExecuteNonQuery(connection, "CREATE INDEX idx_guild_events_end_date ON guild_events(end_date);", transaction);

                    ExecuteNonQuery(connection, @"
                        CREATE TABLE guild_event_registrations (
                            event_id INTEGER NOT NULL,
                            registree_uid TEXT NOT NULL,
                            registration_date TEXT NOT NULL,
                            PRIMARY KEY (event_id, registree_uid),
                            FOREIGN KEY (event_id) REFERENCES guild_events(id) ON DELETE CASCADE
                        );", transaction);

                    ExecuteNonQuery(connection, "CREATE INDEX idx_guild_event_registrations_event ON guild_event_registrations(event_id);", transaction);
                    ExecuteNonQuery(connection, "CREATE INDEX idx_guild_event_registrations_registree ON guild_event_registrations(registree_uid);", transaction);
                }

                transaction.Commit();
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                serverApi.Logger.Error($"[SRGuildsAndKingdoms] Failed to apply migrations: {ex.Message}");
                throw;
            }
        }

        private void ExecuteNonQuery(SqliteConnection connection, string sql, SqliteTransaction? transaction = null)
        {
            using var command = new SqliteCommand(sql, connection);
            if (transaction != null)
            {
                command.Transaction = transaction;
            }
            command.ExecuteNonQuery();
        }

        /// <summary>
        /// Verifies that the schema is properly initialized
        /// </summary>
        public bool VerifySchema(SqliteConnection connection)
        {
            try
            {
                // Check for key tables
                string[] requiredTables = {
                    "guilds", "guild_members", "guild_roles", "guild_invites",
                    "land_claims", "guild_tech_progress", "guild_tech_contributions",
                    "guild_member_tech_progress",
                    "guild_cooldowns", "zone_whitelists", "nodes", "capture_zones",
                    "guild_quests", "guild_member_quests", "guild_events", "guild_event_registrations"
                };

                foreach (var tableName in requiredTables)
                {
                    if (!TableExists(connection, tableName))
                    {
                        serverApi.Logger.Error($"[SRGuildsAndKingdoms] Required table '{tableName}' does not exist");
                        return false;
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                serverApi.Logger.Error($"[SRGuildsAndKingdoms] Schema verification failed: {ex.Message}");
                return false;
            }
        }

        private bool TableExists(SqliteConnection connection, string tableName)
        {
            const string sql = "SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name=@tableName;";
            using var command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("@tableName", tableName);
            var result = command.ExecuteScalar();
            return Convert.ToInt32(result) > 0;
        }
    }
}
