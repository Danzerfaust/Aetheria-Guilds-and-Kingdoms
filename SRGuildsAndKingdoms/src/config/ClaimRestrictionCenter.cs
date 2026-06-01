using System;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using Vintagestory.API.MathTools;

namespace SRGuildsAndKingdoms.src.config
{
	// Token: 0x020000C1 RID: 193
	[NullableContext(1)]
	[Nullable(0)]
	public class ClaimRestrictionCenter
	{
		// Token: 0x17000274 RID: 628
		// (get) Token: 0x06000960 RID: 2400 RVA: 0x00043299 File Offset: 0x00041499
		// (set) Token: 0x06000961 RID: 2401 RVA: 0x000432A1 File Offset: 0x000414A1
		[JsonIgnore]
		public BlockPos Position { get; set; }

		// Token: 0x17000275 RID: 629
		// (get) Token: 0x06000962 RID: 2402 RVA: 0x000432AA File Offset: 0x000414AA
		// (set) Token: 0x06000963 RID: 2403 RVA: 0x000432B7 File Offset: 0x000414B7
		[JsonPropertyName("x")]
		public int X
		{
			get
			{
				return this.Position.X;
			}
			set
			{
				BlockPos position = this.Position;
				int num = (position != null) ? position.Y : 0;
				BlockPos position2 = this.Position;
				this.Position = new BlockPos(value, num, (position2 != null) ? position2.Z : 0);
			}
		}

		// Token: 0x17000276 RID: 630
		// (get) Token: 0x06000964 RID: 2404 RVA: 0x000432E9 File Offset: 0x000414E9
		// (set) Token: 0x06000965 RID: 2405 RVA: 0x000432F6 File Offset: 0x000414F6
		[JsonPropertyName("z")]
		public int Z
		{
			get
			{
				return this.Position.Z;
			}
			set
			{
				BlockPos position = this.Position;
				int num = (position != null) ? position.X : 0;
				BlockPos position2 = this.Position;
				this.Position = new BlockPos(num, (position2 != null) ? position2.Y : 0, value);
			}
		}

		// Token: 0x06000966 RID: 2406 RVA: 0x00043328 File Offset: 0x00041528
		public override string ToString()
		{
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(4, 2);
			defaultInterpolatedStringHandler.AppendLiteral("(");
			defaultInterpolatedStringHandler.AppendFormatted<int>(this.Position.X);
			defaultInterpolatedStringHandler.AppendLiteral(", ");
			defaultInterpolatedStringHandler.AppendFormatted<int>(this.Position.Z);
			defaultInterpolatedStringHandler.AppendLiteral(")");
			return defaultInterpolatedStringHandler.ToStringAndClear();
		}
	}
}
