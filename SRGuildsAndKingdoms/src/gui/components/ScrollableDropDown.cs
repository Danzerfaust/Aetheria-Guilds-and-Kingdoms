using System;
using System.Runtime.CompilerServices;
using Vintagestory.API.Client;

namespace SRGuildsAndKingdoms.src.gui.components
{
	// Token: 0x0200009F RID: 159
	public class ScrollableDropDown : GuiElementDropDown
	{
		// Token: 0x060006DC RID: 1756 RVA: 0x00033DD8 File Offset: 0x00031FD8
		[NullableContext(1)]
		public ScrollableDropDown(ICoreClientAPI capi, string[] values, string[] names, int selectedIndex, SelectionChangedDelegate onSelectionChanged, ElementBounds bounds, CairoFont font, bool multiSelect = false) : base(capi, values, names, selectedIndex, onSelectionChanged, bounds, font, multiSelect)
		{
		}

		// Token: 0x060006DD RID: 1757 RVA: 0x00033DF8 File Offset: 0x00031FF8
		public override void RenderInteractiveElements(float deltaTime)
		{
			ElementBounds highlightBounds = this.highlightBounds;
			if (highlightBounds != null)
			{
				highlightBounds.CalcWorldBounds();
			}
			base.RenderInteractiveElements(deltaTime);
		}
	}
}
