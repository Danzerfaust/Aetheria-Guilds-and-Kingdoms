using System;
using System.Collections.Generic;
using System.Linq;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using SOAGuildsAndKingdoms.src.guilds;

namespace SOAGuildsAndKingdoms.src.gui.tabs
{
    /// <summary>
    /// Guild tab showing node war information and signup interface
    /// Displays owned nodes, active wars, and signup options
    /// </summary>
    public class GuildNodeWarsTab : GuildTabContent
    {
        private readonly ActionConsumable onSignupForWar;
        private readonly ActionConsumable onCancelSignup;
        private readonly ActionConsumable onJoinWar;
        private readonly ActionConsumable onViewWarDetails;

        // Node war data (populated from PVP mod if available)
        private NodeWarTabData? nodeWarData;

        public GuildNodeWarsTab(ICoreClientAPI capi, SOAGuildsAndKingdomsModSystem modSystem,
            GuildSummary? currentGuild, ActionConsumable onSignupForWar,
            ActionConsumable onCancelSignup, ActionConsumable onJoinWar,
            ActionConsumable onViewWarDetails)
            : base(capi, modSystem, currentGuild)
        {
            this.onSignupForWar = onSignupForWar;
            this.onCancelSignup = onCancelSignup;
            this.onJoinWar = onJoinWar;
            this.onViewWarDetails = onViewWarDetails;
        }

        /// <summary>
        /// Set node war data from PVP mod
        /// </summary>
        public void SetNodeWarData(NodeWarTabData data)
        {
            nodeWarData = data;
        }

        public override double AddContent(GuiComposer composer, double startTop)
        {
            if (currentGuild == null) return startTop;

            var top = startTop;
            var elementHeight = 25.0;
            var spacing = 10.0;

            // Check if PVP mod is loaded
            bool pvpModAvailable = nodeWarData != null;

            if (!pvpModAvailable)
            {
                composer.AddStaticText("Node Wars", CairoFont.WhiteMediumText(),
                    ElementBounds.Fixed(0, top, 400, elementHeight));
                top += elementHeight + spacing;

                composer.AddStaticText("No data",
                    CairoFont.WhiteSmallText().WithColor(new double[] { 1, 0.5, 0.5, 1 }),
                    ElementBounds.Fixed(0, top, 400, elementHeight * 2));
                top += (elementHeight * 2) + spacing;

                return top;
            }

            // Title
            composer.AddStaticText("Node Wars", CairoFont.WhiteMediumText(),
                ElementBounds.Fixed(0, top, 400, elementHeight));
            top += elementHeight + spacing;

            // Controlled Nodes Section
            top = AddControlledNodesSection(composer, top, elementHeight, spacing);

            // Active Wars Section
            top = AddActiveWarsSection(composer, top, elementHeight, spacing);

            // Available Wars Section (for signup)
            top = AddAvailableWarsSection(composer, top, elementHeight, spacing);

            return top;
        }

        private double AddControlledNodesSection(GuiComposer composer, double top, double elementHeight, double spacing)
        {
            if (nodeWarData == null || nodeWarData.ControlledNodes.Count == 0)
            {
                composer.AddStaticText("Controlled Nodes: None", CairoFont.WhiteSmallText(),
                    ElementBounds.Fixed(0, top, 400, elementHeight));
                top += elementHeight + spacing;
                return top;
            }

            composer.AddStaticText($"Controlled Nodes ({nodeWarData.ControlledNodes.Count}):",
                CairoFont.WhiteSmallText().WithWeight(Cairo.FontWeight.Bold),
                ElementBounds.Fixed(0, top, 400, elementHeight));
            top += elementHeight + 5;

            foreach (var node in nodeWarData.ControlledNodes.Take(5)) // Limit to 5 for space
            {
                var captureTime = node.CapturedAt.HasValue
                    ? $" (captured {GetRelativeTime(node.CapturedAt.Value)})"
                    : "";

                // Add war status indicator
                string warStatusIcon = node.WarStatus switch
                {
                    1 => " 📅", // Scheduled
                    2 => " ⚔", // Active
                    3 => " ✓", // Completed
                    4 => " ✗", // Cancelled
                    _ => ""
                };

                composer.AddStaticText($"• {node.NodeName}{warStatusIcon}{captureTime}",
                    CairoFont.WhiteSmallText().WithColor(new double[] { 0.7, 1, 0.7, 1 }),
                    ElementBounds.Fixed(10, top, 390, elementHeight));
                top += elementHeight;

                // Show rewards if any
                if (node.InfluencePerDay > 0)
                {
                    composer.AddStaticText($"  +{node.InfluencePerDay} influence/day",
                        CairoFont.WhiteSmallText().WithColor(new double[] { 1, 1, 0.7, 1 }),
                        ElementBounds.Fixed(20, top, 380, elementHeight - 5));
                    top += elementHeight - 5;
                }

                // Show war status details if there's an active or scheduled war
                if (node.WarStatus.HasValue && node.WarStatus != 3 && node.WarStatus != 4) // Not completed or cancelled
                {
                    string warStatusText = node.WarStatus switch
                    {
                        1 => $"  War scheduled: {(node.WarScheduledStartTime.HasValue ? GetRelativeTime(node.WarScheduledStartTime.Value) : "soon")}",
                        2 => "  War in progress!",
                        _ => ""
                    };

                    if (!string.IsNullOrEmpty(warStatusText))
                    {
                        composer.AddStaticText(warStatusText,
                            CairoFont.WhiteSmallText().WithColor(new double[] { 1, 0.8, 0.5, 1 }),
                            ElementBounds.Fixed(20, top, 380, elementHeight - 5));
                        top += elementHeight - 5;
                    }
                }
            }

            if (nodeWarData.ControlledNodes.Count > 5)
            {
                composer.AddStaticText($"...and {nodeWarData.ControlledNodes.Count - 5} more",
                    CairoFont.WhiteSmallText().WithColor(new double[] { 0.7, 0.7, 0.7, 1 }),
                    ElementBounds.Fixed(10, top, 390, elementHeight));
                top += elementHeight;
            }

            top += spacing;
            return top;
        }

        private double AddActiveWarsSection(GuiComposer composer, double top, double elementHeight, double spacing)
        {
            if (nodeWarData == null || nodeWarData.CurrentWar == null)
            {
                composer.AddStaticText("Active Wars: None", CairoFont.WhiteSmallText(),
                    ElementBounds.Fixed(0, top, 400, elementHeight));
                top += elementHeight + spacing;
                return top;
            }

            var war = nodeWarData.CurrentWar;

            composer.AddStaticText("Current War:", CairoFont.WhiteSmallText().WithWeight(Cairo.FontWeight.Bold),
                ElementBounds.Fixed(0, top, 400, elementHeight));
            top += elementHeight + 5;

            composer.AddStaticText($"📍 {war.NodeName}",
                CairoFont.WhiteSmallText().WithColor(new double[] { 1, 0.8, 0.5, 1 }),
                ElementBounds.Fixed(10, top, 390, elementHeight));
            top += elementHeight;

            composer.AddStaticText($"Status: {war.Status}",
                CairoFont.WhiteSmallText(),
                ElementBounds.Fixed(10, top, 200, elementHeight));
            top += elementHeight;

            if (war.YourGuildProgress != null)
            {
                composer.AddStaticText($"Your Progress: {war.YourGuildProgress.CapturePoints:F0}/{war.PointsNeeded:F0}",
                    CairoFont.WhiteSmallText().WithColor(new double[] { 0.7, 1, 1, 1 }),
                    ElementBounds.Fixed(10, top, 390, elementHeight));
                top += elementHeight;

                composer.AddStaticText($"Players in Zone: {war.YourGuildProgress.PlayersInZone}",
                    CairoFont.WhiteSmallText(),
                    ElementBounds.Fixed(10, top, 200, elementHeight));
                top += elementHeight;
            }

            // Action buttons
            if (war.Status == "Active")
            {
                composer.AddSmallButton("Join War", onJoinWar,
                    ElementBounds.Fixed(0, top, 100, elementHeight), EnumButtonStyle.Normal);
            }

            composer.AddSmallButton("View Details", onViewWarDetails,
                ElementBounds.Fixed(110, top, 100, elementHeight), EnumButtonStyle.Normal);

            top += elementHeight + spacing;
            return top;
        }

        private double AddAvailableWarsSection(GuiComposer composer, double top, double elementHeight, double spacing)
        {
            if (nodeWarData == null) return top;

            // Guild is engaged if they have a signup (scheduled war) OR an active war
            bool hasSignup = nodeWarData.CurrentSignup != null;
            bool hasActiveWar = nodeWarData.CurrentWar != null;
            bool isEngaged = hasSignup || hasActiveWar;

            // Check if already signed up
            if (hasSignup)
            {
                var signup = nodeWarData.CurrentSignup;

                composer.AddStaticText("Signed Up For:", CairoFont.WhiteSmallText().WithWeight(Cairo.FontWeight.Bold),
                    ElementBounds.Fixed(0, top, 400, elementHeight));
                top += elementHeight + 5;

                composer.AddStaticText($"📍 {signup.NodeName}",
                    CairoFont.WhiteSmallText().WithColor(new double[] { 1, 1, 0.7, 1 }),
                    ElementBounds.Fixed(10, top, 390, elementHeight));
                top += elementHeight;

                composer.AddStaticText($"Starts: {GetRelativeTime(signup.WarStartTime)}",
                    CairoFont.WhiteSmallText(),
                    ElementBounds.Fixed(10, top, 390, elementHeight));
                top += elementHeight;

                composer.AddStaticText($"Signed up: {GetRelativeTime(signup.SignupTime)}",
                    CairoFont.WhiteSmallText(),
                    ElementBounds.Fixed(10, top, 390, elementHeight));
                top += elementHeight;

                // Cancel button (only for leaders)
                if (IsLeader())
                {
                    composer.AddSmallButton("Cancel Signup", onCancelSignup,
                        ElementBounds.Fixed(0, top, 120, elementHeight), EnumButtonStyle.Normal);
                }

                top += elementHeight + spacing;
            }

            // Show available wars to sign up for
            if (nodeWarData.AvailableWars.Count == 0)
            {
                composer.AddStaticText("Available Wars: None", CairoFont.WhiteSmallText(),
                    ElementBounds.Fixed(0, top, 400, elementHeight));
                top += elementHeight + spacing;
                return top;
            }

            composer.AddStaticText($"Available Wars ({nodeWarData.AvailableWars.Count}):",
                CairoFont.WhiteSmallText().WithWeight(Cairo.FontWeight.Bold),
                ElementBounds.Fixed(0, top, 400, elementHeight));
            top += elementHeight + 5;

            foreach (var war in nodeWarData.AvailableWars.Take(3)) // Show first 3
            {
                composer.AddStaticText($"📍 {war.NodeName}",
                    CairoFont.WhiteSmallText(),
                    ElementBounds.Fixed(10, top, 200, elementHeight));

                // Only show sign up button if not already signed up and is a leader
                if (!isEngaged && IsLeader())
                {
                    // Capture the war nodeId in a local variable to avoid closure issues
                    var capturedNodeId = war.NodeId;
                    composer.AddSmallButton("Sign Up", () =>
                    {
                        // Store selected war and trigger signup
                        if (nodeWarData != null)
                        {
                            nodeWarData.SelectedWarForSignup = capturedNodeId;
                        }
                        return onSignupForWar();
                    }, ElementBounds.Fixed(220, top, 80, elementHeight), EnumButtonStyle.Normal);
                }

                top += elementHeight;

                composer.AddStaticText($"  Starts: {GetRelativeTime(war.WarStartTime)}",
                    CairoFont.WhiteSmallText().WithColor(new double[] { 0.8, 0.8, 0.8, 1 }),
                    ElementBounds.Fixed(20, top, 380, elementHeight - 5));
                top += elementHeight - 5;

                composer.AddStaticText($"  Signups: {war.CurrentSignups}/{(war.MaxGuilds == 0 ? "∞" : war.MaxGuilds.ToString())}",
                    CairoFont.WhiteSmallText().WithColor(new double[] { 0.8, 0.8, 0.8, 1 }),
                    ElementBounds.Fixed(20, top, 380, elementHeight - 5));
                top += elementHeight;
            }

            if (nodeWarData.AvailableWars.Count > 3)
            {
                composer.AddStaticText($"...and {nodeWarData.AvailableWars.Count - 3} more (use /nodewar list)",
                    CairoFont.WhiteSmallText().WithColor(new double[] { 0.7, 0.7, 0.7, 1 }),
                    ElementBounds.Fixed(10, top, 390, elementHeight));
                top += elementHeight;
            }

            if (!IsLeader())
            {
                composer.AddStaticText("Only guild leaders can sign up for wars",
                    CairoFont.WhiteSmallText().WithColor(new double[] { 1, 0.7, 0.7, 1 }),
                    ElementBounds.Fixed(0, top, 400, elementHeight));
                top += elementHeight;
            }
            else if (hasActiveWar)
            {
                composer.AddStaticText("You are already in an active war",
                    CairoFont.WhiteSmallText().WithColor(new double[] { 1, 0.7, 0.7, 1 }),
                    ElementBounds.Fixed(0, top, 400, elementHeight));
                top += elementHeight;
            }
            else if (hasSignup)
            {
                composer.AddStaticText("Cancel your current signup to sign up for another war",
                    CairoFont.WhiteSmallText().WithColor(new double[] { 1, 0.7, 0.7, 1 }),
                    ElementBounds.Fixed(0, top, 400, elementHeight));
                top += elementHeight;
            }

            top += spacing;
            return top;
        }

        private string GetRelativeTime(DateTime time)
        {
            var diff = time - DateTime.UtcNow;

            if (diff.TotalDays > 1)
                return $"in {diff.TotalDays:F0} days";
            if (diff.TotalHours > 1)
                return $"in {diff.TotalHours:F0} hours";
            if (diff.TotalMinutes > 1)
                return $"in {diff.TotalMinutes:F0} minutes";
            if (diff.TotalSeconds > 0)
                return "soon";

            // Past times
            diff = diff.Duration();
            if (diff.TotalDays > 1)
                return $"{diff.TotalDays:F0} days ago";
            if (diff.TotalHours > 1)
                return $"{diff.TotalHours:F0} hours ago";
            if (diff.TotalMinutes > 1)
                return $"{diff.TotalMinutes:F0} minutes ago";

            return "just now";
        }

        /// <summary>
        /// Get the selected war ID for signup
        /// </summary>
        public string? GetSelectedWarForSignup()
        {
            return nodeWarData?.SelectedWarForSignup;
        }

        /// <summary>
        /// Get the current signup info
        /// </summary>
        public CurrentSignupInfo? GetCurrentSignup()
        {
            return nodeWarData?.CurrentSignup;
        }

        /// <summary>
        /// Get the current war info
        /// </summary>
        public CurrentWarInfo? GetCurrentWar()
        {
            return nodeWarData?.CurrentWar;
        }
    }

    #region Data Transfer Objects

    /// <summary>
    /// Data structure passed from PVP mod to guild UI
    /// Contains all node war information for display
    /// </summary>
    public class NodeWarTabData
    {
        public List<ControlledNodeInfo> ControlledNodes { get; set; } = new();
        public CurrentWarInfo? CurrentWar { get; set; }
        public List<AvailableWarInfo> AvailableWars { get; set; } = new();
        public CurrentSignupInfo? CurrentSignup { get; set; }
        public string? SelectedWarForSignup { get; set; }
    }

    public class ControlledNodeInfo
    {
        public string NodeId { get; set; } = string.Empty;
        public string NodeName { get; set; } = string.Empty;
        public DateTime? CapturedAt { get; set; }
        public int InfluencePerDay { get; set; }

        // War status fields (from node data)
        public int? WarStatus { get; set; } // NodeWarStatus enum: 0=Pending, 1=Scheduled, 2=Active, 3=Completed, 4=Cancelled
        public DateTime? WarScheduledStartTime { get; set; }
        public DateTime? WarStartedTime { get; set; }
        public DateTime? WarEndTime { get; set; }
        public int? WarSignupCount { get; set; }
        public int? WarMaxGuilds { get; set; }
        public string? WarWinnerGuildName { get; set; }
    }

    public class CurrentWarInfo
    {
        public string NodeId { get; set; } = string.Empty;
        public string NodeName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty; // "Active", "Scheduled", etc.
        public double PointsNeeded { get; set; }
        public GuildWarProgressInfo? YourGuildProgress { get; set; }
    }

    public class GuildWarProgressInfo
    {
        public double CapturePoints { get; set; }
        public int PlayersInZone { get; set; }
        public int Kills { get; set; }
        public int Deaths { get; set; }
    }

    public class AvailableWarInfo
    {
        public string NodeId { get; set; } = string.Empty;
        public string NodeName { get; set; } = string.Empty;
        public DateTime WarStartTime { get; set; }
        public int CurrentSignups { get; set; }
        public int MaxGuilds { get; set; }
        public bool CanSignup { get; set; }
    }

    public class CurrentSignupInfo
    {
        public string NodeId { get; set; } = string.Empty;
        public string NodeName { get; set; } = string.Empty;
        public DateTime SignupTime { get; set; }
        public DateTime WarStartTime { get; set; }
    }

    #endregion
}
