namespace Anomalies.SpecialContents.Developer.EventideReunion;

public sealed class EventideReunionSamsaraPlayer : AnomalyPlayerBehavior
{
    public override void PostUpdate()
    {
        if (AnomalyPlayer.EventideReunion_SamsaraCooldown > 0)
            AnomalyPlayer.EventideReunion_SamsaraCooldown--;
        if (Player.ownedProjectileCounts[ModContent.ProjectileType<EventideReunionSamsaraSkill>()] == 0)
            return;
        foreach (Projectile projectile in Main.ActiveProjectiles)
        {
            if (projectile.owner == Player.whoAmI && projectile.ModProjectile is EventideReunionSamsaraSkill { IsCharging: true })
            {
                Player.eyeHelper.CurrentEyeFrame = PlayerEyeHelper.EyeFrame.EyeClosed;
                break;
            }
        }
    }

    public override void SetControls()
    {
        if (Player.ownedProjectileCounts[ModContent.ProjectileType<EventideReunionSamsaraSkill>()] > 0)
            Player.controlUseItem = Player.controlUseTile = false;
    }
}
