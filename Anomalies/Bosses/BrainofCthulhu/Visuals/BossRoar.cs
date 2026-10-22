namespace Anomalies.Bosses.BrainofCthulhu;

public class BossRoar : Particle
{
    public override string TexturePath => "Anomalies/Bosses/BrainofCthulhu/Assets/RoarPulse";

    private float OriginalScale;
    private float FinalScale;
    private float BaseOpacity;
    private float opacity;
    private Color BaseColor;
    public BossRoar(Vector2 position, Color color, float rotation, float originalScale, float finalScale, int lifeTime, float baseOpacity = 1f)
    {
        Center = position;
        BaseColor = color;
        OriginalScale = originalScale;
        FinalScale = finalScale;
        Scale = originalScale;
        Lifetime = lifeTime;
        BaseOpacity = baseOpacity;
        Rotation = rotation;
    }

    public override void Update()
    {
        Scale = MathHelper.Lerp(OriginalScale, FinalScale, LifetimeCompletion);
        opacity = 1f;
        if (LifetimeCompletion < 0.1f)
            opacity = MathHelper.Lerp(0f, 1f, LifetimeCompletion * 10);
        Color = BaseColor * opacity;
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 drawOffset = default)
    {
        Texture2D tex = Texture;
        spriteBatch.Draw(tex, Center + drawOffset - Main.screenPosition, null, Color * BaseOpacity, Rotation, tex.Size() / 2f, Scale, SpriteEffects.None, 0);
        return false;
    }
}
