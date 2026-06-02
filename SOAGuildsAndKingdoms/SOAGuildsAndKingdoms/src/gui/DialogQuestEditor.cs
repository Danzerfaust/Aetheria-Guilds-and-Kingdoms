using SOAGuildsAndKingdoms.src.gui.components;
using SOAGuildsAndKingdoms.src.network;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Config;

namespace SOAGuildsAndKingdoms.src.gui
{
    /// <summary>
    /// Dialog for creating or editing a quest
    /// </summary>
    public class DialogQuestEditor : GuiDialog
    {
        private readonly SOAGuildsAndKingdomsModSystem modSystem;
        private readonly QuestDto? existingQuest;
        private readonly CurrencyDefinitionDto? tailsDefinition;
        private readonly CurrencyDefinitionDto? crownsDefinition;

        private static readonly Random rnd = new();

        // Server timezone information
        private readonly long serverLocalTime;
        private readonly double serverTimezoneOffset;

        private const double DIALOG_WIDTH = 950;
        private const string GRS_POINTS_CODE = "game:grspoints";
        private const int QUEST_OBJECTIVE_LIMIT = 5;
        private const int QUEST_REWARD_LIMIT = 5;

        // Persisted form state
        private string formTitle = "";
        private string formDescription = "";
        private string formType = "weekly";
        private bool formIgt = false;
        private bool formRepeating = false;
        private string formRank = "D";
        private string formStartYear = DateTime.Now.Year.ToString();
        private string formStartMonth = DateTime.Now.Month.ToString("D2");
        private string formStartDay = DateTime.Now.Day.ToString("D2");
        private string formEndYear = DateTime.Now.Year.ToString();
        private string formEndMonth = DateTime.Now.Month.ToString("D2");
        private string formEndDay = DateTime.Now.Day.ToString("D2");
        private readonly List<QuestObjectiveDto> formObjectives = [
            new QuestObjectiveDto{
                Id = rnd.Next(1, 10000000),
                AcceptedItems = [],
                AcceptedTargets = [],
                Count = 5,
                Type = "turn_in"
            }
        ];
        private List<QuestRewardDtoWithNullableCode> formRewards = [
            new QuestRewardDtoWithNullableCode{
                Amount = 5,
            }
        ];

        private InventoryGeneric? objectiveInventory;
        private InventoryGeneric? rewardsInventory;

        public event Action? OnDialogClosed;
        public event Action? OnQuestSaved;

        public override string ToggleKeyCombinationCode => "questeditor";

        public class QuestRewardDtoWithNullableCode
        {
            public string? Code { get; set; }
            public string? Nbt { get; set; }
            public int Amount { get; set; }
        }

        /// <summary>
        /// Opens the editor for creating a new quest or editing an existing one.
        /// </summary>
        /// <param name="capi">Client API</param>
        /// <param name="modSystem">Mod system instance</param>
        /// <param name="quest">Existing quest to edit (null for new quest)</param>
        /// <param name="tailsDefinition">Currency definition for Tails (lower value)</param>
        /// <param name="crownsDefinition">Currency definition for Crowns (higher value)</param>
        /// <param name="serverLocalTime">Server's current local time (unix timestamp)</param>
        /// <param name="serverTimezoneOffset">Server's timezone offset from UTC in hours</param>
        public DialogQuestEditor(ICoreClientAPI capi, SOAGuildsAndKingdomsModSystem modSystem, QuestDto? quest = null, CurrencyDefinitionDto? tailsDefinition = null, CurrencyDefinitionDto? crownsDefinition = null, long serverLocalTime = 0, double serverTimezoneOffset = 0)
            : base(capi)
        {
            this.modSystem = modSystem;
            this.existingQuest = quest;
            this.tailsDefinition = tailsDefinition;
            this.crownsDefinition = crownsDefinition;
            this.serverLocalTime = serverLocalTime;
            this.serverTimezoneOffset = serverTimezoneOffset;

            // Initialize default dates from server time if available, otherwise use client time
            var defaultDate = serverLocalTime > 0
                ? DateTimeOffset.FromUnixTimeSeconds(serverLocalTime).ToOffset(TimeSpan.FromHours(serverTimezoneOffset)).DateTime
                : DateTime.Now;

            formStartYear = defaultDate.Year.ToString();
            formStartMonth = defaultDate.Month.ToString("D2");
            formStartDay = defaultDate.Day.ToString("D2");
            formEndYear = defaultDate.Year.ToString();
            formEndMonth = defaultDate.Month.ToString("D2");
            formEndDay = defaultDate.Day.ToString("D2");

            objectiveInventory = new InventoryGeneric(1000, "soaguildsandkingdoms:questeditor-objectives", capi, (id, inv) =>
            {
                return new ItemSlotDisplayOnly(inv) { MaxSlotStackSize = 1 };
            });
            objectiveInventory.SlotModified += OnInventorySlotModified;

            rewardsInventory = new InventoryGeneric(10, "soaguildsandkingdoms:questeditor-rewards", capi, (id, inv) =>
            {
                return new ItemSlotDisplayOnly(inv) { MaxSlotStackSize = 1 };
            });
            rewardsInventory.SlotModified += OnRewardsInventorySlotModified;

            if (existingQuest != null)
            {
                formTitle = existingQuest.Title ?? "";
                formDescription = existingQuest.Description ?? "";
                formType = existingQuest.RecurrenceType ?? "weekly";
                formIgt = existingQuest.UsesIngameTime;
                formRepeating = existingQuest.Repeat;
                formRank = existingQuest.Rank;

                // Parse dates (assuming format "yyyy-MM-dd")
                // If using IGT and year is 0000, we need to bump it to 0001 for DateTime parsing
                string startsAt = existingQuest.StartsAt;
                if (formIgt && startsAt.StartsWith("0000"))
                {
                    startsAt = "0001" + startsAt[4..]; // Replace year 0000 with 0001
                }

                if (DateTime.TryParseExact(startsAt, "yyyy-MM-dd",
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None, out var startDate))
                {
                    formStartYear = startDate.Year.ToString(formIgt ? "D4" : "");
                    formStartMonth = startDate.Month.ToString("D2");
                    formStartDay = startDate.Day.ToString("D2");
                }

                string expiresAt = existingQuest.ExpiresAt;
                if (formIgt && expiresAt.StartsWith("0000"))
                {
                    expiresAt = "0001" + expiresAt[4..]; // Replace year 0000 with 0001
                }

                if (DateTime.TryParseExact(expiresAt, "yyyy-MM-dd",
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None, out var endDate))
                {
                    formEndYear = endDate.Year.ToString(formIgt ? "D4" : "");
                    formEndMonth = endDate.Month.ToString("D2");
                    formEndDay = endDate.Day.ToString("D2");
                }

                // Populate objectives from existing quest
                if (existingQuest.Objectives != null)
                {
                    formObjectives = existingQuest.Objectives.Select(o => new QuestObjectiveDto
                    {
                        Id = o.Id,
                        Type = o.Type,
                        Count = o.Count,
                        AcceptedItems = new List<QuestAcceptedItemDto>(o.AcceptedItems ?? []),
                        AcceptedTargets = new List<string>(o.AcceptedTargets ?? [])
                    }).ToList();
                    // Note: Inventory slots will be populated by SetupDialog()
                }
            }

            // Populate rewards from existing quest
            if (existingQuest?.Rewards.Count > 0)
            {
                formRewards = existingQuest.Rewards.Select(r => new QuestRewardDtoWithNullableCode
                {
                    Code = r.Code,
                    Amount = r.Amount,
                    Nbt = r.Nbt
                }).ToList();
            }

            SetupDialog();
        }

        private void OnInventorySlotModified(int obj)
        {
            // Save current form state to preserve all field values
            SaveFormState();

            // Re-render - inventory will be repopulated from formObjectives
            SetupDialog();
        }

        private void OnRewardsInventorySlotModified(int rwd)
        {
            // Save current form state to preserve all field values
            SaveFormState();

            // Re-render - inventory will be repopulated from formRewards
            SetupDialog();
        }

        private void SetupDialog()
        {
            bool isEditMode = existingQuest != null;
            string title = isEditMode ? $"Edit Quest" : "New Quest";

            ElementBounds dialogBounds = ElementStdBounds.AutosizedMainDialog.WithAlignment(EnumDialogArea.CenterMiddle);
            ElementBounds bgBounds = ElementBounds.Fill.WithFixedPadding(GuiStyle.ElementToDialogPadding);
            bgBounds.BothSizing = ElementSizing.FitToChildren;

            var composer = capi.Gui.CreateCompo("questeditor", dialogBounds)
                .AddShadedDialogBG(bgBounds)
                .AddDialogTitleBar(title, OnTitleBarCloseClicked)
                .BeginChildElements(bgBounds);

            double yPos = 30;

            // Get actual content height from rendering FIRST
            double actualContentHeight = AddQuestEditorFields(composer, yPos);

            // Now create bounds with the actual height
            double contentHeight = actualContentHeight + yPos + 20;
            //var clippingBounds = ElementBounds.Fixed(0, yPos, DIALOG_WIDTH, contentHeight);
            //var insetBounds = clippingBounds.FlatCopy().FixedGrow(6).WithFixedOffset(-3, -3);

            // Add inset AFTER we know the correct height
            //composer.AddInset(insetBounds, 3);

            // Bottom buttons
            composer.AddSmallButton("Cancel", OnCancelClicked, ElementBounds.Fixed(0, actualContentHeight + 50, 80, 30));
            composer.AddSmallButton("Save", () => OnSaveClicked(false), ElementBounds.Fixed(DIALOG_WIDTH - 80, actualContentHeight + 50, 80, 30));

            if (existingQuest != null)
            {
                composer.AddSmallButton("Save as New", () => OnSaveClicked(true), ElementBounds.Fixed(DIALOG_WIDTH - 240, actualContentHeight + 50, 140, 30));
                composer.AddSmallButton("Delete", OnDeleteClicked, ElementBounds.Fixed(100, actualContentHeight + 50, 80, 30));
            }

            composer.EndChildElements();
            SingleComposer = composer.Compose();

            composer.GetDropDown("endYear").Enabled = formType == "seasonal";
            composer.GetDropDown("endMonth").Enabled = formType == "seasonal";
            composer.GetDropDown("endDay").Enabled = formType == "seasonal";
            composer.GetDropDown("startDay").Enabled = formType != "monthly";
            composer.GetTextArea("description").SetValue(formDescription);

            // Set objective target dropdowns
            for (int i = 0; i < formObjectives.Count; i++)
            {
                composer.GetDropDown($"objective_targetSelect_{i}")?.SetSelectedValue(formObjectives[i].AcceptedTargets?.ToArray() ?? []);
            }

            SingleComposer.UnfocusOwnElements();
        }

        private void SaveFormState()
        {
            if (SingleComposer == null) return;

            formTitle = SingleComposer.GetTextInput("title")?.GetText() ?? formTitle;
            formDescription = SingleComposer.GetTextArea("description")?.GetText() ?? formDescription;
            formType = SingleComposer.GetDropDown("type")?.SelectedValue ?? formType;
            formIgt = SingleComposer.GetSwitch("inGameTime")?.On ?? formIgt;
            formRepeating = SingleComposer.GetSwitch("repeating")?.On ?? formRepeating;
            formRank = SingleComposer.GetDropDown("rank")?.SelectedValue ?? formRank;
            formStartYear = SingleComposer.GetDropDown("startYear")?.SelectedValue ?? formStartYear;
            formStartMonth = SingleComposer.GetDropDown("startMonth")?.SelectedValue ?? formStartMonth;
            formStartDay = SingleComposer.GetDropDown("startDay")?.SelectedValue ?? formStartDay;
            formEndYear = SingleComposer.GetDropDown("endYear")?.SelectedValue ?? formEndYear;
            formEndMonth = SingleComposer.GetDropDown("endMonth")?.SelectedValue ?? formEndMonth;
            formEndDay = SingleComposer.GetDropDown("endDay")?.SelectedValue ?? formEndDay;

            for (int i = 0; i < formObjectives.Count; i++)
            {
                var typeDropdown = SingleComposer.GetDropDown($"objective_type_{i}");
                if (typeDropdown != null)
                {
                    formObjectives[i].Type = typeDropdown.SelectedValue;

                    if (formObjectives[i].Type == "turn_in" && objectiveInventory != null)
                    {
                        formObjectives[i].AcceptedItems = [];

                        // Check all 100 possible slots for this objective
                        for (int itemIdx = 0; itemIdx < 100; itemIdx++)
                        {
                            int slotId = i * 100 + itemIdx;
                            var slot = objectiveInventory[slotId];

                            ItemStack? setStack = slot.Itemstack?.Clone();
                            if (setStack != null)
                            {
                                var acceptedItem = new QuestAcceptedItemDto
                                {
                                    Code = setStack.Collectible.Code.ToString()
                                };

                                // Serialize NBT data if present
                                if (setStack.Attributes != null)
                                {
                                    // Remove bad NBT
                                    foreach (var ignoreAttr in QuestNetworkHandler.NbtAttributesToIgnore)
                                    {
                                        if (setStack.Attributes.HasAttribute(ignoreAttr) == true)
                                        {
                                            setStack.Attributes.RemoveAttribute(ignoreAttr);
                                        }
                                    }


                                    if (setStack.Attributes.Count != 0)
                                    {
                                        using var ms = new System.IO.MemoryStream();
                                        using var writer = new System.IO.BinaryWriter(ms);
                                        setStack.Attributes.ToBytes(writer);
                                        acceptedItem.Nbt = System.Convert.ToBase64String(ms.ToArray());
                                    }
                                }

                                formObjectives[i].AcceptedItems.Add(acceptedItem);
                            }
                        }
                    }

                    if (formObjectives[i].Type == "kill")
                    {
                        formObjectives[i].AcceptedTargets = [.. SingleComposer.GetDropDown($"objective_targetSelect_{i}")?.SelectedValues ?? formObjectives[i].AcceptedTargets?.ToArray() ?? []];
                    }
                }

                var objectiveQuantity = SingleComposer.GetNumberInput($"objective_quantity_{i}");
                formObjectives[i].Count = int.TryParse(objectiveQuantity?.GetText(), out int count) ? count : formObjectives[i].Count;
            }

            var localRewards = formRewards;
            formRewards = [];
            for (int i = 0; i < localRewards.Count; i++)
            {
                if (rewardsInventory != null)
                {
                    var slot = rewardsInventory[i];

                    if (slot != null)
                    {
                        string? itemCode = slot is ItemSlotDisplayOnly displaySlot
                            ? displaySlot.GetActualItemCode() ?? slot.Itemstack?.Collectible.Code.ToString()
                            : slot.Itemstack?.Collectible.Code.ToString();

                        int rewardQuantity = int.TryParse(SingleComposer.GetNumberInput($"reward_quantity_{i}")?.GetText(), out int count) ? count : localRewards[i].Amount;

                        var reward = new QuestRewardDtoWithNullableCode
                        {
                            Code = itemCode,
                            Amount = rewardQuantity,
                        };

                        ItemStack? setStack = slot.Itemstack?.Clone();
                        // Serialize NBT data if present
                        if (setStack != null && setStack.Attributes != null)
                        {
                            // Remove bad NBT
                            foreach (var ignoreAttr in QuestNetworkHandler.NbtAttributesToIgnore)
                            {
                                if (setStack.Attributes.HasAttribute(ignoreAttr) == true)
                                {
                                    setStack.Attributes.RemoveAttribute(ignoreAttr);
                                }
                            }

                            if (setStack.Attributes.Count != 0)
                            {
                                using var ms = new System.IO.MemoryStream();
                                using var writer = new System.IO.BinaryWriter(ms);
                                setStack.Attributes.ToBytes(writer);
                                reward.Nbt = System.Convert.ToBase64String(ms.ToArray());
                            }
                        }

                        formRewards.Add(reward);
                    }
                }
            }
        }

        private double AddQuestEditorFields(GuiComposer composer, double yPos)
        {
            var innerYPos = yPos + 5;
            var fieldHeight = 30;
            var gap = 5;
            var leftMargin = 5;

            // Info
            composer.AddDynamicText("Info", CairoFont.WhiteSmallishText(), CreateChildBounds(leftMargin, innerYPos, 90, fieldHeight));
            innerYPos += fieldHeight + gap;

            // Title
            composer.AddDynamicText("Title:", CairoFont.WhiteSmallText(), CreateChildBounds(leftMargin, innerYPos + 5, 90, fieldHeight));
            composer.AddTextInput(CreateChildBounds((leftMargin * 2) + 90, innerYPos, 300, fieldHeight), BlankHandler, CairoFont.SmallTextInput().WithColor([1, 1, 1, 1]), "title");
            composer.GetTextInput("title").SetPlaceHolderText("Title...");
            composer.GetTextInput("title").SetMaxLength(50);
            composer.GetTextInput("title").SetValue(formTitle);
            innerYPos += fieldHeight + gap;

            // Description
            composer.AddDynamicText("Description:", CairoFont.WhiteSmallText(), CreateChildBounds(leftMargin, innerYPos + 5, 90, fieldHeight));
            composer.AddTextArea(CreateChildBounds((leftMargin * 2) + 90, innerYPos, 600, (fieldHeight * 3)), null, CairoFont.SmallTextInput().WithColor([1, 1, 1, 1]).WithFontSize(11), "description");
            composer.GetTextArea("description").SetMaxHeight(fieldHeight * 3);
            composer.GetTextArea("description").Autoheight = false;
            // Value is set after compose to prevent an error.
            innerYPos += (fieldHeight * 3) + gap;

            // Type
            composer.AddDynamicText("Recurrence:", CairoFont.WhiteSmallText(), CreateChildBounds(leftMargin, innerYPos + 5, 90, fieldHeight));
            composer.AddDropDown(["weekly", "monthly", "seasonal"], ["Weekly", "Monthly", "Seasonal"], 0, OnRecurrenceChanged, CreateChildBounds((leftMargin * 2) + 90, innerYPos, 100, fieldHeight), "type");
            composer.GetDropDown("type").SetSelectedValue(formType);
            innerYPos += fieldHeight + (gap * 2);

            // Dates
            innerYPos += gap;

            // In game time?
            if (formType == "seasonal")
            {
                composer.AddSwitch(OnToggleIgtSwitch, CreateChildBounds(leftMargin * 2, innerYPos, 20, fieldHeight), "inGameTime", 20, 0);
                composer.AddDynamicText("In-game time?", CairoFont.WhiteSmallText(), CreateChildBounds((leftMargin * 2) + 20 + leftMargin, innerYPos, 150, fieldHeight));
                composer.GetSwitch("inGameTime").SetValue(formIgt);

                innerYPos += fieldHeight + gap;
            }
            else
            {
                innerYPos -= 10;
            }

            if (formType == "weekly")
            {
                innerYPos += 10;

                // Rank
                composer.AddDynamicText("Rank:", CairoFont.WhiteSmallText(), CreateChildBounds(leftMargin, innerYPos + 5, 90, fieldHeight));
                composer.AddDropDown(["D", "C", "B", "A", "S"], ["D", "C", "B", "A", "S"], 0, OnRankChanged, CreateChildBounds((leftMargin * 2) + 90, innerYPos, 100, fieldHeight), "rank");
                composer.GetDropDown("rank").SetSelectedValue(formRank);
                innerYPos += fieldHeight + (gap * 2);

                // Repeating?
                composer.AddSwitch(OnToggleRepeatingSwitch, CreateChildBounds(leftMargin * 2, innerYPos, 20, fieldHeight), "repeating", 20, 0);
                composer.AddDynamicText("Repeat weekly?", CairoFont.WhiteSmallText(), CreateChildBounds((leftMargin * 2) + 20 + leftMargin, innerYPos, 150, fieldHeight));
                composer.GetSwitch("repeating").SetValue(formRepeating);

                innerYPos += fieldHeight + gap;
            }

            // START DATE
            composer.AddDynamicText("Start date:", CairoFont.WhiteSmallText(), CreateChildBounds(leftMargin, innerYPos + 5, 90, fieldHeight));
            var startDateX = (leftMargin * 2) + 90;

            var cal = capi.World.Calendar;
            DateTime minDate;
            if (formIgt)
            {
                int igYear = cal.Year + 1;
                int igDayOfYear = (int)cal.DayOfYear; // 0-indexed day within the year
                int igMonth = Math.Clamp(igDayOfYear / cal.DaysPerMonth + 1, 1, 12);
                int igDay = Math.Clamp(igDayOfYear % cal.DaysPerMonth + 1, 1, cal.DaysPerMonth);
                minDate = new DateTime(igYear, igMonth, igDay);
            }
            else
            {
                // Use server's current date, not client's date
                minDate = serverLocalTime > 0
                    ? DateTimeOffset.FromUnixTimeSeconds(serverLocalTime).ToOffset(TimeSpan.FromHours(serverTimezoneOffset)).Date
                    : DateTime.Today; // Fallback to client time if server time not available
            }

            System.Func<int, string> formatYear = formIgt ? y => y.ToString("D4") : y => y.ToString();

            // Local helper: get valid Sundays for a year/month from minDate
            string[] GetSundaysForMonth(int year, int month)
            {
                bool isBeforeMin = year < minDate.Year || (year == minDate.Year && month < minDate.Month);
                bool isCurrentMin = year == minDate.Year && month == minDate.Month;
                int dim = formIgt ? cal.DaysPerMonth : DateTime.DaysInMonth(year, month);

                int fd;
                if (isBeforeMin)
                {
                    fd = dim + 1; // No valid days
                }
                else if (isCurrentMin)
                {
                    // If today is Sunday, start from today. Otherwise, find last Sunday.
                    if (minDate.DayOfWeek == DayOfWeek.Sunday)
                    {
                        fd = minDate.Day;
                    }
                    else
                    {
                        // Find the most recent Sunday
                        int daysToSubtract = (int)minDate.DayOfWeek; // Sunday = 0, Monday = 1, etc.
                        int lastSunday = minDate.Day - daysToSubtract;
                        fd = Math.Max(1, lastSunday); // Don't go before day 1
                    }
                }
                else
                {
                    fd = 1; // Future month, start from day 1
                }

                int cnt = Math.Max(0, dim - fd + 1);
                return [.. Enumerable.Range(fd, cnt)
                    .Where(d => new DateTime(year, month, d).DayOfWeek == DayOfWeek.Sunday)
                    .Select(d => d.ToString("D2"))];
            }

            // --- Weekly: auto-advance formStartMonth/Year if selected month has no Sundays ---
            if (formType == "weekly")
            {
                int searchYear = int.TryParse(formStartYear, out int sy) ? sy : minDate.Year;
                int searchMonth = int.TryParse(formStartMonth, out int sm) ? sm : minDate.Month;

                if (GetSundaysForMonth(searchYear, searchMonth).Length == 0)
                {
                    if (++searchMonth > 12) { searchMonth = 1; searchYear++; }

                    int maxYear = minDate.Year + 5;
                    while (searchYear <= maxYear && GetSundaysForMonth(searchYear, searchMonth).Length == 0)
                    {
                        if (++searchMonth > 12) { searchMonth = 1; searchYear++; }
                    }

                    if (searchYear <= maxYear)
                    {
                        formStartYear = formatYear(searchYear);
                        formStartMonth = searchMonth.ToString("D2");
                    }
                }
            }

            // --- Year dropdown ---
            // Weekly: exclude years where no valid Sundays exist at all
            string[] yearValues = formType == "weekly"
                ? [.. Enumerable.Range(minDate.Year, 6)
                    .Where(y =>
                    {
                        int startM = y == minDate.Year ? minDate.Month : 1;
                        return Enumerable.Range(startM, 13 - startM).Any(m => GetSundaysForMonth(y, m).Length > 0);
                    })
                    .Select(formatYear)]
                : [.. Enumerable.Range(minDate.Year, 6).Select(formatYear)];

            if (yearValues.Length == 0)
                yearValues = [formatYear(minDate.Year)]; // safety fallback

            // For IGT: display names show VS year (DateTime year - 1), but values remain DateTime-compatible
            string[] yearNames = formIgt
                ? yearValues.Select(v => (int.Parse(v) - 1).ToString("D4")).ToArray()
                : yearValues;

            if (!yearValues.Contains(formStartYear))
                formStartYear = yearValues[0];

            int selectedYear = int.Parse(formStartYear);

            // --- Month dropdown: only current month onward in the min-year, all months otherwise ---
            int totalMonths = 12;
            int firstMonth = (selectedYear == minDate.Year) ? minDate.Month : 1;
            int monthCount = totalMonths - firstMonth + 1;

            string[] allMonthNames = [.. System.Globalization.DateTimeFormatInfo.InvariantInfo.MonthNames.Where(m => !string.IsNullOrEmpty(m))];
            string[] monthValues = [.. Enumerable.Range(firstMonth, monthCount).Select(m => m.ToString("D2"))];
            string[] monthNames = [.. Enumerable.Range(firstMonth, monthCount).Select(m => allMonthNames[m - 1])];

            if (!monthValues.Contains(formStartMonth))
                formStartMonth = monthValues[0];

            int selectedMonth = int.Parse(formStartMonth);

            // --- Day / Sunday arrays ---
            bool isBeforeMinMonth = selectedYear < minDate.Year ||
                                    (selectedYear == minDate.Year && selectedMonth < minDate.Month);
            bool isMinMonth = selectedYear == minDate.Year && selectedMonth == minDate.Month;

            int daysInMonth = formIgt ? cal.DaysPerMonth : DateTime.DaysInMonth(selectedYear, selectedMonth);
            int firstDay = isBeforeMinMonth ? daysInMonth + 1 : isMinMonth ? minDate.Day : 1;
            int count = Math.Max(0, daysInMonth - firstDay + 1);

            string[] dayValues = [.. Enumerable.Range(firstDay, count).Select(d => d.ToString("D2"))];
            string[] sundayValues = GetSundaysForMonth(selectedYear, selectedMonth);

            // Year
            composer.AddDropDown(yearValues, yearNames, 0, OnStartDateChanged, CreateChildBounds(startDateX, innerYPos, 80, fieldHeight), "startYear");
            composer.GetDropDown("startYear").SetSelectedValue(formStartYear);
            startDateX += leftMargin + 80;

            // Month
            composer.AddDropDown(monthValues, monthNames, 0, OnStartDateChanged, CreateChildBounds(startDateX, innerYPos, 110, fieldHeight), "startMonth");
            composer.GetDropDown("startMonth").SetSelectedValue(formStartMonth);
            startDateX += leftMargin + 110;

            // Day
            var valuesToUse = formType == "weekly" ? sundayValues : formType == "monthly" ? ["1"] : dayValues;

            if (valuesToUse.Length > 0 && !valuesToUse.Contains(formStartDay))
                formStartDay = valuesToUse[0];

            RecalculateEndDate();

            composer.AddDropDown(valuesToUse, valuesToUse, 0, OnStartDateChanged, CreateChildBounds(startDateX, innerYPos, 60, fieldHeight), "startDay");
            composer.GetDropDown("startDay").SetSelectedValue(formStartDay);
            startDateX += leftMargin + 60;

            // Show real-time estimate for IGT start date
            if (formIgt && int.TryParse(formStartYear, out int startYearVal) &&
                int.TryParse(formStartMonth, out int startMonthVal) &&
                int.TryParse(formStartDay, out int startDayVal))
            {
                string startEstimate = CalculateRealTimeEstimate(startYearVal, startMonthVal, startDayVal, isStartOfDay: true);
                composer.AddDynamicText(startEstimate, CairoFont.WhiteSmallText().WithColor([0.7, 0.9, 1.0, 1.0]), CreateChildBounds(startDateX, innerYPos + 5, 200, fieldHeight));
            }

            innerYPos += fieldHeight + gap;

            // END DATE
            composer.AddDynamicText("End date:", CairoFont.WhiteSmallText(), CreateChildBounds(leftMargin, innerYPos + 5, 90, fieldHeight));
            var endDateX = (leftMargin * 2) + 90;

            // Clamp end date values if they fell out of valid range
            if (!yearValues.Contains(formEndYear))
                formEndYear = yearValues[0];

            int endSelectedYear = int.Parse(formEndYear);

            // --- Calculate month values specific to the end date's selected year ---
            // BUT use START date as minimum, not minDate
            int endFirstMonth = (endSelectedYear == selectedYear) ? selectedMonth : 1;
            int endMonthCount = 12 - endFirstMonth + 1;

            string[] endMonthValues = [.. Enumerable.Range(endFirstMonth, endMonthCount).Select(m => m.ToString("D2"))];
            string[] endMonthNames = [.. Enumerable.Range(endFirstMonth, endMonthCount).Select(m => allMonthNames[m - 1])];

            if (!endMonthValues.Contains(formEndMonth))
                formEndMonth = endMonthValues[0];

            int endSelectedMonth = int.Parse(formEndMonth);

            // --- Calculate day values using START date as minimum ---
            bool endIsBeforeStartMonth = endSelectedYear < selectedYear ||
                                         (endSelectedYear == selectedYear && endSelectedMonth < selectedMonth);
            bool endIsStartMonth = endSelectedYear == selectedYear && endSelectedMonth == selectedMonth;

            int endDaysInMonth = formIgt ? cal.DaysPerMonth : DateTime.DaysInMonth(endSelectedYear, endSelectedMonth);
            int endFirstDay = endIsBeforeStartMonth ? endDaysInMonth + 1
                            : endIsStartMonth ? int.Parse(formStartDay)
                            : 1;
            int endCount = Math.Max(0, endDaysInMonth - endFirstDay + 1);

            string[] endDayValues = [.. Enumerable.Range(endFirstDay, endCount).Select(d => d.ToString("D2"))];


            if (endDayValues.Length > 0 && !endDayValues.Contains(formEndDay))
                formEndDay = endDayValues[0];

            // End Year
            composer.AddDropDown(yearValues, yearNames, 0, OnEndDateChanged, CreateChildBounds(endDateX, innerYPos, 80, fieldHeight), "endYear");
            composer.GetDropDown("endYear").SetSelectedValue(formEndYear);
            endDateX += leftMargin + 80;

            // End Month - use endMonthValues/endMonthNames
            composer.AddDropDown(endMonthValues, endMonthNames, 0, OnEndDateChanged, CreateChildBounds(endDateX, innerYPos, 110, fieldHeight), "endMonth");
            composer.GetDropDown("endMonth").SetSelectedValue(formEndMonth);
            endDateX += leftMargin + 110;

            // End Day
            composer.AddDropDown(endDayValues, endDayValues, 0, OnEndDateChanged, CreateChildBounds(endDateX, innerYPos, 60, fieldHeight), "endDay");
            composer.GetDropDown("endDay").SetSelectedValue(formEndDay);
            endDateX += leftMargin + 60;

            // Show real-time estimate for IGT end date
            if (formIgt && int.TryParse(formEndYear, out int endYearVal) &&
                int.TryParse(formEndMonth, out int endMonthVal) &&
                int.TryParse(formEndDay, out int endDayVal))
            {
                string endEstimate = CalculateRealTimeEstimate(endYearVal, endMonthVal, endDayVal, isStartOfDay: false);
                composer.AddDynamicText(endEstimate, CairoFont.WhiteSmallText().WithColor([0.7, 0.9, 1.0, 1.0]), CreateChildBounds(endDateX, innerYPos + 5, 200, fieldHeight));
            }

            innerYPos += fieldHeight + (gap * 2);

            // Objectives
            var objectivesCheckpoint = innerYPos;
            composer.AddDynamicText("Objectives", CairoFont.WhiteSmallishText(), CreateChildBounds(leftMargin, innerYPos, 120, fieldHeight));
            innerYPos += fieldHeight + gap;

            // Populate objective inventory slots from formObjectives before rendering
            if (objectiveInventory != null)
            {
                // Clear all slots first
                for (int i = 0; i < objectiveInventory.Count; i++)
                {
                    objectiveInventory[i].Itemstack = null;
                }

                // Populate from formObjectives
                for (int i = 0; i < formObjectives.Count && i < 10; i++)
                {
                    var obj = formObjectives[i];
                    if (obj.Type == "turn_in" && obj.AcceptedItems != null)
                    {
                        for (int itemIdx = 0; itemIdx < obj.AcceptedItems.Count && itemIdx < 100; itemIdx++)
                        {
                            var acceptedItem = obj.AcceptedItems[itemIdx];
                            int slotId = i * 100 + itemIdx;

                            CollectibleObject? collectible = capi.World.GetItem(new AssetLocation(acceptedItem.Code));
                            if (collectible == null)
                            {
                                collectible = capi.World.GetBlock(new AssetLocation(acceptedItem.Code));
                            }

                            if (collectible != null)
                            {
                                var itemStack = new ItemStack(collectible, 1);

                                // Restore NBT data if present
                                if (!string.IsNullOrEmpty(acceptedItem.Nbt))
                                {
                                    var nbtBytes = System.Convert.FromBase64String(acceptedItem.Nbt);
                                    using var ms = new System.IO.MemoryStream(nbtBytes);
                                    using var reader = new System.IO.BinaryReader(ms);
                                    itemStack.Attributes = new Vintagestory.API.Datastructures.TreeAttribute();
                                    itemStack.Attributes.FromBytes(reader);
                                }

                                objectiveInventory[slotId].Itemstack = itemStack;
                            }
                        }
                    }
                }
            }

            for (int i = 0; i < formObjectives.Count; i++)
            {
                var objective = formObjectives[i];
                var objX = (leftMargin * 2);

                // index
                composer.AddDynamicText($"{i + 1}.", CairoFont.WhiteSmallText(), CreateChildBounds((leftMargin * 2), innerYPos + 10, 30, fieldHeight));

                // delete icon
                if (formObjectives.Count != 1)
                {
                    var localI = i;
                    composer.AddIconButton("none", something =>
                    {
                        DeleteObjective(localI);
                    }, CreateChildBounds((leftMargin), innerYPos + 40, 25, 25));
                }
                objX += 30;

                // type dropdown
                composer.AddDropDown(["turn_in", "kill"], ["Turn in", "Kill"], 0, OnObjectiveTypeChanged, CreateChildBounds((leftMargin * 2) + 30, innerYPos + gap, 80, fieldHeight), $"objective_type_{i}");
                composer.GetDropDown($"objective_type_{i}").SetSelectedValue(formObjectives[i].Type);

                composer.AddNumberInput(CreateChildBounds((leftMargin * 2) + 30, innerYPos + fieldHeight + (gap * 2), 80, fieldHeight), null, CairoFont.SmallTextInput().WithColor([1, 1, 1, 1]), $"objective_quantity_{i}");
                composer.GetNumberInput($"objective_quantity_{i}").SetPlaceHolderText("Quantity");
                composer.GetNumberInput($"objective_quantity_{i}").SetMaxLength(4);
                composer.GetNumberInput($"objective_quantity_{i}").SetValue($"{formObjectives[i].Count}");
                objX += 85;

                if (formObjectives[i].Type == "turn_in")
                {
                    // Add item slot so the user can place an item inside to designate the item they wish to set as the turn in objective
                    // Calculate how many filled slots exist for this objective
                    int filledSlots = objective.AcceptedItems.Count;
                    int totalSlotsToShow = filledSlots + 1; // Always show one empty slot at the end

                    // Render all slots (existing items + 1 empty)
                    for (int itemIdx = 0; itemIdx < totalSlotsToShow && itemIdx < 100; itemIdx++)
                    {
                        int slotId = i * 100 + itemIdx;

                        if (objectiveInventory != null)
                        {
                            composer.AddItemSlotGrid(
                                objectiveInventory,
                                (slotIdParam) => new ItemSlotDisplayOnly(objectiveInventory),
                                1,
                                [slotId],
                                CreateChildBounds(objX, innerYPos, 49, 49),
                                $"objective_item_{i}_{itemIdx}"
                            );
                        }
                        objX += 49;

                        // Wrap to new line after 10 items
                        if ((itemIdx + 1) % 6 == 0)
                        {
                            innerYPos += 49;
                            objX = (leftMargin * 2) + 115; // Reset X, indented
                        }
                    }
                }

                if (formObjectives[i].Type == "kill")
                {
                    // This code pretty much directly from VS survival mod for the spawner dropdown, thx ros
                    List<string> entityCodes = new List<string>();
                    List<string> entityNames = new List<string>();
                    int localI = i;

                    var entityProps = capi.World
                        .SearchItems(new AssetLocation("*", "creature*"))
                        .Select(item => new AssetLocation(item.Code.Domain, item.CodeEndWithoutParts(1)))
                        .Select(location => capi.World.GetEntityType(location))
                        .OfType<EntityProperties>()
                        .OrderBy(type => Lang.Get("item-creature-" + type.Code.Path))
                        .OrderBy(type => type.Code.FirstCodePart())
                    ;

                    foreach (var type in entityProps)
                    {
                        // Quick very ugly hack for now: We have like 150 butterflies, but the drop down system is not designed for large lists. Lets ignore them for now
                        if (type.Code.Path.Contains("butterfly")) continue;

                        entityCodes.Add(type.Code.ToString());
                        entityNames.Add(Lang.Get("item-creature-" + type.Code.Path));
                    }

                    composer.AddMultiSelectDropDown(entityCodes.ToArray(), entityNames.ToArray(), -1, null, CreateChildBounds(objX, (innerYPos + gap), 300, fieldHeight), $"objective_targetSelect_{localI}");
                }

                innerYPos += (fieldHeight * 2) + (gap * 3);
            }

            // Add objective button
            if (formObjectives.Count < QUEST_OBJECTIVE_LIMIT)
            {
                composer.AddButton("Add", OnAddObjective, CreateChildBounds((leftMargin * 2), innerYPos, 80, fieldHeight), CairoFont.WhiteSmallText().WithFontSize(11).WithOrientation(EnumTextOrientation.Center), EnumButtonStyle.Small, "addObjective");
            }

            double objectivesEndY = innerYPos;
            innerYPos = objectivesCheckpoint;

            composer.AddDynamicText("Rewards", CairoFont.WhiteSmallishText(), CreateChildBounds(leftMargin + 460, innerYPos, 120, fieldHeight));
            innerYPos += fieldHeight + gap;

            // Populate reward inventory slots from formRewards before rendering
            if (rewardsInventory != null)
            {
                // Clear all slots first
                for (int i = 0; i < rewardsInventory.Count; i++)
                {
                    rewardsInventory[i].Itemstack = null;
                }

                // Populate from formRewards
                for (int i = 0; i < formRewards.Count && i < rewardsInventory.Count; i++)
                {
                    var reward = formRewards[i];

                    if (reward.Code != null)
                    {
                        CollectibleObject? collectible = capi.World.GetItem(new AssetLocation(reward.Code));
                        if (collectible == null)
                        {
                            collectible = capi.World.GetBlock(new AssetLocation(reward.Code));
                        }

                        // For virtual items (like grspoints), create a minimal stack using a placeholder item
                        if (collectible == null && reward.Code == GRS_POINTS_CODE)
                        {
                            // Use any real item as a placeholder - it will be transformed by SetItemstack
                            collectible = capi.World.GetItem(new AssetLocation("game:paper-parchment"));
                        }

                        if (collectible != null)
                        {
                            var itemStack = new ItemStack(collectible, 1);

                            // Restore NBT data if present
                            if (!string.IsNullOrEmpty(reward.Nbt))
                            {
                                var nbtBytes = System.Convert.FromBase64String(reward.Nbt);
                                using var ms = new System.IO.MemoryStream(nbtBytes);
                                using var reader = new System.IO.BinaryReader(ms);
                                itemStack.Attributes = new Vintagestory.API.Datastructures.TreeAttribute();
                                itemStack.Attributes.FromBytes(reader);
                            }

                            // Use SetItemstack for transformation (grspoints -> parchment)
                            if (rewardsInventory[i] is ItemSlotDisplayOnly displaySlot)
                            {
                                displaySlot.SetItemstack(itemStack, reward.Code);
                            }
                            else
                            {
                                rewardsInventory[i].Itemstack = itemStack;
                            }
                        }
                    }
                }
            }

            for (int i = 0; i < formRewards.Count; i++)
            {
                var localI = i;
                var reward = formRewards[i];
                var objX = (leftMargin * 2) + 460;

                // index
                composer.AddDynamicText($"{i + 1}.", CairoFont.WhiteSmallText(), CreateChildBounds(objX, innerYPos + 10, 30, fieldHeight));

                // delete icon
                if (formRewards.Count != 1)
                {
                    composer.AddIconButton("none", something =>
                    {
                        DeleteReward(localI);
                    }, CreateChildBounds(objX, innerYPos + 40, 25, 25));
                }

                // ez-add
                composer.AddDropDown(["<Empty>", "grs", "tails", "crowns"], ["EZ-Add", "GRS Points", "Tails", "Crowns"], 0, (choice, test) =>
                {
                    if (choice == "grs")
                    {
                        SaveFormState();
                        formRewards[localI].Code = GRS_POINTS_CODE;
                        SetupDialog();
                    }

                    if (choice == "tails")
                    {
                        SaveFormState();
                        if (tailsDefinition != null)
                        {
                            formRewards[localI].Code = tailsDefinition.Code;
                            formRewards[localI].Nbt = tailsDefinition.Nbt;
                        }
                        else
                        {
                            // Fallback if no currency definition is available
                            capi.ShowChatMessage("[Quest Editor] Tails currency not configured on server");
                        }
                        SetupDialog();
                    }

                    if (choice == "crowns")
                    {
                        SaveFormState();
                        if (crownsDefinition != null)
                        {
                            formRewards[localI].Code = crownsDefinition.Code;
                            formRewards[localI].Nbt = crownsDefinition.Nbt;
                        }
                        else
                        {
                            // Fallback if no currency definition is available
                            capi.ShowChatMessage("[Quest Editor] Crowns currency not configured on server");
                        }
                        SetupDialog();
                    }

                    composer.GetDropDown($"reward_type_{localI}").SetSelectedValue("");
                }, CreateChildBounds(objX + 30, innerYPos + gap, 90, fieldHeight), $"reward_type_{localI}");

                composer.AddNumberInput(CreateChildBounds(objX + 30, innerYPos + fieldHeight + (gap * 2), 90, fieldHeight), null, CairoFont.SmallTextInput().WithColor([1, 1, 1, 1]), $"reward_quantity_{i}");
                composer.GetNumberInput($"reward_quantity_{i}").SetPlaceHolderText("Quantity");
                composer.GetNumberInput($"reward_quantity_{i}").SetMaxLength(4);
                composer.GetNumberInput($"reward_quantity_{i}").SetValue($"{formRewards[i].Amount}");
                objX += 125;

                if (rewardsInventory != null)
                {
                    composer.AddItemSlotGrid(
                        rewardsInventory,
                        (slotIdParam) => new ItemSlotDisplayOnly(rewardsInventory),
                        1,
                        [localI],
                        CreateChildBounds(objX, innerYPos, 49, 49),
                        $"reward_item_{localI}"
                    );
                }

                innerYPos += (fieldHeight * 2) + (gap * 3);
            }

            // Add reward button
            if (formRewards.Count < QUEST_REWARD_LIMIT)
            {
                composer.AddButton("Add", OnAddReward, CreateChildBounds((leftMargin * 2) + 460, innerYPos, 80, fieldHeight), CairoFont.WhiteSmallText().WithFontSize(11).WithOrientation(EnumTextOrientation.Center), EnumButtonStyle.Small, "addReward");
            }

            return Math.Max(objectivesEndY, innerYPos);
        }

        private void DeleteObjective(int i)
        {
            SaveFormState();
            formObjectives.RemoveAt(i);
            SetupDialog();
        }

        private void DeleteReward(int i)
        {
            SaveFormState();
            formRewards.RemoveAt(i);
            SetupDialog();
        }

        private bool OnAddObjective()
        {
            SaveFormState();
            formObjectives.Add(new QuestObjectiveDto
            {
                Id = rnd.Next(1, 10000000),
                AcceptedItems = [],
                AcceptedTargets = [],
                Count = 5,
                Type = "turn_in"
            });
            SetupDialog();
            return true;
        }

        private bool OnAddReward()
        {
            SaveFormState();
            formRewards.Add(new QuestRewardDtoWithNullableCode
            {
                Amount = 5,
            });

            SetupDialog();
            return true;
        }

        private void BlankHandler(string obj)
        {
            // do nothing
        }

        private void OnObjectiveTypeChanged(string code, bool selected)
        {
            SaveFormState();
            SetupDialog();
        }

        private void OnStartDateChanged(string code, bool selected)
        {
            SaveFormState();
            RecalculateEndDate();
            SetupDialog();
        }
        private void OnEndDateChanged(string code, bool selected)
        {
            // Only allowed for seasonal quests
            if (formType != "seasonal")
                return;

            SaveFormState();

            // Validate that end date is not before start date
            if (int.TryParse(formStartYear, out int startYear) &&
                int.TryParse(formStartMonth, out int startMonth) &&
                int.TryParse(formStartDay, out int startDay) &&
                int.TryParse(formEndYear, out int endYear) &&
                int.TryParse(formEndMonth, out int endMonth) &&
                int.TryParse(formEndDay, out int endDay))
            {
                var startDate = new DateTime(startYear, startMonth, startDay);
                var endDate = new DateTime(endYear, endMonth, endDay);

                if (endDate < startDate)
                {
                    // Reset end date to start date
                    formEndYear = formStartYear;
                    formEndMonth = formStartMonth;
                    formEndDay = formStartDay;
                }
            }

            SetupDialog();
        }

        private void OnToggleIgtSwitch(bool isOn)
        {
            SaveFormState();
            formStartYear = "";
            formStartMonth = "";
            formStartDay = "";
            SetupDialog();
        }

        private void OnToggleRepeatingSwitch(bool isOn)
        {
            SaveFormState();
            SetupDialog();
        }

        private void OnRecurrenceChanged(string code, bool selected)
        {
            SaveFormState();

            if (code != "seasonal")
            {
                // force igt to false
                formIgt = false;
            }

            if (code != "weekly")
            {
                formRepeating = false;
                formRank = "D";
            }

            RecalculateEndDate();

            SetupDialog();
        }

        private void OnRankChanged(string code, bool selected)
        {
            SaveFormState();
            SetupDialog();
        }

        /// <summary>
        /// Recalculates the end date based on the current start date and recurrence type
        /// </summary>
        private void RecalculateEndDate()
        {
            // Parse current start date (formStart* is already updated by SaveFormState)
            if (!int.TryParse(formStartYear, out int startYear) ||
                !int.TryParse(formStartMonth, out int startMonth) ||
                !int.TryParse(formStartDay, out int startDay))
            {
                return;
            }

            var cal = capi.World.Calendar;

            System.Func<int, string> formatYear = formIgt ? y => y.ToString("D4") : y => y.ToString();

            if (formType == "seasonal")
            {
                // Only update end date if start date is now greater than current end date
                if (int.TryParse(formEndYear, out int endYear) &&
                    int.TryParse(formEndMonth, out int endMonth) &&
                    int.TryParse(formEndDay, out int endDay))
                {
                    var startDate = new DateTime(startYear, startMonth, startDay);
                    var endDate = new DateTime(endYear, endMonth, endDay);

                    if (startDate > endDate)
                    {
                        formEndYear = formStartYear;
                        formEndMonth = formStartMonth;
                        formEndDay = formStartDay;
                    }
                }
                else
                {
                    // No valid end date yet, set to start date
                    formEndYear = formStartYear;
                    formEndMonth = formStartMonth;
                    formEndDay = formStartDay;
                }
            }
            else if (formType == "monthly")
            {
                // End date is the last day of the month
                int daysInMonth = formIgt ? cal.DaysPerMonth : DateTime.DaysInMonth(startYear, startMonth);
                formEndYear = formStartYear;
                formEndMonth = formStartMonth;
                formEndDay = daysInMonth.ToString("D2");
            }
            else if (formType == "weekly")
            {
                // End date is the following Saturday
                var startDate = new DateTime(startYear, startMonth, startDay);

                // Calculate days until Saturday (DayOfWeek.Saturday = 6)
                var endDate = startDate.AddDays(6);

                formEndYear = formatYear(endDate.Year);
                formEndMonth = endDate.Month.ToString("D2");
                formEndDay = endDate.Day.ToString("D2");
            }
        }

        /// <summary>
        /// Calculates the estimated real-time when an in-game date will occur.
        /// </summary>
        /// <param name="targetYear">Target in-game year (DateTime format, so VS year + 1)</param>
        /// <param name="targetMonth">Target month (1-12)</param>
        /// <param name="targetDay">Target day of month (1-based)</param>
        /// <param name="isStartOfDay">True for start of day (midnight), false for end of day</param>
        /// <returns>Formatted date string like "2025/01/15 03:30:00 PM"</returns>
        private string CalculateRealTimeEstimate(int targetYear, int targetMonth, int targetDay, bool isStartOfDay)
        {
            var cal = capi.World.Calendar;

            // Current in-game state
            int currentVsYear = cal.Year; // VS year (0-indexed)
            int currentDayOfYear = (int)cal.DayOfYear; // 0-indexed day within the year
            float currentHourOfDay = cal.FullHourOfDay;
            int daysPerMonth = cal.DaysPerMonth;
            int hoursPerDay = (int)cal.HoursPerDay;
            float calendarSpeedMul = cal.CalendarSpeedMul;

            // Convert target date to VS year format (subtract 1 since we store DateTime year)
            int targetVsYear = targetYear - 1;

            // Calculate target day of year (0-indexed)
            // Month is 1-12, day is 1-based
            int targetDayOfYear = (targetMonth - 1) * daysPerMonth + (targetDay - 1);

            // Calculate total in-game hours from some reference point to current time
            int daysPerYear = daysPerMonth * 12;
            double currentTotalHours = (currentVsYear * daysPerYear + currentDayOfYear) * hoursPerDay + currentHourOfDay;

            // Calculate total in-game hours to target
            // For start of day: hour 0
            // For end of day: last hour of the day (hoursPerDay)
            double targetHourOfDay = isStartOfDay ? 0 : hoursPerDay;
            double targetTotalHours = (targetVsYear * daysPerYear + targetDayOfYear) * hoursPerDay + targetHourOfDay;

            // Difference in in-game hours
            double inGameHoursUntilTarget = targetTotalHours - currentTotalHours;

            if (inGameHoursUntilTarget <= 0)
            {
                return isStartOfDay ? "(now)" : "(ending)";
            }

            // Convert in-game hours to real hours
            // At CalendarSpeedMul = 1, one full in-game day (hoursPerDay) takes 24 real minutes (1.6 real hours)
            // realMinutes = inGameHours * (24 / hoursPerDay) / calendarSpeedMul
            double realMinutesPerInGameHour = (24.0 / hoursPerDay) / calendarSpeedMul;
            double realMinutesUntilTarget = inGameHoursUntilTarget * realMinutesPerInGameHour;

            // Calculate the real-world DateTime when this will occur
            DateTime realDateTime = DateTime.Now.AddMinutes(realMinutesUntilTarget);

            // Format as YYYY/MM/DD HH:MM:SS AM|PM
            return realDateTime.ToString("yyyy/MM/dd hh:mm tt");
        }

        private static ElementBounds CreateChildBounds(double x, double y, double width, double height)
        {
            return ElementBounds.Fixed(x, y, width, height);
        }

        private void OnTitleBarCloseClicked()
        {
            TryClose();
        }

        private bool OnCancelClicked()
        {
            TryClose();
            return true;
        }

        private bool OnDeleteClicked()
        {
            if (existingQuest == null)
            {
                capi.ShowChatMessage("Cannot delete - no quest selected");
                return false;
            }

            // Create confirmation dialog
            var confirmDialog = new ConfirmDeleteQuestDialog(capi, existingQuest.Title, () =>
            {
                var questNetworkHandler = modSystem.QuestNetworkHandler;
                if (questNetworkHandler == null)
                {
                    capi.ShowChatMessage("Quest network handler not available");
                    return;
                }

                // Set up callback for delete response
                questNetworkHandler.OnQuestDeleteResponse = (response) =>
                {
                    if (response.Success)
                    {
                        capi.ShowChatMessage("Quest deleted successfully");
                        OnQuestSaved?.Invoke();
                        TryClose();
                    }
                    else
                    {
                        capi.ShowChatMessage($"Failed to delete quest: {response.Message}");
                    }

                    // Clear the callback
                    questNetworkHandler.OnQuestDeleteResponse = null;
                };

                // Send delete request
                var player = capi.World.Player;
                questNetworkHandler.DeleteQuest(player.PlayerUID, existingQuest.Id);
            });

            confirmDialog.TryOpen();
            return true;
        }

        private bool OnSaveClicked(bool saveAs = false)
        {
            // Save current form state to capture all field values
            SaveFormState();

            // Validate required fields
            if (string.IsNullOrWhiteSpace(formTitle))
            {
                capi.ShowChatMessage("Quest title is required");
                return false;
            }

            if (string.IsNullOrWhiteSpace(formDescription))
            {
                capi.ShowChatMessage("Quest description is required");
                return false;
            }

            if (formObjectives.Count == 0)
            {
                capi.ShowChatMessage("At least one objective is required");
                return false;
            }

            // Validate all objectives have valid data
            for (int i = 0; i < formObjectives.Count; i++)
            {
                var obj = formObjectives[i];
                if (obj.Count <= 0)
                {
                    capi.ShowChatMessage($"Objective {i + 1}: Quantity must be greater than 0");
                    return false;
                }

                if (obj.Type == "turn_in" && (obj.AcceptedItems == null || obj.AcceptedItems.Count == 0))
                {
                    capi.ShowChatMessage($"Objective {i + 1}: At least one item must be specified for turn_in objectives");
                    return false;
                }

                if (obj.Type == "kill" && (obj.AcceptedTargets == null || obj.AcceptedTargets.Count == 0))
                {
                    capi.ShowChatMessage($"Objective {i + 1}: At least one target must be specified for kill objectives");
                    return false;
                }
            }

            // Validate rewards (filter out empty ones)
            var validRewards = formRewards.Where(r => !string.IsNullOrEmpty(r.Code)).ToList();
            if (validRewards.Count == 0)
            {
                capi.ShowChatMessage("At least one reward is required");
                return false;
            }

            // Validate all rewards have valid quantity
            foreach (var reward in validRewards)
            {
                if (reward.Amount <= 0)
                {
                    capi.ShowChatMessage("All rewards must have a quantity greater than 0");
                    return false;
                }
            }

            // Validate and format dates
            if (!int.TryParse(formStartYear, out int startYear) ||
                !int.TryParse(formStartMonth, out int startMonth) ||
                !int.TryParse(formStartDay, out int startDay))
            {
                capi.ShowChatMessage("Invalid start date");
                return false;
            }

            if (!int.TryParse(formEndYear, out int endYear) ||
                !int.TryParse(formEndMonth, out int endMonth) ||
                !int.TryParse(formEndDay, out int endDay))
            {
                capi.ShowChatMessage("Invalid end date");
                return false;
            }

            // For IGT, convert back to year 0000 if displaying year 0001
            string startsAt;
            string expiresAt;

            if (formIgt)
            {
                // VS year is DateTime year - 1, so if user sees "0001" we store "0000-MM-DD"
                int vsStartYear = startYear - 1;
                int vsEndYear = endYear - 1;
                startsAt = $"{vsStartYear:D4}-{startMonth:D2}-{startDay:D2}";
                expiresAt = $"{vsEndYear:D4}-{endMonth:D2}-{endDay:D2}";
            }
            else
            {
                startsAt = $"{startYear:D4}-{startMonth:D2}-{startDay:D2}";
                expiresAt = $"{endYear:D4}-{endMonth:D2}-{endDay:D2}";
            }

            // Build QuestSaveDto
            var questDto = new QuestSaveDto
            {
                Id = saveAs ? null : existingQuest?.Id,
                RecurrenceType = formType,
                Title = formTitle,
                Description = formDescription,
                Objectives = formObjectives.Select(o => new QuestObjectiveDto
                {
                    Id = o.Id,
                    Type = o.Type,
                    Count = o.Count,
                    AcceptedTargets = o.AcceptedTargets ?? [],
                    AcceptedItems = o.AcceptedItems ?? []
                }).ToList(),
                Rewards = validRewards.Select(r => new QuestRewardDto
                {
                    Code = r.Code!,
                    Nbt = r.Nbt,
                    Amount = r.Amount
                }).ToList(),
                StartsAt = startsAt,
                ExpiresAt = expiresAt,
                UsesIngameTime = formIgt,
                Repeat = formRepeating,
                Rank = formRank
            };

            // Set up callback for save response
            var questNetworkHandler = modSystem.QuestNetworkHandler;
            if (questNetworkHandler == null)
            {
                capi.ShowChatMessage("Quest network handler not available");
                return false;
            }

            questNetworkHandler.OnQuestSaveResponse = (response) =>
            {
                if (response.Success)
                {
                    capi.ShowChatMessage(existingQuest != null ? "Quest updated successfully" : "Quest created successfully");
                    OnQuestSaved?.Invoke();
                    TryClose();
                }
                else
                {
                    capi.ShowChatMessage($"Failed to save quest: {response.Message}");
                }

                // Clear the callback
                questNetworkHandler.OnQuestSaveResponse = null;
            };

            // Send save request
            var player = capi.World.Player;
            questNetworkHandler.SaveQuest(player.PlayerUID, questDto);

            return true;
        }

        public override void OnGuiClosed()
        {
            base.OnGuiClosed();
            OnDialogClosed?.Invoke();
        }

        public override void Dispose()
        {
            if (objectiveInventory != null)
            {
                objectiveInventory.SlotModified -= OnInventorySlotModified;

                // Explicitly clear all slots to help with texture disposal
                for (int i = 0; i < objectiveInventory.Count; i++)
                {
                    if (objectiveInventory[i]?.Itemstack != null)
                    {
                        objectiveInventory[i].Itemstack = null;
                        objectiveInventory[i].MarkDirty();
                    }
                }

                objectiveInventory.DiscardAll();
            }
            base.Dispose();
        }
    }

    /// <summary>
    /// Simple confirmation dialog for quest deletion
    /// </summary>
    internal class ConfirmDeleteQuestDialog : GuiDialog
    {
        private readonly string questTitle;
        private readonly Action onConfirm;

        public override string? ToggleKeyCombinationCode => null;

        public ConfirmDeleteQuestDialog(ICoreClientAPI capi, string questTitle, Action onConfirm) : base(capi)
        {
            this.questTitle = questTitle;
            this.onConfirm = onConfirm;
        }

        public override void OnGuiOpened()
        {
            base.OnGuiOpened();
            SetupDialog();
        }

        private void SetupDialog()
        {
            var dialogBounds = ElementStdBounds.AutosizedMainDialog.WithAlignment(EnumDialogArea.CenterMiddle);
            var bgBounds = ElementBounds.Fill.WithFixedPadding(GuiStyle.ElementToDialogPadding);
            bgBounds.BothSizing = ElementSizing.FitToChildren;
            bgBounds.WithChildren(ElementBounds.Fixed(0, 0, 450, 200));

            SingleComposer = capi.Gui.CreateCompo("quest-delete-confirm", dialogBounds)
                .AddShadedDialogBG(bgBounds)
                .AddDialogTitleBar("Confirm Delete", OnTitleBarClose)
                .BeginChildElements(bgBounds);

            var message = $"Are you sure you want to delete the quest '{questTitle}'?\n\nThis action cannot be undone and will remove all player progress for this quest.";
            var textBounds = ElementBounds.Fixed(10, 30, 430, 120);
            SingleComposer.AddStaticText(message, CairoFont.WhiteDetailText(), textBounds);

            var confirmBounds = ElementBounds.Fixed(10, 160, 150, 30);
            var cancelBounds = ElementBounds.Fixed(170, 160, 150, 30);

            SingleComposer.AddSmallButton("Delete", OnConfirmClick, confirmBounds);
            SingleComposer.AddSmallButton("Cancel", OnCancelClick, cancelBounds);

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
