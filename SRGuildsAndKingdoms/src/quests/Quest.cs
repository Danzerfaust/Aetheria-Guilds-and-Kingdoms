using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SRGuildsAndKingdoms.src.quests
{
	// Token: 0x02000018 RID: 24
	[NullableContext(1)]
	[Nullable(0)]
	public class Quest
	{
		// Token: 0x17000039 RID: 57
		// (get) Token: 0x06000115 RID: 277 RVA: 0x0000C92F File Offset: 0x0000AB2F
		// (set) Token: 0x06000116 RID: 278 RVA: 0x0000C937 File Offset: 0x0000AB37
		public int Id { get; set; }

		// Token: 0x1700003A RID: 58
		// (get) Token: 0x06000117 RID: 279 RVA: 0x0000C940 File Offset: 0x0000AB40
		// (set) Token: 0x06000118 RID: 280 RVA: 0x0000C948 File Offset: 0x0000AB48
		[JsonConverter(typeof(JsonStringEnumConverter<QuestRecurrenceType>))]
		public QuestRecurrenceType RecurrenceType { get; set; }

		// Token: 0x1700003B RID: 59
		// (get) Token: 0x06000119 RID: 281 RVA: 0x0000C951 File Offset: 0x0000AB51
		// (set) Token: 0x0600011A RID: 282 RVA: 0x0000C959 File Offset: 0x0000AB59
		public string Title { get; set; } = string.Empty;

		// Token: 0x1700003C RID: 60
		// (get) Token: 0x0600011B RID: 283 RVA: 0x0000C962 File Offset: 0x0000AB62
		// (set) Token: 0x0600011C RID: 284 RVA: 0x0000C96A File Offset: 0x0000AB6A
		public string Description { get; set; } = string.Empty;

		// Token: 0x1700003D RID: 61
		// (get) Token: 0x0600011D RID: 285 RVA: 0x0000C973 File Offset: 0x0000AB73
		// (set) Token: 0x0600011E RID: 286 RVA: 0x0000C97B File Offset: 0x0000AB7B
		public List<QuestObjective> Objectives { get; set; } = new List<QuestObjective>();

		// Token: 0x1700003E RID: 62
		// (get) Token: 0x0600011F RID: 287 RVA: 0x0000C984 File Offset: 0x0000AB84
		// (set) Token: 0x06000120 RID: 288 RVA: 0x0000C98C File Offset: 0x0000AB8C
		public List<QuestReward> Rewards { get; set; } = new List<QuestReward>();

		// Token: 0x1700003F RID: 63
		// (get) Token: 0x06000121 RID: 289 RVA: 0x0000C995 File Offset: 0x0000AB95
		// (set) Token: 0x06000122 RID: 290 RVA: 0x0000C99D File Offset: 0x0000AB9D
		public string StartsAt { get; set; } = string.Empty;

		// Token: 0x17000040 RID: 64
		// (get) Token: 0x06000123 RID: 291 RVA: 0x0000C9A6 File Offset: 0x0000ABA6
		// (set) Token: 0x06000124 RID: 292 RVA: 0x0000C9AE File Offset: 0x0000ABAE
		public string ExpiresAt { get; set; } = string.Empty;

		// Token: 0x17000041 RID: 65
		// (get) Token: 0x06000125 RID: 293 RVA: 0x0000C9B7 File Offset: 0x0000ABB7
		// (set) Token: 0x06000126 RID: 294 RVA: 0x0000C9BF File Offset: 0x0000ABBF
		public bool UsesIngameTime { get; set; }

		// Token: 0x17000042 RID: 66
		// (get) Token: 0x06000127 RID: 295 RVA: 0x0000C9C8 File Offset: 0x0000ABC8
		// (set) Token: 0x06000128 RID: 296 RVA: 0x0000C9D0 File Offset: 0x0000ABD0
		public bool Repeat { get; set; }

		// Token: 0x17000043 RID: 67
		// (get) Token: 0x06000129 RID: 297 RVA: 0x0000C9D9 File Offset: 0x0000ABD9
		// (set) Token: 0x0600012A RID: 298 RVA: 0x0000C9E1 File Offset: 0x0000ABE1
		public long CreatedAt { get; set; }

		// Token: 0x0600012B RID: 299 RVA: 0x0000C9EA File Offset: 0x0000ABEA
		public string GeneratePeriodKey()
		{
			return QuestPeriodKeyGenerator.GeneratePeriodKey(this.RecurrenceType, this.StartsAt, this.ExpiresAt, this.UsesIngameTime);
		}

		// Token: 0x0600012C RID: 300 RVA: 0x0000CA09 File Offset: 0x0000AC09
		public string SerializeRequirements()
		{
			return JsonSerializer.Serialize<QuestRequirementsJson>(new QuestRequirementsJson
			{
				Objectives = this.Objectives
			}, QuestJsonContext.Default.QuestRequirementsJson);
		}

		// Token: 0x0600012D RID: 301 RVA: 0x0000CA2C File Offset: 0x0000AC2C
		public static List<QuestObjective> DeserializeRequirements(string json)
		{
			if (string.IsNullOrWhiteSpace(json))
			{
				return new List<QuestObjective>();
			}
			List<QuestObjective> result;
			try
			{
				QuestRequirementsJson questRequirementsJson = JsonSerializer.Deserialize<QuestRequirementsJson>(json, QuestJsonContext.Default.QuestRequirementsJson);
				result = (((questRequirementsJson != null) ? questRequirementsJson.Objectives : null) ?? new List<QuestObjective>());
			}
			catch
			{
				result = new List<QuestObjective>();
			}
			return result;
		}

		// Token: 0x0600012E RID: 302 RVA: 0x0000CA8C File Offset: 0x0000AC8C
		public string SerializeRewards()
		{
			return JsonSerializer.Serialize<QuestRewardsJson>(new QuestRewardsJson
			{
				Rewards = this.Rewards
			}, QuestJsonContext.Default.QuestRewardsJson);
		}

		// Token: 0x0600012F RID: 303 RVA: 0x0000CAB0 File Offset: 0x0000ACB0
		public static List<QuestReward> DeserializeRewards(string json)
		{
			if (string.IsNullOrWhiteSpace(json))
			{
				return new List<QuestReward>();
			}
			List<QuestReward> result;
			try
			{
				QuestRewardsJson questRewardsJson = JsonSerializer.Deserialize<QuestRewardsJson>(json, QuestJsonContext.Default.QuestRewardsJson);
				result = (((questRewardsJson != null) ? questRewardsJson.Rewards : null) ?? new List<QuestReward>());
			}
			catch
			{
				result = new List<QuestReward>();
			}
			return result;
		}
	}
}
