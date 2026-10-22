namespace Anomalies.SpecialContents.Developer.EventideReunion;

public sealed partial class EventideReunionHoldout
{

    private void DrawThrust()
    {
        float currentProgress = MathHelper.Clamp(Progress, 0f, 1f);
        if (currentProgress <= 0f || currentProgress >= 0.8f)
            return;

        float intensity = MathF.Sin(currentProgress / 0.8f * MathHelper.Pi);
        Texture2D glow = ParticleHandler.GetTexture<BloomParticle>();
        Vector2 tip = GetWeaponTip(Projectile.Center, _BladeVector);
        Vector2 tail = GetThrustTip(0f) - Projectile.velocity * 30f * Projectile.scale;
        Vector2 center = (tail + tip) * 0.5f - Main.screenPosition;
        float length = Vector2.Distance(tail, tip);
        float strength = intensity * (Phase == 2 ? 1f : 0.8f);
        // AlphaBlend 下使用零 alpha 的发光颜色，让短促橙色光痕与白芯叠加在场景上。
        Main.spriteBatch.Draw(glow, center, null, new Color(255, 105, 20, 0) * strength,
            Projectile.rotation, glow.Size() / 2f, new Vector2(length * 1.8f / glow.Width, 0.15f * Projectile.scale), SpriteEffects.None, 0f);
        Main.spriteBatch.Draw(glow, center, null, new Color(255, 245, 220, 0) * strength,
            Projectile.rotation, glow.Size() / 2f, new Vector2(length * 1.5f / glow.Width, 0.045f * Projectile.scale), SpriteEffects.None, 0f);
        Main.spriteBatch.Draw(glow, tip - Main.screenPosition, null, new Color(255, 165, 65, 0) * strength,
            0f, glow.Size() / 2f, 0.19f * Projectile.scale, SpriteEffects.None, 0f);
    }

    private void DrawSweep()
    {
        float currentProgress = MathHelper.Clamp(Progress, 0f, 1f);
        if (currentProgress <= ArcStart)
            return;

        float fade = 1f - MathHelper.Clamp((currentProgress - ArcEnd) / (1f - ArcEnd), 0f, 1f);
        // 残影和刀光都重新采样同一个三维轨迹，避免高速挥动时轨迹断裂。
        for (int i = Phase == 4 ? 7 : 4; i >= 1; i--)
        {
            float sample = currentProgress - i * 0.035f;
            if (sample < ArcStart || sample > ArcEnd)
                continue;
            Vector2 vector = GetArcVector(sample);
            Color tint = Color.Lerp(new Color(255, 105, 25), new Color(255, 245, 220), 1f - i / 5f);
            DrawWeapon(GetArcCenter(sample), vector, tint * (0.16f * fade));
        }

        const int Samples = 48;
        VertexPositionColorTexture[] vertices = new VertexPositionColorTexture[Samples * 2];
        VertexPositionColorTexture[] edgeVertices = Phase == 4 ? new VertexPositionColorTexture[Samples * 2] : null;
        // 收势也继续采样真实武器姿态，前沿始终贴在当前矛尖上。
        float head = currentProgress;
        float tail = Math.Max(ArcStart, currentProgress - (Phase == 4 ? 0.34f : 0.28f));
        if (tail >= head)
            return;
        for (int i = 0; i < Samples; i++)
        {
            float factor = i / (Samples - 1f);
            Vector2 outer = GetArcTip(MathHelper.Lerp(tail, head, factor));
            Color drawTint = new(1f, 1f, 1f, MathF.Sin(factor * MathHelper.PiOver2) * fade);
            Vector2 inner = Vector2.Lerp(_Grip, outer, 0.23f);
            vertices[i * 2] = new(new Vector3(inner, 0f), drawTint, new Vector2(factor, 1f));
            vertices[i * 2 + 1] = new(new Vector3(outer, 0f), drawTint, new Vector2(factor, 0f));
            if (edgeVertices is not null)
            {
                // 宽幅橙色刀身外叠一层窄金白刃光，外沿仍精确落在矛尖轨迹上。
                Color edgeTint = new(1f, 0.95f, 0.75f, drawTint.A / 255f * 0.65f);
                edgeVertices[i * 2] = new(new Vector3(Vector2.Lerp(_Grip, outer, 0.82f), 0f), edgeTint, new Vector2(factor, 1f));
                edgeVertices[i * 2 + 1] = new(new Vector3(outer, 0f), edgeTint, new Vector2(factor, 0f));
            }
        }

        Main.spriteBatch.End();
        try
        {
            EventideReunionBladeTrail.Draw(vertices, Projectile.ai[0] / 60f);
            if (edgeVertices is not null)
                EventideReunionBladeTrail.Draw(edgeVertices, Projectile.ai[0] / 45f);
        }
        finally
        {
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }
    }
}


