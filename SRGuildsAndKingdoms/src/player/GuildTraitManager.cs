using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using SRGuildsAndKingdoms.src.config;
using SRGuildsAndKingdoms.src.guilds;
using SRGuildsAndKingdoms.src.techblock;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.Server;
using Vintagestory.GameContent;

namespace SRGuildsAndKingdoms.src.player
{
	// Token: 0x02000024 RID: 36
	[NullableContext(1)]
	[Nullable(0)]
	public class GuildTraitManager
	{
		// Token: 0x060001A3 RID: 419 RVA: 0x00010001 File Offset: 0x0000E201
		public GuildTraitManager(ICoreServerAPI api, SRGuildsAndKingdomsModSystem modSystem)
		{
			this.serverApi = api;
			this.modSystem = modSystem;
			this.characterSystem = api.ModLoader.GetModSystem<CharacterSystem>(true);
			CharacterSystem characterSystem = this.characterSystem;
		}

		// Token: 0x060001A4 RID: 420 RVA: 0x00010030 File Offset: 0x0000E230
		public void SyncPlayerTraits(IServerPlayer player)
		{
			this.serverApi.Logger.Debug("[GuildTraitManager] === Starting trait sync for player " + ((player != null) ? player.PlayerName : null) + " ===");
			if (this.characterSystem == null)
			{
				this.serverApi.Logger.Error("[GuildTraitManager] CharacterSystem is null! Cannot sync traits.");
				return;
			}
			if (player == null)
			{
				this.serverApi.Logger.Error("[GuildTraitManager] Player is null! Cannot sync traits.");
				return;
			}
			GuildManager guildManager = this.modSystem.GetGuildManager();
			if (guildManager == null)
			{
				this.serverApi.Logger.Error("[GuildTraitManager] GuildManager is null! Cannot sync traits.");
				return;
			}
			Guild guild = guildManager.GetGuildByMember(player.PlayerUID);
			if (guild == null)
			{
				this.RemoveAllGuildTraits(player);
				return;
			}
			HashSet<string> requiredTraits = this.GetRequiredTraitsForGuild(guild);
			HashSet<string> currentGuildTraits = this.GetPlayerGuildTraits(player);
			int grantedCount = 0;
			foreach (string trait in requiredTraits)
			{
				if (!currentGuildTraits.Contains(trait))
				{
					this.GrantTrait(player, trait);
					grantedCount++;
				}
			}
			int revokedCount = 0;
			foreach (string trait2 in currentGuildTraits)
			{
				if (!requiredTraits.Contains(trait2))
				{
					this.RevokeTrait(player, trait2);
					revokedCount++;
				}
			}
		}

		// Token: 0x060001A5 RID: 421 RVA: 0x00010198 File Offset: 0x0000E398
		public void SyncGuildMemberTraits(Guild guild)
		{
			if (guild == null)
			{
				return;
			}
			foreach (string memberUid in guild.Members.Keys)
			{
				IServerPlayer player = this.serverApi.World.PlayerByUid(memberUid) as IServerPlayer;
				if (player != null)
				{
					this.SyncPlayerTraits(player);
				}
			}
		}

		// Token: 0x060001A6 RID: 422 RVA: 0x00010210 File Offset: 0x0000E410
		private HashSet<string> GetRequiredTraitsForGuild(Guild guild)
		{
			HashSet<string> traits = new HashSet<string>();
			List<TechBlock> techBlocks = this.modSystem.TechBlocks;
			int unlockedCount = 0;
			using (Dictionary<int, GuildTechProgress>.ValueCollection.Enumerator enumerator = guild.TechProgress.Values.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					GuildTechProgress techProgress = enumerator.Current;
					TechBlock techBlock = techBlocks.FirstOrDefault((TechBlock tb) => tb.Id == techProgress.TechBlockId);
					if (((techBlock != null) ? techBlock.Text : null) == null)
					{
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(14, 1);
						defaultInterpolatedStringHandler.AppendLiteral("Unknown (ID: ");
						defaultInterpolatedStringHandler.AppendFormatted<int>(techProgress.TechBlockId);
						defaultInterpolatedStringHandler.AppendLiteral(")");
						defaultInterpolatedStringHandler.ToStringAndClear();
					}
					if (techProgress.IsUnlocked)
					{
						unlockedCount++;
						if (techBlock != null && techBlock.GrantedTraits != null && techBlock.GrantedTraits.Count != 0)
						{
							foreach (string trait in techBlock.GrantedTraits)
							{
								if (!string.IsNullOrWhiteSpace(trait))
								{
									traits.Add(trait);
								}
							}
						}
					}
				}
			}
			GuildManager guildManager = this.modSystem.GetGuildManager();
			GuildConfigManager guildConfigManager = (guildManager != null) ? guildManager.GetConfigManager() : null;
			string text;
			if (guildConfigManager == null)
			{
				text = null;
			}
			else
			{
				GuildConfig config = guildConfigManager.GetConfig();
				text = ((config != null) ? config.GetGuildRankClass(guild.Points) : null);
			}
			string guildRankClass = text;
			if (guildRankClass != null)
			{
				traits.Add("guild-rank-" + guildRankClass);
			}
			return traits;
		}

		// Token: 0x060001A7 RID: 423 RVA: 0x000103B4 File Offset: 0x0000E5B4
		private HashSet<string> GetPlayerGuildTraits(IServerPlayer player)
		{
			HashSet<string> guildTraits = new HashSet<string>();
			if (this.characterSystem == null)
			{
				return guildTraits;
			}
			foreach (TechBlock tech in this.modSystem.TechBlocks)
			{
				if (tech.GrantedTraits != null && tech.GrantedTraits.Count != 0)
				{
					foreach (string trait in tech.GrantedTraits)
					{
						if (this.characterSystem.HasTrait(player, trait))
						{
							guildTraits.Add(trait);
						}
					}
				}
			}
			GuildManager guildManager = this.modSystem.GetGuildManager();
			GuildConfigManager guildConfigManager = (guildManager != null) ? guildManager.GetConfigManager() : null;
			IEnumerable<string> enumerable;
			if (guildConfigManager == null)
			{
				enumerable = null;
			}
			else
			{
				GuildConfig config = guildConfigManager.GetConfig();
				if (config == null)
				{
					enumerable = null;
				}
				else
				{
					Dictionary<string, int> classThresholds2 = config.ClassThresholds;
					if (classThresholds2 == null)
					{
						enumerable = null;
					}
					else
					{
						enumerable = from x in classThresholds2
						select x.Key;
					}
				}
			}
			IEnumerable<string> classThresholds = enumerable;
			if (classThresholds != null && classThresholds.Any<string>())
			{
				foreach (string rankClass in classThresholds)
				{
					string rankTrait = "guild-rank-" + rankClass;
					if (this.characterSystem.HasTrait(player, rankTrait))
					{
						guildTraits.Add(rankTrait);
					}
				}
			}
			return guildTraits;
		}

		// Token: 0x060001A8 RID: 424 RVA: 0x00010544 File Offset: 0x0000E744
		private void GrantTrait(IServerPlayer player, string traitCode)
		{
			if (this.characterSystem == null)
			{
				return;
			}
			if (string.IsNullOrWhiteSpace(traitCode))
			{
				return;
			}
			try
			{
				if (!this.characterSystem.HasTrait(player, traitCode))
				{
					EntityPlayer playerEntity = player.Entity;
					if (playerEntity != null)
					{
						SyncedTreeAttribute watchedAttributes = playerEntity.WatchedAttributes;
						if (watchedAttributes != null)
						{
							string[] stringArray = watchedAttributes.GetStringArray("extraTraits", null);
							List<string> currentTraits = ((stringArray != null) ? stringArray.ToList<string>() : null) ?? new List<string>();
							if (!currentTraits.Contains(traitCode))
							{
								currentTraits.Add(traitCode);
								watchedAttributes.SetStringArray("extraTraits", currentTraits.ToArray());
								watchedAttributes.MarkPathDirty("extraTraits");
								playerEntity.WatchedAttributes.MarkAllDirty();
							}
							this.characterSystem.HasTrait(player, traitCode);
						}
					}
				}
			}
			catch (Exception ex)
			{
				ILogger logger = this.serverApi.Logger;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(57, 3);
				defaultInterpolatedStringHandler.AppendLiteral("[GuildTraitManager] Failed to grant trait '");
				defaultInterpolatedStringHandler.AppendFormatted(traitCode);
				defaultInterpolatedStringHandler.AppendLiteral("' to player ");
				defaultInterpolatedStringHandler.AppendFormatted(player.PlayerName);
				defaultInterpolatedStringHandler.AppendLiteral(": ");
				defaultInterpolatedStringHandler.AppendFormatted(ex.Message);
				logger.Error(defaultInterpolatedStringHandler.ToStringAndClear());
				this.serverApi.Logger.Error("[GuildTraitManager] Stack trace: " + ex.StackTrace);
			}
		}

		// Token: 0x060001A9 RID: 425 RVA: 0x0001069C File Offset: 0x0000E89C
		private void RevokeTrait(IServerPlayer player, string traitCode)
		{
			if (this.characterSystem == null || string.IsNullOrWhiteSpace(traitCode))
			{
				return;
			}
			try
			{
				if (this.characterSystem.HasTrait(player, traitCode))
				{
					EntityPlayer playerEntity = player.Entity;
					if (playerEntity != null)
					{
						SyncedTreeAttribute watchedAttributes = playerEntity.WatchedAttributes;
						if (watchedAttributes != null)
						{
							string[] stringArray = watchedAttributes.GetStringArray("extraTraits", null);
							List<string> currentTraits = (stringArray != null) ? stringArray.ToList<string>() : null;
							if (currentTraits != null && currentTraits.Count != 0)
							{
								if (currentTraits.Remove(traitCode))
								{
									watchedAttributes.SetStringArray("extraTraits", currentTraits.ToArray());
									watchedAttributes.MarkPathDirty("extraTraits");
									playerEntity.WatchedAttributes.MarkAllDirty();
								}
							}
						}
					}
				}
			}
			catch (Exception)
			{
			}
		}

		// Token: 0x060001AA RID: 426 RVA: 0x00010750 File Offset: 0x0000E950
		private void RemoveAllGuildTraits(IServerPlayer player)
		{
			foreach (string trait in this.GetPlayerGuildTraits(player))
			{
				this.RevokeTrait(player, trait);
			}
		}

		// Token: 0x040000A4 RID: 164
		private readonly ICoreServerAPI serverApi;

		// Token: 0x040000A5 RID: 165
		private readonly SRGuildsAndKingdomsModSystem modSystem;

		// Token: 0x040000A6 RID: 166
		[Nullable(2)]
		private CharacterSystem characterSystem;
	}
}
