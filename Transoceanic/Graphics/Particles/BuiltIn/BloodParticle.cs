namespace Transoceanic.Graphics.Particles.BuiltIn;

/// <summary>
/// 带重力、方向拉伸和褪色的血滴粒子。
/// </summary>
public class BloodParticle : Particle
{
    /// <summary>
    /// 寿命颜色插值使用的初始颜色。
    /// </summary>
    public Color InitialColor;

    /// <inheritdoc/>
    /// <remarks>此实现返回 <c>BlendState.Additive</c>。</remarks>
    public override BlendState DrawBlendState => BlendState.Additive;

    /// <inheritdoc/>
    /// <remarks>此实现返回 <c>&quot;Transoceanic/Assets/Textures/Particles/Blood&quot;</c>。</remarks>
    public override string TexturePath => "Transoceanic/Assets/Textures/Particles/Blood";

    /// <summary>
    /// 创建血滴粒子，设置中心像素坐标、像素/帧速度、帧寿命、缩放和初始颜色。
    /// </summary>
    public BloodParticle(Vector2 relativeCenter, Vector2 velocity, int lifetime, float scale, Color color)
    {
        Center = relativeCenter;
        Velocity = velocity;
        Scale = scale;
        Lifetime = lifetime;
        Color = InitialColor = color;
    }

    /// <inheritdoc/>
    /// <remarks>
    /// 缩小并减慢横向速度，施加重力，按寿命褪色并朝速度方向旋转；关闭血腥时使用彩虹色。
    /// </remarks>
    public override void Update()
    {
        Scale *= 0.98f;
        Velocity.X *= 0.97f;
        Velocity.Y = MathHelper.Clamp(Velocity.Y + 0.9f, -22f, 22f);
        Color = Color.Lerp(InitialColor, Color.Transparent, (float)Math.Pow(LifetimeCompletion, 3D));
        Rotation = Velocity.ToRotation() + MathHelper.PiOver2;
        // Cycle through disco colors if blood and gore is off
        if (!ChildSafety.Disabled)
        {
            Color = Main.DiscoColor;
        }
    }

    /// <inheritdoc/>
    /// <remarks>
    /// 执行该粒子的自定义绘制，将中心加像素偏移后转换到屏幕坐标；返回 false 跳过默认绘制。
    /// </remarks>
    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 drawOffset = default)
    {
        float verticalStretch = Utils.GetLerpValue(0f, 24f, Math.Abs(Velocity.Y), true) * 0.84f;
        float brightness = (float)Math.Pow(Lighting.Brightness((int)(Center.X / 16f), (int)(Center.Y / 16f)), 0.15);
        Vector2 scale = new Vector2(1f, verticalStretch + 1f) * Scale * 0.1f;
        Texture2D texture = Texture;

        spriteBatch.Draw(texture, Center + drawOffset - Main.screenPosition, null, Color * brightness, Rotation, texture.Size() * 0.5f, scale, 0, 0f);
        return false;
    }
}
