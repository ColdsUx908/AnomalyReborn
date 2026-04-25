namespace CalamityAnomalies.Common.SingleBehaviors;

public sealed class GFBMetalPipeFalling : CAPlayerBehavior
{
    public override bool PreKill(double damage, int hitDirection, bool pvp, ref bool playSound, ref bool genDust, ref PlayerDeathReason damageSource)
    {
        if (Main.zenithWorld && CASharedData.Anomaly)
        {
            playSound = false;
            SoundEngine.PlaySound(CASounds.MetalPipeFalling);
        }

        return true;
    }
}
