using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Cairo;
using Vintagestory.API.Client;
using Vintagestory.API.Common;

namespace SRGuildsAndKingdoms.src.gui.components
{
	// Token: 0x02000097 RID: 151
	[NullableContext(1)]
	[Nullable(0)]
	public class GuiElementCyclingItemDisplay : GuiElement
	{
		// Token: 0x170001BE RID: 446
		// (get) Token: 0x060006BA RID: 1722 RVA: 0x000330CE File Offset: 0x000312CE
		// (set) Token: 0x060006BB RID: 1723 RVA: 0x000330D6 File Offset: 0x000312D6
		public bool ShowStackSize { get; set; } = true;

		// Token: 0x060006BC RID: 1724 RVA: 0x000330E0 File Offset: 0x000312E0
		public GuiElementCyclingItemDisplay(ICoreClientAPI capi, List<ItemStack> stacks, ElementBounds bounds) : base(capi, bounds)
		{
			if (stacks == null || stacks.Count == 0)
			{
				throw new ArgumentException("Must provide at least one ItemStack", "stacks");
			}
			this.itemStacks = stacks;
			DummyInventory inv = new DummyInventory(capi, 1);
			inv.OnAcquireTransitionSpeed += ((EnumTransitionType _, ItemStack _, float mul) => 0f);
			this.slot = new DummySlot(this.itemStacks[0], inv);
			this.tooltip = new ItemstackTooltipRenderer(capi);
		}

		// Token: 0x060006BD RID: 1725 RVA: 0x0003316F File Offset: 0x0003136F
		public override void ComposeElements(Context ctx, ImageSurface surface)
		{
		}

		// Token: 0x060006BE RID: 1726 RVA: 0x00033174 File Offset: 0x00031374
		public override void RenderInteractiveElements(float deltaTime)
		{
			if (this.itemStacks.Count == 0)
			{
				return;
			}
			this.timeSinceLastSwitch += (double)deltaTime;
			if (this.timeSinceLastSwitch >= 1.0 && this.itemStacks.Count > 1)
			{
				this.timeSinceLastSwitch = 0.0;
				this.currentIndex = (this.currentIndex + 1) % this.itemStacks.Count;
				this.slot.Itemstack = this.itemStacks[this.currentIndex];
			}
			if (this.slot.Itemstack == null)
			{
				return;
			}
			this.Bounds.CalcWorldBounds();
			double cx = this.Bounds.renderX + this.Bounds.InnerWidth * 0.5;
			double cy = this.Bounds.renderY + this.Bounds.InnerHeight * 0.5;
			float sz = (float)(Math.Min(this.Bounds.InnerWidth, this.Bounds.InnerHeight) * 0.85);
			this.api.Render.RenderItemstackToGui(this.slot, cx, cy, 450.0, sz, -1, true, false, this.ShowStackSize);
			if (this.Bounds.PointInside(this.api.Input.MouseX, this.api.Input.MouseY))
			{
				this.tooltip.Render(this.slot, (double)this.api.Input.MouseX, (double)this.api.Input.MouseY, deltaTime);
			}
		}

		// Token: 0x060006BF RID: 1727 RVA: 0x00033312 File Offset: 0x00031512
		public override void Dispose()
		{
			base.Dispose();
			ItemstackTooltipRenderer itemstackTooltipRenderer = this.tooltip;
			if (itemstackTooltipRenderer == null)
			{
				return;
			}
			itemstackTooltipRenderer.Dispose();
		}

		// Token: 0x040002D0 RID: 720
		private readonly List<ItemStack> itemStacks;

		// Token: 0x040002D1 RID: 721
		private DummySlot slot;

		// Token: 0x040002D2 RID: 722
		private ItemstackTooltipRenderer tooltip;

		// Token: 0x040002D3 RID: 723
		private int currentIndex;

		// Token: 0x040002D4 RID: 724
		private double timeSinceLastSwitch;

		// Token: 0x040002D5 RID: 725
		private const double SwitchIntervalSeconds = 1.0;
	}
}
