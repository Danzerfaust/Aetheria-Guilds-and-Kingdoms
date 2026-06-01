using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Vintagestory.API.MathTools;

namespace SRGuildsAndKingdoms.src.config
{
	// Token: 0x020000C0 RID: 192
	[NullableContext(1)]
	[Nullable(0)]
	public class GuildConfig
	{
		// Token: 0x1700025E RID: 606
		// (get) Token: 0x06000928 RID: 2344 RVA: 0x000429AC File Offset: 0x00040BAC
		// (set) Token: 0x06000929 RID: 2345 RVA: 0x000429B4 File Offset: 0x00040BB4
		public string ServerName { get; set; } = "MyVintageStoryServer";

		// Token: 0x1700025F RID: 607
		// (get) Token: 0x0600092A RID: 2346 RVA: 0x000429BD File Offset: 0x00040BBD
		// (set) Token: 0x0600092B RID: 2347 RVA: 0x000429C5 File Offset: 0x00040BC5
		public int MaxMembersPerGuild { get; set; } = 40;

		// Token: 0x17000260 RID: 608
		// (get) Token: 0x0600092C RID: 2348 RVA: 0x000429CE File Offset: 0x00040BCE
		// (set) Token: 0x0600092D RID: 2349 RVA: 0x000429D6 File Offset: 0x00040BD6
		public int BaseMaxClaimsPerGuild { get; set; } = 20;

		// Token: 0x17000261 RID: 609
		// (get) Token: 0x0600092E RID: 2350 RVA: 0x000429DF File Offset: 0x00040BDF
		// (set) Token: 0x0600092F RID: 2351 RVA: 0x000429E7 File Offset: 0x00040BE7
		public bool EnableDynamicClaimLimits { get; set; } = true;

		// Token: 0x17000262 RID: 610
		// (get) Token: 0x06000930 RID: 2352 RVA: 0x000429F0 File Offset: 0x00040BF0
		// (set) Token: 0x06000931 RID: 2353 RVA: 0x000429F8 File Offset: 0x00040BF8
		public List<PlayerCountThreshold> PlayerCountThresholds { get; set; } = new List<PlayerCountThreshold>
		{
			new PlayerCountThreshold
			{
				MinPlayerCount = 5,
				AdditionalClaims = 10
			},
			new PlayerCountThreshold
			{
				MinPlayerCount = 10,
				AdditionalClaims = 10
			},
			new PlayerCountThreshold
			{
				MinPlayerCount = 15,
				AdditionalClaims = 10
			},
			new PlayerCountThreshold
			{
				MinPlayerCount = 20,
				AdditionalClaims = 10
			},
			new PlayerCountThreshold
			{
				MinPlayerCount = 25,
				AdditionalClaims = 10
			},
			new PlayerCountThreshold
			{
				MinPlayerCount = 30,
				AdditionalClaims = 10
			},
			new PlayerCountThreshold
			{
				MinPlayerCount = 35,
				AdditionalClaims = 10
			},
			new PlayerCountThreshold
			{
				MinPlayerCount = 40,
				AdditionalClaims = 10
			}
		};

		// Token: 0x17000263 RID: 611
		// (get) Token: 0x06000932 RID: 2354 RVA: 0x00042A01 File Offset: 0x00040C01
		// (set) Token: 0x06000933 RID: 2355 RVA: 0x00042A09 File Offset: 0x00040C09
		public int AbsoluteMaxClaimsPerGuild { get; set; } = 100;

		// Token: 0x17000264 RID: 612
		// (get) Token: 0x06000934 RID: 2356 RVA: 0x00042A12 File Offset: 0x00040C12
		// (set) Token: 0x06000935 RID: 2357 RVA: 0x00042A1A File Offset: 0x00040C1A
		public int BaseMaxOutpostsPerGuild { get; set; }

		// Token: 0x17000265 RID: 613
		// (get) Token: 0x06000936 RID: 2358 RVA: 0x00042A23 File Offset: 0x00040C23
		// (set) Token: 0x06000937 RID: 2359 RVA: 0x00042A2B File Offset: 0x00040C2B
		public bool EnableDynamicOutpostLimits { get; set; } = true;

		// Token: 0x17000266 RID: 614
		// (get) Token: 0x06000938 RID: 2360 RVA: 0x00042A34 File Offset: 0x00040C34
		// (set) Token: 0x06000939 RID: 2361 RVA: 0x00042A3C File Offset: 0x00040C3C
		public List<PlayerCountThreshold> OutpostPlayerCountThresholds { get; set; } = new List<PlayerCountThreshold>
		{
			new PlayerCountThreshold
			{
				MinPlayerCount = 5,
				AdditionalClaims = 1
			},
			new PlayerCountThreshold
			{
				MinPlayerCount = 10,
				AdditionalClaims = 0
			},
			new PlayerCountThreshold
			{
				MinPlayerCount = 15,
				AdditionalClaims = 1
			},
			new PlayerCountThreshold
			{
				MinPlayerCount = 20,
				AdditionalClaims = 0
			},
			new PlayerCountThreshold
			{
				MinPlayerCount = 25,
				AdditionalClaims = 1
			}
		};

		// Token: 0x17000267 RID: 615
		// (get) Token: 0x0600093A RID: 2362 RVA: 0x00042A45 File Offset: 0x00040C45
		// (set) Token: 0x0600093B RID: 2363 RVA: 0x00042A4D File Offset: 0x00040C4D
		public int AbsoluteMaxOutpostsPerGuild { get; set; } = 3;

		// Token: 0x17000268 RID: 616
		// (get) Token: 0x0600093C RID: 2364 RVA: 0x00042A56 File Offset: 0x00040C56
		// (set) Token: 0x0600093D RID: 2365 RVA: 0x00042A5E File Offset: 0x00040C5E
		public int GuildRejoinCooldownDays { get; set; } = 3;

		// Token: 0x17000269 RID: 617
		// (get) Token: 0x0600093E RID: 2366 RVA: 0x00042A67 File Offset: 0x00040C67
		// (set) Token: 0x0600093F RID: 2367 RVA: 0x00042A6F File Offset: 0x00040C6F
		public int GuildDisbandCooldownDays { get; set; } = 1;

		// Token: 0x1700026A RID: 618
		// (get) Token: 0x06000940 RID: 2368 RVA: 0x00042A78 File Offset: 0x00040C78
		// (set) Token: 0x06000941 RID: 2369 RVA: 0x00042A80 File Offset: 0x00040C80
		public Dictionary<string, int> ClassThresholds { get; set; } = new Dictionary<string, int>
		{
			{
				"C",
				400
			},
			{
				"B",
				900
			},
			{
				"A",
				1400
			},
			{
				"S",
				2000
			}
		};

		// Token: 0x1700026B RID: 619
		// (get) Token: 0x06000942 RID: 2370 RVA: 0x00042A89 File Offset: 0x00040C89
		// (set) Token: 0x06000943 RID: 2371 RVA: 0x00042A91 File Offset: 0x00040C91
		public Dictionary<string, int> MemberRankThresholds { get; set; } = new Dictionary<string, int>
		{
			{
				"Jr Shadow Knight 3rd Class",
				150
			},
			{
				"Jr Shadow Knight 2nd Class",
				200
			},
			{
				"Jr Shadow Knight 1st Class",
				250
			},
			{
				"Sr Shadow Knight 3rd Class",
				300
			},
			{
				"Sr Shadow Knight 2nd Class",
				400
			},
			{
				"Sr Shadow Knight 1st Class",
				450
			},
			{
				"Grand Shadow Knight",
				500
			}
		};

		// Token: 0x1700026C RID: 620
		// (get) Token: 0x06000944 RID: 2372 RVA: 0x00042A9A File Offset: 0x00040C9A
		// (set) Token: 0x06000945 RID: 2373 RVA: 0x00042AA2 File Offset: 0x00040CA2
		public CurrencyDefinition QuestTailsDefinition { get; set; } = new CurrencyDefinition
		{
			Code = "coinage:planchet-molybdochalkos-md",
			Nbt = null
		};

		// Token: 0x1700026D RID: 621
		// (get) Token: 0x06000946 RID: 2374 RVA: 0x00042AAB File Offset: 0x00040CAB
		// (set) Token: 0x06000947 RID: 2375 RVA: 0x00042AB3 File Offset: 0x00040CB3
		public CurrencyDefinition QuestCrownsDefinition { get; set; } = new CurrencyDefinition
		{
			Code = "coinage:planchet-platinum-md",
			Nbt = null
		};

		// Token: 0x06000948 RID: 2376 RVA: 0x00042ABC File Offset: 0x00040CBC
		public string GetGuildRankClass(int points)
		{
			string rankClass = "D";
			int highestThreshold = 0;
			foreach (KeyValuePair<string, int> threshold in this.ClassThresholds)
			{
				if (points >= threshold.Value && threshold.Value >= highestThreshold)
				{
					rankClass = threshold.Key;
					highestThreshold = threshold.Value;
				}
			}
			return rankClass;
		}

		// Token: 0x06000949 RID: 2377 RVA: 0x00042B38 File Offset: 0x00040D38
		public string GetMemberRank(int pointsContribution)
		{
			string rank = "Guild Member";
			int highestThreshold = 0;
			foreach (KeyValuePair<string, int> threshold in this.MemberRankThresholds)
			{
				if (pointsContribution >= threshold.Value && threshold.Value >= highestThreshold)
				{
					rank = threshold.Key;
					highestThreshold = threshold.Value;
				}
			}
			return rank;
		}

		// Token: 0x1700026E RID: 622
		// (get) Token: 0x0600094A RID: 2378 RVA: 0x00042BB4 File Offset: 0x00040DB4
		// (set) Token: 0x0600094B RID: 2379 RVA: 0x00042BBC File Offset: 0x00040DBC
		public bool EnableTerritorialRestrictions { get; set; }

		// Token: 0x1700026F RID: 623
		// (get) Token: 0x0600094C RID: 2380 RVA: 0x00042BC5 File Offset: 0x00040DC5
		// (set) Token: 0x0600094D RID: 2381 RVA: 0x00042BCD File Offset: 0x00040DCD
		[Nullable(2)]
		public ClaimRestrictionCenter TerritorialCenter { [NullableContext(2)] get; [NullableContext(2)] set; }

		// Token: 0x17000270 RID: 624
		// (get) Token: 0x0600094E RID: 2382 RVA: 0x00042BD6 File Offset: 0x00040DD6
		// (set) Token: 0x0600094F RID: 2383 RVA: 0x00042BDE File Offset: 0x00040DDE
		public int TerritorialRadius { get; set; } = 1000;

		// Token: 0x17000271 RID: 625
		// (get) Token: 0x06000950 RID: 2384 RVA: 0x00042BE7 File Offset: 0x00040DE7
		// (set) Token: 0x06000951 RID: 2385 RVA: 0x00042BEF File Offset: 0x00040DEF
		public bool EnableProtectedZones { get; set; }

		// Token: 0x17000272 RID: 626
		// (get) Token: 0x06000952 RID: 2386 RVA: 0x00042BF8 File Offset: 0x00040DF8
		// (set) Token: 0x06000953 RID: 2387 RVA: 0x00042C00 File Offset: 0x00040E00
		public List<ProtectedZone> ProtectedZones { get; set; } = new List<ProtectedZone>();

		// Token: 0x17000273 RID: 627
		// (get) Token: 0x06000954 RID: 2388 RVA: 0x00042C09 File Offset: 0x00040E09
		// (set) Token: 0x06000955 RID: 2389 RVA: 0x00042C11 File Offset: 0x00040E11
		public bool EnableNodes { get; set; }

		// Token: 0x06000956 RID: 2390 RVA: 0x00042C1C File Offset: 0x00040E1C
		public int CalculateMaxClaimsPerGuild(int currentPlayerCount)
		{
			if (!this.EnableDynamicClaimLimits)
			{
				return this.BaseMaxClaimsPerGuild;
			}
			int maxClaims = this.BaseMaxClaimsPerGuild;
			foreach (PlayerCountThreshold threshold in this.PlayerCountThresholds)
			{
				if (currentPlayerCount < threshold.MinPlayerCount)
				{
					break;
				}
				maxClaims += threshold.AdditionalClaims;
			}
			return Math.Min(maxClaims, this.AbsoluteMaxClaimsPerGuild);
		}

		// Token: 0x06000957 RID: 2391 RVA: 0x00042C9C File Offset: 0x00040E9C
		public int CalculateMaxOutpostsPerGuild(int currentPlayerCount)
		{
			if (!this.EnableDynamicOutpostLimits)
			{
				return this.BaseMaxOutpostsPerGuild;
			}
			int maxOutposts = this.BaseMaxOutpostsPerGuild;
			foreach (PlayerCountThreshold threshold in this.OutpostPlayerCountThresholds)
			{
				if (currentPlayerCount < threshold.MinPlayerCount)
				{
					break;
				}
				maxOutposts += threshold.AdditionalClaims;
			}
			return Math.Min(maxOutposts, this.AbsoluteMaxOutpostsPerGuild);
		}

		// Token: 0x06000958 RID: 2392 RVA: 0x00042D1C File Offset: 0x00040F1C
		[NullableContext(2)]
		public PlayerCountThreshold GetNextThreshold(int currentPlayerCount)
		{
			foreach (PlayerCountThreshold threshold in this.PlayerCountThresholds)
			{
				if (currentPlayerCount < threshold.MinPlayerCount)
				{
					return threshold;
				}
			}
			return null;
		}

		// Token: 0x06000959 RID: 2393 RVA: 0x00042D78 File Offset: 0x00040F78
		[NullableContext(2)]
		public PlayerCountThreshold GetNextOutpostThreshold(int currentPlayerCount)
		{
			foreach (PlayerCountThreshold threshold in this.OutpostPlayerCountThresholds)
			{
				if (currentPlayerCount < threshold.MinPlayerCount)
				{
					return threshold;
				}
			}
			return null;
		}

		// Token: 0x0600095A RID: 2394 RVA: 0x00042DD4 File Offset: 0x00040FD4
		internal bool IsChunkWithinTerritorialBounds(int chunkX, int chunkZ, BlockPos spawnPos)
		{
			if (this.TerritorialCenter == null || !this.EnableTerritorialRestrictions)
			{
				return true;
			}
			int blockX = chunkX * 32 + 16;
			int blockZ = chunkZ * 32 + 16;
			return this.IsWithinTerritorialBounds(blockX, blockZ, spawnPos);
		}

		// Token: 0x0600095B RID: 2395 RVA: 0x00042E0C File Offset: 0x0004100C
		internal bool IsWithinTerritorialBounds(int blockX, int blockZ, BlockPos spawnPos)
		{
			if (this.TerritorialCenter == null || !this.EnableTerritorialRestrictions)
			{
				return true;
			}
			int num = blockX - this.TerritorialCenter.Position.X - spawnPos.X;
			int deltaZ = blockZ - this.TerritorialCenter.Position.Z - spawnPos.Z;
			return Math.Sqrt((double)(num * num + deltaZ * deltaZ)) <= (double)this.TerritorialRadius;
		}

		// Token: 0x0600095C RID: 2396 RVA: 0x00042E78 File Offset: 0x00041078
		internal bool IsChunkWithinProtectedZone(int chunkX, int chunkZ, BlockPos spawnPos)
		{
			if (!this.EnableProtectedZones || this.ProtectedZones == null || this.ProtectedZones.Count == 0)
			{
				return false;
			}
			int blockX = chunkX * 32 + 16;
			int blockZ = chunkZ * 32 + 16;
			return this.IsWithinProtectedZone(blockX, blockZ, spawnPos);
		}

		// Token: 0x0600095D RID: 2397 RVA: 0x00042EC0 File Offset: 0x000410C0
		internal bool IsWithinProtectedZone(int blockX, int blockZ, BlockPos spawnPos)
		{
			if (!this.EnableProtectedZones || this.ProtectedZones == null || this.ProtectedZones.Count == 0)
			{
				return false;
			}
			using (List<ProtectedZone>.Enumerator enumerator = this.ProtectedZones.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					if (enumerator.Current.IsPositionWithinZone(blockX, blockZ, spawnPos))
					{
						return true;
					}
				}
			}
			return false;
		}

		// Token: 0x0600095E RID: 2398 RVA: 0x00042F3C File Offset: 0x0004113C
		[return: Nullable(2)]
		internal ProtectedZone GetProtectedZoneAt(int blockX, int blockZ, BlockPos spawnPos)
		{
			if (!this.EnableProtectedZones || this.ProtectedZones == null || this.ProtectedZones.Count == 0)
			{
				return null;
			}
			foreach (ProtectedZone zone in this.ProtectedZones)
			{
				if (zone.IsPositionWithinZone(blockX, blockZ, spawnPos))
				{
					return zone;
				}
			}
			return null;
		}
	}
}
