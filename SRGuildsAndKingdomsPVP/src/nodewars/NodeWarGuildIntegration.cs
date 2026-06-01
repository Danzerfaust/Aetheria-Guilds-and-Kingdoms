using System;
using System.Linq;
using Vintagestory.API.Common;
using Vintagestory.API.Server;
using SRGuildsAndKingdoms.src.guilds;
using SRGuildsAndKingdoms;

namespace SRGuildsAndKingdomsPVP.src.nodewars
{
    /// <summary>
    /// Integrates NodeWarManager with the SRGuildsAndKingdoms guild system
    /// Provides helper methods for guild-based node war operations
    /// </summary>
    public class NodeWarGuildIntegration
    {
        private readonly ICoreServerAPI sapi;
        private readonly NodeWarManager nodeWarManager;
        private readonly SRGuildsAndKingdomsModSystem guildModSystem;

        public NodeWarGuildIntegration(ICoreServerAPI api, NodeWarManager nodeWarManager, SRGuildsAndKingdomsModSystem guildModSystem)
        {
            this.sapi = api;
            this.nodeWarManager = nodeWarManager;
            this.guildModSystem = guildModSystem;
        }

        /// <summary>
        /// Sign up a player's guild for a node war (with validation)
        /// </summary>
        public GuildSignupResult SignupPlayerGuildForWar(IServerPlayer player, string nodeId, int minMembersRequired = 3, int minOnlineRequired = 3)
        {
            // Check if player is in a guild
            var guild = guildModSystem.GuildManager?.GetGuildByMember(player.PlayerUID);
            if (guild == null)
            {
                return GuildSignupResult.Failed("You are not in a guild", GuildSignupFailureReason.NotInGuild);
            }

            // Check if player has permission to sign up guild (Leader or specific permission)
            if (!HasSignupPermission(guild, player.PlayerUID))
            {
                return GuildSignupResult.Failed("You don't have permission to sign up your guild for wars. Only guild leaders can do this.", GuildSignupFailureReason.NoPermission);
            }

            // Count online guild members
            int membersOnline = CountOnlineGuildMembers(guild);
            int totalMembers = guild.Members.Count;

            // Validate guild meets requirements
            var validationResult = nodeWarManager.ValidateGuildSignup(
                GetGuildUid(guild),
                membersOnline,
                totalMembers,
                minMembersRequired,
                minOnlineRequired
            );

            if (!validationResult.Success)
            {
                return validationResult;
            }

            // Perform the signup
            return nodeWarManager.SignupGuild(
                GetGuildUid(guild),
                guild.Name,
                nodeId,
                player.PlayerUID,
                membersOnline,
                totalMembers
            );
        }

        /// <summary>
        /// Cancel a player's guild signup for a war
        /// </summary>
        public GuildSignupResult CancelPlayerGuildSignup(IServerPlayer player, string nodeId)
        {
            var guild = guildModSystem.GuildManager?.GetGuildByMember(player.PlayerUID);
            if (guild == null)
            {
                return GuildSignupResult.Failed("You are not in a guild", GuildSignupFailureReason.NotInGuild);
            }

            if (!HasSignupPermission(guild, player.PlayerUID))
            {
                return GuildSignupResult.Failed("You don't have permission to cancel guild war signups", GuildSignupFailureReason.NoPermission);
            }

            return nodeWarManager.CancelGuildSignup(GetGuildUid(guild), nodeId);
        }

        /// <summary>
        /// Join a player into a node war if their guild is signed up
        /// </summary>
        public bool JoinPlayerToGuildWar(IServerPlayer player, string nodeId)
        {
            var guild = guildModSystem.GuildManager?.GetGuildByMember(player.PlayerUID);
            if (guild == null)
            {
                return false;
            }

            var war = nodeWarManager.GetActiveNodeWar(nodeId);
            if (war == null || war.Status != NodeWarStatus.Active)
            {
                return false;
            }

            // Check if guild is signed up for this war
            if (!war.IsGuildSignedUp(GetGuildUid(guild)))
            {
                return false;
            }

            return nodeWarManager.JoinNodeWar(
                player.PlayerUID,
                player.PlayerName,
                GetGuildUid(guild),
                nodeId
            );
        }

        /// <summary>
        /// Get a guild object from a guild UID
        /// </summary>
        public Guild? GetGuildByUid(string guildUid)
        {
            // In the current system, guild UID is the guild name
            return guildModSystem.GuildManager?.GetGuild(guildUid);
        }

        /// <summary>
        /// Count how many members of a guild are currently online
        /// </summary>
        public int CountOnlineGuildMembers(Guild guild)
        {
            int count = 0;
            foreach (var memberUid in guild.Members.Keys)
            {
                var player = sapi.World.PlayerByUid(memberUid);
                if (player != null && player is IServerPlayer serverPlayer)
                {
                    // Check if player is actually connected
                    if (serverPlayer.ConnectionState == EnumClientState.Playing)
                    {
                        count++;
                    }
                }
            }
            return count;
        }

        /// <summary>
        /// Check if a player has permission to sign up their guild for wars
        /// By default, only guild leaders can sign up
        /// </summary>
        public bool HasSignupPermission(Guild guild, string playerUid)
        {
            if (!guild.Members.TryGetValue(playerUid, out var member))
            {
                return false;
            }

            // Check if player is leader
            return member.Role == "Leader";
            
            // Future: Could add a new GuildPermission flag for war management
            // return GuildManager.HasPermission(guild, playerUid, GuildPermission.ManageWars);
        }

        /// <summary>
        /// Get a unique identifier for a guild
        /// Currently uses guild name as UID since guilds are keyed by name
        /// </summary>
        private string GetGuildUid(Guild guild)
        {
            // In current implementation, guild name is the unique identifier
            return guild.Name;
        }

        /// <summary>
        /// Get information about a guild's war signup status
        /// </summary>
        public (bool isSignedUp, NodeWar? war, GuildNodeWarSignup? signup) GetGuildWarStatus(string guildUid)
        {
            var war = nodeWarManager.GetGuildSignedUpWar(guildUid);
            if (war != null)
            {
                war.GuildSignups.TryGetValue(guildUid, out var signup);
                return (true, war, signup);
            }
            return (false, null, null);
        }

        /// <summary>
        /// Send war signup notification to all online guild members
        /// </summary>
        public void NotifyGuildMembersOfSignup(Guild guild, string nodeName, bool isSignup)
        {
            string message = isSignup
                ? $"Your guild has been signed up for the war at {nodeName}!"
                : $"Your guild's signup for the war at {nodeName} has been cancelled.";

            foreach (var memberUid in guild.Members.Keys)
            {
                var player = sapi.World.PlayerByUid(memberUid);
                if (player is IServerPlayer serverPlayer && serverPlayer.ConnectionState == EnumClientState.Playing)
                {
                    serverPlayer.SendMessage(0, message, EnumChatType.Notification);
                }
            }
        }

        /// <summary>
        /// Get a formatted list of guilds signed up for a node war
        /// </summary>
        public string GetSignedUpGuildsFormatted(string nodeId)
        {
            var signups = nodeWarManager.GetSignedUpGuilds(nodeId);
            if (signups.Count == 0)
            {
                return "No guilds signed up yet.";
            }

            var lines = signups.Select((signup, index) =>
                $"{index + 1}. {signup.GuildName} ({signup.TotalMembersAtSignup} members, {signup.MembersOnlineAtSignup} online)"
            );

            return string.Join("\n", lines);
        }
    }
}
