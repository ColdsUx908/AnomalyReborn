namespace Anomalies.SpecialContents.Developer.EventideReunion;

/// <summary>
/// 落日遗逢各技能、印记与共鸣共用的伤害倍率参数。
/// </summary>
public static class EventideReunionDamageConstants
{
    /// <summary>用于 EventideReunionSunSkill 的冲刺命中，以武器伤害为基准的倍率。</summary>
    public const float SunChargeDamageMultiplier = 6f;

    /// <summary>用于 EventideReunionSunSkill 的生成爆炸弹幕，以武器伤害为基准的倍率。</summary>
    public const float SunExplosionDamageMultiplier = 4f;

    /// <summary>用于 EventideReunionSunSkill 的后续横斩命中，以武器伤害为基准的倍率。</summary>
    public const float SunSlashDamageMultiplier = 5f;

    /// <summary>用于 EventideReunionSunSkill 的旋身挥击命中，以武器伤害为基准的倍率。</summary>
    public const float SunSpinDamageMultiplier = 3f;

    /// <summary>用于 EventideReunionSamsaraSkill 的生成刺击伤害代理，以武器伤害为基准的倍率。</summary>
    public const float SamsaraThrustDamageMultiplier = 4f;

    /// <summary>用于 EventideReunionSamsaraSkill 的生成转矛和横抽伤害代理，以武器伤害为基准的倍率。</summary>
    public const float SamsaraSweepDamageMultiplier = 3f;

    /// <summary>用于 EventideReunionDeathSkill 的生成镰刀波，以武器伤害为基准的倍率。</summary>
    public const float DeathWaveDamageMultiplier = 5f;

    /// <summary>用于 EventideReunionDeathSkill 的生成突进伤害代理，以武器伤害为基准的倍率。</summary>
    public const float DeathDashDamageMultiplier = 3f;

    /// <summary>用于 EventideReunionDeathSkill 的生成回身横斩伤害代理，以武器伤害为基准的倍率。</summary>
    public const float DeathSlashDamageMultiplier = 5f;

    /// <summary>用于 EventideReunionSunSkill.ModifyHitNPC 和 EventideReunionSunExplosion.ModifyHitNPC 的白天伤害修正倍率。</summary>
    public const float SunDayDamageMultiplier = 1.2f;

    /// <summary>用于 EventideReunionSunSkill.ModifyHitNPC 和 EventideReunionSunExplosion.ModifyHitNPC 的夜晚伤害修正倍率。</summary>
    public const float SunNightDamageMultiplier = 0.9f;

    /// <summary>用于 EventideReunionDeathMarkBehavior.ModifyIncomingHit，对死亡印记目标受到的最终伤害进行增幅。</summary>
    public const float DeathMarkIncomingDamageMultiplier = 1.15f;

    /// <summary>用于 EventideReunionResonanceRules.DamageMultiplier，每种已记录相位乘算一次近战伤害倍率。</summary>
    public const float ResonanceDamageMultiplierPerAspect = 1.08f;

    /// <summary>用于 EventideReunionSamsaraRecord.Settlement，轮回标记累计伤害结算比例的分子。</summary>
    public const int SamsaraSettlementDamageNumerator = 35;

    /// <summary>用于 EventideReunionSamsaraRecord.Settlement，轮回标记累计伤害结算比例的分母，同时用于整数向上取整。</summary>
    public const int SamsaraSettlementDamageDenominator = 100;
}
