using Anomalies.Visuals.Overlays;
using Terraria.Graphics.Effects;
using Transoceanic.DataStructures.Rendering;

namespace Anomalies.Bosses.Skeletron.LegendaryNight.Entities;

public sealed class CoinJudgmentOverlay : CustomOverlay
{
    public override void Draw(SpriteBatch spriteBatch, RenderLayers layer, bool beginSpriteBatch)
    {
        if (Main.gameMenu || Main.dedServ || layer != RenderLayers.All)
            return;

        // 每个判定单独合成，避免一种币的安全区擦掉另一种币的危险区域。
        foreach (Projectile projectile in Projectile.ActiveProjectiles)
        {
            if (projectile.ModProjectile is not CoinJudgment judgment || judgment.OverlayOpacity <= 0f)
                continue;

            using RenderTargetLease lease = RenderTargetPool.Shared.Rent(Main.instance.GraphicsDevice,
                Main.screenWidth, Main.screenHeight, RenderTargetDescriptor.Default);
            using (lease.Scope(clearColor: Color.Transparent))
                DrawMask(spriteBatch, projectile);

            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, Main.Rasterizer, null, Matrix.Identity);
            spriteBatch.Draw(lease.Target, Vector2.Zero, Color.White * judgment.OverlayOpacity);
            spriteBatch.End();
        }
    }

    private static void DrawMask(SpriteBatch spriteBatch, Projectile projectile)
    {
        int coinType = (int)projectile.ai[0];
        Vector2 center = projectile.Center - Main.screenPosition;
        Texture2D circle = AnomalyTextures.Extra.BasicCircleLarge;
        Texture2D filler = TOTextures.FillerTexture;

        // 将屏幕四角逆变换，条带在缩放及相机变换后仍覆盖整个视口。
        Matrix inverse = Matrix.Invert(Main.GameViewMatrix.TransformationMatrix);
        Vector2 a = Vector2.Transform(Vector2.Zero, inverse);
        Vector2 b = Vector2.Transform(new Vector2(Main.screenWidth, 0f), inverse);
        Vector2 c = Vector2.Transform(new Vector2(0f, Main.screenHeight), inverse);
        Vector2 d = Vector2.Transform(Main.ScreenSize.ToVector2(), inverse);
        Vector2 min = Vector2.Min(Vector2.Min(a, b), Vector2.Min(c, d)) - Vector2.One;
        Vector2 max = Vector2.Max(Vector2.Max(a, b), Vector2.Max(c, d)) + Vector2.One;

        if (coinType == 3)
        {
            // 有限宽度的安全带：先涂白全屏，再擦除中间，左右没有外边界。
            Begin(BlendState.AlphaBlend, Matrix.Identity);
            spriteBatch.Draw(filler, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), Color.White);
            spriteBatch.End();
            Begin(TODrawUtils.EraseBlend, Main.GameViewMatrix.TransformationMatrix);
            Fill(new Vector2(center.X - CoinJudgment.PlatinumSafeHalfWidth, min.Y),
                new Vector2(CoinJudgment.PlatinumSafeHalfWidth * 2f, max.Y - min.Y));
            spriteBatch.End();
            return;
        }

        Begin(BlendState.AlphaBlend, Main.GameViewMatrix.TransformationMatrix);
        switch (coinType)
        {
            case 0:
                Circle(CoinJudgment.CopperRadius);
                break;
            case 1:
                Fill(new Vector2(center.X - CoinJudgment.SilverHalfWidth, min.Y),
                    new Vector2(CoinJudgment.SilverHalfWidth * 2f, max.Y - min.Y));
                Fill(new Vector2(min.X, center.Y - CoinJudgment.SilverHalfWidth),
                    new Vector2(max.X - min.X, CoinJudgment.SilverHalfWidth * 2f));
                break;
            case 2:
                Circle(CoinJudgment.GoldOuterRadius);
                break;
        }
        spriteBatch.End();

        if (coinType == 2)
        {
            Begin(TODrawUtils.EraseBlend, Main.GameViewMatrix.TransformationMatrix);
            Circle(CoinJudgment.GoldInnerRadius);
            spriteBatch.End();
        }

        void Begin(BlendState blend, Matrix transform) => spriteBatch.Begin(SpriteSortMode.Deferred, blend,
            SamplerState.LinearClamp, DepthStencilState.None, Main.Rasterizer, null, transform);

        void Circle(float radius) => spriteBatch.DrawFromCenter(circle, center, null, Color.White,
            0f, radius * 2f / circle.Width);

        void Fill(Vector2 position, Vector2 size) => spriteBatch.Draw(filler, position, null, Color.White,
            0f, Vector2.Zero, size / new Vector2(filler.Width, filler.Height), SpriteEffects.None, 0f);
    }
}
