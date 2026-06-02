using System;
using Vintagestory.API.Client;

namespace SOAGuildsAndKingdoms.src.gui.components
{
    public class ScrollableTextInput(ICoreClientAPI capi, ElementBounds bounds, Action<string> onTextChanged, CairoFont font) : GuiElementTextInput(capi, bounds, onTextChanged, font)
    {
        public override void RenderInteractiveElements(float deltaTime)
        {
            // Re-sync highlightBounds before every render — it's protected so we can access it here
            highlightBounds.CalcWorldBounds();
            base.RenderInteractiveElements(deltaTime);
        }
    }
}
