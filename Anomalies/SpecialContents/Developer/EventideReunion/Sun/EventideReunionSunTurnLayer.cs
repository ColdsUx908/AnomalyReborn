namespace Anomalies.SpecialContents.Developer.EventideReunion;

// 玩家素材是平面贴图：绕竖轴转身时压缩横向投影，并由技能 AI 在过侧面时切换朝向。
public sealed class EventideReunionSunTurnLayer : PlayerDrawLayer
{
    public override Position GetDefaultPosition() => PlayerDrawLayers.AfterLastVanillaLayer;

    public override bool GetDefaultVisibility(PlayerDrawSet drawInfo)
        => drawInfo.drawPlayer.ownedProjectileCounts[ModContent.ProjectileType<EventideReunionSunSkill>()] > 0;

    protected override void Draw(ref PlayerDrawSet drawInfo)
    {
        foreach (Projectile activeProjectile in Main.ActiveProjectiles)
        {
            if (activeProjectile.owner != drawInfo.drawPlayer.whoAmI || activeProjectile.ModProjectile is not EventideReunionSunSkill skill)
                continue;
            float scaleX = Math.Max(0.12f, Math.Abs(MathF.Cos(skill.HorizontalTurnAngle)));
            if (scaleX >= 0.9999f)
                return;
            Vector2 pivot = drawInfo.Position + drawInfo.drawPlayer.Size * 0.5f - Main.screenPosition;
            for (int i = 0; i < drawInfo.DrawDataCache.Count; i++)
            {
                DrawData data = drawInfo.DrawDataCache[i];
                data.position.X = pivot.X + (data.position.X - pivot.X) * scaleX;
                data.scale.X *= scaleX;
                drawInfo.DrawDataCache[i] = data;
            }
            return;
        }
    }
}
