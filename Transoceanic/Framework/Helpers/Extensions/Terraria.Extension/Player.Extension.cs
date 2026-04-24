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

        /// <inheritdoc cref="Player.AddBuff(int, int, bool, bool)"/>
        /// <summary>
        /// 为玩家添加一个 ModBuff。
        /// </summary>
        /// <typeparam name="T">继承自 <see cref="ModBuff"/> 的增益类型。</typeparam>
        /// <param name="time">持续时间（帧数）。</param>
        /// <param name="quiet">是否安静添加（不发出声音或特效）。</param>
        /// <param name="foodHack">是否为食物类增益。</param>
        public void AddBuff<T>(int time, bool quiet = false, bool foodHack = false) where T : ModBuff => player.AddBuff(ModContent.BuffType<T>(), time, quiet, foodHack);
    }

    extension(Player)
    {
        /// <summary>
        /// 获取服务器端虚拟玩家（索引为 <see cref="Main.maxPlayers"/>）。
        /// </summary>
        public static Player Server => Main.player[Main.maxPlayers];

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
    }
}