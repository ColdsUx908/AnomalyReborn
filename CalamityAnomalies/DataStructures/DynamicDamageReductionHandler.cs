// Developed by ColdsUx

using CalamityMod.Events;

namespace CalamityAnomalies.DataStructures;

/// <summary>
/// 动态伤害减免处理器，用于根据战斗阶段的生命值进度与时间进度的差异动态调整伤害减免值。
/// 主要应用于故事模式中（非 Boss Rush），通过对 BOSS 各阶段配置不同的参数来控制动态减伤强度。
/// </summary>
public sealed class DynamicDamageReductionHandler
{
    /// <summary>
    /// 表示单个阶段的动态伤害减免配置与状态。
    /// 每个阶段根据生命值区间、时间预期以及自定义的开始/结束条件，计算实时的动态减伤值。
    /// </summary>
    public sealed class SingleDDRHandler
    {
        /// <summary>
        /// 默认的伤害减免因子，值为 7.5。
        /// </summary>
        public const float DefaultDRFactor = 7.5f;

        /// <summary>
        /// 阶段开始时的目标生命值比例。用于计算生命值进度。
        /// </summary>
        public float PhaseStartLifeRatio;

        /// <summary>
        /// 阶段结束时的目标生命值比例。用于计算生命值进度。
        /// </summary>
        public float PhaseEndLifeRatio;

        /// <summary>
        /// 判定该阶段是否开始的函数。传入目标 NPC，返回 <see langword="true"/> 表示当前处于该阶段。
        /// </summary>
        public Func<NPC, bool> PhaseStartFunction;

        /// <summary>
        /// 判定该阶段是否结束的函数。传入目标 NPC，返回 <see langword="true"/> 表示该阶段已结束。
        /// </summary>
        public Func<NPC, bool> PhaseEndFunction;

        /// <summary>
        /// 预期阶段持续时间（内部单位为帧，由构造函数将秒转为帧，每秒 60 帧）。
        /// </summary>
        public int ExpectedPhaseTime;

        /// <summary>
        /// 动态伤害减免因子。决定减伤值的理论上限（上限为因子的一半）和。
        /// </summary>
        public float DRFactor;

        /// <summary>
        /// 标记该阶段是否已经结束。结束后不再更新计时器，也不再参与动态减伤计算。
        /// </summary>
        public bool PhaseEnded;
        /// <summary>
        /// 阶段内部的计时器，记录从阶段开始后经过的帧数。
        /// </summary>
        public int PhaseTimer;

        /// <summary>
        /// 初始化单个阶段的动态减免处理器。
        /// </summary>
        /// <param name="phaseStartLifeRatio">阶段开始生命比例，默认为 1（满血）。</param>
        /// <param name="phaseEndLifeRatio">阶段结束生命比例，默认为 0（空血）。</param>
        /// <param name="phaseStartFunction">判定阶段开始的函数，若为 <see langword="null"/> 则默认始终返回 true。</param>
        /// <param name="phaseEndFunction">判定阶段结束的函数，若为 <see langword="null"/> 则默认始终返回 false。</param>
        /// <param name="expectedPhaseTimeInSecond">预期阶段持续时间（秒），内部会乘以 60 转换为帧数。</param>
        /// <param name="drFactor">伤害减免因子，默认为 <see cref="DefaultDRFactor"/>。</param>
        public SingleDDRHandler(float phaseStartLifeRatio = 1f, float phaseEndLifeRatio = 0f, Func<NPC, bool> phaseStartFunction = null, Func<NPC, bool> phaseEndFunction = null, int expectedPhaseTimeInSecond = 0, float drFactor = DefaultDRFactor)
        {
            PhaseStartLifeRatio = phaseStartLifeRatio;
            PhaseEndLifeRatio = phaseEndLifeRatio;
            PhaseStartFunction = phaseStartFunction ?? (_ => true);
            PhaseEndFunction = phaseEndFunction ?? (_ => false);
            ExpectedPhaseTime = expectedPhaseTimeInSecond * 60;
            DRFactor = drFactor;
        }

        /// <summary>
        /// 计算当前帧的动态伤害减免值（Dynamic Damage Reduction）。
        /// <br/>该值仅在故事模式且非 Boss Rush 活动中，且预期时间大于 0、计时器未超过预期时间时才会计算。
        /// <para/>算法：
        /// <br/>1. 根据当前生命比例计算生命值完成度 <c>lifeCompletion</c>（在阶段生命区间内的线性插值）。
        /// <br/>2. 根据已用时间与预期时间计算时间完成度 <c>timeCompletion</c>。
        /// <br/>3. 计算强度 <c>ddrIntensity = lifeCompletion - timeCompletion</c>。
        /// <br/>4. 若 <c>ddrIntensity > 0</c>，则减伤值 = <c>drLimit * ddrIntensity / (1 + ddrIntensity)</c>，其中 <c>drLimit = DRFactor / 2</c>。
        /// <br/>公式可使减伤值随强度增大而增加。
        /// </summary>
        /// <param name="npc">目标 NPC 实例。</param>
        /// <returns>当前动态伤害减免值，范围一般为 0 到 <c>DRFactor</c>。不满足条件时返回 0。</returns>
        public float CalculateDDR(NPC npc)
        {
            float dynamicDR = 0f;

            float factor = DRFactor;
            int expectedTime = ExpectedPhaseTime;
            int aiTimer = PhaseTimer;

            if (factor > 0f && expectedTime > 0 && aiTimer < expectedTime && CASharedData.StoryMode && !BossRushEvent.BossRushActive)
            {
                float lifeCompletion = Utils.GetLerpValue(PhaseStartLifeRatio, PhaseEndLifeRatio, npc.LifeRatio);
                float timeCompletion = (float)aiTimer / expectedTime;
                float ddrIntensity = lifeCompletion - timeCompletion;
                if (ddrIntensity > 0f)
                    dynamicDR = factor * ddrIntensity / (1 + ddrIntensity);
            }

            return dynamicDR;
        }
    }

    /// <summary>
    /// 存储所有阶段处理器的列表，按顺序定义 Boss 各个阶段的动态减伤行为。
    /// </summary>
    public List<SingleDDRHandler> PhaseDRList = [];

    /// <summary>
    /// 使用可变数量的阶段处理器列表初始化动态伤害减免处理器。
    /// </summary>
    /// <param name="phaseDRList">一个或多个 <see cref="SingleDDRHandler"/> 列表，表示所有阶段配置。</param>
    public DynamicDamageReductionHandler(params List<SingleDDRHandler> phaseDRList) => PhaseDRList = phaseDRList;

    /// <summary>
    /// 每帧更新所有阶段处理器的内部状态。
    /// 对于所有尚未结束的阶段，递增其计时器，并检查阶段结束条件是否满足。
    /// </summary>
    /// <param name="npc">目标 NPC 实例。</param>
    public void Update(NPC npc)
    {
        SingleDDRHandler currentHandler = GetCurrentDDRHandler(npc);

        if (currentHandler is null)
            return;

        currentHandler.PhaseTimer++;
        if (currentHandler.PhaseEndFunction(npc))
            currentHandler.PhaseEnded = true;
    }

    /// <summary>
    /// 获取当前处于激活状态的阶段处理器。
    /// 遍历列表，返回第一个未结束且满足 <see cref="SingleDDRHandler.PhaseStartFunction"/> 的处理器。
    /// 如果没有符合条件的阶段，则返回 <see langword="null"/>。
    /// </summary>
    /// <param name="npc">目标 NPC 实例。</param>
    /// <returns>当前激活的 <see cref="SingleDDRHandler"/>，若无则返回 <see langword="null"/>。</returns>
    public SingleDDRHandler GetCurrentDDRHandler(NPC npc)
    {
        foreach (SingleDDRHandler phaseDR in PhaseDRList)
        {
            if (!phaseDR.PhaseEnded)
                return phaseDR.PhaseStartFunction(npc) ? phaseDR : null;
        }
        return null;
    }

    /// <summary>
    /// 获取当前帧的动态伤害减免值。
    /// 通过 <see cref="GetCurrentDDRHandler(NPC)"/> 获取当前激活的阶段处理器，并调用其 <see cref="SingleDDRHandler.CalculateDDR(NPC)"/> 方法计算。
    /// </summary>
    /// <param name="npc">目标 NPC 实例。</param>
    /// <returns>当前动态伤害减免值，若无激活阶段则返回 0。</returns>
    public float GetCurrentDDR(NPC npc)
    {
        SingleDDRHandler currentHandler = GetCurrentDDRHandler(npc);
        return currentHandler?.CalculateDDR(npc) ?? 0f;
    }
}
