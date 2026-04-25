// Designed by ColdsUx

using Terraria.GameContent.Creative;

namespace Transoceanic.Common;

public sealed class TOSharedData : ModSystem, ITOLoader
{
    public static bool DEBUG { get; internal set; } =
#if DEBUG
true;
#else
    false;
#endif

    public const string ModLocalizationPrefix = "Mods.Transoceanic.";
    public const string DebugPrefix = ModLocalizationPrefix + "DEBUG.";
    public const string DebugErrorMessageKey = ModLocalizationPrefix + "DEBUG.ErrorMessage";
    public const string StringEmptyError = "String cannot be null or whitespace.";
    private const string DEBUGPlayerName = "~ColdsUx";
    public static bool IsDEBUGPlayer(Player player) => DEBUG && player?.name == DEBUGPlayerName;

    public static readonly Color TODebugWarnColor = Color.Orange;
    public static readonly Color TODebugErrorColor = new(0xFF, 0x00, 0x00);
    public static readonly Color CelestialColor = new(0xAF, 0xFF, 0xFF);

    #region World
    /// <summary>
    /// 是否启用Transoceanic模组内置的网络同步。
    /// <br/>由于Transoceanic为客户端模组，该选项必须由依赖模组手动开启。
    /// </summary>
    public static bool SyncEnabled
    {
        get;
        set
        {
            if (field && !value && !TOMain.Unloading)
                throw new InvalidOperationException("SyncEnabled cannot be set to false after it has been set to true, unless unloading.");
            field = value;
        }
    }

    public static bool AprilFools => DateTime.Now is (int _, 4, 1);

    public static bool Multiplayer => Main.netMode != NetmodeID.SinglePlayer;

    public static bool GeneralClient => Main.netMode != NetmodeID.MultiplayerClient;

    public static GameTime Time { get; private set; }
    public static float TotalSeconds => (float)Time.TotalGameTime.TotalSeconds;
    public static float TotalMinutes => TotalSeconds / 60f;

    public static TerrariaTimer GameTimer { get; internal set; }

    /// <summary>
    /// 获取当前游戏内时间的 24 小时制表示（以小时为单位的浮点数，范围 [0.0, 24.0)）。
    /// </summary>
    /// <remarks>
    /// 泰拉瑞亚内部使用 <see cref="Main.dayTime"/>（<see cref="bool"/>）指示是否为白天，<see cref="Main.time"/>（<see cref="double"/>）记录当前时段内经过的内部时间单位数。
    /// 每 1 游戏小时对应 3600 个内部单位。白天从 4:30 AM 开始持续 15 小时（最大 <c>Main.time</c> = 54000），夜晚从 7:30 PM 开始持续 9 小时（最大 <c>Main.time</c> = 32400）。
    /// 根据时段加上基准偏移：白天偏移 4.5 小时，夜晚偏移 19.5 小时并对 24 取模以正确处理跨午夜情形。
    /// </remarks>
    public static double Time24Hour => Main.dayTime ? 4.5 + Main.time / 3600.0 : (19.5 + Main.time / 3600.0) % 24.0;

    public static TerrariaTime TerrariaTime => new(Time24Hour, Main.GetMoonPhase());

    public static bool TrueMasterMode { get; internal set; }

    public static bool JourneyMasterMode { get; internal set; }

    public static bool MasterMode => TrueMasterMode || JourneyMasterMode;

    public static bool TrueLegedaryMode => Main.getGoodWorld && TrueMasterMode;

    public static bool LegendaryMode => Main.getGoodWorld && MasterMode;

    public static List<NPC> BossList { get; internal set; } = [];

    public static bool BossActive { get; internal set; }

    public override void PreUpdateEntities()
    {
        GameTimer++;

        GameModeData gameModeInfo = Main_Publicizer._currentGameModeInfo;
        TrueMasterMode = gameModeInfo.IsMasterMode;
        if (gameModeInfo.IsJourneyMode)
        {
            CreativePowers.DifficultySliderPower power = CreativePowerManager.Instance.GetPower<CreativePowers.DifficultySliderPower>();
            bool currentJourneyMaster = power.StrengthMultiplierToGiveNPCs == 3f;
            if (power.GetIsUnlocked())
                JourneyMasterMode = currentJourneyMaster;
            else if (!currentJourneyMaster)
                JourneyMasterMode = false;
        }
        else
            JourneyMasterMode = false;
    }

    public override void PostUpdateNPCs()
    {
        BossList = NPC.Bosses.ToList();
        BossActive = BossList.Count > 0;
    }

    public override void OnWorldLoad()
    {
        GameTimer = 0;
    }

    public override void OnWorldUnload()
    {
        GameTimer = 0;
    }

    void ITOLoader.Load()
    {
        GameTimer = 0;

        On_Main.Update += On_Main_Update;

        static void On_Main_Update(On_Main.orig_Update orig, Main self, GameTime gameTime)
        {
            try
            {
                Time = gameTime;
            }
            catch { }
            orig(self, gameTime);
        }
    }

    void ITOLoader.Unload()
    {
        GameTimer = 0;
        TrueMasterMode = false;
        JourneyMasterMode = false;
        BossList = [];
        BossActive = false;
    }
    #endregion World
}