using System;
using System.Runtime.CompilerServices;
using ProtoBuf;

namespace SRGuildsAndKingdoms.src.guilds
{
	// Token: 0x020000A5 RID: 165
	[NullableContext(1)]
	[Nullable(0)]
	[ProtoContract(ImplicitFields = 1)]
	public class LandClaimDto
	{
		// Token: 0x170001EB RID: 491
		// (get) Token: 0x06000749 RID: 1865 RVA: 0x00034600 File Offset: 0x00032800
		// (set) Token: 0x0600074A RID: 1866 RVA: 0x00034608 File Offset: 0x00032808
		public int ChunkX { get; set; }

		// Token: 0x170001EC RID: 492
		// (get) Token: 0x0600074B RID: 1867 RVA: 0x00034611 File Offset: 0x00032811
		// (set) Token: 0x0600074C RID: 1868 RVA: 0x00034619 File Offset: 0x00032819
		public int ChunkZ { get; set; }

		// Token: 0x170001ED RID: 493
		// (get) Token: 0x0600074D RID: 1869 RVA: 0x00034622 File Offset: 0x00032822
		// (set) Token: 0x0600074E RID: 1870 RVA: 0x0003462A File Offset: 0x0003282A
		public string ClaimedByUid { get; set; }

		// Token: 0x170001EE RID: 494
		// (get) Token: 0x0600074F RID: 1871 RVA: 0x00034633 File Offset: 0x00032833
		// (set) Token: 0x06000750 RID: 1872 RVA: 0x0003463B File Offset: 0x0003283B
		public DateTime Timestamp { get; set; }

		// Token: 0x170001EF RID: 495
		// (get) Token: 0x06000751 RID: 1873 RVA: 0x00034644 File Offset: 0x00032844
		// (set) Token: 0x06000752 RID: 1874 RVA: 0x0003464C File Offset: 0x0003284C
		public bool IsGuildHome { get; set; }

		// Token: 0x170001F0 RID: 496
		// (get) Token: 0x06000753 RID: 1875 RVA: 0x00034655 File Offset: 0x00032855
		// (set) Token: 0x06000754 RID: 1876 RVA: 0x0003465D File Offset: 0x0003285D
		public int? HomeCenterX { get; set; }

		// Token: 0x170001F1 RID: 497
		// (get) Token: 0x06000755 RID: 1877 RVA: 0x00034666 File Offset: 0x00032866
		// (set) Token: 0x06000756 RID: 1878 RVA: 0x0003466E File Offset: 0x0003286E
		public int? HomeCenterZ { get; set; }

		// Token: 0x170001F2 RID: 498
		// (get) Token: 0x06000757 RID: 1879 RVA: 0x00034677 File Offset: 0x00032877
		// (set) Token: 0x06000758 RID: 1880 RVA: 0x0003467F File Offset: 0x0003287F
		public bool IsOutpost { get; set; }

		// Token: 0x170001F3 RID: 499
		// (get) Token: 0x06000759 RID: 1881 RVA: 0x00034688 File Offset: 0x00032888
		// (set) Token: 0x0600075A RID: 1882 RVA: 0x00034690 File Offset: 0x00032890
		public string OutpostName { get; set; } = "";
	}
}
