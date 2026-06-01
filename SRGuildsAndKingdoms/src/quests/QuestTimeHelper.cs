using System;
using System.Runtime.CompilerServices;

namespace SRGuildsAndKingdoms.src.quests
{
	// Token: 0x02000022 RID: 34
	public static class QuestTimeHelper
	{
		// Token: 0x06000197 RID: 407 RVA: 0x0000FAD8 File Offset: 0x0000DCD8
		static QuestTimeHelper()
		{
			try
			{
				QuestTimeHelper.EasternTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
			}
			catch (TimeZoneNotFoundException)
			{
				QuestTimeHelper.EasternTimeZone = TimeZoneInfo.FindSystemTimeZoneById("America/New_York");
			}
		}

		// Token: 0x17000062 RID: 98
		// (get) Token: 0x06000198 RID: 408 RVA: 0x0000FB18 File Offset: 0x0000DD18
		public static DateTime NowEastern
		{
			get
			{
				return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, QuestTimeHelper.EasternTimeZone);
			}
		}

		// Token: 0x17000063 RID: 99
		// (get) Token: 0x06000199 RID: 409 RVA: 0x0000FB2C File Offset: 0x0000DD2C
		public static DateTime TodayEastern
		{
			get
			{
				return QuestTimeHelper.NowEastern.Date;
			}
		}

		// Token: 0x17000064 RID: 100
		// (get) Token: 0x0600019A RID: 410 RVA: 0x0000FB46 File Offset: 0x0000DD46
		public static DateTimeOffset NowEasternOffset
		{
			get
			{
				return TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, QuestTimeHelper.EasternTimeZone);
			}
		}

		// Token: 0x17000065 RID: 101
		// (get) Token: 0x0600019B RID: 411 RVA: 0x0000FB58 File Offset: 0x0000DD58
		public static double EasternTimezoneOffsetHours
		{
			get
			{
				return QuestTimeHelper.EasternTimeZone.GetUtcOffset(DateTime.UtcNow).TotalHours;
			}
		}

		// Token: 0x040000A1 RID: 161
		[Nullable(1)]
		private static readonly TimeZoneInfo EasternTimeZone;
	}
}
