namespace Transoceanic.DataStructures.Assets;

public static class TOTextures
{
    /// <summary>
    /// 纹理资源的基础路径。
    /// </summary>
    public const string Path = "Transoceanic/DataStructures/Assets/Textures/";

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

    public static class Extra
    {
        public const string Path = TOTextures.Path + "Extra/";

        [LoadTexture(Path + "BasicCircle")]
        internal static Asset<Texture2D> _BasicCircle;
        public static Texture2D BasicCircle => _BasicCircle?.Value;

        [LoadTexture(Path + "BloomLineThick")]
        internal static Asset<Texture2D> _BloomLineThick;
        public static Texture2D BloomLineThick => _BloomLineThick?.Value;
    }
}
