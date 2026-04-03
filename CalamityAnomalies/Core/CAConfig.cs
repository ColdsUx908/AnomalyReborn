using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace CalamityAnomalies.Core;

public sealed class CAServerConfig : ModConfig
{
    public static CAServerConfig Instance { get; private set; }

    public override ConfigScope Mode => ConfigScope.ServerSide;

    public override bool AcceptClientChanges(ModConfig pendingConfig, int whoAmI, ref NetworkText message) => false;

    public override void OnLoaded() => Instance = this;
}

public sealed class CAClientConfig : ModConfig
{
    public static CAClientConfig Instance { get; private set; }

    public override ConfigScope Mode => ConfigScope.ClientSide;

    public override bool AcceptClientChanges(ModConfig pendingConfig, int whoAmI, ref NetworkText message) => false;

    public override void OnLoaded() => Instance = this;

    /// <summary>
    /// 辅助视觉效果。
    /// </summary>
    [DefaultValue(false)]
    public bool AuxiliaryVisualEffects;
}
