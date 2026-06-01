using System.Collections.Generic;
using Vintagestory.API.Util;

namespace SRGuildsAndKingdoms.src.techblock
{
    /// <summary>
    /// Represents the requirements for accessing a restricted block
    /// </summary>
    public class BlockRestriction
    {
        public TechAge RequiredAge { get; set; }
        public string RequiredTrait { get; set; }
    }

    /// <summary>
    /// Configuration for age-restricted blocks (ores, resources, etc.)
    /// </summary>
    public class AgeRestrictedBlocksConfig
    {
        /// <summary>
        /// Maps block codes (or wildcards) to their age and trait requirements
        /// </summary>
        public Dictionary<string, BlockRestriction> BlockRestrictions { get; set; } = new Dictionary<string, BlockRestriction>();

        /// <summary>
        /// Default configuration with common ore restrictions
        /// </summary>
        public static AgeRestrictedBlocksConfig GetDefault()
        {
            return new AgeRestrictedBlocksConfig
            {
                BlockRestrictions = new Dictionary<string, BlockRestriction>
                {
                    // Stone Age - Basic resources (no trait required)
                    { "game:rock-*", new BlockRestriction { RequiredAge = TechAge.Stone, RequiredTrait = null } },
                    { "game:clay-*", new BlockRestriction { RequiredAge = TechAge.Stone, RequiredTrait = null } },

                    // Copper Age - Copper ores (requires CopperAge trait)
                    { "game:ore-*-nativecopper-*", new BlockRestriction { RequiredAge = TechAge.Copper, RequiredTrait = "CopperAge" } },
                    { "game:ore-*-malachite-*", new BlockRestriction { RequiredAge = TechAge.Copper, RequiredTrait = "CopperAge" } },

                    // Bronze Age - Tin ores (requires BronzeAge trait)
                    { "game:ore-*-cassiterite-*", new BlockRestriction { RequiredAge = TechAge.Bronze, RequiredTrait = "BronzeAge" } },

                    // Other Bronze Age - Advanced bronze alloy ores (requires AdvBronzeAge trait)
                    { "game:ore-*-bismuthinite-*", new BlockRestriction { RequiredAge = TechAge.OtherBronze, RequiredTrait = "AdvBronzeAge" } },
                    { "game:ore-*-sphalerite-*", new BlockRestriction { RequiredAge = TechAge.OtherBronze, RequiredTrait = "AdvBronzeAge" } },
                    { "game:ore-*-quartz_nativegold-*", new BlockRestriction { RequiredAge = TechAge.OtherBronze, RequiredTrait = "AdvBronzeAge" } },
                    { "game:ore-*-quartz_nativesilver-*", new BlockRestriction { RequiredAge = TechAge.OtherBronze, RequiredTrait = "AdvBronzeAge" } },
                    { "game:ore-*-galena-*", new BlockRestriction { RequiredAge = TechAge.OtherBronze, RequiredTrait = "AdvBronzeAge" } },

                    // Iron Age - Iron ores (requires IronAge trait)
                    { "game:ore-*-hematite-*", new BlockRestriction { RequiredAge = TechAge.Iron, RequiredTrait = "IronAge" } },
                    { "game:ore-*-limonite-*", new BlockRestriction { RequiredAge = TechAge.Iron, RequiredTrait = "IronAge" } },
                    { "game:ore-*-magnetite-*", new BlockRestriction { RequiredAge = TechAge.Iron, RequiredTrait = "IronAge" } },

                    // Meteoric Iron Age - Meteoric iron (requires MeteoricIronAge trait)
                    { "game:meteorite-iron", new BlockRestriction { RequiredAge = TechAge.MeteoricIron, RequiredTrait = "MeteoricIronAge" } },

                    // Steel Age - Olivine and Bauxite for steel (requires SteelAge trait)
                    { "game:ore-olivine-*", new BlockRestriction { RequiredAge = TechAge.Steel, RequiredTrait = "SteelAge" } },
                    { "game:rock-bauxite", new BlockRestriction { RequiredAge = TechAge.Steel, RequiredTrait = "SteelAge" } },

                    // Meteoric Steel Age - Rhodochrosite for meteoric steel (requires MeteoricSteelAge trait)
					{ "game:ore-rhodochrosite-*", new BlockRestriction { RequiredAge = TechAge.MeteoricSteel, RequiredTrait = "MeteoricSteelAge" } }
                }
            };
        }

        /// <summary>
        /// Checks if a block code matches any of the restricted patterns
        /// </summary>
        public bool IsBlockRestricted(string blockCode, out TechAge requiredAge, out string requiredTrait)
        {
            requiredAge = TechAge.Stone;
            requiredTrait = null;

            foreach (var restriction in BlockRestrictions)
            {
                if (WildcardUtil.Match(restriction.Key, blockCode))
                {
                    requiredAge = restriction.Value.RequiredAge;
                    requiredTrait = restriction.Value.RequiredTrait;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Checks if a block code matches any of the restricted patterns (legacy method)
        /// </summary>
        public bool IsBlockRestricted(string blockCode, out TechAge requiredAge)
        {
            return IsBlockRestricted(blockCode, out requiredAge, out _);
        }
    }
}
