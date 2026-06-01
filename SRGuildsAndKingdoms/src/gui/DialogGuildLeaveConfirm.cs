using System;
using System.Runtime.CompilerServices;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;

namespace SRGuildsAndKingdoms.src.gui
{
	// Token: 0x02000076 RID: 118
	[NullableContext(1)]
	[Nullable(0)]
	internal class DialogGuildLeaveConfirm : GuiDialog
	{
		// Token: 0x060004DB RID: 1243 RVA: 0x0001D5CA File Offset: 0x0001B7CA
		public DialogGuildLeaveConfirm(ICoreClientAPI capi, string message, Action onConfirm) : base(capi)
		{
			this.message = message;
			this.onConfirm = onConfirm;
			this.SetupDialog();
		}

		// Token: 0x17000165 RID: 357
		// (get) Token: 0x060004DC RID: 1244 RVA: 0x0001D5E7 File Offset: 0x0001B7E7
		public override string ToggleKeyCombinationCode
		{
			get
			{
				return "guildleaveconfirm";
			}
		}

		// Token: 0x060004DD RID: 1245 RVA: 0x0001D5F0 File Offset: 0x0001B7F0
		private void SetupDialog()
		{
			ElementBounds dialogBounds = ElementStdBounds.AutosizedMainDialog.WithAlignment(6);
			ElementBounds bgBounds = ElementBounds.Fill.WithFixedPadding(GuiStyle.ElementToDialogPadding);
			bgBounds.BothSizing = 2;
			GuiComposer composer = GuiComposerHelpers.AddDialogTitleBar(GuiComposerHelpers.AddShadedDialogBG(this.capi.Gui.CreateCompo("guildleaveconfirm", dialogBounds), bgBounds, true, 5.0, 0.75f), Lang.Get("srguildsandkingdoms:leave-guild-title", Array.Empty<object>()), new Action(this.OnTitleBarCloseClicked), null, null, null).BeginChildElements(bgBounds);
			double top = 20.0;
			double spacing = 15.0;
			double elementHeight = 25.0;
			double width = 400.0;
			GuiComposerHelpers.AddStaticText(composer, this.message, CairoFont.WhiteDetailText(), ElementBounds.Fixed(0.0, top, width, elementHeight * 3.0), null);
			top += elementHeight * 3.0 + spacing;
			GuiComposerHelpers.AddSmallButton(composer, Lang.Get("srguildsandkingdoms:confirm", Array.Empty<object>()), new ActionConsumable(this.OnConfirmClick), ElementBounds.Fixed(0.0, top, 100.0, elementHeight), 1, null);
			GuiComposerHelpers.AddSmallButton(composer, Lang.Get("srguildsandkingdoms:cancel", Array.Empty<object>()), new ActionConsumable(this.OnCancelClick), ElementBounds.Fixed(110.0, top, 100.0, elementHeight), 2, null);
			base.SingleComposer = composer.Compose(true);
		}

		// Token: 0x060004DE RID: 1246 RVA: 0x0001D76B File Offset: 0x0001B96B
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

		// Token: 0x060004DF RID: 1247 RVA: 0x0001D786 File Offset: 0x0001B986
		private bool OnCancelClick()
		{
			this.TryClose();
			return true;
		}

		// Token: 0x060004E0 RID: 1248 RVA: 0x0001D790 File Offset: 0x0001B990
		private void OnTitleBarCloseClicked()
		{
			this.TryClose();
		}

		// Token: 0x17000166 RID: 358
		// (get) Token: 0x060004E1 RID: 1249 RVA: 0x0001D799 File Offset: 0x0001B999
		public override bool PrefersUngrabbedMouse
		{
			get
			{
				return false;
			}
		}

		// Token: 0x040001D7 RID: 471
		private string message;

		// Token: 0x040001D8 RID: 472
		private Action onConfirm;
	}
}
