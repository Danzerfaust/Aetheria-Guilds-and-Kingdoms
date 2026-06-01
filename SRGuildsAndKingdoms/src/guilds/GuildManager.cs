using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using SRGuildsAndKingdoms.src.config;
using SRGuildsAndKingdoms.src.database;
using SRGuildsAndKingdoms.src.player;
using SRGuildsAndKingdoms.src.techblock;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;

namespace SRGuildsAndKingdoms.src.guilds
{
	// Token: 0x020000A9 RID: 169
	[NullableContext(1)]
	[Nullable(0)]
	public class GuildManager
	{
		// Token: 0x0600078B RID: 1931 RVA: 0x00034B65 File Offset: 0x00032D65
		public int GetMaxClaimsPerGuild(Guild guild)
		{
			if (guild == null)
			{
				return 0;
			}
			return this.configManager.GetMaxClaimsPerGuild(guild.Members.Count);
		}

		// Token: 0x0600078C RID: 1932 RVA: 0x00034B82 File Offset: 0x00032D82
		public int GetMaxOutpostsPerGuild(Guild guild)
		{
			if (guild == null)
			{
				return 0;
			}
			return this.configManager.GetMaxOutpostsPerGuild(guild.Members.Count);
		}

		// Token: 0x0600078D RID: 1933 RVA: 0x00034B9F File Offset: 0x00032D9F
		public int GetMaxMembersPerGuild()
		{
			return this.configManager.GetConfig().MaxMembersPerGuild;
		}

		// Token: 0x0600078E RID: 1934 RVA: 0x00034BB1 File Offset: 0x00032DB1
		public int GetNonOutpostClaimCount(Guild guild)
		{
			if (guild == null)
			{
				return 0;
			}
			return guild.Claims.Count((LandClaim c) => !(c is OutpostClaim));
		}

		// Token: 0x0600078F RID: 1935 RVA: 0x00034BE2 File Offset: 0x00032DE2
		public int GetOutpostClaimCount(Guild guild)
		{
			if (guild == null)
			{
				return 0;
			}
			return guild.Claims.Count((LandClaim c) => c is OutpostClaim);
		}

		// Token: 0x14000004 RID: 4
		// (add) Token: 0x06000790 RID: 1936 RVA: 0x00034C14 File Offset: 0x00032E14
		// (remove) Token: 0x06000791 RID: 1937 RVA: 0x00034C4C File Offset: 0x00032E4C
		[Nullable(2)]
		[method: NullableContext(2)]
		[Nullable(2)]
		public event Action OnGuildsChanged;

		// Token: 0x14000005 RID: 5
		// (add) Token: 0x06000792 RID: 1938 RVA: 0x00034C84 File Offset: 0x00032E84
		// (remove) Token: 0x06000793 RID: 1939 RVA: 0x00034CBC File Offset: 0x00032EBC
		[Nullable(new byte[]
		{
			2,
			1,
			1,
			1
		})]
		[Nullable(new byte[]
		{
			2,
			1,
			1,
			1
		})]
		public event Action<string, string, string> OnNodeCaptured;

		// Token: 0x14000006 RID: 6
		// (add) Token: 0x06000794 RID: 1940 RVA: 0x00034CF4 File Offset: 0x00032EF4
		// (remove) Token: 0x06000795 RID: 1941 RVA: 0x00034D2C File Offset: 0x00032F2C
		[Nullable(new byte[]
		{
			2,
			1,
			1,
			1
		})]
		[Nullable(new byte[]
		{
			2,
			1,
			1,
			1
		})]
		public event Action<string, string, string> OnNodeLost;

		// Token: 0x14000007 RID: 7
		// (add) Token: 0x06000796 RID: 1942 RVA: 0x00034D64 File Offset: 0x00032F64
		// (remove) Token: 0x06000797 RID: 1943 RVA: 0x00034D9C File Offset: 0x00032F9C
		[Nullable(new byte[]
		{
			2,
			1,
			1
		})]
		[Nullable(new byte[]
		{
			2,
			1,
			1
		})]
		public event Action<string, string> OnGuildSignedUpForWar;

		// Token: 0x14000008 RID: 8
		// (add) Token: 0x06000798 RID: 1944 RVA: 0x00034DD4 File Offset: 0x00032FD4
		// (remove) Token: 0x06000799 RID: 1945 RVA: 0x00034E0C File Offset: 0x0003300C
		[Nullable(new byte[]
		{
			2,
			1,
			1
		})]
		[Nullable(new byte[]
		{
			2,
			1,
			1
		})]
		public event Action<string, string> OnGuildCancelledWarSignup;

		// Token: 0x0600079A RID: 1946 RVA: 0x00034E44 File Offset: 0x00033044
		public GuildManager(ICoreServerAPI sapi, GuildRepository repository, CooldownRepository cooldownRepository, [Nullable(2)] LandClaimRepository landClaimRepository = null)
		{
			GuildManager <>4__this = this;
			this.sapi = sapi;
			this.repository = repository;
			this.cooldownRepository = cooldownRepository;
			this.landClaimRepository = landClaimRepository;
			this.configManager = new GuildConfigManager(sapi);
			sapi.Event.SaveGameLoaded += delegate()
			{
				SRGuildsAndKingdomsModSystem modSystem = sapi.ModLoader.GetModSystem<SRGuildsAndKingdomsModSystem>(true);
				if (modSystem != null)
				{
					<>4__this.traitManager = new GuildTraitManager(sapi, modSystem);
					sapi.Logger.Notification("[GuildManager] Trait manager initialized successfully");
					return;
				}
				sapi.Logger.Warning("[GuildManager] Could not initialize trait manager - mod system not found");
			};
		}

		// Token: 0x0600079B RID: 1947 RVA: 0x00034EBC File Offset: 0x000330BC
		public void OnSaveGameLoading()
		{
			this.configManager.LoadConfig();
			this.sapi.Logger.Notification(this.configManager.GetConfigStatus(1));
			this.LogScalingExamples();
			ILogger logger = this.sapi.Logger;
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(43, 1);
			defaultInterpolatedStringHandler.AppendLiteral("[GuildManager] Loaded ");
			defaultInterpolatedStringHandler.AppendFormatted<int>(this.repository.GetAllGuilds().Count);
			defaultInterpolatedStringHandler.AppendLiteral(" guilds from database");
			logger.Notification(defaultInterpolatedStringHandler.ToStringAndClear());
		}

		// Token: 0x0600079C RID: 1948 RVA: 0x00034F46 File Offset: 0x00033146
		public void OnSaveGameSaving()
		{
			this.repository.CommitChanges();
		}

		// Token: 0x0600079D RID: 1949 RVA: 0x00034F53 File Offset: 0x00033153
		public GuildConfigManager GetConfigManager()
		{
			return this.configManager;
		}

		// Token: 0x0600079E RID: 1950 RVA: 0x00034F5C File Offset: 0x0003315C
		public bool CreateGuild(string name, string creatorUid, string description = "")
		{
			if (this.repository.GetGuild(name) != null)
			{
				return false;
			}
			if (this.GetGuildByMember(creatorUid) != null)
			{
				return false;
			}
			Guild guild = new Guild
			{
				Name = name
			};
			ValueTuple<int, int> colors = GuildManager.GenerateRandomColors();
			guild.DisplayColor = colors.Item1;
			guild.SecondaryColor = colors.Item2;
			guild.Description = (string.IsNullOrWhiteSpace(description) ? ("Guild " + name) : description);
			guild.Members[creatorUid] = new GuildMember
			{
				PlayerUid = creatorUid,
				Role = "Leader"
			};
			this.repository.CreateGuild(guild);
			Action onGuildsChanged = this.OnGuildsChanged;
			if (onGuildsChanged != null)
			{
				onGuildsChanged();
			}
			return true;
		}

		// Token: 0x0600079F RID: 1951 RVA: 0x0003500C File Offset: 0x0003320C
		[NullableContext(0)]
		[return: TupleElementNames(new string[]
		{
			"primary",
			"secondary"
		})]
		private static ValueTuple<int, int> GenerateRandomColors()
		{
			Random random = new Random();
			byte r = (byte)random.Next(80, 255);
			byte g = (byte)random.Next(80, 255);
			byte b = (byte)random.Next(80, 255);
			int primary = -16777216 | (int)r << 16 | (int)g << 8 | (int)b;
			byte r2;
			byte g2;
			byte b2;
			if (random.NextDouble() > 0.5)
			{
				r2 = (byte)Math.Min(255, (int)(r + 40));
				g2 = (byte)Math.Min(255, (int)(g + 40));
				b2 = (byte)Math.Min(255, (int)(b + 40));
			}
			else
			{
				r2 = (byte)Math.Max(60, (int)(r - 40));
				g2 = (byte)Math.Max(60, (int)(g - 40));
				b2 = (byte)Math.Max(60, (int)(b - 40));
			}
			int secondary = -16777216 | (int)r2 << 16 | (int)g2 << 8 | (int)b2;
			return new ValueTuple<int, int>(primary, secondary);
		}

		// Token: 0x060007A0 RID: 1952 RVA: 0x000350E8 File Offset: 0x000332E8
		[return: Nullable(2)]
		public Guild GetGuildByMember(string playerUid)
		{
			return this.repository.GetGuildByMember(playerUid);
		}

		// Token: 0x060007A1 RID: 1953 RVA: 0x000350F6 File Offset: 0x000332F6
		[return: Nullable(2)]
		public Guild GetGuild(string name)
		{
			return this.repository.GetGuild(name);
		}

		// Token: 0x060007A2 RID: 1954 RVA: 0x00035104 File Offset: 0x00033304
		public static bool HasPermission(Guild guild, string playerUid, GuildPermission permission)
		{
			if (guild == null || !guild.Members.ContainsKey(playerUid))
			{
				return false;
			}
			string roleName = guild.Members[playerUid].Role;
			GuildRole role;
			return guild.Roles.TryGetValue(roleName, out role) && (role.Permissions & permission) == permission;
		}

		// Token: 0x060007A3 RID: 1955 RVA: 0x00035154 File Offset: 0x00033354
		public static int GetPlayerHierarchy(Guild guild, string playerUid)
		{
			if (guild == null || !guild.Members.ContainsKey(playerUid))
			{
				return int.MaxValue;
			}
			string roleName = guild.Members[playerUid].Role;
			GuildRole role;
			if (!guild.Roles.TryGetValue(roleName, out role))
			{
				return int.MaxValue;
			}
			return role.Hierarchy;
		}

		// Token: 0x060007A4 RID: 1956 RVA: 0x000351A8 File Offset: 0x000333A8
		public static int GetRoleHierarchy(Guild guild, string roleName)
		{
			GuildRole role;
			if (guild == null || !guild.Roles.TryGetValue(roleName, out role))
			{
				return int.MaxValue;
			}
			return role.Hierarchy;
		}

		// Token: 0x060007A5 RID: 1957 RVA: 0x000351D4 File Offset: 0x000333D4
		public bool CanActOnPlayer(Guild guild, string actorUid, string targetUid)
		{
			int playerHierarchy = GuildManager.GetPlayerHierarchy(guild, actorUid);
			int targetHierarchy = GuildManager.GetPlayerHierarchy(guild, targetUid);
			return playerHierarchy < targetHierarchy;
		}

		// Token: 0x060007A6 RID: 1958 RVA: 0x000351F4 File Offset: 0x000333F4
		public bool CanManageRole(Guild guild, string playerUid, string targetRoleName)
		{
			int playerHierarchy = GuildManager.GetPlayerHierarchy(guild, playerUid);
			int roleHierarchy = GuildManager.GetRoleHierarchy(guild, targetRoleName);
			return playerHierarchy < roleHierarchy;
		}

		// Token: 0x060007A7 RID: 1959 RVA: 0x00035214 File Offset: 0x00033414
		public bool InviteToGuild(string guildName, string inviterUid, string inviteeUid)
		{
			Guild guild = this.GetGuild(guildName);
			if (guild == null)
			{
				return false;
			}
			if (!GuildManager.HasPermission(guild, inviterUid, GuildPermission.Invite))
			{
				return false;
			}
			if (guild.Members.ContainsKey(inviteeUid))
			{
				return false;
			}
			if (guild.PendingInvites.Any((GuildInvite i) => i.InviteeUid == inviteeUid))
			{
				return false;
			}
			int maxMembers = this.configManager.GetConfig().MaxMembersPerGuild;
			if (guild.Members.Count >= maxMembers)
			{
				ILogger logger = this.sapi.Logger;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(81, 2);
				defaultInterpolatedStringHandler.AppendLiteral("[GuildManager] Cannot invite to guild '");
				defaultInterpolatedStringHandler.AppendFormatted(guildName);
				defaultInterpolatedStringHandler.AppendLiteral("': guild is at maximum capacity (");
				defaultInterpolatedStringHandler.AppendFormatted<int>(maxMembers);
				defaultInterpolatedStringHandler.AppendLiteral(" members)");
				logger.Notification(defaultInterpolatedStringHandler.ToStringAndClear());
				return false;
			}
			GuildInvite invite = new GuildInvite
			{
				InviterUid = inviterUid,
				InviteeUid = inviteeUid,
				GuildName = guildName,
				Timestamp = DateTime.UtcNow,
				ExpiresAt = DateTime.UtcNow.AddMinutes(5.0)
			};
			guild.PendingInvites.Add(invite);
			this.repository.MarkDirty(guildName);
			Action onGuildsChanged = this.OnGuildsChanged;
			if (onGuildsChanged != null)
			{
				onGuildsChanged();
			}
			return true;
		}

		// Token: 0x060007A8 RID: 1960 RVA: 0x00035360 File Offset: 0x00033560
		public bool AcceptInvite(string playerUid)
		{
			this.CleanupExpiredInvites();
			TimeSpan remainingTime;
			if (this.IsPlayerOnCooldown(playerUid, out remainingTime))
			{
				ILogger logger = this.sapi.Logger;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(72, 2);
				defaultInterpolatedStringHandler.AppendLiteral("[GuildManager] Player '");
				defaultInterpolatedStringHandler.AppendFormatted(playerUid);
				defaultInterpolatedStringHandler.AppendLiteral("' cannot join guild - on cooldown for ");
				defaultInterpolatedStringHandler.AppendFormatted<double>(remainingTime.TotalHours, "F1");
				defaultInterpolatedStringHandler.AppendLiteral(" more hours");
				logger.Notification(defaultInterpolatedStringHandler.ToStringAndClear());
				return false;
			}
			List<Guild> allGuilds = this.repository.GetAllGuilds();
			GuildInvite invite = allGuilds.SelectMany((Guild g) => g.PendingInvites).FirstOrDefault((GuildInvite i) => i.InviteeUid == playerUid && !i.IsExpired());
			if (invite == null)
			{
				return false;
			}
			Guild guild = allGuilds.First((Guild g) => g.PendingInvites.Contains(invite));
			int maxMembers = this.configManager.GetConfig().MaxMembersPerGuild;
			if (guild.Members.Count >= maxMembers)
			{
				ILogger logger2 = this.sapi.Logger;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(88, 2);
				defaultInterpolatedStringHandler2.AppendLiteral("[GuildManager] Cannot accept invite to guild '");
				defaultInterpolatedStringHandler2.AppendFormatted(guild.Name);
				defaultInterpolatedStringHandler2.AppendLiteral("': guild is at maximum capacity (");
				defaultInterpolatedStringHandler2.AppendFormatted<int>(maxMembers);
				defaultInterpolatedStringHandler2.AppendLiteral(" members)");
				logger2.Notification(defaultInterpolatedStringHandler2.ToStringAndClear());
				guild.PendingInvites.Remove(invite);
				Action onGuildsChanged = this.OnGuildsChanged;
				if (onGuildsChanged != null)
				{
					onGuildsChanged();
				}
				return false;
			}
			guild.Members[playerUid] = new GuildMember
			{
				PlayerUid = playerUid,
				Role = "Member"
			};
			guild.PendingInvites.Remove(invite);
			this.repository.MarkDirty(guild.Name);
			IServerPlayer player = this.sapi.World.PlayerByUid(playerUid) as IServerPlayer;
			if (player != null)
			{
				GuildTraitManager guildTraitManager = this.traitManager;
				if (guildTraitManager != null)
				{
					guildTraitManager.SyncPlayerTraits(player);
				}
			}
			Action onGuildsChanged2 = this.OnGuildsChanged;
			if (onGuildsChanged2 != null)
			{
				onGuildsChanged2();
			}
			return true;
		}

		// Token: 0x060007A9 RID: 1961 RVA: 0x00035598 File Offset: 0x00033798
		public bool DeclineInvite(string playerUid, string guildName)
		{
			Guild guild = this.GetGuild(guildName);
			if (guild == null)
			{
				return false;
			}
			GuildInvite invite = guild.PendingInvites.FirstOrDefault((GuildInvite i) => i.InviteeUid == playerUid);
			if (invite == null)
			{
				return false;
			}
			guild.PendingInvites.Remove(invite);
			this.repository.MarkDirty(guildName);
			Action onGuildsChanged = this.OnGuildsChanged;
			if (onGuildsChanged != null)
			{
				onGuildsChanged();
			}
			return true;
		}

		// Token: 0x060007AA RID: 1962 RVA: 0x00035608 File Offset: 0x00033808
		public bool CancelInvite(string guildName, string cancellerUid, string inviteeUid, out string message)
		{
			message = "";
			Guild guild = this.GetGuild(guildName);
			if (guild == null)
			{
				message = "Guild not found.";
				return false;
			}
			if (!guild.Members.ContainsKey(cancellerUid))
			{
				message = "You are not a member of this guild.";
				return false;
			}
			if (!GuildManager.HasPermission(guild, cancellerUid, GuildPermission.Invite))
			{
				message = "You don't have permission to manage invites.";
				return false;
			}
			GuildInvite invite = guild.PendingInvites.FirstOrDefault((GuildInvite i) => i.InviteeUid == inviteeUid);
			if (invite == null)
			{
				message = "No pending invite found for that player.";
				return false;
			}
			guild.PendingInvites.Remove(invite);
			this.repository.MarkDirty(guildName);
			Action onGuildsChanged = this.OnGuildsChanged;
			if (onGuildsChanged != null)
			{
				onGuildsChanged();
			}
			message = "Invite cancelled successfully.";
			return true;
		}

		// Token: 0x060007AB RID: 1963 RVA: 0x000356C4 File Offset: 0x000338C4
		public List<GuildInvite> GetPlayerInvites(string playerUid)
		{
			this.CleanupExpiredInvites();
			List<GuildInvite> invites = new List<GuildInvite>();
			Func<GuildInvite, bool> <>9__0;
			foreach (Guild guild in this.repository.GetAllGuilds())
			{
				IEnumerable<GuildInvite> pendingInvites = guild.PendingInvites;
				Func<GuildInvite, bool> predicate;
				if ((predicate = <>9__0) == null)
				{
					predicate = (<>9__0 = ((GuildInvite i) => i.InviteeUid == playerUid && !i.IsExpired()));
				}
				List<GuildInvite> playerInvites = pendingInvites.Where(predicate).ToList<GuildInvite>();
				invites.AddRange(playerInvites);
			}
			return invites;
		}

		// Token: 0x060007AC RID: 1964 RVA: 0x00035768 File Offset: 0x00033968
		private void CleanupExpiredInvites()
		{
			bool hasChanges = false;
			foreach (Guild guild in this.repository.GetAllGuilds())
			{
				List<GuildInvite> expiredInvites = (from i in guild.PendingInvites
				where i.IsExpired()
				select i).ToList<GuildInvite>();
				if (expiredInvites.Count > 0)
				{
					foreach (GuildInvite invite in expiredInvites)
					{
						guild.PendingInvites.Remove(invite);
					}
					this.repository.MarkDirty(guild.Name);
					hasChanges = true;
				}
			}
			if (hasChanges)
			{
				Action onGuildsChanged = this.OnGuildsChanged;
				if (onGuildsChanged == null)
				{
					return;
				}
				onGuildsChanged();
			}
		}

		// Token: 0x060007AD RID: 1965 RVA: 0x00035868 File Offset: 0x00033A68
		public void CleanupExpiredInvitesPublic()
		{
			this.CleanupExpiredInvites();
		}

		// Token: 0x060007AE RID: 1966 RVA: 0x00035870 File Offset: 0x00033A70
		public bool KickMember(string guildName, string removerUid, string targetUid, out string message)
		{
			message = "";
			Guild guild = this.GetGuild(guildName);
			if (guild == null)
			{
				message = "Guild '" + guildName + "' not found.";
				this.sapi.Logger.Warning("[GuildManager] KickMember failed: " + message);
				return false;
			}
			if (!guild.Members.ContainsKey(removerUid))
			{
				message = "You are not a member of this guild.";
				ILogger logger = this.sapi.Logger;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(72, 2);
				defaultInterpolatedStringHandler.AppendLiteral("[GuildManager] KickMember failed: Remover '");
				defaultInterpolatedStringHandler.AppendFormatted(removerUid);
				defaultInterpolatedStringHandler.AppendLiteral("' is not a member of guild '");
				defaultInterpolatedStringHandler.AppendFormatted(guildName);
				defaultInterpolatedStringHandler.AppendLiteral("'");
				logger.Warning(defaultInterpolatedStringHandler.ToStringAndClear());
				return false;
			}
			if (!guild.Members.ContainsKey(targetUid))
			{
				message = "The target player is not a member of this guild.";
				ILogger logger2 = this.sapi.Logger;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(71, 2);
				defaultInterpolatedStringHandler2.AppendLiteral("[GuildManager] KickMember failed: Target '");
				defaultInterpolatedStringHandler2.AppendFormatted(targetUid);
				defaultInterpolatedStringHandler2.AppendLiteral("' is not a member of guild '");
				defaultInterpolatedStringHandler2.AppendFormatted(guildName);
				defaultInterpolatedStringHandler2.AppendLiteral("'");
				logger2.Warning(defaultInterpolatedStringHandler2.ToStringAndClear());
				return false;
			}
			if (!GuildManager.HasPermission(guild, removerUid, GuildPermission.Kick))
			{
				message = "You do not have permission to kick members from this guild.";
				ILogger logger3 = this.sapi.Logger;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler3 = new DefaultInterpolatedStringHandler(78, 2);
				defaultInterpolatedStringHandler3.AppendLiteral("[GuildManager] KickMember failed: Remover '");
				defaultInterpolatedStringHandler3.AppendFormatted(removerUid);
				defaultInterpolatedStringHandler3.AppendLiteral("' lacks Kick permission in guild '");
				defaultInterpolatedStringHandler3.AppendFormatted(guildName);
				defaultInterpolatedStringHandler3.AppendLiteral("'");
				logger3.Warning(defaultInterpolatedStringHandler3.ToStringAndClear());
				return false;
			}
			if (!this.CanActOnPlayer(guild, removerUid, targetUid))
			{
				string targetRole = guild.Members[targetUid].Role;
				message = "You cannot kick this member. They have equal or higher authority (Role: " + targetRole + ").";
				ILogger logger4 = this.sapi.Logger;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler4 = new DefaultInterpolatedStringHandler(108, 3);
				defaultInterpolatedStringHandler4.AppendLiteral("[GuildManager] KickMember failed: Remover '");
				defaultInterpolatedStringHandler4.AppendFormatted(removerUid);
				defaultInterpolatedStringHandler4.AppendLiteral("' cannot kick target '");
				defaultInterpolatedStringHandler4.AppendFormatted(targetUid);
				defaultInterpolatedStringHandler4.AppendLiteral("' due to hierarchy restrictions in guild '");
				defaultInterpolatedStringHandler4.AppendFormatted(guildName);
				defaultInterpolatedStringHandler4.AppendLiteral("'");
				logger4.Warning(defaultInterpolatedStringHandler4.ToStringAndClear());
				return false;
			}
			IPlayer player = this.sapi.World.PlayerByUid(targetUid);
			string targetName = ((player != null) ? player.PlayerName : null) ?? targetUid;
			guild.Members.Remove(targetUid);
			this.repository.MarkDirty(guildName);
			this.SetPlayerCooldown(targetUid, false);
			IServerPlayer serverPlayer = player as IServerPlayer;
			if (serverPlayer != null)
			{
				GuildTraitManager guildTraitManager = this.traitManager;
				if (guildTraitManager != null)
				{
					guildTraitManager.SyncPlayerTraits(serverPlayer);
				}
				ILogger logger5 = this.sapi.Logger;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler5 = new DefaultInterpolatedStringHandler(92, 3);
				defaultInterpolatedStringHandler5.AppendLiteral("[GuildManager] Player '");
				defaultInterpolatedStringHandler5.AppendFormatted(targetUid);
				defaultInterpolatedStringHandler5.AppendLiteral("' was kicked from guild '");
				defaultInterpolatedStringHandler5.AppendFormatted(guildName);
				defaultInterpolatedStringHandler5.AppendLiteral("' by '");
				defaultInterpolatedStringHandler5.AppendFormatted(removerUid);
				defaultInterpolatedStringHandler5.AppendLiteral("' (ONLINE - traits synced immediately)");
				logger5.Notification(defaultInterpolatedStringHandler5.ToStringAndClear());
			}
			else
			{
				ILogger logger6 = this.sapi.Logger;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler6 = new DefaultInterpolatedStringHandler(98, 3);
				defaultInterpolatedStringHandler6.AppendLiteral("[GuildManager] Player '");
				defaultInterpolatedStringHandler6.AppendFormatted(targetUid);
				defaultInterpolatedStringHandler6.AppendLiteral("' was kicked from guild '");
				defaultInterpolatedStringHandler6.AppendFormatted(guildName);
				defaultInterpolatedStringHandler6.AppendLiteral("' by '");
				defaultInterpolatedStringHandler6.AppendFormatted(removerUid);
				defaultInterpolatedStringHandler6.AppendLiteral("' (OFFLINE - traits will sync on next login)");
				logger6.Notification(defaultInterpolatedStringHandler6.ToStringAndClear());
			}
			message = "Successfully kicked " + targetName + " from the guild.";
			Action onGuildsChanged = this.OnGuildsChanged;
			if (onGuildsChanged != null)
			{
				onGuildsChanged();
			}
			return true;
		}

		// Token: 0x060007AF RID: 1967 RVA: 0x00035C00 File Offset: 0x00033E00
		public bool LeaveGuild(string playerUid, out string message)
		{
			message = "";
			Guild guild = this.GetGuildByMember(playerUid);
			if (guild == null)
			{
				message = "You are not in a guild.";
				return false;
			}
			if (!(guild.Members[playerUid].Role == "Leader"))
			{
				guild.Members.Remove(playerUid);
				this.repository.MarkDirty(guild.Name);
				message = "You have left guild '" + guild.Name + "'.";
				this.SetPlayerCooldown(playerUid, false);
				IServerPlayer leavingPlayer = this.sapi.World.PlayerByUid(playerUid) as IServerPlayer;
				if (leavingPlayer != null)
				{
					GuildTraitManager guildTraitManager = this.traitManager;
					if (guildTraitManager != null)
					{
						guildTraitManager.SyncPlayerTraits(leavingPlayer);
					}
				}
				Action onGuildsChanged = this.OnGuildsChanged;
				if (onGuildsChanged != null)
				{
					onGuildsChanged();
				}
				return true;
			}
			if (guild.Members.Count == 1)
			{
				string guildName = guild.Name;
				this.repository.DeleteGuild(guildName);
				LandClaimRepository landClaimRepository = this.landClaimRepository;
				if (landClaimRepository != null)
				{
					landClaimRepository.RemoveGuildFromIndex(guildName);
				}
				message = "Guild '" + guildName + "' has been disbanded as you were the last member.";
				this.SetPlayerCooldown(playerUid, true);
				IServerPlayer player = this.sapi.World.PlayerByUid(playerUid) as IServerPlayer;
				if (player != null)
				{
					GuildTraitManager guildTraitManager2 = this.traitManager;
					if (guildTraitManager2 != null)
					{
						guildTraitManager2.SyncPlayerTraits(player);
					}
				}
				Action onGuildsChanged2 = this.OnGuildsChanged;
				if (onGuildsChanged2 != null)
				{
					onGuildsChanged2();
				}
				return true;
			}
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(133, 1);
			defaultInterpolatedStringHandler.AppendLiteral("As guild leader, you must first transfer leadership to another member or remove all members before leaving. Current members: ");
			defaultInterpolatedStringHandler.AppendFormatted<int>(guild.Members.Count - 1);
			defaultInterpolatedStringHandler.AppendLiteral(" others.");
			message = defaultInterpolatedStringHandler.ToStringAndClear();
			return false;
		}

		// Token: 0x060007B0 RID: 1968 RVA: 0x00035D94 File Offset: 0x00033F94
		public bool PromoteMember(string guildName, string promoterUid, string targetUid, string newRole)
		{
			Guild guild = this.GetGuild(guildName);
			if (guild == null)
			{
				return false;
			}
			if (!guild.Members.ContainsKey(promoterUid))
			{
				return false;
			}
			if (!guild.Members.ContainsKey(targetUid))
			{
				return false;
			}
			if (!guild.Roles.ContainsKey(newRole))
			{
				return false;
			}
			if (!GuildManager.HasPermission(guild, promoterUid, GuildPermission.Promote))
			{
				return false;
			}
			if (!this.CanManageRole(guild, promoterUid, newRole))
			{
				return false;
			}
			if (!this.CanActOnPlayer(guild, promoterUid, targetUid))
			{
				return false;
			}
			guild.Members[targetUid].Role = newRole;
			this.repository.MarkDirty(guildName);
			Action onGuildsChanged = this.OnGuildsChanged;
			if (onGuildsChanged != null)
			{
				onGuildsChanged();
			}
			return true;
		}

		// Token: 0x060007B1 RID: 1969 RVA: 0x00035E38 File Offset: 0x00034038
		public bool CreateRole(string guildName, string creatorUid, string roleName, string description, GuildPermission permissions, int hierarchy)
		{
			Guild guild = this.GetGuild(guildName);
			if (guild == null)
			{
				return false;
			}
			if (!GuildManager.HasPermission(guild, creatorUid, GuildPermission.ManageRoles))
			{
				return false;
			}
			if (guild.Roles.ContainsKey(roleName))
			{
				return false;
			}
			int creatorHierarchy = GuildManager.GetPlayerHierarchy(guild, creatorUid);
			if (hierarchy <= creatorHierarchy)
			{
				return false;
			}
			guild.Roles[roleName] = new GuildRole
			{
				Description = description,
				Permissions = permissions,
				Hierarchy = hierarchy
			};
			this.repository.MarkDirty(guildName);
			Action onGuildsChanged = this.OnGuildsChanged;
			if (onGuildsChanged != null)
			{
				onGuildsChanged();
			}
			return true;
		}

		// Token: 0x060007B2 RID: 1970 RVA: 0x00035EC4 File Offset: 0x000340C4
		public bool UpdateRolePermissions(string guildName, string updaterUid, string roleName, GuildPermission permissions)
		{
			Guild guild = this.GetGuild(guildName);
			if (guild == null)
			{
				return false;
			}
			if (!GuildManager.HasPermission(guild, updaterUid, GuildPermission.ManageRoles))
			{
				return false;
			}
			if (!guild.Roles.ContainsKey(roleName))
			{
				return false;
			}
			if (!this.CanManageRole(guild, updaterUid, roleName))
			{
				return false;
			}
			guild.Roles[roleName].Permissions = permissions;
			this.repository.MarkDirty(guildName);
			Action onGuildsChanged = this.OnGuildsChanged;
			if (onGuildsChanged != null)
			{
				onGuildsChanged();
			}
			return true;
		}

		// Token: 0x060007B3 RID: 1971 RVA: 0x00035F38 File Offset: 0x00034138
		public bool UpdateRolePermissions(string guildName, string updaterUid, string roleName, GuildPermission permissions, int newHierarchy)
		{
			Guild guild = this.GetGuild(guildName);
			if (guild == null)
			{
				return false;
			}
			if (!GuildManager.HasPermission(guild, updaterUid, GuildPermission.ManageRoles))
			{
				return false;
			}
			if (!guild.Roles.ContainsKey(roleName))
			{
				return false;
			}
			if (!this.CanManageRole(guild, updaterUid, roleName))
			{
				return false;
			}
			int updaterHierarchy = GuildManager.GetPlayerHierarchy(guild, updaterUid);
			if (newHierarchy <= updaterHierarchy)
			{
				return false;
			}
			guild.Roles[roleName].Permissions = permissions;
			guild.Roles[roleName].Hierarchy = newHierarchy;
			this.repository.MarkDirty(guildName);
			Action onGuildsChanged = this.OnGuildsChanged;
			if (onGuildsChanged != null)
			{
				onGuildsChanged();
			}
			return true;
		}

		// Token: 0x060007B4 RID: 1972 RVA: 0x00035FCE File Offset: 0x000341CE
		public List<GuildMember> ListMembers(string guildName)
		{
			Guild guild = this.GetGuild(guildName);
			return ((guild != null) ? guild.Members.Values.ToList<GuildMember>() : null) ?? new List<GuildMember>();
		}

		// Token: 0x060007B5 RID: 1973 RVA: 0x00035FF8 File Offset: 0x000341F8
		public bool TransferOwnership(string guildName, string currentLeaderUid, string newLeaderUid, out string message)
		{
			message = "";
			Guild guild = this.GetGuild(guildName);
			if (guild == null)
			{
				message = "Guild '" + guildName + "' not found.";
				this.sapi.Logger.Warning("[GuildManager] TransferOwnership failed: " + message);
				return false;
			}
			if (!guild.Members.ContainsKey(currentLeaderUid))
			{
				message = "You are not a member of this guild.";
				ILogger logger = this.sapi.Logger;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(86, 2);
				defaultInterpolatedStringHandler.AppendLiteral("[GuildManager] TransferOwnership failed: Current leader '");
				defaultInterpolatedStringHandler.AppendFormatted(currentLeaderUid);
				defaultInterpolatedStringHandler.AppendLiteral("' is not a member of guild '");
				defaultInterpolatedStringHandler.AppendFormatted(guildName);
				defaultInterpolatedStringHandler.AppendLiteral("'");
				logger.Warning(defaultInterpolatedStringHandler.ToStringAndClear());
				return false;
			}
			if (!guild.Members.ContainsKey(newLeaderUid))
			{
				message = "The target player is not a member of this guild.";
				ILogger logger2 = this.sapi.Logger;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(78, 2);
				defaultInterpolatedStringHandler2.AppendLiteral("[GuildManager] TransferOwnership failed: Target '");
				defaultInterpolatedStringHandler2.AppendFormatted(newLeaderUid);
				defaultInterpolatedStringHandler2.AppendLiteral("' is not a member of guild '");
				defaultInterpolatedStringHandler2.AppendFormatted(guildName);
				defaultInterpolatedStringHandler2.AppendLiteral("'");
				logger2.Warning(defaultInterpolatedStringHandler2.ToStringAndClear());
				return false;
			}
			if (guild.Members[currentLeaderUid].Role != "Leader")
			{
				message = "Only the guild leader can transfer ownership.";
				ILogger logger3 = this.sapi.Logger;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler3 = new DefaultInterpolatedStringHandler(80, 2);
				defaultInterpolatedStringHandler3.AppendLiteral("[GuildManager] TransferOwnership failed: Player '");
				defaultInterpolatedStringHandler3.AppendFormatted(currentLeaderUid);
				defaultInterpolatedStringHandler3.AppendLiteral("' is not the leader of guild '");
				defaultInterpolatedStringHandler3.AppendFormatted(guildName);
				defaultInterpolatedStringHandler3.AppendLiteral("'");
				logger3.Warning(defaultInterpolatedStringHandler3.ToStringAndClear());
				return false;
			}
			if (currentLeaderUid == newLeaderUid)
			{
				message = "You cannot transfer ownership to yourself.";
				return false;
			}
			IPlayer player = this.sapi.World.PlayerByUid(newLeaderUid);
			string targetName = ((player != null) ? player.PlayerName : null) ?? newLeaderUid;
			guild.Members[currentLeaderUid].Role = "Member";
			guild.Members[newLeaderUid].Role = "Leader";
			this.repository.MarkDirty(guildName);
			message = "Successfully transferred guild leadership to " + targetName + ".";
			ILogger logger4 = this.sapi.Logger;
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler4 = new DefaultInterpolatedStringHandler(60, 3);
			defaultInterpolatedStringHandler4.AppendLiteral("[GuildManager] Guild '");
			defaultInterpolatedStringHandler4.AppendFormatted(guildName);
			defaultInterpolatedStringHandler4.AppendLiteral("' leadership transferred from '");
			defaultInterpolatedStringHandler4.AppendFormatted(currentLeaderUid);
			defaultInterpolatedStringHandler4.AppendLiteral("' to '");
			defaultInterpolatedStringHandler4.AppendFormatted(newLeaderUid);
			defaultInterpolatedStringHandler4.AppendLiteral("'");
			logger4.Notification(defaultInterpolatedStringHandler4.ToStringAndClear());
			Action onGuildsChanged = this.OnGuildsChanged;
			if (onGuildsChanged != null)
			{
				onGuildsChanged();
			}
			return true;
		}

		// Token: 0x060007B6 RID: 1974 RVA: 0x0003629C File Offset: 0x0003449C
		public void SyncGuildMemberTraits(Guild guild)
		{
			GuildTraitManager guildTraitManager = this.traitManager;
			if (guildTraitManager == null)
			{
				return;
			}
			guildTraitManager.SyncGuildMemberTraits(guild);
		}

		// Token: 0x060007B7 RID: 1975 RVA: 0x000362AF File Offset: 0x000344AF
		public void SyncPlayerTraits(IServerPlayer player)
		{
			GuildTraitManager guildTraitManager = this.traitManager;
			if (guildTraitManager == null)
			{
				return;
			}
			guildTraitManager.SyncPlayerTraits(player);
		}

		// Token: 0x060007B8 RID: 1976 RVA: 0x000362C4 File Offset: 0x000344C4
		public static bool IsChunkAdjacentToGuildClaims(Guild guild, int chunkX, int chunkZ)
		{
			if (guild == null || guild.Claims.Count == 0)
			{
				return false;
			}
			foreach (LandClaim claim in guild.Claims)
			{
				GuildHomeClaim guildHome = claim as GuildHomeClaim;
				if (guildHome != null)
				{
					using (IEnumerator<LandClaim> enumerator2 = guildHome.GetIndividualChunks().GetEnumerator())
					{
						while (enumerator2.MoveNext())
						{
							LandClaim landClaim = enumerator2.Current;
							int deltaX = Math.Abs(landClaim.ChunkX - chunkX);
							int deltaZ = Math.Abs(landClaim.ChunkZ - chunkZ);
							if ((deltaX == 1 && deltaZ == 0) || (deltaX == 0 && deltaZ == 1))
							{
								return true;
							}
						}
						continue;
					}
				}
				int deltaX2 = Math.Abs(claim.ChunkX - chunkX);
				int deltaZ2 = Math.Abs(claim.ChunkZ - chunkZ);
				if ((deltaX2 == 1 && deltaZ2 == 0) || (deltaX2 == 0 && deltaZ2 == 1))
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x060007B9 RID: 1977 RVA: 0x000363D4 File Offset: 0x000345D4
		public bool ClaimLand(string guildName, string claimerUid, int blockX, int blockZ, out string error)
		{
			return this.ClaimLand(guildName, claimerUid, blockX, blockZ, false, "", out error);
		}

		// Token: 0x060007BA RID: 1978 RVA: 0x000363EC File Offset: 0x000345EC
		public bool ClaimLand(string guildName, string claimerUid, int blockX, int blockZ, bool isOutpost, string outpostName, [Nullable(2)] out string error)
		{
			Guild guild = this.GetGuild(guildName);
			if (guild == null)
			{
				error = "No guild";
				return false;
			}
			if (!guild.Members.ContainsKey(claimerUid))
			{
				error = "Member is not apart of the guild";
				return false;
			}
			if (!GuildManager.HasPermission(guild, claimerUid, GuildPermission.ManageRoles))
			{
				error = "Member has no permissions to claim plots for guild";
				return false;
			}
			if (isOutpost)
			{
				int maxOutposts = this.GetMaxOutpostsPerGuild(guild);
				if (this.GetOutpostClaimCount(guild) >= maxOutposts)
				{
					error = "Maximum number of outposts reached";
					return false;
				}
			}
			else
			{
				int maxClaims = this.GetMaxClaimsPerGuild(guild);
				if (this.GetNonOutpostClaimCount(guild) >= maxClaims)
				{
					error = "Maximum number of claims reached";
					return false;
				}
			}
			int chunkX = LandClaim.FloorDiv(blockX, 32);
			int chunkZ = LandClaim.FloorDiv(blockZ, 32);
			GuildConfig config = this.configManager.GetConfig();
			BlockPos spawnPos = this.sapi.World.DefaultSpawnPosition.AsBlockPos;
			if (!isOutpost && !config.IsWithinTerritorialBounds(blockX, blockZ, spawnPos))
			{
				error = "Plot outside of allowed boundary";
				return false;
			}
			if (config.IsChunkWithinProtectedZone(chunkX, chunkZ, spawnPos))
			{
				ProtectedZone zone = config.GetProtectedZoneAt(blockX, blockZ, spawnPos);
				error = "Cannot claim land in protected zone: " + (((zone != null) ? zone.Name : null) ?? "Unknown");
				return false;
			}
			if (this.HasGuildHome(guild))
			{
				LandClaim newClaim;
				if (isOutpost)
				{
					newClaim = new OutpostClaim(chunkX, chunkZ, claimerUid, outpostName);
				}
				else
				{
					if (!GuildManager.IsChunkAdjacentToGuildClaims(guild, chunkX, chunkZ))
					{
						error = "Plot is not adjacent to exisiting claims";
						return false;
					}
					newClaim = new LandClaim
					{
						ChunkX = chunkX,
						ChunkZ = chunkZ,
						ClaimedByUid = claimerUid,
						Timestamp = DateTime.UtcNow
					};
				}
				foreach (Guild guild2 in this.repository.GetAllGuilds())
				{
					using (List<LandClaim>.Enumerator enumerator2 = guild2.Claims.GetEnumerator())
					{
						while (enumerator2.MoveNext())
						{
							if (enumerator2.Current.Intersects(newClaim))
							{
								error = "Plot overlaps exisiting claims";
								return false;
							}
						}
					}
				}
				guild.Claims.Add(newClaim);
				this.repository.MarkDirty(guildName);
				LandClaimRepository landClaimRepository = this.landClaimRepository;
				if (landClaimRepository != null)
				{
					landClaimRepository.AddClaimToIndex(guildName, chunkX, chunkZ);
				}
				Action onGuildsChanged = this.OnGuildsChanged;
				if (onGuildsChanged != null)
				{
					onGuildsChanged();
				}
				error = null;
				return true;
			}
			if (isOutpost)
			{
				error = "Guild must establish a home base before creating outposts";
				return false;
			}
			error = null;
			return this.ClaimGuildHome(guildName, claimerUid, chunkX, chunkZ);
		}

		// Token: 0x060007BB RID: 1979 RVA: 0x00036658 File Offset: 0x00034858
		public bool ClaimOutpost(string guildName, string claimerUid, int blockX, int blockZ, string outpostName, [Nullable(2)] out string error)
		{
			return this.ClaimLand(guildName, claimerUid, blockX, blockZ, true, outpostName, out error);
		}

		// Token: 0x060007BC RID: 1980 RVA: 0x0003666C File Offset: 0x0003486C
		public bool ClaimGuildHome(string guildName, string claimerUid, int centerChunkX, int centerChunkZ)
		{
			Guild guild = this.GetGuild(guildName);
			if (guild == null)
			{
				return false;
			}
			if (!guild.Members.ContainsKey(claimerUid))
			{
				return false;
			}
			if (!GuildManager.HasPermission(guild, claimerUid, GuildPermission.ManageRoles))
			{
				return false;
			}
			if (this.HasGuildHome(guild))
			{
				return false;
			}
			GuildConfig config = this.configManager.GetConfig();
			BlockPos spawnPos = this.sapi.World.DefaultSpawnPosition.AsBlockPos;
			for (int dx = 0; dx <= 1; dx++)
			{
				for (int dz = 0; dz <= 1; dz++)
				{
					int chunkX = centerChunkX + dx;
					int chunkZ = centerChunkZ + dz;
					if (!config.IsChunkWithinTerritorialBounds(chunkX, chunkZ, spawnPos))
					{
						return false;
					}
					if (config.IsChunkWithinProtectedZone(chunkX, chunkZ, spawnPos))
					{
						return false;
					}
				}
			}
			GuildHomeClaim guildHome = new GuildHomeClaim(centerChunkX, centerChunkZ, claimerUid);
			int maxClaims = this.GetMaxClaimsPerGuild(guild);
			if (this.GetNonOutpostClaimCount(guild) + 1 > maxClaims)
			{
				return false;
			}
			foreach (Guild guild2 in this.repository.GetAllGuilds())
			{
				using (List<LandClaim>.Enumerator enumerator2 = guild2.Claims.GetEnumerator())
				{
					while (enumerator2.MoveNext())
					{
						if (enumerator2.Current.Intersects(guildHome))
						{
							return false;
						}
					}
				}
			}
			guild.Claims.Add(guildHome);
			this.repository.MarkDirty(guildName);
			if (this.landClaimRepository != null)
			{
				foreach (LandClaim chunk in guildHome.GetIndividualChunks())
				{
					this.landClaimRepository.AddClaimToIndex(guildName, chunk.ChunkX, chunk.ChunkZ);
				}
			}
			Action onGuildsChanged = this.OnGuildsChanged;
			if (onGuildsChanged != null)
			{
				onGuildsChanged();
			}
			return true;
		}

		// Token: 0x060007BD RID: 1981 RVA: 0x00036854 File Offset: 0x00034A54
		private bool HasGuildHome(Guild guild)
		{
			if (guild == null)
			{
				return false;
			}
			return guild.Claims.Any((LandClaim c) => c is GuildHomeClaim);
		}

		// Token: 0x060007BE RID: 1982 RVA: 0x00036885 File Offset: 0x00034A85
		[return: Nullable(2)]
		public GuildHomeClaim GetGuildHome(string guildName)
		{
			Guild guild = this.GetGuild(guildName);
			if (guild == null)
			{
				return null;
			}
			return guild.Claims.OfType<GuildHomeClaim>().FirstOrDefault<GuildHomeClaim>();
		}

		// Token: 0x060007BF RID: 1983 RVA: 0x000368A4 File Offset: 0x00034AA4
		public bool ReleaseClaim(string guildName, LandClaim claim, string requesterUid)
		{
			Guild guild = this.GetGuild(guildName);
			if (guild == null)
			{
				return false;
			}
			if (claim == null)
			{
				return false;
			}
			if (!guild.Claims.Contains(claim))
			{
				return false;
			}
			if (!GuildManager.HasPermission(guild, requesterUid, GuildPermission.ManageRoles) && claim.ClaimedByUid != requesterUid)
			{
				return false;
			}
			guild.Claims.Remove(claim);
			this.repository.MarkDirty(guildName);
			if (this.landClaimRepository != null)
			{
				GuildHomeClaim guildHome = claim as GuildHomeClaim;
				if (guildHome != null)
				{
					using (IEnumerator<LandClaim> enumerator = guildHome.GetIndividualChunks().GetEnumerator())
					{
						while (enumerator.MoveNext())
						{
							LandClaim chunk = enumerator.Current;
							this.landClaimRepository.RemoveClaimFromIndex(chunk.ChunkX, chunk.ChunkZ);
						}
						goto IL_BE;
					}
				}
				this.landClaimRepository.RemoveClaimFromIndex(claim.ChunkX, claim.ChunkZ);
			}
			IL_BE:
			Action onGuildsChanged = this.OnGuildsChanged;
			if (onGuildsChanged != null)
			{
				onGuildsChanged();
			}
			return true;
		}

		// Token: 0x060007C0 RID: 1984 RVA: 0x00036994 File Offset: 0x00034B94
		[return: TupleElementNames(new string[]
		{
			"Success",
			"ErrorMessage"
		})]
		[return: Nullable(new byte[]
		{
			0,
			2
		})]
		public ValueTuple<bool, string> UnclaimLand(string guildName, int chunkX, int chunkZ)
		{
			Guild guild = this.repository.GetGuild(guildName);
			if (guild == null)
			{
				return new ValueTuple<bool, string>(false, "Guild not found.");
			}
			GuildHomeClaim guildHome = guild.Claims.OfType<GuildHomeClaim>().FirstOrDefault((GuildHomeClaim gh) => gh.ContainsBlockCoord(chunkX * 32, chunkZ * 32));
			if (guildHome != null)
			{
				if (guild.Claims.Count > 1)
				{
					return new ValueTuple<bool, string>(false, "Cannot unclaim guild home while other claims exist. Unclaim all other territory first.");
				}
				guild.Claims.Remove(guildHome);
				this.repository.MarkDirty(guildName);
				if (this.landClaimRepository != null)
				{
					foreach (LandClaim chunk in guildHome.GetIndividualChunks())
					{
						this.landClaimRepository.RemoveClaimFromIndex(chunk.ChunkX, chunk.ChunkZ);
					}
				}
				this.sapi.Logger.Notification("[GuildManager] Guild home completely unclaimed for guild '" + guildName + "' (all chunks removed)");
				Action onGuildsChanged = this.OnGuildsChanged;
				if (onGuildsChanged != null)
				{
					onGuildsChanged();
				}
				return new ValueTuple<bool, string>(true, null);
			}
			else
			{
				LandClaim claimToRemove = guild.Claims.FirstOrDefault((LandClaim c) => c.ChunkX == chunkX && c.ChunkZ == chunkZ);
				if (claimToRemove == null)
				{
					return new ValueTuple<bool, string>(false, "This chunk is not claimed by your guild.");
				}
				OutpostClaim outpost = claimToRemove as OutpostClaim;
				if (outpost != null)
				{
					return new ValueTuple<bool, string>(false, "Cannot unclaim outpost center '" + outpost.OutpostName + "'. Delete the entire outpost first.");
				}
				if (this.WouldSplitTerritory(guild, chunkX, chunkZ))
				{
					return new ValueTuple<bool, string>(false, "Cannot unclaim this chunk as it would split your territory. Unclaim edge chunks first.");
				}
				guild.Claims.Remove(claimToRemove);
				this.repository.MarkDirty(guildName);
				LandClaimRepository landClaimRepository = this.landClaimRepository;
				if (landClaimRepository != null)
				{
					landClaimRepository.RemoveClaimFromIndex(chunkX, chunkZ);
				}
				Action onGuildsChanged2 = this.OnGuildsChanged;
				if (onGuildsChanged2 != null)
				{
					onGuildsChanged2();
				}
				return new ValueTuple<bool, string>(true, null);
			}
		}

		// Token: 0x060007C1 RID: 1985 RVA: 0x00036B80 File Offset: 0x00034D80
		private bool WouldSplitTerritory(Guild guild, int unclaimX, int unclaimZ)
		{
			List<ValueTuple<int, int>> remainingChunks = new List<ValueTuple<int, int>>();
			foreach (LandClaim claim in guild.Claims)
			{
				if (!(claim is OutpostClaim))
				{
					GuildHomeClaim guildHome = claim as GuildHomeClaim;
					if (guildHome != null)
					{
						using (IEnumerator<LandClaim> enumerator2 = guildHome.GetIndividualChunks().GetEnumerator())
						{
							while (enumerator2.MoveNext())
							{
								LandClaim homeChunk = enumerator2.Current;
								if (homeChunk.ChunkX != unclaimX || homeChunk.ChunkZ != unclaimZ)
								{
									remainingChunks.Add(new ValueTuple<int, int>(homeChunk.ChunkX, homeChunk.ChunkZ));
								}
							}
							continue;
						}
					}
					if (claim.ChunkX != unclaimX || claim.ChunkZ != unclaimZ)
					{
						remainingChunks.Add(new ValueTuple<int, int>(claim.ChunkX, claim.ChunkZ));
					}
				}
			}
			if (remainingChunks.Count <= 1)
			{
				return false;
			}
			HashSet<ValueTuple<int, int>> visited = new HashSet<ValueTuple<int, int>>();
			Queue<ValueTuple<int, int>> queue = new Queue<ValueTuple<int, int>>();
			queue.Enqueue(remainingChunks[0]);
			while (queue.Count > 0)
			{
				ValueTuple<int, int> current = queue.Dequeue();
				if (!visited.Contains(current))
				{
					visited.Add(current);
					foreach (ValueTuple<int, int> adj in new ValueTuple<int, int>[]
					{
						new ValueTuple<int, int>(current.Item1 + 1, current.Item2),
						new ValueTuple<int, int>(current.Item1 - 1, current.Item2),
						new ValueTuple<int, int>(current.Item1, current.Item2 + 1),
						new ValueTuple<int, int>(current.Item1, current.Item2 - 1)
					})
					{
						if (remainingChunks.Contains(adj) && !visited.Contains(adj))
						{
							queue.Enqueue(adj);
						}
					}
				}
			}
			return visited.Count != remainingChunks.Count;
		}

		// Token: 0x060007C2 RID: 1986 RVA: 0x00036DA0 File Offset: 0x00034FA0
		public List<LandClaim> GetClaims(string guildName)
		{
			Guild guild = this.GetGuild(guildName);
			return ((guild != null) ? guild.Claims.ToList<LandClaim>() : null) ?? new List<LandClaim>();
		}

		// Token: 0x060007C3 RID: 1987 RVA: 0x00036DC3 File Offset: 0x00034FC3
		public List<GuildSummary> GetAllGuildSummaries()
		{
			return this.GetGuildSummariesForPlayer("");
		}

		// Token: 0x060007C4 RID: 1988 RVA: 0x00036DD0 File Offset: 0x00034FD0
		public List<GuildSummary> GetGuildSummariesForPlayer(string playerUid)
		{
			this.CleanupExpiredInvites();
			List<GuildSummary> list = new List<GuildSummary>();
			foreach (Guild g in this.repository.GetAllGuilds())
			{
				int currentMaxClaims = this.GetMaxClaimsPerGuild(g);
				int currentMaxOutposts = this.GetMaxOutpostsPerGuild(g);
				GuildSummary guildSummary = new GuildSummary();
				guildSummary.Name = g.Name;
				guildSummary.Description = g.Description;
				guildSummary.DisplayColor = g.DisplayColor;
				guildSummary.SecondaryColor = g.SecondaryColor;
				guildSummary.Points = g.Points;
				guildSummary.RankClass = this.configManager.GetConfig().GetGuildRankClass(g.Points);
				guildSummary.MemberCount = g.Members.Count;
				guildSummary.MemberUids = g.Members.Keys.ToList<string>();
				guildSummary.PendingInviteUids = (from i in g.PendingInvites
				select i.InviteeUid).ToList<string>();
				guildSummary.PendingInvites = (from i in g.PendingInvites
				select new GuildInviteDto
				{
					InviterUid = i.InviterUid,
					InviteeUid = i.InviteeUid,
					ExpiresAt = i.ExpiresAt
				}).ToList<GuildInviteDto>();
				guildSummary.Roles = new Dictionary<string, GuildRole>(g.Roles);
				guildSummary.Claims = this.ConvertClaimsToDto(g.Claims);
				guildSummary.MaxClaims = currentMaxClaims;
				guildSummary.MaxOutposts = currentMaxOutposts;
				guildSummary.TechProgress = new Dictionary<int, GuildTechProgress>(g.TechProgress);
				guildSummary.TechRequiresPersonalUnlock = new Dictionary<int, bool>(g.TechRequiresPersonalUnlock);
				GuildSummary s = guildSummary;
				if (!string.IsNullOrEmpty(playerUid) && g.Members.ContainsKey(playerUid))
				{
					GuildMember member = g.Members[playerUid];
					s.IsPlayerMember = true;
					s.PlayerRole = member.Role;
					s.MemberPointsContribution = member.PointsContribution;
					s.MemberRank = this.configManager.GetConfig().GetMemberRank(member.PointsContribution);
					PlayerTechProgress playerProgress;
					if (g.PlayerTechProgress.TryGetValue(playerUid, out playerProgress))
					{
						s.PlayerTechProgress = new Dictionary<string, PlayerTechProgress>
						{
							{
								playerUid,
								playerProgress
							}
						};
					}
					s.HasBreakPlacePermission = GuildManager.HasPermission(g, playerUid, GuildPermission.BreakAndPlaceBlocks);
					s.HasInteractPermission = GuildManager.HasPermission(g, playerUid, GuildPermission.InteractBlocks);
				}
				list.Add(s);
			}
			return list;
		}

		// Token: 0x060007C5 RID: 1989 RVA: 0x00037050 File Offset: 0x00035250
		private List<LandClaimDto> ConvertClaimsToDto(List<LandClaim> claims)
		{
			List<LandClaimDto> result = new List<LandClaimDto>();
			foreach (LandClaim claim in claims)
			{
				GuildHomeClaim guildHome = claim as GuildHomeClaim;
				if (guildHome != null)
				{
					using (IEnumerator<LandClaim> enumerator2 = guildHome.GetIndividualChunks().GetEnumerator())
					{
						while (enumerator2.MoveNext())
						{
							LandClaim homeChunk = enumerator2.Current;
							result.Add(new LandClaimDto
							{
								ChunkX = homeChunk.ChunkX,
								ChunkZ = homeChunk.ChunkZ,
								ClaimedByUid = homeChunk.ClaimedByUid,
								Timestamp = homeChunk.Timestamp,
								IsGuildHome = true,
								HomeCenterX = new int?(guildHome.CenterChunkX),
								HomeCenterZ = new int?(guildHome.CenterChunkZ)
							});
						}
						continue;
					}
				}
				OutpostClaim outpost = claim as OutpostClaim;
				if (outpost != null)
				{
					result.Add(new LandClaimDto
					{
						ChunkX = outpost.ChunkX,
						ChunkZ = outpost.ChunkZ,
						ClaimedByUid = outpost.ClaimedByUid,
						Timestamp = outpost.Timestamp,
						IsGuildHome = false,
						IsOutpost = true,
						OutpostName = outpost.OutpostName
					});
				}
				else
				{
					result.Add(new LandClaimDto
					{
						ChunkX = claim.ChunkX,
						ChunkZ = claim.ChunkZ,
						ClaimedByUid = claim.ClaimedByUid,
						Timestamp = claim.Timestamp,
						IsGuildHome = false,
						IsOutpost = false
					});
				}
			}
			return result;
		}

		// Token: 0x060007C6 RID: 1990 RVA: 0x00037220 File Offset: 0x00035420
		public bool ChangeGuildName(string oldName, string changerUid, string newName)
		{
			if (string.IsNullOrWhiteSpace(newName))
			{
				return false;
			}
			if (this.repository.GetGuild(newName) != null)
			{
				return false;
			}
			Guild guild = this.repository.GetGuild(oldName);
			if (guild == null)
			{
				return false;
			}
			if (!GuildManager.HasPermission(guild, changerUid, GuildPermission.ManageRoles))
			{
				return false;
			}
			bool result;
			try
			{
				this.repository.RenameGuild(guild, newName);
				LandClaimRepository landClaimRepository = this.landClaimRepository;
				if (landClaimRepository != null)
				{
					landClaimRepository.UpdateGuildName(oldName, newName);
				}
				Action onGuildsChanged = this.OnGuildsChanged;
				if (onGuildsChanged != null)
				{
					onGuildsChanged();
				}
				result = true;
			}
			catch (Exception ex)
			{
				this.sapi.Logger.Error("[GuildManager] Failed to rename guild '" + oldName + "': " + ex.Message);
				result = false;
			}
			return result;
		}

		// Token: 0x060007C7 RID: 1991 RVA: 0x000372D8 File Offset: 0x000354D8
		public bool IsPlayerInGuildClaim(Guild guild, int blockX, int blockZ)
		{
			if (guild == null || guild.Claims == null || guild.Claims.Count == 0)
			{
				return false;
			}
			using (List<LandClaim>.Enumerator enumerator = guild.Claims.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					if (enumerator.Current.ContainsBlockCoord(blockX, blockZ))
					{
						return true;
					}
				}
			}
			return false;
		}

		// Token: 0x060007C8 RID: 1992 RVA: 0x00037350 File Offset: 0x00035550
		private void LogScalingExamples()
		{
			GuildConfig config = this.configManager.GetConfig();
			if (!config.EnableDynamicClaimLimits && !config.EnableDynamicOutpostLimits)
			{
				this.sapi.Logger.Notification(string.Concat(new string[]
				{
					"Dynamic limits are disabled. All guilds have fixed limits: ",
					config.BaseMaxClaimsPerGuild.ToString(),
					" claims, ",
					config.BaseMaxOutpostsPerGuild.ToString(),
					" outposts."
				}));
				return;
			}
			this.sapi.Logger.Notification("Claim limits scale with guild member count - Examples:");
			foreach (int memberCount in new int[]
			{
				1,
				5,
				10,
				15,
				20,
				25
			})
			{
				int maxClaims = config.CalculateMaxClaimsPerGuild(memberCount);
				int maxOutposts = config.CalculateMaxOutpostsPerGuild(memberCount);
				ILogger logger = this.sapi.Logger;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(45, 3);
				defaultInterpolatedStringHandler.AppendLiteral("  With ");
				defaultInterpolatedStringHandler.AppendFormatted<int>(memberCount);
				defaultInterpolatedStringHandler.AppendLiteral(" member(s): ");
				defaultInterpolatedStringHandler.AppendFormatted<int>(maxClaims);
				defaultInterpolatedStringHandler.AppendLiteral(" max claims, ");
				defaultInterpolatedStringHandler.AppendFormatted<int>(maxOutposts);
				defaultInterpolatedStringHandler.AppendLiteral(" max outposts");
				logger.Notification(defaultInterpolatedStringHandler.ToStringAndClear());
			}
		}

		// Token: 0x060007C9 RID: 1993 RVA: 0x0003748C File Offset: 0x0003568C
		private void SetPlayerCooldown(string playerUid, bool isDisbanding)
		{
			GuildConfig config = this.configManager.GetConfig();
			int cooldownDays = isDisbanding ? config.GuildDisbandCooldownDays : config.GuildRejoinCooldownDays;
			DateTime cooldownEndTime = DateTime.UtcNow.AddDays((double)cooldownDays);
			this.cooldownRepository.SetCooldown(playerUid, cooldownEndTime);
			ILogger logger = this.sapi.Logger;
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(60, 3);
			defaultInterpolatedStringHandler.AppendLiteral("[GuildManager] Player '");
			defaultInterpolatedStringHandler.AppendFormatted(playerUid);
			defaultInterpolatedStringHandler.AppendLiteral("' cooldown set for ");
			defaultInterpolatedStringHandler.AppendFormatted<int>(cooldownDays);
			defaultInterpolatedStringHandler.AppendLiteral(" days (until ");
			defaultInterpolatedStringHandler.AppendFormatted<DateTime>(cooldownEndTime, "yyyy-MM-dd HH:mm:ss");
			defaultInterpolatedStringHandler.AppendLiteral(" UTC)");
			logger.Notification(defaultInterpolatedStringHandler.ToStringAndClear());
		}

		// Token: 0x060007CA RID: 1994 RVA: 0x00037542 File Offset: 0x00035742
		public bool IsPlayerOnCooldown(string playerUid, out TimeSpan remainingTime)
		{
			return this.cooldownRepository.IsOnCooldown(playerUid, out remainingTime);
		}

		// Token: 0x060007CB RID: 1995 RVA: 0x00037551 File Offset: 0x00035751
		public DateTime? GetPlayerCooldownEndTime(string playerUid)
		{
			return this.cooldownRepository.GetCooldown(playerUid);
		}

		// Token: 0x060007CC RID: 1996 RVA: 0x0003755F File Offset: 0x0003575F
		public bool ClearPlayerCooldown(string playerUid)
		{
			if (this.cooldownRepository.ClearCooldown(playerUid))
			{
				this.sapi.Logger.Notification("[GuildManager] Cleared cooldown for player '" + playerUid + "'");
				return true;
			}
			return false;
		}

		// Token: 0x060007CD RID: 1997 RVA: 0x00037594 File Offset: 0x00035794
		public void NodeCaptured(string guildName, string nodeId, string nodeName)
		{
			Guild guild = this.GetGuild(guildName);
			if (guild == null)
			{
				this.sapi.Logger.Warning("[GuildManager] NodeCaptured: Guild '" + guildName + "' not found");
				return;
			}
			foreach (Guild otherGuild in this.repository.GetAllGuilds())
			{
				if (otherGuild.Name != guildName && otherGuild.ControlsNode(nodeId))
				{
					otherGuild.RemoveControlledNode(nodeId);
					this.repository.MarkDirty(otherGuild.Name);
					Action<string, string, string> onNodeLost = this.OnNodeLost;
					if (onNodeLost != null)
					{
						onNodeLost(otherGuild.Name, nodeId, nodeName);
					}
					ILogger logger = this.sapi.Logger;
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(36, 2);
					defaultInterpolatedStringHandler.AppendLiteral("[GuildManager] Guild '");
					defaultInterpolatedStringHandler.AppendFormatted(otherGuild.Name);
					defaultInterpolatedStringHandler.AppendLiteral("' lost node '");
					defaultInterpolatedStringHandler.AppendFormatted(nodeName);
					defaultInterpolatedStringHandler.AppendLiteral("'");
					logger.Notification(defaultInterpolatedStringHandler.ToStringAndClear());
				}
			}
			guild.AddControlledNode(nodeId);
			this.repository.MarkDirty(guildName);
			Action<string, string, string> onNodeCaptured = this.OnNodeCaptured;
			if (onNodeCaptured != null)
			{
				onNodeCaptured(guildName, nodeId, nodeName);
			}
			Action onGuildsChanged = this.OnGuildsChanged;
			if (onGuildsChanged != null)
			{
				onGuildsChanged();
			}
			ILogger logger2 = this.sapi.Logger;
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(40, 2);
			defaultInterpolatedStringHandler2.AppendLiteral("[GuildManager] Guild '");
			defaultInterpolatedStringHandler2.AppendFormatted(guildName);
			defaultInterpolatedStringHandler2.AppendLiteral("' captured node '");
			defaultInterpolatedStringHandler2.AppendFormatted(nodeName);
			defaultInterpolatedStringHandler2.AppendLiteral("'");
			logger2.Notification(defaultInterpolatedStringHandler2.ToStringAndClear());
		}

		// Token: 0x060007CE RID: 1998 RVA: 0x0003774C File Offset: 0x0003594C
		public void NodeLost(string guildName, string nodeId, string nodeName)
		{
			Guild guild = this.GetGuild(guildName);
			if (guild == null)
			{
				return;
			}
			guild.RemoveControlledNode(nodeId);
			this.repository.MarkDirty(guildName);
			Action<string, string, string> onNodeLost = this.OnNodeLost;
			if (onNodeLost != null)
			{
				onNodeLost(guildName, nodeId, nodeName);
			}
			Action onGuildsChanged = this.OnGuildsChanged;
			if (onGuildsChanged != null)
			{
				onGuildsChanged();
			}
			ILogger logger = this.sapi.Logger;
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(36, 2);
			defaultInterpolatedStringHandler.AppendLiteral("[GuildManager] Guild '");
			defaultInterpolatedStringHandler.AppendFormatted(guildName);
			defaultInterpolatedStringHandler.AppendLiteral("' lost node '");
			defaultInterpolatedStringHandler.AppendFormatted(nodeName);
			defaultInterpolatedStringHandler.AppendLiteral("'");
			logger.Notification(defaultInterpolatedStringHandler.ToStringAndClear());
		}

		// Token: 0x060007CF RID: 1999 RVA: 0x000377F4 File Offset: 0x000359F4
		public void SetGuildWarSignup(string guildName, string nodeId)
		{
			Guild guild = this.GetGuild(guildName);
			if (guild == null)
			{
				return;
			}
			guild.SetNodeWarSignup(nodeId);
			this.repository.MarkDirty(guildName);
			Action<string, string> onGuildSignedUpForWar = this.OnGuildSignedUpForWar;
			if (onGuildSignedUpForWar != null)
			{
				onGuildSignedUpForWar(guildName, nodeId);
			}
			Action onGuildsChanged = this.OnGuildsChanged;
			if (onGuildsChanged != null)
			{
				onGuildsChanged();
			}
			ILogger logger = this.sapi.Logger;
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(52, 2);
			defaultInterpolatedStringHandler.AppendLiteral("[GuildManager] Guild '");
			defaultInterpolatedStringHandler.AppendFormatted(guildName);
			defaultInterpolatedStringHandler.AppendLiteral("' signed up for war at node '");
			defaultInterpolatedStringHandler.AppendFormatted(nodeId);
			defaultInterpolatedStringHandler.AppendLiteral("'");
			logger.Debug(defaultInterpolatedStringHandler.ToStringAndClear());
		}

		// Token: 0x060007D0 RID: 2000 RVA: 0x0003789C File Offset: 0x00035A9C
		public void ClearGuildWarSignup(string guildName, string nodeId)
		{
			Guild guild = this.GetGuild(guildName);
			if (guild == null)
			{
				return;
			}
			guild.ClearNodeWarSignup();
			this.repository.MarkDirty(guildName);
			Action<string, string> onGuildCancelledWarSignup = this.OnGuildCancelledWarSignup;
			if (onGuildCancelledWarSignup != null)
			{
				onGuildCancelledWarSignup(guildName, nodeId);
			}
			Action onGuildsChanged = this.OnGuildsChanged;
			if (onGuildsChanged != null)
			{
				onGuildsChanged();
			}
			ILogger logger = this.sapi.Logger;
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(56, 2);
			defaultInterpolatedStringHandler.AppendLiteral("[GuildManager] Guild '");
			defaultInterpolatedStringHandler.AppendFormatted(guildName);
			defaultInterpolatedStringHandler.AppendLiteral("' cancelled war signup for node '");
			defaultInterpolatedStringHandler.AppendFormatted(nodeId);
			defaultInterpolatedStringHandler.AppendLiteral("'");
			logger.Debug(defaultInterpolatedStringHandler.ToStringAndClear());
		}

		// Token: 0x060007D1 RID: 2001 RVA: 0x00037940 File Offset: 0x00035B40
		public List<string> GetGuildControlledNodes(string guildName)
		{
			Guild guild = this.GetGuild(guildName);
			return ((guild != null) ? guild.ControlledNodes : null) ?? new List<string>();
		}

		// Token: 0x060007D2 RID: 2002 RVA: 0x0003795E File Offset: 0x00035B5E
		public bool DoesGuildControlNode(string guildName, string nodeId)
		{
			Guild guild = this.GetGuild(guildName);
			return guild != null && guild.ControlsNode(nodeId);
		}

		// Token: 0x060007D3 RID: 2003 RVA: 0x00037974 File Offset: 0x00035B74
		[return: Nullable(2)]
		public string GetNodeControllingGuild(string nodeId)
		{
			foreach (Guild guild in this.repository.GetAllGuilds())
			{
				if (guild.ControlsNode(nodeId))
				{
					return guild.Name;
				}
			}
			return null;
		}

		// Token: 0x04000331 RID: 817
		private readonly GuildRepository repository;

		// Token: 0x04000332 RID: 818
		private readonly CooldownRepository cooldownRepository;

		// Token: 0x04000333 RID: 819
		[Nullable(2)]
		private readonly LandClaimRepository landClaimRepository;

		// Token: 0x04000334 RID: 820
		private readonly ICoreServerAPI sapi;

		// Token: 0x04000335 RID: 821
		private readonly GuildConfigManager configManager;

		// Token: 0x04000336 RID: 822
		[Nullable(2)]
		private GuildTraitManager traitManager;
	}
}
