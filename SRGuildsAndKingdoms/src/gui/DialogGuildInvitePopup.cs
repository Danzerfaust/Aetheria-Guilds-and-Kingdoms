using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using SRGuildsAndKingdoms.src.network;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;

namespace SRGuildsAndKingdoms.src.gui
{
	// Token: 0x02000074 RID: 116
	[NullableContext(1)]
	[Nullable(0)]
	public class DialogGuildInvitePopup : HudElement
	{
		// Token: 0x1700015F RID: 351
		// (get) Token: 0x06000477 RID: 1143 RVA: 0x00019F3A File Offset: 0x0001813A
		public override string ToggleKeyCombinationCode
		{
			get
			{
				return null;
			}
		}

		// Token: 0x06000478 RID: 1144 RVA: 0x00019F3D File Offset: 0x0001813D
		public DialogGuildInvitePopup(ICoreClientAPI capi, SRGuildsAndKingdomsModSystem modSystem) : base(capi)
		{
			this.modSystem = modSystem;
		}

		// Token: 0x06000479 RID: 1145 RVA: 0x00019F64 File Offset: 0x00018164
		public void ShowInvite(GuildInviteNotificationPacket invite)
		{
			DateTime expiresAt = new DateTime(invite.ExpiresAtTicks, DateTimeKind.Utc);
			TimeSpan remaining = expiresAt - DateTime.UtcNow;
			this.capi.Logger.Notification("[InvitePopup] Received invite to " + invite.GuildName + " from " + invite.InviterName);
			ILogger logger = this.capi.Logger;
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(30, 1);
			defaultInterpolatedStringHandler.AppendLiteral("[InvitePopup] ExpiresAtTicks: ");
			defaultInterpolatedStringHandler.AppendFormatted<long>(invite.ExpiresAtTicks);
			logger.Notification(defaultInterpolatedStringHandler.ToStringAndClear());
			ILogger logger2 = this.capi.Logger;
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(25, 1);
			defaultInterpolatedStringHandler2.AppendLiteral("[InvitePopup] ExpiresAt: ");
			defaultInterpolatedStringHandler2.AppendFormatted<DateTime>(expiresAt);
			logger2.Notification(defaultInterpolatedStringHandler2.ToStringAndClear());
			ILogger logger3 = this.capi.Logger;
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler3 = new DefaultInterpolatedStringHandler(27, 1);
			defaultInterpolatedStringHandler3.AppendLiteral("[InvitePopup] Current UTC: ");
			defaultInterpolatedStringHandler3.AppendFormatted<DateTime>(DateTime.UtcNow);
			logger3.Notification(defaultInterpolatedStringHandler3.ToStringAndClear());
			ILogger logger4 = this.capi.Logger;
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler4 = new DefaultInterpolatedStringHandler(33, 1);
			defaultInterpolatedStringHandler4.AppendLiteral("[InvitePopup] Remaining: ");
			defaultInterpolatedStringHandler4.AppendFormatted<double>(remaining.TotalSeconds);
			defaultInterpolatedStringHandler4.AppendLiteral(" seconds");
			logger4.Notification(defaultInterpolatedStringHandler4.ToStringAndClear());
			GuildInviteInfo inviteInfo = new GuildInviteInfo
			{
				GuildName = invite.GuildName,
				InviterName = invite.InviterName,
				InviterUid = invite.InviterUid,
				ExpiresAtTicks = invite.ExpiresAtTicks
			};
			if (!this.currentInvites.Exists((GuildInviteInfo i) => i.GuildName == inviteInfo.GuildName && i.InviterUid == inviteInfo.InviterUid))
			{
				this.currentInvites.Add(inviteInfo);
			}
			if (!this.IsOpened())
			{
				this.currentInviteIndex = 0;
				this.ComposeDialog();
				this.TryOpen();
				this.SetupUpdateListener();
			}
		}

		// Token: 0x0600047A RID: 1146 RVA: 0x0001A134 File Offset: 0x00018334
		public void ShowInvites(List<GuildInviteInfo> invites)
		{
			this.currentInvites.Clear();
			DateTime now = DateTime.UtcNow;
			foreach (GuildInviteInfo invite in invites)
			{
				if (new DateTime(invite.ExpiresAtTicks, DateTimeKind.Utc) > now)
				{
					this.currentInvites.Add(invite);
				}
			}
			if (this.currentInvites.Count > 0)
			{
				this.currentInviteIndex = 0;
				this.ComposeDialog();
				this.TryOpen();
				this.SetupUpdateListener();
				return;
			}
			this.capi.ShowChatMessage(Lang.Get("srguildsandkingdoms:no-pending-invites", Array.Empty<object>()));
		}

		// Token: 0x0600047B RID: 1147 RVA: 0x0001A1F0 File Offset: 0x000183F0
		private void ComposeDialog()
		{
			this.RemoveExpiredInvites();
			if (this.currentInvites.Count == 0)
			{
				this.TryClose();
				return;
			}
			if (this.currentInviteIndex >= this.currentInvites.Count)
			{
				this.currentInviteIndex = this.currentInvites.Count - 1;
			}
			if (this.currentInviteIndex < 0)
			{
				this.currentInviteIndex = 0;
			}
			GuildInviteInfo currentInvite = this.currentInvites[this.currentInviteIndex];
			if (currentInvite.ExpiresAtTicks <= 0L)
			{
				ILogger logger = this.capi.Logger;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(49, 2);
				defaultInterpolatedStringHandler.AppendLiteral("[InvitePopup] Invalid ExpiresAtTicks: ");
				defaultInterpolatedStringHandler.AppendFormatted<long>(currentInvite.ExpiresAtTicks);
				defaultInterpolatedStringHandler.AppendLiteral(" for guild ");
				defaultInterpolatedStringHandler.AppendFormatted(currentInvite.GuildName);
				logger.Error(defaultInterpolatedStringHandler.ToStringAndClear());
				this.currentInvites.RemoveAt(this.currentInviteIndex);
				if (this.currentInvites.Count > 0)
				{
					this.ComposeDialog();
					return;
				}
				this.TryClose();
				return;
			}
			else
			{
				TimeSpan timeRemaining = new DateTime(currentInvite.ExpiresAtTicks, DateTimeKind.Utc) - DateTime.UtcNow;
				if (timeRemaining.TotalSeconds > 0.0)
				{
					this.lastComposeTime = DateTime.UtcNow;
					string timeText;
					if (timeRemaining.TotalMinutes < 1.0)
					{
						timeText = Lang.Get("srguildsandkingdoms:invite-expires-seconds", new object[]
						{
							(int)timeRemaining.TotalSeconds
						});
					}
					else
					{
						timeText = Lang.Get("srguildsandkingdoms:invite-expires-minutes", new object[]
						{
							(int)timeRemaining.TotalMinutes
						});
					}
					double width = 420.0;
					double height = 250.0;
					double rightMargin = 30.0;
					double bottomMargin = 30.0;
					ElementBounds dialogBounds = ElementBounds.Fixed(11, rightMargin, bottomMargin, width, height);
					ElementBounds bgBounds = ElementBounds.Fill.WithFixedPadding(GuiStyle.ElementToDialogPadding);
					bgBounds.BothSizing = 2;
					try
					{
						GuiComposer singleComposer = base.SingleComposer;
						if (singleComposer != null)
						{
							singleComposer.Dispose();
						}
					}
					catch (Exception ex)
					{
						this.capi.Logger.Warning("[InvitePopup] Error disposing composer: " + ex.Message);
					}
					GuiComposer composer = GuiComposerHelpers.AddDialogTitleBar(GuiComposerHelpers.AddShadedDialogBG(this.capi.Gui.CreateCompo("guildinvitepopup", dialogBounds), bgBounds, true, 5.0, 0.75f), Lang.Get("srguildsandkingdoms:guild-invite-title", Array.Empty<object>()), new Action(this.OnCloseClicked), null, null, null).BeginChildElements(bgBounds);
					ElementBounds messageBounds = ElementBounds.Fixed(0.0, 30.0, width - 50.0, 40.0);
					ElementBounds timeBounds = messageBounds.BelowCopy(0.0, 5.0, 0.0, 0.0).WithFixedHeight(20.0);
					ElementBounds keyHintBounds = timeBounds.BelowCopy(0.0, 5.0, 0.0, 0.0).WithFixedHeight(20.0);
					GuiComposer guiComposer = GuiComposerHelpers.AddStaticText(GuiComposerHelpers.AddStaticText(composer, Lang.Get("srguildsandkingdoms:guild-invite-message", new object[]
					{
						currentInvite.GuildName,
						currentInvite.InviterName
					}), CairoFont.WhiteSmallText(), messageBounds, null), timeText, CairoFont.WhiteSmallText().WithColor(GuiStyle.WarningTextColor), timeBounds, null);
					string text = "srguildsandkingdoms:press-key-to-interact";
					object[] array = new object[1];
					int num = 0;
					HotKey hotKeyByCode = this.capi.Input.GetHotKeyByCode("togglemousecontrol");
					object obj;
					if (hotKeyByCode == null)
					{
						obj = null;
					}
					else
					{
						KeyCombination currentMapping = hotKeyByCode.CurrentMapping;
						obj = ((currentMapping != null) ? currentMapping.ToString() : null);
					}
					array[num] = (obj ?? "NOT SET");
					GuiComposerHelpers.AddStaticText(guiComposer, Lang.Get(text, array), CairoFont.WhiteSmallText().WithColor(new double[]
					{
						0.7,
						0.7,
						0.7,
						1.0
					}), keyHintBounds, null);
					if (this.currentInvites.Count > 1)
					{
						ElementBounds countBounds = keyHintBounds.BelowCopy(0.0, 5.0, 0.0, 0.0).WithFixedHeight(20.0);
						GuiComposerHelpers.AddStaticText(composer, Lang.Get("srguildsandkingdoms:invite-count", new object[]
						{
							this.currentInviteIndex + 1,
							this.currentInvites.Count
						}), CairoFont.WhiteSmallText(), countBounds, null);
					}
					double buttonY = height - 100.0;
					int buttonWidth = 110;
					int buttonSpacing = 15;
					ElementBounds acceptBounds = ElementBounds.Fixed(10.0, buttonY, (double)buttonWidth, 30.0);
					ElementBounds declineBounds = ElementBounds.Fixed((double)(10 + buttonWidth + buttonSpacing), buttonY, (double)buttonWidth, 30.0);
					GuiComposerHelpers.AddSmallButton(GuiComposerHelpers.AddSmallButton(composer, Lang.Get("srguildsandkingdoms:accept", Array.Empty<object>()), new ActionConsumable(this.OnAcceptClicked), acceptBounds, 2, null), Lang.Get("srguildsandkingdoms:decline", Array.Empty<object>()), new ActionConsumable(this.OnDeclineClicked), declineBounds, 2, null);
					if (this.currentInvites.Count > 1)
					{
						ElementBounds prevBounds = ElementBounds.Fixed((double)(10 + (buttonWidth + buttonSpacing) * 2), buttonY, 30.0, 30.0);
						ElementBounds nextBounds = ElementBounds.Fixed((double)(10 + (buttonWidth + buttonSpacing) * 2 + 40), buttonY, 30.0, 30.0);
						GuiComposerHelpers.AddSmallButton(GuiComposerHelpers.AddSmallButton(composer, "<", new ActionConsumable(this.OnPreviousClicked), prevBounds, 2, null), ">", new ActionConsumable(this.OnNextClicked), nextBounds, 2, null);
					}
					composer.EndChildElements().Compose(true);
					base.SingleComposer = composer;
					return;
				}
				this.capi.Logger.Warning("[InvitePopup] Invite already expired: " + currentInvite.GuildName);
				this.currentInvites.RemoveAt(this.currentInviteIndex);
				if (this.currentInvites.Count > 0)
				{
					this.ComposeDialog();
					return;
				}
				this.TryClose();
				return;
			}
		}

		// Token: 0x0600047C RID: 1148 RVA: 0x0001A7EC File Offset: 0x000189EC
		private void RemoveExpiredInvites()
		{
			DateTime now = DateTime.UtcNow;
			this.currentInvites.RemoveAll((GuildInviteInfo invite) => new DateTime(invite.ExpiresAtTicks, DateTimeKind.Utc) <= now);
		}

		// Token: 0x0600047D RID: 1149 RVA: 0x0001A824 File Offset: 0x00018A24
		private bool OnAcceptClicked()
		{
			if (this.currentInvites.Count == 0)
			{
				return true;
			}
			GuildInviteInfo guildInviteInfo = this.currentInvites[this.currentInviteIndex];
			GuildAcceptInvitePacket packet = new GuildAcceptInvitePacket
			{
				PlayerUid = this.capi.World.Player.PlayerUID
			};
			this.capi.Network.GetChannel("srguildsandkingdoms:guild").SendPacket<GuildAcceptInvitePacket>(packet);
			this.currentInvites.RemoveAt(this.currentInviteIndex);
			if (this.currentInviteIndex >= this.currentInvites.Count && this.currentInviteIndex > 0)
			{
				this.currentInviteIndex--;
			}
			if (this.currentInvites.Count > 0)
			{
				this.ComposeDialog();
			}
			else
			{
				this.TryClose();
			}
			return true;
		}

		// Token: 0x0600047E RID: 1150 RVA: 0x0001A8E8 File Offset: 0x00018AE8
		private bool OnDeclineClicked()
		{
			if (this.currentInvites.Count == 0)
			{
				return true;
			}
			GuildInviteInfo invite = this.currentInvites[this.currentInviteIndex];
			GuildDeclineInvitePacket packet = new GuildDeclineInvitePacket
			{
				PlayerUid = this.capi.World.Player.PlayerUID,
				GuildName = invite.GuildName
			};
			this.capi.Network.GetChannel("srguildsandkingdoms:guild").SendPacket<GuildDeclineInvitePacket>(packet);
			this.currentInvites.RemoveAt(this.currentInviteIndex);
			if (this.currentInviteIndex >= this.currentInvites.Count && this.currentInviteIndex > 0)
			{
				this.currentInviteIndex--;
			}
			if (this.currentInvites.Count > 0)
			{
				this.ComposeDialog();
			}
			else
			{
				this.TryClose();
			}
			return true;
		}

		// Token: 0x0600047F RID: 1151 RVA: 0x0001A9B8 File Offset: 0x00018BB8
		private bool OnPreviousClicked()
		{
			if (this.currentInvites.Count <= 1)
			{
				return true;
			}
			this.currentInviteIndex--;
			if (this.currentInviteIndex < 0)
			{
				this.currentInviteIndex = this.currentInvites.Count - 1;
			}
			this.ComposeDialog();
			return true;
		}

		// Token: 0x06000480 RID: 1152 RVA: 0x0001AA08 File Offset: 0x00018C08
		private bool OnNextClicked()
		{
			if (this.currentInvites.Count <= 1)
			{
				return true;
			}
			this.currentInviteIndex++;
			if (this.currentInviteIndex >= this.currentInvites.Count)
			{
				this.currentInviteIndex = 0;
			}
			this.ComposeDialog();
			return true;
		}

		// Token: 0x06000481 RID: 1153 RVA: 0x0001AA54 File Offset: 0x00018C54
		private void OnCloseClicked()
		{
			this.TryClose();
		}

		// Token: 0x06000482 RID: 1154 RVA: 0x0001AA60 File Offset: 0x00018C60
		private void SetupUpdateListener()
		{
			if (this.updateListenerId != 0L)
			{
				this.capi.Event.UnregisterGameTickListener(this.updateListenerId);
				this.updateListenerId = 0L;
			}
			this.updateListenerId = this.capi.Event.RegisterGameTickListener(delegate(float dt)
			{
				if (!this.IsOpened())
				{
					if (this.updateListenerId != 0L)
					{
						this.capi.Event.UnregisterGameTickListener(this.updateListenerId);
						this.updateListenerId = 0L;
					}
					return;
				}
				this.RemoveExpiredInvites();
				if (this.currentInvites.Count == 0)
				{
					this.TryClose();
					return;
				}
				if (this.currentInviteIndex >= this.currentInvites.Count)
				{
					this.currentInviteIndex = this.currentInvites.Count - 1;
					this.ComposeDialog();
					return;
				}
				if ((DateTime.UtcNow - this.lastComposeTime).TotalSeconds > 30.0)
				{
					this.ComposeDialog();
				}
			}, 10000, 0);
		}

		// Token: 0x06000483 RID: 1155 RVA: 0x0001AABB File Offset: 0x00018CBB
		public override void OnGuiClosed()
		{
			base.OnGuiClosed();
			if (this.updateListenerId != 0L)
			{
				this.capi.Event.UnregisterGameTickListener(this.updateListenerId);
				this.updateListenerId = 0L;
			}
		}

		// Token: 0x06000484 RID: 1156 RVA: 0x0001AAE9 File Offset: 0x00018CE9
		public override void Dispose()
		{
			base.Dispose();
			if (this.updateListenerId != 0L)
			{
				this.capi.Event.UnregisterGameTickListener(this.updateListenerId);
				this.updateListenerId = 0L;
			}
		}

		// Token: 0x040001B9 RID: 441
		private SRGuildsAndKingdomsModSystem modSystem;

		// Token: 0x040001BA RID: 442
		private List<GuildInviteInfo> currentInvites = new List<GuildInviteInfo>();

		// Token: 0x040001BB RID: 443
		private int currentInviteIndex;

		// Token: 0x040001BC RID: 444
		private long updateListenerId;

		// Token: 0x040001BD RID: 445
		private DateTime lastComposeTime = DateTime.MinValue;
	}
}
