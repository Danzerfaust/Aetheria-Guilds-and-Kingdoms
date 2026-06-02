using System;

namespace SOAGuildsAndKingdoms.src.quests
{
    /// <summary>
    /// Helper class for quest-related time operations.
    /// All quest times use Eastern Time (ET) for consistency.
    /// </summary>
    public static class QuestTimeHelper
    {
        private static readonly TimeZoneInfo EasternTimeZone;

        static QuestTimeHelper()
        {
            // Eastern Time works on both Windows and Linux
            // Windows: "Eastern Standard Time"
            // Linux/macOS: "America/New_York"
            try
            {
                EasternTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
            }
            catch (TimeZoneNotFoundException)
            {
                // Fallback for Linux/macOS
                EasternTimeZone = TimeZoneInfo.FindSystemTimeZoneById("America/New_York");
            }
        }

        /// <summary>
        /// Gets the current time in Eastern Time zone.
        /// </summary>
        public static DateTime NowEastern => TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, EasternTimeZone);

        /// <summary>
        /// Gets the current date in Eastern Time zone (time component is midnight).
        /// </summary>
        public static DateTime TodayEastern => NowEastern.Date;

        /// <summary>
        /// Gets the current DateTimeOffset in Eastern Time zone.
        /// </summary>
        public static DateTimeOffset NowEasternOffset
        {
            get
            {
                var utcNow = DateTimeOffset.UtcNow;
                var easternTime = TimeZoneInfo.ConvertTime(utcNow, EasternTimeZone);
                return easternTime;
            }
        }

        /// <summary>
        /// Gets the Eastern Time zone offset in hours from UTC.
        /// This value changes based on DST (-5 for EST, -4 for EDT).
        /// </summary>
        public static double EasternTimezoneOffsetHours => EasternTimeZone.GetUtcOffset(DateTime.UtcNow).TotalHours;
    }
}
