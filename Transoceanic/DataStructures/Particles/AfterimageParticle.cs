// Developed by ColdsUx

using Transoceanic.DataStructures.Assets;

namespace Transoceanic.DataStructures.Particles;

public class AfterimageParticle : Particle, IContentLoader
{
    public new Texture2D Texture;
    public Rectangle? Frame;
    public float OriginalOpacity = 1f;
    public float Opacity = 1f;
    public Vector2 DrawOffset;

    public override bool AutoLoadTexture => false;
    public override string TexturePath => TOTextures.InvisibleTexturePath;

    public AfterimageParticle(Texture2D texture, Rectangle? frame, Vector2 center, int lifetime, float rotation, float scale, Color color, float originalOpacity = 1f, Vector2? drawOffset = null, bool affectedByLight = true)
    {
        Texture = texture;
        Frame = frame;
        Center = center;
        Lifetime = lifetime;
        Rotation = rotation;
        Scale = scale;
        Color = color;
        OriginalOpacity = originalOpacity;
        DrawOffset = drawOffset ?? Vector2.Zero;
        AffectedByLight = affectedByLight;
    }

    public override bool PreSpawn() => false;

    public override void Update()
    {
        Opacity = OriginalOpacity * 0.65f * TOMathUtils.Interpolation.QuadraticEaseOut(1f - LifetimeCompletion);
    }

    public override bool PreDraw(SpriteBatch spriteBatch) => false;

    public void Draw(SpriteBatch spriteBatch)
    {
        Color color = Color * Opacity;
        if (AffectedByLight)
            color = Lighting.GetColor(Center.ToTileCoordinates()).MultiplyRGBA(color);
        spriteBatch.DrawFromCenter(Texture, Center + DrawOffset - Main.screenPosition, Frame, color, Rotation, Scale, SpriteEffects.None);
    }
}