using System;
using System.Linq;
using Vintagestory.API.Client;
using Vintagestory.API.Config;

namespace SRGuildsAndKingdoms.src.gui
{
    public class DialogCreateOutpost : GuiDialog
    {
        private SRGuildsAndKingdomsModSystem modSystem;
        private Action<string> onOutpostCreated;

        public DialogCreateOutpost(ICoreClientAPI capi, SRGuildsAndKingdomsModSystem modSystem, Action<string> onOutpostCreated) : base(capi)
        {
            this.modSystem = modSystem;
            this.onOutpostCreated = onOutpostCreated;
            SetupDialog();
        }

        public override string ToggleKeyCombinationCode => "createoutpost";

        private void SetupDialog()
        {
            ElementBounds dialogBounds = ElementStdBounds.AutosizedMainDialog.WithAlignment(EnumDialogArea.CenterMiddle);
            ElementBounds bgBounds = ElementBounds.Fill.WithFixedPadding(GuiStyle.ElementToDialogPadding);
            bgBounds.BothSizing = ElementSizing.FitToChildren;

            var composer = capi.Gui.CreateCompo("createoutpost", dialogBounds)
                .AddShadedDialogBG(bgBounds)
                .AddDialogTitleBar(Lang.Get("srguildsandkingdoms:create-outpost-title"), OnTitleBarCloseClicked)
                .BeginChildElements(bgBounds);

            var top = 20.0;
            var spacing = 10.0;
            var elementHeight = 25.0;

            // Instructions
            composer.AddStaticText(Lang.Get("srguildsandkingdoms:create-outpost-instructions"),
                CairoFont.WhiteSmallText(), ElementBounds.Fixed(0, top, 400, elementHeight * 2));
            top += elementHeight * 2 + spacing;

            // Outpost name input
            composer.AddStaticText(Lang.Get("srguildsandkingdoms:outpost-name"),
                CairoFont.WhiteSmallText(), ElementBounds.Fixed(0, top, 100, elementHeight));
            composer.AddTextInput(ElementBounds.Fixed(110, top, 200, elementHeight), null,
                CairoFont.TextInput(), "outpostname");
            top += elementHeight + spacing;

            // Optional note about naming
            composer.AddStaticText(Lang.Get("srguildsandkingdoms:outpost-name-optional"),
                CairoFont.WhiteSmallText().WithColor(new double[] { 0.8, 0.8, 0.8, 1.0 }),
                ElementBounds.Fixed(110, top, 200, elementHeight));
            top += elementHeight + spacing * 1.5;

            // Buttons
            composer.AddSmallButton(Lang.Get("srguildsandkingdoms:create"), OnCreateClick,
                ElementBounds.Fixed(0, top, 80, elementHeight), EnumButtonStyle.MainMenu);

            composer.AddSmallButton(Lang.Get("srguildsandkingdoms:cancel"), OnCancelClick,
                ElementBounds.Fixed(90, top, 80, elementHeight), EnumButtonStyle.Normal);

            SingleComposer = composer.Compose();
        }

        private bool OnCreateClick()
        {
            var outpostName = SingleComposer.GetTextInput("outpostname").GetText();

            // Outpost name is optional, but if provided, do basic validation
            if (!string.IsNullOrWhiteSpace(outpostName))
            {
                if (outpostName.Length > 30)
                {
                    capi.ShowChatMessage(Lang.Get("srguildsandkingdoms:outpost-name-too-long"));
                    return true;
                }

                // Check for invalid characters (basic check)
                if (outpostName.Any(c => !char.IsLetterOrDigit(c) && c != '_' && c != '-' && c != ' '))
                {
                    capi.ShowChatMessage(Lang.Get("srguildsandkingdoms:outpost-name-invalid-chars"));
                    return true;
                }
            }

            // Trigger the callback with the outpost name (can be empty)
            onOutpostCreated?.Invoke(outpostName?.Trim() ?? "");

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