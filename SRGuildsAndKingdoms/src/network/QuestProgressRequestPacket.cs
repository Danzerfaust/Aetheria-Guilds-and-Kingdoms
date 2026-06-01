using System;
using System.Runtime.CompilerServices;
using ProtoBuf;

namespace SRGuildsAndKingdoms.src.network
{
	// Token: 0x02000055 RID: 85
	[NullableContext(1)]
	[Nullable(0)]
	[ProtoContract(ImplicitFields = 1)]
	public class QuestProgressRequestPacket
	{
		// Token: 0x170000F1 RID: 241
		// (get) Token: 0x06000374 RID: 884 RVA: 0x000187E5 File Offset: 0x000169E5
		// (set) Token: 0x06000375 RID: 885 RVA: 0x000187ED File Offset: 0x000169ED
		public string PlayerUid { get; set; } = string.Empty;
	}
}
