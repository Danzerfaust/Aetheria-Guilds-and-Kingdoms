using System;
using System.Runtime.CompilerServices;
using Vintagestory.API.Client;
using Vintagestory.API.Common;

namespace SRGuildsAndKingdoms.src.gui.tabs
{
	// Token: 0x02000090 RID: 144
	[NullableContext(1)]
	[Nullable(0)]
	internal class ConfirmContributionDialog : GuiDialog
	{
		// Token: 0x170001B6 RID: 438
		// (get) Token: 0x0600065F RID: 1631 RVA: 0x0002F49A File Offset: 0x0002D69A
		public override string ToggleKeyCombinationCode
		{
			get
			{
				return null;
			}
		}

		// Token: 0x06000660 RID: 1632 RVA: 0x0002F49D File Offset: 0x0002D69D
		public ConfirmContributionDialog(ICoreClientAPI capi, string message, Action onConfirm) : base(capi)
		{
			this.message = message;
			this.onConfirm = onConfirm;
		}

		// Token: 0x06000661 RID: 1633 RVA: 0x0002F4B4 File Offset: 0x0002D6B4
		public override void OnGuiOpened()
		{
			base.OnGuiOpened();
			this.SetupDialog();
		}

		// Token: 0x06000662 RID: 1634 RVA: 0x0002F4C4 File Offset: 0x0002D6C4
		private void SetupDialog()
		{
			ElementBounds dialogBounds = ElementStdBounds.AutosizedMainDialog.WithAlignment(6);
			ElementBounds bgBounds = ElementBounds.Fill.WithFixedPadding(GuiStyle.ElementToDialogPadding);
			bgBounds.BothSizing = 2;
			bgBounds.WithChildren(new ElementBounds[]
			{
				ElementBounds.Fixed(0.0, 0.0, 400.0, 250.0)
			});
			base.SingleComposer = GuiComposerHelpers.AddDialogTitleBar(GuiComposerHelpers.AddShadedDialogBG(this.capi.Gui.CreateCompo("guildtech-contribute-confirm", dialogBounds), bgBounds, true, 5.0, 0.75f), "Confirm Contribution", new Action(this.OnTitleBarClose), null, null, null).BeginChildElements(bgBounds);
			ElementBounds textBounds = ElementBounds.Fixed(10.0, 30.0, 380.0, 180.0);
			GuiComposerHelpers.AddStaticText(base.SingleComposer, this.message, CairoFont.WhiteDetailText(), textBounds, null);
			ElementBounds confirmBounds = ElementBounds.Fixed(10.0, 220.0, 150.0, 30.0);
			ElementBounds cancelBounds = ElementBounds.Fixed(170.0, 220.0, 150.0, 30.0);
			GuiComposerHelpers.AddSmallButton(base.SingleComposer, "Confirm", new ActionConsumable(this.OnConfirmClick), confirmBounds, 2, null);
			GuiComposerHelpers.AddSmallButton(base.SingleComposer, "Cancel", new ActionConsumable(this.OnCancelClick), cancelBounds, 2, null);
			base.SingleComposer.EndChildElements().Compose(true);
		}

		// Token: 0x06000663 RID: 1635 RVA: 0x0002F668 File Offset: 0x0002D868
		private void OnTitleBarClose()
		{
			this.TryClose();
		}

		// Token: 0x06000664 RID: 1636 RVA: 0x0002F671 File Offset: 0x0002D871
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

		// Token: 0x06000665 RID: 1637 RVA: 0x0002F68C File Offset: 0x0002D88C
		private bool OnCancelClick()
		{
			this.TryClose();
			return true;
		}

		// Token: 0x040002A8 RID: 680
		private string message;

		// Token: 0x040002A9 RID: 681
		private Action onConfirm;
	}
}
