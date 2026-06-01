using System;
using System.Runtime.CompilerServices;
using ProtoBuf;

namespace SRGuildsAndKingdoms.src.network
{
	// Token: 0x0200003F RID: 63
	[NullableContext(1)]
	[Nullable(0)]
	[ProtoContract(ImplicitFields = 1)]
	public class GuildMemberInfo
	{
		// Token: 0x17000096 RID: 150
		// (get) Token: 0x0600026C RID: 620 RVA: 0x00015084 File Offset: 0x00013284
		// (set) Token: 0x0600026D RID: 621 RVA: 0x0001508C File Offset: 0x0001328C
		public string PlayerUid { get; set; }

		// Token: 0x17000097 RID: 151
		// (get) Token: 0x0600026E RID: 622 RVA: 0x00015095 File Offset: 0x00013295
		// (set) Token: 0x0600026F RID: 623 RVA: 0x0001509D File Offset: 0x0001329D
		public string PlayerName { get; set; }

		// Token: 0x17000098 RID: 152
		// (get) Token: 0x06000270 RID: 624 RVA: 0x000150A6 File Offset: 0x000132A6
		// (set) Token: 0x06000271 RID: 625 RVA: 0x000150AE File Offset: 0x000132AE
		public string Role { get; set; }

		// Token: 0x17000099 RID: 153
		// (get) Token: 0x06000272 RID: 626 RVA: 0x000150B7 File Offset: 0x000132B7
		// (set) Token: 0x06000273 RID: 627 RVA: 0x000150BF File Offset: 0x000132BF
		public bool IsOnline { get; set; }

		// Token: 0x1700009A RID: 154
		// (get) Token: 0x06000274 RID: 628 RVA: 0x000150C8 File Offset: 0x000132C8
		// (set) Token: 0x06000275 RID: 629 RVA: 0x000150D0 File Offset: 0x000132D0
		public long LastSeenTicks { get; set; }
	}
}
