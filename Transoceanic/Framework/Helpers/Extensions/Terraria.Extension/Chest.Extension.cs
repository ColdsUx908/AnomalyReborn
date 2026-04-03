namespace Transoceanic.Framework.Helpers;

public static partial class TOExtensions
{
    extension(Chest chest)
    {
        /// <summary>
        /// 获取箱子（左下角格子）在世界中的位置。
        /// </summary>
        public Point Position => new(chest.x, chest.y);

        /// <summary>
        /// 获取箱子（左下角格子）在世界中的位置对应的向量。
        /// </summary>
        public Vector2 Coordinate => chest.Position.ToWorldCoordinates();

        public Vector2 Center => chest.Position.ToWorldCoordinates(0f, 16f);

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