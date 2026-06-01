using System;
using System.Runtime.CompilerServices;

namespace SRGuildsAndKingdoms.src.config
{
	// Token: 0x020000C5 RID: 197
	[NullableContext(1)]
	[Nullable(0)]
	public class CurrencyDefinition
	{
		// Token: 0x17000280 RID: 640
		// (get) Token: 0x0600098B RID: 2443 RVA: 0x000445C0 File Offset: 0x000427C0
		// (set) Token: 0x0600098C RID: 2444 RVA: 0x000445C8 File Offset: 0x000427C8
		public string Code { get; set; } = string.Empty;

		// Token: 0x17000281 RID: 641
		// (get) Token: 0x0600098D RID: 2445 RVA: 0x000445D1 File Offset: 0x000427D1
		// (set) Token: 0x0600098E RID: 2446 RVA: 0x000445D9 File Offset: 0x000427D9
		[Nullable(2)]
		public string Nbt { [NullableContext(2)] get; [NullableContext(2)] set; }

		// Token: 0x0600098F RID: 2447 RVA: 0x000445E2 File Offset: 0x000427E2
		public CurrencyDefinition()
		{
		}

		// Token: 0x06000990 RID: 2448 RVA: 0x000445F5 File Offset: 0x000427F5
		public CurrencyDefinition(string code, [Nullable(2)] string nbt = null)
		{
			this.Code = code;
			this.Nbt = nbt;
		}

		// Token: 0x06000991 RID: 2449 RVA: 0x00044616 File Offset: 0x00042816
		public override string ToString()
		{
			return this.Code + (string.IsNullOrEmpty(this.Nbt) ? "" : " (with NBT)");
		}
	}
}
