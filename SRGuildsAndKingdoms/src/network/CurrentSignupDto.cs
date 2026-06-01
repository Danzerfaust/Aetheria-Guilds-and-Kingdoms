using System;
using System.Runtime.CompilerServices;
using ProtoBuf;

namespace SRGuildsAndKingdoms.src.network
{
	// Token: 0x02000051 RID: 81
	[NullableContext(1)]
	[Nullable(0)]
	[ProtoContract(ImplicitFields = 1)]
	public class CurrentSignupDto
	{
		// Token: 0x170000E0 RID: 224
		// (get) Token: 0x06000312 RID: 786 RVA: 0x000156B9 File Offset: 0x000138B9
		// (set) Token: 0x06000313 RID: 787 RVA: 0x000156C1 File Offset: 0x000138C1
		public string NodeId { get; set; } = string.Empty;

		// Token: 0x170000E1 RID: 225
		// (get) Token: 0x06000314 RID: 788 RVA: 0x000156CA File Offset: 0x000138CA
		// (set) Token: 0x06000315 RID: 789 RVA: 0x000156D2 File Offset: 0x000138D2
		public string NodeName { get; set; } = string.Empty;

		// Token: 0x170000E2 RID: 226
		// (get) Token: 0x06000316 RID: 790 RVA: 0x000156DB File Offset: 0x000138DB
		// (set) Token: 0x06000317 RID: 791 RVA: 0x000156E3 File Offset: 0x000138E3
		public long SignupTimeTicks { get; set; }

		// Token: 0x170000E3 RID: 227
		// (get) Token: 0x06000318 RID: 792 RVA: 0x000156EC File Offset: 0x000138EC
		// (set) Token: 0x06000319 RID: 793 RVA: 0x000156F4 File Offset: 0x000138F4
		public long WarStartTimeTicks { get; set; }
	}
}
