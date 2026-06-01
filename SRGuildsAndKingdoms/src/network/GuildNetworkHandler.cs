using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using SRGuildsAndKingdoms.src.config;
using SRGuildsAndKingdoms.src.database;
using SRGuildsAndKingdoms.src.guilds;
using SRGuildsAndKingdoms.src.techblock;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Server;

namespace SRGuildsAndKingdoms.src.network
{
	// Token: 0x02000027 RID: 39
	[NullableContext(1)]
	[Nullable(0)]
	public class GuildNetworkHandler
	{
		// Token: 0x060001B0 RID: 432 RVA: 0x00010B44 File Offset: 0x0000ED44
		public void InitializeServer(ICoreServerAPI api, GuildManager manager)
		{
			this.serverApi = api;
			this.guildManager = manager;
			this.modSystem = api.ModLoader.GetModSystem<SRGuildsAndKingdomsModSystem>(true);
			this.serverApi.Network.RegisterChannel("srguildsandkingdoms:guild").RegisterMessageType<GuildCommandPacket>().RegisterMessageType<GuildCreatePacket>().RegisterMessageType<GuildInvitePacket>().RegisterMessageType<GuildAcceptInvitePacket>().RegisterMessageType<GuildDeclineInvitePacket>().RegisterMessageType<GuildCancelInvitePacket>().RegisterMessageType<GuildListInvitesPacket>().RegisterMessageType<GuildRemoveMemberPacket>().RegisterMessageType<GuildLeavePacket>().RegisterMessageType<GuildClaimLandPacket>().RegisterMessageType<GuildUnclaimLandPacket>().RegisterMessageType<GuildRoleManagementPacket>().RegisterMessageType<GuildMemberListRequestPacket>().RegisterMessageType<GuildTransferOwnershipPacket>().RegisterMessageType<TechContributionRequestPacket>().RegisterMessageType<TechContributionResponsePacket>().RegisterMessageType<ScaledRequirementsRequestPacket>().RegisterMessageType<ScaledRequirementsResponsePacket>().RegisterMessageType<PersonalTechContributionRequestPacket>().RegisterMessageType<PersonalTechContributionResponsePacket>().RegisterMessageType<PersonalUnlockProgressSyncPacket>().RegisterMessageType<PersonalUnlockDto>().RegisterMessageType<ContributionItemDto>().RegisterMessageType<GuildSyncPacket>().RegisterMessageType<GuildUpdatePacket>().RegisterMessageType<GuildNotificationPacket>().RegisterMessageType<GuildInviteNotificationPacket>().RegisterMessageType<GuildInviteListResponsePacket>().RegisterMessageType<GuildInviteInfo>().RegisterMessageType<GuildMemberListPacket>().RegisterMessageType<GuildConfigPacket>().RegisterMessageType<ProtectedZoneData>().RegisterMessageType<TechBlocksConfigSyncPacket>().RegisterMessageType<NodeWarDataRequestPacket>().RegisterMessageType<NodeWarDataResponsePacket>().RegisterMessageType<ControlledNodeDto>().RegisterMessageType<CurrentWarDto>().RegisterMessageType<GuildWarProgressDto>().RegisterMessageType<AvailableWarDto>().RegisterMessageType<CurrentSignupDto>().SetMessageHandler<GuildCommandPacket>(new NetworkClientMessageHandler<GuildCommandPacket>(this.OnGuildCommandReceived)).SetMessageHandler<GuildCreatePacket>(new NetworkClientMessageHandler<GuildCreatePacket>(this.OnGuildCreateReceived)).SetMessageHandler<GuildInvitePacket>(new NetworkClientMessageHandler<GuildInvitePacket>(this.OnGuildInviteReceived)).SetMessageHandler<GuildAcceptInvitePacket>(new NetworkClientMessageHandler<GuildAcceptInvitePacket>(this.OnGuildAcceptInviteReceived)).SetMessageHandler<GuildDeclineInvitePacket>(new NetworkClientMessageHandler<GuildDeclineInvitePacket>(this.OnGuildDeclineInviteReceived)).SetMessageHandler<GuildCancelInvitePacket>(new NetworkClientMessageHandler<GuildCancelInvitePacket>(this.OnGuildCancelInviteReceived)).SetMessageHandler<GuildListInvitesPacket>(new NetworkClientMessageHandler<GuildListInvitesPacket>(this.OnGuildListInvitesReceived)).SetMessageHandler<GuildRemoveMemberPacket>(new NetworkClientMessageHandler<GuildRemoveMemberPacket>(this.OnGuildRemoveMemberReceived)).SetMessageHandler<GuildLeavePacket>(new NetworkClientMessageHandler<GuildLeavePacket>(this.OnGuildLeaveReceived)).SetMessageHandler<GuildClaimLandPacket>(new NetworkClientMessageHandler<GuildClaimLandPacket>(this.OnGuildClaimLandReceived)).SetMessageHandler<GuildUnclaimLandPacket>(new NetworkClientMessageHandler<GuildUnclaimLandPacket>(this.OnGuildUnclaimLandReceived)).SetMessageHandler<GuildRoleManagementPacket>(new NetworkClientMessageHandler<GuildRoleManagementPacket>(this.OnGuildRoleManagementReceived)).SetMessageHandler<GuildMemberListRequestPacket>(new NetworkClientMessageHandler<GuildMemberListRequestPacket>(this.OnGuildMemberListRequestReceived)).SetMessageHandler<GuildTransferOwnershipPacket>(new NetworkClientMessageHandler<GuildTransferOwnershipPacket>(this.OnGuildTransferOwnershipReceived)).SetMessageHandler<TechContributionRequestPacket>(new NetworkClientMessageHandler<TechContributionRequestPacket>(this.OnTechContributionRequestReceived)).SetMessageHandler<ScaledRequirementsRequestPacket>(new NetworkClientMessageHandler<ScaledRequirementsRequestPacket>(this.OnScaledRequirementsRequestReceived)).SetMessageHandler<NodeWarDataRequestPacket>(new NetworkClientMessageHandler<NodeWarDataRequestPacket>(this.OnNodeWarDataRequestReceived));
			this.serverApi.Event.PlayerJoin += new PlayerDelegate(this.OnPlayerJoin);
			this.serverApi.Event.PlayerDisconnect += new PlayerDelegate(this.OnPlayerDisconnect);
		}

		// Token: 0x060001B1 RID: 433 RVA: 0x00010DA8 File Offset: 0x0000EFA8
		public void InitializeClient(ICoreClientAPI api, Action<string, NotificationType> onNotification)
		{
			this.InitializeClient(api, onNotification, null);
		}

		// Token: 0x060001B2 RID: 434 RVA: 0x00010DB4 File Offset: 0x0000EFB4
		public void InitializeClient(ICoreClientAPI api, Action<string, NotificationType> onNotification, [Nullable(new byte[]
		{
			2,
			1,
			1
		})] Action<List<GuildSummary>> onGuildSummaries)
		{
			this.clientApi = api;
			this.onNotificationReceived = onNotification;
			this.onGuildSummariesReceived = onGuildSummaries;
			this.clientApi.Network.RegisterChannel("srguildsandkingdoms:guild").RegisterMessageType<GuildCommandPacket>().RegisterMessageType<GuildCreatePacket>().RegisterMessageType<GuildInvitePacket>().RegisterMessageType<GuildAcceptInvitePacket>().RegisterMessageType<GuildDeclineInvitePacket>().RegisterMessageType<GuildCancelInvitePacket>().RegisterMessageType<GuildListInvitesPacket>().RegisterMessageType<GuildRemoveMemberPacket>().RegisterMessageType<GuildLeavePacket>().RegisterMessageType<GuildClaimLandPacket>().RegisterMessageType<GuildUnclaimLandPacket>().RegisterMessageType<GuildRoleManagementPacket>().RegisterMessageType<GuildMemberListRequestPacket>().RegisterMessageType<GuildTransferOwnershipPacket>().RegisterMessageType<TechContributionRequestPacket>().RegisterMessageType<TechContributionResponsePacket>().RegisterMessageType<ScaledRequirementsRequestPacket>().RegisterMessageType<ScaledRequirementsResponsePacket>().RegisterMessageType<PersonalTechContributionRequestPacket>().RegisterMessageType<PersonalTechContributionResponsePacket>().RegisterMessageType<PersonalUnlockProgressSyncPacket>().RegisterMessageType<PersonalUnlockDto>().RegisterMessageType<ContributionItemDto>().RegisterMessageType<GuildSyncPacket>().RegisterMessageType<GuildUpdatePacket>().RegisterMessageType<GuildNotificationPacket>().RegisterMessageType<GuildInviteNotificationPacket>().RegisterMessageType<GuildInviteListResponsePacket>().RegisterMessageType<GuildInviteInfo>().RegisterMessageType<GuildMemberListPacket>().RegisterMessageType<GuildConfigPacket>().RegisterMessageType<ProtectedZoneData>().RegisterMessageType<TechBlocksConfigSyncPacket>().RegisterMessageType<NodeWarDataRequestPacket>().RegisterMessageType<NodeWarDataResponsePacket>().RegisterMessageType<ControlledNodeDto>().RegisterMessageType<CurrentWarDto>().RegisterMessageType<GuildWarProgressDto>().RegisterMessageType<AvailableWarDto>().RegisterMessageType<CurrentSignupDto>().SetMessageHandler<GuildSyncPacket>(new NetworkServerMessageHandler<GuildSyncPacket>(this.OnGuildSyncReceived)).SetMessageHandler<GuildUpdatePacket>(new NetworkServerMessageHandler<GuildUpdatePacket>(this.OnGuildUpdateReceived)).SetMessageHandler<GuildNotificationPacket>(new NetworkServerMessageHandler<GuildNotificationPacket>(this.OnGuildNotificationReceived)).SetMessageHandler<GuildInviteNotificationPacket>(new NetworkServerMessageHandler<GuildInviteNotificationPacket>(this.OnGuildInviteNotificationReceived)).SetMessageHandler<GuildInviteListResponsePacket>(new NetworkServerMessageHandler<GuildInviteListResponsePacket>(this.OnGuildInviteListResponseReceived)).SetMessageHandler<GuildMemberListPacket>(new NetworkServerMessageHandler<GuildMemberListPacket>(this.OnGuildMemberListReceived)).SetMessageHandler<TechContributionResponsePacket>(new NetworkServerMessageHandler<TechContributionResponsePacket>(this.OnTechContributionResponseReceived)).SetMessageHandler<ScaledRequirementsResponsePacket>(new NetworkServerMessageHandler<ScaledRequirementsResponsePacket>(this.OnScaledRequirementsResponseReceived)).SetMessageHandler<GuildConfigPacket>(new NetworkServerMessageHandler<GuildConfigPacket>(this.OnGuildConfigReceived)).SetMessageHandler<TechBlocksConfigSyncPacket>(new NetworkServerMessageHandler<TechBlocksConfigSyncPacket>(this.OnTechBlocksConfigReceived)).SetMessageHandler<NodeWarDataResponsePacket>(new NetworkServerMessageHandler<NodeWarDataResponsePacket>(this.OnNodeWarDataResponseReceived));
		}

		// Token: 0x060001B3 RID: 435 RVA: 0x00010F6F File Offset: 0x0000F16F
		private void OnPlayerJoin(IServerPlayer player)
		{
			this.BroadcastGuildSummaries(player);
			this.SendGuildConfig(player);
			this.SendTechBlocksConfig(player);
			this.UpdateMemberLastSeen(player.PlayerUID);
		}

		// Token: 0x060001B4 RID: 436 RVA: 0x00010F92 File Offset: 0x0000F192
		private void OnPlayerDisconnect(IServerPlayer player)
		{
			this.UpdateMemberLastSeen(player.PlayerUID);
		}

		// Token: 0x060001B5 RID: 437 RVA: 0x00010FA0 File Offset: 0x0000F1A0
		private void UpdateMemberLastSeen(string playerUid)
		{
			GuildManager guildManager = this.guildManager;
			Guild guild = (guildManager != null) ? guildManager.GetGuildByMember(playerUid) : null;
			if (guild != null && guild.Members.ContainsKey(playerUid))
			{
				guild.Members[playerUid].LastSeen = DateTime.UtcNow;
				ICoreServerAPI coreServerAPI = this.serverApi;
				SRGuildsAndKingdomsModSystem srguildsAndKingdomsModSystem = (coreServerAPI != null) ? coreServerAPI.ModLoader.GetModSystem<SRGuildsAndKingdomsModSystem>(true) : null;
				if (srguildsAndKingdomsModSystem == null)
				{
					return;
				}
				GuildRepository guildRepository = srguildsAndKingdomsModSystem.GetGuildRepository();
				if (guildRepository == null)
				{
					return;
				}
				guildRepository.MarkDirty(guild.Name);
			}
		}

		// Token: 0x060001B6 RID: 438 RVA: 0x0001101C File Offset: 0x0000F21C
		private void OnGuildMemberListRequestReceived(IServerPlayer player, GuildMemberListRequestPacket packet)
		{
			GuildManager guildManager = this.guildManager;
			Guild guild = (guildManager != null) ? guildManager.GetGuildByMember(player.PlayerUID) : null;
			if (guild == null)
			{
				this.SendNotification(player, "You are not in a guild.", NotificationType.Error);
				return;
			}
			List<GuildMemberInfo> memberInfoList = new List<GuildMemberInfo>();
			IPlayer[] onlinePlayers = this.serverApi.World.AllOnlinePlayers;
			HashSet<string> onlinePlayerUids = new HashSet<string>(from p in onlinePlayers
			select p.PlayerUID);
			using (Dictionary<string, GuildMember>.ValueCollection.Enumerator enumerator = guild.Members.Values.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					GuildMember member = enumerator.Current;
					IPlayer player2 = onlinePlayers.FirstOrDefault((IPlayer p) => p.PlayerUID == member.PlayerUid);
					string text;
					if ((text = ((player2 != null) ? player2.PlayerName : null)) == null)
					{
						IServerPlayerData playerDataByUid = this.serverApi.PlayerData.GetPlayerDataByUid(member.PlayerUid);
						text = (((playerDataByUid != null) ? playerDataByUid.LastKnownPlayername : null) ?? "Unknown");
					}
					string playerName = text;
					GuildMemberInfo memberInfo = new GuildMemberInfo
					{
						PlayerUid = member.PlayerUid,
						PlayerName = playerName,
						Role = member.Role,
						IsOnline = onlinePlayerUids.Contains(member.PlayerUid),
						LastSeenTicks = member.LastSeen.Ticks
					};
					memberInfoList.Add(memberInfo);
				}
			}
			GuildMemberListPacket response = new GuildMemberListPacket
			{
				PlayerUid = player.PlayerUID,
				Members = memberInfoList
			};
			this.serverApi.Network.GetChannel("srguildsandkingdoms:guild").SendPacket<GuildMemberListPacket>(response, new IServerPlayer[]
			{
				player
			});
		}

		// Token: 0x060001B7 RID: 439 RVA: 0x000111F4 File Offset: 0x0000F3F4
		private void OnGuildCommandReceived(IServerPlayer player, GuildCommandPacket packet)
		{
			GuildManager guildManager = this.guildManager;
			Guild guild = (guildManager != null) ? guildManager.GetGuildByMember(player.PlayerUID) : null;
			if (guild == null)
			{
				this.SendNotification(player, "You are not in a guild.", NotificationType.Error);
				return;
			}
			if (!GuildManager.HasPermission(guild, player.PlayerUID, GuildPermission.ManageRoles))
			{
				this.SendNotification(player, "You don't have permission to change guild settings.", NotificationType.Error);
				return;
			}
			string command = packet.Command;
			string a = (command != null) ? command.ToLowerInvariant() : null;
			if (!(a == "changename"))
			{
				if (!(a == "changecolor"))
				{
					if (!(a == "changesecondarycolor"))
					{
						if (!(a == "changedescription"))
						{
							this.SendNotification(player, "Unknown guild command: " + packet.Command, NotificationType.Error);
						}
						else
						{
							string[] arguments = packet.Arguments;
							if (arguments == null || arguments.Length == 0)
							{
								this.SendNotification(player, "Invalid description.", NotificationType.Error);
								return;
							}
							string newDescription = packet.Arguments[0];
							if (newDescription.Length > 100)
							{
								this.SendNotification(player, "Guild description cannot exceed 100 characters.", NotificationType.Error);
								return;
							}
							guild.Description = newDescription;
							ICoreServerAPI coreServerAPI = this.serverApi;
							object obj;
							if (coreServerAPI == null)
							{
								obj = null;
							}
							else
							{
								SRGuildsAndKingdomsModSystem srguildsAndKingdomsModSystem = coreServerAPI.ModLoader.GetModSystem<SRGuildsAndKingdomsModSystem>(true);
								obj = ((srguildsAndKingdomsModSystem != null) ? srguildsAndKingdomsModSystem.GetGuildRepository() : null);
							}
							object obj2 = obj;
							if (obj2 != null)
							{
								obj2.MarkDirty(guild.Name);
							}
							this.SendNotification(player, "Guild description updated.", NotificationType.Success);
							this.BroadcastGuildSummariesToAll();
							return;
						}
					}
					else
					{
						string[] arguments2 = packet.Arguments;
						int secondaryColor;
						if (arguments2 != null && arguments2.Length != 0 && int.TryParse(packet.Arguments[0], out secondaryColor))
						{
							guild.SecondaryColor = secondaryColor;
							ICoreServerAPI coreServerAPI2 = this.serverApi;
							object obj3;
							if (coreServerAPI2 == null)
							{
								obj3 = null;
							}
							else
							{
								SRGuildsAndKingdomsModSystem srguildsAndKingdomsModSystem2 = coreServerAPI2.ModLoader.GetModSystem<SRGuildsAndKingdomsModSystem>(true);
								obj3 = ((srguildsAndKingdomsModSystem2 != null) ? srguildsAndKingdomsModSystem2.GetGuildRepository() : null);
							}
							object obj4 = obj3;
							if (obj4 != null)
							{
								obj4.MarkDirty(guild.Name);
							}
							this.SendNotification(player, "Guild secondary color updated.", NotificationType.Success);
							this.BroadcastGuildSummariesToAll();
							return;
						}
						this.SendNotification(player, "Invalid color value.", NotificationType.Error);
						return;
					}
				}
				else
				{
					string[] arguments3 = packet.Arguments;
					int primaryColor;
					if (arguments3 != null && arguments3.Length != 0 && int.TryParse(packet.Arguments[0], out primaryColor))
					{
						guild.DisplayColor = primaryColor;
						ICoreServerAPI coreServerAPI3 = this.serverApi;
						object obj5;
						if (coreServerAPI3 == null)
						{
							obj5 = null;
						}
						else
						{
							SRGuildsAndKingdomsModSystem srguildsAndKingdomsModSystem3 = coreServerAPI3.ModLoader.GetModSystem<SRGuildsAndKingdomsModSystem>(true);
							obj5 = ((srguildsAndKingdomsModSystem3 != null) ? srguildsAndKingdomsModSystem3.GetGuildRepository() : null);
						}
						object obj6 = obj5;
						if (obj6 != null)
						{
							obj6.MarkDirty(guild.Name);
						}
						this.SendNotification(player, "Guild primary color updated.", NotificationType.Success);
						this.BroadcastGuildSummariesToAll();
						return;
					}
					this.SendNotification(player, "Invalid color value.", NotificationType.Error);
					return;
				}
			}
			else
			{
				string[] arguments4 = packet.Arguments;
				if (arguments4 != null && arguments4.Length != 0)
				{
					string newName = packet.Arguments[0];
					if (string.IsNullOrWhiteSpace(newName))
					{
						this.SendNotification(player, "Guild name cannot be empty.", NotificationType.Error);
						return;
					}
					if (this.guildManager.ChangeGuildName(guild.Name, player.PlayerUID, newName))
					{
						this.SendNotification(player, "Guild name changed to '" + newName + "'.", NotificationType.Success);
						this.BroadcastGuildSummariesToAll();
						return;
					}
					this.SendNotification(player, "Failed to change guild name. Name may already be in use.", NotificationType.Error);
					return;
				}
			}
		}

		// Token: 0x060001B8 RID: 440 RVA: 0x000114D0 File Offset: 0x0000F6D0
		private void OnGuildCreateReceived(IServerPlayer player, GuildCreatePacket packet)
		{
			if (this.guildManager.CreateGuild(packet.GuildName, player.PlayerUID, packet.Description))
			{
				this.SendNotification(player, "Guild '" + packet.GuildName + "' created successfully.", NotificationType.Success);
				this.BroadcastGuildSummariesToAll();
				return;
			}
			this.SendNotification(player, "Failed to create guild '" + packet.GuildName + "'. It may already exist or you may already be in a guild.", NotificationType.Error);
		}

		// Token: 0x060001B9 RID: 441 RVA: 0x00011540 File Offset: 0x0000F740
		private void OnGuildInviteReceived(IServerPlayer player, GuildInvitePacket packet)
		{
			IPlayer invitee = this.serverApi.World.AllOnlinePlayers.FirstOrDefault((IPlayer p) => p.PlayerUID.Equals(packet.TargetPlayerUid, StringComparison.OrdinalIgnoreCase));
			if (invitee == null)
			{
				this.SendNotification(player, "Player not found or is offline.", NotificationType.Error);
				return;
			}
			Guild guild = this.guildManager.GetGuildByMember(player.PlayerUID);
			if (guild == null)
			{
				this.SendNotification(player, "You are not in a guild.", NotificationType.Error);
				return;
			}
			if (this.guildManager.InviteToGuild(guild.Name, player.PlayerUID, invitee.PlayerUID))
			{
				this.SendNotification(player, "Invited " + invitee.PlayerName + " to guild.", NotificationType.Success);
				GuildInvite invite = guild.PendingInvites.FirstOrDefault((GuildInvite i) => i.InviteeUid == invitee.PlayerUID);
				if (invite != null)
				{
					GuildInviteNotificationPacket inviteNotification = new GuildInviteNotificationPacket
					{
						PlayerUid = invitee.PlayerUID,
						InviterName = player.PlayerName,
						InviterUid = player.PlayerUID,
						GuildName = guild.Name,
						ExpiresAtTicks = invite.ExpiresAt.Ticks
					};
					this.serverApi.Network.GetChannel("srguildsandkingdoms:guild").SendPacket<GuildInviteNotificationPacket>(inviteNotification, new IServerPlayer[]
					{
						invitee as IServerPlayer
					});
					return;
				}
			}
			else
			{
				this.SendNotification(player, "Could not invite " + invitee.PlayerName + ".", NotificationType.Error);
			}
		}

		// Token: 0x060001BA RID: 442 RVA: 0x000116C4 File Offset: 0x0000F8C4
		private void OnGuildAcceptInviteReceived(IServerPlayer player, GuildAcceptInvitePacket packet)
		{
			if (this.guildManager.AcceptInvite(player.PlayerUID))
			{
				this.SendNotification(player, "You have joined the guild.", NotificationType.Success);
				Guild guild = this.guildManager.GetGuildByMember(player.PlayerUID);
				if (guild != null && this.serverApi.ModLoader.GetModSystem<SRGuildsAndKingdomsModSystem>(true) != null)
				{
					int count = guild.Members.Count;
				}
				this.BroadcastGuildSummariesToAll();
				return;
			}
			this.SendNotification(player, "No pending guild invite found or invite has expired.", NotificationType.Error);
		}

		// Token: 0x060001BB RID: 443 RVA: 0x0001173C File Offset: 0x0000F93C
		private void OnGuildDeclineInviteReceived(IServerPlayer player, GuildDeclineInvitePacket packet)
		{
			if (this.guildManager.DeclineInvite(player.PlayerUID, packet.GuildName))
			{
				this.SendNotification(player, "You have declined the invite to " + packet.GuildName + ".", NotificationType.Info);
				return;
			}
			this.SendNotification(player, "No pending invite found for that guild.", NotificationType.Error);
		}

		// Token: 0x060001BC RID: 444 RVA: 0x00011790 File Offset: 0x0000F990
		private void OnGuildCancelInviteReceived(IServerPlayer player, GuildCancelInvitePacket packet)
		{
			GuildManager guildManager = this.guildManager;
			Guild guild = (guildManager != null) ? guildManager.GetGuildByMember(player.PlayerUID) : null;
			if (guild == null)
			{
				this.SendNotification(player, "You are not in a guild.", NotificationType.Error);
				return;
			}
			string message;
			if (this.guildManager.CancelInvite(guild.Name, player.PlayerUID, packet.InviteeUid, out message))
			{
				this.SendNotification(player, message, NotificationType.Success);
				this.BroadcastGuildSummariesToAll();
				return;
			}
			this.SendNotification(player, message, NotificationType.Error);
		}

		// Token: 0x060001BD RID: 445 RVA: 0x00011804 File Offset: 0x0000FA04
		private void OnGuildListInvitesReceived(IServerPlayer player, GuildListInvitesPacket packet)
		{
			List<GuildInvite> playerInvites = this.guildManager.GetPlayerInvites(player.PlayerUID);
			List<GuildInviteInfo> inviteInfoList = new List<GuildInviteInfo>();
			using (List<GuildInvite>.Enumerator enumerator = playerInvites.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					GuildInvite invite = enumerator.Current;
					IPlayer inviter = this.serverApi.World.AllPlayers.FirstOrDefault((IPlayer p) => p.PlayerUID == invite.InviterUid);
					inviteInfoList.Add(new GuildInviteInfo
					{
						GuildName = invite.GuildName,
						InviterName = (((inviter != null) ? inviter.PlayerName : null) ?? "Unknown"),
						InviterUid = invite.InviterUid,
						ExpiresAtTicks = invite.ExpiresAt.Ticks
					});
				}
			}
			GuildInviteListResponsePacket response = new GuildInviteListResponsePacket
			{
				PlayerUid = player.PlayerUID,
				Invites = inviteInfoList
			};
			this.serverApi.Network.GetChannel("srguildsandkingdoms:guild").SendPacket<GuildInviteListResponsePacket>(response, new IServerPlayer[]
			{
				player
			});
		}

		// Token: 0x060001BE RID: 446 RVA: 0x00011938 File Offset: 0x0000FB38
		private void OnGuildRemoveMemberReceived(IServerPlayer player, GuildRemoveMemberPacket packet)
		{
			Guild guild = this.guildManager.GetGuildByMember(player.PlayerUID);
			if (guild == null)
			{
				this.SendNotification(player, "You are not in a guild.", NotificationType.Error);
				return;
			}
			string targetPlayerUid = this.FindGuildMemberByName(guild, packet.TargetPlayerName);
			if (targetPlayerUid == null)
			{
				this.SendNotification(player, "Player '" + packet.TargetPlayerName + "' not found in guild.", NotificationType.Error);
				return;
			}
			string message;
			if (this.guildManager.KickMember(guild.Name, player.PlayerUID, targetPlayerUid, out message))
			{
				this.SendNotification(player, message, NotificationType.Success);
				IServerPlayer targetPlayer = this.serverApi.World.AllOnlinePlayers.FirstOrDefault((IPlayer p) => p.PlayerUID == targetPlayerUid) as IServerPlayer;
				if (targetPlayer != null)
				{
					this.SendNotification(targetPlayer, "You have been removed from the guild.", NotificationType.Warning);
				}
				this.BroadcastGuildSummariesToAll();
				return;
			}
			this.SendNotification(player, message, NotificationType.Error);
		}

		// Token: 0x060001BF RID: 447 RVA: 0x00011A18 File Offset: 0x0000FC18
		private void OnGuildLeaveReceived(IServerPlayer player, GuildLeavePacket packet)
		{
			string message;
			if (this.guildManager.LeaveGuild(player.PlayerUID, out message))
			{
				this.SendNotification(player, message, NotificationType.Success);
				this.BroadcastGuildSummariesToAll();
				return;
			}
			this.SendNotification(player, message, NotificationType.Error);
		}

		// Token: 0x060001C0 RID: 448 RVA: 0x00011A54 File Offset: 0x0000FC54
		private void OnGuildTransferOwnershipReceived(IServerPlayer player, GuildTransferOwnershipPacket packet)
		{
			GuildManager guildManager = this.guildManager;
			Guild guild = (guildManager != null) ? guildManager.GetGuildByMember(player.PlayerUID) : null;
			if (guild == null)
			{
				this.SendNotification(player, "You are not in a guild.", NotificationType.Error);
				return;
			}
			string message;
			if (this.guildManager.TransferOwnership(guild.Name, player.PlayerUID, packet.TargetPlayerUid, out message))
			{
				this.SendNotification(player, message, NotificationType.Success);
				ICoreServerAPI coreServerAPI = this.serverApi;
				IServerPlayer newLeader = ((coreServerAPI != null) ? coreServerAPI.World.PlayerByUid(packet.TargetPlayerUid) : null) as IServerPlayer;
				if (newLeader != null)
				{
					this.SendNotification(newLeader, "You have been promoted to guild leader of '" + guild.Name + "'!", NotificationType.Success);
				}
				this.BroadcastGuildSummariesToAll();
				return;
			}
			this.SendNotification(player, message, NotificationType.Error);
		}

		// Token: 0x060001C1 RID: 449 RVA: 0x00011B08 File Offset: 0x0000FD08
		private void OnGuildClaimLandReceived(IServerPlayer player, GuildClaimLandPacket packet)
		{
			Guild guild = this.guildManager.GetGuildByMember(player.PlayerUID);
			if (guild == null)
			{
				this.SendNotification(player, "You are not in a guild.", NotificationType.Error);
				return;
			}
			if (!GuildManager.HasPermission(guild, player.PlayerUID, GuildPermission.ManageRoles))
			{
				this.SendNotification(player, "You don't have permission to claim land for the guild.", NotificationType.Error);
				return;
			}
			if (packet.IsOutpost)
			{
				int maxOutposts = this.guildManager.GetMaxOutpostsPerGuild(guild);
				int currentOutposts = this.guildManager.GetOutpostClaimCount(guild);
				if (currentOutposts >= maxOutposts)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(95, 2);
					defaultInterpolatedStringHandler.AppendLiteral("Cannot create more outposts. Your guild has reached the maximum limit of ");
					defaultInterpolatedStringHandler.AppendFormatted<int>(maxOutposts);
					defaultInterpolatedStringHandler.AppendLiteral(" outposts (current: ");
					defaultInterpolatedStringHandler.AppendFormatted<int>(currentOutposts);
					defaultInterpolatedStringHandler.AppendLiteral(").");
					this.SendNotification(player, defaultInterpolatedStringHandler.ToStringAndClear(), NotificationType.Error);
					return;
				}
			}
			else
			{
				int maxClaims = this.guildManager.GetMaxClaimsPerGuild(guild);
				int currentClaims = this.guildManager.GetNonOutpostClaimCount(guild);
				if (currentClaims >= maxClaims)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(88, 2);
					defaultInterpolatedStringHandler2.AppendLiteral("Cannot claim more land. Your guild has reached the maximum limit of ");
					defaultInterpolatedStringHandler2.AppendFormatted<int>(maxClaims);
					defaultInterpolatedStringHandler2.AppendLiteral(" claims (current: ");
					defaultInterpolatedStringHandler2.AppendFormatted<int>(currentClaims);
					defaultInterpolatedStringHandler2.AppendLiteral(").");
					this.SendNotification(player, defaultInterpolatedStringHandler2.ToStringAndClear(), NotificationType.Error);
					return;
				}
			}
			GuildConfig config = this.guildManager.GetConfigManager().GetConfig();
			int chunkX = LandClaim.FloorDiv(packet.BlockX, 32);
			int chunkZ = LandClaim.FloorDiv(packet.BlockZ, 32);
			bool needsGuildHome = !guild.Claims.Any((LandClaim c) => c is GuildHomeClaim);
			if (packet.IsOutpost && needsGuildHome)
			{
				this.SendNotification(player, "Guild must establish a home base before creating outposts.", NotificationType.Error);
				return;
			}
			if (needsGuildHome && config.EnableTerritorialRestrictions)
			{
				for (int dx = 0; dx <= 1; dx++)
				{
					for (int dz = 0; dz <= 1; dz++)
					{
						int chunkX2 = chunkX;
						int chunkZ2 = chunkZ;
					}
				}
			}
			ICoreServerAPI coreServerAPI = this.serverApi;
			SRGuildsAndKingdomsModSystem srguildsAndKingdomsModSystem = (coreServerAPI != null) ? coreServerAPI.ModLoader.GetModSystem<SRGuildsAndKingdomsModSystem>(true) : null;
			List<Guild> list;
			if (srguildsAndKingdomsModSystem == null)
			{
				list = null;
			}
			else
			{
				GuildRepository guildRepository = srguildsAndKingdomsModSystem.GetGuildRepository();
				list = ((guildRepository != null) ? guildRepository.GetAllGuilds() : null);
			}
			List<Guild> allGuilds = list;
			if (allGuilds != null && allGuilds.Count > 0)
			{
				Dictionary<string, Guild> guildDict = allGuilds.ToDictionary((Guild g) => g.Name, (Guild g) => g);
				ValueTuple<bool, string, double> valueTuple = this.IsChunkTooCloseToOtherGuildClaim(chunkX, chunkZ, guild.Name, guildDict);
				bool tooClose = valueTuple.Item1;
				string nearestGuild = valueTuple.Item2;
				double distance = valueTuple.Item3;
				if (tooClose)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler3 = new DefaultInterpolatedStringHandler(90, 4);
					defaultInterpolatedStringHandler3.AppendLiteral("Cannot claim chunk (");
					defaultInterpolatedStringHandler3.AppendFormatted<int>(chunkX);
					defaultInterpolatedStringHandler3.AppendLiteral(", ");
					defaultInterpolatedStringHandler3.AppendFormatted<int>(chunkZ);
					defaultInterpolatedStringHandler3.AppendLiteral("). Too close to ");
					defaultInterpolatedStringHandler3.AppendFormatted(nearestGuild);
					defaultInterpolatedStringHandler3.AppendLiteral("'s territory (");
					defaultInterpolatedStringHandler3.AppendFormatted<double>(distance, "F0");
					defaultInterpolatedStringHandler3.AppendLiteral(" blocks, minimum 300 blocks required).");
					this.SendNotification(player, defaultInterpolatedStringHandler3.ToStringAndClear(), NotificationType.Error);
					return;
				}
				if (needsGuildHome)
				{
					for (int dx2 = 0; dx2 <= 1; dx2++)
					{
						for (int dz2 = 0; dz2 <= 1; dz2++)
						{
							int checkChunkX = chunkX + dx2;
							int checkChunkZ = chunkZ + dz2;
							ValueTuple<bool, string, double> valueTuple2 = this.IsChunkTooCloseToOtherGuildClaim(checkChunkX, checkChunkZ, guild.Name, guildDict);
							bool homeChunkTooClose = valueTuple2.Item1;
							string homeNearestGuild = valueTuple2.Item2;
							double homeDistance = valueTuple2.Item3;
							if (homeChunkTooClose)
							{
								DefaultInterpolatedStringHandler defaultInterpolatedStringHandler4 = new DefaultInterpolatedStringHandler(131, 4);
								defaultInterpolatedStringHandler4.AppendLiteral("Cannot establish guild home at chunk (");
								defaultInterpolatedStringHandler4.AppendFormatted<int>(chunkX);
								defaultInterpolatedStringHandler4.AppendLiteral(",");
								defaultInterpolatedStringHandler4.AppendFormatted<int>(chunkZ);
								defaultInterpolatedStringHandler4.AppendLiteral("). Part of the 2x2 area is too close to ");
								defaultInterpolatedStringHandler4.AppendFormatted(homeNearestGuild);
								defaultInterpolatedStringHandler4.AppendLiteral("'s territory (");
								defaultInterpolatedStringHandler4.AppendFormatted<double>(homeDistance, "F0");
								defaultInterpolatedStringHandler4.AppendLiteral(" blocks, minimum 300 blocks required).");
								this.SendNotification(player, defaultInterpolatedStringHandler4.ToStringAndClear(), NotificationType.Error);
								return;
							}
							Func<LandClaim, bool> <>9__3;
							foreach (Guild otherGuild in guildDict.Values)
							{
								IEnumerable<LandClaim> claims = otherGuild.Claims;
								Func<LandClaim, bool> predicate;
								if ((predicate = <>9__3) == null)
								{
									predicate = (<>9__3 = ((LandClaim c) => c.ContainsChunk(checkChunkX, checkChunkZ)));
								}
								if (claims.Any(predicate))
								{
									DefaultInterpolatedStringHandler defaultInterpolatedStringHandler5 = new DefaultInterpolatedStringHandler(100, 3);
									defaultInterpolatedStringHandler5.AppendLiteral("Cannot establish guild home at chunk (");
									defaultInterpolatedStringHandler5.AppendFormatted<int>(chunkX);
									defaultInterpolatedStringHandler5.AppendLiteral(",");
									defaultInterpolatedStringHandler5.AppendFormatted<int>(chunkZ);
									defaultInterpolatedStringHandler5.AppendLiteral("). The 2x2 area conflicts with existing claims from guild '");
									defaultInterpolatedStringHandler5.AppendFormatted(otherGuild.Name);
									defaultInterpolatedStringHandler5.AppendLiteral("'.");
									this.SendNotification(player, defaultInterpolatedStringHandler5.ToStringAndClear(), NotificationType.Error);
									return;
								}
							}
						}
					}
					int maxClaims2 = this.guildManager.GetMaxClaimsPerGuild(guild);
					if (this.guildManager.GetNonOutpostClaimCount(guild) + 1 > maxClaims2)
					{
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler6 = new DefaultInterpolatedStringHandler(68, 1);
						defaultInterpolatedStringHandler6.AppendLiteral("Cannot establish guild home. Your guild can only have ");
						defaultInterpolatedStringHandler6.AppendFormatted<int>(maxClaims2);
						defaultInterpolatedStringHandler6.AppendLiteral(" total claims.");
						this.SendNotification(player, defaultInterpolatedStringHandler6.ToStringAndClear(), NotificationType.Error);
						return;
					}
				}
				else
				{
					Func<LandClaim, bool> <>9__4;
					foreach (Guild otherGuild2 in guildDict.Values)
					{
						IEnumerable<LandClaim> claims2 = otherGuild2.Claims;
						Func<LandClaim, bool> predicate2;
						if ((predicate2 = <>9__4) == null)
						{
							predicate2 = (<>9__4 = ((LandClaim c) => c.ContainsChunk(chunkX, chunkZ)));
						}
						if (claims2.Any(predicate2))
						{
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler7 = new DefaultInterpolatedStringHandler(59, 3);
							defaultInterpolatedStringHandler7.AppendLiteral("Cannot claim chunk (");
							defaultInterpolatedStringHandler7.AppendFormatted<int>(chunkX);
							defaultInterpolatedStringHandler7.AppendLiteral(", ");
							defaultInterpolatedStringHandler7.AppendFormatted<int>(chunkZ);
							defaultInterpolatedStringHandler7.AppendLiteral("). It is already claimed by guild '");
							defaultInterpolatedStringHandler7.AppendFormatted(otherGuild2.Name);
							defaultInterpolatedStringHandler7.AppendLiteral("'.");
							this.SendNotification(player, defaultInterpolatedStringHandler7.ToStringAndClear(), NotificationType.Error);
							return;
						}
					}
					if (!packet.IsOutpost && !GuildManager.IsChunkAdjacentToGuildClaims(guild, chunkX, chunkZ))
					{
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler8 = new DefaultInterpolatedStringHandler(108, 2);
						defaultInterpolatedStringHandler8.AppendLiteral("Cannot claim chunk (");
						defaultInterpolatedStringHandler8.AppendFormatted<int>(chunkX);
						defaultInterpolatedStringHandler8.AppendLiteral(", ");
						defaultInterpolatedStringHandler8.AppendFormatted<int>(chunkZ);
						defaultInterpolatedStringHandler8.AppendLiteral("). New claims must be adjacent to existing guild claims, or create an outpost instead.");
						this.SendNotification(player, defaultInterpolatedStringHandler8.ToStringAndClear(), NotificationType.Error);
						return;
					}
				}
			}
			string error;
			if (this.guildManager.ClaimLand(guild.Name, player.PlayerUID, packet.BlockX, packet.BlockZ, packet.IsOutpost, packet.OutpostName, out error))
			{
				if (needsGuildHome)
				{
					int currentClaims2 = this.guildManager.GetNonOutpostClaimCount(guild);
					int maxClaims3 = this.guildManager.GetMaxClaimsPerGuild(guild);
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler9 = new DefaultInterpolatedStringHandler(62, 4);
					defaultInterpolatedStringHandler9.AppendLiteral("Established guild home at chunk (");
					defaultInterpolatedStringHandler9.AppendFormatted<int>(chunkX);
					defaultInterpolatedStringHandler9.AppendLiteral(",");
					defaultInterpolatedStringHandler9.AppendFormatted<int>(chunkZ);
					defaultInterpolatedStringHandler9.AppendLiteral(") with a 2x2 area. Claims: ");
					defaultInterpolatedStringHandler9.AppendFormatted<int>(currentClaims2);
					defaultInterpolatedStringHandler9.AppendLiteral("/");
					defaultInterpolatedStringHandler9.AppendFormatted<int>(maxClaims3);
					this.SendNotification(player, defaultInterpolatedStringHandler9.ToStringAndClear(), NotificationType.Success);
				}
				else if (packet.IsOutpost)
				{
					int currentOutposts2 = this.guildManager.GetOutpostClaimCount(guild);
					int maxOutposts2 = this.guildManager.GetMaxOutpostsPerGuild(guild);
					string outpostNameText = string.IsNullOrEmpty(packet.OutpostName) ? "" : (" '" + packet.OutpostName + "'");
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler10 = new DefaultInterpolatedStringHandler(46, 5);
					defaultInterpolatedStringHandler10.AppendLiteral("Established outpost");
					defaultInterpolatedStringHandler10.AppendFormatted(outpostNameText);
					defaultInterpolatedStringHandler10.AppendLiteral(" at chunk (");
					defaultInterpolatedStringHandler10.AppendFormatted<int>(chunkX);
					defaultInterpolatedStringHandler10.AppendLiteral(", ");
					defaultInterpolatedStringHandler10.AppendFormatted<int>(chunkZ);
					defaultInterpolatedStringHandler10.AppendLiteral("). Outposts: ");
					defaultInterpolatedStringHandler10.AppendFormatted<int>(currentOutposts2);
					defaultInterpolatedStringHandler10.AppendLiteral("/");
					defaultInterpolatedStringHandler10.AppendFormatted<int>(maxOutposts2);
					this.SendNotification(player, defaultInterpolatedStringHandler10.ToStringAndClear(), NotificationType.Success);
				}
				else
				{
					int currentClaims3 = this.guildManager.GetNonOutpostClaimCount(guild);
					int maxClaims4 = this.guildManager.GetMaxClaimsPerGuild(guild);
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler11 = new DefaultInterpolatedStringHandler(42, 5);
					defaultInterpolatedStringHandler11.AppendLiteral("Claimed chunk (");
					defaultInterpolatedStringHandler11.AppendFormatted<int>(chunkX);
					defaultInterpolatedStringHandler11.AppendLiteral(", ");
					defaultInterpolatedStringHandler11.AppendFormatted<int>(chunkZ);
					defaultInterpolatedStringHandler11.AppendLiteral(") for guild '");
					defaultInterpolatedStringHandler11.AppendFormatted(guild.Name);
					defaultInterpolatedStringHandler11.AppendLiteral("'. Claims: ");
					defaultInterpolatedStringHandler11.AppendFormatted<int>(currentClaims3);
					defaultInterpolatedStringHandler11.AppendLiteral("/");
					defaultInterpolatedStringHandler11.AppendFormatted<int>(maxClaims4);
					this.SendNotification(player, defaultInterpolatedStringHandler11.ToStringAndClear(), NotificationType.Success);
				}
				this.BroadcastGuildSummariesToAll();
				return;
			}
			if (!needsGuildHome && !packet.IsOutpost && !GuildManager.IsChunkAdjacentToGuildClaims(guild, chunkX, chunkZ))
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler12 = new DefaultInterpolatedStringHandler(111, 2);
				defaultInterpolatedStringHandler12.AppendLiteral("Could not claim chunk (");
				defaultInterpolatedStringHandler12.AppendFormatted<int>(chunkX);
				defaultInterpolatedStringHandler12.AppendLiteral(", ");
				defaultInterpolatedStringHandler12.AppendFormatted<int>(chunkZ);
				defaultInterpolatedStringHandler12.AppendLiteral("). New claims must be adjacent to existing guild claims, or create an outpost instead.");
				this.SendNotification(player, defaultInterpolatedStringHandler12.ToStringAndClear(), NotificationType.Error);
				return;
			}
			if (error != null)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler13 = new DefaultInterpolatedStringHandler(36, 3);
				defaultInterpolatedStringHandler13.AppendLiteral("Could not claim land at chunk (");
				defaultInterpolatedStringHandler13.AppendFormatted<int>(chunkX);
				defaultInterpolatedStringHandler13.AppendLiteral(", ");
				defaultInterpolatedStringHandler13.AppendFormatted<int>(chunkZ);
				defaultInterpolatedStringHandler13.AppendLiteral("). ");
				defaultInterpolatedStringHandler13.AppendFormatted(error);
				this.SendNotification(player, defaultInterpolatedStringHandler13.ToStringAndClear(), NotificationType.Error);
				return;
			}
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler14 = new DefaultInterpolatedStringHandler(65, 2);
			defaultInterpolatedStringHandler14.AppendLiteral("Could not claim land at chunk (");
			defaultInterpolatedStringHandler14.AppendFormatted<int>(chunkX);
			defaultInterpolatedStringHandler14.AppendLiteral(", ");
			defaultInterpolatedStringHandler14.AppendFormatted<int>(chunkZ);
			defaultInterpolatedStringHandler14.AppendLiteral("). An unexpected error occurred.");
			this.SendNotification(player, defaultInterpolatedStringHandler14.ToStringAndClear(), NotificationType.Error);
		}

		// Token: 0x060001C2 RID: 450 RVA: 0x000125AC File Offset: 0x000107AC
		private void OnGuildUnclaimLandReceived(IServerPlayer player, GuildUnclaimLandPacket packet)
		{
			if (this.guildManager == null || this.serverApi == null)
			{
				return;
			}
			string playerUid = player.PlayerUID;
			Guild guild = this.guildManager.GetGuildByMember(playerUid);
			if (guild == null)
			{
				this.SendNotification(player, "You are not in a guild.", NotificationType.Error);
				return;
			}
			if (!GuildManager.HasPermission(guild, playerUid, GuildPermission.ManageRoles))
			{
				this.SendNotification(player, "You don't have permission to unclaim land.", NotificationType.Error);
				return;
			}
			int chunkX = packet.BlockX / 32;
			int chunkZ = packet.BlockZ / 32;
			ValueTuple<bool, string> result = this.guildManager.UnclaimLand(guild.Name, chunkX, chunkZ);
			if (result.Item1)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(34, 2);
				defaultInterpolatedStringHandler.AppendLiteral("Chunk (");
				defaultInterpolatedStringHandler.AppendFormatted<int>(chunkX);
				defaultInterpolatedStringHandler.AppendLiteral(", ");
				defaultInterpolatedStringHandler.AppendFormatted<int>(chunkZ);
				defaultInterpolatedStringHandler.AppendLiteral(") unclaimed successfully.");
				this.SendNotification(player, defaultInterpolatedStringHandler.ToStringAndClear(), NotificationType.Success);
				this.BroadcastGuildSummariesToAll();
				return;
			}
			this.SendNotification(player, result.Item2 ?? "Failed to unclaim land.", NotificationType.Error);
		}

		// Token: 0x060001C3 RID: 451 RVA: 0x000126A8 File Offset: 0x000108A8
		private void OnGuildRoleManagementReceived(IServerPlayer player, GuildRoleManagementPacket packet)
		{
			Guild guild = this.guildManager.GetGuildByMember(player.PlayerUID);
			if (guild == null)
			{
				this.SendNotification(player, "You are not in a guild.", NotificationType.Error);
				return;
			}
			bool success = false;
			string message = "";
			string action = packet.Action;
			string a = (action != null) ? action.ToLowerInvariant() : null;
			if (!(a == "create"))
			{
				if (!(a == "update"))
				{
					if (!(a == "remove"))
					{
						if (a == "assign")
						{
							string targetPlayerUid = this.FindGuildMemberByName(guild, packet.TargetPlayerName);
							if (targetPlayerUid != null)
							{
								if (!this.guildManager.CanManageRole(guild, player.PlayerUID, packet.RoleName))
								{
									int playerHierarchy = GuildManager.GetPlayerHierarchy(guild, player.PlayerUID);
									int roleHierarchy = GuildManager.GetRoleHierarchy(guild, packet.RoleName);
									DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(84, 3);
									defaultInterpolatedStringHandler.AppendLiteral("Cannot assign role '");
									defaultInterpolatedStringHandler.AppendFormatted(packet.RoleName);
									defaultInterpolatedStringHandler.AppendLiteral("'. Your hierarchy (");
									defaultInterpolatedStringHandler.AppendFormatted<int>(playerHierarchy);
									defaultInterpolatedStringHandler.AppendLiteral(") must be lower than the role's hierarchy (");
									defaultInterpolatedStringHandler.AppendFormatted<int>(roleHierarchy);
									defaultInterpolatedStringHandler.AppendLiteral(").");
									message = defaultInterpolatedStringHandler.ToStringAndClear();
								}
								else if (!this.guildManager.CanActOnPlayer(guild, player.PlayerUID, targetPlayerUid))
								{
									int playerHierarchy2 = GuildManager.GetPlayerHierarchy(guild, player.PlayerUID);
									int targetHierarchy = GuildManager.GetPlayerHierarchy(guild, targetPlayerUid);
									DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(88, 3);
									defaultInterpolatedStringHandler2.AppendLiteral("Cannot change role of ");
									defaultInterpolatedStringHandler2.AppendFormatted(packet.TargetPlayerName);
									defaultInterpolatedStringHandler2.AppendLiteral(". Your hierarchy (");
									defaultInterpolatedStringHandler2.AppendFormatted<int>(playerHierarchy2);
									defaultInterpolatedStringHandler2.AppendLiteral(") must be lower than their current hierarchy (");
									defaultInterpolatedStringHandler2.AppendFormatted<int>(targetHierarchy);
									defaultInterpolatedStringHandler2.AppendLiteral(").");
									message = defaultInterpolatedStringHandler2.ToStringAndClear();
								}
								else
								{
									success = this.guildManager.PromoteMember(guild.Name, player.PlayerUID, targetPlayerUid, packet.RoleName);
									string text;
									if (!success)
									{
										text = "Could not set role.";
									}
									else
									{
										DefaultInterpolatedStringHandler defaultInterpolatedStringHandler3 = new DefaultInterpolatedStringHandler(19, 2);
										defaultInterpolatedStringHandler3.AppendLiteral("Set role of ");
										defaultInterpolatedStringHandler3.AppendFormatted(packet.TargetPlayerName);
										defaultInterpolatedStringHandler3.AppendLiteral(" to '");
										defaultInterpolatedStringHandler3.AppendFormatted(packet.RoleName);
										defaultInterpolatedStringHandler3.AppendLiteral("'.");
										text = defaultInterpolatedStringHandler3.ToStringAndClear();
									}
									message = text;
								}
							}
							else
							{
								message = "Player '" + packet.TargetPlayerName + "' not found in guild.";
							}
						}
					}
					else if (packet.RoleName == "Leader" || packet.RoleName == "Member")
					{
						message = "Cannot remove default roles 'Leader' or 'Member'.";
					}
					else if (!GuildManager.HasPermission(guild, player.PlayerUID, GuildPermission.ManageRoles))
					{
						message = "You don't have permission to remove roles.";
					}
					else if (!this.guildManager.CanManageRole(guild, player.PlayerUID, packet.RoleName))
					{
						int playerHierarchy3 = GuildManager.GetPlayerHierarchy(guild, player.PlayerUID);
						int roleHierarchy2 = GuildManager.GetRoleHierarchy(guild, packet.RoleName);
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler4 = new DefaultInterpolatedStringHandler(84, 3);
						defaultInterpolatedStringHandler4.AppendLiteral("Cannot remove role '");
						defaultInterpolatedStringHandler4.AppendFormatted(packet.RoleName);
						defaultInterpolatedStringHandler4.AppendLiteral("'. Your hierarchy (");
						defaultInterpolatedStringHandler4.AppendFormatted<int>(playerHierarchy3);
						defaultInterpolatedStringHandler4.AppendLiteral(") must be lower than the role's hierarchy (");
						defaultInterpolatedStringHandler4.AppendFormatted<int>(roleHierarchy2);
						defaultInterpolatedStringHandler4.AppendLiteral(").");
						message = defaultInterpolatedStringHandler4.ToStringAndClear();
					}
					else if (!guild.Roles.ContainsKey(packet.RoleName))
					{
						message = "Role '" + packet.RoleName + "' does not exist.";
					}
					else
					{
						foreach (GuildMember member in guild.Members.Values)
						{
							if (member.Role == packet.RoleName)
							{
								member.Role = "Member";
							}
						}
						guild.Roles.Remove(packet.RoleName);
						ICoreServerAPI coreServerAPI = this.serverApi;
						object obj;
						if (coreServerAPI == null)
						{
							obj = null;
						}
						else
						{
							SRGuildsAndKingdomsModSystem srguildsAndKingdomsModSystem = coreServerAPI.ModLoader.GetModSystem<SRGuildsAndKingdomsModSystem>(true);
							obj = ((srguildsAndKingdomsModSystem != null) ? srguildsAndKingdomsModSystem.GetGuildRepository() : null);
						}
						object obj2 = obj;
						if (obj2 != null)
						{
							obj2.MarkDirty(guild.Name);
						}
						success = true;
						message = "Removed role '" + packet.RoleName + "' and reassigned affected members to 'Member'.";
					}
				}
				else
				{
					GuildPermission newPermissions = this.ParsePermissionString(packet.PermissionString);
					if (!this.guildManager.CanManageRole(guild, player.PlayerUID, packet.RoleName))
					{
						int playerHierarchy4 = GuildManager.GetPlayerHierarchy(guild, player.PlayerUID);
						int roleHierarchy3 = GuildManager.GetRoleHierarchy(guild, packet.RoleName);
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler5 = new DefaultInterpolatedStringHandler(84, 3);
						defaultInterpolatedStringHandler5.AppendLiteral("Cannot update role '");
						defaultInterpolatedStringHandler5.AppendFormatted(packet.RoleName);
						defaultInterpolatedStringHandler5.AppendLiteral("'. Your hierarchy (");
						defaultInterpolatedStringHandler5.AppendFormatted<int>(playerHierarchy4);
						defaultInterpolatedStringHandler5.AppendLiteral(") must be lower than the role's hierarchy (");
						defaultInterpolatedStringHandler5.AppendFormatted<int>(roleHierarchy3);
						defaultInterpolatedStringHandler5.AppendLiteral(").");
						message = defaultInterpolatedStringHandler5.ToStringAndClear();
					}
					else if (packet.Hierarchy != 999 && packet.Hierarchy != GuildManager.GetRoleHierarchy(guild, packet.RoleName))
					{
						int playerHierarchy5 = GuildManager.GetPlayerHierarchy(guild, player.PlayerUID);
						if (packet.Hierarchy <= playerHierarchy5)
						{
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler6 = new DefaultInterpolatedStringHandler(76, 2);
							defaultInterpolatedStringHandler6.AppendLiteral("Cannot set hierarchy to ");
							defaultInterpolatedStringHandler6.AppendFormatted<int>(packet.Hierarchy);
							defaultInterpolatedStringHandler6.AppendLiteral(". It must be greater than your current hierarchy (");
							defaultInterpolatedStringHandler6.AppendFormatted<int>(playerHierarchy5);
							defaultInterpolatedStringHandler6.AppendLiteral(").");
							message = defaultInterpolatedStringHandler6.ToStringAndClear();
						}
						else
						{
							success = this.guildManager.UpdateRolePermissions(guild.Name, player.PlayerUID, packet.RoleName, newPermissions, packet.Hierarchy);
							message = (success ? ("Updated role '" + packet.RoleName + "' permissions and hierarchy.") : "Could not update role.");
						}
					}
					else
					{
						success = this.guildManager.UpdateRolePermissions(guild.Name, player.PlayerUID, packet.RoleName, newPermissions);
						message = (success ? ("Updated role '" + packet.RoleName + "' permissions.") : "Could not update role permissions.");
					}
				}
			}
			else
			{
				GuildPermission permissions = this.ParsePermissionString(packet.PermissionString);
				success = this.guildManager.CreateRole(guild.Name, player.PlayerUID, packet.RoleName, "", permissions, packet.Hierarchy);
				if (success)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler7 = new DefaultInterpolatedStringHandler(32, 2);
					defaultInterpolatedStringHandler7.AppendLiteral("Role '");
					defaultInterpolatedStringHandler7.AppendFormatted(packet.RoleName);
					defaultInterpolatedStringHandler7.AppendLiteral("' created with hierarchy ");
					defaultInterpolatedStringHandler7.AppendFormatted<int>(packet.Hierarchy);
					defaultInterpolatedStringHandler7.AppendLiteral(".");
					message = defaultInterpolatedStringHandler7.ToStringAndClear();
				}
				else
				{
					int playerHierarchy6 = GuildManager.GetPlayerHierarchy(guild, player.PlayerUID);
					if (packet.Hierarchy <= playerHierarchy6)
					{
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler8 = new DefaultInterpolatedStringHandler(86, 2);
						defaultInterpolatedStringHandler8.AppendLiteral("Cannot create role with hierarchy ");
						defaultInterpolatedStringHandler8.AppendFormatted<int>(packet.Hierarchy);
						defaultInterpolatedStringHandler8.AppendLiteral(". It must be greater than your current hierarchy (");
						defaultInterpolatedStringHandler8.AppendFormatted<int>(playerHierarchy6);
						defaultInterpolatedStringHandler8.AppendLiteral(").");
						message = defaultInterpolatedStringHandler8.ToStringAndClear();
					}
					else
					{
						message = "Could not create role.";
					}
				}
			}
			this.SendNotification(player, message, success ? NotificationType.Success : NotificationType.Error);
			if (success)
			{
				this.BroadcastGuildSummariesToAll();
			}
		}

		// Token: 0x060001C4 RID: 452 RVA: 0x00012DE0 File Offset: 0x00010FE0
		private void OnTechContributionRequestReceived(IServerPlayer player, TechContributionRequestPacket packet)
		{
			GuildManager guildManager = this.guildManager;
			Guild guild = (guildManager != null) ? guildManager.GetGuild(packet.GuildName) : null;
			if (guild == null)
			{
				this.SendTechContributionResponse(player, false, "Guild not found.", packet.TechBlockId, false, new Dictionary<string, int>());
				return;
			}
			if (!guild.Members.ContainsKey(player.PlayerUID))
			{
				this.SendTechContributionResponse(player, false, "You are not a member of this guild.", packet.TechBlockId, false, new Dictionary<string, int>());
				return;
			}
			EntityPlayer entity = player.Entity;
			EntityPos playerPos = (entity != null) ? entity.ServerPos : null;
			if (playerPos != null)
			{
				int playerBlockX = (int)playerPos.X;
				int playerBlockZ = (int)playerPos.Z;
				if (!this.guildManager.IsPlayerInGuildClaim(guild, playerBlockX, playerBlockZ))
				{
					this.SendTechContributionResponse(player, false, "You must be within guild claimed land to contribute to research.", packet.TechBlockId, false, new Dictionary<string, int>());
					return;
				}
			}
			SRGuildsAndKingdomsModSystem modSystem = this.serverApi.ModLoader.GetModSystem<SRGuildsAndKingdomsModSystem>(true);
			if (modSystem == null)
			{
				this.SendTechContributionResponse(player, false, "System error: mod system not found.", packet.TechBlockId, false, new Dictionary<string, int>());
				return;
			}
			TechBlock techBlock = modSystem.TechBlocks.FirstOrDefault((TechBlock t) => t.Id == packet.TechBlockId);
			if (techBlock == null)
			{
				this.SendTechContributionResponse(player, false, "Tech block not found.", packet.TechBlockId, false, new Dictionary<string, int>());
				return;
			}
			GuildTechProgress progress = guild.GetOrCreateTechProgress(packet.TechBlockId);
			if (progress.IsUnlocked)
			{
				this.SendTechContributionResponse(player, false, "This technology is already unlocked.", packet.TechBlockId, false, new Dictionary<string, int>());
				return;
			}
			GuildTechManager guildTechManager = modSystem.GuildTechManager;
			Dictionary<string, int> baseRequirements = new Dictionary<string, int>();
			foreach (ResourceGroup rg2 in techBlock.ResourceGroups)
			{
				baseRequirements[rg2.Name] = rg2.AmountRequired;
			}
			Dictionary<string, int> scaledRequirements = ((guildTechManager != null) ? guildTechManager.GetScaledRequirements(guild.Name, baseRequirements) : null) ?? baseRequirements;
			int totalItemsContributed = 0;
			EntityPlayer playerEntity = player.Entity;
			if (playerEntity == null)
			{
				this.SendTechContributionResponse(player, false, "Player entity not found.", packet.TechBlockId, false, new Dictionary<string, int>());
				return;
			}
			using (List<ContributionItemDto>.Enumerator enumerator2 = packet.Items.GetEnumerator())
			{
				while (enumerator2.MoveNext())
				{
					ContributionItemDto item = enumerator2.Current;
					IInventory inventory = null;
					foreach (IInventory inv in playerEntity.Player.InventoryManager.Inventories.Values)
					{
						if (inv.InventoryID == item.InventoryId)
						{
							inventory = inv;
							break;
						}
					}
					if (inventory != null && item.SlotId >= 0 && item.SlotId < inventory.Count)
					{
						ItemSlot slot = inventory[item.SlotId];
						if (!slot.Empty)
						{
							ItemStack itemStack = slot.Itemstack;
							string itemCode = itemStack.Collectible.Code.ToString();
							if (!(itemCode != item.ItemCode))
							{
								ResourceGroup resourceGroup = techBlock.ResourceGroups.FirstOrDefault((ResourceGroup rg) => rg.Name == item.ResourceGroupName);
								if (resourceGroup != null && resourceGroup.DoesItemMatch(itemCode))
								{
									int currentSubmitted = progress.GetResourceGroupSubmitted(resourceGroup.Name);
									int remaining = (scaledRequirements.ContainsKey(resourceGroup.Name) ? scaledRequirements[resourceGroup.Name] : resourceGroup.AmountRequired) - currentSubmitted;
									if (remaining > 0)
									{
										int amountToTake = Math.Min(Math.Min(item.Amount, itemStack.StackSize), remaining);
										if (amountToTake > 0)
										{
											itemStack.StackSize -= amountToTake;
											totalItemsContributed += amountToTake;
											if (progress.ResourceGroupsSubmitted.ContainsKey(resourceGroup.Name))
											{
												Dictionary<string, int> resourceGroupsSubmitted = progress.ResourceGroupsSubmitted;
												string name = resourceGroup.Name;
												resourceGroupsSubmitted[name] += amountToTake;
											}
											else
											{
												progress.ResourceGroupsSubmitted[resourceGroup.Name] = amountToTake;
											}
											if (itemStack.StackSize <= 0)
											{
												slot.Itemstack = null;
											}
											slot.MarkDirty();
										}
									}
								}
							}
						}
					}
				}
			}
			if (totalItemsContributed == 0)
			{
				this.SendTechContributionResponse(player, false, "No valid items were contributed.", packet.TechBlockId, false, new Dictionary<string, int>());
				return;
			}
			bool isComplete = true;
			foreach (ResourceGroup resourceGroup2 in techBlock.ResourceGroups)
			{
				int resourceGroupSubmitted = progress.GetResourceGroupSubmitted(resourceGroup2.Name);
				int scaledRequired = scaledRequirements.ContainsKey(resourceGroup2.Name) ? scaledRequirements[resourceGroup2.Name] : resourceGroup2.AmountRequired;
				if (resourceGroupSubmitted < scaledRequired)
				{
					isComplete = false;
					break;
				}
			}
			string message;
			if (isComplete && !progress.IsUnlocked)
			{
				modSystem.GuildTechManager.UnlockTech(guild.Name, techBlock.Id, techBlock);
				message = "Research complete! " + techBlock.Text + " unlocked!";
				this.guildManager.SyncGuildMemberTraits(guild);
				ILogger logger = this.serverApi.Logger;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(74, 3);
				defaultInterpolatedStringHandler.AppendLiteral("[GuildNetworkHandler] Tech ");
				defaultInterpolatedStringHandler.AppendFormatted<int>(techBlock.Id);
				defaultInterpolatedStringHandler.AppendLiteral(" (");
				defaultInterpolatedStringHandler.AppendFormatted(techBlock.Text);
				defaultInterpolatedStringHandler.AppendLiteral(") unlocked for guild ");
				defaultInterpolatedStringHandler.AppendFormatted(guild.Name);
				defaultInterpolatedStringHandler.AppendLiteral(" - syncing member traits");
				logger.Notification(defaultInterpolatedStringHandler.ToStringAndClear());
				bool requiresPersonal;
				if (guild.TechRequiresPersonalUnlock.TryGetValue(techBlock.Id, out requiresPersonal) && requiresPersonal)
				{
					techBlock.GetPersonalRequirements().Values.Sum();
					message += " (Personal unlock required: each member must contribute 5% of resources)";
				}
			}
			else
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(31, 1);
				defaultInterpolatedStringHandler2.AppendLiteral("Contributed ");
				defaultInterpolatedStringHandler2.AppendFormatted<int>(totalItemsContributed);
				defaultInterpolatedStringHandler2.AppendLiteral(" items to research.");
				message = defaultInterpolatedStringHandler2.ToStringAndClear();
			}
			this.BroadcastGuildSummariesToAll();
			this.SendTechContributionResponse(player, true, message, packet.TechBlockId, isComplete, progress.ResourceGroupsSubmitted);
		}

		// Token: 0x060001C5 RID: 453 RVA: 0x000134C0 File Offset: 0x000116C0
		private void SendTechContributionResponse(IServerPlayer player, bool success, string message, int techBlockId, bool techUnlocked, Dictionary<string, int> updatedProgress)
		{
			TechContributionResponsePacket response = new TechContributionResponsePacket
			{
				PlayerUid = player.PlayerUID,
				Success = success,
				Message = message,
				TechBlockId = techBlockId,
				TechUnlocked = techUnlocked,
				UpdatedProgress = updatedProgress
			};
			this.serverApi.Network.GetChannel("srguildsandkingdoms:guild").SendPacket<TechContributionResponsePacket>(response, new IServerPlayer[]
			{
				player
			});
		}

		// Token: 0x060001C6 RID: 454 RVA: 0x0001352A File Offset: 0x0001172A
		private void OnGuildSyncReceived(GuildSyncPacket packet)
		{
			Action<List<GuildSummary>> action = this.onGuildSummariesReceived;
			if (action == null)
			{
				return;
			}
			action(packet.GuildSummaries);
		}

		// Token: 0x060001C7 RID: 455 RVA: 0x00013542 File Offset: 0x00011742
		private void OnGuildUpdateReceived(GuildUpdatePacket packet)
		{
			Action<List<GuildSummary>> action = this.onGuildSummariesReceived;
			if (action == null)
			{
				return;
			}
			action(new List<GuildSummary>
			{
				packet.UpdatedGuild
			});
		}

		// Token: 0x060001C8 RID: 456 RVA: 0x00013565 File Offset: 0x00011765
		private void OnGuildNotificationReceived(GuildNotificationPacket packet)
		{
			Action<string, NotificationType> action = this.onNotificationReceived;
			if (action == null)
			{
				return;
			}
			action(packet.Message, packet.Type);
		}

		// Token: 0x060001C9 RID: 457 RVA: 0x00013584 File Offset: 0x00011784
		private void OnGuildInviteNotificationReceived(GuildInviteNotificationPacket packet)
		{
			ICoreClientAPI coreClientAPI = this.clientApi;
			if (coreClientAPI != null)
			{
				coreClientAPI.Logger.Notification("Received guild invite from " + packet.InviterName + " to join " + packet.GuildName);
			}
			ICoreClientAPI coreClientAPI2 = this.clientApi;
			SRGuildsAndKingdomsModSystem srguildsAndKingdomsModSystem = (coreClientAPI2 != null) ? coreClientAPI2.ModLoader.GetModSystem<SRGuildsAndKingdomsModSystem>(true) : null;
			if (srguildsAndKingdomsModSystem != null)
			{
				srguildsAndKingdomsModSystem.ShowInvitePopup(packet);
			}
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(74, 2);
			defaultInterpolatedStringHandler.AppendLiteral("You have been invited to join '");
			defaultInterpolatedStringHandler.AppendFormatted(packet.GuildName);
			defaultInterpolatedStringHandler.AppendLiteral("' by ");
			defaultInterpolatedStringHandler.AppendFormatted(packet.InviterName);
			defaultInterpolatedStringHandler.AppendLiteral(". Check bottom-right for invite popup.");
			string message = defaultInterpolatedStringHandler.ToStringAndClear();
			Action<string, NotificationType> action = this.onNotificationReceived;
			if (action == null)
			{
				return;
			}
			action(message, NotificationType.Info);
		}

		// Token: 0x060001CA RID: 458 RVA: 0x00013648 File Offset: 0x00011848
		private void OnGuildInviteListResponseReceived(GuildInviteListResponsePacket packet)
		{
			ICoreClientAPI coreClientAPI = this.clientApi;
			if (coreClientAPI != null)
			{
				ILogger logger = coreClientAPI.Logger;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(34, 1);
				defaultInterpolatedStringHandler.AppendLiteral("Received invite list with ");
				defaultInterpolatedStringHandler.AppendFormatted<int>(packet.Invites.Count);
				defaultInterpolatedStringHandler.AppendLiteral(" invites");
				logger.Notification(defaultInterpolatedStringHandler.ToStringAndClear());
			}
			ICoreClientAPI coreClientAPI2 = this.clientApi;
			SRGuildsAndKingdomsModSystem srguildsAndKingdomsModSystem = (coreClientAPI2 != null) ? coreClientAPI2.ModLoader.GetModSystem<SRGuildsAndKingdomsModSystem>(true) : null;
			if (srguildsAndKingdomsModSystem == null)
			{
				return;
			}
			srguildsAndKingdomsModSystem.ShowInviteListPopup(packet.Invites);
		}

		// Token: 0x060001CB RID: 459 RVA: 0x000136CE File Offset: 0x000118CE
		private void OnGuildMemberListReceived(GuildMemberListPacket packet)
		{
			Action<List<GuildMemberInfo>> action = this.onMemberListReceived;
			if (action == null)
			{
				return;
			}
			action(packet.Members);
		}

		// Token: 0x060001CC RID: 460 RVA: 0x000136E6 File Offset: 0x000118E6
		private void OnTechContributionResponseReceived(TechContributionResponsePacket packet)
		{
			Action<TechContributionResponsePacket> action = this.onTechContributionResponseReceived;
			if (action == null)
			{
				return;
			}
			action(packet);
		}

		// Token: 0x060001CD RID: 461 RVA: 0x000136FC File Offset: 0x000118FC
		public void BroadcastGuildSummaries(IServerPlayer player)
		{
			List<GuildSummary> summaries = this.guildManager.GetGuildSummariesForPlayer(player.PlayerUID);
			GuildSyncPacket packet = new GuildSyncPacket
			{
				PlayerUid = player.PlayerUID,
				GuildSummaries = summaries
			};
			this.serverApi.Network.GetChannel("srguildsandkingdoms:guild").SendPacket<GuildSyncPacket>(packet, new IServerPlayer[]
			{
				player
			});
		}

		// Token: 0x060001CE RID: 462 RVA: 0x0001375C File Offset: 0x0001195C
		public void BroadcastGuildSummariesToAll()
		{
			IPlayer[] allOnlinePlayers = this.serverApi.World.AllOnlinePlayers;
			for (int i = 0; i < allOnlinePlayers.Length; i++)
			{
				IServerPlayer serverPlayer = allOnlinePlayers[i] as IServerPlayer;
				if (serverPlayer != null)
				{
					this.BroadcastGuildSummaries(serverPlayer);
				}
			}
		}

		// Token: 0x060001CF RID: 463 RVA: 0x0001379C File Offset: 0x0001199C
		public void SendNotification(IServerPlayer player, string message, NotificationType type)
		{
			GuildNotificationPacket packet = new GuildNotificationPacket
			{
				PlayerUid = player.PlayerUID,
				Message = message,
				Type = type
			};
			this.serverApi.Network.GetChannel("srguildsandkingdoms:guild").SendPacket<GuildNotificationPacket>(packet, new IServerPlayer[]
			{
				player
			});
		}

		// Token: 0x060001D0 RID: 464 RVA: 0x000137F0 File Offset: 0x000119F0
		public void SendGuildCreateRequest(string guildName, string description = "")
		{
			if (this.clientApi == null)
			{
				return;
			}
			GuildCreatePacket packet = new GuildCreatePacket
			{
				PlayerUid = this.clientApi.World.Player.PlayerUID,
				GuildName = guildName,
				Description = description
			};
			this.clientApi.Network.GetChannel("srguildsandkingdoms:guild").SendPacket<GuildCreatePacket>(packet);
		}

		// Token: 0x060001D1 RID: 465 RVA: 0x00013850 File Offset: 0x00011A50
		public void SendGuildInviteRequest(string targetPlayerUid)
		{
			if (this.clientApi == null)
			{
				return;
			}
			GuildInvitePacket packet = new GuildInvitePacket
			{
				PlayerUid = this.clientApi.World.Player.PlayerUID,
				TargetPlayerUid = targetPlayerUid
			};
			this.clientApi.Network.GetChannel("srguildsandkingdoms:guild").SendPacket<GuildInvitePacket>(packet);
		}

		// Token: 0x060001D2 RID: 466 RVA: 0x000138AC File Offset: 0x00011AAC
		public void SendGuildAcceptRequest()
		{
			if (this.clientApi == null)
			{
				return;
			}
			GuildAcceptInvitePacket packet = new GuildAcceptInvitePacket
			{
				PlayerUid = this.clientApi.World.Player.PlayerUID
			};
			this.clientApi.Network.GetChannel("srguildsandkingdoms:guild").SendPacket<GuildAcceptInvitePacket>(packet);
		}

		// Token: 0x060001D3 RID: 467 RVA: 0x00013900 File Offset: 0x00011B00
		public void SendCancelInviteRequest(string inviteeUid)
		{
			if (this.clientApi == null)
			{
				return;
			}
			GuildCancelInvitePacket packet = new GuildCancelInvitePacket
			{
				PlayerUid = this.clientApi.World.Player.PlayerUID,
				InviteeUid = inviteeUid
			};
			this.clientApi.Network.GetChannel("srguildsandkingdoms:guild").SendPacket<GuildCancelInvitePacket>(packet);
		}

		// Token: 0x060001D4 RID: 468 RVA: 0x0001395C File Offset: 0x00011B5C
		public void SendGuildRemoveMemberRequest(string targetPlayerName)
		{
			if (this.clientApi == null)
			{
				return;
			}
			GuildRemoveMemberPacket packet = new GuildRemoveMemberPacket
			{
				PlayerUid = this.clientApi.World.Player.PlayerUID,
				TargetPlayerName = targetPlayerName
			};
			this.clientApi.Network.GetChannel("srguildsandkingdoms:guild").SendPacket<GuildRemoveMemberPacket>(packet);
		}

		// Token: 0x060001D5 RID: 469 RVA: 0x000139B8 File Offset: 0x00011BB8
		public void SendGuildLeaveRequest()
		{
			if (this.clientApi == null)
			{
				return;
			}
			GuildLeavePacket packet = new GuildLeavePacket
			{
				PlayerUid = this.clientApi.World.Player.PlayerUID
			};
			this.clientApi.Network.GetChannel("srguildsandkingdoms:guild").SendPacket<GuildLeavePacket>(packet);
		}

		// Token: 0x060001D6 RID: 470 RVA: 0x00013A0A File Offset: 0x00011C0A
		public void SendGuildClaimLandRequest(int chunkX, int chunkZ)
		{
			this.SendGuildClaimLandRequest(chunkX, chunkZ, false, "");
		}

		// Token: 0x060001D7 RID: 471 RVA: 0x00013A1C File Offset: 0x00011C1C
		public void SendGuildClaimLandRequest(int chunkX, int chunkZ, bool isOutpost, string outpostName = "")
		{
			if (this.clientApi == null)
			{
				return;
			}
			int blockX = chunkX * 32 + 16;
			int blockZ = chunkZ * 32 + 16;
			GuildClaimLandPacket packet = new GuildClaimLandPacket
			{
				PlayerUid = this.clientApi.World.Player.PlayerUID,
				BlockX = blockX,
				BlockZ = blockZ,
				IsOutpost = isOutpost,
				OutpostName = (outpostName ?? "")
			};
			this.clientApi.Network.GetChannel("srguildsandkingdoms:guild").SendPacket<GuildClaimLandPacket>(packet);
		}

		// Token: 0x060001D8 RID: 472 RVA: 0x00013AA4 File Offset: 0x00011CA4
		public void SendGuildUnclaimLandRequest(int chunkX, int chunkZ)
		{
			if (this.clientApi == null)
			{
				return;
			}
			int blockX = chunkX * 32 + 16;
			int blockZ = chunkZ * 32 + 16;
			GuildUnclaimLandPacket packet = new GuildUnclaimLandPacket
			{
				PlayerUid = this.clientApi.World.Player.PlayerUID,
				BlockX = blockX,
				BlockZ = blockZ
			};
			this.clientApi.Network.GetChannel("srguildsandkingdoms:guild").SendPacket<GuildUnclaimLandPacket>(packet);
		}

		// Token: 0x060001D9 RID: 473 RVA: 0x00013B14 File Offset: 0x00011D14
		public void SendGuildRoleManagementRequest(string action, string roleName, [Nullable(2)] string targetPlayerName = null, [Nullable(2)] string permissionString = null, int hierarchy = 999)
		{
			if (this.clientApi == null)
			{
				return;
			}
			GuildRoleManagementPacket packet = new GuildRoleManagementPacket
			{
				PlayerUid = this.clientApi.World.Player.PlayerUID,
				Action = action,
				RoleName = roleName,
				TargetPlayerName = (targetPlayerName ?? ""),
				PermissionString = (permissionString ?? ""),
				Hierarchy = hierarchy
			};
			this.clientApi.Network.GetChannel("srguildsandkingdoms:guild").SendPacket<GuildRoleManagementPacket>(packet);
		}

		// Token: 0x060001DA RID: 474 RVA: 0x00013BA0 File Offset: 0x00011DA0
		public void SendGuildMemberListRequest()
		{
			if (this.clientApi == null)
			{
				return;
			}
			GuildMemberListRequestPacket packet = new GuildMemberListRequestPacket
			{
				PlayerUid = this.clientApi.World.Player.PlayerUID
			};
			this.clientApi.Network.GetChannel("srguildsandkingdoms:guild").SendPacket<GuildMemberListRequestPacket>(packet);
		}

		// Token: 0x060001DB RID: 475 RVA: 0x00013BF4 File Offset: 0x00011DF4
		public void SendGuildTransferOwnershipRequest(string targetPlayerUid)
		{
			if (this.clientApi == null)
			{
				return;
			}
			GuildTransferOwnershipPacket packet = new GuildTransferOwnershipPacket
			{
				PlayerUid = this.clientApi.World.Player.PlayerUID,
				TargetPlayerUid = targetPlayerUid
			};
			this.clientApi.Network.GetChannel("srguildsandkingdoms:guild").SendPacket<GuildTransferOwnershipPacket>(packet);
		}

		// Token: 0x060001DC RID: 476 RVA: 0x00013C4D File Offset: 0x00011E4D
		public void RegisterMemberListCallback(Action<List<GuildMemberInfo>> callback)
		{
			this.onMemberListReceived = callback;
		}

		// Token: 0x060001DD RID: 477 RVA: 0x00013C56 File Offset: 0x00011E56
		public void UnregisterMemberListCallback()
		{
			this.onMemberListReceived = null;
		}

		// Token: 0x060001DE RID: 478 RVA: 0x00013C60 File Offset: 0x00011E60
		public void SendTechContributionRequest(string guildName, int techBlockId, List<ContributionItemDto> items)
		{
			if (this.clientApi == null)
			{
				return;
			}
			TechContributionRequestPacket packet = new TechContributionRequestPacket
			{
				PlayerUid = this.clientApi.World.Player.PlayerUID,
				GuildName = guildName,
				TechBlockId = techBlockId,
				Items = items
			};
			this.clientApi.Network.GetChannel("srguildsandkingdoms:guild").SendPacket<TechContributionRequestPacket>(packet);
		}

		// Token: 0x060001DF RID: 479 RVA: 0x00013CC8 File Offset: 0x00011EC8
		public void SendPersonalTechContributionRequest(string guildName, int techBlockId, List<ContributionItemDto> items)
		{
			if (this.clientApi == null)
			{
				return;
			}
			PersonalTechContributionRequestPacket packet = new PersonalTechContributionRequestPacket
			{
				PlayerUid = this.clientApi.World.Player.PlayerUID,
				GuildName = guildName,
				TechBlockId = techBlockId,
				Items = items
			};
			this.clientApi.Network.GetChannel("srguildsandkingdoms:guild").SendPacket<PersonalTechContributionRequestPacket>(packet);
		}

		// Token: 0x060001E0 RID: 480 RVA: 0x00013D2F File Offset: 0x00011F2F
		public void RegisterTechContributionCallback(Action<TechContributionResponsePacket> callback)
		{
			this.onTechContributionResponseReceived = callback;
		}

		// Token: 0x060001E1 RID: 481 RVA: 0x00013D38 File Offset: 0x00011F38
		public void UnregisterTechContributionCallback()
		{
			this.onTechContributionResponseReceived = null;
		}

		// Token: 0x060001E2 RID: 482 RVA: 0x00013D44 File Offset: 0x00011F44
		[return: Nullable(2)]
		private string FindGuildMemberByName(Guild guild, string playerName)
		{
			if (guild == null || string.IsNullOrWhiteSpace(playerName))
			{
				return null;
			}
			IPlayer onlinePlayer = this.serverApi.World.AllOnlinePlayers.FirstOrDefault((IPlayer p) => p.PlayerName.Equals(playerName, StringComparison.OrdinalIgnoreCase));
			if (onlinePlayer != null && guild.Members.ContainsKey(onlinePlayer.PlayerUID))
			{
				return onlinePlayer.PlayerUID;
			}
			foreach (string memberUid in guild.Members.Keys)
			{
				IServerPlayerData playerDataByUid = this.serverApi.PlayerData.GetPlayerDataByUid(memberUid);
				bool flag;
				if (playerDataByUid == null)
				{
					flag = false;
				}
				else
				{
					string lastKnownPlayername = playerDataByUid.LastKnownPlayername;
					flag = ((lastKnownPlayername != null) ? new bool?(lastKnownPlayername.Equals(playerName, StringComparison.OrdinalIgnoreCase)) : null).GetValueOrDefault();
				}
				if (flag)
				{
					return memberUid;
				}
			}
			return null;
		}

		// Token: 0x060001E3 RID: 483 RVA: 0x00013E48 File Offset: 0x00012048
		private GuildPermission ParsePermissionString(string perms)
		{
			if (string.IsNullOrWhiteSpace(perms))
			{
				return GuildPermission.None;
			}
			GuildPermission result = GuildPermission.None;
			string[] array = perms.Split(new char[]
			{
				',',
				';',
				' '
			}, StringSplitOptions.RemoveEmptyEntries);
			for (int i = 0; i < array.Length; i++)
			{
				string tok = array[i].Trim().ToLowerInvariant();
				if (tok != null)
				{
					switch (tok.Length)
					{
					case 4:
						if (!(tok == "kick"))
						{
							goto IL_15B;
						}
						result |= GuildPermission.Kick;
						goto IL_15B;
					case 5:
					case 9:
					case 12:
					case 13:
					case 15:
						goto IL_15B;
					case 6:
					{
						char c = tok[0];
						if (c != 'i')
						{
							if (c != 'm')
							{
								goto IL_15B;
							}
							if (!(tok == "manage"))
							{
								goto IL_15B;
							}
						}
						else
						{
							if (!(tok == "invite"))
							{
								goto IL_15B;
							}
							result |= GuildPermission.Invite;
							goto IL_15B;
						}
						break;
					}
					case 7:
						if (!(tok == "promote"))
						{
							goto IL_15B;
						}
						result |= GuildPermission.Promote;
						goto IL_15B;
					case 8:
						if (!(tok == "interact"))
						{
							goto IL_15B;
						}
						goto IL_156;
					case 10:
						if (!(tok == "managerole"))
						{
							goto IL_15B;
						}
						break;
					case 11:
						if (!(tok == "manageroles"))
						{
							goto IL_15B;
						}
						break;
					case 14:
						if (!(tok == "interactblocks"))
						{
							goto IL_15B;
						}
						goto IL_156;
					case 16:
						if (!(tok == "breakplaceblocks"))
						{
							goto IL_15B;
						}
						result |= GuildPermission.BreakAndPlaceBlocks;
						goto IL_15B;
					default:
						goto IL_15B;
					}
					result |= GuildPermission.ManageRoles;
					goto IL_15B;
					IL_156:
					result |= GuildPermission.InteractBlocks;
				}
				IL_15B:;
			}
			return result;
		}

		// Token: 0x060001E4 RID: 484 RVA: 0x00013FC0 File Offset: 0x000121C0
		[return: TupleElementNames(new string[]
		{
			"tooClose",
			"nearestGuildName",
			"distance"
		})]
		[return: Nullable(new byte[]
		{
			0,
			1
		})]
		private ValueTuple<bool, string, double> IsChunkTooCloseToOtherGuildClaim(int chunkX, int chunkZ, string currentGuildName, Dictionary<string, Guild> allGuilds)
		{
			int centerBlockX = chunkX * 32 + 16;
			int centerBlockZ = chunkZ * 32 + 16;
			double nearestDistance = double.MaxValue;
			string nearestGuild = null;
			foreach (Guild otherGuild in allGuilds.Values)
			{
				if (!(otherGuild.Name == currentGuildName))
				{
					foreach (LandClaim claim in otherGuild.Claims)
					{
						List<ValueTuple<int, int>> claimChunks = new List<ValueTuple<int, int>>();
						GuildHomeClaim homeClaim = claim as GuildHomeClaim;
						if (homeClaim != null)
						{
							for (int dx = 0; dx <= 1; dx++)
							{
								for (int dz = 0; dz <= 1; dz++)
								{
									claimChunks.Add(new ValueTuple<int, int>(homeClaim.ChunkX + dx, homeClaim.ChunkZ + dz));
								}
							}
						}
						else
						{
							OutpostClaim outpostClaim = claim as OutpostClaim;
							if (outpostClaim != null)
							{
								claimChunks.Add(new ValueTuple<int, int>(outpostClaim.ChunkX, outpostClaim.ChunkZ));
							}
							else
							{
								claimChunks.Add(new ValueTuple<int, int>(claim.ChunkX, claim.ChunkZ));
							}
						}
						foreach (ValueTuple<int, int> valueTuple in claimChunks)
						{
							int claimChunkX = valueTuple.Item1;
							int item = valueTuple.Item2;
							int claimCenterX = claimChunkX * 32 + 16;
							int claimCenterZ = item * 32 + 16;
							double num = (double)(centerBlockX - claimCenterX);
							double deltaZ = (double)(centerBlockZ - claimCenterZ);
							double distance = Math.Sqrt(num * num + deltaZ * deltaZ);
							if (distance < nearestDistance)
							{
								nearestDistance = distance;
								nearestGuild = otherGuild.Name;
							}
							if (distance < 300.0)
							{
								return new ValueTuple<bool, string, double>(true, otherGuild.Name, distance);
							}
						}
					}
				}
			}
			return new ValueTuple<bool, string, double>(false, nearestGuild, nearestDistance);
		}

		// Token: 0x060001E5 RID: 485 RVA: 0x000141F0 File Offset: 0x000123F0
		public void SendGuildConfig(IServerPlayer player)
		{
			GuildManager guildManager = this.guildManager;
			GuildConfig guildConfig;
			if (guildManager == null)
			{
				guildConfig = null;
			}
			else
			{
				GuildConfigManager configManager = guildManager.GetConfigManager();
				guildConfig = ((configManager != null) ? configManager.GetConfig() : null);
			}
			GuildConfig config = guildConfig;
			if (config == null)
			{
				ICoreServerAPI coreServerAPI = this.serverApi;
				if (coreServerAPI == null)
				{
					return;
				}
				coreServerAPI.Logger.Warning("SendGuildConfig: config is null for player " + player.PlayerName);
				return;
			}
			else
			{
				ICoreServerAPI coreServerAPI2 = this.serverApi;
				SRGuildsAndKingdomsModSystem modSystem = (coreServerAPI2 != null) ? coreServerAPI2.ModLoader.GetModSystem<SRGuildsAndKingdomsModSystem>(true) : null;
				List<int> list;
				if (modSystem == null)
				{
					list = null;
				}
				else
				{
					TechBlocksConfig techBlocksConfig = modSystem.TechBlocksConfig;
					if (techBlocksConfig == null)
					{
						list = null;
					}
					else
					{
						List<TechAge> enabledAges2 = techBlocksConfig.EnabledAges;
						if (enabledAges2 == null)
						{
							list = null;
						}
						else
						{
							list = (from age in enabledAges2
							select (int)age).ToList<int>();
						}
					}
				}
				List<int> enabledAges = list ?? new List<int>();
				ZoneWhitelistManager zoneWhitelistManager = (modSystem != null) ? modSystem.GetZoneWhitelistManager() : null;
				NodeManager nodeManager = (modSystem != null) ? modSystem.GetNodeManager() : null;
				ICoreServerAPI coreServerAPI3 = this.serverApi;
				if (coreServerAPI3 != null)
				{
					ILogger logger = coreServerAPI3.Logger;
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(85, 4);
					defaultInterpolatedStringHandler.AppendLiteral("SendGuildConfig to ");
					defaultInterpolatedStringHandler.AppendFormatted(player.PlayerName);
					defaultInterpolatedStringHandler.AppendLiteral(": EnableProtectedZones=");
					defaultInterpolatedStringHandler.AppendFormatted<bool>(config.EnableProtectedZones);
					defaultInterpolatedStringHandler.AppendLiteral(", ProtectedZones count=");
					List<ProtectedZone> protectedZones = config.ProtectedZones;
					defaultInterpolatedStringHandler.AppendFormatted<int>((protectedZones != null) ? protectedZones.Count : 0);
					defaultInterpolatedStringHandler.AppendLiteral(", EnabledAges count=");
					defaultInterpolatedStringHandler.AppendFormatted<int>(enabledAges.Count);
					logger.Debug(defaultInterpolatedStringHandler.ToStringAndClear());
				}
				GuildConfigPacket guildConfigPacket = new GuildConfigPacket();
				guildConfigPacket.PlayerUid = player.PlayerUID;
				guildConfigPacket.TerritorialRestrictionsEnabled = config.EnableTerritorialRestrictions;
				ClaimRestrictionCenter territorialCenter = config.TerritorialCenter;
				guildConfigPacket.TerritorialCenterX = ((territorialCenter != null) ? new int?(territorialCenter.X) : null);
				ClaimRestrictionCenter territorialCenter2 = config.TerritorialCenter;
				guildConfigPacket.TerritorialCenterZ = ((territorialCenter2 != null) ? new int?(territorialCenter2.Z) : null);
				guildConfigPacket.TerritorialRadius = config.TerritorialRadius;
				guildConfigPacket.ProtectedZonesEnabled = config.EnableProtectedZones;
				List<ProtectedZone> protectedZones2 = config.ProtectedZones;
				guildConfigPacket.ProtectedZones = (((protectedZones2 != null) ? protectedZones2.Select(delegate(ProtectedZone z)
				{
					ProtectedZoneData protectedZoneData = new ProtectedZoneData();
					protectedZoneData.Name = z.Name;
					protectedZoneData.X = z.X;
					protectedZoneData.Z = z.Z;
					protectedZoneData.Radius = z.Radius;
					ZoneWhitelistManager zoneWhitelistManager = zoneWhitelistManager;
					protectedZoneData.WhitelistedPlayers = (((zoneWhitelistManager != null) ? zoneWhitelistManager.GetWhitelistedPlayers(z.Id) : null) ?? new List<string>());
					return protectedZoneData;
				}).ToList<ProtectedZoneData>() : null) ?? new List<ProtectedZoneData>());
				guildConfigPacket.Nodes = (((nodeManager != null) ? nodeManager.GetNodesForNetworkPacket() : null) ?? new List<NodeData>());
				guildConfigPacket.EnabledAges = enabledAges;
				GuildConfigPacket packet = guildConfigPacket;
				ICoreServerAPI coreServerAPI4 = this.serverApi;
				if (coreServerAPI4 != null)
				{
					ILogger logger2 = coreServerAPI4.Logger;
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(77, 2);
					defaultInterpolatedStringHandler2.AppendLiteral("SendGuildConfig packet created: ProtectedZonesEnabled=");
					defaultInterpolatedStringHandler2.AppendFormatted<bool>(packet.ProtectedZonesEnabled);
					defaultInterpolatedStringHandler2.AppendLiteral(", ProtectedZones count=");
					List<ProtectedZoneData> protectedZones3 = packet.ProtectedZones;
					defaultInterpolatedStringHandler2.AppendFormatted<int>((protectedZones3 != null) ? protectedZones3.Count : 0);
					logger2.Debug(defaultInterpolatedStringHandler2.ToStringAndClear());
				}
				ICoreServerAPI coreServerAPI5 = this.serverApi;
				if (coreServerAPI5 == null)
				{
					return;
				}
				coreServerAPI5.Network.GetChannel("srguildsandkingdoms:guild").SendPacket<GuildConfigPacket>(packet, new IServerPlayer[]
				{
					player
				});
				return;
			}
		}

		// Token: 0x060001E6 RID: 486 RVA: 0x000144DC File Offset: 0x000126DC
		public void BroadcastGuildConfigToAll()
		{
			IPlayer[] allOnlinePlayers = this.serverApi.World.AllOnlinePlayers;
			for (int i = 0; i < allOnlinePlayers.Length; i++)
			{
				IServerPlayer serverPlayer = allOnlinePlayers[i] as IServerPlayer;
				if (serverPlayer != null)
				{
					this.SendGuildConfig(serverPlayer);
				}
			}
		}

		// Token: 0x060001E7 RID: 487 RVA: 0x0001451C File Offset: 0x0001271C
		private void OnGuildConfigReceived(GuildConfigPacket packet)
		{
			ICoreClientAPI coreClientAPI = this.clientApi;
			if (coreClientAPI != null)
			{
				ILogger logger = coreClientAPI.Logger;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(68, 2);
				defaultInterpolatedStringHandler.AppendLiteral("OnGuildConfigReceived: ProtectedZonesEnabled=");
				defaultInterpolatedStringHandler.AppendFormatted<bool>(packet.ProtectedZonesEnabled);
				defaultInterpolatedStringHandler.AppendLiteral(", ProtectedZones count=");
				List<ProtectedZoneData> protectedZones = packet.ProtectedZones;
				defaultInterpolatedStringHandler.AppendFormatted<int>((protectedZones != null) ? protectedZones.Count : 0);
				logger.Debug(defaultInterpolatedStringHandler.ToStringAndClear());
			}
			Action<GuildConfigPacket> action = this.onConfigReceived;
			if (action == null)
			{
				return;
			}
			action(packet);
		}

		// Token: 0x060001E8 RID: 488 RVA: 0x0001459F File Offset: 0x0001279F
		public void RegisterConfigCallback(Action<GuildConfigPacket> callback)
		{
			this.onConfigReceived = callback;
		}

		// Token: 0x060001E9 RID: 489 RVA: 0x000145A8 File Offset: 0x000127A8
		private void OnScaledRequirementsRequestReceived(IServerPlayer player, ScaledRequirementsRequestPacket packet)
		{
			GuildManager guildManager = this.guildManager;
			Guild guild = (guildManager != null) ? guildManager.GetGuild(packet.GuildName) : null;
			if (guild == null)
			{
				this.SendScaledRequirementsResponse(player, new Dictionary<string, int>(), 1.0m, 0);
				return;
			}
			SRGuildsAndKingdomsModSystem srguildsAndKingdomsModSystem = this.modSystem;
			GuildTechManager techManager = (srguildsAndKingdomsModSystem != null) ? srguildsAndKingdomsModSystem.GuildTechManager : null;
			if (techManager == null)
			{
				this.SendScaledRequirementsResponse(player, packet.BaseRequirements, 1.0m, guild.Members.Count);
				return;
			}
			Dictionary<string, int> scaledRequirements = techManager.GetScaledRequirements(packet.GuildName, packet.BaseRequirements);
			decimal resourceScaling = techManager.GetResourceScaling(guild.Members.Count);
			this.SendScaledRequirementsResponse(player, scaledRequirements, resourceScaling, guild.Members.Count);
		}

		// Token: 0x060001EA RID: 490 RVA: 0x00014660 File Offset: 0x00012860
		private void SendScaledRequirementsResponse(IServerPlayer player, Dictionary<string, int> scaledRequirements, decimal resourceScaling, int memberCount)
		{
			ScaledRequirementsResponsePacket response = new ScaledRequirementsResponsePacket
			{
				PlayerUid = player.PlayerUID,
				ScaledRequirements = scaledRequirements,
				ResourceScaling = resourceScaling,
				MemberCount = memberCount
			};
			ICoreServerAPI coreServerAPI = this.serverApi;
			if (coreServerAPI == null)
			{
				return;
			}
			coreServerAPI.Network.GetChannel("srguildsandkingdoms:guild").SendPacket<ScaledRequirementsResponsePacket>(response, new IServerPlayer[]
			{
				player
			});
		}

		// Token: 0x060001EB RID: 491 RVA: 0x000146C0 File Offset: 0x000128C0
		public void RequestScaledRequirements(string guildName, int techBlockId, Dictionary<string, int> baseRequirements, Action<ScaledRequirementsResponsePacket> callback)
		{
			if (this.clientApi == null)
			{
				return;
			}
			this.onScaledRequirementsResponseReceived = callback;
			ScaledRequirementsRequestPacket packet = new ScaledRequirementsRequestPacket
			{
				PlayerUid = this.clientApi.World.Player.PlayerUID,
				GuildName = guildName,
				TechBlockId = techBlockId,
				BaseRequirements = baseRequirements
			};
			this.clientApi.Network.GetChannel("srguildsandkingdoms:guild").SendPacket<ScaledRequirementsRequestPacket>(packet);
		}

		// Token: 0x060001EC RID: 492 RVA: 0x0001472F File Offset: 0x0001292F
		private void OnScaledRequirementsResponseReceived(ScaledRequirementsResponsePacket packet)
		{
			Action<ScaledRequirementsResponsePacket> action = this.onScaledRequirementsResponseReceived;
			if (action == null)
			{
				return;
			}
			action(packet);
		}

		// Token: 0x060001ED RID: 493 RVA: 0x00014744 File Offset: 0x00012944
		public void SendTechBlocksConfig(IServerPlayer player)
		{
			if (this.serverApi == null)
			{
				return;
			}
			SRGuildsAndKingdomsModSystem modSystem = this.serverApi.ModLoader.GetModSystem<SRGuildsAndKingdomsModSystem>(true);
			if (modSystem == null)
			{
				this.serverApi.Logger.Warning("SendTechBlocksConfig: modSystem is null for player " + player.PlayerName);
				return;
			}
			TechBlocksConfig techBlocksConfig = modSystem.TechBlocksConfig;
			if (techBlocksConfig == null)
			{
				this.serverApi.Logger.Warning("SendTechBlocksConfig: techBlocksConfig is null for player " + player.PlayerName);
				return;
			}
			JsonSerializerOptions options = new JsonSerializerOptions
			{
				WriteIndented = true,
				DefaultIgnoreCondition = JsonIgnoreCondition.Never
			};
			string configJson = JsonSerializer.Serialize<TechBlocksConfig>(techBlocksConfig, options);
			string serverIdentifier = "unknown";
			try
			{
				GuildManager guildManager = this.guildManager;
				GuildConfig guildConfig;
				if (guildManager == null)
				{
					guildConfig = null;
				}
				else
				{
					GuildConfigManager configManager = guildManager.GetConfigManager();
					guildConfig = ((configManager != null) ? configManager.GetConfig() : null);
				}
				GuildConfig config = guildConfig;
				if (config != null && !string.IsNullOrWhiteSpace(config.ServerName))
				{
					serverIdentifier = config.ServerName;
				}
				else
				{
					IWorldManagerAPI worldManager = this.serverApi.WorldManager;
					bool flag;
					if (worldManager == null)
					{
						flag = (null != null);
					}
					else
					{
						ISaveGame saveGame = worldManager.SaveGame;
						flag = (((saveGame != null) ? saveGame.WorldName : null) != null);
					}
					if (flag)
					{
						serverIdentifier = this.serverApi.WorldManager.SaveGame.WorldName;
					}
				}
			}
			catch (Exception ex)
			{
				this.serverApi.Logger.Warning("SendTechBlocksConfig: Failed to get server identifier: " + ex.Message);
			}
			TechBlocksConfigSyncPacket packet = new TechBlocksConfigSyncPacket
			{
				PlayerUid = player.PlayerUID,
				ConfigJson = configJson,
				ServerIdentifier = serverIdentifier
			};
			ILogger logger = this.serverApi.Logger;
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(47, 2);
			defaultInterpolatedStringHandler.AppendLiteral("Sending TechBlocks config to player ");
			defaultInterpolatedStringHandler.AppendFormatted(player.PlayerName);
			defaultInterpolatedStringHandler.AppendLiteral(" (server: ");
			defaultInterpolatedStringHandler.AppendFormatted(serverIdentifier);
			defaultInterpolatedStringHandler.AppendLiteral(")");
			logger.Notification(defaultInterpolatedStringHandler.ToStringAndClear());
			if (this.serverApi.Network == null)
			{
				this.serverApi.Logger.Error("SendTechBlocksConfig: serverApi.Network is null for player " + player.PlayerName);
				return;
			}
			IServerNetworkChannel channel = this.serverApi.Network.GetChannel("srguildsandkingdoms:guild");
			if (channel == null)
			{
				this.serverApi.Logger.Error("SendTechBlocksConfig: Channel 'srguildsandkingdoms:guild' not found for player " + player.PlayerName);
				return;
			}
			channel.SendPacket<TechBlocksConfigSyncPacket>(packet, new IServerPlayer[]
			{
				player
			});
		}

		// Token: 0x060001EE RID: 494 RVA: 0x00014988 File Offset: 0x00012B88
		public void BroadcastTechBlocksConfigToAll()
		{
			IPlayer[] allOnlinePlayers = this.serverApi.World.AllOnlinePlayers;
			for (int i = 0; i < allOnlinePlayers.Length; i++)
			{
				IServerPlayer serverPlayer = allOnlinePlayers[i] as IServerPlayer;
				if (serverPlayer != null)
				{
					this.SendTechBlocksConfig(serverPlayer);
				}
			}
		}

		// Token: 0x060001EF RID: 495 RVA: 0x000149C7 File Offset: 0x00012BC7
		private void OnTechBlocksConfigReceived(TechBlocksConfigSyncPacket packet)
		{
			ICoreClientAPI coreClientAPI = this.clientApi;
			if (coreClientAPI != null)
			{
				coreClientAPI.Logger.Notification("Received TechBlocks config from server (identifier: " + packet.ServerIdentifier + ")");
			}
			Action<TechBlocksConfigSyncPacket> action = this.onTechBlocksConfigReceived;
			if (action == null)
			{
				return;
			}
			action(packet);
		}

		// Token: 0x060001F0 RID: 496 RVA: 0x00014A05 File Offset: 0x00012C05
		public void RegisterTechBlocksConfigCallback(Action<TechBlocksConfigSyncPacket> callback)
		{
			this.onTechBlocksConfigReceived = callback;
		}

		// Token: 0x060001F1 RID: 497 RVA: 0x00014A10 File Offset: 0x00012C10
		public void RequestNodeWarData(string guildName)
		{
			if (this.clientApi == null)
			{
				return;
			}
			NodeWarDataRequestPacket packet = new NodeWarDataRequestPacket
			{
				PlayerUid = this.clientApi.World.Player.PlayerUID,
				GuildName = guildName
			};
			IClientNetworkChannel channel = this.clientApi.Network.GetChannel("srguildsandkingdoms:guild");
			if (channel != null)
			{
				channel.SendPacket<NodeWarDataRequestPacket>(packet);
			}
			this.clientApi.Logger.Debug("[Guild UI] Requesting node war data for guild: " + guildName);
		}

		// Token: 0x060001F2 RID: 498 RVA: 0x00014A8C File Offset: 0x00012C8C
		private void OnNodeWarDataRequestReceived(IServerPlayer player, NodeWarDataRequestPacket packet)
		{
			if (this.serverApi == null)
			{
				return;
			}
			this.serverApi.Logger.Debug("[Guild] Received node war data request from " + player.PlayerName + " for guild " + packet.GuildName);
			ModSystem pvpMod = this.serverApi.ModLoader.GetModSystem("SRGuildsAndKingdomsPVP.PVPModSystem");
			if (pvpMod != null)
			{
				SRGuildsAndKingdomsModSystem modSystem = this.serverApi.ModLoader.GetModSystem<SRGuildsAndKingdomsModSystem>(true);
				if (modSystem != null)
				{
					MethodInfo method = pvpMod.GetType().GetMethod("OnNodeWarDataRequested");
					if (method != null)
					{
						method.Invoke(pvpMod, new object[]
						{
							player,
							packet.GuildName,
							modSystem
						});
						this.serverApi.Logger.Debug("[Guild] Forwarded data request to PVP mod");
						return;
					}
					this.serverApi.Logger.Warning("[Guild] PVP mod found but OnNodeWarDataRequested method not available");
					return;
				}
			}
			else
			{
				this.serverApi.Logger.Debug("[Guild] PVP mod not available - sending empty response");
				NodeWarDataResponsePacket emptyPacket = new NodeWarDataResponsePacket
				{
					PlayerUid = player.PlayerUID
				};
				this.SendNodeWarData(player, emptyPacket);
			}
		}

		// Token: 0x060001F3 RID: 499 RVA: 0x00014B94 File Offset: 0x00012D94
		public void SendNodeWarData(IServerPlayer player, NodeWarDataResponsePacket packet)
		{
			if (this.serverApi == null)
			{
				return;
			}
			IServerNetworkChannel channel = this.serverApi.Network.GetChannel("srguildsandkingdoms:guild");
			if (channel == null)
			{
				this.serverApi.Logger.Error("SendNodeWarData: Channel 'srguildsandkingdoms:guild' not found");
				return;
			}
			packet.PlayerUid = player.PlayerUID;
			channel.SendPacket<NodeWarDataResponsePacket>(packet, new IServerPlayer[]
			{
				player
			});
			this.serverApi.Logger.Debug("[Guild] Sent node war data to " + player.PlayerName);
		}

		// Token: 0x060001F4 RID: 500 RVA: 0x00014C16 File Offset: 0x00012E16
		private void OnNodeWarDataResponseReceived(NodeWarDataResponsePacket packet)
		{
			ICoreClientAPI coreClientAPI = this.clientApi;
			if (coreClientAPI != null)
			{
				coreClientAPI.Logger.Debug("[Guild UI] Received node war data response");
			}
			Action<NodeWarDataResponsePacket> action = this.onNodeWarDataReceived;
			if (action == null)
			{
				return;
			}
			action(packet);
		}

		// Token: 0x060001F5 RID: 501 RVA: 0x00014C44 File Offset: 0x00012E44
		public void RegisterNodeWarDataCallback(Action<NodeWarDataResponsePacket> callback)
		{
			this.onNodeWarDataReceived = callback;
		}

		// Token: 0x040000A8 RID: 168
		private const string ChannelName = "srguildsandkingdoms:guild";

		// Token: 0x040000A9 RID: 169
		[Nullable(2)]
		private ICoreServerAPI serverApi;

		// Token: 0x040000AA RID: 170
		[Nullable(2)]
		private ICoreClientAPI clientApi;

		// Token: 0x040000AB RID: 171
		[Nullable(2)]
		private GuildManager guildManager;

		// Token: 0x040000AC RID: 172
		[Nullable(2)]
		private SRGuildsAndKingdomsModSystem modSystem;

		// Token: 0x040000AD RID: 173
		[Nullable(new byte[]
		{
			2,
			1,
			1
		})]
		private Action<List<GuildSummary>> onGuildSummariesReceived;

		// Token: 0x040000AE RID: 174
		[Nullable(new byte[]
		{
			2,
			1
		})]
		private Action<string, NotificationType> onNotificationReceived;

		// Token: 0x040000AF RID: 175
		[Nullable(new byte[]
		{
			2,
			1,
			1
		})]
		private Action<List<GuildMemberInfo>> onMemberListReceived;

		// Token: 0x040000B0 RID: 176
		[Nullable(new byte[]
		{
			2,
			1
		})]
		private Action<TechContributionResponsePacket> onTechContributionResponseReceived;

		// Token: 0x040000B1 RID: 177
		[Nullable(new byte[]
		{
			2,
			1
		})]
		private Action<GuildConfigPacket> onConfigReceived;

		// Token: 0x040000B2 RID: 178
		[Nullable(new byte[]
		{
			2,
			1
		})]
		private Action<ScaledRequirementsResponsePacket> onScaledRequirementsResponseReceived;

		// Token: 0x040000B3 RID: 179
		[Nullable(new byte[]
		{
			2,
			1
		})]
		private Action<TechBlocksConfigSyncPacket> onTechBlocksConfigReceived;

		// Token: 0x040000B4 RID: 180
		[Nullable(new byte[]
		{
			2,
			1
		})]
		private Action<NodeWarDataResponsePacket> onNodeWarDataReceived;
	}
}
