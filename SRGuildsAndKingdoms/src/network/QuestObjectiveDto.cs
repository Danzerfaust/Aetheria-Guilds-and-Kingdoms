using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using ProtoBuf;

namespace SRGuildsAndKingdoms.src.network
{
	// Token: 0x02000063 RID: 99
	[NullableContext(1)]
	[Nullable(0)]
	[ProtoContract(ImplicitFields = 1)]
	public class QuestObjectiveDto
	{
		// Token: 0x1700011F RID: 287
		// (get) Token: 0x060003DE RID: 990 RVA: 0x00018C8F File Offset: 0x00016E8F
		// (set) Token: 0x060003DF RID: 991 RVA: 0x00018C97 File Offset: 0x00016E97
		public int Id { get; set; }

		// Token: 0x17000120 RID: 288
		// (get) Token: 0x060003E0 RID: 992 RVA: 0x00018CA0 File Offset: 0x00016EA0
		// (set) Token: 0x060003E1 RID: 993 RVA: 0x00018CA8 File Offset: 0x00016EA8
		public string Type { get; set; } = string.Empty;

		// Token: 0x17000121 RID: 289
		// (get) Token: 0x060003E2 RID: 994 RVA: 0x00018CB1 File Offset: 0x00016EB1
		// (set) Token: 0x060003E3 RID: 995 RVA: 0x00018CB9 File Offset: 0x00016EB9
		public int Count { get; set; }

		// Token: 0x17000122 RID: 290
		// (get) Token: 0x060003E4 RID: 996 RVA: 0x00018CC2 File Offset: 0x00016EC2
		// (set) Token: 0x060003E5 RID: 997 RVA: 0x00018CCA File Offset: 0x00016ECA
		public List<string> AcceptedTargets { get; set; } = new List<string>();

		// Token: 0x17000123 RID: 291
		// (get) Token: 0x060003E6 RID: 998 RVA: 0x00018CD3 File Offset: 0x00016ED3
		// (set) Token: 0x060003E7 RID: 999 RVA: 0x00018CDB File Offset: 0x00016EDB
		public List<QuestAcceptedItemDto> AcceptedItems { get; set; } = new List<QuestAcceptedItemDto>();
	}
}
