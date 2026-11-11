using Anomalies.Assets.Effects;

namespace Anomalies.Bosses.Skeletron.LegendaryNight;

/// <summary>
/// 恒星结算与终幕冲击波的共享生命周期和绘制基类。
/// </summary>
/// <remarks>
/// <para>ai[0]：爆发音乐帧；ai[1]：最大扩张半径（世界像素）；ai[2]：主体 NPC 索引 + 1。</para>
/// <para>localAI：未使用；框架 Timer1..5：未使用；主体消失后由 timeLeft 接续生命周期。</para>
/// <para>无实体专属 ExtraAI；普通伤害路径关闭，两个实现分别执行本地单次伤害或试炼失败判定。</para>
/// </remarks>
public abstract class LegendStellarShockwave : AnomalyModProjectile
{
    /// <summary>伴星爆炸持续时间，单位为游戏帧。</summary>
    internal const int CompanionExplosionDuration = 72;
    internal static float CompanionShockwaveRadiusRatio(int count) => count == 0 ? 0.9f : 1.2f;
    internal static float CompanionShockwaveProgress(int age) => BlastExpansion(age);

    private static float BlastExpansion(float age)
    {
        float progress = MathHelper.Clamp(age / (float)(CompanionExplosionDuration - 1), 0f, 1f);
        return TOMathUtils.Interpolation.CubicEaseOut(progress);
    }

    protected static float CoastingOuterExpansion(float age)
    {
        const int CoastStart = 36;
        const float BrakingTime = 105f;
        if (age <= CoastStart)
            return BlastExpansion(age);

        // Match the original cubic's tangent before coasting with a positive, decreasing speed.
        const float RemainingProgress = 1f - CoastStart / (float)(CompanionExplosionDuration - 1);
        const float InitialSpeed = 3f * RemainingProgress * RemainingProgress / (CompanionExplosionDuration - 1);
        float coastAge = age - CoastStart;
        return CompanionShockwaveProgress(CoastStart) + InitialSpeed * coastAge / (1f + coastAge / BrakingTime);
    }

    protected static Vector4 NebulaRemnantState(int age, int shellDuration)
    {
        const int RemnantStart = 31;
        const int BlastEnd = 70;
        float expansion = MathHelper.Clamp((age - RemnantStart) / (float)(shellDuration - 1 - RemnantStart), 0f, 1f);
        float transition = MathHelper.SmoothStep(0f, 1f, (age - RemnantStart) / (float)(BlastEnd - RemnantStart));
        float erosion = MathHelper.SmoothStep(0f, 1f, (expansion - 0.12f) / 0.87f);
        float dilution = 1f - MathHelper.SmoothStep(0f, 1f, (expansion - 0.15f) / 0.85f);
        return new Vector4(expansion, transition, erosion, dilution);
    }

    protected static Vector3 NebulaInteriorState(int age, int duration)
    {
        const int InteriorFadeStart = 52;
        float drift = MathHelper.Clamp((age - InteriorFadeStart) / (float)(duration - 1 - InteriorFadeStart), 0f, 1f);
        float erosion = MathHelper.SmoothStep(0f, 1.04f, drift * drift);
        float density = 1f - MathHelper.SmoothStep(0f, 1f, (drift - 0.25f) / 0.75f);
        return new Vector3(drift, erosion, density);
    }

    internal bool IsOwnedBy(NPC head) => (int)Projectile.ai[2] == head.whoAmI + 1;

    protected abstract bool YellowPalette { get; }
    protected virtual string ExplosionPass => "Pass0";
    protected virtual float VisualOpacity => 1f;
    protected virtual float ShaderRadiusScale => 1f;
    protected virtual float InitialExpansion(int age) => CompanionShockwaveProgress(age);
    protected virtual float OuterExpansion(int age) => CompanionShockwaveProgress(age);
    protected virtual Vector4 RemnantState(int age) => Vector4.Zero;
    protected virtual Vector3 InteriorState(int age) => Vector3.Zero;
    protected virtual Color OuterColor => YellowPalette ? new Color(255, 200, 20, 0) : new Color(70, 130, 255, 0);
    protected virtual Color InnerColor => YellowPalette ? new Color(255, 245, 170, 0) : new Color(195, 225, 255, 0);
    protected virtual int Lifetime => CompanionExplosionDuration;
    protected virtual int InitialTimeLeft => Lifetime;
    protected int Age => LegendaryNightHandler.TryGetLegendaryNightHead((int)Projectile.ai[2], out SkeletronLegendHead head, oneBased: true)
        ? head.Phase2MusicElapsedTime - (int)Projectile.ai[0] : InitialTimeLeft - Projectile.timeLeft;
    public override string Texture => TOTextures.InvisibleTexturePath;
    public override string LocalizationCategory => LegendaryNightHandler.LocalizationCategory;
    public override void SetStaticDefaults() => ProjectileID.Sets.DrawScreenCheckFluff[Type] = 4000;
    public override void SetDefaults()
    {
        Projectile.width = Projectile.height = 16;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.penetrate = -1;
        Projectile.netImportant = true;
        Projectile.hide = !YellowPalette;
        Projectile.timeLeft = InitialTimeLeft;
    }
    public override bool ShouldUpdatePosition() => false;
    public override bool? CanDamage() => false;
    public override void AI()
    {
        int age = Age;
        if (age >= Lifetime)
        {
            Projectile.Kill();
            return;
        }
        if (age >= 0 && !Main.dedServ)
            UpdateExplosion(age);
    }
    protected abstract void UpdateExplosion(int age);
    public override bool PreDraw(ref Color lightColor)
    {
        if (YellowPalette)
            DrawExplosion(Main.spriteBatch);
        return false;
    }

    internal void DrawExplosion(SpriteBatch spriteBatch)
    {
        int age = Age;
        if (age < 0 || age >= Lifetime)
            return;
        float maximumRadius = Projectile.ai[1];
        float progress = MathHelper.Clamp(age / (float)(CompanionExplosionDuration - 1), 0f, 1f);
        float opacity = VisualOpacity * (1f - MathHelper.SmoothStep(0f, 1f, MathHelper.Clamp((progress - 0.82f) / 0.18f, 0f, 1f)));
        Vector4 remnantState = RemnantState(age);
        float outerExpansion = OuterExpansion(age);
        AnomalyEffects.MiscShaderContainer shader = AnomalyClientConfig.Instance.EnableShaders
            ? AnomalyEffects.Overlays.LegendStellarExplosion : null;
        if (shader is not null)
        {
            float drawSize = maximumRadius * 2f * ShaderRadiusScale;
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            shader.Data.SetCustomParameter("uGlobalTime", age / 60f)
                .SetCustomParameter("uLifetimeRatio", progress)
                .SetCustomParameter("uRemnantState", remnantState)
                .SetCustomParameter("uInteriorState", InteriorState(age))
                .SetCustomParameter("uOuterGeometry", new Vector2(outerExpansion, ShaderRadiusScale))
                .SetCustomParameter("uYellowPalette", YellowPalette ? 1f : 0f)
                .SetCustomParameter("uExpansionRatio", InitialExpansion(age))
                .SetCustomParameter("uNoiseTexture", AnomalyTextures.Noise.Perlin2)
                .SetCustomParameter("uTextureSize", new Vector2(maximumRadius * 2f))
                .Apply();
            shader.Asset.Value.CurrentTechnique.Passes[ExplosionPass].Apply();
            Texture2D pixel = TOTextures.FillerTexture;
            spriteBatch.DrawFromCenter(pixel, Projectile.Center - Main.screenPosition, null,
                Color.White * MathHelper.Lerp(opacity, VisualOpacity, remnantState.Y), scale: drawSize / pixel.Width);
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
        }
        else
        {
            opacity = MathHelper.Lerp(opacity, VisualOpacity * remnantState.W, remnantState.Y);
            if (opacity <= 0f)
                return;

            // 回退亮环沿用着色器外圈的扩张轨迹。
            const int Segments = 240;
            float radius = Math.Max(0f, outerExpansion * maximumRadius - 10f);
            Texture2D pixel = TOTextures.FillerTexture;
            for (int i = 0; i < Segments; i++)
            {
                Vector2 start = Projectile.Center + (MathHelper.TwoPi * i / Segments).ToRotationVector2() * radius;
                Vector2 end = Projectile.Center + (MathHelper.TwoPi * (i + 1) / Segments).ToRotationVector2() * radius;
                Vector2 edge = end - start;
                Vector2 center = (start + end) * 0.5f - Main.screenPosition;
                spriteBatch.DrawFromCenter_VectorScale(pixel, center, null, OuterColor * (opacity * 0.85f),
                    edge.ToRotation(), new Vector2(edge.Length() + 1f, 20f) / pixel.Size());
                spriteBatch.DrawFromCenter_VectorScale(pixel, center, null, InnerColor * opacity,
                    edge.ToRotation(), new Vector2(edge.Length() + 1f, 6f) / pixel.Size());
            }
        }
    }
}
