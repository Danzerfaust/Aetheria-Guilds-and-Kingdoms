using System;
using System.Runtime.CompilerServices;
using SRGuildsAndKingdoms.src.guilds;
using Vintagestory.API.Client;

namespace SRGuildsAndKingdoms.src.gui.tabs
{
	// Token: 0x02000092 RID: 146
	[NullableContext(1)]
	[Nullable(0)]
	public abstract class GuildTabContent
	{
		// Token: 0x0600066D RID: 1645 RVA: 0x0002FBD0 File Offset: 0x0002DDD0
		public GuildTabContent(ICoreClientAPI capi, SRGuildsAndKingdomsModSystem modSystem, [Nullable(2)] GuildSummary currentGuild)
		{
			this.capi = capi;
			this.modSystem = modSystem;
			this.currentGuild = currentGuild;
		}

		// Token: 0x0600066E RID: 1646 RVA: 0x0002FBF0 File Offset: 0x0002DDF0
		protected void RequestGuildRefresh()
		{
			if (this.currentGuild != null && this.capi.World.Player != null)
			{
				GuildSummary updatedGuild = this.GetLatestGuildSummary(this.currentGuild.Name);
				if (updatedGuild != null)
				{
					this.Refresh(updatedGuild);
					this.OnGuildDataRefreshed(updatedGuild);
				}
			}
		}

		// Token: 0x0600066F RID: 1647 RVA: 0x0002FC3A File Offset: 0x0002DE3A
		[return: Nullable(2)]
		private GuildSummary GetLatestGuildSummary(string guildName)
		{
			return this.modSystem.GetCachedGuildSummary(guildName) ?? this.currentGuild;
		}

		// Token: 0x06000670 RID: 1648 RVA: 0x0002FC52 File Offset: 0x0002DE52
		protected virtual void OnGuildDataRefreshed(GuildSummary updatedGuild)
		{
		}

		// Token: 0x06000671 RID: 1649
		public abstract double AddContent(GuiComposer composer, double startTop);

		// Token: 0x06000672 RID: 1650 RVA: 0x0002FC54 File Offset: 0x0002DE54
		[NullableContext(2)]
		public virtual void Refresh(GuildSummary updatedGuild)
		{
			this.currentGuild = updatedGuild;
		}

		// Token: 0x06000673 RID: 1651 RVA: 0x0002FC60 File Offset: 0x0002DE60
		protected bool HasManagePermissions()
		{
			GuildRole role;
			return this.currentGuild != null && !string.IsNullOrEmpty(this.currentGuild.PlayerRole) && this.currentGuild.Roles.TryGetValue(this.currentGuild.PlayerRole, out role) && role.Permissions.HasFlag(GuildPermission.ManageRoles);
		}

		// Token: 0x06000674 RID: 1652 RVA: 0x0002FCC0 File Offset: 0x0002DEC0
		protected bool HasInvitePermissions()
		{
			GuildRole role;
			return this.currentGuild != null && !string.IsNullOrEmpty(this.currentGuild.PlayerRole) && this.currentGuild.Roles.TryGetValue(this.currentGuild.PlayerRole, out role) && role.Permissions.HasFlag(GuildPermission.Invite);
		}

		// Token: 0x06000675 RID: 1653 RVA: 0x0002FD20 File Offset: 0x0002DF20
		protected bool IsLeader()
		{
			GuildRole role;
			return this.currentGuild != null && !string.IsNullOrEmpty(this.currentGuild.PlayerRole) && this.currentGuild.Roles.TryGetValue(this.currentGuild.PlayerRole, out role) && role.Hierarchy == 1;
		}

		// Token: 0x06000676 RID: 1654 RVA: 0x0002FD74 File Offset: 0x0002DF74
		protected string ColorToHex(int argbColor)
		{
			byte r = (byte)((uint)argbColor >> 16 & 255U);
			byte g = (byte)((uint)argbColor >> 8 & 255U);
			byte b = (byte)(argbColor & 255);
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(1, 3);
			defaultInterpolatedStringHandler.AppendLiteral("#");
			defaultInterpolatedStringHandler.AppendFormatted<byte>(r, "X2");
			defaultInterpolatedStringHandler.AppendFormatted<byte>(g, "X2");
			defaultInterpolatedStringHandler.AppendFormatted<byte>(b, "X2");
			return defaultInterpolatedStringHandler.ToStringAndClear();
		}

		// Token: 0x040002B4 RID: 692
		protected ICoreClientAPI capi;

		// Token: 0x040002B5 RID: 693
		protected SRGuildsAndKingdomsModSystem modSystem;

		// Token: 0x040002B6 RID: 694
		[Nullable(2)]
		protected GuildSummary currentGuild;
	}
}
