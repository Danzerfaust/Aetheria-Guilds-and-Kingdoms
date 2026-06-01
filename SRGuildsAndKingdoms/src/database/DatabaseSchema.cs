using System;
using System.Runtime.CompilerServices;
using Microsoft.Data.Sqlite;
using Vintagestory.API.Common;
using Vintagestory.API.Server;

namespace SRGuildsAndKingdoms.src.database
{
	// Token: 0x020000B2 RID: 178
	[NullableContext(1)]
	[Nullable(0)]
	public class DatabaseSchema
	{
		// Token: 0x06000820 RID: 2080 RVA: 0x00038CE4 File Offset: 0x00036EE4
		public DatabaseSchema(ICoreServerAPI serverApi)
		{
			this.serverApi = serverApi;
		}

		// Token: 0x06000821 RID: 2081 RVA: 0x00038CF4 File Offset: 0x00036EF4
		public void InitializeSchema(SqliteConnection connection)
		{
			try
			{
				this.CreateSchemaVersionTable(connection);
				int currentVersion = this.GetCurrentSchemaVersion(connection);
				if (currentVersion == 0)
				{
					this.serverApi.Logger.Notification("[SRGuildsAndKingdoms] Initializing database schema...");
					this.CreateAllTables(connection);
					this.SetSchemaVersion(connection, 6);
					ILogger logger = this.serverApi.Logger;
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(52, 1);
					defaultInterpolatedStringHandler.AppendLiteral("[SRGuildsAndKingdoms] Schema initialized to version ");
					defaultInterpolatedStringHandler.AppendFormatted<int>(6);
					logger.Notification(defaultInterpolatedStringHandler.ToStringAndClear());
				}
				else if (currentVersion < 6)
				{
					ILogger logger2 = this.serverApi.Logger;
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(56, 2);
					defaultInterpolatedStringHandler2.AppendLiteral("[SRGuildsAndKingdoms] Migrating schema from version ");
					defaultInterpolatedStringHandler2.AppendFormatted<int>(currentVersion);
					defaultInterpolatedStringHandler2.AppendLiteral(" to ");
					defaultInterpolatedStringHandler2.AppendFormatted<int>(6);
					logger2.Notification(defaultInterpolatedStringHandler2.ToStringAndClear());
					this.ApplyMigrations(connection, currentVersion);
					this.SetSchemaVersion(connection, 6);
					this.serverApi.Logger.Notification("[SRGuildsAndKingdoms] Schema migration complete");
				}
				else if (currentVersion > 6)
				{
					ILogger logger3 = this.serverApi.Logger;
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler3 = new DefaultInterpolatedStringHandler(94, 2);
					defaultInterpolatedStringHandler3.AppendLiteral("[SRGuildsAndKingdoms] Database schema version ");
					defaultInterpolatedStringHandler3.AppendFormatted<int>(currentVersion);
					defaultInterpolatedStringHandler3.AppendLiteral(" is newer than expected ");
					defaultInterpolatedStringHandler3.AppendFormatted<int>(6);
					defaultInterpolatedStringHandler3.AppendLiteral(". Please update the mod.");
					logger3.Error(defaultInterpolatedStringHandler3.ToStringAndClear());
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler4 = new DefaultInterpolatedStringHandler(37, 2);
					defaultInterpolatedStringHandler4.AppendLiteral("Database schema version mismatch: ");
					defaultInterpolatedStringHandler4.AppendFormatted<int>(currentVersion);
					defaultInterpolatedStringHandler4.AppendLiteral(" > ");
					defaultInterpolatedStringHandler4.AppendFormatted<int>(6);
					throw new InvalidOperationException(defaultInterpolatedStringHandler4.ToStringAndClear());
				}
			}
			catch (Exception ex)
			{
				this.serverApi.Logger.Error("[SRGuildsAndKingdoms] Failed to initialize schema: " + ex.Message);
				this.serverApi.Logger.Error("[SRGuildsAndKingdoms] Stack trace: " + ex.StackTrace);
				throw;
			}
		}

		// Token: 0x06000822 RID: 2082 RVA: 0x00038EE4 File Offset: 0x000370E4
		private void CreateSchemaVersionTable(SqliteConnection connection)
		{
			using (SqliteCommand command = new SqliteCommand("\n                CREATE TABLE IF NOT EXISTS schema_version (\n                    version INTEGER PRIMARY KEY,\n                    applied_at INTEGER NOT NULL\n                );", connection))
			{
				command.ExecuteNonQuery();
			}
		}

		// Token: 0x06000823 RID: 2083 RVA: 0x00038F20 File Offset: 0x00037120
		private int GetCurrentSchemaVersion(SqliteConnection connection)
		{
			int result;
			using (SqliteCommand command = new SqliteCommand("SELECT COALESCE(MAX(version), 0) FROM schema_version;", connection))
			{
				result = Convert.ToInt32(command.ExecuteScalar());
			}
			return result;
		}

		// Token: 0x06000824 RID: 2084 RVA: 0x00038F64 File Offset: 0x00037164
		private void SetSchemaVersion(SqliteConnection connection, int version)
		{
			using (SqliteCommand command = new SqliteCommand("INSERT INTO schema_version (version, applied_at) VALUES (@version, @appliedAt);", connection))
			{
				command.Parameters.AddWithValue("@version", version);
				command.Parameters.AddWithValue("@appliedAt", DateTimeOffset.UtcNow.ToUnixTimeSeconds());
				command.ExecuteNonQuery();
			}
		}

		// Token: 0x06000825 RID: 2085 RVA: 0x00038FDC File Offset: 0x000371DC
		private void CreateAllTables(SqliteConnection connection)
		{
			using (SqliteTransaction transaction = connection.BeginTransaction())
			{
				try
				{
					this.ExecuteNonQuery(connection, "\n                    CREATE TABLE guilds (\n                        id INTEGER PRIMARY KEY AUTOINCREMENT,\n                        name TEXT NOT NULL UNIQUE COLLATE NOCASE,\n                        description TEXT NOT NULL DEFAULT '',\n                        display_color INTEGER NOT NULL,\n                        secondary_color INTEGER NOT NULL,\n                        points INTEGER NOT NULL DEFAULT 0,\n                        created_at INTEGER NOT NULL,\n                        updated_at INTEGER NOT NULL\n                    );", transaction);
					this.ExecuteNonQuery(connection, "CREATE INDEX idx_guilds_name ON guilds(name COLLATE NOCASE);", transaction);
					this.ExecuteNonQuery(connection, "CREATE INDEX idx_guilds_points ON guilds(points DESC);", transaction);
					this.ExecuteNonQuery(connection, "CREATE INDEX idx_guilds_created_at ON guilds(created_at);", transaction);
					this.ExecuteNonQuery(connection, "\n                    CREATE TABLE guild_members (\n                        guild_id INTEGER NOT NULL,\n                        player_uid TEXT NOT NULL,\n                        role TEXT NOT NULL,\n                        joined_at INTEGER NOT NULL,\n                        last_seen INTEGER NOT NULL,\n                        PRIMARY KEY (guild_id, player_uid),\n                        FOREIGN KEY (guild_id) REFERENCES guilds(id) ON DELETE CASCADE\n                    );", transaction);
					this.ExecuteNonQuery(connection, "CREATE INDEX idx_guild_members_player_uid ON guild_members(player_uid);", transaction);
					this.ExecuteNonQuery(connection, "CREATE INDEX idx_guild_members_guild_id ON guild_members(guild_id);", transaction);
					this.ExecuteNonQuery(connection, "CREATE INDEX idx_guild_members_role ON guild_members(guild_id, role);", transaction);
					this.ExecuteNonQuery(connection, "\n                    CREATE TABLE guild_roles (\n                        guild_id INTEGER NOT NULL,\n                        role_name TEXT NOT NULL,\n                        description TEXT NOT NULL DEFAULT '',\n                        permissions INTEGER NOT NULL DEFAULT 0,\n                        hierarchy INTEGER NOT NULL DEFAULT 999,\n                        PRIMARY KEY (guild_id, role_name),\n                        FOREIGN KEY (guild_id) REFERENCES guilds(id) ON DELETE CASCADE\n                    );", transaction);
					this.ExecuteNonQuery(connection, "CREATE INDEX idx_guild_roles_guild_id ON guild_roles(guild_id);", transaction);
					this.ExecuteNonQuery(connection, "\n                    CREATE TABLE guild_invites (\n                        id INTEGER PRIMARY KEY AUTOINCREMENT,\n                        guild_id INTEGER NOT NULL,\n                        inviter_uid TEXT NOT NULL,\n                        invitee_uid TEXT NOT NULL,\n                        created_at INTEGER NOT NULL,\n                        expires_at INTEGER NOT NULL,\n                        FOREIGN KEY (guild_id) REFERENCES guilds(id) ON DELETE CASCADE\n                    );", transaction);
					this.ExecuteNonQuery(connection, "CREATE INDEX idx_guild_invites_invitee_uid ON guild_invites(invitee_uid);", transaction);
					this.ExecuteNonQuery(connection, "CREATE INDEX idx_guild_invites_expires_at ON guild_invites(expires_at);", transaction);
					this.ExecuteNonQuery(connection, "CREATE INDEX idx_guild_invites_guild_id ON guild_invites(guild_id);", transaction);
					this.ExecuteNonQuery(connection, "\n                    CREATE TABLE land_claims (\n                        id INTEGER PRIMARY KEY AUTOINCREMENT,\n                        guild_id INTEGER NOT NULL,\n                        chunk_x INTEGER NOT NULL,\n                        chunk_z INTEGER NOT NULL,\n                        claim_type TEXT NOT NULL CHECK(claim_type IN ('regular', 'guild_home', 'outpost')),\n                        claimed_by_uid TEXT NOT NULL,\n                        claimed_at INTEGER NOT NULL,\n                        metadata TEXT,\n                        FOREIGN KEY (guild_id) REFERENCES guilds(id) ON DELETE CASCADE,\n                        UNIQUE (chunk_x, chunk_z)\n                    );", transaction);
					this.ExecuteNonQuery(connection, "CREATE INDEX idx_land_claims_guild_id ON land_claims(guild_id);", transaction);
					this.ExecuteNonQuery(connection, "CREATE INDEX idx_land_claims_chunk_coords ON land_claims(chunk_x, chunk_z);", transaction);
					this.ExecuteNonQuery(connection, "CREATE INDEX idx_land_claims_type ON land_claims(guild_id, claim_type);", transaction);
					this.ExecuteNonQuery(connection, "\n                    CREATE TABLE guild_tech_progress (\n                        guild_id INTEGER NOT NULL,\n                        tech_id INTEGER NOT NULL,\n                        is_unlocked INTEGER NOT NULL DEFAULT 0,\n                        requires_personal_unlock INTEGER NOT NULL DEFAULT 0,\n                        unlocked_at INTEGER,\n                        PRIMARY KEY (guild_id, tech_id),\n                        FOREIGN KEY (guild_id) REFERENCES guilds(id) ON DELETE CASCADE\n                    );", transaction);
					this.ExecuteNonQuery(connection, "CREATE INDEX idx_guild_tech_progress_guild_id ON guild_tech_progress(guild_id);", transaction);
					this.ExecuteNonQuery(connection, "CREATE INDEX idx_guild_tech_progress_unlocked ON guild_tech_progress(guild_id, is_unlocked);", transaction);
					this.ExecuteNonQuery(connection, "\n                    CREATE TABLE guild_tech_contributions (\n                        guild_id INTEGER NOT NULL,\n                        tech_id INTEGER NOT NULL,\n                        resource_group TEXT NOT NULL,\n                        amount_submitted INTEGER NOT NULL DEFAULT 0,\n                        PRIMARY KEY (guild_id, tech_id, resource_group),\n                        FOREIGN KEY (guild_id, tech_id) REFERENCES guild_tech_progress(guild_id, tech_id) ON DELETE CASCADE\n                    );", transaction);
					this.ExecuteNonQuery(connection, "CREATE INDEX idx_guild_tech_contributions_guild_tech ON guild_tech_contributions(guild_id, tech_id);", transaction);
					this.ExecuteNonQuery(connection, "\n                    CREATE TABLE guild_member_tech_progress (\n                        guild_id INTEGER NOT NULL,\n                        player_uid TEXT NOT NULL,\n                        tech_id INTEGER NOT NULL,\n                        is_unlocked INTEGER NOT NULL DEFAULT 0,\n                        unlocked_at INTEGER,\n                        PRIMARY KEY (guild_id, player_uid, tech_id),\n                        FOREIGN KEY (guild_id, tech_id) REFERENCES guild_tech_progress(guild_id, tech_id) ON DELETE CASCADE\n                    );", transaction);
					this.ExecuteNonQuery(connection, "CREATE INDEX idx_guild_member_tech_progress_player ON guild_member_tech_progress(player_uid);", transaction);
					this.ExecuteNonQuery(connection, "CREATE INDEX idx_guild_member_tech_progress_guild_tech ON guild_member_tech_progress(guild_id, tech_id);", transaction);
					this.ExecuteNonQuery(connection, "\n                    CREATE TABLE guild_cooldowns (\n                        player_uid TEXT PRIMARY KEY,\n                        expires_at INTEGER NOT NULL\n                    );", transaction);
					this.ExecuteNonQuery(connection, "CREATE INDEX idx_guild_cooldowns_expires_at ON guild_cooldowns(expires_at);", transaction);
					this.ExecuteNonQuery(connection, "\n                    CREATE TABLE zone_whitelists (\n                        zone_id INTEGER NOT NULL,\n                        player_uid TEXT NOT NULL,\n                        PRIMARY KEY (zone_id, player_uid)\n                    );", transaction);
					this.ExecuteNonQuery(connection, "CREATE INDEX idx_zone_whitelists_zone ON zone_whitelists(zone_id);", transaction);
					this.ExecuteNonQuery(connection, "CREATE INDEX idx_zone_whitelists_player ON zone_whitelists(player_uid);", transaction);
					this.ExecuteNonQuery(connection, "\n                    CREATE TABLE nodes (\n                        id INTEGER PRIMARY KEY AUTOINCREMENT,\n                        name TEXT NOT NULL UNIQUE,\n                        x INTEGER NOT NULL,\n                        z INTEGER NOT NULL,\n                        radius INTEGER NOT NULL,\n                        created_at INTEGER NOT NULL\n                    );", transaction);
					this.ExecuteNonQuery(connection, "CREATE INDEX idx_nodes_name ON nodes(name);", transaction);
					this.ExecuteNonQuery(connection, "CREATE INDEX idx_nodes_location ON nodes(x, z);", transaction);
					this.ExecuteNonQuery(connection, "\n                    CREATE TABLE capture_zones (\n                        id INTEGER PRIMARY KEY AUTOINCREMENT,\n                        node_name TEXT NOT NULL,\n                        zone_id TEXT NOT NULL,\n                        zone_name TEXT NOT NULL,\n                        center_x REAL NOT NULL,\n                        center_y REAL NOT NULL,\n                        center_z REAL NOT NULL,\n                        radius INTEGER NOT NULL,\n                        point_multiplier REAL NOT NULL DEFAULT 1.0,\n                        is_active INTEGER NOT NULL DEFAULT 1,\n                        description TEXT,\n                        created_at INTEGER NOT NULL,\n                        UNIQUE (node_name, zone_id)\n                    );", transaction);
					this.ExecuteNonQuery(connection, "CREATE INDEX idx_capture_zones_node ON capture_zones(node_name);", transaction);
					this.ExecuteNonQuery(connection, "CREATE INDEX idx_capture_zones_active ON capture_zones(node_name, is_active);", transaction);
					this.ExecuteNonQuery(connection, "\n                    CREATE TABLE node_wars (\n                        id INTEGER PRIMARY KEY AUTOINCREMENT,\n                        node_id TEXT NOT NULL,\n                        status TEXT NOT NULL CHECK(status IN ('Scheduled', 'Active', 'Completed', 'Cancelled')),\n                        start_time INTEGER NOT NULL,\n                        end_time INTEGER,\n                        signup_deadline INTEGER,\n                        max_guilds INTEGER NOT NULL DEFAULT 0,\n                        capture_points_needed REAL NOT NULL DEFAULT 10000.0,\n                        controlling_guild_uid TEXT,\n                        controlling_guild_name TEXT,\n                        previous_controlling_guild_uid TEXT,\n                        created_at INTEGER NOT NULL,\n                        updated_at INTEGER NOT NULL\n                    );", transaction);
					this.ExecuteNonQuery(connection, "CREATE INDEX idx_node_wars_node_id ON node_wars(node_id);", transaction);
					this.ExecuteNonQuery(connection, "CREATE INDEX idx_node_wars_status ON node_wars(status);", transaction);
					this.ExecuteNonQuery(connection, "CREATE INDEX idx_node_wars_active ON node_wars(node_id, status) WHERE status IN ('Scheduled', 'Active');", transaction);
					this.ExecuteNonQuery(connection, "\n                    CREATE TABLE guild_war_signups (\n                        id INTEGER PRIMARY KEY AUTOINCREMENT,\n                        war_id INTEGER NOT NULL,\n                        guild_uid TEXT NOT NULL,\n                        guild_name TEXT NOT NULL,\n                        signup_by_player_uid TEXT NOT NULL,\n                        signup_time INTEGER NOT NULL,\n                        members_online INTEGER NOT NULL DEFAULT 0,\n                        total_members INTEGER NOT NULL DEFAULT 0,\n                        is_confirmed INTEGER NOT NULL DEFAULT 1,\n                        FOREIGN KEY (war_id) REFERENCES node_wars(id) ON DELETE CASCADE,\n                        UNIQUE (war_id, guild_uid)\n                    );", transaction);
					this.ExecuteNonQuery(connection, "CREATE INDEX idx_guild_war_signups_war ON guild_war_signups(war_id);", transaction);
					this.ExecuteNonQuery(connection, "CREATE INDEX idx_guild_war_signups_guild ON guild_war_signups(guild_uid);", transaction);
					this.ExecuteNonQuery(connection, "\n                    CREATE TABLE guild_war_progress (\n                        id INTEGER PRIMARY KEY AUTOINCREMENT,\n                        war_id INTEGER NOT NULL,\n                        guild_uid TEXT NOT NULL,\n                        guild_name TEXT NOT NULL,\n                        capture_points REAL NOT NULL DEFAULT 0.0,\n                        players_in_zone INTEGER NOT NULL DEFAULT 0,\n                        kills INTEGER NOT NULL DEFAULT 0,\n                        deaths INTEGER NOT NULL DEFAULT 0,\n                        last_update INTEGER NOT NULL,\n                        FOREIGN KEY (war_id) REFERENCES node_wars(id) ON DELETE CASCADE,\n                        UNIQUE (war_id, guild_uid)\n                    );", transaction);
					this.ExecuteNonQuery(connection, "CREATE INDEX idx_guild_war_progress_war ON guild_war_progress(war_id);", transaction);
					this.ExecuteNonQuery(connection, "CREATE INDEX idx_guild_war_progress_guild ON guild_war_progress(guild_uid);", transaction);
					this.ExecuteNonQuery(connection, "CREATE INDEX idx_guild_war_progress_points ON guild_war_progress(war_id, capture_points DESC);", transaction);
					this.ExecuteNonQuery(connection, "\n                    CREATE TABLE guild_quests (\n                        id INTEGER PRIMARY KEY AUTOINCREMENT,\n                        recurrence_type TEXT NOT NULL CHECK(recurrence_type IN ('daily', 'weekly', 'monthly', 'seasonal')),\n                        title TEXT NOT NULL,\n                        description TEXT NOT NULL,\n                        requirements TEXT NOT NULL,\n                        rewards TEXT NOT NULL,\n                        starts_at TEXT NOT NULL,\n                        expires_at TEXT NOT NULL,\n                        uses_ingame_time INTEGER NOT NULL DEFAULT 0,\n                        repeat INTEGER NOT NULL DEFAULT 0,\n                        created_at INTEGER NOT NULL\n                    );", transaction);
					this.ExecuteNonQuery(connection, "CREATE INDEX idx_guild_quests_expires_at ON guild_quests(expires_at);", transaction);
					this.ExecuteNonQuery(connection, "CREATE INDEX idx_guild_quests_recurrence ON guild_quests(recurrence_type);", transaction);
					this.ExecuteNonQuery(connection, "CREATE INDEX idx_guild_quests_active ON guild_quests(recurrence_type, expires_at);", transaction);
					this.ExecuteNonQuery(connection, "CREATE INDEX idx_guild_quests_time_mode ON guild_quests(uses_ingame_time);", transaction);
					this.ExecuteNonQuery(connection, "\n                    CREATE TABLE guild_member_quests (\n                        id INTEGER PRIMARY KEY AUTOINCREMENT,\n                        player_uid TEXT NOT NULL,\n                        quest_id INTEGER NOT NULL,\n                        status TEXT NOT NULL DEFAULT 'active' CHECK(status IN ('active', 'completed')),\n                        progress TEXT,\n                        recurrence_type TEXT NOT NULL,\n                        period_key TEXT,\n                        started_at INTEGER NOT NULL,\n                        completed_at INTEGER,\n                        FOREIGN KEY (quest_id) REFERENCES guild_quests(id) ON DELETE CASCADE\n                    );", transaction);
					this.ExecuteNonQuery(connection, "CREATE INDEX idx_member_quests_player ON guild_member_quests(player_uid);", transaction);
					this.ExecuteNonQuery(connection, "CREATE INDEX idx_member_quests_quest ON guild_member_quests(quest_id);", transaction);
					this.ExecuteNonQuery(connection, "CREATE INDEX idx_member_quests_status ON guild_member_quests(player_uid, status);", transaction);
					this.ExecuteNonQuery(connection, "CREATE INDEX idx_member_quests_active ON guild_member_quests(quest_id, status);", transaction);
					this.ExecuteNonQuery(connection, "\n                    CREATE UNIQUE INDEX idx_period_lock\n                    ON guild_member_quests(player_uid, recurrence_type, period_key) \n                    WHERE period_key IS NOT NULL;", transaction);
					transaction.Commit();
					this.serverApi.Logger.Notification("[SRGuildsAndKingdoms] All tables created successfully");
				}
				catch (Exception ex)
				{
					transaction.Rollback();
					this.serverApi.Logger.Error("[SRGuildsAndKingdoms] Failed to create tables: " + ex.Message);
					throw;
				}
			}
		}

		// Token: 0x06000826 RID: 2086 RVA: 0x00039374 File Offset: 0x00037574
		private void ApplyMigrations(SqliteConnection connection, int fromVersion)
		{
			using (SqliteTransaction transaction = connection.BeginTransaction())
			{
				try
				{
					if (fromVersion < 2)
					{
						this.serverApi.Logger.Notification("[SRGuildsAndKingdoms] Applying migration to version 2: Adding repeat column to guild_quests");
						this.ExecuteNonQuery(connection, "ALTER TABLE guild_quests ADD COLUMN repeat INTEGER NOT NULL DEFAULT 0;", transaction);
					}
					if (fromVersion < 3)
					{
						this.serverApi.Logger.Notification("[SRGuildsAndKingdoms] Applying migration to version 3: Adding points_contribution column to guild_members and removing composite PRIMARY KEY from guild_member_quests");
						this.ExecuteNonQuery(connection, "ALTER TABLE guild_members ADD COLUMN points_contribution INTEGER NOT NULL DEFAULT 0;", transaction);
						this.serverApi.Logger.Notification("[SRGuildsAndKingdoms] Calculating points_contribution for existing members...");
						this.ExecuteNonQuery(connection, "\n                        UPDATE guild_members\n                        SET points_contribution = (\n                            SELECT COALESCE(SUM(json_extract(reward.value, '$.amount')), 0)\n                            FROM guild_member_quests gmq\n                            JOIN guild_quests gq ON gmq.quest_id = gq.id\n                            CROSS JOIN json_each(gq.rewards, '$.rewards') AS reward\n                            WHERE gmq.player_uid = guild_members.player_uid\n                              AND gmq.status = 'completed'\n                              AND json_extract(reward.value, '$.code') = 'game:grspoints'\n                        );", transaction);
						this.ExecuteNonQuery(connection, "\n                        CREATE TABLE guild_member_quests_new (\n                            id INTEGER PRIMARY KEY AUTOINCREMENT,\n                            player_uid TEXT NOT NULL,\n                            quest_id INTEGER NOT NULL,\n                            status TEXT NOT NULL DEFAULT 'active' CHECK(status IN ('active', 'completed')),\n                            progress TEXT,\n                            recurrence_type TEXT NOT NULL,\n                            period_key TEXT,\n                            started_at INTEGER NOT NULL,\n                            completed_at INTEGER,\n                            FOREIGN KEY (quest_id) REFERENCES guild_quests(id) ON DELETE CASCADE\n                        );", transaction);
						this.ExecuteNonQuery(connection, "\n                        INSERT INTO guild_member_quests_new (player_uid, quest_id, status, progress, recurrence_type, period_key, started_at, completed_at)\n                        SELECT player_uid, quest_id, status, progress, recurrence_type, period_key, started_at, completed_at\n                        FROM guild_member_quests;", transaction);
						this.ExecuteNonQuery(connection, "DROP TABLE guild_member_quests;", transaction);
						this.ExecuteNonQuery(connection, "ALTER TABLE guild_member_quests_new RENAME TO guild_member_quests;", transaction);
						this.ExecuteNonQuery(connection, "CREATE INDEX idx_member_quests_player ON guild_member_quests(player_uid);", transaction);
						this.ExecuteNonQuery(connection, "CREATE INDEX idx_member_quests_quest ON guild_member_quests(quest_id);", transaction);
						this.ExecuteNonQuery(connection, "CREATE INDEX idx_member_quests_status ON guild_member_quests(player_uid, status);", transaction);
						this.ExecuteNonQuery(connection, "CREATE INDEX idx_member_quests_active ON guild_member_quests(quest_id, status);", transaction);
						this.ExecuteNonQuery(connection, "\n                        CREATE UNIQUE INDEX idx_period_lock\n                        ON guild_member_quests(player_uid, recurrence_type, period_key) \n                        WHERE period_key IS NOT NULL;", transaction);
					}
					if (fromVersion < 4)
					{
						this.serverApi.Logger.Notification("[SRGuildsAndKingdoms] Applying migration to version 4: Adding nodes table");
						this.ExecuteNonQuery(connection, "\n                        CREATE TABLE nodes (\n                            id INTEGER PRIMARY KEY AUTOINCREMENT,\n                            name TEXT NOT NULL UNIQUE,\n                            x INTEGER NOT NULL,\n                            z INTEGER NOT NULL,\n                            radius INTEGER NOT NULL,\n                            created_at INTEGER NOT NULL\n                        );", transaction);
						this.ExecuteNonQuery(connection, "CREATE INDEX idx_nodes_name ON nodes(name);", transaction);
						this.ExecuteNonQuery(connection, "CREATE INDEX idx_nodes_location ON nodes(x, z);", transaction);
					}
					if (fromVersion < 5)
					{
						this.serverApi.Logger.Notification("[SRGuildsAndKingdoms] Applying migration to version 5: Adding capture_zones table");
						this.ExecuteNonQuery(connection, "\n                        CREATE TABLE capture_zones (\n                            id INTEGER PRIMARY KEY AUTOINCREMENT,\n                            node_name TEXT NOT NULL,\n                            zone_id TEXT NOT NULL,\n                            zone_name TEXT NOT NULL,\n                            center_x REAL NOT NULL,\n                            center_y REAL NOT NULL,\n                            center_z REAL NOT NULL,\n                            radius INTEGER NOT NULL,\n                            point_multiplier REAL NOT NULL DEFAULT 1.0,\n                            is_active INTEGER NOT NULL DEFAULT 1,\n                            description TEXT,\n                            created_at INTEGER NOT NULL,\n                            UNIQUE (node_name, zone_id)\n                        );", transaction);
						this.ExecuteNonQuery(connection, "CREATE INDEX idx_capture_zones_node ON capture_zones(node_name);", transaction);
						this.ExecuteNonQuery(connection, "CREATE INDEX idx_capture_zones_active ON capture_zones(node_name, is_active);", transaction);
					}
					if (fromVersion < 6)
					{
						this.serverApi.Logger.Notification("[SRGuildsAndKingdoms] Applying migration to version 6: Adding node_wars tables");
						this.ExecuteNonQuery(connection, "\n                        CREATE TABLE node_wars (\n                            id INTEGER PRIMARY KEY AUTOINCREMENT,\n                            node_id TEXT NOT NULL,\n                            status TEXT NOT NULL CHECK(status IN ('Scheduled', 'Active', 'Completed', 'Cancelled')),\n                            start_time INTEGER NOT NULL,\n                            end_time INTEGER,\n                            signup_deadline INTEGER,\n                            max_guilds INTEGER NOT NULL DEFAULT 0,\n                            capture_points_needed REAL NOT NULL DEFAULT 10000.0,\n                            controlling_guild_uid TEXT,\n                            controlling_guild_name TEXT,\n                            previous_controlling_guild_uid TEXT,\n                            created_at INTEGER NOT NULL,\n                            updated_at INTEGER NOT NULL\n                        );", transaction);
						this.ExecuteNonQuery(connection, "CREATE INDEX idx_node_wars_node_id ON node_wars(node_id);", transaction);
						this.ExecuteNonQuery(connection, "CREATE INDEX idx_node_wars_status ON node_wars(status);", transaction);
						this.ExecuteNonQuery(connection, "CREATE INDEX idx_node_wars_active ON node_wars(node_id, status) WHERE status IN ('Scheduled', 'Active');", transaction);
						this.ExecuteNonQuery(connection, "\n                        CREATE TABLE guild_war_signups (\n                            id INTEGER PRIMARY KEY AUTOINCREMENT,\n                            war_id INTEGER NOT NULL,\n                            guild_uid TEXT NOT NULL,\n                            guild_name TEXT NOT NULL,\n                            signup_by_player_uid TEXT NOT NULL,\n                            signup_time INTEGER NOT NULL,\n                            members_online INTEGER NOT NULL DEFAULT 0,\n                            total_members INTEGER NOT NULL DEFAULT 0,\n                            is_confirmed INTEGER NOT NULL DEFAULT 1,\n                            FOREIGN KEY (war_id) REFERENCES node_wars(id) ON DELETE CASCADE,\n                            UNIQUE (war_id, guild_uid)\n                        );", transaction);
						this.ExecuteNonQuery(connection, "CREATE INDEX idx_guild_war_signups_war ON guild_war_signups(war_id);", transaction);
						this.ExecuteNonQuery(connection, "CREATE INDEX idx_guild_war_signups_guild ON guild_war_signups(guild_uid);", transaction);
						this.ExecuteNonQuery(connection, "\n                        CREATE TABLE guild_war_progress (\n                            id INTEGER PRIMARY KEY AUTOINCREMENT,\n                            war_id INTEGER NOT NULL,\n                            guild_uid TEXT NOT NULL,\n                            guild_name TEXT NOT NULL,\n                            capture_points REAL NOT NULL DEFAULT 0.0,\n                            players_in_zone INTEGER NOT NULL DEFAULT 0,\n                            kills INTEGER NOT NULL DEFAULT 0,\n                            deaths INTEGER NOT NULL DEFAULT 0,\n                            last_update INTEGER NOT NULL,\n                            FOREIGN KEY (war_id) REFERENCES node_wars(id) ON DELETE CASCADE,\n                            UNIQUE (war_id, guild_uid)\n                        );", transaction);
						this.ExecuteNonQuery(connection, "CREATE INDEX idx_guild_war_progress_war ON guild_war_progress(war_id);", transaction);
						this.ExecuteNonQuery(connection, "CREATE INDEX idx_guild_war_progress_guild ON guild_war_progress(guild_uid);", transaction);
						this.ExecuteNonQuery(connection, "CREATE INDEX idx_guild_war_progress_points ON guild_war_progress(war_id, capture_points DESC);", transaction);
					}
					transaction.Commit();
				}
				catch (Exception ex)
				{
					transaction.Rollback();
					this.serverApi.Logger.Error("[SRGuildsAndKingdoms] Failed to apply migrations: " + ex.Message);
					throw;
				}
			}
		}

		// Token: 0x06000827 RID: 2087 RVA: 0x00039608 File Offset: 0x00037808
		private void ExecuteNonQuery(SqliteConnection connection, string sql, [Nullable(2)] SqliteTransaction transaction = null)
		{
			using (SqliteCommand command = new SqliteCommand(sql, connection))
			{
				if (transaction != null)
				{
					command.Transaction = transaction;
				}
				command.ExecuteNonQuery();
			}
		}

		// Token: 0x06000828 RID: 2088 RVA: 0x0003964C File Offset: 0x0003784C
		public bool VerifySchema(SqliteConnection connection)
		{
			bool result;
			try
			{
				foreach (string tableName in new string[]
				{
					"guilds",
					"guild_members",
					"guild_roles",
					"guild_invites",
					"land_claims",
					"guild_tech_progress",
					"guild_tech_contributions",
					"guild_member_tech_progress",
					"guild_cooldowns",
					"zone_whitelists",
					"nodes",
					"capture_zones",
					"guild_quests",
					"guild_member_quests"
				})
				{
					if (!this.TableExists(connection, tableName))
					{
						this.serverApi.Logger.Error("[SRGuildsAndKingdoms] Required table '" + tableName + "' does not exist");
						return false;
					}
				}
				result = true;
			}
			catch (Exception ex)
			{
				this.serverApi.Logger.Error("[SRGuildsAndKingdoms] Schema verification failed: " + ex.Message);
				result = false;
			}
			return result;
		}

		// Token: 0x06000829 RID: 2089 RVA: 0x00039754 File Offset: 0x00037954
		private bool TableExists(SqliteConnection connection, string tableName)
		{
			bool result;
			using (SqliteCommand command = new SqliteCommand("SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name=@tableName;", connection))
			{
				command.Parameters.AddWithValue("@tableName", tableName);
				result = (Convert.ToInt32(command.ExecuteScalar()) > 0);
			}
			return result;
		}

		// Token: 0x04000356 RID: 854
		private readonly ICoreServerAPI serverApi;

		// Token: 0x04000357 RID: 855
		private const int CurrentSchemaVersion = 6;
	}
}
