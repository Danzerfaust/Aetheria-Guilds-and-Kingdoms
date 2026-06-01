using System;
using System.Runtime.CompilerServices;
using ProtoBuf;

namespace SRGuildsAndKingdoms.src.network
{
	// Token: 0x02000032 RID: 50
	[NullableContext(1)]
	[Nullable(0)]
	[ProtoContract(ImplicitFields = 1)]
	public class GuildRemoveMemberPacket : GuildPacketBase
	{
		// Token: 0x17000074 RID: 116
		// (get) Token: 0x0600021B RID: 539 RVA: 0x00014D98 File Offset: 0x00012F98
		// (set) Token: 0x0600021C RID: 540 RVA: 0x00014DA0 File Offset: 0x00012FA0
		public string TargetPlayerName { get; set; }
	}
}
