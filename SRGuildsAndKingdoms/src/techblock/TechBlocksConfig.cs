using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using Vintagestory.API.Common;

namespace SRGuildsAndKingdoms.src.techblock
{
	// Token: 0x02000012 RID: 18
	[NullableContext(1)]
	[Nullable(0)]
	public class TechBlocksConfig
	{
		// Token: 0x17000028 RID: 40
		// (get) Token: 0x060000DE RID: 222 RVA: 0x0000ADF9 File Offset: 0x00008FF9
		// (set) Token: 0x060000DF RID: 223 RVA: 0x0000AE01 File Offset: 0x00009001
		[JsonPropertyName("techBlocks")]
		public List<TechBlock> TechBlocks { get; set; } = new List<TechBlock>();

		// Token: 0x17000029 RID: 41
		// (get) Token: 0x060000E0 RID: 224 RVA: 0x0000AE0A File Offset: 0x0000900A
		// (set) Token: 0x060000E1 RID: 225 RVA: 0x0000AE12 File Offset: 0x00009012
		[JsonPropertyName("enabledAges")]
		public List<TechAge> EnabledAges { get; set; } = new List<TechAge>
		{
			TechAge.Stone,
			TechAge.Copper
		};

		// Token: 0x1700002A RID: 42
		// (get) Token: 0x060000E2 RID: 226 RVA: 0x0000AE1B File Offset: 0x0000901B
		// (set) Token: 0x060000E3 RID: 227 RVA: 0x0000AE23 File Offset: 0x00009023
		[JsonPropertyName("ageRestrictedBlocks")]
		public AgeRestrictedBlocksConfig AgeRestrictedBlocks { get; set; } = AgeRestrictedBlocksConfig.GetDefault();

		// Token: 0x060000E4 RID: 228 RVA: 0x0000AE2C File Offset: 0x0000902C
		public static TechBlocksConfig LoadFromFile(ICoreAPI api, string filename = "techblocks.json", [Nullable(2)] string serverIdentifier = null)
		{
			if (api.Side == 2 && !string.IsNullOrEmpty(serverIdentifier))
			{
				string serverFilePath = Path.Combine(api.GetOrCreateDataPath("ModConfig/SRGuildsAndKingdoms/servers/" + serverIdentifier), filename);
				if (File.Exists(serverFilePath))
				{
					try
					{
						string json = File.ReadAllText(serverFilePath);
						JsonSerializerOptions options = new JsonSerializerOptions
						{
							PropertyNameCaseInsensitive = true,
							ReadCommentHandling = JsonCommentHandling.Skip,
							AllowTrailingCommas = true
						};
						TechBlocksConfig config = JsonSerializer.Deserialize<TechBlocksConfig>(json, options);
						ILogger logger = api.Logger;
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(58, 2);
						defaultInterpolatedStringHandler.AppendLiteral("Loaded ");
						int? num;
						if (config == null)
						{
							num = null;
						}
						else
						{
							List<TechBlock> techBlocks = config.TechBlocks;
							num = ((techBlocks != null) ? new int?(techBlocks.Count) : null);
						}
						int? num2 = num;
						defaultInterpolatedStringHandler.AppendFormatted<int>(num2.GetValueOrDefault());
						defaultInterpolatedStringHandler.AppendLiteral(" tech blocks from server-specific config (server: ");
						defaultInterpolatedStringHandler.AppendFormatted(serverIdentifier);
						defaultInterpolatedStringHandler.AppendLiteral(")");
						logger.Notification(defaultInterpolatedStringHandler.ToStringAndClear());
						if (((config != null) ? config.EnabledAges : null) != null && config.EnabledAges.Count > 0)
						{
							api.Logger.Notification("Enabled tech ages: " + string.Join<TechAge>(", ", config.EnabledAges));
						}
						return config ?? new TechBlocksConfig();
					}
					catch (Exception ex)
					{
						api.Logger.Error("Failed to load server-specific tech blocks config: " + ex.Message);
					}
				}
			}
			string filePath = Path.Combine(api.GetOrCreateDataPath("ModConfig/SRGuildsAndKingdoms"), filename);
			if (!File.Exists(filePath))
			{
				api.Logger.Warning("Tech blocks config not found at " + filePath + ", creating default config");
				TechBlocksConfig techBlocksConfig = TechBlocksConfig.CreateDefaultConfig();
				techBlocksConfig.SaveToFile(api, filename);
				return techBlocksConfig;
			}
			TechBlocksConfig result;
			try
			{
				string json2 = File.ReadAllText(filePath);
				JsonSerializerOptions options2 = new JsonSerializerOptions
				{
					PropertyNameCaseInsensitive = true,
					ReadCommentHandling = JsonCommentHandling.Skip,
					AllowTrailingCommas = true
				};
				TechBlocksConfig config2 = JsonSerializer.Deserialize<TechBlocksConfig>(json2, options2);
				ILogger logger2 = api.Logger;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(25, 2);
				defaultInterpolatedStringHandler2.AppendLiteral("Loaded ");
				int? num3;
				if (config2 == null)
				{
					num3 = null;
				}
				else
				{
					List<TechBlock> techBlocks2 = config2.TechBlocks;
					num3 = ((techBlocks2 != null) ? new int?(techBlocks2.Count) : null);
				}
				int? num2 = num3;
				defaultInterpolatedStringHandler2.AppendFormatted<int>(num2.GetValueOrDefault());
				defaultInterpolatedStringHandler2.AppendLiteral(" tech blocks from ");
				defaultInterpolatedStringHandler2.AppendFormatted(filename);
				logger2.Notification(defaultInterpolatedStringHandler2.ToStringAndClear());
				if (((config2 != null) ? config2.EnabledAges : null) != null && config2.EnabledAges.Count > 0)
				{
					api.Logger.Notification("Enabled tech ages: " + string.Join<TechAge>(", ", config2.EnabledAges));
				}
				result = (config2 ?? new TechBlocksConfig());
			}
			catch (Exception ex2)
			{
				api.Logger.Error("Failed to load tech blocks config: " + ex2.Message);
				throw;
			}
			return result;
		}

		// Token: 0x060000E5 RID: 229 RVA: 0x0000B12C File Offset: 0x0000932C
		public static void SaveServerConfig(ICoreAPI api, string configJson, string serverIdentifier, string filename = "techblocks.json")
		{
			if (api.Side != 2)
			{
				api.Logger.Warning("SaveServerConfig should only be called on client side");
				return;
			}
			string filePath = Path.Combine(api.GetOrCreateDataPath("ModConfig/SRGuildsAndKingdoms/servers/" + serverIdentifier), filename);
			try
			{
				File.WriteAllText(filePath, configJson);
				api.Logger.Notification("Saved server-specific tech blocks config to " + filePath);
			}
			catch (Exception ex)
			{
				api.Logger.Error("Failed to save server-specific tech blocks config: " + ex.Message);
				throw;
			}
		}

		// Token: 0x060000E6 RID: 230 RVA: 0x0000B1B8 File Offset: 0x000093B8
		public void SaveToFile(ICoreAPI api, string filename = "techblocks.json")
		{
			string filePath = Path.Combine(api.GetOrCreateDataPath("ModConfig/SRGuildsAndKingdoms"), filename);
			try
			{
				JsonSerializerOptions options = new JsonSerializerOptions
				{
					WriteIndented = true,
					DefaultIgnoreCondition = JsonIgnoreCondition.Never
				};
				string json = JsonSerializer.Serialize<TechBlocksConfig>(this, options);
				File.WriteAllText(filePath, json);
				api.Logger.Notification("Saved tech blocks config to " + filePath);
			}
			catch (Exception ex)
			{
				api.Logger.Error("Failed to save tech blocks config: " + ex.Message);
				throw;
			}
		}

		// Token: 0x060000E7 RID: 231 RVA: 0x0000B240 File Offset: 0x00009440
		public bool IsAgeEnabled(TechAge age)
		{
			return this.EnabledAges != null && this.EnabledAges.Contains(age);
		}

		// Token: 0x060000E8 RID: 232 RVA: 0x0000B258 File Offset: 0x00009458
		public void EnableAge(TechAge age)
		{
			if (this.EnabledAges == null)
			{
				this.EnabledAges = new List<TechAge>();
			}
			if (!this.EnabledAges.Contains(age))
			{
				this.EnabledAges.Add(age);
			}
		}

		// Token: 0x060000E9 RID: 233 RVA: 0x0000B287 File Offset: 0x00009487
		public void DisableAge(TechAge age)
		{
			if (this.EnabledAges != null)
			{
				this.EnabledAges.Remove(age);
			}
		}

		// Token: 0x060000EA RID: 234 RVA: 0x0000B2A0 File Offset: 0x000094A0
		public static TechBlocksConfig CreateDefaultConfig()
		{
			return new TechBlocksConfig
			{
				TechBlocks = new List<TechBlock>
				{
					new TechBlock
					{
						Id = 1,
						Text = "Copper Age",
						Description = "Learn how to melt copper and create ingots",
						Level = 2,
						Age = TechAge.Stone,
						ResourceGroups = new List<ResourceGroup>
						{
							new ResourceGroup
							{
								Name = "Copper Bits or Nuggets",
								AmountRequired = 20,
								ResourcePatterns = new List<string>
								{
									"nugget-nativecopper",
									"metalbit-copper"
								}
							},
							new ResourceGroup
							{
								Name = "Any Raw Hides",
								AmountRequired = 5,
								ResourcePatterns = new List<string>
								{
									"hide-raw-*"
								}
							},
							new ResourceGroup
							{
								Name = "Any Clay",
								AmountRequired = 20,
								ResourcePatterns = new List<string>
								{
									"clay-*"
								}
							},
							new ResourceGroup
							{
								Name = "Firewood",
								AmountRequired = 40,
								ResourcePatterns = new List<string>
								{
									"firewood"
								}
							}
						},
						UnlocksIds = new List<int>
						{
							2
						},
						GrantedTraits = new List<string>
						{
							"CopperAge"
						}
					},
					new TechBlock
					{
						Id = 2,
						Text = "Tin Bronze Age",
						Description = "Advance to the Tin Bronze Age",
						Level = 2,
						Age = TechAge.Copper,
						ResourceGroups = new List<ResourceGroup>
						{
							new ResourceGroup
							{
								Name = "Copper Ingots",
								AmountRequired = 10,
								ResourcePatterns = new List<string>
								{
									"ingot-copper"
								}
							},
							new ResourceGroup
							{
								Name = "Any Raw Hides",
								AmountRequired = 10,
								ResourcePatterns = new List<string>
								{
									"hide-raw-*"
								}
							},
							new ResourceGroup
							{
								Name = "Any Clay",
								AmountRequired = 20,
								ResourcePatterns = new List<string>
								{
									"clay-*"
								}
							},
							new ResourceGroup
							{
								Name = "Cupronickel Nails and Strips",
								AmountRequired = 20,
								ResourcePatterns = new List<string>
								{
									"metalnailsandstrips-cupronickel"
								}
							},
							new ResourceGroup
							{
								Name = "Firewood",
								AmountRequired = 64,
								ResourcePatterns = new List<string>
								{
									"firewood"
								}
							}
						},
						UnlocksIds = new List<int>
						{
							3
						},
						GrantedTraits = new List<string>
						{
							"BronzeAge"
						}
					},
					new TechBlock
					{
						Id = 3,
						Text = "Bismuth and Black Bronze Age",
						Description = "Learn advanced bronze alloys",
						Level = 2,
						Age = TechAge.Bronze,
						ResourceGroups = new List<ResourceGroup>
						{
							new ResourceGroup
							{
								Name = "Copper Ingots",
								AmountRequired = 15,
								ResourcePatterns = new List<string>
								{
									"ingot-copper"
								}
							},
							new ResourceGroup
							{
								Name = "Tin Ingots",
								AmountRequired = 15,
								ResourcePatterns = new List<string>
								{
									"ingot-tinbronze"
								}
							},
							new ResourceGroup
							{
								Name = "Leather",
								AmountRequired = 10,
								ResourcePatterns = new List<string>
								{
									"leather-normal-plain"
								}
							},
							new ResourceGroup
							{
								Name = "Any Clay",
								AmountRequired = 20,
								ResourcePatterns = new List<string>
								{
									"clay-*"
								}
							},
							new ResourceGroup
							{
								Name = "Cupronickel Nails and Strips",
								AmountRequired = 25,
								ResourcePatterns = new List<string>
								{
									"metalnailsandstrips-cupronickel"
								}
							},
							new ResourceGroup
							{
								Name = "Flax Twine",
								AmountRequired = 10,
								ResourcePatterns = new List<string>
								{
									"flaxtwine"
								}
							},
							new ResourceGroup
							{
								Name = "Charcoal",
								AmountRequired = 40,
								ResourcePatterns = new List<string>
								{
									"charcoal"
								}
							}
						},
						UnlocksIds = new List<int>
						{
							4
						},
						GrantedTraits = new List<string>
						{
							"AdvBronzeAge"
						}
					},
					new TechBlock
					{
						Id = 4,
						Text = "Iron Age",
						Description = "Learn the Iron Age",
						Level = 2,
						Age = TechAge.OtherBronze,
						ResourceGroups = new List<ResourceGroup>
						{
							new ResourceGroup
							{
								Name = "Any Bronze Ingots",
								AmountRequired = 20,
								ResourcePatterns = new List<string>
								{
									"ingot-*bronze"
								}
							},
							new ResourceGroup
							{
								Name = "Leather",
								AmountRequired = 10,
								ResourcePatterns = new List<string>
								{
									"leather-normal-plain"
								}
							},
							new ResourceGroup
							{
								Name = "Fire Clay",
								AmountRequired = 20,
								ResourcePatterns = new List<string>
								{
									"clay-fire"
								}
							},
							new ResourceGroup
							{
								Name = "Cupronickel Nails and Strips",
								AmountRequired = 25,
								ResourcePatterns = new List<string>
								{
									"metalnailsandstrips-cupronickel"
								}
							},
							new ResourceGroup
							{
								Name = "Flax Twine",
								AmountRequired = 15,
								ResourcePatterns = new List<string>
								{
									"flaxtwine"
								}
							},
							new ResourceGroup
							{
								Name = "Feathers",
								AmountRequired = 10,
								ResourcePatterns = new List<string>
								{
									"feathers"
								}
							},
							new ResourceGroup
							{
								Name = "Gold Ingots",
								AmountRequired = 5,
								ResourcePatterns = new List<string>
								{
									"ingot-gold"
								}
							},
							new ResourceGroup
							{
								Name = "Silver Ingots",
								AmountRequired = 5,
								ResourcePatterns = new List<string>
								{
									"ingot-silver"
								}
							},
							new ResourceGroup
							{
								Name = "Charcoal",
								AmountRequired = 64,
								ResourcePatterns = new List<string>
								{
									"charcoal"
								}
							}
						},
						UnlocksIds = new List<int>
						{
							5
						},
						GrantedTraits = new List<string>
						{
							"IronAge"
						}
					},
					new TechBlock
					{
						Id = 5,
						Text = "Meteoric Iron",
						Description = "Space Metal Age",
						Level = 2,
						Age = TechAge.Iron,
						ResourceGroups = new List<ResourceGroup>
						{
							new ResourceGroup
							{
								Name = "Iron Ingots",
								AmountRequired = 25,
								ResourcePatterns = new List<string>
								{
									"ingot-iron"
								}
							},
							new ResourceGroup
							{
								Name = "Leather",
								AmountRequired = 15,
								ResourcePatterns = new List<string>
								{
									"leather-normal-plain"
								}
							},
							new ResourceGroup
							{
								Name = "Fire Clay",
								AmountRequired = 25,
								ResourcePatterns = new List<string>
								{
									"clay-fire"
								}
							},
							new ResourceGroup
							{
								Name = "Cupronickel Nails and Strips",
								AmountRequired = 25,
								ResourcePatterns = new List<string>
								{
									"metalnailsandstrips-cupronickel"
								}
							},
							new ResourceGroup
							{
								Name = "Flax Twine",
								AmountRequired = 20,
								ResourcePatterns = new List<string>
								{
									"flaxtwine"
								}
							},
							new ResourceGroup
							{
								Name = "Feathers",
								AmountRequired = 10,
								ResourcePatterns = new List<string>
								{
									"feathers"
								}
							},
							new ResourceGroup
							{
								Name = "Gold Ingots",
								AmountRequired = 5,
								ResourcePatterns = new List<string>
								{
									"ingot-gold"
								}
							},
							new ResourceGroup
							{
								Name = "Silver Ingots",
								AmountRequired = 5,
								ResourcePatterns = new List<string>
								{
									"ingot-silver"
								}
							},
							new ResourceGroup
							{
								Name = "Temporal Gears",
								AmountRequired = 10,
								ResourcePatterns = new List<string>
								{
									"gear-temporal"
								}
							},
							new ResourceGroup
							{
								Name = "Coke",
								AmountRequired = 40,
								ResourcePatterns = new List<string>
								{
									"coke"
								}
							}
						},
						UnlocksIds = new List<int>
						{
							6
						},
						GrantedTraits = new List<string>
						{
							"MeteoricIronAge"
						}
					},
					new TechBlock
					{
						Id = 6,
						Text = "Steel Age",
						Description = "The final stage of the game (unless..?)",
						Level = 2,
						Age = TechAge.MeteoricIron,
						ResourceGroups = new List<ResourceGroup>
						{
							new ResourceGroup
							{
								Name = "Iron Ingots",
								AmountRequired = 30,
								ResourcePatterns = new List<string>
								{
									"ingot-iron"
								}
							},
							new ResourceGroup
							{
								Name = "Meteoric Iron Ingots",
								AmountRequired = 5,
								ResourcePatterns = new List<string>
								{
									"ingot-meteoriciron"
								}
							},
							new ResourceGroup
							{
								Name = "Leather",
								AmountRequired = 20,
								ResourcePatterns = new List<string>
								{
									"leather-normal-plain"
								}
							},
							new ResourceGroup
							{
								Name = "Fire Clay",
								AmountRequired = 25,
								ResourcePatterns = new List<string>
								{
									"clay-fire"
								}
							},
							new ResourceGroup
							{
								Name = "Cupronickel Nails and Strips",
								AmountRequired = 25,
								ResourcePatterns = new List<string>
								{
									"metalnailsandstrips-cupronickel"
								}
							},
							new ResourceGroup
							{
								Name = "Flax Twine",
								AmountRequired = 25,
								ResourcePatterns = new List<string>
								{
									"flaxtwine"
								}
							},
							new ResourceGroup
							{
								Name = "Feathers",
								AmountRequired = 25,
								ResourcePatterns = new List<string>
								{
									"feathers"
								}
							},
							new ResourceGroup
							{
								Name = "Gold Ingots",
								AmountRequired = 5,
								ResourcePatterns = new List<string>
								{
									"ingot-gold"
								}
							},
							new ResourceGroup
							{
								Name = "Silver Ingots",
								AmountRequired = 5,
								ResourcePatterns = new List<string>
								{
									"ingot-silver"
								}
							},
							new ResourceGroup
							{
								Name = "Temporal Gears",
								AmountRequired = 15,
								ResourcePatterns = new List<string>
								{
									"gear-temporal"
								}
							},
							new ResourceGroup
							{
								Name = "Coke",
								AmountRequired = 64,
								ResourcePatterns = new List<string>
								{
									"coke"
								}
							},
							new ResourceGroup
							{
								Name = "Crushed Bauxite",
								AmountRequired = 10,
								ResourcePatterns = new List<string>
								{
									"crushed-bauxite"
								}
							},
							new ResourceGroup
							{
								Name = "Crushed Olivine",
								AmountRequired = 10,
								ResourcePatterns = new List<string>
								{
									"crushed-olivine"
								}
							}
						},
						UnlocksIds = new List<int>
						{
							7
						},
						GrantedTraits = new List<string>
						{
							"SteelAge"
						}
					},
					new TechBlock
					{
						Id = 7,
						Text = "Meteoric Steel Age",
						Description = "",
						Level = 2,
						Age = TechAge.Steel,
						ResourceGroups = new List<ResourceGroup>
						{
							new ResourceGroup
							{
								Name = "Steel Ingots",
								AmountRequired = 35,
								ResourcePatterns = new List<string>
								{
									"ingot-steel"
								}
							},
							new ResourceGroup
							{
								Name = "T3 Refractory Bricks",
								AmountRequired = 15,
								ResourcePatterns = new List<string>
								{
									"refractorybrick-fired-tier3"
								}
							},
							new ResourceGroup
							{
								Name = "Sturdy Leather",
								AmountRequired = 20,
								ResourcePatterns = new List<string>
								{
									"leather-normal-plain"
								}
							},
							new ResourceGroup
							{
								Name = "Cupronickel Nails and Strips",
								AmountRequired = 35,
								ResourcePatterns = new List<string>
								{
									"metalnailsandstrips-cupronickel"
								}
							},
							new ResourceGroup
							{
								Name = "Flax Twine",
								AmountRequired = 35,
								ResourcePatterns = new List<string>
								{
									"flaxtwine"
								}
							},
							new ResourceGroup
							{
								Name = "Feathers",
								AmountRequired = 35,
								ResourcePatterns = new List<string>
								{
									"feathers"
								}
							},
							new ResourceGroup
							{
								Name = "Gold Ingots",
								AmountRequired = 10,
								ResourcePatterns = new List<string>
								{
									"ingot-gold"
								}
							},
							new ResourceGroup
							{
								Name = "Silver Ingots",
								AmountRequired = 10,
								ResourcePatterns = new List<string>
								{
									"ingot-silver"
								}
							},
							new ResourceGroup
							{
								Name = "Temporal Gears",
								AmountRequired = 20,
								ResourcePatterns = new List<string>
								{
									"gear-temporal"
								}
							},
							new ResourceGroup
							{
								Name = "Coke",
								AmountRequired = 96,
								ResourcePatterns = new List<string>
								{
									"coke"
								}
							},
							new ResourceGroup
							{
								Name = "Crushed Bauxite",
								AmountRequired = 15,
								ResourcePatterns = new List<string>
								{
									"crushed-bauxite"
								}
							},
							new ResourceGroup
							{
								Name = "Crushed Olivine",
								AmountRequired = 15,
								ResourcePatterns = new List<string>
								{
									"crushed-olivine"
								}
							},
							new ResourceGroup
							{
								Name = "Crushed Ilmenite",
								AmountRequired = 15,
								ResourcePatterns = new List<string>
								{
									"crushed-ilmenite"
								}
							},
							new ResourceGroup
							{
								Name = "Platinum Ingots",
								AmountRequired = 10,
								ResourcePatterns = new List<string>
								{
									"ingot-platinum"
								}
							}
						},
						UnlocksIds = new List<int>(),
						GrantedTraits = new List<string>
						{
							"MeteoricSteelAge"
						}
					}
				}
			};
		}

		// Token: 0x060000EB RID: 235 RVA: 0x0000C230 File Offset: 0x0000A430
		public bool Validate(ICoreAPI api)
		{
			if (this.TechBlocks == null || this.TechBlocks.Count == 0)
			{
				api.Logger.Error("Tech blocks configuration is empty");
				return false;
			}
			HashSet<int> ids = new HashSet<int>();
			bool isValid = true;
			foreach (TechBlock tech in this.TechBlocks)
			{
				if (ids.Contains(tech.Id))
				{
					ILogger logger = api.Logger;
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(25, 1);
					defaultInterpolatedStringHandler.AppendLiteral("Duplicate tech block ID: ");
					defaultInterpolatedStringHandler.AppendFormatted<int>(tech.Id);
					logger.Error(defaultInterpolatedStringHandler.ToStringAndClear());
					isValid = false;
				}
				ids.Add(tech.Id);
				if (!tech.ValidateResourceRequirements())
				{
					ILogger logger2 = api.Logger;
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(47, 2);
					defaultInterpolatedStringHandler2.AppendLiteral("Invalid resource requirements in tech block ");
					defaultInterpolatedStringHandler2.AppendFormatted<int>(tech.Id);
					defaultInterpolatedStringHandler2.AppendLiteral(" (");
					defaultInterpolatedStringHandler2.AppendFormatted(tech.Text);
					defaultInterpolatedStringHandler2.AppendLiteral(")");
					logger2.Error(defaultInterpolatedStringHandler2.ToStringAndClear());
					isValid = false;
				}
				using (List<int>.Enumerator enumerator2 = tech.UnlocksIds.GetEnumerator())
				{
					while (enumerator2.MoveNext())
					{
						if (enumerator2.Current == tech.Id)
						{
							ILogger logger3 = api.Logger;
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler3 = new DefaultInterpolatedStringHandler(29, 2);
							defaultInterpolatedStringHandler3.AppendLiteral("Tech block ");
							defaultInterpolatedStringHandler3.AppendFormatted<int>(tech.Id);
							defaultInterpolatedStringHandler3.AppendLiteral(" (");
							defaultInterpolatedStringHandler3.AppendFormatted(tech.Text);
							defaultInterpolatedStringHandler3.AppendLiteral(") unlocks itself");
							logger3.Error(defaultInterpolatedStringHandler3.ToStringAndClear());
							isValid = false;
						}
					}
				}
			}
			foreach (TechBlock tech2 in this.TechBlocks)
			{
				foreach (int unlocksId in tech2.UnlocksIds)
				{
					if (!ids.Contains(unlocksId))
					{
						ILogger logger4 = api.Logger;
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler4 = new DefaultInterpolatedStringHandler(44, 3);
						defaultInterpolatedStringHandler4.AppendLiteral("Tech block ");
						defaultInterpolatedStringHandler4.AppendFormatted<int>(tech2.Id);
						defaultInterpolatedStringHandler4.AppendLiteral(" (");
						defaultInterpolatedStringHandler4.AppendFormatted(tech2.Text);
						defaultInterpolatedStringHandler4.AppendLiteral(") references non-existent tech ");
						defaultInterpolatedStringHandler4.AppendFormatted<int>(unlocksId);
						logger4.Warning(defaultInterpolatedStringHandler4.ToStringAndClear());
					}
				}
			}
			if (isValid)
			{
				api.Logger.Notification("Tech blocks configuration validated successfully");
			}
			return isValid;
		}
	}
}
