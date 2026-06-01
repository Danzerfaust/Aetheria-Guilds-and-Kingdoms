using System;
using System.Runtime.CompilerServices;
using ProtoBuf;
using SRGuildsAndKingdoms.src.guilds;

namespace SRGuildsAndKingdoms.src.network
{
	// Token: 0x0200003B RID: 59
	[NullableContext(1)]
	[Nullable(0)]
	[ProtoContract(ImplicitFields = 1)]
	public class GuildUpdatePacket : GuildPacketBase
	{
		// Token: 0x1700008E RID: 142
		// (get) Token: 0x06000258 RID: 600 RVA: 0x00014FD1 File Offset: 0x000131D1
		// (set) Token: 0x06000259 RID: 601 RVA: 0x00014FD9 File Offset: 0x000131D9
		public GuildSummary UpdatedGuild { get; set; }
	}
}
