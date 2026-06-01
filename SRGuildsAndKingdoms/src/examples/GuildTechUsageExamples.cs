using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using SRGuildsAndKingdoms.src.database;
using SRGuildsAndKingdoms.src.guilds;
using SRGuildsAndKingdoms.src.techblock;
using Vintagestory.API.Common;
using Vintagestory.API.Server;

namespace SRGuildsAndKingdoms.src.examples
{
	// Token: 0x020000AE RID: 174
	[NullableContext(1)]
	[Nullable(0)]
	public class GuildTechUsageExamples
	{
		// Token: 0x06000800 RID: 2048 RVA: 0x00037D44 File Offset: 0x00035F44
		public void Initialize(ICoreServerAPI sapi, GuildManager guildManager, GuildRepository repository)
		{
			this.sapi = sapi;
			this.guildManager = guildManager;
			this.repository = repository;
			this.techManager = new GuildTechManager(sapi, (string guildName) => guildManager.GetGuild(guildName), delegate(string guildName)
			{
				repository.MarkDirty(guildName);
			});
		}

		// Token: 0x06000801 RID: 2049 RVA: 0x00037DA8 File Offset: 0x00035FA8
		public void Example1_AccessTechViaGuild()
		{
			string guildName = "The Iron Legion";
			Guild guild = this.guildManager.GetGuild(guildName);
			if (guild == null)
			{
				this.sapi.Logger.Warning("Guild '" + guildName + "' not found");
				return;
			}
			GuildTechProgress orCreateTechProgress = guild.GetOrCreateTechProgress(1);
			orCreateTechProgress.IsUnlocked = true;
			orCreateTechProgress.UnlockedTimestamp = new long?(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
			this.repository.MarkDirty(guildName);
			GuildTechProgress tech;
			bool isUnlocked = guild.TechProgress.TryGetValue(1, out tech) && tech.IsUnlocked;
			ILogger logger = this.sapi.Logger;
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(17, 1);
			defaultInterpolatedStringHandler.AppendLiteral("Tech 1 unlocked: ");
			defaultInterpolatedStringHandler.AppendFormatted<bool>(isUnlocked);
			logger.Notification(defaultInterpolatedStringHandler.ToStringAndClear());
		}

		// Token: 0x06000802 RID: 2050 RVA: 0x00037E70 File Offset: 0x00036070
		public void Example2_UsingTechManager()
		{
			string guildName = "The Copper Alliance";
			this.techManager.UnlockTech(guildName, 1, null);
			Dictionary<string, int> resources = new Dictionary<string, int>
			{
				{
					"plank-*",
					50
				},
				{
					"clay-blue",
					20
				}
			};
			this.techManager.SubmitResources(guildName, 2, resources);
			List<int> unlockedTechs = this.techManager.GetUnlockedTechs(guildName);
			ILogger logger = this.sapi.Logger;
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(25, 1);
			defaultInterpolatedStringHandler.AppendLiteral("Guild has ");
			defaultInterpolatedStringHandler.AppendFormatted<int>(unlockedTechs.Count);
			defaultInterpolatedStringHandler.AppendLiteral(" unlocked techs");
			logger.Notification(defaultInterpolatedStringHandler.ToStringAndClear());
		}

		// Token: 0x06000803 RID: 2051 RVA: 0x00037F14 File Offset: 0x00036114
		public void Example3_NetworkTransmission()
		{
			string guildId = "guild_003";
			GuildTechData guildTechData = this.techManager.GetGuildTechData(guildId);
			GuildTechData.FromJson(guildTechData.ToJson());
			GuildTechData.FromBytes(guildTechData.ToBytes());
		}

		// Token: 0x06000804 RID: 2052 RVA: 0x00037F4C File Offset: 0x0003614C
		public void Example4_CheckingRequirements(List<TechBlock> allTechBlocks)
		{
			string guildName = "The Bronze Collective";
			Guild guild = this.guildManager.GetGuild(guildName);
			if (guild == null)
			{
				return;
			}
			TechBlock techBlock = allTechBlocks.Find((TechBlock t) => t.Id == 5);
			bool hasPrereqs = this.techManager.HasPrerequisites(guildName, techBlock, allTechBlocks);
			Dictionary<string, int> availableItems = new Dictionary<string, int>
			{
				{
					"plank-oak",
					100
				},
				{
					"plank-birch",
					50
				},
				{
					"clay-blue",
					30
				}
			};
			GuildTechProgress progress = guild.GetOrCreateTechProgress(techBlock.Id);
			if (techBlock.CanResearchWithItems(availableItems, progress) && hasPrereqs)
			{
				this.sapi.Logger.Notification("Guild can research this tech!");
			}
		}

		// Token: 0x06000805 RID: 2053 RVA: 0x00038004 File Offset: 0x00036204
		public void Example5_CompleteResearchWorkflow(TechBlock tech, Dictionary<string, int> playerInventory)
		{
			string guildName = "Crescent Coven";
			Guild guild = this.guildManager.GetGuild(guildName);
			if (guild == null)
			{
				return;
			}
			GuildTechProgress progress = guild.GetOrCreateTechProgress(tech.Id);
			if (progress.IsUnlocked)
			{
				this.sapi.Logger.Notification("Tech already unlocked!");
				return;
			}
			foreach (ResourceGroup group in tech.ResourceGroups)
			{
				string groupName = group.Name;
				int amountRequired = group.AmountRequired;
				int alreadySubmitted = progress.GetResourceGroupSubmitted(groupName);
				int stillNeeded = amountRequired - alreadySubmitted;
				if (stillNeeded > 0)
				{
					if (group.GetTotalMatchingQuantity(playerInventory) < stillNeeded)
					{
						ILogger logger = this.sapi.Logger;
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(14, 2);
						defaultInterpolatedStringHandler.AppendLiteral("Need ");
						defaultInterpolatedStringHandler.AppendFormatted<int>(stillNeeded);
						defaultInterpolatedStringHandler.AppendLiteral(" more of ");
						defaultInterpolatedStringHandler.AppendFormatted(groupName);
						logger.Notification(defaultInterpolatedStringHandler.ToStringAndClear());
						return;
					}
					Dictionary<string, int> toSubmit = new Dictionary<string, int>
					{
						{
							groupName,
							stillNeeded
						}
					};
					this.techManager.SubmitResources(guildName, tech.Id, toSubmit);
				}
			}
			this.techManager.UnlockTech(guildName, tech.Id, null);
			this.sapi.Logger.Notification("Tech " + tech.Text + " unlocked and will be saved to database!");
		}

		// Token: 0x06000806 RID: 2054 RVA: 0x00038178 File Offset: 0x00036378
		public void Example6_AutomaticPersistence()
		{
			string guildName = "The Titanium Order";
			Guild guild = this.guildManager.GetGuild(guildName);
			if (guild == null)
			{
				return;
			}
			guild.Points += 100;
			guild.GetOrCreateTechProgress(10).IsUnlocked = true;
			this.repository.MarkDirty(guildName);
			this.sapi.Logger.Notification("Guild marked dirty - changes will be saved automatically");
		}

		// Token: 0x06000807 RID: 2055 RVA: 0x000381DC File Offset: 0x000363DC
		public void Example7_BulkOperations()
		{
			List<Guild> allGuilds = this.repository.GetAllGuilds();
			foreach (Guild guild in allGuilds)
			{
				try
				{
					guild.Points += 10;
					this.repository.MarkDirty(guild.Name);
					this.sapi.Logger.Notification("Processed guild " + guild.Name);
				}
				catch (Exception ex)
				{
					this.sapi.Logger.Error("Failed to process guild " + guild.Name + ": " + ex.Message);
				}
			}
			ILogger logger = this.sapi.Logger;
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(40, 1);
			defaultInterpolatedStringHandler.AppendLiteral("Processed ");
			defaultInterpolatedStringHandler.AppendFormatted<int>(allGuilds.Count);
			defaultInterpolatedStringHandler.AppendLiteral(" guilds - changes pending save");
			logger.Notification(defaultInterpolatedStringHandler.ToStringAndClear());
		}

		// Token: 0x04000348 RID: 840
		private ICoreServerAPI sapi;

		// Token: 0x04000349 RID: 841
		private GuildManager guildManager;

		// Token: 0x0400034A RID: 842
		private GuildTechManager techManager;

		// Token: 0x0400034B RID: 843
		private GuildRepository repository;
	}
}
