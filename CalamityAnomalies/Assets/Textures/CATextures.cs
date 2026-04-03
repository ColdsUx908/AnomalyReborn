namespace CalamityAnomalies.Assets;

public sealed class CATextures
{
    public const string TexturePathPrefix = "CalamityAnomalies/Assets/Textures/";

    [LoadTexture(TexturePathPrefix + "Touhou/Ice1")]
    internal static Asset<Texture2D> _ice1;
    public static Texture2D Ice1 => _ice1?.Value;

    [LoadTexture(TexturePathPrefix + "Touhou/Ice2")]
    internal static Asset<Texture2D> _ice2;
    public static Texture2D Ice2 => _ice2?.Value;

    [LoadTexture(TexturePathPrefix + "Touhou/Ice3")]
    internal static Asset<Texture2D> _ice3;
    public static Texture2D Ice3 => _ice3?.Value;

    internal static Asset<Texture2D> _ice4;
    public static Texture2D Ice4 => _ice4?.Value;

    internal static Asset<Texture2D> _ice5;
    public static Texture2D Ice5 => _ice5?.Value;

    [LoadTexture(TexturePathPrefix + "Touhou/Ice6")]
    internal static Asset<Texture2D> _ice6;
    public static Texture2D Ice6 => _ice6?.Value;

    [LoadTexture(TexturePathPrefix + "Touhou/Ice7")]
    internal static Asset<Texture2D> _ice7;
    public static Texture2D Ice7 => _ice7?.Value;

    [LoadTexture(TexturePathPrefix + "Touhou/Ice8")]
    internal static Asset<Texture2D> _ice8;
    public static Texture2D Ice8 => _ice8?.Value;

    [LoadTexture(TexturePathPrefix + "Touhou/Scale1")]
    internal static Asset<Texture2D> _scale1;
    public static Texture2D Scale1 => _scale1?.Value;

    [LoadTexture(TexturePathPrefix + "Touhou/Scale2")]
    internal static Asset<Texture2D> _scale2;
    public static Texture2D Scale2 => _scale2?.Value;

    [LoadTexture(TexturePathPrefix + "UI/AnomalyModeIndicator")]
    internal static Asset<Texture2D> _anomalyModeIndicator;
    public static Texture2D AnomalyModeIndicator => _anomalyModeIndicator?.Value;

    [LoadTexture(TexturePathPrefix + "UI/AnomalyModeIndicator_Off")]
    internal static Asset<Texture2D> _anomalyModeIndicator_Off;
    public static Texture2D AnomalyModeIndicator_Off => _anomalyModeIndicator_Off?.Value;

    [LoadTexture(TexturePathPrefix + "UI/AnomalyModeIndicator_Border")]
    internal static Asset<Texture2D> _anomalyModeIndicator_Border;
    public static Texture2D AnomalyModeIndicator_Border => _anomalyModeIndicator_Border?.Value;
}
