using Transoceanic.Graphics.Drawing;
using Transoceanic.Primitives.Mathematics;
using static Anomalies.Bosses.Skeletron.LegendaryNight.LegendStarburstVisuals;

namespace Anomalies.Bosses.Skeletron.LegendaryNight;

/// <summary>普通圆环星爆：内孔守护星、外扩亮环和依次绽放的星芒；游戏和 FNA 预览共用。</summary>
internal static class LegendStarburstRingVisuals
{
    public const int EndAge = 84;
    public const int ExpansionDuration = 9;
    private const int PopCount = 28;
    private const int SparkCount = 44;
    private const int OuterStarCount = 34;
    private const int LingerCount = 18;
    public const float ReferenceRadius = 750.4f;
    private static readonly Color _Gold = new(255, 185, 55);
    private static readonly Color _Light = new(255, 222, 120);
    private static readonly Color _Pale = new(255, 244, 205);

    public readonly record struct Textures(Texture2D Gradient, Texture2D ShotLine,
        Texture2D GlowDot, Texture2D Star4, Texture2D StarFlare);
    private readonly record struct Pop(float Angle, float Radius, float Delay, float Life, float Size, float Rotation);
    private readonly record struct Spark(float Angle, float Radius, float Travel, float Delay, float Life, float Size, bool Star, float Rotation);
    private readonly record struct OuterStar(float Angle, float Radius, float Size, float Delay);
    private readonly record struct Linger(float Angle, float Radius, float Life, float Size, bool Big, float Rotation);
    private static readonly Pop[] _Pops;
    private static readonly Spark[] _Sparks;
    private static readonly OuterStar[] _OuterStars;
    private static readonly Linger[] _Lingers;

    static LegendStarburstRingVisuals()
    {
        // 全部粒子依次消耗同一种子序列；调整顺序会改变既有配方。
        Mulberry32Random random = new(91);
        _Pops = new Pop[PopCount];
        for (int i = 0; i < _Pops.Length; i++)
            _Pops[i] = new((i + random.NextSingle() * 0.9f) / (float)PopCount * MathHelper.TwoPi,
                Lerp(0.12f, 0.92f, random.NextSingle()), random.NextSingle() * 4f, Lerp(16f, 24f, random.NextSingle()),
                Lerp(0.8f, 1.25f, random.NextSingle()), (random.NextSingle() - 0.5f) * 0.5f);
        _Sparks = new Spark[SparkCount];
        for (int i = 0; i < _Sparks.Length; i++)
            _Sparks[i] = new(random.NextSingle() * MathHelper.TwoPi, Lerp(0.05f, 0.7f, random.NextSingle()),
                Lerp(0.35f, 0.7f, random.NextSingle()), random.NextSingle() * 8f, Lerp(22f, 36f, random.NextSingle()),
                Lerp(7f, 14f, random.NextSingle()), random.NextSingle() < 0.3f, random.NextSingle() * MathHelper.Pi);
        _OuterStars = new OuterStar[OuterStarCount];
        for (int i = 0; i < _OuterStars.Length; i++)
            _OuterStars[i] = new((i + random.NextSingle() * 0.8f) / (float)OuterStarCount * MathHelper.TwoPi,
                Lerp(0.9f, 1.06f, random.NextSingle()), Lerp(10f, 20f, random.NextSingle()), random.NextSingle() * 6f);
        _Lingers = new Linger[LingerCount];
        for (int i = 0; i < _Lingers.Length; i++)
            _Lingers[i] = new(random.NextSingle() * MathHelper.TwoPi, Lerp(0.15f, 0.9f, random.NextSingle()),
                Lerp(28f, 44f, random.NextSingle()), Lerp(10f, 18f, random.NextSingle()), random.NextSingle() < 0.3f,
                (random.NextSingle() - 0.5f) * 0.4f);
    }

    public static bool IsVisible(float age) => age >= 0f && age < EndAge;
    public static float Front(float age, float innerRadius, float outerRadius) =>
        Lerp(innerRadius, outerRadius, OutExpo(Clamp(age / ExpansionDuration)));

    public static void Draw(SpriteBatch batch, Effect effect, in Textures textures,
        in WorldView2D view, in LegendStarburstVisuals.Burst burst, BlendState blend)
    {
        if (!IsVisible(burst.Age) || burst.OuterRadius <= burst.InnerRadius || burst.InnerRadius <= 0f)
            return;
        float age = burst.Age;
        float scale = burst.OuterRadius / ReferenceRadius;
        float inner = burst.InnerRadius / scale;
        float band = ReferenceRadius - inner;
        float front = Front(age, inner, ReferenceRadius);
        float frontU = (front - inner) / band;
        float boundary = Lerp(burst.SigilRadius * 1.05f / scale, inner, OutCubic(Clamp(age / 6f)));
        float innerAlpha = Smooth(Clamp(age / 2f)) * (1f - Smooth(Clamp((age - 34f) / 26f)));
        float flash = Math.Max(0f, 1f - age / 8f);
        Textures images = textures;
        Vector2 center = burst.Center - view.ScreenPosition;

        batch.Begin(SpriteSortMode.Deferred, blend, SamplerState.LinearClamp,
            DepthStencilState.None, RasterizerState.CullNone, null, view.Transform);
        DrawInnerBoundary();
        batch.End();

        // Only the expanding annulus is dynamic. Its fixed color profile is a packaged PNG.
        effect.Parameters["uInverseViewMatrix"].SetValue(Matrix.Invert(view.Transform));
        effect.Parameters["uScreenPosition"].SetValue(view.ScreenPosition);
        effect.Parameters["uResolution"].SetValue(view.Resolution);
        effect.Parameters["uCenter"].SetValue(burst.Center);
        effect.Parameters["uScale"].SetValue(scale);
        effect.Parameters["uInnerRadius"].SetValue(inner);
        effect.Parameters["uFront"].SetValue(front);
        effect.Parameters["uBody"].SetValue(Bell(Clamp(age / 34f), 0.08f));
        effect.Parameters["uBoundaryRadius"].SetValue(boundary);
        effect.Parameters["uBoundaryOpacity"].SetValue(innerAlpha * (0.8f + 1.5f * flash));
        effect.Parameters["uBoundaryAlpha"].SetValue(Clamp(innerAlpha * (0.8f + 1.5f * flash)));
        effect.Parameters["uPale"].SetValue(_Pale.ToVector3());
        effect.Parameters["uGradient"].SetValue(images.Gradient);
        batch.Begin(SpriteSortMode.Immediate, blend, SamplerState.LinearClamp,
            DepthStencilState.None, RasterizerState.CullNone, null, Matrix.Identity);
        effect.CurrentTechnique.Passes["Pass0"].Apply();
        batch.Draw(images.GlowDot, new Rectangle(0, 0, (int)view.Resolution.X, (int)view.Resolution.Y), Color.White);
        batch.End();

        batch.Begin(SpriteSortMode.Deferred, blend, SamplerState.LinearClamp,
            DepthStencilState.None, RasterizerState.CullNone, null, view.Transform);
        DrawFront();
        DrawPops();
        DrawSparks();
        DrawLingers();
        batch.End();

        void Sprite(Texture2D texture, Color color, Vector2 position, float width, float height, float rotation, float alpha)
        {
            SpriteDrawing.DrawSized(batch, texture, center + position * scale,
                new Vector2(width, height), color, rotation, alpha, scale);
        }
        void Glow(Texture2D texture, Color color, Vector2 position, float size, float rotation, float intensity)
        {
            SpriteDrawing.DrawGlow(batch, texture, center + position * scale,
                new Vector2(size), color, rotation, intensity, scale);
        }

        void Star(Vector2 position, float size, float rotation, float intensity, float diagonal)
        {
            Glow(images.StarFlare, _Light, position, size, rotation, intensity);
            Glow(images.StarFlare, _Light, position, size * diagonal, rotation + MathHelper.PiOver4, intensity * 0.7f);
        }
        void Streak(Color color, Vector2 start, Vector2 end, float width, float alpha)
        {
            Vector2 delta = end - start;
            float length = delta.Length();
            if (length >= 0.5f)
                Sprite(images.ShotLine, color, (start + end) * 0.5f, length * 1.25f, width,
                    MathF.Atan2(delta.Y, delta.X), alpha);
        }
        void DrawInnerBoundary()
        {
            const int Segments = 26;
            for (int i = 0; i < Segments; i++)
            {
                float angle = i / (float)Segments * MathHelper.TwoPi + age * 0.004f;
                float shimmer = 0.85f + 0.15f * MathF.Sin(i * 2.1f + age * 0.8f);
                Sprite(images.ShotLine, _Light, Direction(angle) * boundary, MathHelper.TwoPi * boundary / Segments * 2.2f,
                    16f + 10f * flash, angle + MathHelper.PiOver2, innerAlpha * shimmer * (1f + 1.2f * flash));
            }
            for (int i = 0; i < 6; i++)
            {
                float angle = i / 6f * MathHelper.TwoPi - age * 0.012f;
                float twinkle = 0.75f + 0.25f * MathF.Sin(age * 0.35f + i * 1.7f);
                Glow(images.Star4, _Light, Direction(angle) * boundary, 16f + 10f * flash, 0f,
                    (1.2f + flash) * innerAlpha * twinkle);
            }
        }
        void DrawFront()
        {
            float alpha = Smooth(Clamp(age / 2f)) * MathF.Pow(1f - Clamp((age - 12f) / 18f), 1.5f);
            const int Segments = 52;
            for (int i = 0; i < Segments; i++)
            {
                float angle = i / (float)Segments * MathHelper.TwoPi;
                float shimmer = 0.8f + 0.2f * MathF.Sin(i * 1.9f + age * 1.1f);
                Sprite(images.ShotLine, _Light, Direction(angle) * front, MathHelper.TwoPi * front / Segments * 2.1f,
                    24f, angle + MathHelper.PiOver2, alpha * shimmer);
            }
            foreach (OuterStar star in _OuterStars)
            {
                float a = age - 16f - star.Delay;
                if (a < 0f || a > 24f)
                    continue;
                float t = a / 24f;
                Vector2 position = Direction(star.Angle) * (ReferenceRadius * (star.Radius + 0.06f * t));
                position.Y -= 8f * t;
                Glow(images.Star4, _Light, position, star.Size, 0f, 1.5f * Bell(t, 0.2f));
            }
        }
        void DrawPops()
        {
            foreach (Pop pop in _Pops)
            {
                float a = age - (1f + pop.Radius * 9f + pop.Delay);
                if (a < 0f || a > pop.Life || pop.Radius > frontU + 0.02f)
                    continue;
                float t = a / pop.Life;
                float k = Bell(t, 0.12f);
                float radius = inner + band * pop.Radius;
                Vector2 direction = Direction(pop.Angle);
                Vector2 position = direction * radius;
                float size = (36f + 34f * (1f - pop.Radius)) * pop.Size * (0.7f + 0.5f * OutCubic(t));
                Glow(images.GlowDot, _Gold, position, size * 0.9f, 0f, 1.3f * k);
                Star(position, size * 1.6f, pop.Rotation, 1.5f * k, 0.4f);
                float head = radius + band * 0.22f * OutCubic(t);
                Streak(_Light, direction * (head - band * 0.14f), direction * head, 12f, k * 0.9f);
            }
        }
        void DrawSparks()
        {
            foreach (Spark spark in _Sparks)
            {
                float a = age - 2f - spark.Delay;
                if (a < 0f || a > spark.Life || spark.Radius > frontU + 0.02f)
                    continue;
                float t = a / spark.Life;
                float radius = inner + band * spark.Radius + band * spark.Travel * OutQuart(t);
                Vector2 direction = Direction(spark.Angle);
                Vector2 position = direction * radius;
                position.Y += 26f * t * t;
                float k = MathF.Pow(1f - t, 1.2f);
                if (spark.Star)
                    Glow(images.Star4, _Light, position, spark.Size * 2f, spark.Rotation + a * 0.05f, 1.3f * k);
                else
                {
                    float speed = band * spark.Travel * 4f * MathF.Pow(1f - t, 3f) / spark.Life;
                    if (speed > 0.8f)
                        Streak(_Gold, position - direction * speed * 3f, position, spark.Size * 0.9f, k * 0.8f);
                    Glow(images.GlowDot, _Gold, position, spark.Size, 0f, 1.5f * k);
                }
            }
        }
        void DrawLingers()
        {
            foreach (Linger star in _Lingers)
            {
                float a = age - 18f - star.Radius * 8f;
                if (a < 0f || a > star.Life)
                    continue;
                float t = a / star.Life;
                float k = Bell(t, 0.15f) * (0.7f + 0.3f * MathF.Sin(a * 0.5f + star.Angle * 3f));
                Vector2 position = Direction(star.Angle) * (inner + band * star.Radius);
                position.Y -= 10f * t;
                if (star.Big)
                    Star(position, star.Size * 3f, star.Rotation, 1.5f * k, 0.45f);
                else
                    Glow(images.Star4, _Light, position, star.Size * 1.8f, star.Rotation, 1.3f * k);
            }
        }
    }

    private static Vector2 Direction(float angle) => new(MathF.Cos(angle), MathF.Sin(angle));
}
