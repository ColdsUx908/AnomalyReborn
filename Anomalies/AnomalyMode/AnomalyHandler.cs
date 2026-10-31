using Terraria.GameContent.UI.Elements;

namespace Anomalies.AnomalyMode;

public sealed class AnomalyHandler : ModSystem
{
    public const string LocalizationPrefix = AnomalySharedData.ModLocalizationPrefix + "AnomalyMode.";

    #region 资产
    public const string Path = AnomalySharedData.ModPath + "AnomalyMode/";

    [LoadTexture(Path + "Indicator")]
    internal static Asset<Texture2D> _Indicator;
    public static Texture2D Indicator => _Indicator?.Value;

    [LoadTexture(Path + "Indicator_Off")]
    internal static Asset<Texture2D> _IndicatorOff;
    public static Texture2D Indicator_Off => _IndicatorOff?.Value;

    [LoadTexture(Path + "Indicator_Border")]
    internal static Asset<Texture2D> _IndicatorBorder;
    public static Texture2D Indicator_Border => _IndicatorBorder?.Value;

    [LoadTexture(Path + "Indicator_Locked")]
    internal static Asset<Texture2D> _IndicatorLocked;
    public static Texture2D Indicator_Locked => _IndicatorLocked?.Value;

    [LoadTexture(Path + "UltraIndicator")]
    internal static Asset<Texture2D> _UltraIndicator;
    public static Texture2D UltraIndicator => _UltraIndicator?.Value;

    [LoadTexture(Path + "UltraIndicator_Off")]
    internal static Asset<Texture2D> _UltraIndicatorOff;
    public static Texture2D UltraIndicator_Off => _UltraIndicatorOff?.Value;

    [LoadTexture(Path + "UltraIndicator_Border")]
    internal static Asset<Texture2D> _UltraIndicatorBorder;
    public static Texture2D UltraIndicator_Border => _UltraIndicatorBorder?.Value;

    [LoadTexture(Path + "UltraIndicator_Locked")]
    internal static Asset<Texture2D> _UltraIndicatorLocked;
    public static Texture2D UltraIndicator_Locked => _UltraIndicatorLocked?.Value;

    public static readonly SoundStyle ActivationSound = new(Path + "Activation");
    public static readonly SoundStyle AromalyActivationSound = new(Path + "AromalyActivation") { Volume = 0.6f };
    #endregion 资产

    #region 世界内难度管理
    public static event Action AnomalyEnabledUpdate;

    public override void PreUpdateWorld()
    {
        if (Main.netMode == NetmodeID.MultiplayerClient)
            return;

        if (AnomalySharedData.Anomaly)
        {
            if (Main.netMode != NetmodeID.SinglePlayer) //暂不支持多人
            {
                TOLocalizationUtils.ChatLiteralText("Anomaly doe not support Multiplier.");
                DisableAnomaly();
                return;
            }

            if (!TOWorldState.MasterMode)
            {
                DisableAnomaly();
                return;
            }

            AnomalyEnabledUpdate?.Invoke();
        }

        CheckAnomalyUltra();
    }

    public static void DisableAnomaly()
    {
        if (TONetworkEnvironment.NotClient)
            TOLocalizationUtils.ChatLocalizedText(LocalizationPrefix + "Invalid", Color.Red);
        AnomalySharedData.Anomaly = false;
    }

    public static void DisableUltra()
    {
        AnomalySharedData.AnomalyUltramundane = false;
    }

    public static void EnableUltra()
    {
        AnomalySharedData.AnomalyUltramundane = true;
    }

    public static void InvalidInfo_NotLegendary()
    {
        if (TONetworkEnvironment.NotClient)
            TOLocalizationUtils.ChatLocalizedText(LocalizationPrefix + "UltraInvalid_NotLegendary", Color.Red);
    }

    public static void InvalidInfo_Aromaly()
    {
        if (TONetworkEnvironment.NotClient)
            TOLocalizationUtils.ChatLocalizedText(LocalizationPrefix + "UltraInvalid_Aromaly", AnomalySharedData.AromalyColor);
        //SoundEngine.PlaySound();
    }

    public static void CheckAnomalyUltra()
    {
        if (AnomalySharedData.Anomaly)
        {
            switch (TOWorldState.LegendaryMode, !Main.zenithWorld)
            {
                case (false, true) when AnomalySharedData.AnomalyUltramundane: //不是传奇难度，不在GFB世界
                    InvalidInfo_NotLegendary();
                    DisableUltra();
                    break;
                case (true, false) when AnomalySharedData.AnomalyUltramundane: //是传奇难度，在GFB世界
                    InvalidInfo_Aromaly();
                    DisableUltra();
                    break;
                case (false, false) when AnomalySharedData.AnomalyUltramundane: //不是传奇难度，且在GFB世界
                    InvalidInfo_NotLegendary();
                    InvalidInfo_Aromaly();
                    DisableUltra();
                    break;
                case (true, true) when !AnomalySharedData.AnomalyUltramundane: //是传奇难度，且不在GFB世界，应开启异象超凡
                    EnableUltra();
                    break;
                default:
                    break;
            }
        }
        else if (AnomalySharedData.AnomalyUltramundane)
            DisableUltra();
    }
    #endregion 世界内难度管理

    /// <summary>在内容加载时安装世界难度文本钩子，数据在实际绘制时读取。</summary>
    public override void Load()
    {
        //世界难度显示（渐变色）
        On_AWorldListItem.GetDifficulty += On_AWorldListItem_GetDifficulty;

        static void On_AWorldListItem_GetDifficulty(On_AWorldListItem.orig_GetDifficulty orig, AWorldListItem self, out string expertText, out Color gameModeColor)
        {
            orig(self, out expertText, out gameModeColor);

            if (gameModeColor == Main.creativeModeColor)
                return;

            if (self.Data.TryGetHeaderData<AnomalySharedData>(out TagCompound tag) && tag.GetBool("Anomaly"))
            {
                expertText = Language.GetTextValue(LocalizationPrefix + "Name");
                gameModeColor = AnomalySharedData.AnomalyTitleColor;
            }
        }
    }
}
