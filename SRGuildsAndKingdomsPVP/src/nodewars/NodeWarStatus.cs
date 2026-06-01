namespace SRGuildsAndKingdomsPVP.src.nodewars
{
    /// <summary>
    /// Represents the current status of a node war
    /// </summary>
    public enum NodeWarStatus
    {
        /// <summary>
        /// No war is scheduled or active for this node
        /// </summary>
        None,

        /// <summary>
        /// Node war is scheduled but not yet started
        /// </summary>
        Scheduled,

        /// <summary>
        /// Node war is currently in progress
        /// </summary>
        Active,

        /// <summary>
        /// Node war has been completed
        /// </summary>
        Completed,

        /// <summary>
        /// Node war was cancelled by an administrator
        /// </summary>
        Cancelled
    }
}
