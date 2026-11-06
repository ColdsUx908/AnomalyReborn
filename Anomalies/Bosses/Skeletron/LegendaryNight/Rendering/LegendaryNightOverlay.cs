using Anomalies.Assets.Effects;
using Anomalies.Visuals.Overlays;
using Terraria.Graphics.Effects;

namespace Anomalies.Bosses.Skeletron.LegendaryNight;

public sealed partial class LegendaryNightOverlay : CustomOverlay
{
    /// <summary>顶点数量。</summary>
    private const int VertexCount = Hexadecagon.VertexCount;
    /// <summary>主贴图资源路径。</summary>
    private const string TexturePath = "Anomalies/Bosses/Skeletron/LegendaryNight/Arena/";

    /// <summary>遮罩。</summary>
    [LoadTexture(TexturePath + "Hexdecagon")]
    private static Asset<Texture2D> _Mask;
    /// <summary>骨节绘制贴图。</summary>
    [LoadTexture(TexturePath + "BoneStick")]
    private static Asset<Texture2D> _BoneStick;
    /// <summary>关节绘制贴图。</summary>
    [LoadTexture(TexturePath + "BoneBall")]
    private static Asset<Texture2D> _BoneBall;
    /// <summary>实体化星体。</summary>
    [LoadTexture(TexturePath + "MaterializedStar")]
    private static Asset<Texture2D> _MaterializedStar;

    public LegendaryNightArena Arena;
    public LegendaryNightPhase2Arena Phase2Arena;

    public override void Update()
    {
        Arena = null;
        Phase2Arena = null;
        foreach (Projectile projectile in Projectile.ActiveProjectiles)
        {
            if (projectile.ModProjectile is LegendaryNightArena arena && (Arena is null || arena.Intensity > Arena.Intensity))
                Arena = arena;
            if (projectile.ModProjectile is LegendaryNightPhase2Arena phase2Arena
                && (Phase2Arena is null || phase2Arena.Intensity > Phase2Arena.Intensity))
                Phase2Arena = phase2Arena;
        }
    }

    public override void Draw(SpriteBatch spriteBatch, RenderLayers layer, bool beginSpriteBatch)
    {
        if (Main.gameMenu || Main.dedServ || layer != RenderLayers.All)
            return;

        bool phase2 = Phase2Arena is not null && Phase2Arena.Projectile.active;
        float intensity = phase2 ? Phase2Arena.Intensity : Arena?.Intensity ?? 0f;
        if (intensity <= 0f || !phase2 && (Arena is null || !Arena.Projectile.active))
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

            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearWrap, DepthStencilState.None, Main.Rasterizer, null, Matrix.Identity);

            bool useShaders = AnomalyClientConfig.Instance.EnableShaders && AnomalyEffects.Overlays.LegendaryNightArenaMist is not null;
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

            // 一阶段沿十六边形擦除；二阶段实心圆的边缘与竞技场外沿重合。

            spriteBatch.Begin(SpriteSortMode.Immediate, TODrawUtils.EraseBlend, SamplerState.LinearClamp, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            if (phase2)
            {
                Texture2D circle = AnomalyTextures.Extra.BasicCircleLarge;
                spriteBatch.DrawFromCenter(circle, Phase2Arena.Projectile.Center - Main.screenPosition, null, Color.White,
                    scale: Phase2Arena.OuterRadius * 2f / circle.Width);
            }
            else
            {
                Texture2D mask = _Mask.Value;
                Hexadecagon polygon = Arena.VisualPolygon;
                spriteBatch.DrawFromCenter_VectorScale(mask, polygon.Center - Main.screenPosition, null, Color.White, polygon.Rotation,
                    new Vector2(polygon.CircumRadius * 2f / mask.Width, polygon.CircumRadius * 2f / mask.Height));
            }
            spriteBatch.End();
        }

        spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, Main.Rasterizer, null, Matrix.Identity);
        spriteBatch.Draw(lease.Target, Vector2.Zero, null, Color.White * intensity, 0, Vector2.Zero, 1, 0, 0);
        spriteBatch.End();

        if (phase2)
        {
            // Composite the gold edge above the original outside mist, below warnings and boundary lines.
            DrawArenaEdge(spriteBatch);
            DrawMagicCircle(spriteBatch);
            LegendaryNightAttackDrawing.Draw(spriteBatch, Phase2Arena.MasterBehavior);
        }
        else
            DrawBoundary(spriteBatch);
    }

}