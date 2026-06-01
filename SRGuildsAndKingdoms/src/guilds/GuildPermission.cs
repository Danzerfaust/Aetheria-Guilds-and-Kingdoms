using System;

namespace SRGuildsAndKingdoms.src.guilds
{
	// Token: 0x020000A1 RID: 161
	[Flags]
	public enum GuildPermission
	{
		// Token: 0x040002EA RID: 746
		None = 0,
		// Token: 0x040002EB RID: 747
		Invite = 1,
		// Token: 0x040002EC RID: 748
		Promote = 2,
		// Token: 0x040002ED RID: 749
		Kick = 4,
		// Token: 0x040002EE RID: 750
		ManageRoles = 8,
		// Token: 0x040002EF RID: 751
		BreakAndPlaceBlocks = 16,
		// Token: 0x040002F0 RID: 752
		InteractBlocks = 32
	}
}
