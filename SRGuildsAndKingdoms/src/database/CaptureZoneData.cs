using System;
using System.Runtime.CompilerServices;

namespace SRGuildsAndKingdoms.src.database
{
	// Token: 0x020000BA RID: 186
	[NullableContext(1)]
	[Nullable(0)]
	public class CaptureZoneData
	{
		// Token: 0x17000233 RID: 563
		// (get) Token: 0x060008A6 RID: 2214 RVA: 0x000406BC File Offset: 0x0003E8BC
		// (set) Token: 0x060008A7 RID: 2215 RVA: 0x000406C4 File Offset: 0x0003E8C4
		public int Id { get; set; }

		// Token: 0x17000234 RID: 564
		// (get) Token: 0x060008A8 RID: 2216 RVA: 0x000406CD File Offset: 0x0003E8CD
		// (set) Token: 0x060008A9 RID: 2217 RVA: 0x000406D5 File Offset: 0x0003E8D5
		public string NodeName { get; set; } = string.Empty;

		// Token: 0x17000235 RID: 565
		// (get) Token: 0x060008AA RID: 2218 RVA: 0x000406DE File Offset: 0x0003E8DE
		// (set) Token: 0x060008AB RID: 2219 RVA: 0x000406E6 File Offset: 0x0003E8E6
		public string ZoneId { get; set; } = string.Empty;

		// Token: 0x17000236 RID: 566
		// (get) Token: 0x060008AC RID: 2220 RVA: 0x000406EF File Offset: 0x0003E8EF
		// (set) Token: 0x060008AD RID: 2221 RVA: 0x000406F7 File Offset: 0x0003E8F7
		public string ZoneName { get; set; } = string.Empty;

		// Token: 0x17000237 RID: 567
		// (get) Token: 0x060008AE RID: 2222 RVA: 0x00040700 File Offset: 0x0003E900
		// (set) Token: 0x060008AF RID: 2223 RVA: 0x00040708 File Offset: 0x0003E908
		public double CenterX { get; set; }

		// Token: 0x17000238 RID: 568
		// (get) Token: 0x060008B0 RID: 2224 RVA: 0x00040711 File Offset: 0x0003E911
		// (set) Token: 0x060008B1 RID: 2225 RVA: 0x00040719 File Offset: 0x0003E919
		public double CenterY { get; set; }

		// Token: 0x17000239 RID: 569
		// (get) Token: 0x060008B2 RID: 2226 RVA: 0x00040722 File Offset: 0x0003E922
		// (set) Token: 0x060008B3 RID: 2227 RVA: 0x0004072A File Offset: 0x0003E92A
		public double CenterZ { get; set; }

		// Token: 0x1700023A RID: 570
		// (get) Token: 0x060008B4 RID: 2228 RVA: 0x00040733 File Offset: 0x0003E933
		// (set) Token: 0x060008B5 RID: 2229 RVA: 0x0004073B File Offset: 0x0003E93B
		public int Radius { get; set; }

		// Token: 0x1700023B RID: 571
		// (get) Token: 0x060008B6 RID: 2230 RVA: 0x00040744 File Offset: 0x0003E944
		// (set) Token: 0x060008B7 RID: 2231 RVA: 0x0004074C File Offset: 0x0003E94C
		public double PointMultiplier { get; set; } = 1.0;

		// Token: 0x1700023C RID: 572
		// (get) Token: 0x060008B8 RID: 2232 RVA: 0x00040755 File Offset: 0x0003E955
		// (set) Token: 0x060008B9 RID: 2233 RVA: 0x0004075D File Offset: 0x0003E95D
		public bool IsActive { get; set; } = true;

		// Token: 0x1700023D RID: 573
		// (get) Token: 0x060008BA RID: 2234 RVA: 0x00040766 File Offset: 0x0003E966
		// (set) Token: 0x060008BB RID: 2235 RVA: 0x0004076E File Offset: 0x0003E96E
		[Nullable(2)]
		public string Description { [NullableContext(2)] get; [NullableContext(2)] set; }

		// Token: 0x1700023E RID: 574
		// (get) Token: 0x060008BC RID: 2236 RVA: 0x00040777 File Offset: 0x0003E977
		// (set) Token: 0x060008BD RID: 2237 RVA: 0x0004077F File Offset: 0x0003E97F
		public long CreatedAt { get; set; }
	}
}
