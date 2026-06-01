using System;
using System.Collections.Generic;

namespace SRGuildsAndKingdoms.src.config
{
    /// <summary>
    /// Represents a single whitelist entry for a protected zone
    /// </summary>
    public class ZoneWhitelistEntry
    {
        /// <summary>
        /// The unique ID of the protected zone
        /// </summary>
        public int ZoneId { get; set; } = 0;

        /// <summary>
        /// The name of the protected zone (deprecated, kept for backwards compatibility)
        /// </summary>
        public string ZoneName { get; set; } = string.Empty;

        /// <summary>
        /// List of player UIDs who are whitelisted for this zone
        /// </summary>
        public List<string> WhitelistedPlayerUids { get; set; } = new();

        /// <summary>
        /// When this entry was last modified (for auditing)
        /// </summary>
        public DateTime LastModified { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Root container for zone whitelist data
    /// </summary>
    public class ZoneWhitelistData
    {
        /// <summary>
        /// All whitelist entries
        /// </summary>
        public List<ZoneWhitelistEntry> Whitelists { get; set; } = new();
    }
}
