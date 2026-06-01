using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using SRGuildsAndKingdoms.src.gui.tabs;
using SRGuildsAndKingdoms.src.guilds;
using SRGuildsAndKingdoms.src.network;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;

namespace SRGuildsAndKingdoms.src.gui
{
	// Token: 0x02000075 RID: 117
	[NullableContext(1)]
	[Nullable(0)]
	public class DialogGuildMain : GuiDialog
	{
		// Token: 0x17000160 RID: 352
		// (get) Token: 0x06000486 RID: 1158 RVA: 0x0001ABC5 File Offset: 0x00018DC5
		public override float ZSize
		{
			get
			{
				return 1000f;
			}
		}

		// Token: 0x06000487 RID: 1159 RVA: 0x0001ABCC File Offset: 0x00018DCC
		public DialogGuildMain(ICoreClientAPI capi, SRGuildsAndKingdomsModSystem modSystem) : base(capi)
		{
			this.modSystem = modSystem;
			modSystem.OnClientGuildDataUpdated += this.OnGuildDataUpdated;
			GuildNetworkHandler networkHandler = modSystem.GetNetworkHandler();
			if (networkHandler != null)
			{
				networkHandler.RegisterNodeWarDataCallback(new Action<NodeWarDataResponsePacket>(this.OnNodeWarDataReceived));
			}
			this.SetupDialog();
			PlotMapLayer plotLayer = modSystem.GetPlotLayer();
			if (plotLayer == null)
			{
				return;
			}
			plotLayer.SetActiveGuildDialog(this);
		}

		// Token: 0x17000161 RID: 353
		// (get) Token: 0x06000488 RID: 1160 RVA: 0x0001AC43 File Offset: 0x00018E43
		public override string ToggleKeyCombinationCode
		{
			get
			{
				return "guildmain";
			}
		}

		// Token: 0x06000489 RID: 1161 RVA: 0x0001AC4C File Offset: 0x00018E4C
		private void SetupDialog()
		{
			this.currentGuild = this.modSystem.GetCurrentPlayerGuildSummary();
			if (this.currentGuild == null)
			{
				this.SetupNoGuildDialog();
				return;
			}
			if (this.pendingGuildName == null)
			{
				this.pendingGuildName = this.currentGuild.Name;
			}
			if (this.pendingDescription == null)
			{
				this.pendingDescription = this.currentGuild.Description;
			}
			if (this.pendingPrimaryColor == null)
			{
				this.pendingPrimaryColor = this.ColorToHex(this.currentGuild.DisplayColor);
			}
			if (this.pendingSecondaryColor == null)
			{
				this.pendingSecondaryColor = this.ColorToHex(this.currentGuild.SecondaryColor);
			}
			this.InitializeTabContents();
			ElementBounds dialogBounds = ElementStdBounds.AutosizedMainDialog.WithAlignment(6);
			ElementBounds bgBounds = ElementBounds.Fill.WithFixedPadding(GuiStyle.ElementToDialogPadding);
			bgBounds.BothSizing = 2;
			GuiComposer composer = GuiComposerHelpers.AddDialogTitleBar(GuiComposerHelpers.AddShadedDialogBG(this.capi.Gui.CreateCompo("guildmain", dialogBounds), bgBounds, true, 5.0, 0.75f), Lang.Get("srguildsandkingdoms:guild-main-title", new object[]
			{
				this.currentGuild.Name
			}), new Action(this.OnTitleBarCloseClicked), null, null, null).BeginChildElements(bgBounds);
			double top = 20.0;
			double tabHeight = 30.0;
			double tabWidth = 120.0;
			double contentTop = top + tabHeight + 10.0;
			GuiComposerHelpers.AddSmallButton(composer, "Overview", new ActionConsumable(this.OnOverviewTab), ElementBounds.Fixed(0.0, top, tabWidth, tabHeight), (this.currentTab == 0) ? 1 : 2, null);
			GuiComposerHelpers.AddSmallButton(composer, "Members", new ActionConsumable(this.OnMembersTab), ElementBounds.Fixed(tabWidth + 5.0, top, tabWidth, tabHeight), (this.currentTab == 1) ? 1 : 2, null);
			GuiComposerHelpers.AddSmallButton(composer, "Lands", new ActionConsumable(this.OnLandsTab), ElementBounds.Fixed((tabWidth + 5.0) * 2.0, top, tabWidth, tabHeight), (this.currentTab == 2) ? 1 : 2, null);
			GuiComposerHelpers.AddSmallButton(composer, "Research", new ActionConsumable(this.OnResearchTab), ElementBounds.Fixed((tabWidth + 5.0) * 3.0, top, tabWidth, tabHeight), (this.currentTab == 3) ? 1 : 2, null);
			GuiComposerHelpers.AddSmallButton(composer, "Node Wars", new ActionConsumable(this.OnNodeWarsTab), ElementBounds.Fixed((tabWidth + 5.0) * 4.0, top, tabWidth, tabHeight), (this.currentTab == 4) ? 1 : 2, null);
			if (this.HasManagePermissions())
			{
				GuiComposerHelpers.AddSmallButton(composer, "Settings", new ActionConsumable(this.OnSettingsTab), ElementBounds.Fixed((tabWidth + 5.0) * 5.0, top, tabWidth, tabHeight), (this.currentTab == 5) ? 1 : 2, null);
			}
			switch (this.currentTab)
			{
			case 0:
			{
				GuildOverviewTab guildOverviewTab = this.overviewTab;
				if (guildOverviewTab != null)
				{
					guildOverviewTab.AddContent(composer, contentTop);
				}
				break;
			}
			case 1:
			{
				GuildMembersTab guildMembersTab = this.membersTab;
				if (guildMembersTab != null)
				{
					guildMembersTab.AddContent(composer, contentTop);
				}
				break;
			}
			case 2:
			{
				GuildLandsTab guildLandsTab = this.landsTab;
				if (guildLandsTab != null)
				{
					guildLandsTab.AddContent(composer, contentTop);
				}
				break;
			}
			case 3:
			{
				GuildResearch guildResearch = this.researchTab;
				if (guildResearch != null)
				{
					guildResearch.AddContent(composer, contentTop);
				}
				break;
			}
			case 4:
			{
				GuildNodeWarsTab guildNodeWarsTab = this.nodeWarsTab;
				if (guildNodeWarsTab != null)
				{
					guildNodeWarsTab.AddContent(composer, contentTop);
				}
				break;
			}
			case 5:
			{
				GuildSettingsTab guildSettingsTab = this.settingsTab;
				if (guildSettingsTab != null)
				{
					guildSettingsTab.AddContent(composer, contentTop);
				}
				break;
			}
			}
			base.SingleComposer = composer.Compose(true);
		}

		// Token: 0x0600048A RID: 1162 RVA: 0x0001AFFC File Offset: 0x000191FC
		private void RefreshNodeWarsData()
		{
			if (this.currentGuild == null || this.nodeWarsTab == null)
			{
				return;
			}
			GuildNetworkHandler networkHandler = this.modSystem.GetNetworkHandler();
			if (networkHandler != null)
			{
				networkHandler.RequestNodeWarData(this.currentGuild.Name);
			}
			this.capi.Logger.Notification("[Guild UI] Requested node wars data for guild: " + this.currentGuild.Name);
		}

		// Token: 0x0600048B RID: 1163 RVA: 0x0001B060 File Offset: 0x00019260
		private void InitializeTabContents()
		{
			if (this.researchTab != null)
			{
				this.researchTab.Dispose();
				this.researchTab = null;
			}
			this.overviewTab = new GuildOverviewTab(this.capi, this.modSystem, this.currentGuild, () => this.OnLeaveGuild());
			this.membersTab = new GuildMembersTab(this.capi, this.modSystem, this.currentGuild, () => this.OnViewMembers(), () => this.OnInvitePlayer(), () => this.OnManageRoles(), () => this.OnManagePendingInvites());
			this.landsTab = new GuildLandsTab(this.capi, this.modSystem, this.currentGuild, () => this.OnClaimingToggle(), () => this.OnSavePendingClaims(), () => this.OnShowHologram(), () => this.OnCreateOutpost(), () => this.OnUnclaimingToggle(), () => this.OnSavePendingUnclaims(), () => this.OnClaimCurrentChunk(), () => this.OnUnclaimCurrentChunk());
			this.landsTab.SetClaimingMode(this.isClaimingMode);
			this.landsTab.SetUnclaimingMode(this.isUnclaimingMode);
			this.landsTab.SetPendingClaims(this.pendingClaims);
			this.landsTab.SetPendingUnclaims(this.pendingUnclaims);
			this.researchTab = new GuildResearch(this.capi, this.modSystem, this.currentGuild, delegate()
			{
				this.SetupDialog();
				return true;
			});
			this.nodeWarsTab = new GuildNodeWarsTab(this.capi, this.modSystem, this.currentGuild, () => this.OnNodeWarSignup(), () => this.OnNodeWarCancelSignup(), () => this.OnNodeWarJoin(), () => this.OnNodeWarViewDetails());
			if (this.cachedNodeWarData != null)
			{
				this.nodeWarsTab.SetNodeWarData(this.cachedNodeWarData);
			}
			this.settingsTab = new GuildSettingsTab(this.capi, this.modSystem, this.currentGuild, new Action<string>(this.OnGuildNameChanged), new Action<string>(this.OnDescriptionChanged), new Action<string>(this.OnPrimaryColorChanged), new Action<string>(this.OnSecondaryColorChanged), () => this.OnSaveSettings(), () => this.OnCloseDialog());
			this.settingsTab.SetPendingValues(this.pendingGuildName, this.pendingDescription, this.pendingPrimaryColor, this.pendingSecondaryColor);
		}

		// Token: 0x0600048C RID: 1164 RVA: 0x0001B2D5 File Offset: 0x000194D5
		private bool OnOverviewTab()
		{
			this.currentTab = 0;
			this.SetupDialog();
			return true;
		}

		// Token: 0x0600048D RID: 1165 RVA: 0x0001B2E5 File Offset: 0x000194E5
		private bool OnMembersTab()
		{
			this.currentTab = 1;
			this.SetupDialog();
			return true;
		}

		// Token: 0x0600048E RID: 1166 RVA: 0x0001B2F5 File Offset: 0x000194F5
		private bool OnLandsTab()
		{
			this.currentTab = 2;
			this.SetupDialog();
			return true;
		}

		// Token: 0x0600048F RID: 1167 RVA: 0x0001B305 File Offset: 0x00019505
		private bool OnResearchTab()
		{
			this.currentTab = 3;
			this.SetupDialog();
			return true;
		}

		// Token: 0x06000490 RID: 1168 RVA: 0x0001B315 File Offset: 0x00019515
		private bool OnNodeWarsTab()
		{
			this.currentTab = 4;
			this.RefreshNodeWarsData();
			this.SetupDialog();
			return true;
		}

		// Token: 0x06000491 RID: 1169 RVA: 0x0001B32B File Offset: 0x0001952B
		private bool OnSettingsTab()
		{
			if (this.HasManagePermissions())
			{
				this.currentTab = 5;
				this.SetupDialog();
			}
			return true;
		}

		// Token: 0x06000492 RID: 1170 RVA: 0x0001B344 File Offset: 0x00019544
		private void SetupNoGuildDialog()
		{
			ElementBounds dialogBounds = ElementStdBounds.AutosizedMainDialog.WithAlignment(6);
			ElementBounds bgBounds = ElementBounds.Fill.WithFixedPadding(GuiStyle.ElementToDialogPadding);
			bgBounds.BothSizing = 2;
			GuiComposer composer = GuiComposerHelpers.AddDialogTitleBar(GuiComposerHelpers.AddShadedDialogBG(this.capi.Gui.CreateCompo("noguild", dialogBounds), bgBounds, true, 5.0, 0.75f), Lang.Get("srguildsandkingdoms:no-guild-title", Array.Empty<object>()), new Action(this.OnTitleBarCloseClicked), null, null, null).BeginChildElements(bgBounds);
			GuiComposerHelpers.AddStaticText(composer, Lang.Get("srguildsandkingdoms:no-guild-message", Array.Empty<object>()), CairoFont.WhiteDetailText(), ElementBounds.Fixed(0.0, 20.0, 400.0, 50.0), null);
			GuiComposerHelpers.AddSmallButton(composer, Lang.Get("srguildsandkingdoms:create-guild", Array.Empty<object>()), new ActionConsumable(this.OnCreateGuild), ElementBounds.Fixed(0.0, 60.0, 120.0, 25.0), 2, null);
			base.SingleComposer = composer.Compose(true);
		}

		// Token: 0x06000493 RID: 1171 RVA: 0x0001B468 File Offset: 0x00019668
		private bool HasManagePermissions()
		{
			GuildSummary guildSummary = this.currentGuild;
			return ((guildSummary != null) ? guildSummary.PlayerRole : null) == "Leader";
		}

		// Token: 0x06000494 RID: 1172 RVA: 0x0001B486 File Offset: 0x00019686
		private void OnGuildNameChanged(string newName)
		{
			if (this.currentGuild == null)
			{
				return;
			}
			this.pendingGuildName = newName;
			this.CheckForPendingChanges();
		}

		// Token: 0x06000495 RID: 1173 RVA: 0x0001B49E File Offset: 0x0001969E
		private void OnDescriptionChanged(string newDescription)
		{
			if (this.currentGuild == null)
			{
				return;
			}
			this.pendingDescription = newDescription;
			this.CheckForPendingChanges();
		}

		// Token: 0x06000496 RID: 1174 RVA: 0x0001B4B6 File Offset: 0x000196B6
		private void OnPrimaryColorChanged(string hexColor)
		{
			if (this.currentGuild == null)
			{
				return;
			}
			this.pendingPrimaryColor = hexColor;
			this.CheckForPendingChanges();
		}

		// Token: 0x06000497 RID: 1175 RVA: 0x0001B4CE File Offset: 0x000196CE
		private void OnSecondaryColorChanged(string hexColor)
		{
			if (this.currentGuild == null)
			{
				return;
			}
			this.pendingSecondaryColor = hexColor;
			this.CheckForPendingChanges();
		}

		// Token: 0x06000498 RID: 1176 RVA: 0x0001B4E8 File Offset: 0x000196E8
		private void CheckForPendingChanges()
		{
			if (this.currentGuild == null)
			{
				return;
			}
			bool nameChanged = this.pendingGuildName != this.currentGuild.Name;
			bool descriptionChanged = this.pendingDescription != this.currentGuild.Description;
			bool primaryChanged = this.pendingPrimaryColor != this.ColorToHex(this.currentGuild.DisplayColor);
			bool secondaryChanged = this.pendingSecondaryColor != this.ColorToHex(this.currentGuild.SecondaryColor);
			bool flag = this.hasPendingChanges;
			this.hasPendingChanges = (nameChanged || descriptionChanged || primaryChanged || secondaryChanged);
			GuildSettingsTab guildSettingsTab = this.settingsTab;
			if (guildSettingsTab != null)
			{
				guildSettingsTab.SetPendingValues(this.pendingGuildName, this.pendingDescription, this.pendingPrimaryColor, this.pendingSecondaryColor);
			}
			GuildLandsTab guildLandsTab = this.landsTab;
			if (guildLandsTab == null)
			{
				return;
			}
			guildLandsTab.SetPendingClaims(this.pendingClaims);
		}

		// Token: 0x06000499 RID: 1177 RVA: 0x0001B5BC File Offset: 0x000197BC
		private bool OnClaimingToggle()
		{
			if (this.isUnclaimingMode)
			{
				this.isUnclaimingMode = false;
				GuildLandsTab guildLandsTab = this.landsTab;
				if (guildLandsTab != null)
				{
					guildLandsTab.SetUnclaimingMode(false);
				}
			}
			this.isClaimingMode = !this.isClaimingMode;
			if (this.isClaimingMode)
			{
				GuildSummary guildSummary = this.currentGuild;
				if (guildSummary != null && guildSummary.Claims.Count == 0)
				{
					this.capi.ShowChatMessage(Lang.Get("srguildsandkingdoms:guild-home-mode-enabled-map", Array.Empty<object>()));
				}
				else
				{
					this.capi.ShowChatMessage(Lang.Get("srguildsandkingdoms:claiming-mode-enabled-map", Array.Empty<object>()));
				}
			}
			else
			{
				this.capi.ShowChatMessage(Lang.Get("srguildsandkingdoms:claiming-mode-disabled", Array.Empty<object>()));
			}
			GuildLandsTab guildLandsTab2 = this.landsTab;
			if (guildLandsTab2 != null)
			{
				guildLandsTab2.SetClaimingMode(this.isClaimingMode);
			}
			this.SetupDialog();
			return true;
		}

		// Token: 0x0600049A RID: 1178 RVA: 0x0001B690 File Offset: 0x00019890
		private bool OnUnclaimingToggle()
		{
			if (this.isClaimingMode)
			{
				this.isClaimingMode = false;
				GuildLandsTab guildLandsTab = this.landsTab;
				if (guildLandsTab != null)
				{
					guildLandsTab.SetClaimingMode(false);
				}
			}
			this.isUnclaimingMode = !this.isUnclaimingMode;
			if (this.isUnclaimingMode)
			{
				this.capi.ShowChatMessage(Lang.Get("srguildsandkingdoms:unclaiming-mode-enabled", Array.Empty<object>()));
			}
			else
			{
				this.capi.ShowChatMessage(Lang.Get("srguildsandkingdoms:unclaiming-mode-disabled", Array.Empty<object>()));
			}
			GuildLandsTab guildLandsTab2 = this.landsTab;
			if (guildLandsTab2 != null)
			{
				guildLandsTab2.SetUnclaimingMode(this.isUnclaimingMode);
			}
			this.SetupDialog();
			return true;
		}

		// Token: 0x0600049B RID: 1179 RVA: 0x0001B72C File Offset: 0x0001992C
		private bool OnClaimCurrentChunk()
		{
			if (this.currentGuild == null)
			{
				return true;
			}
			BlockPos asBlockPos = this.capi.World.Player.Entity.Pos.AsBlockPos;
			int chunkSize = 32;
			int chunkX = LandClaim.FloorDiv(asBlockPos.X, chunkSize);
			int chunkZ = LandClaim.FloorDiv(asBlockPos.Z, chunkSize);
			this.ProcessChunkClaim(chunkX, chunkZ, true);
			return true;
		}

		// Token: 0x0600049C RID: 1180 RVA: 0x0001B788 File Offset: 0x00019988
		private bool OnUnclaimCurrentChunk()
		{
			if (this.currentGuild == null)
			{
				return true;
			}
			BlockPos playerPos = this.capi.World.Player.Entity.Pos.AsBlockPos;
			int chunkSize = 32;
			int chunkX = LandClaim.FloorDiv(playerPos.X, chunkSize);
			int chunkZ = LandClaim.FloorDiv(playerPos.Z, chunkSize);
			if (this.currentGuild.Claims.FirstOrDefault((LandClaimDto c) => c.ChunkX == chunkX && c.ChunkZ == chunkZ) == null)
			{
				this.capi.ShowChatMessage(Lang.Get("srguildsandkingdoms:chunk-not-claimed", Array.Empty<object>()));
				return true;
			}
			GuildNetworkHandler networkHandler = this.modSystem.GetNetworkHandler();
			if (networkHandler == null)
			{
				this.capi.ShowChatMessage(Lang.Get("srguildsandkingdoms:network-error", Array.Empty<object>()));
				return true;
			}
			networkHandler.SendGuildUnclaimLandRequest(chunkX, chunkZ);
			this.capi.ShowChatMessage(Lang.Get("srguildsandkingdoms:unclaim-submitted", new object[]
			{
				chunkX,
				chunkZ
			}));
			return true;
		}

		// Token: 0x0600049D RID: 1181 RVA: 0x0001B89B File Offset: 0x00019A9B
		public void OnMapChunkClaimed(int chunkX, int chunkZ)
		{
			if (!this.isClaimingMode || this.currentGuild == null)
			{
				return;
			}
			this.ProcessChunkClaim(chunkX, chunkZ, false);
		}

		// Token: 0x0600049E RID: 1182 RVA: 0x0001B8B8 File Offset: 0x00019AB8
		private void ProcessChunkClaim(int chunkX, int chunkZ, bool sendImmediately)
		{
			if (this.currentGuild == null)
			{
				return;
			}
			if (this.currentGuild.Claims.Any((LandClaimDto c) => c.ChunkX == chunkX && c.ChunkZ == chunkZ))
			{
				this.capi.ShowChatMessage(Lang.Get("srguildsandkingdoms:chunk-already-claimed", Array.Empty<object>()));
				return;
			}
			if (!sendImmediately && this.pendingClaims.Any(([TupleElementNames(new string[]
			{
				"chunkX",
				"chunkZ"
			})] ValueTuple<int, int> p) => p.Item1 == chunkX && p.Item2 == chunkZ))
			{
				this.capi.ShowChatMessage(Lang.Get("srguildsandkingdoms:chunk-already-pending", Array.Empty<object>()));
				return;
			}
			List<GuildSummary> allGuilds = this.modSystem.GetClientGuildSummaries();
			if (!this.currentGuild.Claims.Any((LandClaimDto c) => c.IsGuildHome) && (sendImmediately || this.pendingClaims.Count == 0))
			{
				List<ValueTuple<int, int>> homeChunks = new List<ValueTuple<int, int>>
				{
					new ValueTuple<int, int>(chunkX, chunkZ),
					new ValueTuple<int, int>(chunkX + 1, chunkZ),
					new ValueTuple<int, int>(chunkX, chunkZ + 1),
					new ValueTuple<int, int>(chunkX + 1, chunkZ + 1)
				};
				if (homeChunks.Any(delegate(ValueTuple<int, int> homeChunk)
				{
					Func<LandClaimDto, bool> <>9__5;
					return allGuilds.Any(delegate(GuildSummary guild)
					{
						IEnumerable<LandClaimDto> claims = guild.Claims;
						Func<LandClaimDto, bool> predicate;
						if ((predicate = <>9__5) == null)
						{
							predicate = (<>9__5 = ((LandClaimDto claim) => claim.ChunkX == homeChunk.Item1 && claim.ChunkZ == homeChunk.Item2));
						}
						return claims.Any(predicate);
					});
				}))
				{
					this.capi.ShowChatMessage(Lang.Get("srguildsandkingdoms:guild-home-area-blocked", Array.Empty<object>()));
					return;
				}
				if (sendImmediately)
				{
					GuildNetworkHandler networkHandler = this.modSystem.GetNetworkHandler();
					if (networkHandler == null)
					{
						this.capi.ShowChatMessage(Lang.Get("srguildsandkingdoms:network-error", Array.Empty<object>()));
						return;
					}
					networkHandler.SendGuildClaimLandRequest(chunkX, chunkZ);
					this.capi.ShowChatMessage(Lang.Get("srguildsandkingdoms:guild-home-claim-submitted", new object[]
					{
						chunkX,
						chunkZ
					}));
				}
				else
				{
					this.pendingClaims.AddRange(homeChunks);
					this.capi.ShowChatMessage(Lang.Get("srguildsandkingdoms:guild-home-area-added-to-pending", new object[]
					{
						chunkX,
						chunkZ
					}));
				}
			}
			else
			{
				List<LandClaimDto> nonOutpostClaims = (from c in this.currentGuild.Claims
				where !c.IsOutpost
				select c).ToList<LandClaimDto>();
				bool flag = this.IsChunkAdjacentToAnyClaim(nonOutpostClaims, chunkX, chunkZ);
				bool adjacentToPending = !sendImmediately && this.pendingClaims.Any(([TupleElementNames(new string[]
				{
					"chunkX",
					"chunkZ"
				})] ValueTuple<int, int> p) => this.IsAdjacent(p.Item1, p.Item2, chunkX, chunkZ));
				if (!flag && !adjacentToPending)
				{
					this.capi.ShowChatMessage(Lang.Get("srguildsandkingdoms:claim-must-be-adjacent", Array.Empty<object>()));
					return;
				}
				LandClaimDto adjacentClaim = this.currentGuild.Claims.FirstOrDefault((LandClaimDto c) => this.IsAdjacent(c.ChunkX, c.ChunkZ, chunkX, chunkZ));
				if (adjacentClaim != null && adjacentClaim.IsOutpost)
				{
					this.capi.ShowChatMessage(Lang.Get("srguildsandkingdoms:cannot-expand-from-outpost", Array.Empty<object>()));
					return;
				}
				Func<LandClaimDto, bool> <>9__10;
				if (sendImmediately && allGuilds.Any(delegate(GuildSummary guild)
				{
					IEnumerable<LandClaimDto> claims = guild.Claims;
					Func<LandClaimDto, bool> predicate;
					if ((predicate = <>9__10) == null)
					{
						predicate = (<>9__10 = ((LandClaimDto claim) => claim.ChunkX == chunkX && claim.ChunkZ == chunkZ));
					}
					return claims.Any(predicate);
				}))
				{
					this.capi.ShowChatMessage(Lang.Get("srguildsandkingdoms:chunk-already-claimed-by-other", Array.Empty<object>()));
					return;
				}
				if (sendImmediately)
				{
					GuildNetworkHandler networkHandler2 = this.modSystem.GetNetworkHandler();
					if (networkHandler2 == null)
					{
						this.capi.ShowChatMessage(Lang.Get("srguildsandkingdoms:network-error", Array.Empty<object>()));
						return;
					}
					networkHandler2.SendGuildClaimLandRequest(chunkX, chunkZ);
					this.capi.ShowChatMessage(Lang.Get("srguildsandkingdoms:claim-submitted", new object[]
					{
						chunkX,
						chunkZ
					}));
				}
				else
				{
					this.pendingClaims.Add(new ValueTuple<int, int>(chunkX, chunkZ));
					this.capi.ShowChatMessage(Lang.Get("srguildsandkingdoms:claim-added-to-pending", new object[]
					{
						chunkX,
						chunkZ
					}));
				}
			}
			if (!sendImmediately)
			{
				this.CheckForPendingChanges();
				GuildLandsTab guildLandsTab = this.landsTab;
				if (guildLandsTab != null)
				{
					guildLandsTab.SetPendingClaims(this.pendingClaims);
				}
			}
			this.SetupDialog();
		}

		// Token: 0x0600049F RID: 1183 RVA: 0x0001BD0C File Offset: 0x00019F0C
		private bool IsAdjacent(int chunkX1, int chunkZ1, int chunkX2, int chunkZ2)
		{
			int deltaX = Math.Abs(chunkX1 - chunkX2);
			int deltaZ = Math.Abs(chunkZ1 - chunkZ2);
			return (deltaX == 1 && deltaZ == 0) || (deltaX == 0 && deltaZ == 1);
		}

		// Token: 0x060004A0 RID: 1184 RVA: 0x0001BD40 File Offset: 0x00019F40
		private bool IsChunkAdjacentToAnyClaim(List<LandClaimDto> claims, int targetChunkX, int targetChunkZ)
		{
			return claims.Any((LandClaimDto claim) => this.IsAdjacent(claim.ChunkX, claim.ChunkZ, targetChunkX, targetChunkZ));
		}

		// Token: 0x17000162 RID: 354
		// (get) Token: 0x060004A1 RID: 1185 RVA: 0x0001BD7A File Offset: 0x00019F7A
		public bool IsClaimingModeActive
		{
			get
			{
				return this.isClaimingMode;
			}
		}

		// Token: 0x17000163 RID: 355
		// (get) Token: 0x060004A2 RID: 1186 RVA: 0x0001BD82 File Offset: 0x00019F82
		public bool IsUnclaimingModeActive
		{
			get
			{
				return this.isUnclaimingMode;
			}
		}

		// Token: 0x060004A3 RID: 1187 RVA: 0x0001BD8A File Offset: 0x00019F8A
		[return: TupleElementNames(new string[]
		{
			"chunkX",
			"chunkZ"
		})]
		[return: Nullable(new byte[]
		{
			1,
			0
		})]
		public List<ValueTuple<int, int>> GetPendingClaims()
		{
			return new List<ValueTuple<int, int>>(this.pendingClaims);
		}

		// Token: 0x060004A4 RID: 1188 RVA: 0x0001BD97 File Offset: 0x00019F97
		[return: TupleElementNames(new string[]
		{
			"chunkX",
			"chunkZ"
		})]
		[return: Nullable(new byte[]
		{
			1,
			0
		})]
		public List<ValueTuple<int, int>> GetPendingUnclaims()
		{
			return new List<ValueTuple<int, int>>(this.pendingUnclaims);
		}

		// Token: 0x060004A5 RID: 1189 RVA: 0x0001BDA4 File Offset: 0x00019FA4
		public void OnMapChunkUnclaimed(int chunkX, int chunkZ)
		{
			if (!this.isUnclaimingMode || this.currentGuild == null)
			{
				return;
			}
			if (this.currentGuild.Claims.FirstOrDefault((LandClaimDto c) => c.ChunkX == chunkX && c.ChunkZ == chunkZ) == null)
			{
				this.capi.ShowChatMessage(Lang.Get("srguildsandkingdoms:chunk-not-claimed", Array.Empty<object>()));
				return;
			}
			if (this.pendingUnclaims.Any(([TupleElementNames(new string[]
			{
				"chunkX",
				"chunkZ"
			})] ValueTuple<int, int> p) => p.Item1 == chunkX && p.Item2 == chunkZ))
			{
				this.capi.ShowChatMessage(Lang.Get("srguildsandkingdoms:chunk-already-pending-unclaim", Array.Empty<object>()));
				return;
			}
			this.pendingUnclaims.Add(new ValueTuple<int, int>(chunkX, chunkZ));
			this.capi.ShowChatMessage(Lang.Get("srguildsandkingdoms:unclaim-added-to-pending", new object[]
			{
				chunkX,
				chunkZ
			}));
			GuildLandsTab guildLandsTab = this.landsTab;
			if (guildLandsTab != null)
			{
				guildLandsTab.SetPendingUnclaims(this.pendingUnclaims);
			}
			this.SetupDialog();
		}

		// Token: 0x060004A6 RID: 1190 RVA: 0x0001BEB4 File Offset: 0x0001A0B4
		public bool IsPendingGuildHomeChunk(int chunkX, int chunkZ)
		{
			if (this.currentGuild == null)
			{
				return false;
			}
			if (this.currentGuild.Claims.Any((LandClaimDto c) => c.IsGuildHome))
			{
				return false;
			}
			if (!this.pendingClaims.Any(([TupleElementNames(new string[]
			{
				"chunkX",
				"chunkZ"
			})] ValueTuple<int, int> p) => p.Item1 == chunkX && p.Item2 == chunkZ))
			{
				return false;
			}
			if (this.pendingClaims.Count >= 4)
			{
				List<ValueTuple<int, int>> firstFour = this.pendingClaims.Take(4).ToList<ValueTuple<int, int>>();
				if (this.IsValid2x2Pattern(firstFour))
				{
					return firstFour.Any(([TupleElementNames(new string[]
					{
						"chunkX",
						"chunkZ"
					})] ValueTuple<int, int> p) => p.Item1 == chunkX && p.Item2 == chunkZ);
				}
			}
			return false;
		}

		// Token: 0x060004A7 RID: 1191 RVA: 0x0001BF6C File Offset: 0x0001A16C
		private bool IsValid2x2Pattern([TupleElementNames(new string[]
		{
			"chunkX",
			"chunkZ"
		})] [Nullable(new byte[]
		{
			1,
			0
		})] List<ValueTuple<int, int>> chunks)
		{
			if (chunks.Count != 4)
			{
				return false;
			}
			List<ValueTuple<int, int>> sorted = (from c in chunks
			orderby c.Item1, c.Item2
			select c).ToList<ValueTuple<int, int>>();
			List<ValueTuple<int, int>> list = new List<ValueTuple<int, int>>();
			list.Add(new ValueTuple<int, int>(sorted[0].Item1, sorted[0].Item2));
			list.Add(new ValueTuple<int, int>(sorted[0].Item1 + 1, sorted[0].Item2));
			list.Add(new ValueTuple<int, int>(sorted[0].Item1, sorted[0].Item2 + 1));
			list.Add(new ValueTuple<int, int>(sorted[0].Item1 + 1, sorted[0].Item2 + 1));
			list.Sort();
			List<ValueTuple<int, int>> actual = (from c in chunks
			select new ValueTuple<int, int>(c.Item1, c.Item2)).ToList<ValueTuple<int, int>>();
			actual.Sort();
			return list.SequenceEqual(actual);
		}

		// Token: 0x060004A8 RID: 1192 RVA: 0x0001C0AC File Offset: 0x0001A2AC
		private bool OnSavePendingClaims()
		{
			if (this.currentGuild == null || this.pendingClaims.Count == 0)
			{
				return true;
			}
			GuildNetworkHandler networkHandler = this.modSystem.GetNetworkHandler();
			if (networkHandler != null)
			{
				foreach (ValueTuple<int, int> valueTuple in this.pendingClaims)
				{
					int chunkX = valueTuple.Item1;
					int chunkZ = valueTuple.Item2;
					networkHandler.SendGuildClaimLandRequest(chunkX, chunkZ);
				}
				this.capi.ShowChatMessage(Lang.Get("srguildsandkingdoms:claims-submitted", new object[]
				{
					this.pendingClaims.Count
				}));
				this.pendingClaims.Clear();
				this.isClaimingMode = false;
				GuildLandsTab guildLandsTab = this.landsTab;
				if (guildLandsTab != null)
				{
					guildLandsTab.SetClaimingMode(this.isClaimingMode);
				}
				this.CheckForPendingChanges();
			}
			return true;
		}

		// Token: 0x060004A9 RID: 1193 RVA: 0x0001C194 File Offset: 0x0001A394
		private bool OnSavePendingUnclaims()
		{
			if (this.currentGuild == null || this.pendingUnclaims.Count == 0)
			{
				return true;
			}
			GuildNetworkHandler networkHandler = this.modSystem.GetNetworkHandler();
			if (networkHandler != null)
			{
				foreach (ValueTuple<int, int> valueTuple in this.pendingUnclaims)
				{
					int chunkX = valueTuple.Item1;
					int chunkZ = valueTuple.Item2;
					networkHandler.SendGuildUnclaimLandRequest(chunkX, chunkZ);
				}
				this.capi.ShowChatMessage(Lang.Get("srguildsandkingdoms:unclaims-submitted", new object[]
				{
					this.pendingUnclaims.Count
				}));
				this.pendingUnclaims.Clear();
				this.isUnclaimingMode = false;
				GuildLandsTab guildLandsTab = this.landsTab;
				if (guildLandsTab != null)
				{
					guildLandsTab.SetUnclaimingMode(false);
				}
				GuildLandsTab guildLandsTab2 = this.landsTab;
				if (guildLandsTab2 != null)
				{
					guildLandsTab2.SetPendingUnclaims(this.pendingUnclaims);
				}
				this.SetupDialog();
			}
			return true;
		}

		// Token: 0x060004AA RID: 1194 RVA: 0x0001C290 File Offset: 0x0001A490
		private bool OnSaveSettings()
		{
			if (this.currentGuild == null)
			{
				return true;
			}
			if (!(this.pendingGuildName != this.currentGuild.Name) && !(this.pendingDescription != this.currentGuild.Description) && !(this.pendingPrimaryColor != this.ColorToHex(this.currentGuild.DisplayColor)) && !(this.pendingSecondaryColor != this.ColorToHex(this.currentGuild.SecondaryColor)))
			{
				this.capi.ShowChatMessage(Lang.Get("srguildsandkingdoms:no-changes-to-save", Array.Empty<object>()));
				return true;
			}
			int num;
			if (this.pendingPrimaryColor != this.ColorToHex(this.currentGuild.DisplayColor) && !this.TryParseHexColor(this.pendingPrimaryColor ?? "", out num))
			{
				this.capi.ShowChatMessage(Lang.Get("srguildsandkingdoms:invalid-primary-color", Array.Empty<object>()));
				return false;
			}
			if (this.pendingSecondaryColor != this.ColorToHex(this.currentGuild.SecondaryColor) && !this.TryParseHexColor(this.pendingSecondaryColor ?? "", out num))
			{
				this.capi.ShowChatMessage(Lang.Get("srguildsandkingdoms:invalid-secondary-color", Array.Empty<object>()));
				return false;
			}
			if (this.modSystem.GetNetworkHandler() == null)
			{
				this.capi.ShowChatMessage(Lang.Get("srguildsandkingdoms:network-error", Array.Empty<object>()));
				return false;
			}
			if (this.pendingGuildName != this.currentGuild.Name && !string.IsNullOrWhiteSpace(this.pendingGuildName))
			{
				GuildCommandPacket nameChangePacket = new GuildCommandPacket
				{
					PlayerUid = this.capi.World.Player.PlayerUID,
					Command = "changename",
					Arguments = new string[]
					{
						this.pendingGuildName
					}
				};
				this.capi.Network.GetChannel("srguildsandkingdoms:guild").SendPacket<GuildCommandPacket>(nameChangePacket);
			}
			if (this.pendingDescription != this.currentGuild.Description)
			{
				GuildCommandPacket descriptionChangePacket = new GuildCommandPacket
				{
					PlayerUid = this.capi.World.Player.PlayerUID,
					Command = "changedescription",
					Arguments = new string[]
					{
						this.pendingDescription ?? ""
					}
				};
				this.capi.Network.GetChannel("srguildsandkingdoms:guild").SendPacket<GuildCommandPacket>(descriptionChangePacket);
			}
			int primaryColor;
			if (this.pendingPrimaryColor != this.ColorToHex(this.currentGuild.DisplayColor) && this.TryParseHexColor(this.pendingPrimaryColor ?? "", out primaryColor))
			{
				GuildCommandPacket primaryColorPacket = new GuildCommandPacket
				{
					PlayerUid = this.capi.World.Player.PlayerUID,
					Command = "changecolor",
					Arguments = new string[]
					{
						primaryColor.ToString()
					}
				};
				this.capi.Network.GetChannel("srguildsandkingdoms:guild").SendPacket<GuildCommandPacket>(primaryColorPacket);
			}
			int secondaryColor;
			if (this.pendingSecondaryColor != this.ColorToHex(this.currentGuild.SecondaryColor) && this.TryParseHexColor(this.pendingSecondaryColor ?? "", out secondaryColor))
			{
				GuildCommandPacket secondaryColorPacket = new GuildCommandPacket
				{
					PlayerUid = this.capi.World.Player.PlayerUID,
					Command = "changesecondarycolor",
					Arguments = new string[]
					{
						secondaryColor.ToString()
					}
				};
				this.capi.Network.GetChannel("srguildsandkingdoms:guild").SendPacket<GuildCommandPacket>(secondaryColorPacket);
			}
			this.capi.ShowChatMessage(Lang.Get("srguildsandkingdoms:settings-saved", Array.Empty<object>()));
			this.hasPendingChanges = false;
			this.CheckForPendingChanges();
			this.SetupDialog();
			return true;
		}

		// Token: 0x060004AB RID: 1195 RVA: 0x0001C661 File Offset: 0x0001A861
		private bool OnCloseDialog()
		{
			this.TryClose();
			return true;
		}

		// Token: 0x060004AC RID: 1196 RVA: 0x0001C66B File Offset: 0x0001A86B
		private bool OnInvitePlayer()
		{
			new DialogGuildInvitePlayer(this.capi, this.modSystem).TryOpen();
			return true;
		}

		// Token: 0x060004AD RID: 1197 RVA: 0x0001C685 File Offset: 0x0001A885
		private bool OnManageRoles()
		{
			new DialogGuildManageRoles(this.capi, this.modSystem).TryOpen();
			return true;
		}

		// Token: 0x060004AE RID: 1198 RVA: 0x0001C69F File Offset: 0x0001A89F
		private bool OnViewMembers()
		{
			new DialogGuildMembers(this.capi, this.modSystem).TryOpen();
			return true;
		}

		// Token: 0x060004AF RID: 1199 RVA: 0x0001C6B9 File Offset: 0x0001A8B9
		private bool OnManagePendingInvites()
		{
			new DialogGuildPendingInvites(this.capi, this.modSystem).TryOpen();
			return true;
		}

		// Token: 0x060004B0 RID: 1200 RVA: 0x0001C6D4 File Offset: 0x0001A8D4
		private bool OnNodeWarSignup()
		{
			if (this.currentGuild == null)
			{
				return false;
			}
			GuildNodeWarsTab guildNodeWarsTab = this.nodeWarsTab;
			string selectedNodeId = (guildNodeWarsTab != null) ? guildNodeWarsTab.GetSelectedWarForSignup() : null;
			if (string.IsNullOrEmpty(selectedNodeId))
			{
				this.capi.ShowChatMessage("No war selected for signup");
				return false;
			}
			ModSystem pvpMod = this.capi.ModLoader.GetModSystem("SRGuildsAndKingdomsPVP.PVPModSystem");
			if (pvpMod == null)
			{
				this.capi.ShowChatMessage("PVP mod not loaded");
				return false;
			}
			MethodInfo method = pvpMod.GetType().GetMethod("GetNetworkHandler");
			object networkHandler = (method != null) ? method.Invoke(pvpMod, null) : null;
			if (networkHandler == null)
			{
				this.capi.ShowChatMessage("PVP mod network handler not available");
				return false;
			}
			MethodInfo method2 = networkHandler.GetType().GetMethod("RequestGuildSignup");
			if (method2 != null)
			{
				method2.Invoke(networkHandler, new object[]
				{
					selectedNodeId
				});
			}
			this.capi.ShowChatMessage("Signing up for node war...");
			this.capi.Event.RegisterCallback(delegate(float dt)
			{
				this.RefreshNodeWarsData();
			}, 1000);
			return true;
		}

		// Token: 0x060004B1 RID: 1201 RVA: 0x0001C7D4 File Offset: 0x0001A9D4
		private bool OnNodeWarCancelSignup()
		{
			if (this.currentGuild == null)
			{
				return false;
			}
			GuildNodeWarsTab guildNodeWarsTab = this.nodeWarsTab;
			CurrentSignupInfo currentSignup = (guildNodeWarsTab != null) ? guildNodeWarsTab.GetCurrentSignup() : null;
			if (currentSignup == null)
			{
				this.capi.ShowChatMessage("No active signup to cancel");
				return false;
			}
			ModSystem pvpMod = this.capi.ModLoader.GetModSystem("SRGuildsAndKingdomsPVP.PVPModSystem");
			if (pvpMod == null)
			{
				this.capi.ShowChatMessage("PVP mod not loaded");
				return false;
			}
			MethodInfo method = pvpMod.GetType().GetMethod("GetNetworkHandler");
			object networkHandler = (method != null) ? method.Invoke(pvpMod, null) : null;
			if (networkHandler == null)
			{
				this.capi.ShowChatMessage("PVP mod network handler not available");
				return false;
			}
			MethodInfo method2 = networkHandler.GetType().GetMethod("RequestCancelGuildSignup");
			if (method2 != null)
			{
				method2.Invoke(networkHandler, new object[]
				{
					currentSignup.NodeId
				});
			}
			this.capi.ShowChatMessage("Cancelling signup for " + currentSignup.NodeName + "...");
			this.capi.Event.RegisterCallback(delegate(float dt)
			{
				this.RefreshNodeWarsData();
			}, 1000);
			return true;
		}

		// Token: 0x060004B2 RID: 1202 RVA: 0x0001C8E4 File Offset: 0x0001AAE4
		private bool OnNodeWarJoin()
		{
			if (this.currentGuild == null)
			{
				return false;
			}
			GuildNodeWarsTab guildNodeWarsTab = this.nodeWarsTab;
			CurrentWarInfo currentWar = (guildNodeWarsTab != null) ? guildNodeWarsTab.GetCurrentWar() : null;
			if (currentWar == null)
			{
				this.capi.ShowChatMessage("No active war to join");
				return false;
			}
			if (this.modSystem.GetNetworkHandler() != null)
			{
				this.capi.ShowChatMessage("Joining war at " + currentWar.NodeName + "...");
			}
			return true;
		}

		// Token: 0x060004B3 RID: 1203 RVA: 0x0001C954 File Offset: 0x0001AB54
		private bool OnNodeWarViewDetails()
		{
			if (this.currentGuild == null)
			{
				return false;
			}
			GuildNodeWarsTab guildNodeWarsTab = this.nodeWarsTab;
			CurrentWarInfo currentWar = (guildNodeWarsTab != null) ? guildNodeWarsTab.GetCurrentWar() : null;
			if (currentWar == null)
			{
				this.capi.ShowChatMessage("No active war to view");
				return false;
			}
			this.capi.ShowChatMessage("War Details: " + currentWar.NodeName + " - Status: " + currentWar.Status);
			return true;
		}

		// Token: 0x060004B4 RID: 1204 RVA: 0x0001C9BA File Offset: 0x0001ABBA
		private bool OnCreateGuild()
		{
			new DialogCreateGuild(this.capi, this.modSystem).TryOpen();
			this.TryClose();
			return true;
		}

		// Token: 0x060004B5 RID: 1205 RVA: 0x0001C9DC File Offset: 0x0001ABDC
		private bool OnLeaveGuild()
		{
			if (this.currentGuild == null)
			{
				return false;
			}
			string confirmMessage;
			if (this.currentGuild.PlayerRole == "Leader")
			{
				if (this.currentGuild.MemberCount != 1)
				{
					this.capi.ShowChatMessage(Lang.Get("srguildsandkingdoms:leader-cannot-leave", Array.Empty<object>()));
					return true;
				}
				confirmMessage = Lang.Get("srguildsandkingdoms:confirm-disband-guild", new object[]
				{
					this.currentGuild.Name
				});
			}
			else
			{
				confirmMessage = Lang.Get("srguildsandkingdoms:confirm-leave-guild", new object[]
				{
					this.currentGuild.Name
				});
			}
			new DialogGuildLeaveConfirm(this.capi, confirmMessage, delegate
			{
				GuildNetworkHandler networkHandler = this.modSystem.GetNetworkHandler();
				if (networkHandler != null)
				{
					networkHandler.SendGuildLeaveRequest();
					this.TryClose();
				}
			}).TryOpen();
			return true;
		}

		// Token: 0x060004B6 RID: 1206 RVA: 0x0001CA93 File Offset: 0x0001AC93
		private bool OnShowHologram()
		{
			this.modSystem.ToggleHologram();
			return true;
		}

		// Token: 0x060004B7 RID: 1207 RVA: 0x0001CAA4 File Offset: 0x0001ACA4
		private bool OnCreateOutpost()
		{
			if (this.currentGuild == null)
			{
				return false;
			}
			bool flag = this.currentGuild.Claims.Any((LandClaimDto c) => c.IsGuildHome);
			int outpostCount = this.currentGuild.Claims.Count((LandClaimDto c) => c.IsOutpost);
			int maxOutposts = this.currentGuild.MaxOutposts;
			if (!flag)
			{
				this.capi.ShowChatMessage(Lang.Get("srguildsandkingdoms:outpost-requires-guild-home", Array.Empty<object>()));
				return true;
			}
			if (outpostCount >= maxOutposts)
			{
				this.capi.ShowChatMessage(Lang.Get("srguildsandkingdoms:max-outposts-reached", new object[]
				{
					outpostCount,
					maxOutposts
				}));
				return true;
			}
			new DialogCreateOutpost(this.capi, this.modSystem, delegate(string outpostName)
			{
				this.OnOutpostCreationRequested(outpostName);
			}).TryOpen();
			return true;
		}

		// Token: 0x060004B8 RID: 1208 RVA: 0x0001CB9C File Offset: 0x0001AD9C
		private void OnOutpostCreationRequested(string outpostName)
		{
			if (this.currentGuild == null)
			{
				return;
			}
			BlockPos playerPos = this.capi.World.Player.Entity.Pos.AsBlockPos;
			int chunkX = LandClaim.FloorDiv(playerPos.X, 32);
			int chunkZ = LandClaim.FloorDiv(playerPos.Z, 32);
			Func<LandClaimDto, bool> <>9__1;
			if (this.modSystem.GetClientGuildSummaries().Any(delegate(GuildSummary guild)
			{
				IEnumerable<LandClaimDto> claims = guild.Claims;
				Func<LandClaimDto, bool> predicate;
				if ((predicate = <>9__1) == null)
				{
					predicate = (<>9__1 = ((LandClaimDto claim) => claim.ChunkX == chunkX && claim.ChunkZ == chunkZ));
				}
				return claims.Any(predicate);
			}))
			{
				this.capi.ShowChatMessage(Lang.Get("srguildsandkingdoms:chunk-already-claimed-outpost", new object[]
				{
					chunkX,
					chunkZ
				}));
				return;
			}
			GuildNetworkHandler networkHandler = this.modSystem.GetNetworkHandler();
			if (networkHandler != null)
			{
				networkHandler.SendGuildClaimLandRequest(chunkX, chunkZ, true, outpostName);
				string message = string.IsNullOrEmpty(outpostName) ? Lang.Get("srguildsandkingdoms:outpost-creation-requested", new object[]
				{
					chunkX,
					chunkZ
				}) : Lang.Get("srguildsandkingdoms:outpost-creation-requested-named", new object[]
				{
					outpostName,
					chunkX,
					chunkZ
				});
				this.capi.ShowChatMessage(message);
				return;
			}
			this.capi.ShowChatMessage(Lang.Get("srguildsandkingdoms:network-error", Array.Empty<object>()));
		}

		// Token: 0x060004B9 RID: 1209 RVA: 0x0001CD08 File Offset: 0x0001AF08
		private void OnGuildDataUpdated(List<GuildSummary> updatedGuilds)
		{
			GuildSummary previousGuild = this.currentGuild;
			this.currentGuild = this.modSystem.GetCurrentPlayerGuildSummary();
			if (this.IsOpened() && this.currentGuild != null)
			{
				if (previousGuild != null && this.currentGuild.Claims.Count > previousGuild.Claims.Count)
				{
					using (List<LandClaimDto>.Enumerator enumerator = (from c in this.currentGuild.Claims
					where !previousGuild.Claims.Any((LandClaimDto pc) => pc.ChunkX == c.ChunkX && pc.ChunkZ == c.ChunkZ)
					select c).ToList<LandClaimDto>().GetEnumerator())
					{
						while (enumerator.MoveNext())
						{
							LandClaimDto newClaim = enumerator.Current;
							this.pendingClaims.RemoveAll(([TupleElementNames(new string[]
							{
								"chunkX",
								"chunkZ"
							})] ValueTuple<int, int> p) => p.Item1 == newClaim.ChunkX && p.Item2 == newClaim.ChunkZ);
						}
					}
				}
				if (previousGuild != null && this.currentGuild.Claims.Count < previousGuild.Claims.Count)
				{
					using (List<LandClaimDto>.Enumerator enumerator = (from c in previousGuild.Claims
					where !this.currentGuild.Claims.Any((LandClaimDto nc) => nc.ChunkX == c.ChunkX && nc.ChunkZ == c.ChunkZ)
					select c).ToList<LandClaimDto>().GetEnumerator())
					{
						while (enumerator.MoveNext())
						{
							LandClaimDto removedClaim = enumerator.Current;
							this.pendingUnclaims.RemoveAll(([TupleElementNames(new string[]
							{
								"chunkX",
								"chunkZ"
							})] ValueTuple<int, int> p) => p.Item1 == removedClaim.ChunkX && p.Item2 == removedClaim.ChunkZ);
						}
					}
				}
				string a = this.pendingGuildName;
				GuildSummary previousGuild4 = previousGuild;
				if (a == ((previousGuild4 != null) ? previousGuild4.Name : null))
				{
					this.pendingGuildName = this.currentGuild.Name;
				}
				string a2 = this.pendingPrimaryColor;
				GuildSummary previousGuild2 = previousGuild;
				if (a2 == this.ColorToHex((previousGuild2 != null) ? previousGuild2.DisplayColor : 0))
				{
					this.pendingPrimaryColor = this.ColorToHex(this.currentGuild.DisplayColor);
				}
				string a3 = this.pendingSecondaryColor;
				GuildSummary previousGuild3 = previousGuild;
				if (a3 == this.ColorToHex((previousGuild3 != null) ? previousGuild3.SecondaryColor : 0))
				{
					this.pendingSecondaryColor = this.ColorToHex(this.currentGuild.SecondaryColor);
				}
				this.SetupDialog();
				if (this.modSystem.IsHologramVisible)
				{
					this.modSystem.ShowClaimsHologram();
				}
			}
		}

		// Token: 0x060004BA RID: 1210 RVA: 0x0001CF6C File Offset: 0x0001B16C
		private void OnTitleBarCloseClicked()
		{
			if (this.isClaimingMode)
			{
				this.isClaimingMode = false;
			}
			PlotMapLayer plotLayer = this.modSystem.GetPlotLayer();
			if (plotLayer != null)
			{
				plotLayer.SetActiveGuildDialog(null);
			}
			this.modSystem.OnClientGuildDataUpdated -= this.OnGuildDataUpdated;
			this.TryClose();
		}

		// Token: 0x060004BB RID: 1211 RVA: 0x0001CFC0 File Offset: 0x0001B1C0
		private void OnNodeWarDataReceived(NodeWarDataResponsePacket packet)
		{
			if (this.nodeWarsTab == null)
			{
				return;
			}
			NodeWarTabData nodeWarTabData = new NodeWarTabData();
			nodeWarTabData.ControlledNodes = (from dto in packet.ControlledNodes
			select new ControlledNodeInfo
			{
				NodeId = dto.NodeId,
				NodeName = dto.NodeName,
				CapturedAt = ((dto.CapturedAtTicks > 0L) ? new DateTime?(new DateTime(dto.CapturedAtTicks)) : null),
				InfluencePerDay = dto.InfluencePerDay
			}).ToList<ControlledNodeInfo>();
			CurrentWarInfo currentWar;
			if (packet.CurrentWar == null)
			{
				currentWar = null;
			}
			else
			{
				CurrentWarInfo currentWarInfo = new CurrentWarInfo();
				currentWarInfo.NodeId = packet.CurrentWar.NodeId;
				currentWarInfo.NodeName = packet.CurrentWar.NodeName;
				currentWarInfo.Status = packet.CurrentWar.Status;
				currentWarInfo.PointsNeeded = packet.CurrentWar.PointsNeeded;
				currentWar = currentWarInfo;
				GuildWarProgressInfo yourGuildProgress;
				if (packet.CurrentWar.YourGuildProgress == null)
				{
					yourGuildProgress = null;
				}
				else
				{
					GuildWarProgressInfo guildWarProgressInfo = new GuildWarProgressInfo();
					guildWarProgressInfo.CapturePoints = packet.CurrentWar.YourGuildProgress.CapturePoints;
					guildWarProgressInfo.PlayersInZone = packet.CurrentWar.YourGuildProgress.PlayersInZone;
					guildWarProgressInfo.Kills = packet.CurrentWar.YourGuildProgress.Kills;
					yourGuildProgress = guildWarProgressInfo;
					guildWarProgressInfo.Deaths = packet.CurrentWar.YourGuildProgress.Deaths;
				}
				currentWarInfo.YourGuildProgress = yourGuildProgress;
			}
			nodeWarTabData.CurrentWar = currentWar;
			nodeWarTabData.AvailableWars = (from dto in packet.AvailableWars
			select new AvailableWarInfo
			{
				NodeId = dto.NodeId,
				NodeName = dto.NodeName,
				WarStartTime = new DateTime(dto.WarStartTimeTicks),
				CurrentSignups = dto.CurrentSignups,
				MaxGuilds = dto.MaxGuilds,
				CanSignup = dto.CanSignup
			}).ToList<AvailableWarInfo>();
			CurrentSignupInfo currentSignup;
			if (packet.CurrentSignup == null)
			{
				currentSignup = null;
			}
			else
			{
				CurrentSignupInfo currentSignupInfo = new CurrentSignupInfo();
				currentSignupInfo.NodeId = packet.CurrentSignup.NodeId;
				currentSignupInfo.NodeName = packet.CurrentSignup.NodeName;
				currentSignupInfo.SignupTime = new DateTime(packet.CurrentSignup.SignupTimeTicks);
				currentSignup = currentSignupInfo;
				currentSignupInfo.WarStartTime = new DateTime(packet.CurrentSignup.WarStartTimeTicks);
			}
			nodeWarTabData.CurrentSignup = currentSignup;
			NodeWarTabData nodeWarData = nodeWarTabData;
			this.cachedNodeWarData = nodeWarData;
			this.nodeWarsTab.SetNodeWarData(nodeWarData);
			if (this.currentTab == 4 && this.IsOpened())
			{
				this.SetupDialog();
			}
			ILogger logger = this.capi.Logger;
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(68, 2);
			defaultInterpolatedStringHandler.AppendLiteral("[Guild UI] Node war data updated: ");
			defaultInterpolatedStringHandler.AppendFormatted<int>(packet.ControlledNodes.Count);
			defaultInterpolatedStringHandler.AppendLiteral(" controlled nodes, ");
			defaultInterpolatedStringHandler.AppendFormatted<int>(packet.AvailableWars.Count);
			defaultInterpolatedStringHandler.AppendLiteral(" available wars");
			logger.Debug(defaultInterpolatedStringHandler.ToStringAndClear());
		}

		// Token: 0x060004BC RID: 1212 RVA: 0x0001D210 File Offset: 0x0001B410
		public override void OnGuiClosed()
		{
			if (this.researchTab != null)
			{
				this.researchTab.Dispose();
				this.researchTab = null;
			}
			if (this.isClaimingMode)
			{
				this.isClaimingMode = false;
			}
			this.modSystem.OnClientGuildDataUpdated -= this.OnGuildDataUpdated;
			PlotMapLayer plotLayer = this.modSystem.GetPlotLayer();
			if (plotLayer != null)
			{
				plotLayer.SetActiveGuildDialog(null);
			}
			this.modSystem.OnClientGuildDataUpdated -= this.OnGuildDataUpdated;
			base.OnGuiClosed();
		}

		// Token: 0x060004BD RID: 1213 RVA: 0x0001D294 File Offset: 0x0001B494
		public override void OnGuiOpened()
		{
			base.OnGuiOpened();
			this.currentGuild = this.modSystem.GetCurrentPlayerGuildSummary();
			if (this.currentGuild != null)
			{
				this.pendingGuildName = this.currentGuild.Name;
				this.pendingPrimaryColor = this.ColorToHex(this.currentGuild.DisplayColor);
				this.pendingSecondaryColor = this.ColorToHex(this.currentGuild.SecondaryColor);
				this.hasPendingChanges = false;
				this.SetupDialog();
			}
		}

		// Token: 0x17000164 RID: 356
		// (get) Token: 0x060004BE RID: 1214 RVA: 0x0001D30C File Offset: 0x0001B50C
		public override bool PrefersUngrabbedMouse
		{
			get
			{
				return false;
			}
		}

		// Token: 0x060004BF RID: 1215 RVA: 0x0001D310 File Offset: 0x0001B510
		private string ColorToHex(int argbColor)
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

		// Token: 0x060004C0 RID: 1216 RVA: 0x0001D380 File Offset: 0x0001B580
		private bool TryParseHexColor(string hex, out int argbColor)
		{
			argbColor = 0;
			if (string.IsNullOrWhiteSpace(hex))
			{
				return false;
			}
			hex = hex.Trim();
			if (hex.StartsWith("#"))
			{
				hex = hex.Substring(1);
			}
			if (hex.Length == 3)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(0, 6);
				defaultInterpolatedStringHandler.AppendFormatted<char>(hex[0]);
				defaultInterpolatedStringHandler.AppendFormatted<char>(hex[0]);
				defaultInterpolatedStringHandler.AppendFormatted<char>(hex[1]);
				defaultInterpolatedStringHandler.AppendFormatted<char>(hex[1]);
				defaultInterpolatedStringHandler.AppendFormatted<char>(hex[2]);
				defaultInterpolatedStringHandler.AppendFormatted<char>(hex[2]);
				hex = defaultInterpolatedStringHandler.ToStringAndClear();
			}
			else if (hex.Length != 6)
			{
				return false;
			}
			string text = hex;
			for (int i = 0; i < text.Length; i++)
			{
				if (!DialogGuildMain.IsHexDigit(text[i]))
				{
					return false;
				}
			}
			try
			{
				uint rgb;
				if (uint.TryParse(hex, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out rgb))
				{
					argbColor = (int)(4278190080U | rgb);
					return true;
				}
			}
			catch
			{
			}
			return false;
		}

		// Token: 0x060004C1 RID: 1217 RVA: 0x0001D494 File Offset: 0x0001B694
		private static bool IsHexDigit(char c)
		{
			return (c >= '0' && c <= '9') || (c >= 'A' && c <= 'F') || (c >= 'a' && c <= 'f');
		}

		// Token: 0x060004C2 RID: 1218 RVA: 0x0001D4BC File Offset: 0x0001B6BC
		public override void Dispose()
		{
			IDisposable disposableResearch = this.researchTab as IDisposable;
			if (disposableResearch != null)
			{
				disposableResearch.Dispose();
			}
			base.Dispose();
		}

		// Token: 0x040001BE RID: 446
		private SRGuildsAndKingdomsModSystem modSystem;

		// Token: 0x040001BF RID: 447
		[Nullable(2)]
		private GuildSummary currentGuild;

		// Token: 0x040001C0 RID: 448
		private bool isClaimingMode;

		// Token: 0x040001C1 RID: 449
		private bool isUnclaimingMode;

		// Token: 0x040001C2 RID: 450
		[Nullable(2)]
		private string pendingGuildName;

		// Token: 0x040001C3 RID: 451
		[Nullable(2)]
		private string pendingDescription;

		// Token: 0x040001C4 RID: 452
		[Nullable(2)]
		private string pendingPrimaryColor;

		// Token: 0x040001C5 RID: 453
		[Nullable(2)]
		private string pendingSecondaryColor;

		// Token: 0x040001C6 RID: 454
		private bool hasPendingChanges;

		// Token: 0x040001C7 RID: 455
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
		private List<ValueTuple<int, int>> pendingClaims = new List<ValueTuple<int, int>>();

		// Token: 0x040001C8 RID: 456
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
		private List<ValueTuple<int, int>> pendingUnclaims = new List<ValueTuple<int, int>>();

		// Token: 0x040001C9 RID: 457
		private int currentTab;

		// Token: 0x040001CA RID: 458
		private const int TAB_OVERVIEW = 0;

		// Token: 0x040001CB RID: 459
		private const int TAB_MEMBERS = 1;

		// Token: 0x040001CC RID: 460
		private const int TAB_LANDS = 2;

		// Token: 0x040001CD RID: 461
		private const int TAB_RESEARCH = 3;

		// Token: 0x040001CE RID: 462
		private const int TAB_NODEWARS = 4;

		// Token: 0x040001CF RID: 463
		private const int TAB_SETTINGS = 5;

		// Token: 0x040001D0 RID: 464
		[Nullable(2)]
		private GuildOverviewTab overviewTab;

		// Token: 0x040001D1 RID: 465
		[Nullable(2)]
		private GuildMembersTab membersTab;

		// Token: 0x040001D2 RID: 466
		[Nullable(2)]
		private GuildLandsTab landsTab;

		// Token: 0x040001D3 RID: 467
		[Nullable(2)]
		private GuildResearch researchTab;

		// Token: 0x040001D4 RID: 468
		[Nullable(2)]
		private GuildNodeWarsTab nodeWarsTab;

		// Token: 0x040001D5 RID: 469
		[Nullable(2)]
		private GuildSettingsTab settingsTab;

		// Token: 0x040001D6 RID: 470
		[Nullable(2)]
		private NodeWarTabData cachedNodeWarData;
	}
}
