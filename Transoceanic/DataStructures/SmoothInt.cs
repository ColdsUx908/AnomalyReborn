namespace Transoceanic.DataStructures;

public struct SmoothInt : IEquatable<SmoothInt>
{
    public int LastOn;
    public int LastOff;

    public readonly int GetValue(int actual, int max, bool equal = false) => Math.Clamp(
        LastOn > LastOff || (equal && LastOn == LastOff) ? actual - LastOn : max - actual + LastOff,
        0, max);

    public readonly bool Equals(SmoothInt other) => LastOn == other.LastOn && LastOff == other.LastOff;
    public override readonly bool Equals(object obj) => obj is SmoothInt other && Equals(other);
    public override readonly int GetHashCode() => HashCode.Combine(LastOn, LastOff);
    public static bool operator ==(SmoothInt left, SmoothInt right) => left.Equals(right);
    public static bool operator !=(SmoothInt left, SmoothInt right) => !(left == right);
}
