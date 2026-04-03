namespace Transoceanic.DataStructures;

public readonly struct TerrariaTime : IEquatable<TerrariaTime>
{
    /// <summary>
    /// 时间。
    /// </summary>
    public readonly double Time;

    /// <summary>
    /// 月相。
    /// </summary>
    public readonly MoonPhase? MoonPhase;

    public int TotalSeconds => (int)(Time * 3600);

    /// <summary>
    /// 小时数。
    /// <br/>范围为 [0, 24)。
    /// </summary>
    public int Hour => (int)Time;

    /// <summary>
    /// 分钟数。
    /// <br/>范围为 [0, 60)。
    /// </summary>
    public int Minute => (int)((Time - Hour) * 60.0);

    /// <summary>
    /// 秒数。
    /// <br/>范围为 [0, 60)。
    /// </summary>
    public int Second => (int)(((Time - Hour) * 60.0 - Minute) * 60.0);

    public TerrariaTime(double time, MoonPhase? moonPhase = null)
    {
        Time = time;
        MoonPhase = moonPhase;
    }

    public TerrariaTime(int totalSecond, MoonPhase? moonPhase = null) : this(totalSecond / 3600.0, moonPhase) { }

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

    public TerrariaTime(DateTime dateTime, MoonPhase? moonPhase = null) : this(dateTime.Hour, dateTime.Minute, dateTime.Second, moonPhase) { }

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

    public void Deconstruct(out double time, out MoonPhase? moonPhase)
    {
        time = Time;
        moonPhase = MoonPhase;
    }

    public void Deconstruct(out int totalSecond, out MoonPhase? moonPhase)
    {
        totalSecond = TotalSeconds;
        moonPhase = MoonPhase;
    }

    public void DeConstruct(out int hour, out int minute, out int second, out MoonPhase? moonPhase)
    {
        hour = Hour;
        minute = Minute;
        second = Second;
        moonPhase = MoonPhase;
    }

    public bool Equals(TerrariaTime other) => Hour == other.Hour && Minute == other.Minute && Second == other.Second && MoonPhase == other.MoonPhase;
    public override bool Equals(object obj) => obj is TerrariaTime other && Equals(other);
    public static bool operator ==(TerrariaTime left, TerrariaTime right) => left.Equals(right);
    public static bool operator !=(TerrariaTime left, TerrariaTime right) => !(left == right);
    public override int GetHashCode() => HashCode.Combine(Hour, Minute, Second, MoonPhase);

    public string TimeString => $"{Hour:D2}:{Minute:D2}:{Second:D2}";
    public override string ToString() => MoonPhase.HasValue ? $"{TimeString}, {MoonPhase}" : TimeString;

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

    public PolarVector2 HourHand => PolarVector2.UnitClocks[12].RotatedBy(TOMathUtils.PiOver6 * (float)Time);
    public PolarVector2 MinuteHand => PolarVector2.UnitClocks[12].RotatedBy(TOMathUtils.PiOver30 * (float)(Time - Hour));
    public PolarVector2 SecondHand => PolarVector2.UnitClocks[12].RotatedBy(TOMathUtils.PiOver30 * Second / 1000f);

    public static TerrariaTime RealTime => new(DateTime.Now);
}

public struct TerrariaTimer : IEquatable<TerrariaTimer>, IComparable<TerrariaTimer>
{
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

    public TerrariaTimer() : this(0) { }

    public TerrariaTimer(int totalTicks) => TotalTicks = totalTicks;

    public TerrariaTimer(int minute = 0, int second = 0, int tick = 0)
    {
        Minute = minute;
        Second = second;
        Tick = tick;
    }

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

    public readonly void Deconstruct(out int totalTicks, out int minutes, out int seconds, out int ticks)
    {
        totalTicks = TotalTicks;
        minutes = Minute;
        seconds = Second;
        ticks = Tick;
    }

    public override readonly bool Equals([NotNullWhen(true)] object obj) => obj is TerrariaTimer other && Equals(other);
    public readonly bool Equals(TerrariaTimer other) => TotalTicks == other.TotalTicks;
    public static bool operator ==(TerrariaTimer left, TerrariaTimer right) => left.Equals(right);
    public static bool operator !=(TerrariaTimer left, TerrariaTimer right) => !(left == right);
    public static bool operator ==(TerrariaTimer left, int right) => left.TotalTicks == right;
    public static bool operator !=(TerrariaTimer left, int right) => !(left == right);
    public override readonly int GetHashCode() => TotalTicks.GetHashCode();

    public readonly int CompareTo(TerrariaTimer other) => TotalTicks.CompareTo(other.TotalTicks);
    public static bool operator >(TerrariaTimer left, TerrariaTimer right) => left.TotalTicks > right.TotalTicks;
    public static bool operator <(TerrariaTimer left, TerrariaTimer right) => left.TotalTicks < right.TotalTicks;
    public static bool operator >=(TerrariaTimer left, TerrariaTimer right) => left.TotalTicks >= right.TotalTicks;
    public static bool operator <=(TerrariaTimer left, TerrariaTimer right) => left.TotalTicks <= right.TotalTicks;
    public static bool operator >(TerrariaTimer left, int right) => left.TotalTicks > right;
    public static bool operator <(TerrariaTimer left, int right) => left.TotalTicks < right;
    public static bool operator >=(TerrariaTimer left, int right) => left.TotalTicks >= right;
    public static bool operator <=(TerrariaTimer left, int right) => left.TotalTicks <= right;

    public static TerrariaTimer operator ++(TerrariaTimer timer) => new(++timer.TotalTicks);
    public static TerrariaTimer operator --(TerrariaTimer timer) => new(--timer.TotalTicks);
    public static TerrariaTimer operator +(TerrariaTimer timer, int ticks) => new(timer.TotalTicks + ticks);
    public static TerrariaTimer operator -(TerrariaTimer timer, int ticks) => new(timer.TotalTicks - ticks);
    public static TerrariaTimer operator +(TerrariaTimer left, TerrariaTimer right) => new(left.TotalTicks + right.TotalTicks);
    public static TerrariaTimer operator -(TerrariaTimer left, TerrariaTimer right) => new(left.TotalTicks - right.TotalTicks);

    public static implicit operator TerrariaTimer(int totalTicks) => new(totalTicks);
    public static implicit operator TerrariaTimer((int minutes, int seconds) time) => new(time.minutes, time.seconds, 0);
    public static implicit operator TerrariaTimer((int minutes, int seconds, int ticks) time) => new(time.minutes, time.seconds, time.ticks);

    public override readonly string ToString() => $"{Minute}:{Second:D2}:{Tick:D2}";
}

public static class TerrariaTimeHelper
{
    public const string HourError = "Hour must be between 0 and 23.";
    public const string MinuteError = "Minute must be between 0 and 59.";
    public const string MinuteError2 = "Minute must be non-negative.";
    public const string SecondError = "Second must be between 0 and 59.";
    public const string TickError = "Tick must be between 0 and 59.";
    public const string TotalTicksError = "Total ticks must be non-negative.";
    public const string TerrariaTimeFormatError = "String must be in format 'HH:MM' or 'HH:MM:SS'.";
    public const string TerrariaTimerFormatError = "String must be in format 'MM:SS' or 'MM:SS:TT'.";
}