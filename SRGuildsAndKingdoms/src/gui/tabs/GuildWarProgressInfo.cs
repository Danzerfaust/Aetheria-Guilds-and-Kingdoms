using System;

namespace SRGuildsAndKingdoms.src.gui.tabs
{
	// Token: 0x0200008B RID: 139
	public class GuildWarProgressInfo
	{
		// Token: 0x170001A8 RID: 424
		// (get) Token: 0x0600061D RID: 1565 RVA: 0x0002C51A File Offset: 0x0002A71A
		// (set) Token: 0x0600061E RID: 1566 RVA: 0x0002C522 File Offset: 0x0002A722
		public double CapturePoints { get; set; }

		// Token: 0x170001A9 RID: 425
		// (get) Token: 0x0600061F RID: 1567 RVA: 0x0002C52B File Offset: 0x0002A72B
		// (set) Token: 0x06000620 RID: 1568 RVA: 0x0002C533 File Offset: 0x0002A733
		public int PlayersInZone { get; set; }

		// Token: 0x170001AA RID: 426
		// (get) Token: 0x06000621 RID: 1569 RVA: 0x0002C53C File Offset: 0x0002A73C
		// (set) Token: 0x06000622 RID: 1570 RVA: 0x0002C544 File Offset: 0x0002A744
		public int Kills { get; set; }

		// Token: 0x170001AB RID: 427
		// (get) Token: 0x06000623 RID: 1571 RVA: 0x0002C54D File Offset: 0x0002A74D
		// (set) Token: 0x06000624 RID: 1572 RVA: 0x0002C555 File Offset: 0x0002A755
		public int Deaths { get; set; }
	}
}
