namespace Anomalies.Bosses.Skeletron.LegendaryNight;

public sealed partial class SkeletronLegendHead
{
    public override bool PreAI()
    {
        using BossNetUpdateScope netUpdate = new(NPC);

        int previousTarget = NPC.target;
        bool validTarget;
        if (Phase2ArenaProjectileAlive)
            validTarget = NPC.TargetIfInvalid(true, p => new Circle(ArenaProjectile.Center, Phase2Arena.TargetingRadius).Collides(p.Hitbox));
        else if (ArenaProjectileAlive && ArenaModProjectile.IsActivated)
            validTarget = NPC.TargetIfInvalid(true, p => new Circle(ArenaProjectile.Center, ArenaModProjectile.Radius * 2f).Collides(p.Hitbox));
        else
        {
            // 无竞技场时保持有效目标；重新选取后重新读取目标，避免用旧玩家距离判定退场。
            if (!NPC.HasValidTarget || !NPC.WithinRange(NPC.PlayerTarget.Center, DespawnDistance - NPC.PlayerTarget.aggro))
                NPC.TargetClosest(true);
            validTarget = NPC.HasValidTarget && NPC.WithinRange(NPC.PlayerTarget.Center, DespawnDistance - NPC.PlayerTarget.aggro);
        }
        if (NPC.target != previousTarget && TONetworkEnvironment.NotClient)
            NPC.netUpdate = true;

        if (CurrentBaseBehavior == BaseBehavior.Despawn || !validTarget)
        {
            UpdateAvatar(Avatar.None);
            NPC.active = false;
            NPC.netUpdate = true;
            return false;
        }

        // 目标判定读取上帧激活状态，再由本帧符卡重新激活。
        if (ArenaProjectileAlive)
            ArenaModProjectile.IsActivated = false;

        if (InvalidPhase1)
            BeginPhaseChange_1To2();

        if (Phase2MusicActive)
            Timer5++;

        AnomalyNPC.BossBar_Immune = CurrentPhase == Phase.PhaseChange_1To2;

        switch (CurrentPhase)
        {
            case Phase.Initialize:
                Timer1++;

                if (Timer1 >= 60)
                {
                    Timer1 = 0;
                    CurrentPhase = Phase.Phase1;
                }
                break;
            case Phase.Phase1:
                Phase1AI();
                break;
            case Phase.PhaseChange_1To2:
                PhaseChange_1To2AI();
                break;
            case >= Phase.Phase2_1:
                Phase2AI();
                break;
        }

        if (!Main.dedServ && Phase2GlowOpacity > 0f)
            Lighting.AddLight(NPC.Center, LegendaryNightHandler.StarColor.ToVector3() * Phase2GlowOpacity);

        UpdateAvatar(CurrentPhase == Phase.PhaseChange_1To2 && Timer1 < PhaseChangeAvatarFadeDuration ? CurrentAvatar : RequestedAvatar);
        if (CurrentAvatar == Avatar.None)
            NPC.Opacity = 1f;
        if (CurrentAvatar != Avatar.None)
        {
            Color lightColor = CurrentAvatar switch
            {
                Avatar.FlyingSlime => LegendaryNightHandler.SlimeColor,
                Avatar.Star => LegendaryNightHandler.StarColor,
                Avatar.PlatinumCoin => LegendaryNightHandler.PlatinumColor,
                _ => LegendaryNightHandler.BoneColor
            };
            Lighting.AddLight(NPC.Center, lightColor.ToVector3());
        }
        return false;
    }

    private void ReleaseSpellShockwave(Spell? spell = null)
    {
        if (!TONetworkEnvironment.NotClient)
            return;

        Projectile.NewProjectileAction<LegendShockwave>(SourceAI, NPC.Center, Vector2.Zero, 0, 0f, action: p =>
        {
            p.scale = 0f;
            p.ai[0] = (spell ?? CurrentSpell) switch
            {
                Spell.Slime => 1f,
                Spell.Star => 2f,
                Spell.PlatinumCoin => 3f,
                _ => 0f
            };
            p.netUpdate = true;
        });
    }

    private void SummonStarMeteors(int timer, bool reducedStarCount = false)
    {
        const int AttackInterval = 120;
        const int MeteorCount = 3;
        const float MeteorSpeed = 20f;
        const int SummonSoundLeadTime = 10;

        int summonTimer = timer + SummonSoundLeadTime;
        if (!Main.dedServ && summonTimer >= AttackInterval && summonTimer <= AttackInterval * MeteorCount && summonTimer % AttackInterval == 0)
            SoundEngine.PlaySound(Meteor.SummonSound, Target.Center);

        if (timer >= AttackInterval && timer <= AttackInterval * MeteorCount && timer % AttackInterval == 0)
        {
            if (TONetworkEnvironment.NotClient)
            {
                Vector2 spawnPosition = Target.Center + new Vector2(Main.rand.NextFloat(-500f, 500f), -1000f);
                Vector2 velocity = Target.GetVelocityTowards(spawnPosition, -MeteorSpeed);
                Projectile.NewProjectileAction<Meteor>(SourceAI, spawnPosition, velocity, MeteorDamage, 0f, action: p =>
                {
                    p.ai[0] = NPC.target + 1;
                    p.ai[2] = NPC.whoAmI + 1;
                    p.GetModProjectile<Meteor>().ReducedAttack = reducedStarCount;
                });
            }
        }

    }

    private void SummonRotatingSkullWarnings(int timer)
    {
        if (TONetworkEnvironment.NotClient && timer % 75 == 0 && timer <= 300)
        {
            const int SkullCount = 4;
            float offset = Main.rand.NextFloat(MathHelper.TwoPi);
            for (int i = 0; i < SkullCount; i++)
            {
                Vector2 position = Target.Center + new Vector2(Main.rand.NextFloat(800f, 1000f), 0f)
                    .RotatedBy(offset + MathHelper.TwoPi * i / SkullCount);
                Projectile.NewProjectileAction<SkullWarning>(SourceAI, position, Vector2.Zero, SkullDamage, 0f, action: p =>
                {
                    p.ai[0] = NPC.target + 1;
                    p.ai[2] = NPC.whoAmI + 1;
                    p.netUpdate = true;
                });
            }
        }

    }
}
