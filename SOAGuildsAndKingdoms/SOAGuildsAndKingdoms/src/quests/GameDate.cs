using System;
using System.Globalization;

namespace SOAGuildsAndKingdoms.src.quests
{
    public struct GameDate(int year, int month, int day) : IComparable<GameDate>
    {
        public int Year { get; } = year;
        public int Month { get; } = month;
        public int Day { get; } = day;

        private const int DaysInMonth = 30;
        private const int MonthsInYear = 12;

        private const string DateFormat = "{0:D4}-{1:D2}-{2:D2}";

        public readonly long TotalDays => (long)Year * MonthsInYear * DaysInMonth + (Month - 1) * DaysInMonth + (Day - 1);

        public readonly int CompareTo(GameDate other)
        {
            return TotalDays.CompareTo(other.TotalDays);
        }
        public static bool TryParseExact(string input, out GameDate result)
        {
            result = default;

            if (string.IsNullOrWhiteSpace(input)) return false;

            string[] parts = input.Split('-');

            if (parts.Length == 3 &&
                int.TryParse(parts[0], out int y) &&
                int.TryParse(parts[1], out int m) &&
                int.TryParse(parts[2], out int d))
            {
                if (m >= 1 && m <= 12 && d >= 1 && d <= 30)
                {
                    result = new GameDate(y, m, d);
                    return true;
                }
            }

            return false;
        }

        public override readonly string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, DateFormat, Year, Month, Day);
        }

        public static bool operator >(GameDate a, GameDate b) => a.CompareTo(b) > 0;
        public static bool operator <(GameDate a, GameDate b) => a.CompareTo(b) < 0;
        public static bool operator >=(GameDate a, GameDate b) => a.CompareTo(b) >= 0;
        public static bool operator <=(GameDate a, GameDate b) => a.CompareTo(b) <= 0;
    }
}
