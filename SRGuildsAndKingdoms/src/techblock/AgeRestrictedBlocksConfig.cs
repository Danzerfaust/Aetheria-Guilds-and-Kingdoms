using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Vintagestory.API.Util;

namespace SRGuildsAndKingdoms.src.techblock
{
	// Token: 0x02000007 RID: 7
	[NullableContext(1)]
	[Nullable(0)]
	public class AgeRestrictedBlocksConfig
	{
		// Token: 0x1700000F RID: 15
		// (get) Token: 0x06000073 RID: 115 RVA: 0x000095CA File Offset: 0x000077CA
		// (set) Token: 0x06000074 RID: 116 RVA: 0x000095D2 File Offset: 0x000077D2
		public Dictionary<string, BlockRestriction> BlockRestrictions { get; set; } = new Dictionary<string, BlockRestriction>();

		// Token: 0x06000075 RID: 117 RVA: 0x000095DC File Offset: 0x000077DC
		public static AgeRestrictedBlocksConfig GetDefault()
		{
			return new AgeRestrictedBlocksConfig
			{
				BlockRestrictions = new Dictionary<string, BlockRestriction>
				{
					{
						"game:rock-*",
						new BlockRestriction
						{
							RequiredAge = TechAge.Stone,
							RequiredTrait = null
						}
					},
					{
						"game:clay-*",
						new BlockRestriction
						{
							RequiredAge = TechAge.Stone,
							RequiredTrait = null
						}
					},
					{
						"game:ore-*-nativecopper-*",
						new BlockRestriction
						{
							RequiredAge = TechAge.Copper,
							RequiredTrait = "CopperAge"
						}
					},
					{
						"game:ore-*-malachite-*",
						new BlockRestriction
						{
							RequiredAge = TechAge.Copper,
							RequiredTrait = "CopperAge"
						}
					},
					{
						"game:ore-*-cassiterite-*",
						new BlockRestriction
						{
							RequiredAge = TechAge.Bronze,
							RequiredTrait = "BronzeAge"
						}
					},
					{
						"game:ore-*-bismuthinite-*",
						new BlockRestriction
						{
							RequiredAge = TechAge.OtherBronze,
							RequiredTrait = "AdvBronzeAge"
						}
					},
					{
						"game:ore-*-sphalerite-*",
						new BlockRestriction
						{
							RequiredAge = TechAge.OtherBronze,
							RequiredTrait = "AdvBronzeAge"
						}
					},
					{
						"game:ore-*-quartz_nativegold-*",
						new BlockRestriction
						{
							RequiredAge = TechAge.OtherBronze,
							RequiredTrait = "AdvBronzeAge"
						}
					},
					{
						"game:ore-*-quartz_nativesilver-*",
						new BlockRestriction
						{
							RequiredAge = TechAge.OtherBronze,
							RequiredTrait = "AdvBronzeAge"
						}
					},
					{
						"game:ore-*-galena-*",
						new BlockRestriction
						{
							RequiredAge = TechAge.OtherBronze,
							RequiredTrait = "AdvBronzeAge"
						}
					},
					{
						"game:ore-*-hematite-*",
						new BlockRestriction
						{
							RequiredAge = TechAge.Iron,
							RequiredTrait = "IronAge"
						}
					},
					{
						"game:ore-*-limonite-*",
						new BlockRestriction
						{
							RequiredAge = TechAge.Iron,
							RequiredTrait = "IronAge"
						}
					},
					{
						"game:ore-*-magnetite-*",
						new BlockRestriction
						{
							RequiredAge = TechAge.Iron,
							RequiredTrait = "IronAge"
						}
					},
					{
						"game:meteorite-iron",
						new BlockRestriction
						{
							RequiredAge = TechAge.MeteoricIron,
							RequiredTrait = "MeteoricIronAge"
						}
					},
					{
						"game:ore-olivine-*",
						new BlockRestriction
						{
							RequiredAge = TechAge.Steel,
							RequiredTrait = "SteelAge"
						}
					},
					{
						"game:rock-bauxite",
						new BlockRestriction
						{
							RequiredAge = TechAge.Steel,
							RequiredTrait = "SteelAge"
						}
					},
					{
						"game:ore-rhodochrosite-*",
						new BlockRestriction
						{
							RequiredAge = TechAge.MeteoricSteel,
							RequiredTrait = "MeteoricSteelAge"
						}
					}
				}
			};
		}

		// Token: 0x06000076 RID: 118 RVA: 0x00009834 File Offset: 0x00007A34
		public bool IsBlockRestricted(string blockCode, out TechAge requiredAge, out string requiredTrait)
		{
			requiredAge = TechAge.Stone;
			requiredTrait = null;
			foreach (KeyValuePair<string, BlockRestriction> restriction in this.BlockRestrictions)
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

		// Token: 0x06000077 RID: 119 RVA: 0x000098B8 File Offset: 0x00007AB8
		public bool IsBlockRestricted(string blockCode, out TechAge requiredAge)
		{
			string text;
			return this.IsBlockRestricted(blockCode, out requiredAge, out text);
		}
	}
}
