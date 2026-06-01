using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using SRGuildsAndKingdoms.src.guilds;
using SRGuildsAndKingdoms.src.network;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;

namespace SRGuildsAndKingdoms.src.gui
{
	// Token: 0x0200007B RID: 123
	[NullableContext(1)]
	[Nullable(0)]
	public class DialogGuildPendingInvites : GuiDialog
	{
		// Token: 0x06000520 RID: 1312 RVA: 0x00020107 File Offset: 0x0001E307
		public DialogGuildPendingInvites(ICoreClientAPI capi, SRGuildsAndKingdomsModSystem modSystem) : base(capi)
		{
			this.modSystem = modSystem;
			this.LoadGuildData();
			this.SetupDialog();
		}

		// Token: 0x1700016F RID: 367
		// (get) Token: 0x06000521 RID: 1313 RVA: 0x0002012E File Offset: 0x0001E32E
		public override string ToggleKeyCombinationCode
		{
			get
			{
				return "guildpendinginvites";
			}
		}

		// Token: 0x06000522 RID: 1314 RVA: 0x00020138 File Offset: 0x0001E338
		private void LoadGuildData()
		{
			this.currentGuild = this.modSystem.GetCurrentPlayerGuildSummary();
			if (this.currentGuild == null)
			{
				return;
			}
			this.pendingInvites.Clear();
			foreach (GuildInviteDto invite in this.currentGuild.PendingInvites)
			{
				string inviterName = this.GetPlayerName(invite.InviterUid);
				string inviteeName = this.GetPlayerName(invite.InviteeUid);
				this.pendingInvites.Add(new DialogGuildPendingInvites.PendingInviteInfo
				{
					InviteeUid = invite.InviteeUid,
					InviteeNameDisplay = inviteeName,
					InviterName = inviterName,
					ExpiresAt = invite.ExpiresAt
				});
			}
		}

		// Token: 0x06000523 RID: 1315 RVA: 0x00020200 File Offset: 0x0001E400
		private string GetPlayerName(string playerUid)
		{
			IPlayer onlinePlayer = this.capi.World.AllOnlinePlayers.FirstOrDefault((IPlayer p) => p.PlayerUID == playerUid);
			if (onlinePlayer != null)
			{
				return onlinePlayer.PlayerName;
			}
			if (playerUid.Length <= 8)
			{
				return playerUid;
			}
			return playerUid.Substring(0, 8) + "...";
		}

		// Token: 0x06000524 RID: 1316 RVA: 0x00020274 File Offset: 0x0001E474
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
			GuiComposer composer = GuiComposerHelpers.AddDialogTitleBar(GuiComposerHelpers.AddShadedDialogBG(this.capi.Gui.CreateCompo("guildpendinginvites", dialogBounds), bgBounds, true, 5.0, 0.75f), Lang.Get("srguildsandkingdoms:pending-invites-title", Array.Empty<object>()), new Action(this.OnTitleBarCloseClicked), null, null, null).BeginChildElements(bgBounds);
			double top = 20.0;
			double elementHeight = 25.0;
			double spacing = 10.0;
			double width = 600.0;
			GuiComposerHelpers.AddStaticText(composer, Lang.Get("srguildsandkingdoms:pending-invites-header", new object[]
			{
				this.pendingInvites.Count
			}), CairoFont.WhiteMediumText(), ElementBounds.Fixed(0.0, top, width, elementHeight), null);
			top += elementHeight + spacing;
			if (this.pendingInvites.Count == 0)
			{
				GuiComposerHelpers.AddStaticText(composer, Lang.Get("srguildsandkingdoms:no-pending-invites", Array.Empty<object>()), CairoFont.WhiteSmallText(), ElementBounds.Fixed(0.0, top, width, elementHeight), null);
				top += elementHeight + spacing;
			}
			else
			{
				GuiComposerHelpers.AddStaticText(composer, "Invited Player", CairoFont.WhiteSmallText(), ElementBounds.Fixed(0.0, top, 150.0, elementHeight), null);
				GuiComposerHelpers.AddStaticText(composer, "Invited By", CairoFont.WhiteSmallText(), ElementBounds.Fixed(160.0, top, 150.0, elementHeight), null);
				GuiComposerHelpers.AddStaticText(composer, "Time Remaining", CairoFont.WhiteSmallText(), ElementBounds.Fixed(320.0, top, 120.0, elementHeight), null);
				GuiComposerHelpers.AddStaticText(composer, "Action", CairoFont.WhiteSmallText(), ElementBounds.Fixed(450.0, top, 100.0, elementHeight), null);
				top += elementHeight + 5.0;
				for (int i = 0; i < this.pendingInvites.Count; i++)
				{
					DialogGuildPendingInvites.PendingInviteInfo invite = this.pendingInvites[i];
					double rowTop = top + (double)i * (elementHeight + 5.0);
					GuiComposerHelpers.AddStaticText(composer, invite.InviteeNameDisplay, CairoFont.WhiteDetailText(), ElementBounds.Fixed(0.0, rowTop, 150.0, elementHeight), null);
					GuiComposerHelpers.AddStaticText(composer, invite.InviterName, CairoFont.WhiteDetailText(), ElementBounds.Fixed(160.0, rowTop, 150.0, elementHeight), null);
					string timeRemaining = invite.GetTimeRemaining();
					CairoFont timeColor = (invite.ExpiresAt <= DateTime.UtcNow) ? CairoFont.WhiteDetailText().WithColor(GuiStyle.ErrorTextColor) : CairoFont.WhiteDetailText();
					GuiComposerHelpers.AddStaticText(composer, timeRemaining, timeColor, ElementBounds.Fixed(320.0, rowTop, 120.0, elementHeight), null);
					int inviteIndex = i;
					GuiComposer guiComposer = composer;
					string text = Lang.Get("srguildsandkingdoms:cancel-invite", Array.Empty<object>());
					ActionConsumable actionConsumable = () => this.OnCancelInvite(invite.InviteeUid);
					ElementBounds elementBounds = ElementBounds.Fixed(450.0, rowTop, 100.0, elementHeight);
					EnumButtonStyle enumButtonStyle = 2;
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(13, 1);
					defaultInterpolatedStringHandler.AppendLiteral("cancelinvite_");
					defaultInterpolatedStringHandler.AppendFormatted<int>(inviteIndex);
					GuiComposerHelpers.AddSmallButton(guiComposer, text, actionConsumable, elementBounds, enumButtonStyle, defaultInterpolatedStringHandler.ToStringAndClear());
				}
				top += (double)this.pendingInvites.Count * (elementHeight + 5.0) + spacing;
			}
			GuiComposerHelpers.AddSmallButton(composer, Lang.Get("srguildsandkingdoms:refresh-invites", Array.Empty<object>()), new ActionConsumable(this.OnRefresh), ElementBounds.Fixed(0.0, top, 120.0, elementHeight), 2, null);
			GuiComposerHelpers.AddSmallButton(composer, Lang.Get("srguildsandkingdoms:close", Array.Empty<object>()), new ActionConsumable(this.OnClose), ElementBounds.Fixed(130.0, top, 120.0, elementHeight), 2, null);
			base.SingleComposer = composer.Compose(true);
		}

		// Token: 0x06000525 RID: 1317 RVA: 0x000206BC File Offset: 0x0001E8BC
		private void SetupNoGuildDialog()
		{
			ElementBounds dialogBounds = ElementStdBounds.AutosizedMainDialog.WithAlignment(6);
			ElementBounds bgBounds = ElementBounds.Fill.WithFixedPadding(GuiStyle.ElementToDialogPadding);
			bgBounds.BothSizing = 2;
			GuiComposer composer = GuiComposerHelpers.AddDialogTitleBar(GuiComposerHelpers.AddShadedDialogBG(this.capi.Gui.CreateCompo("guildpendinginvites", dialogBounds), bgBounds, true, 5.0, 0.75f), Lang.Get("srguildsandkingdoms:pending-invites-title", Array.Empty<object>()), new Action(this.OnTitleBarCloseClicked), null, null, null).BeginChildElements(bgBounds);
			GuiComposerHelpers.AddStaticText(composer, Lang.Get("srguildsandkingdoms:not-in-guild", Array.Empty<object>()), CairoFont.WhiteDetailText(), ElementBounds.Fixed(0.0, 20.0, 400.0, 50.0), null);
			base.SingleComposer = composer.Compose(true);
		}

		// Token: 0x06000526 RID: 1318 RVA: 0x00020794 File Offset: 0x0001E994
		private bool OnCancelInvite(string inviteeUid)
		{
			GuildNetworkHandler networkHandler = this.modSystem.GetNetworkHandler();
			if (networkHandler == null)
			{
				this.capi.ShowChatMessage(Lang.Get("srguildsandkingdoms:network-error", Array.Empty<object>()));
				return true;
			}
			networkHandler.SendCancelInviteRequest(inviteeUid);
			this.capi.ShowChatMessage(Lang.Get("srguildsandkingdoms:invite-cancel-requested", Array.Empty<object>()));
			this.capi.Event.EnqueueMainThreadTask(delegate()
			{
				Thread.Sleep(500);
				this.LoadGuildData();
				this.SetupDialog();
			}, "refreshinvites");
			return true;
		}

		// Token: 0x06000527 RID: 1319 RVA: 0x0002080F File Offset: 0x0001EA0F
		private bool OnRefresh()
		{
			this.LoadGuildData();
			this.SetupDialog();
			this.capi.ShowChatMessage(Lang.Get("srguildsandkingdoms:invites-refreshed", Array.Empty<object>()));
			return true;
		}

		// Token: 0x06000528 RID: 1320 RVA: 0x00020838 File Offset: 0x0001EA38
		private bool OnClose()
		{
			this.TryClose();
			return true;
		}

		// Token: 0x06000529 RID: 1321 RVA: 0x00020842 File Offset: 0x0001EA42
		private void OnTitleBarCloseClicked()
		{
			this.TryClose();
		}

		// Token: 0x17000170 RID: 368
		// (get) Token: 0x0600052A RID: 1322 RVA: 0x0002084B File Offset: 0x0001EA4B
		public override bool PrefersUngrabbedMouse
		{
			get
			{
				return false;
			}
		}

		// Token: 0x040001EB RID: 491
		private SRGuildsAndKingdomsModSystem modSystem;

		// Token: 0x040001EC RID: 492
		[Nullable(2)]
		private GuildSummary currentGuild;

		// Token: 0x040001ED RID: 493
		private List<DialogGuildPendingInvites.PendingInviteInfo> pendingInvites = new List<DialogGuildPendingInvites.PendingInviteInfo>();

		// Token: 0x0200011D RID: 285
		[Nullable(0)]
		private class PendingInviteInfo
		{
			// Token: 0x17000287 RID: 647
			// (get) Token: 0x06000AEF RID: 2799 RVA: 0x00046929 File Offset: 0x00044B29
			// (set) Token: 0x06000AF0 RID: 2800 RVA: 0x00046931 File Offset: 0x00044B31
			public string InviteeUid { get; set; } = "";

			// Token: 0x17000288 RID: 648
			// (get) Token: 0x06000AF1 RID: 2801 RVA: 0x0004693A File Offset: 0x00044B3A
			// (set) Token: 0x06000AF2 RID: 2802 RVA: 0x00046942 File Offset: 0x00044B42
			public string InviteeNameDisplay { get; set; } = "";

			// Token: 0x17000289 RID: 649
			// (get) Token: 0x06000AF3 RID: 2803 RVA: 0x0004694B File Offset: 0x00044B4B
			// (set) Token: 0x06000AF4 RID: 2804 RVA: 0x00046953 File Offset: 0x00044B53
			public string InviterName { get; set; } = "";

			// Token: 0x1700028A RID: 650
			// (get) Token: 0x06000AF5 RID: 2805 RVA: 0x0004695C File Offset: 0x00044B5C
			// (set) Token: 0x06000AF6 RID: 2806 RVA: 0x00046964 File Offset: 0x00044B64
			public DateTime ExpiresAt { get; set; }

			// Token: 0x06000AF7 RID: 2807 RVA: 0x00046970 File Offset: 0x00044B70
			public string GetTimeRemaining()
			{
				TimeSpan remaining = this.ExpiresAt - DateTime.UtcNow;
				if (remaining.TotalSeconds <= 0.0)
				{
					return "Expired";
				}
				if (remaining.TotalMinutes < 1.0)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(1, 1);
					defaultInterpolatedStringHandler.AppendFormatted<int>((int)remaining.TotalSeconds);
					defaultInterpolatedStringHandler.AppendLiteral("s");
					return defaultInterpolatedStringHandler.ToStringAndClear();
				}
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(3, 2);
				defaultInterpolatedStringHandler2.AppendFormatted<int>((int)remaining.TotalMinutes);
				defaultInterpolatedStringHandler2.AppendLiteral("m ");
				defaultInterpolatedStringHandler2.AppendFormatted<int>(remaining.Seconds);
				defaultInterpolatedStringHandler2.AppendLiteral("s");
				return defaultInterpolatedStringHandler2.ToStringAndClear();
			}
		}
	}
}
