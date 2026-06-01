using System;
using System.Runtime.CompilerServices;
using ProtoBuf;

namespace SRGuildsAndKingdoms.src.network
{
	// Token: 0x02000065 RID: 101
	[NullableContext(1)]
	[Nullable(0)]
	[ProtoContract(ImplicitFields = 1)]
	public class QuestAcceptedItemDto
	{
		// Token: 0x17000127 RID: 295
		// (get) Token: 0x060003F0 RID: 1008 RVA: 0x00018D53 File Offset: 0x00016F53
		// (set) Token: 0x060003F1 RID: 1009 RVA: 0x00018D5B File Offset: 0x00016F5B
		public string Code { get; set; } = string.Empty;

		// Token: 0x17000128 RID: 296
		// (get) Token: 0x060003F2 RID: 1010 RVA: 0x00018D64 File Offset: 0x00016F64
		// (set) Token: 0x060003F3 RID: 1011 RVA: 0x00018D6C File Offset: 0x00016F6C
		[Nullable(2)]
		public string Nbt { [NullableContext(2)] get; [NullableContext(2)] set; }
	}
}
