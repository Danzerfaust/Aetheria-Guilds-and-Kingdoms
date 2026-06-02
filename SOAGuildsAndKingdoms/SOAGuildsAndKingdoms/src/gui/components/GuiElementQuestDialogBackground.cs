using Cairo;
using Vintagestory.API.Client;
using Vintagestory.API.Common;

namespace SOAGuildsAndKingdoms.src.gui.components
{
    /// <summary>
    /// Custom dialog background using quest-themed texture
    /// </summary>
    public class GuiElementQuestDialogBackground : GuiElement
    {
        public bool Shade = true;
        private readonly bool withTitlebar;
        private readonly double strokeWidth = 0;

        public float Alpha = 1;

        public bool FullBlur = false;

        private const string QuestBgTextureName = "soaguildsandkingdoms:gui/quest_bg.png";

        /// <summary>
        /// Adds a quest-themed background to the dialog
        /// </summary>
        /// <param name="capi">The Client API</param>
        /// <param name="bounds">The bounds of the element</param>
        /// <param name="withTitlebar">Minor style adjustments to accommodate title bar</param>
        /// <param name="strokeWidth">The stroke width for the border</param>
        /// <param name="alpha">Alpha transparency value</param>
        public GuiElementQuestDialogBackground(ICoreClientAPI capi, ElementBounds bounds, bool withTitlebar, double strokeWidth = 0, float alpha = 1) : base(capi, bounds)
        {
            this.strokeWidth = strokeWidth;
            this.withTitlebar = withTitlebar;
            this.Alpha = alpha;
        }

        public override void ComposeElements(Context ctx, ImageSurface surface)
        {
            Bounds.CalcWorldBounds();
            double titleBarOffY = withTitlebar ? scaled(GuiStyle.TitleBarHeight) : 0;

            RoundRectangle(ctx, Bounds.bgDrawX, Bounds.bgDrawY + titleBarOffY, Bounds.OuterWidth, Bounds.OuterHeight - titleBarOffY - 1, GuiStyle.DialogBGRadius);

            ctx.SetSourceRGBA(GuiStyle.DialogStrongBgColor[0] * 1, GuiStyle.DialogStrongBgColor[1] * 1, GuiStyle.DialogStrongBgColor[2] * 1, GuiStyle.DialogStrongBgColor[3] * 1);
            ctx.FillPreserve();

            if (Shade)
            {
                ctx.SetSourceRGBA(GuiStyle.DialogLightBgColor[0] * 2.1, GuiStyle.DialogStrongBgColor[1] * 2.1, GuiStyle.DialogStrongBgColor[2] * 2.1, 1);

                ctx.LineWidth = strokeWidth * 2;
                ctx.StrokePreserve();

                var r = scaled(9);
                if (FullBlur)
                {
                    surface.BlurFull(r);
                }
                else
                {
                    surface.BlurPartial(r, (int)(2 * r + 1), (int)Bounds.bgDrawX, (int)(Bounds.bgDrawY + titleBarOffY), (int)Bounds.OuterWidth, (int)Bounds.OuterHeight);
                }
            }

            SurfacePattern pattern = getPattern(api, QuestBgTextureName, true, (int)(Alpha * 255), 1.0f);
            ctx.SetSource(pattern);
            ctx.FillPreserve();
            ctx.Operator = Operator.Over;

            if (Shade)
            {
                ctx.SetSourceRGBA(new double[] { 45 / 255.0, 35 / 255.0, 33 / 255.0, Alpha * Alpha });
                ctx.LineWidth = strokeWidth;
                ctx.Stroke();
            }
            else
            {
                ctx.SetSourceRGBA(new double[] { 45 / 255.0, 35 / 255.0, 33 / 255.0, Alpha });
                ctx.LineWidth = scaled(2);
                ctx.Stroke();
            }
        }
    }

    /// <summary>
    /// Extension methods for adding quest dialog backgrounds to GUI composers
    /// </summary>
    public static class GuiComposerQuestBackgroundExtensions
    {
        /// <summary>
        /// Adds shaded quest-themed background to the GUI
        /// </summary>
        public static GuiComposer AddQuestDialogBG(this GuiComposer composer, ElementBounds bounds, bool withTitleBar = true, double strokeWidth = 5, float alpha = 0.75f)
        {
            if (!composer.Composed)
            {
                composer.AddStaticElement(new GuiElementQuestDialogBackground(composer.Api, bounds, withTitleBar, strokeWidth, alpha));
            }
            return composer;
        }
    }
}
