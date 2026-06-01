using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using SRGuildsAndKingdoms.src.network;
using SRGuildsAndKingdoms.src.quests;
using Vintagestory.API.Client;
using Vintagestory.API.Common;

namespace SRGuildsAndKingdoms.src.gui
{
	// Token: 0x02000080 RID: 128
	[NullableContext(1)]
	[Nullable(0)]
	public class DialogQuestManager : GuiDialog
	{
		// Token: 0x17000185 RID: 389
		// (get) Token: 0x06000583 RID: 1411 RVA: 0x000251FD File Offset: 0x000233FD
		// (set) Token: 0x06000584 RID: 1412 RVA: 0x00025205 File Offset: 0x00023405
		[Nullable(2)]
		public CurrencyDefinitionDto TailsDefinition { [NullableContext(2)] get; [NullableContext(2)] private set; }

		// Token: 0x17000186 RID: 390
		// (get) Token: 0x06000585 RID: 1413 RVA: 0x0002520E File Offset: 0x0002340E
		// (set) Token: 0x06000586 RID: 1414 RVA: 0x00025216 File Offset: 0x00023416
		[Nullable(2)]
		public CurrencyDefinitionDto CrownsDefinition { [NullableContext(2)] get; [NullableContext(2)] private set; }

		// Token: 0x17000187 RID: 391
		// (get) Token: 0x06000587 RID: 1415 RVA: 0x0002521F File Offset: 0x0002341F
		public override string ToggleKeyCombinationCode
		{
			get
			{
				return "questmanager";
			}
		}

		// Token: 0x06000588 RID: 1416 RVA: 0x00025228 File Offset: 0x00023428
		public DialogQuestManager(ICoreClientAPI capi, SRGuildsAndKingdomsModSystem modSystem) : base(capi)
		{
			this.modSystem = modSystem;
			QuestNetworkHandler questNetworkHandler = modSystem.QuestNetworkHandler;
			if (questNetworkHandler != null)
			{
				QuestNetworkHandler questNetworkHandler2 = questNetworkHandler;
				questNetworkHandler2.OnQuestManagerListReceived = (Action<QuestManagerListResponsePacket>)Delegate.Combine(questNetworkHandler2.OnQuestManagerListReceived, new Action<QuestManagerListResponsePacket>(this.OnQuestListReceived));
			}
			this.SetupDialog();
			this.RequestQuestList();
		}

		// Token: 0x06000589 RID: 1417 RVA: 0x00025298 File Offset: 0x00023498
		public override void OnGuiClosed()
		{
			base.OnGuiClosed();
			if (this.openEditorDialog != null)
			{
				this.openEditorDialog.TryClose();
				this.openEditorDialog = null;
			}
			QuestNetworkHandler questNetworkHandler = this.modSystem.QuestNetworkHandler;
			if (questNetworkHandler != null)
			{
				QuestNetworkHandler questNetworkHandler2 = questNetworkHandler;
				questNetworkHandler2.OnQuestManagerListReceived = (Action<QuestManagerListResponsePacket>)Delegate.Remove(questNetworkHandler2.OnQuestManagerListReceived, new Action<QuestManagerListResponsePacket>(this.OnQuestListReceived));
			}
		}

		// Token: 0x0600058A RID: 1418 RVA: 0x000252F8 File Offset: 0x000234F8
		private void RequestQuestList()
		{
			this.isLoading = true;
			this.errorMessage = null;
			QuestNetworkHandler questNetworkHandler = this.modSystem.QuestNetworkHandler;
			IClientPlayer player = this.capi.World.Player;
			string playerUid = ((player != null) ? player.PlayerUID : null) ?? string.Empty;
			if (questNetworkHandler != null && !string.IsNullOrEmpty(playerUid))
			{
				questNetworkHandler.RequestQuestManagerList(playerUid);
				return;
			}
			this.isLoading = false;
			this.errorMessage = "Failed to request quest list";
			this.SetupDialog();
		}

		// Token: 0x0600058B RID: 1419 RVA: 0x00025370 File Offset: 0x00023570
		private void OnQuestListReceived(QuestManagerListResponsePacket packet)
		{
			this.isLoading = false;
			if (!packet.Success)
			{
				this.errorMessage = packet.Message;
				this.quests = new List<QuestDto>();
			}
			else
			{
				this.errorMessage = null;
				this.quests = packet.Quests;
				this.TailsDefinition = packet.TailsDefinition;
				this.CrownsDefinition = packet.CrownsDefinition;
				this.serverLocalTime = packet.ServerLocalTime;
				this.serverTimezoneOffset = packet.ServerTimezoneOffset;
			}
			this.SetupDialog();
		}

		// Token: 0x0600058C RID: 1420 RVA: 0x000253F0 File Offset: 0x000235F0
		private void SetupDialog()
		{
			ElementBounds dialogBounds = ElementStdBounds.AutosizedMainDialog.WithAlignment(6);
			ElementBounds bgBounds = ElementBounds.Fill.WithFixedPadding(GuiStyle.ElementToDialogPadding);
			bgBounds.BothSizing = 2;
			GuiComposer composer = GuiComposerHelpers.AddDialogTitleBar(GuiComposerHelpers.AddShadedDialogBG(this.capi.Gui.CreateCompo("questmanager", dialogBounds), bgBounds, true, 5.0, 0.75f), "Quest Manager", new Action(this.OnTitleBarCloseClicked), null, null, null).BeginChildElements(bgBounds);
			double yPos = 30.0;
			if (this.isLoading)
			{
				GuiComposerHelpers.AddStaticText(composer, "Loading quests...", CairoFont.WhiteSmallText(), ElementBounds.Fixed(0.0, yPos, 760.0, 30.0), null);
			}
			else if (!string.IsNullOrEmpty(this.errorMessage))
			{
				GuiComposerHelpers.AddStaticText(composer, "Error: " + this.errorMessage, CairoFont.WhiteSmallText().WithColor(new double[]
				{
					1.0,
					0.3,
					0.3,
					1.0
				}), ElementBounds.Fixed(0.0, yPos, 760.0, 30.0), null);
				yPos += 40.0;
				GuiComposerHelpers.AddSmallButton(composer, "Retry", new ActionConsumable(this.OnRetryClicked), ElementBounds.Fixed(0.0, yPos, 80.0, 30.0), 2, null);
			}
			else
			{
				GuiComposer guiComposer = composer;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(14, 1);
				defaultInterpolatedStringHandler.AppendLiteral("Total Quests: ");
				defaultInterpolatedStringHandler.AppendFormatted<int>(this.quests.Count);
				GuiComposerHelpers.AddStaticText(guiComposer, defaultInterpolatedStringHandler.ToStringAndClear(), CairoFont.WhiteSmallText(), ElementBounds.Fixed(0.0, yPos, 200.0, 25.0), null);
				GuiComposerHelpers.AddStaticText(composer, "Show:", CairoFont.WhiteSmallText(), ElementBounds.Fixed(470.0, yPos + 3.0, 50.0, 25.0), null);
				GuiComposerHelpers.AddDropDown(composer, DialogQuestManager.FilterOptions, DialogQuestManager.FilterOptions, this.selectedFilterIndex, new SelectionChangedDelegate(this.OnFilterChanged), ElementBounds.Fixed(520.0, yPos - 3.0, 120.0, 28.0), "filterDropdown");
				GuiComposerHelpers.AddSmallButton(composer, "Add New", new ActionConsumable(this.OnAddNewClicked), ElementBounds.Fixed(680.0, yPos - 5.0, 80.0, 30.0), 2, null);
				yPos += 35.0;
				double colTitleWidth = 280.0;
				double colTypeWidth = 90.0;
				double colDatesWidth = 230.0;
				double colStatusWidth = 80.0;
				GuiComposerHelpers.AddStaticText(composer, "Title", CairoFont.WhiteSmallText().WithWeight(1), ElementBounds.Fixed(0.0, yPos, colTitleWidth, 20.0), null);
				GuiComposerHelpers.AddStaticText(composer, "Type", CairoFont.WhiteSmallText().WithWeight(1), ElementBounds.Fixed(colTitleWidth, yPos, colTypeWidth, 20.0), null);
				GuiComposerHelpers.AddStaticText(composer, "Dates", CairoFont.WhiteSmallText().WithWeight(1), ElementBounds.Fixed(colTitleWidth + colTypeWidth, yPos, colDatesWidth, 20.0), null);
				GuiComposerHelpers.AddStaticText(composer, "Status", CairoFont.WhiteSmallText().WithWeight(1), ElementBounds.Fixed(colTitleWidth + colTypeWidth + colDatesWidth, yPos, colStatusWidth, 20.0), null);
				yPos += 25.0;
				List<QuestDto> sortedQuests = (from q in (this.selectedFilterIndex == 0) ? this.quests : (from q in this.quests
				where q.RecurrenceType.Equals(DialogQuestManager.FilterOptions[this.selectedFilterIndex], StringComparison.OrdinalIgnoreCase)
				select q).ToList<QuestDto>()
				orderby DialogQuestManager.GetStatusSortOrder(this.GetQuestStatus(q)), q.RecurrenceType, q.StartsAt descending
				select q).ToList<QuestDto>();
				this.visibleHeight = 500.0 - yPos - 60.0;
				this.contentHeight = Math.Max(this.visibleHeight, (double)sortedQuests.Count * 40.0 + 20.0);
				ElementBounds clippingBounds = ElementBounds.Fixed(0.0, yPos, 740.0, this.visibleHeight);
				this.scrollableBounds = ElementBounds.Fixed(0.0, 0.0, 720.0, this.contentHeight);
				this.scrollableBounds.WithParent(clippingBounds);
				ElementBounds insetBounds = clippingBounds.FlatCopy().FixedGrow(6.0).WithFixedOffset(-3.0, -3.0);
				ElementBounds scrollbarBounds = insetBounds.CopyOffsetedSibling(clippingBounds.fixedWidth + 7.0, 0.0, 0.0, 0.0).WithFixedWidth(20.0);
				GuiElementInsetHelper.AddInset(composer, insetBounds, 3, 0.85f);
				GuiElementClipHelpler.BeginClip(composer, clippingBounds);
				this.scrollableChildBounds.Clear();
				double rowY = 5.0;
				for (int i = 0; i < sortedQuests.Count; i++)
				{
					QuestDto quest = sortedQuests[i];
					int questIndex = this.quests.IndexOf(quest);
					string status = this.GetQuestStatus(quest);
					double[] statusColor = DialogQuestManager.GetStatusColor(status);
					string displayTitle = (quest.Title.Length > 35) ? (quest.Title.Substring(0, 32) + "...") : quest.Title;
					GuiComposer guiComposer2 = composer;
					string text = displayTitle;
					CairoFont cairoFont = CairoFont.WhiteSmallText();
					ElementBounds elementBounds = this.<SetupDialog>g__CreateChildBounds|31_4(5.0, rowY + 10.0, colTitleWidth - 5.0, 20.0);
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(12, 1);
					defaultInterpolatedStringHandler2.AppendLiteral("quest_");
					defaultInterpolatedStringHandler2.AppendFormatted<int>(i);
					defaultInterpolatedStringHandler2.AppendLiteral("_title");
					GuiElementDynamicTextHelper.AddDynamicText(guiComposer2, text, cairoFont, elementBounds, defaultInterpolatedStringHandler2.ToStringAndClear());
					GuiComposer guiComposer3 = composer;
					string text2 = DialogQuestManager.CapitalizeFirst(quest.RecurrenceType);
					CairoFont cairoFont2 = CairoFont.WhiteSmallText();
					ElementBounds elementBounds2 = this.<SetupDialog>g__CreateChildBounds|31_4(colTitleWidth, rowY + 10.0, colTypeWidth - 5.0, 20.0);
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler3 = new DefaultInterpolatedStringHandler(11, 1);
					defaultInterpolatedStringHandler3.AppendLiteral("quest_");
					defaultInterpolatedStringHandler3.AppendFormatted<int>(i);
					defaultInterpolatedStringHandler3.AppendLiteral("_type");
					GuiElementDynamicTextHelper.AddDynamicText(guiComposer3, text2, cairoFont2, elementBounds2, defaultInterpolatedStringHandler3.ToStringAndClear());
					string dateDisplay = quest.StartsAt + " - " + quest.ExpiresAt;
					GuiComposer guiComposer4 = composer;
					string text3 = dateDisplay;
					CairoFont cairoFont3 = CairoFont.WhiteSmallText();
					ElementBounds elementBounds3 = this.<SetupDialog>g__CreateChildBounds|31_4(colTitleWidth + colTypeWidth, rowY + 10.0, colDatesWidth - 5.0, 20.0);
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler4 = new DefaultInterpolatedStringHandler(12, 1);
					defaultInterpolatedStringHandler4.AppendLiteral("quest_");
					defaultInterpolatedStringHandler4.AppendFormatted<int>(i);
					defaultInterpolatedStringHandler4.AppendLiteral("_dates");
					GuiElementDynamicTextHelper.AddDynamicText(guiComposer4, text3, cairoFont3, elementBounds3, defaultInterpolatedStringHandler4.ToStringAndClear());
					GuiComposer guiComposer5 = composer;
					string text4 = status;
					CairoFont cairoFont4 = CairoFont.WhiteSmallText().WithColor(statusColor);
					ElementBounds elementBounds4 = this.<SetupDialog>g__CreateChildBounds|31_4(colTitleWidth + colTypeWidth + colDatesWidth, rowY + 10.0, colStatusWidth - 5.0, 20.0);
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler5 = new DefaultInterpolatedStringHandler(13, 1);
					defaultInterpolatedStringHandler5.AppendLiteral("quest_");
					defaultInterpolatedStringHandler5.AppendFormatted<int>(i);
					defaultInterpolatedStringHandler5.AppendLiteral("_status");
					GuiElementDynamicTextHelper.AddDynamicText(guiComposer5, text4, cairoFont4, elementBounds4, defaultInterpolatedStringHandler5.ToStringAndClear());
					GuiComposerHelpers.AddSmallButton(composer, "Edit", () => this.OnEditQuestClicked(questIndex), this.<SetupDialog>g__CreateChildBounds|31_4(680.0, rowY + 5.0, 60.0, 28.0), 2, null);
					rowY += 40.0;
				}
				GuiElementClipHelpler.EndClip(composer);
				GuiComposerHelpers.AddVerticalScrollbar(composer, new Action<float>(this.OnNewScrollbarValue), scrollbarBounds, "questListScrollbar");
				yPos += this.visibleHeight + 10.0;
				GuiComposerHelpers.AddSmallButton(composer, "Refresh", new ActionConsumable(this.OnRefreshClicked), ElementBounds.Fixed(0.0, yPos, 80.0, 30.0), 2, null);
			}
			composer.EndChildElements();
			base.SingleComposer = composer.Compose(true);
			if (this.scrollableBounds != null)
			{
				GuiElementScrollbar scrollbar = GuiComposerHelpers.GetScrollbar(base.SingleComposer, "questListScrollbar");
				if (scrollbar == null)
				{
					return;
				}
				scrollbar.SetHeights((float)this.visibleHeight, (float)this.contentHeight);
			}
		}

		// Token: 0x0600058D RID: 1421 RVA: 0x00025CE0 File Offset: 0x00023EE0
		private void OnNewScrollbarValue(float value)
		{
			if (this.scrollableBounds == null)
			{
				return;
			}
			this.scrollableBounds.fixedY = (double)(-(double)value);
			this.scrollableBounds.CalcWorldBounds();
			foreach (ElementBounds elementBounds in this.scrollableChildBounds)
			{
				elementBounds.CalcWorldBounds();
			}
		}

		// Token: 0x0600058E RID: 1422 RVA: 0x00025D54 File Offset: 0x00023F54
		private string GetQuestStatus(QuestDto quest)
		{
			try
			{
				DateTime now;
				if (quest.UsesIngameTime)
				{
					now = this.GetCurrentIngameDate();
				}
				else
				{
					now = ((this.serverLocalTime > 0L) ? DateTimeOffset.FromUnixTimeSeconds(this.serverLocalTime).ToOffset(TimeSpan.FromHours(this.serverTimezoneOffset)).DateTime : DateTime.Now);
				}
				DateTime startsAt;
				DateTime expiresAt;
				if (QuestPeriodKeyGenerator.TryParseDate(quest.StartsAt, out startsAt) && QuestPeriodKeyGenerator.TryParseDate(quest.ExpiresAt, out expiresAt))
				{
					if (now.Date < startsAt.Date)
					{
						return "Future";
					}
					if (now.Date > expiresAt.Date)
					{
						return "Expired";
					}
					return "Active";
				}
			}
			catch
			{
			}
			return "Unknown";
		}

		// Token: 0x0600058F RID: 1423 RVA: 0x00025E2C File Offset: 0x0002402C
		private static int GetStatusSortOrder(string status)
		{
			int result;
			if (!(status == "Active"))
			{
				if (!(status == "Future"))
				{
					if (!(status == "Expired"))
					{
						result = 3;
					}
					else
					{
						result = 2;
					}
				}
				else
				{
					result = 1;
				}
			}
			else
			{
				result = 0;
			}
			return result;
		}

		// Token: 0x06000590 RID: 1424 RVA: 0x00025E74 File Offset: 0x00024074
		private DateTime GetCurrentIngameDate()
		{
			IClientGameCalendar calendar = this.capi.World.Calendar;
			if (calendar == null)
			{
				return DateTime.Now;
			}
			return new DateTime(calendar.Year + 1, calendar.Month, calendar.DayOfYear % calendar.DaysPerMonth + 1);
		}

		// Token: 0x06000591 RID: 1425 RVA: 0x00025EC0 File Offset: 0x000240C0
		private static double[] GetStatusColor(string status)
		{
			double[] result;
			if (!(status == "Active"))
			{
				if (!(status == "Future"))
				{
					if (!(status == "Expired"))
					{
						result = new double[]
						{
							1.0,
							1.0,
							1.0,
							1.0
						};
					}
					else
					{
						result = new double[]
						{
							0.6,
							0.6,
							0.6,
							1.0
						};
					}
				}
				else
				{
					result = new double[]
					{
						0.3,
						0.7,
						1.0,
						1.0
					};
				}
			}
			else
			{
				result = new double[]
				{
					0.3,
					1.0,
					0.3,
					1.0
				};
			}
			return result;
		}

		// Token: 0x06000592 RID: 1426 RVA: 0x00025F48 File Offset: 0x00024148
		private static string CapitalizeFirst(string s)
		{
			if (string.IsNullOrEmpty(s))
			{
				return s;
			}
			char c = char.ToUpper(s[0]);
			return new ReadOnlySpan<char>(ref c) + s.Substring(1, s.Length - 1);
		}

		// Token: 0x06000593 RID: 1427 RVA: 0x00025F8E File Offset: 0x0002418E
		private void OnTitleBarCloseClicked()
		{
			this.TryClose();
		}

		// Token: 0x06000594 RID: 1428 RVA: 0x00025F97 File Offset: 0x00024197
		private bool OnRetryClicked()
		{
			this.RequestQuestList();
			return true;
		}

		// Token: 0x06000595 RID: 1429 RVA: 0x00025FA0 File Offset: 0x000241A0
		private bool OnRefreshClicked()
		{
			this.RequestQuestList();
			return true;
		}

		// Token: 0x06000596 RID: 1430 RVA: 0x00025FA9 File Offset: 0x000241A9
		private void OnFilterChanged(string code, bool selected)
		{
			this.selectedFilterIndex = Array.IndexOf<string>(DialogQuestManager.FilterOptions, code);
			if (this.selectedFilterIndex < 0)
			{
				this.selectedFilterIndex = 0;
			}
			this.SetupDialog();
		}

		// Token: 0x06000597 RID: 1431 RVA: 0x00025FD4 File Offset: 0x000241D4
		private bool OnAddNewClicked()
		{
			if (this.openEditorDialog != null && this.openEditorDialog.IsOpened())
			{
				this.openEditorDialog.TryClose();
			}
			this.openEditorDialog = null;
			this.openEditorDialog = new DialogQuestEditor(this.capi, this.modSystem, null, this.TailsDefinition, this.CrownsDefinition, this.serverLocalTime, this.serverTimezoneOffset);
			this.openEditorDialog.OnDialogClosed += delegate()
			{
				this.openEditorDialog = null;
			};
			this.openEditorDialog.TryOpen();
			return true;
		}

		// Token: 0x06000598 RID: 1432 RVA: 0x00026060 File Offset: 0x00024260
		private bool OnEditQuestClicked(int questIndex)
		{
			if (questIndex < 0 || questIndex >= this.quests.Count)
			{
				return false;
			}
			if (this.openEditorDialog != null && this.openEditorDialog.IsOpened())
			{
				this.openEditorDialog.TryClose();
			}
			this.openEditorDialog = null;
			QuestDto quest = this.quests[questIndex];
			this.openEditorDialog = new DialogQuestEditor(this.capi, this.modSystem, quest, this.TailsDefinition, this.CrownsDefinition, this.serverLocalTime, this.serverTimezoneOffset);
			this.openEditorDialog.OnDialogClosed += delegate()
			{
				this.openEditorDialog = null;
			};
			this.openEditorDialog.OnQuestSaved += delegate()
			{
				this.RequestQuestList();
			};
			this.openEditorDialog.TryOpen();
			return true;
		}

		// Token: 0x0600059C RID: 1436 RVA: 0x00026180 File Offset: 0x00024380
		[CompilerGenerated]
		private ElementBounds <SetupDialog>g__CreateChildBounds|31_4(double x, double y, double width, double height)
		{
			ElementBounds bounds = ElementBounds.Fixed(x, y, width, height);
			bounds.WithParent(this.scrollableBounds);
			this.scrollableChildBounds.Add(bounds);
			return bounds;
		}

		// Token: 0x04000220 RID: 544
		private readonly SRGuildsAndKingdomsModSystem modSystem;

		// Token: 0x04000221 RID: 545
		private List<QuestDto> quests = new List<QuestDto>();

		// Token: 0x04000222 RID: 546
		private bool isLoading = true;

		// Token: 0x04000223 RID: 547
		[Nullable(2)]
		private string errorMessage;

		// Token: 0x04000226 RID: 550
		private long serverLocalTime;

		// Token: 0x04000227 RID: 551
		private double serverTimezoneOffset;

		// Token: 0x04000228 RID: 552
		private const double DIALOG_WIDTH = 800.0;

		// Token: 0x04000229 RID: 553
		private const double DIALOG_HEIGHT = 500.0;

		// Token: 0x0400022A RID: 554
		private const double ROW_HEIGHT = 40.0;

		// Token: 0x0400022B RID: 555
		private const double BUTTON_WIDTH = 80.0;

		// Token: 0x0400022C RID: 556
		[Nullable(2)]
		private ElementBounds scrollableBounds;

		// Token: 0x0400022D RID: 557
		private readonly List<ElementBounds> scrollableChildBounds = new List<ElementBounds>();

		// Token: 0x0400022E RID: 558
		private double visibleHeight;

		// Token: 0x0400022F RID: 559
		private double contentHeight;

		// Token: 0x04000230 RID: 560
		private int selectedFilterIndex;

		// Token: 0x04000231 RID: 561
		private static readonly string[] FilterOptions = new string[]
		{
			"All",
			"Daily",
			"Weekly",
			"Monthly",
			"Seasonal"
		};

		// Token: 0x04000232 RID: 562
		[Nullable(2)]
		private DialogQuestEditor openEditorDialog;
	}
}
