using System;

namespace SRGuildsAndKingdomsPVP.src.nodewars
{
    /// <summary>
    /// Represents a guild's signup/registration for a node war
    /// </summary>
    public class GuildNodeWarSignup
    {
        /// <summary>
        /// Guild unique identifier
        /// </summary>
        public string GuildUid { get; set; }

        /// <summary>
        /// Guild display name
        /// </summary>
        public string GuildName { get; set; }

        /// <summary>
        /// Node ID this guild signed up for
        /// </summary>
        public string NodeId { get; set; }

        /// <summary>
        /// When the guild signed up (real-world UTC time)
        /// </summary>
        public DateTime SignupTime { get; set; }

        /// <summary>
        /// Player UID who initiated the signup (guild leader or authorized member)
        /// </summary>
        public string SignupByPlayerUid { get; set; }

        /// <summary>
        /// Number of guild members online at the time of signup
        /// </summary>
        public int MembersOnlineAtSignup { get; set; }

        /// <summary>
        /// Total guild member count at signup
        /// </summary>
        public int TotalMembersAtSignup { get; set; }

        /// <summary>
        /// Whether this signup has been confirmed (for pre-registration systems)
        /// </summary>
        public bool IsConfirmed { get; set; }

        public GuildNodeWarSignup()
        {
            GuildUid = string.Empty;
            GuildName = string.Empty;
            NodeId = string.Empty;
            SignupTime = DateTime.MinValue;
            SignupByPlayerUid = string.Empty;
            MembersOnlineAtSignup = 0;
            TotalMembersAtSignup = 0;
            IsConfirmed = true;
        }

        public GuildNodeWarSignup(string guildUid, string guildName, string nodeId, string signupByPlayerUid)
        {
            GuildUid = guildUid;
            GuildName = guildName;
            NodeId = nodeId;
            SignupByPlayerUid = signupByPlayerUid;
            SignupTime = DateTime.MinValue;
            MembersOnlineAtSignup = 0;
            TotalMembersAtSignup = 0;
            IsConfirmed = true;
        }
    }

    /// <summary>
    /// Result of a guild signup attempt
    /// </summary>
    public class GuildSignupResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public GuildSignupFailureReason? FailureReason { get; set; }

        public GuildSignupResult(bool success, string message, GuildSignupFailureReason? failureReason = null)
        {
            Success = success;
            Message = message;
            FailureReason = failureReason;
        }

        public static GuildSignupResult Succeeded(string message = "Successfully signed up for node war")
        {
            return new GuildSignupResult(true, message);
        }

        public static GuildSignupResult Failed(string message, GuildSignupFailureReason reason)
        {
            return new GuildSignupResult(false, message, reason);
        }
    }

    /// <summary>
    /// Reasons why a guild signup might fail
    /// </summary>
    public enum GuildSignupFailureReason
    {
        /// <summary>
        /// Node doesn't exist or is not registered
        /// </summary>
        NodeNotFound,

        /// <summary>
        /// Node is not active for wars
        /// </summary>
        NodeNotActive,

        /// <summary>
        /// War is already in progress
        /// </summary>
        WarAlreadyActive,

        /// <summary>
        /// War hasn't been scheduled yet
        /// </summary>
        WarNotScheduled,

        /// <summary>
        /// Guild is already signed up for this war
        /// </summary>
        AlreadySignedUp,

        /// <summary>
        /// Guild is already signed up for another war
        /// </summary>
        SignedUpForAnotherWar,

        /// <summary>
        /// Not enough guild members online
        /// </summary>
        InsufficientMembersOnline,

        /// <summary>
        /// Guild doesn't meet minimum size requirements
        /// </summary>
        GuildTooSmall,

        /// <summary>
        /// Player doesn't have permission to sign up guild
        /// </summary>
        NoPermission,

        /// <summary>
        /// War signup period has closed
        /// </summary>
        SignupPeriodClosed,

        /// <summary>
        /// Maximum number of guilds already signed up
        /// </summary>
        MaxGuildsReached,

        /// <summary>
        /// Player is not in a guild
        /// </summary>
        NotInGuild,

        /// <summary>
        /// Node war system is disabled
        /// </summary>
        SystemDisabled
    }
}
