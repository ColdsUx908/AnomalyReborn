namespace Anomalies.Bosses.Skeletron.LegendaryNight;

public sealed partial class SkeletronLegendHead
{
    private const float StarAvatarRotationSpeed = 0.08f;

    private Avatar RequestedAvatar => CurrentPhase switch
    {
        _ when Phase2SlimeAvatarActive => Avatar.FlyingSlime,
        _ when Phase2CoinAvatarActive => Avatar.PlatinumCoin,
        Phase.Phase1 when CurrentBehavior == Behavior.Spin && CurrentLocalPhase == 1 => CurrentSpell switch
        {
            Spell.Slime => Avatar.FlyingSlime,
            Spell.Star => Avatar.Star,
            Spell.PlatinumCoin => Avatar.PlatinumCoin,
            _ => Avatar.None
        },
        _ => Avatar.None
    };

    private void UpdateAvatar(Avatar avatar)
    {
        if (CurrentAvatar != avatar)
        {
            if (CurrentPhase != Phase.PhaseChange_1To2)
                SpawnAvatarDisappearanceParticles(CurrentAvatar);
            CurrentAvatar = avatar;
            AvatarTimer = 0;
            NPC.netUpdate = true;
        }

        if (CurrentAvatar != Avatar.None)
            AvatarTimer++;

        switch (CurrentAvatar)
        {
            case Avatar.Star:
                // 化身出现时继承头部朝向，之后沿用头部的自转方向独立累积角度。
                AvatarRotation = MathHelper.WrapAngle(AvatarTimer == 1
                    ? NPC.rotation
                    : AvatarRotation + NPC.direction * StarAvatarRotationSpeed);
                break;
            case Avatar.PlatinumCoin:
                {
                    // 贴图上方为 -Y，补偿 π/2，使上方持续指向目标。
                    Vector2 direction = NPC.GetVelocityTowards(Target);
                    AvatarRotation = direction == Vector2.Zero ? 0f : direction.ToRotation() + MathHelper.PiOver2;
                    break;
                }
            case Avatar.FlyingSlime:
                AvatarRotation = NPC.velocity.X * 0.03f;
                break;
            case Avatar.None:
                AvatarRotation = 0f;
                break;
        }
    }

    private void SpawnAvatarDisappearanceParticles(Avatar avatar)
    {
        if (avatar == Avatar.None || Main.dedServ)
            return;

        Color avatarColor = avatar switch
        {
            Avatar.FlyingSlime => LegendaryNightHandler.SlimeColor,
            Avatar.Star => LegendaryNightHandler.StarColor,
            Avatar.PlatinumCoin => LegendaryNightHandler.PlatinumColor,
            _ => LegendaryNightHandler.BoneColor
        };
        for (int i = 0; i < 120; i++)
        {
            Vector2 offset = Main.rand.NextPolarVector2(20f, 100f * NPC.scale);
            Color color = Color.Lerp(avatarColor, LegendaryNightHandler.BoneColor, Main.rand.NextFloat());
            ParticleHandler.SpawnParticle(new OrbParticle(NPC.Center + offset, offset.ToCustomLength(Main.rand.NextFloat(3f, 14f)),
                Main.rand.Next(25, 50), Main.rand.NextFloat(0.4f, 1.2f), color, lifeEndRatio: 0.2f));
        }
    }

    public override void OnKill()
    {
        UpdateAvatar(Avatar.None);
        if (TONetworkEnvironment.NotClient)
            foreach (Projectile projectile in Projectile.ActiveProjectiles)
                if (projectile.ModProjectile is LegendFinalMeteor && (int)projectile.ai[0] == NPC.whoAmI + 1)
                {
                    projectile.ai[2] = 1f;
                    projectile.netUpdate = true;
                }
    }
}