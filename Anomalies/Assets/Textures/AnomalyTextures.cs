// Developed by ColdsUx

namespace Anomalies.Assets;

public sealed class AnomalyTextures
{
    public const string TexturePathPrefix = "Anomalies/Assets/Textures/";

    [LoadTexture(TexturePathPrefix + "Touhou/Ice1")]
    internal static Asset<Texture2D> _Ice1;
    public static Texture2D Ice1 => _Ice1?.Value;

    [LoadTexture(TexturePathPrefix + "Touhou/Ice2")]
    internal static Asset<Texture2D> _Ice2;
    public static Texture2D Ice2 => _Ice2?.Value;

    [LoadTexture(TexturePathPrefix + "Touhou/Ice3")]
    internal static Asset<Texture2D> _Ice3;
    public static Texture2D Ice3 => _Ice3?.Value;

    internal static Asset<Texture2D> _Ice4;
    public static Texture2D Ice4 => _Ice4?.Value;

    internal static Asset<Texture2D> _Ice5;
    public static Texture2D Ice5 => _Ice5?.Value;

    [LoadTexture(TexturePathPrefix + "Touhou/Ice6")]
    internal static Asset<Texture2D> _Ice6;
    public static Texture2D Ice6 => _Ice6?.Value;

    [LoadTexture(TexturePathPrefix + "Touhou/Ice7")]
    internal static Asset<Texture2D> _Ice7;
    public static Texture2D Ice7 => _Ice7?.Value;

    [LoadTexture(TexturePathPrefix + "Touhou/Ice8")]
    internal static Asset<Texture2D> _Ice8;
    public static Texture2D Ice8 => _Ice8?.Value;

    [LoadTexture(TexturePathPrefix + "Touhou/Scale1")]
    internal static Asset<Texture2D> _Scale1;
    public static Texture2D Scale1 => _Scale1?.Value;

    [LoadTexture(TexturePathPrefix + "Touhou/Scale2")]
    internal static Asset<Texture2D> _Scale2;
    public static Texture2D Scale2 => _Scale2?.Value;

    [LoadTexture(TexturePathPrefix + "Extra/BasicCircle")]
    internal static Asset<Texture2D> _BasicCircle;
    public static Texture2D BasicCircle => _BasicCircle?.Value;
}
