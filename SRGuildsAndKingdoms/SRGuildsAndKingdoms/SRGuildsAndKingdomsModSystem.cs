using HarmonyLib;
using Newtonsoft.Json.Linq;
using SRGuildsAndKingdoms.src.config;
using SRGuildsAndKingdoms.src.database;
using SRGuildsAndKingdoms.src.gui;
using SRGuildsAndKingdoms.src.guilds;
using SRGuildsAndKingdoms.src.guilds.behaviors;
using SRGuildsAndKingdoms.src.network;
using SRGuildsAndKingdoms.src.party;
using SRGuildsAndKingdoms.src.techblock;
using SRGuildsAndKingdoms.src.quests;
using SRGuildsAndKingdoms.src.quests.blocks;
using SRGuildsAndKingdoms.src.utils;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.Common;
using Vintagestory.GameContent;
using GuildLandClaim = SRGuildsAndKingdoms.src.guilds.LandClaim;

namespace SRGuildsAndKingdoms
{
    public class SRGuildsAndKingdomsModSystem : ModSystem
    {
        private GuildManager? guildManager;
        private GuildTechManager? guildTechManager;
        private ZoneWhitelistManager? zoneWhitelistManager;
        private NodeManager? nodeManager;
        private GuildDatabase? guildDatabase;
        private GuildRepository? guildRepository;
        private QuestRepository? questRepository;
        private CooldownRepository? cooldownRepository;
        private ZoneWhitelistRepository? zoneWhitelistRepository;
        private NodeRepository? nodeRepository;
        private LandClaimRepository? landClaimRepository;
        private GuildWeeklyPointsRepository? weeklyPointsRepository;
        private EventsRepository? eventsRepository;
        private ICoreServerAPI? serverApi;
        private ICoreClientAPI? clientApi;
        private GuildNetworkHandler? networkHandler;
        private QuestNetworkHandler? questNetworkHandler;
        private PartyManager? partyManager;
        private PartyNetworkHandler? partyNetworkHandler;
        private EventServerNetworkHandler? eventServerNetworkHandler;
        private EventClientNetworkHandler? eventClientNetworkHandler;
        // Client-side cache of guild summaries (populated from server)
        private List<GuildSummary> clientGuildSummaries = new();
        // Client-side plot map layer reference (set by PlotMapLayer constructor)
        private PlotMapLayer? plotLayer;
        private WorldMapManager? worldMapManager;
        // Pending config packet (cached until plot layer is registered)
        private GuildConfigPacket? pendingConfigPacket;
        private Harmony? harmony;
        // Hologram state
        private bool hologramVisible = false;
        // Invite popup dialog
        private DialogGuildInvitePopup? invitePopup;
        // Party invite popup dialog
        private DialogPartyInvitePopup? partyInvitePopup;
        // Party create dialog
        private DialogPartyCreate? partyCreateDialog;
        // Party manager dialog
        private DialogPartyManager? partyManagerDialog;
        // Party HUD
        private PartyHud? partyHud;
        // Current party data (cached from server)
        private Party? currentParty;
        // Main guild dialog so we can toggle it when hotkey is pressed
        private DialogGuildMain? guildDialog;
        // Quest manager dialog (admin only)
        private DialogQuestManager? questManagerDialog;
        private DialogEventManager? eventManagerDialog;

        /// <summary>
        /// Gets whether the hologram is currently visible
        /// </summary>
        public bool IsHologramVisible => hologramVisible;
        private const int HOLOGRAM_SLOT = 99; // Arbitrary slot for hologram
        // Tech blocks configuration
        private TechBlocksConfig? techBlocksConfig;

        /// <summary>
        /// Gets the list of tech blocks loaded from configuration
        /// </summary>
        public List<TechBlock> TechBlocks => techBlocksConfig?.TechBlocks ?? new List<TechBlock>();

        /// <summary>
        /// Gets the tech blocks configuration including age restrictions
        /// </summary>
        public TechBlocksConfig? TechBlocksConfig => techBlocksConfig;

        /// <summary>
        /// Gets the guild tech manager for managing tech progression
        /// </summary>
        public GuildTechManager? GuildTechManager => guildTechManager;

        /// <summary>
        /// Gets the guild manager (server-side)
        /// </summary>
        public GuildManager? GuildManager => guildManager;

        /// <summary>
        /// Gets the guild repository (server-side)
        /// </summary>
        public GuildRepository? GetGuildRepository() => guildRepository;

        /// <summary>
        /// Gets the network handler for client-side network communication
        /// </summary>
        public GuildNetworkHandler? NetworkHandler => networkHandler;

        /// <summary>
        /// Gets the quest network handler for quest-related communication
        /// </summary>
        public QuestNetworkHandler? QuestNetworkHandler => questNetworkHandler;

        /// <summary>
        /// Gets the party network handler for party-related communication
        /// </summary>
        public PartyNetworkHandler? PartyNetworkHandler => partyNetworkHandler;

        /// <summary>
        /// Gets the event client network handler for event-related communication
        /// </summary>
        public EventClientNetworkHandler? EventClientNetworkHandler => eventClientNetworkHandler;

        /// <summary>
        /// Gets the quest repository (server-side)
        /// </summary>
        public QuestRepository? GetQuestRepository() => questRepository;

        /// <summary>
        /// Gets the events repository (server-side)
        /// </summary>
        public EventsRepository? GetEventsRepository() => eventsRepository;

        /// <summary>
        /// Gets the zone whitelist manager (server-side)
        /// </summary>
        public ZoneWhitelistManager? GetZoneWhitelistManager() => zoneWhitelistManager;

        /// <summary>
        /// Gets the node manager (server-side)
        /// </summary>
        public NodeManager? GetNodeManager() => nodeManager;

        /// <summary>
        /// Gets the land claim repository (server-side)
        /// </summary>
        public LandClaimRepository? GetLandClaimRepository() => landClaimRepository;

        public override double ExecuteOrder()
        {
            return 0.2;
        }

        /// <summary>
        /// Checks if a position is within a protected zone (works on both client and server)
        /// Returns the zone information if found
        /// </summary>
        public (bool isProtected, string zoneName, List<string> whitelistedPlayers)? CheckProtectedZone(int x, int z)
        {
            if (serverApi != null)
            {
                // Server-side: get from config
                var config = guildManager?.GetConfigManager()?.GetConfig();
                var spawnPos = serverApi.World.DefaultSpawnPosition.AsBlockPos;

                if (config != null && spawnPos != null && config.IsWithinProtectedZone(x, z, spawnPos))
                {
                    var zone = config.GetProtectedZoneAt(x, z, spawnPos);
                    if (zone != null)
                    {
                        return (true, zone.Name, []);
                    }
                }
            }
            else if (clientApi != null)
            {
                // Client-side: get from plot layer cached config
                var plotLayer = GetPlotLayer();
                if (plotLayer != null)
                {
                    var zoneInfo = plotLayer.GetProtectedZoneAt(x, z);
                    if (zoneInfo.HasValue)
                    {
                        return (true, zoneInfo.Value.name, zoneInfo.Value.whitelistedPlayers);
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Gets a guild summary by name from the client-side cache
        /// </summary>
        public GuildSummary? GetCachedGuildSummary(string guildName)
        {
            return clientGuildSummaries.FirstOrDefault(g => g.Name == guildName);
        }

        /// <summary>
        /// Gets all cached guild summaries
        /// </summary>
        public List<GuildSummary> GetCachedGuildSummaries()
        {
            return new List<GuildSummary>(clientGuildSummaries);
        }

        public override void Start(ICoreAPI api)
        {
            Mod.Logger.Notification("Hello from template mod: " + api.Side);
            networkHandler = new GuildNetworkHandler();
            questNetworkHandler = new QuestNetworkHandler();
            partyNetworkHandler = new PartyNetworkHandler();

            // Load tech blocks configuration
            try
            {
                techBlocksConfig = TechBlocksConfig.LoadFromFile(api);
                Mod.Logger.Notification($"Loaded {TechBlocks.Count} tech blocks from configuration");
            }
            catch (Exception ex)
            {
                Mod.Logger.Error($"Failed to load tech blocks configuration: {ex.Message}");
                techBlocksConfig = new TechBlocksConfig();
            }

            // Register age-restricted block behavior
            api.RegisterBlockBehaviorClass("AgeRestricted", typeof(BlockBehaviorAgeRestricted));

            // Register guild protection block behavior
            //api.RegisterBlockBehaviorClass("GuildProtected", typeof(BlockBehaviorGuildProtected));

            // Apply age restrictions to existing blocks after world is loaded
            if (api.Side == EnumAppSide.Server)
            {
                (api as ICoreServerAPI)!.Event.SaveGameLoaded += () =>
                {
                    ApplyAgeRestrictionsToBlocks(api);
                    ApplyProtectionToBlocks(api);
                };
            }
            else
            {
                (api as ICoreClientAPI)!.Event.LevelFinalize += () =>
                {
                    ApplyAgeRestrictionsToBlocks(api);
                    ApplyProtectionToBlocks(api);
                };
            }

            // Initialize Harmony
            harmony = new Harmony("srguildsandkingdoms.patches");

            // Manual patching - only patch specific methods we can actually patch
            try
            {
                // Find the TestBlockAccess method with the correct signature
                var worldMapTestBlockAccessMethod = typeof(WorldMap).GetMethod("TestBlockAccess",
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance,
                    null,
                    new Type[] {
                        typeof(IPlayer),
                        typeof(BlockSelection),
                        typeof(EnumBlockAccessFlags),
                        typeof(string).MakeByRefType()
                    },
                    null);

                if (worldMapTestBlockAccessMethod != null)
                {
                    var prefixMethod = typeof(SRGuildsAndKingdoms.src.patches.WorldMapTestBlockAccessPatch).GetMethod("Prefix");
                    harmony.Patch(worldMapTestBlockAccessMethod, prefix: new HarmonyMethod(prefixMethod));
                    Mod.Logger.Notification("Successfully patched WorldMap.TestBlockAccess");
                }
                else
                {
                    Mod.Logger.Warning("Could not find WorldMap.TestBlockAccess method to patch");
                }

                // Patch Portcullis BEBehaviorLargeDoor for rank checking (controller block)
                try
                {
                    Type? largeDoorBehaviorType = null;
                    foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                    {
                        if (assembly.GetName().Name == "Portcullis")
                        {
                            largeDoorBehaviorType = assembly.GetType("Portcullis.BEBehaviorLargeDoor");
                            break;
                        }
                    }

                    if (largeDoorBehaviorType != null)
                    {
                        var onBlockInteractStartMethod = largeDoorBehaviorType.GetMethod("OnBlockInteractStart",
                            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

                        if (onBlockInteractStartMethod != null)
                        {
                            var prefixMethod = typeof(SRGuildsAndKingdoms.src.patches.PortcullisLargeDoorPatch).GetMethod("PrefixController");
                            var postfixMethod = typeof(SRGuildsAndKingdoms.src.patches.PortcullisLargeDoorPatch).GetMethod("PostfixController");
                            harmony.Patch(onBlockInteractStartMethod,
                                prefix: new HarmonyMethod(prefixMethod),
                                postfix: new HarmonyMethod(postfixMethod));
                            Mod.Logger.Notification("Successfully patched Portcullis.BEBehaviorLargeDoor.OnBlockInteractStart for rank checking and auto-close");
                        }
                        else
                        {
                            Mod.Logger.Warning("Could not find OnBlockInteractStart method in Portcullis.BEBehaviorLargeDoor");
                        }
                    }
                    else
                    {
                        Mod.Logger.Debug("Portcullis mod not found - skipping large door rank patches");
                    }
                }
                catch (Exception portEx)
                {
                    Mod.Logger.Warning($"Failed to patch Portcullis controller: {portEx.Message}");
                }

                // Patch Portcullis BlockDrawbridgeCell for rank checking (multiblock cells)
                try
                {
                    Type? cellBlockType = null;
                    foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                    {
                        if (assembly.GetName().Name == "Portcullis")
                        {
                            cellBlockType = assembly.GetType("Portcullis.BlockDrawbridgeCell");
                            break;
                        }
                    }

                    if (cellBlockType != null)
                    {
                        var cellInteractMethod = cellBlockType.GetMethod("OnBlockInteractStart",
                            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

                        if (cellInteractMethod != null)
                        {
                            var cellPrefixMethod = typeof(SRGuildsAndKingdoms.src.patches.PortcullisLargeDoorPatch).GetMethod("PrefixCell");
                            var cellPostfixMethod = typeof(SRGuildsAndKingdoms.src.patches.PortcullisLargeDoorPatch).GetMethod("PostfixCell");
                            harmony.Patch(cellInteractMethod,
                                prefix: new HarmonyMethod(cellPrefixMethod),
                                postfix: new HarmonyMethod(cellPostfixMethod));
                            Mod.Logger.Notification("Successfully patched Portcullis.BlockDrawbridgeCell.OnBlockInteractStart for rank checking and auto-close");
                        }
                        else
                        {
                            Mod.Logger.Warning("Could not find OnBlockInteractStart method in Portcullis.BlockDrawbridgeCell");
                        }
                    }
                    else
                    {
                        Mod.Logger.Debug("Portcullis mod not found - skipping cell block rank patches");
                    }
                }
                catch (Exception cellEx)
                {
                    Mod.Logger.Warning($"Failed to patch Portcullis cells: {cellEx.Message}");
                }

                // Note: Client-side interaction blocking is handled via server-side validation
                // The server's WorldMap.TestBlockAccess patch will prevent unauthorized actions
            }
            catch (Exception ex)
            {
                Mod.Logger.Error("Failed to apply Harmony patches: " + ex.Message);
                Mod.Logger.Error("Stack trace: " + ex.StackTrace);
            }

            api.RegisterBlockBehaviorClass("GrsDoor", typeof(BlockBehaviorGrsDoor));
            api.RegisterBlockClass("QuestBoardBlock", typeof(QuestBoardBlock));
        }

        /// <summary>
        /// Applies age restrictions to blocks based on configuration
        /// </summary>
        private void ApplyAgeRestrictionsToBlocks(ICoreAPI api)
        {
            if (techBlocksConfig?.AgeRestrictedBlocks == null)
            {
                Mod.Logger.Warning("No age-restricted blocks configuration found");
                return;
            }

            int restrictedCount = 0;

            foreach (var block in api.World.Blocks)
            {
                if (block == null || block.Code == null)
                    continue;

                var blockCode = block.Code.ToString();

                // Check if this block matches any age restriction patterns
                if (techBlocksConfig.AgeRestrictedBlocks.IsBlockRestricted(blockCode, out TechAge requiredAge, out string requiredTrait))
                {
                    // Add the age restriction behavior to the block
                    var traitJson = string.IsNullOrWhiteSpace(requiredTrait) ? "null" : $"\"{requiredTrait}\"";
                    var behaviorProperties = new JsonObject(JToken.Parse($"{{\"requiredAge\":\"{requiredAge}\",\"requiredTrait\":{traitJson}}}"));

                    var behavior = new BlockBehaviorAgeRestricted(block);
                    behavior.Initialize(behaviorProperties);
                    behavior.OnLoaded(api);

                    // Add to block's behaviors list
                    var behaviorsList = block.BlockBehaviors?.ToList() ?? new List<BlockBehavior>();
                    behaviorsList.Add(behavior);
                    block.BlockBehaviors = behaviorsList.ToArray();

                    restrictedCount++;
                }
            }

            Mod.Logger.Notification($"Applied age restrictions to {restrictedCount} blocks");
        }

        /// <summary>
        /// Applies guild protection behavior to all blocks
        /// </summary>
        private void ApplyProtectionToBlocks(ICoreAPI api)
        {
            // Only apply on server side (client doesn't need this behavior)
            if (api.Side != EnumAppSide.Server)
            {
                return;
            }

            int protectedCount = 0;

            foreach (var block in api.World.Blocks)
            {
                if (block == null || block.Code == null)
                    continue;

                // Add the guild protection behavior to all blocks
                //var behavior = new BlockBehaviorGuildProtected(block);
                //behavior.OnLoaded(api);

                // Add to block's behaviors list
                var behaviorsList = block.BlockBehaviors?.ToList() ?? new List<BlockBehavior>();
                //behaviorsList.Add(behavior);
                block.BlockBehaviors = behaviorsList.ToArray();

                protectedCount++;
            }

            Mod.Logger.Notification($"Applied guild protection to {protectedCount} blocks");
        }

        public override void Dispose()
        {
            harmony?.UnpatchAll("srguildsandkingdoms.patches");
            base.Dispose();
        }

        public override void StartServerSide(ICoreServerAPI api)
        {
            serverApi = api;

            // Initialize database and repositories
            try
            {
                guildDatabase = new GuildDatabase(api);
                guildDatabase.Initialize();

                // Create repositories
                guildRepository = new GuildRepository(api, guildDatabase);
                questRepository = new QuestRepository(api, guildDatabase);
                cooldownRepository = new CooldownRepository(api, guildDatabase);
                zoneWhitelistRepository = new ZoneWhitelistRepository(api, guildDatabase);
                nodeRepository = new NodeRepository(api, guildDatabase);
                weeklyPointsRepository = new GuildWeeklyPointsRepository(api, guildDatabase);
                eventsRepository = new EventsRepository(api, guildDatabase);

                guildRepository.LoadAllGuilds();
                var initialLoadCount = guildRepository.GetAllGuilds().Count;

                // Check if migration is needed
                var migrationManager = new MigrationManager(api, guildDatabase);
                if (migrationManager.NeedsMigration())
                {
                    api.Logger.Warning("[SRGuildsAndKingdoms:Migration] JSON guild data detected - running migration to SQLite...");
                    var result = migrationManager.MigrateFromJson();

                    if (result.Success)
                    {
                        api.Logger.Notification($"[SRGuildsAndKingdoms:Migration] Migration completed successfully:");
                        api.Logger.Notification($"  - {result.GuildsMigrated} guilds migrated");
                        api.Logger.Notification($"  - {result.CooldownsMigrated} cooldowns migrated");
                        api.Logger.Notification($"  - {result.ZoneWhitelistsMigrated} zone whitelists migrated");

                        if (result.Warnings.Count > 0)
                        {
                            api.Logger.Warning($"[SRGuildsAndKingdoms:Migration] Migration had {result.Warnings.Count} warnings:");
                            foreach (var warning in result.Warnings)
                            {
                                api.Logger.Warning($"  - {warning}");
                            }
                        }

                        // Reload guilds from database to populate cache with migrated data
                        api.Logger.Notification("[SRGuildsAndKingdoms:Migration] Reloading guilds from database after migration...");
                        guildRepository.LoadAllGuilds();
                        var loadedCount = guildRepository.GetAllGuilds().Count;
                        api.Logger.Notification($"[SRGuildsAndKingdoms:Migration] Loaded {loadedCount} guild(s) into cache");
                    }
                    else
                    {
                        api.Logger.Error($"[SRGuildsAndKingdoms:Migration] Migration FAILED!");
                        foreach (var error in result.Errors)
                        {
                            api.Logger.Error($"  - {error}");
                        }
                        throw new InvalidOperationException("Guild migration failed. See log for details.");
                    }
                }

                api.Logger.Notification("[SRGuildsAndKingdoms] Database initialization complete");
            }
            catch (Exception ex)
            {
                api.Logger.Error($"[SRGuildsAndKingdoms] Failed to initialize database: {ex.Message}");
                api.Logger.Error($"Stack trace: {ex.StackTrace}");
                throw;
            }

            // Initialize land claim spatial indexing
            landClaimRepository = new LandClaimRepository(api, guildRepository);
            landClaimRepository.RebuildIndexes();
            var (totalClaims, guildsWithClaims) = landClaimRepository.GetStatistics();
            api.Logger.Notification($"[SRGuildsAndKingdoms] Land claim spatial indexes built: {totalClaims} chunks across {guildsWithClaims} guilds");

            // Initialize GuildManager with repositories
            guildManager = new GuildManager(api, guildRepository, cooldownRepository, landClaimRepository);

            // Initialize guild tech manager with MarkDirty callback
            guildTechManager = new GuildTechManager(
                api,
                (guildId) => guildManager?.GetGuild(guildId)!,
                (guildId) => guildRepository?.MarkDirty(guildId)
            );

            // Initialize zone whitelist manager
            zoneWhitelistManager = new ZoneWhitelistManager(api, zoneWhitelistRepository);

            // Initialize party manager
            partyManager = new PartyManager(api);

            // Initialize node manager
            nodeManager = new NodeManager(api, nodeRepository);

            // Initialize networking
            networkHandler!.InitializeServer(api, guildManager);

            // Initialize party networking
            partyNetworkHandler!.InitializeServer(api, partyManager, this);

            // Initialize quest networking
            questNetworkHandler = new QuestNetworkHandler();
            questNetworkHandler.InitializeServer(api, questRepository, guildRepository, guildManager, weeklyPointsRepository);
            questNetworkHandler.OnGuildPointsAwarded = (player) => networkHandler?.BroadcastGuildSummaries(player);

            // Initialize event networking
            eventServerNetworkHandler = new EventServerNetworkHandler();
            eventServerNetworkHandler.InitializeServer(api, eventsRepository);

            api.Event.SaveGameLoaded += OnSaveGameLoaded;
            api.Event.GameWorldSave += OnSaveGameSaving;

            // Process expired repeating quests on server start
            ProcessExpiredRepeatingQuests();

            // Register entity death event for quest tracking
            api.Event.OnEntityDeath += OnEntityDeath;

            // Sync player traits and guild summaries when they join the server
            api.Event.PlayerJoin += (player) =>
            {
                // Delay trait and guild summary sync slightly to ensure all systems are loaded
                api.Event.RegisterCallback((dt) =>
                {
                    if (guildManager != null && player is IServerPlayer serverPlayer)
                    {
                        Mod.Logger.Notification($"[SRGuildsAndKingdoms:GuildSync] Syncing traits and guild summaries for player {serverPlayer.PlayerName} on join");
                        guildManager.SyncPlayerTraits(serverPlayer);
                        // Guild summaries are already sent by GuildNetworkHandler.OnPlayerJoin
                        // but we'll explicitly broadcast them here to ensure sync after trait update
                        networkHandler?.BroadcastGuildSummaries(serverPlayer);
                    }
                }, 1000); // 1 second delay
            };

            // Set up a periodic timer to sync all player traits every 5 seconds
            api.Event.RegisterGameTickListener((dt) =>
            {
                if (guildManager != null && serverApi != null)
                {
                    var onlinePlayers = serverApi.World.AllOnlinePlayers;
                    if (onlinePlayers != null && onlinePlayers.Length > 0)
                    {
                        Mod.Logger.Debug($"[SRGuildsAndKingdoms:TraitSync] Running periodic trait sync for {onlinePlayers.Length} online players");
                        foreach (var player in onlinePlayers)
                        {
                            if (player is IServerPlayer serverPlayer)
                            {
                                guildManager.SyncPlayerTraits(serverPlayer);
                            }
                        }
                    }
                }
            }, 600000); // Run every 5 seconds (5000ms)

            // Set up a periodic timer to cleanup expired guild invites every minute
            api.Event.RegisterGameTickListener((dt) =>
            {
                if (guildManager != null)
                {
                    guildManager.CleanupExpiredInvitesPublic();
                }
            }, 60000); // Run every minute (60000ms)

            // Register chat command for accepting guild invites
            api.ChatCommands.Create("guild")
                .WithDescription("Guild management commands")
                .WithArgs(api.ChatCommands.Parsers.Word("action"))
                .RequiresPrivilege(Privilege.chat)
                .HandleWith(OnGuildChatCommand);

            // Register command to list inventory items (for tech block configuration)
            /*api.ChatCommands.Create("listinventory")
                .WithDescription("Lists all items in your inventory with their codes (useful for tech block configuration)")
                .WithAlias("listinv")
                .WithAlias("li")
                .RequiresPrivilege(Privilege.chat)
                .HandleWith(OnListInventoryCommand);

            // Register command to list player traits (for debugging guild trait system)
            api.ChatCommands.Create("listtraits")
                .WithDescription("Lists all traits currently active on your character")
                .WithAlias("traits")
                .RequiresPrivilege(Privilege.chat)
                .HandleWith(OnListTraitsCommand);

            // Debug command to simulate receiving guild invites
            api.ChatCommands.Create("debuginvite")
                .WithDescription("Debug: Simulate receiving a guild invite (for testing)")
                .WithArgs(
                    api.ChatCommands.Parsers.Word("guildName"),
                    api.ChatCommands.Parsers.OptionalWord("inviterName"),
                    api.ChatCommands.Parsers.OptionalInt("expirySeconds")
                )
                .RequiresPrivilege(Privilege.controlserver)
                .HandleWith(OnDebugInviteCommand);*/

            // Admin command to reset a player's guild cooldown
            api.ChatCommands.Create("guildresetcooldown")
                .WithDescription("Reset a player's guild rejoin cooldown (admin only)")
                .WithAlias("guildclearcd")
                .WithArgs(api.ChatCommands.Parsers.Word("playerName"))
                .RequiresPrivilege(Privilege.controlserver)
                .HandleWith(OnResetCooldownCommand);

            // Admin command to manage guild claims
            api.ChatCommands.Create("guildmanager")
                .WithDescription("Admin guild management commands")
                .WithAlias("gm")
                .RequiresPrivilege(Privilege.controlserver)
                .BeginSubCommand("removeclaim")
                    .WithDescription("Remove the guild claim for the chunk you are currently standing in")
                    .HandleWith(OnGuildManagerRemoveClaimCommand)
                .EndSubCommand()
                .BeginSubCommand("addplayer")
                    .WithDescription("Forcibly add a player to a guild")
                    .WithArgs(
                        api.ChatCommands.Parsers.Word("playerUsername"),
                        api.ChatCommands.Parsers.All("guildName")
                    )
                    .HandleWith(OnGuildManagerAddPlayerCommand)
                .EndSubCommand();

            // Protected zone whitelist commands
            api.ChatCommands.Create("zonewhitelist")
                .WithDescription("Manage protected zone whitelists (admin only)")
                .WithAlias("zonewl")
                .RequiresPrivilege("srguildsandkingdoms:zonewhitelist")
                .BeginSubCommand("add")
                    .WithDescription("Add a player to a protected zone's whitelist. Usage: /zonewhitelist add <zoneId> <playerName>")
                    .WithArgs(
                        api.ChatCommands.Parsers.Int("zoneId"),
                        api.ChatCommands.Parsers.All("playerName")
                    )
                    .HandleWith(OnZoneWhitelistAddCommand)
                .EndSubCommand()
                .BeginSubCommand("remove")
                    .WithDescription("Remove a player from a protected zone's whitelist. Usage: /zonewhitelist remove <zoneId> <playerName>")
                    .WithArgs(
                        api.ChatCommands.Parsers.Int("zoneId"),
                        api.ChatCommands.Parsers.All("playerName")
                    )
                    .HandleWith(OnZoneWhitelistRemoveCommand)
                .EndSubCommand()
                .BeginSubCommand("list")
                    .WithDescription("List whitelists (all, by zone ID, or by player name)")
                    .WithArgs(api.ChatCommands.Parsers.OptionalAll("filter"))
                    .HandleWith(OnZoneWhitelistListCommand)
                .EndSubCommand()
                .BeginSubCommand("clear")
                    .WithDescription("Clear all players from a zone's whitelist. Usage: /zonewhitelist clear <zoneId>")
                    .WithArgs(api.ChatCommands.Parsers.Int("zoneId"))
                    .HandleWith(OnZoneWhitelistClearCommand)
                .EndSubCommand()
                .BeginSubCommand("zones")
                    .WithDescription("List all available protected zones with their IDs")
                    .HandleWith(OnZoneWhitelistZonesCommand)
                .EndSubCommand();

            // Quest admin commands
            api.ChatCommands.Create("quests")
                .WithDescription("Quest management commands (admin only)")
                .RequiresPrivilege(Privilege.controlserver)
                .BeginSubCommand("removeprogress")
                    .WithDescription("Remove a player's quest progress by period key")
                    .WithArgs(
                        api.ChatCommands.Parsers.Word("playerUsername"),
                        api.ChatCommands.Parsers.Word("periodKey")
                    )
                    .HandleWith(OnQuestRemoveProgressCommand)
                .EndSubCommand()
                .BeginSubCommand("givegrs")
                    .WithDescription("Give or remove GRS points from a guild (use negative amount to remove)")
                    .WithArgs(
                        api.ChatCommands.Parsers.Int("amount"),
                        api.ChatCommands.Parsers.All("guildName")
                    )
                    .HandleWith(OnQuestGiveGrsCommand)
                .EndSubCommand();

            // Quest Manager command - opens GUI for quest management
            api.ChatCommands.Create("questmanager")
                .WithDescription("Quest manager commands")
                .WithAlias("qm")
                .RequiresPrivilege("srguildsandkingdoms:questmanager")
                .HandleWith(OnQuestManagerCommand)
                .BeginSubCommand("set")
                    .WithDescription("Set currency definitions for quests. Usage: /questmanager set [crowns|tails] (hold item)")
                    .WithArgs(api.ChatCommands.Parsers.Word("currencyType"))
                    .RequiresPrivilege(Privilege.controlserver)
                    .HandleWith(OnQuestManagerSetCurrencyCommand)
                .EndSubCommand();

            // Add party admin commands here
            api.ChatCommands.Create("partymanager")
                .WithDescription("Admin party management commands")
                .WithAlias("pm")
                .RequiresPrivilege(Privilege.controlserver)
                .BeginSubCommand("addplayer")
                    .WithDescription("Forcibly add a player to your party by UID or username")
                    .WithArgs(api.ChatCommands.Parsers.All("playerIdentifier"))
                    .HandleWith(OnPartyManagerAddPlayerCommand)
                .EndSubCommand();

            // Event admin commands
            api.ChatCommands.Create("eventmanager")
                .WithDescription("Admin event management commands")
                .WithAlias("em")
                .RequiresPrivilege("srguildsandkingdoms:eventmanager")
                .HandleWith(OnEventManagerCommand);
        }

        private TextCommandResult OnEventManagerCommand(TextCommandCallingArgs args)
        {
            if (args.Caller.Player is not IServerPlayer player)
                return TextCommandResult.Error("Command can only be used by players.");

            var packet = new EventManagerOpenPacket { };
            serverApi!.Network.GetChannel(EventServerNetworkHandler.ChannelName)!.SendPacket(packet, player);

            return TextCommandResult.Success("Opening Event Manager...");
        }

        private TextCommandResult OnGuildChatCommand(TextCommandCallingArgs args)
        {
            var player = args.Caller.Player as IServerPlayer;
            if (player == null) return TextCommandResult.Error("Command can only be used by players.");

            var action = args.Parsers[0].GetValue() as string;
            if (string.IsNullOrEmpty(action)) return TextCommandResult.Error("Please specify an action (accept, invites).");

            switch (action.ToLowerInvariant())
            {
                case "accept":
                    return HandleAcceptInvite(player);
                case "invites":
                case "invite":
                case "list":
                    return HandleListInvites(player);
                default:
                    return TextCommandResult.Error($"Unknown guild command: {action}. Available commands: accept, invites");
            }
        }

        private TextCommandResult HandleAcceptInvite(IServerPlayer player)
        {
            if (guildManager == null) return TextCommandResult.Error("Guild system not initialized.");

            bool success = guildManager.AcceptInvite(player.PlayerUID);

            if (success)
            {
                networkHandler?.SendNotification(player, "You have joined the guild.", NotificationType.Success);
                networkHandler?.BroadcastGuildSummariesToAll();
                return TextCommandResult.Success("Successfully joined the guild.");
            }
            else
            {
                return TextCommandResult.Error("No pending guild invite found or invite has expired.");
            }
        }

        private TextCommandResult HandleListInvites(IServerPlayer player)
        {
            if (guildManager == null) return TextCommandResult.Error("Guild system not initialized.");

            var invites = guildManager.GetPlayerInvites(player.PlayerUID);

            if (invites.Count == 0)
            {
                return TextCommandResult.Success("You have no pending guild invites.");
            }

            // Send the invite list packet to trigger the popup on client
            var inviteInfoList = new List<GuildInviteInfo>();
            foreach (var invite in invites)
            {
                var inviter = serverApi!.World.AllPlayers.FirstOrDefault(p => p.PlayerUID == invite.InviterUid);

                inviteInfoList.Add(new GuildInviteInfo
                {
                    GuildName = invite.GuildName,
                    InviterName = inviter?.PlayerName ?? "Unknown",
                    InviterUid = invite.InviterUid,
                    ExpiresAtTicks = invite.ExpiresAt.Ticks
                });
            }

            var response = new GuildInviteListResponsePacket
            {
                PlayerUid = player.PlayerUID,
                Invites = inviteInfoList
            };

            serverApi!.Network.GetChannel("srguildsandkingdoms:guild")!.SendPacket(response, player);

            return TextCommandResult.Success($"You have {invites.Count} pending guild invite(s). Check the popup in the bottom-right corner.");
        }

        private TextCommandResult OnResetCooldownCommand(TextCommandCallingArgs args)
        {
            if (guildManager == null)
                return TextCommandResult.Error("Guild system not initialized.");

            var targetPlayerName = args.Parsers[0].GetValue() as string;
            if (string.IsNullOrEmpty(targetPlayerName))
                return TextCommandResult.Error("Please specify a player name.");

            // Find the target player by name (check both online and offline players)
            string? targetPlayerUid = GetPlayerUidByName(targetPlayerName);
            if (string.IsNullOrEmpty(targetPlayerUid))
            {
                return TextCommandResult.Error($"Player '{targetPlayerName}' not found.");
            }

            IServerPlayer? targetPlayer = serverApi?.World.AllOnlinePlayers
                .FirstOrDefault(p => p.PlayerUID == targetPlayerUid) as IServerPlayer;

            // Check if player has a cooldown
            if (!guildManager.IsPlayerOnCooldown(targetPlayerUid, out var remainingTime))
            {
                return TextCommandResult.Success($"Player '{targetPlayerName}' has no active guild cooldown.");
            }

            // Clear the cooldown
            bool cleared = guildManager.ClearPlayerCooldown(targetPlayerUid);

            if (cleared)
            {
                var message = $"Guild cooldown cleared for player '{targetPlayerName}'. " +
                             $"(Had {remainingTime.TotalHours:F1} hours remaining)";

                // Notify the target player if they're online
                if (targetPlayer != null)
                {
                    targetPlayer.SendMessage(GlobalConstants.GeneralChatGroup,
                        "Your guild rejoin cooldown has been cleared by an administrator.",
                        EnumChatType.Notification);
                }

                return TextCommandResult.Success(message);
            }
            else
            {
                return TextCommandResult.Error($"Failed to clear cooldown for player '{targetPlayerName}'.");
            }
        }

        /// <summary>
        /// Admin command: Remove the guild claim for the chunk the admin is currently standing in
        /// </summary>
        private TextCommandResult OnGuildManagerRemoveClaimCommand(TextCommandCallingArgs args)
        {
            if (guildManager == null || landClaimRepository == null || guildRepository == null)
                return TextCommandResult.Error("Guild system not initialized.");

            var player = args.Caller.Player as IServerPlayer;
            if (player == null)
                return TextCommandResult.Error("Command can only be used by players.");

            // Get player's current position
            var pos = player.Entity.Pos.AsBlockPos;
            int chunkX = GuildLandClaim.FloorDiv(pos.X, GuildLandClaim.ChunkSize);
            int chunkZ = GuildLandClaim.FloorDiv(pos.Z, GuildLandClaim.ChunkSize);

            // Check if this chunk is claimed
            var owningGuildName = landClaimRepository.GetGuildOwningChunk(chunkX, chunkZ);
            if (string.IsNullOrEmpty(owningGuildName))
            {
                return TextCommandResult.Error($"Chunk ({chunkX}, {chunkZ}) is not claimed by any guild.");
            }

            // Get the guild
            var guild = guildManager.GetGuild(owningGuildName);
            if (guild == null)
            {
                return TextCommandResult.Error($"Guild '{owningGuildName}' not found.");
            }

            // Find the claim to remove
            var claimToRemove = guild.Claims.FirstOrDefault(c =>
            {
                if (c is GuildHomeClaim guildHome)
                {
                    return guildHome.ContainsBlockCoord(chunkX * GuildLandClaim.ChunkSize, chunkZ * GuildLandClaim.ChunkSize);
                }
                return c.ChunkX == chunkX && c.ChunkZ == chunkZ;
            });

            if (claimToRemove == null)
            {
                return TextCommandResult.Error($"Could not find claim for chunk ({chunkX}, {chunkZ}) in guild '{owningGuildName}'.");
            }

            // Handle guild home claims (multi-chunk)
            if (claimToRemove is GuildHomeClaim guildHomeClaim)
            {
                // Remove all chunks from the spatial index
                foreach (var chunk in guildHomeClaim.GetIndividualChunks())
                {
                    landClaimRepository.RemoveClaimFromIndex(chunk.ChunkX, chunk.ChunkZ);
                }
                guild.Claims.Remove(claimToRemove);
                guildRepository.MarkDirty(owningGuildName);

                serverApi?.Logger.Notification($"[GuildManager:Admin] {player.PlayerName} removed guild home claim for guild '{owningGuildName}'");
                networkHandler?.BroadcastGuildSummariesToAll();

                return TextCommandResult.Success($"Removed guild home claim for guild '{owningGuildName}' (4 chunks removed).");
            }

            // Handle outpost claims
            if (claimToRemove is OutpostClaim outpostClaim)
            {
                landClaimRepository.RemoveClaimFromIndex(chunkX, chunkZ);
                guild.Claims.Remove(claimToRemove);
                guildRepository.MarkDirty(owningGuildName);

                serverApi?.Logger.Notification($"[GuildManager:Admin] {player.PlayerName} removed outpost '{outpostClaim.OutpostName}' claim at ({chunkX}, {chunkZ}) for guild '{owningGuildName}'");
                networkHandler?.BroadcastGuildSummariesToAll();

                return TextCommandResult.Success($"Removed outpost '{outpostClaim.OutpostName}' claim at ({chunkX}, {chunkZ}) from guild '{owningGuildName}'.");
            }

            // Handle regular claims
            landClaimRepository.RemoveClaimFromIndex(chunkX, chunkZ);
            guild.Claims.Remove(claimToRemove);
            guildRepository.MarkDirty(owningGuildName);

            serverApi?.Logger.Notification($"[GuildManager:Admin] {player.PlayerName} removed claim at ({chunkX}, {chunkZ}) for guild '{owningGuildName}'");
            networkHandler?.BroadcastGuildSummariesToAll();

            return TextCommandResult.Success($"Removed claim at ({chunkX}, {chunkZ}) from guild '{owningGuildName}'.");
        }

        /// <summary>
        /// Admin command: Forcibly add a player to a guild
        /// Usage: /guildmanager addplayer <playerUsername> <Guild Name>
        /// </summary>
        private TextCommandResult OnGuildManagerAddPlayerCommand(TextCommandCallingArgs args)
        {
            if (guildManager == null || guildRepository == null)
                return TextCommandResult.Error("Guild system not initialized.");

            var playerUsername = args.Parsers[0].GetValue() as string;
            var guildName = args.Parsers[1].GetValue() as string;

            if (string.IsNullOrEmpty(playerUsername))
                return TextCommandResult.Error("Please specify a player username.");

            if (string.IsNullOrEmpty(guildName))
                return TextCommandResult.Error("Please specify a guild name.");

            // Look up player UID by username
            string? playerUid = GetPlayerUidByName(playerUsername);
            if (string.IsNullOrEmpty(playerUid))
            {
                return TextCommandResult.Error($"Player '{playerUsername}' not found.");
            }

            // Check if the player is already in a guild
            var existingGuild = guildManager.GetGuildByMember(playerUid);
            if (existingGuild != null)
            {
                return TextCommandResult.Error($"Player '{playerUsername}' is already a member of guild '{existingGuild.Name}'. Remove them from that guild first.");
            }

            // Get the target guild
            var guild = guildManager.GetGuild(guildName);
            if (guild == null)
            {
                return TextCommandResult.Error($"Guild '{guildName}' not found.");
            }

            // Check if guild is at max capacity
            var config = guildManager.GetConfigManager()?.GetConfig();
            if (config != null)
            {
                var maxMembers = config.MaxMembersPerGuild;
                if (guild.Members.Count >= maxMembers)
                {
                    return TextCommandResult.Error($"Guild '{guildName}' is at maximum capacity ({maxMembers} members).");
                }
            }

            // Add the player to the guild as a Member (default role)
            guild.Members[playerUid] = new GuildMember { PlayerUid = playerUid, Role = "Member" };
            guildRepository.MarkDirty(guild.Name);

            // Sync traits if the player is online
            var onlinePlayer = serverApi?.World.PlayerByUid(playerUid) as IServerPlayer;
            if (onlinePlayer != null)
            {
                guildManager.SyncPlayerTraits(onlinePlayer);
                onlinePlayer.SendMessage(GlobalConstants.GeneralChatGroup,
                    $"You have been added to guild '{guild.Name}' by an administrator.",
                    EnumChatType.Notification);
            }

            serverApi?.Logger.Notification($"[GuildManager:Admin] {args.Caller.Player?.PlayerName ?? "Console"} added player '{playerUsername}' (UID: {playerUid}) to guild '{guild.Name}'");
            networkHandler?.BroadcastGuildSummariesToAll();

            var onlineStatus = onlinePlayer != null ? " (player notified)" : " (player offline)";
            return TextCommandResult.Success($"Added player '{playerUsername}' to guild '{guild.Name}' as Member.{onlineStatus}");
        }

        #region Party Admin Commands

        /// <summary>
        /// Admin command: Forcibly add a player to the admin's party
        /// Usage: /partymanager addplayer <playerUID or username>
        /// </summary>
        private TextCommandResult OnPartyManagerAddPlayerCommand(TextCommandCallingArgs args)
        {
            if (partyManager == null)
                return TextCommandResult.Error("Party system not initialized.");

            var admin = args.Caller.Player as IServerPlayer;
            if (admin == null)
                return TextCommandResult.Error("Command can only be used by players.");

            var playerIdentifier = args.Parsers[0].GetValue() as string;
            if (string.IsNullOrEmpty(playerIdentifier))
                return TextCommandResult.Error("Please specify a player UID or username.");

            // Try to interpret as UID first, then as username
            string? targetPlayerUid = null;

            // Check if it looks like a GUID (contains hyphens or is hex-only)
            if (playerIdentifier.Contains("-") || System.Text.RegularExpressions.Regex.IsMatch(playerIdentifier, @"^[0-9a-fA-F]+$"))
            {
                // Treat as UID
                targetPlayerUid = playerIdentifier;
            }
            else
            {
                // Treat as username
                targetPlayerUid = GetPlayerUidByName(playerIdentifier);
                if (string.IsNullOrEmpty(targetPlayerUid))
                {
                    return TextCommandResult.Error($"Player '{playerIdentifier}' not found.");
                }
            }

            // Get or verify target player exists
            var targetPlayerData = serverApi?.PlayerData.PlayerDataByUid.ContainsKey(targetPlayerUid);
            if (targetPlayerData != true)
            {
                return TextCommandResult.Error($"Player with UID '{targetPlayerUid}' not found in player data.");
            }

            // Get admin's party
            var adminParty = partyManager.GetPartyByMember(admin.PlayerUID);
            if (adminParty == null)
            {
                return TextCommandResult.Error("You are not in a party. Create a party first.");
            }

            // Check if target is already in this party
            if (adminParty.HasMember(targetPlayerUid))
            {
                var targetName = GetPlayerNameByUid(targetPlayerUid) ?? targetPlayerUid;
                return TextCommandResult.Error($"Player '{targetName}' is already in your party.");
            }

            // Check if target is in another party
            var targetExistingParty = partyManager.GetPartyByMember(targetPlayerUid);
            if (targetExistingParty != null)
            {
                var targetName = GetPlayerNameByUid(targetPlayerUid) ?? targetPlayerUid;
                return TextCommandResult.Error($"Player '{targetName}' is already in another party: {targetExistingParty.Name}");
            }

            // Force add the player to the party
            var targetPlayer = serverApi?.World.PlayerByUid(targetPlayerUid) as IServerPlayer;
            var targetPlayerName = targetPlayer?.PlayerName ?? GetPlayerNameByUid(targetPlayerUid) ?? "Unknown";
            var isOnline = targetPlayer != null;

            adminParty.Members.Add(new PartyMember(targetPlayerUid, targetPlayerName, isOnline));

            // Notify the target player if they're online
            if (targetPlayer != null)
            {
                targetPlayer.SendMessage(GlobalConstants.GeneralChatGroup,
                    $"You have been added to party '{adminParty.Name}' by an administrator.",
                    EnumChatType.Notification);
            }

            // Use the network handler's SendPartyUpdate to broadcast to all members
            partyNetworkHandler?.SendPartyUpdate(adminParty);

            serverApi?.Logger.Notification($"[PartyManager:Admin] {admin.PlayerName} force-added player '{targetPlayerName}' (UID: {targetPlayerUid}) to party '{adminParty.Name}'");

            var onlineStatus = isOnline ? " (player notified)" : " (player offline)";
            return TextCommandResult.Success($"Added player '{targetPlayerName}' to your party '{adminParty.Name}'.{onlineStatus}");
        }

        #endregion

        #region Zone Whitelist Commands

        private TextCommandResult OnZoneWhitelistAddCommand(TextCommandCallingArgs args)
        {
            if (zoneWhitelistManager == null)
                return TextCommandResult.Error("Zone whitelist system not initialized.");

            // Parse zone ID
            var zoneId = (int?)args.Parsers[0].GetValue();
            if (!zoneId.HasValue)
                return TextCommandResult.Error("Please specify a valid zone ID.");

            // Parse player name (all remaining words)
            var playerName = args.Parsers[1].GetValue() as string ?? "";

            if (string.IsNullOrEmpty(playerName))
                return TextCommandResult.Error("Please specify a player name.");

            // Validate zone exists
            if (!ValidateZoneExistsById(zoneId.Value, out var zone))
            {
                return TextCommandResult.Error($"Protected zone with ID {zoneId} not found. Use /zonewhitelist zones to see all zones.");
            }

            // Get player UID
            string? playerUid = GetPlayerUidByName(playerName);
            if (string.IsNullOrEmpty(playerUid))
            {
                return TextCommandResult.Error($"Player '{playerName}' not found.");
            }

            // Add to whitelist
            bool added = zoneWhitelistManager.AddPlayerToZone(zone!.Id, playerUid);

            if (added)
            {
                // Notify the target player if they're online
                var targetPlayer = serverApi?.World.AllOnlinePlayers
                    .FirstOrDefault(p => p.PlayerUID == playerUid) as IServerPlayer;

                if (targetPlayer != null)
                {
                    targetPlayer.SendMessage(GlobalConstants.GeneralChatGroup,
                        $"You have been granted access to protected zone: {zone.Name} (ID: {zone.Id})",
                        EnumChatType.Notification);
                }

                return TextCommandResult.Success($"Added player '{playerName}' to zone '{zone.Name}' (ID: {zone.Id}) whitelist.");
            }
            else
            {
                return TextCommandResult.Success($"Player '{playerName}' is already whitelisted for zone '{zone.Name}' (ID: {zone.Id}).");
            }
        }

        private TextCommandResult OnZoneWhitelistRemoveCommand(TextCommandCallingArgs args)
        {
            if (zoneWhitelistManager == null)
                return TextCommandResult.Error("Zone whitelist system not initialized.");

            // Parse zone ID
            var zoneId = (int?)args.Parsers[0].GetValue();
            if (!zoneId.HasValue)
                return TextCommandResult.Error("Please specify a valid zone ID.");

            // Parse player name (all remaining words)
            var playerName = args.Parsers[1].GetValue() as string ?? "";

            if (string.IsNullOrEmpty(playerName))
                return TextCommandResult.Error("Please specify a player name.");

            // Validate zone exists
            if (!ValidateZoneExistsById(zoneId.Value, out var zone))
            {
                return TextCommandResult.Error($"Protected zone with ID {zoneId} not found. Use /zonewhitelist zones to see all zones.");
            }

            // Get player UID
            string? playerUid = GetPlayerUidByName(playerName);
            if (string.IsNullOrEmpty(playerUid))
            {
                return TextCommandResult.Error($"Player '{playerName}' not found.");
            }

            // Remove from whitelist
            bool removed = zoneWhitelistManager.RemovePlayerFromZone(zone!.Id, playerUid);

            if (removed)
            {
                // Notify the target player if they're online
                var targetPlayer = serverApi?.World.AllOnlinePlayers
                    .FirstOrDefault(p => p.PlayerUID == playerUid) as IServerPlayer;

                if (targetPlayer != null)
                {
                    targetPlayer.SendMessage(GlobalConstants.GeneralChatGroup,
                        $"Your access to protected zone '{zone.Name}' (ID: {zone.Id}) has been revoked.",
                        EnumChatType.Notification);
                }

                return TextCommandResult.Success($"Removed player '{playerName}' from zone '{zone.Name}' (ID: {zone.Id}) whitelist.");
            }
            else
            {
                return TextCommandResult.Success($"Player '{playerName}' was not whitelisted for zone '{zone.Name}' (ID: {zone.Id}).");
            }
        }

        private TextCommandResult OnZoneWhitelistListCommand(TextCommandCallingArgs args)
        {
            if (zoneWhitelistManager == null)
                return TextCommandResult.Error("Zone whitelist system not initialized.");

            var filter = args.Parsers[0].GetValue() as string ?? "";

            if (string.IsNullOrEmpty(filter))
            {
                // List all whitelists
                return ListAllWhitelists();
            }
            else
            {
                // Check if filter is a zone ID (numeric) or player name
                if (int.TryParse(filter, out int zoneId))
                {
                    // Filter by zone ID
                    if (!ValidateZoneExistsById(zoneId, out var zone))
                    {
                        return TextCommandResult.Error($"Protected zone with ID {zoneId} not found.");
                    }
                    return ListPlayersInZone(zone!);
                }
                else
                {
                    // Try as player name
                    string? playerUid = GetPlayerUidByName(filter);
                    if (!string.IsNullOrEmpty(playerUid))
                    {
                        return ListZonesForPlayer(filter, playerUid);
                    }
                    else
                    {
                        return TextCommandResult.Error($"'{filter}' is neither a valid zone ID nor player name.");
                    }
                }
            }
        }

        private TextCommandResult OnZoneWhitelistClearCommand(TextCommandCallingArgs args)
        {
            if (zoneWhitelistManager == null)
                return TextCommandResult.Error("Zone whitelist system not initialized.");

            var zoneId = (int?)args.Parsers[0].GetValue();
            if (!zoneId.HasValue)
                return TextCommandResult.Error("Please specify a valid zone ID.");

            // Validate zone exists
            if (!ValidateZoneExistsById(zoneId.Value, out var zone))
            {
                return TextCommandResult.Error($"Protected zone with ID {zoneId} not found.");
            }

            // Clear the zone
            int count = zoneWhitelistManager.ClearZone(zone!.Id);

            if (count > 0)
            {
                return TextCommandResult.Success($"Cleared {count} player(s) from zone '{zone.Name}' (ID: {zone.Id}) whitelist.");
            }
            else
            {
                return TextCommandResult.Success($"Zone '{zone.Name}' (ID: {zone.Id}) whitelist was already empty.");
            }
        }

        private TextCommandResult OnQuestItemCommand(TextCommandCallingArgs args)
        {
            var player = args.Caller.Player as IServerPlayer;
            if (player == null)
                return TextCommandResult.Error("Command can only be used by players.");

            // Get the active hotbar slot
            var slot = player.InventoryManager.ActiveHotbarSlot;
            if (slot?.Itemstack == null)
            {
                return TextCommandResult.Error("No item in hand! Hold an item and try again.");
            }

            var itemstack = slot.Itemstack;
            var code = itemstack.Collectible.Code.ToString();
            var amount = itemstack.StackSize;

            // Extract NBT data as Base64 string if present
            string? nbtBase64 = null;
            if (itemstack.Attributes != null && itemstack.Attributes.Count > 0)
            {
                try
                {
                    using var ms = new System.IO.MemoryStream();
                    using var writer = new System.IO.BinaryWriter(ms);
                    itemstack.Attributes.ToBytes(writer);
                    var nbtBytes = ms.ToArray();
                    nbtBase64 = Convert.ToBase64String(nbtBytes);
                }
                catch (Exception ex)
                {
                    serverApi?.Logger.Error($"[QuestItem] Failed to serialize NBT data: {ex.Message}");
                    return TextCommandResult.Error($"Failed to extract NBT data: {ex.Message}");
                }
            }

            // Log the item data to server-main.txt
            serverApi?.Logger.Notification("================================================");
            serverApi?.Logger.Notification($"[QuestItem] Item Code: {code}");
            serverApi?.Logger.Notification($"[QuestItem] Stack Size: {amount}");
            serverApi?.Logger.Notification($"[QuestItem] NBT Base64: {nbtBase64 ?? "NULL"}");
            serverApi?.Logger.Notification("================================================");

            // Also send confirmation to player
            return TextCommandResult.Success($"Item data logged to server-main.txt:\nCode: {code}\nAmount: {amount}\nNBT: {(nbtBase64 == null ? "None" : "See log")}");
        }

        #region Quest Admin Commands

        /// <summary>
        /// Admin command: Remove a player's quest progress by period key
        /// Usage: /quests removeprogress <playerUsername> <period_key>
        /// </summary>
        private TextCommandResult OnQuestRemoveProgressCommand(TextCommandCallingArgs args)
        {
            if (questRepository == null)
                return TextCommandResult.Error("Quest system not initialized.");

            var playerUsername = args.Parsers[0].GetValue() as string;
            var periodKey = args.Parsers[1].GetValue() as string;

            if (string.IsNullOrEmpty(playerUsername) || string.IsNullOrEmpty(periodKey))
                return TextCommandResult.Error("Please specify both player username and period key.");

            // Convert username to UID
            string? playerUid = GetPlayerUidByName(playerUsername);
            if (string.IsNullOrEmpty(playerUid))
            {
                return TextCommandResult.Error($"Player '{playerUsername}' not found.");
            }

            try
            {
                int removedCount = questRepository.RemovePlayerQuestProgressByPeriodKey(playerUid, periodKey);

                if (removedCount > 0)
                {
                    return TextCommandResult.Success($"Removed {removedCount} quest progress entry(s) for player '{playerUsername}' with period key '{periodKey}'.");
                }
                else
                {
                    return TextCommandResult.Success($"No quest progress found for player '{playerUsername}' with period key '{periodKey}'.");
                }
            }
            catch (Exception ex)
            {
                serverApi?.Logger.Error($"[QuestAdmin] Failed to remove quest progress: {ex.Message}");
                return TextCommandResult.Error($"Failed to remove quest progress: {ex.Message}");
            }
        }

        /// <summary>
        /// Admin command: Give GRS points to a guild
        /// Usage: /quests givegrs <amount> <guild name with spaces>
        /// </summary>
        private TextCommandResult OnQuestGiveGrsCommand(TextCommandCallingArgs args)
        {
            if (guildManager == null || guildRepository == null)
                return TextCommandResult.Error("Guild system not initialized.");

            var amount = args.Parsers[0].GetValue() as int?;
            var guildName = args.Parsers[1].GetValue() as string; // All() parser handles spaces

            if (!amount.HasValue || amount.Value == 0)
                return TextCommandResult.Error("Please specify a non-zero amount (positive to add, negative to remove).");

            if (string.IsNullOrEmpty(guildName))
                return TextCommandResult.Error("Please specify a guild name.");

            try
            {
                // Get the guild
                var guild = guildManager.GetGuild(guildName);
                if (guild == null)
                {
                    return TextCommandResult.Error($"Guild '{guildName}' not found.");
                }

                // Add/remove points (ensure points don't go below zero)
                int previousPoints = guild.Points;
                guild.Points = Math.Max(0, guild.Points + amount.Value);

                // Mark dirty to persist changes
                guildRepository.MarkDirty(guild.Name);

                var action = amount.Value > 0 ? "Added" : "Removed";
                var displayAmount = Math.Abs(amount.Value);
                serverApi?.Logger.Notification($"[QuestAdmin] {action} {displayAmount} GRS points {(amount.Value > 0 ? "to" : "from")} guild '{guildName}' (was {previousPoints}, now {guild.Points})");

                // Broadcast updated guild summaries to all online players
                networkHandler?.BroadcastGuildSummariesToAll();

                // Sync traits for all guild members to ensure any trait changes from crossing thresholds are applied immediately
                guildManager.SyncGuildMemberTraits(guild);

                return TextCommandResult.Success($"{action} {displayAmount} GRS points {(amount.Value > 0 ? "to" : "from")} guild '{guildName}'. New total: {guild.Points} GRS.");
            }
            catch (Exception ex)
            {
                serverApi?.Logger.Error($"[QuestAdmin] Failed to modify GRS points: {ex.Message}");
                return TextCommandResult.Error($"Failed to modify GRS points: {ex.Message}");
            }
        }

        /// <summary>
        /// Quest Manager command: Opens the quest manager GUI
        /// Usage: /questmanager or /qm
        /// </summary>
        private TextCommandResult OnQuestManagerCommand(TextCommandCallingArgs args)
        {
            var player = args.Caller.Player as IServerPlayer;
            if (player == null)
                return TextCommandResult.Error("Command can only be used by players.");

            // Send a packet to tell the client to open the quest manager dialog
            var packet = new OpenQuestManagerPacket { PlayerUid = player.PlayerUID };
            serverApi!.Network.GetChannel("srguildsandkingdoms:quest")!.SendPacket(packet, player);

            return TextCommandResult.Success("Opening Quest Manager...");
        }

        /// <summary>
        /// Admin command: Set quest currency definition from held item
        /// Usage: /questmanager set [crowns|tails]
        /// </summary>
        private TextCommandResult OnQuestManagerSetCurrencyCommand(TextCommandCallingArgs args)
        {
            if (guildManager == null)
                return TextCommandResult.Error("Guild system not initialized.");

            var player = args.Caller.Player as IServerPlayer;
            if (player == null)
                return TextCommandResult.Error("Command can only be used by players.");

            var currencyType = args.Parsers[0].GetValue() as string;
            if (string.IsNullOrEmpty(currencyType))
                return TextCommandResult.Error("Please specify currency type: crowns or tails");

            // Validate currency type
            if (!currencyType.Equals("crowns", StringComparison.OrdinalIgnoreCase) &&
                !currencyType.Equals("tails", StringComparison.OrdinalIgnoreCase))
            {
                return TextCommandResult.Error("Currency type must be 'crowns' or 'tails'");
            }

            // Get the active hotbar slot
            var slot = player.InventoryManager.ActiveHotbarSlot;
            if (slot?.Itemstack == null)
            {
                return TextCommandResult.Error("No item in hand! Hold an item and try again.");
            }

            var itemstack = slot.Itemstack;
            var code = itemstack.Collectible.Code.ToString();

            // Extract NBT data as Base64 string if present
            string? nbtBase64 = null;
            if (itemstack.Attributes != null && itemstack.Attributes.Count > 0)
            {
                try
                {
                    using var ms = new System.IO.MemoryStream();
                    using var writer = new System.IO.BinaryWriter(ms);
                    itemstack.Attributes.ToBytes(writer);
                    var nbtBytes = ms.ToArray();
                    nbtBase64 = Convert.ToBase64String(nbtBytes);
                }
                catch (Exception ex)
                {
                    serverApi?.Logger.Error($"[QuestCurrency] Failed to serialize NBT data: {ex.Message}");
                    return TextCommandResult.Error($"Failed to extract NBT data: {ex.Message}");
                }
            }

            // Update the currency definition in config
            try
            {
                var configManager = guildManager.GetConfigManager();
                configManager.UpdateQuestCurrency(currencyType, code, nbtBase64);

                var currencyName = currencyType.Equals("crowns", StringComparison.OrdinalIgnoreCase) ? "Crowns" : "Tails";
                var nbtInfo = nbtBase64 == null ? "" : " (with NBT data)";

                serverApi?.Logger.Notification($"[QuestCurrency] {player.PlayerName} set {currencyName} currency to: {code}{nbtInfo}");

                return TextCommandResult.Success($"Set {currencyName} currency to: {code}{nbtInfo}");
            }
            catch (Exception ex)
            {
                serverApi?.Logger.Error($"[QuestCurrency] Failed to update currency: {ex.Message}");
                return TextCommandResult.Error($"Failed to update currency: {ex.Message}");
            }
        }

        #endregion

        private TextCommandResult OnZoneWhitelistZonesCommand(TextCommandCallingArgs args)
        {
            var config = guildManager?.GetConfigManager()?.GetConfig();
            if (config?.ProtectedZones == null || config.ProtectedZones.Count == 0)
            {
                return TextCommandResult.Success("No protected zones configured.");
            }

            var output = new System.Text.StringBuilder();
            output.AppendLine($"=== PROTECTED ZONES ({config.ProtectedZones.Count}) ===");
            output.AppendLine("ID | Name | Center | Radius");
            output.AppendLine("---|------|--------|-------");

            foreach (var zone in config.ProtectedZones.OrderBy(z => z.Id))
            {
                output.AppendLine($"{zone.Id} | {zone.Name} | ({zone.X}, {zone.Z}) | {zone.Radius} blocks");
            }

            // Log to console
            Mod.Logger.Notification(output.ToString());

            return TextCommandResult.Success($"Listed {config.ProtectedZones.Count} protected zone(s). Check server console for details.");
        }

        #endregion

        #region Helper Methods for Zone Whitelist

        /// <summary>
        /// Gets a player's UID by their name (checks both online and offline players)
        /// </summary>
        private string? GetPlayerUidByName(string playerName)
        {
            if (string.IsNullOrEmpty(playerName) || serverApi == null)
                return null;

            // First check online players
            var onlinePlayer = serverApi.World.AllOnlinePlayers
                .FirstOrDefault(p => p.PlayerName.Equals(playerName, StringComparison.OrdinalIgnoreCase));

            if (onlinePlayer != null)
                return onlinePlayer.PlayerUID;

            // Check offline players via player data
            var playerData = serverApi.PlayerData.PlayerDataByUid
                .FirstOrDefault(kvp => kvp.Value.LastKnownPlayername?.Equals(playerName, StringComparison.OrdinalIgnoreCase) == true);

            return string.IsNullOrEmpty(playerData.Key) ? null : playerData.Key;
        }

        /// <summary>
        /// Validates if a zone exists in the configuration by its ID
        /// </summary>
        private bool ValidateZoneExistsById(int zoneId, out ProtectedZone? zone)
        {
            zone = null;

            if (zoneId < 0)
                return false;

            var config = guildManager?.GetConfigManager()?.GetConfig();
            if (config?.ProtectedZones == null)
                return false;

            zone = config.ProtectedZones.FirstOrDefault(z => z.Id == zoneId);

            return zone != null;
        }

        /// <summary>
        /// Lists all zone whitelists
        /// </summary>
        private TextCommandResult ListAllWhitelists()
        {
            if (zoneWhitelistManager == null || serverApi == null)
                return TextCommandResult.Error("Zone whitelist system not initialized.");

            var allZoneIds = zoneWhitelistManager.GetAllZoneIds();

            if (allZoneIds.Count == 0)
            {
                return TextCommandResult.Success("No zone whitelists configured.");
            }

            var config = guildManager?.GetConfigManager()?.GetConfig();
            var output = new System.Text.StringBuilder();
            output.AppendLine($"=== ZONE WHITELISTS ({allZoneIds.Count} zones) ===");

            foreach (var zoneId in allZoneIds.OrderBy(z => z))
            {
                var zone = config?.ProtectedZones?.FirstOrDefault(z => z.Id == zoneId);
                var zoneName = zone?.Name ?? $"Unknown (ID: {zoneId})";
                var players = zoneWhitelistManager.GetWhitelistedPlayers(zoneId);
                output.AppendLine($"\n[ID: {zoneId}] {zoneName} - {players.Count} player(s):");

                foreach (var playerUid in players)
                {
                    var playerName = GetPlayerNameByUid(playerUid);
                    output.AppendLine($"  • {playerName ?? playerUid}");
                }
            }

            // Log to console
            Mod.Logger.Notification(output.ToString());

            return TextCommandResult.Success($"Listed {allZoneIds.Count} zone whitelist(s). Check server console for details.");
        }

        /// <summary>
        /// Lists all players whitelisted for a specific zone
        /// </summary>
        private TextCommandResult ListPlayersInZone(ProtectedZone zone)
        {
            if (zoneWhitelistManager == null || serverApi == null)
                return TextCommandResult.Error("Zone whitelist system not initialized.");

            var players = zoneWhitelistManager.GetWhitelistedPlayers(zone.Id);

            if (players.Count == 0)
            {
                return TextCommandResult.Success($"No players whitelisted for zone '{zone.Name}' (ID: {zone.Id}).");
            }

            var output = new System.Text.StringBuilder();
            output.AppendLine($"=== WHITELISTED PLAYERS FOR '{zone.Name}' (ID: {zone.Id}) ({players.Count}) ===");

            foreach (var playerUid in players)
            {
                var playerName = GetPlayerNameByUid(playerUid);
                output.AppendLine($"  • {playerName ?? playerUid}");
            }

            // Log to console
            Mod.Logger.Notification(output.ToString());

            return TextCommandResult.Success($"Listed {players.Count} player(s) for zone '{zone.Name}'. Check server console for details.");
        }

        /// <summary>
        /// Lists all zones a player is whitelisted for
        /// </summary>
        private TextCommandResult ListZonesForPlayer(string playerName, string playerUid)
        {
            if (zoneWhitelistManager == null || serverApi == null)
                return TextCommandResult.Error("Zone whitelist system not initialized.");

            var zoneIds = zoneWhitelistManager.GetWhitelistedZones(playerUid);

            if (zoneIds.Count == 0)
            {
                return TextCommandResult.Success($"Player '{playerName}' is not whitelisted for any zones.");
            }

            var config = guildManager?.GetConfigManager()?.GetConfig();
            var output = new System.Text.StringBuilder();
            output.AppendLine($"=== ZONES FOR '{playerName}' ({zoneIds.Count}) ===");

            foreach (var zoneId in zoneIds.OrderBy(z => z))
            {
                var zone = config?.ProtectedZones?.FirstOrDefault(z => z.Id == zoneId);
                var zoneName = zone?.Name ?? $"Unknown (ID: {zoneId})";
                output.AppendLine($"  • [ID: {zoneId}] {zoneName}");
            }

            // Log to console
            Mod.Logger.Notification(output.ToString());

            return TextCommandResult.Success($"Player '{playerName}' is whitelisted for {zoneIds.Count} zone(s). Check server console for details.");
        }

        /// <summary>
        /// Gets a player's name by their UID
        /// </summary>
        private string? GetPlayerNameByUid(string playerUid)
        {
            if (string.IsNullOrEmpty(playerUid) || serverApi == null)
                return null;

            // Check online players first
            var onlinePlayer = serverApi.World.AllOnlinePlayers
                .FirstOrDefault(p => p.PlayerUID == playerUid);

            if (onlinePlayer != null)
                return onlinePlayer.PlayerName;

            // Check player data
            if (serverApi.PlayerData.PlayerDataByUid.TryGetValue(playerUid, out var playerData))
            {
                return playerData.LastKnownPlayername;
            }

            return null;
        }

        #endregion

        private TextCommandResult OnListInventoryCommand(TextCommandCallingArgs args)
        {
            var player = args.Caller.Player as IServerPlayer;
            if (player == null)
                return TextCommandResult.Error("Command can only be used by players.");

            var output = new System.Text.StringBuilder();
            output.AppendLine("=== INVENTORY LISTING ===");
            output.AppendLine("Format: [Quantity] ItemCode - ItemName\n");

            var itemsByCode = new Dictionary<string, (int quantity, string name)>();
            int totalSlots = 0;
            int filledSlots = 0;

            // Scan all player inventories using server-side inventory manager
            var inventoryManager = player.InventoryManager;
            if (inventoryManager == null)
                return TextCommandResult.Error("Could not access inventory manager.");

            foreach (var invPair in inventoryManager.Inventories)
            {
                var invClassName = invPair.Key;
                var inv = invPair.Value;

                // Skip creative inventory
                if (inv.ClassName == "creative")
                    continue;

                output.AppendLine($"--- {invClassName} ---");

                for (int i = 0; i < inv.Count; i++)
                {
                    totalSlots++;
                    var slot = inv[i];
                    if (slot.Empty)
                        continue;

                    filledSlots++;
                    var itemStack = slot.Itemstack;
                    var itemCode = itemStack.Collectible.Code.ToString();
                    var itemName = itemStack.GetName();
                    var quantity = itemStack.StackSize;

                    // Track unique items across all inventories
                    if (itemsByCode.ContainsKey(itemCode))
                    {
                        var existing = itemsByCode[itemCode];
                        itemsByCode[itemCode] = (existing.quantity + quantity, existing.name);
                    }
                    else
                    {
                        itemsByCode[itemCode] = (quantity, itemName);
                    }

                    output.AppendLine($"  [{quantity}] {itemCode}");
                    output.AppendLine($"      └─ {itemName}");
                }

                output.AppendLine();
            }

            // Add summary section
            output.AppendLine("=== UNIQUE ITEMS SUMMARY ===");
            output.AppendLine($"Total unique items: {itemsByCode.Count}");
            output.AppendLine($"Filled slots: {filledSlots}/{totalSlots}\n");

            foreach (var kvp in itemsByCode.OrderBy(x => x.Key))
            {
                output.AppendLine($"[{kvp.Value.quantity}] {kvp.Key}");
            }

            // Write to log file in the world's ModData folder
            var worldDataPath = serverApi?.GetOrCreateDataPath("ModData");
            if (worldDataPath == null)
                return TextCommandResult.Error("Could not access world data path.");

            var logPath = System.IO.Path.Combine(worldDataPath, $"inventory_listing_{player.PlayerName}.txt");
            try
            {
                System.IO.File.WriteAllText(logPath, output.ToString());
                player.SendMessage(GlobalConstants.GeneralChatGroup,
                    $"Inventory listing written to: {logPath}",
                    EnumChatType.Notification);
                return TextCommandResult.Success($"Inventory listing saved to {logPath}");
            }
            catch (Exception ex)
            {
                player.SendMessage(GlobalConstants.GeneralChatGroup,
                    $"Error writing inventory listing: {ex.Message}",
                    EnumChatType.Notification);
                return TextCommandResult.Error($"Failed to write inventory listing: {ex.Message}");
            }
        }

        private TextCommandResult OnListTraitsCommand(TextCommandCallingArgs args)
        {
            var player = args.Caller.Player as IServerPlayer;
            if (player == null)
                return TextCommandResult.Error("Command can only be used by players.");

            try
            {
                // Get the CharacterSystem
                var characterSystem = serverApi?.ModLoader.GetModSystem<Vintagestory.GameContent.CharacterSystem>();
                if (characterSystem == null)
                {
                    return TextCommandResult.Error("CharacterSystem not found. Make sure the character system mod is loaded.");
                }

                var output = new System.Text.StringBuilder();
                output.AppendLine("=== YOUR ACTIVE TRAITS ===");
                output.AppendLine($"Player: {player.PlayerName}");
                output.AppendLine();

                // Use reflection to access the traits list
                var watchedAttributes = player.Entity.WatchedAttributes;
                if (watchedAttributes == null)
                {
                    return TextCommandResult.Error("No attributes");
                }

                // Get current extraTraits array (this is where HasTrait looks for extra traits)
                var traitsList = watchedAttributes.GetStringArray("extraTraits")?.ToList() ?? new List<string>();
                if (traitsList == null || traitsList.Count == 0)
                {
                    output.AppendLine("No traits found in the character system.");
                }
                else
                {
                    var traitType = typeof(Vintagestory.GameContent.Trait);
                    var codeProperty = traitType.GetProperty("Code");
                    var typeProperty = traitType.GetProperty("Type");

                    // Get guild-granted traits
                    var guildTraits = new HashSet<string>();
                    var techBlocks = TechBlocks;
                    foreach (var tech in techBlocks)
                    {
                        if (tech.GrantedTraits != null)
                        {
                            foreach (var trait in tech.GrantedTraits)
                            {
                                guildTraits.Add(trait);
                            }
                        }
                    }

                    output.AppendLine("--- All Traits ---");
                    int count = 0;
                    foreach (var trait in traitsList)
                    {
                        if (codeProperty != null)
                        {
                            var code = codeProperty.GetValue(trait) as string;
                            var type = typeProperty?.GetValue(trait)?.ToString() ?? "Unknown";

                            if (!string.IsNullOrEmpty(code))
                            {
                                count++;
                                var isGuildTrait = guildTraits.Contains(code);
                                var marker = isGuildTrait ? " [GUILD]" : "";
                                output.AppendLine($"{count}. {code} (Type: {type}){marker}");
                            }
                        }
                    }

                    if (count == 0)
                    {
                        output.AppendLine("No traits currently active.");
                    }
                    else
                    {
                        output.AppendLine();
                        output.AppendLine($"Total traits: {count}");

                        // Count guild traits
                        int guildTraitCount = 0;
                        foreach (var trait in traitsList)
                        {
                            if (codeProperty != null)
                            {
                                var code = codeProperty.GetValue(trait) as string;
                                if (!string.IsNullOrEmpty(code) && guildTraits.Contains(code))
                                {
                                    guildTraitCount++;
                                }
                            }
                        }
                        output.AppendLine($"Guild-granted traits: {guildTraitCount}");
                    }
                }

                // Show player's guild info
                output.AppendLine();
                output.AppendLine("--- Guild Information ---");
                var guild = guildManager?.GetGuildByMember(player.PlayerUID);
                if (guild != null)
                {
                    output.AppendLine($"Guild: {guild.Name}");
                    output.AppendLine($"Role: {guild.Members[player.PlayerUID].Role}");

                    // List unlocked techs
                    var unlockedTechs = guild.TechProgress.Values.Where(tp => tp.IsUnlocked).ToList();
                    output.AppendLine($"Unlocked technologies: {unlockedTechs.Count}");

                    foreach (var techProgress in unlockedTechs)
                    {
                        var techBlock = TechBlocks.FirstOrDefault(tb => tb.Id == techProgress.TechBlockId);
                        if (techBlock != null && techBlock.GrantedTraits != null && techBlock.GrantedTraits.Count > 0)
                        {
                            output.AppendLine($"  • {techBlock.Text}: {string.Join(", ", techBlock.GrantedTraits)}");
                        }
                    }
                }
                else
                {
                    output.AppendLine("You are not in a guild.");
                }

                // Send the output to chat and console
                var lines = output.ToString().Split(new[] { Environment.NewLine }, StringSplitOptions.None);
                foreach (var line in lines)
                {
                    player.SendMessage(GlobalConstants.GeneralChatGroup, line, EnumChatType.CommandSuccess);
                }

                return TextCommandResult.Success("Trait listing complete.");
            }
            catch (Exception ex)
            {
                serverApi?.Logger.Error($"[ListTraits] Error listing traits: {ex.Message}");
                serverApi?.Logger.Error($"[ListTraits] Stack trace: {ex.StackTrace}");
                return TextCommandResult.Error($"Failed to list traits: {ex.Message}");
            }
        }

        private TextCommandResult OnDebugInviteCommand(TextCommandCallingArgs args)
        {
            var player = args.Caller.Player as IServerPlayer;
            if (player == null) return TextCommandResult.Error("Command can only be used by players.");

            var guildName = args.Parsers[0].GetValue() as string;
            var inviterName = args.Parsers[1].GetValue() as string ?? "TestPlayer";
            var expirySeconds = args.Parsers[2].GetValue() as int?;

            if (string.IsNullOrEmpty(guildName))
                return TextCommandResult.Error("Please specify a guild name.");

            // Default to 5 minutes if not specified, or use custom seconds
            int seconds = expirySeconds ?? 300; // 300 seconds = 5 minutes

            var expiresAt = DateTime.UtcNow.AddSeconds(seconds);
            var expiresAtTicks = expiresAt.Ticks;

            // Debug logging
            Mod.Logger.Notification($"[DebugInvite] Creating invite for {player.PlayerName}");
            Mod.Logger.Notification($"[DebugInvite] Current UTC: {DateTime.UtcNow}");
            Mod.Logger.Notification($"[DebugInvite] ExpiresAt: {expiresAt}");
            Mod.Logger.Notification($"[DebugInvite] ExpiresAtTicks: {expiresAtTicks}");
            Mod.Logger.Notification($"[DebugInvite] Seconds until expiry: {seconds}");

            // Create a fake invite notification packet
            var inviteNotification = new GuildInviteNotificationPacket
            {
                PlayerUid = player.PlayerUID,
                InviterName = inviterName,
                InviterUid = $"debug-inviter-{Guid.NewGuid()}",
                GuildName = guildName,
                ExpiresAtTicks = expiresAtTicks
            };

            // Send it to the player
            serverApi!.Network.GetChannel("srguildsandkingdoms:guild")!
                .SendPacket(inviteNotification, player);

            var expiryText = seconds < 60
                ? $"{seconds} seconds"
                : $"{seconds / 60} minutes";

            return TextCommandResult.Success(
                $"Debug invite sent from '{inviterName}' to join '{guildName}' (expires in {expiryText})");
        }

        /// <summary>
        /// Processes expired repeating quests and renews them with new dates
        /// This is called on server start and during game saves
        /// </summary>
        private void ProcessExpiredRepeatingQuests()
        {
            if (questRepository == null)
            {
                return;
            }

            try
            {
                // Clean up stale active progress for expired quests
                try
                {
                    var calendar = serverApi?.World.Calendar;
                    GameDate? ingameDate = null;

                    if (calendar != null)
                    {
                        ingameDate = new GameDate(
                            calendar.Year + 1,
                            calendar.Month,
                            (calendar.DayOfYear % calendar.DaysPerMonth) + 1
                        );
                    }

                    int cleanedCount = questRepository.CleanupExpiredQuestProgress(null, ingameDate);

                    if (cleanedCount > 0)
                    {
                        serverApi?.Logger.Notification($"[SRGuildsAndKingdoms:Quests] Cleaned up {cleanedCount} stale active quest progress entries");
                    }
                }
                catch (Exception ex)
                {
                    serverApi?.Logger.Error($"[SRGuildsAndKingdoms:Quests] Failed to cleanup stale quest progress: {ex.Message}");
                }

                var currentDate = QuestTimeHelper.TodayEastern;
                var expiredQuests = questRepository.GetExpiredRepeatingQuests(currentDate);

                if (expiredQuests.Count == 0)
                {
                    return;
                }

                serverApi?.Logger.Notification($"[SRGuildsAndKingdoms:Quests] Renewing {expiredQuests.Count} expired repeating quest(s)");

                foreach (var quest in expiredQuests)
                {
                    try
                    {
                        var oldStartsAt = quest.StartsAt;
                        var oldExpiresAt = quest.ExpiresAt;

                        // For weeklies, re-set dates to be a range of the next week
                        var oldWeekStartDate = DateTime.Parse(quest.StartsAt);
                        var oldWeekExpireDate = DateTime.Parse(quest.ExpiresAt);
                        var newWeekStartDate = oldWeekStartDate.AddDays(7);
                        var newWeekExpireDate = oldWeekExpireDate.AddDays(7);
                        quest.StartsAt = QuestPeriodKeyGenerator.FormatDate(newWeekStartDate);
                        quest.ExpiresAt = QuestPeriodKeyGenerator.FormatDate(newWeekExpireDate);

                        // Update the quest in the database
                        bool updated = questRepository.UpdateQuest(quest);

                        if (!updated)
                        {
                            serverApi?.Logger.Error($"[SRGuildsAndKingdoms:Quests] Failed to renew quest {quest.Id} '{quest.Title}' in database");
                        }
                    }
                    catch (Exception ex)
                    {
                        serverApi?.Logger.Error($"[SRGuildsAndKingdoms:Quests] Failed to renew quest {quest.Id} '{quest.Title}': {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                serverApi?.Logger.Error($"[SRGuildsAndKingdoms:Quests] Failed to process expired repeating quests: {ex.Message}");
            }
        }

        /// <summary>
        /// Processes weekly GRS point data cleanup, removing old entries beyond the retention period
        /// Called on server start and during game saves
        /// </summary>
        private void ProcessWeeklyGrsReset()
        {
            if (weeklyPointsRepository == null)
            {
                return;
            }

            try
            {
                // Generate current week key and cutoff week key (8 weeks ago)
                var currentWeekKey = WeekKeyHelper.GenerateWeekKey(DateTime.Now);
                var cutoffDate = DateTime.Now.AddDays(-56); // 8 weeks ago (56 days)
                var cutoffWeekKey = WeekKeyHelper.GenerateWeekKey(cutoffDate);

                // Cleanup old weekly data
                int cleaned = weeklyPointsRepository.CleanupOldWeeklyData(cutoffWeekKey);

                if (cleaned > 0)
                {
                    serverApi?.Logger.Debug($"[SRGuildsAndKingdoms:WeeklyGRS] Cleaned up {cleaned} old weekly point entries (before week {cutoffWeekKey})");
                }
            }
            catch (Exception ex)
            {
                serverApi?.Logger.Error($"[SRGuildsAndKingdoms:WeeklyGRS] Failed to process weekly GRS reset: {ex.Message}");
            }
        }

        private void OnSaveGameLoaded()
        {
            guildManager!.OnSaveGameLoading();
            zoneWhitelistManager?.Load();
            // Broadcast guild summaries and config to all connected clients
            networkHandler!.BroadcastGuildSummariesToAll();
            networkHandler!.BroadcastGuildConfigToAll();

            // Cleanup old weekly GRS point data
            ProcessWeeklyGrsReset();
        }

        private void OnSaveGameSaving()
        {
            guildManager!.OnSaveGameSaving();

            // Process expired repeating quests and renew them
            ProcessExpiredRepeatingQuests();

            // Cleanup old weekly GRS point data
            ProcessWeeklyGrsReset();

            // Execute WAL checkpoint to flush changes to main database file
            try
            {
                guildDatabase?.Checkpoint();
                serverApi?.Logger.Debug("[SRGuildsAndKingdoms:Database] Database checkpoint complete");
            }
            catch (Exception ex)
            {
                serverApi?.Logger.Error($"[SRGuildsAndKingdoms:Database] Database checkpoint failed: {ex.Message}");
            }

            CreateGuildBackup();
        }

        /// <summary>
        /// Creates timestamped backups of SQLite database files
        /// </summary>
        private void CreateGuildBackup()
        {
            try
            {
                // Get backup directory path
                var backupDir = System.IO.Path.Combine(
                    serverApi!.GetOrCreateDataPath("ModData/SRGuildsAndKingdoms"),
                    "backups"
                );

                // Ensure backup directory exists
                System.IO.Directory.CreateDirectory(backupDir);

                // Create timestamp for this backup
                var timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");

                // Backup SQLite database file
                var dbPath = System.IO.Path.Combine(
                    serverApi.GetOrCreateDataPath("ModData/SRGuildsAndKingdoms"),
                    "guilds.db"
                );

                if (System.IO.File.Exists(dbPath))
                {
                    var dbBackupPath = System.IO.Path.Combine(backupDir, $"guilds_{timestamp}.db");
                    System.IO.File.Copy(dbPath, dbBackupPath, overwrite: true);
                    Mod.Logger.Debug($"[SRGuildsAndKingdoms:Backup] Database backed up to: {dbBackupPath}");

                    // Also backup WAL file if it exists
                    var walPath = dbPath + "-wal";
                    if (System.IO.File.Exists(walPath))
                    {
                        var walBackupPath = System.IO.Path.Combine(backupDir, $"guilds_{timestamp}.db-wal");
                        System.IO.File.Copy(walPath, walBackupPath, overwrite: true);
                        Mod.Logger.Debug($"[SRGuildsAndKingdoms:Backup] WAL file backed up to: {walBackupPath}");
                    }

                    // Backup SHM file if it exists
                    var shmPath = dbPath + "-shm";
                    if (System.IO.File.Exists(shmPath))
                    {
                        var shmBackupPath = System.IO.Path.Combine(backupDir, $"guilds_{timestamp}.db-shm");
                        System.IO.File.Copy(shmPath, shmBackupPath, overwrite: true);
                        Mod.Logger.Debug($"[SRGuildsAndKingdoms:Backup] SHM file backed up to: {shmBackupPath}");
                    }
                }

                // Clean up old backups (keep only last 10)
                CleanupOldBackups(backupDir, maxBackupsToKeep: 10);
            }
            catch (Exception ex)
            {
                Mod.Logger.Error($"[SRGuildsAndKingdoms:Backup] Failed to create database backup: {ex.Message}");
            }
        }

        /// <summary>
        /// Cleans up old backup files by zipping them by day, keeping only the most recent individual backups
        /// </summary>
        private void CleanupOldBackups(string backupDir, int maxBackupsToKeep)
        {
            try
            {
                ProcessBackupType(backupDir, "guilds_*.db", "guilds", maxBackupsToKeep);
                ProcessBackupType(backupDir, "guilds_*.db-wal", "guilds_wal", maxBackupsToKeep);
                ProcessBackupType(backupDir, "guilds_*.db-shm", "guilds_shm", maxBackupsToKeep);
            }
            catch (Exception ex)
            {
                Mod.Logger.Warning($"[SRGuildsAndKingdoms:Backup] Failed to cleanup old backups: {ex.Message}");
            }
        }

        /// <summary>
        /// Process a specific type of backup file (guild data or config)
        /// </summary>
        private void ProcessBackupType(string backupDir, string searchPattern, string backupType, int maxBackupsToKeep)
        {
            try
            {
                // Get all backup files of this type, sorted by creation time (newest first)
                var allBackups = System.IO.Directory.GetFiles(backupDir, searchPattern)
                    .Select(f => new System.IO.FileInfo(f))
                    .OrderByDescending(f => f.CreationTime)
                    .ToList();

                if (allBackups.Count <= maxBackupsToKeep)
                {
                    // Not enough backups to warrant cleanup
                    return;
                }

                // Keep the most recent backups as individual files
                var recentBackups = allBackups.Take(maxBackupsToKeep).ToList();
                var oldBackups = allBackups.Skip(maxBackupsToKeep).ToList();

                if (oldBackups.Count == 0)
                {
                    return;
                }

                // Group old backups by date (day)
                var backupsByDate = oldBackups
                    .GroupBy(f => f.CreationTime.Date)
                    .OrderBy(g => g.Key)
                    .ToList();

                Mod.Logger.Debug($"[SRGuildsAndKingdoms:Backup] Processing {oldBackups.Count} old {backupType} backups across {backupsByDate.Count} days");

                foreach (var dailyGroup in backupsByDate)
                {
                    var date = dailyGroup.Key;
                    var dateString = date.ToString("yyyy-MM-dd");
                    var zipFileName = System.IO.Path.Combine(backupDir, $"{backupType}_archive_{dateString}.zip");

                    // Check if zip already exists
                    if (System.IO.File.Exists(zipFileName))
                    {
                        // Add to existing zip
                        using (var archive = ZipFile.Open(zipFileName, ZipArchiveMode.Update))
                        {
                            foreach (var file in dailyGroup)
                            {
                                var entryName = System.IO.Path.GetFileName(file.FullName);

                                // Check if entry already exists in zip
                                var existingEntry = archive.GetEntry(entryName);
                                if (existingEntry == null)
                                {
                                    archive.CreateEntryFromFile(file.FullName, entryName, CompressionLevel.Optimal);
                                    Mod.Logger.Debug($"[SRGuildsAndKingdoms:Backup] Added {entryName} to existing archive {dateString}");
                                }

                                // Delete the original file
                                file.Delete();
                            }
                        }
                    }
                    else
                    {
                        // Create new zip archive for this day
                        using (var archive = ZipFile.Open(zipFileName, ZipArchiveMode.Create))
                        {
                            foreach (var file in dailyGroup)
                            {
                                var entryName = System.IO.Path.GetFileName(file.FullName);
                                archive.CreateEntryFromFile(file.FullName, entryName, CompressionLevel.Optimal);

                                // Delete the original file
                                file.Delete();
                            }
                        }

                        Mod.Logger.Debug($"[SRGuildsAndKingdoms:Backup] Created {backupType} archive for {dateString} with {dailyGroup.Count()} backup(s)");
                    }
                }

                // Optional: Delete old zip archives (e.g., older than 60 days)
                CleanupOldZipArchives(backupDir, backupType, daysToKeep: 60);
            }
            catch (Exception ex)
            {
                Mod.Logger.Warning($"[SRGuildsAndKingdoms:Backup] Failed to process {backupType} backups: {ex.Message}");
            }
        }

        /// <summary>
        /// Removes zip archives older than the specified number of days
        /// </summary>
        private void CleanupOldZipArchives(string backupDir, string backupType, int daysToKeep)
        {
            try
            {
                var cutoffDate = DateTime.Now.AddDays(-daysToKeep);
                var archivePattern = $"{backupType}_archive_*.zip";

                var oldArchives = System.IO.Directory.GetFiles(backupDir, archivePattern)
                    .Where(f => System.IO.File.GetCreationTime(f) < cutoffDate)
                    .ToList();

                foreach (var archive in oldArchives)
                {
                    System.IO.File.Delete(archive);
                    Mod.Logger.Debug($"[SRGuildsAndKingdoms:Backup] Deleted old archive: {System.IO.Path.GetFileName(archive)}");
                }

                if (oldArchives.Count > 0)
                {
                    Mod.Logger.Notification($"[SRGuildsAndKingdoms:Backup] Deleted {oldArchives.Count} {backupType} archive(s) older than {daysToKeep} days");
                }
            }
            catch (Exception ex)
            {
                Mod.Logger.Warning($"[SRGuildsAndKingdoms:Backup] Failed to cleanup old {backupType} archives: {ex.Message}");
            }
        }

        /// <summary>
        /// Event handler for entity deaths to track kill quest objectives
        /// </summary>
        private void OnEntityDeath(Entity entity, DamageSource damageSource)
        {
            // Check if the killer was a player
            if (damageSource?.GetCauseEntity() is not EntityPlayer killerPlayer)
                return;

            var killerServerPlayer = killerPlayer.Player as IServerPlayer;
            if (killerServerPlayer == null || guildManager == null || questRepository == null)
                return;

            var killerUid = killerServerPlayer.PlayerUID;
            var killerGuild = guildManager.GetGuildByMember(killerUid);
            if (killerGuild == null)
                return; // Player not in a guild

            // Get the entity code that was killed
            var entityCode = entity.Code?.ToString();
            if (string.IsNullOrEmpty(entityCode))
                return;

            // Get current in-game date for filtering IGT quests
            var calendar = serverApi!.World.Calendar;
            var ingameDate = new GameDate(
                calendar.Year + 1,
                calendar.Month,
                (calendar.DayOfYear % calendar.DaysPerMonth) + 1
            );

            // Get player's active quests (excludes expired quests)
            var activeQuests = questRepository.GetPlayerActiveQuests(killerUid, ingameDate);
            if (activeQuests.Count == 0)
                return;

            bool progressUpdated = false;

            // Check each active quest for kill objectives
            foreach (var questProgress in activeQuests)
            {
                var quest = questRepository.GetQuest(questProgress.QuestId);
                if (quest == null)
                    continue;

                // Check each objective in the quest
                foreach (var objective in quest.Objectives)
                {
                    // Only process "kill" type objectives
                    if (!objective.Type.Equals("kill", StringComparison.OrdinalIgnoreCase))
                        continue;

                    // Check if this objective is already complete
                    int currentProgress = questProgress.GetObjectiveProgress(objective.Id);
                    if (currentProgress >= objective.Count)
                        continue; // Already complete

                    // Check if the killed entity matches any accepted targets
                    if (objective.AcceptedTargets == null || objective.AcceptedTargets.Count == 0)
                        continue;

                    bool matches = false;
                    foreach (var targetPattern in objective.AcceptedTargets)
                    {
                        // Support wildcard matching (e.g., "game:drifter-*")
                        if (targetPattern.EndsWith("*"))
                        {
                            var prefix = targetPattern.Substring(0, targetPattern.Length - 1);
                            if (entityCode.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                            {
                                matches = true;
                                break;
                            }
                        }
                        else if (entityCode.Equals(targetPattern, StringComparison.OrdinalIgnoreCase))
                        {
                            matches = true;
                            break;
                        }
                    }

                    if (matches)
                    {
                        // Increment progress by 1
                        int added = questProgress.AddObjectiveProgress(objective.Id, 1, objective.Count);
                        if (added > 0)
                        {
                            progressUpdated = true;
                            serverApi?.Logger.Debug($"[QuestTracker] Player {killerServerPlayer.PlayerName} killed {entityCode}, quest {quest.Title} objective {objective.Id} progress: {currentProgress + 1}/{objective.Count}");
                        }
                    }
                }

                // Update quest progress in database if any objectives were updated
                if (progressUpdated)
                {
                    questRepository.UpdatePlayerQuestProgress(questProgress);
                }
            }
        }

        /// <summary>
        /// Helper method to get the guild that owns a specific chunk
        /// </summary>
        private Guild? GetChunkOwningGuild(int chunkX, int chunkZ)
        {
            if (landClaimRepository == null || guildManager == null) return null;

            var owningGuildName = landClaimRepository.GetGuildOwningChunk(chunkX, chunkZ);
            if (owningGuildName == null) return null;

            // Get the guild object
            return guildManager.GetGuild(owningGuildName);
        }

        /// <summary>
        /// Check if a chunk is adjacent to any existing guild claims (for expansion validation).
        /// Returns false if the guild has no claims (first claim is always allowed).
        /// </summary>
        public bool IsChunkAdjacentToGuildClaims(string guildName, int chunkX, int chunkZ)
        {
            if (guildManager == null) return false;

            var guild = guildManager.GetGuild(guildName);
            if (guild == null || guild.Claims.Count == 0) return false;

            // Check if the target chunk is adjacent to any existing guild claim
            foreach (var claim in guild.Claims)
            {
                // Check if chunks are adjacent (share a side, not diagonal)
                int deltaX = Math.Abs(claim.ChunkX - chunkX);
                int deltaZ = Math.Abs(claim.ChunkZ - chunkZ);

                // Adjacent means one coordinate is the same and the other differs by 1
                if ((deltaX == 1 && deltaZ == 0) || (deltaX == 0 && deltaZ == 1))
                {
                    return true;
                }
            }

            return false;
        }

        // Local permission check (mirrors GuildManager.HasPermission behavior)
        private bool HasPermissionLocal(Guild guild, string playerUid, GuildPermission permission)
        {
            if (guild == null || string.IsNullOrEmpty(playerUid) || !guild.Members.ContainsKey(playerUid)) return false;
            var roleName = guild.Members[playerUid].Role;
            if (!guild.Roles.TryGetValue(roleName, out var role)) return false;
            return (role.Permissions & permission) == permission;
        }

        // Parse permission tokens like "invite,promote,remove,manageroles,breakblocks,placeblocks,interactblocks,kick" into GuildPermission
        private GuildPermission ParsePermissionString(string perms)
        {
            if (string.IsNullOrWhiteSpace(perms)) return GuildPermission.None;

            GuildPermission result = GuildPermission.None;
            var tokens = perms.Split(new[] { ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var t in tokens)
            {
                var tok = t.Trim().ToLowerInvariant();
                switch (tok)
                {
                    case "invite":
                        result |= GuildPermission.Invite;
                        break;
                    case "promote":
                        result |= GuildPermission.Promote;
                        break;
                    case "kick":
                        result |= GuildPermission.Kick;
                        break;
                    case "manageroles":
                    case "managerole":
                    case "manage":
                        result |= GuildPermission.ManageRoles;
                        break;
                    case "breakplaceblocks":
                    case "breakplace":
                        result |= GuildPermission.BreakAndPlaceBlocks;
                        break;
                    case "interactblocks":
                    case "interact":
                        result |= GuildPermission.InteractBlocks;
                        break;
                }
            }
            return result;
        }

        public override void StartClientSide(ICoreClientAPI api)
        {
            clientApi = api;

            worldMapManager = api.ModLoader.GetModSystem<WorldMapManager>();
            worldMapManager.RegisterMapLayer<PlotMapLayer>("guildclaims", 1);

            // Initialize client-side networking with guild summaries callback
            networkHandler!.InitializeClient(api, OnNotificationReceived, OnGuildSummariesReceived);

            // Register config callback to update map layer
            networkHandler.RegisterConfigCallback(OnGuildConfigReceived);

            // Register tech blocks config sync callback
            networkHandler.RegisterTechBlocksConfigCallback(OnTechBlocksConfigReceived);

            // Initialize quest network handler client-side (register channel)
            questNetworkHandler?.InitializeClient(api);

            // Subscribe to quest manager open event
            if (questNetworkHandler != null)
            {
                questNetworkHandler.OnOpenQuestManager += OnOpenQuestManagerDialog;
            }

            // Initialize event network handler client-side
            eventClientNetworkHandler = new EventClientNetworkHandler();
            eventClientNetworkHandler.InitializeClient(api);

            if (eventClientNetworkHandler != null)
            {
                eventClientNetworkHandler.OnOpenEventManager = OnOpenEventManagerDialog;
            }

            // Initialize party network handler client-side with callbacks
            partyInvitePopup = new DialogPartyInvitePopup(api, this);
            partyHud = new PartyHud(api);
            partyNetworkHandler!.InitializeClient(
                api,
                OnPartyInviteReceived,
                OnPartyDataReceived,
                OnPartyOperationResult
            );

            // Register hotkey to open guild dialog
            api.Input.RegisterHotKey("openguild", "Open Guild Dialog", GlKeys.L, HotkeyType.GUIOrOtherControls, ctrlPressed: true);
            api.Input.SetHotKeyHandler("openguild", OnOpenGuildDialog);

            // Register hotkey to open party create dialog
            api.Input.RegisterHotKey("openpartycreate", "Open Party Create Dialog", GlKeys.P, HotkeyType.GUIOrOtherControls, ctrlPressed: true, shiftPressed: true);
            api.Input.SetHotKeyHandler("openpartycreate", OnOpenPartyCreateDialog);

            // Register keybind for toggling hologram
            api.Input.RegisterHotKey("toggleclaimhologram", "Toggle Guild Claim Hologram", GlKeys.P, HotkeyType.GUIOrOtherControls);
            api.Input.SetHotKeyHandler("toggleclaimhologram", OnToggleHologram);

            // Initialize invite popup dialog
            invitePopup = new DialogGuildInvitePopup(api, this);
        }

        private bool OnOpenGuildDialog(KeyCombination comb)
        {
            if (clientApi == null) return false;

            // Toggle dialog on hotkey
            if (guildDialog != null && guildDialog.IsOpened())
            {
                guildDialog.TryClose();
                return true;
            }

            // Try to apply pending config before opening dialog
            TryApplyPendingConfig();

            guildDialog = new DialogGuildMain(clientApi, this);
            guildDialog.TryOpen();
            return true;
        }

        /// <summary>
        /// Called when the client receives a party invite notification
        /// </summary>
        private void OnPartyInviteReceived(PartyInviteNotificationPacket packet)
        {
            clientApi?.Logger.Notification($"[Party] Received invite to '{packet.PartyName}' from {packet.InviterName}");
            partyInvitePopup?.ShowInvite(packet);
        }

        /// <summary>
        /// Called when the client receives updated party data
        /// </summary>
        private void OnPartyDataReceived(Party? party)
        {
            currentParty = party;

            if (party != null)
            {
                clientApi?.Logger.Notification($"[Party] Received party data: {party.Name} with {party.Members.Count} members");
                partyHud?.UpdateParty(party);
                partyManagerDialog?.UpdateParty(party);
            }
            else
            {
                clientApi?.Logger.Notification("[Party] Left party or party disbanded");
                partyHud?.UpdateParty(null);
                partyManagerDialog?.UpdateParty(null);
            }
        }

        /// <summary>
        /// Called when a party operation completes (success or failure message)
        /// </summary>
        private void OnPartyOperationResult(string message)
        {
            clientApi?.ShowChatMessage(message);
        }

        /// <summary>
        /// Opens the party create or manager dialog based on whether player is in a party
        /// </summary>
        private bool OnOpenPartyCreateDialog(KeyCombination comb)
        {
            if (clientApi == null) return false;

            // If player is in a party, show party manager
            if (currentParty != null)
            {
                // Toggle party manager dialog
                if (partyManagerDialog != null && partyManagerDialog.IsOpened())
                {
                    partyManagerDialog.TryClose();
                    return true;
                }

                partyManagerDialog = new DialogPartyManager(clientApi, this);
                partyManagerDialog.UpdateParty(currentParty);
                partyManagerDialog.TryOpen();
                return true;
            }

            // Otherwise show party create dialog
            if (partyCreateDialog != null && partyCreateDialog.IsOpened())
            {
                partyCreateDialog.TryClose();
                return true;
            }

            partyCreateDialog = new DialogPartyCreate(clientApi, this);
            partyCreateDialog.TryOpen();
            return true;
        }
        /// <summary>
        /// Called when the server sends a packet to open the quest manager dialog
        /// </summary>
        private void OnOpenQuestManagerDialog()
        {
            if (clientApi == null) return;

            // Close existing dialog if open
            if (questManagerDialog != null && questManagerDialog.IsOpened())
            {
                questManagerDialog.TryClose();
            }

            // Create and open new dialog
            questManagerDialog = new DialogQuestManager(clientApi, this);
            questManagerDialog.TryOpen();
        }

        private void OnOpenEventManagerDialog()
        {
            if (clientApi == null || eventClientNetworkHandler == null) return;

            if (eventManagerDialog != null)
            {
                eventManagerDialog?.TryClose();
                eventManagerDialog?.Dispose();

                eventClientNetworkHandler.OnEventListReceived = null;
            }

            eventManagerDialog = new DialogEventManager(clientApi, this);
            eventManagerDialog.TryOpen();
        }

        private bool OnToggleHologram(KeyCombination keyCombination)
        {
            ToggleHologram();
            return true;
        }

        public void ToggleHologram()
        {
            hologramVisible = !hologramVisible;

            if (hologramVisible)
            {
                ShowClaimsHologram();
                clientApi?.ShowChatMessage(Lang.Get("srguildsandkingdoms:hologram-shown"));
            }
            else
            {
                ClearHologram();
                clientApi?.ShowChatMessage(Lang.Get("srguildsandkingdoms:hologram-hidden"));
            }
        }

        public void ShowClaimsHologram()
        {
            if (clientApi == null) return;
            var currentGuild = GetCurrentPlayerGuildSummary();
            if (currentGuild == null || currentGuild.Claims.Count == 0)
            {
                // Ensure hologram is hidden if there's nothing to show
                if (hologramVisible)
                {
                    hologramVisible = false;
                    clientApi.ShowChatMessage(Lang.Get("srguildsandkingdoms:hologram-no-claims"));
                }
                return;
            }

            var blockPositions = new List<BlockPos>();
            var player = clientApi.World.Player;
            if (player == null) return;

            int playerY = (int)player.Entity.Pos.Y;
            int minY = Math.Max(0, playerY - 50);
            int maxY = Math.Min(clientApi.World.BlockAccessor.MapSizeY - 1, playerY + 50);

            var claimedChunks = new HashSet<(int, int)>(currentGuild.Claims.Select(c => (c.ChunkX, c.ChunkZ)));

            foreach (var claim in currentGuild.Claims)
            {
                int chunkX = claim.ChunkX;
                int chunkZ = claim.ChunkZ;

                int minBlockX = chunkX * 32;
                int maxBlockX = minBlockX + 31;
                int minBlockZ = chunkZ * 32;
                int maxBlockZ = minBlockZ + 31;

                // Check North border
                if (!claimedChunks.Contains((chunkX, chunkZ + 1)))
                {
                    for (int x = minBlockX; x <= maxBlockX; x++)
                    {
                        for (int y = minY; y <= maxY; y++) blockPositions.Add(new BlockPos(x, y, maxBlockZ));
                    }
                }

                // Check South border
                if (!claimedChunks.Contains((chunkX, chunkZ - 1)))
                {
                    for (int x = minBlockX; x <= maxBlockX; x++)
                    {
                        for (int y = minY; y <= maxY; y++) blockPositions.Add(new BlockPos(x, y, minBlockZ));
                    }
                }

                // Check East border
                if (!claimedChunks.Contains((chunkX + 1, chunkZ)))
                {
                    for (int z = minBlockZ; z <= maxBlockZ; z++)
                    {
                        for (int y = minY; y <= maxY; y++) blockPositions.Add(new BlockPos(maxBlockX, y, z));
                    }
                }

                // Check West border
                if (!claimedChunks.Contains((chunkX - 1, chunkZ)))
                {
                    for (int z = minBlockZ; z <= maxBlockZ; z++)
                    {
                        for (int y = minY; y <= maxY; y++) blockPositions.Add(new BlockPos(minBlockX, y, z));
                    }
                }
            }

            clientApi.World.HighlightBlocks(player, HOLOGRAM_SLOT, blockPositions, EnumHighlightBlocksMode.Absolute, EnumHighlightShape.Arbitrary);
        }

        public void ClearHologram()
        {
            clientApi?.World.HighlightBlocks(clientApi.World.Player, HOLOGRAM_SLOT, new List<BlockPos>());
            hologramVisible = false; // Ensure state is consistent
        }

        private void OnNotificationReceived(string message, NotificationType type)
        {
            // Check for territorial restriction information in error messages
            if (type == NotificationType.Error && message.Contains("Claims are restricted to within"))
            {
                TryExtractTerritorialSettingsFromMessage(message);
            }

            // Display notification to player
            string prefix = type switch
            {
                NotificationType.Success => "[Guild]",
                NotificationType.Warning => "[Guild]",
                NotificationType.Error => "[Guild]",
                _ => "[Guild]"
            };

            clientApi!.ShowChatMessage(prefix + message);
        }

        /// <summary>
        /// Extract territorial restriction settings from server error messages
        /// This is a temporary solution until proper config sync is implemented
        /// </summary>
        private void TryExtractTerritorialSettingsFromMessage(string message)
        {
            try
            {
                // Pattern: "Claims are restricted to within {radius} blocks of ({x}, {z})"
                var pattern = @"Claims are restricted to within (\d+) blocks of \((-?\d+), (-?\d+)\)";
                var match = System.Text.RegularExpressions.Regex.Match(message, pattern);

                if (match.Success)
                {
                    int radius = int.Parse(match.Groups[1].Value);
                    int centerX = int.Parse(match.Groups[2].Value);
                    int centerZ = int.Parse(match.Groups[3].Value);



                    Mod.Logger.Debug($"Extracted territorial settings from server message: Center ({centerX}, {centerZ}), Radius {radius}");
                }
            }
            catch (Exception ex)
            {
                Mod.Logger.Warning($"Failed to extract territorial settings from message: {ex.Message}");
            }
        }

        /// <summary>
        /// Callback invoked when guild summaries are received from the server
        /// </summary>
        private void OnGuildSummariesReceived(List<GuildSummary> summaries)
        {
            // Update the client-side cache
            clientGuildSummaries.Clear();
            clientGuildSummaries.AddRange(summaries);

            // Log the update for debugging
            if (clientApi != null)
            {
                var playerGuild = summaries.FirstOrDefault(g => g.IsPlayerMember);
                if (playerGuild != null)
                {
                    Mod.Logger.Notification($"[SRGuildsAndKingdoms:GuildSync] Received guild summaries - Player is member of: {playerGuild.Name}");
                }
                else
                {
                    Mod.Logger.Debug($"[SRGuildsAndKingdoms:GuildSync] Received {summaries.Count} guild summaries - Player is not in any guild");
                }
            }

            // Optional: Trigger any other systems that need to be notified of guild data changes
            OnClientGuildDataUpdated?.Invoke(summaries);
        }


        /// <summary>
        /// Callback invoked when guild config is received from the server
        /// </summary>
        private void OnGuildConfigReceived(GuildConfigPacket config)
        {
            // Update client's TechBlocksConfig with server's enabled ages
            if (techBlocksConfig != null && config.EnabledAges != null)
            {
                techBlocksConfig.EnabledAges = config.EnabledAges
                    .Select(age => (TechAge)age).ToList();
                Mod.Logger.Notification($"Synced {config.EnabledAges.Count} enabled tech ages from server: {string.Join(", ", techBlocksConfig.EnabledAges)}");
            }

            if (plotLayer != null)
            {
                // Apply config immediately if layer is available
                plotLayer.UpdateConfigFromServer(config);
                pendingConfigPacket = null; // Clear any pending config
                Mod.Logger.Debug($"Successfully updated PlotMapLayer with config: Territorial={config.TerritorialRestrictionsEnabled}, Protected Zones={config.ProtectedZonesEnabled} ({config.ProtectedZones?.Count ?? 0} zones)");
            }
            else
            {
                // Cache the config to apply later when layer becomes available
                pendingConfigPacket = config;
                Mod.Logger.Warning($"PlotMapLayer not registered yet - caching config for later application.");
            }
        }

        /// <summary>
        /// Callback invoked when tech blocks config is received from the server
        /// Saves the server's config to a server-specific folder and reloads it
        /// </summary>
        private void OnTechBlocksConfigReceived(TechBlocksConfigSyncPacket packet)
        {
            if (clientApi == null)
            {
                Mod.Logger.Warning("OnTechBlocksConfigReceived called but clientApi is null");
                return;
            }

            try
            {
                // Save the server's config to a server-specific folder
                TechBlocksConfig.SaveServerConfig(clientApi, packet.ConfigJson, packet.ServerIdentifier);

                // Reload the config from the server-specific folder
                techBlocksConfig = TechBlocksConfig.LoadFromFile(clientApi, "techblocks.json", packet.ServerIdentifier);

                Mod.Logger.Notification($"Tech blocks config synced from server (identifier: {packet.ServerIdentifier})");
                Mod.Logger.Notification($"Loaded {techBlocksConfig.TechBlocks.Count} tech blocks, {techBlocksConfig.EnabledAges.Count} enabled ages");

                // Re-apply age restrictions to blocks with the new config
                ApplyAgeRestrictionsToBlocks(clientApi);
            }
            catch (Exception ex)
            {
                Mod.Logger.Error($"Failed to process tech blocks config from server: {ex.Message}");
                Mod.Logger.Error($"Stack trace: {ex.StackTrace}");
            }
        }

        /// <summary>
        /// Called by PlotMapLayer when it's constructed to register itself
        /// </summary>
        public void RegisterPlotMapLayer(PlotMapLayer layer)
        {
            plotLayer = layer;
            Mod.Logger.Debug("PlotMapLayer registered with mod system");

            // Apply any pending config immediately
            TryApplyPendingConfig();
        }

        /// <summary>
        /// Try to apply pending config to the plot layer (called periodically or when opening map)
        /// </summary>
        private void TryApplyPendingConfig()
        {
            if (pendingConfigPacket == null) return;

            if (plotLayer != null)
            {
                plotLayer.UpdateConfigFromServer(pendingConfigPacket);
                Mod.Logger.Debug($"Applied pending config to PlotMapLayer: Protected Zones={pendingConfigPacket.ProtectedZonesEnabled} ({pendingConfigPacket.ProtectedZones?.Count ?? 0} zones)");
                pendingConfigPacket = null;
            }
        }


        /// <summary>
        /// Event fired when client-side guild data is updated
        /// </summary>
        public event Action<List<GuildSummary>>? OnClientGuildDataUpdated;

        /// <summary>
        /// Expanded method to get guild summaries with additional functionality
        /// </summary>
        public List<GuildSummary> GetClientGuildSummaries()
        {
            return new List<GuildSummary>(clientGuildSummaries); // Return a copy to prevent external modification
        }

        /// <summary>
        /// Get guild summaries filtered by specific criteria
        /// </summary>
        public List<GuildSummary> GetClientGuildSummaries(System.Func<GuildSummary, bool>? filter = null)
        {
            var result = new List<GuildSummary>(clientGuildSummaries);

            if (filter != null)
            {
                result = result.Where(filter).ToList();
            }

            return result;
        }

        /// <summary>
        /// Get guild summary for a specific guild by name
        /// </summary>
        public GuildSummary? GetGuildSummary(String guildName)
        {
            return clientGuildSummaries.FirstOrDefault(g =>
                g.Name.Equals(guildName, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Get the guild summary for the current player's guild, if any
        /// </summary>
        public GuildSummary? GetCurrentPlayerGuildSummary()
        {
            return clientGuildSummaries.FirstOrDefault(g => g.IsPlayerMember);
        }

        /// <summary>
        /// Get guild summaries with claims in a specific chunk area
        /// </summary>
        public List<GuildSummary> GetGuildSummariesWithClaimsInArea(int centerChunkX, int centerChunkZ, int radius)
        {
            return clientGuildSummaries.Where(guild =>
                guild.Claims.Any(claim =>
                    Math.Abs(claim.ChunkX - centerChunkX) <= radius &&
                    Math.Abs(claim.ChunkZ - centerChunkZ) <= radius
                )
            ).ToList();
        }

        /// <summary>
        /// Get all claimed chunks as a dictionary for fast lookup
        /// </summary>
        public Dictionary<(int chunkX, int chunkZ), GuildSummary> GetClaimedChunksLookup()
        {
            var lookup = new Dictionary<(int chunkX, int chunkZ), GuildSummary>();

            foreach (var guild in clientGuildSummaries)
            {
                foreach (var claim in guild.Claims)
                {
                    lookup[(claim.ChunkX, claim.ChunkZ)] = guild;
                }
            }

            return lookup;
        }

        /// <summary>
        /// Check if a specific chunk is claimed by any guild
        /// </summary>
        public bool IsChunkClaimed(int chunkX, int chunkZ)
        {
            return clientGuildSummaries.Any(guild =>
                guild.Claims.Any(claim => claim.ChunkX == chunkX && claim.ChunkZ == chunkZ)
            );
        }

        /// <summary>
        /// Get the guild that owns a specific chunk, if any
        /// </summary>
        public GuildSummary? GetChunkOwner(int chunkX, int chunkZ)
        {
            return clientGuildSummaries.FirstOrDefault(guild =>
                guild.Claims.Any(claim => claim.ChunkX == chunkX && claim.ChunkZ == chunkZ)
            );
        }

        /// <summary>
        /// Request updated guild summaries from the server
        /// </summary>
        public void RequestGuildSummariesUpdate()
        {
            if (clientApi != null && networkHandler != null)
            {
                // Send a request packet to the server for updated guild data
                // This would require implementing a new packet type
                Mod.Logger.Debug("Guild summaries update requested from server");
            }
        }

        // Expose network handler for other components that might need it
        public GuildNetworkHandler? GetNetworkHandler()
        {
            return networkHandler;
        }

        // Expose plot layer for other components that might need it
        public PlotMapLayer? GetPlotLayer()
        {
            PlotMapLayer? mapLayer = worldMapManager?.MapLayers.FirstOrDefault(layer => layer is PlotMapLayer) as PlotMapLayer;

            // If layer is now available and we have a pending config, apply it
            if (mapLayer != null)
            {
                TryApplyPendingConfig();
            }

            return mapLayer;
        }

        // Expose guild manager for configuration access (server-side only)
        public GuildManager? GetGuildManager()
        {
            return guildManager;
        }

        public bool CheckGuildUsePrivilege(IServerPlayer player, BlockPos pos)
        {
            if (player == null || pos == null) return false;

            // Allow creative mode players to bypass all restrictions
            if (player.WorldData.CurrentGameMode == EnumGameMode.Creative)
            {
                return true;
            }

            // Check protected zones first (they take precedence over guild claims)
            var config = guildManager?.GetConfigManager()?.GetConfig();
            var spawnPos = serverApi?.World.DefaultSpawnPosition.AsBlockPos;

            if (config != null && spawnPos != null && config.IsWithinProtectedZone(pos.X, pos.Z, spawnPos))
            {
                var zone = config.GetProtectedZoneAt(pos.X, pos.Z, spawnPos);

                // Check if player is whitelisted for this zone
                if (zone != null && zoneWhitelistManager?.IsPlayerWhitelisted(zone.Id, player.PlayerUID) == true)
                {
                    return true; // Allow whitelisted players full access
                }

                // Allow "Use" actions for everyone in protected zones (opening chests, using doors, etc.)
                // This is checked in the patch, so we return true here to allow the action
                return true;
            }

            // Get the chunk coordinates
            int chunkX = GuildLandClaim.FloorDiv(pos.X, GuildLandClaim.ChunkSize);
            int chunkZ = GuildLandClaim.FloorDiv(pos.Z, GuildLandClaim.ChunkSize);

            // Check if the chunk is claimed by any guild
            var owningGuild = GetChunkOwningGuild(chunkX, chunkZ);
            if (owningGuild == null) return false; // Not guild claimed, don't override

            // Check if player is a member of the owning guild
            var playerGuild = guildManager!.GetGuildByMember(player.PlayerUID);
            if (playerGuild == null || playerGuild.Name != owningGuild.Name) return false;

            // Player is in the guild - check if they have interact permission
            return GuildManager.HasPermission(owningGuild, player.PlayerUID, GuildPermission.InteractBlocks);
        }

        public bool CheckGuildBuildPrivilege(IServerPlayer player, BlockPos pos)
        {
            if (player == null || pos == null) return false;

            // Allow creative mode players to bypass all restrictions
            if (player.WorldData.CurrentGameMode == EnumGameMode.Creative)
            {
                return true;
            }

            // Check protected zones first (they take precedence over guild claims)
            var config = guildManager?.GetConfigManager()?.GetConfig();
            var spawnPos = serverApi?.World.DefaultSpawnPosition.AsBlockPos;

            if (config != null && spawnPos != null && config.IsWithinProtectedZone(pos.X, pos.Z, spawnPos))
            {
                var zone = config.GetProtectedZoneAt(pos.X, pos.Z, spawnPos);

                // Check if player is whitelisted for this zone
                if (zone != null && zoneWhitelistManager?.IsPlayerWhitelisted(zone.Id, player.PlayerUID) == true)
                {
                    return true; // Allow whitelisted players full access
                }

                // Block "BuildOrBreak" actions for non-whitelisted players in protected zones
                return false;
            }

            // Get the chunk coordinates
            int chunkX = GuildLandClaim.FloorDiv(pos.X, GuildLandClaim.ChunkSize);
            int chunkZ = GuildLandClaim.FloorDiv(pos.Z, GuildLandClaim.ChunkSize);

            // Check if the chunk is claimed by any guild
            var owningGuild = GetChunkOwningGuild(chunkX, chunkZ);
            if (owningGuild == null) return false; // Not guild claimed, don't override

            // Check if player is a member of the owning guild
            var playerGuild = guildManager!.GetGuildByMember(player.PlayerUID);
            if (playerGuild == null || playerGuild.Name != owningGuild.Name) return false;

            // Player is in the guild - check if they have build permission
            return GuildManager.HasPermission(owningGuild, player.PlayerUID, GuildPermission.BreakAndPlaceBlocks);
        }

        /// <summary>
        /// Show the invite popup for a single invite
        /// </summary>
        public void ShowInvitePopup(GuildInviteNotificationPacket invite)
        {
            if (clientApi == null || invitePopup == null) return;

            invitePopup.ShowInvite(invite);
        }

        /// <summary>
        /// Show the invite popup with multiple invites
        /// </summary>
        public void ShowInviteListPopup(List<GuildInviteInfo> invites)
        {
            if (clientApi == null || invitePopup == null) return;

            invitePopup.ShowInvites(invites);
        }

        /// <summary>
        /// Register PVP mod as the node war data provider (called by PVP mod on startup)
        /// This establishes the connection between the two mods for node war data requests
        /// </summary>
        public void RegisterNodeWarDataProvider(object pvpModSystem)
        {
            // Store reference if needed, or just acknowledge the registration
            serverApi?.Logger.Notification("[Guild] PVP mod registered as node war data provider");
        }
    }
}
