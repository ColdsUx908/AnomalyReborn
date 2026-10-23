using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Anomalies.SpecialContents.Developer.EventideReunion;

internal static class EventideReunionSamsaraVisualGeometry
{
    internal const float PullbackDistance = 48f;
    internal const float ThrustExtension = 180f;
    internal const float GhostExtension = 280f;
    internal const float ThrustWidth = 42f;

    internal static float ThrustReach(float progress)
    {
        float t = Math.Clamp(progress, 0f, 1f);
        if (t < 0.3f)
        {
            float burst = 1f - MathF.Pow(1f - t / 0.3f, 3f);
            return MathHelper.Lerp(-PullbackDistance, ThrustExtension, burst);
        }
        return MathHelper.Lerp(ThrustExtension, 0f, MathHelper.SmoothStep(0f, 1f, (t - 0.55f) / 0.45f));
    }

    internal static float ThrustGlow(float progress)
        => MathHelper.SmoothStep(0f, 1f, progress / 0.18f)
            * (1f - MathHelper.SmoothStep(0f, 1f, (progress - 0.75f) / 0.25f));

    internal static float ThrustTipDistance(float progress)
        => 219.91f + ThrustReach(progress) + GhostExtension * ThrustGlow(progress);

    internal static Vector2 SpinVector(float progress, int facing)
    {
        float angle = (facing > 0 ? MathHelper.Pi + 0.15f : -0.15f)
            + MathHelper.TwoPi * facing * MathHelper.SmoothStep(0f, 1f, progress);
        return new Vector2(MathF.Cos(angle), MathF.Sin(angle));
    }

    // 固定倾斜平面上的真实圆弧投影；不能在挥动途中改变平面倾角，否则轨迹会折弯。
    internal static Vector2 TurnVector(float progress, float startAngle, Vector2 aim, float gravity)
    {
        float t = MathHelper.SmoothStep(0f, 1f, Math.Clamp(progress, 0f, 1f));
        float aimAngle = MathF.Atan2(aim.Y, aim.X);
        float delta = MathHelper.WrapAngle(startAngle - aimAngle);
        float tilt = -1.05f * gravity * Math.Abs(MathF.Cos(delta));
        // 反投影起手方向，保持横抽首帧与旋身结束的矛尖完全一致。
        float start = MathF.Atan2(MathF.Sin(delta) / MathF.Cos(tilt), MathF.Cos(delta));
        int swingSign = MathF.Cos(startAngle) < 0f ? 1 : -1;
        if (Math.Abs(start) > 0.0001f)
        {
            if (swingSign > 0 && start > 0f)
                start -= MathHelper.TwoPi;
            else if (swingSign < 0 && start < 0f)
                start += MathHelper.TwoPi;
        }
        Vector3 startLocal = Vector3.Transform(new Vector3(MathF.Cos(start), MathF.Sin(start), 0f), Matrix.CreateRotationX(tilt));
        float startRadius = 900f / (900f * new Vector2(startLocal.X, startLocal.Y).Length() + startLocal.Z * 230f);
        float angle = start * (1f - t);
        Vector3 local = new Vector3(MathF.Cos(angle), MathF.Sin(angle), 0f) * MathHelper.Lerp(startRadius, 1f, t);
        local = Vector3.Transform(local, Matrix.CreateRotationX(tilt));
        local = Vector3.Transform(local, Matrix.CreateRotationZ(aimAngle));
        return new Vector2(local.X, local.Y) * (900f / (900f - local.Z * 230f));
    }

    internal static Vector2 AfterimageOffset(int index, float width, float height, float scale)
    {
        float angle = index * MathHelper.TwoPi / 6f - MathHelper.PiOver2;
        // 上下残影的身体边缘保留 9 像素间隙；缩小时仍按两具身体的半高计算。
        return new Vector2(MathF.Cos(angle) * (width * (1f + scale) * 0.5f + 30f),
            MathF.Sin(angle) * (height * (1f + scale) * 0.5f + 9f));
    }
}

// 仅客户端、每次施法独立保存的世界坐标轨迹；收招之前不移除旧采样。
internal sealed class EventideReunionSamsaraTrail
{
    private const int MaximumSamples = 4096;
    internal List<Vector2> Points { get; } = [];
    internal List<float> Distances { get; } = [];
    internal bool Frozen { get; private set; }
    private Vector2 _Grip;
    private float _Angle;
    private float _Reach;

    internal void Freeze() => Frozen = true;

    internal (int Segment, float Fraction) RetractionStart(float progress)
    {
        if (Points.Count < 2)
            return (0, 0f);
        float distance = Distances[^1] * Math.Clamp(progress, 0f, 1f);
        int index = Distances.BinarySearch(distance);
        if (index >= 0)
            return (index, 0f);
        int segment = Math.Max(0, ~index - 1);
        return (segment, (distance - Distances[segment]) / (Distances[segment + 1] - Distances[segment]));
    }

    // 从矛尖往后 220 像素逐渐展开，超过该距离保持完整线束宽度。
    internal static float FanOut(float distanceBehindTip)
        => MathHelper.SmoothStep(0f, 1f, Math.Clamp(distanceBehindTip / 220f, 0f, 1f));

    internal Vector2 StrandPoint(int index, int strand, float extraHeadLength = 0f)
        => StrandAtDistance(Distances[index], strand, extraHeadLength);

    internal Vector2 StrandAtDistance(float distance, int strand, float extraHeadLength = 0f)
    {
        distance = Math.Clamp(distance, 0f, Distances[^1]);
        Vector2 tangent = PointAtDistance(distance + 6f) - PointAtDistance(distance - 6f);
        if (tangent.LengthSquared() < 0.01f)
            tangent = PointAtDistance(distance) - PointAtDistance(distance - 6f);
        tangent = tangent.LengthSquared() > 0.0001f ? Vector2.Normalize(tangent) : Vector2.UnitX;
        float weave = (strand - 2.5f) * 8f + MathF.Sin(distance / 28f + strand * 0.9f) * 1.8f;
        return PointAtDistance(distance) + new Vector2(-tangent.Y, tangent.X) * weave
            * FanOut(Distances[^1] + extraHeadLength - distance);
    }

    internal Vector2 PointAtDistance(float distance)
    {
        if (distance <= 0f)
            return Points[0];
        if (distance >= Distances[^1])
            return Points[^1];
        int index = Distances.BinarySearch(distance);
        if (index >= 0)
            return Points[index];
        int next = ~index;
        return Vector2.Lerp(Points[next - 1], Points[next],
            (distance - Distances[next - 1]) / (Distances[next] - Distances[next - 1]));
    }

    // 对实际矛尖函数直接补点，不将 3D 投影重新解释为二维极角/半径。
    internal void AppendCurve(Func<float, Vector2> sampleTip, float from, float to)
    {
        if (Frozen)
            return;
        int steps = Math.Max(1, (int)MathF.Ceiling(Math.Abs(to - from) * 512f));
        for (int i = 0; i <= steps && Points.Count < MaximumSamples; i++)
        {
            Vector2 point = sampleTip(MathHelper.Lerp(from, to, i / (float)steps));
            float length = Points.Count == 0 ? 0f : Vector2.Distance(Points[^1], point);
            if (Points.Count > 0 && length < 0.0001f)
                continue;
            // 保留精确矛尖作为活动端点；不足 2 像素的末段合并，避免积累退化三角形。
            if (Points.Count >= 2 && Distances[^1] - Distances[^2] < 2f)
            {
                float segment = Vector2.Distance(Points[^2], point);
                if (segment < 0.0001f)
                {
                    Points.RemoveAt(Points.Count - 1);
                    Distances.RemoveAt(Distances.Count - 1);
                }
                else
                {
                    Points[^1] = point;
                    Distances[^1] = Distances[^2] + segment;
                }
            }
            else
            {
                Distances.Add(Points.Count == 0 ? 0f : Distances[^1] + length);
                Points.Add(point);
            }
        }
    }

    internal void Append(Vector2 grip, float angle, float reach)
    {
        if (Frozen || Points.Count >= MaximumSamples)
            return;
        Vector2 tip = grip + new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * reach;
        if (Points.Count == 0)
        {
            Points.Add(tip);
            Distances.Add(0f);
        }
        else
        {
            float turn = MathHelper.WrapAngle(angle - _Angle);
            int steps = Math.Clamp((int)MathF.Ceiling(Math.Max(Math.Abs(turn) / 0.025f,
                Vector2.Distance(Points[^1], tip) / 6f)), 1, 128);
            for (int i = 1; i <= steps && Points.Count < MaximumSamples; i++)
            {
                float t = i / (float)steps;
                float sampleAngle = _Angle + turn * t;
                Vector2 point = Vector2.Lerp(_Grip, grip, t)
                    + new Vector2(MathF.Cos(sampleAngle), MathF.Sin(sampleAngle)) * MathHelper.Lerp(_Reach, reach, t);
                float length = Vector2.Distance(Points[^1], point);
                if (length < 0.25f)
                    continue;
                Distances.Add(Distances[^1] + length);
                Points.Add(point);
            }
        }
        _Grip = grip;
        _Angle = angle;
        _Reach = reach;
    }
}
