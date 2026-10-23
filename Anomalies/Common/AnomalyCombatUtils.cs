namespace Anomalies.Common;

public static class AnomalyCombatUtils
{
    public static void AddScreenshakeAt(Vector2 position, float power)
    {
        if (!Main.dedServ)
            Main.LocalPlayer.SetScreenshake(power / (1f + Main.LocalPlayer.Distance(position) / 1200f));
    }

}
