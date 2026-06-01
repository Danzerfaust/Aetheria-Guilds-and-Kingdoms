using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace SRGuildsAndKingdoms.src.quests
{
	// Token: 0x0200001B RID: 27
	[NullableContext(1)]
	[Nullable(0)]
	public class QuestRewardsJson
	{
		// Token: 0x17000045 RID: 69
		// (get) Token: 0x06000134 RID: 308 RVA: 0x0000CB89 File Offset: 0x0000AD89
		// (set) Token: 0x06000135 RID: 309 RVA: 0x0000CB91 File Offset: 0x0000AD91
		[JsonPropertyName("rewards")]
		public List<QuestReward> Rewards { get; set; } = new List<QuestReward>();
	}
}
