using ProtoBuf;
using System.Collections.Generic;

namespace SRGuildsAndKingdomsPVP.src.network
{
    /// <summary>
    /// Packet to sync PVP status from server to client
    /// </summary>
    [ProtoContract]
    public class PVPStatusPacket
    {
        [ProtoMember(1)]
        public string PlayerUid { get; set; } = string.Empty;

        [ProtoMember(2)]
        public bool PVPEnabled { get; set; }

        [ProtoMember(3)]
        public int CooldownRemaining { get; set; }
    }

    /// <summary>
    /// Packet to request toggling PVP status
    /// </summary>
    [ProtoContract]
    public class PVPToggleRequestPacket
    {
        [ProtoMember(1)]
        public string PlayerUid { get; set; } = string.Empty;
    }

    /// <summary>
    /// Packet to sync all PVP player statuses (for client-side name tags/indicators)
    /// </summary>
    [ProtoContract]
    public class PVPStatusListPacket
    {
        [ProtoMember(1)]
        public List<PVPPlayerInfo> PVPPlayers { get; set; } = new();
    }

    /// <summary>
    /// Info about a player's PVP status
    /// </summary>
    [ProtoContract]
    public class PVPPlayerInfo
    {
        [ProtoMember(1)]
        public string PlayerUid { get; set; } = string.Empty;

        [ProtoMember(2)]
        public string PlayerName { get; set; } = string.Empty;

        [ProtoMember(3)]
        public bool PVPEnabled { get; set; }
    }

    /// <summary>
    /// Notification packet for PVP events
    /// </summary>
    [ProtoContract]
    public class PVPNotificationPacket
    {
        [ProtoMember(1)]
        public string Message { get; set; } = string.Empty;

        [ProtoMember(2)]
        public NotificationType Type { get; set; }
    }

    public enum NotificationType
    {
        Info = 0,
        Success = 1,
        Warning = 2,
        Error = 3
    }

    /// <summary>
    /// Packet to request a duel challenge
    /// </summary>
    [ProtoContract]
    public class DuelChallengePacket
    {
        [ProtoMember(1)]
        public string ChallengerUid { get; set; } = string.Empty;

        [ProtoMember(2)]
        public string TargetUid { get; set; } = string.Empty;
    }

    /// <summary>
    /// Packet to accept a duel challenge
    /// </summary>
    [ProtoContract]
    public class DuelAcceptPacket
    {
        [ProtoMember(1)]
        public string AccepterUid { get; set; } = string.Empty;
    }

    /// <summary>
    /// Packet to decline a duel challenge
    /// </summary>
    [ProtoContract]
    public class DuelDeclinePacket
    {
        [ProtoMember(1)]
        public string DeclinerUid { get; set; } = string.Empty;
    }

    /// <summary>
    /// Packet to notify about duel status changes
    /// </summary>
    [ProtoContract]
    public class DuelStatusPacket
    {
        [ProtoMember(1)]
        public string Message { get; set; } = string.Empty;

        [ProtoMember(2)]
        public DuelStatus Status { get; set; }

        [ProtoMember(3)]
        public string Player1Uid { get; set; } = string.Empty;

        [ProtoMember(4)]
        public string Player1Name { get; set; } = string.Empty;

        [ProtoMember(5)]
        public string Player2Uid { get; set; } = string.Empty;

        [ProtoMember(6)]
        public string Player2Name { get; set; } = string.Empty;
    }

    public enum DuelStatus
    {
        ChallengeReceived = 0,
        DuelStarted = 1,
        DuelEnded = 2,
        ChallengeDeclined = 3
    }

    /// <summary>
    /// Packet to request opening the Node War Admin UI
    /// </summary>
    [ProtoContract]
    public class NodeWarAdminOpenPacket
    {
        // Empty packet - just a signal to open the UI
    }

    #region Node War Admin Packets

    /// <summary>
    /// Request node war admin data from server
    /// </summary>
    [ProtoContract]
    public class NodeWarAdminDataRequestPacket
    {
        // Empty - requests all data
    }

    /// <summary>
    /// Send node war admin data to client
    /// </summary>
    [ProtoContract]
    public class NodeWarAdminDataPacket
    {
        [ProtoMember(1)]
        public List<NodeWarAdminNodeData> Nodes { get; set; } = new();

        [ProtoMember(2)]
        public List<NodeWarAdminWarData> Wars { get; set; } = new();

        [ProtoMember(3)]
        public List<string> AvailableGuilds { get; set; } = new();
    }

    [ProtoContract]
    public class NodeWarAdminNodeData
    {
        [ProtoMember(1)]
        public string NodeId { get; set; } = string.Empty;

        [ProtoMember(2)]
        public string NodeName { get; set; } = string.Empty;

        [ProtoMember(3)]
        public double CenterX { get; set; }

        [ProtoMember(4)]
        public double CenterY { get; set; }

        [ProtoMember(5)]
        public double CenterZ { get; set; }

        [ProtoMember(6)]
        public int Radius { get; set; }

        [ProtoMember(7)]
        public string? OwningGuildName { get; set; }

        [ProtoMember(8)]
        public bool IsActive { get; set; }

        [ProtoMember(9)]
        public List<NodeWarAdminZoneData> CaptureZones { get; set; } = new();

        [ProtoMember(10)]
        public string? Description { get; set; }

        // War status fields
        [ProtoMember(11)]
        public int? WarStatus { get; set; } // NodeWarStatus enum: 0=Pending, 1=Scheduled, 2=Active, 3=Completed, 4=Cancelled

        [ProtoMember(12)]
        public double? WarScheduledStartTime { get; set; } // When war is scheduled to start (game hours)

        [ProtoMember(13)]
        public double? WarStartedTime { get; set; } // When war actually started (game hours)

        [ProtoMember(14)]
        public double? WarEndTime { get; set; } // When war ended (game hours)

        [ProtoMember(15)]
        public int? WarSignupCount { get; set; } // Number of guilds signed up

        [ProtoMember(16)]
        public int? WarMaxGuilds { get; set; } // Max guilds allowed

        [ProtoMember(17)]
        public string? WarWinnerGuildName { get; set; } // Name of winning guild (if war completed)
    }

    [ProtoContract]
    public class NodeWarAdminZoneData
    {
        [ProtoMember(1)]
        public string ZoneId { get; set; } = string.Empty;

        [ProtoMember(2)]
        public string ZoneName { get; set; } = string.Empty;

        [ProtoMember(3)]
        public double CenterX { get; set; }

        [ProtoMember(4)]
        public double CenterY { get; set; }

        [ProtoMember(5)]
        public double CenterZ { get; set; }

        [ProtoMember(6)]
        public int Radius { get; set; }

        [ProtoMember(7)]
        public bool IsActive { get; set; }
    }

    [ProtoContract]
    public class NodeWarAdminWarData
    {
        [ProtoMember(1)]
        public string NodeId { get; set; } = string.Empty;

        [ProtoMember(2)]
        public int Status { get; set; } // NodeWarStatus enum

        [ProtoMember(3)]
        public List<NodeWarAdminGuildSignup> Signups { get; set; } = new();

        [ProtoMember(4)]
        public List<NodeWarAdminGuildProgress> Progress { get; set; } = new();

        [ProtoMember(5)]
        public int MaxGuilds { get; set; }

        [ProtoMember(6)]
        public double CapturePointsNeeded { get; set; }
    }

    [ProtoContract]
    public class NodeWarAdminGuildSignup
    {
        [ProtoMember(1)]
        public string GuildName { get; set; } = string.Empty;

        [ProtoMember(2)]
        public string SignupByPlayerUid { get; set; } = string.Empty;
    }

    [ProtoContract]
    public class NodeWarAdminGuildProgress
    {
        [ProtoMember(1)]
        public string GuildName { get; set; } = string.Empty;

        [ProtoMember(2)]
        public double CapturePoints { get; set; }

        [ProtoMember(3)]
        public int PlayersInZone { get; set; }
    }

    /// <summary>
    /// Command to register a new node
    /// </summary>
    [ProtoContract]
    public class NodeWarRegisterNodePacket
    {
        [ProtoMember(1)]
        public string NodeId { get; set; } = string.Empty;

        [ProtoMember(2)]
        public string NodeName { get; set; } = string.Empty;

        [ProtoMember(3)]
        public double PositionX { get; set; }

        [ProtoMember(4)]
        public double PositionY { get; set; }

        [ProtoMember(5)]
        public double PositionZ { get; set; }

        [ProtoMember(6)]
        public int Radius { get; set; }
    }

    /// <summary>
    /// Command to update node position
    /// </summary>
    [ProtoContract]
    public class NodeWarUpdateNodePacket
    {
        [ProtoMember(1)]
        public string NodeId { get; set; } = string.Empty;

        [ProtoMember(2)]
        public double PositionX { get; set; }

        [ProtoMember(3)]
        public double PositionY { get; set; }

        [ProtoMember(4)]
        public double PositionZ { get; set; }
    }

    /// <summary>
    /// Command to unregister a node
    /// </summary>
    [ProtoContract]
    public class NodeWarUnregisterNodePacket
    {
        [ProtoMember(1)]
        public string NodeId { get; set; } = string.Empty;
    }

    /// <summary>
    /// Command to schedule a node war
    /// </summary>
    [ProtoContract]
    public class NodeWarScheduleWarPacket
    {
        [ProtoMember(1)]
        public string NodeId { get; set; } = string.Empty;

        [ProtoMember(2)]
        public double StartTime { get; set; }
    }

    /// <summary>
    /// Command to start a node war
    /// </summary>
    [ProtoContract]
    public class NodeWarStartWarPacket
    {
        [ProtoMember(1)]
        public string NodeId { get; set; } = string.Empty;
    }

    /// <summary>
    /// Command to end a node war
    /// </summary>
    [ProtoContract]
    public class NodeWarEndWarPacket
    {
        [ProtoMember(1)]
        public string NodeId { get; set; } = string.Empty;

        [ProtoMember(2)]
        public string? WinnerGuildUid { get; set; }
    }

    /// <summary>
    /// Command to cancel a node war
    /// </summary>
    [ProtoContract]
    public class NodeWarCancelWarPacket
    {
        [ProtoMember(1)]
        public string NodeId { get; set; } = string.Empty;
    }

    /// <summary>
    /// Command to add a capture zone
    /// </summary>
    [ProtoContract]
    public class NodeWarAddCaptureZonePacket
    {
        [ProtoMember(1)]
        public string NodeId { get; set; } = string.Empty;

        [ProtoMember(2)]
        public string ZoneId { get; set; } = string.Empty;

        [ProtoMember(3)]
        public string ZoneName { get; set; } = string.Empty;

        [ProtoMember(4)]
        public double PositionX { get; set; }

        [ProtoMember(5)]
        public double PositionY { get; set; }

        [ProtoMember(6)]
        public double PositionZ { get; set; }

        [ProtoMember(7)]
        public int Radius { get; set; }
    }

    /// <summary>
    /// Command to remove a capture zone
    /// </summary>
    [ProtoContract]
    public class NodeWarRemoveCaptureZonePacket
    {
        [ProtoMember(1)]
        public string NodeId { get; set; } = string.Empty;

        [ProtoMember(2)]
        public string ZoneId { get; set; } = string.Empty;
    }

    /// <summary>
    /// Command to sign up a guild for a node war
    /// </summary>
    [ProtoContract]
    public class NodeWarGuildSignupPacket
    {
        [ProtoMember(1)]
        public string PlayerUid { get; set; } = string.Empty;

        [ProtoMember(2)]
        public string NodeId { get; set; } = string.Empty;
    }

    /// <summary>
    /// Command to cancel a guild's signup for a node war
    /// </summary>
    [ProtoContract]
    public class NodeWarGuildCancelSignupPacket
    {
        [ProtoMember(1)]
        public string PlayerUid { get; set; } = string.Empty;

        [ProtoMember(2)]
        public string NodeId { get; set; } = string.Empty;
    }

    /// <summary>
    /// Response packet for node war admin commands
    /// </summary>
    [ProtoContract]
    public class NodeWarAdminResponsePacket
    {
        [ProtoMember(1)]
        public bool Success { get; set; }

        [ProtoMember(2)]
        public string Message { get; set; } = string.Empty;

        [ProtoMember(3)]
        public NodeWarAdminDataPacket? UpdatedData { get; set; }
    }

    #endregion

    #region Capture Zone Hologram Packets

    /// <summary>
    /// Request active capture zones from server for hologram display
    /// </summary>
    [ProtoContract]
    public class CaptureZoneRequestPacket
    {
        // Empty - just a signal to request zones
    }

    /// <summary>
    /// Send active capture zones to client for hologram display
    /// </summary>
    [ProtoContract]
    public class CaptureZoneSyncPacket
    {
        [ProtoMember(1)]
        public List<CaptureZoneInfo> ActiveZones { get; set; } = new();
    }

    /// <summary>
    /// Information about a single capture zone
    /// </summary>
    [ProtoContract]
    public class CaptureZoneInfo
    {
        [ProtoMember(1)]
        public string NodeId { get; set; } = string.Empty;

        [ProtoMember(2)]
        public string NodeName { get; set; } = string.Empty;

        [ProtoMember(3)]
        public double CenterX { get; set; }

        [ProtoMember(4)]
        public double CenterY { get; set; }

        [ProtoMember(5)]
        public double CenterZ { get; set; }

        [ProtoMember(6)]
        public int Radius { get; set; }

        [ProtoMember(7)]
        public int Status { get; set; } // NodeWarStatus enum

        [ProtoMember(8)]
        public string? OwningGuildName { get; set; }
    }

    /// <summary>
    /// Packet to automatically toggle capture zone hologram based on player position
    /// </summary>
    [ProtoContract]
    public class AutoCaptureZoneHologramPacket
    {
        [ProtoMember(1)]
        public bool Enable { get; set; }

        [ProtoMember(2)]
        public bool IsAutomatic { get; set; }

        [ProtoMember(3)]
        public string? WarId { get; set; }
    }

    #endregion
}
