using System;
using System.Runtime.CompilerServices;
using Vintagestory.API.Client;
using Vintagestory.API.Common;

namespace SRGuildsAndKingdoms.src.gui
{
	// Token: 0x0200007F RID: 127
	[NullableContext(1)]
	[Nullable(0)]
	internal class ConfirmDeleteQuestDialog : GuiDialog
	{
		// Token: 0x17000184 RID: 388
		// (get) Token: 0x0600057C RID: 1404 RVA: 0x00024FF0 File Offset: 0x000231F0
		public override string ToggleKeyCombinationCode
		{
			get
			{
				return null;
			}
		}

		// Token: 0x0600057D RID: 1405 RVA: 0x00024FF3 File Offset: 0x000231F3
		public ConfirmDeleteQuestDialog(ICoreClientAPI capi, string questTitle, Action onConfirm) : base(capi)
		{
			this.questTitle = questTitle;
			this.onConfirm = onConfirm;
		}

		// Token: 0x0600057E RID: 1406 RVA: 0x0002500A File Offset: 0x0002320A
		public override void OnGuiOpened()
		{
			base.OnGuiOpened();
			this.SetupDialog();
		}

		// Token: 0x0600057F RID: 1407 RVA: 0x00025018 File Offset: 0x00023218
		private void SetupDialog()
		{
			ElementBounds dialogBounds = ElementStdBounds.AutosizedMainDialog.WithAlignment(6);
			ElementBounds bgBounds = ElementBounds.Fill.WithFixedPadding(GuiStyle.ElementToDialogPadding);
			bgBounds.BothSizing = 2;
			bgBounds.WithChildren(new ElementBounds[]
			{
				ElementBounds.Fixed(0.0, 0.0, 450.0, 200.0)
			});
			base.SingleComposer = GuiComposerHelpers.AddDialogTitleBar(GuiComposerHelpers.AddShadedDialogBG(this.capi.Gui.CreateCompo("quest-delete-confirm", dialogBounds), bgBounds, true, 5.0, 0.75f), "Confirm Delete", new Action(this.OnTitleBarClose), null, null, null).BeginChildElements(bgBounds);
			string message = "Are you sure you want to delete the quest '" + this.questTitle + "'?\n\nThis action cannot be undone and will remove all player progress for this quest.";
			ElementBounds textBounds = ElementBounds.Fixed(10.0, 30.0, 430.0, 120.0);
			GuiComposerHelpers.AddStaticText(base.SingleComposer, message, CairoFont.WhiteDetailText(), textBounds, null);
			ElementBounds confirmBounds = ElementBounds.Fixed(10.0, 160.0, 150.0, 30.0);
			ElementBounds cancelBounds = ElementBounds.Fixed(170.0, 160.0, 150.0, 30.0);
			GuiComposerHelpers.AddSmallButton(base.SingleComposer, "Delete", new ActionConsumable(this.OnConfirmClick), confirmBounds, 2, null);
			GuiComposerHelpers.AddSmallButton(base.SingleComposer, "Cancel", new ActionConsumable(this.OnCancelClick), cancelBounds, 2, null);
			base.SingleComposer.EndChildElements().Compose(true);
		}

		// Token: 0x06000580 RID: 1408 RVA: 0x000251CF File Offset: 0x000233CF
		private void OnTitleBarClose()
		{
			this.TryClose();
		}

		// Token: 0x06000581 RID: 1409 RVA: 0x000251D8 File Offset: 0x000233D8
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

		// Token: 0x06000582 RID: 1410 RVA: 0x000251F3 File Offset: 0x000233F3
		private bool OnCancelClick()
		{
			this.TryClose();
			return true;
		}

		// Token: 0x0400021E RID: 542
		private readonly string questTitle;

		// Token: 0x0400021F RID: 543
		private readonly Action onConfirm;
	}
}
