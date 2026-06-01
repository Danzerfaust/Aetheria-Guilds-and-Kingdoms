using SRGuildsAndKingdoms.src.config;
using SRGuildsAndKingdoms.src.database;
using SRGuildsAndKingdoms.src.player;
using SRGuildsAndKingdoms.src.techblock;
using System;
using System.Collections.Generic;
using System.Linq;
using Vintagestory.API.Server;

namespace SRGuildsAndKingdoms.src.guilds
{
    public class GuildManager
    {
        private readonly GuildRepository repository;
        private readonly CooldownRepository cooldownRepository;
        private readonly LandClaimRepository? landClaimRepository;
        private readonly ICoreServerAPI sapi;

        // Configuration manager for dynamic settings
        private readonly GuildConfigManager configManager;

        // Trait manager for syncing guild tech traits to players
        private GuildTraitManager? traitManager;

        /// <summary>
        /// Gets the maximum number of claims allowed for a specific guild based on its member count
        /// Excludes outposts from the count
        /// </summary>
        /// <param name="guild">The guild to calculate max claims for</param>
        public int GetMaxClaimsPerGuild(Guild guild)
        {
            if (guild == null) return 0;
            return configManager.GetMaxClaimsPerGuild(guild.Members.Count);
        }

        /// <summary>
        /// Gets the maximum number of outposts allowed for a specific guild based on its member count
        /// </summary>
        /// <param name="guild">The guild to calculate max outposts for</param>
        public int GetMaxOutpostsPerGuild(Guild guild)
        {
            if (guild == null) return 0;
            return configManager.GetMaxOutpostsPerGuild(guild.Members.Count);
        }

        /// <summary>
        /// Gets the maximum number of members allowed per guild
        /// </summary>
        public int GetMaxMembersPerGuild()
        {
            return configManager.GetConfig().MaxMembersPerGuild;
        }

        /// <summary>
        /// Gets the current number of non-outpost claims for a guild
        /// </summary>
        public int GetNonOutpostClaimCount(Guild guild)
        {
            if (guild == null) return 0;
            return guild.Claims.Count(c => !(c is OutpostClaim));
        }

        /// <summary>
        /// Gets the current number of outpost claims for a guild
        /// </summary>
        public int GetOutpostClaimCount(Guild guild)
        {
            if (guild == null) return 0;
            return guild.Claims.Count(c => c is OutpostClaim);
        }

        // Event fired on server when guilds/claims metadata changed so ModSystem can sync to clients
        public event Action? OnGuildsChanged;

        // Node Wars: Events for cross-mod communication with PVP mod
        /// <summary>
        /// Fired when a guild captures a node. Parameters: (guildName, nodeId, nodeName)
        /// </summary>
        public event Action<string, string, string>? OnNodeCaptured;

        /// <summary>
        /// Fired when a guild loses a node. Parameters: (guildName, nodeId, nodeName)
        /// </summary>
        public event Action<string, string, string>? OnNodeLost;

        /// <summary>
        /// Fired when a guild signs up for a node war. Parameters: (guildName, nodeId)
        /// </summary>
        public event Action<string, string>? OnGuildSignedUpForWar;

        /// <summary>
        /// Fired when a guild cancels a node war signup. Parameters: (guildName, nodeId)
        /// </summary>
        public event Action<string, string>? OnGuildCancelledWarSignup;

        public GuildManager(ICoreServerAPI sapi, GuildRepository repository, CooldownRepository cooldownRepository, LandClaimRepository? landClaimRepository = null)
        {
            this.sapi = sapi;
            this.repository = repository;
            this.cooldownRepository = cooldownRepository;
            this.landClaimRepository = landClaimRepository;
            this.configManager = new GuildConfigManager(sapi);

            // Initialize trait manager after mod system loads
            sapi.Event.SaveGameLoaded += () =>
             {
                 var modSystem = sapi.ModLoader.GetModSystem<SRGuildsAndKingdomsModSystem>();
                 if (modSystem != null)
                 {
                     traitManager = new GuildTraitManager(sapi, modSystem);
                     sapi.Logger.Notification("[GuildManager] Trait manager initialized successfully");
                 }
                 else
                 {
                     sapi.Logger.Warning("[GuildManager] Could not initialize trait manager - mod system not found");
                 }
             };
        }

        public void OnSaveGameLoading()
        {
            // Load configuration first
            configManager.LoadConfig();

            // Log current configuration status
            sapi.Logger.Notification(configManager.GetConfigStatus());

            // Log an example of how the system scales with player count
            LogScalingExamples();

            sapi.Logger.Notification($"[GuildManager] Loaded {repository.GetAllGuilds().Count} guilds from database");
        }

        public void OnSaveGameSaving()
        {
            // Commit all pending changes to db
            repository.CommitChanges();
        }

        /// <summary>
        /// Get the configuration manager
        /// </summary>
        public GuildConfigManager GetConfigManager()
        {
            return configManager;
        }

        public bool CreateGuild(string name, string creatorUid, string description = "")
        {
            if (repository.GetGuild(name) != null) return false;
            if (GetGuildByMember(creatorUid) != null) return false; // User already in a guild
            var guild = new Guild { Name = name };

            // Generate random colors for both main and secondary
            var colors = GenerateRandomColors();
            guild.DisplayColor = colors.primary;
            guild.SecondaryColor = colors.secondary;
            guild.Description = string.IsNullOrWhiteSpace(description) ? $"Guild {name}" : description;

            // ensure defaults exist (Guild constructor already sets defaults)
            guild.Members[creatorUid] = new GuildMember { PlayerUid = creatorUid, Role = "Leader" };
            repository.CreateGuild(guild);

            OnGuildsChanged?.Invoke();
            return true;
        }

        // Generate random colors for guild creation
        private static (int primary, int secondary) GenerateRandomColors()
        {
            var random = new Random();

            // Generate primary color with good saturation and brightness
            byte r1 = (byte)(random.Next(80, 255));
            byte g1 = (byte)(random.Next(80, 255));
            byte b1 = (byte)(random.Next(80, 255));
            int primary = (int)((0xFFu << 24) | ((uint)r1 << 16) | ((uint)g1 << 8) | (uint)b1);

            // Generate secondary color that complements the primary
            // Use a lighter/darker variant or complementary hue
            byte r2, g2, b2;
            if (random.NextDouble() > 0.5)
            {
                // Lighter variant
                r2 = (byte)Math.Min(255, r1 + 40);
                g2 = (byte)Math.Min(255, g1 + 40);
                b2 = (byte)Math.Min(255, b1 + 40);
            }
            else
            {
                // Darker variant
                r2 = (byte)Math.Max(60, r1 - 40);
                g2 = (byte)Math.Max(60, g1 - 40);
                b2 = (byte)Math.Max(60, b1 - 40);
            }

            int secondary = (int)((0xFFu << 24) | ((uint)r2 << 16) | ((uint)g2 << 8) | (uint)b2);

            return (primary, secondary);
        }

        public Guild? GetGuildByMember(string playerUid)
        {
            return repository.GetGuildByMember(playerUid);
        }

        public Guild? GetGuild(string name)
        {
            return repository.GetGuild(name);
        }

        /// <summary>
        /// Check if a player has a specific permission in their guild
        /// </summary>
        public static bool HasPermission(Guild guild, string playerUid, GuildPermission permission)
        {
            if (guild == null || !guild.Members.ContainsKey(playerUid)) return false;
            var roleName = guild.Members[playerUid].Role;
            if (!guild.Roles.TryGetValue(roleName, out var role)) return false;
            return (role.Permissions & permission) == permission;
        }

        /// <summary>
        /// Get the hierarchy level of a player's role in their guild
        /// Lower numbers = higher authority
        /// </summary>
        public static int GetPlayerHierarchy(Guild guild, string playerUid)
        {
            if (guild == null || !guild.Members.ContainsKey(playerUid)) return int.MaxValue;
            var roleName = guild.Members[playerUid].Role;
            if (!guild.Roles.TryGetValue(roleName, out var role)) return int.MaxValue;
            return role.Hierarchy;
        }

        /// <summary>
        /// Get the hierarchy level of a specific role in a guild
        /// Lower numbers = higher authority
        /// </summary>
        public static int GetRoleHierarchy(Guild guild, string roleName)
        {
            if (guild == null || !guild.Roles.TryGetValue(roleName, out var role)) return int.MaxValue;
            return role.Hierarchy;
        }

        /// <summary>
        /// Check if one player can perform actions on another based on hierarchy
        /// Returns true if actorUid has higher or equal authority (lower hierarchy number) than targetUid
        /// </summary>
        public bool CanActOnPlayer(Guild guild, string actorUid, string targetUid)
        {
            var actorHierarchy = GetPlayerHierarchy(guild, actorUid);
            var targetHierarchy = GetPlayerHierarchy(guild, targetUid);
            return actorHierarchy < targetHierarchy; // Actor must have strictly lower hierarchy number (higher authority)
        }

        /// <summary>
        /// Check if a player can manage a specific role based on hierarchy
        /// Returns true if the player's role has higher authority (lower hierarchy number) than the target role
        /// </summary>
        public bool CanManageRole(Guild guild, string playerUid, string targetRoleName)
        {
            var playerHierarchy = GetPlayerHierarchy(guild, playerUid);
            var roleHierarchy = GetRoleHierarchy(guild, targetRoleName);
            return playerHierarchy < roleHierarchy; // Player must have strictly lower hierarchy number (higher authority)
        }

        public bool InviteToGuild(string guildName, string inviterUid, string inviteeUid)
        {
            var guild = GetGuild(guildName);
            if (guild == null) return false;
            if (!HasPermission(guild, inviterUid, GuildPermission.Invite)) return false;
            if (guild.Members.ContainsKey(inviteeUid)) return false;
            if (guild.PendingInvites.Any(i => i.InviteeUid == inviteeUid)) return false;

            // Check if guild is at max capacity
            var maxMembers = configManager.GetConfig().MaxMembersPerGuild;
            if (guild.Members.Count >= maxMembers)
            {
                sapi.Logger.Notification($"[GuildManager] Cannot invite to guild '{guildName}': guild is at maximum capacity ({maxMembers} members)");
                return false;
            }

            // Create invite with 5 minute expiry
            var invite = new GuildInvite
            {
                InviterUid = inviterUid,
                InviteeUid = inviteeUid,
                GuildName = guildName,
                Timestamp = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddMinutes(5)
            };

            guild.PendingInvites.Add(invite);
            repository.MarkDirty(guildName);

            OnGuildsChanged?.Invoke();
            return true;
        }

        public bool AcceptInvite(string playerUid)
        {
            // Clean up expired invites first
            CleanupExpiredInvites();

            // Check if player is on cooldown
            if (IsPlayerOnCooldown(playerUid, out var remainingTime))
            {
                sapi.Logger.Notification($"[GuildManager] Player '{playerUid}' cannot join guild - on cooldown for {remainingTime.TotalHours:F1} more hours");
                return false;
            }

            var allGuilds = repository.GetAllGuilds();
            var invite = allGuilds.SelectMany(g => g.PendingInvites).FirstOrDefault(i => i.InviteeUid == playerUid && !i.IsExpired());
            if (invite == null) return false;
            var guild = allGuilds.First(g => g.PendingInvites.Contains(invite));

            // Check if guild is at max capacity before accepting
            var maxMembers = configManager.GetConfig().MaxMembersPerGuild;
            if (guild.Members.Count >= maxMembers)
            {
                sapi.Logger.Notification($"[GuildManager] Cannot accept invite to guild '{guild.Name}': guild is at maximum capacity ({maxMembers} members)");
                guild.PendingInvites.Remove(invite);
                OnGuildsChanged?.Invoke();
                return false;
            }

            guild.Members[playerUid] = new GuildMember { PlayerUid = playerUid, Role = "Member" };
            guild.PendingInvites.Remove(invite);
            repository.MarkDirty(guild.Name);

            // Sync traits for the newly joined player
            var player = sapi.World.PlayerByUid(playerUid) as IServerPlayer;
            if (player != null)
            {
                traitManager?.SyncPlayerTraits(player);
            }

            OnGuildsChanged?.Invoke();
            return true;
        }

        /// <summary>
        /// Decline a guild invite for a specific guild
        /// </summary>
        public bool DeclineInvite(string playerUid, string guildName)
        {
            var guild = GetGuild(guildName);
            if (guild == null) return false;

            var invite = guild.PendingInvites.FirstOrDefault(i => i.InviteeUid == playerUid);
            if (invite == null) return false;

            guild.PendingInvites.Remove(invite);
            repository.MarkDirty(guildName);
            OnGuildsChanged?.Invoke();
            return true;
        }

        /// <summary>
        /// Cancel a pending invite (by a guild member with invite permissions)
        /// </summary>
        public bool CancelInvite(string guildName, string cancellerUid, string inviteeUid, out string message)
        {
            message = "";
            var guild = GetGuild(guildName);
            if (guild == null)
            {
                message = "Guild not found.";
                return false;
            }

            // Check if canceller is a member with invite permissions
            if (!guild.Members.ContainsKey(cancellerUid))
            {
                message = "You are not a member of this guild.";
                return false;
            }

            if (!HasPermission(guild, cancellerUid, GuildPermission.Invite))
            {
                message = "You don't have permission to manage invites.";
                return false;
            }

            // Find and remove the invite
            var invite = guild.PendingInvites.FirstOrDefault(i => i.InviteeUid == inviteeUid);
            if (invite == null)
            {
                message = "No pending invite found for that player.";
                return false;
            }

            guild.PendingInvites.Remove(invite);
            repository.MarkDirty(guildName);
            OnGuildsChanged?.Invoke();

            message = "Invite cancelled successfully.";
            return true;
        }

        /// <summary>
        /// Get all pending invites for a player
        /// </summary>
        public List<GuildInvite> GetPlayerInvites(string playerUid)
        {
            CleanupExpiredInvites();

            var invites = new List<GuildInvite>();
            foreach (var guild in repository.GetAllGuilds())
            {
                var playerInvites = guild.PendingInvites
                    .Where(i => i.InviteeUid == playerUid && !i.IsExpired())
                    .ToList();
                invites.AddRange(playerInvites);
            }
            return invites;
        }

        /// <summary>
        /// Clean up expired invites from all guilds
        /// </summary>
        private void CleanupExpiredInvites()
        {
            bool hasChanges = false;
            foreach (var guild in repository.GetAllGuilds())
            {
                var expiredInvites = guild.PendingInvites.Where(i => i.IsExpired()).ToList();
                if (expiredInvites.Count > 0)
                {
                    foreach (var invite in expiredInvites)
                    {
                        guild.PendingInvites.Remove(invite);
                    }
                    repository.MarkDirty(guild.Name);
                    hasChanges = true;
                }
            }

            if (hasChanges)
            {
                OnGuildsChanged?.Invoke();
            }
        }

        /// <summary>
        /// Public method to clean up expired invites - can be called by mod system periodic timer
        /// </summary>
        public void CleanupExpiredInvitesPublic()
        {
            CleanupExpiredInvites();
        }

        public bool KickMember(string guildName, string removerUid, string targetUid, out string message)
        {
            message = "";
            var guild = GetGuild(guildName);

            if (guild == null)
            {
                message = $"Guild '{guildName}' not found.";
                sapi.Logger.Warning($"[GuildManager] KickMember failed: {message}");
                return false;
            }

            if (!guild.Members.ContainsKey(removerUid))
            {
                message = "You are not a member of this guild.";
                sapi.Logger.Warning($"[GuildManager] KickMember failed: Remover '{removerUid}' is not a member of guild '{guildName}'");
                return false;
            }

            if (!guild.Members.ContainsKey(targetUid))
            {
                message = "The target player is not a member of this guild.";
                sapi.Logger.Warning($"[GuildManager] KickMember failed: Target '{targetUid}' is not a member of guild '{guildName}'");
                return false;
            }

            // Check permission
            if (!HasPermission(guild, removerUid, GuildPermission.Kick))
            {
                message = "You do not have permission to kick members from this guild.";
                sapi.Logger.Warning($"[GuildManager] KickMember failed: Remover '{removerUid}' lacks Kick permission in guild '{guildName}'");
                return false;
            }

            // Check hierarchy - cannot kick someone with equal or higher authority
            if (!CanActOnPlayer(guild, removerUid, targetUid))
            {
                var targetRole = guild.Members[targetUid].Role;
                message = $"You cannot kick this member. They have equal or higher authority (Role: {targetRole}).";
                sapi.Logger.Warning($"[GuildManager] KickMember failed: Remover '{removerUid}' cannot kick target '{targetUid}' due to hierarchy restrictions in guild '{guildName}'");
                return false;
            }

            // Get target player name for success message
            var targetPlayer = sapi.World.PlayerByUid(targetUid);
            var targetName = targetPlayer?.PlayerName ?? targetUid;

            guild.Members.Remove(targetUid);
            repository.MarkDirty(guildName);

            // Set cooldown for kicked player (normal cooldown)
            SetPlayerCooldown(targetUid, false);

            // Remove guild traits from kicked player
            if (targetPlayer is IServerPlayer serverPlayer)
            {
                traitManager?.SyncPlayerTraits(serverPlayer);
                sapi.Logger.Notification($"[GuildManager] Player '{targetUid}' was kicked from guild '{guildName}' by '{removerUid}' (ONLINE - traits synced immediately)");
            }
            else
            {
                sapi.Logger.Notification($"[GuildManager] Player '{targetUid}' was kicked from guild '{guildName}' by '{removerUid}' (OFFLINE - traits will sync on next login)");
            }

            message = $"Successfully kicked {targetName} from the guild.";
            OnGuildsChanged?.Invoke();
            return true;
        }

        /// <summary>
        /// Allows a player to leave their guild. Special handling for guild leaders.
        /// </summary>
        public bool LeaveGuild(string playerUid, out string message)
        {
            message = "";
            var guild = GetGuildByMember(playerUid);
            if (guild == null)
            {
                message = "You are not in a guild.";
                return false;
            }

            var member = guild.Members[playerUid];
            bool isLeader = member.Role == "Leader";

            // If the player is the leader, check if they're the only member
            if (isLeader)
            {
                if (guild.Members.Count == 1)
                {
                    // Leader is the only member - disband the entire guild
                    string guildName = guild.Name;
                    repository.DeleteGuild(guildName);

                    // Remove all guild claims from spatial index
                    landClaimRepository?.RemoveGuildFromIndex(guildName);

                    message = $"Guild '{guildName}' has been disbanded as you were the last member.";

                    // Set reduced cooldown for disbanding guild
                    SetPlayerCooldown(playerUid, true);

                    // Remove guild traits from player
                    var player = sapi.World.PlayerByUid(playerUid) as IServerPlayer;
                    if (player != null)
                    {
                        traitManager?.SyncPlayerTraits(player);
                    }

                    OnGuildsChanged?.Invoke();
                    return true;
                }
                else
                {
                    // Leader with other members - need to transfer leadership first
                    message = $"As guild leader, you must first transfer leadership to another member or remove all members before leaving. Current members: {guild.Members.Count - 1} others.";
                    return false;
                }
            }

            // Regular member leaving
            guild.Members.Remove(playerUid);
            repository.MarkDirty(guild.Name);
            message = $"You have left guild '{guild.Name}'.";

            // Set normal cooldown for leaving player
            SetPlayerCooldown(playerUid, false);

            // Remove guild traits from leaving player
            var leavingPlayer = sapi.World.PlayerByUid(playerUid) as IServerPlayer;
            if (leavingPlayer != null)
            {
                traitManager?.SyncPlayerTraits(leavingPlayer);
            }

            OnGuildsChanged?.Invoke();
            return true;
        }

        public bool PromoteMember(string guildName, string promoterUid, string targetUid, string newRole)
        {
            var guild = GetGuild(guildName);
            if (guild == null) return false;
            if (!guild.Members.ContainsKey(promoterUid)) return false;
            if (!guild.Members.ContainsKey(targetUid)) return false;
            if (!guild.Roles.ContainsKey(newRole)) return false;

            if (!HasPermission(guild, promoterUid, GuildPermission.Promote)) return false;

            // Check hierarchy - cannot assign a role with equal or higher authority than your own
            if (!CanManageRole(guild, promoterUid, newRole)) return false;

            // Check hierarchy - cannot affect a player with equal or higher authority
            if (!CanActOnPlayer(guild, promoterUid, targetUid)) return false;

            guild.Members[targetUid].Role = newRole;
            repository.MarkDirty(guildName);

            OnGuildsChanged?.Invoke();
            return true;
        }

        public bool CreateRole(string guildName, string creatorUid, string roleName, string description, GuildPermission permissions, int hierarchy)
        {
            var guild = GetGuild(guildName);
            if (guild == null) return false;
            if (!HasPermission(guild, creatorUid, GuildPermission.ManageRoles)) return false;
            if (guild.Roles.ContainsKey(roleName)) return false;

            // Check hierarchy - cannot create a role with equal or higher authority than your own
            var creatorHierarchy = GetPlayerHierarchy(guild, creatorUid);
            if (hierarchy <= creatorHierarchy) return false;

            guild.Roles[roleName] = new GuildRole { Description = description, Permissions = permissions, Hierarchy = hierarchy };
            repository.MarkDirty(guildName);

            OnGuildsChanged?.Invoke();
            return true;
        }

        public bool UpdateRolePermissions(string guildName, string updaterUid, string roleName, GuildPermission permissions)
        {
            var guild = GetGuild(guildName);
            if (guild == null) return false;
            if (!HasPermission(guild, updaterUid, GuildPermission.ManageRoles)) return false;
            if (!guild.Roles.ContainsKey(roleName)) return false;

            // Check hierarchy - cannot update a role with equal or higher authority than your own
            if (!CanManageRole(guild, updaterUid, roleName)) return false;

            guild.Roles[roleName].Permissions = permissions;
            repository.MarkDirty(guildName);

            OnGuildsChanged?.Invoke();
            return true;
        }

        /// <summary>
        /// Update role permissions and hierarchy
        /// </summary>
        public bool UpdateRolePermissions(string guildName, string updaterUid, string roleName, GuildPermission permissions, int newHierarchy)
        {
            var guild = GetGuild(guildName);
            if (guild == null) return false;
            if (!HasPermission(guild, updaterUid, GuildPermission.ManageRoles)) return false;
            if (!guild.Roles.ContainsKey(roleName)) return false;

            // Check hierarchy - cannot update a role with equal or higher authority than your own
            if (!CanManageRole(guild, updaterUid, roleName)) return false;

            // Check that new hierarchy is valid (must be higher than updater's hierarchy)
            var updaterHierarchy = GetPlayerHierarchy(guild, updaterUid);
            if (newHierarchy <= updaterHierarchy) return false;

            guild.Roles[roleName].Permissions = permissions;
            guild.Roles[roleName].Hierarchy = newHierarchy;
            repository.MarkDirty(guildName);

            OnGuildsChanged?.Invoke();
            return true;
        }

        public List<GuildMember> ListMembers(string guildName)
        {
            var guild = GetGuild(guildName);
            return guild?.Members.Values.ToList() ?? new List<GuildMember>();
        }

        /// <summary>
        /// Transfer guild ownership from current leader to another member
        /// </summary>
        /// <param name="guildName">Name of the guild</param>
        /// <param name="currentLeaderUid">UID of the current leader</param>
        /// <param name="newLeaderUid">UID of the member to become leader</param>
        /// <param name="message">Output message with result details</param>
        /// <returns>True if transfer was successful</returns>
        public bool TransferOwnership(string guildName, string currentLeaderUid, string newLeaderUid, out string message)
        {
            message = "";
            var guild = GetGuild(guildName);

            if (guild == null)
            {
                message = $"Guild '{guildName}' not found.";
                sapi.Logger.Warning($"[GuildManager] TransferOwnership failed: {message}");
                return false;
            }

            if (!guild.Members.ContainsKey(currentLeaderUid))
            {
                message = "You are not a member of this guild.";
                sapi.Logger.Warning($"[GuildManager] TransferOwnership failed: Current leader '{currentLeaderUid}' is not a member of guild '{guildName}'");
                return false;
            }

            if (!guild.Members.ContainsKey(newLeaderUid))
            {
                message = "The target player is not a member of this guild.";
                sapi.Logger.Warning($"[GuildManager] TransferOwnership failed: Target '{newLeaderUid}' is not a member of guild '{guildName}'");
                return false;
            }

            // Check that current player is actually the leader
            if (guild.Members[currentLeaderUid].Role != "Leader")
            {
                message = "Only the guild leader can transfer ownership.";
                sapi.Logger.Warning($"[GuildManager] TransferOwnership failed: Player '{currentLeaderUid}' is not the leader of guild '{guildName}'");
                return false;
            }

            // Cannot transfer to yourself
            if (currentLeaderUid == newLeaderUid)
            {
                message = "You cannot transfer ownership to yourself.";
                return false;
            }

            // Get target player name for success message
            var targetPlayer = sapi.World.PlayerByUid(newLeaderUid);
            var targetName = targetPlayer?.PlayerName ?? newLeaderUid;

            // Transfer leadership
            guild.Members[currentLeaderUid].Role = "Member";
            guild.Members[newLeaderUid].Role = "Leader";
            repository.MarkDirty(guildName);

            message = $"Successfully transferred guild leadership to {targetName}.";
            sapi.Logger.Notification($"[GuildManager] Guild '{guildName}' leadership transferred from '{currentLeaderUid}' to '{newLeaderUid}'");

            OnGuildsChanged?.Invoke();
            return true;
        }

        /// <summary>
        /// Syncs guild tech traits for all members of a guild.
        /// Should be called when a guild unlocks new technology.
        /// </summary>
        public void SyncGuildMemberTraits(Guild guild)
        {
            traitManager?.SyncGuildMemberTraits(guild);
        }

        /// <summary>
        /// Syncs guild tech traits for a specific player.
        /// Should be called when a player joins the server or changes guilds.
        /// </summary>
        public void SyncPlayerTraits(IServerPlayer player)
        {
            traitManager?.SyncPlayerTraits(player);
        }

        /// <summary>
        /// Checks if a chunk at the given coordinates is adjacent to any existing claims of the specified guild.
        /// Adjacent means sharing a side (not diagonal).
        /// </summary>
        public static bool IsChunkAdjacentToGuildClaims(Guild guild, int chunkX, int chunkZ)
        {
            if (guild == null || guild.Claims.Count == 0) return false;

            // Check if the target chunk is adjacent to any existing guild claim
            foreach (var claim in guild.Claims)
            {
                if (claim is GuildHomeClaim guildHome)
                {
                    // For guild home claims, check adjacency to any of the 4 chunks in the 2x2 area
                    foreach (var homeChunk in guildHome.GetIndividualChunks())
                    {
                        int deltaX = Math.Abs(homeChunk.ChunkX - chunkX);
                        int deltaZ = Math.Abs(homeChunk.ChunkZ - chunkZ);

                        // Adjacent means one coordinate is the same and the other differs by 1
                        if ((deltaX == 1 && deltaZ == 0) || (deltaX == 0 && deltaZ == 1))
                        {
                            return true;
                        }
                    }
                }
                else
                {
                    // Regular single chunk claim
                    int deltaX = Math.Abs(claim.ChunkX - chunkX);
                    int deltaZ = Math.Abs(claim.ChunkZ - chunkZ);

                    // Adjacent means one coordinate is the same and the other differs by 1
                    if ((deltaX == 1 && deltaZ == 0) || (deltaX == 0 && deltaZ == 1))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        // -- New: Land claim system --

        /// <summary>
        /// Attempt to claim land for a guild. If this is the guild's first claim, it will create a guild home (2x2 chunk area).
        /// Otherwise, it claims a single 16x16 chunk that must be adjacent to existing guild claims.    
        /// Requires the claimer to be a guild member with ManageRoles permission (adjustable).
        /// Returns true on success.
        /// </summary>
        public bool ClaimLand(string guildName, string claimerUid, int blockX, int blockZ, out string error)
        {
            return ClaimLand(guildName, claimerUid, blockX, blockZ, false, "", out error);
        }

        /// <summary>
        /// Attempt to claim land for a guild with option to create an outpost.
        /// If this is the guild's first claim, it will create a guild home (2x2 chunk area).
        /// Otherwise, it claims a single 16x16 chunk that must be adjacent to existing guild claims, unless isOutpost is true.
        /// Outposts can be placed anywhere without adjacency restrictions.
        /// Requires the claimer to be a guild member with ManageRoles permission (adjustable).
        /// Returns true on success.
        /// </summary>
        public bool ClaimLand(string guildName, string claimerUid, int blockX, int blockZ, bool isOutpost, string outpostName, out string? error)
        {
            var guild = GetGuild(guildName);
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

            // Permission check - adjust to a different permission if desired
            if (!HasPermission(guild, claimerUid, GuildPermission.ManageRoles))
            {
                error = "Member has no permissions to claim plots for guild";
                return false;
            }

            // Check appropriate claim limit based on claim type
            if (isOutpost)
            {
                var maxOutposts = GetMaxOutpostsPerGuild(guild);
                var currentOutposts = GetOutpostClaimCount(guild);
                if (currentOutposts >= maxOutposts)
                {
                    error = "Maximum number of outposts reached";
                    return false;
                }
            }
            else
            {
                var maxClaims = GetMaxClaimsPerGuild(guild);
                var currentClaims = GetNonOutpostClaimCount(guild);
                if (currentClaims >= maxClaims)
                {
                    error = "Maximum number of claims reached";
                    return false;
                }
            }

            // Convert block coordinates to chunk coordinates
            int chunkX = LandClaim.FloorDiv(blockX, LandClaim.ChunkSize);
            int chunkZ = LandClaim.FloorDiv(blockZ, LandClaim.ChunkSize);

            // Check territorial restrictions (outposts are exempt from territorial bounds)
            var config = configManager.GetConfig();
            var spawnPos = sapi.World.DefaultSpawnPosition.AsBlockPos;
            if (!isOutpost && !config.IsWithinTerritorialBounds(blockX, blockZ, spawnPos))
            {
                error = "Plot outside of allowed boundary";
                return false; // Outside allowed territorial boundaries
            }

            // Check protected zones - cannot claim land in protected areas
            if (config.IsChunkWithinProtectedZone(chunkX, chunkZ, spawnPos))
            {
                var zone = config.GetProtectedZoneAt(blockX, blockZ, spawnPos);
                error = $"Cannot claim land in protected zone: {zone?.Name ?? "Unknown"}";
                return false;
            }

            // If guild has no claims yet, create a guild home (not an outpost)
            if (!HasGuildHome(guild))
            {
                if (isOutpost)
                {
                    error = "Guild must establish a home base before creating outposts";
                    return false;
                }
                error = null;
                return ClaimGuildHome(guildName, claimerUid, chunkX, chunkZ);
            }

            LandClaim newClaim;

            if (isOutpost)
            {
                // Create outpost claim - no adjacency check required
                newClaim = new OutpostClaim(chunkX, chunkZ, claimerUid, outpostName);
            }
            else
            {
                // Regular single chunk claim - must be adjacent to existing guild claims
                if (!IsChunkAdjacentToGuildClaims(guild, chunkX, chunkZ))
                {
                    error = "Plot is not adjacent to exisiting claims";
                    return false; // Not adjacent to existing claims
                }

                // Regular single chunk claim
                newClaim = new LandClaim
                {
                    ChunkX = chunkX,
                    ChunkZ = chunkZ,
                    ClaimedByUid = claimerUid,
                    Timestamp = DateTime.UtcNow
                };
            }

            // Ensure no overlap with any existing claim across all guilds
            foreach (var g in repository.GetAllGuilds())
            {
                foreach (var c in g.Claims)
                {
                    if (c.Intersects(newClaim))
                    {
                        error = "Plot overlaps exisiting claims";
                        return false;
                    }
                }
            }

            // Add claim
            guild.Claims.Add(newClaim);
            repository.MarkDirty(guildName);

            landClaimRepository?.AddClaimToIndex(guildName, chunkX, chunkZ);

            OnGuildsChanged?.Invoke();
            error = null;
            return true;
        }

        /// <summary>
        /// Creates an outpost claim for the guild at the specified coordinates.
        /// Outposts do not require adjacency to existing claims.
        /// </summary>
        public bool ClaimOutpost(string guildName, string claimerUid, int blockX, int blockZ, string outpostName, out string? error)
        {
            return ClaimLand(guildName, claimerUid, blockX, blockZ, true, outpostName, out error);
        }

        /// <summary>
        /// Creates a guild home (2x2 chunk area) centered on the specified chunk coordinates.
        /// This replaces the concept of "first claim" with a dedicated guild home.
        /// </summary>
        public bool ClaimGuildHome(string guildName, string claimerUid, int centerChunkX, int centerChunkZ)
        {
            var guild = GetGuild(guildName);
            if (guild == null) return false;
            if (!guild.Members.ContainsKey(claimerUid)) return false;

            // Permission check
            if (!HasPermission(guild, claimerUid, GuildPermission.ManageRoles)) return false;

            // Check if guild already has a home
            if (HasGuildHome(guild))
            {
                return false; // Guild already has a home
            }

            // Check territorial restrictions for the guild home (all 4 chunks in 2x2 area)
            var config = configManager.GetConfig();
            var spawnPos = sapi.World.DefaultSpawnPosition.AsBlockPos;
            for (int dx = 0; dx <= 1; dx++)
            {
                for (int dz = 0; dz <= 1; dz++)
                {
                    int chunkX = centerChunkX + dx;
                    int chunkZ = centerChunkZ + dz;
                    if (!config.IsChunkWithinTerritorialBounds(chunkX, chunkZ, spawnPos))
                    {
                        return false; // Part of guild home is outside allowed territorial boundaries
                    }

                    // Check protected zones - guild home cannot overlap with protected areas
                    if (config.IsChunkWithinProtectedZone(chunkX, chunkZ, spawnPos))
                    {
                        return false; // Part of guild home is in a protected zone
                    }
                }
            }

            // Create the guild home claim
            var guildHome = new GuildHomeClaim(centerChunkX, centerChunkZ, claimerUid);

            // Check if we have enough claim slots (guild home counts as 1 claim but uses 4 chunk spaces)
            // Only check non-outpost claims since guild home is a regular claim
            var maxClaims = GetMaxClaimsPerGuild(guild);
            var currentClaims = GetNonOutpostClaimCount(guild);
            if (currentClaims + 1 > maxClaims)
            {
                return false; // Not enough claim slots available
            }

            // Ensure no overlap with any existing claims across all guilds
            foreach (var g in repository.GetAllGuilds())
            {
                foreach (var existingClaim in g.Claims)
                {
                    if (existingClaim.Intersects(guildHome))
                    {
                        return false; // Overlap detected
                    }
                }
            }

            // All checks passed, add the guild home
            guild.Claims.Add(guildHome);
            repository.MarkDirty(guildName);

            if (landClaimRepository != null)
            {
                foreach (var chunk in guildHome.GetIndividualChunks())
                {
                    landClaimRepository.AddClaimToIndex(guildName, chunk.ChunkX, chunk.ChunkZ);
                }
            }

            OnGuildsChanged?.Invoke();
            return true;
        }

        /// <summary>
        /// Checks if a guild has a guild home claim
        /// </summary>
        private bool HasGuildHome(Guild guild)
        {
            return guild?.Claims.Any(c => c is GuildHomeClaim) ?? false;
        }

        /// <summary>
        /// Gets the guild home claim for a guild, if it exists
        /// </summary>
        public GuildHomeClaim? GetGuildHome(string guildName)
        {
            var guild = GetGuild(guildName);
            return guild?.Claims.OfType<GuildHomeClaim>().FirstOrDefault();
        }

        /// <summary>
        /// Releases a claim owned by the guild. The requester must either have ManageRoles or be the original claimer.
        /// </summary>
        public bool ReleaseClaim(string guildName, LandClaim claim, string requesterUid)
        {
            var guild = GetGuild(guildName);
            if (guild == null) return false;
            if (claim == null) return false;
            if (!guild.Claims.Contains(claim)) return false;
            if (!HasPermission(guild, requesterUid, GuildPermission.ManageRoles) && claim.ClaimedByUid != requesterUid) return false;

            guild.Claims.Remove(claim);
            repository.MarkDirty(guildName);

            // Update spatial index
            if (landClaimRepository != null)
            {
                if (claim is GuildHomeClaim guildHome)
                {
                    foreach (var chunk in guildHome.GetIndividualChunks())
                    {
                        landClaimRepository.RemoveClaimFromIndex(chunk.ChunkX, chunk.ChunkZ);
                    }
                }
                else
                {
                    landClaimRepository.RemoveClaimFromIndex(claim.ChunkX, claim.ChunkZ);
                }
            }

            OnGuildsChanged?.Invoke();
            return true;
        }

        /// <summary>
        /// Unclaims a chunk of land from a guild
        /// </summary>
        public (bool Success, string? ErrorMessage) UnclaimLand(string guildName, int chunkX, int chunkZ)
        {
            var guild = repository.GetGuild(guildName);
            if (guild == null)
                return (false, "Guild not found.");

            // Check if this chunk is part of a guild home claim
            var guildHome = guild.Claims.OfType<GuildHomeClaim>().FirstOrDefault(gh =>
                gh.ContainsBlockCoord(chunkX * LandClaim.ChunkSize, chunkZ * LandClaim.ChunkSize));

            if (guildHome != null)
            {
                // Guild home must be the last claim to be unclaimed
                if (guild.Claims.Count > 1)
                {
                    return (false, "Cannot unclaim guild home while other claims exist. Unclaim all other territory first.");
                }

                // This chunk is part of a guild home - unclaim the entire guild home (all 4 chunks)
                guild.Claims.Remove(guildHome);
                repository.MarkDirty(guildName);

                // Update spatial index
                if (landClaimRepository != null)
                {
                    foreach (var chunk in guildHome.GetIndividualChunks())
                    {
                        landClaimRepository.RemoveClaimFromIndex(chunk.ChunkX, chunk.ChunkZ);
                    }
                }

                sapi.Logger.Notification($"[GuildManager] Guild home completely unclaimed for guild '{guildName}' (all chunks removed)");

                // Trigger save and sync
                OnGuildsChanged?.Invoke();
                return (true, null);
            }

            // Find the claim to remove (for non-guild-home claims)
            var claimToRemove = guild.Claims.FirstOrDefault(c => c.ChunkX == chunkX && c.ChunkZ == chunkZ);

            if (claimToRemove == null)
                return (false, "This chunk is not claimed by your guild.");

            // Prevent unclaiming outpost center
            if (claimToRemove is OutpostClaim outpost)
            {
                return (false, $"Cannot unclaim outpost center '{outpost.OutpostName}'. Delete the entire outpost first.");
            }

            // Check if unclaiming would split the territory (optional validation)
            if (WouldSplitTerritory(guild, chunkX, chunkZ))
                return (false, "Cannot unclaim this chunk as it would split your territory. Unclaim edge chunks first.");

            // Remove the claim
            guild.Claims.Remove(claimToRemove);
            repository.MarkDirty(guildName);

            // Update spatial index
            landClaimRepository?.RemoveClaimFromIndex(chunkX, chunkZ);

            // Trigger save and sync
            OnGuildsChanged?.Invoke();

            return (true, null);
        }

        /// <summary>
        /// Check if unclaiming a chunk would split the guild's territory into disconnected regions
        /// </summary>
        private bool WouldSplitTerritory(Guild guild, int unclaimX, int unclaimZ)
        {
            // Get all claims except the one being unclaimed
            // For guild homes, we need to check individual chunks
            var remainingChunks = new List<(int chunkX, int chunkZ)>();

            foreach (var claim in guild.Claims)
            {
                // Skip outpost claims - they are meant to be separate from main territory
                if (claim is OutpostClaim)
                    continue;

                if (claim is GuildHomeClaim guildHome)
                {
                    // Add all 4 individual chunks of the guild home
                    foreach (var homeChunk in guildHome.GetIndividualChunks())
                    {
                        if (homeChunk.ChunkX != unclaimX || homeChunk.ChunkZ != unclaimZ)
                        {
                            remainingChunks.Add((homeChunk.ChunkX, homeChunk.ChunkZ));
                        }
                    }
                }
                else if (claim.ChunkX != unclaimX || claim.ChunkZ != unclaimZ)
                {
                    remainingChunks.Add((claim.ChunkX, claim.ChunkZ));
                }
            }

            if (remainingChunks.Count <= 1)
                return false; // Can't split if only 0-1 chunks remain

            // Use flood fill to check if all remaining chunks are connected
            var visited = new HashSet<(int, int)>();
            var queue = new Queue<(int, int)>();
            queue.Enqueue(remainingChunks[0]);

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                if (visited.Contains(current))
                    continue;

                visited.Add(current);

                // Check adjacent chunks
                var adjacent = new[]
                {
                    (current.Item1 + 1, current.Item2),
                    (current.Item1 - 1, current.Item2),
                    (current.Item1, current.Item2 + 1),
                    (current.Item1, current.Item2 - 1)
                };

                foreach (var adj in adjacent)
                {
                    if (remainingChunks.Contains(adj) && !visited.Contains(adj))
                        queue.Enqueue(adj);
                }
            }

            // If we visited all remaining chunks, territory is still connected
            return visited.Count != remainingChunks.Count;
        }

        /// <summary>
        /// Returns all claims for the named guild.
        /// </summary>
        public List<LandClaim> GetClaims(string guildName)
        {
            var guild = GetGuild(guildName);
            return guild?.Claims.ToList() ?? new List<LandClaim>();
        }

        /// <summary>
        /// Returns a serializable list of GuildSummary for syncing to clients.
        /// </summary>
        public List<GuildSummary> GetAllGuildSummaries()
        {
            return GetGuildSummariesForPlayer("");
        }

        /// <summary>
        /// Returns a serializable list of GuildSummary for syncing to a specific client.
        /// Includes player membership information if playerUid is provided.
        /// </summary>
        public List<GuildSummary> GetGuildSummariesForPlayer(string playerUid)
        {
            // Clean up expired invites before generating summaries
            CleanupExpiredInvites();

            var list = new List<GuildSummary>();

            foreach (var g in repository.GetAllGuilds())
            {
                // Calculate max claims and outposts based on this guild's member count
                var currentMaxClaims = GetMaxClaimsPerGuild(g);
                var currentMaxOutposts = GetMaxOutpostsPerGuild(g);

                var s = new GuildSummary
                {
                    Name = g.Name,
                    Description = g.Description,
                    DisplayColor = g.DisplayColor,
                    SecondaryColor = g.SecondaryColor,
                    Points = g.Points,
                    RankClass = configManager.GetConfig().GetGuildRankClass(g.Points),
                    MemberCount = g.Members.Count,
                    MemberUids = g.Members.Keys.ToList(), // Add member UIDs for filtering
                    PendingInviteUids = g.PendingInvites.Select(i => i.InviteeUid).ToList(), // Add pending invite UIDs for filtering
                    PendingInvites = g.PendingInvites.Select(i => new GuildInviteDto
                    {
                        InviterUid = i.InviterUid,
                        InviteeUid = i.InviteeUid,
                        ExpiresAt = i.ExpiresAt
                    }).ToList(), // Include full invite details for guild members
                    Roles = new Dictionary<string, GuildRole>(g.Roles), // Include role definitions
                    Claims = ConvertClaimsToDto(g.Claims),
                    MaxClaims = currentMaxClaims, // Add current max claims limit based on guild's member count
                    MaxOutposts = currentMaxOutposts, // Add current max outposts limit based on guild's member count
                    TechProgress = new Dictionary<int, GuildTechProgress>(g.TechProgress), // Copy tech progress to sync to clients
                    TechRequiresPersonalUnlock = new Dictionary<int, bool>(g.TechRequiresPersonalUnlock) // Copy personal unlock requirements
                };

                // Add player-specific information if requested
                if (!string.IsNullOrEmpty(playerUid) && g.Members.ContainsKey(playerUid))
                {
                    var member = g.Members[playerUid];
                    s.IsPlayerMember = true;
                    s.PlayerRole = member.Role;

                    // Add player's points contribution and calculated rank
                    s.MemberPointsContribution = member.PointsContribution;
                    s.MemberRank = configManager.GetConfig().GetMemberRank(member.PointsContribution);

                    // Include personal tech progress for this player only
                    if (g.PlayerTechProgress.TryGetValue(playerUid, out var playerProgress))
                    {
                        s.PlayerTechProgress = new Dictionary<string, PlayerTechProgress>
                        {
                            { playerUid, playerProgress }
                        };
                    }
                    // Add player permissions for client-side validation (prevents desync)
                    s.HasBreakPlacePermission = HasPermission(g, playerUid, GuildPermission.BreakAndPlaceBlocks);
                    s.HasInteractPermission = HasPermission(g, playerUid, GuildPermission.InteractBlocks);
                }

                list.Add(s);
            }
            return list;
        }

        /// <summary>
        /// Converts LandClaim objects to LandClaimDto objects, properly handling GuildHomeClaim and OutpostClaim
        /// </summary>
        private List<LandClaimDto> ConvertClaimsToDto(List<LandClaim> claims)
        {
            var result = new List<LandClaimDto>();

            foreach (var claim in claims)
            {
                if (claim is GuildHomeClaim guildHome)
                {
                    // For guild homes, add all individual chunks with guild home markers
                    foreach (var homeChunk in guildHome.GetIndividualChunks())
                    {
                        result.Add(new LandClaimDto
                        {
                            ChunkX = homeChunk.ChunkX,
                            ChunkZ = homeChunk.ChunkZ,
                            ClaimedByUid = homeChunk.ClaimedByUid,
                            Timestamp = homeChunk.Timestamp,
                            IsGuildHome = true,
                            HomeCenterX = guildHome.CenterChunkX,
                            HomeCenterZ = guildHome.CenterChunkZ
                        });
                    }
                }
                else if (claim is OutpostClaim outpost)
                {
                    // Outpost claim
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
                    // Regular claim
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

        /// <summary>
        /// Changes the name of a guild. Returns true if successful.
        /// </summary>
        public bool ChangeGuildName(string oldName, string changerUid, string newName)
        {
            if (string.IsNullOrWhiteSpace(newName)) return false;
            if (repository.GetGuild(newName) != null) return false; // Name already exists
            var guild = repository.GetGuild(oldName);
            if (guild == null) return false;
            if (!HasPermission(guild, changerUid, GuildPermission.ManageRoles)) return false;

            try
            {
                repository.RenameGuild(guild, newName);

                landClaimRepository?.UpdateGuildName(oldName, newName);

                OnGuildsChanged?.Invoke();
                return true;
            }
            catch (Exception ex)
            {
                sapi.Logger.Error($"[GuildManager] Failed to rename guild '{oldName}': {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Checks if a player is currently standing within their guild's claimed land
        /// </summary>
        /// <param name="guild">The guild to check claims for</param>
        /// <param name="blockX">Player's X block coordinate</param>
        /// <param name="blockZ">Player's Z block coordinate</param>
        /// <returns>True if player is within any of the guild's claims</returns>
        public bool IsPlayerInGuildClaim(Guild guild, int blockX, int blockZ)
        {
            if (guild == null || guild.Claims == null || guild.Claims.Count == 0)
                return false;

            // Check if the player's position is within any of the guild's claims
            foreach (var claim in guild.Claims)
            {
                if (claim.ContainsBlockCoord(blockX, blockZ))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Log examples of how claims scale with guild member count for demonstration
        /// </summary>
        private void LogScalingExamples()
        {
            var config = configManager.GetConfig();
            if (!config.EnableDynamicClaimLimits && !config.EnableDynamicOutpostLimits)
            {
                sapi.Logger.Notification("Dynamic limits are disabled. All guilds have fixed limits: " +
                    config.BaseMaxClaimsPerGuild + " claims, " + config.BaseMaxOutpostsPerGuild + " outposts.");
                return;
            }

            sapi.Logger.Notification("Claim limits scale with guild member count - Examples:");
            int[] testMemberCounts = { 1, 5, 10, 15, 20, 25 };

            foreach (int memberCount in testMemberCounts)
            {
                int maxClaims = config.CalculateMaxClaimsPerGuild(memberCount);
                int maxOutposts = config.CalculateMaxOutpostsPerGuild(memberCount);
                sapi.Logger.Notification($"  With {memberCount} member(s): {maxClaims} max claims, {maxOutposts} max outposts");
            }
        }

        // -- Guild Rejoin Cooldown System --

        /// <summary>
        /// Sets a cooldown for a player who left or was kicked from a guild
        /// </summary>
        /// <param name="playerUid">The player's UID</param>
        /// <param name="isDisbanding">True if the player is the last member disbanding the guild (reduced cooldown)</param>
        private void SetPlayerCooldown(string playerUid, bool isDisbanding)
        {
            var config = configManager.GetConfig();
            int cooldownDays = isDisbanding ? config.GuildDisbandCooldownDays : config.GuildRejoinCooldownDays;

            var cooldownEndTime = DateTime.UtcNow.AddDays(cooldownDays);
            cooldownRepository.SetCooldown(playerUid, cooldownEndTime);

            sapi.Logger.Notification($"[GuildManager] Player '{playerUid}' cooldown set for {cooldownDays} days (until {cooldownEndTime:yyyy-MM-dd HH:mm:ss} UTC)");
        }

        /// <summary>
        /// Checks if a player is currently on cooldown from joining guilds
        /// </summary>
        /// <param name="playerUid">The player's UID</param>
        /// <param name="remainingTime">Outputs the remaining cooldown time if on cooldown</param>
        /// <returns>True if player is on cooldown, false otherwise</returns>
        public bool IsPlayerOnCooldown(string playerUid, out TimeSpan remainingTime)
        {
            return cooldownRepository.IsOnCooldown(playerUid, out remainingTime);
        }

        /// <summary>
        /// Gets the cooldown end time for a player, if they have one
        /// </summary>
        /// <param name="playerUid">The player's UID</param>
        /// <returns>The cooldown end time, or null if no cooldown is active</returns>
        public DateTime? GetPlayerCooldownEndTime(string playerUid)
        {
            return cooldownRepository.GetCooldown(playerUid);
        }

        /// <summary>
        /// Clears a player's cooldown (admin/debug use)
        /// </summary>
        /// <param name="playerUid">The player's UID</param>
        /// <returns>True if a cooldown was cleared, false if no cooldown existed</returns>
        public bool ClearPlayerCooldown(string playerUid)
        {
            if (cooldownRepository.ClearCooldown(playerUid))
            {
                sapi.Logger.Notification($"[GuildManager] Cleared cooldown for player '{playerUid}'");
                return true;
            }
            return false;
        }

        // -- Node Wars Integration Methods --

        /// <summary>
        /// Update guild's controlled nodes when a node is captured
        /// </summary>
        public void NodeCaptured(string guildName, string nodeId, string nodeName)
        {
            var guild = GetGuild(guildName);
            if (guild == null)
            {
                sapi.Logger.Warning($"[GuildManager] NodeCaptured: Guild '{guildName}' not found");
                return;
            }

            // Remove node from previous owner
            foreach (var otherGuild in repository.GetAllGuilds())
            {
                if (otherGuild.Name != guildName && otherGuild.ControlsNode(nodeId))
                {
                    otherGuild.RemoveControlledNode(nodeId);
                    repository.MarkDirty(otherGuild.Name);
                    OnNodeLost?.Invoke(otherGuild.Name, nodeId, nodeName);
                    sapi.Logger.Notification($"[GuildManager] Guild '{otherGuild.Name}' lost node '{nodeName}'");
                }
            }

            // Add node to capturing guild
            guild.AddControlledNode(nodeId);
            repository.MarkDirty(guildName);

            OnNodeCaptured?.Invoke(guildName, nodeId, nodeName);
            OnGuildsChanged?.Invoke();

            sapi.Logger.Notification($"[GuildManager] Guild '{guildName}' captured node '{nodeName}'");
        }

        /// <summary>
        /// Remove a node from guild control (when node is lost or removed)
        /// </summary>
        public void NodeLost(string guildName, string nodeId, string nodeName)
        {
            var guild = GetGuild(guildName);
            if (guild == null) return;

            guild.RemoveControlledNode(nodeId);
            repository.MarkDirty(guildName);

            OnNodeLost?.Invoke(guildName, nodeId, nodeName);
            OnGuildsChanged?.Invoke();

            sapi.Logger.Notification($"[GuildManager] Guild '{guildName}' lost node '{nodeName}'");
        }

        /// <summary>
        /// Set guild's node war signup status
        /// </summary>
        public void SetGuildWarSignup(string guildName, string nodeId)
        {
            var guild = GetGuild(guildName);
            if (guild == null) return;

            guild.SetNodeWarSignup(nodeId);
            repository.MarkDirty(guildName);

            OnGuildSignedUpForWar?.Invoke(guildName, nodeId);
            OnGuildsChanged?.Invoke();

            sapi.Logger.Debug($"[GuildManager] Guild '{guildName}' signed up for war at node '{nodeId}'");
        }

        /// <summary>
        /// Clear guild's node war signup
        /// </summary>
        public void ClearGuildWarSignup(string guildName, string nodeId)
        {
            var guild = GetGuild(guildName);
            if (guild == null) return;

            guild.ClearNodeWarSignup();
            repository.MarkDirty(guildName);

            OnGuildCancelledWarSignup?.Invoke(guildName, nodeId);
            OnGuildsChanged?.Invoke();

            sapi.Logger.Debug($"[GuildManager] Guild '{guildName}' cancelled war signup for node '{nodeId}'");
        }

        /// <summary>
        /// Get all nodes controlled by a guild
        /// </summary>
        public List<string> GetGuildControlledNodes(string guildName)
        {
            var guild = GetGuild(guildName);
            return guild?.ControlledNodes ?? new List<string>();
        }

        /// <summary>
        /// Check if a guild controls a specific node
        /// </summary>
        public bool DoesGuildControlNode(string guildName, string nodeId)
        {
            var guild = GetGuild(guildName);
            return guild?.ControlsNode(nodeId) ?? false;
        }

        /// <summary>
        /// Get which guild controls a specific node
        /// </summary>
        public string? GetNodeControllingGuild(string nodeId)
        {
            foreach (var guild in repository.GetAllGuilds())
            {
                if (guild.ControlsNode(nodeId))
                {
                    return guild.Name;
                }
            }
            return null;
        }
    }
}
