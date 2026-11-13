namespace Anomalies.SpecialContents.Developer.EventideReunion;

// 不依赖实体的计账规则，便于单独验证取整、治疗和标记生命周期。
public sealed class EventideReunionSamsaraRecord
{
    public const int Duration = 300;
    public const int Cooldown = 900;
    public int Remaining { get; private set; }
    public long Damage { get; private set; }
    public int LastLife { get; private set; }
    public bool Active => Remaining > 0;

    public bool Begin(int life)
    {
        if (Active || life <= 0)
            return false;
        Remaining = Duration;
        Damage = 0;
        LastLife = life;
        return true;
    }

    public void Observe(int life)
    {
        if (!Active)
            return;
        long lost = Math.Max(0L, (long)LastLife - Math.Max(0, life));
        Damage = Math.Min(long.MaxValue - lost, Damage) + lost;
        LastLife = Math.Max(0, life);
    }

    public bool Tick()
    {
        if (!Active)
            return false;
        return --Remaining == 0;
    }

    public int Settlement => (int)Math.Min(int.MaxValue, Damage / EventideReunionDamageConstants.SamsaraSettlementDamageDenominator * EventideReunionDamageConstants.SamsaraSettlementDamageNumerator
        + (Damage % EventideReunionDamageConstants.SamsaraSettlementDamageDenominator * EventideReunionDamageConstants.SamsaraSettlementDamageNumerator
            + EventideReunionDamageConstants.SamsaraSettlementDamageDenominator - 1) / EventideReunionDamageConstants.SamsaraSettlementDamageDenominator);

    public void Clear()
    {
        Remaining = 0;
        Damage = 0;
        LastLife = 0;
    }

    public static bool Prefer(bool boss, int lifeMax, int index, bool bestBoss, int bestLifeMax, int bestIndex)
        => bestIndex < 0 || boss && !bestBoss || boss == bestBoss
            && (lifeMax > bestLifeMax || lifeMax == bestLifeMax && index < bestIndex);
}
