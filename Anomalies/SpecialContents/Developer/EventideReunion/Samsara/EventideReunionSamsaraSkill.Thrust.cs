namespace Anomalies.SpecialContents.Developer.EventideReunion;

public sealed partial class EventideReunionSamsaraSkill
{
    private static Texture2D _ThrustGlowTexture;

    private void DrawThrustGhost()
    {
        if (Phase != SamsaraPhase.Thrust)
            return;
        float strength = EventideReunionSamsaraVisualGeometry.ThrustGlow(Progress);
        if (strength <= 0.001f)
            return;

        Texture2D glow = GetThrustGlowTexture();
        float extension = EventideReunionSamsaraVisualGeometry.GhostExtension * strength;
        Vector2 root = WeaponTip - Aim * 180f - Main.screenPosition;
        // 与实体武器一起提交 SpriteBatch；不切换 Effect，也不依赖因果线的绘制状态。
        // 纹理的零 alpha 颜色在 AlphaBlend 下提供自发光，与普通攻击的矛光一致。
        Main.spriteBatch.Draw(glow, root, null, Color.White * strength, Aim.ToRotation(),
            new Vector2(0f, glow.Height * 0.5f), new Vector2((180f + extension) / glow.Width, 1f),
            SpriteEffects.None, 0f);
    }

    private static Texture2D GetThrustGlowTexture()
    {
        if (_ThrustGlowTexture is { IsDisposed: false })
            return _ThrustGlowTexture;
        const int Width = 512;
        const int Height = 128;
        Color[] pixels = new Color[Width * Height];
        for (int x = 0; x < Width; x++)
        {
            float along = x / (Width - 1f) * 460f;
            // 刃根、肩部、实体尖端、延伸刃身到远端尖点，连续构成一个矛形。
            float halfWidth = along < 75f ? MathHelper.Lerp(0f, 42f, along / 75f)
                : along < 180f ? MathHelper.Lerp(42f, 32f, (along - 75f) / 105f)
                : along < 362f ? MathHelper.Lerp(32f, 24f, (along - 180f) / 182f)
                : MathHelper.Lerp(24f, 0f, (along - 362f) / 98f);
            float endFade = MathHelper.SmoothStep(0f, 1f, along / 25f)
                * MathHelper.SmoothStep(0f, 1f, (460f - along) / 35f);
            for (int y = 0; y < Height; y++)
            {
                float across = Math.Abs(y + 0.5f - Height * 0.5f) / Math.Max(halfWidth, 0.5f);
                float bounds = 1f - MathHelper.SmoothStep(0f, 1f, (across - 0.45f) / 0.55f);
                float core = MathF.Exp(-across * across * 20f);
                float intensity = (MathF.Exp(-across * across * 4f) * 0.65f + core * 0.55f) * bounds * endFade;
                Color tint = Color.Lerp(new Color(10, 225, 220, 0), new Color(110, 255, 245, 0), core * 0.5f);
                pixels[y * Width + x] = tint * intensity;
            }
        }
        _ThrustGlowTexture = new Texture2D(Main.instance.GraphicsDevice, Width, Height);
        _ThrustGlowTexture.SetData(pixels);
        return _ThrustGlowTexture;
    }

    public override void Unload()
    {
        Texture2D texture = _ThrustGlowTexture;
        _ThrustGlowTexture = null;
        if (texture is not null)
            Main.QueueMainThreadAction(texture.Dispose);
    }
}
