using ProtoBuf;
using SOAGuildsAndKingdoms.src.party;
using System.Collections.Generic;

namespace SOAGuildsAndKingdoms.src.network
{
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public abstract class PartyPacketBase
    {
        public string PlayerUid { get; set; } = null!;
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class PartyCreatePacket : PartyPacketBase
    {
        public string PartyName { get; set; } = null!;
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class PartyInvitePacket : PartyPacketBase
    {
        public string TargetPlayerUid { get; set; } = null!;
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class PartyAcceptInvitePacket : PartyPacketBase
    {
        public string InviterUid { get; set; } = null!;
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class PartyDeclineInvitePacket : PartyPacketBase
    {
        public string InviterUid { get; set; } = null!;
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class PartyCancelInvitePacket : PartyPacketBase
    {
        public string InviteeUid { get; set; } = null!;
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class PartyLeavePacket : PartyPacketBase
    {
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class PartyKickPacket : PartyPacketBase
    {
        public string TargetPlayerUid { get; set; } = null!;
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class PartyPromotePacket : PartyPacketBase
    {
        public string TargetPlayerUid { get; set; } = null!;
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class PartyDisbandPacket : PartyPacketBase
    {
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class PartyDataRequestPacket : PartyPacketBase
    {
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class PartyInviteNotificationPacket : PartyPacketBase
    {
        public string InviterName { get; set; } = null!;
        public string InviterUid { get; set; } = null!;
        public string PartyName { get; set; } = null!;
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class PartyInviteAcceptedPacket : PartyPacketBase
    {
        public string AccepterName { get; set; } = null!;
        public string AccepterUid { get; set; } = null!;
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class PartyInviteDeclinedPacket : PartyPacketBase
    {
        public string DeclinerName { get; set; } = null!;
        public string DeclinerUid { get; set; } = null!;
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class PartyInviteCancelledPacket : PartyPacketBase
    {
        public string InviterName { get; set; } = null!;
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class PartyUpdatePacket : PartyPacketBase
    {
        public Party? PartyData { get; set; }
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class PartyOperationResultPacket : PartyPacketBase
    {
        public bool Success { get; set; }
        public string Message { get; set; } = null!;
        public PartyOperationType OperationType { get; set; }
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public enum PartyOperationType
    {
        Create,
        Invite,
        Accept,
        Decline,
        Leave,
        Kick,
        Promote,
        Disband,
        CancelInvite
    }
}
