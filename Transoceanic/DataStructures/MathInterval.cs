namespace Transoceanic.DataStructures;

/// <summary>
/// 表示一个数学区间。
/// </summary>
public struct MathInterval : IEquatable<MathInterval>
{
    /// <summary>
    /// 区间左端点。
    /// </summary>
    public float Left;

    /// <summary>
    /// 区间右端点。
    /// </summary>
    public float Right;

    /// <summary>
    /// 左端点是否包含在区间内。
    /// </summary>
    public bool LeftInclusive;

    /// <summary>
    /// 右端点是否包含在区间内。
    /// </summary>
    public bool RightInclusive;

    /// <summary>
    /// 判断区间是否为空。
    /// </summary>
    public bool IsEmpty;

    /// <summary>
    /// 创建一个数学区间。
    /// </summary>
    /// <param name="left">左端点。</param>
    /// <param name="right">右端点。</param>
    /// <param name="leftInclusive">左端点是否包含。</param>
    /// <param name="rightInclusive">右端点是否包含。</param>
    public MathInterval(float left, float right, bool leftInclusive = true, bool rightInclusive = true)
    {
        // 检查是否为特殊空区间
        if (float.IsNaN(left) || float.IsNaN(right) || (left >= right && (!leftInclusive || !rightInclusive)))
        {
            InitializeEmpty();
            return;
        }

        Left = left;
        Right = right;
        LeftInclusive = leftInclusive;
        RightInclusive = rightInclusive;
    }

    private void InitializeEmpty()
    {
        Left = float.NaN;
        Right = float.NaN;
        LeftInclusive = false;
        RightInclusive = false;
        IsEmpty = true;
    }

    /// <summary>
    /// 检查指定值是否在区间内。
    /// </summary>
    /// <param name="value">要检查的值。</param>
    /// <returns>如果值在区间内返回true，否则返回false。</returns>
    public readonly bool Contains(float value) =>
        !IsEmpty
        && (float.IsNegativeInfinity(Left) || LeftInclusive ? value >= Left : value > Left) //greater than left
        && (float.IsPositiveInfinity(Right) || RightInclusive ? value <= Right : value < Right); //smaller than right

    /// <summary>
    /// 返回区间的字符串表示形式
    /// </summary>
    /// <returns>区间字符串表示</returns>
    public override readonly string ToString()
    {
        if (IsEmpty)
            return "∅";

        string leftBracket = LeftInclusive ? "[" : "(";
        string rightBracket = RightInclusive ? "]" : ")";
        string leftValue = float.IsNegativeInfinity(Left) ? "-∞"
            : float.IsNaN(Left) ? "NaN" : Left.ToString();
        string rightValue = float.IsPositiveInfinity(Right) ? "∞"
            : float.IsNaN(Right) ? "NaN" : Right.ToString();

        return $"{leftBracket}{leftValue}, {rightValue}{rightBracket}";
    }

    public readonly bool Equals(MathInterval other) => IsEmpty == other.IsEmpty
        && Left == other.Left
        && Right == other.Right
        && LeftInclusive == other.LeftInclusive
        && RightInclusive == other.RightInclusive;
    public override readonly bool Equals(object obj) => obj is MathInterval other && Equals(other);
    public override readonly int GetHashCode() => HashCode.Combine(Left, Right, LeftInclusive, RightInclusive);
    public static bool operator ==(MathInterval left, MathInterval right) => left.Equals(right);
    public static bool operator !=(MathInterval left, MathInterval right) => !(left == right);

    /// <summary>
    /// 检查当前区间是否与另一个区间有重叠。
    /// </summary>
    /// <param name="b">另一个区间。</param>
    /// <returns>如果有重叠返回true，否则返回false。</returns>
    public static bool Overlap(MathInterval a, MathInterval b)
    {
        if (a.IsEmpty || b.IsEmpty)
            return false;

        //检查分离情况：a在b的左边
        if (a.Right < b.Left || (a.Right == b.Left && !(a.RightInclusive && b.LeftInclusive)))
            return false;

        //检查分离情况：a在b的右边
        if (a.Left > b.Right || (a.Left == b.Right && !(a.LeftInclusive && b.RightInclusive)))
            return false;

        return true;
    }

    public static bool OverlapOnClosedInterval(MathInterval a, MathInterval b) => !a.IsEmpty && !b.IsEmpty && a.Right >= b.Left && a.Left <= b.Right;

    /// <summary>
    /// 检查两个区间是否相邻（一个区间的右端点等于另一个区间的左端点）。
    /// </summary>
    /// <param name="b">另一个区间。</param>
    /// <returns>如果相邻返回true，否则返回false。</returns>
    public static bool IsAdjacent(MathInterval a, MathInterval b)
    {
        if (a.IsEmpty || b.IsEmpty)
            return false;

        //只有当一端是闭区间另一端是开区间时，才能合并
        //检查当前区间的右端点是否等于另一个区间的左端点
        if (a.Right == b.Left)
            return a.RightInclusive || b.LeftInclusive;

        //检查另一个区间的右端点是否等于当前区间的左端点
        if (b.Right == a.Left)
            return b.RightInclusive || a.LeftInclusive;

        return false;
    }

    /// <summary>
    /// 计算两个区间的凸包。
    /// </summary>
    /// <param name="a">第一个区间</param>
    /// <param name="b">第二个区间</param>
    /// <returns>合并后的区间（如果区间不连续，返回能包含两者的最小区间）。</returns>
    public static MathInterval operator |(MathInterval a, MathInterval b)
    {
        if (a.IsEmpty)
            return b;
        if (b.IsEmpty)
            return a;

        // 如果区间不重叠，返回能包含两者的最小区间
        if (!Overlap(a, b) && !IsAdjacent(a, b))
        {
            // 确定哪个区间在左边
            return a.Left < b.Left
                ? new MathInterval(a.Left, b.Right, a.LeftInclusive, b.RightInclusive)
                : new MathInterval(b.Left, a.Right, b.LeftInclusive, a.RightInclusive);
        }

        // 计算合并后的左右端点
        float newLeft = Math.Min(a.Left, b.Left);
        float newRight = Math.Max(a.Right, b.Right);

        // 确定新端点的开闭性
        bool newLeftInclusive = (newLeft == a.Left && a.LeftInclusive) || (newLeft == b.Left && b.LeftInclusive);
        bool newRightInclusive = (newRight == a.Right && a.RightInclusive) || (newRight == b.Right && b.RightInclusive);

        return new(newLeft, newRight, newLeftInclusive, newRightInclusive);
    }

    /// <summary>
    /// 计算两个区间的交集。
    /// </summary>
    /// <param name="a">第一个区间</param>
    /// <param name="b">第二个区间</param>
    /// <returns>两个区间的交集（如果没有交集，返回空区间）。</returns>
    public static MathInterval operator &(MathInterval a, MathInterval b)
    {
        if (a.IsEmpty || b.IsEmpty || !Overlap(a, b))
            return Empty;

        //计算交集的左右端点
        float newLeft = Math.Max(a.Left, b.Left);
        float newRight = Math.Min(a.Right, b.Right);

        //确定新端点的开闭性
        bool newLeftInclusive = a.Contains(newLeft) && b.Contains(newLeft) && ((newLeft == a.Left && a.LeftInclusive) || (newLeft == b.Left && b.LeftInclusive));
        bool newRightInclusive = a.Contains(newRight) && b.Contains(newRight) && ((newRight == a.Right && a.RightInclusive) || (newRight == b.Right && b.RightInclusive));

        //检查交集是否有效
        if (newLeft > newRight || (newLeft == newRight && (!newLeftInclusive || !newRightInclusive)))
            return Empty;

        return new(newLeft, newRight, newLeftInclusive, newRightInclusive);
    }


    public static MathInterval FromValues(params ReadOnlySpan<float> values)
    {
        float min = float.MinValue, max = float.MaxValue;
        foreach (float value in values)
        {
            min = Math.Min(min, value);
            max = Math.Max(max, value);
        }
        return new(min, max, true, true);
    }

    public static MathInterval Empty => new(float.NaN, float.NaN, false, false);
    public static MathInterval AllReals => new(float.NegativeInfinity, float.PositiveInfinity, false, false);
    public static MathInterval PositiveReals => new(0, float.PositiveInfinity, false, false);
    public static MathInterval NegativeReals => new(float.NegativeInfinity, 0, false, false);
    public static MathInterval NonNegativeReals => new(0, float.PositiveInfinity, true, false);
    public static MathInterval NonPositiveReals => new(float.NegativeInfinity, 0, false, true);
    public static MathInterval UnitInterval => new(0, 1, true, true);

    public readonly float GetRandomValue(UnifiedRandom rand) => rand.NextFloat(Left, Right);
}
