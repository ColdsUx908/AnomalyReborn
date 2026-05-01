// Developed by ColdsUx

using Terraria.GameContent.Creative;

namespace Transoceanic.Common;

public sealed class TOSharedData : ModSystem, ITOLoader
{
    /// <summary>
    /// 指示当前是否处于调试（DEBUG）模式。
    /// <br/>在 Debug 编译配置下默认为 <see langword="true"/>，否则默认为 <see langword="false"/>。
    /// <br/>可通过 <see cref="DebugModeCommand"/> 在游戏内动态调整。
    /// </summary>
    public static bool DEBUG { get; internal set; } =
#if DEBUG
    true;
#else
    false;
#endif

    /// <summary>
    /// 模组本地化键的前缀，用于拼接完整的本地化键路径。
    /// </summary>
    public const string ModLocalizationPrefix = "Mods.Transoceanic.";

    /// <summary>
    /// 调试相关本地化键的前缀，以 <see cref="ModLocalizationPrefix"/> 开头。
    /// </summary>
    public const string DebugPrefix = ModLocalizationPrefix + "DEBUG.";

    /// <summary>
    /// 调试用错误信息的完整本地化键。
    /// </summary>
    public const string DebugErrorMessageKey = ModLocalizationPrefix + "DEBUG.ErrorMessage";

    /// <summary>
    /// 通用的字符串为空或空白时的错误提示文本。
    /// </summary>
    public const string StringEmptyError = "String cannot be null or whitespace.";

    /// <summary>
    /// 调试模式下的预设玩家名称，用于身份检查。
    /// </summary>
    private const string DEBUGPlayerName = "~ColdsUx";

    /// <summary>
    /// 判断给定的玩家是否为调试玩家。
    /// </summary>
    /// <param name="player">要检查的玩家实例。</param>
    /// <returns>当 <see cref="DEBUG"/> 为 <see langword="true"/> 且玩家名称等于 <see cref="DEBUGPlayerName"/> 时返回 <see langword="true"/>；否则为 <see langword="false"/>。</returns>
    public static bool IsDEBUGPlayer(Player player) => DEBUG && player?.name == DEBUGPlayerName;

    /// <summary>
    /// 调试警告信息的颜色（橙色）。
    /// </summary>
    public static readonly Color TODebugWarnColor = Color.Orange;

    /// <summary>
    /// 调试错误信息的颜色（红色）。
    /// </summary>
    public static readonly Color TODebugErrorColor = new(0xFF, 0x00, 0x00);

    /// <summary>
    /// 天空的颜色（淡青色）。
    /// </summary>
    public static readonly Color CelestialColor = new(0xAF, 0xFF, 0xFF);

    #region World
    /// <summary>
    /// 是否启用 Transoceanic 模组内置的网络同步。
    /// <br/>由于 Transoceanic 是客户端模组，该选项必须由依赖模组手动开启。
    /// </summary>
    /// <remarks>
    /// 一旦设置为 <see langword="true"/> 后，除非模组正在卸载，否则不可重新设置为 <see langword="false"/>，否则将抛出 <see cref="InvalidOperationException"/>。
    /// </remarks>
    /// <exception cref="InvalidOperationException">在非卸载状态下尝试将已启用的同步关闭。</exception>
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

    /// <summary>
    /// 当前日期是否为愚人节（4 月 1 日）。
    /// </summary>
    public static bool AprilFools => DateTime.Now is (int _, 4, 1);

    /// <summary>
    /// 当前是否处于多人模式（既非单人客户端也非单人服务器）。
    /// </summary>
    public static bool Multiplayer => Main.netMode != NetmodeID.SinglePlayer;

    /// <summary>
    /// 当前是否处于通用客户端模式（服务器或单人，而非多人客户端）。
    /// </summary>
    /// <remarks>
    /// 即 <see cref="Main.netMode"/> 不等于 <see cref="NetmodeID.MultiplayerClient"/>，用于判断代码是否运行在本地完整权限环境下。
    /// </remarks>
    public static bool GeneralClient => Main.netMode != NetmodeID.MultiplayerClient;

    /// <summary>
    /// 获取当前帧的游戏时间对象 <see cref="GameTime"/>，由主循环更新。
    /// </summary>
    public static GameTime Time { get; private set; }

    /// <summary>
    /// 从游戏开始经过的总时间，以秒为单位。
    /// </summary>
    public static float TotalSeconds => (float)Time.TotalGameTime.TotalSeconds;

    /// <summary>
    /// 从游戏开始经过的总时间，以分钟为单位。
    /// </summary>
    public static float TotalMinutes => TotalSeconds / 60f;

    /// <summary>
    /// 游戏内的帧计时器（模组自定义类型 <c>TerrariaTimer</c>）。
    /// 自世界加载起开始累计，每帧递增。
    /// </summary>
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

    /// <summary>
    /// 获取封装了当前游戏日时间和月相数据的 <see cref="TerrariaTime"/> 实例。
    /// </summary>
    public static TerrariaTime TerrariaNow => new(Time24Hour, Main.GetMoonPhase());

    /// <summary>
    /// 当前世界是否为真正的大师模式（非旅途模式滑块模拟，非专家+FTW）。
    /// </summary>
    public static bool TrueMasterMode { get; internal set; }

    /// <summary>
    /// 当前旅途模式世界是否通过难度滑块启用了大师难度。
    /// </summary>
    public static bool JourneyMasterMode { get; internal set; }

    /// <summary>
    /// 综合判断当前世界是否处于大师模式（包含真正大师模式和旅途大师滑块）。
    /// </summary>
    public static bool MasterMode => TrueMasterMode || JourneyMasterMode;

    /// <summary>
    /// 综合判断当前世界是否处于传奇模式（FTW 且处于任何大师模式）。
    /// </summary>
    public static bool LegendaryMode => Main.getGoodWorld && MasterMode;

    /// <summary>
    /// 当前世界中存活的 Boss 实体列表。
    /// 在 <see cref="PostUpdateNPCs"/> 中更新。
    /// </summary>
    public static List<NPC> BossList { get; internal set; } = [];

    /// <summary>
    /// 当前世界中是否有 Boss 存活。
    /// </summary>
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