using System;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace SRGuildsAndKingdoms.src.quests
{
	// Token: 0x02000013 RID: 19
	[NullableContext(1)]
	[Nullable(0)]
	public struct GameDate : IComparable<GameDate>
	{
		// Token: 0x060000ED RID: 237 RVA: 0x0000C573 File Offset: 0x0000A773
		public GameDate(int year, int month, int day)
		{
			this.Year = year;
			this.Month = month;
			this.Day = day;
		}

		// Token: 0x1700002B RID: 43
		// (get) Token: 0x060000EE RID: 238 RVA: 0x0000C58A File Offset: 0x0000A78A
		public readonly int Year { get; }

		// Token: 0x1700002C RID: 44
		// (get) Token: 0x060000EF RID: 239 RVA: 0x0000C592 File Offset: 0x0000A792
		public readonly int Month { get; }

		// Token: 0x1700002D RID: 45
		// (get) Token: 0x060000F0 RID: 240 RVA: 0x0000C59A File Offset: 0x0000A79A
		public readonly int Day { get; }

		// Token: 0x1700002E RID: 46
		// (get) Token: 0x060000F1 RID: 241 RVA: 0x0000C5A2 File Offset: 0x0000A7A2
		public readonly long TotalDays
		{
			get
			{
				return (long)this.Year * 12L * 30L + (long)((this.Month - 1) * 30) + (long)(this.Day - 1);
			}
		}

		// Token: 0x060000F2 RID: 242 RVA: 0x0000C5CC File Offset: 0x0000A7CC
		public readonly int CompareTo(GameDate other)
		{
			return this.TotalDays.CompareTo(other.TotalDays);
		}

		// Token: 0x060000F3 RID: 243 RVA: 0x0000C5F0 File Offset: 0x0000A7F0
		public static bool TryParseExact(string input, out GameDate result)
		{
			result = default(GameDate);
			if (string.IsNullOrWhiteSpace(input))
			{
				return false;
			}
			string[] parts = input.Split('-', StringSplitOptions.None);
			int y;
			int i;
			int d;
			if (parts.Length == 3 && int.TryParse(parts[0], out y) && int.TryParse(parts[1], out i) && int.TryParse(parts[2], out d) && i >= 1 && i <= 12 && d >= 1 && d <= 30)
			{
				result = new GameDate(y, i, d);
				return true;
			}
			return false;
		}

		// Token: 0x060000F4 RID: 244 RVA: 0x0000C665 File Offset: 0x0000A865
		public override readonly string ToString()
		{
			return string.Format(CultureInfo.InvariantCulture, "{0:D4}-{1:D2}-{2:D2}", this.Year, this.Month, this.Day);
		}

		// Token: 0x060000F5 RID: 245 RVA: 0x0000C697 File Offset: 0x0000A897
		public static bool operator >(GameDate a, GameDate b)
		{
			return a.CompareTo(b) > 0;
		}

		// Token: 0x060000F6 RID: 246 RVA: 0x0000C6A4 File Offset: 0x0000A8A4
		public static bool operator <(GameDate a, GameDate b)
		{
			return a.CompareTo(b) < 0;
		}

		// Token: 0x060000F7 RID: 247 RVA: 0x0000C6B1 File Offset: 0x0000A8B1
		public static bool operator >=(GameDate a, GameDate b)
		{
			return a.CompareTo(b) >= 0;
		}

		// Token: 0x060000F8 RID: 248 RVA: 0x0000C6C1 File Offset: 0x0000A8C1
		public static bool operator <=(GameDate a, GameDate b)
		{
			return a.CompareTo(b) <= 0;
		}

		// Token: 0x04000053 RID: 83
		private const int DaysInMonth = 30;

		// Token: 0x04000054 RID: 84
		private const int MonthsInYear = 12;

		// Token: 0x04000055 RID: 85
		private const string DateFormat = "{0:D4}-{1:D2}-{2:D2}";
	}
}
