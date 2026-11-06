using static Anomalies.Bosses.Skeletron.LegendaryNight.SkeletronLegendHead.Mechanic6Parameters;
using static Anomalies.Bosses.Skeletron.LegendaryNight.SkeletronLegendHead.Mechanic6Timeline;

namespace Anomalies.Bosses.Skeletron.LegendaryNight;

public sealed partial class LegendEnergySphere
{
    public override void AI()
    {
        SkeletronLegendHead head = Head;
        if (head is not { StellarMechanicsActive: true } || head.CurrentBehavior == SkeletronLegendHead.Behavior.Despawn)
        {
            if (TONetworkEnvironment.NotClient)
                Projectile.Kill();
            return;
        }
        int time = head.Phase2MusicElapsedTime;
        Projectile.timeLeft = 100;
        Projectile.Center = head.Phase2Arena is LegendaryNightPhase2Arena arena ? arena.Projectile.Center
            : Master.Center + SphereOffset(time);
        Projectile.velocity = Vector2.Zero;
        ResetInactivePlayerBonuses();
        if (TONetworkEnvironment.NotClient && ShouldSpawnLightOrb(time) && head.Phase2Arena is LegendaryNightPhase2Arena orbArena)
        {
            SpawnLightOrb(orbArena);
        }
        if (TONetworkEnvironment.NotClient)
        {
            if (head.Mechanic7Active && SkeletronLegendHead.Mechanic7Parameters.ShouldSpawnLaser(time))
                SpawnLasers(SkeletronLegendHead.Mechanic7Parameters.LaserCount);
            else if (head.Mechanic6Active && ShouldSpawnLaser(time))
                SpawnLasers(LaserCounts[(time - LaserFirstMusicTime) / LaserInterval]);
        }
        EnsureCompanion(head, time);
        if (Main.dedServ)
            return;
        _LightOrbTrails?.Update();
        Lighting.AddLight(Projectile.Center, LegendaryNightHandler.StarColor.ToVector3() * 1.5f);
        EnsureChargeSound();
        SpawnAbsorptionParticles();
        void SpawnLightOrb(LegendaryNightPhase2Arena arena)
        {
            int index = (time - LightOrbFirstMusicTime) / LightOrbInterval;
            if (index < _LightOrbsSpawned)
                return;
            if (index == 0)
                TOLocalizationUtils.ChatLocalizedText(AnomalySharedData.ModLocalizationPrefix
                    + LegendaryNightHandler.LocalizationCategory + ".InterceptStarPower", LegendaryNightHandler.StarColor);
            float angle = Main.rand.NextFloat(MathHelper.TwoPi);
            int color = index % 2;
            int lifetime = Main.rand.Next(GetLightOrbLifetime(index, 0f), GetLightOrbLifetime(index, 1f) + 1);
            Projectile.NewProjectileAction<LegendLightOrb>(SourceAI,
                arena.Projectile.Center + angle.ToRotationVector2() * arena.Radius, Vector2.Zero, 0, 0f,
                action: p =>
                {
                    p.ai[0] = color;
                    p.ai[1] = time;
                    p.ai[2] = angle;
                    LegendLightOrb orb = p.GetModProjectile<LegendLightOrb>();
                    orb.SphereProjectile = Projectile;
                    orb.SpawnRadius = arena.Radius;
                    orb.TravelDuration = lifetime;
                    p.scale = 0f;
                    p.Opacity = 0f;
                    p.netUpdate = true;
                });
            _LightOrbsSpawned = index + 1;
            Projectile.netUpdate = true;
        }

        void ResetInactivePlayerBonuses()
        {
            if (TONetworkEnvironment.NotClient)
            {
                for (int i = 0; i < Main.maxPlayers; i++)
                {
                    if (Main.player[i].active || (_PlayerBonuses[i].DamageStacks == 0
                        && _PlayerBonuses[i].RegenerationStacks == 0 && !_PlayerBonuses[i].ExpectsPlatinum))
                        continue;
                    _PlayerBonuses[i] = default;
                    Projectile.netUpdate = true;
                }
            }
        }

        void EnsureChargeSound()
        {
            if (!_SoundStarted && SkeletronLegendHead.Mechanic7Parameters.StarDissipation(time) < 1f)
            {
                _SoundStarted = true;
                // 自行按本机玩家距离衰减，避免引擎按镜头距离再次降低近处音量。
                _ChargeSoundSlot = SoundEngine.PlaySound(_ChargeSound, updateCallback: sound =>
                {
                    if (!Projectile.active || Head is not SkeletronLegendHead { StellarMechanicsActive: true } soundHead
                        || soundHead.CurrentBehavior == SkeletronLegendHead.Behavior.Despawn
                        || SkeletronLegendHead.Mechanic7Parameters.StarDissipation(soundHead.Phase2MusicElapsedTime) >= 1f)
                        return false;
                    float distance = Vector2.Distance(Main.LocalPlayer.Center, Projectile.Center);
                    float attenuation = Utils.GetLerpValue(ChargeSoundFadeOutDistance, ChargeSoundFullVolumeDistance, distance, true);
                    sound.Volume = (0.15f + 0.5f * ChargeProgress(soundHead.Phase2MusicElapsedTime)) * attenuation;
                    sound.Sound.Pan = MathHelper.Clamp((Projectile.Center.X - Main.Camera.Center.X) / (Main.screenWidth * 0.5f), -1f, 1f);
                    return true;
                });
            }
        }

        void SpawnAbsorptionParticles()
        {
            const int AbsorptionParticleCount = 10;
            // 与橙星的285帧消散共用时钟，平滑降低生成密度；完全消散后不再生成。
            float absorptionSpawnChance = 1f - MathHelper.SmoothStep(0f, 1f,
                SkeletronLegendHead.Mechanic7Parameters.StarDissipation(time));
            for (int i = 0; i < AbsorptionParticleCount && absorptionSpawnChance > 0f; i++)
            {
                if (absorptionSpawnChance < 1f && Main.rand.NextFloat() >= absorptionSpawnChance)
                    continue;
                // 起点覆盖整圈并位于球面之外，沿直线快速汇聚为星光能量。
                float angle = MathHelper.TwoPi * (i + Main.rand.NextFloat()) / AbsorptionParticleCount;
                float distance = SphereRadius + Main.rand.NextFloat(100f, 260f);
                Vector2 origin = Projectile.Center + angle.ToRotationVector2() * distance;
                ParticleHandler.SpawnParticle(new LegendaryNightStellarMatterParticle(Projectile, origin,
                    Main.rand.NextFloat(0.3f, 0.6f), Main.rand.Next(12, 21)));
            }
        }
    }

    private void EnsureCompanion(SkeletronLegendHead head, int time)
    {
        if (!TONetworkEnvironment.NotClient || time < SkeletronLegendHead.Mechanic6Timeline.CompanionStartMusicTime
            || time >= SkeletronLegendHead.Mechanic6Timeline.CompanionImpactMusicTime)
            return;
        Projectile companion = _CompanionReference.Resolve();
        if (companion?.ModProjectile is LegendBlueStar cached && cached.PrimaryProjectile == Projectile)
            return;
        foreach (Projectile other in Projectile.ActiveProjectiles)
            if (other.ModProjectile is LegendBlueStar blue && blue.PrimaryProjectile == Projectile)
            {
                _CompanionReference.Set(other);
                return;
            }
        Projectile.NewProjectileAction<LegendBlueStar>(SourceAI, Projectile.Center, Vector2.Zero, 0, 0f,
            action: p =>
            {
                p.ai[0] = head.NPC.whoAmI + 1;
                p.GetModProjectile<LegendBlueStar>().PrimaryProjectile = Projectile;
                _CompanionReference.Set(p);
                p.netUpdate = true;
            });
    }
}