using SRGuildsAndKingdoms.src.guilds;
using System;
using Vintagestory.API.Client;
using Vintagestory.API.Common;

namespace SRGuildsAndKingdoms.src.gui.tabs
{
    public class GuildSettingsTab : GuildTabContent
    {
        private string? pendingGuildName;
        private string? pendingDescription;
        private string? pendingPrimaryColor;
        private string? pendingSecondaryColor;

        private readonly Action<string> onGuildNameChanged;
        private readonly Action<string> onDescriptionChanged;
        private readonly Action<string> onPrimaryColorChanged;
        private readonly Action<string> onSecondaryColorChanged;
        private readonly ActionConsumable onSaveSettings;
        private readonly ActionConsumable onCloseDialog;

        public GuildSettingsTab(ICoreClientAPI capi, SRGuildsAndKingdomsModSystem modSystem,
            GuildSummary? currentGuild, Action<string> onGuildNameChanged,
            Action<string> onDescriptionChanged, Action<string> onPrimaryColorChanged,
            Action<string> onSecondaryColorChanged, ActionConsumable onSaveSettings,
            ActionConsumable onCloseDialog)
            : base(capi, modSystem, currentGuild)
        {
            this.onGuildNameChanged = onGuildNameChanged;
            this.onDescriptionChanged = onDescriptionChanged;
            this.onPrimaryColorChanged = onPrimaryColorChanged;
            this.onSecondaryColorChanged = onSecondaryColorChanged;
            this.onSaveSettings = onSaveSettings;
            this.onCloseDialog = onCloseDialog;

            InitializePendingValues();
        }

        private void InitializePendingValues()
        {
            if (currentGuild != null)
            {
                pendingGuildName = currentGuild.Name;
                pendingDescription = currentGuild.Description;
                pendingPrimaryColor = ColorToHex(currentGuild.DisplayColor);
                pendingSecondaryColor = ColorToHex(currentGuild.SecondaryColor);
            }
        }

        public void SetPendingValues(string? guildName, string? description, string? primaryColor, string? secondaryColor)
        {
            this.pendingGuildName = guildName;
            this.pendingDescription = description;
            this.pendingPrimaryColor = primaryColor;
            this.pendingSecondaryColor = secondaryColor;
        }

        public override void Refresh(GuildSummary? updatedGuild)
        {
            base.Refresh(updatedGuild);
            if (pendingGuildName == null && updatedGuild != null)
            {
                InitializePendingValues();
            }
        }

        public override double AddContent(GuiComposer composer, double startTop)
        {
            if (!HasManagePermissions() || currentGuild == null) return startTop;

            var top = startTop;
            var spacing = 10.0;
            var elementHeight = 25.0;

            composer.AddStaticText("Guild Settings:", CairoFont.WhiteMediumText(),
                ElementBounds.Fixed(0, top, 400, elementHeight));
            top += elementHeight + spacing;

            // Guild Name editing
            composer.AddStaticText("Guild Name:", CairoFont.WhiteSmallText(),
                ElementBounds.Fixed(0, top, 150, elementHeight));
            composer.AddTextInput(ElementBounds.Fixed(150, top, 200, elementHeight), onGuildNameChanged,
                CairoFont.TextInput(), "guildname");

            var guildNameInput = composer.GetTextInput("guildname");
            if (guildNameInput != null)
            {
                guildNameInput.SetValue(pendingGuildName ?? currentGuild.Name ?? "");
            }
            top += elementHeight + spacing;

            // Guild Description editing
            composer.AddStaticText("Description:", CairoFont.WhiteSmallText(),
                ElementBounds.Fixed(0, top, 150, elementHeight));
            composer.AddTextInput(ElementBounds.Fixed(150, top, 200, elementHeight), onDescriptionChanged,
                CairoFont.TextInput(), "guilddescription");

            var descriptionInput = composer.GetTextInput("guilddescription");
            if (descriptionInput != null)
            {
                descriptionInput.SetValue(pendingDescription ?? currentGuild.Description ?? "");
            }
            top += elementHeight + spacing;

            // Primary Color editing
            composer.AddStaticText("Primary Color:", CairoFont.WhiteSmallText(),
                ElementBounds.Fixed(0, top, 150, elementHeight));
            composer.AddTextInput(ElementBounds.Fixed(150, top, 100, elementHeight), onPrimaryColorChanged,
                CairoFont.TextInput(), "primarycolor");

            var primaryColorInput = composer.GetTextInput("primarycolor");
            if (primaryColorInput != null)
            {
                primaryColorInput.SetValue(pendingPrimaryColor ?? ColorToHex(currentGuild.DisplayColor));
            }
            // Add color preview using inset
            composer.AddInset(ElementBounds.Fixed(260, top + 2, 20, 20), 2);
            top += elementHeight + spacing;

            // Secondary Color editing
            composer.AddStaticText("Secondary Color:", CairoFont.WhiteSmallText(),
                ElementBounds.Fixed(0, top, 150, elementHeight));
            composer.AddTextInput(ElementBounds.Fixed(150, top, 100, elementHeight), onSecondaryColorChanged,
                CairoFont.TextInput(), "secondarycolor");

            var secondaryColorInput = composer.GetTextInput("secondarycolor");
            if (secondaryColorInput != null)
            {
                secondaryColorInput.SetValue(pendingSecondaryColor ?? ColorToHex(currentGuild.SecondaryColor));
            }
            // Add color preview using inset
            composer.AddInset(ElementBounds.Fixed(260, top + 2, 20, 20), 2);
            top += elementHeight + spacing * 2;

            // Save Settings button
            composer.AddSmallButton("Save Settings", onSaveSettings,
                ElementBounds.Fixed(0, top, 120, elementHeight), EnumButtonStyle.MainMenu);

            // Close Dialog button
            composer.AddSmallButton("Close Dialog", onCloseDialog,
                ElementBounds.Fixed(130, top, 120, elementHeight), EnumButtonStyle.Normal);

            top += elementHeight + spacing * 2;

            // Transfer Ownership button (only for leaders)
            if (IsLeader())
            {
                composer.AddStaticText("Transfer Ownership:", CairoFont.WhiteMediumText(),
                    ElementBounds.Fixed(0, top, 400, elementHeight));
                top += elementHeight + spacing;

                composer.AddSmallButton("Transfer Leadership", OnTransferOwnership,
                    ElementBounds.Fixed(0, top, 150, elementHeight), EnumButtonStyle.Normal);

                top += elementHeight;
            }

            return top;
        }

        private bool OnTransferOwnership()
        {
            // Open the transfer ownership dialog
            var dialog = new DialogTransferOwnership(capi, modSystem, currentGuild, () =>
            {
                // Callback when transfer is complete - close the settings dialog
                onCloseDialog();
            });
            dialog.TryOpen();

            return true;
        }
    }
}