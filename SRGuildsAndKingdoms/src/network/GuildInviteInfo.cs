using System;
using System.Runtime.CompilerServices;
using ProtoBuf;

namespace SRGuildsAndKingdoms.src.network
{
	// Token: 0x02000031 RID: 49
	[NullableContext(1)]
	[Nullable(0)]
	[ProtoContract(ImplicitFields = 1)]
	public class GuildInviteInfo
	{
		// Token: 0x17000070 RID: 112
		// (get) Token: 0x06000212 RID: 530 RVA: 0x00014D4C File Offset: 0x00012F4C
		// (set) Token: 0x06000213 RID: 531 RVA: 0x00014D54 File Offset: 0x00012F54
		public string GuildName { get; set; }

		// Token: 0x17000071 RID: 113
		// (get) Token: 0x06000214 RID: 532 RVA: 0x00014D5D File Offset: 0x00012F5D
		// (set) Token: 0x06000215 RID: 533 RVA: 0x00014D65 File Offset: 0x00012F65
		public string InviterName { get; set; }

		// Token: 0x17000072 RID: 114
		// (get) Token: 0x06000216 RID: 534 RVA: 0x00014D6E File Offset: 0x00012F6E
		// (set) Token: 0x06000217 RID: 535 RVA: 0x00014D76 File Offset: 0x00012F76
		public string InviterUid { get; set; }

		// Token: 0x17000073 RID: 115
		// (get) Token: 0x06000218 RID: 536 RVA: 0x00014D7F File Offset: 0x00012F7F
		// (set) Token: 0x06000219 RID: 537 RVA: 0x00014D87 File Offset: 0x00012F87
		public long ExpiresAtTicks { get; set; }
	}
}
