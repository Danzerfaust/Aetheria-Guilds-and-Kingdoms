using System;
using System.Runtime.CompilerServices;
using ProtoBuf;

namespace SRGuildsAndKingdoms.src.network
{
	// Token: 0x02000067 RID: 103
	[NullableContext(1)]
	[Nullable(0)]
	[ProtoContract(ImplicitFields = 1)]
	public class QuestSubmittableItem
	{
		// Token: 0x17000136 RID: 310
		// (get) Token: 0x06000410 RID: 1040 RVA: 0x00018EDE File Offset: 0x000170DE
		// (set) Token: 0x06000411 RID: 1041 RVA: 0x00018EE6 File Offset: 0x000170E6
		public int ObjectiveId { get; set; }

		// Token: 0x17000137 RID: 311
		// (get) Token: 0x06000412 RID: 1042 RVA: 0x00018EEF File Offset: 0x000170EF
		// (set) Token: 0x06000413 RID: 1043 RVA: 0x00018EF7 File Offset: 0x000170F7
		public string ItemCode { get; set; } = string.Empty;

		// Token: 0x17000138 RID: 312
		// (get) Token: 0x06000414 RID: 1044 RVA: 0x00018F00 File Offset: 0x00017100
		// (set) Token: 0x06000415 RID: 1045 RVA: 0x00018F08 File Offset: 0x00017108
		public string DisplayName { get; set; } = string.Empty;

		// Token: 0x17000139 RID: 313
		// (get) Token: 0x06000416 RID: 1046 RVA: 0x00018F11 File Offset: 0x00017111
		// (set) Token: 0x06000417 RID: 1047 RVA: 0x00018F19 File Offset: 0x00017119
		public int Quantity { get; set; }
	}
}
