namespace Anomalies.SpecialContents.Developer.EventideReunion;

public sealed class EventideReunionSunPlayer : AnomalyPlayerBehavior
{
    public override void ResetEffects()
    {
        if (AnomalyPlayer.EventideReunion_SunFallProtection > 0
            || Player.ownedProjectileCounts[ModContent.ProjectileType<EventideReunionSunSkill>()] > 0)
            Player.noFallDmg = true;
    }

    public override void SetControls()
    {
        if (Player.ownedProjectileCounts[ModContent.ProjectileType<EventideReunionSunSkill>()] == 0)
            return;
        Player.controlUseItem = Player.controlUseTile = false;
        foreach (Projectile activeProjectile in Main.ActiveProjectiles)
        {
            if (activeProjectile.owner != Player.whoAmI || activeProjectile.ModProjectile is not EventideReunionSunSkill { IsDashing: true })
                continue;
            Player.controlLeft = Player.controlRight = Player.controlUp = Player.controlDown = false;
            Player.controlJump = Player.controlHook = Player.controlMount = false;
            break;
        }
    }

    public override void PreUpdateMovement()
    {
        if (Player.ownedProjectileCounts[ModContent.ProjectileType<EventideReunionSunSkill>()] == 0)
            return;
        foreach (Projectile activeProjectile in Main.ActiveProjectiles)
        {
            if (activeProjectile.owner != Player.whoAmI || activeProjectile.ModProjectile is not EventideReunionSunSkill { IsDashing: true })
                continue;
            // 玩家物理先静止，随后由技能弹幕按锁定向量推进，避免一次普通移动加一次冲刺移动。
            Player.velocity = Vector2.Zero;
            Player.gravity = 0f;
            Player.ignoreWater = true;
            Player.dashDelay = -1;
            Player.RemoveAllGrapplingHooks();
            if (Player.mount.Active)
                Player.mount.Dismount(Player);
            break;
        }
    }

    public override void PostUpdate()
    {
        if (AnomalyPlayer.EventideReunion_SunCooldown > 0)
            AnomalyPlayer.EventideReunion_SunCooldown--;
        if (AnomalyPlayer.EventideReunion_SunFallProtection > 0)
        {
            AnomalyPlayer.EventideReunion_SunFallProtection--;
            Player.noFallDmg = true;
        }
    }
}
