using System;
using System.Collections.Generic;
using System.Linq;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using SOAGuildsAndKingdomsPVP.src.utils;

namespace SOAGuildsAndKingdomsPVP.src.nodewars
{
    /// <summary>
    /// Manages capture zone mechanics during active node wars
    /// Handles player tracking, point accumulation, and victory conditions
    /// </summary>
    public class CaptureZoneSystem
    {
        private readonly ICoreServerAPI sapi;
        private readonly NodeWarManager nodeWarManager;
        private long updateListenerId;
        private const double UPDATE_INTERVAL_MS = 1000.0; // Update every second

        // Track players in capture zones for each active war
        private readonly Dictionary<string, HashSet<string>> playersInZoneByNode;

        // Track average zone multiplier for each guild in each war
        // Key: nodeId, Value: Dictionary<guildUid, average multiplier>
        private readonly Dictionary<string, Dictionary<string, double>> guildZoneMultipliers;

        public CaptureZoneSystem(ICoreServerAPI api, NodeWarManager manager)
        {
            sapi = api;
            nodeWarManager = manager;
            playersInZoneByNode = new Dictionary<string, HashSet<string>>();
            guildZoneMultipliers = new Dictionary<string, Dictionary<string, double>>();
        }

        /// <summary>
        /// Initialize and start the capture zone system
        /// </summary>
        public void Initialize()
        {
            // Register game tick listener for capture zone updates
            updateListenerId = sapi.Event.RegisterGameTickListener(OnGameTick, (int)UPDATE_INTERVAL_MS);
            sapi.Logger.Notification("[CaptureZones] Capture zone system initialized");
        }

        /// <summary>
        /// Stop the capture zone system
        /// </summary>
        public void Shutdown()
        {
            if (updateListenerId != 0)
            {
                sapi.Event.UnregisterGameTickListener(updateListenerId);
                updateListenerId = 0;
            }
            playersInZoneByNode.Clear();
            guildZoneMultipliers.Clear();
            sapi.Logger.Notification("[CaptureZones] Capture zone system shutdown");
        }

        /// <summary>
        /// Main tick handler for capture zone logic
        /// </summary>
        private void OnGameTick(float dt)
        {

            double deltaTime = dt;

            // Process all active node wars
            var activeWars = nodeWarManager.GetAllActiveNodeWars();
            foreach (var war in activeWars)
            {
                ProcessCaptureZone(war, deltaTime);
            }

            // Clean up tracking for completed wars
            CleanupCompletedWars(activeWars);
        }

		/// <summary>
		/// Process capture zones for a node war
		/// </summary>
		private void ProcessCaptureZone(NodeWar war, double deltaTime)
		{
			if (war.Status != NodeWarStatus.Active)
			{
				return;
			}

			var node = nodeWarManager.GetNode(war.NodeId);
			if (node == null || node.CaptureZones.Count == 0)
			{
				sapi.Logger.Warning($"[CaptureZones] Node {war.NodeId} has no capture zones defined!");
				return;
			}

			// Ensure tracking dictionary exists for this node
			if (!playersInZoneByNode.ContainsKey(war.NodeId))
			{
				playersInZoneByNode[war.NodeId] = new HashSet<string>();
			}
			if (!guildZoneMultipliers.ContainsKey(war.NodeId))
			{
				guildZoneMultipliers[war.NodeId] = new Dictionary<string, double>();
			}

			var previousPlayersInZone = new HashSet<string>(playersInZoneByNode[war.NodeId]);
			var currentPlayersInZone = new HashSet<string>();

			// Reset player counts in all guild progress
			foreach (var progress in war.GuildProgress.Values)
			{
				progress.PlayersInZone = 0;
			}

			// Track multipliers for each guild (sum of all player multipliers)
			var guildMultiplierSum = new Dictionary<string, double>();
			var guildPlayerCount = new Dictionary<string, int>();

			// Check all online players
			foreach (var player in sapi.World.AllOnlinePlayers)
			{
				var serverPlayer = player as IServerPlayer;
				if (serverPlayer == null || serverPlayer.Entity == null)
				{
					continue;
				}

				string playerUid = serverPlayer.PlayerUID;
				var participant = nodeWarManager.GetParticipant(playerUid);

				// Only count players who are participating in this specific war
				if (participant == null || !participant.IsParticipating || participant.CurrentNodeWarId != war.NodeId)
				{
					continue;
				}

				// Convert player's absolute position to spawn-relative offset
				var playerPos = PositionUtils.GetOffsetFromSpawn(sapi.World, serverPlayer.Entity.Pos.XYZ);

				// Check if player is within ANY active capture zone
				bool isInAnyCaptureZone = false;
				double bestMultiplier = 1.0;
				string? captureZoneName = null;

				foreach (var captureZone in node.CaptureZones.Values)
				{
					if (!captureZone.IsActive)
						continue;

					if (captureZone.IsPositionInZone(playerPos))
					{
						isInAnyCaptureZone = true;

						// Use the highest multiplier if player is in multiple zones
						if (captureZone.PointMultiplier > bestMultiplier)
						{
							bestMultiplier = captureZone.PointMultiplier;
							captureZoneName = captureZone.ZoneName;
						}
					}
				}

				if (isInAnyCaptureZone)
				{
					currentPlayersInZone.Add(playerUid);

					// Update guild progress count and multiplier tracking
					if (war.GuildProgress.TryGetValue(participant.GuildUid, out var progress))
					{
						progress.PlayersInZone++;
					}
					else
					{
						// Initialize guild progress if not exists
						var guildProgress = new GuildWarProgress(participant.GuildUid, participant.GuildUid)
						{
							PlayersInZone = 1,
							LastUpdateTime = DateTime.UtcNow
						};
						war.GuildProgress[participant.GuildUid] = guildProgress;
					}

					// Track multiplier sum for averaging
					if (!guildMultiplierSum.ContainsKey(participant.GuildUid))
					{
						guildMultiplierSum[participant.GuildUid] = 0;
						guildPlayerCount[participant.GuildUid] = 0;
					}
					guildMultiplierSum[participant.GuildUid] += bestMultiplier;
					guildPlayerCount[participant.GuildUid]++;

					// Notify player if they just entered a capture zone
					if (!previousPlayersInZone.Contains(playerUid))
					{
						OnPlayerEnterCaptureZone(serverPlayer, war, captureZoneName);
					}
				}
				else
				{
					// Notify player if they just left all capture zones
					if (previousPlayersInZone.Contains(playerUid))
					{
						OnPlayerLeaveCaptureZone(serverPlayer, war);
					}
				}
			}

			// Calculate average multiplier for each guild
			guildZoneMultipliers[war.NodeId].Clear();
			foreach (var guildUid in guildMultiplierSum.Keys)
			{
				if (guildPlayerCount[guildUid] > 0)
				{
					guildZoneMultipliers[war.NodeId][guildUid] = guildMultiplierSum[guildUid] / guildPlayerCount[guildUid];
				}
			}

			// Update tracking
			playersInZoneByNode[war.NodeId] = currentPlayersInZone;

			// Update capture progress for all guilds with zone multipliers
			nodeWarManager.UpdateCaptureProgress(war.NodeId, deltaTime, guildZoneMultipliers[war.NodeId]);

			// Send periodic status updates to players in the zone
			SendStatusUpdatesToPlayers(war, currentPlayersInZone);
		}

        /// <summary>
        /// Called when a player enters a capture zone
        /// </summary>
        private void OnPlayerEnterCaptureZone(IServerPlayer player, NodeWar war, string? captureZoneName)
        {
            var node = nodeWarManager.GetNode(war.NodeId);
            string nodeName = node?.NodeName ?? war.NodeId;
            string zoneName = captureZoneName ?? "Unknown Zone";

            player.SendMessage(
                0,
                $"⚔ Entered capture zone: {zoneName} ({nodeName})",
                EnumChatType.Notification
            );

            sapi.Logger.Debug($"[CaptureZones] {player.PlayerName} entered capture zone '{zoneName}' at {nodeName}");
        }

        /// <summary>
        /// Called when a player leaves a capture zone
        /// </summary>
        private void OnPlayerLeaveCaptureZone(IServerPlayer player, NodeWar war)
        {
            var node = nodeWarManager.GetNode(war.NodeId);
            string nodeName = node?.NodeName ?? war.NodeId;

            player.SendMessage(
                0,
                $"Left capture zone: {nodeName}",
                EnumChatType.Notification
            );

            sapi.Logger.Debug($"[CaptureZones] {player.PlayerName} left capture zone at {nodeName}");
        }

        /// <summary>
        /// Send status updates to players in the capture zone
        /// </summary>
        private void SendStatusUpdatesToPlayers(NodeWar war, HashSet<string> playersInZone)
        {
            // Only send updates every 5 seconds to avoid spam
            DateTime currentTime = DateTime.UtcNow;
            /*bool shouldSendUpdate = war.GuildProgress.Values.Any(p => 
                (currentTime - p.LastUpdateTime).TotalSeconds >= 2.0
            );

            if (!shouldSendUpdate)
            {
                return;
            }*/

            var node = nodeWarManager.GetNode(war.NodeId);
            string nodeName = node?.NodeName ?? war.NodeId;

            // Get leading guild
            var leader = war.GetLeadingGuild();
            
            // Build status message
            string statusMessage = BuildCaptureStatusMessage(war, leader, nodeName);

            // Send to all players in the zone
            foreach (var playerUid in playersInZone)
            {
                var player = sapi.World.PlayerByUid(playerUid) as IServerPlayer;
                if (player != null)
                {
                    player.SendMessage(
                        0,
                        statusMessage,
                        EnumChatType.Notification
                    );
                }
            }
        }

        /// <summary>
        /// Build a status message showing capture progress
        /// </summary>
        private string BuildCaptureStatusMessage(NodeWar war, GuildWarProgress? leader, string nodeName)
        {
            if (leader == null)
            {
                return $"📍 {nodeName} - No guild is currently capturing";
            }

            double progress = (leader.CapturePoints / war.Config.CapturePointsNeeded) * 100.0;
            bool isContested = war.IsContested();

            string statusIcon = isContested ? "⚔" : "🏴";
            string statusText = isContested ? "CONTESTED" : "CAPTURING";

            return $"{statusIcon} {nodeName} - {leader.GuildName}: {progress:F1}% ({leader.PlayersInZone} players) [{statusText}]";
        }

        /// <summary>
        /// Clean up tracking data for wars that are no longer active
        /// </summary>
        private void CleanupCompletedWars(List<NodeWar> activeWars)
        {
            var activeNodeIds = new HashSet<string>(activeWars.Select(w => w.NodeId));
            var nodesToRemove = playersInZoneByNode.Keys.Where(nodeId => !activeNodeIds.Contains(nodeId)).ToList();

            foreach (var nodeId in nodesToRemove)
            {
                // Notify all players in this zone that the war has ended
                if (playersInZoneByNode.TryGetValue(nodeId, out var players))
                {
                    var node = nodeWarManager.GetNode(nodeId);
                    string nodeName = node?.NodeName ?? nodeId;

                    foreach (var playerUid in players)
                    {
                        var player = sapi.World.PlayerByUid(playerUid) as IServerPlayer;
                        if (player != null)
                        {
                            player.SendMessage(
                                0,
                                $"🏁 Node war at {nodeName} has ended",
                                EnumChatType.Notification
                            );
                        }
                    }
                }

                playersInZoneByNode.Remove(nodeId);
                guildZoneMultipliers.Remove(nodeId);
                sapi.Logger.Debug($"[CaptureZones] Cleaned up tracking for completed war: {nodeId}");
            }
        }

        /// <summary>
        /// Get the current status of a capture zone
        /// </summary>
        public CaptureZoneStatus? GetCaptureZoneStatus(string nodeId)
        {
            var war = nodeWarManager.GetActiveNodeWar(nodeId);
            if (war == null || war.Status != NodeWarStatus.Active)
            {
                return null;
            }

            var leader = war.GetLeadingGuild();
            bool isContested = war.IsContested();

            return new CaptureZoneStatus
            {
                NodeId = nodeId,
                IsActive = true,
                IsContested = isContested,
                LeadingGuild = leader?.GuildName,
                LeadingGuildProgress = leader?.CapturePoints ?? 0,
                RequiredPoints = war.Config.CapturePointsNeeded,
                PlayersInZone = playersInZoneByNode.TryGetValue(nodeId, out var players) ? players.Count : 0,
                GuildProgress = war.GuildProgress.Values.Select(p => new GuildCaptureProgress
                {
                    GuildName = p.GuildName,
                    CapturePoints = p.CapturePoints,
                    PlayersInZone = p.PlayersInZone
                }).ToList()
            };
        }

        /// <summary>
        /// Manually trigger a player zone check (useful for debugging or special events)
        /// </summary>
        public void ForceZoneCheck(string nodeId)
        {
            var war = nodeWarManager.GetActiveNodeWar(nodeId);
            if (war != null && war.Status == NodeWarStatus.Active)
            {
                ProcessCaptureZone(war, 0);
            }
        }
    }

    /// <summary>
    /// Status information for a capture zone
    /// </summary>
    public class CaptureZoneStatus
    {
        public string NodeId { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public bool IsContested { get; set; }
        public string? LeadingGuild { get; set; }
        public double LeadingGuildProgress { get; set; }
        public double RequiredPoints { get; set; }
        public int PlayersInZone { get; set; }
        public List<GuildCaptureProgress> GuildProgress { get; set; } = new List<GuildCaptureProgress>();
    }

    /// <summary>
    /// Progress information for a single guild
    /// </summary>
    public class GuildCaptureProgress
    {
        public string GuildName { get; set; } = string.Empty;
        public double CapturePoints { get; set; }
        public int PlayersInZone { get; set; }
    }
}
