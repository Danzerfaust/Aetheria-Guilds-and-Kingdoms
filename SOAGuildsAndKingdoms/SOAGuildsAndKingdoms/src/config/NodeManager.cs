using SOAGuildsAndKingdoms.src.database;
using System;
using System.Collections.Generic;
using System.Linq;
using Vintagestory.API.Server;
using DbNodeData = SOAGuildsAndKingdoms.src.database.NodeData;
using NetworkNodeData = SOAGuildsAndKingdoms.src.network.NodeData;

namespace SOAGuildsAndKingdoms.src.config
{
    /// <summary>
    /// Manages node data with in-memory cache and database persistence
    /// </summary>
    public class NodeManager
    {
        private readonly ICoreServerAPI api;
        private readonly NodeRepository repository;

        // Fast lookup cache: node name -> NodeData
        private readonly Dictionary<string, DbNodeData> nodesByName = new();
        // Also cache by ID for quick lookups
        private readonly Dictionary<int, DbNodeData> nodesById = new();
        private bool cacheLoaded = false;
        private SOAGuildsAndKingdomsModSystem? modSystem;

        public NodeManager(ICoreServerAPI api, NodeRepository repository)
        {
            this.api = api;
            this.repository = repository;
        }

        /// <summary>
        /// Adds a new node
        /// </summary>
        /// <param name="name">Unique node name</param>
        /// <param name="x">World X coordinate</param>
        /// <param name="z">World Z coordinate</param>
        /// <param name="radius">Node radius in blocks</param>
        /// <returns>The created NodeData, or null if failed</returns>
        public DbNodeData? AddNode(string name, int x, int z, int radius)
        {
            EnsureCacheLoaded();

            if (string.IsNullOrWhiteSpace(name) || radius <= 0)
                return null;

            // Check if node already exists
            if (nodesByName.ContainsKey(name))
            {
                api.Logger.Warning($"[NodeManager] Node '{name}' already exists");
                return null;
            }

            // Add to database
            int nodeId = repository.AddNode(name, x, z, radius);
            
            if (nodeId <= 0)
                return null;

            // Create node data and add to cache
            var nodeData = new DbNodeData
            {
                Id = nodeId,
                Name = name,
                X = x,
                Z = z,
                Radius = radius,
                CreatedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            };

            nodesByName[name] = nodeData;
            nodesById[nodeId] = nodeData;

            // Broadcast config update to all clients
            modSystem?.NetworkHandler?.BroadcastGuildConfigToAll();

            api.Logger.Notification($"[NodeManager] Added node '{name}' at ({x}, {z}) with radius {radius}");

            return nodeData;
        }

        /// <summary>
        /// Removes a node by name
        /// </summary>
        public bool RemoveNode(string name)
        {
            EnsureCacheLoaded();

            if (string.IsNullOrWhiteSpace(name))
                return false;

            if (!nodesByName.TryGetValue(name, out var node))
                return false;

            // Remove from database
            bool removed = repository.RemoveNodeByName(name);

            if (removed)
            {
                // Remove from cache
                nodesByName.Remove(name);
                nodesById.Remove(node.Id);

                // Broadcast config update to all clients
                modSystem?.NetworkHandler?.BroadcastGuildConfigToAll();

                api.Logger.Notification($"[NodeManager] Removed node '{name}'");
            }

            return removed;
        }

        /// <summary>
        /// Removes a node by ID
        /// </summary>
        public bool RemoveNodeById(int nodeId)
        {
            EnsureCacheLoaded();

            if (!nodesById.TryGetValue(nodeId, out var node))
                return false;

            return RemoveNode(node.Name);
        }

        /// <summary>
        /// Updates a node's properties
        /// </summary>
        public bool UpdateNode(string name, string? newName = null, int? x = null, int? z = null, int? radius = null)
        {
            EnsureCacheLoaded();

            if (string.IsNullOrWhiteSpace(name))
                return false;

            if (!nodesByName.TryGetValue(name, out var node))
                return false;

            // Update in database
            bool updated = repository.UpdateNode(node.Id, newName, x, z, radius);

            if (updated)
            {
                // Update cache
                if (!string.IsNullOrWhiteSpace(newName) && newName != name)
                {
                    nodesByName.Remove(name);
                    node.Name = newName;
                    nodesByName[newName] = node;
                }

                if (x.HasValue)
                    node.X = x.Value;

                if (z.HasValue)
                    node.Z = z.Value;

                if (radius.HasValue && radius.Value > 0)
                    node.Radius = radius.Value;

                // Broadcast config update to all clients
                modSystem?.NetworkHandler?.BroadcastGuildConfigToAll();

                api.Logger.Notification($"[NodeManager] Updated node '{name}'");
            }

            return updated;
        }

        /// <summary>
        /// Gets a node by name
        /// </summary>
        public DbNodeData? GetNode(string name)
        {
            EnsureCacheLoaded();

            if (string.IsNullOrWhiteSpace(name))
                return null;

            nodesByName.TryGetValue(name, out var node);
            return node;
        }

        /// <summary>
        /// Gets a node by ID
        /// </summary>
        public DbNodeData? GetNodeById(int nodeId)
        {
            EnsureCacheLoaded();

            nodesById.TryGetValue(nodeId, out var node);
            return node;
        }

        /// <summary>
        /// Gets all nodes
        /// </summary>
        public List<DbNodeData> GetAllNodes()
        {
            EnsureCacheLoaded();
            return nodesByName.Values.ToList();
        }

        /// <summary>
        /// Checks if a node exists by name
        /// </summary>
        public bool NodeExists(string name)
        {
            EnsureCacheLoaded();

            if (string.IsNullOrWhiteSpace(name))
                return false;

            return nodesByName.ContainsKey(name);
        }

        /// <summary>
        /// Gets the total number of nodes
        /// </summary>
        public int GetNodeCount()
        {
            EnsureCacheLoaded();
            return nodesByName.Count;
        }

        /// <summary>
        /// Converts all nodes to network packet format for client sync
        /// </summary>
        public List<NetworkNodeData> GetNodesForNetworkPacket()
        {
            EnsureCacheLoaded();

            return nodesByName.Values.Select(n => new NetworkNodeData
            {
                Name = n.Name,
                X = n.X,
                Z = n.Z,
                Radius = n.Radius
            }).ToList();
        }

        /// <summary>
        /// Loads node data from database into cache.
        /// Called automatically on first access, but can be called manually during startup.
        /// </summary>
        public void Load()
        {
            try
            {
                api.Logger.Notification("[NodeManager] Loading nodes from database...");

                nodesByName.Clear();
                nodesById.Clear();

                var allNodes = repository.GetAllNodes();

                foreach (var node in allNodes)
                {
                    nodesByName[node.Name] = node;
                    nodesById[node.Id] = node;
                }

                cacheLoaded = true;
                api.Logger.Notification($"[NodeManager] Loaded {nodesByName.Count} node(s) from database");

                modSystem = api.ModLoader.GetModSystem<SOAGuildsAndKingdomsModSystem>();
            }
            catch (Exception ex)
            {
                api.Logger.Error($"[NodeManager] Failed to load node data: {ex.Message}");
                nodesByName.Clear();
                nodesById.Clear();
                cacheLoaded = true; // Mark as loaded to prevent repeated failures
            }
        }

        /// <summary>
        /// Ensures the cache is loaded before accessing data
        /// </summary>
        private void EnsureCacheLoaded()
        {
            if (!cacheLoaded)
            {
                Load();
            }
        }

        #region Capture Zone Methods

        /// <summary>
        /// Adds a new capture zone to a node
        /// </summary>
        public int AddCaptureZone(string nodeName, string zoneId, string zoneName, double centerX, double centerY, double centerZ,
            int radius, double pointMultiplier = 1.0, bool isActive = true, string? description = null)
        {
            EnsureCacheLoaded();

            if (!NodeExists(nodeName))
            {
                api.Logger.Warning($"[NodeManager] Cannot add capture zone: Node '{nodeName}' does not exist");
                return -1;
            }

            int captureZoneId = repository.AddCaptureZone(nodeName, zoneId, zoneName, centerX, centerY, centerZ, radius, pointMultiplier, isActive, description);

            if (captureZoneId > 0)
            {
                api.Logger.Notification($"[NodeManager] Added capture zone '{zoneName}' ({zoneId}) to node '{nodeName}'");
            }

            return captureZoneId;
        }

        /// <summary>
        /// Removes a capture zone from a node
        /// </summary>
        public bool RemoveCaptureZone(string nodeName, string zoneId)
        {
            EnsureCacheLoaded();

            bool removed = repository.RemoveCaptureZone(nodeName, zoneId);

            if (removed)
            {
                api.Logger.Notification($"[NodeManager] Removed capture zone '{zoneId}' from node '{nodeName}'");
            }

            return removed;
        }

        /// <summary>
        /// Gets all capture zones for a specific node
        /// </summary>
        public List<CaptureZoneData> GetCaptureZonesForNode(string nodeName)
        {
            EnsureCacheLoaded();

            if (!NodeExists(nodeName))
            {
                api.Logger.Warning($"[NodeManager] Cannot get capture zones: Node '{nodeName}' does not exist");
                return new List<CaptureZoneData>();
            }

            return repository.GetCaptureZonesForNode(nodeName);
        }

        /// <summary>
        /// Gets all capture zones from the database
        /// </summary>
        public List<CaptureZoneData> GetAllCaptureZones()
        {
            EnsureCacheLoaded();
            return repository.GetAllCaptureZones();
        }

        /// <summary>
        /// Updates a capture zone's properties
        /// </summary>
        public bool UpdateCaptureZone(string nodeName, string zoneId, string? zoneName = null, double? centerX = null,
            double? centerY = null, double? centerZ = null, int? radius = null, double? pointMultiplier = null, bool? isActive = null, string? description = null)
        {
            EnsureCacheLoaded();

            bool updated = repository.UpdateCaptureZone(nodeName, zoneId, zoneName, centerX, centerY, centerZ, radius, pointMultiplier, isActive, description);

            if (updated)
            {
                api.Logger.Notification($"[NodeManager] Updated capture zone '{zoneId}' in node '{nodeName}'");
            }

            return updated;
        }

        /// <summary>
        /// Removes all capture zones for a specific node
        /// </summary>
        public bool RemoveAllCaptureZonesForNode(string nodeName)
        {
            EnsureCacheLoaded();

            bool removed = repository.RemoveAllCaptureZonesForNode(nodeName);

            if (removed)
            {
                api.Logger.Notification($"[NodeManager] Removed all capture zones from node '{nodeName}'");
            }

            return removed;
        }

        #endregion

        #region Repository Access

        /// <summary>
        /// Gets the underlying NodeRepository for direct database access
        /// </summary>
        /// <returns>The NodeRepository instance</returns>
        public NodeRepository GetNodeRepository()
        {
            return repository;
        }

        #endregion
    }
}
