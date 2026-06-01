using System;
using System.Runtime.CompilerServices;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;

namespace SRGuildsAndKingdoms.src.gui
{
	// Token: 0x0200007A RID: 122
	[NullableContext(1)]
	[Nullable(0)]
	internal class DialogKickConfirm : GuiDialog
	{
		// Token: 0x06000519 RID: 1305 RVA: 0x0001FF20 File Offset: 0x0001E120
		public DialogKickConfirm(ICoreClientAPI capi, string memberName, Action onConfirm) : base(capi)
		{
			this.memberName = memberName;
			this.onConfirm = onConfirm;
			this.SetupDialog();
		}

		// Token: 0x1700016D RID: 365
		// (get) Token: 0x0600051A RID: 1306 RVA: 0x0001FF3D File Offset: 0x0001E13D
		public override string ToggleKeyCombinationCode
		{
			get
			{
				return "guildkickconfirm";
			}
		}

		// Token: 0x0600051B RID: 1307 RVA: 0x0001FF44 File Offset: 0x0001E144
		private void SetupDialog()
		{
			ElementBounds dialogBounds = ElementStdBounds.AutosizedMainDialog.WithAlignment(6);
			ElementBounds bgBounds = ElementBounds.Fill.WithFixedPadding(GuiStyle.ElementToDialogPadding);
			bgBounds.BothSizing = 2;
			GuiComposer composer = GuiComposerHelpers.AddDialogTitleBar(GuiComposerHelpers.AddShadedDialogBG(this.capi.Gui.CreateCompo("guildkickconfirm", dialogBounds), bgBounds, true, 5.0, 0.75f), Lang.Get("srguildsandkingdoms:kick-member-title", Array.Empty<object>()), new Action(this.OnTitleBarCloseClicked), null, null, null).BeginChildElements(bgBounds);
			double top = 20.0;
			double spacing = 15.0;
			double elementHeight = 25.0;
			double width = 400.0;
			string message = Lang.Get("srguildsandkingdoms:confirm-kick-member", new object[]
			{
				this.memberName
			});
			GuiComposerHelpers.AddStaticText(composer, message, CairoFont.WhiteDetailText(), ElementBounds.Fixed(0.0, top, width, elementHeight * 3.0), null);
			top += elementHeight * 3.0 + spacing;
			GuiComposerHelpers.AddSmallButton(composer, Lang.Get("srguildsandkingdoms:kick", Array.Empty<object>()), new ActionConsumable(this.OnConfirmClick), ElementBounds.Fixed(0.0, top, 100.0, elementHeight), 1, null);
			GuiComposerHelpers.AddSmallButton(composer, Lang.Get("srguildsandkingdoms:cancel", Array.Empty<object>()), new ActionConsumable(this.OnCancelClick), ElementBounds.Fixed(110.0, top, 100.0, elementHeight), 2, null);
			base.SingleComposer = composer.Compose(true);
		}

		// Token: 0x0600051C RID: 1308 RVA: 0x000200D6 File Offset: 0x0001E2D6
		private bool OnConfirmClick()
		{
			Action action = this.onConfirm;
			if (action != null)
			{
				action();
			}
			this.TryClose();
			return true;
		}

		// Token: 0x0600051D RID: 1309 RVA: 0x000200F1 File Offset: 0x0001E2F1
		private bool OnCancelClick()
		{
			this.TryClose();
			return true;
		}

		// Token: 0x0600051E RID: 1310 RVA: 0x000200FB File Offset: 0x0001E2FB
		private void OnTitleBarCloseClicked()
		{
			this.TryClose();
		}

		// Token: 0x1700016E RID: 366
		// (get) Token: 0x0600051F RID: 1311 RVA: 0x00020104 File Offset: 0x0001E304
		public override bool PrefersUngrabbedMouse
		{
			get
			{
				return false;
			}
		}

		// Token: 0x040001E9 RID: 489
		private string memberName;

		// Token: 0x040001EA RID: 490
		private Action onConfirm;
	}
}
