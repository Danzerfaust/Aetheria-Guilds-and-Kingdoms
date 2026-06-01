using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using SRGuildsAndKingdoms.src.database;
using SRGuildsAndKingdoms.src.network;
using Vintagestory.API.Common;
using Vintagestory.API.Server;

namespace SRGuildsAndKingdoms.src.config
{
	// Token: 0x020000C6 RID: 198
	[NullableContext(1)]
	[Nullable(0)]
	public class NodeManager
	{
		// Token: 0x06000992 RID: 2450 RVA: 0x0004463C File Offset: 0x0004283C
		public NodeManager(ICoreServerAPI api, NodeRepository repository)
		{
			this.api = api;
			this.repository = repository;
		}

		// Token: 0x06000993 RID: 2451 RVA: 0x00044668 File Offset: 0x00042868
		[return: Nullable(2)]
		public SRGuildsAndKingdoms.src.database.NodeData AddNode(string name, int x, int z, int radius)
		{
			this.EnsureCacheLoaded();
			if (string.IsNullOrWhiteSpace(name) || radius <= 0)
			{
				return null;
			}
			if (this.nodesByName.ContainsKey(name))
			{
				this.api.Logger.Warning("[NodeManager] Node '" + name + "' already exists");
				return null;
			}
			int nodeId = this.repository.AddNode(name, x, z, radius);
			if (nodeId <= 0)
			{
				return null;
			}
			SRGuildsAndKingdoms.src.database.NodeData nodeData = new SRGuildsAndKingdoms.src.database.NodeData
			{
				Id = nodeId,
				Name = name,
				X = x,
				Z = z,
				Radius = radius,
				CreatedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
			};
			this.nodesByName[name] = nodeData;
			this.nodesById[nodeId] = nodeData;
			SRGuildsAndKingdomsModSystem srguildsAndKingdomsModSystem = this.modSystem;
			if (srguildsAndKingdomsModSystem != null)
			{
				GuildNetworkHandler networkHandler = srguildsAndKingdomsModSystem.NetworkHandler;
				if (networkHandler != null)
				{
					networkHandler.BroadcastGuildConfigToAll();
				}
			}
			ILogger logger = this.api.Logger;
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(48, 4);
			defaultInterpolatedStringHandler.AppendLiteral("[NodeManager] Added node '");
			defaultInterpolatedStringHandler.AppendFormatted(name);
			defaultInterpolatedStringHandler.AppendLiteral("' at (");
			defaultInterpolatedStringHandler.AppendFormatted<int>(x);
			defaultInterpolatedStringHandler.AppendLiteral(", ");
			defaultInterpolatedStringHandler.AppendFormatted<int>(z);
			defaultInterpolatedStringHandler.AppendLiteral(") with radius ");
			defaultInterpolatedStringHandler.AppendFormatted<int>(radius);
			logger.Notification(defaultInterpolatedStringHandler.ToStringAndClear());
			return nodeData;
		}

		// Token: 0x06000994 RID: 2452 RVA: 0x000447B8 File Offset: 0x000429B8
		public bool RemoveNode(string name)
		{
			this.EnsureCacheLoaded();
			if (string.IsNullOrWhiteSpace(name))
			{
				return false;
			}
			SRGuildsAndKingdoms.src.database.NodeData node;
			if (!this.nodesByName.TryGetValue(name, out node))
			{
				return false;
			}
			bool flag = this.repository.RemoveNodeByName(name);
			if (flag)
			{
				this.nodesByName.Remove(name);
				this.nodesById.Remove(node.Id);
				SRGuildsAndKingdomsModSystem srguildsAndKingdomsModSystem = this.modSystem;
				if (srguildsAndKingdomsModSystem != null)
				{
					GuildNetworkHandler networkHandler = srguildsAndKingdomsModSystem.NetworkHandler;
					if (networkHandler != null)
					{
						networkHandler.BroadcastGuildConfigToAll();
					}
				}
				this.api.Logger.Notification("[NodeManager] Removed node '" + name + "'");
			}
			return flag;
		}

		// Token: 0x06000995 RID: 2453 RVA: 0x00044854 File Offset: 0x00042A54
		public bool RemoveNodeById(int nodeId)
		{
			this.EnsureCacheLoaded();
			SRGuildsAndKingdoms.src.database.NodeData node;
			return this.nodesById.TryGetValue(nodeId, out node) && this.RemoveNode(node.Name);
		}

		// Token: 0x06000996 RID: 2454 RVA: 0x00044888 File Offset: 0x00042A88
		public bool UpdateNode(string name, [Nullable(2)] string newName = null, int? x = null, int? z = null, int? radius = null)
		{
			this.EnsureCacheLoaded();
			if (string.IsNullOrWhiteSpace(name))
			{
				return false;
			}
			SRGuildsAndKingdoms.src.database.NodeData node;
			if (!this.nodesByName.TryGetValue(name, out node))
			{
				return false;
			}
			bool flag = this.repository.UpdateNode(node.Id, newName, x, z, radius);
			if (flag)
			{
				if (!string.IsNullOrWhiteSpace(newName) && newName != name)
				{
					this.nodesByName.Remove(name);
					node.Name = newName;
					this.nodesByName[newName] = node;
				}
				if (x != null)
				{
					node.X = x.Value;
				}
				if (z != null)
				{
					node.Z = z.Value;
				}
				if (radius != null && radius.Value > 0)
				{
					node.Radius = radius.Value;
				}
				SRGuildsAndKingdomsModSystem srguildsAndKingdomsModSystem = this.modSystem;
				if (srguildsAndKingdomsModSystem != null)
				{
					GuildNetworkHandler networkHandler = srguildsAndKingdomsModSystem.NetworkHandler;
					if (networkHandler != null)
					{
						networkHandler.BroadcastGuildConfigToAll();
					}
				}
				this.api.Logger.Notification("[NodeManager] Updated node '" + name + "'");
			}
			return flag;
		}

		// Token: 0x06000997 RID: 2455 RVA: 0x00044990 File Offset: 0x00042B90
		[return: Nullable(2)]
		public SRGuildsAndKingdoms.src.database.NodeData GetNode(string name)
		{
			this.EnsureCacheLoaded();
			if (string.IsNullOrWhiteSpace(name))
			{
				return null;
			}
			SRGuildsAndKingdoms.src.database.NodeData node;
			this.nodesByName.TryGetValue(name, out node);
			return node;
		}

		// Token: 0x06000998 RID: 2456 RVA: 0x000449C0 File Offset: 0x00042BC0
		[NullableContext(2)]
		public SRGuildsAndKingdoms.src.database.NodeData GetNodeById(int nodeId)
		{
			this.EnsureCacheLoaded();
			SRGuildsAndKingdoms.src.database.NodeData node;
			this.nodesById.TryGetValue(nodeId, out node);
			return node;
		}

		// Token: 0x06000999 RID: 2457 RVA: 0x000449E3 File Offset: 0x00042BE3
		public List<SRGuildsAndKingdoms.src.database.NodeData> GetAllNodes()
		{
			this.EnsureCacheLoaded();
			return this.nodesByName.Values.ToList<SRGuildsAndKingdoms.src.database.NodeData>();
		}

		// Token: 0x0600099A RID: 2458 RVA: 0x000449FB File Offset: 0x00042BFB
		public bool NodeExists(string name)
		{
			this.EnsureCacheLoaded();
			return !string.IsNullOrWhiteSpace(name) && this.nodesByName.ContainsKey(name);
		}

		// Token: 0x0600099B RID: 2459 RVA: 0x00044A19 File Offset: 0x00042C19
		public int GetNodeCount()
		{
			this.EnsureCacheLoaded();
			return this.nodesByName.Count;
		}

		// Token: 0x0600099C RID: 2460 RVA: 0x00044A2C File Offset: 0x00042C2C
		public List<SRGuildsAndKingdoms.src.network.NodeData> GetNodesForNetworkPacket()
		{
			this.EnsureCacheLoaded();
			return (from n in this.nodesByName.Values
			select new SRGuildsAndKingdoms.src.network.NodeData
			{
				Name = n.Name,
				X = n.X,
				Z = n.Z,
				Radius = n.Radius
			}).ToList<SRGuildsAndKingdoms.src.network.NodeData>();
		}

		// Token: 0x0600099D RID: 2461 RVA: 0x00044A68 File Offset: 0x00042C68
		public void Load()
		{
			try
			{
				this.api.Logger.Notification("[NodeManager] Loading nodes from database...");
				this.nodesByName.Clear();
				this.nodesById.Clear();
				foreach (SRGuildsAndKingdoms.src.database.NodeData node in this.repository.GetAllNodes())
				{
					this.nodesByName[node.Name] = node;
					this.nodesById[node.Id] = node;
				}
				this.cacheLoaded = true;
				ILogger logger = this.api.Logger;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(43, 1);
				defaultInterpolatedStringHandler.AppendLiteral("[NodeManager] Loaded ");
				defaultInterpolatedStringHandler.AppendFormatted<int>(this.nodesByName.Count);
				defaultInterpolatedStringHandler.AppendLiteral(" node(s) from database");
				logger.Notification(defaultInterpolatedStringHandler.ToStringAndClear());
				this.modSystem = this.api.ModLoader.GetModSystem<SRGuildsAndKingdomsModSystem>(true);
			}
			catch (Exception ex)
			{
				this.api.Logger.Error("[NodeManager] Failed to load node data: " + ex.Message);
				this.nodesByName.Clear();
				this.nodesById.Clear();
				this.cacheLoaded = true;
			}
		}

		// Token: 0x0600099E RID: 2462 RVA: 0x00044BC0 File Offset: 0x00042DC0
		private void EnsureCacheLoaded()
		{
			if (!this.cacheLoaded)
			{
				this.Load();
			}
		}

		// Token: 0x0600099F RID: 2463 RVA: 0x00044BD0 File Offset: 0x00042DD0
		public int AddCaptureZone(string nodeName, string zoneId, string zoneName, double centerX, double centerY, double centerZ, int radius, double pointMultiplier = 1.0, bool isActive = true, [Nullable(2)] string description = null)
		{
			this.EnsureCacheLoaded();
			if (!this.NodeExists(nodeName))
			{
				this.api.Logger.Warning("[NodeManager] Cannot add capture zone: Node '" + nodeName + "' does not exist");
				return -1;
			}
			int num = this.repository.AddCaptureZone(nodeName, zoneId, zoneName, centerX, centerY, centerZ, radius, pointMultiplier, isActive, description);
			if (num > 0)
			{
				ILogger logger = this.api.Logger;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(49, 3);
				defaultInterpolatedStringHandler.AppendLiteral("[NodeManager] Added capture zone '");
				defaultInterpolatedStringHandler.AppendFormatted(zoneName);
				defaultInterpolatedStringHandler.AppendLiteral("' (");
				defaultInterpolatedStringHandler.AppendFormatted(zoneId);
				defaultInterpolatedStringHandler.AppendLiteral(") to node '");
				defaultInterpolatedStringHandler.AppendFormatted(nodeName);
				defaultInterpolatedStringHandler.AppendLiteral("'");
				logger.Notification(defaultInterpolatedStringHandler.ToStringAndClear());
			}
			return num;
		}

		// Token: 0x060009A0 RID: 2464 RVA: 0x00044C98 File Offset: 0x00042E98
		public bool RemoveCaptureZone(string nodeName, string zoneId)
		{
			this.EnsureCacheLoaded();
			bool flag = this.repository.RemoveCaptureZone(nodeName, zoneId);
			if (flag)
			{
				ILogger logger = this.api.Logger;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(50, 2);
				defaultInterpolatedStringHandler.AppendLiteral("[NodeManager] Removed capture zone '");
				defaultInterpolatedStringHandler.AppendFormatted(zoneId);
				defaultInterpolatedStringHandler.AppendLiteral("' from node '");
				defaultInterpolatedStringHandler.AppendFormatted(nodeName);
				defaultInterpolatedStringHandler.AppendLiteral("'");
				logger.Notification(defaultInterpolatedStringHandler.ToStringAndClear());
			}
			return flag;
		}

		// Token: 0x060009A1 RID: 2465 RVA: 0x00044D10 File Offset: 0x00042F10
		public List<CaptureZoneData> GetCaptureZonesForNode(string nodeName)
		{
			this.EnsureCacheLoaded();
			if (!this.NodeExists(nodeName))
			{
				this.api.Logger.Warning("[NodeManager] Cannot get capture zones: Node '" + nodeName + "' does not exist");
				return new List<CaptureZoneData>();
			}
			return this.repository.GetCaptureZonesForNode(nodeName);
		}

		// Token: 0x060009A2 RID: 2466 RVA: 0x00044D5E File Offset: 0x00042F5E
		public List<CaptureZoneData> GetAllCaptureZones()
		{
			this.EnsureCacheLoaded();
			return this.repository.GetAllCaptureZones();
		}

		// Token: 0x060009A3 RID: 2467 RVA: 0x00044D74 File Offset: 0x00042F74
		public bool UpdateCaptureZone(string nodeName, string zoneId, [Nullable(2)] string zoneName = null, double? centerX = null, double? centerY = null, double? centerZ = null, int? radius = null, double? pointMultiplier = null, bool? isActive = null, [Nullable(2)] string description = null)
		{
			this.EnsureCacheLoaded();
			bool flag = this.repository.UpdateCaptureZone(nodeName, zoneId, zoneName, centerX, centerY, centerZ, radius, pointMultiplier, isActive, description);
			if (flag)
			{
				ILogger logger = this.api.Logger;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(48, 2);
				defaultInterpolatedStringHandler.AppendLiteral("[NodeManager] Updated capture zone '");
				defaultInterpolatedStringHandler.AppendFormatted(zoneId);
				defaultInterpolatedStringHandler.AppendLiteral("' in node '");
				defaultInterpolatedStringHandler.AppendFormatted(nodeName);
				defaultInterpolatedStringHandler.AppendLiteral("'");
				logger.Notification(defaultInterpolatedStringHandler.ToStringAndClear());
			}
			return flag;
		}

		// Token: 0x060009A4 RID: 2468 RVA: 0x00044DFB File Offset: 0x00042FFB
		public bool RemoveAllCaptureZonesForNode(string nodeName)
		{
			this.EnsureCacheLoaded();
			bool flag = this.repository.RemoveAllCaptureZonesForNode(nodeName);
			if (flag)
			{
				this.api.Logger.Notification("[NodeManager] Removed all capture zones from node '" + nodeName + "'");
			}
			return flag;
		}

		// Token: 0x060009A5 RID: 2469 RVA: 0x00044E32 File Offset: 0x00043032
		public NodeRepository GetNodeRepository()
		{
			return this.repository;
		}

		// Token: 0x040003CF RID: 975
		private readonly ICoreServerAPI api;

		// Token: 0x040003D0 RID: 976
		private readonly NodeRepository repository;

		// Token: 0x040003D1 RID: 977
		private readonly Dictionary<string, SRGuildsAndKingdoms.src.database.NodeData> nodesByName = new Dictionary<string, SRGuildsAndKingdoms.src.database.NodeData>();

		// Token: 0x040003D2 RID: 978
		private readonly Dictionary<int, SRGuildsAndKingdoms.src.database.NodeData> nodesById = new Dictionary<int, SRGuildsAndKingdoms.src.database.NodeData>();

		// Token: 0x040003D3 RID: 979
		private bool cacheLoaded;

		// Token: 0x040003D4 RID: 980
		[Nullable(2)]
		private SRGuildsAndKingdomsModSystem modSystem;
	}
}
