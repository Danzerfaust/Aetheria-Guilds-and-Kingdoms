using SOAGuildsAndKingdoms.src.network;
using Vintagestory.API.Client;

namespace SOAGuildsAndKingdoms.src.gui
{
    public class DialogPartyInvitePopup(ICoreClientAPI capi, SOAGuildsAndKingdomsModSystem modSystem) : HudElement(capi)
    {
        private PartyInviteNotificationPacket? currentInvite;

        public void ShowInvite(PartyInviteNotificationPacket invite)
        {
            currentInvite = invite;

            ComposeDialog();
            TryOpen();
        }

        public void DismissInvite()
        {
            currentInvite = null;
            TryClose();
        }

        public override void OnOwnPlayerDataReceived()
        {
            base.OnOwnPlayerDataReceived();
        }

        private void ComposeDialog()
        {
            if (currentInvite == null) return;

            ClearComposers();

            ElementBounds dialogBounds = ElementStdBounds.AutosizedMainDialog.WithAlignment(EnumDialogArea.RightMiddle).WithFixedAlignmentOffset(-10, 0);
            
            var bgBounds = ElementBounds.Fill.WithFixedPadding(GuiStyle.ElementToDialogPadding);
            bgBounds.BothSizing = ElementSizing.FitToChildren;
            
            var composer = capi.Gui.CreateCompo("partyinvitepopup", dialogBounds)
                .AddShadedDialogBG(bgBounds)
                .AddDialogTitleBar($"Party Invite", OnTitleBarClose)
                .BeginChildElements(bgBounds)
                    .AddStaticText($"{currentInvite.InviterName} invited you to join '{currentInvite.PartyName}'", 
                        CairoFont.WhiteDetailText(), 
                        ElementBounds.Fixed(0, 30, 300, 25))
                    .AddSmallButton("Accept", OnAccept, ElementBounds.Fixed(0, 70, 90, 25))
                    .AddSmallButton("Decline", OnDecline, ElementBounds.Fixed(100, 70, 90, 25))
                .EndChildElements()
                .Compose();

            Composers["partyinvitepopup"] = composer;
        }

        private void OnTitleBarClose()
        {
            OnDecline();
        }

        private bool OnAccept()
        {
            if (currentInvite == null) return true;

            modSystem.PartyNetworkHandler?.SendAcceptInvite(currentInvite.InviterUid);

            DismissInvite();
            return true;
        }

        private bool OnDecline()
        {
            if (currentInvite == null) return true;

            modSystem.PartyNetworkHandler?.SendDeclineInvite(currentInvite.InviterUid);

            DismissInvite();
            return true;
        }

        public override void Dispose()
        {
            base.Dispose();
            currentInvite = null;
        }
    }
}
