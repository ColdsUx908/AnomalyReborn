using Anomalies.GameContents.Base;

namespace Anomalies.Bosses.Skeletron.SkeletronLegend;

public sealed class LegendShockwave : BaseShockwaveProjectile, IContentLoader
{
    public static readonly Color SlimeColor = new(105, 140, 255);
    public static readonly Color FallenStarColor = new(255, 215, 0);
    public static readonly Color PlatinumColor = new(242, 243, 236);

    public override string LocalizationCategory => "Bosses.Skeletron.Legend";

    public override int LifeTime => 60;
    public override float FinalScale => 1.5f;
    public override bool UseHDTexture => true;

    public override Color? GetAlpha(Color lightColor) => (int)Projectile.ai[0] switch
    {
        1 => SlimeColor, //史莱姆色
        2 => FallenStarColor, //落星色
        _ => PlatinumColor //铂金币色
    };
}

