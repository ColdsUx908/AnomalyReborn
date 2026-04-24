namespace CalamityAnomalies.DataStructures;

/// <summary>
/// 用于存储弹幕在不同游戏难度与特殊模式下的预设伤害值的容器结构。
/// </summary>
/// <remarks>
/// 该结构为只读记录结构体，通过构造函数一次性初始化所有难度对应的伤害值。
/// 各伤害参数均存在倍数约束，以确保在进行反算基础伤害时结果为整数，避免取整误差。
/// <para>
/// 使用 <see cref="Value"/> 属性可获取适应于当前游戏环境（包括难度与激活的特殊模式）的实际弹幕伤害值。
/// 属性内部会根据激活模式的优先级自动选择对应的预设伤害，并利用原版弹幕伤害乘数反推出基础伤害。
/// </para>
/// </remarks>
/// <param name="NormalDamage">普通模式伤害，须为 2 的倍数。</param>
/// <param name="ExpertDamage">专家模式伤害，须为 4 的倍数。</param>
/// <param name="MasterDamage">大师模式伤害，须为 6 的倍数。</param>
/// <param name="LegendaryDamage">传奇模式伤害，须为 6 的倍数。</param>
/// <param name="AnomalyDamage">异象模式伤害，须为 6 的倍数。</param>
/// <param name="AnomalyUltramundaneDamage">异象超凡伤害，须为 6 的倍数。</param>
public readonly record struct ProjectileDamageContainer(
    int NormalDamage, int ExpertDamage, int MasterDamage, int LegendaryDamage,
    int AnomalyDamage, int AnomalyUltramundaneDamage)
{
    /// <summary>
    /// 专家模式下接触伤害的原版乘数。
    /// </summary>
    public const float ExpertContactVanillaMultiplier = 2f;

    /// <summary>
    /// 大师模式下接触伤害的原版乘数。
    /// </summary>
    public const float MasterContactVanillaMultiplier = 3f;

    /// <summary>
    /// 普通模式下弹幕伤害的原版乘数。
    /// </summary>
    public const float NormalProjectileVanillaMultiplier = 2f;

    /// <summary>
    /// 专家模式下弹幕伤害的原版乘数。
    /// </summary>
    public const float ExpertProjectileVanillaMultiplier = 4f;

    /// <summary>
    /// 大师模式下弹幕伤害的原版乘数。
    /// </summary>
    public const float MasterProjectileVanillaMultiplier = 6f;

    /// <summary>
    /// 获取适应于当前游戏环境的实际弹幕伤害值。
    /// </summary>
    /// <returns>
    /// 一个 <see cref="int"/> 值，表示根据当前激活的难度与特殊模式反算得出的弹幕基础伤害。
    /// 该值可直接用于设置 <see cref="Projectile.damage"/> 字段。
    /// </returns>
    /// <remarks>
    /// 属性的计算逻辑分为三步：
    /// <list type="number">
    /// <item>
    /// <description>
    /// <b>确定伤害调整系数</b>：
    /// 根据 <see cref="Main.masterMode"/> 和 <see cref="Main.expertMode"/> 选取对应的弹幕伤害乘数常量：
    /// 大师模式为 <see cref="MasterProjectileVanillaMultiplier"/>（6），
    /// 专家模式为 <see cref="ExpertProjectileVanillaMultiplier"/>（4），
    /// 普通模式为 <see cref="NormalProjectileVanillaMultiplier"/>（2）。
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// <b>根据激活模式选择预设伤害值</b>：
    /// 按照以下优先级依次判断当前激活的特殊模式：
    /// <list type="bullet">
    /// <item/><description/>异象超凡（<see cref="CASharedData.AnomalyUltramundane"/>）
    /// <item/><description/>异象模式（<see cref="CASharedData.Anomaly"/>）
    /// <item/><description/>传奇模式（<see cref="TOSharedData.LegendaryMode"/>）
    /// <item/><description/>大师模式（<see cref="Main.masterMode"/>）
    /// <item/><description/>专家模式（<see cref="Main.expertMode"/>）
    /// <item/><description/>普通模式（默认）
    /// </list>
    /// 较高优先级的模式激活时，将忽略较低优先级的模式，直接使用对应参数的伤害值。
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// <b>反算基础伤害</b>：
    /// 用选定的预设伤害值除以第一步中获得的伤害调整系数，并通过 <see cref="MathF.Round(float)"/> 取整后返回。
    /// 由于各预设伤害值均被约束为对应乘数的整数倍，除法结果应为整数，舍入步骤用于消除浮点运算可能产生的微小误差。
    /// </description>
    /// </item>
    /// </list>
    /// </remarks>
    public int Value
    {
        get
        {
            float damageAdjustment = Main.masterMode ? MasterProjectileVanillaMultiplier : Main.expertMode ? ExpertProjectileVanillaMultiplier : NormalProjectileVanillaMultiplier;
            float expectedDamage =
                CASharedData.AnomalyUltramundane ? AnomalyUltramundaneDamage
                : CASharedData.Anomaly ? AnomalyDamage
                : TOSharedData.LegendaryMode ? LegendaryDamage
                : Main.masterMode ? MasterDamage
                : Main.expertMode ? ExpertDamage
                : NormalDamage;

            return (int)MathF.Round(expectedDamage / damageAdjustment);
        }
    }
}