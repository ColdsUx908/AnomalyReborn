namespace Anomalies.AnomalyMode;

public sealed class AnomalyPlayerSync : AnomalyPlayerBehavior
{
    public override decimal Priority => 100m;

    public override void OnEnterWorld() => AnomalySynchronization.SyncAnomalyModeFromServer();
}