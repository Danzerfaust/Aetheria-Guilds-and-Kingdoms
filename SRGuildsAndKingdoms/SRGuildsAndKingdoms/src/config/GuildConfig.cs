using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;

namespace SRGuildsAndKingdoms.src.config
{
    /// <summary>
    /// Configuration class for guild system settings
    /// </summary>
    public class GuildConfig
    {
        /// <summary>
        /// Server name identifier for config synchronization
        /// </summary>
        public string ServerName { get; set; } = "MyVintageStoryServer";

        /// <summary>
        /// Maximum number of members allowed per guild
        /// </summary>
        public int MaxMembersPerGuild { get; set; } = 40;

        /// <summary>
        /// Default base maximum claims per guild
        /// </summary>
        public int BaseMaxClaimsPerGuild { get; set; } = 20;

        /// <summary>
        /// Whether to enable dynamic claim limits based on player count
        /// </summary>
        public bool EnableDynamicClaimLimits { get; set; } = true;

        /// <summary>
        /// Guild member count thresholds that increase max claims
        /// </summary>
        public List<PlayerCountThreshold> PlayerCountThresholds { get; set; } = new()
        {
            new PlayerCountThreshold { MinPlayerCount = 5, AdditionalClaims = 10 },
            new PlayerCountThreshold { MinPlayerCount = 10, AdditionalClaims = 10 },
            new PlayerCountThreshold { MinPlayerCount = 15, AdditionalClaims = 10 },
            new PlayerCountThreshold { MinPlayerCount = 20, AdditionalClaims = 10 },
            new PlayerCountThreshold { MinPlayerCount = 25, AdditionalClaims = 10 },
            new PlayerCountThreshold { MinPlayerCount = 30, AdditionalClaims = 10 },
            new PlayerCountThreshold { MinPlayerCount = 35, AdditionalClaims = 10 },
            new PlayerCountThreshold { MinPlayerCount = 40, AdditionalClaims = 10 },
        };

        /// <summary>
        /// Maximum absolute limit for claims per guild (safety cap)
        /// </summary>
        public int AbsoluteMaxClaimsPerGuild { get; set; } = 100;

        /// <summary>
        /// Base maximum number of outposts per guild
        /// </summary>
        public int BaseMaxOutpostsPerGuild { get; set; } = 0;

        /// <summary>
        /// Whether to enable dynamic outpost limits based on player count
        /// </summary>
        public bool EnableDynamicOutpostLimits { get; set; } = true;

        /// <summary>
        /// Guild member count thresholds that increase max outposts
        /// </summary>
        public List<PlayerCountThreshold> OutpostPlayerCountThresholds { get; set; } = new()
        {
            new PlayerCountThreshold { MinPlayerCount = 5, AdditionalClaims = 1 },
            new PlayerCountThreshold { MinPlayerCount = 10, AdditionalClaims = 0 },
            new PlayerCountThreshold { MinPlayerCount = 15, AdditionalClaims = 1 },
            new PlayerCountThreshold { MinPlayerCount = 20, AdditionalClaims = 0 },
            new PlayerCountThreshold { MinPlayerCount = 25, AdditionalClaims = 1 },
        };

        /// <summary>
        /// Maximum absolute limit for outposts per guild (safety cap)
        /// </summary>
        public int AbsoluteMaxOutpostsPerGuild { get; set; } = 3;

        /// <summary>
        /// Number of IRL days a player must wait before joining another guild after leaving/being kicked
        /// </summary>
        public int GuildRejoinCooldownDays { get; set; } = 3;

        /// <summary>
        /// Reduced cooldown in days for players who disband their guild (last member leaving)
        /// </summary>
        public int GuildDisbandCooldownDays { get; set; } = 1;

        /// <summary>
        /// Point thresholds for guild rank classes. Key is the class name, value is the minimum points required.
        /// Guilds below the lowest threshold are considered "D" class.
        /// </summary>
        public Dictionary<string, int> ClassThresholds { get; set; } = new()
        {
            { "C", 400 },
            { "B", 900 },
            { "A", 1400 },
            { "S", 2000 }
        };

        /// <summary>
        /// Point contribution thresholds for individual member ranks. Key is the rank name, value is the minimum points_contribution required.
        /// Members below the lowest threshold are considered "Guild Member".
        /// </summary>
        public Dictionary<string, int> MemberRankThresholds { get; set; } = new()
        {
            { "Jr Shadow Knight 3rd Class", 150 },
            { "Jr Shadow Knight 2nd Class", 200 },
            { "Jr Shadow Knight 1st Class", 250 },
            { "Sr Shadow Knight 3rd Class", 300 },
            { "Sr Shadow Knight 2nd Class", 400 },
            { "Sr Shadow Knight 1st Class", 450 },
            { "Grand Shadow Knight", 500 }
        };

        /// <summary>
        /// Maximum GRS points a guild can earn per week (resets Sunday 12am EST). 
        /// Set to 0 for unlimited.
        /// </summary>
        public int MaxWeeklyGrsPoints { get; set; } = 100;

        /// <summary>
        /// Definition for "Tails" currency used in quest rewards
        /// </summary>
        public CurrencyDefinition QuestTailsDefinition { get; set; } = new()
        {
            Code = "coinage:planchet-molybdochalkos-md",
            Nbt = null
        };

        /// <summary>
        /// Definition for "Crowns" currency used in quest rewards
        /// </summary>
        public CurrencyDefinition QuestCrownsDefinition { get; set; } = new()
        {
            Code = "coinage:planchet-platinum-md",
            Nbt = null
        };

        /// <summary>
        /// Gets the guild rank class based on points
        /// </summary>
        /// <param name="points">The guild's current points</param>
        /// <returns>The rank class (e.g., "S", "A", "B", "C", or "D")</returns>
        public string GetGuildRankClass(int points)
        {
            string rankClass = "D"; // Default class for guilds below all thresholds
            int highestThreshold = 0;

            foreach (var threshold in ClassThresholds)
            {
                if (points >= threshold.Value && threshold.Value >= highestThreshold)
                {
                    rankClass = threshold.Key;
                    highestThreshold = threshold.Value;
                }
            }

            return rankClass;
        }

        /// <summary>
        /// Gets the member rank based on points contribution
        /// </summary>
        /// <param name="pointsContribution">The member's current points_contribution</param>
        /// <returns>The member rank (e.g., "Grand Shadow Knight", "Jr Shadow Knight 3rd Class", or "Guild Member")</returns>
        public string GetMemberRank(int pointsContribution)
        {
            string rank = "Guild Member"; // Default rank for members below all thresholds
            int highestThreshold = 0;

            foreach (var threshold in MemberRankThresholds)
            {
                if (pointsContribution >= threshold.Value && threshold.Value >= highestThreshold)
                {
                    rank = threshold.Key;
                    highestThreshold = threshold.Value;
                }
            }

            return rank;
        }

        /// <summary>
        /// Whether to enable territorial restrictions for claims
        /// </summary>
        public bool EnableTerritorialRestrictions { get; set; } = false;

        /// <summary>
        /// Center coordinate of the allowed claiming area (block coordinates)
        /// </summary>
        public ClaimRestrictionCenter? TerritorialCenter { get; set; } = null;

        /// <summary>
        /// Radius in blocks from the center where claims are allowed
        /// </summary>
        public int TerritorialRadius { get; set; } = 1000;

        /// <summary>
        /// Whether to enable protected zones where claims and block breaking are forbidden
        /// </summary>
        public bool EnableProtectedZones { get; set; } = false;

        /// <summary>
        /// List of protected zones where land cannot be claimed and blocks cannot be broken
        /// </summary>
        public List<ProtectedZone> ProtectedZones { get; set; } = new();

        /// <summary>
        /// Whether to enable nodes that can be captured and controlled
        /// </summary>
        public bool EnableNodes { get; set; } = false;

        /// <summary>
        /// Calculate the maximum claims allowed based on guild member count
        /// </summary>
        /// <param name="currentPlayerCount">Current guild member count</param>
        /// <returns>Maximum claims per guild</returns>
        public int CalculateMaxClaimsPerGuild(int currentPlayerCount)
        {
            if (!EnableDynamicClaimLimits)
            {
                return BaseMaxClaimsPerGuild;
            }

            int maxClaims = BaseMaxClaimsPerGuild;

            // Apply thresholds in order (they should be sorted by MinPlayerCount)
            foreach (var threshold in PlayerCountThresholds)
            {
                if (currentPlayerCount >= threshold.MinPlayerCount)
                {
                    maxClaims = maxClaims + threshold.AdditionalClaims;
                }
                else
                {
                    break; // Stop at first threshold not met
                }
            }

            // Apply absolute maximum cap
            return Math.Min(maxClaims, AbsoluteMaxClaimsPerGuild);
        }

        /// <summary>
        /// Calculate the maximum outposts allowed based on guild member count
        /// </summary>
        /// <param name="currentPlayerCount">Current guild member count</param>
        /// <returns>Maximum outposts per guild</returns>
        public int CalculateMaxOutpostsPerGuild(int currentPlayerCount)
        {
            if (!EnableDynamicOutpostLimits)
            {
                return BaseMaxOutpostsPerGuild;
            }

            int maxOutposts = BaseMaxOutpostsPerGuild;

            // Apply thresholds in order (they should be sorted by MinPlayerCount)
            foreach (var threshold in OutpostPlayerCountThresholds)
            {
                if (currentPlayerCount >= threshold.MinPlayerCount)
                {
                    maxOutposts = maxOutposts + threshold.AdditionalClaims;
                }
                else
                {
                    break; // Stop at first threshold not met
                }
            }

            // Apply absolute maximum cap
            return Math.Min(maxOutposts, AbsoluteMaxOutpostsPerGuild);
        }

        /// <summary>
        /// Get the next threshold that would increase claims
        /// </summary>
        /// <param name="currentPlayerCount">Current guild member count</param>
        /// <returns>Next threshold or null if at max</returns>
        public PlayerCountThreshold? GetNextThreshold(int currentPlayerCount)
        {
            foreach (var threshold in PlayerCountThresholds)
            {
                if (currentPlayerCount < threshold.MinPlayerCount)
                {
                    return threshold;
                }
            }
            return null; // Already at or above the highest threshold
        }

        /// <summary>
        /// Get the next threshold that would increase outposts
        /// </summary>
        /// <param name="currentPlayerCount">Current guild member count</param>
        /// <returns>Next threshold or null if at max</returns>
        public PlayerCountThreshold? GetNextOutpostThreshold(int currentPlayerCount)
        {
            foreach (var threshold in OutpostPlayerCountThresholds)
            {
                if (currentPlayerCount < threshold.MinPlayerCount)
                {
                    return threshold;
                }
            }
            return null; // Already at or above the highest threshold
        }

        internal bool IsChunkWithinTerritorialBounds(int chunkX, int chunkZ, BlockPos spawnPos)
        {
            if (TerritorialCenter == null || !EnableTerritorialRestrictions)
            {
                return true; // No restrictions, always within bounds
            }

            // Convert chunk coordinates to block coordinates (assuming 32 blocks per chunk)
            // Use the center of the chunk for the calculation
            int blockX = chunkX * 32 + 16;
            int blockZ = chunkZ * 32 + 16;

            return IsWithinTerritorialBounds(blockX, blockZ, spawnPos);
        }

        internal bool IsWithinTerritorialBounds(int blockX, int blockZ, BlockPos spawnPos)
        {
            if (TerritorialCenter == null || !EnableTerritorialRestrictions)
            {
                return true; // No restrictions, always within bounds
            }

            // Calculate the distance from the center (relative to spawn position)
            int deltaX = blockX - TerritorialCenter.Position.X - spawnPos.X;
            int deltaZ = blockZ - TerritorialCenter.Position.Z - spawnPos.Z;
            double distance = Math.Sqrt(deltaX * deltaX + deltaZ * deltaZ);

            // Check if within territorial radius
            return distance <= TerritorialRadius;
        }

        /// <summary>
        /// Check if a chunk is within any protected zone
        /// </summary>
        internal bool IsChunkWithinProtectedZone(int chunkX, int chunkZ, BlockPos spawnPos)
        {
            if (!EnableProtectedZones || ProtectedZones == null || ProtectedZones.Count == 0)
            {
                return false; // No protected zones, not within any
            }

            // Convert chunk coordinates to block coordinates (assuming 32 blocks per chunk)
            // Use the center of the chunk for the calculation
            int blockX = chunkX * 32 + 16;
            int blockZ = chunkZ * 32 + 16;

            return IsWithinProtectedZone(blockX, blockZ, spawnPos);
        }

        /// <summary>
        /// Check if a block position is within any protected zone
        /// </summary>
        internal bool IsWithinProtectedZone(int blockX, int blockZ, BlockPos spawnPos)
        {
            if (!EnableProtectedZones || ProtectedZones == null || ProtectedZones.Count == 0)
            {
                return false; // No protected zones, not within any
            }

            // Check if the position is within any protected zone
            foreach (var zone in ProtectedZones)
            {
                if (zone.IsPositionWithinZone(blockX, blockZ, spawnPos))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Get the protected zone at a specific position, if any
        /// </summary>
        internal ProtectedZone? GetProtectedZoneAt(int blockX, int blockZ, BlockPos spawnPos)
        {
            if (!EnableProtectedZones || ProtectedZones == null || ProtectedZones.Count == 0)
            {
                return null;
            }

            foreach (var zone in ProtectedZones)
            {
                if (zone.IsPositionWithinZone(blockX, blockZ, spawnPos))
                {
                    return zone;
                }
            }

            return null;
        }
    }

    /// <summary>
    /// Represents the center coordinate for territorial claim restrictions
    /// </summary>
    public class ClaimRestrictionCenter
    {
        /// <summary>
        /// Block position of the center (uses Vintage Story's BlockPos)
        /// </summary>
        [JsonIgnore]
        public BlockPos Position { get; set; }

        /// <summary>
        /// X coordinate for JSON serialization (backwards compatibility)
        /// </summary>
        [JsonPropertyName("x")]
        public int X
        {
            get => Position.X;
            set => Position = new BlockPos(value, Position?.Y ?? 0, Position?.Z ?? 0);
        }

        /// <summary>
        /// Z coordinate for JSON serialization (backwards compatibility)
        /// </summary>
        [JsonPropertyName("z")]
        public int Z
        {
            get => Position.Z;
            set => Position = new BlockPos(Position?.X ?? 0, Position?.Y ?? 0, value);
        }

        public override string ToString()
        {
            return $"({Position.X}, {Position.Z})";
        }
    }

    /// <summary>
    /// Represents a protected zone where land cannot be claimed and blocks cannot be broken
    /// </summary>
    public class ProtectedZone
    {
        /// <summary>
        /// Unique identifier for this protected zone (for easy command reference)
        /// </summary>
        public int Id { get; set; } = 0;

        /// <summary>
        /// Name or description of this protected zone
        /// </summary>
        public string Name { get; set; } = "Unnamed Zone";

        /// <summary>
        /// Block position of the zone center (uses Vintage Story's BlockPos)
        /// </summary>
        [JsonIgnore]
        public BlockPos Center { get; set; } = new BlockPos(0, 0, 0);

        /// <summary>
        /// X coordinate for JSON serialization
        /// </summary>
        [JsonPropertyName("x")]
        public int X
        {
            get => Center.X;
            set => Center = new BlockPos(value, Center?.Y ?? 0, Center?.Z ?? 0);
        }

        /// <summary>
        /// Z coordinate for JSON serialization
        /// </summary>
        [JsonPropertyName("z")]
        public int Z
        {
            get => Center.Z;
            set => Center = new BlockPos(Center?.X ?? 0, Center?.Y ?? 0, value);
        }

        /// <summary>
        /// Radius in blocks from the center where protection is active
        /// </summary>
        public int Radius { get; set; } = 100;

        /// <summary>
        /// Check if a position is within this protected zone
        /// </summary>
        internal bool IsPositionWithinZone(int blockX, int blockZ, BlockPos spawnPos)
        {
            // Calculate the distance from the zone center (relative to spawn position)
            int deltaX = blockX - Center.X - spawnPos.X;
            int deltaZ = blockZ - Center.Z - spawnPos.Z;
            double distance = Math.Sqrt(deltaX * deltaX + deltaZ * deltaZ);

            return distance <= Radius;
        }

        public override string ToString()
        {
            return $"{Name} at ({Center.X}, {Center.Z}) with radius {Radius}";
        }
    }

    /// <summary>
    /// Represents a guild member count threshold that grants additional claims
    /// </summary>
    public class PlayerCountThreshold
    {
        /// <summary>
        /// Minimum number of guild members required for this threshold
        /// </summary>
        public int MinPlayerCount { get; set; }

        /// <summary>
        /// Additional claims granted when this threshold is reached
        /// </summary>
        public int AdditionalClaims { get; set; }

        /// <summary>
        /// Optional description for this threshold
        /// </summary>
        public string Description { get; set; } = "";

        public override string ToString()
        {
            return $"At {MinPlayerCount} members: +{AdditionalClaims} claims";
        }
    }

    /// <summary>
    /// Manager class for handling guild configuration (read-only after creation)
    /// </summary>
    public class GuildConfigManager
    {
        private readonly ICoreServerAPI serverApi;
        private GuildConfig config;
        private readonly string configPath;

        public GuildConfigManager(ICoreServerAPI serverApi)
        {
            this.serverApi = serverApi;
            configPath = Path.Combine(serverApi.GetOrCreateDataPath("ModConfig/SRGuildsAndKingdoms"), "guild-config.json");
            config = new GuildConfig();
        }

        /// <summary>
        /// Load configuration from file, creating defaults if file doesn't exist
        /// Configuration is read-only after creation
        /// </summary>
        public void LoadConfig()
        {
            try
            {
                if (File.Exists(configPath))
                {
                    var json = File.ReadAllText(configPath);
                    var loadedConfig = JsonSerializer.Deserialize<GuildConfig>(json, GetSerializerOptions());
                    if (loadedConfig != null)
                    {
                        config = loadedConfig;

                        // Assign IDs to protected zones if they don't have them
                        bool idsAssigned = EnsureProtectedZoneIds();

                        serverApi.Logger.Notification("Guild configuration loaded successfully");

                        // Save config if IDs were assigned for backwards compatibility
                        if (idsAssigned)
                        {
                            SaveConfig();
                            serverApi.Logger.Notification("Auto-assigned IDs to protected zones and saved config");
                        }
                    }
                    else
                    {
                        serverApi.Logger.Warning("Failed to deserialize guild config, using defaults");
                    }
                }
                else
                {
                    // Create default config file
                    SaveConfig();
                    serverApi.Logger.Notification("Created default guild configuration file at: " + configPath);
                    serverApi.Logger.Notification("You can edit this file to customize guild settings, then restart the server.");
                }

                // Validate and sort thresholds
                ValidateConfig();
            }
            catch (Exception ex)
            {
                serverApi.Logger.Error($"Error loading guild configuration: {ex.Message}");
                config = new GuildConfig(); // Fall back to defaults
            }
        }

        /// <summary>
        /// Save configuration to file (only used for initial creation)
        /// </summary>
        private void SaveConfig()
        {
            try
            {
                var json = JsonSerializer.Serialize(config, GetSerializerOptions());
                File.WriteAllText(configPath, json);
                serverApi.Logger.Debug("Guild configuration saved");
            }
            catch (Exception ex)
            {
                serverApi.Logger.Error($"Error saving guild configuration: {ex.Message}");
            }
        }

        /// <summary>
        /// Get current configuration (read-only)
        /// </summary>
        public GuildConfig GetConfig()
        {
            return config;
        }

        /// <summary>
        /// Get maximum claims per guild based on the guild's member count
        /// </summary>
        /// <param name="guildMemberCount">Number of members in the guild</param>
        public int GetMaxClaimsPerGuild(int guildMemberCount)
        {
            return config.CalculateMaxClaimsPerGuild(guildMemberCount);
        }

        /// <summary>
        /// Get maximum outposts per guild based on the guild's member count
        /// </summary>
        /// <param name="guildMemberCount">Number of members in the guild</param>
        public int GetMaxOutpostsPerGuild(int guildMemberCount)
        {
            return config.CalculateMaxOutpostsPerGuild(guildMemberCount);
        }

        /// <summary>
        /// Update a quest currency definition and save the config
        /// </summary>
        /// <param name="currencyType">Type of currency: "tails" or "crowns"</param>
        /// <param name="itemCode">Item code for the currency</param>
        /// <param name="nbtBase64">Base64-encoded NBT data (null if none)</param>
        public void UpdateQuestCurrency(string currencyType, string itemCode, string? nbtBase64)
        {
            var currencyDef = new CurrencyDefinition(itemCode, nbtBase64);

            if (currencyType.Equals("tails", StringComparison.OrdinalIgnoreCase))
            {
                config.QuestTailsDefinition = currencyDef;
                serverApi.Logger.Notification($"[GuildConfig] Updated Tails currency: {itemCode}" + (nbtBase64 == null ? "" : " (with NBT)"));
            }
            else if (currencyType.Equals("crowns", StringComparison.OrdinalIgnoreCase))
            {
                config.QuestCrownsDefinition = currencyDef;
                serverApi.Logger.Notification($"[GuildConfig] Updated Crowns currency: {itemCode}" + (nbtBase64 == null ? "" : " (with NBT)"));
            }
            else
            {
                serverApi.Logger.Warning($"[GuildConfig] Unknown currency type: {currencyType}");
                return;
            }

            // Save the updated config
            SaveConfig();
        }

        /// <summary>
        /// Ensures all protected zones have unique IDs assigned
        /// Auto-assigns sequential IDs to zones with Id = 0
        /// </summary>
        /// <returns>True if any IDs were assigned, false if all zones already had IDs</returns>
        private bool EnsureProtectedZoneIds()
        {
            if (config.ProtectedZones == null || config.ProtectedZones.Count == 0)
                return false;

            // Check if any zones need ID assignment
            var zonesNeedingIds = config.ProtectedZones.Where(z => z.Id == 0).ToList();
            if (zonesNeedingIds.Count == 0)
            {
                // Check for duplicate IDs
                var duplicateIds = config.ProtectedZones
                    .GroupBy(z => z.Id)
                    .Where(g => g.Count() > 1)
                    .Select(g => g.Key)
                    .ToList();

                if (duplicateIds.Count > 0)
                {
                    serverApi.Logger.Warning($"Found duplicate zone IDs: {string.Join(", ", duplicateIds)}. Reassigning IDs...");
                    // Reassign all IDs to fix duplicates
                    ReassignAllZoneIds();
                    return true;
                }

                // All zones have unique IDs
                return false;
            }

            // Get the highest existing ID
            int maxId = config.ProtectedZones
                .Where(z => z.Id > 0)
                .Select(z => z.Id)
                .DefaultIfEmpty(-1)
                .Max();

            int nextId = maxId + 1;
            int assignedCount = 0;

            // Assign IDs to zones that don't have them
            foreach (var zone in zonesNeedingIds)
            {
                zone.Id = nextId;
                serverApi.Logger.Notification($"[ZoneConfig] Auto-assigned ID {nextId} to zone '{zone.Name}'");
                nextId++;
                assignedCount++;
            }

            serverApi.Logger.Notification($"[ZoneConfig] Assigned IDs to {assignedCount} protected zone(s)");
            return assignedCount > 0;
        }

        /// <summary>
        /// Reassigns all zone IDs sequentially (used when duplicates are detected)
        /// </summary>
        private void ReassignAllZoneIds()
        {
            for (int i = 0; i < config.ProtectedZones.Count; i++)
            {
                config.ProtectedZones[i].Id = i;
                serverApi.Logger.Debug($"[ZoneConfig] Reassigned ID {i} to zone '{config.ProtectedZones[i].Name}'");
            }
        }

        /// <summary>
        /// Validate configuration settings and fix any issues
        /// </summary>
        private void ValidateConfig()
        {
            // Ensure reasonable bounds
            config.BaseMaxClaimsPerGuild = Math.Max(1, config.BaseMaxClaimsPerGuild);
            config.AbsoluteMaxClaimsPerGuild = Math.Max(config.BaseMaxClaimsPerGuild, config.AbsoluteMaxClaimsPerGuild);

            // Ensure reasonable outpost bounds
            config.BaseMaxOutpostsPerGuild = Math.Max(0, config.BaseMaxOutpostsPerGuild);
            config.AbsoluteMaxOutpostsPerGuild = Math.Max(config.BaseMaxOutpostsPerGuild, config.AbsoluteMaxOutpostsPerGuild);

            // Validate territorial restrictions
            if (config.EnableTerritorialRestrictions)
            {
                if (config.TerritorialCenter == null)
                {
                    serverApi.Logger.Warning("Territorial restrictions enabled but no center coordinates specified. Disabling territorial restrictions.");
                    config.EnableTerritorialRestrictions = false;
                }
                else
                {
                    config.TerritorialRadius = Math.Max(1000, config.TerritorialRadius); // Minimum 100 block radius
                    serverApi.Logger.Notification($"Territorial restrictions enabled: Center {config.TerritorialCenter}, Radius {config.TerritorialRadius} blocks");
                }
            }

            // Validate protected zones
            if (config.EnableProtectedZones)
            {
                if (config.ProtectedZones == null || config.ProtectedZones.Count == 0)
                {
                    serverApi.Logger.Warning("Protected zones enabled but no zones defined. Disabling protected zones.");
                    config.EnableProtectedZones = false;
                }
                else
                {
                    // Validate each zone and ensure minimum radius
                    int validZoneCount = 0;
                    foreach (var zone in config.ProtectedZones)
                    {
                        zone.Radius = Math.Max(50, zone.Radius); // Minimum 50 block radius
                        if (string.IsNullOrWhiteSpace(zone.Name))
                        {
                            zone.Name = $"Protected Zone {validZoneCount + 1}";
                        }
                        validZoneCount++;
                    }
                    serverApi.Logger.Notification($"Protected zones enabled: {validZoneCount} zone(s) defined");
                    foreach (var zone in config.ProtectedZones)
                    {
                        serverApi.Logger.Notification($"  - [ID: {zone.Id}] {zone}");
                    }
                }
            }

            // Sort thresholds by player count
            config.PlayerCountThresholds.Sort((a, b) => a.MinPlayerCount.CompareTo(b.MinPlayerCount));
            config.OutpostPlayerCountThresholds.Sort((a, b) => a.MinPlayerCount.CompareTo(b.MinPlayerCount));

            // Remove duplicate thresholds for regular claims
            var uniqueThresholds = new List<PlayerCountThreshold>();
            int lastPlayerCount = -1;
            foreach (var threshold in config.PlayerCountThresholds)
            {
                if (threshold.MinPlayerCount > lastPlayerCount && threshold.MinPlayerCount > 0)
                {
                    uniqueThresholds.Add(threshold);
                    lastPlayerCount = threshold.MinPlayerCount;
                }
            }
            config.PlayerCountThresholds = uniqueThresholds;

            // Remove duplicate thresholds for outposts
            var uniqueOutpostThresholds = new List<PlayerCountThreshold>();
            int lastOutpostPlayerCount = -1;
            foreach (var threshold in config.OutpostPlayerCountThresholds)
            {
                if (threshold.MinPlayerCount > lastOutpostPlayerCount && threshold.MinPlayerCount > 0)
                {
                    uniqueOutpostThresholds.Add(threshold);
                    lastOutpostPlayerCount = threshold.MinPlayerCount;
                }
            }
            config.OutpostPlayerCountThresholds = uniqueOutpostThresholds;

            serverApi.Logger.Debug($"Guild config validated: Base={config.BaseMaxClaimsPerGuild}, Dynamic={config.EnableDynamicClaimLimits}, Thresholds={config.PlayerCountThresholds.Count}");
            serverApi.Logger.Debug($"Outpost config validated: Base={config.BaseMaxOutpostsPerGuild}, Dynamic={config.EnableDynamicOutpostLimits}, Thresholds={config.OutpostPlayerCountThresholds.Count}");
        }

        /// <summary>
        /// Get JSON serializer options for configuration files
        /// </summary>
        private static JsonSerializerOptions GetSerializerOptions()
        {
            return new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };
        }

        /// <summary>
        /// Get configuration status information for logging/debugging
        /// </summary>
        /// <param name="exampleMemberCount">Optional example member count to show claim limits (defaults to 1)</param>
        public string GetConfigStatus(int exampleMemberCount = 1)
        {
            int currentMaxClaims = GetMaxClaimsPerGuild(exampleMemberCount);
            int currentMaxOutposts = GetMaxOutpostsPerGuild(exampleMemberCount);
            var nextThreshold = config.GetNextThreshold(exampleMemberCount);
            var nextOutpostThreshold = config.GetNextOutpostThreshold(exampleMemberCount);

            var status = $"Guild Config Status:\n";
            status += $"  Base Max Claims: {config.BaseMaxClaimsPerGuild}\n";
            status += $"  Dynamic Limits: {config.EnableDynamicClaimLimits}\n";
            status += $"  Base Max Outposts: {config.BaseMaxOutpostsPerGuild}\n";
            status += $"  Dynamic Outpost Limits: {config.EnableDynamicOutpostLimits}\n";
            status += $"  Claim limits scale with guild member count\n";
            status += $"  Example: {exampleMemberCount} member(s) = {currentMaxClaims} max claims, {currentMaxOutposts} max outposts\n";


            if (config.EnableTerritorialRestrictions && config.TerritorialCenter != null)
            {
                status += $"  Territorial Restrictions: Enabled\n";
                status += $"  Allowed Center: {config.TerritorialCenter}\n";
                status += $"  Allowed Radius: {config.TerritorialRadius} blocks\n";
            }
            else
            {
                status += $"  Territorial Restrictions: Disabled\n";
            }

            if (config.EnableProtectedZones && config.ProtectedZones != null && config.ProtectedZones.Count > 0)
            {
                status += $"  Protected Zones: Enabled ({config.ProtectedZones.Count} zone(s))\n";
                foreach (var zone in config.ProtectedZones)
                {
                    status += $"    - {zone}\n";
                }
            }
            else
            {
                status += $"  Protected Zones: Disabled\n";
            }

            if (nextThreshold != null)
            {
                status += $"  Next Claim Threshold: {nextThreshold.MinPlayerCount} members (+{nextThreshold.AdditionalClaims} claims)\n";
            }
            else
            {
                status += $"  At Maximum Claim Threshold\n";
            }

            if (nextOutpostThreshold != null)
            {
                status += $"  Next Outpost Threshold: {nextOutpostThreshold.MinPlayerCount} members (+{nextOutpostThreshold.AdditionalClaims} outposts)\n";
            }
            else
            {
                status += $"  At Maximum Outpost Threshold\n";
            }

            return status;
        }
    }

    /// <summary>
    /// Represents a currency definition for quest rewards
    /// </summary>
    public class CurrencyDefinition
    {
        /// <summary>
        /// Item code for the currency (e.g., "coinage:planchet-platinum-md")
        /// </summary>
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// Base64-encoded NBT data for the currency item (null if no NBT required)
        /// </summary>
        public string? Nbt { get; set; } = null;

        /// <summary>
        /// Default constructor
        /// </summary>
        public CurrencyDefinition() { }

        /// <summary>
        /// Constructor with code and NBT
        /// </summary>
        public CurrencyDefinition(string code, string? nbt = null)
        {
            Code = code;
            Nbt = nbt;
        }

        public override string ToString()
        {
            return $"{Code}" + (string.IsNullOrEmpty(Nbt) ? "" : " (with NBT)");
        }
    }
}