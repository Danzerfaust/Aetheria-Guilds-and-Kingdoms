using System;
using System.Runtime.CompilerServices;

namespace SRGuildsAndKingdoms.src.gui.tabs
{
	// Token: 0x02000089 RID: 137
	[NullableContext(1)]
	[Nullable(0)]
	public class ControlledNodeInfo
	{
		// Token: 0x17000198 RID: 408
		// (get) Token: 0x060005FB RID: 1531 RVA: 0x0002C3C3 File Offset: 0x0002A5C3
		// (set) Token: 0x060005FC RID: 1532 RVA: 0x0002C3CB File Offset: 0x0002A5CB
		public string NodeId { get; set; } = string.Empty;

		// Token: 0x17000199 RID: 409
		// (get) Token: 0x060005FD RID: 1533 RVA: 0x0002C3D4 File Offset: 0x0002A5D4
		// (set) Token: 0x060005FE RID: 1534 RVA: 0x0002C3DC File Offset: 0x0002A5DC
		public string NodeName { get; set; } = string.Empty;

		// Token: 0x1700019A RID: 410
		// (get) Token: 0x060005FF RID: 1535 RVA: 0x0002C3E5 File Offset: 0x0002A5E5
		// (set) Token: 0x06000600 RID: 1536 RVA: 0x0002C3ED File Offset: 0x0002A5ED
		public DateTime? CapturedAt { get; set; }

		// Token: 0x1700019B RID: 411
		// (get) Token: 0x06000601 RID: 1537 RVA: 0x0002C3F6 File Offset: 0x0002A5F6
		// (set) Token: 0x06000602 RID: 1538 RVA: 0x0002C3FE File Offset: 0x0002A5FE
		public int InfluencePerDay { get; set; }

		// Token: 0x1700019C RID: 412
		// (get) Token: 0x06000603 RID: 1539 RVA: 0x0002C407 File Offset: 0x0002A607
		// (set) Token: 0x06000604 RID: 1540 RVA: 0x0002C40F File Offset: 0x0002A60F
		public int? WarStatus { get; set; }

		// Token: 0x1700019D RID: 413
		// (get) Token: 0x06000605 RID: 1541 RVA: 0x0002C418 File Offset: 0x0002A618
		// (set) Token: 0x06000606 RID: 1542 RVA: 0x0002C420 File Offset: 0x0002A620
		public DateTime? WarScheduledStartTime { get; set; }

		// Token: 0x1700019E RID: 414
		// (get) Token: 0x06000607 RID: 1543 RVA: 0x0002C429 File Offset: 0x0002A629
		// (set) Token: 0x06000608 RID: 1544 RVA: 0x0002C431 File Offset: 0x0002A631
		public DateTime? WarStartedTime { get; set; }

		// Token: 0x1700019F RID: 415
		// (get) Token: 0x06000609 RID: 1545 RVA: 0x0002C43A File Offset: 0x0002A63A
		// (set) Token: 0x0600060A RID: 1546 RVA: 0x0002C442 File Offset: 0x0002A642
		public DateTime? WarEndTime { get; set; }

		// Token: 0x170001A0 RID: 416
		// (get) Token: 0x0600060B RID: 1547 RVA: 0x0002C44B File Offset: 0x0002A64B
		// (set) Token: 0x0600060C RID: 1548 RVA: 0x0002C453 File Offset: 0x0002A653
		public int? WarSignupCount { get; set; }

		// Token: 0x170001A1 RID: 417
		// (get) Token: 0x0600060D RID: 1549 RVA: 0x0002C45C File Offset: 0x0002A65C
		// (set) Token: 0x0600060E RID: 1550 RVA: 0x0002C464 File Offset: 0x0002A664
		public int? WarMaxGuilds { get; set; }

		// Token: 0x170001A2 RID: 418
		// (get) Token: 0x0600060F RID: 1551 RVA: 0x0002C46D File Offset: 0x0002A66D
		// (set) Token: 0x06000610 RID: 1552 RVA: 0x0002C475 File Offset: 0x0002A675
		[Nullable(2)]
		public string WarWinnerGuildName { [NullableContext(2)] get; [NullableContext(2)] set; }
	}
}
