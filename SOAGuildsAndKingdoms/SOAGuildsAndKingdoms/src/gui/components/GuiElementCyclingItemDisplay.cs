using System;
using System.Collections.Generic;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace SOAGuildsAndKingdoms.src.gui.components
{
    /// <summary>
    /// Cycles through multiple ItemStacks, displaying one at a time with smooth transitions.
    /// Similar to SlideShowItemStack behavior for quest objectives with multiple valid items.
    /// </summary>
    public class GuiElementCyclingItemDisplay : GuiElement
    {
        private readonly List<ItemStack> itemStacks;
        private DummySlot slot;
        private ItemstackTooltipRenderer tooltip;
        private int currentIndex = 0;
        private double timeSinceLastSwitch = 0;
        private const double SwitchIntervalSeconds = 1.0; // Switch every second

        /// <summary>
        /// Whether to show the quantity number in the bottom-right corner.
        /// </summary>
        public bool ShowStackSize { get; set; } = true;

        public GuiElementCyclingItemDisplay(ICoreClientAPI capi, List<ItemStack> stacks, ElementBounds bounds)
            : base(capi, bounds)
        {
            if (stacks == null || stacks.Count == 0)
                throw new ArgumentException("Must provide at least one ItemStack", nameof(stacks));

            itemStacks = stacks;

            var inv = new DummyInventory(capi);
            inv.OnAcquireTransitionSpeed += (_, _, mul) => 0f;
            slot = new DummySlot(itemStacks[0], inv);
            tooltip = new ItemstackTooltipRenderer(capi);
        }

        public override void ComposeElements(Cairo.Context ctx, Cairo.ImageSurface surface)
        {
            // Nothing to compose � no background drawn
        }

        public override void RenderInteractiveElements(float deltaTime)
        {
            if (itemStacks.Count == 0) return;

            // Update cycling logic
            timeSinceLastSwitch += deltaTime;
            if (timeSinceLastSwitch >= SwitchIntervalSeconds && itemStacks.Count > 1)
            {
                timeSinceLastSwitch = 0;
                currentIndex = (currentIndex + 1) % itemStacks.Count;
                slot.Itemstack = itemStacks[currentIndex];
            }

            if (slot.Itemstack == null) return;

            Bounds.CalcWorldBounds();

            double cx = Bounds.renderX + Bounds.InnerWidth * 0.5;
            double cy = Bounds.renderY + Bounds.InnerHeight * 0.5;

            // Use the smaller of width/height to fit within bounds, with some padding
            float sz = (float)(Math.Min(Bounds.InnerWidth, Bounds.InnerHeight) * 0.85);

            api.Render.RenderItemstackToGui(
                slot,
                cx, cy,
                450,  // z-depth
                sz,   // size scaled to bounds
                ColorUtil.WhiteArgb,
                showStackSize: ShowStackSize
            );

            // Show tooltip when mouse is inside our bounds
            if (Bounds.PointInside(api.Input.MouseX, api.Input.MouseY))
            {
                tooltip.Render(slot, api.Input.MouseX, api.Input.MouseY, deltaTime);
            }
        }

        public override void Dispose()
        {
            base.Dispose();
            tooltip?.Dispose();
        }
    }
}
