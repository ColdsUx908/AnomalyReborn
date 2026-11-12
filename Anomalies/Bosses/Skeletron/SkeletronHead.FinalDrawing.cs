using Anomalies.Assets.Effects;

namespace Anomalies.Bosses.Skeletron;

public sealed partial class SkeletronHead
{
    private void DrawFinalCharge(SpriteBatch spriteBatch)
    {
        if (Main.dedServ || CurrentPhase != Phase.ToFinal || CurrentLocalPhase != 2)
            return;

        float charge = MathHelper.Clamp(Timer / FinalChargeDuration, 0f, 1f);
        float closing = MathHelper.Clamp((charge - 0.6f) / 0.4f, 0f, 1f);
        float collapse = closing * closing;
        float opacity = MathHelper.SmoothStep(0f, 1f, Math.Min(Timer / 18f, 1f));
        Effect effect = AnomalyClientConfig.Instance.EnableShaders ? AnomalyEffects.Overlays.SkeletronFinalCharge?.Asset.Value : null;
        spriteBatch.End();
        if (effect is not null)
        {
            Matrix projection = Matrix.CreateOrthographicOffCenter(0f, Main.screenWidth, Main.screenHeight, 0f, 0f, 1f);
            projection.M41 -= 1f / Main.screenWidth;
            projection.M42 += 1f / Main.screenHeight;
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, Main.Rasterizer, null, Matrix.Identity);
            effect
                .SetCustomParameter("uTransformMatrix", projection)
                .SetCustomParameter("uInverseViewMatrix", Matrix.Invert(Main.GameViewMatrix.TransformationMatrix))
                .SetCustomParameter("uScreenPosition", Main.screenPosition)
                .SetCustomParameter("uCenter", NPC.Center)
                .SetCustomParameter("uAge", Timer)
                .SetCustomParameter("uCharge", charge)
                .SetCustomParameter("uFieldScale", 1f - collapse)
                .SetCustomParameter("uFlow", Timer * 0.05f + charge * charge * charge * 22f)
                .SetCustomParameter("uOpacity", opacity);
            effect.CurrentTechnique.Passes["Pass0"].Apply();
            spriteBatch.Draw(TOTextures.FillerTexture, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), Color.White);
            spriteBatch.End();
        }

        // 稳定、连续的中心 Bloom；不生成逐帧叠加的光环粒子，关闭着色器也保留包裹感。
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
            DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
        Texture2D bloom = BloomParticle.BloomCircleLarge;
        float breath = 1f + 0.035f * MathF.Sin(Timer * 0.15f);
        float bloomScale = 1f - 0.45f * collapse;
        spriteBatch.DrawFromCenter(bloom, NPC.Center - Main.screenPosition, null,
            SkeletronCombatVisuals.BoneColor * (opacity * (0.2f + 0.3f * charge + 0.32f * collapse)), 0f,
            (1.5f + 0.9f * charge) * breath * bloomScale);
        spriteBatch.DrawFromCenter(bloom, NPC.Center - Main.screenPosition, null,
            new Color(255, 232, 184) * (opacity * (0.12f + 0.18f * charge + 0.25f * collapse)), 0f,
            (0.75f + 0.25f * charge) * bloomScale);
        TODrawUtils.ResetSpriteBatch(spriteBatch);
    }

}
