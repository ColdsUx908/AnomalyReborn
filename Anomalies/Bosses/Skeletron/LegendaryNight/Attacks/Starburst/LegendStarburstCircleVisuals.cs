using Transoceanic.Graphics.Rendering;

namespace Anomalies.Bosses.Skeletron.LegendaryNight;

/// <summary>GoldenShockwavePreview 的 A 星芒环爆；只依赖 FNA，游戏与离线预览共用。</summary>
internal static class LegendStarburstCircleVisuals
{
    public const int StartAge = 0;
    public const int EndAge = 84;
    public const int ExpansionDuration = 7;
    public const int BlurDuration = 20;
    public const int ShakeDuration = 14;
    public const int WarningDuration = 30;
    public const float WarningStarSize = 3f;
    private const int RingExpansionDuration = 20;
    // 与 HTML A 方案共用 350 世界像素的设计坐标，所有爆炸层只缩放一次。
    private const float ReferenceRadius = 350f;
    private const float RingMaxRadius = ReferenceRadius;
    private static readonly Color _Deep = new(240, 160, 40);
    private static readonly Color _Gold = new(255, 185, 55);
    private static readonly Color _Light = new(255, 222, 120);
    private static readonly Color _Pale = new(255, 244, 205);

    public readonly record struct Textures(Texture2D GlowDot, Texture2D Star4, Texture2D StarFlare,
        Texture2D ShotLine, Texture2D AoeDisc, Texture2D ShockShell, Texture2D HShotA, Texture2D SparkTail);

    private readonly record struct Ray(float Angle, float Length, float Delay, float Life, float Width);
    private readonly record struct Spark(float Angle, float Distance, float Delay, float Life, float Size,
        bool Star, float Rotation, float Spin, float Phase);
    private readonly record struct Twinkle(float Angle, float Radius, float Life, float Size, float Rotation,
        bool Big, float Drift, float Born, float TravelScale);
    private readonly record struct RimStar(float Angle, float Distance, float Size, float Delay);

    private static readonly Ray[] _Rays = MakeRays();
    private static readonly Spark[] _Sparks = MakeSparks();
    private static readonly Twinkle[] _Twinkles = MakeTwinkles();
    private static readonly RimStar[] _RimStars = MakeRimStars();

    public static bool IsVisible(float age) => age >= StartAge && age < EndAge;

    public static bool IsWarningVisible(float age) => age > -WarningDuration && age < 0f;

    private static float RingRadius(float age) => 20f + (RingMaxRadius - 20f)
        * (1f - MathF.Pow(2f, -10f * Clamp(age / RingExpansionDuration))) / (1f - MathF.Pow(2f, -10f));

    public static float BlurStrength(float age) => age < 0f || age >= BlurDuration ? 0f
        : 0.12f * Smooth(Clamp(age / 1.5f)) * MathF.Pow(1f - age / BlurDuration, 2f);

    public static float Expansion(float age) => OutExpo(Clamp(age / ExpansionDuration));

    /// <summary>将 HTML 的 1280 输出基准与爆区比例换算成最终屏幕像素光晕。</summary>
    public static BloomSettings GetBloomSettings(float outerRadius, in Matrix transform, bool dayTime)
    {
        float cameraScale = new Vector2(transform.M11, transform.M12).Length();
        // HTML: (输出宽度 / 1280) × 镜头缩放 × (爆区 / 350) / 1.1。
        // 世界坐标按 1920 视野映射，所以以 1920/1280 还原到最终屏幕像素。
        float haloScale = 1.5f * cameraScale * (outerRadius / ReferenceRadius) / 1.1f;
        BloomSettings defaults = BloomSettings.Default;
        return defaults with
        {
            NearRadius = defaults.NearRadius * haloScale,
            FarRadius = defaults.FarRadius * haloScale,
            AdditiveBody = !dayTime,
            BodyPasses = dayTime ? 2 : 1
        };
    }


    /// <summary>HTML 整套技能的世界像素偏移；相机、爆点和背景共用同一年龄。</summary>
    public static Vector2 ShakeOffset(float age)
    {
        if (age < 0f || age >= ShakeDuration)
            return Vector2.Zero;
        float amplitude = 9f * (1f - age / ShakeDuration);
        return new Vector2(MathF.Sin(age * 2.7f), MathF.Cos(age * 3.3f) * 0.7f) * amplitude;
    }

    public static void Draw(SpriteBatch batch, in Textures textures, in LegendStarburstVisuals.View view,
        in LegendStarburstVisuals.Burst burst, BlendState blend)
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
        DrawWarningStar(burst.SigilRadius);
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

        void DrawWarningStar(float sigilRadius)
        {
            if (age >= 2f || images.HShotA is null)
                return;
            float elapsed = age + WarningDuration;
            float time = Math.Min(elapsed, WarningDuration);
            float t = time / WarningDuration;
            float remaining = MathF.Pow(1f - t, 2.7f);
            float rotation = 0.01f + 1.49f * t;
            float pulse = 1f + (1f - MathF.Abs(time % 20f - 10f) / 10f);
            float envelope = Smooth(Clamp(elapsed / 2f)) * (1f - Smooth(Clamp(age / 2f)));
            float baseScale = sigilRadius / 112f * 0.55f;
            float starScale = baseScale * 5;
            float width = 720f * 1.8f * remaining * starScale;
            float height = 360f * 0.1f * (1f - t) * starScale;
            for (int i = 0; i < 2; i++)
            {
                DrawGlow(images.HShotA, _Gold, width, height, rotation + i * MathHelper.PiOver2, 3.5f * pulse * envelope);
                DrawGlow(images.HShotA, _Pale, width * 0.72f, height * 0.55f,
                    rotation + i * MathHelper.PiOver2, 1.3f * pulse * envelope);
            }
            float core = Lerp(22f * baseScale, 38f * starScale, remaining);
            DrawGlow(images.GlowDot, _Light, core, core, 0f, pulse * envelope);

            void DrawGlow(Texture2D texture, Color color, float spriteWidth, float spriteHeight, float spriteRotation, float intensity)
            {
                if (spriteWidth <= 0f || spriteHeight <= 0f)
                    return;
                for (float k = intensity; k > 0.002f; k -= 1f)
                    batch.Draw(texture, center, null, color * Math.Min(k, 1f), spriteRotation,
                        new Vector2(texture.Width, texture.Height) * 0.5f,
                        new Vector2(spriteWidth / texture.Width, spriteHeight / texture.Height), SpriteEffects.None, 0f);
            }
        }

        void Sprite(Texture2D texture, Color color, float x, float y, float width, float height,
            float rotation = 0f, float alpha = 1f)
        {
            if (alpha <= 0.002f || width <= 0f || height <= 0f)
                return;
            float unit = scale;
            batch.Draw(texture, center + new Vector2(x, y) * unit, null, color * Math.Min(alpha, 1f),
                rotation, new Vector2(texture.Width, texture.Height) * 0.5f,
                new Vector2(width / texture.Width, height / texture.Height) * unit, SpriteEffects.None, 0f);
        }
        void Glow(Texture2D texture, Color color, float x, float y, float size, float rotation, float intensity)
        {
            for (float k = intensity; k > 0.002f; k -= 1f)
                Sprite(texture, color, x, y, size, size, rotation, Math.Min(1f, k));
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
            Sprite(images.AoeDisc, Color.White, 0, 0, radius * 2f, radius * 2f, alpha: body);
            Glow(images.GlowDot, _Deep, 0, 0, radius * 1.6f, 0f, 0.55f * body);
            float rim = MathF.Pow(1f - Clamp((age - 8f) / 16f), 1.5f) * Smooth(Clamp(age / 2f));
            for (int i = 0; i < 40; i++)
            {
                float angle = i / 40f * MathHelper.TwoPi;
                float flicker = 0.8f + 0.2f * MathF.Sin(i * 1.9f + age * 1.1f);
                Sprite(images.ShotLine, _Light, MathF.Cos(angle) * radius, MathF.Sin(angle) * radius,
                    MathHelper.TwoPi * radius / 40f * 2.1f, 22f, angle + MathHelper.PiOver2, rim * flicker);
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
            for (int i = 0; i < 46; i++)
            {
                float angle = i / 46f * MathHelper.TwoPi + 0.15f * t;
                float shimmer = 0.75f + 0.25f * MathF.Sin(i * 2.3f + a * 0.9f);
                Sprite(images.ShotLine, _Gold, MathF.Cos(angle) * radius, MathF.Sin(angle) * radius,
                    MathHelper.TwoPi * radius / 46f * 2.1f, 30f * Lerp(1f, 0.35f, t),
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

    private static Ray[] MakeRays()
    {
        RandomSequence random = new(11);
        Ray[] result = new Ray[18];
        for (int i = 0; i < result.Length; i++)
            result[i] = new((i + random.Next() * 0.7f) / 18f * MathHelper.TwoPi, Lerp(160, 300, random.Next()),
                random.Next() * 3f, Lerp(14, 22, random.Next()), Lerp(0.7f, 1.2f, random.Next()));
        return result;
    }
    private static Spark[] MakeSparks()
    {
        RandomSequence random = new(12);
        Spark[] result = new Spark[46];
        for (int i = 0; i < result.Length; i++)
        {
            result[i] = new(random.Next() * MathHelper.TwoPi, Lerp(90, 360, MathF.Pow(random.Next(), 0.7f)),
                random.Next() * 4f, Lerp(28, 46, random.Next()), Lerp(8, 16, random.Next()), random.Next() < 0.25f,
                random.Next() * MathHelper.Pi, (random.Next() - 0.5f) * 0.12f, random.Next() * 6.28f);
            _ = random.Next(); // HTML consumes a random value for rise, even when its amplitude is zero.
        }
        return result;
    }
    private static Twinkle[] MakeTwinkles()
    {
        RandomSequence random = new(13);
        Twinkle[] result = new Twinkle[22];
        for (int i = 0; i < result.Length; i++)
        {
            float angle = random.Next() * MathHelper.TwoPi;
            float radius = Lerp(80, 330, random.Next());
            float life = Lerp(26, 44, random.Next());
            float size = Lerp(10, 18, random.Next());
            float rotation = (random.Next() - 0.5f) * 0.4f;
            bool big = random.Next() < 0.25f;
            float drift = Lerp(4, 14, random.Next());
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
        RandomSequence random = new(77);
        RimStar[] result = new RimStar[30];
        for (int i = 0; i < result.Length; i++)
            result[i] = new((i + random.Next() * 0.8f) / 30f * MathHelper.TwoPi,
                Lerp(0.78f, 0.98f, random.Next()), Lerp(10, 20, random.Next()), random.Next() * 6f);
        return result;
    }
    private struct RandomSequence(uint seed)
    {
        private uint _State = seed;
        public float Next()
        {
            unchecked
            {
                uint t = _State += 0x6D2B79F5;
                t = (t ^ (t >> 15)) * (1u | t);
                t ^= t + (t ^ (t >> 7)) * (61u | t);
                return (float)((t ^ (t >> 14)) / 4294967296.0);
            }
        }
    }
    // 按整段轨迹留出旋转星芒的包围半径，避免逐帧夹位置造成圆周停滞。
    private static float StarTravelScale(float travel, float size) => Math.Min(1f,
        Math.Max(0f, ReferenceRadius - size * MathF.Sqrt(0.5f) - 6f) / Math.Max(travel, 0.001f));

    private static float Clamp(float t) => Math.Clamp(t, 0f, 1f);
    private static float Lerp(float a, float b, float t) => a + (b - a) * t;
    private static float Smooth(float t) => t * t * (3f - 2f * t);
    private static float OutCubic(float t) => 1f - MathF.Pow(1f - t, 3f);
    private static float OutQuart(float t) => 1f - MathF.Pow(1f - t, 4f);
    private static float OutExpo(float t) => t >= 1f ? 1f : 1f - MathF.Pow(2f, -10f * t);
    private static float Bell(float t, float rise) => t <= 0f || t >= 1f ? 0f
        : t < rise ? Smooth(t / rise) : MathF.Pow(1f - (t - rise) / (1f - rise), 1.6f);
}
