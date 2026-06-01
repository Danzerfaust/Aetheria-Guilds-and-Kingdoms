using System;
using System.Runtime.CompilerServices;
using ProtoBuf;

namespace SRGuildsAndKingdoms.src.network
{
	// Token: 0x0200005A RID: 90
	[NullableContext(1)]
	[Nullable(0)]
	[ProtoContract(ImplicitFields = 1)]
	public class QuestCompleteRequestPacket
	{
		// Token: 0x170000FB RID: 251
		// (get) Token: 0x0600038D RID: 909 RVA: 0x000188F9 File Offset: 0x00016AF9
		// (set) Token: 0x0600038E RID: 910 RVA: 0x00018901 File Offset: 0x00016B01
		public string PlayerUid { get; set; } = string.Empty;

		// Token: 0x170000FC RID: 252
		// (get) Token: 0x0600038F RID: 911 RVA: 0x0001890A File Offset: 0x00016B0A
		// (set) Token: 0x06000390 RID: 912 RVA: 0x00018912 File Offset: 0x00016B12
		public int QuestId { get; set; }
	}
}
