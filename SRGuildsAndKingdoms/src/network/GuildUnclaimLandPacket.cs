using System;
using ProtoBuf;

namespace SRGuildsAndKingdoms.src.network
{
	// Token: 0x02000034 RID: 52
	[ProtoContract(ImplicitFields = 1)]
	public class GuildUnclaimLandPacket : GuildPacketBase
	{
		// Token: 0x17000079 RID: 121
		// (get) Token: 0x06000227 RID: 551 RVA: 0x00014E08 File Offset: 0x00013008
		// (set) Token: 0x06000228 RID: 552 RVA: 0x00014E10 File Offset: 0x00013010
		public int BlockX { get; set; }

		// Token: 0x1700007A RID: 122
		// (get) Token: 0x06000229 RID: 553 RVA: 0x00014E19 File Offset: 0x00013019
		// (set) Token: 0x0600022A RID: 554 RVA: 0x00014E21 File Offset: 0x00013021
		public int BlockZ { get; set; }
	}
}
