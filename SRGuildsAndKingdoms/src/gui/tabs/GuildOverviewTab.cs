using System;
using System.Runtime.CompilerServices;
using SRGuildsAndKingdoms.src.guilds;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;

namespace SRGuildsAndKingdoms.src.gui.tabs
{
	// Token: 0x0200008E RID: 142
	[NullableContext(1)]
	[Nullable(0)]
	public class GuildOverviewTab : GuildTabContent
	{
		// Token: 0x0600063C RID: 1596 RVA: 0x0002C64C File Offset: 0x0002A84C
		public GuildOverviewTab(ICoreClientAPI capi, SRGuildsAndKingdomsModSystem modSystem, [Nullable(2)] GuildSummary currentGuild, ActionConsumable onLeaveGuild) : base(capi, modSystem, currentGuild)
		{
			this.onLeaveGuild = onLeaveGuild;
		}

		// Token: 0x0600063D RID: 1597 RVA: 0x0002C660 File Offset: 0x0002A860
		public override double AddContent(GuiComposer composer, double startTop)
		{
			if (this.currentGuild == null)
			{
				return startTop;
			}
			double spacing = 10.0;
			double elementHeight = 25.0;
			double width = 500.0;
			GuiComposerHelpers.AddStaticText(composer, "Guild: " + this.currentGuild.Name, CairoFont.WhiteMediumText(), ElementBounds.Fixed(0.0, startTop, width, elementHeight + 5.0), null);
			double top = startTop + (elementHeight + spacing);
			if (!string.IsNullOrWhiteSpace(this.currentGuild.Description))
			{
				GuiComposerHelpers.AddStaticText(composer, "Description: " + this.currentGuild.Description, CairoFont.WhiteSmallText(), ElementBounds.Fixed(0.0, top, width, elementHeight * 2.0), null);
				top += elementHeight * 2.0 + spacing;
			}
			GuiComposerHelpers.AddStaticText(composer, "Guild Colors:", CairoFont.WhiteSmallText(), ElementBounds.Fixed(0.0, top, 150.0, elementHeight), null);
			top += elementHeight + 5.0;
			int primaryColorInt = this.currentGuild.DisplayColor;
			GuiComposerHelpers.AddStaticText(composer, "Primary: " + base.ColorToHex(primaryColorInt), CairoFont.WhiteSmallText(), ElementBounds.Fixed(0.0, top, 200.0, elementHeight), null);
			GuiElementInsetHelper.AddInset(composer, ElementBounds.Fixed(210.0, top + 2.0, 20.0, 20.0), 2, 0.85f);
			top += elementHeight + spacing;
			int secondaryColorInt = this.currentGuild.SecondaryColor;
			GuiComposerHelpers.AddStaticText(composer, "Secondary: " + base.ColorToHex(secondaryColorInt), CairoFont.WhiteSmallText(), ElementBounds.Fixed(0.0, top, 200.0, elementHeight), null);
			GuiElementInsetHelper.AddInset(composer, ElementBounds.Fixed(210.0, top + 2.0, 20.0, 20.0), 2, 0.85f);
			top += elementHeight + spacing * 2.0;
			GuiComposerHelpers.AddStaticText(composer, "Guild Statistics:", CairoFont.WhiteSmallText(), ElementBounds.Fixed(0.0, top, width, elementHeight), null);
			top += elementHeight + 5.0;
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(9, 1);
			defaultInterpolatedStringHandler.AppendLiteral("Members: ");
			defaultInterpolatedStringHandler.AppendFormatted<int>(this.currentGuild.MemberCount);
			GuiComposerHelpers.AddStaticText(composer, defaultInterpolatedStringHandler.ToStringAndClear(), CairoFont.WhiteSmallText(), ElementBounds.Fixed(0.0, top, 200.0, elementHeight), null);
			top += elementHeight + 5.0;
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(13, 1);
			defaultInterpolatedStringHandler2.AppendLiteral("Land Claims: ");
			defaultInterpolatedStringHandler2.AppendFormatted<int>(this.currentGuild.Claims.Count);
			GuiComposerHelpers.AddStaticText(composer, defaultInterpolatedStringHandler2.ToStringAndClear(), CairoFont.WhiteSmallText(), ElementBounds.Fixed(0.0, top, 200.0, elementHeight), null);
			top += elementHeight + 5.0;
			string rankText;
			if (this.currentGuild.RankClass == "D")
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler3 = new DefaultInterpolatedStringHandler(14, 1);
				defaultInterpolatedStringHandler3.AppendLiteral("Guild Points: ");
				defaultInterpolatedStringHandler3.AppendFormatted<int>(this.currentGuild.Points);
				rankText = defaultInterpolatedStringHandler3.ToStringAndClear();
			}
			else
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler4 = new DefaultInterpolatedStringHandler(28, 2);
				defaultInterpolatedStringHandler4.AppendLiteral("Guild Rank: Class ");
				defaultInterpolatedStringHandler4.AppendFormatted(this.currentGuild.RankClass);
				defaultInterpolatedStringHandler4.AppendLiteral(" (");
				defaultInterpolatedStringHandler4.AppendFormatted<int>(this.currentGuild.Points);
				defaultInterpolatedStringHandler4.AppendLiteral(" Points)");
				rankText = defaultInterpolatedStringHandler4.ToStringAndClear();
			}
			GuiComposerHelpers.AddStaticText(composer, rankText, CairoFont.WhiteSmallText(), ElementBounds.Fixed(0.0, top, 300.0, elementHeight), null);
			top += elementHeight + 5.0;
			GuiComposerHelpers.AddStaticText(composer, "Your Role: " + this.currentGuild.PlayerRole, CairoFont.WhiteSmallText(), ElementBounds.Fixed(0.0, top, 200.0, elementHeight), null);
			top += elementHeight + 5.0;
			GuiComposerHelpers.AddStaticText(composer, "Your Rank: " + this.currentGuild.MemberRank, CairoFont.WhiteSmallText(), ElementBounds.Fixed(0.0, top, width, elementHeight), null);
			top += elementHeight + spacing * 2.0;
			GuiComposerHelpers.AddSmallButton(composer, Lang.Get("srguildsandkingdoms:leave-guild", Array.Empty<object>()), this.onLeaveGuild, ElementBounds.Fixed(0.0, top + 20.0, 120.0, elementHeight), 2, null);
			return top + 20.0 + elementHeight;
		}

		// Token: 0x04000288 RID: 648
		private readonly ActionConsumable onLeaveGuild;
	}
}
