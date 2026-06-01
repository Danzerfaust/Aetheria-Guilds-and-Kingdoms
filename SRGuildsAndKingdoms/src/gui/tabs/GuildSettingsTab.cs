using System;
using System.Runtime.CompilerServices;
using SRGuildsAndKingdoms.src.guilds;
using Vintagestory.API.Client;
using Vintagestory.API.Common;

namespace SRGuildsAndKingdoms.src.gui.tabs
{
	// Token: 0x02000091 RID: 145
	[NullableContext(1)]
	[Nullable(0)]
	public class GuildSettingsTab : GuildTabContent
	{
		// Token: 0x06000666 RID: 1638 RVA: 0x0002F698 File Offset: 0x0002D898
		public GuildSettingsTab(ICoreClientAPI capi, SRGuildsAndKingdomsModSystem modSystem, [Nullable(2)] GuildSummary currentGuild, Action<string> onGuildNameChanged, Action<string> onDescriptionChanged, Action<string> onPrimaryColorChanged, Action<string> onSecondaryColorChanged, ActionConsumable onSaveSettings, ActionConsumable onCloseDialog) : base(capi, modSystem, currentGuild)
		{
			this.onGuildNameChanged = onGuildNameChanged;
			this.onDescriptionChanged = onDescriptionChanged;
			this.onPrimaryColorChanged = onPrimaryColorChanged;
			this.onSecondaryColorChanged = onSecondaryColorChanged;
			this.onSaveSettings = onSaveSettings;
			this.onCloseDialog = onCloseDialog;
			this.InitializePendingValues();
		}

		// Token: 0x06000667 RID: 1639 RVA: 0x0002F6E4 File Offset: 0x0002D8E4
		private void InitializePendingValues()
		{
			if (this.currentGuild != null)
			{
				this.pendingGuildName = this.currentGuild.Name;
				this.pendingDescription = this.currentGuild.Description;
				this.pendingPrimaryColor = base.ColorToHex(this.currentGuild.DisplayColor);
				this.pendingSecondaryColor = base.ColorToHex(this.currentGuild.SecondaryColor);
			}
		}

		// Token: 0x06000668 RID: 1640 RVA: 0x0002F749 File Offset: 0x0002D949
		[NullableContext(2)]
		public void SetPendingValues(string guildName, string description, string primaryColor, string secondaryColor)
		{
			this.pendingGuildName = guildName;
			this.pendingDescription = description;
			this.pendingPrimaryColor = primaryColor;
			this.pendingSecondaryColor = secondaryColor;
		}

		// Token: 0x06000669 RID: 1641 RVA: 0x0002F768 File Offset: 0x0002D968
		[NullableContext(2)]
		public override void Refresh(GuildSummary updatedGuild)
		{
			base.Refresh(updatedGuild);
			if (this.pendingGuildName == null && updatedGuild != null)
			{
				this.InitializePendingValues();
			}
		}

		// Token: 0x0600066A RID: 1642 RVA: 0x0002F784 File Offset: 0x0002D984
		public override double AddContent(GuiComposer composer, double startTop)
		{
			if (!base.HasManagePermissions() || this.currentGuild == null)
			{
				return startTop;
			}
			double spacing = 10.0;
			double elementHeight = 25.0;
			GuiComposerHelpers.AddStaticText(composer, "Guild Settings:", CairoFont.WhiteMediumText(), ElementBounds.Fixed(0.0, startTop, 400.0, elementHeight), null);
			double top = startTop + (elementHeight + spacing);
			GuiComposerHelpers.AddStaticText(composer, "Guild Name:", CairoFont.WhiteSmallText(), ElementBounds.Fixed(0.0, top, 150.0, elementHeight), null);
			GuiComposerHelpers.AddTextInput(composer, ElementBounds.Fixed(150.0, top, 200.0, elementHeight), this.onGuildNameChanged, CairoFont.TextInput(), "guildname");
			GuiElementTextInput guildNameInput = GuiComposerHelpers.GetTextInput(composer, "guildname");
			if (guildNameInput != null)
			{
				GuiElementEditableTextBase guiElementEditableTextBase = guildNameInput;
				string text;
				if ((text = this.pendingGuildName) == null)
				{
					text = (this.currentGuild.Name ?? "");
				}
				guiElementEditableTextBase.SetValue(text, true);
			}
			top += elementHeight + spacing;
			GuiComposerHelpers.AddStaticText(composer, "Description:", CairoFont.WhiteSmallText(), ElementBounds.Fixed(0.0, top, 150.0, elementHeight), null);
			GuiComposerHelpers.AddTextInput(composer, ElementBounds.Fixed(150.0, top, 200.0, elementHeight), this.onDescriptionChanged, CairoFont.TextInput(), "guilddescription");
			GuiElementTextInput descriptionInput = GuiComposerHelpers.GetTextInput(composer, "guilddescription");
			if (descriptionInput != null)
			{
				GuiElementEditableTextBase guiElementEditableTextBase2 = descriptionInput;
				string text2;
				if ((text2 = this.pendingDescription) == null)
				{
					text2 = (this.currentGuild.Description ?? "");
				}
				guiElementEditableTextBase2.SetValue(text2, true);
			}
			top += elementHeight + spacing;
			GuiComposerHelpers.AddStaticText(composer, "Primary Color:", CairoFont.WhiteSmallText(), ElementBounds.Fixed(0.0, top, 150.0, elementHeight), null);
			GuiComposerHelpers.AddTextInput(composer, ElementBounds.Fixed(150.0, top, 100.0, elementHeight), this.onPrimaryColorChanged, CairoFont.TextInput(), "primarycolor");
			GuiElementTextInput primaryColorInput = GuiComposerHelpers.GetTextInput(composer, "primarycolor");
			if (primaryColorInput != null)
			{
				primaryColorInput.SetValue(this.pendingPrimaryColor ?? base.ColorToHex(this.currentGuild.DisplayColor), true);
			}
			GuiElementInsetHelper.AddInset(composer, ElementBounds.Fixed(260.0, top + 2.0, 20.0, 20.0), 2, 0.85f);
			top += elementHeight + spacing;
			GuiComposerHelpers.AddStaticText(composer, "Secondary Color:", CairoFont.WhiteSmallText(), ElementBounds.Fixed(0.0, top, 150.0, elementHeight), null);
			GuiComposerHelpers.AddTextInput(composer, ElementBounds.Fixed(150.0, top, 100.0, elementHeight), this.onSecondaryColorChanged, CairoFont.TextInput(), "secondarycolor");
			GuiElementTextInput secondaryColorInput = GuiComposerHelpers.GetTextInput(composer, "secondarycolor");
			if (secondaryColorInput != null)
			{
				secondaryColorInput.SetValue(this.pendingSecondaryColor ?? base.ColorToHex(this.currentGuild.SecondaryColor), true);
			}
			GuiElementInsetHelper.AddInset(composer, ElementBounds.Fixed(260.0, top + 2.0, 20.0, 20.0), 2, 0.85f);
			top += elementHeight + spacing * 2.0;
			GuiComposerHelpers.AddSmallButton(composer, "Save Settings", this.onSaveSettings, ElementBounds.Fixed(0.0, top, 120.0, elementHeight), 1, null);
			GuiComposerHelpers.AddSmallButton(composer, "Close Dialog", this.onCloseDialog, ElementBounds.Fixed(130.0, top, 120.0, elementHeight), 2, null);
			top += elementHeight + spacing * 2.0;
			if (base.IsLeader())
			{
				GuiComposerHelpers.AddStaticText(composer, "Transfer Ownership:", CairoFont.WhiteMediumText(), ElementBounds.Fixed(0.0, top, 400.0, elementHeight), null);
				top += elementHeight + spacing;
				GuiComposerHelpers.AddSmallButton(composer, "Transfer Leadership", new ActionConsumable(this.OnTransferOwnership), ElementBounds.Fixed(0.0, top, 150.0, elementHeight), 2, null);
				top += elementHeight;
			}
			return top;
		}

		// Token: 0x0600066B RID: 1643 RVA: 0x0002FB96 File Offset: 0x0002DD96
		private bool OnTransferOwnership()
		{
			new DialogTransferOwnership(this.capi, this.modSystem, this.currentGuild, delegate
			{
				this.onCloseDialog.Invoke();
			}).TryOpen();
			return true;
		}

		// Token: 0x040002AA RID: 682
		[Nullable(2)]
		private string pendingGuildName;

		// Token: 0x040002AB RID: 683
		[Nullable(2)]
		private string pendingDescription;

		// Token: 0x040002AC RID: 684
		[Nullable(2)]
		private string pendingPrimaryColor;

		// Token: 0x040002AD RID: 685
		[Nullable(2)]
		private string pendingSecondaryColor;

		// Token: 0x040002AE RID: 686
		private readonly Action<string> onGuildNameChanged;

		// Token: 0x040002AF RID: 687
		private readonly Action<string> onDescriptionChanged;

		// Token: 0x040002B0 RID: 688
		private readonly Action<string> onPrimaryColorChanged;

		// Token: 0x040002B1 RID: 689
		private readonly Action<string> onSecondaryColorChanged;

		// Token: 0x040002B2 RID: 690
		private readonly ActionConsumable onSaveSettings;

		// Token: 0x040002B3 RID: 691
		private readonly ActionConsumable onCloseDialog;
	}
}
