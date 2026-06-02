using System;
using System.Collections.Generic;
using System.Linq;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using SOAGuildsAndKingdomsPVP.src.pvp;
using SOAGuildsAndKingdomsPVP.src.network;
using SOAGuildsAndKingdomsPVP.src.nodewars;
using SOAGuildsAndKingdomsPVP.src.gui;
using SOAGuildsAndKingdoms;

namespace SOAGuildsAndKingdomsPVP
{
    /// <summary>
    /// Main mod system for guild-based PVP
    /// Requires the SOAGuildsAndKingdoms mod to be installed
    /// </summary>
    public class PVPModSystem : ModSystem
    {
        // Server-side
        private ICoreServerAPI? serverApi;
        private PVPManager? pvpManager;

        // Client-side
        private ICoreClientAPI? clientApi;
        private PVPStatusPacket? currentPVPStatus;
        private DialogNodeWarAdminClient? nodeWarAdminDialog;
        private bool captureZoneHologramVisible = false;
        private const int CAPTURE_ZONE_HOLOGRAM_SLOT = 98; // Arbitrary slot for capture zone hologram
        private CaptureZoneSyncPacket? cachedCaptureZones; // Cache of active capture zones
        private bool isHologramAutoManaged = false; // Whether hologram is currently auto-controlled
        private bool manualOverride = false; // Whether user manually toggled while in a zone

        // Shared
        private PVPNetworkHandler? networkHandler;

        public override void Start(ICoreAPI api)
        {
            base.Start(api);
            api.Logger.Notification("[PVP] Guild-based PVP mod initializing...");
            api.Logger.Notification("[PVP] This mod requires SOAGuildsAndKingdoms to be installed");

            networkHandler = new PVPNetworkHandler();
        }

        #region Server-side initialization

        public override void StartServerSide(ICoreServerAPI api)
        {
            base.StartServerSide(api);
            serverApi = api;

            // Verify that the guild mod is loaded
            var guildMod = api.ModLoader.GetModSystem<SOAGuildsAndKingdomsModSystem>();
            if (guildMod == null)
            {
                api.Logger.Error("[PVP] CRITICAL ERROR: SOAGuildsAndKingdoms mod is not loaded!");
                api.Logger.Error("[PVP] This mod cannot function without SOAGuildsAndKingdoms.");
                api.Logger.Error("[PVP] Please install SOAGuildsAndKingdoms mod (modid: soaguildsandkingdoms)");
                return;
            }

            api.Logger.Notification("[PVP] SOAGuildsAndKingdoms mod detected - initializing guild-based PVP");

            // Initialize PVP manager
            pvpManager = new PVPManager(api);

            // Initialize networking
            networkHandler?.InitializeServer(api, pvpManager);

            // Store reference to guild mod for node war data requests
            // The guild mod's network handler will call our OnNodeWarDataRequested method directly
            guildMod.RegisterNodeWarDataProvider(this);

            // Register event handlers
            api.Event.PlayerJoin += OnPlayerJoin;
            api.Event.PlayerDisconnect += OnPlayerDisconnect;
            api.Event.SaveGameLoaded += OnSaveGameLoaded;
            api.Event.GameWorldSave += OnGameWorldSave;
            api.Event.PlayerDeath += OnPlayerDeath;

            // Register commands
            RegisterCommands(api);

            api.Logger.Notification("[PVP] Server-side initialized - Guild-based PVP is active (opt-in)");
            api.Logger.Notification("[PVP] Players must join a guild and enable PVP with /pvp to participate");
        }

        private void OnPlayerJoin(IServerPlayer player)
        {
            // Load player's PVP data
            pvpManager?.LoadPlayerData(player.PlayerUID);

            // Add PVP damage protection behavior to the player entity
            if (player.Entity != null && pvpManager != null && serverApi != null)
            {
                var behavior = player.Entity.GetBehavior<PVPDamageProtectionBehavior>();
                if (behavior == null)
                {
                    player.Entity.AddBehavior(new PVPDamageProtectionBehavior(player.Entity, pvpManager, serverApi));
                    serverApi.Logger.Debug($"[PVP] Added damage protection behavior to {player.PlayerName}");
                }
            }
        }

        private void OnPlayerDisconnect(IServerPlayer player)
        {
            // End any active duel
            if (pvpManager?.DuelManager != null && pvpManager.DuelManager.IsPlayerInDuel(player.PlayerUID))
            {
                pvpManager.DuelManager.EndDuel(player.PlayerUID, out string opponentUid, out string opponentName);

                var opponent = serverApi?.World.PlayerByUid(opponentUid) as IServerPlayer;
                if (opponent != null && serverApi != null)
                {
                    serverApi.SendMessage(opponent, 0,
                        $"{player.PlayerName} disconnected. The duel has ended.",
                        EnumChatType.Notification);
                    networkHandler?.SendDuelEnded(player.PlayerUID, opponentUid, opponentName);
                }
            }

            // Save player's PVP data
            pvpManager?.SavePlayerData(player.PlayerUID);
        }

        private void OnPlayerDeath(IServerPlayer player, DamageSource damageSource)
        {
            // Check if player was in a duel
            if (pvpManager?.DuelManager != null && pvpManager.DuelManager.IsPlayerInDuel(player.PlayerUID))
            {
                pvpManager.DuelManager.EndDuel(player.PlayerUID, out string opponentUid, out string opponentName);

                if (serverApi != null)
                {
                    // Notify the dead player
                    serverApi.SendMessage(player, 0,
                        $"You were defeated by {opponentName} in the duel!",
                        EnumChatType.Notification);

                    // Notify the winner
                    var opponent = serverApi.World.PlayerByUid(opponentUid) as IServerPlayer;
                    if (opponent != null)
                    {
                        serverApi.SendMessage(opponent, 0,
                            $"Victory! You defeated {player.PlayerName} in the duel!",
                            EnumChatType.Notification);
                    }

                    // Send duel ended packet
                    networkHandler?.SendDuelEnded(player.PlayerUID, opponentUid, opponentName);
                }
            }
        }

        private void OnSaveGameLoaded()
        {
            // Load all PVP data from world storage
            pvpManager?.LoadAllPlayerData();

            // Ensure all currently online players have data loaded and damage protection behavior
            if (serverApi != null && pvpManager != null)
            {
                foreach (var player in serverApi.World.AllOnlinePlayers)
                {
                    pvpManager.LoadPlayerData(player.PlayerUID);

                    // Add PVP damage protection behavior to existing online players
                    if (player is IServerPlayer serverPlayer && serverPlayer.Entity != null)
                    {
                        var behavior = serverPlayer.Entity.GetBehavior<PVPDamageProtectionBehavior>();
                        if (behavior == null)
                        {
                            serverPlayer.Entity.AddBehavior(new PVPDamageProtectionBehavior(serverPlayer.Entity, pvpManager, serverApi));
                            serverApi.Logger.Debug($"[PVP] Added damage protection behavior to {serverPlayer.PlayerName}");
                        }
                    }
                }

                // Register periodic cleanup for expired duel challenges (every 30 seconds)
                serverApi.Event.RegisterGameTickListener((dt) =>
                {
                    pvpManager.DuelManager?.CleanupExpiredChallenges();
                }, 30000);
            }
        }

        private void OnGameWorldSave()
        {
            // Save all player data
            pvpManager?.SaveAllPlayerData();
        }

        private void RegisterCommands(ICoreServerAPI api)
        {
            // Register Node War admin command
            if (pvpManager?.NodeWarManager != null)
            {
                var guildMod = api.ModLoader.GetModSystem<SOAGuildsAndKingdomsModSystem>();
                if (guildMod != null)
                {
                    api.ChatCommands.Create("nodewaradmin")
                        .WithDescription("Open Node War Admin UI (admins only)")
                        .RequiresPrivilege(Privilege.controlserver)
                        .HandleWith((args) => OnNodeWarAdminCommand(args, guildMod));
                }
            }

            // Player command to toggle PVP status
            api.ChatCommands.Create("pvp")
                .WithDescription("Toggle your PVP status on/off (requires guild membership)")
                .RequiresPrivilege(Privilege.chat)
                .HandleWith(OnPVPCommand);

            // Admin command to view all PVP-enabled players (guild members)
            api.ChatCommands.Create("pvplist")
                .WithDescription("List all players in guilds who can participate in PVP (admin only)")
                .RequiresPrivilege(Privilege.controlserver)
                .HandleWith(OnPVPListCommand);

            // Admin command to force set PVP state (deprecated but kept for compatibility)
            api.ChatCommands.Create("pvpset")
                .WithDescription("Check PVP eligibility for a player (admin only)")
                .WithArgs(
                    api.ChatCommands.Parsers.Word("playerName"),
                    api.ChatCommands.Parsers.Bool("enabled")
                )
                .RequiresPrivilege(Privilege.controlserver)
                .HandleWith(OnPVPSetCommand);

            // Duel command with subcommands
            api.ChatCommands.Create("duel")
                .WithDescription("Challenge another player to a duel")
                .RequiresPrivilege(Privilege.chat)
                .BeginSubCommand("accept")
                    .WithDescription("Accept a duel challenge")
                    .HandleWith(OnDuelAcceptCommand)
                .EndSubCommand()
                .BeginSubCommand("decline")
                    .WithDescription("Decline a duel challenge")
                    .HandleWith(OnDuelDeclineCommand)
                .EndSubCommand()
                .BeginSubCommand("surrender")
                    .WithDescription("Surrender your current duel")
                    .HandleWith(OnDuelSurrenderCommand)
                .EndSubCommand()
                .WithArgs(api.ChatCommands.Parsers.OptionalWord("playerName"))
                .HandleWith(OnDuelCommand);
        }

        private TextCommandResult OnPVPCommand(TextCommandCallingArgs args)
        {
            var player = args.Caller.Player as IServerPlayer;
            if (player == null || pvpManager == null)
                return TextCommandResult.Error("Command can only be used by players.");

            bool success = pvpManager.TogglePVP(player.PlayerUID, out string message);

            if (success)
            {
                networkHandler?.SendPVPStatus(player);
                networkHandler?.BroadcastPVPStatusList();
                return TextCommandResult.Success(message);
            }

            return TextCommandResult.Error(message);
        }

        private TextCommandResult OnPVPListCommand(TextCommandCallingArgs args)
        {
            if (pvpManager == null || serverApi == null)
                return TextCommandResult.Error("PVP system not initialized.");

            var pvpPlayers = pvpManager.GetPVPPlayers();

            if (pvpPlayers.Count == 0)
                return TextCommandResult.Success("No players currently have PVP enabled.");

            var output = new System.Text.StringBuilder();
            output.AppendLine($"Players with PVP enabled ({pvpPlayers.Count}):");

            foreach (var playerUid in pvpPlayers)
            {
                var player = serverApi.World.PlayerByUid(playerUid);
                if (player != null)
                {
                    var guildName = pvpManager.GetPlayerGuildName(playerUid);
                    output.AppendLine($"  • {player.PlayerName} [{guildName ?? "No Guild"}]");
                }
            }

            return TextCommandResult.Success(output.ToString());
        }

        private TextCommandResult OnPVPSetCommand(TextCommandCallingArgs args)
        {
            if (pvpManager == null || serverApi == null)
                return TextCommandResult.Error("PVP system not initialized.");

            var targetName = args.Parsers[0].GetValue() as string;
            var enabled = (bool?)args.Parsers[1].GetValue();

            if (string.IsNullOrEmpty(targetName) || !enabled.HasValue)
                return TextCommandResult.Error("Usage: /pvpset <playerName> <true|false>");

            // Find player
            var targetPlayer = serverApi.World.AllPlayers
                .FirstOrDefault(p => p.PlayerName.Equals(targetName, StringComparison.OrdinalIgnoreCase));

            if (targetPlayer == null)
                return TextCommandResult.Error($"Player '{targetName}' not found.");

            pvpManager.SetPVPState(targetPlayer.PlayerUID, enabled.Value);

            // Update client if online
            if (targetPlayer is IServerPlayer serverPlayer)
            {
                networkHandler?.SendPVPStatus(serverPlayer);
            }

            networkHandler?.BroadcastPVPStatusList();

            return TextCommandResult.Success(
                $"Set PVP to {(enabled.Value ? "enabled" : "disabled")} for {targetPlayer.PlayerName}"
            );
        }

        private TextCommandResult OnDuelCommand(TextCommandCallingArgs args)
        {
            var player = args.Caller.Player as IServerPlayer;
            if (player == null || pvpManager?.DuelManager == null || serverApi == null)
                return TextCommandResult.Error("Command can only be used by players.");

            var targetName = args.Parsers[0].GetValue() as string;

            if (string.IsNullOrEmpty(targetName))
            {
                // Show duel status/help
                if (pvpManager.DuelManager.IsPlayerInDuel(player.PlayerUID))
                {
                    return TextCommandResult.Success("You are currently in a duel! Use /duel surrender to give up.");
                }

                var pendingChallenge = pvpManager.DuelManager.GetPendingChallenge(player.PlayerUID);
                if (pendingChallenge != null)
                {
                    return TextCommandResult.Success(
                        $"You have a pending duel challenge from {pendingChallenge.ChallengerName}.\n" +
                        "Use /duel accept to accept or /duel decline to decline."
                    );
                }

                return TextCommandResult.Success(
                    "Duel System - Challenge another player to a 1v1 fight!\n" +
                    "Usage:\n" +
                    "  /duel <playerName> - Challenge a player\n" +
                    "  /duel accept - Accept a challenge\n" +
                    "  /duel decline - Decline a challenge\n" +
                    "  /duel surrender - Give up your current duel"
                );
            }

            // Find target player
            var targetPlayer = serverApi.World.AllOnlinePlayers
                .FirstOrDefault(p => p.PlayerName.Equals(targetName, StringComparison.OrdinalIgnoreCase));

            if (targetPlayer == null)
                return TextCommandResult.Error($"Player '{targetName}' not found or is not online.");

            // Challenge the player via network
            var packet = new DuelChallengePacket
            {
                ChallengerUid = player.PlayerUID,
                TargetUid = targetPlayer.PlayerUID
            };

            // Handle directly on server side
            networkHandler?.GetType().GetMethod("OnDuelChallengeReceived",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?
                .Invoke(networkHandler, new object[] { player, packet });

            return TextCommandResult.Success();
        }

        private TextCommandResult OnDuelAcceptCommand(TextCommandCallingArgs args)
        {
            var player = args.Caller.Player as IServerPlayer;
            if (player == null || pvpManager?.DuelManager == null)
                return TextCommandResult.Error("Command can only be used by players.");

            var packet = new DuelAcceptPacket
            {
                AccepterUid = player.PlayerUID
            };

            // Handle directly on server side
            networkHandler?.GetType().GetMethod("OnDuelAcceptReceived",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?
                .Invoke(networkHandler, new object[] { player, packet });

            return TextCommandResult.Success();
        }

        private TextCommandResult OnDuelDeclineCommand(TextCommandCallingArgs args)
        {
            var player = args.Caller.Player as IServerPlayer;
            if (player == null || pvpManager?.DuelManager == null)
                return TextCommandResult.Error("Command can only be used by players.");

            var packet = new DuelDeclinePacket
            {
                DeclinerUid = player.PlayerUID
            };

            // Handle directly on server side
            networkHandler?.GetType().GetMethod("OnDuelDeclineReceived",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?
                .Invoke(networkHandler, new object[] { player, packet });

            return TextCommandResult.Success();
        }

        private TextCommandResult OnDuelSurrenderCommand(TextCommandCallingArgs args)
        {
            var player = args.Caller.Player as IServerPlayer;
            if (player == null || pvpManager?.DuelManager == null || serverApi == null)
                return TextCommandResult.Error("Command can only be used by players.");

            if (!pvpManager.DuelManager.IsPlayerInDuel(player.PlayerUID))
                return TextCommandResult.Error("You are not in a duel!");

            // End the duel
            pvpManager.DuelManager.EndDuel(player.PlayerUID, out string opponentUid, out string opponentName);

            // Notify both players
            networkHandler?.SendDuelEnded(player.PlayerUID, opponentUid, opponentName);

            serverApi.SendMessage(player, 0,
                $"You surrendered the duel. {opponentName} wins!",
                EnumChatType.Notification);

            var opponent = serverApi.World.PlayerByUid(opponentUid) as IServerPlayer;
            if (opponent != null)
            {
                serverApi.SendMessage(opponent, 0,
                    $"{player.PlayerName} surrendered! You win the duel!",
                    EnumChatType.Notification);
            }

            return TextCommandResult.Success();
        }

        #endregion

        #region Client-side initialization

        public override void StartClientSide(ICoreClientAPI api)
        {
            base.StartClientSide(api);
            clientApi = api;

            // Initialize networking with callbacks
            networkHandler?.InitializeClient(
                api, 
                OnNotificationReceived, 
                OnPVPStatusReceived, 
                OnNodeWarAdminOpenReceived,
                OnNodeWarAdminDataReceived,
                OnNodeWarAdminResponseReceived,
                OnCaptureZoneSyncReceived,
                OnAutoHologramToggleReceived);

            // Register hotkey for toggling PVP
            api.Input.RegisterHotKey("togglepvp", "Toggle PVP", GlKeys.P, HotkeyType.GUIOrOtherControls, shiftPressed: true);
            api.Input.SetHotKeyHandler("togglepvp", OnTogglePVPHotkey);

            // Register hotkey for toggling capture zone hologram
            api.Input.RegisterHotKey("togglecapturezones", "Toggle Capture Zone Hologram", GlKeys.O, HotkeyType.GUIOrOtherControls, shiftPressed: true);
            api.Input.SetHotKeyHandler("togglecapturezones", OnToggleCaptureZoneHologram);

            api.Logger.Notification("[PVP] Client-side initialized");
        }

        private TextCommandResult OnNodeWarAdminCommand(TextCommandCallingArgs args, SOAGuildsAndKingdomsModSystem guildMod)
        {
            var player = args.Caller.Player as IServerPlayer;
            if (player == null)
                return TextCommandResult.Error("Command can only be used by players");

            if (pvpManager?.NodeWarManager == null)
                return TextCommandResult.Error("Node War system not initialized");

            if (networkHandler == null || serverApi == null)
                return TextCommandResult.Error("Network system not initialized");

            // Send packet to client to open the UI - works in both single-player and multiplayer!
            networkHandler.SendNodeWarAdminOpen(player);

            serverApi.Logger.Debug($"[PVP] Node War Admin command used by {player.PlayerName}");
            return TextCommandResult.Success();
        }

        private bool OnTogglePVPHotkey(KeyCombination key)
        {
            networkHandler?.RequestTogglePVP();
            return true;
        }

        private bool OnToggleCaptureZoneHologram(KeyCombination key)
        {
            ToggleCaptureZoneHologram();
            return true;
        }

        private void OnNotificationReceived(string message, NotificationType type)
        {
            if (clientApi == null) return;

            var prefix = type switch
            {
                NotificationType.Success => "[PVP]",
                NotificationType.Warning => "[PVP]",
                NotificationType.Error => "[PVP Error]",
                _ => "[PVP]"
            };

            clientApi.ShowChatMessage($"{prefix} {message}");
        }

        private void OnPVPStatusReceived(PVPStatusPacket packet)
        {
            currentPVPStatus = packet;

            if (clientApi != null)
            {
                var statusText = packet.PVPEnabled ? "enabled" : "disabled";
                clientApi.Logger.Debug($"[PVP] Status updated: {statusText}");
            }
        }

        private void OnNodeWarAdminOpenReceived()
        {
            if (clientApi == null || networkHandler == null)
            {
                return;
            }

            // Create and open the node war admin dialog
            // Works in both single-player and multiplayer via network packets
            clientApi.Event.EnqueueMainThreadTask(() =>
            {
                nodeWarAdminDialog = new DialogNodeWarAdminClient(clientApi, networkHandler);
                nodeWarAdminDialog.TryOpen();
                clientApi.Logger.Notification("[PVP] Opened Node War Admin UI");
            }, "openNodeWarAdminUI");
        }

        private void OnNodeWarAdminDataReceived(NodeWarAdminDataPacket packet)
        {
            if (clientApi == null) return;

            // Update the dialog with new data
            clientApi.Event.EnqueueMainThreadTask(() =>
            {
                nodeWarAdminDialog?.UpdateData(packet);
            }, "updateNodeWarAdminData");
        }

        private void OnNodeWarAdminResponseReceived(NodeWarAdminResponsePacket packet)
        {
            // Response is already shown as a chat message by the network handler
            // No additional action needed here
        }

        #endregion

        #region Public Accessors

        /// <summary>
        /// Get node war data for a specific guild
        /// Used by the Guild mod to populate the Node Wars tab
        /// </summary>
        public NodeWarTabData? GetNodeWarDataForGuild(string guildName)
        {
            if (pvpManager?.NodeWarManager == null || serverApi == null)
                return null;

            var guildMod = serverApi.ModLoader.GetModSystem<SOAGuildsAndKingdomsModSystem>();
            if (guildMod == null)
                return null;

            var dataProvider = new NodeWarDataProvider(serverApi, pvpManager.NodeWarManager, guildMod);
            return dataProvider.GetNodeWarDataForGuild(guildName);
        }

        /// <summary>
        /// Handle node war data request from Guild mod
        /// Called when a client requests node war data for their guild
        /// </summary>
        public void OnNodeWarDataRequested(IServerPlayer player, string guildName, SOAGuildsAndKingdomsModSystem guildMod)
        {
            if (serverApi == null || pvpManager?.NodeWarManager == null)
            {
                serverApi?.Logger.Warning($"[PVP] Node war data requested but system not initialized");
                return;
            }

            serverApi.Logger.Debug($"[PVP] Processing node war data request from {player.PlayerName} for guild {guildName}");

            // Get node war data
            var dataProvider = new NodeWarDataProvider(serverApi, pvpManager.NodeWarManager, guildMod);
            var nodeWarData = dataProvider.GetNodeWarDataForGuild(guildName);

            if (nodeWarData == null)
            {
                serverApi.Logger.Debug($"[PVP] No node war data available for guild {guildName}");
                // Send empty response so UI knows PVP mod is available but no data exists
                nodeWarData = new NodeWarTabData();
            }

            // Convert to network packet
            var responsePacket = dataProvider.ConvertToNetworkPacket(nodeWarData);

            // Send response via Guild mod's network handler
            var guildNetworkHandler = guildMod.GetNetworkHandler();
            if (guildNetworkHandler != null)
            {
                guildNetworkHandler.SendNodeWarData(player, responsePacket);
                serverApi.Logger.Debug($"[PVP] Sent node war data to {player.PlayerName}: " +
                                      $"{nodeWarData.ControlledNodes.Count} controlled nodes, " +
                                      $"{nodeWarData.AvailableWars.Count} available wars");
            }
            else
            {
                serverApi.Logger.Error($"[PVP] Could not send node war data - Guild network handler not available");
            }
        }

        /// <summary>
        /// Get the PVP manager instance
        /// </summary>
        public PVPManager? GetPVPManager() => pvpManager;

        /// <summary>
        /// Get the PVP network handler instance
        /// </summary>
        public PVPNetworkHandler? GetNetworkHandler() => networkHandler;

        #endregion

        #region Capture Zone Hologram

        /// <summary>
        /// Toggle the capture zone hologram visibility
        /// </summary>
        public void ToggleCaptureZoneHologram()
        {
            if (isHologramAutoManaged)
            {
                // User is manually toggling while in auto mode
                manualOverride = !manualOverride;

                if (manualOverride)
                {
                    // User wants it off despite auto
                    ClearCaptureZoneHologram();
                    clientApi?.ShowChatMessage("[PVP] Hologram manually disabled (in war zone)");
                }
                else
                {
                    // User wants auto behavior back - request fresh data
                    networkHandler?.RequestCaptureZones();
                    clientApi?.ShowChatMessage("[PVP] Hologram auto-enabled (in war zone) - refreshing...");
                }
            }
            else
            {
                // Normal manual toggle behavior
                captureZoneHologramVisible = !captureZoneHologramVisible;

                if (captureZoneHologramVisible)
                {
                    // Turn on hologram and request fresh data from server
                    networkHandler?.RequestCaptureZones();
                    clientApi?.ShowChatMessage("[PVP] Requesting capture zone data...");
                }
                else
                {
                    // Clear the hologram
                    ClearCaptureZoneHologram();
                    clientApi?.ShowChatMessage("[PVP] Capture zone hologram hidden");
                }
            }
        }

        /// <summary>
        /// Show capture zones as a hologram (border visualization)
        /// </summary>
        public void ShowCaptureZoneHologram()
        {
            if (clientApi == null || cachedCaptureZones == null) return;

            var player = clientApi.World.Player;
            if (player == null) return;

            if (cachedCaptureZones.ActiveZones.Count == 0)
            {
                clientApi.ShowChatMessage("[PVP] No active capture zones to display");
                captureZoneHologramVisible = false;
                return;
            }

            var blockPositions = new List<BlockPos>();
            int playerY = (int)player.Entity.Pos.Y;
            int minY = Math.Max(0, playerY - 50);
            int maxY = Math.Min(clientApi.World.BlockAccessor.MapSizeY - 1, playerY + 50);

            var spawnPos = clientApi.World.DefaultSpawnPosition;
            int spawnX = ((int?)spawnPos?.X) ?? 0;
            int spawnZ = ((int?)spawnPos?.Z) ?? 0;

            foreach (var zone in cachedCaptureZones.ActiveZones)
            {
                // Draw a circular border around the capture zone
                int centerX = (int)zone.CenterX + spawnX;
                int centerZ = (int)zone.CenterZ + spawnZ;
                int radius = zone.Radius;

                // Create circle approximation using blocks
                int segments = 128; // Number of segments for circle approximation
                for (int i = 0; i < segments; i++)
                {
                    double angle = (2 * Math.PI * i) / segments;
                    int x = centerX + (int)(radius * Math.Cos(angle));
                    int z = centerZ + (int)(radius * Math.Sin(angle));

                    // Add blocks every 3 vertical units for performance
                    for (int y = minY; y <= maxY; y += 3)
                    {
                        blockPositions.Add(new BlockPos(x, y, z));
                    }
                }
            }

            clientApi.World.HighlightBlocks(player, CAPTURE_ZONE_HOLOGRAM_SLOT, blockPositions, 
                EnumHighlightBlocksMode.Absolute, EnumHighlightShape.Arbitrary);

            clientApi.ShowChatMessage($"[PVP] Showing {cachedCaptureZones.ActiveZones.Count} active capture zone(s)");
        }

        /// <summary>
        /// Clear the capture zone hologram
        /// </summary>
        public void ClearCaptureZoneHologram()
        {
            if (clientApi == null) return;

            var player = clientApi.World.Player;
            if (player == null) return;

            clientApi.World.HighlightBlocks(player, CAPTURE_ZONE_HOLOGRAM_SLOT, new List<BlockPos>());
            captureZoneHologramVisible = false;
        }

        /// <summary>
        /// Gets whether the capture zone hologram is currently visible
        /// </summary>
        public bool IsCaptureZoneHologramVisible => captureZoneHologramVisible;

        /// <summary>
        /// Callback for when capture zone data is received from server
        /// </summary>
        private void OnCaptureZoneSyncReceived(CaptureZoneSyncPacket packet)
        {
            if (clientApi == null) return;

            // Cache the zones
            cachedCaptureZones = packet;

            clientApi.Logger.Debug($"[PVP] Received {packet.ActiveZones.Count} capture zones from server");

            // If hologram is currently visible, update it
            if (isHologramAutoManaged || captureZoneHologramVisible)
            {
                ShowCaptureZoneHologram();
            }
        }

        /// <summary>
        /// Callback for auto-hologram toggle from server
        /// </summary>
        private void OnAutoHologramToggleReceived(AutoCaptureZoneHologramPacket packet)
        {
            if (clientApi == null) return;

            if (packet.Enable)
            {
                // Enable hologram (auto-managed)
                isHologramAutoManaged = true;

                if (!captureZoneHologramVisible || manualOverride)
                {
                    // Request and show hologram
                    networkHandler?.RequestCaptureZones();
                    manualOverride = false; // Clear manual override
                    clientApi.ShowChatMessage("[PVP] Entered war zone - hologram enabled");
                }
            }
            else
            {
                // Disable hologram only if not manually overridden
                if (isHologramAutoManaged && !manualOverride)
                {
                    ClearCaptureZoneHologram();
                    isHologramAutoManaged = false;
                    clientApi.ShowChatMessage("[PVP] Left war zone - hologram disabled");
                }
            }
        }

        #endregion

        public override void Dispose()
        {
            // Cleanup PVP manager and capture zone system
            pvpManager?.Dispose();

            base.Dispose();
        }
    }
}
