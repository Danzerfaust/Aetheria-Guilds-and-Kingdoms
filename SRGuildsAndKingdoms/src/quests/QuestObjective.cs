using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using SRGuildsAndKingdoms.src.network;

namespace SRGuildsAndKingdoms.src.quests
{
	// Token: 0x0200001D RID: 29
	[NullableContext(1)]
	[Nullable(0)]
	public class QuestObjective
	{
		// Token: 0x1700005A RID: 90
		// (get) Token: 0x06000178 RID: 376 RVA: 0x0000F3E5 File Offset: 0x0000D5E5
		// (set) Token: 0x06000179 RID: 377 RVA: 0x0000F3ED File Offset: 0x0000D5ED
		[JsonPropertyName("id")]
		public int Id { get; set; }

		// Token: 0x1700005B RID: 91
		// (get) Token: 0x0600017A RID: 378 RVA: 0x0000F3F6 File Offset: 0x0000D5F6
		// (set) Token: 0x0600017B RID: 379 RVA: 0x0000F3FE File Offset: 0x0000D5FE
		[JsonPropertyName("type")]
		public string Type { get; set; } = string.Empty;

		// Token: 0x1700005C RID: 92
		// (get) Token: 0x0600017C RID: 380 RVA: 0x0000F407 File Offset: 0x0000D607
		// (set) Token: 0x0600017D RID: 381 RVA: 0x0000F40F File Offset: 0x0000D60F
		[JsonPropertyName("count")]
		public int Count { get; set; }

		// Token: 0x1700005D RID: 93
		// (get) Token: 0x0600017E RID: 382 RVA: 0x0000F418 File Offset: 0x0000D618
		// (set) Token: 0x0600017F RID: 383 RVA: 0x0000F420 File Offset: 0x0000D620
		[Nullable(new byte[]
		{
			2,
			1
		})]
		[JsonPropertyName("acceptedTargets")]
		public List<string> AcceptedTargets { [return: Nullable(new byte[]
		{
			2,
			1
		})] get; [param: Nullable(new byte[]
		{
			2,
			1
		})] set; }

		// Token: 0x1700005E RID: 94
		// (get) Token: 0x06000180 RID: 384 RVA: 0x0000F429 File Offset: 0x0000D629
		// (set) Token: 0x06000181 RID: 385 RVA: 0x0000F431 File Offset: 0x0000D631
		[Nullable(new byte[]
		{
			2,
			1
		})]
		[JsonPropertyName("acceptedItems")]
		public List<QuestAcceptedItemDto> AcceptedItems { [return: Nullable(new byte[]
		{
			2,
			1
		})] get; [param: Nullable(new byte[]
		{
			2,
			1
		})] set; }
	}
}
