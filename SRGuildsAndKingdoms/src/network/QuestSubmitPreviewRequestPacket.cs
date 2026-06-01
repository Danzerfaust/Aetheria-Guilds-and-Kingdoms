using System;
using System.Runtime.CompilerServices;
using ProtoBuf;

namespace SRGuildsAndKingdoms.src.network
{
	// Token: 0x02000058 RID: 88
	[NullableContext(1)]
	[Nullable(0)]
	[ProtoContract(ImplicitFields = 1)]
	public class QuestSubmitPreviewRequestPacket
	{
		// Token: 0x170000F6 RID: 246
		// (get) Token: 0x06000381 RID: 897 RVA: 0x00018873 File Offset: 0x00016A73
		// (set) Token: 0x06000382 RID: 898 RVA: 0x0001887B File Offset: 0x00016A7B
		public string PlayerUid { get; set; } = string.Empty;

		// Token: 0x170000F7 RID: 247
		// (get) Token: 0x06000383 RID: 899 RVA: 0x00018884 File Offset: 0x00016A84
		// (set) Token: 0x06000384 RID: 900 RVA: 0x0001888C File Offset: 0x00016A8C
		public int QuestId { get; set; }
	}
}
