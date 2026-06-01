using System;
using System.Runtime.CompilerServices;
using ProtoBuf;

namespace SRGuildsAndKingdoms.src.network
{
	// Token: 0x02000054 RID: 84
	[NullableContext(1)]
	[Nullable(0)]
	[ProtoContract(ImplicitFields = 1)]
	public class QuestListRequestPacket
	{
		// Token: 0x170000F0 RID: 240
		// (get) Token: 0x06000371 RID: 881 RVA: 0x000187C1 File Offset: 0x000169C1
		// (set) Token: 0x06000372 RID: 882 RVA: 0x000187C9 File Offset: 0x000169C9
		public string PlayerUid { get; set; } = string.Empty;
	}
}
