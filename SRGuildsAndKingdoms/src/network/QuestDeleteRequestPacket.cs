using System;
using System.Runtime.CompilerServices;
using ProtoBuf;

namespace SRGuildsAndKingdoms.src.network
{
	// Token: 0x0200006D RID: 109
	[NullableContext(1)]
	[Nullable(0)]
	[ProtoContract(ImplicitFields = 1)]
	public class QuestDeleteRequestPacket
	{
		// Token: 0x17000148 RID: 328
		// (get) Token: 0x0600043A RID: 1082 RVA: 0x000190AE File Offset: 0x000172AE
		// (set) Token: 0x0600043B RID: 1083 RVA: 0x000190B6 File Offset: 0x000172B6
		public string PlayerUid { get; set; } = string.Empty;

		// Token: 0x17000149 RID: 329
		// (get) Token: 0x0600043C RID: 1084 RVA: 0x000190BF File Offset: 0x000172BF
		// (set) Token: 0x0600043D RID: 1085 RVA: 0x000190C7 File Offset: 0x000172C7
		public int QuestId { get; set; }
	}
}
