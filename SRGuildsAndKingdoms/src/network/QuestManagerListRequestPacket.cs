using System;
using System.Runtime.CompilerServices;
using ProtoBuf;

namespace SRGuildsAndKingdoms.src.network
{
	// Token: 0x02000068 RID: 104
	[NullableContext(1)]
	[Nullable(0)]
	[ProtoContract(ImplicitFields = 1)]
	public class QuestManagerListRequestPacket
	{
		// Token: 0x1700013A RID: 314
		// (get) Token: 0x06000419 RID: 1049 RVA: 0x00018F40 File Offset: 0x00017140
		// (set) Token: 0x0600041A RID: 1050 RVA: 0x00018F48 File Offset: 0x00017148
		public string PlayerUid { get; set; } = string.Empty;
	}
}
