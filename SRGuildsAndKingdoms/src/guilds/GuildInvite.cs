using System;
using System.Runtime.CompilerServices;

namespace SRGuildsAndKingdoms.src.guilds
{
	// Token: 0x020000A8 RID: 168
	[NullableContext(1)]
	[Nullable(0)]
	public class GuildInvite
	{
		// Token: 0x17000204 RID: 516
		// (get) Token: 0x0600077E RID: 1918 RVA: 0x00034AD0 File Offset: 0x00032CD0
		// (set) Token: 0x0600077F RID: 1919 RVA: 0x00034AD8 File Offset: 0x00032CD8
		public string InviterUid { get; set; }

		// Token: 0x17000205 RID: 517
		// (get) Token: 0x06000780 RID: 1920 RVA: 0x00034AE1 File Offset: 0x00032CE1
		// (set) Token: 0x06000781 RID: 1921 RVA: 0x00034AE9 File Offset: 0x00032CE9
		public string InviteeUid { get; set; }

		// Token: 0x17000206 RID: 518
		// (get) Token: 0x06000782 RID: 1922 RVA: 0x00034AF2 File Offset: 0x00032CF2
		// (set) Token: 0x06000783 RID: 1923 RVA: 0x00034AFA File Offset: 0x00032CFA
		public string GuildName { get; set; }

		// Token: 0x17000207 RID: 519
		// (get) Token: 0x06000784 RID: 1924 RVA: 0x00034B03 File Offset: 0x00032D03
		// (set) Token: 0x06000785 RID: 1925 RVA: 0x00034B0B File Offset: 0x00032D0B
		public DateTime Timestamp { get; set; }

		// Token: 0x17000208 RID: 520
		// (get) Token: 0x06000786 RID: 1926 RVA: 0x00034B14 File Offset: 0x00032D14
		// (set) Token: 0x06000787 RID: 1927 RVA: 0x00034B1C File Offset: 0x00032D1C
		public DateTime ExpiresAt { get; set; }

		// Token: 0x06000788 RID: 1928 RVA: 0x00034B25 File Offset: 0x00032D25
		public bool IsExpired()
		{
			return DateTime.UtcNow > this.ExpiresAt;
		}

		// Token: 0x06000789 RID: 1929 RVA: 0x00034B38 File Offset: 0x00032D38
		public double GetRemainingSeconds()
		{
			return (this.ExpiresAt - DateTime.UtcNow).TotalSeconds;
		}
	}
}
