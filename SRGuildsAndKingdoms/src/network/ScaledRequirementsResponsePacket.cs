using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using ProtoBuf;

namespace SRGuildsAndKingdoms.src.network
{
	// Token: 0x02000041 RID: 65
	[NullableContext(1)]
	[Nullable(0)]
	[ProtoContract(ImplicitFields = 1)]
	public class ScaledRequirementsResponsePacket : GuildPacketBase
	{
		// Token: 0x1700009E RID: 158
		// (get) Token: 0x0600027E RID: 638 RVA: 0x00015127 File Offset: 0x00013327
		// (set) Token: 0x0600027F RID: 639 RVA: 0x0001512F File Offset: 0x0001332F
		public Dictionary<string, int> ScaledRequirements { get; set; } = new Dictionary<string, int>();

		// Token: 0x1700009F RID: 159
		// (get) Token: 0x06000280 RID: 640 RVA: 0x00015138 File Offset: 0x00013338
		// (set) Token: 0x06000281 RID: 641 RVA: 0x00015140 File Offset: 0x00013340
		public decimal ResourceScaling { get; set; }

		// Token: 0x170000A0 RID: 160
		// (get) Token: 0x06000282 RID: 642 RVA: 0x00015149 File Offset: 0x00013349
		// (set) Token: 0x06000283 RID: 643 RVA: 0x00015151 File Offset: 0x00013351
		public int MemberCount { get; set; }
	}
}
