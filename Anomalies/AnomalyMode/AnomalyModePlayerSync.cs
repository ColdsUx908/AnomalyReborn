// Developed by ColdsUx

namespace Anomalies.AnomalyMode;

public sealed class AnomalyModePlayerSync : AnomalyPlayerBehavior
{
    public override decimal Priority => 100m;

    public override void OnEnterWorld() => AnomalySynchronization.SyncAnomalyModeFromServer();
}