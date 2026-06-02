using SOAGuildsAndKingdoms.src.network;
using System;
using System.Collections.Generic;
using Vintagestory.API.Client;
using Vintagestory.API.Config;

namespace SOAGuildsAndKingdoms.src.gui
{
    /// <summary>
    /// HUD element that appears in the bottom-right when a player receives a guild invite
    /// </summary>
    public class DialogGuildInvitePopup : HudElement
    {
        private SOAGuildsAndKingdomsModSystem modSystem;
        private List<GuildInviteInfo> currentInvites = new();
        private int currentInviteIndex = 0;
        private long updateListenerId = 0;
        private DateTime lastComposeTime = DateTime.MinValue;

        public override string ToggleKeyCombinationCode => null;

        public DialogGuildInvitePopup(ICoreClientAPI capi, SOAGuildsAndKingdomsModSystem modSystem) : base(capi)
        {
            this.modSystem = modSystem;
        }

        /// <summary>
        /// Show a guild invite notification
        /// </summary>
        public void ShowInvite(GuildInviteNotificationPacket invite)
        {
            // Debug logging
            var expiresAt = new DateTime(invite.ExpiresAtTicks, DateTimeKind.Utc);
            var remaining = expiresAt - DateTime.UtcNow;
            capi.Logger.Notification($"[InvitePopup] Received invite to {invite.GuildName} from {invite.InviterName}");
            capi.Logger.Notification($"[InvitePopup] ExpiresAtTicks: {invite.ExpiresAtTicks}");
            capi.Logger.Notification($"[InvitePopup] ExpiresAt: {expiresAt}");
            capi.Logger.Notification($"[InvitePopup] Current UTC: {DateTime.UtcNow}");
            capi.Logger.Notification($"[InvitePopup] Remaining: {remaining.TotalSeconds} seconds");

            var inviteInfo = new GuildInviteInfo
            {
                GuildName = invite.GuildName,
                InviterName = invite.InviterName,
                InviterUid = invite.InviterUid,
                ExpiresAtTicks = invite.ExpiresAtTicks
            };

            // Check if this invite already exists
            bool exists = currentInvites.Exists(i =>
                i.GuildName == inviteInfo.GuildName &&
                i.InviterUid == inviteInfo.InviterUid);

            if (!exists)
            {
                currentInvites.Add(inviteInfo);
            }

            // Show the first invite if not visible
            if (!IsOpened())
            {
                currentInviteIndex = 0;
                ComposeDialog();
                TryOpen();
                SetupUpdateListener();
            }
            else
            {
                // Only recompose if this is a new invite (not just a refresh)
                // The update listener will handle expired invites
            }
        }

        /// <summary>
        /// Show multiple invites at once
        /// </summary>
        public void ShowInvites(List<GuildInviteInfo> invites)
        {
            currentInvites.Clear();

            // Filter out expired invites
            var now = DateTime.UtcNow;
            foreach (var invite in invites)
            {
                var expiresAt = new DateTime(invite.ExpiresAtTicks, DateTimeKind.Utc);
                if (expiresAt > now)
                {
                    currentInvites.Add(invite);
                }
            }

            if (currentInvites.Count > 0)
            {
                currentInviteIndex = 0;
                ComposeDialog();
                TryOpen();
                SetupUpdateListener();
            }
            else
            {
                capi.ShowChatMessage(Lang.Get("soaguildsandkingdoms:no-pending-invites"));
            }
        }

        private void ComposeDialog()
        {
            // Remove expired and invalid invites
            RemoveExpiredInvites();

            if (currentInvites.Count == 0)
            {
                TryClose();
                return;
            }

            // Ensure currentInviteIndex is valid
            if (currentInviteIndex >= currentInvites.Count)
            {
                currentInviteIndex = currentInvites.Count - 1;
            }
            if (currentInviteIndex < 0)
            {
                currentInviteIndex = 0;
            }

            var currentInvite = currentInvites[currentInviteIndex];

            // Validate ExpiresAtTicks
            if (currentInvite.ExpiresAtTicks <= 0)
            {
                capi.Logger.Error($"[InvitePopup] Invalid ExpiresAtTicks: {currentInvite.ExpiresAtTicks} for guild {currentInvite.GuildName}");
                currentInvites.RemoveAt(currentInviteIndex);
                if (currentInvites.Count > 0)
                {
                    ComposeDialog(); // Recursive call is safe here - only happens on invalid data
                }
                else
                {
                    TryClose();
                }
                return;
            }

            var expiresAt = new DateTime(currentInvite.ExpiresAtTicks, DateTimeKind.Utc);
            var timeRemaining = expiresAt - DateTime.UtcNow;

            // Handle expired invites
            if (timeRemaining.TotalSeconds <= 0)
            {
                capi.Logger.Warning($"[InvitePopup] Invite already expired: {currentInvite.GuildName}");
                currentInvites.RemoveAt(currentInviteIndex);
                if (currentInvites.Count > 0)
                {
                    ComposeDialog(); // Recursive call is safe here - only happens when invite expires
                }
                else
                {
                    TryClose();
                }
                return;
            }

            lastComposeTime = DateTime.UtcNow;

            string timeText;
            if (timeRemaining.TotalMinutes < 1)
            {
                timeText = Lang.Get("soaguildsandkingdoms:invite-expires-seconds", (int)timeRemaining.TotalSeconds);
            }
            else
            {
                timeText = Lang.Get("soaguildsandkingdoms:invite-expires-minutes", (int)timeRemaining.TotalMinutes);
            }

            // Calculate dialog bounds - bottom right corner
            double width = 420;
            double height = 250;
            double rightMargin = 30;
            double bottomMargin = 30;

            ElementBounds dialogBounds = ElementBounds.Fixed(
                EnumDialogArea.RightBottom,
                rightMargin,
                bottomMargin,
                width,
                height
            );

            ElementBounds bgBounds = ElementBounds.Fill.WithFixedPadding(GuiStyle.ElementToDialogPadding);
            bgBounds.BothSizing = ElementSizing.FitToChildren;

            // Dispose old composer safely
            try
            {
                SingleComposer?.Dispose();
            }
            catch (Exception ex)
            {
                capi.Logger.Warning($"[InvitePopup] Error disposing composer: {ex.Message}");
            }

            var composer = capi.Gui.CreateCompo("guildinvitepopup", dialogBounds)
                .AddShadedDialogBG(bgBounds, true)
                .AddDialogTitleBar(Lang.Get("soaguildsandkingdoms:guild-invite-title"), OnCloseClicked)
                .BeginChildElements(bgBounds);

            // Main content area - start with message text
            var messageBounds = ElementBounds.Fixed(0, 30, width - 50, 40);
            var timeBounds = messageBounds.BelowCopy(0, 5).WithFixedHeight(20);
            var keyHintBounds = timeBounds.BelowCopy(0, 5).WithFixedHeight(20);

            composer
                .AddStaticText(
                    Lang.Get("soaguildsandkingdoms:guild-invite-message", currentInvite.GuildName, currentInvite.InviterName),
                    CairoFont.WhiteSmallText(),
                    messageBounds
                )
                .AddStaticText(
                    timeText,
                    CairoFont.WhiteSmallText().WithColor(GuiStyle.WarningTextColor),
                    timeBounds
                )
                .AddStaticText(
                    Lang.Get("soaguildsandkingdoms:press-key-to-interact", capi.Input.GetHotKeyByCode("togglemousecontrol")?.CurrentMapping?.ToString() ?? "NOT SET"),
                    CairoFont.WhiteSmallText().WithColor(new double[] { 0.7, 0.7, 0.7, 1.0 }),
                    keyHintBounds
                );

            // Show invite count if multiple
            if (currentInvites.Count > 1)
            {
                var countBounds = keyHintBounds.BelowCopy(0, 5).WithFixedHeight(20);
                composer.AddStaticText(
                    Lang.Get("soaguildsandkingdoms:invite-count", currentInviteIndex + 1, currentInvites.Count),
                    CairoFont.WhiteSmallText(),
                    countBounds
                );
            }

            // Buttons
            var buttonY = height - 100;
            var buttonWidth = 110;
            var buttonSpacing = 15;

            var acceptBounds = ElementBounds.Fixed(10, buttonY, buttonWidth, 30);
            var declineBounds = ElementBounds.Fixed(10 + buttonWidth + buttonSpacing, buttonY, buttonWidth, 30);

            composer
                .AddSmallButton(
                    Lang.Get("soaguildsandkingdoms:accept"),
                    OnAcceptClicked,
                    acceptBounds,
                    EnumButtonStyle.Normal
                )
                .AddSmallButton(
                    Lang.Get("soaguildsandkingdoms:decline"),
                    OnDeclineClicked,
                    declineBounds,
                    EnumButtonStyle.Normal
                );

            // Navigation buttons if multiple invites
            if (currentInvites.Count > 1)
            {
                var prevBounds = ElementBounds.Fixed(10 + (buttonWidth + buttonSpacing) * 2, buttonY, 30, 30);
                var nextBounds = ElementBounds.Fixed(10 + (buttonWidth + buttonSpacing) * 2 + 40, buttonY, 30, 30);

                composer
                    .AddSmallButton("<", OnPreviousClicked, prevBounds, EnumButtonStyle.Normal)
                    .AddSmallButton(">", OnNextClicked, nextBounds, EnumButtonStyle.Normal);
            }

            composer.EndChildElements().Compose();
            SingleComposer = composer;
        }

        private void RemoveExpiredInvites()
        {
            var now = DateTime.UtcNow;
            currentInvites.RemoveAll(invite =>
            {
                var expiresAt = new DateTime(invite.ExpiresAtTicks, DateTimeKind.Utc);
                return expiresAt <= now;
            });
        }

        private bool OnAcceptClicked()
        {
            if (currentInvites.Count == 0) return true;

            var invite = currentInvites[currentInviteIndex];

            // Send accept packet
            var packet = new GuildAcceptInvitePacket
            {
                PlayerUid = capi.World.Player.PlayerUID
            };

            var channel = capi.Network.GetChannel("soaguildsandkingdoms:guild");
            channel.SendPacket(packet);

            // Remove this invite from the list
            currentInvites.RemoveAt(currentInviteIndex);

            // Adjust index if needed
            if (currentInviteIndex >= currentInvites.Count && currentInviteIndex > 0)
            {
                currentInviteIndex--;
            }

            // Refresh or close
            if (currentInvites.Count > 0)
            {
                ComposeDialog();
            }
            else
            {
                TryClose();
            }

            return true;
        }

        private bool OnDeclineClicked()
        {
            if (currentInvites.Count == 0) return true;

            var invite = currentInvites[currentInviteIndex];

            // Send decline packet
            var packet = new GuildDeclineInvitePacket
            {
                PlayerUid = capi.World.Player.PlayerUID,
                GuildName = invite.GuildName
            };

            var channel = capi.Network.GetChannel("soaguildsandkingdoms:guild");
            channel.SendPacket(packet);

            // Remove this invite from the list
            currentInvites.RemoveAt(currentInviteIndex);

            // Adjust index if needed
            if (currentInviteIndex >= currentInvites.Count && currentInviteIndex > 0)
            {
                currentInviteIndex--;
            }

            // Refresh or close
            if (currentInvites.Count > 0)
            {
                ComposeDialog();
            }
            else
            {
                TryClose();
            }

            return true;
        }

        private bool OnPreviousClicked()
        {
            if (currentInvites.Count <= 1) return true;

            currentInviteIndex--;
            if (currentInviteIndex < 0)
            {
                currentInviteIndex = currentInvites.Count - 1;
            }

            ComposeDialog();
            return true;
        }

        private bool OnNextClicked()
        {
            if (currentInvites.Count <= 1) return true;

            currentInviteIndex++;
            if (currentInviteIndex >= currentInvites.Count)
            {
                currentInviteIndex = 0;
            }

            ComposeDialog();
            return true;
        }

        private void OnCloseClicked()
        {
            TryClose();
        }

        private void SetupUpdateListener()
        {
            // Remove existing listener if any
            if (updateListenerId != 0)
            {
                capi.Event.UnregisterGameTickListener(updateListenerId);
                updateListenerId = 0;
            }

            // Check for expired invites every 10 seconds (only recompose if needed)
            updateListenerId = capi.Event.RegisterGameTickListener((dt) =>
            {
                if (!IsOpened())
                {
                    // Stop updating when closed
                    if (updateListenerId != 0)
                    {
                        capi.Event.UnregisterGameTickListener(updateListenerId);
                        updateListenerId = 0;
                    }
                    return;
                }

                // Only check if invites have expired, don't constantly recompose
                RemoveExpiredInvites();

                if (currentInvites.Count == 0)
                {
                    TryClose();
                }
                else if (currentInviteIndex >= currentInvites.Count)
                {
                    // Current invite was removed, need to recompose
                    currentInviteIndex = currentInvites.Count - 1;
                    ComposeDialog();
                }
                // Only recompose every 30 seconds to update the time display (much less aggressive)
                else if ((DateTime.UtcNow - lastComposeTime).TotalSeconds > 30)
                {
                    ComposeDialog();
                }
            }, 10000); // Check every 10 seconds
        }

        public override void OnGuiClosed()
        {
            base.OnGuiClosed();

            // Clean up listener
            if (updateListenerId != 0)
            {
                capi.Event.UnregisterGameTickListener(updateListenerId);
                updateListenerId = 0;
            }
        }

        public override void Dispose()
        {
            base.Dispose();

            if (updateListenerId != 0)
            {
                capi.Event.UnregisterGameTickListener(updateListenerId);
                updateListenerId = 0;
            }
        }
    }
}
