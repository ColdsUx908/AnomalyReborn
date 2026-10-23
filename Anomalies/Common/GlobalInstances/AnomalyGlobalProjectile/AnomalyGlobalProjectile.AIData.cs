namespace Anomalies.Common;

public sealed partial class AnomalyGlobalProjectile
{
    #region AI 数组映射数据
    #region 变种 AI 控制
    public int OverrideType
    {
        get => InternalAnomalyAI32[1].i;
        set
        {
            if (InternalAnomalyAI32[1].i != value)
            {
                InternalAnomalyAI32[1].i = value;
                InternalAIChanged32[1] = true;
            }
        }
    }

    public bool ShouldRunAnomalyAI
    {
        get => InternalAnomalyAI32[0].bits[0];
        set
        {
            if (InternalAnomalyAI32[0].bits[0] != value)
            {
                InternalAnomalyAI32[0].bits[0] = value;
                InternalAIChanged32[0] = true;
            }
        }
    }
    #endregion

    #region 深度
    /// <summary>
    /// 视觉深度（世界像素）。0 为正常平面，正值向屏幕后方远离，负值向屏幕前方靠近。
    /// <br/>仅影响绘制，不改变位置、碰撞或伤害；持续保留，回到正常平面时应设为 0。
    /// <br/>随弹幕额外 AI 同步；由弹幕的网络控制端修改时按需设置 projectile.netUpdate。
    /// </summary>
    public float Z
    {
        get => InternalAnomalyAI32[2].f;
        set
        {
            if (!float.IsFinite(value))
                throw new ArgumentOutOfRangeException(nameof(value));
            if (InternalAnomalyAI32[2].f != value)
            {
                InternalAnomalyAI32[2].f = value;
                InternalAIChanged32[2] = true;
            }
        }
    }
    #endregion

    #endregion AI 数组映射数据
}
