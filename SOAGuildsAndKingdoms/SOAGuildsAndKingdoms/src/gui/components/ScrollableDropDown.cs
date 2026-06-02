using Vintagestory.API.Client;

namespace SOAGuildsAndKingdoms.src.gui.components
{
    public class ScrollableDropDown(ICoreClientAPI capi, string[] values, string[] names, int selectedIndex,
        SelectionChangedDelegate onSelectionChanged, ElementBounds bounds, CairoFont font, bool multiSelect = false) : GuiElementDropDown(capi, values, names, selectedIndex, onSelectionChanged, bounds, font, multiSelect)
    {
        public override void RenderInteractiveElements(float deltaTime)
        {
            // Re-sync highlightBounds before every render
            highlightBounds?.CalcWorldBounds();
            base.RenderInteractiveElements(deltaTime);
        }
    }
}
