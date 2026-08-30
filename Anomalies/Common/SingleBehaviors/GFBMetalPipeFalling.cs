namespace Anomalies.Common.SingleBehaviors;

public sealed class GFBMetalPipeFalling : AnomalyPlayerBehavior
{
    public override bool PreKill(double damage, int hitDirection, bool pvp, ref bool playSound, ref bool genDust, ref PlayerDeathReason damageSource)
    {
        if (Main.zenithWorld && AnomalySharedData.Anomaly)
        {
            playSound = false;
            SoundEngine.PlaySound(AnomalySounds.MetalPipeFalling);
        }

        return true;
    }
}

