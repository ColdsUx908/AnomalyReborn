namespace Transoceanic.Graphics.Particles.BuiltIn;

/// <summary>
/// 在局部坐标中向原点收拢并变色缩小的附魔粒子。
/// </summary>
public class EnchantedParticle : Particle
{
    /// <summary>
    /// 预留的附魔效果强度数据，当前粒子更新逻辑未读取此字段。
    /// </summary>
    public float RelativePower;

    /// <summary>
    /// 每次更新向局部原点收拢的线性插值系数。
    /// </summary>
    public float InterpolationSpeed;
    /// <summary>
    /// 计算附魔粒子尺寸插值的距离阈值，单位与局部坐标一致。
    /// </summary>
    public float EdgeOffset;
    /// <summary>
    /// 附魔粒子在寿命初期使用的边缘颜色。
    /// </summary>
    public Color EdgeColor;
    /// <summary>
    /// 粒子靠近局部中心时使用的颜色。
    /// </summary>
    public Color CenterColor;

    /// <inheritdoc/>
    /// <remarks>此实现返回 <c>ParticleHandler.BaseParticleTexturePath + &quot;Light&quot;</c>。</remarks>
    public override string TexturePath => ParticleHandler.BaseParticleTexturePath + "Light";

    /// <summary>
    /// 创建向局部原点收拢的附魔粒子，设置帧寿命、边缘/中心颜色、插值速度和距离阈值。
    /// </summary>
    public EnchantedParticle(Vector2 relativePosition, int lifetime, float scale, Color edgeColor, Color centerColor, float interpolationSpeed, float edgeOffset)
    {
        Center = relativePosition;
        Velocity = Vector2.Zero;
        Scale = scale;
        Lifetime = lifetime;
        EdgeColor = edgeColor;
        CenterColor = centerColor;
        InterpolationSpeed = interpolationSpeed;
        EdgeOffset = edgeOffset;
    }

    /// <inheritdoc/>
    /// <remarks>
    /// 按距局部原点的距离更新尺寸与位置，接近中心时缩小直至死亡，并插值边缘和中心颜色。
    /// </remarks>
    public override void Update()
    {
        float distanceToCenter = Center.Length();
        Scale = MathHelper.SmoothStep(0.05f, 0.125f, Utils.GetLerpValue(EdgeOffset, 6f, distanceToCenter, true));
        Scale *= Utils.GetLerpValue(Lifetime, Lifetime - 10f, Timer, true);

        if (distanceToCenter > 4f)
            Center = Vector2.Lerp(Center, Vector2.Zero, InterpolationSpeed);
        else
        {
            Scale *= 0.92f;
            if (Scale < 0.05f)
                Kill();
        }

        Color = Color.Lerp(EdgeColor, CenterColor, Utils.GetLerpValue(0f, 0.67f, LifetimeCompletion, true));
        Color.A = 50;
    }
}
