using Anomalies.AnomalyMode;

namespace Anomalies.Common;

public sealed partial class AnomalySharedData : ModSystem
{
    public const string ModLocalizationPrefix = "Mods.Anomalies.";
    public const string BossLocalizationPrefix = ModLocalizationPrefix + "Bosses.";
    public const string TweakLocalizationPrefix = ModLocalizationPrefix + "Tweaks.";
    public const string CalamityModLocalizationPrefix = "Mods.CalamityMod.";

    public const string ModPath = "Anomalies/";
    public const string ModTexturePath = "Anomalies/Assets/Textures/";

    public static readonly Color MainColor = Color.HotPink;

    public static readonly Color SecondaryColor = Color.Pink;

    public static readonly List<Color> ColorList = [MainColor, SecondaryColor, MainColor];
    public static readonly List<Color> ColorList2 = [MainColor, TOColors.CelestialColor, MainColor];

    public static readonly Color AnomalyUltramundaneColor = new(232, 151, 255);

    public static readonly List<Color> ColorList3 = [MainColor, AnomalyUltramundaneColor, MainColor];

    public static readonly Color AromalyColor = Color.IndianRed;

    public static readonly Color RebornColor = new(0xff, 0xa5, 0x00);

    public static Color AnomalyTitleColor => Color.LerpMany(ColorList2, TOMathUtils.Interpolation.QuadraticEaseIn(TOGameTimeFunctions.GetTimeSin(0.5f, 2.5f, unsigned: true)) / 2f);
    public static Color AnomalyUltramundaneTitleColor => Color.LerpMany(ColorList3, TOMathUtils.Interpolation.QuadraticEaseIn(TOGameTimeFunctions.GetTimeSin(0.5f, 3f, unsigned: true)) / 1.5f);

    public static Assembly Assembly => field ??= AnomalyMain.Instance.Code;

    public static string ModName => field ??= AnomalyMain.Instance.Name;

    public static Color GetGradientColor(float maxRatio = 0.5f) => Color.LerpMany(ColorList, TOGameTimeFunctions.GetTimeSin(maxRatio / 2f, unsigned: true));

    /// <summary>
    /// 是否使用 Anomalies 的着色器。
    /// <br/>启用条件：
    /// <list type="bullet">
    /// <item>Anomalies 客户端配置中的 EnableShaders 为 true</item>
    /// <item>照明不为复古或迷幻</item>
    /// <item>水波质量开启</item>
    /// </list>
    /// </summary>
    public static bool ShouldUseShaders => AnomalyClientConfig.Instance.EnableShaders
        && Lighting.NotRetro //照明不为复古或迷幻
        && Main.WaveQuality >= 1; //水波质量开启

    #region Sets
    public static bool[] TweakedNPCs { get; private set; }
    public static bool[] TweakedProjectiles { get; private set; }
    public static bool[] TweakedItems { get; private set; }

    public static bool[] DontApplyAnomalyRarityShaderItems { get; private set; }

    public override void ResizeArrays()
    {
        TweakedNPCs = NPCID.Sets.Factory.CreateBoolSet(false);
        TweakedProjectiles = ProjectileID.Sets.Factory.CreateBoolSet(false);
        TweakedItems = ItemID.Sets.Factory.CreateBoolSet(false);

        DontApplyAnomalyRarityShaderItems = ItemID.Sets.Factory.CreateBoolSet(false);
    }
    #endregion

    #region World
    /// <summary>
    /// 异象模式。
    /// </summary>
    public static bool Anomaly
    {
        get;
        internal set
        {
            if (Main.netMode == NetmodeID.MultiplayerClient && !AnomalySynchronization.ReceivingWorldState)
            {
                AnomalySynchronization.RequestAnomalyMode(value);
                return;
            }

            if (field == value)
                return;

            if (!value && AnomalyUltramundane)
                AnomalyHandler.DisableUltra();

            field = value;

            if (TONetworkEnvironment.NotClient && !AnomalySynchronization.ReceivingWorldState)
            {
                string key = ModLocalizationPrefix + "AnomalyMode." + (Main.zenithWorld ? "Aromaly." : "") + (value ? "Activate" : "Deactivate");
                Color color = Main.zenithWorld ? AromalyColor : MainColor;
                TOLocalizationUtils.ChatLocalizedText(key, color);
            }
            if (value && !AnomalySynchronization.ReceivingWorldState)
                AnomalyHandler.CheckAnomalyUltra();

            OnAnomalyModeToggled?.Invoke(value);
            AnomalySynchronization.SyncAnomalyMode();
        }
    }
    public static event Action<bool> OnAnomalyModeToggled;

    /// <summary>
    /// 异象超凡。
    /// </summary>
    public static bool AnomalyUltramundane
    {
        get;
        internal set
        {
            if (Main.netMode == NetmodeID.MultiplayerClient && !AnomalySynchronization.ReceivingWorldState)
                return;
            if (field == value)
                return;

            field = value;
            if (TONetworkEnvironment.NotClient && !AnomalySynchronization.ReceivingWorldState)
                TOLocalizationUtils.ChatLocalizedText(ModLocalizationPrefix + "AnomalyMode." + (value ? "UltraActivate" : "UltraDeactivate"), Color.Red);

            OnAnomalyUltramundaneToggled?.Invoke(value);
            AnomalySynchronization.SyncAnomalyMode();
        }
    }
    public static event Action<bool> OnAnomalyUltramundaneToggled;

    /// <summary>
    /// 香溢模式。
    /// <br/>在 GFB 世界中启用。
    /// </summary>
    public static bool Aromaly => Anomaly && Main.zenithWorld;

    /// <summary>
    /// 故事模式。
    /// </summary>
    public static bool StoryMode
    {
        get;
        internal set
        {
            if (Main.netMode == NetmodeID.MultiplayerClient && !AnomalySynchronization.ReceivingWorldState || field == value)
                return;
            field = value;
            AnomalySynchronization.SyncAnomalyMode();
        }
    }

    /// <summary>
    /// 极限模式。
    /// </summary>
    public static bool EXMode
    {
        get;
        internal set
        {
            if (Main.netMode == NetmodeID.MultiplayerClient && !AnomalySynchronization.ReceivingWorldState || field == value)
                return;
            field = value;
            AnomalySynchronization.SyncAnomalyMode();
        }
    }

    public override void NetSend(BinaryWriter writer) => AnomalySynchronization.WriteWorldState(writer);

    public override void NetReceive(BinaryReader reader) => AnomalySynchronization.ReadWorldState(reader);

    public override void OnWorldLoad()
    {
        ResetWorldState();
    }

    public override void OnWorldUnload()
    {
        ResetWorldState();
    }

    private static void ResetWorldState()
    {
        // Reset through the receive path: no packets are sent while entering/leaving a world.
        AnomalySynchronization.ApplyWorldState(default);
    }

    public override void SaveWorldData(TagCompound tag)
    {
        tag["Anomaly"] = Anomaly;
    }

    public override void LoadWorldData(TagCompound tag)
    {
        Anomaly = tag.GetBool("Anomaly");
    }

    public override void SaveWorldHeader(TagCompound tag)
    {
        tag["Anomaly"] = Anomaly;
    }

    #endregion World
}
