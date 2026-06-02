using Cairo;
using Vintagestory.API.Client;

namespace SOAGuildsAndKingdoms.src.gui.components
{
    public class GuiElementPartyMemberInset : GuiElement
    {
        private readonly int depth;
        private readonly float normalBrightness;
        private readonly float hoverBrightness;
        private LoadedTexture? normalTexture;
        private LoadedTexture? hoverTexture;

        public override void OnMouseDown(ICoreClientAPI api, MouseEvent mouse)
        {
            // required for buttons to work
            mouse.Handled = false;
        }

        public GuiElementPartyMemberInset(ICoreClientAPI capi, ElementBounds bounds, int depth = 0, float normalBrightness = 0.75f, float hoverBrightness = 0.95f)
            : base(capi, bounds)
        {
            this.depth = depth;
            this.normalBrightness = normalBrightness;
            this.hoverBrightness = hoverBrightness;

            normalTexture = new LoadedTexture(capi);
            hoverTexture = new LoadedTexture(capi);
        }

        public override void ComposeElements(Context ctx, ImageSurface surface)
        {
            Bounds.CalcWorldBounds();

            ComposeTexture(ctx, surface, normalBrightness, ref normalTexture);
            ComposeTexture(ctx, surface, hoverBrightness, ref hoverTexture);
        }

        private void ComposeTexture(Context ctx, ImageSurface surface, float brightness, ref LoadedTexture? texture)
        {
            ImageSurface texSurface = new ImageSurface(Format.Argb32, (int)Bounds.OuterWidth, (int)Bounds.OuterHeight);
            Context texCtx = genContext(texSurface);

            if (brightness < 1)
            {
                texCtx.SetSourceRGBA(0, 0, 0, 1 - brightness);
                Rectangle(texCtx, 0, 0, Bounds.OuterWidth, Bounds.OuterHeight);
                texCtx.Fill();
            }

            EmbossRoundRectangleElement(texCtx, 0, 0, Bounds.OuterWidth, Bounds.OuterHeight, false, depth);

            generateTexture(texSurface, ref texture);

            texCtx.Dispose();
            texSurface.Dispose();
        }

        public override void RenderInteractiveElements(float deltaTime)
        {
            if (normalTexture == null || hoverTexture == null) return;

            bool isHovered = Bounds.PointInside(api.Input.MouseX, api.Input.MouseY);

            LoadedTexture textureToRender = isHovered ? hoverTexture : normalTexture;

            api.Render.Render2DTexturePremultipliedAlpha(
                textureToRender.TextureId,
                (int)Bounds.renderX,
                (int)Bounds.renderY,
                (int)Bounds.OuterWidth,
                (int)Bounds.OuterHeight,
                50
            );
        }

        public override void Dispose()
        {
            base.Dispose();
            normalTexture?.Dispose();
            hoverTexture?.Dispose();
        }
    }
}
