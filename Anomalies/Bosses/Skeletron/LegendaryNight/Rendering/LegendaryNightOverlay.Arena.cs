using Anomalies.Assets.Effects;

namespace Anomalies.Bosses.Skeletron.LegendaryNight;

public sealed partial class LegendaryNightOverlay
{
    private static readonly Color _MaterializationCoreColor = new(255, 214, 90);
    private static readonly Color _MaterializationGlowColor = new(255, 162, 24);

    private void DrawMagicCircle(SpriteBatch spriteBatch)
    {
        DrawMechanic6ContractionWarning(spriteBatch);
        foreach (Projectile projectile in Projectile.ActiveProjectiles)
            if (projectile.ModProjectile is LegendStarburst burst && burst.Head?.NPC == Phase2Arena.MasterBehavior?.NPC)
                burst.Draw(spriteBatch);
        if (!UseImpactFlame)
            DrawMechanic2HalfField(spriteBatch);
        for (int i = 0; i < 3; i++)
        {
            (Vector2 center, float radius, float rotation) = Phase2Arena.GetMagicCircle(i);
            DrawMagicCirclePart(spriteBatch, center, radius, rotation, true, i);
        }
        DrawMagicCirclePart(spriteBatch, Phase2Arena.Projectile.Center, Phase2Arena.Radius, 0f, false);
        DrawCircleStarbursts(spriteBatch);
        DrawRingStarbursts(spriteBatch);
        if (Phase2Arena.MasterBehavior is SkeletronLegendHead { Mechanic7Active: true } head)
            for (int i = 0; i < 8; i++)
            {
                (Vector2 center, float radius, float rotation) = Phase2Arena.GetMechanic7Circle(i);
                DrawMagicCirclePart(spriteBatch, center, radius, rotation, true,
                    opacityMultiplier: SkeletronLegendHead.Mechanic7Parameters.CircleOpacity(head.Phase2MusicElapsedTime));
            }
    }

    private void DrawCircleStarbursts(SpriteBatch spriteBatch)
    {
        if (!LegendStarburstCircleAssets.IsLoaded || Phase2Arena.MasterBehavior is not SkeletronLegendHead head)
            return;
        NPC owner = head.NPC;
        bool warning = TryGetCircleWarning(head, out LegendStarburstVisuals.Burst warningFrame);
        bool bloom = AnomalyClientConfig.Instance.EnableShaders && LightweightBloom.IsAvailable;
        BlendState blend = bloom || !Main.dayTime ? LightweightBloom.AdditiveBlend : BlendState.AlphaBlend;
        if (warning)
        {
            // 预告星保留自身光晕，不跟随后续爆区的放大倍率。
            BloomSettings warningSettings = BloomSettings.Default with
            {
                AdditiveBody = !Main.dayTime,
                BodyPasses = Main.dayTime ? 2 : 1
            };
            LightweightBloom.Draw(spriteBatch, bloom, warningSettings, () =>
            {
                LegendStarburstVisuals.View warningView = new(Main.screenPosition,
                    Main.GameViewMatrix.TransformationMatrix, new Vector2(Main.screenWidth, Main.screenHeight));
                LegendStarburstCircleVisuals.Draw(spriteBatch, LegendStarburstCircleAssets.Textures,
                    warningView, warningFrame, blend);
            });
        }
        float outerRadius = 0f;
        foreach (Projectile projectile in Projectile.ActiveProjectiles)
            if (projectile.ModProjectile is LegendStarburst burst && burst.Head?.NPC == owner
                && burst.TryGetCircleFrame(out _, out LegendStarburstVisuals.Burst frame))
                outerRadius = Math.Max(outerRadius, frame.OuterRadius);
        if (outerRadius <= 0f)
            return;
        // 同一场地的普通圆形星爆半径相同，共用一次捕获；法阵不进入该光晕层。
        BloomSettings settings = LegendStarburstCircleVisuals.GetBloomSettings(outerRadius,
            Main.GameViewMatrix.TransformationMatrix, Main.dayTime);
        LightweightBloom.Draw(spriteBatch, bloom, settings, () =>
        {
            foreach (Projectile projectile in Projectile.ActiveProjectiles)
                if (projectile.ModProjectile is LegendStarburst burst && burst.Head?.NPC == owner
                    && burst.TryGetCircleFrame(out LegendStarburstVisuals.View view, out LegendStarburstVisuals.Burst frame))
                    LegendStarburstCircleVisuals.Draw(spriteBatch, LegendStarburstCircleAssets.Textures, view, frame, blend);
        });
    }

    private void DrawRingStarbursts(SpriteBatch spriteBatch)
    {
        if (!LegendStarburstRingAssets.IsLoaded || Phase2Arena.MasterBehavior is not SkeletronLegendHead head)
            return;
        float outerRadius = 0f;
        foreach (Projectile projectile in Projectile.ActiveProjectiles)
            if (projectile.ModProjectile is LegendStarburst burst && burst.Head?.NPC == head.NPC
                && burst.TryGetRingFrame(out _, out LegendStarburstVisuals.Burst frame))
                outerRadius = Math.Max(outerRadius, frame.OuterRadius);
        if (outerRadius <= 0f)
            return;
        bool bloom = LightweightBloom.IsAvailable;
        BlendState blend = bloom || !Main.dayTime ? LightweightBloom.AdditiveBlend : BlendState.AlphaBlend;
        BloomSettings settings = LegendStarburstRingVisuals.GetBloomSettings(outerRadius,
            Main.GameViewMatrix.TransformationMatrix, Main.dayTime);
        LightweightBloom.Draw(spriteBatch, bloom, settings, () =>
        {
            foreach (Projectile projectile in Projectile.ActiveProjectiles)
                if (projectile.ModProjectile is LegendStarburst burst && burst.Head?.NPC == head.NPC
                    && burst.TryGetRingFrame(out LegendStarburstVisuals.View view, out LegendStarburstVisuals.Burst frame))
                    LegendStarburstRingVisuals.Draw(spriteBatch, AnomalyEffects.Overlays.LegendStarburstRingMark.Asset.Value,
                        LegendStarburstRingAssets.Textures, view, frame, blend);
        });
    }

    private bool TryGetCircleWarning(SkeletronLegendHead head, out LegendStarburstVisuals.Burst frame)
    {
        frame = default;
        if (!head.StarburstActive || head.CurrentBehavior == SkeletronLegendHead.Behavior.Despawn)
            return false;
        // 预告早于实体的 12 帧红圈窗口，由音乐时钟直接恢复；不提前生成伤害实体。
        for (int index = 0; index < LegendaryNightStarburstParameters.StarburstCount; index += 2)
        {
            int age = head.Phase2MusicElapsedTime - LegendaryNightStarburstTimeline.GetStarburstTime(index);
            if (!LegendStarburstCircleVisuals.IsWarningVisible(age))
                continue;
            // 与服务器起爆共用解析预测点，蓄势期间不随两阵中点漂移。
            Vector2 warningCenter = Phase2Arena.PredictStarburstPosition(index);
            frame = new(warningCenter, Phase2Arena.Radius * 1.5f, 0f,
                Phase2Arena.SmallCircleRadius, 0f, 0f, age, index);
            return true;
        }
        return false;
    }

    private void DrawMaterializedFill(SpriteBatch spriteBatch)
    {
        float intensity = Phase2Arena.MaterializationIntensity;
        float materializedOpacity = MathHelper.Lerp(Phase2Arena.MaterializationWarning * 0.22f, 0.24f, intensity);
        if (materializedOpacity > 0f)
        {
            using RenderTargetLease lease = RenderTargetPool.Shared.Rent(
                Main.instance.GraphicsDevice, Main.screenWidth, Main.screenHeight, RenderTargetDescriptor.Default);
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
                if (!Phase2Arena.ChargeMaterialization)
                    spriteBatch.DrawFromCenter(_MaterializedStar.Value, Phase2Arena.Projectile.Center - Main.screenPosition, null,
                        Color.White, Phase2Arena.MagicCircleRotation, Phase2Arena.MainCircleRadius / 250f);
                spriteBatch.End();
            }
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, Main.Rasterizer, null, Matrix.Identity);
            Color fillColor = Color.Lerp(LegendaryNightHandler.StarColor, _MaterializationGlowColor, intensity);
            // 薄底色勾勒实心区域，额外的发光分量只增加亮度。
            float emission = 0.24f * intensity;
            Color tint = (fillColor * (materializedOpacity + emission)) with { A = (byte)(255f * materializedOpacity) };
            spriteBatch.Draw(lease.Target, Vector2.Zero, tint);
            spriteBatch.End();
        }
    }

    private void DrawArenaEdge(SpriteBatch spriteBatch)
    {
        const int FadeInDuration = 60;
        SkeletronLegendHead head = Phase2Arena.MasterBehavior;
        if (head is null
            || !AnomalyClientConfig.Instance.EnableShaders
            || AnomalyEffects.Overlays.LegendaryNightArenaEdge is not AnomalyEffects.MiscShaderContainer shader
            || AnomalyTextures.Noise._Wave2 is not Asset<Texture2D> { IsLoaded: true } noise)
            return;
        int age = head.Phase2MusicElapsedTime - SkeletronLegendHead.Mechanic2Timeline.StartMusicTime;
        float opacity = Phase2Arena.Intensity * MathHelper.SmoothStep(0f, 1f,
            MathHelper.Clamp(age / (float)FadeInDuration, 0f, 1f));
        if (opacity <= 0f)
            return;

        // World pixels keep the boundary and noise scale independent of zoom and aspect ratio.
        spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearWrap,
            DepthStencilState.None, Main.Rasterizer, null, Matrix.Identity);
        shader.Data
            .UseImage1(noise)
            .SetCustomParameter("uScreenPosition", Main.screenPosition)
            .SetCustomParameter("uInverseViewMatrix", Matrix.Invert(Main.GameViewMatrix.TransformationMatrix))
            .SetCustomParameter("uCenter", Phase2Arena.Projectile.Center)
            .SetCustomParameter("uRadius", Phase2Arena.Radius)
            .SetCustomParameter("uFlowTime", age / 60f)
            .UseColor(LegendaryNightHandler.StarColor)
            .UseOpacity(opacity)
            .Apply();
        Main.instance.GraphicsDevice.SamplerStates[1] = SamplerState.LinearWrap;
        spriteBatch.Draw(TOTextures.FillerTexture, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), Color.White);
        spriteBatch.End();
    }

    private void DrawMagicCirclePart(SpriteBatch spriteBatch, Vector2 worldCenter, float radius, float rotation, bool drawStar, int circleIndex = 0, float opacityMultiplier = 1f)
    {
        if (radius <= 0.01f)
            return;
        float materialization = Phase2Arena.MaterializationIntensity;
        float warning = Phase2Arena.MaterializationWarning;
        float starburstGlow = circleIndex > 0 ? Phase2Arena.StarburstGlow : 0f;
        float intensity = Math.Max(materialization, starburstGlow);
        Color color = Color.Lerp(Color.Lerp(LegendaryNightHandler.StarColor, Color.OrangeRed, warning), Color.White, starburstGlow);
        if (circleIndex > 0)
            color = Color.Lerp(color, circleIndex == 2 ? LegendaryNightStarburstParameters.FastStarburstColor
                : LegendaryNightStarburstParameters.SlowStarburstColor, starburstGlow);
        float opacity = Phase2Arena.Intensity * MathHelper.Lerp(0.65f + warning * 0.3f, 1f, intensity) * opacityMultiplier;
        float width = MathHelper.Lerp(3f + warning * 5f, LegendaryNightPhase2Arena.LaserWidth, intensity);
        Color coreColor = Color.Lerp(Color.Lerp(color, Color.White, 0.75f), _MaterializationCoreColor, materialization);
        color = Color.Lerp(color, _MaterializationGlowColor, materialization);
        float glowWidth = MathHelper.Lerp(MathHelper.Lerp(14f, 22f, starburstGlow), 36f, materialization);
        AnomalyDrawUtils.DrawGlowingCircle(spriteBatch, worldCenter, radius, color, opacity, width, glowWidth, coreColor);
        if (!drawStar)
            return;

        Span<(Vector2 Start, Vector2 End)> edges = stackalloc (Vector2, Vector2)[5];
        for (int i = 0; i < edges.Length; i++)
            edges[i] = (worldCenter + (rotation + MathHelper.TwoPi * i / 5f).ToRotationVector2() * radius,
                worldCenter + (rotation + MathHelper.TwoPi * (i + 2) / 5f).ToRotationVector2() * radius);
        AnomalyDrawUtils.DrawGlowingLines(spriteBatch, edges, color, opacity, width, glowWidth, coreColor);
    }
}