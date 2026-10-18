namespace Anomalies.Bosses.BrainofCthulhu;

// The encounter's seed-specific texture selection stays outside the generic pulse API.
public sealed class CustomPulse : TexturePulseParticle
{
    public CustomPulse(Vector2 position, Vector2 velocity, Color color, string texture, Vector2 squish, float rotation, float originalScale, float finalScale, int lifeTime, bool useAdditiveBlend = true, float baseOpacity = 1f, bool fade = true, float makeLight = 1f, SpriteEffects effects = SpriteEffects.None) : base(position, velocity, color, texture, squish, rotation, originalScale, finalScale, lifeTime, useAdditiveBlend, baseOpacity, fade, makeLight, effects)
    {
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 drawOffset = default)
    {
        Asset<Texture2D> originalAsset = SpriteAsset;
        Vector2 originalSquish = Squish;
        if (Main.zenithWorld && DateTime.Now.DayOfWeek == DayOfWeek.Tuesday)
        {
            SpriteAsset = ModContent.Request<Texture2D>("Anomalies/Bosses/BrainofCthulhu/Assets/Particles/MammothParticle");
            Squish *= MathHelper.Lerp(originalAsset.Value.Width / (float)SpriteAsset.Value.Width, originalAsset.Value.Height / (float)SpriteAsset.Value.Height, 0.5f);
        }

        bool result = base.PreDraw(spriteBatch, drawOffset);
        SpriteAsset = originalAsset;
        Squish = originalSquish;
        return result;
    }
}
