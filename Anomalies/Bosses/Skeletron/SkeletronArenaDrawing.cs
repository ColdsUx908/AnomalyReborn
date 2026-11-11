using Anomalies.Assets.Effects;

namespace Anomalies.Bosses.Skeletron;

internal static class SkeletronArenaDrawing
{
    private const int VertexCount = Hexadecagon.VertexCount;
    private const string TexturePath = "Anomalies/Bosses/Skeletron/";

    /// <summary>遮罩。</summary>
    [LoadTexture(TexturePath + "Hexdecagon")]
    private static Asset<Texture2D> _Mask;
    /// <summary>骨节绘制贴图。</summary>
    [LoadTexture(TexturePath + "BoneStick")]
    private static Asset<Texture2D> _BoneStick;
    /// <summary>关节绘制贴图。</summary>
    [LoadTexture(TexturePath + "BoneBall")]
    private static Asset<Texture2D> _BoneBall;

    internal static void DrawBoundary(SpriteBatch spriteBatch, Hexadecagon polygon, float intensity)
    {
        Span<Vector2> vertices = stackalloc Vector2[VertexCount];
        for (int i = 0; i < VertexCount; i++)
            vertices[i] = polygon.GetVertex(i) - Main.screenPosition;

        Texture2D stick = _BoneStick.Value;
        Texture2D ball = _BoneBall.Value;
        Color color = Color.White * intensity;

        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
            DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

        // 先画所有骨棒，仅沿材质自身的横轴拉伸，再用原尺寸骨球覆盖接缝。
        for (int i = 0; i < VertexCount; i++)
        {
            Vector2 start = vertices[i];
            Vector2 end = vertices[(i + 1) % VertexCount];
            Vector2 edge = end - start;
            spriteBatch.DrawFromCenter_VectorScale(stick, (start + end) * 0.5f, null, color, edge.ToRotation(),
                new Vector2(edge.Length() / stick.Width, 1f));
        }

        for (int i = 0; i < VertexCount; i++)
            spriteBatch.DrawFromCenter(ball, vertices[i], null, color);

        spriteBatch.End();
    }

    internal static void DrawMist(SpriteBatch spriteBatch, float intensity, Hexadecagon? polygon, Vector2 center = default, float radius = 0f)
    {
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

            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearWrap, DepthStencilState.None, Main.Rasterizer, null, Matrix.Identity);

            bool useShaders = AnomalyClientConfig.Instance.EnableShaders
                && AnomalyEffects.Overlays.LegendaryNightArenaMist is not null
                && AnomalyTextures.Noise._Wave is { IsLoaded: true };
            if (useShaders)
                AnomalyEffects.Overlays.LegendaryNightArenaMist.Data
                    .UseImage1(AnomalyTextures.Noise._Wave)
                    .SetCustomParameter("uScreenResolution", Main.ScreenSize.ToVector2() * Math.Max(Main.UIScale, 1f) / 2f)
                    .SetCustomParameter("uScreenPosition", Main.screenPosition)
                    .SetCustomParameter("uInverseViewMatrix", Matrix.Invert(Main.GameViewMatrix.TransformationMatrix))
                    .Apply();

            Rectangle screen = new(0, 0, Main.screenWidth, Main.screenHeight);
            if (useShaders)
                spriteBatch.Draw(TOTextures.FillerTexture, screen, Color.White);
            else
            {
                // 无着色器时同样只叠加流动雾纹，不铺实色底；Alpha为零使噪声暗部保持透明。
                // Keep the original 1536px texture period and average additive brightness.
                const float WaveSourceScale = 512f / 1536f;
                const float WaveFallbackBrightness = 0.293363195f;
                Rectangle noiseArea = new((int)((Main.screenPosition.X * 0.4f + Main.timeForVisualEffects * 0.4f) * WaveSourceScale),
                    (int)(Main.screenPosition.Y * 0.4f * WaveSourceScale),
                    Math.Max(1, (int)(Main.screenWidth * 0.5f * WaveSourceScale)),
                    Math.Max(1, (int)(Main.screenHeight * 0.5f * WaveSourceScale)));
                spriteBatch.Draw(AnomalyTextures.Noise.Wave, screen, noiseArea,
                    new Color(210, 200, 180, 0) * (0.8f * WaveFallbackBrightness));
            }

            spriteBatch.End();

            // 按竞技场的实际外沿擦除内部雾纹。

            spriteBatch.Begin(SpriteSortMode.Immediate, TODrawUtils.EraseBlend, SamplerState.LinearClamp, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            if (!polygon.HasValue)
            {
                Texture2D circle = AnomalyTextures.Extra.BasicCircleLarge;
                spriteBatch.DrawFromCenter(circle, center - Main.screenPosition, null, Color.White,
                    scale: radius * 2f / circle.Width);
            }
            else
            {
                Texture2D mask = _Mask.Value;
                Hexadecagon hexadecagon = polygon.Value;
                spriteBatch.DrawFromCenter_VectorScale(mask, hexadecagon.Center - Main.screenPosition, null, Color.White, hexadecagon.Rotation,
                    new Vector2(hexadecagon.CircumRadius * 2f / mask.Width, hexadecagon.CircumRadius * 2f / mask.Height));
            }
            spriteBatch.End();
        }

        spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, Main.Rasterizer, null, Matrix.Identity);
        spriteBatch.Draw(lease.Target, Vector2.Zero, null, Color.White * intensity, 0, Vector2.Zero, 1, 0, 0);
        spriteBatch.End();
    }
}
