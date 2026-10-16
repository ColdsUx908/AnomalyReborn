using Anomalies.Assets.Effects;
using Anomalies.Visuals.Overlays;
using Terraria.Graphics.Effects;
using Transoceanic.DataStructures.Rendering;

namespace Anomalies.Bosses.Skeletron.LegendaryNight.Arena;

public sealed class LegendaryNightOverlay : CustomOverlay
{
    public LegendaryNightArena Arena;

    public override void Update()
    {
        Arena = null;
        foreach (Projectile projectile in Projectile.ActiveProjectiles)
        {
            if (projectile.ModProjectile is LegendaryNightArena arena && (Arena is null || arena.Intensity > Arena.Intensity))
                Arena = arena;
        }
    }

    public override void Draw(SpriteBatch spriteBatch, RenderLayers layer, bool beginSpriteBatch)
    {
        if (Main.gameMenu || layer != RenderLayers.All || Arena is null || !Arena.Projectile.active || Arena.Intensity <= 0f)
            return;

        GraphicsDevice device = Main.instance.GraphicsDevice;
        using RenderTargetLease lease = RenderTargetPool.Shared.Rent(
            device,
            Main.screenWidth,
            Main.screenHeight,
            RenderTargetDescriptor.Default
        );

        using (lease.Scope(clearColor: Color.Transparent))
        {
            //绘制竞技场遮罩

            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, Main.Rasterizer, null, Matrix.Identity);

            AnomalyEffects.Overlays.LegendaryNightOverlay.Data
                .UseImage1(AnomalyTextures.Noise._Wave)
                .SetCustomParameter("uScreenResolution", Main.ScreenSize.ToVector2() * Math.Max(Main.UIScale, 1f) / 2f)
                .SetCustomParameter("uScreenPosition", Main.screenPosition)
                .SetCustomParameter("uInverseViewMatrix", Matrix.Invert(Main.GameViewMatrix.TransformationMatrix))
                .Apply();

            spriteBatch.Draw(TOTextures.FillerTexture, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), Color.White);

            spriteBatch.End();

            //擦除竞技场圆形

            spriteBatch.Begin(SpriteSortMode.Deferred, TODrawUtils.EraseBlend, SamplerState.LinearClamp, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            spriteBatch.DrawFromCenter(BloomParticle.BloomCircleLarge, Arena.Projectile.Center - Main.screenPosition, null, Color.White * 1.5f, 0f, Arena.Radius / BloomParticle.BloomCircleLargeRadius);
            spriteBatch.End();
        }

        spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, Main.Rasterizer, null, Matrix.Identity);
        spriteBatch.Draw(lease.Target, Vector2.Zero, null, Color.White * Arena.Intensity, 0, Vector2.Zero, 1, 0, 0);
        spriteBatch.End();
    }
}
