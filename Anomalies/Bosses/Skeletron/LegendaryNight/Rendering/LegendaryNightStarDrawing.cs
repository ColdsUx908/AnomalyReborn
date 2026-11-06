namespace Anomalies.Bosses.Skeletron.LegendaryNight;

/// <summary>传说之夜星体绘制的共享几何与着色参数。</summary>
public static class LegendaryNightStarDrawing
{
    /// <summary>光晕贴图。</summary>
    [LoadTexture(LegendaryNightHandler.LegendaryNightPath + "Attacks/Stars/FallingStarLight")]
    private static Asset<Texture2D> _LightTexture;
    /// <summary>光晕贴图。</summary>
    public static Texture2D LightTexture => _LightTexture.Value;

    /// <summary>星体贴图。</summary>
    [LoadTexture(LegendaryNightHandler.LegendaryNightPath + "Attacks/Stars/FallingStar")]
    private static Asset<Texture2D> _StarTexture;
    /// <summary>星体贴图。</summary>
    public static Texture2D StarTexture => _StarTexture.Value;

    /// <summary>中等变体贴图。</summary>
    [LoadTexture(LegendaryNightHandler.LegendaryNightPath + "Attacks/Stars/FallingStar_Medium")]
    private static Asset<Texture2D> _MediumTexture;
    /// <summary>中等变体贴图。</summary>
    public static Texture2D MediumTexture => _MediumTexture.Value;

    /// <summary>发光贴图。</summary>
    [LoadTexture(LegendaryNightHandler.LegendaryNightPath + "Attacks/Stars/FallingStar_Glow")]
    private static Asset<Texture2D> _GlowTexture;
    /// <summary>发光贴图。</summary>
    public static Texture2D GlowTexture => _GlowTexture.Value;

    /// <summary>中等变体发光贴图。</summary>
    [LoadTexture(LegendaryNightHandler.LegendaryNightPath + "Attacks/Stars/FallingStar_MediumGlow")]
    private static Asset<Texture2D> _MediumGlowTexture;
    /// <summary>中等变体发光贴图。</summary>
    public static Texture2D MediumGlowTexture => _MediumGlowTexture.Value;

    /// <summary>困难变体贴图。</summary>
    [LoadTexture(LegendaryNightHandler.LegendaryNightPath + "Attacks/Stars/FallingStar_Hard")]
    private static Asset<Texture2D> _HardTexture;
    /// <summary>困难变体贴图。</summary>
    public static Texture2D HardTexture => _HardTexture.Value;
    public static void DrawFallingStar(Vector2 position, float rotation, float scale, Vector2 velocity, int trailType = 0, bool useMediumTexture = false, Vector2[] oldPositions = null, Vector2 trailOffset = default, Color? trailColorOverride = null)
    {
        const float MediumToLargeScaleFactor = 2.55f; // 大型落星与中型落星的缩放比例

        position -= Main.screenPosition;

        float timeForVisualEffects = (float)Main.timeForVisualEffects / 60f;

        switch (trailType)
        {
            case 1:
                Texture2D lightTexture = LightTexture;
                Vector2 lightOrigin = new(lightTexture.Width / 2f, lightTexture.Height * 0.2f);
                Color lightColor = ((trailColorOverride ?? Color.Blue) * 0.25f) with { A = 0 };
                float trailRotation = velocity.ToRotation(MathHelper.PiOver2);
                float lightScale = scale * 7.5f;
                if (useMediumTexture)
                    lightScale /= MediumToLargeScaleFactor;
                PolarVector2 offset = new(12f * lightScale, MathHelper.TwoPi * timeForVisualEffects - MathHelper.PiOver2);
                Main.spriteBatch.Draw(lightTexture, position + offset, null, lightColor, trailRotation, lightOrigin, lightScale * 1.5f, SpriteEffects.None, 0f);
                Main.spriteBatch.Draw(lightTexture, position + offset.RotatedBy(TOMathUtils.PiOver3 * 2), null, lightColor, trailRotation, lightOrigin, lightScale * 1.1f, SpriteEffects.None, 0f);
                Main.spriteBatch.Draw(lightTexture, position + offset.RotatedBy(TOMathUtils.PiOver3 * 4), null, lightColor, trailRotation, lightOrigin, lightScale * 1.3f, SpriteEffects.None, 0f);

                for (float phaseOffset = 0f; phaseOffset < 1f; phaseOffset += 0.5f)
                {
                    float pulsePhase = timeForVisualEffects % 0.5f / 0.5f;
                    pulsePhase = (pulsePhase + phaseOffset) % 1f;

                    float pulseOpacity = pulsePhase * 2f;
                    if (pulseOpacity > 1f)
                        pulseOpacity = 2f - pulseOpacity;

                    Color pulseColor = trailColorOverride.HasValue ? Color.Lerp(trailColorOverride.Value, Color.White, 0.35f) : Color.White;
                    Main.spriteBatch.Draw(lightTexture, position - velocity * 0.7f, null, (pulseColor * 0.2f) with { A = 0 } * pulseOpacity, trailRotation, lightOrigin, lightScale * (0.3f + pulsePhase * 0.5f), SpriteEffects.None, 0f);
                }
                break;
            case 2:
                DrawTrailType2(position + Main.screenPosition, scale, velocity, oldPositions, trailOffset, trailColorOverride, useMediumTexture);
                break;
        }

        Texture2D starTexture = useMediumTexture ? MediumTexture : StarTexture;

        // 计算五角星材质的几何中心位置。
        // 注意：SpriteBatch.Draw 中的 origin 参数应以未缩放的纹理像素为单位传入，
        // 因此这里使用 texture.Width/Height（未乘 scale）。
        float cosPiOver5 = MathF.Cos(TOMathUtils.PiOver5);
        Vector2 starOrigin = new(starTexture.Width / 2f, starTexture.Height / (1f + cosPiOver5));

        //TODrawUtils.DrawBorderTexture(Main.spriteBatch, starTexture, position, null, LegendaryNightHandler.StarColor, projectile.rotation, starOrigin, projectile.scale, way: 15, borderWidth: 3f);

        Texture2D glowTexture = useMediumTexture ? MediumGlowTexture : GlowTexture;
        for (int i = 0; i < 10; i++)
        {
            Color color = (Color.Yellow * 0.008f * MathF.Pow(i, 1.3f)) with { A = 0 };
            Main.spriteBatch.Draw(glowTexture, position, null, color, rotation, starOrigin, scale * (1.6f - i * 0.06f), SpriteEffects.None, 0f);
        }

        Main.spriteBatch.Draw(starTexture, position, null, Color.White, rotation, starOrigin, scale, SpriteEffects.None, 0f);
    }

    /// <summary>绘制类型2的连续光斑拖尾；位置和历史采样均使用世界坐标。</summary>
    public static void DrawTrailType2(Vector2 position, float scale, Vector2 velocity, Vector2[] oldPositions,
        Vector2 trailOffset = default, Color? trailColorOverride = null, bool useMediumTexture = false)
    {
        position -= Main.screenPosition;
        Texture2D bloom = ParticleHandler.GetTexture<BloomParticle>();
        float sizeFactor = 4f * (useMediumTexture ? scale / 2.55f : scale);
        float speed = velocity.Length();
        if (oldPositions is null)
            return;

        int trailSamples = oldPositions.Length;

        for (int i = trailSamples - 1; i >= 0; i--)
        {
            if (oldPositions[i] == Vector2.Zero)
                continue;
            float progress = (float)i / trailSamples;
            float strength = 1f - progress;
            Vector2 trailPosition = oldPositions[i] + trailOffset - Main.screenPosition;
            Vector2 nextPosition = i == 0 ? position
                : oldPositions[i - 1] != Vector2.Zero ? oldPositions[i - 1] + trailOffset - Main.screenPosition : trailPosition;
            Vector2 segment = nextPosition - trailPosition;
            float segmentLength = segment.Length();
            float segmentRotation = segment.ToRotation();
            Vector2 segmentCenter = (trailPosition + nextPosition) * 0.5f;
            float diameter = MathHelper.Lerp(12f, 240f, strength) * sizeFactor;
            float intensity = strength * strength * MathHelper.Clamp(speed / 2f, 0f, 1f);
            Color outerColor = ((trailColorOverride ?? new Color(255, 90, 10)) with { A = 0 }) * (intensity * 0.12f);
            Color innerColor = ((trailColorOverride.HasValue ? Color.Lerp(trailColorOverride.Value, Color.White, 0.35f) : new Color(255, 225, 100)) with { A = 0 }) * (intensity * 0.18f);
            // 将每个光斑置于相邻采样点中间，沿轨迹拉长；额外重叠抵消光斑边缘的淡出。
            // 内芯也独立补足线段长度，避免随外层一起缩小后再次出现断点。
            Vector2 outerScale = new((diameter + segmentLength * 2f) / bloom.Width, diameter / bloom.Height);
            float innerDiameter = diameter * 0.45f;
            Vector2 innerScale = new((innerDiameter + segmentLength * 2f) / bloom.Width, innerDiameter / bloom.Height);
            Main.spriteBatch.DrawFromCenter_VectorScale(bloom, segmentCenter, null, outerColor, segmentRotation, outerScale);
            Main.spriteBatch.DrawFromCenter_VectorScale(bloom, segmentCenter, null, innerColor, segmentRotation, innerScale);
        }
    }
}