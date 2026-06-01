using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using Vintagestory.API.Server;

namespace SRGuildsAndKingdoms.src.database
{
    /// <summary>
    /// Repository for managing node data in the database.
    /// Nodes represent strategic locations that can be captured or controlled by guilds.
    /// </summary>
    public class NodeRepository
    {
        private readonly ICoreServerAPI serverApi;
        private readonly GuildDatabase database;

        public NodeRepository(ICoreServerAPI serverApi, GuildDatabase database)
        {
            this.serverApi = serverApi;
            this.database = database;
        }

        /// <summary>
        /// Adds a new node to the database
        /// </summary>
        /// <param name="name">The node's unique name</param>
        /// <param name="x">World X coordinate</param>
        /// <param name="z">World Z coordinate</param>
        /// <param name="radius">Node radius in blocks</param>
        /// <returns>The ID of the created node, or -1 if failed</returns>
        public int AddNode(string name, int x, int z, int radius)
        {
            if (string.IsNullOrWhiteSpace(name) || radius <= 0)
                return -1;

            try
            {
                var connection = database.Connection;

                const string sql = @"
                    INSERT INTO nodes (name, x, z, radius, created_at)
                    VALUES (@name, @x, @z, @radius, @createdAt);
                    SELECT last_insert_rowid();";

                using var command = new SqliteCommand(sql, connection);
                command.Parameters.AddWithValue("@name", name);
                command.Parameters.AddWithValue("@x", x);
                command.Parameters.AddWithValue("@z", z);
                command.Parameters.AddWithValue("@radius", radius);
                command.Parameters.AddWithValue("@createdAt", DateTimeOffset.UtcNow.ToUnixTimeSeconds());

                var result = command.ExecuteScalar();
                int nodeId = Convert.ToInt32(result);

                if (nodeId > 0)
                {
                    serverApi.Logger.Notification($"[SRGuildsAndKingdoms:NodeRepository] Created node '{name}' at ({x}, {z}) with radius {radius} (ID: {nodeId})");
                }

                return nodeId;
            }
            catch (Exception ex)
            {
                serverApi.Logger.Error($"[SRGuildsAndKingdoms:NodeRepository] Failed to add node: {ex.Message}");
                return -1;
            }
        }

        /// <summary>
        /// Removes a node from the database by ID
        /// </summary>
        public bool RemoveNode(int nodeId)
        {
            if (nodeId <= 0)
                return false;

            try
            {
                var connection = database.Connection;

                const string sql = "DELETE FROM nodes WHERE id = @nodeId;";

                using var command = new SqliteCommand(sql, connection);
                command.Parameters.AddWithValue("@nodeId", nodeId);

                int rowsAffected = command.ExecuteNonQuery();

                if (rowsAffected > 0)
                {
                    serverApi.Logger.Notification($"[SRGuildsAndKingdoms:NodeRepository] Deleted node ID {nodeId}");
                }

                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                serverApi.Logger.Error($"[SRGuildsAndKingdoms:NodeRepository] Failed to remove node: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Removes a node from the database by name
        /// </summary>
        public bool RemoveNodeByName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return false;

            try
            {
                var connection = database.Connection;

                const string sql = "DELETE FROM nodes WHERE name = @name;";

                using var command = new SqliteCommand(sql, connection);
                command.Parameters.AddWithValue("@name", name);

                int rowsAffected = command.ExecuteNonQuery();

                if (rowsAffected > 0)
                {
                    serverApi.Logger.Notification($"[SRGuildsAndKingdoms:NodeRepository] Deleted node '{name}'");
                }

                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                serverApi.Logger.Error($"[SRGuildsAndKingdoms:NodeRepository] Failed to remove node by name: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Updates a node's properties
        /// </summary>
        public bool UpdateNode(int nodeId, string? name = null, int? x = null, int? z = null, int? radius = null)
        {
            if (nodeId <= 0)
                return false;

            try
            {
                var connection = database.Connection;
                var updates = new List<string>();
                var command = new SqliteCommand { Connection = connection };

                if (!string.IsNullOrWhiteSpace(name))
                {
                    updates.Add("name = @name");
                    command.Parameters.AddWithValue("@name", name);
                }

                if (x.HasValue)
                {
                    updates.Add("x = @x");
                    command.Parameters.AddWithValue("@x", x.Value);
                }

                if (z.HasValue)
                {
                    updates.Add("z = @z");
                    command.Parameters.AddWithValue("@z", z.Value);
                }

                if (radius.HasValue && radius.Value > 0)
                {
                    updates.Add("radius = @radius");
                    command.Parameters.AddWithValue("@radius", radius.Value);
                }

                if (updates.Count == 0)
                    return false;

                command.CommandText = $"UPDATE nodes SET {string.Join(", ", updates)} WHERE id = @nodeId;";
                command.Parameters.AddWithValue("@nodeId", nodeId);

                int rowsAffected = command.ExecuteNonQuery();

                if (rowsAffected > 0)
                {
                    serverApi.Logger.Debug($"[SRGuildsAndKingdoms:NodeRepository] Updated node ID {nodeId}");
                }

                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                serverApi.Logger.Error($"[SRGuildsAndKingdoms:NodeRepository] Failed to update node: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Gets a node by ID
        /// </summary>
        public NodeData? GetNode(int nodeId)
        {
            if (nodeId <= 0)
                return null;

            try
            {
                var connection = database.Connection;

                const string sql = "SELECT id, name, x, z, radius, created_at FROM nodes WHERE id = @nodeId;";

                using var command = new SqliteCommand(sql, connection);
                command.Parameters.AddWithValue("@nodeId", nodeId);

                using var reader = command.ExecuteReader();

                if (reader.Read())
                {
                    return new NodeData
                    {
                        Id = reader.GetInt32(0),
                        Name = reader.GetString(1),
                        X = reader.GetInt32(2),
                        Z = reader.GetInt32(3),
                        Radius = reader.GetInt32(4),
                        CreatedAt = reader.GetInt64(5)
                    };
                }
            }
            catch (Exception ex)
            {
                serverApi.Logger.Error($"[SRGuildsAndKingdoms:NodeRepository] Failed to get node: {ex.Message}");
            }

            return null;
        }

        /// <summary>
        /// Gets a node by name
        /// </summary>
        public NodeData? GetNodeByName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return null;

            try
            {
                var connection = database.Connection;

                const string sql = "SELECT id, name, x, z, radius, created_at FROM nodes WHERE name = @name;";

                using var command = new SqliteCommand(sql, connection);
                command.Parameters.AddWithValue("@name", name);

                using var reader = command.ExecuteReader();

                if (reader.Read())
                {
                    return new NodeData
                    {
                        Id = reader.GetInt32(0),
                        Name = reader.GetString(1),
                        X = reader.GetInt32(2),
                        Z = reader.GetInt32(3),
                        Radius = reader.GetInt32(4),
                        CreatedAt = reader.GetInt64(5)
                    };
                }
            }
            catch (Exception ex)
            {
                serverApi.Logger.Error($"[SRGuildsAndKingdoms:NodeRepository] Failed to get node by name: {ex.Message}");
            }

            return null;
        }

        /// <summary>
        /// Gets all nodes from the database
        /// </summary>
        public List<NodeData> GetAllNodes()
        {
            var nodes = new List<NodeData>();

            try
            {
                var connection = database.Connection;

                const string sql = "SELECT id, name, x, z, radius, created_at FROM nodes ORDER BY name;";

                using var command = new SqliteCommand(sql, connection);
                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    nodes.Add(new NodeData
                    {
                        Id = reader.GetInt32(0),
                        Name = reader.GetString(1),
                        X = reader.GetInt32(2),
                        Z = reader.GetInt32(3),
                        Radius = reader.GetInt32(4),
                        CreatedAt = reader.GetInt64(5)
                    });
                }

                serverApi.Logger.Debug($"[SRGuildsAndKingdoms:NodeRepository] Loaded {nodes.Count} nodes from database");
            }
            catch (Exception ex)
            {
                serverApi.Logger.Error($"[SRGuildsAndKingdoms:NodeRepository] Failed to get all nodes: {ex.Message}");
            }

            return nodes;
        }

        /// <summary>
        /// Checks if a node with the given name already exists
        /// </summary>
        public bool NodeExists(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return false;

            try
            {
                var connection = database.Connection;

                const string sql = "SELECT COUNT(*) FROM nodes WHERE name = @name;";

                using var command = new SqliteCommand(sql, connection);
                command.Parameters.AddWithValue("@name", name);

                var count = Convert.ToInt32(command.ExecuteScalar());
                return count > 0;
            }
            catch (Exception ex)
            {
                serverApi.Logger.Error($"[SRGuildsAndKingdoms:NodeRepository] Failed to check node existence: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Gets the total number of nodes in the database
        /// </summary>
        public int GetNodeCount()
        {
            try
            {
                var connection = database.Connection;

                const string sql = "SELECT COUNT(*) FROM nodes;";

                using var command = new SqliteCommand(sql, connection);
                return Convert.ToInt32(command.ExecuteScalar());
            }
            catch (Exception ex)
            {
                serverApi.Logger.Error($"[SRGuildsAndKingdoms:NodeRepository] Failed to get node count: {ex.Message}");
                return 0;
            }
        }

        #region Capture Zone Methods

        /// <summary>
        /// Adds a new capture zone to the database
        /// </summary>
        /// <param name="nodeName">The name of the node this zone belongs to</param>
        /// <param name="zoneId">Unique ID for this zone within the node</param>
        /// <param name="zoneName">Display name for the zone</param>
        /// <param name="centerX">World X coordinate</param>
        /// <param name="centerY">World Y coordinate</param>
        /// <param name="centerZ">World Z coordinate</param>
        /// <param name="radius">Zone radius in blocks</param>
        /// <param name="pointMultiplier">Point capture multiplier</param>
        /// <param name="isActive">Whether the zone is active</param>
        /// <param name="description">Optional description</param>
        /// <returns>The ID of the created capture zone, or -1 if failed</returns>
        public int AddCaptureZone(string nodeName, string zoneId, string zoneName, double centerX, double centerY, double centerZ, 
            int radius, double pointMultiplier = 1.0, bool isActive = true, string? description = null)
        {
            if (string.IsNullOrWhiteSpace(nodeName) || string.IsNullOrWhiteSpace(zoneId) || string.IsNullOrWhiteSpace(zoneName) || radius <= 0)
                return -1;

            try
            {
                var connection = database.Connection;

                const string sql = @"
                    INSERT INTO capture_zones (node_name, zone_id, zone_name, center_x, center_y, center_z, radius, point_multiplier, is_active, description, created_at)
                    VALUES (@nodeName, @zoneId, @zoneName, @centerX, @centerY, @centerZ, @radius, @pointMultiplier, @isActive, @description, @createdAt);
                    SELECT last_insert_rowid();";

                using var command = new SqliteCommand(sql, connection);
                command.Parameters.AddWithValue("@nodeName", nodeName);
                command.Parameters.AddWithValue("@zoneId", zoneId);
                command.Parameters.AddWithValue("@zoneName", zoneName);
                command.Parameters.AddWithValue("@centerX", centerX);
                command.Parameters.AddWithValue("@centerY", centerY);
                command.Parameters.AddWithValue("@centerZ", centerZ);
                command.Parameters.AddWithValue("@radius", radius);
                command.Parameters.AddWithValue("@pointMultiplier", pointMultiplier);
                command.Parameters.AddWithValue("@isActive", isActive ? 1 : 0);
                command.Parameters.AddWithValue("@description", (object?)description ?? DBNull.Value);
                command.Parameters.AddWithValue("@createdAt", DateTimeOffset.UtcNow.ToUnixTimeSeconds());

                var result = command.ExecuteScalar();
                int captureZoneId = Convert.ToInt32(result);

                if (captureZoneId > 0)
                {
                    serverApi.Logger.Notification($"[SRGuildsAndKingdoms:NodeRepository] Created capture zone '{zoneName}' ({zoneId}) for node '{nodeName}' (ID: {captureZoneId})");
                }

                return captureZoneId;
            }
            catch (Exception ex)
            {
                serverApi.Logger.Error($"[SRGuildsAndKingdoms:NodeRepository] Failed to add capture zone: {ex.Message}");
                return -1;
            }
        }

        /// <summary>
        /// Removes a capture zone from the database
        /// </summary>
        public bool RemoveCaptureZone(string nodeName, string zoneId)
        {
            if (string.IsNullOrWhiteSpace(nodeName) || string.IsNullOrWhiteSpace(zoneId))
                return false;

            try
            {
                var connection = database.Connection;

                const string sql = "DELETE FROM capture_zones WHERE node_name = @nodeName AND zone_id = @zoneId;";

                using var command = new SqliteCommand(sql, connection);
                command.Parameters.AddWithValue("@nodeName", nodeName);
                command.Parameters.AddWithValue("@zoneId", zoneId);

                int rowsAffected = command.ExecuteNonQuery();

                if (rowsAffected > 0)
                {
                    serverApi.Logger.Notification($"[SRGuildsAndKingdoms:NodeRepository] Deleted capture zone '{zoneId}' from node '{nodeName}'");
                }

                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                serverApi.Logger.Error($"[SRGuildsAndKingdoms:NodeRepository] Failed to remove capture zone: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Gets all capture zones for a specific node
        /// </summary>
        public List<CaptureZoneData> GetCaptureZonesForNode(string nodeName)
        {
            var zones = new List<CaptureZoneData>();

            if (string.IsNullOrWhiteSpace(nodeName))
                return zones;

            try
            {
                var connection = database.Connection;

                const string sql = @"
                    SELECT id, node_name, zone_id, zone_name, center_x, center_y, center_z, radius, point_multiplier, is_active, description, created_at 
                    FROM capture_zones 
                    WHERE node_name = @nodeName 
                    ORDER BY zone_name;";

                using var command = new SqliteCommand(sql, connection);
                command.Parameters.AddWithValue("@nodeName", nodeName);

                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    zones.Add(new CaptureZoneData
                    {
                        Id = reader.GetInt32(0),
                        NodeName = reader.GetString(1),
                        ZoneId = reader.GetString(2),
                        ZoneName = reader.GetString(3),
                        CenterX = reader.GetDouble(4),
                        CenterY = reader.GetDouble(5),
                        CenterZ = reader.GetDouble(6),
                        Radius = reader.GetInt32(7),
                        PointMultiplier = reader.GetDouble(8),
                        IsActive = reader.GetInt32(9) == 1,
                        Description = reader.IsDBNull(10) ? null : reader.GetString(10),
                        CreatedAt = reader.GetInt64(11)
                    });
                }

                serverApi.Logger.Debug($"[SRGuildsAndKingdoms:NodeRepository] Loaded {zones.Count} capture zones for node '{nodeName}'");
            }
            catch (Exception ex)
            {
                serverApi.Logger.Error($"[SRGuildsAndKingdoms:NodeRepository] Failed to get capture zones: {ex.Message}");
            }

            return zones;
        }

        /// <summary>
        /// Gets all capture zones from the database
        /// </summary>
        public List<CaptureZoneData> GetAllCaptureZones()
        {
            var zones = new List<CaptureZoneData>();

            try
            {
                var connection = database.Connection;

                const string sql = @"
                    SELECT id, node_name, zone_id, zone_name, center_x, center_y, center_z, radius, point_multiplier, is_active, description, created_at 
                    FROM capture_zones 
                    ORDER BY node_name, zone_name;";

                using var command = new SqliteCommand(sql, connection);
                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    zones.Add(new CaptureZoneData
                    {
                        Id = reader.GetInt32(0),
                        NodeName = reader.GetString(1),
                        ZoneId = reader.GetString(2),
                        ZoneName = reader.GetString(3),
                        CenterX = reader.GetDouble(4),
                        CenterY = reader.GetDouble(5),
                        CenterZ = reader.GetDouble(6),
                        Radius = reader.GetInt32(7),
                        PointMultiplier = reader.GetDouble(8),
                        IsActive = reader.GetInt32(9) == 1,
                        Description = reader.IsDBNull(10) ? null : reader.GetString(10),
                        CreatedAt = reader.GetInt64(11)
                    });
                }

                serverApi.Logger.Debug($"[SRGuildsAndKingdoms:NodeRepository] Loaded {zones.Count} total capture zones");
            }
            catch (Exception ex)
            {
                serverApi.Logger.Error($"[SRGuildsAndKingdoms:NodeRepository] Failed to get all capture zones: {ex.Message}");
            }

            return zones;
        }

        /// <summary>
        /// Updates a capture zone's properties
        /// </summary>
        public bool UpdateCaptureZone(string nodeName, string zoneId, string? zoneName = null, double? centerX = null, 
            double? centerY = null, double? centerZ = null, int? radius = null, double? pointMultiplier = null, bool? isActive = null, string? description = null)
        {
            if (string.IsNullOrWhiteSpace(nodeName) || string.IsNullOrWhiteSpace(zoneId))
                return false;

            try
            {
                var connection = database.Connection;
                var updates = new List<string>();
                var command = new SqliteCommand { Connection = connection };

                if (!string.IsNullOrWhiteSpace(zoneName))
                {
                    updates.Add("zone_name = @zoneName");
                    command.Parameters.AddWithValue("@zoneName", zoneName);
                }

                if (centerX.HasValue)
                {
                    updates.Add("center_x = @centerX");
                    command.Parameters.AddWithValue("@centerX", centerX.Value);
                }

                if (centerY.HasValue)
                {
                    updates.Add("center_y = @centerY");
                    command.Parameters.AddWithValue("@centerY", centerY.Value);
                }

                if (centerZ.HasValue)
                {
                    updates.Add("center_z = @centerZ");
                    command.Parameters.AddWithValue("@centerZ", centerZ.Value);
                }

                if (radius.HasValue && radius.Value > 0)
                {
                    updates.Add("radius = @radius");
                    command.Parameters.AddWithValue("@radius", radius.Value);
                }

                if (pointMultiplier.HasValue)
                {
                    updates.Add("point_multiplier = @pointMultiplier");
                    command.Parameters.AddWithValue("@pointMultiplier", pointMultiplier.Value);
                }

                if (isActive.HasValue)
                {
                    updates.Add("is_active = @isActive");
                    command.Parameters.AddWithValue("@isActive", isActive.Value ? 1 : 0);
                }

                if (description != null)
                {
                    updates.Add("description = @description");
                    command.Parameters.AddWithValue("@description", (object?)description ?? DBNull.Value);
                }

                if (updates.Count == 0)
                    return false;

                command.CommandText = $"UPDATE capture_zones SET {string.Join(", ", updates)} WHERE node_name = @nodeName AND zone_id = @zoneId;";
                command.Parameters.AddWithValue("@nodeName", nodeName);
                command.Parameters.AddWithValue("@zoneId", zoneId);

                int rowsAffected = command.ExecuteNonQuery();

                if (rowsAffected > 0)
                {
                    serverApi.Logger.Debug($"[SRGuildsAndKingdoms:NodeRepository] Updated capture zone '{zoneId}' in node '{nodeName}'");
                }

                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                serverApi.Logger.Error($"[SRGuildsAndKingdoms:NodeRepository] Failed to update capture zone: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Removes all capture zones for a specific node
        /// </summary>
        public bool RemoveAllCaptureZonesForNode(string nodeName)
        {
            if (string.IsNullOrWhiteSpace(nodeName))
                return false;

            try
            {
                var connection = database.Connection;

                const string sql = "DELETE FROM capture_zones WHERE node_name = @nodeName;";

                using var command = new SqliteCommand(sql, connection);
                command.Parameters.AddWithValue("@nodeName", nodeName);

                int rowsAffected = command.ExecuteNonQuery();

                if (rowsAffected > 0)
                {
                    serverApi.Logger.Notification($"[SRGuildsAndKingdoms:NodeRepository] Deleted {rowsAffected} capture zones from node '{nodeName}'");
                }

                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                serverApi.Logger.Error($"[SRGuildsAndKingdoms:NodeRepository] Failed to remove all capture zones: {ex.Message}");
                return false;
            }
        }

        #endregion

        #region Node War Methods

        /// <summary>
        /// Creates a new node war and returns its ID
        /// </summary>
        public int CreateNodeWar(string nodeId, string status, long startTime, long? endTime, long? signupDeadline,
            int maxGuilds, double capturePointsNeeded, string? controllingGuildUid, string? controllingGuildName, string? previousControllingGuildUid)
        {
            try
            {
                var connection = database.Connection;
                var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

                const string sql = @"
                    INSERT INTO node_wars (node_id, status, start_time, end_time, signup_deadline, max_guilds, 
                        capture_points_needed, controlling_guild_uid, controlling_guild_name, previous_controlling_guild_uid, created_at, updated_at)
                    VALUES (@nodeId, @status, @startTime, @endTime, @signupDeadline, @maxGuilds, 
                        @capturePointsNeeded, @controllingGuildUid, @controllingGuildName, @previousControllingGuildUid, @createdAt, @updatedAt);
                    SELECT last_insert_rowid();";

                using var command = new SqliteCommand(sql, connection);
                command.Parameters.AddWithValue("@nodeId", nodeId);
                command.Parameters.AddWithValue("@status", status);
                command.Parameters.AddWithValue("@startTime", startTime);
                command.Parameters.AddWithValue("@endTime", (object?)endTime ?? DBNull.Value);
                command.Parameters.AddWithValue("@signupDeadline", (object?)signupDeadline ?? DBNull.Value);
                command.Parameters.AddWithValue("@maxGuilds", maxGuilds);
                command.Parameters.AddWithValue("@capturePointsNeeded", capturePointsNeeded);
                command.Parameters.AddWithValue("@controllingGuildUid", (object?)controllingGuildUid ?? DBNull.Value);
                command.Parameters.AddWithValue("@controllingGuildName", (object?)controllingGuildName ?? DBNull.Value);
                command.Parameters.AddWithValue("@previousControllingGuildUid", (object?)previousControllingGuildUid ?? DBNull.Value);
                command.Parameters.AddWithValue("@createdAt", now);
                command.Parameters.AddWithValue("@updatedAt", now);

                var result = command.ExecuteScalar();
                int warId = Convert.ToInt32(result);

                if (warId > 0)
                {
                    serverApi.Logger.Notification($"[NodeRepository] Created node war for '{nodeId}' with status '{status}' (ID: {warId})");
                }

                return warId;
            }
            catch (Exception ex)
            {
                serverApi.Logger.Error($"[NodeRepository] Failed to create node war: {ex.Message}");
                return -1;
            }
        }

        /// <summary>
        /// Updates an existing node war
        /// </summary>
        public bool UpdateNodeWar(int warId, string? status = null, long? startTime = null, long? endTime = null,
            string? controllingGuildUid = null, string? controllingGuildName = null)
        {
            if (warId <= 0) return false;

            try
            {
                var connection = database.Connection;
                var updates = new List<string>();
                var command = new SqliteCommand { Connection = connection };

                if (status != null)
                {
                    updates.Add("status = @status");
                    command.Parameters.AddWithValue("@status", status);
                }

                if (startTime.HasValue)
                {
                    updates.Add("start_time = @startTime");
                    command.Parameters.AddWithValue("@startTime", startTime.Value);
                }

                if (endTime.HasValue)
                {
                    updates.Add("end_time = @endTime");
                    command.Parameters.AddWithValue("@endTime", endTime.Value);
                }

                if (controllingGuildUid != null)
                {
                    updates.Add("controlling_guild_uid = @controllingGuildUid");
                    command.Parameters.AddWithValue("@controllingGuildUid", (object?)controllingGuildUid ?? DBNull.Value);
                }

                if (controllingGuildName != null)
                {
                    updates.Add("controlling_guild_name = @controllingGuildName");
                    command.Parameters.AddWithValue("@controllingGuildName", (object?)controllingGuildName ?? DBNull.Value);
                }

                if (updates.Count == 0) return false;

                updates.Add("updated_at = @updatedAt");
                command.Parameters.AddWithValue("@updatedAt", DateTimeOffset.UtcNow.ToUnixTimeSeconds());

                command.CommandText = $"UPDATE node_wars SET {string.Join(", ", updates)} WHERE id = @warId;";
                command.Parameters.AddWithValue("@warId", warId);

                int rowsAffected = command.ExecuteNonQuery();

                if (rowsAffected > 0)
                {
                    serverApi.Logger.Debug($"[NodeRepository] Updated node war {warId}");
                }

                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                serverApi.Logger.Error($"[NodeRepository] Failed to update node war: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Gets the active or scheduled war for a specific node
        /// </summary>
        public NodeWarData? GetActiveWarForNode(string nodeId)
        {
            try
            {
                var connection = database.Connection;

                const string sql = @"
                    SELECT id, node_id, status, start_time, end_time, signup_deadline, max_guilds, 
                        capture_points_needed, controlling_guild_uid, controlling_guild_name, previous_controlling_guild_uid, created_at, updated_at
                    FROM node_wars 
                    WHERE node_id = @nodeId AND status IN ('Scheduled', 'Active')
                    ORDER BY created_at DESC 
                    LIMIT 1;";

                using var command = new SqliteCommand(sql, connection);
                command.Parameters.AddWithValue("@nodeId", nodeId);

                using var reader = command.ExecuteReader();

                if (reader.Read())
                {
                    return ReadNodeWarData(reader);
                }
            }
            catch (Exception ex)
            {
                serverApi.Logger.Error($"[NodeRepository] Failed to get active war for node: {ex.Message}");
            }

            return null;
        }

        /// <summary>
        /// Gets all active wars
        /// </summary>
        public List<NodeWarData> GetAllActiveWars()
        {
            var wars = new List<NodeWarData>();

            try
            {
                var connection = database.Connection;

                const string sql = @"
                    SELECT id, node_id, status, start_time, end_time, signup_deadline, max_guilds, 
                        capture_points_needed, controlling_guild_uid, controlling_guild_name, previous_controlling_guild_uid, created_at, updated_at
                    FROM node_wars 
                    WHERE status IN ('Scheduled', 'Active')
                    ORDER BY start_time;";

                using var command = new SqliteCommand(sql, connection);
                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    wars.Add(ReadNodeWarData(reader));
                }
            }
            catch (Exception ex)
            {
                serverApi.Logger.Error($"[NodeRepository] Failed to get all active wars: {ex.Message}");
            }

            return wars;
        }

        /// <summary>
        /// Adds a guild signup for a node war
        /// </summary>
        public int AddGuildSignup(int warId, string guildUid, string guildName, string signupByPlayerUid,
            long signupTime, int membersOnline, int totalMembers, bool isConfirmed)
        {
            try
            {
                var connection = database.Connection;

                const string sql = @"
                    INSERT INTO guild_war_signups (war_id, guild_uid, guild_name, signup_by_player_uid, 
                        signup_time, members_online, total_members, is_confirmed)
                    VALUES (@warId, @guildUid, @guildName, @signupByPlayerUid, 
                        @signupTime, @membersOnline, @totalMembers, @isConfirmed);
                    SELECT last_insert_rowid();";

                using var command = new SqliteCommand(sql, connection);
                command.Parameters.AddWithValue("@warId", warId);
                command.Parameters.AddWithValue("@guildUid", guildUid);
                command.Parameters.AddWithValue("@guildName", guildName);
                command.Parameters.AddWithValue("@signupByPlayerUid", signupByPlayerUid);
                command.Parameters.AddWithValue("@signupTime", signupTime);
                command.Parameters.AddWithValue("@membersOnline", membersOnline);
                command.Parameters.AddWithValue("@totalMembers", totalMembers);
                command.Parameters.AddWithValue("@isConfirmed", isConfirmed ? 1 : 0);

                var result = command.ExecuteScalar();
                return Convert.ToInt32(result);
            }
            catch (Exception ex)
            {
                serverApi.Logger.Error($"[NodeRepository] Failed to add guild signup: {ex.Message}");
                return -1;
            }
        }

        /// <summary>
        /// Removes a guild signup
        /// </summary>
        public bool RemoveGuildSignup(int warId, string guildUid)
        {
            try
            {
                var connection = database.Connection;

                const string sql = "DELETE FROM guild_war_signups WHERE war_id = @warId AND guild_uid = @guildUid;";

                using var command = new SqliteCommand(sql, connection);
                command.Parameters.AddWithValue("@warId", warId);
                command.Parameters.AddWithValue("@guildUid", guildUid);

                int rowsAffected = command.ExecuteNonQuery();
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                serverApi.Logger.Error($"[NodeRepository] Failed to remove guild signup: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Saves or updates guild war progress
        /// </summary>
        public bool SaveGuildWarProgress(int warId, string guildUid, string guildName, double capturePoints,
            int playersInZone, int kills, int deaths)
        {
            try
            {
                var connection = database.Connection;
                var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

                const string sql = @"
                    INSERT INTO guild_war_progress (war_id, guild_uid, guild_name, capture_points, 
                        players_in_zone, kills, deaths, last_update)
                    VALUES (@warId, @guildUid, @guildName, @capturePoints, @playersInZone, @kills, @deaths, @lastUpdate)
                    ON CONFLICT(war_id, guild_uid) DO UPDATE SET
                        capture_points = @capturePoints,
                        players_in_zone = @playersInZone,
                        kills = @kills,
                        deaths = @deaths,
                        last_update = @lastUpdate;";

                using var command = new SqliteCommand(sql, connection);
                command.Parameters.AddWithValue("@warId", warId);
                command.Parameters.AddWithValue("@guildUid", guildUid);
                command.Parameters.AddWithValue("@guildName", guildName);
                command.Parameters.AddWithValue("@capturePoints", capturePoints);
                command.Parameters.AddWithValue("@playersInZone", playersInZone);
                command.Parameters.AddWithValue("@kills", kills);
                command.Parameters.AddWithValue("@deaths", deaths);
                command.Parameters.AddWithValue("@lastUpdate", now);

                command.ExecuteNonQuery();
                return true;
            }
            catch (Exception ex)
            {
                serverApi.Logger.Error($"[NodeRepository] Failed to save guild war progress: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Gets guild progress for a specific war
        /// </summary>
        public List<GuildWarProgressData> GetGuildProgressForWar(int warId)
        {
            var progress = new List<GuildWarProgressData>();

            try
            {
                var connection = database.Connection;

                const string sql = @"
                    SELECT id, war_id, guild_uid, guild_name, capture_points, players_in_zone, kills, deaths, last_update
                    FROM guild_war_progress 
                    WHERE war_id = @warId
                    ORDER BY capture_points DESC;";

                using var command = new SqliteCommand(sql, connection);
                command.Parameters.AddWithValue("@warId", warId);

                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    progress.Add(new GuildWarProgressData
                    {
                        Id = reader.GetInt32(0),
                        WarId = reader.GetInt32(1),
                        GuildUid = reader.GetString(2),
                        GuildName = reader.GetString(3),
                        CapturePoints = reader.GetDouble(4),
                        PlayersInZone = reader.GetInt32(5),
                        Kills = reader.GetInt32(6),
                        Deaths = reader.GetInt32(7),
                        LastUpdate = reader.GetInt64(8)
                    });
                }
            }
            catch (Exception ex)
            {
                serverApi.Logger.Error($"[NodeRepository] Failed to get guild progress for war: {ex.Message}");
            }

            return progress;
        }

        /// <summary>
        /// Gets guild signups for a specific war
        /// </summary>
        public List<GuildSignupData> GetGuildSignupsForWar(int warId)
        {
            var signups = new List<GuildSignupData>();

            try
            {
                var connection = database.Connection;

                const string sql = @"
                    SELECT id, war_id, guild_uid, guild_name, signup_by_player_uid, signup_time, 
                           members_online, total_members, is_confirmed
                    FROM guild_war_signups 
                    WHERE war_id = @warId
                    ORDER BY signup_time;";

                using var command = new SqliteCommand(sql, connection);
                command.Parameters.AddWithValue("@warId", warId);

                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    signups.Add(new GuildSignupData
                    {
                        Id = reader.GetInt32(0),
                        WarId = reader.GetInt32(1),
                        GuildUid = reader.GetString(2),
                        GuildName = reader.GetString(3),
                        SignupByPlayerUid = reader.GetString(4),
                        SignupTime = reader.GetInt64(5),
                        MembersOnline = reader.GetInt32(6),
                        TotalMembers = reader.GetInt32(7),
                        IsConfirmed = reader.GetInt32(8) == 1
                    });
                }
            }
            catch (Exception ex)
            {
                serverApi.Logger.Error($"[NodeRepository] Failed to get guild signups for war: {ex.Message}");
            }

            return signups;
        }

        private NodeWarData ReadNodeWarData(SqliteDataReader reader)
        {
            return new NodeWarData
            {
                Id = reader.GetInt32(0),
                NodeId = reader.GetString(1),
                Status = reader.GetString(2),
                StartTime = reader.GetInt64(3),
                EndTime = reader.IsDBNull(4) ? null : reader.GetInt64(4),
                SignupDeadline = reader.IsDBNull(5) ? null : reader.GetInt64(5),
                MaxGuilds = reader.GetInt32(6),
                CapturePointsNeeded = reader.GetDouble(7),
                ControllingGuildUid = reader.IsDBNull(8) ? null : reader.GetString(8),
                ControllingGuildName = reader.IsDBNull(9) ? null : reader.GetString(9),
                PreviousControllingGuildUid = reader.IsDBNull(10) ? null : reader.GetString(10),
                CreatedAt = reader.GetInt64(11),
                UpdatedAt = reader.GetInt64(12)
            };
        }

        #endregion
    }

    /// <summary>
    /// Data transfer object for node information
    /// </summary>
    public class NodeData
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int X { get; set; }
        public int Z { get; set; }
        public int Radius { get; set; }
        public long CreatedAt { get; set; }
    }

    /// <summary>
    /// Data transfer object for capture zone information
    /// </summary>
    public class CaptureZoneData
    {
        public int Id { get; set; }
        public string NodeName { get; set; } = string.Empty;
        public string ZoneId { get; set; } = string.Empty;
        public string ZoneName { get; set; } = string.Empty;
        public double CenterX { get; set; }
        public double CenterY { get; set; }
        public double CenterZ { get; set; }
        public int Radius { get; set; }
        public double PointMultiplier { get; set; } = 1.0;
        public bool IsActive { get; set; } = true;
        public string? Description { get; set; }
        public long CreatedAt { get; set; }
    }

    /// <summary>
    /// Data transfer object for node war information
    /// </summary>
    public class NodeWarData
    {
        public int Id { get; set; }
        public string NodeId { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public long StartTime { get; set; }
        public long? EndTime { get; set; }
        public long? SignupDeadline { get; set; }
        public int MaxGuilds { get; set; }
        public double CapturePointsNeeded { get; set; }
        public string? ControllingGuildUid { get; set; }
        public string? ControllingGuildName { get; set; }
        public string? PreviousControllingGuildUid { get; set; }
        public long CreatedAt { get; set; }
        public long UpdatedAt { get; set; }
    }

    /// <summary>
    /// Data transfer object for guild war progress
    /// </summary>
    public class GuildWarProgressData
    {
        public int Id { get; set; }
        public int WarId { get; set; }
        public string GuildUid { get; set; } = string.Empty;
        public string GuildName { get; set; } = string.Empty;
        public double CapturePoints { get; set; }
        public int PlayersInZone { get; set; }
        public int Kills { get; set; }
        public int Deaths { get; set; }
        public long LastUpdate { get; set; }
    }

    /// <summary>
    /// Data transfer object for guild war signup
    /// </summary>
    public class GuildSignupData
    {
        public int Id { get; set; }
        public int WarId { get; set; }
        public string GuildUid { get; set; } = string.Empty;
        public string GuildName { get; set; } = string.Empty;
        public string SignupByPlayerUid { get; set; } = string.Empty;
        public long SignupTime { get; set; }
        public int MembersOnline { get; set; }
        public int TotalMembers { get; set; }
        public bool IsConfirmed { get; set; }
    }
}
