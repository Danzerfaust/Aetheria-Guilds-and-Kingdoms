using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using ProtoBuf;

namespace SRGuildsAndKingdoms.src.network
{
	// Token: 0x0200005C RID: 92
	[NullableContext(1)]
	[Nullable(0)]
	[ProtoContract(ImplicitFields = 1)]
	public class QuestProgressResponsePacket
	{
		// Token: 0x170000FE RID: 254
		// (get) Token: 0x06000395 RID: 917 RVA: 0x00018952 File Offset: 0x00016B52
		// (set) Token: 0x06000396 RID: 918 RVA: 0x0001895A File Offset: 0x00016B5A
		public List<PlayerQuestProgressDto> Progress { get; set; } = new List<PlayerQuestProgressDto>();

		// Token: 0x170000FF RID: 255
		// (get) Token: 0x06000397 RID: 919 RVA: 0x00018963 File Offset: 0x00016B63
		// (set) Token: 0x06000398 RID: 920 RVA: 0x0001896B File Offset: 0x00016B6B
		public List<string> CompletedPeriodKeys { get; set; } = new List<string>();
	}
}
