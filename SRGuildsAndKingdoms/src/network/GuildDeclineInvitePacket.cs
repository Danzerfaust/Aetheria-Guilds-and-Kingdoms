using System;
using System.Runtime.CompilerServices;
using ProtoBuf;

namespace SRGuildsAndKingdoms.src.network
{
	// Token: 0x0200002D RID: 45
	[NullableContext(1)]
	[Nullable(0)]
	[ProtoContract(ImplicitFields = 1)]
	public class GuildDeclineInvitePacket : GuildPacketBase
	{
		// Token: 0x1700006D RID: 109
		// (get) Token: 0x06000208 RID: 520 RVA: 0x00014CEE File Offset: 0x00012EEE
		// (set) Token: 0x06000209 RID: 521 RVA: 0x00014CF6 File Offset: 0x00012EF6
		public string GuildName { get; set; }
	}
}
