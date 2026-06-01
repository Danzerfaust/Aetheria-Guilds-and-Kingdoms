using System;
using System.Runtime.CompilerServices;
using ProtoBuf;

namespace SRGuildsAndKingdoms.src.network
{
	// Token: 0x0200006A RID: 106
	[NullableContext(1)]
	[Nullable(0)]
	[ProtoContract(ImplicitFields = 1)]
	public class OpenQuestManagerPacket
	{
		// Token: 0x17000142 RID: 322
		// (get) Token: 0x0600042B RID: 1067 RVA: 0x00018FF9 File Offset: 0x000171F9
		// (set) Token: 0x0600042C RID: 1068 RVA: 0x00019001 File Offset: 0x00017201
		public string PlayerUid { get; set; } = string.Empty;
	}
}
