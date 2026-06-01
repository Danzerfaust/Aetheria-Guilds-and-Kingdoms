using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using ProtoBuf;

namespace SRGuildsAndKingdoms.src.network
{
	// Token: 0x0200005F RID: 95
	[NullableContext(1)]
	[Nullable(0)]
	[ProtoContract(ImplicitFields = 1)]
	public class QuestSubmitPreviewResponsePacket
	{
		// Token: 0x17000106 RID: 262
		// (get) Token: 0x060003A8 RID: 936 RVA: 0x00018A1E File Offset: 0x00016C1E
		// (set) Token: 0x060003A9 RID: 937 RVA: 0x00018A26 File Offset: 0x00016C26
		public bool Success { get; set; }

		// Token: 0x17000107 RID: 263
		// (get) Token: 0x060003AA RID: 938 RVA: 0x00018A2F File Offset: 0x00016C2F
		// (set) Token: 0x060003AB RID: 939 RVA: 0x00018A37 File Offset: 0x00016C37
		public string Message { get; set; } = string.Empty;

		// Token: 0x17000108 RID: 264
		// (get) Token: 0x060003AC RID: 940 RVA: 0x00018A40 File Offset: 0x00016C40
		// (set) Token: 0x060003AD RID: 941 RVA: 0x00018A48 File Offset: 0x00016C48
		public int QuestId { get; set; }

		// Token: 0x17000109 RID: 265
		// (get) Token: 0x060003AE RID: 942 RVA: 0x00018A51 File Offset: 0x00016C51
		// (set) Token: 0x060003AF RID: 943 RVA: 0x00018A59 File Offset: 0x00016C59
		public List<QuestSubmittableItem> Items { get; set; } = new List<QuestSubmittableItem>();
	}
}
