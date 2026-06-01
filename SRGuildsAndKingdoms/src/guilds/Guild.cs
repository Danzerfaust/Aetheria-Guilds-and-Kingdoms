using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using ProtoBuf;
using SRGuildsAndKingdoms.src.techblock;

namespace SRGuildsAndKingdoms.src.guilds
{
	// Token: 0x020000A3 RID: 163
	[NullableContext(1)]
	[Nullable(0)]
	public class Guild
	{
		// Token: 0x170001C3 RID: 451
		// (get) Token: 0x060006E7 RID: 1767 RVA: 0x00033E79 File Offset: 0x00032079
		// (set) Token: 0x060006E8 RID: 1768 RVA: 0x00033E81 File Offset: 0x00032081
		[JsonIgnore]
		[ProtoIgnore]
		public int? DatabaseId { get; set; }

		// Token: 0x170001C4 RID: 452
		// (get) Token: 0x060006E9 RID: 1769 RVA: 0x00033E8A File Offset: 0x0003208A
		// (set) Token: 0x060006EA RID: 1770 RVA: 0x00033E92 File Offset: 0x00032092
		public string Name { get; set; }

		// Token: 0x170001C5 RID: 453
		// (get) Token: 0x060006EB RID: 1771 RVA: 0x00033E9B File Offset: 0x0003209B
		// (set) Token: 0x060006EC RID: 1772 RVA: 0x00033EA3 File Offset: 0x000320A3
		public string Description { get; set; } = "";

		// Token: 0x170001C6 RID: 454
		// (get) Token: 0x060006ED RID: 1773 RVA: 0x00033EAC File Offset: 0x000320AC
		// (set) Token: 0x060006EE RID: 1774 RVA: 0x00033EB4 File Offset: 0x000320B4
		public int DisplayColor { get; set; } = -8421377;

		// Token: 0x170001C7 RID: 455
		// (get) Token: 0x060006EF RID: 1775 RVA: 0x00033EBD File Offset: 0x000320BD
		// (set) Token: 0x060006F0 RID: 1776 RVA: 0x00033EC5 File Offset: 0x000320C5
		public int SecondaryColor { get; set; } = -6316033;

		// Token: 0x170001C8 RID: 456
		// (get) Token: 0x060006F1 RID: 1777 RVA: 0x00033ECE File Offset: 0x000320CE
		// (set) Token: 0x060006F2 RID: 1778 RVA: 0x00033ED6 File Offset: 0x000320D6
		public int Points { get; set; }

		// Token: 0x170001C9 RID: 457
		// (get) Token: 0x060006F3 RID: 1779 RVA: 0x00033EDF File Offset: 0x000320DF
		// (set) Token: 0x060006F4 RID: 1780 RVA: 0x00033EE7 File Offset: 0x000320E7
		public Dictionary<string, GuildMember> Members { get; set; } = new Dictionary<string, GuildMember>();

		// Token: 0x170001CA RID: 458
		// (get) Token: 0x060006F5 RID: 1781 RVA: 0x00033EF0 File Offset: 0x000320F0
		// (set) Token: 0x060006F6 RID: 1782 RVA: 0x00033EF8 File Offset: 0x000320F8
		public List<GuildInvite> PendingInvites { get; set; } = new List<GuildInvite>();

		// Token: 0x170001CB RID: 459
		// (get) Token: 0x060006F7 RID: 1783 RVA: 0x00033F01 File Offset: 0x00032101
		// (set) Token: 0x060006F8 RID: 1784 RVA: 0x00033F09 File Offset: 0x00032109
		public Dictionary<string, GuildRole> Roles { get; set; } = new Dictionary<string, GuildRole>();

		// Token: 0x170001CC RID: 460
		// (get) Token: 0x060006F9 RID: 1785 RVA: 0x00033F12 File Offset: 0x00032112
		// (set) Token: 0x060006FA RID: 1786 RVA: 0x00033F1A File Offset: 0x0003211A
		public List<LandClaim> Claims { get; set; } = new List<LandClaim>();

		// Token: 0x170001CD RID: 461
		// (get) Token: 0x060006FB RID: 1787 RVA: 0x00033F23 File Offset: 0x00032123
		// (set) Token: 0x060006FC RID: 1788 RVA: 0x00033F2B File Offset: 0x0003212B
		public Dictionary<int, GuildTechProgress> TechProgress { get; set; } = new Dictionary<int, GuildTechProgress>();

		// Token: 0x170001CE RID: 462
		// (get) Token: 0x060006FD RID: 1789 RVA: 0x00033F34 File Offset: 0x00032134
		// (set) Token: 0x060006FE RID: 1790 RVA: 0x00033F3C File Offset: 0x0003213C
		public Dictionary<int, bool> TechRequiresPersonalUnlock { get; set; } = new Dictionary<int, bool>();

		// Token: 0x170001CF RID: 463
		// (get) Token: 0x060006FF RID: 1791 RVA: 0x00033F45 File Offset: 0x00032145
		// (set) Token: 0x06000700 RID: 1792 RVA: 0x00033F4D File Offset: 0x0003214D
		public Dictionary<string, PlayerTechProgress> PlayerTechProgress { get; set; } = new Dictionary<string, PlayerTechProgress>();

		// Token: 0x170001D0 RID: 464
		// (get) Token: 0x06000701 RID: 1793 RVA: 0x00033F56 File Offset: 0x00032156
		// (set) Token: 0x06000702 RID: 1794 RVA: 0x00033F5E File Offset: 0x0003215E
		public List<string> ControlledNodes { get; set; } = new List<string>();

		// Token: 0x170001D1 RID: 465
		// (get) Token: 0x06000703 RID: 1795 RVA: 0x00033F67 File Offset: 0x00032167
		// (set) Token: 0x06000704 RID: 1796 RVA: 0x00033F6F File Offset: 0x0003216F
		public Dictionary<string, DateTime> NodeCaptureHistory { get; set; } = new Dictionary<string, DateTime>();

		// Token: 0x170001D2 RID: 466
		// (get) Token: 0x06000705 RID: 1797 RVA: 0x00033F78 File Offset: 0x00032178
		// (set) Token: 0x06000706 RID: 1798 RVA: 0x00033F80 File Offset: 0x00032180
		[Nullable(2)]
		public string CurrentNodeWarSignup { [NullableContext(2)] get; [NullableContext(2)] set; }

		// Token: 0x170001D3 RID: 467
		// (get) Token: 0x06000707 RID: 1799 RVA: 0x00033F89 File Offset: 0x00032189
		// (set) Token: 0x06000708 RID: 1800 RVA: 0x00033F91 File Offset: 0x00032191
		public DateTime? NodeWarSignupTime { get; set; }

		// Token: 0x06000709 RID: 1801 RVA: 0x00033F9C File Offset: 0x0003219C
		public Guild()
		{
			this.Name = string.Empty;
			if (!this.Roles.ContainsKey("Leader"))
			{
				this.Roles["Leader"] = new GuildRole
				{
					Description = "Leader",
					Permissions = (GuildPermission.Invite | GuildPermission.Promote | GuildPermission.Kick | GuildPermission.ManageRoles | GuildPermission.BreakAndPlaceBlocks | GuildPermission.InteractBlocks),
					Hierarchy = 1
				};
			}
			if (!this.Roles.ContainsKey("Member"))
			{
				this.Roles["Member"] = new GuildRole
				{
					Description = "Member",
					Permissions = (GuildPermission.BreakAndPlaceBlocks | GuildPermission.InteractBlocks),
					Hierarchy = 100
				};
			}
		}

		// Token: 0x0600070A RID: 1802 RVA: 0x000340C4 File Offset: 0x000322C4
		public List<OutpostClaim> GetOutpostClaims()
		{
			List<OutpostClaim> outposts = new List<OutpostClaim>();
			foreach (LandClaim landClaim in this.Claims)
			{
				OutpostClaim outpost = landClaim as OutpostClaim;
				if (outpost != null)
				{
					outposts.Add(outpost);
				}
			}
			return outposts;
		}

		// Token: 0x0600070B RID: 1803 RVA: 0x00034128 File Offset: 0x00032328
		public List<LandClaim> GetRegularClaims()
		{
			List<LandClaim> regularClaims = new List<LandClaim>();
			foreach (LandClaim claim in this.Claims)
			{
				if (!(claim is GuildHomeClaim) && !(claim is OutpostClaim))
				{
					regularClaims.Add(claim);
				}
			}
			return regularClaims;
		}

		// Token: 0x0600070C RID: 1804 RVA: 0x00034194 File Offset: 0x00032394
		public List<GuildHomeClaim> GetGuildHomeClaims()
		{
			List<GuildHomeClaim> homeClaims = new List<GuildHomeClaim>();
			foreach (LandClaim landClaim in this.Claims)
			{
				GuildHomeClaim home = landClaim as GuildHomeClaim;
				if (home != null)
				{
					homeClaims.Add(home);
				}
			}
			return homeClaims;
		}

		// Token: 0x0600070D RID: 1805 RVA: 0x000341F8 File Offset: 0x000323F8
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

		// Token: 0x0600070E RID: 1806 RVA: 0x0003422C File Offset: 0x0003242C
		public bool IsTechUnlocked(int techBlockId)
		{
			GuildTechProgress progress;
			return this.TechProgress.TryGetValue(techBlockId, out progress) && progress.IsUnlocked;
		}

		// Token: 0x0600070F RID: 1807 RVA: 0x00034254 File Offset: 0x00032454
		public bool HasPlayerUnlockedTech(string playerUid, int techId)
		{
			bool requiresPersonal;
			PlayerTechProgress progress;
			return this.IsTechUnlocked(techId) && (!this.TechRequiresPersonalUnlock.TryGetValue(techId, out requiresPersonal) || !requiresPersonal || (this.PlayerTechProgress.TryGetValue(playerUid, out progress) && progress.IsPersonallyUnlocked(techId)));
		}

		// Token: 0x06000710 RID: 1808 RVA: 0x0003429A File Offset: 0x0003249A
		public PlayerTechProgress GetOrCreatePlayerProgress(string playerUid)
		{
			if (!this.PlayerTechProgress.ContainsKey(playerUid))
			{
				this.PlayerTechProgress[playerUid] = new PlayerTechProgress
				{
					PlayerUid = playerUid
				};
			}
			return this.PlayerTechProgress[playerUid];
		}

		// Token: 0x06000711 RID: 1809 RVA: 0x000342CE File Offset: 0x000324CE
		public void AddControlledNode(string nodeId)
		{
			if (!this.ControlledNodes.Contains(nodeId))
			{
				this.ControlledNodes.Add(nodeId);
				this.NodeCaptureHistory[nodeId] = DateTime.UtcNow;
			}
		}

		// Token: 0x06000712 RID: 1810 RVA: 0x000342FB File Offset: 0x000324FB
		public void RemoveControlledNode(string nodeId)
		{
			this.ControlledNodes.Remove(nodeId);
		}

		// Token: 0x06000713 RID: 1811 RVA: 0x0003430A File Offset: 0x0003250A
		public bool ControlsNode(string nodeId)
		{
			return this.ControlledNodes.Contains(nodeId);
		}

		// Token: 0x06000714 RID: 1812 RVA: 0x00034318 File Offset: 0x00032518
		public DateTime? GetNodeCaptureTime(string nodeId)
		{
			DateTime time;
			if (!this.NodeCaptureHistory.TryGetValue(nodeId, out time))
			{
				return null;
			}
			return new DateTime?(time);
		}

		// Token: 0x06000715 RID: 1813 RVA: 0x00034345 File Offset: 0x00032545
		public void SetNodeWarSignup(string nodeId)
		{
			this.CurrentNodeWarSignup = nodeId;
			this.NodeWarSignupTime = new DateTime?(DateTime.UtcNow);
		}

		// Token: 0x06000716 RID: 1814 RVA: 0x00034360 File Offset: 0x00032560
		public void ClearNodeWarSignup()
		{
			this.CurrentNodeWarSignup = null;
			this.NodeWarSignupTime = null;
		}

		// Token: 0x06000717 RID: 1815 RVA: 0x00034383 File Offset: 0x00032583
		public bool IsSignedUpForNodeWar()
		{
			return this.CurrentNodeWarSignup != null;
		}
	}
}
