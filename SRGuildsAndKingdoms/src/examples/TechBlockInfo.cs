using System;
using System.Runtime.CompilerServices;
using SRGuildsAndKingdoms.src.techblock;

namespace SRGuildsAndKingdoms.src.examples
{
	// Token: 0x020000B0 RID: 176
	[NullableContext(1)]
	[Nullable(0)]
	public class TechBlockInfo
	{
		// Token: 0x1700021B RID: 539
		// (get) Token: 0x06000810 RID: 2064 RVA: 0x00038962 File Offset: 0x00036B62
		// (set) Token: 0x06000811 RID: 2065 RVA: 0x0003896A File Offset: 0x00036B6A
		public TechBlock TechBlock { get; set; }

		// Token: 0x1700021C RID: 540
		// (get) Token: 0x06000812 RID: 2066 RVA: 0x00038973 File Offset: 0x00036B73
		// (set) Token: 0x06000813 RID: 2067 RVA: 0x0003897B File Offset: 0x00036B7B
		public bool IsUnlocked { get; set; }

		// Token: 0x1700021D RID: 541
		// (get) Token: 0x06000814 RID: 2068 RVA: 0x00038984 File Offset: 0x00036B84
		// (set) Token: 0x06000815 RID: 2069 RVA: 0x0003898C File Offset: 0x00036B8C
		public bool IsAvailable { get; set; }

		// Token: 0x1700021E RID: 542
		// (get) Token: 0x06000816 RID: 2070 RVA: 0x00038995 File Offset: 0x00036B95
		// (set) Token: 0x06000817 RID: 2071 RVA: 0x0003899D File Offset: 0x00036B9D
		public GuildTechProgress Progress { get; set; }

		// Token: 0x1700021F RID: 543
		// (get) Token: 0x06000818 RID: 2072 RVA: 0x000389A6 File Offset: 0x00036BA6
		// (set) Token: 0x06000819 RID: 2073 RVA: 0x000389AE File Offset: 0x00036BAE
		public double ProgressPercentage { get; set; }
	}
}
