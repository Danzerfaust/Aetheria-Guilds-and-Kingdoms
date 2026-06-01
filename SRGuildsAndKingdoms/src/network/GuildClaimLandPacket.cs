using System;
using System.Runtime.CompilerServices;
using ProtoBuf;

namespace SRGuildsAndKingdoms.src.network
{
	// Token: 0x02000033 RID: 51
	[NullableContext(1)]
	[Nullable(0)]
	[ProtoContract(ImplicitFields = 1)]
	public class GuildClaimLandPacket : GuildPacketBase
	{
		// Token: 0x17000075 RID: 117
		// (get) Token: 0x0600021E RID: 542 RVA: 0x00014DB1 File Offset: 0x00012FB1
		// (set) Token: 0x0600021F RID: 543 RVA: 0x00014DB9 File Offset: 0x00012FB9
		public int BlockX { get; set; }

		// Token: 0x17000076 RID: 118
		// (get) Token: 0x06000220 RID: 544 RVA: 0x00014DC2 File Offset: 0x00012FC2
		// (set) Token: 0x06000221 RID: 545 RVA: 0x00014DCA File Offset: 0x00012FCA
		public int BlockZ { get; set; }

		// Token: 0x17000077 RID: 119
		// (get) Token: 0x06000222 RID: 546 RVA: 0x00014DD3 File Offset: 0x00012FD3
		// (set) Token: 0x06000223 RID: 547 RVA: 0x00014DDB File Offset: 0x00012FDB
		public bool IsOutpost { get; set; }

		// Token: 0x17000078 RID: 120
		// (get) Token: 0x06000224 RID: 548 RVA: 0x00014DE4 File Offset: 0x00012FE4
		// (set) Token: 0x06000225 RID: 549 RVA: 0x00014DEC File Offset: 0x00012FEC
		public string OutpostName { get; set; } = "";
	}
}
