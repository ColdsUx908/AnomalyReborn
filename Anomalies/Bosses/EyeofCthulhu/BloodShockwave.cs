// Developed by ColdsUx

using Anomalies.GameContents.Base;

namespace Anomalies.Bosses.EyeofCthulhu;

public sealed class BloodShockwave : BaseShockwaveProjectile, IContentLoader
{
    public override bool Hostile => false;
    public override List<int> NPCTypesToHit => _NpcTypesToHit;
    public override int LifeTime => 150;
    public override float FinalScale => 2.5f;
    public override bool UseHDTexture => true;

    private static readonly List<int> _NpcTypesToHit = [NPCID.ServantofCthulhu];

    public NPC Master
    {
        get => NPC.GetNPCFromIndex((int)Projectile.ai[0]);
        set => Projectile.ai[0] = value?.whoAmI ?? -1;
    }

    public override string LocalizationCategory => "Bosses.EyeofCthulhu";

    public override bool? CanHitNPC(NPC target) => base.CanHitNPC(target) == true && target.Master == Master;

    public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) => modifiers.SetInstantKillBetter(target);

    public override Color? GetAlpha(Color lightColor) => Color.Red * 0.75f;
}
