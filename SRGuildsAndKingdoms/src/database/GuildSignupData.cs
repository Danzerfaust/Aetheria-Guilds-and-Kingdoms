using System;
using System.Runtime.CompilerServices;

namespace SRGuildsAndKingdoms.src.database
{
	// Token: 0x020000BD RID: 189
	[NullableContext(1)]
	[Nullable(0)]
	public class GuildSignupData
	{
		// Token: 0x17000255 RID: 597
		// (get) Token: 0x060008ED RID: 2285 RVA: 0x00040979 File Offset: 0x0003EB79
		// (set) Token: 0x060008EE RID: 2286 RVA: 0x00040981 File Offset: 0x0003EB81
		public int Id { get; set; }

		// Token: 0x17000256 RID: 598
		// (get) Token: 0x060008EF RID: 2287 RVA: 0x0004098A File Offset: 0x0003EB8A
		// (set) Token: 0x060008F0 RID: 2288 RVA: 0x00040992 File Offset: 0x0003EB92
		public int WarId { get; set; }

		// Token: 0x17000257 RID: 599
		// (get) Token: 0x060008F1 RID: 2289 RVA: 0x0004099B File Offset: 0x0003EB9B
		// (set) Token: 0x060008F2 RID: 2290 RVA: 0x000409A3 File Offset: 0x0003EBA3
		public string GuildUid { get; set; } = string.Empty;

		// Token: 0x17000258 RID: 600
		// (get) Token: 0x060008F3 RID: 2291 RVA: 0x000409AC File Offset: 0x0003EBAC
		// (set) Token: 0x060008F4 RID: 2292 RVA: 0x000409B4 File Offset: 0x0003EBB4
		public string GuildName { get; set; } = string.Empty;

		// Token: 0x17000259 RID: 601
		// (get) Token: 0x060008F5 RID: 2293 RVA: 0x000409BD File Offset: 0x0003EBBD
		// (set) Token: 0x060008F6 RID: 2294 RVA: 0x000409C5 File Offset: 0x0003EBC5
		public string SignupByPlayerUid { get; set; } = string.Empty;

		// Token: 0x1700025A RID: 602
		// (get) Token: 0x060008F7 RID: 2295 RVA: 0x000409CE File Offset: 0x0003EBCE
		// (set) Token: 0x060008F8 RID: 2296 RVA: 0x000409D6 File Offset: 0x0003EBD6
		public long SignupTime { get; set; }

		// Token: 0x1700025B RID: 603
		// (get) Token: 0x060008F9 RID: 2297 RVA: 0x000409DF File Offset: 0x0003EBDF
		// (set) Token: 0x060008FA RID: 2298 RVA: 0x000409E7 File Offset: 0x0003EBE7
		public int MembersOnline { get; set; }

		// Token: 0x1700025C RID: 604
		// (get) Token: 0x060008FB RID: 2299 RVA: 0x000409F0 File Offset: 0x0003EBF0
		// (set) Token: 0x060008FC RID: 2300 RVA: 0x000409F8 File Offset: 0x0003EBF8
		public int TotalMembers { get; set; }

		// Token: 0x1700025D RID: 605
		// (get) Token: 0x060008FD RID: 2301 RVA: 0x00040A01 File Offset: 0x0003EC01
		// (set) Token: 0x060008FE RID: 2302 RVA: 0x00040A09 File Offset: 0x0003EC09
		public bool IsConfirmed { get; set; }
	}
}
