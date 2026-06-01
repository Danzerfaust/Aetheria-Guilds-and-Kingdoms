using System;
using System.Runtime.CompilerServices;
using ProtoBuf;

namespace SRGuildsAndKingdoms.src.network
{
	// Token: 0x0200004E RID: 78
	[NullableContext(1)]
	[Nullable(0)]
	[ProtoContract(ImplicitFields = 1)]
	public class CurrentWarDto
	{
		// Token: 0x170000D1 RID: 209
		// (get) Token: 0x060002F1 RID: 753 RVA: 0x0001556B File Offset: 0x0001376B
		// (set) Token: 0x060002F2 RID: 754 RVA: 0x00015573 File Offset: 0x00013773
		public string NodeId { get; set; } = string.Empty;

		// Token: 0x170000D2 RID: 210
		// (get) Token: 0x060002F3 RID: 755 RVA: 0x0001557C File Offset: 0x0001377C
		// (set) Token: 0x060002F4 RID: 756 RVA: 0x00015584 File Offset: 0x00013784
		public string NodeName { get; set; } = string.Empty;

		// Token: 0x170000D3 RID: 211
		// (get) Token: 0x060002F5 RID: 757 RVA: 0x0001558D File Offset: 0x0001378D
		// (set) Token: 0x060002F6 RID: 758 RVA: 0x00015595 File Offset: 0x00013795
		public string Status { get; set; } = string.Empty;

		// Token: 0x170000D4 RID: 212
		// (get) Token: 0x060002F7 RID: 759 RVA: 0x0001559E File Offset: 0x0001379E
		// (set) Token: 0x060002F8 RID: 760 RVA: 0x000155A6 File Offset: 0x000137A6
		public double PointsNeeded { get; set; }

		// Token: 0x170000D5 RID: 213
		// (get) Token: 0x060002F9 RID: 761 RVA: 0x000155AF File Offset: 0x000137AF
		// (set) Token: 0x060002FA RID: 762 RVA: 0x000155B7 File Offset: 0x000137B7
		[Nullable(2)]
		public GuildWarProgressDto YourGuildProgress { [NullableContext(2)] get; [NullableContext(2)] set; }
	}
}
