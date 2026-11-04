namespace Anomalies.Bosses.EaterofWorlds;

internal sealed class EaterPortalAssets : ModSystem
{
    internal const string Path = EaterofWorldsHead.AnomalyEaterofWorldsPath + "Portal/";
    internal static readonly Color Twilight = new(147, 24, 204);
    internal static readonly Color LightBlue = new(0, 221, 250);
    private static readonly Dictionary<string, Asset<Texture2D>> _Textures = [];

    public override void Load()
    {
        if (Main.dedServ)
            return;
        foreach (string name in new[] { "CrackedGlass_Glowing", "PlasmaExplosion",
            "RiftStar", "ShineExplosion1", "ShineExplosion2" })
            _Textures.Add(Path + name, Mod.Assets.Request<Texture2D>("Bosses/EaterofWorlds/Portal/" + name));
    }

    internal static Asset<Texture2D> Get(string path) => _Textures[path];
    public override void Unload() => _Textures.Clear();
}
