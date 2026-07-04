// Developed by ColdsUx

using CalamityAnomalies.GameContents.Base;
using CalamityMod.NPCs.NormalNPCs;

namespace CalamityAnomalies.Anomaly.KingSlime;

public sealed class RainbowShockwave : BaseShockwaveProjectile, IContentLoader
{
    public override bool Hostile => false;
    public override List<int> NPCTypesToHit => _npcTypesToHit;
    public override int LifeTime => 150;
    public override float FinalScale => 3.5f;
    public override bool UseHDTexture => true;

    private static List<int> _npcTypesToHit;

    public NPC Master
    {
        get => NPC.TryGetNPC((int)Projectile.ai[0]);
        set => Projectile.ai[0] = value?.whoAmI ?? -1;
    }

    public NPC Jewel
    {
        get => NPC.TryGetNPC((int)Projectile.ai[1]);
        set => Projectile.ai[1] = value?.whoAmI ?? -1;
    }


    public override string LocalizationCategory => "Anomaly.KingSlime";

    public override bool? CanHitNPC(NPC target) => base.CanHitNPC(target) == true && target.Master == Master;

    public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) => modifiers.SetInstantKillBetter(target);

    public override Color? GetAlpha(Color lightColor) => Color.LerpMany(Color.RainbowColors, TOMathUtils.Interpolation.QuadraticEaseOut(LifeCompletion));

    void IContentLoader.PostSetupContent()
    {
        _npcTypesToHit =
        [
            NPCID.GreenSlime,
            NPCID.BlueSlime,
            NPCID.RedSlime,
            NPCID.PurpleSlime,
            NPCID.YellowSlime,
            NPCID.IceSlime,
            NPCID.JungleSlime,
            NPCID.SlimeSpiked,
            NPCID.SpikedIceSlime,
            NPCID.SpikedJungleSlime,
            NPCID.UmbrellaSlime,
            NPCID.CorruptSlime,
            NPCID.Crimslime,
            NPCID.ShimmerSlime,
            NPCID.IlluminantSlime,
            ModContent.NPCType<EbonianBlightSlime>(),
            ModContent.NPCType<CrimulanBlightSlime>(),
        ];
    }

    void IContentLoader.OnModUnload()
    {
        _npcTypesToHit = null;
    }
}
