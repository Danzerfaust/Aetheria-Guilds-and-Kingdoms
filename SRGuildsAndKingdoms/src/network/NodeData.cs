using System;
using System.Runtime.CompilerServices;
using ProtoBuf;

namespace SRGuildsAndKingdoms.src.network
{
	// Token: 0x02000049 RID: 73
	[NullableContext(1)]
	[Nullable(0)]
	[ProtoContract(ImplicitFields = 1)]
	public class NodeData
	{
		// Token: 0x170000BB RID: 187
		// (get) Token: 0x060002C0 RID: 704 RVA: 0x000153A1 File Offset: 0x000135A1
		// (set) Token: 0x060002C1 RID: 705 RVA: 0x000153A9 File Offset: 0x000135A9
		public string Name { get; set; }

		// Token: 0x170000BC RID: 188
		// (get) Token: 0x060002C2 RID: 706 RVA: 0x000153B2 File Offset: 0x000135B2
		// (set) Token: 0x060002C3 RID: 707 RVA: 0x000153BA File Offset: 0x000135BA
		public int X { get; set; }

		// Token: 0x170000BD RID: 189
		// (get) Token: 0x060002C4 RID: 708 RVA: 0x000153C3 File Offset: 0x000135C3
		// (set) Token: 0x060002C5 RID: 709 RVA: 0x000153CB File Offset: 0x000135CB
		public int Z { get; set; }

		// Token: 0x170000BE RID: 190
		// (get) Token: 0x060002C6 RID: 710 RVA: 0x000153D4 File Offset: 0x000135D4
		// (set) Token: 0x060002C7 RID: 711 RVA: 0x000153DC File Offset: 0x000135DC
		public int Radius { get; set; }
	}
}
