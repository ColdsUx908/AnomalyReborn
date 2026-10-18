namespace Anomalies.Visuals;

/// <summary>
/// 由外部持续递增强度启用的背景星光；无人启用时自行淡出。
/// </summary>
public sealed class AnomalyStarVisual : ModSystem
{
    private const float FadeOutStep = 1f / 60f;

    public static float Intensity
    {
        get;
        set => field = MathHelper.Clamp(value, 0f, 1f);
    }

    public override void Load()
    {
        if (!Main.dedServ)
            On_Main.DrawStarsInBackground += DrawBackgroundStar;
    }

    public override void Unload()
    {
        Intensity = 0f;
    }

    public override void PostUpdateEverything() => Intensity -= FadeOutStep;
    public override void OnWorldLoad() => Intensity = 0f;
    public override void OnWorldUnload() => Intensity = 0f;

    private static void DrawBackgroundStar(On_Main.orig_DrawStarsInBackground orig, Main self, Main.SceneArea sceneArea, bool artificial)
    {
        orig(self, sceneArea, artificial);
        if (Main.gameMenu || Intensity <= 0f)
            return;

        Draw(Main.spriteBatch, Intensity);
    }

    /// <summary>在当前 SpriteBatch 中绘制星光，强度独立于世界内的淡入淡出状态。</summary>
    public static void Draw(SpriteBatch spriteBatch, float intensity)
    {
        intensity = MathHelper.Clamp(intensity, 0f, 1f);
        if (intensity <= 0f)
            return;

        Texture2D bloom = ParticleHandler.GetTexture<BloomParticle>();
        Texture2D star = AnomalyTextures.Extra.CrystalTextSparkle;
        Vector2 position = new(MathF.Round(Main.screenWidth * 0.7f), MathF.Round(Main.screenHeight * 0.08f));
        float pulse = 1f + 0.035f * MathF.Sin(Main.GlobalTimeWrappedHourly * 1.5f);
        float size = Math.Min(Main.screenWidth, Main.screenHeight) * 0.1f * pulse;
        Color purple = (AnomalySharedData.AnomalyUltramundaneColor with { A = 0 }) * intensity;
        Color white = (Color.White with { A = 0 }) * intensity;

        // 零 Alpha 的预乘颜色叠加光，保持天空绘制的 SpriteBatch 状态。
        spriteBatch.DrawFromCenter(bloom, position, null, purple * 0.22f, 0f, size * 3f / bloom.Width);
        spriteBatch.DrawFromCenter(bloom, position, null, white * 0.35f, 0f, size * 1.5f / bloom.Width);
        DrawStarRays(spriteBatch, position, size, purple, white);
        // 29px 水晶纹理只用作小亮芯，避免放大后的模糊覆盖清晰星芒。
        float coreScale = Math.Min(size * 0.3f / star.Width, 1f);
        spriteBatch.DrawFromCenter(star, position, null, purple * 0.65f, 0f, coreScale);
        spriteBatch.DrawFromCenter(star, position, null, white * 0.85f, 0f, coreScale * 0.8f);
    }

    private static void DrawStarRays(SpriteBatch spriteBatch, Vector2 position, float size, Color purple, Color white)
    {
        // 与 ImmaculateBolt 相同，将 SharpTears 拉成长光芒后交叉叠加。
        Texture2D rays = TextureAssets.Extra[ExtrasID.SharpTears].Value;
        Vector2 origin = rays.Size() * 0.5f;
        for (int i = 0; i < 4; i++)
        {
            float rotation = i switch
            {
                0 => 0f,
                1 => MathHelper.PiOver2,
                2 => MathHelper.PiOver4,
                _ => MathHelper.PiOver4 * 3f
            };
            float breathPhase = Main.GlobalTimeWrappedHourly * 2.2f + (i >= 2 ? MathHelper.PiOver2 : 0f);
            float lengthPulse = 1f + 0.2f * MathF.Sin(breathPhase);
            float length = size * (i < 2 ? 2.2f : 1.1f) * lengthPulse;
            float strength = i < 2 ? 1f : 0.45f;
            Vector2 outerScale = new(size * 0.12f / rays.Width, length / rays.Height);
            Vector2 coreScale = new(size * 0.045f / rays.Width, length * 0.9f / rays.Height);
            spriteBatch.Draw(rays, position, null, purple * (0.55f * strength), rotation, origin, outerScale, SpriteEffects.None, 0f);
            spriteBatch.Draw(rays, position, null, white * (0.8f * strength), rotation, origin, coreScale, SpriteEffects.None, 0f);
        }
    }
}
