namespace Transoceanic.Framework.Helpers;

public static partial class TOExtensions
{
    extension(Chest chest)
    {
        /// <summary>
        /// 获取箱子（左下角格子）在世界中的位置。
        /// </summary>
        /// <returns>表示箱子左下角格子的坐标点。</returns>
        public Point Position => new(chest.x, chest.y);

        /// <summary>
        /// 获取箱子（左下角格子）在世界中的位置对应的向量。
        /// </summary>
        /// <returns>箱子左下角格子的世界坐标向量。</returns>
        public Vector2 Coordinate => chest.Position.ToWorldCoordinates();

        /// <summary>
        /// 获取箱子的中心点在世界中的坐标。
        /// </summary>
        /// <remarks>中心点基于左下角格子位置，偏移 (0, 16) 像素计算得出。</remarks>
        /// <returns>箱子中心点的世界坐标向量。</returns>
        public Vector2 Center => chest.Position.ToWorldCoordinates(0f, 16f);

        /// <summary>
        /// 检查箱子中是否包含指定类型的物品。
        /// </summary>
        /// <param name="itemType">要查找的物品类型 ID。</param>
        /// <param name="index">当找到物品时，输出该物品在箱子物品数组中的索引；否则输出 -1。</param>
        /// <param name="item">当找到物品时，输出对应的 <see cref="Item"/> 实例；否则输出 <c>null</c>。</param>
        /// <returns>如果箱子中存在指定类型的物品，则为 <see langword="true"/>；否则为 <see langword="false"/>。</returns>
        public bool HasItem(int itemType, out int index, [NotNullWhen(true)] out Item item)
        {
            for (int i = 0; i < chest.item.Length; i++)
            {
                Item current = chest.item[i];
                if (current.type == itemType)
                {
                    index = i;
                    item = current;
                    return true;
                }
            }
            index = -1;
            item = null;
            return false;
        }
    }
}