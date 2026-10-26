namespace Transoceanic.Game.Timing;

public sealed class TOGameTime : ModSystem, ITOLoader
{
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

    public override void PreUpdateEntities() => GameTimer++;

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
            finally
            {
                orig(self, gameTime);
            }
        }
    }

    void ITOLoader.Unload() => GameTimer = 0;
}
