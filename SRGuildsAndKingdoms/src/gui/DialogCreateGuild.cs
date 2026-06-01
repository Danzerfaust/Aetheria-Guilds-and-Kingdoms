using System;
using System.Linq;
using System.Runtime.CompilerServices;
using SRGuildsAndKingdoms.src.network;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;

namespace SRGuildsAndKingdoms.src.gui
{
	// Token: 0x02000071 RID: 113
	[NullableContext(1)]
	[Nullable(0)]
	internal class DialogCreateGuild : GuiDialog
	{
		// Token: 0x06000460 RID: 1120 RVA: 0x00019274 File Offset: 0x00017474
		public DialogCreateGuild(ICoreClientAPI capi, SRGuildsAndKingdomsModSystem modSystem) : base(capi)
		{
			this.modSystem = modSystem;
			this.SetupDialog();
		}

		// Token: 0x17000159 RID: 345
		// (get) Token: 0x06000461 RID: 1121 RVA: 0x0001928A File Offset: 0x0001748A
		public override string ToggleKeyCombinationCode
		{
			get
			{
				return "guildcreate";
			}
		}

		// Token: 0x06000462 RID: 1122 RVA: 0x00019294 File Offset: 0x00017494
		private void SetupDialog()
		{
			ElementBounds dialogBounds = ElementStdBounds.AutosizedMainDialog.WithAlignment(6);
			ElementBounds bgBounds = ElementBounds.Fill.WithFixedPadding(GuiStyle.ElementToDialogPadding);
			bgBounds.BothSizing = 2;
			GuiComposer composer = GuiComposerHelpers.AddDialogTitleBar(GuiComposerHelpers.AddShadedDialogBG(this.capi.Gui.CreateCompo("guildcreate", dialogBounds), bgBounds, true, 5.0, 0.75f), Lang.Get("srguildsandkingdoms:create-guild-title", Array.Empty<object>()), new Action(this.OnTitleBarCloseClicked), null, null, null).BeginChildElements(bgBounds);
			double top = 20.0;
			double spacing = 10.0;
			double elementHeight = 25.0;
			GuiComposerHelpers.AddStaticText(composer, Lang.Get("srguildsandkingdoms:create-guild-instructions", Array.Empty<object>()), CairoFont.WhiteSmallText(), ElementBounds.Fixed(0.0, top, 350.0, elementHeight), null);
			top += elementHeight + spacing;
			GuiComposerHelpers.AddStaticText(composer, Lang.Get("srguildsandkingdoms:guild-name", Array.Empty<object>()), CairoFont.WhiteSmallText(), ElementBounds.Fixed(0.0, top, 100.0, elementHeight), null);
			GuiComposerHelpers.AddTextInput(composer, ElementBounds.Fixed(110.0, top, 200.0, elementHeight), null, CairoFont.TextInput(), "guildname");
			top += elementHeight + spacing;
			GuiComposerHelpers.AddStaticText(composer, Lang.Get("srguildsandkingdoms:guild-description", Array.Empty<object>()), CairoFont.WhiteSmallText(), ElementBounds.Fixed(0.0, top, 100.0, elementHeight), null);
			GuiComposerHelpers.AddTextInput(composer, ElementBounds.Fixed(110.0, top, 200.0, elementHeight), null, CairoFont.TextInput(), "guilddescription");
			top += elementHeight + spacing * 1.5;
			GuiComposerHelpers.AddSmallButton(composer, Lang.Get("srguildsandkingdoms:create", Array.Empty<object>()), new ActionConsumable(this.OnCreateClick), ElementBounds.Fixed(0.0, top, 80.0, elementHeight), 2, null);
			GuiComposerHelpers.AddSmallButton(composer, Lang.Get("srguildsandkingdoms:cancel", Array.Empty<object>()), new ActionConsumable(this.OnCancelClick), ElementBounds.Fixed(90.0, top, 80.0, elementHeight), 2, null);
			base.SingleComposer = composer.Compose(true);
		}

		// Token: 0x06000463 RID: 1123 RVA: 0x000194E0 File Offset: 0x000176E0
		private bool OnCreateClick()
		{
			string guildName = GuiComposerHelpers.GetTextInput(base.SingleComposer, "guildname").GetText();
			string guildDescription = GuiComposerHelpers.GetTextInput(base.SingleComposer, "guilddescription").GetText();
			if (string.IsNullOrWhiteSpace(guildName))
			{
				this.capi.ShowChatMessage(Lang.Get("srguildsandkingdoms:please-enter-guild-name", Array.Empty<object>()));
				return true;
			}
			if (guildName.Length < 3)
			{
				this.capi.ShowChatMessage(Lang.Get("srguildsandkingdoms:guild-name-too-short", Array.Empty<object>()));
				return true;
			}
			if (guildName.Length > 20)
			{
				this.capi.ShowChatMessage(Lang.Get("srguildsandkingdoms:guild-name-too-long", Array.Empty<object>()));
				return true;
			}
			if (guildName.Any((char c) => !char.IsLetterOrDigit(c) && c != '_' && c != '-' && c != ' '))
			{
				this.capi.ShowChatMessage(Lang.Get("srguildsandkingdoms:guild-name-invalid-chars", Array.Empty<object>()));
				return true;
			}
			if (guildDescription.Length > 100)
			{
				this.capi.ShowChatMessage(Lang.Get("srguildsandkingdoms:guild-description-too-long", Array.Empty<object>()));
				return true;
			}
			GuildNetworkHandler networkHandler = this.modSystem.GetNetworkHandler();
			if (networkHandler != null)
			{
				networkHandler.SendGuildCreateRequest(guildName, guildDescription);
				this.capi.ShowChatMessage(Lang.Get("srguildsandkingdoms:guild-creation-sent", new object[]
				{
					guildName
				}));
			}
			else
			{
				this.capi.SendChatMessage("/guild create " + guildName, null);
			}
			this.TryClose();
			return true;
		}

		// Token: 0x06000464 RID: 1124 RVA: 0x00019648 File Offset: 0x00017848
		private bool OnCancelClick()
		{
			this.TryClose();
			return true;
		}

		// Token: 0x06000465 RID: 1125 RVA: 0x00019652 File Offset: 0x00017852
		private void OnTitleBarCloseClicked()
		{
			this.TryClose();
		}

		// Token: 0x1700015A RID: 346
		// (get) Token: 0x06000466 RID: 1126 RVA: 0x0001965B File Offset: 0x0001785B
		public override bool PrefersUngrabbedMouse
		{
			get
			{
				return false;
			}
		}

		// Token: 0x040001B3 RID: 435
		private SRGuildsAndKingdomsModSystem modSystem;
	}
}
