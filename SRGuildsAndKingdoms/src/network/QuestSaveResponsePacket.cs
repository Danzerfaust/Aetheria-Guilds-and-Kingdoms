using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using ProtoBuf;

namespace SRGuildsAndKingdoms.src.network
{
	// Token: 0x0200006C RID: 108
	[NullableContext(1)]
	[Nullable(0)]
	[ProtoContract(ImplicitFields = 1)]
	public class QuestSaveResponsePacket
	{
		// Token: 0x17000145 RID: 325
		// (get) Token: 0x06000433 RID: 1075 RVA: 0x0001905D File Offset: 0x0001725D
		// (set) Token: 0x06000434 RID: 1076 RVA: 0x00019065 File Offset: 0x00017265
		public bool Success { get; set; }

		// Token: 0x17000146 RID: 326
		// (get) Token: 0x06000435 RID: 1077 RVA: 0x0001906E File Offset: 0x0001726E
		// (set) Token: 0x06000436 RID: 1078 RVA: 0x00019076 File Offset: 0x00017276
		public string Message { get; set; } = string.Empty;

		// Token: 0x17000147 RID: 327
		// (get) Token: 0x06000437 RID: 1079 RVA: 0x0001907F File Offset: 0x0001727F
		// (set) Token: 0x06000438 RID: 1080 RVA: 0x00019087 File Offset: 0x00017287
		public List<QuestDto> AllQuests { get; set; } = new List<QuestDto>();
	}
}
