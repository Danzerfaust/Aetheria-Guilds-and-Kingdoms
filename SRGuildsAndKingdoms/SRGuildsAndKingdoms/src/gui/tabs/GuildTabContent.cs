using SRGuildsAndKingdoms.src.guilds;
using Vintagestory.API.Client;

namespace SRGuildsAndKingdoms.src.gui.tabs
{
    /// <summary>
    /// Base class for guild dialog tab content
    /// </summary>
    public abstract class GuildTabContent
    {
        protected ICoreClientAPI capi;
        protected SRGuildsAndKingdomsModSystem modSystem;
        protected GuildSummary? currentGuild;

        public GuildTabContent(ICoreClientAPI capi, SRGuildsAndKingdomsModSystem modSystem, GuildSummary? currentGuild)
        {
            this.capi = capi;
            this.modSystem = modSystem;
            this.currentGuild = currentGuild;
        }

        /// <summary>
        /// Requests updated guild data from the server.
        /// Note: This triggers a server-side guild sync, which will update all clients.
        /// Override OnGuildDataRefreshed() in your tab to handle the updated data.
        /// </summary>
        protected void RequestGuildRefresh()
        {
            // The guild summaries are automatically refreshed when guild data changes on the server
            // This method can be called after actions that modify guild data
            // The actual refresh happens through the OnGuildsChanged event on the server
            // which broadcasts updated summaries to all clients

            // For immediate feedback, you can also manually refresh from cached data in modSystem
            if (currentGuild != null && capi.World.Player != null)
            {
                // Get the latest cached guild summary
                var updatedGuild = GetLatestGuildSummary(currentGuild.Name);
                if (updatedGuild != null)
                {
                    Refresh(updatedGuild);
                    OnGuildDataRefreshed(updatedGuild);
                }
            }
        }

        /// <summary>
        /// Gets the latest guild summary from the mod system's cached data
        /// </summary>
        private GuildSummary? GetLatestGuildSummary(string guildName)
        {
            // Access the mod system's cached guild summaries
            return modSystem.GetCachedGuildSummary(guildName) ?? currentGuild;
        }

        /// <summary>
        /// Called when guild data has been refreshed. Override in derived classes to handle updates.
        /// </summary>
        protected virtual void OnGuildDataRefreshed(GuildSummary updatedGuild)
        {
            // Base implementation does nothing - override in derived classes
        }

        /// <summary>
        /// Add the tab's content to the provided composer starting at the given Y position
        /// </summary>
        /// <param name="composer">The GuiComposer to add content to</param>
        /// <param name="startTop">The Y position to start at</param>
        /// <returns>The final Y position after adding all content</returns>
        public abstract double AddContent(GuiComposer composer, double startTop);

        /// <summary>
        /// Called when the tab needs to refresh its content (e.g., guild data changed)
        /// </summary>
        public virtual void Refresh(GuildSummary? updatedGuild)
        {
            this.currentGuild = updatedGuild;
        }

        /// <summary>
        /// Check if the current player has manage permissions
        /// </summary>
        protected bool HasManagePermissions()
        {
            if (currentGuild == null || string.IsNullOrEmpty(currentGuild.PlayerRole))
                return false;

            if (currentGuild.Roles.TryGetValue(currentGuild.PlayerRole, out var role))
            {
                return role.Permissions.HasFlag(GuildPermission.ManageRoles);
            }

            return false;
        }

        /// <summary>
        /// Check if the current player has invite permissions
        /// </summary>
        protected bool HasInvitePermissions()
        {
            if (currentGuild == null || string.IsNullOrEmpty(currentGuild.PlayerRole))
                return false;

            if (currentGuild.Roles.TryGetValue(currentGuild.PlayerRole, out var role))
            {
                return role.Permissions.HasFlag(GuildPermission.Invite);
            }

            return false;
        }

        /// <summary>
        /// Check if the current player is the guild leader (hierarchy level 1)
        /// </summary>
        protected bool IsLeader()
        {
            if (currentGuild == null || string.IsNullOrEmpty(currentGuild.PlayerRole))
                return false;

            if (currentGuild.Roles.TryGetValue(currentGuild.PlayerRole, out var role))
            {
                return role.Hierarchy == 1;
            }

            return false;
        }

        /// <summary>
        /// Convert ARGB color to hex string
        /// </summary>
        protected string ColorToHex(int argbColor)
        {
            uint color = unchecked((uint)argbColor);
            byte r = (byte)((color >> 16) & 0xFF);
            byte g = (byte)((color >> 8) & 0xFF);
            byte b = (byte)(color & 0xFF);
            return $"#{r:X2}{g:X2}{b:X2}";
        }
    }
}