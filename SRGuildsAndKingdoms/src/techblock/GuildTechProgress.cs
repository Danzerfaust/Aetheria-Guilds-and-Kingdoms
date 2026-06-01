using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using ProtoBuf;

namespace SRGuildsAndKingdoms.src.techblock
{
	// Token: 0x0200000A RID: 10
	[NullableContext(1)]
	[Nullable(0)]
	[ProtoContract(ImplicitFields = 1)]
	public class GuildTechProgress
	{
		// Token: 0x17000010 RID: 16
		// (get) Token: 0x0600008C RID: 140 RVA: 0x0000A272 File Offset: 0x00008472
		// (set) Token: 0x0600008D RID: 141 RVA: 0x0000A27A File Offset: 0x0000847A
		public int TechBlockId { get; set; }

		// Token: 0x17000011 RID: 17
		// (get) Token: 0x0600008E RID: 142 RVA: 0x0000A283 File Offset: 0x00008483
		// (set) Token: 0x0600008F RID: 143 RVA: 0x0000A28B File Offset: 0x0000848B
		public bool IsUnlocked { get; set; }

		// Token: 0x17000012 RID: 18
		// (get) Token: 0x06000090 RID: 144 RVA: 0x0000A294 File Offset: 0x00008494
		// (set) Token: 0x06000091 RID: 145 RVA: 0x0000A29C File Offset: 0x0000849C
		public Dictionary<string, int> ResourcesSubmitted { get; set; } = new Dictionary<string, int>();

		// Token: 0x17000013 RID: 19
		// (get) Token: 0x06000092 RID: 146 RVA: 0x0000A2A5 File Offset: 0x000084A5
		// (set) Token: 0x06000093 RID: 147 RVA: 0x0000A2AD File Offset: 0x000084AD
		[JsonPropertyName("resourceGroupsSubmitted")]
		public Dictionary<string, int> ResourceGroupsSubmitted { get; set; } = new Dictionary<string, int>();

		// Token: 0x17000014 RID: 20
		// (get) Token: 0x06000094 RID: 148 RVA: 0x0000A2B6 File Offset: 0x000084B6
		// (set) Token: 0x06000095 RID: 149 RVA: 0x0000A2BE File Offset: 0x000084BE
		public long? UnlockedTimestamp { get; set; }

		// Token: 0x06000096 RID: 150 RVA: 0x0000A2C7 File Offset: 0x000084C7
		public int GetResourceGroupSubmitted(string groupName)
		{
			return this.ResourceGroupsSubmitted.GetValueOrDefault(groupName, 0);
		}
	}
}
