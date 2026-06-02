using Cairo;
using SOAGuildsAndKingdoms.src.guilds;
using SOAGuildsAndKingdoms.src.network;
using SOAGuildsAndKingdoms.src.techblock;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace SOAGuildsAndKingdoms.src.gui.tabs
{
    internal class GuildResearch : GuildTabContent
    {
        // Define connections between tech blocks (from tech block ID to tech block ID)
        // Connections can only be made between adjacent levels (level N to level N+1)
        // Connections are now generated automatically based on TechBlock.UnlocksIds
        private List<Connection> connections = new List<Connection>();

        private Vec2d canvasOffset = new Vec2d(0, 0);
        private ElementBounds canvasBounds;
        private const int FixedBoxWidth = 120; // Fixed width for all boxes
        private const int MinBoxHeight = 50;
        private const int TextPadding = 10; // Padding around text inside boxes
        private const int CanvasWidth = 500;
        private const int CanvasHeight = 300;
        private TechAge currentAge = TechAge.Stone;

        // Drag state tracking
        private bool isDragging = false;
        private Vec2d lastMousePos = new Vec2d(0, 0);

        // Layout configuration constants
        private const int LevelSpacing = 140; // Vertical spacing between levels
        private const int BoxSpacing = 150;    // Horizontal spacing between boxes in same level
        private const int StartX = 60;        // Starting X position for layout
        private const int StartY = 40;        // Starting Y position for layout
        private List<TechBlock> techBlocks = new List<TechBlock>();

        // Store calculated box sizes for each tech block
        private Dictionary<int, Vec2d> boxSizes = new Dictionary<int, Vec2d>();

        // Track selected tech block for displaying details
        private TechBlock selectedTechBlock = null;

        // Store composer reference for dynamic updates
        private GuiComposer currentComposer = null;
        private GuiElementDragCanvas canvasElement = null;
        private double detailsSectionTop = 0;

        // Store filtered tech blocks for current age
        private List<TechBlock> ageFilteredBlocks = new List<TechBlock>();

        // Keys for dynamic elements
        private const string TechInfoKey = "techInfo";
        private const string TechTitleKey = "techDetailTitle";
        private const string TechDescKey = "techDetailDesc";
        private const string TechStatusKey = "techDetailStatus";
        private const string TechProgressKey = "techDetailProgress";
        private const string TechResourcesKey = "techDetailResources";
        private const string ContributeButtonKey = "contributeButton";

        private readonly ActionConsumable onRefresh;
        private Dictionary<string, List<ContributionItem>> pendingContributionPlan = null;

        public GuildResearch(ICoreClientAPI capi, SOAGuildsAndKingdomsModSystem modSystem, GuildSummary? currentGuild, ActionConsumable onRefresh) : base(capi, modSystem, currentGuild)
        {
            this.onRefresh = onRefresh;

            // Register callback for tech contribution responses
            modSystem.NetworkHandler.RegisterTechContributionCallback(OnTechContributionResponse);
        }

        /// <summary>
        /// Cleanup method to prevent memory leaks - call this when the tab is closed/disposed
        /// </summary>
        public void Dispose()
        {
            // Unregister callback to prevent memory leak
            modSystem.NetworkHandler.UnregisterTechContributionCallback();

            // Cleanup canvas element (which will delete its cached texture)
            canvasElement?.Cleanup();
            canvasElement = null;

            // Clear references to GUI elements
            currentComposer = null;
            selectedTechBlock = null;
            pendingContributionPlan = null;

            // Clear collections
            connections?.Clear();
            boxSizes?.Clear();
            ageFilteredBlocks?.Clear();

            // Note: techBlocks is a reference to modSystem.TechBlocks, don't clear it
        }

        public override double AddContent(GuiComposer composer, double startTop)
        {
            // Store composer reference for later updates
            currentComposer = composer;

            // Access tech blocks from the mod system
            techBlocks = modSystem.TechBlocks;

            // Update age-filtered blocks
            UpdateAgeFilteredBlocks();

            // Calculate box sizes first
            CalculateBoxSizes();

            // Generate connections from UnlocksIds before calculating positions
            connections = GenerateConnections();
            // Calculate positions before setting up the dialog
            CalculateBoxPositions();

            var top = startTop;
            var elementHeight = 25.0;
            var spacing = 10.0;

            // Age tab buttons
            var tabWidth = 100;
            var tabHeight = 25;
            var tabSpacing = 5;

            var config = modSystem.TechBlocksConfig;

            // Only show tabs for ages that are enabled in the configuration
            var allAges = Enum.GetValues(typeof(TechAge)).Cast<TechAge>().ToList();
            var enabledAges = config != null
                ? allAges.Where(age => config.IsAgeEnabled(age)).ToList()
                : allAges;

            for (int i = 0; i < enabledAges.Count; i++)
            {
                var age = enabledAges[i];
                var tabBounds = ElementBounds.Fixed(i * (tabWidth + tabSpacing), top, tabWidth, tabHeight);

                // Highlight the current age
                var isCurrentAge = age == currentAge;
                var buttonKey = $"ageTab_{age}";

                composer.AddSmallButton(age.ToString(), () => OnAgeTabClicked(age), tabBounds,
                    isCurrentAge ? EnumButtonStyle.MainMenu : EnumButtonStyle.Normal, buttonKey);
            }

            top += tabHeight + spacing;

            // Instructions
            var instructionBounds = ElementBounds.Fixed(0, top, CanvasWidth, elementHeight);
            composer.AddStaticText("Drag the canvas to reveal tech tree nodes!",
                CairoFont.WhiteDetailText(), instructionBounds);

            top += elementHeight + spacing;

            // Canvas area with inset border
            canvasBounds = ElementBounds.Fixed(0, top, CanvasWidth, CanvasHeight);
            composer.AddInset(canvasBounds, 3);

            // Add the drag canvas element that will contain all visual elements
            var interactiveBounds = ElementBounds.Fixed(3, 3, CanvasWidth - 6, CanvasHeight - 6).WithParent(canvasBounds);
            canvasElement = new GuiElementDragCanvas(capi, interactiveBounds, this);
            composer.AddInteractiveElement(canvasElement);

            top += CanvasHeight + spacing;

            // Show tech info for the current age
            if (currentGuild != null)
            {
                var techInfoBounds = ElementBounds.Fixed(0, top, CanvasWidth, elementHeight);
                composer.AddDynamicText("", CairoFont.WhiteSmallText(), techInfoBounds, TechInfoKey);
                UpdateTechInfoText();

                top += elementHeight + spacing;

                // Store the position where details section starts
                detailsSectionTop = top;

                // Add placeholder dynamic text elements for tech details
                // These will be updated when a tech block is clicked
                var separatorBounds = ElementBounds.Fixed(0, top, CanvasWidth, 2);
                composer.AddInset(separatorBounds, 1);
                top += 5;

                var titleBounds = ElementBounds.Fixed(10, top, CanvasWidth - 20, 20);
                composer.AddDynamicText("", CairoFont.WhiteSmallText().WithWeight(Cairo.FontWeight.Bold), titleBounds, TechTitleKey);
                top += 25;

                var descBounds = ElementBounds.Fixed(10, top, CanvasWidth - 20, 40);
                composer.AddDynamicText("", CairoFont.WhiteDetailText(), descBounds, TechDescKey);
                top += 45;

                var statusBounds = ElementBounds.Fixed(10, top, CanvasWidth - 20, 20);
                composer.AddDynamicText("", CairoFont.WhiteSmallText(), statusBounds, TechStatusKey);
                top += 25;

                var progressBounds = ElementBounds.Fixed(10, top, CanvasWidth - 20, 20);
                composer.AddDynamicText("", CairoFont.WhiteSmallText(), progressBounds, TechProgressKey);
                top += 25;

                // Create scrollable resources area
                var clipBounds = ElementBounds.Fixed(10, top, CanvasWidth - 30, 120);
                var scrollbarBounds = ElementBounds.Fixed(clipBounds.fixedWidth + 2, 1, 20, clipBounds.fixedHeight + 2).WithParent(clipBounds);

                // Inset inside the clip bounds
                var insetBounds = ElementBounds.Fixed(2, 2, clipBounds.fixedWidth - 4, clipBounds.fixedHeight);

                // Content bounds - width matches inset, height is large to allow scrolling
                var contentBounds = ElementBounds.Fixed(0, 0, insetBounds.fixedWidth - 10, 500);

                composer.BeginClip(clipBounds);
                composer.AddInset(insetBounds.WithParent(clipBounds), 2);

                composer.BeginChildElements(insetBounds.WithParent(clipBounds));
                composer.AddDynamicText("", CairoFont.WhiteDetailText(), contentBounds, TechResourcesKey);
                composer.EndChildElements();

                composer.EndClip();

                composer.AddVerticalScrollbar(OnResourcesScroll, scrollbarBounds, "resourcesScrollbar");

                // Initialize scrollbar with proper heights
                var scrollbar = composer.GetScrollbar("resourcesScrollbar");
                scrollbar.SetHeights((float)clipBounds.fixedHeight, (float)contentBounds.fixedHeight);
                scrollbar.Bounds.CalcWorldBounds();
                scrollbar.SetScrollbarPosition(0);

                top += 125;

                // Add contribute button (for guild research)
                var buttonBounds = ElementBounds.Fixed(10, top, 150, 30);
                composer.AddSmallButton("Contribute Resources", OnContributeClicked, buttonBounds, EnumButtonStyle.Normal, ContributeButtonKey);
                top += 40;

                // Update details section if a tech is already selected
                if (selectedTechBlock != null)
                {
                    UpdateDetailsSection();
                }
            }

            return top;
        }

        private List<Connection> GenerateConnections()
        {
            var connections = new List<Connection>();

            foreach (var block in techBlocks)
            {
                if (block.UnlocksIds == null || block.UnlocksIds.Count == 0)
                    continue;

                foreach (var unlockedId in block.UnlocksIds)
                {
                    // Create connection from this block to the unlocked block
                    connections.Add(new Connection
                    {
                        FromId = block.Id,
                        ToId = unlockedId,
                        LineColor = new double[] { 0.8, 0.8, 0.8 }, // Default gray color
                        LineWidth = 2
                    });
                }
            }

            return connections;
        }

        private void CalculateBoxSizes()
        {
            boxSizes.Clear();

            // Create a temporary surface to measure text
            using (ImageSurface surface = new ImageSurface(Format.Argb32, 1, 1))
            using (Context ctx = new Context(surface))
            {
                var font = CairoFont.WhiteSmallText();
                font.SetupContext(ctx);

                foreach (var block in techBlocks)
                {
                    // Use fixed width
                    double width = FixedBoxWidth;

                    // Calculate height based on wrapped text
                    var lines = WrapText(ctx, block.Text, width - (TextPadding * 2));
                    var lineHeight = font.GetFontExtents().Height;
                    double height = Math.Max(MinBoxHeight, (lines.Count * lineHeight) + (TextPadding * 2) + 5);

                    boxSizes[block.Id] = new Vec2d(width, height);
                }
            }
        }

        private List<string> WrapText(Context ctx, string text, double maxWidth)
        {
            var lines = new List<string>();
            var words = text.Split(' ');
            var currentLine = "";

            foreach (var word in words)
            {
                var testLine = string.IsNullOrEmpty(currentLine) ? word : currentLine + " " + word;
                var extents = ctx.TextExtents(testLine);

                if (extents.Width > maxWidth && !string.IsNullOrEmpty(currentLine))
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

            return lines.Count > 0 ? lines : new List<string> { text };
        }

        private void UpdateAgeFilteredBlocks()
        {
            var config = modSystem.TechBlocksConfig;

            // Start with all blocks from the current age (only if the age is enabled)
            if (config != null && !config.IsAgeEnabled(currentAge))
            {
                // If current age is not enabled, show no blocks
                ageFilteredBlocks = new List<TechBlock>();
                return;
            }

            ageFilteredBlocks = techBlocks.Where(b => b.Age == currentAge).ToList();

            // Add tech blocks from previous ages that connect to current age blocks
            // But only include prerequisites from ages that are enabled
            var currentAgeBlockIds = new HashSet<int>(ageFilteredBlocks.Select(b => b.Id));
            var prerequisiteBlocks = new List<TechBlock>();

            foreach (var currentBlock in ageFilteredBlocks)
            {
                // Find all blocks that unlock this current block (prerequisites from previous ages)
                var prerequisites = techBlocks.Where(b =>
                    b.Age < currentAge &&
                    b.UnlocksIds != null &&
                    b.UnlocksIds.Contains(currentBlock.Id) &&
                    (config == null || config.IsAgeEnabled(b.Age)) // Only show prerequisites from enabled ages
                ).ToList();

                foreach (var prereq in prerequisites)
                {
                    if (!currentAgeBlockIds.Contains(prereq.Id))
                    {
                        prerequisiteBlocks.Add(prereq);
                        currentAgeBlockIds.Add(prereq.Id);
                    }
                }
            }

            // Add the prerequisite blocks to the filtered list
            ageFilteredBlocks.AddRange(prerequisiteBlocks);
        }

        private void CalculateBoxPositions()
        {
            // Group filtered tech blocks by display level (cross-age prerequisites shown at level 1)
            var blocksByLevel = ageFilteredBlocks.GroupBy(b => GetDisplayLevel(b)).OrderBy(g => g.Key).ToList();

            // Track occupied positions at each level to prevent overlaps
            var occupiedPositions = new Dictionary<int, List<double>>(); // level -> list of X positions

            foreach (var levelGroup in blocksByLevel)
            {
                var level = levelGroup.Key;
                var levelBlocks = levelGroup.ToList();

                // Calculate Y position for this level
                var levelY = StartY + (level - 1) * LevelSpacing;

                // Initialize occupied positions for this level
                occupiedPositions[level] = new List<double>();

                if (level == 1)
                {
                    // For level 1, use simple horizontal spacing
                    var sortedBlocks = SortBoxesInLevel(levelBlocks, level);
                    for (int i = 0; i < sortedBlocks.Count; i++)
                    {
                        var block = sortedBlocks[i];
                        var xPos = StartX + i * BoxSpacing;
                        block.Position = new Vec2d(xPos, levelY);
                        occupiedPositions[level].Add(xPos);
                    }
                }
                else
                {
                    // For subsequent levels, assign each parent's leftmost child to be positioned directly underneath
                    // Track which child has been assigned to which parent
                    var assignedChildren = new HashSet<int>();
                    var parentToLeftmostChild = new Dictionary<int, TechBlock>();

                    // First pass: assign the leftmost child to each parent
                    foreach (var block in levelBlocks)
                    {
                        var prerequisites = techBlocks.Where(b =>
                            b.UnlocksIds != null &&
                            b.UnlocksIds.Contains(block.Id) &&
                            b.Position != null &&
                            GetDisplayLevel(b) == level - 1
                        ).OrderBy(p => p.Position.X).ToList();

                        if (prerequisites.Count > 0)
                        {
                            var leftmostParent = prerequisites[0];
                            // If this parent hasn't been assigned a leftmost child yet, assign this one
                            if (!parentToLeftmostChild.ContainsKey(leftmostParent.Id))
                            {
                                parentToLeftmostChild[leftmostParent.Id] = block;
                                assignedChildren.Add(block.Id);
                            }
                        }
                    }

                    // Second pass: position all children
                    // First position the assigned leftmost children directly under their parents
                    foreach (var kvp in parentToLeftmostChild.OrderBy(k =>
                    {
                        var parent = techBlocks.Find(b => b.Id == k.Key);
                        return parent?.Position?.X ?? double.MaxValue;
                    }))
                    {
                        var parentId = kvp.Key;
                        var child = kvp.Value;
                        var parent = techBlocks.Find(b => b.Id == parentId);

                        if (parent?.Position != null)
                        {
                            var targetX = parent.Position.X;
                            var finalX = FindNonOverlappingPosition(targetX, occupiedPositions[level], level);

                            child.Position = new Vec2d(finalX, levelY);
                            occupiedPositions[level].Add(finalX);
                        }
                    }

                    // Then position the remaining children
                    var remainingChildren = levelBlocks.Where(b => !assignedChildren.Contains(b.Id)).ToList();
                    foreach (var block in remainingChildren)
                    {
                        var prerequisites = techBlocks.Where(b =>
                            b.UnlocksIds != null &&
                            b.UnlocksIds.Contains(block.Id) &&
                            b.Position != null &&
                            GetDisplayLevel(b) == level - 1
                        ).OrderBy(p => p.Position.X).ToList();

                        double targetX;
                        if (prerequisites.Count > 0)
                        {
                            // Try to position near the leftmost prerequisite, but to the right of already placed children
                            targetX = prerequisites[0].Position.X;

                            // If there are already children at this X, offset to the right
                            if (occupiedPositions[level].Any(x => Math.Abs(x - targetX) < FixedBoxWidth + 30))
                            {
                                targetX = occupiedPositions[level].Max() + BoxSpacing;
                            }
                        }
                        else
                        {
                            // No prerequisites at previous level, use next available position
                            targetX = occupiedPositions[level].Count > 0
                                ? occupiedPositions[level].Max() + BoxSpacing
                                : StartX;
                        }

                        var finalX = FindNonOverlappingPosition(targetX, occupiedPositions[level], level);
                        block.Position = new Vec2d(finalX, levelY);
                        occupiedPositions[level].Add(finalX);
                    }
                }
            }
        }

        /// <summary>
        /// Finds a non-overlapping position for a box at the target X position
        /// </summary>
        private double FindNonOverlappingPosition(double targetX, List<double> occupiedPositions, int level)
        {
            // Minimum spacing between boxes
            const double minSpacing = FixedBoxWidth + 30;

            // Check if target position overlaps with any occupied position
            bool hasOverlap = occupiedPositions.Any(x => Math.Abs(x - targetX) < minSpacing);

            if (!hasOverlap)
            {
                return targetX;
            }

            // Try positions to the right and left alternately
            double rightOffset = minSpacing;
            double leftOffset = -minSpacing;

            for (int i = 0; i < 20; i++) // Max 20 attempts
            {
                // Try right
                double rightPos = targetX + rightOffset;
                if (!occupiedPositions.Any(x => Math.Abs(x - rightPos) < minSpacing))
                {
                    return rightPos;
                }

                // Try left
                double leftPos = targetX + leftOffset;
                if (leftPos >= StartX && !occupiedPositions.Any(x => Math.Abs(x - leftPos) < minSpacing))
                {
                    return leftPos;
                }

                rightOffset += minSpacing;
                leftOffset -= minSpacing;
            }

            // Fallback: place at the end
            return occupiedPositions.Count > 0 ? occupiedPositions.Max() + minSpacing : targetX;
        }

        /// <summary>
        /// Gets the display level for a tech block. Cross-age prerequisites are shown at level 1.
        /// </summary>
        private int GetDisplayLevel(TechBlock block)
        {
            // If this is a prerequisite from a previous age, display it at level 1
            if (block.Age < currentAge)
            {
                return 1;
            }

            // Otherwise use the actual level
            return block.Level;
        }

        private List<TechBlock> SortBoxesInLevel(List<TechBlock> levelBlocks, int level)
        {
            // For level 1, sort by number of outgoing connections (most connected first)
            if (level == 1)
            {
                return levelBlocks.OrderByDescending(block => block.UnlocksIds?.Count ?? 0).ToList();
            }

            // For higher levels, try to minimize connection crossings by considering incoming connections
            var sortedBlocks = new List<TechBlock>(levelBlocks);

            // Simple heuristic: sort by the average X position of connected tech blocks from previous level
            sortedBlocks.Sort((a, b) =>
            {
                var aAvgX = GetAverageConnectedX(a.Id, level - 1);
                var bAvgX = GetAverageConnectedX(b.Id, level - 1);
                return aAvgX.CompareTo(bAvgX);
            });

            return sortedBlocks;
        }

        private List<Connection> GetIncomingConnections(int blockId)
        {
            return connections.Where(c => c.ToId == blockId).ToList();
        }

        private double GetAverageConnectedX(int blockId, int fromLevel)
        {
            var incomingConnections = GetIncomingConnections(blockId);
            var connectedBlocks = incomingConnections
                .Select(c => techBlocks.Find(b => b.Id == c.FromId))
                .Where(b => b != null && b.Level == fromLevel)
                .ToList();

            if (!connectedBlocks.Any())
            {
                // If no connections, use a default position
                return StartX;
            }

            return connectedBlocks.Average(b => b.Position?.X ?? StartX);
        }

        internal bool OnBoxClick(int blockIndex)
        {
            if (blockIndex >= 0 && blockIndex < techBlocks.Count)
            {
                var block = techBlocks[blockIndex];
                var connectedBlocks = GetConnectedBoxes(block.Id);
                var connectionInfo = connectedBlocks.Count > 0 ?
                    $" Connected to: {string.Join(", ", connectedBlocks.Select(b => $"{b.Text} (L{b.Level})"))} " :
                    " No connections ";

                // Show resource requirements
                var resourceInfo = block.ResourceGroups.Count > 0 ?
                    $" Resources needed: {string.Join(", ", block.ResourceGroups.Select(r => $"{r.AmountRequired}x {r.Name}"))} " :
                    " No resources required ";

                capi.Logger.Debug($"Tech block ({block.Text}) at Level {block.Level} clicked!{connectionInfo}{resourceInfo}");

                // Open the tech node dialog instead of just showing a chat message
                ShowTechResearch(block);
            }
            return true;
        }

        private void ShowTechResearch(TechBlock block)
        {
            if (currentGuild == null)
            {
                capi.ShowChatMessage("No guild selected");
                return;
            }

            // Set the selected tech block
            selectedTechBlock = block;

            // Update the details section without refreshing the whole dialog
            UpdateDetailsSection();
        }

        private void UpdateTechInfoText()
        {
            if (currentComposer == null || currentGuild == null)
                return;

            var unlockedCount = ageFilteredBlocks.Count(t => currentGuild.IsTechUnlocked(t.Id));
            var techInfoText = $"{currentAge} Age: {unlockedCount}/{ageFilteredBlocks.Count} techs unlocked";

            currentComposer.GetDynamicText(TechInfoKey)?.SetNewText(techInfoText);
        }

        private bool ArePrerequisitesUnlocked(TechBlock techBlock)
        {
            // Find all tech blocks that unlock this tech (prerequisites)
            var prerequisites = techBlocks.Where(b =>
                b.UnlocksIds != null &&
                b.UnlocksIds.Contains(techBlock.Id)
            ).ToList();

            // If there are no prerequisites, it's unlocked by default
            if (prerequisites.Count == 0)
                return true;

            // Check if all prerequisites are unlocked
            return prerequisites.All(prereq => currentGuild.IsTechUnlocked(prereq.Id));
        }

        private void UpdateDetailsSection()
        {
            if (currentComposer == null || currentGuild == null || selectedTechBlock == null)
                return;

            var progress = currentGuild.GetOrCreateTechProgress(selectedTechBlock.Id);
            var config = modSystem.TechBlocksConfig;

            // Check if the age is enabled
            bool ageIsLocked = config != null && !config.IsAgeEnabled(selectedTechBlock.Age);

            // Get base requirements
            var baseRequirements = new Dictionary<string, int>();
            foreach (var rg in selectedTechBlock.ResourceGroups)
            {
                baseRequirements[rg.Name] = rg.AmountRequired;
            }

            // Request scaled requirements from server
            modSystem.NetworkHandler.RequestScaledRequirements(
                currentGuild.Name,
                selectedTechBlock.Id,
                baseRequirements,
                (response) => OnScaledRequirementsReceived(response, progress, ageIsLocked)
            );
        }

        private void OnScaledRequirementsReceived(ScaledRequirementsResponsePacket response, GuildTechProgress progress, bool ageIsLocked)
        {
            if (currentComposer == null || selectedTechBlock == null)
                return;

            var scaledRequirements = response.ScaledRequirements;
            var scaling = response.ResourceScaling;
            var memberCount = response.MemberCount;

            // Calculate overall progress using scaled requirements
            var totalRequired = scaledRequirements.Values.Sum();
            var totalSubmitted = selectedTechBlock.ResourceGroups.Sum(rg => progress.GetResourceGroupSubmitted(rg.Name));
            var overallPercent = totalRequired > 0 ? (totalSubmitted * 100.0) / totalRequired : 0;

            // Update title
            var titleText = $"Selected: {selectedTechBlock.Text} (Level {selectedTechBlock.Level})";
            currentComposer.GetDynamicText(TechTitleKey)?.SetNewText(titleText);

            // Update description
            var descText = !string.IsNullOrWhiteSpace(selectedTechBlock.Description)
                ? selectedTechBlock.Description
                : "";
            currentComposer.GetDynamicText(TechDescKey)?.SetNewText(descText);

            // Update status
            string statusText;
            if (progress.IsUnlocked)
            {
                statusText = "✓ UNLOCKED";
            }
            else if (ageIsLocked)
            {
                statusText = $"🔒 LOCKED - {selectedTechBlock.Age} Age is not yet enabled";
            }
            else
            {
                statusText = "";
            }
            currentComposer.GetDynamicText(TechStatusKey)?.SetNewText(statusText);

            // Update progress
            var progressText = !progress.IsUnlocked && !ageIsLocked ? $"Progress: {overallPercent:F1}%" : "";
            currentComposer.GetDynamicText(TechProgressKey)?.SetNewText(progressText);

            // Update resources
            var resourcesText = new StringBuilder();
            if (!progress.IsUnlocked && !ageIsLocked && selectedTechBlock.ResourceGroups.Count > 0)
            {
                // Show guild size and scaling info if scaling is active
                if (scaling > 1.0m)
                {
                    var scalingPercent = (scaling - 1.0m) * 100m;
                    resourcesText.AppendLine($"Guild Size: {memberCount} members (+{scalingPercent:F0}% resources)");
                    resourcesText.AppendLine();
                }

                resourcesText.AppendLine("Guild Resources:");
                foreach (var resourceGroup in selectedTechBlock.ResourceGroups)
                {
                    var submitted = progress.GetResourceGroupSubmitted(resourceGroup.Name);
                    var scaledRequired = scaledRequirements[resourceGroup.Name];
                    var groupPercent = scaledRequired > 0 ? (submitted * 100.0) / scaledRequired : 0;

                    // Show base requirement if scaling is active
                    if (scaling > 1.0m)
                    {
                        var baseRequired = resourceGroup.AmountRequired;
                        resourcesText.AppendLine($"  • {resourceGroup.Name}: {submitted}/{scaledRequired} (base: {baseRequired}) ({groupPercent:F0}%)");
                    }
                    else
                    {
                        resourcesText.AppendLine($"  • {resourceGroup.Name}: {submitted}/{scaledRequired} ({groupPercent:F0}%)");
                    }
                }
            }
            else if (ageIsLocked)
            {
                resourcesText.AppendLine($"\nThis technology belongs to the {selectedTechBlock.Age} Age,");
                resourcesText.AppendLine("which is currently disabled in the server configuration.");
            }
            currentComposer.GetDynamicText(TechResourcesKey)?.SetNewText(resourcesText.ToString());

            // Update button visibility
            var contributeButton = currentComposer.GetButton(ContributeButtonKey);
            if (contributeButton != null)
            {
                bool showGuildButton = !progress.IsUnlocked && !ageIsLocked;
                contributeButton.Enabled = showGuildButton;
                contributeButton.Visible = showGuildButton;
            }
        }

        private void OnResourcesScroll(float value)
        {
            // Get the scrollbar to access its current position
            var scrollbar = currentComposer?.GetScrollbar("resourcesScrollbar");
            if (scrollbar == null) return;

            // Find the dynamic text element
            var textElement = currentComposer.GetDynamicText(TechResourcesKey);
            if (textElement?.Bounds == null) return;

            // Calculate the scroll offset
            // The scrollbar's CurrentYPosition tells us how far we've scrolled
            var scrollOffset = scrollbar.CurrentYPosition;

            // Update the text element's own bounds directly
            textElement.Bounds.fixedY = 5 - scrollOffset; // 5 is the original Y offset from contentBounds

            // Recalculate all bounds in the hierarchy
            textElement.Bounds.CalcWorldBounds();
        }

        private bool OnContributeClicked()
        {
            if (currentGuild == null || selectedTechBlock == null)
            {
                capi.ShowChatMessage("No tech selected or no guild active");
                return false;
            }

            var config = modSystem.TechBlocksConfig;

            // Check if the age is enabled
            if (config != null && !config.IsAgeEnabled(selectedTechBlock.Age))
            {
                capi.ShowChatMessage($"Cannot contribute: {selectedTechBlock.Age} Age is not enabled yet");
                return false;
            }

            // Check if all prerequisite techs are unlocked
            if (!ArePrerequisitesUnlocked(selectedTechBlock))
            {
                capi.ShowChatMessage("Cannot contribute: prerequisite technologies must be unlocked first");
                return false;
            }

            var player = capi.World.Player;
            if (player?.Entity == null)
            {
                capi.ShowChatMessage("Player not found");
                return false;
            }

            var progress = currentGuild.GetOrCreateTechProgress(selectedTechBlock.Id);

            if (progress.IsUnlocked)
            {
                capi.ShowChatMessage("This tech is already unlocked!");
                return true;
            }

            // Get base requirements
            var baseRequirements = new Dictionary<string, int>();
            foreach (var rg in selectedTechBlock.ResourceGroups)
            {
                baseRequirements[rg.Name] = rg.AmountRequired;
            }

            // Request scaled requirements from server before scanning inventory
            modSystem.NetworkHandler.RequestScaledRequirements(
                currentGuild.Name,
                selectedTechBlock.Id,
                baseRequirements,
                (response) => ProcessContribution(response, progress, player)
            );

            return true;
        }

        private void ProcessContribution(ScaledRequirementsResponsePacket response, GuildTechProgress progress, IPlayer player)
        {
            if (currentGuild == null || selectedTechBlock == null)
                return;

            var scaledRequirements = response.ScaledRequirements;

            // Scan player inventories for matching items
            var contributionPlan = new Dictionary<string, List<ContributionItem>>();
            var totalItemsToContribute = 0;

            foreach (var resourceGroup in selectedTechBlock.ResourceGroups)
            {
                var currentSubmitted = progress.GetResourceGroupSubmitted(resourceGroup.Name);
                var scaledRequired = scaledRequirements[resourceGroup.Name];
                var remaining = scaledRequired - currentSubmitted;

                if (remaining <= 0)
                    continue;

                var itemsForGroup = new List<ContributionItem>();
                var amountNeeded = remaining;

                // Scan only survival inventory and backpacks
                foreach (var invPair in player.InventoryManager.Inventories)
                {
                    var invClassName = invPair.Key;
                    var inv = invPair.Value;

                    // Only scan character inventory and backpacks
                    if (inv.ClassName != "creative")
                    {
                        if (amountNeeded <= 0)
                            break;

                        for (int i = 0; i < inv.Count; i++)
                        {
                            var slot = inv[i];
                            if (slot.Empty)
                                continue;

                            var itemStack = slot.Itemstack;
                            var itemCode = itemStack.Collectible.Code.ToString();

                            if (resourceGroup.DoesItemMatch(itemCode))
                            {
                                var toContribute = Math.Min(itemStack.StackSize, amountNeeded);
                                itemsForGroup.Add(new ContributionItem
                                {
                                    Slot = slot,
                                    Amount = toContribute,
                                    ItemName = itemStack.GetName()
                                });

                                amountNeeded -= toContribute;
                                totalItemsToContribute += toContribute;

                                if (amountNeeded <= 0)
                                    break;
                            }
                        }
                    }
                }

                if (itemsForGroup.Count > 0)
                {
                    contributionPlan[resourceGroup.Name] = itemsForGroup;
                }
            }

            if (totalItemsToContribute == 0)
            {
                capi.ShowChatMessage("No valid items found in inventory to contribute");
                return;
            }

            // Build confirmation message
            var confirmMessage = new StringBuilder();
            confirmMessage.AppendLine($"Contribute to {selectedTechBlock.Text}?\n");

            foreach (var kvp in contributionPlan)
            {
                var resourceGroupName = kvp.Key;
                var items = kvp.Value;
                var totalAmount = items.Sum(i => i.Amount);

                confirmMessage.AppendLine($"{resourceGroupName}:");

                // Group by item name and sum amounts
                var groupedItems = items.GroupBy(i => i.ItemName)
                    .Select(g => new { Name = g.Key, Amount = g.Sum(i => i.Amount) });

                foreach (var item in groupedItems)
                {
                    confirmMessage.AppendLine($"  • {item.Amount}x {item.Name}");
                }
            }

            // Store the contribution plan for when user confirms
            pendingContributionPlan = contributionPlan;

            // Show confirmation dialog
            var confirmDialog = new ConfirmContributionDialog(capi, confirmMessage.ToString(), () =>
            {
                SendContributionToServer(contributionPlan);
            });
            confirmDialog.TryOpen();
        }


        private void SendContributionToServer(Dictionary<string, List<ContributionItem>> contributionPlan)
        {
            if (currentGuild == null || selectedTechBlock == null)
                return;

            // Convert contribution plan to DTO format
            var items = new List<ContributionItemDto>();

            foreach (var kvp in contributionPlan)
            {
                var resourceGroupName = kvp.Key;
                var contributions = kvp.Value;

                foreach (var contribution in contributions)
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

            // Send packet to server
            modSystem.NetworkHandler.SendTechContributionRequest(currentGuild.Name, selectedTechBlock.Id, items);
        }

        private void OnTechContributionResponse(TechContributionResponsePacket response)
        {
            // Show message to player
            if (!string.IsNullOrEmpty(response.Message))
            {
                capi.ShowChatMessage(response.Message);
            }

            // Refresh guild data from server to get updated tech progress
            if (response.Success)
            {
                // Request a refresh of guild data from the cached summaries
                RequestGuildRefresh();
            }

            // Clear pending contribution plan
            pendingContributionPlan = null;
        }


        /// <summary>
        /// Called when guild data has been refreshed
        /// </summary>
        protected override void OnGuildDataRefreshed(GuildSummary updatedGuild)
        {
            // Update the details section to show new progress
            if (selectedTechBlock != null)
            {
                UpdateDetailsSection();
            }

            // Update the tech info text to reflect any unlocks
            UpdateTechInfoText();
        }

        private class ContributionItem
        {
            public ItemSlot Slot { get; set; }
            public int Amount { get; set; }
            public string ItemName { get; set; }
        }

        private void ClearDetailsSection()
        {
            if (currentComposer == null)
                return;

            currentComposer.GetDynamicText(TechTitleKey)?.SetNewText("");
            currentComposer.GetDynamicText(TechDescKey)?.SetNewText("");
            currentComposer.GetDynamicText(TechStatusKey)?.SetNewText("");
            currentComposer.GetDynamicText(TechProgressKey)?.SetNewText("");
            currentComposer.GetDynamicText(TechResourcesKey)?.SetNewText("");
        }

        private List<TechBlock> GetConnectedBoxes(int blockId)
        {
            var connected = new List<TechBlock>();

            foreach (var connection in connections)
            {
                if (connection.FromId == blockId)
                {
                    var toBlock = techBlocks.Find(b => b.Id == connection.ToId);
                    if (toBlock != null) connected.Add(toBlock);
                }
                else if (connection.ToId == blockId)
                {
                    var fromBlock = techBlocks.Find(b => b.Id == connection.FromId);
                    if (fromBlock != null) connected.Add(fromBlock);
                }
            }

            return connected;
        }

        internal List<Connection> GetVisibleConnections()
        {
            var visibleConnections = new List<Connection>();
            var ageFilteredIds = new HashSet<int>(ageFilteredBlocks.Select(b => b.Id));

            foreach (var connection in connections)
            {
                // Validate the connection follows level rules before processing
                if (!IsValidConnection(connection))
                {
                    //capi.Logger.Warning($"Invalid connection detected: Tech block {connection.FromId} to tech block {connection.ToId} - skipping");
                    continue;
                }

                var fromBlock = techBlocks.Find(b => b.Id == connection.FromId);
                var toBlock = techBlocks.Find(b => b.Id == connection.ToId);

                // Show connections where both blocks are in the age-filtered list (includes cross-age prerequisites)
                if (fromBlock != null && toBlock != null &&
                    ageFilteredIds.Contains(fromBlock.Id) && ageFilteredIds.Contains(toBlock.Id) &&
                    fromBlock.Position != null && toBlock.Position != null)
                {
                    visibleConnections.Add(connection);
                }
            }

            return visibleConnections;
        }

        private bool IsValidConnection(Connection connection)
        {
            var fromBlock = techBlocks.Find(b => b.Id == connection.FromId);
            var toBlock = techBlocks.Find(b => b.Id == connection.ToId);

            if (fromBlock == null || toBlock == null)
                return false;

            // Cross-age connections are always valid (they're shown with special golden styling)
            if (fromBlock.Age != toBlock.Age)
                return true;

            // Within the same age, connections can only be made between adjacent levels
            // Allow both directions: level N to level N+1, or level N+1 to level N
            var levelDiff = Math.Abs(fromBlock.Level - toBlock.Level);
            return levelDiff == 1;
        }

        private bool OnAgeTabClicked(TechAge age)
        {
            // Check if the age is enabled
            var config = modSystem.TechBlocksConfig;
            if (config != null && !config.IsAgeEnabled(age))
            {
                capi.ShowChatMessage($"The {age} Age is not yet enabled on this server");
                return false;
            }

            if (currentAge != age)
            {
                currentAge = age;

                // Reset canvas offset when switching ages
                canvasOffset = new Vec2d(0, 0);

                // Clear selected tech block when switching ages
                selectedTechBlock = null;
                ClearDetailsSection();

                // Repopulate age-filtered blocks collection
                UpdateAgeFilteredBlocks();

                // Recalculate box sizes for the new age
                CalculateBoxSizes();

                // Regenerate connections and recalculate positions for the new age
                connections = GenerateConnections();
                CalculateBoxPositions();

                // Update the tech info text to reflect the new age
                UpdateTechInfoText();

                // Recompose the canvas element to update the visible tech blocks
                if (canvasElement != null)
                {
                    using (ImageSurface surface = new ImageSurface(Format.Argb32, (int)canvasElement.Bounds.InnerWidth, (int)canvasElement.Bounds.InnerHeight))
                    using (Context ctx = new Context(surface))
                    {
                        canvasElement.ComposeElements(ctx, surface);
                    }
                    // Mark for redraw (ComposeElements already does this, but being explicit)
                    canvasElement.MarkDirty();
                }

                capi.Logger.Debug($"Switched to {age} Age");
            }
            return true;
        }

        internal class Connection
        {
            public int FromId { get; set; }
            public int ToId { get; set; }
            public double[] LineColor { get; set; } = new double[] { 1.0, 1.0, 1.0 }; // RGB, white by default
            public double LineWidth { get; set; } = 2.0;
            public bool ShowArrow { get; set; } = false;
        }

        // Custom GUI element for handling canvas drag interactions and rendering
        private class GuiElementDragCanvas : GuiElement
        {
            private GuildResearch parent;
            private Dictionary<int, BoxBounds> buttonBounds = new Dictionary<int, BoxBounds>();
            private int cachedTextureId = 0;
            private bool needsRedraw = true;

            // Helper class to store both position and size
            private class BoxBounds
            {
                public ElementBounds Bounds { get; set; }
                public Vec2d Size { get; set; }
            }

            // Tech block state for visual representation
            private enum TechBlockState
            {
                Unlocked,           // Green border - tech is unlocked
                Available,          // Yellow/gold border - can contribute resources
                Locked,             // Red border - prerequisites not met
                AgeLocked          // Dark red border - age not enabled
            }

            public GuiElementDragCanvas(ICoreClientAPI capi, ElementBounds bounds, GuildResearch parent) : base(capi, bounds)
            {
                this.parent = parent;
            }

            public void MarkDirty()
            {
                needsRedraw = true;
            }

            public void Cleanup()
            {
                if (cachedTextureId != 0)
                {
                    api.Gui.DeleteTexture(cachedTextureId);
                    cachedTextureId = 0;
                }
            }

            public override void ComposeElements(Context ctx, ImageSurface surface)
            {
                base.ComposeElements(ctx, surface);

                // Update button bounds for all visible tech blocks
                buttonBounds.Clear();
                for (int i = 0; i < parent.techBlocks.Count; i++)
                {
                    var block = parent.techBlocks[i];

                    // Skip blocks not in the age-filtered list or without a position
                    if (!parent.ageFilteredBlocks.Any(b => b.Id == block.Id) || block.Position == null)
                        continue;

                    // Get the size for this block
                    var size = parent.boxSizes.ContainsKey(block.Id)
                        ? parent.boxSizes[block.Id]
                        : new Vec2d(FixedBoxWidth, MinBoxHeight);

                    // Calculate position relative to canvas
                    var relativeX = block.Position.X + parent.canvasOffset.X;
                    var relativeY = block.Position.Y + parent.canvasOffset.Y;

                    // Create bounds for this button
                    var bounds = ElementBounds.Fixed(relativeX, relativeY, size.X, size.Y);
                    buttonBounds[i] = new BoxBounds
                    {
                        Bounds = bounds,
                        Size = size
                    };
                }

                // Mark that we need to redraw the texture
                needsRedraw = true;
            }

            public override void RenderInteractiveElements(float deltaTime)
            {
                // Only regenerate texture if needed
                if (needsRedraw)
                {
                    // Delete old texture before creating new one
                    if (cachedTextureId != 0)
                    {
                        api.Gui.DeleteTexture(cachedTextureId);
                        cachedTextureId = 0;
                    }

                    // Create an image surface to render to
                    using (ImageSurface surface = new ImageSurface(Format.Argb32, (int)Bounds.InnerWidth, (int)Bounds.InnerHeight))
                    using (Context ctx = new Context(surface))
                    {
                        // Draw connection lines
                        DrawConnectionLines(ctx);

                        // Draw buttons
                        foreach (var kvp in buttonBounds)
                        {
                            var blockIndex = kvp.Key;
                            var boxBounds = kvp.Value;
                            var block = parent.techBlocks[blockIndex];

                            // Determine tech block state
                            var techState = GetTechBlockState(block);

                            ctx.Save();
                            ctx.Translate(boxBounds.Bounds.fixedX, boxBounds.Bounds.fixedY);
                            DrawButton(ctx, boxBounds.Size, block.Text, techState);
                            ctx.Restore();
                        }

                        // Generate texture from surface
                        surface.Flush();
                        cachedTextureId = api.Gui.LoadCairoTexture(surface, false);
                    }

                    needsRedraw = false;
                }

                // Render the cached texture
                if (cachedTextureId != 0)
                {
                    api.Render.Render2DTexture(
                        cachedTextureId,
                        (int)Bounds.renderX, (int)Bounds.renderY,
                        (int)Bounds.InnerWidth, (int)Bounds.InnerHeight,
                        255
                    );
                }
            }

            private void DrawConnectionLines(Context ctx)
            {
                var visibleConnections = parent.GetVisibleConnections();
                var ageFilteredIds = new HashSet<int>(parent.ageFilteredBlocks.Select(b => b.Id));

                ctx.Save();

                foreach (var connection in visibleConnections)
                {
                    var fromBlock = parent.techBlocks.Find(b => b.Id == connection.FromId);
                    var toBlock = parent.techBlocks.Find(b => b.Id == connection.ToId);

                    // Show connections where both blocks are in the age-filtered list (includes cross-age prerequisites)
                    if (fromBlock != null && toBlock != null &&
                        ageFilteredIds.Contains(fromBlock.Id) && ageFilteredIds.Contains(toBlock.Id) &&
                        fromBlock.Position != null && toBlock.Position != null)
                    {
                        // Get box sizes
                        var fromSize = parent.boxSizes.ContainsKey(fromBlock.Id)
                            ? parent.boxSizes[fromBlock.Id]
                            : new Vec2d(FixedBoxWidth, MinBoxHeight);
                        var toSize = parent.boxSizes.ContainsKey(toBlock.Id)
                            ? parent.boxSizes[toBlock.Id]
                            : new Vec2d(FixedBoxWidth, MinBoxHeight);

                        // Calculate edge points of the blocks (bottom center of parent, top center of child)
                        var fromX = fromBlock.Position.X + parent.canvasOffset.X + fromSize.X / 2;
                        var fromY = fromBlock.Position.Y + parent.canvasOffset.Y + fromSize.Y; // Bottom edge
                        var toX = toBlock.Position.X + parent.canvasOffset.X + toSize.X / 2;
                        var toY = toBlock.Position.Y + parent.canvasOffset.Y; // Top edge

                        // Set line properties - use different color for cross-age connections
                        bool isCrossAge = fromBlock.Age != toBlock.Age;
                        if (isCrossAge)
                        {
                            // Golden color for cross-age connections
                            ctx.SetSourceRGBA(0.8, 0.6, 0.2, 1.0);
                        }
                        else
                        {
                            ctx.SetSourceRGBA(connection.LineColor[0], connection.LineColor[1], connection.LineColor[2], 1.0);
                        }
                        ctx.LineWidth = connection.LineWidth;

                        // Set dash pattern for dashed lines
                        double[] dashPattern = { 5.0, 3.0 }; // 5 pixel dash, 3 pixel gap
                        ctx.SetDash(dashPattern, 0);

                        // Check if parent has multiple children
                        int childCount = fromBlock.UnlocksIds?.Count ?? 0;

                        if (childCount > 1)
                        {
                            // Draw orthogonal line (vertical-horizontal-vertical) for multiple children
                            double midY = (fromY + toY) / 2;

                            ctx.MoveTo(fromX, fromY);
                            ctx.LineTo(fromX, midY);  // Vertical down
                            ctx.LineTo(toX, midY);     // Horizontal
                            ctx.LineTo(toX, toY);      // Vertical down
                            ctx.Stroke();
                        }
                        else
                        {
                            // Draw diagonal line for single child
                            ctx.MoveTo(fromX, fromY);
                            ctx.LineTo(toX, toY);
                            ctx.Stroke();
                        }

                        // Reset dash pattern to solid
                        ctx.SetDash(new double[0], 0);
                    }
                }

                ctx.Restore();
            }

            private TechBlockState GetTechBlockState(TechBlock block)
            {
                if (parent.currentGuild == null)
                    return TechBlockState.AgeLocked;

                var config = parent.modSystem.TechBlocksConfig;

                // Check if age is locked
                if (config != null && !config.IsAgeEnabled(block.Age))
                    return TechBlockState.AgeLocked;

                // Check if unlocked
                if (parent.currentGuild.IsTechUnlocked(block.Id))
                    return TechBlockState.Unlocked;

                // Check if prerequisites are met
                if (!parent.ArePrerequisitesUnlocked(block))
                    return TechBlockState.Locked;

                // Prerequisites met but not unlocked - can contribute
                return TechBlockState.Available;
            }

            private void DrawButton(Context ctx, Vec2d size, string text, TechBlockState state)
            {
                // Draw button background
                if (state == TechBlockState.AgeLocked || state == TechBlockState.Locked)
                {
                    // Darker, grayed-out background for locked techs
                    ctx.SetSourceRGBA(0.1, 0.1, 0.1, 0.6);
                }
                else
                {
                    ctx.SetSourceRGBA(0.2, 0.2, 0.2, 0.8);
                }
                RoundRectangle(ctx, 0, 0, size.X, size.Y, 5);
                ctx.Fill();

                // Draw button border with state-based colors
                switch (state)
                {
                    case TechBlockState.Unlocked:
                        // Green border for unlocked
                        ctx.SetSourceRGBA(0.2, 0.8, 0.2, 1.0);
                        break;
                    case TechBlockState.Available:
                        // Gold/yellow border for available to contribute
                        ctx.SetSourceRGBA(0.9, 0.7, 0.1, 1.0);
                        break;
                    case TechBlockState.Locked:
                        // Red border for locked (prerequisites not met)
                        ctx.SetSourceRGBA(0.8, 0.2, 0.2, 1.0);
                        break;
                    case TechBlockState.AgeLocked:
                        // Dark red border for age locked
                        ctx.SetSourceRGBA(0.5, 0.1, 0.1, 1.0);
                        break;
                }
                ctx.LineWidth = 2;
                RoundRectangle(ctx, 0, 0, size.X, size.Y, 5);
                ctx.Stroke();

                // Draw lock icon for locked techs
                if (state == TechBlockState.Locked || state == TechBlockState.AgeLocked)
                {
                    // Draw a simple lock icon in the top-right corner
                    var lockSize = 12;
                    var lockX = size.X - lockSize - 5;
                    var lockY = 5;

                    // Draw lock body
                    ctx.SetSourceRGBA(0.8, 0.3, 0.3, 1.0);
                    ctx.Rectangle(lockX + 2, lockY + 5, lockSize - 4, lockSize - 5);
                    ctx.Fill();

                    // Draw lock shackle
                    ctx.Arc(lockX + lockSize / 2, lockY + 5, lockSize / 3, Math.PI, 0);
                    ctx.LineWidth = 1.5;
                    ctx.Stroke();
                }
                else if (state == TechBlockState.Unlocked)
                {
                    // Draw checkmark for unlocked techs
                    var checkSize = 12;
                    var checkX = size.X - checkSize - 5;
                    var checkY = 5;

                    ctx.SetSourceRGBA(0.2, 0.8, 0.2, 1.0);
                    ctx.LineWidth = 2;
                    ctx.MoveTo(checkX, checkY + checkSize / 2);
                    ctx.LineTo(checkX + checkSize / 3, checkY + checkSize - 2);
                    ctx.LineTo(checkX + checkSize, checkY);
                    ctx.Stroke();
                }

                // Draw wrapped text
                var font = CairoFont.WhiteSmallText();
                if (state == TechBlockState.Locked || state == TechBlockState.AgeLocked)
                {
                    // Dimmer text for locked techs
                    DrawTextWrapped(ctx, text, size.X / 2, size.Y / 2, size.X - (TextPadding * 2), font, 0.5);
                }
                else
                {
                    DrawTextWrapped(ctx, text, size.X / 2, size.Y / 2, size.X - (TextPadding * 2), font);
                }
            }

            private void RoundRectangle(Context ctx, double x, double y, double width, double height, double radius)
            {
                ctx.NewPath();
                ctx.Arc(x + width - radius, y + radius, radius, -Math.PI / 2, 0);
                ctx.Arc(x + width - radius, y + height - radius, radius, 0, Math.PI / 2);
                ctx.Arc(x + radius, y + height - radius, radius, Math.PI / 2, Math.PI);
                ctx.Arc(x + radius, y + radius, radius, Math.PI, Math.PI * 1.5);
                ctx.ClosePath();
            }

            private void DrawTextCentered(Context ctx, string text, double centerX, double centerY, CairoFont font, double alpha = 1.0)
            {
                font.SetupContext(ctx);

                TextExtents extents = ctx.TextExtents(text);
                double x = centerX - (extents.Width / 2 + extents.XBearing);
                double y = centerY - (extents.Height / 2 + extents.YBearing);

                ctx.MoveTo(x, y);
                ctx.SetSourceRGBA(1, 1, 1, alpha);
                ctx.ShowText(text);
            }

            private void DrawTextWrapped(Context ctx, string text, double centerX, double centerY, double maxWidth, CairoFont font, double alpha = 1.0)
            {
                font.SetupContext(ctx);

                // Get wrapped lines
                var lines = WrapTextForDrawing(ctx, text, maxWidth);
                var lineHeight = font.GetFontExtents().Height;
                var totalHeight = lines.Count * lineHeight;

                // Start from top of centered text block
                var startY = centerY - (totalHeight / 2);

                // Draw each line centered
                for (int i = 0; i < lines.Count; i++)
                {
                    var line = lines[i];
                    TextExtents extents = ctx.TextExtents(line);
                    double x = centerX - (extents.Width / 2 + extents.XBearing);
                    double y = startY + (i * lineHeight) + lineHeight * 0.8; // Offset for baseline

                    ctx.MoveTo(x, y);
                    ctx.SetSourceRGBA(1, 1, 1, alpha);
                    ctx.ShowText(line);
                }
            }

            private List<string> WrapTextForDrawing(Context ctx, string text, double maxWidth)
            {
                var lines = new List<string>();
                var words = text.Split(' ');
                var currentLine = "";

                foreach (var word in words)
                {
                    var testLine = string.IsNullOrEmpty(currentLine) ? word : currentLine + " " + word;
                    var extents = ctx.TextExtents(testLine);

                    if (extents.Width > maxWidth && !string.IsNullOrEmpty(currentLine))
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

                return lines.Count > 0 ? lines : new List<string> { text };
            }

            public override void OnMouseDownOnElement(ICoreClientAPI api, MouseEvent args)
            {
                // Check if mouse is over a button
                var localX = args.X - Bounds.renderX;
                var localY = args.Y - Bounds.renderY;

                foreach (var kvp in buttonBounds)
                {
                    var blockIndex = kvp.Key;
                    var boxBounds = kvp.Value;

                    if (localX >= boxBounds.Bounds.fixedX && localX <= boxBounds.Bounds.fixedX + boxBounds.Size.X &&
                        localY >= boxBounds.Bounds.fixedY && localY <= boxBounds.Bounds.fixedY + boxBounds.Size.Y)
                    {
                        parent.OnBoxClick(blockIndex);
                        args.Handled = true;
                        return;
                    }
                }

                // If not clicking a button, start dragging
                parent.isDragging = true;
                parent.lastMousePos = new Vec2d(args.X, args.Y);
                args.Handled = true;
            }

            public override void OnMouseUp(ICoreClientAPI api, MouseEvent args)
            {
                parent.isDragging = false;
            }

            public override void OnMouseMove(ICoreClientAPI api, MouseEvent args)
            {
                if (parent.isDragging)
                {
                    var currentMousePos = new Vec2d(args.X, args.Y);
                    var deltaX = currentMousePos.X - parent.lastMousePos.X;
                    var deltaY = currentMousePos.Y - parent.lastMousePos.Y;

                    parent.canvasOffset.X += deltaX;
                    parent.canvasOffset.Y += deltaY;

                    parent.lastMousePos = currentMousePos;

                    // Recompose the element to update button positions
                    using (ImageSurface surface = new ImageSurface(Format.Argb32, (int)Bounds.InnerWidth, (int)Bounds.InnerHeight))
                    using (Context ctx = new Context(surface))
                    {
                        ComposeElements(ctx, surface);
                    }

                    args.Handled = true;
                }
            }
        }
    }

    /// <summary>
    /// Simple confirmation dialog for resource contribution
    /// </summary>
    internal class ConfirmContributionDialog : GuiDialog
    {
        private string message;
        private Action onConfirm;

        public override string ToggleKeyCombinationCode => null;

        public ConfirmContributionDialog(ICoreClientAPI capi, string message, Action onConfirm) : base(capi)
        {
            this.message = message;
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
            bgBounds.WithChildren(
                ElementBounds.Fixed(0, 0, 400, 250)
            );

            SingleComposer = capi.Gui.CreateCompo("guildtech-contribute-confirm", dialogBounds)
                .AddShadedDialogBG(bgBounds)
                .AddDialogTitleBar("Confirm Contribution", OnTitleBarClose)
                .BeginChildElements(bgBounds);

            var textBounds = ElementBounds.Fixed(10, 30, 380, 180);
            SingleComposer.AddStaticText(message, CairoFont.WhiteDetailText(), textBounds);

            var confirmBounds = ElementBounds.Fixed(10, 220, 150, 30);
            var cancelBounds = ElementBounds.Fixed(170, 220, 150, 30);

            SingleComposer.AddSmallButton("Confirm", OnConfirmClick, confirmBounds);
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

