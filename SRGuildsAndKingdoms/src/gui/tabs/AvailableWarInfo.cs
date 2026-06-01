using System;
using System.Runtime.CompilerServices;

namespace SRGuildsAndKingdoms.src.gui.tabs
{
	// Token: 0x0200008C RID: 140
	[NullableContext(1)]
	[Nullable(0)]
	public class AvailableWarInfo
	{
		// Token: 0x170001AC RID: 428
		// (get) Token: 0x06000626 RID: 1574 RVA: 0x0002C566 File Offset: 0x0002A766
		// (set) Token: 0x06000627 RID: 1575 RVA: 0x0002C56E File Offset: 0x0002A76E
		public string NodeId { get; set; } = string.Empty;

		// Token: 0x170001AD RID: 429
		// (get) Token: 0x06000628 RID: 1576 RVA: 0x0002C577 File Offset: 0x0002A777
		// (set) Token: 0x06000629 RID: 1577 RVA: 0x0002C57F File Offset: 0x0002A77F
		public string NodeName { get; set; } = string.Empty;

		// Token: 0x170001AE RID: 430
		// (get) Token: 0x0600062A RID: 1578 RVA: 0x0002C588 File Offset: 0x0002A788
		// (set) Token: 0x0600062B RID: 1579 RVA: 0x0002C590 File Offset: 0x0002A790
		public DateTime WarStartTime { get; set; }

		// Token: 0x170001AF RID: 431
		// (get) Token: 0x0600062C RID: 1580 RVA: 0x0002C599 File Offset: 0x0002A799
		// (set) Token: 0x0600062D RID: 1581 RVA: 0x0002C5A1 File Offset: 0x0002A7A1
		public int CurrentSignups { get; set; }

		// Token: 0x170001B0 RID: 432
		// (get) Token: 0x0600062E RID: 1582 RVA: 0x0002C5AA File Offset: 0x0002A7AA
		// (set) Token: 0x0600062F RID: 1583 RVA: 0x0002C5B2 File Offset: 0x0002A7B2
		public int MaxGuilds { get; set; }

		// Token: 0x170001B1 RID: 433
		// (get) Token: 0x06000630 RID: 1584 RVA: 0x0002C5BB File Offset: 0x0002A7BB
		// (set) Token: 0x06000631 RID: 1585 RVA: 0x0002C5C3 File Offset: 0x0002A7C3
		public bool CanSignup { get; set; }
	}
}
