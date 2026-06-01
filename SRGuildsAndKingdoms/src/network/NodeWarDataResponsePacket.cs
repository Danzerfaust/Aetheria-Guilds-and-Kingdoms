using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using ProtoBuf;

namespace SRGuildsAndKingdoms.src.network
{
	// Token: 0x0200004C RID: 76
	[NullableContext(1)]
	[Nullable(0)]
	[ProtoContract(ImplicitFields = 1)]
	public class NodeWarDataResponsePacket : GuildPacketBase
	{
		// Token: 0x170000C2 RID: 194
		// (get) Token: 0x060002D1 RID: 721 RVA: 0x00015430 File Offset: 0x00013630
		// (set) Token: 0x060002D2 RID: 722 RVA: 0x00015438 File Offset: 0x00013638
		public List<ControlledNodeDto> ControlledNodes { get; set; } = new List<ControlledNodeDto>();

		// Token: 0x170000C3 RID: 195
		// (get) Token: 0x060002D3 RID: 723 RVA: 0x00015441 File Offset: 0x00013641
		// (set) Token: 0x060002D4 RID: 724 RVA: 0x00015449 File Offset: 0x00013649
		[Nullable(2)]
		public CurrentWarDto CurrentWar { [NullableContext(2)] get; [NullableContext(2)] set; }

		// Token: 0x170000C4 RID: 196
		// (get) Token: 0x060002D5 RID: 725 RVA: 0x00015452 File Offset: 0x00013652
		// (set) Token: 0x060002D6 RID: 726 RVA: 0x0001545A File Offset: 0x0001365A
		public List<AvailableWarDto> AvailableWars { get; set; } = new List<AvailableWarDto>();

		// Token: 0x170000C5 RID: 197
		// (get) Token: 0x060002D7 RID: 727 RVA: 0x00015463 File Offset: 0x00013663
		// (set) Token: 0x060002D8 RID: 728 RVA: 0x0001546B File Offset: 0x0001366B
		[Nullable(2)]
		public CurrentSignupDto CurrentSignup { [NullableContext(2)] get; [NullableContext(2)] set; }
	}
}
