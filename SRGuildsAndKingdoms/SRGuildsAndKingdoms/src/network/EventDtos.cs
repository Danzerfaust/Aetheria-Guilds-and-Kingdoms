using ProtoBuf;
using System.Collections.Generic;

namespace SRGuildsAndKingdoms.src.network
{
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class EventDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int MaxPlayers { get; set; }
        public string StartDate { get; set; } = string.Empty;
        public string EndDate { get; set; } = string.Empty;
        public int? LocationX { get; set; }
        public int? LocationY { get; set; }
        public int? LocationZ { get; set; }
        public long CreatedAt { get; set; }
        public List<EventRegistrationDto> Registrations { get; set; } = [];
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class EventRegistrationDto
    {
        public int EventId { get; set; }
        public string RegistreeUid { get; set; } = string.Empty;
        public string RegistrationDate { get; set; } = string.Empty;
    }
}
