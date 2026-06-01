using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace SRGuildsAndKingdoms.src.guilds
{
	// Token: 0x020000A7 RID: 167
	[NullableContext(1)]
	[Nullable(0)]
	public class GuildHomeClaim : LandClaim
	{
		// Token: 0x170001F7 RID: 503
		// (get) Token: 0x06000763 RID: 1891 RVA: 0x000346E7 File Offset: 0x000328E7
		// (set) Token: 0x06000764 RID: 1892 RVA: 0x000346EF File Offset: 0x000328EF
		public List<LandClaim> HomeChunks { get; set; } = new List<LandClaim>();

		// Token: 0x170001F8 RID: 504
		// (get) Token: 0x06000765 RID: 1893 RVA: 0x000346F8 File Offset: 0x000328F8
		// (set) Token: 0x06000766 RID: 1894 RVA: 0x00034700 File Offset: 0x00032900
		public int CenterChunkX { get; set; }

		// Token: 0x170001F9 RID: 505
		// (get) Token: 0x06000767 RID: 1895 RVA: 0x00034709 File Offset: 0x00032909
		// (set) Token: 0x06000768 RID: 1896 RVA: 0x00034711 File Offset: 0x00032911
		public int CenterChunkZ { get; set; }

		// Token: 0x170001FA RID: 506
		// (get) Token: 0x06000769 RID: 1897 RVA: 0x0003471A File Offset: 0x0003291A
		// (set) Token: 0x0600076A RID: 1898 RVA: 0x00034722 File Offset: 0x00032922
		public bool IsGuildHome { get; set; } = true;

		// Token: 0x170001FB RID: 507
		// (get) Token: 0x0600076B RID: 1899 RVA: 0x0003472C File Offset: 0x0003292C
		public override int MinChunkX
		{
			get
			{
				if (this.HomeChunks.Count <= 0)
				{
					return this.CenterChunkX;
				}
				return this.HomeChunks.Min((LandClaim c) => c.ChunkX);
			}
		}

		// Token: 0x170001FC RID: 508
		// (get) Token: 0x0600076C RID: 1900 RVA: 0x00034778 File Offset: 0x00032978
		public override int MaxChunkX
		{
			get
			{
				if (this.HomeChunks.Count <= 0)
				{
					return this.CenterChunkX;
				}
				return this.HomeChunks.Max((LandClaim c) => c.ChunkX);
			}
		}

		// Token: 0x170001FD RID: 509
		// (get) Token: 0x0600076D RID: 1901 RVA: 0x000347C4 File Offset: 0x000329C4
		public override int MinChunkZ
		{
			get
			{
				if (this.HomeChunks.Count <= 0)
				{
					return this.CenterChunkZ;
				}
				return this.HomeChunks.Min((LandClaim c) => c.ChunkZ);
			}
		}

		// Token: 0x170001FE RID: 510
		// (get) Token: 0x0600076E RID: 1902 RVA: 0x00034810 File Offset: 0x00032A10
		public override int MaxChunkZ
		{
			get
			{
				if (this.HomeChunks.Count <= 0)
				{
					return this.CenterChunkZ;
				}
				return this.HomeChunks.Max((LandClaim c) => c.ChunkZ);
			}
		}

		// Token: 0x170001FF RID: 511
		// (get) Token: 0x0600076F RID: 1903 RVA: 0x0003485C File Offset: 0x00032A5C
		public override int MinBlockX
		{
			get
			{
				return this.MinChunkX * 32;
			}
		}

		// Token: 0x17000200 RID: 512
		// (get) Token: 0x06000770 RID: 1904 RVA: 0x00034867 File Offset: 0x00032A67
		public override int MaxBlockX
		{
			get
			{
				return (this.MaxChunkX + 1) * 32 - 1;
			}
		}

		// Token: 0x17000201 RID: 513
		// (get) Token: 0x06000771 RID: 1905 RVA: 0x00034876 File Offset: 0x00032A76
		public override int MinBlockZ
		{
			get
			{
				return this.MinChunkZ * 32;
			}
		}

		// Token: 0x17000202 RID: 514
		// (get) Token: 0x06000772 RID: 1906 RVA: 0x00034881 File Offset: 0x00032A81
		public override int MaxBlockZ
		{
			get
			{
				return (this.MaxChunkZ + 1) * 32 - 1;
			}
		}

		// Token: 0x06000773 RID: 1907 RVA: 0x00034890 File Offset: 0x00032A90
		public GuildHomeClaim()
		{
			this.IsGuildHome = true;
		}

		// Token: 0x06000774 RID: 1908 RVA: 0x000348B4 File Offset: 0x00032AB4
		public GuildHomeClaim(int centerChunkX, int centerChunkZ, string claimedByUid)
		{
			this.IsGuildHome = true;
			this.CenterChunkX = centerChunkX;
			this.CenterChunkZ = centerChunkZ;
			base.ClaimedByUid = claimedByUid;
			base.Timestamp = DateTime.UtcNow;
			base.ChunkX = centerChunkX;
			base.ChunkZ = centerChunkZ;
			this.GenerateHomeChunks();
		}

		// Token: 0x06000775 RID: 1909 RVA: 0x00034914 File Offset: 0x00032B14
		public void GenerateHomeChunks()
		{
			this.HomeChunks.Clear();
			for (int dx = 0; dx <= 1; dx++)
			{
				for (int dz = 0; dz <= 1; dz++)
				{
					int chunkX = this.CenterChunkX + dx;
					int chunkZ = this.CenterChunkZ + dz;
					LandClaim chunk = new LandClaim
					{
						ChunkX = chunkX,
						ChunkZ = chunkZ,
						ClaimedByUid = base.ClaimedByUid,
						Timestamp = base.Timestamp
					};
					this.HomeChunks.Add(chunk);
				}
			}
		}

		// Token: 0x06000776 RID: 1910 RVA: 0x00034990 File Offset: 0x00032B90
		public override bool Intersects(LandClaim other)
		{
			if (other == null)
			{
				return false;
			}
			GuildHomeClaim otherHome = other as GuildHomeClaim;
			if (otherHome != null)
			{
				return this.HomeChunks.Any((LandClaim thisChunk) => otherHome.HomeChunks.Any((LandClaim otherChunk) => thisChunk.Intersects(otherChunk)));
			}
			return this.HomeChunks.Any((LandClaim chunk) => chunk.Intersects(other));
		}

		// Token: 0x06000777 RID: 1911 RVA: 0x000349FC File Offset: 0x00032BFC
		public override bool ContainsChunk(int chunkX, int chunkZ)
		{
			return this.HomeChunks.Any((LandClaim chunk) => chunk.ContainsChunk(chunkX, chunkZ));
		}

		// Token: 0x06000778 RID: 1912 RVA: 0x00034A34 File Offset: 0x00032C34
		public override bool ContainsBlockCoord(int blockX, int blockZ)
		{
			return this.HomeChunks.Any((LandClaim chunk) => chunk.ContainsBlockCoord(blockX, blockZ));
		}

		// Token: 0x06000779 RID: 1913 RVA: 0x00034A6C File Offset: 0x00032C6C
		public IEnumerable<LandClaim> GetIndividualChunks()
		{
			return this.HomeChunks;
		}

		// Token: 0x17000203 RID: 515
		// (get) Token: 0x0600077A RID: 1914 RVA: 0x00034A74 File Offset: 0x00032C74
		public int ChunkCount
		{
			get
			{
				return this.HomeChunks.Count;
			}
		}

		// Token: 0x0600077B RID: 1915 RVA: 0x00034A81 File Offset: 0x00032C81
		public static GuildHomeClaim CreateFromCenterChunk(int centerChunkX, int centerChunkZ, string claimedByUid)
		{
			return new GuildHomeClaim(centerChunkX, centerChunkZ, claimedByUid);
		}

		// Token: 0x0600077C RID: 1916 RVA: 0x00034A8C File Offset: 0x00032C8C
		public new static GuildHomeClaim CreateFromBlockPosition(int blockX, int blockZ, string claimedByUid)
		{
			int centerChunkX = LandClaim.FloorDiv(blockX, 32);
			int centerChunkZ = LandClaim.FloorDiv(blockZ, 32);
			return GuildHomeClaim.CreateFromCenterChunk(centerChunkX, centerChunkZ, claimedByUid);
		}

		// Token: 0x0600077D RID: 1917 RVA: 0x00034AB1 File Offset: 0x00032CB1
		[NullableContext(0)]
		[return: TupleElementNames(new string[]
		{
			"minX",
			"maxX",
			"minZ",
			"maxZ"
		})]
		public override ValueTuple<int, int, int, int> ToBlockBounds()
		{
			return new ValueTuple<int, int, int, int>(this.MinBlockX, this.MaxBlockX, this.MinBlockZ, this.MaxBlockZ);
		}
	}
}
