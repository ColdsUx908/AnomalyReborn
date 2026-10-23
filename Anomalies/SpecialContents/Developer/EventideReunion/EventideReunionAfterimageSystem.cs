namespace Anomalies.SpecialContents.Developer.EventideReunion;

// 持有弹幕的 PreDraw 会嵌套在 PlayerDrawLayers.RenderAllLayers 中。
// 原版玩家渲染器的 DrawData 和 Main.spriteBuffer 均不支持重入，残影必须先独立绘制。
public sealed class EventideReunionAfterimageSystem : ModSystem
{
    public override void PostDrawTiles()
    {
        if (Main.dedServ)
            return;
        // 在独立世界阶段绘制 shader 齿轮，位于人物和残影后方。
        // 不在 PlayerDrawLayer 内结束 SpriteBatch 或修改玩家绘制缓存。
        foreach (Projectile projectile in Main.ActiveProjectiles)
        {
            if (projectile.ModProjectile is EventideReunionSamsaraSkill samsara
                && projectile.Owner.active && !projectile.Owner.dead)
                samsara.DrawGear();
        }
        bool begun = false;
        try
        {
            foreach (Projectile projectile in Main.ActiveProjectiles)
            {
                if (projectile.ModProjectile is not (EventideReunionSamsaraSkill or EventideReunionDeathSkill)
                    || !projectile.Owner.active || projectile.Owner.dead)
                    continue;
                if (!begun)
                {
                    Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState,
                        DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
                    begun = true;
                }
                if (projectile.ModProjectile is EventideReunionSamsaraSkill samsara)
                    samsara.DrawAfterimages();
                else if (projectile.ModProjectile is EventideReunionDeathSkill death)
                    death.DrawAfterimages();
            }
        }
        finally
        {
            if (begun)
                Main.spriteBatch.End();
        }
    }
}
