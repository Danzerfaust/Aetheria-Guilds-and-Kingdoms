using System;
using System.Runtime.CompilerServices;
using ProtoBuf;

namespace SRGuildsAndKingdoms.src.network
{
	// Token: 0x0200004D RID: 77
	[NullableContext(1)]
	[Nullable(0)]
	[ProtoContract(ImplicitFields = 1)]
	public class ControlledNodeDto
	{
		// Token: 0x170000C6 RID: 198
		// (get) Token: 0x060002DA RID: 730 RVA: 0x00015492 File Offset: 0x00013692
		// (set) Token: 0x060002DB RID: 731 RVA: 0x0001549A File Offset: 0x0001369A
		public string NodeId { get; set; } = string.Empty;

		// Token: 0x170000C7 RID: 199
		// (get) Token: 0x060002DC RID: 732 RVA: 0x000154A3 File Offset: 0x000136A3
		// (set) Token: 0x060002DD RID: 733 RVA: 0x000154AB File Offset: 0x000136AB
		public string NodeName { get; set; } = string.Empty;

		// Token: 0x170000C8 RID: 200
		// (get) Token: 0x060002DE RID: 734 RVA: 0x000154B4 File Offset: 0x000136B4
		// (set) Token: 0x060002DF RID: 735 RVA: 0x000154BC File Offset: 0x000136BC
		public long CapturedAtTicks { get; set; }

		// Token: 0x170000C9 RID: 201
		// (get) Token: 0x060002E0 RID: 736 RVA: 0x000154C5 File Offset: 0x000136C5
		// (set) Token: 0x060002E1 RID: 737 RVA: 0x000154CD File Offset: 0x000136CD
		public int InfluencePerDay { get; set; }

		// Token: 0x170000CA RID: 202
		// (get) Token: 0x060002E2 RID: 738 RVA: 0x000154D6 File Offset: 0x000136D6
		// (set) Token: 0x060002E3 RID: 739 RVA: 0x000154DE File Offset: 0x000136DE
		public int? WarStatus { get; set; }

		// Token: 0x170000CB RID: 203
		// (get) Token: 0x060002E4 RID: 740 RVA: 0x000154E7 File Offset: 0x000136E7
		// (set) Token: 0x060002E5 RID: 741 RVA: 0x000154EF File Offset: 0x000136EF
		public long? WarScheduledStartTimeTicks { get; set; }

		// Token: 0x170000CC RID: 204
		// (get) Token: 0x060002E6 RID: 742 RVA: 0x000154F8 File Offset: 0x000136F8
		// (set) Token: 0x060002E7 RID: 743 RVA: 0x00015500 File Offset: 0x00013700
		public long? WarStartedTimeTicks { get; set; }

		// Token: 0x170000CD RID: 205
		// (get) Token: 0x060002E8 RID: 744 RVA: 0x00015509 File Offset: 0x00013709
		// (set) Token: 0x060002E9 RID: 745 RVA: 0x00015511 File Offset: 0x00013711
		public long? WarEndTimeTicks { get; set; }

		// Token: 0x170000CE RID: 206
		// (get) Token: 0x060002EA RID: 746 RVA: 0x0001551A File Offset: 0x0001371A
		// (set) Token: 0x060002EB RID: 747 RVA: 0x00015522 File Offset: 0x00013722
		public int? WarSignupCount { get; set; }

		// Token: 0x170000CF RID: 207
		// (get) Token: 0x060002EC RID: 748 RVA: 0x0001552B File Offset: 0x0001372B
		// (set) Token: 0x060002ED RID: 749 RVA: 0x00015533 File Offset: 0x00013733
		public int? WarMaxGuilds { get; set; }

		// Token: 0x170000D0 RID: 208
		// (get) Token: 0x060002EE RID: 750 RVA: 0x0001553C File Offset: 0x0001373C
		// (set) Token: 0x060002EF RID: 751 RVA: 0x00015544 File Offset: 0x00013744
		[Nullable(2)]
		public string WarWinnerGuildName { [NullableContext(2)] get; [NullableContext(2)] set; }
	}
}
