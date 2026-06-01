using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using SRGuildsAndKingdoms.src.guilds;
using SRGuildsAndKingdoms.src.network;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace SRGuildsAndKingdoms.src.gui
{
	// Token: 0x02000082 RID: 130
	[NullableContext(1)]
	[Nullable(0)]
	public class PlotMapLayer : RGBMapLayer
	{
		// Token: 0x17000189 RID: 393
		// (get) Token: 0x060005AA RID: 1450 RVA: 0x0002665F File Offset: 0x0002485F
		public override string Title
		{
			get
			{
				return "guildclaims";
			}
		}

		// Token: 0x1700018A RID: 394
		// (get) Token: 0x060005AB RID: 1451 RVA: 0x00026666 File Offset: 0x00024866
		public override string LayerGroupCode
		{
			get
			{
				return "guildclaims";
			}
		}

		// Token: 0x1700018B RID: 395
		// (get) Token: 0x060005AC RID: 1452 RVA: 0x0002666D File Offset: 0x0002486D
		public override EnumMapAppSide DataSide
		{
			get
			{
				return 0;
			}
		}

		// Token: 0x1700018C RID: 396
		// (get) Token: 0x060005AD RID: 1453 RVA: 0x00026670 File Offset: 0x00024870
		public override EnumMinMagFilter MinFilter
		{
			get
			{
				return 0;
			}
		}

		// Token: 0x1700018D RID: 397
		// (get) Token: 0x060005AE RID: 1454 RVA: 0x00026673 File Offset: 0x00024873
		public override EnumMinMagFilter MagFilter
		{
			get
			{
				return 0;
			}
		}

		// Token: 0x1700018E RID: 398
		// (get) Token: 0x060005AF RID: 1455 RVA: 0x00026676 File Offset: 0x00024876
		public override MapLegendItem[] LegendItems
		{
			get
			{
				return null;
			}
		}

		// Token: 0x060005B0 RID: 1456 RVA: 0x0002667C File Offset: 0x0002487C
		public PlotMapLayer(ICoreAPI api, IWorldMapManager mapSink) : base(api, mapSink)
		{
			this.clientApi = (api as ICoreClientAPI);
			this.chunkDataCache = new Dictionary<Vec2i, ChunkData>();
			this.modSystem = api.ModLoader.GetModSystem<SRGuildsAndKingdomsModSystem>(true);
			this.modSystem.RegisterPlotMapLayer(this);
		}

		// Token: 0x060005B1 RID: 1457 RVA: 0x000266E7 File Offset: 0x000248E7
		[NullableContext(2)]
		public void SetActiveGuildDialog(DialogGuildMain dialog)
		{
			this.activeGuildDialog = dialog;
		}

		// Token: 0x060005B2 RID: 1458 RVA: 0x000266F0 File Offset: 0x000248F0
		public void UpdateConfigFromServer(GuildConfigPacket config)
		{
			this.territorialRestrictionsEnabled = config.TerritorialRestrictionsEnabled;
			if (config.TerritorialCenterX != null && config.TerritorialCenterZ != null)
			{
				this.territorialCenter = new ValueTuple<int, int>?(new ValueTuple<int, int>(config.TerritorialCenterX.Value, config.TerritorialCenterZ.Value));
			}
			else
			{
				this.territorialCenter = null;
			}
			this.territorialRadius = config.TerritorialRadius;
			this.protectedZonesEnabled = config.ProtectedZonesEnabled;
			this.protectedZones.Clear();
			if (config.ProtectedZones != null)
			{
				foreach (ProtectedZoneData zone in config.ProtectedZones)
				{
					this.protectedZones.Add(new ValueTuple<string, int, int, int, List<string>>(zone.Name, zone.X, zone.Z, zone.Radius, zone.WhitelistedPlayers));
				}
			}
			this.nodes.Clear();
			if (config.Nodes != null)
			{
				foreach (NodeData node in config.Nodes)
				{
					this.nodes.Add(new ValueTuple<string, int, int, int>(node.Name, node.X, node.Z, node.Radius));
				}
			}
			this.lastConfigUpdate = DateTime.UtcNow.Ticks;
		}

		// Token: 0x060005B3 RID: 1459 RVA: 0x0002688C File Offset: 0x00024A8C
		private bool IsChunkWithinTerritorialBounds(int chunkX, int chunkZ)
		{
			if (!this.territorialRestrictionsEnabled || this.territorialCenter == null)
			{
				return true;
			}
			Vec3i mapSize = this.clientApi.World.BlockAccessor.MapSize;
			int blockX = chunkX * 32;
			int blockZ = chunkZ * 32;
			int blockX2 = blockX + 32 - 1;
			int blockZ2 = blockZ + 32 - 1;
			return this.IsWithinTerritorialBounds(blockX, blockZ, mapSize) && this.IsWithinTerritorialBounds(blockX2, blockZ, mapSize) && this.IsWithinTerritorialBounds(blockX, blockZ2, mapSize) && this.IsWithinTerritorialBounds(blockX2, blockZ2, mapSize);
		}

		// Token: 0x060005B4 RID: 1460 RVA: 0x00026910 File Offset: 0x00024B10
		private bool IsWithinTerritorialBounds(int blockX, int blockZ, Vec3i mapSize)
		{
			if (!this.territorialRestrictionsEnabled || this.territorialCenter == null)
			{
				return true;
			}
			double num = (double)(blockX - this.territorialCenter.Value.Item1 - mapSize.X / 2);
			double deltaZ = (double)(blockZ - this.territorialCenter.Value.Item2 - mapSize.Z / 2);
			return Math.Sqrt(num * num + deltaZ * deltaZ) <= (double)this.territorialRadius;
		}

		// Token: 0x060005B5 RID: 1461 RVA: 0x00026984 File Offset: 0x00024B84
		private bool IsChunkWithinProtectedZone(int chunkX, int chunkZ)
		{
			if (!this.protectedZonesEnabled || this.protectedZones == null || this.protectedZones.Count == 0)
			{
				return false;
			}
			BlockPos spawnPos = this.clientApi.World.DefaultSpawnPosition.AsBlockPos;
			int blockX = chunkX * 32;
			int blockZ = chunkZ * 32;
			int blockX2 = blockX + 32 - 1;
			int blockZ2 = blockZ + 32 - 1;
			return this.IsWithinProtectedZone(blockX, blockZ, spawnPos) || this.IsWithinProtectedZone(blockX2, blockZ, spawnPos) || this.IsWithinProtectedZone(blockX, blockZ2, spawnPos) || this.IsWithinProtectedZone(blockX2, blockZ2, spawnPos);
		}

		// Token: 0x060005B6 RID: 1462 RVA: 0x00026A10 File Offset: 0x00024C10
		private bool IsWithinProtectedZone(int blockX, int blockZ, BlockPos spawnPos)
		{
			if (!this.protectedZonesEnabled || this.protectedZones == null || this.protectedZones.Count == 0)
			{
				return false;
			}
			foreach (ValueTuple<string, int, int, int, List<string>> zone in this.protectedZones)
			{
				double num = (double)(blockX - zone.Item2 - spawnPos.X);
				double deltaZ = (double)(blockZ - zone.Item3 - spawnPos.Z);
				if (Math.Sqrt(num * num + deltaZ * deltaZ) <= (double)zone.Item4)
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x060005B7 RID: 1463 RVA: 0x00026AB8 File Offset: 0x00024CB8
		[return: TupleElementNames(new string[]
		{
			"name",
			"x",
			"z",
			"radius",
			"whitelistedPlayers"
		})]
		[return: Nullable(new byte[]
		{
			0,
			1,
			1,
			1
		})]
		public ValueTuple<string, int, int, int, List<string>>? GetProtectedZoneAt(int blockX, int blockZ)
		{
			ICoreClientAPI coreClientAPI = this.clientApi;
			BlockPos spawnPos = (coreClientAPI != null) ? coreClientAPI.World.DefaultSpawnPosition.AsBlockPos : null;
			if (spawnPos == null)
			{
				return null;
			}
			return this.GetProtectedZoneAtInternal(blockX, blockZ, spawnPos);
		}

		// Token: 0x060005B8 RID: 1464 RVA: 0x00026AFE File Offset: 0x00024CFE
		[return: TupleElementNames(new string[]
		{
			"name",
			"x",
			"z",
			"radius",
			"whitelistedPlayers"
		})]
		[return: Nullable(new byte[]
		{
			0,
			1,
			1,
			1
		})]
		private ValueTuple<string, int, int, int, List<string>>? GetProtectedZoneAt(int blockX, int blockZ, BlockPos spawnPos)
		{
			return this.GetProtectedZoneAtInternal(blockX, blockZ, spawnPos);
		}

		// Token: 0x060005B9 RID: 1465 RVA: 0x00026B0C File Offset: 0x00024D0C
		private bool IsChunkWithinNode(int chunkX, int chunkZ)
		{
			if (this.nodes == null || this.nodes.Count == 0)
			{
				return false;
			}
			BlockPos spawnPos = this.clientApi.World.DefaultSpawnPosition.AsBlockPos;
			int blockX = chunkX * 32;
			int blockZ = chunkZ * 32;
			int blockX2 = blockX + 32 - 1;
			int blockZ2 = blockZ + 32 - 1;
			return this.IsWithinNode(blockX, blockZ, spawnPos) || this.IsWithinNode(blockX2, blockZ, spawnPos) || this.IsWithinNode(blockX, blockZ2, spawnPos) || this.IsWithinNode(blockX2, blockZ2, spawnPos);
		}

		// Token: 0x060005BA RID: 1466 RVA: 0x00026B90 File Offset: 0x00024D90
		private bool IsWithinNode(int blockX, int blockZ, BlockPos spawnPos)
		{
			if (this.nodes == null || this.nodes.Count == 0)
			{
				return false;
			}
			foreach (ValueTuple<string, int, int, int> node in this.nodes)
			{
				double num = (double)(blockX - node.Item2 - spawnPos.X);
				double deltaZ = (double)(blockZ - node.Item3 - spawnPos.Z);
				if (Math.Sqrt(num * num + deltaZ * deltaZ) <= (double)node.Item4)
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x060005BB RID: 1467 RVA: 0x00026C30 File Offset: 0x00024E30
		[return: TupleElementNames(new string[]
		{
			"name",
			"x",
			"z",
			"radius",
			"whitelistedPlayers"
		})]
		[return: Nullable(new byte[]
		{
			0,
			1,
			1,
			1
		})]
		private ValueTuple<string, int, int, int, List<string>>? GetProtectedZoneAtInternal(int blockX, int blockZ, BlockPos spawnPos)
		{
			if (!this.protectedZonesEnabled || this.protectedZones == null || this.protectedZones.Count == 0)
			{
				return null;
			}
			foreach (ValueTuple<string, int, int, int, List<string>> zone in this.protectedZones)
			{
				double num = (double)(blockX - zone.Item2 - spawnPos.X);
				double deltaZ = (double)(blockZ - zone.Item3 - spawnPos.Z);
				if (Math.Sqrt(num * num + deltaZ * deltaZ) <= (double)zone.Item4)
				{
					return new ValueTuple<string, int, int, int, List<string>>?(zone);
				}
			}
			return null;
		}

		// Token: 0x060005BC RID: 1468 RVA: 0x00026CF0 File Offset: 0x00024EF0
		[return: TupleElementNames(new string[]
		{
			"name",
			"x",
			"z",
			"radius"
		})]
		[return: Nullable(new byte[]
		{
			0,
			1
		})]
		private ValueTuple<string, int, int, int>? GetNodeAt(int blockX, int blockZ, BlockPos spawnPos)
		{
			if (this.nodes == null || this.nodes.Count == 0)
			{
				return null;
			}
			foreach (ValueTuple<string, int, int, int> node in this.nodes)
			{
				double num = (double)(blockX - node.Item2 - spawnPos.X);
				double deltaZ = (double)(blockZ - node.Item3 - spawnPos.Z);
				if (Math.Sqrt(num * num + deltaZ * deltaZ) <= (double)node.Item4)
				{
					return new ValueTuple<string, int, int, int>?(node);
				}
			}
			return null;
		}

		// Token: 0x060005BD RID: 1469 RVA: 0x00026DA8 File Offset: 0x00024FA8
		[return: TupleElementNames(new string[]
		{
			"tooClose",
			"nearestGuildName",
			"distance"
		})]
		[return: Nullable(new byte[]
		{
			0,
			1
		})]
		private ValueTuple<bool, string, double> IsChunkTooCloseToOtherGuildClaim(int chunkX, int chunkZ, string currentGuildName, [TupleElementNames(new string[]
		{
			"guildName",
			"guildSummary",
			"claim"
		})] [Nullable(new byte[]
		{
			1,
			1,
			0,
			1,
			1,
			1
		})] Dictionary<Vec2i, ValueTuple<string, GuildSummary, LandClaimDto>> claimedChunks)
		{
			int centerBlockX = chunkX * 32 + 16;
			int centerBlockZ = chunkZ * 32 + 16;
			double nearestDistance = double.MaxValue;
			string nearestGuild = null;
			foreach (KeyValuePair<Vec2i, ValueTuple<string, GuildSummary, LandClaimDto>> claimEntry in claimedChunks)
			{
				ValueTuple<string, GuildSummary, LandClaimDto> value = claimEntry.Value;
				string guildName = value.Item1;
				LandClaimDto claim = value.Item3;
				if (!(guildName == currentGuildName))
				{
					int claimCenterX = claim.ChunkX * 32 + 16;
					int claimCenterZ = claim.ChunkZ * 32 + 16;
					double num = (double)(centerBlockX - claimCenterX);
					double deltaZ = (double)(centerBlockZ - claimCenterZ);
					double distance = Math.Sqrt(num * num + deltaZ * deltaZ);
					if (distance < nearestDistance)
					{
						nearestDistance = distance;
						nearestGuild = guildName;
					}
					if (distance < 300.0)
					{
						return new ValueTuple<bool, string, double>(true, guildName, distance);
					}
				}
			}
			return new ValueTuple<bool, string, double>(false, nearestGuild, nearestDistance);
		}

		// Token: 0x060005BE RID: 1470 RVA: 0x00026EA0 File Offset: 0x000250A0
		private Vec4f ArgbToRgbaVec4f(int argbColor)
		{
			byte a = (byte)(argbColor >> 24 & 255);
			float num = (float)((byte)(argbColor >> 16 & 255));
			byte g = (byte)(argbColor >> 8 & 255);
			byte b = (byte)(argbColor & 255);
			return new Vec4f(num / 255f, (float)g / 255f, (float)b / 255f, (float)a / 255f);
		}

		// Token: 0x060005BF RID: 1471 RVA: 0x00026EFC File Offset: 0x000250FC
		private void RenderFilledRectangle(int textureId, ElementBounds mapBounds, float x1, float y1, float width, float height, float z, Vec4f color)
		{
			if (textureId <= 0)
			{
				return;
			}
			this.clientApi.Render.Render2DTexture(textureId, (float)((int)(mapBounds.renderX + (double)x1)), (float)((int)(mapBounds.renderY + (double)y1)), (float)((int)width), (float)((int)height), z, color);
		}

		// Token: 0x060005C0 RID: 1472 RVA: 0x00026F38 File Offset: 0x00025138
		private void RenderBorder(int textureId, ElementBounds mapBounds, float x1, float y1, float x2, float y2, float borderWidth, float z, Vec4f color)
		{
			if (textureId <= 0)
			{
				return;
			}
			float width = x2 - x1;
			float height = y2 - y1;
			this.RenderFilledRectangle(textureId, mapBounds, x1, y1, width, borderWidth, z, color);
			this.RenderFilledRectangle(textureId, mapBounds, x2 - borderWidth, y1, borderWidth, height, z, color);
			this.RenderFilledRectangle(textureId, mapBounds, x1, y2 - borderWidth, width, borderWidth, z, color);
			this.RenderFilledRectangle(textureId, mapBounds, x1, y1, borderWidth, height, z, color);
		}

		// Token: 0x060005C1 RID: 1473 RVA: 0x00026FA4 File Offset: 0x000251A4
		private void RenderSelectiveBorder(int textureId, ElementBounds mapBounds, float x1, float y1, float x2, float y2, float borderWidth, float z, Vec4f color, bool drawTop, bool drawRight, bool drawBottom, bool drawLeft)
		{
			if (textureId <= 0)
			{
				return;
			}
			float width = x2 - x1;
			float height = y2 - y1;
			if (drawTop)
			{
				this.RenderFilledRectangle(textureId, mapBounds, x1, y1, width, borderWidth, z, color);
			}
			if (drawRight)
			{
				this.RenderFilledRectangle(textureId, mapBounds, x2 - borderWidth, y1, borderWidth, height, z, color);
			}
			if (drawBottom)
			{
				this.RenderFilledRectangle(textureId, mapBounds, x1, y2 - borderWidth, width, borderWidth, z, color);
			}
			if (drawLeft)
			{
				this.RenderFilledRectangle(textureId, mapBounds, x1, y1, borderWidth, height, z, color);
			}
		}

		// Token: 0x060005C2 RID: 1474 RVA: 0x00027020 File Offset: 0x00025220
		private void RenderClaimedChunk(ElementBounds mapBounds, float x1, float y1, float x2, float y2, GuildSummary guildSummary, bool isGuildHome = false, bool drawTopBorder = true, bool drawRightBorder = true, bool drawBottomBorder = true, bool drawLeftBorder = true)
		{
			if (this.whiteTextureId == null || this.whiteTextureId.Value <= 0)
			{
				return;
			}
			float width = x2 - x1;
			float height = y2 - y1;
			Vec4f guildColor = this.ArgbToRgbaVec4f(guildSummary.DisplayColor);
			guildColor.A = (isGuildHome ? 0.6f : 0.4f);
			this.RenderFilledRectangle(this.whiteTextureId.Value, mapBounds, x1, y1, width, height, 50f, guildColor);
			Vec4f secondaryColor = this.ArgbToRgbaVec4f(guildSummary.SecondaryColor);
			secondaryColor.A = 1f;
			float borderWidth = isGuildHome ? 3f : 2f;
			this.RenderSelectiveBorder(this.whiteTextureId.Value, mapBounds, x1, y1, x2, y2, borderWidth, 51f, secondaryColor, drawTopBorder, drawRightBorder, drawBottomBorder, drawLeftBorder);
		}

		// Token: 0x060005C3 RID: 1475 RVA: 0x000270E8 File Offset: 0x000252E8
		private void RenderPendingUnclaimChunk(ElementBounds mapBounds, float x1, float y1, float x2, float y2)
		{
			if (this.whiteTextureId == null || this.whiteTextureId.Value <= 0)
			{
				return;
			}
			float width = x2 - x1;
			float height = y2 - y1;
			Vec4f pendingUnclaimColor = ColorUtil.ToRGBAVec4f(ColorUtil.ColorFromRgba(0, 100, 255, 120));
			this.RenderFilledRectangle(this.whiteTextureId.Value, mapBounds, x1, y1, width, height, 52f, pendingUnclaimColor);
			Vec4f pendingUnclaimBorderColor = ColorUtil.ToRGBAVec4f(ColorUtil.ColorFromRgba(0, 50, 255, 255));
			float borderWidth = 2f;
			this.RenderBorder(this.whiteTextureId.Value, mapBounds, x1, y1, x2, y2, borderWidth, 53f, pendingUnclaimBorderColor);
		}

		// Token: 0x060005C4 RID: 1476 RVA: 0x0002718C File Offset: 0x0002538C
		private void RenderPendingChunk(ElementBounds mapBounds, float x1, float y1, float x2, float y2, bool isPendingGuildHome = false)
		{
			if (this.whiteTextureId == null || this.whiteTextureId.Value <= 0)
			{
				return;
			}
			float width = x2 - x1;
			float height = y2 - y1;
			Vec4f pendingColor = ColorUtil.ToRGBAVec4f(ColorUtil.ColorFromRgba(255, 255, 0, isPendingGuildHome ? 150 : 102));
			this.RenderFilledRectangle(this.whiteTextureId.Value, mapBounds, x1, y1, width, height, 50f, pendingColor);
			Vec4f pendingBorderColor = ColorUtil.ToRGBAVec4f(ColorUtil.ColorFromRgba(255, 255, 0, 255));
			float borderWidth = isPendingGuildHome ? 3f : 2f;
			this.RenderBorder(this.whiteTextureId.Value, mapBounds, x1, y1, x2, y2, borderWidth, 51f, pendingBorderColor);
		}

		// Token: 0x060005C5 RID: 1477 RVA: 0x0002724C File Offset: 0x0002544C
		private void RenderHoverHighlight(ElementBounds mapBounds, float x1, float y1, float x2, float y2, bool canClaim)
		{
			if (this.whiteTextureId == null || this.whiteTextureId.Value <= 0)
			{
				return;
			}
			float width = x2 - x1;
			float height = y2 - y1;
			Vec4f hoverColor;
			if (!canClaim)
			{
				hoverColor = ColorUtil.ToRGBAVec4f(ColorUtil.ColorFromRgba(255, 100, 100, 200));
			}
			else
			{
				hoverColor = ColorUtil.ToRGBAVec4f(ColorUtil.ColorFromRgba(255, 255, 255, 255));
			}
			this.RenderFilledRectangle(this.whiteTextureId.Value, mapBounds, x1, y1, width, height, 52f, hoverColor);
		}

		// Token: 0x060005C6 RID: 1478 RVA: 0x000272DC File Offset: 0x000254DC
		private void RenderTerritorialRestriction(ElementBounds mapBounds, float x1, float y1, float x2, float y2)
		{
			if (this.whiteTextureId == null || this.whiteTextureId.Value <= 0)
			{
				return;
			}
			float width = x2 - x1;
			float height = y2 - y1;
			Vec4f restrictedColor = ColorUtil.ToRGBAVec4f(ColorUtil.ColorFromRgba(255, 0, 0, 40));
			this.RenderFilledRectangle(this.whiteTextureId.Value, mapBounds, x1, y1, width, height, 49f, restrictedColor);
			Vec4f borderColor = ColorUtil.ToRGBAVec4f(ColorUtil.ColorFromRgba(255, 0, 0, 150));
			this.RenderBorder(this.whiteTextureId.Value, mapBounds, x1, y1, x2, y2, 1.5f, 50f, borderColor);
		}

		// Token: 0x060005C7 RID: 1479 RVA: 0x0002737C File Offset: 0x0002557C
		private void RenderProtectedZone(ElementBounds mapBounds, float x1, float y1, float x2, float y2)
		{
			if (this.whiteTextureId == null || this.whiteTextureId.Value <= 0)
			{
				return;
			}
			float width = x2 - x1;
			float height = y2 - y1;
			Vec4f protectedColor = ColorUtil.ToRGBAVec4f(ColorUtil.ColorFromRgba(128, 0, 255, 50));
			this.RenderFilledRectangle(this.whiteTextureId.Value, mapBounds, x1, y1, width, height, 49f, protectedColor);
			Vec4f borderColor = ColorUtil.ToRGBAVec4f(ColorUtil.ColorFromRgba(128, 0, 255, 180));
			this.RenderBorder(this.whiteTextureId.Value, mapBounds, x1, y1, x2, y2, 2f, 50f, borderColor);
		}

		// Token: 0x060005C8 RID: 1480 RVA: 0x00027424 File Offset: 0x00025624
		private void RenderNode(ElementBounds mapBounds, float x1, float y1, float x2, float y2)
		{
			if (this.whiteTextureId == null || this.whiteTextureId.Value <= 0)
			{
				return;
			}
			float width = x2 - x1;
			float height = y2 - y1;
			Vec4f nodeColor = ColorUtil.ToRGBAVec4f(ColorUtil.ColorFromRgba(0, 255, 200, 50));
			this.RenderFilledRectangle(this.whiteTextureId.Value, mapBounds, x1, y1, width, height, 49f, nodeColor);
			Vec4f borderColor = ColorUtil.ToRGBAVec4f(ColorUtil.ColorFromRgba(0, 255, 200, 180));
			this.RenderBorder(this.whiteTextureId.Value, mapBounds, x1, y1, x2, y2, 2f, 50f, borderColor);
		}

		// Token: 0x060005C9 RID: 1481 RVA: 0x000274CC File Offset: 0x000256CC
		private void RenderTerritorialBoundary(GuiElementMap mapElem, ElementBounds mapBounds)
		{
			if (!this.territorialRestrictionsEnabled || this.territorialCenter == null || this.whiteTextureId == null)
			{
				return;
			}
			int chunkSize = 32;
			BlockPos spawnPos = this.clientApi.World.DefaultSpawnPosition.AsBlockPos;
			ValueTuple<int, int> value = this.territorialCenter.Value;
			double worldX = (double)(value.Item1 + spawnPos.X);
			double worldZ = (double)(value.Item2 + spawnPos.Z);
			Vec2f centerViewPos = new Vec2f();
			mapElem.TranslateWorldPosToViewPos(new Vec3d(worldX, 0.0, worldZ), ref centerViewPos);
			double zoomLevel = (double)mapElem.ZoomLevel;
			float margin = (float)((double)this.territorialRadius * zoomLevel) + 100f;
			if (centerViewPos.X + margin < 0f || (double)(centerViewPos.X - margin) > mapBounds.InnerWidth || centerViewPos.Y + margin < 0f || (double)(centerViewPos.Y - margin) > mapBounds.InnerHeight)
			{
				return;
			}
			int radiusInChunks = (int)Math.Ceiling((double)this.territorialRadius / (double)chunkSize);
			int samplesPerQuadrant = (int)((double)Math.Max(15, radiusInChunks / 2) * Math.Max(0.15, Math.Pow(zoomLevel, 1.5)));
			samplesPerQuadrant = Math.Max(12, Math.Min(samplesPerQuadrant, 60));
			Vec4f boundaryColor = ColorUtil.ToRGBAVec4f(ColorUtil.ColorFromRgba(255, 100, 0, 200));
			for (int i = 0; i < samplesPerQuadrant * 4; i++)
			{
				double angle = (double)i / (double)(samplesPerQuadrant * 4) * 3.141592653589793 * 2.0;
				double x = worldX + Math.Cos(angle) * (double)this.territorialRadius;
				double z = worldZ + Math.Sin(angle) * (double)this.territorialRadius;
				Vec2f viewPos = new Vec2f();
				mapElem.TranslateWorldPosToViewPos(new Vec3d(x, 0.0, z), ref viewPos);
				if (viewPos.X >= -10f && (double)viewPos.X <= mapBounds.InnerWidth + 10.0 && viewPos.Y >= -10f && (double)viewPos.Y <= mapBounds.InnerHeight + 10.0)
				{
					float dotSize = Math.Max(2f, 3f * (float)zoomLevel);
					this.RenderFilledRectangle(this.whiteTextureId.Value, mapBounds, viewPos.X - dotSize / 2f, viewPos.Y - dotSize / 2f, dotSize, dotSize, 55f, boundaryColor);
				}
			}
			if (centerViewPos.X >= -50f && (double)centerViewPos.X <= mapBounds.InnerWidth + 50.0 && centerViewPos.Y >= -50f && (double)centerViewPos.Y <= mapBounds.InnerHeight + 50.0)
			{
				Vec4f centerColor = ColorUtil.ToRGBAVec4f(ColorUtil.ColorFromRgba(255, 100, 0, 255));
				float markerSize = Math.Max(6f, 8f * (float)zoomLevel);
				this.RenderFilledRectangle(this.whiteTextureId.Value, mapBounds, centerViewPos.X - markerSize / 2f, centerViewPos.Y - markerSize / 2f, markerSize, markerSize, 56f, centerColor);
			}
		}

		// Token: 0x060005CA RID: 1482 RVA: 0x00027814 File Offset: 0x00025A14
		private void RenderProtectedZoneBoundaries(GuiElementMap mapElem, ElementBounds mapBounds)
		{
			if (!this.protectedZonesEnabled || this.protectedZones == null || this.protectedZones.Count == 0 || this.whiteTextureId == null)
			{
				return;
			}
			int chunkSize = 32;
			BlockPos spawnPos = this.clientApi.World.DefaultSpawnPosition.AsBlockPos;
			double zoomLevel = (double)mapElem.ZoomLevel;
			foreach (ValueTuple<string, int, int, int, List<string>> zone in this.protectedZones)
			{
				double worldX = (double)(zone.Item2 + spawnPos.X);
				double worldZ = (double)(zone.Item3 + spawnPos.Z);
				Vec2f zoneCenterViewPos = new Vec2f();
				mapElem.TranslateWorldPosToViewPos(new Vec3d(worldX, 0.0, worldZ), ref zoneCenterViewPos);
				float viewportMargin = (float)((double)zone.Item4 * zoomLevel) + 100f;
				if (zoneCenterViewPos.X >= -viewportMargin && (double)zoneCenterViewPos.X <= mapBounds.InnerWidth + (double)viewportMargin && zoneCenterViewPos.Y >= -viewportMargin && (double)zoneCenterViewPos.Y <= mapBounds.InnerHeight + (double)viewportMargin)
				{
					int radiusInChunks = (int)Math.Ceiling((double)zone.Item4 / (double)chunkSize);
					int samplesPerQuadrant = (int)((double)Math.Max(10, Math.Min(20, radiusInChunks / 4)) * Math.Max(0.15, Math.Pow(zoomLevel, 1.5)));
					if (zone.Item4 > 1500 || zoomLevel < 0.3)
					{
						samplesPerQuadrant = Math.Max(8, samplesPerQuadrant / 2);
					}
					samplesPerQuadrant = Math.Max(8, Math.Min(samplesPerQuadrant, 40));
					Vec4f boundaryColor = ColorUtil.ToRGBAVec4f(ColorUtil.ColorFromRgba(128, 0, 255, 200));
					for (int i = 0; i < samplesPerQuadrant * 4; i++)
					{
						double angle = (double)i / (double)(samplesPerQuadrant * 4) * 3.141592653589793 * 2.0;
						double x = worldX + Math.Cos(angle) * (double)zone.Item4;
						double z = worldZ + Math.Sin(angle) * (double)zone.Item4;
						Vec2f viewPos = new Vec2f();
						mapElem.TranslateWorldPosToViewPos(new Vec3d(x, 0.0, z), ref viewPos);
						if (viewPos.X >= -10f && (double)viewPos.X <= mapBounds.InnerWidth + 10.0 && viewPos.Y >= -10f && (double)viewPos.Y <= mapBounds.InnerHeight + 10.0)
						{
							float dotSize = Math.Max(2f, 3f * (float)zoomLevel);
							this.RenderFilledRectangle(this.whiteTextureId.Value, mapBounds, viewPos.X - dotSize / 2f, viewPos.Y - dotSize / 2f, dotSize, dotSize, 55f, boundaryColor);
						}
					}
					if (zoneCenterViewPos.X >= -50f && (double)zoneCenterViewPos.X <= mapBounds.InnerWidth + 50.0 && zoneCenterViewPos.Y >= -50f && (double)zoneCenterViewPos.Y <= mapBounds.InnerHeight + 50.0)
					{
						Vec4f centerColor = ColorUtil.ToRGBAVec4f(ColorUtil.ColorFromRgba(128, 0, 255, 255));
						float markerSize = Math.Max(4f, 6f * (float)zoomLevel);
						this.RenderFilledRectangle(this.whiteTextureId.Value, mapBounds, zoneCenterViewPos.X - markerSize / 2f, zoneCenterViewPos.Y - markerSize / 2f, markerSize, markerSize, 56f, centerColor);
					}
				}
			}
		}

		// Token: 0x060005CB RID: 1483 RVA: 0x00027BE8 File Offset: 0x00025DE8
		private void RenderNodeBoundaries(GuiElementMap mapElem, ElementBounds mapBounds)
		{
			if (this.nodes == null || this.nodes.Count == 0 || this.whiteTextureId == null)
			{
				return;
			}
			int chunkSize = 32;
			BlockPos spawnPos = this.clientApi.World.DefaultSpawnPosition.AsBlockPos;
			double zoomLevel = (double)mapElem.ZoomLevel;
			foreach (ValueTuple<string, int, int, int> node in this.nodes)
			{
				double worldX = (double)(node.Item2 + spawnPos.X);
				double worldZ = (double)(node.Item3 + spawnPos.Z);
				Vec2f nodeCenterViewPos = new Vec2f();
				mapElem.TranslateWorldPosToViewPos(new Vec3d(worldX, 0.0, worldZ), ref nodeCenterViewPos);
				float viewportMargin = (float)((double)node.Item4 * zoomLevel) + 100f;
				if (nodeCenterViewPos.X >= -viewportMargin && (double)nodeCenterViewPos.X <= mapBounds.InnerWidth + (double)viewportMargin && nodeCenterViewPos.Y >= -viewportMargin && (double)nodeCenterViewPos.Y <= mapBounds.InnerHeight + (double)viewportMargin)
				{
					int radiusInChunks = (int)Math.Ceiling((double)node.Item4 / (double)chunkSize);
					int samplesPerSide = (int)((double)Math.Max(8, Math.Min(15, radiusInChunks / 3)) * Math.Max(0.15, Math.Pow(zoomLevel, 1.5)));
					if (node.Item4 > 1500 || zoomLevel < 0.3)
					{
						samplesPerSide = Math.Max(6, samplesPerSide / 2);
					}
					samplesPerSide = Math.Max(6, Math.Min(samplesPerSide, 30));
					Vec4f boundaryColor = ColorUtil.ToRGBAVec4f(ColorUtil.ColorFromRgba(0, 255, 200, 200));
					double angleOffset = -1.5707963267948966;
					Vec2f[] triangleVertices = new Vec2f[3];
					for (int v = 0; v < 3; v++)
					{
						double angle = angleOffset + (double)(v * 2) * 3.141592653589793 / 3.0;
						double x = worldX + Math.Cos(angle) * (double)node.Item4;
						double z = worldZ + Math.Sin(angle) * (double)node.Item4;
						Vec2f viewPos = new Vec2f();
						mapElem.TranslateWorldPosToViewPos(new Vec3d(x, 0.0, z), ref viewPos);
						triangleVertices[v] = viewPos;
					}
					for (int side = 0; side < 3; side++)
					{
						Vec2f start = triangleVertices[side];
						Vec2f end = triangleVertices[(side + 1) % 3];
						for (int i = 0; i <= samplesPerSide; i++)
						{
							float t = (float)i / (float)samplesPerSide;
							float x2 = start.X + (end.X - start.X) * t;
							float y = start.Y + (end.Y - start.Y) * t;
							if (x2 >= -10f && (double)x2 <= mapBounds.InnerWidth + 10.0 && y >= -10f && (double)y <= mapBounds.InnerHeight + 10.0)
							{
								float dotSize = Math.Max(2f, 3f * (float)zoomLevel);
								this.RenderFilledRectangle(this.whiteTextureId.Value, mapBounds, x2 - dotSize / 2f, y - dotSize / 2f, dotSize, dotSize, 55f, boundaryColor);
							}
						}
					}
					if (nodeCenterViewPos.X >= -50f && (double)nodeCenterViewPos.X <= mapBounds.InnerWidth + 50.0 && nodeCenterViewPos.Y >= -50f && (double)nodeCenterViewPos.Y <= mapBounds.InnerHeight + 50.0)
					{
						Vec4f centerColor = ColorUtil.ToRGBAVec4f(ColorUtil.ColorFromRgba(0, 255, 200, 255));
						float markerSize = Math.Max(6f, 9f * (float)zoomLevel);
						float topX = nodeCenterViewPos.X;
						float topY = nodeCenterViewPos.Y - markerSize / 2f;
						float blX = nodeCenterViewPos.X - markerSize / 2f;
						float blY = nodeCenterViewPos.Y + markerSize / 2f;
						float brX = nodeCenterViewPos.X + markerSize / 2f;
						float brY = nodeCenterViewPos.Y + markerSize / 2f;
						int markerDots = 8;
						for (int j = 0; j <= markerDots; j++)
						{
							float t2 = (float)j / (float)markerDots;
							float x3 = topX + (blX - topX) * t2;
							float y2 = topY + (blY - topY) * t2;
							this.RenderFilledRectangle(this.whiteTextureId.Value, mapBounds, x3 - 1f, y2 - 1f, 2f, 2f, 56f, centerColor);
							float x4 = blX + (brX - blX) * t2;
							float y3 = blY + (brY - blY) * t2;
							this.RenderFilledRectangle(this.whiteTextureId.Value, mapBounds, x4 - 1f, y3 - 1f, 2f, 2f, 56f, centerColor);
							float x5 = brX + (topX - brX) * t2;
							float y4 = brY + (topY - brY) * t2;
							this.RenderFilledRectangle(this.whiteTextureId.Value, mapBounds, x5 - 1f, y4 - 1f, 2f, 2f, 56f, centerColor);
						}
					}
				}
			}
		}

		// Token: 0x060005CC RID: 1484 RVA: 0x00028154 File Offset: 0x00026354
		[return: TupleElementNames(new string[]
		{
			"type",
			"guild",
			"claim"
		})]
		[return: Nullable(new byte[]
		{
			0,
			2,
			2
		})]
		private ValueTuple<ChunkType, GuildSummary, LandClaimDto> GetChunkInfo(int chunkX, int chunkZ, [TupleElementNames(new string[]
		{
			"guildName",
			"guildSummary",
			"claim"
		})] [Nullable(new byte[]
		{
			1,
			1,
			0,
			1,
			1,
			1
		})] Dictionary<Vec2i, ValueTuple<string, GuildSummary, LandClaimDto>> claimedChunks, [Nullable(new byte[]
		{
			1,
			0
		})] List<ValueTuple<int, int>> pendingClaims)
		{
			Vec2i chunkKey = new Vec2i(chunkX, chunkZ);
			ValueTuple<string, GuildSummary, LandClaimDto> claimInfo;
			if (claimedChunks.TryGetValue(chunkKey, out claimInfo))
			{
				return new ValueTuple<ChunkType, GuildSummary, LandClaimDto>(claimInfo.Item3.IsGuildHome ? ChunkType.GuildHome : ChunkType.Claimed, claimInfo.Item2, claimInfo.Item3);
			}
			if (pendingClaims.Any((ValueTuple<int, int> p) => p.Item1 == chunkX && p.Item2 == chunkZ))
			{
				DialogGuildMain dialogGuildMain = this.activeGuildDialog;
				return new ValueTuple<ChunkType, GuildSummary, LandClaimDto>((dialogGuildMain != null && dialogGuildMain.IsPendingGuildHomeChunk(chunkX, chunkZ)) ? ChunkType.PendingGuildHome : ChunkType.Pending, null, null);
			}
			return new ValueTuple<ChunkType, GuildSummary, LandClaimDto>(ChunkType.Unclaimed, null, null);
		}

		// Token: 0x060005CD RID: 1485 RVA: 0x00028200 File Offset: 0x00026400
		private bool IsChunkOwnedByGuild(int chunkX, int chunkZ, string guildName, [TupleElementNames(new string[]
		{
			"guildName",
			"guildSummary",
			"claim"
		})] [Nullable(new byte[]
		{
			1,
			1,
			0,
			1,
			1,
			1
		})] Dictionary<Vec2i, ValueTuple<string, GuildSummary, LandClaimDto>> claimedChunks)
		{
			Vec2i chunkKey = new Vec2i(chunkX, chunkZ);
			ValueTuple<string, GuildSummary, LandClaimDto> claimInfo;
			return claimedChunks.TryGetValue(chunkKey, out claimInfo) && claimInfo.Item1 == guildName;
		}

		// Token: 0x060005CE RID: 1486 RVA: 0x00028230 File Offset: 0x00026430
		public override void Render(GuiElementMap mapElem, float dt)
		{
			if (!base.Active)
			{
				return;
			}
			ICoreClientAPI coreClientAPI = this.clientApi;
			bool flag;
			if (coreClientAPI == null)
			{
				flag = (null != null);
			}
			else
			{
				IClientWorldAccessor world = coreClientAPI.World;
				if (world == null)
				{
					flag = (null != null);
				}
				else
				{
					IClientPlayer player = world.Player;
					if (player == null)
					{
						flag = (null != null);
					}
					else
					{
						EntityPlayer entity = player.Entity;
						flag = (((entity != null) ? entity.Pos : null) != null);
					}
				}
			}
			if (!flag)
			{
				return;
			}
			BlockPos asBlockPos = this.clientApi.World.Player.Entity.Pos.AsBlockPos;
			int chunkSize = 32;
			ElementBounds mapBounds = mapElem.Bounds;
			double scale = (double)mapElem.ZoomLevel;
			int centerChunkX = LandClaim.FloorDiv(asBlockPos.X, chunkSize);
			int centerChunkZ = LandClaim.FloorDiv(asBlockPos.Z, chunkSize);
			Math.Max(1, (int)(Math.Max(mapBounds.InnerWidth, mapBounds.InnerHeight) / ((double)chunkSize * scale)));
			IRenderAPI renderApi = this.clientApi.Render;
			if (this.whiteTextureId == null)
			{
				this.whiteTextureId = new int?(renderApi.GetOrLoadTexture(new AssetLocation("srguildsandkingdoms:textures/gui/white.png")));
			}
			SRGuildsAndKingdomsModSystem srguildsAndKingdomsModSystem = this.modSystem;
			List<GuildSummary> guildSummaries = ((srguildsAndKingdomsModSystem != null) ? srguildsAndKingdomsModSystem.GetClientGuildSummaries() : null) ?? new List<GuildSummary>();
			Dictionary<Vec2i, ValueTuple<string, GuildSummary, LandClaimDto>> claimedChunks = new Dictionary<Vec2i, ValueTuple<string, GuildSummary, LandClaimDto>>();
			foreach (GuildSummary guild in guildSummaries)
			{
				foreach (LandClaimDto claim in guild.Claims)
				{
					Vec2i chunkKey = new Vec2i(claim.ChunkX, claim.ChunkZ);
					claimedChunks[chunkKey] = new ValueTuple<string, GuildSummary, LandClaimDto>(guild.Name, guild, claim);
				}
			}
			DialogGuildMain dialogGuildMain = this.activeGuildDialog;
			List<ValueTuple<int, int>> pendingClaims = ((dialogGuildMain != null) ? dialogGuildMain.GetPendingClaims() : null) ?? new List<ValueTuple<int, int>>();
			DialogGuildMain dialogGuildMain2 = this.activeGuildDialog;
			List<ValueTuple<int, int>> pendingUnclaims = ((dialogGuildMain2 != null) ? dialogGuildMain2.GetPendingUnclaims() : null) ?? new List<ValueTuple<int, int>>();
			DialogGuildMain dialogGuildMain3 = this.activeGuildDialog;
			bool isClaimingMode = dialogGuildMain3 != null && dialogGuildMain3.IsClaimingModeActive;
			DialogGuildMain dialogGuildMain4 = this.activeGuildDialog;
			bool isUnclaimingMode = dialogGuildMain4 != null && dialogGuildMain4.IsUnclaimingModeActive;
			Vec3d topLeft = new Vec3d();
			Vec3d bottomRight = new Vec3d();
			mapElem.TranslateViewPosToWorldPos(new Vec2f(0f, 0f), ref topLeft);
			mapElem.TranslateViewPosToWorldPos(new Vec2f((float)mapBounds.InnerWidth, (float)mapBounds.InnerHeight), ref bottomRight);
			int num = LandClaim.FloorDiv((int)topLeft.X, chunkSize) - 1;
			int maxVisibleChunkX = LandClaim.FloorDiv((int)bottomRight.X, chunkSize) + 1;
			int minVisibleChunkZ = LandClaim.FloorDiv((int)topLeft.Z, chunkSize) - 1;
			int maxVisibleChunkZ = LandClaim.FloorDiv((int)bottomRight.Z, chunkSize) + 1;
			bool isZoomedOut = scale < 0.5;
			float minRenderSize = isZoomedOut ? 3f : 2f;
			HashSet<ValueTuple<int, int>> chunksToRender = new HashSet<ValueTuple<int, int>>();
			for (int chunkX2 = num; chunkX2 <= maxVisibleChunkX; chunkX2++)
			{
				for (int chunkZ2 = minVisibleChunkZ; chunkZ2 <= maxVisibleChunkZ; chunkZ2++)
				{
					chunksToRender.Add(new ValueTuple<int, int>(chunkX2, chunkZ2));
				}
			}
			this.lastFrameChunkCount = chunksToRender.Count;
			foreach (ValueTuple<int, int> valueTuple in chunksToRender)
			{
				int chunkX = valueTuple.Item1;
				int chunkZ = valueTuple.Item2;
				double worldX = (double)(chunkX * chunkSize);
				double worldZ = (double)(chunkZ * chunkSize);
				double worldX2 = (double)((chunkX + 1) * chunkSize);
				double worldZ2 = (double)((chunkZ + 1) * chunkSize);
				Vec2f mapPos = new Vec2f();
				Vec2f mapPos2 = new Vec2f();
				mapElem.TranslateWorldPosToViewPos(new Vec3d(worldX, 0.0, worldZ), ref mapPos);
				mapElem.TranslateWorldPosToViewPos(new Vec3d(worldX2, 0.0, worldZ2), ref mapPos2);
				float x = Math.Min(mapPos.X, mapPos2.X);
				float y = Math.Min(mapPos.Y, mapPos2.Y);
				float x2 = Math.Max(mapPos.X, mapPos2.X);
				float y2 = Math.Max(mapPos.Y, mapPos2.Y);
				float num2 = x2 - x;
				float height = y2 - y;
				if (num2 >= minRenderSize && height >= minRenderSize)
				{
					try
					{
						bool isHovered = this.hoveredChunk != null && this.hoveredChunk.Value.Item1 == chunkX && this.hoveredChunk.Value.Item2 == chunkZ;
						bool isWithinTerritorialBounds = true;
						bool isInProtectedZone = false;
						bool isInNode = false;
						bool tooCloseToOtherGuild = false;
						bool flag2 = isClaimingMode && isHovered;
						if (flag2 || this.territorialRestrictionsEnabled || this.protectedZonesEnabled)
						{
							isWithinTerritorialBounds = this.IsChunkWithinTerritorialBounds(chunkX, chunkZ);
							isInProtectedZone = this.IsChunkWithinProtectedZone(chunkX, chunkZ);
							isInNode = this.IsChunkWithinNode(chunkX, chunkZ);
						}
						if (flag2)
						{
							GuildSummary currentGuild = guildSummaries.FirstOrDefault((GuildSummary g) => g.IsPlayerMember);
							tooCloseToOtherGuild = this.IsChunkTooCloseToOtherGuildClaim(chunkX, chunkZ, ((currentGuild != null) ? currentGuild.Name : null) ?? "", claimedChunks).Item1;
						}
						ValueTuple<ChunkType, GuildSummary, LandClaimDto> chunkInfo = this.GetChunkInfo(chunkX, chunkZ, claimedChunks, pendingClaims);
						ChunkType chunkType = chunkInfo.Item1;
						GuildSummary guild2 = chunkInfo.Item2;
						bool isPendingUnclaim = pendingUnclaims.Any(([TupleElementNames(new string[]
						{
							"chunkX",
							"chunkZ"
						})] ValueTuple<int, int> p) => p.Item1 == chunkX && p.Item2 == chunkZ);
						if (this.whiteTextureId != null && this.whiteTextureId.Value > 0)
						{
							switch (chunkType)
							{
							case ChunkType.Unclaimed:
								if (isClaimingMode && isHovered)
								{
									bool canClaim = isWithinTerritorialBounds && !isInProtectedZone && !tooCloseToOtherGuild;
									this.RenderHoverHighlight(mapBounds, x, y, x2, y2, canClaim);
								}
								break;
							case ChunkType.Claimed:
							{
								bool drawTop = !this.IsChunkOwnedByGuild(chunkX, chunkZ - 1, guild2.Name, claimedChunks);
								bool drawRight = !this.IsChunkOwnedByGuild(chunkX + 1, chunkZ, guild2.Name, claimedChunks);
								bool drawBottom = !this.IsChunkOwnedByGuild(chunkX, chunkZ + 1, guild2.Name, claimedChunks);
								bool drawLeft = !this.IsChunkOwnedByGuild(chunkX - 1, chunkZ, guild2.Name, claimedChunks);
								this.RenderClaimedChunk(mapBounds, x, y, x2, y2, guild2, false, drawTop, drawRight, drawBottom, drawLeft);
								if (isPendingUnclaim)
								{
									this.RenderPendingUnclaimChunk(mapBounds, x, y, x2, y2);
								}
								break;
							}
							case ChunkType.GuildHome:
							{
								bool drawTopGH = !this.IsChunkOwnedByGuild(chunkX, chunkZ - 1, guild2.Name, claimedChunks);
								bool drawRightGH = !this.IsChunkOwnedByGuild(chunkX + 1, chunkZ, guild2.Name, claimedChunks);
								bool drawBottomGH = !this.IsChunkOwnedByGuild(chunkX, chunkZ + 1, guild2.Name, claimedChunks);
								bool drawLeftGH = !this.IsChunkOwnedByGuild(chunkX - 1, chunkZ, guild2.Name, claimedChunks);
								this.RenderClaimedChunk(mapBounds, x, y, x2, y2, guild2, true, drawTopGH, drawRightGH, drawBottomGH, drawLeftGH);
								if (isPendingUnclaim)
								{
									this.RenderPendingUnclaimChunk(mapBounds, x, y, x2, y2);
								}
								break;
							}
							case ChunkType.Pending:
								this.RenderPendingChunk(mapBounds, x, y, x2, y2, false);
								break;
							case ChunkType.PendingGuildHome:
								this.RenderPendingChunk(mapBounds, x, y, x2, y2, true);
								break;
							}
							if (isUnclaimingMode && isHovered && (chunkType == ChunkType.Claimed || chunkType == ChunkType.GuildHome))
							{
								Vec4f unclaimHoverColor = ColorUtil.ToRGBAVec4f(ColorUtil.ColorFromRgba(100, 150, 255, 150));
								this.RenderFilledRectangle(this.whiteTextureId.Value, mapBounds, x, y, x2 - x, y2 - y, 54f, unclaimHoverColor);
							}
							if (isInNode && chunkType == ChunkType.Unclaimed)
							{
								this.RenderNode(mapBounds, x, y, x2, y2);
							}
						}
					}
					catch
					{
					}
					if (!isZoomedOut || (Math.Abs(chunkX - centerChunkX) < 10 && Math.Abs(chunkZ - centerChunkZ) < 10))
					{
						Vec2i chunkDataKey = new Vec2i(chunkX, chunkZ);
						if (!this.chunkDataCache.ContainsKey(chunkDataKey))
						{
							IWorldChunk chunk = this.clientApi.World.BlockAccessor.GetChunk(chunkX, 0, chunkZ);
							Dictionary<Vec2i, ChunkData> dictionary = this.chunkDataCache;
							Vec2i key = chunkDataKey;
							ChunkData chunkData = new ChunkData();
							chunkData.ChunkX = chunkX;
							chunkData.ChunkZ = chunkZ;
							chunkData.IsLoaded = (chunk != null);
							int? num3;
							if (chunk == null)
							{
								num3 = null;
							}
							else
							{
								Dictionary<BlockPos, BlockEntity> blockEntities = chunk.BlockEntities;
								num3 = ((blockEntities != null) ? new int?(blockEntities.Count) : null);
							}
							int? num4 = num3;
							chunkData.LoadedBlocks = num4.GetValueOrDefault();
							dictionary[key] = chunkData;
						}
					}
				}
			}
			this.RenderTerritorialBoundary(mapElem, mapBounds);
			this.RenderProtectedZoneBoundaries(mapElem, mapBounds);
			this.RenderNodeBoundaries(mapElem, mapBounds);
		}

		// Token: 0x060005CF RID: 1487 RVA: 0x00028B98 File Offset: 0x00026D98
		public override void OnMouseMoveClient(MouseEvent args, GuiElementMap mapElem, StringBuilder hoverText)
		{
			ICoreClientAPI coreClientAPI = this.clientApi;
			bool flag;
			if (coreClientAPI == null)
			{
				flag = (null != null);
			}
			else
			{
				IClientWorldAccessor world = coreClientAPI.World;
				if (world == null)
				{
					flag = (null != null);
				}
				else
				{
					IClientPlayer player = world.Player;
					if (player == null)
					{
						flag = (null != null);
					}
					else
					{
						EntityPlayer entity = player.Entity;
						flag = (((entity != null) ? entity.Pos : null) != null);
					}
				}
			}
			if (!flag)
			{
				return;
			}
			int chunkSize = 32;
			bool flag2 = (double)mapElem.ZoomLevel < 0.25 && this.lastFrameChunkCount > 500;
			SRGuildsAndKingdomsModSystem srguildsAndKingdomsModSystem = this.modSystem;
			List<GuildSummary> guildSummaries = ((srguildsAndKingdomsModSystem != null) ? srguildsAndKingdomsModSystem.GetClientGuildSummaries() : null) ?? new List<GuildSummary>();
			double mouseX = (double)args.X - mapElem.Bounds.renderX;
			double mouseY = (double)args.Y - mapElem.Bounds.renderY;
			this.hoveredChunk = null;
			if (flag2)
			{
				hoverText.AppendLine("Zoom in for detailed information");
				StringBuilder.AppendInterpolatedStringHandler appendInterpolatedStringHandler = new StringBuilder.AppendInterpolatedStringHandler(16, 1, hoverText);
				appendInterpolatedStringHandler.AppendLiteral("Visible chunks: ");
				appendInterpolatedStringHandler.AppendFormatted<int>(this.lastFrameChunkCount);
				hoverText.AppendLine(ref appendInterpolatedStringHandler);
				return;
			}
			foreach (GuildSummary guild in guildSummaries)
			{
				using (List<LandClaimDto>.Enumerator enumerator2 = guild.Claims.GetEnumerator())
				{
					while (enumerator2.MoveNext())
					{
						LandClaimDto claim = enumerator2.Current;
						Vec2f viewPos = new Vec2f();
						mapElem.TranslateWorldPosToViewPos(new Vec3d((double)(claim.ChunkX * chunkSize + chunkSize / 2), 0.0, (double)(claim.ChunkZ * chunkSize + chunkSize / 2)), ref viewPos);
						double chunkViewSize = (double)((float)chunkSize * mapElem.ZoomLevel);
						if (Math.Abs((double)viewPos.X - mouseX) < chunkViewSize / 2.0 && Math.Abs((double)viewPos.Y - mouseY) < chunkViewSize / 2.0)
						{
							this.hoveredChunk = new ValueTuple<int, int>?(new ValueTuple<int, int>(claim.ChunkX, claim.ChunkZ));
							StringBuilder.AppendInterpolatedStringHandler appendInterpolatedStringHandler = new StringBuilder.AppendInterpolatedStringHandler(11, 2, hoverText);
							appendInterpolatedStringHandler.AppendLiteral("Chunk: (");
							appendInterpolatedStringHandler.AppendFormatted<int>(claim.ChunkX);
							appendInterpolatedStringHandler.AppendLiteral(", ");
							appendInterpolatedStringHandler.AppendFormatted<int>(claim.ChunkZ);
							appendInterpolatedStringHandler.AppendLiteral(")");
							hoverText.AppendLine(ref appendInterpolatedStringHandler);
							appendInterpolatedStringHandler = new StringBuilder.AppendInterpolatedStringHandler(12, 1, hoverText);
							appendInterpolatedStringHandler.AppendLiteral("Claimed by: ");
							appendInterpolatedStringHandler.AppendFormatted(guild.Name);
							hoverText.AppendLine(ref appendInterpolatedStringHandler);
							if (!string.IsNullOrEmpty(guild.Description))
							{
								appendInterpolatedStringHandler = new StringBuilder.AppendInterpolatedStringHandler(0, 1, hoverText);
								appendInterpolatedStringHandler.AppendFormatted(guild.Description);
								hoverText.AppendLine(ref appendInterpolatedStringHandler);
							}
							IPlayer player2 = this.clientApi.World.AllOnlinePlayers.FirstOrDefault((IPlayer p) => p.PlayerUID == claim.ClaimedByUid);
							string claimerName = ((player2 != null) ? player2.PlayerName : null) ?? "Unknown";
							appendInterpolatedStringHandler = new StringBuilder.AppendInterpolatedStringHandler(12, 1, hoverText);
							appendInterpolatedStringHandler.AppendLiteral("Claimed by: ");
							appendInterpolatedStringHandler.AppendFormatted(claimerName);
							hoverText.AppendLine(ref appendInterpolatedStringHandler);
							appendInterpolatedStringHandler = new StringBuilder.AppendInterpolatedStringHandler(12, 1, hoverText);
							appendInterpolatedStringHandler.AppendLiteral("Claim Date: ");
							appendInterpolatedStringHandler.AppendFormatted(claim.Timestamp.ToString("yyyy-MM-dd"));
							hoverText.AppendLine(ref appendInterpolatedStringHandler);
							DialogGuildMain dialogGuildMain = this.activeGuildDialog;
							List<ValueTuple<int, int>> pendingUnclaims = ((dialogGuildMain != null) ? dialogGuildMain.GetPendingUnclaims() : null) ?? new List<ValueTuple<int, int>>();
							bool isPendingUnclaim = pendingUnclaims.Any(([TupleElementNames(new string[]
							{
								"chunkX",
								"chunkZ"
							})] ValueTuple<int, int> p) => p.Item1 == claim.ChunkX && p.Item2 == claim.ChunkZ);
							if (isPendingUnclaim)
							{
								hoverText.AppendLine("Status: PENDING UNCLAIM");
							}
							if (claim.IsGuildHome)
							{
								hoverText.AppendLine("Type: Guild Home Territory");
								if (claim.HomeCenterX != null && claim.HomeCenterZ != null)
								{
									appendInterpolatedStringHandler = new StringBuilder.AppendInterpolatedStringHandler(17, 2, hoverText);
									appendInterpolatedStringHandler.AppendLiteral("Home Center: (");
									appendInterpolatedStringHandler.AppendFormatted<int?>(claim.HomeCenterX);
									appendInterpolatedStringHandler.AppendLiteral(", ");
									appendInterpolatedStringHandler.AppendFormatted<int?>(claim.HomeCenterZ);
									appendInterpolatedStringHandler.AppendLiteral(")");
									hoverText.AppendLine(ref appendInterpolatedStringHandler);
									int offsetX = claim.ChunkX - claim.HomeCenterX.Value;
									int offsetZ = claim.ChunkZ - claim.HomeCenterZ.Value;
									string position = this.GetGuildHomeQuadrantName(offsetX, offsetZ);
									appendInterpolatedStringHandler = new StringBuilder.AppendInterpolatedStringHandler(15, 1, hoverText);
									appendInterpolatedStringHandler.AppendLiteral("Home Quadrant: ");
									appendInterpolatedStringHandler.AppendFormatted(position);
									hoverText.AppendLine(ref appendInterpolatedStringHandler);
								}
							}
							DialogGuildMain dialogGuildMain2 = this.activeGuildDialog;
							if (dialogGuildMain2 != null && dialogGuildMain2.IsUnclaimingModeActive)
							{
								GuildSummary currentGuild = guildSummaries.FirstOrDefault((GuildSummary g) => g.IsPlayerMember);
								if (currentGuild != null && currentGuild.Name == guild.Name)
								{
									if (claim.IsGuildHome)
									{
										int nonGuildHomeClaims = currentGuild.Claims.Count((LandClaimDto c) => !c.IsGuildHome);
										int pendingNonGuildHomeUnclaims = 0;
										using (List<LandClaimDto>.Enumerator enumerator3 = currentGuild.Claims.GetEnumerator())
										{
											while (enumerator3.MoveNext())
											{
												LandClaimDto c = enumerator3.Current;
												if (!c.IsGuildHome && pendingUnclaims.Any(([TupleElementNames(new string[]
												{
													"chunkX",
													"chunkZ"
												})] ValueTuple<int, int> p) => p.Item1 == c.ChunkX && p.Item2 == c.ChunkZ))
												{
													pendingNonGuildHomeUnclaims++;
												}
											}
										}
										int remainingNonGuildHomeClaims = nonGuildHomeClaims - pendingNonGuildHomeUnclaims;
										if (remainingNonGuildHomeClaims > 0)
										{
											appendInterpolatedStringHandler = new StringBuilder.AppendInterpolatedStringHandler(58, 1, hoverText);
											appendInterpolatedStringHandler.AppendLiteral("Cannot unclaim: Guild Home (unclaim other ");
											appendInterpolatedStringHandler.AppendFormatted<int>(remainingNonGuildHomeClaims);
											appendInterpolatedStringHandler.AppendLiteral(" claim(s) first)");
											hoverText.AppendLine(ref appendInterpolatedStringHandler);
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
										int nonOutpostNonGuildHomeClaims = currentGuild.Claims.Count((LandClaimDto c) => !c.IsOutpost && !c.IsGuildHome);
										int pendingNonOutpostNonGuildHomeUnclaims = 0;
										using (List<LandClaimDto>.Enumerator enumerator3 = currentGuild.Claims.GetEnumerator())
										{
											while (enumerator3.MoveNext())
											{
												LandClaimDto c = enumerator3.Current;
												if (!c.IsOutpost && !c.IsGuildHome && pendingUnclaims.Any(([TupleElementNames(new string[]
												{
													"chunkX",
													"chunkZ"
												})] ValueTuple<int, int> p) => p.Item1 == c.ChunkX && p.Item2 == c.ChunkZ))
												{
													pendingNonOutpostNonGuildHomeUnclaims++;
												}
											}
										}
										int remainingNonOutpostNonGuildHomeClaims = nonOutpostNonGuildHomeClaims - pendingNonOutpostNonGuildHomeUnclaims;
										if (remainingNonOutpostNonGuildHomeClaims > 0)
										{
											appendInterpolatedStringHandler = new StringBuilder.AppendInterpolatedStringHandler(55, 1, hoverText);
											appendInterpolatedStringHandler.AppendLiteral("Cannot unclaim: Outpost (unclaim other ");
											appendInterpolatedStringHandler.AppendFormatted<int>(remainingNonOutpostNonGuildHomeClaims);
											appendInterpolatedStringHandler.AppendLiteral(" claim(s) first)");
											hoverText.AppendLine(ref appendInterpolatedStringHandler);
										}
										else if (isPendingUnclaim)
										{
											hoverText.AppendLine("Already marked for unclaim");
										}
										else
										{
											int outpostChunkCount = currentGuild.Claims.Count((LandClaimDto c) => c.IsOutpost && c.OutpostName == claim.OutpostName);
											appendInterpolatedStringHandler = new StringBuilder.AppendInterpolatedStringHandler(50, 1, hoverText);
											appendInterpolatedStringHandler.AppendLiteral("Left-click to unclaim all outpost chunks (");
											appendInterpolatedStringHandler.AppendFormatted<int>(outpostChunkCount);
											appendInterpolatedStringHandler.AppendLiteral(" chunks)");
											hoverText.AppendLine(ref appendInterpolatedStringHandler);
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
							return;
						}
					}
				}
			}
			BlockPos asBlockPos = this.clientApi.World.Player.Entity.Pos.AsBlockPos;
			int centerChunkX = LandClaim.FloorDiv(asBlockPos.X, chunkSize);
			int centerChunkZ = LandClaim.FloorDiv(asBlockPos.Z, chunkSize);
			ElementBounds mapBounds = mapElem.Bounds;
			double scale = (double)mapElem.ZoomLevel;
			int viewRadius = Math.Max(1, (int)(Math.Max(mapBounds.InnerWidth, mapBounds.InnerHeight) / ((double)chunkSize * scale))) + 2;
			for (int dx = -viewRadius; dx <= viewRadius; dx++)
			{
				int dz = -viewRadius;
				while (dz <= viewRadius)
				{
					int chunkX = centerChunkX + dx;
					int chunkZ = centerChunkZ + dz;
					Vec2f viewPos2 = new Vec2f();
					mapElem.TranslateWorldPosToViewPos(new Vec3d((double)(chunkX * chunkSize + chunkSize / 2), 0.0, (double)(chunkZ * chunkSize + chunkSize / 2)), ref viewPos2);
					double chunkViewSize2 = (double)((float)chunkSize * mapElem.ZoomLevel);
					if (Math.Abs((double)viewPos2.X - mouseX) < chunkViewSize2 / 2.0 && Math.Abs((double)viewPos2.Y - mouseY) < chunkViewSize2 / 2.0)
					{
						this.hoveredChunk = new ValueTuple<int, int>?(new ValueTuple<int, int>(chunkX, chunkZ));
						DialogGuildMain dialogGuildMain3 = this.activeGuildDialog;
						List<ValueTuple<int, int>> pendingClaims = ((dialogGuildMain3 != null) ? dialogGuildMain3.GetPendingClaims() : null) ?? new List<ValueTuple<int, int>>();
						DialogGuildMain dialogGuildMain4 = this.activeGuildDialog;
						IEnumerable<ValueTuple<int, int>> source = ((dialogGuildMain4 != null) ? dialogGuildMain4.GetPendingUnclaims() : null) ?? new List<ValueTuple<int, int>>();
						bool isPending = pendingClaims.Any(([TupleElementNames(new string[]
						{
							"chunkX",
							"chunkZ"
						})] ValueTuple<int, int> p) => p.Item1 == chunkX && p.Item2 == chunkZ);
						source.Any(([TupleElementNames(new string[]
						{
							"chunkX",
							"chunkZ"
						})] ValueTuple<int, int> p) => p.Item1 == chunkX && p.Item2 == chunkZ);
						bool isWithinTerritorialBounds = this.IsChunkWithinTerritorialBounds(chunkX, chunkZ);
						StringBuilder.AppendInterpolatedStringHandler appendInterpolatedStringHandler;
						if (!isPending)
						{
							appendInterpolatedStringHandler = new StringBuilder.AppendInterpolatedStringHandler(11, 2, hoverText);
							appendInterpolatedStringHandler.AppendLiteral("Chunk: (");
							appendInterpolatedStringHandler.AppendFormatted<int>(chunkX);
							appendInterpolatedStringHandler.AppendLiteral(", ");
							appendInterpolatedStringHandler.AppendFormatted<int>(chunkZ);
							appendInterpolatedStringHandler.AppendLiteral(")");
							hoverText.AppendLine(ref appendInterpolatedStringHandler);
							hoverText.AppendLine("Status: Unclaimed");
							bool isInProtectedZone = this.IsChunkWithinProtectedZone(chunkX, chunkZ);
							if (this.protectedZonesEnabled && isInProtectedZone)
							{
								ValueTuple<string, int, int, int, List<string>>? zone = this.GetProtectedZoneAt(chunkX * 32 + 16, chunkZ * 32 + 16, this.clientApi.World.DefaultSpawnPosition.AsBlockPos);
								if (zone != null)
								{
									appendInterpolatedStringHandler = new StringBuilder.AppendInterpolatedStringHandler(16, 1, hoverText);
									appendInterpolatedStringHandler.AppendLiteral("Protected Zone: ");
									appendInterpolatedStringHandler.AppendFormatted(zone.Value.Item1);
									hoverText.AppendLine(ref appendInterpolatedStringHandler);
									hoverText.AppendLine("Cannot claim or break blocks here");
									double num = (double)(chunkX * 32 + 16);
									int blockZ = chunkZ * 32 + 16;
									Vec3i mapSize = this.clientApi.World.BlockAccessor.MapSize;
									double num2 = num - (double)zone.Value.Item2 - (double)(mapSize.X / 2);
									double deltaZ = (double)(blockZ - zone.Value.Item3 - mapSize.Z / 2);
									double distance = Math.Sqrt(num2 * num2 + deltaZ * deltaZ);
									appendInterpolatedStringHandler = new StringBuilder.AppendInterpolatedStringHandler(32, 2, hoverText);
									appendInterpolatedStringHandler.AppendLiteral("Distance from center: ");
									appendInterpolatedStringHandler.AppendFormatted<double>(distance, "F0");
									appendInterpolatedStringHandler.AppendLiteral(" / ");
									appendInterpolatedStringHandler.AppendFormatted<int>(zone.Value.Item4);
									appendInterpolatedStringHandler.AppendLiteral(" blocks");
									hoverText.AppendLine(ref appendInterpolatedStringHandler);
								}
							}
							if (this.IsChunkWithinNode(chunkX, chunkZ))
							{
								ValueTuple<string, int, int, int>? node = this.GetNodeAt(chunkX * 32 + 16, chunkZ * 32 + 16, this.clientApi.World.DefaultSpawnPosition.AsBlockPos);
								if (node != null)
								{
									appendInterpolatedStringHandler = new StringBuilder.AppendInterpolatedStringHandler(6, 1, hoverText);
									appendInterpolatedStringHandler.AppendLiteral("Node: ");
									appendInterpolatedStringHandler.AppendFormatted(node.Value.Item1);
									hoverText.AppendLine(ref appendInterpolatedStringHandler);
									double num3 = (double)(chunkX * 32 + 16);
									int blockZ2 = chunkZ * 32 + 16;
									Vec3i mapSize2 = this.clientApi.World.BlockAccessor.MapSize;
									double num4 = num3 - (double)node.Value.Item2 - (double)(mapSize2.X / 2);
									double deltaZ2 = (double)(blockZ2 - node.Value.Item3 - mapSize2.Z / 2);
									double distance2 = Math.Sqrt(num4 * num4 + deltaZ2 * deltaZ2);
									appendInterpolatedStringHandler = new StringBuilder.AppendInterpolatedStringHandler(32, 2, hoverText);
									appendInterpolatedStringHandler.AppendLiteral("Distance from center: ");
									appendInterpolatedStringHandler.AppendFormatted<double>(distance2, "F0");
									appendInterpolatedStringHandler.AppendLiteral(" / ");
									appendInterpolatedStringHandler.AppendFormatted<int>(node.Value.Item4);
									appendInterpolatedStringHandler.AppendLiteral(" blocks");
									hoverText.AppendLine(ref appendInterpolatedStringHandler);
								}
							}
							GuildSummary currentGuild2 = guildSummaries.FirstOrDefault((GuildSummary g) => g.IsPlayerMember);
							Dictionary<Vec2i, ValueTuple<string, GuildSummary, LandClaimDto>> claimedChunks = new Dictionary<Vec2i, ValueTuple<string, GuildSummary, LandClaimDto>>();
							foreach (GuildSummary guild2 in guildSummaries)
							{
								foreach (LandClaimDto claim2 in guild2.Claims)
								{
									Vec2i chunkKey = new Vec2i(claim2.ChunkX, claim2.ChunkZ);
									claimedChunks[chunkKey] = new ValueTuple<string, GuildSummary, LandClaimDto>(guild2.Name, guild2, claim2);
								}
							}
							ValueTuple<bool, string, double> valueTuple = this.IsChunkTooCloseToOtherGuildClaim(chunkX, chunkZ, ((currentGuild2 != null) ? currentGuild2.Name : null) ?? "", claimedChunks);
							bool tooClose = valueTuple.Item1;
							string nearestGuild = valueTuple.Item2;
							double distanceToNearest = valueTuple.Item3;
							if (tooClose && !string.IsNullOrEmpty(nearestGuild))
							{
								appendInterpolatedStringHandler = new StringBuilder.AppendInterpolatedStringHandler(26, 1, hoverText);
								appendInterpolatedStringHandler.AppendLiteral("TOO CLOSE to ");
								appendInterpolatedStringHandler.AppendFormatted(nearestGuild);
								appendInterpolatedStringHandler.AppendLiteral("'s territory!");
								hoverText.AppendLine(ref appendInterpolatedStringHandler);
								appendInterpolatedStringHandler = new StringBuilder.AppendInterpolatedStringHandler(28, 1, hoverText);
								appendInterpolatedStringHandler.AppendLiteral("Distance: ");
								appendInterpolatedStringHandler.AppendFormatted<double>(distanceToNearest, "F0");
								appendInterpolatedStringHandler.AppendLiteral(" blocks (min: 300)");
								hoverText.AppendLine(ref appendInterpolatedStringHandler);
								hoverText.AppendLine("Cannot claim within 300 blocks of another guild");
							}
							else if (!string.IsNullOrEmpty(nearestGuild) && distanceToNearest < 600.0)
							{
								appendInterpolatedStringHandler = new StringBuilder.AppendInterpolatedStringHandler(26, 2, hoverText);
								appendInterpolatedStringHandler.AppendLiteral("Near ");
								appendInterpolatedStringHandler.AppendFormatted(nearestGuild);
								appendInterpolatedStringHandler.AppendLiteral("'s territory: ");
								appendInterpolatedStringHandler.AppendFormatted<double>(distanceToNearest, "F0");
								appendInterpolatedStringHandler.AppendLiteral(" blocks");
								hoverText.AppendLine(ref appendInterpolatedStringHandler);
							}
							if (this.territorialRestrictionsEnabled)
							{
								if (isWithinTerritorialBounds)
								{
									hoverText.AppendLine("Territory: Allowed claiming area");
								}
								else
								{
									hoverText.AppendLine("Territory: RESTRICTED - Outside claiming zone");
									if (this.territorialCenter != null)
									{
										double num5 = (double)(chunkX * 32 + 16);
										int blockZ3 = chunkZ * 32 + 16;
										Vec3i mapSize3 = this.clientApi.World.BlockAccessor.MapSize;
										double num6 = num5 - (double)this.territorialCenter.Value.Item1 - (double)(mapSize3.X / 2);
										double deltaZ3 = (double)(blockZ3 - this.territorialCenter.Value.Item2 - mapSize3.Z / 2);
										double distance3 = Math.Sqrt(num6 * num6 + deltaZ3 * deltaZ3);
										appendInterpolatedStringHandler = new StringBuilder.AppendInterpolatedStringHandler(34, 3, hoverText);
										appendInterpolatedStringHandler.AppendLiteral("Distance from center (");
										appendInterpolatedStringHandler.AppendFormatted<int>(this.territorialCenter.Value.Item1);
										appendInterpolatedStringHandler.AppendLiteral(", ");
										appendInterpolatedStringHandler.AppendFormatted<int>(this.territorialCenter.Value.Item2);
										appendInterpolatedStringHandler.AppendLiteral("): ");
										appendInterpolatedStringHandler.AppendFormatted<double>(distance3, "F0");
										appendInterpolatedStringHandler.AppendLiteral(" blocks");
										hoverText.AppendLine(ref appendInterpolatedStringHandler);
										appendInterpolatedStringHandler = new StringBuilder.AppendInterpolatedStringHandler(29, 1, hoverText);
										appendInterpolatedStringHandler.AppendLiteral("Max allowed distance: ");
										appendInterpolatedStringHandler.AppendFormatted<int>(this.territorialRadius);
										appendInterpolatedStringHandler.AppendLiteral(" blocks");
										hoverText.AppendLine(ref appendInterpolatedStringHandler);
									}
								}
							}
							DialogGuildMain dialogGuildMain5 = this.activeGuildDialog;
							if (dialogGuildMain5 != null && dialogGuildMain5.IsClaimingModeActive)
							{
								if (isInProtectedZone && this.protectedZonesEnabled)
								{
									hoverText.AppendLine("Cannot claim - Protected zone");
									return;
								}
								if (tooClose)
								{
									hoverText.AppendLine("Cannot claim - Too close to another guild");
									return;
								}
								if (isWithinTerritorialBounds)
								{
									GuildSummary currentGuildForHome = guildSummaries.FirstOrDefault((GuildSummary g) => g.IsPlayerMember);
									bool flag3;
									if (currentGuildForHome != null)
									{
										if (!currentGuildForHome.Claims.Any((LandClaimDto c) => c.IsGuildHome))
										{
											flag3 = (pendingClaims.Count == 0);
											goto IL_11F5;
										}
									}
									flag3 = false;
									IL_11F5:
									if (flag3)
									{
										hoverText.AppendLine("Left-click to establish Guild Home (2x2)");
										hoverText.AppendLine("Will claim this chunk + 3 adjacent chunks");
										return;
									}
									hoverText.AppendLine("Left-click to claim");
									return;
								}
								else if (this.territorialRestrictionsEnabled)
								{
									hoverText.AppendLine("Cannot claim - Outside allowed area");
								}
							}
							return;
						}
						appendInterpolatedStringHandler = new StringBuilder.AppendInterpolatedStringHandler(11, 2, hoverText);
						appendInterpolatedStringHandler.AppendLiteral("Chunk: (");
						appendInterpolatedStringHandler.AppendFormatted<int>(chunkX);
						appendInterpolatedStringHandler.AppendLiteral(", ");
						appendInterpolatedStringHandler.AppendFormatted<int>(chunkZ);
						appendInterpolatedStringHandler.AppendLiteral(")");
						hoverText.AppendLine(ref appendInterpolatedStringHandler);
						DialogGuildMain dialogGuildMain6 = this.activeGuildDialog;
						if (dialogGuildMain6 != null && dialogGuildMain6.IsPendingGuildHomeChunk(chunkX, chunkZ))
						{
							hoverText.AppendLine("Status: Pending Guild Home");
							hoverText.AppendLine("This will become your guild's home base (2x2 area)");
							return;
						}
						hoverText.AppendLine("Status: Pending Claim");
						return;
					}
					else
					{
						dz++;
					}
				}
			}
		}

		// Token: 0x060005D0 RID: 1488 RVA: 0x00029E8C File Offset: 0x0002808C
		private string GetGuildHomeQuadrantName(int offsetX, int offsetZ)
		{
			if (offsetX != 0)
			{
				if (offsetX == 1)
				{
					if (offsetZ == 0)
					{
						return "Southeast";
					}
					if (offsetZ == 1)
					{
						return "Northeast";
					}
				}
			}
			else
			{
				if (offsetZ == 0)
				{
					return "Southwest";
				}
				if (offsetZ == 1)
				{
					return "Northwest";
				}
			}
			return "Unknown";
		}

		// Token: 0x060005D1 RID: 1489 RVA: 0x00029EDC File Offset: 0x000280DC
		public override void OnMouseUpClient(MouseEvent args, GuiElementMap mapElem)
		{
			int chunkSize = 32;
			double mouseX = (double)args.X - mapElem.Bounds.renderX;
			double mouseY = (double)args.Y - mapElem.Bounds.renderY;
			SRGuildsAndKingdomsModSystem srguildsAndKingdomsModSystem = this.modSystem;
			List<GuildSummary> guildSummaries = ((srguildsAndKingdomsModSystem != null) ? srguildsAndKingdomsModSystem.GetClientGuildSummaries() : null) ?? new List<GuildSummary>();
			GuildSummary currentGuild = guildSummaries.FirstOrDefault((GuildSummary g) => g.IsPlayerMember);
			if (args.Button == null)
			{
				DialogGuildMain dialogGuildMain = this.activeGuildDialog;
				if (dialogGuildMain != null && dialogGuildMain.IsUnclaimingModeActive)
				{
					if (currentGuild != null)
					{
						using (List<LandClaimDto>.Enumerator enumerator = currentGuild.Claims.GetEnumerator())
						{
							while (enumerator.MoveNext())
							{
								LandClaimDto claim = enumerator.Current;
								Vec2f viewPos = new Vec2f();
								mapElem.TranslateWorldPosToViewPos(new Vec3d((double)(claim.ChunkX * chunkSize + chunkSize / 2), 0.0, (double)(claim.ChunkZ * chunkSize + chunkSize / 2)), ref viewPos);
								double chunkViewSize = (double)((float)chunkSize * mapElem.ZoomLevel);
								if (Math.Abs((double)viewPos.X - mouseX) < chunkViewSize / 2.0 && Math.Abs((double)viewPos.Y - mouseY) < chunkViewSize / 2.0)
								{
									DialogGuildMain dialogGuildMain2 = this.activeGuildDialog;
									List<ValueTuple<int, int>> pendingUnclaims = ((dialogGuildMain2 != null) ? dialogGuildMain2.GetPendingUnclaims() : null) ?? new List<ValueTuple<int, int>>();
									if (claim.IsGuildHome && claim.HomeCenterX != null && claim.HomeCenterZ != null)
									{
										int nonGuildHomeClaims = currentGuild.Claims.Count((LandClaimDto c) => !c.IsGuildHome);
										int pendingNonGuildHomeUnclaims = 0;
										using (List<LandClaimDto>.Enumerator enumerator2 = currentGuild.Claims.GetEnumerator())
										{
											while (enumerator2.MoveNext())
											{
												LandClaimDto c = enumerator2.Current;
												if (!c.IsGuildHome && pendingUnclaims.Any(([TupleElementNames(new string[]
												{
													"chunkX",
													"chunkZ"
												})] ValueTuple<int, int> p) => p.Item1 == c.ChunkX && p.Item2 == c.ChunkZ))
												{
													pendingNonGuildHomeUnclaims++;
												}
											}
										}
										int remainingNonGuildHomeClaims = nonGuildHomeClaims - pendingNonGuildHomeUnclaims;
										if (remainingNonGuildHomeClaims > 0)
										{
											ICoreClientAPI coreClientAPI = this.clientApi;
											if (coreClientAPI == null)
											{
												return;
											}
											DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(101, 1);
											defaultInterpolatedStringHandler.AppendLiteral("[Guild] Cannot unclaim guild home - You must unclaim all other territory first (");
											defaultInterpolatedStringHandler.AppendFormatted<int>(remainingNonGuildHomeClaims);
											defaultInterpolatedStringHandler.AppendLiteral(" claim(s) remaining).");
											coreClientAPI.ShowChatMessage(defaultInterpolatedStringHandler.ToStringAndClear());
											return;
										}
										else
										{
											int centerX = claim.HomeCenterX.Value;
											int centerZ = claim.HomeCenterZ.Value;
											this.activeGuildDialog.OnMapChunkUnclaimed(centerX, centerZ);
											this.activeGuildDialog.OnMapChunkUnclaimed(centerX + 1, centerZ);
											this.activeGuildDialog.OnMapChunkUnclaimed(centerX, centerZ + 1);
											this.activeGuildDialog.OnMapChunkUnclaimed(centerX + 1, centerZ + 1);
											ICoreClientAPI coreClientAPI2 = this.clientApi;
											if (coreClientAPI2 == null)
											{
												return;
											}
											coreClientAPI2.ShowChatMessage("[Guild] Marking all guild home chunks for unclaim...");
											return;
										}
									}
									else
									{
										if (!claim.IsOutpost || string.IsNullOrEmpty(claim.OutpostName))
										{
											this.activeGuildDialog.OnMapChunkUnclaimed(claim.ChunkX, claim.ChunkZ);
											return;
										}
										int nonOutpostNonGuildHomeClaims = currentGuild.Claims.Count((LandClaimDto c) => !c.IsOutpost && !c.IsGuildHome);
										int pendingNonOutpostNonGuildHomeUnclaims = 0;
										using (List<LandClaimDto>.Enumerator enumerator2 = currentGuild.Claims.GetEnumerator())
										{
											while (enumerator2.MoveNext())
											{
												LandClaimDto c = enumerator2.Current;
												if (!c.IsOutpost && !c.IsGuildHome && pendingUnclaims.Any(([TupleElementNames(new string[]
												{
													"chunkX",
													"chunkZ"
												})] ValueTuple<int, int> p) => p.Item1 == c.ChunkX && p.Item2 == c.ChunkZ))
												{
													pendingNonOutpostNonGuildHomeUnclaims++;
												}
											}
										}
										int remainingNonOutpostNonGuildHomeClaims = nonOutpostNonGuildHomeClaims - pendingNonOutpostNonGuildHomeUnclaims;
										if (remainingNonOutpostNonGuildHomeClaims > 0)
										{
											ICoreClientAPI coreClientAPI3 = this.clientApi;
											if (coreClientAPI3 == null)
											{
												return;
											}
											DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(100, 1);
											defaultInterpolatedStringHandler2.AppendLiteral("[Guild] Cannot unclaim outpost - You must unclaim all regular territory first (");
											defaultInterpolatedStringHandler2.AppendFormatted<int>(remainingNonOutpostNonGuildHomeClaims);
											defaultInterpolatedStringHandler2.AppendLiteral(" claim(s) remaining).");
											coreClientAPI3.ShowChatMessage(defaultInterpolatedStringHandler2.ToStringAndClear());
											return;
										}
										else
										{
											List<LandClaimDto> outpostChunks = (from c in currentGuild.Claims
											where c.IsOutpost && c.OutpostName == claim.OutpostName
											select c).ToList<LandClaimDto>();
											foreach (LandClaimDto outpostChunk in outpostChunks)
											{
												this.activeGuildDialog.OnMapChunkUnclaimed(outpostChunk.ChunkX, outpostChunk.ChunkZ);
											}
											ICoreClientAPI coreClientAPI4 = this.clientApi;
											if (coreClientAPI4 == null)
											{
												return;
											}
											DefaultInterpolatedStringHandler defaultInterpolatedStringHandler3 = new DefaultInterpolatedStringHandler(60, 2);
											defaultInterpolatedStringHandler3.AppendLiteral("[Guild] Marking all ");
											defaultInterpolatedStringHandler3.AppendFormatted(claim.OutpostName);
											defaultInterpolatedStringHandler3.AppendLiteral(" outpost chunks for unclaim (");
											defaultInterpolatedStringHandler3.AppendFormatted<int>(outpostChunks.Count);
											defaultInterpolatedStringHandler3.AppendLiteral(" chunks)...");
											coreClientAPI4.ShowChatMessage(defaultInterpolatedStringHandler3.ToStringAndClear());
											return;
										}
									}
								}
							}
						}
					}
					ICoreClientAPI coreClientAPI5 = this.clientApi;
					if (coreClientAPI5 == null)
					{
						return;
					}
					coreClientAPI5.ShowChatMessage("[Guild] You can only unclaim chunks owned by your guild.");
					return;
				}
			}
			if (args.Button == null)
			{
				DialogGuildMain dialogGuildMain3 = this.activeGuildDialog;
				if (dialogGuildMain3 != null && dialogGuildMain3.IsClaimingModeActive)
				{
					foreach (GuildSummary guildSummary in guildSummaries)
					{
						foreach (LandClaimDto claim3 in guildSummary.Claims)
						{
							Vec2f viewPos2 = new Vec2f();
							mapElem.TranslateWorldPosToViewPos(new Vec3d((double)(claim3.ChunkX * chunkSize + chunkSize / 2), 0.0, (double)(claim3.ChunkZ * chunkSize + chunkSize / 2)), ref viewPos2);
							double chunkViewSize2 = (double)((float)chunkSize * mapElem.ZoomLevel);
							if (Math.Abs((double)viewPos2.X - mouseX) < chunkViewSize2 / 2.0 && Math.Abs((double)viewPos2.Y - mouseY) < chunkViewSize2 / 2.0)
							{
								return;
							}
						}
					}
					BlockPos asBlockPos = this.clientApi.World.Player.Entity.Pos.AsBlockPos;
					int centerChunkX = LandClaim.FloorDiv(asBlockPos.X, chunkSize);
					int centerChunkZ = LandClaim.FloorDiv(asBlockPos.Z, chunkSize);
					ElementBounds mapBounds = mapElem.Bounds;
					double scale = (double)mapElem.ZoomLevel;
					int viewRadius = Math.Max(1, (int)(Math.Max(mapBounds.InnerWidth, mapBounds.InnerHeight) / ((double)chunkSize * scale))) + 2;
					for (int dx = -viewRadius; dx <= viewRadius; dx++)
					{
						int dz = -viewRadius;
						while (dz <= viewRadius)
						{
							int chunkX = centerChunkX + dx;
							int chunkZ = centerChunkZ + dz;
							Vec2f viewPos3 = new Vec2f();
							mapElem.TranslateWorldPosToViewPos(new Vec3d((double)(chunkX * chunkSize + chunkSize / 2), 0.0, (double)(chunkZ * chunkSize + chunkSize / 2)), ref viewPos3);
							double chunkViewSize3 = (double)((float)chunkSize * mapElem.ZoomLevel);
							if (Math.Abs((double)viewPos3.X - mouseX) < chunkViewSize3 / 2.0 && Math.Abs((double)viewPos3.Y - mouseY) < chunkViewSize3 / 2.0)
							{
								if (this.territorialRestrictionsEnabled && !this.IsChunkWithinTerritorialBounds(chunkX, chunkZ))
								{
									this.clientApi.ShowChatMessage("[Guild] Cannot claim here - This area is outside the allowed claiming zone.");
									return;
								}
								Dictionary<Vec2i, ValueTuple<string, GuildSummary, LandClaimDto>> claimedChunksForCheck = new Dictionary<Vec2i, ValueTuple<string, GuildSummary, LandClaimDto>>();
								foreach (GuildSummary guild in guildSummaries)
								{
									foreach (LandClaimDto claim2 in guild.Claims)
									{
										Vec2i chunkKey = new Vec2i(claim2.ChunkX, claim2.ChunkZ);
										claimedChunksForCheck[chunkKey] = new ValueTuple<string, GuildSummary, LandClaimDto>(guild.Name, guild, claim2);
									}
								}
								ValueTuple<bool, string, double> valueTuple = this.IsChunkTooCloseToOtherGuildClaim(chunkX, chunkZ, ((currentGuild != null) ? currentGuild.Name : null) ?? "", claimedChunksForCheck);
								bool tooClose = valueTuple.Item1;
								string nearestGuild = valueTuple.Item2;
								double distance = valueTuple.Item3;
								if (tooClose)
								{
									ICoreClientAPI coreClientAPI6 = this.clientApi;
									DefaultInterpolatedStringHandler defaultInterpolatedStringHandler4 = new DefaultInterpolatedStringHandler(93, 2);
									defaultInterpolatedStringHandler4.AppendLiteral("[Guild] Cannot claim here - Too close to ");
									defaultInterpolatedStringHandler4.AppendFormatted(nearestGuild);
									defaultInterpolatedStringHandler4.AppendLiteral("'s territory (");
									defaultInterpolatedStringHandler4.AppendFormatted<double>(distance, "F0");
									defaultInterpolatedStringHandler4.AppendLiteral(" blocks, minimum 300 blocks required).");
									coreClientAPI6.ShowChatMessage(defaultInterpolatedStringHandler4.ToStringAndClear());
									return;
								}
								this.activeGuildDialog.OnMapChunkClaimed(chunkX, chunkZ);
								return;
							}
							else
							{
								dz++;
							}
						}
					}
				}
			}
			base.OnMouseUpClient(args, mapElem);
		}

		// Token: 0x060005D2 RID: 1490 RVA: 0x0002A8CC File Offset: 0x00028ACC
		public override void OnViewChangedClient(List<FastVec2i> nowVisible, List<FastVec2i> nowHidden)
		{
			foreach (FastVec2i hidden in nowHidden)
			{
				Vec2i key = new Vec2i(hidden.X, hidden.Y);
				this.chunkDataCache.Remove(key);
			}
		}

		// Token: 0x060005D3 RID: 1491 RVA: 0x0002A934 File Offset: 0x00028B34
		public override void Dispose()
		{
			Dictionary<Vec2i, ChunkData> dictionary = this.chunkDataCache;
			if (dictionary != null)
			{
				dictionary.Clear();
			}
			this.activeGuildDialog = null;
			base.Dispose();
		}

		// Token: 0x04000238 RID: 568
		private ICoreClientAPI clientApi;

		// Token: 0x04000239 RID: 569
		private Dictionary<Vec2i, ChunkData> chunkDataCache;

		// Token: 0x0400023A RID: 570
		private SRGuildsAndKingdomsModSystem modSystem;

		// Token: 0x0400023B RID: 571
		[Nullable(2)]
		private DialogGuildMain activeGuildDialog;

		// Token: 0x0400023C RID: 572
		[TupleElementNames(new string[]
		{
			"chunkX",
			"chunkZ"
		})]
		[Nullable(0)]
		private ValueTuple<int, int>? hoveredChunk;

		// Token: 0x0400023D RID: 573
		private int lastFrameChunkCount;

		// Token: 0x0400023E RID: 574
		private bool territorialRestrictionsEnabled;

		// Token: 0x0400023F RID: 575
		[TupleElementNames(new string[]
		{
			"x",
			"z"
		})]
		[Nullable(0)]
		private ValueTuple<int, int>? territorialCenter;

		// Token: 0x04000240 RID: 576
		private int territorialRadius = 1000;

		// Token: 0x04000241 RID: 577
		private long lastConfigUpdate;

		// Token: 0x04000242 RID: 578
		private bool protectedZonesEnabled;

		// Token: 0x04000243 RID: 579
		[TupleElementNames(new string[]
		{
			"name",
			"x",
			"z",
			"radius",
			"whitelistedPlayers"
		})]
		[Nullable(new byte[]
		{
			1,
			0,
			1,
			1,
			1
		})]
		private List<ValueTuple<string, int, int, int, List<string>>> protectedZones = new List<ValueTuple<string, int, int, int, List<string>>>();

		// Token: 0x04000244 RID: 580
		[TupleElementNames(new string[]
		{
			"name",
			"x",
			"z",
			"radius"
		})]
		[Nullable(new byte[]
		{
			1,
			0,
			1
		})]
		private List<ValueTuple<string, int, int, int>> nodes = new List<ValueTuple<string, int, int, int>>();

		// Token: 0x04000245 RID: 581
		private int? whiteTextureId;
	}
}
