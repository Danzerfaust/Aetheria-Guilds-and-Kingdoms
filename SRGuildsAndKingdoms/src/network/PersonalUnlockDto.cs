using System;
using ProtoBuf;

namespace SRGuildsAndKingdoms.src.network
{
	// Token: 0x02000046 RID: 70
	[ProtoContract(ImplicitFields = 1)]
	public class PersonalUnlockDto
	{
		// Token: 0x170000AB RID: 171
		// (get) Token: 0x0600029D RID: 669 RVA: 0x0001524D File Offset: 0x0001344D
		// (set) Token: 0x0600029E RID: 670 RVA: 0x00015255 File Offset: 0x00013455
		public int TechId { get; set; }

		// Token: 0x170000AC RID: 172
		// (get) Token: 0x0600029F RID: 671 RVA: 0x0001525E File Offset: 0x0001345E
		// (set) Token: 0x060002A0 RID: 672 RVA: 0x00015266 File Offset: 0x00013466
		public bool IsPersonallyUnlocked { get; set; }

		// Token: 0x170000AD RID: 173
		// (get) Token: 0x060002A1 RID: 673 RVA: 0x0001526F File Offset: 0x0001346F
		// (set) Token: 0x060002A2 RID: 674 RVA: 0x00015277 File Offset: 0x00013477
		public bool RequiresPersonalUnlock { get; set; }
	}
}
