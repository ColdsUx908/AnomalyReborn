namespace Transoceanic.Graphics.Particles.BuiltIn;

/// <summary>沿预采样路径移动、由调用者指定绘制顺序的残影。</summary>
public class PathAfterimageParticle : AfterimageParticle
{
    private readonly IReadOnlyList<Vector2> _Path;
    public Vector2 DrawScale;
    public float StartFadeRatio;
    public override bool AutoUpdatePosition => false;

    public PathAfterimageParticle(Texture2D texture, IReadOnlyList<Vector2> path, float rotation, Vector2 scale,
        Rectangle? frame = null, float startFadeRatio = 0f)
        : base(texture, frame, path is { Count: > 0 } ? path[0] : throw new ArgumentException("Path must contain at least one point.", nameof(path)),
            path.Count, rotation, 1f, Color.White)
    {
        _Path = path;
        DrawScale = scale;
        StartFadeRatio = startFadeRatio;
        Important = true;
        Opacity = 0.5f;
    }

    public override bool PreSpawn() => true;

    public override void Update()
    {
        int index = Timer - 1;
        if (index >= _Path.Count) { Kill(); return; }
        Center = _Path[Math.Max(index, 0)];
        float completion = index / (float)Lifetime;
        float fade = StartFadeRatio > 0f ? Utils.GetLerpValue(StartFadeRatio, 1f, completion, true) : completion;
        Opacity = OriginalOpacity * 0.5f * (1f - TOMathUtils.Interpolation.SineEaseIn(fade));
    }

    public new void Draw(SpriteBatch spriteBatch)
    {
        Color color = Color * Opacity;
        if (AffectedByLight) color.MultiplyWithWorldLight(Center);
        spriteBatch.Draw(Texture, Center + DrawOffset - Main.screenPosition, Frame, color, Rotation,
            (Frame?.Size() ?? Texture.Size()) / 2f, DrawScale, SpriteEffects.None, 0f);
    }
}
