namespace Transoceanic.Graphics.Particles.BuiltIn;

/// <summary>
/// 带有火焰细节的扩散爆炸粒子，支持颜色、拉伸、旋转和混合模式。
/// </summary>
public sealed class DetailedExplosion : TexturePulseParticle
{
    /// <summary>
    /// 使用 DetailedExplosion 纹理创建脉冲爆炸，设置起止缩放、帧寿命、弧度旋转、双轴形变和混合模式。
    /// </summary>
    public DetailedExplosion(Vector2 position, Vector2 velocity, Color color, Vector2 squish, float rotation,
        float originalScale, float finalScale, int lifeTime, bool additive = true)
        : base(position, velocity, color, ParticleHandler.BaseParticleTexturePath + "DetailedExplosion", squish,
            rotation, originalScale, finalScale, lifeTime, additive)
    {
    }
}
