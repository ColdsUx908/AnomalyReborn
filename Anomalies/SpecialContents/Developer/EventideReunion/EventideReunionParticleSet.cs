namespace Anomalies.SpecialContents.Developer.EventideReunion;

/// <summary>物品栏专用：沿矛身闪烁的星芒与向矛尖逸散的金色火花。</summary>
public sealed class EventideReunionParticleSet : ParticleSet
{
    public override int ParticleLifetime => 42;

    public EventideReunionParticleSet() : base(-1, 0.55f) { }

    public override Particle SpawnParticle()
    {
        Vector2 bladeDirection = new Vector2(1f, -1f).SafeNormalize(Vector2.UnitX);
        Vector2 sideDirection = bladeDirection.RotatedBy(MathHelper.PiOver2);
        Vector2 position = bladeDirection * Main.rand.NextFloat(-18f, 21f)
            + sideDirection * Main.rand.NextFloat(-7f, 7f);
        bool star = Main.rand.NextBool(3);
        Vector2 velocity = star
            ? sideDirection * Main.rand.NextFloat(-0.08f, 0.08f)
            : bladeDirection.RotatedBy(Main.rand.NextFloat(-0.7f, 0.7f)) * Main.rand.NextFloat(0.3f, 0.75f);
        return new EventideReunionInventoryParticle(position, velocity, star,
            star ? ParticleLifetime : Main.rand.Next(22, 33));
    }
}

public sealed class EventideReunionInventoryParticle : Particle
{
    public override string TexturePath => "Transoceanic/Assets/Textures/Particles/Sparkle";

    private readonly bool _Star;
    private readonly float _Size;

    public EventideReunionInventoryParticle(Vector2 position, Vector2 velocity, bool star, int lifetime)
    {
        Center = position;
        Velocity = velocity;
        Lifetime = lifetime;
        _Star = star;
        _Size = Main.rand.NextFloat(0.75f, 1.2f);
        Rotation = Main.rand.NextFloat(-0.25f, 0.25f);
        Color = Color.Lerp(new Color(255, 177, 48), new Color(255, 232, 150), Main.rand.NextFloat());
    }

    public override void Update()
    {
        Velocity *= _Star ? 0.96f : 0.98f;
        if (_Star)
            Rotation += 0.008f;
        else
            Velocity.Y += 0.008f;
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 drawOffset = default)
    {
        Vector2 position = Center + drawOffset - Main.screenPosition;
        float opacity = MathF.Sin(MathHelper.Pi * LifetimeCompletion);
        // 物品栏沿用 AlphaBlend；零 alpha 的预乘颜色提供加亮效果，无需切换 SpriteBatch。
        Color gold = Color with { A = 0 };
        Color core = new Color(255, 250, 222, 0);
        if (_Star)
        {
            Vector2 size = new Vector2(10f, 14f) * (_Size * opacity);
            Vector2 scale = size / Texture.Size();
            spriteBatch.Draw(Texture, position, null, gold * (opacity * 0.8f), Rotation,
                Texture.Size() * 0.5f, scale, SpriteEffects.None, 0f);
            spriteBatch.Draw(Texture, position, null, core * opacity, Rotation,
                Texture.Size() * 0.5f, scale * 0.5f, SpriteEffects.None, 0f);
        }
        else
        {
            float rotation = Velocity.ToRotation();
            Vector2 size = new Vector2(4.5f * _Size, 1.2f) * opacity;
            Texture2D pixel = TextureAssets.MagicPixel.Value;
            spriteBatch.Draw(pixel, position, null, gold * opacity, rotation,
                pixel.Size() * 0.5f, size / pixel.Size(), SpriteEffects.None, 0f);
            spriteBatch.Draw(pixel, position, null, core * opacity, rotation,
                pixel.Size() * 0.5f, size * new Vector2(0.55f, 0.5f) / pixel.Size(), SpriteEffects.None, 0f);
        }
        return false;
    }
}
