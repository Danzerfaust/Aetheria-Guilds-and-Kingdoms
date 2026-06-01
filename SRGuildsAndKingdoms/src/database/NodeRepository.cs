using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Microsoft.Data.Sqlite;
using Vintagestory.API.Common;
using Vintagestory.API.Server;

namespace SRGuildsAndKingdoms.src.database
{
	// Token: 0x020000B8 RID: 184
	[NullableContext(1)]
	[Nullable(0)]
	public class NodeRepository
	{
		// Token: 0x0600087F RID: 2175 RVA: 0x0003E519 File Offset: 0x0003C719
		public NodeRepository(ICoreServerAPI serverApi, GuildDatabase database)
		{
			this.serverApi = serverApi;
			this.database = database;
		}

		// Token: 0x06000880 RID: 2176 RVA: 0x0003E530 File Offset: 0x0003C730
		public int AddNode(string name, int x, int z, int radius)
		{
			if (string.IsNullOrWhiteSpace(name) || radius <= 0)
			{
				return -1;
			}
			int result;
			try
			{
				SqliteConnection connection = this.database.Connection;
				using (SqliteCommand command = new SqliteCommand("\n                    INSERT INTO nodes (name, x, z, radius, created_at)\n                    VALUES (@name, @x, @z, @radius, @createdAt);\n                    SELECT last_insert_rowid();", connection))
				{
					command.Parameters.AddWithValue("@name", name);
					command.Parameters.AddWithValue("@x", x);
					command.Parameters.AddWithValue("@z", z);
					command.Parameters.AddWithValue("@radius", radius);
					command.Parameters.AddWithValue("@createdAt", DateTimeOffset.UtcNow.ToUnixTimeSeconds());
					int nodeId = Convert.ToInt32(command.ExecuteScalar());
					if (nodeId > 0)
					{
						ILogger logger = this.serverApi.Logger;
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(80, 5);
						defaultInterpolatedStringHandler.AppendLiteral("[SRGuildsAndKingdoms:NodeRepository] Created node '");
						defaultInterpolatedStringHandler.AppendFormatted(name);
						defaultInterpolatedStringHandler.AppendLiteral("' at (");
						defaultInterpolatedStringHandler.AppendFormatted<int>(x);
						defaultInterpolatedStringHandler.AppendLiteral(", ");
						defaultInterpolatedStringHandler.AppendFormatted<int>(z);
						defaultInterpolatedStringHandler.AppendLiteral(") with radius ");
						defaultInterpolatedStringHandler.AppendFormatted<int>(radius);
						defaultInterpolatedStringHandler.AppendLiteral(" (ID: ");
						defaultInterpolatedStringHandler.AppendFormatted<int>(nodeId);
						defaultInterpolatedStringHandler.AppendLiteral(")");
						logger.Notification(defaultInterpolatedStringHandler.ToStringAndClear());
					}
					result = nodeId;
				}
			}
			catch (Exception ex)
			{
				this.serverApi.Logger.Error("[SRGuildsAndKingdoms:NodeRepository] Failed to add node: " + ex.Message);
				result = -1;
			}
			return result;
		}

		// Token: 0x06000881 RID: 2177 RVA: 0x0003E6F4 File Offset: 0x0003C8F4
		public bool RemoveNode(int nodeId)
		{
			if (nodeId <= 0)
			{
				return false;
			}
			bool result;
			try
			{
				SqliteConnection connection = this.database.Connection;
				using (SqliteCommand command = new SqliteCommand("DELETE FROM nodes WHERE id = @nodeId;", connection))
				{
					command.Parameters.AddWithValue("@nodeId", nodeId);
					int num = command.ExecuteNonQuery();
					if (num > 0)
					{
						ILogger logger = this.serverApi.Logger;
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(53, 1);
						defaultInterpolatedStringHandler.AppendLiteral("[SRGuildsAndKingdoms:NodeRepository] Deleted node ID ");
						defaultInterpolatedStringHandler.AppendFormatted<int>(nodeId);
						logger.Notification(defaultInterpolatedStringHandler.ToStringAndClear());
					}
					result = (num > 0);
				}
			}
			catch (Exception ex)
			{
				this.serverApi.Logger.Error("[SRGuildsAndKingdoms:NodeRepository] Failed to remove node: " + ex.Message);
				result = false;
			}
			return result;
		}

		// Token: 0x06000882 RID: 2178 RVA: 0x0003E7CC File Offset: 0x0003C9CC
		public bool RemoveNodeByName(string name)
		{
			if (string.IsNullOrWhiteSpace(name))
			{
				return false;
			}
			bool result;
			try
			{
				SqliteConnection connection = this.database.Connection;
				using (SqliteCommand command = new SqliteCommand("DELETE FROM nodes WHERE name = @name;", connection))
				{
					command.Parameters.AddWithValue("@name", name);
					int num = command.ExecuteNonQuery();
					if (num > 0)
					{
						this.serverApi.Logger.Notification("[SRGuildsAndKingdoms:NodeRepository] Deleted node '" + name + "'");
					}
					result = (num > 0);
				}
			}
			catch (Exception ex)
			{
				this.serverApi.Logger.Error("[SRGuildsAndKingdoms:NodeRepository] Failed to remove node by name: " + ex.Message);
				result = false;
			}
			return result;
		}

		// Token: 0x06000883 RID: 2179 RVA: 0x0003E88C File Offset: 0x0003CA8C
		[NullableContext(2)]
		public bool UpdateNode(int nodeId, string name = null, int? x = null, int? z = null, int? radius = null)
		{
			if (nodeId <= 0)
			{
				return false;
			}
			bool result;
			try
			{
				SqliteConnection connection = this.database.Connection;
				List<string> updates = new List<string>();
				SqliteCommand command = new SqliteCommand
				{
					Connection = connection
				};
				if (!string.IsNullOrWhiteSpace(name))
				{
					updates.Add("name = @name");
					command.Parameters.AddWithValue("@name", name);
				}
				if (x != null)
				{
					updates.Add("x = @x");
					command.Parameters.AddWithValue("@x", x.Value);
				}
				if (z != null)
				{
					updates.Add("z = @z");
					command.Parameters.AddWithValue("@z", z.Value);
				}
				if (radius != null && radius.Value > 0)
				{
					updates.Add("radius = @radius");
					command.Parameters.AddWithValue("@radius", radius.Value);
				}
				if (updates.Count == 0)
				{
					result = false;
				}
				else
				{
					command.CommandText = "UPDATE nodes SET " + string.Join(", ", updates) + " WHERE id = @nodeId;";
					command.Parameters.AddWithValue("@nodeId", nodeId);
					int num = command.ExecuteNonQuery();
					if (num > 0)
					{
						ILogger logger = this.serverApi.Logger;
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(53, 1);
						defaultInterpolatedStringHandler.AppendLiteral("[SRGuildsAndKingdoms:NodeRepository] Updated node ID ");
						defaultInterpolatedStringHandler.AppendFormatted<int>(nodeId);
						logger.Debug(defaultInterpolatedStringHandler.ToStringAndClear());
					}
					result = (num > 0);
				}
			}
			catch (Exception ex)
			{
				this.serverApi.Logger.Error("[SRGuildsAndKingdoms:NodeRepository] Failed to update node: " + ex.Message);
				result = false;
			}
			return result;
		}

		// Token: 0x06000884 RID: 2180 RVA: 0x0003EA50 File Offset: 0x0003CC50
		[NullableContext(2)]
		public NodeData GetNode(int nodeId)
		{
			if (nodeId <= 0)
			{
				return null;
			}
			try
			{
				SqliteConnection connection = this.database.Connection;
				using (SqliteCommand command = new SqliteCommand("SELECT id, name, x, z, radius, created_at FROM nodes WHERE id = @nodeId;", connection))
				{
					command.Parameters.AddWithValue("@nodeId", nodeId);
					using (SqliteDataReader reader = command.ExecuteReader())
					{
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
				}
			}
			catch (Exception ex)
			{
				this.serverApi.Logger.Error("[SRGuildsAndKingdoms:NodeRepository] Failed to get node: " + ex.Message);
			}
			return null;
		}

		// Token: 0x06000885 RID: 2181 RVA: 0x0003EB64 File Offset: 0x0003CD64
		[return: Nullable(2)]
		public NodeData GetNodeByName(string name)
		{
			if (string.IsNullOrWhiteSpace(name))
			{
				return null;
			}
			try
			{
				SqliteConnection connection = this.database.Connection;
				using (SqliteCommand command = new SqliteCommand("SELECT id, name, x, z, radius, created_at FROM nodes WHERE name = @name;", connection))
				{
					command.Parameters.AddWithValue("@name", name);
					using (SqliteDataReader reader = command.ExecuteReader())
					{
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
				}
			}
			catch (Exception ex)
			{
				this.serverApi.Logger.Error("[SRGuildsAndKingdoms:NodeRepository] Failed to get node by name: " + ex.Message);
			}
			return null;
		}

		// Token: 0x06000886 RID: 2182 RVA: 0x0003EC78 File Offset: 0x0003CE78
		public List<NodeData> GetAllNodes()
		{
			List<NodeData> nodes = new List<NodeData>();
			try
			{
				SqliteConnection connection = this.database.Connection;
				using (SqliteCommand command = new SqliteCommand("SELECT id, name, x, z, radius, created_at FROM nodes ORDER BY name;", connection))
				{
					using (SqliteDataReader reader = command.ExecuteReader())
					{
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
						ILogger logger = this.serverApi.Logger;
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(64, 1);
						defaultInterpolatedStringHandler.AppendLiteral("[SRGuildsAndKingdoms:NodeRepository] Loaded ");
						defaultInterpolatedStringHandler.AppendFormatted<int>(nodes.Count);
						defaultInterpolatedStringHandler.AppendLiteral(" nodes from database");
						logger.Debug(defaultInterpolatedStringHandler.ToStringAndClear());
					}
				}
			}
			catch (Exception ex)
			{
				this.serverApi.Logger.Error("[SRGuildsAndKingdoms:NodeRepository] Failed to get all nodes: " + ex.Message);
			}
			return nodes;
		}

		// Token: 0x06000887 RID: 2183 RVA: 0x0003EDBC File Offset: 0x0003CFBC
		public bool NodeExists(string name)
		{
			if (string.IsNullOrWhiteSpace(name))
			{
				return false;
			}
			bool result;
			try
			{
				SqliteConnection connection = this.database.Connection;
				using (SqliteCommand command = new SqliteCommand("SELECT COUNT(*) FROM nodes WHERE name = @name;", connection))
				{
					command.Parameters.AddWithValue("@name", name);
					result = (Convert.ToInt32(command.ExecuteScalar()) > 0);
				}
			}
			catch (Exception ex)
			{
				this.serverApi.Logger.Error("[SRGuildsAndKingdoms:NodeRepository] Failed to check node existence: " + ex.Message);
				result = false;
			}
			return result;
		}

		// Token: 0x06000888 RID: 2184 RVA: 0x0003EE5C File Offset: 0x0003D05C
		public int GetNodeCount()
		{
			int result;
			try
			{
				SqliteConnection connection = this.database.Connection;
				using (SqliteCommand command = new SqliteCommand("SELECT COUNT(*) FROM nodes;", connection))
				{
					result = Convert.ToInt32(command.ExecuteScalar());
				}
			}
			catch (Exception ex)
			{
				this.serverApi.Logger.Error("[SRGuildsAndKingdoms:NodeRepository] Failed to get node count: " + ex.Message);
				result = 0;
			}
			return result;
		}

		// Token: 0x06000889 RID: 2185 RVA: 0x0003EEDC File Offset: 0x0003D0DC
		public int AddCaptureZone(string nodeName, string zoneId, string zoneName, double centerX, double centerY, double centerZ, int radius, double pointMultiplier = 1.0, bool isActive = true, [Nullable(2)] string description = null)
		{
			if (string.IsNullOrWhiteSpace(nodeName) || string.IsNullOrWhiteSpace(zoneId) || string.IsNullOrWhiteSpace(zoneName) || radius <= 0)
			{
				return -1;
			}
			int result;
			try
			{
				SqliteConnection connection = this.database.Connection;
				using (SqliteCommand command = new SqliteCommand("\n                    INSERT INTO capture_zones (node_name, zone_id, zone_name, center_x, center_y, center_z, radius, point_multiplier, is_active, description, created_at)\n                    VALUES (@nodeName, @zoneId, @zoneName, @centerX, @centerY, @centerZ, @radius, @pointMultiplier, @isActive, @description, @createdAt);\n                    SELECT last_insert_rowid();", connection))
				{
					command.Parameters.AddWithValue("@nodeName", nodeName);
					command.Parameters.AddWithValue("@zoneId", zoneId);
					command.Parameters.AddWithValue("@zoneName", zoneName);
					command.Parameters.AddWithValue("@centerX", centerX);
					command.Parameters.AddWithValue("@centerY", centerY);
					command.Parameters.AddWithValue("@centerZ", centerZ);
					command.Parameters.AddWithValue("@radius", radius);
					command.Parameters.AddWithValue("@pointMultiplier", pointMultiplier);
					command.Parameters.AddWithValue("@isActive", (isActive > false) ? 1 : 0);
					command.Parameters.AddWithValue("@description", description ?? DBNull.Value);
					command.Parameters.AddWithValue("@createdAt", DateTimeOffset.UtcNow.ToUnixTimeSeconds());
					int captureZoneId = Convert.ToInt32(command.ExecuteScalar());
					if (captureZoneId > 0)
					{
						ILogger logger = this.serverApi.Logger;
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(82, 4);
						defaultInterpolatedStringHandler.AppendLiteral("[SRGuildsAndKingdoms:NodeRepository] Created capture zone '");
						defaultInterpolatedStringHandler.AppendFormatted(zoneName);
						defaultInterpolatedStringHandler.AppendLiteral("' (");
						defaultInterpolatedStringHandler.AppendFormatted(zoneId);
						defaultInterpolatedStringHandler.AppendLiteral(") for node '");
						defaultInterpolatedStringHandler.AppendFormatted(nodeName);
						defaultInterpolatedStringHandler.AppendLiteral("' (ID: ");
						defaultInterpolatedStringHandler.AppendFormatted<int>(captureZoneId);
						defaultInterpolatedStringHandler.AppendLiteral(")");
						logger.Notification(defaultInterpolatedStringHandler.ToStringAndClear());
					}
					result = captureZoneId;
				}
			}
			catch (Exception ex)
			{
				this.serverApi.Logger.Error("[SRGuildsAndKingdoms:NodeRepository] Failed to add capture zone: " + ex.Message);
				result = -1;
			}
			return result;
		}

		// Token: 0x0600088A RID: 2186 RVA: 0x0003F124 File Offset: 0x0003D324
		public bool RemoveCaptureZone(string nodeName, string zoneId)
		{
			if (string.IsNullOrWhiteSpace(nodeName) || string.IsNullOrWhiteSpace(zoneId))
			{
				return false;
			}
			bool result;
			try
			{
				SqliteConnection connection = this.database.Connection;
				using (SqliteCommand command = new SqliteCommand("DELETE FROM capture_zones WHERE node_name = @nodeName AND zone_id = @zoneId;", connection))
				{
					command.Parameters.AddWithValue("@nodeName", nodeName);
					command.Parameters.AddWithValue("@zoneId", zoneId);
					int num = command.ExecuteNonQuery();
					if (num > 0)
					{
						ILogger logger = this.serverApi.Logger;
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(73, 2);
						defaultInterpolatedStringHandler.AppendLiteral("[SRGuildsAndKingdoms:NodeRepository] Deleted capture zone '");
						defaultInterpolatedStringHandler.AppendFormatted(zoneId);
						defaultInterpolatedStringHandler.AppendLiteral("' from node '");
						defaultInterpolatedStringHandler.AppendFormatted(nodeName);
						defaultInterpolatedStringHandler.AppendLiteral("'");
						logger.Notification(defaultInterpolatedStringHandler.ToStringAndClear());
					}
					result = (num > 0);
				}
			}
			catch (Exception ex)
			{
				this.serverApi.Logger.Error("[SRGuildsAndKingdoms:NodeRepository] Failed to remove capture zone: " + ex.Message);
				result = false;
			}
			return result;
		}

		// Token: 0x0600088B RID: 2187 RVA: 0x0003F234 File Offset: 0x0003D434
		public List<CaptureZoneData> GetCaptureZonesForNode(string nodeName)
		{
			List<CaptureZoneData> zones = new List<CaptureZoneData>();
			if (string.IsNullOrWhiteSpace(nodeName))
			{
				return zones;
			}
			try
			{
				SqliteConnection connection = this.database.Connection;
				using (SqliteCommand command = new SqliteCommand("\n                    SELECT id, node_name, zone_id, zone_name, center_x, center_y, center_z, radius, point_multiplier, is_active, description, created_at \n                    FROM capture_zones \n                    WHERE node_name = @nodeName \n                    ORDER BY zone_name;", connection))
				{
					command.Parameters.AddWithValue("@nodeName", nodeName);
					using (SqliteDataReader reader = command.ExecuteReader())
					{
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
								IsActive = (reader.GetInt32(9) == 1),
								Description = (reader.IsDBNull(10) ? null : reader.GetString(10)),
								CreatedAt = reader.GetInt64(11)
							});
						}
						ILogger logger = this.serverApi.Logger;
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(70, 2);
						defaultInterpolatedStringHandler.AppendLiteral("[SRGuildsAndKingdoms:NodeRepository] Loaded ");
						defaultInterpolatedStringHandler.AppendFormatted<int>(zones.Count);
						defaultInterpolatedStringHandler.AppendLiteral(" capture zones for node '");
						defaultInterpolatedStringHandler.AppendFormatted(nodeName);
						defaultInterpolatedStringHandler.AppendLiteral("'");
						logger.Debug(defaultInterpolatedStringHandler.ToStringAndClear());
					}
				}
			}
			catch (Exception ex)
			{
				this.serverApi.Logger.Error("[SRGuildsAndKingdoms:NodeRepository] Failed to get capture zones: " + ex.Message);
			}
			return zones;
		}

		// Token: 0x0600088C RID: 2188 RVA: 0x0003F434 File Offset: 0x0003D634
		public List<CaptureZoneData> GetAllCaptureZones()
		{
			List<CaptureZoneData> zones = new List<CaptureZoneData>();
			try
			{
				SqliteConnection connection = this.database.Connection;
				using (SqliteCommand command = new SqliteCommand("\n                    SELECT id, node_name, zone_id, zone_name, center_x, center_y, center_z, radius, point_multiplier, is_active, description, created_at \n                    FROM capture_zones \n                    ORDER BY node_name, zone_name;", connection))
				{
					using (SqliteDataReader reader = command.ExecuteReader())
					{
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
								IsActive = (reader.GetInt32(9) == 1),
								Description = (reader.IsDBNull(10) ? null : reader.GetString(10)),
								CreatedAt = reader.GetInt64(11)
							});
						}
						ILogger logger = this.serverApi.Logger;
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(64, 1);
						defaultInterpolatedStringHandler.AppendLiteral("[SRGuildsAndKingdoms:NodeRepository] Loaded ");
						defaultInterpolatedStringHandler.AppendFormatted<int>(zones.Count);
						defaultInterpolatedStringHandler.AppendLiteral(" total capture zones");
						logger.Debug(defaultInterpolatedStringHandler.ToStringAndClear());
					}
				}
			}
			catch (Exception ex)
			{
				this.serverApi.Logger.Error("[SRGuildsAndKingdoms:NodeRepository] Failed to get all capture zones: " + ex.Message);
			}
			return zones;
		}

		// Token: 0x0600088D RID: 2189 RVA: 0x0003F604 File Offset: 0x0003D804
		public bool UpdateCaptureZone(string nodeName, string zoneId, [Nullable(2)] string zoneName = null, double? centerX = null, double? centerY = null, double? centerZ = null, int? radius = null, double? pointMultiplier = null, bool? isActive = null, [Nullable(2)] string description = null)
		{
			if (string.IsNullOrWhiteSpace(nodeName) || string.IsNullOrWhiteSpace(zoneId))
			{
				return false;
			}
			bool result;
			try
			{
				SqliteConnection connection = this.database.Connection;
				List<string> updates = new List<string>();
				SqliteCommand command = new SqliteCommand
				{
					Connection = connection
				};
				if (!string.IsNullOrWhiteSpace(zoneName))
				{
					updates.Add("zone_name = @zoneName");
					command.Parameters.AddWithValue("@zoneName", zoneName);
				}
				if (centerX != null)
				{
					updates.Add("center_x = @centerX");
					command.Parameters.AddWithValue("@centerX", centerX.Value);
				}
				if (centerY != null)
				{
					updates.Add("center_y = @centerY");
					command.Parameters.AddWithValue("@centerY", centerY.Value);
				}
				if (centerZ != null)
				{
					updates.Add("center_z = @centerZ");
					command.Parameters.AddWithValue("@centerZ", centerZ.Value);
				}
				if (radius != null && radius.Value > 0)
				{
					updates.Add("radius = @radius");
					command.Parameters.AddWithValue("@radius", radius.Value);
				}
				if (pointMultiplier != null)
				{
					updates.Add("point_multiplier = @pointMultiplier");
					command.Parameters.AddWithValue("@pointMultiplier", pointMultiplier.Value);
				}
				if (isActive != null)
				{
					updates.Add("is_active = @isActive");
					command.Parameters.AddWithValue("@isActive", (isActive.Value > false) ? 1 : 0);
				}
				if (description != null)
				{
					updates.Add("description = @description");
					command.Parameters.AddWithValue("@description", description ?? DBNull.Value);
				}
				if (updates.Count == 0)
				{
					result = false;
				}
				else
				{
					command.CommandText = "UPDATE capture_zones SET " + string.Join(", ", updates) + " WHERE node_name = @nodeName AND zone_id = @zoneId;";
					command.Parameters.AddWithValue("@nodeName", nodeName);
					command.Parameters.AddWithValue("@zoneId", zoneId);
					int num = command.ExecuteNonQuery();
					if (num > 0)
					{
						ILogger logger = this.serverApi.Logger;
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(71, 2);
						defaultInterpolatedStringHandler.AppendLiteral("[SRGuildsAndKingdoms:NodeRepository] Updated capture zone '");
						defaultInterpolatedStringHandler.AppendFormatted(zoneId);
						defaultInterpolatedStringHandler.AppendLiteral("' in node '");
						defaultInterpolatedStringHandler.AppendFormatted(nodeName);
						defaultInterpolatedStringHandler.AppendLiteral("'");
						logger.Debug(defaultInterpolatedStringHandler.ToStringAndClear());
					}
					result = (num > 0);
				}
			}
			catch (Exception ex)
			{
				this.serverApi.Logger.Error("[SRGuildsAndKingdoms:NodeRepository] Failed to update capture zone: " + ex.Message);
				result = false;
			}
			return result;
		}

		// Token: 0x0600088E RID: 2190 RVA: 0x0003F8C4 File Offset: 0x0003DAC4
		public bool RemoveAllCaptureZonesForNode(string nodeName)
		{
			if (string.IsNullOrWhiteSpace(nodeName))
			{
				return false;
			}
			bool result;
			try
			{
				SqliteConnection connection = this.database.Connection;
				using (SqliteCommand command = new SqliteCommand("DELETE FROM capture_zones WHERE node_name = @nodeName;", connection))
				{
					command.Parameters.AddWithValue("@nodeName", nodeName);
					int rowsAffected = command.ExecuteNonQuery();
					if (rowsAffected > 0)
					{
						ILogger logger = this.serverApi.Logger;
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(72, 2);
						defaultInterpolatedStringHandler.AppendLiteral("[SRGuildsAndKingdoms:NodeRepository] Deleted ");
						defaultInterpolatedStringHandler.AppendFormatted<int>(rowsAffected);
						defaultInterpolatedStringHandler.AppendLiteral(" capture zones from node '");
						defaultInterpolatedStringHandler.AppendFormatted(nodeName);
						defaultInterpolatedStringHandler.AppendLiteral("'");
						logger.Notification(defaultInterpolatedStringHandler.ToStringAndClear());
					}
					result = (rowsAffected > 0);
				}
			}
			catch (Exception ex)
			{
				this.serverApi.Logger.Error("[SRGuildsAndKingdoms:NodeRepository] Failed to remove all capture zones: " + ex.Message);
				result = false;
			}
			return result;
		}

		// Token: 0x0600088F RID: 2191 RVA: 0x0003F9C0 File Offset: 0x0003DBC0
		[NullableContext(2)]
		public int CreateNodeWar([Nullable(1)] string nodeId, [Nullable(1)] string status, long startTime, long? endTime, long? signupDeadline, int maxGuilds, double capturePointsNeeded, string controllingGuildUid, string controllingGuildName, string previousControllingGuildUid)
		{
			int result;
			try
			{
				SqliteConnection connection = this.database.Connection;
				long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
				using (SqliteCommand command = new SqliteCommand("\n                    INSERT INTO node_wars (node_id, status, start_time, end_time, signup_deadline, max_guilds, \n                        capture_points_needed, controlling_guild_uid, controlling_guild_name, previous_controlling_guild_uid, created_at, updated_at)\n                    VALUES (@nodeId, @status, @startTime, @endTime, @signupDeadline, @maxGuilds, \n                        @capturePointsNeeded, @controllingGuildUid, @controllingGuildName, @previousControllingGuildUid, @createdAt, @updatedAt);\n                    SELECT last_insert_rowid();", connection))
				{
					command.Parameters.AddWithValue("@nodeId", nodeId);
					command.Parameters.AddWithValue("@status", status);
					command.Parameters.AddWithValue("@startTime", startTime);
					command.Parameters.AddWithValue("@endTime", endTime ?? DBNull.Value);
					command.Parameters.AddWithValue("@signupDeadline", signupDeadline ?? DBNull.Value);
					command.Parameters.AddWithValue("@maxGuilds", maxGuilds);
					command.Parameters.AddWithValue("@capturePointsNeeded", capturePointsNeeded);
					command.Parameters.AddWithValue("@controllingGuildUid", controllingGuildUid ?? DBNull.Value);
					command.Parameters.AddWithValue("@controllingGuildName", controllingGuildName ?? DBNull.Value);
					command.Parameters.AddWithValue("@previousControllingGuildUid", previousControllingGuildUid ?? DBNull.Value);
					command.Parameters.AddWithValue("@createdAt", now);
					command.Parameters.AddWithValue("@updatedAt", now);
					int warId = Convert.ToInt32(command.ExecuteScalar());
					if (warId > 0)
					{
						ILogger logger = this.serverApi.Logger;
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(62, 3);
						defaultInterpolatedStringHandler.AppendLiteral("[NodeRepository] Created node war for '");
						defaultInterpolatedStringHandler.AppendFormatted(nodeId);
						defaultInterpolatedStringHandler.AppendLiteral("' with status '");
						defaultInterpolatedStringHandler.AppendFormatted(status);
						defaultInterpolatedStringHandler.AppendLiteral("' (ID: ");
						defaultInterpolatedStringHandler.AppendFormatted<int>(warId);
						defaultInterpolatedStringHandler.AppendLiteral(")");
						logger.Notification(defaultInterpolatedStringHandler.ToStringAndClear());
					}
					result = warId;
				}
			}
			catch (Exception ex)
			{
				this.serverApi.Logger.Error("[NodeRepository] Failed to create node war: " + ex.Message);
				result = -1;
			}
			return result;
		}

		// Token: 0x06000890 RID: 2192 RVA: 0x0003FC0C File Offset: 0x0003DE0C
		[NullableContext(2)]
		public bool UpdateNodeWar(int warId, string status = null, long? startTime = null, long? endTime = null, string controllingGuildUid = null, string controllingGuildName = null)
		{
			if (warId <= 0)
			{
				return false;
			}
			bool result;
			try
			{
				SqliteConnection connection = this.database.Connection;
				List<string> updates = new List<string>();
				SqliteCommand command = new SqliteCommand
				{
					Connection = connection
				};
				if (status != null)
				{
					updates.Add("status = @status");
					command.Parameters.AddWithValue("@status", status);
				}
				if (startTime != null)
				{
					updates.Add("start_time = @startTime");
					command.Parameters.AddWithValue("@startTime", startTime.Value);
				}
				if (endTime != null)
				{
					updates.Add("end_time = @endTime");
					command.Parameters.AddWithValue("@endTime", endTime.Value);
				}
				if (controllingGuildUid != null)
				{
					updates.Add("controlling_guild_uid = @controllingGuildUid");
					command.Parameters.AddWithValue("@controllingGuildUid", controllingGuildUid ?? DBNull.Value);
				}
				if (controllingGuildName != null)
				{
					updates.Add("controlling_guild_name = @controllingGuildName");
					command.Parameters.AddWithValue("@controllingGuildName", controllingGuildName ?? DBNull.Value);
				}
				if (updates.Count == 0)
				{
					result = false;
				}
				else
				{
					updates.Add("updated_at = @updatedAt");
					command.Parameters.AddWithValue("@updatedAt", DateTimeOffset.UtcNow.ToUnixTimeSeconds());
					command.CommandText = "UPDATE node_wars SET " + string.Join(", ", updates) + " WHERE id = @warId;";
					command.Parameters.AddWithValue("@warId", warId);
					int num = command.ExecuteNonQuery();
					if (num > 0)
					{
						ILogger logger = this.serverApi.Logger;
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(34, 1);
						defaultInterpolatedStringHandler.AppendLiteral("[NodeRepository] Updated node war ");
						defaultInterpolatedStringHandler.AppendFormatted<int>(warId);
						logger.Debug(defaultInterpolatedStringHandler.ToStringAndClear());
					}
					result = (num > 0);
				}
			}
			catch (Exception ex)
			{
				this.serverApi.Logger.Error("[NodeRepository] Failed to update node war: " + ex.Message);
				result = false;
			}
			return result;
		}

		// Token: 0x06000891 RID: 2193 RVA: 0x0003FE18 File Offset: 0x0003E018
		[return: Nullable(2)]
		public NodeWarData GetActiveWarForNode(string nodeId)
		{
			try
			{
				SqliteConnection connection = this.database.Connection;
				using (SqliteCommand command = new SqliteCommand("\n                    SELECT id, node_id, status, start_time, end_time, signup_deadline, max_guilds, \n                        capture_points_needed, controlling_guild_uid, controlling_guild_name, previous_controlling_guild_uid, created_at, updated_at\n                    FROM node_wars \n                    WHERE node_id = @nodeId AND status IN ('Scheduled', 'Active')\n                    ORDER BY created_at DESC \n                    LIMIT 1;", connection))
				{
					command.Parameters.AddWithValue("@nodeId", nodeId);
					using (SqliteDataReader reader = command.ExecuteReader())
					{
						if (reader.Read())
						{
							return this.ReadNodeWarData(reader);
						}
					}
				}
			}
			catch (Exception ex)
			{
				this.serverApi.Logger.Error("[NodeRepository] Failed to get active war for node: " + ex.Message);
			}
			return null;
		}

		// Token: 0x06000892 RID: 2194 RVA: 0x0003FED4 File Offset: 0x0003E0D4
		public List<NodeWarData> GetAllActiveWars()
		{
			List<NodeWarData> wars = new List<NodeWarData>();
			try
			{
				SqliteConnection connection = this.database.Connection;
				using (SqliteCommand command = new SqliteCommand("\n                    SELECT id, node_id, status, start_time, end_time, signup_deadline, max_guilds, \n                        capture_points_needed, controlling_guild_uid, controlling_guild_name, previous_controlling_guild_uid, created_at, updated_at\n                    FROM node_wars \n                    WHERE status IN ('Scheduled', 'Active')\n                    ORDER BY start_time;", connection))
				{
					using (SqliteDataReader reader = command.ExecuteReader())
					{
						while (reader.Read())
						{
							wars.Add(this.ReadNodeWarData(reader));
						}
					}
				}
			}
			catch (Exception ex)
			{
				this.serverApi.Logger.Error("[NodeRepository] Failed to get all active wars: " + ex.Message);
			}
			return wars;
		}

		// Token: 0x06000893 RID: 2195 RVA: 0x0003FF88 File Offset: 0x0003E188
		public int AddGuildSignup(int warId, string guildUid, string guildName, string signupByPlayerUid, long signupTime, int membersOnline, int totalMembers, bool isConfirmed)
		{
			int result;
			try
			{
				SqliteConnection connection = this.database.Connection;
				using (SqliteCommand command = new SqliteCommand("\n                    INSERT INTO guild_war_signups (war_id, guild_uid, guild_name, signup_by_player_uid, \n                        signup_time, members_online, total_members, is_confirmed)\n                    VALUES (@warId, @guildUid, @guildName, @signupByPlayerUid, \n                        @signupTime, @membersOnline, @totalMembers, @isConfirmed);\n                    SELECT last_insert_rowid();", connection))
				{
					command.Parameters.AddWithValue("@warId", warId);
					command.Parameters.AddWithValue("@guildUid", guildUid);
					command.Parameters.AddWithValue("@guildName", guildName);
					command.Parameters.AddWithValue("@signupByPlayerUid", signupByPlayerUid);
					command.Parameters.AddWithValue("@signupTime", signupTime);
					command.Parameters.AddWithValue("@membersOnline", membersOnline);
					command.Parameters.AddWithValue("@totalMembers", totalMembers);
					command.Parameters.AddWithValue("@isConfirmed", (isConfirmed > false) ? 1 : 0);
					result = Convert.ToInt32(command.ExecuteScalar());
				}
			}
			catch (Exception ex)
			{
				this.serverApi.Logger.Error("[NodeRepository] Failed to add guild signup: " + ex.Message);
				result = -1;
			}
			return result;
		}

		// Token: 0x06000894 RID: 2196 RVA: 0x000400B8 File Offset: 0x0003E2B8
		public bool RemoveGuildSignup(int warId, string guildUid)
		{
			bool result;
			try
			{
				SqliteConnection connection = this.database.Connection;
				using (SqliteCommand command = new SqliteCommand("DELETE FROM guild_war_signups WHERE war_id = @warId AND guild_uid = @guildUid;", connection))
				{
					command.Parameters.AddWithValue("@warId", warId);
					command.Parameters.AddWithValue("@guildUid", guildUid);
					result = (command.ExecuteNonQuery() > 0);
				}
			}
			catch (Exception ex)
			{
				this.serverApi.Logger.Error("[NodeRepository] Failed to remove guild signup: " + ex.Message);
				result = false;
			}
			return result;
		}

		// Token: 0x06000895 RID: 2197 RVA: 0x00040160 File Offset: 0x0003E360
		public bool SaveGuildWarProgress(int warId, string guildUid, string guildName, double capturePoints, int playersInZone, int kills, int deaths)
		{
			bool result;
			try
			{
				SqliteConnection connection = this.database.Connection;
				long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
				using (SqliteCommand command = new SqliteCommand("\n                    INSERT INTO guild_war_progress (war_id, guild_uid, guild_name, capture_points, \n                        players_in_zone, kills, deaths, last_update)\n                    VALUES (@warId, @guildUid, @guildName, @capturePoints, @playersInZone, @kills, @deaths, @lastUpdate)\n                    ON CONFLICT(war_id, guild_uid) DO UPDATE SET\n                        capture_points = @capturePoints,\n                        players_in_zone = @playersInZone,\n                        kills = @kills,\n                        deaths = @deaths,\n                        last_update = @lastUpdate;", connection))
				{
					command.Parameters.AddWithValue("@warId", warId);
					command.Parameters.AddWithValue("@guildUid", guildUid);
					command.Parameters.AddWithValue("@guildName", guildName);
					command.Parameters.AddWithValue("@capturePoints", capturePoints);
					command.Parameters.AddWithValue("@playersInZone", playersInZone);
					command.Parameters.AddWithValue("@kills", kills);
					command.Parameters.AddWithValue("@deaths", deaths);
					command.Parameters.AddWithValue("@lastUpdate", now);
					command.ExecuteNonQuery();
					result = true;
				}
			}
			catch (Exception ex)
			{
				this.serverApi.Logger.Error("[NodeRepository] Failed to save guild war progress: " + ex.Message);
				result = false;
			}
			return result;
		}

		// Token: 0x06000896 RID: 2198 RVA: 0x000402A4 File Offset: 0x0003E4A4
		public List<GuildWarProgressData> GetGuildProgressForWar(int warId)
		{
			List<GuildWarProgressData> progress = new List<GuildWarProgressData>();
			try
			{
				SqliteConnection connection = this.database.Connection;
				using (SqliteCommand command = new SqliteCommand("\n                    SELECT id, war_id, guild_uid, guild_name, capture_points, players_in_zone, kills, deaths, last_update\n                    FROM guild_war_progress \n                    WHERE war_id = @warId\n                    ORDER BY capture_points DESC;", connection))
				{
					command.Parameters.AddWithValue("@warId", warId);
					using (SqliteDataReader reader = command.ExecuteReader())
					{
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
				}
			}
			catch (Exception ex)
			{
				this.serverApi.Logger.Error("[NodeRepository] Failed to get guild progress for war: " + ex.Message);
			}
			return progress;
		}

		// Token: 0x06000897 RID: 2199 RVA: 0x000403E8 File Offset: 0x0003E5E8
		public List<GuildSignupData> GetGuildSignupsForWar(int warId)
		{
			List<GuildSignupData> signups = new List<GuildSignupData>();
			try
			{
				SqliteConnection connection = this.database.Connection;
				using (SqliteCommand command = new SqliteCommand("\n                    SELECT id, war_id, guild_uid, guild_name, signup_by_player_uid, signup_time, \n                           members_online, total_members, is_confirmed\n                    FROM guild_war_signups \n                    WHERE war_id = @warId\n                    ORDER BY signup_time;", connection))
				{
					command.Parameters.AddWithValue("@warId", warId);
					using (SqliteDataReader reader = command.ExecuteReader())
					{
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
								IsConfirmed = (reader.GetInt32(8) == 1)
							});
						}
					}
				}
			}
			catch (Exception ex)
			{
				this.serverApi.Logger.Error("[NodeRepository] Failed to get guild signups for war: " + ex.Message);
			}
			return signups;
		}

		// Token: 0x06000898 RID: 2200 RVA: 0x0004052C File Offset: 0x0003E72C
		private NodeWarData ReadNodeWarData(SqliteDataReader reader)
		{
			return new NodeWarData
			{
				Id = reader.GetInt32(0),
				NodeId = reader.GetString(1),
				Status = reader.GetString(2),
				StartTime = reader.GetInt64(3),
				EndTime = (reader.IsDBNull(4) ? null : new long?(reader.GetInt64(4))),
				SignupDeadline = (reader.IsDBNull(5) ? null : new long?(reader.GetInt64(5))),
				MaxGuilds = reader.GetInt32(6),
				CapturePointsNeeded = reader.GetDouble(7),
				ControllingGuildUid = (reader.IsDBNull(8) ? null : reader.GetString(8)),
				ControllingGuildName = (reader.IsDBNull(9) ? null : reader.GetString(9)),
				PreviousControllingGuildUid = (reader.IsDBNull(10) ? null : reader.GetString(10)),
				CreatedAt = reader.GetInt64(11),
				UpdatedAt = reader.GetInt64(12)
			};
		}

		// Token: 0x04000375 RID: 885
		private readonly ICoreServerAPI serverApi;

		// Token: 0x04000376 RID: 886
		private readonly GuildDatabase database;
	}
}
