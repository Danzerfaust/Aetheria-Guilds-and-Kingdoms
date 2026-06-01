using System;
using System.Runtime.CompilerServices;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;

namespace SRGuildsAndKingdoms.src.gui.dialogs
{
	// Token: 0x02000095 RID: 149
	[NullableContext(1)]
	[Nullable(0)]
	public class ConfirmAbandonDialog : GuiDialog
	{
		// Token: 0x170001BA RID: 442
		// (get) Token: 0x060006AA RID: 1706 RVA: 0x00032BA1 File Offset: 0x00030DA1
		[Nullable(2)]
		public override string ToggleKeyCombinationCode
		{
			[NullableContext(2)]
			get
			{
				return null;
			}
		}

		// Token: 0x060006AB RID: 1707 RVA: 0x00032BA4 File Offset: 0x00030DA4
		public ConfirmAbandonDialog(ICoreClientAPI capi, string message, Action onConfirm) : base(capi)
		{
			this.message = message;
			this.onConfirm = onConfirm;
		}

		// Token: 0x060006AC RID: 1708 RVA: 0x00032BBB File Offset: 0x00030DBB
		public override void OnGuiOpened()
		{
			base.OnGuiOpened();
			this.SetupDialog();
		}

		// Token: 0x170001BB RID: 443
		// (get) Token: 0x060006AD RID: 1709 RVA: 0x00032BC9 File Offset: 0x00030DC9
		public override double DrawOrder
		{
			get
			{
				return 1.0;
			}
		}

		// Token: 0x060006AE RID: 1710 RVA: 0x00032BD4 File Offset: 0x00030DD4
		private void SetupDialog()
		{
			ElementBounds dialogBounds = ElementStdBounds.AutosizedMainDialog.WithAlignment(6);
			ElementBounds bgBounds = ElementBounds.Fill.WithFixedPadding(GuiStyle.ElementToDialogPadding);
			bgBounds.BothSizing = 2;
			bgBounds.WithChildren(new ElementBounds[]
			{
				ElementBounds.Fixed(0.0, 0.0, 320.0, 120.0)
			});
			base.SingleComposer = GuiComposerHelpers.AddDialogTitleBar(GuiComposerHelpers.AddShadedDialogBG(this.capi.Gui.CreateCompo("guild-quest-abandon-confirm", dialogBounds), bgBounds, true, 5.0, 0.75f), Lang.Get("srguildsandkingdoms:quests-abandon-confirm-title", Array.Empty<object>()), new Action(this.OnTitleBarClose), null, null, null).BeginChildElements(bgBounds);
			ElementBounds textBounds = ElementBounds.Fixed(10.0, 30.0, 320.0, 120.0);
			GuiComposerHelpers.AddStaticText(base.SingleComposer, this.message, CairoFont.WhiteDetailText().WithOrientation(2), textBounds, null);
			ElementBounds cancelBounds = ElementBounds.Fixed(10.0, 150.0, 150.0, 30.0);
			ElementBounds confirmBounds = ElementBounds.Fixed(170.0, 150.0, 150.0, 30.0);
			GuiComposerHelpers.AddSmallButton(base.SingleComposer, Lang.Get("srguildsandkingdoms:quests-abandon-confirm-yes", Array.Empty<object>()), new ActionConsumable(this.OnConfirmClick), confirmBounds, 2, null);
			GuiComposerHelpers.AddSmallButton(base.SingleComposer, Lang.Get("srguildsandkingdoms:quests-abandon-confirm-no", Array.Empty<object>()), new ActionConsumable(this.OnCancelClick), cancelBounds, 2, null);
			base.SingleComposer.EndChildElements().Compose(true);
		}

		// Token: 0x060006AF RID: 1711 RVA: 0x00032D9C File Offset: 0x00030F9C
		private void OnTitleBarClose()
		{
			this.TryClose();
		}

		// Token: 0x060006B0 RID: 1712 RVA: 0x00032DA5 File Offset: 0x00030FA5
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

		// Token: 0x060006B1 RID: 1713 RVA: 0x00032DC0 File Offset: 0x00030FC0
		private bool OnCancelClick()
		{
			this.TryClose();
			return true;
		}

		// Token: 0x040002CB RID: 715
		private readonly string message;

		// Token: 0x040002CC RID: 716
		private readonly Action onConfirm;
	}
}
