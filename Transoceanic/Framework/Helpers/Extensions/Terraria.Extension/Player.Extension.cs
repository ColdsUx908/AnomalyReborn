using Transoceanic.DataStructures;

namespace Transoceanic.Framework.Helpers;

public static partial class TOExtensions
{
    extension(Player player)
    {
        public TOPlayer Ocean => player?.GetModPlayer<TOPlayer>();

        public bool Alive => player.active && !player.dead && !player.ghost;

        public bool IsPvP => player.Alive && player.hostile;

        public bool IsTeammateOf(Player other) => player.Alive && player.team != 0 && player.team == other.team;

        public TOExclusiveIterator<Player> OtherAlivePlayers => TOIteratorFactory.NewPlayerIterator(p => p.Alive, player);

        public TOExclusiveIterator<Player> Teammates => TOIteratorFactory.NewPlayerIterator(p => p.IsTeammateOf(player), player);

        public TOExclusiveIterator<Player> NonTeammates => TOIteratorFactory.NewPlayerIterator(p => !p.IsTeammateOf(player), player);

        /// <summary>
        /// 获取玩家的手持物品。
        /// </summary>
        /// <returns>若玩家光标持有物品，返回该物品；否则返回玩家物品栏中选中的物品。</returns>
        public Item CurrentItem => Main.mouseItem.IsAir ? player.HeldItem : Main.mouseItem;

        /// <inheritdoc cref="Player.AddBuff(int, int, bool, bool)"/>
        public void AddBuff<T>(int time, bool quiet = false, bool foodHack = false) where T : ModBuff => player.AddBuff(ModContent.BuffType<T>(), time, quiet, foodHack);
    }

    extension(Player)
    {
        public static Player Server => Main.player[Main.maxPlayers];

        public static TOIterator<Player> ActivePlayers => TOIteratorFactory.NewPlayerIterator(IteratorMatches.Player_IsActive);

        public static TOIterator<Player> AlivePlayers => TOIteratorFactory.NewPlayerIterator(IteratorMatches.Player_IsAlive);

        public static TOIterator<Player> PVPPlayers => TOIteratorFactory.NewPlayerIterator(IteratorMatches.Player_IsPVP);

        public static int ActivePlayerCount => Main.netMode == NetmodeID.SinglePlayer ? 1 : Main.CurrentFrameFlags.ActivePlayersCount;
    }
}