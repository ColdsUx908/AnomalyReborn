namespace Transoceanic.Primitives.Mathematics;

/// <summary>
/// 任意阶二维 Bézier 曲线，使用 de Casteljau 算法求值。
/// </summary>
public sealed class BezierCurve
{
    private readonly Vector2[] _ControlPoints;

    /// <summary>
    /// 使用给定控制点创建 Bézier 曲线。
    /// 控制点按曲线定义顺序排列；调用 <see cref="Evaluate(float)"/> 时会依据参数值计算曲线位置。
    /// </summary>
    public BezierCurve(params Vector2[] controlPoints)
    {
        if (controlPoints is not { Length: > 0 }) throw new ArgumentException("At least one control point is required.", nameof(controlPoints));
        _ControlPoints = (Vector2[])controlPoints.Clone();
    }

    /// <summary>
    /// 用 de Casteljau 插值计算曲线上的点；参数限制在 [0, 1]，两端分别返回首末控制点。
    /// </summary>
    public Vector2 Evaluate(float interpolant)
    {
        float ratio = Math.Clamp(interpolant, 0f, 1f);
        Span<Vector2> points = _ControlPoints.Length <= 64 ? stackalloc Vector2[_ControlPoints.Length] : new Vector2[_ControlPoints.Length];
        _ControlPoints.AsSpan().CopyTo(points);
        for (int count = points.Length - 1; count > 0; count--)
            for (int i = 0; i < count; i++) points[i] = Vector2.Lerp(points[i], points[i + 1], ratio);
        return points[0];
    }

    /// <summary>
    /// 将曲线划分为指定数量的区间，返回包含两个端点的 intervalCount + 1 个采样点。
    /// </summary>
    public List<Vector2> GetPoints(int intervalCount)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(intervalCount);
        List<Vector2> points = [with(intervalCount + 1)];
        for (int i = 0; i <= intervalCount; i++) points.Add(Evaluate(i / (float)intervalCount));
        return points;
    }
}
