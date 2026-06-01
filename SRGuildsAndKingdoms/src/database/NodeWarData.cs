using System;
using System.Runtime.CompilerServices;

namespace SRGuildsAndKingdoms.src.database
{
	// Token: 0x020000BB RID: 187
	[NullableContext(2)]
	[Nullable(0)]
	public class NodeWarData
	{
		// Token: 0x1700023F RID: 575
		// (get) Token: 0x060008BF RID: 2239 RVA: 0x000407C7 File Offset: 0x0003E9C7
		// (set) Token: 0x060008C0 RID: 2240 RVA: 0x000407CF File Offset: 0x0003E9CF
		public int Id { get; set; }

		// Token: 0x17000240 RID: 576
		// (get) Token: 0x060008C1 RID: 2241 RVA: 0x000407D8 File Offset: 0x0003E9D8
		// (set) Token: 0x060008C2 RID: 2242 RVA: 0x000407E0 File Offset: 0x0003E9E0
		[Nullable(1)]
		public string NodeId { [NullableContext(1)] get; [NullableContext(1)] set; } = string.Empty;

		// Token: 0x17000241 RID: 577
		// (get) Token: 0x060008C3 RID: 2243 RVA: 0x000407E9 File Offset: 0x0003E9E9
		// (set) Token: 0x060008C4 RID: 2244 RVA: 0x000407F1 File Offset: 0x0003E9F1
		[Nullable(1)]
		public string Status { [NullableContext(1)] get; [NullableContext(1)] set; } = string.Empty;

		// Token: 0x17000242 RID: 578
		// (get) Token: 0x060008C5 RID: 2245 RVA: 0x000407FA File Offset: 0x0003E9FA
		// (set) Token: 0x060008C6 RID: 2246 RVA: 0x00040802 File Offset: 0x0003EA02
		public long StartTime { get; set; }

		// Token: 0x17000243 RID: 579
		// (get) Token: 0x060008C7 RID: 2247 RVA: 0x0004080B File Offset: 0x0003EA0B
		// (set) Token: 0x060008C8 RID: 2248 RVA: 0x00040813 File Offset: 0x0003EA13
		public long? EndTime { get; set; }

		// Token: 0x17000244 RID: 580
		// (get) Token: 0x060008C9 RID: 2249 RVA: 0x0004081C File Offset: 0x0003EA1C
		// (set) Token: 0x060008CA RID: 2250 RVA: 0x00040824 File Offset: 0x0003EA24
		public long? SignupDeadline { get; set; }

		// Token: 0x17000245 RID: 581
		// (get) Token: 0x060008CB RID: 2251 RVA: 0x0004082D File Offset: 0x0003EA2D
		// (set) Token: 0x060008CC RID: 2252 RVA: 0x00040835 File Offset: 0x0003EA35
		public int MaxGuilds { get; set; }

		// Token: 0x17000246 RID: 582
		// (get) Token: 0x060008CD RID: 2253 RVA: 0x0004083E File Offset: 0x0003EA3E
		// (set) Token: 0x060008CE RID: 2254 RVA: 0x00040846 File Offset: 0x0003EA46
		public double CapturePointsNeeded { get; set; }

		// Token: 0x17000247 RID: 583
		// (get) Token: 0x060008CF RID: 2255 RVA: 0x0004084F File Offset: 0x0003EA4F
		// (set) Token: 0x060008D0 RID: 2256 RVA: 0x00040857 File Offset: 0x0003EA57
		public string ControllingGuildUid { get; set; }

		// Token: 0x17000248 RID: 584
		// (get) Token: 0x060008D1 RID: 2257 RVA: 0x00040860 File Offset: 0x0003EA60
		// (set) Token: 0x060008D2 RID: 2258 RVA: 0x00040868 File Offset: 0x0003EA68
		public string ControllingGuildName { get; set; }

		// Token: 0x17000249 RID: 585
		// (get) Token: 0x060008D3 RID: 2259 RVA: 0x00040871 File Offset: 0x0003EA71
		// (set) Token: 0x060008D4 RID: 2260 RVA: 0x00040879 File Offset: 0x0003EA79
		public string PreviousControllingGuildUid { get; set; }

		// Token: 0x1700024A RID: 586
		// (get) Token: 0x060008D5 RID: 2261 RVA: 0x00040882 File Offset: 0x0003EA82
		// (set) Token: 0x060008D6 RID: 2262 RVA: 0x0004088A File Offset: 0x0003EA8A
		public long CreatedAt { get; set; }

		// Token: 0x1700024B RID: 587
		// (get) Token: 0x060008D7 RID: 2263 RVA: 0x00040893 File Offset: 0x0003EA93
		// (set) Token: 0x060008D8 RID: 2264 RVA: 0x0004089B File Offset: 0x0003EA9B
		public long UpdatedAt { get; set; }
	}
}
