using System;
using System.Runtime.CompilerServices;
using ProtoBuf;

namespace SRGuildsAndKingdoms.src.network
{
	// Token: 0x02000044 RID: 68
	[NullableContext(1)]
	[Nullable(0)]
	[ProtoContract(ImplicitFields = 1)]
	public class PersonalTechContributionResponsePacket : GuildPacketBase
	{
		// Token: 0x170000A5 RID: 165
		// (get) Token: 0x0600028F RID: 655 RVA: 0x000151CC File Offset: 0x000133CC
		// (set) Token: 0x06000290 RID: 656 RVA: 0x000151D4 File Offset: 0x000133D4
		public bool Success { get; set; }

		// Token: 0x170000A6 RID: 166
		// (get) Token: 0x06000291 RID: 657 RVA: 0x000151DD File Offset: 0x000133DD
		// (set) Token: 0x06000292 RID: 658 RVA: 0x000151E5 File Offset: 0x000133E5
		public string Message { get; set; }

		// Token: 0x170000A7 RID: 167
		// (get) Token: 0x06000293 RID: 659 RVA: 0x000151EE File Offset: 0x000133EE
		// (set) Token: 0x06000294 RID: 660 RVA: 0x000151F6 File Offset: 0x000133F6
		public int TechBlockId { get; set; }

		// Token: 0x170000A8 RID: 168
		// (get) Token: 0x06000295 RID: 661 RVA: 0x000151FF File Offset: 0x000133FF
		// (set) Token: 0x06000296 RID: 662 RVA: 0x00015207 File Offset: 0x00013407
		public bool PersonalUnlockComplete { get; set; }
	}
}
