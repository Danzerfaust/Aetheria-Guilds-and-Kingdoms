using System;
using System.Runtime.CompilerServices;

namespace SRGuildsAndKingdoms.src.techblock
{
	// Token: 0x02000006 RID: 6
	[NullableContext(1)]
	[Nullable(0)]
	public class BlockRestriction
	{
		// Token: 0x1700000D RID: 13
		// (get) Token: 0x0600006E RID: 110 RVA: 0x000095A0 File Offset: 0x000077A0
		// (set) Token: 0x0600006F RID: 111 RVA: 0x000095A8 File Offset: 0x000077A8
		public TechAge RequiredAge { get; set; }

		// Token: 0x1700000E RID: 14
		// (get) Token: 0x06000070 RID: 112 RVA: 0x000095B1 File Offset: 0x000077B1
		// (set) Token: 0x06000071 RID: 113 RVA: 0x000095B9 File Offset: 0x000077B9
		public string RequiredTrait { get; set; }
	}
}
