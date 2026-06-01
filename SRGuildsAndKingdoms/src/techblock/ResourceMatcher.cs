using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace SRGuildsAndKingdoms.src.techblock
{
	// Token: 0x0200000F RID: 15
	[NullableContext(1)]
	[Nullable(0)]
	public static class ResourceMatcher
	{
		// Token: 0x060000BD RID: 189 RVA: 0x0000A71C File Offset: 0x0000891C
		public static bool DoesItemMatchResource(string itemName, string resourceRequirement)
		{
			if (string.IsNullOrEmpty(itemName) || string.IsNullOrEmpty(resourceRequirement))
			{
				return false;
			}
			string normalizedItem = ResourceMatcher.NormalizeString(itemName);
			string normalizedRequirement = ResourceMatcher.NormalizeString(resourceRequirement);
			if (normalizedRequirement.Contains(","))
			{
				return (from type in normalizedRequirement.Split(',', StringSplitOptions.None)
				select type.Trim() into type
				where !string.IsNullOrEmpty(type)
				select type).Any((string type) => ResourceMatcher.DoesItemMatchSingleResource(normalizedItem, type));
			}
			return ResourceMatcher.DoesItemMatchSingleResource(normalizedItem, normalizedRequirement);
		}

		// Token: 0x060000BE RID: 190 RVA: 0x0000A7D0 File Offset: 0x000089D0
		private static bool DoesItemMatchSingleResource(string normalizedItem, string normalizedResource)
		{
			if (normalizedResource.Contains("*"))
			{
				return ResourceMatcher.MatchesWildcardPattern(normalizedItem, normalizedResource);
			}
			return normalizedItem == normalizedResource || (normalizedItem.Contains(normalizedResource) || normalizedResource.Contains(normalizedItem));
		}

		// Token: 0x060000BF RID: 191 RVA: 0x0000A808 File Offset: 0x00008A08
		private static bool MatchesWildcardPattern(string item, string pattern)
		{
			if (pattern == "*")
			{
				return true;
			}
			if (pattern.StartsWith("*") && pattern.EndsWith("*"))
			{
				string middle = pattern.Substring(1, pattern.Length - 2);
				if (string.IsNullOrEmpty(middle))
				{
					return true;
				}
				if (middle.Contains("*"))
				{
					return ResourceMatcher.MatchesWildcardRecursive(item, pattern);
				}
				return item.Contains(middle);
			}
			else if (pattern.StartsWith("*"))
			{
				string suffix = pattern.Substring(1);
				if (string.IsNullOrEmpty(suffix))
				{
					return true;
				}
				if (suffix.Contains("*"))
				{
					return ResourceMatcher.MatchesWildcardRecursive(item, pattern);
				}
				return item.EndsWith(suffix);
			}
			else
			{
				if (!pattern.EndsWith("*"))
				{
					return pattern.Contains("*") && ResourceMatcher.MatchesWildcardRecursive(item, pattern);
				}
				string prefix = pattern.Substring(0, pattern.Length - 1);
				if (string.IsNullOrEmpty(prefix))
				{
					return true;
				}
				if (prefix.Contains("*"))
				{
					return ResourceMatcher.MatchesWildcardRecursive(item, pattern);
				}
				return item.StartsWith(prefix);
			}
		}

		// Token: 0x060000C0 RID: 192 RVA: 0x0000A90C File Offset: 0x00008B0C
		private static bool MatchesWildcardRecursive(string text, string pattern)
		{
			int textIndex = 0;
			int patternIndex = 0;
			int starIndex = -1;
			int matchIndex = 0;
			while (textIndex < text.Length)
			{
				if (patternIndex < pattern.Length && pattern[patternIndex] == '*')
				{
					starIndex = patternIndex;
					matchIndex = textIndex;
					patternIndex++;
				}
				else if (patternIndex < pattern.Length && pattern[patternIndex] == text[textIndex])
				{
					textIndex++;
					patternIndex++;
				}
				else
				{
					if (starIndex == -1)
					{
						return false;
					}
					patternIndex = starIndex + 1;
					matchIndex++;
					textIndex = matchIndex;
				}
			}
			while (patternIndex < pattern.Length && pattern[patternIndex] == '*')
			{
				patternIndex++;
			}
			return patternIndex == pattern.Length;
		}

		// Token: 0x060000C1 RID: 193 RVA: 0x0000A9A4 File Offset: 0x00008BA4
		private static string NormalizeString(string input)
		{
			return ((input != null) ? input.ToLower().Replace(" ", "").Replace("_", "").Replace("-", "") : null) ?? string.Empty;
		}

		// Token: 0x060000C2 RID: 194 RVA: 0x0000A9F4 File Offset: 0x00008BF4
		public static Dictionary<string, int> GetMatchingItems(Dictionary<string, int> availableItems, string resourceRequirement)
		{
			if (availableItems == null || string.IsNullOrEmpty(resourceRequirement))
			{
				return new Dictionary<string, int>();
			}
			return (from item in availableItems
			where ResourceMatcher.DoesItemMatchResource(item.Key, resourceRequirement)
			select item).ToDictionary((KeyValuePair<string, int> item) => item.Key, (KeyValuePair<string, int> item) => item.Value);
		}

		// Token: 0x060000C3 RID: 195 RVA: 0x0000AA79 File Offset: 0x00008C79
		public static int GetTotalMatchingQuantity(Dictionary<string, int> availableItems, string resourceRequirement)
		{
			return ResourceMatcher.GetMatchingItems(availableItems, resourceRequirement).Sum((KeyValuePair<string, int> item) => item.Value);
		}

		// Token: 0x060000C4 RID: 196 RVA: 0x0000AAA8 File Offset: 0x00008CA8
		public static bool IsValidResourceRequirement(string resourceRequirement)
		{
			if (string.IsNullOrWhiteSpace(resourceRequirement))
			{
				return false;
			}
			if (resourceRequirement.Contains("**"))
			{
				return false;
			}
			if (resourceRequirement.Contains(","))
			{
				return resourceRequirement.Split(',', StringSplitOptions.None).All((string part) => !string.IsNullOrWhiteSpace(part.Trim()));
			}
			return true;
		}
	}
}
