using System;
using System.Runtime.CompilerServices;
using ProtoBuf;

namespace SRGuildsAndKingdoms.src.network
{
	// Token: 0x0200002B RID: 43
	[NullableContext(1)]
	[Nullable(0)]
	[ProtoContract(ImplicitFields = 1)]
	public class GuildInvitePacket : GuildPacketBase
	{
		// Token: 0x1700006C RID: 108
		// (get) Token: 0x06000204 RID: 516 RVA: 0x00014CCD File Offset: 0x00012ECD
		// (set) Token: 0x06000205 RID: 517 RVA: 0x00014CD5 File Offset: 0x00012ED5
		public string TargetPlayerUid { get; set; }
	}
}
