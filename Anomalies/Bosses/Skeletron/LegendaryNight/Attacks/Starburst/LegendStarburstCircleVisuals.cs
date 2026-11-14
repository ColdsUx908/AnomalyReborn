using Transoceanic.Graphics.Drawing;
using Transoceanic.Primitives.Mathematics;
using static Anomalies.Bosses.Skeletron.LegendaryNight.LegendStarburstVisuals;

namespace Anomalies.Bosses.Skeletron.LegendaryNight;

/// <summary>普通圆形星爆的金色冲击波、预告星和余辉；游戏与独立图形宿主共用。</summary>
internal static class LegendStarburstCircleVisuals
{
    public const int StartAge = 0;
    public const int EndAge = 84;
    public const int ExpansionDuration = 7;
    public const int WarningDuration = 30;
    public const int WarningFadeDuration = 2;
    private const int RingExpansionDuration = 20;
    private const int RimSegmentCount = 40;
    private const int WaveSegmentCount = 46;
    private const int RayCount = 18;
    private const int SparkCount = 46;
    private const int TwinkleCount = 22;
    private const int RimStarCount = 30;
    // 使用 350 世界像素的设计半径，所有爆炸层只缩放一次。
    internal const float ReferenceRadius = 350f;
    private const float RingMaxRadius = ReferenceRadius;
    private static readonly Color _Deep = new(240, 160, 40);
    private static readonly Color _Gold = new(255, 185, 55);
    private static readonly Color _Light = new(255, 222, 120);
    private static readonly Color _Pale = new(255, 244, 205);

    public readonly record struct Textures(Texture2D GlowDot, Texture2D Star4, Texture2D StarFlare,
        Texture2D ShotLine, Texture2D ExplosionDisc, Texture2D ShockShell, Texture2D WarningFlare, Texture2D SparkTail);

    private readonly record struct Ray(float Angle, float Length, float Delay, float Life, float Width);
    private readonly record struct Spark(float Angle, float Distance, float Delay, float Life, float Size,
        bool Star, float Rotation, float Spin);
    private readonly record struct Twinkle(float Angle, float Radius, float Life, float Size, float Rotation,
        bool Big, float Drift, float Born, float TravelScale);
    private readonly record struct RimStar(float Angle, float Distance, float Size, float Delay);

    private static readonly Ray[] _Rays = MakeRays();
    private static readonly Spark[] _Sparks = MakeSparks();
    private static readonly Twinkle[] _Twinkles = MakeTwinkles();
    private static readonly RimStar[] _RimStars = MakeRimStars();

    public static bool IsVisible(float age) => age >= StartAge && age < EndAge;

    public static bool IsWarningVisible(float age) => age > -WarningDuration && age < WarningFadeDuration;

    private static float RingRadius(float age) => 20f + (RingMaxRadius - 20f)
        * (1f - MathF.Pow(2f, -10f * Clamp(age / RingExpansionDuration))) / (1f - MathF.Pow(2f, -10f));

    public static float Expansion(float age) => OutExpo(Clamp(age / ExpansionDuration));

    public static void Draw(SpriteBatch batch, in Textures textures, in WorldView2D view,
        in LegendStarburstVisuals.Burst burst, BlendState blend, bool drawWarning = true)
    {
        if (!IsVisible(burst.Age) && !IsWarningVisible(burst.Age))
            return;
        // 爆炸按伤害半径缩放；预告星单独按小法阵半径缩放。
        float scale = burst.OuterRadius / ReferenceRadius;
        Vector2 center = burst.Center - view.ScreenPosition;
        float age = burst.Age;
        Textures images = textures;
        batch.Begin(SpriteSortMode.Deferred, blend, SamplerState.LinearClamp,
            DepthStencilState.None, RasterizerState.CullNone, null, view.Transform);
        if (drawWarning)
            DrawWarningStar(batch, images, center, age, burst.SigilRadius);
        if (age >= 0f)
        {
            DrawAoe();
            DrawFlash();
            DrawRing();
            DrawRays();
            DrawSparks();
            DrawTwinkles();
        }
        batch.End();

        void Sprite(Texture2D texture, Color color, float x, float y, float width, float height,
            float rotation = 0f, float alpha = 1f)
        {
            SpriteDrawing.DrawSized(batch, texture, center + new Vector2(x, y) * scale,
                new Vector2(width, height), color, rotation, alpha, scale);
        }
        void Glow(Texture2D texture, Color color, float x, float y, float size, float rotation, float intensity)
        {
            SpriteDrawing.DrawGlow(batch, texture, center + new Vector2(x, y) * scale,
                new Vector2(size), color, rotation, intensity, scale);
        }

        void Star(float x, float y, float size, float rotation, float intensity, float diagonal)
        {
            Glow(images.StarFlare, _Light, x, y, size, rotation, intensity);
            if (diagonal > 0f)
                Glow(images.StarFlare, _Light, x, y, size * diagonal, rotation + MathHelper.PiOver4, intensity * 0.7f);
        }
        void Line(Color color, float ax, float ay, float bx, float by, float width, float alpha)
        {
            Vector2 delta = new(bx - ax, by - ay);
            float length = delta.Length();
            if (length >= 0.5f)
                Sprite(images.ShotLine, color, (ax + bx) * 0.5f, (ay + by) * 0.5f, length * 1.25f,
                    width, MathF.Atan2(delta.Y, delta.X), alpha);
        }
        void DrawAoe()
        {
            if (age > 40f)
                return;
            float radius = ReferenceRadius * Expansion(age);
            float body = Bell(Clamp(age / 30f), 0.08f);
            Sprite(images.ExplosionDisc, Color.White, 0, 0, radius * 2f, radius * 2f, alpha: body);
            Glow(images.GlowDot, _Deep, 0, 0, radius * 1.6f, 0f, 0.55f * body);
            float rim = MathF.Pow(1f - Clamp((age - 8f) / 16f), 1.5f) * Smooth(Clamp(age / 2f));
            for (int i = 0; i < RimSegmentCount; i++)
            {
                float angle = i / (float)RimSegmentCount * MathHelper.TwoPi;
                float flicker = 0.8f + 0.2f * MathF.Sin(i * 1.9f + age * 1.1f);
                Sprite(images.ShotLine, _Light, MathF.Cos(angle) * radius, MathF.Sin(angle) * radius,
                    MathHelper.TwoPi * radius / (float)RimSegmentCount * 2.1f, 22f, angle + MathHelper.PiOver2, rim * flicker);
            }
            foreach (RimStar star in _RimStars)
            {
                float a = age - 14f - star.Delay;
                if (a < 0f || a > 24f)
                    continue;
                float t = a / 24f;
                float distance = (ReferenceRadius - star.Size * MathF.Sqrt(0.5f) - 14f) * (star.Distance - 0.04f * t);
                Glow(images.Star4, _Light, MathF.Cos(star.Angle) * distance, MathF.Sin(star.Angle) * distance - 8f * t,
                    star.Size, 0f, 1.5f * Bell(t, 0.2f));
            }
        }
        void DrawFlash()
        {
            if (age > 16f)
                return;
            float t = age / 16f;
            float k = Bell(t, 0.12f);
            Glow(images.GlowDot, _Gold, 0, 0, 180f * (0.6f + 0.6f * OutCubic(t)), 0, 2.4f * k);
            Star(0, 0, 420f * (0.5f + 0.7f * OutQuart(t)), 0, 2.2f * k, 0.45f);
        }
        void DrawRing()
        {
            float a = age - 2f;
            if (a < 0f || a > 34f)
                return;
            float t = a / 34f;
            float radius = RingRadius(a);
            float fade = Lerp(1f, 0.65f, Clamp(a / RingExpansionDuration))
                * (1f - Smooth(Clamp((a - RingExpansionDuration) / (34f - RingExpansionDuration))))
                * Smooth(Clamp(a / 3f));
            Sprite(images.ShockShell, _Gold, 0, 0, radius * 2.04f, radius * 2.04f, alpha: fade);
            for (int i = 0; i < WaveSegmentCount; i++)
            {
                float angle = i / (float)WaveSegmentCount * MathHelper.TwoPi + 0.15f * t;
                float shimmer = 0.75f + 0.25f * MathF.Sin(i * 2.3f + a * 0.9f);
                Sprite(images.ShotLine, _Gold, MathF.Cos(angle) * radius, MathF.Sin(angle) * radius,
                    MathHelper.TwoPi * radius / (float)WaveSegmentCount * 2.1f, 30f * Lerp(1f, 0.35f, t),
                    angle + MathHelper.PiOver2, fade * shimmer);
            }
        }
        void DrawRays()
        {
            foreach (Ray ray in _Rays)
            {
                float a = age - ray.Delay;
                if (a < 0f || a > ray.Life)
                    continue;
                float t = a / ray.Life;
                float head = ray.Length * OutCubic(t);
                float tail = Math.Max(0f, head - ray.Length * 0.55f * (1f - t * 0.6f));
                float x = MathF.Cos(ray.Angle);
                float y = MathF.Sin(ray.Angle);
                Line(_Light, x * tail, y * tail, x * head, y * head, 18f * ray.Width * (1f - t * 0.5f), MathF.Pow(1f - t, 1.3f));
            }
        }
        void DrawSparks()
        {
            foreach (Spark spark in _Sparks)
            {
                float a = age - spark.Delay;
                if (a < 0f || a > spark.Life)
                    continue;
                float travelScale = StarTravelScale(spark.Distance + 30f, spark.Size * (spark.Star ? 2.2f : 2.6f));
                float t = a / spark.Life;
                float distance = spark.Distance * OutQuart(t) * travelScale;
                float x = MathF.Cos(spark.Angle) * distance;
                float y = MathF.Sin(spark.Angle) * distance + 30f * t * t * travelScale;
                float k = MathF.Pow(1f - t, 1.2f);
                if (spark.Star)
                    Glow(images.Star4, _Light, x, y, spark.Size * 2.2f * (1f - t * 0.4f), spark.Rotation + spark.Spin * a, 1.4f * k);
                else
                {
                    // 尾端取最近三帧的实际轨迹，减速与重力同时影响方向和长度。
                    float previousT = Math.Max(0f, a - 3f) / spark.Life;
                    float previousDistance = spark.Distance * OutQuart(previousT) * travelScale;
                    float bx = MathF.Cos(spark.Angle) * previousDistance;
                    float by = MathF.Sin(spark.Angle) * previousDistance + 30f * previousT * previousT * travelScale;
                    Vector2 delta = new(x - bx, y - by);
                    float length = delta.Length();
                    float tailAlpha = k * 0.9f * Smooth(Clamp(length / (spark.Size * 0.75f)));
                    Sprite(images.SparkTail, _Gold, (bx + x) * 0.5f, (by + y) * 0.5f,
                        length, spark.Size * 0.72f * (1f - t * 0.35f), MathF.Atan2(delta.Y, delta.X), tailAlpha);
                    Star(x, y, spark.Size * 2.6f * (1f - t * 0.35f), 0f, 1.8f * k, 0f);
                    Glow(images.GlowDot, _Pale, x, y, spark.Size * 0.52f, 0f, 1.1f * k);
                }
            }
        }
        void DrawTwinkles()
        {
            foreach (Twinkle star in _Twinkles)
            {
                float a = age - star.Born;
                if (a < 0f || a > star.Life)
                    continue;
                float t = a / star.Life;
                float k = Bell(t, 0.15f) * (0.7f + 0.3f * MathF.Sin(a * 0.5f + star.Radius));
                float distance = (star.Radius + star.Drift * t) * star.TravelScale;
                float x = MathF.Cos(star.Angle) * distance;
                float y = MathF.Sin(star.Angle) * distance - 10f * t * star.TravelScale;
                if (star.Big)
                    Star(x, y, star.Size * 3.2f, star.Rotation, 1.6f * k, 0.45f);
                else
                    Glow(images.Star4, _Light, x, y, star.Size * 1.8f, star.Rotation, 1.3f * k);
            }
        }
    }

    public static void DrawWarning(SpriteBatch batch, in Textures textures, in WorldView2D view,
        in LegendStarburstVisuals.Burst burst, BlendState blend)
    {
        if (!IsWarningVisible(burst.Age))
            return;
        batch.Begin(SpriteSortMode.Deferred, blend, SamplerState.LinearClamp,
            DepthStencilState.None, RasterizerState.CullNone, null, view.Transform);
        DrawWarningStar(batch, textures, burst.Center - view.ScreenPosition, burst.Age, burst.SigilRadius);
        batch.End();
    }

    private static void DrawWarningStar(SpriteBatch batch, Textures images, Vector2 center, float age, float sigilRadius)
    {
        if (age >= WarningFadeDuration || images.WarningFlare is null)
            return;
        float elapsed = age + WarningDuration;
        float time = Math.Min(elapsed, WarningDuration);
        float t = time / WarningDuration;
        float remaining = MathF.Pow(1f - t, 2.7f);
        float rotation = 0.01f + 1.49f * t;
        float pulse = 1f + (1f - MathF.Abs(time % 20f - 10f) / 10f);
        float envelope = Smooth(Clamp(elapsed / 2f)) * (1f - Smooth(Clamp(age / WarningFadeDuration)));
        float baseScale = sigilRadius / 112f * 0.55f;
        float starScale = baseScale * 5;
        float width = 720f * 1.8f * remaining * starScale;
        float height = 360f * 0.1f * (1f - t) * starScale;
        for (int i = 0; i < 2; i++)
        {
            DrawGlow(images.WarningFlare, _Gold, width, height, rotation + i * MathHelper.PiOver2, 3.5f * pulse * envelope);
            DrawGlow(images.WarningFlare, _Pale, width * 0.72f, height * 0.55f,
                rotation + i * MathHelper.PiOver2, 1.3f * pulse * envelope);
        }
        float core = Lerp(22f * baseScale, 38f * starScale, remaining);
        DrawGlow(images.GlowDot, _Light, core, core, 0f, pulse * envelope);

        void DrawGlow(Texture2D texture, Color color, float spriteWidth, float spriteHeight, float spriteRotation, float intensity)
        {
            SpriteDrawing.DrawGlow(batch, texture, center, new Vector2(spriteWidth, spriteHeight),
                color, spriteRotation, intensity);
        }
    }

    private static Ray[] MakeRays()
    {
        Mulberry32Random random = new(11);
        Ray[] result = new Ray[RayCount];
        for (int i = 0; i < result.Length; i++)
            result[i] = new((i + random.NextSingle() * 0.7f) / (float)RayCount * MathHelper.TwoPi, Lerp(160, 300, random.NextSingle()),
                random.NextSingle() * 3f, Lerp(14, 22, random.NextSingle()), Lerp(0.7f, 1.2f, random.NextSingle()));
        return result;
    }
    private static Spark[] MakeSparks()
    {
        Mulberry32Random random = new(12);
        Spark[] result = new Spark[SparkCount];
        for (int i = 0; i < result.Length; i++)
        {
            result[i] = new(random.NextSingle() * MathHelper.TwoPi, Lerp(90, 360, MathF.Pow(random.NextSingle(), 0.7f)),
                random.NextSingle() * 4f, Lerp(28, 46, random.NextSingle()), Lerp(8, 16, random.NextSingle()), random.NextSingle() < 0.25f,
                random.NextSingle() * MathHelper.Pi, (random.NextSingle() - 0.5f) * 0.12f);
            _ = random.NextSingle(); // 保留未使用相位占用的配方样本。
            _ = random.NextSingle(); // 保留零幅度上升量占用的配方样本。
        }
        return result;
    }
    private static Twinkle[] MakeTwinkles()
    {
        Mulberry32Random random = new(13);
        Twinkle[] result = new Twinkle[TwinkleCount];
        for (int i = 0; i < result.Length; i++)
        {
            float angle = random.NextSingle() * MathHelper.TwoPi;
            float radius = Lerp(80, 330, random.NextSingle());
            float life = Lerp(26, 44, random.NextSingle());
            float size = Lerp(10, 18, random.NextSingle());
            float rotation = (random.NextSingle() - 0.5f) * 0.4f;
            bool big = random.NextSingle() < 0.25f;
            float drift = Lerp(4, 14, random.NextSingle());
            float travelScale = StarTravelScale(radius + drift + 10f, size * (big ? 3.2f : 1.8f));
            float born = 36f;
            for (float f = 0; f <= 34f; f += 0.5f)
                if (RingRadius(f) >= radius * travelScale)
                {
                    born = f + 2f;
                    break;
                }
            result[i] = new(angle, radius, life, size, rotation, big, drift, born, travelScale);
        }
        return result;
    }
    private static RimStar[] MakeRimStars()
    {
        Mulberry32Random random = new(77);
        RimStar[] result = new RimStar[RimStarCount];
        for (int i = 0; i < result.Length; i++)
            result[i] = new((i + random.NextSingle() * 0.8f) / (float)RimStarCount * MathHelper.TwoPi,
                Lerp(0.78f, 0.98f, random.NextSingle()), Lerp(10, 20, random.NextSingle()), random.NextSingle() * 6f);
        return result;
    }
    // 按整段轨迹留出旋转星芒的包围半径，避免逐帧夹位置造成圆周停滞。
    private static float StarTravelScale(float travel, float size) => Math.Min(1f,
        Math.Max(0f, ReferenceRadius - size * MathF.Sqrt(0.5f) - 6f) / Math.Max(travel, 0.001f));

}
