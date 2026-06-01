using System;
using System.Runtime.CompilerServices;
using Vintagestory.API.Client;

namespace SRGuildsAndKingdoms.src.gui.components
{
	// Token: 0x0200009B RID: 155
	public static class GuiComposerQuestBackgroundExtensions
	{
		// Token: 0x060006CC RID: 1740 RVA: 0x000339B6 File Offset: 0x00031BB6
		[NullableContext(1)]
		public static GuiComposer AddQuestDialogBG(this GuiComposer composer, ElementBounds bounds, bool withTitleBar = true, double strokeWidth = 5.0, float alpha = 0.75f)
		{
			if (!composer.Composed)
			{
				composer.AddStaticElement(new GuiElementQuestDialogBackground(composer.Api, bounds, withTitleBar, strokeWidth, alpha), null);
			}
			return composer;
		}
	}
}
