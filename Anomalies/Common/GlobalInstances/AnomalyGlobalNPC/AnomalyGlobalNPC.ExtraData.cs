using Anomalies.SpecialContents.Developer.EventideReunion;
using Anomalies.Visuals.BossBar;

namespace Anomalies.Common;

public sealed partial class AnomalyGlobalNPC
{
    #region 独立实例数据
    #region 变种 AI 状态
    public bool AnomalyAI_IsRunning => AnomalyAI_Timer > 0;
    public int AnomalyAI_Timer;
    public int AnomalyAI_UltraTimer;
    #endregion

    #region 血条显示
    public bool BossBar_Enabled;
    public bool BossBar_Enraged;
    public List<HPThresholdIndicator> BossBar_HPThresholdIndicators = [];
    public bool BossBar_Immune;
    public bool BossBar_IncreasingDefenseOrDR;
    public bool BossBar_IsSmall;
    public int BossBar_UltraTimer;
    #endregion


    #region 伤害减免
    public IDynamicDRHandler DamageReduction_DynamicHandler;
    #endregion

    #region 深度绘制
    /// <summary>本帧的视觉锚点：实体局部位置、未投影的世界目标和混合权重。仅影响绘制。</summary>
    internal (Vector2 LocalOffset, Vector2 Target, float Weight)? Depth_DrawAnchor;
    #endregion


    #region 落日遗逢
    // 仅在回斩/波的伤害结算期间有效，供致死回调使用，不参与网络快照。
    public int EventideReunion_DeathHitOwner = -1;

    // 仅客户端的标记入场/退场进度，不参与网络快照。
    public float EventideReunion_DeathMarkProgress;
    // 服务器权威计时；客户端接收快照后逐帧递减，不使用 Buff。
    public int EventideReunion_DeathMarkTime;
    public int EventideReunion_DeathOwner = -1;
    internal int EventideReunion_SamsaraOwner = -1;
    internal EventideReunionSamsaraRecord EventideReunion_SamsaraRecord = new();
    internal ulong EventideReunion_SamsaraStartTick;
    internal int EventideReunion_SamsaraVisualTime;
    #endregion

    #region 通用状态
    /// <summary>
    /// 特殊标志位，供外部判定使用。
    /// <br/>每帧都会被重置为默认值，外部判定需要在每帧内使用。
    /// </summary>
    public BitArray64 General_SpecialFlags = default;
    #endregion

    #region 网络引用缓存
    // Per-NPC local lookup hints. The authoritative owner/identity remains in AnomalyAI64.
    private ProjectileNetReference[] _Network_ProjectileReferenceCache = new ProjectileNetReference[AISlot2 - 1];
    #endregion


    #endregion
}
