using System;
using System.Runtime.CompilerServices;

namespace SRGuildsAndKingdoms.src.guilds
{
	// Token: 0x020000AC RID: 172
	[NullableContext(1)]
	[Nullable(0)]
	public class OutpostClaim : LandClaim
	{
		// Token: 0x17000219 RID: 537
		// (get) Token: 0x060007F5 RID: 2037 RVA: 0x00037BB1 File Offset: 0x00035DB1
		// (set) Token: 0x060007F6 RID: 2038 RVA: 0x00037BB9 File Offset: 0x00035DB9
		public bool IsOutpost { get; set; } = true;

		// Token: 0x1700021A RID: 538
		// (get) Token: 0x060007F7 RID: 2039 RVA: 0x00037BC2 File Offset: 0x00035DC2
		// (set) Token: 0x060007F8 RID: 2040 RVA: 0x00037BCA File Offset: 0x00035DCA
		public string OutpostName { get; set; } = "";

		// Token: 0x060007F9 RID: 2041 RVA: 0x00037BD3 File Offset: 0x00035DD3
		public OutpostClaim()
		{
			this.IsOutpost = true;
		}

		// Token: 0x060007FA RID: 2042 RVA: 0x00037BF4 File Offset: 0x00035DF4
		public OutpostClaim(int chunkX, int chunkZ, string claimedByUid, string outpostName = "")
		{
			this.IsOutpost = true;
			base.ChunkX = chunkX;
			base.ChunkZ = chunkZ;
			base.ClaimedByUid = claimedByUid;
			this.OutpostName = outpostName;
			base.Timestamp = DateTime.UtcNow;
		}

		// Token: 0x060007FB RID: 2043 RVA: 0x00037C48 File Offset: 0x00035E48
		public static OutpostClaim CreateFromChunk(int chunkX, int chunkZ, [Nullable(2)] string claimedByUid = null, string outpostName = "")
		{
			return new OutpostClaim(chunkX, chunkZ, claimedByUid ?? "", outpostName);
		}

		// Token: 0x060007FC RID: 2044 RVA: 0x00037C5C File Offset: 0x00035E5C
		public static OutpostClaim CreateFromBlockPosition(int blockX, int blockZ, [Nullable(2)] string claimedByUid = null, string outpostName = "")
		{
			int chunkX = LandClaim.FloorDiv(blockX, 32);
			int chunkZ = LandClaim.FloorDiv(blockZ, 32);
			return OutpostClaim.CreateFromChunk(chunkX, chunkZ, claimedByUid, outpostName);
		}
	}
}
