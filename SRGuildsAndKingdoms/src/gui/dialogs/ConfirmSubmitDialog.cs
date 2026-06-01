using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;

namespace SRGuildsAndKingdoms.src.gui.dialogs
{
	// Token: 0x02000096 RID: 150
	[NullableContext(1)]
	[Nullable(0)]
	public class ConfirmSubmitDialog : GuiDialog
	{
		// Token: 0x170001BC RID: 444
		// (get) Token: 0x060006B2 RID: 1714 RVA: 0x00032DCA File Offset: 0x00030FCA
		[Nullable(2)]
		public override string ToggleKeyCombinationCode
		{
			[NullableContext(2)]
			get
			{
				return null;
			}
		}

		// Token: 0x170001BD RID: 445
		// (get) Token: 0x060006B3 RID: 1715 RVA: 0x00032DCD File Offset: 0x00030FCD
		public override double DrawOrder
		{
			get
			{
				return 2.0;
			}
		}

		// Token: 0x060006B4 RID: 1716 RVA: 0x00032DD8 File Offset: 0x00030FD8
		public ConfirmSubmitDialog(ICoreClientAPI capi, string message, List<string> itemLines, Action onConfirm) : base(capi)
		{
			this.message = message;
			this.itemLines = itemLines;
			this.onConfirm = onConfirm;
		}

		// Token: 0x060006B5 RID: 1717 RVA: 0x00032DF7 File Offset: 0x00030FF7
		public override void OnGuiOpened()
		{
			base.OnGuiOpened();
			this.SetupDialog();
		}

		// Token: 0x060006B6 RID: 1718 RVA: 0x00032E08 File Offset: 0x00031008
		private void SetupDialog()
		{
			double itemsHeight = (double)(this.itemLines.Count * 22);
			double dialogHeight = 50.0 + itemsHeight + 20.0 + 50.0;
			ElementBounds dialogBounds = ElementStdBounds.AutosizedMainDialog.WithAlignment(6);
			ElementBounds bgBounds = ElementBounds.Fill.WithFixedPadding(GuiStyle.ElementToDialogPadding);
			bgBounds.BothSizing = 2;
			bgBounds.WithChildren(new ElementBounds[]
			{
				ElementBounds.Fixed(0.0, 0.0, 320.0, dialogHeight)
			});
			base.SingleComposer = GuiComposerHelpers.AddDialogTitleBar(GuiComposerHelpers.AddShadedDialogBG(this.capi.Gui.CreateCompo("guild-quest-submit-confirm", dialogBounds), bgBounds, true, 5.0, 0.75f), Lang.Get("srguildsandkingdoms:quests-submit-confirm-title", Array.Empty<object>()), new Action(this.OnTitleBarClose), null, null, null).BeginChildElements(bgBounds);
			ElementBounds textBounds = ElementBounds.Fixed(10.0, 30.0, 320.0, 25.0);
			GuiComposerHelpers.AddStaticText(base.SingleComposer, this.message, CairoFont.WhiteDetailText(), textBounds, null);
			double yPos = 80.0;
			foreach (string line in this.itemLines)
			{
				ElementBounds lineBounds = ElementBounds.Fixed(20.0, yPos, 300.0, 20.0);
				GuiComposerHelpers.AddStaticText(base.SingleComposer, line, CairoFont.WhiteSmallText().WithColor(new double[]
				{
					0.89,
					0.72,
					0.04,
					1.0
				}), lineBounds, null);
				yPos += 22.0;
			}
			double buttonY = yPos + 20.0;
			ElementBounds cancelBounds = ElementBounds.Fixed(10.0, buttonY, 150.0, 30.0);
			ElementBounds confirmBounds = ElementBounds.Fixed(170.0, buttonY, 150.0, 30.0);
			GuiComposerHelpers.AddSmallButton(base.SingleComposer, Lang.Get("srguildsandkingdoms:quests-submit-confirm-yes", Array.Empty<object>()), new ActionConsumable(this.OnConfirmClick), confirmBounds, 2, null);
			GuiComposerHelpers.AddSmallButton(base.SingleComposer, Lang.Get("srguildsandkingdoms:quests-submit-confirm-no", Array.Empty<object>()), new ActionConsumable(this.OnCancelClick), cancelBounds, 2, null);
			base.SingleComposer.EndChildElements().Compose(true);
		}

		// Token: 0x060006B7 RID: 1719 RVA: 0x000330A0 File Offset: 0x000312A0
		private void OnTitleBarClose()
		{
			this.TryClose();
		}

		// Token: 0x060006B8 RID: 1720 RVA: 0x000330A9 File Offset: 0x000312A9
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

		// Token: 0x060006B9 RID: 1721 RVA: 0x000330C4 File Offset: 0x000312C4
		private bool OnCancelClick()
		{
			this.TryClose();
			return true;
		}

		// Token: 0x040002CD RID: 717
		private readonly string message;

		// Token: 0x040002CE RID: 718
		private readonly List<string> itemLines;

		// Token: 0x040002CF RID: 719
		private readonly Action onConfirm;
	}
}
