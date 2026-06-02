using SOAGuildsAndKingdoms.src.party;
using System;
using System.Collections.Generic;
using System.Linq;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;
using Cairo;

namespace SOAGuildsAndKingdoms.src.gui
{
    public class PartyMemberOutlineRenderer : IRenderer
    {
        private ICoreClientAPI capi;
        private Party? currentParty;
        private Dictionary<string, (float health, float maxHealth)> healthData;
        private LoadedTexture? hpBarBackTexture;
        private LoadedTexture? hpBarFillTexture;

        public double RenderOrder => 1.0;
        public int RenderRange => 30;

        public PartyMemberOutlineRenderer(ICoreClientAPI capi)
        {
            this.capi = capi;
            this.healthData = new Dictionary<string, (float health, float maxHealth)>();
            CreateHPBarTextures();
            capi.Event.RegisterRenderer(this, EnumRenderStage.Ortho, "partymemberhpbars");
        }

        private void CreateHPBarTextures()
        {
            int width = 220;
            int height = 16;

            ImageSurface surface = new ImageSurface(Format.Argb32, width, height);
            Context ctx = new Context(surface);
            ctx.SetSourceRGBA(0.14, 0.38, 0.55, 200.0 / 255);
            ctx.Paint();
            ctx.Dispose();

            hpBarBackTexture = new LoadedTexture(capi);
            capi.Gui.LoadOrUpdateCairoTexture(surface, true, ref hpBarBackTexture);
            surface.Dispose();

            surface = new ImageSurface(Format.Argb32, width, height);
            ctx = new Context(surface);
            ctx.SetSourceRGBA(0.9, 0, 0, 230.0 / 255);
            ctx.Paint();
            ctx.Dispose();

            hpBarFillTexture = new LoadedTexture(capi);
            capi.Gui.LoadOrUpdateCairoTexture(surface, true, ref hpBarFillTexture);
            surface.Dispose();
        }

        public void SetParty(Party? party)
        {
            currentParty = party;
        }

        public void SetHealthData(Dictionary<string, (float health, float maxHealth)> data)
        {
            healthData = data;
        }

        public void OnRenderFrame(float deltaTime, EnumRenderStage stage)
        {
            if (stage != EnumRenderStage.Ortho) return;
            if (currentParty == null) return;
            if (hpBarBackTexture == null || hpBarFillTexture == null) return;

            var player = capi.World.Player;
            if (player?.Entity == null) return;

            IRenderAPI rapi = capi.Render;

            foreach (var member in currentParty.Members)
            {
                var memberPlayer = capi.World.AllOnlinePlayers.FirstOrDefault(p => p.PlayerUID == member.PlayerUid);
                if (memberPlayer?.Entity == null || memberPlayer.Entity == player.Entity) continue;

                float healthPercent = 1f;
                if (healthData.TryGetValue(member.PlayerName, out var hp))
                {
                    healthPercent = hp.maxHealth > 0 ? hp.health / hp.maxHealth : 0f;
                }

                RenderHPBar(memberPlayer.Entity, healthPercent, rapi);
            }
        }

        private void RenderHPBar(Entity entity, float healthPercent, IRenderAPI rapi)
        {
            var esr = entity.Properties?.Client?.Renderer as EntityShapeRenderer;
            if (esr == null) return;

            if (hpBarBackTexture == null || hpBarFillTexture == null) return;

            Vec3d aboveHeadPos = esr.getAboveHeadPosition(capi.World.Player.Entity);
            aboveHeadPos.Y -= 0.1;

            Vec3d screenPos = MatrixToolsd.Project(
                aboveHeadPos,
                rapi.PerspectiveProjectionMat,
                rapi.PerspectiveViewMat,
                rapi.FrameWidth,
                rapi.FrameHeight
            );

            if (screenPos.Z < 0) return;

            float scale = 4f / Math.Max(1, (float)screenPos.Z);
            float cappedScale = Math.Min(1f, scale);
            if (cappedScale > 0.75f) cappedScale = 0.75f + (cappedScale - 0.75f) / 2;

            double dist = capi.World.Player.Entity.Pos.SquareDistanceTo(entity.Pos);
            if (RenderRange * RenderRange < dist) return;

            float barWidth = cappedScale * hpBarBackTexture.Width;
            float barHeight = cappedScale * hpBarBackTexture.Height;
            float posx = (float)screenPos.X - barWidth / 2;
            float posy = rapi.FrameHeight - (float)screenPos.Y - barHeight;

            rapi.Render2DTexture(
                hpBarBackTexture.TextureId,
                posx,
                posy,
                barWidth,
                barHeight,
                20
            );

            if (healthPercent > 0)
            {
                float fillWidth = barWidth * healthPercent;
                rapi.Render2DTexture(
                    hpBarFillTexture.TextureId,
                    posx,
                    posy,
                    fillWidth,
                    barHeight,
                    20
                );
            }
        }

        public void Dispose()
        {
            hpBarBackTexture?.Dispose();
            hpBarFillTexture?.Dispose();
            capi.Event.UnregisterRenderer(this, EnumRenderStage.Ortho);
        }
    }
}
