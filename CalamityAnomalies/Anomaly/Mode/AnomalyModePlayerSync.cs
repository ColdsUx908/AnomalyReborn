// Developed by ColdsUx

namespace CalamityAnomalies.Anomaly.Mode;

public sealed class AnomalyModePlayerSync : CAPlayerBehavior
{
    public override decimal Priority => 100m;

    public override void OnEnterWorld() => CASynchronization.SyncAnomalyModeFromServer();
}