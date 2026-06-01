using System;
using System.Runtime.CompilerServices;
using ProtoBuf;

namespace SRGuildsAndKingdoms.src.network
{
	// Token: 0x02000050 RID: 80
	[NullableContext(1)]
	[Nullable(0)]
	[ProtoContract(ImplicitFields = 1)]
	public class AvailableWarDto
	{
		// Token: 0x170000DA RID: 218
		// (get) Token: 0x06000305 RID: 773 RVA: 0x00015635 File Offset: 0x00013835
		// (set) Token: 0x06000306 RID: 774 RVA: 0x0001563D File Offset: 0x0001383D
		public string NodeId { get; set; } = string.Empty;

		// Token: 0x170000DB RID: 219
		// (get) Token: 0x06000307 RID: 775 RVA: 0x00015646 File Offset: 0x00013846
		// (set) Token: 0x06000308 RID: 776 RVA: 0x0001564E File Offset: 0x0001384E
		public string NodeName { get; set; } = string.Empty;

		// Token: 0x170000DC RID: 220
		// (get) Token: 0x06000309 RID: 777 RVA: 0x00015657 File Offset: 0x00013857
		// (set) Token: 0x0600030A RID: 778 RVA: 0x0001565F File Offset: 0x0001385F
		public long WarStartTimeTicks { get; set; }

		// Token: 0x170000DD RID: 221
		// (get) Token: 0x0600030B RID: 779 RVA: 0x00015668 File Offset: 0x00013868
		// (set) Token: 0x0600030C RID: 780 RVA: 0x00015670 File Offset: 0x00013870
		public int CurrentSignups { get; set; }

		// Token: 0x170000DE RID: 222
		// (get) Token: 0x0600030D RID: 781 RVA: 0x00015679 File Offset: 0x00013879
		// (set) Token: 0x0600030E RID: 782 RVA: 0x00015681 File Offset: 0x00013881
		public int MaxGuilds { get; set; }

		// Token: 0x170000DF RID: 223
		// (get) Token: 0x0600030F RID: 783 RVA: 0x0001568A File Offset: 0x0001388A
		// (set) Token: 0x06000310 RID: 784 RVA: 0x00015692 File Offset: 0x00013892
		public bool CanSignup { get; set; }
	}
}
