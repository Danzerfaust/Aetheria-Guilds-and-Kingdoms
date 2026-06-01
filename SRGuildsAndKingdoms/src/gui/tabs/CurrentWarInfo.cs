using System;
using System.Runtime.CompilerServices;

namespace SRGuildsAndKingdoms.src.gui.tabs
{
	// Token: 0x0200008A RID: 138
	[NullableContext(1)]
	[Nullable(0)]
	public class CurrentWarInfo
	{
		// Token: 0x170001A3 RID: 419
		// (get) Token: 0x06000612 RID: 1554 RVA: 0x0002C49C File Offset: 0x0002A69C
		// (set) Token: 0x06000613 RID: 1555 RVA: 0x0002C4A4 File Offset: 0x0002A6A4
		public string NodeId { get; set; } = string.Empty;

		// Token: 0x170001A4 RID: 420
		// (get) Token: 0x06000614 RID: 1556 RVA: 0x0002C4AD File Offset: 0x0002A6AD
		// (set) Token: 0x06000615 RID: 1557 RVA: 0x0002C4B5 File Offset: 0x0002A6B5
		public string NodeName { get; set; } = string.Empty;

		// Token: 0x170001A5 RID: 421
		// (get) Token: 0x06000616 RID: 1558 RVA: 0x0002C4BE File Offset: 0x0002A6BE
		// (set) Token: 0x06000617 RID: 1559 RVA: 0x0002C4C6 File Offset: 0x0002A6C6
		public string Status { get; set; } = string.Empty;

		// Token: 0x170001A6 RID: 422
		// (get) Token: 0x06000618 RID: 1560 RVA: 0x0002C4CF File Offset: 0x0002A6CF
		// (set) Token: 0x06000619 RID: 1561 RVA: 0x0002C4D7 File Offset: 0x0002A6D7
		public double PointsNeeded { get; set; }

		// Token: 0x170001A7 RID: 423
		// (get) Token: 0x0600061A RID: 1562 RVA: 0x0002C4E0 File Offset: 0x0002A6E0
		// (set) Token: 0x0600061B RID: 1563 RVA: 0x0002C4E8 File Offset: 0x0002A6E8
		[Nullable(2)]
		public GuildWarProgressInfo YourGuildProgress { [NullableContext(2)] get; [NullableContext(2)] set; }
	}
}
