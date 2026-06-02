using System;
using System.Collections.Generic;
using Vintagestory.API.Client;
using Vintagestory.API.Config;

namespace SOAGuildsAndKingdoms.src.gui
{
    /// <summary>
    /// Dialog for signing up a guild for a node war
    /// Shows detailed war information and confirmation
    /// </summary>
    public class DialogNodeWarSignup : GuiDialog
    {
        private readonly Action<string> onConfirmSignup;
        private readonly Action onCancel;
        private readonly NodeWarSignupData warData;

        public override string ToggleKeyCombinationCode => null;

        public DialogNodeWarSignup(ICoreClientAPI capi, NodeWarSignupData warData, 
            Action<string> onConfirmSignup, Action onCancel) : base(capi)
        {
            this.warData = warData;
            this.onConfirmSignup = onConfirmSignup;
            this.onCancel = onCancel;
        }

        public override void OnGuiOpened()
        {
            base.OnGuiOpened();
            SetupDialog();
        }

        private void SetupDialog()
        {
            // Auto-sized dialog. Note: dialog will not be shown until after it's first composed,
            // so all size and position changes made here will be applied before it is shown.
            ElementBounds dialogBounds = ElementStdBounds.AutosizedMainDialog.WithAlignment(EnumDialogArea.CenterMiddle);

            ElementBounds insetBounds = ElementBounds.Fixed(0, 0, 450, 350);
            ElementBounds contentBounds = ElementBounds.Fixed(0, 30, 450, 320);

            ElementBounds bgBounds = ElementBounds.Fill.WithFixedPadding(GuiStyle.ElementToDialogPadding);
            bgBounds.BothSizing = ElementSizing.FitToChildren;
            bgBounds.WithChildren(insetBounds, contentBounds);

            SingleComposer = capi.Gui.CreateCompo("nodewar-signup", dialogBounds)
                .AddShadedDialogBG(bgBounds)
                .AddDialogTitleBar("Sign Up for Node War", OnTitleBarClose)
                .BeginChildElements(bgBounds)
                ;

            var top = 30.0;
            var elementHeight = 25.0;
            var spacing = 10.0;

            // Node Name
            SingleComposer.AddStaticText($"Node: {warData.NodeName}", 
                CairoFont.WhiteMediumText(), 
                ElementBounds.Fixed(10, top, 430, elementHeight));
            top += elementHeight + 5;

            // Description
            if (!string.IsNullOrEmpty(warData.NodeDescription))
            {
                SingleComposer.AddStaticText(warData.NodeDescription, 
                    CairoFont.WhiteSmallText(), 
                    ElementBounds.Fixed(10, top, 430, elementHeight * 2));
                top += (elementHeight * 2) + spacing;
            }

            // War Details Section
            SingleComposer.AddStaticText("War Details:", 
                CairoFont.WhiteSmallText().WithWeight(Cairo.FontWeight.Bold), 
                ElementBounds.Fixed(10, top, 430, elementHeight));
            top += elementHeight + 5;

            SingleComposer.AddStaticText($"Starts: {GetTimeString(warData.StartTime)}", 
                CairoFont.WhiteSmallText(), 
                ElementBounds.Fixed(20, top, 410, elementHeight));
            top += elementHeight;

            SingleComposer.AddStaticText($"Duration: {warData.DurationMinutes} minutes", 
                CairoFont.WhiteSmallText(), 
                ElementBounds.Fixed(20, top, 410, elementHeight));
            top += elementHeight;

            SingleComposer.AddStaticText($"Points to Win: {warData.PointsNeeded:F0}", 
                CairoFont.WhiteSmallText(), 
                ElementBounds.Fixed(20, top, 410, elementHeight));
            top += elementHeight;

            SingleComposer.AddStaticText($"Min Players: {warData.MinPlayersRequired}", 
                CairoFont.WhiteSmallText(), 
                ElementBounds.Fixed(20, top, 410, elementHeight));
            top += elementHeight + spacing;

            // Guild Signups
            SingleComposer.AddStaticText("Guild Signups:", 
                CairoFont.WhiteSmallText().WithWeight(Cairo.FontWeight.Bold), 
                ElementBounds.Fixed(10, top, 430, elementHeight));
            top += elementHeight + 5;

            var maxGuilds = warData.MaxGuilds == 0 ? "Unlimited" : warData.MaxGuilds.ToString();
            SingleComposer.AddStaticText($"{warData.CurrentSignups} guild(s) signed up (Max: {maxGuilds})", 
                CairoFont.WhiteSmallText(), 
                ElementBounds.Fixed(20, top, 410, elementHeight));
            top += elementHeight;

            if (warData.SignedUpGuilds.Count > 0)
            {
                foreach (var guild in warData.SignedUpGuilds)
                {
                    SingleComposer.AddStaticText($"• {guild}", 
                        CairoFont.WhiteSmallText().WithColor(new double[] { 0.8, 0.8, 1, 1 }), 
                        ElementBounds.Fixed(30, top, 400, elementHeight - 5));
                    top += elementHeight - 5;
                }
            }
            top += spacing;

            // Requirements Section
            SingleComposer.AddStaticText("Your Guild Requirements:", 
                CairoFont.WhiteSmallText().WithWeight(Cairo.FontWeight.Bold), 
                ElementBounds.Fixed(10, top, 430, elementHeight));
            top += elementHeight + 5;

            // Check requirements
            var requirements = CheckRequirements();
            foreach (var (requirement, met) in requirements)
            {
                var color = met ? new double[] { 0.7, 1, 0.7, 1 } : new double[] { 1, 0.7, 0.7, 1 };
                var icon = met ? "✓" : "✗";
                
                SingleComposer.AddStaticText($"{icon} {requirement}", 
                    CairoFont.WhiteSmallText().WithColor(color), 
                    ElementBounds.Fixed(20, top, 410, elementHeight));
                top += elementHeight;
            }

            top += spacing;

            // Warning if requirements not met
            if (!AllRequirementsMet())
            {
                SingleComposer.AddStaticText("Your guild does not meet all requirements!", 
                    CairoFont.WhiteSmallText().WithColor(new double[] { 1, 0.5, 0.5, 1 }), 
                    ElementBounds.Fixed(10, top, 430, elementHeight));
                top += elementHeight + spacing;
            }

            // Buttons
            var buttonY = top;
            SingleComposer.AddSmallButton("Sign Up", OnConfirmClick, 
                ElementBounds.Fixed(10, buttonY, 100, elementHeight), 
                EnumButtonStyle.Normal, 
                key: "btnConfirm");

            if (!AllRequirementsMet())
            {
                // Disable confirm button if requirements not met
                SingleComposer.GetButton("btnConfirm").Enabled = false;
            }

            SingleComposer.AddSmallButton("Cancel", OnCancelClick, 
                ElementBounds.Fixed(120, buttonY, 100, elementHeight), 
                EnumButtonStyle.Normal);

            SingleComposer.EndChildElements().Compose();
        }

        private List<(string requirement, bool met)> CheckRequirements()
        {
            var requirements = new List<(string, bool)>();

            // Total members
            requirements.Add((
                $"At least {warData.MinGuildMembers} total members ({warData.GuildTotalMembers} current)",
                warData.GuildTotalMembers >= warData.MinGuildMembers
            ));

            // Online members
            requirements.Add((
                $"At least {warData.MinOnlineMembers} members online ({warData.GuildOnlineMembers} current)",
                warData.GuildOnlineMembers >= warData.MinOnlineMembers
            ));

            // Not in another war
            requirements.Add((
                "Not signed up for another war",
                !warData.IsAlreadySignedUp
            ));

            // Guild leader
            requirements.Add((
                "You must be the guild leader",
                warData.IsPlayerLeader
            ));

            // Signup not closed
            requirements.Add((
                "Signup period still open",
                !warData.IsSignupClosed
            ));

            // Space available
            if (warData.MaxGuilds > 0)
            {
                requirements.Add((
                    $"Space available ({warData.CurrentSignups}/{warData.MaxGuilds})",
                    warData.CurrentSignups < warData.MaxGuilds
                ));
            }

            return requirements;
        }

        private bool AllRequirementsMet()
        {
            var requirements = CheckRequirements();
            foreach (var (_, met) in requirements)
            {
                if (!met) return false;
            }
            return true;
        }

        private bool OnConfirmClick()
        {
            onConfirmSignup?.Invoke(warData.NodeId);
            TryClose();
            return true;
        }

        private bool OnCancelClick()
        {
            onCancel?.Invoke();
            TryClose();
            return true;
        }

        private void OnTitleBarClose()
        {
            TryClose();
        }

        private string GetTimeString(DateTime time)
        {
            var diff = time - DateTime.UtcNow;

            if (diff.TotalDays > 1)
                return $"{time:yyyy-MM-dd HH:mm} ({diff.TotalDays:F0} days from now)";
            if (diff.TotalHours > 1)
                return $"{time:HH:mm} ({diff.TotalHours:F0} hours from now)";
            if (diff.TotalMinutes > 1)
                return $"{time:HH:mm} ({diff.TotalMinutes:F0} minutes from now)";

            return $"{time:HH:mm} (starting soon)";
        }
    }

    /// <summary>
    /// Data structure for node war signup dialog
    /// </summary>
    public class NodeWarSignupData
    {
        public string NodeId { get; set; } = string.Empty;
        public string NodeName { get; set; } = string.Empty;
        public string NodeDescription { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public int DurationMinutes { get; set; }
        public double PointsNeeded { get; set; }
        public int MinPlayersRequired { get; set; }
        public int CurrentSignups { get; set; }
        public int MaxGuilds { get; set; }
        public List<string> SignedUpGuilds { get; set; } = new();
        
        // Guild requirements
        public int MinGuildMembers { get; set; }
        public int MinOnlineMembers { get; set; }
        public int GuildTotalMembers { get; set; }
        public int GuildOnlineMembers { get; set; }
        public bool IsAlreadySignedUp { get; set; }
        public bool IsPlayerLeader { get; set; }
        public bool IsSignupClosed { get; set; }
    }
}
