using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using SRGuildsAndKingdoms.src.guilds;
using SRGuildsAndKingdoms.src.network;
using Vintagestory.API.Client;
using Vintagestory.API.Common;

namespace SRGuildsAndKingdoms.src.gui
{
	// Token: 0x02000081 RID: 129
	[NullableContext(1)]
	[Nullable(0)]
	public class DialogTransferOwnership : GuiDialog
	{
		// Token: 0x17000188 RID: 392
		// (get) Token: 0x060005A0 RID: 1440 RVA: 0x000261CC File Offset: 0x000243CC
		public override string ToggleKeyCombinationCode
		{
			get
			{
				return "transferownership";
			}
		}

		// Token: 0x060005A1 RID: 1441 RVA: 0x000261D4 File Offset: 0x000243D4
		public DialogTransferOwnership(ICoreClientAPI capi, SRGuildsAndKingdomsModSystem modSystem, GuildSummary currentGuild, Action onTransferComplete) : base(capi)
		{
			this.modSystem = modSystem;
			this.currentGuild = currentGuild;
			this.onTransferComplete = onTransferComplete;
			this.members = new List<GuildMemberInfo>();
			GuildNetworkHandler networkHandler = modSystem.GetNetworkHandler();
			networkHandler.RegisterMemberListCallback(new Action<List<GuildMemberInfo>>(this.OnMemberListReceived));
			networkHandler.SendGuildMemberListRequest();
		}

		// Token: 0x060005A2 RID: 1442 RVA: 0x00026228 File Offset: 0x00024428
		private void OnMemberListReceived(List<GuildMemberInfo> memberList)
		{
			this.members = (from m in memberList
			where m.PlayerUid != this.capi.World.Player.PlayerUID && m.Role != "Leader"
			select m).ToList<GuildMemberInfo>();
			if (this.members.Count > 0)
			{
				this.selectedMemberUid = this.members[0].PlayerUid;
			}
			this.SetupDialog();
		}

		// Token: 0x060005A3 RID: 1443 RVA: 0x00026280 File Offset: 0x00024480
		private void SetupDialog()
		{
			ElementBounds dialogBounds = ElementStdBounds.AutosizedMainDialog.WithAlignment(6);
			ElementBounds bgBounds = ElementBounds.Fill.WithFixedPadding(GuiStyle.ElementToDialogPadding);
			bgBounds.BothSizing = 2;
			GuiComposer composer = GuiComposerHelpers.AddDialogTitleBar(GuiComposerHelpers.AddShadedDialogBG(this.capi.Gui.CreateCompo("transferownership", dialogBounds), bgBounds, true, 5.0, 0.75f), "Transfer Guild Ownership", new Action(this.OnTitleBarCloseClicked), null, null, null).BeginChildElements(bgBounds);
			double top = 20.0;
			double spacing = 10.0;
			double elementHeight = 25.0;
			if (this.members.Count == 0)
			{
				GuiComposerHelpers.AddStaticText(composer, "No eligible members to transfer ownership to.", CairoFont.WhiteDetailText(), ElementBounds.Fixed(0.0, top, 400.0, 50.0), null);
				GuiComposerHelpers.AddSmallButton(composer, "Close", new ActionConsumable(this.OnCancel), ElementBounds.Fixed(0.0, top + 60.0, 100.0, elementHeight), 2, null);
			}
			else
			{
				GuiComposerHelpers.AddStaticText(composer, "Select the member to transfer guild leadership to:", CairoFont.WhiteDetailText(), ElementBounds.Fixed(0.0, top, 400.0, elementHeight), null);
				top += elementHeight + spacing;
				string[] memberNames = (from m in this.members
				select m.PlayerName).ToArray<string>();
				GuiComposerHelpers.AddStaticText(composer, "Member:", CairoFont.WhiteSmallText(), ElementBounds.Fixed(0.0, top, 100.0, elementHeight), null);
				GuiComposerHelpers.AddDropDown(composer, memberNames, memberNames, 0, new SelectionChangedDelegate(this.OnMemberSelected), ElementBounds.Fixed(110.0, top, 250.0, elementHeight), "memberDropdown");
				top += elementHeight + spacing * 2.0;
				GuiComposerHelpers.AddStaticText(composer, "Warning: This action cannot be undone!", CairoFont.WhiteSmallText().WithColor(new double[]
				{
					1.0,
					0.5,
					0.0,
					1.0
				}), ElementBounds.Fixed(0.0, top, 400.0, elementHeight), null);
				top += elementHeight + spacing;
				GuiComposerHelpers.AddStaticText(composer, "You will become a regular member after transferring.", CairoFont.WhiteSmallText(), ElementBounds.Fixed(0.0, top, 400.0, elementHeight), null);
				top += elementHeight + spacing * 2.0;
				GuiComposerHelpers.AddSmallButton(composer, "Transfer", new ActionConsumable(this.OnTransfer), ElementBounds.Fixed(0.0, top, 100.0, elementHeight), 2, null);
				GuiComposerHelpers.AddSmallButton(composer, "Cancel", new ActionConsumable(this.OnCancel), ElementBounds.Fixed(110.0, top, 100.0, elementHeight), 2, null);
			}
			base.SingleComposer = composer.Compose(true);
		}

		// Token: 0x060005A4 RID: 1444 RVA: 0x00026578 File Offset: 0x00024778
		private void OnMemberSelected(string code, bool selected)
		{
			GuildMemberInfo selectedMember = this.members.FirstOrDefault((GuildMemberInfo m) => m.PlayerName == code);
			if (selectedMember != null)
			{
				this.selectedMemberUid = selectedMember.PlayerUid;
			}
		}

		// Token: 0x060005A5 RID: 1445 RVA: 0x000265B9 File Offset: 0x000247B9
		private bool OnTransfer()
		{
			if (string.IsNullOrEmpty(this.selectedMemberUid))
			{
				return true;
			}
			this.modSystem.GetNetworkHandler().SendGuildTransferOwnershipRequest(this.selectedMemberUid);
			this.TryClose();
			Action action = this.onTransferComplete;
			if (action != null)
			{
				action();
			}
			return true;
		}

		// Token: 0x060005A6 RID: 1446 RVA: 0x000265F9 File Offset: 0x000247F9
		private bool OnCancel()
		{
			this.TryClose();
			return true;
		}

		// Token: 0x060005A7 RID: 1447 RVA: 0x00026603 File Offset: 0x00024803
		private void OnTitleBarCloseClicked()
		{
			this.TryClose();
		}

		// Token: 0x060005A8 RID: 1448 RVA: 0x0002660C File Offset: 0x0002480C
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

		// Token: 0x04000233 RID: 563
		private SRGuildsAndKingdomsModSystem modSystem;

		// Token: 0x04000234 RID: 564
		private GuildSummary currentGuild;

		// Token: 0x04000235 RID: 565
		private List<GuildMemberInfo> members;

		// Token: 0x04000236 RID: 566
		[Nullable(2)]
		private string selectedMemberUid;

		// Token: 0x04000237 RID: 567
		private Action onTransferComplete;
	}
}
