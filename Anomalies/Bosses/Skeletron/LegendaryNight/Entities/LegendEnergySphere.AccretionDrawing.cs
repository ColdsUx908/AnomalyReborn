using Anomalies.Assets.Effects;

namespace Anomalies.Bosses.Skeletron.LegendaryNight;

public sealed partial class LegendEnergySphere
{
    private Vector3[] _MatterStreamPath;
    private int _MatterStreamFrame = int.MinValue;
    private VertexPositionColorTexture[] _MatterRibbonVertices;
    private float _MatterStreamLength;

    internal Vector3[] BuildMatterStreamPath(int age)
    {
        if (_MatterStreamFrame == age && _MatterStreamPath is not null)
            return _MatterStreamPath;
        _MatterStreamFrame = age;
        PrepareCompanionMatter(CompanionStartMusicTime + age);
        const int Duration = CompanionImpactMusicTime - CompanionStartMusicTime;
        if (_CompanionMatter is null || age < 1 || age >= Duration + 60)
            return _MatterStreamPath = [];

        List<Vector3> points = new(128);
        if (age < Duration)
        {
            Vector3 donor = CompanionOrbitPosition(age);
            // 起点深入蓝星色球层，让宽阔外晕与物质包络自然连成一体。
            points.Add(donor - Vector3.Normalize(donor) * (SkeletronLegendHead.Mechanic6Data.SphereRadius * CompanionRadiusRatio * 0.4f));
        }
        for (int group = _CompanionMatter.Count - MatterStreams; group >= 0; group -= MatterStreams)
        {
            Vector3 position = Vector3.Zero;
            int count = 0;
            for (int lane = 0; lane < MatterStreams; lane++)
            {
                StellarMatterTrack track = _CompanionMatter[group + lane];
                int elapsed = age - track.BirthAge;
                if (elapsed < 0 || elapsed >= track.Positions.Length)
                    continue;
                position += track.Positions[elapsed];
                count++;
            }
            if (count > 0)
                points.Add(position / count);
        }
        if (points.Count < 3)
            return _MatterStreamPath = [];
        const float SurfaceRadius = SkeletronLegendHead.Mechanic6Data.SphereRadius;
        bool reachesSurface = points[^1].Length() < SurfaceRadius * 1.3f;
        if (reachesSurface)
            points.Add(Vector3.Normalize(points[^1]) * (SurfaceRadius * 0.94f));

        if (age < Duration)
        {
            // 将尚处在蓝星外晕内的紧密弯折整理成平滑喷口，后续中心线仍使用真实物质位置。
            Vector3 source = points[0];
            int join = 1;
            while (join < points.Count - 1 && Vector3.Distance(source, points[join]) < 190f)
                join++;
            if (join > 1)
            {
                Vector3 end = points[join];
                Vector3 tangent = Vector3.Normalize(points[Math.Min(join + 1, points.Count - 1)] - points[join - 1]);
                Vector3 firstControl = Vector3.Lerp(source, end, 0.35f);
                Vector3 secondControl = end - tangent * Math.Min(Vector3.Distance(source, end) * 0.25f, 65f);
                List<Vector3> launch = new(12);
                for (int step = 0; step < 10; step++)
                {
                    float t = step / 10f;
                    float remaining = 1f - t;
                    launch.Add(source * (remaining * remaining * remaining) + firstControl * (3f * remaining * remaining * t)
                        + secondControl * (3f * remaining * t * t) + end * (t * t * t));
                }
                points.RemoveRange(0, join);
                points.InsertRange(0, launch);
            }
        }
        // 相邻出生时刻构成同一时刻的流体中心线，保留公转/内落，消除三股采样的细碎跳动。
        for (int pass = 0; pass < 2; pass++)
        {
            Vector3[] previous = points.ToArray();
            for (int i = 1; i < points.Count - 1; i++)
                points[i] = (previous[i - 1] + previous[i] * 2f + previous[i + 1]) * 0.25f;
        }
        List<Vector3> samples = new(512);
        for (int i = 0; i < points.Count - 1; i++)
        {
            Vector3 start = points[i];
            Vector3 end = points[i + 1];
            int steps = Math.Clamp((int)MathF.Ceiling(Vector3.Distance(start, end) / 18f), 1, 80);
            for (int step = 0; step < steps; step++)
                samples.Add(Vector3.CatmullRom(points[Math.Max(0, i - 1)], start, end,
                    points[Math.Min(points.Count - 1, i + 2)], step / (float)steps));
        }
        samples.Add(points[^1]);
        // 统一弧长间距，避免出口的密集采样造成带宽翻转和重复叠亮。
        List<Vector3> uniform = new(samples.Count) { samples[0] };
        float remainingDistance = 14f;
        Vector3 cursor = samples[0];
        for (int i = 1; i < samples.Count; i++)
        {
            Vector3 target = samples[i];
            float distance = Vector3.Distance(cursor, target);
            while (distance >= remainingDistance)
            {
                cursor = Vector3.Lerp(cursor, target, remainingDistance / distance);
                uniform.Add(cursor);
                distance = Vector3.Distance(cursor, target);
                remainingDistance = 14f;
            }
            remainingDistance -= distance;
            cursor = target;
        }
        if (Vector3.Distance(uniform[^1], samples[^1]) > 0.1f)
            uniform.Add(samples[^1]);
        return _MatterStreamPath = uniform.ToArray();
    }

    internal static float MatterStreamHalfWidth(float flow)
    {
        float throat = MathHelper.SmoothStep(0f, 1f, MathHelper.Clamp(flow / 0.13f, 0f, 1f));
        return MathHelper.Lerp(SkeletronLegendHead.Mechanic6Data.SphereRadius * CompanionRadiusRatio * 0.32f, 22f, throat)
            + 52f * MathF.Pow(flow, 5f);
    }

    internal VertexPositionColorTexture[] BuildMatterRibbonVertices(int age, bool behind, out int vertexCount)
    {
        Vector3[] path = BuildMatterStreamPath(age);
        int capacity = Math.Max(4096, path.Length * 12);
        if (_MatterRibbonVertices is null || _MatterRibbonVertices.Length < capacity)
            _MatterRibbonVertices = new VertexPositionColorTexture[capacity];
        vertexCount = 0;
        if (path.Length < 3)
            return _MatterRibbonVertices;
        float[] distances = new float[path.Length];
        for (int i = 1; i < path.Length; i++)
            distances[i] = distances[i - 1] + Vector3.Distance(path[i - 1], path[i]);
        float total = Math.Max(1f, distances[^1]);
        _MatterStreamLength = total / 260f;
        const int Duration = CompanionImpactMusicTime - CompanionStartMusicTime;
        float appearance = MathHelper.SmoothStep(0f, 1f, Math.Min(1f, age / 24f))
            * MathHelper.Clamp((Duration + 60 - age) / 60f, 0f, 1f);
        int written = 0;
        for (int i = 0; i < path.Length - 1; i++)
        {
            Vector3 a = path[i];
            Vector3 b = path[i + 1];
            float aDepth = Depth(a);
            float bDepth = Depth(b);
            Vector2 aNormal = NormalAt(i);
            Vector2 bNormal = NormalAt(i + 1);
            float start = distances[i];
            float end = distances[i + 1];
            if ((aDepth > 0f) != (bDepth > 0f))
            {
                float fraction = aDepth / (aDepth - bDepth);
                Vector3 split = Vector3.Lerp(a, b, fraction);
                float middle = MathHelper.Lerp(start, end, fraction);
                Vector2 middleNormal = Vector2.Lerp(aNormal, bNormal, fraction).SafeNormalize(aNormal);
                Append(a, split, start, middle, aDepth > 0f, aNormal, middleNormal);
                Append(split, b, middle, end, bDepth > 0f, middleNormal, bNormal);
            }
            else
                Append(a, b, start, end, aDepth > 0f, aNormal, bNormal);
        }
        vertexCount = written;
        return _MatterRibbonVertices;

        static float Depth(Vector3 position) => position.Z * MathF.Cos(CompanionCameraInclination)
            + position.Y * MathF.Sin(CompanionCameraInclination);

        Vector2 NormalAt(int index)
        {
            ProjectCompanion(path[Math.Max(0, index - 1)], out Vector2 previous, out _, out _);
            ProjectCompanion(path[Math.Min(path.Length - 1, index + 1)], out Vector2 next, out _, out _);
            Vector2 direction = (next - previous).SafeNormalize(Vector2.UnitX);
            return new Vector2(-direction.Y, direction.X);
        }

        float WidthLimit(float distance)
        {
            int index = Array.BinarySearch(distances, distance);
            if (index < 0)
                index = Math.Clamp(~index, 0, path.Length - 1);
            float limit = float.MaxValue;
            // 将曲率限制传播到相邻接缝，防止视线方向上的折返形成细碎扇形三角片。
            for (int sample = Math.Max(1, index - 2); sample <= Math.Min(path.Length - 2, index + 2); sample++)
            {
                ProjectCompanion(path[sample - 1], out Vector2 previous, out _, out _);
                ProjectCompanion(path[sample], out Vector2 current, out _, out _);
                ProjectCompanion(path[sample + 1], out Vector2 next, out _, out _);
                Vector2 incoming = current - previous;
                Vector2 outgoing = next - current;
                float dot = Vector2.Dot(incoming.SafeNormalize(Vector2.UnitX), outgoing.SafeNormalize(Vector2.UnitX));
                float sine = MathF.Sqrt(Math.Max(0f, (1f - MathHelper.Clamp(dot, -1f, 1f)) * 0.5f));
                float curvatureRadius = (incoming.Length() + outgoing.Length()) * 0.25f / Math.Max(sine, 0.001f);
                limit = Math.Min(limit, Math.Max(0.8f, curvatureRadius * 0.65f));
            }
            return limit;
        }
        void Append(Vector3 a, Vector3 b, float start, float end, bool segmentBehind, Vector2 aNormal, Vector2 bNormal)
        {
            if (segmentBehind != behind || !ProjectCompanion(a, out Vector2 aCenter, out float aScale, out float aOpacity)
                || !ProjectCompanion(b, out Vector2 bCenter, out float bScale, out float bOpacity))
                return;
            float aFlow = start / total;
            float bFlow = end / total;
            float aWidth = Math.Min(MatterStreamHalfWidth(aFlow) * Math.Min(aScale, 6f) * 3f, WidthLimit(start));
            float bWidth = Math.Min(MatterStreamHalfWidth(bFlow) * Math.Min(bScale, 6f) * 3f, WidthLimit(end));
            Color aColor = StreamColor(aFlow, aOpacity);
            Color bColor = StreamColor(bFlow, bOpacity);
            VertexPositionColorTexture aLeft = new(new Vector3(aCenter - aNormal * aWidth, Depth(a)), aColor, new(start / 260f, 0f));
            VertexPositionColorTexture aRight = new(new Vector3(aCenter + aNormal * aWidth, Depth(a)), aColor, new(start / 260f, 1f));
            VertexPositionColorTexture bLeft = new(new Vector3(bCenter - bNormal * bWidth, Depth(b)), bColor, new(end / 260f, 0f));
            VertexPositionColorTexture bRight = new(new Vector3(bCenter + bNormal * bWidth, Depth(b)), bColor, new(end / 260f, 1f));
            _MatterRibbonVertices[written++] = aLeft;
            _MatterRibbonVertices[written++] = aRight;
            _MatterRibbonVertices[written++] = bLeft;
            _MatterRibbonVertices[written++] = aRight;
            _MatterRibbonVertices[written++] = bRight;
            _MatterRibbonVertices[written++] = bLeft;
        }

        Color StreamColor(float flow, float opacity)
        {
            float heat = MathHelper.SmoothStep(0f, 1f, MathHelper.Clamp((flow - 0.3f) / 0.7f, 0f, 1f));
            Color color = Color.Lerp(new Color(65, 145, 255), new Color(255, 190, 65), heat);
            // 首尾位于星面内部，不在中途做稀疏闪点或沿轴打断密度。
            color.A = (byte)(255f * opacity * appearance);
            return color;
        }
    }

    internal void DrawCompanionMatter(SpriteBatch spriteBatch, bool behind)
    {
        int age = Head.Phase2MusicElapsedTime - CompanionStartMusicTime;
        VertexPositionColorTexture[] vertices = BuildMatterRibbonVertices(age, behind, out int count);
        if (count == 0)
            return;
        AnomalyEffects.MiscShaderContainer shader = AnomalyClientConfig.Instance.EnableShaders
            ? AnomalyEffects.Primitives.LegendStellarAccretion : null;
        if (shader is null)
        {
            DrawMatterRibbonFallback(spriteBatch, vertices, count);
            return;
        }
        spriteBatch.End();
        GraphicsDevice device = Main.instance.GraphicsDevice;
        BlendState blend = device.BlendState;
        RasterizerState rasterizer = device.RasterizerState;
        DepthStencilState depth = device.DepthStencilState;
        SamplerState sampler = device.SamplerStates[1];
        try
        {
            device.BlendState = BlendState.AlphaBlend;
            device.RasterizerState = RasterizerState.CullNone;
            device.DepthStencilState = DepthStencilState.None;
            shader.Data.SetCustomParameter("uTransformMatrix",
                Matrix.CreateTranslation(Projectile.Center.X - Main.screenPosition.X, Projectile.Center.Y - Main.screenPosition.Y, 0f)
                * Main.GameViewMatrix.TransformationMatrix
                * Matrix.CreateOrthographicOffCenter(0f, Main.screenWidth, Main.screenHeight, 0f, -1f, 1f))
                .SetCustomParameter("uNebulaNoiseTexture", AnomalyTextures.Noise.Perlin2)
                .SetCustomParameter("uSphereRadius", SphereRadius)
                .SetCustomParameter("uStreamLength", _MatterStreamLength)
                .SetCustomParameter("uDetachedEnd", _MatterStreamPath[^1].Length() > SkeletronLegendHead.Mechanic6Data.SphereRadius * 1.3f ? 1f : 0f)
                .SetCustomParameter("uGlobalTime", age / 60f)
                .Apply();
            device.DrawUserPrimitives(PrimitiveType.TriangleList, vertices, 0, count / 3);
        }
        finally
        {
            device.BlendState = blend;
            device.RasterizerState = rasterizer;
            device.DepthStencilState = depth;
            device.SamplerStates[1] = sampler;
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
        }
    }

    private void DrawMatterRibbonFallback(SpriteBatch spriteBatch, VertexPositionColorTexture[] vertices, int count)
    {
        Texture2D bloom = ParticleHandler.GetTexture<BloomParticle>();
        for (int i = 0; i < count; i += 6)
        {
            Vector2 a = new(vertices[i].Position.X, vertices[i].Position.Y);
            Vector2 b = new(vertices[i + 1].Position.X, vertices[i + 1].Position.Y);
            Vector2 c = new(vertices[i + 2].Position.X, vertices[i + 2].Position.Y);
            Vector2 d = new(vertices[i + 4].Position.X, vertices[i + 4].Position.Y);
            Vector2 start = (a + b) * 0.5f;
            Vector2 end = (c + d) * 0.5f;
            float width = Vector2.Distance(a, b);
            Color color = vertices[i].Color;
            float opacity = color.A / 255f;
            Vector2 midpoint = (start + end) * 0.5f;
            float radial = midpoint.Length();
            float surfaceDepth = -MathF.Sqrt(Math.Max(0f, SphereRadius * SphereRadius - radial * radial));
            float streamDepth = (vertices[i].Position.Z + vertices[i + 2].Position.Z) * 0.5f;
            float contact = MathHelper.SmoothStep(0f, 1f, MathHelper.Clamp((surfaceDepth - streamDepth) / 16f, 0f, 1f));
            float outside = MathHelper.SmoothStep(0f, 1f, MathHelper.Clamp((radial - SphereRadius + 12f) / 12f, 0f, 1f));
            opacity *= MathHelper.Lerp(contact, 1f, outside);
            float cameraScale = Math.Max(0.01f, (CompanionCameraDistance + streamDepth) / CompanionCameraDistance);
            float distanceToStar = new Vector3(midpoint * cameraScale, streamDepth).Length();
            float surfaceHeat = 1f - MathHelper.SmoothStep(0f, 1f,
                MathHelper.Clamp((distanceToStar - SphereRadius - 24f) / 176f, 0f, 1f));
            color = Color.Lerp(color, new Color(255, 214, 122), surfaceHeat);
            opacity *= 1f + surfaceHeat * 0.65f;
            color.A = 0;
            spriteBatch.DrawFromCenter_VectorScale(bloom,
                Projectile.Center + (start + end) * 0.5f - Main.screenPosition, null, color * (opacity * 0.12f),
                (end - start).ToRotation(), new Vector2(Vector2.Distance(start, end) + width * 0.5f, width * 0.65f) / bloom.Size());
        }
    }
}
