using CalamityMod.NPCs.NormalNPCs;

namespace CalamityAnomalies.Publicizers.CalamityMod.NPCs;

#pragma warning disable IDE1006

public record KingSlimeJewelRuby_Publicizer : PublicizerNoSource<KingSlimeJewelRuby>
{
    // BoltShootGateValue (const)
    public const int BoltShootGateValue = 60;

    // BoltShootGateValue_Death (const)
    public const int BoltShootGateValue_Death = 75;

    // BoltShootGateValue_BossRush (const)
    public const int BoltShootGateValue_BossRush = 45;

    // LightTelegraphDuration (const)
    public const float LightTelegraphDuration = 45f;
}