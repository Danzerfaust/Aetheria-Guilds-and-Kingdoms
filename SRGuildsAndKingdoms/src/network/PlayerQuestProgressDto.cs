using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using ProtoBuf;

namespace SRGuildsAndKingdoms.src.network
{
	// Token: 0x02000066 RID: 102
	[NullableContext(1)]
	[Nullable(0)]
	[ProtoContract(ImplicitFields = 1)]
	public class PlayerQuestProgressDto
	{
		// Token: 0x17000129 RID: 297
		// (get) Token: 0x060003F5 RID: 1013 RVA: 0x00018D88 File Offset: 0x00016F88
		// (set) Token: 0x060003F6 RID: 1014 RVA: 0x00018D90 File Offset: 0x00016F90
		public int QuestId { get; set; }

		// Token: 0x1700012A RID: 298
		// (get) Token: 0x060003F7 RID: 1015 RVA: 0x00018D99 File Offset: 0x00016F99
		// (set) Token: 0x060003F8 RID: 1016 RVA: 0x00018DA1 File Offset: 0x00016FA1
		public string Status { get; set; } = string.Empty;

		// Token: 0x1700012B RID: 299
		// (get) Token: 0x060003F9 RID: 1017 RVA: 0x00018DAA File Offset: 0x00016FAA
		// (set) Token: 0x060003FA RID: 1018 RVA: 0x00018DB2 File Offset: 0x00016FB2
		public Dictionary<int, int> ObjectiveProgress { get; set; } = new Dictionary<int, int>();

		// Token: 0x1700012C RID: 300
		// (get) Token: 0x060003FB RID: 1019 RVA: 0x00018DBB File Offset: 0x00016FBB
		// (set) Token: 0x060003FC RID: 1020 RVA: 0x00018DC3 File Offset: 0x00016FC3
		public long StartedAt { get; set; }

		// Token: 0x1700012D RID: 301
		// (get) Token: 0x060003FD RID: 1021 RVA: 0x00018DCC File Offset: 0x00016FCC
		// (set) Token: 0x060003FE RID: 1022 RVA: 0x00018DD4 File Offset: 0x00016FD4
		public long? CompletedAt { get; set; }

		// Token: 0x1700012E RID: 302
		// (get) Token: 0x060003FF RID: 1023 RVA: 0x00018DDD File Offset: 0x00016FDD
		// (set) Token: 0x06000400 RID: 1024 RVA: 0x00018DE5 File Offset: 0x00016FE5
		public string PeriodKey { get; set; } = string.Empty;

		// Token: 0x1700012F RID: 303
		// (get) Token: 0x06000401 RID: 1025 RVA: 0x00018DEE File Offset: 0x00016FEE
		// (set) Token: 0x06000402 RID: 1026 RVA: 0x00018DF6 File Offset: 0x00016FF6
		public string QuestTitle { get; set; } = string.Empty;

		// Token: 0x17000130 RID: 304
		// (get) Token: 0x06000403 RID: 1027 RVA: 0x00018DFF File Offset: 0x00016FFF
		// (set) Token: 0x06000404 RID: 1028 RVA: 0x00018E07 File Offset: 0x00017007
		public string QuestDescription { get; set; } = string.Empty;

		// Token: 0x17000131 RID: 305
		// (get) Token: 0x06000405 RID: 1029 RVA: 0x00018E10 File Offset: 0x00017010
		// (set) Token: 0x06000406 RID: 1030 RVA: 0x00018E18 File Offset: 0x00017018
		public string RecurrenceType { get; set; } = string.Empty;

		// Token: 0x17000132 RID: 306
		// (get) Token: 0x06000407 RID: 1031 RVA: 0x00018E21 File Offset: 0x00017021
		// (set) Token: 0x06000408 RID: 1032 RVA: 0x00018E29 File Offset: 0x00017029
		public string ExpiresAt { get; set; } = string.Empty;

		// Token: 0x17000133 RID: 307
		// (get) Token: 0x06000409 RID: 1033 RVA: 0x00018E32 File Offset: 0x00017032
		// (set) Token: 0x0600040A RID: 1034 RVA: 0x00018E3A File Offset: 0x0001703A
		public bool UsesIngameTime { get; set; }

		// Token: 0x17000134 RID: 308
		// (get) Token: 0x0600040B RID: 1035 RVA: 0x00018E43 File Offset: 0x00017043
		// (set) Token: 0x0600040C RID: 1036 RVA: 0x00018E4B File Offset: 0x0001704B
		public List<QuestObjectiveDto> Objectives { get; set; } = new List<QuestObjectiveDto>();

		// Token: 0x17000135 RID: 309
		// (get) Token: 0x0600040D RID: 1037 RVA: 0x00018E54 File Offset: 0x00017054
		// (set) Token: 0x0600040E RID: 1038 RVA: 0x00018E5C File Offset: 0x0001705C
		public List<QuestRewardDto> Rewards { get; set; } = new List<QuestRewardDto>();
	}
}
