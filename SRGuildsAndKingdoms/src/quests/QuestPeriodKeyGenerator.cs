using System;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace SRGuildsAndKingdoms.src.quests
{
	// Token: 0x0200001F RID: 31
	[NullableContext(1)]
	[Nullable(0)]
	public static class QuestPeriodKeyGenerator
	{
		// Token: 0x06000183 RID: 387 RVA: 0x0000F450 File Offset: 0x0000D650
		public static string GeneratePeriodKey(QuestRecurrenceType recurrenceType, string startsAt, string expiresAt, bool usesIngameTime)
		{
			string result;
			switch (recurrenceType)
			{
			case QuestRecurrenceType.Daily:
				result = QuestPeriodKeyGenerator.GenerateDailyKey(startsAt, usesIngameTime);
				break;
			case QuestRecurrenceType.Weekly:
				result = QuestPeriodKeyGenerator.GenerateWeeklyKey(startsAt, usesIngameTime);
				break;
			case QuestRecurrenceType.Monthly:
				result = QuestPeriodKeyGenerator.GenerateMonthlyKey(startsAt, usesIngameTime);
				break;
			case QuestRecurrenceType.Seasonal:
				result = QuestPeriodKeyGenerator.GenerateSeasonalKey(startsAt, expiresAt, usesIngameTime);
				break;
			default:
				throw new ArgumentOutOfRangeException("recurrenceType");
			}
			return result;
		}

		// Token: 0x06000184 RID: 388 RVA: 0x0000F4AC File Offset: 0x0000D6AC
		private static string GenerateDailyKey(string startsAt, bool usesIngameTime)
		{
			DateTime date;
			if (!QuestPeriodKeyGenerator.TryParseDate(startsAt, out date))
			{
				throw new ArgumentException("Invalid date format: " + startsAt, "startsAt");
			}
			string prefix = usesIngameTime ? "daily_ig" : "daily";
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(3, 4);
			defaultInterpolatedStringHandler.AppendFormatted(prefix);
			defaultInterpolatedStringHandler.AppendLiteral("_");
			defaultInterpolatedStringHandler.AppendFormatted<int>(date.Year, "D4");
			defaultInterpolatedStringHandler.AppendLiteral("_");
			defaultInterpolatedStringHandler.AppendFormatted<int>(date.Month, "D2");
			defaultInterpolatedStringHandler.AppendLiteral("_");
			defaultInterpolatedStringHandler.AppendFormatted<int>(date.Day, "D2");
			return defaultInterpolatedStringHandler.ToStringAndClear();
		}

		// Token: 0x06000185 RID: 389 RVA: 0x0000F560 File Offset: 0x0000D760
		private static string GenerateWeeklyKey(string startsAt, bool usesIngameTime)
		{
			DateTime date;
			if (!QuestPeriodKeyGenerator.TryParseDate(startsAt, out date))
			{
				throw new ArgumentException("Invalid date format: " + startsAt, "startsAt");
			}
			DateTime sunday = date.AddDays((double)(-(double)date.DayOfWeek));
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(9, 3);
			defaultInterpolatedStringHandler.AppendLiteral("weekly_");
			defaultInterpolatedStringHandler.AppendFormatted<int>(sunday.Year, "D4");
			defaultInterpolatedStringHandler.AppendLiteral("_");
			defaultInterpolatedStringHandler.AppendFormatted<int>(sunday.Month, "D2");
			defaultInterpolatedStringHandler.AppendLiteral("_");
			defaultInterpolatedStringHandler.AppendFormatted<int>(sunday.Day, "D2");
			return defaultInterpolatedStringHandler.ToStringAndClear();
		}

		// Token: 0x06000186 RID: 390 RVA: 0x0000F60C File Offset: 0x0000D80C
		private static string GenerateMonthlyKey(string startsAt, bool usesIngameTime)
		{
			DateTime date;
			if (!QuestPeriodKeyGenerator.TryParseDate(startsAt, out date))
			{
				throw new ArgumentException("Invalid date format: " + startsAt, "startsAt");
			}
			string prefix = usesIngameTime ? "monthly_ig" : "monthly";
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(2, 3);
			defaultInterpolatedStringHandler.AppendFormatted(prefix);
			defaultInterpolatedStringHandler.AppendLiteral("_");
			defaultInterpolatedStringHandler.AppendFormatted<int>(date.Year, "D4");
			defaultInterpolatedStringHandler.AppendLiteral("_");
			defaultInterpolatedStringHandler.AppendFormatted<int>(date.Month, "D2");
			return defaultInterpolatedStringHandler.ToStringAndClear();
		}

		// Token: 0x06000187 RID: 391 RVA: 0x0000F6A0 File Offset: 0x0000D8A0
		private static string GenerateSeasonalKey(string startsAt, string expiresAt, bool usesIngameTime)
		{
			DateTime startDate;
			if (!QuestPeriodKeyGenerator.TryParseDate(startsAt, out startDate))
			{
				throw new ArgumentException("Invalid start date format: " + startsAt, "startsAt");
			}
			DateTime endDate;
			if (!QuestPeriodKeyGenerator.TryParseDate(expiresAt, out endDate))
			{
				throw new ArgumentException("Invalid end date format: " + expiresAt, "expiresAt");
			}
			string prefix = usesIngameTime ? "seasonal_ig" : "seasonal";
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(9, 7);
			defaultInterpolatedStringHandler.AppendFormatted(prefix);
			defaultInterpolatedStringHandler.AppendLiteral("_");
			defaultInterpolatedStringHandler.AppendFormatted<int>(startDate.Year, "D4");
			defaultInterpolatedStringHandler.AppendLiteral("_");
			defaultInterpolatedStringHandler.AppendFormatted<int>(startDate.Month, "D2");
			defaultInterpolatedStringHandler.AppendLiteral("_");
			defaultInterpolatedStringHandler.AppendFormatted<int>(startDate.Day, "D2");
			defaultInterpolatedStringHandler.AppendLiteral("_to_");
			defaultInterpolatedStringHandler.AppendFormatted<int>(endDate.Year, "D4");
			defaultInterpolatedStringHandler.AppendLiteral("_");
			defaultInterpolatedStringHandler.AppendFormatted<int>(endDate.Month, "D2");
			defaultInterpolatedStringHandler.AppendLiteral("_");
			defaultInterpolatedStringHandler.AppendFormatted<int>(endDate.Day, "D2");
			return defaultInterpolatedStringHandler.ToStringAndClear();
		}

		// Token: 0x06000188 RID: 392 RVA: 0x0000F7D0 File Offset: 0x0000D9D0
		public static string GetCurrentDailyKey()
		{
			DateTime now = QuestTimeHelper.NowEastern;
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(8, 3);
			defaultInterpolatedStringHandler.AppendLiteral("daily_");
			defaultInterpolatedStringHandler.AppendFormatted<int>(now.Year, "D4");
			defaultInterpolatedStringHandler.AppendLiteral("_");
			defaultInterpolatedStringHandler.AppendFormatted<int>(now.Month, "D2");
			defaultInterpolatedStringHandler.AppendLiteral("_");
			defaultInterpolatedStringHandler.AppendFormatted<int>(now.Day, "D2");
			return defaultInterpolatedStringHandler.ToStringAndClear();
		}

		// Token: 0x06000189 RID: 393 RVA: 0x0000F850 File Offset: 0x0000DA50
		public static string GetCurrentWeeklyKey()
		{
			DateTime now = QuestTimeHelper.NowEastern;
			DateTime sunday = now.AddDays((double)(-(double)now.DayOfWeek));
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(9, 3);
			defaultInterpolatedStringHandler.AppendLiteral("weekly_");
			defaultInterpolatedStringHandler.AppendFormatted<int>(sunday.Year, "D4");
			defaultInterpolatedStringHandler.AppendLiteral("_");
			defaultInterpolatedStringHandler.AppendFormatted<int>(sunday.Month, "D2");
			defaultInterpolatedStringHandler.AppendLiteral("_");
			defaultInterpolatedStringHandler.AppendFormatted<int>(sunday.Day, "D2");
			return defaultInterpolatedStringHandler.ToStringAndClear();
		}

		// Token: 0x0600018A RID: 394 RVA: 0x0000F8E4 File Offset: 0x0000DAE4
		public static string GetCurrentMonthlyKey()
		{
			DateTime now = QuestTimeHelper.NowEastern;
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(9, 2);
			defaultInterpolatedStringHandler.AppendLiteral("monthly_");
			defaultInterpolatedStringHandler.AppendFormatted<int>(now.Year, "D4");
			defaultInterpolatedStringHandler.AppendLiteral("_");
			defaultInterpolatedStringHandler.AppendFormatted<int>(now.Month, "D2");
			return defaultInterpolatedStringHandler.ToStringAndClear();
		}

		// Token: 0x0600018B RID: 395 RVA: 0x0000F948 File Offset: 0x0000DB48
		public static bool TryParseDate(string dateString, out DateTime date)
		{
			if (dateString.StartsWith("0000"))
			{
				string str = "0001";
				string text = dateString;
				dateString = str + text.Substring(4, text.Length - 4);
			}
			return DateTime.TryParseExact(dateString, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out date);
		}

		// Token: 0x0600018C RID: 396 RVA: 0x0000F994 File Offset: 0x0000DB94
		public static bool TryParseInGameDate(string dateString, out GameDate date)
		{
			if (dateString.StartsWith("0000"))
			{
				string str = "0001";
				string text = dateString;
				dateString = str + text.Substring(4, text.Length - 4);
			}
			return GameDate.TryParseExact(dateString, out date);
		}

		// Token: 0x0600018D RID: 397 RVA: 0x0000F9D2 File Offset: 0x0000DBD2
		public static string FormatDate(DateTime date)
		{
			return date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
		}

		// Token: 0x0600018E RID: 398 RVA: 0x0000F9E5 File Offset: 0x0000DBE5
		public static string FormatInGameDate(GameDate date)
		{
			return date.ToString();
		}

		// Token: 0x0600018F RID: 399 RVA: 0x0000F9F4 File Offset: 0x0000DBF4
		public static bool IsQuestActive(string startsAt, string expiresAt, bool usesIngameTime, GameDate? currentIngameDate = null)
		{
			DateTime startDate;
			DateTime endDate;
			if (!QuestPeriodKeyGenerator.TryParseDate(startsAt, out startDate) || !QuestPeriodKeyGenerator.TryParseDate(expiresAt, out endDate))
			{
				return false;
			}
			if (usesIngameTime)
			{
				GameDate startGameDate;
				GameDate endGameDate;
				return QuestPeriodKeyGenerator.TryParseInGameDate(startsAt, out startGameDate) && QuestPeriodKeyGenerator.TryParseInGameDate(expiresAt, out endGameDate) && currentIngameDate >= startGameDate && currentIngameDate <= endGameDate;
			}
			DateTime currentDate = QuestTimeHelper.TodayEastern;
			return currentDate >= startDate && currentDate <= endDate;
		}

		// Token: 0x0400009C RID: 156
		private const string DateFormat = "yyyy-MM-dd";
	}
}
