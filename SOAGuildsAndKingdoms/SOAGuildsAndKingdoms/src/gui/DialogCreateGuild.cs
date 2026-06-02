using System.Linq;
using Vintagestory.API.Client;
using Vintagestory.API.Config;

namespace SOAGuildsAndKingdoms.src.gui
{
    internal class DialogCreateGuild : GuiDialog
    {
        private SOAGuildsAndKingdomsModSystem modSystem;

        public DialogCreateGuild(ICoreClientAPI capi, SOAGuildsAndKingdomsModSystem modSystem) : base(capi)
        {
            this.modSystem = modSystem;
            SetupDialog();
        }

        public override string ToggleKeyCombinationCode => "guildcreate";

        private void SetupDialog()
        {
            ElementBounds dialogBounds = ElementStdBounds.AutosizedMainDialog.WithAlignment(EnumDialogArea.CenterMiddle);
            ElementBounds bgBounds = ElementBounds.Fill.WithFixedPadding(GuiStyle.ElementToDialogPadding);
            bgBounds.BothSizing = ElementSizing.FitToChildren;

            var composer = capi.Gui.CreateCompo("guildcreate", dialogBounds)
                .AddShadedDialogBG(bgBounds)
                .AddDialogTitleBar(Lang.Get("soaguildsandkingdoms:create-guild-title"), OnTitleBarCloseClicked)
                .BeginChildElements(bgBounds);

            var top = 20.0;
            var spacing = 10.0;
            var elementHeight = 25.0;

            // Instructions
            composer.AddStaticText(Lang.Get("soaguildsandkingdoms:create-guild-instructions"),
                CairoFont.WhiteSmallText(), ElementBounds.Fixed(0, top, 350, elementHeight));
            top += elementHeight + spacing;

            // Guild name input
            composer.AddStaticText(Lang.Get("soaguildsandkingdoms:guild-name"),
                CairoFont.WhiteSmallText(), ElementBounds.Fixed(0, top, 100, elementHeight));
            composer.AddTextInput(ElementBounds.Fixed(110, top, 200, elementHeight), null,
                CairoFont.TextInput(), "guildname");
            top += elementHeight + spacing;

            // Guild description input (optional)
            composer.AddStaticText(Lang.Get("soaguildsandkingdoms:guild-description"),
                CairoFont.WhiteSmallText(), ElementBounds.Fixed(0, top, 100, elementHeight));
            composer.AddTextInput(ElementBounds.Fixed(110, top, 200, elementHeight), null,
                CairoFont.TextInput(), "guilddescription");
            top += elementHeight + spacing * 1.5;

            // Buttons
            composer.AddSmallButton(Lang.Get("soaguildsandkingdoms:create"), OnCreateClick,
                ElementBounds.Fixed(0, top, 80, elementHeight), EnumButtonStyle.Normal);

            composer.AddSmallButton(Lang.Get("soaguildsandkingdoms:cancel"), OnCancelClick,
                ElementBounds.Fixed(90, top, 80, elementHeight), EnumButtonStyle.Normal);

            SingleComposer = composer.Compose();
        }

        private bool OnCreateClick()
        {
            var guildName = SingleComposer.GetTextInput("guildname").GetText();
            var guildDescription = SingleComposer.GetTextInput("guilddescription").GetText();

            if (string.IsNullOrWhiteSpace(guildName))
            {
                capi.ShowChatMessage(Lang.Get("soaguildsandkingdoms:please-enter-guild-name"));
                return true;
            }

            // Validate guild name (basic validation)
            if (guildName.Length < 3)
            {
                capi.ShowChatMessage(Lang.Get("soaguildsandkingdoms:guild-name-too-short"));
                return true;
            }

            if (guildName.Length > 20)
            {
                capi.ShowChatMessage(Lang.Get("soaguildsandkingdoms:guild-name-too-long"));
                return true;
            }

            // Check for invalid characters (basic check)
            if (guildName.Any(c => !char.IsLetterOrDigit(c) && c != '_' && c != '-' && c != ' '))
            {
                capi.ShowChatMessage(Lang.Get("soaguildsandkingdoms:guild-name-invalid-chars"));
                return true;
            }

            // Validate description length
            if (guildDescription.Length > 100)
            {
                capi.ShowChatMessage(Lang.Get("soaguildsandkingdoms:guild-description-too-long"));
                return true;
            }

            // Use network handler to create guild directly
            var networkHandler = modSystem.GetNetworkHandler();
            if (networkHandler != null)
            {
                networkHandler.SendGuildCreateRequest(guildName, guildDescription);
                capi.ShowChatMessage(Lang.Get("soaguildsandkingdoms:guild-creation-sent", guildName));
            }
            else
            {
                // Fallback to chat command if network handler not available
                capi.SendChatMessage($"/guild create {guildName}");
            }

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