using System;
using System.Collections.Generic;
using Vintagestory.API.MathTools;

namespace SRGuildsAndKingdomsPVP.src.nodewars
{
    /// <summary>
    /// Represents an active or scheduled node war
    /// </summary>
    public class NodeWar
    {
        /// <summary>
        /// The node zone this war is for
        /// </summary>
        public string NodeId { get; set; }

        /// <summary>
        /// Center position of the war zone
        /// </summary>
        public Vec3d NodeCenter { get; set; }

        /// <summary>
        /// Radius of the war zone
        /// </summary>
        public int NodeRadius { get; set; }

        /// <summary>
        /// When the war started (real-world UTC time)
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// When the war ended (null if still active)
        /// </summary>
        public DateTime? EndTime { get; set; }

        /// <summary>
        /// Current status of the war
        /// </summary>
        public NodeWarStatus Status { get; set; }

        /// <summary>
        /// Progress tracking for each participating guild
        /// </summary>
        public Dictionary<string, GuildWarProgress> GuildProgress { get; set; }

        /// <summary>
        /// UID of the guild that controlled the node before the war
        /// </summary>
        public string? PreviousControllingGuildUid { get; set; }

        /// <summary>
        /// UID of the guild that currently controls/won the node
        /// </summary>
        public string? ControllingGuildUid { get; set; }

        /// <summary>
        /// Configuration for this node war
        /// </summary>
        public NodeWarConfig Config { get; set; }

        /// <summary>
        /// Whether the war is currently in overtime
        /// </summary>
        public bool IsOvertime { get; set; }

        /// <summary>
        /// When overtime started (null if not in overtime)
        /// </summary>
        public DateTime? OvertimeStartTime { get; set; }

        /// <summary>
        /// Guilds that have signed up for this node war
        /// </summary>
        public Dictionary<string, GuildNodeWarSignup> GuildSignups { get; set; }

        /// <summary>
        /// Maximum number of guilds that can sign up (0 = unlimited)
        /// </summary>
        public int MaxGuilds { get; set; }

        /// <summary>
        /// When the signup period ends (null = no deadline)
        /// </summary>
        public DateTime? SignupDeadline { get; set; }

        public NodeWar()
        {
            NodeId = string.Empty;
            NodeCenter = new Vec3d(0, 0, 0);
            NodeRadius = 100;
            StartTime = DateTime.MinValue;
            EndTime = null;
            Status = NodeWarStatus.Scheduled;
            GuildProgress = new Dictionary<string, GuildWarProgress>();
            PreviousControllingGuildUid = null;
            ControllingGuildUid = null;
            Config = new NodeWarConfig();
            IsOvertime = false;
            OvertimeStartTime = null;
            GuildSignups = new Dictionary<string, GuildNodeWarSignup>();
            MaxGuilds = 0;
            SignupDeadline = null;
        }

        public NodeWar(string nodeId, Vec3d center, int radius, NodeWarConfig config)
        {
            NodeId = nodeId;
            NodeCenter = center;
            NodeRadius = radius;
            StartTime = DateTime.MinValue;
            EndTime = null;
            Status = NodeWarStatus.Scheduled;
            GuildProgress = new Dictionary<string, GuildWarProgress>();
            PreviousControllingGuildUid = null;
            ControllingGuildUid = null;
            Config = config ?? new NodeWarConfig();
            IsOvertime = false;
            OvertimeStartTime = null;
            GuildSignups = new Dictionary<string, GuildNodeWarSignup>();
            MaxGuilds = 0;
            SignupDeadline = null;
        }

        /// <summary>
        /// Check if a position is within this war zone
        /// </summary>
        public bool IsPositionInZone(Vec3d position)
        {
            return NodeCenter.DistanceTo(position) <= NodeRadius;
        }

        /// <summary>
        /// Get the leading guild (highest capture points)
        /// </summary>
        public GuildWarProgress? GetLeadingGuild()
        {
            GuildWarProgress? leader = null;
            double maxPoints = 0;

            foreach (var progress in GuildProgress.Values)
            {
                if (progress.CapturePoints > maxPoints)
                {
                    maxPoints = progress.CapturePoints;
                    leader = progress;
                }
            }

            return leader;
        }

        /// <summary>
        /// Check if the node is currently contested (multiple guilds meeting requirements)
        /// </summary>
        public bool IsContested()
        {
            int guildsWithMinPlayers = 0;

            foreach (var progress in GuildProgress.Values)
            {
                if (progress.PlayersInZone >= Config.MinPlayersToCapture)
                {
                    guildsWithMinPlayers++;
                }
            }

            return guildsWithMinPlayers > 1;
        }

        /// <summary>
        /// Check if a guild is signed up for this war
        /// </summary>
        public bool IsGuildSignedUp(string guildUid)
        {
            return GuildSignups.ContainsKey(guildUid);
        }

        /// <summary>
        /// Get the number of guilds signed up
        /// </summary>
        public int GetSignupCount()
        {
            return GuildSignups.Count;
        }

        /// <summary>
        /// Check if signup period is still open
        /// </summary>
        public bool IsSignupOpen(DateTime currentTime)
        {
            if (Status != NodeWarStatus.Scheduled)
            {
                return false; // Can only sign up for scheduled wars
            }

            if (SignupDeadline.HasValue && currentTime >= SignupDeadline.Value)
            {
                return false; // Signup deadline passed
            }

            if (MaxGuilds > 0 && GuildSignups.Count >= MaxGuilds)
            {
                return false; // Max guilds reached
            }

            return true;
        }
    }
}
