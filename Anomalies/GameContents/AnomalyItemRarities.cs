namespace Anomalies.GameContents;

public sealed class Celestial : ModRarity
{
    public const int CelestialPrice = 1000000;
    public override Color RarityColor => TOSharedData.CelestialColor;
    public override int GetPrefixedRarity(int offset, float valueMult) => Type;
}

public sealed class AnomalyRarity : ModRarity
{
    public const int AnomalyPrice = 100000;
    public override Color RarityColor => AnomalySharedData.AnomalyTitleColor;
    public override int GetPrefixedRarity(int offset, float valueMult) => Type;
}

public sealed class AnomalyUltramundaneRarity : ModRarity
{
    public const int AnomalyUltramundanePrice = 500000;
    public override Color RarityColor => AnomalySharedData.AnomalyUltramundaneTitleColor;
    public override int GetPrefixedRarity(int offset, float valueMult) => Type;
}