using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using SRGuildsAndKingdoms.src.guilds;
using Vintagestory.API.Server;

namespace SRGuildsAndKingdoms.src.database
{
	// Token: 0x020000B5 RID: 181
	[NullableContext(1)]
	[Nullable(0)]
	public class LandClaimRepository
	{
		// Token: 0x06000855 RID: 2133 RVA: 0x0003BFC8 File Offset: 0x0003A1C8
		public LandClaimRepository(ICoreServerAPI serverApi, GuildRepository guildRepository)
		{
		}

		// Token: 0x06000856 RID: 2134 RVA: 0x0003BFF4 File Offset: 0x0003A1F4
		public void RebuildIndexes()
		{
			try
			{
				this.chunkToGuildIndex.Clear();
				this.guildToChunksIndex.Clear();
				foreach (Guild guild in this.<guildRepository>P.GetAllGuilds())
				{
					List<ValueTuple<int, int>> chunkList = new List<ValueTuple<int, int>>();
					foreach (LandClaim claim in guild.Claims)
					{
						GuildHomeClaim guildHome = claim as GuildHomeClaim;
						if (guildHome != null)
						{
							using (IEnumerator<LandClaim> enumerator3 = guildHome.GetIndividualChunks().GetEnumerator())
							{
								while (enumerator3.MoveNext())
								{
									LandClaim homeChunk = enumerator3.Current;
									ValueTuple<int, int> coords = new ValueTuple<int, int>(homeChunk.ChunkX, homeChunk.ChunkZ);
									this.chunkToGuildIndex[coords] = guild.Name;
									chunkList.Add(coords);
								}
								continue;
							}
						}
						ValueTuple<int, int> coords2 = new ValueTuple<int, int>(claim.ChunkX, claim.ChunkZ);
						this.chunkToGuildIndex[coords2] = guild.Name;
						chunkList.Add(coords2);
					}
					if (chunkList.Count > 0)
					{
						this.guildToChunksIndex[guild.Name] = chunkList;
					}
				}
			}
			catch (Exception ex)
			{
				this.<serverApi>P.Logger.Error("[LandClaimRepository] Failed to build indexes: " + ex.Message);
				throw;
			}
		}

		// Token: 0x06000857 RID: 2135 RVA: 0x0003C1D0 File Offset: 0x0003A3D0
		[NullableContext(2)]
		public string GetGuildOwningChunk(int chunkX, int chunkZ)
		{
			ValueTuple<int, int> coords = new ValueTuple<int, int>(chunkX, chunkZ);
			string guildName;
			if (!this.chunkToGuildIndex.TryGetValue(coords, out guildName))
			{
				return null;
			}
			return guildName;
		}

		// Token: 0x06000858 RID: 2136 RVA: 0x0003C1FC File Offset: 0x0003A3FC
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
		public List<ValueTuple<int, int>> GetGuildClaims(string guildName)
		{
			List<ValueTuple<int, int>> chunks;
			if (!this.guildToChunksIndex.TryGetValue(guildName, out chunks))
			{
				return new List<ValueTuple<int, int>>();
			}
			return new List<ValueTuple<int, int>>(chunks);
		}

		// Token: 0x06000859 RID: 2137 RVA: 0x0003C225 File Offset: 0x0003A425
		public bool IsChunkClaimed(int chunkX, int chunkZ)
		{
			return this.chunkToGuildIndex.ContainsKey(new ValueTuple<int, int>(chunkX, chunkZ));
		}

		// Token: 0x0600085A RID: 2138 RVA: 0x0003C23C File Offset: 0x0003A43C
		public void AddClaimToIndex(string guildName, int chunkX, int chunkZ)
		{
			ValueTuple<int, int> coords = new ValueTuple<int, int>(chunkX, chunkZ);
			this.chunkToGuildIndex[coords] = guildName;
			if (!this.guildToChunksIndex.ContainsKey(guildName))
			{
				this.guildToChunksIndex[guildName] = new List<ValueTuple<int, int>>();
			}
			this.guildToChunksIndex[guildName].Add(coords);
		}

		// Token: 0x0600085B RID: 2139 RVA: 0x0003C290 File Offset: 0x0003A490
		public void RemoveClaimFromIndex(int chunkX, int chunkZ)
		{
			ValueTuple<int, int> coords = new ValueTuple<int, int>(chunkX, chunkZ);
			string guildName;
			if (this.chunkToGuildIndex.TryGetValue(coords, out guildName))
			{
				this.chunkToGuildIndex.Remove(coords);
				List<ValueTuple<int, int>> chunkList;
				if (this.guildToChunksIndex.TryGetValue(guildName, out chunkList))
				{
					chunkList.Remove(coords);
					if (chunkList.Count == 0)
					{
						this.guildToChunksIndex.Remove(guildName);
					}
				}
			}
		}

		// Token: 0x0600085C RID: 2140 RVA: 0x0003C2F0 File Offset: 0x0003A4F0
		public void UpdateGuildName(string oldName, string newName)
		{
			List<ValueTuple<int, int>> chunkList;
			if (this.guildToChunksIndex.TryGetValue(oldName, out chunkList))
			{
				this.guildToChunksIndex.Remove(oldName);
				this.guildToChunksIndex[newName] = chunkList;
				foreach (ValueTuple<int, int> coords in chunkList)
				{
					this.chunkToGuildIndex[coords] = newName;
				}
			}
		}

		// Token: 0x0600085D RID: 2141 RVA: 0x0003C370 File Offset: 0x0003A570
		public void RemoveGuildFromIndex(string guildName)
		{
			List<ValueTuple<int, int>> chunkList;
			if (this.guildToChunksIndex.TryGetValue(guildName, out chunkList))
			{
				foreach (ValueTuple<int, int> coords in chunkList)
				{
					this.chunkToGuildIndex.Remove(coords);
				}
				this.guildToChunksIndex.Remove(guildName);
			}
		}

		// Token: 0x0600085E RID: 2142 RVA: 0x0003C3E4 File Offset: 0x0003A5E4
		[NullableContext(0)]
		[return: TupleElementNames(new string[]
		{
			"totalClaimedChunks",
			"guildsWithClaims"
		})]
		public ValueTuple<int, int> GetStatistics()
		{
			return new ValueTuple<int, int>(this.chunkToGuildIndex.Count, this.guildToChunksIndex.Count);
		}

		// Token: 0x0600085F RID: 2143 RVA: 0x0003C404 File Offset: 0x0003A604
		[return: TupleElementNames(new string[]
		{
			"chunkX",
			"chunkZ",
			"guildName"
		})]
		[return: Nullable(new byte[]
		{
			1,
			0,
			1
		})]
		public List<ValueTuple<int, int, string>> GetClaimsInRadius(int centerX, int centerZ, int radius)
		{
			List<ValueTuple<int, int, string>> results = new List<ValueTuple<int, int, string>>();
			for (int x = centerX - radius; x <= centerX + radius; x++)
			{
				for (int z = centerZ - radius; z <= centerZ + radius; z++)
				{
					ValueTuple<int, int> coords = new ValueTuple<int, int>(x, z);
					string guildName;
					if (this.chunkToGuildIndex.TryGetValue(coords, out guildName))
					{
						results.Add(new ValueTuple<int, int, string>(x, z, guildName));
					}
				}
			}
			return results;
		}

		// Token: 0x04000362 RID: 866
		[CompilerGenerated]
		private ICoreServerAPI <serverApi>P = serverApi;

		// Token: 0x04000363 RID: 867
		[CompilerGenerated]
		private GuildRepository <guildRepository>P = guildRepository;

		// Token: 0x04000364 RID: 868
		[TupleElementNames(new string[]
		{
			"chunkX",
			"chunkZ"
		})]
		[Nullable(new byte[]
		{
			1,
			0,
			1
		})]
		private readonly Dictionary<ValueTuple<int, int>, string> chunkToGuildIndex = new Dictionary<ValueTuple<int, int>, string>();

		// Token: 0x04000365 RID: 869
		[TupleElementNames(new string[]
		{
			"chunkX",
			"chunkZ"
		})]
		[Nullable(new byte[]
		{
			1,
			1,
			1,
			0
		})]
		private readonly Dictionary<string, List<ValueTuple<int, int>>> guildToChunksIndex = new Dictionary<string, List<ValueTuple<int, int>>>();
	}
}
