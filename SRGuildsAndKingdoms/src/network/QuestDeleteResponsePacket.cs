using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using ProtoBuf;

namespace SRGuildsAndKingdoms.src.network
{
	// Token: 0x0200006E RID: 110
	[NullableContext(1)]
	[Nullable(0)]
	[ProtoContract(ImplicitFields = 1)]
	public class QuestDeleteResponsePacket
	{
		// Token: 0x1700014A RID: 330
		// (get) Token: 0x0600043F RID: 1087 RVA: 0x000190E3 File Offset: 0x000172E3
		// (set) Token: 0x06000440 RID: 1088 RVA: 0x000190EB File Offset: 0x000172EB
		public bool Success { get; set; }

		// Token: 0x1700014B RID: 331
		// (get) Token: 0x06000441 RID: 1089 RVA: 0x000190F4 File Offset: 0x000172F4
		// (set) Token: 0x06000442 RID: 1090 RVA: 0x000190FC File Offset: 0x000172FC
		public string Message { get; set; } = string.Empty;

		// Token: 0x1700014C RID: 332
		// (get) Token: 0x06000443 RID: 1091 RVA: 0x00019105 File Offset: 0x00017305
		// (set) Token: 0x06000444 RID: 1092 RVA: 0x0001910D File Offset: 0x0001730D
		public List<QuestDto> AllQuests { get; set; } = new List<QuestDto>();
	}
}
