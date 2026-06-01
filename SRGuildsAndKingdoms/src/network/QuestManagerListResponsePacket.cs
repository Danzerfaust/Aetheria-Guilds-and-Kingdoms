using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using ProtoBuf;

namespace SRGuildsAndKingdoms.src.network
{
	// Token: 0x02000069 RID: 105
	[NullableContext(1)]
	[Nullable(0)]
	[ProtoContract(ImplicitFields = 1)]
	public class QuestManagerListResponsePacket
	{
		// Token: 0x1700013B RID: 315
		// (get) Token: 0x0600041C RID: 1052 RVA: 0x00018F64 File Offset: 0x00017164
		// (set) Token: 0x0600041D RID: 1053 RVA: 0x00018F6C File Offset: 0x0001716C
		public bool Success { get; set; }

		// Token: 0x1700013C RID: 316
		// (get) Token: 0x0600041E RID: 1054 RVA: 0x00018F75 File Offset: 0x00017175
		// (set) Token: 0x0600041F RID: 1055 RVA: 0x00018F7D File Offset: 0x0001717D
		public string Message { get; set; } = string.Empty;

		// Token: 0x1700013D RID: 317
		// (get) Token: 0x06000420 RID: 1056 RVA: 0x00018F86 File Offset: 0x00017186
		// (set) Token: 0x06000421 RID: 1057 RVA: 0x00018F8E File Offset: 0x0001718E
		public List<QuestDto> Quests { get; set; } = new List<QuestDto>();

		// Token: 0x1700013E RID: 318
		// (get) Token: 0x06000422 RID: 1058 RVA: 0x00018F97 File Offset: 0x00017197
		// (set) Token: 0x06000423 RID: 1059 RVA: 0x00018F9F File Offset: 0x0001719F
		[Nullable(2)]
		public CurrencyDefinitionDto TailsDefinition { [NullableContext(2)] get; [NullableContext(2)] set; }

		// Token: 0x1700013F RID: 319
		// (get) Token: 0x06000424 RID: 1060 RVA: 0x00018FA8 File Offset: 0x000171A8
		// (set) Token: 0x06000425 RID: 1061 RVA: 0x00018FB0 File Offset: 0x000171B0
		[Nullable(2)]
		public CurrencyDefinitionDto CrownsDefinition { [NullableContext(2)] get; [NullableContext(2)] set; }

		// Token: 0x17000140 RID: 320
		// (get) Token: 0x06000426 RID: 1062 RVA: 0x00018FB9 File Offset: 0x000171B9
		// (set) Token: 0x06000427 RID: 1063 RVA: 0x00018FC1 File Offset: 0x000171C1
		public long ServerLocalTime { get; set; }

		// Token: 0x17000141 RID: 321
		// (get) Token: 0x06000428 RID: 1064 RVA: 0x00018FCA File Offset: 0x000171CA
		// (set) Token: 0x06000429 RID: 1065 RVA: 0x00018FD2 File Offset: 0x000171D2
		public double ServerTimezoneOffset { get; set; }
	}
}
