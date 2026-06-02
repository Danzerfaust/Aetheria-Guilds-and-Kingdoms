using ProtoBuf;
using System.Collections.Generic;
using System.Linq;

namespace SOAGuildsAndKingdoms.src.party
{
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class PartyMember
    {
        public string PlayerUid { get; set; } = "";
        public string PlayerName { get; set; } = "";
        public bool IsOnline { get; set; } = false;

        public PartyMember()
        {
        }

        public PartyMember(string uid, string name, bool isOnline)
        {
            PlayerUid = uid;
            PlayerName = name;
            IsOnline = isOnline;
        }
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class Party
    {
        public string Name { get; set; } = "";
        public List<PartyMember> Members { get; set; } = [];

        [ProtoIgnore]
        public string? LeaderUid => Members.FirstOrDefault()?.PlayerUid;

        public bool IsLeader(string playerUid)
        {
            return LeaderUid == playerUid;
        }

        public bool HasMember(string playerUid)
        {
            return Members.Any(m => m.PlayerUid == playerUid);
        }

        public PartyMember? GetMember(string playerUid)
        {
            return Members.FirstOrDefault(m => m.PlayerUid == playerUid);
        }

        public Party()
        {
        }

        public Party(string name, string leaderUid, string leaderName = "")
        {
            Name = name;
            Members.Add(new PartyMember(leaderUid, leaderName, true));
        }
    }
}
