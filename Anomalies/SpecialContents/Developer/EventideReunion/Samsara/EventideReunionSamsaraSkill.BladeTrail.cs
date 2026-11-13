namespace Anomalies.SpecialContents.Developer.EventideReunion;

public sealed partial class EventideReunionSamsaraSkill
{
    private Vector2 BladeVector(bool turn, float progress, Vector2? turnAim = null) => (turn
        ? EventideReunionSamsaraVisualGeometry.TurnVector(progress,
            Facing > 0 ? MathHelper.Pi + 0.15f : -0.15f, turnAim ?? Aim, Projectile.Owner.gravDir)
        : EventideReunionSamsaraVisualGeometry.SpinVector(progress, Facing)) * WeaponTipDistance;

    private void DrawBladeTrails()
    {
        if (Phase is not (SamsaraPhase.Spin or SamsaraPhase.Turn or SamsaraPhase.Pullback or SamsaraPhase.Thrust)
            || Phase == SamsaraPhase.Thrust && Timer >= 6)
            return;
        Main.spriteBatch.End();
        try
        {
            if (Phase == SamsaraPhase.Spin)
                DrawArc(false, Progress, 1f);
            else if (Phase == SamsaraPhase.Turn)
            {
                DrawArc(false, 1f, 1f - Progress);
                DrawArc(true, Progress, 1f);
            }
            else if (Phase == SamsaraPhase.Pullback)
                DrawArc(true, 1f, 1f - Progress);
            else
                DrawArc(true, 1f, 0f);
        }
        finally
        {
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
                DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
        }

        void DrawArc(bool turn, float head, float opacity)
            => EventideReunionBladeTrail.DrawArc(Grip, head, Math.Max(0f, head - (turn ? 0.65f : 0.7f)),
                opacity, Main.GlobalTimeWrappedHourly, sample => BladeVector(turn, sample, Phase == SamsaraPhase.Pullback ? _PullbackFromAim : null), samsaraPalette: true);
    }

    private void EmitBladeParticles()
    {
        bool turn = Phase == SamsaraPhase.Turn;
        float duration = turn ? TurnDuration : SpinDuration;
        float previous = Math.Max(0f, (Timer - 1f) / duration);
        for (int i = 0; i < 3; i++)
        {
            float sample = MathHelper.Lerp(previous, Progress, (i + 0.5f) / 3f);
            Vector2 vector = BladeVector(turn, sample);
            Vector2 tangent = (BladeVector(turn, Math.Min(1f, sample + 0.01f)) - vector).SafeNormalize(Aim);
            Vector2 position = Grip + vector * Main.rand.NextFloat(0.85f, 1f);
            Color color = Color.Lerp(new Color(40, 225, 245), new Color(210, 165, 255), Main.rand.NextFloat());
            Vector2 motion = tangent * Main.rand.NextFloat(1.5f, 4f);
            ParticleHandler.SpawnParticle(new PointingParticle(position, motion, false, Main.rand.Next(14, 23),
                Main.rand.NextFloat(0.2f, 0.4f), color));
            if (i == 0 && Timer % 2 == 0)
                ParticleHandler.SpawnParticle(new GenericSparkle(position, motion * 0.35f,
                    new Color(225, 250, 255), color, 0.24f, 20, rotationSpeed: 0.035f, bloomScale: 1.2f));
        }
    }
}
