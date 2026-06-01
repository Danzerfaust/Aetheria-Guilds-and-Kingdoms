using System;
using System.Runtime.CompilerServices;
using ProtoBuf;

namespace SRGuildsAndKingdoms.src.network
{
	// Token: 0x02000057 RID: 87
	[NullableContext(1)]
	[Nullable(0)]
	[ProtoContract(ImplicitFields = 1)]
	public class QuestAbandonRequestPacket
	{
		// Token: 0x170000F4 RID: 244
		// (get) Token: 0x0600037C RID: 892 RVA: 0x0001883E File Offset: 0x00016A3E
		// (set) Token: 0x0600037D RID: 893 RVA: 0x00018846 File Offset: 0x00016A46
		public string PlayerUid { get; set; } = string.Empty;

		// Token: 0x170000F5 RID: 245
		// (get) Token: 0x0600037E RID: 894 RVA: 0x0001884F File Offset: 0x00016A4F
		// (set) Token: 0x0600037F RID: 895 RVA: 0x00018857 File Offset: 0x00016A57
		public int QuestId { get; set; }
	}
}
