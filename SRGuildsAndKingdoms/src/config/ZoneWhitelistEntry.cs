using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace SRGuildsAndKingdoms.src.config
{
	// Token: 0x020000C7 RID: 199
	[NullableContext(1)]
	[Nullable(0)]
	public class ZoneWhitelistEntry
	{
		// Token: 0x17000282 RID: 642
		// (get) Token: 0x060009A6 RID: 2470 RVA: 0x00044E3A File Offset: 0x0004303A
		// (set) Token: 0x060009A7 RID: 2471 RVA: 0x00044E42 File Offset: 0x00043042
		public int ZoneId { get; set; }

		// Token: 0x17000283 RID: 643
		// (get) Token: 0x060009A8 RID: 2472 RVA: 0x00044E4B File Offset: 0x0004304B
		// (set) Token: 0x060009A9 RID: 2473 RVA: 0x00044E53 File Offset: 0x00043053
		public string ZoneName { get; set; } = string.Empty;

		// Token: 0x17000284 RID: 644
		// (get) Token: 0x060009AA RID: 2474 RVA: 0x00044E5C File Offset: 0x0004305C
		// (set) Token: 0x060009AB RID: 2475 RVA: 0x00044E64 File Offset: 0x00043064
		public List<string> WhitelistedPlayerUids { get; set; } = new List<string>();

		// Token: 0x17000285 RID: 645
		// (get) Token: 0x060009AC RID: 2476 RVA: 0x00044E6D File Offset: 0x0004306D
		// (set) Token: 0x060009AD RID: 2477 RVA: 0x00044E75 File Offset: 0x00043075
		public DateTime LastModified { get; set; } = DateTime.UtcNow;
	}
}
