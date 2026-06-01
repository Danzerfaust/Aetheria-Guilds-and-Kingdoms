using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using SRGuildsAndKingdoms.src.config;
using SRGuildsAndKingdoms.src.guilds;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;

namespace SRGuildsAndKingdoms.src.patches
{
	// Token: 0x02000025 RID: 37
	public class WorldMapTestBlockAccessPatch
	{
		// Token: 0x060001AB RID: 427 RVA: 0x000107A8 File Offset: 0x0000E9A8
		[NullableContext(1)]
		public static bool Prefix(IPlayer player, BlockSelection blockSel, EnumBlockAccessFlags accessType, out string claimant, ref EnumWorldAccessResponse __result)
		{
			claimant = "";
			__result = 0;
			bool flag;
			if (player == null)
			{
				flag = (null != null);
			}
			else
			{
				EntityPlayer entity = player.Entity;
				if (entity == null)
				{
					flag = (null != null);
				}
				else
				{
					IWorldAccessor world = entity.World;
					flag = (((world != null) ? world.Api : null) != null);
				}
			}
			if (!flag || ((blockSel != null) ? blockSel.Position : null) == null)
			{
				return true;
			}
			ICoreAPI api = player.Entity.World.Api;
			IModLoader modLoader = api.ModLoader;
			SRGuildsAndKingdomsModSystem modSystem = (modLoader != null) ? modLoader.GetModSystem<SRGuildsAndKingdomsModSystem>(true) : null;
			if (modSystem == null)
			{
				return true;
			}
			if (player != null)
			{
				IWorldPlayerData worldData = player.WorldData;
				if (((worldData != null) ? new EnumGameMode?(worldData.CurrentGameMode) : null).GetValueOrDefault() == 2)
				{
					return true;
				}
			}
			int chunkX = LandClaim.FloorDiv(blockSel.Position.X, 32);
			int chunkZ = LandClaim.FloorDiv(blockSel.Position.Z, 32);
			if (api.Side == 1)
			{
				IServerPlayer serverPlayer = player as IServerPlayer;
				if (serverPlayer != null)
				{
					GuildManager guildManager = modSystem.GetGuildManager();
					GuildConfig guildConfig;
					if (guildManager == null)
					{
						guildConfig = null;
					}
					else
					{
						GuildConfigManager configManager = guildManager.GetConfigManager();
						guildConfig = ((configManager != null) ? configManager.GetConfig() : null);
					}
					GuildConfig config = guildConfig;
					ICoreServerAPI coreServerAPI = api as ICoreServerAPI;
					BlockPos spawnPos = (coreServerAPI != null) ? coreServerAPI.World.DefaultSpawnPosition.AsBlockPos : null;
					if (config != null && spawnPos != null && config.IsWithinProtectedZone(blockSel.Position.X, blockSel.Position.Z, spawnPos))
					{
						ProtectedZone zone = config.GetProtectedZoneAt(blockSel.Position.X, blockSel.Position.Z, spawnPos);
						if (zone != null)
						{
							ZoneWhitelistManager zoneWhitelistManager = modSystem.GetZoneWhitelistManager();
							if (zoneWhitelistManager != null && zoneWhitelistManager.IsPlayerWhitelisted(zone.Id, serverPlayer.PlayerUID))
							{
								return true;
							}
						}
						if (zone != null)
						{
							if (accessType == 2)
							{
								return true;
							}
							if (accessType == 1)
							{
								claimant = zone.Name;
								serverPlayer.SendMessage(GlobalConstants.GeneralChatGroup, "You cannot build or break blocks in protected zone: " + zone.Name, 4, null);
								__result = 5;
								return false;
							}
						}
					}
					bool guildAllows = false;
					if (accessType == 2)
					{
						guildAllows = modSystem.CheckGuildUsePrivilege(serverPlayer, blockSel.Position);
					}
					else if (accessType == 1)
					{
						guildAllows = modSystem.CheckGuildBuildPrivilege(serverPlayer, blockSel.Position);
					}
					if (guildAllows)
					{
						return true;
					}
					MethodInfo method = modSystem.GetType().GetMethod("GetChunkOwningGuild", BindingFlags.Instance | BindingFlags.NonPublic);
					object owningGuild = (method != null) ? method.Invoke(modSystem, new object[]
					{
						chunkX,
						chunkZ
					}) : null;
					if (owningGuild == null)
					{
						return true;
					}
					PropertyInfo property = owningGuild.GetType().GetProperty("Name");
					string text;
					if (property == null)
					{
						text = null;
					}
					else
					{
						object value = property.GetValue(owningGuild);
						text = ((value != null) ? value.ToString() : null);
					}
					string guildName = text;
					if (!string.IsNullOrEmpty(guildName))
					{
						claimant = guildName;
						__result = 5;
						return false;
					}
					return true;
				}
			}
			if (api.Side == 2)
			{
				ValueTuple<bool, string, List<string>>? protectedZoneInfo = modSystem.CheckProtectedZone(blockSel.Position.X, blockSel.Position.Z);
				if (protectedZoneInfo != null && protectedZoneInfo.Value.Item1)
				{
					if (player != null && protectedZoneInfo.Value.Item3.Contains(player.PlayerUID))
					{
						return true;
					}
					if (accessType != 2 && accessType == 1)
					{
						claimant = protectedZoneInfo.Value.Item2;
						__result = 5;
						return false;
					}
				}
				GuildSummary owningGuild2 = modSystem.GetChunkOwner(chunkX, chunkZ);
				if (owningGuild2 != null)
				{
					claimant = owningGuild2.Name;
					if (!owningGuild2.IsPlayerMember)
					{
						__result = 5;
						return false;
					}
					bool hasPermission = false;
					if (accessType == 1)
					{
						hasPermission = owningGuild2.HasBreakPlacePermission;
					}
					else if (accessType == 2)
					{
						hasPermission = owningGuild2.HasInteractPermission;
					}
					if (!hasPermission)
					{
						__result = 5;
						return false;
					}
				}
			}
			return true;
		}
	}
}
