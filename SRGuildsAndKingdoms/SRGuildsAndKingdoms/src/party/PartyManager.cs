using System;
using System.Collections.Generic;
using System.Linq;
using Vintagestory.API.Server;

namespace SRGuildsAndKingdoms.src.party
{
    public class PendingPartyInvite
    {
        public string InviterUid { get; set; } = "";
        public string InviteeUid { get; set; } = "";
        public string PartyLeaderUid { get; set; } = "";
    }

    public class PartyManager(ICoreServerAPI sapi)
    {
        private readonly List<Party> parties = [];
        private readonly Dictionary<string, PendingPartyInvite> pendingInvites = [];
        private const int MAX_PARTY_SIZE = 8;

        public event Action? OnPartiesChanged;

        public Party? CreateParty(string partyName, string leaderUid)
        {
            if (GetPartyByMember(leaderUid) != null)
            {
                sapi.Logger.Debug($"[PartyManager] Player {leaderUid} is already in a party");
                return null;
            }

            var leaderPlayer = sapi.World.AllOnlinePlayers.FirstOrDefault(p => p.PlayerUID == leaderUid);
            var leaderName = leaderPlayer?.PlayerName ?? "Unknown";
            var party = new Party(partyName, leaderUid, leaderName);
            parties.Add(party);

            sapi.Logger.Notification($"[PartyManager] Party '{partyName}' created by {leaderUid}");
            OnPartiesChanged?.Invoke();

            return party;
        }

        public bool AddPlayerToParty(Party party, string playerUid)
        {
            if (party == null) return false;

            if (GetPartyByMember(playerUid) != null)
            {
                sapi.Logger.Debug($"[PartyManager] Player {playerUid} is already in a party");
                return false;
            }

            if (party.Members.Count >= MAX_PARTY_SIZE)
            {
                sapi.Logger.Debug($"[PartyManager] Party '{party.Name}' is full");
                return false;
            }

            var player = sapi.World.AllOnlinePlayers.FirstOrDefault(p => p.PlayerUID == playerUid);
            var playerName = player?.PlayerName ?? "Unknown";
            var isOnline = player != null;

            party.Members.Add(new PartyMember(playerUid, playerName, isOnline));
            sapi.Logger.Notification($"[PartyManager] Player {playerUid} joined party '{party.Name}'");
            OnPartiesChanged?.Invoke();

            return true;
        }

        public Party? GetPartyByMember(string playerUid)
        {
            return parties.FirstOrDefault(p => p.HasMember(playerUid));
        }

        public bool RemovePlayerFromParty(string playerUid)
        {
            var party = GetPartyByMember(playerUid);
            if (party == null) return false;

            var member = party.GetMember(playerUid);
            if (member != null)
            {
                party.Members.Remove(member);
                sapi.Logger.Notification($"[PartyManager] Player {playerUid} left party '{party.Name}'");
            }

            if (party.Members.Count == 0)
            {
                DisbandParty(party);
            }
            else
            {
                OnPartiesChanged?.Invoke();
            }

            return true;
        }

        public bool KickPlayerFromParty(string kickerUid, string targetUid)
        {
            var party = GetPartyByMember(kickerUid);
            if (party == null) return false;

            if (!party.IsLeader(kickerUid))
            {
                sapi.Logger.Debug($"[PartyManager] Player {kickerUid} is not the party leader");
                return false;
            }

            if (kickerUid == targetUid)
            {
                sapi.Logger.Debug($"[PartyManager] {kickerUid} cannot kick self");
                return false;
            }

            if (!party.HasMember(targetUid))
            {
                sapi.Logger.Debug($"[PartyManager] {kickerUid} cannot kick non-party member {targetUid}");
                return false;
            }

            var member = party.GetMember(targetUid);
            if (member != null)
            {
                party.Members.Remove(member);
                sapi.Logger.Notification($"[PartyManager] Player {targetUid} was kicked from party '{party.Name}' by {kickerUid}");
                OnPartiesChanged?.Invoke();
            }

            return true;
        }

        public bool PromotePlayerToLeader(string promoterUid, string targetUid)
        {
            var party = GetPartyByMember(promoterUid);
            if (party == null) return false;

            if (!party.IsLeader(promoterUid))
            {
                sapi.Logger.Debug($"[PartyManager] Player {promoterUid} is not the party leader");
                return false;
            }

            if (promoterUid == targetUid)
            {
                sapi.Logger.Debug($"[PartyManager] {promoterUid} cannot promote self");
                return false;
            }

            if (!party.HasMember(targetUid))
            {
                sapi.Logger.Debug($"[PartyManager] {promoterUid} cannot promote non-party member {targetUid}");
                return false;
            }

            var targetMember = party.GetMember(targetUid);
            if (targetMember == null) return false;

            // Move target member to the front of the list (making them leader)
            party.Members.Remove(targetMember);
            party.Members.Insert(0, targetMember);

            sapi.Logger.Notification($"[PartyManager] Player {targetUid} was promoted to leader of party '{party.Name}' by {promoterUid}");
            OnPartiesChanged?.Invoke();

            return true;
        }

        public bool DisbandParty(Party party)
        {
            if (party == null) return false;

            var leaderUid = party.LeaderUid;
            parties.Remove(party);

            if (leaderUid != null)
            {
                ClearPendingInvitesForParty(leaderUid);
            }

            sapi.Logger.Notification($"[PartyManager] Party '{party.Name}' disbanded");
            OnPartiesChanged?.Invoke();

            return true;
        }

        public bool DisbandPartyByLeader(string leaderUid)
        {
            var party = GetPartyByMember(leaderUid);
            if (party == null) return false;

            if (!party.IsLeader(leaderUid))
            {
                sapi.Logger.Debug($"[PartyManager] Player {leaderUid} is not the party leader");
                return false;
            }

            return DisbandParty(party);
        }

        public List<string> GetOnlinePlayerNames()
        {
            return [.. sapi.World.AllOnlinePlayers
                .Select(p => p.PlayerName)
                .OrderBy(name => name)];
        }

        public List<string> GetOnlinePlayerUids()
        {
            return [.. sapi.World.AllOnlinePlayers.Select(p => p.PlayerUID)];
        }

        public string? GetPlayerUidByName(string playerName)
        {
            return sapi.World.AllOnlinePlayers
                .FirstOrDefault(p => p.PlayerName == playerName)
                ?.PlayerUID;
        }

        public bool IsPlayerInParty(string playerUid)
        {
            return GetPartyByMember(playerUid) != null;
        }

        public List<Party> GetAllParties()
        {
            return [.. parties];
        }

        public void AddPendingInvite(string partyLeaderUid, string inviterUid, string inviteeUid)
        {
            var invite = new PendingPartyInvite
            {
                PartyLeaderUid = partyLeaderUid,
                InviterUid = inviterUid,
                InviteeUid = inviteeUid
            };

            pendingInvites[inviteeUid] = invite;
            sapi.Logger.Debug($"[PartyManager] Added pending invite for {inviteeUid} from {inviterUid}");
        }

        public PendingPartyInvite? GetPendingInvite(string inviteeUid)
        {
            return pendingInvites.TryGetValue(inviteeUid, out var invite) ? invite : null;
        }

        public bool HasPendingInvite(string inviteeUid)
        {
            return pendingInvites.ContainsKey(inviteeUid);
        }

        public bool RemovePendingInvite(string inviteeUid)
        {
            if (pendingInvites.Remove(inviteeUid))
            {
                sapi.Logger.Debug($"[PartyManager] Removed pending invite for {inviteeUid}");
                return true;
            }
            return false;
        }

        public List<PendingPartyInvite> GetPendingInvitesForParty(string partyLeaderUid)
        {
            return pendingInvites.Values
                .Where(invite => invite.PartyLeaderUid == partyLeaderUid)
                .ToList();
        }

        public void ClearPendingInvitesForParty(string partyLeaderUid)
        {
            var invitesToRemove = pendingInvites.Values
                .Where(invite => invite.PartyLeaderUid == partyLeaderUid)
                .Select(invite => invite.InviteeUid)
                .ToList();

            foreach (var inviteeUid in invitesToRemove)
            {
                pendingInvites.Remove(inviteeUid);
            }

            if (invitesToRemove.Count > 0)
            {
                sapi.Logger.Debug($"[PartyManager] Cleared {invitesToRemove.Count} pending invites for party leader {partyLeaderUid}");
            }
        }
    }
}
