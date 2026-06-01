using System;
using System.Globalization;

namespace SRGuildsAndKingdoms.src.quests
{
    /// <summary>
    /// Utility class for generating and parsing quest period keys
    /// </summary>
    public static class QuestPeriodKeyGenerator
    {
        private const string DateFormat = "yyyy-MM-dd";

        /// <summary>
        /// Generates a period key for a quest based on its recurrence type and dates
        /// </summary>
        /// <param name="recurrenceType">The quest's recurrence type</param>
        /// <param name="startsAt">Start date string in yyyy-MM-dd format (ISO 8601)</param>
        /// <param name="expiresAt">End date string in yyyy-MM-dd format (ISO 8601)</param>
        /// <param name="usesIngameTime">Whether dates are in-game time</param>
        /// <returns>Period key string for period-locking</returns>
        public static string GeneratePeriodKey(QuestRecurrenceType recurrenceType, string startsAt, string expiresAt, bool usesIngameTime)
        {
            return recurrenceType switch
            {
                QuestRecurrenceType.Daily => GenerateDailyKey(startsAt, usesIngameTime),
                QuestRecurrenceType.Weekly => GenerateWeeklyKey(startsAt, usesIngameTime),
                QuestRecurrenceType.Monthly => GenerateMonthlyKey(startsAt, usesIngameTime),
                QuestRecurrenceType.Seasonal => GenerateSeasonalKey(startsAt, expiresAt, usesIngameTime),
                _ => throw new ArgumentOutOfRangeException(nameof(recurrenceType))
            };
        }

        /// <summary>
        /// Generates a daily period key
        /// Format: "daily_yyyy_MM_dd" (e.g., "daily_2026_04_21")
        /// </summary>
        private static string GenerateDailyKey(string startsAt, bool usesIngameTime)
        {
            if (!TryParseDate(startsAt, out var date))
                throw new ArgumentException($"Invalid date format: {startsAt}", nameof(startsAt));

            var prefix = usesIngameTime ? "daily_ig" : "daily";
            return $"{prefix}_{date.Year:D4}_{date.Month:D2}_{date.Day:D2}";
        }

        /// <summary>
        /// Generates a weekly period key based on the Sunday of that week
        /// Format: "weekly_MM_dd" (e.g., "weekly_04_03")
        /// </summary>
        private static string GenerateWeeklyKey(string startsAt, bool usesIngameTime)
        {
            if (!TryParseDate(startsAt, out var date))
                throw new ArgumentException($"Invalid date format: {startsAt}", nameof(startsAt));

            // Find the Sunday of this week (start of week)
            var sunday = date.AddDays(-(int)date.DayOfWeek);

            return $"weekly_{sunday.Year:D4}_{sunday.Month:D2}_{sunday.Day:D2}";
        }

        /// <summary>
        /// Generates a monthly period key
        /// Format: "monthly_yyyy_MM" (e.g., "monthly_2026_04")
        /// </summary>
        private static string GenerateMonthlyKey(string startsAt, bool usesIngameTime)
        {
            if (!TryParseDate(startsAt, out var date))
                throw new ArgumentException($"Invalid date format: {startsAt}", nameof(startsAt));

            var prefix = usesIngameTime ? "monthly_ig" : "monthly";
            return $"{prefix}_{date.Year:D4}_{date.Month:D2}";
        }

        /// <summary>
        /// Generates a seasonal period key based on start and end dates
        /// Format: "seasonal_yyyy_MM_dd_to_yyyy_MM_dd" (e.g., "seasonal_2026_12_01_to_2027_01_01")
        /// </summary>
        private static string GenerateSeasonalKey(string startsAt, string expiresAt, bool usesIngameTime)
        {
            if (!TryParseDate(startsAt, out var startDate))
                throw new ArgumentException($"Invalid start date format: {startsAt}", nameof(startsAt));

            if (!TryParseDate(expiresAt, out var endDate))
                throw new ArgumentException($"Invalid end date format: {expiresAt}", nameof(expiresAt));

            var prefix = usesIngameTime ? "seasonal_ig" : "seasonal";
            return $"{prefix}_{startDate.Year:D4}_{startDate.Month:D2}_{startDate.Day:D2}_to_{endDate.Year:D4}_{endDate.Month:D2}_{endDate.Day:D2}";
        }

        /// <summary>
        /// Tries to parse a date string in yyyy-MM-dd format.
        /// Handles year 0000 (IGT quests) by converting to year 0001 for DateTime compatibility.
        /// </summary>
        public static bool TryParseDate(string dateString, out DateTime date)
        {
            // Handle year 0000 for IGT quests (DateTime doesn't support year 0000)
            if (dateString.StartsWith("0000"))
            {
                dateString = "0001" + dateString[4..]; // Replace year 0000 with 0001
            }

            return DateTime.TryParseExact(
                dateString,
                DateFormat,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out date);
        }

        public static bool TryParseInGameDate(string dateString, out GameDate date)
        {
            if (dateString.StartsWith("0000"))
            {
                dateString = "0001" + dateString[4..];
            }

            return GameDate.TryParseExact(
                dateString,
                out date
            );
        }

        /// <summary>
        /// Formats a DateTime to the standard date string format (yyyy-MM-dd / ISO 8601)
        /// </summary>
        public static string FormatDate(DateTime date)
        {
            return date.ToString(DateFormat, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Formats a DateTime to the standard date string format (yyyy-MM-dd / ISO 8601)
        /// </summary>
        public static string FormatInGameDate(GameDate date)
        {
            return date.ToString();
        }

        /// <summary>
        /// Checks if a quest is currently active based on its dates and time mode
        /// </summary>
        /// <param name="startsAt">Quest start date</param>
        /// <param name="expiresAt">Quest expiration date</param>
        /// <param name="usesIngameTime">Whether to use in-game time</param>
        /// <param name="currentIngameDate">Current in-game date (only used if usesIngameTime is true)</param>
        public static bool IsQuestActive(string startsAt, string expiresAt, bool usesIngameTime, GameDate? currentIngameDate = null)
        {
            if (!TryParseDate(startsAt, out var startDate) || !TryParseDate(expiresAt, out var endDate))
                return false;

            if (usesIngameTime)
            {
                if (!TryParseInGameDate(startsAt, out var startGameDate) || !TryParseInGameDate(expiresAt, out var endGameDate))
                    return false;

                return currentIngameDate >= startGameDate && currentIngameDate <= endGameDate;
            }
            else
            {
                DateTime currentDate = QuestTimeHelper.TodayEastern;
                return currentDate >= startDate && currentDate <= endDate;
            }
        }
    }
}
