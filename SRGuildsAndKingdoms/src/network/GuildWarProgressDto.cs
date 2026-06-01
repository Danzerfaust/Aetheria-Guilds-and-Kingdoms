using System;
using ProtoBuf;

namespace SRGuildsAndKingdoms.src.network
{
	// Token: 0x0200004F RID: 79
	[ProtoContract(ImplicitFields = 1)]
	public class GuildWarProgressDto
	{
		// Token: 0x170000D6 RID: 214
		// (get) Token: 0x060002FC RID: 764 RVA: 0x000155E9 File Offset: 0x000137E9
		// (set) Token: 0x060002FD RID: 765 RVA: 0x000155F1 File Offset: 0x000137F1
		public double CapturePoints { get; set; }

		// Token: 0x170000D7 RID: 215
		// (get) Token: 0x060002FE RID: 766 RVA: 0x000155FA File Offset: 0x000137FA
		// (set) Token: 0x060002FF RID: 767 RVA: 0x00015602 File Offset: 0x00013802
		public int PlayersInZone { get; set; }

		// Token: 0x170000D8 RID: 216
		// (get) Token: 0x06000300 RID: 768 RVA: 0x0001560B File Offset: 0x0001380B
		// (set) Token: 0x06000301 RID: 769 RVA: 0x00015613 File Offset: 0x00013813
		public int Kills { get; set; }

		// Token: 0x170000D9 RID: 217
		// (get) Token: 0x06000302 RID: 770 RVA: 0x0001561C File Offset: 0x0001381C
		// (set) Token: 0x06000303 RID: 771 RVA: 0x00015624 File Offset: 0x00013824
		public int Deaths { get; set; }
	}
}
