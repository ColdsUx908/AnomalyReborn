using Anomalies.Assets.Effects;

namespace Anomalies.Bosses.Skeletron;

internal static class SkeletronArenaDrawing
{
    private const int VertexCount = Hexadecagon.VertexCount;
    private const string TexturePath = "Anomalies/Bosses/Skeletron/";

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
        if (polygon is Hexadecagon boundary)
        {
            DrawPolygonMist(spriteBatch, boundary, intensity);
            DrawExterior(spriteBatch, boundary, intensity);
            return;
        }

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
                    .SetCustomParameter("uScreenPosition", Main.screenPosition)
                    .SetCustomParameter("uInverseViewMatrix", Matrix.Invert(Main.GameViewMatrix.TransformationMatrix))
                    .SetCustomParameter("uPolygonCenter", Vector2.Zero)
                    .SetCustomParameter("uPolygonRadius", 0f)
                    .SetCustomParameter("uPolygonRotation", 0f)
                    .Apply();

            Rectangle screen = new(0, 0, Main.screenWidth, Main.screenHeight);
            if (useShaders)
                spriteBatch.Draw(TOTextures.FillerTexture, screen, Color.White);
            else
            {
                // 无着色器时同样只叠加流动雾纹，不铺实色底；Alpha为零使噪声暗部保持透明。
                // Preserve the original 3072-world-pixel period and brightness at unit camera zoom.
                // Draw in world space so camera movement and zoom cannot slide or resize the noise.
                const float WaveSourceScale = 512f / 1536f;
                const float WorldSourceScale = WaveSourceScale * 0.5f;
                const float WaveFallbackBrightness = 0.293363195f;
                FloatRectangle bounds = AnomalyDrawUtils.GetVisibleWorldBounds(1f);
                int left = (int)MathF.Floor(bounds.Left * WorldSourceScale);
                int top = (int)MathF.Floor(bounds.Top * WorldSourceScale);
                int right = (int)MathF.Ceiling(bounds.Right * WorldSourceScale);
                int bottom = (int)MathF.Ceiling(bounds.Bottom * WorldSourceScale);
                Rectangle noiseArea = new(left + (int)(Main.timeForVisualEffects * 0.4f * WaveSourceScale),
                    top, Math.Max(1, right - left), Math.Max(1, bottom - top));
                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearWrap,
                    DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
                spriteBatch.Draw(AnomalyTextures.Noise.Wave,
                    new Vector2(left, top) / WorldSourceScale - Main.screenPosition, noiseArea,
                    new Color(210, 200, 180, 0) * (0.8f * WaveFallbackBrightness),
                    0f, Vector2.Zero, 1f / WorldSourceScale, SpriteEffects.None, 0f);
            }

            spriteBatch.End();

            // 按竞技场的实际外沿擦除内部雾纹。

            spriteBatch.Begin(SpriteSortMode.Immediate, TODrawUtils.EraseBlend, SamplerState.LinearClamp, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            Texture2D circle = AnomalyTextures.Extra.BasicCircleLarge;
            spriteBatch.DrawFromCenter(circle, center - Main.screenPosition, null, Color.White,
                scale: radius * 2f / circle.Width);
            spriteBatch.End();
        }

        spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, Main.Rasterizer, null, Matrix.Identity);
        spriteBatch.Draw(lease.Target, Vector2.Zero, null, Color.White * intensity, 0, Vector2.Zero, 1, 0, 0);
        spriteBatch.End();
    }

    private static void DrawPolygonMist(SpriteBatch spriteBatch, Hexadecagon polygon, float intensity)
    {
        if (Main.dedServ || !AnomalyClientConfig.Instance.EnableShaders || polygon.CircumRadius <= 0f
            || AnomalyEffects.Overlays.LegendaryNightArenaMist is not AnomalyEffects.MiscShaderContainer shader
            || AnomalyTextures.Noise._Wave is not Asset<Texture2D> { IsLoaded: true } noise)
            return;

        // Restore the original Wave mist below the bounded bone overlay, with exact interior clipping.
        spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearWrap,
            DepthStencilState.None, Main.Rasterizer, null, Matrix.Identity);
        shader.Data
            .UseImage1(noise)
            .SetCustomParameter("uScreenPosition", Main.screenPosition)
            .SetCustomParameter("uInverseViewMatrix", Matrix.Invert(Main.GameViewMatrix.TransformationMatrix))
            .SetCustomParameter("uPolygonCenter", polygon.Center)
            .SetCustomParameter("uPolygonRadius", polygon.CircumRadius)
            .SetCustomParameter("uPolygonRotation", polygon.Rotation)
            .Apply();
        Main.instance.GraphicsDevice.SamplerStates[1] = SamplerState.LinearWrap;
        spriteBatch.Draw(TOTextures.FillerTexture, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), Color.White * intensity);
        spriteBatch.End();
    }

    private static void DrawExterior(SpriteBatch spriteBatch, Hexadecagon polygon, float intensity)
    {
        // World-pixel geometry follows the same polygon as the bone boundary at every zoom.
        // Without shaders keep the bone boundary; never substitute an approximate interior mask.
        if (Main.dedServ || !AnomalyClientConfig.Instance.EnableShaders || polygon.CircumRadius <= 0f
            || AnomalyEffects.Overlays.SkeletronArenaExterior is not AnomalyEffects.MiscShaderContainer shader)
            return;

        spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp,
            DepthStencilState.None, Main.Rasterizer, null, Matrix.Identity);
        shader.Data
            .SetCustomParameter("uScreenPosition", Main.screenPosition)
            .SetCustomParameter("uInverseViewMatrix", Matrix.Invert(Main.GameViewMatrix.TransformationMatrix))
            .SetCustomParameter("uCenter", polygon.Center)
            .SetCustomParameter("uRadius", polygon.CircumRadius)
            .SetCustomParameter("uRotation", polygon.Rotation)
            .SetCustomParameter("uFlowTime", (float)(Main.timeForVisualEffects / 60.0))
            .UseOpacity(intensity)
            .Apply();
        spriteBatch.Draw(TOTextures.FillerTexture, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), Color.White);
        spriteBatch.End();
    }
}
