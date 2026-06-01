using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using HarmonyLib;
using Newtonsoft.Json.Linq;
using SRGuildsAndKingdoms.src.config;
using SRGuildsAndKingdoms.src.database;
using SRGuildsAndKingdoms.src.gui;
using SRGuildsAndKingdoms.src.guilds;
using SRGuildsAndKingdoms.src.guilds.behaviors;
using SRGuildsAndKingdoms.src.network;
using SRGuildsAndKingdoms.src.patches;
using SRGuildsAndKingdoms.src.quests;
using SRGuildsAndKingdoms.src.quests.blocks;
using SRGuildsAndKingdoms.src.techblock;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.Common;
using Vintagestory.GameContent;

namespace SRGuildsAndKingdoms
{
	// Token: 0x02000005 RID: 5
	[NullableContext(1)]
	[Nullable(0)]
	public class SRGuildsAndKingdomsModSystem : ModSystem
	{
		// Token: 0x17000006 RID: 6
		// (get) Token: 0x06000012 RID: 18 RVA: 0x00002336 File Offset: 0x00000536
		public bool IsHologramVisible
		{
			get
			{
				return this.hologramVisible;
			}
		}

		// Token: 0x17000007 RID: 7
		// (get) Token: 0x06000013 RID: 19 RVA: 0x0000233E File Offset: 0x0000053E
		public List<TechBlock> TechBlocks
		{
			get
			{
				TechBlocksConfig techBlocksConfig = this.techBlocksConfig;
				return ((techBlocksConfig != null) ? techBlocksConfig.TechBlocks : null) ?? new List<TechBlock>();
			}
		}

		// Token: 0x17000008 RID: 8
		// (get) Token: 0x06000014 RID: 20 RVA: 0x0000235B File Offset: 0x0000055B
		[Nullable(2)]
		public TechBlocksConfig TechBlocksConfig
		{
			[NullableContext(2)]
			get
			{
				return this.techBlocksConfig;
			}
		}

		// Token: 0x17000009 RID: 9
		// (get) Token: 0x06000015 RID: 21 RVA: 0x00002363 File Offset: 0x00000563
		[Nullable(2)]
		public GuildTechManager GuildTechManager
		{
			[NullableContext(2)]
			get
			{
				return this.guildTechManager;
			}
		}

		// Token: 0x1700000A RID: 10
		// (get) Token: 0x06000016 RID: 22 RVA: 0x0000236B File Offset: 0x0000056B
		[Nullable(2)]
		public GuildManager GuildManager
		{
			[NullableContext(2)]
			get
			{
				return this.guildManager;
			}
		}

		// Token: 0x06000017 RID: 23 RVA: 0x00002373 File Offset: 0x00000573
		[NullableContext(2)]
		public GuildRepository GetGuildRepository()
		{
			return this.guildRepository;
		}

		// Token: 0x1700000B RID: 11
		// (get) Token: 0x06000018 RID: 24 RVA: 0x0000237B File Offset: 0x0000057B
		[Nullable(2)]
		public GuildNetworkHandler NetworkHandler
		{
			[NullableContext(2)]
			get
			{
				return this.networkHandler;
			}
		}

		// Token: 0x1700000C RID: 12
		// (get) Token: 0x06000019 RID: 25 RVA: 0x00002383 File Offset: 0x00000583
		[Nullable(2)]
		public QuestNetworkHandler QuestNetworkHandler
		{
			[NullableContext(2)]
			get
			{
				return this.questNetworkHandler;
			}
		}

		// Token: 0x0600001A RID: 26 RVA: 0x0000238B File Offset: 0x0000058B
		[NullableContext(2)]
		public QuestRepository GetQuestRepository()
		{
			return this.questRepository;
		}

		// Token: 0x0600001B RID: 27 RVA: 0x00002393 File Offset: 0x00000593
		[NullableContext(2)]
		public ZoneWhitelistManager GetZoneWhitelistManager()
		{
			return this.zoneWhitelistManager;
		}

		// Token: 0x0600001C RID: 28 RVA: 0x0000239B File Offset: 0x0000059B
		[NullableContext(2)]
		public NodeManager GetNodeManager()
		{
			return this.nodeManager;
		}

		// Token: 0x0600001D RID: 29 RVA: 0x000023A3 File Offset: 0x000005A3
		[NullableContext(2)]
		public LandClaimRepository GetLandClaimRepository()
		{
			return this.landClaimRepository;
		}

		// Token: 0x0600001E RID: 30 RVA: 0x000023AC File Offset: 0x000005AC
		[return: TupleElementNames(new string[]
		{
			"isProtected",
			"zoneName",
			"whitelistedPlayers"
		})]
		[return: Nullable(new byte[]
		{
			0,
			1,
			1,
			1
		})]
		public ValueTuple<bool, string, List<string>>? CheckProtectedZone(int x, int z)
		{
			if (this.serverApi != null)
			{
				GuildManager guildManager = this.guildManager;
				GuildConfig guildConfig;
				if (guildManager == null)
				{
					guildConfig = null;
				}
				else
				{
					GuildConfigManager configManager = guildManager.GetConfigManager();
					guildConfig = ((configManager != null) ? configManager.GetConfig() : null);
				}
				GuildConfig config = guildConfig;
				BlockPos spawnPos = this.serverApi.World.DefaultSpawnPosition.AsBlockPos;
				if (config != null && spawnPos != null && config.IsWithinProtectedZone(x, z, spawnPos))
				{
					ProtectedZone zone = config.GetProtectedZoneAt(x, z, spawnPos);
					if (zone != null)
					{
						return new ValueTuple<bool, string, List<string>>?(new ValueTuple<bool, string, List<string>>(true, zone.Name, new List<string>()));
					}
				}
			}
			else if (this.clientApi != null)
			{
				PlotMapLayer plotLayer = this.GetPlotLayer();
				if (plotLayer != null)
				{
					ValueTuple<string, int, int, int, List<string>>? zoneInfo = plotLayer.GetProtectedZoneAt(x, z);
					if (zoneInfo != null)
					{
						return new ValueTuple<bool, string, List<string>>?(new ValueTuple<bool, string, List<string>>(true, zoneInfo.Value.Item1, zoneInfo.Value.Item5));
					}
				}
			}
			return null;
		}

		// Token: 0x0600001F RID: 31 RVA: 0x00002488 File Offset: 0x00000688
		[return: Nullable(2)]
		public GuildSummary GetCachedGuildSummary(string guildName)
		{
			return this.clientGuildSummaries.FirstOrDefault((GuildSummary g) => g.Name == guildName);
		}

		// Token: 0x06000020 RID: 32 RVA: 0x000024B9 File Offset: 0x000006B9
		public List<GuildSummary> GetCachedGuildSummaries()
		{
			return new List<GuildSummary>(this.clientGuildSummaries);
		}

		// Token: 0x06000021 RID: 33 RVA: 0x000024C8 File Offset: 0x000006C8
		public override void Start(ICoreAPI api)
		{
			base.Mod.Logger.Notification("Hello from template mod: " + api.Side.ToString());
			this.networkHandler = new GuildNetworkHandler();
			this.questNetworkHandler = new QuestNetworkHandler();
			try
			{
				this.techBlocksConfig = TechBlocksConfig.LoadFromFile(api, "techblocks.json", null);
				ILogger logger = base.Mod.Logger;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(38, 1);
				defaultInterpolatedStringHandler.AppendLiteral("Loaded ");
				defaultInterpolatedStringHandler.AppendFormatted<int>(this.TechBlocks.Count);
				defaultInterpolatedStringHandler.AppendLiteral(" tech blocks from configuration");
				logger.Notification(defaultInterpolatedStringHandler.ToStringAndClear());
			}
			catch (Exception ex)
			{
				base.Mod.Logger.Error("Failed to load tech blocks configuration: " + ex.Message);
				this.techBlocksConfig = new TechBlocksConfig();
			}
			api.RegisterBlockBehaviorClass("AgeRestricted", typeof(BlockBehaviorAgeRestricted));
			if (api.Side == 1)
			{
				(api as ICoreServerAPI).Event.SaveGameLoaded += delegate()
				{
					this.ApplyAgeRestrictionsToBlocks(api);
					this.ApplyProtectionToBlocks(api);
				};
			}
			else
			{
				(api as ICoreClientAPI).Event.LevelFinalize += delegate()
				{
					this.ApplyAgeRestrictionsToBlocks(api);
					this.ApplyProtectionToBlocks(api);
				};
			}
			this.harmony = new Harmony("srguildsandkingdoms.patches");
			try
			{
				MethodInfo worldMapTestBlockAccessMethod = typeof(WorldMap).GetMethod("TestBlockAccess", BindingFlags.Instance | BindingFlags.Public, null, new Type[]
				{
					typeof(IPlayer),
					typeof(BlockSelection),
					typeof(EnumBlockAccessFlags),
					typeof(string).MakeByRefType()
				}, null);
				if (worldMapTestBlockAccessMethod != null)
				{
					MethodInfo prefixMethod = typeof(WorldMapTestBlockAccessPatch).GetMethod("Prefix");
					this.harmony.Patch(worldMapTestBlockAccessMethod, new HarmonyMethod(prefixMethod), null, null, null);
					base.Mod.Logger.Notification("Successfully patched WorldMap.TestBlockAccess");
				}
				else
				{
					base.Mod.Logger.Warning("Could not find WorldMap.TestBlockAccess method to patch");
				}
			}
			catch (Exception ex2)
			{
				base.Mod.Logger.Error("Failed to apply Harmony patches: " + ex2.Message);
				base.Mod.Logger.Error("Stack trace: " + ex2.StackTrace);
			}
			api.RegisterBlockBehaviorClass("GrsDoor", typeof(BlockBehaviorGrsDoor));
			api.RegisterBlockClass("QuestBoardBlock", typeof(QuestBoardBlock));
		}

		// Token: 0x06000022 RID: 34 RVA: 0x00002794 File Offset: 0x00000994
		private void ApplyAgeRestrictionsToBlocks(ICoreAPI api)
		{
			TechBlocksConfig techBlocksConfig = this.techBlocksConfig;
			if (((techBlocksConfig != null) ? techBlocksConfig.AgeRestrictedBlocks : null) == null)
			{
				base.Mod.Logger.Warning("No age-restricted blocks configuration found");
				return;
			}
			int restrictedCount = 0;
			foreach (Block block in api.World.Blocks)
			{
				if (block != null && !(block.Code == null))
				{
					string blockCode = block.Code.ToString();
					TechAge requiredAge;
					string requiredTrait;
					if (this.techBlocksConfig.AgeRestrictedBlocks.IsBlockRestricted(blockCode, out requiredAge, out requiredTrait))
					{
						string traitJson = string.IsNullOrWhiteSpace(requiredTrait) ? "null" : ("\"" + requiredTrait + "\"");
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(35, 2);
						defaultInterpolatedStringHandler.AppendLiteral("{\"requiredAge\":\"");
						defaultInterpolatedStringHandler.AppendFormatted<TechAge>(requiredAge);
						defaultInterpolatedStringHandler.AppendLiteral("\",\"requiredTrait\":");
						defaultInterpolatedStringHandler.AppendFormatted(traitJson);
						defaultInterpolatedStringHandler.AppendLiteral("}");
						JsonObject behaviorProperties = new JsonObject(JToken.Parse(defaultInterpolatedStringHandler.ToStringAndClear()));
						BlockBehaviorAgeRestricted behavior = new BlockBehaviorAgeRestricted(block);
						behavior.Initialize(behaviorProperties);
						behavior.OnLoaded(api);
						BlockBehavior[] blockBehaviors = block.BlockBehaviors;
						List<BlockBehavior> behaviorsList = ((blockBehaviors != null) ? blockBehaviors.ToList<BlockBehavior>() : null) ?? new List<BlockBehavior>();
						behaviorsList.Add(behavior);
						block.BlockBehaviors = behaviorsList.ToArray();
						restrictedCount++;
					}
				}
			}
			ILogger logger = base.Mod.Logger;
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(35, 1);
			defaultInterpolatedStringHandler2.AppendLiteral("Applied age restrictions to ");
			defaultInterpolatedStringHandler2.AppendFormatted<int>(restrictedCount);
			defaultInterpolatedStringHandler2.AppendLiteral(" blocks");
			logger.Notification(defaultInterpolatedStringHandler2.ToStringAndClear());
		}

		// Token: 0x06000023 RID: 35 RVA: 0x00002964 File Offset: 0x00000B64
		private void ApplyProtectionToBlocks(ICoreAPI api)
		{
			if (api.Side != 1)
			{
				return;
			}
			int protectedCount = 0;
			foreach (Block block in api.World.Blocks)
			{
				if (block != null && !(block.Code == null))
				{
					BlockBehavior[] blockBehaviors = block.BlockBehaviors;
					List<BlockBehavior> behaviorsList = ((blockBehaviors != null) ? blockBehaviors.ToList<BlockBehavior>() : null) ?? new List<BlockBehavior>();
					block.BlockBehaviors = behaviorsList.ToArray();
					protectedCount++;
				}
			}
			ILogger logger = base.Mod.Logger;
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(35, 1);
			defaultInterpolatedStringHandler.AppendLiteral("Applied guild protection to ");
			defaultInterpolatedStringHandler.AppendFormatted<int>(protectedCount);
			defaultInterpolatedStringHandler.AppendLiteral(" blocks");
			logger.Notification(defaultInterpolatedStringHandler.ToStringAndClear());
		}

		// Token: 0x06000024 RID: 36 RVA: 0x00002A3C File Offset: 0x00000C3C
		public override void Dispose()
		{
			Harmony harmony = this.harmony;
			if (harmony != null)
			{
				harmony.UnpatchAll("srguildsandkingdoms.patches");
			}
			base.Dispose();
		}

		// Token: 0x06000025 RID: 37 RVA: 0x00002A5C File Offset: 0x00000C5C
		public override void StartServerSide(ICoreServerAPI api)
		{
			this.serverApi = api;
			try
			{
				this.guildDatabase = new GuildDatabase(api);
				this.guildDatabase.Initialize();
				this.guildRepository = new GuildRepository(api, this.guildDatabase);
				this.questRepository = new QuestRepository(api, this.guildDatabase);
				this.cooldownRepository = new CooldownRepository(api, this.guildDatabase);
				this.zoneWhitelistRepository = new ZoneWhitelistRepository(api, this.guildDatabase);
				this.nodeRepository = new NodeRepository(api, this.guildDatabase);
				this.guildRepository.LoadAllGuilds();
				int count = this.guildRepository.GetAllGuilds().Count;
				MigrationManager migrationManager = new MigrationManager(api, this.guildDatabase);
				if (migrationManager.NeedsMigration())
				{
					api.Logger.Warning("[SRGuildsAndKingdoms:Migration] JSON guild data detected - running migration to SQLite...");
					MigrationResult result = migrationManager.MigrateFromJson();
					if (!result.Success)
					{
						api.Logger.Error("[SRGuildsAndKingdoms:Migration] Migration FAILED!");
						foreach (string error in result.Errors)
						{
							api.Logger.Error("  - " + error);
						}
						throw new InvalidOperationException("Guild migration failed. See log for details.");
					}
					api.Logger.Notification("[SRGuildsAndKingdoms:Migration] Migration completed successfully:");
					ILogger logger = api.Logger;
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(20, 1);
					defaultInterpolatedStringHandler.AppendLiteral("  - ");
					defaultInterpolatedStringHandler.AppendFormatted<int>(result.GuildsMigrated);
					defaultInterpolatedStringHandler.AppendLiteral(" guilds migrated");
					logger.Notification(defaultInterpolatedStringHandler.ToStringAndClear());
					ILogger logger2 = api.Logger;
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(23, 1);
					defaultInterpolatedStringHandler2.AppendLiteral("  - ");
					defaultInterpolatedStringHandler2.AppendFormatted<int>(result.CooldownsMigrated);
					defaultInterpolatedStringHandler2.AppendLiteral(" cooldowns migrated");
					logger2.Notification(defaultInterpolatedStringHandler2.ToStringAndClear());
					ILogger logger3 = api.Logger;
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler3 = new DefaultInterpolatedStringHandler(29, 1);
					defaultInterpolatedStringHandler3.AppendLiteral("  - ");
					defaultInterpolatedStringHandler3.AppendFormatted<int>(result.ZoneWhitelistsMigrated);
					defaultInterpolatedStringHandler3.AppendLiteral(" zone whitelists migrated");
					logger3.Notification(defaultInterpolatedStringHandler3.ToStringAndClear());
					if (result.Warnings.Count > 0)
					{
						ILogger logger4 = api.Logger;
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler4 = new DefaultInterpolatedStringHandler(56, 1);
						defaultInterpolatedStringHandler4.AppendLiteral("[SRGuildsAndKingdoms:Migration] Migration had ");
						defaultInterpolatedStringHandler4.AppendFormatted<int>(result.Warnings.Count);
						defaultInterpolatedStringHandler4.AppendLiteral(" warnings:");
						logger4.Warning(defaultInterpolatedStringHandler4.ToStringAndClear());
						foreach (string warning in result.Warnings)
						{
							api.Logger.Warning("  - " + warning);
						}
					}
					api.Logger.Notification("[SRGuildsAndKingdoms:Migration] Reloading guilds from database after migration...");
					this.guildRepository.LoadAllGuilds();
					int loadedCount = this.guildRepository.GetAllGuilds().Count;
					ILogger logger5 = api.Logger;
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler5 = new DefaultInterpolatedStringHandler(59, 1);
					defaultInterpolatedStringHandler5.AppendLiteral("[SRGuildsAndKingdoms:Migration] Loaded ");
					defaultInterpolatedStringHandler5.AppendFormatted<int>(loadedCount);
					defaultInterpolatedStringHandler5.AppendLiteral(" guild(s) into cache");
					logger5.Notification(defaultInterpolatedStringHandler5.ToStringAndClear());
				}
				api.Logger.Notification("[SRGuildsAndKingdoms] Database initialization complete");
			}
			catch (Exception ex)
			{
				api.Logger.Error("[SRGuildsAndKingdoms] Failed to initialize database: " + ex.Message);
				api.Logger.Error("Stack trace: " + ex.StackTrace);
				throw;
			}
			this.landClaimRepository = new LandClaimRepository(api, this.guildRepository);
			this.landClaimRepository.RebuildIndexes();
			ValueTuple<int, int> statistics = this.landClaimRepository.GetStatistics();
			int totalClaims = statistics.Item1;
			int guildsWithClaims = statistics.Item2;
			ILogger logger6 = api.Logger;
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler6 = new DefaultInterpolatedStringHandler(78, 2);
			defaultInterpolatedStringHandler6.AppendLiteral("[SRGuildsAndKingdoms] Land claim spatial indexes built: ");
			defaultInterpolatedStringHandler6.AppendFormatted<int>(totalClaims);
			defaultInterpolatedStringHandler6.AppendLiteral(" chunks across ");
			defaultInterpolatedStringHandler6.AppendFormatted<int>(guildsWithClaims);
			defaultInterpolatedStringHandler6.AppendLiteral(" guilds");
			logger6.Notification(defaultInterpolatedStringHandler6.ToStringAndClear());
			this.guildManager = new GuildManager(api, this.guildRepository, this.cooldownRepository, this.landClaimRepository);
			this.guildTechManager = new GuildTechManager(api, delegate(string guildId)
			{
				GuildManager guildManager = this.guildManager;
				if (guildManager == null)
				{
					return null;
				}
				return guildManager.GetGuild(guildId);
			}, delegate(string guildId)
			{
				GuildRepository guildRepository = this.guildRepository;
				if (guildRepository == null)
				{
					return;
				}
				guildRepository.MarkDirty(guildId);
			});
			this.zoneWhitelistManager = new ZoneWhitelistManager(api, this.zoneWhitelistRepository);
			this.nodeManager = new NodeManager(api, this.nodeRepository);
			this.networkHandler.InitializeServer(api, this.guildManager);
			this.questNetworkHandler = new QuestNetworkHandler();
			this.questNetworkHandler.InitializeServer(api, this.questRepository, this.guildRepository, this.guildManager);
			this.questNetworkHandler.OnGuildPointsAwarded = delegate(IServerPlayer player)
			{
				GuildNetworkHandler guildNetworkHandler = this.networkHandler;
				if (guildNetworkHandler == null)
				{
					return;
				}
				guildNetworkHandler.BroadcastGuildSummaries(player);
			};
			api.Event.SaveGameLoaded += this.OnSaveGameLoaded;
			api.Event.GameWorldSave += this.OnSaveGameSaving;
			this.ProcessExpiredRepeatingQuests();
			api.Event.OnEntityDeath += new EntityDeathDelegate(this.OnEntityDeath);
			api.Event.PlayerJoin += delegate(IServerPlayer player)
			{
				api.Event.RegisterCallback(delegate(float dt)
				{
					if (this.guildManager != null && player != null)
					{
						IServerPlayer serverPlayer = player;
						this.Mod.Logger.Notification("[SRGuildsAndKingdoms:GuildSync] Syncing traits and guild summaries for player " + serverPlayer.PlayerName + " on join");
						this.guildManager.SyncPlayerTraits(serverPlayer);
						GuildNetworkHandler guildNetworkHandler = this.networkHandler;
						if (guildNetworkHandler == null)
						{
							return;
						}
						guildNetworkHandler.BroadcastGuildSummaries(serverPlayer);
					}
				}, 1000);
			};
			api.Event.RegisterGameTickListener(delegate(float dt)
			{
				if (this.guildManager != null && this.serverApi != null)
				{
					IPlayer[] onlinePlayers = this.serverApi.World.AllOnlinePlayers;
					if (onlinePlayers != null && onlinePlayers.Length != 0)
					{
						ILogger logger7 = this.Mod.Logger;
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler7 = new DefaultInterpolatedStringHandler(79, 1);
						defaultInterpolatedStringHandler7.AppendLiteral("[SRGuildsAndKingdoms:TraitSync] Running periodic trait sync for ");
						defaultInterpolatedStringHandler7.AppendFormatted<int>(onlinePlayers.Length);
						defaultInterpolatedStringHandler7.AppendLiteral(" online players");
						logger7.Debug(defaultInterpolatedStringHandler7.ToStringAndClear());
						IPlayer[] array = onlinePlayers;
						for (int i = 0; i < array.Length; i++)
						{
							IServerPlayer serverPlayer = array[i] as IServerPlayer;
							if (serverPlayer != null)
							{
								this.guildManager.SyncPlayerTraits(serverPlayer);
							}
						}
					}
				}
			}, 600000, 0);
			api.Event.RegisterGameTickListener(delegate(float dt)
			{
				if (this.guildManager != null)
				{
					this.guildManager.CleanupExpiredInvitesPublic();
				}
			}, 60000, 0);
			api.ChatCommands.Create("guild").WithDescription("Guild management commands").WithArgs(new ICommandArgumentParser[]
			{
				api.ChatCommands.Parsers.Word("action")
			}).RequiresPrivilege(Privilege.chat).HandleWith(new OnCommandDelegate(this.OnGuildChatCommand));
			api.ChatCommands.Create("guildresetcooldown").WithDescription("Reset a player's guild rejoin cooldown (admin only)").WithAlias(new string[]
			{
				"guildclearcd"
			}).WithArgs(new ICommandArgumentParser[]
			{
				api.ChatCommands.Parsers.Word("playerName")
			}).RequiresPrivilege(Privilege.controlserver).HandleWith(new OnCommandDelegate(this.OnResetCooldownCommand));
			api.ChatCommands.Create("guildmanager").WithDescription("Admin guild management commands").WithAlias(new string[]
			{
				"gm"
			}).RequiresPrivilege(Privilege.controlserver).BeginSubCommand("removeclaim").WithDescription("Remove the guild claim for the chunk you are currently standing in").HandleWith(new OnCommandDelegate(this.OnGuildManagerRemoveClaimCommand)).EndSubCommand().BeginSubCommand("addplayer").WithDescription("Forcibly add a player to a guild").WithArgs(new ICommandArgumentParser[]
			{
				api.ChatCommands.Parsers.Word("playerUsername"),
				api.ChatCommands.Parsers.All("guildName")
			}).HandleWith(new OnCommandDelegate(this.OnGuildManagerAddPlayerCommand)).EndSubCommand();
			api.ChatCommands.Create("zonewhitelist").WithDescription("Manage protected zone whitelists (admin only)").WithAlias(new string[]
			{
				"zonewl"
			}).RequiresPrivilege("srguildsandkingdoms:zonewhitelist").BeginSubCommand("add").WithDescription("Add a player to a protected zone's whitelist. Usage: /zonewhitelist add <zoneId> <playerName>").WithArgs(new ICommandArgumentParser[]
			{
				api.ChatCommands.Parsers.Int("zoneId"),
				api.ChatCommands.Parsers.All("playerName")
			}).HandleWith(new OnCommandDelegate(this.OnZoneWhitelistAddCommand)).EndSubCommand().BeginSubCommand("remove").WithDescription("Remove a player from a protected zone's whitelist. Usage: /zonewhitelist remove <zoneId> <playerName>").WithArgs(new ICommandArgumentParser[]
			{
				api.ChatCommands.Parsers.Int("zoneId"),
				api.ChatCommands.Parsers.All("playerName")
			}).HandleWith(new OnCommandDelegate(this.OnZoneWhitelistRemoveCommand)).EndSubCommand().BeginSubCommand("list").WithDescription("List whitelists (all, by zone ID, or by player name)").WithArgs(new ICommandArgumentParser[]
			{
				api.ChatCommands.Parsers.OptionalAll("filter")
			}).HandleWith(new OnCommandDelegate(this.OnZoneWhitelistListCommand)).EndSubCommand().BeginSubCommand("clear").WithDescription("Clear all players from a zone's whitelist. Usage: /zonewhitelist clear <zoneId>").WithArgs(new ICommandArgumentParser[]
			{
				api.ChatCommands.Parsers.Int("zoneId")
			}).HandleWith(new OnCommandDelegate(this.OnZoneWhitelistClearCommand)).EndSubCommand().BeginSubCommand("zones").WithDescription("List all available protected zones with their IDs").HandleWith(new OnCommandDelegate(this.OnZoneWhitelistZonesCommand)).EndSubCommand();
			api.ChatCommands.Create("quests").WithDescription("Quest management commands (admin only)").RequiresPrivilege(Privilege.controlserver).BeginSubCommand("removeprogress").WithDescription("Remove a player's quest progress by period key").WithArgs(new ICommandArgumentParser[]
			{
				api.ChatCommands.Parsers.Word("playerUsername"),
				api.ChatCommands.Parsers.Word("periodKey")
			}).HandleWith(new OnCommandDelegate(this.OnQuestRemoveProgressCommand)).EndSubCommand().BeginSubCommand("listactivequests").WithDescription("List all active quests with period keys").HandleWith(new OnCommandDelegate(this.OnQuestListActiveCommand)).EndSubCommand().BeginSubCommand("givegrs").WithDescription("Give or remove GRS points from a guild (use negative amount to remove)").WithArgs(new ICommandArgumentParser[]
			{
				api.ChatCommands.Parsers.Int("amount"),
				api.ChatCommands.Parsers.All("guildName")
			}).HandleWith(new OnCommandDelegate(this.OnQuestGiveGrsCommand)).EndSubCommand();
			api.ChatCommands.Create("questmanager").WithDescription("Quest manager commands").WithAlias(new string[]
			{
				"qm"
			}).RequiresPrivilege("srguildsandkingdoms:questmanager").HandleWith(new OnCommandDelegate(this.OnQuestManagerCommand)).BeginSubCommand("set").WithDescription("Set currency definitions for quests. Usage: /questmanager set [crowns|tails] (hold item)").WithArgs(new ICommandArgumentParser[]
			{
				api.ChatCommands.Parsers.Word("currencyType")
			}).RequiresPrivilege(Privilege.controlserver).HandleWith(new OnCommandDelegate(this.OnQuestManagerSetCurrencyCommand)).EndSubCommand();
		}

		// Token: 0x06000026 RID: 38 RVA: 0x00003634 File Offset: 0x00001834
		private TextCommandResult OnGuildChatCommand(TextCommandCallingArgs args)
		{
			IServerPlayer player = args.Caller.Player as IServerPlayer;
			if (player == null)
			{
				return TextCommandResult.Error("Command can only be used by players.", "");
			}
			string action = args.Parsers[0].GetValue() as string;
			if (string.IsNullOrEmpty(action))
			{
				return TextCommandResult.Error("Please specify an action (accept, invites).", "");
			}
			string a = action.ToLowerInvariant();
			if (a == "accept")
			{
				return this.HandleAcceptInvite(player);
			}
			if (!(a == "invites") && !(a == "invite") && !(a == "list"))
			{
				return TextCommandResult.Error("Unknown guild command: " + action + ". Available commands: accept, invites", "");
			}
			return this.HandleListInvites(player);
		}

		// Token: 0x06000027 RID: 39 RVA: 0x000036FC File Offset: 0x000018FC
		private TextCommandResult HandleAcceptInvite(IServerPlayer player)
		{
			if (this.guildManager == null)
			{
				return TextCommandResult.Error("Guild system not initialized.", "");
			}
			if (this.guildManager.AcceptInvite(player.PlayerUID))
			{
				GuildNetworkHandler guildNetworkHandler = this.networkHandler;
				if (guildNetworkHandler != null)
				{
					guildNetworkHandler.SendNotification(player, "You have joined the guild.", NotificationType.Success);
				}
				GuildNetworkHandler guildNetworkHandler2 = this.networkHandler;
				if (guildNetworkHandler2 != null)
				{
					guildNetworkHandler2.BroadcastGuildSummariesToAll();
				}
				return TextCommandResult.Success("Successfully joined the guild.", null);
			}
			return TextCommandResult.Error("No pending guild invite found or invite has expired.", "");
		}

		// Token: 0x06000028 RID: 40 RVA: 0x00003778 File Offset: 0x00001978
		private TextCommandResult HandleListInvites(IServerPlayer player)
		{
			if (this.guildManager == null)
			{
				return TextCommandResult.Error("Guild system not initialized.", "");
			}
			List<GuildInvite> invites = this.guildManager.GetPlayerInvites(player.PlayerUID);
			if (invites.Count == 0)
			{
				return TextCommandResult.Success("You have no pending guild invites.", null);
			}
			List<GuildInviteInfo> inviteInfoList = new List<GuildInviteInfo>();
			using (List<GuildInvite>.Enumerator enumerator = invites.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					GuildInvite invite = enumerator.Current;
					IPlayer inviter = this.serverApi.World.AllPlayers.FirstOrDefault((IPlayer p) => p.PlayerUID == invite.InviterUid);
					inviteInfoList.Add(new GuildInviteInfo
					{
						GuildName = invite.GuildName,
						InviterName = (((inviter != null) ? inviter.PlayerName : null) ?? "Unknown"),
						InviterUid = invite.InviterUid,
						ExpiresAtTicks = invite.ExpiresAt.Ticks
					});
				}
			}
			GuildInviteListResponsePacket response = new GuildInviteListResponsePacket
			{
				PlayerUid = player.PlayerUID,
				Invites = inviteInfoList
			};
			this.serverApi.Network.GetChannel("srguildsandkingdoms:guild").SendPacket<GuildInviteListResponsePacket>(response, new IServerPlayer[]
			{
				player
			});
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(78, 1);
			defaultInterpolatedStringHandler.AppendLiteral("You have ");
			defaultInterpolatedStringHandler.AppendFormatted<int>(invites.Count);
			defaultInterpolatedStringHandler.AppendLiteral(" pending guild invite(s). Check the popup in the bottom-right corner.");
			return TextCommandResult.Success(defaultInterpolatedStringHandler.ToStringAndClear(), null);
		}

		// Token: 0x06000029 RID: 41 RVA: 0x0000391C File Offset: 0x00001B1C
		private TextCommandResult OnResetCooldownCommand(TextCommandCallingArgs args)
		{
			if (this.guildManager == null)
			{
				return TextCommandResult.Error("Guild system not initialized.", "");
			}
			string targetPlayerName = args.Parsers[0].GetValue() as string;
			if (string.IsNullOrEmpty(targetPlayerName))
			{
				return TextCommandResult.Error("Please specify a player name.", "");
			}
			string targetPlayerUid = this.GetPlayerUidByName(targetPlayerName);
			if (string.IsNullOrEmpty(targetPlayerUid))
			{
				return TextCommandResult.Error("Player '" + targetPlayerName + "' not found.", "");
			}
			ICoreServerAPI coreServerAPI = this.serverApi;
			IServerPlayer targetPlayer = ((coreServerAPI != null) ? coreServerAPI.World.AllOnlinePlayers.FirstOrDefault((IPlayer p) => p.PlayerUID == targetPlayerUid) : null) as IServerPlayer;
			TimeSpan remainingTime;
			if (!this.guildManager.IsPlayerOnCooldown(targetPlayerUid, out remainingTime))
			{
				return TextCommandResult.Success("Player '" + targetPlayerName + "' has no active guild cooldown.", null);
			}
			if (this.guildManager.ClearPlayerCooldown(targetPlayerUid))
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(60, 2);
				defaultInterpolatedStringHandler.AppendLiteral("Guild cooldown cleared for player '");
				defaultInterpolatedStringHandler.AppendFormatted(targetPlayerName);
				defaultInterpolatedStringHandler.AppendLiteral("'. ");
				defaultInterpolatedStringHandler.AppendLiteral("(Had ");
				defaultInterpolatedStringHandler.AppendFormatted<double>(remainingTime.TotalHours, "F1");
				defaultInterpolatedStringHandler.AppendLiteral(" hours remaining)");
				string text = defaultInterpolatedStringHandler.ToStringAndClear();
				if (targetPlayer != null)
				{
					targetPlayer.SendMessage(GlobalConstants.GeneralChatGroup, "Your guild rejoin cooldown has been cleared by an administrator.", 4, null);
				}
				return TextCommandResult.Success(text, null);
			}
			return TextCommandResult.Error("Failed to clear cooldown for player '" + targetPlayerName + "'.", "");
		}

		// Token: 0x0600002A RID: 42 RVA: 0x00003AAC File Offset: 0x00001CAC
		private TextCommandResult OnGuildManagerRemoveClaimCommand(TextCommandCallingArgs args)
		{
			if (this.guildManager == null || this.landClaimRepository == null || this.guildRepository == null)
			{
				return TextCommandResult.Error("Guild system not initialized.", "");
			}
			IServerPlayer player = args.Caller.Player as IServerPlayer;
			if (player == null)
			{
				return TextCommandResult.Error("Command can only be used by players.", "");
			}
			BlockPos pos = player.Entity.Pos.AsBlockPos;
			int chunkX = LandClaim.FloorDiv(pos.X, 32);
			int chunkZ = LandClaim.FloorDiv(pos.Z, 32);
			string owningGuildName = this.landClaimRepository.GetGuildOwningChunk(chunkX, chunkZ);
			if (string.IsNullOrEmpty(owningGuildName))
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(39, 2);
				defaultInterpolatedStringHandler.AppendLiteral("Chunk (");
				defaultInterpolatedStringHandler.AppendFormatted<int>(chunkX);
				defaultInterpolatedStringHandler.AppendLiteral(", ");
				defaultInterpolatedStringHandler.AppendFormatted<int>(chunkZ);
				defaultInterpolatedStringHandler.AppendLiteral(") is not claimed by any guild.");
				return TextCommandResult.Error(defaultInterpolatedStringHandler.ToStringAndClear(), "");
			}
			Guild guild = this.guildManager.GetGuild(owningGuildName);
			if (guild == null)
			{
				return TextCommandResult.Error("Guild '" + owningGuildName + "' not found.", "");
			}
			LandClaim claimToRemove = guild.Claims.FirstOrDefault(delegate(LandClaim c)
			{
				GuildHomeClaim guildHome = c as GuildHomeClaim;
				if (guildHome != null)
				{
					return guildHome.ContainsBlockCoord(chunkX * 32, chunkZ * 32);
				}
				return c.ChunkX == chunkX && c.ChunkZ == chunkZ;
			});
			if (claimToRemove == null)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(48, 3);
				defaultInterpolatedStringHandler2.AppendLiteral("Could not find claim for chunk (");
				defaultInterpolatedStringHandler2.AppendFormatted<int>(chunkX);
				defaultInterpolatedStringHandler2.AppendLiteral(", ");
				defaultInterpolatedStringHandler2.AppendFormatted<int>(chunkZ);
				defaultInterpolatedStringHandler2.AppendLiteral(") in guild '");
				defaultInterpolatedStringHandler2.AppendFormatted(owningGuildName);
				defaultInterpolatedStringHandler2.AppendLiteral("'.");
				return TextCommandResult.Error(defaultInterpolatedStringHandler2.ToStringAndClear(), "");
			}
			GuildHomeClaim guildHomeClaim = claimToRemove as GuildHomeClaim;
			if (guildHomeClaim != null)
			{
				foreach (LandClaim chunk in guildHomeClaim.GetIndividualChunks())
				{
					this.landClaimRepository.RemoveClaimFromIndex(chunk.ChunkX, chunk.ChunkZ);
				}
				guild.Claims.Remove(claimToRemove);
				this.guildRepository.MarkDirty(owningGuildName);
				ICoreServerAPI coreServerAPI = this.serverApi;
				if (coreServerAPI != null)
				{
					ILogger logger = coreServerAPI.Logger;
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler3 = new DefaultInterpolatedStringHandler(59, 2);
					defaultInterpolatedStringHandler3.AppendLiteral("[GuildManager:Admin] ");
					defaultInterpolatedStringHandler3.AppendFormatted(player.PlayerName);
					defaultInterpolatedStringHandler3.AppendLiteral(" removed guild home claim for guild '");
					defaultInterpolatedStringHandler3.AppendFormatted(owningGuildName);
					defaultInterpolatedStringHandler3.AppendLiteral("'");
					logger.Notification(defaultInterpolatedStringHandler3.ToStringAndClear());
				}
				GuildNetworkHandler guildNetworkHandler = this.networkHandler;
				if (guildNetworkHandler != null)
				{
					guildNetworkHandler.BroadcastGuildSummariesToAll();
				}
				return TextCommandResult.Success("Removed guild home claim for guild '" + owningGuildName + "' (4 chunks removed).", null);
			}
			OutpostClaim outpostClaim = claimToRemove as OutpostClaim;
			if (outpostClaim != null)
			{
				this.landClaimRepository.RemoveClaimFromIndex(chunkX, chunkZ);
				guild.Claims.Remove(claimToRemove);
				this.guildRepository.MarkDirty(owningGuildName);
				ICoreServerAPI coreServerAPI2 = this.serverApi;
				if (coreServerAPI2 != null)
				{
					ILogger logger2 = coreServerAPI2.Logger;
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler4 = new DefaultInterpolatedStringHandler(67, 5);
					defaultInterpolatedStringHandler4.AppendLiteral("[GuildManager:Admin] ");
					defaultInterpolatedStringHandler4.AppendFormatted(player.PlayerName);
					defaultInterpolatedStringHandler4.AppendLiteral(" removed outpost '");
					defaultInterpolatedStringHandler4.AppendFormatted(outpostClaim.OutpostName);
					defaultInterpolatedStringHandler4.AppendLiteral("' claim at (");
					defaultInterpolatedStringHandler4.AppendFormatted<int>(chunkX);
					defaultInterpolatedStringHandler4.AppendLiteral(", ");
					defaultInterpolatedStringHandler4.AppendFormatted<int>(chunkZ);
					defaultInterpolatedStringHandler4.AppendLiteral(") for guild '");
					defaultInterpolatedStringHandler4.AppendFormatted(owningGuildName);
					defaultInterpolatedStringHandler4.AppendLiteral("'");
					logger2.Notification(defaultInterpolatedStringHandler4.ToStringAndClear());
				}
				GuildNetworkHandler guildNetworkHandler2 = this.networkHandler;
				if (guildNetworkHandler2 != null)
				{
					guildNetworkHandler2.BroadcastGuildSummariesToAll();
				}
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler5 = new DefaultInterpolatedStringHandler(47, 4);
				defaultInterpolatedStringHandler5.AppendLiteral("Removed outpost '");
				defaultInterpolatedStringHandler5.AppendFormatted(outpostClaim.OutpostName);
				defaultInterpolatedStringHandler5.AppendLiteral("' claim at (");
				defaultInterpolatedStringHandler5.AppendFormatted<int>(chunkX);
				defaultInterpolatedStringHandler5.AppendLiteral(", ");
				defaultInterpolatedStringHandler5.AppendFormatted<int>(chunkZ);
				defaultInterpolatedStringHandler5.AppendLiteral(") from guild '");
				defaultInterpolatedStringHandler5.AppendFormatted(owningGuildName);
				defaultInterpolatedStringHandler5.AppendLiteral("'.");
				return TextCommandResult.Success(defaultInterpolatedStringHandler5.ToStringAndClear(), null);
			}
			this.landClaimRepository.RemoveClaimFromIndex(chunkX, chunkZ);
			guild.Claims.Remove(claimToRemove);
			this.guildRepository.MarkDirty(owningGuildName);
			ICoreServerAPI coreServerAPI3 = this.serverApi;
			if (coreServerAPI3 != null)
			{
				ILogger logger3 = coreServerAPI3.Logger;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler6 = new DefaultInterpolatedStringHandler(56, 4);
				defaultInterpolatedStringHandler6.AppendLiteral("[GuildManager:Admin] ");
				defaultInterpolatedStringHandler6.AppendFormatted(player.PlayerName);
				defaultInterpolatedStringHandler6.AppendLiteral(" removed claim at (");
				defaultInterpolatedStringHandler6.AppendFormatted<int>(chunkX);
				defaultInterpolatedStringHandler6.AppendLiteral(", ");
				defaultInterpolatedStringHandler6.AppendFormatted<int>(chunkZ);
				defaultInterpolatedStringHandler6.AppendLiteral(") for guild '");
				defaultInterpolatedStringHandler6.AppendFormatted(owningGuildName);
				defaultInterpolatedStringHandler6.AppendLiteral("'");
				logger3.Notification(defaultInterpolatedStringHandler6.ToStringAndClear());
			}
			GuildNetworkHandler guildNetworkHandler3 = this.networkHandler;
			if (guildNetworkHandler3 != null)
			{
				guildNetworkHandler3.BroadcastGuildSummariesToAll();
			}
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler7 = new DefaultInterpolatedStringHandler(36, 3);
			defaultInterpolatedStringHandler7.AppendLiteral("Removed claim at (");
			defaultInterpolatedStringHandler7.AppendFormatted<int>(chunkX);
			defaultInterpolatedStringHandler7.AppendLiteral(", ");
			defaultInterpolatedStringHandler7.AppendFormatted<int>(chunkZ);
			defaultInterpolatedStringHandler7.AppendLiteral(") from guild '");
			defaultInterpolatedStringHandler7.AppendFormatted(owningGuildName);
			defaultInterpolatedStringHandler7.AppendLiteral("'.");
			return TextCommandResult.Success(defaultInterpolatedStringHandler7.ToStringAndClear(), null);
		}

		// Token: 0x0600002B RID: 43 RVA: 0x00004048 File Offset: 0x00002248
		private TextCommandResult OnGuildManagerAddPlayerCommand(TextCommandCallingArgs args)
		{
			if (this.guildManager == null || this.guildRepository == null)
			{
				return TextCommandResult.Error("Guild system not initialized.", "");
			}
			string playerUsername = args.Parsers[0].GetValue() as string;
			string guildName = args.Parsers[1].GetValue() as string;
			if (string.IsNullOrEmpty(playerUsername))
			{
				return TextCommandResult.Error("Please specify a player username.", "");
			}
			if (string.IsNullOrEmpty(guildName))
			{
				return TextCommandResult.Error("Please specify a guild name.", "");
			}
			string playerUid = this.GetPlayerUidByName(playerUsername);
			if (string.IsNullOrEmpty(playerUid))
			{
				return TextCommandResult.Error("Player '" + playerUsername + "' not found.", "");
			}
			Guild existingGuild = this.guildManager.GetGuildByMember(playerUid);
			if (existingGuild != null)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(77, 2);
				defaultInterpolatedStringHandler.AppendLiteral("Player '");
				defaultInterpolatedStringHandler.AppendFormatted(playerUsername);
				defaultInterpolatedStringHandler.AppendLiteral("' is already a member of guild '");
				defaultInterpolatedStringHandler.AppendFormatted(existingGuild.Name);
				defaultInterpolatedStringHandler.AppendLiteral("'. Remove them from that guild first.");
				return TextCommandResult.Error(defaultInterpolatedStringHandler.ToStringAndClear(), "");
			}
			Guild guild = this.guildManager.GetGuild(guildName);
			if (guild == null)
			{
				return TextCommandResult.Error("Guild '" + guildName + "' not found.", "");
			}
			GuildConfigManager configManager = this.guildManager.GetConfigManager();
			GuildConfig config = (configManager != null) ? configManager.GetConfig() : null;
			if (config != null)
			{
				int maxMembers = config.MaxMembersPerGuild;
				if (guild.Members.Count >= maxMembers)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(43, 2);
					defaultInterpolatedStringHandler2.AppendLiteral("Guild '");
					defaultInterpolatedStringHandler2.AppendFormatted(guildName);
					defaultInterpolatedStringHandler2.AppendLiteral("' is at maximum capacity (");
					defaultInterpolatedStringHandler2.AppendFormatted<int>(maxMembers);
					defaultInterpolatedStringHandler2.AppendLiteral(" members).");
					return TextCommandResult.Error(defaultInterpolatedStringHandler2.ToStringAndClear(), "");
				}
			}
			guild.Members[playerUid] = new GuildMember
			{
				PlayerUid = playerUid,
				Role = "Member"
			};
			this.guildRepository.MarkDirty(guild.Name);
			ICoreServerAPI coreServerAPI = this.serverApi;
			IServerPlayer onlinePlayer = ((coreServerAPI != null) ? coreServerAPI.World.PlayerByUid(playerUid) : null) as IServerPlayer;
			if (onlinePlayer != null)
			{
				this.guildManager.SyncPlayerTraits(onlinePlayer);
				onlinePlayer.SendMessage(GlobalConstants.GeneralChatGroup, "You have been added to guild '" + guild.Name + "' by an administrator.", 4, null);
			}
			ICoreServerAPI coreServerAPI2 = this.serverApi;
			if (coreServerAPI2 != null)
			{
				ILogger logger = coreServerAPI2.Logger;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler3 = new DefaultInterpolatedStringHandler(57, 4);
				defaultInterpolatedStringHandler3.AppendLiteral("[GuildManager:Admin] ");
				IPlayer player = args.Caller.Player;
				defaultInterpolatedStringHandler3.AppendFormatted(((player != null) ? player.PlayerName : null) ?? "Console");
				defaultInterpolatedStringHandler3.AppendLiteral(" added player '");
				defaultInterpolatedStringHandler3.AppendFormatted(playerUsername);
				defaultInterpolatedStringHandler3.AppendLiteral("' (UID: ");
				defaultInterpolatedStringHandler3.AppendFormatted(playerUid);
				defaultInterpolatedStringHandler3.AppendLiteral(") to guild '");
				defaultInterpolatedStringHandler3.AppendFormatted(guild.Name);
				defaultInterpolatedStringHandler3.AppendLiteral("'");
				logger.Notification(defaultInterpolatedStringHandler3.ToStringAndClear());
			}
			GuildNetworkHandler guildNetworkHandler = this.networkHandler;
			if (guildNetworkHandler != null)
			{
				guildNetworkHandler.BroadcastGuildSummariesToAll();
			}
			string onlineStatus = (onlinePlayer != null) ? " (player notified)" : " (player offline)";
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler4 = new DefaultInterpolatedStringHandler(38, 3);
			defaultInterpolatedStringHandler4.AppendLiteral("Added player '");
			defaultInterpolatedStringHandler4.AppendFormatted(playerUsername);
			defaultInterpolatedStringHandler4.AppendLiteral("' to guild '");
			defaultInterpolatedStringHandler4.AppendFormatted(guild.Name);
			defaultInterpolatedStringHandler4.AppendLiteral("' as Member.");
			defaultInterpolatedStringHandler4.AppendFormatted(onlineStatus);
			return TextCommandResult.Success(defaultInterpolatedStringHandler4.ToStringAndClear(), null);
		}

		// Token: 0x0600002C RID: 44 RVA: 0x000043CC File Offset: 0x000025CC
		private TextCommandResult OnZoneWhitelistAddCommand(TextCommandCallingArgs args)
		{
			if (this.zoneWhitelistManager == null)
			{
				return TextCommandResult.Error("Zone whitelist system not initialized.", "");
			}
			int? zoneId = (int?)args.Parsers[0].GetValue();
			if (zoneId == null)
			{
				return TextCommandResult.Error("Please specify a valid zone ID.", "");
			}
			string playerName = (args.Parsers[1].GetValue() as string) ?? "";
			if (string.IsNullOrEmpty(playerName))
			{
				return TextCommandResult.Error("Please specify a player name.", "");
			}
			ProtectedZone zone;
			if (!this.ValidateZoneExistsById(zoneId.Value, out zone))
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(77, 1);
				defaultInterpolatedStringHandler.AppendLiteral("Protected zone with ID ");
				defaultInterpolatedStringHandler.AppendFormatted<int?>(zoneId);
				defaultInterpolatedStringHandler.AppendLiteral(" not found. Use /zonewhitelist zones to see all zones.");
				return TextCommandResult.Error(defaultInterpolatedStringHandler.ToStringAndClear(), "");
			}
			string playerUid = this.GetPlayerUidByName(playerName);
			if (string.IsNullOrEmpty(playerUid))
			{
				return TextCommandResult.Error("Player '" + playerName + "' not found.", "");
			}
			if (this.zoneWhitelistManager.AddPlayerToZone(zone.Id, playerUid))
			{
				ICoreServerAPI coreServerAPI = this.serverApi;
				IServerPlayer targetPlayer = ((coreServerAPI != null) ? coreServerAPI.World.AllOnlinePlayers.FirstOrDefault((IPlayer p) => p.PlayerUID == playerUid) : null) as IServerPlayer;
				if (targetPlayer != null)
				{
					IServerPlayer serverPlayer = targetPlayer;
					int generalChatGroup = GlobalConstants.GeneralChatGroup;
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(55, 2);
					defaultInterpolatedStringHandler2.AppendLiteral("You have been granted access to protected zone: ");
					defaultInterpolatedStringHandler2.AppendFormatted(zone.Name);
					defaultInterpolatedStringHandler2.AppendLiteral(" (ID: ");
					defaultInterpolatedStringHandler2.AppendFormatted<int>(zone.Id);
					defaultInterpolatedStringHandler2.AppendLiteral(")");
					serverPlayer.SendMessage(generalChatGroup, defaultInterpolatedStringHandler2.ToStringAndClear(), 4, null);
				}
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler3 = new DefaultInterpolatedStringHandler(44, 3);
				defaultInterpolatedStringHandler3.AppendLiteral("Added player '");
				defaultInterpolatedStringHandler3.AppendFormatted(playerName);
				defaultInterpolatedStringHandler3.AppendLiteral("' to zone '");
				defaultInterpolatedStringHandler3.AppendFormatted(zone.Name);
				defaultInterpolatedStringHandler3.AppendLiteral("' (ID: ");
				defaultInterpolatedStringHandler3.AppendFormatted<int>(zone.Id);
				defaultInterpolatedStringHandler3.AppendLiteral(") whitelist.");
				return TextCommandResult.Success(defaultInterpolatedStringHandler3.ToStringAndClear(), null);
			}
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler4 = new DefaultInterpolatedStringHandler(52, 3);
			defaultInterpolatedStringHandler4.AppendLiteral("Player '");
			defaultInterpolatedStringHandler4.AppendFormatted(playerName);
			defaultInterpolatedStringHandler4.AppendLiteral("' is already whitelisted for zone '");
			defaultInterpolatedStringHandler4.AppendFormatted(zone.Name);
			defaultInterpolatedStringHandler4.AppendLiteral("' (ID: ");
			defaultInterpolatedStringHandler4.AppendFormatted<int>(zone.Id);
			defaultInterpolatedStringHandler4.AppendLiteral(").");
			return TextCommandResult.Success(defaultInterpolatedStringHandler4.ToStringAndClear(), null);
		}

		// Token: 0x0600002D RID: 45 RVA: 0x00004660 File Offset: 0x00002860
		private TextCommandResult OnZoneWhitelistRemoveCommand(TextCommandCallingArgs args)
		{
			if (this.zoneWhitelistManager == null)
			{
				return TextCommandResult.Error("Zone whitelist system not initialized.", "");
			}
			int? zoneId = (int?)args.Parsers[0].GetValue();
			if (zoneId == null)
			{
				return TextCommandResult.Error("Please specify a valid zone ID.", "");
			}
			string playerName = (args.Parsers[1].GetValue() as string) ?? "";
			if (string.IsNullOrEmpty(playerName))
			{
				return TextCommandResult.Error("Please specify a player name.", "");
			}
			ProtectedZone zone;
			if (!this.ValidateZoneExistsById(zoneId.Value, out zone))
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(77, 1);
				defaultInterpolatedStringHandler.AppendLiteral("Protected zone with ID ");
				defaultInterpolatedStringHandler.AppendFormatted<int?>(zoneId);
				defaultInterpolatedStringHandler.AppendLiteral(" not found. Use /zonewhitelist zones to see all zones.");
				return TextCommandResult.Error(defaultInterpolatedStringHandler.ToStringAndClear(), "");
			}
			string playerUid = this.GetPlayerUidByName(playerName);
			if (string.IsNullOrEmpty(playerUid))
			{
				return TextCommandResult.Error("Player '" + playerName + "' not found.", "");
			}
			if (this.zoneWhitelistManager.RemovePlayerFromZone(zone.Id, playerUid))
			{
				ICoreServerAPI coreServerAPI = this.serverApi;
				IServerPlayer targetPlayer = ((coreServerAPI != null) ? coreServerAPI.World.AllOnlinePlayers.FirstOrDefault((IPlayer p) => p.PlayerUID == playerUid) : null) as IServerPlayer;
				if (targetPlayer != null)
				{
					IServerPlayer serverPlayer = targetPlayer;
					int generalChatGroup = GlobalConstants.GeneralChatGroup;
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(57, 2);
					defaultInterpolatedStringHandler2.AppendLiteral("Your access to protected zone '");
					defaultInterpolatedStringHandler2.AppendFormatted(zone.Name);
					defaultInterpolatedStringHandler2.AppendLiteral("' (ID: ");
					defaultInterpolatedStringHandler2.AppendFormatted<int>(zone.Id);
					defaultInterpolatedStringHandler2.AppendLiteral(") has been revoked.");
					serverPlayer.SendMessage(generalChatGroup, defaultInterpolatedStringHandler2.ToStringAndClear(), 4, null);
				}
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler3 = new DefaultInterpolatedStringHandler(48, 3);
				defaultInterpolatedStringHandler3.AppendLiteral("Removed player '");
				defaultInterpolatedStringHandler3.AppendFormatted(playerName);
				defaultInterpolatedStringHandler3.AppendLiteral("' from zone '");
				defaultInterpolatedStringHandler3.AppendFormatted(zone.Name);
				defaultInterpolatedStringHandler3.AppendLiteral("' (ID: ");
				defaultInterpolatedStringHandler3.AppendFormatted<int>(zone.Id);
				defaultInterpolatedStringHandler3.AppendLiteral(") whitelist.");
				return TextCommandResult.Success(defaultInterpolatedStringHandler3.ToStringAndClear(), null);
			}
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler4 = new DefaultInterpolatedStringHandler(49, 3);
			defaultInterpolatedStringHandler4.AppendLiteral("Player '");
			defaultInterpolatedStringHandler4.AppendFormatted(playerName);
			defaultInterpolatedStringHandler4.AppendLiteral("' was not whitelisted for zone '");
			defaultInterpolatedStringHandler4.AppendFormatted(zone.Name);
			defaultInterpolatedStringHandler4.AppendLiteral("' (ID: ");
			defaultInterpolatedStringHandler4.AppendFormatted<int>(zone.Id);
			defaultInterpolatedStringHandler4.AppendLiteral(").");
			return TextCommandResult.Success(defaultInterpolatedStringHandler4.ToStringAndClear(), null);
		}

		// Token: 0x0600002E RID: 46 RVA: 0x000048F4 File Offset: 0x00002AF4
		private TextCommandResult OnZoneWhitelistListCommand(TextCommandCallingArgs args)
		{
			if (this.zoneWhitelistManager == null)
			{
				return TextCommandResult.Error("Zone whitelist system not initialized.", "");
			}
			string filter = (args.Parsers[0].GetValue() as string) ?? "";
			if (string.IsNullOrEmpty(filter))
			{
				return this.ListAllWhitelists();
			}
			int zoneId;
			if (int.TryParse(filter, out zoneId))
			{
				ProtectedZone zone;
				if (!this.ValidateZoneExistsById(zoneId, out zone))
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(34, 1);
					defaultInterpolatedStringHandler.AppendLiteral("Protected zone with ID ");
					defaultInterpolatedStringHandler.AppendFormatted<int>(zoneId);
					defaultInterpolatedStringHandler.AppendLiteral(" not found.");
					return TextCommandResult.Error(defaultInterpolatedStringHandler.ToStringAndClear(), "");
				}
				return this.ListPlayersInZone(zone);
			}
			else
			{
				string playerUid = this.GetPlayerUidByName(filter);
				if (!string.IsNullOrEmpty(playerUid))
				{
					return this.ListZonesForPlayer(filter, playerUid);
				}
				return TextCommandResult.Error("'" + filter + "' is neither a valid zone ID nor player name.", "");
			}
		}

		// Token: 0x0600002F RID: 47 RVA: 0x000049D8 File Offset: 0x00002BD8
		private TextCommandResult OnZoneWhitelistClearCommand(TextCommandCallingArgs args)
		{
			if (this.zoneWhitelistManager == null)
			{
				return TextCommandResult.Error("Zone whitelist system not initialized.", "");
			}
			int? zoneId = (int?)args.Parsers[0].GetValue();
			if (zoneId == null)
			{
				return TextCommandResult.Error("Please specify a valid zone ID.", "");
			}
			ProtectedZone zone;
			if (!this.ValidateZoneExistsById(zoneId.Value, out zone))
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(34, 1);
				defaultInterpolatedStringHandler.AppendLiteral("Protected zone with ID ");
				defaultInterpolatedStringHandler.AppendFormatted<int?>(zoneId);
				defaultInterpolatedStringHandler.AppendLiteral(" not found.");
				return TextCommandResult.Error(defaultInterpolatedStringHandler.ToStringAndClear(), "");
			}
			int count = this.zoneWhitelistManager.ClearZone(zone.Id);
			if (count > 0)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(49, 3);
				defaultInterpolatedStringHandler2.AppendLiteral("Cleared ");
				defaultInterpolatedStringHandler2.AppendFormatted<int>(count);
				defaultInterpolatedStringHandler2.AppendLiteral(" player(s) from zone '");
				defaultInterpolatedStringHandler2.AppendFormatted(zone.Name);
				defaultInterpolatedStringHandler2.AppendLiteral("' (ID: ");
				defaultInterpolatedStringHandler2.AppendFormatted<int>(zone.Id);
				defaultInterpolatedStringHandler2.AppendLiteral(") whitelist.");
				return TextCommandResult.Success(defaultInterpolatedStringHandler2.ToStringAndClear(), null);
			}
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler3 = new DefaultInterpolatedStringHandler(43, 2);
			defaultInterpolatedStringHandler3.AppendLiteral("Zone '");
			defaultInterpolatedStringHandler3.AppendFormatted(zone.Name);
			defaultInterpolatedStringHandler3.AppendLiteral("' (ID: ");
			defaultInterpolatedStringHandler3.AppendFormatted<int>(zone.Id);
			defaultInterpolatedStringHandler3.AppendLiteral(") whitelist was already empty.");
			return TextCommandResult.Success(defaultInterpolatedStringHandler3.ToStringAndClear(), null);
		}

		// Token: 0x06000030 RID: 48 RVA: 0x00004B50 File Offset: 0x00002D50
		private TextCommandResult OnQuestItemCommand(TextCommandCallingArgs args)
		{
			IServerPlayer player = args.Caller.Player as IServerPlayer;
			if (player == null)
			{
				return TextCommandResult.Error("Command can only be used by players.", "");
			}
			ItemSlot slot = player.InventoryManager.ActiveHotbarSlot;
			if (((slot != null) ? slot.Itemstack : null) == null)
			{
				return TextCommandResult.Error("No item in hand! Hold an item and try again.", "");
			}
			ItemStack itemstack = slot.Itemstack;
			string code = itemstack.Collectible.Code.ToString();
			int amount = itemstack.StackSize;
			string nbtBase64 = null;
			if (itemstack.Attributes != null && itemstack.Attributes.Count > 0)
			{
				try
				{
					using (MemoryStream ms = new MemoryStream())
					{
						using (BinaryWriter writer = new BinaryWriter(ms))
						{
							itemstack.Attributes.ToBytes(writer);
							nbtBase64 = Convert.ToBase64String(ms.ToArray());
						}
					}
				}
				catch (Exception ex)
				{
					ICoreServerAPI coreServerAPI = this.serverApi;
					if (coreServerAPI != null)
					{
						coreServerAPI.Logger.Error("[QuestItem] Failed to serialize NBT data: " + ex.Message);
					}
					return TextCommandResult.Error("Failed to extract NBT data: " + ex.Message, "");
				}
			}
			ICoreServerAPI coreServerAPI2 = this.serverApi;
			if (coreServerAPI2 != null)
			{
				coreServerAPI2.Logger.Notification("================================================");
			}
			ICoreServerAPI coreServerAPI3 = this.serverApi;
			if (coreServerAPI3 != null)
			{
				coreServerAPI3.Logger.Notification("[QuestItem] Item Code: " + code);
			}
			ICoreServerAPI coreServerAPI4 = this.serverApi;
			if (coreServerAPI4 != null)
			{
				ILogger logger = coreServerAPI4.Logger;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(24, 1);
				defaultInterpolatedStringHandler.AppendLiteral("[QuestItem] Stack Size: ");
				defaultInterpolatedStringHandler.AppendFormatted<int>(amount);
				logger.Notification(defaultInterpolatedStringHandler.ToStringAndClear());
			}
			ICoreServerAPI coreServerAPI5 = this.serverApi;
			if (coreServerAPI5 != null)
			{
				coreServerAPI5.Logger.Notification("[QuestItem] NBT Base64: " + (nbtBase64 ?? "NULL"));
			}
			ICoreServerAPI coreServerAPI6 = this.serverApi;
			if (coreServerAPI6 != null)
			{
				coreServerAPI6.Logger.Notification("================================================");
			}
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(58, 3);
			defaultInterpolatedStringHandler2.AppendLiteral("Item data logged to server-main.txt:\nCode: ");
			defaultInterpolatedStringHandler2.AppendFormatted(code);
			defaultInterpolatedStringHandler2.AppendLiteral("\nAmount: ");
			defaultInterpolatedStringHandler2.AppendFormatted<int>(amount);
			defaultInterpolatedStringHandler2.AppendLiteral("\nNBT: ");
			defaultInterpolatedStringHandler2.AppendFormatted((nbtBase64 == null) ? "None" : "See log");
			return TextCommandResult.Success(defaultInterpolatedStringHandler2.ToStringAndClear(), null);
		}

		// Token: 0x06000031 RID: 49 RVA: 0x00004DCC File Offset: 0x00002FCC
		private TextCommandResult OnQuestRemoveProgressCommand(TextCommandCallingArgs args)
		{
			if (this.questRepository == null)
			{
				return TextCommandResult.Error("Quest system not initialized.", "");
			}
			string playerUsername = args.Parsers[0].GetValue() as string;
			string periodKey = args.Parsers[1].GetValue() as string;
			if (string.IsNullOrEmpty(playerUsername) || string.IsNullOrEmpty(periodKey))
			{
				return TextCommandResult.Error("Please specify both player username and period key.", "");
			}
			string playerUid = this.GetPlayerUidByName(playerUsername);
			if (string.IsNullOrEmpty(playerUid))
			{
				return TextCommandResult.Error("Player '" + playerUsername + "' not found.", "");
			}
			TextCommandResult result;
			try
			{
				int removedCount = this.questRepository.RemovePlayerQuestProgressByPeriodKey(playerUid, periodKey);
				if (removedCount > 0)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(66, 3);
					defaultInterpolatedStringHandler.AppendLiteral("Removed ");
					defaultInterpolatedStringHandler.AppendFormatted<int>(removedCount);
					defaultInterpolatedStringHandler.AppendLiteral(" quest progress entry(s) for player '");
					defaultInterpolatedStringHandler.AppendFormatted(playerUsername);
					defaultInterpolatedStringHandler.AppendLiteral("' with period key '");
					defaultInterpolatedStringHandler.AppendFormatted(periodKey);
					defaultInterpolatedStringHandler.AppendLiteral("'.");
					result = TextCommandResult.Success(defaultInterpolatedStringHandler.ToStringAndClear(), null);
				}
				else
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(57, 2);
					defaultInterpolatedStringHandler2.AppendLiteral("No quest progress found for player '");
					defaultInterpolatedStringHandler2.AppendFormatted(playerUsername);
					defaultInterpolatedStringHandler2.AppendLiteral("' with period key '");
					defaultInterpolatedStringHandler2.AppendFormatted(periodKey);
					defaultInterpolatedStringHandler2.AppendLiteral("'.");
					result = TextCommandResult.Success(defaultInterpolatedStringHandler2.ToStringAndClear(), null);
				}
			}
			catch (Exception ex)
			{
				ICoreServerAPI coreServerAPI = this.serverApi;
				if (coreServerAPI != null)
				{
					coreServerAPI.Logger.Error("[QuestAdmin] Failed to remove quest progress: " + ex.Message);
				}
				result = TextCommandResult.Error("Failed to remove quest progress: " + ex.Message, "");
			}
			return result;
		}

		// Token: 0x06000032 RID: 50 RVA: 0x00004F8C File Offset: 0x0000318C
		private TextCommandResult OnQuestListActiveCommand(TextCommandCallingArgs args)
		{
			if (this.questRepository == null)
			{
				return TextCommandResult.Error("Quest system not initialized.", "");
			}
			TextCommandResult result;
			try
			{
				ICoreServerAPI coreServerAPI = this.serverApi;
				IGameCalendar calendar = (coreServerAPI != null) ? coreServerAPI.World.Calendar : null;
				GameDate? ingameDate = null;
				if (calendar != null)
				{
					ingameDate = new GameDate?(new GameDate(calendar.Year + 1, calendar.Month, calendar.DayOfYear % calendar.DaysPerMonth + 1));
				}
				List<Quest> activeQuests = this.questRepository.GetAllActiveQuests(ingameDate);
				if (activeQuests.Count == 0)
				{
					result = TextCommandResult.Success("No active quests found.", null);
				}
				else
				{
					StringBuilder output = new StringBuilder();
					StringBuilder stringBuilder = output;
					StringBuilder stringBuilder2 = stringBuilder;
					StringBuilder.AppendInterpolatedStringHandler appendInterpolatedStringHandler = new StringBuilder.AppendInterpolatedStringHandler(24, 1, stringBuilder);
					appendInterpolatedStringHandler.AppendLiteral("=== ACTIVE QUESTS (");
					appendInterpolatedStringHandler.AppendFormatted<int>(activeQuests.Count);
					appendInterpolatedStringHandler.AppendLiteral(") ===");
					stringBuilder2.AppendLine(ref appendInterpolatedStringHandler);
					output.AppendLine("Format: {ID} - {PeriodKey} - {Title}");
					output.AppendLine();
					foreach (Quest quest in from q in activeQuests
					orderby q.RecurrenceType, q.Id
					select q)
					{
						string periodKey = quest.GeneratePeriodKey();
						stringBuilder = output;
						StringBuilder stringBuilder3 = stringBuilder;
						appendInterpolatedStringHandler = new StringBuilder.AppendInterpolatedStringHandler(6, 3, stringBuilder);
						appendInterpolatedStringHandler.AppendFormatted<int>(quest.Id);
						appendInterpolatedStringHandler.AppendLiteral(" - ");
						appendInterpolatedStringHandler.AppendFormatted(periodKey);
						appendInterpolatedStringHandler.AppendLiteral(" - ");
						appendInterpolatedStringHandler.AppendFormatted(quest.Title);
						stringBuilder3.AppendLine(ref appendInterpolatedStringHandler);
						stringBuilder = output;
						StringBuilder stringBuilder4 = stringBuilder;
						appendInterpolatedStringHandler = new StringBuilder.AppendInterpolatedStringHandler(22, 2, stringBuilder);
						appendInterpolatedStringHandler.AppendLiteral("  └─ Type: ");
						appendInterpolatedStringHandler.AppendFormatted<QuestRecurrenceType>(quest.RecurrenceType);
						appendInterpolatedStringHandler.AppendLiteral(", Expires: ");
						appendInterpolatedStringHandler.AppendFormatted(quest.ExpiresAt);
						stringBuilder4.AppendLine(ref appendInterpolatedStringHandler);
					}
					ICoreServerAPI coreServerAPI2 = this.serverApi;
					if (coreServerAPI2 != null)
					{
						coreServerAPI2.Logger.Notification(output.ToString());
					}
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(58, 1);
					defaultInterpolatedStringHandler.AppendLiteral("Listed ");
					defaultInterpolatedStringHandler.AppendFormatted<int>(activeQuests.Count);
					defaultInterpolatedStringHandler.AppendLiteral(" active quest(s). Check server console for details.");
					result = TextCommandResult.Success(defaultInterpolatedStringHandler.ToStringAndClear(), null);
				}
			}
			catch (Exception ex)
			{
				ICoreServerAPI coreServerAPI3 = this.serverApi;
				if (coreServerAPI3 != null)
				{
					coreServerAPI3.Logger.Error("[QuestAdmin] Failed to list active quests: " + ex.Message);
				}
				result = TextCommandResult.Error("Failed to list active quests: " + ex.Message, "");
			}
			return result;
		}

		// Token: 0x06000033 RID: 51 RVA: 0x0000527C File Offset: 0x0000347C
		private TextCommandResult OnQuestGiveGrsCommand(TextCommandCallingArgs args)
		{
			if (this.guildManager == null || this.guildRepository == null)
			{
				return TextCommandResult.Error("Guild system not initialized.", "");
			}
			int? amount = args.Parsers[0].GetValue() as int?;
			string guildName = args.Parsers[1].GetValue() as string;
			if (amount == null || amount.Value == 0)
			{
				return TextCommandResult.Error("Please specify a non-zero amount (positive to add, negative to remove).", "");
			}
			if (string.IsNullOrEmpty(guildName))
			{
				return TextCommandResult.Error("Please specify a guild name.", "");
			}
			TextCommandResult result;
			try
			{
				Guild guild = this.guildManager.GetGuild(guildName);
				if (guild == null)
				{
					result = TextCommandResult.Error("Guild '" + guildName + "' not found.", "");
				}
				else
				{
					int previousPoints = guild.Points;
					guild.Points = Math.Max(0, guild.Points + amount.Value);
					this.guildRepository.MarkDirty(guild.Name);
					string action = (amount.Value > 0) ? "Added" : "Removed";
					int displayAmount = Math.Abs(amount.Value);
					ICoreServerAPI coreServerAPI = this.serverApi;
					if (coreServerAPI != null)
					{
						ILogger logger = coreServerAPI.Logger;
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(48, 6);
						defaultInterpolatedStringHandler.AppendLiteral("[QuestAdmin] ");
						defaultInterpolatedStringHandler.AppendFormatted(action);
						defaultInterpolatedStringHandler.AppendLiteral(" ");
						defaultInterpolatedStringHandler.AppendFormatted<int>(displayAmount);
						defaultInterpolatedStringHandler.AppendLiteral(" GRS points ");
						defaultInterpolatedStringHandler.AppendFormatted((amount.Value > 0) ? "to" : "from");
						defaultInterpolatedStringHandler.AppendLiteral(" guild '");
						defaultInterpolatedStringHandler.AppendFormatted(guildName);
						defaultInterpolatedStringHandler.AppendLiteral("' (was ");
						defaultInterpolatedStringHandler.AppendFormatted<int>(previousPoints);
						defaultInterpolatedStringHandler.AppendLiteral(", now ");
						defaultInterpolatedStringHandler.AppendFormatted<int>(guild.Points);
						defaultInterpolatedStringHandler.AppendLiteral(")");
						logger.Notification(defaultInterpolatedStringHandler.ToStringAndClear());
					}
					GuildNetworkHandler guildNetworkHandler = this.networkHandler;
					if (guildNetworkHandler != null)
					{
						guildNetworkHandler.BroadcastGuildSummariesToAll();
					}
					this.guildManager.SyncGuildMemberTraits(guild);
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(40, 5);
					defaultInterpolatedStringHandler2.AppendFormatted(action);
					defaultInterpolatedStringHandler2.AppendLiteral(" ");
					defaultInterpolatedStringHandler2.AppendFormatted<int>(displayAmount);
					defaultInterpolatedStringHandler2.AppendLiteral(" GRS points ");
					defaultInterpolatedStringHandler2.AppendFormatted((amount.Value > 0) ? "to" : "from");
					defaultInterpolatedStringHandler2.AppendLiteral(" guild '");
					defaultInterpolatedStringHandler2.AppendFormatted(guildName);
					defaultInterpolatedStringHandler2.AppendLiteral("'. New total: ");
					defaultInterpolatedStringHandler2.AppendFormatted<int>(guild.Points);
					defaultInterpolatedStringHandler2.AppendLiteral(" GRS.");
					result = TextCommandResult.Success(defaultInterpolatedStringHandler2.ToStringAndClear(), null);
				}
			}
			catch (Exception ex)
			{
				ICoreServerAPI coreServerAPI2 = this.serverApi;
				if (coreServerAPI2 != null)
				{
					coreServerAPI2.Logger.Error("[QuestAdmin] Failed to modify GRS points: " + ex.Message);
				}
				result = TextCommandResult.Error("Failed to modify GRS points: " + ex.Message, "");
			}
			return result;
		}

		// Token: 0x06000034 RID: 52 RVA: 0x00005588 File Offset: 0x00003788
		private TextCommandResult OnQuestManagerCommand(TextCommandCallingArgs args)
		{
			IServerPlayer player = args.Caller.Player as IServerPlayer;
			if (player == null)
			{
				return TextCommandResult.Error("Command can only be used by players.", "");
			}
			OpenQuestManagerPacket packet = new OpenQuestManagerPacket
			{
				PlayerUid = player.PlayerUID
			};
			this.serverApi.Network.GetChannel("srguildsandkingdoms:quest").SendPacket<OpenQuestManagerPacket>(packet, new IServerPlayer[]
			{
				player
			});
			return TextCommandResult.Success("Opening Quest Manager...", null);
		}

		// Token: 0x06000035 RID: 53 RVA: 0x000055FC File Offset: 0x000037FC
		private TextCommandResult OnQuestManagerSetCurrencyCommand(TextCommandCallingArgs args)
		{
			if (this.guildManager == null)
			{
				return TextCommandResult.Error("Guild system not initialized.", "");
			}
			IServerPlayer player = args.Caller.Player as IServerPlayer;
			if (player == null)
			{
				return TextCommandResult.Error("Command can only be used by players.", "");
			}
			string currencyType = args.Parsers[0].GetValue() as string;
			if (string.IsNullOrEmpty(currencyType))
			{
				return TextCommandResult.Error("Please specify currency type: crowns or tails", "");
			}
			if (!currencyType.Equals("crowns", StringComparison.OrdinalIgnoreCase) && !currencyType.Equals("tails", StringComparison.OrdinalIgnoreCase))
			{
				return TextCommandResult.Error("Currency type must be 'crowns' or 'tails'", "");
			}
			ItemSlot slot = player.InventoryManager.ActiveHotbarSlot;
			if (((slot != null) ? slot.Itemstack : null) == null)
			{
				return TextCommandResult.Error("No item in hand! Hold an item and try again.", "");
			}
			ItemStack itemstack = slot.Itemstack;
			string code = itemstack.Collectible.Code.ToString();
			string nbtBase64 = null;
			if (itemstack.Attributes != null && itemstack.Attributes.Count > 0)
			{
				try
				{
					using (MemoryStream ms = new MemoryStream())
					{
						using (BinaryWriter writer = new BinaryWriter(ms))
						{
							itemstack.Attributes.ToBytes(writer);
							nbtBase64 = Convert.ToBase64String(ms.ToArray());
						}
					}
				}
				catch (Exception ex)
				{
					ICoreServerAPI coreServerAPI = this.serverApi;
					if (coreServerAPI != null)
					{
						coreServerAPI.Logger.Error("[QuestCurrency] Failed to serialize NBT data: " + ex.Message);
					}
					return TextCommandResult.Error("Failed to extract NBT data: " + ex.Message, "");
				}
			}
			TextCommandResult result;
			try
			{
				this.guildManager.GetConfigManager().UpdateQuestCurrency(currencyType, code, nbtBase64);
				string currencyName = currencyType.Equals("crowns", StringComparison.OrdinalIgnoreCase) ? "Crowns" : "Tails";
				string nbtInfo = (nbtBase64 == null) ? "" : " (with NBT data)";
				ICoreServerAPI coreServerAPI2 = this.serverApi;
				if (coreServerAPI2 != null)
				{
					ILogger logger = coreServerAPI2.Logger;
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(35, 4);
					defaultInterpolatedStringHandler.AppendLiteral("[QuestCurrency] ");
					defaultInterpolatedStringHandler.AppendFormatted(player.PlayerName);
					defaultInterpolatedStringHandler.AppendLiteral(" set ");
					defaultInterpolatedStringHandler.AppendFormatted(currencyName);
					defaultInterpolatedStringHandler.AppendLiteral(" currency to: ");
					defaultInterpolatedStringHandler.AppendFormatted(code);
					defaultInterpolatedStringHandler.AppendFormatted(nbtInfo);
					logger.Notification(defaultInterpolatedStringHandler.ToStringAndClear());
				}
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(18, 3);
				defaultInterpolatedStringHandler2.AppendLiteral("Set ");
				defaultInterpolatedStringHandler2.AppendFormatted(currencyName);
				defaultInterpolatedStringHandler2.AppendLiteral(" currency to: ");
				defaultInterpolatedStringHandler2.AppendFormatted(code);
				defaultInterpolatedStringHandler2.AppendFormatted(nbtInfo);
				result = TextCommandResult.Success(defaultInterpolatedStringHandler2.ToStringAndClear(), null);
			}
			catch (Exception ex2)
			{
				ICoreServerAPI coreServerAPI3 = this.serverApi;
				if (coreServerAPI3 != null)
				{
					coreServerAPI3.Logger.Error("[QuestCurrency] Failed to update currency: " + ex2.Message);
				}
				result = TextCommandResult.Error("Failed to update currency: " + ex2.Message, "");
			}
			return result;
		}

		// Token: 0x06000036 RID: 54 RVA: 0x0000594C File Offset: 0x00003B4C
		private TextCommandResult OnZoneWhitelistZonesCommand(TextCommandCallingArgs args)
		{
			GuildManager guildManager = this.guildManager;
			GuildConfig guildConfig;
			if (guildManager == null)
			{
				guildConfig = null;
			}
			else
			{
				GuildConfigManager configManager = guildManager.GetConfigManager();
				guildConfig = ((configManager != null) ? configManager.GetConfig() : null);
			}
			GuildConfig config = guildConfig;
			if (((config != null) ? config.ProtectedZones : null) == null || config.ProtectedZones.Count == 0)
			{
				return TextCommandResult.Success("No protected zones configured.", null);
			}
			StringBuilder output = new StringBuilder();
			StringBuilder stringBuilder = output;
			StringBuilder stringBuilder2 = stringBuilder;
			StringBuilder.AppendInterpolatedStringHandler appendInterpolatedStringHandler = new StringBuilder.AppendInterpolatedStringHandler(26, 1, stringBuilder);
			appendInterpolatedStringHandler.AppendLiteral("=== PROTECTED ZONES (");
			appendInterpolatedStringHandler.AppendFormatted<int>(config.ProtectedZones.Count);
			appendInterpolatedStringHandler.AppendLiteral(") ===");
			stringBuilder2.AppendLine(ref appendInterpolatedStringHandler);
			output.AppendLine("ID | Name | Center | Radius");
			output.AppendLine("---|------|--------|-------");
			foreach (ProtectedZone zone in from z in config.ProtectedZones
			orderby z.Id
			select z)
			{
				stringBuilder = output;
				StringBuilder stringBuilder3 = stringBuilder;
				appendInterpolatedStringHandler = new StringBuilder.AppendInterpolatedStringHandler(20, 5, stringBuilder);
				appendInterpolatedStringHandler.AppendFormatted<int>(zone.Id);
				appendInterpolatedStringHandler.AppendLiteral(" | ");
				appendInterpolatedStringHandler.AppendFormatted(zone.Name);
				appendInterpolatedStringHandler.AppendLiteral(" | (");
				appendInterpolatedStringHandler.AppendFormatted<int>(zone.X);
				appendInterpolatedStringHandler.AppendLiteral(", ");
				appendInterpolatedStringHandler.AppendFormatted<int>(zone.Z);
				appendInterpolatedStringHandler.AppendLiteral(") | ");
				appendInterpolatedStringHandler.AppendFormatted<int>(zone.Radius);
				appendInterpolatedStringHandler.AppendLiteral(" blocks");
				stringBuilder3.AppendLine(ref appendInterpolatedStringHandler);
			}
			base.Mod.Logger.Notification(output.ToString());
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(60, 1);
			defaultInterpolatedStringHandler.AppendLiteral("Listed ");
			defaultInterpolatedStringHandler.AppendFormatted<int>(config.ProtectedZones.Count);
			defaultInterpolatedStringHandler.AppendLiteral(" protected zone(s). Check server console for details.");
			return TextCommandResult.Success(defaultInterpolatedStringHandler.ToStringAndClear(), null);
		}

		// Token: 0x06000037 RID: 55 RVA: 0x00005B54 File Offset: 0x00003D54
		[return: Nullable(2)]
		private string GetPlayerUidByName(string playerName)
		{
			if (string.IsNullOrEmpty(playerName) || this.serverApi == null)
			{
				return null;
			}
			IPlayer onlinePlayer = this.serverApi.World.AllOnlinePlayers.FirstOrDefault((IPlayer p) => p.PlayerName.Equals(playerName, StringComparison.OrdinalIgnoreCase));
			if (onlinePlayer != null)
			{
				return onlinePlayer.PlayerUID;
			}
			KeyValuePair<string, IServerPlayerData> playerData = this.serverApi.PlayerData.PlayerDataByUid.FirstOrDefault(delegate(KeyValuePair<string, IServerPlayerData> kvp)
			{
				string lastKnownPlayername = kvp.Value.LastKnownPlayername;
				return lastKnownPlayername != null && lastKnownPlayername.Equals(playerName, StringComparison.OrdinalIgnoreCase);
			});
			if (!string.IsNullOrEmpty(playerData.Key))
			{
				return playerData.Key;
			}
			return null;
		}

		// Token: 0x06000038 RID: 56 RVA: 0x00005BEC File Offset: 0x00003DEC
		[NullableContext(2)]
		private bool ValidateZoneExistsById(int zoneId, out ProtectedZone zone)
		{
			zone = null;
			if (zoneId < 0)
			{
				return false;
			}
			GuildManager guildManager = this.guildManager;
			GuildConfig guildConfig;
			if (guildManager == null)
			{
				guildConfig = null;
			}
			else
			{
				GuildConfigManager configManager = guildManager.GetConfigManager();
				guildConfig = ((configManager != null) ? configManager.GetConfig() : null);
			}
			GuildConfig config = guildConfig;
			if (((config != null) ? config.ProtectedZones : null) == null)
			{
				return false;
			}
			zone = config.ProtectedZones.FirstOrDefault((ProtectedZone z) => z.Id == zoneId);
			return zone != null;
		}

		// Token: 0x06000039 RID: 57 RVA: 0x00005C64 File Offset: 0x00003E64
		private TextCommandResult ListAllWhitelists()
		{
			if (this.zoneWhitelistManager == null || this.serverApi == null)
			{
				return TextCommandResult.Error("Zone whitelist system not initialized.", "");
			}
			List<int> allZoneIds = this.zoneWhitelistManager.GetAllZoneIds();
			if (allZoneIds.Count == 0)
			{
				return TextCommandResult.Success("No zone whitelists configured.", null);
			}
			GuildManager guildManager = this.guildManager;
			GuildConfig guildConfig;
			if (guildManager == null)
			{
				guildConfig = null;
			}
			else
			{
				GuildConfigManager configManager = guildManager.GetConfigManager();
				guildConfig = ((configManager != null) ? configManager.GetConfig() : null);
			}
			GuildConfig config = guildConfig;
			StringBuilder output = new StringBuilder();
			StringBuilder stringBuilder = output;
			StringBuilder stringBuilder2 = stringBuilder;
			StringBuilder.AppendInterpolatedStringHandler appendInterpolatedStringHandler = new StringBuilder.AppendInterpolatedStringHandler(32, 1, stringBuilder);
			appendInterpolatedStringHandler.AppendLiteral("=== ZONE WHITELISTS (");
			appendInterpolatedStringHandler.AppendFormatted<int>(allZoneIds.Count);
			appendInterpolatedStringHandler.AppendLiteral(" zones) ===");
			stringBuilder2.AppendLine(ref appendInterpolatedStringHandler);
			using (IEnumerator<int> enumerator = (from z in allZoneIds
			orderby z
			select z).GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					int zoneId = enumerator.Current;
					object obj;
					if (config == null)
					{
						obj = null;
					}
					else
					{
						List<ProtectedZone> protectedZones = config.ProtectedZones;
						obj = ((protectedZones != null) ? protectedZones.FirstOrDefault((ProtectedZone z) => z.Id == zoneId) : null);
					}
					object obj2 = obj;
					string text;
					if ((text = ((obj2 != null) ? obj2.Name : null)) == null)
					{
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(14, 1);
						defaultInterpolatedStringHandler.AppendLiteral("Unknown (ID: ");
						defaultInterpolatedStringHandler.AppendFormatted<int>(zoneId);
						defaultInterpolatedStringHandler.AppendLiteral(")");
						text = defaultInterpolatedStringHandler.ToStringAndClear();
					}
					string zoneName = text;
					List<string> players = this.zoneWhitelistManager.GetWhitelistedPlayers(zoneId);
					stringBuilder = output;
					StringBuilder stringBuilder3 = stringBuilder;
					appendInterpolatedStringHandler = new StringBuilder.AppendInterpolatedStringHandler(22, 3, stringBuilder);
					appendInterpolatedStringHandler.AppendLiteral("\n[ID: ");
					appendInterpolatedStringHandler.AppendFormatted<int>(zoneId);
					appendInterpolatedStringHandler.AppendLiteral("] ");
					appendInterpolatedStringHandler.AppendFormatted(zoneName);
					appendInterpolatedStringHandler.AppendLiteral(" - ");
					appendInterpolatedStringHandler.AppendFormatted<int>(players.Count);
					appendInterpolatedStringHandler.AppendLiteral(" player(s):");
					stringBuilder3.AppendLine(ref appendInterpolatedStringHandler);
					foreach (string playerUid in players)
					{
						string playerName = this.GetPlayerNameByUid(playerUid);
						stringBuilder = output;
						StringBuilder stringBuilder4 = stringBuilder;
						appendInterpolatedStringHandler = new StringBuilder.AppendInterpolatedStringHandler(4, 1, stringBuilder);
						appendInterpolatedStringHandler.AppendLiteral("  • ");
						appendInterpolatedStringHandler.AppendFormatted(playerName ?? playerUid);
						stringBuilder4.AppendLine(ref appendInterpolatedStringHandler);
					}
				}
			}
			base.Mod.Logger.Notification(output.ToString());
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(60, 1);
			defaultInterpolatedStringHandler2.AppendLiteral("Listed ");
			defaultInterpolatedStringHandler2.AppendFormatted<int>(allZoneIds.Count);
			defaultInterpolatedStringHandler2.AppendLiteral(" zone whitelist(s). Check server console for details.");
			return TextCommandResult.Success(defaultInterpolatedStringHandler2.ToStringAndClear(), null);
		}

		// Token: 0x0600003A RID: 58 RVA: 0x00005F4C File Offset: 0x0000414C
		private TextCommandResult ListPlayersInZone(ProtectedZone zone)
		{
			if (this.zoneWhitelistManager == null || this.serverApi == null)
			{
				return TextCommandResult.Error("Zone whitelist system not initialized.", "");
			}
			List<string> players = this.zoneWhitelistManager.GetWhitelistedPlayers(zone.Id);
			if (players.Count == 0)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(42, 2);
				defaultInterpolatedStringHandler.AppendLiteral("No players whitelisted for zone '");
				defaultInterpolatedStringHandler.AppendFormatted(zone.Name);
				defaultInterpolatedStringHandler.AppendLiteral("' (ID: ");
				defaultInterpolatedStringHandler.AppendFormatted<int>(zone.Id);
				defaultInterpolatedStringHandler.AppendLiteral(").");
				return TextCommandResult.Success(defaultInterpolatedStringHandler.ToStringAndClear(), null);
			}
			StringBuilder output = new StringBuilder();
			StringBuilder stringBuilder = output;
			StringBuilder stringBuilder2 = stringBuilder;
			StringBuilder.AppendInterpolatedStringHandler appendInterpolatedStringHandler = new StringBuilder.AppendInterpolatedStringHandler(44, 3, stringBuilder);
			appendInterpolatedStringHandler.AppendLiteral("=== WHITELISTED PLAYERS FOR '");
			appendInterpolatedStringHandler.AppendFormatted(zone.Name);
			appendInterpolatedStringHandler.AppendLiteral("' (ID: ");
			appendInterpolatedStringHandler.AppendFormatted<int>(zone.Id);
			appendInterpolatedStringHandler.AppendLiteral(") (");
			appendInterpolatedStringHandler.AppendFormatted<int>(players.Count);
			appendInterpolatedStringHandler.AppendLiteral(") ===");
			stringBuilder2.AppendLine(ref appendInterpolatedStringHandler);
			foreach (string playerUid in players)
			{
				string playerName = this.GetPlayerNameByUid(playerUid);
				stringBuilder = output;
				StringBuilder stringBuilder3 = stringBuilder;
				appendInterpolatedStringHandler = new StringBuilder.AppendInterpolatedStringHandler(4, 1, stringBuilder);
				appendInterpolatedStringHandler.AppendLiteral("  • ");
				appendInterpolatedStringHandler.AppendFormatted(playerName ?? playerUid);
				stringBuilder3.AppendLine(ref appendInterpolatedStringHandler);
			}
			base.Mod.Logger.Notification(output.ToString());
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(64, 2);
			defaultInterpolatedStringHandler2.AppendLiteral("Listed ");
			defaultInterpolatedStringHandler2.AppendFormatted<int>(players.Count);
			defaultInterpolatedStringHandler2.AppendLiteral(" player(s) for zone '");
			defaultInterpolatedStringHandler2.AppendFormatted(zone.Name);
			defaultInterpolatedStringHandler2.AppendLiteral("'. Check server console for details.");
			return TextCommandResult.Success(defaultInterpolatedStringHandler2.ToStringAndClear(), null);
		}

		// Token: 0x0600003B RID: 59 RVA: 0x00006140 File Offset: 0x00004340
		private TextCommandResult ListZonesForPlayer(string playerName, string playerUid)
		{
			if (this.zoneWhitelistManager == null || this.serverApi == null)
			{
				return TextCommandResult.Error("Zone whitelist system not initialized.", "");
			}
			List<int> zoneIds = this.zoneWhitelistManager.GetWhitelistedZones(playerUid);
			if (zoneIds.Count == 0)
			{
				return TextCommandResult.Success("Player '" + playerName + "' is not whitelisted for any zones.", null);
			}
			GuildManager guildManager = this.guildManager;
			GuildConfig guildConfig;
			if (guildManager == null)
			{
				guildConfig = null;
			}
			else
			{
				GuildConfigManager configManager = guildManager.GetConfigManager();
				guildConfig = ((configManager != null) ? configManager.GetConfig() : null);
			}
			GuildConfig config = guildConfig;
			StringBuilder output = new StringBuilder();
			StringBuilder stringBuilder = output;
			StringBuilder stringBuilder2 = stringBuilder;
			StringBuilder.AppendInterpolatedStringHandler appendInterpolatedStringHandler = new StringBuilder.AppendInterpolatedStringHandler(23, 2, stringBuilder);
			appendInterpolatedStringHandler.AppendLiteral("=== ZONES FOR '");
			appendInterpolatedStringHandler.AppendFormatted(playerName);
			appendInterpolatedStringHandler.AppendLiteral("' (");
			appendInterpolatedStringHandler.AppendFormatted<int>(zoneIds.Count);
			appendInterpolatedStringHandler.AppendLiteral(") ===");
			stringBuilder2.AppendLine(ref appendInterpolatedStringHandler);
			using (IEnumerator<int> enumerator = (from z in zoneIds
			orderby z
			select z).GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					int zoneId = enumerator.Current;
					object obj;
					if (config == null)
					{
						obj = null;
					}
					else
					{
						List<ProtectedZone> protectedZones = config.ProtectedZones;
						obj = ((protectedZones != null) ? protectedZones.FirstOrDefault((ProtectedZone z) => z.Id == zoneId) : null);
					}
					object obj2 = obj;
					string text;
					if ((text = ((obj2 != null) ? obj2.Name : null)) == null)
					{
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(14, 1);
						defaultInterpolatedStringHandler.AppendLiteral("Unknown (ID: ");
						defaultInterpolatedStringHandler.AppendFormatted<int>(zoneId);
						defaultInterpolatedStringHandler.AppendLiteral(")");
						text = defaultInterpolatedStringHandler.ToStringAndClear();
					}
					string zoneName = text;
					stringBuilder = output;
					StringBuilder stringBuilder3 = stringBuilder;
					appendInterpolatedStringHandler = new StringBuilder.AppendInterpolatedStringHandler(11, 2, stringBuilder);
					appendInterpolatedStringHandler.AppendLiteral("  • [ID: ");
					appendInterpolatedStringHandler.AppendFormatted<int>(zoneId);
					appendInterpolatedStringHandler.AppendLiteral("] ");
					appendInterpolatedStringHandler.AppendFormatted(zoneName);
					stringBuilder3.AppendLine(ref appendInterpolatedStringHandler);
				}
			}
			base.Mod.Logger.Notification(output.ToString());
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(72, 2);
			defaultInterpolatedStringHandler2.AppendLiteral("Player '");
			defaultInterpolatedStringHandler2.AppendFormatted(playerName);
			defaultInterpolatedStringHandler2.AppendLiteral("' is whitelisted for ");
			defaultInterpolatedStringHandler2.AppendFormatted<int>(zoneIds.Count);
			defaultInterpolatedStringHandler2.AppendLiteral(" zone(s). Check server console for details.");
			return TextCommandResult.Success(defaultInterpolatedStringHandler2.ToStringAndClear(), null);
		}

		// Token: 0x0600003C RID: 60 RVA: 0x00006398 File Offset: 0x00004598
		[return: Nullable(2)]
		private string GetPlayerNameByUid(string playerUid)
		{
			if (string.IsNullOrEmpty(playerUid) || this.serverApi == null)
			{
				return null;
			}
			IPlayer onlinePlayer = this.serverApi.World.AllOnlinePlayers.FirstOrDefault((IPlayer p) => p.PlayerUID == playerUid);
			if (onlinePlayer != null)
			{
				return onlinePlayer.PlayerName;
			}
			IServerPlayerData playerData;
			if (this.serverApi.PlayerData.PlayerDataByUid.TryGetValue(playerUid, out playerData))
			{
				return playerData.LastKnownPlayername;
			}
			return null;
		}

		// Token: 0x0600003D RID: 61 RVA: 0x0000641C File Offset: 0x0000461C
		private TextCommandResult OnListInventoryCommand(TextCommandCallingArgs args)
		{
			IServerPlayer player = args.Caller.Player as IServerPlayer;
			if (player == null)
			{
				return TextCommandResult.Error("Command can only be used by players.", "");
			}
			StringBuilder output = new StringBuilder();
			output.AppendLine("=== INVENTORY LISTING ===");
			output.AppendLine("Format: [Quantity] ItemCode - ItemName\n");
			Dictionary<string, ValueTuple<int, string>> itemsByCode = new Dictionary<string, ValueTuple<int, string>>();
			int totalSlots = 0;
			int filledSlots = 0;
			IPlayerInventoryManager inventoryManager = player.InventoryManager;
			if (inventoryManager == null)
			{
				return TextCommandResult.Error("Could not access inventory manager.", "");
			}
			StringBuilder stringBuilder;
			StringBuilder.AppendInterpolatedStringHandler appendInterpolatedStringHandler;
			foreach (KeyValuePair<string, IInventory> invPair in inventoryManager.Inventories)
			{
				string invClassName = invPair.Key;
				IInventory inv = invPair.Value;
				if (!(inv.ClassName == "creative"))
				{
					stringBuilder = output;
					StringBuilder stringBuilder2 = stringBuilder;
					appendInterpolatedStringHandler = new StringBuilder.AppendInterpolatedStringHandler(8, 1, stringBuilder);
					appendInterpolatedStringHandler.AppendLiteral("--- ");
					appendInterpolatedStringHandler.AppendFormatted(invClassName);
					appendInterpolatedStringHandler.AppendLiteral(" ---");
					stringBuilder2.AppendLine(ref appendInterpolatedStringHandler);
					for (int i = 0; i < inv.Count; i++)
					{
						totalSlots++;
						ItemSlot slot = inv[i];
						if (!slot.Empty)
						{
							filledSlots++;
							ItemStack itemstack = slot.Itemstack;
							string itemCode = itemstack.Collectible.Code.ToString();
							string itemName = itemstack.GetName();
							int quantity = itemstack.StackSize;
							if (itemsByCode.ContainsKey(itemCode))
							{
								ValueTuple<int, string> existing = itemsByCode[itemCode];
								itemsByCode[itemCode] = new ValueTuple<int, string>(existing.Item1 + quantity, existing.Item2);
							}
							else
							{
								itemsByCode[itemCode] = new ValueTuple<int, string>(quantity, itemName);
							}
							stringBuilder = output;
							StringBuilder stringBuilder3 = stringBuilder;
							appendInterpolatedStringHandler = new StringBuilder.AppendInterpolatedStringHandler(5, 2, stringBuilder);
							appendInterpolatedStringHandler.AppendLiteral("  [");
							appendInterpolatedStringHandler.AppendFormatted<int>(quantity);
							appendInterpolatedStringHandler.AppendLiteral("] ");
							appendInterpolatedStringHandler.AppendFormatted(itemCode);
							stringBuilder3.AppendLine(ref appendInterpolatedStringHandler);
							stringBuilder = output;
							StringBuilder stringBuilder4 = stringBuilder;
							appendInterpolatedStringHandler = new StringBuilder.AppendInterpolatedStringHandler(9, 1, stringBuilder);
							appendInterpolatedStringHandler.AppendLiteral("      └─ ");
							appendInterpolatedStringHandler.AppendFormatted(itemName);
							stringBuilder4.AppendLine(ref appendInterpolatedStringHandler);
						}
					}
					output.AppendLine();
				}
			}
			output.AppendLine("=== UNIQUE ITEMS SUMMARY ===");
			stringBuilder = output;
			StringBuilder stringBuilder5 = stringBuilder;
			appendInterpolatedStringHandler = new StringBuilder.AppendInterpolatedStringHandler(20, 1, stringBuilder);
			appendInterpolatedStringHandler.AppendLiteral("Total unique items: ");
			appendInterpolatedStringHandler.AppendFormatted<int>(itemsByCode.Count);
			stringBuilder5.AppendLine(ref appendInterpolatedStringHandler);
			stringBuilder = output;
			StringBuilder stringBuilder6 = stringBuilder;
			appendInterpolatedStringHandler = new StringBuilder.AppendInterpolatedStringHandler(16, 2, stringBuilder);
			appendInterpolatedStringHandler.AppendLiteral("Filled slots: ");
			appendInterpolatedStringHandler.AppendFormatted<int>(filledSlots);
			appendInterpolatedStringHandler.AppendLiteral("/");
			appendInterpolatedStringHandler.AppendFormatted<int>(totalSlots);
			appendInterpolatedStringHandler.AppendLiteral("\n");
			stringBuilder6.AppendLine(ref appendInterpolatedStringHandler);
			foreach (KeyValuePair<string, ValueTuple<int, string>> kvp in from x in itemsByCode
			orderby x.Key
			select x)
			{
				stringBuilder = output;
				StringBuilder stringBuilder7 = stringBuilder;
				appendInterpolatedStringHandler = new StringBuilder.AppendInterpolatedStringHandler(3, 2, stringBuilder);
				appendInterpolatedStringHandler.AppendLiteral("[");
				appendInterpolatedStringHandler.AppendFormatted<int>(kvp.Value.Item1);
				appendInterpolatedStringHandler.AppendLiteral("] ");
				appendInterpolatedStringHandler.AppendFormatted(kvp.Key);
				stringBuilder7.AppendLine(ref appendInterpolatedStringHandler);
			}
			ICoreServerAPI coreServerAPI = this.serverApi;
			string worldDataPath = (coreServerAPI != null) ? coreServerAPI.GetOrCreateDataPath("ModData") : null;
			if (worldDataPath == null)
			{
				return TextCommandResult.Error("Could not access world data path.", "");
			}
			string logPath = Path.Combine(worldDataPath, "inventory_listing_" + player.PlayerName + ".txt");
			TextCommandResult result;
			try
			{
				File.WriteAllText(logPath, output.ToString());
				player.SendMessage(GlobalConstants.GeneralChatGroup, "Inventory listing written to: " + logPath, 4, null);
				result = TextCommandResult.Success("Inventory listing saved to " + logPath, null);
			}
			catch (Exception ex)
			{
				player.SendMessage(GlobalConstants.GeneralChatGroup, "Error writing inventory listing: " + ex.Message, 4, null);
				result = TextCommandResult.Error("Failed to write inventory listing: " + ex.Message, "");
			}
			return result;
		}

		// Token: 0x0600003E RID: 62 RVA: 0x0000688C File Offset: 0x00004A8C
		private TextCommandResult OnListTraitsCommand(TextCommandCallingArgs args)
		{
			IServerPlayer player = args.Caller.Player as IServerPlayer;
			if (player == null)
			{
				return TextCommandResult.Error("Command can only be used by players.", "");
			}
			TextCommandResult result;
			try
			{
				ICoreServerAPI coreServerAPI = this.serverApi;
				if (((coreServerAPI != null) ? coreServerAPI.ModLoader.GetModSystem<CharacterSystem>(true) : null) == null)
				{
					result = TextCommandResult.Error("CharacterSystem not found. Make sure the character system mod is loaded.", "");
				}
				else
				{
					StringBuilder output = new StringBuilder();
					output.AppendLine("=== YOUR ACTIVE TRAITS ===");
					StringBuilder stringBuilder = output;
					StringBuilder stringBuilder2 = stringBuilder;
					StringBuilder.AppendInterpolatedStringHandler appendInterpolatedStringHandler = new StringBuilder.AppendInterpolatedStringHandler(8, 1, stringBuilder);
					appendInterpolatedStringHandler.AppendLiteral("Player: ");
					appendInterpolatedStringHandler.AppendFormatted(player.PlayerName);
					stringBuilder2.AppendLine(ref appendInterpolatedStringHandler);
					output.AppendLine();
					SyncedTreeAttribute watchedAttributes = player.Entity.WatchedAttributes;
					if (watchedAttributes == null)
					{
						result = TextCommandResult.Error("No attributes", "");
					}
					else
					{
						string[] stringArray = watchedAttributes.GetStringArray("extraTraits", null);
						List<string> traitsList = ((stringArray != null) ? stringArray.ToList<string>() : null) ?? new List<string>();
						if (traitsList == null || traitsList.Count == 0)
						{
							output.AppendLine("No traits found in the character system.");
						}
						else
						{
							Type typeFromHandle = typeof(Trait);
							PropertyInfo codeProperty = typeFromHandle.GetProperty("Code");
							PropertyInfo typeProperty = typeFromHandle.GetProperty("Type");
							HashSet<string> guildTraits = new HashSet<string>();
							foreach (TechBlock tech in this.TechBlocks)
							{
								if (tech.GrantedTraits != null)
								{
									foreach (string trait in tech.GrantedTraits)
									{
										guildTraits.Add(trait);
									}
								}
							}
							output.AppendLine("--- All Traits ---");
							int count = 0;
							foreach (string trait2 in traitsList)
							{
								if (codeProperty != null)
								{
									string code = codeProperty.GetValue(trait2) as string;
									string text;
									if (typeProperty == null)
									{
										text = null;
									}
									else
									{
										object value = typeProperty.GetValue(trait2);
										text = ((value != null) ? value.ToString() : null);
									}
									string type = text ?? "Unknown";
									if (!string.IsNullOrEmpty(code))
									{
										count++;
										string marker = guildTraits.Contains(code) ? " [GUILD]" : "";
										stringBuilder = output;
										StringBuilder stringBuilder3 = stringBuilder;
										appendInterpolatedStringHandler = new StringBuilder.AppendInterpolatedStringHandler(11, 4, stringBuilder);
										appendInterpolatedStringHandler.AppendFormatted<int>(count);
										appendInterpolatedStringHandler.AppendLiteral(". ");
										appendInterpolatedStringHandler.AppendFormatted(code);
										appendInterpolatedStringHandler.AppendLiteral(" (Type: ");
										appendInterpolatedStringHandler.AppendFormatted(type);
										appendInterpolatedStringHandler.AppendLiteral(")");
										appendInterpolatedStringHandler.AppendFormatted(marker);
										stringBuilder3.AppendLine(ref appendInterpolatedStringHandler);
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
								stringBuilder = output;
								StringBuilder stringBuilder4 = stringBuilder;
								appendInterpolatedStringHandler = new StringBuilder.AppendInterpolatedStringHandler(14, 1, stringBuilder);
								appendInterpolatedStringHandler.AppendLiteral("Total traits: ");
								appendInterpolatedStringHandler.AppendFormatted<int>(count);
								stringBuilder4.AppendLine(ref appendInterpolatedStringHandler);
								int guildTraitCount = 0;
								foreach (string trait3 in traitsList)
								{
									if (codeProperty != null)
									{
										string code2 = codeProperty.GetValue(trait3) as string;
										if (!string.IsNullOrEmpty(code2) && guildTraits.Contains(code2))
										{
											guildTraitCount++;
										}
									}
								}
								stringBuilder = output;
								StringBuilder stringBuilder5 = stringBuilder;
								appendInterpolatedStringHandler = new StringBuilder.AppendInterpolatedStringHandler(22, 1, stringBuilder);
								appendInterpolatedStringHandler.AppendLiteral("Guild-granted traits: ");
								appendInterpolatedStringHandler.AppendFormatted<int>(guildTraitCount);
								stringBuilder5.AppendLine(ref appendInterpolatedStringHandler);
							}
						}
						output.AppendLine();
						output.AppendLine("--- Guild Information ---");
						GuildManager guildManager = this.guildManager;
						Guild guild = (guildManager != null) ? guildManager.GetGuildByMember(player.PlayerUID) : null;
						if (guild != null)
						{
							stringBuilder = output;
							StringBuilder stringBuilder6 = stringBuilder;
							appendInterpolatedStringHandler = new StringBuilder.AppendInterpolatedStringHandler(7, 1, stringBuilder);
							appendInterpolatedStringHandler.AppendLiteral("Guild: ");
							appendInterpolatedStringHandler.AppendFormatted(guild.Name);
							stringBuilder6.AppendLine(ref appendInterpolatedStringHandler);
							stringBuilder = output;
							StringBuilder stringBuilder7 = stringBuilder;
							appendInterpolatedStringHandler = new StringBuilder.AppendInterpolatedStringHandler(6, 1, stringBuilder);
							appendInterpolatedStringHandler.AppendLiteral("Role: ");
							appendInterpolatedStringHandler.AppendFormatted(guild.Members[player.PlayerUID].Role);
							stringBuilder7.AppendLine(ref appendInterpolatedStringHandler);
							List<GuildTechProgress> unlockedTechs = (from tp in guild.TechProgress.Values
							where tp.IsUnlocked
							select tp).ToList<GuildTechProgress>();
							stringBuilder = output;
							StringBuilder stringBuilder8 = stringBuilder;
							appendInterpolatedStringHandler = new StringBuilder.AppendInterpolatedStringHandler(23, 1, stringBuilder);
							appendInterpolatedStringHandler.AppendLiteral("Unlocked technologies: ");
							appendInterpolatedStringHandler.AppendFormatted<int>(unlockedTechs.Count);
							stringBuilder8.AppendLine(ref appendInterpolatedStringHandler);
							using (List<GuildTechProgress>.Enumerator enumerator3 = unlockedTechs.GetEnumerator())
							{
								while (enumerator3.MoveNext())
								{
									GuildTechProgress techProgress = enumerator3.Current;
									TechBlock techBlock = this.TechBlocks.FirstOrDefault((TechBlock tb) => tb.Id == techProgress.TechBlockId);
									if (techBlock != null && techBlock.GrantedTraits != null && techBlock.GrantedTraits.Count > 0)
									{
										stringBuilder = output;
										StringBuilder stringBuilder9 = stringBuilder;
										appendInterpolatedStringHandler = new StringBuilder.AppendInterpolatedStringHandler(6, 2, stringBuilder);
										appendInterpolatedStringHandler.AppendLiteral("  • ");
										appendInterpolatedStringHandler.AppendFormatted(techBlock.Text);
										appendInterpolatedStringHandler.AppendLiteral(": ");
										appendInterpolatedStringHandler.AppendFormatted(string.Join(", ", techBlock.GrantedTraits));
										stringBuilder9.AppendLine(ref appendInterpolatedStringHandler);
									}
								}
								goto IL_577;
							}
						}
						output.AppendLine("You are not in a guild.");
						IL_577:
						foreach (string line in output.ToString().Split(new string[]
						{
							Environment.NewLine
						}, StringSplitOptions.None))
						{
							player.SendMessage(GlobalConstants.GeneralChatGroup, line, 0, null);
						}
						result = TextCommandResult.Success("Trait listing complete.", null);
					}
				}
			}
			catch (Exception ex)
			{
				ICoreServerAPI coreServerAPI2 = this.serverApi;
				if (coreServerAPI2 != null)
				{
					coreServerAPI2.Logger.Error("[ListTraits] Error listing traits: " + ex.Message);
				}
				ICoreServerAPI coreServerAPI3 = this.serverApi;
				if (coreServerAPI3 != null)
				{
					coreServerAPI3.Logger.Error("[ListTraits] Stack trace: " + ex.StackTrace);
				}
				result = TextCommandResult.Error("Failed to list traits: " + ex.Message, "");
			}
			return result;
		}

		// Token: 0x0600003F RID: 63 RVA: 0x00006F6C File Offset: 0x0000516C
		private TextCommandResult OnDebugInviteCommand(TextCommandCallingArgs args)
		{
			IServerPlayer player = args.Caller.Player as IServerPlayer;
			if (player == null)
			{
				return TextCommandResult.Error("Command can only be used by players.", "");
			}
			string guildName = args.Parsers[0].GetValue() as string;
			string inviterName = (args.Parsers[1].GetValue() as string) ?? "TestPlayer";
			int? expirySeconds = args.Parsers[2].GetValue() as int?;
			if (string.IsNullOrEmpty(guildName))
			{
				return TextCommandResult.Error("Please specify a guild name.", "");
			}
			int seconds = expirySeconds.GetValueOrDefault(300);
			DateTime expiresAt = DateTime.UtcNow.AddSeconds((double)seconds);
			long expiresAtTicks = expiresAt.Ticks;
			base.Mod.Logger.Notification("[DebugInvite] Creating invite for " + player.PlayerName);
			ILogger logger = base.Mod.Logger;
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(27, 1);
			defaultInterpolatedStringHandler.AppendLiteral("[DebugInvite] Current UTC: ");
			defaultInterpolatedStringHandler.AppendFormatted<DateTime>(DateTime.UtcNow);
			logger.Notification(defaultInterpolatedStringHandler.ToStringAndClear());
			ILogger logger2 = base.Mod.Logger;
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(25, 1);
			defaultInterpolatedStringHandler2.AppendLiteral("[DebugInvite] ExpiresAt: ");
			defaultInterpolatedStringHandler2.AppendFormatted<DateTime>(expiresAt);
			logger2.Notification(defaultInterpolatedStringHandler2.ToStringAndClear());
			ILogger logger3 = base.Mod.Logger;
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler3 = new DefaultInterpolatedStringHandler(30, 1);
			defaultInterpolatedStringHandler3.AppendLiteral("[DebugInvite] ExpiresAtTicks: ");
			defaultInterpolatedStringHandler3.AppendFormatted<long>(expiresAtTicks);
			logger3.Notification(defaultInterpolatedStringHandler3.ToStringAndClear());
			ILogger logger4 = base.Mod.Logger;
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler4 = new DefaultInterpolatedStringHandler(36, 1);
			defaultInterpolatedStringHandler4.AppendLiteral("[DebugInvite] Seconds until expiry: ");
			defaultInterpolatedStringHandler4.AppendFormatted<int>(seconds);
			logger4.Notification(defaultInterpolatedStringHandler4.ToStringAndClear());
			GuildInviteNotificationPacket guildInviteNotificationPacket = new GuildInviteNotificationPacket();
			guildInviteNotificationPacket.PlayerUid = player.PlayerUID;
			guildInviteNotificationPacket.InviterName = inviterName;
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler5 = new DefaultInterpolatedStringHandler(14, 1);
			defaultInterpolatedStringHandler5.AppendLiteral("debug-inviter-");
			defaultInterpolatedStringHandler5.AppendFormatted<Guid>(Guid.NewGuid());
			guildInviteNotificationPacket.InviterUid = defaultInterpolatedStringHandler5.ToStringAndClear();
			guildInviteNotificationPacket.GuildName = guildName;
			guildInviteNotificationPacket.ExpiresAtTicks = expiresAtTicks;
			GuildInviteNotificationPacket inviteNotification = guildInviteNotificationPacket;
			this.serverApi.Network.GetChannel("srguildsandkingdoms:guild").SendPacket<GuildInviteNotificationPacket>(inviteNotification, new IServerPlayer[]
			{
				player
			});
			string text;
			if (seconds >= 60)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler6 = new DefaultInterpolatedStringHandler(8, 1);
				defaultInterpolatedStringHandler6.AppendFormatted<int>(seconds / 60);
				defaultInterpolatedStringHandler6.AppendLiteral(" minutes");
				text = defaultInterpolatedStringHandler6.ToStringAndClear();
			}
			else
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler7 = new DefaultInterpolatedStringHandler(8, 1);
				defaultInterpolatedStringHandler7.AppendFormatted<int>(seconds);
				defaultInterpolatedStringHandler7.AppendLiteral(" seconds");
				text = defaultInterpolatedStringHandler7.ToStringAndClear();
			}
			string expiryText = text;
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler8 = new DefaultInterpolatedStringHandler(50, 3);
			defaultInterpolatedStringHandler8.AppendLiteral("Debug invite sent from '");
			defaultInterpolatedStringHandler8.AppendFormatted(inviterName);
			defaultInterpolatedStringHandler8.AppendLiteral("' to join '");
			defaultInterpolatedStringHandler8.AppendFormatted(guildName);
			defaultInterpolatedStringHandler8.AppendLiteral("' (expires in ");
			defaultInterpolatedStringHandler8.AppendFormatted(expiryText);
			defaultInterpolatedStringHandler8.AppendLiteral(")");
			return TextCommandResult.Success(defaultInterpolatedStringHandler8.ToStringAndClear(), null);
		}

		// Token: 0x06000040 RID: 64 RVA: 0x00007264 File Offset: 0x00005464
		private void ProcessExpiredRepeatingQuests()
		{
			if (this.questRepository == null)
			{
				return;
			}
			try
			{
				try
				{
					ICoreServerAPI coreServerAPI = this.serverApi;
					IGameCalendar calendar = (coreServerAPI != null) ? coreServerAPI.World.Calendar : null;
					GameDate? ingameDate = null;
					if (calendar != null)
					{
						ingameDate = new GameDate?(new GameDate(calendar.Year + 1, calendar.Month, calendar.DayOfYear % calendar.DaysPerMonth + 1));
					}
					int cleanedCount = this.questRepository.CleanupExpiredQuestProgress(null, ingameDate);
					if (cleanedCount > 0)
					{
						ICoreServerAPI coreServerAPI2 = this.serverApi;
						if (coreServerAPI2 != null)
						{
							ILogger logger = coreServerAPI2.Logger;
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(76, 1);
							defaultInterpolatedStringHandler.AppendLiteral("[SRGuildsAndKingdoms:Quests] Cleaned up ");
							defaultInterpolatedStringHandler.AppendFormatted<int>(cleanedCount);
							defaultInterpolatedStringHandler.AppendLiteral(" stale active quest progress entries");
							logger.Notification(defaultInterpolatedStringHandler.ToStringAndClear());
						}
					}
				}
				catch (Exception ex)
				{
					ICoreServerAPI coreServerAPI3 = this.serverApi;
					if (coreServerAPI3 != null)
					{
						coreServerAPI3.Logger.Error("[SRGuildsAndKingdoms:Quests] Failed to cleanup stale quest progress: " + ex.Message);
					}
				}
				DateTime currentDate = QuestTimeHelper.TodayEastern;
				List<Quest> expiredQuests = this.questRepository.GetExpiredRepeatingQuests(new DateTime?(currentDate));
				if (expiredQuests.Count != 0)
				{
					ICoreServerAPI coreServerAPI4 = this.serverApi;
					if (coreServerAPI4 != null)
					{
						ILogger logger2 = coreServerAPI4.Logger;
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(65, 1);
						defaultInterpolatedStringHandler2.AppendLiteral("[SRGuildsAndKingdoms:Quests] Renewing ");
						defaultInterpolatedStringHandler2.AppendFormatted<int>(expiredQuests.Count);
						defaultInterpolatedStringHandler2.AppendLiteral(" expired repeating quest(s)");
						logger2.Notification(defaultInterpolatedStringHandler2.ToStringAndClear());
					}
					foreach (Quest quest in expiredQuests)
					{
						try
						{
							string startsAt = quest.StartsAt;
							string expiresAt = quest.ExpiresAt;
							DateTime oldWeekStartDate = DateTime.Parse(quest.StartsAt);
							DateTime oldWeekExpireDate = DateTime.Parse(quest.ExpiresAt);
							DateTime newWeekStartDate = oldWeekStartDate.AddDays(7.0);
							DateTime newWeekExpireDate = oldWeekExpireDate.AddDays(7.0);
							quest.StartsAt = QuestPeriodKeyGenerator.FormatDate(newWeekStartDate);
							quest.ExpiresAt = QuestPeriodKeyGenerator.FormatDate(newWeekExpireDate);
							if (!this.questRepository.UpdateQuest(quest))
							{
								ICoreServerAPI coreServerAPI5 = this.serverApi;
								if (coreServerAPI5 != null)
								{
									ILogger logger3 = coreServerAPI5.Logger;
									DefaultInterpolatedStringHandler defaultInterpolatedStringHandler3 = new DefaultInterpolatedStringHandler(66, 2);
									defaultInterpolatedStringHandler3.AppendLiteral("[SRGuildsAndKingdoms:Quests] Failed to renew quest ");
									defaultInterpolatedStringHandler3.AppendFormatted<int>(quest.Id);
									defaultInterpolatedStringHandler3.AppendLiteral(" '");
									defaultInterpolatedStringHandler3.AppendFormatted(quest.Title);
									defaultInterpolatedStringHandler3.AppendLiteral("' in database");
									logger3.Error(defaultInterpolatedStringHandler3.ToStringAndClear());
								}
							}
						}
						catch (Exception ex2)
						{
							ICoreServerAPI coreServerAPI6 = this.serverApi;
							if (coreServerAPI6 != null)
							{
								ILogger logger4 = coreServerAPI6.Logger;
								DefaultInterpolatedStringHandler defaultInterpolatedStringHandler4 = new DefaultInterpolatedStringHandler(56, 3);
								defaultInterpolatedStringHandler4.AppendLiteral("[SRGuildsAndKingdoms:Quests] Failed to renew quest ");
								defaultInterpolatedStringHandler4.AppendFormatted<int>(quest.Id);
								defaultInterpolatedStringHandler4.AppendLiteral(" '");
								defaultInterpolatedStringHandler4.AppendFormatted(quest.Title);
								defaultInterpolatedStringHandler4.AppendLiteral("': ");
								defaultInterpolatedStringHandler4.AppendFormatted(ex2.Message);
								logger4.Error(defaultInterpolatedStringHandler4.ToStringAndClear());
							}
						}
					}
				}
			}
			catch (Exception ex3)
			{
				ICoreServerAPI coreServerAPI7 = this.serverApi;
				if (coreServerAPI7 != null)
				{
					coreServerAPI7.Logger.Error("[SRGuildsAndKingdoms:Quests] Failed to process expired repeating quests: " + ex3.Message);
				}
			}
		}

		// Token: 0x06000041 RID: 65 RVA: 0x000075EC File Offset: 0x000057EC
		private void OnSaveGameLoaded()
		{
			this.guildManager.OnSaveGameLoading();
			ZoneWhitelistManager zoneWhitelistManager = this.zoneWhitelistManager;
			if (zoneWhitelistManager != null)
			{
				zoneWhitelistManager.Load();
			}
			this.networkHandler.BroadcastGuildSummariesToAll();
			this.networkHandler.BroadcastGuildConfigToAll();
		}

		// Token: 0x06000042 RID: 66 RVA: 0x00007620 File Offset: 0x00005820
		private void OnSaveGameSaving()
		{
			this.guildManager.OnSaveGameSaving();
			this.ProcessExpiredRepeatingQuests();
			try
			{
				GuildDatabase guildDatabase = this.guildDatabase;
				if (guildDatabase != null)
				{
					guildDatabase.Checkpoint();
				}
				ICoreServerAPI coreServerAPI = this.serverApi;
				if (coreServerAPI != null)
				{
					coreServerAPI.Logger.Debug("[SRGuildsAndKingdoms:Database] Database checkpoint complete");
				}
			}
			catch (Exception ex)
			{
				ICoreServerAPI coreServerAPI2 = this.serverApi;
				if (coreServerAPI2 != null)
				{
					coreServerAPI2.Logger.Error("[SRGuildsAndKingdoms:Database] Database checkpoint failed: " + ex.Message);
				}
			}
			this.CreateGuildBackup();
		}

		// Token: 0x06000043 RID: 67 RVA: 0x000076AC File Offset: 0x000058AC
		private void CreateGuildBackup()
		{
			try
			{
				string backupDir = Path.Combine(this.serverApi.GetOrCreateDataPath("ModData/SRGuildsAndKingdoms"), "backups");
				Directory.CreateDirectory(backupDir);
				string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
				string dbPath = Path.Combine(this.serverApi.GetOrCreateDataPath("ModData/SRGuildsAndKingdoms"), "guilds.db");
				if (File.Exists(dbPath))
				{
					string dbBackupPath = Path.Combine(backupDir, "guilds_" + timestamp + ".db");
					File.Copy(dbPath, dbBackupPath, true);
					base.Mod.Logger.Debug("[SRGuildsAndKingdoms:Backup] Database backed up to: " + dbBackupPath);
					string walPath = dbPath + "-wal";
					if (File.Exists(walPath))
					{
						string walBackupPath = Path.Combine(backupDir, "guilds_" + timestamp + ".db-wal");
						File.Copy(walPath, walBackupPath, true);
						base.Mod.Logger.Debug("[SRGuildsAndKingdoms:Backup] WAL file backed up to: " + walBackupPath);
					}
					string shmPath = dbPath + "-shm";
					if (File.Exists(shmPath))
					{
						string shmBackupPath = Path.Combine(backupDir, "guilds_" + timestamp + ".db-shm");
						File.Copy(shmPath, shmBackupPath, true);
						base.Mod.Logger.Debug("[SRGuildsAndKingdoms:Backup] SHM file backed up to: " + shmBackupPath);
					}
				}
				this.CleanupOldBackups(backupDir, 10);
			}
			catch (Exception ex)
			{
				base.Mod.Logger.Error("[SRGuildsAndKingdoms:Backup] Failed to create database backup: " + ex.Message);
			}
		}

		// Token: 0x06000044 RID: 68 RVA: 0x00007848 File Offset: 0x00005A48
		private void CleanupOldBackups(string backupDir, int maxBackupsToKeep)
		{
			try
			{
				this.ProcessBackupType(backupDir, "guilds_*.db", "guilds", maxBackupsToKeep);
				this.ProcessBackupType(backupDir, "guilds_*.db-wal", "guilds_wal", maxBackupsToKeep);
				this.ProcessBackupType(backupDir, "guilds_*.db-shm", "guilds_shm", maxBackupsToKeep);
			}
			catch (Exception ex)
			{
				base.Mod.Logger.Warning("[SRGuildsAndKingdoms:Backup] Failed to cleanup old backups: " + ex.Message);
			}
		}

		// Token: 0x06000045 RID: 69 RVA: 0x000078C0 File Offset: 0x00005AC0
		private void ProcessBackupType(string backupDir, string searchPattern, string backupType, int maxBackupsToKeep)
		{
			try
			{
				List<FileInfo> allBackups = (from f in Directory.GetFiles(backupDir, searchPattern)
				select new FileInfo(f) into f
				orderby f.CreationTime descending
				select f).ToList<FileInfo>();
				if (allBackups.Count > maxBackupsToKeep)
				{
					allBackups.Take(maxBackupsToKeep).ToList<FileInfo>();
					List<FileInfo> oldBackups = allBackups.Skip(maxBackupsToKeep).ToList<FileInfo>();
					if (oldBackups.Count != 0)
					{
						List<IGrouping<DateTime, FileInfo>> backupsByDate = (from f in oldBackups
						group f by f.CreationTime.Date into g
						orderby g.Key
						select g).ToList<IGrouping<DateTime, FileInfo>>();
						ILogger logger = base.Mod.Logger;
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(66, 3);
						defaultInterpolatedStringHandler.AppendLiteral("[SRGuildsAndKingdoms:Backup] Processing ");
						defaultInterpolatedStringHandler.AppendFormatted<int>(oldBackups.Count);
						defaultInterpolatedStringHandler.AppendLiteral(" old ");
						defaultInterpolatedStringHandler.AppendFormatted(backupType);
						defaultInterpolatedStringHandler.AppendLiteral(" backups across ");
						defaultInterpolatedStringHandler.AppendFormatted<int>(backupsByDate.Count);
						defaultInterpolatedStringHandler.AppendLiteral(" days");
						logger.Debug(defaultInterpolatedStringHandler.ToStringAndClear());
						foreach (IGrouping<DateTime, FileInfo> dailyGroup in backupsByDate)
						{
							string dateString = dailyGroup.Key.ToString("yyyy-MM-dd");
							string zipFileName = Path.Combine(backupDir, backupType + "_archive_" + dateString + ".zip");
							if (File.Exists(zipFileName))
							{
								using (ZipArchive archive = ZipFile.Open(zipFileName, ZipArchiveMode.Update))
								{
									foreach (FileInfo file in dailyGroup)
									{
										string entryName = Path.GetFileName(file.FullName);
										if (archive.GetEntry(entryName) == null)
										{
											archive.CreateEntryFromFile(file.FullName, entryName, CompressionLevel.Optimal);
											base.Mod.Logger.Debug("[SRGuildsAndKingdoms:Backup] Added " + entryName + " to existing archive " + dateString);
										}
										file.Delete();
									}
									continue;
								}
							}
							using (ZipArchive archive2 = ZipFile.Open(zipFileName, ZipArchiveMode.Create))
							{
								foreach (FileInfo file2 in dailyGroup)
								{
									string entryName2 = Path.GetFileName(file2.FullName);
									archive2.CreateEntryFromFile(file2.FullName, entryName2, CompressionLevel.Optimal);
									file2.Delete();
								}
							}
							ILogger logger2 = base.Mod.Logger;
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(66, 3);
							defaultInterpolatedStringHandler2.AppendLiteral("[SRGuildsAndKingdoms:Backup] Created ");
							defaultInterpolatedStringHandler2.AppendFormatted(backupType);
							defaultInterpolatedStringHandler2.AppendLiteral(" archive for ");
							defaultInterpolatedStringHandler2.AppendFormatted(dateString);
							defaultInterpolatedStringHandler2.AppendLiteral(" with ");
							defaultInterpolatedStringHandler2.AppendFormatted<int>(dailyGroup.Count<FileInfo>());
							defaultInterpolatedStringHandler2.AppendLiteral(" backup(s)");
							logger2.Debug(defaultInterpolatedStringHandler2.ToStringAndClear());
						}
						this.CleanupOldZipArchives(backupDir, backupType, 60);
					}
				}
			}
			catch (Exception ex)
			{
				base.Mod.Logger.Warning("[SRGuildsAndKingdoms:Backup] Failed to process " + backupType + " backups: " + ex.Message);
			}
		}

		// Token: 0x06000046 RID: 70 RVA: 0x00007CCC File Offset: 0x00005ECC
		private void CleanupOldZipArchives(string backupDir, string backupType, int daysToKeep)
		{
			try
			{
				DateTime cutoffDate = DateTime.Now.AddDays((double)(-(double)daysToKeep));
				string archivePattern = backupType + "_archive_*.zip";
				List<string> oldArchives = (from f in Directory.GetFiles(backupDir, archivePattern)
				where File.GetCreationTime(f) < cutoffDate
				select f).ToList<string>();
				foreach (string archive in oldArchives)
				{
					File.Delete(archive);
					base.Mod.Logger.Debug("[SRGuildsAndKingdoms:Backup] Deleted old archive: " + Path.GetFileName(archive));
				}
				if (oldArchives.Count > 0)
				{
					ILogger logger = base.Mod.Logger;
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(66, 3);
					defaultInterpolatedStringHandler.AppendLiteral("[SRGuildsAndKingdoms:Backup] Deleted ");
					defaultInterpolatedStringHandler.AppendFormatted<int>(oldArchives.Count);
					defaultInterpolatedStringHandler.AppendLiteral(" ");
					defaultInterpolatedStringHandler.AppendFormatted(backupType);
					defaultInterpolatedStringHandler.AppendLiteral(" archive(s) older than ");
					defaultInterpolatedStringHandler.AppendFormatted<int>(daysToKeep);
					defaultInterpolatedStringHandler.AppendLiteral(" days");
					logger.Notification(defaultInterpolatedStringHandler.ToStringAndClear());
				}
			}
			catch (Exception ex)
			{
				base.Mod.Logger.Warning("[SRGuildsAndKingdoms:Backup] Failed to cleanup old " + backupType + " archives: " + ex.Message);
			}
		}

		// Token: 0x06000047 RID: 71 RVA: 0x00007E4C File Offset: 0x0000604C
		private void OnEntityDeath(Entity entity, DamageSource damageSource)
		{
			EntityPlayer killerPlayer = ((damageSource != null) ? damageSource.GetCauseEntity() : null) as EntityPlayer;
			if (killerPlayer == null)
			{
				return;
			}
			IServerPlayer killerServerPlayer = killerPlayer.Player as IServerPlayer;
			if (killerServerPlayer == null || this.guildManager == null || this.questRepository == null)
			{
				return;
			}
			string killerUid = killerServerPlayer.PlayerUID;
			if (this.guildManager.GetGuildByMember(killerUid) == null)
			{
				return;
			}
			AssetLocation code = entity.Code;
			string entityCode = (code != null) ? code.ToString() : null;
			if (string.IsNullOrEmpty(entityCode))
			{
				return;
			}
			IGameCalendar calendar = this.serverApi.World.Calendar;
			GameDate ingameDate = new GameDate(calendar.Year + 1, calendar.Month, calendar.DayOfYear % calendar.DaysPerMonth + 1);
			List<PlayerQuestProgress> activeQuests = this.questRepository.GetPlayerActiveQuests(killerUid, new GameDate?(ingameDate));
			if (activeQuests.Count == 0)
			{
				return;
			}
			bool progressUpdated = false;
			foreach (PlayerQuestProgress questProgress in activeQuests)
			{
				Quest quest = this.questRepository.GetQuest(questProgress.QuestId);
				if (quest != null)
				{
					foreach (QuestObjective objective in quest.Objectives)
					{
						if (objective.Type.Equals("kill", StringComparison.OrdinalIgnoreCase))
						{
							int currentProgress = questProgress.GetObjectiveProgress(objective.Id);
							if (currentProgress < objective.Count && objective.AcceptedTargets != null && objective.AcceptedTargets.Count != 0)
							{
								bool matches = false;
								foreach (string targetPattern in objective.AcceptedTargets)
								{
									if (targetPattern.EndsWith("*"))
									{
										string prefix = targetPattern.Substring(0, targetPattern.Length - 1);
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
								if (matches && questProgress.AddObjectiveProgress(objective.Id, 1, objective.Count) > 0)
								{
									progressUpdated = true;
									ICoreServerAPI coreServerAPI = this.serverApi;
									if (coreServerAPI != null)
									{
										ILogger logger = coreServerAPI.Logger;
										DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(61, 6);
										defaultInterpolatedStringHandler.AppendLiteral("[QuestTracker] Player ");
										defaultInterpolatedStringHandler.AppendFormatted(killerServerPlayer.PlayerName);
										defaultInterpolatedStringHandler.AppendLiteral(" killed ");
										defaultInterpolatedStringHandler.AppendFormatted(entityCode);
										defaultInterpolatedStringHandler.AppendLiteral(", quest ");
										defaultInterpolatedStringHandler.AppendFormatted(quest.Title);
										defaultInterpolatedStringHandler.AppendLiteral(" objective ");
										defaultInterpolatedStringHandler.AppendFormatted<int>(objective.Id);
										defaultInterpolatedStringHandler.AppendLiteral(" progress: ");
										defaultInterpolatedStringHandler.AppendFormatted<int>(currentProgress + 1);
										defaultInterpolatedStringHandler.AppendLiteral("/");
										defaultInterpolatedStringHandler.AppendFormatted<int>(objective.Count);
										logger.Debug(defaultInterpolatedStringHandler.ToStringAndClear());
									}
								}
							}
						}
					}
					if (progressUpdated)
					{
						this.questRepository.UpdatePlayerQuestProgress(questProgress);
					}
				}
			}
		}

		// Token: 0x06000048 RID: 72 RVA: 0x000081AC File Offset: 0x000063AC
		[NullableContext(2)]
		private Guild GetChunkOwningGuild(int chunkX, int chunkZ)
		{
			if (this.landClaimRepository == null || this.guildManager == null)
			{
				return null;
			}
			string owningGuildName = this.landClaimRepository.GetGuildOwningChunk(chunkX, chunkZ);
			if (owningGuildName == null)
			{
				return null;
			}
			return this.guildManager.GetGuild(owningGuildName);
		}

		// Token: 0x06000049 RID: 73 RVA: 0x000081EC File Offset: 0x000063EC
		public bool IsChunkAdjacentToGuildClaims(string guildName, int chunkX, int chunkZ)
		{
			if (this.guildManager == null)
			{
				return false;
			}
			Guild guild = this.guildManager.GetGuild(guildName);
			if (guild == null || guild.Claims.Count == 0)
			{
				return false;
			}
			foreach (LandClaim landClaim in guild.Claims)
			{
				int deltaX = Math.Abs(landClaim.ChunkX - chunkX);
				int deltaZ = Math.Abs(landClaim.ChunkZ - chunkZ);
				if ((deltaX == 1 && deltaZ == 0) || (deltaX == 0 && deltaZ == 1))
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x0600004A RID: 74 RVA: 0x00008294 File Offset: 0x00006494
		private bool HasPermissionLocal(Guild guild, string playerUid, GuildPermission permission)
		{
			if (guild == null || string.IsNullOrEmpty(playerUid) || !guild.Members.ContainsKey(playerUid))
			{
				return false;
			}
			string roleName = guild.Members[playerUid].Role;
			GuildRole role;
			return guild.Roles.TryGetValue(roleName, out role) && (role.Permissions & permission) == permission;
		}

		// Token: 0x0600004B RID: 75 RVA: 0x000082EC File Offset: 0x000064EC
		private GuildPermission ParsePermissionString(string perms)
		{
			if (string.IsNullOrWhiteSpace(perms))
			{
				return GuildPermission.None;
			}
			GuildPermission result = GuildPermission.None;
			string[] array = perms.Split(new char[]
			{
				',',
				';',
				' '
			}, StringSplitOptions.RemoveEmptyEntries);
			for (int i = 0; i < array.Length; i++)
			{
				string tok = array[i].Trim().ToLowerInvariant();
				if (tok != null)
				{
					switch (tok.Length)
					{
					case 4:
						if (!(tok == "kick"))
						{
							goto IL_187;
						}
						result |= GuildPermission.Kick;
						goto IL_187;
					case 5:
					case 9:
					case 12:
					case 13:
					case 15:
						goto IL_187;
					case 6:
					{
						char c = tok[0];
						if (c != 'i')
						{
							if (c != 'm')
							{
								goto IL_187;
							}
							if (!(tok == "manage"))
							{
								goto IL_187;
							}
						}
						else
						{
							if (!(tok == "invite"))
							{
								goto IL_187;
							}
							result |= GuildPermission.Invite;
							goto IL_187;
						}
						break;
					}
					case 7:
						if (!(tok == "promote"))
						{
							goto IL_187;
						}
						result |= GuildPermission.Promote;
						goto IL_187;
					case 8:
						if (!(tok == "interact"))
						{
							goto IL_187;
						}
						goto IL_182;
					case 10:
					{
						char c = tok[0];
						if (c != 'b')
						{
							if (c != 'm')
							{
								goto IL_187;
							}
							if (!(tok == "managerole"))
							{
								goto IL_187;
							}
						}
						else
						{
							if (!(tok == "breakplace"))
							{
								goto IL_187;
							}
							goto IL_17B;
						}
						break;
					}
					case 11:
						if (!(tok == "manageroles"))
						{
							goto IL_187;
						}
						break;
					case 14:
						if (!(tok == "interactblocks"))
						{
							goto IL_187;
						}
						goto IL_182;
					case 16:
						if (!(tok == "breakplaceblocks"))
						{
							goto IL_187;
						}
						goto IL_17B;
					default:
						goto IL_187;
					}
					result |= GuildPermission.ManageRoles;
					goto IL_187;
					IL_17B:
					result |= GuildPermission.BreakAndPlaceBlocks;
					goto IL_187;
					IL_182:
					result |= GuildPermission.InteractBlocks;
				}
				IL_187:;
			}
			return result;
		}

		// Token: 0x0600004C RID: 76 RVA: 0x00008490 File Offset: 0x00006690
		public override void StartClientSide(ICoreClientAPI api)
		{
			this.clientApi = api;
			this.worldMapManager = api.ModLoader.GetModSystem<WorldMapManager>(true);
			this.worldMapManager.RegisterMapLayer<PlotMapLayer>("guildclaims", 1.0);
			this.networkHandler.InitializeClient(api, new Action<string, NotificationType>(this.OnNotificationReceived), new Action<List<GuildSummary>>(this.OnGuildSummariesReceived));
			this.networkHandler.RegisterConfigCallback(new Action<GuildConfigPacket>(this.OnGuildConfigReceived));
			this.networkHandler.RegisterTechBlocksConfigCallback(new Action<TechBlocksConfigSyncPacket>(this.OnTechBlocksConfigReceived));
			QuestNetworkHandler questNetworkHandler = this.questNetworkHandler;
			if (questNetworkHandler != null)
			{
				questNetworkHandler.InitializeClient(api);
			}
			if (this.questNetworkHandler != null)
			{
				QuestNetworkHandler questNetworkHandler2 = this.questNetworkHandler;
				questNetworkHandler2.OnOpenQuestManager = (Action)Delegate.Combine(questNetworkHandler2.OnOpenQuestManager, new Action(this.OnOpenQuestManagerDialog));
			}
			api.Input.RegisterHotKey("openguild", "Open Guild Dialog", 94, 2, false, true, false);
			api.Input.SetHotKeyHandler("openguild", new ActionConsumable<KeyCombination>(this.OnOpenGuildDialog));
			api.Input.RegisterHotKey("toggleclaimhologram", "Toggle Guild Claim Hologram", 98, 2, false, false, false);
			api.Input.SetHotKeyHandler("toggleclaimhologram", new ActionConsumable<KeyCombination>(this.OnToggleHologram));
			this.invitePopup = new DialogGuildInvitePopup(api, this);
		}

		// Token: 0x0600004D RID: 77 RVA: 0x000085E0 File Offset: 0x000067E0
		private bool OnOpenGuildDialog(KeyCombination comb)
		{
			if (this.clientApi == null)
			{
				return false;
			}
			if (this.guildDialog != null && this.guildDialog.IsOpened())
			{
				this.guildDialog.TryClose();
				return true;
			}
			this.TryApplyPendingConfig();
			this.guildDialog = new DialogGuildMain(this.clientApi, this);
			this.guildDialog.TryOpen();
			return true;
		}

		// Token: 0x0600004E RID: 78 RVA: 0x00008640 File Offset: 0x00006840
		private void OnOpenQuestManagerDialog()
		{
			if (this.clientApi == null)
			{
				return;
			}
			if (this.questManagerDialog != null && this.questManagerDialog.IsOpened())
			{
				this.questManagerDialog.TryClose();
			}
			this.questManagerDialog = new DialogQuestManager(this.clientApi, this);
			this.questManagerDialog.TryOpen();
		}

		// Token: 0x0600004F RID: 79 RVA: 0x00008695 File Offset: 0x00006895
		private bool OnToggleHologram(KeyCombination keyCombination)
		{
			this.ToggleHologram();
			return true;
		}

		// Token: 0x06000050 RID: 80 RVA: 0x000086A0 File Offset: 0x000068A0
		public void ToggleHologram()
		{
			this.hologramVisible = !this.hologramVisible;
			if (this.hologramVisible)
			{
				this.ShowClaimsHologram();
				ICoreClientAPI coreClientAPI = this.clientApi;
				if (coreClientAPI == null)
				{
					return;
				}
				coreClientAPI.ShowChatMessage(Lang.Get("srguildsandkingdoms:hologram-shown", Array.Empty<object>()));
				return;
			}
			else
			{
				this.ClearHologram();
				ICoreClientAPI coreClientAPI2 = this.clientApi;
				if (coreClientAPI2 == null)
				{
					return;
				}
				coreClientAPI2.ShowChatMessage(Lang.Get("srguildsandkingdoms:hologram-hidden", Array.Empty<object>()));
				return;
			}
		}

		// Token: 0x06000051 RID: 81 RVA: 0x00008710 File Offset: 0x00006910
		public void ShowClaimsHologram()
		{
			if (this.clientApi == null)
			{
				return;
			}
			GuildSummary currentGuild = this.GetCurrentPlayerGuildSummary();
			if (currentGuild == null || currentGuild.Claims.Count == 0)
			{
				if (this.hologramVisible)
				{
					this.hologramVisible = false;
					this.clientApi.ShowChatMessage(Lang.Get("srguildsandkingdoms:hologram-no-claims", Array.Empty<object>()));
				}
				return;
			}
			List<BlockPos> blockPositions = new List<BlockPos>();
			IClientPlayer player = this.clientApi.World.Player;
			if (player == null)
			{
				return;
			}
			int playerY = (int)player.Entity.Pos.Y;
			int minY = Math.Max(0, playerY - 50);
			int maxY = Math.Min(this.clientApi.World.BlockAccessor.MapSizeY - 1, playerY + 50);
			HashSet<ValueTuple<int, int>> claimedChunks = new HashSet<ValueTuple<int, int>>(from c in currentGuild.Claims
			select new ValueTuple<int, int>(c.ChunkX, c.ChunkZ));
			foreach (LandClaimDto landClaimDto in currentGuild.Claims)
			{
				int chunkX = landClaimDto.ChunkX;
				int chunkZ = landClaimDto.ChunkZ;
				int minBlockX = chunkX * 32;
				int maxBlockX = minBlockX + 31;
				int minBlockZ = chunkZ * 32;
				int maxBlockZ = minBlockZ + 31;
				if (!claimedChunks.Contains(new ValueTuple<int, int>(chunkX, chunkZ + 1)))
				{
					for (int x = minBlockX; x <= maxBlockX; x++)
					{
						for (int y = minY; y <= maxY; y++)
						{
							blockPositions.Add(new BlockPos(x, y, maxBlockZ));
						}
					}
				}
				if (!claimedChunks.Contains(new ValueTuple<int, int>(chunkX, chunkZ - 1)))
				{
					for (int x2 = minBlockX; x2 <= maxBlockX; x2++)
					{
						for (int y2 = minY; y2 <= maxY; y2++)
						{
							blockPositions.Add(new BlockPos(x2, y2, minBlockZ));
						}
					}
				}
				if (!claimedChunks.Contains(new ValueTuple<int, int>(chunkX + 1, chunkZ)))
				{
					for (int z = minBlockZ; z <= maxBlockZ; z++)
					{
						for (int y3 = minY; y3 <= maxY; y3++)
						{
							blockPositions.Add(new BlockPos(maxBlockX, y3, z));
						}
					}
				}
				if (!claimedChunks.Contains(new ValueTuple<int, int>(chunkX - 1, chunkZ)))
				{
					for (int z2 = minBlockZ; z2 <= maxBlockZ; z2++)
					{
						for (int y4 = minY; y4 <= maxY; y4++)
						{
							blockPositions.Add(new BlockPos(minBlockX, y4, z2));
						}
					}
				}
			}
			this.clientApi.World.HighlightBlocks(player, 99, blockPositions, 0, 0);
		}

		// Token: 0x06000052 RID: 82 RVA: 0x000089AC File Offset: 0x00006BAC
		public void ClearHologram()
		{
			ICoreClientAPI coreClientAPI = this.clientApi;
			if (coreClientAPI != null)
			{
				coreClientAPI.World.HighlightBlocks(this.clientApi.World.Player, 99, new List<BlockPos>(), 0, 0);
			}
			this.hologramVisible = false;
		}

		// Token: 0x06000053 RID: 83 RVA: 0x000089E4 File Offset: 0x00006BE4
		private void OnNotificationReceived(string message, NotificationType type)
		{
			if (type == NotificationType.Error && message.Contains("Claims are restricted to within"))
			{
				this.TryExtractTerritorialSettingsFromMessage(message);
			}
			string text;
			switch (type)
			{
			case NotificationType.Success:
				text = "[Guild]";
				break;
			case NotificationType.Warning:
				text = "[Guild]";
				break;
			case NotificationType.Error:
				text = "[Guild]";
				break;
			default:
				text = "[Guild]";
				break;
			}
			string prefix = text;
			this.clientApi.ShowChatMessage(prefix + message);
		}

		// Token: 0x06000054 RID: 84 RVA: 0x00008A54 File Offset: 0x00006C54
		private void TryExtractTerritorialSettingsFromMessage(string message)
		{
			try
			{
				string pattern = "Claims are restricted to within (\\d+) blocks of \\((-?\\d+), (-?\\d+)\\)";
				Match match = Regex.Match(message, pattern);
				if (match.Success)
				{
					int radius = int.Parse(match.Groups[1].Value);
					int centerX = int.Parse(match.Groups[2].Value);
					int centerZ = int.Parse(match.Groups[3].Value);
					ILogger logger = base.Mod.Logger;
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(72, 3);
					defaultInterpolatedStringHandler.AppendLiteral("Extracted territorial settings from server message: Center (");
					defaultInterpolatedStringHandler.AppendFormatted<int>(centerX);
					defaultInterpolatedStringHandler.AppendLiteral(", ");
					defaultInterpolatedStringHandler.AppendFormatted<int>(centerZ);
					defaultInterpolatedStringHandler.AppendLiteral("), Radius ");
					defaultInterpolatedStringHandler.AppendFormatted<int>(radius);
					logger.Debug(defaultInterpolatedStringHandler.ToStringAndClear());
				}
			}
			catch (Exception ex)
			{
				base.Mod.Logger.Warning("Failed to extract territorial settings from message: " + ex.Message);
			}
		}

		// Token: 0x06000055 RID: 85 RVA: 0x00008B58 File Offset: 0x00006D58
		private void OnGuildSummariesReceived(List<GuildSummary> summaries)
		{
			this.clientGuildSummaries.Clear();
			this.clientGuildSummaries.AddRange(summaries);
			if (this.clientApi != null)
			{
				GuildSummary playerGuild = summaries.FirstOrDefault((GuildSummary g) => g.IsPlayerMember);
				if (playerGuild != null)
				{
					base.Mod.Logger.Notification("[SRGuildsAndKingdoms:GuildSync] Received guild summaries - Player is member of: " + playerGuild.Name);
				}
				else
				{
					ILogger logger = base.Mod.Logger;
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(86, 1);
					defaultInterpolatedStringHandler.AppendLiteral("[SRGuildsAndKingdoms:GuildSync] Received ");
					defaultInterpolatedStringHandler.AppendFormatted<int>(summaries.Count);
					defaultInterpolatedStringHandler.AppendLiteral(" guild summaries - Player is not in any guild");
					logger.Debug(defaultInterpolatedStringHandler.ToStringAndClear());
				}
			}
			Action<List<GuildSummary>> onClientGuildDataUpdated = this.OnClientGuildDataUpdated;
			if (onClientGuildDataUpdated == null)
			{
				return;
			}
			onClientGuildDataUpdated(summaries);
		}

		// Token: 0x06000056 RID: 86 RVA: 0x00008C2C File Offset: 0x00006E2C
		private void OnGuildConfigReceived(GuildConfigPacket config)
		{
			if (this.techBlocksConfig != null && config.EnabledAges != null)
			{
				this.techBlocksConfig.EnabledAges = (from age in config.EnabledAges
				select (TechAge)age).ToList<TechAge>();
				ILogger logger = base.Mod.Logger;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(39, 2);
				defaultInterpolatedStringHandler.AppendLiteral("Synced ");
				defaultInterpolatedStringHandler.AppendFormatted<int>(config.EnabledAges.Count);
				defaultInterpolatedStringHandler.AppendLiteral(" enabled tech ages from server: ");
				defaultInterpolatedStringHandler.AppendFormatted(string.Join<TechAge>(", ", this.techBlocksConfig.EnabledAges));
				logger.Notification(defaultInterpolatedStringHandler.ToStringAndClear());
			}
			if (this.plotLayer != null)
			{
				this.plotLayer.UpdateConfigFromServer(config);
				this.pendingConfigPacket = null;
				ILogger logger2 = base.Mod.Logger;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(86, 3);
				defaultInterpolatedStringHandler2.AppendLiteral("Successfully updated PlotMapLayer with config: Territorial=");
				defaultInterpolatedStringHandler2.AppendFormatted<bool>(config.TerritorialRestrictionsEnabled);
				defaultInterpolatedStringHandler2.AppendLiteral(", Protected Zones=");
				defaultInterpolatedStringHandler2.AppendFormatted<bool>(config.ProtectedZonesEnabled);
				defaultInterpolatedStringHandler2.AppendLiteral(" (");
				List<ProtectedZoneData> protectedZones = config.ProtectedZones;
				defaultInterpolatedStringHandler2.AppendFormatted<int>((protectedZones != null) ? protectedZones.Count : 0);
				defaultInterpolatedStringHandler2.AppendLiteral(" zones)");
				logger2.Debug(defaultInterpolatedStringHandler2.ToStringAndClear());
				return;
			}
			this.pendingConfigPacket = config;
			base.Mod.Logger.Warning("PlotMapLayer not registered yet - caching config for later application.");
		}

		// Token: 0x06000057 RID: 87 RVA: 0x00008DB0 File Offset: 0x00006FB0
		private void OnTechBlocksConfigReceived(TechBlocksConfigSyncPacket packet)
		{
			if (this.clientApi == null)
			{
				base.Mod.Logger.Warning("OnTechBlocksConfigReceived called but clientApi is null");
				return;
			}
			try
			{
				TechBlocksConfig.SaveServerConfig(this.clientApi, packet.ConfigJson, packet.ServerIdentifier, "techblocks.json");
				this.techBlocksConfig = TechBlocksConfig.LoadFromFile(this.clientApi, "techblocks.json", packet.ServerIdentifier);
				base.Mod.Logger.Notification("Tech blocks config synced from server (identifier: " + packet.ServerIdentifier + ")");
				ILogger logger = base.Mod.Logger;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(34, 2);
				defaultInterpolatedStringHandler.AppendLiteral("Loaded ");
				defaultInterpolatedStringHandler.AppendFormatted<int>(this.techBlocksConfig.TechBlocks.Count);
				defaultInterpolatedStringHandler.AppendLiteral(" tech blocks, ");
				defaultInterpolatedStringHandler.AppendFormatted<int>(this.techBlocksConfig.EnabledAges.Count);
				defaultInterpolatedStringHandler.AppendLiteral(" enabled ages");
				logger.Notification(defaultInterpolatedStringHandler.ToStringAndClear());
				this.ApplyAgeRestrictionsToBlocks(this.clientApi);
			}
			catch (Exception ex)
			{
				base.Mod.Logger.Error("Failed to process tech blocks config from server: " + ex.Message);
				base.Mod.Logger.Error("Stack trace: " + ex.StackTrace);
			}
		}

		// Token: 0x06000058 RID: 88 RVA: 0x00008F0C File Offset: 0x0000710C
		public void RegisterPlotMapLayer(PlotMapLayer layer)
		{
			this.plotLayer = layer;
			base.Mod.Logger.Debug("PlotMapLayer registered with mod system");
			this.TryApplyPendingConfig();
		}

		// Token: 0x06000059 RID: 89 RVA: 0x00008F30 File Offset: 0x00007130
		private void TryApplyPendingConfig()
		{
			if (this.pendingConfigPacket == null)
			{
				return;
			}
			if (this.plotLayer != null)
			{
				this.plotLayer.UpdateConfigFromServer(this.pendingConfigPacket);
				ILogger logger = base.Mod.Logger;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(65, 2);
				defaultInterpolatedStringHandler.AppendLiteral("Applied pending config to PlotMapLayer: Protected Zones=");
				defaultInterpolatedStringHandler.AppendFormatted<bool>(this.pendingConfigPacket.ProtectedZonesEnabled);
				defaultInterpolatedStringHandler.AppendLiteral(" (");
				List<ProtectedZoneData> protectedZones = this.pendingConfigPacket.ProtectedZones;
				defaultInterpolatedStringHandler.AppendFormatted<int>((protectedZones != null) ? protectedZones.Count : 0);
				defaultInterpolatedStringHandler.AppendLiteral(" zones)");
				logger.Debug(defaultInterpolatedStringHandler.ToStringAndClear());
				this.pendingConfigPacket = null;
			}
		}

		// Token: 0x14000001 RID: 1
		// (add) Token: 0x0600005A RID: 90 RVA: 0x00008FE0 File Offset: 0x000071E0
		// (remove) Token: 0x0600005B RID: 91 RVA: 0x00009018 File Offset: 0x00007218
		[Nullable(new byte[]
		{
			2,
			1,
			1
		})]
		[Nullable(new byte[]
		{
			2,
			1,
			1
		})]
		public event Action<List<GuildSummary>> OnClientGuildDataUpdated;

		// Token: 0x0600005C RID: 92 RVA: 0x0000904D File Offset: 0x0000724D
		public List<GuildSummary> GetClientGuildSummaries()
		{
			return new List<GuildSummary>(this.clientGuildSummaries);
		}

		// Token: 0x0600005D RID: 93 RVA: 0x0000905C File Offset: 0x0000725C
		public List<GuildSummary> GetClientGuildSummaries([Nullable(new byte[]
		{
			2,
			1
		})] Func<GuildSummary, bool> filter = null)
		{
			List<GuildSummary> result = new List<GuildSummary>(this.clientGuildSummaries);
			if (filter != null)
			{
				result = result.Where(filter).ToList<GuildSummary>();
			}
			return result;
		}

		// Token: 0x0600005E RID: 94 RVA: 0x00009088 File Offset: 0x00007288
		[return: Nullable(2)]
		public GuildSummary GetGuildSummary(string guildName)
		{
			return this.clientGuildSummaries.FirstOrDefault((GuildSummary g) => g.Name.Equals(guildName, StringComparison.OrdinalIgnoreCase));
		}

		// Token: 0x0600005F RID: 95 RVA: 0x000090B9 File Offset: 0x000072B9
		[NullableContext(2)]
		public GuildSummary GetCurrentPlayerGuildSummary()
		{
			return this.clientGuildSummaries.FirstOrDefault((GuildSummary g) => g.IsPlayerMember);
		}

		// Token: 0x06000060 RID: 96 RVA: 0x000090E8 File Offset: 0x000072E8
		public List<GuildSummary> GetGuildSummariesWithClaimsInArea(int centerChunkX, int centerChunkZ, int radius)
		{
			Func<LandClaimDto, bool> <>9__1;
			return this.clientGuildSummaries.Where(delegate(GuildSummary guild)
			{
				IEnumerable<LandClaimDto> claims = guild.Claims;
				Func<LandClaimDto, bool> predicate;
				if ((predicate = <>9__1) == null)
				{
					predicate = (<>9__1 = ((LandClaimDto claim) => Math.Abs(claim.ChunkX - centerChunkX) <= radius && Math.Abs(claim.ChunkZ - centerChunkZ) <= radius));
				}
				return claims.Any(predicate);
			}).ToList<GuildSummary>();
		}

		// Token: 0x06000061 RID: 97 RVA: 0x0000912C File Offset: 0x0000732C
		[return: TupleElementNames(new string[]
		{
			"chunkX",
			"chunkZ"
		})]
		[return: Nullable(new byte[]
		{
			1,
			0,
			1
		})]
		public Dictionary<ValueTuple<int, int>, GuildSummary> GetClaimedChunksLookup()
		{
			Dictionary<ValueTuple<int, int>, GuildSummary> lookup = new Dictionary<ValueTuple<int, int>, GuildSummary>();
			foreach (GuildSummary guild in this.clientGuildSummaries)
			{
				foreach (LandClaimDto claim in guild.Claims)
				{
					lookup[new ValueTuple<int, int>(claim.ChunkX, claim.ChunkZ)] = guild;
				}
			}
			return lookup;
		}

		// Token: 0x06000062 RID: 98 RVA: 0x000091D8 File Offset: 0x000073D8
		public bool IsChunkClaimed(int chunkX, int chunkZ)
		{
			Func<LandClaimDto, bool> <>9__1;
			return this.clientGuildSummaries.Any(delegate(GuildSummary guild)
			{
				IEnumerable<LandClaimDto> claims = guild.Claims;
				Func<LandClaimDto, bool> predicate;
				if ((predicate = <>9__1) == null)
				{
					predicate = (<>9__1 = ((LandClaimDto claim) => claim.ChunkX == chunkX && claim.ChunkZ == chunkZ));
				}
				return claims.Any(predicate);
			});
		}

		// Token: 0x06000063 RID: 99 RVA: 0x00009210 File Offset: 0x00007410
		[NullableContext(2)]
		public GuildSummary GetChunkOwner(int chunkX, int chunkZ)
		{
			Func<LandClaimDto, bool> <>9__1;
			return this.clientGuildSummaries.FirstOrDefault(delegate(GuildSummary guild)
			{
				IEnumerable<LandClaimDto> claims = guild.Claims;
				Func<LandClaimDto, bool> predicate;
				if ((predicate = <>9__1) == null)
				{
					predicate = (<>9__1 = ((LandClaimDto claim) => claim.ChunkX == chunkX && claim.ChunkZ == chunkZ));
				}
				return claims.Any(predicate);
			});
		}

		// Token: 0x06000064 RID: 100 RVA: 0x00009248 File Offset: 0x00007448
		public void RequestGuildSummariesUpdate()
		{
			if (this.clientApi != null && this.networkHandler != null)
			{
				base.Mod.Logger.Debug("Guild summaries update requested from server");
			}
		}

		// Token: 0x06000065 RID: 101 RVA: 0x0000926F File Offset: 0x0000746F
		[NullableContext(2)]
		public GuildNetworkHandler GetNetworkHandler()
		{
			return this.networkHandler;
		}

		// Token: 0x06000066 RID: 102 RVA: 0x00009278 File Offset: 0x00007478
		[NullableContext(2)]
		public PlotMapLayer GetPlotLayer()
		{
			WorldMapManager worldMapManager = this.worldMapManager;
			object obj;
			if (worldMapManager == null)
			{
				obj = null;
			}
			else
			{
				obj = worldMapManager.MapLayers.FirstOrDefault((MapLayer layer) => layer is PlotMapLayer);
			}
			PlotMapLayer plotMapLayer = obj as PlotMapLayer;
			if (plotMapLayer != null)
			{
				this.TryApplyPendingConfig();
			}
			return plotMapLayer;
		}

		// Token: 0x06000067 RID: 103 RVA: 0x000092C9 File Offset: 0x000074C9
		[NullableContext(2)]
		public GuildManager GetGuildManager()
		{
			return this.guildManager;
		}

		// Token: 0x06000068 RID: 104 RVA: 0x000092D4 File Offset: 0x000074D4
		public bool CheckGuildUsePrivilege(IServerPlayer player, BlockPos pos)
		{
			if (player == null || pos == null)
			{
				return false;
			}
			if (player.WorldData.CurrentGameMode == 2)
			{
				return true;
			}
			GuildManager guildManager = this.guildManager;
			GuildConfig guildConfig;
			if (guildManager == null)
			{
				guildConfig = null;
			}
			else
			{
				GuildConfigManager configManager = guildManager.GetConfigManager();
				guildConfig = ((configManager != null) ? configManager.GetConfig() : null);
			}
			GuildConfig config = guildConfig;
			ICoreServerAPI coreServerAPI = this.serverApi;
			BlockPos spawnPos = (coreServerAPI != null) ? coreServerAPI.World.DefaultSpawnPosition.AsBlockPos : null;
			if (config != null && spawnPos != null && config.IsWithinProtectedZone(pos.X, pos.Z, spawnPos))
			{
				ProtectedZone zone = config.GetProtectedZoneAt(pos.X, pos.Z, spawnPos);
				if (zone != null)
				{
					ZoneWhitelistManager zoneWhitelistManager = this.zoneWhitelistManager;
					if (zoneWhitelistManager != null)
					{
						zoneWhitelistManager.IsPlayerWhitelisted(zone.Id, player.PlayerUID);
					}
					return true;
				}
				return true;
			}
			else
			{
				int chunkX = LandClaim.FloorDiv(pos.X, 32);
				int chunkZ = LandClaim.FloorDiv(pos.Z, 32);
				Guild owningGuild = this.GetChunkOwningGuild(chunkX, chunkZ);
				if (owningGuild == null)
				{
					return false;
				}
				Guild playerGuild = this.guildManager.GetGuildByMember(player.PlayerUID);
				return playerGuild != null && !(playerGuild.Name != owningGuild.Name) && GuildManager.HasPermission(owningGuild, player.PlayerUID, GuildPermission.InteractBlocks);
			}
		}

		// Token: 0x06000069 RID: 105 RVA: 0x00009404 File Offset: 0x00007604
		public bool CheckGuildBuildPrivilege(IServerPlayer player, BlockPos pos)
		{
			if (player == null || pos == null)
			{
				return false;
			}
			if (player.WorldData.CurrentGameMode == 2)
			{
				return true;
			}
			GuildManager guildManager = this.guildManager;
			GuildConfig guildConfig;
			if (guildManager == null)
			{
				guildConfig = null;
			}
			else
			{
				GuildConfigManager configManager = guildManager.GetConfigManager();
				guildConfig = ((configManager != null) ? configManager.GetConfig() : null);
			}
			GuildConfig config = guildConfig;
			ICoreServerAPI coreServerAPI = this.serverApi;
			BlockPos spawnPos = (coreServerAPI != null) ? coreServerAPI.World.DefaultSpawnPosition.AsBlockPos : null;
			if (config != null && spawnPos != null && config.IsWithinProtectedZone(pos.X, pos.Z, spawnPos))
			{
				ProtectedZone zone = config.GetProtectedZoneAt(pos.X, pos.Z, spawnPos);
				if (zone != null)
				{
					ZoneWhitelistManager zoneWhitelistManager = this.zoneWhitelistManager;
					if (zoneWhitelistManager != null && zoneWhitelistManager.IsPlayerWhitelisted(zone.Id, player.PlayerUID))
					{
						return true;
					}
				}
				return false;
			}
			int chunkX = LandClaim.FloorDiv(pos.X, 32);
			int chunkZ = LandClaim.FloorDiv(pos.Z, 32);
			Guild owningGuild = this.GetChunkOwningGuild(chunkX, chunkZ);
			if (owningGuild == null)
			{
				return false;
			}
			Guild playerGuild = this.guildManager.GetGuildByMember(player.PlayerUID);
			return playerGuild != null && !(playerGuild.Name != owningGuild.Name) && GuildManager.HasPermission(owningGuild, player.PlayerUID, GuildPermission.BreakAndPlaceBlocks);
		}

		// Token: 0x0600006A RID: 106 RVA: 0x00009533 File Offset: 0x00007733
		public void ShowInvitePopup(GuildInviteNotificationPacket invite)
		{
			if (this.clientApi == null || this.invitePopup == null)
			{
				return;
			}
			this.invitePopup.ShowInvite(invite);
		}

		// Token: 0x0600006B RID: 107 RVA: 0x00009552 File Offset: 0x00007752
		public void ShowInviteListPopup(List<GuildInviteInfo> invites)
		{
			if (this.clientApi == null || this.invitePopup == null)
			{
				return;
			}
			this.invitePopup.ShowInvites(invites);
		}

		// Token: 0x0600006C RID: 108 RVA: 0x00009571 File Offset: 0x00007771
		public void RegisterNodeWarDataProvider(object pvpModSystem)
		{
			ICoreServerAPI coreServerAPI = this.serverApi;
			if (coreServerAPI == null)
			{
				return;
			}
			coreServerAPI.Logger.Notification("[Guild] PVP mod registered as node war data provider");
		}

		// Token: 0x04000006 RID: 6
		[Nullable(2)]
		private GuildManager guildManager;

		// Token: 0x04000007 RID: 7
		[Nullable(2)]
		private GuildTechManager guildTechManager;

		// Token: 0x04000008 RID: 8
		[Nullable(2)]
		private ZoneWhitelistManager zoneWhitelistManager;

		// Token: 0x04000009 RID: 9
		[Nullable(2)]
		private NodeManager nodeManager;

		// Token: 0x0400000A RID: 10
		[Nullable(2)]
		private GuildDatabase guildDatabase;

		// Token: 0x0400000B RID: 11
		[Nullable(2)]
		private GuildRepository guildRepository;

		// Token: 0x0400000C RID: 12
		[Nullable(2)]
		private QuestRepository questRepository;

		// Token: 0x0400000D RID: 13
		[Nullable(2)]
		private CooldownRepository cooldownRepository;

		// Token: 0x0400000E RID: 14
		[Nullable(2)]
		private ZoneWhitelistRepository zoneWhitelistRepository;

		// Token: 0x0400000F RID: 15
		[Nullable(2)]
		private NodeRepository nodeRepository;

		// Token: 0x04000010 RID: 16
		[Nullable(2)]
		private LandClaimRepository landClaimRepository;

		// Token: 0x04000011 RID: 17
		[Nullable(2)]
		private ICoreServerAPI serverApi;

		// Token: 0x04000012 RID: 18
		[Nullable(2)]
		private ICoreClientAPI clientApi;

		// Token: 0x04000013 RID: 19
		[Nullable(2)]
		private GuildNetworkHandler networkHandler;

		// Token: 0x04000014 RID: 20
		[Nullable(2)]
		private QuestNetworkHandler questNetworkHandler;

		// Token: 0x04000015 RID: 21
		private List<GuildSummary> clientGuildSummaries = new List<GuildSummary>();

		// Token: 0x04000016 RID: 22
		[Nullable(2)]
		private PlotMapLayer plotLayer;

		// Token: 0x04000017 RID: 23
		[Nullable(2)]
		private WorldMapManager worldMapManager;

		// Token: 0x04000018 RID: 24
		[Nullable(2)]
		private GuildConfigPacket pendingConfigPacket;

		// Token: 0x04000019 RID: 25
		[Nullable(2)]
		private Harmony harmony;

		// Token: 0x0400001A RID: 26
		private bool hologramVisible;

		// Token: 0x0400001B RID: 27
		[Nullable(2)]
		private DialogGuildInvitePopup invitePopup;

		// Token: 0x0400001C RID: 28
		[Nullable(2)]
		private DialogGuildMain guildDialog;

		// Token: 0x0400001D RID: 29
		[Nullable(2)]
		private DialogQuestManager questManagerDialog;

		// Token: 0x0400001E RID: 30
		private const int HOLOGRAM_SLOT = 99;

		// Token: 0x0400001F RID: 31
		[Nullable(2)]
		private TechBlocksConfig techBlocksConfig;
	}
}
