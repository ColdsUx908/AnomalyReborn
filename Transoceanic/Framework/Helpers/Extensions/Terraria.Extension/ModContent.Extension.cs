namespace Transoceanic.Framework.Helpers;

public static partial class TOExtensions
{
    extension(ModContent)
    {
        /// <summary>
        /// 获取指定类型的 <see cref="ModNPC"/> 实例。
        /// </summary>
        /// <typeparam name="T">继承自 <see cref="ModNPC"/> 的类型。</typeparam>
        /// <returns>对应的 <see cref="ModNPC"/> 实例。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T GetModNPC<T>() where T : ModNPC => (T)ModContent.GetModNPC(ModContent.NPCType<T>());

        /// <summary>
        /// 获取指定类型的 <see cref="ModItem"/> 实例。
        /// </summary>
        /// <typeparam name="T">继承自 <see cref="ModItem"/> 的类型。</typeparam>
        /// <returns>对应的 <see cref="ModItem"/> 实例。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T GetModItem<T>() where T : ModItem => (T)ModContent.GetModItem(ModContent.ItemType<T>());

        /// <summary>
        /// 获取指定类型的 <see cref="ModDust"/> 实例。
        /// </summary>
        /// <typeparam name="T">继承自 <see cref="ModDust"/> 的类型。</typeparam>
        /// <returns>对应的 <see cref="ModDust"/> 实例。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T GetModDust<T>() where T : ModDust => (T)ModContent.GetModDust(ModContent.DustType<T>());

        /// <summary>
        /// 获取指定类型的 <see cref="ModProjectile"/> 实例。
        /// </summary>
        /// <typeparam name="T">继承自 <see cref="ModProjectile"/> 的类型。</typeparam>
        /// <returns>对应的 <see cref="ModProjectile"/> 实例。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T GetModProjectile<T>() where T : ModProjectile => (T)ModContent.GetModProjectile(ModContent.ProjectileType<T>());

        /// <summary>
        /// 获取指定类型的 <see cref="ModBuff"/> 实例。
        /// </summary>
        /// <typeparam name="T">继承自 <see cref="ModBuff"/> 的类型。</typeparam>
        /// <returns>对应的 <see cref="ModBuff"/> 实例。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T GetModBuff<T>() where T : ModBuff => (T)ModContent.GetModBuff(ModContent.BuffType<T>());

        /// <summary>
        /// 获取指定类型的 <see cref="ModMount"/> 实例。
        /// </summary>
        /// <typeparam name="T">继承自 <see cref="ModMount"/> 的类型。</typeparam>
        /// <returns>对应的 <see cref="ModMount"/> 实例。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T GetModMount<T>() where T : ModMount => (T)ModContent.GetModMount(ModContent.MountType<T>());

        /// <summary>
        /// 获取指定类型的 <see cref="ModTile"/> 实例。
        /// </summary>
        /// <typeparam name="T">继承自 <see cref="ModTile"/> 的类型。</typeparam>
        /// <returns>对应的 <see cref="ModTile"/> 实例。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T GetModTile<T>() where T : ModTile => (T)ModContent.GetModTile(ModContent.TileType<T>());

        /// <summary>
        /// 获取指定类型的 <see cref="ModWall"/> 实例。
        /// </summary>
        /// <typeparam name="T">继承自 <see cref="ModWall"/> 的类型。</typeparam>
        /// <returns>对应的 <see cref="ModWall"/> 实例。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T GetModWall<T>() where T : ModWall => (T)ModContent.GetModWall(ModContent.WallType<T>());
    }
}