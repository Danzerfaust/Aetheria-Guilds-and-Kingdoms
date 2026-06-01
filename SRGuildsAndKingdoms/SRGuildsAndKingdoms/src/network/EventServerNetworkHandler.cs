using SRGuildsAndKingdoms.src.database;
using SRGuildsAndKingdoms.src.events;
using System;
using System.Linq;
using Vintagestory.API.Server;

namespace SRGuildsAndKingdoms.src.network
{
    /// <summary>
    /// Server-side network handler for Events
    /// Important: ensure privilege checks are in place for all admin operations
    /// </summary>
    public class EventServerNetworkHandler
    {
        public const string ChannelName = "srguildsandkingdoms:event";
        public const string ManagerPrivilege = "srguildsandkingdoms:eventmanager";

        private ICoreServerAPI? serverApi;
        private EventsRepository? eventsRepository;

        public void InitializeServer(ICoreServerAPI api, EventsRepository eventsRepo)
        {
            serverApi = api;
            eventsRepository = eventsRepo;

            serverApi.Network.RegisterChannel(ChannelName)
                .RegisterMessageType<EventManagerOpenPacket>()
                .RegisterMessageType<EventListRequestPacket>()
                .RegisterMessageType<EventListResponsePacket>()
                .RegisterMessageType<EventDto>()
                .RegisterMessageType<EventRegistrationDto>()
                .SetMessageHandler<EventListRequestPacket>(OnEventListRequest);
        }

        private void OnEventListRequest(IServerPlayer player, EventListRequestPacket packet)
        {
            if (serverApi == null || eventsRepository == null) return;

            if (!player.HasPrivilege(ManagerPrivilege)) return;

            try
            {
                var events = eventsRepository.GetAllEvents();
                var eventDtos = events.Select(MapEventToDto).ToList();

                serverApi.Network.GetChannel(ChannelName)
                    .SendPacket(new EventListResponsePacket
                    {
                        Events = eventDtos
                    }, player);
            }
            catch (Exception ex)
            {
                serverApi.Logger.Error($"[EventServerNetworkHandler] Failed to handle event list request: {ex.Message}");
            }
        }

        private EventDto MapEventToDto(Event eventData)
        {
            return new EventDto
            {
                Id = eventData.Id,
                Name = eventData.Name,
                Description = eventData.Description,
                MaxPlayers = eventData.MaxPlayers,
                StartDate = eventData.StartDate,
                EndDate = eventData.EndDate,
                LocationX = eventData.LocationX,
                LocationY = eventData.LocationY,
                LocationZ = eventData.LocationZ,
                CreatedAt = eventData.CreatedAt,
                Registrations = [.. eventData.Registrations.Select(r => new EventRegistrationDto
                {
                    EventId = r.EventId,
                    RegistreeUid = r.RegistreeUid,
                    RegistrationDate = r.RegistrationDate
                })]
            };
        }
    }
}
