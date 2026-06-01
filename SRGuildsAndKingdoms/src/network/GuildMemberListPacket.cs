using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using ProtoBuf;

namespace SRGuildsAndKingdoms.src.network
{
	// Token: 0x0200003E RID: 62
	[NullableContext(1)]
	[Nullable(0)]
	[ProtoContract(ImplicitFields = 1)]
	public class GuildMemberListPacket : GuildPacketBase
	{
		// Token: 0x17000095 RID: 149
		// (get) Token: 0x06000269 RID: 617 RVA: 0x00015060 File Offset: 0x00013260
		// (set) Token: 0x0600026A RID: 618 RVA: 0x00015068 File Offset: 0x00013268
		public List<GuildMemberInfo> Members { get; set; } = new List<GuildMemberInfo>();
	}
}
