using System;
using System.Runtime.CompilerServices;
using SRGuildsAndKingdoms.src.gui.dialogs;
using SRGuildsAndKingdoms.src.guilds;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;

namespace SRGuildsAndKingdoms.src.quests.blocks
{
	// Token: 0x02000023 RID: 35
	[NullableContext(1)]
	[Nullable(0)]
	public class QuestBoardBlock : Block
	{
		// Token: 0x0600019C RID: 412 RVA: 0x0000FB7C File Offset: 0x0000DD7C
		public override void OnLoaded(ICoreAPI api)
		{
			base.OnLoaded(api);
			this.isWallBoard = (this.Variant["attachment"] == "wall");
			this.interactions = ObjectCacheUtil.GetOrCreate<WorldInteraction[]>(api, "questBoardInteraction-" + this.Variant["questType"], () => new WorldInteraction[]
			{
				new WorldInteraction
				{
					ActionLangCode = "srguildsandkingdoms:quests-view-" + this.Variant["questType"],
					MouseButton = 2
				}
			});
		}

		// Token: 0x0600019D RID: 413 RVA: 0x0000FBE2 File Offset: 0x0000DDE2
		public override WorldInteraction[] GetPlacedBlockInteractionHelp(IWorldAccessor world, BlockSelection selection, IPlayer forPlayer)
		{
			return ArrayExtensions.Append<WorldInteraction>(this.interactions, base.GetPlacedBlockInteractionHelp(world, selection, forPlayer));
		}

		// Token: 0x0600019E RID: 414 RVA: 0x0000FBF8 File Offset: 0x0000DDF8
		public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
		{
			if (world.Side != 2)
			{
				return base.OnBlockInteractStart(world, byPlayer, blockSel);
			}
			ICoreClientAPI capi = world.Api as ICoreClientAPI;
			if (capi == null)
			{
				return false;
			}
			bool isWeeklyBoard = this.Variant["questType"] == "daily-weekly";
			SRGuildsAndKingdomsModSystem modSystem = capi.ModLoader.GetModSystem<SRGuildsAndKingdomsModSystem>(true);
			if (modSystem == null)
			{
				return false;
			}
			if (isWeeklyBoard)
			{
				int chunkX = LandClaim.FloorDiv(blockSel.Position.X, 32);
				int chunkZ = LandClaim.FloorDiv(blockSel.Position.Z, 32);
				GuildSummary owningGuild = modSystem.GetChunkOwner(chunkX, chunkZ);
				if (owningGuild == null)
				{
					capi.ShowChatMessage(Lang.Get("srguildsandkingdoms:quest-board-not-in-claim", Array.Empty<object>()));
					return false;
				}
				if (!owningGuild.HasInteractPermission)
				{
					capi.ShowChatMessage(Lang.Get("srguildsandkingdoms:quest-board-wrong-claim", Array.Empty<object>()));
					return false;
				}
			}
			QuestDialog.CloseCurrentDialog();
			new QuestDialog(capi, modSystem, this.Variant["questType"]).TryOpen();
			return true;
		}

		// Token: 0x0600019F RID: 415 RVA: 0x0000FCEC File Offset: 0x0000DEEC
		public override bool TryPlaceBlock(IWorldAccessor world, IPlayer byPlayer, ItemStack itemstack, BlockSelection blockSel, ref string failureCode)
		{
			BlockPos supportingPos = blockSel.Position.AddCopy(blockSel.Face.Opposite);
			Block supportingBlock = world.BlockAccessor.GetBlock(supportingPos);
			if (blockSel.Face.IsHorizontal)
			{
				if (!supportingBlock.CanAttachBlockAt(world.BlockAccessor, this, supportingPos, blockSel.Face, null))
				{
					JsonObject attributes = supportingBlock.GetAttributes(world.BlockAccessor, supportingPos);
					if (attributes == null || !attributes.IsTrue("partialAttachable"))
					{
						goto IL_D0;
					}
				}
				Block wallblock = world.BlockAccessor.GetBlock(base.CodeWithParts(new string[]
				{
					"wall",
					blockSel.Face.Opposite.Code
				}));
				if (!wallblock.CanPlaceBlock(world, byPlayer, blockSel, ref failureCode))
				{
					return false;
				}
				world.BlockAccessor.SetBlock(wallblock.BlockId, blockSel.Position);
				return true;
			}
			IL_D0:
			if (!this.CanPlaceBlock(world, byPlayer, blockSel, ref failureCode))
			{
				return false;
			}
			BlockFacing[] horVer = Block.SuggestedHVOrientation(byPlayer, blockSel);
			AssetLocation blockCode = base.CodeWithParts(horVer[0].Code);
			Block block = world.BlockAccessor.GetBlock(blockCode);
			world.BlockAccessor.SetBlock(block.BlockId, blockSel.Position);
			BlockPos targetPos = blockSel.DidOffset ? blockSel.Position.AddCopy(blockSel.Face.Opposite) : blockSel.Position;
			double y = byPlayer.Entity.Pos.X - ((double)targetPos.X + blockSel.HitPosition.X);
			double dz = (double)((float)byPlayer.Entity.Pos.Z) - ((double)targetPos.Z + blockSel.HitPosition.Z);
			double num = (double)((float)Math.Atan2(y, dz));
			float deg45 = 0.7853982f;
			Math.Round(num / (double)deg45);
			return true;
		}

		// Token: 0x060001A0 RID: 416 RVA: 0x0000FEB0 File Offset: 0x0000E0B0
		public override void OnBlockBroken(IWorldAccessor world, BlockPos pos, [Nullable(2)] IPlayer byPlayer, float dropQuantityMultiplier = 1f)
		{
			if (world.Side == 1)
			{
				if (byPlayer != null && byPlayer.WorldData.CurrentGameMode == 2)
				{
					base.OnBlockBroken(world, pos, byPlayer, dropQuantityMultiplier);
					return;
				}
				Block block = world.BlockAccessor.GetBlock(base.CodeWithParts(new string[]
				{
					"ground",
					"north"
				}));
				if (block == null)
				{
					block = world.BlockAccessor.GetBlock(base.CodeWithParts(new string[]
					{
						"wall",
						"north"
					}));
				}
				ItemStack[] dropStacks = new ItemStack[]
				{
					new ItemStack(block, 1)
				};
				if (dropStacks != null)
				{
					foreach (ItemStack stack in dropStacks)
					{
						if (stack != null)
						{
							world.SpawnItemEntity(stack, pos.ToVec3d().Add(0.5, 0.5, 0.5), null);
						}
					}
				}
			}
			base.SpawnBlockBrokenParticles(pos, byPlayer);
			world.BlockAccessor.SetBlock(0, pos);
			EnumAppSideExtensions.IsServer(world.Side);
		}

		// Token: 0x040000A2 RID: 162
		private WorldInteraction[] interactions = Array.Empty<WorldInteraction>();

		// Token: 0x040000A3 RID: 163
		protected bool isWallBoard;
	}
}
