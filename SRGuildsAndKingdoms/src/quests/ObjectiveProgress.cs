using System;
using System.Text.Json.Serialization;

namespace SRGuildsAndKingdoms.src.quests
{
	// Token: 0x02000015 RID: 21
	public class ObjectiveProgress
	{
		// Token: 0x17000037 RID: 55
		// (get) Token: 0x0600010F RID: 271 RVA: 0x0000C8F2 File Offset: 0x0000AAF2
		// (set) Token: 0x06000110 RID: 272 RVA: 0x0000C8FA File Offset: 0x0000AAFA
		[JsonPropertyName("current")]
		public int Current { get; set; }
	}
}
