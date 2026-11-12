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
        Color headColor = _CoolWhiteGlow;
        Color tailColor = SkeletronCombatVisuals.BoneColor;
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
        effect.Parameters["uHeadColor"].SetValue(headColor.ToVector3());
        effect.Parameters["uTailColor"].SetValue(tailColor.ToVector3());
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
            {
                // 光带沿贴图 Y 轴延伸；分段控制纵向衰减，避免把横向亮芯拉成矩形。
                const float SegmentLength = 4f;
                int segments = (int)MathF.Ceiling(trailLength / SegmentLength);
                float opacity = Math.Min(1f, trailLength / 10f);
                for (int i = 0; i < segments; i++)
                {
                    float start = i * SegmentLength;
                    float length = Math.Min(SegmentLength, trailLength - start);
                    float distance = start + length * 0.5f;
                    float progress = distance / trailLength;
                    float fade = (1f - progress) * (1f - progress);
                    float width = MathHelper.Lerp(GlowRadius * 2f, 2f, progress);
                    Color color = Color.Lerp(headColor, tailColor, progress) with { A = 0 };
                    spriteBatch.Draw(line, center - forward * distance, null,
                        color * (0.75f * opacity * fade), rotation - MathHelper.PiOver2, line.Size() * 0.5f,
                        new Vector2(width / line.Width, length / line.Height), SpriteEffects.None, 0f);
                }
            }

            Texture2D bloom = BloomParticle.BloomCircleLarge;
            spriteBatch.Draw(bloom, center, null, (headColor with { A = 0 }) * 0.65f, 0f,
                bloom.Size() * 0.5f, GlowPadding * 2f / bloom.Width, SpriteEffects.None, 0f);
        }
    }

    private void SpawnEffects(Vector2 soundPosition)
    {
        if (Projectile.localAI[0] != 0f)
            return;
        Projectile.localAI[0] = 1f;
        SoundEngine.PlaySound(SoundID.Item8, soundPosition);
        for (int i = 0; i < 10; i++)
        {
            Dust.NewDustAction(Projectile.Center, Projectile.width, Projectile.height, DustID.Blood, Projectile.velocity, dust =>
            {
                dust.scale *= 2f;
                dust.noGravity = true;
                dust.velocity = (Projectile.Center - dust.position).SafeNormalize(Vector2.UnitY) * -5f + Projectile.velocity / 2f;
            });
        }
    }

    private void EmitTrail()
    {
        for (int i = 0; i < 2; i++)
        {
            Dust.NewDustAction(Projectile.Center, Projectile.width - 8, Projectile.height - 8, DustID.Blood, Projectile.velocity * 0.2f, dust =>
            {
                dust.alpha = 100;
                dust.scale *= 1.5f;
                dust.position -= Projectile.velocity;
                dust.noGravity = true;
                dust.velocity *= 0.3f;
            });
        }
    }

}
