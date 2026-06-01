using System;
using System.Runtime.CompilerServices;

namespace SRGuildsAndKingdoms.src.database
{
	// Token: 0x020000B9 RID: 185
	[NullableContext(1)]
	[Nullable(0)]
	public class NodeData
	{
		// Token: 0x1700022D RID: 557
		// (get) Token: 0x06000899 RID: 2201 RVA: 0x00040643 File Offset: 0x0003E843
		// (set) Token: 0x0600089A RID: 2202 RVA: 0x0004064B File Offset: 0x0003E84B
		public int Id { get; set; }

		// Token: 0x1700022E RID: 558
		// (get) Token: 0x0600089B RID: 2203 RVA: 0x00040654 File Offset: 0x0003E854
		// (set) Token: 0x0600089C RID: 2204 RVA: 0x0004065C File Offset: 0x0003E85C
		public string Name { get; set; } = string.Empty;

		// Token: 0x1700022F RID: 559
		// (get) Token: 0x0600089D RID: 2205 RVA: 0x00040665 File Offset: 0x0003E865
		// (set) Token: 0x0600089E RID: 2206 RVA: 0x0004066D File Offset: 0x0003E86D
		public int X { get; set; }

		// Token: 0x17000230 RID: 560
		// (get) Token: 0x0600089F RID: 2207 RVA: 0x00040676 File Offset: 0x0003E876
		// (set) Token: 0x060008A0 RID: 2208 RVA: 0x0004067E File Offset: 0x0003E87E
		public int Z { get; set; }

		// Token: 0x17000231 RID: 561
		// (get) Token: 0x060008A1 RID: 2209 RVA: 0x00040687 File Offset: 0x0003E887
		// (set) Token: 0x060008A2 RID: 2210 RVA: 0x0004068F File Offset: 0x0003E88F
		public int Radius { get; set; }

		// Token: 0x17000232 RID: 562
		// (get) Token: 0x060008A3 RID: 2211 RVA: 0x00040698 File Offset: 0x0003E898
		// (set) Token: 0x060008A4 RID: 2212 RVA: 0x000406A0 File Offset: 0x0003E8A0
		public long CreatedAt { get; set; }
	}
}
