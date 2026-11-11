using Anomalies.GameContents.Base;

namespace Anomalies.Bosses.Skeletron;

public sealed class BoneShockwave : BaseShockwaveProjectile
{
    internal const float PhaseChangeVariant = 1f;
    private bool IsPhaseChange => Projectile.ai[0] == PhaseChangeVariant;

    public override string LocalizationCategory => "Bosses.Skeletron";
    public override int LifeTime => IsPhaseChange ? 150 : 180;
    public override float FinalScale => IsPhaseChange ? 3.5f : 3f;
    public override bool UseHDTexture => true;

    public override void AI()
    {
        if (Timer1 == 0 && !Main.dedServ)
            SoundEngine.PlaySound(SoundID.ForceRoar, Projectile.Center);

        base.AI();
        if (!IsPhaseChange)
            Projectile.Opacity = 1f - MathHelper.SmoothStep(0f, 1f, (LifeCompletion - 0.55f) / 0.45f);
    }

    public override Color? GetAlpha(Color lightColor) => SkeletronHandler.BoneColor;
}
