using System;
using System.Runtime.CompilerServices;
using ProtoBuf;

namespace SRGuildsAndKingdoms.src.guilds
{
	// Token: 0x020000A6 RID: 166
	[NullableContext(1)]
	[Nullable(0)]
	[ProtoContract(ImplicitFields = 1)]
	public class GuildInviteDto
	{
		// Token: 0x170001F4 RID: 500
		// (get) Token: 0x0600075C RID: 1884 RVA: 0x000346AC File Offset: 0x000328AC
		// (set) Token: 0x0600075D RID: 1885 RVA: 0x000346B4 File Offset: 0x000328B4
		public string InviterUid { get; set; }

		// Token: 0x170001F5 RID: 501
		// (get) Token: 0x0600075E RID: 1886 RVA: 0x000346BD File Offset: 0x000328BD
		// (set) Token: 0x0600075F RID: 1887 RVA: 0x000346C5 File Offset: 0x000328C5
		public string InviteeUid { get; set; }

		// Token: 0x170001F6 RID: 502
		// (get) Token: 0x06000760 RID: 1888 RVA: 0x000346CE File Offset: 0x000328CE
		// (set) Token: 0x06000761 RID: 1889 RVA: 0x000346D6 File Offset: 0x000328D6
		public DateTime ExpiresAt { get; set; }
	}
}
