// Designed by ColdsUx

using Transoceanic.DataStructures.Assets;

namespace Transoceanic.DataStructures.Particles;

public class CustomSpriteParticle : Particle
{
    public new Texture2D Texture;
    public float MaxGravity;
    public float Opacity = 1f;
    public SpriteEffects SpriteEffects;

    public Func<CustomSpriteParticle, Rectangle?> CustomGetFrameAction;
    public Action<CustomSpriteParticle> CustomUpdateAction;

    public const int ExtraDataSlots = 8;
    public Union32[] ExtraData = new Union32[ExtraDataSlots];

    public override bool AutoLoadTexture => false;
    public override string TexturePath => TOTextures.InvisibleTexturePath;
    public override BlendState DrawBlendState { get; }

    public CustomSpriteParticle(Texture2D texture, Vector2 center, Vector2 velocity, int lifetime, float rotation, float scale, Color color, float maxGravity = 0f, BlendState blendState = null, SpriteEffects spriteEffects = SpriteEffects.None,
        Func<CustomSpriteParticle, Rectangle?> customGetFrameAction = null, Action<CustomSpriteParticle> customUpdateAction = null)
    {
        Texture = texture;
        Center = center;
        Velocity = velocity;
        Lifetime = lifetime;
        Rotation = rotation;
        Scale = scale;
        Color = color;
        MaxGravity = maxGravity;
        DrawBlendState = blendState ?? BlendState.AlphaBlend;
        SpriteEffects = spriteEffects;
        CustomGetFrameAction = customGetFrameAction;
        CustomUpdateAction = customUpdateAction;
    }

    public CustomSpriteParticle(string texturePath, Vector2 center, Vector2 velocity, int lifetime, float rotation, float scale, Color color, float maxGravity = 0f, BlendState blendState = null, SpriteEffects spriteEffects = SpriteEffects.None,
        Func<CustomSpriteParticle, Rectangle?> customGetFrameAction = null, Action<CustomSpriteParticle> customUpdateAction = null)
        : this(ModContent.Request<Texture2D>(texturePath).Value, center, velocity, lifetime, rotation, scale, color, maxGravity, blendState, spriteEffects, customGetFrameAction, customUpdateAction)
    { }

    public override void Update()
    {
        if (Timer > Lifetime - 20)
        {
            Scale *= 0.9f;
            Opacity *= 0.9f;
        }

        Velocity *= 0.85f;
        if (MaxGravity != 0f)
        {
            if (Velocity.Length() < MaxGravity)
            {
                Velocity.X *= 0.94f;
                Velocity.Y += MaxGravity / 10f;
            }
        }

        CustomUpdateAction?.Invoke(this);
    }

    public override bool PreDraw(SpriteBatch spriteBatch)
    {
        spriteBatch.DrawFromCenter(Texture, Center - Main.screenPosition, CustomGetFrameAction?.Invoke(this), Color * Opacity, Rotation, Scale, SpriteEffects.None);
        return false;
    }
}
