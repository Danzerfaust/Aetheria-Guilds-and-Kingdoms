using ProtoBuf;
using System.Collections.Generic;

namespace SOAGuildsAndKingdoms.src.network
{

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class EventManagerOpenPacket
    {
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class EventListRequestPacket
    {
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class EventListResponsePacket
    {
        public List<EventDto> Events { get; set; } = [];
    }
}
