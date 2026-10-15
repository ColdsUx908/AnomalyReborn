using Anomalies.GameContents.Base;

namespace Anomalies.Bosses.Skeletron.LegendaryNight;

public sealed class LegendShockwave : BaseShockwaveProjectile, IContentLoader
{
    public override string LocalizationCategory => LegendaryNightHandler.LocalizationCategory;

    public override int LifeTime => 60;
    public override float FinalScale => 1.5f;
    public override bool UseHDTexture => true;

    public override Color? GetAlpha(Color lightColor) => (int)Projectile.ai[0] switch
    {
        1 => LegendaryNightHandler.SlimeColor, //史莱姆色
        2 => LegendaryNightHandler.StarColor, //星星色
        3 => LegendaryNightHandler.PlatinumColor, //铂金币色
        _ => LegendaryNightHandler.BoneColor //骨色
    };
}

