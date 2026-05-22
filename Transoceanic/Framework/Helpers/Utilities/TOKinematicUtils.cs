// Developed by ColdsUx

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
    /// 当前生命值最低单位。
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
    /// <param name="ignoreTiles">是否忽略实体物块阻挡。若为 true，则只进行距离判断；若为 false，则需要视线通畅（<see cref="Collision.CanHit(Vector2, int, int, Vector2, int, int)"/>）。</param>
    /// <param name="priorityType">目标排序优先级类型，可选最近距离、最高最大生命值、最低当前生命值。</param>
    /// <param name="bossPriority">是否优先锁定Boss单位。若为 true，则当范围内存在Boss时，只返回符合条件的Boss，否则再考虑普通敌怪。</param>
    /// <returns>符合条件的 NPC 实例；若未检索到则返回 null。</returns>
    /// <remarks>
    /// <strong>警告：</strong>遍历 NPC 集合对性能有较大影响，应仅在必要的时候（例如弹幕索敌、召唤物 AI 更新）调用此方法，避免在每帧绘制中调用。
    /// </remarks>
    public static NPC GetNPCTarget(Vector2 origin, float maxDistanceToCheck, bool ignoreTiles = true, PriorityType priorityType = PriorityType.Closest, bool bossPriority = false)
    {
        float maxDistanceSquared = maxDistanceToCheck * maxDistanceToCheck;
        NPC target = null;
        float bestValue = 0f;

        if (bossPriority)
        {
            bool anyBoss = false;

            foreach (NPC npc in NPC.ActiveNPCs)
            {
                if (!npc.CanBeChasedBy())
                    continue;

                float distanceSquared = Vector2.DistanceSquared(origin, npc.Center);
                if (distanceSquared > maxDistanceSquared)
                    continue;

                if (!ignoreTiles && !Collision.CanHit(origin, 1, 1, npc.Center, 1, 1))
                    continue;

                // 首次命中或与当前最佳目标比较
                if (target is null)
                {
                    target = npc;
                    anyBoss = npc.IsBossEnemy;
                    bestValue = priorityType switch
                    {
                        PriorityType.Closest => distanceSquared,
                        PriorityType.LifeMax => npc.lifeMax,
                        PriorityType.Life => npc.life,
                        _ => distanceSquared
                    };
                }
                else
                {
                    switch (anyBoss, npc.IsBossEnemy)
                    {
                        case (true, false):
                            break;
                        case (false, true):
                            target = npc;
                            anyBoss = true;
                            bestValue = priorityType switch
                            {
                                PriorityType.Closest => distanceSquared,
                                PriorityType.LifeMax => npc.lifeMax,
                                PriorityType.Life => npc.life,
                                _ => distanceSquared
                            };
                            break;
                        case (true, true) or (false, false):
                            TryReplaceTarget(ref target, npc, distanceSquared, ref bestValue, priorityType);
                            break;
                    }
                }
            }
        }
        else
        {
            foreach (NPC npc in NPC.ActiveNPCs)
            {
                if (!npc.CanBeChasedBy())
                    continue;

                float distanceSquared = Vector2.DistanceSquared(origin, npc.Center);
                if (distanceSquared > maxDistanceSquared)
                    continue;

                if (!ignoreTiles && !Collision.CanHit(origin, 1, 1, npc.Center, 1, 1))
                    continue;

                // 首次命中或与当前最佳目标比较
                if (target is null)
                {
                    target = npc;
                    bestValue = priorityType switch
                    {
                        PriorityType.Closest => distanceSquared,
                        PriorityType.LifeMax => npc.lifeMax,
                        PriorityType.Life => npc.life,
                        _ => distanceSquared
                    };
                }
                else
                    TryReplaceTarget(ref target, npc, distanceSquared, ref bestValue, priorityType);
            }
        }

        return target;
    }

    /// <summary>
    /// 根据指定条件检索有效的 NPC 目标。
    /// 可选择性考虑弹幕的无敌帧，以排除当前无法命中的 NPC。
    /// 可选择性忽略 NPC 的可追逐属性。
    /// </summary>
    /// <param name="origin">检索的中心点（世界坐标）。</param>
    /// <param name="maxDistanceToCheck">最大检索距离（像素）。</param>
    /// <param name="ignoreTiles">是否忽略实体物块阻挡。若为 <see langword="true"/>，则只进行距离判断；若为 <see langword="false"/>，则需要视线通畅（<see cref="Collision.CanHit(Vector2, int, int, Vector2, int, int)"/>）。</param>
    /// <param name="priorityType">目标排序优先级类型，可选最近距离、最高最大生命值、最低当前生命值。</param>
    /// <param name="respectImmuneFrames">是否尊重弹幕无敌帧。若为 <see langword="true"/>，且 <paramref name="projectile"/> 不为 <see langword="null"/>，则不会返回当前对该弹幕处于无敌状态的 NPC（即 <see cref="Projectile.localNPCImmunity"/> 或 <see cref="NPC.immune"/> 计数尚未结束）。</param>
    /// <param name="projectile">进行索敌的弹幕实例，用于检查 NPC 对该弹幕的免疫状态。</param>
    /// <param name="ignoreChaseable">是否忽略可追逐性判断。若为 <see langword="true"/>，则可追踪 <see cref="NPC.chaseable"/> 为 <see langword="false"/> 的 NPC（仍以 <see cref="NPC.chaseable"/> 为 <see langword="true"/> 的为优先）；若为 <see langword="false"/>，则需要满足可追逐条件。</param>
    /// <param name="bossPriority">是否优先锁定Boss单位。若为 <see langword="true"/>，则当范围内存在Boss时，只返回符合条件的Boss，否则再考虑普通敌怪。</param>
    /// <returns>符合条件的 NPC 实例；若未检索到则返回 <see langword="null"/>。</returns>
    /// <remarks>
    /// <strong>警告：</strong>遍历 NPC 集合对性能有较大影响，应仅在必要的时候（例如弹幕索敌、召唤物 AI 更新）调用此方法，避免在每帧绘制中调用。
    /// </remarks>
    public static NPC GetNPCTarget_Advanced(Vector2 origin, float maxDistanceToCheck, bool ignoreTiles = true, PriorityType priorityType = PriorityType.Closest, bool respectImmuneFrames = false, Projectile projectile = null, bool ignoreChaseable = false, bool bossPriority = false)
    {
        bool hasProjectile = projectile is not null;
        float maxDistanceSquared = maxDistanceToCheck * maxDistanceToCheck;
        NPC target = null;
        float bestValue = 0f;

        if (ignoreChaseable)
        {
            if (bossPriority)
            {
                bool anyBoss = false;
                bool anyChaseable = false;

                foreach (NPC npc in NPC.ActiveNPCs)
                {
                    if (!npc.CanBeChasedBy_IgnoreChaseable())
                        continue;
                    float distanceSquared = Vector2.DistanceSquared(origin, npc.Center);
                    if (distanceSquared > maxDistanceSquared)
                        continue;
                    if (!ignoreTiles && !Collision.CanHit(origin, 1, 1, npc.Center, 1, 1))
                        continue;
                    if (respectImmuneFrames && hasProjectile && (projectile.localNPCImmunity[npc.whoAmI] is -1 or > 0 || npc.immune[projectile.owner] > 0))
                        continue;

                    if (target is null)
                    {
                        target = npc;
                        anyBoss = npc.IsBossEnemy;
                        anyChaseable = npc.chaseable;
                        bestValue = priorityType switch
                        {
                            PriorityType.Closest => distanceSquared,
                            PriorityType.LifeMax => npc.lifeMax,
                            PriorityType.Life => npc.life,
                            _ => distanceSquared
                        };
                    }
                    else
                    {
                        switch ((anyBoss, npc.IsBossEnemy))
                        {
                            case (true, false):
                                break;
                            case (false, true):
                                target = npc;
                                anyBoss = true;
                                anyChaseable = npc.chaseable;
                                bestValue = priorityType switch
                                {
                                    PriorityType.Closest => distanceSquared,
                                    PriorityType.LifeMax => npc.lifeMax,
                                    PriorityType.Life => npc.life,
                                    _ => distanceSquared
                                };
                                break;
                            case (true, true) or (false, false):
                                switch (anyChaseable, npc.chaseable)
                                {
                                    case (true, false):
                                        break;
                                    case (false, true):
                                        target = npc;
                                        anyChaseable = true;
                                        bestValue = priorityType switch
                                        {
                                            PriorityType.Closest => distanceSquared,
                                            PriorityType.LifeMax => npc.lifeMax,
                                            PriorityType.Life => npc.life,
                                            _ => distanceSquared
                                        };
                                        break;
                                    case (true, true) or (false, false):
                                        TryReplaceTarget(ref target, npc, distanceSquared, ref bestValue, priorityType);
                                        break;
                                }
                                break;
                        }
                    }
                }
            }
            else
            {
                bool anyChaseable = false;

                foreach (NPC npc in NPC.ActiveNPCs)
                {
                    if (!npc.CanBeChasedBy_IgnoreChaseable())
                        continue;
                    float distanceSquared = Vector2.DistanceSquared(origin, npc.Center);
                    if (distanceSquared > maxDistanceSquared)
                        continue;
                    if (!ignoreTiles && !Collision.CanHit(origin, 1, 1, npc.Center, 1, 1))
                        continue;
                    if (respectImmuneFrames && hasProjectile && (projectile.localNPCImmunity[npc.whoAmI] is -1 or > 0 || npc.immune[projectile.owner] > 0))
                        continue;

                    // 首次命中：直接记录目标与对应属性
                    if (target is null)
                    {
                        target = npc;
                        anyChaseable = npc.chaseable;
                        bestValue = priorityType switch
                        {
                            PriorityType.Closest => distanceSquared,
                            PriorityType.LifeMax => npc.lifeMax,
                            PriorityType.Life => npc.life,
                            _ => distanceSquared
                        };
                    }
                    else
                    {
                        switch (anyChaseable, npc.chaseable)
                        {
                            case (true, false):
                                break;

                            case (false, true):
                                target = npc;
                                anyChaseable = true;
                                bestValue = priorityType switch
                                {
                                    PriorityType.Closest => distanceSquared,
                                    PriorityType.LifeMax => npc.lifeMax,
                                    PriorityType.Life => npc.life,
                                    _ => distanceSquared
                                };
                                break;

                            case (true, true) or (false, false):
                                TryReplaceTarget(ref target, npc, distanceSquared, ref bestValue, priorityType);
                                break;
                        }
                    }
                }
            }
        }
        else
        {
            if (bossPriority)
            {
                bool anyBoss = false;

                foreach (NPC npc in NPC.ActiveNPCs)
                {
                    if (!npc.CanBeChasedBy())
                        continue;
                    float distanceSquared = Vector2.DistanceSquared(origin, npc.Center);
                    if (distanceSquared > maxDistanceSquared)
                        continue;
                    if (!ignoreTiles && !Collision.CanHit(origin, 1, 1, npc.Center, 1, 1))
                        continue;
                    if (respectImmuneFrames && hasProjectile && (projectile.localNPCImmunity[npc.whoAmI] is -1 or > 0 || npc.immune[projectile.owner] > 0))
                        continue;

                    // 首次命中：直接记录目标与对应属性
                    if (target is null)
                    {
                        target = npc;
                        anyBoss = npc.IsBossEnemy;
                        bestValue = priorityType switch
                        {
                            PriorityType.Closest => distanceSquared,
                            PriorityType.LifeMax => npc.lifeMax,
                            PriorityType.Life => npc.life,
                            _ => distanceSquared
                        };
                    }
                    else
                    {
                        switch (anyBoss, npc.IsBossEnemy)
                        {
                            case (true, false):
                                break;

                            case (false, true):
                                target = npc;
                                anyBoss = true;
                                bestValue = priorityType switch
                                {
                                    PriorityType.Closest => distanceSquared,
                                    PriorityType.LifeMax => npc.lifeMax,
                                    PriorityType.Life => npc.life,
                                    _ => distanceSquared
                                };
                                break;

                            case (true, true) or (false, false):
                                TryReplaceTarget(ref target, npc, distanceSquared, ref bestValue, priorityType);
                                break;
                        }
                    }
                }
            }
            else
            {
                foreach (NPC npc in NPC.ActiveNPCs)
                {
                    if (!npc.CanBeChasedBy())
                        continue;
                    float distanceSquared = Vector2.DistanceSquared(origin, npc.Center);
                    if (distanceSquared > maxDistanceSquared)
                        continue;
                    if (!ignoreTiles && !Collision.CanHit(origin, 1, 1, npc.Center, 1, 1))
                        continue;
                    if (respectImmuneFrames && hasProjectile && (projectile.localNPCImmunity[npc.whoAmI] is -1 or > 0 || npc.immune[projectile.owner] > 0))
                        continue;
                    if (target is null)
                    {
                        target = npc;
                        bestValue = priorityType switch
                        {
                            PriorityType.Closest => distanceSquared,
                            PriorityType.LifeMax => npc.lifeMax,
                            PriorityType.Life => npc.life,
                            _ => distanceSquared
                        };
                    }
                    else
                        TryReplaceTarget(ref target, npc, distanceSquared, ref bestValue, priorityType);
                }
            }
        }

        return target;
    }

    /// <summary>
    /// 比较两个 NPC 的对应属性，若候选者更优则替换当前目标。
    /// 此方法不涉及 Boss 优先级，仅执行纯属性比较。
    /// </summary>
    /// <param name="current">当前最佳目标（引用，可能被替换）。</param>
    /// <param name="other">待比较的新候选 NPC。</param>
    /// <param name="otherDistanceSq">候选 NPC 与搜索起点的距离平方（仅在优先级为 Closest 时使用）。</param>
    /// <param name="bestValue">当前最佳值（Closest 时为最小距离平方，LifeMax 时为最高最大生命值，Life 时为最低当前生命值），可能被更新。</param>
    /// <param name="priorityType">排序优先级类型。</param>
    private static void TryReplaceTarget(ref NPC current, NPC other, float otherDistanceSq, ref float bestValue, PriorityType priorityType)
    {
        switch (priorityType)
        {
            case PriorityType.Closest:
                if (otherDistanceSq < bestValue)
                {
                    current = other;
                    bestValue = otherDistanceSq;
                }
                break;

            case PriorityType.LifeMax:
                if (other.lifeMax > bestValue)
                {
                    current = other;
                    bestValue = other.lifeMax;
                }
                break;

            case PriorityType.Life:
                if (other.life < bestValue)
                {
                    current = other;
                    bestValue = other.life;
                }
                break;
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
                foreach (Player player in Player.AlivePlayers)
                {
                    if (Vector2.DistanceSquared(origin, player.Center) > maxDistanceToCheckSquared || (!ignoreTiles && !Collision.CanHit(origin, 1, 1, player.Center, 1, 1)))
                        continue;

                    if (target is null || player.statLifeMax2 > target.statLifeMax2)
                        target = player;
                }
                return target;
            case PriorityType.Life:
                foreach (Player player in Player.AlivePlayers)
                {
                    if (Vector2.DistanceSquared(origin, player.Center) > maxDistanceToCheckSquared || (!ignoreTiles && !Collision.CanHit(origin, 1, 1, player.Center, 1, 1)))
                        continue;

                    if (target is null || player.statLife > target.statLife)
                        target = player;
                }
                return target;
            case PriorityType.Closest:
            default:
                float distanceTemp = maxDistanceToCheckSquared;
                foreach (Player player in Player.AlivePlayers)
                {
                    float distanceSquared = Vector2.DistanceSquared(origin, player.Center);
                    if (distanceSquared > maxDistanceToCheckSquared || (!ignoreTiles && !Collision.CanHit(origin, 1, 1, player.Center, 1, 1)))
                        continue;

                    if (target is null || distanceSquared < distanceTemp)
                    {
                        target = player;
                        distanceTemp = distanceSquared;
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
                foreach (Player player in Player.PVPPlayers)
                {
                    if (player == owner || Vector2.DistanceSquared(origin, player.Center) > maxDistanceToCheckSquared || (!ignoreTiles && !Collision.CanHit(origin, 1, 1, player.Center, 1, 1)))
                        continue;

                    if (target is null || player.statLifeMax2 > target.statLifeMax2)
                        target = player;
                }
                return target;
            case PriorityType.Life:
                foreach (Player player in Player.PVPPlayers)
                {
                    if (player == owner || Vector2.DistanceSquared(origin, player.Center) > maxDistanceToCheckSquared || (!ignoreTiles && !Collision.CanHit(origin, 1, 1, player.Center, 1, 1)))
                        continue;

                    if (target is null || player.statLife > target.statLife)
                        target = player;
                }
                return target;
            case PriorityType.Closest:
            default:
                float distanceTemp = maxDistanceToCheckSquared;
                foreach (Player player in Player.PVPPlayers)
                {
                    float distanceSquared = Vector2.DistanceSquared(origin, player.Center);
                    if (player == owner || distanceSquared > maxDistanceToCheckSquared || (!ignoreTiles && !Collision.CanHit(origin, 1, 1, player.Center, 1, 1)))
                        continue;

                    if (target is null || distanceSquared < distanceTemp)
                    {
                        target = player;
                        distanceTemp = distanceSquared;
                    }
                }
                return target;
        }
    }
}