using System;
using System.Collections.Generic;
using System.Linq;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using SOAGuildsAndKingdoms.src.config;
using SOAGuildsAndKingdomsPVP.src.utils;

namespace SOAGuildsAndKingdomsPVP.src.nodewars
{
    /// <summary>
    /// Manages all node war operations including scheduling, tracking, and resolution
    /// </summary>
    public class NodeWarManager
    {
        private readonly ICoreServerAPI sapi;
        private readonly Dictionary<string, NodeWar> activeWars;
        private readonly Dictionary<string, NodeWarParticipant> participants;
        private readonly Dictionary<string, NodeZone> nodes;
        private NodeManager? nodeManager;
        private CaptureZoneSystem? captureZoneSystem;

        // Maps node IDs to database war IDs for persistence
        private readonly Dictionary<string, int> warIdMapping;

        // Player zone tracking for automatic hologram management
        // Key: playerUID, Value: set of war node IDs they're currently inside
        private readonly Dictionary<string, HashSet<string>> playerZoneTracking;

        // Active war zone monitors (tick listeners)
        // Key: nodeId, Value: listener ID
        private readonly Dictionary<string, long> activeWarMonitors;

        // Reference to network handler for sending hologram packets
        private network.PVPNetworkHandler? networkHandler;

        public NodeWarManager(ICoreServerAPI api)
        {
            sapi = api;
            activeWars = new Dictionary<string, NodeWar>();
            participants = new Dictionary<string, NodeWarParticipant>();
            nodes = new Dictionary<string, NodeZone>();
            warIdMapping = new Dictionary<string, int>();
            playerZoneTracking = new Dictionary<string, HashSet<string>>();
            activeWarMonitors = new Dictionary<string, long>();
        }

        /// <summary>
        /// Initialize the node war manager and capture zone system
        /// </summary>
        public void Initialize()
        {
            captureZoneSystem = new CaptureZoneSystem(sapi, this);
            captureZoneSystem.Initialize();
            sapi.Logger.Notification("[NodeWars] Node war manager initialized with capture zone system");
        }

        /// <summary>
        /// Shutdown the node war manager and cleanup resources
        /// </summary>
        public void Shutdown()
        {
            captureZoneSystem?.Shutdown();
            sapi.Logger.Notification("[NodeWars] Node war manager shutdown");
        }

        /// <summary>
        /// Sets the NodeManager instance for database persistence
        /// </summary>
        public void SetNodeManager(NodeManager manager)
        {
            nodeManager = manager;
            LoadNodesFromDatabase();
            LoadWarsFromDatabase();
        }

        /// <summary>
        /// Load all nodes from the database into memory
        /// </summary>
        private void LoadNodesFromDatabase()
        {
            if (nodeManager == null)
            {
                sapi.Logger.Warning("[NodeWars] Cannot load nodes - NodeManager not set");
                return;
            }

            try
            {
                var dbNodes = nodeManager.GetAllNodes();

                if (dbNodes.Count == 0)
                {
                    sapi.Logger.Notification("[NodeWars] No nodes found in database");
                    return;
                }

                int loadedCount = 0;
                int captureZonesLoaded = 0;

                foreach (var dbNode in dbNodes)
                {
                    // Create NodeZone from database data
                    var nodeZone = new NodeZone(
                        dbNode.Name, // Use name as nodeId
                        dbNode.Name, // Use name as nodeName
                        new Vintagestory.API.MathTools.Vec3d(dbNode.X, 0, dbNode.Z), // Y coordinate doesn't matter for zones
                        dbNode.Radius
                    )
                    {
                        IsActive = true,
                        Description = $"Node zone: {dbNode.Name}"
                    };

                    // Load capture zones for this node
                    var captureZones = nodeManager.GetCaptureZonesForNode(dbNode.Name);
                    foreach (var czData in captureZones)
                    {
                        var captureZone = new CaptureZone(
                            czData.ZoneId,
                            czData.ZoneName,
                            new Vintagestory.API.MathTools.Vec3d(czData.CenterX, czData.CenterY, czData.CenterZ),
                            czData.Radius
                        )
                        {
                            Description = czData.Description ?? $"Capture zone: {czData.ZoneName}",
                            IsActive = czData.IsActive,
                            PointMultiplier = czData.PointMultiplier
                        };

                        nodeZone.CaptureZones[czData.ZoneId] = captureZone;
                        captureZonesLoaded++;
                    }

                    // Add to in-memory collection
                    nodes[dbNode.Name] = nodeZone;
                    loadedCount++;
                }

                sapi.Logger.Notification($"[NodeWars] Loaded {loadedCount} node(s) and {captureZonesLoaded} capture zone(s) from database");
            }
            catch (Exception ex)
            {
                sapi.Logger.Error($"[NodeWars] Failed to load nodes from database: {ex.Message}");
            }
        }

        /// <summary>
        /// Load all active and scheduled wars from the database into memory
        /// </summary>
        private void LoadWarsFromDatabase()
        {
            if (nodeManager == null)
            {
                sapi.Logger.Warning("[NodeWars] Cannot load wars - NodeManager not set");
                return;
            }

            try
            {
                var repository = nodeManager.GetNodeRepository();
                if (repository == null)
                {
                    sapi.Logger.Warning("[NodeWars] Cannot load wars - NodeRepository not available");
                    return;
                }

                // Get all scheduled and active wars
                var dbWars = repository.GetAllActiveWars();

                if (dbWars.Count == 0)
                {
                    sapi.Logger.Notification("[NodeWars] No active or scheduled wars found in database");
                    return;
                }

                int loadedCount = 0;
                int signupsLoaded = 0;
                int progressLoaded = 0;

                foreach (var dbWar in dbWars)
                {
                    // Get the node zone for this war
                    if (!nodes.TryGetValue(dbWar.NodeId, out var node))
                    {
                        sapi.Logger.Warning($"[NodeWars] Cannot load war {dbWar.Id}: Node '{dbWar.NodeId}' not found");
                        continue;
                    }

                    // Parse status enum
                    if (!Enum.TryParse<NodeWarStatus>(dbWar.Status, out var status))
                    {
                        sapi.Logger.Warning($"[NodeWars] Cannot load war {dbWar.Id}: Invalid status '{dbWar.Status}'");
                        continue;
                    }

                    // Create NodeWar object
                    var war = new NodeWar(dbWar.NodeId, node.Center, node.Radius, new NodeWarConfig())
                    {
                        StartTime = DateTimeOffset.FromUnixTimeSeconds(dbWar.StartTime).UtcDateTime,
                        EndTime = dbWar.EndTime.HasValue ? DateTimeOffset.FromUnixTimeSeconds(dbWar.EndTime.Value).UtcDateTime : null,
                        SignupDeadline = dbWar.SignupDeadline.HasValue ? DateTimeOffset.FromUnixTimeSeconds(dbWar.SignupDeadline.Value).UtcDateTime : null,
                        Status = status,
                        MaxGuilds = dbWar.MaxGuilds,
                        PreviousControllingGuildUid = dbWar.PreviousControllingGuildUid,
                        ControllingGuildUid = dbWar.ControllingGuildUid
                    };

                    // Set capture points needed in config
                    war.Config.CapturePointsNeeded = dbWar.CapturePointsNeeded;

                    // Load guild signups for scheduled wars
                    if (status == NodeWarStatus.Scheduled)
                    {
                        var signups = repository.GetGuildSignupsForWar(dbWar.Id);
                        foreach (var signup in signups)
                        {
                            var guildSignup = new GuildNodeWarSignup(signup.GuildUid, signup.GuildName, dbWar.NodeId, signup.SignupByPlayerUid)
                            {
                                SignupTime = DateTimeOffset.FromUnixTimeSeconds(signup.SignupTime).UtcDateTime,
                                MembersOnlineAtSignup = signup.MembersOnline,
                                TotalMembersAtSignup = signup.TotalMembers,
                                IsConfirmed = signup.IsConfirmed
                            };

                            war.GuildSignups[signup.GuildUid] = guildSignup;
                            signupsLoaded++;
                        }
                    }

                    // Load guild progress for active wars
                    if (status == NodeWarStatus.Active)
                    {
                        var progressList = repository.GetGuildProgressForWar(dbWar.Id);
                        foreach (var prog in progressList)
                        {
                            var guildProgress = new GuildWarProgress(prog.GuildUid, prog.GuildName)
                            {
                                CapturePoints = prog.CapturePoints,
                                PlayersInZone = prog.PlayersInZone,
                                Kills = prog.Kills,
                                Deaths = prog.Deaths,
                                LastUpdateTime = DateTimeOffset.FromUnixTimeSeconds(prog.LastUpdate).UtcDateTime
                            };

                            war.GuildProgress[prog.GuildUid] = guildProgress;
                            progressLoaded++;
                        }
                    }

                    // Add to active wars
                    activeWars[dbWar.NodeId] = war;
                    warIdMapping[dbWar.NodeId] = dbWar.Id;
                    loadedCount++;

                    sapi.Logger.Debug($"[NodeWars] Loaded war {dbWar.Id} for node '{dbWar.NodeId}' (status: {status})");
                }

                sapi.Logger.Notification($"[NodeWars] Loaded {loadedCount} war(s), {signupsLoaded} signup(s), and {progressLoaded} progress record(s) from database");
            }
            catch (Exception ex)
            {
                sapi.Logger.Error($"[NodeWars] Failed to load wars from database: {ex.Message}");
            }
        }

        #region Node Zone Management

        /// <summary>
        /// Register a node zone for node wars
        /// </summary>
        public void RegisterNode(NodeZone node)
        {
            if (nodes.ContainsKey(node.NodeId))
            {
                sapi.Logger.Warning($"[NodeWars] Node {node.NodeId} already registered, updating...");
            }

            nodes[node.NodeId] = node;
            sapi.Logger.Notification($"[NodeWars] Registered node: {node.NodeName} ({node.NodeId})");

            // Persist to database if NodeManager is available
            if (nodeManager != null)
            {
                var dbNode = nodeManager.GetNode(node.NodeId);
                if (dbNode == null)
                {
                    // Add new node to database
                    var result = nodeManager.AddNode(
                        node.NodeId,
                        (int)node.Center.X,
                        (int)node.Center.Z,
                        node.Radius
                    );
                    if (result == null)
                    {
                        sapi.Logger.Error($"[NodeWars] Failed to save node {node.NodeId} to database");
                    }
                    else
                    {
                        sapi.Logger.Notification($"[NodeWars] Saved node {node.NodeId} to database");
                    }
                }
                else
                {
                    // Update existing node in database
                    bool updated = nodeManager.UpdateNode(
                        node.NodeId,
                        null,
                        (int)node.Center.X,
                        (int)node.Center.Z,
                        node.Radius
                    );
                    if (updated)
                    {
                        sapi.Logger.Notification($"[NodeWars] Updated node {node.NodeId} in database");
                    }
                    else
                    {
                        sapi.Logger.Error($"[NodeWars] Failed to update node {node.NodeId} in database");
                    }
                }
            }
            else
            {
                sapi.Logger.Warning($"[NodeWars] NodeManager not available - node {node.NodeId} not persisted to database");
            }
        }

        /// <summary>
        /// Remove a node zone from the system
        /// </summary>
        public void UnregisterNode(string nodeId)
        {
            if (nodes.Remove(nodeId))
            {
                sapi.Logger.Notification($"[NodeWars] Unregistered node: {nodeId}");

                // Remove from database if NodeManager is available
                if (nodeManager != null)
                {
                    bool removed = nodeManager.RemoveNode(nodeId);
                    if (removed)
                    {
                        sapi.Logger.Notification($"[NodeWars] Removed node {nodeId} from database");
                    }
                    else
                    {
                        sapi.Logger.Warning($"[NodeWars] Failed to remove node {nodeId} from database");
                    }
                }
            }
        }

        /// <summary>
        /// Get a node zone by ID
        /// </summary>
        public NodeZone? GetNode(string nodeId)
        {
            return nodes.TryGetValue(nodeId, out var node) ? node : null;
        }

        /// <summary>
        /// Get all registered nodes
        /// </summary>
        public List<NodeZone> GetAllNodes()
        {
            return nodes.Values.ToList();
        }

        /// <summary>
        /// Add a capture zone to a node and persist to database
        /// </summary>
        public bool AddCaptureZone(string nodeId, CaptureZone zone)
        {
            var node = GetNode(nodeId);
            if (node == null)
            {
                sapi.Logger.Warning($"[NodeWars] Cannot add capture zone: Node {nodeId} not found");
                return false;
            }

            // Add to node's capture zones
            node.CaptureZones[zone.ZoneId] = zone;

            // Persist to database if NodeManager is available
            if (nodeManager != null)
            {
                int captureZoneId = nodeManager.AddCaptureZone(
                    nodeId,
                    zone.ZoneId,
                    zone.ZoneName,
                    zone.Center.X,
                    zone.Center.Y,
                    zone.Center.Z,
                    zone.Radius,
                    zone.PointMultiplier,
                    zone.IsActive,
                    zone.Description
                );

                if (captureZoneId > 0)
                {
                    sapi.Logger.Notification($"[NodeWars] Added and saved capture zone '{zone.ZoneName}' ({zone.ZoneId}) to node {nodeId}");
                    return true;
                }
                else
                {
                    // Rollback in-memory change if database save failed
                    node.CaptureZones.Remove(zone.ZoneId);
                    sapi.Logger.Error($"[NodeWars] Failed to save capture zone {zone.ZoneId} to database");
                    return false;
                }
            }
            else
            {
                sapi.Logger.Warning($"[NodeWars] NodeManager not available - capture zone {zone.ZoneId} not persisted to database");
                return true; // Return true since in-memory addition succeeded
            }
        }

        /// <summary>
        /// Remove a capture zone from a node and delete from database
        /// </summary>
        public bool RemoveCaptureZone(string nodeId, string zoneId)
        {
            var node = GetNode(nodeId);
            if (node == null)
            {
                sapi.Logger.Warning($"[NodeWars] Cannot remove capture zone: Node {nodeId} not found");
                return false;
            }

            if (!node.CaptureZones.ContainsKey(zoneId))
            {
                sapi.Logger.Warning($"[NodeWars] Capture zone {zoneId} not found in node {nodeId}");
                return false;
            }

            // Remove from database if NodeManager is available
            if (nodeManager != null)
            {
                bool removed = nodeManager.RemoveCaptureZone(nodeId, zoneId);
                if (removed)
                {
                    node.CaptureZones.Remove(zoneId);
                    sapi.Logger.Notification($"[NodeWars] Removed capture zone {zoneId} from node {nodeId}");
                    return true;
                }
                else
                {
                    sapi.Logger.Error($"[NodeWars] Failed to remove capture zone {zoneId} from database");
                    return false;
                }
            }
            else
            {
                // Just remove from memory if no database available
                node.CaptureZones.Remove(zoneId);
                sapi.Logger.Warning($"[NodeWars] NodeManager not available - capture zone {zoneId} removed from memory only");
                return true;
            }
        }

        /// <summary>
        /// Find which node zone a position is in (if any)
        /// </summary>
        public NodeZone? GetNodeAtPosition(Vec3d position)
        {
            foreach (var node in nodes.Values)
            {
                if (node.IsPositionInZone(position))
                {
                    return node;
                }
            }
            return null;
        }

        #endregion

        #region Node War Lifecycle

        /// <summary>
        /// Schedule a node war to start at a specific time
        /// </summary>
        public bool ScheduleNodeWar(string nodeId, DateTime startTime, NodeWarConfig? config = null)
        {
            if (!nodes.TryGetValue(nodeId, out var node))
            {
                sapi.Logger.Warning($"[NodeWars] Cannot schedule war: Node {nodeId} not found");
                return false;
            }

            if (!node.IsActive)
            {
                sapi.Logger.Warning($"[NodeWars] Cannot schedule war: Node {nodeId} is not active");
                return false;
            }

            if (activeWars.ContainsKey(nodeId))
            {
                sapi.Logger.Warning($"[NodeWars] Cannot schedule war: Node {nodeId} already has an active or scheduled war");
                return false;
            }

            var war = new NodeWar(nodeId, node.Center, node.Radius, config ?? new NodeWarConfig())
            {
                StartTime = startTime,
                Status = NodeWarStatus.Scheduled,
                PreviousControllingGuildUid = node.OwningGuildUid
            };

            activeWars[nodeId] = war;
            sapi.Logger.Notification($"[NodeWars] Scheduled node war for {node.NodeName} at {startTime:yyyy-MM-dd HH:mm:ss} UTC");

            // Persist to database
            if (nodeManager != null)
            {
                var repository = nodeManager.GetNodeRepository();
                if (repository != null)
                {
                    int warId = repository.CreateNodeWar(
                        nodeId,
                        war.Status.ToString(),
                        new DateTimeOffset(war.StartTime).ToUnixTimeSeconds(),
                        null, // no end time yet
                        null, // no signup deadline for this method
                        war.MaxGuilds,
                        war.Config.CapturePointsNeeded,
                        null, // no controlling guild yet
                        null,
                        war.PreviousControllingGuildUid
                    );

                    if (warId > 0)
                    {
                        warIdMapping[nodeId] = warId;
                        sapi.Logger.Debug($"[NodeWars] Saved war to database (ID: {warId})");
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Open a node for guild signups with optional signup deadline and war start time
        /// </summary>
        public bool OpenNodeForSignups(string nodeId, DateTime? signupDeadline = null, DateTime? warStartTime = null, NodeWarConfig? config = null)
        {
            if (!nodes.TryGetValue(nodeId, out var node))
            {
                sapi.Logger.Warning($"[NodeWars] Cannot open signups: Node {nodeId} not found");
                return false;
            }

            if (!node.IsActive)
            {
                sapi.Logger.Warning($"[NodeWars] Cannot open signups: Node {nodeId} is not active");
                return false;
            }

            if (activeWars.ContainsKey(nodeId))
            {
                var existingWar = activeWars[nodeId];
                if (existingWar.Status != NodeWarStatus.Scheduled)
                {
                    sapi.Logger.Warning($"[NodeWars] Cannot open signups: Node {nodeId} already has an active or completed war");
                    return false;
                }

                // Update existing scheduled war
                if (signupDeadline.HasValue)
                {
                    existingWar.SignupDeadline = signupDeadline.Value;
                }
                if (warStartTime.HasValue)
                {
                    existingWar.StartTime = warStartTime.Value;
                }

                sapi.Logger.Notification($"[NodeWars] Updated signup window for {node.NodeName}");
                return true;
            }

            // Calculate default times if not provided
            DateTime currentTime = DateTime.UtcNow;
            DateTime startTime = warStartTime ?? currentTime.AddHours(24); // Default: 24 hours from now
            DateTime deadline = signupDeadline ?? startTime.AddHours(-1); // Default: 1 hour before start

            var war = new NodeWar(nodeId, node.Center, node.Radius, config ?? new NodeWarConfig())
            {
                StartTime = startTime,
                SignupDeadline = deadline,
                Status = NodeWarStatus.Scheduled,
                PreviousControllingGuildUid = node.OwningGuildUid
            };

            activeWars[nodeId] = war;
            sapi.Logger.Notification($"[NodeWars] Opened {node.NodeName} for guild signups (deadline: {deadline:yyyy-MM-dd HH:mm:ss} UTC, war starts: {startTime:yyyy-MM-dd HH:mm:ss} UTC)");

            // Persist to database
            if (nodeManager != null)
            {
                var repository = nodeManager.GetNodeRepository();
                if (repository != null)
                {
                    int warId = repository.CreateNodeWar(
                        nodeId,
                        war.Status.ToString(),
                        new DateTimeOffset(war.StartTime).ToUnixTimeSeconds(),
                        null, // no end time yet
                        new DateTimeOffset(war.SignupDeadline.Value).ToUnixTimeSeconds(),
                        war.MaxGuilds,
                        war.Config.CapturePointsNeeded,
                        null, // no controlling guild yet
                        null,
                        war.PreviousControllingGuildUid
                    );

                    if (warId > 0)
                    {
                        warIdMapping[nodeId] = warId;
                        sapi.Logger.Debug($"[NodeWars] Saved war to database (ID: {warId})");
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Start a node war immediately or transition from scheduled to active
        /// </summary>
        public bool StartNodeWar(string nodeId)
        {
            if (!activeWars.TryGetValue(nodeId, out var war))
            {
                sapi.Logger.Warning($"[NodeWars] Cannot start war: No war found for node {nodeId}");
                return false;
            }

            if (war.Status == NodeWarStatus.Active)
            {
                sapi.Logger.Warning($"[NodeWars] Node war for {nodeId} is already active");
                return false;
            }

            war.Status = NodeWarStatus.Active;
            war.StartTime = DateTime.UtcNow;
            sapi.Logger.Notification($"[NodeWars] Started node war for {nodeId}");

            // Persist to database
            if (nodeManager != null)
            {
                var repository = nodeManager.GetNodeRepository();
                if (repository != null)
                {
                    // Get or create war ID
                    if (!warIdMapping.TryGetValue(nodeId, out int warId))
                    {
                        // Create new war in database
                        warId = repository.CreateNodeWar(
                            nodeId,
                            war.Status.ToString(),
                            new DateTimeOffset(war.StartTime).ToUnixTimeSeconds(),
                            war.EndTime.HasValue ? new DateTimeOffset(war.EndTime.Value).ToUnixTimeSeconds() : null,
                            war.SignupDeadline.HasValue ? new DateTimeOffset(war.SignupDeadline.Value).ToUnixTimeSeconds() : null,
                            war.MaxGuilds,
                            war.Config.CapturePointsNeeded,
                            war.ControllingGuildUid,
                            null, // controlling guild name - will be set when war completes
                            war.PreviousControllingGuildUid
                        );

                        if (warId > 0)
                        {
                            warIdMapping[nodeId] = warId;
                            sapi.Logger.Debug($"[NodeWars] Created war record in database (ID: {warId})");
                        }
                    }
                    else
                    {
                        // Update existing war
                        bool success = repository.UpdateNodeWar(
                            warId,
                            war.Status.ToString(),
                            new DateTimeOffset(war.StartTime).ToUnixTimeSeconds(),
                            null, // don't update end time yet
                            null, // don't update controlling guild yet
                            null
                        );

                        if (success)
                        {
                            sapi.Logger.Debug($"[NodeWars] Updated war status to Active in database (ID: {warId})");
                        }
                        else
                        {
                            sapi.Logger.Warning($"[NodeWars] Failed to update war status in database (ID: {warId})");
                        }
                    }
                }
                else
                {
                    sapi.Logger.Warning("[NodeWars] NodeRepository not available - war not persisted to database");
                }
            }
            else
            {
                sapi.Logger.Warning("[NodeWars] NodeManager not available - war not persisted to database");
            }

            // Register zone monitoring for automatic hologram management
            RegisterWarZoneMonitoring(nodeId);

            // Auto-enroll all online players from signed-up guilds
            EnrollOnlinePlayersFromSignedUpGuilds(nodeId);

            // TODO: Broadcast to all online players

            return true;
        }

        /// <summary>
        /// End a node war with an optional winner
        /// </summary>
        public void EndNodeWar(string nodeId, string? winnerGuildUid = null)
        {
            if (!activeWars.TryGetValue(nodeId, out var war))
            {
                return;
            }

            war.Status = NodeWarStatus.Completed;
            war.EndTime = DateTime.UtcNow;
            war.ControllingGuildUid = winnerGuildUid;

            string? winnerGuildName = null;

            // Update node ownership
            if (nodes.TryGetValue(nodeId, out var node))
            {
                if (winnerGuildUid != null && war.GuildProgress.TryGetValue(winnerGuildUid, out var winner))
                {
                    node.OwningGuildUid = winnerGuildUid;
                    node.OwningGuildName = winner.GuildName;
                    node.LastCapturedTime = DateTime.UtcNow;
                    winnerGuildName = winner.GuildName;

                    sapi.Logger.Notification($"[NodeWars] {winner.GuildName} captured {node.NodeName}!");
                    // TODO: Broadcast victory message
                }
                else
                {
                    sapi.Logger.Notification($"[NodeWars] Node war for {node.NodeName} ended with no winner");
                    // TODO: Broadcast draw/cancellation message
                }
            }

            // Persist to database
            if (nodeManager != null && warIdMapping.TryGetValue(nodeId, out int warId))
            {
                var repository = nodeManager.GetNodeRepository();
                if (repository != null)
                {
                    bool success = repository.UpdateNodeWar(
                        warId,
                        war.Status.ToString(),
                        null, // don't update start time
                        new DateTimeOffset(war.EndTime.Value).ToUnixTimeSeconds(),
                        winnerGuildUid,
                        winnerGuildName
                    );

                    if (success)
                    {
                        sapi.Logger.Debug($"[NodeWars] Updated war end status in database (ID: {warId})");
                    }
                }
            }

            // Remove all participants from this war
            var participantsToRemove = participants.Values
                .Where(p => p.CurrentNodeWarId == nodeId)
                .ToList();

            foreach (var participant in participantsToRemove)
            {
                participant.IsParticipating = false;
                participant.CurrentNodeWarId = null;
            }

            // Cleanup zone monitoring and disable holograms
            CleanupWarZoneMonitoring(nodeId);

            // Remove the war from active wars after a delay (for history/statistics)
            // For now, remove immediately
            activeWars.Remove(nodeId);
        }

        /// <summary>
        /// Cancel a node war
        /// </summary>
        public void CancelNodeWar(string nodeId)
        {
            if (activeWars.TryGetValue(nodeId, out var war))
            {
                war.Status = NodeWarStatus.Cancelled;

                // Persist cancellation to database before calling EndNodeWar
                if (nodeManager != null && warIdMapping.TryGetValue(nodeId, out int warId))
                {
                    var repository = nodeManager.GetNodeRepository();
                    if (repository != null)
                    {
                        bool success = repository.UpdateNodeWar(
                            warId,
                            war.Status.ToString(),
                            null, // don't update start time
                            new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds(),
                            null, // no winner
                            null
                        );

                        if (success)
                        {
                            sapi.Logger.Debug($"[NodeWars] Updated war status to Cancelled in database (ID: {warId})");
                        }
                    }
                }

                EndNodeWar(nodeId, null);
                sapi.Logger.Notification($"[NodeWars] Cancelled node war for {nodeId}");
            }
        }

        #endregion

        #region Enrollment and Participant Management

        /// <summary>
        /// Automatically enroll all online players from guilds that signed up for the war
        /// Called when a war transitions to Active status
        /// </summary>
        private void EnrollOnlinePlayersFromSignedUpGuilds(string nodeId)
        {
            if (!activeWars.TryGetValue(nodeId, out var war))
            {
                sapi.Logger.Warning($"[NodeWars] Cannot enroll players: War not found for node {nodeId}");
                return;
            }

            if (war.GuildSignups.Count == 0)
            {
                sapi.Logger.Warning($"[NodeWars] No guilds signed up for war at node {nodeId}");
                return;
            }

            var guildMod = sapi.ModLoader.GetModSystem<SOAGuildsAndKingdoms.SOAGuildsAndKingdomsModSystem>();
            if (guildMod == null)
            {
                sapi.Logger.Error($"[NodeWars] Cannot enroll players: GuildModSystem not found");
                return;
            }

            var guildManager = guildMod.GetGuildManager();
            if (guildManager == null)
            {
                sapi.Logger.Error($"[NodeWars] Cannot enroll players: GuildManager not found");
                return;
            }

            int enrolledCount = 0;
            int totalAttempted = 0;

            // Enroll all online players from each signed-up guild
            foreach (var signup in war.GuildSignups.Values)
            {
                var guild = guildManager.GetGuild(signup.GuildName);
                if (guild == null)
                {
                    sapi.Logger.Warning($"[NodeWars] Guild '{signup.GuildName}' not found for enrollment");
                    continue;
                }

                // Get all online players from this guild
                var guildPlayers = sapi.World.AllOnlinePlayers
                    .Where(p => guildManager.GetGuildByMember(p.PlayerUID)?.Name == signup.GuildName)
                    .ToList();

                foreach (var player in guildPlayers)
                {
                    totalAttempted++;
                    bool enrolled = JoinNodeWar(player.PlayerUID, player.PlayerName, signup.GuildUid, nodeId);
                    if (enrolled)
                    {
                        enrolledCount++;
                        sapi.Logger.Debug($"[NodeWars] Auto-enrolled {player.PlayerName} from guild '{signup.GuildName}'");

                        // TODO: Send notification to player about auto-enrollment
                        // player.SendMessage(groupId, $"You have been enrolled in the node war at {node.NodeName}!", EnumChatType.Notification);
                    }
                }
            }

            sapi.Logger.Notification($"[NodeWars] Auto-enrolled {enrolledCount}/{totalAttempted} online players for war at node {nodeId}");
        }

        /// <summary>
        /// Enroll a late-joining player in an active node war
        /// Validates that the player's guild is signed up for the war
        /// </summary>
        /// <param name="playerUid">Player UID</param>
        /// <param name="playerName">Player display name</param>
        /// <param name="nodeId">Node war ID to enroll in</param>
        /// <returns>Enrollment result with success/failure and message</returns>
        public EnrollmentResult EnrollPlayerInNodeWar(string playerUid, string playerName, string nodeId)
        {
            // Validate war exists and is active
            if (!activeWars.TryGetValue(nodeId, out var war))
            {
                return EnrollmentResult.Failed("Node war not found", EnrollmentFailureReason.WarNotFound);
            }

            if (war.Status != NodeWarStatus.Active)
            {
                return EnrollmentResult.Failed("Node war is not currently active", EnrollmentFailureReason.WarNotActive);
            }

            // Check if player is already enrolled
            if (participants.TryGetValue(playerUid, out var existingParticipant) && 
                existingParticipant.IsParticipating && 
                existingParticipant.CurrentNodeWarId == nodeId)
            {
                return EnrollmentResult.Failed("You are already enrolled in this node war", EnrollmentFailureReason.AlreadyEnrolled);
            }

            // Get player's guild
            var guildMod = sapi.ModLoader.GetModSystem<SOAGuildsAndKingdoms.SOAGuildsAndKingdomsModSystem>();
            if (guildMod == null)
            {
                return EnrollmentResult.Failed("Guild system not available", EnrollmentFailureReason.SystemError);
            }

            var guildManager = guildMod.GetGuildManager();
            if (guildManager == null)
            {
                return EnrollmentResult.Failed("Guild manager not available", EnrollmentFailureReason.SystemError);
            }

            var playerGuild = guildManager.GetGuildByMember(playerUid);
            if (playerGuild == null)
            {
                return EnrollmentResult.Failed("You must be in a guild to participate in node wars", EnrollmentFailureReason.NoGuild);
            }

            // Check if player's guild is signed up for this war
            if (!war.IsGuildSignedUp(playerGuild.Name))
            {
                return EnrollmentResult.Failed($"Your guild '{playerGuild.Name}' is not signed up for this node war", EnrollmentFailureReason.GuildNotSignedUp);
            }

            // Check if player is already in a different node war
            if (existingParticipant != null && 
                existingParticipant.IsParticipating && 
                existingParticipant.CurrentNodeWarId != null && 
                existingParticipant.CurrentNodeWarId != nodeId)
            {
                var otherNode = GetNode(existingParticipant.CurrentNodeWarId);
                return EnrollmentResult.Failed(
                    $"You are already enrolled in a node war at '{otherNode?.NodeName ?? "Unknown"}'", 
                    EnrollmentFailureReason.InAnotherWar);
            }

            // Enroll the player
            bool enrolled = JoinNodeWar(playerUid, playerName, playerGuild.Name, nodeId);
            if (!enrolled)
            {
                return EnrollmentResult.Failed("Failed to enroll in node war", EnrollmentFailureReason.SystemError);
            }

            var node = GetNode(nodeId);
            sapi.Logger.Notification($"[NodeWars] Player {playerName} enrolled in node war at {node?.NodeName ?? nodeId}");

            return EnrollmentResult.Succeeded($"Successfully enrolled in the node war at '{node?.NodeName ?? nodeId}'!");
        }

        #endregion

        #region Participant Management (Legacy)

        /// <summary>
        /// Add a player to a node war
        /// </summary>
        public bool JoinNodeWar(string playerUid, string playerName, string guildUid, string nodeId)
        {
            if (!activeWars.TryGetValue(nodeId, out var war))
            {
                return false;
            }

            if (war.Status != NodeWarStatus.Active)
            {
                return false;
            }

            // Check if player is already in a different node war
            if (participants.TryGetValue(playerUid, out var existingParticipant))
            {
                if (existingParticipant.IsParticipating && existingParticipant.CurrentNodeWarId != nodeId)
                {
                    return false; // Already in another war
                }
            }

            var participant = new NodeWarParticipant(playerUid, playerName, guildUid, nodeId)
            {
                JoinTime = DateTime.UtcNow
            };

            participants[playerUid] = participant;

            // Initialize guild progress if not exists
            if (!war.GuildProgress.ContainsKey(guildUid))
            {
                var guildName = GetGuildName(guildUid); // TODO: Implement or inject guild manager
                war.GuildProgress[guildUid] = new GuildWarProgress(guildUid, guildName)
                {
                    LastUpdateTime = DateTime.UtcNow
                };
            }

            sapi.Logger.Debug($"[NodeWars] {playerName} joined node war at {nodeId} for guild {guildUid}");

            return true;
        }

        /// <summary>
        /// Remove a player from their current node war
        /// </summary>
        public void LeaveNodeWar(string playerUid)
        {
            if (participants.TryGetValue(playerUid, out var participant))
            {
                participant.IsParticipating = false;
                participant.CurrentNodeWarId = null;

                sapi.Logger.Debug($"[NodeWars] {participant.PlayerName} left node war");
            }
        }

        /// <summary>
        /// Check if a player is participating in any node war
        /// </summary>
        public bool IsPlayerInNodeWar(string playerUid)
        {
            return participants.TryGetValue(playerUid, out var participant) &&
                   participant.IsParticipating &&
                   participant.CurrentNodeWarId != null;
        }

        /// <summary>
        /// Get participant data for a player
        /// </summary>
        public NodeWarParticipant? GetParticipant(string playerUid)
        {
            return participants.TryGetValue(playerUid, out var participant) ? participant : null;
        }

        #endregion

        #region Guild Signup Management

        /// <summary>
        /// Sign up a guild for a node war
        /// </summary>
        public GuildSignupResult SignupGuild(string guildUid, string guildName, string nodeId, string playerUid, int membersOnline, int totalMembers)
        {
            DateTime currentTime = DateTime.UtcNow;

            // Validate node exists
            if (!nodes.TryGetValue(nodeId, out var node))
            {
                return GuildSignupResult.Failed("Node not found", GuildSignupFailureReason.NodeNotFound);
            }

            // Check if node is active
            if (!node.IsActive)
            {
                return GuildSignupResult.Failed($"Node '{node.NodeName}' is not active for wars", GuildSignupFailureReason.NodeNotActive);
            }

            // Get the war for this node
            if (!activeWars.TryGetValue(nodeId, out var war))
            {
                return GuildSignupResult.Failed($"No war scheduled for node '{node.NodeName}'", GuildSignupFailureReason.WarNotScheduled);
            }

            // Check if signup is still open
            if (!war.IsSignupOpen(currentTime))
            {
                if (war.Status != NodeWarStatus.Scheduled)
                {
                    return GuildSignupResult.Failed("War has already started or ended", GuildSignupFailureReason.WarAlreadyActive);
                }
                if (war.SignupDeadline.HasValue && currentTime >= war.SignupDeadline.Value)
                {
                    return GuildSignupResult.Failed("Signup period has closed", GuildSignupFailureReason.SignupPeriodClosed);
                }
                if (war.MaxGuilds > 0 && war.GuildSignups.Count >= war.MaxGuilds)
                {
                    return GuildSignupResult.Failed($"Maximum number of guilds ({war.MaxGuilds}) already signed up", GuildSignupFailureReason.MaxGuildsReached);
                }
            }

            // Check if guild is already signed up for this war
            if (war.IsGuildSignedUp(guildUid))
            {
                return GuildSignupResult.Failed($"Your guild is already signed up for the war at '{node.NodeName}'", GuildSignupFailureReason.AlreadySignedUp);
            }

            // Check if guild is signed up for another war
            foreach (var otherWar in activeWars.Values)
            {
                if (otherWar.NodeId != nodeId && otherWar.IsGuildSignedUp(guildUid) && otherWar.Status == NodeWarStatus.Scheduled)
                {
                    var otherNode = GetNode(otherWar.NodeId);
                    return GuildSignupResult.Failed($"Your guild is already signed up for a war at '{otherNode?.NodeName ?? "Unknown"}'", GuildSignupFailureReason.SignedUpForAnotherWar);
                }
            }

            // Create the signup
            var signup = new GuildNodeWarSignup(guildUid, guildName, nodeId, playerUid)
            {
                SignupTime = currentTime,
                MembersOnlineAtSignup = membersOnline,
                TotalMembersAtSignup = totalMembers,
                IsConfirmed = true
            };

            war.GuildSignups[guildUid] = signup;

            // Persist signup to database
            if (nodeManager != null && warIdMapping.TryGetValue(nodeId, out int warId))
            {
                var repository = nodeManager.GetNodeRepository();
                if (repository != null)
                {
                    int signupId = repository.AddGuildSignup(
                        warId,
                        guildUid,
                        guildName,
                        playerUid,
                        new DateTimeOffset(currentTime).ToUnixTimeSeconds(),
                        membersOnline,
                        totalMembers,
                        true
                    );

                    if (signupId > 0)
                    {
                        sapi.Logger.Debug($"[NodeWars] Saved guild signup to database (ID: {signupId})");
                    }
                    else
                    {
                        sapi.Logger.Warning($"[NodeWars] Failed to save guild signup to database");
                    }
                }
            }

            sapi.Logger.Notification($"[NodeWars] Guild '{guildName}' signed up for war at '{node.NodeName}' (by {playerUid})");

            return GuildSignupResult.Succeeded($"Successfully signed up for the war at '{node.NodeName}'!");
        }

        /// <summary>
        /// Cancel a guild's signup for a node war
        /// </summary>
        public GuildSignupResult CancelGuildSignup(string guildUid, string nodeId)
        {
            if (!activeWars.TryGetValue(nodeId, out var war))
            {
                return GuildSignupResult.Failed("War not found", GuildSignupFailureReason.WarNotScheduled);
            }

            if (war.Status != NodeWarStatus.Scheduled)
            {
                return GuildSignupResult.Failed("Cannot cancel signup - war has already started", GuildSignupFailureReason.WarAlreadyActive);
            }

            if (!war.GuildSignups.ContainsKey(guildUid))
            {
                return GuildSignupResult.Failed("Your guild is not signed up for this war", GuildSignupFailureReason.AlreadySignedUp);
            }

            war.GuildSignups.Remove(guildUid);

            // Remove from database
            if (nodeManager != null && warIdMapping.TryGetValue(nodeId, out int warId))
            {
                var repository = nodeManager.GetNodeRepository();
                if (repository != null)
                {
                    bool removed = repository.RemoveGuildSignup(warId, guildUid);
                    if (removed)
                    {
                        sapi.Logger.Debug($"[NodeWars] Removed guild signup from database");
                    }
                    else
                    {
                        sapi.Logger.Warning($"[NodeWars] Failed to remove guild signup from database");
                    }
                }
            }

            var node = GetNode(nodeId);
            sapi.Logger.Notification($"[NodeWars] Guild {guildUid} cancelled signup for war at '{node?.NodeName ?? nodeId}'");

            return GuildSignupResult.Succeeded($"Successfully cancelled signup for '{node?.NodeName ?? nodeId}'");
        }

        /// <summary>
        /// Get all guilds signed up for a specific node war
        /// </summary>
        public List<GuildNodeWarSignup> GetSignedUpGuilds(string nodeId)
        {
            if (activeWars.TryGetValue(nodeId, out var war))
            {
                return war.GuildSignups.Values.ToList();
            }
            return new List<GuildNodeWarSignup>();
        }

        /// <summary>
        /// Check if a guild is signed up for any node war
        /// </summary>
        public bool IsGuildSignedUpForAnyWar(string guildUid)
        {
            return activeWars.Values.Any(war => war.IsGuildSignedUp(guildUid) && war.Status == NodeWarStatus.Scheduled);
        }

        /// <summary>
        /// Get the node war a guild is signed up for (if any)
        /// </summary>
        public NodeWar? GetGuildSignedUpWar(string guildUid)
        {
            return activeWars.Values.FirstOrDefault(war => war.IsGuildSignedUp(guildUid) && war.Status == NodeWarStatus.Scheduled);
        }

        /// <summary>
        /// Validate if a guild meets requirements to sign up
        /// </summary>
        public GuildSignupResult ValidateGuildSignup(string guildUid, int membersOnline, int totalMembers, int minMembersRequired, int minOnlineRequired)
        {
            if (totalMembers < minMembersRequired)
            {
                return GuildSignupResult.Failed($"Guild must have at least {minMembersRequired} members (currently: {totalMembers})", GuildSignupFailureReason.GuildTooSmall);
            }

            if (membersOnline < minOnlineRequired)
            {
                return GuildSignupResult.Failed($"At least {minOnlineRequired} guild members must be online (currently: {membersOnline})", GuildSignupFailureReason.InsufficientMembersOnline);
            }

            return GuildSignupResult.Succeeded();
        }

        #endregion

        #region Progress Tracking

		/// <summary>
		/// Update capture progress for all active node wars
		/// This is called by the CaptureZoneSystem automatically
		/// </summary>
		/// <param name="nodeId">The node war to update</param>
		/// <param name="deltaTime">Time elapsed since last update in milliseconds</param>
		/// <param name="guildZoneMultipliers">Dictionary of guild UID to average zone multiplier (from capture zones)</param>
		public void UpdateCaptureProgress(string nodeId, double deltaTime, Dictionary<string, double>? guildZoneMultipliers = null)
		{
			if (!activeWars.TryGetValue(nodeId, out var war))
			{
				return;
			}

			if (war.Status != NodeWarStatus.Active)
			{
				return;
			}

			DateTime currentTime = DateTime.UtcNow;

			// Check for time limit
			if (war.Config.MaxDurationSeconds > 0)
			{
				TimeSpan elapsed = currentTime - war.StartTime;
				if (elapsed.TotalSeconds >= war.Config.MaxDurationSeconds)
				{
					HandleTimeExpired(war);
					return;
				}
			}

			bool isContested = war.IsContested();

			foreach (var (guildUid, progress) in war.GuildProgress)
			{
				// Check if guild has minimum players in capture zones
				if (progress.PlayersInZone < war.Config.MinPlayersToCapture)
				//if (progress.PlayersInZone < 1)
				{
					progress.FirstCaptureTime = null;
					continue;
				}

				// Track first time meeting requirements (for overtime)
				if (progress.FirstCaptureTime == null)
				{
					progress.FirstCaptureTime = currentTime;
				}

				// Calculate base points per second
				double pointsGained = war.Config.PointsPerSecondBase * (deltaTime);

				// Bonus for extra players (diminishing returns)
				//int extraPlayers = progress.PlayersInZone - 0;
				int extraPlayers = progress.PlayersInZone - war.Config.MinPlayersToCapture;
				pointsGained *= (1.0 + (extraPlayers * war.Config.ExtraPlayerBonus));

				// Apply contested multiplier
				if (isContested && war.Config.AllowContesting)
				{
					pointsGained *= war.Config.ContestedMultiplier;
				}

				// Apply capture zone multiplier (from zone PointMultiplier property)
				if (guildZoneMultipliers != null && guildZoneMultipliers.TryGetValue(guildUid, out double zoneMultiplier))
				{
					pointsGained *= zoneMultiplier;
					sapi.Logger.Debug($"[NodeWars] Guild {guildUid} gaining {pointsGained:F2} points/sec (zone multiplier: {zoneMultiplier:F2}x)");
				}

				progress.CapturePoints += pointsGained;
				progress.LastUpdateTime = currentTime;

				// Persist progress to database (for active wars)
				if (nodeManager != null && warIdMapping.TryGetValue(nodeId, out int warId))
				{
					var repository = nodeManager.GetNodeRepository();
					if (repository != null)
					{
						repository.SaveGuildWarProgress(
							warId,
							guildUid,
							progress.GuildName,
							progress.CapturePoints,
							progress.PlayersInZone,
							progress.Kills,
							progress.Deaths
						);
					}
				}

				// Check for victory
				if (progress.CapturePoints >= war.Config.CapturePointsNeeded)
				{
					EndNodeWar(nodeId, guildUid);
					return;
				}
			}
		}

        /// <summary>
        /// Get the capture zone status for a node (if active)
        /// </summary>
        public CaptureZoneStatus? GetCaptureZoneStatus(string nodeId)
        {
            return captureZoneSystem?.GetCaptureZoneStatus(nodeId);
        }

        /// <summary>
        /// Update player counts in zones for all active wars
        /// DEPRECATED: This is now handled automatically by CaptureZoneSystem
        /// Kept for backward compatibility
        /// </summary>
        [Obsolete("Use CaptureZoneSystem instead - this is handled automatically")]
        public void CheckPlayersInZone(string nodeId)
        {
            // This method is now handled by CaptureZoneSystem
            // Force a zone check if called manually
            captureZoneSystem?.ForceZoneCheck(nodeId);
        }

        /// <summary>
        /// Record a kill in a node war
        /// </summary>
        public void OnPlayerKillInNode(string killerUid, string victimUid, string nodeId)
        {
            if (!activeWars.TryGetValue(nodeId, out var war))
            {
                return;
            }

            var killerParticipant = GetParticipant(killerUid);
            var victimParticipant = GetParticipant(victimUid);

            bool progressChanged = false;

            if (killerParticipant?.IsParticipating == true && killerParticipant.CurrentNodeWarId == nodeId)
            {
                killerParticipant.Kills++;

                if (war.GuildProgress.TryGetValue(killerParticipant.GuildUid, out var killerGuild))
                {
                    killerGuild.Kills++;
                    killerGuild.CapturePoints += war.Config.PointsPerKill;
                    progressChanged = true;

                    // Persist progress to database
                    if (nodeManager != null && warIdMapping.TryGetValue(nodeId, out int warId))
                    {
                        var repository = nodeManager.GetNodeRepository();
                        repository?.SaveGuildWarProgress(
                            warId,
                            killerGuild.GuildUid,
                            killerGuild.GuildName,
                            killerGuild.CapturePoints,
                            killerGuild.PlayersInZone,
                            killerGuild.Kills,
                            killerGuild.Deaths
                        );
                    }
                }
            }

            if (victimParticipant?.IsParticipating == true && victimParticipant.CurrentNodeWarId == nodeId)
            {
                victimParticipant.Deaths++;

                if (war.GuildProgress.TryGetValue(victimParticipant.GuildUid, out var victimGuild))
                {
                    victimGuild.Deaths++;
                    victimGuild.CapturePoints += war.Config.PointsPerDeath; // Negative value
                    progressChanged = true;

                    // Persist progress to database
                    if (nodeManager != null && warIdMapping.TryGetValue(nodeId, out int warId))
                    {
                        var repository = nodeManager.GetNodeRepository();
                        repository?.SaveGuildWarProgress(
                            warId,
                            victimGuild.GuildUid,
                            victimGuild.GuildName,
                            victimGuild.CapturePoints,
                            victimGuild.PlayersInZone,
                            victimGuild.Kills,
                            victimGuild.Deaths
                        );
                    }
                }
            }
        }

        /// <summary>
        /// Check if a location is in an active node war
        /// </summary>
        public bool IsLocationInActiveNodeWar(Vec3d position, out NodeWar? nodeWar)
        {
            foreach (var war in activeWars.Values)
            {
                if (war.Status == NodeWarStatus.Active && war.IsPositionInZone(position))
                {
                    nodeWar = war;
                    return true;
                }
            }

            nodeWar = null;
            return false;
        }

        #endregion

        #region Query Methods

        /// <summary>
        /// Get an active node war by node ID
        /// </summary>
        public NodeWar? GetActiveNodeWar(string nodeId)
        {
            return activeWars.TryGetValue(nodeId, out var war) ? war : null;
        }

        /// <summary>
        /// Get all active node wars
        /// </summary>
        public List<NodeWar> GetAllActiveNodeWars()
        {
            return activeWars.Values.Where(w => w.Status == NodeWarStatus.Active).ToList();
        }

        /// <summary>
        /// Get all scheduled node wars (not yet started)
        /// </summary>
        public List<NodeWar> GetScheduledNodeWars()
        {
            return activeWars.Values.Where(w => w.Status == NodeWarStatus.Scheduled).ToList();
        }

        /// <summary>
        /// Get guild progress in a specific node war
        /// </summary>
        public GuildWarProgress? GetGuildProgress(string nodeId, string guildUid)
        {
            if (activeWars.TryGetValue(nodeId, out var war))
            {
                return war.GuildProgress.TryGetValue(guildUid, out var progress) ? progress : null;
            }
            return null;
        }

        #endregion

        #region Helper Methods

        private void HandleTimeExpired(NodeWar war)
        {
            if (!war.Config.EnableOvertime || !war.IsContested())
            {
                // No overtime or not contested, end with leader as winner
                var leader = war.GetLeadingGuild();
                EndNodeWar(war.NodeId, leader?.GuildUid);
                return;
            }

            // Enter overtime
            if (!war.IsOvertime)
            {
                war.IsOvertime = true;
                war.OvertimeStartTime = DateTime.UtcNow;
                sapi.Logger.Notification($"[NodeWars] Node war {war.NodeId} entered OVERTIME!");
                // TODO: Broadcast overtime message
            }
            else
            {
                // Check overtime victory conditions
                CheckOvertimeVictory(war);
            }
        }

        private void CheckOvertimeVictory(NodeWar war)
        {
            if (war.OvertimeStartTime == null)
            {
                return;
            }

            DateTime currentTime = DateTime.UtcNow;
            TimeSpan overtimeElapsed = currentTime - war.OvertimeStartTime.Value;

            // Find guild with minimum players
            GuildWarProgress? controllingGuild = null;
            int controllingCount = 0;

            foreach (var progress in war.GuildProgress.Values)
            {
                if (progress.PlayersInZone >= war.Config.MinPlayersToCapture)
                {
                    controllingCount++;
                    controllingGuild = progress;
                }
            }

            // If only one guild has minimum players for required duration, they win
            if (controllingCount == 1 && overtimeElapsed.TotalSeconds >= war.Config.OvertimeControlSeconds)
            {
                EndNodeWar(war.NodeId, controllingGuild?.GuildUid);
            }
            else if (controllingCount != 1)
            {
                // Reset overtime timer if contested again
                war.OvertimeStartTime = currentTime;
            }
        }

        private string GetGuildName(string guildUid)
        {
            // TODO: Integrate with actual GuildManager
            // For now, return UID as placeholder
            return guildUid;
        }

        #region Auto-Hologram Zone Tracking

        /// <summary>
        /// Set the network handler for sending hologram packets
        /// </summary>
        public void SetNetworkHandler(network.PVPNetworkHandler handler)
        {
            networkHandler = handler;
        }

        /// <summary>
        /// Register zone monitoring when a war starts
        /// Automatically enables hologram for players already in zone and tracks entry/exit
        /// </summary>
        private void RegisterWarZoneMonitoring(string nodeId)
        {
            if (!activeWars.TryGetValue(nodeId, out var war) || !nodes.TryGetValue(nodeId, out var node))
                return;

            // Check all participants who are already in the zone when war starts
            var participants = GetWarParticipants(nodeId);
            foreach (var playerUid in participants)
            {
                var player = sapi.World.PlayerByUid(playerUid) as IServerPlayer;
                if (player?.Entity == null) continue;

                var pos = PositionUtils.GetOffsetFromSpawn(sapi.World, player.Entity.Pos.XYZ);
                bool isInZone = IsPlayerInZone(pos, node.Center, node.Radius);
                if (isInZone)
                {
                    OnPlayerEnteredWarZone(player, nodeId);
                }
            }

            // Register periodic monitoring (every 2 seconds)
            long listenerId = sapi.Event.RegisterGameTickListener((dt) =>
            {
                CheckPlayersInWarZone(nodeId);
            }, 2000);

            activeWarMonitors[nodeId] = listenerId;
            sapi.Logger.Debug($"[NodeWars] Registered zone monitoring for war at {nodeId}");
        }

        /// <summary>
        /// Check all participants for entry/exit of war zone
        /// </summary>
        private void CheckPlayersInWarZone(string nodeId)
        {
            if (!activeWars.TryGetValue(nodeId, out var war) || !nodes.TryGetValue(nodeId, out var node))
                return;

            var participants = GetWarParticipants(nodeId);

            foreach (var playerUid in participants)
            {
                var player = sapi.World.PlayerByUid(playerUid) as IServerPlayer;
                if (player?.Entity == null) continue;

                var pos = PositionUtils.GetOffsetFromSpawn(sapi.World, player.Entity.Pos.XYZ);
				bool isInZone = IsPlayerInZone(pos, node.Center, node.Radius);
                bool wasInZone = playerZoneTracking.ContainsKey(playerUid) &&
                                 playerZoneTracking[playerUid].Contains(nodeId);

                if (isInZone && !wasInZone)
                {
                    OnPlayerEnteredWarZone(player, nodeId);
                }
                else if (!isInZone && wasInZone)
                {
                    OnPlayerLeftWarZone(player, nodeId);
                }
            }
        }

        /// <summary>
        /// Get all participating players for a war
        /// </summary>
        private HashSet<string> GetWarParticipants(string nodeId)
        {
            var result = new HashSet<string>();

            if (!activeWars.TryGetValue(nodeId, out var war))
                return result;

            // Get all players from signed up guilds
            foreach (var signup in war.GuildSignups.Values)
            {
                // Get all online players in this guild
                var guildMod = sapi.ModLoader.GetModSystem<SOAGuildsAndKingdoms.SOAGuildsAndKingdomsModSystem>();
                if (guildMod != null)
                {
                    var guildPlayers = sapi.World.AllOnlinePlayers
                        .Where(p => guildMod.GetGuildManager().GetGuildByMember(p.PlayerUID)?.Name == signup.GuildName)
                        .Select(p => p.PlayerUID);

                    foreach (var playerUid in guildPlayers)
                    {
                        result.Add(playerUid);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Check if player position is within a zone
        /// </summary>
        private bool IsPlayerInZone(Vec3d position, Vec3d nodeCenter, int radius)
        {
            double dx = position.X - nodeCenter.X;
            double dz = position.Z - nodeCenter.Z;
            double distanceSq = dx * dx + dz * dz;

            return distanceSq <= (radius * radius);
        }

        /// <summary>
        /// Handle player entering a war zone
        /// </summary>
        private void OnPlayerEnteredWarZone(IServerPlayer player, string nodeId)
        {
            if (!playerZoneTracking.ContainsKey(player.PlayerUID))
                playerZoneTracking[player.PlayerUID] = new HashSet<string>();

            playerZoneTracking[player.PlayerUID].Add(nodeId);

            // Send enable packet
            networkHandler?.SendAutoToggleCaptureZoneHologram(player, true, nodeId);

            sapi.Logger.Debug($"[NodeWars] Player {player.PlayerName} entered war zone {nodeId}");
        }

        /// <summary>
        /// Handle player leaving a war zone
        /// </summary>
        private void OnPlayerLeftWarZone(IServerPlayer player, string nodeId)
        {
            if (playerZoneTracking.TryGetValue(player.PlayerUID, out var zones))
            {
                zones.Remove(nodeId);

                // Only disable hologram if player is not in ANY war zone
                if (zones.Count == 0)
                {
                    networkHandler?.SendAutoToggleCaptureZoneHologram(player, false, null);
                    playerZoneTracking.Remove(player.PlayerUID);

                    sapi.Logger.Debug($"[NodeWars] Player {player.PlayerName} left all war zones");
                }
                else
                {
                    sapi.Logger.Debug($"[NodeWars] Player {player.PlayerName} left war zone {nodeId} but still in {zones.Count} other zone(s)");
                }
            }
        }

        /// <summary>
        /// Cleanup zone monitoring when a war ends
        /// </summary>
        private void CleanupWarZoneMonitoring(string nodeId)
        {
            // Unregister the tick listener
            if (activeWarMonitors.TryGetValue(nodeId, out long listenerId))
            {
                sapi.Event.UnregisterGameTickListener(listenerId);
                activeWarMonitors.Remove(nodeId);
                sapi.Logger.Debug($"[NodeWars] Unregistered zone monitoring for war at {nodeId}");
            }

            // Clean up player tracking and disable holograms
            var playersToUpdate = new List<string>();
            foreach (var kvp in playerZoneTracking)
            {
                if (kvp.Value.Remove(nodeId) && kvp.Value.Count == 0)
                {
                    playersToUpdate.Add(kvp.Key);
                }
            }

            foreach (var playerUid in playersToUpdate)
            {
                var player = sapi.World.PlayerByUid(playerUid) as IServerPlayer;
                if (player != null)
                {
                    networkHandler?.SendAutoToggleCaptureZoneHologram(player, false, null);
                }
                playerZoneTracking.Remove(playerUid);
            }
        }

        #endregion

        #endregion
    }

    #region Enrollment Result Classes

    /// <summary>
    /// Result of a player enrollment attempt
    /// </summary>
    public class EnrollmentResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public EnrollmentFailureReason? FailureReason { get; set; }

        private EnrollmentResult(bool success, string message, EnrollmentFailureReason? reason = null)
        {
            Success = success;
            Message = message;
            FailureReason = reason;
        }

        public static EnrollmentResult Succeeded(string message = "")
        {
            return new EnrollmentResult(true, message);
        }

        public static EnrollmentResult Failed(string message, EnrollmentFailureReason reason)
        {
            return new EnrollmentResult(false, message, reason);
        }
    }

    /// <summary>
    /// Reasons why enrollment might fail
    /// </summary>
    public enum EnrollmentFailureReason
    {
        WarNotFound,
        WarNotActive,
        AlreadyEnrolled,
        NoGuild,
        GuildNotSignedUp,
        InAnotherWar,
        SystemError
    }

    #endregion
}
