using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Cairo;
using SRGuildsAndKingdoms.src.gui.components;
using SRGuildsAndKingdoms.src.guilds;
using SRGuildsAndKingdoms.src.network;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;

namespace SRGuildsAndKingdoms.src.gui.dialogs
{
	// Token: 0x02000094 RID: 148
	[NullableContext(1)]
	[Nullable(0)]
	public class QuestDialogContent
	{
		// Token: 0x06000683 RID: 1667 RVA: 0x00030218 File Offset: 0x0002E418
		public QuestDialogContent(ICoreClientAPI capi, SRGuildsAndKingdomsModSystem modSystem, [Nullable(2)] GuildSummary currentGuild, ActionConsumable onLeaveGuild, [Nullable(2)] Action onNeedsRefresh = null, [Nullable(2)] string questType = null)
		{
			this.capi = capi;
			this.modSystem = modSystem;
			this.currentGuild = currentGuild;
			this.onLeaveGuild = onLeaveGuild;
			this.onNeedsRefresh = onNeedsRefresh;
			this.questType = questType;
			this.selectedPeriodTab = ((questType == "monthly-seasonal") ? 2 : 0);
			QuestNetworkHandler questNetHandler = modSystem.QuestNetworkHandler;
			if (questNetHandler != null)
			{
				questNetHandler.OnQuestListReceived = new Action<List<QuestDto>>(this.OnQuestListReceived);
				questNetHandler.OnProgressReceived = new Action<List<PlayerQuestProgressDto>, List<string>>(this.OnProgressReceived);
				questNetHandler.OnQuestStartResponse = new Action<QuestStartResponsePacket>(this.OnStartResponse);
				questNetHandler.OnQuestAbandonResponse = new Action<QuestAbandonResponsePacket>(this.OnAbandonResponse);
				questNetHandler.OnSubmitPreviewReceived = new Action<QuestSubmitPreviewResponsePacket>(this.OnSubmitPreviewReceived);
				questNetHandler.OnSubmitConfirmReceived = new Action<QuestSubmitConfirmResponsePacket>(this.OnSubmitConfirmReceived);
				questNetHandler.OnQuestCompleteReceived = new Action<QuestCompleteResponsePacket>(this.OnCompleteReceived);
			}
		}

		// Token: 0x06000684 RID: 1668 RVA: 0x0003031C File Offset: 0x0002E51C
		public double AddPeriodTabsToComposer(GuiComposer composer, double yPos, int magicWidth, double xOffset = 0.0, [Nullable(2)] string questType = null)
		{
			if (!this.dataLoaded)
			{
				return yPos;
			}
			object obj = questType == null || questType == "daily-weekly";
			bool showWeekly = questType == null || questType == "daily-weekly";
			bool showMonthly = true;
			bool showSeasonal = true;
			object obj2 = obj;
			int visibleTabCount = ((obj2 != 0) + (showWeekly > false) + (showMonthly > false) + (showSeasonal > false)) ? 1 : 0;
			double tabWidth = (double)(magicWidth / visibleTabCount) - 3.7;
			double tabHeight = 30.0;
			int tabIndex = 0;
			if (obj2 != null)
			{
				string dailyLabel = Lang.Get("srguildsandkingdoms:quests-tab-daily", Array.Empty<object>());
				CairoFont dailyFont = this.IsPeriodCompleted("daily") ? CairoFont.WhiteSmallText().WithColor(new double[]
				{
					0.4,
					0.9,
					0.4,
					1.0
				}) : CairoFont.WhiteSmallText();
				GuiComposerHelpers.AddToggleButton(composer, dailyLabel, dailyFont, delegate(bool on)
				{
					this.OnPeriodTabClicked(0);
				}, ElementBounds.Fixed(xOffset + (tabWidth + 5.0) * (double)tabIndex, yPos, tabWidth, tabHeight), "quest-tab-0");
				GuiComposerHelpers.GetToggleButton(composer, "quest-tab-0").SetValue(this.selectedPeriodTab == 0);
				tabIndex++;
			}
			if (showWeekly)
			{
				string weeklyLabel = Lang.Get("srguildsandkingdoms:quests-tab-weekly", Array.Empty<object>());
				CairoFont weeklyFont = this.IsPeriodCompleted("weekly") ? CairoFont.WhiteSmallText().WithColor(new double[]
				{
					0.4,
					0.9,
					0.4,
					1.0
				}) : CairoFont.WhiteSmallText();
				GuiComposerHelpers.AddToggleButton(composer, weeklyLabel, weeklyFont, delegate(bool on)
				{
					this.OnPeriodTabClicked(1);
				}, ElementBounds.Fixed(xOffset + (tabWidth + 5.0) * (double)tabIndex, yPos, tabWidth, tabHeight), "quest-tab-1");
				GuiComposerHelpers.GetToggleButton(composer, "quest-tab-1").SetValue(this.selectedPeriodTab == 1);
				tabIndex++;
			}
			if (showMonthly)
			{
				string monthlyLabel = Lang.Get("srguildsandkingdoms:quests-tab-monthly", Array.Empty<object>());
				CairoFont monthlyFont = this.IsPeriodCompleted("monthly") ? CairoFont.WhiteSmallText().WithColor(new double[]
				{
					0.4,
					0.9,
					0.4,
					1.0
				}) : CairoFont.WhiteSmallText();
				GuiComposerHelpers.AddToggleButton(composer, monthlyLabel, monthlyFont, delegate(bool on)
				{
					this.OnPeriodTabClicked(2);
				}, ElementBounds.Fixed(xOffset + (tabWidth + 5.0) * (double)tabIndex, yPos, tabWidth, tabHeight), "quest-tab-2");
				GuiComposerHelpers.GetToggleButton(composer, "quest-tab-2").SetValue(this.selectedPeriodTab == 2);
				tabIndex++;
			}
			if (showSeasonal)
			{
				string seasonalLabel = Lang.Get("srguildsandkingdoms:quests-tab-seasonal", Array.Empty<object>());
				CairoFont seasonalFont = this.IsPeriodCompleted("seasonal") ? CairoFont.WhiteSmallText().WithColor(new double[]
				{
					0.4,
					0.9,
					0.4,
					1.0
				}) : CairoFont.WhiteSmallText();
				GuiComposerHelpers.AddToggleButton(composer, seasonalLabel, seasonalFont, delegate(bool on)
				{
					this.OnPeriodTabClicked(3);
				}, ElementBounds.Fixed(xOffset + (tabWidth + 5.0) * (double)tabIndex, yPos, tabWidth, tabHeight), "quest-tab-3");
				GuiComposerHelpers.GetToggleButton(composer, "quest-tab-3").SetValue(this.selectedPeriodTab == 3);
				tabIndex++;
			}
			this.composer = composer;
			return yPos + tabHeight;
		}

		// Token: 0x06000685 RID: 1669 RVA: 0x0003061C File Offset: 0x0002E81C
		public double AddQuestContentAsElements(GuiElementContainer container, double startTop)
		{
			if (this.currentGuild == null)
			{
				return startTop;
			}
			if (!this.dataLoaded && !this.isLoading)
			{
				this.FetchQuestData();
			}
			double yPos = startTop;
			int magicWidth = 620;
			if (this.isLoading)
			{
				ElementBounds loadingBounds = ElementBounds.Fixed(0.0, yPos, (double)magicWidth, 30.0);
				container.Add(new GuiElementStaticText(this.capi, Lang.Get("srguildsandkingdoms:quests-loading", Array.Empty<object>()), 2, loadingBounds, CairoFont.WhiteSmallText()), -1);
				return yPos + 35.0;
			}
			if (!this.dataLoaded)
			{
				return yPos;
			}
			List<PlayerQuestProgressDto> filteredActiveQuests = this.GetActiveQuestsForCurrentTab();
			HashSet<int> activeQuestIds = (from q in filteredActiveQuests
			select q.QuestId).ToHashSet<int>();
			List<QuestDto> filteredAvailableQuests = (from q in this.GetQuestsForCurrentTab()
			where !activeQuestIds.Contains(q.Id) && !this.completedPeriodKeys.Contains(q.PeriodKey)
			select q).ToList<QuestDto>();
			if (filteredActiveQuests.Count == 0 && filteredAvailableQuests.Count == 0)
			{
				return this.AddNoQuestsMessage(container, yPos, magicWidth);
			}
			if (filteredActiveQuests.Count > 0)
			{
				yPos = this.RenderActiveQuestsAsElements(container, yPos, magicWidth, filteredActiveQuests);
			}
			if (filteredAvailableQuests.Count > 0)
			{
				yPos = this.RenderAvailableQuestsAsElements(container, yPos, magicWidth, filteredAvailableQuests);
			}
			return yPos;
		}

		// Token: 0x06000686 RID: 1670 RVA: 0x00030760 File Offset: 0x0002E960
		private double AddNoQuestsMessage(GuiElementContainer container, double yPos, int magicWidth)
		{
			string periodType = this.GetCurrentPeriodTypeName();
			bool flag = this.completedPeriodKeys.Any((string k) => this.GetQuestsForCurrentTab().Any((QuestDto q) => q.PeriodKey == k));
			string message = flag ? Lang.Get("srguildsandkingdoms:quests-period-already-completed", Array.Empty<object>()) : Lang.Get("srguildsandkingdoms:quests-none-" + periodType, Array.Empty<object>());
			ElementBounds messageBounds = ElementBounds.Fixed(0.0, yPos, (double)magicWidth, 30.0);
			container.Add(new GuiElementStaticText(this.capi, message, 2, messageBounds, CairoFont.WhiteSmallText()), -1);
			yPos += 25.0;
			if (flag && periodType != "seasonal")
			{
				List<QuestDto> questsForTab = this.GetQuestsForCurrentTab();
				if (questsForTab.Count > 0 && !questsForTab[0].UsesIngameTime)
				{
					string resetTimeDisplay = this.GetNextResetTimeDisplay(questsForTab[0].ExpiresAt);
					if (!string.IsNullOrEmpty(resetTimeDisplay))
					{
						ElementBounds resetBounds = ElementBounds.Fixed(0.0, yPos, (double)magicWidth, 25.0);
						container.Add(new GuiElementStaticText(this.capi, resetTimeDisplay, 2, resetBounds, CairoFont.WhiteSmallText().WithFontSize(11f).WithColor(new double[]
						{
							0.7,
							0.7,
							0.7,
							1.0
						})), -1);
						yPos += 10.0;
					}
				}
			}
			return yPos;
		}

		// Token: 0x06000687 RID: 1671 RVA: 0x000308B4 File Offset: 0x0002EAB4
		private double RenderActiveQuestsAsElements(GuiElementContainer container, double yPos, int magicWidth, List<PlayerQuestProgressDto> quests)
		{
			foreach (PlayerQuestProgressDto progressDto in quests)
			{
				yPos = this.RenderActiveQuestAsElement(container, yPos, magicWidth, progressDto);
				yPos += 5.0;
			}
			return yPos;
		}

		// Token: 0x06000688 RID: 1672 RVA: 0x00030918 File Offset: 0x0002EB18
		private double RenderActiveQuestAsElement(GuiElementContainer container, double yPos, int magicWidth, PlayerQuestProgressDto progressDto)
		{
			CairoFont descFont = CairoFont.WhiteSmallText().WithFontSize(11f).WithColor(new double[]
			{
				0.85,
				0.85,
				0.85,
				1.0
			});
			double descWidth = (double)(magicWidth - 20);
			double descHeight = this.capi.Gui.Text.GetMultilineTextHeight(descFont, progressDto.QuestDescription, descWidth * (double)RuntimeEnv.GUIScale, 0) / (double)RuntimeEnv.GUIScale;
			List<QuestObjectiveDto> objectives = progressDto.Objectives;
			int objectiveCount = (objectives != null) ? objectives.Count : 0;
			double containerHeight = 28.0 + descHeight + 3.0 + (double)(objectiveCount * 22) + 60.0 + 40.0;
			ElementBounds insetBounds = ElementBounds.Fixed(0.0, yPos, (double)magicWidth, containerHeight);
			GuiElementQuestInset inset = new GuiElementQuestInset(this.capi, insetBounds, 4, 0.85f);
			container.Add(inset, -1);
			ElementBounds titleBounds = ElementBounds.Fixed(10.0, yPos + 9.0, (double)(magicWidth - 220), 20.0);
			container.Add(new GuiElementStaticText(this.capi, progressDto.QuestTitle, 0, titleBounds, CairoFont.WhiteSmallText().WithWeight(1)), -1);
			ElementBounds activeBounds = ElementBounds.Fixed((double)(magicWidth - 110), yPos + 13.0, 100.0, 20.0);
			container.Add(new GuiElementStaticText(this.capi, Lang.Get("srguildsandkingdoms:quests-active", Array.Empty<object>()), 1, activeBounds, CairoFont.WhiteSmallText().WithFontSize(12f).WithWeight(1).WithColor(new double[]
			{
				0.89,
				0.72,
				0.04,
				1.0
			})), -1);
			ElementBounds descBounds = ElementBounds.Fixed(10.0, yPos + 28.0, descWidth, descHeight);
			container.Add(new GuiElementStaticText(this.capi, progressDto.QuestDescription, 0, descBounds, descFont), -1);
			double objYPos = yPos + 28.0 + descHeight + 9.0;
			if (progressDto.Objectives != null && progressDto.Objectives.Count > 0)
			{
				foreach (QuestObjectiveDto objective in progressDto.Objectives)
				{
					int currentProgress = progressDto.ObjectiveProgress.GetValueOrDefault(objective.Id, 0);
					objYPos = this.RenderObjectiveAsElement(container, objective, 15.0, objYPos, currentProgress, objective.Count, false);
				}
			}
			double rewardsYPos = objYPos + 4.0;
			rewardsYPos = this.RenderRewardsAsElements(container, progressDto.Rewards, 15.0, rewardsYPos);
			string expirationText = this.FormatExpirationDate(progressDto.ExpiresAt, progressDto.UsesIngameTime);
			ElementBounds expBounds = ElementBounds.Fixed(10.0, rewardsYPos + 5.0, 400.0, 20.0);
			container.Add(new GuiElementStaticText(this.capi, expirationText, 0, expBounds, CairoFont.WhiteSmallText().WithFontSize(10f).WithColor(new double[]
			{
				0.8,
				0.8,
				0.8,
				1.0
			})), -1);
			List<QuestObjectiveDto> objectives2 = progressDto.Objectives;
			bool flag = objectives2 != null && objectives2.Any((QuestObjectiveDto obj) => obj.Type.Equals("turn_in", StringComparison.OrdinalIgnoreCase) && progressDto.ObjectiveProgress.GetValueOrDefault(obj.Id, 0) < obj.Count);
			bool allObjectivesComplete = this.AreAllObjectivesComplete(progressDto);
			double buttonYPos = yPos + containerHeight - 75.0;
			if (flag)
			{
				ElementBounds submitBounds = ElementBounds.Fixed((double)(magicWidth - 200), buttonYPos, 190.0, 30.0);
				GuiElementTextButton submitButton = new GuiElementTextButton(this.capi, Lang.Get("srguildsandkingdoms:quests-submit-items", Array.Empty<object>()), CairoFont.WhiteSmallText().WithFontSize(12f).WithWeight(1), CairoFont.WhiteSmallText().WithFontSize(12f).WithWeight(1), delegate()
				{
					this.OnSubmitItemsClicked(progressDto.QuestId);
					return true;
				}, submitBounds, 2);
				container.Add(submitButton, -1);
			}
			if (allObjectivesComplete)
			{
				ElementBounds claimBounds = ElementBounds.Fixed((double)(magicWidth - 200), buttonYPos + 35.0, 190.0, 30.0);
				GuiElementTextButton claimButton = new GuiElementTextButton(this.capi, Lang.Get("srguildsandkingdoms:quests-claim-reward", Array.Empty<object>()), CairoFont.WhiteSmallText().WithFontSize(12f).WithWeight(1), CairoFont.WhiteSmallText().WithFontSize(12f).WithWeight(1), delegate()
				{
					this.OnClaimRewardClicked(progressDto.QuestId);
					return true;
				}, claimBounds, 2);
				container.Add(claimButton, -1);
			}
			else
			{
				ElementBounds abandonBounds = ElementBounds.Fixed((double)(magicWidth - 200), buttonYPos + 35.0, 190.0, 30.0);
				GuiElementTextButton abandonButton = new GuiElementTextButton(this.capi, Lang.Get("srguildsandkingdoms:quests-abandon", Array.Empty<object>()), CairoFont.WhiteSmallText().WithFontSize(12f).WithWeight(1), CairoFont.WhiteSmallText().WithFontSize(12f).WithWeight(1), delegate()
				{
					this.OnAbandonQuestClicked(progressDto.QuestId);
					return true;
				}, abandonBounds, 2);
				container.Add(abandonButton, -1);
			}
			return yPos + containerHeight;
		}

		// Token: 0x06000689 RID: 1673 RVA: 0x00030E88 File Offset: 0x0002F088
		private double RenderAvailableQuestsAsElements(GuiElementContainer container, double yPos, int magicWidth, List<QuestDto> quests)
		{
			foreach (QuestDto quest in quests)
			{
				yPos = this.RenderAvailableQuestAsElement(container, yPos, magicWidth, quest);
				yPos += 5.0;
			}
			return yPos;
		}

		// Token: 0x0600068A RID: 1674 RVA: 0x00030EEC File Offset: 0x0002F0EC
		private double RenderAvailableQuestAsElement(GuiElementContainer container, double yPos, int magicWidth, QuestDto quest)
		{
			bool flag = this.completedPeriodKeys.Contains(quest.PeriodKey);
			bool isPeriodActive = this.activeQuestProgress.Any((PlayerQuestProgressDto p) => p.PeriodKey == quest.PeriodKey);
			bool isPeriodLocked = flag || isPeriodActive;
			CairoFont descFont = isPeriodLocked ? CairoFont.WhiteSmallText().WithFontSize(11f).WithColor(new double[]
			{
				0.6,
				0.6,
				0.6,
				1.0
			}) : CairoFont.WhiteSmallText().WithFontSize(11f).WithColor(new double[]
			{
				0.85,
				0.85,
				0.85,
				1.0
			});
			double descWidth = (double)(magicWidth - 20);
			double descHeight = this.capi.Gui.Text.GetMultilineTextHeight(descFont, quest.Description, descWidth * (double)RuntimeEnv.GUIScale, 0) / (double)RuntimeEnv.GUIScale;
			List<QuestObjectiveDto> objectives = quest.Objectives;
			int objectiveCount = (objectives != null) ? objectives.Count : 0;
			double containerHeight = 28.0 + descHeight + 3.0 + (double)(objectiveCount * 22) + 60.0 + 40.0;
			ElementBounds insetBounds = ElementBounds.Fixed(0.0, yPos, (double)magicWidth, containerHeight);
			GuiElementQuestInset inset = new GuiElementQuestInset(this.capi, insetBounds, 4, 0.85f);
			container.Add(inset, -1);
			CairoFont titleFont = isPeriodLocked ? CairoFont.WhiteSmallText().WithColor(new double[]
			{
				0.6,
				0.6,
				0.6,
				1.0
			}) : CairoFont.WhiteSmallText().WithWeight(1);
			ElementBounds titleBounds = ElementBounds.Fixed(10.0, yPos + 9.0, (double)(magicWidth - 220), 20.0);
			container.Add(new GuiElementStaticText(this.capi, quest.Title, 0, titleBounds, titleFont), -1);
			ElementBounds descBounds = ElementBounds.Fixed(10.0, yPos + 28.0, descWidth, descHeight);
			container.Add(new GuiElementStaticText(this.capi, quest.Description, 0, descBounds, descFont), -1);
			double objYPos = yPos + 28.0 + descHeight + 9.0;
			if (quest.Objectives != null && quest.Objectives.Count > 0)
			{
				foreach (QuestObjectiveDto objective in quest.Objectives)
				{
					objYPos = this.RenderObjectiveAsElement(container, objective, 15.0, objYPos, 0, objective.Count, true);
				}
			}
			double rewardsYPos = objYPos + 4.0;
			rewardsYPos = this.RenderRewardsAsElements(container, quest.Rewards, 15.0, rewardsYPos);
			string expirationText = this.FormatExpirationDate(quest.ExpiresAt, quest.UsesIngameTime);
			ElementBounds expBounds = ElementBounds.Fixed(10.0, rewardsYPos + 5.0, (double)(magicWidth - 220), 20.0);
			container.Add(new GuiElementStaticText(this.capi, expirationText, 0, expBounds, CairoFont.WhiteSmallText().WithFontSize(10f).WithColor(new double[]
			{
				0.8,
				0.8,
				0.8,
				1.0
			})), -1);
			bool alreadyCompletedLastWeek = quest.AlreadyCompletedLastWeek;
			double buttonYPos = yPos + containerHeight - 40.0;
			bool needsMonthlyBoard = (quest.RecurrenceType.Equals("monthly", StringComparison.OrdinalIgnoreCase) || quest.RecurrenceType.Equals("seasonal", StringComparison.OrdinalIgnoreCase)) && this.questType == "daily-weekly";
			bool needsDailyBoard = (quest.RecurrenceType.Equals("daily", StringComparison.OrdinalIgnoreCase) || quest.RecurrenceType.Equals("weekly", StringComparison.OrdinalIgnoreCase)) && this.questType == "monthly-seasonal";
			string buttonText = Lang.Get("srguildsandkingdoms:quests-start", Array.Empty<object>());
			double buttonWidth = 80.0;
			if (needsMonthlyBoard)
			{
				buttonText = Lang.Get("srguildsandkingdoms:quests-needs-monthly-board", Array.Empty<object>());
				buttonWidth = 165.0;
			}
			if (needsDailyBoard)
			{
				buttonText = Lang.Get("srguildsandkingdoms:quests-needs-daily-board", Array.Empty<object>());
				buttonWidth = 165.0;
			}
			if (alreadyCompletedLastWeek)
			{
				buttonText = Lang.Get("srguildsandkingdoms:quests-recently-completed", Array.Empty<object>());
				buttonWidth = 150.0;
			}
			if (isPeriodActive)
			{
				buttonText = Lang.Get("srguildsandkingdoms:quests-period-in-progress", Array.Empty<object>());
				buttonWidth = 120.0;
			}
			ElementBounds startBounds = ElementBounds.Fixed((double)magicWidth - (buttonWidth + 10.0), buttonYPos, buttonWidth, 30.0);
			GuiElementTextButton startButton = new GuiElementTextButton(this.capi, buttonText, CairoFont.WhiteSmallText().WithFontSize(12f).WithWeight(1), CairoFont.WhiteSmallText().WithFontSize(12f).WithWeight(1), delegate()
			{
				this.OnStartQuestClicked(quest.Id);
				return true;
			}, startBounds, 2);
			if (isPeriodLocked || alreadyCompletedLastWeek || needsMonthlyBoard || needsDailyBoard)
			{
				startButton.Enabled = false;
			}
			container.Add(startButton, -1);
			return yPos + containerHeight;
		}

		// Token: 0x0600068B RID: 1675 RVA: 0x0003142C File Offset: 0x0002F62C
		private double RenderObjectiveAsElement(GuiElementContainer container, QuestObjectiveDto objective, double xPos, double yPos, int currentProgress, int requiredCount, bool hideProgress = false)
		{
			double currentX = xPos;
			List<ItemStack> itemStacks = new List<ItemStack>();
			string a = objective.Type.ToLower();
			string actionText;
			double actionWidth;
			if (!(a == "kill"))
			{
				if (!(a == "turn_in"))
				{
					if (a == "craft")
					{
						goto IL_149;
					}
					if (!(a == "harvest"))
					{
						goto IL_219;
					}
					goto IL_1B4;
				}
			}
			else
			{
				actionText = "• Kill";
				actionWidth = 30.0;
				if (objective.AcceptedTargets == null)
				{
					goto IL_229;
				}
				using (List<string>.Enumerator enumerator = objective.AcceptedTargets.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						string text = enumerator.Current;
						string creatureCode = text.Replace(":", ":creature-") ?? "";
						ItemStack stack = this.CreateItemStackFromCode(creatureCode, objective.Count);
						if (stack != null)
						{
							itemStacks.Add(stack);
						}
					}
					goto IL_229;
				}
			}
			actionText = "• Turn in";
			actionWidth = 45.0;
			if (objective.AcceptedItems == null)
			{
				goto IL_229;
			}
			using (List<QuestAcceptedItemDto>.Enumerator enumerator2 = objective.AcceptedItems.GetEnumerator())
			{
				while (enumerator2.MoveNext())
				{
					QuestAcceptedItemDto acceptedItem = enumerator2.Current;
					ItemStack stack2 = this.CreateItemStackFromAcceptedItem(acceptedItem, objective.Count);
					if (stack2 != null)
					{
						itemStacks.Add(stack2);
					}
				}
				goto IL_229;
			}
			IL_149:
			actionText = "• Craft";
			actionWidth = 33.0;
			if (objective.AcceptedItems == null)
			{
				goto IL_229;
			}
			using (List<QuestAcceptedItemDto>.Enumerator enumerator2 = objective.AcceptedItems.GetEnumerator())
			{
				while (enumerator2.MoveNext())
				{
					QuestAcceptedItemDto acceptedItem2 = enumerator2.Current;
					ItemStack stack3 = this.CreateItemStackFromAcceptedItem(acceptedItem2, objective.Count);
					if (stack3 != null)
					{
						itemStacks.Add(stack3);
					}
				}
				goto IL_229;
			}
			IL_1B4:
			actionText = "• Harvest";
			actionWidth = 47.0;
			if (objective.AcceptedItems == null)
			{
				goto IL_229;
			}
			using (List<QuestAcceptedItemDto>.Enumerator enumerator2 = objective.AcceptedItems.GetEnumerator())
			{
				while (enumerator2.MoveNext())
				{
					QuestAcceptedItemDto acceptedItem3 = enumerator2.Current;
					ItemStack stack4 = this.CreateItemStackFromAcceptedItem(acceptedItem3, objective.Count);
					if (stack4 != null)
					{
						itemStacks.Add(stack4);
					}
				}
				goto IL_229;
			}
			IL_219:
			actionText = "•";
			actionWidth = 15.0;
			IL_229:
			CairoFont actionFont = (currentProgress >= requiredCount) ? CairoFont.WhiteSmallText().WithFontSize(10f).WithColor(new double[]
			{
				0.89,
				0.72,
				0.04,
				1.0
			}) : CairoFont.WhiteSmallText().WithFontSize(10f).WithColor(new double[]
			{
				0.75,
				0.75,
				0.75,
				1.0
			});
			ElementBounds actionBounds = ElementBounds.Fixed(currentX, yPos + 3.0, actionWidth, 16.0);
			container.Add(new GuiElementStaticText(this.capi, actionText, 0, actionBounds, actionFont), -1);
			currentX += actionWidth;
			if (itemStacks.Count > 0)
			{
				ElementBounds itemBounds = ElementBounds.Fixed(currentX, yPos + 3.0, 15.0, 15.0);
				if (itemStacks.Count == 1)
				{
					GuiElementItemstackDisplay element = new GuiElementItemstackDisplay(this.capi, itemStacks[0], itemBounds)
					{
						ShowStackSize = false
					};
					container.Add(element, -1);
				}
				else
				{
					GuiElementCyclingItemDisplay cyclingElement = new GuiElementCyclingItemDisplay(this.capi, itemStacks, itemBounds)
					{
						ShowStackSize = false
					};
					container.Add(cyclingElement, -1);
				}
				currentX += 20.0;
			}
			string text2;
			if (!hideProgress)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(3, 2);
				defaultInterpolatedStringHandler.AppendLiteral("(");
				defaultInterpolatedStringHandler.AppendFormatted<int>(currentProgress);
				defaultInterpolatedStringHandler.AppendLiteral("/");
				defaultInterpolatedStringHandler.AppendFormatted<int>(requiredCount);
				defaultInterpolatedStringHandler.AppendLiteral(")");
				text2 = defaultInterpolatedStringHandler.ToStringAndClear();
			}
			else
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(2, 1);
				defaultInterpolatedStringHandler2.AppendLiteral("(");
				defaultInterpolatedStringHandler2.AppendFormatted<int>(requiredCount);
				defaultInterpolatedStringHandler2.AppendLiteral(")");
				text2 = defaultInterpolatedStringHandler2.ToStringAndClear();
			}
			string progressText = text2;
			CairoFont progressFont = (currentProgress >= requiredCount) ? CairoFont.WhiteSmallText().WithFontSize(10f).WithColor(new double[]
			{
				0.89,
				0.72,
				0.04,
				1.0
			}) : CairoFont.WhiteSmallText().WithFontSize(10f).WithColor(new double[]
			{
				0.6,
				0.6,
				0.6,
				1.0
			});
			ElementBounds progressBounds = ElementBounds.Fixed(currentX, yPos + 3.0, 60.0, 16.0);
			container.Add(new GuiElementStaticText(this.capi, progressText, 0, progressBounds, progressFont), -1);
			return yPos + 22.0;
		}

		// Token: 0x0600068C RID: 1676 RVA: 0x000318D4 File Offset: 0x0002FAD4
		private double RenderRewardsAsElements(GuiElementContainer container, List<QuestRewardDto> rewards, double xPos, double yPos)
		{
			if (rewards == null || rewards.Count == 0)
			{
				return yPos;
			}
			double currentX = xPos - 5.0;
			CairoFont labelFont = CairoFont.WhiteSmallishText().WithFontSize(13f).WithColor(new double[]
			{
				0.75,
				0.75,
				0.75,
				1.0
			});
			string labelText = "Rewards";
			TextExtents labelExtents = labelFont.GetTextExtents(labelText);
			ElementBounds labelBounds = ElementBounds.Fixed(currentX, yPos + 5.0, labelExtents.Width, 20.0);
			container.Add(new GuiElementStaticText(this.capi, labelText, 0, labelBounds, labelFont), -1);
			yPos += 30.0;
			currentX += 5.0;
			foreach (QuestRewardDto reward in rewards)
			{
				ItemStack itemstack = this.CreateItemStackFromReward(reward);
				if (itemstack != null)
				{
					ElementBounds itemBounds = ElementBounds.Fixed(currentX, yPos - 2.0, 20.0, 20.0);
					GuiElementItemstackDisplay element = new GuiElementItemstackDisplay(this.capi, itemstack, itemBounds)
					{
						ShowStackSize = true
					};
					container.Add(element, -1);
					currentX += 33.0;
				}
			}
			return yPos + 30.0;
		}

		// Token: 0x0600068D RID: 1677 RVA: 0x00031A30 File Offset: 0x0002FC30
		private bool IsPeriodCompleted(string periodType)
		{
			if (this.activeQuestProgress.Any((PlayerQuestProgressDto p) => p.RecurrenceType.Equals(periodType, StringComparison.OrdinalIgnoreCase)))
			{
				return false;
			}
			List<QuestDto> questsForPeriod = (from q in this.availableQuests
			where q.RecurrenceType.Equals(periodType, StringComparison.OrdinalIgnoreCase)
			select q).ToList<QuestDto>();
			return questsForPeriod.Count == 0 || questsForPeriod.All((QuestDto q) => this.completedPeriodKeys.Contains(q.PeriodKey));
		}

		// Token: 0x0600068E RID: 1678 RVA: 0x00031AA8 File Offset: 0x0002FCA8
		private void OnPeriodTabClicked(int tabIndex)
		{
			if (this.composer == null)
			{
				return;
			}
			if (this.selectedPeriodTab == tabIndex)
			{
				GuiComposer guiComposer = this.composer;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(10, 1);
				defaultInterpolatedStringHandler.AppendLiteral("quest-tab-");
				defaultInterpolatedStringHandler.AppendFormatted<int>(tabIndex);
				GuiComposerHelpers.GetToggleButton(guiComposer, defaultInterpolatedStringHandler.ToStringAndClear()).SetValue(true);
				return;
			}
			this.selectedPeriodTab = tabIndex;
			for (int i = 0; i < 4; i++)
			{
				GuiComposer guiComposer2 = this.composer;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(10, 1);
				defaultInterpolatedStringHandler2.AppendLiteral("quest-tab-");
				defaultInterpolatedStringHandler2.AppendFormatted<int>(i);
				GuiElementToggleButton toggleButton = GuiComposerHelpers.GetToggleButton(guiComposer2, defaultInterpolatedStringHandler2.ToStringAndClear());
				if (toggleButton != null)
				{
					toggleButton.SetValue(i == this.selectedPeriodTab);
				}
			}
			Action action = this.onNeedsRefresh;
			if (action == null)
			{
				return;
			}
			action();
		}

		// Token: 0x0600068F RID: 1679 RVA: 0x00031B68 File Offset: 0x0002FD68
		private List<QuestDto> GetQuestsForCurrentTab()
		{
			string periodType = this.GetCurrentPeriodTypeName();
			return (from q in this.availableQuests
			where q.RecurrenceType.Equals(periodType, StringComparison.OrdinalIgnoreCase)
			select q).ToList<QuestDto>();
		}

		// Token: 0x06000690 RID: 1680 RVA: 0x00031BA4 File Offset: 0x0002FDA4
		private List<PlayerQuestProgressDto> GetActiveQuestsForCurrentTab()
		{
			string periodType = this.GetCurrentPeriodTypeName();
			return (from p in this.activeQuestProgress
			where p.RecurrenceType.Equals(periodType, StringComparison.OrdinalIgnoreCase)
			select p).ToList<PlayerQuestProgressDto>();
		}

		// Token: 0x06000691 RID: 1681 RVA: 0x00031BE0 File Offset: 0x0002FDE0
		private string GetCurrentPeriodTypeName()
		{
			string result;
			switch (this.selectedPeriodTab)
			{
			case 0:
				result = "daily";
				break;
			case 1:
				result = "weekly";
				break;
			case 2:
				result = "monthly";
				break;
			case 3:
				result = "seasonal";
				break;
			default:
				result = "daily";
				break;
			}
			return result;
		}

		// Token: 0x06000692 RID: 1682 RVA: 0x00031C34 File Offset: 0x0002FE34
		[return: Nullable(2)]
		private ItemStack CreateItemStackFromCode(string code, int quantity)
		{
			ItemStack result;
			try
			{
				Item item = this.capi.World.GetItem(new AssetLocation(code));
				if (item != null)
				{
					result = new ItemStack(item, quantity);
				}
				else
				{
					Block block = this.capi.World.GetBlock(new AssetLocation(code));
					if (block != null)
					{
						result = new ItemStack(block, quantity);
					}
					else
					{
						result = null;
					}
				}
			}
			catch
			{
				result = null;
			}
			return result;
		}

		// Token: 0x06000693 RID: 1683 RVA: 0x00031CA4 File Offset: 0x0002FEA4
		[return: Nullable(2)]
		private ItemStack CreateItemStackFromAcceptedItem(QuestAcceptedItemDto acceptedItem, int quantity)
		{
			ItemStack result;
			try
			{
				Item item = this.capi.World.GetItem(new AssetLocation(acceptedItem.Code));
				if (item == null)
				{
					Block block = this.capi.World.GetBlock(new AssetLocation(acceptedItem.Code));
					if (block == null)
					{
						result = null;
					}
					else
					{
						ItemStack blockStack = new ItemStack(block, quantity);
						if (!string.IsNullOrEmpty(acceptedItem.Nbt))
						{
							using (MemoryStream ms = new MemoryStream(Convert.FromBase64String(acceptedItem.Nbt)))
							{
								using (BinaryReader reader = new BinaryReader(ms))
								{
									blockStack.Attributes = new TreeAttribute();
									blockStack.Attributes.FromBytes(reader);
								}
							}
						}
						result = blockStack;
					}
				}
				else
				{
					ItemStack itemStack = new ItemStack(item, quantity);
					if (!string.IsNullOrEmpty(acceptedItem.Nbt))
					{
						using (MemoryStream ms2 = new MemoryStream(Convert.FromBase64String(acceptedItem.Nbt)))
						{
							using (BinaryReader reader2 = new BinaryReader(ms2))
							{
								itemStack.Attributes = new TreeAttribute();
								itemStack.Attributes.FromBytes(reader2);
							}
						}
					}
					result = itemStack;
				}
			}
			catch
			{
				result = null;
			}
			return result;
		}

		// Token: 0x06000694 RID: 1684 RVA: 0x00031E50 File Offset: 0x00030050
		[return: Nullable(2)]
		private ItemStack CreateItemStackFromReward(QuestRewardDto reward)
		{
			ItemStack result;
			try
			{
				if (reward.Code == "game:grspoints")
				{
					Item parchmentItem = this.capi.World.GetItem(new AssetLocation("game:paper-parchment"));
					if (parchmentItem == null)
					{
						result = null;
					}
					else
					{
						ItemStack itemStack2 = new ItemStack(parchmentItem, reward.Amount);
						itemStack2.Attributes = new TreeAttribute();
						itemStack2.Attributes.SetString("title", "GRS Points");
						itemStack2.Attributes.SetString("text", "");
						itemStack2.Attributes.SetString("signedby", "The Shadow Realm");
						result = itemStack2;
					}
				}
				else
				{
					Item item = this.capi.World.GetItem(new AssetLocation(reward.Code));
					if (item == null)
					{
						Block block = this.capi.World.GetBlock(new AssetLocation(reward.Code));
						if (block == null)
						{
							result = null;
						}
						else
						{
							ItemStack blockStack = new ItemStack(block, reward.Amount);
							if (!string.IsNullOrEmpty(reward.Nbt))
							{
								using (MemoryStream ms = new MemoryStream(Convert.FromBase64String(reward.Nbt)))
								{
									using (BinaryReader reader = new BinaryReader(ms))
									{
										blockStack.Attributes = new TreeAttribute();
										blockStack.Attributes.FromBytes(reader);
									}
								}
							}
							result = blockStack;
						}
					}
					else
					{
						ItemStack itemStack = new ItemStack(item, reward.Amount);
						if (!string.IsNullOrEmpty(reward.Nbt))
						{
							using (MemoryStream ms2 = new MemoryStream(Convert.FromBase64String(reward.Nbt)))
							{
								using (BinaryReader reader2 = new BinaryReader(ms2))
								{
									itemStack.Attributes = new TreeAttribute();
									itemStack.Attributes.FromBytes(reader2);
								}
							}
						}
						result = itemStack;
					}
				}
			}
			catch (Exception ex)
			{
				this.capi.Logger.Error("[QuestDialogContent] Failed to create itemstack for reward " + reward.Code + ": " + ex.Message);
				result = null;
			}
			return result;
		}

		// Token: 0x06000695 RID: 1685 RVA: 0x000320D0 File Offset: 0x000302D0
		private string FormatExpirationDate(string expiresAt, bool usesIngameTime)
		{
			if (string.IsNullOrEmpty(expiresAt))
			{
				return "Expires: Unknown";
			}
			string result;
			try
			{
				string[] parts = expiresAt.Split('-', StringSplitOptions.None);
				if (parts.Length != 3)
				{
					result = "Expires: " + expiresAt;
				}
				else
				{
					int year = int.Parse(parts[0]);
					int month = int.Parse(parts[1]);
					int day = int.Parse(parts[2]);
					int dateTimeYear = (year == 0) ? 1 : year;
					DateTime date = new DateTime(dateTimeYear, month, day);
					string monthName = date.ToString("MMMM");
					if (usesIngameTime)
					{
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(16, 3);
						defaultInterpolatedStringHandler.AppendLiteral("Ends on ");
						defaultInterpolatedStringHandler.AppendFormatted(monthName);
						defaultInterpolatedStringHandler.AppendLiteral(" ");
						defaultInterpolatedStringHandler.AppendFormatted<int>(day);
						defaultInterpolatedStringHandler.AppendLiteral(", Year ");
						defaultInterpolatedStringHandler.AppendFormatted<int>(year);
						result = defaultInterpolatedStringHandler.ToStringAndClear();
					}
					else
					{
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(11, 3);
						defaultInterpolatedStringHandler2.AppendLiteral("Ends on ");
						defaultInterpolatedStringHandler2.AppendFormatted(monthName);
						defaultInterpolatedStringHandler2.AppendLiteral(" ");
						defaultInterpolatedStringHandler2.AppendFormatted<int>(day);
						defaultInterpolatedStringHandler2.AppendLiteral(", ");
						defaultInterpolatedStringHandler2.AppendFormatted<int>(year);
						result = defaultInterpolatedStringHandler2.ToStringAndClear();
					}
				}
			}
			catch
			{
				result = "Expires: " + expiresAt;
			}
			return result;
		}

		// Token: 0x06000696 RID: 1686 RVA: 0x00032220 File Offset: 0x00030420
		private string GetNextResetTimeDisplay(string expiresAt)
		{
			string result;
			try
			{
				DateTime expirationDate;
				if (!DateTime.TryParseExact(expiresAt, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out expirationDate))
				{
					result = "";
				}
				else
				{
					DateTime nextResetDateEst = expirationDate.AddDays(1.0);
					TimeZoneInfo easternZone;
					try
					{
						easternZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
					}
					catch (TimeZoneNotFoundException)
					{
						easternZone = TimeZoneInfo.FindSystemTimeZoneById("America/New_York");
					}
					DateTime resetTimeLocal = TimeZoneInfo.ConvertTimeFromUtc(TimeZoneInfo.ConvertTimeToUtc(new DateTime(nextResetDateEst.Year, nextResetDateEst.Month, nextResetDateEst.Day, 0, 0, 0, DateTimeKind.Unspecified), easternZone), TimeZoneInfo.Local);
					string tzAbbreviation = QuestDialogContent.GetTimezoneAbbreviation(TimeZoneInfo.Local, resetTimeLocal);
					string text = "srguildsandkingdoms:quests-next-reset";
					object[] array = new object[1];
					int num = 0;
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(1, 2);
					defaultInterpolatedStringHandler.AppendFormatted<DateTime>(resetTimeLocal, "MMMM d, yyyy h:mm tt");
					defaultInterpolatedStringHandler.AppendLiteral(" ");
					defaultInterpolatedStringHandler.AppendFormatted(tzAbbreviation);
					array[num] = defaultInterpolatedStringHandler.ToStringAndClear();
					result = Lang.Get(text, array);
				}
			}
			catch
			{
				result = "";
			}
			return result;
		}

		// Token: 0x06000697 RID: 1687 RVA: 0x00032328 File Offset: 0x00030528
		private static string GetTimezoneAbbreviation(TimeZoneInfo timeZone, DateTime dateTime)
		{
			string[] words = (timeZone.IsDaylightSavingTime(dateTime) ? timeZone.DaylightName : timeZone.StandardName).Split(' ', StringSplitOptions.RemoveEmptyEntries);
			if (words.Length > 1)
			{
				return string.Concat<char>(from w in words
				select char.ToUpper(w[0]));
			}
			TimeSpan offset = timeZone.GetUtcOffset(dateTime);
			string sign = (offset >= TimeSpan.Zero) ? "+" : "";
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(3, 2);
			defaultInterpolatedStringHandler.AppendLiteral("UTC");
			defaultInterpolatedStringHandler.AppendFormatted(sign);
			defaultInterpolatedStringHandler.AppendFormatted<int>(offset.Hours, "0");
			return defaultInterpolatedStringHandler.ToStringAndClear();
		}

		// Token: 0x06000698 RID: 1688 RVA: 0x000323E0 File Offset: 0x000305E0
		private void FetchQuestData()
		{
			QuestNetworkHandler questNetHandler = this.modSystem.QuestNetworkHandler;
			if (questNetHandler == null)
			{
				this.capi.Logger.Warning("[QuestDialogContent] QuestNetworkHandler is null!");
				return;
			}
			IClientPlayer player = this.capi.World.Player;
			string playerUid = (player != null) ? player.PlayerUID : null;
			if (string.IsNullOrEmpty(playerUid))
			{
				this.capi.Logger.Warning("[QuestDialogContent] Player UID is null or empty!");
				return;
			}
			this.capi.Logger.Notification("[QuestDialogContent] Fetching quest data for player " + playerUid);
			this.isLoading = true;
			this.questListReceived = false;
			this.progressReceived = false;
			questNetHandler.RequestQuestList(playerUid);
			questNetHandler.RequestQuestProgress(playerUid);
		}

		// Token: 0x06000699 RID: 1689 RVA: 0x0003248C File Offset: 0x0003068C
		private bool AreAllObjectivesComplete(PlayerQuestProgressDto progressDto)
		{
			if (progressDto.Objectives == null || progressDto.Objectives.Count == 0)
			{
				return false;
			}
			foreach (QuestObjectiveDto objective in progressDto.Objectives)
			{
				if (progressDto.ObjectiveProgress.GetValueOrDefault(objective.Id, 0) < objective.Count)
				{
					return false;
				}
			}
			return true;
		}

		// Token: 0x0600069A RID: 1690 RVA: 0x00032510 File Offset: 0x00030710
		private void OnStartQuestClicked(int questId)
		{
			QuestNetworkHandler questNetHandler = this.modSystem.QuestNetworkHandler;
			if (questNetHandler == null)
			{
				return;
			}
			IClientPlayer player = this.capi.World.Player;
			string playerUid = (player != null) ? player.PlayerUID : null;
			if (string.IsNullOrEmpty(playerUid))
			{
				return;
			}
			questNetHandler.RequestStartQuest(playerUid, questId);
		}

		// Token: 0x0600069B RID: 1691 RVA: 0x0003255C File Offset: 0x0003075C
		private void OnAbandonQuestClicked(int questId)
		{
			QuestDialogContent.<>c__DisplayClass39_0 CS$<>8__locals1 = new QuestDialogContent.<>c__DisplayClass39_0();
			CS$<>8__locals1.questId = questId;
			CS$<>8__locals1.questNetHandler = this.modSystem.QuestNetworkHandler;
			if (CS$<>8__locals1.questNetHandler == null)
			{
				return;
			}
			QuestDialogContent.<>c__DisplayClass39_0 CS$<>8__locals2 = CS$<>8__locals1;
			IClientPlayer player = this.capi.World.Player;
			CS$<>8__locals2.playerUid = ((player != null) ? player.PlayerUID : null);
			if (string.IsNullOrEmpty(CS$<>8__locals1.playerUid))
			{
				return;
			}
			PlayerQuestProgressDto playerQuestProgressDto = this.activeQuestProgress.FirstOrDefault((PlayerQuestProgressDto p) => p.QuestId == CS$<>8__locals1.questId);
			string text;
			if ((text = ((playerQuestProgressDto != null) ? playerQuestProgressDto.QuestTitle : null)) == null)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(7, 1);
				defaultInterpolatedStringHandler.AppendLiteral("Quest #");
				defaultInterpolatedStringHandler.AppendFormatted<int>(CS$<>8__locals1.questId);
				text = defaultInterpolatedStringHandler.ToStringAndClear();
			}
			string questTitle = text;
			new ConfirmAbandonDialog(this.capi, Lang.Get("srguildsandkingdoms:quests-abandon-confirm-message", new object[]
			{
				questTitle
			}), delegate
			{
				CS$<>8__locals1.questNetHandler.RequestAbandonQuest(CS$<>8__locals1.playerUid, CS$<>8__locals1.questId);
			}).TryOpen();
		}

		// Token: 0x0600069C RID: 1692 RVA: 0x00032644 File Offset: 0x00030844
		private void OnSubmitItemsClicked(int questId)
		{
			QuestNetworkHandler questNetHandler = this.modSystem.QuestNetworkHandler;
			if (questNetHandler == null)
			{
				return;
			}
			IClientPlayer player = this.capi.World.Player;
			string playerUid = (player != null) ? player.PlayerUID : null;
			if (string.IsNullOrEmpty(playerUid))
			{
				return;
			}
			questNetHandler.RequestSubmitPreview(playerUid, questId);
		}

		// Token: 0x0600069D RID: 1693 RVA: 0x00032690 File Offset: 0x00030890
		private void OnClaimRewardClicked(int questId)
		{
			QuestNetworkHandler questNetHandler = this.modSystem.QuestNetworkHandler;
			if (questNetHandler == null)
			{
				return;
			}
			IClientPlayer player = this.capi.World.Player;
			string playerUid = (player != null) ? player.PlayerUID : null;
			if (string.IsNullOrEmpty(playerUid))
			{
				return;
			}
			questNetHandler.RequestCompleteQuest(playerUid, questId);
		}

		// Token: 0x0600069E RID: 1694 RVA: 0x000326DC File Offset: 0x000308DC
		private void OnQuestListReceived(List<QuestDto> quests)
		{
			ILogger logger = this.capi.Logger;
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(49, 1);
			defaultInterpolatedStringHandler.AppendLiteral("[QuestDialogContent] Received ");
			defaultInterpolatedStringHandler.AppendFormatted<int>(quests.Count);
			defaultInterpolatedStringHandler.AppendLiteral(" quests from server");
			logger.Notification(defaultInterpolatedStringHandler.ToStringAndClear());
			this.availableQuests = quests;
			this.questListReceived = true;
			if (this.questListReceived && this.progressReceived)
			{
				this.isLoading = false;
				this.dataLoaded = true;
				Action action = this.onNeedsRefresh;
				if (action == null)
				{
					return;
				}
				action();
			}
		}

		// Token: 0x0600069F RID: 1695 RVA: 0x0003276C File Offset: 0x0003096C
		private void OnProgressReceived(List<PlayerQuestProgressDto> progress, List<string> completedKeys)
		{
			ILogger logger = this.capi.Logger;
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(68, 2);
			defaultInterpolatedStringHandler.AppendLiteral("[QuestDialogContent] Received ");
			defaultInterpolatedStringHandler.AppendFormatted<int>(progress.Count);
			defaultInterpolatedStringHandler.AppendLiteral(" active quests, ");
			defaultInterpolatedStringHandler.AppendFormatted<int>(completedKeys.Count);
			defaultInterpolatedStringHandler.AppendLiteral(" completed period keys");
			logger.Notification(defaultInterpolatedStringHandler.ToStringAndClear());
			this.activeQuestProgress = progress;
			this.completedPeriodKeys = completedKeys;
			this.progressReceived = true;
			if (this.questListReceived && this.progressReceived)
			{
				this.isLoading = false;
				this.dataLoaded = true;
				Action action = this.onNeedsRefresh;
				if (action == null)
				{
					return;
				}
				action();
			}
		}

		// Token: 0x060006A0 RID: 1696 RVA: 0x0003281C File Offset: 0x00030A1C
		private void OnStartResponse(QuestStartResponsePacket response)
		{
			if (response.Success)
			{
				this.capi.ShowChatMessage(Lang.Get("srguildsandkingdoms:quests-started-success", Array.Empty<object>()));
				this.FetchQuestData();
				return;
			}
			this.capi.ShowChatMessage(Lang.Get("srguildsandkingdoms:quests-started-failed", new object[]
			{
				response.Message
			}));
		}

		// Token: 0x060006A1 RID: 1697 RVA: 0x00032878 File Offset: 0x00030A78
		private void OnAbandonResponse(QuestAbandonResponsePacket response)
		{
			if (response.Success)
			{
				this.capi.ShowChatMessage(Lang.Get("srguildsandkingdoms:quests-abandoned", Array.Empty<object>()));
				this.FetchQuestData();
				return;
			}
			this.capi.ShowChatMessage(Lang.Get("srguildsandkingdoms:quests-abandon-failed", new object[]
			{
				response.Message
			}));
		}

		// Token: 0x060006A2 RID: 1698 RVA: 0x000328D4 File Offset: 0x00030AD4
		private void OnSubmitPreviewReceived(QuestSubmitPreviewResponsePacket response)
		{
			QuestDialogContent.<>c__DisplayClass46_0 CS$<>8__locals1 = new QuestDialogContent.<>c__DisplayClass46_0();
			CS$<>8__locals1.response = response;
			if (!CS$<>8__locals1.response.Success)
			{
				this.capi.ShowChatMessage(Lang.Get("srguildsandkingdoms:quests-submit-failed", new object[]
				{
					CS$<>8__locals1.response.Message
				}));
				return;
			}
			List<string> lines = new List<string>();
			foreach (QuestSubmittableItem item in CS$<>8__locals1.response.Items)
			{
				List<string> list = lines;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(2, 2);
				defaultInterpolatedStringHandler.AppendFormatted(item.DisplayName);
				defaultInterpolatedStringHandler.AppendLiteral(" x");
				defaultInterpolatedStringHandler.AppendFormatted<int>(item.Quantity);
				list.Add(defaultInterpolatedStringHandler.ToStringAndClear());
			}
			CS$<>8__locals1.questNetHandler = this.modSystem.QuestNetworkHandler;
			if (CS$<>8__locals1.questNetHandler == null)
			{
				return;
			}
			QuestDialogContent.<>c__DisplayClass46_0 CS$<>8__locals2 = CS$<>8__locals1;
			IClientPlayer player = this.capi.World.Player;
			CS$<>8__locals2.playerUid = ((player != null) ? player.PlayerUID : null);
			if (string.IsNullOrEmpty(CS$<>8__locals1.playerUid))
			{
				return;
			}
			new ConfirmSubmitDialog(this.capi, Lang.Get("srguildsandkingdoms:quests-submit-confirm-message", Array.Empty<object>()), lines, delegate
			{
				CS$<>8__locals1.questNetHandler.ConfirmSubmit(CS$<>8__locals1.playerUid, CS$<>8__locals1.response.QuestId, CS$<>8__locals1.response.Items);
			}).TryOpen();
		}

		// Token: 0x060006A3 RID: 1699 RVA: 0x00032A24 File Offset: 0x00030C24
		private void OnSubmitConfirmReceived(QuestSubmitConfirmResponsePacket response)
		{
			if (response.Success)
			{
				this.capi.ShowChatMessage(Lang.Get("srguildsandkingdoms:quests-submit-success", Array.Empty<object>()));
				this.FetchQuestData();
				return;
			}
			this.capi.ShowChatMessage(Lang.Get("srguildsandkingdoms:quests-submit-failed", new object[]
			{
				response.Message
			}));
		}

		// Token: 0x060006A4 RID: 1700 RVA: 0x00032A80 File Offset: 0x00030C80
		private void OnCompleteReceived(QuestCompleteResponsePacket response)
		{
			if (response.Success)
			{
				this.capi.ShowChatMessage(Lang.Get("srguildsandkingdoms:quests-complete-success", Array.Empty<object>()));
				if (!string.IsNullOrEmpty(response.PeriodKey) && !this.completedPeriodKeys.Contains(response.PeriodKey))
				{
					this.completedPeriodKeys.Add(response.PeriodKey);
				}
				this.activeQuestProgress.RemoveAll((PlayerQuestProgressDto p) => p.QuestId == response.QuestId);
				this.FetchQuestData();
				return;
			}
			this.capi.ShowChatMessage(Lang.Get("srguildsandkingdoms:quests-complete-failed", new object[]
			{
				response.Message
			}));
		}

		// Token: 0x040002BC RID: 700
		private readonly ICoreClientAPI capi;

		// Token: 0x040002BD RID: 701
		private readonly SRGuildsAndKingdomsModSystem modSystem;

		// Token: 0x040002BE RID: 702
		[Nullable(2)]
		private readonly GuildSummary currentGuild;

		// Token: 0x040002BF RID: 703
		private readonly ActionConsumable onLeaveGuild;

		// Token: 0x040002C0 RID: 704
		[Nullable(2)]
		private readonly Action onNeedsRefresh;

		// Token: 0x040002C1 RID: 705
		[Nullable(2)]
		private readonly string questType;

		// Token: 0x040002C2 RID: 706
		[Nullable(2)]
		private GuiComposer composer;

		// Token: 0x040002C3 RID: 707
		private List<QuestDto> availableQuests = new List<QuestDto>();

		// Token: 0x040002C4 RID: 708
		private List<PlayerQuestProgressDto> activeQuestProgress = new List<PlayerQuestProgressDto>();

		// Token: 0x040002C5 RID: 709
		private List<string> completedPeriodKeys = new List<string>();

		// Token: 0x040002C6 RID: 710
		private int selectedPeriodTab;

		// Token: 0x040002C7 RID: 711
		private bool dataLoaded;

		// Token: 0x040002C8 RID: 712
		private bool isLoading;

		// Token: 0x040002C9 RID: 713
		private bool questListReceived;

		// Token: 0x040002CA RID: 714
		private bool progressReceived;
	}
}
