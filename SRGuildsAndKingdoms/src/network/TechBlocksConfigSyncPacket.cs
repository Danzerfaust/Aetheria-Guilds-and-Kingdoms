using System;
using System.Runtime.CompilerServices;
using ProtoBuf;

namespace SRGuildsAndKingdoms.src.network
{
	// Token: 0x0200004A RID: 74
	[NullableContext(1)]
	[Nullable(0)]
	[ProtoContract(ImplicitFields = 1)]
	public class TechBlocksConfigSyncPacket : GuildPacketBase
	{
		// Token: 0x170000BF RID: 191
		// (get) Token: 0x060002C9 RID: 713 RVA: 0x000153ED File Offset: 0x000135ED
		// (set) Token: 0x060002CA RID: 714 RVA: 0x000153F5 File Offset: 0x000135F5
		public string ConfigJson { get; set; }

		// Token: 0x170000C0 RID: 192
		// (get) Token: 0x060002CB RID: 715 RVA: 0x000153FE File Offset: 0x000135FE
		// (set) Token: 0x060002CC RID: 716 RVA: 0x00015406 File Offset: 0x00013606
		public string ServerIdentifier { get; set; }
	}
}
