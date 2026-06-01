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
	// Token: 0x02000073 RID: 115
	[NullableContext(1)]
	[Nullable(0)]
	internal class DialogGuildInvitePlayer : GuiDialog
	{
		// Token: 0x0600046E RID: 1134 RVA: 0x000199A4 File Offset: 0x00017BA4
		public DialogGuildInvitePlayer(ICoreClientAPI capi, SRGuildsAndKingdomsModSystem modSystem) : base(capi)
		{
			this.modSystem = modSystem;
			this.PopulateAvailablePlayers();
			this.SetupDialog();
		}

		// Token: 0x1700015D RID: 349
		// (get) Token: 0x0600046F RID: 1135 RVA: 0x000199D6 File Offset: 0x00017BD6
		public override string ToggleKeyCombinationCode
		{
			get
			{
				return "guildinvite";
			}
		}

		// Token: 0x06000470 RID: 1136 RVA: 0x000199E0 File Offset: 0x00017BE0
		private void PopulateAvailablePlayers()
		{
			this.availablePlayers.Clear();
			this.availablePlayerNames.Clear();
			IPlayer[] allPlayers = this.capi.World.AllPlayers;
			string currentPlayerUid = this.capi.World.Player.PlayerUID;
			List<GuildSummary> clientGuildSummaries = this.modSystem.GetClientGuildSummaries();
			HashSet<string> playersInGuilds = new HashSet<string>();
			HashSet<string> playersWithPendingInvites = new HashSet<string>();
			foreach (GuildSummary guild in clientGuildSummaries)
			{
				foreach (string memberUid in guild.MemberUids)
				{
					playersInGuilds.Add(memberUid);
				}
				foreach (string pendingInviteUid in guild.PendingInviteUids)
				{
					playersWithPendingInvites.Add(pendingInviteUid);
				}
			}
			foreach (IPlayer player in allPlayers)
			{
				if (player.PlayerUID != currentPlayerUid && !playersInGuilds.Contains(player.PlayerUID) && !playersWithPendingInvites.Contains(player.PlayerUID))
				{
					this.availablePlayers.Add(player);
				}
			}
			this.availablePlayers = (from p in this.availablePlayers
			orderby p.PlayerName
			select p).ToList<IPlayer>();
			this.availablePlayerNames.Add("Select a player...");
			foreach (IPlayer player2 in this.availablePlayers)
			{
				this.availablePlayerNames.Add(player2.PlayerName);
			}
		}

		// Token: 0x06000471 RID: 1137 RVA: 0x00019BF8 File Offset: 0x00017DF8
		private void SetupDialog()
		{
			ElementBounds dialogBounds = ElementStdBounds.AutosizedMainDialog.WithAlignment(6);
			ElementBounds bgBounds = ElementBounds.Fill.WithFixedPadding(GuiStyle.ElementToDialogPadding);
			bgBounds.BothSizing = 2;
			GuiComposer composer = GuiComposerHelpers.AddDialogTitleBar(GuiComposerHelpers.AddShadedDialogBG(this.capi.Gui.CreateCompo("guildinvite", dialogBounds), bgBounds, true, 5.0, 0.75f), Lang.Get("srguildsandkingdoms:invite-player-title", Array.Empty<object>()), new Action(this.OnTitleBarCloseClicked), null, null, null).BeginChildElements(bgBounds);
			double top = 20.0;
			double spacing = 10.0;
			double elementHeight = 25.0;
			string instructionText = (this.availablePlayerNames.Count > 1) ? Lang.Get("srguildsandkingdoms:invite-player-instructions", Array.Empty<object>()) : "No players available to invite. All online players are either already in guilds or it's just you online.";
			GuiComposerHelpers.AddStaticText(composer, instructionText, CairoFont.WhiteSmallText(), ElementBounds.Fixed(0.0, top, 350.0, elementHeight * 2.0), null);
			top += elementHeight * 2.0 + spacing;
			if (this.availablePlayerNames.Count > 1)
			{
				GuiComposerHelpers.AddStaticText(composer, Lang.Get("srguildsandkingdoms:player-name", Array.Empty<object>()), CairoFont.WhiteSmallText(), ElementBounds.Fixed(0.0, top, 100.0, elementHeight), null);
				GuiComposerHelpers.AddDropDown(composer, this.availablePlayerNames.ToArray(), this.availablePlayerNames.ToArray(), 0, new SelectionChangedDelegate(this.OnPlayerSelected), ElementBounds.Fixed(110.0, top, 200.0, elementHeight), "playerdropdown");
				top += elementHeight + spacing * 1.5;
				GuiComposerHelpers.AddSmallButton(composer, Lang.Get("srguildsandkingdoms:invite", Array.Empty<object>()), new ActionConsumable(this.OnInviteClick), ElementBounds.Fixed(0.0, top, 80.0, elementHeight), 2, null);
				GuiComposerHelpers.AddSmallButton(composer, Lang.Get("srguildsandkingdoms:cancel", Array.Empty<object>()), new ActionConsumable(this.OnCancelClick), ElementBounds.Fixed(90.0, top, 80.0, elementHeight), 2, null);
			}
			else
			{
				GuiComposerHelpers.AddSmallButton(composer, Lang.Get("srguildsandkingdoms:close", Array.Empty<object>()), new ActionConsumable(this.OnCancelClick), ElementBounds.Fixed(0.0, top, 80.0, elementHeight), 2, null);
			}
			base.SingleComposer = composer.Compose(true);
		}

		// Token: 0x06000472 RID: 1138 RVA: 0x00019E73 File Offset: 0x00018073
		private void OnPlayerSelected(string code, bool selected)
		{
		}

		// Token: 0x06000473 RID: 1139 RVA: 0x00019E78 File Offset: 0x00018078
		private bool OnInviteClick()
		{
			if (this.availablePlayers.Count == 0)
			{
				return true;
			}
			int selectedIndex = GuiComposerHelpers.GetDropDown(base.SingleComposer, "playerdropdown").SelectedIndices[0];
			if (selectedIndex <= 0)
			{
				this.capi.ShowChatMessage(Lang.Get("srguildsandkingdoms:please-select-player", Array.Empty<object>()));
				return true;
			}
			int playerIndex = selectedIndex - 1;
			IPlayer selectedPlayer = this.availablePlayers[playerIndex];
			GuildNetworkHandler networkHandler = this.modSystem.GetNetworkHandler();
			if (networkHandler != null)
			{
				networkHandler.SendGuildInviteRequest(selectedPlayer.PlayerUID);
				this.capi.ShowChatMessage(Lang.Get("srguildsandkingdoms:invite-sent", new object[]
				{
					selectedPlayer.PlayerName
				}));
			}
			this.TryClose();
			return true;
		}

		// Token: 0x06000474 RID: 1140 RVA: 0x00019F24 File Offset: 0x00018124
		private bool OnCancelClick()
		{
			this.TryClose();
			return true;
		}

		// Token: 0x06000475 RID: 1141 RVA: 0x00019F2E File Offset: 0x0001812E
		private void OnTitleBarCloseClicked()
		{
			this.TryClose();
		}

		// Token: 0x1700015E RID: 350
		// (get) Token: 0x06000476 RID: 1142 RVA: 0x00019F37 File Offset: 0x00018137
		public override bool PrefersUngrabbedMouse
		{
			get
			{
				return false;
			}
		}

		// Token: 0x040001B6 RID: 438
		private SRGuildsAndKingdomsModSystem modSystem;

		// Token: 0x040001B7 RID: 439
		private List<string> availablePlayerNames = new List<string>();

		// Token: 0x040001B8 RID: 440
		private List<IPlayer> availablePlayers = new List<IPlayer>();
	}
}
