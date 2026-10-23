namespace Anomalies.Common;

public sealed partial class AnomalyPlayer
{
    #region 独立实例数据
    #region 寒心
    public int Coldheart_Phase;
    public int Coldheart_SubPhase;
    #endregion

    #region 减益
    public bool Debuff_DimensionalRend;
    #endregion

    #region 落日遗逢
    public int EventideReunion_DeathCooldown;
    public int EventideReunion_DeathProtection;
    public byte EventideReunion_ResonanceMask;
    public int EventideReunion_ResonanceTime;

    // 落日遗逢的本地输入状态，不参与网络快照。
    public bool EventideReunion_RightWasDown;
    public int EventideReunion_SamsaraCooldown;
    public int EventideReunion_SunCooldown;
    public int EventideReunion_SunFallProtection;
    #endregion

    #region 传奇之夜
    public bool LegendaryNight_StarGodBlessingActive;
    public float LegendaryNight_StarGodBlessingOpacity;
    public Item LegendaryNight_VirtualBoneWings;
    #endregion

    #region 召唤物
    public bool Minion_VacuousBlack;
    #endregion

    #region 玩家进度
    public PlayerDownedBoss Progression_DownedBoss = new();
    #endregion

    #region 通用视觉效果
    public float Visual_DarknessIntensity
    {
        get;
        set => field = MathHelper.Clamp(value, 0f, 1f);
    }
    #endregion

    #endregion
}
