using System;
using System.Runtime.CompilerServices;
using ProtoBuf;

namespace SRGuildsAndKingdoms.src.network
{
	// Token: 0x0200005D RID: 93
	[NullableContext(1)]
	[Nullable(0)]
	[ProtoContract(ImplicitFields = 1)]
	public class QuestStartResponsePacket
	{
		// Token: 0x17000100 RID: 256
		// (get) Token: 0x0600039A RID: 922 RVA: 0x00018992 File Offset: 0x00016B92
		// (set) Token: 0x0600039B RID: 923 RVA: 0x0001899A File Offset: 0x00016B9A
		public bool Success { get; set; }

		// Token: 0x17000101 RID: 257
		// (get) Token: 0x0600039C RID: 924 RVA: 0x000189A3 File Offset: 0x00016BA3
		// (set) Token: 0x0600039D RID: 925 RVA: 0x000189AB File Offset: 0x00016BAB
		public string Message { get; set; } = string.Empty;

		// Token: 0x17000102 RID: 258
		// (get) Token: 0x0600039E RID: 926 RVA: 0x000189B4 File Offset: 0x00016BB4
		// (set) Token: 0x0600039F RID: 927 RVA: 0x000189BC File Offset: 0x00016BBC
		public int QuestId { get; set; }
	}
}
