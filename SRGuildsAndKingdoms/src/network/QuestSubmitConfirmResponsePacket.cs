using System;
using System.Runtime.CompilerServices;
using ProtoBuf;

namespace SRGuildsAndKingdoms.src.network
{
	// Token: 0x02000060 RID: 96
	[NullableContext(1)]
	[Nullable(0)]
	[ProtoContract(ImplicitFields = 1)]
	public class QuestSubmitConfirmResponsePacket
	{
		// Token: 0x1700010A RID: 266
		// (get) Token: 0x060003B1 RID: 945 RVA: 0x00018A80 File Offset: 0x00016C80
		// (set) Token: 0x060003B2 RID: 946 RVA: 0x00018A88 File Offset: 0x00016C88
		public bool Success { get; set; }

		// Token: 0x1700010B RID: 267
		// (get) Token: 0x060003B3 RID: 947 RVA: 0x00018A91 File Offset: 0x00016C91
		// (set) Token: 0x060003B4 RID: 948 RVA: 0x00018A99 File Offset: 0x00016C99
		public string Message { get; set; } = string.Empty;

		// Token: 0x1700010C RID: 268
		// (get) Token: 0x060003B5 RID: 949 RVA: 0x00018AA2 File Offset: 0x00016CA2
		// (set) Token: 0x060003B6 RID: 950 RVA: 0x00018AAA File Offset: 0x00016CAA
		public int QuestId { get; set; }

		// Token: 0x1700010D RID: 269
		// (get) Token: 0x060003B7 RID: 951 RVA: 0x00018AB3 File Offset: 0x00016CB3
		// (set) Token: 0x060003B8 RID: 952 RVA: 0x00018ABB File Offset: 0x00016CBB
		public int ItemsConsumed { get; set; }
	}
}
