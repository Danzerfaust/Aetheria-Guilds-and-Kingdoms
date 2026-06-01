using System;
using System.Runtime.CompilerServices;
using ProtoBuf;

namespace SRGuildsAndKingdoms.src.network
{
	// Token: 0x0200003D RID: 61
	[NullableContext(1)]
	[Nullable(0)]
	[ProtoContract(ImplicitFields = 1)]
	public class GuildInviteNotificationPacket : GuildPacketBase
	{
		// Token: 0x17000091 RID: 145
		// (get) Token: 0x06000260 RID: 608 RVA: 0x00015014 File Offset: 0x00013214
		// (set) Token: 0x06000261 RID: 609 RVA: 0x0001501C File Offset: 0x0001321C
		public string InviterName { get; set; }

		// Token: 0x17000092 RID: 146
		// (get) Token: 0x06000262 RID: 610 RVA: 0x00015025 File Offset: 0x00013225
		// (set) Token: 0x06000263 RID: 611 RVA: 0x0001502D File Offset: 0x0001322D
		public string InviterUid { get; set; }

		// Token: 0x17000093 RID: 147
		// (get) Token: 0x06000264 RID: 612 RVA: 0x00015036 File Offset: 0x00013236
		// (set) Token: 0x06000265 RID: 613 RVA: 0x0001503E File Offset: 0x0001323E
		public string GuildName { get; set; }

		// Token: 0x17000094 RID: 148
		// (get) Token: 0x06000266 RID: 614 RVA: 0x00015047 File Offset: 0x00013247
		// (set) Token: 0x06000267 RID: 615 RVA: 0x0001504F File Offset: 0x0001324F
		public long ExpiresAtTicks { get; set; }
	}
}
