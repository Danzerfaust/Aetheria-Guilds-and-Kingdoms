using System;
using System.Runtime.CompilerServices;
using ProtoBuf;

namespace SRGuildsAndKingdoms.src.network
{
	// Token: 0x0200005E RID: 94
	[NullableContext(1)]
	[Nullable(0)]
	[ProtoContract(ImplicitFields = 1)]
	public class QuestAbandonResponsePacket
	{
		// Token: 0x17000103 RID: 259
		// (get) Token: 0x060003A1 RID: 929 RVA: 0x000189D8 File Offset: 0x00016BD8
		// (set) Token: 0x060003A2 RID: 930 RVA: 0x000189E0 File Offset: 0x00016BE0
		public bool Success { get; set; }

		// Token: 0x17000104 RID: 260
		// (get) Token: 0x060003A3 RID: 931 RVA: 0x000189E9 File Offset: 0x00016BE9
		// (set) Token: 0x060003A4 RID: 932 RVA: 0x000189F1 File Offset: 0x00016BF1
		public string Message { get; set; } = string.Empty;

		// Token: 0x17000105 RID: 261
		// (get) Token: 0x060003A5 RID: 933 RVA: 0x000189FA File Offset: 0x00016BFA
		// (set) Token: 0x060003A6 RID: 934 RVA: 0x00018A02 File Offset: 0x00016C02
		public int QuestId { get; set; }
	}
}
