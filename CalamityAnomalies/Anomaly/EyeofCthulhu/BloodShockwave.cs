using Transoceanic.DataStructures.GameContent;

namespace CalamityAnomalies.Anomaly.EyeofCthulhu;

public sealed class BloodShockwave : TOShockwaveProjectile, IContentLoader
{
    public override bool Hostile => false;
    public override List<int> NPCTypesToHit => _npcTypesToHit;
    public override int LifeTime => 150;
    public override float FinalScale => 2.5f;
    public override bool UseHDTexture => true;

    private static readonly List<int> _npcTypesToHit = [NPCID.ServantofCthulhu];

    public NPC Master
    {
        get
        {
            int temp = (int)Projectile.ai[0];
            return temp >= 0 && temp < Main.maxNPCs ? Main.npc[temp] : null;
        }
        set => Projectile.ai[0] = value?.whoAmI ?? Main.maxNPCs;
    }

    public override string LocalizationCategory => "Anomaly.EyeofCthulhu";

    public override bool? CanHitNPC(NPC target) => base.CanHitNPC(target) == true && target.Master == Master;

    public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) => modifiers.SetInstantKillBetter(target);

    public override Color? GetAlpha(Color lightColor) => Color.Red * 0.75f;
}
