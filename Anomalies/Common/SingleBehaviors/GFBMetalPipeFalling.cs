namespace Anomalies.Common.SingleBehaviors;

public sealed class GFBMetalPipeFalling : AnomalyPlayerBehavior
{
    public override bool PreKill(double damage, int hitDirection, bool pvp, ref bool playSound, ref bool genDust, ref PlayerDeathReason damageSource)
    {
        if (Aroma)
        {
            playSound = false;
            SoundEngine.PlaySound(AnomalySounds.MetalPipeFalling);
        }

        return true;
    }
}

