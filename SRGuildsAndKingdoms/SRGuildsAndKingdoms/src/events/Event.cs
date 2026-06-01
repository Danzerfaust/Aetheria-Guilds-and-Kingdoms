using System.Collections.Generic;

namespace SRGuildsAndKingdoms.src.events
{
    public class Event
    {
        public int Id { get; set; }
        
        public string Name { get; set; } = string.Empty;
        
        public string? Description { get; set; }
        
        public int MaxPlayers { get; set; } = 1;
        
        public string StartDate { get; set; } = string.Empty;
        
        public string EndDate { get; set; } = string.Empty;
        
        public int? LocationX { get; set; }
        
        public int? LocationY { get; set; }
        
        public int? LocationZ { get; set; }
        
        public long CreatedAt { get; set; }
        
        public List<EventRegistration> Registrations { get; set; } = [];
    }
}
