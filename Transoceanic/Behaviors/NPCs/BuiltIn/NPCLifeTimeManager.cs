using Transoceanic.Game.Timing;
namespace Transoceanic.Behaviors.NPCs.BuiltIn;

/// <summary>
/// 初始化 NPC 主人引用，记录生成时刻并累计存活帧数。
/// </summary>
internal sealed class NPCLifeTimeManager : GlobalNPCBehavior
{
    /// <inheritdoc/>
    /// <remarks>此实现返回 <c>1000m</c>。</remarks>
    public override decimal Priority => 1000m;

    /// <inheritdoc/>
    /// <remarks>
    /// 清除 NPC 的主人引用。
    /// </remarks>
    public override void SetDefaults(NPC npc)
    {
        npc.Master = null;
    }

    /// <inheritdoc/>
    /// <remarks>
    /// 将全局游戏帧计时记录为 NPC 的生成时间。
    /// </remarks>
    public override void OnSpawn(NPC npc, IEntitySource source)
    {
        npc.SpawnTime = TOGameTime.GameTimer.TotalTicks;
    }

    /// <inheritdoc/>
    /// <remarks>
    /// 将 NPC 存活计时增加一并返回 true，继续执行 AI。
    /// </remarks>
    public override bool PreAI(NPC npc)
    {
        npc.ActiveTime++;
        return true;
    }
}

