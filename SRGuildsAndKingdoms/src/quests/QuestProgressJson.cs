using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace SRGuildsAndKingdoms.src.quests
{
	// Token: 0x02000016 RID: 22
	[NullableContext(1)]
	[Nullable(0)]
	public class QuestProgressJson
	{
		// Token: 0x17000038 RID: 56
		// (get) Token: 0x06000112 RID: 274 RVA: 0x0000C90B File Offset: 0x0000AB0B
		// (set) Token: 0x06000113 RID: 275 RVA: 0x0000C913 File Offset: 0x0000AB13
		[JsonPropertyName("objectives")]
		public Dictionary<int, ObjectiveProgress> Objectives { get; set; } = new Dictionary<int, ObjectiveProgress>();
	}
}
