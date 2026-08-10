// Developed by ColdsUx

using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace Anomalies.Common;

public sealed class AnomalyServerConfig : ModConfig
{
    public static AnomalyServerConfig Instance { get; private set; }

    public override ConfigScope Mode => ConfigScope.ServerSide;

    public override bool AcceptClientChanges(ModConfig pendingConfig, int whoAmI, ref NetworkText message) => false;

    public override void OnLoaded() => Instance = this;
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
