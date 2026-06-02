using System;
using System.Collections.Generic;
using System.Linq;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using SOAGuildsAndKingdomsPVP.src.pvp;
using SOAGuildsAndKingdomsPVP.src.nodewars;
using SOAGuildsAndKingdomsPVP.src.utils;
using SOAGuildsAndKingdoms;

namespace SOAGuildsAndKingdomsPVP.src.network
{
    /// <summary>
    /// Handles network communication for PVP system
    /// </summary>
    public class PVPNetworkHandler
    {
        private const string CHANNEL_NAME = "soaguildsandkingdomspvp:main";
        private PVPManager? pvpManager;
        private ICoreServerAPI? serverApi;
        private ICoreClientAPI? clientApi;

        // Client-side callback for notifications
        private Action<string, NotificationType>? onNotificationReceived;

        // Client-side callback for PVP status updates
        private Action<PVPStatusPacket>? onPVPStatusReceived;

        // Client-side callback for opening Node War Admin UI
        private Action? onNodeWarAdminOpenReceived;

        // Client-side callback for Node War Admin UI data
        private Action<NodeWarAdminDataPacket>? onNodeWarAdminDataReceived;

        // Client-side callback for Node War Admin responses
        private Action<NodeWarAdminResponsePacket>? onNodeWarAdminResponseReceived;

        // Client-side callback for capture zone sync
        private Action<CaptureZoneSyncPacket>? onCaptureZoneSyncReceived;

        // Client-side callback for auto-hologram toggle
        private Action<AutoCaptureZoneHologramPacket>? onAutoHologramToggleReceived;

        /// <summary>
        /// Initialize server-side networking
        /// </summary>
        public void InitializeServer(ICoreServerAPI api, PVPManager manager)
        {
            serverApi = api;
            pvpManager = manager;

            var channel = api.Network.RegisterChannel(CHANNEL_NAME)
                .RegisterMessageType<PVPToggleRequestPacket>()
                .RegisterMessageType<PVPStatusPacket>()
                .RegisterMessageType<PVPStatusListPacket>()
                .RegisterMessageType<PVPNotificationPacket>()
                .RegisterMessageType<DuelChallengePacket>()
                .RegisterMessageType<DuelAcceptPacket>()
                .RegisterMessageType<DuelDeclinePacket>()
                .RegisterMessageType<DuelStatusPacket>()
                .RegisterMessageType<NodeWarAdminOpenPacket>()
                .RegisterMessageType<NodeWarAdminDataRequestPacket>()
                .RegisterMessageType<NodeWarAdminDataPacket>()
                .RegisterMessageType<NodeWarRegisterNodePacket>()
                .RegisterMessageType<NodeWarUpdateNodePacket>()
                .RegisterMessageType<NodeWarUnregisterNodePacket>()
                .RegisterMessageType<NodeWarScheduleWarPacket>()
                .RegisterMessageType<NodeWarStartWarPacket>()
                .RegisterMessageType<NodeWarEndWarPacket>()
                .RegisterMessageType<NodeWarCancelWarPacket>()
                .RegisterMessageType<NodeWarAddCaptureZonePacket>()
                .RegisterMessageType<NodeWarRemoveCaptureZonePacket>()
                .RegisterMessageType<NodeWarGuildSignupPacket>()
                .RegisterMessageType<NodeWarGuildCancelSignupPacket>()
                .RegisterMessageType<NodeWarAdminResponsePacket>()
                .RegisterMessageType<CaptureZoneRequestPacket>()
                .RegisterMessageType<CaptureZoneSyncPacket>()
                .RegisterMessageType<AutoCaptureZoneHologramPacket>()
                .SetMessageHandler<PVPToggleRequestPacket>(OnToggleRequestReceived)
                .SetMessageHandler<DuelChallengePacket>(OnDuelChallengeReceived)
                .SetMessageHandler<DuelAcceptPacket>(OnDuelAcceptReceived)
                .SetMessageHandler<DuelDeclinePacket>(OnDuelDeclineReceived)
                .SetMessageHandler<NodeWarAdminDataRequestPacket>(OnNodeWarAdminDataRequest)
                .SetMessageHandler<NodeWarRegisterNodePacket>(OnNodeWarRegisterNode)
                .SetMessageHandler<NodeWarUpdateNodePacket>(OnNodeWarUpdateNode)
                .SetMessageHandler<NodeWarUnregisterNodePacket>(OnNodeWarUnregisterNode)
                .SetMessageHandler<NodeWarScheduleWarPacket>(OnNodeWarScheduleWar)
                .SetMessageHandler<NodeWarStartWarPacket>(OnNodeWarStartWar)
                .SetMessageHandler<NodeWarEndWarPacket>(OnNodeWarEndWar)
                .SetMessageHandler<NodeWarCancelWarPacket>(OnNodeWarCancelWar)
                .SetMessageHandler<NodeWarAddCaptureZonePacket>(OnNodeWarAddCaptureZone)
                .SetMessageHandler<NodeWarRemoveCaptureZonePacket>(OnNodeWarRemoveCaptureZone)
                .SetMessageHandler<NodeWarGuildSignupPacket>(OnNodeWarGuildSignup)
                .SetMessageHandler<NodeWarGuildCancelSignupPacket>(OnNodeWarGuildCancelSignup)
                .SetMessageHandler<CaptureZoneRequestPacket>(OnCaptureZoneRequest);

            // Send PVP status when player joins
            api.Event.PlayerJoin += (player) =>
            {
                if (player is IServerPlayer serverPlayer)
                {
                    SendPVPStatus(serverPlayer);
                    BroadcastPVPStatusList(); // Update all clients with new player
                }
            };

            // Set network handler reference in NodeWarManager for hologram packets
            if (pvpManager?.NodeWarManager != null)
            {
                pvpManager.NodeWarManager.SetNetworkHandler(this);
            }

            api.Logger.Notification("[PVP] Network handler initialized (server-side)");
        }

        /// <summary>
        /// Initialize client-side networking
        /// </summary>
        public void InitializeClient(
            ICoreClientAPI api,
            Action<string, NotificationType> notificationCallback,
            Action<PVPStatusPacket> statusCallback,
            Action nodeWarAdminOpenCallback,
            Action<NodeWarAdminDataPacket>? nodeWarAdminDataCallback = null,
            Action<NodeWarAdminResponsePacket>? nodeWarAdminResponseCallback = null,
            Action<CaptureZoneSyncPacket>? captureZoneSyncCallback = null,
            Action<AutoCaptureZoneHologramPacket>? autoHologramToggleCallback = null)
        {
            clientApi = api;
            onNotificationReceived = notificationCallback;
            onPVPStatusReceived = statusCallback;
            onNodeWarAdminOpenReceived = nodeWarAdminOpenCallback;
            onNodeWarAdminDataReceived = nodeWarAdminDataCallback;
            onNodeWarAdminResponseReceived = nodeWarAdminResponseCallback;
            onCaptureZoneSyncReceived = captureZoneSyncCallback;
            onAutoHologramToggleReceived = autoHologramToggleCallback;

            var channel = api.Network.RegisterChannel(CHANNEL_NAME)
                .RegisterMessageType<PVPToggleRequestPacket>()
                .RegisterMessageType<PVPStatusPacket>()
                .RegisterMessageType<PVPStatusListPacket>()
                .RegisterMessageType<PVPNotificationPacket>()
                .RegisterMessageType<DuelChallengePacket>()
                .RegisterMessageType<DuelAcceptPacket>()
                .RegisterMessageType<DuelDeclinePacket>()
                .RegisterMessageType<DuelStatusPacket>()
                .RegisterMessageType<NodeWarAdminOpenPacket>()
                .RegisterMessageType<NodeWarAdminDataRequestPacket>()
                .RegisterMessageType<NodeWarAdminDataPacket>()
                .RegisterMessageType<NodeWarRegisterNodePacket>()
                .RegisterMessageType<NodeWarUpdateNodePacket>()
                .RegisterMessageType<NodeWarUnregisterNodePacket>()
                .RegisterMessageType<NodeWarScheduleWarPacket>()
                .RegisterMessageType<NodeWarStartWarPacket>()
                .RegisterMessageType<NodeWarEndWarPacket>()
                .RegisterMessageType<NodeWarCancelWarPacket>()
                .RegisterMessageType<NodeWarAddCaptureZonePacket>()
                .RegisterMessageType<NodeWarRemoveCaptureZonePacket>()
                .RegisterMessageType<NodeWarGuildSignupPacket>()
                .RegisterMessageType<NodeWarGuildCancelSignupPacket>()
                .RegisterMessageType<NodeWarAdminResponsePacket>()
                .RegisterMessageType<CaptureZoneRequestPacket>()
                .RegisterMessageType<CaptureZoneSyncPacket>()
                .RegisterMessageType<AutoCaptureZoneHologramPacket>()
                .SetMessageHandler<PVPStatusPacket>(OnPVPStatusReceived)
                .SetMessageHandler<PVPStatusListPacket>(OnPVPStatusListReceived)
                .SetMessageHandler<PVPNotificationPacket>(OnNotificationReceived)
                .SetMessageHandler<DuelStatusPacket>(OnDuelStatusReceived)
                .SetMessageHandler<NodeWarAdminOpenPacket>(OnNodeWarAdminOpenReceived)
                .SetMessageHandler<NodeWarAdminDataPacket>(OnNodeWarAdminDataReceived)
                .SetMessageHandler<NodeWarAdminResponsePacket>(OnNodeWarAdminResponseReceived)
                .SetMessageHandler<CaptureZoneSyncPacket>(OnCaptureZoneSyncReceived)
                .SetMessageHandler<AutoCaptureZoneHologramPacket>(OnAutoHologramToggleReceived);

            api.Logger.Notification("[PVP] Network handler initialized (client-side)");
        }

        #region Server-side methods

        /// <summary>
        /// Handle toggle request from client
        /// </summary>
        private void OnToggleRequestReceived(IServerPlayer player, PVPToggleRequestPacket packet)
        {
            if (pvpManager == null || serverApi == null) return;

            bool success = pvpManager.TogglePVP(player.PlayerUID, out string message);

            if (success)
            {
                // Send updated status to requesting player
                SendPVPStatus(player);

                // Broadcast updated status list to all players
                BroadcastPVPStatusList();

                // Send notification
                SendNotification(player, message, NotificationType.Success);
            }
            else
            {
                SendNotification(player, message, NotificationType.Warning);
            }
        }

        /// <summary>
        /// Send PVP status to a specific player
        /// </summary>
        public void SendPVPStatus(IServerPlayer player)
        {
            if (pvpManager == null || serverApi == null) return;

            var data = pvpManager.GetPlayerData(player.PlayerUID);
            if (data == null) return;

            var packet = new PVPStatusPacket
            {
                PlayerUid = player.PlayerUID,
                PVPEnabled = data.PVPEnabled,
                CooldownRemaining = data.GetRemainingCooldown()
            };

            var channel = serverApi.Network.GetChannel(CHANNEL_NAME);
            channel?.SendPacket(packet, player);
        }

        /// <summary>
        /// Broadcast PVP status list to all players
        /// </summary>
        public void BroadcastPVPStatusList()
        {
            if (serverApi == null || pvpManager == null) return;

            var packet = new PVPStatusListPacket();

            foreach (var player in serverApi.World.AllOnlinePlayers)
            {
                var data = pvpManager.GetPlayerData(player.PlayerUID);
                if (data != null)
                {
                    packet.PVPPlayers.Add(new PVPPlayerInfo
                    {
                        PlayerUid = player.PlayerUID,
                        PlayerName = player.PlayerName,
                        PVPEnabled = data.PVPEnabled
                    });
                }
            }

            var channel = serverApi.Network.GetChannel(CHANNEL_NAME);
            channel?.BroadcastPacket(packet);
        }

        /// <summary>
        /// Send notification to a player
        /// </summary>
        public void SendNotification(IServerPlayer player, string message, NotificationType type)
        {
            if (serverApi == null) return;

            var packet = new PVPNotificationPacket
            {
                Message = message,
                Type = type
            };

            var channel = serverApi.Network.GetChannel(CHANNEL_NAME);
            channel?.SendPacket(packet, player);
        }

        /// <summary>
        /// Handle duel challenge request from client
        /// </summary>
        private void OnDuelChallengeReceived(IServerPlayer player, DuelChallengePacket packet)
        {
            if (pvpManager?.DuelManager == null || serverApi == null) return;

            bool success = pvpManager.DuelManager.CreateChallenge(
                packet.ChallengerUid,
                packet.TargetUid,
                out string message);

            if (success)
            {
                // Send confirmation to challenger
                SendNotification(player, message, NotificationType.Success);

                // Notify target player
                var targetPlayer = serverApi.World.PlayerByUid(packet.TargetUid) as IServerPlayer;
                if (targetPlayer != null)
                {
                    var challengerName = serverApi.World.PlayerByUid(packet.ChallengerUid)?.PlayerName ?? "Unknown";
                    var duelPacket = new DuelStatusPacket
                    {
                        Message = $"{challengerName} has challenged you to a duel! Type /duel accept to accept or /duel decline to decline.",
                        Status = DuelStatus.ChallengeReceived,
                        Player1Uid = packet.ChallengerUid,
                        Player1Name = challengerName,
                        Player2Uid = packet.TargetUid,
                        Player2Name = targetPlayer.PlayerName
                    };

                    var channel = serverApi.Network.GetChannel(CHANNEL_NAME);
                    channel?.SendPacket(duelPacket, targetPlayer);
                }
            }
            else
            {
                SendNotification(player, message, NotificationType.Warning);
            }
        }

        /// <summary>
        /// Handle duel accept from client
        /// </summary>
        private void OnDuelAcceptReceived(IServerPlayer player, DuelAcceptPacket packet)
        {
            if (pvpManager?.DuelManager == null || serverApi == null) return;

            bool success = pvpManager.DuelManager.AcceptChallenge(
                packet.AccepterUid,
                out string message,
                out string challengerUid);

            if (success)
            {
                // Send confirmation to accepter
                SendNotification(player, message, NotificationType.Success);

                // Notify challenger
                var challengerPlayer = serverApi.World.PlayerByUid(challengerUid) as IServerPlayer;
                if (challengerPlayer != null)
                {
                    var duelPacket = new DuelStatusPacket
                    {
                        Message = $"{player.PlayerName} accepted your duel challenge! The duel has begun!",
                        Status = DuelStatus.DuelStarted,
                        Player1Uid = challengerUid,
                        Player1Name = challengerPlayer.PlayerName,
                        Player2Uid = packet.AccepterUid,
                        Player2Name = player.PlayerName
                    };

                    var channel = serverApi.Network.GetChannel(CHANNEL_NAME);
                    channel?.SendPacket(duelPacket, challengerPlayer);
                }
            }
            else
            {
                SendNotification(player, message, NotificationType.Warning);
            }
        }

        /// <summary>
        /// Handle duel decline from client
        /// </summary>
        private void OnDuelDeclineReceived(IServerPlayer player, DuelDeclinePacket packet)
        {
            if (pvpManager?.DuelManager == null || serverApi == null) return;

            bool success = pvpManager.DuelManager.DeclineChallenge(
                packet.DeclinerUid,
                out string message,
                out string challengerUid);

            if (success)
            {
                // Send confirmation to decliner
                SendNotification(player, message, NotificationType.Info);

                // Notify challenger
                var challengerPlayer = serverApi.World.PlayerByUid(challengerUid) as IServerPlayer;
                if (challengerPlayer != null)
                {
                    var duelPacket = new DuelStatusPacket
                    {
                        Message = $"{player.PlayerName} declined your duel challenge.",
                        Status = DuelStatus.ChallengeDeclined,
                        Player1Uid = challengerUid,
                        Player1Name = challengerPlayer.PlayerName,
                        Player2Uid = packet.DeclinerUid,
                        Player2Name = player.PlayerName
                    };

                    var channel = serverApi.Network.GetChannel(CHANNEL_NAME);
                    channel?.SendPacket(duelPacket, challengerPlayer);
                }
            }
            else
            {
                SendNotification(player, message, NotificationType.Warning);
            }
        }

        /// <summary>
        /// Send duel ended notification to both players
        /// </summary>
        public void SendDuelEnded(string player1Uid, string player2Uid, string winnerName)
        {
            if (serverApi == null) return;

            var player1 = serverApi.World.PlayerByUid(player1Uid) as IServerPlayer;
            var player2 = serverApi.World.PlayerByUid(player2Uid) as IServerPlayer;

            var packet = new DuelStatusPacket
            {
                Message = $"The duel has ended! {winnerName} is victorious!",
                Status = DuelStatus.DuelEnded,
                Player1Uid = player1Uid,
                Player1Name = player1?.PlayerName ?? "Unknown",
                Player2Uid = player2Uid,
                Player2Name = player2?.PlayerName ?? "Unknown"
            };

            var channel = serverApi.Network.GetChannel(CHANNEL_NAME);
            if (player1 != null)
                channel?.SendPacket(packet, player1);
            if (player2 != null)
                channel?.SendPacket(packet, player2);
        }

        /// <summary>
        /// Send request to open Node War Admin UI to a player
        /// </summary>
        public void SendNodeWarAdminOpen(IServerPlayer player)
        {
            if (serverApi == null) return;

            var packet = new NodeWarAdminOpenPacket();
            var channel = serverApi.Network.GetChannel(CHANNEL_NAME);
            channel?.SendPacket(packet, player);

            serverApi.Logger.Debug($"[PVP] Sent Node War Admin UI open request to {player.PlayerName}");
        }

        #region Node War Admin Server Handlers

        /// <summary>
        /// Handle node war admin data request
        /// </summary>
        private void OnNodeWarAdminDataRequest(IServerPlayer player, NodeWarAdminDataRequestPacket packet)
        {
            if (serverApi == null || pvpManager?.NodeWarManager == null) return;

            serverApi.Logger.Debug($"[PVP] Processing node war admin data request from {player.PlayerName}");

            var dataPacket = BuildNodeWarAdminDataPacket();

            var channel = serverApi.Network.GetChannel(CHANNEL_NAME);
            channel?.SendPacket(dataPacket, player);

            serverApi.Logger.Debug($"[PVP] Sent node war admin data to {player.PlayerName}");
        }

        /// <summary>
        /// Build node war admin data packet
        /// </summary>
        private NodeWarAdminDataPacket BuildNodeWarAdminDataPacket()
        {
            if (pvpManager?.NodeWarManager == null)
                return new NodeWarAdminDataPacket();

            var packet = new NodeWarAdminDataPacket();

            // Get all nodes
            var allNodes = pvpManager.NodeWarManager.GetAllNodes();
            foreach (var node in allNodes)
            {
                var nodeData = new NodeWarAdminNodeData
                {
                    NodeId = node.NodeId,
                    NodeName = node.NodeName,
                    CenterX = node.Center.X,
                    CenterY = node.Center.Y,
                    CenterZ = node.Center.Z,
                    Radius = node.Radius,
                    OwningGuildName = node.OwningGuildName,
                    IsActive = node.IsActive,
                    Description = node.Description
                };

                // Add capture zones
                foreach (var zone in node.CaptureZones.Values)
                {
                    nodeData.CaptureZones.Add(new NodeWarAdminZoneData
                    {
                        ZoneId = zone.ZoneId,
                        ZoneName = zone.ZoneName,
                        CenterX = zone.Center.X,
                        CenterY = zone.Center.Y,
                        CenterZ = zone.Center.Z,
                        Radius = zone.Radius,
                        IsActive = zone.IsActive
                    });
                }

                // Get war data for this node and populate both node data and separate war data
                var war = pvpManager.NodeWarManager.GetActiveNodeWar(node.NodeId);

                // Always set war status (None if no war)
                nodeData.WarStatus = war != null ? (int)war.Status : (int)NodeWarStatus.None;

                if (war != null)
                {
                    // Populate additional war details in node data for easy UI display
                    nodeData.WarMaxGuilds = war.MaxGuilds;

                    // For scheduled wars, StartTime is the scheduled start time
                    // For active/completed wars, StartTime is when it actually started
                    if (war.Status == NodeWarStatus.Scheduled)
                    {
                        nodeData.WarScheduledStartTime = war.StartTime > DateTime.MinValue ? (war.StartTime - DateTime.UtcNow).TotalHours : null;
                    }
                    else if (war.Status == NodeWarStatus.Active || war.Status == NodeWarStatus.Completed)
                    {
                        nodeData.WarStartedTime = war.StartTime > DateTime.MinValue ? (war.StartTime - DateTime.UtcNow).TotalHours : null;
                    }

                    nodeData.WarEndTime = war.EndTime.HasValue ? (war.EndTime.Value - DateTime.UtcNow).TotalHours : null;

                    // Get signup count for scheduled wars
                    if (war.Status == NodeWarStatus.Scheduled)
                    {
                        var signups = pvpManager.NodeWarManager.GetSignedUpGuilds(node.NodeId);
                        nodeData.WarSignupCount = signups.Count;
                    }

                    // Get winner guild name for completed wars
                    if (war.Status == NodeWarStatus.Completed && !string.IsNullOrEmpty(war.ControllingGuildUid))
                    {
                        nodeData.WarWinnerGuildName = war.ControllingGuildUid; // In this system, UIDs are guild names
                    }

                    var warData = new NodeWarAdminWarData
                    {
                        NodeId = node.NodeId,
                        Status = (int)war.Status,
                        MaxGuilds = war.MaxGuilds,
                        CapturePointsNeeded = war.Config.CapturePointsNeeded
                    };

                    // Add signups for scheduled wars
                    if (war.Status == NodeWarStatus.Scheduled)
                    {
                        var signups = pvpManager.NodeWarManager.GetSignedUpGuilds(node.NodeId);
                        foreach (var signup in signups)
                        {
                            warData.Signups.Add(new NodeWarAdminGuildSignup
                            {
                                GuildName = signup.GuildName,
                                SignupByPlayerUid = signup.SignupByPlayerUid
                            });
                        }
                    }

                    // Add progress for active wars
                    if (war.Status == NodeWarStatus.Active)
                    {
                        foreach (var progress in war.GuildProgress.Values)
                        {
                            warData.Progress.Add(new NodeWarAdminGuildProgress
                            {
                                GuildName = progress.GuildName,
                                CapturePoints = progress.CapturePoints,
                                PlayersInZone = progress.PlayersInZone
                            });
                        }
                    }

                    packet.Wars.Add(warData);
                }

                packet.Nodes.Add(nodeData);
            }

            // Get available guilds
            var guildMod = serverApi?.ModLoader.GetModSystem<SOAGuildsAndKingdomsModSystem>();
            if (guildMod?.GuildManager != null)
            {
                // Use reflection to call GetAllGuilds since it's not a public method
                var getAllGuildsMethod = guildMod.GuildManager.GetType()
                    .GetMethod("GetAllGuilds", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

                if (getAllGuildsMethod != null)
                {
                    var result = getAllGuildsMethod.Invoke(guildMod.GuildManager, null);
                    if (result is IEnumerable<object> guilds)
                    {
                        foreach (var guild in guilds)
                        {
                            var nameProperty = guild.GetType().GetProperty("Name");
                            if (nameProperty != null)
                            {
                                var name = nameProperty.GetValue(guild) as string;
                                if (name != null)
                                {
                                    packet.AvailableGuilds.Add(name);
                                }
                            }
                        }
                    }
                }
            }

            return packet;
        }

        /// <summary>
        /// Handle register node request
        /// </summary>
        private void OnNodeWarRegisterNode(IServerPlayer player, NodeWarRegisterNodePacket packet)
        {
            if (serverApi == null || pvpManager?.NodeWarManager == null) return;

            serverApi.Logger.Debug($"[PVP] {player.PlayerName} requesting to register node: {packet.NodeName}");

            // Calculate position as offset from default spawn
            var pos = PositionUtils.GetOffsetFromSpawn(serverApi.World, packet.PositionX, packet.PositionY, packet.PositionZ);
            var node = new NodeZone(packet.NodeId, packet.NodeName, pos, packet.Radius)
            {
                Description = $"Node zone: {packet.NodeName}"
            };

            pvpManager.NodeWarManager.RegisterNode(node);

            SendNodeWarAdminResponse(player, true, $"✓ Registered node '{packet.NodeName}'");
        }

        /// <summary>
        /// Handle update node request
        /// </summary>
        private void OnNodeWarUpdateNode(IServerPlayer player, NodeWarUpdateNodePacket packet)
        {
            if (serverApi == null || pvpManager?.NodeWarManager == null) return;

            var existingNode = pvpManager.NodeWarManager.GetNode(packet.NodeId);
            if (existingNode == null)
            {
                SendNodeWarAdminResponse(player, false, $"Node '{packet.NodeId}' not found");
                return;
            }

            // Calculate position as offset from default spawn
            var pos = PositionUtils.GetOffsetFromSpawn(serverApi.World, packet.PositionX, packet.PositionY, packet.PositionZ);
            var updatedNode = new NodeZone(packet.NodeId, existingNode.NodeName, pos, existingNode.Radius)
            {
                Description = existingNode.Description,
                IsActive = existingNode.IsActive,
                OwningGuildUid = existingNode.OwningGuildUid,
                OwningGuildName = existingNode.OwningGuildName,
                LastCapturedTime = existingNode.LastCapturedTime,
                Rewards = existingNode.Rewards
            };

            // Copy capture zones
            foreach (var zone in existingNode.CaptureZones.Values)
            {
                updatedNode.CaptureZones[zone.ZoneId] = zone;
            }

            pvpManager.NodeWarManager.RegisterNode(updatedNode);

            SendNodeWarAdminResponse(player, true, $"✓ Updated node '{existingNode.NodeName}' position");
        }

        /// <summary>
        /// Handle unregister node request
        /// </summary>
        private void OnNodeWarUnregisterNode(IServerPlayer player, NodeWarUnregisterNodePacket packet)
        {
            if (serverApi == null || pvpManager?.NodeWarManager == null) return;

            var node = pvpManager.NodeWarManager.GetNode(packet.NodeId);
            if (node == null)
            {
                SendNodeWarAdminResponse(player, false, $"Node '{packet.NodeId}' not found");
                return;
            }

            var war = pvpManager.NodeWarManager.GetActiveNodeWar(packet.NodeId);
            if (war != null && (war.Status == NodeWarStatus.Active || war.Status == NodeWarStatus.Scheduled))
            {
                SendNodeWarAdminResponse(player, false, "Cannot unregister - node has an active or scheduled war");
                return;
            }

            pvpManager.NodeWarManager.UnregisterNode(packet.NodeId);

            SendNodeWarAdminResponse(player, true, $"✓ Unregistered node '{node.NodeName}'");
        }

        /// <summary>
        /// Handle schedule war request
        /// </summary>
        private void OnNodeWarScheduleWar(IServerPlayer player, NodeWarScheduleWarPacket packet)
        {
            if (serverApi == null || pvpManager?.NodeWarManager == null) return;

            // Convert hours from now to DateTime
            DateTime startTime = DateTime.UtcNow.AddHours(packet.StartTime);
            bool success = pvpManager.NodeWarManager.ScheduleNodeWar(packet.NodeId, startTime);

            if (success)
            {
                var node = pvpManager.NodeWarManager.GetNode(packet.NodeId);
                SendNodeWarAdminResponse(player, true, $"✓ Scheduled war for '{node?.NodeName}' in {packet.StartTime:F1} hours (at {startTime:yyyy-MM-dd HH:mm:ss} UTC)");
            }
            else
            {
                SendNodeWarAdminResponse(player, false, "Failed to schedule war");
            }
        }

        /// <summary>
        /// Handle start war request
        /// </summary>
        private void OnNodeWarStartWar(IServerPlayer player, NodeWarStartWarPacket packet)
        {
            if (serverApi == null || pvpManager?.NodeWarManager == null) return;

            bool success = pvpManager.NodeWarManager.StartNodeWar(packet.NodeId);

            if (success)
            {
                var node = pvpManager.NodeWarManager.GetNode(packet.NodeId);
                SendNodeWarAdminResponse(player, true, $"✓ Started war at '{node?.NodeName}'");
            }
            else
            {
                SendNodeWarAdminResponse(player, false, "Failed to start war");
            }
        }

        /// <summary>
        /// Handle end war request
        /// </summary>
        private void OnNodeWarEndWar(IServerPlayer player, NodeWarEndWarPacket packet)
        {
            if (serverApi == null || pvpManager?.NodeWarManager == null) return;

            pvpManager.NodeWarManager.EndNodeWar(packet.NodeId, packet.WinnerGuildUid);
            var node = pvpManager.NodeWarManager.GetNode(packet.NodeId);

            SendNodeWarAdminResponse(player, true, $"✓ Ended war at '{node?.NodeName}'");
        }

        /// <summary>
        /// Handle cancel war request
        /// </summary>
        private void OnNodeWarCancelWar(IServerPlayer player, NodeWarCancelWarPacket packet)
        {
            if (serverApi == null || pvpManager?.NodeWarManager == null) return;

            pvpManager.NodeWarManager.CancelNodeWar(packet.NodeId);
            var node = pvpManager.NodeWarManager.GetNode(packet.NodeId);

            SendNodeWarAdminResponse(player, true, $"✓ Cancelled war at '{node?.NodeName}'");
        }

        /// <summary>
        /// Handle add capture zone request
        /// </summary>
        private void OnNodeWarAddCaptureZone(IServerPlayer player, NodeWarAddCaptureZonePacket packet)
        {
            if (serverApi == null || pvpManager?.NodeWarManager == null) return;

            var node = pvpManager.NodeWarManager.GetNode(packet.NodeId);
            if (node == null)
            {
                SendNodeWarAdminResponse(player, false, "Node not found");
                return;
			}

			// Calculate position as offset from default spawn
			var pos = PositionUtils.GetOffsetFromSpawn(serverApi.World, packet.PositionX, packet.PositionY, packet.PositionZ);

			if (!node.IsPositionInZone(pos))
            {
                SendNodeWarAdminResponse(player, false, $"Position is outside node boundary ({node.Radius} blocks)");
                return;
            }

            var captureZone = new CaptureZone(packet.ZoneId, packet.ZoneName, pos, packet.Radius)
            {
                Description = $"Capture zone: {packet.ZoneName}",
                IsActive = true,
                PointMultiplier = 1.0
            };

            bool success = pvpManager.NodeWarManager.AddCaptureZone(packet.NodeId, captureZone);

            if (success)
            {
                SendNodeWarAdminResponse(player, true, $"✓ Added capture zone '{packet.ZoneName}'");
            }
            else
            {
                SendNodeWarAdminResponse(player, false, $"Failed to add capture zone (ID '{packet.ZoneId}' may already exist)");
            }
        }

        /// <summary>
        /// Handle remove capture zone request
        /// </summary>
        private void OnNodeWarRemoveCaptureZone(IServerPlayer player, NodeWarRemoveCaptureZonePacket packet)
        {
            if (serverApi == null || pvpManager?.NodeWarManager == null) return;

            bool success = pvpManager.NodeWarManager.RemoveCaptureZone(packet.NodeId, packet.ZoneId);

            if (success)
            {
                SendNodeWarAdminResponse(player, true, $"✓ Removed capture zone");
            }
            else
            {
                SendNodeWarAdminResponse(player, false, "Failed to remove capture zone");
            }
        }

        /// <summary>
        /// Handle guild signup for node war
        /// </summary>
        private void OnNodeWarGuildSignup(IServerPlayer player, NodeWarGuildSignupPacket packet)
        {
            if (serverApi == null || pvpManager?.NodeWarManager == null) return;

            // Get guild mod system to find player's guild
            var guildMod = serverApi.ModLoader.GetModSystem<SOAGuildsAndKingdoms.SOAGuildsAndKingdomsModSystem>();
            var guildManager = guildMod?.GetGuildManager();
            if (guildManager == null)
            {
                SendNotification(player, "Guild system not available", NotificationType.Error);
                return;
            }

            var guild = guildManager.GetGuildByMember(player.PlayerUID);
            if (guild == null)
            {
                SendNotification(player, "You are not in a guild", NotificationType.Error);
                return;
            }

            // Check if player is guild leader
            if (!guild.Members.TryGetValue(player.PlayerUID, out var member) || member.Role != "Leader")
            {
                SendNotification(player, "Only guild leaders can sign up for node wars", NotificationType.Error);
                return;
            }

            // Get number of online guild members
            int onlineMembers = serverApi.World.AllOnlinePlayers.Count(p => guild.Members.ContainsKey(p.PlayerUID));

            // Attempt signup - note that guild.Name is used as the UID in the node war system
            var result = pvpManager.NodeWarManager.SignupGuild(
                guild.Name, 
                guild.Name, 
                packet.NodeId, 
                player.PlayerUID, 
                onlineMembers, 
                guild.Members.Count
            );

            if (result.Success)
            {
                SendNotification(player, result.Message, NotificationType.Success);

                // Request updated node war data for the guild
                var guildNetworkHandler = guildMod?.GetNetworkHandler();
                guildNetworkHandler?.BroadcastGuildSummariesToAll();
            }
            else
            {
                SendNotification(player, result.Message, NotificationType.Error);
            }
        }

        /// <summary>
        /// Handle guild cancellation of node war signup
        /// </summary>
        private void OnNodeWarGuildCancelSignup(IServerPlayer player, NodeWarGuildCancelSignupPacket packet)
        {
            if (serverApi == null || pvpManager?.NodeWarManager == null) return;

            // Get guild mod system to find player's guild
            var guildMod = serverApi.ModLoader.GetModSystem<SOAGuildsAndKingdoms.SOAGuildsAndKingdomsModSystem>();
            var guildManager = guildMod?.GetGuildManager();
            if (guildManager == null)
            {
                SendNotification(player, "Guild system not available", NotificationType.Error);
                return;
            }

            var guild = guildManager.GetGuildByMember(player.PlayerUID);
            if (guild == null)
            {
                SendNotification(player, "You are not in a guild", NotificationType.Error);
                return;
            }

            // Check if player is guild leader
            if (!guild.Members.TryGetValue(player.PlayerUID, out var member) || member.Role != "Leader")
            {
                SendNotification(player, "Only guild leaders can cancel war signups", NotificationType.Error);
                return;
            }

            // Attempt cancel - note that guild.Name is used as the UID in the node war system
            var result = pvpManager.NodeWarManager.CancelGuildSignup(guild.Name, packet.NodeId);

            if (result.Success)
            {
                SendNotification(player, result.Message, NotificationType.Success);

                // Request updated node war data for the guild
                var guildNetworkHandler = guildMod?.GetNetworkHandler();
                guildNetworkHandler?.BroadcastGuildSummariesToAll();
            }
            else
            {
                SendNotification(player, result.Message, NotificationType.Error);
            }
        }

        /// <summary>
        /// Send node war admin response to player
        /// </summary>
        private void SendNodeWarAdminResponse(IServerPlayer player, bool success, string message)
        {
            if (serverApi == null) return;

            var response = new NodeWarAdminResponsePacket
            {
                Success = success,
                Message = message,
                UpdatedData = success ? BuildNodeWarAdminDataPacket() : null
            };

            var channel = serverApi.Network.GetChannel(CHANNEL_NAME);
            channel?.SendPacket(response, player);

            serverApi.Logger.Debug($"[PVP] Sent node war admin response to {player.PlayerName}: {message}");
        }

        #endregion

        #endregion

        #region Client-side methods

        /// <summary>
        /// Request PVP toggle from server
        /// </summary>
        public void RequestTogglePVP()
        {
            if (clientApi == null) return;

            var packet = new PVPToggleRequestPacket
            {
                PlayerUid = clientApi.World.Player.PlayerUID
            };

            var channel = clientApi.Network.GetChannel(CHANNEL_NAME);
            channel?.SendPacket(packet);
        }

        /// <summary>
        /// Handle PVP status packet from server
        /// </summary>
        private void OnPVPStatusReceived(PVPStatusPacket packet)
        {
            onPVPStatusReceived?.Invoke(packet);
        }

        /// <summary>
        /// Handle PVP status list from server
        /// </summary>
        private void OnPVPStatusListReceived(PVPStatusListPacket packet)
        {
            // Store this data for client-side display (name tags, etc.)
            // You can add a callback here if needed
            clientApi?.Logger.Debug($"[PVP] Received status for {packet.PVPPlayers.Count} players");
        }

        /// <summary>
        /// Handle notification from server
        /// </summary>
        private void OnNotificationReceived(PVPNotificationPacket packet)
        {
            onNotificationReceived?.Invoke(packet.Message, packet.Type);
        }

        /// <summary>
        /// Handle duel status updates from server
        /// </summary>
        private void OnDuelStatusReceived(DuelStatusPacket packet)
        {
            if (clientApi == null) return;

            // Convert duel status to notification type
            NotificationType notifType = packet.Status switch
            {
                DuelStatus.ChallengeReceived => NotificationType.Info,
                DuelStatus.DuelStarted => NotificationType.Success,
                DuelStatus.DuelEnded => NotificationType.Success,
                DuelStatus.ChallengeDeclined => NotificationType.Warning,
                _ => NotificationType.Info
            };

            onNotificationReceived?.Invoke(packet.Message, notifType);
        }

        /// <summary>
        /// Handle Node War Admin UI open request from server
        /// </summary>
        private void OnNodeWarAdminOpenReceived(NodeWarAdminOpenPacket packet)
        {
            if (clientApi == null) return;

            clientApi.Logger.Debug("[PVP] Received Node War Admin UI open request");
            onNodeWarAdminOpenReceived?.Invoke();
        }

        /// <summary>
        /// Handle Node War Admin data from server
        /// </summary>
        private void OnNodeWarAdminDataReceived(NodeWarAdminDataPacket packet)
        {
            if (clientApi == null) return;

            clientApi.Logger.Debug($"[PVP] Received node war admin data: {packet.Nodes.Count} nodes, {packet.Wars.Count} wars");
            onNodeWarAdminDataReceived?.Invoke(packet);
        }

        /// <summary>
        /// Handle Node War Admin response from server
        /// </summary>
        private void OnNodeWarAdminResponseReceived(NodeWarAdminResponsePacket packet)
        {
            if (clientApi == null) return;

            clientApi.Logger.Debug($"[PVP] Received node war admin response: {packet.Message}");

            // Show message to player
            if (!string.IsNullOrEmpty(packet.Message))
            {
                clientApi.ShowChatMessage(packet.Message);
            }

            // If there's updated data, invoke the data callback
            if (packet.UpdatedData != null)
            {
                onNodeWarAdminDataReceived?.Invoke(packet.UpdatedData);
            }

            // Invoke response callback
            onNodeWarAdminResponseReceived?.Invoke(packet);
        }

        #region Node War Admin Client Methods

        /// <summary>
        /// Request node war admin data from server
        /// </summary>
        public void RequestNodeWarAdminData()
        {
            if (clientApi == null) return;

            var packet = new NodeWarAdminDataRequestPacket();
            var channel = clientApi.Network.GetChannel(CHANNEL_NAME);
            channel?.SendPacket(packet);

            clientApi.Logger.Debug("[PVP] Requested node war admin data");
        }

        /// <summary>
        /// Request to register a new node
        /// </summary>
        public void RequestRegisterNode(string nodeId, string nodeName, Vec3d position, int radius)
        {
            if (clientApi == null) return;

            var packet = new NodeWarRegisterNodePacket
            {
                NodeId = nodeId,
                NodeName = nodeName,
                PositionX = position.X,
                PositionY = position.Y,
                PositionZ = position.Z,
                Radius = radius
            };

            var channel = clientApi.Network.GetChannel(CHANNEL_NAME);
            channel?.SendPacket(packet);
        }

        /// <summary>
        /// Request to update a node position
        /// </summary>
        public void RequestUpdateNode(string nodeId, Vec3d position)
        {
            if (clientApi == null) return;

            var packet = new NodeWarUpdateNodePacket
            {
                NodeId = nodeId,
                PositionX = position.X,
                PositionY = position.Y,
                PositionZ = position.Z
            };

            var channel = clientApi.Network.GetChannel(CHANNEL_NAME);
            channel?.SendPacket(packet);
        }

        /// <summary>
        /// Request to unregister a node
        /// </summary>
        public void RequestUnregisterNode(string nodeId)
        {
            if (clientApi == null) return;

            var packet = new NodeWarUnregisterNodePacket
            {
                NodeId = nodeId
            };

            var channel = clientApi.Network.GetChannel(CHANNEL_NAME);
            channel?.SendPacket(packet);
        }

        /// <summary>
        /// Request to schedule a node war
        /// </summary>
        public void RequestScheduleWar(string nodeId, double startTime)
        {
            if (clientApi == null) return;

            var packet = new NodeWarScheduleWarPacket
            {
                NodeId = nodeId,
                StartTime = startTime
            };

            var channel = clientApi.Network.GetChannel(CHANNEL_NAME);
            channel?.SendPacket(packet);
        }

        /// <summary>
        /// Request to start a node war
        /// </summary>
        public void RequestStartWar(string nodeId)
        {
            if (clientApi == null) return;

            var packet = new NodeWarStartWarPacket
            {
                NodeId = nodeId
            };

            var channel = clientApi.Network.GetChannel(CHANNEL_NAME);
            channel?.SendPacket(packet);
        }

        /// <summary>
        /// Request to end a node war
        /// </summary>
        public void RequestEndWar(string nodeId, string? winnerGuildUid)
        {
            if (clientApi == null) return;

            var packet = new NodeWarEndWarPacket
            {
                NodeId = nodeId,
                WinnerGuildUid = winnerGuildUid
            };

            var channel = clientApi.Network.GetChannel(CHANNEL_NAME);
            channel?.SendPacket(packet);
        }

        /// <summary>
        /// Request to cancel a node war
        /// </summary>
        public void RequestCancelWar(string nodeId)
        {
            if (clientApi == null) return;

            var packet = new NodeWarCancelWarPacket
            {
                NodeId = nodeId
            };

            var channel = clientApi.Network.GetChannel(CHANNEL_NAME);
            channel?.SendPacket(packet);
        }

        /// <summary>
        /// Request to add a capture zone
        /// </summary>
        public void RequestAddCaptureZone(string nodeId, string zoneId, string zoneName, Vec3d position, int radius)
        {
            if (clientApi == null) return;

            var packet = new NodeWarAddCaptureZonePacket
            {
                NodeId = nodeId,
                ZoneId = zoneId,
                ZoneName = zoneName,
                PositionX = position.X,
                PositionY = position.Y,
                PositionZ = position.Z,
                Radius = radius
            };

            var channel = clientApi.Network.GetChannel(CHANNEL_NAME);
            channel?.SendPacket(packet);
        }

        /// <summary>
        /// Request to remove a capture zone
        /// </summary>
        public void RequestRemoveCaptureZone(string nodeId, string zoneId)
        {
            if (clientApi == null) return;

            var packet = new NodeWarRemoveCaptureZonePacket
            {
                NodeId = nodeId,
                ZoneId = zoneId
            };

            var channel = clientApi.Network.GetChannel(CHANNEL_NAME);
            channel?.SendPacket(packet);
        }

        /// <summary>
        /// Request to sign up guild for a node war
        /// </summary>
        public void RequestGuildSignup(string nodeId)
        {
            if (clientApi == null) return;

            var packet = new NodeWarGuildSignupPacket
            {
                PlayerUid = clientApi.World.Player.PlayerUID,
                NodeId = nodeId
            };

            var channel = clientApi.Network.GetChannel(CHANNEL_NAME);
            channel?.SendPacket(packet);

            clientApi.Logger.Debug($"[PVP] Requested guild signup for node war: {nodeId}");
        }

        /// <summary>
        /// Request to cancel guild signup for a node war
        /// </summary>
        public void RequestCancelGuildSignup(string nodeId)
        {
            if (clientApi == null) return;

            var packet = new NodeWarGuildCancelSignupPacket
            {
                PlayerUid = clientApi.World.Player.PlayerUID,
                NodeId = nodeId
            };

            var channel = clientApi.Network.GetChannel(CHANNEL_NAME);
            channel?.SendPacket(packet);

            clientApi.Logger.Debug($"[PVP] Requested cancel guild signup for node war: {nodeId}");
        }

        #endregion

        #region Capture Zone Hologram

        /// <summary>
        /// Handle capture zone request from client (server-side)
        /// </summary>
        private void OnCaptureZoneRequest(IServerPlayer player, CaptureZoneRequestPacket packet)
        {
            if (serverApi == null || pvpManager?.NodeWarManager == null) return;

            serverApi.Logger.Debug($"[PVP] Received capture zone request from {player.PlayerName}");

            // Get all active node wars
            var activeWars = pvpManager.NodeWarManager.GetAllActiveNodeWars();

            // Build capture zone info list
            var zones = new List<CaptureZoneInfo>();
            foreach (var war in activeWars)
            {
                var node = pvpManager.NodeWarManager.GetNode(war.NodeId);
                if (node == null) continue;

                // Get owning guild name if applicable
                string? owningGuildName = null;
                if (!string.IsNullOrEmpty(war.ControllingGuildUid))
                {
                    var guildMod = serverApi.ModLoader.GetModSystem<SOAGuildsAndKingdomsModSystem>();
                    var guild = guildMod?.GetGuildManager()?.GetGuild(war.ControllingGuildUid);
                    owningGuildName = guild?.Name;
                }

                // Add all active capture zones for this node
                foreach (var captureZone in node.CaptureZones.Values)
                {
                    if (!captureZone.IsActive) continue;

                    zones.Add(new CaptureZoneInfo
                    {
                        NodeId = war.NodeId,
                        NodeName = $"{node.NodeName} - {captureZone.ZoneName}",
                        CenterX = captureZone.Center.X,
                        CenterY = captureZone.Center.Y,
                        CenterZ = captureZone.Center.Z,
                        Radius = captureZone.Radius,
                        Status = (int)war.Status,
                        OwningGuildName = owningGuildName
                    });
                }
            }

            // Send response
            var response = new CaptureZoneSyncPacket
            {
                ActiveZones = zones
            };

            var channel = serverApi.Network.GetChannel(CHANNEL_NAME);
            channel?.SendPacket(response, player);

            serverApi.Logger.Debug($"[PVP] Sent {zones.Count} active capture zones to {player.PlayerName}");
        }

        /// <summary>
        /// Handle capture zone sync from server (client-side)
        /// </summary>
        private void OnCaptureZoneSyncReceived(CaptureZoneSyncPacket packet)
        {
            if (clientApi == null) return;

            clientApi.Logger.Debug($"[PVP] Received capture zone sync: {packet.ActiveZones.Count} zones");
            onCaptureZoneSyncReceived?.Invoke(packet);
        }

        /// <summary>
        /// Request capture zone data from server (client-side)
        /// </summary>
        public void RequestCaptureZones()
        {
            if (clientApi == null) return;

            var packet = new CaptureZoneRequestPacket();
            var channel = clientApi.Network.GetChannel(CHANNEL_NAME);
            channel?.SendPacket(packet);

            clientApi.Logger.Debug("[PVP] Requested capture zone data");
        }

        /// <summary>
        /// Send automatic hologram toggle to client (server-side)
        /// </summary>
        public void SendAutoToggleCaptureZoneHologram(IServerPlayer player, bool enable, string? warId)
        {
            if (serverApi == null) return;

            var packet = new AutoCaptureZoneHologramPacket
            {
                Enable = enable,
                IsAutomatic = true,
                WarId = warId
            };

            var channel = serverApi.Network.GetChannel(CHANNEL_NAME);
            channel?.SendPacket(packet, player);

            serverApi.Logger.Debug($"[PVP] Sent auto-hologram {(enable ? "enable" : "disable")} to {player.PlayerName}");
        }

        /// <summary>
        /// Handle auto-hologram toggle from server (client-side)
        /// </summary>
        private void OnAutoHologramToggleReceived(AutoCaptureZoneHologramPacket packet)
        {
            if (clientApi == null) return;

            clientApi.Logger.Debug($"[PVP] Received auto-hologram toggle: {(packet.Enable ? "enable" : "disable")}");
            onAutoHologramToggleReceived?.Invoke(packet);
        }

        #endregion

        #endregion
    }
}
