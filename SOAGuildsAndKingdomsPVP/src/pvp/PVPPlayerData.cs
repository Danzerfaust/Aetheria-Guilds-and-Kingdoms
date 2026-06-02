using System;
using Vintagestory.API.Common;

namespace SOAGuildsAndKingdomsPVP.src.pvp
{
    /// <summary>
    /// Stores PVP state for a player
    /// </summary>
    public class PVPPlayerData
    {
        /// <summary>
        /// Player's unique identifier
        /// </summary>
        public string PlayerUid { get; set; } = string.Empty;

        /// <summary>
        /// Whether the player has PVP enabled
        /// </summary>
        public bool PVPEnabled { get; set; } = false;

        /// <summary>
        /// When the player last toggled PVP (to prevent spam toggling)
        /// </summary>
        public DateTime LastToggleTime { get; set; } = DateTime.MinValue;

        /// <summary>
        /// Cooldown in seconds before player can toggle PVP again
        /// </summary>
        public const int TOGGLE_COOLDOWN_SECONDS = 60;

        /// <summary>
        /// Check if player can toggle PVP (not on cooldown)
        /// </summary>
        public bool CanTogglePVP()
        {
            var timeSinceLastToggle = DateTime.UtcNow - LastToggleTime;
            return timeSinceLastToggle.TotalSeconds >= TOGGLE_COOLDOWN_SECONDS;
        }

        /// <summary>
        /// Get remaining cooldown time in seconds
        /// </summary>
        public int GetRemainingCooldown()
        {
            var timeSinceLastToggle = DateTime.UtcNow - LastToggleTime;
            var remaining = TOGGLE_COOLDOWN_SECONDS - (int)timeSinceLastToggle.TotalSeconds;
            return Math.Max(0, remaining);
        }

        /// <summary>
        /// Toggle PVP state and update last toggle time
        /// </summary>
        public bool TogglePVP()
        {
            if (!CanTogglePVP())
                return false;

            PVPEnabled = !PVPEnabled;
            LastToggleTime = DateTime.UtcNow;
            return true;
        }

        /// <summary>
        /// Set PVP state directly (for admin commands)
        /// </summary>
        public void SetPVPState(bool enabled)
        {
            PVPEnabled = enabled;
            LastToggleTime = DateTime.UtcNow;
        }
    }
}
