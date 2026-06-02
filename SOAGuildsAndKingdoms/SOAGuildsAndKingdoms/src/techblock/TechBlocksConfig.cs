using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using Vintagestory.API.Common;

namespace SOAGuildsAndKingdoms.src.techblock
{
    /// <summary>
    /// Configuration class for loading tech blocks from JSON
    /// </summary>
    public class TechBlocksConfig
    {
        [JsonPropertyName("techBlocks")]
        public List<TechBlock> TechBlocks { get; set; } = new List<TechBlock>();

        /// <summary>
        /// List of ages that are currently enabled for research.
        /// Tech blocks belonging to locked ages cannot be researched until their age is enabled.
        /// </summary>
        [JsonPropertyName("enabledAges")]
        public List<TechAge> EnabledAges { get; set; } = new List<TechAge>
        {
            TechAge.Stone,
            TechAge.Copper
        };

        /// <summary>
        /// Configuration for blocks (ores, resources) that are restricted by age
        /// </summary>
        [JsonPropertyName("ageRestrictedBlocks")]
        public AgeRestrictedBlocksConfig AgeRestrictedBlocks { get; set; } = AgeRestrictedBlocksConfig.GetDefault();

        /// <summary>
        /// Loads tech blocks configuration from JSON file
        /// On client side, tries to load server-specific config first, falling back to global config
        /// On server side, loads the global config
        /// </summary>
        public static TechBlocksConfig LoadFromFile(ICoreAPI api, string filename = "techblocks.json", string? serverIdentifier = null)
        {
            // On client side, check for server-specific config if identifier is provided
            if (api.Side == EnumAppSide.Client && !string.IsNullOrEmpty(serverIdentifier))
            {
                var serverConfigPath = api.GetOrCreateDataPath($"ModConfig/SOAGuildsAndKingdoms/servers/{serverIdentifier}");
                var serverFilePath = Path.Combine(serverConfigPath, filename);

                if (File.Exists(serverFilePath))
                {
                    try
                    {
                        var json = File.ReadAllText(serverFilePath);
                        var options = new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true,
                            ReadCommentHandling = JsonCommentHandling.Skip,
                            AllowTrailingCommas = true
                        };

                        var config = JsonSerializer.Deserialize<TechBlocksConfig>(json, options);
                        api.Logger.Notification($"Loaded {config?.TechBlocks?.Count ?? 0} tech blocks from server-specific config (server: {serverIdentifier})");

                        if (config?.EnabledAges != null && config.EnabledAges.Count > 0)
                        {
                            api.Logger.Notification($"Enabled tech ages: {string.Join(", ", config.EnabledAges)}");
                        }

                        return config ?? new TechBlocksConfig();
                    }
                    catch (Exception ex)
                    {
                        api.Logger.Error($"Failed to load server-specific tech blocks config: {ex.Message}");
                        // Fall through to load global config
                    }
                }
            }

            // Load global config (server side or client fallback)
            var configPath = api.GetOrCreateDataPath("ModConfig/SOAGuildsAndKingdoms");
            var filePath = Path.Combine(configPath, filename);

            if (!File.Exists(filePath))
            {
                api.Logger.Warning($"Tech blocks config not found at {filePath}, creating default config");
                var defaultConfig = CreateDefaultConfig();
                defaultConfig.SaveToFile(api, filename);
                return defaultConfig;
            }

            try
            {
                var json = File.ReadAllText(filePath);
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    ReadCommentHandling = JsonCommentHandling.Skip,
                    AllowTrailingCommas = true
                };

                var config = JsonSerializer.Deserialize<TechBlocksConfig>(json, options);
                api.Logger.Notification($"Loaded {config?.TechBlocks?.Count ?? 0} tech blocks from {filename}");

                // Log enabled ages
                if (config?.EnabledAges != null && config.EnabledAges.Count > 0)
                {
                    api.Logger.Notification($"Enabled tech ages: {string.Join(", ", config.EnabledAges)}");
                }

                return config ?? new TechBlocksConfig();
            }
            catch (Exception ex)
            {
                api.Logger.Error($"Failed to load tech blocks config: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Saves config to server-specific folder (client side only)
        /// </summary>
        public static void SaveServerConfig(ICoreAPI api, string configJson, string serverIdentifier, string filename = "techblocks.json")
        {
            if (api.Side != EnumAppSide.Client)
            {
                api.Logger.Warning("SaveServerConfig should only be called on client side");
                return;
            }

            var serverConfigPath = api.GetOrCreateDataPath($"ModConfig/SOAGuildsAndKingdoms/servers/{serverIdentifier}");
            var filePath = Path.Combine(serverConfigPath, filename);

            try
            {
                File.WriteAllText(filePath, configJson);
                api.Logger.Notification($"Saved server-specific tech blocks config to {filePath}");
            }
            catch (Exception ex)
            {
                api.Logger.Error($"Failed to save server-specific tech blocks config: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Saves tech blocks configuration to JSON file
        /// </summary>
        public void SaveToFile(ICoreAPI api, string filename = "techblocks.json")
        {
            var configPath = api.GetOrCreateDataPath("ModConfig/SOAGuildsAndKingdoms");
            var filePath = Path.Combine(configPath, filename);

            try
            {
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    DefaultIgnoreCondition = JsonIgnoreCondition.Never
                };

                var json = JsonSerializer.Serialize(this, options);
                File.WriteAllText(filePath, json);
                api.Logger.Notification($"Saved tech blocks config to {filePath}");
            }
            catch (Exception ex)
            {
                api.Logger.Error($"Failed to save tech blocks config: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Checks if a specific tech age is enabled for research
        /// </summary>
        /// <param name="age">The tech age to check</param>
        /// <returns>True if the age is enabled, false otherwise</returns>
        public bool IsAgeEnabled(TechAge age)
        {
            return EnabledAges != null && EnabledAges.Contains(age);
        }

        /// <summary>
        /// Enables a tech age for research
        /// </summary>
        /// <param name="age">The tech age to enable</param>
        public void EnableAge(TechAge age)
        {
            if (EnabledAges == null)
            {
                EnabledAges = new List<TechAge>();
            }

            if (!EnabledAges.Contains(age))
            {
                EnabledAges.Add(age);
            }
        }

        /// <summary>
        /// Disables a tech age for research
        /// </summary>
        /// <param name="age">The tech age to disable</param>
        public void DisableAge(TechAge age)
        {
            if (EnabledAges != null)
            {
                EnabledAges.Remove(age);
            }
        }

        /// <summary>
        /// Creates a default configuration with example tech blocks
        /// </summary>
        public static TechBlocksConfig CreateDefaultConfig()
        {
            return new TechBlocksConfig
            {
                TechBlocks = new List<TechBlock>
                {
                    new TechBlock
                    {
                        Id = 1,
                        Text = "Copper Age",
                        Description = "Learn how to melt copper and create ingots",
                        Level = 2,
                        Age = TechAge.Stone,
                        ResourceGroups = new List<ResourceGroup>
                        {
                            new ResourceGroup
                            {
                                Name = "Copper Bits or Nuggets",
                                AmountRequired = 20,
                                ResourcePatterns = new List<string> { "nugget-nativecopper", "metalbit-copper" }
                            },
                            new ResourceGroup
                            {
                                Name = "Any Raw Hides",
                                AmountRequired = 5,
                                ResourcePatterns = new List<string> { "hide-raw-*" }
                            },
                            new ResourceGroup
                            {
                                Name = "Any Clay",
                                AmountRequired = 20,
                                ResourcePatterns = new List<string> { "clay-*" }
                            },
                            new ResourceGroup
                            {
                                Name = "Firewood",
                                AmountRequired = 40,
                                ResourcePatterns = new List<string> { "firewood" }
                            }
                        },
                        UnlocksIds = new List<int> { 2 },
                        GrantedTraits = new List<string> { "CopperAge" }
                    },
                    new TechBlock
                    {
                        Id = 2,
                        Text = "Tin Bronze Age",
                        Description = "Advance to the Tin Bronze Age",
                        Level = 2,
                        Age = TechAge.Copper,
                        ResourceGroups = new List<ResourceGroup>
                        {
                            new ResourceGroup
                            {
                                Name = "Copper Ingots",
                                AmountRequired = 10,
                                ResourcePatterns = new List<string> { "ingot-copper" }
                            },
                            new ResourceGroup
                            {
                                Name = "Any Raw Hides",
                                AmountRequired = 10,
                                ResourcePatterns = new List<string> { "hide-raw-*" }
                            },
                            new ResourceGroup
                            {
                                Name = "Any Clay",
                                AmountRequired = 20,
                                ResourcePatterns = new List<string> { "clay-*" }
                            },
                            new ResourceGroup
                            {
                                Name = "Cupronickel Nails and Strips",
                                AmountRequired = 20,
                                ResourcePatterns = new List<string> { "metalnailsandstrips-cupronickel" }
                            },
                            new ResourceGroup
                            {
                                Name = "Firewood",
                                AmountRequired = 64,
                                ResourcePatterns = new List<string> { "firewood" }
                            }
                        },
                        UnlocksIds = new List<int> { 3 },
                        GrantedTraits = new List<string> { "BronzeAge" }
                    },
                    new TechBlock
                    {
                        Id = 3,
                        Text = "Bismuth and Black Bronze Age",
                        Description = "Learn advanced bronze alloys",
                        Level = 2,
                        Age = TechAge.Bronze,
                        ResourceGroups = new List<ResourceGroup>
                        {
                            new ResourceGroup
                            {
                                Name = "Copper Ingots",
                                AmountRequired = 15,
                                ResourcePatterns = new List<string> { "ingot-copper" }
                            },
                            new ResourceGroup
                            {
                                Name = "Tin Ingots",
                                AmountRequired = 15,
                                ResourcePatterns = new List<string> { "ingot-tinbronze" }
                            },
                            new ResourceGroup
                            {
                                Name = "Leather",
                                AmountRequired = 10,
                                ResourcePatterns = new List<string> { "leather-normal-plain" }
                            },
                            new ResourceGroup
                            {
                                Name = "Any Clay",
                                AmountRequired = 20,
                                ResourcePatterns = new List<string> { "clay-*" }
                            },
                            new ResourceGroup
                            {
                                Name = "Cupronickel Nails and Strips",
                                AmountRequired = 25,
                                ResourcePatterns = new List<string> { "metalnailsandstrips-cupronickel" }
                            },
                            new ResourceGroup
                            {
                                Name = "Flax Twine",
                                AmountRequired = 10,
                                ResourcePatterns = new List<string> { "flaxtwine" }
                            },
                            new ResourceGroup
                            {
                                Name = "Charcoal",
                                AmountRequired = 40,
                                ResourcePatterns = new List<string> { "charcoal" }
                            }
                        },
                        UnlocksIds = new List<int> { 4 },
                        GrantedTraits = new List<string> { "AdvBronzeAge" }
                    },
                    new TechBlock
                    {
                        Id = 4,
                        Text = "Iron Age",
                        Description = "Learn the Iron Age",
                        Level = 2,
                        Age = TechAge.OtherBronze,
                        ResourceGroups = new List<ResourceGroup>
                        {
                            new ResourceGroup
                            {
                                Name = "Any Bronze Ingots",
                                AmountRequired = 20,
                                ResourcePatterns = new List<string> { "ingot-*bronze" }
                            },
                            new ResourceGroup
                            {
                                Name = "Leather",
                                AmountRequired = 10,
                                ResourcePatterns = new List<string> { "leather-normal-plain" }
                            },
                            new ResourceGroup
                            {
                                Name = "Fire Clay",
                                AmountRequired = 20,
                                ResourcePatterns = new List<string> { "clay-fire" }
                            },
                            new ResourceGroup
                            {
                                Name = "Cupronickel Nails and Strips",
                                AmountRequired = 25,
                                ResourcePatterns = new List<string> { "metalnailsandstrips-cupronickel" }
                            },
                            new ResourceGroup
                            {
                                Name = "Flax Twine",
                                AmountRequired = 15,
                                ResourcePatterns = new List<string> { "flaxtwine" }
                            },
                            new ResourceGroup
                            {
                                Name = "Feathers",
                                AmountRequired = 10,
                                ResourcePatterns = new List<string> { "feathers" }
                            },
                            new ResourceGroup
                            {
                                Name = "Gold Ingots",
                                AmountRequired = 5,
                                ResourcePatterns = new List<string> { "ingot-gold" }
                            },
                            new ResourceGroup
                            {
                                Name = "Silver Ingots",
                                AmountRequired = 5,
                                ResourcePatterns = new List<string> { "ingot-silver" }
                            },
                            new ResourceGroup
                            {
                                Name = "Charcoal",
                                AmountRequired = 64,
                                ResourcePatterns = new List<string> { "charcoal" }
                            }
                        },
                        UnlocksIds = new List<int> { 5 },
                        GrantedTraits = new List<string> { "IronAge" }
                    },
                    new TechBlock
                    {
                        Id = 5,
                        Text = "Meteoric Iron",
                        Description = "Space Metal Age",
                        Level = 2,
                        Age = TechAge.Iron,
                        ResourceGroups = new List<ResourceGroup>
                        {
                            new ResourceGroup
                            {
                                Name = "Iron Ingots",
                                AmountRequired = 25,
                                ResourcePatterns = new List<string> { "ingot-iron" }
                            },
                            new ResourceGroup
                            {
                                Name = "Leather",
                                AmountRequired = 15,
                                ResourcePatterns = new List<string> { "leather-normal-plain" }
                            },
                            new ResourceGroup
                            {
                                Name = "Fire Clay",
                                AmountRequired = 25,
                                ResourcePatterns = new List<string> { "clay-fire" }
                            },
                            new ResourceGroup
                            {
                                Name = "Cupronickel Nails and Strips",
                                AmountRequired = 25,
                                ResourcePatterns = new List<string> { "metalnailsandstrips-cupronickel" }
                            },
                            new ResourceGroup
                            {
                                Name = "Flax Twine",
                                AmountRequired = 20,
                                ResourcePatterns = new List<string> { "flaxtwine" }
                            },
                            new ResourceGroup
                            {
                                Name = "Feathers",
                                AmountRequired = 10,
                                ResourcePatterns = new List<string> { "feathers" }
                            },
                            new ResourceGroup
                            {
                                Name = "Gold Ingots",
                                AmountRequired = 5,
                                ResourcePatterns = new List<string> { "ingot-gold" }
                            },
                            new ResourceGroup
                            {
                                Name = "Silver Ingots",
                                AmountRequired = 5,
                                ResourcePatterns = new List<string> { "ingot-silver" }
                            },
                            new ResourceGroup
                            {
                                Name = "Temporal Gears",
                                AmountRequired = 10,
                                ResourcePatterns = new List<string> { "gear-temporal" }
                            },
                            new ResourceGroup
                            {
                                Name = "Coke",
                                AmountRequired = 40,
                                ResourcePatterns = new List<string> { "coke" }
                            }
                        },
                        UnlocksIds = new List<int> { 6 },
                        GrantedTraits = new List<string> { "MeteoricIronAge" }
                    },
                    new TechBlock
                    {
                        Id = 6,
                        Text = "Steel Age",
                        Description = "The final stage of the game (unless..?)",
                        Level = 2,
                        Age = TechAge.MeteoricIron,
                        ResourceGroups = new List<ResourceGroup>
                        {
                            new ResourceGroup
                            {
                                Name = "Iron Ingots",
                                AmountRequired = 30,
                                ResourcePatterns = new List<string> { "ingot-iron" }
                            },
                            new ResourceGroup
                            {
                                Name = "Meteoric Iron Ingots",
                                AmountRequired = 5,
                                ResourcePatterns = new List<string> { "ingot-meteoriciron" }
                            },
                            new ResourceGroup
                            {
                                Name = "Leather",
                                AmountRequired = 20,
                                ResourcePatterns = new List<string> { "leather-normal-plain" }
                            },
                            new ResourceGroup
                            {
                                Name = "Fire Clay",
                                AmountRequired = 25,
                                ResourcePatterns = new List<string> { "clay-fire" }
                            },
                            new ResourceGroup
                            {
                                Name = "Cupronickel Nails and Strips",
                                AmountRequired = 25,
                                ResourcePatterns = new List<string> { "metalnailsandstrips-cupronickel" }
                            },
                            new ResourceGroup
                            {
                                Name = "Flax Twine",
                                AmountRequired = 25,
                                ResourcePatterns = new List<string> { "flaxtwine" }
                            },
                            new ResourceGroup
                            {
                                Name = "Feathers",
                                AmountRequired = 25,
                                ResourcePatterns = new List<string> { "feathers" }
                            },
                            new ResourceGroup
                            {
                                Name = "Gold Ingots",
                                AmountRequired = 5,
                                ResourcePatterns = new List<string> { "ingot-gold" }
                            },
                            new ResourceGroup
                            {
                                Name = "Silver Ingots",
                                AmountRequired = 5,
                                ResourcePatterns = new List<string> { "ingot-silver" }
                            },
                            new ResourceGroup
                            {
                                Name = "Temporal Gears",
                                AmountRequired = 15,
                                ResourcePatterns = new List<string> { "gear-temporal" }
                            },
                            new ResourceGroup
                            {
                                Name = "Coke",
                                AmountRequired = 64,
                                ResourcePatterns = new List<string> { "coke" }
                            },
                            new ResourceGroup
                            {
                                Name = "Crushed Bauxite",
                                AmountRequired = 10,
                                ResourcePatterns = new List<string> { "crushed-bauxite" }
                            },
                            new ResourceGroup
                            {
                                Name = "Crushed Olivine",
                                AmountRequired = 10,
                                ResourcePatterns = new List<string> { "crushed-olivine" }
                            }
                        },
                        UnlocksIds = new List<int> { 7 },
                        GrantedTraits = new List<string> { "SteelAge" }
                    },
                    new TechBlock
                    {
                        Id = 7,
                        Text = "Meteoric Steel Age",
                        Description = "",
                        Level = 2,
                        Age = TechAge.Steel,
                        ResourceGroups = new List<ResourceGroup>
                        {
                            new ResourceGroup
                            {
                                Name = "Steel Ingots",
                                AmountRequired = 35,
                                ResourcePatterns = new List<string> { "ingot-steel" }
                            },
                            new ResourceGroup
                            {
                                Name = "T3 Refractory Bricks",
                                AmountRequired = 15,
                                ResourcePatterns = new List<string> { "refractorybrick-fired-tier3" }
                            },
                            new ResourceGroup
                            {
                                Name = "Sturdy Leather",
                                AmountRequired = 20,
                                ResourcePatterns = new List<string> { "leather-normal-plain" }
                            },
                            new ResourceGroup
                            {
                                Name = "Cupronickel Nails and Strips",
                                AmountRequired = 35,
                                ResourcePatterns = new List<string> { "metalnailsandstrips-cupronickel" }
                            },
                            new ResourceGroup
                            {
                                Name = "Flax Twine",
                                AmountRequired = 35,
                                ResourcePatterns = new List<string> { "flaxtwine" }
                            },
                            new ResourceGroup
                            {
                                Name = "Feathers",
                                AmountRequired = 35,
                                ResourcePatterns = new List<string> { "feathers" }
                            },
                            new ResourceGroup
                            {
                                Name = "Gold Ingots",
                                AmountRequired = 10,
                                ResourcePatterns = new List<string> { "ingot-gold" }
                            },
                            new ResourceGroup
                            {
                                Name = "Silver Ingots",
                                AmountRequired = 10,
                                ResourcePatterns = new List<string> { "ingot-silver" }
                            },
                            new ResourceGroup
                            {
                                Name = "Temporal Gears",
                                AmountRequired = 20,
                                ResourcePatterns = new List<string> { "gear-temporal" }
                            },
                            new ResourceGroup
                            {
                                Name = "Coke",
                                AmountRequired = 96,
                                ResourcePatterns = new List<string> { "coke" }
                            },
                            new ResourceGroup
                            {
                                Name = "Crushed Bauxite",
                                AmountRequired = 15,
                                ResourcePatterns = new List<string> { "crushed-bauxite" }
                            },
                            new ResourceGroup
                            {
                                Name = "Crushed Olivine",
                                AmountRequired = 15,
                                ResourcePatterns = new List<string> { "crushed-olivine" }
                            },
                            new ResourceGroup
                            {
                                Name = "Crushed Ilmenite",
                                AmountRequired = 15,
                                ResourcePatterns = new List<string> { "crushed-ilmenite" }
                            },
                            new ResourceGroup
                            {
                                Name = "Platinum Ingots",
                                AmountRequired = 10,
                                ResourcePatterns = new List<string> { "ingot-platinum" }
                            }

                        },
                        UnlocksIds = new List<int> { },
                        GrantedTraits = new List<string> { "MeteoricSteelAge" }
                    }
                }
            };
        }

        /// <summary>
        /// Validates all tech blocks in the configuration
        /// </summary>
        public bool Validate(ICoreAPI api)
        {
            if (TechBlocks == null || TechBlocks.Count == 0)
            {
                api.Logger.Error("Tech blocks configuration is empty");
                return false;
            }

            var ids = new HashSet<int>();
            var isValid = true;

            foreach (var tech in TechBlocks)
            {
                // Check for duplicate IDs
                if (ids.Contains(tech.Id))
                {
                    api.Logger.Error($"Duplicate tech block ID: {tech.Id}");
                    isValid = false;
                }
                ids.Add(tech.Id);

                // Validate resource requirements
                if (!tech.ValidateResourceRequirements())
                {
                    api.Logger.Error($"Invalid resource requirements in tech block {tech.Id} ({tech.Text})");
                    isValid = false;
                }

                // Check for circular dependencies
                foreach (var unlocksId in tech.UnlocksIds)
                {
                    if (unlocksId == tech.Id)
                    {
                        api.Logger.Error($"Tech block {tech.Id} ({tech.Text}) unlocks itself");
                        isValid = false;
                    }
                }
            }

            // Validate UnlocksIds reference existing techs
            foreach (var tech in TechBlocks)
            {
                foreach (var unlocksId in tech.UnlocksIds)
                {
                    if (!ids.Contains(unlocksId))
                    {
                        api.Logger.Warning($"Tech block {tech.Id} ({tech.Text}) references non-existent tech {unlocksId}");
                    }
                }
            }

            if (isValid)
                api.Logger.Notification("Tech blocks configuration validated successfully");

            return isValid;
        }
    }
}

