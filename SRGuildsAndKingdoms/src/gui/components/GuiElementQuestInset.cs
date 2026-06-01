using System;
using System.Runtime.CompilerServices;
using Cairo;
using Vintagestory.API.Client;

namespace SRGuildsAndKingdoms.src.gui.components
{
	// Token: 0x0200009C RID: 156
	[NullableContext(1)]
	[Nullable(0)]
	public class GuiElementQuestInset : GuiElement
	{
		// Token: 0x060006CD RID: 1741 RVA: 0x000339D9 File Offset: 0x00031BD9
		public override void OnMouseDown(ICoreClientAPI api, MouseEvent mouse)
		{
			mouse.Handled = false;
		}

		// Token: 0x060006CE RID: 1742 RVA: 0x000339E2 File Offset: 0x00031BE2
		public GuiElementQuestInset(ICoreClientAPI capi, ElementBounds bounds, int depth = 4, float brightness = 0.85f) : base(capi, bounds)
		{
			this.depth = depth;
			this.brightness = brightness;
		}

		// Token: 0x060006CF RID: 1743 RVA: 0x000339FC File Offset: 0x00031BFC
		public override void ComposeElements(Context ctx, ImageSurface surface)
		{
			this.Bounds.CalcWorldBounds();
			if (this.brightness < 1f)
			{
				ctx.SetSourceRGBA(0.0, 0.0, 0.0, (double)(1f - this.brightness));
				GuiElement.Rectangle(ctx, this.Bounds);
				ctx.Fill();
			}
			base.EmbossRoundRectangleElement(ctx, this.Bounds, true, this.depth, -1);
		}

		// Token: 0x060006D0 RID: 1744 RVA: 0x00033A76 File Offset: 0x00031C76
		public override void RenderInteractiveElements(float deltaTime)
		{
		}

		// Token: 0x040002E2 RID: 738
		private readonly int depth;

		// Token: 0x040002E3 RID: 739
		private readonly float brightness;
	}
}
