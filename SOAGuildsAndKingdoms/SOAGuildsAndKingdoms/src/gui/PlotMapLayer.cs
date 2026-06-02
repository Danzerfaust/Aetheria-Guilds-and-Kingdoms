using SOAGuildsAndKingdoms.src.guilds;
using SOAGuildsAndKingdoms.src.network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace SOAGuildsAndKingdoms.src.gui
{
    public class PlotMapLayer : RGBMapLayer
    {
        private ICoreClientAPI clientApi;
        private Dictionary<Vec2i, ChunkData> chunkDataCache;
        private SOAGuildsAndKingdomsModSystem modSystem;
        private DialogGuildMain? activeGuildDialog;
        private (int chunkX, int chunkZ)? hoveredChunk = null;

        // Cache for last frame's calculations
        private int lastFrameChunkCount = 0;

        // Territorial restriction settings (cached from server)
        private bool territorialRestrictionsEnabled = false;
        private (int x, int z)? territorialCenter = null;
        private int territorialRadius = 1000;
        private long lastConfigUpdate = 0;

        // Protected zones settings (cached from server)
        private bool protectedZonesEnabled = false;
        private List<(string name, int x, int z, int radius, List<string> whitelistedPlayers)> protectedZones = new();

        // Node settings (cached from server)
        private List<(string name, int x, int z, int radius)> nodes = new();

        public override string Title => "guildclaims";

        public override string LayerGroupCode => "guildclaims";
        public override EnumMapAppSide DataSide => EnumMapAppSide.Client;
        public override EnumMinMagFilter MinFilter => EnumMinMagFilter.Linear; // Linear filtering for smooth rendering
        public override EnumMinMagFilter MagFilter => EnumMinMagFilter.Linear; // Linear filtering for smooth rendering
        public override MapLegendItem[] LegendItems => null; // No legend items needed for this layer

        public PlotMapLayer(ICoreAPI api, IWorldMapManager mapSink) : base(api, mapSink)
        {
            clientApi = api as ICoreClientAPI;
            chunkDataCache = new Dictionary<Vec2i, ChunkData>();

            // Get reference to the mod system to access guild data
            modSystem = api.ModLoader.GetModSystem<SOAGuildsAndKingdomsModSystem>();

            // Register this layer instance with the mod system
            modSystem.RegisterPlotMapLayer(this);
        }

        // Method to be called by DialogGuildMain when it opens/closes
        public void SetActiveGuildDialog(DialogGuildMain? dialog)
        {
            // Add debug logging
            System.Diagnostics.Debug.WriteLine($"PlotMapLayer.SetActiveGuildDialog called with: {(dialog != null ? "valid dialog" : "null")}");
            System.Diagnostics.Debug.WriteLine($"PlotMapLayer instance: {this.GetHashCode()}");
            activeGuildDialog = dialog;
            System.Diagnostics.Debug.WriteLine($"activeGuildDialog is now: {(activeGuildDialog != null ? "set" : "null")}");
        }

        /// <summary>
        /// Update config cache from server data
        /// </summary>
        public void UpdateConfigFromServer(GuildConfigPacket config)
        {
            System.Diagnostics.Debug.WriteLine($"PlotMapLayer.UpdateConfigFromServer called - Before: protectedZonesEnabled={protectedZonesEnabled}");
            System.Diagnostics.Debug.WriteLine($"Config packet values - ProtectedZonesEnabled={config.ProtectedZonesEnabled}, ProtectedZones count={config.ProtectedZones?.Count ?? 0}");

            // Update territorial restrictions
            territorialRestrictionsEnabled = config.TerritorialRestrictionsEnabled;
            if (config.TerritorialCenterX.HasValue && config.TerritorialCenterZ.HasValue)
            {
                territorialCenter = (config.TerritorialCenterX.Value, config.TerritorialCenterZ.Value);
            }
            else
            {
                territorialCenter = null;
            }
            territorialRadius = config.TerritorialRadius;

            // Update protected zones
            protectedZonesEnabled = config.ProtectedZonesEnabled;
            protectedZones.Clear();
            if (config.ProtectedZones != null)
            {
                foreach (var zone in config.ProtectedZones)
                {
                    protectedZones.Add((zone.Name, zone.X, zone.Z, zone.Radius, zone.WhitelistedPlayers));
                    System.Diagnostics.Debug.WriteLine($"Added protected zone: {zone.Name} at ({zone.X}, {zone.Z}) radius={zone.Radius}");
                }
            }

            // Update nodes
            nodes.Clear();
            if (config.Nodes != null)
            {
                foreach (var node in config.Nodes)
                {
                    nodes.Add((node.Name, node.X, node.Z, node.Radius));
                    System.Diagnostics.Debug.WriteLine($"Added node: {node.Name} at ({node.X}, {node.Z}) radius={node.Radius}");
                }
            }

            lastConfigUpdate = DateTime.UtcNow.Ticks;

            System.Diagnostics.Debug.WriteLine($"PlotMapLayer config updated - After: Territorial={territorialRestrictionsEnabled}, Protected Zones={protectedZonesEnabled} ({protectedZones.Count} zones), Nodes=({nodes.Count} nodes)");
        }

        /// <summary>
        /// Check if a chunk is within territorial restrictions
        /// </summary>
        private bool IsChunkWithinTerritorialBounds(int chunkX, int chunkZ)
        {
            if (!territorialRestrictionsEnabled || territorialCenter == null)
            {
                return true; // No restrictions enabled
            }

            // Get map size for offset calculation
            var mapSize = clientApi.World.BlockAccessor.MapSize;

            // Check all four corners of the chunk to ensure entire chunk is within bounds
            const int chunkSize = 32; // VintageStory chunk size
            int blockX1 = chunkX * chunkSize;
            int blockZ1 = chunkZ * chunkSize;
            int blockX2 = blockX1 + chunkSize - 1;
            int blockZ2 = blockZ1 + chunkSize - 1;

            // Check if all corners are within bounds
            return IsWithinTerritorialBounds(blockX1, blockZ1, mapSize) &&
                   IsWithinTerritorialBounds(blockX2, blockZ1, mapSize) &&
                   IsWithinTerritorialBounds(blockX1, blockZ2, mapSize) &&
                   IsWithinTerritorialBounds(blockX2, blockZ2, mapSize);
        }

        /// <summary>
        /// Check if a block position is within territorial bounds
        /// </summary>
        private bool IsWithinTerritorialBounds(int blockX, int blockZ, Vec3i mapSize)
        {
            if (!territorialRestrictionsEnabled || territorialCenter == null)
            {
                return true; // No restrictions enabled
            }

            // Match server-side calculation with map size offset
            double deltaX = blockX - territorialCenter.Value.x - mapSize.X / 2;
            double deltaZ = blockZ - territorialCenter.Value.z - mapSize.Z / 2;
            double distance = Math.Sqrt(deltaX * deltaX + deltaZ * deltaZ);

            return distance <= territorialRadius;
        }

        /// <summary>
        /// Check if a chunk is within any protected zone
        /// </summary>
        private bool IsChunkWithinProtectedZone(int chunkX, int chunkZ)
        {
            if (!protectedZonesEnabled || protectedZones == null || protectedZones.Count == 0)
            {
                return false;
            }

            // Get spawn position for offset calculation
            var spawnPos = clientApi.World.DefaultSpawnPosition.AsBlockPos;

            // Check all four corners of the chunk
            const int chunkSize = 32;
            int blockX1 = chunkX * chunkSize;
            int blockZ1 = chunkZ * chunkSize;
            int blockX2 = blockX1 + chunkSize - 1;
            int blockZ2 = blockZ1 + chunkSize - 1;
            // Check if any corner is within a protected zone
            return IsWithinProtectedZone(blockX1, blockZ1, spawnPos) ||
                   IsWithinProtectedZone(blockX2, blockZ1, spawnPos) ||
                   IsWithinProtectedZone(blockX1, blockZ2, spawnPos) ||
                   IsWithinProtectedZone(blockX2, blockZ2, spawnPos);
        }

        /// <summary>
        /// Check if a block position is within any protected zone
        /// </summary>
        private bool IsWithinProtectedZone(int blockX, int blockZ, BlockPos spawnPos)
        {
            if (!protectedZonesEnabled || protectedZones == null || protectedZones.Count == 0)
            {
                return false;
            }

            foreach (var zone in protectedZones)
            {
                // Calculate distance relative to spawn position
                double deltaX = blockX - zone.x - spawnPos.X;
                double deltaZ = blockZ - zone.z - spawnPos.Z;
                double distance = Math.Sqrt(deltaX * deltaX + deltaZ * deltaZ);

                if (distance <= zone.radius)
                {
                    return true;
                }
            }

            return false;
        }

		/// <summary>
		/// Get the protected zone at a specific position, if any (public API for mod system)
		/// </summary>
		public (string name, int x, int z, int radius, List<string> whitelistedPlayers)? GetProtectedZoneAt(int blockX, int blockZ)
		{
			var spawnPos = clientApi?.World.DefaultSpawnPosition.AsBlockPos;
			if (spawnPos == null)
				return null;

			return GetProtectedZoneAtInternal(blockX, blockZ, spawnPos);
		}

		/// <summary>
		/// Get the protected zone at a specific position, if any (with explicit spawn position)
		/// </summary>
		private (string name, int x, int z, int radius, List<string> whitelistedPlayers)? GetProtectedZoneAt(int blockX, int blockZ, BlockPos spawnPos)
		{
			return GetProtectedZoneAtInternal(blockX, blockZ, spawnPos);
		}

		/// <summary>
		/// Check if a chunk is within any node
		/// </summary>
		private bool IsChunkWithinNode(int chunkX, int chunkZ)
        {
            if (nodes == null || nodes.Count == 0)
            {
                return false;
            }

            // Get spawn position for offset calculation
            var spawnPos = clientApi.World.DefaultSpawnPosition.AsBlockPos;

            // Check all four corners of the chunk
            const int chunkSize = 32;
            int blockX1 = chunkX * chunkSize;
            int blockZ1 = chunkZ * chunkSize;
            int blockX2 = blockX1 + chunkSize - 1;
            int blockZ2 = blockZ1 + chunkSize - 1;

            // Check if any corner is within a node
            return IsWithinNode(blockX1, blockZ1, spawnPos) ||
                   IsWithinNode(blockX2, blockZ1, spawnPos) ||
                   IsWithinNode(blockX1, blockZ2, spawnPos) ||
                   IsWithinNode(blockX2, blockZ2, spawnPos);
        }

        /// <summary>
        /// Check if a block position is within any node
        /// </summary>
        private bool IsWithinNode(int blockX, int blockZ, BlockPos spawnPos)
        {
            if (nodes == null || nodes.Count == 0)
            {
                return false;
            }

            foreach (var node in nodes)
            {
                // Calculate distance relative to spawn position
                double deltaX = blockX - node.x - spawnPos.X;
                double deltaZ = blockZ - node.z - spawnPos.Z;
                double distance = Math.Sqrt(deltaX * deltaX + deltaZ * deltaZ);

                if (distance <= node.radius)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Get the protected zone at a specific position, if any
        /// </summary>
        private (string name, int x, int z, int radius, List<string> whitelistedPlayers)? GetProtectedZoneAtInternal(int blockX, int blockZ, BlockPos spawnPos)
        {
            if (!protectedZonesEnabled || protectedZones == null || protectedZones.Count == 0)
            {
                return null;
            }

            foreach (var zone in protectedZones)
            {
                // Calculate distance relative to spawn position
                double deltaX = blockX - zone.x - spawnPos.X;
                double deltaZ = blockZ - zone.z - spawnPos.Z;
                double distance = Math.Sqrt(deltaX * deltaX + deltaZ * deltaZ);

                if (distance <= zone.radius)
                {
                    return zone;
                }
            }

            return null;
        }

        /// <summary>
        /// Get the node at a specific position, if any
        /// </summary>
        private (string name, int x, int z, int radius)? GetNodeAt(int blockX, int blockZ, BlockPos spawnPos)
        {
            if (nodes == null || nodes.Count == 0)
            {
                return null;
            }

            foreach (var node in nodes)
            {
                // Calculate distance relative to spawn position
                double deltaX = blockX - node.x - spawnPos.X;
                double deltaZ = blockZ - node.z - spawnPos.Z;
                double distance = Math.Sqrt(deltaX * deltaX + deltaZ * deltaZ);

                if (distance <= node.radius)
                {
                    return node;
                }
            }

            return null;
        }

        /// <summary>
        /// Check if a chunk is too close (within 300 blocks) to another guild's claim
        /// </summary>
        private (bool tooClose, string nearestGuildName, double distance) IsChunkTooCloseToOtherGuildClaim(
            int chunkX, int chunkZ, string currentGuildName,
            Dictionary<Vec2i, (string guildName, GuildSummary guildSummary, LandClaimDto claim)> claimedChunks)
        {
            const int minDistance = 300; // Minimum distance in blocks
            const int chunkSize = 32; // VintageStory chunk size

            // Calculate the center block position of the chunk being checked
            int centerBlockX = chunkX * chunkSize + chunkSize / 2;
            int centerBlockZ = chunkZ * chunkSize + chunkSize / 2;

            double nearestDistance = double.MaxValue;
            string nearestGuild = null;

            // Check all claimed chunks
            foreach (var claimEntry in claimedChunks)
            {
                var (guildName, guildSummary, claim) = claimEntry.Value;

                // Skip claims from the same guild
                if (guildName == currentGuildName)
                {
                    continue;
                }

                // Calculate the center block position of the claimed chunk
                int claimCenterX = claim.ChunkX * chunkSize + chunkSize / 2;
                int claimCenterZ = claim.ChunkZ * chunkSize + chunkSize / 2;

                // Calculate distance between the two chunk centers
                double deltaX = centerBlockX - claimCenterX;
                double deltaZ = centerBlockZ - claimCenterZ;
                double distance = Math.Sqrt(deltaX * deltaX + deltaZ * deltaZ);

                // Track the nearest claim
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearestGuild = guildName;
                }

                // If within minimum distance, return immediately
                if (distance < minDistance)
                {
                    return (true, guildName, distance);
                }
            }

            return (false, nearestGuild, nearestDistance);
        }

        /// <summary>
        /// Convert ARGB integer color to Vec4f for rendering
        /// </summary>
        private Vec4f ArgbToRgbaVec4f(int argbColor)
        {
            byte a = (byte)((argbColor >> 24) & 0xFF);
            byte r = (byte)((argbColor >> 16) & 0xFF);
            byte g = (byte)((argbColor >> 8) & 0xFF);
            byte b = (byte)(argbColor & 0xFF);

            return new Vec4f(
                r / 255f,
                g / 255f,
                b / 255f,
                a / 255f
            );
        }

        private int? whiteTextureId = null;

        /// <summary>
        /// Renders a filled rectangle with the specified color
        /// </summary>
        private void RenderFilledRectangle(int textureId, ElementBounds mapBounds, float x1, float y1, float width, float height, float z, Vec4f color)
        {
            if (textureId <= 0) return;

            clientApi.Render.Render2DTexture(textureId,
                (int)(mapBounds.renderX + x1),
                (int)(mapBounds.renderY + y1),
                (int)width,
                (int)height,
                z, color);
        }

        /// <summary>
        /// Renders a border around a rectangle with the specified color and width
        /// </summary>
        private void RenderBorder(int textureId, ElementBounds mapBounds, float x1, float y1, float x2, float y2, float borderWidth, float z, Vec4f color)
        {
            if (textureId <= 0) return;

            float width = x2 - x1;
            float height = y2 - y1;

            // Top border
            RenderFilledRectangle(textureId, mapBounds, x1, y1, width, borderWidth, z, color);
            // Right border  
            RenderFilledRectangle(textureId, mapBounds, x2 - borderWidth, y1, borderWidth, height, z, color);
            // Bottom border
            RenderFilledRectangle(textureId, mapBounds, x1, y2 - borderWidth, width, borderWidth, z, color);
            // Left border
            RenderFilledRectangle(textureId, mapBounds, x1, y1, borderWidth, height, z, color);
        }

        /// <summary>
        /// Renders selective borders based on which sides should have borders
        /// </summary>
        private void RenderSelectiveBorder(int textureId, ElementBounds mapBounds, float x1, float y1, float x2, float y2,
            float borderWidth, float z, Vec4f color, bool drawTop, bool drawRight, bool drawBottom, bool drawLeft)
        {
            if (textureId <= 0) return;

            float width = x2 - x1;
            float height = y2 - y1;

            // Top border
            if (drawTop)
                RenderFilledRectangle(textureId, mapBounds, x1, y1, width, borderWidth, z, color);
            // Right border  
            if (drawRight)
                RenderFilledRectangle(textureId, mapBounds, x2 - borderWidth, y1, borderWidth, height, z, color);
            // Bottom border
            if (drawBottom)
                RenderFilledRectangle(textureId, mapBounds, x1, y2 - borderWidth, width, borderWidth, z, color);
            // Left border
            if (drawLeft)
                RenderFilledRectangle(textureId, mapBounds, x1, y1, borderWidth, height, z, color);
        }

        /// <summary>
        /// Renders a claimed chunk with guild colors
        /// </summary>
        private void RenderClaimedChunk(ElementBounds mapBounds, float x1, float y1, float x2, float y2, GuildSummary guildSummary,
            bool isGuildHome = false, bool drawTopBorder = true, bool drawRightBorder = true, bool drawBottomBorder = true, bool drawLeftBorder = true)
        {
            if (!whiteTextureId.HasValue || whiteTextureId.Value <= 0) return;

            float width = x2 - x1;
            float height = y2 - y1;

            // Render filled square for guild claims using primary color
            var guildColor = ArgbToRgbaVec4f(guildSummary.DisplayColor);

            // Make guild home slightly more opaque
            guildColor.A = isGuildHome ? 0.6f : 0.4f;

            // Render filled rectangle with primary color
            RenderFilledRectangle(whiteTextureId.Value, mapBounds, x1, y1, width, height, 50f, guildColor);

            // Render border with secondary color at full opacity (only on edges that need borders)
            var secondaryColor = ArgbToRgbaVec4f(guildSummary.SecondaryColor);
            secondaryColor.A = 1.0f;
            float borderWidth = isGuildHome ? 3f : 2f; // Thicker border for guild homes

            RenderSelectiveBorder(whiteTextureId.Value, mapBounds, x1, y1, x2, y2, borderWidth, 51f, secondaryColor,
                drawTopBorder, drawRightBorder, drawBottomBorder, drawLeftBorder);
        }

        /// <summary>
        /// Renders a pending unclaim chunk with red/orange overlay
        /// </summary>
        private void RenderPendingUnclaimChunk(ElementBounds mapBounds, float x1, float y1, float x2, float y2)
        {
            if (!whiteTextureId.HasValue || whiteTextureId.Value <= 0) return;

            float width = x2 - x1;
            float height = y2 - y1;

            // Render pending unclaims with red/orange color
            var pendingUnclaimColor = ColorUtil.ToRGBAVec4f(ColorUtil.ColorFromRgba(0, 100, 255, 120));

            // Render filled rectangle for pending unclaim
            RenderFilledRectangle(whiteTextureId.Value, mapBounds, x1, y1, width, height, 52f, pendingUnclaimColor);

            // Render border for pending unclaim
            var pendingUnclaimBorderColor = ColorUtil.ToRGBAVec4f(ColorUtil.ColorFromRgba(0, 50, 255, 255));
            float borderWidth = 2f;

            RenderBorder(whiteTextureId.Value, mapBounds, x1, y1, x2, y2, borderWidth, 53f, pendingUnclaimBorderColor);
        }

        /// <summary>
        /// Renders a pending claim chunk
        /// </summary>
        private void RenderPendingChunk(ElementBounds mapBounds, float x1, float y1, float x2, float y2, bool isPendingGuildHome = false)
        {
            if (!whiteTextureId.HasValue || whiteTextureId.Value <= 0) return;

            float width = x2 - x1;
            float height = y2 - y1;

            // Render pending claims with yellow color
            var pendingColor = ColorUtil.ToRGBAVec4f(ColorUtil.ColorFromRgba(255, 255, 0, isPendingGuildHome ? 150 : 102));

            // Render filled rectangle for pending claim
            RenderFilledRectangle(whiteTextureId.Value, mapBounds, x1, y1, width, height, 50f, pendingColor);

            // Render border for pending claim
            var pendingBorderColor = ColorUtil.ToRGBAVec4f(ColorUtil.ColorFromRgba(255, 255, 0, 255));
            float borderWidth = isPendingGuildHome ? 3f : 2f; // Thicker border for pending guild homes

            RenderBorder(whiteTextureId.Value, mapBounds, x1, y1, x2, y2, borderWidth, 51f, pendingBorderColor);
        }

        /// <summary>
        /// Renders a hover highlight for claiming mode
        /// </summary>
        private void RenderHoverHighlight(ElementBounds mapBounds, float x1, float y1, float x2, float y2, bool canClaim)
        {
            if (!whiteTextureId.HasValue || whiteTextureId.Value <= 0) return;

            float width = x2 - x1;
            float height = y2 - y1;

            // Choose hover color based on whether the chunk can be claimed
            Vec4f hoverColor;
            if (!canClaim)
            {
                // Red color for restricted areas
                hoverColor = ColorUtil.ToRGBAVec4f(ColorUtil.ColorFromRgba(255, 100, 100, 200));
            }
            else
            {
                // White color for allowed areas
                hoverColor = ColorUtil.ToRGBAVec4f(ColorUtil.ColorFromRgba(255, 255, 255, 255));
            }

            // Render filled rectangle for hover highlight
            RenderFilledRectangle(whiteTextureId.Value, mapBounds, x1, y1, width, height, 52f, hoverColor);
        }

        /// <summary>
        /// Renders a territorial restriction overlay
        /// </summary>
        private void RenderTerritorialRestriction(ElementBounds mapBounds, float x1, float y1, float x2, float y2)
        {
            if (!whiteTextureId.HasValue || whiteTextureId.Value <= 0) return;

            float width = x2 - x1;
            float height = y2 - y1;

            // Show red overlay for restricted areas (more visible now that it's always shown)
            var restrictedColor = ColorUtil.ToRGBAVec4f(ColorUtil.ColorFromRgba(255, 0, 0, 40));

            RenderFilledRectangle(whiteTextureId.Value, mapBounds, x1, y1, width, height, 49f, restrictedColor);

            // Add a red border to make territorial restrictions more visible
            var borderColor = ColorUtil.ToRGBAVec4f(ColorUtil.ColorFromRgba(255, 0, 0, 150));
            RenderBorder(whiteTextureId.Value, mapBounds, x1, y1, x2, y2, 1.5f, 50f, borderColor);
        }

        /// <summary>
        /// Renders a protected zone overlay
        /// </summary>
        private void RenderProtectedZone(ElementBounds mapBounds, float x1, float y1, float x2, float y2)
        {
            if (!whiteTextureId.HasValue || whiteTextureId.Value <= 0) return;

            float width = x2 - x1;
            float height = y2 - y1;

            // Show purple overlay for protected zones
            var protectedColor = ColorUtil.ToRGBAVec4f(ColorUtil.ColorFromRgba(128, 0, 255, 50));

            RenderFilledRectangle(whiteTextureId.Value, mapBounds, x1, y1, width, height, 49f, protectedColor);

            // Add a purple border to make it more visible
            var borderColor = ColorUtil.ToRGBAVec4f(ColorUtil.ColorFromRgba(128, 0, 255, 180));
            RenderBorder(whiteTextureId.Value, mapBounds, x1, y1, x2, y2, 2f, 50f, borderColor);
        }

        /// <summary>
        /// Renders a node overlay
        /// </summary>
        private void RenderNode(ElementBounds mapBounds, float x1, float y1, float x2, float y2)
        {
            if (!whiteTextureId.HasValue || whiteTextureId.Value <= 0) return;

            float width = x2 - x1;
            float height = y2 - y1;

            // Show cyan/teal overlay for nodes
            var nodeColor = ColorUtil.ToRGBAVec4f(ColorUtil.ColorFromRgba(0, 255, 200, 50));

            RenderFilledRectangle(whiteTextureId.Value, mapBounds, x1, y1, width, height, 49f, nodeColor);

            // Add a cyan border to make it more visible
            var borderColor = ColorUtil.ToRGBAVec4f(ColorUtil.ColorFromRgba(0, 255, 200, 180));
            RenderBorder(whiteTextureId.Value, mapBounds, x1, y1, x2, y2, 2f, 50f, borderColor);
        }

        /// <summary>
        /// Renders the territorial center marker and boundary circle outline
        /// </summary>
        private void RenderTerritorialBoundary(GuiElementMap mapElem, ElementBounds mapBounds)
        {
            if (!territorialRestrictionsEnabled || !territorialCenter.HasValue || !whiteTextureId.HasValue) return;

            int chunkSize = guilds.LandClaim.ChunkSize;
            var spawnPos = clientApi.World.DefaultSpawnPosition.AsBlockPos;
            var center = territorialCenter.Value;

            // Calculate center in world coordinates (accounting for spawn offset)
            double worldX = center.x + spawnPos.X;
            double worldZ = center.z + spawnPos.Z;

            // Check if the boundary circle overlaps with the viewport before rendering
            Vec2f centerViewPos = new Vec2f();
            mapElem.TranslateWorldPosToViewPos(new Vec3d(worldX, 0, worldZ), ref centerViewPos);

            // Calculate the circle's screen radius and check if it could be visible
            double zoomLevel = mapElem.ZoomLevel;
            float screenRadius = (float)(territorialRadius * zoomLevel);
            float margin = screenRadius + 100f; // Extra margin for safety

            // Early exit if the entire circle is outside the viewport
            if (centerViewPos.X + margin < 0 || centerViewPos.X - margin > mapBounds.InnerWidth ||
                centerViewPos.Y + margin < 0 || centerViewPos.Y - margin > mapBounds.InnerHeight)
            {
                return; // Circle is completely outside viewport
            }

            // Adaptive sampling based on zoom level and radius
            int radiusInChunks = (int)Math.Ceiling((double)territorialRadius / chunkSize);

            // OPTIMIZATION: Much more aggressive reduction when zoomed out
            int baseSamplesPerQuadrant = Math.Max(15, radiusInChunks / 2);

            // Scale dramatically with zoom - very few samples when zoomed out
            int samplesPerQuadrant = (int)(baseSamplesPerQuadrant * Math.Max(0.15, Math.Pow(zoomLevel, 1.5)));

            // Ensure minimum samples for visibility but cap maximum
            samplesPerQuadrant = Math.Max(12, Math.Min(samplesPerQuadrant, 60));

            var boundaryColor = ColorUtil.ToRGBAVec4f(ColorUtil.ColorFromRgba(255, 100, 0, 200)); // Orange color

            // Draw the boundary circle by sampling points around the perimeter
            for (int i = 0; i < samplesPerQuadrant * 4; i++)
            {
                double angle = (i / (double)(samplesPerQuadrant * 4)) * Math.PI * 2;
                double x = worldX + Math.Cos(angle) * territorialRadius;
                double z = worldZ + Math.Sin(angle) * territorialRadius;

                // Convert world position to map position
                Vec2f viewPos = new Vec2f();
                mapElem.TranslateWorldPosToViewPos(new Vec3d(x, 0, z), ref viewPos);

                // Viewport culling for individual dots
                if (viewPos.X < -10 || viewPos.X > mapBounds.InnerWidth + 10 ||
                    viewPos.Y < -10 || viewPos.Y > mapBounds.InnerHeight + 10)
                {
                    continue;
                }

                // Draw a small marker - size scales with zoom
                float dotSize = Math.Max(2, 3 * (float)zoomLevel);
                RenderFilledRectangle(whiteTextureId.Value, mapBounds,
                    viewPos.X - dotSize / 2, viewPos.Y - dotSize / 2,
                    dotSize, dotSize, 55f, boundaryColor);
            }

            // Draw center marker only if visible
            if (centerViewPos.X >= -50 && centerViewPos.X <= mapBounds.InnerWidth + 50 &&
                centerViewPos.Y >= -50 && centerViewPos.Y <= mapBounds.InnerHeight + 50)
            {
                var centerColor = ColorUtil.ToRGBAVec4f(ColorUtil.ColorFromRgba(255, 100, 0, 255)); // Bright orange
                float markerSize = Math.Max(6f, 8f * (float)zoomLevel);
                RenderFilledRectangle(whiteTextureId.Value, mapBounds,
                    centerViewPos.X - markerSize / 2, centerViewPos.Y - markerSize / 2,
                    markerSize, markerSize, 56f, centerColor);
            }
        }

        /// <summary>
        /// Renders protected zone center markers and boundary circles
        /// </summary>
        private void RenderProtectedZoneBoundaries(GuiElementMap mapElem, ElementBounds mapBounds)
        {
            if (!protectedZonesEnabled || protectedZones == null || protectedZones.Count == 0 || !whiteTextureId.HasValue) return;

            int chunkSize = guilds.LandClaim.ChunkSize;
            var spawnPos = clientApi.World.DefaultSpawnPosition.AsBlockPos;
            double zoomLevel = mapElem.ZoomLevel;

            foreach (var zone in protectedZones)
            {
                // Calculate zone center in world coordinates (accounting for spawn offset)
                double worldX = zone.x + spawnPos.X;
                double worldZ = zone.z + spawnPos.Z;

                // Check if zone is near viewport before rendering
                Vec2f zoneCenterViewPos = new Vec2f();
                mapElem.TranslateWorldPosToViewPos(new Vec3d(worldX, 0, worldZ), ref zoneCenterViewPos);

                // Calculate approximate zone size on screen for culling
                float zoneScreenRadius = (float)(zone.radius * zoomLevel);
                float viewportMargin = zoneScreenRadius + 100f;

                bool zoneNearViewport = zoneCenterViewPos.X >= -viewportMargin && zoneCenterViewPos.X <= mapBounds.InnerWidth + viewportMargin &&
                                       zoneCenterViewPos.Y >= -viewportMargin && zoneCenterViewPos.Y <= mapBounds.InnerHeight + viewportMargin;

                if (!zoneNearViewport) continue; // Skip zones not near viewport

                // Adaptive sampling based on zoom and radius
                int radiusInChunks = (int)Math.Ceiling((double)zone.radius / chunkSize);

                // OPTIMIZATION: Much more aggressive sampling reduction
                int baseSamplesPerQuadrant = Math.Max(10, Math.Min(20, radiusInChunks / 4));
                int samplesPerQuadrant = (int)(baseSamplesPerQuadrant * Math.Max(0.15, Math.Pow(zoomLevel, 1.5)));

                // Cap samples for very large zones and very small zoom levels
                if (zone.radius > 1500 || zoomLevel < 0.3)
                {
                    samplesPerQuadrant = Math.Max(8, samplesPerQuadrant / 2);
                }

                samplesPerQuadrant = Math.Max(8, Math.Min(samplesPerQuadrant, 40));

                var boundaryColor = ColorUtil.ToRGBAVec4f(ColorUtil.ColorFromRgba(128, 0, 255, 200)); // Purple color

                // Draw the boundary circle with viewport culling
                for (int i = 0; i < samplesPerQuadrant * 4; i++)
                {
                    double angle = (i / (double)(samplesPerQuadrant * 4)) * Math.PI * 2;
                    double x = worldX + Math.Cos(angle) * zone.radius;
                    double z = worldZ + Math.Sin(angle) * zone.radius;

                    // Convert world position to map position
                    Vec2f viewPos = new Vec2f();
                    mapElem.TranslateWorldPosToViewPos(new Vec3d(x, 0, z), ref viewPos);

                    // Viewport culling
                    if (viewPos.X < -10 || viewPos.X > mapBounds.InnerWidth + 10 ||
                        viewPos.Y < -10 || viewPos.Y > mapBounds.InnerHeight + 10)
                    {
                        continue;
                    }

                    // Draw a small marker - size scales with zoom
                    float dotSize = Math.Max(2, 3 * (float)zoomLevel);
                    RenderFilledRectangle(whiteTextureId.Value, mapBounds,
                        viewPos.X - dotSize / 2, viewPos.Y - dotSize / 2,
                        dotSize, dotSize, 55f, boundaryColor);
                }

                // Draw center marker only if visible
                if (zoneCenterViewPos.X >= -50 && zoneCenterViewPos.X <= mapBounds.InnerWidth + 50 &&
                    zoneCenterViewPos.Y >= -50 && zoneCenterViewPos.Y <= mapBounds.InnerHeight + 50)
                {
                    var centerColor = ColorUtil.ToRGBAVec4f(ColorUtil.ColorFromRgba(128, 0, 255, 255)); // Bright purple
                    float markerSize = Math.Max(4f, 6f * (float)zoomLevel);
                    RenderFilledRectangle(whiteTextureId.Value, mapBounds,
                        zoneCenterViewPos.X - markerSize / 2, zoneCenterViewPos.Y - markerSize / 2,
                        markerSize, markerSize, 56f, centerColor);
                }
            }
        }

        /// <summary>
        /// Renders node center markers and boundary triangles
        /// </summary>
        private void RenderNodeBoundaries(GuiElementMap mapElem, ElementBounds mapBounds)
        {
            if (nodes == null || nodes.Count == 0 || !whiteTextureId.HasValue) return;

            int chunkSize = guilds.LandClaim.ChunkSize;
            var spawnPos = clientApi.World.DefaultSpawnPosition.AsBlockPos;
            double zoomLevel = mapElem.ZoomLevel;

            foreach (var node in nodes)
            {
                // Calculate node center in world coordinates (accounting for spawn offset)
                double worldX = node.x + spawnPos.X;
                double worldZ = node.z + spawnPos.Z;

                // Check if node is near viewport before rendering
                Vec2f nodeCenterViewPos = new Vec2f();
                mapElem.TranslateWorldPosToViewPos(new Vec3d(worldX, 0, worldZ), ref nodeCenterViewPos);

                // Calculate approximate node size on screen for culling
                float nodeScreenRadius = (float)(node.radius * zoomLevel);
                float viewportMargin = nodeScreenRadius + 100f;

                bool nodeNearViewport = nodeCenterViewPos.X >= -viewportMargin && nodeCenterViewPos.X <= mapBounds.InnerWidth + viewportMargin &&
                                       nodeCenterViewPos.Y >= -viewportMargin && nodeCenterViewPos.Y <= mapBounds.InnerHeight + viewportMargin;

                if (!nodeNearViewport) continue; // Skip nodes not near viewport

                // Adaptive sampling based on zoom and radius
                int radiusInChunks = (int)Math.Ceiling((double)node.radius / chunkSize);

                // OPTIMIZATION: Sampling reduction for performance
                int baseSamplesPerSide = Math.Max(8, Math.Min(15, radiusInChunks / 3));
                int samplesPerSide = (int)(baseSamplesPerSide * Math.Max(0.15, Math.Pow(zoomLevel, 1.5)));

                // Cap samples for very large nodes and very small zoom levels
                if (node.radius > 1500 || zoomLevel < 0.3)
                {
                    samplesPerSide = Math.Max(6, samplesPerSide / 2);
                }

                samplesPerSide = Math.Max(6, Math.Min(samplesPerSide, 30));

                var boundaryColor = ColorUtil.ToRGBAVec4f(ColorUtil.ColorFromRgba(0, 255, 200, 200)); // Cyan color

                // Draw triangle boundary (equilateral triangle pointing up)
                // Calculate the three vertices of the triangle
                double angleOffset = -Math.PI / 2; // Point top vertex upward
                Vec2f[] triangleVertices = new Vec2f[3];

                for (int v = 0; v < 3; v++)
                {
                    double angle = angleOffset + (v * 2 * Math.PI / 3);
                    double x = worldX + Math.Cos(angle) * node.radius;
                    double z = worldZ + Math.Sin(angle) * node.radius;

                    Vec2f viewPos = new Vec2f();
                    mapElem.TranslateWorldPosToViewPos(new Vec3d(x, 0, z), ref viewPos);
                    triangleVertices[v] = viewPos;
                }

                // Draw the three sides of the triangle
                for (int side = 0; side < 3; side++)
                {
                    Vec2f start = triangleVertices[side];
                    Vec2f end = triangleVertices[(side + 1) % 3];

                    // Sample points along this side
                    for (int i = 0; i <= samplesPerSide; i++)
                    {
                        float t = i / (float)samplesPerSide;
                        float x = start.X + (end.X - start.X) * t;
                        float y = start.Y + (end.Y - start.Y) * t;

                        // Viewport culling
                        if (x < -10 || x > mapBounds.InnerWidth + 10 ||
                            y < -10 || y > mapBounds.InnerHeight + 10)
                        {
                            continue;
                        }

                        // Draw a small marker - size scales with zoom
                        float dotSize = Math.Max(2, 3 * (float)zoomLevel);
                        RenderFilledRectangle(whiteTextureId.Value, mapBounds, 
                            x - dotSize / 2, y - dotSize / 2, 
                            dotSize, dotSize, 55f, boundaryColor);
                    }
                }

                // Draw center marker only if visible (as a triangle)
                if (nodeCenterViewPos.X >= -50 && nodeCenterViewPos.X <= mapBounds.InnerWidth + 50 &&
                    nodeCenterViewPos.Y >= -50 && nodeCenterViewPos.Y <= mapBounds.InnerHeight + 50)
                {
                    var centerColor = ColorUtil.ToRGBAVec4f(ColorUtil.ColorFromRgba(0, 255, 200, 255)); // Bright cyan
                    float markerSize = Math.Max(6f, 9f * (float)zoomLevel);

                    // Draw a small triangle marker at center
                    // Top vertex
                    float topX = nodeCenterViewPos.X;
                    float topY = nodeCenterViewPos.Y - markerSize / 2;
                    // Bottom left vertex
                    float blX = nodeCenterViewPos.X - markerSize / 2;
                    float blY = nodeCenterViewPos.Y + markerSize / 2;
                    // Bottom right vertex
                    float brX = nodeCenterViewPos.X + markerSize / 2;
                    float brY = nodeCenterViewPos.Y + markerSize / 2;

                    // Draw three lines forming a triangle
                    int markerDots = 8;
                    for (int i = 0; i <= markerDots; i++)
                    {
                        float t = i / (float)markerDots;

                        // Side 1: top to bottom-left
                        float x1 = topX + (blX - topX) * t;
                        float y1 = topY + (blY - topY) * t;
                        RenderFilledRectangle(whiteTextureId.Value, mapBounds, x1 - 1, y1 - 1, 2, 2, 56f, centerColor);

                        // Side 2: bottom-left to bottom-right
                        float x2 = blX + (brX - blX) * t;
                        float y2 = blY + (brY - blY) * t;
                        RenderFilledRectangle(whiteTextureId.Value, mapBounds, x2 - 1, y2 - 1, 2, 2, 56f, centerColor);

                        // Side 3: bottom-right to top
                        float x3 = brX + (topX - brX) * t;
                        float y3 = brY + (topY - brY) * t;
                        RenderFilledRectangle(whiteTextureId.Value, mapBounds, x3 - 1, y3 - 1, 2, 2, 56f, centerColor);
                    }
                }
            }
        }

        /// <summary>
        /// Gets the chunk type and associated information for rendering and interaction
        /// </summary>
        private (ChunkType type, GuildSummary? guild, LandClaimDto? claim) GetChunkInfo(int chunkX, int chunkZ,
            Dictionary<Vec2i, (string guildName, GuildSummary guildSummary, LandClaimDto claim)> claimedChunks,
            List<(int, int)> pendingClaims)
        {
            var chunkKey = new Vec2i(chunkX, chunkZ);

            // Check if claimed
            if (claimedChunks.TryGetValue(chunkKey, out var claimInfo))
            {
                return (claimInfo.claim.IsGuildHome ? ChunkType.GuildHome : ChunkType.Claimed, claimInfo.guildSummary, claimInfo.claim);
            }

            // Check if pending
            bool isPending = pendingClaims.Any(p => p.Item1 == chunkX && p.Item2 == chunkZ);
            if (isPending)
            {
                // Check if this would be a guild home chunk using the dialog's method
                bool wouldBeGuildHome = activeGuildDialog?.IsPendingGuildHomeChunk(chunkX, chunkZ) ?? false;

                return (wouldBeGuildHome ? ChunkType.PendingGuildHome : ChunkType.Pending, null, null);
            }

            return (ChunkType.Unclaimed, null, null);
        }

        /// <summary>
        /// Checks if a chunk at the given coordinates belongs to the specified guild
        /// </summary>
        private bool IsChunkOwnedByGuild(int chunkX, int chunkZ, string guildName,
            Dictionary<Vec2i, (string guildName, GuildSummary guildSummary, LandClaimDto claim)> claimedChunks)
        {
            var chunkKey = new Vec2i(chunkX, chunkZ);
            if (claimedChunks.TryGetValue(chunkKey, out var claimInfo))
            {
                return claimInfo.guildName == guildName;
            }
            return false;
        }

        public override void Render(GuiElementMap mapElem, float dt)
        {
            if (!Active) return;

            if (clientApi?.World?.Player?.Entity?.Pos == null) return;

            var playerPos = clientApi.World.Player.Entity.Pos.AsBlockPos;
            int chunkSize = guilds.LandClaim.ChunkSize;

            // Get the current map viewport
            var mapBounds = mapElem.Bounds;
            double scale = mapElem.ZoomLevel;
            // Calculate visible chunk range with better logic - USE PROPER FLOOR DIVISION
            int centerChunkX = guilds.LandClaim.FloorDiv(playerPos.X, chunkSize);
            int centerChunkZ = guilds.LandClaim.FloorDiv(playerPos.Z, chunkSize);

            // Better view radius calculation
            int viewRadius = Math.Max(1, (int)(Math.Max(mapBounds.InnerWidth, mapBounds.InnerHeight) / (chunkSize * scale))) + 2;

            // Use rendering API for drawing
            var renderApi = clientApi.Render;

            // Load texture once and cache it
            if (whiteTextureId == null)
            {
                whiteTextureId = renderApi.GetOrLoadTexture(new AssetLocation("soaguildsandkingdoms:textures/gui/white.png"));
            }

            // Get guild summaries from the mod system
            var guildSummaries = modSystem?.GetClientGuildSummaries() ?? new List<GuildSummary>();

            // Create a lookup for claimed chunks - updated to handle guild homes
            var claimedChunks = new Dictionary<Vec2i, (string guildName, GuildSummary guildSummary, LandClaimDto claim)>();

            foreach (var guild in guildSummaries)
            {
                foreach (var claim in guild.Claims)
                {
                    var chunkKey = new Vec2i(claim.ChunkX, claim.ChunkZ);
                    claimedChunks[chunkKey] = (guild.Name, guild, claim);
                }
            }

            // Get pending claims if claiming mode is active
            var pendingClaims = activeGuildDialog?.GetPendingClaims() ?? new List<(int, int)>();
            var pendingUnclaims = activeGuildDialog?.GetPendingUnclaims() ?? new List<(int, int)>();
            var isClaimingMode = activeGuildDialog?.IsClaimingModeActive ?? false;
            var isUnclaimingMode = activeGuildDialog?.IsUnclaimingModeActive ?? false;

            // OPTIMIZATION: Calculate viewport bounds in world coordinates for culling
            Vec3d topLeft = new Vec3d();
            Vec3d bottomRight = new Vec3d();
            mapElem.TranslateViewPosToWorldPos(new Vec2f(0, 0), ref topLeft);
            mapElem.TranslateViewPosToWorldPos(new Vec2f((float)mapBounds.InnerWidth, (float)mapBounds.InnerHeight), ref bottomRight);

            int minVisibleChunkX = guilds.LandClaim.FloorDiv((int)topLeft.X, chunkSize) - 1;
            int maxVisibleChunkX = guilds.LandClaim.FloorDiv((int)bottomRight.X, chunkSize) + 1;
            int minVisibleChunkZ = guilds.LandClaim.FloorDiv((int)topLeft.Z, chunkSize) - 1;
            int maxVisibleChunkZ = guilds.LandClaim.FloorDiv((int)bottomRight.Z, chunkSize) + 1;

            // OPTIMIZATION: When zoomed out significantly, skip rendering very small chunks
            bool isZoomedOut = scale < 0.5;
            float minRenderSize = isZoomedOut ? 3f : 2f;

            // OPTIMIZATION: Build a set of chunks to render (with viewport culling)
            var chunksToRender = new HashSet<(int x, int z)>();

            // Only iterate through actually visible chunks
            for (int chunkX = minVisibleChunkX; chunkX <= maxVisibleChunkX; chunkX++)
            {
                for (int chunkZ = minVisibleChunkZ; chunkZ <= maxVisibleChunkZ; chunkZ++)
                {
                    chunksToRender.Add((chunkX, chunkZ));
                }
            }

            // Track performance metrics
            lastFrameChunkCount = chunksToRender.Count;

            // Draw chunk boundaries and guild claims
            foreach (var (chunkX, chunkZ) in chunksToRender)
            {
                // Convert chunk coordinates to world coordinates
                double worldX1 = chunkX * chunkSize;
                double worldZ1 = chunkZ * chunkSize;
                double worldX2 = (chunkX + 1) * chunkSize;
                double worldZ2 = (chunkZ + 1) * chunkSize;

                // Convert world coordinates to map coordinates
                Vec2f mapPos1 = new Vec2f();
                Vec2f mapPos3 = new Vec2f();

                mapElem.TranslateWorldPosToViewPos(new Vec3d(worldX1, 0, worldZ1), ref mapPos1);
                mapElem.TranslateWorldPosToViewPos(new Vec3d(worldX2, 0, worldZ2), ref mapPos3);

                // Calculate rectangle bounds
                float x1 = Math.Min(mapPos1.X, mapPos3.X);
                float y1 = Math.Min(mapPos1.Y, mapPos3.Y);
                float x2 = Math.Max(mapPos1.X, mapPos3.X);
                float y2 = Math.Max(mapPos1.Y, mapPos3.Y);

                float width = x2 - x1;
                float height = y2 - y1;

                // Skip very small rectangles (more aggressive when zoomed out)
                if (width < minRenderSize || height < minRenderSize) continue;

                try
                {
                    bool isHovered = hoveredChunk.HasValue && hoveredChunk.Value.chunkX == chunkX && hoveredChunk.Value.chunkZ == chunkZ;

                    // OPTIMIZATION: Cache expensive checks (only compute when needed)
                    bool isWithinTerritorialBounds = true;
                    bool isInProtectedZone = false;
                    bool isInNode = false;
                    bool tooCloseToOtherGuild = false;

                    // Only do expensive checks for unclaimed chunks in claiming mode
                    bool needsRestrictionsCheck = isClaimingMode && isHovered;

                    if (needsRestrictionsCheck || territorialRestrictionsEnabled || protectedZonesEnabled)
                    {
                        isWithinTerritorialBounds = IsChunkWithinTerritorialBounds(chunkX, chunkZ);
                        isInProtectedZone = IsChunkWithinProtectedZone(chunkX, chunkZ);
                        isInNode = IsChunkWithinNode(chunkX, chunkZ);
                    }

                    // Check distance to other guilds' claims (only for hovered chunks in claiming mode)
                    if (needsRestrictionsCheck)
                    {
                        var currentGuild = guildSummaries.FirstOrDefault(g => g.IsPlayerMember);
                        (tooCloseToOtherGuild, _, _) = IsChunkTooCloseToOtherGuildClaim(chunkX, chunkZ, currentGuild?.Name ?? "", claimedChunks);
                    }

                    var (chunkType, guild, claim) = GetChunkInfo(chunkX, chunkZ, claimedChunks, pendingClaims);

                    // Check if this chunk is pending unclaim
                    bool isPendingUnclaim = pendingUnclaims.Any(p => p.Item1 == chunkX && p.Item2 == chunkZ);

                    // Only render if we have a valid texture
                    if (whiteTextureId.HasValue && whiteTextureId.Value > 0)
                    {
                        switch (chunkType)
                        {
                            case ChunkType.Claimed:
                                // Check neighbors to determine which borders to draw
                                bool drawTop = !IsChunkOwnedByGuild(chunkX, chunkZ - 1, guild!.Name, claimedChunks);
                                bool drawRight = !IsChunkOwnedByGuild(chunkX + 1, chunkZ, guild!.Name, claimedChunks);
                                bool drawBottom = !IsChunkOwnedByGuild(chunkX, chunkZ + 1, guild!.Name, claimedChunks);
                                bool drawLeft = !IsChunkOwnedByGuild(chunkX - 1, chunkZ, guild!.Name, claimedChunks);

                                RenderClaimedChunk(mapBounds, x1, y1, x2, y2, guild!, false,
                                    drawTop, drawRight, drawBottom, drawLeft);

                                // Render pending unclaim overlay on top if applicable
                                if (isPendingUnclaim)
                                {
                                    RenderPendingUnclaimChunk(mapBounds, x1, y1, x2, y2);
                                }
                                break;

                            case ChunkType.GuildHome:
                                // Check neighbors to determine which borders to draw
                                bool drawTopGH = !IsChunkOwnedByGuild(chunkX, chunkZ - 1, guild!.Name, claimedChunks);
                                bool drawRightGH = !IsChunkOwnedByGuild(chunkX + 1, chunkZ, guild!.Name, claimedChunks);
                                bool drawBottomGH = !IsChunkOwnedByGuild(chunkX, chunkZ + 1, guild!.Name, claimedChunks);
                                bool drawLeftGH = !IsChunkOwnedByGuild(chunkX - 1, chunkZ, guild!.Name, claimedChunks);

                                RenderClaimedChunk(mapBounds, x1, y1, x2, y2, guild!, true,
                                    drawTopGH, drawRightGH, drawBottomGH, drawLeftGH);

                                // Render pending unclaim overlay on top if applicable
                                if (isPendingUnclaim)
                                {
                                    RenderPendingUnclaimChunk(mapBounds, x1, y1, x2, y2);
                                }
                                break;

                            case ChunkType.Pending:
                                RenderPendingChunk(mapBounds, x1, y1, x2, y2, false);
                                break;

                            case ChunkType.PendingGuildHome:
                                RenderPendingChunk(mapBounds, x1, y1, x2, y2, true);
                                break;

                            case ChunkType.Unclaimed:
                                // Only render hover highlighting in claiming mode
                                // The boundary circles are rendered separately at the end
                                if (isClaimingMode && isHovered)
                                {
                                    // Determine if the chunk can be claimed
                                    bool canClaim = isWithinTerritorialBounds && !isInProtectedZone && !tooCloseToOtherGuild;
                                    RenderHoverHighlight(mapBounds, x1, y1, x2, y2, canClaim);
                                }
                                break;
                        }

                                // Add hover highlight for unclaiming mode on claimed chunks
                                if (isUnclaimingMode && isHovered && (chunkType == ChunkType.Claimed || chunkType == ChunkType.GuildHome))
                                {
                                    // Show red highlight when hovering over claimable chunks in unclaiming mode
                                    var unclaimHoverColor = ColorUtil.ToRGBAVec4f(ColorUtil.ColorFromRgba(100, 150, 255, 150));
                                    RenderFilledRectangle(whiteTextureId.Value, mapBounds, x1, y1, x2 - x1, y2 - y1, 54f, unclaimHoverColor);
                                }

                                // Render node overlay if chunk is within a node (render on top of everything except hover)
                                if (isInNode && chunkType == ChunkType.Unclaimed)
                                {
                                    RenderNode(mapBounds, x1, y1, x2, y2);
                                }
                            }
                        }
                            catch
                            {
                                // Fallback - visual rendering failed
                            }

                // Store chunk data for hover info (updated below)
                // OPTIMIZATION: Only cache data for chunks near the player when zoomed out
                if (!isZoomedOut || (Math.Abs(chunkX - centerChunkX) < 10 && Math.Abs(chunkZ - centerChunkZ) < 10))
                {
                    var chunkDataKey = new Vec2i(chunkX, chunkZ);
                    if (!chunkDataCache.ContainsKey(chunkDataKey))
                    {
                        var chunk = clientApi.World.BlockAccessor.GetChunk(chunkX, 0, chunkZ);
                        chunkDataCache[chunkDataKey] = new ChunkData
                        {
                            ChunkX = chunkX,
                            ChunkZ = chunkZ,
                            IsLoaded = chunk != null,
                            LoadedBlocks = chunk?.BlockEntities?.Count ?? 0
                        };
                    }
                }
            }

            // Render territorial boundary circle and center marker (after all chunks)
            RenderTerritorialBoundary(mapElem, mapBounds);

            // Render protected zone boundaries and center markers (after all chunks)
            RenderProtectedZoneBoundaries(mapElem, mapBounds);

            // Render node boundaries and center markers (after all chunks)
            RenderNodeBoundaries(mapElem, mapBounds);
        }

        public override void OnMouseMoveClient(MouseEvent args, GuiElementMap mapElem, StringBuilder hoverText)
        {
            if (clientApi?.World?.Player?.Entity?.Pos == null) return;

            int chunkSize = guilds.LandClaim.ChunkSize;

            // OPTIMIZATION: Skip expensive hover calculations when zoomed way out with many chunks
            double zoomLevel = mapElem.ZoomLevel;
            bool isVeryZoomedOut = zoomLevel < 0.25 && lastFrameChunkCount > 500;

            // Get guild summaries and check each claimed chunk
            var guildSummaries = modSystem?.GetClientGuildSummaries() ?? new List<GuildSummary>();

            // Calculate mouse position relative to map bounds
            double mouseX = args.X - mapElem.Bounds.renderX;
            double mouseY = args.Y - mapElem.Bounds.renderY;

            // Reset hovered chunk
            hoveredChunk = null;

            // OPTIMIZATION: When very zoomed out, show simplified info and return early
            if (isVeryZoomedOut)
            {
                hoverText.AppendLine($"Zoom in for detailed information");
                hoverText.AppendLine($"Visible chunks: {lastFrameChunkCount}");
                return;
            }

            foreach (var guild in guildSummaries)
            {
                foreach (var claim in guild.Claims)
                {
                    // Convert chunk coordinates to world position (center of chunk)
                    Vec2f viewPos = new Vec2f();
                    mapElem.TranslateWorldPosToViewPos(new Vec3d(claim.ChunkX * chunkSize + chunkSize / 2, 0, claim.ChunkZ * chunkSize + chunkSize / 2), ref viewPos);

                    // Check if mouse is within chunk bounds on the map
                    double chunkViewSize = chunkSize * mapElem.ZoomLevel;
                    if (Math.Abs(viewPos.X - mouseX) < chunkViewSize / 2 && Math.Abs(viewPos.Y - mouseY) < chunkViewSize / 2)
                    {
                        // Set hovered chunk for claiming mode highlighting
                        hoveredChunk = (claim.ChunkX, claim.ChunkZ);

                        // Show chunk coordinates
                        hoverText.AppendLine($"Chunk: ({claim.ChunkX}, {claim.ChunkZ})");
                        hoverText.AppendLine($"Claimed by: {guild.Name}");
                        if (!string.IsNullOrEmpty(guild.Description))
                        {
                            hoverText.AppendLine($"{guild.Description}");
                        }

                        var claimerName = clientApi.World.AllOnlinePlayers
                            .FirstOrDefault(p => p.PlayerUID == claim.ClaimedByUid)?.PlayerName ?? "Unknown";

                        hoverText.AppendLine($"Claimed by: {claimerName}");
                        hoverText.AppendLine($"Claim Date: {claim.Timestamp.ToString("yyyy-MM-dd")}");

                        // Check if this chunk is in pending unclaims
                        var pendingUnclaims = activeGuildDialog?.GetPendingUnclaims() ?? new List<(int, int)>();
                        bool isPendingUnclaim = pendingUnclaims.Any(p => p.Item1 == claim.ChunkX && p.Item2 == claim.ChunkZ);

                        if (isPendingUnclaim)
                        {
                            hoverText.AppendLine("Status: PENDING UNCLAIM");
                        }

                        // Check if this is part of a guild home
                        if (claim.IsGuildHome)
                        {
                            hoverText.AppendLine("Type: Guild Home Territory");
                            if (claim.HomeCenterX.HasValue && claim.HomeCenterZ.HasValue)
                            {
                                hoverText.AppendLine($"Home Center: ({claim.HomeCenterX}, {claim.HomeCenterZ})");

                                // Show which part of the 2x2 home this is
                                int offsetX = claim.ChunkX - claim.HomeCenterX.Value;
                                int offsetZ = claim.ChunkZ - claim.HomeCenterZ.Value;
                                string position = GetGuildHomeQuadrantName(offsetX, offsetZ);
                                hoverText.AppendLine($"Home Quadrant: {position}");
                            }
                        }

                        // Show unclaiming information if in unclaiming mode
                        if (activeGuildDialog?.IsUnclaimingModeActive == true)
                        {
                            // Check if this chunk belongs to the player's guild
                            var currentGuild = guildSummaries.FirstOrDefault(g => g.IsPlayerMember);
                            if (currentGuild != null && currentGuild.Name == guild.Name)
                            {
                                // Check if chunk can be unclaimed
                                if (claim.IsGuildHome)
                                {
                                    // Count remaining non-guild-home claims
                                    int nonGuildHomeClaims = currentGuild.Claims.Count(c => !c.IsGuildHome);

                                    // Check pending unclaims
                                    int pendingNonGuildHomeUnclaims = 0;
                                    foreach (var c in currentGuild.Claims)
                                    {
                                        if (!c.IsGuildHome && pendingUnclaims.Any(p => p.Item1 == c.ChunkX && p.Item2 == c.ChunkZ))
                                        {
                                            pendingNonGuildHomeUnclaims++;
                                        }
                                    }

                                    int remainingNonGuildHomeClaims = nonGuildHomeClaims - pendingNonGuildHomeUnclaims;

                                    if (remainingNonGuildHomeClaims > 0)
                                    {
                                        hoverText.AppendLine($"Cannot unclaim: Guild Home (unclaim other {remainingNonGuildHomeClaims} claim(s) first)");
                                    }
                                    else if (isPendingUnclaim)
                                    {
                                        hoverText.AppendLine("Already marked for unclaim");
                                    }
                                    else
                                    {
                                        hoverText.AppendLine("Left-click to unclaim all guild home chunks");
                                    }
                                }
                                else if (claim.IsOutpost)
                                {
                                    // Count remaining non-outpost, non-guild-home claims
                                    int nonOutpostNonGuildHomeClaims = currentGuild.Claims.Count(c => !c.IsOutpost && !c.IsGuildHome);

                                    // Check pending unclaims
                                    int pendingNonOutpostNonGuildHomeUnclaims = 0;
                                    foreach (var c in currentGuild.Claims)
                                    {
                                        if (!c.IsOutpost && !c.IsGuildHome && pendingUnclaims.Any(p => p.Item1 == c.ChunkX && p.Item2 == c.ChunkZ))
                                        {
                                            pendingNonOutpostNonGuildHomeUnclaims++;
                                        }
                                    }

                                    int remainingNonOutpostNonGuildHomeClaims = nonOutpostNonGuildHomeClaims - pendingNonOutpostNonGuildHomeUnclaims;

                                    if (remainingNonOutpostNonGuildHomeClaims > 0)
                                    {
                                        hoverText.AppendLine($"Cannot unclaim: Outpost (unclaim other {remainingNonOutpostNonGuildHomeClaims} claim(s) first)");
                                    }
                                    else if (isPendingUnclaim)
                                    {
                                        hoverText.AppendLine("Already marked for unclaim");
                                    }
                                    else
                                    {
                                        // Show how many chunks will be unclaimed
                                        int outpostChunkCount = currentGuild.Claims.Count(c => c.IsOutpost && c.OutpostName == claim.OutpostName);
                                        hoverText.AppendLine($"Left-click to unclaim all outpost chunks ({outpostChunkCount} chunks)");
                                    }
                                }
                                else if (isPendingUnclaim)
                                {
                                    hoverText.AppendLine("Already marked for unclaim");
                                }
                                else
                                {
                                    hoverText.AppendLine("Left-click to mark for unclaim");
                                }
                            }
                        }

                        return; // Found the claim, show info and exit
                    }
                }
            }

            // If we get here, no claimed chunk was found under the mouse
            // Check if mouse is over any chunk area for unclaimed/pending status
            var playerPos = clientApi.World.Player.Entity.Pos.AsBlockPos;
            int centerChunkX = guilds.LandClaim.FloorDiv(playerPos.X, chunkSize);
            int centerChunkZ = guilds.LandClaim.FloorDiv(playerPos.Z, chunkSize);

            // Better view radius calculation
            var mapBounds = mapElem.Bounds;
            double scale = mapElem.ZoomLevel;
            int viewRadius = Math.Max(1, (int)(Math.Max(mapBounds.InnerWidth, mapBounds.InnerHeight) / (chunkSize * scale))) + 2;

            // Check visible chunks for unclaimed areas
            for (int dx = -viewRadius; dx <= viewRadius; dx++)
            {
                for (int dz = -viewRadius; dz <= viewRadius; dz++)
                {
                    int chunkX = centerChunkX + dx;
                    int chunkZ = centerChunkZ + dz;

                    // Convert chunk coordinates to world position (center of chunk)
                    Vec2f viewPos = new Vec2f();
                    mapElem.TranslateWorldPosToViewPos(new Vec3d(chunkX * chunkSize + chunkSize / 2, 0, chunkZ * chunkSize + chunkSize / 2), ref viewPos);

                    // Check if mouse is within chunk bounds on the map
                    double chunkViewSize = chunkSize * mapElem.ZoomLevel;
                    if (Math.Abs(viewPos.X - mouseX) < chunkViewSize / 2 && Math.Abs(viewPos.Y - mouseY) < chunkViewSize / 2)
                    {
                        // Set hovered chunk for claiming mode highlighting
                        hoveredChunk = (chunkX, chunkZ);

                        // Check if this chunk is in pending claims
                        var pendingClaims = activeGuildDialog?.GetPendingClaims() ?? new List<(int, int)>();
                        var pendingUnclaims = activeGuildDialog?.GetPendingUnclaims() ?? new List<(int, int)>();
                        bool isPending = pendingClaims.Any(p => p.Item1 == chunkX && p.Item2 == chunkZ);
                        bool isPendingUnclaim = pendingUnclaims.Any(p => p.Item1 == chunkX && p.Item2 == chunkZ);

                        // Check territorial restrictions
                        bool isWithinTerritorialBounds = IsChunkWithinTerritorialBounds(chunkX, chunkZ);

                        if (isPending)
                        {
                            hoverText.AppendLine($"Chunk: ({chunkX}, {chunkZ})");

                            // Check if this would be a guild home chunk
                            bool wouldBeGuildHome = activeGuildDialog?.IsPendingGuildHomeChunk(chunkX, chunkZ) ?? false;

                            if (wouldBeGuildHome)
                            {
                                hoverText.AppendLine("Status: Pending Guild Home");
                                hoverText.AppendLine("This will become your guild's home base (2x2 area)");
                            }
                            else
                            {
                                hoverText.AppendLine("Status: Pending Claim");
                            }
                        }
                        else
                        {
                            hoverText.AppendLine($"Chunk: ({chunkX}, {chunkZ})");
                            hoverText.AppendLine("Status: Unclaimed");

                            // Check if in a protected zone
                            bool isInProtectedZone = IsChunkWithinProtectedZone(chunkX, chunkZ);
                            if (protectedZonesEnabled && isInProtectedZone)
                            {
                                var zone = GetProtectedZoneAt(chunkX * 32 + 16, chunkZ * 32 + 16, clientApi.World.DefaultSpawnPosition.AsBlockPos);
                                if (zone.HasValue)
                                {
                                    hoverText.AppendLine($"Protected Zone: {zone.Value.name}");
                                    hoverText.AppendLine("Cannot claim or break blocks here");

                                    // Calculate distance from zone center
                                    int blockX = chunkX * 32 + 16;
                                    int blockZ = chunkZ * 32 + 16;
                                    var mapSize = clientApi.World.BlockAccessor.MapSize;
                                    double deltaX = blockX - zone.Value.x - mapSize.X / 2;
                                    double deltaZ = blockZ - zone.Value.z - mapSize.Z / 2;
                                    double distance = Math.Sqrt(deltaX * deltaX + deltaZ * deltaZ);

                                    hoverText.AppendLine($"Distance from center: {distance:F0} / {zone.Value.radius} blocks");
                                }
                            }

                            // Check if in a node
                            bool isInNode = IsChunkWithinNode(chunkX, chunkZ);
                            if (isInNode)
                            {
                                var node = GetNodeAt(chunkX * 32 + 16, chunkZ * 32 + 16, clientApi.World.DefaultSpawnPosition.AsBlockPos);
                                if (node.HasValue)
                                {
                                    hoverText.AppendLine($"Node: {node.Value.name}");

                                    // Calculate distance from node center
                                    int blockX = chunkX * 32 + 16;
                                    int blockZ = chunkZ * 32 + 16;
                                    var mapSize = clientApi.World.BlockAccessor.MapSize;
                                    double deltaX = blockX - node.Value.x - mapSize.X / 2;
                                    double deltaZ = blockZ - node.Value.z - mapSize.Z / 2;
                                    double distance = Math.Sqrt(deltaX * deltaX + deltaZ * deltaZ);

                                    hoverText.AppendLine($"Distance from center: {distance:F0} / {node.Value.radius} blocks");
                                }
                            }

                            // Check proximity to other guilds' claims
                            var currentGuild = guildSummaries.FirstOrDefault(g => g.IsPlayerMember);
                            var claimedChunks = new Dictionary<Vec2i, (string guildName, GuildSummary guildSummary, LandClaimDto claim)>();
                            foreach (var guild in guildSummaries)
                            {
                                foreach (var claim in guild.Claims)
                                {
                                    var chunkKey = new Vec2i(claim.ChunkX, claim.ChunkZ);
                                    claimedChunks[chunkKey] = (guild.Name, guild, claim);
                                }
                            }

                            var (tooClose, nearestGuild, distanceToNearest) = IsChunkTooCloseToOtherGuildClaim(
                                chunkX, chunkZ, currentGuild?.Name ?? "", claimedChunks);

                            if (tooClose && !string.IsNullOrEmpty(nearestGuild))
                            {
                                hoverText.AppendLine($"TOO CLOSE to {nearestGuild}'s territory!");
                                hoverText.AppendLine($"Distance: {distanceToNearest:F0} blocks (min: 300)");
                                hoverText.AppendLine("Cannot claim within 300 blocks of another guild");
                            }
                            else if (!string.IsNullOrEmpty(nearestGuild) && distanceToNearest < 600)
                            {
                                // Show warning when getting close (within 600 blocks)
                                hoverText.AppendLine($"Near {nearestGuild}'s territory: {distanceToNearest:F0} blocks");
                            }

                            // Show territorial restriction information
                            if (territorialRestrictionsEnabled)
                            {
                                if (isWithinTerritorialBounds)
                                {
                                    hoverText.AppendLine("Territory: Allowed claiming area");
                                }
                                else
                                {
                                    hoverText.AppendLine("Territory: RESTRICTED - Outside claiming zone");
                                    if (territorialCenter.HasValue)
                                    {
                                        // Calculate distance from center (accounting for map offset)
                                        int blockX = chunkX * 32 + 16; // Center of chunk
                                        int blockZ = chunkZ * 32 + 16;
                                        var mapSize = clientApi.World.BlockAccessor.MapSize;
                                        double deltaX = blockX - territorialCenter.Value.x - mapSize.X / 2;
                                        double deltaZ = blockZ - territorialCenter.Value.z - mapSize.Z / 2;
                                        double distance = Math.Sqrt(deltaX * deltaX + deltaZ * deltaZ);

                                        hoverText.AppendLine($"Distance from center ({territorialCenter.Value.x}, {territorialCenter.Value.z}): {distance:F0} blocks");
                                        hoverText.AppendLine($"Max allowed distance: {territorialRadius} blocks");
                                    }
                                }
                            }

                            // If in claiming mode, show that this chunk can be claimed (or not)
                            if (activeGuildDialog?.IsClaimingModeActive == true)
                            {
                                if (isInProtectedZone && protectedZonesEnabled)
                                {
                                    hoverText.AppendLine("Cannot claim - Protected zone");
                                }
                                else if (tooClose)
                                {
                                    hoverText.AppendLine("Cannot claim - Too close to another guild");
                                }
                                else if (isWithinTerritorialBounds)
                                {
                                    // Check if this would be a guild home
                                    var currentGuildForHome = guildSummaries.FirstOrDefault(g => g.IsPlayerMember);
                                    bool wouldBeGuildHome = currentGuildForHome != null && !currentGuildForHome.Claims.Any(c => c.IsGuildHome) && pendingClaims.Count == 0;

                                    if (wouldBeGuildHome)
                                    {
                                        hoverText.AppendLine("Left-click to establish Guild Home (2x2)");
                                        hoverText.AppendLine("Will claim this chunk + 3 adjacent chunks");
                                    }
                                    else
                                    {
                                        hoverText.AppendLine("Left-click to claim");
                                    }
                                }
                                else if (territorialRestrictionsEnabled)
                                {
                                    hoverText.AppendLine("Cannot claim - Outside allowed area");
                                }
                            }
                        }
                        return;
                    }
                }
            }
        }

        /// <summary>
        /// Gets a readable name for a guild home quadrant based on offset from center
        /// </summary>
        private string GetGuildHomeQuadrantName(int offsetX, int offsetZ)
        {
            return (offsetX, offsetZ) switch
            {
                (0, 0) => "Southwest",
                (1, 0) => "Southeast",
                (0, 1) => "Northwest",
                (1, 1) => "Northeast",
                _ => "Unknown"
            };
        }

        public override void OnMouseUpClient(MouseEvent args, GuiElementMap mapElem)
        {
            int chunkSize = guilds.LandClaim.ChunkSize;

            // Calculate mouse position relative to map bounds
            double mouseX = args.X - mapElem.Bounds.renderX;
            double mouseY = args.Y - mapElem.Bounds.renderY;

            // Get guild summaries
            var guildSummaries = modSystem?.GetClientGuildSummaries() ?? new List<GuildSummary>();
            var currentGuild = guildSummaries.FirstOrDefault(g => g.IsPlayerMember);

            // Handle left-click for unclaiming when in unclaiming mode
            if (args.Button == EnumMouseButton.Left && activeGuildDialog?.IsUnclaimingModeActive == true)
            {
                // Check if clicking on one of the player's guild's claims
                if (currentGuild != null)
                {
                    foreach (var claim in currentGuild.Claims)
                    {
                        // Convert chunk coordinates to world position (center of chunk)
                        Vec2f viewPos = new Vec2f();
                        mapElem.TranslateWorldPosToViewPos(new Vec3d(claim.ChunkX * chunkSize + chunkSize / 2, 0, claim.ChunkZ * chunkSize + chunkSize / 2), ref viewPos);

                        // Check if mouse is within chunk bounds on the map
                        double chunkViewSize = chunkSize * mapElem.ZoomLevel;
                        if (Math.Abs(viewPos.X - mouseX) < chunkViewSize / 2 && Math.Abs(viewPos.Y - mouseY) < chunkViewSize / 2)
                        {
                            var pendingUnclaims = activeGuildDialog?.GetPendingUnclaims() ?? new List<(int, int)>();

                            // Check if this is a guild home chunk
                            if (claim.IsGuildHome && claim.HomeCenterX.HasValue && claim.HomeCenterZ.HasValue)
                            {
                                // Count how many non-guild-home claims exist
                                int nonGuildHomeClaims = currentGuild.Claims.Count(c => !c.IsGuildHome);

                                // Also check pending unclaims to account for claims being marked for removal
                                int pendingNonGuildHomeUnclaims = 0;
                                foreach (var c in currentGuild.Claims)
                                {
                                    if (!c.IsGuildHome && pendingUnclaims.Any(p => p.Item1 == c.ChunkX && p.Item2 == c.ChunkZ))
                                    {
                                        pendingNonGuildHomeUnclaims++;
                                    }
                                }

                                int remainingNonGuildHomeClaims = nonGuildHomeClaims - pendingNonGuildHomeUnclaims;

                                // Only allow unclaiming guild home if there are no other claims remaining
                                if (remainingNonGuildHomeClaims > 0)
                                {
                                    clientApi?.ShowChatMessage($"[Guild] Cannot unclaim guild home - You must unclaim all other territory first ({remainingNonGuildHomeClaims} claim(s) remaining).");
                                    return;
                                }

                                // Unclaim all 4 chunks of the guild home (2x2 area)
                                int centerX = claim.HomeCenterX.Value;
                                int centerZ = claim.HomeCenterZ.Value;

                                // Unclaim all 4 quadrants of the guild home
                                activeGuildDialog.OnMapChunkUnclaimed(centerX, centerZ);         // Southwest
                                activeGuildDialog.OnMapChunkUnclaimed(centerX + 1, centerZ);     // Southeast
                                activeGuildDialog.OnMapChunkUnclaimed(centerX, centerZ + 1);     // Northwest
                                activeGuildDialog.OnMapChunkUnclaimed(centerX + 1, centerZ + 1); // Northeast

                                clientApi?.ShowChatMessage("[Guild] Marking all guild home chunks for unclaim...");
                            }
                            // Check if this is an outpost chunk
                            else if (claim.IsOutpost && !string.IsNullOrEmpty(claim.OutpostName))
                            {
                                // Count non-outpost, non-guild-home claims
                                int nonOutpostNonGuildHomeClaims = currentGuild.Claims.Count(c => !c.IsOutpost && !c.IsGuildHome);

                                // Check pending unclaims for non-outpost, non-guild-home claims
                                int pendingNonOutpostNonGuildHomeUnclaims = 0;
                                foreach (var c in currentGuild.Claims)
                                {
                                    if (!c.IsOutpost && !c.IsGuildHome && pendingUnclaims.Any(p => p.Item1 == c.ChunkX && p.Item2 == c.ChunkZ))
                                    {
                                        pendingNonOutpostNonGuildHomeUnclaims++;
                                    }
                                }

                                int remainingNonOutpostNonGuildHomeClaims = nonOutpostNonGuildHomeClaims - pendingNonOutpostNonGuildHomeUnclaims;

                                // Only allow unclaiming outpost if there are no other non-outpost, non-guild-home claims remaining
                                if (remainingNonOutpostNonGuildHomeClaims > 0)
                                {
                                    clientApi?.ShowChatMessage($"[Guild] Cannot unclaim outpost - You must unclaim all regular territory first ({remainingNonOutpostNonGuildHomeClaims} claim(s) remaining).");
                                    return;
                                }

                                // Get all chunks belonging to this outpost
                                var outpostChunks = currentGuild.Claims
                                    .Where(c => c.IsOutpost && c.OutpostName == claim.OutpostName)
                                    .ToList();

                                // Unclaim all chunks of this outpost
                                foreach (var outpostChunk in outpostChunks)
                                {
                                    activeGuildDialog.OnMapChunkUnclaimed(outpostChunk.ChunkX, outpostChunk.ChunkZ);
                                }

                                clientApi?.ShowChatMessage($"[Guild] Marking all {claim.OutpostName} outpost chunks for unclaim ({outpostChunks.Count} chunks)...");
                            }
                            else
                            {
                                // Regular claim, unclaim just this chunk
                                activeGuildDialog.OnMapChunkUnclaimed(claim.ChunkX, claim.ChunkZ);
                            }
                            return;
                        }
                    }
                }

                // If we get here, clicked on a non-guild chunk while in unclaiming mode
                clientApi?.ShowChatMessage("[Guild] You can only unclaim chunks owned by your guild.");
                return;
            }

            // Handle left-click for claiming when in claiming mode
            if (args.Button == EnumMouseButton.Left && activeGuildDialog?.IsClaimingModeActive == true)
            {
                // First check if clicking on an existing claim (should not claim)
                foreach (var guild in guildSummaries)
                {
                    foreach (var claim in guild.Claims)
                    {
                        // Convert chunk coordinates to world position (center of chunk)
                        Vec2f viewPos = new Vec2f();
                        mapElem.TranslateWorldPosToViewPos(new Vec3d(claim.ChunkX * chunkSize + chunkSize / 2, 0, claim.ChunkZ * chunkSize + chunkSize / 2), ref viewPos);

                        // Check if mouse is within chunk bounds on the map
                        double chunkViewSize = chunkSize * mapElem.ZoomLevel;
                        if (Math.Abs(viewPos.X - mouseX) < chunkViewSize / 2 && Math.Abs(viewPos.Y - mouseY) < chunkViewSize / 2)
                        {
                            // Clicking on an already claimed chunk, do nothing
                            return;
                        }
                    }
                }

                // Check for unclaimed chunks in the visible area
                var playerPos = clientApi.World.Player.Entity.Pos.AsBlockPos;
                int centerChunkX = guilds.LandClaim.FloorDiv(playerPos.X, chunkSize);
                int centerChunkZ = guilds.LandClaim.FloorDiv(playerPos.Z, chunkSize);

                // Better view radius calculation
                var mapBounds = mapElem.Bounds;
                double scale = mapElem.ZoomLevel;
                int viewRadius = Math.Max(1, (int)(Math.Max(mapBounds.InnerWidth, mapBounds.InnerHeight) / (chunkSize * scale))) + 2;

                // Check visible chunks for unclaimed areas
                for (int dx = -viewRadius; dx <= viewRadius; dx++)
                {
                    for (int dz = -viewRadius; dz <= viewRadius; dz++)
                    {
                        int chunkX = centerChunkX + dx;
                        int chunkZ = centerChunkZ + dz;

                        // Convert chunk coordinates to world position (center of chunk)
                        Vec2f viewPos = new Vec2f();
                        mapElem.TranslateWorldPosToViewPos(new Vec3d(chunkX * chunkSize + chunkSize / 2, 0, chunkZ * chunkSize + chunkSize / 2), ref viewPos);

                        // Check if mouse is within chunk bounds on the map
                        double chunkViewSize = chunkSize * mapElem.ZoomLevel;
                        if (Math.Abs(viewPos.X - mouseX) < chunkViewSize / 2 && Math.Abs(viewPos.Y - mouseY) < chunkViewSize / 2)
                        {
                            // Check if chunk is within territorial bounds before allowing claim
                            if (territorialRestrictionsEnabled && !IsChunkWithinTerritorialBounds(chunkX, chunkZ))
                            {
                                // Show client-side warning for restricted areas
                                clientApi.ShowChatMessage("[Guild] Cannot claim here - This area is outside the allowed claiming zone.");
                                return;
                            }

                            // Check if chunk is too close to another guild's claim
                            var claimedChunksForCheck = new Dictionary<Vec2i, (string guildName, GuildSummary guildSummary, LandClaimDto claim)>();
                            foreach (var guild in guildSummaries)
                            {
                                foreach (var claim in guild.Claims)
                                {
                                    var chunkKey = new Vec2i(claim.ChunkX, claim.ChunkZ);
                                    claimedChunksForCheck[chunkKey] = (guild.Name, guild, claim);
                                }
                            }

                            var (tooClose, nearestGuild, distance) = IsChunkTooCloseToOtherGuildClaim(
                                chunkX, chunkZ, currentGuild?.Name ?? "", claimedChunksForCheck);

                            if (tooClose)
                            {
                                clientApi.ShowChatMessage($"[Guild] Cannot claim here - Too close to {nearestGuild}'s territory ({distance:F0} blocks, minimum 300 blocks required).");
                                return;
                            }

                            // Found the clicked chunk, send claim request
                            activeGuildDialog.OnMapChunkClaimed(chunkX, chunkZ);
                            return;
                        }
                    }
                }
            }

            base.OnMouseUpClient(args, mapElem);
        }

        public override void OnViewChangedClient(List<FastVec2i> nowVisible, List<FastVec2i> nowHidden)
        {
            // Clear cache for hidden chunks to prevent memory bloat
            foreach (var hidden in nowHidden)
            {
                var key = new Vec2i(hidden.X, hidden.Y);
                chunkDataCache.Remove(key);
            }
        }

        public override void Dispose()
        {
            chunkDataCache?.Clear();
            activeGuildDialog = null;
            base.Dispose();
        }
    }

    // Enum to define different chunk types for rendering and interaction
    public enum ChunkType
    {
        Unclaimed,
        Claimed,
        GuildHome,
        Pending,
        PendingGuildHome
    }

    // Helper class for chunk data
    public class ChunkData
    {
        public int ChunkX { get; set; }
        public int ChunkZ { get; set; }
        public bool IsLoaded { get; set; }
        public int LoadedBlocks { get; set; }
    }
}
