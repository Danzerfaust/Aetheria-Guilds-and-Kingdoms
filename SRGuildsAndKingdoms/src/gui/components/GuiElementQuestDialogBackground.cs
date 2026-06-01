using System;
using System.Runtime.CompilerServices;
using Cairo;
using Vintagestory.API.Client;

namespace SRGuildsAndKingdoms.src.gui.components
{
	// Token: 0x0200009A RID: 154
	[NullableContext(1)]
	[Nullable(0)]
	public class GuiElementQuestDialogBackground : GuiElement
	{
		// Token: 0x060006CA RID: 1738 RVA: 0x00033723 File Offset: 0x00031923
		public GuiElementQuestDialogBackground(ICoreClientAPI capi, ElementBounds bounds, bool withTitlebar, double strokeWidth = 0.0, float alpha = 1f) : base(capi, bounds)
		{
			this.strokeWidth = strokeWidth;
			this.withTitlebar = withTitlebar;
			this.Alpha = alpha;
		}

		// Token: 0x060006CB RID: 1739 RVA: 0x00033758 File Offset: 0x00031958
		public override void ComposeElements(Context ctx, ImageSurface surface)
		{
			this.Bounds.CalcWorldBounds();
			double titleBarOffY = this.withTitlebar ? GuiElement.scaled(GuiStyle.TitleBarHeight) : 0.0;
			GuiElement.RoundRectangle(ctx, this.Bounds.bgDrawX, this.Bounds.bgDrawY + titleBarOffY, this.Bounds.OuterWidth, this.Bounds.OuterHeight - titleBarOffY - 1.0, GuiStyle.DialogBGRadius);
			ctx.SetSourceRGBA(GuiStyle.DialogStrongBgColor[0] * 1.0, GuiStyle.DialogStrongBgColor[1] * 1.0, GuiStyle.DialogStrongBgColor[2] * 1.0, GuiStyle.DialogStrongBgColor[3] * 1.0);
			ctx.FillPreserve();
			if (this.Shade)
			{
				ctx.SetSourceRGBA(GuiStyle.DialogLightBgColor[0] * 2.1, GuiStyle.DialogStrongBgColor[1] * 2.1, GuiStyle.DialogStrongBgColor[2] * 2.1, 1.0);
				ctx.LineWidth = this.strokeWidth * 2.0;
				ctx.StrokePreserve();
				double r = GuiElement.scaled(9.0);
				if (this.FullBlur)
				{
					SurfaceTransformBlur.BlurFull(surface, r);
				}
				else
				{
					SurfaceTransformBlur.BlurPartial(surface, r, (int)(2.0 * r + 1.0), (int)this.Bounds.bgDrawX, (int)(this.Bounds.bgDrawY + titleBarOffY), (int)this.Bounds.OuterWidth, (int)this.Bounds.OuterHeight);
				}
			}
			SurfacePattern pattern = GuiElement.getPattern(this.api, "srguildsandkingdoms:gui/quest_bg.png", true, (int)(this.Alpha * 255f), 1f);
			ctx.SetSource(pattern);
			ctx.FillPreserve();
			ctx.Operator = 2;
			if (this.Shade)
			{
				ctx.SetSourceRGBA(new double[]
				{
					0.17647058823529413,
					0.13725490196078433,
					0.12941176470588237,
					0.0
				});
				ctx.LineWidth = this.strokeWidth;
				ctx.Stroke();
				return;
			}
			ctx.SetSourceRGBA(new double[]
			{
				0.17647058823529413,
				0.13725490196078433,
				0.12941176470588237,
				0.0
			});
			ctx.LineWidth = GuiElement.scaled(2.0);
			ctx.Stroke();
		}

		// Token: 0x040002DC RID: 732
		public bool Shade = true;

		// Token: 0x040002DD RID: 733
		private readonly bool withTitlebar;

		// Token: 0x040002DE RID: 734
		private readonly double strokeWidth;

		// Token: 0x040002DF RID: 735
		public float Alpha = 1f;

		// Token: 0x040002E0 RID: 736
		public bool FullBlur;

		// Token: 0x040002E1 RID: 737
		private const string QuestBgTextureName = "srguildsandkingdoms:gui/quest_bg.png";
	}
}
