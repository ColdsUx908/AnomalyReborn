using Anomalies.Visuals.Overlays;
using Terraria.Graphics.Effects;
using Transoceanic.DataStructures.Rendering;

namespace Anomalies.Bosses.Skeletron.LegendaryNight;

public sealed class CoinJudgmentOverlay : CustomOverlay
{
    public override void Draw(SpriteBatch spriteBatch, RenderLayers layer, bool beginSpriteBatch)
    {
        if (Main.gameMenu || Main.dedServ || layer != RenderLayers.All)
            return;

        // 每个判定单独合成，避免一种币的安全区擦掉另一种币的危险区域。
        foreach (Projectile projectile in Projectile.ActiveProjectiles)
        {
            if (projectile.ModProjectile is not CoinJudgment judgment)
                continue;
            float boundaryOpacity = judgment.BoundaryOpacity;
            float warningOpacity = judgment.WarningOverlayOpacity;
            float impactOpacity = judgment.OverlayOpacity;
            if (warningOpacity <= 0f && impactOpacity <= 0f)
            {
                if (boundaryOpacity > 0f)
                    DrawBoundary(spriteBatch, projectile, boundaryOpacity);
                continue;
            }

            using RenderTargetLease lease = RenderTargetPool.Shared.Rent(Main.instance.GraphicsDevice,
                Main.screenWidth, Main.screenHeight, RenderTargetDescriptor.Default);
            using (lease.Scope(clearColor: Color.Transparent))
                DrawMask(spriteBatch, projectile);

            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, Main.Rasterizer, null, Matrix.Identity);
            // 两个阶段共用同一张形状遮罩，确保红色预警与白色命中范围完全重合。
            if (warningOpacity > 0f)
                spriteBatch.Draw(lease.Target, Vector2.Zero, new Color(255, 120, 120) * warningOpacity);
            if (impactOpacity > 0f)
                spriteBatch.Draw(lease.Target, Vector2.Zero, Color.White * impactOpacity);
            spriteBatch.End();
            // 边界画在遮罩上方，保持币色，并与白色遮罩同步淡出。
            if (boundaryOpacity > 0f)
                DrawBoundary(spriteBatch, projectile, boundaryOpacity);
        }
    }

    private static void DrawBoundary(SpriteBatch spriteBatch, Projectile projectile, float opacity)
    {
        int coinType = (int)projectile.ai[0];
        Vector2 center = projectile.Center;
        Color color = CoinRoulette.GetCoinColor(coinType);
        switch (coinType)
        {
            case 0:
                AnomalyDrawUtils.DrawGlowingCircle(spriteBatch, center, CoinJudgment.CopperRadius, color, opacity);
                return;
            case 2:
                AnomalyDrawUtils.DrawGlowingCircle(spriteBatch, center, CoinJudgment.GoldInnerRadius, color, opacity);
                AnomalyDrawUtils.DrawGlowingCircle(spriteBatch, center, CoinJudgment.GoldOuterRadius, color, opacity);
                return;
        }

        // 只在屏幕范围内截取无限边界，端点外扩以免光晕在视口边缘出现圆头。
        FloatRectangle bounds = AnomalyDrawUtils.GetVisibleWorldBounds(84f);
        Span<(Vector2 Start, Vector2 End)> lines = stackalloc (Vector2, Vector2)[8];
        int count = 0;
        for (int side = -1; side <= 1; side += 2)
        {
            if (coinType == 1)
            {
                float halfWidth = CoinJudgment.SilverHalfWidth;
                float x = center.X + side * halfWidth;
                float y = center.Y + side * halfWidth;
                // 十字重叠区内部没有边界，分别绘制四条伸向视口外的臂。
                lines[count++] = (new Vector2(x, bounds.Top),
                    new Vector2(x, Math.Clamp(center.Y - halfWidth, bounds.Top, bounds.Bottom)));
                lines[count++] = (new Vector2(x, Math.Clamp(center.Y + halfWidth, bounds.Top, bounds.Bottom)),
                    new Vector2(x, bounds.Bottom));
                lines[count++] = (new Vector2(bounds.Left, y),
                    new Vector2(Math.Clamp(center.X - halfWidth, bounds.Left, bounds.Right), y));
                lines[count++] = (new Vector2(Math.Clamp(center.X + halfWidth, bounds.Left, bounds.Right), y),
                    new Vector2(bounds.Right, y));
            }
            else if (coinType == 3)
            {
                float x = center.X + side * CoinJudgment.PlatinumSafeHalfWidth;
                lines[count++] = (new Vector2(x, bounds.Top), new Vector2(x, bounds.Bottom));
            }
        }
        AnomalyDrawUtils.DrawGlowingLines(spriteBatch, lines[..count], color, opacity);
    }

    private static void DrawMask(SpriteBatch spriteBatch, Projectile projectile)
    {
        int coinType = (int)projectile.ai[0];
        Vector2 center = projectile.Center - Main.screenPosition;
        Texture2D circle = AnomalyTextures.Extra.BasicCircleLarge;
        Texture2D filler = TOTextures.FillerTexture;

        // 将屏幕四角逆变换，条带在缩放及相机变换后仍覆盖整个视口。
        FloatRectangle bounds = AnomalyDrawUtils.GetVisibleWorldBounds(1f);
        Vector2 min = bounds.TopLeft - Main.screenPosition;
        Vector2 max = bounds.BottomRight - Main.screenPosition;

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
