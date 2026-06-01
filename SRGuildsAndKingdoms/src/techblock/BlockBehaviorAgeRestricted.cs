using System;
using System.Runtime.CompilerServices;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.API.Util;

namespace SRGuildsAndKingdoms.src.techblock
{
	// Token: 0x02000008 RID: 8
	[NullableContext(1)]
	[Nullable(0)]
	public class BlockBehaviorAgeRestricted : BlockBehavior
	{
		// Token: 0x06000079 RID: 121 RVA: 0x000098E2 File Offset: 0x00007AE2
		public BlockBehaviorAgeRestricted(Block block) : base(block)
		{
		}

		// Token: 0x0600007A RID: 122 RVA: 0x000098EC File Offset: 0x00007AEC
		public override void Initialize(JsonObject properties)
		{
			base.Initialize(properties);
			if (!Enum.TryParse<TechAge>(properties["requiredAge"].AsString("Stone"), true, out this.requiredAge))
			{
				this.requiredAge = TechAge.Stone;
			}
			this.requiredTrait = properties["requiredTrait"].AsString(null);
		}

		// Token: 0x0600007B RID: 123 RVA: 0x00009941 File Offset: 0x00007B41
		public override void OnLoaded(ICoreAPI api)
		{
			base.OnLoaded(api);
			this.api = api;
		}

		// Token: 0x0600007C RID: 124 RVA: 0x00009954 File Offset: 0x00007B54
		private bool PlayerHasRequiredTrait(IPlayer player)
		{
			if (string.IsNullOrWhiteSpace(this.requiredTrait))
			{
				return true;
			}
			SyncedTreeAttribute syncedTreeAttribute;
			if (player == null)
			{
				syncedTreeAttribute = null;
			}
			else
			{
				EntityPlayer entity = player.Entity;
				syncedTreeAttribute = ((entity != null) ? entity.WatchedAttributes : null);
			}
			SyncedTreeAttribute watchedAttributes = syncedTreeAttribute;
			if (watchedAttributes == null)
			{
				return false;
			}
			string[] extraTraits = watchedAttributes.GetStringArray("extraTraits", null);
			return extraTraits != null && extraTraits.Length != 0 && ArrayExtensions.Contains<string>(extraTraits, this.requiredTrait);
		}

		// Token: 0x0600007D RID: 125 RVA: 0x000099B0 File Offset: 0x00007BB0
		public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel, ref EnumHandling handling)
		{
			if (byPlayer != null)
			{
				IWorldPlayerData worldData = byPlayer.WorldData;
				if (((worldData != null) ? new EnumGameMode?(worldData.CurrentGameMode) : null).GetValueOrDefault() == 2)
				{
					return base.OnBlockInteractStart(world, byPlayer, blockSel, ref handling);
				}
			}
			ICoreAPI coreAPI = this.api;
			SRGuildsAndKingdomsModSystem modSystem = (coreAPI != null) ? coreAPI.ModLoader.GetModSystem<SRGuildsAndKingdomsModSystem>(true) : null;
			if (((modSystem != null) ? modSystem.TechBlocksConfig : null) != null && !modSystem.TechBlocksConfig.IsAgeEnabled(this.requiredAge))
			{
				if (world.Side == 2)
				{
					ICoreClientAPI coreClientAPI = this.api as ICoreClientAPI;
					if (coreClientAPI != null)
					{
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(51, 1);
						defaultInterpolatedStringHandler.AppendLiteral("This ore cannot be mined until the ");
						defaultInterpolatedStringHandler.AppendFormatted<TechAge>(this.requiredAge);
						defaultInterpolatedStringHandler.AppendLiteral(" Age is enabled.");
						coreClientAPI.ShowChatMessage(defaultInterpolatedStringHandler.ToStringAndClear());
					}
				}
				handling = 2;
				return false;
			}
			if (!this.PlayerHasRequiredTrait(byPlayer))
			{
				if (world.Side == 2)
				{
					string message = (!string.IsNullOrWhiteSpace(this.requiredTrait)) ? ("Your guild hasn't researched the required technology (" + this.requiredTrait + ") to mine this resource.") : "Your guild hasn't researched the required technology to mine this resource.";
					ICoreClientAPI coreClientAPI2 = this.api as ICoreClientAPI;
					if (coreClientAPI2 != null)
					{
						coreClientAPI2.ShowChatMessage(message);
					}
				}
				else if (world.Side == 1)
				{
					IServerPlayer serverPlayer = byPlayer as IServerPlayer;
					if (serverPlayer != null)
					{
						string message2 = (!string.IsNullOrWhiteSpace(this.requiredTrait)) ? ("Your guild hasn't researched the required technology (" + this.requiredTrait + ") to mine this resource.") : "Your guild hasn't researched the required technology to mine this resource.";
						serverPlayer.SendIngameError("age_restricted", message2, Array.Empty<object>());
					}
				}
				handling = 2;
				return false;
			}
			return base.OnBlockInteractStart(world, byPlayer, blockSel, ref handling);
		}

		// Token: 0x0600007E RID: 126 RVA: 0x00009B48 File Offset: 0x00007D48
		public override void OnBlockBroken(IWorldAccessor world, BlockPos pos, IPlayer byPlayer, ref EnumHandling handling)
		{
			if (byPlayer != null)
			{
				IWorldPlayerData worldData = byPlayer.WorldData;
				if (((worldData != null) ? new EnumGameMode?(worldData.CurrentGameMode) : null).GetValueOrDefault() == 2)
				{
					base.OnBlockBroken(world, pos, byPlayer, ref handling);
					return;
				}
			}
			ICoreAPI coreAPI = this.api;
			SRGuildsAndKingdomsModSystem modSystem = (coreAPI != null) ? coreAPI.ModLoader.GetModSystem<SRGuildsAndKingdomsModSystem>(true) : null;
			if (((modSystem != null) ? modSystem.TechBlocksConfig : null) != null && !modSystem.TechBlocksConfig.IsAgeEnabled(this.requiredAge))
			{
				handling = 2;
				return;
			}
			if (!this.PlayerHasRequiredTrait(byPlayer))
			{
				handling = 2;
				return;
			}
			base.OnBlockBroken(world, pos, byPlayer, ref handling);
		}

		// Token: 0x0600007F RID: 127 RVA: 0x00009BE8 File Offset: 0x00007DE8
		public override bool OnBlockInteractStep(float secondsUsed, IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel, ref EnumHandling handling)
		{
			if (byPlayer != null)
			{
				IWorldPlayerData worldData = byPlayer.WorldData;
				if (((worldData != null) ? new EnumGameMode?(worldData.CurrentGameMode) : null).GetValueOrDefault() == 2)
				{
					return base.OnBlockInteractStep(secondsUsed, world, byPlayer, blockSel, ref handling);
				}
			}
			ICoreAPI coreAPI = this.api;
			SRGuildsAndKingdomsModSystem modSystem = (coreAPI != null) ? coreAPI.ModLoader.GetModSystem<SRGuildsAndKingdomsModSystem>(true) : null;
			if (((modSystem != null) ? modSystem.TechBlocksConfig : null) != null && !modSystem.TechBlocksConfig.IsAgeEnabled(this.requiredAge))
			{
				handling = 2;
				return false;
			}
			if (!this.PlayerHasRequiredTrait(byPlayer))
			{
				handling = 2;
				return false;
			}
			return base.OnBlockInteractStep(secondsUsed, world, byPlayer, blockSel, ref handling);
		}

		// Token: 0x04000024 RID: 36
		private TechAge requiredAge;

		// Token: 0x04000025 RID: 37
		private string requiredTrait;

		// Token: 0x04000026 RID: 38
		private ICoreAPI api;
	}
}
