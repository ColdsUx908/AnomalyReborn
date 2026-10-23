using Anomalies.Visuals.Overlays;
using Terraria.Graphics.Effects;

namespace Anomalies.SpecialContents.Developer.EventideReunion;

// 独立屏幕边缘遮罩，不写入通用的全屏 Visual_DarknessIntensity。
public sealed class EventideReunionDeathVignette : CustomOverlay
{
    public override void Draw(SpriteBatch spriteBatch, RenderLayers layer, bool beginSpriteBatch)
    {
        if (Main.dedServ || Main.gameMenu || layer != RenderLayers.All)
            return;
        float opacity = 0f;
        foreach (Projectile active in Main.ActiveProjectiles)
            if (active.owner == Main.myPlayer && active.ModProjectile is EventideReunionDeathSkill skill)
                opacity = Math.Max(opacity, skill.EdgeDarknessOpacity);
        if (opacity > 0f)
            EventideReunionDeathDrawing.DrawVignette(opacity);
    }
}
