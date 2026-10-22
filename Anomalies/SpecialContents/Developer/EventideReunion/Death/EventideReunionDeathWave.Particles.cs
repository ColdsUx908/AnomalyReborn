namespace Anomalies.SpecialContents.Developer.EventideReunion;

public sealed partial class EventideReunionDeathWave
{
    // 纯本地视觉历史，不进入网络快照。
    private Vector2 _PreviousTrailCenter;
    private bool _TrailInitialized;

    private void EmitFlightTrail()
    {
        if (Main.dedServ)
            return;
        Vector2 previous = _TrailInitialized ? _PreviousTrailCenter : Projectile.Center;
        _PreviousTrailCenter = Projectile.Center;
        _TrailInitialized = true;
        float fade = MathHelper.SmoothStep(0f, 1f, Math.Min(1f, Projectile.timeLeft / 12f));
        int samples = Math.Clamp((int)MathF.Ceiling(Vector2.Distance(previous, Projectile.Center) / 6f), 1, 4);
        Vector2 normal = new(-Aim.Y, Aim.X);
        for (int i = 0; i < samples; i++)
        {
            // 沿两帧之间的世界轨迹补点，粒子生成后留在身后，不再跟着弹幕移动。
            Vector2 center = Vector2.Lerp(previous, Projectile.Center, (i + 1f) / samples);
            for (int j = 0; j < 2; j++)
            {
                if (Main.rand.NextFloat() > fade)
                    continue;
                float arc = Progress * Main.rand.NextFloat(0.08f, 0.92f);
                Vector2 blade = EventideReunionDeathGeometry.SlashVector(arc, Aim, SwingSign);
                Vector2 position = center + blade * Main.rand.NextFloat(0.55f, 0.96f)
                    - Aim * Main.rand.NextFloat(8f, 24f);
                Vector2 velocity = -Aim * Main.rand.NextFloat(1.2f, 3.5f) + normal * Main.rand.NextFloat(-0.35f, 0.35f);
                Color color = Color.Lerp(new Color(135, 55, 255), new Color(230, 180, 255), Main.rand.NextFloat());
                ParticleHandler.SpawnParticle(new PointingParticle(position, velocity, false, Main.rand.Next(20, 34),
                    Main.rand.NextFloat(0.25f, 0.48f) * fade, color));
                if (j == 0 && Main.rand.NextBool(3))
                    ParticleHandler.SpawnParticle(new GenericSparkle(position, velocity * 0.2f,
                        new Color(245, 225, 255), new Color(155, 65, 255), Main.rand.NextFloat(0.25f, 0.45f) * fade,
                        Main.rand.Next(22, 36), rotationSpeed: 0.025f, bloomScale: 1.2f));
            }
        }
    }
}
