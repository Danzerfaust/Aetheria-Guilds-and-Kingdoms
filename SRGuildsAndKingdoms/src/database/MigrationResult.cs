using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace SRGuildsAndKingdoms.src.database
{
	// Token: 0x020000B7 RID: 183
	[NullableContext(1)]
	[Nullable(0)]
	public class MigrationResult
	{
		// Token: 0x17000222 RID: 546
		// (get) Token: 0x0600086A RID: 2154 RVA: 0x0003E360 File Offset: 0x0003C560
		// (set) Token: 0x0600086B RID: 2155 RVA: 0x0003E368 File Offset: 0x0003C568
		public bool Success { get; set; }

		// Token: 0x17000223 RID: 547
		// (get) Token: 0x0600086C RID: 2156 RVA: 0x0003E371 File Offset: 0x0003C571
		// (set) Token: 0x0600086D RID: 2157 RVA: 0x0003E379 File Offset: 0x0003C579
		public DateTime StartTime { get; set; }

		// Token: 0x17000224 RID: 548
		// (get) Token: 0x0600086E RID: 2158 RVA: 0x0003E382 File Offset: 0x0003C582
		// (set) Token: 0x0600086F RID: 2159 RVA: 0x0003E38A File Offset: 0x0003C58A
		public DateTime EndTime { get; set; }

		// Token: 0x17000225 RID: 549
		// (get) Token: 0x06000870 RID: 2160 RVA: 0x0003E393 File Offset: 0x0003C593
		public TimeSpan Duration
		{
			get
			{
				return this.EndTime - this.StartTime;
			}
		}

		// Token: 0x17000226 RID: 550
		// (get) Token: 0x06000871 RID: 2161 RVA: 0x0003E3A6 File Offset: 0x0003C5A6
		// (set) Token: 0x06000872 RID: 2162 RVA: 0x0003E3AE File Offset: 0x0003C5AE
		public int GuildsMigrated { get; set; }

		// Token: 0x17000227 RID: 551
		// (get) Token: 0x06000873 RID: 2163 RVA: 0x0003E3B7 File Offset: 0x0003C5B7
		// (set) Token: 0x06000874 RID: 2164 RVA: 0x0003E3BF File Offset: 0x0003C5BF
		public int CooldownsMigrated { get; set; }

		// Token: 0x17000228 RID: 552
		// (get) Token: 0x06000875 RID: 2165 RVA: 0x0003E3C8 File Offset: 0x0003C5C8
		// (set) Token: 0x06000876 RID: 2166 RVA: 0x0003E3D0 File Offset: 0x0003C5D0
		public int ZoneWhitelistsMigrated { get; set; }

		// Token: 0x17000229 RID: 553
		// (get) Token: 0x06000877 RID: 2167 RVA: 0x0003E3D9 File Offset: 0x0003C5D9
		// (set) Token: 0x06000878 RID: 2168 RVA: 0x0003E3E1 File Offset: 0x0003C5E1
		public List<string> BackupFiles { get; set; } = new List<string>();

		// Token: 0x1700022A RID: 554
		// (get) Token: 0x06000879 RID: 2169 RVA: 0x0003E3EA File Offset: 0x0003C5EA
		// (set) Token: 0x0600087A RID: 2170 RVA: 0x0003E3F2 File Offset: 0x0003C5F2
		public List<string> Errors { get; set; } = new List<string>();

		// Token: 0x1700022B RID: 555
		// (get) Token: 0x0600087B RID: 2171 RVA: 0x0003E3FB File Offset: 0x0003C5FB
		// (set) Token: 0x0600087C RID: 2172 RVA: 0x0003E403 File Offset: 0x0003C603
		public List<string> Warnings { get; set; } = new List<string>();

		// Token: 0x1700022C RID: 556
		// (get) Token: 0x0600087D RID: 2173 RVA: 0x0003E40C File Offset: 0x0003C60C
		public string Summary
		{
			get
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(86, 7);
				defaultInterpolatedStringHandler.AppendLiteral("Success: ");
				defaultInterpolatedStringHandler.AppendFormatted<bool>(this.Success);
				defaultInterpolatedStringHandler.AppendLiteral(", Guilds: ");
				defaultInterpolatedStringHandler.AppendFormatted<int>(this.GuildsMigrated);
				defaultInterpolatedStringHandler.AppendLiteral(", Cooldowns: ");
				defaultInterpolatedStringHandler.AppendFormatted<int>(this.CooldownsMigrated);
				defaultInterpolatedStringHandler.AppendLiteral(", Zone Whitelists: ");
				defaultInterpolatedStringHandler.AppendFormatted<int>(this.ZoneWhitelistsMigrated);
				defaultInterpolatedStringHandler.AppendLiteral(", Duration: ");
				defaultInterpolatedStringHandler.AppendFormatted<double>(this.Duration.TotalSeconds, "F2");
				defaultInterpolatedStringHandler.AppendLiteral("s, Errors: ");
				defaultInterpolatedStringHandler.AppendFormatted<int>(this.Errors.Count);
				defaultInterpolatedStringHandler.AppendLiteral(", Warnings: ");
				defaultInterpolatedStringHandler.AppendFormatted<int>(this.Warnings.Count);
				return defaultInterpolatedStringHandler.ToStringAndClear();
			}
		}
	}
}
