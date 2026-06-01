using SRGuildsAndKingdoms.src.guilds;
using System;
using System.Collections.Generic;
using System.Linq;
using Vintagestory.API.Client;
using Vintagestory.API.Common;

namespace SRGuildsAndKingdoms.src.gui.tabs
{
    public class GuildLandsTab : GuildTabContent
    {
        private bool isClaimingMode;
        private bool isUnclaimingMode;
        private List<(int chunkX, int chunkZ)> pendingClaims;
        private List<(int chunkX, int chunkZ)> pendingUnclaims;
        private readonly ActionConsumable onClaimingToggle;
        private readonly ActionConsumable onSavePendingClaims;
        private readonly ActionConsumable onShowHologram;
        private readonly ActionConsumable onCreateOutpost;
        private readonly ActionConsumable onUnclaimingToggle;
        private readonly ActionConsumable onSavePendingUnclaims;
        private readonly ActionConsumable onClaimCurrentChunk;
        private readonly ActionConsumable onUnclaimCurrentChunk;

        public GuildLandsTab(ICoreClientAPI capi, SRGuildsAndKingdomsModSystem modSystem,
            GuildSummary? currentGuild, ActionConsumable onClaimingToggle,
            ActionConsumable onSavePendingClaims, ActionConsumable onShowHologram,
            ActionConsumable onCreateOutpost, ActionConsumable onUnclaimingToggle,
            ActionConsumable onSavePendingUnclaims, ActionConsumable onClaimCurrentChunk,
            ActionConsumable onUnclaimCurrentChunk)
            : base(capi, modSystem, currentGuild)
        {
            this.onClaimingToggle = onClaimingToggle;
            this.onSavePendingClaims = onSavePendingClaims;
            this.onShowHologram = onShowHologram;
            this.onCreateOutpost = onCreateOutpost;
            this.onUnclaimingToggle = onUnclaimingToggle;
            this.onSavePendingUnclaims = onSavePendingUnclaims;
            this.onClaimCurrentChunk = onClaimCurrentChunk;
            this.onUnclaimCurrentChunk = onUnclaimCurrentChunk;
            this.pendingClaims = new List<(int, int)>();
            this.pendingUnclaims = new List<(int, int)>();
        }

        public void SetClaimingMode(bool isClaimingMode)
        {
            this.isClaimingMode = isClaimingMode;
        }

        public void SetUnclaimingMode(bool isUnclaimingMode)
        {
            this.isUnclaimingMode = isUnclaimingMode;
        }

        public void SetPendingClaims(List<(int chunkX, int chunkZ)> pendingClaims)
        {
            this.pendingClaims = new List<(int, int)>(pendingClaims);
        }

        public void SetPendingUnclaims(List<(int chunkX, int chunkZ)> pendingUnclaims)
        {
            this.pendingUnclaims = new List<(int, int)>(pendingUnclaims);
        }

        /// <summary>
        /// Calculates the effective claim count where all guild home claims count as a single claim.
        /// Outpost claims are counted individually.
        /// </summary>
        private int GetEffectiveClaimCount()
        {
            if (currentGuild == null || currentGuild.Claims.Count == 0)
                return 0;

            int homeClaims = currentGuild.Claims.Count(c => c.IsGuildHome);
            int outpostClaims = currentGuild.Claims.Count(c => c.IsOutpost);
            int regularClaims = currentGuild.Claims.Count - homeClaims - outpostClaims;

            // Home claims count as 1 (if any exist), outpost and regular claims count individually
            int effectiveHomeClaims = homeClaims > 0 ? 1 : 0;

            return effectiveHomeClaims + outpostClaims + regularClaims;
        }

        public override double AddContent(GuiComposer composer, double startTop)
        {
            if (currentGuild == null) return startTop;

            var top = startTop;
            var elementHeight = 25.0;
            var spacing = 10.0;

            composer.AddStaticText("Guild Lands:", CairoFont.WhiteMediumText(),
                ElementBounds.Fixed(0, top, 400, elementHeight));
            top += elementHeight + spacing;

            var effectiveClaimCount = GetEffectiveClaimCount();
            composer.AddStaticText($"Claimed Chunks: {effectiveClaimCount}", CairoFont.WhiteSmallText(),
                ElementBounds.Fixed(0, top, 200, elementHeight));
            top += elementHeight + 5;

            // Get max claims from guild summary if available, otherwise use 0 as fallback
            var maxClaims = currentGuild.MaxClaims;
            composer.AddStaticText($"Max Claims: {maxClaims}", CairoFont.WhiteSmallText(),
                ElementBounds.Fixed(0, top, 200, elementHeight));
            top += elementHeight + 5;

            // Count and display outposts
            var outpostCount = currentGuild.Claims.Count(c => c.IsOutpost);
            var maxOutposts = currentGuild.MaxOutposts;
            composer.AddStaticText($"Outposts: {outpostCount}", CairoFont.WhiteSmallText(),
                ElementBounds.Fixed(0, top, 200, elementHeight));
            top += elementHeight + 5;

            composer.AddStaticText($"Max Outposts: {maxOutposts}", CairoFont.WhiteSmallText(),
                ElementBounds.Fixed(0, top, 200, elementHeight));
            top += elementHeight + spacing;

            // Show pending claims count if any
            if (pendingClaims.Count > 0)
            {
                composer.AddStaticText($"Pending Claims: {pendingClaims.Count}",
                    CairoFont.WhiteDetailText(), ElementBounds.Fixed(0, top, 400, elementHeight));
                top += elementHeight + spacing;
            }

            // Show pending unclaims count if any
            if (pendingUnclaims.Count > 0)
            {
                composer.AddStaticText($"Pending Unclaims: {pendingUnclaims.Count}",
                    CairoFont.WhiteDetailText().WithColor(new double[] { 1.0, 0.5, 0.5, 1.0 }),
                    ElementBounds.Fixed(0, top, 400, elementHeight));
                top += elementHeight + spacing;
            }


            // Claiming actions
            if (HasManagePermissions())
            {
                // Row 1: Claiming controls
                var claimButtonText = isClaimingMode ? "Stop Claiming" :
                    (currentGuild.Claims.Count == 0 ? "Set Guild Home" : "Start Claiming");
                composer.AddSmallButton(claimButtonText, onClaimingToggle,
                    ElementBounds.Fixed(0, top, 120, elementHeight), EnumButtonStyle.Normal);

                // Add "Claim Current" button to quickly claim the chunk the player is standing in
                composer.AddSmallButton("Claim Current", onClaimCurrentChunk,
                    ElementBounds.Fixed(130, top, 120, elementHeight), EnumButtonStyle.Normal);

                // Add Save Pending Claims button on the same row if there are pending claims
                if (pendingClaims.Count > 0)
                {
                    composer.AddSmallButton("Save Claims", onSavePendingClaims,
                        ElementBounds.Fixed(260, top, 120, elementHeight), EnumButtonStyle.MainMenu);
                }

                top += elementHeight + spacing;

                // Row 2: Unclaiming controls (only if there are claims to unclaim)
                if (currentGuild.Claims.Count > 0)
                {
                    var unclaimButtonText = isUnclaimingMode ? "Stop Unclaiming" : "Start Unclaiming";
                    composer.AddSmallButton(unclaimButtonText, onUnclaimingToggle,
                        ElementBounds.Fixed(0, top, 120, elementHeight), EnumButtonStyle.Normal);

                    // Add "Unclaim Current" button to quickly unclaim the chunk the player is standing in
                    composer.AddSmallButton("Unclaim Current", onUnclaimCurrentChunk,
                        ElementBounds.Fixed(130, top, 120, elementHeight), EnumButtonStyle.Normal);

                    // Add Save Pending Unclaims button on the same row if there are pending unclaims
                    if (pendingUnclaims.Count > 0)
                    {
                        composer.AddSmallButton("Save Unclaims", onSavePendingUnclaims,
                            ElementBounds.Fixed(260, top, 120, elementHeight), EnumButtonStyle.MainMenu);
                    }

                    top += elementHeight + spacing;
                }

                // Row 3: Utility buttons
                if (currentGuild.Claims.Count > 0)
                {
                    composer.AddSmallButton("Show Outline", onShowHologram,
                        ElementBounds.Fixed(0, top, 120, elementHeight), EnumButtonStyle.Normal);
                }

                // Add Create Outpost button if guild has a home and hasn't reached max outposts
                bool hasGuildHome = currentGuild.Claims.Any(c => c.IsGuildHome);
                bool canCreateOutpost = hasGuildHome && outpostCount < maxOutposts;

                if (canCreateOutpost)
                {
                    composer.AddSmallButton("Create Outpost", onCreateOutpost,
                        ElementBounds.Fixed(130, top, 120, elementHeight), EnumButtonStyle.Normal);
                }
                else if (!hasGuildHome)
                {
                    // Show disabled button with tooltip explaining why it's disabled
                    composer.AddStaticText("Create Outpost (requires guild home)",
                        CairoFont.WhiteSmallText().WithColor(new double[] { 0.5, 0.5, 0.5, 1.0 }),
                        ElementBounds.Fixed(130, top, 250, elementHeight));
                }
                else if (outpostCount >= maxOutposts)
                {
                    // Show disabled button with tooltip explaining max reached
                    composer.AddStaticText("Create Outpost (max reached)",
                        CairoFont.WhiteSmallText().WithColor(new double[] { 0.5, 0.5, 0.5, 1.0 }),
                        ElementBounds.Fixed(130, top, 200, elementHeight));
                }
            }

            return top + elementHeight;
        }
    }
}