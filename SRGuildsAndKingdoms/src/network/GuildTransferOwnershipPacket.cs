using System;
using System.Runtime.CompilerServices;
using ProtoBuf;

namespace SRGuildsAndKingdoms.src.network
{
	// Token: 0x02000042 RID: 66
	[NullableContext(1)]
	[Nullable(0)]
	[ProtoContract(ImplicitFields = 1)]
	public class GuildTransferOwnershipPacket : GuildPacketBase
	{
		// Token: 0x170000A1 RID: 161
		// (get) Token: 0x06000285 RID: 645 RVA: 0x0001516D File Offset: 0x0001336D
		// (set) Token: 0x06000286 RID: 646 RVA: 0x00015175 File Offset: 0x00013375
		public string TargetPlayerUid { get; set; }
	}
}
