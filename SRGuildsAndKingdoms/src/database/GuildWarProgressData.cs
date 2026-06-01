using System;
using System.Runtime.CompilerServices;

namespace SRGuildsAndKingdoms.src.database
{
	// Token: 0x020000BC RID: 188
	[NullableContext(1)]
	[Nullable(0)]
	public class GuildWarProgressData
	{
		// Token: 0x1700024C RID: 588
		// (get) Token: 0x060008DA RID: 2266 RVA: 0x000408C2 File Offset: 0x0003EAC2
		// (set) Token: 0x060008DB RID: 2267 RVA: 0x000408CA File Offset: 0x0003EACA
		public int Id { get; set; }

		// Token: 0x1700024D RID: 589
		// (get) Token: 0x060008DC RID: 2268 RVA: 0x000408D3 File Offset: 0x0003EAD3
		// (set) Token: 0x060008DD RID: 2269 RVA: 0x000408DB File Offset: 0x0003EADB
		public int WarId { get; set; }

		// Token: 0x1700024E RID: 590
		// (get) Token: 0x060008DE RID: 2270 RVA: 0x000408E4 File Offset: 0x0003EAE4
		// (set) Token: 0x060008DF RID: 2271 RVA: 0x000408EC File Offset: 0x0003EAEC
		public string GuildUid { get; set; } = string.Empty;

		// Token: 0x1700024F RID: 591
		// (get) Token: 0x060008E0 RID: 2272 RVA: 0x000408F5 File Offset: 0x0003EAF5
		// (set) Token: 0x060008E1 RID: 2273 RVA: 0x000408FD File Offset: 0x0003EAFD
		public string GuildName { get; set; } = string.Empty;

		// Token: 0x17000250 RID: 592
		// (get) Token: 0x060008E2 RID: 2274 RVA: 0x00040906 File Offset: 0x0003EB06
		// (set) Token: 0x060008E3 RID: 2275 RVA: 0x0004090E File Offset: 0x0003EB0E
		public double CapturePoints { get; set; }

		// Token: 0x17000251 RID: 593
		// (get) Token: 0x060008E4 RID: 2276 RVA: 0x00040917 File Offset: 0x0003EB17
		// (set) Token: 0x060008E5 RID: 2277 RVA: 0x0004091F File Offset: 0x0003EB1F
		public int PlayersInZone { get; set; }

		// Token: 0x17000252 RID: 594
		// (get) Token: 0x060008E6 RID: 2278 RVA: 0x00040928 File Offset: 0x0003EB28
		// (set) Token: 0x060008E7 RID: 2279 RVA: 0x00040930 File Offset: 0x0003EB30
		public int Kills { get; set; }

		// Token: 0x17000253 RID: 595
		// (get) Token: 0x060008E8 RID: 2280 RVA: 0x00040939 File Offset: 0x0003EB39
		// (set) Token: 0x060008E9 RID: 2281 RVA: 0x00040941 File Offset: 0x0003EB41
		public int Deaths { get; set; }

		// Token: 0x17000254 RID: 596
		// (get) Token: 0x060008EA RID: 2282 RVA: 0x0004094A File Offset: 0x0003EB4A
		// (set) Token: 0x060008EB RID: 2283 RVA: 0x00040952 File Offset: 0x0003EB52
		public long LastUpdate { get; set; }
	}
}
