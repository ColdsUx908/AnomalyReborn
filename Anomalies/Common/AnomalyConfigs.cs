// Developed by ColdsUx

using System.ComponentModel;
using CalamityMod;
using Terraria.ModLoader.Config;

namespace Anomalies.Common;

public sealed class AnomalyServerConfig : ModConfig
{
    public static AnomalyServerConfig Instance { get; private set; }

    public override ConfigScope Mode => ConfigScope.ServerSide;

    public override bool AcceptClientChanges(ModConfig pendingConfig, int whoAmI, ref NetworkText message)
    {
        if (whoAmI == 0)
            return true;
        else
        {
            message = Language.GetText(AnomalySharedData.ModLocalizationPrefix + "Configs.AnomalyServerConfig.Denied").ToNetworkText();
            return false;
        }
    }

    public override void OnLoaded() => Instance = this;

    [Header("BaseBoosts")]

    [BackgroundColor(192, 54, 64, 192)]
    [DefaultValue(true)]
    public bool FasterBaseSpeed { get; set; }

    [BackgroundColor(192, 54, 64, 192)]
    [DefaultValue(true)]
    public bool FasterJumpSpeed { get; set; }

    [BackgroundColor(192, 54, 64, 192)]
    [DefaultValue(true)]
    public bool FasterRopeClimbSpeed { get; set; }

    [BackgroundColor(192, 54, 64, 192)]
    [DefaultValue(true)]
    public bool FasterTilePlacement { get; set; }

    [BackgroundColor(192, 54, 64, 192)]
    [DefaultValue(true)]
    public bool AllBaseBoosts
    {
        get => FasterBaseSpeed && FasterJumpSpeed && FasterRopeClimbSpeed && FasterTilePlacement;
        set
        {
            FasterBaseSpeed = value;
            FasterJumpSpeed = value;
            FasterRopeClimbSpeed = value;
            FasterTilePlacement = value;
        }
    }
}

public sealed class AnomalyClientConfig : ModConfig
{
    public static AnomalyClientConfig Instance { get; private set; }

    public override ConfigScope Mode => ConfigScope.ClientSide;

    public override bool AcceptClientChanges(ModConfig pendingConfig, int whoAmI, ref NetworkText message) => false;

    public override void OnLoaded() => Instance = this;

    /// <summary>
    /// 辅助视觉效果。
    /// </summary>
    [DefaultValue(false)]
    public bool AuxiliaryVisualEffects;

    /// <summary>
    /// 是否使用灾厄模组风格的纹理。
    /// </summary>
    [DefaultValue(false)]
    public bool UseCalamityStyleTextures;
}
