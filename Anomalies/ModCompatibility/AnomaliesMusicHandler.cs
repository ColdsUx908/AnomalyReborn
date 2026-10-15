namespace Anomalies.ModCompatibility;

public sealed class AnomaliesMusicHandler : IContentLoader
{
    public static int CurseoftheDungeon { get; private set; }
    public static int TornfromtheHeavens { get; private set; }

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
                    value = MusicLoader.GetMusicSlot(anomaliesMusicMod, "Music/" + field.Name);
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
