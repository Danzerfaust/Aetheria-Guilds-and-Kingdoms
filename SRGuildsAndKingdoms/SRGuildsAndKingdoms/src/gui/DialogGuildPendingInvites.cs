using SRGuildsAndKingdoms.src.guilds;
using System;
using System.Collections.Generic;
using System.Linq;
using Vintagestory.API.Client;
using Vintagestory.API.Config;

namespace SRGuildsAndKingdoms.src.gui
{
    /// <summary>
    /// Dialog for managing pending guild invites.
    /// Allows guild members with invite permissions to view and cancel pending invites.
    /// </summary>
    public class DialogGuildPendingInvites : GuiDialog
    {
        private SRGuildsAndKingdomsModSystem modSystem;
        private GuildSummary? currentGuild;
        private List<PendingInviteInfo> pendingInvites = new List<PendingInviteInfo>();

        private class PendingInviteInfo
        {
            public string InviteeUid { get; set; } = "";
            public string InviteeNameDisplay { get; set; } = "";
            public string InviterName { get; set; } = "";
            public DateTime ExpiresAt { get; set; }

            public string GetTimeRemaining()
            {
                var remaining = ExpiresAt - DateTime.UtcNow;
                if (remaining.TotalSeconds <= 0)
                    return "Expired";

                if (remaining.TotalMinutes < 1)
                    return $"{(int)remaining.TotalSeconds}s";

                return $"{(int)remaining.TotalMinutes}m {remaining.Seconds}s";
            }
        }

        public DialogGuildPendingInvites(ICoreClientAPI capi, SRGuildsAndKingdomsModSystem modSystem) : base(capi)
        {
            this.modSystem = modSystem;
            LoadGuildData();
            SetupDialog();
        }

        public override string ToggleKeyCombinationCode => "guildpendinginvites";

        private void LoadGuildData()
        {
            currentGuild = modSystem.GetCurrentPlayerGuildSummary();

            if (currentGuild == null)
                return;

            // Load pending invites from the guild summary
            pendingInvites.Clear();

            foreach (var invite in currentGuild.PendingInvites)
            {
                // Get inviter name
                var inviterName = GetPlayerName(invite.InviterUid);

                // Get invitee name - try to get from player data
                var inviteeName = GetPlayerName(invite.InviteeUid);

                pendingInvites.Add(new PendingInviteInfo
                {
                    InviteeUid = invite.InviteeUid,
                    InviteeNameDisplay = inviteeName,
                    InviterName = inviterName,
                    ExpiresAt = invite.ExpiresAt
                });
            }
        }

        private string GetPlayerName(string playerUid)
        {
            // Try to get player name from online players first
            var onlinePlayer = capi.World.AllOnlinePlayers.FirstOrDefault(p => p.PlayerUID == playerUid);
            if (onlinePlayer != null)
                return onlinePlayer.PlayerName;

            // If not online, show UID (we don't have access to offline player data on client)
            return playerUid.Length > 8 ? playerUid.Substring(0, 8) + "..." : playerUid;
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

            var composer = capi.Gui.CreateCompo("guildpendinginvites", dialogBounds)
                .AddShadedDialogBG(bgBounds)
                .AddDialogTitleBar(Lang.Get("srguildsandkingdoms:pending-invites-title"), OnTitleBarCloseClicked)
                .BeginChildElements(bgBounds);

            var top = 20.0;
            var elementHeight = 25.0;
            var spacing = 10.0;
            var width = 600.0;

            // Header
            composer.AddStaticText(Lang.Get("srguildsandkingdoms:pending-invites-header", pendingInvites.Count),
                CairoFont.WhiteMediumText(),
                ElementBounds.Fixed(0, top, width, elementHeight));
            top += elementHeight + spacing;

            if (pendingInvites.Count == 0)
            {
                composer.AddStaticText(Lang.Get("srguildsandkingdoms:no-pending-invites"),
                    CairoFont.WhiteSmallText(),
                    ElementBounds.Fixed(0, top, width, elementHeight));
                top += elementHeight + spacing;
            }
            else
            {
                // Column headers
                composer.AddStaticText("Invited Player", CairoFont.WhiteSmallText(),
                    ElementBounds.Fixed(0, top, 150, elementHeight));
                composer.AddStaticText("Invited By", CairoFont.WhiteSmallText(),
                    ElementBounds.Fixed(160, top, 150, elementHeight));
                composer.AddStaticText("Time Remaining", CairoFont.WhiteSmallText(),
                    ElementBounds.Fixed(320, top, 120, elementHeight));
                composer.AddStaticText("Action", CairoFont.WhiteSmallText(),
                    ElementBounds.Fixed(450, top, 100, elementHeight));
                top += elementHeight + 5;

                // List each pending invite
                for (int i = 0; i < pendingInvites.Count; i++)
                {
                    var invite = pendingInvites[i];
                    var rowTop = top + (i * (elementHeight + 5));

                    // Invitee name
                    composer.AddStaticText(invite.InviteeNameDisplay, CairoFont.WhiteDetailText(),
                        ElementBounds.Fixed(0, rowTop, 150, elementHeight));

                    // Inviter name
                    composer.AddStaticText(invite.InviterName, CairoFont.WhiteDetailText(),
                        ElementBounds.Fixed(160, rowTop, 150, elementHeight));

                    // Time remaining
                    var timeRemaining = invite.GetTimeRemaining();
                    var timeColor = invite.ExpiresAt <= DateTime.UtcNow ? CairoFont.WhiteDetailText().WithColor(GuiStyle.ErrorTextColor) : CairoFont.WhiteDetailText();
                    composer.AddStaticText(timeRemaining, timeColor,
                        ElementBounds.Fixed(320, rowTop, 120, elementHeight));

                    // Cancel button
                    var inviteIndex = i; // Capture for closure
                    composer.AddSmallButton(Lang.Get("srguildsandkingdoms:cancel-invite"),
                        () => OnCancelInvite(invite.InviteeUid),
                        ElementBounds.Fixed(450, rowTop, 100, elementHeight),
                        EnumButtonStyle.Normal,
                        $"cancelinvite_{inviteIndex}");
                }

                top += pendingInvites.Count * (elementHeight + 5) + spacing;
            }

            // Refresh button
            composer.AddSmallButton(Lang.Get("srguildsandkingdoms:refresh-invites"),
                OnRefresh,
                ElementBounds.Fixed(0, top, 120, elementHeight),
                EnumButtonStyle.Normal);

            // Close button
            composer.AddSmallButton(Lang.Get("srguildsandkingdoms:close"),
                OnClose,
                ElementBounds.Fixed(130, top, 120, elementHeight),
                EnumButtonStyle.Normal);

            SingleComposer = composer.Compose();
        }

        private void SetupNoGuildDialog()
        {
            ElementBounds dialogBounds = ElementStdBounds.AutosizedMainDialog.WithAlignment(EnumDialogArea.CenterMiddle);
            ElementBounds bgBounds = ElementBounds.Fill.WithFixedPadding(GuiStyle.ElementToDialogPadding);
            bgBounds.BothSizing = ElementSizing.FitToChildren;

            var composer = capi.Gui.CreateCompo("guildpendinginvites", dialogBounds)
                .AddShadedDialogBG(bgBounds)
                .AddDialogTitleBar(Lang.Get("srguildsandkingdoms:pending-invites-title"), OnTitleBarCloseClicked)
                .BeginChildElements(bgBounds);

            composer.AddStaticText(Lang.Get("srguildsandkingdoms:not-in-guild"),
                CairoFont.WhiteDetailText(),
                ElementBounds.Fixed(0, 20, 400, 50));

            SingleComposer = composer.Compose();
        }

        private bool OnCancelInvite(string inviteeUid)
        {
            var networkHandler = modSystem.GetNetworkHandler();
            if (networkHandler == null)
            {
                capi.ShowChatMessage(Lang.Get("srguildsandkingdoms:network-error"));
                return true;
            }

            // Send cancel invite request to server
            networkHandler.SendCancelInviteRequest(inviteeUid);
            capi.ShowChatMessage(Lang.Get("srguildsandkingdoms:invite-cancel-requested"));

            // Refresh the dialog after a short delay to allow server to process
            capi.Event.EnqueueMainThreadTask(() =>
            {
                System.Threading.Thread.Sleep(500); // Wait for server response
                LoadGuildData();
                SetupDialog();
            }, "refreshinvites");

            return true;
        }

        private bool OnRefresh()
        {
            LoadGuildData();
            SetupDialog();
            capi.ShowChatMessage(Lang.Get("srguildsandkingdoms:invites-refreshed"));
            return true;
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

        public override bool PrefersUngrabbedMouse => false;
    }
}
