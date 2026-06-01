using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using ProtoBuf;

namespace SRGuildsAndKingdoms.src.network
{
	// Token: 0x02000048 RID: 72
	[NullableContext(1)]
	[Nullable(0)]
	[ProtoContract(ImplicitFields = 1)]
	public class ProtectedZoneData
	{
		// Token: 0x170000B6 RID: 182
		// (get) Token: 0x060002B5 RID: 693 RVA: 0x00015339 File Offset: 0x00013539
		// (set) Token: 0x060002B6 RID: 694 RVA: 0x00015341 File Offset: 0x00013541
		public string Name { get; set; }

		// Token: 0x170000B7 RID: 183
		// (get) Token: 0x060002B7 RID: 695 RVA: 0x0001534A File Offset: 0x0001354A
		// (set) Token: 0x060002B8 RID: 696 RVA: 0x00015352 File Offset: 0x00013552
		public int X { get; set; }

		// Token: 0x170000B8 RID: 184
		// (get) Token: 0x060002B9 RID: 697 RVA: 0x0001535B File Offset: 0x0001355B
		// (set) Token: 0x060002BA RID: 698 RVA: 0x00015363 File Offset: 0x00013563
		public int Z { get; set; }

		// Token: 0x170000B9 RID: 185
		// (get) Token: 0x060002BB RID: 699 RVA: 0x0001536C File Offset: 0x0001356C
		// (set) Token: 0x060002BC RID: 700 RVA: 0x00015374 File Offset: 0x00013574
		public int Radius { get; set; }

		// Token: 0x170000BA RID: 186
		// (get) Token: 0x060002BD RID: 701 RVA: 0x0001537D File Offset: 0x0001357D
		// (set) Token: 0x060002BE RID: 702 RVA: 0x00015385 File Offset: 0x00013585
		public List<string> WhitelistedPlayers { get; set; } = new List<string>();
	}
}
