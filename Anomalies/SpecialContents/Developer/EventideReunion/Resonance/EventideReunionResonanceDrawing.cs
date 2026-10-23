namespace Anomalies.SpecialContents.Developer.EventideReunion;

internal static class EventideReunionResonanceDrawing
{
    private static Color GetAspectColor(EventideReunionAspect aspect) => aspect switch
    {
        EventideReunionAspect.Sun => new Color(255, 155, 35),
        EventideReunionAspect.Samsara => new Color(125, 110, 255),
        EventideReunionAspect.Life => new Color(65, 235, 115),
        EventideReunionAspect.Death => new Color(170, 65, 225),
        EventideReunionAspect.Sacredness => new Color(255, 240, 175),
        EventideReunionAspect.Star => new Color(85, 155, 255),
        _ => Color.White
    };

    public static void EmitSparkles(Vector2 center, float radius, float scale)
    {
        if (!Main.rand.NextBool(3))
            return;
        Vector2 offset = Main.rand.NextVector2Circular(radius * 0.85f, radius * 0.85f);
        ParticleHandler.SpawnParticle(new GenericSparkle(center + offset,
            offset.SafeNormalize(Vector2.UnitY) * 0.25f + new Vector2(0f, -0.75f), Color.White,
            Color.Gold, Main.rand.NextFloat(0.6f, 0.9f) * scale, Main.rand.Next(30, 43),
            rotationSpeed: 0.025f, bloomScale: 1.65f));
    }

    public static void DrawGlow(SpriteBatch batch, Texture2D rune, Vector2 center,
        float runeScale, float opacity, EventideReunionAspect aspect)
    {
        float breath = 0.5f + 0.5f * MathF.Sin(Main.GlobalTimeWrappedHourly * MathHelper.TwoPi / 2.4f);
        Color color = GetAspectColor(aspect);
        color.A = 0;
        Vector2 position = center - Main.screenPosition;
        Texture2D bloom = ParticleHandler.GetTexture<BloomParticle>();
        float diameter = Math.Max(rune.Width, rune.Height) * runeScale;
        batch.Draw(bloom, position, null, color * (opacity * MathHelper.Lerp(0.3f, 0.6f, breath)),
            0f, bloom.Size() * 0.5f, diameter * MathHelper.Lerp(1.8f, 2.15f, breath) / bloom.Width, SpriteEffects.None, 0f);

        // 沿符文轮廓外扩柔光，中心由随后绘制的原符文保持清晰。
        float spread = diameter * MathHelper.Lerp(0.035f, 0.065f, breath);
        for (int i = 0; i < 8; i++)
        {
            Vector2 offset = (MathHelper.TwoPi * i / 8f).ToRotationVector2() * spread;
            batch.Draw(rune, position + offset, null, color * (opacity * MathHelper.Lerp(0.1f, 0.19f, breath)),
                0f, rune.Size() * 0.5f, runeScale, SpriteEffects.None, 0f);
        }
    }
}
