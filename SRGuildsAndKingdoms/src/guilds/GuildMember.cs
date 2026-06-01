using System;
using System.Runtime.CompilerServices;

namespace SRGuildsAndKingdoms.src.guilds
{
	// Token: 0x020000AA RID: 170
	[NullableContext(1)]
	[Nullable(0)]
	public class GuildMember
	{
		// Token: 0x17000209 RID: 521
		// (get) Token: 0x060007D4 RID: 2004 RVA: 0x000379DC File Offset: 0x00035BDC
		// (set) Token: 0x060007D5 RID: 2005 RVA: 0x000379E4 File Offset: 0x00035BE4
		public string PlayerUid { get; set; }

		// Token: 0x1700020A RID: 522
		// (get) Token: 0x060007D6 RID: 2006 RVA: 0x000379ED File Offset: 0x00035BED
		// (set) Token: 0x060007D7 RID: 2007 RVA: 0x000379F5 File Offset: 0x00035BF5
		public string Role { get; set; }

		// Token: 0x1700020B RID: 523
		// (get) Token: 0x060007D8 RID: 2008 RVA: 0x000379FE File Offset: 0x00035BFE
		// (set) Token: 0x060007D9 RID: 2009 RVA: 0x00037A06 File Offset: 0x00035C06
		public DateTime LastSeen { get; set; } = DateTime.UtcNow;

		// Token: 0x1700020C RID: 524
		// (get) Token: 0x060007DA RID: 2010 RVA: 0x00037A0F File Offset: 0x00035C0F
		// (set) Token: 0x060007DB RID: 2011 RVA: 0x00037A17 File Offset: 0x00035C17
		public int PointsContribution { get; set; }
	}
}
