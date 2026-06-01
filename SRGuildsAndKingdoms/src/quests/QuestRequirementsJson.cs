using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace SRGuildsAndKingdoms.src.quests
{
	// Token: 0x0200001A RID: 26
	[NullableContext(1)]
	[Nullable(0)]
	public class QuestRequirementsJson
	{
		// Token: 0x17000044 RID: 68
		// (get) Token: 0x06000131 RID: 305 RVA: 0x0000CB65 File Offset: 0x0000AD65
		// (set) Token: 0x06000132 RID: 306 RVA: 0x0000CB6D File Offset: 0x0000AD6D
		[JsonPropertyName("objectives")]
		public List<QuestObjective> Objectives { get; set; } = new List<QuestObjective>();
	}
}
