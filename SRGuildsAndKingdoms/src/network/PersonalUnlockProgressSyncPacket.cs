using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using ProtoBuf;

namespace SRGuildsAndKingdoms.src.network
{
	// Token: 0x02000045 RID: 69
	[NullableContext(1)]
	[Nullable(0)]
	[ProtoContract(ImplicitFields = 1)]
	public class PersonalUnlockProgressSyncPacket : GuildPacketBase
	{
		// Token: 0x170000A9 RID: 169
		// (get) Token: 0x06000298 RID: 664 RVA: 0x00015218 File Offset: 0x00013418
		// (set) Token: 0x06000299 RID: 665 RVA: 0x00015220 File Offset: 0x00013420
		public string GuildName { get; set; }

		// Token: 0x170000AA RID: 170
		// (get) Token: 0x0600029A RID: 666 RVA: 0x00015229 File Offset: 0x00013429
		// (set) Token: 0x0600029B RID: 667 RVA: 0x00015231 File Offset: 0x00013431
		public Dictionary<int, PersonalUnlockDto> PersonalUnlocks { get; set; } = new Dictionary<int, PersonalUnlockDto>();
	}
}
