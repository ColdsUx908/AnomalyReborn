namespace Transoceanic.Framework.Helpers;

public static partial class TOExtensions
{
    extension(Tile tile)
    {
        public void SetTileType(int type) => tile.TileType = (ushort)type;

        public void SetTileType<T>() where T : ModTile => tile.SetTileType(ModContent.TileType<T>());
    }
}