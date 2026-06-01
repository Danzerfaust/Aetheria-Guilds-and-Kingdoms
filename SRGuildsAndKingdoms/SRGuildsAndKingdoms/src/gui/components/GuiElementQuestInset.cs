using Cairo;
using Vintagestory.API.Client;
using System;

namespace SRGuildsAndKingdoms.src.gui.components
{
    /// <summary>
    /// Custom inset element for quest cards - replicates GuiElementInset functionality
    /// </summary>
    public class GuiElementQuestInset : GuiElement
    {
        private readonly int depth;
        private readonly float brightness;

        public override void OnMouseDown(ICoreClientAPI api, MouseEvent mouse)
        {
            // required for buttons to work
            mouse.Handled = false;
        }

        public GuiElementQuestInset(ICoreClientAPI capi, ElementBounds bounds, int depth = 4, float brightness = 0.85f)
            : base(capi, bounds)
        {
            this.depth = depth;
            this.brightness = brightness;
        }

        public override void ComposeElements(Context ctx, ImageSurface surface)
        {
            Bounds.CalcWorldBounds();

            // Draw dark background if brightness < 1
            if (brightness < 1)
            {
                ctx.SetSourceRGBA(0, 0, 0, 1 - brightness);
                Rectangle(ctx, Bounds);
                ctx.Fill();
            }

            // Draw embossed border
            EmbossRoundRectangleElement(ctx, Bounds, true, depth);
        }

        // Make this element non-interactive so clicks pass through to buttons underneath
        public override void RenderInteractiveElements(float deltaTime)
        {
            // Do nothing - this is purely decorative
        }
    }
}
