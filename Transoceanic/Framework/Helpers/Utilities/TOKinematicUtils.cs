using Transoceanic.DataStructures;

namespace Transoceanic.Framework.Helpers;

/// <summary>
/// NPC优先级设定。
/// </summary>
public enum PriorityType : byte
{
    /// <summary>
    /// 距离最近单位。
    /// </summary>
    Closest = 0,
    /// <summary>
    /// 最大生命值最高单位。
    /// </summary>
    LifeMax = 1,
    /// <summary>
    /// 当前生命值最高单位。
    /// </summary>
    Life = 2
}

/// <summary>
/// 运动学工具类。
/// </summary>
public static class TOKinematicUtils
{
    /// <summary>
    /// 获取NPC目标。
    /// </summary>
    /// <param name="origin">追踪检索原点。</param>
    /// <param name="maxDistanceToCheck">最大检索距离。</param>
    /// <param name="ignoreTiles">是否忽视实体物块。</param>
    /// <param name="bossPriority">是否对Boss具有优先级（即若能获取到Boss目标，则不获取非Boss目标）。</param>
    /// <param name="priorityType">优先级类型。<para/>可选择获取距离最近目标、生命值最低目标、最大生命值最高目标。</param>
    /// <returns>获取到的NPC目标。如未获取成功，返回 <see langword="null"/>。</returns>
    /// <remarks><strong>警告</strong> 遍历NPC对性能有较大影响，应只在需要时调用该方法。</remarks>
    public static NPC GetNPCTarget(Vector2 origin, float maxDistanceToCheck, bool ignoreTiles = true, bool bossPriority = false, PriorityType priorityType = PriorityType.Closest)
    {
        float maxDistanceToCheckSquared = maxDistanceToCheck * maxDistanceToCheck;
        NPC target = null;
        bool hasPriority = false;
        switch (priorityType)
        {
            case PriorityType.LifeMax:
                if (bossPriority)
                {
                    foreach (NPC npc in
                        TOIteratorFactory.NewNPCIterator(
                            ignoreTiles ? n => n.CanBeChasedBy() && Vector2.DistanceSquared(origin, n.Center) <= maxDistanceToCheckSquared
                            : n => n.CanBeChasedBy() && Vector2.DistanceSquared(origin, n.Center) <= maxDistanceToCheckSquared && Collision.CanHit(origin, 1, 1, n.Center, 1, 1)))
                    {
                        if (target is null)
                        {
                            target = npc;
                            continue;
                        }
                        switch (hasPriority, npc.IsBossEnemy)
                        {
                            case (true, false):
                                break;
                            case (false, true):
                                target = npc;
                                hasPriority = true;
                                break;
                            case (true, true) or (false, false) when npc.lifeMax > target.lifeMax:
                                target = npc;
                                break;
                        }
                    }
                }
                else
                {
                    foreach (NPC npc in
                        TOIteratorFactory.NewNPCIterator(
                            ignoreTiles ? n => n.CanBeChasedBy() && Vector2.DistanceSquared(origin, n.Center) <= maxDistanceToCheckSquared
                            : n => n.CanBeChasedBy() && Vector2.DistanceSquared(origin, n.Center) <= maxDistanceToCheckSquared && Collision.CanHit(origin, 1, 1, n.Center, 1, 1)))
                    {
                        if (target is null || npc.lifeMax > target.lifeMax)
                            target = npc;
                    }
                }
                return target;
            case PriorityType.Life:
                if (bossPriority)
                {
                    foreach (NPC npc in
                        TOIteratorFactory.NewNPCIterator(
                            ignoreTiles ? n => n.CanBeChasedBy() && Vector2.DistanceSquared(origin, n.Center) <= maxDistanceToCheckSquared
                            : n => n.CanBeChasedBy() && Vector2.DistanceSquared(origin, n.Center) <= maxDistanceToCheckSquared && Collision.CanHit(origin, 1, 1, n.Center, 1, 1)))
                    {
                        if (target is null)
                        {
                            target = npc;
                            continue;
                        }
                        switch (hasPriority, npc.IsBossEnemy)
                        {
                            case (true, false):
                                break;
                            case (false, true):
                                target = npc;
                                hasPriority = true;
                                break;
                            case (true, true) or (false, false) when npc.life > target.life:
                                target = npc;
                                break;
                        }
                    }
                }
                else
                {
                    foreach (NPC npc in
                        TOIteratorFactory.NewNPCIterator(
                            ignoreTiles ? n => n.CanBeChasedBy() && Vector2.DistanceSquared(origin, n.Center) <= maxDistanceToCheckSquared
                            : n => n.CanBeChasedBy() && Vector2.DistanceSquared(origin, n.Center) <= maxDistanceToCheckSquared && Collision.CanHit(origin, 1, 1, n.Center, 1, 1)))
                    {
                        if (target is null || npc.life < target.life)
                            target = npc;
                    }
                }
                return target;
            case PriorityType.Closest:
            default:
                float distanceTemp1 = 0f;
                float distanceTemp2 = maxDistanceToCheckSquared;
                if (bossPriority)
                {
                    foreach (NPC npc in
                        TOIteratorFactory.NewNPCIterator(
                            ignoreTiles ? n => n.CanBeChasedBy() && (distanceTemp1 = Vector2.DistanceSquared(origin, n.Center)) <= maxDistanceToCheckSquared
                            : n => n.CanBeChasedBy() && (distanceTemp1 = Vector2.DistanceSquared(origin, n.Center)) <= maxDistanceToCheckSquared && Collision.CanHit(origin, 1, 1, n.Center, 1, 1)))
                    {
                        if (target is null)
                        {
                            target = npc;
                            distanceTemp2 = distanceTemp1;
                            continue;
                        }
                        switch (hasPriority, npc.IsBossEnemy)
                        {
                            case (true, false):
                                break;
                            case (false, true):
                                target = npc;
                                distanceTemp2 = distanceTemp1;
                                hasPriority = true;
                                break;
                            case (true, true) or (false, false) when distanceTemp1 < distanceTemp2:
                                target = npc;
                                distanceTemp2 = distanceTemp1;
                                break;
                        }
                    }
                }
                else
                {
                    foreach (NPC npc in
                        TOIteratorFactory.NewNPCIterator(
                            ignoreTiles ? n => n.CanBeChasedBy() && (distanceTemp1 = Vector2.DistanceSquared(origin, n.Center)) <= maxDistanceToCheckSquared
                            : n => n.CanBeChasedBy() && (distanceTemp1 = Vector2.DistanceSquared(origin, n.Center)) <= maxDistanceToCheckSquared && Collision.CanHit(origin, 1, 1, n.Center, 1, 1)))
                    {
                        if (target is null || distanceTemp1 < distanceTemp2)
                        {
                            target = npc;
                            distanceTemp2 = distanceTemp1;
                        }
                    }
                }
                return target;
        }
    }

    /// <summary>
    /// 获取玩家目标。
    /// </summary>
    /// <param name="origin">追踪检索原点。</param>
    /// <param name="maxDistanceToCheck">最大检索距离。</param>
    /// <param name="ignoreTiles">是否忽视实体物块。</param>
    /// <param name="priorityType">优先级类型。<para/>可选择获取距离最近目标、生命值最低目标、最大生命值最高目标。</param>
    /// <returns>获取到的玩家目标。如未获取成功，返回 <see langword="null"/>。</returns>
    /// <remarks><strong>警告</strong> 遍历玩家对性能有较大影响，应只在需要时调用该方法。该方法应由NPC调用。</remarks>
    public static Player GetPlayerTarget(Vector2 origin, float maxDistanceToCheck, bool ignoreTiles = true, PriorityType priorityType = PriorityType.Closest)
    {
        float maxDistanceToCheckSquared = maxDistanceToCheck * maxDistanceToCheck;

        if (Main.netMode == NetmodeID.SinglePlayer)
            return Main.LocalPlayer.Alive && Vector2.DistanceSquared(origin, Main.LocalPlayer.Center) <= maxDistanceToCheckSquared ? Main.LocalPlayer : null;

        Player target = null;
        switch (priorityType)
        {
            case PriorityType.LifeMax:
                foreach (Player player in
                    TOIteratorFactory.NewPlayerIterator(
                        ignoreTiles ? p => p.Alive && Vector2.DistanceSquared(origin, p.Center) <= maxDistanceToCheckSquared
                        : p => p.Alive && Vector2.DistanceSquared(origin, p.Center) <= maxDistanceToCheckSquared && Collision.CanHit(origin, 1, 1, p.Center, 1, 1)))
                {
                    if (target is null || player.statLifeMax2 > target.statLifeMax2)
                        target = player;
                }
                return target;
            case PriorityType.Life:
                foreach (Player player in
                    TOIteratorFactory.NewPlayerIterator(
                        ignoreTiles ? p => p.Alive && Vector2.DistanceSquared(origin, p.Center) <= maxDistanceToCheckSquared
                        : p => p.Alive && Vector2.DistanceSquared(origin, p.Center) <= maxDistanceToCheckSquared && Collision.CanHit(origin, 1, 1, p.Center, 1, 1)))
                {
                    if (target is null || player.statLife > target.statLife)
                        target = player;
                }
                return target;
            case PriorityType.Closest:
            default:
                float distanceTemp1 = 0f;
                float distanceTemp2 = maxDistanceToCheckSquared;
                foreach (Player player in
                    TOIteratorFactory.NewPlayerIterator(
                        ignoreTiles ? p => p.Alive && (distanceTemp1 = Vector2.DistanceSquared(origin, p.Center)) <= maxDistanceToCheckSquared
                        : p => p.Alive && (distanceTemp1 = Vector2.DistanceSquared(origin, p.Center)) <= maxDistanceToCheckSquared && Collision.CanHit(origin, 1, 1, p.Center, 1, 1)))
                {
                    if (target is null || distanceTemp1 < distanceTemp2)
                    {
                        target = player;
                        distanceTemp2 = distanceTemp1;
                    }
                }
                return target;
        }
    }

    /// <summary>
    /// 获取PvP状态的玩家目标。
    /// </summary>
    /// <param name="owner">发起检索调用的玩家。不会将该玩家作为目标。</param>
    /// <param name="origin">追踪检索原点。</param>
    /// <param name="maxDistanceToCheck">最大检索距离。</param>
    /// <param name="ignoreTiles">是否忽视实体物块。</param>
    /// <param name="priorityType">优先级类型。<para/>可选择获取距离最近目标、生命值最低目标、最大生命值最高目标。</param>
    /// <returns>获取到的玩家目标。如未获取成功，返回 <see langword="null"/>。</returns>
    /// <remarks><strong>警告</strong> 遍历玩家对性能有较大影响，应只在需要时调用该方法。该方法应由玩家调用。</remarks>
    public static Player GetPvPPlayerTarget(Player owner, Vector2 origin, float maxDistanceToCheck, bool ignoreTiles = true, PriorityType priorityType = PriorityType.Closest)
    {
        if (Main.netMode == NetmodeID.SinglePlayer || !owner.active || !owner.hostile)
            return null;

        float maxDistanceToCheckSquared = maxDistanceToCheck * maxDistanceToCheck;
        Player target = null;

        switch (priorityType)
        {
            case PriorityType.LifeMax:
                foreach (Player player in
                    TOIteratorFactory.NewPlayerIterator(
                        ignoreTiles ? p => p.IsPvP && Vector2.DistanceSquared(origin, p.Center) <= maxDistanceToCheckSquared
                        : p => p.IsPvP && Vector2.DistanceSquared(origin, p.Center) <= maxDistanceToCheckSquared && Collision.CanHit(origin, 1, 1, p.Center, 1, 1), owner))
                {
                    if (target is null || player.statLifeMax2 > target.statLifeMax2)
                        target = player;
                }
                return target;
            case PriorityType.Life:
                foreach (Player player in
                    TOIteratorFactory.NewPlayerIterator(
                        ignoreTiles ? p => p.IsPvP && Vector2.DistanceSquared(origin, p.Center) <= maxDistanceToCheckSquared
                        : p => p.IsPvP && Vector2.DistanceSquared(origin, p.Center) <= maxDistanceToCheckSquared && Collision.CanHit(origin, 1, 1, p.Center, 1, 1), owner))
                {
                    if (target is null || player.statLife > target.statLife)
                        target = player;
                }
                return target;
            case PriorityType.Closest:
            default:
                float distanceTemp1 = 0f;
                float distanceTemp2 = maxDistanceToCheckSquared;
                foreach (Player player in
                    TOIteratorFactory.NewPlayerIterator(
                        ignoreTiles ? p => p.IsPvP && (distanceTemp1 = Vector2.DistanceSquared(origin, p.Center)) <= maxDistanceToCheckSquared
                        : p => p.IsPvP && (distanceTemp1 = Vector2.DistanceSquared(origin, p.Center)) <= maxDistanceToCheckSquared && Collision.CanHit(origin, 1, 1, p.Center, 1, 1), owner))
                {
                    if (target is null || distanceTemp1 < distanceTemp2)
                    {
                        target = player;
                        distanceTemp2 = distanceTemp1;
                    }
                }
                return target;
        }
    }
}