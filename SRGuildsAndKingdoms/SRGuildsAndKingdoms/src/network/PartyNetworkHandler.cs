using SRGuildsAndKingdoms.src.party;
using System;
using System.Collections.Generic;
using System.Linq;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Server;

namespace SRGuildsAndKingdoms.src.network
{
    public static class PartyMessages
    {
        // Should translate this at some point
        public const string PartyCreated = "Party '{0}' created.";
        public const string PlayerInvited = "Invited {0} to the party.";
        public const string JoinedParty = "You joined '{0}'.";
        public const string InviteDeclined = "Party invite declined.";
        public const string InviteCancelled = "Party invite cancelled.";
        public const string LeftParty = "You left the party.";
        public const string PlayerKicked = "{0} was kicked from the party.";
        public const string YouWereKicked = "You were kicked from the party.";
        public const string PartyDisbanded = "The party has been disbanded.";
        public const string PlayerJoined = "{0} joined your party.";
        public const string PlayerDeclined = "{0} declined your party invite.";
        public const string InviterCancelled = "{0} cancelled the party invite.";
        public const string PlayerPromoted = "{0} has been promoted to party leader.";
        public const string YouWerePromoted = "You have been promoted to party leader.";

        public const string NotInParty = "You are not in a party.";
        public const string CreateFailedAlreadyInParty = "Failed to create party. You may already be in a party.";
        public const string OnlyLeaderCanInvite = "Only the party leader can invite players.";
        public const string PlayerNotFound = "Player '{0}' not found or is offline.";
        public const string PlayerNotOnline = "Player '{0}' is not online.";
        public const string PlayerAlreadyInvited = "{0} already has a pending party invite.";
        public const string PlayerAlreadyInParty = "{0} is already in a party.";
        public const string PartyNoLongerExists = "The party no longer exists.";
        public const string OnlyLeaderCanCancelInvites = "Only the party leader can cancel invites.";
        public const string LeavePartyFailed = "Failed to leave party.";
        public const string KickFailedNotLeader = "Failed to kick player. You may not be the leader or the player is not in your party.";
        public const string PromoteFailedNotLeader = "Failed to promote player. You may not be the leader or the player is not in your party.";
        public const string OnlyLeaderCanDisband = "Only the party leader can disband the party.";
        public const string DisbandFailed = "Failed to disband party.";
        public const string JoinPartyFailed = "Failed to join party. It may be full or no longer exist.";
        public const string InviteSendFailed = "Failed to send invite.";
        public const string NoPendingInvite = "No pending invite found for that player.";
    }

    public class PartyNetworkHandler
    {
        private const string ChannelName = "srguildsandkingdoms:party";

        private ICoreServerAPI? serverApi;
        private ICoreClientAPI? clientApi;
        private PartyManager? partyManager;
        private SRGuildsAndKingdomsModSystem? modSystem;

        private Action<PartyInviteNotificationPacket>? onInviteReceived;
        private Action<Party?>? onPartyDataReceived;
        private Action<string>? onOperationResult;

        public void InitializeServer(ICoreServerAPI api, PartyManager manager, SRGuildsAndKingdomsModSystem modSystem)
        {
            serverApi = api;
            partyManager = manager;
            this.modSystem = modSystem;

            serverApi.Network.RegisterChannel(ChannelName)
                .RegisterMessageType<PartyCreatePacket>()
                .RegisterMessageType<PartyInvitePacket>()
                .RegisterMessageType<PartyAcceptInvitePacket>()
                .RegisterMessageType<PartyDeclineInvitePacket>()
                .RegisterMessageType<PartyCancelInvitePacket>()
                .RegisterMessageType<PartyLeavePacket>()
                .RegisterMessageType<PartyKickPacket>()
                .RegisterMessageType<PartyPromotePacket>()
                .RegisterMessageType<PartyDisbandPacket>()
                .RegisterMessageType<PartyDataRequestPacket>()
                .RegisterMessageType<PartyInviteNotificationPacket>()
                .RegisterMessageType<PartyInviteAcceptedPacket>()
                .RegisterMessageType<PartyInviteDeclinedPacket>()
                .RegisterMessageType<PartyInviteCancelledPacket>()
                .RegisterMessageType<PartyUpdatePacket>()
                .RegisterMessageType<PartyOperationResultPacket>()
                .RegisterMessageType<PartyMember>()
                .RegisterMessageType<Party>()
                .SetMessageHandler<PartyCreatePacket>(OnPartyCreateReceived)
                .SetMessageHandler<PartyInvitePacket>(OnPartyInviteReceived)
                .SetMessageHandler<PartyAcceptInvitePacket>(OnPartyAcceptInviteReceived)
                .SetMessageHandler<PartyDeclineInvitePacket>(OnPartyDeclineInviteReceived)
                .SetMessageHandler<PartyCancelInvitePacket>(OnPartyCancelInviteReceived)
                .SetMessageHandler<PartyLeavePacket>(OnPartyLeaveReceived)
                .SetMessageHandler<PartyKickPacket>(OnPartyKickReceived)
                .SetMessageHandler<PartyPromotePacket>(OnPartyPromoteReceived)
                .SetMessageHandler<PartyDisbandPacket>(OnPartyDisbandReceived)
                .SetMessageHandler<PartyDataRequestPacket>(OnPartyDataRequestReceived);

            serverApi.Event.PlayerJoin += OnPlayerJoin;
            serverApi.Event.PlayerLeave += OnPlayerLeave;
        }

        public void InitializeClient(
            ICoreClientAPI api,
            Action<PartyInviteNotificationPacket> onInvite,
            Action<Party?> onPartyData,
            Action<string> onResult)
        {
            clientApi = api;
            onInviteReceived = onInvite;
            onPartyDataReceived = onPartyData;
            onOperationResult = onResult;

            clientApi.Network.RegisterChannel(ChannelName)
                .RegisterMessageType<PartyCreatePacket>()
                .RegisterMessageType<PartyInvitePacket>()
                .RegisterMessageType<PartyAcceptInvitePacket>()
                .RegisterMessageType<PartyDeclineInvitePacket>()
                .RegisterMessageType<PartyCancelInvitePacket>()
                .RegisterMessageType<PartyLeavePacket>()
                .RegisterMessageType<PartyKickPacket>()
                .RegisterMessageType<PartyPromotePacket>()
                .RegisterMessageType<PartyDisbandPacket>()
                .RegisterMessageType<PartyDataRequestPacket>()
                .RegisterMessageType<PartyInviteNotificationPacket>()
                .RegisterMessageType<PartyInviteAcceptedPacket>()
                .RegisterMessageType<PartyInviteDeclinedPacket>()
                .RegisterMessageType<PartyInviteCancelledPacket>()
                .RegisterMessageType<PartyUpdatePacket>()
                .RegisterMessageType<PartyOperationResultPacket>()
                .RegisterMessageType<PartyMember>()
                .RegisterMessageType<Party>()
                .SetMessageHandler<PartyInviteNotificationPacket>(OnPartyInviteNotificationReceived)
                .SetMessageHandler<PartyInviteAcceptedPacket>(OnPartyInviteAcceptedReceived)
                .SetMessageHandler<PartyInviteDeclinedPacket>(OnPartyInviteDeclinedReceived)
                .SetMessageHandler<PartyInviteCancelledPacket>(OnPartyInviteCancelledReceived)
                .SetMessageHandler<PartyUpdatePacket>(OnPartyUpdateReceived)
                .SetMessageHandler<PartyOperationResultPacket>(OnPartyOperationResultReceived);
        }

        private void OnPlayerJoin(IServerPlayer player)
        {
            var party = partyManager?.GetPartyByMember(player.PlayerUID);
            if (party != null)
            {
                SendPartyUpdate(party);
            }

            var pendingInvite = partyManager?.GetPendingInvite(player.PlayerUID);
            if (pendingInvite != null)
            {
                var inviterParty = partyManager?.GetPartyByMember(pendingInvite.PartyLeaderUid);
                if (inviterParty != null)
                {
                    var inviterPlayer = serverApi?.World.AllOnlinePlayers.FirstOrDefault(p => p.PlayerUID == pendingInvite.InviterUid);
                    var inviteNotification = new PartyInviteNotificationPacket
                    {
                        PlayerUid = player.PlayerUID,
                        InviterName = inviterPlayer?.PlayerName ?? "Unknown",
                        InviterUid = pendingInvite.InviterUid,
                        PartyName = inviterParty.Name
                    };
                    serverApi?.Network.GetChannel(ChannelName).SendPacket(inviteNotification, player);
                }
                else
                {
                    partyManager?.RemovePendingInvite(player.PlayerUID);
                }
            }
        }

        private void OnPlayerLeave(IServerPlayer player)
        {
            var party = partyManager?.GetPartyByMember(player.PlayerUID);
            if (party != null)
            {
                SendPartyUpdate(party);
            }
        }

        private void OnPartyCreateReceived(IServerPlayer player, PartyCreatePacket packet)
        {
            var party = partyManager?.CreateParty(packet.PartyName, player.PlayerUID);

            if (party != null)
            {
                SendOperationResult(player, true, string.Format(PartyMessages.PartyCreated, packet.PartyName), PartyOperationType.Create);
                SendPartyUpdate(party);
            }
            else
            {
                SendOperationResult(player, false, PartyMessages.CreateFailedAlreadyInParty, PartyOperationType.Create);
            }
        }

        private void OnPartyInviteReceived(IServerPlayer player, PartyInvitePacket packet)
        {
            var party = partyManager?.GetPartyByMember(player.PlayerUID);
            if (party == null)
            {
                SendOperationResult(player, false, PartyMessages.NotInParty, PartyOperationType.Invite);
                return;
            }

            if (!party.IsLeader(player.PlayerUID))
            {
                SendOperationResult(player, false, PartyMessages.OnlyLeaderCanInvite, PartyOperationType.Invite);
                return;
            }

            var targetUid = packet.TargetPlayerUid;
            var name = UidToName(targetUid);
            if (targetUid == null || name == "Unknown")
            {
                SendOperationResult(player, false, string.Format(PartyMessages.PlayerNotFound, name), PartyOperationType.Invite);
                return;
            }

            var targetPlayer = serverApi?.World.AllOnlinePlayers.FirstOrDefault(p => p.PlayerUID == targetUid);
            if (targetPlayer == null)
            {
                SendOperationResult(player, false, string.Format(PartyMessages.PlayerNotOnline, name), PartyOperationType.Invite);
                return;
            }

            if (partyManager?.IsPlayerInParty(targetUid) == true)
            {
                SendOperationResult(player, false, string.Format(PartyMessages.PlayerAlreadyInParty, name), PartyOperationType.Invite);
                return;
            }

            if (partyManager?.HasPendingInvite(targetUid) == true)
            {
                SendOperationResult(player, false, string.Format(PartyMessages.PlayerAlreadyInvited, name), PartyOperationType.Invite);
                return;
            }

            if (party.LeaderUid == null)
            {
                return;
            }

            partyManager?.AddPendingInvite(party.LeaderUid, player.PlayerUID, targetUid);

            var inviteNotification = new PartyInviteNotificationPacket
            {
                PlayerUid = targetUid,
                InviterName = player.PlayerName,
                InviterUid = player.PlayerUID,
                PartyName = party.Name
            };

            serverApi?.Network.GetChannel(ChannelName).SendPacket(inviteNotification, targetPlayer as IServerPlayer);
            SendOperationResult(player, true, string.Format(PartyMessages.PlayerInvited, name), PartyOperationType.Invite);
        }

        private void OnPartyAcceptInviteReceived(IServerPlayer player, PartyAcceptInvitePacket packet)
        {
            var inviterParty = partyManager?.GetPartyByMember(packet.InviterUid);
            if (inviterParty == null)
            {
                SendOperationResult(player, false, PartyMessages.PartyNoLongerExists, PartyOperationType.Accept);
                return;
            }

            bool success = partyManager?.AddPlayerToParty(inviterParty, player.PlayerUID) ?? false;

            if (success)
            {
                partyManager?.RemovePendingInvite(player.PlayerUID);

                SendOperationResult(player, true, string.Format(PartyMessages.JoinedParty, inviterParty.Name), PartyOperationType.Accept);

                var leaderPlayer = serverApi?.World.AllOnlinePlayers.FirstOrDefault(p => p.PlayerUID == packet.InviterUid);
                if (leaderPlayer != null)
                {
                    var acceptedNotification = new PartyInviteAcceptedPacket
                    {
                        PlayerUid = packet.InviterUid,
                        AccepterName = player.PlayerName,
                        AccepterUid = player.PlayerUID
                    };
                    serverApi?.Network.GetChannel(ChannelName).SendPacket(acceptedNotification, leaderPlayer as IServerPlayer);
                }

                SendPartyUpdate(inviterParty);
            }
            else
            {
                SendOperationResult(player, false, PartyMessages.JoinPartyFailed, PartyOperationType.Accept);
            }
        }

        private void OnPartyDeclineInviteReceived(IServerPlayer player, PartyDeclineInvitePacket packet)
        {
            partyManager?.RemovePendingInvite(player.PlayerUID);

            SendOperationResult(player, true, PartyMessages.InviteDeclined, PartyOperationType.Decline);

            var inviterPlayer = serverApi?.World.AllOnlinePlayers.FirstOrDefault(p => p.PlayerUID == packet.InviterUid);
            if (inviterPlayer != null)
            {
                var declinedNotification = new PartyInviteDeclinedPacket
                {
                    PlayerUid = packet.InviterUid,
                    DeclinerName = player.PlayerName,
                    DeclinerUid = player.PlayerUID
                };
                serverApi?.Network.GetChannel(ChannelName).SendPacket(declinedNotification, inviterPlayer as IServerPlayer);
            }
        }

        private void OnPartyCancelInviteReceived(IServerPlayer player, PartyCancelInvitePacket packet)
        {
            var party = partyManager?.GetPartyByMember(player.PlayerUID);
            if (party == null || !party.IsLeader(player.PlayerUID))
            {
                SendOperationResult(player, false, PartyMessages.OnlyLeaderCanCancelInvites, PartyOperationType.CancelInvite);
                return;
            }

            bool removed = partyManager?.RemovePendingInvite(packet.InviteeUid) ?? false;
            if (!removed)
            {
                SendOperationResult(player, false, PartyMessages.NoPendingInvite, PartyOperationType.CancelInvite);
                return;
            }

            var inviteePlayer = serverApi?.World.AllOnlinePlayers.FirstOrDefault(p => p.PlayerUID == packet.InviteeUid);
            if (inviteePlayer != null)
            {
                var cancelledNotification = new PartyInviteCancelledPacket
                {
                    PlayerUid = packet.InviteeUid,
                    InviterName = player.PlayerName
                };
                serverApi?.Network.GetChannel(ChannelName).SendPacket(cancelledNotification, inviteePlayer as IServerPlayer);
            }

            SendOperationResult(player, true, PartyMessages.InviteCancelled, PartyOperationType.CancelInvite);
        }

        private void OnPartyLeaveReceived(IServerPlayer player, PartyLeavePacket packet)
        {
            var party = partyManager?.GetPartyByMember(player.PlayerUID);
            if (party == null)
            {
                SendOperationResult(player, false, PartyMessages.NotInParty, PartyOperationType.Leave);
                return;
            }

            bool success = partyManager?.RemovePlayerFromParty(player.PlayerUID) ?? false;

            if (success)
            {
                SendOperationResult(player, true, PartyMessages.LeftParty, PartyOperationType.Leave);

                SendPartyUpdateToPlayer(player, null);

                if (party.Members.Count > 0)
                {
                    SendPartyUpdate(party);
                }
            }
            else
            {
                SendOperationResult(player, false, PartyMessages.LeavePartyFailed, PartyOperationType.Leave);
            }
        }

        private void OnPartyKickReceived(IServerPlayer player, PartyKickPacket packet)
        {
            bool success = partyManager?.KickPlayerFromParty(player.PlayerUID, packet.TargetPlayerUid) ?? false;

            if (success)
            {
                var party = partyManager?.GetPartyByMember(player.PlayerUID);
                var targetPlayer = serverApi?.World.AllOnlinePlayers.FirstOrDefault(p => p.PlayerUID == packet.TargetPlayerUid);
                var targetName = UidToName(packet.TargetPlayerUid);

                SendOperationResult(player, true, string.Format(PartyMessages.PlayerKicked, targetName), PartyOperationType.Kick);

                if (targetPlayer != null)
                {
                    SendOperationResult(targetPlayer as IServerPlayer, true, PartyMessages.YouWereKicked, PartyOperationType.Kick);
                    SendPartyUpdateToPlayer(targetPlayer as IServerPlayer, null);
                }

                if (party != null)
                {
                    SendPartyUpdate(party);
                }
            }
            else
            {
                SendOperationResult(player, false, PartyMessages.KickFailedNotLeader, PartyOperationType.Kick);
            }
        }

        private void OnPartyPromoteReceived(IServerPlayer player, PartyPromotePacket packet)
        {
            bool success = partyManager?.PromotePlayerToLeader(player.PlayerUID, packet.TargetPlayerUid) ?? false;

            if (success)
            {
                var party = partyManager?.GetPartyByMember(player.PlayerUID);
                var targetName = UidToName(packet.TargetPlayerUid);

                SendOperationResult(player, true, string.Format(PartyMessages.PlayerPromoted, targetName), PartyOperationType.Promote);

                var targetPlayer = serverApi?.World.AllOnlinePlayers.FirstOrDefault(p => p.PlayerUID == packet.TargetPlayerUid);
                if (targetPlayer != null)
                {
                    SendOperationResult(targetPlayer as IServerPlayer, true, PartyMessages.YouWerePromoted, PartyOperationType.Promote);
                }

                if (party != null)
                {
                    SendPartyUpdate(party);
                }
            }
            else
            {
                SendOperationResult(player, false, PartyMessages.PromoteFailedNotLeader, PartyOperationType.Promote);
            }
        }

        private void OnPartyDisbandReceived(IServerPlayer player, PartyDisbandPacket packet)
        {
            var party = partyManager?.GetPartyByMember(player.PlayerUID);
            if (party == null)
            {
                SendOperationResult(player, false, PartyMessages.NotInParty, PartyOperationType.Disband);
                return;
            }

            if (!party.IsLeader(player.PlayerUID))
            {
                SendOperationResult(player, false, PartyMessages.OnlyLeaderCanDisband, PartyOperationType.Disband);
                return;
            }

            var memberUids = party.Members.Select(m => m.PlayerUid).ToList();

            bool success = partyManager?.DisbandPartyByLeader(player.PlayerUID) ?? false;

            if (success)
            {
                foreach (var memberUid in memberUids)
                {
                    var memberPlayer = serverApi?.World.AllOnlinePlayers.FirstOrDefault(p => p.PlayerUID == memberUid);
                    if (memberPlayer != null)
                    {
                        SendOperationResult(memberPlayer as IServerPlayer, true, PartyMessages.PartyDisbanded, PartyOperationType.Disband);
                        SendPartyUpdateToPlayer(memberPlayer as IServerPlayer, null);
                    }
                }
            }
            else
            {
                SendOperationResult(player, false, PartyMessages.DisbandFailed, PartyOperationType.Disband);
            }
        }

        private void OnPartyDataRequestReceived(IServerPlayer player, PartyDataRequestPacket packet)
        {
            var party = partyManager?.GetPartyByMember(player.PlayerUID);
            SendPartyUpdateToPlayer(player, party);
        }

        private void OnPartyInviteNotificationReceived(PartyInviteNotificationPacket packet)
        {
            onInviteReceived?.Invoke(packet);
        }

        private void OnPartyInviteAcceptedReceived(PartyInviteAcceptedPacket packet)
        {
            onOperationResult?.Invoke(string.Format(PartyMessages.PlayerJoined, packet.AccepterName));
        }

        private void OnPartyInviteDeclinedReceived(PartyInviteDeclinedPacket packet)
        {
            onOperationResult?.Invoke(string.Format(PartyMessages.PlayerDeclined, packet.DeclinerName));
        }

        private void OnPartyInviteCancelledReceived(PartyInviteCancelledPacket packet)
        {
            onOperationResult?.Invoke(string.Format(PartyMessages.InviterCancelled, packet.InviterName));
        }

        private void OnPartyUpdateReceived(PartyUpdatePacket packet)
        {
            onPartyDataReceived?.Invoke(packet.PartyData);
        }

        private void OnPartyOperationResultReceived(PartyOperationResultPacket packet)
        {
            onOperationResult?.Invoke(packet.Message);
        }

        private void SendOperationResult(IServerPlayer? player, bool success, string message, PartyOperationType operationType)
        {
            if (player == null) return;

            var resultPacket = new PartyOperationResultPacket
            {
                PlayerUid = player.PlayerUID,
                Success = success,
                Message = message,
                OperationType = operationType
            };

            serverApi?.Network.GetChannel(ChannelName).SendPacket(resultPacket, player);
        }

        private void PopulateMemberData(Party? party)
        {
            if (serverApi == null || party == null) return;

            var onlinePlayerUids = new HashSet<string>(serverApi.World.AllOnlinePlayers.Select(p => p.PlayerUID));

            foreach (var member in party.Members)
            {
                // Update online status
                member.IsOnline = onlinePlayerUids.Contains(member.PlayerUid);

                // Update name if it's missing or unknown
                if (string.IsNullOrEmpty(member.PlayerName) || member.PlayerName == "Unknown")
                {
                    member.PlayerName = UidToName(member.PlayerUid);
                }
            }
        }

        private string UidToName(string? uid)
        {
            if (uid == null) return "Unknown";

            var onlinePlayer = serverApi?.World.AllOnlinePlayers.FirstOrDefault(p => p.PlayerUID == uid);
            if (onlinePlayer != null)
            {
                return onlinePlayer.PlayerName;
            }
            else
            {
                var playerData = serverApi?.PlayerData.GetPlayerDataByUid(uid);
                return playerData?.LastKnownPlayername ?? "Unknown";
            }
        }

        public void SendPartyUpdate(Party party)
        {
            if (serverApi == null || party == null) return;

            PopulateMemberData(party);

            var updatePacket = new PartyUpdatePacket
            {
                PlayerUid = "",
                PartyData = party
            };

            foreach (var member in party.Members)
            {
                var player = serverApi.World.AllOnlinePlayers.FirstOrDefault(p => p.PlayerUID == member.PlayerUid);
                if (player != null)
                {
                    serverApi.Network.GetChannel(ChannelName).SendPacket(updatePacket, player as IServerPlayer);
                }
            }
        }

        private void SendPartyUpdateToPlayer(IServerPlayer? player, Party? party)
        {
            if (serverApi == null || player == null) return;

            PopulateMemberData(party);

            var updatePacket = new PartyUpdatePacket
            {
                PlayerUid = player.PlayerUID,
                PartyData = party
            };

            serverApi.Network.GetChannel(ChannelName).SendPacket(updatePacket, player);
        }

        public void SendCreateParty(string partyName)
        {
            if (clientApi == null) return;

            var packet = new PartyCreatePacket
            {
                PlayerUid = clientApi.World.Player.PlayerUID,
                PartyName = partyName
            };

            clientApi.Network.GetChannel(ChannelName).SendPacket(packet);
        }

        public void SendInvitePlayer(string targetPlayerUid)
        {
            if (clientApi == null) return;

            var packet = new PartyInvitePacket
            {
                PlayerUid = clientApi.World.Player.PlayerUID,
                TargetPlayerUid = targetPlayerUid
            };

            clientApi.Network.GetChannel(ChannelName).SendPacket(packet);
        }

        public void SendAcceptInvite(string inviterUid)
        {
            if (clientApi == null) return;

            var packet = new PartyAcceptInvitePacket
            {
                PlayerUid = clientApi.World.Player.PlayerUID,
                InviterUid = inviterUid
            };

            clientApi.Network.GetChannel(ChannelName).SendPacket(packet);
        }

        public void SendDeclineInvite(string inviterUid)
        {
            if (clientApi == null) return;

            var packet = new PartyDeclineInvitePacket
            {
                PlayerUid = clientApi.World.Player.PlayerUID,
                InviterUid = inviterUid
            };

            clientApi.Network.GetChannel(ChannelName).SendPacket(packet);
        }

        public void SendCancelInvite(string inviteeUid)
        {
            if (clientApi == null) return;

            var packet = new PartyCancelInvitePacket
            {
                PlayerUid = clientApi.World.Player.PlayerUID,
                InviteeUid = inviteeUid
            };

            clientApi.Network.GetChannel(ChannelName).SendPacket(packet);
        }

        public void SendLeaveParty()
        {
            if (clientApi == null) return;

            var packet = new PartyLeavePacket
            {
                PlayerUid = clientApi.World.Player.PlayerUID
            };

            clientApi.Network.GetChannel(ChannelName).SendPacket(packet);
        }

        public void SendKickPlayer(string targetPlayerUid)
        {
            if (clientApi == null) return;

            var packet = new PartyKickPacket
            {
                PlayerUid = clientApi.World.Player.PlayerUID,
                TargetPlayerUid = targetPlayerUid
            };

            clientApi.Network.GetChannel(ChannelName).SendPacket(packet);
        }

        public void SendPromotePlayer(string targetPlayerUid)
        {
            if (clientApi == null) return;

            var packet = new PartyPromotePacket
            {
                PlayerUid = clientApi.World.Player.PlayerUID,
                TargetPlayerUid = targetPlayerUid
            };

            clientApi.Network.GetChannel(ChannelName).SendPacket(packet);
        }

        public void SendDisbandParty()
        {
            if (clientApi == null) return;

            var packet = new PartyDisbandPacket
            {
                PlayerUid = clientApi.World.Player.PlayerUID
            };

            clientApi.Network.GetChannel(ChannelName).SendPacket(packet);
        }

        public void SendRequestPartyData()
        {
            if (clientApi == null) return;

            var packet = new PartyDataRequestPacket
            {
                PlayerUid = clientApi.World.Player.PlayerUID
            };

            clientApi.Network.GetChannel(ChannelName).SendPacket(packet);
        }
    }
}
