// Developed by ColdsUx

namespace Anomalies.Visuals;

public interface IAnomalyNPCWithCustomShader
{
    /// <summary>
    /// 在绘制Boss血条时应用自定义的着色器。
    /// </summary>
    /// <param name="newBar">Boss血条实例。</param>
    /// <param name="spriteBatch">SpriteBatch实例。</param>
    /// <param name="destinationRentangle">绘制目标矩形。</param>
    /// <remarks>
    /// <b>注意：</b>本方法调用前将自动改变 <paramref name="spriteBatch"/> 状态为：
    /// <list type="bullet">
    /// <item/><description/><see cref="SpriteSortMode.Immediate"/>
    /// <item/><description/><see cref="BlendState.AlphaBlend"/>
    /// <item/><description/><see cref="SamplerState.LinearClamp"/>
    /// </list>
    /// </remarks>
    public virtual void ApplyCustomMainBossBarShader(BossHealthBar newBar, SpriteBatch spriteBatch, Rectangle destinationRentangle) { }
}
