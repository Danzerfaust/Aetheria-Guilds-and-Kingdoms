using System;
using System.Linq;
using System.Runtime.CompilerServices;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;

namespace SRGuildsAndKingdoms.src.gui
{
	// Token: 0x02000072 RID: 114
	[NullableContext(1)]
	[Nullable(0)]
	public class DialogCreateOutpost : GuiDialog
	{
		// Token: 0x06000467 RID: 1127 RVA: 0x0001965E File Offset: 0x0001785E
		public DialogCreateOutpost(ICoreClientAPI capi, SRGuildsAndKingdomsModSystem modSystem, Action<string> onOutpostCreated) : base(capi)
		{
			this.modSystem = modSystem;
			this.onOutpostCreated = onOutpostCreated;
			this.SetupDialog();
		}

		// Token: 0x1700015B RID: 347
		// (get) Token: 0x06000468 RID: 1128 RVA: 0x0001967B File Offset: 0x0001787B
		public override string ToggleKeyCombinationCode
		{
			get
			{
				return "createoutpost";
			}
		}

		// Token: 0x06000469 RID: 1129 RVA: 0x00019684 File Offset: 0x00017884
		private void SetupDialog()
		{
			ElementBounds dialogBounds = ElementStdBounds.AutosizedMainDialog.WithAlignment(6);
			ElementBounds bgBounds = ElementBounds.Fill.WithFixedPadding(GuiStyle.ElementToDialogPadding);
			bgBounds.BothSizing = 2;
			GuiComposer composer = GuiComposerHelpers.AddDialogTitleBar(GuiComposerHelpers.AddShadedDialogBG(this.capi.Gui.CreateCompo("createoutpost", dialogBounds), bgBounds, true, 5.0, 0.75f), Lang.Get("srguildsandkingdoms:create-outpost-title", Array.Empty<object>()), new Action(this.OnTitleBarCloseClicked), null, null, null).BeginChildElements(bgBounds);
			double top = 20.0;
			double spacing = 10.0;
			double elementHeight = 25.0;
			GuiComposerHelpers.AddStaticText(composer, Lang.Get("srguildsandkingdoms:create-outpost-instructions", Array.Empty<object>()), CairoFont.WhiteSmallText(), ElementBounds.Fixed(0.0, top, 400.0, elementHeight * 2.0), null);
			top += elementHeight * 2.0 + spacing;
			GuiComposerHelpers.AddStaticText(composer, Lang.Get("srguildsandkingdoms:outpost-name", Array.Empty<object>()), CairoFont.WhiteSmallText(), ElementBounds.Fixed(0.0, top, 100.0, elementHeight), null);
			GuiComposerHelpers.AddTextInput(composer, ElementBounds.Fixed(110.0, top, 200.0, elementHeight), null, CairoFont.TextInput(), "outpostname");
			top += elementHeight + spacing;
			GuiComposerHelpers.AddStaticText(composer, Lang.Get("srguildsandkingdoms:outpost-name-optional", Array.Empty<object>()), CairoFont.WhiteSmallText().WithColor(new double[]
			{
				0.8,
				0.8,
				0.8,
				1.0
			}), ElementBounds.Fixed(110.0, top, 200.0, elementHeight), null);
			top += elementHeight + spacing * 1.5;
			GuiComposerHelpers.AddSmallButton(composer, Lang.Get("srguildsandkingdoms:create", Array.Empty<object>()), new ActionConsumable(this.OnCreateClick), ElementBounds.Fixed(0.0, top, 80.0, elementHeight), 1, null);
			GuiComposerHelpers.AddSmallButton(composer, Lang.Get("srguildsandkingdoms:cancel", Array.Empty<object>()), new ActionConsumable(this.OnCancelClick), ElementBounds.Fixed(90.0, top, 80.0, elementHeight), 2, null);
			base.SingleComposer = composer.Compose(true);
		}

		// Token: 0x0600046A RID: 1130 RVA: 0x000198CC File Offset: 0x00017ACC
		private bool OnCreateClick()
		{
			string outpostName = GuiComposerHelpers.GetTextInput(base.SingleComposer, "outpostname").GetText();
			if (!string.IsNullOrWhiteSpace(outpostName))
			{
				if (outpostName.Length > 30)
				{
					this.capi.ShowChatMessage(Lang.Get("srguildsandkingdoms:outpost-name-too-long", Array.Empty<object>()));
					return true;
				}
				if (outpostName.Any((char c) => !char.IsLetterOrDigit(c) && c != '_' && c != '-' && c != ' '))
				{
					this.capi.ShowChatMessage(Lang.Get("srguildsandkingdoms:outpost-name-invalid-chars", Array.Empty<object>()));
					return true;
				}
			}
			Action<string> action = this.onOutpostCreated;
			if (action != null)
			{
				action(((outpostName != null) ? outpostName.Trim() : null) ?? "");
			}
			this.TryClose();
			return true;
		}

		// Token: 0x0600046B RID: 1131 RVA: 0x0001998E File Offset: 0x00017B8E
		private bool OnCancelClick()
		{
			this.TryClose();
			return true;
		}

		// Token: 0x0600046C RID: 1132 RVA: 0x00019998 File Offset: 0x00017B98
		private void OnTitleBarCloseClicked()
		{
			this.TryClose();
		}

		// Token: 0x1700015C RID: 348
		// (get) Token: 0x0600046D RID: 1133 RVA: 0x000199A1 File Offset: 0x00017BA1
		public override bool PrefersUngrabbedMouse
		{
			get
			{
				return false;
			}
		}

		// Token: 0x040001B4 RID: 436
		private SRGuildsAndKingdomsModSystem modSystem;

		// Token: 0x040001B5 RID: 437
		private Action<string> onOutpostCreated;
	}
}
