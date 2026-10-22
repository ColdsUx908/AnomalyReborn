namespace Anomalies.SpecialContents.Developer.EventideReunion;

public sealed partial class EventideReunionSunSkill
{
    private float _PreviousSpinProgress;
    private float _PreviousSlashProgress;
    private bool _SpinBurstPlayed;
    private bool _SpinCompletionPlayed;

    private void UpdateSpinParticles()
    {
        if (Main.dedServ)
            return;
        const int Slices = 96;
        if (!_SpinBurstPlayed)
        {
            _SpinBurstPlayed = true;
            EmitBladeBurst(GetSpinTip(0f), 22);
        }
        int first = (int)MathF.Floor(_PreviousSpinProgress * Slices);
        int last = (int)MathF.Floor(SpinProgress * Slices);
        for (int slice = first; slice < last; slice++)
        {
            float sample = (slice + 0.5f) / Slices;
            Vector2 tip = GetSpinTip(sample);
            Vector2 tangent = (GetSpinTip(Math.Min(1f, sample + 0.002f)) - GetSpinTip(Math.Max(0f, sample - 0.002f)))
                .SafeNormalize(Vector2.UnitX * Facing);
            EmitBladeTrail(tip, tangent, slice);
        }
        if (SpinProgress >= 1f && !_SpinCompletionPlayed)
        {
            _SpinCompletionPlayed = true;
            EmitBladeBurst(GetSpinTip(1f), 36);
        }
        _PreviousSpinProgress = SpinProgress;
    }

    private void UpdateSlashParticles()
    {
        if (Main.dedServ)
            return;
        const int Slices = 48;
        int first = (int)MathF.Floor(_PreviousSlashProgress * Slices);
        int last = (int)MathF.Floor(SlashProgress * Slices);
        for (int slice = first; slice < last; slice++)
        {
            float sample = (slice + 0.5f) / Slices;
            Vector2 tip = GetSlashTip(sample);
            Vector2 tangent = (GetSlashTip(Math.Min(1f, sample + 0.002f)) - GetSlashTip(Math.Max(0f, sample - 0.002f)))
                .SafeNormalize(SlashAim);
            EmitBladeTrail(tip, tangent, slice);
        }
        if (_PreviousSlashProgress < 0.5f && SlashProgress >= 0.5f)
            EmitBladeBurst(GetSlashTip(0.5f), 28);
        _PreviousSlashProgress = SlashProgress;
    }

    private Vector2 GetSpinTip(float progress) => Grip + EventideReunionSunGeometry.SweepVector(progress, Facing,
        Projectile.Owner.gravDir) * EventideReunionSunGeometry.SweepOuterRadius;

    private void EmitBladeTrail(Vector2 tip, Vector2 tangent, int slice)
    {
        Vector2 outward = (tip - Grip).SafeNormalize(Vector2.UnitX * Facing);
        Vector2 position = Vector2.Lerp(Grip, tip, Main.rand.NextFloat(0.62f, 1f));
        Vector2 motion = tangent.RotatedBy(Main.rand.NextFloat(-0.4f, 0.4f)) * Main.rand.NextFloat(4f, 9f)
            + outward * Main.rand.NextFloat(1f, 4f);
        Color flame = Color.Lerp(new Color(255, 40, 10), new Color(255, 200, 55), Main.rand.NextFloat());
        ParticleHandler.SpawnParticle(new OrbParticle(position, motion * 0.65f, Main.rand.Next(18, 30),
            Main.rand.NextFloat(0.14f, 0.28f), flame, lifeEndRatio: 0.1f));
        ParticleHandler.SpawnParticle(new PointingParticle(tip, motion, false, Main.rand.Next(16, 28),
            Main.rand.NextFloat(0.4f, 0.85f), slice % 3 == 0 ? new Color(255, 245, 200) : flame));
        if (slice % 3 == 0)
            ParticleHandler.SpawnParticle(new GenericSparkle(position, motion * 0.4f, new Color(255, 240, 180), flame,
                Main.rand.NextFloat(0.35f, 0.65f), 22, rotationSpeed: 0.025f, bloomScale: 1.5f));
        if (slice % 2 == 0)
            Dust.NewDustPerfectAction(tip, DustID.RainbowMk2, dust =>
            {
                dust.noGravity = true;
                dust.color = flame;
                dust.scale = Main.rand.NextFloat(0.65f, 1f);
                dust.velocity = motion * 0.7f;
            });
    }

    private void EmitBladeBurst(Vector2 center, int count)
    {
        for (int i = 0; i < count; i++)
        {
            Vector2 motion = Main.rand.NextVector2Unit() * Main.rand.NextFloat(3f, 10f);
            ParticleHandler.SpawnParticle(new PointingParticle(center, motion, false, Main.rand.Next(18, 32),
                Main.rand.NextFloat(0.45f, 0.9f), i % 3 == 0 ? Color.Gold : new Color(255, 75, 15)));
        }
        ParticleHandler.SpawnParticle(new GenericSparkle(center, Vector2.Zero, new Color(255, 245, 200), Color.OrangeRed,
            1.2f, 22, rotationSpeed: 0.015f, bloomScale: 1.8f));
        ParticleHandler.SpawnParticle(new BloomParticle(center, Vector2.Zero, Color.Orange,
            0.1f, 0.45f, 16, lifeEndRatio: 0.1f));
    }
}
