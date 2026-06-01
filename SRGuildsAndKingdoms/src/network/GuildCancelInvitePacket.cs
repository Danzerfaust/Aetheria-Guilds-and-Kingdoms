using System;
using System.Runtime.CompilerServices;
using ProtoBuf;

namespace SRGuildsAndKingdoms.src.network
{
	// Token: 0x0200002E RID: 46
	[NullableContext(1)]
	[Nullable(0)]
	[ProtoContract(ImplicitFields = 1)]
	public class GuildCancelInvitePacket : GuildPacketBase
	{
		// Token: 0x1700006E RID: 110
		// (get) Token: 0x0600020B RID: 523 RVA: 0x00014D07 File Offset: 0x00012F07
		// (set) Token: 0x0600020C RID: 524 RVA: 0x00014D0F File Offset: 0x00012F0F
		public string InviteeUid { get; set; }
	}
}
