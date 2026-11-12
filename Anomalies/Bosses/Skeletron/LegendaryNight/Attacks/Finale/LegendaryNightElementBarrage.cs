namespace Anomalies.Bosses.Skeletron.LegendaryNight;

/// <summary>机制七的元素载体补建与发射编排。</summary>
internal static class LegendaryNightElementBarrage
{
    internal static void Update(SkeletronLegendHead head, LegendaryNightPhase2Arena arena)
    {
        if (!head.Mechanic7Active)
            return;
        int age = head.Phase2MusicElapsedTime - SkeletronLegendHead.Mechanic7Timeline.CircleStartMusicTime;
        if (age < 0)
            return;
        if (TONetworkEnvironment.NotClient && head.Phase2MusicElapsedTime < SkeletronLegendHead.Mechanic7Timeline.ElementFadeOutStartMusicTime)
        {
            int existingElements = 0;
            int crownType = ModContent.ProjectileType<LegendCircleCrown>();
            int starCannonType = ModContent.ProjectileType<LegendCircleStarCannon>();
            int coinGunType = ModContent.ProjectileType<LegendCircleCoinGun>();
            foreach (Projectile other in Projectile.ActiveProjectiles)
            {
                int index = (int)other.ai[1];
                if ((int)other.ai[0] != head.NPC.whoAmI + 1 || index is < 2 or >= 8)
                    continue;
                int expectedType = index < 4 ? crownType : index < 6 ? starCannonType : coinGunType;
                if (other.type == expectedType)
                    existingElements |= 1 << index;
            }
            for (int i = 2; i < 8; i++)
            {
                int type = i < 4 ? crownType : i < 6 ? starCannonType : coinGunType;
                if ((existingElements & (1 << i)) == 0)
                    Projectile.NewProjectileAction(arena.Projectile.GetSource_FromAI(), arena.GetMechanic7Circle(i).Center,
                        Vector2.Zero, type, 0, 0f, action: p =>
                        {
                            p.ai[0] = head.NPC.whoAmI + 1;
                            p.ai[1] = i;
                            p.Opacity = 0f;
                        });
            }
        }
        bool fire = SkeletronLegendHead.Mechanic7Parameters.ShouldFireElementWave(head.Phase2MusicElapsedTime);
        if (fire && !Main.dedServ)
        {
            for (int i = 0; i < 8; i++)
            {
                SoundStyle sound = (i / 2) switch
                {
                    0 => SoundID.Item8,
                    1 => SoundID.Item81,
                    2 => SoundID.Item9,
                    _ => PlatinumCoin.UnlimitedTossSound,
                };
                // 同种元素的两个发射点均保留自己的声音实例。
                SoundEngine.PlaySound(sound with { MaxInstances = 0 }, arena.GetMechanic7Circle(i).Center);
            }
        }
        if (TONetworkEnvironment.NotClient && fire)
        {
            for (int i = 0; i < 8; i++)
            {
                Vector2 origin = arena.GetMechanic7Circle(i).Center;
                Vector2 toCenter = arena.Projectile.Center - origin;
                int travelDuration = Math.Max(1, (int)MathF.Ceiling(toCenter.Length()
                    / SkeletronLegendHead.Mechanic7Parameters.ElementProjectileSpeed));
                // 匀速运动的寿命恰好覆盖至场心的行程，最后一步精确落在场心后消失。
                Vector2 velocity = toCenter / travelDuration;
                int type = i < 2 ? ProjectileID.Skull : i < 4 ? ModContent.ProjectileType<SlimeMount>()
                    : i < 6 ? ModContent.ProjectileType<FallingStar>() : ModContent.ProjectileType<PlatinumCoin>();
                int damage = i < 2 ? SkeletronLegendHead.SkullDamage : i < 4 ? SkeletronLegendHead.SlimeMountDamage
                    : i < 6 ? SkeletronLegendHead.FallingStarDamage : SkeletronLegendHead.PlatinumCoinDamage;
                Projectile.NewProjectileAction(arena.Projectile.GetSource_FromAI(), origin, velocity, type, damage, 0f,
                    action: p =>
                    {
                        // 专用标记随原版 AI 同步，仅用于头骨和金币的消失音效判定。
                        if (p.type == ProjectileID.Skull || p.ModProjectile is PlatinumCoin)
                            p.ai[2] = LegendaryNightHandler.Mechanic7ElementProjectileMarker;
                        p.timeLeft = travelDuration;
                        p.tileCollide = false;
                        if (p.type == ProjectileID.Skull)
                            Skull.InitializeFlight(p, Skull.Straight, Skull.MaxSpeed, 1f);
                        else if (p.ModProjectile is SlimeMount mount)
                        {
                            p.ai[1] = 2f; // 直线匀速。
                            p.rotation = velocity.ToRotation() - MathHelper.PiOver2;
                            mount.MasterIndex = head.NPC.whoAmI + 1;
                        }
                        else if (p.ModProjectile is FallingStar star)
                        {
                            p.scale = 0.2f;
                            p.Resize(16, 16);
                            star.TrailType = 2;
                            star.TrailColorOverride = Color.Orange;
                            star.AccelerationStartTime = 600;
                        }
                        else if (p.ModProjectile is PlatinumCoin)
                        {
                            p.scale = 1.5f;
                            p.Resize(18, 18);
                        }
                    });
            }
        }
        if (Main.dedServ)
            return;
        for (int i = 0; i < 8; i++)
        {
            if (head.Phase2MusicElapsedTime != SkeletronLegendHead.Mechanic7Parameters.GetElementStartMusicTime(i)
                && head.Phase2MusicElapsedTime != SkeletronLegendHead.Mechanic7Timeline.ElementFadeOutStartMusicTime)
                continue;
            Vector2 center = arena.GetMechanic7Circle(i).Center;
            Color color = SkeletronLegendHead.Mechanic7Parameters.ElementColor(i);
            for (int j = 0; j < 48; j++)
            {
                Vector2 velocity = Main.rand.NextPolarVector2(2f, 12f);
                ParticleHandler.SpawnParticle(new OrbParticle(center, velocity, Main.rand.Next(30, 65),
                    Main.rand.NextFloat(0.4f, 1f), Color.Lerp(color, Color.White, Main.rand.NextFloat(0.4f))));
                if (j % 2 == 0)
                    ParticleHandler.SpawnParticle(new GenericSparkle(center, velocity * 0.7f, color,
                        color * 0.35f, Main.rand.NextFloat(0.2f, 0.5f), Main.rand.Next(30, 60),
                        rotationSpeed: 0.02f, bloomScale: 0.8f));
            }
        }
    }
}