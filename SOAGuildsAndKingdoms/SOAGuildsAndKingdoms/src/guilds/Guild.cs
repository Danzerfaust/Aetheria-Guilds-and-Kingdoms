using ProtoBuf;
using SOAGuildsAndKingdoms.src.techblock;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SOAGuildsAndKingdoms.src.guilds
{
    [Flags]
    public enum GuildPermission
    {
        None = 0,
        Invite = 1 << 0,
        Promote = 1 << 1,
        Kick = 1 << 2,
        ManageRoles = 1 << 3,
        BreakAndPlaceBlocks = 1 << 4,
        InteractBlocks = 1 << 5,
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class GuildRole
    {
        public string Description { get; set; } = null!;
        public GuildPermission Permissions { get; set; } = GuildPermission.None;

        // Hierarchy level - lower numbers = higher authority
        // 1 = highest (Leader), higher numbers = lower authority
        // A role can only affect roles with higher hierarchy numbers
        public int Hierarchy { get; set; } = 999;
    }

    public class Guild
    {
        /// <summary>
        /// Database primary key (internal), nullable if not yet saved to database and ignored @ JSON
        /// </summary>
        [JsonIgnore]
        [ProtoIgnore]
        public int? DatabaseId { get; set; }

        public string Name { get; set; }

        // A short human-readable description shown on the map tooltip
        public string Description { get; set; } = "";

        // ARGB color packed as int (0xAARRGGBB). Choose when guild created, can be edited later.
        public int DisplayColor { get; set; } = unchecked((int)0xFF7F7FFF); // default pale purple

        // Secondary color for more advanced guild theming
        public int SecondaryColor { get; set; } = unchecked((int)0xFF9F9FFF); // default lighter purple

        // GRS points for leaderboard and guild rank
        public int Points { get; set; } = 0;

        // member uid -> GuildMember
        public Dictionary<string, GuildMember> Members { get; set; } = new();

        public List<GuildInvite> PendingInvites { get; set; } = new();

        // role name -> GuildRole
        public Dictionary<string, GuildRole> Roles { get; set; } = new();

        // New: list of land claims owned by this guild
        public List<LandClaim> Claims { get; set; } = new();

        // New: tech progression tracking for this guild (TechBlockId -> Progress)
        public Dictionary<int, GuildTechProgress> TechProgress { get; set; } = new();

        // New: track which techs require personal unlocks (set when unlocked based on guild size > 10)
        public Dictionary<int, bool> TechRequiresPersonalUnlock { get; set; } = new();

        // New: track personal tech progress for all players (PlayerUid -> PlayerTechProgress)
        public Dictionary<string, PlayerTechProgress> PlayerTechProgress { get; set; } = new();

        // Node Wars: List of node IDs controlled by this guild
        public List<string> ControlledNodes { get; set; } = new();

        // Node Wars: History of node captures (NodeId -> CaptureTime)
        public Dictionary<string, DateTime> NodeCaptureHistory { get; set; } = new();

        // Node Wars: Current node war signup (NodeId or null if not signed up)
        public string? CurrentNodeWarSignup { get; set; } = null;

        // Node Wars: When the guild signed up for the current war
        public DateTime? NodeWarSignupTime { get; set; } = null;

        public Guild()
        {
            Name = string.Empty;

            // sensible defaults if needed
            if (!Roles.ContainsKey("Leader"))
            {
                Roles["Leader"] = new GuildRole
                {
                    Description = "Leader",
                    Permissions = GuildPermission.Invite | GuildPermission.Promote | GuildPermission.Kick | GuildPermission.ManageRoles | GuildPermission.BreakAndPlaceBlocks | GuildPermission.InteractBlocks,
                    Hierarchy = 1 // Highest authority
                };
            }

            if (!Roles.ContainsKey("Member"))
            {
                Roles["Member"] = new GuildRole
                {
                    Description = "Member",
                    Permissions = GuildPermission.BreakAndPlaceBlocks | GuildPermission.InteractBlocks,
                    Hierarchy = 100 // Lower authority
                };
            }
        }

        // Helper method to get outpost claims
        public List<OutpostClaim> GetOutpostClaims()
        {
            var outposts = new List<OutpostClaim>();
            foreach (var claim in Claims)
            {
                if (claim is OutpostClaim outpost)
                {
                    outposts.Add(outpost);
                }
            }
            return outposts;
        }

        // Helper method to get regular land claims (excluding guild homes and outposts)
        public List<LandClaim> GetRegularClaims()
        {
            var regularClaims = new List<LandClaim>();
            foreach (var claim in Claims)
            {
                if (!(claim is GuildHomeClaim) && !(claim is OutpostClaim))
                {
                    regularClaims.Add(claim);
                }
            }
            return regularClaims;
        }

        // Helper method to get guild home claims
        public List<GuildHomeClaim> GetGuildHomeClaims()
        {
            var homeClaims = new List<GuildHomeClaim>();
            foreach (var claim in Claims)
            {
                if (claim is GuildHomeClaim home)
                {
                    homeClaims.Add(home);
                }
            }
            return homeClaims;
        }

        // Helper method to get or create tech progress for a specific tech block
        public GuildTechProgress GetOrCreateTechProgress(int techBlockId)
        {
            if (!TechProgress.ContainsKey(techBlockId))
            {
                TechProgress[techBlockId] = new GuildTechProgress { TechBlockId = techBlockId };
            }
            return TechProgress[techBlockId];
        }

        // Helper method to check if a tech is unlocked
        public bool IsTechUnlocked(int techBlockId)
        {
            return TechProgress.TryGetValue(techBlockId, out var progress) && progress.IsUnlocked;
        }

        /// <summary>
        /// Checks if a player has full access to a tech (guild unlocked + personal unlock if required)
        /// </summary>
        /// <param name="playerUid">Player's unique identifier</param>
        /// <param name="techId">Tech block ID to check</param>
        /// <returns>True if player can use the tech</returns>
        public bool HasPlayerUnlockedTech(string playerUid, int techId)
        {
            // First check if guild has unlocked the tech
            if (!IsTechUnlocked(techId))
                return false;

            // Check if this tech requires personal unlock
            if (!TechRequiresPersonalUnlock.TryGetValue(techId, out bool requiresPersonal) || !requiresPersonal)
                return true; // No personal unlock required, player has access

            // Check if player has completed their personal unlock
            if (PlayerTechProgress.TryGetValue(playerUid, out var progress))
            {
                return progress.IsPersonallyUnlocked(techId);
            }

            return false; // No progress tracked, so not unlocked
        }

        /// <summary>
        /// Gets or creates player tech progress for a specific player
        /// </summary>
        public PlayerTechProgress GetOrCreatePlayerProgress(string playerUid)
        {
            if (!PlayerTechProgress.ContainsKey(playerUid))
            {
                PlayerTechProgress[playerUid] = new PlayerTechProgress { PlayerUid = playerUid };
            }
            return PlayerTechProgress[playerUid];
        }

        // Node Wars: Helper methods for node management

        /// <summary>
        /// Add a controlled node to this guild
        /// </summary>
        public void AddControlledNode(string nodeId)
        {
            if (!ControlledNodes.Contains(nodeId))
            {
                ControlledNodes.Add(nodeId);
                NodeCaptureHistory[nodeId] = DateTime.UtcNow;
            }
        }

        /// <summary>
        /// Remove a controlled node from this guild
        /// </summary>
        public void RemoveControlledNode(string nodeId)
        {
            ControlledNodes.Remove(nodeId);
        }

        /// <summary>
        /// Check if guild controls a specific node
        /// </summary>
        public bool ControlsNode(string nodeId)
        {
            return ControlledNodes.Contains(nodeId);
        }

        /// <summary>
        /// Get when a node was captured
        /// </summary>
        public DateTime? GetNodeCaptureTime(string nodeId)
        {
            return NodeCaptureHistory.TryGetValue(nodeId, out var time) ? time : null;
        }

        /// <summary>
        /// Set the guild's current node war signup
        /// </summary>
        public void SetNodeWarSignup(string nodeId)
        {
            CurrentNodeWarSignup = nodeId;
            NodeWarSignupTime = DateTime.UtcNow;
        }

        /// <summary>
        /// Clear the guild's node war signup
        /// </summary>
        public void ClearNodeWarSignup()
        {
            CurrentNodeWarSignup = null;
            NodeWarSignupTime = null;
        }

        /// <summary>
        /// Check if guild is signed up for a node war
        /// </summary>
        public bool IsSignedUpForNodeWar()
        {
            return CurrentNodeWarSignup != null;
        }
    }

    // Lightweight DTO used when syncing guild metadata & claims to clients
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class GuildSummary
    {
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public int DisplayColor { get; set; }
        public int SecondaryColor { get; set; }
        public int Points { get; set; } = 0;
        public string RankClass { get; set; } = "D"; // Guild rank class based on points (e.g., "S", "A", "B", "C", "D")
        public int MemberPointsContribution { get; set; } = 0;
        public string MemberRank { get; set; } = "Guild Member";
        // Claims: store as minimal land-claim DTO
        public List<LandClaimDto> Claims { get; set; } = new();

        // New: Member information for current player (if they're a member)
        public string PlayerRole { get; set; } = ""; // Empty if player is not a member
        public bool IsPlayerMember { get; set; } = false;
        public int MemberCount { get; set; } = 0;
        // New: Player permissions for client-side validation (prevents desync)
        public bool HasBreakPlacePermission { get; set; } = false;
        public bool HasInteractPermission { get; set; } = false;

        // New: List of member UIDs for filtering purposes (doesn't include roles or other sensitive data)
        public List<string> MemberUids { get; set; } = new();

        // New: List of player UIDs who have pending invites to this guild
        public List<string> PendingInviteUids { get; set; } = new();

        // New: Full pending invites with details (for guild members with invite permissions)
        public List<GuildInviteDto> PendingInvites { get; set; } = new();

        // New: Role definitions for the guild (needed for role management dialog)
        public Dictionary<string, GuildRole> Roles { get; set; } = new();

        // New: Current maximum claims allowed (calculated server-side based on config)
        public int MaxClaims { get; set; } = 0;

        // New: Current maximum outposts allowed (calculated server-side based on config)
        public int MaxOutposts { get; set; } = 0;

        // New: Tech progression data (synced to clients)
        public Dictionary<int, GuildTechProgress> TechProgress { get; set; } = new();

        // New: Personal unlock tracking (synced to clients for current player only)
        public Dictionary<int, bool> TechRequiresPersonalUnlock { get; set; } = new();
        public Dictionary<string, PlayerTechProgress> PlayerTechProgress { get; set; } = new();

        // Helper method to get or create tech progress for a specific tech block
        public GuildTechProgress GetOrCreateTechProgress(int techBlockId)
        {
            if (!TechProgress.ContainsKey(techBlockId))
            {
                TechProgress[techBlockId] = new GuildTechProgress { TechBlockId = techBlockId };
            }
            return TechProgress[techBlockId];
        }

        // Helper method to check if a tech is unlocked
        public bool IsTechUnlocked(int techBlockId)
        {
            return TechProgress.TryGetValue(techBlockId, out var progress) && progress.IsUnlocked;
        }
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class LandClaimDto
    {
        public int ChunkX { get; set; }
        public int ChunkZ { get; set; }
        public string ClaimedByUid { get; set; } = null!;
        public DateTime Timestamp { get; set; }

        // New: Indicate if this claim is part of a guild home
        public bool IsGuildHome { get; set; } = false;

        // New: For guild homes, store the center coordinates
        public int? HomeCenterX { get; set; }
        public int? HomeCenterZ { get; set; }

        // New: Indicate if this claim is an outpost
        public bool IsOutpost { get; set; } = false;

        // New: For outposts, store the outpost name
        public string OutpostName { get; set; } = "";
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class GuildInviteDto
    {
        public string InviterUid { get; set; } = null!;
        public string InviteeUid { get; set; } = null!;
        public DateTime ExpiresAt { get; set; }
    }
}
