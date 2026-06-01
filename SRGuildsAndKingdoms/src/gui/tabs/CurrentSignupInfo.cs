using System;
using System.Runtime.CompilerServices;

namespace SRGuildsAndKingdoms.src.gui.tabs
{
	// Token: 0x0200008D RID: 141
	[NullableContext(1)]
	[Nullable(0)]
	public class CurrentSignupInfo
	{
		// Token: 0x170001B2 RID: 434
		// (get) Token: 0x06000633 RID: 1587 RVA: 0x0002C5EA File Offset: 0x0002A7EA
		// (set) Token: 0x06000634 RID: 1588 RVA: 0x0002C5F2 File Offset: 0x0002A7F2
		public string NodeId { get; set; } = string.Empty;

		// Token: 0x170001B3 RID: 435
		// (get) Token: 0x06000635 RID: 1589 RVA: 0x0002C5FB File Offset: 0x0002A7FB
		// (set) Token: 0x06000636 RID: 1590 RVA: 0x0002C603 File Offset: 0x0002A803
		public string NodeName { get; set; } = string.Empty;

		// Token: 0x170001B4 RID: 436
		// (get) Token: 0x06000637 RID: 1591 RVA: 0x0002C60C File Offset: 0x0002A80C
		// (set) Token: 0x06000638 RID: 1592 RVA: 0x0002C614 File Offset: 0x0002A814
		public DateTime SignupTime { get; set; }

		// Token: 0x170001B5 RID: 437
		// (get) Token: 0x06000639 RID: 1593 RVA: 0x0002C61D File Offset: 0x0002A81D
		// (set) Token: 0x0600063A RID: 1594 RVA: 0x0002C625 File Offset: 0x0002A825
		public DateTime WarStartTime { get; set; }
	}
}
