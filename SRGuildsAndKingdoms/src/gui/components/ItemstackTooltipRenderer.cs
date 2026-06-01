using System;
using System.Runtime.CompilerServices;
using Vintagestory.API.Client;
using Vintagestory.API.Common;

namespace SRGuildsAndKingdoms.src.gui.components
{
	// Token: 0x02000099 RID: 153
	[NullableContext(1)]
	[Nullable(0)]
	internal class ItemstackTooltipRenderer : ItemstackComponentBase
	{
		// Token: 0x060006C8 RID: 1736 RVA: 0x0003370D File Offset: 0x0003190D
		public ItemstackTooltipRenderer(ICoreClientAPI capi) : base(capi)
		{
		}

		// Token: 0x060006C9 RID: 1737 RVA: 0x00033716 File Offset: 0x00031916
		public void Render(ItemSlot slot, double mouseX, double mouseY, float dt)
		{
			base.RenderItemstackTooltip(slot, mouseX, mouseY, dt);
		}
	}
}
