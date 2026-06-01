using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using Vintagestory.API.Common;
using Vintagestory.API.Server;

namespace SRGuildsAndKingdoms.src.config
{
	// Token: 0x020000C4 RID: 196
	[NullableContext(1)]
	[Nullable(0)]
	public class GuildConfigManager
	{
		// Token: 0x0600097F RID: 2431 RVA: 0x000435F3 File Offset: 0x000417F3
		public GuildConfigManager(ICoreServerAPI serverApi)
		{
			this.serverApi = serverApi;
			this.configPath = Path.Combine(serverApi.GetOrCreateDataPath("ModConfig/SRGuildsAndKingdoms"), "guild-config.json");
			this.config = new GuildConfig();
		}

		// Token: 0x06000980 RID: 2432 RVA: 0x00043628 File Offset: 0x00041828
		public void LoadConfig()
		{
			try
			{
				if (File.Exists(this.configPath))
				{
					GuildConfig loadedConfig = JsonSerializer.Deserialize<GuildConfig>(File.ReadAllText(this.configPath), GuildConfigManager.GetSerializerOptions());
					if (loadedConfig != null)
					{
						this.config = loadedConfig;
						bool flag = this.EnsureProtectedZoneIds();
						this.serverApi.Logger.Notification("Guild configuration loaded successfully");
						if (flag)
						{
							this.SaveConfig();
							this.serverApi.Logger.Notification("Auto-assigned IDs to protected zones and saved config");
						}
					}
					else
					{
						this.serverApi.Logger.Warning("Failed to deserialize guild config, using defaults");
					}
				}
				else
				{
					this.SaveConfig();
					this.serverApi.Logger.Notification("Created default guild configuration file at: " + this.configPath);
					this.serverApi.Logger.Notification("You can edit this file to customize guild settings, then restart the server.");
				}
				this.ValidateConfig();
			}
			catch (Exception ex)
			{
				this.serverApi.Logger.Error("Error loading guild configuration: " + ex.Message);
				this.config = new GuildConfig();
			}
		}

		// Token: 0x06000981 RID: 2433 RVA: 0x00043734 File Offset: 0x00041934
		private void SaveConfig()
		{
			try
			{
				string json = JsonSerializer.Serialize<GuildConfig>(this.config, GuildConfigManager.GetSerializerOptions());
				File.WriteAllText(this.configPath, json);
				this.serverApi.Logger.Debug("Guild configuration saved");
			}
			catch (Exception ex)
			{
				this.serverApi.Logger.Error("Error saving guild configuration: " + ex.Message);
			}
		}

		// Token: 0x06000982 RID: 2434 RVA: 0x000437A8 File Offset: 0x000419A8
		public GuildConfig GetConfig()
		{
			return this.config;
		}

		// Token: 0x06000983 RID: 2435 RVA: 0x000437B0 File Offset: 0x000419B0
		public int GetMaxClaimsPerGuild(int guildMemberCount)
		{
			return this.config.CalculateMaxClaimsPerGuild(guildMemberCount);
		}

		// Token: 0x06000984 RID: 2436 RVA: 0x000437BE File Offset: 0x000419BE
		public int GetMaxOutpostsPerGuild(int guildMemberCount)
		{
			return this.config.CalculateMaxOutpostsPerGuild(guildMemberCount);
		}

		// Token: 0x06000985 RID: 2437 RVA: 0x000437CC File Offset: 0x000419CC
		public void UpdateQuestCurrency(string currencyType, string itemCode, [Nullable(2)] string nbtBase64)
		{
			CurrencyDefinition currencyDef = new CurrencyDefinition(itemCode, nbtBase64);
			if (currencyType.Equals("tails", StringComparison.OrdinalIgnoreCase))
			{
				this.config.QuestTailsDefinition = currencyDef;
				this.serverApi.Logger.Notification("[GuildConfig] Updated Tails currency: " + itemCode + ((nbtBase64 == null) ? "" : " (with NBT)"));
			}
			else
			{
				if (!currencyType.Equals("crowns", StringComparison.OrdinalIgnoreCase))
				{
					this.serverApi.Logger.Warning("[GuildConfig] Unknown currency type: " + currencyType);
					return;
				}
				this.config.QuestCrownsDefinition = currencyDef;
				this.serverApi.Logger.Notification("[GuildConfig] Updated Crowns currency: " + itemCode + ((nbtBase64 == null) ? "" : " (with NBT)"));
			}
			this.SaveConfig();
		}

		// Token: 0x06000986 RID: 2438 RVA: 0x00043890 File Offset: 0x00041A90
		private bool EnsureProtectedZoneIds()
		{
			if (this.config.ProtectedZones == null || this.config.ProtectedZones.Count == 0)
			{
				return false;
			}
			List<ProtectedZone> zonesNeedingIds = (from z in this.config.ProtectedZones
			where z.Id == 0
			select z).ToList<ProtectedZone>();
			if (zonesNeedingIds.Count != 0)
			{
				int nextId = (from z in this.config.ProtectedZones
				where z.Id > 0
				select z.Id).DefaultIfEmpty(-1).Max() + 1;
				int assignedCount = 0;
				foreach (ProtectedZone zone in zonesNeedingIds)
				{
					zone.Id = nextId;
					ILogger logger = this.serverApi.Logger;
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(41, 2);
					defaultInterpolatedStringHandler.AppendLiteral("[ZoneConfig] Auto-assigned ID ");
					defaultInterpolatedStringHandler.AppendFormatted<int>(nextId);
					defaultInterpolatedStringHandler.AppendLiteral(" to zone '");
					defaultInterpolatedStringHandler.AppendFormatted(zone.Name);
					defaultInterpolatedStringHandler.AppendLiteral("'");
					logger.Notification(defaultInterpolatedStringHandler.ToStringAndClear());
					nextId++;
					assignedCount++;
				}
				ILogger logger2 = this.serverApi.Logger;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(47, 1);
				defaultInterpolatedStringHandler2.AppendLiteral("[ZoneConfig] Assigned IDs to ");
				defaultInterpolatedStringHandler2.AppendFormatted<int>(assignedCount);
				defaultInterpolatedStringHandler2.AppendLiteral(" protected zone(s)");
				logger2.Notification(defaultInterpolatedStringHandler2.ToStringAndClear());
				return assignedCount > 0;
			}
			List<int> duplicateIds = (from z in this.config.ProtectedZones
			group z by z.Id into g
			where g.Count<ProtectedZone>() > 1
			select g.Key).ToList<int>();
			if (duplicateIds.Count > 0)
			{
				this.serverApi.Logger.Warning("Found duplicate zone IDs: " + string.Join<int>(", ", duplicateIds) + ". Reassigning IDs...");
				this.ReassignAllZoneIds();
				return true;
			}
			return false;
		}

		// Token: 0x06000987 RID: 2439 RVA: 0x00043B08 File Offset: 0x00041D08
		private void ReassignAllZoneIds()
		{
			for (int i = 0; i < this.config.ProtectedZones.Count; i++)
			{
				this.config.ProtectedZones[i].Id = i;
				ILogger logger = this.serverApi.Logger;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(38, 2);
				defaultInterpolatedStringHandler.AppendLiteral("[ZoneConfig] Reassigned ID ");
				defaultInterpolatedStringHandler.AppendFormatted<int>(i);
				defaultInterpolatedStringHandler.AppendLiteral(" to zone '");
				defaultInterpolatedStringHandler.AppendFormatted(this.config.ProtectedZones[i].Name);
				defaultInterpolatedStringHandler.AppendLiteral("'");
				logger.Debug(defaultInterpolatedStringHandler.ToStringAndClear());
			}
		}

		// Token: 0x06000988 RID: 2440 RVA: 0x00043BB8 File Offset: 0x00041DB8
		private void ValidateConfig()
		{
			this.config.BaseMaxClaimsPerGuild = Math.Max(1, this.config.BaseMaxClaimsPerGuild);
			this.config.AbsoluteMaxClaimsPerGuild = Math.Max(this.config.BaseMaxClaimsPerGuild, this.config.AbsoluteMaxClaimsPerGuild);
			this.config.BaseMaxOutpostsPerGuild = Math.Max(0, this.config.BaseMaxOutpostsPerGuild);
			this.config.AbsoluteMaxOutpostsPerGuild = Math.Max(this.config.BaseMaxOutpostsPerGuild, this.config.AbsoluteMaxOutpostsPerGuild);
			if (this.config.EnableTerritorialRestrictions)
			{
				if (this.config.TerritorialCenter == null)
				{
					this.serverApi.Logger.Warning("Territorial restrictions enabled but no center coordinates specified. Disabling territorial restrictions.");
					this.config.EnableTerritorialRestrictions = false;
				}
				else
				{
					this.config.TerritorialRadius = Math.Max(1000, this.config.TerritorialRadius);
					ILogger logger = this.serverApi.Logger;
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(57, 2);
					defaultInterpolatedStringHandler.AppendLiteral("Territorial restrictions enabled: Center ");
					defaultInterpolatedStringHandler.AppendFormatted<ClaimRestrictionCenter>(this.config.TerritorialCenter);
					defaultInterpolatedStringHandler.AppendLiteral(", Radius ");
					defaultInterpolatedStringHandler.AppendFormatted<int>(this.config.TerritorialRadius);
					defaultInterpolatedStringHandler.AppendLiteral(" blocks");
					logger.Notification(defaultInterpolatedStringHandler.ToStringAndClear());
				}
			}
			if (this.config.EnableProtectedZones)
			{
				if (this.config.ProtectedZones == null || this.config.ProtectedZones.Count == 0)
				{
					this.serverApi.Logger.Warning("Protected zones enabled but no zones defined. Disabling protected zones.");
					this.config.EnableProtectedZones = false;
				}
				else
				{
					int validZoneCount = 0;
					foreach (ProtectedZone zone in this.config.ProtectedZones)
					{
						zone.Radius = Math.Max(50, zone.Radius);
						if (string.IsNullOrWhiteSpace(zone.Name))
						{
							ProtectedZone protectedZone = zone;
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(15, 1);
							defaultInterpolatedStringHandler2.AppendLiteral("Protected Zone ");
							defaultInterpolatedStringHandler2.AppendFormatted<int>(validZoneCount + 1);
							protectedZone.Name = defaultInterpolatedStringHandler2.ToStringAndClear();
						}
						validZoneCount++;
					}
					ILogger logger2 = this.serverApi.Logger;
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler3 = new DefaultInterpolatedStringHandler(41, 1);
					defaultInterpolatedStringHandler3.AppendLiteral("Protected zones enabled: ");
					defaultInterpolatedStringHandler3.AppendFormatted<int>(validZoneCount);
					defaultInterpolatedStringHandler3.AppendLiteral(" zone(s) defined");
					logger2.Notification(defaultInterpolatedStringHandler3.ToStringAndClear());
					foreach (ProtectedZone zone2 in this.config.ProtectedZones)
					{
						ILogger logger3 = this.serverApi.Logger;
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler4 = new DefaultInterpolatedStringHandler(11, 2);
						defaultInterpolatedStringHandler4.AppendLiteral("  - [ID: ");
						defaultInterpolatedStringHandler4.AppendFormatted<int>(zone2.Id);
						defaultInterpolatedStringHandler4.AppendLiteral("] ");
						defaultInterpolatedStringHandler4.AppendFormatted<ProtectedZone>(zone2);
						logger3.Notification(defaultInterpolatedStringHandler4.ToStringAndClear());
					}
				}
			}
			this.config.PlayerCountThresholds.Sort((PlayerCountThreshold a, PlayerCountThreshold b) => a.MinPlayerCount.CompareTo(b.MinPlayerCount));
			this.config.OutpostPlayerCountThresholds.Sort((PlayerCountThreshold a, PlayerCountThreshold b) => a.MinPlayerCount.CompareTo(b.MinPlayerCount));
			List<PlayerCountThreshold> uniqueThresholds = new List<PlayerCountThreshold>();
			int lastPlayerCount = -1;
			foreach (PlayerCountThreshold threshold in this.config.PlayerCountThresholds)
			{
				if (threshold.MinPlayerCount > lastPlayerCount && threshold.MinPlayerCount > 0)
				{
					uniqueThresholds.Add(threshold);
					lastPlayerCount = threshold.MinPlayerCount;
				}
			}
			this.config.PlayerCountThresholds = uniqueThresholds;
			List<PlayerCountThreshold> uniqueOutpostThresholds = new List<PlayerCountThreshold>();
			int lastOutpostPlayerCount = -1;
			foreach (PlayerCountThreshold threshold2 in this.config.OutpostPlayerCountThresholds)
			{
				if (threshold2.MinPlayerCount > lastOutpostPlayerCount && threshold2.MinPlayerCount > 0)
				{
					uniqueOutpostThresholds.Add(threshold2);
					lastOutpostPlayerCount = threshold2.MinPlayerCount;
				}
			}
			this.config.OutpostPlayerCountThresholds = uniqueOutpostThresholds;
			ILogger logger4 = this.serverApi.Logger;
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler5 = new DefaultInterpolatedStringHandler(52, 3);
			defaultInterpolatedStringHandler5.AppendLiteral("Guild config validated: Base=");
			defaultInterpolatedStringHandler5.AppendFormatted<int>(this.config.BaseMaxClaimsPerGuild);
			defaultInterpolatedStringHandler5.AppendLiteral(", Dynamic=");
			defaultInterpolatedStringHandler5.AppendFormatted<bool>(this.config.EnableDynamicClaimLimits);
			defaultInterpolatedStringHandler5.AppendLiteral(", Thresholds=");
			defaultInterpolatedStringHandler5.AppendFormatted<int>(this.config.PlayerCountThresholds.Count);
			logger4.Debug(defaultInterpolatedStringHandler5.ToStringAndClear());
			ILogger logger5 = this.serverApi.Logger;
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler6 = new DefaultInterpolatedStringHandler(54, 3);
			defaultInterpolatedStringHandler6.AppendLiteral("Outpost config validated: Base=");
			defaultInterpolatedStringHandler6.AppendFormatted<int>(this.config.BaseMaxOutpostsPerGuild);
			defaultInterpolatedStringHandler6.AppendLiteral(", Dynamic=");
			defaultInterpolatedStringHandler6.AppendFormatted<bool>(this.config.EnableDynamicOutpostLimits);
			defaultInterpolatedStringHandler6.AppendLiteral(", Thresholds=");
			defaultInterpolatedStringHandler6.AppendFormatted<int>(this.config.OutpostPlayerCountThresholds.Count);
			logger5.Debug(defaultInterpolatedStringHandler6.ToStringAndClear());
		}

		// Token: 0x06000989 RID: 2441 RVA: 0x00044134 File Offset: 0x00042334
		private static JsonSerializerOptions GetSerializerOptions()
		{
			return new JsonSerializerOptions
			{
				WriteIndented = true,
				PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
				DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
			};
		}

		// Token: 0x0600098A RID: 2442 RVA: 0x00044154 File Offset: 0x00042354
		public string GetConfigStatus(int exampleMemberCount = 1)
		{
			int currentMaxClaims = this.GetMaxClaimsPerGuild(exampleMemberCount);
			int currentMaxOutposts = this.GetMaxOutpostsPerGuild(exampleMemberCount);
			PlayerCountThreshold nextThreshold = this.config.GetNextThreshold(exampleMemberCount);
			PlayerCountThreshold nextOutpostThreshold = this.config.GetNextOutpostThreshold(exampleMemberCount);
			string status = "Guild Config Status:\n";
			string str = status;
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(20, 1);
			defaultInterpolatedStringHandler.AppendLiteral("  Base Max Claims: ");
			defaultInterpolatedStringHandler.AppendFormatted<int>(this.config.BaseMaxClaimsPerGuild);
			defaultInterpolatedStringHandler.AppendLiteral("\n");
			status = str + defaultInterpolatedStringHandler.ToStringAndClear();
			string str2 = status;
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(19, 1);
			defaultInterpolatedStringHandler2.AppendLiteral("  Dynamic Limits: ");
			defaultInterpolatedStringHandler2.AppendFormatted<bool>(this.config.EnableDynamicClaimLimits);
			defaultInterpolatedStringHandler2.AppendLiteral("\n");
			status = str2 + defaultInterpolatedStringHandler2.ToStringAndClear();
			string str3 = status;
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler3 = new DefaultInterpolatedStringHandler(22, 1);
			defaultInterpolatedStringHandler3.AppendLiteral("  Base Max Outposts: ");
			defaultInterpolatedStringHandler3.AppendFormatted<int>(this.config.BaseMaxOutpostsPerGuild);
			defaultInterpolatedStringHandler3.AppendLiteral("\n");
			status = str3 + defaultInterpolatedStringHandler3.ToStringAndClear();
			string str4 = status;
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler4 = new DefaultInterpolatedStringHandler(27, 1);
			defaultInterpolatedStringHandler4.AppendLiteral("  Dynamic Outpost Limits: ");
			defaultInterpolatedStringHandler4.AppendFormatted<bool>(this.config.EnableDynamicOutpostLimits);
			defaultInterpolatedStringHandler4.AppendLiteral("\n");
			status = str4 + defaultInterpolatedStringHandler4.ToStringAndClear();
			status += "  Claim limits scale with guild member count\n";
			string str5 = status;
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler5 = new DefaultInterpolatedStringHandler(51, 3);
			defaultInterpolatedStringHandler5.AppendLiteral("  Example: ");
			defaultInterpolatedStringHandler5.AppendFormatted<int>(exampleMemberCount);
			defaultInterpolatedStringHandler5.AppendLiteral(" member(s) = ");
			defaultInterpolatedStringHandler5.AppendFormatted<int>(currentMaxClaims);
			defaultInterpolatedStringHandler5.AppendLiteral(" max claims, ");
			defaultInterpolatedStringHandler5.AppendFormatted<int>(currentMaxOutposts);
			defaultInterpolatedStringHandler5.AppendLiteral(" max outposts\n");
			status = str5 + defaultInterpolatedStringHandler5.ToStringAndClear();
			if (this.config.EnableTerritorialRestrictions && this.config.TerritorialCenter != null)
			{
				status += "  Territorial Restrictions: Enabled\n";
				string str6 = status;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler6 = new DefaultInterpolatedStringHandler(19, 1);
				defaultInterpolatedStringHandler6.AppendLiteral("  Allowed Center: ");
				defaultInterpolatedStringHandler6.AppendFormatted<ClaimRestrictionCenter>(this.config.TerritorialCenter);
				defaultInterpolatedStringHandler6.AppendLiteral("\n");
				status = str6 + defaultInterpolatedStringHandler6.ToStringAndClear();
				string str7 = status;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler7 = new DefaultInterpolatedStringHandler(26, 1);
				defaultInterpolatedStringHandler7.AppendLiteral("  Allowed Radius: ");
				defaultInterpolatedStringHandler7.AppendFormatted<int>(this.config.TerritorialRadius);
				defaultInterpolatedStringHandler7.AppendLiteral(" blocks\n");
				status = str7 + defaultInterpolatedStringHandler7.ToStringAndClear();
			}
			else
			{
				status += "  Territorial Restrictions: Disabled\n";
			}
			if (this.config.EnableProtectedZones && this.config.ProtectedZones != null && this.config.ProtectedZones.Count > 0)
			{
				string str8 = status;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler8 = new DefaultInterpolatedStringHandler(38, 1);
				defaultInterpolatedStringHandler8.AppendLiteral("  Protected Zones: Enabled (");
				defaultInterpolatedStringHandler8.AppendFormatted<int>(this.config.ProtectedZones.Count);
				defaultInterpolatedStringHandler8.AppendLiteral(" zone(s))\n");
				status = str8 + defaultInterpolatedStringHandler8.ToStringAndClear();
				using (List<ProtectedZone>.Enumerator enumerator = this.config.ProtectedZones.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						ProtectedZone zone = enumerator.Current;
						string str9 = status;
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler9 = new DefaultInterpolatedStringHandler(7, 1);
						defaultInterpolatedStringHandler9.AppendLiteral("    - ");
						defaultInterpolatedStringHandler9.AppendFormatted<ProtectedZone>(zone);
						defaultInterpolatedStringHandler9.AppendLiteral("\n");
						status = str9 + defaultInterpolatedStringHandler9.ToStringAndClear();
					}
					goto IL_374;
				}
			}
			status += "  Protected Zones: Disabled\n";
			IL_374:
			if (nextThreshold != null)
			{
				string str10 = status;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler10 = new DefaultInterpolatedStringHandler(44, 2);
				defaultInterpolatedStringHandler10.AppendLiteral("  Next Claim Threshold: ");
				defaultInterpolatedStringHandler10.AppendFormatted<int>(nextThreshold.MinPlayerCount);
				defaultInterpolatedStringHandler10.AppendLiteral(" members (+");
				defaultInterpolatedStringHandler10.AppendFormatted<int>(nextThreshold.AdditionalClaims);
				defaultInterpolatedStringHandler10.AppendLiteral(" claims)\n");
				status = str10 + defaultInterpolatedStringHandler10.ToStringAndClear();
			}
			else
			{
				status += "  At Maximum Claim Threshold\n";
			}
			if (nextOutpostThreshold != null)
			{
				string str11 = status;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler11 = new DefaultInterpolatedStringHandler(48, 2);
				defaultInterpolatedStringHandler11.AppendLiteral("  Next Outpost Threshold: ");
				defaultInterpolatedStringHandler11.AppendFormatted<int>(nextOutpostThreshold.MinPlayerCount);
				defaultInterpolatedStringHandler11.AppendLiteral(" members (+");
				defaultInterpolatedStringHandler11.AppendFormatted<int>(nextOutpostThreshold.AdditionalClaims);
				defaultInterpolatedStringHandler11.AppendLiteral(" outposts)\n");
				status = str11 + defaultInterpolatedStringHandler11.ToStringAndClear();
			}
			else
			{
				status += "  At Maximum Outpost Threshold\n";
			}
			return status;
		}

		// Token: 0x040003CA RID: 970
		private readonly ICoreServerAPI serverApi;

		// Token: 0x040003CB RID: 971
		private GuildConfig config;

		// Token: 0x040003CC RID: 972
		private readonly string configPath;
	}
}
