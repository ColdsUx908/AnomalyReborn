// Developed by ColdsUx

using CalamityAnomalies.Anomaly;

namespace CalamityAnomalies.Common;

public sealed class CASharedData : ModSystem
{
    public const string ModLocalizationPrefix = "Mods.CalamityAnomalies.";
    public const string AnomalyLocalizationPrefix = ModLocalizationPrefix + "Anomaly.";
    public const string TweakLocalizationPrefix = ModLocalizationPrefix + "Tweaks.";
    public const string CalamityModLocalizationPrefix = "Mods.CalamityMod.";
    public const string CalamityInvisibleProj = "CalamityMod/Projectiles/InvisibleProj";
    public const string CATexturePath = "CalamityAnomalies/Assets/Textures/";

    public static readonly Color MainColor = Color.HotPink;

    public static readonly Color SecondaryColor = Color.Pink;

    public static readonly List<Color> ColorList = [MainColor, SecondaryColor, MainColor];
    public static readonly List<Color> ColorList2 = [MainColor, TOSharedData.CelestialColor, MainColor];

    public static readonly Color AnomalyUltramundaneColor = new(0xE8, 0x97, 0xFF);

    public static readonly List<Color> ColorList3 = [MainColor, AnomalyUltramundaneColor, MainColor];

    public static readonly Color GFBColor = Color.IndianRed;

    public static readonly Color RebornColor = new(0xff, 0xa5, 0x00);

    public static Assembly Assembly => field ??= CAMain.Instance.Code;

    public static string ModName => field ??= CAMain.Instance.Name;

    public static Color GetGradientColor(float maxRatio = 0.5f) => Color.LerpMany(ColorList, TOMathUtils.TimeWrappingFunction.GetTimeSin(maxRatio / 2f, unsigned: true));

    #region Sets
    public static bool[] TweakedNPCs { get; private set; }
    public static bool[] TweakedProjectiles { get; private set; }
    public static bool[] TweakedItems { get; private set; }

    public override void ResizeArrays()
    {
        TweakedNPCs = NPCID.Sets.Factory.CreateBoolSet(false);
        TweakedProjectiles = ProjectileID.Sets.Factory.CreateBoolSet(false);
        TweakedItems = ItemID.Sets.Factory.CreateBoolSet(false);
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
            if (field == value)
                return;

            if (!value && AnomalyUltramundane)
                AnomalyModeHandler.DisableUltra();

            field = value;

            if (TOSharedData.GeneralClient)
            {
                string key = AnomalyLocalizationPrefix + "AnomalyMode." + (value ? "Activate" : "Deactivate") + (Main.zenithWorld ? "_GFB" : "");
                Color color = Main.zenithWorld ? GFBColor : MainColor;
                TOLocalizationUtils.ChatLocalizedText(key, color);
            }
            if (value)
                AnomalyModeHandler.CheckAnomalyUltra();

            CASynchronization.SyncAnomalyMode();
            OnAnomalyModeToggled?.Invoke(value);
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
            if (field == value)
                return;

            field = value;
            if (TOSharedData.GeneralClient)
                TOLocalizationUtils.ChatLocalizedText(AnomalyLocalizationPrefix + "AnomalyMode." + (value ? "UltraActivate" : "UltraDeactivate"), Color.Red);

            OnAnomalyUltramundaneToggled?.Invoke(value);
        }

    }
    public static event Action<bool> OnAnomalyUltramundaneToggled;

    /// <summary>
    /// 故事模式。
    /// </summary>
    public static bool StoryMode { get; internal set; }

    public override void OnWorldLoad()
    {
        AnomalyUltramundane = false;
    }

    public override void OnWorldUnload()
    {
        AnomalyUltramundane = false;
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

    public static class QuickAccess
    {
        public static bool Ultra => AnomalyUltramundane;
        public static bool Story => StoryMode;
    }
}


