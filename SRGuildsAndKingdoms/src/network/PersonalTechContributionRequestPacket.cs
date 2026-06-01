using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using ProtoBuf;

namespace SRGuildsAndKingdoms.src.network
{
	// Token: 0x02000043 RID: 67
	[NullableContext(1)]
	[Nullable(0)]
	[ProtoContract(ImplicitFields = 1)]
	public class PersonalTechContributionRequestPacket : GuildPacketBase
	{
		// Token: 0x170000A2 RID: 162
		// (get) Token: 0x06000288 RID: 648 RVA: 0x00015186 File Offset: 0x00013386
		// (set) Token: 0x06000289 RID: 649 RVA: 0x0001518E File Offset: 0x0001338E
		public string GuildName { get; set; }

		// Token: 0x170000A3 RID: 163
		// (get) Token: 0x0600028A RID: 650 RVA: 0x00015197 File Offset: 0x00013397
		// (set) Token: 0x0600028B RID: 651 RVA: 0x0001519F File Offset: 0x0001339F
		public int TechBlockId { get; set; }

		// Token: 0x170000A4 RID: 164
		// (get) Token: 0x0600028C RID: 652 RVA: 0x000151A8 File Offset: 0x000133A8
		// (set) Token: 0x0600028D RID: 653 RVA: 0x000151B0 File Offset: 0x000133B0
		public List<ContributionItemDto> Items { get; set; } = new List<ContributionItemDto>();
	}
}
