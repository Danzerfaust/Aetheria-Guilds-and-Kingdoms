using System;
using System.Runtime.CompilerServices;
using ProtoBuf;

namespace SRGuildsAndKingdoms.src.network
{
	// Token: 0x02000038 RID: 56
	[NullableContext(1)]
	[Nullable(0)]
	[ProtoContract(ImplicitFields = 1)]
	public class ContributionItemDto
	{
		// Token: 0x17000083 RID: 131
		// (get) Token: 0x0600023F RID: 575 RVA: 0x00014EE8 File Offset: 0x000130E8
		// (set) Token: 0x06000240 RID: 576 RVA: 0x00014EF0 File Offset: 0x000130F0
		public string ResourceGroupName { get; set; }

		// Token: 0x17000084 RID: 132
		// (get) Token: 0x06000241 RID: 577 RVA: 0x00014EF9 File Offset: 0x000130F9
		// (set) Token: 0x06000242 RID: 578 RVA: 0x00014F01 File Offset: 0x00013101
		public string InventoryId { get; set; }

		// Token: 0x17000085 RID: 133
		// (get) Token: 0x06000243 RID: 579 RVA: 0x00014F0A File Offset: 0x0001310A
		// (set) Token: 0x06000244 RID: 580 RVA: 0x00014F12 File Offset: 0x00013112
		public int SlotId { get; set; }

		// Token: 0x17000086 RID: 134
		// (get) Token: 0x06000245 RID: 581 RVA: 0x00014F1B File Offset: 0x0001311B
		// (set) Token: 0x06000246 RID: 582 RVA: 0x00014F23 File Offset: 0x00013123
		public int Amount { get; set; }

		// Token: 0x17000087 RID: 135
		// (get) Token: 0x06000247 RID: 583 RVA: 0x00014F2C File Offset: 0x0001312C
		// (set) Token: 0x06000248 RID: 584 RVA: 0x00014F34 File Offset: 0x00013134
		public string ItemCode { get; set; }
	}
}
