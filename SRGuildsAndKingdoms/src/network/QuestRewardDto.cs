using System;
using System.Runtime.CompilerServices;
using ProtoBuf;

namespace SRGuildsAndKingdoms.src.network
{
	// Token: 0x02000064 RID: 100
	[NullableContext(1)]
	[Nullable(0)]
	[ProtoContract(ImplicitFields = 1)]
	public class QuestRewardDto
	{
		// Token: 0x17000124 RID: 292
		// (get) Token: 0x060003E9 RID: 1001 RVA: 0x00018D0D File Offset: 0x00016F0D
		// (set) Token: 0x060003EA RID: 1002 RVA: 0x00018D15 File Offset: 0x00016F15
		public string Code { get; set; } = string.Empty;

		// Token: 0x17000125 RID: 293
		// (get) Token: 0x060003EB RID: 1003 RVA: 0x00018D1E File Offset: 0x00016F1E
		// (set) Token: 0x060003EC RID: 1004 RVA: 0x00018D26 File Offset: 0x00016F26
		[Nullable(2)]
		public string Nbt { [NullableContext(2)] get; [NullableContext(2)] set; }

		// Token: 0x17000126 RID: 294
		// (get) Token: 0x060003ED RID: 1005 RVA: 0x00018D2F File Offset: 0x00016F2F
		// (set) Token: 0x060003EE RID: 1006 RVA: 0x00018D37 File Offset: 0x00016F37
		public int Amount { get; set; }
	}
}
