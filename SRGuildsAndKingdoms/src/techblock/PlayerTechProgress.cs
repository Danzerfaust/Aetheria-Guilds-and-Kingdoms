using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace SRGuildsAndKingdoms.src.techblock
{
	// Token: 0x0200000D RID: 13
	[NullableContext(1)]
	[Nullable(0)]
	public class PlayerTechProgress
	{
		// Token: 0x1700001A RID: 26
		// (get) Token: 0x060000AB RID: 171 RVA: 0x0000A483 File Offset: 0x00008683
		// (set) Token: 0x060000AC RID: 172 RVA: 0x0000A48B File Offset: 0x0000868B
		[JsonPropertyName("playerUid")]
		public string PlayerUid { get; set; }

		// Token: 0x1700001B RID: 27
		// (get) Token: 0x060000AD RID: 173 RVA: 0x0000A494 File Offset: 0x00008694
		// (set) Token: 0x060000AE RID: 174 RVA: 0x0000A49C File Offset: 0x0000869C
		[JsonPropertyName("personalUnlocks")]
		public Dictionary<int, PersonalTechUnlock> PersonalUnlocks { get; set; } = new Dictionary<int, PersonalTechUnlock>();

		// Token: 0x060000AF RID: 175 RVA: 0x0000A4A5 File Offset: 0x000086A5
		public PersonalTechUnlock GetOrCreateUnlock(int techId)
		{
			if (!this.PersonalUnlocks.ContainsKey(techId))
			{
				this.PersonalUnlocks[techId] = new PersonalTechUnlock
				{
					TechId = techId
				};
			}
			return this.PersonalUnlocks[techId];
		}

		// Token: 0x060000B0 RID: 176 RVA: 0x0000A4DC File Offset: 0x000086DC
		public bool IsPersonallyUnlocked(int techId)
		{
			PersonalTechUnlock unlock;
			return this.PersonalUnlocks.TryGetValue(techId, out unlock) && (!unlock.RequiresPersonalUnlock || unlock.IsPersonallyUnlocked);
		}
	}
}
