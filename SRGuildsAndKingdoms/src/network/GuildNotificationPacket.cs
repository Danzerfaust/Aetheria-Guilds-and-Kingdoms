using System;
using System.Runtime.CompilerServices;
using ProtoBuf;

namespace SRGuildsAndKingdoms.src.network
{
	// Token: 0x0200003C RID: 60
	[NullableContext(1)]
	[Nullable(0)]
	[ProtoContract(ImplicitFields = 1)]
	public class GuildNotificationPacket : GuildPacketBase
	{
		// Token: 0x1700008F RID: 143
		// (get) Token: 0x0600025B RID: 603 RVA: 0x00014FEA File Offset: 0x000131EA
		// (set) Token: 0x0600025C RID: 604 RVA: 0x00014FF2 File Offset: 0x000131F2
		public string Message { get; set; }

		// Token: 0x17000090 RID: 144
		// (get) Token: 0x0600025D RID: 605 RVA: 0x00014FFB File Offset: 0x000131FB
		// (set) Token: 0x0600025E RID: 606 RVA: 0x00015003 File Offset: 0x00013203
		public NotificationType Type { get; set; }
	}
}
