using SOAGuildsAndKingdoms.src.gui.components;
using SOAGuildsAndKingdoms.src.guilds;
using System;
using Vintagestory.API.Client;
using Vintagestory.API.Config;

namespace SOAGuildsAndKingdoms.src.gui.dialogs
{
    internal class QuestDialog : GuiDialog
    {
        private static QuestDialog? currentlyOpenDialog;

        private readonly SOAGuildsAndKingdomsModSystem modSystem;
        private readonly string? questType;
        private GuildSummary? currentGuild;
        private QuestDialogContent? questContent;

        public QuestDialog(ICoreClientAPI capi, SOAGuildsAndKingdomsModSystem modSystem, string? questType = null) : base(capi)
        {
            this.modSystem = modSystem;
            this.questType = questType;
            currentGuild = modSystem.GetCurrentPlayerGuildSummary();
            SetupDialog();
        }

        public static void CloseCurrentDialog()
        {
            currentlyOpenDialog?.TryClose();
            currentlyOpenDialog = null;
        }

        public override string ToggleKeyCombinationCode => "guildquests";

        // Prevent itemstacks from rendering on top of other dialogs (important)
        public override float ZSize => 1000;

        public override bool PrefersUngrabbedMouse => false;

        private void SetupDialog()
        {
            currentGuild = modSystem.GetCurrentPlayerGuildSummary();

            if (currentGuild == null)
            {
                SetupNoGuildDialog();
                return;
            }

            questContent ??= new QuestDialogContent(capi, modSystem, currentGuild, OnLeaveGuild, SetupDialog, questType);

            int dialogWidth = 640;
            int tabAreaHeight = 50;
            int scrollAreaHeight = 580;
            int scrollbarWidth = 20;

            var dialogBounds = ElementStdBounds.AutosizedMainDialog.WithAlignment(EnumDialogArea.CenterMiddle);

            var tabAreaBounds = ElementBounds.Fixed(0, GuiStyle.TitleBarHeight, dialogWidth, tabAreaHeight);

            var scrollInsetBounds = ElementBounds.Fixed(0, GuiStyle.TitleBarHeight + tabAreaHeight, dialogWidth, scrollAreaHeight);
            var scrollbarBounds = scrollInsetBounds.RightCopy().WithFixedWidth(scrollbarWidth);

            var clipBounds = scrollInsetBounds.ForkContainingChild(GuiStyle.HalfPadding, 0, GuiStyle.HalfPadding, 0);
            var containerBounds = scrollInsetBounds.ForkContainingChild(GuiStyle.HalfPadding, GuiStyle.HalfPadding, GuiStyle.HalfPadding, GuiStyle.HalfPadding);

            var bgBounds = ElementBounds.Fill
                .WithFixedPadding(GuiStyle.ElementToDialogPadding)
                .WithSizing(ElementSizing.FitToChildren)
                .WithChildren(tabAreaBounds, scrollInsetBounds, scrollbarBounds);

            var composer = capi.Gui.CreateCompo("guildquests", dialogBounds)
                .AddQuestDialogBG(bgBounds)
                .AddDialogTitleBar(Lang.Get("soaguildsandkingdoms:quests-tab-title"), OnTitleBarCloseClicked)
                .BeginChildElements(bgBounds);

            questContent.AddPeriodTabsToComposer(composer, GuiStyle.HalfPadding + 25, 655, 5, questType);

            composer.BeginClip(clipBounds)
                        .AddContainer(containerBounds, "scroll-content")
                    .EndClip()
                    .AddInteractiveElement(new GuiElementQuestScrollbar(capi, OnNewScrollbarValue, scrollbarBounds), "scrollbar")
                .EndChildElements();

            GuiElementContainer scrollArea = composer.GetContainer("scroll-content");
            double contentHeight = questContent.AddQuestContentAsElements(scrollArea, 2);

            SingleComposer = composer.Compose();

            float scrollVisibleHeight = (float)clipBounds.fixedHeight;
            float scrollTotalHeight = (float)Math.Max(contentHeight, scrollVisibleHeight + 1);
            SingleComposer.GetScrollbar("scrollbar").SetHeights(scrollVisibleHeight, scrollTotalHeight);
        }

        private void SetupNoGuildDialog()
        {
            var dialogBounds = ElementStdBounds.AutosizedMainDialog.WithAlignment(EnumDialogArea.CenterMiddle);
            var bgBounds = ElementBounds.Fill.WithFixedPadding(GuiStyle.ElementToDialogPadding);
            bgBounds.BothSizing = ElementSizing.FitToChildren;

            var composer = capi.Gui.CreateCompo("guildquests", dialogBounds)
                .AddShadedDialogBG(bgBounds)
                .AddDialogTitleBar(Lang.Get("soaguildsandkingdoms:quest-dialog-title"), OnTitleBarCloseClicked)
                .BeginChildElements(bgBounds);

            composer.AddStaticText(
                Lang.Get("soaguildsandkingdoms:no-guild-message"),
                CairoFont.WhiteDetailText(),
                ElementBounds.Fixed(0, 20, 400, 50)
            );

            SingleComposer = composer.Compose();
        }

        private bool OnLeaveGuild()
        {
            TryClose();
            return true;
        }

        private void OnNewScrollbarValue(float value)
        {
            ElementBounds bounds = SingleComposer.GetContainer("scroll-content").Bounds;
            bounds.fixedY = 0 - value;
            bounds.CalcWorldBounds();
        }

        private void OnTitleBarCloseClicked()
        {
            TryClose();
        }

        public override void OnGuiOpened()
        {
            base.OnGuiOpened();
            currentlyOpenDialog = this;
            SetupDialog();
        }

        public override void OnGuiClosed()
        {
            base.OnGuiClosed();
            if (currentlyOpenDialog == this)
            {
                currentlyOpenDialog = null;
            }
        }
    }
}
