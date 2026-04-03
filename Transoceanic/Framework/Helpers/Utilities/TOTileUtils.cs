namespace Transoceanic.Framework.Helpers;

public static class TOTileUtils
{
    /// <summary>
    /// 尝试获取指定坐标处的 <see cref="Tile"/> 实例。
    /// </summary>
    /// <param name="i">横坐标。</param>
    /// <param name="j">纵坐标。</param>
    /// <param name="fluff">判定坐标是否在世界范围内时的容错值。坐标被视为在世界范围内当且仅当其横、纵坐标距世界边缘的距离均不小于该值。</param>
    /// <param name="tile">输出的 <see cref="Tile"/> 实例。</param>
    /// <returns>如果坐标不在世界范围内，返回 <see langword="false"/>，并将输出参数设置为默认值；否则返回 <see langword="true"/>，并将输出参数设置为对应坐标处的 <see cref="Tile"/> 实例。</returns>
    public static bool TryGetTile(int i, int j, int fluff, out Tile tile)
    {
        if (WorldGen.InWorld(i, j, fluff))
        {
            tile = Main.tile[i, j];
            return true;
        }
        else
        {
            tile = default;
            return false;
        }
    }

    public static bool IsTree(int tileType) => tileType is 5 or 72 or 323 or 571 or (>= 583 and <= 589) or 596 or 616 or 634;

    public static IEnumerable<(Tile tile, int i, int j)> GetBorderTiles(int minX, int maxX, int minY, int maxY, int fluff = 0)
    {
        switch (minX == maxX, minY == maxY)
        {
            case (true, true):
                if (TryGetTile(minX, minY, fluff, out Tile tile1))
                    yield return (tile1, minX, minY);
                break;
            case (true, false):
                for (int j = minY; j <= maxY; j++)
                {
                    if (TryGetTile(minX, j, fluff, out Tile tile2))
                        yield return (tile2, minX, j);
                }
                break;
            case (false, true):
                for (int i = minX; i <= maxX; i++)
                {
                    if (TryGetTile(i, minY, fluff, out Tile tile3))
                        yield return (tile3, i, minY);
                }
                break;
            case (false, false):
                for (int i = minX; i <= maxX; i++)
                {
                    if (TryGetTile(i, minY, fluff, out Tile tileUp))
                        yield return (tileUp, i, minY);
                    if (TryGetTile(i, maxY, fluff, out Tile tileDown))
                        yield return (tileDown, i, maxY);
                }
                for (int j = minY + 1; j < maxY; j++)
                {
                    if (TryGetTile(minX, j, fluff, out Tile tileLeft))
                        yield return (tileLeft, minX, j);
                    if (TryGetTile(maxX, j, fluff, out Tile tileRight))
                        yield return (tileRight, maxX, j);
                }
                break;
        }
    }

    public static IEnumerable<(Tile tile, int i, int j)> GetBorderTiles(Point corner1, Point corner2, int fluff = 0) =>
        GetBorderTiles(Math.Min(corner1.X, corner2.X), Math.Max(corner1.X, corner2.X), Math.Min(corner1.Y, corner2.Y), Math.Max(corner1.Y, corner2.Y), fluff);
}
