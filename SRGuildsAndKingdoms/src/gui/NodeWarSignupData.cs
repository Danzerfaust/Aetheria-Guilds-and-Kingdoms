using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace SRGuildsAndKingdoms.src.gui
{
	// Token: 0x0200007D RID: 125
	[NullableContext(1)]
	[Nullable(0)]
	public class NodeWarSignupData
	{
		// Token: 0x17000172 RID: 370
		// (get) Token: 0x06000536 RID: 1334 RVA: 0x0002140B File Offset: 0x0001F60B
		// (set) Token: 0x06000537 RID: 1335 RVA: 0x00021413 File Offset: 0x0001F613
		public string NodeId { get; set; } = string.Empty;

		// Token: 0x17000173 RID: 371
		// (get) Token: 0x06000538 RID: 1336 RVA: 0x0002141C File Offset: 0x0001F61C
		// (set) Token: 0x06000539 RID: 1337 RVA: 0x00021424 File Offset: 0x0001F624
		public string NodeName { get; set; } = string.Empty;

		// Token: 0x17000174 RID: 372
		// (get) Token: 0x0600053A RID: 1338 RVA: 0x0002142D File Offset: 0x0001F62D
		// (set) Token: 0x0600053B RID: 1339 RVA: 0x00021435 File Offset: 0x0001F635
		public string NodeDescription { get; set; } = string.Empty;

		// Token: 0x17000175 RID: 373
		// (get) Token: 0x0600053C RID: 1340 RVA: 0x0002143E File Offset: 0x0001F63E
		// (set) Token: 0x0600053D RID: 1341 RVA: 0x00021446 File Offset: 0x0001F646
		public DateTime StartTime { get; set; }

		// Token: 0x17000176 RID: 374
		// (get) Token: 0x0600053E RID: 1342 RVA: 0x0002144F File Offset: 0x0001F64F
		// (set) Token: 0x0600053F RID: 1343 RVA: 0x00021457 File Offset: 0x0001F657
		public int DurationMinutes { get; set; }

		// Token: 0x17000177 RID: 375
		// (get) Token: 0x06000540 RID: 1344 RVA: 0x00021460 File Offset: 0x0001F660
		// (set) Token: 0x06000541 RID: 1345 RVA: 0x00021468 File Offset: 0x0001F668
		public double PointsNeeded { get; set; }

		// Token: 0x17000178 RID: 376
		// (get) Token: 0x06000542 RID: 1346 RVA: 0x00021471 File Offset: 0x0001F671
		// (set) Token: 0x06000543 RID: 1347 RVA: 0x00021479 File Offset: 0x0001F679
		public int MinPlayersRequired { get; set; }

		// Token: 0x17000179 RID: 377
		// (get) Token: 0x06000544 RID: 1348 RVA: 0x00021482 File Offset: 0x0001F682
		// (set) Token: 0x06000545 RID: 1349 RVA: 0x0002148A File Offset: 0x0001F68A
		public int CurrentSignups { get; set; }

		// Token: 0x1700017A RID: 378
		// (get) Token: 0x06000546 RID: 1350 RVA: 0x00021493 File Offset: 0x0001F693
		// (set) Token: 0x06000547 RID: 1351 RVA: 0x0002149B File Offset: 0x0001F69B
		public int MaxGuilds { get; set; }

		// Token: 0x1700017B RID: 379
		// (get) Token: 0x06000548 RID: 1352 RVA: 0x000214A4 File Offset: 0x0001F6A4
		// (set) Token: 0x06000549 RID: 1353 RVA: 0x000214AC File Offset: 0x0001F6AC
		public List<string> SignedUpGuilds { get; set; } = new List<string>();

		// Token: 0x1700017C RID: 380
		// (get) Token: 0x0600054A RID: 1354 RVA: 0x000214B5 File Offset: 0x0001F6B5
		// (set) Token: 0x0600054B RID: 1355 RVA: 0x000214BD File Offset: 0x0001F6BD
		public int MinGuildMembers { get; set; }

		// Token: 0x1700017D RID: 381
		// (get) Token: 0x0600054C RID: 1356 RVA: 0x000214C6 File Offset: 0x0001F6C6
		// (set) Token: 0x0600054D RID: 1357 RVA: 0x000214CE File Offset: 0x0001F6CE
		public int MinOnlineMembers { get; set; }

		// Token: 0x1700017E RID: 382
		// (get) Token: 0x0600054E RID: 1358 RVA: 0x000214D7 File Offset: 0x0001F6D7
		// (set) Token: 0x0600054F RID: 1359 RVA: 0x000214DF File Offset: 0x0001F6DF
		public int GuildTotalMembers { get; set; }

		// Token: 0x1700017F RID: 383
		// (get) Token: 0x06000550 RID: 1360 RVA: 0x000214E8 File Offset: 0x0001F6E8
		// (set) Token: 0x06000551 RID: 1361 RVA: 0x000214F0 File Offset: 0x0001F6F0
		public int GuildOnlineMembers { get; set; }

		// Token: 0x17000180 RID: 384
		// (get) Token: 0x06000552 RID: 1362 RVA: 0x000214F9 File Offset: 0x0001F6F9
		// (set) Token: 0x06000553 RID: 1363 RVA: 0x00021501 File Offset: 0x0001F701
		public bool IsAlreadySignedUp { get; set; }

		// Token: 0x17000181 RID: 385
		// (get) Token: 0x06000554 RID: 1364 RVA: 0x0002150A File Offset: 0x0001F70A
		// (set) Token: 0x06000555 RID: 1365 RVA: 0x00021512 File Offset: 0x0001F712
		public bool IsPlayerLeader { get; set; }

		// Token: 0x17000182 RID: 386
		// (get) Token: 0x06000556 RID: 1366 RVA: 0x0002151B File Offset: 0x0001F71B
		// (set) Token: 0x06000557 RID: 1367 RVA: 0x00021523 File Offset: 0x0001F723
		public bool IsSignupClosed { get; set; }
	}
}
