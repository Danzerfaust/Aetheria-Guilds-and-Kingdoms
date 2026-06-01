using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using ProtoBuf;

namespace SRGuildsAndKingdoms.src.network
{
	// Token: 0x02000039 RID: 57
	[NullableContext(1)]
	[Nullable(0)]
	[ProtoContract(ImplicitFields = 1)]
	public class TechContributionResponsePacket : GuildPacketBase
	{
		// Token: 0x17000088 RID: 136
		// (get) Token: 0x0600024A RID: 586 RVA: 0x00014F45 File Offset: 0x00013145
		// (set) Token: 0x0600024B RID: 587 RVA: 0x00014F4D File Offset: 0x0001314D
		public bool Success { get; set; }

		// Token: 0x17000089 RID: 137
		// (get) Token: 0x0600024C RID: 588 RVA: 0x00014F56 File Offset: 0x00013156
		// (set) Token: 0x0600024D RID: 589 RVA: 0x00014F5E File Offset: 0x0001315E
		public string Message { get; set; }

		// Token: 0x1700008A RID: 138
		// (get) Token: 0x0600024E RID: 590 RVA: 0x00014F67 File Offset: 0x00013167
		// (set) Token: 0x0600024F RID: 591 RVA: 0x00014F6F File Offset: 0x0001316F
		public int TechBlockId { get; set; }

		// Token: 0x1700008B RID: 139
		// (get) Token: 0x06000250 RID: 592 RVA: 0x00014F78 File Offset: 0x00013178
		// (set) Token: 0x06000251 RID: 593 RVA: 0x00014F80 File Offset: 0x00013180
		public bool TechUnlocked { get; set; }

		// Token: 0x1700008C RID: 140
		// (get) Token: 0x06000252 RID: 594 RVA: 0x00014F89 File Offset: 0x00013189
		// (set) Token: 0x06000253 RID: 595 RVA: 0x00014F91 File Offset: 0x00013191
		public Dictionary<string, int> UpdatedProgress { get; set; } = new Dictionary<string, int>();
	}
}
