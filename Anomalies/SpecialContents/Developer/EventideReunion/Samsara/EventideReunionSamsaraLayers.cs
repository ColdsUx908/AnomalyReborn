namespace Anomalies.SpecialContents.Developer.EventideReunion;

// 沿用 Sun 的横向投影方式，旋身以左半身为轴，快速转身后恢复正常宽度。
public sealed class EventideReunionSamsaraTurnLayer : PlayerDrawLayer
{
    public override Position GetDefaultPosition() => PlayerDrawLayers.AfterLastVanillaLayer;
    public override bool GetDefaultVisibility(PlayerDrawSet drawInfo)
        => drawInfo.drawPlayer.ownedProjectileCounts[ModContent.ProjectileType<EventideReunionSamsaraSkill>()] > 0;
    protected override void Draw(ref PlayerDrawSet drawInfo)
    {
        foreach (Projectile projectile in Main.ActiveProjectiles)
        {
            if (projectile.owner != drawInfo.drawPlayer.whoAmI || projectile.ModProjectile is not EventideReunionSamsaraSkill skill)
                continue;
            float scaleX = Math.Max(0.12f, Math.Abs(MathF.Cos(skill.BodyTurnAngle)));
            if (scaleX >= 0.9999f)
                return;
            float pivotX = drawInfo.Position.X + drawInfo.drawPlayer.width * 0.25f - Main.screenPosition.X;
            for (int i = 0; i < drawInfo.DrawDataCache.Count; i++)
            {
                DrawData data = drawInfo.DrawDataCache[i];
                data.position.X = pivotX + (data.position.X - pivotX) * scaleX;
                data.scale.X *= scaleX;
                drawInfo.DrawDataCache[i] = data;
            }
            return;
        }
    }
}
