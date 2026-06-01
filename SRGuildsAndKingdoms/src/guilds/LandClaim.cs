using System;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace SRGuildsAndKingdoms.src.guilds
{
	// Token: 0x020000AB RID: 171
	[NullableContext(2)]
	[Nullable(0)]
	[JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
	[JsonDerivedType(typeof(LandClaim), "LandClaim")]
	[JsonDerivedType(typeof(GuildHomeClaim), "GuildHomeClaim")]
	[JsonDerivedType(typeof(OutpostClaim), "OutpostClaim")]
	public class LandClaim
	{
		// Token: 0x1700020D RID: 525
		// (get) Token: 0x060007DD RID: 2013 RVA: 0x00037A33 File Offset: 0x00035C33
		// (set) Token: 0x060007DE RID: 2014 RVA: 0x00037A3B File Offset: 0x00035C3B
		public int ChunkX { get; set; }

		// Token: 0x1700020E RID: 526
		// (get) Token: 0x060007DF RID: 2015 RVA: 0x00037A44 File Offset: 0x00035C44
		// (set) Token: 0x060007E0 RID: 2016 RVA: 0x00037A4C File Offset: 0x00035C4C
		public int ChunkZ { get; set; }

		// Token: 0x1700020F RID: 527
		// (get) Token: 0x060007E1 RID: 2017 RVA: 0x00037A55 File Offset: 0x00035C55
		// (set) Token: 0x060007E2 RID: 2018 RVA: 0x00037A5D File Offset: 0x00035C5D
		public string ClaimedByUid { get; set; }

		// Token: 0x17000210 RID: 528
		// (get) Token: 0x060007E3 RID: 2019 RVA: 0x00037A66 File Offset: 0x00035C66
		// (set) Token: 0x060007E4 RID: 2020 RVA: 0x00037A6E File Offset: 0x00035C6E
		public DateTime Timestamp { get; set; }

		// Token: 0x17000211 RID: 529
		// (get) Token: 0x060007E5 RID: 2021 RVA: 0x00037A77 File Offset: 0x00035C77
		public virtual int MinChunkX
		{
			get
			{
				return this.ChunkX;
			}
		}

		// Token: 0x17000212 RID: 530
		// (get) Token: 0x060007E6 RID: 2022 RVA: 0x00037A7F File Offset: 0x00035C7F
		public virtual int MaxChunkX
		{
			get
			{
				return this.ChunkX;
			}
		}

		// Token: 0x17000213 RID: 531
		// (get) Token: 0x060007E7 RID: 2023 RVA: 0x00037A87 File Offset: 0x00035C87
		public virtual int MinChunkZ
		{
			get
			{
				return this.ChunkZ;
			}
		}

		// Token: 0x17000214 RID: 532
		// (get) Token: 0x060007E8 RID: 2024 RVA: 0x00037A8F File Offset: 0x00035C8F
		public virtual int MaxChunkZ
		{
			get
			{
				return this.ChunkZ;
			}
		}

		// Token: 0x17000215 RID: 533
		// (get) Token: 0x060007E9 RID: 2025 RVA: 0x00037A97 File Offset: 0x00035C97
		public virtual int MinBlockX
		{
			get
			{
				return this.ChunkX * 32;
			}
		}

		// Token: 0x17000216 RID: 534
		// (get) Token: 0x060007EA RID: 2026 RVA: 0x00037AA2 File Offset: 0x00035CA2
		public virtual int MaxBlockX
		{
			get
			{
				return (this.ChunkX + 1) * 32 - 1;
			}
		}

		// Token: 0x17000217 RID: 535
		// (get) Token: 0x060007EB RID: 2027 RVA: 0x00037AB1 File Offset: 0x00035CB1
		public virtual int MinBlockZ
		{
			get
			{
				return this.ChunkZ * 32;
			}
		}

		// Token: 0x17000218 RID: 536
		// (get) Token: 0x060007EC RID: 2028 RVA: 0x00037ABC File Offset: 0x00035CBC
		public virtual int MaxBlockZ
		{
			get
			{
				return (this.ChunkZ + 1) * 32 - 1;
			}
		}

		// Token: 0x060007ED RID: 2029 RVA: 0x00037ACB File Offset: 0x00035CCB
		[NullableContext(1)]
		public virtual bool Intersects(LandClaim other)
		{
			return other != null && this.ChunkX == other.ChunkX && this.ChunkZ == other.ChunkZ;
		}

		// Token: 0x060007EE RID: 2030 RVA: 0x00037AF0 File Offset: 0x00035CF0
		public virtual bool ContainsChunk(int chunkX, int chunkZ)
		{
			return this.ChunkX == chunkX && this.ChunkZ == chunkZ;
		}

		// Token: 0x060007EF RID: 2031 RVA: 0x00037B06 File Offset: 0x00035D06
		public virtual bool ContainsBlockCoord(int blockX, int blockZ)
		{
			return blockX >= this.MinBlockX && blockX <= this.MaxBlockX && blockZ >= this.MinBlockZ && blockZ <= this.MaxBlockZ;
		}

		// Token: 0x060007F0 RID: 2032 RVA: 0x00037B31 File Offset: 0x00035D31
		[NullableContext(1)]
		public static LandClaim CreateFromChunk(int chunkX, int chunkZ, [Nullable(2)] string claimedByUid = null)
		{
			return new LandClaim
			{
				ChunkX = chunkX,
				ChunkZ = chunkZ,
				ClaimedByUid = claimedByUid,
				Timestamp = DateTime.UtcNow
			};
		}

		// Token: 0x060007F1 RID: 2033 RVA: 0x00037B58 File Offset: 0x00035D58
		[NullableContext(1)]
		public static LandClaim CreateFromBlockPosition(int blockX, int blockZ, [Nullable(2)] string claimedByUid = null)
		{
			int chunkX = LandClaim.FloorDiv(blockX, 32);
			int chunkZ = LandClaim.FloorDiv(blockZ, 32);
			return LandClaim.CreateFromChunk(chunkX, chunkZ, claimedByUid);
		}

		// Token: 0x060007F2 RID: 2034 RVA: 0x00037B7D File Offset: 0x00035D7D
		[NullableContext(0)]
		[return: TupleElementNames(new string[]
		{
			"minX",
			"maxX",
			"minZ",
			"maxZ"
		})]
		public virtual ValueTuple<int, int, int, int> ToBlockBounds()
		{
			return new ValueTuple<int, int, int, int>(this.MinBlockX, this.MaxBlockX, this.MinBlockZ, this.MaxBlockZ);
		}

		// Token: 0x060007F3 RID: 2035 RVA: 0x00037B9C File Offset: 0x00035D9C
		public static int FloorDiv(int a, int b)
		{
			return (int)Math.Floor((double)a / (double)b);
		}

		// Token: 0x04000340 RID: 832
		public const int ChunkSize = 32;
	}
}
