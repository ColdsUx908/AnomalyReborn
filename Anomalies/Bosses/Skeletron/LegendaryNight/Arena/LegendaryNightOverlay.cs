using Anomalies.Assets.Effects;
using Anomalies.Visuals.Overlays;
using Terraria.Graphics.Effects;
using Transoceanic.DataStructures.Rendering;

namespace Anomalies.Bosses.Skeletron.LegendaryNight;

public sealed class LegendaryNightOverlay : CustomOverlay
{
    private const int VertexCount = Hexadecagon.VertexCount;
    private const string TexturePath = "Anomalies/Bosses/Skeletron/LegendaryNight/Arena/";

    [LoadTexture(TexturePath + "Hexdecagon")]
    private static Asset<Texture2D> _Mask;
    [LoadTexture(TexturePath + "BoneStick")]
    private static Asset<Texture2D> _BoneStick;
    [LoadTexture(TexturePath + "BoneBall")]
    private static Asset<Texture2D> _BoneBall;
    [LoadTexture(TexturePath + "Pentagon")]
    private static Asset<Texture2D> _Pentagon;

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
        if (Main.gameMenu || layer != RenderLayers.All)
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

            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, Main.Rasterizer, null, Matrix.Identity);

            bool useShaders = AnomalyClientConfig.Instance.EnableShaders && AnomalyEffects.Overlays.LegendaryNightOverlay is not null;
            if (useShaders)
                AnomalyEffects.Overlays.LegendaryNightOverlay.Data
                    .UseImage1(AnomalyTextures.Noise._Wave)
                    .SetCustomParameter("uScreenResolution", Main.ScreenSize.ToVector2() * Math.Max(Main.UIScale, 1f) / 2f)
                    .SetCustomParameter("uScreenPosition", Main.screenPosition)
                    .SetCustomParameter("uInverseViewMatrix", Matrix.Invert(Main.GameViewMatrix.TransformationMatrix))
                    .Apply();

            spriteBatch.Draw(TOTextures.FillerTexture, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight),
                useShaders ? Color.White : new Color(35, 32, 28, 180));

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
            DrawMagicCircle(spriteBatch);
        else
            DrawBoundary(spriteBatch);
    }

    private void DrawBoundary(SpriteBatch spriteBatch)
    {
        Hexadecagon polygon = Arena.VisualPolygon;
        Span<Vector2> vertices = stackalloc Vector2[VertexCount];
        for (int i = 0; i < VertexCount; i++)
            vertices[i] = polygon.GetVertex(i) - Main.screenPosition;

        Texture2D stick = _BoneStick.Value;
        Texture2D ball = _BoneBall.Value;
        Color color = Color.White * Arena.Intensity;

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

    private void DrawMagicCircle(SpriteBatch spriteBatch)
    {
        DrawMechanic2HalfField(spriteBatch);
        float intensity = Phase2Arena.MaterializationIntensity;
        float materializedOpacity = MathHelper.Lerp(Phase2Arena.MaterializationWarning * 0.22f, 0.65f, intensity);
        if (materializedOpacity > 0f)
        {
            using RenderTargetLease lease = RenderTargetPool.Shared.Rent(
                Main.instance.GraphicsDevice, Main.screenWidth, Main.screenHeight, RenderTargetDescriptor.Default);
            Pentagon pentagon = Phase2Arena.CentralPentagon;
            Annulus ring = Phase2Arena.MaterializedRing;
            Texture2D circle = AnomalyTextures.Extra.BasicCircleLarge;
            using (lease.Scope(clearColor: Color.Transparent))
            {
                // 在独立遮罩上叠加外圆、擦除内圆，避免擦掉半场预警和竞技场背景。
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                    DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
                spriteBatch.DrawFromCenter(circle, ring.Center - Main.screenPosition, null, Color.White,
                    scale: ring.OuterRadius * 2f / circle.Width);
                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Deferred, TODrawUtils.EraseBlend, SamplerState.LinearClamp,
                    DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
                spriteBatch.DrawFromCenter(circle, ring.Center - Main.screenPosition, null, Color.White,
                    scale: ring.InnerRadius * 2f / circle.Width);
                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                    DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
                // 512 方形透明图，几何中心 (256,256)，外接圆半径 250，顶点朝右。
                spriteBatch.DrawFromCenter(_Pentagon.Value, pentagon.Center - Main.screenPosition, null,
                    Color.White, pentagon.Rotation, pentagon.CircumRadius / 250f);
                spriteBatch.End();
            }
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, Main.Rasterizer, null, Matrix.Identity);
            spriteBatch.Draw(lease.Target, Vector2.Zero,
                Color.Lerp(LegendaryNightHandler.StarColor, Color.White, intensity) * materializedOpacity);
            spriteBatch.End();
        }
        for (int i = 0; i < 3; i++)
        {
            (Vector2 center, float radius, float rotation) = Phase2Arena.GetMagicCircle(i);
            DrawMagicCirclePart(spriteBatch, center, radius, rotation, true);
        }
        DrawMagicCirclePart(spriteBatch, Phase2Arena.Projectile.Center, Phase2Arena.Radius, 0f, false);
        DrawMechanic3Connections(spriteBatch);
    }

    private void DrawMechanic3Connections(SpriteBatch spriteBatch)
    {
        SkeletronLegendHead head = Phase2Arena.MasterBehavior;
        if (head is null || head.CurrentPhase != SkeletronLegendHead.Phase.Phase2_3)
            return;

        // 与法阵共用发光线绘制及其 Begin/End 管理；每组只画五条完整边，避免采样点接缝。
        Span<(Vector2 Start, Vector2 End)> edges = stackalloc (Vector2, Vector2)[5];
        for (int circleIndex = 1; circleIndex <= SkeletronLegendHead.Mechanic3Data.CircleCount; circleIndex++)
        {
            int age = head.Phase2MusicElapsedTime - SkeletronLegendHead.Mechanic3Data.GetStartMusicTime(circleIndex);
            if (age < 0 || head.CurrentLocalPhase < circleIndex)
                continue;
            int samplesPerEdge = SkeletronLegendHead.Mechanic3Data.StarCount / edges.Length;
            for (int i = 0; i < edges.Length; i++)
                edges[i] = (
                    SkeletronLegendHead.Mechanic3Data.GetStarPosition(Phase2Arena, i * samplesPerEdge, age, circleIndex),
                    SkeletronLegendHead.Mechanic3Data.GetStarPosition(Phase2Arena, (i + 1) % edges.Length * samplesPerEdge, age, circleIndex));
            AnomalyDrawUtils.DrawGlowingLines(spriteBatch, edges,
                SkeletronLegendHead.Mechanic3Data.ConnectionColor, Phase2Arena.Intensity * 0.65f,
                SkeletronLegendHead.Mechanic3Data.ConnectionWidth, SkeletronLegendHead.Mechanic3Data.ConnectionGlowWidth);
        }
    }

    private void DrawMagicCirclePart(SpriteBatch spriteBatch, Vector2 worldCenter, float radius, float rotation, bool drawStar)
    {
        float intensity = Phase2Arena.MaterializationIntensity;
        float warning = Phase2Arena.MaterializationWarning;
        Color color = Color.Lerp(Color.Lerp(LegendaryNightHandler.StarColor, Color.OrangeRed, warning), Color.White, intensity);
        float opacity = Phase2Arena.Intensity * MathHelper.Lerp(0.65f + warning * 0.3f, 1f, intensity);
        float width = MathHelper.Lerp(3f + warning * 5f, LegendaryNightPhase2Arena.LaserWidth, intensity);
        float glowWidth = MathHelper.Lerp(14f, 22f, intensity);
        AnomalyDrawUtils.DrawGlowingCircle(spriteBatch, worldCenter, radius, color, opacity, width, glowWidth);
        if (!drawStar)
            return;

        Span<(Vector2 Start, Vector2 End)> edges = stackalloc (Vector2, Vector2)[5];
        for (int i = 0; i < edges.Length; i++)
            edges[i] = (worldCenter + (rotation + MathHelper.TwoPi * i / 5f).ToRotationVector2() * radius,
                worldCenter + (rotation + MathHelper.TwoPi * (i + 2) / 5f).ToRotationVector2() * radius);
        AnomalyDrawUtils.DrawGlowingLines(spriteBatch, edges, color, opacity, width, glowWidth);
    }

    private void DrawMechanic2HalfField(SpriteBatch spriteBatch)
    {
        SkeletronLegendHead head = Phase2Arena.MasterBehavior;
        if (head is not { Mechanic2Active: true } || head.Mechanic2HandPattern == 0)
            return;
        int age = head.Mechanic2Age;
        Texture2D circle = AnomalyTextures.Extra.BasicCircleLarge;
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
            DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
        // 第二次预警可能与第一次的冲击余光重叠，分别绘制，避免丢失预警最初的帧数。
        for (int strike = 0; strike < SkeletronLegendHead.Mechanic2Data.StrikeCount; strike++)
        {
            int relativeAge = age - SkeletronLegendHead.Mechanic2Data.GetStrikeTime(strike);
            float opacity = SkeletronLegendHead.Mechanic2Data.HalfFieldOpacity(relativeAge);
            if (opacity <= 0f)
                continue;
            int side = head.Mechanic2StrikeSide(strike);
            Rectangle frame = new(side < 0 ? 0 : circle.Width / 2, 0, circle.Width / 2, circle.Height);
            Vector2 origin = new(side < 0 ? circle.Width / 2f : 0f, circle.Height / 2f);
            Color color = relativeAge < 0 ? Color.OrangeRed : Color.White;
            spriteBatch.Draw(circle, Phase2Arena.Projectile.Center - Main.screenPosition, frame, color * opacity,
                0f, origin, Phase2Arena.Radius * 2f / circle.Width, SpriteEffects.None, 0f);
        }
        spriteBatch.End();
    }
}
