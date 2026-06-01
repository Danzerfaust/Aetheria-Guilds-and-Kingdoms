using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using ProtoBuf;

namespace SRGuildsAndKingdoms.src.network
{
	// Token: 0x0200005B RID: 91
	[NullableContext(1)]
	[Nullable(0)]
	[ProtoContract(ImplicitFields = 1)]
	public class QuestListResponsePacket
	{
		// Token: 0x170000FD RID: 253
		// (get) Token: 0x06000392 RID: 914 RVA: 0x0001892E File Offset: 0x00016B2E
		// (set) Token: 0x06000393 RID: 915 RVA: 0x00018936 File Offset: 0x00016B36
		public List<QuestDto> Quests { get; set; } = new List<QuestDto>();
	}
}
