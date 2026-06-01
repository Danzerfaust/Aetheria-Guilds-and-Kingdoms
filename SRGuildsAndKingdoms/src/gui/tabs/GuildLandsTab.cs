using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using SRGuildsAndKingdoms.src.guilds;
using Vintagestory.API.Client;
using Vintagestory.API.Common;

namespace SRGuildsAndKingdoms.src.gui.tabs
{
	// Token: 0x02000085 RID: 133
	[NullableContext(1)]
	[Nullable(0)]
	public class GuildLandsTab : GuildTabContent
	{
		// Token: 0x060005DD RID: 1501 RVA: 0x0002A9A0 File Offset: 0x00028BA0
		public GuildLandsTab(ICoreClientAPI capi, SRGuildsAndKingdomsModSystem modSystem, [Nullable(2)] GuildSummary currentGuild, ActionConsumable onClaimingToggle, ActionConsumable onSavePendingClaims, ActionConsumable onShowHologram, ActionConsumable onCreateOutpost, ActionConsumable onUnclaimingToggle, ActionConsumable onSavePendingUnclaims, ActionConsumable onClaimCurrentChunk, ActionConsumable onUnclaimCurrentChunk) : base(capi, modSystem, currentGuild)
		{
			this.onClaimingToggle = onClaimingToggle;
			this.onSavePendingClaims = onSavePendingClaims;
			this.onShowHologram = onShowHologram;
			this.onCreateOutpost = onCreateOutpost;
			this.onUnclaimingToggle = onUnclaimingToggle;
			this.onSavePendingUnclaims = onSavePendingUnclaims;
			this.onClaimCurrentChunk = onClaimCurrentChunk;
			this.onUnclaimCurrentChunk = onUnclaimCurrentChunk;
			this.pendingClaims = new List<ValueTuple<int, int>>();
			this.pendingUnclaims = new List<ValueTuple<int, int>>();
		}

		// Token: 0x060005DE RID: 1502 RVA: 0x0002AA0C File Offset: 0x00028C0C
		public void SetClaimingMode(bool isClaimingMode)
		{
			this.isClaimingMode = isClaimingMode;
		}

		// Token: 0x060005DF RID: 1503 RVA: 0x0002AA15 File Offset: 0x00028C15
		public void SetUnclaimingMode(bool isUnclaimingMode)
		{
			this.isUnclaimingMode = isUnclaimingMode;
		}

		// Token: 0x060005E0 RID: 1504 RVA: 0x0002AA1E File Offset: 0x00028C1E
		public void SetPendingClaims([TupleElementNames(new string[]
		{
			"chunkX",
			"chunkZ"
		})] [Nullable(new byte[]
		{
			1,
			0
		})] List<ValueTuple<int, int>> pendingClaims)
		{
			this.pendingClaims = new List<ValueTuple<int, int>>(pendingClaims);
		}

		// Token: 0x060005E1 RID: 1505 RVA: 0x0002AA2C File Offset: 0x00028C2C
		public void SetPendingUnclaims([TupleElementNames(new string[]
		{
			"chunkX",
			"chunkZ"
		})] [Nullable(new byte[]
		{
			1,
			0
		})] List<ValueTuple<int, int>> pendingUnclaims)
		{
			this.pendingUnclaims = new List<ValueTuple<int, int>>(pendingUnclaims);
		}

		// Token: 0x060005E2 RID: 1506 RVA: 0x0002AA3C File Offset: 0x00028C3C
		private int GetEffectiveClaimCount()
		{
			if (this.currentGuild == null || this.currentGuild.Claims.Count == 0)
			{
				return 0;
			}
			int homeClaims = this.currentGuild.Claims.Count((LandClaimDto c) => c.IsGuildHome);
			int outpostClaims = this.currentGuild.Claims.Count((LandClaimDto c) => c.IsOutpost);
			int regularClaims = this.currentGuild.Claims.Count - homeClaims - outpostClaims;
			return ((homeClaims > 0) ? 1 : 0) + outpostClaims + regularClaims;
		}

		// Token: 0x060005E3 RID: 1507 RVA: 0x0002AAE4 File Offset: 0x00028CE4
		public override double AddContent(GuiComposer composer, double startTop)
		{
			if (this.currentGuild == null)
			{
				return startTop;
			}
			double elementHeight = 25.0;
			double spacing = 10.0;
			GuiComposerHelpers.AddStaticText(composer, "Guild Lands:", CairoFont.WhiteMediumText(), ElementBounds.Fixed(0.0, startTop, 400.0, elementHeight), null);
			double top = startTop + (elementHeight + spacing);
			int effectiveClaimCount = this.GetEffectiveClaimCount();
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(16, 1);
			defaultInterpolatedStringHandler.AppendLiteral("Claimed Chunks: ");
			defaultInterpolatedStringHandler.AppendFormatted<int>(effectiveClaimCount);
			GuiComposerHelpers.AddStaticText(composer, defaultInterpolatedStringHandler.ToStringAndClear(), CairoFont.WhiteSmallText(), ElementBounds.Fixed(0.0, top, 200.0, elementHeight), null);
			top += elementHeight + 5.0;
			int maxClaims = this.currentGuild.MaxClaims;
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(12, 1);
			defaultInterpolatedStringHandler2.AppendLiteral("Max Claims: ");
			defaultInterpolatedStringHandler2.AppendFormatted<int>(maxClaims);
			GuiComposerHelpers.AddStaticText(composer, defaultInterpolatedStringHandler2.ToStringAndClear(), CairoFont.WhiteSmallText(), ElementBounds.Fixed(0.0, top, 200.0, elementHeight), null);
			top += elementHeight + 5.0;
			int outpostCount = this.currentGuild.Claims.Count((LandClaimDto c) => c.IsOutpost);
			int maxOutposts = this.currentGuild.MaxOutposts;
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler3 = new DefaultInterpolatedStringHandler(10, 1);
			defaultInterpolatedStringHandler3.AppendLiteral("Outposts: ");
			defaultInterpolatedStringHandler3.AppendFormatted<int>(outpostCount);
			GuiComposerHelpers.AddStaticText(composer, defaultInterpolatedStringHandler3.ToStringAndClear(), CairoFont.WhiteSmallText(), ElementBounds.Fixed(0.0, top, 200.0, elementHeight), null);
			top += elementHeight + 5.0;
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler4 = new DefaultInterpolatedStringHandler(14, 1);
			defaultInterpolatedStringHandler4.AppendLiteral("Max Outposts: ");
			defaultInterpolatedStringHandler4.AppendFormatted<int>(maxOutposts);
			GuiComposerHelpers.AddStaticText(composer, defaultInterpolatedStringHandler4.ToStringAndClear(), CairoFont.WhiteSmallText(), ElementBounds.Fixed(0.0, top, 200.0, elementHeight), null);
			top += elementHeight + spacing;
			if (this.pendingClaims.Count > 0)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler5 = new DefaultInterpolatedStringHandler(16, 1);
				defaultInterpolatedStringHandler5.AppendLiteral("Pending Claims: ");
				defaultInterpolatedStringHandler5.AppendFormatted<int>(this.pendingClaims.Count);
				GuiComposerHelpers.AddStaticText(composer, defaultInterpolatedStringHandler5.ToStringAndClear(), CairoFont.WhiteDetailText(), ElementBounds.Fixed(0.0, top, 400.0, elementHeight), null);
				top += elementHeight + spacing;
			}
			if (this.pendingUnclaims.Count > 0)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler6 = new DefaultInterpolatedStringHandler(18, 1);
				defaultInterpolatedStringHandler6.AppendLiteral("Pending Unclaims: ");
				defaultInterpolatedStringHandler6.AppendFormatted<int>(this.pendingUnclaims.Count);
				GuiComposerHelpers.AddStaticText(composer, defaultInterpolatedStringHandler6.ToStringAndClear(), CairoFont.WhiteDetailText().WithColor(new double[]
				{
					1.0,
					0.5,
					0.5,
					1.0
				}), ElementBounds.Fixed(0.0, top, 400.0, elementHeight), null);
				top += elementHeight + spacing;
			}
			if (base.HasManagePermissions())
			{
				string claimButtonText = this.isClaimingMode ? "Stop Claiming" : ((this.currentGuild.Claims.Count == 0) ? "Set Guild Home" : "Start Claiming");
				GuiComposerHelpers.AddSmallButton(composer, claimButtonText, this.onClaimingToggle, ElementBounds.Fixed(0.0, top, 120.0, elementHeight), 2, null);
				GuiComposerHelpers.AddSmallButton(composer, "Claim Current", this.onClaimCurrentChunk, ElementBounds.Fixed(130.0, top, 120.0, elementHeight), 2, null);
				if (this.pendingClaims.Count > 0)
				{
					GuiComposerHelpers.AddSmallButton(composer, "Save Claims", this.onSavePendingClaims, ElementBounds.Fixed(260.0, top, 120.0, elementHeight), 1, null);
				}
				top += elementHeight + spacing;
				if (this.currentGuild.Claims.Count > 0)
				{
					string unclaimButtonText = this.isUnclaimingMode ? "Stop Unclaiming" : "Start Unclaiming";
					GuiComposerHelpers.AddSmallButton(composer, unclaimButtonText, this.onUnclaimingToggle, ElementBounds.Fixed(0.0, top, 120.0, elementHeight), 2, null);
					GuiComposerHelpers.AddSmallButton(composer, "Unclaim Current", this.onUnclaimCurrentChunk, ElementBounds.Fixed(130.0, top, 120.0, elementHeight), 2, null);
					if (this.pendingUnclaims.Count > 0)
					{
						GuiComposerHelpers.AddSmallButton(composer, "Save Unclaims", this.onSavePendingUnclaims, ElementBounds.Fixed(260.0, top, 120.0, elementHeight), 1, null);
					}
					top += elementHeight + spacing;
				}
				if (this.currentGuild.Claims.Count > 0)
				{
					GuiComposerHelpers.AddSmallButton(composer, "Show Outline", this.onShowHologram, ElementBounds.Fixed(0.0, top, 120.0, elementHeight), 2, null);
				}
				bool hasGuildHome = this.currentGuild.Claims.Any((LandClaimDto c) => c.IsGuildHome);
				if (hasGuildHome && outpostCount < maxOutposts)
				{
					GuiComposerHelpers.AddSmallButton(composer, "Create Outpost", this.onCreateOutpost, ElementBounds.Fixed(130.0, top, 120.0, elementHeight), 2, null);
				}
				else if (!hasGuildHome)
				{
					GuiComposerHelpers.AddStaticText(composer, "Create Outpost (requires guild home)", CairoFont.WhiteSmallText().WithColor(new double[]
					{
						0.5,
						0.5,
						0.5,
						1.0
					}), ElementBounds.Fixed(130.0, top, 250.0, elementHeight), null);
				}
				else if (outpostCount >= maxOutposts)
				{
					GuiComposerHelpers.AddStaticText(composer, "Create Outpost (max reached)", CairoFont.WhiteSmallText().WithColor(new double[]
					{
						0.5,
						0.5,
						0.5,
						1.0
					}), ElementBounds.Fixed(130.0, top, 200.0, elementHeight), null);
				}
			}
			return top + elementHeight;
		}

		// Token: 0x04000250 RID: 592
		private bool isClaimingMode;

		// Token: 0x04000251 RID: 593
		private bool isUnclaimingMode;

		// Token: 0x04000252 RID: 594
		[TupleElementNames(new string[]
		{
			"chunkX",
			"chunkZ"
		})]
		[Nullable(new byte[]
		{
			1,
			0
		})]
		private List<ValueTuple<int, int>> pendingClaims;

		// Token: 0x04000253 RID: 595
		[TupleElementNames(new string[]
		{
			"chunkX",
			"chunkZ"
		})]
		[Nullable(new byte[]
		{
			1,
			0
		})]
		private List<ValueTuple<int, int>> pendingUnclaims;

		// Token: 0x04000254 RID: 596
		private readonly ActionConsumable onClaimingToggle;

		// Token: 0x04000255 RID: 597
		private readonly ActionConsumable onSavePendingClaims;

		// Token: 0x04000256 RID: 598
		private readonly ActionConsumable onShowHologram;

		// Token: 0x04000257 RID: 599
		private readonly ActionConsumable onCreateOutpost;

		// Token: 0x04000258 RID: 600
		private readonly ActionConsumable onUnclaimingToggle;

		// Token: 0x04000259 RID: 601
		private readonly ActionConsumable onSavePendingUnclaims;

		// Token: 0x0400025A RID: 602
		private readonly ActionConsumable onClaimCurrentChunk;

		// Token: 0x0400025B RID: 603
		private readonly ActionConsumable onUnclaimCurrentChunk;
	}
}
