using SOAGuildsAndKingdoms.src.guilds;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;

namespace SOAGuildsAndKingdoms.src.gui.tabs
{
    public class GuildOverviewTab : GuildTabContent
    {
        private readonly ActionConsumable onLeaveGuild;

        public GuildOverviewTab(ICoreClientAPI capi, SOAGuildsAndKingdomsModSystem modSystem,
            GuildSummary? currentGuild, ActionConsumable onLeaveGuild)
            : base(capi, modSystem, currentGuild)
        {
            this.onLeaveGuild = onLeaveGuild;
        }

        public override double AddContent(GuiComposer composer, double startTop)
        {
            if (currentGuild == null) return startTop;

            var top = startTop;
            var spacing = 10.0;
            var elementHeight = 25.0;
            var width = 500.0;

            // Guild name display
            composer.AddStaticText($"Guild: {currentGuild.Name}", CairoFont.WhiteMediumText(),
                ElementBounds.Fixed(0, top, width, elementHeight + 5));
            top += elementHeight + spacing;

            // Guild description
            if (!string.IsNullOrWhiteSpace(currentGuild.Description))
            {
                composer.AddStaticText($"Description: {currentGuild.Description}", CairoFont.WhiteSmallText(),
                    ElementBounds.Fixed(0, top, width, elementHeight * 2));
                top += elementHeight * 2 + spacing;
            }

            // Guild colors display
            composer.AddStaticText("Guild Colors:", CairoFont.WhiteSmallText(),
                ElementBounds.Fixed(0, top, 150, elementHeight));
            top += elementHeight + 5;

            // Primary color display with visual indicator
            var primaryColorInt = currentGuild.DisplayColor;
            composer.AddStaticText($"Primary: {ColorToHex(primaryColorInt)}", CairoFont.WhiteSmallText(),
                ElementBounds.Fixed(0, top, 200, elementHeight));

            // Add color swatch for primary color using inset
            composer.AddInset(ElementBounds.Fixed(210, top + 2, 20, 20), 2);
            top += elementHeight + spacing;

            // Secondary color display with visual indicator
            var secondaryColorInt = currentGuild.SecondaryColor;
            composer.AddStaticText($"Secondary: {ColorToHex(secondaryColorInt)}", CairoFont.WhiteSmallText(),
                ElementBounds.Fixed(0, top, 200, elementHeight));

            // Add color swatch for secondary color using inset
            composer.AddInset(ElementBounds.Fixed(210, top + 2, 20, 20), 2);
            top += elementHeight + spacing * 2;

            // Guild statistics
            composer.AddStaticText("Guild Statistics:", CairoFont.WhiteSmallText(),
                ElementBounds.Fixed(0, top, width, elementHeight));
            top += elementHeight + 5;

            composer.AddStaticText($"Members: {currentGuild.MemberCount}", CairoFont.WhiteSmallText(),
                ElementBounds.Fixed(0, top, 200, elementHeight));
            top += elementHeight + 5;

            composer.AddStaticText($"Land Claims: {currentGuild.Claims.Count}", CairoFont.WhiteSmallText(),
                ElementBounds.Fixed(0, top, 200, elementHeight));
            top += elementHeight + 5;

            string? rankText;
            if (currentGuild.RankClass == "D")
            {
                // Hide rank class for D rank guilds and just show points
                rankText = $"Guild Points: {currentGuild.Points}";
            }
            else
            {
                rankText = $"Guild Rank: Class {currentGuild.RankClass} ({currentGuild.Points} Points)";
            }

            composer.AddStaticText(rankText, CairoFont.WhiteSmallText(),
                ElementBounds.Fixed(0, top, 300, elementHeight));
            top += elementHeight + 5;

            composer.AddStaticText($"Your Role: {currentGuild.PlayerRole}", CairoFont.WhiteSmallText(),
                ElementBounds.Fixed(0, top, 200, elementHeight));
            top += elementHeight + 5;

            composer.AddStaticText($"Your Rank: {currentGuild.MemberRank}", CairoFont.WhiteSmallText(),
                ElementBounds.Fixed(0, top, width, elementHeight));
            top += elementHeight + spacing * 2;

            // Leave Guild button at the bottom
            composer.AddSmallButton(Lang.Get("soaguildsandkingdoms:leave-guild"), onLeaveGuild,
                ElementBounds.Fixed(0, top + 20, 120, elementHeight), EnumButtonStyle.Normal);

            return top + 20 + elementHeight;
        }
    }
}