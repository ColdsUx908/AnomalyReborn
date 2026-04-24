namespace Transoceanic.Framework.Helpers;

/// <summary>
/// 提供获取原版游戏中各种纹理资源的工具方法，以及格式化对应纹理路径的静态方法。
/// </summary>
public static class TOAssetUtils
{
    /// <summary>
    /// 根据 NPC 类型 ID 格式化对应的原版 NPC 纹理路径。
    /// </summary>
    /// <param name="type">NPC 的类型 ID。</param>
    /// <returns>格式为 "Terraria/Images/NPC_{type}" 的路径字符串。</returns>
    public static string FormatVanillaNPCTexturePath(int type) => $"Terraria/Images/NPC_{type}";

    /// <summary>
    /// 根据射弹类型 ID 格式化对应的原版射弹纹理路径。
    /// </summary>
    /// <param name="type">射弹的类型 ID。</param>
    /// <returns>格式为 "Terraria/Images/Projectile_{type}" 的路径字符串。</returns>
    public static string FormatVanillaProjectileTexturePath(int type) => $"Terraria/Images/Projectile_{type}";

    /// <summary>
    /// 根据物品类型 ID 格式化对应的原版物品纹理路径。
    /// </summary>
    /// <param name="type">物品的类型 ID。</param>
    /// <returns>格式为 "Terraria/Images/Item_{type}" 的路径字符串。</returns>
    public static string FormatVanillaItemTexturePath(int type) => $"Terraria/Images/Item_{type}";

    /// <summary>
    /// 获取指定物品类型的 <see cref="Texture2D"/> 纹理对象。
    /// 该方法会确保对应物品的纹理已加载。
    /// </summary>
    /// <param name="type">物品的类型 ID。</param>
    /// <returns>对应的物品纹理。</returns>
    public static Texture2D GetItemTexture(int type)
    {
        Main.instance.LoadItem(type);
        return TextureAssets.Item[type].Value;
    }

    /// <summary>
    /// 获取指定 NPC 类型的 <see cref="Texture2D"/> 纹理对象。
    /// 该方法会确保对应 NPC 的纹理已加载。
    /// </summary>
    /// <param name="type">NPC 的类型 ID。</param>
    /// <returns>对应的 NPC 纹理。</returns>
    public static Texture2D GetNPCTexture(int type)
    {
        Main.instance.LoadNPC(type);
        return TextureAssets.Npc[type].Value;
    }

    /// <summary>
    /// 获取指定射弹类型的 <see cref="Texture2D"/> 纹理对象。
    /// 该方法会确保对应射弹的纹理已加载。
    /// </summary>
    /// <param name="type">射弹的类型 ID。</param>
    /// <returns>对应的射弹纹理。</returns>
    public static Texture2D GetProjectileTexture(int type)
    {
        Main.instance.LoadProjectile(type);
        return TextureAssets.Projectile[type].Value;
    }

    /// <summary>
    /// 获取指定血污（Gore）类型的 <see cref="Texture2D"/> 纹理对象。
    /// 该方法会确保对应血污的纹理已加载。
    /// </summary>
    /// <param name="type">血污的类型 ID。</param>
    /// <returns>对应的血污纹理。</returns>
    public static Texture2D GetGoreTexture(int type)
    {
        Main.instance.LoadGore(type);
        return TextureAssets.Gore[type].Value;
    }

    /// <summary>
    /// 获取指定墙类型的 <see cref="Texture2D"/> 纹理对象。
    /// 该方法会确保对应墙的纹理已加载。
    /// </summary>
    /// <param name="type">墙的类型 ID。</param>
    /// <returns>对应的墙纹理。</returns>
    public static Texture2D GetWallTexture(int type)
    {
        Main.instance.LoadWall(type);
        return TextureAssets.Wall[type].Value;
    }

    /// <summary>
    /// 获取指定图格类型的 <see cref="Texture2D"/> 纹理对象。
    /// 该方法会确保对应图格的纹理已加载。
    /// </summary>
    /// <param name="type">图格的类型 ID。</param>
    /// <returns>对应的图格纹理。</returns>
    public static Texture2D GetTileTexture(int type)
    {
        Main.instance.LoadTiles(type);
        return TextureAssets.Tile[type].Value;
    }

    /// <summary>
    /// 获取指定物品的火焰效果纹理（通常用于魔法武器等）<see cref="Texture2D"/> 对象。
    /// 该方法会确保对应物品的火焰纹理已加载。
    /// </summary>
    /// <param name="type">物品的类型 ID。</param>
    /// <returns>对应的物品火焰纹理。</returns>
    public static Texture2D GetItemFlameTexture(int type)
    {
        Main.instance.LoadItemFlames(type);
        return TextureAssets.ItemFlame[type].Value;
    }

    /// <summary>
    /// 获取指定背景类型的 <see cref="Texture2D"/> 纹理对象。
    /// 该方法会确保对应背景的纹理已加载。
    /// </summary>
    /// <param name="type">背景的类型 ID。</param>
    /// <returns>对应的背景纹理。</returns>
    public static Texture2D GetBackgroundTexture(int type)
    {
        Main.instance.LoadBackground(type);
        return TextureAssets.Background[type].Value;
    }
}
