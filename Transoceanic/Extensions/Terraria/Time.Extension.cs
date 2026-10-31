namespace Transoceanic.Extensions.Terraria;

/// <summary>
/// 提供 Terraria 实体、生成、世界与着色器的便捷扩展。
/// </summary>
public static partial class TOTerrariaExtensions
{
    /// <summary>
    /// 将当前时间直接应用到泰拉瑞亚游戏世界中。
    /// </summary>
    /// <remarks>
    /// 此方法通过修改 <c>Main.dayTime</c> 和 <c>Main.time</c> 来设置游戏内时间，并在月相有值时设置 <c>Main.moonPhase</c>。
    /// 游戏内时间转换规则基于泰拉瑞亚的标准计时方式：
    /// <list type="bullet">
    /// <item><description>0:00 AM（总秒数 0）到 4:30 AM（16200 秒）为夜晚。</description></item>
    /// <item><description>4:30 AM 到 7:30 PM（70200 秒）为白天。</description></item>
    /// <item><description>7:30 PM 到午夜为夜晚。</description></item>
    /// </list>
    /// </remarks>
    public static void Apply(this TerrariaTime time)
    {
        (Main.dayTime, Main.time) = time.TotalSeconds switch
        {
            < 16200 => (false, 16200 + time.TotalSeconds),
            < 70200 => (true, time.TotalSeconds - 16200),
            _ => (false, time.TotalSeconds - 70200)
        };
        if (time.MoonPhase.HasValue)
            Main.moonPhase = (int)time.MoonPhase;
    }
}
