namespace Transoceanic.Primitives.Mathematics;

/// <summary>
/// 任意阶二维 Bézier 曲线，使用 de Casteljau 算法求值，低阶曲线采用展开的插值路径。
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
        if (_ControlPoints.Length <= 4)
            return EvaluateLowDegree(ratio);
        Span<Vector2> workspace = _ControlPoints.Length <= 64 ? stackalloc Vector2[_ControlPoints.Length] : new Vector2[_ControlPoints.Length];
        return Evaluate(ratio, workspace);
    }

    private Vector2 EvaluateLowDegree(float ratio)
    {
        if (_ControlPoints.Length == 1)
            return _ControlPoints[0];
        Vector2 first = Vector2.Lerp(_ControlPoints[0], _ControlPoints[1], ratio);
        if (_ControlPoints.Length == 2)
            return first;
        Vector2 second = Vector2.Lerp(_ControlPoints[1], _ControlPoints[2], ratio);
        if (_ControlPoints.Length == 3)
            return Vector2.Lerp(first, second, ratio);
        Vector2 third = Vector2.Lerp(_ControlPoints[2], _ControlPoints[3], ratio);
        return Vector2.Lerp(Vector2.Lerp(first, second, ratio), Vector2.Lerp(second, third, ratio), ratio);
    }

    private Vector2 Evaluate(float ratio, Span<Vector2> workspace)
    {
        _ControlPoints.AsSpan().CopyTo(workspace);
        for (int count = workspace.Length - 1; count > 0; count--)
            for (int i = 0; i < count; i++)
                workspace[i] = Vector2.Lerp(workspace[i], workspace[i + 1], ratio);
        return workspace[0];
    }

    /// <summary>
    /// 将曲线划分为指定数量的区间，返回包含两个端点的 intervalCount + 1 个采样点。
    /// 高阶曲线在本次采样内复用工作缓冲，不为每个采样点重新分配。
    /// </summary>
    public List<Vector2> GetPoints(int intervalCount)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(intervalCount);
        List<Vector2> points = [with(intervalCount + 1)];
        if (_ControlPoints.Length <= 4)
        {
            for (int i = 0; i <= intervalCount; i++)
                points.Add(EvaluateLowDegree(i / (float)intervalCount));
        }
        else
        {
            Span<Vector2> workspace = _ControlPoints.Length <= 64 ? stackalloc Vector2[_ControlPoints.Length] : new Vector2[_ControlPoints.Length];
            for (int i = 0; i <= intervalCount; i++)
                points.Add(Evaluate(i / (float)intervalCount, workspace));
        }
        return points;
    }
}
