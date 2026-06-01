using SRGuildsAndKingdoms.src.network;
using Vintagestory.API.Client;
using Vintagestory.API.Config;

namespace SRGuildsAndKingdoms.src.gui
{
    public class DialogPartyCreate(ICoreClientAPI capi, SRGuildsAndKingdomsModSystem modSystem) : GuiDialog(capi)
    {
        private string partyName = "";

        public override string ToggleKeyCombinationCode => "partycreatedialog";

        private void ComposeDialog()
        {
            ClearComposers();

            ElementBounds dialogBounds = ElementStdBounds.AutosizedMainDialog.WithAlignment(EnumDialogArea.CenterMiddle);

            var bgBounds = ElementBounds.Fill.WithFixedPadding(GuiStyle.ElementToDialogPadding);
            bgBounds.BothSizing = ElementSizing.FitToChildren;

            var composer = capi.Gui.CreateCompo("partycreate", dialogBounds)
                .AddShadedDialogBG(bgBounds)
                .AddDialogTitleBar("Create Party", OnTitleBarClose)
                .BeginChildElements(bgBounds)
                    .AddStaticText("Party Name:", CairoFont.WhiteDetailText(), ElementBounds.Fixed(0, 30, 100, 25))
                    .AddTextInput(ElementBounds.Fixed(110, 30, 200, 25), OnPartyNameChanged, CairoFont.WhiteDetailText(), "partyname")
                    .AddSmallButton("Create", OnCreateParty, ElementBounds.Fixed(0, 70, 90, 25))
                    .AddSmallButton("Cancel", OnCancel, ElementBounds.Fixed(100, 70, 90, 25))
                .EndChildElements()
                .Compose();

            Composers["partycreate"] = composer;

            capi.Logger.Notification("[DialogPartyCreate] Dialog composition stub");
        }

        public override void OnGuiOpened()
        {
            base.OnGuiOpened();
            ComposeDialog();
        }

        private void OnTitleBarClose()
        {
            TryClose();
        }

        private void OnPartyNameChanged(string value)
        {
            partyName = value;
        }

        private bool OnCreateParty()
        {
            if (string.IsNullOrWhiteSpace(partyName))
            {
                capi.ShowChatMessage("Party name cannot be empty.");
                return true;
            }

            capi.Logger.Notification($"[DialogPartyCreate] Creating party '{partyName}'");

            modSystem.PartyNetworkHandler?.SendCreateParty(partyName);

            TryClose();
            return true;
        }

        private bool OnCancel()
        {
            TryClose();
            return true;
        }
    }
}
