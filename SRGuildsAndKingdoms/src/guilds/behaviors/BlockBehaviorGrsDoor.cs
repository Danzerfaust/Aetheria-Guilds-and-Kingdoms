using System;
using System.Runtime.CompilerServices;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.GameContent;

namespace SRGuildsAndKingdoms.src.guilds.behaviors
{
	// Token: 0x020000AD RID: 173
	[NullableContext(1)]
	[Nullable(0)]
	public class BlockBehaviorGrsDoor : BlockBehavior
	{
		// Token: 0x060007FD RID: 2045 RVA: 0x00037C82 File Offset: 0x00035E82
		public BlockBehaviorGrsDoor(Block block) : base(block)
		{
		}

		// Token: 0x060007FE RID: 2046 RVA: 0x00037C8B File Offset: 0x00035E8B
		public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel, ref EnumHandling handling)
		{
			handling = 3;
			bool flag = this.HandleBlockInteract(world, byPlayer);
			if (flag)
			{
				base.OnBlockInteractStart(world, byPlayer, blockSel, ref handling);
			}
			return flag;
		}

		// Token: 0x060007FF RID: 2047 RVA: 0x00037CA8 File Offset: 0x00035EA8
		private bool HandleBlockInteract(IWorldAccessor world, IPlayer byPlayer)
		{
			CharacterSystem characterSystem = world.Api.ModLoader.GetModSystem<CharacterSystem>(true);
			if (characterSystem == null)
			{
				return true;
			}
			string rank = this.block.Variant["rank"];
			if (rank == null || rank == "")
			{
				return true;
			}
			if (characterSystem.HasTrait(byPlayer, "guild-rank-" + rank.ToUpper()))
			{
				return true;
			}
			if (world.Side == 2)
			{
				ICoreClientAPI coreClientAPI = world.Api as ICoreClientAPI;
				if (coreClientAPI != null)
				{
					coreClientAPI.TriggerIngameError(this, "locked", Lang.Get("srguildsandkingdoms:grsdoor-locked", Array.Empty<object>()));
				}
			}
			return false;
		}

		// Token: 0x04000347 RID: 839
		private const string RANK_VARIANT_NAME = "rank";
	}
}
