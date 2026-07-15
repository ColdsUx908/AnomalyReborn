// Developed by ColdsUx

namespace CalamityAnomalies.Visuals;

/// <summary>
/// 血量阈值指示器，用于在异象模式下 Boss 血条上标记关键血量百分比。
/// </summary>
public class HPThresholdIndicator
{
    /// <summary>
    /// 获取阈值浮点值（0~1）的委托。返回值代表血量条上的位置比例。
    /// </summary>
    /// <param name="indicator">当前指示器实例。</param>
    /// <param name="npc">关联的 NPC。</param>
    /// <param name="bar">对应的 BetterBossHPUI。</param>
    /// <returns>比例值，0~1 之间。</returns>
    public delegate float HPThresholdIndicatorValueFunction(HPThresholdIndicator indicator, NPC npc, CABossHPUI bar);

    /// <summary>
    /// 自定义更新行为委托。
    /// </summary>
    /// <param name="indicator">当前指示器实例。</param>
    /// <param name="npc">关联的 NPC。</param>
    /// <param name="bar">对应的 BetterBossHPUI。</param>
    /// <returns>返回 <see langword="false"/> 表示已处理更新，阻止默认计时逻辑；返回 <see langword="true"/> 则继续默认计时和生命周期控制。</returns>
    public delegate bool HPThresholdIndicatorUpdateFunction(HPThresholdIndicator indicator, NPC npc, CABossHPUI bar);

    /// <summary>
    /// 自定义绘制行为委托。
    /// </summary>
    /// <param name="indicator">当前指示器实例。</param>
    /// <param name="npc">关联的 NPC。</param>
    /// <param name="bar">对应的 BetterBossHPUI。</param>
    /// <param name="spriteBatch">用于绘制的 SpriteBatch。</param>
    /// <param name="center">指示器中心点的坐标。</param>
    /// <returns>返回 <see langword="false"/> 表示已处理绘制，阻止默认绘制；返回 <see langword="true"/> 则继续默认绘制逻辑。</returns>
    public delegate bool HPThresholdIndicatorDrawFunction(HPThresholdIndicator indicator, NPC npc, CABossHPUI bar, SpriteBatch spriteBatch, Vector2 center);

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
    /// 获取当前指示器的阈值比例。
    /// </summary>
    /// <returns>比例值，0~1。</returns>
    public float GetValue(NPC npc, CABossHPUI bar) => ValueFunction?.Invoke(this, npc, bar) ?? 0f;
}