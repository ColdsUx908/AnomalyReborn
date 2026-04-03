namespace CalamityAnomalies.Anomaly.KingSlime;

public interface IKingSlimeJewel : IContentLoader
{
    public abstract bool HasEnteredPhase2 { get; set; }
    public abstract bool CanAttack { get; set; }
    public abstract bool KingSlimeDead { get; set; }
}
