namespace Transoceanic.Framework.Helpers;

/// <summary>
/// 提供与物块（<see cref="Tile"/>）相关的工具方法，如安全获取、边界遍历、类型判断等。
/// </summary>
public static class TOTileUtils
{
    /// <summary>
    /// 尝试安全地获取指定坐标处的 <see cref="Tile"/> 实例，并进行世界边界检查。
    /// </summary>
    /// <param name="i">物块横坐标（图格坐标）。</param>
    /// <param name="j">物块纵坐标（图格坐标）。</param>
    /// <param name="fluff">边界容错值。坐标必须在距离世界边缘不小于该值的位置才被视为有效。</param>
    /// <param name="tile">输出参数，若坐标有效则返回对应的 <see cref="Tile"/> 实例，否则为 <see langword="default"/>。</param>
    /// <returns>若坐标在世界范围内，返回 <see langword="true"/>；否则返回 <see langword="false"/>。</returns>
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

    /// <summary>
    /// 判断给定的物块类型是否为树木（包括各种树苗、成年树、棕榈树等）。
    /// </summary>
    /// <param name="tileType">物块类型 ID。</param>
    /// <returns>若为树木类型，返回 <see langword="true"/>；否则返回 <see langword="false"/>。</returns>
    public static bool IsTree(int tileType) => tileType is 5 or 72 or 323 or 571 or (>= 583 and <= 589) or 596 or 616 or 634;

    /// <summary>
    /// 遍历指定矩形区域的边界物块（仅包含最外圈的一层图格）。
    /// </summary>
    /// <param name="minX">矩形区域的最小横坐标（包含）。</param>
    /// <param name="maxX">矩形区域的最大横坐标（包含）。</param>
    /// <param name="minY">矩形区域的最小纵坐标（包含）。</param>
    /// <param name="maxY">矩形区域的最大纵坐标（包含）。</param>
    /// <param name="fluff">世界边界检查容错值。</param>
    /// <returns>一个包含边界物块及其坐标的枚举序列。</returns>
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

    /// <summary>
    /// 遍历由两个对角点确定的矩形区域的边界物块（仅包含最外圈的一层图格）。
    /// </summary>
    /// <param name="corner1">矩形区域的一个角点坐标。</param>
    /// <param name="corner2">矩形区域的另一个对角点坐标。</param>
    /// <param name="fluff">世界边界检查容错值。</param>
    /// <returns>一个包含边界物块及其坐标的枚举序列。</returns>
    public static IEnumerable<(Tile tile, int i, int j)> GetBorderTiles(Point corner1, Point corner2, int fluff = 0) =>
        GetBorderTiles(Math.Min(corner1.X, corner2.X), Math.Max(corner1.X, corner2.X), Math.Min(corner1.Y, corner2.Y), Math.Max(corner1.Y, corner2.Y), fluff);
}