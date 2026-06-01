using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using SRGuildsAndKingdoms.src.guilds;
using Vintagestory.API.Common;

namespace SRGuildsAndKingdoms.src.techblock
{
	// Token: 0x02000009 RID: 9
	[NullableContext(1)]
	[Nullable(0)]
	public class GuildTechManager
	{
		// Token: 0x06000080 RID: 128 RVA: 0x00009C8C File Offset: 0x00007E8C
		public GuildTechManager(ICoreAPI api, [Nullable(new byte[]
		{
			2,
			1,
			1
		})] Func<string, Guild> getGuildFunc = null, [Nullable(new byte[]
		{
			2,
			1
		})] Action<string> markDirtyAction = null)
		{
			if (api == null)
			{
				throw new ArgumentNullException("api");
			}
			this.api = api;
			base..ctor();
		}

		// Token: 0x06000081 RID: 129 RVA: 0x00009CB8 File Offset: 0x00007EB8
		public GuildTechData GetGuildTechData(string guildId)
		{
			if (string.IsNullOrWhiteSpace(guildId))
			{
				throw new ArgumentException("GuildId cannot be null or empty", "guildId");
			}
			if (this.<getGuildFunc>P != null)
			{
				Guild guild = this.<getGuildFunc>P(guildId);
				if (guild != null)
				{
					return new GuildTechData
					{
						GuildId = guildId,
						TechProgress = guild.TechProgress
					};
				}
			}
			return new GuildTechData
			{
				GuildId = guildId
			};
		}

		// Token: 0x06000082 RID: 130 RVA: 0x00009D1C File Offset: 0x00007F1C
		public void UpdateTechProgress(string guildId, int techBlockId, Action<GuildTechProgress> updateAction)
		{
			if (this.<getGuildFunc>P != null)
			{
				Guild guild = this.<getGuildFunc>P(guildId);
				if (guild != null)
				{
					GuildTechProgress progress = guild.GetOrCreateTechProgress(techBlockId);
					updateAction(progress);
					Action<string> action = this.<markDirtyAction>P;
					if (action == null)
					{
						return;
					}
					action(guildId);
					return;
				}
			}
			GuildTechProgress techProgress = this.GetGuildTechData(guildId).GetOrCreateProgress(techBlockId);
			updateAction(techProgress);
		}

		// Token: 0x06000083 RID: 131 RVA: 0x00009D78 File Offset: 0x00007F78
		public decimal GetResourceScaling(int memberCount)
		{
			if (memberCount < 5)
			{
				return 1.0m;
			}
			decimal scalingPercentage = (memberCount - 4) * 6;
			return 1.0m + scalingPercentage / 100m;
		}

		// Token: 0x06000084 RID: 132 RVA: 0x00009DC0 File Offset: 0x00007FC0
		public Dictionary<string, int> GetScaledRequirements(string guildId, Dictionary<string, int> baseRequirements)
		{
			Func<string, Guild> func = this.<getGuildFunc>P;
			Guild guild = (func != null) ? func(guildId) : null;
			if (guild == null)
			{
				return new Dictionary<string, int>(baseRequirements);
			}
			decimal scaling = this.GetResourceScaling(guild.Members.Count);
			Dictionary<string, int> scaledRequirements = new Dictionary<string, int>();
			foreach (KeyValuePair<string, int> req in baseRequirements)
			{
				scaledRequirements[req.Key] = (int)Math.Ceiling(req.Value * scaling);
			}
			return scaledRequirements;
		}

		// Token: 0x06000085 RID: 133 RVA: 0x00009E68 File Offset: 0x00008068
		public void UnlockTech(string guildId, int techBlockId, TechBlock techBlock = null)
		{
			Func<string, Guild> func = this.<getGuildFunc>P;
			Guild guild = (func != null) ? func(guildId) : null;
			this.UpdateTechProgress(guildId, techBlockId, delegate(GuildTechProgress progress)
			{
				progress.IsUnlocked = true;
				progress.UnlockedTimestamp = new long?(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
			});
			if (guild != null && guild.Members.Count > 10)
			{
				guild.TechRequiresPersonalUnlock[techBlockId] = true;
				if (techBlock != null)
				{
					foreach (string memberUid in guild.Members.Keys)
					{
						this.InitializePersonalUnlock(guild, memberUid, techBlockId, techBlock);
					}
				}
				Action<string> action = this.<markDirtyAction>P;
				if (action != null)
				{
					action(guildId);
				}
				ILogger logger = this.api.Logger;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(61, 3);
				defaultInterpolatedStringHandler.AppendLiteral("Guild ");
				defaultInterpolatedStringHandler.AppendFormatted(guildId);
				defaultInterpolatedStringHandler.AppendLiteral(" unlocked tech ");
				defaultInterpolatedStringHandler.AppendFormatted<int>(techBlockId);
				defaultInterpolatedStringHandler.AppendLiteral(" (requires personal unlock for ");
				defaultInterpolatedStringHandler.AppendFormatted<int>(guild.Members.Count);
				defaultInterpolatedStringHandler.AppendLiteral(" members)");
				logger.Notification(defaultInterpolatedStringHandler.ToStringAndClear());
				return;
			}
			if (guild != null)
			{
				guild.TechRequiresPersonalUnlock[techBlockId] = false;
			}
			ILogger logger2 = this.api.Logger;
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(21, 2);
			defaultInterpolatedStringHandler2.AppendLiteral("Guild ");
			defaultInterpolatedStringHandler2.AppendFormatted(guildId);
			defaultInterpolatedStringHandler2.AppendLiteral(" unlocked tech ");
			defaultInterpolatedStringHandler2.AppendFormatted<int>(techBlockId);
			logger2.Notification(defaultInterpolatedStringHandler2.ToStringAndClear());
		}

		// Token: 0x06000086 RID: 134 RVA: 0x0000A008 File Offset: 0x00008208
		private void InitializePersonalUnlock(Guild guild, string playerUid, int techId, TechBlock techBlock)
		{
			PersonalTechUnlock orCreateUnlock = guild.GetOrCreatePlayerProgress(playerUid).GetOrCreateUnlock(techId);
			orCreateUnlock.RequiresPersonalUnlock = true;
			orCreateUnlock.IsPersonallyUnlocked = false;
		}

		// Token: 0x06000087 RID: 135 RVA: 0x0000A024 File Offset: 0x00008224
		public bool ContributeToPersonalUnlock(string guildId, string playerUid, int techId, string resourceGroupName, int amount, TechBlock techBlock)
		{
			Func<string, Guild> func = this.<getGuildFunc>P;
			Guild guild = (func != null) ? func(guildId) : null;
			if (guild == null)
			{
				return false;
			}
			PersonalTechUnlock personalUnlock = guild.GetOrCreatePlayerProgress(playerUid).GetOrCreateUnlock(techId);
			if (!personalUnlock.RequiresPersonalUnlock || personalUnlock.IsPersonallyUnlocked)
			{
				return personalUnlock.IsPersonallyUnlocked;
			}
			personalUnlock.IsPersonallyUnlocked = true;
			Action<string> action = this.<markDirtyAction>P;
			if (action != null)
			{
				action(guildId);
			}
			ILogger logger = this.api.Logger;
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(53, 3);
			defaultInterpolatedStringHandler.AppendLiteral("Player ");
			defaultInterpolatedStringHandler.AppendFormatted(playerUid);
			defaultInterpolatedStringHandler.AppendLiteral(" completed personal unlock for tech ");
			defaultInterpolatedStringHandler.AppendFormatted<int>(techId);
			defaultInterpolatedStringHandler.AppendLiteral(" in guild ");
			defaultInterpolatedStringHandler.AppendFormatted(guildId);
			logger.Notification(defaultInterpolatedStringHandler.ToStringAndClear());
			return true;
		}

		// Token: 0x06000088 RID: 136 RVA: 0x0000A0E8 File Offset: 0x000082E8
		public void InitializePersonalUnlocksForNewMember(string guildId, string playerUid, List<TechBlock> techBlocks)
		{
			Func<string, Guild> func = this.<getGuildFunc>P;
			Guild guild = (func != null) ? func(guildId) : null;
			if (guild == null)
			{
				return;
			}
			bool anyInitialized = false;
			using (Dictionary<int, bool>.Enumerator enumerator = guild.TechRequiresPersonalUnlock.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					KeyValuePair<int, bool> kvp = enumerator.Current;
					if (kvp.Value && guild.IsTechUnlocked(kvp.Key))
					{
						TechBlock techBlock = techBlocks.FirstOrDefault((TechBlock tb) => tb.Id == kvp.Key);
						if (techBlock != null)
						{
							this.InitializePersonalUnlock(guild, playerUid, kvp.Key, techBlock);
							anyInitialized = true;
						}
					}
				}
			}
			if (anyInitialized)
			{
				Action<string> action = this.<markDirtyAction>P;
				if (action == null)
				{
					return;
				}
				action(guildId);
			}
		}

		// Token: 0x06000089 RID: 137 RVA: 0x0000A1BC File Offset: 0x000083BC
		public void SubmitResources(string guildId, int techBlockId, Dictionary<string, int> resources)
		{
			this.UpdateTechProgress(guildId, techBlockId, delegate(GuildTechProgress progress)
			{
				foreach (KeyValuePair<string, int> resource in resources)
				{
					if (progress.ResourcesSubmitted.ContainsKey(resource.Key))
					{
						Dictionary<string, int> resourcesSubmitted = progress.ResourcesSubmitted;
						string key = resource.Key;
						resourcesSubmitted[key] += resource.Value;
					}
					else
					{
						progress.ResourcesSubmitted[resource.Key] = resource.Value;
					}
				}
			});
		}

		// Token: 0x0600008A RID: 138 RVA: 0x0000A1EC File Offset: 0x000083EC
		public bool HasPrerequisites(string guildId, TechBlock tech, List<TechBlock> allTechs)
		{
			GuildTechData guildData = this.GetGuildTechData(guildId);
			return tech.IsAvailableForGuild(guildData, allTechs);
		}

		// Token: 0x0600008B RID: 139 RVA: 0x0000A20C File Offset: 0x0000840C
		public List<int> GetUnlockedTechs(string guildId)
		{
			return (from kvp in this.GetGuildTechData(guildId).TechProgress
			where kvp.Value.IsUnlocked
			select kvp.Key).ToList<int>();
		}

		// Token: 0x04000027 RID: 39
		[Nullable(new byte[]
		{
			2,
			1,
			1
		})]
		[CompilerGenerated]
		private Func<string, Guild> <getGuildFunc>P = getGuildFunc;

		// Token: 0x04000028 RID: 40
		[Nullable(new byte[]
		{
			2,
			1
		})]
		[CompilerGenerated]
		private Action<string> <markDirtyAction>P = markDirtyAction;

		// Token: 0x04000029 RID: 41
		private readonly ICoreAPI api;
	}
}
