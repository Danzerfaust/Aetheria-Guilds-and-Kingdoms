using System;
using System.Runtime.CompilerServices;
using Cairo;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;

namespace SRGuildsAndKingdoms.src.gui.components
{
	// Token: 0x02000098 RID: 152
	[NullableContext(1)]
	[Nullable(0)]
	public class GuiElementItemstackDisplay : GuiElement
	{
		// Token: 0x170001BF RID: 447
		// (get) Token: 0x060006C0 RID: 1728 RVA: 0x0003332A File Offset: 0x0003152A
		// (set) Token: 0x060006C1 RID: 1729 RVA: 0x00033332 File Offset: 0x00031532
		public bool ShowStackSize { get; set; } = true;

		// Token: 0x060006C2 RID: 1730 RVA: 0x0003333C File Offset: 0x0003153C
		public GuiElementItemstackDisplay(ICoreClientAPI capi, ItemStack stack, ElementBounds bounds) : base(capi, bounds)
		{
			DummyInventory inv = new DummyInventory(capi, 1);
			inv.OnAcquireTransitionSpeed += ((EnumTransitionType _, ItemStack _, float mul) => 0f);
			this.slot = new DummySlot(stack, inv);
			this.tooltip = new ItemstackTooltipRenderer(capi);
		}

		// Token: 0x060006C3 RID: 1731 RVA: 0x000333A5 File Offset: 0x000315A5
		public void SetStack(ItemStack stack)
		{
			this.slot.Itemstack = stack;
			this.lastRenderedStackSize = -1;
		}

		// Token: 0x060006C4 RID: 1732 RVA: 0x000333BA File Offset: 0x000315BA
		public override void ComposeElements(Context ctx, ImageSurface surface)
		{
			this.ComposeStackSizeTexture();
		}

		// Token: 0x060006C5 RID: 1733 RVA: 0x000333C4 File Offset: 0x000315C4
		private void ComposeStackSizeTexture()
		{
			LoadedTexture loadedTexture = this.stackSizeTexture;
			if (loadedTexture != null)
			{
				loadedTexture.Dispose();
			}
			this.stackSizeTexture = null;
			if (!this.ShowStackSize || this.slot.Itemstack == null || this.slot.Itemstack.StackSize <= 1)
			{
				ItemStack itemstack = this.slot.Itemstack;
				this.lastRenderedStackSize = ((itemstack != null) ? itemstack.StackSize : 0);
				return;
			}
			float mul = (float)(Math.Min(this.Bounds.InnerWidth, this.Bounds.InnerHeight) * 0.85) / (float)GuiElement.scaled(20.6);
			CairoFont font = CairoFont.WhiteSmallText();
			font.UnscaledFontsize *= (double)mul;
			font.FontWeight = 1;
			font.StrokeColor = new double[]
			{
				0.0,
				0.0,
				0.0,
				1.0
			};
			font.StrokeWidth = (double)RuntimeEnv.GUIScale;
			this.stackSizeTexture = this.api.Gui.TextTexture.GenTextTexture(this.slot.Itemstack.StackSize.ToString(), font, null);
			this.lastRenderedStackSize = this.slot.Itemstack.StackSize;
		}

		// Token: 0x060006C6 RID: 1734 RVA: 0x000334F4 File Offset: 0x000316F4
		public override void RenderInteractiveElements(float deltaTime)
		{
			if (this.slot.Itemstack == null)
			{
				return;
			}
			this.Bounds.CalcWorldBounds();
			if (this.ShowStackSize && this.slot.Itemstack.StackSize != this.lastRenderedStackSize)
			{
				this.ComposeStackSizeTexture();
			}
			double cx = this.Bounds.renderX + this.Bounds.InnerWidth * 0.5;
			double cy = this.Bounds.renderY + this.Bounds.InnerHeight * 0.5;
			float sz = (float)(Math.Min(this.Bounds.InnerWidth, this.Bounds.InnerHeight) * 0.85);
			this.api.Render.RenderItemstackToGui(this.slot, cx, cy, 250.0, sz, -1, true, false, false);
			if (this.ShowStackSize && this.stackSizeTexture != null && this.stackSizeTexture.TextureId != 0)
			{
				float mul = sz / (float)GuiElement.scaled(25.6);
				this.api.Render.Render2DTexturePremultipliedAlpha(this.stackSizeTexture.TextureId, (float)((int)(cx + (double)sz + 1.0 - (double)this.stackSizeTexture.Width)), (float)((int)(cy + (double)mul * GuiElement.scaled(3.0) - GuiElement.scaled(4.0))), (float)this.stackSizeTexture.Width, (float)this.stackSizeTexture.Height, 350f, null);
			}
			if (this.Bounds.PointInside(this.api.Input.MouseX, this.api.Input.MouseY))
			{
				this.tooltip.Render(this.slot, (double)this.api.Input.MouseX, (double)this.api.Input.MouseY, deltaTime);
			}
		}

		// Token: 0x060006C7 RID: 1735 RVA: 0x000336E4 File Offset: 0x000318E4
		public override void Dispose()
		{
			base.Dispose();
			LoadedTexture loadedTexture = this.stackSizeTexture;
			if (loadedTexture != null)
			{
				loadedTexture.Dispose();
			}
			ItemstackTooltipRenderer itemstackTooltipRenderer = this.tooltip;
			if (itemstackTooltipRenderer == null)
			{
				return;
			}
			itemstackTooltipRenderer.Dispose();
		}

		// Token: 0x040002D7 RID: 727
		private readonly DummySlot slot;

		// Token: 0x040002D8 RID: 728
		private readonly ItemstackTooltipRenderer tooltip;

		// Token: 0x040002D9 RID: 729
		[Nullable(2)]
		private LoadedTexture stackSizeTexture;

		// Token: 0x040002DA RID: 730
		private int lastRenderedStackSize = -1;
	}
}
