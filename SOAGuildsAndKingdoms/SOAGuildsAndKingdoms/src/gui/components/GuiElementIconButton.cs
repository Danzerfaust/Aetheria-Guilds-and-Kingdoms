using Cairo;
using System;
using Vintagestory.API.Client;

namespace SOAGuildsAndKingdoms.src.gui.components
{
    public class GuiElementIconButton(ICoreClientAPI capi, string icon, string text, CairoFont font, Action<bool> onToggled, ElementBounds bounds, bool toggleButton) : GuiElementToggleButton(capi, icon, text, font, onToggled, bounds, toggleButton)
    {
        LoadedTexture releasedTexture = new(capi);
        LoadedTexture hoverTexture = new(capi);
        private readonly string _icon = icon;

        public override void ComposeElements(Context ctx, ImageSurface surface)
        {
            Bounds.CalcWorldBounds();

            ComposeReleasedButton();
            ComposeHoveredButton();
        }

        void ComposeReleasedButton()
        {
            ImageSurface surface = new(Format.Argb32, (int)(Bounds.OuterWidth), (int)(Bounds.OuterHeight));
            Context ctx = genContext(surface);

            if (_icon != null && _icon.Length > 0)
            {
                api.Gui.Icons.DrawIcon(ctx, _icon, Bounds.absPaddingX + scaled(2), Bounds.absPaddingY + scaled(5), Bounds.InnerWidth - scaled(8), Bounds.InnerHeight - scaled(8), Font.Color);
            }

            generateTexture(surface, ref releasedTexture);

            ctx.Dispose();
            surface.Dispose();
        }

        void ComposeHoveredButton()
        {
            ImageSurface surface = new(Format.Argb32, (int)(Bounds.OuterWidth), (int)(Bounds.OuterHeight));
            Context ctx = genContext(surface);

            if (_icon != null && _icon.Length > 0)
            {
                api.Gui.Icons.DrawIcon(ctx, _icon, Bounds.absPaddingX + scaled(2), Bounds.absPaddingY + scaled(5), Bounds.InnerWidth - scaled(8), Bounds.InnerHeight - scaled(8), [0.7, 0.7, 0.7, 1]);
            }

            generateTexture(surface, ref hoverTexture);

            ctx.Dispose();
            surface.Dispose();
        }

        public override void RenderInteractiveElements(float deltaTime)
        {
            api.Render.Render2DTexturePremultipliedAlpha(Bounds.PointInside(api.Input.MouseX, api.Input.MouseY) ? hoverTexture.TextureId : releasedTexture.TextureId, Bounds);
        }

        public override void Dispose()
        {
            releasedTexture.Dispose();
            hoverTexture.Dispose();

            base.Dispose();
        }
    }
}