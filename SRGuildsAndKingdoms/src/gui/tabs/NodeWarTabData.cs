using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace SRGuildsAndKingdoms.src.gui.tabs
{
	// Token: 0x02000088 RID: 136
	[NullableContext(2)]
	[Nullable(0)]
	public class NodeWarTabData
	{
		// Token: 0x17000193 RID: 403
		// (get) Token: 0x060005F0 RID: 1520 RVA: 0x0002C350 File Offset: 0x0002A550
		// (set) Token: 0x060005F1 RID: 1521 RVA: 0x0002C358 File Offset: 0x0002A558
		[Nullable(1)]
		public List<ControlledNodeInfo> ControlledNodes { [NullableContext(1)] get; [NullableContext(1)] set; } = new List<ControlledNodeInfo>();

		// Token: 0x17000194 RID: 404
		// (get) Token: 0x060005F2 RID: 1522 RVA: 0x0002C361 File Offset: 0x0002A561
		// (set) Token: 0x060005F3 RID: 1523 RVA: 0x0002C369 File Offset: 0x0002A569
		public CurrentWarInfo CurrentWar { get; set; }

		// Token: 0x17000195 RID: 405
		// (get) Token: 0x060005F4 RID: 1524 RVA: 0x0002C372 File Offset: 0x0002A572
		// (set) Token: 0x060005F5 RID: 1525 RVA: 0x0002C37A File Offset: 0x0002A57A
		[Nullable(1)]
		public List<AvailableWarInfo> AvailableWars { [NullableContext(1)] get; [NullableContext(1)] set; } = new List<AvailableWarInfo>();

		// Token: 0x17000196 RID: 406
		// (get) Token: 0x060005F6 RID: 1526 RVA: 0x0002C383 File Offset: 0x0002A583
		// (set) Token: 0x060005F7 RID: 1527 RVA: 0x0002C38B File Offset: 0x0002A58B
		public CurrentSignupInfo CurrentSignup { get; set; }

		// Token: 0x17000197 RID: 407
		// (get) Token: 0x060005F8 RID: 1528 RVA: 0x0002C394 File Offset: 0x0002A594
		// (set) Token: 0x060005F9 RID: 1529 RVA: 0x0002C39C File Offset: 0x0002A59C
		public string SelectedWarForSignup { get; set; }
	}
}
