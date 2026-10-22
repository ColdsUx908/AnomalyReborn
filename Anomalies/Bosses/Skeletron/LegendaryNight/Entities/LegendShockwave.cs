using Anomalies.GameContents.Base;

namespace Anomalies.Bosses.Skeletron.LegendaryNight;

public sealed class LegendShockwave : BaseShockwaveProjectile, IContentLoader
{
    public override string LocalizationCategory => LegendaryNightHandler.LocalizationCategory;

    public override int LifeTime => 60;
    // ai[1]：可同步的最终缩放；0 保留普通符卡冲击波的默认大小。
    public float TargetScale
    {
        get => Projectile.ai[1] > 0f ? Projectile.ai[1] : 1.5f;
        set => Projectile.ai[1] = value;
    }
    public override float FinalScale => TargetScale;
    public override bool UseHDTexture => true;

    public override Color? GetAlpha(Color lightColor) => (int)Projectile.ai[0] switch
    {
        1 => LegendaryNightHandler.SlimeColor, //史莱姆色
        2 => LegendaryNightHandler.StarColor, //星星色
        3 => LegendaryNightHandler.PlatinumColor, //铂金币色
        _ => LegendaryNightHandler.BoneColor //骨色
    };
}

