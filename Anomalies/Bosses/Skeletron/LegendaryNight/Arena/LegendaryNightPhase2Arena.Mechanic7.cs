namespace Anomalies.Bosses.Skeletron.LegendaryNight;

public sealed partial class LegendaryNightPhase2Arena
{
    // 由音乐绝对时钟推导，客户端和中途加入无需同步独立角度。
    public (Vector2 Center, float Radius, float Rotation) GetMechanic7Circle(int index)
    {
        float age = SkeletronLegendHead.Mechanic7Data.RollingTime(MasterBehavior?.Phase2MusicElapsedTime ?? 0);
        float radius = SkeletronLegendHead.Mechanic7Data.CircleRadius;
        float direction = index % 2 == 0 ? 1f : -1f;
        float travel = age * SkeletronLegendHead.Mechanic7Data.RollingSpeeds[index] * direction;
        float angle = -MathHelper.PiOver2 + index * MathHelper.TwoPi / 8f + travel;
        // 外切无滑动滚动：中心走过的弧长等于小圆自转的弧长。
        return (Projectile.Center + angle.ToRotationVector2() * (Radius + radius),
            radius, travel * (Radius + radius) / radius);
    }

    private void UpdateMechanic7Circles(SkeletronLegendHead head)
    {
        if (!head.Mechanic7Active)
            return;
        int age = head.Phase2MusicElapsedTime - SkeletronLegendHead.Mechanic7Data.CircleStartMusicTime;
        if (age < 0)
            return;
        if (TONetworkEnvironment.NotClient && head.Phase2MusicElapsedTime < SkeletronLegendHead.Mechanic7Data.T9MusicTime)
        {
            for (int i = 2; i < 8; i++)
            {
                int type = i < 4 ? ModContent.ProjectileType<LegendCircleCrown>()
                    : i < 6 ? ModContent.ProjectileType<LegendCircleStarCannon>()
                    : ModContent.ProjectileType<LegendCircleCoinGun>();
                bool exists = false;
                foreach (Projectile other in Projectile.ActiveProjectiles)
                    if (other.type == type && (int)other.ai[0] == head.NPC.whoAmI + 1 && (int)other.ai[1] == i)
                    {
                        exists = true;
                        break;
                    }
                if (!exists)
                    Projectile.NewProjectileAction(Projectile.GetSource_FromAI(), GetMechanic7Circle(i).Center,
                        Vector2.Zero, type, 0, 0f, action: p =>
                        {
                            p.ai[0] = head.NPC.whoAmI + 1;
                            p.ai[1] = i;
                            p.Opacity = 0f;
                        });
            }
        }
        bool fire = SkeletronLegendHead.Mechanic7Data.ShouldFire(head.Phase2MusicElapsedTime);
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
                SoundEngine.PlaySound(sound with { MaxInstances = 0 }, GetMechanic7Circle(i).Center);
            }
        }
        if (TONetworkEnvironment.NotClient && fire)
        {
            for (int i = 0; i < 8; i++)
            {
                Vector2 origin = GetMechanic7Circle(i).Center;
                Vector2 toCenter = Projectile.Center - origin;
                int travelDuration = Math.Max(1, (int)MathF.Ceiling(toCenter.Length()
                    / SkeletronLegendHead.Mechanic7Data.ElementProjectileSpeed));
                // 匀速运动的寿命恰好覆盖至场心的行程，最后一步精确落在场心后消失。
                Vector2 velocity = toCenter / travelDuration;
                int type = i < 2 ? ProjectileID.Skull : i < 4 ? ModContent.ProjectileType<SlimeMount>()
                    : i < 6 ? ModContent.ProjectileType<FallingStar>() : ModContent.ProjectileType<PlatinumCoin>();
                int damage = i < 2 ? SkeletronLegendHead.SkullDamage : i < 4 ? SkeletronLegendHead.SlimeMountDamage
                    : i < 6 ? SkeletronLegendHead.FallingStarDamage : SkeletronLegendHead.PlatinumCoinDamage;
                Projectile.NewProjectileAction(Projectile.GetSource_FromAI(), origin, velocity, type, damage, 0f,
                    action: p =>
                    {
                        // 专用标记随原版 AI 同步，仅用于头骨和金币的消失音效判定。
                        if (p.type == ProjectileID.Skull || p.ModProjectile is PlatinumCoin)
                            p.ai[2] = LegendaryNightHandler.Mechanic7ElementProjectileMarker;
                        p.timeLeft = travelDuration;
                        p.tileCollide = false;
                        if (p.type == ProjectileID.Skull)
                            p.ai[0] = 1f; // 跳过追踪和加速。
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
            if (head.Phase2MusicElapsedTime != SkeletronLegendHead.Mechanic7Data.GetElementStartMusicTime(i)
                && head.Phase2MusicElapsedTime != SkeletronLegendHead.Mechanic7Data.T9MusicTime)
                continue;
            Vector2 center = GetMechanic7Circle(i).Center;
            Color color = SkeletronLegendHead.Mechanic7Data.ElementColor(i);
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
