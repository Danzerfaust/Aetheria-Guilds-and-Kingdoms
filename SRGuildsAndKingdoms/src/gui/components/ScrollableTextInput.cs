using System;
using System.Runtime.CompilerServices;
using Vintagestory.API.Client;

namespace SRGuildsAndKingdoms.src.gui.components
{
	// Token: 0x020000A0 RID: 160
	public class ScrollableTextInput : GuiElementTextInput
	{
		// Token: 0x060006DE RID: 1758 RVA: 0x00033E12 File Offset: 0x00032012
		[NullableContext(1)]
		public ScrollableTextInput(ICoreClientAPI capi, ElementBounds bounds, Action<string> onTextChanged, CairoFont font) : base(capi, bounds, onTextChanged, font)
		{
		}

		// Token: 0x060006DF RID: 1759 RVA: 0x00033E1F File Offset: 0x0003201F
		public override void RenderInteractiveElements(float deltaTime)
		{
			this.highlightBounds.CalcWorldBounds();
			base.RenderInteractiveElements(deltaTime);
		}
	}
}
