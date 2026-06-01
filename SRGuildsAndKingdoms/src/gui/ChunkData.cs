using System;

namespace SRGuildsAndKingdoms.src.gui
{
	// Token: 0x02000084 RID: 132
	public class ChunkData
	{
		// Token: 0x1700018F RID: 399
		// (get) Token: 0x060005D4 RID: 1492 RVA: 0x0002A954 File Offset: 0x00028B54
		// (set) Token: 0x060005D5 RID: 1493 RVA: 0x0002A95C File Offset: 0x00028B5C
		public int ChunkX { get; set; }

		// Token: 0x17000190 RID: 400
		// (get) Token: 0x060005D6 RID: 1494 RVA: 0x0002A965 File Offset: 0x00028B65
		// (set) Token: 0x060005D7 RID: 1495 RVA: 0x0002A96D File Offset: 0x00028B6D
		public int ChunkZ { get; set; }

		// Token: 0x17000191 RID: 401
		// (get) Token: 0x060005D8 RID: 1496 RVA: 0x0002A976 File Offset: 0x00028B76
		// (set) Token: 0x060005D9 RID: 1497 RVA: 0x0002A97E File Offset: 0x00028B7E
		public bool IsLoaded { get; set; }

		// Token: 0x17000192 RID: 402
		// (get) Token: 0x060005DA RID: 1498 RVA: 0x0002A987 File Offset: 0x00028B87
		// (set) Token: 0x060005DB RID: 1499 RVA: 0x0002A98F File Offset: 0x00028B8F
		public int LoadedBlocks { get; set; }
	}
}
