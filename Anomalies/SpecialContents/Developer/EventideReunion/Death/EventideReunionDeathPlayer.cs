namespace Anomalies.SpecialContents.Developer.EventideReunion;

public sealed class EventideReunionDeathPlayer : AnomalyPlayerBehavior
{
    public override bool ImmuneTo(PlayerDeathReason damageSource, int cooldownCounter, bool dodgeable) =>
        AnomalyPlayer.EventideReunionDeathProtection > 0 || IsDashing();

    private bool IsDashing()
    {
        foreach (Projectile projectile in Main.ActiveProjectiles)
            if (projectile.owner == Player.whoAmI && projectile.ModProjectile is EventideReunionDeathSkill { IsDashing: true })
                return true;
        return false;
    }

    public override void PostUpdate()
    {
        if (AnomalyPlayer.EventideReunionDeathCooldown > 0)
            AnomalyPlayer.EventideReunionDeathCooldown--;
        if (AnomalyPlayer.EventideReunionDeathProtection > 0)
            AnomalyPlayer.EventideReunionDeathProtection--;
    }
    public override void SetControls()
    {
        if (Player.ownedProjectileCounts[ModContent.ProjectileType<EventideReunionDeathSkill>()] == 0)
            return;
        Player.controlUseItem = Player.controlUseTile = false;
        foreach (Projectile projectile in Main.ActiveProjectiles)
        {
            if (projectile.owner != Player.whoAmI || projectile.ModProjectile is not EventideReunionDeathSkill { IsDashing: true })
                continue;
            Player.controlLeft = Player.controlRight = Player.controlUp = Player.controlDown = false;
            Player.controlJump = Player.controlHook = Player.controlMount = false;
            break;
        }
    }
    public override void PreUpdateMovement()
    {
        foreach (Projectile projectile in Main.ActiveProjectiles)
        {
            if (projectile.owner != Player.whoAmI || projectile.ModProjectile is not EventideReunionDeathSkill { IsDashing: true })
                continue;
            Player.velocity = Vector2.Zero;
            Player.gravity = 0f;
            Player.noFallDmg = true;
            Player.RemoveAllGrapplingHooks();
            if (Player.mount.Active)
                Player.mount.Dismount(Player);
            break;
        }
    }
}
