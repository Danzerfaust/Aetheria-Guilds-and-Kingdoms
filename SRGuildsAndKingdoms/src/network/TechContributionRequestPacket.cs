using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using ProtoBuf;

namespace SRGuildsAndKingdoms.src.network
{
	// Token: 0x02000037 RID: 55
	[NullableContext(1)]
	[Nullable(0)]
	[ProtoContract(ImplicitFields = 1)]
	public class TechContributionRequestPacket : GuildPacketBase
	{
		// Token: 0x17000080 RID: 128
		// (get) Token: 0x06000238 RID: 568 RVA: 0x00014EA2 File Offset: 0x000130A2
		// (set) Token: 0x06000239 RID: 569 RVA: 0x00014EAA File Offset: 0x000130AA
		public string GuildName { get; set; }

		// Token: 0x17000081 RID: 129
		// (get) Token: 0x0600023A RID: 570 RVA: 0x00014EB3 File Offset: 0x000130B3
		// (set) Token: 0x0600023B RID: 571 RVA: 0x00014EBB File Offset: 0x000130BB
		public int TechBlockId { get; set; }

		// Token: 0x17000082 RID: 130
		// (get) Token: 0x0600023C RID: 572 RVA: 0x00014EC4 File Offset: 0x000130C4
		// (set) Token: 0x0600023D RID: 573 RVA: 0x00014ECC File Offset: 0x000130CC
		public List<ContributionItemDto> Items { get; set; } = new List<ContributionItemDto>();
	}
}
