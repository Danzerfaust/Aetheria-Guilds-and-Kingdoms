using System;
using System.Runtime.CompilerServices;
using ProtoBuf;

namespace SRGuildsAndKingdoms.src.network
{
	// Token: 0x02000028 RID: 40
	[NullableContext(1)]
	[Nullable(0)]
	[ProtoContract(ImplicitFields = 1)]
	public abstract class GuildPacketBase
	{
		// Token: 0x17000067 RID: 103
		// (get) Token: 0x060001F7 RID: 503 RVA: 0x00014C55 File Offset: 0x00012E55
		// (set) Token: 0x060001F8 RID: 504 RVA: 0x00014C5D File Offset: 0x00012E5D
		public string PlayerUid { get; set; }
	}
}
