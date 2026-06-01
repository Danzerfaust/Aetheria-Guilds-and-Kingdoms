using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SRGuildsAndKingdoms.src.quests
{
	// Token: 0x02000014 RID: 20
	[NullableContext(1)]
	[Nullable(0)]
	public class PlayerQuestProgress
	{
		// Token: 0x1700002F RID: 47
		// (get) Token: 0x060000F9 RID: 249 RVA: 0x0000C6D1 File Offset: 0x0000A8D1
		// (set) Token: 0x060000FA RID: 250 RVA: 0x0000C6D9 File Offset: 0x0000A8D9
		public string PlayerUid { get; set; } = string.Empty;

		// Token: 0x17000030 RID: 48
		// (get) Token: 0x060000FB RID: 251 RVA: 0x0000C6E2 File Offset: 0x0000A8E2
		// (set) Token: 0x060000FC RID: 252 RVA: 0x0000C6EA File Offset: 0x0000A8EA
		public int QuestId { get; set; }

		// Token: 0x17000031 RID: 49
		// (get) Token: 0x060000FD RID: 253 RVA: 0x0000C6F3 File Offset: 0x0000A8F3
		// (set) Token: 0x060000FE RID: 254 RVA: 0x0000C6FB File Offset: 0x0000A8FB
		[JsonConverter(typeof(JsonStringEnumConverter))]
		public PlayerQuestStatus Status { get; set; }

		// Token: 0x17000032 RID: 50
		// (get) Token: 0x060000FF RID: 255 RVA: 0x0000C704 File Offset: 0x0000A904
		// (set) Token: 0x06000100 RID: 256 RVA: 0x0000C70C File Offset: 0x0000A90C
		public Dictionary<int, ObjectiveProgress> ObjectiveProgress { get; set; } = new Dictionary<int, ObjectiveProgress>();

		// Token: 0x17000033 RID: 51
		// (get) Token: 0x06000101 RID: 257 RVA: 0x0000C715 File Offset: 0x0000A915
		// (set) Token: 0x06000102 RID: 258 RVA: 0x0000C71D File Offset: 0x0000A91D
		[JsonConverter(typeof(JsonStringEnumConverter))]
		public QuestRecurrenceType RecurrenceType { get; set; }

		// Token: 0x17000034 RID: 52
		// (get) Token: 0x06000103 RID: 259 RVA: 0x0000C726 File Offset: 0x0000A926
		// (set) Token: 0x06000104 RID: 260 RVA: 0x0000C72E File Offset: 0x0000A92E
		[Nullable(2)]
		public string PeriodKey { [NullableContext(2)] get; [NullableContext(2)] set; }

		// Token: 0x17000035 RID: 53
		// (get) Token: 0x06000105 RID: 261 RVA: 0x0000C737 File Offset: 0x0000A937
		// (set) Token: 0x06000106 RID: 262 RVA: 0x0000C73F File Offset: 0x0000A93F
		public long StartedAt { get; set; }

		// Token: 0x17000036 RID: 54
		// (get) Token: 0x06000107 RID: 263 RVA: 0x0000C748 File Offset: 0x0000A948
		// (set) Token: 0x06000108 RID: 264 RVA: 0x0000C750 File Offset: 0x0000A950
		public long? CompletedAt { get; set; }

		// Token: 0x06000109 RID: 265 RVA: 0x0000C75C File Offset: 0x0000A95C
		public bool AreAllObjectivesComplete(List<QuestObjective> questObjectives)
		{
			foreach (QuestObjective objective in questObjectives)
			{
				ObjectiveProgress progress;
				if (!this.ObjectiveProgress.TryGetValue(objective.Id, out progress))
				{
					return false;
				}
				if (progress.Current < objective.Count)
				{
					return false;
				}
			}
			return true;
		}

		// Token: 0x0600010A RID: 266 RVA: 0x0000C7D4 File Offset: 0x0000A9D4
		public int GetObjectiveProgress(int objectiveId)
		{
			ObjectiveProgress progress;
			if (!this.ObjectiveProgress.TryGetValue(objectiveId, out progress))
			{
				return 0;
			}
			return progress.Current;
		}

		// Token: 0x0600010B RID: 267 RVA: 0x0000C7FC File Offset: 0x0000A9FC
		public int AddObjectiveProgress(int objectiveId, int amount, int maxCount)
		{
			ObjectiveProgress progress;
			if (!this.ObjectiveProgress.TryGetValue(objectiveId, out progress))
			{
				progress = new ObjectiveProgress();
				this.ObjectiveProgress[objectiveId] = progress;
			}
			int remaining = maxCount - progress.Current;
			int actualAdded = Math.Min(amount, remaining);
			if (actualAdded > 0)
			{
				progress.Current += actualAdded;
			}
			return actualAdded;
		}

		// Token: 0x0600010C RID: 268 RVA: 0x0000C850 File Offset: 0x0000AA50
		public string SerializeProgress()
		{
			return JsonSerializer.Serialize<QuestProgressJson>(new QuestProgressJson
			{
				Objectives = this.ObjectiveProgress
			}, QuestJsonContext.Default.QuestProgressJson);
		}

		// Token: 0x0600010D RID: 269 RVA: 0x0000C874 File Offset: 0x0000AA74
		public static Dictionary<int, ObjectiveProgress> DeserializeProgress([Nullable(2)] string json)
		{
			if (string.IsNullOrWhiteSpace(json))
			{
				return new Dictionary<int, ObjectiveProgress>();
			}
			Dictionary<int, ObjectiveProgress> result;
			try
			{
				QuestProgressJson questProgressJson = JsonSerializer.Deserialize<QuestProgressJson>(json, QuestJsonContext.Default.QuestProgressJson);
				result = (((questProgressJson != null) ? questProgressJson.Objectives : null) ?? new Dictionary<int, ObjectiveProgress>());
			}
			catch
			{
				result = new Dictionary<int, ObjectiveProgress>();
			}
			return result;
		}
	}
}
