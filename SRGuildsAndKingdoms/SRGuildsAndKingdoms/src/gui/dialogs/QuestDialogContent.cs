using SRGuildsAndKingdoms.src.guilds;
using SRGuildsAndKingdoms.src.network;
using SRGuildsAndKingdoms.src.quests;
using System;
using System.Collections.Generic;
using System.Linq;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;

namespace SRGuildsAndKingdoms.src.gui.dialogs
{
    public class QuestDialogContent
    {
        private readonly ICoreClientAPI capi;
        private readonly SRGuildsAndKingdomsModSystem modSystem;
        private readonly GuildSummary? currentGuild;
        private readonly ActionConsumable onLeaveGuild;
        private readonly Action? onNeedsRefresh;
        private readonly string? questType;
        private GuiComposer? composer;

        // Quest data cache
        private List<QuestDto> availableQuests = [];
        private List<PlayerQuestProgressDto> activeQuestProgress = [];
        private List<string> completedPeriodKeys = [];
        private HashSet<(int questId, string periodKey)> completedQuestsByPeriod = [];

        // Current selected period tab (0=Weekly, 1=Monthly, 2=Seasonal)
        private int selectedPeriodTab;

        private bool dataLoaded = false;
        private bool isLoading = false;
        private bool questListReceived = false;
        private bool progressReceived = false;

        public QuestDialogContent(ICoreClientAPI capi, SRGuildsAndKingdomsModSystem modSystem,
            GuildSummary? currentGuild, ActionConsumable onLeaveGuild, Action? onNeedsRefresh = null, string? questType = null)
        {
            this.capi = capi;
            this.modSystem = modSystem;
            this.currentGuild = currentGuild;
            this.onLeaveGuild = onLeaveGuild;
            this.onNeedsRefresh = onNeedsRefresh;
            this.questType = questType;

            // Set default selected tab based on quest type
            selectedPeriodTab = questType == "monthly-seasonal" ? 1 : 0;

            var questNetHandler = modSystem.QuestNetworkHandler;
            if (questNetHandler != null)
            {
                questNetHandler.OnQuestListReceived = OnQuestListReceived;
                questNetHandler.OnProgressReceived = OnProgressReceived;
                questNetHandler.OnQuestStartResponse = OnStartResponse;
                questNetHandler.OnQuestAbandonResponse = OnAbandonResponse;
                questNetHandler.OnSubmitPreviewReceived = OnSubmitPreviewReceived;
                questNetHandler.OnSubmitConfirmReceived = OnSubmitConfirmReceived;
                questNetHandler.OnQuestCompleteReceived = OnCompleteReceived;
            }
        }

        public double AddPeriodTabsToComposer(GuiComposer composer, double yPos, int magicWidth, double xOffset = 0, string? questType = null)
        {
            if (!dataLoaded) return yPos;

            // Determine which tabs to show based on quest type
            bool showWeekly = questType == null || questType == "daily-weekly";
            bool showMonthly = true;
            bool showSeasonal = true;

            int visibleTabCount = (showWeekly ? 1 : 0) + (showMonthly ? 1 : 0) + (showSeasonal ? 1 : 0);
            var tabWidth = (magicWidth / visibleTabCount) - 3.7;
            var tabHeight = 30.0;

            int tabIndex = 0;

            // Weekly tab
            if (showWeekly)
            {
                var weeklyLabel = Lang.Get("srguildsandkingdoms:quests-tab-weekly");
                var weeklyFont = IsPeriodCompleted("weekly")
                    ? CairoFont.WhiteSmallText().WithColor([0.4, 0.9, 0.4, 1.0])
                    : CairoFont.WhiteSmallText();
                composer.AddToggleButton(
                    weeklyLabel,
                    weeklyFont,
                    (on) => { OnPeriodTabClicked(0); },
                    ElementBounds.Fixed(xOffset + (tabWidth + 5) * tabIndex, yPos, tabWidth, tabHeight),
                    "quest-tab-0"
                );
                composer.GetToggleButton("quest-tab-0").SetValue(selectedPeriodTab == 0);
                tabIndex++;
            }

            // Monthly tab
            if (showMonthly)
            {
                var monthlyLabel = Lang.Get("srguildsandkingdoms:quests-tab-monthly");
                var monthlyFont = IsPeriodCompleted("monthly")
                    ? CairoFont.WhiteSmallText().WithColor([0.4, 0.9, 0.4, 1.0])
                    : CairoFont.WhiteSmallText();
                composer.AddToggleButton(
                    monthlyLabel,
                    monthlyFont,
                    (on) => { OnPeriodTabClicked(1); },
                    ElementBounds.Fixed(xOffset + (tabWidth + 5) * tabIndex, yPos, tabWidth, tabHeight),
                    "quest-tab-1"
                );
                composer.GetToggleButton("quest-tab-1").SetValue(selectedPeriodTab == 1);
                tabIndex++;
            }

            // Seasonal tab
            if (showSeasonal)
            {
                var seasonalLabel = Lang.Get("srguildsandkingdoms:quests-tab-seasonal");
                var seasonalFont = IsPeriodCompleted("seasonal")
                    ? CairoFont.WhiteSmallText().WithColor([0.4, 0.9, 0.4, 1.0])
                    : CairoFont.WhiteSmallText();
                composer.AddToggleButton(
                    seasonalLabel,
                    seasonalFont,
                    (on) => { OnPeriodTabClicked(2); },
                    ElementBounds.Fixed(xOffset + (tabWidth + 5) * tabIndex, yPos, tabWidth, tabHeight),
                    "quest-tab-2"
                );
                composer.GetToggleButton("quest-tab-2").SetValue(selectedPeriodTab == 2);
                tabIndex++;
            }

            // Store composer reference for period tab callbacks
            this.composer = composer;

            return yPos + tabHeight;
        }

        public double AddQuestContentAsElements(GuiElementContainer container, double startTop)
        {
            if (currentGuild == null) return startTop;

            if (!dataLoaded && !isLoading)
            {
                FetchQuestData();
            }

            var yPos = startTop;
            int magicWidth = 620;

            if (isLoading)
            {
                var loadingBounds = ElementBounds.Fixed(0, yPos, magicWidth, 30);
                container.Add(new GuiElementStaticText(capi, Lang.Get("srguildsandkingdoms:quests-loading"),
                    EnumTextOrientation.Center, loadingBounds, CairoFont.WhiteSmallText()));
                yPos += 35;
                return yPos;
            }

            if (!dataLoaded)
            {
                return yPos;
            }

            var filteredActiveQuests = GetActiveQuestsForCurrentTab();
            var activeQuestIds = filteredActiveQuests.Select(q => q.QuestId).ToHashSet();
            var filteredAvailableQuests = GetQuestsForCurrentTab()
                .Where(q => 
                {
                    // Always exclude if currently active
                    if (activeQuestIds.Contains(q.Id))
                        return false;

                    // For weekly quests, check if THIS SPECIFIC quest has been completed for its period
                    if (q.RecurrenceType.Equals("weekly", StringComparison.OrdinalIgnoreCase))
                        return !completedQuestsByPeriod.Contains((q.Id, q.PeriodKey));

                    // For monthly/seasonal, exclude if ANY quest completed for this period
                    return !completedPeriodKeys.Contains(q.PeriodKey);
                })
                .OrderByDescending(q => GetRankValue(q.Rank))
                .ToList();

            if (filteredActiveQuests.Count == 0 && filteredAvailableQuests.Count == 0)
            {
                yPos = AddNoQuestsMessage(container, yPos, magicWidth);
                return yPos;
            }

            if (filteredActiveQuests.Count > 0)
            {
                yPos = RenderActiveQuestsAsElements(container, yPos, magicWidth, filteredActiveQuests);
            }

            if (filteredAvailableQuests.Count > 0)
            {
                yPos = RenderAvailableQuestsAsElements(container, yPos, magicWidth, filteredAvailableQuests);
            }

            return yPos;
        }

        private double AddNoQuestsMessage(GuiElementContainer container, double yPos, int magicWidth)
        {
            var periodType = GetCurrentPeriodTypeName();
            bool hasPeriodCompleted = completedPeriodKeys.Any(k =>
                GetQuestsForCurrentTab().Any(q => q.PeriodKey == k));

            var message = hasPeriodCompleted && periodType != "weekly"
                ? Lang.Get("srguildsandkingdoms:quests-period-already-completed")
                : Lang.Get($"srguildsandkingdoms:quests-none-{periodType}");

            var messageBounds = ElementBounds.Fixed(0, yPos, magicWidth, 30);
            container.Add(new GuiElementStaticText(capi, message, EnumTextOrientation.Center,
                messageBounds, CairoFont.WhiteSmallText()));
            yPos += 25;

            if (hasPeriodCompleted && periodType != "seasonal")
            {
                var questsForTab = GetQuestsForCurrentTab();
                if (questsForTab.Count > 0 && !questsForTab[0].UsesIngameTime)
                {
                    var resetTimeDisplay = GetNextResetTimeDisplay(questsForTab[0].ExpiresAt);
                    if (!string.IsNullOrEmpty(resetTimeDisplay))
                    {
                        var resetBounds = ElementBounds.Fixed(0, yPos, magicWidth, 25);
                        container.Add(new GuiElementStaticText(capi, resetTimeDisplay, EnumTextOrientation.Center,
                            resetBounds, CairoFont.WhiteSmallText().WithFontSize(11).WithColor([0.7, 0.7, 0.7, 1.0])));
                        yPos += 10;
                    }
                }
            }

            return yPos;
        }

        private double RenderActiveQuestsAsElements(GuiElementContainer container, double yPos, int magicWidth, List<PlayerQuestProgressDto> quests)
        {
            foreach (var progressDto in quests)
            {
                yPos = RenderActiveQuestAsElement(container, yPos, magicWidth, progressDto);
                yPos += 5; // Gap between quests
            }
            return yPos;
        }

        private double RenderActiveQuestAsElement(GuiElementContainer container, double yPos, int magicWidth, PlayerQuestProgressDto progressDto)
        {
            double startY = yPos;

            // Calculate description height
            var descFont = CairoFont.WhiteSmallText().WithFontSize(11).WithColor([0.85, 0.85, 0.85, 1.0]);
            double descWidth = magicWidth - 20;
            double descHeight = capi.Gui.Text.GetMultilineTextHeight(descFont, progressDto.QuestDescription, descWidth * RuntimeEnv.GUIScale) / RuntimeEnv.GUIScale;

            int objectiveCount = progressDto.Objectives?.Count ?? 0;
            double containerHeight = 28 + descHeight + 3 + (objectiveCount * 22) + 60 + 40;

            var insetBounds = ElementBounds.Fixed(0, yPos, magicWidth, containerHeight);
            var inset = new components.GuiElementQuestInset(capi, insetBounds, 4, 0.85f);
            container.Add(inset);

            // Quest title
            var titleBounds = ElementBounds.Fixed(10, yPos + 9, magicWidth - 220, 20);
            container.Add(new GuiElementStaticText(capi, progressDto.QuestTitle, EnumTextOrientation.Left,
                titleBounds, CairoFont.WhiteSmallText().WithWeight(Cairo.FontWeight.Bold)));

            // Active badge
            var activeBounds = ElementBounds.Fixed(magicWidth - 110, yPos + 13, 100, 20);
            container.Add(new GuiElementStaticText(capi, Lang.Get("srguildsandkingdoms:quests-active"),
                EnumTextOrientation.Right, activeBounds,
                CairoFont.WhiteSmallText().WithFontSize(12).WithWeight(Cairo.FontWeight.Bold).WithColor([0.89, 0.72, 0.04, 1.0])));

            // Quest description
            var descBounds = ElementBounds.Fixed(10, yPos + 28, descWidth, descHeight);
            container.Add(new GuiElementStaticText(capi, progressDto.QuestDescription, EnumTextOrientation.Left,
                descBounds, descFont));

            // Objectives
            double objYPos = yPos + 28 + descHeight + 9;
            if (progressDto.Objectives != null && progressDto.Objectives.Count > 0)
            {
                foreach (var objective in progressDto.Objectives)
                {
                    int currentProgress = progressDto.ObjectiveProgress.GetValueOrDefault(objective.Id, 0);
                    objYPos = RenderObjectiveAsElement(container, objective, 15, objYPos, currentProgress, objective.Count);
                }
            }

            // Rewards
            double rewardsYPos = objYPos + 4;
            rewardsYPos = RenderRewardsAsElements(container, progressDto.Rewards, 15, rewardsYPos);

            // Expiration
            var expirationText = FormatExpirationDate(progressDto.ExpiresAt, progressDto.UsesIngameTime, progressDto.Rank == "D" ? null : progressDto.Rank);
            var expBounds = ElementBounds.Fixed(10, rewardsYPos + 5, 400, 20);
            container.Add(new GuiElementStaticText(capi, expirationText, EnumTextOrientation.Left,
                expBounds, CairoFont.WhiteSmallText().WithFontSize(10).WithColor([0.8, 0.8, 0.8, 1.0])));

            // Action buttons
            bool hasIncompleteTurnIn = progressDto.Objectives?.Any(obj =>
                obj.Type.Equals("turn_in", StringComparison.OrdinalIgnoreCase) &&
                progressDto.ObjectiveProgress.GetValueOrDefault(obj.Id, 0) < obj.Count) ?? false;

            bool allObjectivesComplete = AreAllObjectivesComplete(progressDto);
            double buttonYPos = yPos + containerHeight - 75;

            if (hasIncompleteTurnIn)
            {
                var submitBounds = ElementBounds.Fixed(magicWidth - 200, buttonYPos, 190, 30);
                var submitButton = new GuiElementTextButton(capi, Lang.Get("srguildsandkingdoms:quests-submit-items"),
                    CairoFont.WhiteSmallText().WithFontSize(12).WithWeight(Cairo.FontWeight.Bold),
                    CairoFont.WhiteSmallText().WithFontSize(12).WithWeight(Cairo.FontWeight.Bold),
                    () => { OnSubmitItemsClicked(progressDto.QuestId); return true; },
                    submitBounds, EnumButtonStyle.Normal);
                container.Add(submitButton);
            }

            if (allObjectivesComplete)
            {
                var claimBounds = ElementBounds.Fixed(magicWidth - 200, buttonYPos + 35, 190, 30);
                var claimButton = new GuiElementTextButton(capi, Lang.Get("srguildsandkingdoms:quests-claim-reward"),
                    CairoFont.WhiteSmallText().WithFontSize(12).WithWeight(Cairo.FontWeight.Bold),
                    CairoFont.WhiteSmallText().WithFontSize(12).WithWeight(Cairo.FontWeight.Bold),
                    () => { OnClaimRewardClicked(progressDto.QuestId); return true; },
                    claimBounds, EnumButtonStyle.Normal);
                container.Add(claimButton);
            }
            else
            {
                var abandonBounds = ElementBounds.Fixed(magicWidth - 200, buttonYPos + 35, 190, 30);
                var abandonButton = new GuiElementTextButton(capi, Lang.Get("srguildsandkingdoms:quests-abandon"),
                    CairoFont.WhiteSmallText().WithFontSize(12).WithWeight(Cairo.FontWeight.Bold),
                    CairoFont.WhiteSmallText().WithFontSize(12).WithWeight(Cairo.FontWeight.Bold),
                    () => { OnAbandonQuestClicked(progressDto.QuestId); return true; },
                    abandonBounds, EnumButtonStyle.Normal);
                container.Add(abandonButton);
            }

            return startY + containerHeight;
        }

        private double RenderAvailableQuestsAsElements(GuiElementContainer container, double yPos, int magicWidth, List<QuestDto> quests)
        {
            foreach (var quest in quests)
            {
                yPos = RenderAvailableQuestAsElement(container, yPos, magicWidth, quest);
                yPos += 5; // Gap between quests
            }
            return yPos;
        }

        private double RenderAvailableQuestAsElement(GuiElementContainer container, double yPos, int magicWidth, QuestDto quest)
        {
            double startY = yPos;

            bool isPeriodCompleted = completedPeriodKeys.Contains(quest.PeriodKey);
            bool isPeriodActive = (quest.RecurrenceType != "weekly") && activeQuestProgress.Any(p => p.PeriodKey == quest.PeriodKey);
            bool isPeriodLocked = (quest.RecurrenceType != "weekly") && (isPeriodCompleted || isPeriodActive);

            var descFont = isPeriodLocked
                ? CairoFont.WhiteSmallText().WithFontSize(11).WithColor([0.6, 0.6, 0.6, 1.0])
                : CairoFont.WhiteSmallText().WithFontSize(11).WithColor([0.85, 0.85, 0.85, 1.0]);

            double descWidth = magicWidth - 20;
            double descHeight = capi.Gui.Text.GetMultilineTextHeight(descFont, quest.Description, descWidth * RuntimeEnv.GUIScale) / RuntimeEnv.GUIScale;

            int objectiveCount = quest.Objectives?.Count ?? 0;
            double containerHeight = 28 + descHeight + 3 + (objectiveCount * 22) + 60 + 40;

            var insetBounds = ElementBounds.Fixed(0, yPos, magicWidth, containerHeight);
            var inset = new components.GuiElementQuestInset(capi, insetBounds, 4, 0.85f);
            container.Add(inset);

            // Quest title
            var titleFont = isPeriodLocked
                ? CairoFont.WhiteSmallText().WithColor([0.6, 0.6, 0.6, 1.0])
                : CairoFont.WhiteSmallText().WithWeight(Cairo.FontWeight.Bold);
            var titleBounds = ElementBounds.Fixed(10, yPos + 9, magicWidth - 220, 20);
            container.Add(new GuiElementStaticText(capi, quest.Title, EnumTextOrientation.Left, titleBounds, titleFont));

            // Description
            var descBounds = ElementBounds.Fixed(10, yPos + 28, descWidth, descHeight);
            container.Add(new GuiElementStaticText(capi, quest.Description, EnumTextOrientation.Left, descBounds, descFont));

            // Objectives
            double objYPos = yPos + 28 + descHeight + 9;
            if (quest.Objectives != null && quest.Objectives.Count > 0)
            {
                foreach (var objective in quest.Objectives)
                {
                    objYPos = RenderObjectiveAsElement(container, objective, 15, objYPos, 0, objective.Count, hideProgress: true);
                }
            }

            // Rewards
            double rewardsYPos = objYPos + 4;
            rewardsYPos = RenderRewardsAsElements(container, quest.Rewards, 15, rewardsYPos);

            // Expiration
            var expirationText = FormatExpirationDate(quest.ExpiresAt, quest.UsesIngameTime, quest.Rank == "D" ? null : quest.Rank);
            var expBounds = ElementBounds.Fixed(10, rewardsYPos + 5, magicWidth - 220, 20);
            container.Add(new GuiElementStaticText(capi, expirationText, EnumTextOrientation.Left,
                expBounds, CairoFont.WhiteSmallText().WithFontSize(10).WithColor([0.8, 0.8, 0.8, 1.0])));

            // Start button
            var alreadyCompletedLastWeek = quest.AlreadyCompletedLastWeek;
            double buttonYPos = yPos + containerHeight - 40;

            bool needsMonthlyBoard = (quest.RecurrenceType.Equals("monthly", StringComparison.OrdinalIgnoreCase) || quest.RecurrenceType.Equals("seasonal", StringComparison.OrdinalIgnoreCase)) && questType == "daily-weekly";
            bool needsDailyBoard = (quest.RecurrenceType.Equals("daily", StringComparison.OrdinalIgnoreCase) || quest.RecurrenceType.Equals("weekly", StringComparison.OrdinalIgnoreCase)) && questType == "monthly-seasonal";

            var buttonText = Lang.Get("srguildsandkingdoms:quests-start");
            var buttonWidth = 80.0;

            if (needsMonthlyBoard)
            {
                buttonText = Lang.Get("srguildsandkingdoms:quests-needs-monthly-board");
                buttonWidth = 165.0;
            }

            if (needsDailyBoard)
            {
                buttonText = Lang.Get("srguildsandkingdoms:quests-needs-daily-board");
                buttonWidth = 165.0;
            }

            if (alreadyCompletedLastWeek)
            {
                buttonText = Lang.Get("srguildsandkingdoms:quests-recently-completed");
                buttonWidth = 150.0;
            }

            if (isPeriodActive && quest.RecurrenceType != "weekly")
            {
                buttonText = Lang.Get("srguildsandkingdoms:quests-period-in-progress");
                buttonWidth = 120.0;
            }

            var buttonX = magicWidth - (buttonWidth + 10);

            var startBounds = ElementBounds.Fixed(buttonX, buttonYPos, buttonWidth, 30);
            var startButton = new GuiElementTextButton(capi, buttonText,
                CairoFont.WhiteSmallText().WithFontSize(12).WithWeight(Cairo.FontWeight.Bold),
                CairoFont.WhiteSmallText().WithFontSize(12).WithWeight(Cairo.FontWeight.Bold),
                () => { OnStartQuestClicked(quest.Id); return true; },
                startBounds, EnumButtonStyle.Normal);

            if (isPeriodLocked || alreadyCompletedLastWeek || needsMonthlyBoard || needsDailyBoard)
            {
                startButton.Enabled = false;
            }

            container.Add(startButton);

            return startY + containerHeight;
        }

        private double RenderObjectiveAsElement(GuiElementContainer container, QuestObjectiveDto objective, double xPos, double yPos, int currentProgress, int requiredCount, bool hideProgress = false)
        {
            const double iconSize = 15;
            double currentX = xPos;

            // Determine action text
            string actionText;
            double actionWidth;
            List<ItemStack> itemStacks = new();

            switch (objective.Type.ToLower())
            {
                case "kill":
                    actionText = "• Kill";
                    actionWidth = 30;
                    if (objective.AcceptedTargets != null)
                    {
                        foreach (var target in objective.AcceptedTargets)
                        {
                            var creatureCode = $"{target.Replace(":", ":creature-")}";
                            var stack = CreateItemStackFromCode(creatureCode, objective.Count);
                            if (stack != null) itemStacks.Add(stack);
                        }
                    }
                    break;
                case "turn_in":
                    actionText = "• Turn in";
                    actionWidth = 45;
                    if (objective.AcceptedItems != null)
                    {
                        foreach (var acceptedItem in objective.AcceptedItems)
                        {
                            var stack = CreateItemStackFromAcceptedItem(acceptedItem, objective.Count);
                            if (stack != null) itemStacks.Add(stack);
                        }
                    }
                    break;
                case "craft":
                    actionText = "• Craft";
                    actionWidth = 33;
                    if (objective.AcceptedItems != null)
                    {
                        foreach (var acceptedItem in objective.AcceptedItems)
                        {
                            var stack = CreateItemStackFromAcceptedItem(acceptedItem, objective.Count);
                            if (stack != null) itemStacks.Add(stack);
                        }
                    }
                    break;
                case "harvest":
                    actionText = "• Harvest";
                    actionWidth = 47;
                    if (objective.AcceptedItems != null)
                    {
                        foreach (var acceptedItem in objective.AcceptedItems)
                        {
                            var stack = CreateItemStackFromAcceptedItem(acceptedItem, objective.Count);
                            if (stack != null) itemStacks.Add(stack);
                        }
                    }
                    break;
                default:
                    actionText = "•";
                    actionWidth = 15;
                    break;
            }

            // Action text
            var actionFont = currentProgress >= requiredCount
                ? CairoFont.WhiteSmallText().WithFontSize(10).WithColor([0.89, 0.72, 0.04, 1.0])
                : CairoFont.WhiteSmallText().WithFontSize(10).WithColor([0.75, 0.75, 0.75, 1.0]);
            var actionBounds = ElementBounds.Fixed(currentX, yPos + 3, actionWidth, 16);
            container.Add(new GuiElementStaticText(capi, actionText, EnumTextOrientation.Left,
                actionBounds, actionFont));
            currentX += actionWidth;

            // Item icon
            if (itemStacks.Count > 0)
            {
                var itemBounds = ElementBounds.Fixed(currentX, yPos + 3, iconSize, iconSize);
                if (itemStacks.Count == 1)
                {
                    var element = new components.GuiElementItemstackDisplay(capi, itemStacks[0], itemBounds)
                    {
                        ShowStackSize = false
                    };
                    container.Add(element);
                }
                else
                {
                    var cyclingElement = new components.GuiElementCyclingItemDisplay(capi, itemStacks, itemBounds)
                    {
                        ShowStackSize = false
                    };
                    container.Add(cyclingElement);
                }
                currentX += iconSize + 5;
            }

            // Progress text
            var progressText = hideProgress ? $"({requiredCount})" : $"({currentProgress}/{requiredCount})";
            var progressFont = currentProgress >= requiredCount
                ? CairoFont.WhiteSmallText().WithFontSize(10).WithColor([0.89, 0.72, 0.04, 1.0])
                : CairoFont.WhiteSmallText().WithFontSize(10).WithColor([0.6, 0.6, 0.6, 1.0]);
            var progressBounds = ElementBounds.Fixed(currentX, yPos + 3, 60, 16);
            container.Add(new GuiElementStaticText(capi, progressText, EnumTextOrientation.Left,
                progressBounds, progressFont));

            return yPos + 22;
        }

        private double RenderRewardsAsElements(GuiElementContainer container, List<QuestRewardDto> rewards, double xPos, double yPos)
        {
            if (rewards == null || rewards.Count == 0) return yPos;

            double currentX = xPos - 5;
            const double iconSize = 20;
            const double iconSpacing = 33;

            // Rewards label
            var labelFont = CairoFont.WhiteSmallishText().WithFontSize(13).WithColor([0.75, 0.75, 0.75, 1.0]);
            var labelText = "Rewards";
            var labelExtents = labelFont.GetTextExtents(labelText);
            var labelBounds = ElementBounds.Fixed(currentX, yPos + 5, labelExtents.Width, 20);
            container.Add(new GuiElementStaticText(capi, labelText, EnumTextOrientation.Left,
                labelBounds, labelFont));
            yPos += 30;
            currentX += 5;

            // Reward icons
            foreach (var reward in rewards)
            {
                var itemstack = CreateItemStackFromReward(reward);
                if (itemstack == null) continue;

                var itemBounds = ElementBounds.Fixed(currentX, yPos - 2, iconSize, iconSize);
                var element = new components.GuiElementItemstackDisplay(capi, itemstack, itemBounds)
                {
                    ShowStackSize = true
                };
                container.Add(element);
                currentX += iconSpacing;
            }

            return yPos + 30;
        }

        private static int GetRankValue(string rank)
        {
            return rank.ToUpperInvariant() switch
            {
                "D" => 0,
                "C" => 1,
                "B" => 2,
                "A" => 3,
                "S" => 4,
                _ => 0
            };
        }

        private bool IsPeriodCompleted(string periodType)
        {
            var hasActiveQuests = activeQuestProgress.Any(p =>
                p.RecurrenceType.Equals(periodType, StringComparison.OrdinalIgnoreCase)
            );

            if (hasActiveQuests)
                return false;

            var questsForPeriod = availableQuests.Where(q =>
                q.RecurrenceType.Equals(periodType, StringComparison.OrdinalIgnoreCase)
            ).ToList();

            if (questsForPeriod.Count == 0)
                return true;

            return questsForPeriod.All(q => completedPeriodKeys.Contains(q.PeriodKey));
        }

        private void OnPeriodTabClicked(int tabIndex)
        {
            if (composer == null) return;

            if (selectedPeriodTab == tabIndex)
            {
                // Don't allow them to un-toggle directly, must select another tab
                composer.GetToggleButton($"quest-tab-{tabIndex}").SetValue(true);
                return;
            }

            selectedPeriodTab = tabIndex;

            // Loop through all tabs and update their toggled state
            for (int i = 0; i < 4; i++)
            {
                var button = composer.GetToggleButton($"quest-tab-{i}");
                button?.SetValue(i == selectedPeriodTab);
            }

            // Trigger UI refresh to show quests for the selected period
            onNeedsRefresh?.Invoke();
        }

        private List<QuestDto> GetQuestsForCurrentTab()
        {
            var periodType = GetCurrentPeriodTypeName();
            return [.. availableQuests.Where(q =>
                q.RecurrenceType.Equals(periodType, StringComparison.OrdinalIgnoreCase)
            )];
        }

        private List<PlayerQuestProgressDto> GetActiveQuestsForCurrentTab()
        {
            var periodType = GetCurrentPeriodTypeName();
            return [.. activeQuestProgress.Where(p =>
                p.RecurrenceType.Equals(periodType, StringComparison.OrdinalIgnoreCase)
            )];
        }

        private string GetCurrentPeriodTypeName()
        {
            return selectedPeriodTab switch
            {
                0 => "weekly",
                1 => "monthly",
                2 => "seasonal",
                _ => "weekly"
            };
        }

        /// <summary>
        /// Creates an ItemStack from an item/block code with specified quantity
        /// </summary>
        private ItemStack? CreateItemStackFromCode(string code, int quantity)
        {
            try
            {
                var item = capi.World.GetItem(new AssetLocation(code));
                if (item != null)
                {
                    return new ItemStack(item, quantity);
                }

                var block = capi.World.GetBlock(new AssetLocation(code));
                if (block != null)
                {
                    return new ItemStack(block, quantity);
                }

                return null;
            }
            catch
            {
                return null;
            }
        }

        private ItemStack? CreateItemStackFromAcceptedItem(QuestAcceptedItemDto acceptedItem, int quantity)
        {
            try
            {
                var item = capi.World.GetItem(new AssetLocation(acceptedItem.Code));
                if (item == null)
                {
                    var block = capi.World.GetBlock(new AssetLocation(acceptedItem.Code));
                    if (block == null) return null;

                    var blockStack = new ItemStack(block, quantity);

                    if (!string.IsNullOrEmpty(acceptedItem.Nbt))
                    {
                        var nbtBytes = System.Convert.FromBase64String(acceptedItem.Nbt);
                        using var ms = new System.IO.MemoryStream(nbtBytes);
                        using var reader = new System.IO.BinaryReader(ms);
                        blockStack.Attributes = new Vintagestory.API.Datastructures.TreeAttribute();
                        blockStack.Attributes.FromBytes(reader);
                    }

                    return blockStack;
                }

                var itemStack = new ItemStack(item, quantity);

                if (!string.IsNullOrEmpty(acceptedItem.Nbt))
                {
                    var nbtBytes = System.Convert.FromBase64String(acceptedItem.Nbt);
                    using var ms = new System.IO.MemoryStream(nbtBytes);
                    using var reader = new System.IO.BinaryReader(ms);
                    itemStack.Attributes = new Vintagestory.API.Datastructures.TreeAttribute();
                    itemStack.Attributes.FromBytes(reader);
                }

                return itemStack;
            }
            catch
            {
                return null;
            }
        }

        private ItemStack? CreateItemStackFromReward(QuestRewardDto reward)
        {
            try
            {
                if (reward.Code == "game:grspoints")
                {
                    // For GRS points, render as a parchment with "GRS Points" as the title
                    var parchmentItem = capi.World.GetItem(new AssetLocation("game:paper-parchment"));
                    if (parchmentItem == null) return null;

                    var parchmentStack = new ItemStack(parchmentItem, reward.Amount);
                    parchmentStack.Attributes = new Vintagestory.API.Datastructures.TreeAttribute();
                    parchmentStack.Attributes.SetString("title", $"GRS Points");
                    parchmentStack.Attributes.SetString("text", "");
                    parchmentStack.Attributes.SetString("signedby", $"The Shadow Realm");

                    return parchmentStack;
                }

                var item = capi.World.GetItem(new AssetLocation(reward.Code));
                if (item == null)
                {
                    var block = capi.World.GetBlock(new AssetLocation(reward.Code));
                    if (block == null) return null;

                    var blockStack = new ItemStack(block, reward.Amount);

                    if (!string.IsNullOrEmpty(reward.Nbt))
                    {
                        var nbtBytes = System.Convert.FromBase64String(reward.Nbt);
                        using var ms = new System.IO.MemoryStream(nbtBytes);
                        using var reader = new System.IO.BinaryReader(ms);
                        blockStack.Attributes = new Vintagestory.API.Datastructures.TreeAttribute();
                        blockStack.Attributes.FromBytes(reader);
                    }

                    return blockStack;
                }

                var itemStack = new ItemStack(item, reward.Amount);

                if (!string.IsNullOrEmpty(reward.Nbt))
                {
                    var nbtBytes = System.Convert.FromBase64String(reward.Nbt);
                    using var ms = new System.IO.MemoryStream(nbtBytes);
                    using var reader = new System.IO.BinaryReader(ms);
                    itemStack.Attributes = new Vintagestory.API.Datastructures.TreeAttribute();
                    itemStack.Attributes.FromBytes(reader);
                }

                return itemStack;
            }
            catch (Exception ex)
            {
                capi.Logger.Error($"[QuestDialogContent] Failed to create itemstack for reward {reward.Code}: {ex.Message}");
                return null;
            }
        }

        private string FormatExpirationDate(string expiresAt, bool usesIngameTime, string? questRank)
        {
            if (string.IsNullOrEmpty(expiresAt))
                return "Expires: Unknown";

            try
            {
                // Parse the date string (format: YYYY-MM-DD)
                var parts = expiresAt.Split('-');
                if (parts.Length != 3) return $"Expires: {expiresAt}";

                int year = int.Parse(parts[0]);
                int month = int.Parse(parts[1]);
                int day = int.Parse(parts[2]);

                // If year is 0 (IGT dates), use year 1 for DateTime parsing
                int dateTimeYear = year == 0 ? 1 : year;

                var date = new DateTime(dateTimeYear, month, day);
                var monthName = date.ToString("MMMM");

                if (usesIngameTime)
                {
                    // For IGT, display the actual VS year (DateTime year - 1 for year 0000 dates)
                    var retStr = $"Ends on {monthName} {day}, Year {year}";
                    if (questRank != null)
                    {
                        retStr += $" • Rank {questRank}";
                    }

                    return retStr;
                }
                else
                {
                    var retStr = $"Ends on {monthName} {day}, {year}";
                    if (questRank != null)
                    {
                        retStr += $" • Rank {questRank}";
                    }

                    return retStr;
                }
            }
            catch
            {
                return $"Expires: {expiresAt}";
            }
        }

        private string GetNextResetTimeDisplay(string expiresAt)
        {
            try
            {
                if (!DateTime.TryParseExact(expiresAt, "yyyy-MM-dd",
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None, out var expirationDate))
                {
                    return "";
                }

                var nextResetDateEst = expirationDate.AddDays(1);

                TimeZoneInfo easternZone;
                try
                {
                    easternZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
                }
                catch (TimeZoneNotFoundException)
                {
                    // Fallback for Linux/macOS
                    easternZone = TimeZoneInfo.FindSystemTimeZoneById("America/New_York");
                }

                var resetTimeEst = new DateTime(nextResetDateEst.Year, nextResetDateEst.Month, nextResetDateEst.Day, 0, 0, 0, DateTimeKind.Unspecified);

                var resetTimeUtc = TimeZoneInfo.ConvertTimeToUtc(resetTimeEst, easternZone);
                var resetTimeLocal = TimeZoneInfo.ConvertTimeFromUtc(resetTimeUtc, TimeZoneInfo.Local);

                var tzAbbreviation = GetTimezoneAbbreviation(TimeZoneInfo.Local, resetTimeLocal);

                return Lang.Get("srguildsandkingdoms:quests-next-reset", $"{resetTimeLocal:MMMM d, yyyy h:mm tt} {tzAbbreviation}");
            }
            catch
            {
                return "";
            }
        }

        private static string GetTimezoneAbbreviation(TimeZoneInfo timeZone, DateTime dateTime)
        {
            bool isDst = timeZone.IsDaylightSavingTime(dateTime);
            string tzName = isDst ? timeZone.DaylightName : timeZone.StandardName;

            var words = tzName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (words.Length > 1)
            {
                return string.Concat(words.Select(w => char.ToUpper(w[0])));
            }

            var offset = timeZone.GetUtcOffset(dateTime);
            var sign = offset >= TimeSpan.Zero ? "+" : "";
            return $"UTC{sign}{offset.Hours:0}";
        }

        private void FetchQuestData()
        {
            var questNetHandler = modSystem.QuestNetworkHandler;
            if (questNetHandler == null)
            {
                capi.Logger.Warning("[QuestDialogContent] QuestNetworkHandler is null!");
                return;
            }

            var playerUid = capi.World.Player?.PlayerUID;
            if (string.IsNullOrEmpty(playerUid))
            {
                capi.Logger.Warning("[QuestDialogContent] Player UID is null or empty!");
                return;
            }

            capi.Logger.Notification($"[QuestDialogContent] Fetching quest data for player {playerUid}");
            isLoading = true;
            questListReceived = false;
            progressReceived = false;

            // Request both quest list and progress
            questNetHandler.RequestQuestList(playerUid);
            questNetHandler.RequestQuestProgress(playerUid);
        }

        /// <summary>
        /// Checks if all objectives in a quest are complete
        /// </summary>
        private bool AreAllObjectivesComplete(PlayerQuestProgressDto progressDto)
        {
            if (progressDto.Objectives == null || progressDto.Objectives.Count == 0)
                return false;

            foreach (var objective in progressDto.Objectives)
            {
                int currentProgress = progressDto.ObjectiveProgress.GetValueOrDefault(objective.Id, 0);
                if (currentProgress < objective.Count)
                    return false;
            }

            return true;
        }

        private void OnStartQuestClicked(int questId)
        {
            var questNetHandler = modSystem.QuestNetworkHandler;
            if (questNetHandler == null) return;

            var playerUid = capi.World.Player?.PlayerUID;
            if (string.IsNullOrEmpty(playerUid)) return;

            questNetHandler.RequestStartQuest(playerUid, questId);
        }

        private void OnAbandonQuestClicked(int questId)
        {
            var questNetHandler = modSystem.QuestNetworkHandler;
            if (questNetHandler == null) return;

            var playerUid = capi.World.Player?.PlayerUID;
            if (string.IsNullOrEmpty(playerUid)) return;

            var progress = activeQuestProgress.FirstOrDefault(p => p.QuestId == questId);
            var questTitle = progress?.QuestTitle ?? $"Quest #{questId}";

            var confirmDialog = new ConfirmAbandonDialog(
                capi,
                Lang.Get("srguildsandkingdoms:quests-abandon-confirm-message", questTitle),
                () => questNetHandler.RequestAbandonQuest(playerUid, questId)
            );
            confirmDialog.TryOpen();
        }

        private void OnSubmitItemsClicked(int questId)
        {
            var questNetHandler = modSystem.QuestNetworkHandler;
            if (questNetHandler == null) return;

            var playerUid = capi.World.Player?.PlayerUID;
            if (string.IsNullOrEmpty(playerUid)) return;

            questNetHandler.RequestSubmitPreview(playerUid, questId);
        }

        private void OnClaimRewardClicked(int questId)
        {
            var questNetHandler = modSystem.QuestNetworkHandler;
            if (questNetHandler == null) return;

            var playerUid = capi.World.Player?.PlayerUID;
            if (string.IsNullOrEmpty(playerUid)) return;

            questNetHandler.RequestCompleteQuest(playerUid, questId);
        }

        #region Network Response Callbacks

        private void OnQuestListReceived(List<QuestDto> quests)
        {
            capi.Logger.Notification($"[QuestDialogContent] Received {quests.Count} quests from server");
            availableQuests = quests;
            questListReceived = true;

            // Only mark as loaded and refresh if we have both pieces of data
            if (questListReceived && progressReceived)
            {
                isLoading = false;
                dataLoaded = true;
                onNeedsRefresh?.Invoke();
            }
        }

        private void OnProgressReceived(List<PlayerQuestProgressDto> progress, List<string> completedKeys, List<CompletedQuestInfo> completedQuests)
        {
            capi.Logger.Notification($"[QuestDialogContent] Received {progress.Count} active quests, {completedKeys.Count} completed period keys, {completedQuests.Count} completed quests");
            activeQuestProgress = progress;
            completedPeriodKeys = completedKeys;
            completedQuestsByPeriod = completedQuests
                .Select(cq => (cq.QuestId, cq.PeriodKey))
                .ToHashSet();
            progressReceived = true;

            // Only mark as loaded and refresh if we have both pieces of data
            if (questListReceived && progressReceived)
            {
                isLoading = false;
                dataLoaded = true;
                onNeedsRefresh?.Invoke();
            }
        }

        private void OnStartResponse(QuestStartResponsePacket response)
        {
            if (response.Success)
            {
                capi.ShowChatMessage(Lang.Get("srguildsandkingdoms:quests-started-success"));
                // Refresh quest data to update UI
                FetchQuestData();
            }
            else
            {
                capi.ShowChatMessage(Lang.Get("srguildsandkingdoms:quests-started-failed", response.Message));
            }
        }

        private void OnAbandonResponse(QuestAbandonResponsePacket response)
        {
            if (response.Success)
            {
                capi.ShowChatMessage(Lang.Get("srguildsandkingdoms:quests-abandoned"));
                FetchQuestData();
            }
            else
            {
                capi.ShowChatMessage(Lang.Get("srguildsandkingdoms:quests-abandon-failed", response.Message));
            }
        }

        private void OnSubmitPreviewReceived(QuestSubmitPreviewResponsePacket response)
        {
            if (!response.Success)
            {
                capi.ShowChatMessage(Lang.Get("srguildsandkingdoms:quests-submit-failed", response.Message));
                return;
            }

            // Build the display lines from preview items
            var lines = new List<string>();
            foreach (var item in response.Items)
            {
                lines.Add($"{item.DisplayName} x{item.Quantity}");
            }

            var questNetHandler = modSystem.QuestNetworkHandler;
            if (questNetHandler == null) return;

            var playerUid = capi.World.Player?.PlayerUID;
            if (string.IsNullOrEmpty(playerUid)) return;

            var confirmDialog = new ConfirmSubmitDialog(
                capi,
                Lang.Get("srguildsandkingdoms:quests-submit-confirm-message"),
                lines,
                () => questNetHandler.ConfirmSubmit(playerUid, response.QuestId, response.Items)
            );
            confirmDialog.TryOpen();
        }

        private void OnSubmitConfirmReceived(QuestSubmitConfirmResponsePacket response)
        {
            if (response.Success)
            {
                capi.ShowChatMessage(Lang.Get("srguildsandkingdoms:quests-submit-success"));
                FetchQuestData();
            }
            else
            {
                capi.ShowChatMessage(Lang.Get("srguildsandkingdoms:quests-submit-failed", response.Message));
            }
        }

        private void OnCompleteReceived(QuestCompleteResponsePacket response)
        {
            if (response.Success)
            {
                capi.ShowChatMessage(Lang.Get("srguildsandkingdoms:quests-complete-success"));

                // Immediately update local caches so the UI reflects the completion
                if (!string.IsNullOrEmpty(response.PeriodKey) && !completedPeriodKeys.Contains(response.PeriodKey))
                {
                    completedPeriodKeys.Add(response.PeriodKey);
                }

                // Remove the completed quest from the active list
                activeQuestProgress.RemoveAll(p => p.QuestId == response.QuestId);

                FetchQuestData();
            }
            else
            {
                capi.ShowChatMessage(Lang.Get("srguildsandkingdoms:quests-complete-failed", response.Message));
            }
        }

        #endregion
    }

    /// <summary>
    /// Confirmation dialog for abandoning a quest
    /// </summary>
    public class ConfirmAbandonDialog : GuiDialog
    {
        private readonly string message;
        private readonly Action onConfirm;

        public override string? ToggleKeyCombinationCode => null;

        public ConfirmAbandonDialog(ICoreClientAPI capi, string message, Action onConfirm) : base(capi)
        {
            this.message = message;
            this.onConfirm = onConfirm;
        }

        public override void OnGuiOpened()
        {
            base.OnGuiOpened();
            SetupDialog();
        }

        public override double DrawOrder => 1;

        private void SetupDialog()
        {
            var dialogBounds = ElementStdBounds.AutosizedMainDialog.WithAlignment(EnumDialogArea.CenterMiddle);
            var bgBounds = ElementBounds.Fill.WithFixedPadding(GuiStyle.ElementToDialogPadding);
            bgBounds.BothSizing = ElementSizing.FitToChildren;
            bgBounds.WithChildren(
                ElementBounds.Fixed(0, 0, 320, 120)
            );

            SingleComposer = capi.Gui.CreateCompo("guild-quest-abandon-confirm", dialogBounds)
                .AddShadedDialogBG(bgBounds)
                .AddDialogTitleBar(Lang.Get("srguildsandkingdoms:quests-abandon-confirm-title"), OnTitleBarClose)
                .BeginChildElements(bgBounds);

            var textBounds = ElementBounds.Fixed(10, 30, 320, 120);
            SingleComposer.AddStaticText(message, CairoFont.WhiteDetailText().WithOrientation(EnumTextOrientation.Center), textBounds);

            var cancelBounds = ElementBounds.Fixed(10, 150, 150, 30);
            var confirmBounds = ElementBounds.Fixed(170, 150, 150, 30);

            SingleComposer.AddSmallButton(Lang.Get("srguildsandkingdoms:quests-abandon-confirm-yes"), OnConfirmClick, confirmBounds);
            SingleComposer.AddSmallButton(Lang.Get("srguildsandkingdoms:quests-abandon-confirm-no"), OnCancelClick, cancelBounds);

            SingleComposer.EndChildElements().Compose();
        }

        private void OnTitleBarClose()
        {
            TryClose();
        }

        private bool OnConfirmClick()
        {
            onConfirm?.Invoke();
            TryClose();
            return true;
        }

        private bool OnCancelClick()
        {
            TryClose();
            return true;
        }
    }

    /// <summary>
    /// Confirmation dialog for submitting quest items
    /// </summary>
    public class ConfirmSubmitDialog : GuiDialog
    {
        private readonly string message;
        private readonly List<string> itemLines;
        private readonly Action onConfirm;

        public override string? ToggleKeyCombinationCode => null;
        public override double DrawOrder => 2;

        public ConfirmSubmitDialog(ICoreClientAPI capi, string message, List<string> itemLines, Action onConfirm) : base(capi)
        {
            this.message = message;
            this.itemLines = itemLines;
            this.onConfirm = onConfirm;
        }

        public override void OnGuiOpened()
        {
            base.OnGuiOpened();
            SetupDialog();
        }

        private void SetupDialog()
        {
            // Calculate height based on number of item lines
            double itemsHeight = itemLines.Count * 22;
            double contentHeight = 50 + itemsHeight + 20;
            double dialogHeight = contentHeight + 50;

            var dialogBounds = ElementStdBounds.AutosizedMainDialog.WithAlignment(EnumDialogArea.CenterMiddle);
            var bgBounds = ElementBounds.Fill.WithFixedPadding(GuiStyle.ElementToDialogPadding);
            bgBounds.BothSizing = ElementSizing.FitToChildren;
            bgBounds.WithChildren(
                ElementBounds.Fixed(0, 0, 320, dialogHeight)
            );

            SingleComposer = capi.Gui.CreateCompo("guild-quest-submit-confirm", dialogBounds)
                .AddShadedDialogBG(bgBounds)
                .AddDialogTitleBar(Lang.Get("srguildsandkingdoms:quests-submit-confirm-title"), OnTitleBarClose)
                .BeginChildElements(bgBounds);

            // Message text
            var textBounds = ElementBounds.Fixed(10, 30, 320, 25);
            SingleComposer.AddStaticText(message, CairoFont.WhiteDetailText(), textBounds);

            // Item lines
            double yPos = 80;
            foreach (var line in itemLines)
            {
                var lineBounds = ElementBounds.Fixed(20, yPos, 300, 20);
                SingleComposer.AddStaticText(
                    line,
                    CairoFont.WhiteSmallText().WithColor([0.89, 0.72, 0.04, 1.0]),
                    lineBounds
                );
                yPos += 22;
            }

            // Buttons
            double buttonY = yPos + 20;
            var cancelBounds = ElementBounds.Fixed(10, buttonY, 150, 30);
            var confirmBounds = ElementBounds.Fixed(170, buttonY, 150, 30);

            SingleComposer.AddSmallButton(Lang.Get("srguildsandkingdoms:quests-submit-confirm-yes"), OnConfirmClick, confirmBounds);
            SingleComposer.AddSmallButton(Lang.Get("srguildsandkingdoms:quests-submit-confirm-no"), OnCancelClick, cancelBounds);

            SingleComposer.EndChildElements().Compose();
        }

        private void OnTitleBarClose()
        {
            TryClose();
        }

        private bool OnConfirmClick()
        {
            onConfirm?.Invoke();
            TryClose();
            return true;
        }

        private bool OnCancelClick()
        {
            TryClose();
            return true;
        }
    }
}
