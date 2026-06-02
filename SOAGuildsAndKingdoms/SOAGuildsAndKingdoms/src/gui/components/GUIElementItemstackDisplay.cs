using Cairo;
using System;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;

namespace SOAGuildsAndKingdoms.src.gui.components
{
    /// <summary>
    /// so stupid, wasted so much time getting stack size working
    /// </summary>
    public class GuiElementItemstackDisplay : GuiElement
    {
        private readonly DummySlot slot;
        private readonly ItemstackTooltipRenderer tooltip;

        private LoadedTexture? stackSizeTexture;
        private int lastRenderedStackSize = -1;

        public bool ShowStackSize { get; set; } = true;

        public GuiElementItemstackDisplay(ICoreClientAPI capi, ItemStack stack, ElementBounds bounds)
            : base(capi, bounds)
        {
            var inv = new DummyInventory(capi);
            inv.OnAcquireTransitionSpeed += (_, _, mul) => 0f;
            slot = new DummySlot(stack, inv);
            tooltip = new ItemstackTooltipRenderer(capi);
        }

        public void SetStack(ItemStack stack)
        {
            slot.Itemstack = stack;
            lastRenderedStackSize = -1;
        }

        public override void ComposeElements(Context ctx, ImageSurface surface)
        {
            ComposeStackSizeTexture();
        }

        private void ComposeStackSizeTexture()
        {
            stackSizeTexture?.Dispose();
            stackSizeTexture = null;

            if (!ShowStackSize || slot.Itemstack == null || slot.Itemstack.StackSize <= 1)
            {
                lastRenderedStackSize = slot.Itemstack?.StackSize ?? 0;
                return;
            }

            float sz = (float)(Math.Min(Bounds.InnerWidth, Bounds.InnerHeight) * 0.85);
            float mul = sz / (float)GuiElement.scaled(20.6);

            CairoFont font = CairoFont.WhiteSmallText();
            font.UnscaledFontsize *= mul;
            font.FontWeight = FontWeight.Bold;
            font.StrokeColor = new double[] { 0, 0, 0, 1 };
            font.StrokeWidth = RuntimeEnv.GUIScale;

            stackSizeTexture = api.Gui.TextTexture.GenTextTexture(
                slot.Itemstack.StackSize.ToString(),
                font
            );

            lastRenderedStackSize = slot.Itemstack.StackSize;
        }

        public override void RenderInteractiveElements(float deltaTime)
        {
            if (slot.Itemstack == null) return;

            Bounds.CalcWorldBounds();

            if (ShowStackSize && slot.Itemstack.StackSize != lastRenderedStackSize)
            {
                ComposeStackSizeTexture();
            }

            double cx = Bounds.renderX + Bounds.InnerWidth * 0.5;
            double cy = Bounds.renderY + Bounds.InnerHeight * 0.5;

            float sz = (float)(Math.Min(Bounds.InnerWidth, Bounds.InnerHeight) * 0.85);

            api.Render.RenderItemstackToGui(
                slot,
                cx, cy,
                250,
                sz,
                ColorUtil.WhiteArgb,
                showStackSize: false
            );

            if (ShowStackSize && stackSizeTexture != null && stackSizeTexture.TextureId != 0)
            {
                float mul = sz / (float)GuiElement.scaled(25.6);

                api.Render.Render2DTexturePremultipliedAlpha(
                    stackSizeTexture.TextureId,
                    (float)((int)(cx + sz + 1.0 - stackSizeTexture.Width)),
                    (float)((int)(cy + mul * GuiElement.scaled(3.0) - GuiElement.scaled(4.0))),
                    stackSizeTexture.Width,
                    stackSizeTexture.Height,
                    350
                );
            }

            if (Bounds.PointInside(api.Input.MouseX, api.Input.MouseY))
            {
                tooltip.Render(slot, api.Input.MouseX, api.Input.MouseY, deltaTime);
            }
        }

        public override void Dispose()
        {
            base.Dispose();
            stackSizeTexture?.Dispose();
            tooltip?.Dispose();
        }
    }

    internal class ItemstackTooltipRenderer(ICoreClientAPI capi) : ItemstackComponentBase(capi)
    {
        public void Render(ItemSlot slot, double mouseX, double mouseY, float dt)
        {
            RenderItemstackTooltip(slot, mouseX, mouseY, dt);
        }
    }
}
