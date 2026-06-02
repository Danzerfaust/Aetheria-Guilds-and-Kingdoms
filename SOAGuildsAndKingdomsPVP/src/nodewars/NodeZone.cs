using Vintagestory.API.MathTools;
using System;
using System.Collections.Generic;

namespace SOAGuildsAndKingdomsPVP.src.nodewars
{
    /// <summary>
    /// Defines a capturable node zone in the world
    /// </summary>
    public class NodeZone
    {
        /// <summary>
        /// Unique identifier for this node
        /// </summary>
        public string NodeId { get; set; }

        /// <summary>
        /// Display name of the node
        /// </summary>
        public string NodeName { get; set; }

        /// <summary>
        /// Center position of the node zone
        /// </summary>
        public Vec3d Center { get; set; }

        /// <summary>
        /// Radius of the node zone in blocks
        /// </summary>
        public int Radius { get; set; }

        /// <summary>
        /// UID of the guild that currently owns this node (null if unclaimed)
        /// </summary>
        public string? OwningGuildUid { get; set; }

        /// <summary>
        /// Name of the guild that currently owns this node
        /// </summary>
        public string? OwningGuildName { get; set; }

        /// <summary>
        /// When this node was last captured
        /// </summary>
        public DateTime? LastCapturedTime { get; set; }

        /// <summary>
        /// Rewards configuration for controlling this node
        /// </summary>
        public NodeRewards Rewards { get; set; }

        /// <summary>
        /// Whether this node can be used for wars
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Description of the node for players
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Capture zones within this node where players must stand to gain points
        /// </summary>
        public Dictionary<string, CaptureZone> CaptureZones { get; set; }

        public NodeZone()
        {
            NodeId = string.Empty;
            NodeName = string.Empty;
            Center = new Vec3d(0, 0, 0);
            Radius = 100;
            OwningGuildUid = null;
            OwningGuildName = null;
            LastCapturedTime = null;
            Rewards = new NodeRewards();
            IsActive = true;
            Description = string.Empty;
            CaptureZones = new Dictionary<string, CaptureZone>();
        }

        public NodeZone(string nodeId, string nodeName, Vec3d center, int radius)
        {
            NodeId = nodeId;
            NodeName = nodeName;
            Center = center;
            Radius = radius;
            OwningGuildUid = null;
            OwningGuildName = null;
            LastCapturedTime = null;
            Rewards = new NodeRewards();
            IsActive = true;
            Description = string.Empty;
            CaptureZones = new Dictionary<string, CaptureZone>();
        }

        /// <summary>
        /// Check if a position is within this node's zone
        /// </summary>
        public bool IsPositionInZone(Vec3d position)
        {
            return Center.DistanceTo(position) <= Radius;
        }

        /// <summary>
        /// Check if a position is within any capture zone
        /// </summary>
        public bool IsPositionInAnyCaptureZone(Vec3d position)
        {
            foreach (var zone in CaptureZones.Values)
            {
                if (zone.IsActive && zone.IsPositionInZone(position))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Get the capture zone that contains the position (if any)
        /// </summary>
        public CaptureZone? GetCaptureZoneAtPosition(Vec3d position)
        {
            foreach (var zone in CaptureZones.Values)
            {
                if (zone.IsActive && zone.IsPositionInZone(position))
                {
                    return zone;
                }
            }
            return null;
        }
    }

    /// <summary>
    /// Rewards for controlling a node
    /// </summary>
    public class NodeRewards
    {
        /// <summary>
        /// Resource gathering bonuses (e.g., "ore:copper" => 20 for 20% bonus)
        /// </summary>
        public Dictionary<string, int> ResourceBonuses { get; set; }

        /// <summary>
        /// Daily influence points awarded to the controlling guild
        /// </summary>
        public int InfluencePerDay { get; set; }

        /// <summary>
        /// Special abilities or buffs granted to guild members
        /// </summary>
        public List<string> SpecialAbilities { get; set; }

        public NodeRewards()
        {
            ResourceBonuses = new Dictionary<string, int>();
            InfluencePerDay = 100;
            SpecialAbilities = new List<string>();
        }
    }
}
