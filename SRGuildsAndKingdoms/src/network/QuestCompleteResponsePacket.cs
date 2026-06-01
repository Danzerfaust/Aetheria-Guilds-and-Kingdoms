using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using ProtoBuf;

namespace SRGuildsAndKingdoms.src.network
{
	// Token: 0x02000061 RID: 97
	[NullableContext(1)]
	[Nullable(0)]
	[ProtoContract(ImplicitFields = 1)]
	public class QuestCompleteResponsePacket
	{
		// Token: 0x1700010E RID: 270
		// (get) Token: 0x060003BA RID: 954 RVA: 0x00018AD7 File Offset: 0x00016CD7
		// (set) Token: 0x060003BB RID: 955 RVA: 0x00018ADF File Offset: 0x00016CDF
		public bool Success { get; set; }

		// Token: 0x1700010F RID: 271
		// (get) Token: 0x060003BC RID: 956 RVA: 0x00018AE8 File Offset: 0x00016CE8
		// (set) Token: 0x060003BD RID: 957 RVA: 0x00018AF0 File Offset: 0x00016CF0
		public string Message { get; set; } = string.Empty;

		// Token: 0x17000110 RID: 272
		// (get) Token: 0x060003BE RID: 958 RVA: 0x00018AF9 File Offset: 0x00016CF9
		// (set) Token: 0x060003BF RID: 959 RVA: 0x00018B01 File Offset: 0x00016D01
		public int QuestId { get; set; }

		// Token: 0x17000111 RID: 273
		// (get) Token: 0x060003C0 RID: 960 RVA: 0x00018B0A File Offset: 0x00016D0A
		// (set) Token: 0x060003C1 RID: 961 RVA: 0x00018B12 File Offset: 0x00016D12
		public List<QuestRewardDto> RewardsGranted { get; set; } = new List<QuestRewardDto>();

		// Token: 0x17000112 RID: 274
		// (get) Token: 0x060003C2 RID: 962 RVA: 0x00018B1B File Offset: 0x00016D1B
		// (set) Token: 0x060003C3 RID: 963 RVA: 0x00018B23 File Offset: 0x00016D23
		public string PeriodKey { get; set; } = string.Empty;
	}
}
