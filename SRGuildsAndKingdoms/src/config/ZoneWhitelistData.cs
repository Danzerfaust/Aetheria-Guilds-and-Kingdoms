using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace SRGuildsAndKingdoms.src.config
{
	// Token: 0x020000C8 RID: 200
	[NullableContext(1)]
	[Nullable(0)]
	public class ZoneWhitelistData
	{
		// Token: 0x17000286 RID: 646
		// (get) Token: 0x060009AF RID: 2479 RVA: 0x00044EA7 File Offset: 0x000430A7
		// (set) Token: 0x060009B0 RID: 2480 RVA: 0x00044EAF File Offset: 0x000430AF
		public List<ZoneWhitelistEntry> Whitelists { get; set; } = new List<ZoneWhitelistEntry>();
	}
}
