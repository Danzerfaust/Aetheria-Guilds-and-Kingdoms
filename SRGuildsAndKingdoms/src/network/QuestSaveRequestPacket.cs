using System;
using System.Runtime.CompilerServices;
using ProtoBuf;

namespace SRGuildsAndKingdoms.src.network
{
	// Token: 0x0200006B RID: 107
	[NullableContext(1)]
	[Nullable(0)]
	[ProtoContract(ImplicitFields = 1)]
	public class QuestSaveRequestPacket
	{
		// Token: 0x17000143 RID: 323
		// (get) Token: 0x0600042E RID: 1070 RVA: 0x0001901D File Offset: 0x0001721D
		// (set) Token: 0x0600042F RID: 1071 RVA: 0x00019025 File Offset: 0x00017225
		public string PlayerUid { get; set; } = string.Empty;

		// Token: 0x17000144 RID: 324
		// (get) Token: 0x06000430 RID: 1072 RVA: 0x0001902E File Offset: 0x0001722E
		// (set) Token: 0x06000431 RID: 1073 RVA: 0x00019036 File Offset: 0x00017236
		public QuestSaveDto Quest { get; set; } = new QuestSaveDto();
	}
}
