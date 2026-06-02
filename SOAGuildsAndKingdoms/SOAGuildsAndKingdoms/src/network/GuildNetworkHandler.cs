using ProtoBuf;
using SOAGuildsAndKingdoms.src.guilds;
using SOAGuildsAndKingdoms.src.config;
using System;
using System.Collections.Generic;
using System.Linq;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Server;

namespace SOAGuildsAndKingdoms.src.network
{
    // Add this new packet class near the top of the file or in a separate packets file
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class GuildLeavePacket
    {
        public string PlayerUid { get; set; } = "";
    }

    public class GuildNetworkHandler
    {
        private const string ChannelName = "soaguildsandkingdoms:guild";

        private ICoreServerAPI? serverApi;
        private ICoreClientAPI? clientApi;
        private GuildManager? guildManager;
        private SOAGuildsAndKingdomsModSystem? modSystem;
        private Action<List<GuildSummary>>? onGuildSummariesReceived;
        private Action<string, NotificationType>? onNotificationReceived;
        private Action<List<GuildMemberInfo>>? onMemberListReceived;
        private Action<TechContributionResponsePacket>? onTechContributionResponseReceived;
        private Action<GuildConfigPacket>? onConfigReceived;
        private Action<ScaledRequirementsResponsePacket>? onScaledRequirementsResponseReceived;
        private Action<TechBlocksConfigSyncPacket>? onTechBlocksConfigReceived;
        private Action<NodeWarDataResponsePacket>? onNodeWarDataReceived;

        // Server-side initialization
        public void InitializeServer(ICoreServerAPI api, GuildManager manager)
        {
            serverApi = api;
            guildManager = manager;
            // Get mod system for GuildTechManager access
            modSystem = api.ModLoader.GetModSystem<SOAGuildsAndKingdomsModSystem>();

            // Register server-side packet handlers
            serverApi.Network.RegisterChannel(ChannelName)
                .RegisterMessageType<GuildCommandPacket>()
                .RegisterMessageType<GuildCreatePacket>()
                .RegisterMessageType<GuildInvitePacket>()
                .RegisterMessageType<GuildAcceptInvitePacket>()
                .RegisterMessageType<GuildDeclineInvitePacket>()
                .RegisterMessageType<GuildCancelInvitePacket>()
                .RegisterMessageType<GuildListInvitesPacket>()
                .RegisterMessageType<GuildRemoveMemberPacket>()
                .RegisterMessageType<GuildLeavePacket>()
                .RegisterMessageType<GuildClaimLandPacket>()
                .RegisterMessageType<GuildUnclaimLandPacket>()
                .RegisterMessageType<GuildRoleManagementPacket>()
                .RegisterMessageType<GuildMemberListRequestPacket>()
                .RegisterMessageType<GuildTransferOwnershipPacket>()
                .RegisterMessageType<TechContributionRequestPacket>()
                .RegisterMessageType<TechContributionResponsePacket>()
                .RegisterMessageType<ScaledRequirementsRequestPacket>()
                .RegisterMessageType<ScaledRequirementsResponsePacket>()
                .RegisterMessageType<PersonalTechContributionRequestPacket>()
                .RegisterMessageType<PersonalTechContributionResponsePacket>()
                .RegisterMessageType<PersonalUnlockProgressSyncPacket>()
                .RegisterMessageType<PersonalUnlockDto>()
                .RegisterMessageType<ContributionItemDto>()
                .RegisterMessageType<GuildSyncPacket>()
                .RegisterMessageType<GuildUpdatePacket>()
                .RegisterMessageType<GuildNotificationPacket>()
                .RegisterMessageType<GuildInviteNotificationPacket>()
                .RegisterMessageType<GuildInviteListResponsePacket>()
                .RegisterMessageType<GuildInviteInfo>()
                .RegisterMessageType<GuildMemberListPacket>()
                .RegisterMessageType<GuildConfigPacket>()
                .RegisterMessageType<ProtectedZoneData>()
                .RegisterMessageType<TechBlocksConfigSyncPacket>()
                .RegisterMessageType<NodeWarDataRequestPacket>()
                .RegisterMessageType<NodeWarDataResponsePacket>()
                .RegisterMessageType<ControlledNodeDto>()
                .RegisterMessageType<CurrentWarDto>()
                .RegisterMessageType<GuildWarProgressDto>()
                .RegisterMessageType<AvailableWarDto>()
                .RegisterMessageType<CurrentSignupDto>()
                .SetMessageHandler<GuildCommandPacket>(OnGuildCommandReceived)
                .SetMessageHandler<GuildCreatePacket>(OnGuildCreateReceived)
                .SetMessageHandler<GuildInvitePacket>(OnGuildInviteReceived)
                .SetMessageHandler<GuildAcceptInvitePacket>(OnGuildAcceptInviteReceived)
                .SetMessageHandler<GuildDeclineInvitePacket>(OnGuildDeclineInviteReceived)
                .SetMessageHandler<GuildCancelInvitePacket>(OnGuildCancelInviteReceived)
                .SetMessageHandler<GuildListInvitesPacket>(OnGuildListInvitesReceived)
                .SetMessageHandler<GuildRemoveMemberPacket>(OnGuildRemoveMemberReceived)
                .SetMessageHandler<GuildLeavePacket>(OnGuildLeaveReceived)
                .SetMessageHandler<GuildClaimLandPacket>(OnGuildClaimLandReceived)
                .SetMessageHandler<GuildUnclaimLandPacket>(OnGuildUnclaimLandReceived)
                .SetMessageHandler<GuildRoleManagementPacket>(OnGuildRoleManagementReceived)
                .SetMessageHandler<GuildMemberListRequestPacket>(OnGuildMemberListRequestReceived)
                .SetMessageHandler<GuildTransferOwnershipPacket>(OnGuildTransferOwnershipReceived)
                .SetMessageHandler<TechContributionRequestPacket>(OnTechContributionRequestReceived)
                .SetMessageHandler<ScaledRequirementsRequestPacket>(OnScaledRequirementsRequestReceived)
                .SetMessageHandler<NodeWarDataRequestPacket>(OnNodeWarDataRequestReceived);

            // Send guild data when players join
            serverApi.Event.PlayerJoin += OnPlayerJoin;
            serverApi.Event.PlayerDisconnect += OnPlayerDisconnect;
        }

        // Client-side initialization - overloaded method to support guild summaries callback
        public void InitializeClient(ICoreClientAPI api, Action<string, NotificationType> onNotification)
        {
            InitializeClient(api, onNotification, null);
        }

        public void InitializeClient(ICoreClientAPI api, Action<string, NotificationType> onNotification, Action<List<GuildSummary>>? onGuildSummaries)
        {
            clientApi = api;
            onNotificationReceived = onNotification;
            onGuildSummariesReceived = onGuildSummaries;

            // Register client-side packet handlers
            clientApi.Network.RegisterChannel(ChannelName)
                .RegisterMessageType<GuildCommandPacket>()
                .RegisterMessageType<GuildCreatePacket>()
                .RegisterMessageType<GuildInvitePacket>()
                .RegisterMessageType<GuildAcceptInvitePacket>()
                .RegisterMessageType<GuildDeclineInvitePacket>()
                .RegisterMessageType<GuildCancelInvitePacket>()
                .RegisterMessageType<GuildListInvitesPacket>()
                .RegisterMessageType<GuildRemoveMemberPacket>()
                .RegisterMessageType<GuildLeavePacket>()
                .RegisterMessageType<GuildClaimLandPacket>()
                .RegisterMessageType<GuildUnclaimLandPacket>()
                .RegisterMessageType<GuildRoleManagementPacket>()
                .RegisterMessageType<GuildMemberListRequestPacket>()
                .RegisterMessageType<GuildTransferOwnershipPacket>()
                .RegisterMessageType<TechContributionRequestPacket>()
                .RegisterMessageType<TechContributionResponsePacket>()
                .RegisterMessageType<ScaledRequirementsRequestPacket>()
                .RegisterMessageType<ScaledRequirementsResponsePacket>()
                .RegisterMessageType<PersonalTechContributionRequestPacket>()
                .RegisterMessageType<PersonalTechContributionResponsePacket>()
                .RegisterMessageType<PersonalUnlockProgressSyncPacket>()
                .RegisterMessageType<PersonalUnlockDto>()
                .RegisterMessageType<ContributionItemDto>()
                .RegisterMessageType<GuildSyncPacket>()
                .RegisterMessageType<GuildUpdatePacket>()
                .RegisterMessageType<GuildNotificationPacket>()
                .RegisterMessageType<GuildInviteNotificationPacket>()
                .RegisterMessageType<GuildInviteListResponsePacket>()
                .RegisterMessageType<GuildInviteInfo>()
                .RegisterMessageType<GuildMemberListPacket>()
                .RegisterMessageType<GuildConfigPacket>()
                .RegisterMessageType<ProtectedZoneData>()
                .RegisterMessageType<TechBlocksConfigSyncPacket>()
                .RegisterMessageType<NodeWarDataRequestPacket>()
                .RegisterMessageType<NodeWarDataResponsePacket>()
                .RegisterMessageType<ControlledNodeDto>()
                .RegisterMessageType<CurrentWarDto>()
                .RegisterMessageType<GuildWarProgressDto>()
                .RegisterMessageType<AvailableWarDto>()
                .RegisterMessageType<CurrentSignupDto>()
                .SetMessageHandler<GuildSyncPacket>(OnGuildSyncReceived)
                .SetMessageHandler<GuildUpdatePacket>(OnGuildUpdateReceived)
                .SetMessageHandler<GuildNotificationPacket>(OnGuildNotificationReceived)
                .SetMessageHandler<GuildInviteNotificationPacket>(OnGuildInviteNotificationReceived)
                .SetMessageHandler<GuildInviteListResponsePacket>(OnGuildInviteListResponseReceived)
                .SetMessageHandler<GuildMemberListPacket>(OnGuildMemberListReceived)
                .SetMessageHandler<TechContributionResponsePacket>(OnTechContributionResponseReceived)
                .SetMessageHandler<ScaledRequirementsResponsePacket>(OnScaledRequirementsResponseReceived)
                .SetMessageHandler<GuildConfigPacket>(OnGuildConfigReceived)
                .SetMessageHandler<TechBlocksConfigSyncPacket>(OnTechBlocksConfigReceived)
                .SetMessageHandler<NodeWarDataResponsePacket>(OnNodeWarDataResponseReceived);
        }

        // Server-side packet handlers
        private void OnPlayerJoin(IServerPlayer player)
        {
            // Send guild summaries to newly joined player
            BroadcastGuildSummaries(player);

            // Send guild config to newly joined player
            SendGuildConfig(player);

            // Send tech blocks config to newly joined player
            SendTechBlocksConfig(player);

            // Update last seen time for guild members
            UpdateMemberLastSeen(player.PlayerUID);
        }

        private void OnPlayerDisconnect(IServerPlayer player)
        {
            // Update last seen time when player disconnects
            UpdateMemberLastSeen(player.PlayerUID);
        }

        private void UpdateMemberLastSeen(string playerUid)
        {
            var guild = guildManager?.GetGuildByMember(playerUid);
            if (guild != null && guild.Members.ContainsKey(playerUid))
            {
                guild.Members[playerUid].LastSeen = DateTime.UtcNow;

                var modSystem = serverApi?.ModLoader.GetModSystem<SOAGuildsAndKingdomsModSystem>();
                modSystem?.GetGuildRepository()?.MarkDirty(guild.Name);
            }
        }

        private void OnGuildMemberListRequestReceived(IServerPlayer player, GuildMemberListRequestPacket packet)
        {
            var guild = guildManager?.GetGuildByMember(player.PlayerUID);
            if (guild == null)
            {
                SendNotification(player, "You are not in a guild.", NotificationType.Error);
                return;
            }

            var memberInfoList = new List<GuildMemberInfo>();
            var onlinePlayers = serverApi!.World.AllOnlinePlayers;
            var onlinePlayerUids = new HashSet<string>(onlinePlayers.Select(p => p.PlayerUID));

            foreach (var member in guild.Members.Values)
            {
                // Get player name - first check if they're online, otherwise get from player data
                var onlinePlayer = onlinePlayers.FirstOrDefault(p => p.PlayerUID == member.PlayerUid);
                var playerName = onlinePlayer?.PlayerName ?? serverApi.PlayerData.GetPlayerDataByUid(member.PlayerUid)?.LastKnownPlayername ?? "Unknown";

                var memberInfo = new GuildMemberInfo
                {
                    PlayerUid = member.PlayerUid,
                    PlayerName = playerName,
                    Role = member.Role,
                    IsOnline = onlinePlayerUids.Contains(member.PlayerUid),
                    LastSeenTicks = member.LastSeen.Ticks
                };

                memberInfoList.Add(memberInfo);
            }

            // Send member list to the requesting player
            var response = new GuildMemberListPacket
            {
                PlayerUid = player.PlayerUID,
                Members = memberInfoList
            };

            serverApi.Network.GetChannel(ChannelName).SendPacket(response, player);
        }

        private void OnGuildCommandReceived(IServerPlayer player, GuildCommandPacket packet)
        {
            var guild = guildManager?.GetGuildByMember(player.PlayerUID);
            if (guild == null)
            {
                SendNotification(player, "You are not in a guild.", NotificationType.Error);
                return;
            }

            if (!GuildManager.HasPermission(guild, player.PlayerUID, GuildPermission.ManageRoles))
            {
                SendNotification(player, "You don't have permission to change guild settings.", NotificationType.Error);
                return;
            }

            switch (packet.Command?.ToLowerInvariant())
            {
                case "changename":
                    if (packet.Arguments?.Length > 0)
                    {
                        string newName = packet.Arguments[0];
                        if (string.IsNullOrWhiteSpace(newName))
                        {
                            SendNotification(player, "Guild name cannot be empty.", NotificationType.Error);
                            return;
                        }

                        bool success = guildManager.ChangeGuildName(guild.Name, player.PlayerUID, newName);
                        if (success)
                        {
                            SendNotification(player, $"Guild name changed to '{newName}'.", NotificationType.Success);
                            BroadcastGuildSummariesToAll();
                        }
                        else
                        {
                            SendNotification(player, "Failed to change guild name. Name may already be in use.", NotificationType.Error);
                        }
                    }
                    break;

                case "changecolor":
                    if (packet.Arguments?.Length > 0 && int.TryParse(packet.Arguments[0], out int primaryColor))
                    {
                        guild.DisplayColor = primaryColor;
                        var guildRepo = serverApi?.ModLoader.GetModSystem<SOAGuildsAndKingdomsModSystem>()?.GetGuildRepository();
                        guildRepo?.MarkDirty(guild.Name);
                        SendNotification(player, "Guild primary color updated.", NotificationType.Success);
                        BroadcastGuildSummariesToAll();
                    }
                    else
                    {
                        SendNotification(player, "Invalid color value.", NotificationType.Error);
                    }
                    break;

                case "changesecondarycolor":
                    if (packet.Arguments?.Length > 0 && int.TryParse(packet.Arguments[0], out int secondaryColor))
                    {
                        guild.SecondaryColor = secondaryColor;
                        var guildRepo = serverApi?.ModLoader.GetModSystem<SOAGuildsAndKingdomsModSystem>()?.GetGuildRepository();
                        guildRepo?.MarkDirty(guild.Name);
                        SendNotification(player, "Guild secondary color updated.", NotificationType.Success);
                        BroadcastGuildSummariesToAll();
                    }
                    else
                    {
                        SendNotification(player, "Invalid color value.", NotificationType.Error);
                    }
                    break;

                case "changedescription":
                    if (packet.Arguments?.Length > 0)
                    {
                        string newDescription = packet.Arguments[0];
                        if (newDescription.Length > 100)
                        {
                            SendNotification(player, "Guild description cannot exceed 100 characters.", NotificationType.Error);
                            return;
                        }

                        guild.Description = newDescription;
                        var guildRepo = serverApi?.ModLoader.GetModSystem<SOAGuildsAndKingdomsModSystem>()?.GetGuildRepository();
                        guildRepo?.MarkDirty(guild.Name);
                        SendNotification(player, "Guild description updated.", NotificationType.Success);
                        BroadcastGuildSummariesToAll();
                    }
                    else
                    {
                        SendNotification(player, "Invalid description.", NotificationType.Error);
                    }
                    break;

                default:
                    SendNotification(player, $"Unknown guild command: {packet.Command}", NotificationType.Error);
                    break;
            }
        }

        private void OnGuildCreateReceived(IServerPlayer player, GuildCreatePacket packet)
        {
            bool success = guildManager.CreateGuild(packet.GuildName, player.PlayerUID, packet.Description);

            if (success)
            {
                SendNotification(player, $"Guild '{packet.GuildName}' created successfully.", NotificationType.Success);
                BroadcastGuildSummariesToAll();
            }
            else
            {
                SendNotification(player, $"Failed to create guild '{packet.GuildName}'. It may already exist or you may already be in a guild.", NotificationType.Error);
            }
        }

        private void OnGuildInviteReceived(IServerPlayer player, GuildInvitePacket packet)
        {
            var invitee = serverApi.World.AllOnlinePlayers.FirstOrDefault(p =>
                p.PlayerUID.Equals(packet.TargetPlayerUid, StringComparison.OrdinalIgnoreCase));

            if (invitee == null)
            {
                SendNotification(player, $"Player not found or is offline.", NotificationType.Error);
                return;
            }

            var guild = guildManager.GetGuildByMember(player.PlayerUID);
            if (guild == null)
            {
                SendNotification(player, "You are not in a guild.", NotificationType.Error);
                return;
            }

            bool success = guildManager.InviteToGuild(guild.Name, player.PlayerUID, invitee.PlayerUID);

            if (success)
            {
                SendNotification(player, $"Invited {invitee.PlayerName} to guild.", NotificationType.Success);

                // Get the invite to get expiry time
                var invite = guild.PendingInvites.FirstOrDefault(i => i.InviteeUid == invitee.PlayerUID);
                if (invite != null)
                {
                    // Notify the invitee with full invite information
                    var inviteNotification = new GuildInviteNotificationPacket
                    {
                        PlayerUid = invitee.PlayerUID,
                        InviterName = player.PlayerName,
                        InviterUid = player.PlayerUID,
                        GuildName = guild.Name,
                        ExpiresAtTicks = invite.ExpiresAt.Ticks
                    };
                    serverApi.Network.GetChannel(ChannelName).SendPacket(inviteNotification, invitee as IServerPlayer);
                }
            }
            else
            {
                SendNotification(player, $"Could not invite {invitee.PlayerName}.", NotificationType.Error);
            }
        }

        private void OnGuildAcceptInviteReceived(IServerPlayer player, GuildAcceptInvitePacket packet)
        {
            bool success = guildManager.AcceptInvite(player.PlayerUID);

            if (success)
            {
                SendNotification(player, "You have joined the guild.", NotificationType.Success);

                // Initialize personal unlocks for the new member if guild has large size (>10 members)
                var guild = guildManager.GetGuildByMember(player.PlayerUID);
                if (guild != null)
                {
                    var modSystem = serverApi.ModLoader.GetModSystem<SOAGuildsAndKingdomsModSystem>();
                    if (modSystem != null && guild.Members.Count > 10)
                    {
                        //modSystem.GuildTechManager.InitializePersonalUnlocksForNewMember(guild.Name, player.PlayerUID, modSystem.TechBlocks);
                        // Note: AcceptInvite already marks guild as dirty
                    }
                }

                BroadcastGuildSummariesToAll();
            }
            else
            {
                SendNotification(player, "No pending guild invite found or invite has expired.", NotificationType.Error);
            }
        }

        private void OnGuildDeclineInviteReceived(IServerPlayer player, GuildDeclineInvitePacket packet)
        {
            bool success = guildManager.DeclineInvite(player.PlayerUID, packet.GuildName);

            if (success)
            {
                SendNotification(player, $"You have declined the invite to {packet.GuildName}.", NotificationType.Info);
            }
            else
            {
                SendNotification(player, "No pending invite found for that guild.", NotificationType.Error);
            }
        }

        private void OnGuildCancelInviteReceived(IServerPlayer player, GuildCancelInvitePacket packet)
        {
            var guild = guildManager?.GetGuildByMember(player.PlayerUID);
            if (guild == null)
            {
                SendNotification(player, "You are not in a guild.", NotificationType.Error);
                return;
            }

            bool success = guildManager.CancelInvite(guild.Name, player.PlayerUID, packet.InviteeUid, out string message);

            if (success)
            {
                SendNotification(player, message, NotificationType.Success);
                BroadcastGuildSummariesToAll(); // Update all clients with the new invite list
            }
            else
            {
                SendNotification(player, message, NotificationType.Error);
            }
        }

        private void OnGuildListInvitesReceived(IServerPlayer player, GuildListInvitesPacket packet)
        {
            var invites = guildManager.GetPlayerInvites(player.PlayerUID);

            var inviteInfoList = new List<GuildInviteInfo>();
            foreach (var invite in invites)
            {
                var inviter = serverApi.World.AllPlayers.FirstOrDefault(p => p.PlayerUID == invite.InviterUid);

                inviteInfoList.Add(new GuildInviteInfo
                {
                    GuildName = invite.GuildName,
                    InviterName = inviter?.PlayerName ?? "Unknown",
                    InviterUid = invite.InviterUid,
                    ExpiresAtTicks = invite.ExpiresAt.Ticks
                });
            }

            var response = new GuildInviteListResponsePacket
            {
                PlayerUid = player.PlayerUID,
                Invites = inviteInfoList
            };

            serverApi.Network.GetChannel(ChannelName).SendPacket(response, player);
        }

        private void OnGuildRemoveMemberReceived(IServerPlayer player, GuildRemoveMemberPacket packet)
        {
            var guild = guildManager!.GetGuildByMember(player.PlayerUID);
            if (guild == null)
            {
                SendNotification(player, "You are not in a guild.", NotificationType.Error);
                return;
            }

            var targetPlayerUid = FindGuildMemberByName(guild, packet.TargetPlayerName);

            if (targetPlayerUid == null)
            {
                SendNotification(player, $"Player '{packet.TargetPlayerName}' not found in guild.", NotificationType.Error);
                return;
            }

            bool success = guildManager.KickMember(guild.Name, player.PlayerUID, targetPlayerUid, out string message);

            if (success)
            {
                SendNotification(player, message, NotificationType.Success);

                // Only notify the target player if they're currently online
                var targetPlayer = serverApi!.World.AllOnlinePlayers.FirstOrDefault(p => p.PlayerUID == targetPlayerUid) as IServerPlayer;
                if (targetPlayer != null)
                {
                    SendNotification(targetPlayer, "You have been removed from the guild.", NotificationType.Warning);
                }

                BroadcastGuildSummariesToAll();
            }
            else
            {
                SendNotification(player, message, NotificationType.Error);
            }
        }

        private void OnGuildLeaveReceived(IServerPlayer player, GuildLeavePacket packet)
        {
            bool success = guildManager!.LeaveGuild(player.PlayerUID, out string message);

            if (success)
            {
                SendNotification(player, message, NotificationType.Success);
                BroadcastGuildSummariesToAll();
            }
            else
            {
                SendNotification(player, message, NotificationType.Error);
            }
        }

        private void OnGuildTransferOwnershipReceived(IServerPlayer player, GuildTransferOwnershipPacket packet)
        {
            var guild = guildManager?.GetGuildByMember(player.PlayerUID);
            if (guild == null)
            {
                SendNotification(player, "You are not in a guild.", NotificationType.Error);
                return;
            }

            bool success = guildManager.TransferOwnership(guild.Name, player.PlayerUID, packet.TargetPlayerUid, out string message);

            if (success)
            {
                SendNotification(player, message, NotificationType.Success);

                // Notify the new leader
                var newLeader = serverApi?.World.PlayerByUid(packet.TargetPlayerUid) as IServerPlayer;
                if (newLeader != null)
                {
                    SendNotification(newLeader, $"You have been promoted to guild leader of '{guild.Name}'!", NotificationType.Success);
                }

                BroadcastGuildSummariesToAll();
            }
            else
            {
                SendNotification(player, message, NotificationType.Error);
            }
        }

        private void OnGuildClaimLandReceived(IServerPlayer player, GuildClaimLandPacket packet)
        {
            var guild = guildManager.GetGuildByMember(player.PlayerUID);
            if (guild == null)
            {
                SendNotification(player, "You are not in a guild.", NotificationType.Error);
                return;
            }

            // Check permissions first
            if (!GuildManager.HasPermission(guild, player.PlayerUID, GuildPermission.ManageRoles))
            {
                SendNotification(player, "You don't have permission to claim land for the guild.", NotificationType.Error);
                return;
            }

            // Check appropriate claim limit based on claim type
            if (packet.IsOutpost)
            {
                var maxOutposts = guildManager.GetMaxOutpostsPerGuild(guild);
                var currentOutposts = guildManager.GetOutpostClaimCount(guild);
                if (currentOutposts >= maxOutposts)
                {
                    SendNotification(player, $"Cannot create more outposts. Your guild has reached the maximum limit of {maxOutposts} outposts (current: {currentOutposts}).", NotificationType.Error);
                    return;
                }
            }
            else
            {
                var maxClaims = guildManager.GetMaxClaimsPerGuild(guild);
                var currentClaims = guildManager.GetNonOutpostClaimCount(guild);
                if (currentClaims >= maxClaims)
                {
                    SendNotification(player, $"Cannot claim more land. Your guild has reached the maximum limit of {maxClaims} claims (current: {currentClaims}).", NotificationType.Error);
                    return;
                }
            }

            // Check territorial restrictions early with specific error message
            var config = guildManager.GetConfigManager().GetConfig();

            // Convert block coordinates to chunk coordinates for overlap check
            int chunkX = SOAGuildsAndKingdoms.src.guilds.LandClaim.FloorDiv(packet.BlockX, SOAGuildsAndKingdoms.src.guilds.LandClaim.ChunkSize);
            int chunkZ = SOAGuildsAndKingdoms.src.guilds.LandClaim.FloorDiv(packet.BlockZ, SOAGuildsAndKingdoms.src.guilds.LandClaim.ChunkSize);

            // Check if guild needs to establish a home
            bool needsGuildHome = !guild.Claims.Any(c => c is GuildHomeClaim);

            // If trying to create an outpost but guild has no home, reject
            if (packet.IsOutpost && needsGuildHome)
            {
                SendNotification(player, "Guild must establish a home base before creating outposts.", NotificationType.Error);
                return;
            }

            // Additional check for guild home territorial restrictions (2x2 area)
            if (needsGuildHome && config.EnableTerritorialRestrictions)
            {
                for (int dx = 0; dx <= 1; dx++)
                {
                    for (int dz = 0; dz <= 1; dz++)
                    {
                        int homeChunkX = chunkX + dx;
                        int homeChunkZ = chunkZ + dz;
                        /* if (!config.IsChunkWithinTerritorialBounds(homeChunkX, homeChunkZ))
                         {
                             if (config.TerritorialCenter != null)
                             {
                                 SendNotification(player, $"Cannot establish guild home at chunk ({chunkX},{chunkZ}). Part of the 2x2 guild home area would be outside the allowed claiming zone (within {config.TerritorialRadius} blocks of {config.TerritorialCenter}).", NotificationType.Error);
                             }
                             else
                             {
                                 SendNotification(player, $"Cannot establish guild home at chunk ({chunkX},{chunkZ}). Part of the 2x2 guild home area would be outside the allowed claiming zone.", NotificationType.Error);
                             }
                             return;
                         }*/
                    }
                }
            }

            // Get all guilds for overlap checking
            var modSystem = serverApi?.ModLoader.GetModSystem<SOAGuildsAndKingdomsModSystem>();
            var allGuilds = modSystem?.GetGuildRepository()?.GetAllGuilds();

            if (allGuilds != null && allGuilds.Count > 0)
            {
                var guildDict = allGuilds.ToDictionary(g => g.Name, g => g);

                // Check distance to other guilds' claims (300 block minimum)
                var (tooClose, nearestGuild, distance) = IsChunkTooCloseToOtherGuildClaim(chunkX, chunkZ, guild.Name, guildDict);

                if (tooClose)
                {
                    SendNotification(player, $"Cannot claim chunk ({chunkX}, {chunkZ}). Too close to {nearestGuild}'s territory ({distance:F0} blocks, minimum 300 blocks required).", NotificationType.Error);
                    return;
                }

                if (needsGuildHome)
                {
                    // For guild home establishment, check if 2x2 area starting from current chunk is free
                    // Also check distance for all 4 chunks of the guild home
                    for (int dx = 0; dx <= 1; dx++)
                    {
                        for (int dz = 0; dz <= 1; dz++)
                        {
                            int checkChunkX = chunkX + dx;
                            int checkChunkZ = chunkZ + dz;

                            // Check distance for each chunk of the guild home
                            var (homeChunkTooClose, homeNearestGuild, homeDistance) = IsChunkTooCloseToOtherGuildClaim(checkChunkX, checkChunkZ, guild.Name, guildDict);

                            if (homeChunkTooClose)
                            {
                                SendNotification(player, $"Cannot establish guild home at chunk ({chunkX},{chunkZ}). Part of the 2x2 area is too close to {homeNearestGuild}'s territory ({homeDistance:F0} blocks, minimum 300 blocks required).", NotificationType.Error);
                                return;
                            }

                            foreach (var otherGuild in guildDict.Values)
                            {
                                if (otherGuild.Claims.Any(c => c.ContainsChunk(checkChunkX, checkChunkZ)))
                                {
                                    SendNotification(player, $"Cannot establish guild home at chunk ({chunkX},{chunkZ}). The 2x2 area conflicts with existing claims from guild '{otherGuild.Name}'.", NotificationType.Error);
                                    return;
                                }
                            }
                        }
                    }

                    // For guild home, we only use 1 claim slot (the GuildHomeClaim itself)
                    var maxClaims = guildManager.GetMaxClaimsPerGuild(guild);
                    var currentClaims = guildManager.GetNonOutpostClaimCount(guild);
                    if (currentClaims + 1 > maxClaims)
                    {
                        SendNotification(player, $"Cannot establish guild home. Your guild can only have {maxClaims} total claims.", NotificationType.Error);
                        return;
                    }
                }
                else
                {
                    // Regular single chunk claim - check if chunk is already claimed
                    foreach (var otherGuild in guildDict.Values)
                    {
                        if (otherGuild.Claims.Any(c => c.ContainsChunk(chunkX, chunkZ)))
                        {
                            SendNotification(player, $"Cannot claim chunk ({chunkX}, {chunkZ}). It is already claimed by guild '{otherGuild.Name}'.", NotificationType.Error);
                            return;
                        }
                    }

                    // Check adjacency requirement for non-home claims (skip for outposts)
                    if (!packet.IsOutpost && !GuildManager.IsChunkAdjacentToGuildClaims(guild, chunkX, chunkZ))
                    {
                        SendNotification(player, $"Cannot claim chunk ({chunkX}, {chunkZ}). New claims must be adjacent to existing guild claims, or create an outpost instead.", NotificationType.Error);
                        return;
                    }
                }
            }

            // Attempt to claim the land with outpost information
            string error;
            bool success = guildManager.ClaimLand(guild.Name, player.PlayerUID, packet.BlockX, packet.BlockZ, packet.IsOutpost, packet.OutpostName, out error);

            if (success)
            {
                if (needsGuildHome)
                {
                    var currentClaims = guildManager.GetNonOutpostClaimCount(guild);
                    var maxClaims = guildManager.GetMaxClaimsPerGuild(guild);
                    SendNotification(player, $"Established guild home at chunk ({chunkX},{chunkZ}) with a 2x2 area. Claims: {currentClaims}/{maxClaims}", NotificationType.Success);
                }
                else if (packet.IsOutpost)
                {
                    var currentOutposts = guildManager.GetOutpostClaimCount(guild);
                    var maxOutposts = guildManager.GetMaxOutpostsPerGuild(guild);
                    string outpostNameText = string.IsNullOrEmpty(packet.OutpostName) ? "" : $" '{packet.OutpostName}'";
                    SendNotification(player, $"Established outpost{outpostNameText} at chunk ({chunkX}, {chunkZ}). Outposts: {currentOutposts}/{maxOutposts}", NotificationType.Success);
                }
                else
                {
                    var currentClaims = guildManager.GetNonOutpostClaimCount(guild);
                    var maxClaims = guildManager.GetMaxClaimsPerGuild(guild);
                    SendNotification(player, $"Claimed chunk ({chunkX}, {chunkZ}) for guild '{guild.Name}'. Claims: {currentClaims}/{maxClaims}", NotificationType.Success);
                }
                BroadcastGuildSummariesToAll();
            }
            else
            {
                // More detailed error message based on the likely reason for failure
                if (!needsGuildHome && !packet.IsOutpost && !GuildManager.IsChunkAdjacentToGuildClaims(guild, chunkX, chunkZ))
                {
                    SendNotification(player, $"Could not claim chunk ({chunkX}, {chunkZ}). New claims must be adjacent to existing guild claims, or create an outpost instead.", NotificationType.Error);
                }
                else if (error != null)
                {
                    SendNotification(player, $"Could not claim land at chunk ({chunkX}, {chunkZ}). {error}", NotificationType.Error);
                }
                else
                {
                    SendNotification(player, $"Could not claim land at chunk ({chunkX}, {chunkZ}). An unexpected error occurred.", NotificationType.Error);
                }
            }
        }

        private void OnGuildUnclaimLandReceived(IServerPlayer player, GuildUnclaimLandPacket packet)
        {
            if (guildManager == null || serverApi == null) return;

            var playerUid = player.PlayerUID;
            var guild = guildManager.GetGuildByMember(playerUid);

            if (guild == null)
            {
                SendNotification(player, "You are not in a guild.", NotificationType.Error);
                return;
            }

            // Check permissions
            if (!GuildManager.HasPermission(guild, playerUid, GuildPermission.ManageRoles))
            {
                SendNotification(player, "You don't have permission to unclaim land.", NotificationType.Error);
                return;
            }

            int chunkX = packet.BlockX / guilds.LandClaim.ChunkSize;
            int chunkZ = packet.BlockZ / guilds.LandClaim.ChunkSize;

            var result = guildManager.UnclaimLand(guild.Name, chunkX, chunkZ);

            if (result.Success)
            {
                SendNotification(player, $"Chunk ({chunkX}, {chunkZ}) unclaimed successfully.", NotificationType.Success);

                // Broadcast guild update to all players
                BroadcastGuildSummariesToAll();
            }
            else
            {
                SendNotification(player, result.ErrorMessage ?? "Failed to unclaim land.", NotificationType.Error);
            }
        }

        private void OnGuildRoleManagementReceived(IServerPlayer player, GuildRoleManagementPacket packet)
        {
            var guild = guildManager.GetGuildByMember(player.PlayerUID);
            if (guild == null)
            {
                SendNotification(player, "You are not in a guild.", NotificationType.Error);
                return;
            }

            bool success = false;
            string message = "";

            switch (packet.Action?.ToLowerInvariant())
            {
                case "create":
                    var permissions = ParsePermissionString(packet.PermissionString);
                    success = guildManager.CreateRole(guild.Name, player.PlayerUID, packet.RoleName, "", permissions, packet.Hierarchy);
                    if (success)
                    {
                        message = $"Role '{packet.RoleName}' created with hierarchy {packet.Hierarchy}.";
                    }
                    else
                    {
                        // Check if it's a hierarchy issue
                        var playerHierarchy = GuildManager.GetPlayerHierarchy(guild, player.PlayerUID);
                        if (packet.Hierarchy <= playerHierarchy)
                        {
                            message = $"Cannot create role with hierarchy {packet.Hierarchy}. It must be greater than your current hierarchy ({playerHierarchy}).";
                        }
                        else
                        {
                            message = "Could not create role.";
                        }
                    }
                    break;

                case "update":
                    var newPermissions = ParsePermissionString(packet.PermissionString);

                    // Check hierarchy before updating
                    if (!guildManager.CanManageRole(guild, player.PlayerUID, packet.RoleName))
                    {
                        var playerHierarchy = GuildManager.GetPlayerHierarchy(guild, player.PlayerUID);
                        var roleHierarchy = GuildManager.GetRoleHierarchy(guild, packet.RoleName);
                        message = $"Cannot update role '{packet.RoleName}'. Your hierarchy ({playerHierarchy}) must be lower than the role's hierarchy ({roleHierarchy}).";
                        break;
                    }

                    // Check if hierarchy is being updated
                    if (packet.Hierarchy != 999 && packet.Hierarchy != GuildManager.GetRoleHierarchy(guild, packet.RoleName))
                    {
                        // Validate new hierarchy
                        var playerHierarchy = GuildManager.GetPlayerHierarchy(guild, player.PlayerUID);
                        if (packet.Hierarchy <= playerHierarchy)
                        {
                            message = $"Cannot set hierarchy to {packet.Hierarchy}. It must be greater than your current hierarchy ({playerHierarchy}).";
                            break;
                        }

                        // Update with new hierarchy
                        success = guildManager.UpdateRolePermissions(guild.Name, player.PlayerUID, packet.RoleName, newPermissions, packet.Hierarchy);
                        message = success ? $"Updated role '{packet.RoleName}' permissions and hierarchy." : "Could not update role.";
                    }
                    else
                    {
                        // Update only permissions
                        success = guildManager.UpdateRolePermissions(guild.Name, player.PlayerUID, packet.RoleName, newPermissions);
                        message = success ? $"Updated role '{packet.RoleName}' permissions." : "Could not update role permissions.";
                    }
                    break;

                case "remove":
                    // Prevent removing default roles
                    if (packet.RoleName == "Leader" || packet.RoleName == "Member")
                    {
                        message = "Cannot remove default roles 'Leader' or 'Member'.";
                        break;
                    }

                    // Check if player has ManageRoles permission
                    if (!GuildManager.HasPermission(guild, player.PlayerUID, GuildPermission.ManageRoles))
                    {
                        message = "You don't have permission to remove roles.";
                        break;
                    }

                    // Check hierarchy before removing
                    if (!guildManager.CanManageRole(guild, player.PlayerUID, packet.RoleName))
                    {
                        var playerHierarchy = GuildManager.GetPlayerHierarchy(guild, player.PlayerUID);
                        var roleHierarchy = GuildManager.GetRoleHierarchy(guild, packet.RoleName);
                        message = $"Cannot remove role '{packet.RoleName}'. Your hierarchy ({playerHierarchy}) must be lower than the role's hierarchy ({roleHierarchy}).";
                        break;
                    }

                    if (!guild.Roles.ContainsKey(packet.RoleName))
                    {
                        message = $"Role '{packet.RoleName}' does not exist.";
                        break;
                    }

                    // Reassign members who had this role to "Member"
                    foreach (var member in guild.Members.Values)
                    {
                        if (member.Role == packet.RoleName)
                        {
                            member.Role = "Member";
                        }
                    }

                    guild.Roles.Remove(packet.RoleName);
                    var guildRepo = serverApi?.ModLoader.GetModSystem<SOAGuildsAndKingdomsModSystem>()?.GetGuildRepository();
                    guildRepo?.MarkDirty(guild.Name);
                    success = true;
                    message = $"Removed role '{packet.RoleName}' and reassigned affected members to 'Member'.";
                    break;

                case "assign":
                    // Find target player by name within the guild members
                    string? targetPlayerUid = FindGuildMemberByName(guild, packet.TargetPlayerName);

                    if (targetPlayerUid != null)
                    {
                        // Check if the role can be managed by this player
                        if (!guildManager.CanManageRole(guild, player.PlayerUID, packet.RoleName))
                        {
                            var playerHierarchy = GuildManager.GetPlayerHierarchy(guild, player.PlayerUID);
                            var roleHierarchy = GuildManager.GetRoleHierarchy(guild, packet.RoleName);
                            message = $"Cannot assign role '{packet.RoleName}'. Your hierarchy ({playerHierarchy}) must be lower than the role's hierarchy ({roleHierarchy}).";
                            break;
                        }

                        // Check if the target player can be acted upon
                        if (!guildManager.CanActOnPlayer(guild, player.PlayerUID, targetPlayerUid))
                        {
                            var playerHierarchy = GuildManager.GetPlayerHierarchy(guild, player.PlayerUID);
                            var targetHierarchy = GuildManager.GetPlayerHierarchy(guild, targetPlayerUid);
                            message = $"Cannot change role of {packet.TargetPlayerName}. Your hierarchy ({playerHierarchy}) must be lower than their current hierarchy ({targetHierarchy}).";
                            break;
                        }

                        success = guildManager.PromoteMember(guild.Name, player.PlayerUID, targetPlayerUid, packet.RoleName);
                        message = success ? $"Set role of {packet.TargetPlayerName} to '{packet.RoleName}'." : "Could not set role.";
                    }
                    else
                    {
                        message = $"Player '{packet.TargetPlayerName}' not found in guild.";
                    }
                    break;
            }

            SendNotification(player, message, success ? NotificationType.Success : NotificationType.Error);

            if (success)
            {
                BroadcastGuildSummariesToAll();
            }
        }

        private void OnTechContributionRequestReceived(IServerPlayer player, TechContributionRequestPacket packet)
        {
            var guild = guildManager?.GetGuild(packet.GuildName);
            if (guild == null)
            {
                SendTechContributionResponse(player, false, "Guild not found.", packet.TechBlockId, false, new Dictionary<string, int>());
                return;
            }

            // Verify player is in the guild
            if (!guild.Members.ContainsKey(player.PlayerUID))
            {
                SendTechContributionResponse(player, false, "You are not a member of this guild.", packet.TechBlockId, false, new Dictionary<string, int>());
                return;
            }

            // Check if player is within guild claimed land
            var playerPos = player.Entity?.ServerPos;
            if (playerPos != null)
            {
                int playerBlockX = (int)playerPos.X;
                int playerBlockZ = (int)playerPos.Z;

                if (!guildManager.IsPlayerInGuildClaim(guild, playerBlockX, playerBlockZ))
                {
                    SendTechContributionResponse(player, false, "You must be within guild claimed land to contribute to research.", packet.TechBlockId, false, new Dictionary<string, int>());
                    return;
                }
            }

            // Get the mod system to access tech blocks
            var modSystem = serverApi.ModLoader.GetModSystem<SOAGuildsAndKingdomsModSystem>();
            if (modSystem == null)
            {
                SendTechContributionResponse(player, false, "System error: mod system not found.", packet.TechBlockId, false, new Dictionary<string, int>());
                return;
            }

            var techBlock = modSystem.TechBlocks.FirstOrDefault(t => t.Id == packet.TechBlockId);
            if (techBlock == null)
            {
                SendTechContributionResponse(player, false, "Tech block not found.", packet.TechBlockId, false, new Dictionary<string, int>());
                return;
            }

            // Get tech progress directly from guild object
            var progress = guild.GetOrCreateTechProgress(packet.TechBlockId);

            if (progress.IsUnlocked)
            {
                SendTechContributionResponse(player, false, "This technology is already unlocked.", packet.TechBlockId, false, new Dictionary<string, int>());
                return;
            }

            // Get scaled requirements for this guild (accounts for guild size scaling)
            var guildTechManager = modSystem.GuildTechManager;
            var baseRequirements = new Dictionary<string, int>();
            foreach (var rg in techBlock.ResourceGroups)
            {
                baseRequirements[rg.Name] = rg.AmountRequired;
            }
            var scaledRequirements = guildTechManager?.GetScaledRequirements(guild.Name, baseRequirements) ?? baseRequirements;

            // Validate and process each item
            int totalItemsContributed = 0;
            var playerEntity = player.Entity;
            if (playerEntity == null)
            {
                SendTechContributionResponse(player, false, "Player entity not found.", packet.TechBlockId, false, new Dictionary<string, int>());
                return;
            }

            foreach (var item in packet.Items)
            {
                // Find the inventory and slot
                IInventory? inventory = null;
                foreach (var inv in playerEntity.Player.InventoryManager.Inventories.Values)
                {
                    if (inv.InventoryID == item.InventoryId)
                    {
                        inventory = inv;
                        break;
                    }
                }

                if (inventory == null || item.SlotId < 0 || item.SlotId >= inventory.Count)
                {
                    continue; // Skip invalid slots
                }

                var slot = inventory[item.SlotId];
                if (slot.Empty)
                {
                    continue; // Slot is empty
                }

                var itemStack = slot.Itemstack;
                var itemCode = itemStack.Collectible.Code.ToString();

                // Verify the item matches what was requested
                if (itemCode != item.ItemCode)
                {
                    continue; // Item mismatch, skip
                }

                // Find the resource group
                var resourceGroup = techBlock.ResourceGroups.FirstOrDefault(rg => rg.Name == item.ResourceGroupName);
                if (resourceGroup == null)
                {
                    continue; // Invalid resource group
                }

                // Verify item matches the resource group
                if (!resourceGroup.DoesItemMatch(itemCode))
                {
                    continue; // Item doesn't match resource group
                }

                // Check how much we can still accept for this resource group (use scaled requirements)
                var currentSubmitted = progress.GetResourceGroupSubmitted(resourceGroup.Name);
                var scaledRequired = scaledRequirements.ContainsKey(resourceGroup.Name) ? scaledRequirements[resourceGroup.Name] : resourceGroup.AmountRequired;
                var remaining = scaledRequired - currentSubmitted;

                if (remaining <= 0)
                {
                    continue; // This resource group is already fulfilled
                }

                // Calculate amount to actually take (can't take more than available or more than needed)
                var amountToTake = Math.Min(Math.Min(item.Amount, itemStack.StackSize), remaining);

                if (amountToTake > 0)
                {
                    // Remove items from inventory
                    itemStack.StackSize -= amountToTake;
                    totalItemsContributed += amountToTake;

                    // Update progress
                    if (progress.ResourceGroupsSubmitted.ContainsKey(resourceGroup.Name))
                    {
                        progress.ResourceGroupsSubmitted[resourceGroup.Name] += amountToTake;
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

            if (totalItemsContributed == 0)
            {
                SendTechContributionResponse(player, false, "No valid items were contributed.", packet.TechBlockId, false, new Dictionary<string, int>());
                return;
            }

            // Check if tech is now complete (using scaled requirements)
            bool isComplete = true;
            foreach (var resourceGroup in techBlock.ResourceGroups)
            {
                var submitted = progress.GetResourceGroupSubmitted(resourceGroup.Name);
                var scaledRequired = scaledRequirements.ContainsKey(resourceGroup.Name) ? scaledRequirements[resourceGroup.Name] : resourceGroup.AmountRequired;
                if (submitted < scaledRequired)
                {
                    isComplete = false;
                    break;
                }
            }

            string message;
            if (isComplete && !progress.IsUnlocked)
            {
                // Use GuildTechManager to unlock, which handles personal unlock requirements for large guilds
                var techManager = modSystem.GuildTechManager;
                techManager.UnlockTech(guild.Name, techBlock.Id, techBlock);

                message = $"Research complete! {techBlock.Text} unlocked!";

                // Sync traits for all guild members when tech is unlocked
                guildManager.SyncGuildMemberTraits(guild);
                serverApi.Logger.Notification($"[GuildNetworkHandler] Tech {techBlock.Id} ({techBlock.Text}) unlocked for guild {guild.Name} - syncing member traits");
                // Add info about personal unlock if required
                if (guild.TechRequiresPersonalUnlock.TryGetValue(techBlock.Id, out bool requiresPersonal) && requiresPersonal)
                {
                    var personalReqs = techBlock.GetPersonalRequirements();
                    var totalPersonal = personalReqs.Values.Sum();
                    message += $" (Personal unlock required: each member must contribute 5% of resources)";
                }
            }
            else
            {
                message = $"Contributed {totalItemsContributed} items to research.";
            }

            // Broadcast updated guild summaries to all clients so they get the fresh tech progress
            BroadcastGuildSummariesToAll();

            // Send success response
            SendTechContributionResponse(player, true, message, packet.TechBlockId, isComplete, progress.ResourceGroupsSubmitted);
        }

        private void SendTechContributionResponse(IServerPlayer player, bool success, string message, int techBlockId, bool techUnlocked, Dictionary<string, int> updatedProgress)
        {
            var response = new TechContributionResponsePacket
            {
                PlayerUid = player.PlayerUID,
                Success = success,
                Message = message,
                TechBlockId = techBlockId,
                TechUnlocked = techUnlocked,
                UpdatedProgress = updatedProgress
            };

            serverApi.Network.GetChannel(ChannelName).SendPacket(response, player);
        }

        // Client-side packet handlers
        private void OnGuildSyncReceived(GuildSyncPacket packet)
        {
            onGuildSummariesReceived?.Invoke(packet.GuildSummaries);
        }

        private void OnGuildUpdateReceived(GuildUpdatePacket packet)
        {
            // Handle individual guild updates
            onGuildSummariesReceived?.Invoke(new List<GuildSummary> { packet.UpdatedGuild });
        }

        private void OnGuildNotificationReceived(GuildNotificationPacket packet)
        {
            onNotificationReceived?.Invoke(packet.Message, packet.Type);
        }

        private void OnGuildInviteNotificationReceived(GuildInviteNotificationPacket packet)
        {
            clientApi?.Logger.Notification($"Received guild invite from {packet.InviterName} to join {packet.GuildName}");

            // Get mod system to show the popup
            var modSystem = clientApi?.ModLoader.GetModSystem<SOAGuildsAndKingdomsModSystem>();
            modSystem?.ShowInvitePopup(packet);

            // Also show chat notification
            string message = $"You have been invited to join '{packet.GuildName}' by {packet.InviterName}. Check bottom-right for invite popup.";
            onNotificationReceived?.Invoke(message, NotificationType.Info);
        }

        private void OnGuildInviteListResponseReceived(GuildInviteListResponsePacket packet)
        {
            clientApi?.Logger.Notification($"Received invite list with {packet.Invites.Count} invites");

            // Get mod system to show the popup with all invites
            var modSystem = clientApi?.ModLoader.GetModSystem<SOAGuildsAndKingdomsModSystem>();
            modSystem?.ShowInviteListPopup(packet.Invites);
        }

        private void OnGuildMemberListReceived(GuildMemberListPacket packet)
        {
            onMemberListReceived?.Invoke(packet.Members);
        }

        private void OnTechContributionResponseReceived(TechContributionResponsePacket packet)
        {
            onTechContributionResponseReceived?.Invoke(packet);
        }

        // Helper methods
        public void BroadcastGuildSummaries(IServerPlayer player)
        {
            var summaries = guildManager.GetGuildSummariesForPlayer(player.PlayerUID);
            var packet = new GuildSyncPacket
            {
                PlayerUid = player.PlayerUID,
                GuildSummaries = summaries
            };

            serverApi.Network.GetChannel(ChannelName).SendPacket(packet, player);
        }

        public void BroadcastGuildSummariesToAll()
        {
            foreach (var player in serverApi!.World.AllOnlinePlayers)
            {
                if (player is IServerPlayer serverPlayer)
                {
                    BroadcastGuildSummaries(serverPlayer);
                }
            }
        }

        public void SendNotification(IServerPlayer player, string message, NotificationType type)
        {
            var packet = new GuildNotificationPacket
            {
                PlayerUid = player.PlayerUID,
                Message = message,
                Type = type
            };

            serverApi.Network.GetChannel(ChannelName).SendPacket(packet, player);
        }

        // Client-side methods for sending packets to server
        public void SendGuildCreateRequest(string guildName, string description = "")
        {
            if (clientApi == null) return;

            var packet = new GuildCreatePacket
            {
                PlayerUid = clientApi.World.Player.PlayerUID,
                GuildName = guildName,
                Description = description
            };

            clientApi.Network.GetChannel(ChannelName).SendPacket(packet);
        }

        public void SendGuildInviteRequest(string targetPlayerUid)
        {
            if (clientApi == null) return;

            var packet = new GuildInvitePacket
            {
                PlayerUid = clientApi.World.Player.PlayerUID,
                TargetPlayerUid = targetPlayerUid
            };

            clientApi.Network.GetChannel(ChannelName).SendPacket(packet);
        }

        public void SendGuildAcceptRequest()
        {
            if (clientApi == null) return;

            var packet = new GuildAcceptInvitePacket
            {
                PlayerUid = clientApi.World.Player.PlayerUID
            };

            clientApi.Network.GetChannel(ChannelName).SendPacket(packet);
        }

        public void SendCancelInviteRequest(string inviteeUid)
        {
            if (clientApi == null) return;

            var packet = new GuildCancelInvitePacket
            {
                PlayerUid = clientApi.World.Player.PlayerUID,
                InviteeUid = inviteeUid
            };

            clientApi.Network.GetChannel(ChannelName).SendPacket(packet);
        }

        public void SendGuildRemoveMemberRequest(string targetPlayerName)
        {
            if (clientApi == null) return;

            var packet = new GuildRemoveMemberPacket
            {
                PlayerUid = clientApi.World.Player.PlayerUID,
                TargetPlayerName = targetPlayerName
            };

            clientApi.Network.GetChannel(ChannelName).SendPacket(packet);
        }

        public void SendGuildLeaveRequest()
        {
            if (clientApi == null) return;

            var packet = new GuildLeavePacket
            {
                PlayerUid = clientApi.World.Player.PlayerUID
            };

            clientApi.Network.GetChannel(ChannelName).SendPacket(packet);
        }

        public void SendGuildClaimLandRequest(int chunkX, int chunkZ)
        {
            SendGuildClaimLandRequest(chunkX, chunkZ, false, "");
        }

        public void SendGuildClaimLandRequest(int chunkX, int chunkZ, bool isOutpost, string outpostName = "")
        {
            if (clientApi == null)
                return;

            // Convert chunk coordinates to block coordinates for the packet
            int blockX = chunkX * SOAGuildsAndKingdoms.src.guilds.LandClaim.ChunkSize + (SOAGuildsAndKingdoms.src.guilds.LandClaim.ChunkSize / 2);
            int blockZ = chunkZ * SOAGuildsAndKingdoms.src.guilds.LandClaim.ChunkSize + (SOAGuildsAndKingdoms.src.guilds.LandClaim.ChunkSize / 2);

            var packet = new GuildClaimLandPacket
            {
                PlayerUid = clientApi.World.Player.PlayerUID,
                BlockX = blockX,
                BlockZ = blockZ,
                IsOutpost = isOutpost,
                OutpostName = outpostName ?? ""
            };

            clientApi.Network.GetChannel(ChannelName).SendPacket(packet);
        }

        public void SendGuildUnclaimLandRequest(int chunkX, int chunkZ)
        {
            if (clientApi == null)
                return;

            // Convert chunk coordinates to block coordinates for the packet
            int blockX = chunkX * SOAGuildsAndKingdoms.src.guilds.LandClaim.ChunkSize + (SOAGuildsAndKingdoms.src.guilds.LandClaim.ChunkSize / 2);
            int blockZ = chunkZ * SOAGuildsAndKingdoms.src.guilds.LandClaim.ChunkSize + (SOAGuildsAndKingdoms.src.guilds.LandClaim.ChunkSize / 2);

            var packet = new GuildUnclaimLandPacket
            {
                PlayerUid = clientApi.World.Player.PlayerUID,
                BlockX = blockX,
                BlockZ = blockZ
            };

            clientApi.Network.GetChannel(ChannelName).SendPacket(packet);
        }

        public void SendGuildRoleManagementRequest(string action, string roleName, string? targetPlayerName = null, string? permissionString = null, int hierarchy = 999)
        {
            if (clientApi == null) return;

            var packet = new GuildRoleManagementPacket
            {
                PlayerUid = clientApi.World.Player.PlayerUID,
                Action = action,
                RoleName = roleName,
                TargetPlayerName = targetPlayerName ?? "",
                PermissionString = permissionString ?? "",
                Hierarchy = hierarchy
            };

            clientApi.Network.GetChannel(ChannelName).SendPacket(packet);
        }

        public void SendGuildMemberListRequest()
        {
            if (clientApi == null) return;

            var packet = new GuildMemberListRequestPacket
            {
                PlayerUid = clientApi.World.Player.PlayerUID
            };

            clientApi.Network.GetChannel(ChannelName).SendPacket(packet);
        }

        public void SendGuildTransferOwnershipRequest(string targetPlayerUid)
        {
            if (clientApi == null) return;

            var packet = new GuildTransferOwnershipPacket
            {
                PlayerUid = clientApi.World.Player.PlayerUID,
                TargetPlayerUid = targetPlayerUid
            };

            clientApi.Network.GetChannel(ChannelName).SendPacket(packet);
        }

        public void RegisterMemberListCallback(Action<List<GuildMemberInfo>> callback)
        {
            onMemberListReceived = callback;
        }

        public void UnregisterMemberListCallback()
        {
            onMemberListReceived = null;
        }

        public void SendTechContributionRequest(string guildName, int techBlockId, List<ContributionItemDto> items)
        {
            if (clientApi == null) return;

            var packet = new TechContributionRequestPacket
            {
                PlayerUid = clientApi.World.Player.PlayerUID,
                GuildName = guildName,
                TechBlockId = techBlockId,
                Items = items
            };

            clientApi.Network.GetChannel(ChannelName).SendPacket(packet);
        }

        public void SendPersonalTechContributionRequest(string guildName, int techBlockId, List<ContributionItemDto> items)
        {
            if (clientApi == null) return;

            var packet = new PersonalTechContributionRequestPacket
            {
                PlayerUid = clientApi.World.Player.PlayerUID,
                GuildName = guildName,
                TechBlockId = techBlockId,
                Items = items
            };

            clientApi.Network.GetChannel(ChannelName).SendPacket(packet);
        }

        public void RegisterTechContributionCallback(Action<TechContributionResponsePacket> callback)
        {
            onTechContributionResponseReceived = callback;
        }

        public void UnregisterTechContributionCallback()
        {
            onTechContributionResponseReceived = null;
        }

        // Helper method to find a guild member's UID by player name
        private string? FindGuildMemberByName(Guild guild, string playerName)
        {
            if (guild == null || string.IsNullOrWhiteSpace(playerName)) return null;

            var onlinePlayers = serverApi!.World.AllOnlinePlayers;

            // First check online players (more efficient and always up-to-date)
            var onlinePlayer = onlinePlayers.FirstOrDefault(p =>
                p.PlayerName.Equals(playerName, StringComparison.OrdinalIgnoreCase));

            if (onlinePlayer != null && guild.Members.ContainsKey(onlinePlayer.PlayerUID))
            {
                return onlinePlayer.PlayerUID;
            }

            // If not found among online players, check all guild members using stored player data
            foreach (var memberUid in guild.Members.Keys)
            {
                var playerData = serverApi.PlayerData.GetPlayerDataByUid(memberUid);
                if (playerData?.LastKnownPlayername?.Equals(playerName, StringComparison.OrdinalIgnoreCase) == true)
                {
                    return memberUid;
                }
            }

            return null;
        }

        private GuildPermission ParsePermissionString(string perms)
        {
            if (string.IsNullOrWhiteSpace(perms)) return GuildPermission.None;

            GuildPermission result = GuildPermission.None;
            var tokens = perms.Split(new[] { ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var t in tokens)
            {
                var tok = t.Trim().ToLowerInvariant();
                switch (tok)
                {
                    case "invite":
                        result |= GuildPermission.Invite;
                        break;
                    case "promote":
                        result |= GuildPermission.Promote;
                        break;
                    case "kick":
                        result |= GuildPermission.Kick;
                        break;
                    case "manageroles":
                    case "managerole":
                    case "manage":
                        result |= GuildPermission.ManageRoles;
                        break;
                    case "breakplaceblocks":
                        result |= GuildPermission.BreakAndPlaceBlocks;
                        break;
                    case "interactblocks":
                    case "interact":
                        result |= GuildPermission.InteractBlocks;
                        break;
                }
            }
            return result;
        }

        /// <summary>
        /// Check if a chunk is too close (within 300 blocks) to another guild's claim
        /// </summary>
        private (bool tooClose, string nearestGuildName, double distance) IsChunkTooCloseToOtherGuildClaim(
            int chunkX, int chunkZ, string currentGuildName, Dictionary<string, Guild> allGuilds)
        {
            const int minDistance = 300; // Minimum distance in blocks
            const int chunkSize = 32; // VintageStory chunk size

            // Calculate the center block position of the chunk being checked
            int centerBlockX = chunkX * chunkSize + chunkSize / 2;
            int centerBlockZ = chunkZ * chunkSize + chunkSize / 2;

            double nearestDistance = double.MaxValue;
            string nearestGuild = null;

            // Check all guilds
            foreach (var otherGuild in allGuilds.Values)
            {
                // Skip the current guild's claims
                if (otherGuild.Name == currentGuildName)
                {
                    continue;
                }

                // Check all claims from other guilds
                foreach (var claim in otherGuild.Claims)
                {
                    // Get chunks for this claim based on its type
                    List<(int x, int z)> claimChunks = new List<(int x, int z)>();

                    if (claim is GuildHomeClaim homeClaim)
                    {
                        // Guild home is a 2x2 area
                        for (int dx = 0; dx <= 1; dx++)
                        {
                            for (int dz = 0; dz <= 1; dz++)
                            {
                                claimChunks.Add((homeClaim.ChunkX + dx, homeClaim.ChunkZ + dz));
                            }
                        }
                    }
                    else if (claim is OutpostClaim outpostClaim)
                    {
                        // Outpost is a single chunk
                        claimChunks.Add((outpostClaim.ChunkX, outpostClaim.ChunkZ));
                    }
                    else
                    {
                        // Regular single chunk claim
                        claimChunks.Add((claim.ChunkX, claim.ChunkZ));
                    }

                    foreach (var (claimChunkX, claimChunkZ) in claimChunks)
                    {
                        // Calculate the center block position of the claimed chunk
                        int claimCenterX = claimChunkX * chunkSize + chunkSize / 2;
                        int claimCenterZ = claimChunkZ * chunkSize + chunkSize / 2;

                        // Calculate distance between the two chunk centers
                        double deltaX = centerBlockX - claimCenterX;
                        double deltaZ = centerBlockZ - claimCenterZ;
                        double distance = Math.Sqrt(deltaX * deltaX + deltaZ * deltaZ);

                        // Track the nearest claim
                        if (distance < nearestDistance)
                        {
                            nearestDistance = distance;
                            nearestGuild = otherGuild.Name;
                        }

                        // If within minimum distance, return immediately
                        if (distance < minDistance)
                        {
                            return (true, otherGuild.Name, distance);
                        }
                    }
                }
            }

            return (false, nearestGuild, nearestDistance);
        }

        // Config synchronization methods
        public void SendGuildConfig(IServerPlayer player)
        {
            var config = guildManager?.GetConfigManager()?.GetConfig();
            if (config == null)
            {
                serverApi?.Logger.Warning($"SendGuildConfig: config is null for player {player.PlayerName}");
                return;
            }

            // Get tech blocks config to include enabled ages
            var modSystem = serverApi?.ModLoader.GetModSystem<SOAGuildsAndKingdomsModSystem>();
            var enabledAges = modSystem?.TechBlocksConfig?.EnabledAges?.
                Select(age => (int)age).ToList() ?? [];

            ZoneWhitelistManager? zoneWhitelistManager = modSystem?.GetZoneWhitelistManager();
            NodeManager? nodeManager = modSystem?.GetNodeManager();

            serverApi?.Logger.Debug($"SendGuildConfig to {player.PlayerName}: EnableProtectedZones={config.EnableProtectedZones}, ProtectedZones count={config.ProtectedZones?.Count ?? 0}, EnabledAges count={enabledAges.Count}");

            var packet = new GuildConfigPacket
            {
                PlayerUid = player.PlayerUID,
                TerritorialRestrictionsEnabled = config.EnableTerritorialRestrictions,
                TerritorialCenterX = config.TerritorialCenter?.X,
                TerritorialCenterZ = config.TerritorialCenter?.Z,
                TerritorialRadius = config.TerritorialRadius,
                ProtectedZonesEnabled = config.EnableProtectedZones,
                ProtectedZones = config.ProtectedZones?.Select(z => new ProtectedZoneData
                {
                    Name = z.Name,
                    X = z.X,
                    Z = z.Z,
                    Radius = z.Radius,
                    WhitelistedPlayers = zoneWhitelistManager?.GetWhitelistedPlayers(z.Id) ?? []
                }).ToList() ?? [],
                Nodes = nodeManager?.GetNodesForNetworkPacket() ?? [],
                EnabledAges = enabledAges
            };

            serverApi?.Logger.Debug($"SendGuildConfig packet created: ProtectedZonesEnabled={packet.ProtectedZonesEnabled}, ProtectedZones count={packet.ProtectedZones?.Count ?? 0}");

            serverApi?.Network.GetChannel(ChannelName).SendPacket(packet, player);
        }

        public void BroadcastGuildConfigToAll()
        {
            foreach (var player in serverApi!.World.AllOnlinePlayers)
            {
                if (player is IServerPlayer serverPlayer)
                {
                    SendGuildConfig(serverPlayer);
                }
            }
        }

        private void OnGuildConfigReceived(GuildConfigPacket packet)
        {
            clientApi?.Logger.Debug($"OnGuildConfigReceived: ProtectedZonesEnabled={packet.ProtectedZonesEnabled}, ProtectedZones count={packet.ProtectedZones?.Count ?? 0}");
            onConfigReceived?.Invoke(packet);
        }

        public void RegisterConfigCallback(Action<GuildConfigPacket> callback)
        {
            onConfigReceived = callback;
        }

        // Scaled requirements support
        private void OnScaledRequirementsRequestReceived(IServerPlayer player, ScaledRequirementsRequestPacket packet)
        {
            var guild = guildManager?.GetGuild(packet.GuildName);
            if (guild == null)
            {
                SendScaledRequirementsResponse(player, new Dictionary<string, int>(), 1.0m, 0);
                return;
            }

            var techManager = modSystem?.GuildTechManager;
            if (techManager == null)
            {
                SendScaledRequirementsResponse(player, packet.BaseRequirements, 1.0m, guild.Members.Count);
                return;
            }

            // Calculate scaled requirements
            var scaledRequirements = techManager.GetScaledRequirements(packet.GuildName, packet.BaseRequirements);
            var resourceScaling = techManager.GetResourceScaling(guild.Members.Count);

            // Send response back to client
            SendScaledRequirementsResponse(player, scaledRequirements, resourceScaling, guild.Members.Count);
        }

        private void SendScaledRequirementsResponse(IServerPlayer player, Dictionary<string, int> scaledRequirements, decimal resourceScaling, int memberCount)
        {
            var response = new ScaledRequirementsResponsePacket
            {
                PlayerUid = player.PlayerUID,
                ScaledRequirements = scaledRequirements,
                ResourceScaling = resourceScaling,
                MemberCount = memberCount
            };

            serverApi?.Network.GetChannel(ChannelName).SendPacket(response, player);
        }

        // Client-side: Request scaled requirements from server
        public void RequestScaledRequirements(string guildName, int techBlockId, Dictionary<string, int> baseRequirements, Action<ScaledRequirementsResponsePacket> callback)
        {
            if (clientApi == null) return;

            // Store callback for when response arrives
            onScaledRequirementsResponseReceived = callback;

            var packet = new ScaledRequirementsRequestPacket
            {
                PlayerUid = clientApi.World.Player.PlayerUID,
                GuildName = guildName,
                TechBlockId = techBlockId,
                BaseRequirements = baseRequirements
            };

            clientApi.Network.GetChannel(ChannelName).SendPacket(packet);
        }

        // Client-side: Handle scaled requirements response from server
        private void OnScaledRequirementsResponseReceived(ScaledRequirementsResponsePacket packet)
        {
            onScaledRequirementsResponseReceived?.Invoke(packet);
        }

        // Tech blocks config synchronization methods
        public void SendTechBlocksConfig(IServerPlayer player)
        {
            if (serverApi == null)
            {
                return;
            }

            var modSystem = serverApi.ModLoader.GetModSystem<SOAGuildsAndKingdomsModSystem>();
            if (modSystem == null)
            {
                serverApi.Logger.Warning($"SendTechBlocksConfig: modSystem is null for player {player.PlayerName}");
                return;
            }

            var techBlocksConfig = modSystem.TechBlocksConfig;
            if (techBlocksConfig == null)
            {
                serverApi.Logger.Warning($"SendTechBlocksConfig: techBlocksConfig is null for player {player.PlayerName}");
                return;
            }

            // Serialize config to JSON
            var options = new System.Text.Json.JsonSerializerOptions
            {
                WriteIndented = true,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.Never
            };
            var configJson = System.Text.Json.JsonSerializer.Serialize(techBlocksConfig, options);

            // Get server identifier from guild config
            string serverIdentifier = "unknown";
            try
            {
                var config = guildManager?.GetConfigManager()?.GetConfig();
                if (config != null && !string.IsNullOrWhiteSpace(config.ServerName))
                {
                    serverIdentifier = config.ServerName;
                }
                // Fallback to world name if server name not configured
                else if (serverApi.WorldManager?.SaveGame?.WorldName != null)
                {
                    serverIdentifier = serverApi.WorldManager.SaveGame.WorldName;
                }
            }
            catch (Exception ex)
            {
                serverApi.Logger.Warning($"SendTechBlocksConfig: Failed to get server identifier: {ex.Message}");
            }

            var packet = new TechBlocksConfigSyncPacket
            {
                PlayerUid = player.PlayerUID,
                ConfigJson = configJson,
                ServerIdentifier = serverIdentifier
            };

            serverApi.Logger.Notification($"Sending TechBlocks config to player {player.PlayerName} (server: {serverIdentifier})");

            if (serverApi.Network == null)
            {
                serverApi.Logger.Error($"SendTechBlocksConfig: serverApi.Network is null for player {player.PlayerName}");
                return;
            }

            var channel = serverApi.Network.GetChannel(ChannelName);
            if (channel == null)
            {
                serverApi.Logger.Error($"SendTechBlocksConfig: Channel '{ChannelName}' not found for player {player.PlayerName}");
                return;
            }

            channel.SendPacket(packet, player);
        }

        public void BroadcastTechBlocksConfigToAll()
        {
            foreach (var player in serverApi!.World.AllOnlinePlayers)
            {
                if (player is IServerPlayer serverPlayer)
                {
                    SendTechBlocksConfig(serverPlayer);
                }
            }
        }

        private void OnTechBlocksConfigReceived(TechBlocksConfigSyncPacket packet)
        {
            clientApi?.Logger.Notification($"Received TechBlocks config from server (identifier: {packet.ServerIdentifier})");
            onTechBlocksConfigReceived?.Invoke(packet);
        }

        public void RegisterTechBlocksConfigCallback(Action<TechBlocksConfigSyncPacket> callback)
        {
            onTechBlocksConfigReceived = callback;
        }

        // Node Wars Data Methods

        /// <summary>
        /// Client sends request for node war data
        /// </summary>
        public void RequestNodeWarData(string guildName)
        {
            if (clientApi == null) return;

            var packet = new NodeWarDataRequestPacket
            {
                PlayerUid = clientApi.World.Player.PlayerUID,
                GuildName = guildName
            };

            var channel = clientApi.Network.GetChannel(ChannelName);
            channel?.SendPacket(packet);

            clientApi.Logger.Debug($"[Guild UI] Requesting node war data for guild: {guildName}");
        }

        /// <summary>
        /// Server handles node war data request - forwards to PVP mod if available
        /// </summary>
        private void OnNodeWarDataRequestReceived(IServerPlayer player, NodeWarDataRequestPacket packet)
        {
            if (serverApi == null) return;

            serverApi.Logger.Debug($"[Guild] Received node war data request from {player.PlayerName} for guild {packet.GuildName}");

            // Try to get the PVP mod system dynamically (without hard assembly reference)
            var pvpMod = serverApi.ModLoader.GetModSystem("SOAGuildsAndKingdomsPVP.PVPModSystem");
            if (pvpMod != null)
            {
                // Call PVP mod's node war data handler using reflection
                var modSystem = serverApi.ModLoader.GetModSystem<SOAGuildsAndKingdomsModSystem>();
                if (modSystem != null)
                {
                    var method = pvpMod.GetType().GetMethod("OnNodeWarDataRequested");
                    if (method != null)
                    {
                        method.Invoke(pvpMod, new object[] { player, packet.GuildName, modSystem });
                        serverApi.Logger.Debug($"[Guild] Forwarded data request to PVP mod");
                    }
                    else
                    {
                        serverApi.Logger.Warning($"[Guild] PVP mod found but OnNodeWarDataRequested method not available");
                    }
                }
            }
            else
            {
                serverApi.Logger.Debug($"[Guild] PVP mod not available - sending empty response");
                // Send empty response to indicate PVP mod is not installed
                var emptyPacket = new NodeWarDataResponsePacket
                {
                    PlayerUid = player.PlayerUID
                };
                SendNodeWarData(player, emptyPacket);
            }
        }

        /// <summary>
        /// Server sends node war data response to client (called by PVP mod)
        /// </summary>
        public void SendNodeWarData(IServerPlayer player, NodeWarDataResponsePacket packet)
        {
            if (serverApi == null) return;

            var channel = serverApi.Network.GetChannel(ChannelName);
            if (channel == null)
            {
                serverApi.Logger.Error($"SendNodeWarData: Channel '{ChannelName}' not found");
                return;
            }

            packet.PlayerUid = player.PlayerUID;
            channel.SendPacket(packet, player);
            serverApi.Logger.Debug($"[Guild] Sent node war data to {player.PlayerName}");
        }

        /// <summary>
        /// Client receives node war data response
        /// </summary>
        private void OnNodeWarDataResponseReceived(NodeWarDataResponsePacket packet)
        {
            clientApi?.Logger.Debug($"[Guild UI] Received node war data response");
            onNodeWarDataReceived?.Invoke(packet);
        }

        /// <summary>
        /// Register callback for when node war data is received (client-side)
        /// </summary>
        public void RegisterNodeWarDataCallback(Action<NodeWarDataResponsePacket> callback)
        {
            onNodeWarDataReceived = callback;
        }
    }
}
