namespace Transoceanic.Framework.Helpers;

public static partial class TOExtensions
{
    extension(Item item)
    {
        /// <summary>
        /// 获取物品的全局数据 <see cref="TOGlobalItem"/>。
        /// </summary>
        public TOGlobalItem Ocean => item?.GetGlobalItem<TOGlobalItem>();

        /// <summary>
        /// 获取物品所关联的 <see cref="ModItem"/> 实例，并转换为指定类型。
        /// </summary>
        /// <typeparam name="T">目标 <see cref="ModItem"/> 类型。</typeparam>
        /// <returns>转换后的实例，若不存在则返回 <c>null</c>。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T GetModItem<T>() where T : ModItem => item?.ModItem as T;

        /// <summary>
        /// 获取物品所关联的 <see cref="ModItem"/> 实例，并转换为指定类型；若不存在则抛出异常。
        /// </summary>
        /// <typeparam name="T">目标 <see cref="ModItem"/> 类型。</typeparam>
        /// <returns>转换后的实例。</returns>
        /// <exception cref="ArgumentException">当物品没有指定类型的 <see cref="ModItem"/> 时抛出。</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T GetModItemThrow<T>() where T : ModItem => item.GetModItem<T>() ?? throw new ArgumentException($"Item {item.Name} ({item.type}) does not have a ModItem of type {typeof(T).FullName}.", nameof(item));

        /// <summary>
        /// 尝试获取物品所关联的 <see cref="ModItem"/> 实例，并转换为指定类型。
        /// </summary>
        /// <typeparam name="T">目标 <see cref="ModItem"/> 类型。</typeparam>
        /// <param name="result">输出转换后的实例，成功时为有效值，否则为 <c>null</c>。</param>
        /// <returns>如果成功获取则返回 <see langword="true"/>，否则返回 <see langword="false"/>。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetModItem<T>([NotNullWhen(true)] out T result) where T : ModItem => (result = item.GetModItem<T>()) is not null;

        /// <summary>
        /// 获取物品的纹理贴图。
        /// </summary>
        public Texture2D Texture => TOAssetUtils.GetItemTexture(item.type);

        /// <summary>
        /// 在物品栏中绘制带有边框的物品图标。
        /// </summary>
        /// <param name="spriteBatch">精灵批处理对象。</param>
        /// <param name="position">绘制位置。</param>
        /// <param name="frame">源矩形区域。</param>
        /// <param name="origin">旋转原点。</param>
        /// <param name="scale">缩放比例。</param>
        /// <param name="way">边框类型。</param>
        /// <param name="borderWidth">边框宽度。</param>
        /// <param name="borderColor">边框颜色。</param>
        public void DrawInventoryWithBorder(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Vector2 origin, float scale,
            int way, float borderWidth, Color borderColor) =>
            TODrawUtils.DrawBorderTexture(spriteBatch, TextureAssets.Item[item.type].Value, position, frame, borderColor, 0f, origin, scale, way: way, borderWidth: borderWidth);

        #region GlobalItem
        /// <summary>
        /// 获取物品的装备时长。
        /// </summary>
        /// <param name="max">时长最大值（完全装备时的值）。</param>
        /// <returns>
        /// 装备时长值。装备时从 0 逐渐增加至 <paramref name="max"/>；未装备时从 <paramref name="max"/> 逐渐减少至 0。
        /// </returns>
        public int GetEquippedTimer(int max) => item.Ocean.Equip_Timer.GetValue(TOSharedData.GameTimer.TotalTicks, max);
        #endregion GlobalItem
    }

    extension(Item)
    {
        /// <summary>
        /// 获取一个迭代器，用于遍历所有激活状态的物品（<see cref="Item.active"/> 为 <see langword="true"/>）。
        /// </summary>
        public static TOIterator<Item> ActiveItems => TOIteratorFactory.NewItemIterator(IteratorMatches.Item_IsActive);

        /// <summary>
        /// 根据物品类型创建新的 <see cref="Item"/> 实例，并设置其默认值。
        /// </summary>
        /// <param name="type">物品类型 ID。</param>
        /// <returns>新创建的物品实例。</returns>
        public static Item CreateItem(int type)
        {
            Item item = new();
            item.SetDefaults(type);
            return item;
        }

        /// <summary>
        /// 根据 <see cref="ModItem"/> 类型创建新的 <see cref="Item"/> 实例。
        /// </summary>
        /// <typeparam name="T">继承自 <see cref="ModItem"/> 的物品类型。</typeparam>
        /// <returns>新创建的物品实例。</returns>
        public static Item CreateItem<T>() where T : ModItem => CreateItem(ModContent.ItemType<T>());

        /// <summary>
        /// 根据物品类型创建新的 <see cref="Item"/> 实例，并在创建后执行一个 <see cref="Action{Item}"/>。
        /// </summary>
        /// <param name="type">物品类型 ID。</param>
        /// <param name="action">对创建后的物品执行的行为。</param>
        /// <returns>新创建的物品实例。</returns>
        public static Item CreateItem(int type, Action<Item> action)
        {
            Item item = CreateItem(type);
            action?.Invoke(item);
            return item;
        }

        /// <summary>
        /// 根据 <see cref="ModItem"/> 类型创建新的 <see cref="Item"/> 实例，并在创建后执行一个 <see cref="Action{Item}"/>。
        /// </summary>
        /// <typeparam name="T">继承自 <see cref="ModItem"/> 的物品类型。</typeparam>
        /// <param name="action">对创建后的物品执行的行为。</param>
        /// <returns>新创建的物品实例。</returns>
        public static Item CreateItem<T>(Action<Item> action) where T : ModItem
        {
            Item item = CreateItem<T>();
            action?.Invoke(item);
            return item;
        }
    }
}