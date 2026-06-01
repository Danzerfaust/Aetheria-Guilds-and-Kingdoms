using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using ProtoBuf;

namespace SRGuildsAndKingdoms.src.network
{
	// Token: 0x02000062 RID: 98
	[NullableContext(1)]
	[Nullable(0)]
	[ProtoContract(ImplicitFields = 1)]
	public class QuestDto
	{
		// Token: 0x17000113 RID: 275
		// (get) Token: 0x060003C5 RID: 965 RVA: 0x00018B55 File Offset: 0x00016D55
		// (set) Token: 0x060003C6 RID: 966 RVA: 0x00018B5D File Offset: 0x00016D5D
		public int Id { get; set; }

		// Token: 0x17000114 RID: 276
		// (get) Token: 0x060003C7 RID: 967 RVA: 0x00018B66 File Offset: 0x00016D66
		// (set) Token: 0x060003C8 RID: 968 RVA: 0x00018B6E File Offset: 0x00016D6E
		public string RecurrenceType { get; set; } = string.Empty;

		// Token: 0x17000115 RID: 277
		// (get) Token: 0x060003C9 RID: 969 RVA: 0x00018B77 File Offset: 0x00016D77
		// (set) Token: 0x060003CA RID: 970 RVA: 0x00018B7F File Offset: 0x00016D7F
		public string Title { get; set; } = string.Empty;

		// Token: 0x17000116 RID: 278
		// (get) Token: 0x060003CB RID: 971 RVA: 0x00018B88 File Offset: 0x00016D88
		// (set) Token: 0x060003CC RID: 972 RVA: 0x00018B90 File Offset: 0x00016D90
		public string Description { get; set; } = string.Empty;

		// Token: 0x17000117 RID: 279
		// (get) Token: 0x060003CD RID: 973 RVA: 0x00018B99 File Offset: 0x00016D99
		// (set) Token: 0x060003CE RID: 974 RVA: 0x00018BA1 File Offset: 0x00016DA1
		public List<QuestObjectiveDto> Objectives { get; set; } = new List<QuestObjectiveDto>();

		// Token: 0x17000118 RID: 280
		// (get) Token: 0x060003CF RID: 975 RVA: 0x00018BAA File Offset: 0x00016DAA
		// (set) Token: 0x060003D0 RID: 976 RVA: 0x00018BB2 File Offset: 0x00016DB2
		public List<QuestRewardDto> Rewards { get; set; } = new List<QuestRewardDto>();

		// Token: 0x17000119 RID: 281
		// (get) Token: 0x060003D1 RID: 977 RVA: 0x00018BBB File Offset: 0x00016DBB
		// (set) Token: 0x060003D2 RID: 978 RVA: 0x00018BC3 File Offset: 0x00016DC3
		public string StartsAt { get; set; } = string.Empty;

		// Token: 0x1700011A RID: 282
		// (get) Token: 0x060003D3 RID: 979 RVA: 0x00018BCC File Offset: 0x00016DCC
		// (set) Token: 0x060003D4 RID: 980 RVA: 0x00018BD4 File Offset: 0x00016DD4
		public string ExpiresAt { get; set; } = string.Empty;

		// Token: 0x1700011B RID: 283
		// (get) Token: 0x060003D5 RID: 981 RVA: 0x00018BDD File Offset: 0x00016DDD
		// (set) Token: 0x060003D6 RID: 982 RVA: 0x00018BE5 File Offset: 0x00016DE5
		public bool UsesIngameTime { get; set; }

		// Token: 0x1700011C RID: 284
		// (get) Token: 0x060003D7 RID: 983 RVA: 0x00018BEE File Offset: 0x00016DEE
		// (set) Token: 0x060003D8 RID: 984 RVA: 0x00018BF6 File Offset: 0x00016DF6
		public bool Repeat { get; set; }

		// Token: 0x1700011D RID: 285
		// (get) Token: 0x060003D9 RID: 985 RVA: 0x00018BFF File Offset: 0x00016DFF
		// (set) Token: 0x060003DA RID: 986 RVA: 0x00018C07 File Offset: 0x00016E07
		public string PeriodKey { get; set; } = string.Empty;

		// Token: 0x1700011E RID: 286
		// (get) Token: 0x060003DB RID: 987 RVA: 0x00018C10 File Offset: 0x00016E10
		// (set) Token: 0x060003DC RID: 988 RVA: 0x00018C18 File Offset: 0x00016E18
		public bool AlreadyCompletedLastWeek { get; set; }
	}
}
