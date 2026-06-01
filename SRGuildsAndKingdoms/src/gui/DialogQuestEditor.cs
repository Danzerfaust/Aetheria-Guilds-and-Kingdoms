using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using SRGuildsAndKingdoms.src.gui.components;
using SRGuildsAndKingdoms.src.network;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;

namespace SRGuildsAndKingdoms.src.gui
{
	// Token: 0x0200007E RID: 126
	[NullableContext(1)]
	[Nullable(0)]
	public class DialogQuestEditor : GuiDialog
	{
		// Token: 0x14000002 RID: 2
		// (add) Token: 0x06000559 RID: 1369 RVA: 0x00021560 File Offset: 0x0001F760
		// (remove) Token: 0x0600055A RID: 1370 RVA: 0x00021598 File Offset: 0x0001F798
		[Nullable(2)]
		[method: NullableContext(2)]
		[Nullable(2)]
		public event Action OnDialogClosed;

		// Token: 0x14000003 RID: 3
		// (add) Token: 0x0600055B RID: 1371 RVA: 0x000215D0 File Offset: 0x0001F7D0
		// (remove) Token: 0x0600055C RID: 1372 RVA: 0x00021608 File Offset: 0x0001F808
		[Nullable(2)]
		[method: NullableContext(2)]
		[Nullable(2)]
		public event Action OnQuestSaved;

		// Token: 0x17000183 RID: 387
		// (get) Token: 0x0600055D RID: 1373 RVA: 0x0002163D File Offset: 0x0001F83D
		public override string ToggleKeyCombinationCode
		{
			get
			{
				return "questeditor";
			}
		}

		// Token: 0x0600055E RID: 1374 RVA: 0x00021644 File Offset: 0x0001F844
		[NullableContext(2)]
		public unsafe DialogQuestEditor([Nullable(1)] ICoreClientAPI capi, [Nullable(1)] SRGuildsAndKingdomsModSystem modSystem, QuestDto quest = null, CurrencyDefinitionDto tailsDefinition = null, CurrencyDefinitionDto crownsDefinition = null, long serverLocalTime = 0L, double serverTimezoneOffset = 0.0)
		{
			int num = DateTime.Now.Year;
			this.formStartYear = num.ToString();
			num = DateTime.Now.Month;
			this.formStartMonth = num.ToString("D2");
			num = DateTime.Now.Day;
			this.formStartDay = num.ToString("D2");
			num = DateTime.Now.Year;
			this.formEndYear = num.ToString();
			num = DateTime.Now.Month;
			this.formEndMonth = num.ToString("D2");
			num = DateTime.Now.Day;
			this.formEndDay = num.ToString("D2");
			num = 1;
			List<QuestObjectiveDto> list = new List<QuestObjectiveDto>(num);
			CollectionsMarshal.SetCount<QuestObjectiveDto>(list, num);
			*CollectionsMarshal.AsSpan<QuestObjectiveDto>(list)[0] = new QuestObjectiveDto
			{
				Id = DialogQuestEditor.rnd.Next(1, 10000000),
				AcceptedItems = new List<QuestAcceptedItemDto>(),
				AcceptedTargets = new List<string>(),
				Count = 5,
				Type = "turn_in"
			};
			this.formObjectives = list;
			num = 1;
			List<DialogQuestEditor.QuestRewardDtoWithNullableCode> list2 = new List<DialogQuestEditor.QuestRewardDtoWithNullableCode>(num);
			CollectionsMarshal.SetCount<DialogQuestEditor.QuestRewardDtoWithNullableCode>(list2, num);
			*CollectionsMarshal.AsSpan<DialogQuestEditor.QuestRewardDtoWithNullableCode>(list2)[0] = new DialogQuestEditor.QuestRewardDtoWithNullableCode
			{
				Amount = 5
			};
			this.formRewards = list2;
			base..ctor(capi);
			this.modSystem = modSystem;
			this.existingQuest = quest;
			this.tailsDefinition = tailsDefinition;
			this.crownsDefinition = crownsDefinition;
			this.serverLocalTime = serverLocalTime;
			this.serverTimezoneOffset = serverTimezoneOffset;
			DateTime defaultDate = (serverLocalTime > 0L) ? DateTimeOffset.FromUnixTimeSeconds(serverLocalTime).ToOffset(TimeSpan.FromHours(serverTimezoneOffset)).DateTime : DateTime.Now;
			num = defaultDate.Year;
			this.formStartYear = num.ToString();
			num = defaultDate.Month;
			this.formStartMonth = num.ToString("D2");
			num = defaultDate.Day;
			this.formStartDay = num.ToString("D2");
			num = defaultDate.Year;
			this.formEndYear = num.ToString();
			num = defaultDate.Month;
			this.formEndMonth = num.ToString("D2");
			num = defaultDate.Day;
			this.formEndDay = num.ToString("D2");
			this.objectiveInventory = new InventoryGeneric(1000, "srguildsandkingdoms:questeditor-objectives", capi, (int id, InventoryGeneric inv) => new ItemSlotDisplayOnly(inv)
			{
				MaxSlotStackSize = 1
			});
			this.objectiveInventory.SlotModified += this.OnInventorySlotModified;
			this.rewardsInventory = new InventoryGeneric(10, "srguildsandkingdoms:questeditor-rewards", capi, (int id, InventoryGeneric inv) => new ItemSlotDisplayOnly(inv)
			{
				MaxSlotStackSize = 1
			});
			this.rewardsInventory.SlotModified += this.OnRewardsInventorySlotModified;
			if (this.existingQuest != null)
			{
				this.formTitle = (this.existingQuest.Title ?? "");
				this.formDescription = (this.existingQuest.Description ?? "");
				this.formType = (this.existingQuest.RecurrenceType ?? "daily");
				this.formIgt = this.existingQuest.UsesIngameTime;
				this.formRepeating = this.existingQuest.Repeat;
				string startsAt = this.existingQuest.StartsAt;
				if (this.formIgt && startsAt.StartsWith("0000"))
				{
					string str = "0001";
					string text = startsAt;
					startsAt = str + text.Substring(4, text.Length - 4);
				}
				DateTime startDate;
				if (DateTime.TryParseExact(startsAt, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out startDate))
				{
					num = startDate.Year;
					this.formStartYear = num.ToString(this.formIgt ? "D4" : "");
					num = startDate.Month;
					this.formStartMonth = num.ToString("D2");
					num = startDate.Day;
					this.formStartDay = num.ToString("D2");
				}
				string expiresAt = this.existingQuest.ExpiresAt;
				if (this.formIgt && expiresAt.StartsWith("0000"))
				{
					string str2 = "0001";
					string text = expiresAt;
					expiresAt = str2 + text.Substring(4, text.Length - 4);
				}
				DateTime endDate;
				if (DateTime.TryParseExact(expiresAt, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out endDate))
				{
					num = endDate.Year;
					this.formEndYear = num.ToString(this.formIgt ? "D4" : "");
					num = endDate.Month;
					this.formEndMonth = num.ToString("D2");
					num = endDate.Day;
					this.formEndDay = num.ToString("D2");
				}
				if (this.existingQuest.Objectives != null)
				{
					this.formObjectives = (from o in this.existingQuest.Objectives
					select new QuestObjectiveDto
					{
						Id = o.Id,
						Type = o.Type,
						Count = o.Count,
						AcceptedItems = new List<QuestAcceptedItemDto>(o.AcceptedItems ?? new List<QuestAcceptedItemDto>()),
						AcceptedTargets = new List<string>(o.AcceptedTargets ?? new List<string>())
					}).ToList<QuestObjectiveDto>();
				}
			}
			QuestDto questDto = this.existingQuest;
			if (questDto != null && questDto.Rewards.Count > 0)
			{
				this.formRewards = (from r in this.existingQuest.Rewards
				select new DialogQuestEditor.QuestRewardDtoWithNullableCode
				{
					Code = r.Code,
					Amount = r.Amount,
					Nbt = r.Nbt
				}).ToList<DialogQuestEditor.QuestRewardDtoWithNullableCode>();
			}
			this.SetupDialog();
		}

		// Token: 0x0600055F RID: 1375 RVA: 0x00021BD8 File Offset: 0x0001FDD8
		private void OnInventorySlotModified(int obj)
		{
			this.SaveFormState();
			this.SetupDialog();
		}

		// Token: 0x06000560 RID: 1376 RVA: 0x00021BE6 File Offset: 0x0001FDE6
		private void OnRewardsInventorySlotModified(int rwd)
		{
			this.SaveFormState();
			this.SetupDialog();
		}

		// Token: 0x06000561 RID: 1377 RVA: 0x00021BF4 File Offset: 0x0001FDF4
		private void SetupDialog()
		{
			string title = (this.existingQuest != null) ? "Edit Quest" : "New Quest";
			ElementBounds dialogBounds = ElementStdBounds.AutosizedMainDialog.WithAlignment(6);
			ElementBounds bgBounds = ElementBounds.Fill.WithFixedPadding(GuiStyle.ElementToDialogPadding);
			bgBounds.BothSizing = 2;
			GuiComposer composer = GuiComposerHelpers.AddDialogTitleBar(GuiComposerHelpers.AddShadedDialogBG(this.capi.Gui.CreateCompo("questeditor", dialogBounds), bgBounds, true, 5.0, 0.75f), title, new Action(this.OnTitleBarCloseClicked), null, null, null).BeginChildElements(bgBounds);
			double yPos = 30.0;
			double actualContentHeight = this.AddQuestEditorFields(composer, yPos);
			GuiComposerHelpers.AddSmallButton(composer, "Cancel", new ActionConsumable(this.OnCancelClicked), ElementBounds.Fixed(0.0, actualContentHeight + 50.0, 80.0, 30.0), 2, null);
			GuiComposerHelpers.AddSmallButton(composer, "Save", () => this.OnSaveClicked(false), ElementBounds.Fixed(870.0, actualContentHeight + 50.0, 80.0, 30.0), 2, null);
			if (this.existingQuest != null)
			{
				GuiComposerHelpers.AddSmallButton(composer, "Save as New", () => this.OnSaveClicked(true), ElementBounds.Fixed(710.0, actualContentHeight + 50.0, 140.0, 30.0), 2, null);
				GuiComposerHelpers.AddSmallButton(composer, "Delete", new ActionConsumable(this.OnDeleteClicked), ElementBounds.Fixed(100.0, actualContentHeight + 50.0, 80.0, 30.0), 2, null);
			}
			composer.EndChildElements();
			base.SingleComposer = composer.Compose(true);
			GuiComposerHelpers.GetDropDown(composer, "endYear").Enabled = (this.formType == "seasonal");
			GuiComposerHelpers.GetDropDown(composer, "endMonth").Enabled = (this.formType == "seasonal");
			GuiComposerHelpers.GetDropDown(composer, "endDay").Enabled = (this.formType == "seasonal");
			GuiComposerHelpers.GetDropDown(composer, "startDay").Enabled = (this.formType != "monthly");
			GuiComposerHelpers.GetTextArea(composer, "description").SetValue(this.formDescription, true);
			for (int i = 0; i < this.formObjectives.Count; i++)
			{
				GuiComposer guiComposer = composer;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(23, 1);
				defaultInterpolatedStringHandler.AppendLiteral("objective_targetSelect_");
				defaultInterpolatedStringHandler.AppendFormatted<int>(i);
				GuiElementDropDown dropDown = GuiComposerHelpers.GetDropDown(guiComposer, defaultInterpolatedStringHandler.ToStringAndClear());
				if (dropDown != null)
				{
					List<string> acceptedTargets = this.formObjectives[i].AcceptedTargets;
					dropDown.SetSelectedValue(((acceptedTargets != null) ? acceptedTargets.ToArray() : null) ?? Array.Empty<string>());
				}
			}
			base.SingleComposer.UnfocusOwnElements();
		}

		// Token: 0x06000562 RID: 1378 RVA: 0x00021EEC File Offset: 0x000200EC
		private void SaveFormState()
		{
			if (base.SingleComposer == null)
			{
				return;
			}
			GuiElementTextInput textInput = GuiComposerHelpers.GetTextInput(base.SingleComposer, "title");
			this.formTitle = (((textInput != null) ? textInput.GetText() : null) ?? this.formTitle);
			GuiElementTextArea textArea = GuiComposerHelpers.GetTextArea(base.SingleComposer, "description");
			this.formDescription = (((textArea != null) ? textArea.GetText() : null) ?? this.formDescription);
			GuiElementDropDown dropDown = GuiComposerHelpers.GetDropDown(base.SingleComposer, "type");
			this.formType = (((dropDown != null) ? dropDown.SelectedValue : null) ?? this.formType);
			GuiElementSwitch @switch = GuiComposerHelpers.GetSwitch(base.SingleComposer, "inGameTime");
			this.formIgt = ((@switch != null) ? @switch.On : this.formIgt);
			GuiElementSwitch switch2 = GuiComposerHelpers.GetSwitch(base.SingleComposer, "repeating");
			this.formRepeating = ((switch2 != null) ? switch2.On : this.formRepeating);
			GuiElementDropDown dropDown2 = GuiComposerHelpers.GetDropDown(base.SingleComposer, "startYear");
			this.formStartYear = (((dropDown2 != null) ? dropDown2.SelectedValue : null) ?? this.formStartYear);
			GuiElementDropDown dropDown3 = GuiComposerHelpers.GetDropDown(base.SingleComposer, "startMonth");
			this.formStartMonth = (((dropDown3 != null) ? dropDown3.SelectedValue : null) ?? this.formStartMonth);
			GuiElementDropDown dropDown4 = GuiComposerHelpers.GetDropDown(base.SingleComposer, "startDay");
			this.formStartDay = (((dropDown4 != null) ? dropDown4.SelectedValue : null) ?? this.formStartDay);
			GuiElementDropDown dropDown5 = GuiComposerHelpers.GetDropDown(base.SingleComposer, "endYear");
			this.formEndYear = (((dropDown5 != null) ? dropDown5.SelectedValue : null) ?? this.formEndYear);
			GuiElementDropDown dropDown6 = GuiComposerHelpers.GetDropDown(base.SingleComposer, "endMonth");
			this.formEndMonth = (((dropDown6 != null) ? dropDown6.SelectedValue : null) ?? this.formEndMonth);
			GuiElementDropDown dropDown7 = GuiComposerHelpers.GetDropDown(base.SingleComposer, "endDay");
			this.formEndDay = (((dropDown7 != null) ? dropDown7.SelectedValue : null) ?? this.formEndDay);
			for (int i = 0; i < this.formObjectives.Count; i++)
			{
				GuiComposer singleComposer = base.SingleComposer;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(15, 1);
				defaultInterpolatedStringHandler.AppendLiteral("objective_type_");
				defaultInterpolatedStringHandler.AppendFormatted<int>(i);
				GuiElementDropDown typeDropdown = GuiComposerHelpers.GetDropDown(singleComposer, defaultInterpolatedStringHandler.ToStringAndClear());
				if (typeDropdown != null)
				{
					this.formObjectives[i].Type = typeDropdown.SelectedValue;
					if (this.formObjectives[i].Type == "turn_in" && this.objectiveInventory != null)
					{
						this.formObjectives[i].AcceptedItems = new List<QuestAcceptedItemDto>();
						for (int itemIdx = 0; itemIdx < 100; itemIdx++)
						{
							int slotId = i * 100 + itemIdx;
							ItemStack itemstack = this.objectiveInventory[slotId].Itemstack;
							ItemStack setStack = (itemstack != null) ? itemstack.Clone() : null;
							if (setStack != null)
							{
								QuestAcceptedItemDto acceptedItem = new QuestAcceptedItemDto
								{
									Code = setStack.Collectible.Code.ToString()
								};
								if (setStack.Attributes != null)
								{
									foreach (string ignoreAttr in QuestNetworkHandler.NbtAttributesToIgnore)
									{
										if (setStack.Attributes.HasAttribute(ignoreAttr))
										{
											setStack.Attributes.RemoveAttribute(ignoreAttr);
										}
									}
									if (setStack.Attributes.Count != 0)
									{
										using (MemoryStream ms = new MemoryStream())
										{
											using (BinaryWriter writer = new BinaryWriter(ms))
											{
												setStack.Attributes.ToBytes(writer);
												acceptedItem.Nbt = Convert.ToBase64String(ms.ToArray());
											}
										}
									}
								}
								this.formObjectives[i].AcceptedItems.Add(acceptedItem);
							}
						}
					}
					if (this.formObjectives[i].Type == "kill")
					{
						QuestObjectiveDto questObjectiveDto = this.formObjectives[i];
						GuiComposer singleComposer2 = base.SingleComposer;
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(23, 1);
						defaultInterpolatedStringHandler2.AppendLiteral("objective_targetSelect_");
						defaultInterpolatedStringHandler2.AppendFormatted<int>(i);
						GuiElementDropDown dropDown8 = GuiComposerHelpers.GetDropDown(singleComposer2, defaultInterpolatedStringHandler2.ToStringAndClear());
						string[] source;
						if ((source = ((dropDown8 != null) ? dropDown8.SelectedValues : null)) == null)
						{
							List<string> acceptedTargets = this.formObjectives[i].AcceptedTargets;
							source = (((acceptedTargets != null) ? acceptedTargets.ToArray() : null) ?? Array.Empty<string>());
						}
						questObjectiveDto.AcceptedTargets = source.ToList<string>();
					}
				}
				GuiComposer singleComposer3 = base.SingleComposer;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler3 = new DefaultInterpolatedStringHandler(19, 1);
				defaultInterpolatedStringHandler3.AppendLiteral("objective_quantity_");
				defaultInterpolatedStringHandler3.AppendFormatted<int>(i);
				GuiElementNumberInput objectiveQuantity = GuiComposerHelpers.GetNumberInput(singleComposer3, defaultInterpolatedStringHandler3.ToStringAndClear());
				int count;
				this.formObjectives[i].Count = (int.TryParse((objectiveQuantity != null) ? objectiveQuantity.GetText() : null, out count) ? count : this.formObjectives[i].Count);
			}
			List<DialogQuestEditor.QuestRewardDtoWithNullableCode> localRewards = this.formRewards;
			this.formRewards = new List<DialogQuestEditor.QuestRewardDtoWithNullableCode>();
			for (int j = 0; j < localRewards.Count; j++)
			{
				if (this.rewardsInventory != null)
				{
					ItemSlot slot = this.rewardsInventory[j];
					if (slot != null)
					{
						ItemSlotDisplayOnly displaySlot = slot as ItemSlotDisplayOnly;
						string text;
						if (displaySlot == null)
						{
							ItemStack itemstack2 = slot.Itemstack;
							text = ((itemstack2 != null) ? itemstack2.Collectible.Code.ToString() : null);
						}
						else if ((text = displaySlot.GetActualItemCode()) == null)
						{
							ItemStack itemstack3 = slot.Itemstack;
							text = ((itemstack3 != null) ? itemstack3.Collectible.Code.ToString() : null);
						}
						string itemCode = text;
						GuiComposer singleComposer4 = base.SingleComposer;
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler4 = new DefaultInterpolatedStringHandler(16, 1);
						defaultInterpolatedStringHandler4.AppendLiteral("reward_quantity_");
						defaultInterpolatedStringHandler4.AppendFormatted<int>(j);
						GuiElementNumberInput numberInput = GuiComposerHelpers.GetNumberInput(singleComposer4, defaultInterpolatedStringHandler4.ToStringAndClear());
						int count2;
						int rewardQuantity = int.TryParse((numberInput != null) ? numberInput.GetText() : null, out count2) ? count2 : localRewards[j].Amount;
						DialogQuestEditor.QuestRewardDtoWithNullableCode reward = new DialogQuestEditor.QuestRewardDtoWithNullableCode
						{
							Code = itemCode,
							Amount = rewardQuantity
						};
						ItemStack itemstack4 = slot.Itemstack;
						ItemStack setStack2 = (itemstack4 != null) ? itemstack4.Clone() : null;
						if (setStack2 != null && setStack2.Attributes != null)
						{
							foreach (string ignoreAttr2 in QuestNetworkHandler.NbtAttributesToIgnore)
							{
								if (setStack2.Attributes.HasAttribute(ignoreAttr2))
								{
									setStack2.Attributes.RemoveAttribute(ignoreAttr2);
								}
							}
							if (setStack2.Attributes.Count != 0)
							{
								using (MemoryStream ms2 = new MemoryStream())
								{
									using (BinaryWriter writer2 = new BinaryWriter(ms2))
									{
										setStack2.Attributes.ToBytes(writer2);
										reward.Nbt = Convert.ToBase64String(ms2.ToArray());
									}
								}
							}
						}
						this.formRewards.Add(reward);
					}
				}
			}
		}

		// Token: 0x06000563 RID: 1379 RVA: 0x000225F8 File Offset: 0x000207F8
		private double AddQuestEditorFields(GuiComposer composer, double yPos)
		{
			DialogQuestEditor.<>c__DisplayClass40_0 CS$<>8__locals1 = new DialogQuestEditor.<>c__DisplayClass40_0();
			CS$<>8__locals1.<>4__this = this;
			CS$<>8__locals1.composer = composer;
			double innerYPos = yPos + 5.0;
			int fieldHeight = 30;
			int gap = 5;
			int leftMargin = 5;
			GuiElementDynamicTextHelper.AddDynamicText(CS$<>8__locals1.composer, "Info", CairoFont.WhiteSmallishText(), DialogQuestEditor.CreateChildBounds((double)leftMargin, innerYPos, 90.0, (double)fieldHeight), null);
			innerYPos += (double)(fieldHeight + gap);
			GuiElementDynamicTextHelper.AddDynamicText(CS$<>8__locals1.composer, "Title:", CairoFont.WhiteSmallText(), DialogQuestEditor.CreateChildBounds((double)leftMargin, innerYPos + 5.0, 90.0, (double)fieldHeight), null);
			GuiComposerHelpers.AddTextInput(CS$<>8__locals1.composer, DialogQuestEditor.CreateChildBounds((double)(leftMargin * 2 + 90), innerYPos, 300.0, (double)fieldHeight), new Action<string>(this.BlankHandler), CairoFont.SmallTextInput().WithColor(new double[]
			{
				1.0,
				1.0,
				1.0,
				1.0
			}), "title");
			GuiComposerHelpers.GetTextInput(CS$<>8__locals1.composer, "title").SetPlaceHolderText("Title...");
			GuiComposerHelpers.GetTextInput(CS$<>8__locals1.composer, "title").SetMaxLength(50);
			GuiComposerHelpers.GetTextInput(CS$<>8__locals1.composer, "title").SetValue(this.formTitle, true);
			innerYPos += (double)(fieldHeight + gap);
			GuiElementDynamicTextHelper.AddDynamicText(CS$<>8__locals1.composer, "Description:", CairoFont.WhiteSmallText(), DialogQuestEditor.CreateChildBounds((double)leftMargin, innerYPos + 5.0, 90.0, (double)fieldHeight), null);
			GuiComposerHelpers.AddTextArea(CS$<>8__locals1.composer, DialogQuestEditor.CreateChildBounds((double)(leftMargin * 2 + 90), innerYPos, 600.0, (double)(fieldHeight * 3)), null, CairoFont.SmallTextInput().WithColor(new double[]
			{
				1.0,
				1.0,
				1.0,
				1.0
			}).WithFontSize(11f), "description");
			GuiComposerHelpers.GetTextArea(CS$<>8__locals1.composer, "description").SetMaxHeight(fieldHeight * 3);
			GuiComposerHelpers.GetTextArea(CS$<>8__locals1.composer, "description").Autoheight = false;
			innerYPos += (double)(fieldHeight * 3 + gap);
			GuiElementDynamicTextHelper.AddDynamicText(CS$<>8__locals1.composer, "Recurrence:", CairoFont.WhiteSmallText(), DialogQuestEditor.CreateChildBounds((double)leftMargin, innerYPos + 5.0, 90.0, (double)fieldHeight), null);
			GuiComposerHelpers.AddDropDown(CS$<>8__locals1.composer, new string[]
			{
				"daily",
				"weekly",
				"monthly",
				"seasonal"
			}, new string[]
			{
				"Daily",
				"Weekly",
				"Monthly",
				"Seasonal"
			}, 0, new SelectionChangedDelegate(this.OnRecurrenceChanged), DialogQuestEditor.CreateChildBounds((double)(leftMargin * 2 + 90), innerYPos, 100.0, (double)fieldHeight), "type");
			GuiComposerHelpers.GetDropDown(CS$<>8__locals1.composer, "type").SetSelectedValue(new string[]
			{
				this.formType
			});
			innerYPos += (double)(fieldHeight + gap * 2);
			innerYPos += (double)gap;
			if (this.formType == "seasonal")
			{
				GuiComposerHelpers.AddSwitch(CS$<>8__locals1.composer, new Action<bool>(this.OnToggleIgtSwitch), DialogQuestEditor.CreateChildBounds((double)(leftMargin * 2), innerYPos, 20.0, (double)fieldHeight), "inGameTime", 20.0, 0.0);
				GuiElementDynamicTextHelper.AddDynamicText(CS$<>8__locals1.composer, "In-game time?", CairoFont.WhiteSmallText(), DialogQuestEditor.CreateChildBounds((double)(leftMargin * 2 + 20 + leftMargin), innerYPos, 150.0, (double)fieldHeight), null);
				GuiComposerHelpers.GetSwitch(CS$<>8__locals1.composer, "inGameTime").SetValue(this.formIgt);
				innerYPos += (double)(fieldHeight + gap);
			}
			else
			{
				innerYPos -= 10.0;
			}
			if (this.formType == "daily")
			{
				innerYPos += 10.0;
				GuiComposerHelpers.AddSwitch(CS$<>8__locals1.composer, new Action<bool>(this.OnToggleRepeatingSwitch), DialogQuestEditor.CreateChildBounds((double)(leftMargin * 2), innerYPos, 20.0, (double)fieldHeight), "repeating", 20.0, 0.0);
				GuiElementDynamicTextHelper.AddDynamicText(CS$<>8__locals1.composer, "Repeat weekly?", CairoFont.WhiteSmallText(), DialogQuestEditor.CreateChildBounds((double)(leftMargin * 2 + 20 + leftMargin), innerYPos, 150.0, (double)fieldHeight), null);
				GuiComposerHelpers.GetSwitch(CS$<>8__locals1.composer, "repeating").SetValue(this.formRepeating);
				innerYPos += (double)(fieldHeight + gap);
			}
			GuiElementDynamicTextHelper.AddDynamicText(CS$<>8__locals1.composer, "Start date:", CairoFont.WhiteSmallText(), DialogQuestEditor.CreateChildBounds((double)leftMargin, innerYPos + 5.0, 90.0, (double)fieldHeight), null);
			int startDateX = leftMargin * 2 + 90;
			CS$<>8__locals1.cal = this.capi.World.Calendar;
			if (this.formIgt)
			{
				int igYear = CS$<>8__locals1.cal.Year + 1;
				int dayOfYear = CS$<>8__locals1.cal.DayOfYear;
				int igMonth = Math.Clamp(dayOfYear / CS$<>8__locals1.cal.DaysPerMonth + 1, 1, 12);
				int igDay = Math.Clamp(dayOfYear % CS$<>8__locals1.cal.DaysPerMonth + 1, 1, CS$<>8__locals1.cal.DaysPerMonth);
				CS$<>8__locals1.minDate = new DateTime(igYear, igMonth, igDay);
			}
			else
			{
				CS$<>8__locals1.minDate = ((this.serverLocalTime > 0L) ? DateTimeOffset.FromUnixTimeSeconds(this.serverLocalTime).ToOffset(TimeSpan.FromHours(this.serverTimezoneOffset)).Date : DateTime.Today);
			}
			Func<int, string> func;
			if (!this.formIgt)
			{
				func = ((int y) => y.ToString());
			}
			else
			{
				func = ((int y) => y.ToString("D4"));
			}
			Func<int, string> formatYear = func;
			if (this.formType == "weekly")
			{
				int sy;
				int searchYear = int.TryParse(this.formStartYear, out sy) ? sy : CS$<>8__locals1.minDate.Year;
				int sm;
				int searchMonth = int.TryParse(this.formStartMonth, out sm) ? sm : CS$<>8__locals1.minDate.Month;
				if (CS$<>8__locals1.<AddQuestEditorFields>g__GetSundaysForMonth|2(searchYear, searchMonth).Length == 0)
				{
					if (++searchMonth > 12)
					{
						searchMonth = 1;
						searchYear++;
					}
					int maxYear = CS$<>8__locals1.minDate.Year + 5;
					while (searchYear <= maxYear && CS$<>8__locals1.<AddQuestEditorFields>g__GetSundaysForMonth|2(searchYear, searchMonth).Length == 0)
					{
						if (++searchMonth > 12)
						{
							searchMonth = 1;
							searchYear++;
						}
					}
					if (searchYear <= maxYear)
					{
						this.formStartYear = formatYear(searchYear);
						this.formStartMonth = searchMonth.ToString("D2");
					}
				}
			}
			int m;
			string[] yearValues = (this.formType == "weekly") ? Enumerable.Range(CS$<>8__locals1.minDate.Year, 6).Where(delegate(int y)
			{
				int startM = (y == CS$<>8__locals1.minDate.Year) ? CS$<>8__locals1.minDate.Month : 1;
				return Enumerable.Range(startM, 13 - startM).Any((int m) => CS$<>8__locals1.<AddQuestEditorFields>g__GetSundaysForMonth|2(y, m).Length != 0);
			}).Select(formatYear).ToArray<string>() : Enumerable.Range(CS$<>8__locals1.minDate.Year, 6).Select(formatYear).ToArray<string>();
			if (yearValues.Length == 0)
			{
				yearValues = new string[]
				{
					formatYear(CS$<>8__locals1.minDate.Year)
				};
			}
			string[] array;
			if (!this.formIgt)
			{
				array = yearValues;
			}
			else
			{
				array = (from v in yearValues
				select (int.Parse(v) - 1).ToString("D4")).ToArray<string>();
			}
			string[] yearNames = array;
			if (!yearValues.Contains(this.formStartYear))
			{
				this.formStartYear = yearValues[0];
			}
			int selectedYear = int.Parse(this.formStartYear);
			int num = 12;
			int firstMonth = (selectedYear == CS$<>8__locals1.minDate.Year) ? CS$<>8__locals1.minDate.Month : 1;
			int monthCount = num - firstMonth + 1;
			CS$<>8__locals1.allMonthNames = (from m in DateTimeFormatInfo.InvariantInfo.MonthNames
			where !string.IsNullOrEmpty(m)
			select m).ToArray<string>();
			string[] monthValues = (from m in Enumerable.Range(firstMonth, monthCount)
			select m.ToString("D2")).ToArray<string>();
			string[] monthNames = (from m in Enumerable.Range(firstMonth, monthCount)
			select CS$<>8__locals1.allMonthNames[m - 1]).ToArray<string>();
			if (!monthValues.Contains(this.formStartMonth))
			{
				this.formStartMonth = monthValues[0];
			}
			int selectedMonth = int.Parse(this.formStartMonth);
			bool flag = selectedYear < CS$<>8__locals1.minDate.Year || (selectedYear == CS$<>8__locals1.minDate.Year && selectedMonth < CS$<>8__locals1.minDate.Month);
			bool isMinMonth = selectedYear == CS$<>8__locals1.minDate.Year && selectedMonth == CS$<>8__locals1.minDate.Month;
			int daysInMonth = this.formIgt ? CS$<>8__locals1.cal.DaysPerMonth : DateTime.DaysInMonth(selectedYear, selectedMonth);
			int firstDay = flag ? (daysInMonth + 1) : (isMinMonth ? CS$<>8__locals1.minDate.Day : 1);
			int count = Math.Max(0, daysInMonth - firstDay + 1);
			string[] dayValues = (from d in Enumerable.Range(firstDay, count)
			select d.ToString("D2")).ToArray<string>();
			string[] sundayValues = CS$<>8__locals1.<AddQuestEditorFields>g__GetSundaysForMonth|2(selectedYear, selectedMonth);
			GuiComposerHelpers.AddDropDown(CS$<>8__locals1.composer, yearValues, yearNames, 0, new SelectionChangedDelegate(this.OnStartDateChanged), DialogQuestEditor.CreateChildBounds((double)startDateX, innerYPos, 80.0, (double)fieldHeight), "startYear");
			GuiComposerHelpers.GetDropDown(CS$<>8__locals1.composer, "startYear").SetSelectedValue(new string[]
			{
				this.formStartYear
			});
			startDateX += leftMargin + 80;
			GuiComposerHelpers.AddDropDown(CS$<>8__locals1.composer, monthValues, monthNames, 0, new SelectionChangedDelegate(this.OnStartDateChanged), DialogQuestEditor.CreateChildBounds((double)startDateX, innerYPos, 110.0, (double)fieldHeight), "startMonth");
			GuiComposerHelpers.GetDropDown(CS$<>8__locals1.composer, "startMonth").SetSelectedValue(new string[]
			{
				this.formStartMonth
			});
			startDateX += leftMargin + 110;
			string[] array2;
			if (!(this.formType == "weekly"))
			{
				if (!(this.formType == "monthly"))
				{
					array2 = dayValues;
				}
				else
				{
					(array2 = new string[1])[0] = "1";
				}
			}
			else
			{
				array2 = sundayValues;
			}
			string[] valuesToUse = array2;
			if (valuesToUse.Length != 0 && !valuesToUse.Contains(this.formStartDay))
			{
				this.formStartDay = valuesToUse[0];
			}
			this.RecalculateEndDate();
			GuiComposerHelpers.AddDropDown(CS$<>8__locals1.composer, valuesToUse, valuesToUse, 0, new SelectionChangedDelegate(this.OnStartDateChanged), DialogQuestEditor.CreateChildBounds((double)startDateX, innerYPos, 60.0, (double)fieldHeight), "startDay");
			GuiComposerHelpers.GetDropDown(CS$<>8__locals1.composer, "startDay").SetSelectedValue(new string[]
			{
				this.formStartDay
			});
			startDateX += leftMargin + 60;
			int startYearVal;
			int startMonthVal;
			int startDayVal;
			if (this.formIgt && int.TryParse(this.formStartYear, out startYearVal) && int.TryParse(this.formStartMonth, out startMonthVal) && int.TryParse(this.formStartDay, out startDayVal))
			{
				string startEstimate = this.CalculateRealTimeEstimate(startYearVal, startMonthVal, startDayVal, true);
				GuiElementDynamicTextHelper.AddDynamicText(CS$<>8__locals1.composer, startEstimate, CairoFont.WhiteSmallText().WithColor(new double[]
				{
					0.7,
					0.9,
					1.0,
					1.0
				}), DialogQuestEditor.CreateChildBounds((double)startDateX, innerYPos + 5.0, 200.0, (double)fieldHeight), null);
			}
			innerYPos += (double)(fieldHeight + gap);
			GuiElementDynamicTextHelper.AddDynamicText(CS$<>8__locals1.composer, "End date:", CairoFont.WhiteSmallText(), DialogQuestEditor.CreateChildBounds((double)leftMargin, innerYPos + 5.0, 90.0, (double)fieldHeight), null);
			int endDateX = leftMargin * 2 + 90;
			if (!yearValues.Contains(this.formEndYear))
			{
				this.formEndYear = yearValues[0];
			}
			int endSelectedYear = int.Parse(this.formEndYear);
			int endFirstMonth = (endSelectedYear == selectedYear) ? selectedMonth : 1;
			int endMonthCount = 12 - endFirstMonth + 1;
			string[] endMonthValues = (from m in Enumerable.Range(endFirstMonth, endMonthCount)
			select m.ToString("D2")).ToArray<string>();
			string[] endMonthNames = (from m in Enumerable.Range(endFirstMonth, endMonthCount)
			select CS$<>8__locals1.allMonthNames[m - 1]).ToArray<string>();
			if (!endMonthValues.Contains(this.formEndMonth))
			{
				this.formEndMonth = endMonthValues[0];
			}
			int endSelectedMonth = int.Parse(this.formEndMonth);
			bool flag2 = endSelectedYear < selectedYear || (endSelectedYear == selectedYear && endSelectedMonth < selectedMonth);
			bool endIsStartMonth = endSelectedYear == selectedYear && endSelectedMonth == selectedMonth;
			int endDaysInMonth = this.formIgt ? CS$<>8__locals1.cal.DaysPerMonth : DateTime.DaysInMonth(endSelectedYear, endSelectedMonth);
			int endFirstDay = flag2 ? (endDaysInMonth + 1) : (endIsStartMonth ? int.Parse(this.formStartDay) : 1);
			int endCount = Math.Max(0, endDaysInMonth - endFirstDay + 1);
			string[] endDayValues = (from d in Enumerable.Range(endFirstDay, endCount)
			select d.ToString("D2")).ToArray<string>();
			if (endDayValues.Length != 0 && !endDayValues.Contains(this.formEndDay))
			{
				this.formEndDay = endDayValues[0];
			}
			GuiComposerHelpers.AddDropDown(CS$<>8__locals1.composer, yearValues, yearNames, 0, new SelectionChangedDelegate(this.OnEndDateChanged), DialogQuestEditor.CreateChildBounds((double)endDateX, innerYPos, 80.0, (double)fieldHeight), "endYear");
			GuiComposerHelpers.GetDropDown(CS$<>8__locals1.composer, "endYear").SetSelectedValue(new string[]
			{
				this.formEndYear
			});
			endDateX += leftMargin + 80;
			GuiComposerHelpers.AddDropDown(CS$<>8__locals1.composer, endMonthValues, endMonthNames, 0, new SelectionChangedDelegate(this.OnEndDateChanged), DialogQuestEditor.CreateChildBounds((double)endDateX, innerYPos, 110.0, (double)fieldHeight), "endMonth");
			GuiComposerHelpers.GetDropDown(CS$<>8__locals1.composer, "endMonth").SetSelectedValue(new string[]
			{
				this.formEndMonth
			});
			endDateX += leftMargin + 110;
			GuiComposerHelpers.AddDropDown(CS$<>8__locals1.composer, endDayValues, endDayValues, 0, new SelectionChangedDelegate(this.OnEndDateChanged), DialogQuestEditor.CreateChildBounds((double)endDateX, innerYPos, 60.0, (double)fieldHeight), "endDay");
			GuiComposerHelpers.GetDropDown(CS$<>8__locals1.composer, "endDay").SetSelectedValue(new string[]
			{
				this.formEndDay
			});
			endDateX += leftMargin + 60;
			int endYearVal;
			int endMonthVal;
			int endDayVal;
			if (this.formIgt && int.TryParse(this.formEndYear, out endYearVal) && int.TryParse(this.formEndMonth, out endMonthVal) && int.TryParse(this.formEndDay, out endDayVal))
			{
				string endEstimate = this.CalculateRealTimeEstimate(endYearVal, endMonthVal, endDayVal, false);
				GuiElementDynamicTextHelper.AddDynamicText(CS$<>8__locals1.composer, endEstimate, CairoFont.WhiteSmallText().WithColor(new double[]
				{
					0.7,
					0.9,
					1.0,
					1.0
				}), DialogQuestEditor.CreateChildBounds((double)endDateX, innerYPos + 5.0, 200.0, (double)fieldHeight), null);
			}
			innerYPos += (double)(fieldHeight + gap * 2);
			double objectivesCheckpoint = innerYPos;
			GuiElementDynamicTextHelper.AddDynamicText(CS$<>8__locals1.composer, "Objectives", CairoFont.WhiteSmallishText(), DialogQuestEditor.CreateChildBounds((double)leftMargin, innerYPos, 120.0, (double)fieldHeight), null);
			innerYPos += (double)(fieldHeight + gap);
			if (this.objectiveInventory != null)
			{
				for (int i = 0; i < this.objectiveInventory.Count; i++)
				{
					this.objectiveInventory[i].Itemstack = null;
				}
				int j = 0;
				while (j < this.formObjectives.Count && j < 10)
				{
					QuestObjectiveDto obj = this.formObjectives[j];
					if (obj.Type == "turn_in" && obj.AcceptedItems != null)
					{
						int itemIdx = 0;
						while (itemIdx < obj.AcceptedItems.Count && itemIdx < 100)
						{
							QuestAcceptedItemDto acceptedItem = obj.AcceptedItems[itemIdx];
							int slotId = j * 100 + itemIdx;
							CollectibleObject collectible = this.capi.World.GetItem(new AssetLocation(acceptedItem.Code));
							if (collectible == null)
							{
								collectible = this.capi.World.GetBlock(new AssetLocation(acceptedItem.Code));
							}
							if (collectible != null)
							{
								ItemStack itemStack = new ItemStack(collectible, 1);
								if (!string.IsNullOrEmpty(acceptedItem.Nbt))
								{
									using (MemoryStream ms = new MemoryStream(Convert.FromBase64String(acceptedItem.Nbt)))
									{
										using (BinaryReader reader = new BinaryReader(ms))
										{
											itemStack.Attributes = new TreeAttribute();
											itemStack.Attributes.FromBytes(reader);
										}
									}
								}
								this.objectiveInventory[slotId].Itemstack = itemStack;
							}
							itemIdx++;
						}
					}
					j++;
				}
			}
			for (int k = 0; k < this.formObjectives.Count; k++)
			{
				QuestObjectiveDto objective = this.formObjectives[k];
				int objX = leftMargin * 2;
				GuiComposer composer2 = CS$<>8__locals1.composer;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(1, 1);
				defaultInterpolatedStringHandler.AppendFormatted<int>(k + 1);
				defaultInterpolatedStringHandler.AppendLiteral(".");
				GuiElementDynamicTextHelper.AddDynamicText(composer2, defaultInterpolatedStringHandler.ToStringAndClear(), CairoFont.WhiteSmallText(), DialogQuestEditor.CreateChildBounds((double)(leftMargin * 2), innerYPos + 10.0, 30.0, (double)fieldHeight), null);
				if (this.formObjectives.Count != 1)
				{
					int localI = k;
					GuiComposerHelpers.AddIconButton(CS$<>8__locals1.composer, "none", delegate(bool something)
					{
						CS$<>8__locals1.<>4__this.DeleteObjective(localI);
					}, DialogQuestEditor.CreateChildBounds((double)leftMargin, innerYPos + 40.0, 25.0, 25.0), null);
				}
				objX += 30;
				GuiComposer composer3 = CS$<>8__locals1.composer;
				string[] array3 = new string[]
				{
					"turn_in",
					"kill"
				};
				string[] array4 = new string[]
				{
					"Turn in",
					"Kill"
				};
				int num2 = 0;
				SelectionChangedDelegate selectionChangedDelegate = new SelectionChangedDelegate(this.OnObjectiveTypeChanged);
				ElementBounds elementBounds = DialogQuestEditor.CreateChildBounds((double)(leftMargin * 2 + 30), innerYPos + (double)gap, 80.0, (double)fieldHeight);
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(15, 1);
				defaultInterpolatedStringHandler2.AppendLiteral("objective_type_");
				defaultInterpolatedStringHandler2.AppendFormatted<int>(k);
				GuiComposerHelpers.AddDropDown(composer3, array3, array4, num2, selectionChangedDelegate, elementBounds, defaultInterpolatedStringHandler2.ToStringAndClear());
				GuiComposer composer4 = CS$<>8__locals1.composer;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler3 = new DefaultInterpolatedStringHandler(15, 1);
				defaultInterpolatedStringHandler3.AppendLiteral("objective_type_");
				defaultInterpolatedStringHandler3.AppendFormatted<int>(k);
				GuiComposerHelpers.GetDropDown(composer4, defaultInterpolatedStringHandler3.ToStringAndClear()).SetSelectedValue(new string[]
				{
					this.formObjectives[k].Type
				});
				GuiComposer composer5 = CS$<>8__locals1.composer;
				ElementBounds elementBounds2 = DialogQuestEditor.CreateChildBounds((double)(leftMargin * 2 + 30), innerYPos + (double)fieldHeight + (double)(gap * 2), 80.0, (double)fieldHeight);
				Action<string> action = null;
				CairoFont cairoFont = CairoFont.SmallTextInput().WithColor(new double[]
				{
					1.0,
					1.0,
					1.0,
					1.0
				});
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler4 = new DefaultInterpolatedStringHandler(19, 1);
				defaultInterpolatedStringHandler4.AppendLiteral("objective_quantity_");
				defaultInterpolatedStringHandler4.AppendFormatted<int>(k);
				GuiComposerHelpers.AddNumberInput(composer5, elementBounds2, action, cairoFont, defaultInterpolatedStringHandler4.ToStringAndClear());
				GuiComposer composer6 = CS$<>8__locals1.composer;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler5 = new DefaultInterpolatedStringHandler(19, 1);
				defaultInterpolatedStringHandler5.AppendLiteral("objective_quantity_");
				defaultInterpolatedStringHandler5.AppendFormatted<int>(k);
				GuiComposerHelpers.GetNumberInput(composer6, defaultInterpolatedStringHandler5.ToStringAndClear()).SetPlaceHolderText("Quantity");
				GuiComposer composer7 = CS$<>8__locals1.composer;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler6 = new DefaultInterpolatedStringHandler(19, 1);
				defaultInterpolatedStringHandler6.AppendLiteral("objective_quantity_");
				defaultInterpolatedStringHandler6.AppendFormatted<int>(k);
				GuiComposerHelpers.GetNumberInput(composer7, defaultInterpolatedStringHandler6.ToStringAndClear()).SetMaxLength(4);
				GuiComposer composer8 = CS$<>8__locals1.composer;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler7 = new DefaultInterpolatedStringHandler(19, 1);
				defaultInterpolatedStringHandler7.AppendLiteral("objective_quantity_");
				defaultInterpolatedStringHandler7.AppendFormatted<int>(k);
				GuiElementEditableTextBase numberInput = GuiComposerHelpers.GetNumberInput(composer8, defaultInterpolatedStringHandler7.ToStringAndClear());
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler8 = new DefaultInterpolatedStringHandler(0, 1);
				defaultInterpolatedStringHandler8.AppendFormatted<int>(this.formObjectives[k].Count);
				numberInput.SetValue(defaultInterpolatedStringHandler8.ToStringAndClear(), true);
				objX += 85;
				if (this.formObjectives[k].Type == "turn_in")
				{
					int totalSlotsToShow = objective.AcceptedItems.Count + 1;
					int itemIdx2 = 0;
					while (itemIdx2 < totalSlotsToShow && itemIdx2 < 100)
					{
						int slotId2 = k * 100 + itemIdx2;
						if (this.objectiveInventory != null)
						{
							GuiComposer composer9 = CS$<>8__locals1.composer;
							IInventory inventory = this.objectiveInventory;
							Action<object> action2;
							if ((action2 = CS$<>8__locals1.<>9__16) == null)
							{
								action2 = (CS$<>8__locals1.<>9__16 = delegate(object slotIdParam)
								{
									new ItemSlotDisplayOnly(CS$<>8__locals1.<>4__this.objectiveInventory);
								});
							}
							int num3 = 1;
							int[] array5 = new int[]
							{
								slotId2
							};
							ElementBounds elementBounds3 = DialogQuestEditor.CreateChildBounds((double)objX, innerYPos, 49.0, 49.0);
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler9 = new DefaultInterpolatedStringHandler(16, 2);
							defaultInterpolatedStringHandler9.AppendLiteral("objective_item_");
							defaultInterpolatedStringHandler9.AppendFormatted<int>(k);
							defaultInterpolatedStringHandler9.AppendLiteral("_");
							defaultInterpolatedStringHandler9.AppendFormatted<int>(itemIdx2);
							GuiComposerHelpers.AddItemSlotGrid(composer9, inventory, action2, num3, array5, elementBounds3, defaultInterpolatedStringHandler9.ToStringAndClear());
						}
						objX += 49;
						if ((itemIdx2 + 1) % 6 == 0)
						{
							innerYPos += 49.0;
							objX = leftMargin * 2 + 115;
						}
						itemIdx2++;
					}
				}
				if (this.formObjectives[k].Type == "kill")
				{
					List<string> entityCodes = new List<string>();
					List<string> entityNames = new List<string>();
					int localI2 = k;
					IEnumerable<AssetLocation> source = from item in this.capi.World.SearchItems(new AssetLocation("*", "creature*"))
					select new AssetLocation(item.Code.Domain, item.CodeEndWithoutParts(1));
					Func<AssetLocation, EntityProperties> selector;
					if ((selector = CS$<>8__locals1.<>9__18) == null)
					{
						selector = (CS$<>8__locals1.<>9__18 = ((AssetLocation location) => CS$<>8__locals1.<>4__this.capi.World.GetEntityType(location)));
					}
					foreach (EntityProperties type2 in from type in source.Select(selector).OfType<EntityProperties>()
					orderby Lang.Get("item-creature-" + type.Code.Path, Array.Empty<object>())
					orderby type.Code.FirstCodePart()
					select type)
					{
						if (!type2.Code.Path.Contains("butterfly"))
						{
							entityCodes.Add(type2.Code.ToString());
							entityNames.Add(Lang.Get("item-creature-" + type2.Code.Path, Array.Empty<object>()));
						}
					}
					GuiComposer composer10 = CS$<>8__locals1.composer;
					string[] array6 = entityCodes.ToArray();
					string[] array7 = entityNames.ToArray();
					int num4 = -1;
					SelectionChangedDelegate selectionChangedDelegate2 = null;
					ElementBounds elementBounds4 = DialogQuestEditor.CreateChildBounds((double)objX, innerYPos + (double)gap, 300.0, (double)fieldHeight);
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler10 = new DefaultInterpolatedStringHandler(23, 1);
					defaultInterpolatedStringHandler10.AppendLiteral("objective_targetSelect_");
					defaultInterpolatedStringHandler10.AppendFormatted<int>(localI2);
					GuiComposerHelpers.AddMultiSelectDropDown(composer10, array6, array7, num4, selectionChangedDelegate2, elementBounds4, defaultInterpolatedStringHandler10.ToStringAndClear());
				}
				innerYPos += (double)(fieldHeight * 2 + gap * 3);
			}
			if (this.formObjectives.Count < 5)
			{
				GuiComposerHelpers.AddButton(CS$<>8__locals1.composer, "Add", new ActionConsumable(this.OnAddObjective), DialogQuestEditor.CreateChildBounds((double)(leftMargin * 2), innerYPos, 80.0, (double)fieldHeight), CairoFont.WhiteSmallText().WithFontSize(11f).WithOrientation(2), 3, "addObjective");
			}
			double objectivesEndY = innerYPos;
			innerYPos = objectivesCheckpoint;
			GuiElementDynamicTextHelper.AddDynamicText(CS$<>8__locals1.composer, "Rewards", CairoFont.WhiteSmallishText(), DialogQuestEditor.CreateChildBounds((double)(leftMargin + 460), innerYPos, 120.0, (double)fieldHeight), null);
			innerYPos += (double)(fieldHeight + gap);
			if (this.rewardsInventory != null)
			{
				for (int l = 0; l < this.rewardsInventory.Count; l++)
				{
					this.rewardsInventory[l].Itemstack = null;
				}
				m = 0;
				while (m < this.formRewards.Count && m < this.rewardsInventory.Count)
				{
					DialogQuestEditor.QuestRewardDtoWithNullableCode reward = this.formRewards[m];
					if (reward.Code != null)
					{
						CollectibleObject collectible2 = this.capi.World.GetItem(new AssetLocation(reward.Code));
						if (collectible2 == null)
						{
							collectible2 = this.capi.World.GetBlock(new AssetLocation(reward.Code));
						}
						if (collectible2 == null && reward.Code == "game:grspoints")
						{
							collectible2 = this.capi.World.GetItem(new AssetLocation("game:paper-parchment"));
						}
						if (collectible2 != null)
						{
							ItemStack itemStack2 = new ItemStack(collectible2, 1);
							if (!string.IsNullOrEmpty(reward.Nbt))
							{
								using (MemoryStream ms2 = new MemoryStream(Convert.FromBase64String(reward.Nbt)))
								{
									using (BinaryReader reader2 = new BinaryReader(ms2))
									{
										itemStack2.Attributes = new TreeAttribute();
										itemStack2.Attributes.FromBytes(reader2);
									}
								}
							}
							ItemSlotDisplayOnly displaySlot = this.rewardsInventory[m] as ItemSlotDisplayOnly;
							if (displaySlot != null)
							{
								displaySlot.SetItemstack(itemStack2, reward.Code);
							}
							else
							{
								this.rewardsInventory[m].Itemstack = itemStack2;
							}
						}
					}
					m++;
				}
			}
			for (int n = 0; n < this.formRewards.Count; n++)
			{
				int localI = n;
				DialogQuestEditor.QuestRewardDtoWithNullableCode questRewardDtoWithNullableCode = this.formRewards[n];
				int objX2 = leftMargin * 2 + 460;
				GuiComposer composer11 = CS$<>8__locals1.composer;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler11 = new DefaultInterpolatedStringHandler(1, 1);
				defaultInterpolatedStringHandler11.AppendFormatted<int>(n + 1);
				defaultInterpolatedStringHandler11.AppendLiteral(".");
				GuiElementDynamicTextHelper.AddDynamicText(composer11, defaultInterpolatedStringHandler11.ToStringAndClear(), CairoFont.WhiteSmallText(), DialogQuestEditor.CreateChildBounds((double)objX2, innerYPos + 10.0, 30.0, (double)fieldHeight), null);
				if (this.formRewards.Count != 1)
				{
					GuiComposerHelpers.AddIconButton(CS$<>8__locals1.composer, "none", delegate(bool something)
					{
						CS$<>8__locals1.<>4__this.DeleteReward(localI);
					}, DialogQuestEditor.CreateChildBounds((double)objX2, innerYPos + 40.0, 25.0, 25.0), null);
				}
				GuiComposer composer12 = CS$<>8__locals1.composer;
				string[] array8 = new string[]
				{
					"<Empty>",
					"grs",
					"tails",
					"crowns"
				};
				string[] array9 = new string[]
				{
					"EZ-Add",
					"GRS Points",
					"Tails",
					"Crowns"
				};
				int num5 = 0;
				SelectionChangedDelegate selectionChangedDelegate3 = delegate(string choice, bool test)
				{
					if (choice == "grs")
					{
						CS$<>8__locals1.<>4__this.SaveFormState();
						CS$<>8__locals1.<>4__this.formRewards[localI].Code = "game:grspoints";
						CS$<>8__locals1.<>4__this.SetupDialog();
					}
					if (choice == "tails")
					{
						CS$<>8__locals1.<>4__this.SaveFormState();
						if (CS$<>8__locals1.<>4__this.tailsDefinition != null)
						{
							CS$<>8__locals1.<>4__this.formRewards[localI].Code = CS$<>8__locals1.<>4__this.tailsDefinition.Code;
							CS$<>8__locals1.<>4__this.formRewards[localI].Nbt = CS$<>8__locals1.<>4__this.tailsDefinition.Nbt;
						}
						else
						{
							CS$<>8__locals1.<>4__this.capi.ShowChatMessage("[Quest Editor] Tails currency not configured on server");
						}
						CS$<>8__locals1.<>4__this.SetupDialog();
					}
					if (choice == "crowns")
					{
						CS$<>8__locals1.<>4__this.SaveFormState();
						if (CS$<>8__locals1.<>4__this.crownsDefinition != null)
						{
							CS$<>8__locals1.<>4__this.formRewards[localI].Code = CS$<>8__locals1.<>4__this.crownsDefinition.Code;
							CS$<>8__locals1.<>4__this.formRewards[localI].Nbt = CS$<>8__locals1.<>4__this.crownsDefinition.Nbt;
						}
						else
						{
							CS$<>8__locals1.<>4__this.capi.ShowChatMessage("[Quest Editor] Crowns currency not configured on server");
						}
						CS$<>8__locals1.<>4__this.SetupDialog();
					}
					GuiComposer composer18 = CS$<>8__locals1.composer;
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler19 = new DefaultInterpolatedStringHandler(12, 1);
					defaultInterpolatedStringHandler19.AppendLiteral("reward_type_");
					defaultInterpolatedStringHandler19.AppendFormatted<int>(localI);
					GuiComposerHelpers.GetDropDown(composer18, defaultInterpolatedStringHandler19.ToStringAndClear()).SetSelectedValue(new string[]
					{
						""
					});
				};
				ElementBounds elementBounds5 = DialogQuestEditor.CreateChildBounds((double)(objX2 + 30), innerYPos + (double)gap, 90.0, (double)fieldHeight);
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler12 = new DefaultInterpolatedStringHandler(12, 1);
				defaultInterpolatedStringHandler12.AppendLiteral("reward_type_");
				defaultInterpolatedStringHandler12.AppendFormatted<int>(localI);
				GuiComposerHelpers.AddDropDown(composer12, array8, array9, num5, selectionChangedDelegate3, elementBounds5, defaultInterpolatedStringHandler12.ToStringAndClear());
				GuiComposer composer13 = CS$<>8__locals1.composer;
				ElementBounds elementBounds6 = DialogQuestEditor.CreateChildBounds((double)(objX2 + 30), innerYPos + (double)fieldHeight + (double)(gap * 2), 90.0, (double)fieldHeight);
				Action<string> action3 = null;
				CairoFont cairoFont2 = CairoFont.SmallTextInput().WithColor(new double[]
				{
					1.0,
					1.0,
					1.0,
					1.0
				});
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler13 = new DefaultInterpolatedStringHandler(16, 1);
				defaultInterpolatedStringHandler13.AppendLiteral("reward_quantity_");
				defaultInterpolatedStringHandler13.AppendFormatted<int>(n);
				GuiComposerHelpers.AddNumberInput(composer13, elementBounds6, action3, cairoFont2, defaultInterpolatedStringHandler13.ToStringAndClear());
				GuiComposer composer14 = CS$<>8__locals1.composer;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler14 = new DefaultInterpolatedStringHandler(16, 1);
				defaultInterpolatedStringHandler14.AppendLiteral("reward_quantity_");
				defaultInterpolatedStringHandler14.AppendFormatted<int>(n);
				GuiComposerHelpers.GetNumberInput(composer14, defaultInterpolatedStringHandler14.ToStringAndClear()).SetPlaceHolderText("Quantity");
				GuiComposer composer15 = CS$<>8__locals1.composer;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler15 = new DefaultInterpolatedStringHandler(16, 1);
				defaultInterpolatedStringHandler15.AppendLiteral("reward_quantity_");
				defaultInterpolatedStringHandler15.AppendFormatted<int>(n);
				GuiComposerHelpers.GetNumberInput(composer15, defaultInterpolatedStringHandler15.ToStringAndClear()).SetMaxLength(4);
				GuiComposer composer16 = CS$<>8__locals1.composer;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler16 = new DefaultInterpolatedStringHandler(16, 1);
				defaultInterpolatedStringHandler16.AppendLiteral("reward_quantity_");
				defaultInterpolatedStringHandler16.AppendFormatted<int>(n);
				GuiElementEditableTextBase numberInput2 = GuiComposerHelpers.GetNumberInput(composer16, defaultInterpolatedStringHandler16.ToStringAndClear());
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler17 = new DefaultInterpolatedStringHandler(0, 1);
				defaultInterpolatedStringHandler17.AppendFormatted<int>(this.formRewards[n].Amount);
				numberInput2.SetValue(defaultInterpolatedStringHandler17.ToStringAndClear(), true);
				objX2 += 125;
				if (this.rewardsInventory != null)
				{
					GuiComposer composer17 = CS$<>8__locals1.composer;
					IInventory inventory2 = this.rewardsInventory;
					Action<object> action4;
					if ((action4 = CS$<>8__locals1.<>9__23) == null)
					{
						action4 = (CS$<>8__locals1.<>9__23 = delegate(object slotIdParam)
						{
							new ItemSlotDisplayOnly(CS$<>8__locals1.<>4__this.rewardsInventory);
						});
					}
					int num6 = 1;
					int[] array10 = new int[]
					{
						localI
					};
					ElementBounds elementBounds7 = DialogQuestEditor.CreateChildBounds((double)objX2, innerYPos, 49.0, 49.0);
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler18 = new DefaultInterpolatedStringHandler(12, 1);
					defaultInterpolatedStringHandler18.AppendLiteral("reward_item_");
					defaultInterpolatedStringHandler18.AppendFormatted<int>(localI);
					GuiComposerHelpers.AddItemSlotGrid(composer17, inventory2, action4, num6, array10, elementBounds7, defaultInterpolatedStringHandler18.ToStringAndClear());
				}
				innerYPos += (double)(fieldHeight * 2 + gap * 3);
			}
			if (this.formRewards.Count < 5)
			{
				GuiComposerHelpers.AddButton(CS$<>8__locals1.composer, "Add", new ActionConsumable(this.OnAddReward), DialogQuestEditor.CreateChildBounds((double)(leftMargin * 2 + 460), innerYPos, 80.0, (double)fieldHeight), CairoFont.WhiteSmallText().WithFontSize(11f).WithOrientation(2), 3, "addReward");
			}
			return Math.Max(objectivesEndY, innerYPos);
		}

		// Token: 0x06000564 RID: 1380 RVA: 0x0002433C File Offset: 0x0002253C
		private void DeleteObjective(int i)
		{
			this.SaveFormState();
			this.formObjectives.RemoveAt(i);
			this.SetupDialog();
		}

		// Token: 0x06000565 RID: 1381 RVA: 0x00024356 File Offset: 0x00022556
		private void DeleteReward(int i)
		{
			this.SaveFormState();
			this.formRewards.RemoveAt(i);
			this.SetupDialog();
		}

		// Token: 0x06000566 RID: 1382 RVA: 0x00024370 File Offset: 0x00022570
		private bool OnAddObjective()
		{
			this.SaveFormState();
			this.formObjectives.Add(new QuestObjectiveDto
			{
				Id = DialogQuestEditor.rnd.Next(1, 10000000),
				AcceptedItems = new List<QuestAcceptedItemDto>(),
				AcceptedTargets = new List<string>(),
				Count = 5,
				Type = "turn_in"
			});
			this.SetupDialog();
			return true;
		}

		// Token: 0x06000567 RID: 1383 RVA: 0x000243D8 File Offset: 0x000225D8
		private bool OnAddReward()
		{
			this.SaveFormState();
			this.formRewards.Add(new DialogQuestEditor.QuestRewardDtoWithNullableCode
			{
				Amount = 5
			});
			this.SetupDialog();
			return true;
		}

		// Token: 0x06000568 RID: 1384 RVA: 0x000243FE File Offset: 0x000225FE
		private void BlankHandler(string obj)
		{
		}

		// Token: 0x06000569 RID: 1385 RVA: 0x00024400 File Offset: 0x00022600
		private void OnObjectiveTypeChanged(string code, bool selected)
		{
			this.SaveFormState();
			this.SetupDialog();
		}

		// Token: 0x0600056A RID: 1386 RVA: 0x0002440E File Offset: 0x0002260E
		private void OnStartDateChanged(string code, bool selected)
		{
			this.SaveFormState();
			this.RecalculateEndDate();
			this.SetupDialog();
		}

		// Token: 0x0600056B RID: 1387 RVA: 0x00024424 File Offset: 0x00022624
		private void OnEndDateChanged(string code, bool selected)
		{
			if (this.formType != "seasonal")
			{
				return;
			}
			this.SaveFormState();
			int startYear;
			int startMonth;
			int startDay;
			int endYear;
			int endMonth;
			int endDay;
			if (int.TryParse(this.formStartYear, out startYear) && int.TryParse(this.formStartMonth, out startMonth) && int.TryParse(this.formStartDay, out startDay) && int.TryParse(this.formEndYear, out endYear) && int.TryParse(this.formEndMonth, out endMonth) && int.TryParse(this.formEndDay, out endDay))
			{
				DateTime startDate = new DateTime(startYear, startMonth, startDay);
				if (new DateTime(endYear, endMonth, endDay) < startDate)
				{
					this.formEndYear = this.formStartYear;
					this.formEndMonth = this.formStartMonth;
					this.formEndDay = this.formStartDay;
				}
			}
			this.SetupDialog();
		}

		// Token: 0x0600056C RID: 1388 RVA: 0x000244EE File Offset: 0x000226EE
		private void OnToggleIgtSwitch(bool isOn)
		{
			this.SaveFormState();
			this.formStartYear = "";
			this.formStartMonth = "";
			this.formStartDay = "";
			this.SetupDialog();
		}

		// Token: 0x0600056D RID: 1389 RVA: 0x0002451D File Offset: 0x0002271D
		private void OnToggleRepeatingSwitch(bool isOn)
		{
			this.SaveFormState();
			this.SetupDialog();
		}

		// Token: 0x0600056E RID: 1390 RVA: 0x0002452B File Offset: 0x0002272B
		private void OnRecurrenceChanged(string code, bool selected)
		{
			this.SaveFormState();
			if (code != "seasonal")
			{
				this.formIgt = false;
			}
			if (code != "daily")
			{
				this.formRepeating = false;
			}
			this.RecalculateEndDate();
			this.SetupDialog();
		}

		// Token: 0x0600056F RID: 1391 RVA: 0x00024568 File Offset: 0x00022768
		private void RecalculateEndDate()
		{
			int startYear;
			int startMonth;
			int startDay;
			if (!int.TryParse(this.formStartYear, out startYear) || !int.TryParse(this.formStartMonth, out startMonth) || !int.TryParse(this.formStartDay, out startDay))
			{
				return;
			}
			IClientGameCalendar cal = this.capi.World.Calendar;
			Func<int, string> func;
			if (!this.formIgt)
			{
				func = ((int y) => y.ToString());
			}
			else
			{
				func = ((int y) => y.ToString("D4"));
			}
			Func<int, string> formatYear = func;
			if (this.formType == "seasonal")
			{
				int endYear;
				int endMonth;
				int endDay;
				if (!int.TryParse(this.formEndYear, out endYear) || !int.TryParse(this.formEndMonth, out endMonth) || !int.TryParse(this.formEndDay, out endDay))
				{
					this.formEndYear = this.formStartYear;
					this.formEndMonth = this.formStartMonth;
					this.formEndDay = this.formStartDay;
					return;
				}
				DateTime t = new DateTime(startYear, startMonth, startDay);
				DateTime endDate = new DateTime(endYear, endMonth, endDay);
				if (t > endDate)
				{
					this.formEndYear = this.formStartYear;
					this.formEndMonth = this.formStartMonth;
					this.formEndDay = this.formStartDay;
					return;
				}
			}
			else
			{
				if (this.formType == "monthly")
				{
					int daysInMonth = this.formIgt ? cal.DaysPerMonth : DateTime.DaysInMonth(startYear, startMonth);
					this.formEndYear = this.formStartYear;
					this.formEndMonth = this.formStartMonth;
					this.formEndDay = daysInMonth.ToString("D2");
					return;
				}
				if (this.formType == "weekly")
				{
					DateTime startDate = new DateTime(startYear, startMonth, startDay);
					DateTime endDate2 = startDate.AddDays(6.0);
					this.formEndYear = formatYear(endDate2.Year);
					this.formEndMonth = endDate2.Month.ToString("D2");
					this.formEndDay = endDate2.Day.ToString("D2");
					return;
				}
				this.formEndYear = this.formStartYear;
				this.formEndMonth = this.formStartMonth;
				this.formEndDay = this.formStartDay;
			}
		}

		// Token: 0x06000570 RID: 1392 RVA: 0x000247A0 File Offset: 0x000229A0
		private string CalculateRealTimeEstimate(int targetYear, int targetMonth, int targetDay, bool isStartOfDay)
		{
			IClientGameCalendar calendar = this.capi.World.Calendar;
			int currentVsYear = calendar.Year;
			int currentDayOfYear = calendar.DayOfYear;
			float currentHourOfDay = (float)calendar.FullHourOfDay;
			int daysPerMonth = calendar.DaysPerMonth;
			int hoursPerDay = (int)calendar.HoursPerDay;
			float calendarSpeedMul = calendar.CalendarSpeedMul;
			double num = (double)(targetYear - 1);
			int targetDayOfYear = (targetMonth - 1) * daysPerMonth + (targetDay - 1);
			int daysPerYear = daysPerMonth * 12;
			double currentTotalHours = (double)((float)((currentVsYear * daysPerYear + currentDayOfYear) * hoursPerDay) + currentHourOfDay);
			double targetHourOfDay = (double)(isStartOfDay ? 0 : hoursPerDay);
			double inGameHoursUntilTarget = (num * (double)daysPerYear + (double)targetDayOfYear) * (double)hoursPerDay + targetHourOfDay - currentTotalHours;
			if (inGameHoursUntilTarget > 0.0)
			{
				double realMinutesPerInGameHour = 24.0 / (double)hoursPerDay / (double)calendarSpeedMul;
				double realMinutesUntilTarget = inGameHoursUntilTarget * realMinutesPerInGameHour;
				return DateTime.Now.AddMinutes(realMinutesUntilTarget).ToString("yyyy/MM/dd hh:mm tt");
			}
			if (!isStartOfDay)
			{
				return "(ending)";
			}
			return "(now)";
		}

		// Token: 0x06000571 RID: 1393 RVA: 0x00024880 File Offset: 0x00022A80
		private static ElementBounds CreateChildBounds(double x, double y, double width, double height)
		{
			return ElementBounds.Fixed(x, y, width, height);
		}

		// Token: 0x06000572 RID: 1394 RVA: 0x0002488B File Offset: 0x00022A8B
		private void OnTitleBarCloseClicked()
		{
			this.TryClose();
		}

		// Token: 0x06000573 RID: 1395 RVA: 0x00024894 File Offset: 0x00022A94
		private bool OnCancelClicked()
		{
			this.TryClose();
			return true;
		}

		// Token: 0x06000574 RID: 1396 RVA: 0x000248A0 File Offset: 0x00022AA0
		private bool OnDeleteClicked()
		{
			if (this.existingQuest == null)
			{
				this.capi.ShowChatMessage("Cannot delete - no quest selected");
				return false;
			}
			new ConfirmDeleteQuestDialog(this.capi, this.existingQuest.Title, delegate
			{
				QuestNetworkHandler questNetworkHandler = this.modSystem.QuestNetworkHandler;
				if (questNetworkHandler == null)
				{
					this.capi.ShowChatMessage("Quest network handler not available");
					return;
				}
				questNetworkHandler.OnQuestDeleteResponse = delegate(QuestDeleteResponsePacket response)
				{
					if (response.Success)
					{
						this.capi.ShowChatMessage("Quest deleted successfully");
						Action onQuestSaved = this.OnQuestSaved;
						if (onQuestSaved != null)
						{
							onQuestSaved();
						}
						this.TryClose();
					}
					else
					{
						this.capi.ShowChatMessage("Failed to delete quest: " + response.Message);
					}
					questNetworkHandler.OnQuestDeleteResponse = null;
				};
				IClientPlayer player = this.capi.World.Player;
				questNetworkHandler.DeleteQuest(player.PlayerUID, this.existingQuest.Id);
			}).TryOpen();
			return true;
		}

		// Token: 0x06000575 RID: 1397 RVA: 0x000248F0 File Offset: 0x00022AF0
		private bool OnSaveClicked(bool saveAs = false)
		{
			this.SaveFormState();
			if (string.IsNullOrWhiteSpace(this.formTitle))
			{
				this.capi.ShowChatMessage("Quest title is required");
				return false;
			}
			if (string.IsNullOrWhiteSpace(this.formDescription))
			{
				this.capi.ShowChatMessage("Quest description is required");
				return false;
			}
			if (this.formObjectives.Count == 0)
			{
				this.capi.ShowChatMessage("At least one objective is required");
				return false;
			}
			for (int i = 0; i < this.formObjectives.Count; i++)
			{
				QuestObjectiveDto obj = this.formObjectives[i];
				if (obj.Count <= 0)
				{
					ICoreClientAPI capi = this.capi;
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(43, 1);
					defaultInterpolatedStringHandler.AppendLiteral("Objective ");
					defaultInterpolatedStringHandler.AppendFormatted<int>(i + 1);
					defaultInterpolatedStringHandler.AppendLiteral(": Quantity must be greater than 0");
					capi.ShowChatMessage(defaultInterpolatedStringHandler.ToStringAndClear());
					return false;
				}
				if (obj.Type == "turn_in" && (obj.AcceptedItems == null || obj.AcceptedItems.Count == 0))
				{
					ICoreClientAPI capi2 = this.capi;
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(70, 1);
					defaultInterpolatedStringHandler2.AppendLiteral("Objective ");
					defaultInterpolatedStringHandler2.AppendFormatted<int>(i + 1);
					defaultInterpolatedStringHandler2.AppendLiteral(": At least one item must be specified for turn_in objectives");
					capi2.ShowChatMessage(defaultInterpolatedStringHandler2.ToStringAndClear());
					return false;
				}
				if (obj.Type == "kill" && (obj.AcceptedTargets == null || obj.AcceptedTargets.Count == 0))
				{
					ICoreClientAPI capi3 = this.capi;
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler3 = new DefaultInterpolatedStringHandler(69, 1);
					defaultInterpolatedStringHandler3.AppendLiteral("Objective ");
					defaultInterpolatedStringHandler3.AppendFormatted<int>(i + 1);
					defaultInterpolatedStringHandler3.AppendLiteral(": At least one target must be specified for kill objectives");
					capi3.ShowChatMessage(defaultInterpolatedStringHandler3.ToStringAndClear());
					return false;
				}
			}
			List<DialogQuestEditor.QuestRewardDtoWithNullableCode> validRewards = (from r in this.formRewards
			where !string.IsNullOrEmpty(r.Code)
			select r).ToList<DialogQuestEditor.QuestRewardDtoWithNullableCode>();
			if (validRewards.Count == 0)
			{
				this.capi.ShowChatMessage("At least one reward is required");
				return false;
			}
			using (List<DialogQuestEditor.QuestRewardDtoWithNullableCode>.Enumerator enumerator = validRewards.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					if (enumerator.Current.Amount <= 0)
					{
						this.capi.ShowChatMessage("All rewards must have a quantity greater than 0");
						return false;
					}
				}
			}
			int startYear;
			int startMonth;
			int startDay;
			if (!int.TryParse(this.formStartYear, out startYear) || !int.TryParse(this.formStartMonth, out startMonth) || !int.TryParse(this.formStartDay, out startDay))
			{
				this.capi.ShowChatMessage("Invalid start date");
				return false;
			}
			int endYear;
			int endMonth;
			int endDay;
			if (!int.TryParse(this.formEndYear, out endYear) || !int.TryParse(this.formEndMonth, out endMonth) || !int.TryParse(this.formEndDay, out endDay))
			{
				this.capi.ShowChatMessage("Invalid end date");
				return false;
			}
			string startsAt;
			string expiresAt;
			if (this.formIgt)
			{
				int vsStartYear = startYear - 1;
				int vsEndYear = endYear - 1;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler4 = new DefaultInterpolatedStringHandler(2, 3);
				defaultInterpolatedStringHandler4.AppendFormatted<int>(vsStartYear, "D4");
				defaultInterpolatedStringHandler4.AppendLiteral("-");
				defaultInterpolatedStringHandler4.AppendFormatted<int>(startMonth, "D2");
				defaultInterpolatedStringHandler4.AppendLiteral("-");
				defaultInterpolatedStringHandler4.AppendFormatted<int>(startDay, "D2");
				startsAt = defaultInterpolatedStringHandler4.ToStringAndClear();
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler5 = new DefaultInterpolatedStringHandler(2, 3);
				defaultInterpolatedStringHandler5.AppendFormatted<int>(vsEndYear, "D4");
				defaultInterpolatedStringHandler5.AppendLiteral("-");
				defaultInterpolatedStringHandler5.AppendFormatted<int>(endMonth, "D2");
				defaultInterpolatedStringHandler5.AppendLiteral("-");
				defaultInterpolatedStringHandler5.AppendFormatted<int>(endDay, "D2");
				expiresAt = defaultInterpolatedStringHandler5.ToStringAndClear();
			}
			else
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler6 = new DefaultInterpolatedStringHandler(2, 3);
				defaultInterpolatedStringHandler6.AppendFormatted<int>(startYear, "D4");
				defaultInterpolatedStringHandler6.AppendLiteral("-");
				defaultInterpolatedStringHandler6.AppendFormatted<int>(startMonth, "D2");
				defaultInterpolatedStringHandler6.AppendLiteral("-");
				defaultInterpolatedStringHandler6.AppendFormatted<int>(startDay, "D2");
				startsAt = defaultInterpolatedStringHandler6.ToStringAndClear();
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler7 = new DefaultInterpolatedStringHandler(2, 3);
				defaultInterpolatedStringHandler7.AppendFormatted<int>(endYear, "D4");
				defaultInterpolatedStringHandler7.AppendLiteral("-");
				defaultInterpolatedStringHandler7.AppendFormatted<int>(endMonth, "D2");
				defaultInterpolatedStringHandler7.AppendLiteral("-");
				defaultInterpolatedStringHandler7.AppendFormatted<int>(endDay, "D2");
				expiresAt = defaultInterpolatedStringHandler7.ToStringAndClear();
			}
			QuestSaveDto questSaveDto = new QuestSaveDto();
			int? id;
			if (!saveAs)
			{
				QuestDto questDto2 = this.existingQuest;
				id = ((questDto2 != null) ? new int?(questDto2.Id) : null);
			}
			else
			{
				id = null;
			}
			questSaveDto.Id = id;
			questSaveDto.RecurrenceType = this.formType;
			questSaveDto.Title = this.formTitle;
			questSaveDto.Description = this.formDescription;
			questSaveDto.Objectives = (from o in this.formObjectives
			select new QuestObjectiveDto
			{
				Id = o.Id,
				Type = o.Type,
				Count = o.Count,
				AcceptedTargets = (o.AcceptedTargets ?? new List<string>()),
				AcceptedItems = (o.AcceptedItems ?? new List<QuestAcceptedItemDto>())
			}).ToList<QuestObjectiveDto>();
			questSaveDto.Rewards = (from r in validRewards
			select new QuestRewardDto
			{
				Code = r.Code,
				Nbt = r.Nbt,
				Amount = r.Amount
			}).ToList<QuestRewardDto>();
			questSaveDto.StartsAt = startsAt;
			questSaveDto.ExpiresAt = expiresAt;
			questSaveDto.UsesIngameTime = this.formIgt;
			questSaveDto.Repeat = this.formRepeating;
			QuestSaveDto questDto = questSaveDto;
			QuestNetworkHandler questNetworkHandler = this.modSystem.QuestNetworkHandler;
			if (questNetworkHandler == null)
			{
				this.capi.ShowChatMessage("Quest network handler not available");
				return false;
			}
			questNetworkHandler.OnQuestSaveResponse = delegate(QuestSaveResponsePacket response)
			{
				if (response.Success)
				{
					this.capi.ShowChatMessage((this.existingQuest != null) ? "Quest updated successfully" : "Quest created successfully");
					Action onQuestSaved = this.OnQuestSaved;
					if (onQuestSaved != null)
					{
						onQuestSaved();
					}
					this.TryClose();
				}
				else
				{
					this.capi.ShowChatMessage("Failed to save quest: " + response.Message);
				}
				questNetworkHandler.OnQuestSaveResponse = null;
			};
			IClientPlayer player = this.capi.World.Player;
			questNetworkHandler.SaveQuest(player.PlayerUID, questDto);
			return true;
		}

		// Token: 0x06000576 RID: 1398 RVA: 0x00024EA0 File Offset: 0x000230A0
		public override void OnGuiClosed()
		{
			base.OnGuiClosed();
			Action onDialogClosed = this.OnDialogClosed;
			if (onDialogClosed == null)
			{
				return;
			}
			onDialogClosed();
		}

		// Token: 0x06000577 RID: 1399 RVA: 0x00024EB8 File Offset: 0x000230B8
		public override void Dispose()
		{
			if (this.objectiveInventory != null)
			{
				this.objectiveInventory.SlotModified -= this.OnInventorySlotModified;
				for (int i = 0; i < this.objectiveInventory.Count; i++)
				{
					ItemSlot itemSlot = this.objectiveInventory[i];
					if (((itemSlot != null) ? itemSlot.Itemstack : null) != null)
					{
						this.objectiveInventory[i].Itemstack = null;
						this.objectiveInventory[i].MarkDirty();
					}
				}
				this.objectiveInventory.DiscardAll();
			}
			base.Dispose();
		}

		// Token: 0x04000202 RID: 514
		private readonly SRGuildsAndKingdomsModSystem modSystem;

		// Token: 0x04000203 RID: 515
		[Nullable(2)]
		private readonly QuestDto existingQuest;

		// Token: 0x04000204 RID: 516
		[Nullable(2)]
		private readonly CurrencyDefinitionDto tailsDefinition;

		// Token: 0x04000205 RID: 517
		[Nullable(2)]
		private readonly CurrencyDefinitionDto crownsDefinition;

		// Token: 0x04000206 RID: 518
		private static readonly Random rnd = new Random();

		// Token: 0x04000207 RID: 519
		private readonly long serverLocalTime;

		// Token: 0x04000208 RID: 520
		private readonly double serverTimezoneOffset;

		// Token: 0x04000209 RID: 521
		private const double DIALOG_WIDTH = 950.0;

		// Token: 0x0400020A RID: 522
		private const string GRS_POINTS_CODE = "game:grspoints";

		// Token: 0x0400020B RID: 523
		private const int QUEST_OBJECTIVE_LIMIT = 5;

		// Token: 0x0400020C RID: 524
		private const int QUEST_REWARD_LIMIT = 5;

		// Token: 0x0400020D RID: 525
		private string formTitle = "";

		// Token: 0x0400020E RID: 526
		private string formDescription = "";

		// Token: 0x0400020F RID: 527
		private string formType = "daily";

		// Token: 0x04000210 RID: 528
		private bool formIgt;

		// Token: 0x04000211 RID: 529
		private bool formRepeating;

		// Token: 0x04000212 RID: 530
		private string formStartYear;

		// Token: 0x04000213 RID: 531
		private string formStartMonth;

		// Token: 0x04000214 RID: 532
		private string formStartDay;

		// Token: 0x04000215 RID: 533
		private string formEndYear;

		// Token: 0x04000216 RID: 534
		private string formEndMonth;

		// Token: 0x04000217 RID: 535
		private string formEndDay;

		// Token: 0x04000218 RID: 536
		private readonly List<QuestObjectiveDto> formObjectives;

		// Token: 0x04000219 RID: 537
		private List<DialogQuestEditor.QuestRewardDtoWithNullableCode> formRewards;

		// Token: 0x0400021A RID: 538
		[Nullable(2)]
		private InventoryGeneric objectiveInventory;

		// Token: 0x0400021B RID: 539
		[Nullable(2)]
		private InventoryGeneric rewardsInventory;

		// Token: 0x02000120 RID: 288
		[NullableContext(2)]
		[Nullable(0)]
		public class QuestRewardDtoWithNullableCode
		{
			// Token: 0x1700028B RID: 651
			// (get) Token: 0x06000AFD RID: 2813 RVA: 0x00046A8D File Offset: 0x00044C8D
			// (set) Token: 0x06000AFE RID: 2814 RVA: 0x00046A95 File Offset: 0x00044C95
			public string Code { get; set; }

			// Token: 0x1700028C RID: 652
			// (get) Token: 0x06000AFF RID: 2815 RVA: 0x00046A9E File Offset: 0x00044C9E
			// (set) Token: 0x06000B00 RID: 2816 RVA: 0x00046AA6 File Offset: 0x00044CA6
			public string Nbt { get; set; }

			// Token: 0x1700028D RID: 653
			// (get) Token: 0x06000B01 RID: 2817 RVA: 0x00046AAF File Offset: 0x00044CAF
			// (set) Token: 0x06000B02 RID: 2818 RVA: 0x00046AB7 File Offset: 0x00044CB7
			public int Amount { get; set; }
		}
	}
}
