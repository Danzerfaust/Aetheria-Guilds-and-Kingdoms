using SRGuildsAndKingdoms.src.guilds;
using Vintagestory.API.Client;
using Vintagestory.API.Common;

namespace SRGuildsAndKingdoms.src.gui.tabs
{
    public class GuildMembersTab : GuildTabContent
    {
        private readonly ActionConsumable onViewMembers;
        private readonly ActionConsumable onInvitePlayer;
        private readonly ActionConsumable onManageRoles;
        private readonly ActionConsumable onManagePendingInvites;

        public GuildMembersTab(ICoreClientAPI capi, SRGuildsAndKingdomsModSystem modSystem,
            GuildSummary? currentGuild, ActionConsumable onViewMembers,
            ActionConsumable onInvitePlayer, ActionConsumable onManageRoles,
            ActionConsumable onManagePendingInvites)
            : base(capi, modSystem, currentGuild)
        {
            this.onViewMembers = onViewMembers;
            this.onInvitePlayer = onInvitePlayer;
            this.onManageRoles = onManageRoles;
            this.onManagePendingInvites = onManagePendingInvites;
        }

        public override double AddContent(GuiComposer composer, double startTop)
        {
            if (currentGuild == null) return startTop;

            var top = startTop;
            var elementHeight = 25.0;
            var spacing = 10.0;

            composer.AddStaticText("Guild Members:", CairoFont.WhiteMediumText(),
                ElementBounds.Fixed(0, top, 400, elementHeight));
            top += elementHeight + spacing;

            composer.AddStaticText($"Total Members: {currentGuild.MemberCount}", CairoFont.WhiteSmallText(),
                ElementBounds.Fixed(0, top, 200, elementHeight));
            top += elementHeight + spacing;

            // Show pending invites count if player has invite permissions
            if (HasInvitePermissions() && currentGuild.PendingInvites != null)
            {
                composer.AddStaticText($"Pending Invites: {currentGuild.PendingInvites.Count}", CairoFont.WhiteSmallText(),
                    ElementBounds.Fixed(0, top, 200, elementHeight));
                top += elementHeight + spacing;
            }

            // Action buttons
            composer.AddSmallButton("View Members", onViewMembers,
                ElementBounds.Fixed(0, top, 120, elementHeight), EnumButtonStyle.Normal);

            if (HasInvitePermissions())
            {
                composer.AddSmallButton("Invite Player", onInvitePlayer,
                    ElementBounds.Fixed(130, top, 120, elementHeight), EnumButtonStyle.Normal);

                composer.AddSmallButton("Manage Invites", onManagePendingInvites,
                    ElementBounds.Fixed(260, top, 120, elementHeight), EnumButtonStyle.Normal);
            }

            top += elementHeight + spacing;

            if (HasManagePermissions())
            {
                composer.AddSmallButton("Manage Roles", onManageRoles,
                    ElementBounds.Fixed(0, top, 120, elementHeight), EnumButtonStyle.Normal);
            }

            return top + elementHeight;
        }
    }
}