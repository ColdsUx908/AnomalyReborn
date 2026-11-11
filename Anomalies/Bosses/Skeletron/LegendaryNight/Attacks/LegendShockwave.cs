using Anomalies.GameContents.Base;

namespace Anomalies.Bosses.Skeletron.LegendaryNight;

/// <summary>
/// 多个符卡共用的普通冲击波。
/// </summary>
/// <remarks>
/// <para>ai[0]：符卡颜色编号，按 Head.Spell 解释；ai[1]：基础缩放，0使用默认1.5；实际最终缩放为基础值的三倍；ai[2]：未使用。</para>
/// <para>localAI：未使用；Timer1（OceanAI32[2].I）：基类扩张累计帧；其余 Timer 未使用。</para>
/// <para>无实体专属 ExtraAI；继承 BaseShockwaveProjectile 的几何与生命周期。</para>
/// </remarks>
public sealed class LegendShockwave : BaseShockwaveProjectile
{
    public override string LocalizationCategory => LegendaryNightHandler.LocalizationCategory;

    private const int ExpansionMultiplier = 3;
    public override int LifeTime => 60 * ExpansionMultiplier;
    // ai[1]：可同步的基础缩放。
    public float TargetScale
    {
        get => Projectile.ai[1] > 0f ? Projectile.ai[1] : 2.25f;
        set => Projectile.ai[1] = value;
    }
    public override float FinalScale => TargetScale * ExpansionMultiplier;
    public override bool UseHDTexture => true;

    public override void AI()
    {
        base.AI();
        Projectile.Opacity = 1f - MathHelper.SmoothStep(0f, 1f, (LifeCompletion - 0.55f) / 0.45f);
    }

    public override Color? GetAlpha(Color lightColor) => (int)Projectile.ai[0] switch
    {
        1 => LegendaryNightHandler.SlimeColor, //史莱姆色
        2 => LegendaryNightHandler.StarColor, //星星色
        3 => LegendaryNightHandler.PlatinumColor, //铂金币色
        _ => LegendaryNightHandler.BoneColor //骨色
    };
}
