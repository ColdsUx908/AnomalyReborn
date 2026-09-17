namespace Transoceanic.Framework.Helpers;

public static partial class TOExtensions
{
    extension(Player player)
    {
        /// <summary>
        /// 获取玩家的全局数据 <see cref="TOPlayer"/>。
        /// </summary>
        public TOPlayer Ocean => player?.GetModPlayer<TOPlayer>();

        /// <summary>
        /// 判断玩家是否处于存活状态（活跃、未死亡、非幽灵）。
        /// </summary>
        public bool Alive => player.active && !player.dead && !player.ghost;

        /// <summary>
        /// 判断玩家是否为 PvP 敌对状态（存活且 hostile 为真）。
        /// </summary>
        public bool IsPvP => player.Alive && player.hostile;

        /// <summary>
        /// 判断玩家是否为另一玩家的队友（双方存活、同队伍且队伍非 0）。
        /// </summary>
        /// <param name="other">另一玩家。</param>
        /// <returns>如果是队友则返回 <see langword="true"/>，否则返回 <see langword="false"/>。</returns>
        public bool IsTeammateOf(Player other) => player.Alive && player.team != 0 && player.team == other.team;

        /// <summary>
        /// 获取一个迭代器，用于遍历除自身以外的其他存活玩家。
        /// </summary>
        public TOExclusiveIterator<Player> OtherAlivePlayers => TOIteratorFactory.NewPlayerIterator(p => p.Alive, player);

        /// <summary>
        /// 获取一个迭代器，用于遍历玩家的队友（包括自身？实际排除了自身）。
        /// </summary>
        public TOExclusiveIterator<Player> Teammates => TOIteratorFactory.NewPlayerIterator(p => p.IsTeammateOf(player), player);

        /// <summary>
        /// 获取一个迭代器，用于遍历非队友的存活玩家。
        /// </summary>
        public TOExclusiveIterator<Player> NonTeammates => TOIteratorFactory.NewPlayerIterator(p => !p.IsTeammateOf(player), player);

        /// <summary>
        /// 获取玩家当前持有的物品（光标物品优先，若为空则使用手持物品）。
        /// </summary>
        /// <returns>当前持有的物品实例。</returns>
        public Item CurrentItem => Main.mouseItem.IsAir ? player.HeldItem : Main.mouseItem;

        /// <summary>
        /// 获取玩家当前可用的召唤物槽位数量。
        /// </summary>
        public float AvailableMinionSlots
        {
            get
            {
                float totalSlots = player.maxMinions;
                float usedSlots = 0f;
                foreach (Projectile proj in Projectile.ActiveProjectiles)
                {
                    if (proj.owner == player.whoAmI && proj.minion)
                        usedSlots += proj.minionSlots;
                }
                return totalSlots - usedSlots;
            }
        }

        /// <summary>
        /// 判断玩家是否可以进行控制操作（移动、跳跃、使用物品等）。
        /// </summary>
        /// <param name="allowWoFTongue">是否忽略玩家被血肉墙控制的情况。</param>
        /// <returns>如果玩家可以进行控制操作，返回 <see langword="true"/>；否则返回 <see langword="false"/>。</returns>
        public bool ControlsEnabled(bool allowWoFTongue = false)
        {
            if (player.CCed) // Covers frozen (player.frozen), webs (player.webbed), and Medusa (player.stoned)
                return false;
            if (player.tongued && !allowWoFTongue)
                return false;
            return true;
        }

        /// <summary>
        /// 判断玩家是否无法使用物品（持有物品或光标物品）或无法使用物品的条件。
        /// </summary>
        /// <param name="right">是否检查右键使用物品的条件。</param>
        /// <param name="needsToHold">是否需要持续按住物品使用键。</param>
        /// <returns>如果玩家无法使用物品，返回 <see langword="true"/>；否则返回 <see langword="false"/>。</returns>
        public bool CanNotUseHoldOut(bool? right = null, bool needsToHold = true)
        {
            bool notHolding = right switch
            {
                true => !Main.mouseRight,
                false => !player.channel,
                null => !Main.mouseRight && !player.channel,
            };
            return !player.Alive || (notHolding && needsToHold) || player.CCed || player.noItems;
        }

        /// <inheritdoc cref="Player.AddBuff(int, int, bool, bool)"/>
        /// <summary>
        /// 为玩家添加一个 ModBuff。
        /// </summary>
        /// <typeparam name="T">继承自 <see cref="ModBuff"/> 的增益类型。</typeparam>
        /// <param name="time">持续时间（帧数）。</param>
        /// <param name="quiet">是否安静添加（不发出声音或特效）。</param>
        /// <param name="foodHack">是否为食物类增益。</param>
        public void AddBuff<T>(int time, bool quiet = false, bool foodHack = false) where T : ModBuff => player.AddBuff(ModContent.BuffType<T>(), time, quiet, foodHack);

        /// <summary>
        /// 使玩家进入无法行动状态。
        /// </summary>
        /// <param name="preventDashing">是否禁止玩家冲刺。</param>
        public void Incapacitate(bool preventDashing = true)
        {
            player.controlLeft = false;
            player.controlRight = false;
            player.controlJump = false;
            player.controlDown = false;
            player.controlUseItem = false;
            player.controlUseTile = false;
            player.controlHook = false;
            player.releaseHook = true;

            if (player.grapCount > 0)
                player.RemoveAllGrapplingHooks();
            if (player.mount.Active)
                player.mount.Dismount(player);
            if (preventDashing)
            {
                for (int i = 0; i < 4; i++)
                {
                    player.doubleTapCardinalTimer[i] = 0;
                    player.holdDownCardinalTimer[i] = 0;
                }
            }
            if (player.dashDelay < 10 && preventDashing)
                player.dashDelay = 10;
        }

        /// <summary>
        /// 设置玩家的屏幕震动强度，若当前强度小于传入值则更新为传入值。
        /// </summary>
        /// <param name="intensity">屏幕震动强度。</param>
        public void SetScreenshake(float intensity) => player.Ocean.CurrentScreenShakePower = Math.Max(player.Ocean.CurrentScreenShakePower, intensity);
    }

    extension(Player)
    {
        /// <summary>
        /// 获取服务器端虚拟玩家（索引为 <see cref="Main.maxPlayers"/>）。
        /// </summary>
        public static Player Server => Main.player[Main.maxPlayers];

        /// <summary>
        /// 根据传入的索引尝试获取 <see cref="Main.player"/> 数组中对应的玩家实例。
        /// </summary>
        /// <param name="index">索引。</param>
        /// <returns>
        /// 弹幕实例。
        /// <br/>若索引越界或等于 <see cref="Main.maxPlayers"/>（对应玩家为服务器端），返回 Server。
        /// <br/>永不返回 <see langword="null"/>。
        /// </returns>
        public static Player GetPlayerFromIndex(int index) => index is >= 0 and < Main.maxPlayers ? Main.player[index] : Player.Server;

        /// <summary>
        /// 尝试根据传入的索引获取 <see cref="Main.player"/> 数组中对应的玩家实例。
        /// </summary>
        /// <param name="index">索引。</param>
        /// <param name="player">输出参数，返回对应的玩家实例。</param>
        /// <returns>如果索引有效且对应玩家存在，返回 <see langword="true"/>；否则返回 <see langword="false"/>。</returns>
        public static bool TryGetPlayerFromIndex(int index, out Player player)
        {
            if (index is >= 0 and < Main.maxPlayers)
            {
                player = Main.player[index];
                return true;
            }
            else
            {
                player = Player.Server;
                return false;
            }
        }

        /// <summary>
        /// 获取一个迭代器，用于遍历所有激活状态的玩家。
        /// </summary>
        public static TOIterator<Player> ActivePlayers => TOIteratorFactory.NewPlayerIterator(IteratorMatches.Player_IsActive);

        /// <summary>
        /// 获取一个迭代器，用于遍历所有存活的玩家。
        /// </summary>
        public static TOIterator<Player> AlivePlayers => TOIteratorFactory.NewPlayerIterator(IteratorMatches.Player_IsAlive);

        /// <summary>
        /// 获取一个迭代器，用于遍历所有 PvP 敌对的玩家。
        /// </summary>
        public static TOIterator<Player> PVPPlayers => TOIteratorFactory.NewPlayerIterator(IteratorMatches.Player_IsPVP);

        /// <summary>
        /// 获取当前活跃玩家数量（单机模式下为 1，联机模式下为 <see cref="Main.CurrentFrameFlags.ActivePlayersCount"/>）。
        /// </summary>
        public static int ActivePlayerCount => Main.netMode == NetmodeID.SinglePlayer ? 1 : Main.CurrentFrameFlags.ActivePlayersCount;

        /// <summary>
        /// 获取当前活跃玩家的游戏时间计时器。
        /// </summary>
        public static TerrariaTimer ActivePlayerTimer => new(Main.ActivePlayerFileData.GetPlayTime());
    }
}
