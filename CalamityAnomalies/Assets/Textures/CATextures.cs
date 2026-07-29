// Developed by ColdsUx

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

    [LoadTexture(TexturePathPrefix + "UI/AnomalyIndicator")]
    internal static Asset<Texture2D> _anomalyIndicator;
    public static Texture2D AnomalyIndicator => _anomalyIndicator?.Value;

    [LoadTexture(TexturePathPrefix + "UI/AnomalyIndicator_Off")]
    internal static Asset<Texture2D> _anomalyIndicator_Off;
    public static Texture2D AnomalyIndicator_Off => _anomalyIndicator_Off?.Value;

    [LoadTexture(TexturePathPrefix + "UI/AnomalyIndicator_Border")]
    internal static Asset<Texture2D> _anomalyIndicator_Border;
    public static Texture2D AnomalyIndicator_Border => _anomalyIndicator_Border?.Value;

    [LoadTexture(TexturePathPrefix + "UI/AnomalyIndicator_Locked")]
    internal static Asset<Texture2D> _anomalyIndicator_Locked;
    public static Texture2D AnomalyIndicator_Locked => _anomalyIndicator_Locked?.Value;

    [LoadTexture(TexturePathPrefix + "UI/AnomalyUltraIndicator")]
    internal static Asset<Texture2D> _anomalyUltraIndicator;
    public static Texture2D AnomalyUltraIndicator => _anomalyUltraIndicator?.Value;

    [LoadTexture(TexturePathPrefix + "UI/AnomalyUltraIndicator_Off")]
    internal static Asset<Texture2D> _anomalyUltraIndicator_Off;
    public static Texture2D AnomalyUltraIndicator_Off => _anomalyUltraIndicator_Off?.Value;

    [LoadTexture(TexturePathPrefix + "UI/AnomalyUltraIndicator_Border")]
    internal static Asset<Texture2D> _anomalyUltraIndicator_Border;
    public static Texture2D AnomalyUltraIndicator_Border => _anomalyUltraIndicator_Border?.Value;

    [LoadTexture(TexturePathPrefix + "UI/AnomalyUltraIndicator_Locked")]
    internal static Asset<Texture2D> _anomalyUltraIndicator_Locked;
    public static Texture2D AnomalyUltraIndicator_Locked => _anomalyUltraIndicator_Locked?.Value;

    [LoadTexture(TexturePathPrefix + "UI/HPThresholdIndicator")]
    internal static Asset<Texture2D> _hpThresholdIndicator;
    public static Texture2D HPThresholdIndicator => _hpThresholdIndicator?.Value;

    [LoadTexture(TexturePathPrefix + "UI/HPThresholdIndicator_Sub")]
    internal static Asset<Texture2D> _hpThresholdIndicator_Sub;
    public static Texture2D HPThresholdIndicator_Sub => _hpThresholdIndicator_Sub?.Value;

    [LoadTexture(TexturePathPrefix + "UI/HPThresholdIndicator_Border")]
    internal static Asset<Texture2D> _hpThresholdIndicator_Border;
    public static Texture2D HPThresholdIndicator_Border => _hpThresholdIndicator_Border?.Value;

    [LoadTexture(TexturePathPrefix + "UI/HPThresholdIndicator_SubBorder")]
    internal static Asset<Texture2D> _hpThresholdIndicator_SubBorder;
    public static Texture2D HPThresholdIndicator_SubBorder => _hpThresholdIndicator_SubBorder?.Value;
}
