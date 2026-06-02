using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;

namespace SOAGuildsAndKingdomsPVP.src.utils
{
    /// <summary>
    /// Utility methods for position calculations
    /// </summary>
    public static class PositionUtils
    {
        /// <summary>
        /// Calculate position as offset from the world's default spawn position
        /// </summary>
        /// <param name="world">The game world</param>
        /// <param name="absolutePos">The absolute position to convert</param>
        /// <returns>Position offset from spawn</returns>
        public static Vec3d GetOffsetFromSpawn(IWorldAccessor world, Vec3d absolutePos)
        {
            var spawnPos = world.DefaultSpawnPosition.XYZ;
            return absolutePos.Sub(spawnPos);
        }

        /// <summary>
        /// Calculate position as offset from the world's default spawn position
        /// </summary>
        /// <param name="world">The game world</param>
        /// <param name="x">Absolute X position</param>
        /// <param name="y">Absolute Y position</param>
        /// <param name="z">Absolute Z position</param>
        /// <returns>Position offset from spawn</returns>
        public static Vec3d GetOffsetFromSpawn(IWorldAccessor world, double x, double y, double z)
        {
            var absolutePos = new Vec3d(x, y, z);
            return GetOffsetFromSpawn(world, absolutePos);
        }

        /// <summary>
        /// Calculate absolute position from an offset relative to spawn
        /// </summary>
        /// <param name="world">The game world</param>
        /// <param name="offsetPos">Position offset from spawn</param>
        /// <returns>Absolute position in the world</returns>
        public static Vec3d GetAbsoluteFromSpawnOffset(IWorldAccessor world, Vec3d offsetPos)
        {
            var spawnPos = world.DefaultSpawnPosition.XYZ;
            return offsetPos.Add(spawnPos);
        }

        /// <summary>
        /// Calculate absolute position from an offset relative to spawn
        /// </summary>
        /// <param name="world">The game world</param>
        /// <param name="x">Offset X position</param>
        /// <param name="y">Offset Y position</param>
        /// <param name="z">Offset Z position</param>
        /// <returns>Absolute position in the world</returns>
        public static Vec3d GetAbsoluteFromSpawnOffset(IWorldAccessor world, double x, double y, double z)
        {
            var offsetPos = new Vec3d(x, y, z);
            return GetAbsoluteFromSpawnOffset(world, offsetPos);
        }
    }
}
