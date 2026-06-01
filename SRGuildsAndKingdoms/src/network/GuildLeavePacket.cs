using System;
using System.Runtime.CompilerServices;
using ProtoBuf;

namespace SRGuildsAndKingdoms.src.network
{
	// Token: 0x02000026 RID: 38
	[NullableContext(1)]
	[Nullable(0)]
	[ProtoContract(ImplicitFields = 1)]
	public class GuildLeavePacket
	{
		// Token: 0x17000066 RID: 102
		// (get) Token: 0x060001AD RID: 429 RVA: 0x00010B1D File Offset: 0x0000ED1D
		// (set) Token: 0x060001AE RID: 430 RVA: 0x00010B25 File Offset: 0x0000ED25
		public string PlayerUid { get; set; } = "";
	}
}
