using SOAGuildsAndKingdoms.src.gui.tabs;
using SOAGuildsAndKingdoms.src.guilds;
using SOAGuildsAndKingdoms.src.network;
using System;
using System.Collections.Generic;
using System.Linq;
using Vintagestory.API.Client;
using Vintagestory.API.Config;

namespace SOAGuildsAndKingdoms.src.gui
{
    public class DialogGuildMain : GuiDialog
    {
        private SOAGuildsAndKingdomsModSystem modSystem;
        private GuildSummary? currentGuild;
        private bool isClaimingMode = false;
        private bool isUnclaimingMode = false;

        // Track pending changes
        private string? pendingGuildName;
        private string? pendingDescription;
        private string? pendingPrimaryColor;
        private string? pendingSecondaryColor;
        private bool hasPendingChanges = false;

        // Track pending claims (chunks to be claimed when save is clicked)
        private List<(int chunkX, int chunkZ)> pendingClaims = new List<(int, int)>();

        // Track pending unclaims (chunks to be unclaimed when save is clicked)
        private List<(int chunkX, int chunkZ)> pendingUnclaims = new List<(int, int)>();

        // Tab management
        private int currentTab = 0;
        private const int TAB_OVERVIEW = 0;
        private const int TAB_MEMBERS = 1;
        private const int TAB_LANDS = 2;
        private const int TAB_RESEARCH = 3;
		private const int TAB_NODEWARS = 4;
		private const int TAB_SETTINGS = 5;

        // Tab content classes
        private GuildOverviewTab? overviewTab;
        private GuildMembersTab? membersTab;
        private GuildLandsTab? landsTab;
        private GuildResearch? researchTab;
        private GuildNodeWarsTab? nodeWarsTab;
        private GuildSettingsTab? settingsTab;

        // Cached node war data that persists across tab recreation
        private tabs.NodeWarTabData? cachedNodeWarData;

        // Clueless why this works, but prevents the issue where itemstacks render on top of other dialogs
        public override float ZSize => 1000;

        public DialogGuildMain(ICoreClientAPI capi, SOAGuildsAndKingdomsModSystem modSystem) : base(capi)
        {
            this.modSystem = modSystem;

            // Subscribe to guild data updates
            modSystem.OnClientGuildDataUpdated += OnGuildDataUpdated;

            // Register callback for node war data responses
            modSystem.GetNetworkHandler()?.RegisterNodeWarDataCallback(OnNodeWarDataReceived);

            SetupDialog();

            // Register with the PlotMapLayer for map-based claiming
            var plotLayer = modSystem.GetPlotLayer();
            System.Diagnostics.Debug.WriteLine($"DialogGuildMain constructor - plotLayer: {(plotLayer != null ? $"found (hash: {plotLayer.GetHashCode()})" : "null")}");
            System.Diagnostics.Debug.WriteLine($"DialogGuildMain constructor - this dialog hash: {this.GetHashCode()}");
            plotLayer?.SetActiveGuildDialog(this);
        }

        public override string ToggleKeyCombinationCode => "guildmain";

        private void SetupDialog()
        {
            // Get current player's guild
            currentGuild = modSystem.GetCurrentPlayerGuildSummary();

            if (currentGuild == null)
            {
                // Player is not in a guild
                SetupNoGuildDialog();
                return;
            }

            // Initialize pending changes with current values if not already set
            if (pendingGuildName == null)
                pendingGuildName = currentGuild.Name;
            if (pendingDescription == null)
                pendingDescription = currentGuild.Description;
            if (pendingPrimaryColor == null)
                pendingPrimaryColor = ColorToHex(currentGuild.DisplayColor);
            if (pendingSecondaryColor == null)
                pendingSecondaryColor = ColorToHex(currentGuild.SecondaryColor);

            // Initialize tab content classes
            InitializeTabContents();

            ElementBounds dialogBounds = ElementStdBounds.AutosizedMainDialog.WithAlignment(EnumDialogArea.CenterMiddle);
            ElementBounds bgBounds = ElementBounds.Fill.WithFixedPadding(GuiStyle.ElementToDialogPadding);
            bgBounds.BothSizing = ElementSizing.FitToChildren;

            var composer = capi.Gui.CreateCompo("guildmain", dialogBounds)
                .AddShadedDialogBG(bgBounds)
                .AddDialogTitleBar(Lang.Get("soaguildsandkingdoms:guild-main-title", currentGuild.Name), OnTitleBarCloseClicked)
                .BeginChildElements(bgBounds);

            var top = 20.0;
            var tabHeight = 30.0;
            var tabWidth = 120.0;
            var contentTop = top + tabHeight + 10.0;

            // Add tab buttons
            composer.AddSmallButton("Overview", OnOverviewTab,
                ElementBounds.Fixed(0, top, tabWidth, tabHeight),
                currentTab == TAB_OVERVIEW ? EnumButtonStyle.MainMenu : EnumButtonStyle.Normal);

            composer.AddSmallButton("Members", OnMembersTab,
                ElementBounds.Fixed(tabWidth + 5, top, tabWidth, tabHeight),
                currentTab == TAB_MEMBERS ? EnumButtonStyle.MainMenu : EnumButtonStyle.Normal);

            composer.AddSmallButton("Lands", OnLandsTab,
                ElementBounds.Fixed((tabWidth + 5) * 2, top, tabWidth, tabHeight),
                currentTab == TAB_LANDS ? EnumButtonStyle.MainMenu : EnumButtonStyle.Normal);

            composer.AddSmallButton("Research", OnResearchTab,
                ElementBounds.Fixed((tabWidth + 5) * 3, top, tabWidth, tabHeight),
                currentTab == TAB_RESEARCH ? EnumButtonStyle.MainMenu : EnumButtonStyle.Normal);

            composer.AddSmallButton("Node Wars", OnNodeWarsTab,
                ElementBounds.Fixed((tabWidth + 5) * 4, top, tabWidth, tabHeight),
                currentTab == TAB_NODEWARS ? EnumButtonStyle.MainMenu : EnumButtonStyle.Normal);

            // Add Quests tab (always visible to all guild members)
            if (HasManagePermissions())
            {
                composer.AddSmallButton("Settings", OnSettingsTab,
                    ElementBounds.Fixed((tabWidth + 5) * 5, top, tabWidth, tabHeight),
                    currentTab == TAB_SETTINGS ? EnumButtonStyle.MainMenu : EnumButtonStyle.Normal);
            }

            // Add tab content based on current tab
            switch (currentTab)
            {
                case TAB_OVERVIEW:
                    overviewTab?.AddContent(composer, contentTop);
                    break;
                case TAB_MEMBERS:
                    membersTab?.AddContent(composer, contentTop);
                    break;
                case TAB_LANDS:
                    landsTab?.AddContent(composer, contentTop);
                    break;
                case TAB_RESEARCH:
                    researchTab?.AddContent(composer, contentTop);
                    break;
                case TAB_NODEWARS:
                    nodeWarsTab?.AddContent(composer, contentTop);
                    break;
                case TAB_SETTINGS:
                    settingsTab?.AddContent(composer, contentTop);
                    break;
            }

            SingleComposer = composer.Compose();
        }

        /// <summary>
        /// Refresh node wars data from the PVP mod
        /// </summary>
        private void RefreshNodeWarsData()
        {
            if (currentGuild == null || nodeWarsTab == null) return;

            // Request node war data from server via network packet
            // The server will check if PVP mod is available and respond with data
            modSystem.GetNetworkHandler()?.RequestNodeWarData(currentGuild.Name);

            capi.Logger.Notification($"[Guild UI] Requested node wars data for guild: {currentGuild.Name}");
        }

        private void InitializeTabContents()
        {
            // Dispose old research tab before creating a new one to prevent memory leaks
            if (researchTab != null)
            {
                researchTab.Dispose();
                researchTab = null;
            }

            // Initialize overview tab
            overviewTab = new GuildOverviewTab(capi, modSystem, currentGuild, () => OnLeaveGuild());

            // Initialize members tab
            membersTab = new GuildMembersTab(capi, modSystem, currentGuild,
                () => OnViewMembers(), () => OnInvitePlayer(), () => OnManageRoles(), () => OnManagePendingInvites());

            // Initialize lands tab with save pending claims and create outpost callbacks
            landsTab = new GuildLandsTab(capi, modSystem, currentGuild,
                () => OnClaimingToggle(), () => OnSavePendingClaims(), () => OnShowHologram(), () => OnCreateOutpost(),
                () => OnUnclaimingToggle(), () => OnSavePendingUnclaims(), () => OnClaimCurrentChunk(), () => OnUnclaimCurrentChunk());
            landsTab.SetClaimingMode(isClaimingMode);
            landsTab.SetUnclaimingMode(isUnclaimingMode);
            landsTab.SetPendingClaims(pendingClaims);
            landsTab.SetPendingUnclaims(pendingUnclaims);

            // Initialize research tab with refresh callback
            researchTab = new GuildResearch(capi, modSystem, currentGuild, () => { SetupDialog(); return true; });

            // Initialize node wars tab with action callbacks
            nodeWarsTab = new GuildNodeWarsTab(capi, modSystem, currentGuild,
                () => OnNodeWarSignup(),
                () => OnNodeWarCancelSignup(),
                () => OnNodeWarJoin(),
                () => OnNodeWarViewDetails());

            // If we have cached node war data, restore it to the new tab instance
            if (cachedNodeWarData != null)
            {
                nodeWarsTab.SetNodeWarData(cachedNodeWarData);
            }

            // DO NOT fetch node wars data here - it will be fetched when switching to the tab
            // to prevent infinite request loops

            // Initialize settings tab without cancel callback
            settingsTab = new GuildSettingsTab(capi, modSystem, currentGuild,
                OnGuildNameChanged, OnDescriptionChanged, OnPrimaryColorChanged,
                OnSecondaryColorChanged, () => OnSaveSettings(), () => OnCloseDialog());
            settingsTab.SetPendingValues(pendingGuildName, pendingDescription, pendingPrimaryColor, pendingSecondaryColor);
        }

        // Tab switching methods
        private bool OnOverviewTab()
        {
            currentTab = TAB_OVERVIEW;
            SetupDialog();
            return true;
        }

        private bool OnMembersTab()
        {
            currentTab = TAB_MEMBERS;
            SetupDialog();
            return true;
        }

        private bool OnLandsTab()
        {
            currentTab = TAB_LANDS;
            SetupDialog();
            return true;
        }

        private bool OnResearchTab()
        {
            currentTab = TAB_RESEARCH;
            SetupDialog();
            return true;
        }

        private bool OnNodeWarsTab()
        {
            currentTab = TAB_NODEWARS;

            // Request fresh node wars data when switching to this tab
            RefreshNodeWarsData();

            SetupDialog();
            return true;
        }

        private bool OnSettingsTab()
        {
            if (HasManagePermissions())
            {
                currentTab = TAB_SETTINGS;
                SetupDialog();
            }
            return true;
        }

        private void SetupNoGuildDialog()
        {
            // Define dialog bounds - smaller now without player selection
            ElementBounds dialogBounds = ElementStdBounds.AutosizedMainDialog.WithAlignment(EnumDialogArea.CenterMiddle);

            // Background bounds
            ElementBounds bgBounds = ElementBounds.Fill.WithFixedPadding(GuiStyle.ElementToDialogPadding);
            bgBounds.BothSizing = ElementSizing.FitToChildren;

            var composer = capi.Gui.CreateCompo("noguild", dialogBounds)
                .AddShadedDialogBG(bgBounds)
                .AddDialogTitleBar(Lang.Get("soaguildsandkingdoms:no-guild-title"), OnTitleBarCloseClicked)
                .BeginChildElements(bgBounds);

            composer.AddStaticText(Lang.Get("soaguildsandkingdoms:no-guild-message"), CairoFont.WhiteDetailText(),
                ElementBounds.Fixed(0, 20, 400, 50));

            composer.AddSmallButton(Lang.Get("soaguildsandkingdoms:create-guild"), OnCreateGuild,
                ElementBounds.Fixed(0, 60, 120, 25), EnumButtonStyle.Normal);

            SingleComposer = composer.Compose();
        }

        private bool HasManagePermissions()
        {
            // Check if player has manage permissions (Leader or specific roles)
            // This is simplified - in a full implementation you'd check actual permissions
            return currentGuild?.PlayerRole == "Leader";
        }

        private void OnGuildNameChanged(string newName)
        {
            if (currentGuild == null) return;

            pendingGuildName = newName;
            CheckForPendingChanges();
        }

        private void OnDescriptionChanged(string newDescription)
        {
            if (currentGuild == null) return;

            pendingDescription = newDescription;
            CheckForPendingChanges();
        }

        private void OnPrimaryColorChanged(string hexColor)
        {
            if (currentGuild == null) return;

            pendingPrimaryColor = hexColor;
            CheckForPendingChanges();
        }

        private void OnSecondaryColorChanged(string hexColor)
        {
            if (currentGuild == null) return;

            pendingSecondaryColor = hexColor;
            CheckForPendingChanges();
        }

        private void CheckForPendingChanges()
        {
            if (currentGuild == null) return;

            bool nameChanged = pendingGuildName != currentGuild.Name;
            bool descriptionChanged = pendingDescription != currentGuild.Description;
            bool primaryChanged = pendingPrimaryColor != ColorToHex(currentGuild.DisplayColor);
            bool secondaryChanged = pendingSecondaryColor != ColorToHex(currentGuild.SecondaryColor);

            bool oldHasPending = hasPendingChanges;
            hasPendingChanges = nameChanged || descriptionChanged || primaryChanged || secondaryChanged;

            // Update settings tab with new pending values (without hasPendingChanges parameter)
            settingsTab?.SetPendingValues(pendingGuildName, pendingDescription, pendingPrimaryColor, pendingSecondaryColor);

            // Update lands tab with current pending claims
            landsTab?.SetPendingClaims(pendingClaims);
        }

        private bool OnClaimingToggle()
        {
            // Can't be in both modes at once
            if (isUnclaimingMode)
            {
                isUnclaimingMode = false;
                landsTab?.SetUnclaimingMode(false);
            }

            isClaimingMode = !isClaimingMode;

            if (isClaimingMode)
            {
                // Different message for first claim (guild home) vs regular claims
                if (currentGuild?.Claims.Count == 0)
                {
                    capi.ShowChatMessage(Lang.Get("soaguildsandkingdoms:guild-home-mode-enabled-map"));
                }
                else
                {
                    capi.ShowChatMessage(Lang.Get("soaguildsandkingdoms:claiming-mode-enabled-map"));
                }

                // Enable map-based claiming mode
                // This will be handled by the PlotMapLayer
            }
            else
            {
                // Disable claiming mode
                capi.ShowChatMessage(Lang.Get("soaguildsandkingdoms:claiming-mode-disabled"));
            }

            // Update the lands tab with new claiming mode state
            landsTab?.SetClaimingMode(isClaimingMode);

            // Recreate dialog to update button text and show pending claims
            SetupDialog();

            return true;
        }

        private bool OnUnclaimingToggle()
        {
            // Can't be in both modes at once
            if (isClaimingMode)
            {
                isClaimingMode = false;
                landsTab?.SetClaimingMode(false);
            }

            isUnclaimingMode = !isUnclaimingMode;

            if (isUnclaimingMode)
            {
                capi.ShowChatMessage(Lang.Get("soaguildsandkingdoms:unclaiming-mode-enabled"));
            }
            else
            {
                capi.ShowChatMessage(Lang.Get("soaguildsandkingdoms:unclaiming-mode-disabled"));
            }

            landsTab?.SetUnclaimingMode(isUnclaimingMode);
            SetupDialog();

            return true;
        }

        private bool OnClaimCurrentChunk()
        {
            if (currentGuild == null) return true;

            // Get player's current position
            var playerPos = capi.World.Player.Entity.Pos.AsBlockPos;
            int chunkSize = guilds.LandClaim.ChunkSize;

            // Calculate chunk coordinates from player position
            int chunkX = guilds.LandClaim.FloorDiv(playerPos.X, chunkSize);
            int chunkZ = guilds.LandClaim.FloorDiv(playerPos.Z, chunkSize);

            // Use unified claiming logic - send immediately to server
            ProcessChunkClaim(chunkX, chunkZ, sendImmediately: true);

            return true;
        }

        private bool OnUnclaimCurrentChunk()
        {
            if (currentGuild == null) return true;

            // Get player's current position
            var playerPos = capi.World.Player.Entity.Pos.AsBlockPos;
            int chunkSize = guilds.LandClaim.ChunkSize;

            // Calculate chunk coordinates from player position
            int chunkX = guilds.LandClaim.FloorDiv(playerPos.X, chunkSize);
            int chunkZ = guilds.LandClaim.FloorDiv(playerPos.Z, chunkSize);

            // Check if chunk is actually claimed by the guild
            var existingClaim = currentGuild.Claims.FirstOrDefault(c => c.ChunkX == chunkX && c.ChunkZ == chunkZ);
            if (existingClaim == null)
            {
                capi.ShowChatMessage(Lang.Get("soaguildsandkingdoms:chunk-not-claimed"));
                return true;
            }

            // Prevent unclaiming guild home
            /*if (existingClaim.IsGuildHome)
            {
                capi.ShowChatMessage(Lang.Get("soaguildsandkingdoms:cannot-unclaim-guild-home"));
                return true;
            }*/

            // Prevent unclaiming outposts
            /*if (existingClaim.IsOutpost)
            {
                capi.ShowChatMessage(Lang.Get("soaguildsandkingdoms:cannot-unclaim-outpost"));
                return true;
            }*/

            // Send unclaim request immediately to server
            var networkHandler = modSystem.GetNetworkHandler();
            if (networkHandler == null)
            {
                capi.ShowChatMessage(Lang.Get("soaguildsandkingdoms:network-error"));
                return true;
            }

            networkHandler.SendGuildUnclaimLandRequest(chunkX, chunkZ);
            capi.ShowChatMessage(Lang.Get("soaguildsandkingdoms:unclaim-submitted", chunkX, chunkZ));

            return true;
        }

        // Method to be called by PlotMapLayer when a chunk is clicked during claiming mode
        public void OnMapChunkClaimed(int chunkX, int chunkZ)
        {
            // Map-based claiming requires claiming mode to be active
            if (!isClaimingMode || currentGuild == null) return;

            // Use unified claiming logic - add to pending claims
            ProcessChunkClaim(chunkX, chunkZ, sendImmediately: false);
        }

        /// <summary>
        /// Unified logic for processing a chunk claim. Validates and either sends immediately or adds to pending.
        /// </summary>
        /// <param name="chunkX">Chunk X coordinate</param>
        /// <param name="chunkZ">Chunk Z coordinate</param>
        /// <param name="sendImmediately">If true, sends to server immediately. If false, adds to pending claims.</param>
        private void ProcessChunkClaim(int chunkX, int chunkZ, bool sendImmediately)
        {
            if (currentGuild == null) return;

            // Validate: Check if chunk is already claimed by this guild
            bool alreadyClaimed = currentGuild.Claims.Any(c => c.ChunkX == chunkX && c.ChunkZ == chunkZ);
            if (alreadyClaimed)
            {
                capi.ShowChatMessage(Lang.Get("soaguildsandkingdoms:chunk-already-claimed"));
                return;
            }

            // Validate: Check if chunk is already in pending claims (only relevant for pending mode)
            if (!sendImmediately)
            {
                bool alreadyPending = pendingClaims.Any(p => p.chunkX == chunkX && p.chunkZ == chunkZ);
                if (alreadyPending)
                {
                    capi.ShowChatMessage(Lang.Get("soaguildsandkingdoms:chunk-already-pending"));
                    return;
                }
            }

            // Get all guilds for validation
            var allGuilds = modSystem.GetClientGuildSummaries();

            // Check if guild needs a home
            bool needsGuildHome = !currentGuild.Claims.Any(c => c.IsGuildHome) &&
                                  (!sendImmediately ? pendingClaims.Count == 0 : true);

            if (needsGuildHome)
            {
                // Validate: Check if 2x2 area is available for guild home
                var homeChunks = new List<(int, int)>
                {
                    (chunkX, chunkZ),
                    (chunkX + 1, chunkZ),
                    (chunkX, chunkZ + 1),
                    (chunkX + 1, chunkZ + 1)
                };

                bool anyHomeChunkClaimed = homeChunks.Any(homeChunk =>
                    allGuilds.Any(guild => guild.Claims.Any(claim =>
                        claim.ChunkX == homeChunk.Item1 && claim.ChunkZ == homeChunk.Item2)));

                if (anyHomeChunkClaimed)
                {
                    capi.ShowChatMessage(Lang.Get("soaguildsandkingdoms:guild-home-area-blocked"));
                    return;
                }

                // Process guild home claim
                if (sendImmediately)
                {
                    var networkHandler = modSystem.GetNetworkHandler();
                    if (networkHandler == null)
                    {
                        capi.ShowChatMessage(Lang.Get("soaguildsandkingdoms:network-error"));
                        return;
                    }

                    networkHandler.SendGuildClaimLandRequest(chunkX, chunkZ);
                    capi.ShowChatMessage(Lang.Get("soaguildsandkingdoms:guild-home-claim-submitted", chunkX, chunkZ));
                }
                else
                {
                    pendingClaims.AddRange(homeChunks);
                    capi.ShowChatMessage(Lang.Get("soaguildsandkingdoms:guild-home-area-added-to-pending", chunkX, chunkZ));
                }
            }
            else
            {
                // Validate: Check adjacency to existing claims (excluding outpost claims for expansion)
                // Regular claims can only be adjacent to non-outpost claims
                var nonOutpostClaims = currentGuild.Claims.Where(c => !c.IsOutpost).ToList();
                bool adjacentToExisting = IsChunkAdjacentToAnyClaim(nonOutpostClaims, chunkX, chunkZ);

                // For pending mode, also check adjacency to pending claims
                bool adjacentToPending = !sendImmediately &&
                                        pendingClaims.Any(p => IsAdjacent(p.chunkX, p.chunkZ, chunkX, chunkZ));

                if (!adjacentToExisting && !adjacentToPending)
                {
                    capi.ShowChatMessage(Lang.Get("soaguildsandkingdoms:claim-must-be-adjacent"));
                    return;
                }

                // Validate: Check that we're not trying to expand from an outpost
                // Find which claim this chunk would be adjacent to
                var adjacentClaim = currentGuild.Claims.FirstOrDefault(c => IsAdjacent(c.ChunkX, c.ChunkZ, chunkX, chunkZ));
                if (adjacentClaim != null && adjacentClaim.IsOutpost)
                {
                    capi.ShowChatMessage(Lang.Get("soaguildsandkingdoms:cannot-expand-from-outpost"));
                    return;
                }

                // Validate: For immediate send, check if already claimed by any guild
                if (sendImmediately)
                {
                    bool claimedByAnyGuild = allGuilds.Any(guild =>
                        guild.Claims.Any(claim => claim.ChunkX == chunkX && claim.ChunkZ == chunkZ));

                    if (claimedByAnyGuild)
                    {
                        capi.ShowChatMessage(Lang.Get("soaguildsandkingdoms:chunk-already-claimed-by-other"));
                        return;
                    }
                }

                // Process regular claim
                if (sendImmediately)
                {
                    var networkHandler = modSystem.GetNetworkHandler();
                    if (networkHandler == null)
                    {
                        capi.ShowChatMessage(Lang.Get("soaguildsandkingdoms:network-error"));
                        return;
                    }

                    networkHandler.SendGuildClaimLandRequest(chunkX, chunkZ);
                    capi.ShowChatMessage(Lang.Get("soaguildsandkingdoms:claim-submitted", chunkX, chunkZ));
                }
                else
                {
                    pendingClaims.Add((chunkX, chunkZ));
                    capi.ShowChatMessage(Lang.Get("soaguildsandkingdoms:claim-added-to-pending", chunkX, chunkZ));
                }
            }

            // Update UI - always refresh to show latest state
            if (!sendImmediately)
            {
                // For pending mode, update the pending claims list
                CheckForPendingChanges();
                landsTab?.SetPendingClaims(pendingClaims);
            }

            // Always refresh the dialog to show updated UI
            SetupDialog();
        }

        // Helper method to check if two chunks are adjacent
        private bool IsAdjacent(int chunkX1, int chunkZ1, int chunkX2, int chunkZ2)
        {
            int deltaX = Math.Abs(chunkX1 - chunkX2);
            int deltaZ = Math.Abs(chunkZ1 - chunkZ2);
            return (deltaX == 1 && deltaZ == 0) || (deltaX == 0 && deltaZ == 1);
        }

        // Helper method to check if a chunk is adjacent to any claim in the list
        private bool IsChunkAdjacentToAnyClaim(List<LandClaimDto> claims, int targetChunkX, int targetChunkZ)
        {
            return claims.Any(claim => IsAdjacent(claim.ChunkX, claim.ChunkZ, targetChunkX, targetChunkZ));
        }

        // Public property to check if claiming mode is active (for PlotMapLayer)
        public bool IsClaimingModeActive => isClaimingMode;

        // Public property to check if unclaiming mode is active (for PlotMapLayer)
        public bool IsUnclaimingModeActive => isUnclaimingMode;

        // Public property to get pending claims (for PlotMapLayer to highlight them)
        public List<(int chunkX, int chunkZ)> GetPendingClaims() => new List<(int, int)>(pendingClaims);

        // Public property to get pending unclaims (for PlotMapLayer to highlight them)
        public List<(int chunkX, int chunkZ)> GetPendingUnclaims() => new List<(int, int)>(pendingUnclaims);

        // Method to be called by PlotMapLayer when a chunk is clicked during unclaiming mode
        public void OnMapChunkUnclaimed(int chunkX, int chunkZ)
        {
            if (!isUnclaimingMode || currentGuild == null) return;

            // Check if chunk is actually claimed by the guild
            var existingClaim = currentGuild.Claims.FirstOrDefault(c => c.ChunkX == chunkX && c.ChunkZ == chunkZ);
            if (existingClaim == null)
            {
                capi.ShowChatMessage(Lang.Get("soaguildsandkingdoms:chunk-not-claimed"));
                return;
            }

            // Prevent unclaiming guild home
            /*if (existingClaim.IsGuildHome)
            {
                capi.ShowChatMessage(Lang.Get("soaguildsandkingdoms:cannot-unclaim-guild-home"));
                return;
            }*/

            // Prevent unclaiming outposts
            /*if (existingClaim.IsOutpost)
            {
                capi.ShowChatMessage(Lang.Get("soaguildsandkingdoms:cannot-unclaim-outpost"));
                return;
            }*/

            // Check if already in pending unclaims
            if (pendingUnclaims.Any(p => p.chunkX == chunkX && p.chunkZ == chunkZ))
            {
                capi.ShowChatMessage(Lang.Get("soaguildsandkingdoms:chunk-already-pending-unclaim"));
                return;
            }

            pendingUnclaims.Add((chunkX, chunkZ));
            capi.ShowChatMessage(Lang.Get("soaguildsandkingdoms:unclaim-added-to-pending", chunkX, chunkZ));

            landsTab?.SetPendingUnclaims(pendingUnclaims);
            SetupDialog();
        }

        /// <summary>
        /// Check if a specific chunk in pending claims would be part of a guild home
        /// </summary>
        public bool IsPendingGuildHomeChunk(int chunkX, int chunkZ)
        {
            if (currentGuild == null) return false;

            // If the guild already has a home, no pending claims can be guild home chunks
            if (currentGuild.Claims.Any(c => c.IsGuildHome)) return false;

            // If this chunk is not in pending claims, it's not a guild home chunk
            if (!pendingClaims.Any(p => p.chunkX == chunkX && p.chunkZ == chunkZ)) return false;

            // Check if this chunk is part of the first 4 pending claims (which would be the 2x2 guild home)
            if (pendingClaims.Count >= 4)
            {
                // Find the potential guild home pattern in the first 4 pending claims
                var firstFour = pendingClaims.Take(4).ToList();

                // Check if these form a valid 2x2 grid starting from the first chunk
                if (IsValid2x2Pattern(firstFour))
                {
                    return firstFour.Any(p => p.chunkX == chunkX && p.chunkZ == chunkZ);
                }
            }

            return false;
        }

        /// <summary>
        /// Checks if the given chunks form a valid 2x2 pattern
        /// </summary>
        private bool IsValid2x2Pattern(List<(int chunkX, int chunkZ)> chunks)
        {
            if (chunks.Count != 4) return false;

            // Sort chunks to find the bottom-left corner
            var sorted = chunks.OrderBy(c => c.chunkX).ThenBy(c => c.chunkZ).ToList();

            // Check if they form a 2x2 grid starting from the first chunk
            var expected = new List<(int, int)>
            {
                (sorted[0].chunkX, sorted[0].chunkZ),
                (sorted[0].chunkX + 1, sorted[0].chunkZ),
                (sorted[0].chunkX, sorted[0].chunkZ + 1),
                (sorted[0].chunkX + 1, sorted[0].chunkZ + 1)
            };

            expected.Sort();
            var actual = chunks.Select(c => (c.chunkX, c.chunkZ)).ToList();
            actual.Sort();

            return expected.SequenceEqual(actual);
        }

        // New method to save only pending land claims
        private bool OnSavePendingClaims()
        {
            if (currentGuild == null || pendingClaims.Count == 0) return true;

            var networkHandler = modSystem.GetNetworkHandler();
            if (networkHandler != null)
            {
                foreach (var (chunkX, chunkZ) in pendingClaims)
                {
                    // Send chunk coordinates directly - no unnecessary conversion
                    networkHandler.SendGuildClaimLandRequest(chunkX, chunkZ);
                }
                capi.ShowChatMessage(Lang.Get("soaguildsandkingdoms:claims-submitted", pendingClaims.Count));
                pendingClaims.Clear();

                // Stop claiming mode after claims are saved
                isClaimingMode = false;
                landsTab?.SetClaimingMode(isClaimingMode);

                // Update pending changes status since claims are cleared
                CheckForPendingChanges();

                // Don't refresh the dialog immediately - let the server response trigger the refresh
                // when updated guild data arrives. This prevents showing stale data during the brief
                // network delay.
            }
            return true;
        }

        private bool OnSavePendingUnclaims()
        {
            if (currentGuild == null || pendingUnclaims.Count == 0) return true;

            var networkHandler = modSystem.GetNetworkHandler();
            if (networkHandler != null)
            {
                foreach (var (chunkX, chunkZ) in pendingUnclaims)
                {
                    networkHandler.SendGuildUnclaimLandRequest(chunkX, chunkZ);
                }
                capi.ShowChatMessage(Lang.Get("soaguildsandkingdoms:unclaims-submitted", pendingUnclaims.Count));
                pendingUnclaims.Clear();
                isUnclaimingMode = false;

                landsTab?.SetUnclaimingMode(false);
                landsTab?.SetPendingUnclaims(pendingUnclaims);
                SetupDialog();
            }
            return true;
        }

        // New method to save only guild settings (without closing)
        private bool OnSaveSettings()
        {
            if (currentGuild == null) return true;

            // Check for any settings changes
            bool hasSettingsChanges = pendingGuildName != currentGuild.Name ||
                                    pendingDescription != currentGuild.Description ||
                                    pendingPrimaryColor != ColorToHex(currentGuild.DisplayColor) ||
                                    pendingSecondaryColor != ColorToHex(currentGuild.SecondaryColor);

            if (!hasSettingsChanges)
            {
                capi.ShowChatMessage(Lang.Get("soaguildsandkingdoms:no-changes-to-save"));
                return true;
            }

            // Validate colors before saving
            if (pendingPrimaryColor != ColorToHex(currentGuild.DisplayColor))
            {
                if (!TryParseHexColor(pendingPrimaryColor ?? "", out int _))
                {
                    capi.ShowChatMessage(Lang.Get("soaguildsandkingdoms:invalid-primary-color"));
                    return false; // Don't save if validation fails
                }
            }

            if (pendingSecondaryColor != ColorToHex(currentGuild.SecondaryColor))
            {
                if (!TryParseHexColor(pendingSecondaryColor ?? "", out int _))
                {
                    capi.ShowChatMessage(Lang.Get("soaguildsandkingdoms:invalid-secondary-color"));
                    return false; // Don't save if validation fails
                }
            }

            var networkHandler = modSystem.GetNetworkHandler();
            if (networkHandler == null)
            {
                capi.ShowChatMessage(Lang.Get("soaguildsandkingdoms:network-error"));
                return false;
            }

            // Send guild name change packet
            if (pendingGuildName != currentGuild.Name && !string.IsNullOrWhiteSpace(pendingGuildName))
            {
                var nameChangePacket = new GuildCommandPacket
                {
                    PlayerUid = capi.World.Player.PlayerUID,
                    Command = "changename",
                    Arguments = new[] { pendingGuildName }
                };
                capi.Network.GetChannel("soaguildsandkingdoms:guild").SendPacket(nameChangePacket);
            }

            // Send guild description change packet
            if (pendingDescription != currentGuild.Description)
            {
                var descriptionChangePacket = new GuildCommandPacket
                {
                    PlayerUid = capi.World.Player.PlayerUID,
                    Command = "changedescription",
                    Arguments = new[] { pendingDescription ?? "" }
                };
                capi.Network.GetChannel("soaguildsandkingdoms:guild").SendPacket(descriptionChangePacket);
            }

            // Send primary color change packet
            if (pendingPrimaryColor != ColorToHex(currentGuild.DisplayColor))
            {
                if (TryParseHexColor(pendingPrimaryColor ?? "", out int primaryColor))
                {
                    var primaryColorPacket = new GuildCommandPacket
                    {
                        PlayerUid = capi.World.Player.PlayerUID,
                        Command = "changecolor",
                        Arguments = new[] { primaryColor.ToString() }
                    };
                    capi.Network.GetChannel("soaguildsandkingdoms:guild").SendPacket(primaryColorPacket);
                }
            }

            // Send secondary color change packet
            if (pendingSecondaryColor != ColorToHex(currentGuild.SecondaryColor))
            {
                if (TryParseHexColor(pendingSecondaryColor ?? "", out int secondaryColor))
                {
                    var secondaryColorPacket = new GuildCommandPacket
                    {
                        PlayerUid = capi.World.Player.PlayerUID,
                        Command = "changesecondarycolor",
                        Arguments = new[] { secondaryColor.ToString() }
                    };
                    capi.Network.GetChannel("soaguildsandkingdoms:guild").SendPacket(secondaryColorPacket);
                }
            }

            capi.ShowChatMessage(Lang.Get("soaguildsandkingdoms:settings-saved"));

            // The guild data will be updated when the server sends back the updated guild summary
            // For now, we can clear the pending changes flag since we've submitted the changes
            hasPendingChanges = false;
            CheckForPendingChanges();

            // Refresh the dialog to update button states
            SetupDialog();
            return true;
        }

        // New method to close the dialog without saving
        private bool OnCloseDialog()
        {
            TryClose();
            return true;
        }

        private bool OnInvitePlayer()
        {
            // Open invite player dialog
            var inviteDialog = new DialogGuildInvitePlayer(capi, modSystem);
            inviteDialog.TryOpen();
            return true;
        }

        private bool OnManageRoles()
        {
            // Open role management dialog
            var rolesDialog = new DialogGuildManageRoles(capi, modSystem);
            rolesDialog.TryOpen();
            return true;
        }

		private bool OnViewMembers()
		{
			// Open members list dialog
			var membersDialog = new DialogGuildMembers(capi, modSystem);
			membersDialog.TryOpen();
			return true;
		}
        
		private bool OnManagePendingInvites()
		{
			// Open pending invites management dialog
			var invitesDialog = new DialogGuildPendingInvites(capi, modSystem);
			invitesDialog.TryOpen();
			return true;
		}

		private bool OnNodeWarSignup()
		{
			if (currentGuild == null) return false;

			// The selected war ID should be set in nodeWarData.SelectedWarForSignup
			// by the button click in the tab
			var selectedNodeId = nodeWarsTab?.GetSelectedWarForSignup();
			if (string.IsNullOrEmpty(selectedNodeId))
			{
				capi.ShowChatMessage("No war selected for signup");
				return false;
			}

			// Get the PVP mod using reflection since we don't have a direct reference
			var pvpMod = capi.ModLoader.GetModSystem("SOAGuildsAndKingdomsPVP.PVPModSystem");
			if (pvpMod == null)
			{
				capi.ShowChatMessage("PVP mod not loaded");
				return false;
			}

			// Call RequestGuildSignup method via reflection
			var networkHandlerMethod = pvpMod.GetType().GetMethod("GetNetworkHandler");
			var networkHandler = networkHandlerMethod?.Invoke(pvpMod, null);

			if (networkHandler == null)
			{
				capi.ShowChatMessage("PVP mod network handler not available");
				return false;
			}

			var requestMethod = networkHandler.GetType().GetMethod("RequestGuildSignup");
			requestMethod?.Invoke(networkHandler, new object[] { selectedNodeId });

			capi.ShowChatMessage($"Signing up for node war...");

			// Refresh node war data after a short delay to show the updated signup status
			capi.Event.RegisterCallback((dt) =>
			{
				RefreshNodeWarsData();
			}, 1000);

			return true;
		}

        private bool OnNodeWarCancelSignup()
        {
            if (currentGuild == null) return false;

            // Get current signup from tab data
            var currentSignup = nodeWarsTab?.GetCurrentSignup();
            if (currentSignup == null)
            {
                capi.ShowChatMessage("No active signup to cancel");
                return false;
            }

            // Get the PVP mod using reflection since we don't have a direct reference
            var pvpMod = capi.ModLoader.GetModSystem("SOAGuildsAndKingdomsPVP.PVPModSystem");
            if (pvpMod == null)
            {
                capi.ShowChatMessage("PVP mod not loaded");
                return false;
            }

            // Call RequestCancelGuildSignup method via reflection
            var networkHandlerMethod = pvpMod.GetType().GetMethod("GetNetworkHandler");
            var networkHandler = networkHandlerMethod?.Invoke(pvpMod, null);

            if (networkHandler == null)
            {
                capi.ShowChatMessage("PVP mod network handler not available");
                return false;
            }

            var requestMethod = networkHandler.GetType().GetMethod("RequestCancelGuildSignup");
            requestMethod?.Invoke(networkHandler, new object[] { currentSignup.NodeId });

            capi.ShowChatMessage($"Cancelling signup for {currentSignup.NodeName}...");

            // Refresh node war data after a short delay
            capi.Event.RegisterCallback((dt) =>
            {
                RefreshNodeWarsData();
            }, 1000);

            return true;
        }

        private bool OnNodeWarJoin()
        {
            if (currentGuild == null) return false;

            // Get current war from tab data  
            var currentWar = nodeWarsTab?.GetCurrentWar();
            if (currentWar == null)
            {
                capi.ShowChatMessage("No active war to join");
                return false;
            }

            // Send join request to server
            var networkHandler = modSystem.GetNetworkHandler();
            if (networkHandler != null)
            {
                capi.ShowChatMessage($"Joining war at {currentWar.NodeName}...");
                // TODO: Implement network packet for join war
            }

            return true;
        }

        private bool OnNodeWarViewDetails()
        {
            if (currentGuild == null) return false;

            // Get current war from tab data
            var currentWar = nodeWarsTab?.GetCurrentWar();
            if (currentWar == null)
            {
                capi.ShowChatMessage("No active war to view");
                return false;
            }

            // For now just show a message with basic info
            capi.ShowChatMessage($"War Details: {currentWar.NodeName} - Status: {currentWar.Status}");
            // TODO: Implement detailed war status dialog
            return true;
        }

        private bool OnCreateGuild()
        {
            // Create a simple input dialog for guild name
            var createDialog = new DialogCreateGuild(capi, modSystem);
            createDialog.TryOpen();
            TryClose();
            return true;
        }

        // Add this new method to handle the Leave Guild button click
        private bool OnLeaveGuild()
        {
            if (currentGuild == null) return false;

            // Show confirmation dialog for leaving guild
            string confirmMessage;
            if (currentGuild.PlayerRole == "Leader")
            {
                if (currentGuild.MemberCount == 1)
                {
                    confirmMessage = Lang.Get("soaguildsandkingdoms:confirm-disband-guild", currentGuild.Name);
                }
                else
                {
                    // Leader with other members - show error message
                    capi.ShowChatMessage(Lang.Get("soaguildsandkingdoms:leader-cannot-leave"));
                    return true;
                }
            }
            else
            {
                confirmMessage = Lang.Get("soaguildsandkingdoms:confirm-leave-guild", currentGuild.Name);
            }

            // Create and show custom confirmation dialog
            var confirmDialog = new DialogGuildLeaveConfirm(capi, confirmMessage, () =>
            {
                // On confirm - send leave guild request
                var networkHandler = modSystem.GetNetworkHandler();
                if (networkHandler != null)
                {
                    networkHandler.SendGuildLeaveRequest();
                    TryClose(); // Close the main guild dialog
                }
            });

            confirmDialog.TryOpen();
            return true;
        }

        private bool OnShowHologram()
        {
            modSystem.ToggleHologram();
            return true;
        }

        private bool OnCreateOutpost()
        {
            if (currentGuild == null) return false;

            // Check if guild has a home and hasn't reached max outposts
            bool hasGuildHome = currentGuild.Claims.Any(c => c.IsGuildHome);
            var outpostCount = currentGuild.Claims.Count(c => c.IsOutpost);
            var maxOutposts = currentGuild.MaxOutposts;

            if (!hasGuildHome)
            {
                capi.ShowChatMessage(Lang.Get("soaguildsandkingdoms:outpost-requires-guild-home"));
                return true;
            }

            if (outpostCount >= maxOutposts)
            {
                capi.ShowChatMessage(Lang.Get("soaguildsandkingdoms:max-outposts-reached", outpostCount, maxOutposts));
                return true;
            }

            // Open the create outpost dialog
            var outpostDialog = new DialogCreateOutpost(capi, modSystem, (outpostName) =>
            {
                OnOutpostCreationRequested(outpostName);
            });
            outpostDialog.TryOpen();

            return true;
        }

        private void OnOutpostCreationRequested(string outpostName)
        {
            if (currentGuild == null) return;

            // Get player position to create outpost at current location
            var playerPos = capi.World.Player.Entity.Pos.AsBlockPos;
            int chunkX = SOAGuildsAndKingdoms.src.guilds.LandClaim.FloorDiv(playerPos.X, SOAGuildsAndKingdoms.src.guilds.LandClaim.ChunkSize);
            int chunkZ = SOAGuildsAndKingdoms.src.guilds.LandClaim.FloorDiv(playerPos.Z, SOAGuildsAndKingdoms.src.guilds.LandClaim.ChunkSize);

            // Check if this chunk is already claimed
            var allGuilds = modSystem.GetClientGuildSummaries();
            bool alreadyClaimed = allGuilds.Any(guild => guild.Claims.Any(claim => claim.ChunkX == chunkX && claim.ChunkZ == chunkZ));

            if (alreadyClaimed)
            {
                capi.ShowChatMessage(Lang.Get("soaguildsandkingdoms:chunk-already-claimed-outpost", chunkX, chunkZ));
                return;
            }

            // Send outpost creation request to server
            var networkHandler = modSystem.GetNetworkHandler();
            if (networkHandler != null)
            {
                networkHandler.SendGuildClaimLandRequest(chunkX, chunkZ, true, outpostName);

                string message = string.IsNullOrEmpty(outpostName)
                    ? Lang.Get("soaguildsandkingdoms:outpost-creation-requested", chunkX, chunkZ)
                    : Lang.Get("soaguildsandkingdoms:outpost-creation-requested-named", outpostName, chunkX, chunkZ);

                capi.ShowChatMessage(message);
            }
            else
            {
                capi.ShowChatMessage(Lang.Get("soaguildsandkingdoms:network-error"));
            }
        }

        /// <summary>
        /// Called when guild data is updated from the server (e.g., after a successful claim)
        /// </summary>
        private void OnGuildDataUpdated(List<GuildSummary> updatedGuilds)
        {
            // Refresh current guild data
            var previousGuild = currentGuild;
            currentGuild = modSystem.GetCurrentPlayerGuildSummary();

            // If the dialog is open and showing guild data, refresh it
            if (IsOpened() && currentGuild != null)
            {
                // Clear pending claims if they were successfully processed
                // (They're now in the actual claims list)
                if (previousGuild != null && currentGuild.Claims.Count > previousGuild.Claims.Count)
                {
                    // Compare pending claims with new claims and remove those that were added
                    var newClaims = currentGuild.Claims
                        .Where(c => !previousGuild.Claims.Any(pc => pc.ChunkX == c.ChunkX && pc.ChunkZ == c.ChunkZ))
                        .ToList();

                    foreach (var newClaim in newClaims)
                    {
                        pendingClaims.RemoveAll(p => p.chunkX == newClaim.ChunkX && p.chunkZ == newClaim.ChunkZ);
                    }
                }

                // Clear pending unclaims if they were successfully processed
                if (previousGuild != null && currentGuild.Claims.Count < previousGuild.Claims.Count)
                {
                    // Compare pending unclaims with removed claims
                    var removedClaims = previousGuild.Claims
                        .Where(c => !currentGuild.Claims.Any(nc => nc.ChunkX == c.ChunkX && nc.ChunkZ == c.ChunkZ))
                        .ToList();

                    foreach (var removedClaim in removedClaims)
                    {
                        pendingUnclaims.RemoveAll(p => p.chunkX == removedClaim.ChunkX && p.chunkZ == removedClaim.ChunkZ);
                    }
                }

                // Update pending values with current values if not changed
                if (pendingGuildName == previousGuild?.Name)
                    pendingGuildName = currentGuild.Name;
                if (pendingPrimaryColor == ColorToHex(previousGuild?.DisplayColor ?? 0))
                    pendingPrimaryColor = ColorToHex(currentGuild.DisplayColor);
                if (pendingSecondaryColor == ColorToHex(previousGuild?.SecondaryColor ?? 0))
                    pendingSecondaryColor = ColorToHex(currentGuild.SecondaryColor);

                // Refresh the UI to show updated claims
                SetupDialog();

                // Update hologram if it's currently visible
                if (modSystem.IsHologramVisible)
                {
                    modSystem.ShowClaimsHologram();
                }
            }
        }

        private void OnTitleBarCloseClicked()
        {
            // Clean up claiming mode if active
            if (isClaimingMode)
            {
                isClaimingMode = false;
            }

            // Disconnect from PlotMapLayer
            var plotLayer = modSystem.GetPlotLayer();
            plotLayer?.SetActiveGuildDialog(null);

            // Unsubscribe from guild data updates
            modSystem.OnClientGuildDataUpdated -= OnGuildDataUpdated;

            TryClose();
        }

        /// <summary>
        /// Called when node war data is received from the server/PVP mod
        /// </summary>
        private void OnNodeWarDataReceived(SOAGuildsAndKingdoms.src.network.NodeWarDataResponsePacket packet)
        {
            if (nodeWarsTab == null) return;

            // Convert DTOs to UI data model
            var nodeWarData = new tabs.NodeWarTabData
            {
                ControlledNodes = packet.ControlledNodes.Select(dto => new tabs.ControlledNodeInfo
                {
                    NodeId = dto.NodeId,
                    NodeName = dto.NodeName,
                    CapturedAt = dto.CapturedAtTicks > 0 ? new DateTime(dto.CapturedAtTicks) : null,
                    InfluencePerDay = dto.InfluencePerDay
                }).ToList(),

                CurrentWar = packet.CurrentWar != null ? new tabs.CurrentWarInfo
                {
                    NodeId = packet.CurrentWar.NodeId,
                    NodeName = packet.CurrentWar.NodeName,
                    Status = packet.CurrentWar.Status,
                    PointsNeeded = packet.CurrentWar.PointsNeeded,
                    YourGuildProgress = packet.CurrentWar.YourGuildProgress != null ? new tabs.GuildWarProgressInfo
                    {
                        CapturePoints = packet.CurrentWar.YourGuildProgress.CapturePoints,
                        PlayersInZone = packet.CurrentWar.YourGuildProgress.PlayersInZone,
                        Kills = packet.CurrentWar.YourGuildProgress.Kills,
                        Deaths = packet.CurrentWar.YourGuildProgress.Deaths
                    } : null
                } : null,

                AvailableWars = packet.AvailableWars.Select(dto => new tabs.AvailableWarInfo
                {
                    NodeId = dto.NodeId,
                    NodeName = dto.NodeName,
                    WarStartTime = new DateTime(dto.WarStartTimeTicks),
                    CurrentSignups = dto.CurrentSignups,
                    MaxGuilds = dto.MaxGuilds,
                    CanSignup = dto.CanSignup
                }).ToList(),

                CurrentSignup = packet.CurrentSignup != null ? new tabs.CurrentSignupInfo
                {
                    NodeId = packet.CurrentSignup.NodeId,
                    NodeName = packet.CurrentSignup.NodeName,
                    SignupTime = new DateTime(packet.CurrentSignup.SignupTimeTicks),
                    WarStartTime = new DateTime(packet.CurrentSignup.WarStartTimeTicks)
                } : null
            };

            // Cache the data so it persists across tab recreation
            cachedNodeWarData = nodeWarData;

            // Update the tab with the new data
            nodeWarsTab.SetNodeWarData(nodeWarData);

            // Refresh the dialog if we're currently on the node wars tab to display the new data
            // This is safe because InitializeTabContents() no longer calls RefreshNodeWarsData(),
            // so we won't trigger an infinite request loop
            if (currentTab == TAB_NODEWARS && IsOpened())
            {
                SetupDialog();
            }

            capi.Logger.Debug($"[Guild UI] Node war data updated: {packet.ControlledNodes.Count} controlled nodes, " +
                             $"{packet.AvailableWars.Count} available wars");
        }

        public override void OnGuiClosed()
        {
            // Dispose research tab to prevent memory leaks
            if (researchTab != null)
            {
                researchTab.Dispose();
                researchTab = null;
            }

            // Clean up claiming mode when dialog is closed
            if (isClaimingMode)
            {
                isClaimingMode = false;
            }

            // Unsubscribe from guild data updates
            modSystem.OnClientGuildDataUpdated -= OnGuildDataUpdated;

            // Disconnect from PlotMapLayer
            var plotLayer = modSystem.GetPlotLayer();
            plotLayer?.SetActiveGuildDialog(null);

            // Unsubscribe from guild data updates
            modSystem.OnClientGuildDataUpdated -= OnGuildDataUpdated;

            base.OnGuiClosed();
        }

        public override void OnGuiOpened()
        {
            base.OnGuiOpened();
            // Refresh guild data when dialog opens
            currentGuild = modSystem.GetCurrentPlayerGuildSummary();
            if (currentGuild != null)
            {
                // Reset pending changes to current values
                pendingGuildName = currentGuild.Name;
                pendingPrimaryColor = ColorToHex(currentGuild.DisplayColor);
                pendingSecondaryColor = ColorToHex(currentGuild.SecondaryColor);
                hasPendingChanges = false;

                SetupDialog(); // Rebuild dialog with fresh data
            }
        }

        public override bool PrefersUngrabbedMouse => false;

        private string ColorToHex(int argbColor)
        {
            // Convert ARGB int to hex string (without alpha)
            // Use unchecked to handle negative values properly
            uint color = unchecked((uint)argbColor);
            byte r = (byte)((color >> 16) & 0xFF);
            byte g = (byte)((color >> 8) & 0xFF);
            byte b = (byte)(color & 0xFF);
            return $"#{r:X2}{g:X2}{b:X2}";
        }

        private bool TryParseHexColor(string hex, out int argbColor)
        {
            argbColor = 0;

            if (string.IsNullOrWhiteSpace(hex))
                return false;

            // Remove # if present and trim whitespace
            hex = hex.Trim();
            if (hex.StartsWith("#"))
                hex = hex.Substring(1);

            // Validate length (should be 6 for RGB or 3 for short RGB)
            if (hex.Length == 3)
            {
                // Convert short form (e.g., "F0A" -> "FF00AA")
                hex = $"{hex[0]}{hex[0]}{hex[1]}{hex[1]}{hex[2]}{hex[2]}";
            }
            else if (hex.Length != 6)
            {
                return false;
            }

            // Validate that all characters are valid hex digits
            foreach (char c in hex)
            {
                if (!IsHexDigit(c))
                    return false;
            }

            // Try to parse as hexadecimal
            try
            {
                if (uint.TryParse(hex, System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture, out uint rgb))
                {
                    // Convert RGB to ARGB with full alpha (0xFF000000 = fully opaque)
                    argbColor = unchecked((int)(0xFF000000U | rgb));
                    return true;
                }
            }
            catch
            {
                // Parsing failed
            }

            return false;
        }

        private static bool IsHexDigit(char c)
        {
            return (c >= '0' && c <= '9') ||
                   (c >= 'A' && c <= 'F') ||
                   (c >= 'a' && c <= 'f');
        }

        /// <summary>
        /// Override Dispose to clean up resources when dialog closes
        /// </summary>
        public override void Dispose()
        {
            // Dispose the research tab to drop any items in contribution slots
            if (researchTab is IDisposable disposableResearch)
            {
                disposableResearch.Dispose();
            }

            base.Dispose();
        }
    }

    /// <summary>
    /// Simple confirmation dialog for leaving guild
    /// </summary>
    internal class DialogGuildLeaveConfirm : GuiDialog
    {
        private string message;
        private Action onConfirm;

        public DialogGuildLeaveConfirm(ICoreClientAPI capi, string message, Action onConfirm) : base(capi)
        {
            this.message = message;
            this.onConfirm = onConfirm;
            SetupDialog();
        }

        public override string ToggleKeyCombinationCode => "guildleaveconfirm";

        private void SetupDialog()
        {
            ElementBounds dialogBounds = ElementStdBounds.AutosizedMainDialog.WithAlignment(EnumDialogArea.CenterMiddle);
            ElementBounds bgBounds = ElementBounds.Fill.WithFixedPadding(GuiStyle.ElementToDialogPadding);
            bgBounds.BothSizing = ElementSizing.FitToChildren;

            var composer = capi.Gui.CreateCompo("guildleaveconfirm", dialogBounds)
                .AddShadedDialogBG(bgBounds)
                .AddDialogTitleBar(Lang.Get("soaguildsandkingdoms:leave-guild-title"), OnTitleBarCloseClicked)
                .BeginChildElements(bgBounds);

            var top = 20.0;
            var spacing = 15.0;
            var elementHeight = 25.0;
            var width = 400.0;

            // Message
            composer.AddStaticText(message, CairoFont.WhiteDetailText(),
                ElementBounds.Fixed(0, top, width, elementHeight * 3));
            top += elementHeight * 3 + spacing;

            // Buttons
            composer.AddSmallButton(Lang.Get("soaguildsandkingdoms:confirm"), OnConfirmClick,
                ElementBounds.Fixed(0, top, 100, elementHeight), EnumButtonStyle.MainMenu);

            composer.AddSmallButton(Lang.Get("soaguildsandkingdoms:cancel"), OnCancelClick,
                ElementBounds.Fixed(110, top, 100, elementHeight), EnumButtonStyle.Normal);

            SingleComposer = composer.Compose();
        }

        private bool OnConfirmClick()
        {
            onConfirm?.Invoke();
            TryClose();
            return true;
        }

        private bool OnCancelClick()
        {
            TryClose();
            return true;
        }

        private void OnTitleBarCloseClicked()
        {
            TryClose();
        }

        public override bool PrefersUngrabbedMouse => false;
    }
}
