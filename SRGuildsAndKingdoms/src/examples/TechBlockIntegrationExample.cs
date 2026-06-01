using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using SRGuildsAndKingdoms.src.techblock;
using Vintagestory.API.Common;
using Vintagestory.API.Server;

namespace SRGuildsAndKingdoms.src.examples
{
	// Token: 0x020000AF RID: 175
	[NullableContext(1)]
	[Nullable(0)]
	public class TechBlockIntegrationExample
	{
		// Token: 0x06000809 RID: 2057 RVA: 0x000382FC File Offset: 0x000364FC
		public void InitializeTechSystem(ICoreServerAPI api)
		{
			this.serverApi = api;
			try
			{
				api.Logger.Notification("Loading tech blocks configuration...");
				this.techConfig = TechBlocksConfig.LoadFromFile(api, "techblocks.json", null);
				if (!this.techConfig.Validate(api))
				{
					api.Logger.Error("Tech blocks configuration validation failed!");
				}
				else
				{
					this.techManager = new GuildTechManager(api, null, null);
					ILogger logger = api.Logger;
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(42, 1);
					defaultInterpolatedStringHandler.AppendLiteral("Tech system initialized with ");
					defaultInterpolatedStringHandler.AppendFormatted<int>(this.techConfig.TechBlocks.Count);
					defaultInterpolatedStringHandler.AppendLiteral(" technologies");
					logger.Notification(defaultInterpolatedStringHandler.ToStringAndClear());
				}
			}
			catch (Exception ex)
			{
				api.Logger.Error("Failed to initialize tech system: " + ex.Message);
				throw;
			}
		}

		// Token: 0x0600080A RID: 2058 RVA: 0x000383DC File Offset: 0x000365DC
		public void OnPlayerContributeToTech(string guildId, string playerName, int techBlockId, Dictionary<string, int> playerInventory)
		{
			try
			{
				TechBlock techBlock = this.techConfig.TechBlocks.Find((TechBlock t) => t.Id == techBlockId);
				if (techBlock == null)
				{
					ILogger logger = this.serverApi.Logger;
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(21, 1);
					defaultInterpolatedStringHandler.AppendLiteral("Tech block ");
					defaultInterpolatedStringHandler.AppendFormatted<int>(techBlockId);
					defaultInterpolatedStringHandler.AppendLiteral(" not found");
					logger.Warning(defaultInterpolatedStringHandler.ToStringAndClear());
				}
				else
				{
					GuildTechProgress progress = this.techManager.GetGuildTechData(guildId).GetOrCreateProgress(techBlockId);
					if (progress.IsUnlocked)
					{
						this.serverApi.Logger.Notification("Tech '" + techBlock.Text + "' is already unlocked for guild " + guildId);
					}
					else if (!this.techManager.HasPrerequisites(guildId, techBlock, this.techConfig.TechBlocks))
					{
						this.serverApi.Logger.Notification("Prerequisites not met for tech '" + techBlock.Text + "'");
					}
					else if (!techBlock.CanResearchWithItems(playerInventory, progress))
					{
						this.serverApi.Logger.Notification("Player " + playerName + " doesn't have required resources");
					}
					else
					{
						Dictionary<string, int> contributed = new Dictionary<string, int>();
						bool allRequirementsMet = true;
						foreach (ResourceGroup group in techBlock.ResourceGroups)
						{
							string groupName = group.Name;
							int amountRequired = group.AmountRequired;
							int alreadySubmitted = progress.GetResourceGroupSubmitted(groupName);
							int stillNeeded = amountRequired - alreadySubmitted;
							if (stillNeeded > 0)
							{
								int toContribute = Math.Min(group.GetTotalMatchingQuantity(playerInventory), stillNeeded);
								if (toContribute > 0)
								{
									contributed[groupName] = toContribute;
								}
								if (toContribute < stillNeeded)
								{
									allRequirementsMet = false;
								}
							}
						}
						if (contributed.Count > 0)
						{
							this.techManager.SubmitResources(guildId, techBlockId, contributed);
							ILogger logger2 = this.serverApi.Logger;
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(35, 2);
							defaultInterpolatedStringHandler2.AppendLiteral("Player ");
							defaultInterpolatedStringHandler2.AppendFormatted(playerName);
							defaultInterpolatedStringHandler2.AppendLiteral(" contributed resources to '");
							defaultInterpolatedStringHandler2.AppendFormatted(techBlock.Text);
							defaultInterpolatedStringHandler2.AppendLiteral("'");
							logger2.Notification(defaultInterpolatedStringHandler2.ToStringAndClear());
						}
						if (allRequirementsMet)
						{
							this.techManager.UnlockTech(guildId, techBlockId, null);
							this.serverApi.Logger.Event("Guild " + guildId + " unlocked technology: " + techBlock.Text);
							this.NotifyGuildMembers(guildId, "Technology '" + techBlock.Text + "' has been unlocked!");
						}
					}
				}
			}
			catch (Exception ex)
			{
				this.serverApi.Logger.Error("Error contributing to tech: " + ex.Message);
			}
		}

		// Token: 0x0600080B RID: 2059 RVA: 0x000386D4 File Offset: 0x000368D4
		public List<TechBlockInfo> GetAvailableTechsForGuild(string guildId)
		{
			List<TechBlockInfo> result = new List<TechBlockInfo>();
			GuildTechData guildData = this.techManager.GetGuildTechData(guildId);
			foreach (TechBlock techBlock in this.techConfig.TechBlocks)
			{
				GuildTechProgress progress = guildData.GetOrCreateProgress(techBlock.Id);
				bool hasPrereqs = this.techManager.HasPrerequisites(guildId, techBlock, this.techConfig.TechBlocks);
				result.Add(new TechBlockInfo
				{
					TechBlock = techBlock,
					IsUnlocked = progress.IsUnlocked,
					IsAvailable = (hasPrereqs && !progress.IsUnlocked),
					Progress = progress,
					ProgressPercentage = this.CalculateProgressPercentage(techBlock, progress)
				});
			}
			return result;
		}

		// Token: 0x0600080C RID: 2060 RVA: 0x000387B4 File Offset: 0x000369B4
		private double CalculateProgressPercentage(TechBlock techBlock, GuildTechProgress progress)
		{
			if (progress.IsUnlocked)
			{
				return 100.0;
			}
			if (techBlock.ResourceGroups == null || techBlock.ResourceGroups.Count == 0)
			{
				return 0.0;
			}
			double totalRequired = 0.0;
			double totalSubmitted = 0.0;
			foreach (ResourceGroup group in techBlock.ResourceGroups)
			{
				totalRequired += (double)group.AmountRequired;
				totalSubmitted += (double)progress.GetResourceGroupSubmitted(group.Name);
			}
			if (totalRequired <= 0.0)
			{
				return 0.0;
			}
			return totalSubmitted / totalRequired * 100.0;
		}

		// Token: 0x0600080D RID: 2061 RVA: 0x00038884 File Offset: 0x00036A84
		public void ReloadTechConfig()
		{
			try
			{
				this.techConfig = TechBlocksConfig.LoadFromFile(this.serverApi, "techblocks.json", null);
				if (this.techConfig.Validate(this.serverApi))
				{
					ILogger logger = this.serverApi.Logger;
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(21, 1);
					defaultInterpolatedStringHandler.AppendLiteral("Reloaded ");
					defaultInterpolatedStringHandler.AppendFormatted<int>(this.techConfig.TechBlocks.Count);
					defaultInterpolatedStringHandler.AppendLiteral(" tech blocks");
					logger.Notification(defaultInterpolatedStringHandler.ToStringAndClear());
				}
				else
				{
					this.serverApi.Logger.Error("Failed to validate reloaded tech config");
				}
			}
			catch (Exception ex)
			{
				this.serverApi.Logger.Error("Failed to reload tech config: " + ex.Message);
			}
		}

		// Token: 0x0600080E RID: 2062 RVA: 0x00038958 File Offset: 0x00036B58
		private void NotifyGuildMembers(string guildId, string message)
		{
		}

		// Token: 0x0400034C RID: 844
		private ICoreServerAPI serverApi;

		// Token: 0x0400034D RID: 845
		private TechBlocksConfig techConfig;

		// Token: 0x0400034E RID: 846
		private GuildTechManager techManager;
	}
}
