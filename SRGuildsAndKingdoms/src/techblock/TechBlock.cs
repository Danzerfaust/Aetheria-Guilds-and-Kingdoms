using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using Vintagestory.API.MathTools;

namespace SRGuildsAndKingdoms.src.techblock
{
	// Token: 0x02000010 RID: 16
	[NullableContext(1)]
	[Nullable(0)]
	public class TechBlock
	{
		// Token: 0x1700001F RID: 31
		// (get) Token: 0x060000C5 RID: 197 RVA: 0x0000AB0A File Offset: 0x00008D0A
		// (set) Token: 0x060000C6 RID: 198 RVA: 0x0000AB12 File Offset: 0x00008D12
		public int Id { get; set; }

		// Token: 0x17000020 RID: 32
		// (get) Token: 0x060000C7 RID: 199 RVA: 0x0000AB1B File Offset: 0x00008D1B
		// (set) Token: 0x060000C8 RID: 200 RVA: 0x0000AB23 File Offset: 0x00008D23
		public Vec2d Position { get; set; }

		// Token: 0x17000021 RID: 33
		// (get) Token: 0x060000C9 RID: 201 RVA: 0x0000AB2C File Offset: 0x00008D2C
		// (set) Token: 0x060000CA RID: 202 RVA: 0x0000AB34 File Offset: 0x00008D34
		public string Text { get; set; }

		// Token: 0x17000022 RID: 34
		// (get) Token: 0x060000CB RID: 203 RVA: 0x0000AB3D File Offset: 0x00008D3D
		// (set) Token: 0x060000CC RID: 204 RVA: 0x0000AB45 File Offset: 0x00008D45
		public int Level { get; set; } = 1;

		// Token: 0x17000023 RID: 35
		// (get) Token: 0x060000CD RID: 205 RVA: 0x0000AB4E File Offset: 0x00008D4E
		// (set) Token: 0x060000CE RID: 206 RVA: 0x0000AB56 File Offset: 0x00008D56
		[JsonConverter(typeof(JsonStringEnumConverter))]
		public TechAge Age { get; set; }

		// Token: 0x17000024 RID: 36
		// (get) Token: 0x060000CF RID: 207 RVA: 0x0000AB5F File Offset: 0x00008D5F
		// (set) Token: 0x060000D0 RID: 208 RVA: 0x0000AB67 File Offset: 0x00008D67
		[JsonPropertyName("resourceGroups")]
		public List<ResourceGroup> ResourceGroups { get; set; } = new List<ResourceGroup>();

		// Token: 0x17000025 RID: 37
		// (get) Token: 0x060000D1 RID: 209 RVA: 0x0000AB70 File Offset: 0x00008D70
		// (set) Token: 0x060000D2 RID: 210 RVA: 0x0000AB78 File Offset: 0x00008D78
		public string Description { get; set; } = "";

		// Token: 0x17000026 RID: 38
		// (get) Token: 0x060000D3 RID: 211 RVA: 0x0000AB81 File Offset: 0x00008D81
		// (set) Token: 0x060000D4 RID: 212 RVA: 0x0000AB89 File Offset: 0x00008D89
		public List<int> UnlocksIds { get; set; } = new List<int>();

		// Token: 0x17000027 RID: 39
		// (get) Token: 0x060000D5 RID: 213 RVA: 0x0000AB92 File Offset: 0x00008D92
		// (set) Token: 0x060000D6 RID: 214 RVA: 0x0000AB9A File Offset: 0x00008D9A
		[JsonPropertyName("grantedTraits")]
		public List<string> GrantedTraits { get; set; } = new List<string>();

		// Token: 0x060000D7 RID: 215 RVA: 0x0000ABA4 File Offset: 0x00008DA4
		public bool CanResearchWithItems(Dictionary<string, int> availableItems, GuildTechProgress progress = null)
		{
			if (progress != null && progress.IsUnlocked)
			{
				return false;
			}
			if (this.ResourceGroups == null || this.ResourceGroups.Count == 0)
			{
				return false;
			}
			if (availableItems == null || availableItems.Count == 0)
			{
				return false;
			}
			foreach (ResourceGroup group in this.ResourceGroups)
			{
				int submitted = (progress != null) ? progress.GetResourceGroupSubmitted(group.Name) : 0;
				int stillNeeded = group.AmountRequired - submitted;
				if (stillNeeded > 0 && group.GetTotalMatchingQuantity(availableItems) < stillNeeded)
				{
					return false;
				}
			}
			return true;
		}

		// Token: 0x060000D8 RID: 216 RVA: 0x0000AC54 File Offset: 0x00008E54
		public bool CanResearchWithItems(Dictionary<string, int> availableItems, GuildTechProgress progress, TechBlocksConfig config)
		{
			return (config == null || config.IsAgeEnabled(this.Age)) && this.CanResearchWithItems(availableItems, progress);
		}

		// Token: 0x060000D9 RID: 217 RVA: 0x0000AC74 File Offset: 0x00008E74
		public bool IsAvailableForGuild(GuildTechData guildData, List<TechBlock> allTechBlocks)
		{
			List<TechBlock> prerequisites = (from tb in allTechBlocks
			where tb.UnlocksIds.Contains(this.Id)
			select tb).ToList<TechBlock>();
			return !prerequisites.Any<TechBlock>() || prerequisites.All((TechBlock prereq) => guildData.IsTechUnlocked(prereq.Id));
		}

		// Token: 0x060000DA RID: 218 RVA: 0x0000ACC9 File Offset: 0x00008EC9
		public bool IsAvailableForGuild(GuildTechData guildData, List<TechBlock> allTechBlocks, TechBlocksConfig config)
		{
			return (config == null || config.IsAgeEnabled(this.Age)) && this.IsAvailableForGuild(guildData, allTechBlocks);
		}

		// Token: 0x060000DB RID: 219 RVA: 0x0000ACE8 File Offset: 0x00008EE8
		public Dictionary<string, int> GetPersonalRequirements()
		{
			Dictionary<string, int> personalReqs = new Dictionary<string, int>();
			if (this.ResourceGroups == null)
			{
				return personalReqs;
			}
			foreach (ResourceGroup group in this.ResourceGroups)
			{
				int personalAmount = (int)Math.Ceiling((double)group.AmountRequired * 0.05);
				personalReqs[group.Name] = personalAmount;
			}
			return personalReqs;
		}

		// Token: 0x060000DC RID: 220 RVA: 0x0000AD6C File Offset: 0x00008F6C
		public bool ValidateResourceRequirements()
		{
			if (this.ResourceGroups != null && this.ResourceGroups.Count > 0)
			{
				if (!this.ResourceGroups.All((ResourceGroup group) => group.Validate()))
				{
					return false;
				}
			}
			return true;
		}
	}
}
