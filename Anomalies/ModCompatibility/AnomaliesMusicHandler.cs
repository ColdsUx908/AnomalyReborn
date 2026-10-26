using Transoceanic.Runtime.Reflection;
namespace Anomalies.ModCompatibility;

public sealed class AnomaliesMusicHandler : IContentLoader
{
    public static int CurseoftheDungeon { get; private set; } = -1;
    public static int TornfromtheHeavens { get; private set; } = -1;
    public static int PathwaytoReverie { get; private set; } = -1;

    void IContentLoader.PostSetupContent()
    {
        bool isAnomaliesMusicLoaded = ModReference.AnomaliesMusic.IsLoaded;
        Mod anomaliesMusicMod = ModReference.AnomaliesMusic.Mod;

        foreach (PropertyInfo field in typeof(AnomaliesMusicHandler).GetProperties(TOReflectionUtils.StaticBindingFlags))
        {
            if (field.PropertyType == typeof(int))
            {
                int value = -1;
                if (isAnomaliesMusicLoaded)
                {
                    value = MusicLoader.GetMusicSlot(anomaliesMusicMod, "Music/" + field.Name);
                    // GetMusicSlot 在资源不存在时返回 0（静音），而不是 -1。
                    if (value <= 0)
                    {
                        anomaliesMusicMod.Logger.Warn($"Music/{field.Name} was not registered; falling back to vanilla music.");
                        value = -1;
                    }
                }
                field.SetValue(null, value);
            }
        }
    }

    void IContentLoader.OnModUnload()
    {
        foreach (PropertyInfo field in typeof(AnomaliesMusicHandler).GetProperties(TOReflectionUtils.StaticBindingFlags))
        {
            if (field.PropertyType == typeof(int))
            {
                int value = -1;
                field.SetValue(null, value);
            }
        }
    }
}
