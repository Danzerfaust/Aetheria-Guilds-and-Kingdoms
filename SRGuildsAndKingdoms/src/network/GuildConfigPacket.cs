using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using ProtoBuf;

namespace SRGuildsAndKingdoms.src.network
{
	// Token: 0x02000047 RID: 71
	[NullableContext(1)]
	[Nullable(0)]
	[ProtoContract(ImplicitFields = 1)]
	public class GuildConfigPacket : GuildPacketBase
	{
		// Token: 0x170000AE RID: 174
		// (get) Token: 0x060002A4 RID: 676 RVA: 0x00015288 File Offset: 0x00013488
		// (set) Token: 0x060002A5 RID: 677 RVA: 0x00015290 File Offset: 0x00013490
		public bool TerritorialRestrictionsEnabled { get; set; }

		// Token: 0x170000AF RID: 175
		// (get) Token: 0x060002A6 RID: 678 RVA: 0x00015299 File Offset: 0x00013499
		// (set) Token: 0x060002A7 RID: 679 RVA: 0x000152A1 File Offset: 0x000134A1
		public int? TerritorialCenterX { get; set; }

		// Token: 0x170000B0 RID: 176
		// (get) Token: 0x060002A8 RID: 680 RVA: 0x000152AA File Offset: 0x000134AA
		// (set) Token: 0x060002A9 RID: 681 RVA: 0x000152B2 File Offset: 0x000134B2
		public int? TerritorialCenterZ { get; set; }

		// Token: 0x170000B1 RID: 177
		// (get) Token: 0x060002AA RID: 682 RVA: 0x000152BB File Offset: 0x000134BB
		// (set) Token: 0x060002AB RID: 683 RVA: 0x000152C3 File Offset: 0x000134C3
		public int TerritorialRadius { get; set; }

		// Token: 0x170000B2 RID: 178
		// (get) Token: 0x060002AC RID: 684 RVA: 0x000152CC File Offset: 0x000134CC
		// (set) Token: 0x060002AD RID: 685 RVA: 0x000152D4 File Offset: 0x000134D4
		public bool ProtectedZonesEnabled { get; set; }

		// Token: 0x170000B3 RID: 179
		// (get) Token: 0x060002AE RID: 686 RVA: 0x000152DD File Offset: 0x000134DD
		// (set) Token: 0x060002AF RID: 687 RVA: 0x000152E5 File Offset: 0x000134E5
		public List<ProtectedZoneData> ProtectedZones { get; set; } = new List<ProtectedZoneData>();

		// Token: 0x170000B4 RID: 180
		// (get) Token: 0x060002B0 RID: 688 RVA: 0x000152EE File Offset: 0x000134EE
		// (set) Token: 0x060002B1 RID: 689 RVA: 0x000152F6 File Offset: 0x000134F6
		public List<NodeData> Nodes { get; set; } = new List<NodeData>();

		// Token: 0x170000B5 RID: 181
		// (get) Token: 0x060002B2 RID: 690 RVA: 0x000152FF File Offset: 0x000134FF
		// (set) Token: 0x060002B3 RID: 691 RVA: 0x00015307 File Offset: 0x00013507
		public List<int> EnabledAges { get; set; } = new List<int>();
	}
}
