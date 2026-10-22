namespace Anomalies.SpecialContents.Developer.EventideReunion;

public sealed partial class EventideReunionSunSkill
{
    public override void SetStaticDefaults() => ProjectileID.Sets.DrawScreenCheckFluff[Type] = 640;

    public override bool PreDraw(ref Color lightColor)
    {
        SpriteBatch batch = Main.spriteBatch;
        batch.End();
        float crown = Phase switch
        {
            SunPhase.Spin => MathHelper.Clamp(Timer / 18f, 0f, 1f),
            SunPhase.Charge => 1f,
            SunPhase.Dive => 1f,
            SunPhase.Retract or SunPhase.SlashCharge or SunPhase.Slash => 1f,
            SunPhase.Recovery => 1f - MathHelper.Clamp(Timer / (float)RecoveryDuration, 0f, 1f),
            _ => 0f
        };
        float time = (float)Main.GlobalTimeWrappedHourly;
        if (crown > 0f)
        {
            AnomalyDrawUtils.DrawGlowingCircle(batch, Projectile.Owner.Center, 28f + crown * 15f,
                new Color(255, 145, 25), crown * 0.8f, 3f, 14f);
        }
        if (Phase is SunPhase.Spin or SunPhase.Charge)
        {
            float opacity = Phase == SunPhase.Spin ? 1f : Math.Max(0f, 1f - Timer / 18f);
            float progress = Phase == SunPhase.Spin ? SpinProgress : 1f;
            EventideReunionSunDrawing.DrawSweepHalo(Grip, progress, Facing, Projectile.Owner.gravDir, opacity, time);
        }
        if (Phase is SunPhase.Slash or SunPhase.Recovery)
        {
            float progress = Phase == SunPhase.Slash ? SlashProgress : 1f;
            float opacity = Phase == SunPhase.Slash ? 1f : 1f - MathHelper.Clamp(Timer / (float)RecoveryDuration, 0f, 1f);
            EventideReunionSunDrawing.DrawFinishingSlash(Grip, SlashAim, progress, Facing, Projectile.Owner.gravDir, opacity, time);
        }
        // 矛尖日轮由带有十二条短辐射线的圆环组成，起手时加速旋转。
        AnomalyDrawUtils.DrawGlowingCircle(batch, WeaponTip, 20f, Color.Gold, 0.85f, 2f, 8f);
        Span<(Vector2 Start, Vector2 End)> rays = stackalloc (Vector2, Vector2)[12];
        for (int i = 0; i < rays.Length; i++)
        {
            Vector2 direction = (time * 12f + i * MathHelper.TwoPi / rays.Length).ToRotationVector2();
            rays[i] = (WeaponTip + direction * 23f, WeaponTip + direction * 33f);
        }
        AnomalyDrawUtils.DrawGlowingLines(batch, rays, Color.Gold, 0.7f, 2f, 7f);
        batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
            DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
        Texture2D weapon = ModContent.Request<Texture2D>(Texture).Value;
        if (Phase is SunPhase.Spin or SunPhase.Slash)
        {
            for (int i = 3; i > 0; i--)
            {
                float sample = (Phase == SunPhase.Spin ? SpinProgress : SlashProgress) - i * 0.035f;
                if (sample < 0f)
                    continue;
                Vector2 vector = Phase == SunPhase.Spin
                    ? EventideReunionSunGeometry.SweepVector(sample, Facing, Projectile.Owner.gravDir) * SpinWeaponScale
                    : GetSlashVector(sample);
                float offset = Phase == SunPhase.Spin ? 48f : EventideReunionSunGeometry.SlashArmOffset(sample);
                batch.Draw(weapon, Grip + vector * offset - Main.screenPosition, null, new Color(255, 105, 20, 0) * (0.15f / i),
                    vector.ToRotation() + MathHelper.PiOver4, new Vector2(144f, 156f), vector.Length(), SpriteEffects.None, 0f);
            }
        }
        if (Phase == SunPhase.Dive)
        {
            for (int i = 3; i > 0; i--)
                batch.Draw(weapon, _WeaponCenter - DiveDirection * i * 15f - Main.screenPosition, null,
                    new Color(255, 165, 40, 0) * (0.25f / i), _WeaponDirection.ToRotation() + MathHelper.PiOver4,
                    new Vector2(144f, 156f), _WeaponScale, SpriteEffects.None, 0f);
        }
        batch.Draw(weapon, _WeaponCenter - Main.screenPosition, null, Color.White,
            _WeaponDirection.ToRotation() + MathHelper.PiOver4, new Vector2(144f, 156f), _WeaponScale, SpriteEffects.None, 0f);
        return false;
    }
}

// 调用前 SpriteBatch 必须处于未开始状态；所有输入均为世界像素坐标。
internal static class EventideReunionSunDrawing
{
    public static void DrawSweepHalo(Vector2 grip, float progress, int facing, float gravity, float opacity, float time)
    {
        if (progress <= 0f || opacity <= 0f)
            return;
        DrawBlade(grip, progress, Math.Max(0f, progress - 0.7f), opacity, time,
            sample => EventideReunionSunGeometry.SweepVector(sample, facing, gravity) * EventideReunionSunGeometry.SweepOuterRadius);
    }

    public static void DrawFinishingSlash(Vector2 grip, Vector2 aim, float progress, int facing, float gravity, float opacity, float time)
    {
        if (progress <= 0f || opacity <= 0f)
            return;
        DrawBlade(grip, progress, Math.Max(0f, progress - 0.65f), opacity, time,
            sample => EventideReunionSunGeometry.SlashVector(sample, aim, facing, gravity)
                * (220f + EventideReunionSunGeometry.SlashArmOffset(sample)));
    }

    private static void DrawBlade(Vector2 grip, float head, float tail, float opacity, float time, Func<float, Vector2> sampleVector)
    {
        const int Samples = 96;
        VertexPositionColorTexture[] vertices = new VertexPositionColorTexture[Samples * 2];
        for (int i = 0; i < Samples; i++)
        {
            float factor = i / (Samples - 1f);
            Vector2 vector = sampleVector(MathHelper.Lerp(tail, head, factor));
            Color tint = new(1f, 1f, 1f, MathF.Sin(factor * MathHelper.PiOver2) * opacity);
            vertices[i * 2] = new(new Vector3(grip + vector * 0.23f, 0f), tint, new Vector2(factor, 1f));
            vertices[i * 2 + 1] = new(new Vector3(grip + vector, 0f), tint, new Vector2(factor, 0f));
        }
        EventideReunionBladeTrail.Draw(vertices, time);
    }

}
