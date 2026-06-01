using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using ProtoBuf;

namespace SRGuildsAndKingdoms.src.network
{
	// Token: 0x02000070 RID: 112
	[NullableContext(1)]
	[Nullable(0)]
	[ProtoContract(ImplicitFields = 1)]
	public class QuestSaveDto
	{
		// Token: 0x1700014F RID: 335
		// (get) Token: 0x0600044B RID: 1099 RVA: 0x00019169 File Offset: 0x00017369
		// (set) Token: 0x0600044C RID: 1100 RVA: 0x00019171 File Offset: 0x00017371
		public int? Id { get; set; }

		// Token: 0x17000150 RID: 336
		// (get) Token: 0x0600044D RID: 1101 RVA: 0x0001917A File Offset: 0x0001737A
		// (set) Token: 0x0600044E RID: 1102 RVA: 0x00019182 File Offset: 0x00017382
		public string RecurrenceType { get; set; } = string.Empty;

		// Token: 0x17000151 RID: 337
		// (get) Token: 0x0600044F RID: 1103 RVA: 0x0001918B File Offset: 0x0001738B
		// (set) Token: 0x06000450 RID: 1104 RVA: 0x00019193 File Offset: 0x00017393
		public string Title { get; set; } = string.Empty;

		// Token: 0x17000152 RID: 338
		// (get) Token: 0x06000451 RID: 1105 RVA: 0x0001919C File Offset: 0x0001739C
		// (set) Token: 0x06000452 RID: 1106 RVA: 0x000191A4 File Offset: 0x000173A4
		public string Description { get; set; } = string.Empty;

		// Token: 0x17000153 RID: 339
		// (get) Token: 0x06000453 RID: 1107 RVA: 0x000191AD File Offset: 0x000173AD
		// (set) Token: 0x06000454 RID: 1108 RVA: 0x000191B5 File Offset: 0x000173B5
		public List<QuestObjectiveDto> Objectives { get; set; } = new List<QuestObjectiveDto>();

		// Token: 0x17000154 RID: 340
		// (get) Token: 0x06000455 RID: 1109 RVA: 0x000191BE File Offset: 0x000173BE
		// (set) Token: 0x06000456 RID: 1110 RVA: 0x000191C6 File Offset: 0x000173C6
		public List<QuestRewardDto> Rewards { get; set; } = new List<QuestRewardDto>();

		// Token: 0x17000155 RID: 341
		// (get) Token: 0x06000457 RID: 1111 RVA: 0x000191CF File Offset: 0x000173CF
		// (set) Token: 0x06000458 RID: 1112 RVA: 0x000191D7 File Offset: 0x000173D7
		public string StartsAt { get; set; } = string.Empty;

		// Token: 0x17000156 RID: 342
		// (get) Token: 0x06000459 RID: 1113 RVA: 0x000191E0 File Offset: 0x000173E0
		// (set) Token: 0x0600045A RID: 1114 RVA: 0x000191E8 File Offset: 0x000173E8
		public string ExpiresAt { get; set; } = string.Empty;

		// Token: 0x17000157 RID: 343
		// (get) Token: 0x0600045B RID: 1115 RVA: 0x000191F1 File Offset: 0x000173F1
		// (set) Token: 0x0600045C RID: 1116 RVA: 0x000191F9 File Offset: 0x000173F9
		public bool UsesIngameTime { get; set; }

		// Token: 0x17000158 RID: 344
		// (get) Token: 0x0600045D RID: 1117 RVA: 0x00019202 File Offset: 0x00017402
		// (set) Token: 0x0600045E RID: 1118 RVA: 0x0001920A File Offset: 0x0001740A
		public bool Repeat { get; set; }
	}
}
