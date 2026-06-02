using SOAGuildsAndKingdoms.src.guilds;
using SOAGuildsAndKingdoms.src.network;
using System;
using System.Collections.Generic;
using System.Linq;
using Vintagestory.API.Client;
using Vintagestory.API.Config;

namespace SOAGuildsAndKingdoms.src.gui
{
    /// <summary>
    /// Dialog for viewing all guild members with their roles, online status, and last seen time.
    /// Allows role changes if the player has appropriate permissions.
    /// </summary>
    internal class DialogGuildMembers : GuiDialog
    {
        private SOAGuildsAndKingdomsModSystem modSystem;
        private GuildSummary? currentGuild;
        private List<GuildMemberInfo> members = new();
        private bool canManageRoles = false;
        private bool canKickMembers = false;

        public DialogGuildMembers(ICoreClientAPI capi, SOAGuildsAndKingdomsModSystem modSystem) : base(capi)
        {
            this.modSystem = modSystem;
            currentGuild = modSystem.GetCurrentPlayerGuildSummary();

            if (currentGuild != null)
            {
                // Check if player can manage roles
                canManageRoles = HasManageRolesPermission();

                // Check if player can kick members
                canKickMembers = HasKickPermission();

                // Register callback for member list updates
                var networkHandler = modSystem.GetNetworkHandler();
                networkHandler?.RegisterMemberListCallback(OnMemberListReceived);

                // Request member list from server
                networkHandler?.SendGuildMemberListRequest();
            }

            SetupDialog();
        }

        public override string ToggleKeyCombinationCode => "guildmembers";

        private bool HasManageRolesPermission()
        {
            if (currentGuild == null) return false;

            // Get player's role permissions
            if (currentGuild.Roles.TryGetValue(currentGuild.PlayerRole, out var role))
            {
                return (role.Permissions & GuildPermission.Promote) != 0;
            }

            return false;
        }

        private bool HasKickPermission()
        {
            if (currentGuild == null) return false;

            // Get player's role permissions
            if (currentGuild.Roles.TryGetValue(currentGuild.PlayerRole, out var role))
            {
                return (role.Permissions & GuildPermission.Kick) != 0;
            }

            return false;
        }

        private void OnMemberListReceived(List<GuildMemberInfo> memberList)
        {
            members = memberList;

            // Sort members: online first, then by role (Leader first), then alphabetically
            members = members.OrderByDescending(m => m.IsOnline)
                            .ThenBy(m => m.Role == "Leader" ? 0 : 1)
                            .ThenBy(m => m.PlayerName)
                            .ToList();

            // Refresh dialog if it's open
            if (IsOpened())
            {
                SetupDialog();
            }
        }

        private void SetupDialog()
        {
            if (currentGuild == null)
            {
                SetupNoGuildDialog();
                return;
            }

            ElementBounds dialogBounds = ElementStdBounds.AutosizedMainDialog.WithAlignment(EnumDialogArea.CenterMiddle);
            ElementBounds bgBounds = ElementBounds.Fill.WithFixedPadding(GuiStyle.ElementToDialogPadding);
            bgBounds.BothSizing = ElementSizing.FitToChildren;

            var composer = capi.Gui.CreateCompo("guildmembers", dialogBounds)
                .AddShadedDialogBG(bgBounds)
                .AddDialogTitleBar(Lang.Get("soaguildsandkingdoms:guild-members-title", currentGuild.Name), OnTitleBarCloseClicked)
                .BeginChildElements(bgBounds);

            var top = 20.0;
            var spacing = 8.0;
            var elementHeight = 25.0;
            var dialogWidth = 700.0; // Increased width to accommodate actions column

            // Instructions
            composer.AddStaticText(Lang.Get("soaguildsandkingdoms:guild-members-instructions"),
                CairoFont.WhiteSmallText(), ElementBounds.Fixed(0, top, dialogWidth, elementHeight));
            top += elementHeight + spacing;

            // Header row
            var nameColWidth = 180.0;
            var roleColWidth = 150.0;
            var statusColWidth = 100.0;
            var lastSeenColWidth = 170.0;
            var actionsColWidth = 100.0; // New actions column

            composer.AddStaticText(Lang.Get("soaguildsandkingdoms:member-name"),
                CairoFont.WhiteDetailText(), ElementBounds.Fixed(0, top, nameColWidth, elementHeight));
            composer.AddStaticText(Lang.Get("soaguildsandkingdoms:member-role"),
                CairoFont.WhiteDetailText(), ElementBounds.Fixed(nameColWidth, top, roleColWidth, elementHeight));
            composer.AddStaticText(Lang.Get("soaguildsandkingdoms:member-status"),
                CairoFont.WhiteDetailText(), ElementBounds.Fixed(nameColWidth + roleColWidth, top, statusColWidth, elementHeight));
            composer.AddStaticText(Lang.Get("soaguildsandkingdoms:member-lastseen"),
                CairoFont.WhiteDetailText(), ElementBounds.Fixed(nameColWidth + roleColWidth + statusColWidth, top, lastSeenColWidth, elementHeight));

            // Only show actions column if player has kick permissions
            if (canKickMembers)
            {
                composer.AddStaticText(Lang.Get("soaguildsandkingdoms:member-actions"),
                    CairoFont.WhiteDetailText(), ElementBounds.Fixed(nameColWidth + roleColWidth + statusColWidth + lastSeenColWidth, top, actionsColWidth, elementHeight));
            }

            top += elementHeight + spacing;

            // Member list
            if (members.Count == 0)
            {
                composer.AddStaticText(Lang.Get("soaguildsandkingdoms:loading-members"),
                    CairoFont.WhiteSmallText().WithColor(new double[] { 0.7, 0.7, 0.7, 1.0 }),
                    ElementBounds.Fixed(0, top, dialogWidth, elementHeight * 2));
                top += elementHeight * 3;
            }
            else
            {
                // Scrollable area for members
                var scrollableTop = top;
                var maxVisibleMembers = 10;
                var memberListHeight = Math.Min(members.Count, maxVisibleMembers) * (elementHeight + spacing);

                foreach (var member in members)
                {
                    AddMemberRow(composer, ref top, member, nameColWidth, roleColWidth, statusColWidth, lastSeenColWidth, actionsColWidth, elementHeight, spacing);
                }

                // If we have more members than can fit, the composer will handle scrolling
                if (members.Count > maxVisibleMembers)
                {
                    top = scrollableTop + memberListHeight + spacing * 2;
                }
            }

            // Close button
            composer.AddSmallButton(Lang.Get("soaguildsandkingdoms:close"), OnClose,
                ElementBounds.Fixed(0, top + spacing, 100, elementHeight), EnumButtonStyle.Normal);

            SingleComposer = composer.Compose();
        }

        private void AddMemberRow(GuiComposer composer, ref double top, GuildMemberInfo member,
            double nameColWidth, double roleColWidth, double statusColWidth, double lastSeenColWidth, double actionsColWidth,
            double elementHeight, double spacing)
        {
            // Member name
            var nameColor = member.IsOnline ? CairoFont.WhiteSmallText() : CairoFont.WhiteSmallText().WithColor(new double[] { 0.6, 0.6, 0.6, 1.0 });
            composer.AddStaticText(member.PlayerName, nameColor,
                ElementBounds.Fixed(0, top, nameColWidth, elementHeight));

            // Role - if player can manage roles, show as dropdown, otherwise as text
            if (canManageRoles && member.PlayerUid != capi.World.Player.PlayerUID)
            {
                // Create dropdown with available roles
                var roleNames = currentGuild!.Roles.Keys.ToArray();
                var selectedIndex = Array.IndexOf(roleNames, member.Role);
                if (selectedIndex < 0) selectedIndex = 0;

                composer.AddDropDown(roleNames, roleNames, selectedIndex,
                    (code, selected) => OnRoleChanged(member, code),
                    ElementBounds.Fixed(nameColWidth, top, roleColWidth - 10, elementHeight),
                    $"role_dropdown_{member.PlayerUid}");
            }
            else
            {
                // Show role as text (can't change own role or don't have permission)
                var roleText = member.PlayerUid == capi.World.Player.PlayerUID
                    ? $"{member.Role} (You)"
                    : member.Role;
                composer.AddStaticText(roleText, CairoFont.WhiteSmallText(),
                    ElementBounds.Fixed(nameColWidth, top, roleColWidth, elementHeight));
            }

            // Online status
            var statusText = member.IsOnline
                ? Lang.Get("soaguildsandkingdoms:status-online")
                : Lang.Get("soaguildsandkingdoms:status-offline");
            var statusColor = member.IsOnline
                ? CairoFont.WhiteSmallText().WithColor(new double[] { 0.3, 1.0, 0.3, 1.0 })
                : CairoFont.WhiteSmallText().WithColor(new double[] { 0.7, 0.3, 0.3, 1.0 });
            composer.AddStaticText(statusText, statusColor,
                ElementBounds.Fixed(nameColWidth + roleColWidth, top, statusColWidth, elementHeight));

            // Last seen
            var lastSeen = new DateTime(member.LastSeenTicks);
            var lastSeenText = member.IsOnline
                ? Lang.Get("soaguildsandkingdoms:now")
                : FormatLastSeen(lastSeen);
            composer.AddStaticText(lastSeenText, CairoFont.WhiteSmallText(),
                ElementBounds.Fixed(nameColWidth + roleColWidth + statusColWidth, top, lastSeenColWidth, elementHeight));

            // Actions column (kick button)
            if (canKickMembers)
            {
                // Only show kick button if:
                // 1. The member is not the current player (can't kick yourself)
                // 2. The member is not the guild leader (can't kick the leader)
                bool canKickThisMember = member.PlayerUid != capi.World.Player.PlayerUID && member.Role != "Leader";

                if (canKickThisMember)
                {
                    composer.AddSmallButton(Lang.Get("soaguildsandkingdoms:kick"), () => OnKickMember(member),
                        ElementBounds.Fixed(nameColWidth + roleColWidth + statusColWidth + lastSeenColWidth, top, 60, elementHeight),
                        EnumButtonStyle.Normal, $"kick_btn_{member.PlayerUid}");
                }
            }

            top += elementHeight + spacing;
        }

        private void OnRoleChanged(GuildMemberInfo member, string newRole)
        {
            if (currentGuild == null) return;

            // Send role change request to server
            var networkHandler = modSystem.GetNetworkHandler();
            networkHandler?.SendGuildRoleManagementRequest("assign", newRole, member.PlayerName);

            capi.ShowChatMessage(Lang.Get("soaguildsandkingdoms:role-change-sent", member.PlayerName, newRole));

            // Request updated member list
            networkHandler?.SendGuildMemberListRequest();
        }

        private bool OnKickMember(GuildMemberInfo member)
        {
            if (currentGuild == null) return false;

            // Validate kick operation
            if (member.PlayerUid == capi.World.Player.PlayerUID)
            {
                capi.ShowChatMessage(Lang.Get("soaguildsandkingdoms:cannot-kick-self"));
                return false;
            }

            if (member.Role == "Leader")
            {
                capi.ShowChatMessage(Lang.Get("soaguildsandkingdoms:cannot-kick-leader"));
                return false;
            }

            if (!canKickMembers)
            {
                capi.ShowChatMessage(Lang.Get("soaguildsandkingdoms:no-permission-kick"));
                return false;
            }

            // Show confirmation dialog
            var confirmDialog = new DialogKickConfirm(capi, member.PlayerName, () =>
            {
                // Send kick request to server
                var networkHandler = modSystem.GetNetworkHandler();
                networkHandler?.SendGuildRemoveMemberRequest(member.PlayerName);

                capi.ShowChatMessage(Lang.Get("soaguildsandkingdoms:kick-request-sent", member.PlayerName));

                // Request updated member list
                networkHandler?.SendGuildMemberListRequest();
            });

            confirmDialog.TryOpen();
            return true;
        }

        private string FormatLastSeen(DateTime lastSeen)
        {
            var now = DateTime.UtcNow;
            var timeSpan = now - lastSeen;

            if (timeSpan.TotalMinutes < 1)
                return Lang.Get("soaguildsandkingdoms:just-now");
            if (timeSpan.TotalMinutes < 60)
                return Lang.Get("soaguildsandkingdoms:minutes-ago", (int)timeSpan.TotalMinutes);
            if (timeSpan.TotalHours < 24)
                return Lang.Get("soaguildsandkingdoms:hours-ago", (int)timeSpan.TotalHours);
            if (timeSpan.TotalDays < 7)
                return Lang.Get("soaguildsandkingdoms:days-ago", (int)timeSpan.TotalDays);
            if (timeSpan.TotalDays < 30)
                return Lang.Get("soaguildsandkingdoms:weeks-ago", (int)(timeSpan.TotalDays / 7));

            return lastSeen.ToString("yyyy-MM-dd");
        }

        private void SetupNoGuildDialog()
        {
            ElementBounds dialogBounds = ElementStdBounds.AutosizedMainDialog.WithAlignment(EnumDialogArea.CenterMiddle);
            ElementBounds bgBounds = ElementBounds.Fill.WithFixedPadding(GuiStyle.ElementToDialogPadding);
            bgBounds.BothSizing = ElementSizing.FitToChildren;

            var composer = capi.Gui.CreateCompo("guildmembers", dialogBounds)
                .AddShadedDialogBG(bgBounds)
                .AddDialogTitleBar(Lang.Get("soaguildsandkingdoms:guild-members-title", "None"), OnTitleBarCloseClicked)
                .BeginChildElements(bgBounds);

            composer.AddStaticText(Lang.Get("soaguildsandkingdoms:not-in-guild"),
                CairoFont.WhiteDetailText(), ElementBounds.Fixed(0, 0, 400, 50));

            composer.AddSmallButton(Lang.Get("soaguildsandkingdoms:close"), OnClose,
                ElementBounds.Fixed(0, 60, 100, 25), EnumButtonStyle.Normal);

            SingleComposer = composer.Compose();
        }

        private bool OnClose()
        {
            TryClose();
            return true;
        }

        private void OnTitleBarCloseClicked()
        {
            TryClose();
        }

        public override void OnGuiClosed()
        {
            base.OnGuiClosed();

            // Unregister callback when dialog is closed
            var networkHandler = modSystem.GetNetworkHandler();
            networkHandler?.UnregisterMemberListCallback();
        }

        public override bool PrefersUngrabbedMouse => false;
    }

    /// <summary>
    /// Confirmation dialog for kicking a guild member
    /// </summary>
    internal class DialogKickConfirm : GuiDialog
    {
        private string memberName;
        private Action onConfirm;

        public DialogKickConfirm(ICoreClientAPI capi, string memberName, Action onConfirm) : base(capi)
        {
            this.memberName = memberName;
            this.onConfirm = onConfirm;
            SetupDialog();
        }

        public override string ToggleKeyCombinationCode => "guildkickconfirm";

        private void SetupDialog()
        {
            ElementBounds dialogBounds = ElementStdBounds.AutosizedMainDialog.WithAlignment(EnumDialogArea.CenterMiddle);
            ElementBounds bgBounds = ElementBounds.Fill.WithFixedPadding(GuiStyle.ElementToDialogPadding);
            bgBounds.BothSizing = ElementSizing.FitToChildren;

            var composer = capi.Gui.CreateCompo("guildkickconfirm", dialogBounds)
                .AddShadedDialogBG(bgBounds)
                .AddDialogTitleBar(Lang.Get("soaguildsandkingdoms:kick-member-title"), OnTitleBarCloseClicked)
                .BeginChildElements(bgBounds);

            var top = 20.0;
            var spacing = 15.0;
            var elementHeight = 25.0;
            var width = 400.0;

            // Message
            var message = Lang.Get("soaguildsandkingdoms:confirm-kick-member", memberName);
            composer.AddStaticText(message, CairoFont.WhiteDetailText(),
                ElementBounds.Fixed(0, top, width, elementHeight * 3));
            top += elementHeight * 3 + spacing;

            // Buttons
            composer.AddSmallButton(Lang.Get("soaguildsandkingdoms:kick"), OnConfirmClick,
                ElementBounds.Fixed(0, top, 100, elementHeight), EnumButtonStyle.MainMenu);

            composer.AddSmallButton(Lang.Get("soaguildsandkingdoms:cancel"), OnCancelClick,
                ElementBounds.Fixed(110, top, 100, elementHeight), EnumButtonStyle.Normal);

            SingleComposer = composer.Compose();
        }

        private bool OnConfirmClick()
        {
            onConfirm?.Invoke();
            TryClose();
            return true;
        }

        private bool OnCancelClick()
        {
            TryClose();
            return true;
        }

        private void OnTitleBarCloseClicked()
        {
            TryClose();
        }

        public override bool PrefersUngrabbedMouse => false;
    }
}
