using System;
using System.Runtime.CompilerServices;
using ProtoBuf;

namespace SRGuildsAndKingdoms.src.network
{
	// Token: 0x0200006F RID: 111
	[NullableContext(1)]
	[Nullable(0)]
	[ProtoContract(ImplicitFields = 1)]
	public class CurrencyDefinitionDto
	{
		// Token: 0x1700014D RID: 333
		// (get) Token: 0x06000446 RID: 1094 RVA: 0x00019134 File Offset: 0x00017334
		// (set) Token: 0x06000447 RID: 1095 RVA: 0x0001913C File Offset: 0x0001733C
		public string Code { get; set; } = string.Empty;

		// Token: 0x1700014E RID: 334
		// (get) Token: 0x06000448 RID: 1096 RVA: 0x00019145 File Offset: 0x00017345
		// (set) Token: 0x06000449 RID: 1097 RVA: 0x0001914D File Offset: 0x0001734D
		[Nullable(2)]
		public string Nbt { [NullableContext(2)] get; [NullableContext(2)] set; }
	}
}
