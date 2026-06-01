using System;
using System.Text.Json.Serialization;

namespace SRGuildsAndKingdoms.src.techblock
{
	// Token: 0x0200000C RID: 12
	public class PersonalTechUnlock
	{
		// Token: 0x17000017 RID: 23
		// (get) Token: 0x060000A4 RID: 164 RVA: 0x0000A448 File Offset: 0x00008648
		// (set) Token: 0x060000A5 RID: 165 RVA: 0x0000A450 File Offset: 0x00008650
		[JsonPropertyName("techId")]
		public int TechId { get; set; }

		// Token: 0x17000018 RID: 24
		// (get) Token: 0x060000A6 RID: 166 RVA: 0x0000A459 File Offset: 0x00008659
		// (set) Token: 0x060000A7 RID: 167 RVA: 0x0000A461 File Offset: 0x00008661
		[JsonPropertyName("isPersonallyUnlocked")]
		public bool IsPersonallyUnlocked { get; set; }

		// Token: 0x17000019 RID: 25
		// (get) Token: 0x060000A8 RID: 168 RVA: 0x0000A46A File Offset: 0x0000866A
		// (set) Token: 0x060000A9 RID: 169 RVA: 0x0000A472 File Offset: 0x00008672
		[JsonPropertyName("requiresPersonalUnlock")]
		public bool RequiresPersonalUnlock { get; set; }
	}
}
