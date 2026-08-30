using Anomalies.DataStructures;

namespace Anomalies.Visuals.BossBar;

/// <summary>
/// 血量阈值指示器，用于在异象模式下 Boss 血条上标记关键血量百分比。
/// </summary>
public class HPThresholdIndicator
{
    public const string Path = "Anomalies/Visuals/BossHealthBar/";

    // 纹理已迁移到 BossBarTextures.Fancy

    /// <summary>
    /// 获取阈值浮点值（0~1）的委托。返回值代表血量条上的位置比例。
    /// </summary>
    /// <param name="indicator">当前指示器实例。</param>
    /// <param name="npc">关联的 NPC。</param>
    /// <param name="bar">对应的 BetterBossHPUI。</param>
    /// <returns>比例值，0~1 之间。</returns>
    public delegate float HPThresholdIndicatorValueFunction(HPThresholdIndicator indicator, NPC npc, BossHealthBar bar);

    /// <summary>
    /// 自定义更新行为委托。
    /// </summary>
    /// <param name="indicator">当前指示器实例。</param>
    /// <param name="npc">关联的 NPC。</param>
    /// <param name="bar">对应的 BetterBossHPUI。</param>
    /// <returns>返回 <see langword="false"/> 表示已处理更新，阻止默认计时逻辑；返回 <see langword="true"/> 则继续默认计时和生命周期控制。</returns>
    public delegate bool HPThresholdIndicatorUpdateFunction(HPThresholdIndicator indicator, NPC npc, BossHealthBar bar);

    /// <summary>
    /// 自定义绘制行为委托。
    /// </summary>
    /// <param name="indicator">当前指示器实例。</param>
    /// <param name="npc">关联的 NPC。</param>
    /// <param name="bar">对应的 BetterBossHPUI。</param>
    /// <param name="spriteBatch">用于绘制的 SpriteBatch。</param>
    /// <param name="center">指示器中心点的坐标。</param>
    /// <returns>返回 <see langword="false"/> 表示已处理绘制，阻止默认绘制；返回 <see langword="true"/> 则继续默认绘制逻辑。</returns>
    public delegate bool HPThresholdIndicatorDrawFunction(HPThresholdIndicator indicator, NPC npc, BossHealthBar bar, SpriteBatch spriteBatch, Vector2 center);

    /// <summary>
    /// 存在计时器，控制指示器的显现动画。
    /// </summary>
    public int Timer;

    /// <summary>
    /// 淡出计时器，在血量低于阈值后开始计时，控制指示器的消失动画。
    /// </summary>
    public int EaseOutTimer;

    /// <summary>
    /// 获取阈值比例的函数。
    /// </summary>
    /// <remarks>用法参见 <see cref="HPThresholdIndicatorValueFunction"/>。</remarks>
    public HPThresholdIndicatorValueFunction ValueFunction;

    /// <summary>
    /// 自定义更新函数。
    /// </summary>
    /// <remarks>用法参见 <see cref="HPThresholdIndicatorUpdateFunction"/>。</remarks>
    public HPThresholdIndicatorUpdateFunction CustomUpdateFunction;

    /// <summary>
    /// 自定义绘制函数。
    /// </summary>
    /// <remarks>用法参见 <see cref="HPThresholdIndicatorDrawFunction"/>。</remarks>
    public HPThresholdIndicatorDrawFunction CustomDrawFunction;

    /// <summary>
    /// 是否为亚阶段指示器。
    /// <br/>若为 <see langword="true"/>，默认绘制时将使用银色纹理，而非金色纹理。此设定仅影响默认绘制逻辑，不会限制自定义绘制函数的表现形式。
    /// </summary>
    public bool IsSubPhaseIndicator;

    /// <summary>
    /// 粒子效果集，用于在指示器中心附近生成粒子效果。
    /// </summary>
    public EnchantedParticleSet ParticleSet;

    /// <summary>
    /// 获取当前指示器的阈值比例。
    /// </summary>
    /// <returns>比例值，0~1。</returns>
    public float GetValue(NPC npc, BossHealthBar bar) => ValueFunction?.Invoke(this, npc, bar) ?? 0f;

    public void Update(NPC npc, BossHealthBar bar)
    {
        if (CustomUpdateFunction?.Invoke(this, npc, bar) == false)
            return;

        Timer++;
        if (npc.LifeRatio <= GetValue(npc, bar) || EaseOutTimer > 0)
            EaseOutTimer++;

        float value = GetValue(npc, bar);
        float realLifeRatio = npc.LifeRatio;
        float particleIntensity = Utils.Remap(realLifeRatio - value, IsSubPhaseIndicator ? 0.04f : 0.05f, 0f, 0f, 1f); //接近或超过阈值时粒子效果更明显

        if (particleIntensity > 0f)
        {
            float interpolatedIntensity = TOMathUtils.Interpolation.QuadraticEaseOut(particleIntensity);

            ParticleSet.ParticleSpawnRate = realLifeRatio >= value ? particleIntensity * (IsSubPhaseIndicator ? 0.4f : 0.5f) : 0f;
            ParticleSet.MaxEdgeRadius = interpolatedIntensity * (IsSubPhaseIndicator ? 32f : 40f);
            ParticleSet.Scale = interpolatedIntensity * 0.5f;

            ParticleSet.Update();
        }
    }

    /// <summary>
    /// 绘制主方法。
    /// </summary>
    /// <param name="npc"></param>
    /// <param name="bar"></param>
    /// <param name="spriteBatch"></param>
    /// <param name="center"></param>
    public void Draw(NPC npc, BossHealthBar bar, SpriteBatch spriteBatch, Vector2 center)
    {
        if (CustomDrawFunction?.Invoke(this, npc, bar, spriteBatch, center) == false)
            return;

        float opacity = Math.Clamp(bar.AnimationCompletionRatio * 3f, 0f, 1f) * Math.Clamp((60f - EaseOutTimer) / 60f, 0f, 1f);
        float value = GetValue(npc, bar);
        if (value == 0f)
            return;

        //依次绘制：指示器底层，粒子效果，指示器中心宝石

        spriteBatch.DrawFromCenter(IsSubPhaseIndicator ? BossBarTextures.Fancy.SubPhaseIndicator : BossBarTextures.Fancy.PhaseIndicator, center, null, Color.White * opacity);
        ParticleSet.DrawSet(center + Main.screenPosition);
        spriteBatch.DrawFromCenter(IsSubPhaseIndicator ? BossBarTextures.Fancy.SubPhaseIndicatorCenter : BossBarTextures.Fancy.PhaseIndicatorCenter, center, null, Color.White * opacity);
    }
}