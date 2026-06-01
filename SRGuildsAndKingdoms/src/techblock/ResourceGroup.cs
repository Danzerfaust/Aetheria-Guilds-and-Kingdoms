using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace SRGuildsAndKingdoms.src.techblock
{
	// Token: 0x0200000E RID: 14
	[NullableContext(1)]
	[Nullable(0)]
	public class ResourceGroup
	{
		// Token: 0x1700001C RID: 28
		// (get) Token: 0x060000B2 RID: 178 RVA: 0x0000A51E File Offset: 0x0000871E
		// (set) Token: 0x060000B3 RID: 179 RVA: 0x0000A526 File Offset: 0x00008726
		[JsonPropertyName("name")]
		public string Name { get; set; }

		// Token: 0x1700001D RID: 29
		// (get) Token: 0x060000B4 RID: 180 RVA: 0x0000A52F File Offset: 0x0000872F
		// (set) Token: 0x060000B5 RID: 181 RVA: 0x0000A537 File Offset: 0x00008737
		[JsonPropertyName("amountRequired")]
		public int AmountRequired { get; set; }

		// Token: 0x1700001E RID: 30
		// (get) Token: 0x060000B6 RID: 182 RVA: 0x0000A540 File Offset: 0x00008740
		// (set) Token: 0x060000B7 RID: 183 RVA: 0x0000A548 File Offset: 0x00008748
		[JsonPropertyName("resourcePatterns")]
		public List<string> ResourcePatterns { get; set; } = new List<string>();

		// Token: 0x060000B8 RID: 184 RVA: 0x0000A554 File Offset: 0x00008754
		public bool DoesItemMatch(string itemName)
		{
			if (string.IsNullOrEmpty(itemName) || this.ResourcePatterns == null || this.ResourcePatterns.Count == 0)
			{
				return false;
			}
			int colonIndex = itemName.IndexOf(':');
			if (colonIndex >= 0 && colonIndex < itemName.Length - 1)
			{
				itemName = itemName.Substring(colonIndex + 1);
			}
			return this.ResourcePatterns.Any((string pattern) => ResourceMatcher.DoesItemMatchResource(itemName, pattern));
		}

		// Token: 0x060000B9 RID: 185 RVA: 0x0000A5E0 File Offset: 0x000087E0
		public Dictionary<string, int> GetMatchingItems(Dictionary<string, int> availableItems)
		{
			if (availableItems == null || this.ResourcePatterns == null || this.ResourcePatterns.Count == 0)
			{
				return new Dictionary<string, int>();
			}
			Dictionary<string, int> matchingItems = new Dictionary<string, int>();
			foreach (KeyValuePair<string, int> item in availableItems)
			{
				if (this.DoesItemMatch(item.Key))
				{
					matchingItems[item.Key] = item.Value;
				}
			}
			return matchingItems;
		}

		// Token: 0x060000BA RID: 186 RVA: 0x0000A670 File Offset: 0x00008870
		public int GetTotalMatchingQuantity(Dictionary<string, int> availableItems)
		{
			return this.GetMatchingItems(availableItems).Sum((KeyValuePair<string, int> item) => item.Value);
		}

		// Token: 0x060000BB RID: 187 RVA: 0x0000A6A0 File Offset: 0x000088A0
		public bool Validate()
		{
			if (string.IsNullOrWhiteSpace(this.Name))
			{
				return false;
			}
			if (this.AmountRequired <= 0)
			{
				return false;
			}
			if (this.ResourcePatterns == null || this.ResourcePatterns.Count == 0)
			{
				return false;
			}
			return this.ResourcePatterns.All((string pattern) => ResourceMatcher.IsValidResourceRequirement(pattern));
		}
	}
}
