// Designed by ColdsUx

namespace Transoceanic.Framework.Helpers;

public static partial class TOExtensions
{
    extension(Tile tile)
    {
        /// <summary>
        /// 设置物块的类型。
        /// </summary>
        /// <param name="type">瓦片类型 ID。</param>
        public void SetTileType(int type) => tile.TileType = (ushort)type;

        /// <summary>
        /// 设置物块的类型为指定的 <see cref="ModTile"/> 类型。
        /// </summary>
        /// <typeparam name="T">继承自 <see cref="ModTile"/> 的类型。</typeparam>
        public void SetTileType<T>() where T : ModTile => tile.SetTileType(ModContent.TileType<T>());
    }
}