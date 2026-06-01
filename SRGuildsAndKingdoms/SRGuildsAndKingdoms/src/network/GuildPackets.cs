using ProtoBuf;
using SRGuildsAndKingdoms.src.guilds;
using System.Collections.Generic;

namespace SRGuildsAndKingdoms.src.network
{
    // Base packet class for all guild-related network messages
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public abstract class GuildPacketBase
    {
        public string PlayerUid { get; set; } = null!;
    }

    // Client-to-Server packets
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class GuildCommandPacket : GuildPacketBase
    {
        public string Command { get; set; } = null!;
        public string[] Arguments { get; set; } = null!;
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class GuildCreatePacket : GuildPacketBase
    {
        public string GuildName { get; set; } = null!;
        public string Description { get; set; } = "";
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class GuildInvitePacket : GuildPacketBase
    {
        public string TargetPlayerUid { get; set; } = null!;
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class GuildAcceptInvitePacket : GuildPacketBase
    {
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class GuildDeclineInvitePacket : GuildPacketBase
    {
        public string GuildName { get; set; } = null!;
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class GuildCancelInvitePacket : GuildPacketBase
    {
        public string InviteeUid { get; set; } = null!;
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class GuildListInvitesPacket : GuildPacketBase
    {
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class GuildInviteListResponsePacket : GuildPacketBase
    {
        public List<GuildInviteInfo> Invites { get; set; } = new();
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class GuildInviteInfo
    {
        public string GuildName { get; set; } = null!;
        public string InviterName { get; set; } = null!;
        public string InviterUid { get; set; } = null!;
        public long ExpiresAtTicks { get; set; }
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class GuildRemoveMemberPacket : GuildPacketBase
    {
        public string TargetPlayerName { get; set; } = null!;
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class GuildClaimLandPacket : GuildPacketBase
    {
        public int BlockX { get; set; }
        public int BlockZ { get; set; }

        // New: Support for outpost claims
        public bool IsOutpost { get; set; } = false;
        public string OutpostName { get; set; } = "";
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class GuildUnclaimLandPacket : GuildPacketBase
    {
        public int BlockX { get; set; }
        public int BlockZ { get; set; }
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class GuildRoleManagementPacket : GuildPacketBase
    {
        public string Action { get; set; } = null!; // "create", "update", "remove", "assign"
        public string RoleName { get; set; } = null!;
        public string TargetPlayerName { get; set; } = null!;
        public string PermissionString { get; set; } = null!;
        public int Hierarchy { get; set; } = 999; // Default to low authority
    }

    // New: Request member list for guild
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class GuildMemberListRequestPacket : GuildPacketBase
    {
    }

    // Tech Contribution packets
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class TechContributionRequestPacket : GuildPacketBase
    {
        public string GuildName { get; set; } = null!;
        public int TechBlockId { get; set; }
        public List<ContributionItemDto> Items { get; set; } = new();
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class ContributionItemDto
    {
        public string ResourceGroupName { get; set; } = null!;
        public string InventoryId { get; set; } = null!;
        public int SlotId { get; set; }
        public int Amount { get; set; }
        public string ItemCode { get; set; } = null!;
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class TechContributionResponsePacket : GuildPacketBase
    {
        public bool Success { get; set; }
        public string Message { get; set; } = null!;
        public int TechBlockId { get; set; }
        public bool TechUnlocked { get; set; }
        public Dictionary<string, int> UpdatedProgress { get; set; } = new();
    }

    // Server-to-Client packets
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class GuildSyncPacket : GuildPacketBase
    {
        public List<GuildSummary> GuildSummaries { get; set; } = new();
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class GuildUpdatePacket : GuildPacketBase
    {
        public GuildSummary UpdatedGuild { get; set; } = null!;
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class GuildNotificationPacket : GuildPacketBase
    {
        public string Message { get; set; } = null!;
        public NotificationType Type { get; set; }
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class GuildInviteNotificationPacket : GuildPacketBase
    {
        public string InviterName { get; set; } = null!;
        public string InviterUid { get; set; } = null!;
        public string GuildName { get; set; } = null!;
        public long ExpiresAtTicks { get; set; } // DateTime.Ticks for serialization
    }

    // New: Response with member list data
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class GuildMemberListPacket : GuildPacketBase
    {
        public List<GuildMemberInfo> Members { get; set; } = new();
    }

    // DTO for member information including online status and last seen
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class GuildMemberInfo
    {
        public string PlayerUid { get; set; } = null!;
        public string PlayerName { get; set; } = null!;
        public string Role { get; set; } = null!;
        public bool IsOnline { get; set; }
        public long LastSeenTicks { get; set; } // DateTime.Ticks for serialization
    }

    // Scaled requirements packets
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class ScaledRequirementsRequestPacket : GuildPacketBase
    {
        public string GuildName { get; set; } = null!;
        public int TechBlockId { get; set; }
        public Dictionary<string, int> BaseRequirements { get; set; } = new();
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class ScaledRequirementsResponsePacket : GuildPacketBase
    {
        public Dictionary<string, int> ScaledRequirements { get; set; } = new();
        public decimal ResourceScaling { get; set; }
        public int MemberCount { get; set; }
    }

    // Guild ownership transfer packet
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class GuildTransferOwnershipPacket : GuildPacketBase
    {
        public string TargetPlayerUid { get; set; } = null!;
    }

    // Personal unlock contribution packets
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class PersonalTechContributionRequestPacket : GuildPacketBase
    {
        public string GuildName { get; set; } = null!;
        public int TechBlockId { get; set; }
        public List<ContributionItemDto> Items { get; set; } = new();
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class PersonalTechContributionResponsePacket : GuildPacketBase
    {
        public bool Success { get; set; }
        public string Message { get; set; } = null!;
        public int TechBlockId { get; set; }
        public bool PersonalUnlockComplete { get; set; }
    }

    // Sync personal unlock progress to client
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class PersonalUnlockProgressSyncPacket : GuildPacketBase
    {
        public string GuildName { get; set; } = null!;
        public Dictionary<int, PersonalUnlockDto> PersonalUnlocks { get; set; } = new();
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class PersonalUnlockDto
    {
        public int TechId { get; set; }
        public bool IsPersonallyUnlocked { get; set; }
        public bool RequiresPersonalUnlock { get; set; }
    }

    // Config sync packet - sends guild configuration to clients
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class GuildConfigPacket : GuildPacketBase
    {
        // Territorial restrictions
        public bool TerritorialRestrictionsEnabled { get; set; }
        public int? TerritorialCenterX { get; set; }
        public int? TerritorialCenterZ { get; set; }
        public int TerritorialRadius { get; set; }

        // Protected zones
        public bool ProtectedZonesEnabled { get; set; }
        public List<ProtectedZoneData> ProtectedZones { get; set; } = new();

        // Nodes
        public List<NodeData> Nodes { get; set; } = new();

        // Tech ages (for age-restricted blocks)
        public List<int> EnabledAges { get; set; } = new();
    }

    // DTO for protected zone data
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class ProtectedZoneData
    {
        public string Name { get; set; } = null!;
        public int X { get; set; }
        public int Z { get; set; }
        public int Radius { get; set; }
        public List<string> WhitelistedPlayers { get; set; } = [];
    }

    // DTO for node data
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class NodeData
    {
        public string Name { get; set; } = null!;
        public int X { get; set; }
        public int Z { get; set; }
        public int Radius { get; set; }
    }

    // TechBlocks config sync packet - sends tech blocks configuration JSON to client
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class TechBlocksConfigSyncPacket : GuildPacketBase
    {
        public string ConfigJson { get; set; } = null!;
        public string ServerIdentifier { get; set; } = null!; // Used to identify which server's config this is
    }

    // Node Wars data request/response packets
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class NodeWarDataRequestPacket : GuildPacketBase
    {
        public string GuildName { get; set; } = null!;
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class NodeWarDataResponsePacket : GuildPacketBase
    {
        public List<ControlledNodeDto> ControlledNodes { get; set; } = new();
        public CurrentWarDto? CurrentWar { get; set; }
        public List<AvailableWarDto> AvailableWars { get; set; } = new();
        public CurrentSignupDto? CurrentSignup { get; set; }
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class ControlledNodeDto
    {
        public string NodeId { get; set; } = string.Empty;
        public string NodeName { get; set; } = string.Empty;
        public long CapturedAtTicks { get; set; } // 0 if null
        public int InfluencePerDay { get; set; }

        // War status fields
        public int? WarStatus { get; set; } // NodeWarStatus enum: 0=Pending, 1=Scheduled, 2=Active, 3=Completed, 4=Cancelled
        public long? WarScheduledStartTimeTicks { get; set; }
        public long? WarStartedTimeTicks { get; set; }
        public long? WarEndTimeTicks { get; set; }
        public int? WarSignupCount { get; set; }
        public int? WarMaxGuilds { get; set; }
        public string? WarWinnerGuildName { get; set; }
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class CurrentWarDto
    {
        public string NodeId { get; set; } = string.Empty;
        public string NodeName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public double PointsNeeded { get; set; }
        public GuildWarProgressDto? YourGuildProgress { get; set; }
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class GuildWarProgressDto
    {
        public double CapturePoints { get; set; }
        public int PlayersInZone { get; set; }
        public int Kills { get; set; }
        public int Deaths { get; set; }
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class AvailableWarDto
    {
        public string NodeId { get; set; } = string.Empty;
        public string NodeName { get; set; } = string.Empty;
        public long WarStartTimeTicks { get; set; }
        public int CurrentSignups { get; set; }
        public int MaxGuilds { get; set; }
        public bool CanSignup { get; set; }
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class CurrentSignupDto
    {
        public string NodeId { get; set; } = string.Empty;
        public string NodeName { get; set; } = string.Empty;
        public long SignupTimeTicks { get; set; }
        public long WarStartTimeTicks { get; set; }
    }

    [ProtoContract]
    public enum NotificationType
    {
        [ProtoEnum]
        Info,
        [ProtoEnum]
        Success,
        [ProtoEnum]
        Warning,
        [ProtoEnum]
        Error
    }
}