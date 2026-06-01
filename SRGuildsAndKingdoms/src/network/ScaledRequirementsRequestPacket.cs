using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using ProtoBuf;

namespace SRGuildsAndKingdoms.src.network
{
	// Token: 0x02000040 RID: 64
	[NullableContext(1)]
	[Nullable(0)]
	[ProtoContract(ImplicitFields = 1)]
	public class ScaledRequirementsRequestPacket : GuildPacketBase
	{
		// Token: 0x1700009B RID: 155
		// (get) Token: 0x06000277 RID: 631 RVA: 0x000150E1 File Offset: 0x000132E1
		// (set) Token: 0x06000278 RID: 632 RVA: 0x000150E9 File Offset: 0x000132E9
		public string GuildName { get; set; }

		// Token: 0x1700009C RID: 156
		// (get) Token: 0x06000279 RID: 633 RVA: 0x000150F2 File Offset: 0x000132F2
		// (set) Token: 0x0600027A RID: 634 RVA: 0x000150FA File Offset: 0x000132FA
		public int TechBlockId { get; set; }

		// Token: 0x1700009D RID: 157
		// (get) Token: 0x0600027B RID: 635 RVA: 0x00015103 File Offset: 0x00013303
		// (set) Token: 0x0600027C RID: 636 RVA: 0x0001510B File Offset: 0x0001330B
		public Dictionary<string, int> BaseRequirements { get; set; } = new Dictionary<string, int>();
	}
}
