using System;
using System.Runtime.CompilerServices;
using SRGuildsAndKingdoms.src.gui.components;
using SRGuildsAndKingdoms.src.guilds;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;

namespace SRGuildsAndKingdoms.src.gui.dialogs
{
	// Token: 0x02000093 RID: 147
	[NullableContext(1)]
	[Nullable(0)]
	internal class QuestDialog : GuiDialog
	{
		// Token: 0x06000677 RID: 1655 RVA: 0x0002FDE4 File Offset: 0x0002DFE4
		public QuestDialog(ICoreClientAPI capi, SRGuildsAndKingdomsModSystem modSystem, [Nullable(2)] string questType = null) : base(capi)
		{
			this.modSystem = modSystem;
			this.questType = questType;
			this.currentGuild = modSystem.GetCurrentPlayerGuildSummary();
			this.SetupDialog();
		}

		// Token: 0x06000678 RID: 1656 RVA: 0x0002FE0D File Offset: 0x0002E00D
		public static void CloseCurrentDialog()
		{
			QuestDialog questDialog = QuestDialog.currentlyOpenDialog;
			if (questDialog != null)
			{
				questDialog.TryClose();
			}
			QuestDialog.currentlyOpenDialog = null;
		}

		// Token: 0x170001B7 RID: 439
		// (get) Token: 0x06000679 RID: 1657 RVA: 0x0002FE26 File Offset: 0x0002E026
		public override string ToggleKeyCombinationCode
		{
			get
			{
				return "guildquests";
			}
		}

		// Token: 0x170001B8 RID: 440
		// (get) Token: 0x0600067A RID: 1658 RVA: 0x0002FE2D File Offset: 0x0002E02D
		public override float ZSize
		{
			get
			{
				return 1000f;
			}
		}

		// Token: 0x170001B9 RID: 441
		// (get) Token: 0x0600067B RID: 1659 RVA: 0x0002FE34 File Offset: 0x0002E034
		public override bool PrefersUngrabbedMouse
		{
			get
			{
				return false;
			}
		}

		// Token: 0x0600067C RID: 1660 RVA: 0x0002FE38 File Offset: 0x0002E038
		private void SetupDialog()
		{
			this.currentGuild = this.modSystem.GetCurrentPlayerGuildSummary();
			if (this.currentGuild == null)
			{
				this.SetupNoGuildDialog();
				return;
			}
			if (this.questContent == null)
			{
				this.questContent = new QuestDialogContent(this.capi, this.modSystem, this.currentGuild, new ActionConsumable(this.OnLeaveGuild), new Action(this.SetupDialog), this.questType);
			}
			int dialogWidth = 640;
			int tabAreaHeight = 50;
			int scrollAreaHeight = 580;
			int scrollbarWidth = 20;
			ElementBounds dialogBounds = ElementStdBounds.AutosizedMainDialog.WithAlignment(6);
			ElementBounds tabAreaBounds = ElementBounds.Fixed(0.0, GuiStyle.TitleBarHeight, (double)dialogWidth, (double)tabAreaHeight);
			ElementBounds scrollInsetBounds = ElementBounds.Fixed(0.0, GuiStyle.TitleBarHeight + (double)tabAreaHeight, (double)dialogWidth, (double)scrollAreaHeight);
			ElementBounds scrollbarBounds = scrollInsetBounds.RightCopy(0.0, 0.0, 0.0, 0.0).WithFixedWidth((double)scrollbarWidth);
			ElementBounds clipBounds = scrollInsetBounds.ForkContainingChild(GuiStyle.HalfPadding, 0.0, GuiStyle.HalfPadding, 0.0);
			ElementBounds containerBounds = scrollInsetBounds.ForkContainingChild(GuiStyle.HalfPadding, GuiStyle.HalfPadding, GuiStyle.HalfPadding, GuiStyle.HalfPadding);
			ElementBounds bgBounds = ElementBounds.Fill.WithFixedPadding(GuiStyle.ElementToDialogPadding).WithSizing(2).WithChildren(new ElementBounds[]
			{
				tabAreaBounds,
				scrollInsetBounds,
				scrollbarBounds
			});
			GuiComposer composer = GuiComposerHelpers.AddDialogTitleBar(this.capi.Gui.CreateCompo("guildquests", dialogBounds).AddQuestDialogBG(bgBounds, true, 5.0, 0.75f), Lang.Get("srguildsandkingdoms:quests-tab-title", Array.Empty<object>()), new Action(this.OnTitleBarCloseClicked), null, null, null).BeginChildElements(bgBounds);
			this.questContent.AddPeriodTabsToComposer(composer, GuiStyle.HalfPadding + 25.0, 655, 5.0, this.questType);
			GuiElementClipHelpler.EndClip(GuiComposerHelpers.AddContainer(GuiElementClipHelpler.BeginClip(composer, clipBounds), containerBounds, "scroll-content")).AddInteractiveElement(new GuiElementQuestScrollbar(this.capi, new Action<float>(this.OnNewScrollbarValue), scrollbarBounds), "scrollbar").EndChildElements();
			GuiElementContainer scrollArea = GuiComposerHelpers.GetContainer(composer, "scroll-content");
			double val = this.questContent.AddQuestContentAsElements(scrollArea, 2.0);
			base.SingleComposer = composer.Compose(true);
			float scrollVisibleHeight = (float)clipBounds.fixedHeight;
			float scrollTotalHeight = (float)Math.Max(val, (double)(scrollVisibleHeight + 1f));
			GuiComposerHelpers.GetScrollbar(base.SingleComposer, "scrollbar").SetHeights(scrollVisibleHeight, scrollTotalHeight);
		}

		// Token: 0x0600067D RID: 1661 RVA: 0x000300D8 File Offset: 0x0002E2D8
		private void SetupNoGuildDialog()
		{
			ElementBounds dialogBounds = ElementStdBounds.AutosizedMainDialog.WithAlignment(6);
			ElementBounds bgBounds = ElementBounds.Fill.WithFixedPadding(GuiStyle.ElementToDialogPadding);
			bgBounds.BothSizing = 2;
			GuiComposer composer = GuiComposerHelpers.AddDialogTitleBar(GuiComposerHelpers.AddShadedDialogBG(this.capi.Gui.CreateCompo("guildquests", dialogBounds), bgBounds, true, 5.0, 0.75f), Lang.Get("srguildsandkingdoms:quest-dialog-title", Array.Empty<object>()), new Action(this.OnTitleBarCloseClicked), null, null, null).BeginChildElements(bgBounds);
			GuiComposerHelpers.AddStaticText(composer, Lang.Get("srguildsandkingdoms:no-guild-message", Array.Empty<object>()), CairoFont.WhiteDetailText(), ElementBounds.Fixed(0.0, 20.0, 400.0, 50.0), null);
			base.SingleComposer = composer.Compose(true);
		}

		// Token: 0x0600067E RID: 1662 RVA: 0x000301AF File Offset: 0x0002E3AF
		private bool OnLeaveGuild()
		{
			this.TryClose();
			return true;
		}

		// Token: 0x0600067F RID: 1663 RVA: 0x000301B9 File Offset: 0x0002E3B9
		private void OnNewScrollbarValue(float value)
		{
			ElementBounds bounds = GuiComposerHelpers.GetContainer(base.SingleComposer, "scroll-content").Bounds;
			bounds.fixedY = (double)(0f - value);
			bounds.CalcWorldBounds();
		}

		// Token: 0x06000680 RID: 1664 RVA: 0x000301E3 File Offset: 0x0002E3E3
		private void OnTitleBarCloseClicked()
		{
			this.TryClose();
		}

		// Token: 0x06000681 RID: 1665 RVA: 0x000301EC File Offset: 0x0002E3EC
		public override void OnGuiOpened()
		{
			base.OnGuiOpened();
			QuestDialog.currentlyOpenDialog = this;
			this.SetupDialog();
		}

		// Token: 0x06000682 RID: 1666 RVA: 0x00030200 File Offset: 0x0002E400
		public override void OnGuiClosed()
		{
			base.OnGuiClosed();
			if (QuestDialog.currentlyOpenDialog == this)
			{
				QuestDialog.currentlyOpenDialog = null;
			}
		}

		// Token: 0x040002B7 RID: 695
		[Nullable(2)]
		private static QuestDialog currentlyOpenDialog;

		// Token: 0x040002B8 RID: 696
		private readonly SRGuildsAndKingdomsModSystem modSystem;

		// Token: 0x040002B9 RID: 697
		[Nullable(2)]
		private readonly string questType;

		// Token: 0x040002BA RID: 698
		[Nullable(2)]
		private GuildSummary currentGuild;

		// Token: 0x040002BB RID: 699
		[Nullable(2)]
		private QuestDialogContent questContent;
	}
}
