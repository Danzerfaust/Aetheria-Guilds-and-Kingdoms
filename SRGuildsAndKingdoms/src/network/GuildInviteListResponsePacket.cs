using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using ProtoBuf;

namespace SRGuildsAndKingdoms.src.network
{
	// Token: 0x02000030 RID: 48
	[NullableContext(1)]
	[Nullable(0)]
	[ProtoContract(ImplicitFields = 1)]
	public class GuildInviteListResponsePacket : GuildPacketBase
	{
		// Token: 0x1700006F RID: 111
		// (get) Token: 0x0600020F RID: 527 RVA: 0x00014D28 File Offset: 0x00012F28
		// (set) Token: 0x06000210 RID: 528 RVA: 0x00014D30 File Offset: 0x00012F30
		public List<GuildInviteInfo> Invites { get; set; } = new List<GuildInviteInfo>();
	}
}
