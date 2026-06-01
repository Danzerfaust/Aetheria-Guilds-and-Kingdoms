using SRGuildsAndKingdoms.src.guilds;
using SRGuildsAndKingdoms.src.network;
using System;
using System.Collections.Generic;
using System.Linq;
using Vintagestory.API.Client;

namespace SRGuildsAndKingdoms.src.gui
{
    public class DialogTransferOwnership : GuiDialog
    {
        private SRGuildsAndKingdomsModSystem modSystem;
        private GuildSummary currentGuild;
        private List<GuildMemberInfo> members;
        private string? selectedMemberUid;
        private Action onTransferComplete;

        public override string ToggleKeyCombinationCode => "transferownership";

        public DialogTransferOwnership(ICoreClientAPI capi, SRGuildsAndKingdomsModSystem modSystem,
            GuildSummary currentGuild, Action onTransferComplete) : base(capi)
        {
            this.modSystem = modSystem;
            this.currentGuild = currentGuild;
            this.onTransferComplete = onTransferComplete;
            this.members = new List<GuildMemberInfo>();

            // Request member list from server
            var networkHandler = modSystem.GetNetworkHandler();
            networkHandler.RegisterMemberListCallback(OnMemberListReceived);
            networkHandler.SendGuildMemberListRequest();
        }

        private void OnMemberListReceived(List<GuildMemberInfo> memberList)
        {
            // Filter out the current player and only show members who are not already leaders
            members = memberList
                .Where(m => m.PlayerUid != capi.World.Player.PlayerUID && m.Role != "Leader")
                .ToList();

            if (members.Count > 0)
            {
                selectedMemberUid = members[0].PlayerUid;
            }

            SetupDialog();
        }

        private void SetupDialog()
        {
            ElementBounds dialogBounds = ElementStdBounds.AutosizedMainDialog.WithAlignment(EnumDialogArea.CenterMiddle);
            ElementBounds bgBounds = ElementBounds.Fill.WithFixedPadding(GuiStyle.ElementToDialogPadding);
            bgBounds.BothSizing = ElementSizing.FitToChildren;

            var composer = capi.Gui.CreateCompo("transferownership", dialogBounds)
                .AddShadedDialogBG(bgBounds)
                .AddDialogTitleBar("Transfer Guild Ownership", OnTitleBarCloseClicked)
                .BeginChildElements(bgBounds);

            var top = 20.0;
            var spacing = 10.0;
            var elementHeight = 25.0;

            if (members.Count == 0)
            {
                composer.AddStaticText("No eligible members to transfer ownership to.",
                    CairoFont.WhiteDetailText(), ElementBounds.Fixed(0, top, 400, 50));

                composer.AddSmallButton("Close", OnCancel,
                    ElementBounds.Fixed(0, top + 60, 100, elementHeight), EnumButtonStyle.Normal);
            }
            else
            {
                composer.AddStaticText("Select the member to transfer guild leadership to:",
                    CairoFont.WhiteDetailText(), ElementBounds.Fixed(0, top, 400, elementHeight));
                top += elementHeight + spacing;

                // Create array of member names for the dropdown
                string[] memberNames = members.Select(m => m.PlayerName).ToArray();

                composer.AddStaticText("Member:", CairoFont.WhiteSmallText(),
                    ElementBounds.Fixed(0, top, 100, elementHeight));

                composer.AddDropDown(memberNames, memberNames, 0, OnMemberSelected,
                    ElementBounds.Fixed(110, top, 250, elementHeight), "memberDropdown");

                top += elementHeight + spacing * 2;

                composer.AddStaticText("Warning: This action cannot be undone!",
                    CairoFont.WhiteSmallText().WithColor(new double[] { 1.0, 0.5, 0.0, 1.0 }),
                    ElementBounds.Fixed(0, top, 400, elementHeight));
                top += elementHeight + spacing;

                composer.AddStaticText("You will become a regular member after transferring.",
                    CairoFont.WhiteSmallText(), ElementBounds.Fixed(0, top, 400, elementHeight));
                top += elementHeight + spacing * 2;

                // Buttons
                composer.AddSmallButton("Transfer", OnTransfer,
                    ElementBounds.Fixed(0, top, 100, elementHeight), EnumButtonStyle.Normal);

                composer.AddSmallButton("Cancel", OnCancel,
                    ElementBounds.Fixed(110, top, 100, elementHeight), EnumButtonStyle.Normal);
            }

            SingleComposer = composer.Compose();
        }

        private void OnMemberSelected(string code, bool selected)
        {
            // Find the member by name
            var selectedMember = members.FirstOrDefault(m => m.PlayerName == code);
            if (selectedMember != null)
            {
                selectedMemberUid = selectedMember.PlayerUid;
            }
        }

        private bool OnTransfer()
        {
            if (string.IsNullOrEmpty(selectedMemberUid))
            {
                return true;
            }

            // Send transfer request to server
            var networkHandler = modSystem.GetNetworkHandler();
            networkHandler.SendGuildTransferOwnershipRequest(selectedMemberUid);

            // Close dialog and callback
            TryClose();
            onTransferComplete?.Invoke();

            return true;
        }

        private bool OnCancel()
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

            // Unregister callback when dialog closes
            var networkHandler = modSystem.GetNetworkHandler();
            networkHandler?.UnregisterMemberListCallback();
        }
    }
}
