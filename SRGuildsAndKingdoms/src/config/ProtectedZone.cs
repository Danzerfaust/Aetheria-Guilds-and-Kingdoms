using System;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using Vintagestory.API.MathTools;

namespace SRGuildsAndKingdoms.src.config
{
	// Token: 0x020000C2 RID: 194
	[NullableContext(1)]
	[Nullable(0)]
	public class ProtectedZone
	{
		// Token: 0x17000277 RID: 631
		// (get) Token: 0x06000968 RID: 2408 RVA: 0x00043395 File Offset: 0x00041595
		// (set) Token: 0x06000969 RID: 2409 RVA: 0x0004339D File Offset: 0x0004159D
		public int Id { get; set; }

		// Token: 0x17000278 RID: 632
		// (get) Token: 0x0600096A RID: 2410 RVA: 0x000433A6 File Offset: 0x000415A6
		// (set) Token: 0x0600096B RID: 2411 RVA: 0x000433AE File Offset: 0x000415AE
		public string Name { get; set; } = "Unnamed Zone";

		// Token: 0x17000279 RID: 633
		// (get) Token: 0x0600096C RID: 2412 RVA: 0x000433B7 File Offset: 0x000415B7
		// (set) Token: 0x0600096D RID: 2413 RVA: 0x000433BF File Offset: 0x000415BF
		[JsonIgnore]
		public BlockPos Center { get; set; } = new BlockPos(0, 0, 0);

		// Token: 0x1700027A RID: 634
		// (get) Token: 0x0600096E RID: 2414 RVA: 0x000433C8 File Offset: 0x000415C8
		// (set) Token: 0x0600096F RID: 2415 RVA: 0x000433D5 File Offset: 0x000415D5
		[JsonPropertyName("x")]
		public int X
		{
			get
			{
				return this.Center.X;
			}
			set
			{
				BlockPos center = this.Center;
				int num = (center != null) ? center.Y : 0;
				BlockPos center2 = this.Center;
				this.Center = new BlockPos(value, num, (center2 != null) ? center2.Z : 0);
			}
		}

		// Token: 0x1700027B RID: 635
		// (get) Token: 0x06000970 RID: 2416 RVA: 0x00043407 File Offset: 0x00041607
		// (set) Token: 0x06000971 RID: 2417 RVA: 0x00043414 File Offset: 0x00041614
		[JsonPropertyName("z")]
		public int Z
		{
			get
			{
				return this.Center.Z;
			}
			set
			{
				BlockPos center = this.Center;
				int num = (center != null) ? center.X : 0;
				BlockPos center2 = this.Center;
				this.Center = new BlockPos(num, (center2 != null) ? center2.Y : 0, value);
			}
		}

		// Token: 0x1700027C RID: 636
		// (get) Token: 0x06000972 RID: 2418 RVA: 0x00043446 File Offset: 0x00041646
		// (set) Token: 0x06000973 RID: 2419 RVA: 0x0004344E File Offset: 0x0004164E
		public int Radius { get; set; } = 100;

		// Token: 0x06000974 RID: 2420 RVA: 0x00043458 File Offset: 0x00041658
		internal bool IsPositionWithinZone(int blockX, int blockZ, BlockPos spawnPos)
		{
			int num = blockX - this.Center.X - spawnPos.X;
			int deltaZ = blockZ - this.Center.Z - spawnPos.Z;
			return Math.Sqrt((double)(num * num + deltaZ * deltaZ)) <= (double)this.Radius;
		}

		// Token: 0x06000975 RID: 2421 RVA: 0x000434A8 File Offset: 0x000416A8
		public override string ToString()
		{
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(21, 4);
			defaultInterpolatedStringHandler.AppendFormatted(this.Name);
			defaultInterpolatedStringHandler.AppendLiteral(" at (");
			defaultInterpolatedStringHandler.AppendFormatted<int>(this.Center.X);
			defaultInterpolatedStringHandler.AppendLiteral(", ");
			defaultInterpolatedStringHandler.AppendFormatted<int>(this.Center.Z);
			defaultInterpolatedStringHandler.AppendLiteral(") with radius ");
			defaultInterpolatedStringHandler.AppendFormatted<int>(this.Radius);
			return defaultInterpolatedStringHandler.ToStringAndClear();
		}
	}
}
