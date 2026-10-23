namespace Anomalies.Common;

public sealed partial class AnomalyGlobalNPC
{
    #region AI 数组映射数据
    #region 变种 AI 控制
    /// <summary>
    /// 自定义AI类型，仅供NPC的变种AI使用。
    /// <br/>如果不使用变种AI，请不要修改此值。
    /// </summary>
    public int CustomAIType
    {
        get => InternalAnomalyAI32[2].i;
        set
        {
            if (InternalAnomalyAI32[2].i != value)
            {
                InternalAnomalyAI32[2].i = value;
                InternalAIChanged32[2] = true;
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

    #region 减益
    public bool Debuff_DimensionalRend
    {
        get => InternalAnomalyAI32[1].bits[0];
        set
        {
            if (InternalAnomalyAI32[1].bits[0] != value)
            {
                InternalAnomalyAI32[1].bits[0] = value;
                InternalAIChanged32[1] = true;
            }
        }
    }
    #endregion

    #region 深度
    /// <summary>
    /// 深度（世界像素）。0 为正常平面，正值向屏幕后方远离，负值向屏幕前方靠近。
    /// <br/>不改变世界位置；玩家武器和弹幕使用投影碰撞箱，越过观察者平面后不可被击中。
    /// <br/>NPC 之间的接触伤害仍要求 Z=0，避免使用某个客户端的镜头决定服务端碰撞。
    /// <br/>持续保留，回到正常战斗平面时应设为 0。
    /// <br/>随 NPC 额外 AI 同步；由服务端修改时按需设置 npc.netUpdate。
    /// </summary>
    public float Z
    {
        get => InternalAnomalyAI32[3].f;
        set
        {
            if (!float.IsFinite(value))
                throw new ArgumentOutOfRangeException(nameof(value));
            if (InternalAnomalyAI32[3].f != value)
            {
                InternalAnomalyAI32[3].f = value;
                InternalAIChanged32[3] = true;
            }
        }
    }
    #endregion

    #region 伤害减免
    /// <summary>
    /// 伤害减免，在计算伤害时会被应用。
    /// </summary>
    /// <remarks>谨慎使用。</remarks>
    public float DR
    {
        get => InternalAnomalyAI32[4].f;
        set
        {
            if (InternalAnomalyAI32[4].f != value)
            {
                InternalAnomalyAI32[4].f = value;
                InternalAIChanged32[4] = true;
            }
        }
    }

    /// <summary>
    /// 额外DR，不受任何修改DR的机制影响。
    /// </summary>
    /// <remarks>谨慎使用。</remarks>
    public float ExtraDR
    {
        get => InternalAnomalyAI32[5].f;
        set
        {
            if (InternalAnomalyAI32[5].f != value)
            {
                InternalAnomalyAI32[5].f = value;
                InternalAIChanged32[5] = true;
            }
        }
    }
    #endregion

    #endregion AI 数组映射数据
}
