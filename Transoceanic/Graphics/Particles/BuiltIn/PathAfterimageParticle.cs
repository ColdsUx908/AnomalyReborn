namespace Transoceanic.Graphics.Particles.BuiltIn;

/// <summary>
/// 沿预采样路径移动、由调用者指定绘制顺序的残影。
/// </summary>
public class PathAfterimageParticle : AfterimageParticle
{
    private readonly IReadOnlyList<Vector2> _Path;
    /// <summary>
    /// 路径残影绘制的双轴缩放，不改变路径坐标。
    /// </summary>
    public Vector2 DrawScale;
    /// <summary>
    /// 路径残影开始淡出的寿命比例；不大于零时从路径起点开始淡出。
    /// </summary>
    public float StartFadeRatio;
    /// <inheritdoc/>
    /// <remarks>
    /// 是否在自定义更新后自动累加速度；false 时由具体类型自行定位。
    /// </remarks>
    public override bool AutoUpdatePosition => false;

    /// <summary>
    /// 创建沿非空路径逐帧移动的残影，寿命等于路径点数；保留路径引用，调用方应保持路径稳定，startFadeRatio 使用寿命比例。
    /// </summary>
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

    /// <inheritdoc/>
    /// <remarks>
    /// 返回 true，允许提交路径残影；本类型沿用残影的自定义 Draw，默认绘制仍被跳过。
    /// </remarks>
    public override bool PreSpawn() => true;

    /// <inheritdoc/>
    /// <remarks>
    /// 按 Timer - 1 选取路径点并更新淡出；超出路径末端时标记死亡。
    /// </remarks>
    public override void Update()
    {
        int index = Timer - 1;
        if (index >= _Path.Count) { Kill(); return; }
        Center = _Path[Math.Max(index, 0)];
        float completion = index / (float)Lifetime;
        float fade = StartFadeRatio > 0f ? Utils.GetLerpValue(StartFadeRatio, 1f, completion, true) : completion;
        Opacity = OriginalOpacity * 0.5f * (1f - TOMathUtils.Interpolation.SineEaseIn(fade));
    }

    /// <summary>
    /// 以双轴 DrawScale 绘制当前路径点残影，并按需应用世界光照。
    /// </summary>
    public new void Draw(SpriteBatch spriteBatch)
    {
        Color color = Color * Opacity;
        if (AffectedByLight) color.MultiplyWithWorldLight(Center);
        spriteBatch.Draw(Texture, Center + DrawOffset - Main.screenPosition, Frame, color, Rotation,
            (Frame?.Size() ?? Texture.Size()) / 2f, DrawScale, SpriteEffects.None, 0f);
    }
}
