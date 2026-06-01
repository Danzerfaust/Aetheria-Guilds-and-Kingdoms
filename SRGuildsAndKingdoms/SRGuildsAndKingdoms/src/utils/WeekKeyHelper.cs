using System;
using System.Globalization;

namespace SRGuildsAndKingdoms.src.utils
{
    /// <summary>
    /// Helper class for generating week keys and handling EST timezone conversions
    /// Weeks start on Sunday at 12am EST
    /// </summary>
    public static class WeekKeyHelper
    {
        private static readonly TimeZoneInfo EstTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");

        public static string GenerateWeekKey(DateTime date)
        {
            var estDate = TimeZoneInfo.ConvertTime(date, EstTimeZone);

            var weekStart = GetWeekStartDate(estDate);

            var calendar = CultureInfo.CurrentCulture.Calendar;
            var weekNumber = calendar.GetWeekOfYear(weekStart, CalendarWeekRule.FirstDay, DayOfWeek.Sunday);

            var year = weekStart.Year;
            if (weekStart.Month == 1 && weekNumber >= 52)
            {
                year--;
            }
            else if (weekStart.Month == 12 && weekNumber == 1)
            {
                year++;
            }

            return $"{year}-W{weekNumber:D2}";
        }


        public static DateTime GetWeekStartDate(DateTime date)
        {
            var estDate = TimeZoneInfo.ConvertTime(date, EstTimeZone);

            var daysToSubtract = (int)estDate.DayOfWeek;

            var weekStart = estDate.Date.AddDays(-daysToSubtract);

            return weekStart;
        }

        public static long GetWeekStartUnix(DateTime date)
        {
            var weekStart = GetWeekStartDate(date);
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var utcWeekStart = TimeZoneInfo.ConvertTimeToUtc(weekStart, EstTimeZone);
            return (long)(utcWeekStart - epoch).TotalSeconds;
        }

        public static (int year, int weekNumber) ParseWeekKey(string weekKey)
        {
            if (string.IsNullOrEmpty(weekKey) || !weekKey.Contains("-W"))
            {
                throw new ArgumentException($"Invalid week key format: {weekKey}. Expected 'YYYY-Wnn'");
            }

            var parts = weekKey.Split('-');
            if (parts.Length != 2 || !parts[1].StartsWith("W"))
            {
                throw new ArgumentException($"Invalid week key format: {weekKey}. Expected 'YYYY-Wnn'");
            }

            if (!int.TryParse(parts[0], out int year) || !int.TryParse(parts[1].Substring(1), out int weekNumber))
            {
                throw new ArgumentException($"Invalid week key format: {weekKey}. Could not parse year or week number");
            }

            return (year, weekNumber);
        }
    }
}
