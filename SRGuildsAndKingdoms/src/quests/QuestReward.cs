using System;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace SRGuildsAndKingdoms.src.quests
{
	// Token: 0x02000020 RID: 32
	[NullableContext(1)]
	[Nullable(0)]
	public class QuestReward
	{
		// Token: 0x1700005F RID: 95
		// (get) Token: 0x06000190 RID: 400 RVA: 0x0000FA8F File Offset: 0x0000DC8F
		// (set) Token: 0x06000191 RID: 401 RVA: 0x0000FA97 File Offset: 0x0000DC97
		[JsonPropertyName("code")]
		public string Code { get; set; } = string.Empty;

		// Token: 0x17000060 RID: 96
		// (get) Token: 0x06000192 RID: 402 RVA: 0x0000FAA0 File Offset: 0x0000DCA0
		// (set) Token: 0x06000193 RID: 403 RVA: 0x0000FAA8 File Offset: 0x0000DCA8
		[Nullable(2)]
		[JsonPropertyName("nbt")]
		public string Nbt { [NullableContext(2)] get; [NullableContext(2)] set; }

		// Token: 0x17000061 RID: 97
		// (get) Token: 0x06000194 RID: 404 RVA: 0x0000FAB1 File Offset: 0x0000DCB1
		// (set) Token: 0x06000195 RID: 405 RVA: 0x0000FAB9 File Offset: 0x0000DCB9
		[JsonPropertyName("amount")]
		public int Amount { get; set; }
	}
}
