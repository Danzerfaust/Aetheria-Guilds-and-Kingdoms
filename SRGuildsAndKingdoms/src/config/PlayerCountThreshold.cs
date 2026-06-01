using System;
using System.Runtime.CompilerServices;

namespace SRGuildsAndKingdoms.src.config
{
	// Token: 0x020000C3 RID: 195
	[NullableContext(1)]
	[Nullable(0)]
	public class PlayerCountThreshold
	{
		// Token: 0x1700027D RID: 637
		// (get) Token: 0x06000977 RID: 2423 RVA: 0x00043551 File Offset: 0x00041751
		// (set) Token: 0x06000978 RID: 2424 RVA: 0x00043559 File Offset: 0x00041759
		public int MinPlayerCount { get; set; }

		// Token: 0x1700027E RID: 638
		// (get) Token: 0x06000979 RID: 2425 RVA: 0x00043562 File Offset: 0x00041762
		// (set) Token: 0x0600097A RID: 2426 RVA: 0x0004356A File Offset: 0x0004176A
		public int AdditionalClaims { get; set; }

		// Token: 0x1700027F RID: 639
		// (get) Token: 0x0600097B RID: 2427 RVA: 0x00043573 File Offset: 0x00041773
		// (set) Token: 0x0600097C RID: 2428 RVA: 0x0004357B File Offset: 0x0004177B
		public string Description { get; set; } = "";

		// Token: 0x0600097D RID: 2429 RVA: 0x00043584 File Offset: 0x00041784
		public override string ToString()
		{
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(21, 2);
			defaultInterpolatedStringHandler.AppendLiteral("At ");
			defaultInterpolatedStringHandler.AppendFormatted<int>(this.MinPlayerCount);
			defaultInterpolatedStringHandler.AppendLiteral(" members: +");
			defaultInterpolatedStringHandler.AppendFormatted<int>(this.AdditionalClaims);
			defaultInterpolatedStringHandler.AppendLiteral(" claims");
			return defaultInterpolatedStringHandler.ToStringAndClear();
		}
	}
}
