using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using SRGuildsAndKingdoms.src.guilds;
using SRGuildsAndKingdoms.src.network;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;

namespace SRGuildsAndKingdoms.src.gui
{
	// Token: 0x02000079 RID: 121
	[NullableContext(1)]
	[Nullable(0)]
	internal class DialogGuildMembers : GuiDialog
	{
		// Token: 0x0600050A RID: 1290 RVA: 0x0001F2E0 File Offset: 0x0001D4E0
		public DialogGuildMembers(ICoreClientAPI capi, SRGuildsAndKingdomsModSystem modSystem) : base(capi)
		{
			this.modSystem = modSystem;
			this.currentGuild = modSystem.GetCurrentPlayerGuildSummary();
			if (this.currentGuild != null)
			{
				this.canManageRoles = this.HasManageRolesPermission();
				this.canKickMembers = this.HasKickPermission();
				GuildNetworkHandler networkHandler = modSystem.GetNetworkHandler();
				if (networkHandler != null)
				{
					networkHandler.RegisterMemberListCallback(new Action<List<GuildMemberInfo>>(this.OnMemberListReceived));
				}
				if (networkHandler != null)
				{
					networkHandler.SendGuildMemberListRequest();
				}
			}
			this.SetupDialog();
		}

		// Token: 0x1700016B RID: 363
		// (get) Token: 0x0600050B RID: 1291 RVA: 0x0001F361 File Offset: 0x0001D561
		public override string ToggleKeyCombinationCode
		{
			get
			{
				return "guildmembers";
			}
		}

		// Token: 0x0600050C RID: 1292 RVA: 0x0001F368 File Offset: 0x0001D568
		private bool HasManageRolesPermission()
		{
			GuildRole role;
			return this.currentGuild != null && this.currentGuild.Roles.TryGetValue(this.currentGuild.PlayerRole, out role) && (role.Permissions & GuildPermission.Promote) > GuildPermission.None;
		}

		// Token: 0x0600050D RID: 1293 RVA: 0x0001F3AC File Offset: 0x0001D5AC
		private bool HasKickPermission()
		{
			GuildRole role;
			return this.currentGuild != null && this.currentGuild.Roles.TryGetValue(this.currentGuild.PlayerRole, out role) && (role.Permissions & GuildPermission.Kick) > GuildPermission.None;
		}

		// Token: 0x0600050E RID: 1294 RVA: 0x0001F3F0 File Offset: 0x0001D5F0
		private void OnMemberListReceived(List<GuildMemberInfo> memberList)
		{
			this.members = memberList;
			this.members = (from m in this.members
			orderby m.IsOnline descending, (!(m.Role == "Leader")) ? 1 : 0, m.PlayerName
			select m).ToList<GuildMemberInfo>();
			if (this.IsOpened())
			{
				this.SetupDialog();
			}
		}

		// Token: 0x0600050F RID: 1295 RVA: 0x0001F490 File Offset: 0x0001D690
		private void SetupDialog()
		{
			if (this.currentGuild == null)
			{
				this.SetupNoGuildDialog();
				return;
			}
			ElementBounds dialogBounds = ElementStdBounds.AutosizedMainDialog.WithAlignment(6);
			ElementBounds bgBounds = ElementBounds.Fill.WithFixedPadding(GuiStyle.ElementToDialogPadding);
			bgBounds.BothSizing = 2;
			GuiComposer composer = GuiComposerHelpers.AddDialogTitleBar(GuiComposerHelpers.AddShadedDialogBG(this.capi.Gui.CreateCompo("guildmembers", dialogBounds), bgBounds, true, 5.0, 0.75f), Lang.Get("srguildsandkingdoms:guild-members-title", new object[]
			{
				this.currentGuild.Name
			}), new Action(this.OnTitleBarCloseClicked), null, null, null).BeginChildElements(bgBounds);
			double top = 20.0;
			double spacing = 8.0;
			double elementHeight = 25.0;
			double dialogWidth = 700.0;
			GuiComposerHelpers.AddStaticText(composer, Lang.Get("srguildsandkingdoms:guild-members-instructions", Array.Empty<object>()), CairoFont.WhiteSmallText(), ElementBounds.Fixed(0.0, top, dialogWidth, elementHeight), null);
			top += elementHeight + spacing;
			double nameColWidth = 180.0;
			double roleColWidth = 150.0;
			double statusColWidth = 100.0;
			double lastSeenColWidth = 170.0;
			double actionsColWidth = 100.0;
			GuiComposerHelpers.AddStaticText(composer, Lang.Get("srguildsandkingdoms:member-name", Array.Empty<object>()), CairoFont.WhiteDetailText(), ElementBounds.Fixed(0.0, top, nameColWidth, elementHeight), null);
			GuiComposerHelpers.AddStaticText(composer, Lang.Get("srguildsandkingdoms:member-role", Array.Empty<object>()), CairoFont.WhiteDetailText(), ElementBounds.Fixed(nameColWidth, top, roleColWidth, elementHeight), null);
			GuiComposerHelpers.AddStaticText(composer, Lang.Get("srguildsandkingdoms:member-status", Array.Empty<object>()), CairoFont.WhiteDetailText(), ElementBounds.Fixed(nameColWidth + roleColWidth, top, statusColWidth, elementHeight), null);
			GuiComposerHelpers.AddStaticText(composer, Lang.Get("srguildsandkingdoms:member-lastseen", Array.Empty<object>()), CairoFont.WhiteDetailText(), ElementBounds.Fixed(nameColWidth + roleColWidth + statusColWidth, top, lastSeenColWidth, elementHeight), null);
			if (this.canKickMembers)
			{
				GuiComposerHelpers.AddStaticText(composer, Lang.Get("srguildsandkingdoms:member-actions", Array.Empty<object>()), CairoFont.WhiteDetailText(), ElementBounds.Fixed(nameColWidth + roleColWidth + statusColWidth + lastSeenColWidth, top, actionsColWidth, elementHeight), null);
			}
			top += elementHeight + spacing;
			if (this.members.Count == 0)
			{
				GuiComposerHelpers.AddStaticText(composer, Lang.Get("srguildsandkingdoms:loading-members", Array.Empty<object>()), CairoFont.WhiteSmallText().WithColor(new double[]
				{
					0.7,
					0.7,
					0.7,
					1.0
				}), ElementBounds.Fixed(0.0, top, dialogWidth, elementHeight * 2.0), null);
				top += elementHeight * 3.0;
			}
			else
			{
				double scrollableTop = top;
				int maxVisibleMembers = 10;
				double memberListHeight = (double)Math.Min(this.members.Count, maxVisibleMembers) * (elementHeight + spacing);
				foreach (GuildMemberInfo member in this.members)
				{
					this.AddMemberRow(composer, ref top, member, nameColWidth, roleColWidth, statusColWidth, lastSeenColWidth, actionsColWidth, elementHeight, spacing);
				}
				if (this.members.Count > maxVisibleMembers)
				{
					top = scrollableTop + memberListHeight + spacing * 2.0;
				}
			}
			GuiComposerHelpers.AddSmallButton(composer, Lang.Get("srguildsandkingdoms:close", Array.Empty<object>()), new ActionConsumable(this.OnClose), ElementBounds.Fixed(0.0, top + spacing, 100.0, elementHeight), 2, null);
			base.SingleComposer = composer.Compose(true);
		}

		// Token: 0x06000510 RID: 1296 RVA: 0x0001F81C File Offset: 0x0001DA1C
		private void AddMemberRow(GuiComposer composer, ref double top, GuildMemberInfo member, double nameColWidth, double roleColWidth, double statusColWidth, double lastSeenColWidth, double actionsColWidth, double elementHeight, double spacing)
		{
			CairoFont nameColor = member.IsOnline ? CairoFont.WhiteSmallText() : CairoFont.WhiteSmallText().WithColor(new double[]
			{
				0.6,
				0.6,
				0.6,
				1.0
			});
			GuiComposerHelpers.AddStaticText(composer, member.PlayerName, nameColor, ElementBounds.Fixed(0.0, top, nameColWidth, elementHeight), null);
			if (this.canManageRoles && member.PlayerUid != this.capi.World.Player.PlayerUID)
			{
				string[] roleNames = this.currentGuild.Roles.Keys.ToArray<string>();
				int selectedIndex = Array.IndexOf<string>(roleNames, member.Role);
				if (selectedIndex < 0)
				{
					selectedIndex = 0;
				}
				GuiComposerHelpers.AddDropDown(composer, roleNames, roleNames, selectedIndex, delegate(string code, bool selected)
				{
					this.OnRoleChanged(member, code);
				}, ElementBounds.Fixed(nameColWidth, top, roleColWidth - 10.0, elementHeight), "role_dropdown_" + member.PlayerUid);
			}
			else
			{
				string roleText = (member.PlayerUid == this.capi.World.Player.PlayerUID) ? (member.Role + " (You)") : member.Role;
				GuiComposerHelpers.AddStaticText(composer, roleText, CairoFont.WhiteSmallText(), ElementBounds.Fixed(nameColWidth, top, roleColWidth, elementHeight), null);
			}
			string statusText = member.IsOnline ? Lang.Get("srguildsandkingdoms:status-online", Array.Empty<object>()) : Lang.Get("srguildsandkingdoms:status-offline", Array.Empty<object>());
			CairoFont statusColor = member.IsOnline ? CairoFont.WhiteSmallText().WithColor(new double[]
			{
				0.3,
				1.0,
				0.3,
				1.0
			}) : CairoFont.WhiteSmallText().WithColor(new double[]
			{
				0.7,
				0.3,
				0.3,
				1.0
			});
			GuiComposerHelpers.AddStaticText(composer, statusText, statusColor, ElementBounds.Fixed(nameColWidth + roleColWidth, top, statusColWidth, elementHeight), null);
			DateTime lastSeen = new DateTime(member.LastSeenTicks);
			string lastSeenText = member.IsOnline ? Lang.Get("srguildsandkingdoms:now", Array.Empty<object>()) : this.FormatLastSeen(lastSeen);
			GuiComposerHelpers.AddStaticText(composer, lastSeenText, CairoFont.WhiteSmallText(), ElementBounds.Fixed(nameColWidth + roleColWidth + statusColWidth, top, lastSeenColWidth, elementHeight), null);
			if (this.canKickMembers && (member.PlayerUid != this.capi.World.Player.PlayerUID && member.Role != "Leader"))
			{
				GuiComposerHelpers.AddSmallButton(composer, Lang.Get("srguildsandkingdoms:kick", Array.Empty<object>()), () => this.OnKickMember(member), ElementBounds.Fixed(nameColWidth + roleColWidth + statusColWidth + lastSeenColWidth, top, 60.0, elementHeight), 2, "kick_btn_" + member.PlayerUid);
			}
			top += elementHeight + spacing;
		}

		// Token: 0x06000511 RID: 1297 RVA: 0x0001FB40 File Offset: 0x0001DD40
		private void OnRoleChanged(GuildMemberInfo member, string newRole)
		{
			if (this.currentGuild == null)
			{
				return;
			}
			GuildNetworkHandler networkHandler = this.modSystem.GetNetworkHandler();
			if (networkHandler != null)
			{
				networkHandler.SendGuildRoleManagementRequest("assign", newRole, member.PlayerName, null, 999);
			}
			this.capi.ShowChatMessage(Lang.Get("srguildsandkingdoms:role-change-sent", new object[]
			{
				member.PlayerName,
				newRole
			}));
			if (networkHandler != null)
			{
				networkHandler.SendGuildMemberListRequest();
			}
		}

		// Token: 0x06000512 RID: 1298 RVA: 0x0001FBB0 File Offset: 0x0001DDB0
		private bool OnKickMember(GuildMemberInfo member)
		{
			if (this.currentGuild == null)
			{
				return false;
			}
			if (member.PlayerUid == this.capi.World.Player.PlayerUID)
			{
				this.capi.ShowChatMessage(Lang.Get("srguildsandkingdoms:cannot-kick-self", Array.Empty<object>()));
				return false;
			}
			if (member.Role == "Leader")
			{
				this.capi.ShowChatMessage(Lang.Get("srguildsandkingdoms:cannot-kick-leader", Array.Empty<object>()));
				return false;
			}
			if (!this.canKickMembers)
			{
				this.capi.ShowChatMessage(Lang.Get("srguildsandkingdoms:no-permission-kick", Array.Empty<object>()));
				return false;
			}
			new DialogKickConfirm(this.capi, member.PlayerName, delegate
			{
				GuildNetworkHandler networkHandler = this.modSystem.GetNetworkHandler();
				if (networkHandler != null)
				{
					networkHandler.SendGuildRemoveMemberRequest(member.PlayerName);
				}
				this.capi.ShowChatMessage(Lang.Get("srguildsandkingdoms:kick-request-sent", new object[]
				{
					member.PlayerName
				}));
				if (networkHandler != null)
				{
					networkHandler.SendGuildMemberListRequest();
				}
			}).TryOpen();
			return true;
		}

		// Token: 0x06000513 RID: 1299 RVA: 0x0001FCA0 File Offset: 0x0001DEA0
		private string FormatLastSeen(DateTime lastSeen)
		{
			TimeSpan timeSpan = DateTime.UtcNow - lastSeen;
			if (timeSpan.TotalMinutes < 1.0)
			{
				return Lang.Get("srguildsandkingdoms:just-now", Array.Empty<object>());
			}
			if (timeSpan.TotalMinutes < 60.0)
			{
				return Lang.Get("srguildsandkingdoms:minutes-ago", new object[]
				{
					(int)timeSpan.TotalMinutes
				});
			}
			if (timeSpan.TotalHours < 24.0)
			{
				return Lang.Get("srguildsandkingdoms:hours-ago", new object[]
				{
					(int)timeSpan.TotalHours
				});
			}
			if (timeSpan.TotalDays < 7.0)
			{
				return Lang.Get("srguildsandkingdoms:days-ago", new object[]
				{
					(int)timeSpan.TotalDays
				});
			}
			if (timeSpan.TotalDays < 30.0)
			{
				return Lang.Get("srguildsandkingdoms:weeks-ago", new object[]
				{
					(int)(timeSpan.TotalDays / 7.0)
				});
			}
			return lastSeen.ToString("yyyy-MM-dd");
		}

		// Token: 0x06000514 RID: 1300 RVA: 0x0001FDC0 File Offset: 0x0001DFC0
		private void SetupNoGuildDialog()
		{
			ElementBounds dialogBounds = ElementStdBounds.AutosizedMainDialog.WithAlignment(6);
			ElementBounds bgBounds = ElementBounds.Fill.WithFixedPadding(GuiStyle.ElementToDialogPadding);
			bgBounds.BothSizing = 2;
			GuiComposer composer = GuiComposerHelpers.AddDialogTitleBar(GuiComposerHelpers.AddShadedDialogBG(this.capi.Gui.CreateCompo("guildmembers", dialogBounds), bgBounds, true, 5.0, 0.75f), Lang.Get("srguildsandkingdoms:guild-members-title", new object[]
			{
				"None"
			}), new Action(this.OnTitleBarCloseClicked), null, null, null).BeginChildElements(bgBounds);
			GuiComposerHelpers.AddStaticText(composer, Lang.Get("srguildsandkingdoms:not-in-guild", Array.Empty<object>()), CairoFont.WhiteDetailText(), ElementBounds.Fixed(0.0, 0.0, 400.0, 50.0), null);
			GuiComposerHelpers.AddSmallButton(composer, Lang.Get("srguildsandkingdoms:close", Array.Empty<object>()), new ActionConsumable(this.OnClose), ElementBounds.Fixed(0.0, 60.0, 100.0, 25.0), 2, null);
			base.SingleComposer = composer.Compose(true);
		}

		// Token: 0x06000515 RID: 1301 RVA: 0x0001FEED File Offset: 0x0001E0ED
		private bool OnClose()
		{
			this.TryClose();
			return true;
		}

		// Token: 0x06000516 RID: 1302 RVA: 0x0001FEF7 File Offset: 0x0001E0F7
		private void OnTitleBarCloseClicked()
		{
			this.TryClose();
		}

		// Token: 0x06000517 RID: 1303 RVA: 0x0001FF00 File Offset: 0x0001E100
		public override void OnGuiClosed()
		{
			base.OnGuiClosed();
			GuildNetworkHandler networkHandler = this.modSystem.GetNetworkHandler();
			if (networkHandler == null)
			{
				return;
			}
			networkHandler.UnregisterMemberListCallback();
		}

		// Token: 0x1700016C RID: 364
		// (get) Token: 0x06000518 RID: 1304 RVA: 0x0001FF1D File Offset: 0x0001E11D
		public override bool PrefersUngrabbedMouse
		{
			get
			{
				return false;
			}
		}

		// Token: 0x040001E4 RID: 484
		private SRGuildsAndKingdomsModSystem modSystem;

		// Token: 0x040001E5 RID: 485
		[Nullable(2)]
		private GuildSummary currentGuild;

		// Token: 0x040001E6 RID: 486
		private List<GuildMemberInfo> members = new List<GuildMemberInfo>();

		// Token: 0x040001E7 RID: 487
		private bool canManageRoles;

		// Token: 0x040001E8 RID: 488
		private bool canKickMembers;
	}
}
