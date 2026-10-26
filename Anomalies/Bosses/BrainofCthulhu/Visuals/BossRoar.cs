namespace Anomalies.Bosses.BrainofCthulhu;

public class BossRoar : Particle
{
    public override string TexturePath => "Anomalies/Bosses/BrainofCthulhu/Assets/RoarPulse";

    private float _OriginalScale;
    private float _FinalScale;
    private float _BaseOpacity;
    private float _Opacity;
    private Color _BaseColor;
    public BossRoar(Vector2 position, Color color, float rotation, float originalScale, float finalScale, int lifeTime, float baseOpacity = 1f)
    {
        Center = position;
        _BaseColor = color;
        _OriginalScale = originalScale;
        _FinalScale = finalScale;
        Scale = originalScale;
        Lifetime = lifeTime;
        _BaseOpacity = baseOpacity;
        Rotation = rotation;
    }

    public override void Update()
    {
        Scale = MathHelper.Lerp(_OriginalScale, _FinalScale, LifetimeCompletion);
        _Opacity = 1f;
        if (LifetimeCompletion < 0.1f)
            _Opacity = MathHelper.Lerp(0f, 1f, LifetimeCompletion * 10);
        Color = _BaseColor * _Opacity;
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 drawOffset = default)
    {
        Texture2D tex = Texture;
        spriteBatch.Draw(tex, Center + drawOffset - Main.screenPosition, null, Color * _BaseOpacity, Rotation, tex.Size() / 2f, Scale, SpriteEffects.None, 0);
        return false;
    }
}
