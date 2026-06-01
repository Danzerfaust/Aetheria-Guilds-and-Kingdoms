using System;
using System.Collections.Generic;
using System.Linq;
using Vintagestory.API.Common;
using Vintagestory.API.Server;
using SRGuildsAndKingdoms;

namespace SRGuildsAndKingdomsPVP.src.nodewars
{
    /// <summary>
    /// Provides Node Wars data for the Guild UI
    /// This is the bridge between the PVP mod and the Guild mod UI
    /// </summary>
    public class NodeWarDataProvider
    {
        private readonly ICoreServerAPI sapi;
        private readonly NodeWarManager nodeWarManager;
        private readonly SRGuildsAndKingdomsModSystem guildModSystem;

        public NodeWarDataProvider(ICoreServerAPI api, NodeWarManager manager, SRGuildsAndKingdomsModSystem guildMod)
        {
            sapi = api;
            nodeWarManager = manager;
            guildModSystem = guildMod;
        }

        /// <summary>
        /// Get all node wars data for a specific guild
        /// This data is sent to the client for display in the Guild UI
        /// </summary>
        public NodeWarTabData GetNodeWarDataForGuild(string guildName)
        {
            var data = new NodeWarTabData
            {
                ControlledNodes = new List<ControlledNodeInfo>(),
                AvailableWars = new List<AvailableWarInfo>()
            };

            // NOTE: In this system, guild names ARE the unique identifiers (used as UIDs)
            // The GuildSignups dictionary uses guild name as the key

            // Get all nodes controlled by this guild
            var controlledNodes = nodeWarManager.GetAllNodes()
                .Where(n => n.OwningGuildName == guildName)
                .ToList();

            foreach (var node in controlledNodes)
            {
                var controlledNodeInfo = new ControlledNodeInfo
                {
                    NodeId = node.NodeId,
                    NodeName = node.NodeName,
                    CapturedAt = node.LastCapturedTime,
                    InfluencePerDay = 10 // TODO: Make this configurable or calculated
                };

                // Add war status information if there's an active/scheduled war for this node
                var war = nodeWarManager.GetActiveNodeWar(node.NodeId);
                if (war != null)
                {
                    controlledNodeInfo.WarStatus = (int)war.Status;

                    // For scheduled wars, StartTime is the scheduled start time
                    // For active/completed wars, StartTime is when it actually started
                    if (war.Status == NodeWarStatus.Scheduled)
                    {
                        controlledNodeInfo.WarScheduledStartTime = war.StartTime > DateTime.MinValue ? war.StartTime : null;
                    }
                    else if (war.Status == NodeWarStatus.Active || war.Status == NodeWarStatus.Completed)
                    {
                        controlledNodeInfo.WarStartedTime = war.StartTime > DateTime.MinValue ? war.StartTime : null;
                    }

                    controlledNodeInfo.WarEndTime = war.EndTime;
                    controlledNodeInfo.WarMaxGuilds = war.MaxGuilds;

                    // Get signup count for scheduled wars
                    if (war.Status == NodeWarStatus.Scheduled)
                    {
                        controlledNodeInfo.WarSignupCount = war.GetSignupCount();
                    }

                    // Get winner guild name for completed wars
                    if (war.Status == NodeWarStatus.Completed && !string.IsNullOrEmpty(war.ControllingGuildUid))
                    {
                        // In this system, guild UIDs are guild names
                        controlledNodeInfo.WarWinnerGuildName = war.ControllingGuildUid;
                    }
                }

                data.ControlledNodes.Add(controlledNodeInfo);
            }

            // Get current signup for scheduled wars
            var scheduledWars = nodeWarManager.GetScheduledNodeWars();
            foreach (var war in scheduledWars)
            {
                if (war.IsGuildSignedUp(guildName))
                {
                    // Guild is signed up for a scheduled war
                    if (war.GuildSignups.TryGetValue(guildName, out var signup))
                    {
                        data.CurrentSignup = new CurrentSignupInfo
                        {
                            NodeId = war.NodeId,
                            NodeName = nodeWarManager.GetNode(war.NodeId)?.NodeName ?? war.NodeId,
                            SignupTime = signup.SignupTime, // Already DateTime
                            WarStartTime = war.StartTime // Already DateTime
                        };
                        break; // Guild can only be signed up for one war at a time
                    }
                }
            }

            // Get current active war this guild is participating in
            var activeWars = nodeWarManager.GetAllActiveNodeWars();
            foreach (var war in activeWars)
            {
                if (war.IsGuildSignedUp(guildName) && war.Status == NodeWarStatus.Active)
                {
                    // War is active
                    var node = nodeWarManager.GetNode(war.NodeId);
                    data.CurrentWar = new CurrentWarInfo
                    {
                        NodeId = war.NodeId,
                        NodeName = node?.NodeName ?? war.NodeId,
                        Status = war.Status.ToString(),
                        PointsNeeded = war.Config.CapturePointsNeeded
                    };

                    // Add guild's progress if they're participating
                    if (war.GuildProgress.TryGetValue(guildName, out var progress))
                    {
                        data.CurrentWar.YourGuildProgress = new GuildWarProgressInfo
                        {
                            CapturePoints = progress.CapturePoints,
                            PlayersInZone = progress.PlayersInZone,
                            Kills = progress.Kills,
                            Deaths = progress.Deaths
                        };
                    }
                    break; // Guild can only be in one active war at a time
                }
            }

            // Populate available wars list
            // (scheduledWars already retrieved above for CurrentSignup check)
            foreach (var war in scheduledWars)
            {
                // Show all scheduled wars - don't hide based on signup status
                // Wars that are signed up will also appear in CurrentSignup section above
                // Only wars that have started (Active) are excluded by the Scheduled filter

                var node = nodeWarManager.GetNode(war.NodeId);
                if (node == null) continue;

                data.AvailableWars.Add(new AvailableWarInfo
                {
                    NodeId = war.NodeId,
                    NodeName = node.NodeName,
                    WarStartTime = war.StartTime, // Already DateTime
                    CurrentSignups = war.GetSignupCount(),
                    MaxGuilds = war.MaxGuilds,
                    CanSignup = !war.IsGuildSignedUp(guildName) // Indicate if guild can still sign up
                });
            }

            return data;
        }

        /// <summary>
        /// Convert NodeWarTabData to network packet format for transmission to client
        /// </summary>
        public SRGuildsAndKingdoms.src.network.NodeWarDataResponsePacket ConvertToNetworkPacket(NodeWarTabData data)
        {
            var packet = new SRGuildsAndKingdoms.src.network.NodeWarDataResponsePacket
            {
                PlayerUid = "", // Will be set by network handler
                ControlledNodes = data.ControlledNodes.Select(node => new SRGuildsAndKingdoms.src.network.ControlledNodeDto
                {
                    NodeId = node.NodeId,
                    NodeName = node.NodeName,
                    CapturedAtTicks = node.CapturedAt?.Ticks ?? 0,
                    InfluencePerDay = node.InfluencePerDay,
                    // War status fields
                    WarStatus = node.WarStatus,
                    WarScheduledStartTimeTicks = node.WarScheduledStartTime?.Ticks,
                    WarStartedTimeTicks = node.WarStartedTime?.Ticks,
                    WarEndTimeTicks = node.WarEndTime?.Ticks,
                    WarSignupCount = node.WarSignupCount,
                    WarMaxGuilds = node.WarMaxGuilds,
                    WarWinnerGuildName = node.WarWinnerGuildName
                }).ToList(),

                CurrentWar = data.CurrentWar != null ? new SRGuildsAndKingdoms.src.network.CurrentWarDto
                {
                    NodeId = data.CurrentWar.NodeId,
                    NodeName = data.CurrentWar.NodeName,
                    Status = data.CurrentWar.Status,
                    PointsNeeded = data.CurrentWar.PointsNeeded,
                    YourGuildProgress = data.CurrentWar.YourGuildProgress != null ? new SRGuildsAndKingdoms.src.network.GuildWarProgressDto
                    {
                        CapturePoints = data.CurrentWar.YourGuildProgress.CapturePoints,
                        PlayersInZone = data.CurrentWar.YourGuildProgress.PlayersInZone,
                        Kills = data.CurrentWar.YourGuildProgress.Kills,
                        Deaths = data.CurrentWar.YourGuildProgress.Deaths
                    } : null
                } : null,

                AvailableWars = data.AvailableWars.Select(war => new SRGuildsAndKingdoms.src.network.AvailableWarDto
                {
                    NodeId = war.NodeId,
                    NodeName = war.NodeName,
                    WarStartTimeTicks = war.WarStartTime.Ticks,
                    CurrentSignups = war.CurrentSignups,
                    MaxGuilds = war.MaxGuilds,
                    CanSignup = war.CanSignup
                }).ToList(),

                CurrentSignup = data.CurrentSignup != null ? new SRGuildsAndKingdoms.src.network.CurrentSignupDto
                {
                    NodeId = data.CurrentSignup.NodeId,
                    NodeName = data.CurrentSignup.NodeName,
                    SignupTimeTicks = data.CurrentSignup.SignupTime.Ticks,
                    WarStartTimeTicks = data.CurrentSignup.WarStartTime.Ticks
                } : null
            };

            return packet;
        }
    }

    #region Data Transfer Objects (matching Guild mod)

    public class NodeWarTabData
    {
        public List<ControlledNodeInfo> ControlledNodes { get; set; } = new();
        public CurrentWarInfo? CurrentWar { get; set; }
        public List<AvailableWarInfo> AvailableWars { get; set; } = new();
        public CurrentSignupInfo? CurrentSignup { get; set; }
        public string? SelectedWarForSignup { get; set; }
    }

    public class ControlledNodeInfo
    {
        public string NodeId { get; set; } = string.Empty;
        public string NodeName { get; set; } = string.Empty;
        public DateTime? CapturedAt { get; set; }
        public int InfluencePerDay { get; set; }

        // War status fields (from node data)
        public int? WarStatus { get; set; } // NodeWarStatus enum: 0=Pending, 1=Scheduled, 2=Active, 3=Completed, 4=Cancelled
        public DateTime? WarScheduledStartTime { get; set; }
        public DateTime? WarStartedTime { get; set; }
        public DateTime? WarEndTime { get; set; }
        public int? WarSignupCount { get; set; }
        public int? WarMaxGuilds { get; set; }
        public string? WarWinnerGuildName { get; set; }
    }

    public class CurrentWarInfo
    {
        public string NodeId { get; set; } = string.Empty;
        public string NodeName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public double PointsNeeded { get; set; }
        public GuildWarProgressInfo? YourGuildProgress { get; set; }
    }

    public class GuildWarProgressInfo
    {
        public double CapturePoints { get; set; }
        public int PlayersInZone { get; set; }
        public int Kills { get; set; }
        public int Deaths { get; set; }
    }

    public class AvailableWarInfo
    {
        public string NodeId { get; set; } = string.Empty;
        public string NodeName { get; set; } = string.Empty;
        public DateTime WarStartTime { get; set; }
        public int CurrentSignups { get; set; }
        public int MaxGuilds { get; set; }
        public bool CanSignup { get; set; }
    }

    public class CurrentSignupInfo
    {
        public string NodeId { get; set; } = string.Empty;
        public string NodeName { get; set; } = string.Empty;
        public DateTime SignupTime { get; set; }
        public DateTime WarStartTime { get; set; }
    }

    #endregion
}
