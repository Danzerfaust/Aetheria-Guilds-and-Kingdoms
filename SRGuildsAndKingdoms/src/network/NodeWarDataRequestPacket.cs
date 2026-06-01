using System;
using System.Runtime.CompilerServices;
using ProtoBuf;

namespace SRGuildsAndKingdoms.src.network
{
	// Token: 0x0200004B RID: 75
	[NullableContext(1)]
	[Nullable(0)]
	[ProtoContract(ImplicitFields = 1)]
	public class NodeWarDataRequestPacket : GuildPacketBase
	{
		// Token: 0x170000C1 RID: 193
		// (get) Token: 0x060002CE RID: 718 RVA: 0x00015417 File Offset: 0x00013617
		// (set) Token: 0x060002CF RID: 719 RVA: 0x0001541F File Offset: 0x0001361F
		public string GuildName { get; set; }
	}
}
