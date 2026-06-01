using System;
using System.Runtime.CompilerServices;
using ProtoBuf;

namespace SRGuildsAndKingdoms.src.guilds
{
	// Token: 0x020000A2 RID: 162
	[NullableContext(1)]
	[Nullable(0)]
	[ProtoContract(ImplicitFields = 1)]
	public class GuildRole
	{
		// Token: 0x170001C0 RID: 448
		// (get) Token: 0x060006E0 RID: 1760 RVA: 0x00033E33 File Offset: 0x00032033
		// (set) Token: 0x060006E1 RID: 1761 RVA: 0x00033E3B File Offset: 0x0003203B
		public string Description { get; set; }

		// Token: 0x170001C1 RID: 449
		// (get) Token: 0x060006E2 RID: 1762 RVA: 0x00033E44 File Offset: 0x00032044
		// (set) Token: 0x060006E3 RID: 1763 RVA: 0x00033E4C File Offset: 0x0003204C
		public GuildPermission Permissions { get; set; }

		// Token: 0x170001C2 RID: 450
		// (get) Token: 0x060006E4 RID: 1764 RVA: 0x00033E55 File Offset: 0x00032055
		// (set) Token: 0x060006E5 RID: 1765 RVA: 0x00033E5D File Offset: 0x0003205D
		public int Hierarchy { get; set; } = 999;
	}
}
