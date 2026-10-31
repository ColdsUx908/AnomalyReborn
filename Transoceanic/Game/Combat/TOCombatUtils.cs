namespace Transoceanic.Game.Combat;

/// <summary>
/// 指定 NPC 选择玩家目标时如何处理当前目标与仇恨优先级。
/// 该策略由 <see cref="NPCTargetingParameters.TargetType"/> 传给 <see cref="TOCombatUtils.TargetWithOptions"/> 使用。
/// </summary>
public enum NPCTargetPreference
{
    /// <summary>
    /// 选取当前有效目标中综合距离与仇恨值最优的玩家，不额外偏向先前目标。
    /// </summary>
    Anyone,
    /// <summary>
    /// 只要先前目标仍符合筛选条件，就优先保留该玩家，即使其他玩家距离更近或仇恨更高。
    /// </summary>
    PreferSame,
    /// <summary>
    /// 在多人模式中排除先前目标并尝试切换至其他有效玩家；单人模式没有可切换对象，因此此策略不产生切换。
    /// </summary>
    ForceSwitch
}

/// <summary>
/// 汇总 <see cref="TOCombatUtils.TargetWithOptions"/> 的目标筛选、仇恨计算和朝向选项。
/// 使用 <see cref="Defaults"/> 或 <see cref="BossDefaults"/> 创建有效默认值；避免以 <see langword="default"/> 初始化，因为结构体默认值会绕过字段初始值。
/// </summary>
public struct NPCTargetingParameters : IEquatable<NPCTargetingParameters>
{
    // Vanilla argument to TargetClosest. Defaults to true as it does in vanilla.
    // If true, the NPC will turn to face the target.
    /// <summary>
    /// 指定成功选定玩家后是否更新 NPC 的水平和垂直朝向。
    /// 对无效目标、死亡玩家或忽略该 NPC 仇恨的玩家，朝向仍受原版目标规则限制。
    /// </summary>
    public bool FaceTarget = true;
    // Vanilla argument to TargetClosestUpgraded. That function is never used, but the flexibility is implemented here.
    // Allows targeting distance calculations to be measured from a different position than the NPC's center, if desired.
    /// <summary>
    /// 指定计算目标距离时使用的参考点；未设置时使用 NPC 中心。
    /// 视线检测仍从 NPC 中心执行，因此该值只改变距离测量位置。
    /// </summary>
    public Vector2? TargetingCenter = null;
    // Hard maximum range to search for targets. Players outside this physical Manhattan distance will always be ignored.
    //
    // Vanilla always uses infinity for this, leading to behavior like Queen Bee crossing the world to spawncamp you.
    // Calamity defaults to a very high but not infinite value.
    /// <summary>
    /// 设置搜索玩家的曼哈顿距离上限，超出此范围的玩家不会成为候选目标。
    /// 数值以像素计；默认值为 9600（600 格）。
    /// </summary>
    public float MaxSearchRange = 9600f; // 600 tiles
    // Targeting preference enum.
    // Anyone = Target the "closest" player, no other considerations. Vanilla behavior.
    // PreferSame = Always pick the same player if they're within the search range, even if another player is closer or has more aggro.
    // ForceSwitch = Try to pick any other player but the current player, if possible. Similar to an "aggro drop" in MMOs.
    //
    // ForceSwitch intentionally does nothing in single player, because there is nobody to switch to.
    /// <summary>
    /// 选择默认最近目标、优先保留旧目标或在多人模式下强制切换目标的策略。
    /// 具体语义由 <see cref="NPCTargetPreference"/> 定义。
    /// </summary>
    public NPCTargetPreference TargetType = NPCTargetPreference.Anyone;
    // The ratio at which to consider aggro bonuses from player gear.
    // 1f is vanilla. Set to 0f to ignore aggro bonuses entirely.
    // Set to a negative value to make the NPC intentionally avoid tanks and preferentially go after other players.
    /// <summary>
    /// 设置玩家装备仇恨对目标距离评分的缩放比例；1 为原版行为，0 忽略装备仇恨，负值会反向偏好低仇恨玩家。
    /// 同一比例也用于计算星尘守护者提供的仇恨修正。
    /// </summary>
    public float AggroRatio = 1f;
    // Whether or not players must have line of sight to the NPC to be considered valid targets.
    // This is always line of sight to the NPC itself, even if a different targeting center for range finding is specified.
    /// <summary>
    /// 指定候选玩家是否必须与 NPC 中心保持视线可达才可被选中。
    /// 开启后建议同时限制 <see cref="MaxSearchRange"/>，以避免大范围视线检测开销。
    /// </summary>
    public bool RequireLineOfSight = false;
    // If true, the targeting algorithm counts missing health as a gigantic boost to aggro.
    // This makes for a "merciless" or "bloodthirsty" NPC which is focused on killing the lowest health players.
    /// <summary>
    /// 指定是否根据玩家已损失生命值增加虚拟仇恨，使 NPC 更倾向攻击低生命值玩家。
    /// 加成从零线性增长，并在内部上限处封顶。
    /// </summary>
    public bool FinishThemOff = false;
    /// <summary>
    /// “优先攻击低生命值玩家”模式可施加的最大虚拟仇恨加成，单位与距离评分一致。
    /// </summary>
    internal const float FinishThemOffMaxAggroBoost = 4000f;
    // If true, this NPC ignores the Stardust armor set bonus "JoJo Tank Minion" (or "Algalon the Observer" according to the wiki).
    //
    // This is set to false by default, because that's vanilla behavior.
    // As Stardust armor is postgame in vanilla, no vanilla bosses ignore the Stardust Guardian.
    // It is highly recommended to set this to true for all bosses, or their aggro can be abusively manipulated.
    /// <summary>
    /// 指定是否忽略星尘守护者的诱饵仇恨，避免 NPC 转而锁定守护者弹幕。
    /// 默认为 <see langword="false"/>，以保持原版行为；Boss 通常应设为 <see langword="true"/>。
    /// </summary>
    public bool IgnoreTankMinions = false;
    // If true, this NPC ignores players who have less than zero net aggro and are not actively using items.
    //
    // This is set to true by default, because it's (undocumented) vanilla behavior.
    // Bosses will automatically attack stealthed players anyway -- you don't need to set this to false for that to occur.
    /// <summary>
    /// 指定普通 NPC 是否沿用原版规则忽略负仇恨且未使用物品的玩家。
    /// 此选项不会令 Boss 忽略潜行玩家，默认开启以匹配原版行为。
    /// </summary>
    public bool IgnoreStealthedPlayers = true;
    // If true, this targeting change forces a net update.
    // In vanilla, targeting updates cause net updates if direction changed or the target player changed,
    // but NEVER if the NPC has collideX or collideY set to true.
    //
    // Generally this doesn't need to be set to true, as bosses will never have collideX or collideY set to true.
    /// <summary>
    /// 指定本次目标选择是否无条件请求 NPC 网络同步。
    /// 默认仅在目标或朝向变化且 NPC 未发生水平、垂直碰撞时同步；通常无需强制开启。
    /// </summary>
    public bool ForceNetUpdate = false;
    // Player indexes put into this list will not be considered for targetting.
    /// <summary>
    /// 存放不参与目标候选的玩家索引集合。
    /// 排除集合只在多人模式下应用；集合内容由调用方维护。
    /// 结构体复制仍共享此集合引用，需要独立修改时应复制集合；作为哈希键期间不得修改集合内容。
    /// </summary>
    public HashSet<int> ExcludedPlayers = [];
    /// <summary>
    /// 创建包含字段初始值的目标选项。
    /// 此构造函数适用于需要调整个别选项的场景，其他字段保留其声明的默认值。
    /// </summary>
    public NPCTargetingParameters()
    {
    }

    /// <summary>
    /// 创建目标选项，并按 NPC 是否为 Boss 配置星尘守护者忽略规则。
    /// 传入 <paramref name="isBoss"/> 时会同步设置 <see cref="IgnoreTankMinions"/>；其他选项保留字段默认值。
    /// </summary>
    public NPCTargetingParameters(bool isBoss) => IgnoreTankMinions = isBoss;
    // Using the default keyword on structs produces garbage. Please use the below instead, or define your own parameters.
    /// <summary>
    /// 提供含字段初始值的通用目标选择选项。
    /// 通过此属性创建实例可避免结构体 <see langword="default"/> 值清零字段初始值。
    /// </summary>
    public static NPCTargetingParameters Defaults => new();
    /// <summary>
    /// 提供适用于 Boss 的目标选择选项，默认忽略星尘守护者诱饵。
    /// 其余筛选、仇恨及网络行为与 <see cref="Defaults"/> 相同。
    /// </summary>
    public static NPCTargetingParameters BossDefaults => new(true);

    #region Equality Operators
    /// <summary>
    /// 比较寻敌中心、全部标量和被排除玩家索引集合；集合按整数内容比较，忽略插入顺序，null 与空集合等价。
    /// </summary>
    public static bool operator ==(NPCTargetingParameters ctp1, NPCTargetingParameters ctp2)
    {
        bool targetingCentersEqual = ctp1.TargetingCenter is { } first
            ? ctp2.TargetingCenter is { } second && first.X.Equals(second.X) && first.Y.Equals(second.Y)
            : ctp2.TargetingCenter is null;
        return targetingCentersEqual
            && ctp1.FaceTarget == ctp2.FaceTarget
            && ctp1.MaxSearchRange.Equals(ctp2.MaxSearchRange)
            && ctp1.TargetType == ctp2.TargetType
            && ctp1.AggroRatio.Equals(ctp2.AggroRatio)
            && ctp1.RequireLineOfSight == ctp2.RequireLineOfSight
            && ctp1.FinishThemOff == ctp2.FinishThemOff
            && ctp1.IgnoreTankMinions == ctp2.IgnoreTankMinions
            && ctp1.IgnoreStealthedPlayers == ctp2.IgnoreStealthedPlayers
            && ctp1.ForceNetUpdate == ctp2.ForceNetUpdate
            && (ctp1.ExcludedPlayers is null
                ? ctp2.ExcludedPlayers is null || ctp2.ExcludedPlayers.Count == 0
                : ctp2.ExcludedPlayers is null ? ctp1.ExcludedPlayers.Count == 0
                : ctp1.ExcludedPlayers.Count == ctp2.ExcludedPlayers.Count
                    && ctp1.ExcludedPlayers.All(player => ctp2.ExcludedPlayers.Contains(player, EqualityComparer<int>.Default)));
    }

    /// <summary>
    /// 返回相等运算符结果的逻辑取反。
    /// </summary>
    public static bool operator !=(NPCTargetingParameters ctp1, NPCTargetingParameters ctp2) => !(ctp1 == ctp2);
    /// <summary>
    /// 按全部选项及排除集合内容比较；浮点 NaN 与自身相等。
    /// </summary>
    /// <remarks>排除集合按整数内容比较，与插入顺序无关；null 与空集合等价。相等实例的哈希相同。</remarks>
    public readonly bool Equals(NPCTargetingParameters other) => this == other;
    /// <inheritdoc/>
    public override readonly bool Equals([NotNullWhen(true)] object obj)
    {
        if (obj is not NPCTargetingParameters)
            return false;
        return this == (NPCTargetingParameters)obj;
    }

    /// <inheritdoc/>
    /// <remarks>
    /// 对所有参与相等比较的选项计算哈希；排除集合顺序不影响结果。作为哈希键期间不得修改字段或集合。
    /// </remarks>
    public override readonly int GetHashCode()
    {
        HashCode hash = new();
        hash.Add(TargetingCenter);
        hash.Add(FaceTarget);
        hash.Add(MaxSearchRange);
        hash.Add(TargetType);
        hash.Add(AggroRatio);
        hash.Add(RequireLineOfSight);
        hash.Add(FinishThemOff);
        hash.Add(IgnoreTankMinions);
        hash.Add(IgnoreStealthedPlayers);
        hash.Add(ForceNetUpdate);
        int excludedHash = 0;
        if (ExcludedPlayers is not null)
        {
            foreach (int player in ExcludedPlayers)
                excludedHash ^= player.GetHashCode();
        }
        hash.Add(ExcludedPlayers?.Count ?? 0);
        hash.Add(excludedHash);
        return hash.ToHashCode();
    }
    #endregion
}

/// <summary>
/// 提供 NPC 寻敌、战斗运动及伤害计算工具。
/// </summary>
public static class TOCombatUtils
{
    /// <summary>
    /// 按 <paramref name="options"/> 选择 NPC 当前目标，并更新目标碰撞框、朝向及必要的网络同步标记。
    /// 保留原版目标筛选语义，同时应用范围、视线、仇恨、潜行和诱饵选项；返回最终目标玩家索引。
    /// </summary>
    public static int TargetWithOptions(this NPC npc, NPCTargetingParameters options)
    {
        // 05JUN2024: Ozzatron: Struct defaults are always memset to all-zero, giving garbage parameters.
        // If you actually call this function with the default keyword for the struct,
        // change the options on the spot to valid default / intended default parameters.
        if (options == default)
            options = new NPCTargetingParameters();
        float distance = 0f;
        // float realDist = 0f; // Defined but not used by vanilla. Commented out here.
        bool anyTargetAvailable = false;
        int tankMinionProjectileID = -1;
        // The setup and initial loop is equivalent to vanilla NPC.TargetClosest, but optimized.
        foreach (Player p in Main.ActivePlayers)
        {
            bool playerDead = p.dead || p.ghost;
            if (playerDead)
                continue;
            // ForceSwitch targeting. If the same player from last time is iterated over, just ignore them.
            // Player exclusion. If the player is to be excluded, do not consider them.
            bool sameTargetAsLastTime = p.whoAmI == npc.oldTarget;
            bool notSinglePlayer = Main.netMode != NetmodeID.SinglePlayer;
            if (notSinglePlayer && (options.ExcludedPlayers?.Contains(p.whoAmI) == true || (options.TargetType == NPCTargetPreference.ForceSwitch && sameTargetAsLastTime)))
                continue;
            //
            // The below code is implemented in vanilla as a separate method. Here, it's inlined for efficiency.
            //
            Vector2 pCenter = p.Center;
            Vector2 targetCenter = options.TargetingCenter ?? npc.Center;
            float manhattanDist = Math.Abs(targetCenter.X - pCenter.X) + Math.Abs(targetCenter.Y - pCenter.Y);
            // Hard cutoff range specified in options. If the player is further, completely ignore them.
            if (manhattanDist > options.MaxSearchRange)
                continue;
            // Line of sight requirement specified in options. Please don't use this without reducing the max search range.
            if (options.RequireLineOfSight && !Collision.CanHit(npc.Center, 1, 1, pCenter, 1, 1))
                continue;
            float aggroAdjustedDist = manhattanDist - options.AggroRatio * p.aggro;
            // Implementation of "Finish Them Off": Add enormous amounts of virtual aggro to low health players
            if (options.FinishThemOff)
            {
                float missingHPRatio = MathHelper.Clamp(1f - p.statLife / (float)p.statLifeMax2, 0f, 1f);
                float bloodthirstAggro = MathHelper.Lerp(0f, NPCTargetingParameters.FinishThemOffMaxAggroBoost, missingHPRatio);
                aggroAdjustedDist -= bloodthirstAggro;
            }

            bool aggroDisabled = p.npcTypeNoAggro[npc.type];
            if (aggroDisabled && npc.direction != 0)
                aggroAdjustedDist += 1000f;
            bool cancelTargeting = false;
            // PreferSame targeting. If the same player from last time is a valid target, even if not the "best" target, pick it anyway.
            bool preferSameFound = options.TargetType == NPCTargetPreference.PreferSame && sameTargetAsLastTime;
            // Standard targeting. If the adjusted distance is lower, or this is the first valid target, actually choose the new target.
            bool standardTargetingRequirementsMet = !anyTargetAvailable || aggroAdjustedDist < distance;
            // If either targeting method succeeded, then this target is being engaged.
            bool engageThisTarget = preferSameFound || standardTargetingRequirementsMet;
            if (engageThisTarget)
            {
                anyTargetAvailable = true;
                tankMinionProjectileID = -1; // Reset any Stardust Guardian aggro because a real player was found.
                distance = aggroAdjustedDist;
                npc.target = p.whoAmI;
                // If PreferSame targeting is active, and the same player was found, cancel further iteration. They are being chosen above all others.
                if (preferSameFound)
                    cancelTargeting = true;
            }

            // "Tank pet" accomodation, AKA the 1.4+ Stardust Guardian
            // Basically, if the player would be targeted, give a chance to instead target their tank minion
            //
            // This behavior is not documented on the vanilla wiki.
            if (p.tankPet >= 0 && !aggroDisabled && !options.IgnoreTankMinions)
            {
                Projectile tankMinion = Main.projectile[p.tankPet];
                Vector2 tmCenter = tankMinion.Center;
                float manhattanDistToTankMinion = Math.Abs(targetCenter.X - tmCenter.X) + Math.Abs(targetCenter.Y - tmCenter.Y);
                // The Stardust Guardian is considered to have a 200 aggro bonus by default.
                // In Calamity this is scaled by the aggro ratio specified in options.
                manhattanDistToTankMinion -= options.AggroRatio * 200f;
                // The Stardust Guardian only attracts the attention of NPCs within a very short distance
                if (manhattanDistToTankMinion < distance && manhattanDistToTankMinion < 200f && Collision.CanHit(npc.Center, 1, 1, tmCenter, 1, 1))
                    tankMinionProjectileID = p.tankPet;
            }

            // If targeting has been short-circuited for any reason, cancel iteration over players.
            if (cancelTargeting)
                break;
        }

        // If the NPC has been aggroed by a Stardust Guardian instead of an actual player, account for that
        if (tankMinionProjectileID >= 0)
        {
            Projectile tankMinion = Main.projectile[tankMinionProjectileID];
            npc.targetRect = tankMinion.Hitbox;
            // Always set direction to a nonzero value. This NPC has been engaged in combat.
            npc.direction = 1;
            if (tankMinion.Center.X < npc.Center.X)
                npc.direction = -1;
            npc.directionY = 1;
            if (tankMinion.Center.Y < npc.Center.Y)
                npc.directionY = -1;
        }
        // Standard player aggro occurs here
        else
        {
            bool shouldFaceTarget = options.FaceTarget;
            // Sanitize targeted player index
            if (npc.target is < 0 or >= Main.maxPlayers)
                npc.target = 0;
            Player targetPlayer = Main.player[npc.target];
            npc.targetRect = targetPlayer.Hitbox;
            // Do not switch facing to look at dead players.
            if (targetPlayer.dead)
                shouldFaceTarget = false;
            // If already engaged in combat, do not switch facing to look at players that ignore your aggro.
            if (targetPlayer.npcTypeNoAggro[npc.type] && npc.direction != 0)
                shouldFaceTarget = false;
            if (shouldFaceTarget)
            {
                bool oldTargetWasValid = npc.oldTarget is >= 0 and < Main.maxPlayers;
                bool targetIsLowAggroNotUsingItem = targetPlayer.itemAnimation == 0 && targetPlayer.aggro < 0;
                bool willIgnoreStealthedPlayers = !npc.boss && options.IgnoreStealthedPlayers;
                // Regular NPCs (not bosses) will voluntarily ignore otherwise-valid player targets with less than zero aggro if they are not actively using an item.
                // This ONLY WORKS if they already have another valid target, aka multiplayer.
                // As such, having net less than zero aggro enables you to remain "stealthed" to regular enemies if you are not doing anything.
                // This is undocumented vanilla behavior.
                bool ignoreStealthedPlayer = willIgnoreStealthedPlayers && oldTargetWasValid && targetIsLowAggroNotUsingItem;
                if (!ignoreStealthedPlayer)
                {
                    // Always set direction to a nonzero value. This NPC has been engaged in combat.
                    npc.direction = 1;
                    if (targetPlayer.Center.X < npc.Center.X)
                        npc.direction = -1;
                    npc.directionY = 1;
                    if (targetPlayer.Center.Y < npc.Center.Y)
                        npc.directionY = -1;
                }
            }
        }

        // Confused enemies always run in the exact wrong direction, horizontally at least.
        if (npc.confused)
            npc.direction *= -1;
        // Apply net updates.
        bool directionChange = npc.direction != npc.oldDirection || npc.directionY != npc.oldDirectionY;
        bool targetChange = npc.target != npc.oldTarget;
        bool shouldNetUpdate = (directionChange || targetChange) && !npc.collideX && !npc.collideY;
        if (shouldNetUpdate || options.ForceNetUpdate)
            npc.netUpdate = true;
        return npc.target;
    }

    /// <summary>
    /// 沿指定旋转方向逐步检测前方实心地形，返回从起点到首次接触瓦片的距离。
    /// 检测会处理半砖和斜坡；<paramref name="step"/> 控制采样间隔，未命中时返回 <paramref name="length"/>。
    /// </summary>
    public static float PreciseDistanceToTileCollisionHit(Vector2 start, float rotation, float length, float step = 1)
    {
        Vector2 unitVect = rotation.ToRotationVector2();
        Vector2 end = unitVect * length;
        if (length < 1f)
        {
            Point endWorldPos = end.ToTileCoordinates();
            return IsSolid(Framing.GetTileSafely(endWorldPos)) ? 0 : length;
        }

        Vector2 currentPos = start;
        Point lastAirPos = new(-1, -1);
        for (float i = 0; i < length; i += step)
        {
            currentPos += unitVect * step;
            Point tilePos = currentPos.ToTileCoordinates();
            if (tilePos == lastAirPos)
                continue;
            if (!WorldGen.InWorld(tilePos.X, tilePos.Y))
                continue;
            Tile tile = Main.tile[tilePos.X, tilePos.Y];
            if (!IsSolid(tile))
            {
                lastAirPos = tilePos;
                continue;
            }

            if (tile.Slope == SlopeType.Solid && !tile.IsHalfBlock)
                return (currentPos - start).Length();
            Vector2 tileWorldPos = new(tilePos.X * 16, tilePos.Y * 16);
            Vector2 currentPosInTile = currentPos - tileWorldPos;
            if (tile.IsHalfBlock)
            {
                if (currentPosInTile.Y >= 8f)
                    return (currentPos - start).Length();
            }
            else if (tile.Slope == SlopeType.SlopeDownLeft)
            {
                if (currentPosInTile.X <= currentPosInTile.Y)
                    return (currentPos - start).Length();
            }
            else if (tile.Slope == SlopeType.SlopeDownRight)
            {
                if ((16 - currentPosInTile.X) <= currentPosInTile.Y)
                    return (currentPos - start).Length();
            }
            else if (tile.Slope == SlopeType.SlopeUpLeft)
            {
                if (currentPosInTile.X <= (16 - currentPosInTile.Y))
                    return (currentPos - start).Length();
            }
            else if (tile.Slope == SlopeType.SlopeUpRight)
            {
                if (currentPosInTile.X >= currentPosInTile.Y)
                    return (currentPos - start).Length();
            }
        }

        return length;
    }

    private static bool IsSolid(Tile tile) => tile.HasUnactuatedTile && Main.tileSolid[tile.TileType] && !Main.tileSolidTop[tile.TileType];
    /// <summary>
    /// 统计当前活动弹幕中类型编号等于 <paramref name="type"/> 的数量。
    /// 只遍历活动弹幕集合，不修改弹幕状态。
    /// </summary>
    public static int CountProjectiles(int type)
    {
        int count = 0;
        foreach (Projectile p in Projectile.ActiveProjectiles)
            if (p.type == type)
                count++;
        return count;
    }

    /// <summary>
    /// 按当前网络模式使用召唤物生成 Boss：服务器或单人模式直接调用原版生成入口，客户端发送召唤请求消息。
    /// 网络分支确保多人客户端不在本地重复生成实体。
    /// </summary>
    public static void SpawnBossUsingItem(Player player, int type)
    {
        if (Main.netMode != NetmodeID.MultiplayerClient)
            NPC.SpawnOnPlayer(player.whoAmI, type);
        else
            NetMessage.SendData(MessageID.SpawnBossUseLicenseStartEvent, number: player.whoAmI, number2: type);
    }
}
