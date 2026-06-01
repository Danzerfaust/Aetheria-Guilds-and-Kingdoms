using System;
using System.Runtime.CompilerServices;
using ProtoBuf;

namespace SRGuildsAndKingdoms.src.network
{
	// Token: 0x02000029 RID: 41
	[NullableContext(1)]
	[Nullable(0)]
	[ProtoContract(ImplicitFields = 1)]
	public class GuildCommandPacket : GuildPacketBase
	{
		// Token: 0x17000068 RID: 104
		// (get) Token: 0x060001FA RID: 506 RVA: 0x00014C6E File Offset: 0x00012E6E
		// (set) Token: 0x060001FB RID: 507 RVA: 0x00014C76 File Offset: 0x00012E76
		public string Command { get; set; }

		// Token: 0x17000069 RID: 105
		// (get) Token: 0x060001FC RID: 508 RVA: 0x00014C7F File Offset: 0x00012E7F
		// (set) Token: 0x060001FD RID: 509 RVA: 0x00014C87 File Offset: 0x00012E87
		public string[] Arguments { get; set; }
	}
}
