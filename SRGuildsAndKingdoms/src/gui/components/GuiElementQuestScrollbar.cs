using System;
using System.Runtime.CompilerServices;
using Vintagestory.API.Client;
using Vintagestory.API.MathTools;

namespace SRGuildsAndKingdoms.src.gui.components
{
	// Token: 0x0200009D RID: 157
	[NullableContext(1)]
	[Nullable(0)]
	public class GuiElementQuestScrollbar : GuiElementScrollbar
	{
		// Token: 0x060006D1 RID: 1745 RVA: 0x00033A78 File Offset: 0x00031C78
		public GuiElementQuestScrollbar(ICoreClientAPI capi, Action<float> OnNewScrollbarValue, ElementBounds bounds) : base(capi, OnNewScrollbarValue, bounds)
		{
		}

		// Token: 0x060006D2 RID: 1746 RVA: 0x00033A84 File Offset: 0x00031C84
		public override void OnMouseWheel(ICoreClientAPI api, MouseWheelEventArgs args)
		{
			if (this.Bounds.InnerHeight <= (double)this.currentHandleHeight + 0.001)
			{
				return;
			}
			float scrollAmount = (float)GuiElement.scaled(51.0) * args.deltaPrecise / base.ScrollConversionFactor;
			this.targetHandlePosition = this.currentHandlePosition - scrollAmount;
			double scrollbarMoveableHeight = this.Bounds.InnerHeight - (double)this.currentHandleHeight;
			this.targetHandlePosition = (float)GameMath.Clamp((double)this.targetHandlePosition, 0.0, scrollbarMoveableHeight);
			this.isSmoothScrolling = true;
			args.SetHandled(true);
		}

		// Token: 0x060006D3 RID: 1747 RVA: 0x00033B1B File Offset: 0x00031D1B
		public override void OnMouseDownOnElement(ICoreClientAPI api, MouseEvent args)
		{
			this.isSmoothScrolling = false;
			base.OnMouseDownOnElement(api, args);
			this.targetHandlePosition = this.currentHandlePosition;
		}

		// Token: 0x060006D4 RID: 1748 RVA: 0x00033B38 File Offset: 0x00031D38
		public override void OnMouseMove(ICoreClientAPI api, MouseEvent args)
		{
			base.OnMouseMove(api, args);
			if (!this.isSmoothScrolling)
			{
				this.targetHandlePosition = this.currentHandlePosition;
			}
		}

		// Token: 0x060006D5 RID: 1749 RVA: 0x00033B58 File Offset: 0x00031D58
		public override void RenderInteractiveElements(float deltaTime)
		{
			if (this.isSmoothScrolling)
			{
				float diff = this.targetHandlePosition - this.currentHandlePosition;
				if (Math.Abs(diff) > 0.5f)
				{
					this.currentHandlePosition += diff * 0.10000001f;
					double scrollbarMoveableHeight = this.Bounds.InnerHeight - (double)this.currentHandleHeight;
					this.currentHandlePosition = (float)GameMath.Clamp((double)this.currentHandlePosition, 0.0, scrollbarMoveableHeight);
					base.TriggerChanged();
				}
				else
				{
					this.currentHandlePosition = this.targetHandlePosition;
					this.isSmoothScrolling = false;
					base.TriggerChanged();
				}
			}
			base.RenderInteractiveElements(deltaTime);
		}

		// Token: 0x040002E4 RID: 740
		private float targetHandlePosition;

		// Token: 0x040002E5 RID: 741
		private bool isSmoothScrolling;

		// Token: 0x040002E6 RID: 742
		private const float SmoothingFactor = 0.3f;
	}
}
