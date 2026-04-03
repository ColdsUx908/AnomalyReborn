using Transoceanic.DataStructures;

namespace Transoceanic.Framework.Helpers;

public static partial class TOExtensions
{
    extension(Item item)
    {
        public TOGlobalItem Ocean => item?.GetGlobalItem<TOGlobalItem>();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T GetModItem<T>() where T : ModItem => item?.ModItem as T;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T GetModItemThrow<T>() where T : ModItem => item.GetModItem<T>() ?? throw new ArgumentException($"Item {item.Name} ({item.type}) does not have a ModItem of type {typeof(T).FullName}.", nameof(item));
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetModItem<T>([NotNullWhen(true)] out T result) where T : ModItem => (result = item.GetModItem<T>()) is not null;

        public Texture2D Texture => TOAssetUtils.GetItemTexture(item.type);

        public void DrawInventoryWithBorder(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Vector2 origin, float scale,
            int way, float borderWidth, Color borderColor) =>
            TODrawUtils.DrawBorderTexture(spriteBatch, TextureAssets.Item[item.type].Value, position, frame, borderColor, 0f, origin, scale, way: way, borderWidth: borderWidth);

        #region GlobalItem
        /// <summary>
        /// 获取一个物品的装备时长。
        /// </summary>
        /// <param name="max"></param>
        /// <returns>装备时长。
        /// <br/>在物品装备时，返回值从0逐渐增加至 <paramref name="max"/>；未装备时，从 <paramref name="max"/> 逐渐减少至 <c>0</c>。
        /// </returns>
        public int GetEquippedTimer(int max) => item.Ocean.Equip_Timer.GetValue(TOSharedData.GameTimer.TotalTicks, max);
        #endregion GlobalItem
    }

    extension(Item)
    {
        public static TOIterator<Item> ActiveItems => TOIteratorFactory.NewItemIterator(IteratorMatches.Item_IsActive);

        public static Item CreateItem(int type)
        {
            Item item = new();
            item.SetDefaults(type);
            return item;
        }

        public static Item CreateItem<T>() where T : ModItem => CreateItem(ModContent.ItemType<T>());

        public static Item CreateItem(int type, Action<Item> action)
        {
            Item item = CreateItem(type);
            action?.Invoke(item);
            return item;
        }

        public static Item CreateItem<T>(Action<Item> action) where T : ModItem
        {
            Item item = CreateItem<T>();
            action?.Invoke(item);
            return item;
        }
    }
}