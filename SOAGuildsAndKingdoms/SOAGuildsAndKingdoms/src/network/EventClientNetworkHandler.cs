using System;
using System.Collections.Generic;
using Vintagestory.API.Client;

namespace SOAGuildsAndKingdoms.src.network
{
    public class EventClientNetworkHandler
    {
        private const string ChannelName = "soaguildsandkingdoms:event";

        private ICoreClientAPI? clientApi;

        public Action<List<EventDto>>? OnEventListReceived { get; set; }
        public Action? OnOpenEventManager { get; set; }

        public void InitializeClient(ICoreClientAPI api)
        {
            clientApi = api;

            clientApi.Network.RegisterChannel(ChannelName)
                .RegisterMessageType<EventManagerOpenPacket>()
                .RegisterMessageType<EventListRequestPacket>()
                .RegisterMessageType<EventListResponsePacket>()
                .RegisterMessageType<EventDto>()
                .RegisterMessageType<EventRegistrationDto>()
                .SetMessageHandler<EventListResponsePacket>(OnEventListResponse)
                .SetMessageHandler<EventManagerOpenPacket>(OnEventManagerOpenPacketReceived);
        }

        private void OnEventManagerOpenPacketReceived(EventManagerOpenPacket packet)
        {
            OnOpenEventManager?.Invoke();
        }

        public void RequestEventList()
        {
            if (clientApi == null) return;

            clientApi.Network.GetChannel(ChannelName)
                .SendPacket(new EventListRequestPacket());
        }

        private void OnEventListResponse(EventListResponsePacket packet)
        {
            OnEventListReceived?.Invoke(packet.Events);
        }
    }
}
