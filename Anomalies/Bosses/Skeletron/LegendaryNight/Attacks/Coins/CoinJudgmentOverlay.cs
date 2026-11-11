using Anomalies.Visuals.Overlays;
using Terraria.Graphics.Effects;

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
            if (impactOpacity > 0f && LegendaryNightOverlay.UseImpactFlame)
                continue;
            if (warningOpacity <= 0f && impactOpacity <= 0f)
            {
                if (boundaryOpacity > 0f)
                    DrawBoundary(spriteBatch, judgment, boundaryOpacity);
                continue;
            }

            using RenderTargetLease lease = RenderTargetPool.Shared.Rent(Main.instance.GraphicsDevice,
                Main.screenWidth, Main.screenHeight, RenderTargetDescriptor.Default);
            using (lease.Scope(clearColor: Color.Transparent))
                DrawMask(spriteBatch, judgment);

            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, Main.Rasterizer, null, Matrix.Identity);
            // 两个阶段共用同一张形状遮罩，确保红色预警与命中范围完全重合。
            if (warningOpacity > 0f)
                spriteBatch.Draw(lease.Target, Vector2.Zero, new Color(255, 120, 120) * warningOpacity);
            if (impactOpacity > 0f)
                spriteBatch.Draw(lease.Target, Vector2.Zero, new Color(234, 154, 54) * (impactOpacity * 0.45f));
            spriteBatch.End();
            // 边界画在遮罩上方，保持币色，并与遮罩同步淡出。
            if (boundaryOpacity > 0f)
                DrawBoundary(spriteBatch, judgment, boundaryOpacity);
        }
    }
    private static void DrawBoundary(SpriteBatch spriteBatch, CoinJudgment judgment, float opacity)
    {
        Projectile projectile = judgment.Projectile;
        int coinType = (int)projectile.ai[0];
        Vector2 center = projectile.Center;
        Color color = CoinRoulette.GetCoinColor(coinType);
        switch (coinType)
        {
            case 0:
                AnomalyDrawUtils.DrawGlowingCircle(spriteBatch, center, judgment.EffectiveCopperRadius, color, opacity);
                return;
            case 2:
                AnomalyDrawUtils.DrawGlowingCircle(spriteBatch, center, judgment.EffectiveGoldInnerRadius, color, opacity);
                AnomalyDrawUtils.DrawGlowingCircle(spriteBatch, center, CoinJudgment.GoldOuterRadius, color, opacity);
                return;
        }

        // 只在屏幕范围内截取无限边界，端点外扩以免光晕在视口边缘出现圆头。
        FloatRectangle bounds = AnomalyDrawUtils.GetVisibleWorldBounds(84f);
        Span<(Vector2 Start, Vector2 End)> lines = stackalloc (Vector2, Vector2)[8];
        float halfWidth = judgment.CrossHalfWidth;
        float reach = GetCrossReach(bounds, center, halfWidth);
        float rotation = CoinJudgment.CrossRotation(coinType);
        // 十字相交的部分没有边界。沿每条臂绘制两侧，从交点延伸到视口外。
        for (int arm = 0; arm < 4; arm++)
        {
            Vector2 direction = (rotation + arm * MathHelper.PiOver2).ToRotationVector2();
            Vector2 side = new Vector2(-direction.Y, direction.X) * halfWidth;
            Vector2 start = center + direction * halfWidth;
            Vector2 end = center + direction * reach;
            lines[2 * arm] = (start + side, end + side);
            lines[2 * arm + 1] = (start - side, end - side);
        }
        AnomalyDrawUtils.DrawGlowingLines(spriteBatch, lines, color, opacity);
    }
    private static void DrawMask(SpriteBatch spriteBatch, CoinJudgment judgment)
    {
        Projectile projectile = judgment.Projectile;
        int coinType = (int)projectile.ai[0];
        Vector2 center = projectile.Center - Main.screenPosition;
        Texture2D circle = AnomalyTextures.Extra.BasicCircleLarge;
        Texture2D filler = TOTextures.FillerTexture;

        // 将屏幕四角逆变换，条带在缩放及相机变换后仍覆盖整个视口。
        FloatRectangle bounds = AnomalyDrawUtils.GetVisibleWorldBounds(1f);
        Begin(BlendState.AlphaBlend, Main.GameViewMatrix.TransformationMatrix);
        switch (coinType)
        {
            case 0:
                Circle(judgment.EffectiveCopperRadius);
                break;
            case 1:
            case 3:
                float reach = GetCrossReach(bounds, projectile.Center, judgment.CrossHalfWidth);
                float rotation = CoinJudgment.CrossRotation(coinType);
                Stripe(rotation, reach);
                Stripe(rotation + MathHelper.PiOver2, reach);
                break;
            case 2:
                Circle(CoinJudgment.GoldOuterRadius);
                break;
        }
        spriteBatch.End();

        if (coinType == 2)
        {
            Begin(TODrawUtils.EraseBlend, Main.GameViewMatrix.TransformationMatrix);
            Circle(judgment.EffectiveGoldInnerRadius);
            spriteBatch.End();
        }

        void Begin(BlendState blend, Matrix transform) => spriteBatch.Begin(SpriteSortMode.Deferred, blend,
            SamplerState.LinearClamp, DepthStencilState.None, Main.Rasterizer, null, transform);

        void Circle(float radius) => spriteBatch.DrawFromCenter(circle, center, null, Color.White,
            0f, radius * 2f / circle.Width);

        void Stripe(float rotation, float reach) => spriteBatch.DrawFromCenter_VectorScale(filler, center, null, Color.White,
            rotation, new Vector2(reach, judgment.CrossHalfWidth) * 2f / filler.Size());
    }

    private static float GetCrossReach(FloatRectangle bounds, Vector2 center, float halfWidth) =>
        Vector2.Distance(center, (bounds.TopLeft + bounds.BottomRight) * 0.5f)
        + Vector2.Distance(bounds.TopLeft, bounds.BottomRight) * 0.5f + halfWidth;
}