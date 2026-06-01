using System;
using System.Runtime.CompilerServices;
using ProtoBuf;

namespace SRGuildsAndKingdoms.src.network
{
	// Token: 0x02000056 RID: 86
	[NullableContext(1)]
	[Nullable(0)]
	[ProtoContract(ImplicitFields = 1)]
	public class QuestStartRequestPacket
	{
		// Token: 0x170000F2 RID: 242
		// (get) Token: 0x06000377 RID: 887 RVA: 0x00018809 File Offset: 0x00016A09
		// (set) Token: 0x06000378 RID: 888 RVA: 0x00018811 File Offset: 0x00016A11
		public string PlayerUid { get; set; } = string.Empty;

		// Token: 0x170000F3 RID: 243
		// (get) Token: 0x06000379 RID: 889 RVA: 0x0001881A File Offset: 0x00016A1A
		// (set) Token: 0x0600037A RID: 890 RVA: 0x00018822 File Offset: 0x00016A22
		public int QuestId { get; set; }
	}
}
