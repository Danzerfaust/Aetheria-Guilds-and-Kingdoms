using System;
using System.Runtime.CompilerServices;
using SRGuildsAndKingdoms.src.guilds;
using Vintagestory.API.Client;
using Vintagestory.API.Common;

namespace SRGuildsAndKingdoms.src.gui.tabs
{
	// Token: 0x02000086 RID: 134
	[NullableContext(1)]
	[Nullable(0)]
	public class GuildMembersTab : GuildTabContent
	{
		// Token: 0x060005E4 RID: 1508 RVA: 0x0002B0B4 File Offset: 0x000292B4
		public GuildMembersTab(ICoreClientAPI capi, SRGuildsAndKingdomsModSystem modSystem, [Nullable(2)] GuildSummary currentGuild, ActionConsumable onViewMembers, ActionConsumable onInvitePlayer, ActionConsumable onManageRoles, ActionConsumable onManagePendingInvites) : base(capi, modSystem, currentGuild)
		{
			this.onViewMembers = onViewMembers;
			this.onInvitePlayer = onInvitePlayer;
			this.onManageRoles = onManageRoles;
			this.onManagePendingInvites = onManagePendingInvites;
		}

		// Token: 0x060005E5 RID: 1509 RVA: 0x0002B0E0 File Offset: 0x000292E0
		public override double AddContent(GuiComposer composer, double startTop)
		{
			if (this.currentGuild == null)
			{
				return startTop;
			}
			double elementHeight = 25.0;
			double spacing = 10.0;
			GuiComposerHelpers.AddStaticText(composer, "Guild Members:", CairoFont.WhiteMediumText(), ElementBounds.Fixed(0.0, startTop, 400.0, elementHeight), null);
			double top = startTop + (elementHeight + spacing);
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(15, 1);
			defaultInterpolatedStringHandler.AppendLiteral("Total Members: ");
			defaultInterpolatedStringHandler.AppendFormatted<int>(this.currentGuild.MemberCount);
			GuiComposerHelpers.AddStaticText(composer, defaultInterpolatedStringHandler.ToStringAndClear(), CairoFont.WhiteSmallText(), ElementBounds.Fixed(0.0, top, 200.0, elementHeight), null);
			top += elementHeight + spacing;
			if (base.HasInvitePermissions() && this.currentGuild.PendingInvites != null)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(17, 1);
				defaultInterpolatedStringHandler2.AppendLiteral("Pending Invites: ");
				defaultInterpolatedStringHandler2.AppendFormatted<int>(this.currentGuild.PendingInvites.Count);
				GuiComposerHelpers.AddStaticText(composer, defaultInterpolatedStringHandler2.ToStringAndClear(), CairoFont.WhiteSmallText(), ElementBounds.Fixed(0.0, top, 200.0, elementHeight), null);
				top += elementHeight + spacing;
			}
			GuiComposerHelpers.AddSmallButton(composer, "View Members", this.onViewMembers, ElementBounds.Fixed(0.0, top, 120.0, elementHeight), 2, null);
			if (base.HasInvitePermissions())
			{
				GuiComposerHelpers.AddSmallButton(composer, "Invite Player", this.onInvitePlayer, ElementBounds.Fixed(130.0, top, 120.0, elementHeight), 2, null);
				GuiComposerHelpers.AddSmallButton(composer, "Manage Invites", this.onManagePendingInvites, ElementBounds.Fixed(260.0, top, 120.0, elementHeight), 2, null);
			}
			top += elementHeight + spacing;
			if (base.HasManagePermissions())
			{
				GuiComposerHelpers.AddSmallButton(composer, "Manage Roles", this.onManageRoles, ElementBounds.Fixed(0.0, top, 120.0, elementHeight), 2, null);
			}
			return top + elementHeight;
		}

		// Token: 0x0400025C RID: 604
		private readonly ActionConsumable onViewMembers;

		// Token: 0x0400025D RID: 605
		private readonly ActionConsumable onInvitePlayer;

		// Token: 0x0400025E RID: 606
		private readonly ActionConsumable onManageRoles;

		// Token: 0x0400025F RID: 607
		private readonly ActionConsumable onManagePendingInvites;
	}
}
