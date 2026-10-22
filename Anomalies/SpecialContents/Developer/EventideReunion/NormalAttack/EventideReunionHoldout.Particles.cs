namespace Anomalies.SpecialContents.Developer.EventideReunion;

public sealed partial class EventideReunionHoldout
{
    private void UpdateThrustParticles()
    {
        const float ReleaseStart = 0.04f;
        const float ReleaseEnd = 0.36f;
        const int EmissionSlices = 12;
        if (Main.dedServ || Progress <= ReleaseStart || _PreviousProgress >= ReleaseEnd)
            return;

        float start = MathHelper.Clamp((_PreviousProgress - ReleaseStart) / (ReleaseEnd - ReleaseStart), 0f, 1f);
        float end = MathHelper.Clamp((Progress - ReleaseStart) / (ReleaseEnd - ReleaseStart), 0f, 1f);
        int first = (int)MathF.Floor(start * EmissionSlices);
        int last = (int)MathF.Floor(end * EmissionSlices);
        for (int slice = first; slice < last; slice++)
        {
            float sample = MathHelper.Lerp(ReleaseStart, ReleaseEnd, (slice + 0.5f) / EmissionSlices);
            Vector2 tip = GetThrustTip(sample);
            Vector2 velocity = Projectile.velocity.RotatedBy(Main.rand.NextFloat(-0.22f, 0.22f))
                * Main.rand.NextFloat(3f, Phase == 2 ? 8f : 6f);
            Color orange = Color.Lerp(new Color(255, 95, 20), new Color(255, 205, 110), Main.rand.NextFloat());
            ParticleHandler.SpawnParticle(new PointingParticle(tip, velocity, false, Main.rand.Next(10, 17),
                Main.rand.NextFloat(0.3f, 0.55f) * Projectile.scale, slice % 3 == 0 ? new Color(255, 245, 225) : orange));
            if (slice % 3 == 0)
            {
                ParticleHandler.SpawnParticle(new GenericSparkle(tip, velocity * 0.25f, new Color(255, 250, 230),
                    orange, Main.rand.NextFloat(0.25f, 0.45f) * Projectile.scale, 16,
                    rotationSpeed: 0.025f, bloomScale: 1.2f));
            }
            Dust.NewDustPerfectAction(tip, DustID.RainbowMk2, dust =>
            {
                dust.noGravity = true;
                dust.color = orange;
                dust.scale = Main.rand.NextFloat(0.4f, 0.65f);
                dust.velocity = velocity * 0.6f;
            });
        }

        if (Progress >= ReleaseEnd)
            EmitTipBurst();

        void EmitTipBurst()
        {
            Vector2 tip = GetThrustTip(ReleaseEnd);
            for (int i = 0; i < (Phase == 2 ? 10 : 6); i++)
            {
                Vector2 velocity = Projectile.velocity.RotatedBy(Main.rand.NextFloat(-0.65f, 0.65f))
                    * Main.rand.NextFloat(3.5f, 8f);
                ParticleHandler.SpawnParticle(new PointingParticle(tip, velocity, false, Main.rand.Next(12, 20),
                    Main.rand.NextFloat(0.35f, 0.65f) * Projectile.scale,
                    i % 3 == 0 ? new Color(255, 250, 230) : new Color(255, 145, 40)));
            }
            ParticleHandler.SpawnParticle(new GenericSparkle(tip, Projectile.velocity * 0.6f,
                new Color(255, 253, 240), new Color(255, 135, 30), (Phase == 2 ? 0.85f : 0.65f) * Projectile.scale,
                18, rotationSpeed: 0.01f, bloomScale: 1.5f));
            ParticleHandler.SpawnParticle(new BloomParticle(tip, Vector2.Zero, new Color(255, 145, 45) * 0.6f,
                0.06f * Projectile.scale, 0.25f * Projectile.scale, 10, lifeEndRatio: 0.1f));
        }
    }

    private void UpdateSweepSparks()
    {
        if (Main.dedServ || Progress <= ArcStart || _PreviousProgress >= ArcEnd)
            return;

        // 参考 MEAC 永夜刃：沿刀光铺开光团，并沿切线甩出、减速消散。
        // 固定进度分段，高攻速跨过的分段也补齐，每套上挑的粒子数量保持一致。
        int emissionSlices = Phase == 4 ? 36 : 24;
        float start = MathHelper.Clamp((_PreviousProgress - ArcStart) / (ArcEnd - ArcStart), 0f, 1f);
        float end = MathHelper.Clamp((Progress - ArcStart) / (ArcEnd - ArcStart), 0f, 1f);
        int firstSlice = (int)MathF.Floor(start * emissionSlices);
        int lastSlice = (int)MathF.Floor(end * emissionSlices);

        if (_PreviousProgress <= ArcStart)
            EmitBurst(ArcStart, false);

        for (int slice = firstSlice; slice < lastSlice; slice++)
        {
            float sample = MathHelper.Lerp(ArcStart, ArcEnd, (slice + 0.5f) / emissionSlices);
            EmitTrailSample(sample, slice);
        }

        if (Progress >= ArcEnd)
            EmitBurst(ArcEnd, true);

        void EmitTrailSample(float sample, int slice)
        {
            Vector2 tip = GetArcTip(sample);
            Vector2 tangent = GetTangent(sample);
            Vector2 outward = (tip - _Grip).SafeNormalize(Projectile.velocity);
            Vector2 position = Vector2.Lerp(_Grip, tip, Main.rand.NextFloat(0.48f, 1f));
            Vector2 velocity = tangent.RotatedBy(Main.rand.NextFloat(-0.45f, 0.45f)) * Main.rand.NextFloat(2.5f, 6f)
                + outward * Main.rand.NextFloat(0.8f, 2.8f) + Projectile.Owner.velocity * 0.35f;
            Color orange = Color.Lerp(new Color(255, 85, 15), new Color(255, 195, 85), Main.rand.NextFloat());

            // 大小错落的光团和细长火花，避免只剩均匀的小 Dust。
            ParticleHandler.SpawnParticle(new OrbParticle(position, velocity * 0.65f, Main.rand.Next(18, 29),
                Main.rand.NextFloat(0.11f, 0.24f) * Projectile.scale, orange, lifeEndRatio: 0.1f));
            ParticleHandler.SpawnParticle(new PointingParticle(position, velocity * 1.5f, false, Main.rand.Next(12, 21),
                Main.rand.NextFloat(0.35f, 0.65f) * Projectile.scale, slice % 3 == 0 ? new Color(255, 245, 225) : orange));

            if (slice % 2 == 0)
            {
                Vector2 starPosition = Vector2.Lerp(_Grip, tip, Main.rand.NextFloat(0.55f, 1.02f));
                ParticleHandler.SpawnParticle(new GenericSparkle(starPosition, velocity * 0.35f,
                    new Color(255, 248, 230), orange, Main.rand.NextFloat(0.25f, 0.6f) * Projectile.scale,
                    Main.rand.Next(18, 31), rotationSpeed: 0.025f, bloomScale: 1.4f));
            }

            Dust.NewDustPerfectAction(tip, DustID.RainbowMk2, dust =>
            {
                dust.noGravity = true;
                dust.color = orange;
                dust.scale = Main.rand.NextFloat(0.45f, 0.75f);
                dust.velocity = velocity * 0.7f;
                dust.fadeIn = 0f;
            });
        }

        void EmitBurst(float sample, bool finishing)
        {
            Vector2 tip = GetArcTip(sample);
            Vector2 tangent = GetTangent(sample);
            Vector2 outward = (tip - _Grip).SafeNormalize(Projectile.velocity);
            int count = finishing ? (Phase == 4 ? 24 : 14) : 10;
            for (int i = 0; i < count; i++)
            {
                Vector2 direction = tangent.RotatedBy(Main.rand.NextFloat(-1.15f, 1.15f));
                Vector2 velocity = direction * Main.rand.NextFloat(3f, finishing ? 9f : 6f) + outward * 1.5f;
                Vector2 position = tip + Main.rand.NextVector2Circular(5f, 5f) * Projectile.scale;
                Color drawTint = i % 3 == 0 ? new Color(255, 245, 220) : new Color(255, 135, 35);
                ParticleHandler.SpawnParticle(new PointingParticle(position, velocity, false, Main.rand.Next(14, 24),
                    Main.rand.NextFloat(0.4f, 0.8f) * Projectile.scale, drawTint));
                if (i % 2 == 0)
                {
                    ParticleHandler.SpawnParticle(new GenericSparkle(position, velocity * 0.4f,
                        new Color(255, 250, 235), new Color(255, 125, 25), Main.rand.NextFloat(0.25f, 0.5f) * Projectile.scale,
                        Main.rand.Next(20, 33), rotationSpeed: 0.035f, bloomScale: 1.3f));
                }
            }

            // 白色十字星芯与橙色短闪光，收招后由粒子系统独立完成余辉。
            ParticleHandler.SpawnParticle(new GenericSparkle(tip, outward * 0.4f, new Color(255, 253, 240),
                new Color(255, 155, 45), (finishing ? (Phase == 4 ? 1.6f : 1.15f) : 0.8f) * Projectile.scale,
                finishing ? 24 : 16, rotationSpeed: 0.015f, bloomScale: 1.6f));
            ParticleHandler.SpawnParticle(new BloomParticle(tip, Vector2.Zero, new Color(255, 125, 35) * 0.65f,
                0.08f * Projectile.scale, (finishing ? (Phase == 4 ? 0.65f : 0.45f) : 0.3f) * Projectile.scale, 12, lifeEndRatio: 0.15f));
        }

        Vector2 GetTangent(float sample)
        {
            float before = Math.Max(sample - 0.002f, ArcStart);
            float after = Math.Min(sample + 0.002f, ArcEnd);
            return (GetArcTip(after) - GetArcTip(before)).SafeNormalize(-Vector2.UnitY * Projectile.Owner.gravDir);
        }
    }
}

