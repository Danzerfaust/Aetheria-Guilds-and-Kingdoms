using SOAGuildsAndKingdoms.src.gui.components;
using SOAGuildsAndKingdoms.src.network;
using System;
using System.Collections.Generic;
using Vintagestory.API.Client;
using Vintagestory.API.Common;

namespace SOAGuildsAndKingdoms.src.gui
{
    public class DialogEventManager : GuiDialog
    {
        private readonly SOAGuildsAndKingdomsModSystem modSystem;

        public DialogEventManager(ICoreClientAPI capi, SOAGuildsAndKingdomsModSystem modSystem) : base(capi)
        {
            this.modSystem = modSystem;

            var eventNetworkHandler = modSystem.EventClientNetworkHandler;
            if (eventNetworkHandler != null)
            {
                eventNetworkHandler.OnEventListReceived += OnEventListReceived;
            }

            RequestEventList();
        }

        public override string? ToggleKeyCombinationCode => null;

        #region Network

        public void RequestEventList()
        {
            isLoading = true;

            modSystem.EventClientNetworkHandler?.RequestEventList();
        }

        public void OnEventListReceived(List<EventDto> list)
        {
            isLoading = false;
            events = list;

            ComposeDialog();
        }

        #endregion

        #region Defaults

        private const string DialogTitle = "Event Manager";
        private const string DialogId = "event-manager-dialog";

        private readonly int DialogWidth = 600;
        private readonly int ScrollAreaHeight = 200;
        private readonly int ScrollbarWidth = 20;
        private readonly int ColumnsHeight = 20;

        private readonly int EventInsetHeight = 30;
        private readonly int OffsetEventId = 5;
        private readonly int OffsetEventName = 45;
        private readonly int OffsetEventStatus = 400;
        private readonly int OffsetEventRegistrations = 470;
        private readonly int OffsetEventActions = 540;

        private readonly CairoFont Font = CairoFont.WhiteSmallishText().WithFontSize(12);

        #endregion

        #region State

        private bool isLoading = true;
        private List<EventDto> events = [];
        private GuiElementListMenu? listMenu;
        private DialogEventEditor? openEditorDialog;

        #endregion

        #region Dialog composition

        private void ComposeDialog()
        {
            ClearComposers();

            ElementBounds dialogBounds = ElementStdBounds.AutosizedMainDialog.WithAlignment(EnumDialogArea.CenterMiddle);

            var scrollInsetBounds = ElementBounds.Fixed(0, GuiStyle.TitleBarHeight + ColumnsHeight, DialogWidth, ScrollAreaHeight);
            var scrollbarBounds = scrollInsetBounds.RightCopy().WithFixedWidth(ScrollbarWidth);

            var clipBounds = scrollInsetBounds.ForkContainingChild();
            var containerBounds = scrollInsetBounds.ForkContainingChild();

            var bgBounds = ElementBounds.Fill
                .WithFixedPadding(GuiStyle.ElementToDialogPadding)
                .WithSizing(ElementSizing.FitToChildren)
                .WithChildren(scrollInsetBounds, scrollbarBounds);

            var composer = capi.Gui.CreateCompo(DialogId, dialogBounds)
                .AddShadedDialogBG(bgBounds)
                .AddDialogTitleBar(DialogTitle, OnTitleBarClose, Font.Clone().WithFontSize(14))
                .BeginChildElements(bgBounds);

            composer.AddStaticText("ID", Font, ElementBounds.Fixed(OffsetEventId, GuiStyle.TitleBarHeight, OffsetEventName - OffsetEventId, ColumnsHeight));
            composer.AddStaticText("Name", Font, ElementBounds.Fixed(OffsetEventName, GuiStyle.TitleBarHeight, OffsetEventStatus - OffsetEventName, ColumnsHeight));
            composer.AddStaticText("Status", Font, ElementBounds.Fixed(OffsetEventStatus, GuiStyle.TitleBarHeight, OffsetEventRegistrations - OffsetEventStatus, ColumnsHeight));
            composer.AddStaticText("Players", Font, ElementBounds.Fixed(OffsetEventRegistrations, GuiStyle.TitleBarHeight, OffsetEventActions - OffsetEventRegistrations, ColumnsHeight));
            composer.AddStaticText("Actions", Font, ElementBounds.Fixed(OffsetEventActions, GuiStyle.TitleBarHeight, DialogWidth - OffsetEventActions, ColumnsHeight));

            composer
                .AddInset(scrollInsetBounds)
                .BeginClip(clipBounds)
                .AddContainer(containerBounds, "scroll-content")
                .EndClip()
                .AddInteractiveElement(new GuiElementQuestScrollbar(capi, OnNewScrollbarValue, scrollbarBounds), "scrollbar");

            GuiElementContainer scrollArea = composer.GetContainer("scroll-content");
            double contentHeight = AddEventList(scrollArea);

            float scrollVisibleHeight = (float)clipBounds.fixedHeight;

            if (contentHeight < scrollVisibleHeight)
            {
                scrollInsetBounds.fixedHeight = contentHeight;
                clipBounds.fixedHeight = contentHeight;
                scrollbarBounds.fixedHeight = contentHeight;

                bgBounds.CalcWorldBounds();

                scrollVisibleHeight = (float)contentHeight;
            }


            composer.EndChildElements();

            SingleComposer = composer.Compose();

            float scrollTotalHeight = (float)Math.Max(contentHeight, scrollVisibleHeight);
            SingleComposer.GetScrollbar("scrollbar").SetHeights(scrollVisibleHeight, scrollTotalHeight);

            Composers["eventmanager"] = composer;
        }

        private double AddEventList(GuiElementContainer scrollArea)
        {
            double yPos = 0;

            if (isLoading)
            {
                yPos += 18;

                scrollArea.Add(new GuiElementStaticText(capi, "Loading events...", EnumTextOrientation.Center, ElementBounds.Fixed(0, yPos, DialogWidth, 30), Font));

                yPos += 30;
            }
            else if (events.Count == 0)
            {
                yPos += 18;

                scrollArea.Add(new GuiElementStaticText(capi, "No events found.", EnumTextOrientation.Center, ElementBounds.Fixed(0, yPos, DialogWidth, 30), Font));

                yPos += 30;
            }
            else
            {
                for (int i = 0; i < events.Count; i++)
                {
                    EventDto eventDto = events[i];

                    // Encapsulating inset
                    var insetBounds = ElementBounds.Fixed(0, yPos, DialogWidth, EventInsetHeight);
                    var inset = new components.GuiElementPartyMemberInset(capi, insetBounds, 0, 1f, 0.85f);
                    scrollArea.Add(inset);

                    // ID
                    var idBounds = ElementBounds.Fixed(OffsetEventId, yPos + 8, OffsetEventName - OffsetEventId, EventInsetHeight);
                    var idComponents = VtmlUtil.Richtextify(
                        capi,
                        $"{eventDto.Id}",
                        Font
                    );
                    scrollArea.Add(new GuiElementRichtext(capi, idComponents, idBounds));

                    // Name
                    var nameBounds = ElementBounds.Fixed(OffsetEventName, yPos + 8, OffsetEventStatus - OffsetEventName, EventInsetHeight);
                    var nameComponents = VtmlUtil.Richtextify(
                        capi,
                        $"{eventDto.Name}",
                        Font
                    );
                    scrollArea.Add(new GuiElementRichtext(capi, nameComponents, nameBounds));

                    // Status
                    var (statusText, statusColor) = GetStatusFromDate(eventDto.StartDate, eventDto.EndDate);
                    var statusBounds = ElementBounds.Fixed(OffsetEventStatus, yPos + 8, OffsetEventRegistrations - OffsetEventStatus, EventInsetHeight);
                    var statusComponents = VtmlUtil.Richtextify(
                        capi,
                        $"<font color={statusColor}>{statusText}</font>",
                        Font
                    );
                    scrollArea.Add(new GuiElementRichtext(capi, statusComponents, statusBounds));

                    // Registrations
                    var registrationBounds = ElementBounds.Fixed(OffsetEventRegistrations, yPos + 8, OffsetEventActions - OffsetEventRegistrations, EventInsetHeight);
                    var registrationComponents = VtmlUtil.Richtextify(
                        capi,
                        $"{eventDto.Registrations.Count}/{eventDto.MaxPlayers}",
                        Font
                    );
                    scrollArea.Add(new GuiElementRichtext(capi, registrationComponents, registrationBounds));

                    // Actions
                    // + 10 to center
                    var dropdownTriggerBounds = ElementBounds.Fixed(OffsetEventActions + 10, yPos + 4, 20, 20);
                    var dropdownBounds = ElementBounds.Fixed(OffsetEventActions + 10, yPos + 4, 20, 20);
                    var menu = new GuiElementListMenu(capi, [null, "edit", "manage"], ["...", "Edit", "Manage"], 0, (code, selected) => OnEventAction(code, selected, eventDto), dropdownTriggerBounds, Font, false);

                    scrollArea.Add(menu);
                    scrollArea.Add(new GuiElementIconButton(capi, "wpGear", "", Font, obj =>
                    {
                        if (listMenu != null)
                        {
                            listMenu.OnFocusLost();
                            listMenu.SelectedIndex = -1;
                        }

                        listMenu = menu;
                        listMenu.SelectedIndex = -1;
                        listMenu.Open();
                    }, dropdownTriggerBounds, false));

                    yPos += EventInsetHeight;
                }
            }

            return yPos;
        }

        #endregion

        #region Behaviors

        private static (string status, string color) GetStatusFromDate(string startDate, string endDate)
        {
            if (DateTimeOffset.TryParse(startDate, out DateTimeOffset start) && DateTimeOffset.TryParse(endDate, out DateTimeOffset end))
            {
                DateTimeOffset now = DateTimeOffset.UtcNow;

                if (now < start)
                {
                    return ("Future", "#4cb1ff");
                }
                else if (now >= start && now <= end)
                {
                    return ("Ongoing", "#4cff4c");
                }
                else
                {
                    return ("Past", "#999999");
                }
            }
            else
            {
                return ("Unknown", "#333333");
            }
        }

        private void OnEventAction(string code, bool selected, EventDto eventDto)
        {
            if (code == "edit")
            {
                capi.ShowChatMessage($"Editing {eventDto.Name}...");
                OpenEventEditor(eventDto);
            }
            else if (code == "manage")
            {
                capi.ShowChatMessage($"Managing {eventDto.Name}...");
                // TODO: implement event manager
            }
        }

        private void OnNewScrollbarValue(float value)
        {
            ElementBounds bounds = SingleComposer.GetContainer("scroll-content").Bounds;
            bounds.fixedY = 0 - value;
            bounds.CalcWorldBounds();
        }

        private void OpenEventEditor(EventDto eventDto)
        {
            if (openEditorDialog != null && openEditorDialog.IsOpened())
            {
                openEditorDialog.TryClose();
                openEditorDialog.Dispose();
            }

            openEditorDialog = null;

            openEditorDialog = new DialogEventEditor(capi, modSystem, eventDto);

            openEditorDialog.TryOpen();
        }

        private void OnTitleBarClose()
        {
            TryClose();
        }

        #endregion
    }
}