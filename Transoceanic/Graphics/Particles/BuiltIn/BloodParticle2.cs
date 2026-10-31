namespace Transoceanic.Graphics.Particles.BuiltIn;

/// <summary>
/// 使用纵向三帧纹理、随寿命褪色的血迹粒子。
/// </summary>
public class BloodParticle2 : Particle
{
    /// <summary>
    /// 寿命颜色插值使用的初始颜色。
    /// </summary>
    public Color InitialColor;

    /// <inheritdoc/>
    /// <remarks>此实现返回 <c>BlendState.Additive</c>。</remarks>
    public override BlendState DrawBlendState => BlendState.Additive;

    /// <inheritdoc/>
    /// <remarks>此实现返回 <c>&quot;Transoceanic/Assets/Textures/Particles/Blood2&quot;</c>。</remarks>
    public override string TexturePath => "Transoceanic/Assets/Textures/Particles/Blood2";

    /// <summary>
    /// 创建三帧血迹粒子，设置中心像素坐标、像素/帧速度、帧寿命、缩放和初始颜色。
    /// </summary>
    public BloodParticle2(Vector2 relativeCenter, Vector2 velocity, int lifetime, float scale, Color color)
    {
        Center = relativeCenter;
        Velocity = velocity;
        Scale = scale;
        Lifetime = lifetime;
        Color = InitialColor = color;
    }

    /// <inheritdoc/>
    /// <remarks>
    /// 衰减速度并按寿命褪色，朝速度方向旋转；关闭血腥时使用彩虹色。
    /// </remarks>
    public override void Update()
    {
        Velocity *= 0.98f;
        Color = Color.Lerp(InitialColor, Color.Transparent, (float)Math.Pow(LifetimeCompletion, 4D));
        Rotation = Velocity.ToRotation();
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
        float brightness = (float)Math.Pow(Lighting.Brightness((int)(Center.X / 16f), (int)(Center.Y / 16f)), 0.15);
        Texture2D texture = Texture;
        Rectangle frame = texture.Frame(1, 3, 0, Math.Clamp((int)(LifetimeCompletion * 3f), 0, 2));
        Vector2 origin = frame.Size() * 0.5f;

        spriteBatch.Draw(texture, Center + drawOffset - Main.screenPosition, frame, Color * brightness, Rotation, origin, Scale, 0, 0f);
        return false;
    }
}
