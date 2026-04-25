// Designed by ColdsUx

namespace Transoceanic.DataStructures;

/// <summary>
/// 表示泰拉瑞亚世界中的时间，包含一天中的具体时刻（小时、分钟、秒）以及可选的月相信息。
/// </summary>
/// <remarks>
/// <para/>如需使用泰拉瑞亚游戏内部时间，请使用 <see cref="TOSharedData.TerrariaTime"/>，或用 <see cref="TOSharedData.Time24Hour"/> 手动构造。
/// <para/>此结构为只读值类型，实现了 <see cref="IEquatable{TerrariaTime}"/> 接口以支持高效的相等性比较。
/// </remarks>
public readonly struct TerrariaTime : IEquatable<TerrariaTime>
{
    /// <summary>
    /// 以小时为单位的时间值，范围为 [0.0, 24.0)。
    /// </summary>
    public readonly double Time;

    /// <summary>
    /// 月相状态。如果为 <see langword="null"/>，则在应用时间时不会修改游戏当前的月相。
    /// </summary>
    public readonly MoonPhase? MoonPhase;

    /// <summary>
    /// 获取从午夜开始经过的总秒数。
    /// </summary>
    /// <value>范围在 [0, 86399] 之间的整数。</value>
    public int TotalSeconds => (int)(Time * 3600);

    /// <summary>
    /// 获取当前时间的小时部分，采用 24 小时制。
    /// </summary>
    /// <value>范围在 [0, 23] 之间的整数。</value>
    public int Hour => (int)Time;

    /// <summary>
    /// 获取当前时间的分钟部分。
    /// </summary>
    /// <value>范围在 [0, 59] 之间的整数。</value>
    public int Minute => (int)((Time - Hour) * 60.0);

    /// <summary>
    /// 获取当前时间的秒钟部分。
    /// </summary>
    /// <value>范围在 [0, 59] 之间的整数。</value>
    public int Second => (int)(((Time - Hour) * 60.0 - Minute) * 60.0);

    /// <summary>
    /// 使用小时数（浮点）和可选的月相初始化 <see cref="TerrariaTime"/> 结构的新实例。
    /// </summary>
    /// <param name="time">以小时为单位的时间，必须处于 [0, 24) 范围内。</param>
    /// <param name="moonPhase">月相，如果为 <see langword="null"/> 则表示不指定月相。</param>
    public TerrariaTime(double time, MoonPhase? moonPhase = null)
    {
        Time = time;
        MoonPhase = moonPhase;
    }

    /// <summary>
    /// 使用从午夜开始的总秒数和可选的月相初始化 <see cref="TerrariaTime"/> 结构的新实例。
    /// </summary>
    /// <param name="totalSecond">从 0:00 开始经过的秒数，有效范围为 [0, 86400)。</param>
    /// <param name="moonPhase">月相，如果为 <see langword="null"/> 则表示不指定月相。</param>
    public TerrariaTime(int totalSecond, MoonPhase? moonPhase = null) : this(totalSecond / 3600.0, moonPhase) { }

    /// <summary>
    /// 使用小时、分钟、秒和可选的月相初始化 <see cref="TerrariaTime"/> 结构的新实例。
    /// </summary>
    /// <param name="hour">小时（0-23）。</param>
    /// <param name="minute">分钟（0-59）。</param>
    /// <param name="second">秒（0-59），默认为 0。</param>
    /// <param name="moonPhase">月相，如果为 <see langword="null"/> 则表示不指定月相。</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// 当 <paramref name="hour"/> 不在 0 到 23 之间，
    /// 或 <paramref name="minute"/> 不在 0 到 59 之间，
    /// 或 <paramref name="second"/> 不在 0 到 59 之间时抛出。
    /// </exception>
    public TerrariaTime(int hour, int minute, int second = 0, MoonPhase? moonPhase = null)
    {
        if (hour is < 0 or >= 24)
            throw new ArgumentOutOfRangeException(nameof(hour), TerrariaTimeHelper.HourError);
        if (minute is < 0 or >= 60)
            throw new ArgumentOutOfRangeException(nameof(minute), TerrariaTimeHelper.MinuteError);
        if (second is < 0 or >= 60)
            throw new ArgumentOutOfRangeException(nameof(second), TerrariaTimeHelper.SecondError);

        Time = hour + minute / 60.0 + second / 3600.0;
        MoonPhase = moonPhase;
    }

    /// <summary>
    /// 使用 <see cref="DateTime"/> 的时间部分和可选的月相初始化 <see cref="TerrariaTime"/> 结构的新实例。
    /// </summary>
    /// <param name="dateTime">包含所需小时、分钟和秒的日期时间实例。</param>
    /// <param name="moonPhase">月相，如果为 <see langword="null"/> 则表示不指定月相。</param>
    public TerrariaTime(DateTime dateTime, MoonPhase? moonPhase = null) : this(dateTime.Hour, dateTime.Minute, dateTime.Second, moonPhase) { }

    /// <summary>
    /// 将泰拉瑞亚时间的字符串表示形式转换为其等效的 <see cref="TerrariaTime"/> 实例。
    /// </summary>
    /// <param name="s">要转换的字符串，格式必须为 "HH:MM" 或 "HH:MM:SS"。</param>
    /// <returns>解析得到的 <see cref="TerrariaTime"/> 实例。</returns>
    /// <exception cref="ArgumentException"><paramref name="s"/> 为 <see langword="null"/> 或空白。</exception>
    /// <exception cref="FormatException"><paramref name="s"/> 格式无效，或者小时、分钟、秒数值超出允许范围。</exception>
    public static TerrariaTime Parse(string s)
    {
        if (string.IsNullOrWhiteSpace(s))
            throw new ArgumentException(TOSharedData.StringEmptyError, nameof(s));
        string[] split = s.Split(':');
        if (split.Length is not (2 or 3))
            throw new FormatException(TerrariaTimeHelper.TerrariaTimeFormatError);
        if (!int.TryParse(split[0], out int hour) || hour is < 0 or >= 24)
            throw new FormatException(TerrariaTimeHelper.HourError);
        if (!int.TryParse(split[1], out int minute) || minute is < 0 or >= 60)
            throw new FormatException(TerrariaTimeHelper.MinuteError);
        int second = 0;
        if (split.Length == 3 && (!int.TryParse(split[2], out second) || second is < 0 or >= 60))
            throw new FormatException(TerrariaTimeHelper.SecondError);
        return new TerrariaTime(hour, minute, second);
    }

    /// <summary>
    /// 尝试将泰拉瑞亚时间的字符串表示形式转换为其等效的 <see cref="TerrariaTime"/> 实例，并返回指示转换是否成功的值。
    /// </summary>
    /// <param name="s">要转换的字符串。</param>
    /// <param name="result">当转换成功时，包含解析得到的 <see cref="TerrariaTime"/> 实例；否则为默认值。</param>
    /// <returns>如果转换成功，则为 <see langword="true"/>；否则为 <see langword="false"/>。</returns>
    public static bool TryParse(string s, out TerrariaTime result)
    {
        if (string.IsNullOrWhiteSpace(s))
        {
            result = default;
            return false;
        }
        string[] split = s.Split(':');
        if (split.Length is not (2 or 3))
        {
            result = default;
            return false;
        }
        if (!int.TryParse(split[0], out int hour) || hour is < 0 or >= 24)
        {
            result = default;
            return false;
        }
        if (!int.TryParse(split[1], out int minute) || minute is < 0 or >= 60)
        {
            result = default;
            return false;
        }
        int second = 0;
        if (split.Length == 3 && (!int.TryParse(split[2], out second) || second is < 0 or >= 60))
        {
            result = default;
            return false;
        }
        result = new TerrariaTime(hour, minute, second);
        return true;
    }

    /// <summary>
    /// 将当前实例解构为时间浮点值和月相。
    /// </summary>
    /// <param name="time">解构后的小时数。</param>
    /// <param name="moonPhase">解构后的月相。</param>
    public void Deconstruct(out double time, out MoonPhase? moonPhase)
    {
        time = Time;
        moonPhase = MoonPhase;
    }

    /// <summary>
    /// 将当前实例解构为总秒数和月相。
    /// </summary>
    /// <param name="totalSecond">解构后的总秒数。</param>
    /// <param name="moonPhase">解构后的月相。</param>
    public void Deconstruct(out int totalSecond, out MoonPhase? moonPhase)
    {
        totalSecond = TotalSeconds;
        moonPhase = MoonPhase;
    }

    /// <summary>
    /// 将当前实例解构为小时、分钟、秒和月相。
    /// </summary>
    /// <param name="hour">解构后的小时。</param>
    /// <param name="minute">解构后的分钟。</param>
    /// <param name="second">解构后的秒。</param>
    /// <param name="moonPhase">解构后的月相。</param>
    public void DeConstruct(out int hour, out int minute, out int second, out MoonPhase? moonPhase)
    {
        hour = Hour;
        minute = Minute;
        second = Second;
        moonPhase = MoonPhase;
    }

    /// <summary>
    /// 指示当前对象是否等于同一类型的另一个对象。
    /// </summary>
    /// <param name="other">要与此对象进行比较的 <see cref="TerrariaTime"/>。</param>
    /// <returns>如果小时、分钟、秒和月相均相等，则为 <see langword="true"/>；否则为 <see langword="false"/>。</returns>
    public bool Equals(TerrariaTime other) => Hour == other.Hour && Minute == other.Minute && Second == other.Second && MoonPhase == other.MoonPhase;

    /// <summary>
    /// 指示此实例与指定对象是否相等。
    /// </summary>
    /// <param name="obj">要与当前实例进行比较的对象。</param>
    /// <returns>如果 <paramref name="obj"/> 是 <see cref="TerrariaTime"/> 且其值相等，则为 <see langword="true"/>；否则为 <see langword="false"/>。</returns>
    public override bool Equals(object obj) => obj is TerrariaTime other && Equals(other);

    /// <summary>
    /// 确定两个 <see cref="TerrariaTime"/> 实例是否相等。
    /// </summary>
    /// <param name="left">要比较的第一个实例。</param>
    /// <param name="right">要比较的第二个实例。</param>
    /// <returns>如果两个实例相等，则为 <see langword="true"/>；否则为 <see langword="false"/>。</returns>
    public static bool operator ==(TerrariaTime left, TerrariaTime right) => left.Equals(right);

    /// <summary>
    /// 确定两个 <see cref="TerrariaTime"/> 实例是否不相等。
    /// </summary>
    /// <param name="left">要比较的第一个实例。</param>
    /// <param name="right">要比较的第二个实例。</param>
    /// <returns>如果两个实例不相等，则为 <see langword="true"/>；否则为 <see langword="false"/>。</returns>
    public static bool operator !=(TerrariaTime left, TerrariaTime right) => !(left == right);

    /// <summary>
    /// 返回此实例的哈希代码。
    /// </summary>
    /// <returns>基于小时、分钟、秒和月相结合的哈希代码。</returns>
    public override int GetHashCode() => HashCode.Combine(Hour, Minute, Second, MoonPhase);

    /// <summary>
    /// 获取格式为 "HH:MM:SS" 的时间字符串。
    /// </summary>
    /// <value>零填充的 24 小时制时间字符串。</value>
    public string TimeString => $"{Hour:D2}:{Minute:D2}:{Second:D2}";

    /// <summary>
    /// 返回表示当前对象的字符串。
    /// </summary>
    /// <returns>如果包含月相，则为 "HH:MM:SS, MoonPhase" 格式；否则仅返回 "HH:MM:SS"。</returns>
    public override string ToString() => MoonPhase.HasValue ? $"{TimeString}, {MoonPhase}" : TimeString;

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
    public void Apply()
    {
        (Main.dayTime, Main.time) = TotalSeconds switch
        {
            < 16200 => (false, 16200 + TotalSeconds),
            < 70200 => (true, TotalSeconds - 16200),
            _ => (false, TotalSeconds - 70200)
        };
        if (MoonPhase.HasValue)
            Main.moonPhase = (int)MoonPhase;
    }

    /// <summary>
    /// 获取表示当前时间时针方向的极坐标向量（长度为 1）。
    /// </summary>
    /// <value>一个 <see cref="PolarVector2"/> 向量，指向时钟表盘上时针对应的方向。</value>
    public PolarVector2 HourHand => PolarVector2.UnitClocks[12].RotatedBy(TOMathUtils.PiOver6 * (float)Time);

    /// <summary>
    /// 获取表示当前时间分针方向的极坐标向量（长度为 1）。
    /// </summary>
    /// <value>一个 <see cref="PolarVector2"/> 向量，指向时钟表盘上分针对应的方向。</value>
    public PolarVector2 MinuteHand => PolarVector2.UnitClocks[12].RotatedBy(TOMathUtils.PiOver30 * (float)(Time - Hour));

    /// <summary>
    /// 获取表示当前时间秒针方向的极坐标向量（长度为 1）。
    /// </summary>
    /// <value>一个 <see cref="PolarVector2"/> 向量，指向时钟表盘上秒针对应的方向。</value>
    public PolarVector2 SecondHand => PolarVector2.UnitClocks[12].RotatedBy(TOMathUtils.PiOver30 * Second / 1000f);

    /// <summary>
    /// 获取基于当前系统时间的 <see cref="TerrariaTime"/> 实例。
    /// </summary>
    /// <value>使用 <see cref="DateTime.Now"/> 创建的泰拉瑞亚时间。</value>
    public static TerrariaTime RealTime => new(DateTime.Now);
}

/// <summary>
/// 表示泰拉瑞亚中的计时器，以游戏刻（tick）为最小单位进行倒计时或正计时。
/// </summary>
/// <remarks>
/// <para>游戏刻是泰拉瑞亚中时间的最小单位，1 秒 = 60 游戏刻，1 分钟 = 3600 游戏刻。</para>
/// <para>此结构为可变值类型，实现了 <see cref="IEquatable{TerrariaTimer}"/> 和 <see cref="IComparable{TerrariaTimer}"/> 接口，支持相等性比较和排序。</para>
/// </remarks>
public struct TerrariaTimer : IEquatable<TerrariaTimer>, IComparable<TerrariaTimer>
{
    /// <summary>
    /// 获取或设置计时器的总游戏刻数。
    /// </summary>
    /// <value>非负整数，表示从零时刻开始经过的刻数。</value>
    /// <exception cref="ArgumentOutOfRangeException">当设置的值小于 0 时抛出。</exception>
    public int TotalTicks
    {
        get;
        private set
        {
            if (value < 0)
                throw new ArgumentOutOfRangeException(nameof(value), TerrariaTimeHelper.TotalTicksError);
            field = value;
        }
    }

    /// <summary>
    /// 获取或设置计时器的分钟部分。
    /// </summary>
    /// <value>非负整数分钟数。设置此属性将保留秒和刻的部分不变。</value>
    /// <exception cref="ArgumentOutOfRangeException">当设置的值小于 0 时抛出。</exception>
    public int Minute
    {
        readonly get => TotalTicks / 3600;
        set
        {
            if (value < 0)
                throw new ArgumentOutOfRangeException(nameof(value), TerrariaTimeHelper.MinuteError2);
            TotalTicks = value * 3600 + TotalTicks % 3600;
        }
    }

    /// <summary>
    /// 获取或设置计时器的秒钟部分。
    /// </summary>
    /// <value>介于 0 到 59 之间的秒数。</value>
    /// <exception cref="ArgumentOutOfRangeException">当设置的值小于 0 或大于 59 时抛出。</exception>
    public int Second
    {
        readonly get => TotalTicks / 60 % 60;
        set
        {
            if (value is < 0 or >= 60)
                throw new ArgumentOutOfRangeException(nameof(value), TerrariaTimeHelper.SecondError);
            TotalTicks = value * 60 + TotalTicks % 60;
        }
    }

    /// <summary>
    /// 获取或设置计时器的游戏刻部分。
    /// </summary>
    /// <value>介于 0 到 59 之间的刻数。</value>
    /// <exception cref="ArgumentOutOfRangeException">当设置的值小于 0 或大于 59 时抛出。</exception>
    public int Tick
    {
        readonly get => TotalTicks % 60;
        set
        {
            if (value is < 0 or >= 60)
                throw new ArgumentOutOfRangeException(nameof(value), TerrariaTimeHelper.TickError);
            TotalTicks = value + TotalTicks / 60 * 60;
        }
    }

    /// <summary>
    /// 初始化 <see cref="TerrariaTimer"/> 结构的新实例，其总游戏刻数为 0。
    /// </summary>
    public TerrariaTimer() : this(0) { }

    /// <summary>
    /// 使用指定的总游戏刻数初始化 <see cref="TerrariaTimer"/> 结构的新实例。
    /// </summary>
    /// <param name="totalTicks">总游戏刻数，必须为非负值。</param>
    /// <exception cref="ArgumentOutOfRangeException">当 <paramref name="totalTicks"/> 小于 0 时抛出。</exception>
    public TerrariaTimer(int totalTicks) => TotalTicks = totalTicks;

    /// <summary>
    /// 使用分钟、秒和游戏刻初始化 <see cref="TerrariaTimer"/> 结构的新实例。
    /// </summary>
    /// <param name="minute">分钟数（非负）。</param>
    /// <param name="second">秒数（0-59）。</param>
    /// <param name="tick">游戏刻数（0-59）。</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// 当 <paramref name="minute"/> 小于 0，
    /// 或 <paramref name="second"/> 不在 0 到 59 之间，
    /// 或 <paramref name="tick"/> 不在 0 到 59 之间时抛出。
    /// </exception>
    public TerrariaTimer(int minute = 0, int second = 0, int tick = 0)
    {
        Minute = minute;
        Second = second;
        Tick = tick;
    }

    /// <summary>
    /// 将计时器字符串表示形式转换为其等效的 <see cref="TerrariaTimer"/> 实例。
    /// </summary>
    /// <param name="s">要转换的字符串，格式必须为 "MM:SS" 或 "MM:SS:TT"。</param>
    /// <returns>解析得到的 <see cref="TerrariaTimer"/> 实例。</returns>
    /// <exception cref="ArgumentException"><paramref name="s"/> 为 <see langword="null"/> 或空白。</exception>
    /// <exception cref="FormatException"><paramref name="s"/> 格式无效，或者分钟、秒、刻数值超出允许范围。</exception>
    public static TerrariaTimer Parse(string s)
    {
        if (string.IsNullOrWhiteSpace(s))
            throw new ArgumentException(TOSharedData.StringEmptyError, nameof(s));
        string[] split = s.Split(':');
        if (split.Length is not (2 or 3))
            throw new FormatException(TerrariaTimeHelper.TerrariaTimerFormatError);
        if (!int.TryParse(split[0], out int minute) || minute is < 0 or >= 24)
            throw new FormatException(TerrariaTimeHelper.MinuteError2);
        if (!int.TryParse(split[1], out int second) || second is < 0 or >= 60)
            throw new FormatException(TerrariaTimeHelper.SecondError);
        int tick = 0;
        if (split.Length == 3 && (!int.TryParse(split[2], out tick) || tick is < 0 or >= 60))
            throw new FormatException(TerrariaTimeHelper.TickError);
        return new TerrariaTimer(minute, second, tick);
    }

    /// <summary>
    /// 尝试将计时器字符串表示形式转换为其等效的 <see cref="TerrariaTimer"/> 实例，并返回指示转换是否成功的值。
    /// </summary>
    /// <param name="s">要转换的字符串。</param>
    /// <param name="result">当转换成功时，包含解析得到的 <see cref="TerrariaTimer"/> 实例；否则为默认值。</param>
    /// <returns>如果转换成功，则为 <see langword="true"/>；否则为 <see langword="false"/>。</returns>
    public static bool TryParse(string s, out TerrariaTimer result)
    {
        if (string.IsNullOrWhiteSpace(s))
        {
            result = default;
            return false;
        }
        string[] split = s.Split(':');
        if (split.Length is not (2 or 3))
        {
            result = default;
            return false;
        }
        if (!int.TryParse(split[0], out int minute) || minute is < 0)
        {
            result = default;
            return false;
        }
        if (!int.TryParse(split[1], out int second) || second is < 0 or >= 60)
        {
            result = default;
            return false;
        }
        int tick = 0;
        if (split.Length == 3 && (!int.TryParse(split[2], out tick) || tick is < 0 or >= 60))
        {
            result = default;
            return false;
        }
        result = new TerrariaTimer(minute, second, tick);
        return true;
    }

    /// <summary>
    /// 将当前实例解构为总刻数、分钟、秒和刻。
    /// </summary>
    /// <param name="totalTicks">解构后的总游戏刻数。</param>
    /// <param name="minutes">解构后的分钟部分。</param>
    /// <param name="seconds">解构后的秒钟部分。</param>
    /// <param name="ticks">解构后的刻部分。</param>
    public readonly void Deconstruct(out int totalTicks, out int minutes, out int seconds, out int ticks)
    {
        totalTicks = TotalTicks;
        minutes = Minute;
        seconds = Second;
        ticks = Tick;
    }

    /// <summary>
    /// 指示此实例与指定对象是否相等。
    /// </summary>
    /// <param name="obj">要与当前实例进行比较的对象。</param>
    /// <returns>如果 <paramref name="obj"/> 是 <see cref="TerrariaTimer"/> 且总刻数相等，则为 <see langword="true"/>；否则为 <see langword="false"/>。</returns>
    public override readonly bool Equals([NotNullWhen(true)] object obj) => obj is TerrariaTimer other && Equals(other);

    /// <summary>
    /// 指示当前对象是否等于同一类型的另一个对象。
    /// </summary>
    /// <param name="other">要与此对象进行比较的 <see cref="TerrariaTimer"/>。</param>
    /// <returns>如果总刻数相等，则为 <see langword="true"/>；否则为 <see langword="false"/>。</returns>
    public readonly bool Equals(TerrariaTimer other) => TotalTicks == other.TotalTicks;

    /// <summary>
    /// 确定两个 <see cref="TerrariaTimer"/> 实例是否相等。
    /// </summary>
    /// <param name="left">要比较的第一个实例。</param>
    /// <param name="right">要比较的第二个实例。</param>
    /// <returns>如果两个实例相等，则为 <see langword="true"/>；否则为 <see langword="false"/>。</returns>
    public static bool operator ==(TerrariaTimer left, TerrariaTimer right) => left.Equals(right);

    /// <summary>
    /// 确定两个 <see cref="TerrariaTimer"/> 实例是否不相等。
    /// </summary>
    /// <param name="left">要比较的第一个实例。</param>
    /// <param name="right">要比较的第二个实例。</param>
    /// <returns>如果两个实例不相等，则为 <see langword="true"/>；否则为 <see langword="false"/>。</returns>
    public static bool operator !=(TerrariaTimer left, TerrariaTimer right) => !(left == right);

    /// <summary>
    /// 确定 <see cref="TerrariaTimer"/> 实例的总刻数是否与指定整数相等。
    /// </summary>
    /// <param name="left">要比较的计时器实例。</param>
    /// <param name="right">要比较的整数值。</param>
    /// <returns>如果计时器的 <see cref="TotalTicks"/> 等于 <paramref name="right"/>，则为 <see langword="true"/>；否则为 <see langword="false"/>。</returns>
    public static bool operator ==(TerrariaTimer left, int right) => left.TotalTicks == right;

    /// <summary>
    /// 确定 <see cref="TerrariaTimer"/> 实例的总刻数是否与指定整数不相等。
    /// </summary>
    /// <param name="left">要比较的计时器实例。</param>
    /// <param name="right">要比较的整数值。</param>
    /// <returns>如果计时器的 <see cref="TotalTicks"/> 不等于 <paramref name="right"/>，则为 <see langword="true"/>；否则为 <see langword="false"/>。</returns>
    public static bool operator !=(TerrariaTimer left, int right) => !(left == right);

    /// <summary>
    /// 返回此实例的哈希代码。
    /// </summary>
    /// <returns>基于总刻数的哈希代码。</returns>
    public override readonly int GetHashCode() => TotalTicks.GetHashCode();

    /// <summary>
    /// 将此实例与另一个 <see cref="TerrariaTimer"/> 实例进行比较，并返回一个整数指示其相对顺序。
    /// </summary>
    /// <param name="other">要比较的 <see cref="TerrariaTimer"/>。</param>
    /// <returns>一个值，指示实例的相对顺序：小于零表示此实例早于 <paramref name="other"/>；零表示相等；大于零表示此实例晚于 <paramref name="other"/>。</returns>
    public readonly int CompareTo(TerrariaTimer other) => TotalTicks.CompareTo(other.TotalTicks);

    /// <summary>
    /// 确定一个 <see cref="TerrariaTimer"/> 是否大于另一个 <see cref="TerrariaTimer"/>。
    /// </summary>
    public static bool operator >(TerrariaTimer left, TerrariaTimer right) => left.TotalTicks > right.TotalTicks;

    /// <summary>
    /// 确定一个 <see cref="TerrariaTimer"/> 是否小于另一个 <see cref="TerrariaTimer"/>。
    /// </summary>
    public static bool operator <(TerrariaTimer left, TerrariaTimer right) => left.TotalTicks < right.TotalTicks;

    /// <summary>
    /// 确定一个 <see cref="TerrariaTimer"/> 是否大于或等于另一个 <see cref="TerrariaTimer"/>。
    /// </summary>
    public static bool operator >=(TerrariaTimer left, TerrariaTimer right) => left.TotalTicks >= right.TotalTicks;

    /// <summary>
    /// 确定一个 <see cref="TerrariaTimer"/> 是否小于或等于另一个 <see cref="TerrariaTimer"/>。
    /// </summary>
    public static bool operator <=(TerrariaTimer left, TerrariaTimer right) => left.TotalTicks <= right.TotalTicks;

    /// <summary>
    /// 确定 <see cref="TerrariaTimer"/> 的总刻数是否大于指定整数。
    /// </summary>
    public static bool operator >(TerrariaTimer left, int right) => left.TotalTicks > right;

    /// <summary>
    /// 确定 <see cref="TerrariaTimer"/> 的总刻数是否小于指定整数。
    /// </summary>
    public static bool operator <(TerrariaTimer left, int right) => left.TotalTicks < right;

    /// <summary>
    /// 确定 <see cref="TerrariaTimer"/> 的总刻数是否大于或等于指定整数。
    /// </summary>
    public static bool operator >=(TerrariaTimer left, int right) => left.TotalTicks >= right;

    /// <summary>
    /// 确定 <see cref="TerrariaTimer"/> 的总刻数是否小于或等于指定整数。
    /// </summary>
    public static bool operator <=(TerrariaTimer left, int right) => left.TotalTicks <= right;

    /// <summary>
    /// 将 <see cref="TerrariaTimer"/> 实例的总刻数递增 1。
    /// </summary>
    /// <param name="timer">要递增的计时器。</param>
    /// <returns>一个新的 <see cref="TerrariaTimer"/>，其总刻数增加了 1。</returns>
    public static TerrariaTimer operator ++(TerrariaTimer timer) => new(++timer.TotalTicks);

    /// <summary>
    /// 将 <see cref="TerrariaTimer"/> 实例的总刻数递减 1。
    /// </summary>
    /// <param name="timer">要递减的计时器。</param>
    /// <returns>一个新的 <see cref="TerrariaTimer"/>，其总刻数减少了 1。</returns>
    public static TerrariaTimer operator --(TerrariaTimer timer) => new(--timer.TotalTicks);

    /// <summary>
    /// 将指定的刻数加到 <see cref="TerrariaTimer"/> 实例上。
    /// </summary>
    /// <param name="timer">计时器。</param>
    /// <param name="ticks">要增加的刻数。</param>
    /// <returns>一个新的 <see cref="TerrariaTimer"/>，表示增加后的总刻数。</returns>
    public static TerrariaTimer operator +(TerrariaTimer timer, int ticks) => new(timer.TotalTicks + ticks);

    /// <summary>
    /// 从 <see cref="TerrariaTimer"/> 实例中减去指定的刻数。
    /// </summary>
    /// <param name="timer">计时器。</param>
    /// <param name="ticks">要减去的刻数。</param>
    /// <returns>一个新的 <see cref="TerrariaTimer"/>，表示减去后的总刻数。</returns>
    public static TerrariaTimer operator -(TerrariaTimer timer, int ticks) => new(timer.TotalTicks - ticks);

    /// <summary>
    /// 将两个 <see cref="TerrariaTimer"/> 实例相加。
    /// </summary>
    /// <param name="left">第一个计时器。</param>
    /// <param name="right">第二个计时器。</param>
    /// <returns>一个新的 <see cref="TerrariaTimer"/>，其总刻数为两个操作数之和。</returns>
    public static TerrariaTimer operator +(TerrariaTimer left, TerrariaTimer right) => new(left.TotalTicks + right.TotalTicks);

    /// <summary>
    /// 从一个 <see cref="TerrariaTimer"/> 中减去另一个 <see cref="TerrariaTimer"/>。
    /// </summary>
    /// <param name="left">被减数计时器。</param>
    /// <param name="right">减数计时器。</param>
    /// <returns>一个新的 <see cref="TerrariaTimer"/>，其总刻数为两个操作数之差。</returns>
    public static TerrariaTimer operator -(TerrariaTimer left, TerrariaTimer right) => new(left.TotalTicks - right.TotalTicks);

    /// <summary>
    /// 将整数隐式转换为具有相应总刻数的 <see cref="TerrariaTimer"/>。
    /// </summary>
    /// <param name="totalTicks">总刻数。</param>
    public static implicit operator TerrariaTimer(int totalTicks) => new(totalTicks);

    /// <summary>
    /// 将分钟和秒的元组隐式转换为 <see cref="TerrariaTimer"/>。
    /// </summary>
    /// <param name="time">包含 (分钟, 秒) 的元组。</param>
    public static implicit operator TerrariaTimer((int minutes, int seconds) time) => new(time.minutes, time.seconds, 0);

    /// <summary>
    /// 将分钟、秒和刻的元组隐式转换为 <see cref="TerrariaTimer"/>。
    /// </summary>
    /// <param name="time">包含 (分钟, 秒, 刻) 的元组。</param>
    public static implicit operator TerrariaTimer((int minutes, int seconds, int ticks) time) => new(time.minutes, time.seconds, time.ticks);

    /// <summary>
    /// 返回表示当前对象的字符串，格式为 "MM:SS:TT"。
    /// </summary>
    /// <returns>零填充的分钟:秒:刻字符串。</returns>
    public override readonly string ToString() => $"{Minute}:{Second:D2}:{Tick:D2}";
}

/// <summary>
/// 提供与泰拉瑞亚时间相关的辅助方法和常量错误消息。
/// </summary>
public static class TerrariaTimeHelper
{
    /// <summary>
    /// 小时超出范围的错误消息。
    /// </summary>
    public const string HourError = "Hour must be between 0 and 23.";

    /// <summary>
    /// 分钟超出范围的错误消息（用于小时内的分钟）。
    /// </summary>
    public const string MinuteError = "Minute must be between 0 and 59.";

    /// <summary>
    /// 分钟为负数的错误消息（用于计时器中的分钟）。
    /// </summary>
    public const string MinuteError2 = "Minute must be non-negative.";

    /// <summary>
    /// 秒钟超出范围的错误消息。
    /// </summary>
    public const string SecondError = "Second must be between 0 and 59.";

    /// <summary>
    /// 游戏刻超出范围的错误消息。
    /// </summary>
    public const string TickError = "Tick must be between 0 and 59.";

    /// <summary>
    /// 总游戏刻数为负数的错误消息。
    /// </summary>
    public const string TotalTicksError = "Total ticks must be non-negative.";

    /// <summary>
    /// <see cref="TerrariaTime"/> 字符串格式错误的提示。
    /// </summary>
    public const string TerrariaTimeFormatError = "String must be in format 'HH:MM' or 'HH:MM:SS'.";

    /// <summary>
    /// <see cref="TerrariaTimer"/> 字符串格式错误的提示。
    /// </summary>
    public const string TerrariaTimerFormatError = "String must be in format 'MM:SS' or 'MM:SS:TT'.";
}