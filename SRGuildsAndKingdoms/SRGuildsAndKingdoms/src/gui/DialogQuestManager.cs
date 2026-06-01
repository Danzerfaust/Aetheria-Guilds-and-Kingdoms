using SRGuildsAndKingdoms.src.network;
using SRGuildsAndKingdoms.src.quests;
using System;
using System.Collections.Generic;
using System.Linq;
using Vintagestory.API.Client;

namespace SRGuildsAndKingdoms.src.gui
{
    /// <summary>
    /// Admin dialog for managing quests (viewing, editing, creating)
    /// </summary>
    public class DialogQuestManager : GuiDialog
    {
        private readonly SRGuildsAndKingdomsModSystem modSystem;
        private List<QuestDto> quests = [];
        private bool isLoading = true;
        private string? errorMessage;

        // Currency definitions from server
        public CurrencyDefinitionDto? TailsDefinition { get; private set; }
        public CurrencyDefinitionDto? CrownsDefinition { get; private set; }

        // Server time information
        private long serverLocalTime;
        private double serverTimezoneOffset;

        private const double DIALOG_WIDTH = 800;
        private const double DIALOG_HEIGHT = 500;
        private const double ROW_HEIGHT = 40;
        private const double BUTTON_WIDTH = 80;

        private ElementBounds? scrollableBounds;
        private readonly List<ElementBounds> scrollableChildBounds = [];
        private double visibleHeight;
        private double contentHeight;

        private int selectedFilterIndex = 0;
        private static readonly string[] FilterOptions = ["All", "Weekly", "Monthly", "Seasonal"];

        private DialogQuestEditor? openEditorDialog;

        public override string ToggleKeyCombinationCode => "questmanager";

        public DialogQuestManager(ICoreClientAPI capi, SRGuildsAndKingdomsModSystem modSystem) : base(capi)
        {
            this.modSystem = modSystem;

            // Subscribe to quest manager list response
            var questNetworkHandler = modSystem.QuestNetworkHandler;
            if (questNetworkHandler != null)
            {
                questNetworkHandler.OnQuestManagerListReceived += OnQuestListReceived;
            }

            SetupDialog();
            RequestQuestList();
        }

        public override void OnGuiClosed()
        {
            base.OnGuiClosed();

            // Clean up editor dialog reference
            if (openEditorDialog != null)
            {
                openEditorDialog.TryClose();
                openEditorDialog = null;
            }

            // Unsubscribe from events
            var questNetworkHandler = modSystem.QuestNetworkHandler;
            if (questNetworkHandler != null)
            {
                questNetworkHandler.OnQuestManagerListReceived -= OnQuestListReceived;
            }
        }

        private void RequestQuestList()
        {
            isLoading = true;
            errorMessage = null;

            var questNetworkHandler = modSystem.QuestNetworkHandler;
            var playerUid = capi.World.Player?.PlayerUID ?? string.Empty;

            if (questNetworkHandler != null && !string.IsNullOrEmpty(playerUid))
            {
                questNetworkHandler.RequestQuestManagerList(playerUid);
            }
            else
            {
                isLoading = false;
                errorMessage = "Failed to request quest list";
                SetupDialog();
            }
        }

        private void OnQuestListReceived(QuestManagerListResponsePacket packet)
        {
            isLoading = false;

            if (!packet.Success)
            {
                errorMessage = packet.Message;
                quests = [];
            }
            else
            {
                errorMessage = null;
                quests = packet.Quests;

                // Store currency definitions from server
                TailsDefinition = packet.TailsDefinition;
                CrownsDefinition = packet.CrownsDefinition;

                // Store server time information
                serverLocalTime = packet.ServerLocalTime;
                serverTimezoneOffset = packet.ServerTimezoneOffset;
            }

            SetupDialog();
        }

        private void SetupDialog()
        {
            ElementBounds dialogBounds = ElementStdBounds.AutosizedMainDialog.WithAlignment(EnumDialogArea.CenterMiddle);

            ElementBounds bgBounds = ElementBounds.Fill.WithFixedPadding(GuiStyle.ElementToDialogPadding);
            bgBounds.BothSizing = ElementSizing.FitToChildren;

            var composer = capi.Gui.CreateCompo("questmanager", dialogBounds)
                .AddShadedDialogBG(bgBounds)
                .AddDialogTitleBar("Quest Manager", OnTitleBarCloseClicked)
                .BeginChildElements(bgBounds);

            double yPos = 30;

            if (isLoading)
            {
                composer.AddStaticText(
                    "Loading quests...",
                    CairoFont.WhiteSmallText(),
                    ElementBounds.Fixed(0, yPos, DIALOG_WIDTH - 40, 30)
                );
            }
            else if (!string.IsNullOrEmpty(errorMessage))
            {
                composer.AddStaticText(
                    $"Error: {errorMessage}",
                    CairoFont.WhiteSmallText().WithColor(new double[] { 1, 0.3, 0.3, 1 }),
                    ElementBounds.Fixed(0, yPos, DIALOG_WIDTH - 40, 30)
                );

                yPos += 40;

                composer.AddSmallButton(
                    "Retry",
                    OnRetryClicked,
                    ElementBounds.Fixed(0, yPos, BUTTON_WIDTH, 30)
                );
            }
            else
            {
                composer.AddStaticText(
                    $"Total Quests: {quests.Count}",
                    CairoFont.WhiteSmallText(),
                    ElementBounds.Fixed(0, yPos, 200, 25)
                );

                // Filter dropdown
                composer.AddStaticText(
                    "Show:",
                    CairoFont.WhiteSmallText(),
                    ElementBounds.Fixed(DIALOG_WIDTH + 80 - 330, yPos + 3, 50, 25)
                );

                composer.AddDropDown(
                    FilterOptions,
                    FilterOptions,
                    selectedFilterIndex,
                    OnFilterChanged,
                    ElementBounds.Fixed(DIALOG_WIDTH + 80 - 280, yPos - 3, 120, 28),
                    "filterDropdown"
                );

                composer.AddSmallButton(
                    "Add New",
                    OnAddNewClicked,
                    ElementBounds.Fixed(DIALOG_WIDTH + 80 - 120, yPos - 5, BUTTON_WIDTH, 30)
                );

                yPos += 35;

                double colTitleWidth = 280;
                double colTypeWidth = 90;
                double colRankWidth = 80;
                double colDatesWidth = 230;
                double colStatusWidth = 80;

                composer.AddStaticText("Title", CairoFont.WhiteSmallText().WithWeight(Cairo.FontWeight.Bold),
                    ElementBounds.Fixed(0, yPos, colTitleWidth, 20));
                composer.AddStaticText("Type", CairoFont.WhiteSmallText().WithWeight(Cairo.FontWeight.Bold),
                    ElementBounds.Fixed(colTitleWidth, yPos, colTypeWidth, 20));
                composer.AddStaticText("Rank", CairoFont.WhiteSmallText().WithWeight(Cairo.FontWeight.Bold),
                    ElementBounds.Fixed(colTitleWidth + colRankWidth, yPos, colRankWidth, 20));
                composer.AddStaticText("Dates", CairoFont.WhiteSmallText().WithWeight(Cairo.FontWeight.Bold),
                    ElementBounds.Fixed(colTitleWidth + colRankWidth + colTypeWidth, yPos, colDatesWidth, 20));
                composer.AddStaticText("Status", CairoFont.WhiteSmallText().WithWeight(Cairo.FontWeight.Bold),
                    ElementBounds.Fixed(colTitleWidth + colRankWidth + colTypeWidth + colDatesWidth, yPos, colStatusWidth, 20));

                yPos += 25;

                // Filter quests by selected type
                var filteredQuests = selectedFilterIndex == 0
                    ? quests
                    : quests.Where(q => q.RecurrenceType.Equals(FilterOptions[selectedFilterIndex], StringComparison.OrdinalIgnoreCase)).ToList();

                // Sort quests by status (Active first), then by type, then by start date (latest first)
                var sortedQuests = filteredQuests
                    .OrderBy(q => GetStatusSortOrder(GetQuestStatus(q)))
                    .ThenBy(q => q.RecurrenceType)
                    .ThenByDescending(q => q.StartsAt)
                    .ToList();

                visibleHeight = DIALOG_HEIGHT - yPos - 60;
                contentHeight = Math.Max(visibleHeight, sortedQuests.Count * ROW_HEIGHT + 20);

                var clippingBounds = ElementBounds.Fixed(0, yPos, DIALOG_WIDTH + 80 - 60, visibleHeight);

                scrollableBounds = ElementBounds.Fixed(0, 0, DIALOG_WIDTH + 80 - 80, contentHeight);
                scrollableBounds.WithParent(clippingBounds);

                var insetBounds = clippingBounds.FlatCopy().FixedGrow(6).WithFixedOffset(-3, -3);

                var scrollbarBounds = insetBounds.CopyOffsetedSibling(clippingBounds.fixedWidth + 7).WithFixedWidth(20);

                composer.AddInset(insetBounds, 3);

                composer.BeginClip(clippingBounds);

                scrollableChildBounds.Clear();

                ElementBounds CreateChildBounds(double x, double y, double width, double height)
                {
                    var bounds = ElementBounds.Fixed(x, y, width, height);
                    bounds.WithParent(scrollableBounds);
                    scrollableChildBounds.Add(bounds);
                    return bounds;
                }

                double rowY = 5;
                for (int i = 0; i < sortedQuests.Count; i++)
                {
                    var quest = sortedQuests[i];
                    int questIndex = quests.IndexOf(quest);

                    string status = GetQuestStatus(quest);
                    double[] statusColor = GetStatusColor(status);

                    string displayTitle = quest.Title.Length > 35 ? quest.Title[..32] + "..." : quest.Title;
                    composer.AddDynamicText(
                        displayTitle,
                        CairoFont.WhiteSmallText(),
                        CreateChildBounds(5, rowY + 10, colTitleWidth - 5, 20),
                        $"quest_{i}_title"
                    );

                    composer.AddDynamicText(
                        CapitalizeFirst(quest.RecurrenceType),
                        CairoFont.WhiteSmallText(),
                        CreateChildBounds(colTitleWidth, rowY + 10, colRankWidth - 5, 20),
                        $"quest_{i}_type"
                    );

                    composer.AddDynamicText(
                        CapitalizeFirst(quest.Rank),
                        CairoFont.WhiteSmallText(),
                        CreateChildBounds(colTitleWidth + colRankWidth, rowY + 10, colTypeWidth - 5, 20),
                        $"quest_{i}_rank"
                    );

                    string dateDisplay = $"{quest.StartsAt} - {quest.ExpiresAt}";
                    composer.AddDynamicText(
                        dateDisplay,
                        CairoFont.WhiteSmallText(),
                        CreateChildBounds(colTitleWidth + colRankWidth + colTypeWidth, rowY + 10, colDatesWidth - 5, 20),
                        $"quest_{i}_dates"
                    );

                    composer.AddDynamicText(
                        status,
                        CairoFont.WhiteSmallText().WithColor(statusColor),
                        CreateChildBounds(colTitleWidth + colRankWidth + colTypeWidth + colDatesWidth, rowY + 10, colStatusWidth - 5, 20),
                        $"quest_{i}_status"
                    );

                    composer.AddSmallButton(
                        "Edit",
                        () => OnEditQuestClicked(questIndex),
                        CreateChildBounds(DIALOG_WIDTH + 80 - 120, rowY + 5, 60, 28)
                    );

                    rowY += ROW_HEIGHT;
                }

                composer.EndClip();

                // Add scrollbar after clip
                composer.AddVerticalScrollbar(
                    OnNewScrollbarValue,
                    scrollbarBounds,
                    "questListScrollbar"
                );

                yPos += visibleHeight + 10;

                // Refresh button at bottom
                composer.AddSmallButton(
                    "Refresh",
                    OnRefreshClicked,
                    ElementBounds.Fixed(0, yPos, BUTTON_WIDTH, 30)
                );
            }

            composer.EndChildElements();
            SingleComposer = composer.Compose();

            // Set scrollbar heights after composition
            if (scrollableBounds != null)
            {
                SingleComposer.GetScrollbar("questListScrollbar")?.SetHeights(
                    (float)visibleHeight,
                    (float)contentHeight
                );
            }
        }

        private void OnNewScrollbarValue(float value)
        {
            if (scrollableBounds == null) return;

            // Adjust the Y position of the scrollable content (value is in pixels)
            scrollableBounds.fixedY = -value;
            scrollableBounds.CalcWorldBounds();

            // Recalculate all child bounds since CalcWorldBounds doesn't propagate to children
            foreach (var childBounds in scrollableChildBounds)
            {
                childBounds.CalcWorldBounds();
            }
        }

        private string GetQuestStatus(QuestDto quest)
        {
            try
            {
                DateTime now;
                if (quest.UsesIngameTime)
                {
                    now = GetCurrentIngameDate();
                }
                else
                {
                    // Use server local time, not UTC
                    // Reconstruct server's current date from the timestamp we received
                    now = serverLocalTime > 0
                        ? DateTimeOffset.FromUnixTimeSeconds(serverLocalTime).ToOffset(TimeSpan.FromHours(serverTimezoneOffset)).DateTime
                        : DateTime.Now; // Fallback to client time if server time not available
                }

                if (QuestPeriodKeyGenerator.TryParseDate(quest.StartsAt, out var startsAt) &&
                    QuestPeriodKeyGenerator.TryParseDate(quest.ExpiresAt, out var expiresAt))
                {
                    if (now.Date < startsAt.Date)
                        return "Future";
                    if (now.Date > expiresAt.Date)
                        return "Expired";
                    return "Active";
                }
            }
            catch
            {
                // Fall through to unknown
            }

            return "Unknown";
        }

        private static int GetStatusSortOrder(string status)
        {
            return status switch
            {
                "Active" => 0,
                "Future" => 1,
                "Expired" => 2,
                _ => 3
            };
        }

        private DateTime GetCurrentIngameDate()
        {
            var calendar = capi.World.Calendar;
            if (calendar == null) return DateTime.Now;

            return new DateTime(
                calendar.Year + 1,
                calendar.Month,
                (calendar.DayOfYear % calendar.DaysPerMonth) + 1
            );
        }

        private static double[] GetStatusColor(string status)
        {
            return status switch
            {
                "Active" => [0.3, 1, 0.3, 1],   // Green
                "Future" => [0.3, 0.7, 1, 1],   // Blue
                "Expired" => [0.6, 0.6, 0.6, 1], // Gray
                _ => [1, 1, 1, 1]                // White
            };
        }

        private static string CapitalizeFirst(string s)
        {
            if (string.IsNullOrEmpty(s)) return s;
            return char.ToUpper(s[0]) + s[1..];
        }

        private void OnTitleBarCloseClicked()
        {
            TryClose();
        }

        private bool OnRetryClicked()
        {
            RequestQuestList();
            return true;
        }

        private bool OnRefreshClicked()
        {
            RequestQuestList();
            return true;
        }

        private void OnFilterChanged(string code, bool selected)
        {
            selectedFilterIndex = Array.IndexOf(FilterOptions, code);
            if (selectedFilterIndex < 0) selectedFilterIndex = 0;
            SetupDialog();
        }

        private bool OnAddNewClicked()
        {
            // If editor is already open, close it first
            if (openEditorDialog != null && openEditorDialog.IsOpened())
            {
                openEditorDialog.TryClose();
            }

            // Clean up any disposed reference
            openEditorDialog = null;

            // Create new editor with currency definitions
            openEditorDialog = new DialogQuestEditor(capi, modSystem, null, TailsDefinition, CrownsDefinition, serverLocalTime, serverTimezoneOffset);

            // Subscribe to close event to clean up reference
            openEditorDialog.OnDialogClosed += () =>
            {
                openEditorDialog = null;
            };

            openEditorDialog.TryOpen();
            return true;
        }

        private bool OnEditQuestClicked(int questIndex)
        {
            if (questIndex < 0 || questIndex >= quests.Count) return false;

            // If editor is already open, close it first
            if (openEditorDialog != null && openEditorDialog.IsOpened())
            {
                openEditorDialog.TryClose();
            }

            // Clean up any disposed reference
            openEditorDialog = null;

            // Create new editor with quest and currency definitions
            var quest = quests[questIndex];
            openEditorDialog = new DialogQuestEditor(capi, modSystem, quest, TailsDefinition, CrownsDefinition, serverLocalTime, serverTimezoneOffset);

            // Subscribe to events
            openEditorDialog.OnDialogClosed += () =>
            {
                openEditorDialog = null;
            };

            openEditorDialog.OnQuestSaved += () =>
            {
                // Refresh quest list when a quest is saved
                RequestQuestList();
            };

            openEditorDialog.TryOpen();
            return true;
        }
    }
}
