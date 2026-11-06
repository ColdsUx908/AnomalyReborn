namespace Anomalies.Bosses.Skeletron.LegendaryNight;

/// <summary>
/// 传说之夜客户端视觉粒子。
/// </summary>
/// <remarks>
/// <para>数据槽位：不使用实体 ai、localAI 或网络槽位；Timer/Lifetime 为粒子框架本地生命周期，实例字段保存目标和轨迹参数，不同步。</para>
/// </remarks>
public sealed class LegendaryNightChargeSparkle : GenericSparkle
{
    private readonly Vector2 _Target;
    private readonly Vector2 _Offset;
    private readonly float _InitialScale;
    private readonly float _SpiralAngle;

    public override bool AutoUpdatePosition => false;

    public LegendaryNightChargeSparkle(Vector2 target, Vector2 offset, Color color, float scale, int lifetime, float spiralAngle = 1.2f)
        : base(target + offset, Vector2.Zero, color, LegendaryNightHandler.StarColor, scale, lifetime, 0.06f, 1.4f)
    {
        _Target = target;
        _Offset = offset;
        _InitialScale = scale;
        _SpiralAngle = spiralAngle;
    }

    public override void Update()
    {
        base.Update();
        float progress = MathHelper.Clamp(LifetimeCompletion, 0f, 1f);
        // 中段吸入、末段减速；径向与旋转一起缓停，配合星芒自带的淡出自然收束。
        float absorption = MathHelper.SmoothStep(0f, 1f, progress);
        float angle = absorption * _SpiralAngle;
        Vector2 rotated = new(_Offset.X * MathF.Cos(angle) - _Offset.Y * MathF.Sin(angle),
            _Offset.X * MathF.Sin(angle) + _Offset.Y * MathF.Cos(angle));
        Center = _Target + rotated * (1f - absorption);
        Scale = _InitialScale * (1f - 0.45f * absorption);
    }
}