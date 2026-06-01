using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using ProtoBuf;

namespace SRGuildsAndKingdoms.src.network
{
	// Token: 0x02000059 RID: 89
	[NullableContext(1)]
	[Nullable(0)]
	[ProtoContract(ImplicitFields = 1)]
	public class QuestSubmitConfirmPacket
	{
		// Token: 0x170000F8 RID: 248
		// (get) Token: 0x06000386 RID: 902 RVA: 0x000188A8 File Offset: 0x00016AA8
		// (set) Token: 0x06000387 RID: 903 RVA: 0x000188B0 File Offset: 0x00016AB0
		public string PlayerUid { get; set; } = string.Empty;

		// Token: 0x170000F9 RID: 249
		// (get) Token: 0x06000388 RID: 904 RVA: 0x000188B9 File Offset: 0x00016AB9
		// (set) Token: 0x06000389 RID: 905 RVA: 0x000188C1 File Offset: 0x00016AC1
		public int QuestId { get; set; }

		// Token: 0x170000FA RID: 250
		// (get) Token: 0x0600038A RID: 906 RVA: 0x000188CA File Offset: 0x00016ACA
		// (set) Token: 0x0600038B RID: 907 RVA: 0x000188D2 File Offset: 0x00016AD2
		public List<QuestSubmittableItem> Items { get; set; } = new List<QuestSubmittableItem>();
	}
}
