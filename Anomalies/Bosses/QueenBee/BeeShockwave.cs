using Anomalies.GameContents.Base;

namespace Anomalies.Bosses.QueenBee;

public sealed class BeeShockwave : BaseShockwaveProjectile, IContentLoader
{
    public override bool Hostile => false;
    public override List<int> NPCTypesToHit => null;
    public override int LifeTime => 150;
    public override float FinalScale => 2.5f;
    public override bool UseHDTexture => true;

    public NPC Master
    {
        get => NPC.GetNPCFromIndex((int)Projectile.ai[0]);
        set => Projectile.ai[0] = value?.whoAmI ?? -1;
    }

    public override string LocalizationCategory => "Bosses.QueenBee";

    public override bool? CanHitNPC(NPC target) => base.CanHitNPC(target) == true && target.Master == Master;

    public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) => modifiers.SetInstantKillBetter(target);

    public override Color? GetAlpha(Color lightColor) => Color.Orange;
}

