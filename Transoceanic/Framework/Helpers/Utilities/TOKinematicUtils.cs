namespace Transoceanic.Framework.Helpers;

/// <summary>
/// 目标检索时的优先级类型。
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
/// 提供与运动及目标检索有关的工具方法，常用于 NPC 或投射物的 AI 逻辑。
/// </summary>
public static class TOKinematicUtils
{
    /// <summary>
    /// 根据指定条件检索有效的 NPC 目标。
    /// </summary>
    /// <param name="origin">检索的中心点（世界坐标）。</param>
    /// <param name="maxDistanceToCheck">最大检索距离（像素）。</param>
    /// <param name="ignoreTiles">是否忽略实体物块阻挡。若为 <see langword="true"/>，则只进行距离判断；若为 <see langword="false"/>，则需要视线通畅（<see cref="Collision.CanHit(Vector2, int, int, Vector2, int, int)"/>）。</param>
    /// <param name="bossPriority">是否优先锁定Boss单位。若为 <see langword="true"/>，则当范围内存在Boss时，只返回符合条件的Boss，否则再考虑普通敌怪。</param>
    /// <param name="priorityType">目标排序优先级类型，可选最近距离、最高最大生命值、最高当前生命值。</param>
    /// <returns>符合条件的 NPC 实例；若未检索到则返回 <see langword="null"/>。</returns>
    /// <remarks>
    /// <strong>警告：</strong>遍历 NPC 集合对性能有较大影响，应仅在必要的时候（例如弹幕索敌、召唤物 AI 更新）调用此方法，避免在每帧绘制中调用。
    /// </remarks>
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
    /// 根据指定条件检索有效的玩家目标（通常用于敌怪 AI 或 BOSS 行为）。
    /// </summary>
    /// <param name="origin">检索的中心点（世界坐标）。</param>
    /// <param name="maxDistanceToCheck">最大检索距离（像素）。</param>
    /// <param name="ignoreTiles">是否忽略实体物块阻挡。若为 <see langword="true"/>，则只进行距离判断；若为 <see langword="false"/>，则需要视线通畅。</param>
    /// <param name="priorityType">目标排序优先级类型，可选最近距离、最高最大生命值、最高当前生命值。</param>
    /// <returns>符合条件的 <see cref="Player"/> 实例；若未检索到则返回 <see langword="null"/>。在单人模式下直接返回本地玩家（如果在范围内）。</returns>
    /// <remarks>
    /// <strong>警告：</strong>遍历玩家集合对性能有较大影响，应仅在必要的时候（例如 NPC AI 更新）调用此方法。
    /// </remarks>
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
    /// 获取处于 PvP 状态的有效玩家目标（用于玩家间对抗的武器或弹幕）。
    /// </summary>
    /// <param name="owner">发起检索的玩家，该玩家自身不会被选为目标。</param>
    /// <param name="origin">检索的中心点（世界坐标）。</param>
    /// <param name="maxDistanceToCheck">最大检索距离（像素）。</param>
    /// <param name="ignoreTiles">是否忽略实体物块阻挡。若为 <see langword="true"/>，则只进行距离判断；若为 <see langword="false"/>，则需要视线通畅。</param>
    /// <param name="priorityType">目标排序优先级类型，可选最近距离、最高最大生命值、最高当前生命值。</param>
    /// <returns>符合条件的 PvP 玩家实例；若未检索到或当前不处于多人模式/PvP 状态，则返回 <see langword="null"/>。</returns>
    /// <remarks>
    /// <strong>警告：</strong>遍历玩家集合对性能有较大影响，应仅在必要的时候调用此方法。此方法仅应由玩家主动发起的逻辑调用（例如武器使用、弹幕更新）。
    /// </remarks>
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