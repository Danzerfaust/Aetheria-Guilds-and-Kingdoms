using Cairo;
using SRGuildsAndKingdoms.src.party;
using System;
using System.Collections.Generic;
using System.Linq;
using Vintagestory.API.Client;
using Vintagestory.API.MathTools;
using Vintagestory.API.Config;

namespace SRGuildsAndKingdoms.src.gui
{
    public class PartyHud(ICoreClientAPI capi) : HudElement(capi)
    {
        private Party? currentParty;
        private readonly CairoFont Font = CairoFont.WhiteSmallishText().WithFontSize(12).WithColor([1, 1, 1, 1]).WithStroke([1, 1, 1, 0], 0).WithWeight(FontWeight.Bold);
        private readonly CairoFont TitleFont = CairoFont.WhiteSmallishText().WithFontSize(14).WithColor([1, 1, 1, 1]).WithStroke([1, 1, 1, 0], 0).WithWeight(FontWeight.Bold);
        private readonly CairoFont TitleShadowFont = CairoFont.WhiteSmallishText().WithFontSize(14).WithColor([0, 0, 0, 0.5]).WithStroke([0, 0, 0, 0], 0).WithWeight(FontWeight.Bold);

        private bool moving = false;
        private readonly Vec2i movingStartPos = new Vec2i();

        private Dictionary<string, (float health, float maxHealth)> memberHealthData = new();
        private long healthUpdateListenerId;
        private PartyMemberOutlineRenderer? outlineRenderer;

        public override string ToggleKeyCombinationCode => "partyhud";

        public void UpdateMemberHealth()
        {
            if (moving) return;
            if (currentParty == null) return;

            memberHealthData.Clear();

            foreach (var member in currentParty.Members)
            {
                var player = capi.World.AllOnlinePlayers.FirstOrDefault(p => p.PlayerUID == member.PlayerUid);
                if (player?.Entity != null)
                {
                    var healthTree = player.Entity.WatchedAttributes.GetTreeAttribute("health");
                    if (healthTree != null)
                    {
                        float currentHealth = healthTree.GetFloat("currenthealth");
                        float maxHealth = healthTree.GetFloat("maxhealth");
                        memberHealthData[member.PlayerName] = (currentHealth, maxHealth);
                    }
                }
            }

            outlineRenderer?.SetHealthData(memberHealthData);

            if (IsOpened())
            {
                ComposeDialog();
            }
        }

        public void UpdateParty(Party? party)
        {
            if (moving) return;

            currentParty = party;

            if (party == null)
            {
                capi.Event.UnregisterGameTickListener(healthUpdateListenerId);
                outlineRenderer?.SetParty(null);
                TryClose();
                return;
            }

            outlineRenderer ??= new PartyMemberOutlineRenderer(capi);

            outlineRenderer.SetParty(party);

            capi.Event.UnregisterGameTickListener(healthUpdateListenerId);
            healthUpdateListenerId = capi.Event.RegisterGameTickListener((dt) => UpdateMemberHealth(), 2000);

            UpdateMemberHealth();
            ComposeDialog();
            TryOpen();
        }

        private void ComposeDialog()
        {
            if (currentParty == null) return;

            ClearComposers();

            var sortedMembers = currentParty.Members
                .OrderBy(m => !m.IsOnline)
                .ThenBy(m => !memberHealthData.ContainsKey(m.PlayerName))
                .ThenBy(m =>
                {
                    if (memberHealthData.TryGetValue(m.PlayerName, out var healthData) && healthData.maxHealth > 0)
                    {
                        return healthData.health / healthData.maxHealth;
                    }
                    return 1f;
                })
                .ToList();

            int maxPlayers = Math.Min(sortedMembers.Count, 12);
            int width = 200;
            int lineHeight = 25;
            int hpBarHeight = 8;
            int hpBarSpacing = 2;
            int padding = 10;
            int titleHeight = 14;
            int titleOffset = 5;
            int totalLineHeight = lineHeight + hpBarHeight + hpBarSpacing;
            int height = maxPlayers * totalLineHeight + padding + titleHeight + padding;

            ElementBounds hudBounds = ElementBounds.Fixed(EnumDialogArea.RightMiddle, 10, -height / 2, width, height);

            Vec2i? savedPos = capi.Gui.GetDialogPosition("partyhud");
            if (savedPos != null)
            {
                bool isOnScreen = savedPos.X >= 0 &&
                                  savedPos.Y >= 0 &&
                                  savedPos.X + width <= capi.Render.FrameWidth &&
                                  savedPos.Y + height <= capi.Render.FrameHeight;

                if (isOnScreen)
                {
                    hudBounds.Alignment = EnumDialogArea.None;
                    hudBounds.fixedX = savedPos.X;
                    hudBounds.fixedY = savedPos.Y;
                    hudBounds.absMarginX = 0;
                    hudBounds.absMarginY = 0;
                }
                else
                {
                    capi.Gui.SetDialogPosition("partyhud", null);
                }
            }

            var containerBounds = ElementBounds.Fill.WithFixedPadding(0);

            var composer = capi.Gui.CreateCompo("partyhud", hudBounds)
                .BeginChildElements(containerBounds);

            var bgDrawBounds = ElementBounds.Fixed(0, titleHeight - titleOffset, width, height - (titleHeight - titleOffset));
            composer.AddInset(bgDrawBounds, 0, 0.7f);

            double[] angles = [0, 45, 90, 135, 180, 225, 270, 315];
            for (int layer = 2; layer >= 1; layer--)
            {
                double distance = layer * 0.8;
                double alpha = 0.1;
                var shadowFont = CairoFont.WhiteSmallishText().WithFontSize(14).WithColor([0, 0, 0, alpha]).WithStroke([0, 0, 0, 0], 0).WithWeight(FontWeight.Bold);

                foreach (double angle in angles)
                {
                    double radians = angle * Math.PI / 180.0;
                    double offsetX = Math.Cos(radians) * distance;
                    double offsetY = Math.Sin(radians) * distance;
                    var shadowBounds = ElementBounds.Fixed(padding + offsetX, offsetY, width - padding * 2, titleHeight);
                    composer.AddStaticText("Party", shadowFont, shadowBounds);
                }
            }

            var titleBounds = ElementBounds.Fixed(padding, 0, width - padding * 2, titleHeight);
            composer.AddStaticText("Party", TitleFont, titleBounds);

            double yPos = titleHeight + padding;

            for (int i = 0; i < maxPlayers; i++)
            {
                var member = sortedMembers[i];
                var textBounds = ElementBounds.Fixed(padding, yPos, width - padding * 2, lineHeight);

                var font = member.IsOnline ? Font : Font.Clone().WithColor([1, 1, 1, 0.7]);

                composer.AddStaticText(member.PlayerName, font, textBounds);

                double hpBarY = yPos + lineHeight - 10;
                var hpBarBounds = ElementBounds.Fixed(padding, hpBarY, width - padding * 2, hpBarHeight);

                float healthPercent = 0f;
                float? healthValue = null;
                if (memberHealthData.TryGetValue(member.PlayerName, out var healthData))
                {
                    healthPercent = healthData.maxHealth > 0 ? healthData.health / healthData.maxHealth : 0f;
                    healthValue = healthData.health;
                }

                composer.AddDynamicCustomDraw(hpBarBounds, (ctx, surface, bounds) => DrawHealthBar(ctx, bounds, healthPercent, member.IsOnline));

                int shadowPadding = 4;
                var textBoundsHp = ElementBounds.Fixed(padding - shadowPadding, yPos + 26 - shadowPadding, width - padding * 2 + shadowPadding * 2, lineHeight + shadowPadding * 2);
                composer.AddDynamicCustomDraw(textBoundsHp, (ctx, surface, bounds) => DrawHealthText(ctx, bounds, healthValue, shadowPadding, member.IsOnline));

                yPos += totalLineHeight;
            }

            composer
                .EndChildElements();

            SingleComposer = composer.Compose();

            Composers["partyinvitepopup"] = composer;
        }

        public override void OnMouseDown(MouseEvent args)
        {
            base.OnMouseDown(args);

            if (SingleComposer != null && SingleComposer.Bounds.PointInside(args.X, args.Y))
            {
                moving = true;
                movingStartPos.Set(args.X, args.Y);
            }
        }

        public override void OnMouseMove(MouseEvent args)
        {
            base.OnMouseMove(args);

            if (moving && SingleComposer != null)
            {
                if (SingleComposer.Bounds.Alignment != EnumDialogArea.None)
                {
                    SingleComposer.Bounds.fixedX = SingleComposer.Bounds.absX / RuntimeEnv.GUIScale;
                    SingleComposer.Bounds.fixedY = SingleComposer.Bounds.absY / RuntimeEnv.GUIScale;
                    SingleComposer.Bounds.Alignment = EnumDialogArea.None;
                    SingleComposer.Bounds.absMarginX = 0;
                    SingleComposer.Bounds.absMarginY = 0;
                }

                SingleComposer.Bounds.fixedX += (args.X - movingStartPos.X) / RuntimeEnv.GUIScale;
                SingleComposer.Bounds.fixedY += (args.Y - movingStartPos.Y) / RuntimeEnv.GUIScale;
                movingStartPos.Set(args.X, args.Y);
                SingleComposer.Bounds.CalcWorldBounds();
            }
        }

        public override void OnMouseUp(MouseEvent args)
        {
            base.OnMouseUp(args);

            if (moving && SingleComposer != null)
            {
                capi.Gui.SetDialogPosition("partyhud", new Vec2i((int)SingleComposer.Bounds.fixedX, (int)SingleComposer.Bounds.fixedY));
            }

            moving = false;
        }

        private void DrawHealthBar(Context ctx, ElementBounds bounds, float healthPercent, bool isOnline)
        {
            double width = bounds.InnerWidth;
            double height = bounds.InnerHeight;

            var opacity = 0.8;

            if (!isOnline)
            {
                opacity = 0.6;
            }

            ctx.Rectangle(0, 0, width, height);
            ctx.SetSourceRGBA(0.14, 0.38, 0.55, opacity);
            ctx.Fill();

            double healthWidth = width * Math.Clamp(healthPercent, 0, 1);
            if (healthWidth > 0)
            {
                ctx.Rectangle(0, 0, healthWidth, height);

                ctx.SetSourceRGBA(0.9, 0, 0, 0.9);

                ctx.Fill();
            }

            ctx.Rectangle(0, 0, width, height);
            ctx.SetSourceRGBA(1, 1, 1, opacity);
            ctx.LineWidth = 1;
            ctx.Stroke();
        }

        private void DrawHealthText(Context ctx, ElementBounds bounds, float? healthValue, int shadowPadding, bool isOnline)
        {
            string healthText = healthValue.HasValue ? healthValue.Value.ToString("F2") : "Out of Range";
            if (!isOnline)
            {
                healthText = "Offline";
            }

            var font = TitleFont.Clone().WithFontSize(12);
            double[] angles = [0, 45, 90, 135, 180, 225, 270, 315];

            for (int layer = 2; layer >= 1; layer--)
            {
                double distance = layer * 0.8;
                double alpha = 0.1;

                foreach (double angle in angles)
                {
                    double radians = angle * Math.PI / 180.0;
                    double offsetX = Math.Cos(radians) * distance;
                    double offsetY = Math.Sin(radians) * distance;

                    font.SetupContext(ctx);
                    TextExtents extents = ctx.TextExtents(healthText);
                    double textX = bounds.InnerWidth - extents.Width - shadowPadding + offsetX;
                    double textY = extents.Height + shadowPadding + offsetY;

                    ctx.SetSourceRGBA(0.14, 0.38, 0.55, alpha);
                    ctx.MoveTo(textX, textY);
                    ctx.ShowText(healthText);
                }
            }

            font.SetupContext(ctx);
            TextExtents finalExtents = ctx.TextExtents(healthText);
            double finalX = bounds.InnerWidth - finalExtents.Width - shadowPadding;
            double finalY = finalExtents.Height + shadowPadding;

            ctx.SetSourceRGBA(0.9, 0.9, 0.9, 1);
            ctx.MoveTo(finalX, finalY);
            ctx.ShowText(healthText);
        }
    }
}
