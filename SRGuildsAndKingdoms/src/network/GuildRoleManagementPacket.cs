using System;
using System.Runtime.CompilerServices;
using ProtoBuf;

namespace SRGuildsAndKingdoms.src.network
{
	// Token: 0x02000035 RID: 53
	[NullableContext(1)]
	[Nullable(0)]
	[ProtoContract(ImplicitFields = 1)]
	public class GuildRoleManagementPacket : GuildPacketBase
	{
		// Token: 0x1700007B RID: 123
		// (get) Token: 0x0600022C RID: 556 RVA: 0x00014E32 File Offset: 0x00013032
		// (set) Token: 0x0600022D RID: 557 RVA: 0x00014E3A File Offset: 0x0001303A
		public string Action { get; set; }

		// Token: 0x1700007C RID: 124
		// (get) Token: 0x0600022E RID: 558 RVA: 0x00014E43 File Offset: 0x00013043
		// (set) Token: 0x0600022F RID: 559 RVA: 0x00014E4B File Offset: 0x0001304B
		public string RoleName { get; set; }

		// Token: 0x1700007D RID: 125
		// (get) Token: 0x06000230 RID: 560 RVA: 0x00014E54 File Offset: 0x00013054
		// (set) Token: 0x06000231 RID: 561 RVA: 0x00014E5C File Offset: 0x0001305C
		public string TargetPlayerName { get; set; }

		// Token: 0x1700007E RID: 126
		// (get) Token: 0x06000232 RID: 562 RVA: 0x00014E65 File Offset: 0x00013065
		// (set) Token: 0x06000233 RID: 563 RVA: 0x00014E6D File Offset: 0x0001306D
		public string PermissionString { get; set; }

		// Token: 0x1700007F RID: 127
		// (get) Token: 0x06000234 RID: 564 RVA: 0x00014E76 File Offset: 0x00013076
		// (set) Token: 0x06000235 RID: 565 RVA: 0x00014E7E File Offset: 0x0001307E
		public int Hierarchy { get; set; } = 999;
	}
}
