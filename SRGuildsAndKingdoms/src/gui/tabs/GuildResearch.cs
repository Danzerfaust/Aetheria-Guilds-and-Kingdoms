using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Cairo;
using SRGuildsAndKingdoms.src.guilds;
using SRGuildsAndKingdoms.src.network;
using SRGuildsAndKingdoms.src.techblock;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace SRGuildsAndKingdoms.src.gui.tabs
{
	// Token: 0x0200008F RID: 143
	[NullableContext(1)]
	[Nullable(0)]
	internal class GuildResearch : GuildTabContent
	{
		// Token: 0x0600063E RID: 1598 RVA: 0x0002CB34 File Offset: 0x0002AD34
		public GuildResearch(ICoreClientAPI capi, SRGuildsAndKingdomsModSystem modSystem, [Nullable(2)] GuildSummary currentGuild, ActionConsumable onRefresh) : base(capi, modSystem, currentGuild)
		{
			this.onRefresh = onRefresh;
			modSystem.NetworkHandler.RegisterTechContributionCallback(new Action<TechContributionResponsePacket>(this.OnTechContributionResponse));
		}

		// Token: 0x0600063F RID: 1599 RVA: 0x0002CBD0 File Offset: 0x0002ADD0
		public void Dispose()
		{
			this.modSystem.NetworkHandler.UnregisterTechContributionCallback();
			GuildResearch.GuiElementDragCanvas guiElementDragCanvas = this.canvasElement;
			if (guiElementDragCanvas != null)
			{
				guiElementDragCanvas.Cleanup();
			}
			this.canvasElement = null;
			this.currentComposer = null;
			this.selectedTechBlock = null;
			this.pendingContributionPlan = null;
			List<GuildResearch.Connection> list = this.connections;
			if (list != null)
			{
				list.Clear();
			}
			Dictionary<int, Vec2d> dictionary = this.boxSizes;
			if (dictionary != null)
			{
				dictionary.Clear();
			}
			List<TechBlock> list2 = this.ageFilteredBlocks;
			if (list2 == null)
			{
				return;
			}
			list2.Clear();
		}

		// Token: 0x06000640 RID: 1600 RVA: 0x0002CC4C File Offset: 0x0002AE4C
		public override double AddContent(GuiComposer composer, double startTop)
		{
			this.currentComposer = composer;
			this.techBlocks = this.modSystem.TechBlocks;
			this.UpdateAgeFilteredBlocks();
			this.CalculateBoxSizes();
			this.connections = this.GenerateConnections();
			this.CalculateBoxPositions();
			double elementHeight = 25.0;
			double spacing = 10.0;
			int tabWidth = 100;
			int tabHeight = 25;
			int tabSpacing = 5;
			TechBlocksConfig config = this.modSystem.TechBlocksConfig;
			List<TechAge> allAges = Enum.GetValues(typeof(TechAge)).Cast<TechAge>().ToList<TechAge>();
			List<TechAge> enabledAges = (config != null) ? (from age in allAges
			where config.IsAgeEnabled(age)
			select age).ToList<TechAge>() : allAges;
			for (int i = 0; i < enabledAges.Count; i++)
			{
				TechAge age = enabledAges[i];
				ElementBounds tabBounds = ElementBounds.Fixed((double)(i * (tabWidth + tabSpacing)), startTop, (double)tabWidth, (double)tabHeight);
				bool isCurrentAge = age == this.currentAge;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(7, 1);
				defaultInterpolatedStringHandler.AppendLiteral("ageTab_");
				defaultInterpolatedStringHandler.AppendFormatted<TechAge>(age);
				string buttonKey = defaultInterpolatedStringHandler.ToStringAndClear();
				GuiComposerHelpers.AddSmallButton(composer, age.ToString(), () => this.OnAgeTabClicked(age), tabBounds, isCurrentAge ? 1 : 2, buttonKey);
			}
			double top = startTop + ((double)tabHeight + spacing);
			ElementBounds instructionBounds = ElementBounds.Fixed(0.0, top, 500.0, elementHeight);
			GuiComposerHelpers.AddStaticText(composer, "Drag the canvas to reveal tech tree nodes!", CairoFont.WhiteDetailText(), instructionBounds, null);
			top += elementHeight + spacing;
			this.canvasBounds = ElementBounds.Fixed(0.0, top, 500.0, 300.0);
			GuiElementInsetHelper.AddInset(composer, this.canvasBounds, 3, 0.85f);
			ElementBounds interactiveBounds = ElementBounds.Fixed(3.0, 3.0, 494.0, 294.0).WithParent(this.canvasBounds);
			this.canvasElement = new GuildResearch.GuiElementDragCanvas(this.capi, interactiveBounds, this);
			composer.AddInteractiveElement(this.canvasElement, null);
			top += 300.0 + spacing;
			if (this.currentGuild != null)
			{
				ElementBounds techInfoBounds = ElementBounds.Fixed(0.0, top, 500.0, elementHeight);
				GuiElementDynamicTextHelper.AddDynamicText(composer, "", CairoFont.WhiteSmallText(), techInfoBounds, "techInfo");
				this.UpdateTechInfoText();
				top += elementHeight + spacing;
				this.detailsSectionTop = top;
				ElementBounds separatorBounds = ElementBounds.Fixed(0.0, top, 500.0, 2.0);
				GuiElementInsetHelper.AddInset(composer, separatorBounds, 1, 0.85f);
				top += 5.0;
				ElementBounds titleBounds = ElementBounds.Fixed(10.0, top, 480.0, 20.0);
				GuiElementDynamicTextHelper.AddDynamicText(composer, "", CairoFont.WhiteSmallText().WithWeight(1), titleBounds, "techDetailTitle");
				top += 25.0;
				ElementBounds descBounds = ElementBounds.Fixed(10.0, top, 480.0, 40.0);
				GuiElementDynamicTextHelper.AddDynamicText(composer, "", CairoFont.WhiteDetailText(), descBounds, "techDetailDesc");
				top += 45.0;
				ElementBounds statusBounds = ElementBounds.Fixed(10.0, top, 480.0, 20.0);
				GuiElementDynamicTextHelper.AddDynamicText(composer, "", CairoFont.WhiteSmallText(), statusBounds, "techDetailStatus");
				top += 25.0;
				ElementBounds progressBounds = ElementBounds.Fixed(10.0, top, 480.0, 20.0);
				GuiElementDynamicTextHelper.AddDynamicText(composer, "", CairoFont.WhiteSmallText(), progressBounds, "techDetailProgress");
				top += 25.0;
				ElementBounds clipBounds = ElementBounds.Fixed(10.0, top, 470.0, 120.0);
				ElementBounds scrollbarBounds = ElementBounds.Fixed(clipBounds.fixedWidth + 2.0, 1.0, 20.0, clipBounds.fixedHeight + 2.0).WithParent(clipBounds);
				ElementBounds insetBounds = ElementBounds.Fixed(2.0, 2.0, clipBounds.fixedWidth - 4.0, clipBounds.fixedHeight);
				ElementBounds contentBounds = ElementBounds.Fixed(0.0, 0.0, insetBounds.fixedWidth - 10.0, 500.0);
				GuiElementClipHelpler.BeginClip(composer, clipBounds);
				GuiElementInsetHelper.AddInset(composer, insetBounds.WithParent(clipBounds), 2, 0.85f);
				composer.BeginChildElements(insetBounds.WithParent(clipBounds));
				GuiElementDynamicTextHelper.AddDynamicText(composer, "", CairoFont.WhiteDetailText(), contentBounds, "techDetailResources");
				composer.EndChildElements();
				GuiElementClipHelpler.EndClip(composer);
				GuiComposerHelpers.AddVerticalScrollbar(composer, new Action<float>(this.OnResourcesScroll), scrollbarBounds, "resourcesScrollbar");
				GuiElementScrollbar scrollbar = GuiComposerHelpers.GetScrollbar(composer, "resourcesScrollbar");
				scrollbar.SetHeights((float)clipBounds.fixedHeight, (float)contentBounds.fixedHeight);
				scrollbar.Bounds.CalcWorldBounds();
				scrollbar.SetScrollbarPosition(0);
				top += 125.0;
				ElementBounds buttonBounds = ElementBounds.Fixed(10.0, top, 150.0, 30.0);
				GuiComposerHelpers.AddSmallButton(composer, "Contribute Resources", new ActionConsumable(this.OnContributeClicked), buttonBounds, 2, "contributeButton");
				top += 40.0;
				if (this.selectedTechBlock != null)
				{
					this.UpdateDetailsSection();
				}
			}
			return top;
		}

		// Token: 0x06000641 RID: 1601 RVA: 0x0002D234 File Offset: 0x0002B434
		private List<GuildResearch.Connection> GenerateConnections()
		{
			List<GuildResearch.Connection> connections = new List<GuildResearch.Connection>();
			foreach (TechBlock block in this.techBlocks)
			{
				if (block.UnlocksIds != null && block.UnlocksIds.Count != 0)
				{
					foreach (int unlockedId in block.UnlocksIds)
					{
						connections.Add(new GuildResearch.Connection
						{
							FromId = block.Id,
							ToId = unlockedId,
							LineColor = new double[]
							{
								0.8,
								0.8,
								0.8
							},
							LineWidth = 2.0
						});
					}
				}
			}
			return connections;
		}

		// Token: 0x06000642 RID: 1602 RVA: 0x0002D330 File Offset: 0x0002B530
		private void CalculateBoxSizes()
		{
			this.boxSizes.Clear();
			using (ImageSurface surface = new ImageSurface(0, 1, 1))
			{
				using (Context ctx = new Context(surface))
				{
					CairoFont font = CairoFont.WhiteSmallText();
					font.SetupContext(ctx);
					foreach (TechBlock block in this.techBlocks)
					{
						double width = 120.0;
						List<string> lines = this.WrapText(ctx, block.Text, width - 20.0);
						double lineHeight = font.GetFontExtents().Height;
						double height = Math.Max(50.0, (double)lines.Count * lineHeight + 20.0 + 5.0);
						this.boxSizes[block.Id] = new Vec2d(width, height);
					}
				}
			}
		}

		// Token: 0x06000643 RID: 1603 RVA: 0x0002D45C File Offset: 0x0002B65C
		private List<string> WrapText(Context ctx, string text, double maxWidth)
		{
			List<string> lines = new List<string>();
			string[] array = text.Split(' ', StringSplitOptions.None);
			string currentLine = "";
			foreach (string word in array)
			{
				string testLine = string.IsNullOrEmpty(currentLine) ? word : (currentLine + " " + word);
				if (ctx.TextExtents(testLine).Width > maxWidth && !string.IsNullOrEmpty(currentLine))
				{
					lines.Add(currentLine);
					currentLine = word;
				}
				else
				{
					currentLine = testLine;
				}
			}
			if (!string.IsNullOrEmpty(currentLine))
			{
				lines.Add(currentLine);
			}
			if (lines.Count <= 0)
			{
				return new List<string>
				{
					text
				};
			}
			return lines;
		}

		// Token: 0x06000644 RID: 1604 RVA: 0x0002D500 File Offset: 0x0002B700
		private void UpdateAgeFilteredBlocks()
		{
			TechBlocksConfig config = this.modSystem.TechBlocksConfig;
			if (config != null && !config.IsAgeEnabled(this.currentAge))
			{
				this.ageFilteredBlocks = new List<TechBlock>();
				return;
			}
			this.ageFilteredBlocks = (from b in this.techBlocks
			where b.Age == this.currentAge
			select b).ToList<TechBlock>();
			HashSet<int> currentAgeBlockIds = new HashSet<int>(from b in this.ageFilteredBlocks
			select b.Id);
			List<TechBlock> prerequisiteBlocks = new List<TechBlock>();
			using (List<TechBlock>.Enumerator enumerator = this.ageFilteredBlocks.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					TechBlock currentBlock = enumerator.Current;
					foreach (TechBlock prereq in (from b in this.techBlocks
					where b.Age < this.currentAge && b.UnlocksIds != null && b.UnlocksIds.Contains(currentBlock.Id) && (config == null || config.IsAgeEnabled(b.Age))
					select b).ToList<TechBlock>())
					{
						if (!currentAgeBlockIds.Contains(prereq.Id))
						{
							prerequisiteBlocks.Add(prereq);
							currentAgeBlockIds.Add(prereq.Id);
						}
					}
				}
			}
			this.ageFilteredBlocks.AddRange(prerequisiteBlocks);
		}

		// Token: 0x06000645 RID: 1605 RVA: 0x0002D68C File Offset: 0x0002B88C
		private void CalculateBoxPositions()
		{
			List<IGrouping<int, TechBlock>> list = (from b in this.ageFilteredBlocks
			group b by this.GetDisplayLevel(b) into g
			orderby g.Key
			select g).ToList<IGrouping<int, TechBlock>>();
			Dictionary<int, List<double>> occupiedPositions = new Dictionary<int, List<double>>();
			foreach (IGrouping<int, TechBlock> levelGroup in list)
			{
				int level = levelGroup.Key;
				List<TechBlock> levelBlocks = levelGroup.ToList<TechBlock>();
				int levelY = 40 + (level - 1) * 140;
				occupiedPositions[level] = new List<double>();
				if (level == 1)
				{
					List<TechBlock> sortedBlocks = this.SortBoxesInLevel(levelBlocks, level);
					for (int i = 0; i < sortedBlocks.Count; i++)
					{
						TechBlock techBlock = sortedBlocks[i];
						int xPos = 60 + i * 150;
						techBlock.Position = new Vec2d((double)xPos, (double)levelY);
						occupiedPositions[level].Add((double)xPos);
					}
				}
				else
				{
					HashSet<int> assignedChildren = new HashSet<int>();
					Dictionary<int, TechBlock> parentToLeftmostChild = new Dictionary<int, TechBlock>();
					using (List<TechBlock>.Enumerator enumerator2 = levelBlocks.GetEnumerator())
					{
						while (enumerator2.MoveNext())
						{
							TechBlock block = enumerator2.Current;
							List<TechBlock> prerequisites = (from b in this.techBlocks
							where b.UnlocksIds != null && b.UnlocksIds.Contains(block.Id) && b.Position != null && this.GetDisplayLevel(b) == level - 1
							select b into p
							orderby p.Position.X
							select p).ToList<TechBlock>();
							if (prerequisites.Count > 0)
							{
								TechBlock leftmostParent = prerequisites[0];
								if (!parentToLeftmostChild.ContainsKey(leftmostParent.Id))
								{
									parentToLeftmostChild[leftmostParent.Id] = block;
									assignedChildren.Add(block.Id);
								}
							}
						}
					}
					foreach (KeyValuePair<int, TechBlock> kvp in parentToLeftmostChild.OrderBy(delegate(KeyValuePair<int, TechBlock> k)
					{
						TechBlock techBlock2 = this.techBlocks.Find((TechBlock b) => b.Id == k.Key);
						double? num;
						if (techBlock2 == null)
						{
							num = null;
						}
						else
						{
							Vec2d position = techBlock2.Position;
							num = ((position != null) ? new double?(position.X) : null);
						}
						double? num2 = num;
						return num2.GetValueOrDefault(double.MaxValue);
					}))
					{
						int parentId = kvp.Key;
						TechBlock child = kvp.Value;
						TechBlock parent = this.techBlocks.Find((TechBlock b) => b.Id == parentId);
						if (((parent != null) ? parent.Position : null) != null)
						{
							double targetX2 = parent.Position.X;
							double finalX = this.FindNonOverlappingPosition(targetX2, occupiedPositions[level], level);
							child.Position = new Vec2d(finalX, (double)levelY);
							occupiedPositions[level].Add(finalX);
						}
					}
					using (List<TechBlock>.Enumerator enumerator2 = (from b in levelBlocks
					where !assignedChildren.Contains(b.Id)
					select b).ToList<TechBlock>().GetEnumerator())
					{
						while (enumerator2.MoveNext())
						{
							TechBlock block = enumerator2.Current;
							List<TechBlock> prerequisites2 = (from b in this.techBlocks
							where b.UnlocksIds != null && b.UnlocksIds.Contains(block.Id) && b.Position != null && this.GetDisplayLevel(b) == level - 1
							select b into p
							orderby p.Position.X
							select p).ToList<TechBlock>();
							double targetX;
							if (prerequisites2.Count > 0)
							{
								targetX = prerequisites2[0].Position.X;
								if (occupiedPositions[level].Any((double x) => Math.Abs(x - targetX) < 150.0))
								{
									targetX = occupiedPositions[level].Max() + 150.0;
								}
							}
							else
							{
								targetX = ((occupiedPositions[level].Count > 0) ? (occupiedPositions[level].Max() + 150.0) : 60.0);
							}
							double finalX2 = this.FindNonOverlappingPosition(targetX, occupiedPositions[level], level);
							block.Position = new Vec2d(finalX2, (double)levelY);
							occupiedPositions[level].Add(finalX2);
						}
					}
				}
			}
		}

		// Token: 0x06000646 RID: 1606 RVA: 0x0002DBBC File Offset: 0x0002BDBC
		private double FindNonOverlappingPosition(double targetX, List<double> occupiedPositions, int level)
		{
			if (!occupiedPositions.Any((double x) => Math.Abs(x - targetX) < 150.0))
			{
				return targetX;
			}
			double rightOffset = 150.0;
			double leftOffset = -150.0;
			for (int i = 0; i < 20; i++)
			{
				double rightPos = targetX + rightOffset;
				if (!occupiedPositions.Any((double x) => Math.Abs(x - rightPos) < 150.0))
				{
					return rightPos;
				}
				double leftPos = targetX + leftOffset;
				if (leftPos >= 60.0 && !occupiedPositions.Any((double x) => Math.Abs(x - leftPos) < 150.0))
				{
					return leftPos;
				}
				rightOffset += 150.0;
				leftOffset -= 150.0;
			}
			if (occupiedPositions.Count <= 0)
			{
				return targetX;
			}
			return occupiedPositions.Max() + 150.0;
		}

		// Token: 0x06000647 RID: 1607 RVA: 0x0002DCC1 File Offset: 0x0002BEC1
		private int GetDisplayLevel(TechBlock block)
		{
			if (block.Age < this.currentAge)
			{
				return 1;
			}
			return block.Level;
		}

		// Token: 0x06000648 RID: 1608 RVA: 0x0002DCDC File Offset: 0x0002BEDC
		private List<TechBlock> SortBoxesInLevel(List<TechBlock> levelBlocks, int level)
		{
			if (level == 1)
			{
				return levelBlocks.OrderByDescending(delegate(TechBlock block)
				{
					List<int> unlocksIds = block.UnlocksIds;
					if (unlocksIds == null)
					{
						return 0;
					}
					return unlocksIds.Count;
				}).ToList<TechBlock>();
			}
			List<TechBlock> list = new List<TechBlock>(levelBlocks);
			list.Sort(delegate(TechBlock a, TechBlock b)
			{
				double aAvgX = this.GetAverageConnectedX(a.Id, level - 1);
				double bAvgX = this.GetAverageConnectedX(b.Id, level - 1);
				return aAvgX.CompareTo(bAvgX);
			});
			return list;
		}

		// Token: 0x06000649 RID: 1609 RVA: 0x0002DD4C File Offset: 0x0002BF4C
		private List<GuildResearch.Connection> GetIncomingConnections(int blockId)
		{
			return (from c in this.connections
			where c.ToId == blockId
			select c).ToList<GuildResearch.Connection>();
		}

		// Token: 0x0600064A RID: 1610 RVA: 0x0002DD84 File Offset: 0x0002BF84
		private double GetAverageConnectedX(int blockId, int fromLevel)
		{
			List<TechBlock> connectedBlocks = (from c in this.GetIncomingConnections(blockId)
			select this.techBlocks.Find((TechBlock b) => b.Id == c.FromId) into b
			where b != null && b.Level == fromLevel
			select b).ToList<TechBlock>();
			if (!connectedBlocks.Any<TechBlock>())
			{
				return 60.0;
			}
			return connectedBlocks.Average(delegate(TechBlock b)
			{
				Vec2d position = b.Position;
				if (position == null)
				{
					return 60.0;
				}
				return position.X;
			});
		}

		// Token: 0x0600064B RID: 1611 RVA: 0x0002DE0C File Offset: 0x0002C00C
		internal bool OnBoxClick(int blockIndex)
		{
			if (blockIndex >= 0 && blockIndex < this.techBlocks.Count)
			{
				TechBlock block = this.techBlocks[blockIndex];
				List<TechBlock> connectedBlocks = this.GetConnectedBoxes(block.Id);
				string text;
				if (connectedBlocks.Count <= 0)
				{
					text = " No connections ";
				}
				else
				{
					text = " Connected to: " + string.Join(", ", connectedBlocks.Select(delegate(TechBlock b)
					{
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(4, 2);
						defaultInterpolatedStringHandler2.AppendFormatted(b.Text);
						defaultInterpolatedStringHandler2.AppendLiteral(" (L");
						defaultInterpolatedStringHandler2.AppendFormatted<int>(b.Level);
						defaultInterpolatedStringHandler2.AppendLiteral(")");
						return defaultInterpolatedStringHandler2.ToStringAndClear();
					})) + " ";
				}
				string connectionInfo = text;
				string text2;
				if (block.ResourceGroups.Count <= 0)
				{
					text2 = " No resources required ";
				}
				else
				{
					text2 = " Resources needed: " + string.Join(", ", block.ResourceGroups.Select(delegate(ResourceGroup r)
					{
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(2, 2);
						defaultInterpolatedStringHandler2.AppendFormatted<int>(r.AmountRequired);
						defaultInterpolatedStringHandler2.AppendLiteral("x ");
						defaultInterpolatedStringHandler2.AppendFormatted(r.Name);
						return defaultInterpolatedStringHandler2.ToStringAndClear();
					})) + " ";
				}
				string resourceInfo = text2;
				ILogger logger = this.capi.Logger;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(32, 4);
				defaultInterpolatedStringHandler.AppendLiteral("Tech block (");
				defaultInterpolatedStringHandler.AppendFormatted(block.Text);
				defaultInterpolatedStringHandler.AppendLiteral(") at Level ");
				defaultInterpolatedStringHandler.AppendFormatted<int>(block.Level);
				defaultInterpolatedStringHandler.AppendLiteral(" clicked!");
				defaultInterpolatedStringHandler.AppendFormatted(connectionInfo);
				defaultInterpolatedStringHandler.AppendFormatted(resourceInfo);
				logger.Debug(defaultInterpolatedStringHandler.ToStringAndClear());
				this.ShowTechResearch(block);
			}
			return true;
		}

		// Token: 0x0600064C RID: 1612 RVA: 0x0002DF6A File Offset: 0x0002C16A
		private void ShowTechResearch(TechBlock block)
		{
			if (this.currentGuild == null)
			{
				this.capi.ShowChatMessage("No guild selected");
				return;
			}
			this.selectedTechBlock = block;
			this.UpdateDetailsSection();
		}

		// Token: 0x0600064D RID: 1613 RVA: 0x0002DF94 File Offset: 0x0002C194
		private void UpdateTechInfoText()
		{
			if (this.currentComposer == null || this.currentGuild == null)
			{
				return;
			}
			int unlockedCount = this.ageFilteredBlocks.Count((TechBlock t) => this.currentGuild.IsTechUnlocked(t.Id));
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(22, 3);
			defaultInterpolatedStringHandler.AppendFormatted<TechAge>(this.currentAge);
			defaultInterpolatedStringHandler.AppendLiteral(" Age: ");
			defaultInterpolatedStringHandler.AppendFormatted<int>(unlockedCount);
			defaultInterpolatedStringHandler.AppendLiteral("/");
			defaultInterpolatedStringHandler.AppendFormatted<int>(this.ageFilteredBlocks.Count);
			defaultInterpolatedStringHandler.AppendLiteral(" techs unlocked");
			string techInfoText = defaultInterpolatedStringHandler.ToStringAndClear();
			GuiElementDynamicText dynamicText = GuiElementDynamicTextHelper.GetDynamicText(this.currentComposer, "techInfo");
			if (dynamicText == null)
			{
				return;
			}
			dynamicText.SetNewText(techInfoText, false, false, false);
		}

		// Token: 0x0600064E RID: 1614 RVA: 0x0002E048 File Offset: 0x0002C248
		private bool ArePrerequisitesUnlocked(TechBlock techBlock)
		{
			List<TechBlock> prerequisites = (from b in this.techBlocks
			where b.UnlocksIds != null && b.UnlocksIds.Contains(techBlock.Id)
			select b).ToList<TechBlock>();
			return prerequisites.Count == 0 || prerequisites.All((TechBlock prereq) => this.currentGuild.IsTechUnlocked(prereq.Id));
		}

		// Token: 0x0600064F RID: 1615 RVA: 0x0002E0A4 File Offset: 0x0002C2A4
		private void UpdateDetailsSection()
		{
			if (this.currentComposer == null || this.currentGuild == null || this.selectedTechBlock == null)
			{
				return;
			}
			GuildTechProgress progress = this.currentGuild.GetOrCreateTechProgress(this.selectedTechBlock.Id);
			TechBlocksConfig config = this.modSystem.TechBlocksConfig;
			bool ageIsLocked = config != null && !config.IsAgeEnabled(this.selectedTechBlock.Age);
			Dictionary<string, int> baseRequirements = new Dictionary<string, int>();
			foreach (ResourceGroup rg in this.selectedTechBlock.ResourceGroups)
			{
				baseRequirements[rg.Name] = rg.AmountRequired;
			}
			this.modSystem.NetworkHandler.RequestScaledRequirements(this.currentGuild.Name, this.selectedTechBlock.Id, baseRequirements, delegate(ScaledRequirementsResponsePacket response)
			{
				this.OnScaledRequirementsReceived(response, progress, ageIsLocked);
			});
		}

		// Token: 0x06000650 RID: 1616 RVA: 0x0002E1B4 File Offset: 0x0002C3B4
		private void OnScaledRequirementsReceived(ScaledRequirementsResponsePacket response, GuildTechProgress progress, bool ageIsLocked)
		{
			if (this.currentComposer == null || this.selectedTechBlock == null)
			{
				return;
			}
			Dictionary<string, int> scaledRequirements = response.ScaledRequirements;
			decimal scaling = response.ResourceScaling;
			int memberCount = response.MemberCount;
			int totalRequired = scaledRequirements.Values.Sum();
			int totalSubmitted = this.selectedTechBlock.ResourceGroups.Sum((ResourceGroup rg) => progress.GetResourceGroupSubmitted(rg.Name));
			double overallPercent = (totalRequired > 0) ? ((double)totalSubmitted * 100.0 / (double)totalRequired) : 0.0;
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(19, 2);
			defaultInterpolatedStringHandler.AppendLiteral("Selected: ");
			defaultInterpolatedStringHandler.AppendFormatted(this.selectedTechBlock.Text);
			defaultInterpolatedStringHandler.AppendLiteral(" (Level ");
			defaultInterpolatedStringHandler.AppendFormatted<int>(this.selectedTechBlock.Level);
			defaultInterpolatedStringHandler.AppendLiteral(")");
			string titleText = defaultInterpolatedStringHandler.ToStringAndClear();
			GuiElementDynamicText dynamicText = GuiElementDynamicTextHelper.GetDynamicText(this.currentComposer, "techDetailTitle");
			if (dynamicText != null)
			{
				dynamicText.SetNewText(titleText, false, false, false);
			}
			string descText = (!string.IsNullOrWhiteSpace(this.selectedTechBlock.Description)) ? this.selectedTechBlock.Description : "";
			GuiElementDynamicText dynamicText2 = GuiElementDynamicTextHelper.GetDynamicText(this.currentComposer, "techDetailDesc");
			if (dynamicText2 != null)
			{
				dynamicText2.SetNewText(descText, false, false, false);
			}
			string statusText;
			if (progress.IsUnlocked)
			{
				statusText = "✓ UNLOCKED";
			}
			else if (ageIsLocked)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(35, 1);
				defaultInterpolatedStringHandler2.AppendLiteral("\ud83d\udd12 LOCKED - ");
				defaultInterpolatedStringHandler2.AppendFormatted<TechAge>(this.selectedTechBlock.Age);
				defaultInterpolatedStringHandler2.AppendLiteral(" Age is not yet enabled");
				statusText = defaultInterpolatedStringHandler2.ToStringAndClear();
			}
			else
			{
				statusText = "";
			}
			GuiElementDynamicText dynamicText3 = GuiElementDynamicTextHelper.GetDynamicText(this.currentComposer, "techDetailStatus");
			if (dynamicText3 != null)
			{
				dynamicText3.SetNewText(statusText, false, false, false);
			}
			string text;
			if (progress.IsUnlocked || ageIsLocked)
			{
				text = "";
			}
			else
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler3 = new DefaultInterpolatedStringHandler(11, 1);
				defaultInterpolatedStringHandler3.AppendLiteral("Progress: ");
				defaultInterpolatedStringHandler3.AppendFormatted<double>(overallPercent, "F1");
				defaultInterpolatedStringHandler3.AppendLiteral("%");
				text = defaultInterpolatedStringHandler3.ToStringAndClear();
			}
			string progressText = text;
			GuiElementDynamicText dynamicText4 = GuiElementDynamicTextHelper.GetDynamicText(this.currentComposer, "techDetailProgress");
			if (dynamicText4 != null)
			{
				dynamicText4.SetNewText(progressText, false, false, false);
			}
			StringBuilder resourcesText = new StringBuilder();
			if (!progress.IsUnlocked && !ageIsLocked && this.selectedTechBlock.ResourceGroups.Count > 0)
			{
				if (scaling > 1.0m)
				{
					decimal scalingPercent = (scaling - 1.0m) * 100m;
					StringBuilder stringBuilder = resourcesText;
					StringBuilder stringBuilder2 = stringBuilder;
					StringBuilder.AppendInterpolatedStringHandler appendInterpolatedStringHandler = new StringBuilder.AppendInterpolatedStringHandler(35, 2, stringBuilder);
					appendInterpolatedStringHandler.AppendLiteral("Guild Size: ");
					appendInterpolatedStringHandler.AppendFormatted<int>(memberCount);
					appendInterpolatedStringHandler.AppendLiteral(" members (+");
					appendInterpolatedStringHandler.AppendFormatted<decimal>(scalingPercent, "F0");
					appendInterpolatedStringHandler.AppendLiteral("% resources)");
					stringBuilder2.AppendLine(ref appendInterpolatedStringHandler);
					resourcesText.AppendLine();
				}
				resourcesText.AppendLine("Guild Resources:");
				using (List<ResourceGroup>.Enumerator enumerator = this.selectedTechBlock.ResourceGroups.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						ResourceGroup resourceGroup = enumerator.Current;
						int submitted = progress.GetResourceGroupSubmitted(resourceGroup.Name);
						int scaledRequired = scaledRequirements[resourceGroup.Name];
						double groupPercent = (scaledRequired > 0) ? ((double)submitted * 100.0 / (double)scaledRequired) : 0.0;
						if (scaling > 1.0m)
						{
							int baseRequired = resourceGroup.AmountRequired;
							StringBuilder stringBuilder = resourcesText;
							StringBuilder stringBuilder3 = stringBuilder;
							StringBuilder.AppendInterpolatedStringHandler appendInterpolatedStringHandler = new StringBuilder.AppendInterpolatedStringHandler(20, 5, stringBuilder);
							appendInterpolatedStringHandler.AppendLiteral("  • ");
							appendInterpolatedStringHandler.AppendFormatted(resourceGroup.Name);
							appendInterpolatedStringHandler.AppendLiteral(": ");
							appendInterpolatedStringHandler.AppendFormatted<int>(submitted);
							appendInterpolatedStringHandler.AppendLiteral("/");
							appendInterpolatedStringHandler.AppendFormatted<int>(scaledRequired);
							appendInterpolatedStringHandler.AppendLiteral(" (base: ");
							appendInterpolatedStringHandler.AppendFormatted<int>(baseRequired);
							appendInterpolatedStringHandler.AppendLiteral(") (");
							appendInterpolatedStringHandler.AppendFormatted<double>(groupPercent, "F0");
							appendInterpolatedStringHandler.AppendLiteral("%)");
							stringBuilder3.AppendLine(ref appendInterpolatedStringHandler);
						}
						else
						{
							StringBuilder stringBuilder = resourcesText;
							StringBuilder stringBuilder4 = stringBuilder;
							StringBuilder.AppendInterpolatedStringHandler appendInterpolatedStringHandler = new StringBuilder.AppendInterpolatedStringHandler(11, 4, stringBuilder);
							appendInterpolatedStringHandler.AppendLiteral("  • ");
							appendInterpolatedStringHandler.AppendFormatted(resourceGroup.Name);
							appendInterpolatedStringHandler.AppendLiteral(": ");
							appendInterpolatedStringHandler.AppendFormatted<int>(submitted);
							appendInterpolatedStringHandler.AppendLiteral("/");
							appendInterpolatedStringHandler.AppendFormatted<int>(scaledRequired);
							appendInterpolatedStringHandler.AppendLiteral(" (");
							appendInterpolatedStringHandler.AppendFormatted<double>(groupPercent, "F0");
							appendInterpolatedStringHandler.AppendLiteral("%)");
							stringBuilder4.AppendLine(ref appendInterpolatedStringHandler);
						}
					}
					goto IL_516;
				}
			}
			if (ageIsLocked)
			{
				StringBuilder stringBuilder = resourcesText;
				StringBuilder stringBuilder5 = stringBuilder;
				StringBuilder.AppendInterpolatedStringHandler appendInterpolatedStringHandler = new StringBuilder.AppendInterpolatedStringHandler(37, 1, stringBuilder);
				appendInterpolatedStringHandler.AppendLiteral("\nThis technology belongs to the ");
				appendInterpolatedStringHandler.AppendFormatted<TechAge>(this.selectedTechBlock.Age);
				appendInterpolatedStringHandler.AppendLiteral(" Age,");
				stringBuilder5.AppendLine(ref appendInterpolatedStringHandler);
				resourcesText.AppendLine("which is currently disabled in the server configuration.");
			}
			IL_516:
			GuiElementDynamicText dynamicText5 = GuiElementDynamicTextHelper.GetDynamicText(this.currentComposer, "techDetailResources");
			if (dynamicText5 != null)
			{
				dynamicText5.SetNewText(resourcesText.ToString(), false, false, false);
			}
			GuiElementTextButton contributeButton = GuiComposerHelpers.GetButton(this.currentComposer, "contributeButton");
			if (contributeButton != null)
			{
				bool showGuildButton = !progress.IsUnlocked && !ageIsLocked;
				contributeButton.Enabled = showGuildButton;
				contributeButton.Visible = showGuildButton;
			}
		}

		// Token: 0x06000651 RID: 1617 RVA: 0x0002E758 File Offset: 0x0002C958
		private void OnResourcesScroll(float value)
		{
			GuiComposer guiComposer = this.currentComposer;
			GuiElementScrollbar scrollbar = (guiComposer != null) ? GuiComposerHelpers.GetScrollbar(guiComposer, "resourcesScrollbar") : null;
			if (scrollbar == null)
			{
				return;
			}
			GuiElementDynamicText textElement = GuiElementDynamicTextHelper.GetDynamicText(this.currentComposer, "techDetailResources");
			if (((textElement != null) ? textElement.Bounds : null) == null)
			{
				return;
			}
			float scrollOffset = scrollbar.CurrentYPosition;
			textElement.Bounds.fixedY = (double)(5f - scrollOffset);
			textElement.Bounds.CalcWorldBounds();
		}

		// Token: 0x06000652 RID: 1618 RVA: 0x0002E7C8 File Offset: 0x0002C9C8
		private bool OnContributeClicked()
		{
			if (this.currentGuild == null || this.selectedTechBlock == null)
			{
				this.capi.ShowChatMessage("No tech selected or no guild active");
				return false;
			}
			TechBlocksConfig config = this.modSystem.TechBlocksConfig;
			if (config != null && !config.IsAgeEnabled(this.selectedTechBlock.Age))
			{
				ICoreClientAPI capi = this.capi;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(42, 1);
				defaultInterpolatedStringHandler.AppendLiteral("Cannot contribute: ");
				defaultInterpolatedStringHandler.AppendFormatted<TechAge>(this.selectedTechBlock.Age);
				defaultInterpolatedStringHandler.AppendLiteral(" Age is not enabled yet");
				capi.ShowChatMessage(defaultInterpolatedStringHandler.ToStringAndClear());
				return false;
			}
			if (!this.ArePrerequisitesUnlocked(this.selectedTechBlock))
			{
				this.capi.ShowChatMessage("Cannot contribute: prerequisite technologies must be unlocked first");
				return false;
			}
			IClientPlayer player = this.capi.World.Player;
			IClientPlayer player2 = player;
			if (((player2 != null) ? player2.Entity : null) == null)
			{
				this.capi.ShowChatMessage("Player not found");
				return false;
			}
			GuildTechProgress progress = this.currentGuild.GetOrCreateTechProgress(this.selectedTechBlock.Id);
			if (progress.IsUnlocked)
			{
				this.capi.ShowChatMessage("This tech is already unlocked!");
				return true;
			}
			Dictionary<string, int> baseRequirements = new Dictionary<string, int>();
			foreach (ResourceGroup rg in this.selectedTechBlock.ResourceGroups)
			{
				baseRequirements[rg.Name] = rg.AmountRequired;
			}
			this.modSystem.NetworkHandler.RequestScaledRequirements(this.currentGuild.Name, this.selectedTechBlock.Id, baseRequirements, delegate(ScaledRequirementsResponsePacket response)
			{
				this.ProcessContribution(response, progress, player);
			});
			return true;
		}

		// Token: 0x06000653 RID: 1619 RVA: 0x0002E99C File Offset: 0x0002CB9C
		private void ProcessContribution(ScaledRequirementsResponsePacket response, GuildTechProgress progress, IPlayer player)
		{
			if (this.currentGuild == null || this.selectedTechBlock == null)
			{
				return;
			}
			Dictionary<string, int> scaledRequirements = response.ScaledRequirements;
			Dictionary<string, List<GuildResearch.ContributionItem>> contributionPlan = new Dictionary<string, List<GuildResearch.ContributionItem>>();
			int totalItemsToContribute = 0;
			foreach (ResourceGroup resourceGroup in this.selectedTechBlock.ResourceGroups)
			{
				int currentSubmitted = progress.GetResourceGroupSubmitted(resourceGroup.Name);
				int remaining = scaledRequirements[resourceGroup.Name] - currentSubmitted;
				if (remaining > 0)
				{
					List<GuildResearch.ContributionItem> itemsForGroup = new List<GuildResearch.ContributionItem>();
					int amountNeeded = remaining;
					foreach (KeyValuePair<string, IInventory> invPair in player.InventoryManager.Inventories)
					{
						string key = invPair.Key;
						IInventory inv = invPair.Value;
						if (inv.ClassName != "creative")
						{
							if (amountNeeded <= 0)
							{
								break;
							}
							for (int j = 0; j < inv.Count; j++)
							{
								ItemSlot slot = inv[j];
								if (!slot.Empty)
								{
									ItemStack itemStack = slot.Itemstack;
									string itemCode = itemStack.Collectible.Code.ToString();
									if (resourceGroup.DoesItemMatch(itemCode))
									{
										int toContribute = Math.Min(itemStack.StackSize, amountNeeded);
										itemsForGroup.Add(new GuildResearch.ContributionItem
										{
											Slot = slot,
											Amount = toContribute,
											ItemName = itemStack.GetName()
										});
										amountNeeded -= toContribute;
										totalItemsToContribute += toContribute;
										if (amountNeeded <= 0)
										{
											break;
										}
									}
								}
							}
						}
					}
					if (itemsForGroup.Count > 0)
					{
						contributionPlan[resourceGroup.Name] = itemsForGroup;
					}
				}
			}
			if (totalItemsToContribute == 0)
			{
				this.capi.ShowChatMessage("No valid items found in inventory to contribute");
				return;
			}
			StringBuilder confirmMessage = new StringBuilder();
			StringBuilder stringBuilder = confirmMessage;
			StringBuilder stringBuilder2 = stringBuilder;
			StringBuilder.AppendInterpolatedStringHandler appendInterpolatedStringHandler = new StringBuilder.AppendInterpolatedStringHandler(16, 1, stringBuilder);
			appendInterpolatedStringHandler.AppendLiteral("Contribute to ");
			appendInterpolatedStringHandler.AppendFormatted(this.selectedTechBlock.Text);
			appendInterpolatedStringHandler.AppendLiteral("?\n");
			stringBuilder2.AppendLine(ref appendInterpolatedStringHandler);
			foreach (KeyValuePair<string, List<GuildResearch.ContributionItem>> kvp in contributionPlan)
			{
				string resourceGroupName = kvp.Key;
				List<GuildResearch.ContributionItem> value = kvp.Value;
				value.Sum((GuildResearch.ContributionItem i) => i.Amount);
				stringBuilder = confirmMessage;
				StringBuilder stringBuilder3 = stringBuilder;
				appendInterpolatedStringHandler = new StringBuilder.AppendInterpolatedStringHandler(1, 1, stringBuilder);
				appendInterpolatedStringHandler.AppendFormatted(resourceGroupName);
				appendInterpolatedStringHandler.AppendLiteral(":");
				stringBuilder3.AppendLine(ref appendInterpolatedStringHandler);
				foreach (var item in from i in value
				group i by i.ItemName into g
				select new
				{
					Name = g.Key,
					Amount = g.Sum((GuildResearch.ContributionItem i) => i.Amount)
				})
				{
					stringBuilder = confirmMessage;
					StringBuilder stringBuilder4 = stringBuilder;
					appendInterpolatedStringHandler = new StringBuilder.AppendInterpolatedStringHandler(6, 2, stringBuilder);
					appendInterpolatedStringHandler.AppendLiteral("  • ");
					appendInterpolatedStringHandler.AppendFormatted<int>(item.Amount);
					appendInterpolatedStringHandler.AppendLiteral("x ");
					appendInterpolatedStringHandler.AppendFormatted(item.Name);
					stringBuilder4.AppendLine(ref appendInterpolatedStringHandler);
				}
			}
			this.pendingContributionPlan = contributionPlan;
			new ConfirmContributionDialog(this.capi, confirmMessage.ToString(), delegate
			{
				this.SendContributionToServer(contributionPlan);
			}).TryOpen();
		}

		// Token: 0x06000654 RID: 1620 RVA: 0x0002EDC0 File Offset: 0x0002CFC0
		private void SendContributionToServer(Dictionary<string, List<GuildResearch.ContributionItem>> contributionPlan)
		{
			if (this.currentGuild == null || this.selectedTechBlock == null)
			{
				return;
			}
			List<ContributionItemDto> items = new List<ContributionItemDto>();
			foreach (KeyValuePair<string, List<GuildResearch.ContributionItem>> kvp in contributionPlan)
			{
				string resourceGroupName = kvp.Key;
				foreach (GuildResearch.ContributionItem contribution in kvp.Value)
				{
					items.Add(new ContributionItemDto
					{
						ResourceGroupName = resourceGroupName,
						InventoryId = contribution.Slot.Inventory.InventoryID,
						SlotId = contribution.Slot.Inventory.GetSlotId(contribution.Slot),
						Amount = contribution.Amount,
						ItemCode = contribution.Slot.Itemstack.Collectible.Code.ToString()
					});
				}
			}
			this.modSystem.NetworkHandler.SendTechContributionRequest(this.currentGuild.Name, this.selectedTechBlock.Id, items);
		}

		// Token: 0x06000655 RID: 1621 RVA: 0x0002EF0C File Offset: 0x0002D10C
		private void OnTechContributionResponse(TechContributionResponsePacket response)
		{
			if (!string.IsNullOrEmpty(response.Message))
			{
				this.capi.ShowChatMessage(response.Message);
			}
			if (response.Success)
			{
				base.RequestGuildRefresh();
			}
			this.pendingContributionPlan = null;
		}

		// Token: 0x06000656 RID: 1622 RVA: 0x0002EF41 File Offset: 0x0002D141
		protected override void OnGuildDataRefreshed(GuildSummary updatedGuild)
		{
			if (this.selectedTechBlock != null)
			{
				this.UpdateDetailsSection();
			}
			this.UpdateTechInfoText();
		}

		// Token: 0x06000657 RID: 1623 RVA: 0x0002EF58 File Offset: 0x0002D158
		private void ClearDetailsSection()
		{
			if (this.currentComposer == null)
			{
				return;
			}
			GuiElementDynamicText dynamicText = GuiElementDynamicTextHelper.GetDynamicText(this.currentComposer, "techDetailTitle");
			if (dynamicText != null)
			{
				dynamicText.SetNewText("", false, false, false);
			}
			GuiElementDynamicText dynamicText2 = GuiElementDynamicTextHelper.GetDynamicText(this.currentComposer, "techDetailDesc");
			if (dynamicText2 != null)
			{
				dynamicText2.SetNewText("", false, false, false);
			}
			GuiElementDynamicText dynamicText3 = GuiElementDynamicTextHelper.GetDynamicText(this.currentComposer, "techDetailStatus");
			if (dynamicText3 != null)
			{
				dynamicText3.SetNewText("", false, false, false);
			}
			GuiElementDynamicText dynamicText4 = GuiElementDynamicTextHelper.GetDynamicText(this.currentComposer, "techDetailProgress");
			if (dynamicText4 != null)
			{
				dynamicText4.SetNewText("", false, false, false);
			}
			GuiElementDynamicText dynamicText5 = GuiElementDynamicTextHelper.GetDynamicText(this.currentComposer, "techDetailResources");
			if (dynamicText5 == null)
			{
				return;
			}
			dynamicText5.SetNewText("", false, false, false);
		}

		// Token: 0x06000658 RID: 1624 RVA: 0x0002F01C File Offset: 0x0002D21C
		private List<TechBlock> GetConnectedBoxes(int blockId)
		{
			List<TechBlock> connected = new List<TechBlock>();
			using (List<GuildResearch.Connection>.Enumerator enumerator = this.connections.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					GuildResearch.Connection connection = enumerator.Current;
					if (connection.FromId == blockId)
					{
						TechBlock toBlock = this.techBlocks.Find((TechBlock b) => b.Id == connection.ToId);
						if (toBlock != null)
						{
							connected.Add(toBlock);
						}
					}
					else if (connection.ToId == blockId)
					{
						TechBlock fromBlock = this.techBlocks.Find((TechBlock b) => b.Id == connection.FromId);
						if (fromBlock != null)
						{
							connected.Add(fromBlock);
						}
					}
				}
			}
			return connected;
		}

		// Token: 0x06000659 RID: 1625 RVA: 0x0002F0E4 File Offset: 0x0002D2E4
		internal List<GuildResearch.Connection> GetVisibleConnections()
		{
			List<GuildResearch.Connection> visibleConnections = new List<GuildResearch.Connection>();
			HashSet<int> ageFilteredIds = new HashSet<int>(from b in this.ageFilteredBlocks
			select b.Id);
			using (List<GuildResearch.Connection>.Enumerator enumerator = this.connections.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					GuildResearch.Connection connection = enumerator.Current;
					if (this.IsValidConnection(connection))
					{
						TechBlock fromBlock = this.techBlocks.Find((TechBlock b) => b.Id == connection.FromId);
						TechBlock toBlock = this.techBlocks.Find((TechBlock b) => b.Id == connection.ToId);
						if (fromBlock != null && toBlock != null && ageFilteredIds.Contains(fromBlock.Id) && ageFilteredIds.Contains(toBlock.Id) && fromBlock.Position != null && toBlock.Position != null)
						{
							visibleConnections.Add(connection);
						}
					}
				}
			}
			return visibleConnections;
		}

		// Token: 0x0600065A RID: 1626 RVA: 0x0002F20C File Offset: 0x0002D40C
		private bool IsValidConnection(GuildResearch.Connection connection)
		{
			TechBlock fromBlock = this.techBlocks.Find((TechBlock b) => b.Id == connection.FromId);
			TechBlock toBlock = this.techBlocks.Find((TechBlock b) => b.Id == connection.ToId);
			return fromBlock != null && toBlock != null && (fromBlock.Age != toBlock.Age || Math.Abs(fromBlock.Level - toBlock.Level) == 1);
		}

		// Token: 0x0600065B RID: 1627 RVA: 0x0002F284 File Offset: 0x0002D484
		private bool OnAgeTabClicked(TechAge age)
		{
			TechBlocksConfig config = this.modSystem.TechBlocksConfig;
			if (config != null && !config.IsAgeEnabled(age))
			{
				ICoreClientAPI capi = this.capi;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(42, 1);
				defaultInterpolatedStringHandler.AppendLiteral("The ");
				defaultInterpolatedStringHandler.AppendFormatted<TechAge>(age);
				defaultInterpolatedStringHandler.AppendLiteral(" Age is not yet enabled on this server");
				capi.ShowChatMessage(defaultInterpolatedStringHandler.ToStringAndClear());
				return false;
			}
			if (this.currentAge != age)
			{
				this.currentAge = age;
				this.canvasOffset = new Vec2d(0.0, 0.0);
				this.selectedTechBlock = null;
				this.ClearDetailsSection();
				this.UpdateAgeFilteredBlocks();
				this.CalculateBoxSizes();
				this.connections = this.GenerateConnections();
				this.CalculateBoxPositions();
				this.UpdateTechInfoText();
				if (this.canvasElement != null)
				{
					using (ImageSurface surface = new ImageSurface(0, (int)this.canvasElement.Bounds.InnerWidth, (int)this.canvasElement.Bounds.InnerHeight))
					{
						using (Context ctx = new Context(surface))
						{
							this.canvasElement.ComposeElements(ctx, surface);
						}
					}
					this.canvasElement.MarkDirty();
				}
				ILogger logger = this.capi.Logger;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(16, 1);
				defaultInterpolatedStringHandler2.AppendLiteral("Switched to ");
				defaultInterpolatedStringHandler2.AppendFormatted<TechAge>(age);
				defaultInterpolatedStringHandler2.AppendLiteral(" Age");
				logger.Debug(defaultInterpolatedStringHandler2.ToStringAndClear());
			}
			return true;
		}

		// Token: 0x04000289 RID: 649
		private List<GuildResearch.Connection> connections = new List<GuildResearch.Connection>();

		// Token: 0x0400028A RID: 650
		private Vec2d canvasOffset = new Vec2d(0.0, 0.0);

		// Token: 0x0400028B RID: 651
		private ElementBounds canvasBounds;

		// Token: 0x0400028C RID: 652
		private const int FixedBoxWidth = 120;

		// Token: 0x0400028D RID: 653
		private const int MinBoxHeight = 50;

		// Token: 0x0400028E RID: 654
		private const int TextPadding = 10;

		// Token: 0x0400028F RID: 655
		private const int CanvasWidth = 500;

		// Token: 0x04000290 RID: 656
		private const int CanvasHeight = 300;

		// Token: 0x04000291 RID: 657
		private TechAge currentAge;

		// Token: 0x04000292 RID: 658
		private bool isDragging;

		// Token: 0x04000293 RID: 659
		private Vec2d lastMousePos = new Vec2d(0.0, 0.0);

		// Token: 0x04000294 RID: 660
		private const int LevelSpacing = 140;

		// Token: 0x04000295 RID: 661
		private const int BoxSpacing = 150;

		// Token: 0x04000296 RID: 662
		private const int StartX = 60;

		// Token: 0x04000297 RID: 663
		private const int StartY = 40;

		// Token: 0x04000298 RID: 664
		private List<TechBlock> techBlocks = new List<TechBlock>();

		// Token: 0x04000299 RID: 665
		private Dictionary<int, Vec2d> boxSizes = new Dictionary<int, Vec2d>();

		// Token: 0x0400029A RID: 666
		private TechBlock selectedTechBlock;

		// Token: 0x0400029B RID: 667
		private GuiComposer currentComposer;

		// Token: 0x0400029C RID: 668
		private GuildResearch.GuiElementDragCanvas canvasElement;

		// Token: 0x0400029D RID: 669
		private double detailsSectionTop;

		// Token: 0x0400029E RID: 670
		private List<TechBlock> ageFilteredBlocks = new List<TechBlock>();

		// Token: 0x0400029F RID: 671
		private const string TechInfoKey = "techInfo";

		// Token: 0x040002A0 RID: 672
		private const string TechTitleKey = "techDetailTitle";

		// Token: 0x040002A1 RID: 673
		private const string TechDescKey = "techDetailDesc";

		// Token: 0x040002A2 RID: 674
		private const string TechStatusKey = "techDetailStatus";

		// Token: 0x040002A3 RID: 675
		private const string TechProgressKey = "techDetailProgress";

		// Token: 0x040002A4 RID: 676
		private const string TechResourcesKey = "techDetailResources";

		// Token: 0x040002A5 RID: 677
		private const string ContributeButtonKey = "contributeButton";

		// Token: 0x040002A6 RID: 678
		private readonly ActionConsumable onRefresh;

		// Token: 0x040002A7 RID: 679
		private Dictionary<string, List<GuildResearch.ContributionItem>> pendingContributionPlan;

		// Token: 0x02000139 RID: 313
		[Nullable(0)]
		private class ContributionItem
		{
			// Token: 0x1700028E RID: 654
			// (get) Token: 0x06000B64 RID: 2916 RVA: 0x00047677 File Offset: 0x00045877
			// (set) Token: 0x06000B65 RID: 2917 RVA: 0x0004767F File Offset: 0x0004587F
			public ItemSlot Slot { get; set; }

			// Token: 0x1700028F RID: 655
			// (get) Token: 0x06000B66 RID: 2918 RVA: 0x00047688 File Offset: 0x00045888
			// (set) Token: 0x06000B67 RID: 2919 RVA: 0x00047690 File Offset: 0x00045890
			public int Amount { get; set; }

			// Token: 0x17000290 RID: 656
			// (get) Token: 0x06000B68 RID: 2920 RVA: 0x00047699 File Offset: 0x00045899
			// (set) Token: 0x06000B69 RID: 2921 RVA: 0x000476A1 File Offset: 0x000458A1
			public string ItemName { get; set; }
		}

		// Token: 0x0200013A RID: 314
		[Nullable(0)]
		internal class Connection
		{
			// Token: 0x17000291 RID: 657
			// (get) Token: 0x06000B6B RID: 2923 RVA: 0x000476B2 File Offset: 0x000458B2
			// (set) Token: 0x06000B6C RID: 2924 RVA: 0x000476BA File Offset: 0x000458BA
			public int FromId { get; set; }

			// Token: 0x17000292 RID: 658
			// (get) Token: 0x06000B6D RID: 2925 RVA: 0x000476C3 File Offset: 0x000458C3
			// (set) Token: 0x06000B6E RID: 2926 RVA: 0x000476CB File Offset: 0x000458CB
			public int ToId { get; set; }

			// Token: 0x17000293 RID: 659
			// (get) Token: 0x06000B6F RID: 2927 RVA: 0x000476D4 File Offset: 0x000458D4
			// (set) Token: 0x06000B70 RID: 2928 RVA: 0x000476DC File Offset: 0x000458DC
			public double[] LineColor { get; set; } = new double[]
			{
				1.0,
				1.0,
				1.0
			};

			// Token: 0x17000294 RID: 660
			// (get) Token: 0x06000B71 RID: 2929 RVA: 0x000476E5 File Offset: 0x000458E5
			// (set) Token: 0x06000B72 RID: 2930 RVA: 0x000476ED File Offset: 0x000458ED
			public double LineWidth { get; set; } = 2.0;

			// Token: 0x17000295 RID: 661
			// (get) Token: 0x06000B73 RID: 2931 RVA: 0x000476F6 File Offset: 0x000458F6
			// (set) Token: 0x06000B74 RID: 2932 RVA: 0x000476FE File Offset: 0x000458FE
			public bool ShowArrow { get; set; }
		}

		// Token: 0x0200013B RID: 315
		[Nullable(0)]
		private class GuiElementDragCanvas : GuiElement
		{
			// Token: 0x06000B76 RID: 2934 RVA: 0x00047735 File Offset: 0x00045935
			public GuiElementDragCanvas(ICoreClientAPI capi, ElementBounds bounds, GuildResearch parent) : base(capi, bounds)
			{
				this.parent = parent;
			}

			// Token: 0x06000B77 RID: 2935 RVA: 0x00047758 File Offset: 0x00045958
			public void MarkDirty()
			{
				this.needsRedraw = true;
			}

			// Token: 0x06000B78 RID: 2936 RVA: 0x00047761 File Offset: 0x00045961
			public void Cleanup()
			{
				if (this.cachedTextureId != 0)
				{
					this.api.Gui.DeleteTexture(this.cachedTextureId);
					this.cachedTextureId = 0;
				}
			}

			// Token: 0x06000B79 RID: 2937 RVA: 0x00047788 File Offset: 0x00045988
			public override void ComposeElements(Context ctx, ImageSurface surface)
			{
				base.ComposeElements(ctx, surface);
				this.buttonBounds.Clear();
				for (int i = 0; i < this.parent.techBlocks.Count; i++)
				{
					TechBlock block = this.parent.techBlocks[i];
					if (this.parent.ageFilteredBlocks.Any((TechBlock b) => b.Id == block.Id) && !(block.Position == null))
					{
						Vec2d size = this.parent.boxSizes.ContainsKey(block.Id) ? this.parent.boxSizes[block.Id] : new Vec2d(120.0, 50.0);
						double num = block.Position.X + this.parent.canvasOffset.X;
						double relativeY = block.Position.Y + this.parent.canvasOffset.Y;
						ElementBounds bounds = ElementBounds.Fixed(num, relativeY, size.X, size.Y);
						this.buttonBounds[i] = new GuildResearch.GuiElementDragCanvas.BoxBounds
						{
							Bounds = bounds,
							Size = size
						};
					}
				}
				this.needsRedraw = true;
			}

			// Token: 0x06000B7A RID: 2938 RVA: 0x000478F0 File Offset: 0x00045AF0
			public override void RenderInteractiveElements(float deltaTime)
			{
				if (this.needsRedraw)
				{
					if (this.cachedTextureId != 0)
					{
						this.api.Gui.DeleteTexture(this.cachedTextureId);
						this.cachedTextureId = 0;
					}
					using (ImageSurface surface = new ImageSurface(0, (int)this.Bounds.InnerWidth, (int)this.Bounds.InnerHeight))
					{
						using (Context ctx = new Context(surface))
						{
							this.DrawConnectionLines(ctx);
							foreach (KeyValuePair<int, GuildResearch.GuiElementDragCanvas.BoxBounds> kvp in this.buttonBounds)
							{
								int blockIndex = kvp.Key;
								GuildResearch.GuiElementDragCanvas.BoxBounds boxBounds = kvp.Value;
								TechBlock block = this.parent.techBlocks[blockIndex];
								GuildResearch.GuiElementDragCanvas.TechBlockState techState = this.GetTechBlockState(block);
								ctx.Save();
								ctx.Translate(boxBounds.Bounds.fixedX, boxBounds.Bounds.fixedY);
								this.DrawButton(ctx, boxBounds.Size, block.Text, techState);
								ctx.Restore();
							}
							surface.Flush();
							this.cachedTextureId = this.api.Gui.LoadCairoTexture(surface, false);
						}
					}
					this.needsRedraw = false;
				}
				if (this.cachedTextureId != 0)
				{
					this.api.Render.Render2DTexture(this.cachedTextureId, (float)((int)this.Bounds.renderX), (float)((int)this.Bounds.renderY), (float)((int)this.Bounds.InnerWidth), (float)((int)this.Bounds.InnerHeight), 255f, null);
				}
			}

			// Token: 0x06000B7B RID: 2939 RVA: 0x00047AB8 File Offset: 0x00045CB8
			private void DrawConnectionLines(Context ctx)
			{
				List<GuildResearch.Connection> visibleConnections = this.parent.GetVisibleConnections();
				HashSet<int> ageFilteredIds = new HashSet<int>(from b in this.parent.ageFilteredBlocks
				select b.Id);
				ctx.Save();
				using (List<GuildResearch.Connection>.Enumerator enumerator = visibleConnections.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						GuildResearch.Connection connection = enumerator.Current;
						TechBlock fromBlock = this.parent.techBlocks.Find((TechBlock b) => b.Id == connection.FromId);
						TechBlock toBlock = this.parent.techBlocks.Find((TechBlock b) => b.Id == connection.ToId);
						if (fromBlock != null && toBlock != null && ageFilteredIds.Contains(fromBlock.Id) && ageFilteredIds.Contains(toBlock.Id) && fromBlock.Position != null && toBlock.Position != null)
						{
							Vec2d fromSize = this.parent.boxSizes.ContainsKey(fromBlock.Id) ? this.parent.boxSizes[fromBlock.Id] : new Vec2d(120.0, 50.0);
							Vec2d toSize = this.parent.boxSizes.ContainsKey(toBlock.Id) ? this.parent.boxSizes[toBlock.Id] : new Vec2d(120.0, 50.0);
							double fromX = fromBlock.Position.X + this.parent.canvasOffset.X + fromSize.X / 2.0;
							double fromY = fromBlock.Position.Y + this.parent.canvasOffset.Y + fromSize.Y;
							double toX = toBlock.Position.X + this.parent.canvasOffset.X + toSize.X / 2.0;
							double toY = toBlock.Position.Y + this.parent.canvasOffset.Y;
							if (fromBlock.Age != toBlock.Age)
							{
								ctx.SetSourceRGBA(0.8, 0.6, 0.2, 1.0);
							}
							else
							{
								ctx.SetSourceRGBA(connection.LineColor[0], connection.LineColor[1], connection.LineColor[2], 1.0);
							}
							ctx.LineWidth = connection.LineWidth;
							double[] dashPattern = new double[]
							{
								5.0,
								3.0
							};
							ctx.SetDash(dashPattern, 0.0);
							List<int> unlocksIds = fromBlock.UnlocksIds;
							if (((unlocksIds != null) ? unlocksIds.Count : 0) > 1)
							{
								double midY = (fromY + toY) / 2.0;
								ctx.MoveTo(fromX, fromY);
								ctx.LineTo(fromX, midY);
								ctx.LineTo(toX, midY);
								ctx.LineTo(toX, toY);
								ctx.Stroke();
							}
							else
							{
								ctx.MoveTo(fromX, fromY);
								ctx.LineTo(toX, toY);
								ctx.Stroke();
							}
							ctx.SetDash(new double[0], 0.0);
						}
					}
				}
				ctx.Restore();
			}

			// Token: 0x06000B7C RID: 2940 RVA: 0x00047E6C File Offset: 0x0004606C
			private GuildResearch.GuiElementDragCanvas.TechBlockState GetTechBlockState(TechBlock block)
			{
				if (this.parent.currentGuild == null)
				{
					return GuildResearch.GuiElementDragCanvas.TechBlockState.AgeLocked;
				}
				TechBlocksConfig config = this.parent.modSystem.TechBlocksConfig;
				if (config != null && !config.IsAgeEnabled(block.Age))
				{
					return GuildResearch.GuiElementDragCanvas.TechBlockState.AgeLocked;
				}
				if (this.parent.currentGuild.IsTechUnlocked(block.Id))
				{
					return GuildResearch.GuiElementDragCanvas.TechBlockState.Unlocked;
				}
				if (!this.parent.ArePrerequisitesUnlocked(block))
				{
					return GuildResearch.GuiElementDragCanvas.TechBlockState.Locked;
				}
				return GuildResearch.GuiElementDragCanvas.TechBlockState.Available;
			}

			// Token: 0x06000B7D RID: 2941 RVA: 0x00047ED8 File Offset: 0x000460D8
			private void DrawButton(Context ctx, Vec2d size, string text, GuildResearch.GuiElementDragCanvas.TechBlockState state)
			{
				if (state == GuildResearch.GuiElementDragCanvas.TechBlockState.AgeLocked || state == GuildResearch.GuiElementDragCanvas.TechBlockState.Locked)
				{
					ctx.SetSourceRGBA(0.1, 0.1, 0.1, 0.6);
				}
				else
				{
					ctx.SetSourceRGBA(0.2, 0.2, 0.2, 0.8);
				}
				this.RoundRectangle(ctx, 0.0, 0.0, size.X, size.Y, 5.0);
				ctx.Fill();
				switch (state)
				{
				case GuildResearch.GuiElementDragCanvas.TechBlockState.Unlocked:
					ctx.SetSourceRGBA(0.2, 0.8, 0.2, 1.0);
					break;
				case GuildResearch.GuiElementDragCanvas.TechBlockState.Available:
					ctx.SetSourceRGBA(0.9, 0.7, 0.1, 1.0);
					break;
				case GuildResearch.GuiElementDragCanvas.TechBlockState.Locked:
					ctx.SetSourceRGBA(0.8, 0.2, 0.2, 1.0);
					break;
				case GuildResearch.GuiElementDragCanvas.TechBlockState.AgeLocked:
					ctx.SetSourceRGBA(0.5, 0.1, 0.1, 1.0);
					break;
				}
				ctx.LineWidth = 2.0;
				this.RoundRectangle(ctx, 0.0, 0.0, size.X, size.Y, 5.0);
				ctx.Stroke();
				if (state == GuildResearch.GuiElementDragCanvas.TechBlockState.Locked || state == GuildResearch.GuiElementDragCanvas.TechBlockState.AgeLocked)
				{
					int lockSize = 12;
					double lockX = size.X - (double)lockSize - 5.0;
					int lockY = 5;
					ctx.SetSourceRGBA(0.8, 0.3, 0.3, 1.0);
					ctx.Rectangle(lockX + 2.0, (double)(lockY + 5), (double)(lockSize - 4), (double)(lockSize - 5));
					ctx.Fill();
					ctx.Arc(lockX + (double)(lockSize / 2), (double)(lockY + 5), (double)(lockSize / 3), 3.141592653589793, 0.0);
					ctx.LineWidth = 1.5;
					ctx.Stroke();
				}
				else if (state == GuildResearch.GuiElementDragCanvas.TechBlockState.Unlocked)
				{
					int checkSize = 12;
					double checkX = size.X - (double)checkSize - 5.0;
					int checkY = 5;
					ctx.SetSourceRGBA(0.2, 0.8, 0.2, 1.0);
					ctx.LineWidth = 2.0;
					ctx.MoveTo(checkX, (double)(checkY + checkSize / 2));
					ctx.LineTo(checkX + (double)(checkSize / 3), (double)(checkY + checkSize - 2));
					ctx.LineTo(checkX + (double)checkSize, (double)checkY);
					ctx.Stroke();
				}
				CairoFont font = CairoFont.WhiteSmallText();
				if (state == GuildResearch.GuiElementDragCanvas.TechBlockState.Locked || state == GuildResearch.GuiElementDragCanvas.TechBlockState.AgeLocked)
				{
					this.DrawTextWrapped(ctx, text, size.X / 2.0, size.Y / 2.0, size.X - 20.0, font, 0.5);
					return;
				}
				this.DrawTextWrapped(ctx, text, size.X / 2.0, size.Y / 2.0, size.X - 20.0, font, 1.0);
			}

			// Token: 0x06000B7E RID: 2942 RVA: 0x0004826C File Offset: 0x0004646C
			private void RoundRectangle(Context ctx, double x, double y, double width, double height, double radius)
			{
				ctx.NewPath();
				ctx.Arc(x + width - radius, y + radius, radius, -1.5707963267948966, 0.0);
				ctx.Arc(x + width - radius, y + height - radius, radius, 0.0, 1.5707963267948966);
				ctx.Arc(x + radius, y + height - radius, radius, 1.5707963267948966, 3.141592653589793);
				ctx.Arc(x + radius, y + radius, radius, 3.141592653589793, 4.71238898038469);
				ctx.ClosePath();
			}

			// Token: 0x06000B7F RID: 2943 RVA: 0x0004831C File Offset: 0x0004651C
			private void DrawTextCentered(Context ctx, string text, double centerX, double centerY, CairoFont font, double alpha = 1.0)
			{
				font.SetupContext(ctx);
				TextExtents extents = ctx.TextExtents(text);
				double x = centerX - (extents.Width / 2.0 + extents.XBearing);
				double y = centerY - (extents.Height / 2.0 + extents.YBearing);
				ctx.MoveTo(x, y);
				ctx.SetSourceRGBA(1.0, 1.0, 1.0, alpha);
				ctx.ShowText(text);
			}

			// Token: 0x06000B80 RID: 2944 RVA: 0x000483A4 File Offset: 0x000465A4
			private void DrawTextWrapped(Context ctx, string text, double centerX, double centerY, double maxWidth, CairoFont font, double alpha = 1.0)
			{
				font.SetupContext(ctx);
				List<string> lines = this.WrapTextForDrawing(ctx, text, maxWidth);
				double lineHeight = font.GetFontExtents().Height;
				double totalHeight = (double)lines.Count * lineHeight;
				double startY = centerY - totalHeight / 2.0;
				for (int i = 0; i < lines.Count; i++)
				{
					string line = lines[i];
					TextExtents extents = ctx.TextExtents(line);
					double x = centerX - (extents.Width / 2.0 + extents.XBearing);
					double y = startY + (double)i * lineHeight + lineHeight * 0.8;
					ctx.MoveTo(x, y);
					ctx.SetSourceRGBA(1.0, 1.0, 1.0, alpha);
					ctx.ShowText(line);
				}
			}

			// Token: 0x06000B81 RID: 2945 RVA: 0x00048484 File Offset: 0x00046684
			private List<string> WrapTextForDrawing(Context ctx, string text, double maxWidth)
			{
				List<string> lines = new List<string>();
				string[] array = text.Split(' ', StringSplitOptions.None);
				string currentLine = "";
				foreach (string word in array)
				{
					string testLine = string.IsNullOrEmpty(currentLine) ? word : (currentLine + " " + word);
					if (ctx.TextExtents(testLine).Width > maxWidth && !string.IsNullOrEmpty(currentLine))
					{
						lines.Add(currentLine);
						currentLine = word;
					}
					else
					{
						currentLine = testLine;
					}
				}
				if (!string.IsNullOrEmpty(currentLine))
				{
					lines.Add(currentLine);
				}
				if (lines.Count <= 0)
				{
					return new List<string>
					{
						text
					};
				}
				return lines;
			}

			// Token: 0x06000B82 RID: 2946 RVA: 0x00048528 File Offset: 0x00046728
			public override void OnMouseDownOnElement(ICoreClientAPI api, MouseEvent args)
			{
				double localX = (double)args.X - this.Bounds.renderX;
				double localY = (double)args.Y - this.Bounds.renderY;
				foreach (KeyValuePair<int, GuildResearch.GuiElementDragCanvas.BoxBounds> kvp in this.buttonBounds)
				{
					int blockIndex = kvp.Key;
					GuildResearch.GuiElementDragCanvas.BoxBounds boxBounds = kvp.Value;
					if (localX >= boxBounds.Bounds.fixedX && localX <= boxBounds.Bounds.fixedX + boxBounds.Size.X && localY >= boxBounds.Bounds.fixedY && localY <= boxBounds.Bounds.fixedY + boxBounds.Size.Y)
					{
						this.parent.OnBoxClick(blockIndex);
						args.Handled = true;
						return;
					}
				}
				this.parent.isDragging = true;
				this.parent.lastMousePos = new Vec2d((double)args.X, (double)args.Y);
				args.Handled = true;
			}

			// Token: 0x06000B83 RID: 2947 RVA: 0x00048654 File Offset: 0x00046854
			public override void OnMouseUp(ICoreClientAPI api, MouseEvent args)
			{
				this.parent.isDragging = false;
			}

			// Token: 0x06000B84 RID: 2948 RVA: 0x00048664 File Offset: 0x00046864
			public override void OnMouseMove(ICoreClientAPI api, MouseEvent args)
			{
				if (this.parent.isDragging)
				{
					Vec2d currentMousePos = new Vec2d((double)args.X, (double)args.Y);
					double deltaX = currentMousePos.X - this.parent.lastMousePos.X;
					double deltaY = currentMousePos.Y - this.parent.lastMousePos.Y;
					this.parent.canvasOffset.X += deltaX;
					this.parent.canvasOffset.Y += deltaY;
					this.parent.lastMousePos = currentMousePos;
					using (ImageSurface surface = new ImageSurface(0, (int)this.Bounds.InnerWidth, (int)this.Bounds.InnerHeight))
					{
						using (Context ctx = new Context(surface))
						{
							this.ComposeElements(ctx, surface);
						}
					}
					args.Handled = true;
				}
			}

			// Token: 0x0400054E RID: 1358
			private GuildResearch parent;

			// Token: 0x0400054F RID: 1359
			private Dictionary<int, GuildResearch.GuiElementDragCanvas.BoxBounds> buttonBounds = new Dictionary<int, GuildResearch.GuiElementDragCanvas.BoxBounds>();

			// Token: 0x04000550 RID: 1360
			private int cachedTextureId;

			// Token: 0x04000551 RID: 1361
			private bool needsRedraw = true;

			// Token: 0x02000182 RID: 386
			[Nullable(0)]
			private class BoxBounds
			{
				// Token: 0x17000296 RID: 662
				// (get) Token: 0x06000C45 RID: 3141 RVA: 0x0004957C File Offset: 0x0004777C
				// (set) Token: 0x06000C46 RID: 3142 RVA: 0x00049584 File Offset: 0x00047784
				public ElementBounds Bounds { get; set; }

				// Token: 0x17000297 RID: 663
				// (get) Token: 0x06000C47 RID: 3143 RVA: 0x0004958D File Offset: 0x0004778D
				// (set) Token: 0x06000C48 RID: 3144 RVA: 0x00049595 File Offset: 0x00047795
				public Vec2d Size { get; set; }
			}

			// Token: 0x02000183 RID: 387
			[NullableContext(0)]
			private enum TechBlockState
			{
				// Token: 0x040005E3 RID: 1507
				Unlocked,
				// Token: 0x040005E4 RID: 1508
				Available,
				// Token: 0x040005E5 RID: 1509
				Locked,
				// Token: 0x040005E6 RID: 1510
				AgeLocked
			}
		}
	}
}
