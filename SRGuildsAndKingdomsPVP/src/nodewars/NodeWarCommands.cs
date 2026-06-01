using System;
using System.Linq;
using System.Text;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Server;
using Vintagestory.API.Client;
using SRGuildsAndKingdoms;
using SRGuildsAndKingdomsPVP.src.gui;
using SRGuildsAndKingdomsPVP.src.network;

namespace SRGuildsAndKingdomsPVP.src.nodewars
{
    /// <summary>
    /// Chat commands for the Node Wars system
    /// </summary>
    public class NodeWarCommands
    {
        private readonly ICoreServerAPI sapi;
        private readonly ICoreAPI api;
        private readonly NodeWarManager nodeWarManager;
        private readonly NodeWarGuildIntegration guildIntegration;
        private readonly SRGuildsAndKingdomsModSystem guildModSystem;

        public NodeWarCommands(ICoreAPI coreApi, NodeWarManager manager, SRGuildsAndKingdomsModSystem guildMod)
        {
            api = coreApi;
            sapi = coreApi as ICoreServerAPI;
            nodeWarManager = manager;
            guildModSystem = guildMod;
            if (sapi != null)
            {
                guildIntegration = new NodeWarGuildIntegration(sapi, manager, guildMod);
            }
        }

        /// <summary>
        /// Register all node war commands
        /// </summary>
        public void RegisterCommands()
        {
            // Player commands have been moved to the Guild UI - Node Wars tab
            // Admin UI is registered in PVPModSystem for proper client/server handling

            // This class is kept for future command extensions if needed
        }
    }
}
