using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using ProtoBuf;
using SRGuildsAndKingdoms.src.techblock;

namespace SRGuildsAndKingdoms.src.guilds
{
	// Token: 0x020000A4 RID: 164
	[NullableContext(1)]
	[Nullable(0)]
	[ProtoContract(ImplicitFields = 1)]
	public class GuildSummary
	{
		// Token: 0x170001D4 RID: 468
		// (get) Token: 0x06000718 RID: 1816 RVA: 0x0003438E File Offset: 0x0003258E
		// (set) Token: 0x06000719 RID: 1817 RVA: 0x00034396 File Offset: 0x00032596
		public string Name { get; set; }

		// Token: 0x170001D5 RID: 469
		// (get) Token: 0x0600071A RID: 1818 RVA: 0x0003439F File Offset: 0x0003259F
		// (set) Token: 0x0600071B RID: 1819 RVA: 0x000343A7 File Offset: 0x000325A7
		public string Description { get; set; }

		// Token: 0x170001D6 RID: 470
		// (get) Token: 0x0600071C RID: 1820 RVA: 0x000343B0 File Offset: 0x000325B0
		// (set) Token: 0x0600071D RID: 1821 RVA: 0x000343B8 File Offset: 0x000325B8
		public int DisplayColor { get; set; }

		// Token: 0x170001D7 RID: 471
		// (get) Token: 0x0600071E RID: 1822 RVA: 0x000343C1 File Offset: 0x000325C1
		// (set) Token: 0x0600071F RID: 1823 RVA: 0x000343C9 File Offset: 0x000325C9
		public int SecondaryColor { get; set; }

		// Token: 0x170001D8 RID: 472
		// (get) Token: 0x06000720 RID: 1824 RVA: 0x000343D2 File Offset: 0x000325D2
		// (set) Token: 0x06000721 RID: 1825 RVA: 0x000343DA File Offset: 0x000325DA
		public int Points { get; set; }

		// Token: 0x170001D9 RID: 473
		// (get) Token: 0x06000722 RID: 1826 RVA: 0x000343E3 File Offset: 0x000325E3
		// (set) Token: 0x06000723 RID: 1827 RVA: 0x000343EB File Offset: 0x000325EB
		public string RankClass { get; set; } = "D";

		// Token: 0x170001DA RID: 474
		// (get) Token: 0x06000724 RID: 1828 RVA: 0x000343F4 File Offset: 0x000325F4
		// (set) Token: 0x06000725 RID: 1829 RVA: 0x000343FC File Offset: 0x000325FC
		public int MemberPointsContribution { get; set; }

		// Token: 0x170001DB RID: 475
		// (get) Token: 0x06000726 RID: 1830 RVA: 0x00034405 File Offset: 0x00032605
		// (set) Token: 0x06000727 RID: 1831 RVA: 0x0003440D File Offset: 0x0003260D
		public string MemberRank { get; set; } = "Guild Member";

		// Token: 0x170001DC RID: 476
		// (get) Token: 0x06000728 RID: 1832 RVA: 0x00034416 File Offset: 0x00032616
		// (set) Token: 0x06000729 RID: 1833 RVA: 0x0003441E File Offset: 0x0003261E
		public List<LandClaimDto> Claims { get; set; } = new List<LandClaimDto>();

		// Token: 0x170001DD RID: 477
		// (get) Token: 0x0600072A RID: 1834 RVA: 0x00034427 File Offset: 0x00032627
		// (set) Token: 0x0600072B RID: 1835 RVA: 0x0003442F File Offset: 0x0003262F
		public string PlayerRole { get; set; } = "";

		// Token: 0x170001DE RID: 478
		// (get) Token: 0x0600072C RID: 1836 RVA: 0x00034438 File Offset: 0x00032638
		// (set) Token: 0x0600072D RID: 1837 RVA: 0x00034440 File Offset: 0x00032640
		public bool IsPlayerMember { get; set; }

		// Token: 0x170001DF RID: 479
		// (get) Token: 0x0600072E RID: 1838 RVA: 0x00034449 File Offset: 0x00032649
		// (set) Token: 0x0600072F RID: 1839 RVA: 0x00034451 File Offset: 0x00032651
		public int MemberCount { get; set; }

		// Token: 0x170001E0 RID: 480
		// (get) Token: 0x06000730 RID: 1840 RVA: 0x0003445A File Offset: 0x0003265A
		// (set) Token: 0x06000731 RID: 1841 RVA: 0x00034462 File Offset: 0x00032662
		public bool HasBreakPlacePermission { get; set; }

		// Token: 0x170001E1 RID: 481
		// (get) Token: 0x06000732 RID: 1842 RVA: 0x0003446B File Offset: 0x0003266B
		// (set) Token: 0x06000733 RID: 1843 RVA: 0x00034473 File Offset: 0x00032673
		public bool HasInteractPermission { get; set; }

		// Token: 0x170001E2 RID: 482
		// (get) Token: 0x06000734 RID: 1844 RVA: 0x0003447C File Offset: 0x0003267C
		// (set) Token: 0x06000735 RID: 1845 RVA: 0x00034484 File Offset: 0x00032684
		public List<string> MemberUids { get; set; } = new List<string>();

		// Token: 0x170001E3 RID: 483
		// (get) Token: 0x06000736 RID: 1846 RVA: 0x0003448D File Offset: 0x0003268D
		// (set) Token: 0x06000737 RID: 1847 RVA: 0x00034495 File Offset: 0x00032695
		public List<string> PendingInviteUids { get; set; } = new List<string>();

		// Token: 0x170001E4 RID: 484
		// (get) Token: 0x06000738 RID: 1848 RVA: 0x0003449E File Offset: 0x0003269E
		// (set) Token: 0x06000739 RID: 1849 RVA: 0x000344A6 File Offset: 0x000326A6
		public List<GuildInviteDto> PendingInvites { get; set; } = new List<GuildInviteDto>();

		// Token: 0x170001E5 RID: 485
		// (get) Token: 0x0600073A RID: 1850 RVA: 0x000344AF File Offset: 0x000326AF
		// (set) Token: 0x0600073B RID: 1851 RVA: 0x000344B7 File Offset: 0x000326B7
		public Dictionary<string, GuildRole> Roles { get; set; } = new Dictionary<string, GuildRole>();

		// Token: 0x170001E6 RID: 486
		// (get) Token: 0x0600073C RID: 1852 RVA: 0x000344C0 File Offset: 0x000326C0
		// (set) Token: 0x0600073D RID: 1853 RVA: 0x000344C8 File Offset: 0x000326C8
		public int MaxClaims { get; set; }

		// Token: 0x170001E7 RID: 487
		// (get) Token: 0x0600073E RID: 1854 RVA: 0x000344D1 File Offset: 0x000326D1
		// (set) Token: 0x0600073F RID: 1855 RVA: 0x000344D9 File Offset: 0x000326D9
		public int MaxOutposts { get; set; }

		// Token: 0x170001E8 RID: 488
		// (get) Token: 0x06000740 RID: 1856 RVA: 0x000344E2 File Offset: 0x000326E2
		// (set) Token: 0x06000741 RID: 1857 RVA: 0x000344EA File Offset: 0x000326EA
		public Dictionary<int, GuildTechProgress> TechProgress { get; set; } = new Dictionary<int, GuildTechProgress>();

		// Token: 0x170001E9 RID: 489
		// (get) Token: 0x06000742 RID: 1858 RVA: 0x000344F3 File Offset: 0x000326F3
		// (set) Token: 0x06000743 RID: 1859 RVA: 0x000344FB File Offset: 0x000326FB
		public Dictionary<int, bool> TechRequiresPersonalUnlock { get; set; } = new Dictionary<int, bool>();

		// Token: 0x170001EA RID: 490
		// (get) Token: 0x06000744 RID: 1860 RVA: 0x00034504 File Offset: 0x00032704
		// (set) Token: 0x06000745 RID: 1861 RVA: 0x0003450C File Offset: 0x0003270C
		public Dictionary<string, PlayerTechProgress> PlayerTechProgress { get; set; } = new Dictionary<string, PlayerTechProgress>();

		// Token: 0x06000746 RID: 1862 RVA: 0x00034515 File Offset: 0x00032715
		public GuildTechProgress GetOrCreateTechProgress(int techBlockId)
		{
			if (!this.TechProgress.ContainsKey(techBlockId))
			{
				this.TechProgress[techBlockId] = new GuildTechProgress
				{
					TechBlockId = techBlockId
				};
			}
			return this.TechProgress[techBlockId];
		}

		// Token: 0x06000747 RID: 1863 RVA: 0x0003454C File Offset: 0x0003274C
		public bool IsTechUnlocked(int techBlockId)
		{
			GuildTechProgress progress;
			return this.TechProgress.TryGetValue(techBlockId, out progress) && progress.IsUnlocked;
		}
	}
}
