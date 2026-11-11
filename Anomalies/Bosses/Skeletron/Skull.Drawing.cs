using Anomalies.Assets.Effects;

namespace Anomalies.Bosses.Skeletron;

public sealed partial class Skull
{
    private const int TrailLength = 240;
    private const float GlowRadius = 13f;
    private const int GlowPadding = 34;
    private static readonly Color _CoolWhiteGlow = new(231, 240, 255);

    public override void Load() =>
        ProjectileID.Sets.DrawScreenCheckFluff[ApplyingType] =
            Math.Max(ProjectileID.Sets.DrawScreenCheckFluff[ApplyingType], TrailLength + GlowPadding);

    public override bool PreDraw(ref Color lightColor)
    {
        if (!HasBeamTrail)
            return true;

        SpriteBatch spriteBatch = Main.spriteBatch;
        Vector2 center = Projectile.Center - Main.screenPosition;
        float rotation = Projectile.velocity.ToRotation();
        Vector2 forward = rotation.ToRotationVector2();
        float trailLength = Projectile.localAI[1];
        Effect effect = AnomalyClientConfig.Instance.EnableShaders
            ? AnomalyEffects.Primitives.SkeletronForwardBeam?.Asset.Value
            : null;
        if (effect is null)
        {
            DrawFallback();
            return true;
        }

        spriteBatch.End();
        spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp,
            DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
        effect.Parameters["uTrailLength"].SetValue(trailLength);
        effect.Parameters["uPadding"].SetValue(GlowPadding);
        effect.Parameters["uInverseHeadRadius"].SetValue(1f / GlowRadius);
        effect.Parameters["uInverseTrailLength"].SetValue(1f / Math.Max(1f, trailLength));
        effect.Parameters["uTrailOpacity"].SetValue(Math.Min(1f, trailLength / 10f));
        effect.Parameters["uHeadColor"].SetValue(_CoolWhiteGlow.ToVector3());
        effect.Parameters["uTailColor"].SetValue(SkeletronHandler.BoneColor.ToVector3());
        effect.CurrentTechnique.Passes["Pass0"].Apply();

        Texture2D pixel = TOTextures.FillerTexture;
        spriteBatch.Draw(pixel, center - forward * (trailLength * 0.5f), null, Color.White,
            rotation, pixel.Size() * 0.5f,
            new Vector2((trailLength + 2f * GlowPadding) / pixel.Width, 2f * GlowPadding / pixel.Height),
            SpriteEffects.None, 0f);

        spriteBatch.End();
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
            DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
        return true;

        void DrawFallback()
        {
            Texture2D line = TOTextures.Extra.BloomLineThick;
            if (line is not null && trailLength > 0f)
                spriteBatch.Draw(line, center - forward * (trailLength * 0.5f), null,
                    (SkeletronHandler.BoneColor with { A = 0 }) * 0.75f, rotation, line.Size() * 0.5f,
                    new Vector2(trailLength / line.Width, GlowRadius * 2f / line.Height), SpriteEffects.None, 0f);

            Texture2D bloom = BloomParticle.BloomCircleLarge;
            spriteBatch.Draw(bloom, center, null, (_CoolWhiteGlow with { A = 0 }) * 0.65f, 0f,
                bloom.Size() * 0.5f, GlowPadding * 2f / bloom.Width, SpriteEffects.None, 0f);
        }
    }
}
