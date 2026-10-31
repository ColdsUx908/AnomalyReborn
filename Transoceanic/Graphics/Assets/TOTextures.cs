using Transoceanic.Graphics.Assets.Loading;
namespace Transoceanic.Graphics.Assets;

/// <summary>
/// 集中访问框架纹理资源，资源由加载器管理。
/// </summary>
public static class TOTextures
{
    /// <summary>
    /// 纹理资源的基础路径。
    /// </summary>
    public const string Path = "Transoceanic/Assets/Textures/Common/";

    /// <summary>
    /// 代表一个空纹理（1×1透明像素）资源的路径。
    /// </summary>
    public const string InvisibleTexturePath = Path + "InvisibleTexture";

    [LoadTexture(Path + "InvisibleTexture")]
    private static Asset<Texture2D> _InvisibleTexture;
    /// <summary>
    /// 1x1透明纹理。
    /// </summary>
    public static Texture2D InvisibleTexture => _InvisibleTexture.Value;

    [LoadTexture(Path + "FillerTexture")]
    private static Asset<Texture2D> _FillerTexture;
    /// <summary>
    /// 1×1纯白纹理。
    /// </summary>
    public static Texture2D FillerTexture => _FillerTexture.Value;

    /// <summary>
    /// 附加的通用圆形和光晕线条纹理。
    /// </summary>
    public static class Extra
    {
        /// <summary>
        /// 资源加载时使用的模组资源路径。
        /// </summary>
        public const string Path = TOTextures.Path + "Extra/";

        /// <summary>
        /// 基础实心圆纹理，使用前须完成资源加载。
        /// </summary>
        [LoadTexture(Path + "BasicCircle")]
        internal static Asset<Texture2D> _BasicCircle;
        /// <summary>
        /// 基础实心圆纹理，使用前须完成资源加载。
        /// </summary>
        public static Texture2D BasicCircle => _BasicCircle?.Value;

        /// <summary>
        /// 较粗的光晕线条纹理。
        /// </summary>
        [LoadTexture(Path + "BloomLineThick")]
        internal static Asset<Texture2D> _BloomLineThick;
        /// <summary>
        /// 较粗的光晕线条纹理。
        /// </summary>
        public static Texture2D BloomLineThick => _BloomLineThick?.Value;
    }
}
