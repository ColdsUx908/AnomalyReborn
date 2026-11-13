namespace Anomalies.SpecialContents.Developer.EventideReunion;

public sealed partial class EventideReunionHoldout
{
    // 第四击继承上挑末端姿态，在高位后引蓄势，然后以更大的半径向下劈斩。
    private Vector2 GetDownwardVector(float progress)
    {
        float charge = MathHelper.SmoothStep(0f, 1f, MathHelper.Clamp(progress / (12f / DownwardDuration), 0f, 1f));
        float angle = MathHelper.Lerp(-1.65f, -2.1f, charge);
        if (progress >= DownwardStart)
        {
            float slash = MathHelper.Clamp((progress - DownwardStart) / (DownwardEnd - DownwardStart), 0f, 1f);
            angle = MathHelper.Lerp(-2.1f, 1.8f, 1f - MathF.Pow(1f - slash, 3f));
        }
        if (progress > DownwardEnd)
        {
            float settle = MathHelper.SmoothStep(0f, 1f, MathHelper.Clamp((progress - DownwardEnd) / (1f - DownwardEnd), 0f, 1f));
            angle = MathHelper.Lerp(1.8f, 1.65f, settle);
        }
        float handedness = (FaceRight ? 1f : -1f) * Projectile.Owner.gravDir;
        Vector3 point = new(MathF.Cos(angle), MathF.Sin(angle) * handedness, 0f);
        point = Vector3.Transform(point, Matrix.CreateRotationX(0.72f));
        point = Vector3.Transform(point, Matrix.CreateRotationZ(Projectile.velocity.ToRotation()));
        return new Vector2(point.X, point.Y) * (900f / (900f - point.Z * 250f));
    }

    private Vector2 GetDownwardCenter(float progress)
    {
        float reach = MathHelper.SmoothStep(60f, 125f, MathHelper.Clamp(progress / (12f / DownwardDuration), 0f, 1f));
        return _Grip + GetDownwardVector(progress) * reach * Projectile.scale;
    }

    private Vector2 GetArcVector(float progress) => Phase == 4 ? GetDownwardVector(progress) : GetSweepVector(progress);
    private Vector2 GetArcCenter(float progress) => Phase == 4 ? GetDownwardCenter(progress) : _Grip + GetSweepVector(progress) * 60f * Projectile.scale;
    private Vector2 GetArcTip(float progress) => GetWeaponTip(GetArcCenter(progress), GetArcVector(progress));

    private void UpdateDownwardCharge()
    {
        if (Main.dedServ || _PreviousProgress >= DownwardStart)
            return;
        const int Slices = 8;
        int first = (int)(MathHelper.Clamp(_PreviousProgress / DownwardStart, 0f, 1f) * Slices);
        int last = (int)(MathHelper.Clamp(Progress / DownwardStart, 0f, 1f) * Slices);
        for (int i = first; i < last; i++)
        {
            float sample = DownwardStart * (i + 0.5f) / Slices;
            Vector2 tip = GetArcTip(sample);
            Vector2 offset = Main.rand.NextVector2Unit() * Main.rand.NextFloat(18f, 38f);
            ParticleHandler.SpawnParticle(new GenericSparkle(tip + offset, -offset / 10f,
                new Color(255, 245, 210), new Color(255, 125, 20), 0.3f + i * 0.035f, 14,
                rotationSpeed: 0.04f, bloomScale: 1.5f));
        }
    }
}
