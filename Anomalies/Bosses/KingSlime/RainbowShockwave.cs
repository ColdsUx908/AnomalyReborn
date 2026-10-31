using Anomalies.GameContents.Base;

namespace Anomalies.Bosses.KingSlime;

public sealed class RainbowShockwave : BaseShockwaveProjectile
{
    public override List<int> NPCTypesToHit => _NpcTypesToHit;
    public override int LifeTime => 150;
    public override float FinalScale => 3.5f;
    public override bool UseHDTexture => true;

    private static List<int> _NpcTypesToHit;

    public NPC Master
    {
        get => NPC.GetNPCFromIndex((int)Projectile.ai[0]);
        set => Projectile.ai[0] = value?.whoAmI ?? -1;
    }

    public NPC Jewel
    {
        get => NPC.GetNPCFromIndex((int)Projectile.ai[1]);
        set => Projectile.ai[1] = value?.whoAmI ?? -1;
    }


    public override string LocalizationCategory => "Bosses.KingSlime";

    public override bool? CanHitNPC(NPC target) => base.CanHitNPC(target) == true && target.Master == Master;

    public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) => modifiers.SetInstantKillBetter(target);

    public override Color? GetAlpha(Color lightColor) => Color.LerpMany(Color.RainbowColors, TOMathUtils.Interpolation.QuadraticEaseOut(LifeCompletion));

    /// <summary>在 tML 内容加载时建立仅包含原版 NPC 常量的命中集合。</summary>
    public override void Load()
    {
        TOCleanup.Register(() => _NpcTypesToHit = null);
        _NpcTypesToHit =
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
        ];
    }

}

