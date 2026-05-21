// Developed by ColdsUx

namespace CalamityAnomalies.DataStructures;

public interface IDynamicDRHandler
{
    public abstract void Update(NPC npc);

    public abstract float GetCurrentDDR(NPC npc);
}

/// <summary>
/// 组合动态伤害减免处理器，可同时管理多个 <see cref="IDynamicDRHandler"/> 实例。
/// 更新时会依次调用所有内部处理器，获取总减伤值时将所有处理器的返回值累加并钳制到 [0, 1] 区间。
/// 构造函数会扁平化嵌套的 <see cref="CombinedDDRHandler"/>，避免层级嵌套。
/// </summary>
public sealed class CombinedDDRHandler : IDynamicDRHandler
{
    /// <summary>
    /// 存储所有待处理的动态伤害减免处理器集合。
    /// </summary>
    public List<IDynamicDRHandler> _handlers;

    /// <summary>
    /// 初始化组合处理器，并扁平化所有嵌套的 <see cref="CombinedDDRHandler"/>。
    /// </summary>
    /// <param name="handlers">要包含的处理器集合。若某个处理器本身是 <see cref="CombinedDDRHandler"/>，
    /// 则会将其内部的处理器展开并加入当前列表，以避免多层嵌套。</param>
    public CombinedDDRHandler(params IDynamicDRHandler[] handlers)
    {
        _handlers = [];
        FlattenAndAdd(handlers);
    }

    /// <summary>
    /// 递归遍历处理器数组，将非 <see cref="CombinedDDRHandler"/> 的处理器直接添加，
    /// 对 <see cref="CombinedDDRHandler"/> 则展开其内部 <see cref="_handlers"/> 列表并继续扁平化。
    /// </summary>
    /// <param name="handlers">待扁平化并添加的处理器数组。</param>
    private void FlattenAndAdd(IEnumerable<IDynamicDRHandler> handlers)
    {
        foreach (IDynamicDRHandler handler in handlers)
        {
            if (handler is CombinedDDRHandler combined) //递归处理
                FlattenAndAdd(combined._handlers);
            else
                _handlers.Add(handler);
        }
    }

    /// <summary>
    /// 依次更新内部所有处理器的状态。
    /// </summary>
    /// <param name="npc">目标 NPC 实例。</param>
    public void Update(NPC npc)
    {
        foreach (IDynamicDRHandler handler in _handlers)
            handler.Update(npc);
    }

    /// <summary>
    /// 计算当前帧的动态伤害减免总值。
    /// 累加所有内部处理器的当前减伤值，并将结果钳制在 [0, 1] 范围内。
    /// </summary>
    /// <param name="npc">目标 NPC 实例。</param>
    /// <returns>钳制到 [0, 1] 区间后的动态伤害减免值。</returns>
    public float GetCurrentDDR(NPC npc)
    {
        float total = 0f;

        foreach (IDynamicDRHandler handler in _handlers)
            total += handler.GetCurrentDDR(npc);

        return Math.Clamp(total, 0f, 1f);
    }
}

/// <summary>
/// 动态伤害减免处理器，用于根据战斗阶段的生命值进度与时间进度的差异动态调整伤害减免值。
/// 主要应用于故事模式中（非 Boss Rush），通过对 Boss 各阶段配置不同的参数来控制动态减伤强度。
/// </summary>
public sealed class TimedDDRHandler : IDynamicDRHandler
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

            if (factor > 0f && expectedTime > 0 && aiTimer < expectedTime && CASharedData.StoryMode && !BossRushEvent_Bridge.BossRushActive)
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
    /// 当前的动态伤害减免值。
    /// </summary>
    public float DynamicDR;

    /// <summary>
    /// 使用可变数量的阶段处理器列表初始化动态伤害减免处理器。
    /// </summary>
    /// <param name="phaseDRList">一个或多个 <see cref="SingleDDRHandler"/> 列表，表示所有阶段配置。</param>
    public TimedDDRHandler(params List<SingleDDRHandler> phaseDRList) => PhaseDRList = phaseDRList;

    /// <summary>
    /// 更新阶段处理器的内部状态，并计算动态伤害减免值。
    /// </summary>
    /// <param name="npc">目标 NPC 实例。</param>
    public void Update(NPC npc)
    {
        SingleDDRHandler currentHandler = GetCurrentDDRHandler(npc);

        float targetDR;

        if (currentHandler is null)
            targetDR = 0f;
        else
        {
            currentHandler.PhaseTimer++;
            if (currentHandler.PhaseEndFunction(npc))
                currentHandler.PhaseEnded = true;

            targetDR = currentHandler.CalculateDDR(npc);
        }

        DynamicDR = TOMathUtils.Max(0f, DynamicDR - 0.003f, targetDR);
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
    /// 获取动态伤害减免值。
    /// </summary>
    /// <param name="npc">目标 NPC 实例。</param>
    /// <returns>当前动态伤害减免值。</returns>
    public float GetCurrentDDR(NPC npc) => DynamicDR;
}
