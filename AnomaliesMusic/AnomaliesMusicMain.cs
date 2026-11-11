namespace AnomaliesMusic;

public sealed class AnomaliesMusicMain : Mod
{
    internal static AnomaliesMusicMain Instance { get; private set; }

    public override object Call(params object[] args) => args switch
    {
        ["IsPathwaytoReverieEnabled"] => AnomaliesMusicConfig.Instance.PathwaytoReverie,
        _ => base.Call(args),
    };

    public override void Load()
    {
        Instance = this;
    }

    public override void Unload()
    {
        Instance = null;
    }
}
