using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using ProtoBuf;
using SRGuildsAndKingdoms.src.guilds;

namespace SRGuildsAndKingdoms.src.network
{
	// Token: 0x0200003A RID: 58
	[NullableContext(1)]
	[Nullable(0)]
	[ProtoContract(ImplicitFields = 1)]
	public class GuildSyncPacket : GuildPacketBase
	{
		// Token: 0x1700008D RID: 141
		// (get) Token: 0x06000255 RID: 597 RVA: 0x00014FAD File Offset: 0x000131AD
		// (set) Token: 0x06000256 RID: 598 RVA: 0x00014FB5 File Offset: 0x000131B5
		public List<GuildSummary> GuildSummaries { get; set; } = new List<GuildSummary>();
	}
}
