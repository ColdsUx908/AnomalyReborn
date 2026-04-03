namespace Transoceanic.Framework.Helpers;

public static partial class TOExtensions
{
    extension(ModContent)
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T GetModNPC<T>() where T : ModNPC => (T)ModContent.GetModNPC(ModContent.NPCType<T>());

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T GetModItem<T>() where T : ModItem => (T)ModContent.GetModItem(ModContent.ItemType<T>());

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T GetModDust<T>() where T : ModDust => (T)ModContent.GetModDust(ModContent.DustType<T>());

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T GetModProjectile<T>() where T : ModProjectile => (T)ModContent.GetModProjectile(ModContent.ProjectileType<T>());

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T GetModBuff<T>() where T : ModBuff => (T)ModContent.GetModBuff(ModContent.BuffType<T>());

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T GetModMount<T>() where T : ModMount => (T)ModContent.GetModMount(ModContent.MountType<T>());

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T GetModTile<T>() where T : ModTile => (T)ModContent.GetModTile(ModContent.TileType<T>());

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T GetModWall<T>() where T : ModWall => (T)ModContent.GetModWall(ModContent.WallType<T>());
    }
}