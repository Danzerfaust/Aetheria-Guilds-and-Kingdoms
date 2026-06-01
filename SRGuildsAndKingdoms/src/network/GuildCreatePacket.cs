using System;
using System.Runtime.CompilerServices;
using ProtoBuf;

namespace SRGuildsAndKingdoms.src.network
{
	// Token: 0x0200002A RID: 42
	[NullableContext(1)]
	[Nullable(0)]
	[ProtoContract(ImplicitFields = 1)]
	public class GuildCreatePacket : GuildPacketBase
	{
		// Token: 0x1700006A RID: 106
		// (get) Token: 0x060001FF RID: 511 RVA: 0x00014C98 File Offset: 0x00012E98
		// (set) Token: 0x06000200 RID: 512 RVA: 0x00014CA0 File Offset: 0x00012EA0
		public string GuildName { get; set; }

		// Token: 0x1700006B RID: 107
		// (get) Token: 0x06000201 RID: 513 RVA: 0x00014CA9 File Offset: 0x00012EA9
		// (set) Token: 0x06000202 RID: 514 RVA: 0x00014CB1 File Offset: 0x00012EB1
		public string Description { get; set; } = "";
	}
}
