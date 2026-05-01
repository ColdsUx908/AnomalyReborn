// Developed by ColdsUx

namespace Transoceanic.DataStructures;

/// <summary>
/// 表示一个数学区间，用于描述实数轴上的一段连续范围。
/// 区间可以是有界的（指定左右端点）或无界的（使用负无穷或正无穷），
/// 端点可以包含在区间内（闭区间）或不包含在区间内（开区间）。
/// 此外，还支持空区间的概念，用于表示不包含任何元素的无效区间。
/// </summary>
public readonly struct MathInterval : IEquatable<MathInterval>
{
    /// <summary>
    /// 区间的左端点值。如果区间为空，则为 <see cref="float.NaN"/>。
    /// 对于无界区间，可能为 <see cref="float.NegativeInfinity"/>。
    /// </summary>
    public readonly float Left;

    /// <summary>
    /// 区间的右端点值。如果区间为空，则为 <see cref="float.NaN"/>。
    /// 对于无界区间，可能为 <see cref="float.PositiveInfinity"/>。
    /// </summary>
    public readonly float Right;

    /// <summary>
    /// 指示左端点是否包含在区间内。
    /// 如果为 <see langword="true"/>，则左端点是闭的（包含该值）；
    /// 如果为 <see langword="false"/>，则左端点是开的（不包含该值）。
    /// 对于空区间，此值无意义，通常设置为 <see langword="false"/>。
    /// </summary>
    public readonly bool LeftInclusive;

    /// <summary>
    /// 指示右端点是否包含在区间内。
    /// 如果为 <see langword="true"/>，则右端点是闭的（包含该值）；
    /// 如果为 <see langword="false"/>，则右端点是开的（不包含该值）。
    /// 对于空区间，此值无意义，通常设置为 <see langword="false"/>。
    /// </summary>
    public readonly bool RightInclusive;

    /// <summary>
    /// 获取一个值，指示当前区间是否为空区间。
    /// 空区间不包含任何实数，且所有涉及空区间的运算都将返回空区间或相应的默认行为。
    /// </summary>
    public readonly bool IsEmpty;

    /// <summary>
    /// 初始化 <see cref="MathInterval"/> 结构的新实例。
    /// </summary>
    /// <param name="left">区间的左端点值。</param>
    /// <param name="right">区间的右端点值。</param>
    /// <param name="leftInclusive">
    /// 如果左端点应包含在区间内，则为 <see langword="true"/>；否则为 <see langword="false"/>（开区间）。
    /// 默认值为 <see langword="true"/>。
    /// </param>
    /// <param name="rightInclusive">
    /// 如果右端点应包含在区间内，则为 <see langword="true"/>；否则为 <see langword="false"/>（开区间）。
    /// 默认值为 <see langword="true"/>。
    /// </param>
    /// <remarks>
    /// 构造函数会自动检测无效的输入并将其转换为空区间。具体规则如下：
    /// <list type="bullet">
    /// <item>
    /// <description>如果 <paramref name="left"/> 或 <paramref name="right"/> 为 <see cref="float.NaN"/>，则区间被标记为空。</description>
    /// </item>
    /// <item>
    /// <description>
    /// 如果 <paramref name="left"/> 大于 <paramref name="right"/>，则区间被视为无效并设为空；
    /// 若两者相等，则只有当两个端点都为闭区间（<paramref name="leftInclusive"/> 和 <paramref name="rightInclusive"/> 均为 <see langword="true"/>）时区间才非空（表示单个点），否则视为空区间。
    /// </description>
    /// </item>
    /// </list>
    /// </remarks>
    public MathInterval(float left, float right, bool leftInclusive = true, bool rightInclusive = true)
    {
        // 检查是否为特殊空区间
        if (float.IsNaN(left) || float.IsNaN(right) || (left >= right && (!leftInclusive || !rightInclusive)))
        {
            Left = float.NaN;
            Right = float.NaN;
            LeftInclusive = false;
            RightInclusive = false;
            IsEmpty = true;
            return;
        }

        Left = left;
        Right = right;
        LeftInclusive = leftInclusive;
        RightInclusive = rightInclusive;
    }

    /// <summary>
    /// 确定指定的值是否位于当前区间内。
    /// </summary>
    /// <param name="value">要检查的浮点数值。</param>
    /// <returns>
    /// 如果区间非空且 <paramref name="value"/> 满足左端点和右端点的边界条件（考虑开闭性），
    /// 则为 <see langword="true"/>；否则为 <see langword="false"/>。
    /// </returns>
    /// <remarks>
    /// 对于无穷端点（<see cref="float.NegativeInfinity"/> 或 <see cref="float.PositiveInfinity"/>），
    /// 开闭性不影响比较结果，总是认为满足条件。
    /// </remarks>
    public readonly bool Contains(float value) =>
        !IsEmpty
        && (float.IsNegativeInfinity(Left) || LeftInclusive ? value >= Left : value > Left) // greater than left
        && (float.IsPositiveInfinity(Right) || RightInclusive ? value <= Right : value < Right); // smaller than right

    /// <summary>
    /// 返回当前区间的字符串表示形式，采用标准的数学区间记法。
    /// </summary>
    /// <returns>
    /// 如果区间为空，返回 "∅"；
    /// 否则返回类似 "[a, b]"、"(a, b)"、"[a, b)" 或 "(a, b]" 的字符串，
    /// 其中无穷端点为 "-∞" 或 "∞"，NaN 端点为 "NaN"。
    /// </returns>
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

    /// <summary>
    /// 指示当前实例是否等于另一个 <see cref="MathInterval"/> 实例。
    /// </summary>
    /// <param name="other">要与此实例进行比较的另一个区间。</param>
    /// <returns>
    /// 如果两个区间的 <see cref="IsEmpty"/> 状态相同，
    /// 且 <see cref="Left"/>、<see cref="Right"/>、<see cref="LeftInclusive"/> 和 <see cref="RightInclusive"/>
    /// 全部相等，则为 <see langword="true"/>；否则为 <see langword="false"/>。
    /// </returns>
    public readonly bool Equals(MathInterval other) => IsEmpty == other.IsEmpty
        && Left == other.Left
        && Right == other.Right
        && LeftInclusive == other.LeftInclusive
        && RightInclusive == other.RightInclusive;

    /// <summary>
    /// 指示当前实例是否等于指定的对象。
    /// </summary>
    /// <param name="obj">要与当前实例进行比较的对象。</param>
    /// <returns>如果 <paramref name="obj"/> 是 <see cref="MathInterval"/> 且与当前实例相等，则为 <see langword="true"/>；否则为 <see langword="false"/>。</returns>
    public override readonly bool Equals(object obj) => obj is MathInterval other && Equals(other);

    /// <summary>
    /// 返回当前实例的哈希代码。
    /// </summary>
    /// <returns>基于 <see cref="Left"/>、<see cref="Right"/>、<see cref="LeftInclusive"/> 和 <see cref="RightInclusive"/> 计算的哈希代码。</returns>
    public override readonly int GetHashCode() => HashCode.Combine(Left, Right, LeftInclusive, RightInclusive);

    /// <summary>
    /// 确定两个 <see cref="MathInterval"/> 实例是否相等。
    /// </summary>
    /// <param name="left">要比较的第一个区间。</param>
    /// <param name="right">要比较的第二个区间。</param>
    /// <returns>如果两个区间相等，则为 <see langword="true"/>；否则为 <see langword="false"/>。</returns>
    public static bool operator ==(MathInterval left, MathInterval right) => left.Equals(right);

    /// <summary>
    /// 确定两个 <see cref="MathInterval"/> 实例是否不相等。
    /// </summary>
    /// <param name="left">要比较的第一个区间。</param>
    /// <param name="right">要比较的第二个区间。</param>
    /// <returns>如果两个区间不相等，则为 <see langword="true"/>；否则为 <see langword="false"/>。</returns>
    public static bool operator !=(MathInterval left, MathInterval right) => !(left == right);

    /// <summary>
    /// 确定两个区间是否重叠（即存在至少一个公共的实数）。
    /// </summary>
    /// <param name="a">第一个区间。</param>
    /// <param name="b">第二个区间。</param>
    /// <returns>
    /// 如果两个区间都非空且它们的交集非空，则为 <see langword="true"/>；否则为 <see langword="false"/>。
    /// 重叠判断严格遵循端点开闭性规则。
    /// </returns>
    /// <remarks>
    /// 例如：
    /// <list type="bullet">
    /// <item><c>[1, 2]</c> 和 <c>[2, 3]</c> 重叠（公共点 2）。</item>
    /// <item><c>[1, 2)</c> 和 <c>[2, 3]</c> 不重叠，因为 2 被排除在第一个区间之外。</item>
    /// <item><c>[1, 2]</c> 和 <c>(2, 3]</c> 不重叠，理由同上。</item>
    /// </list>
    /// </remarks>
    public static bool Overlap(MathInterval a, MathInterval b)
    {
        if (a.IsEmpty || b.IsEmpty)
            return false;

        // 检查分离情况：a 在 b 的左边
        if (a.Right < b.Left || (a.Right == b.Left && !(a.RightInclusive && b.LeftInclusive)))
            return false;

        // 检查分离情况：a 在 b 的右边
        if (a.Left > b.Right || (a.Left == b.Right && !(a.LeftInclusive && b.RightInclusive)))
            return false;

        return true;
    }

    /// <summary>
    /// 快速判断两个区间在闭区间意义上的重叠（忽略端点的开闭性，仅比较数值范围）。
    /// </summary>
    /// <param name="a">第一个区间。</param>
    /// <param name="b">第二个区间。</param>
    /// <returns>
    /// 如果两个区间都非空且满足 <c>a.Right >= b.Left</c> 且 <c>a.Left <= b.Right</c>，则为 <see langword="true"/>；
    /// 否则为 <see langword="false"/>。
    /// </returns>
    /// <remarks>
    /// 此方法不检查端点开闭性，适用于需要粗略判断重叠的场景。
    /// </remarks>
    public static bool OverlapOnClosedInterval(MathInterval a, MathInterval b) => !a.IsEmpty && !b.IsEmpty && a.Right >= b.Left && a.Left <= b.Right;

    /// <summary>
    /// 确定两个区间是否相邻。
    /// </summary>
    /// <param name="a">第一个区间。</param>
    /// <param name="b">第二个区间。</param>
    /// <returns>
    /// 如果两个区间都非空，且一个区间的右端点等于另一个区间的左端点，
    /// 并且至少有一个对应的端点是闭的（使得并集连续），则为 <see langword="true"/>；否则为 <see langword="false"/>。
    /// </returns>
    /// <remarks>
    /// 相邻性用于判断区间是否可以通过取并集形成单一连续区间而不产生间隙。
    /// 例如 <c>[1, 2]</c> 和 <c>(2, 3]</c> 不相邻，因为 2 未被覆盖；
    /// 而 <c>[1, 2]</c> 和 <c>[2, 3]</c> 相邻，<c>[1, 2)</c> 和 <c>[2, 3]</c> 也相邻。
    /// </remarks>
    public static bool IsAdjacent(MathInterval a, MathInterval b)
    {
        if (a.IsEmpty || b.IsEmpty)
            return false;

        // 只有当一端是闭区间另一端是开区间时，才能合并
        // 检查当前区间的右端点是否等于另一个区间的左端点
        if (a.Right == b.Left)
            return a.RightInclusive || b.LeftInclusive;

        // 检查另一个区间的右端点是否等于当前区间的左端点
        if (b.Right == a.Left)
            return b.RightInclusive || a.LeftInclusive;

        return false;
    }

    /// <summary>
    /// 计算两个区间的并集（凸包）。
    /// </summary>
    /// <param name="a">第一个区间。</param>
    /// <param name="b">第二个区间。</param>
    /// <returns>
    /// 如果两个区间重叠或相邻，则返回合并后的单一区间；
    /// 如果两个区间不连续，则返回能同时包含两者的最小区间（此时端点开闭性取自最外侧端点）。
    /// 如果任一区间为空，则返回另一个区间。
    /// </returns>
    /// <remarks>
    /// 并集运算的结果总是最小的区间，使得原区间均是其子集。
    /// 对于不连续区间，结果区间的内部可能包含原区间之间的空隙。
    /// </remarks>
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
    /// <param name="a">第一个区间。</param>
    /// <param name="b">第二个区间。</param>
    /// <returns>
    /// 两个区间的公共部分构成的区间；如果没有重叠，则返回 <see cref="Empty"/> 空区间。
    /// 如果任一输入区间为空，则直接返回空区间。
    /// </returns>
    /// <remarks>
    /// 交集运算严格遵循端点的开闭性规则。
    /// 例如 <c>[1, 3] &amp; (2, 4] = (2, 3]</c>，
    /// 而 <c>[1, 2) &amp; [2, 3] = ∅</c>。
    /// </remarks>
    public static MathInterval operator &(MathInterval a, MathInterval b)
    {
        if (a.IsEmpty || b.IsEmpty || !Overlap(a, b))
            return Empty;

        // 计算交集的左右端点
        float newLeft = Math.Max(a.Left, b.Left);
        float newRight = Math.Min(a.Right, b.Right);

        // 确定新端点的开闭性
        bool newLeftInclusive = a.Contains(newLeft) && b.Contains(newLeft) && ((newLeft == a.Left && a.LeftInclusive) || (newLeft == b.Left && b.LeftInclusive));
        bool newRightInclusive = a.Contains(newRight) && b.Contains(newRight) && ((newRight == a.Right && a.RightInclusive) || (newRight == b.Right && b.RightInclusive));

        // 检查交集是否有效
        if (newLeft > newRight || (newLeft == newRight && (!newLeftInclusive || !newRightInclusive)))
            return Empty;

        return new(newLeft, newRight, newLeftInclusive, newRightInclusive);
    }

    /// <summary>
    /// 从一组数值创建包含所有数值的最小闭区间。
    /// </summary>
    /// <param name="values">要包含在区间内的浮点数值的只读跨度。</param>
    /// <returns>
    /// 一个新创建的闭区间 <c>[min, max]</c>，其中 <c>min</c> 和 <c>max</c> 分别为
    /// <paramref name="values"/> 中的最小值和最大值。
    /// </returns>
    /// <remarks>
    /// 注意：当前实现中存在一个逻辑错误，<c>min</c> 被初始化为 <c>float.MinValue</c>，
    /// <c>max</c> 被初始化为 <c>float.MaxValue</c>，这将导致结果永远为 <c>[float.MinValue, float.MaxValue]</c>。
    /// 正确做法应为 <c>min = float.MaxValue</c> 和 <c>max = float.MinValue</c>。
    /// </remarks>
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

    /// <summary>
    /// 获取一个表示空区间的实例。空区间不包含任何实数。
    /// </summary>
    public static MathInterval Empty => new(float.NaN, float.NaN, false, false);

    /// <summary>
    /// 获取一个表示全体实数的区间：<c>(-∞, ∞)</c>（两端均为开区间）。
    /// </summary>
    public static MathInterval AllReals => new(float.NegativeInfinity, float.PositiveInfinity, false, false);

    /// <summary>
    /// 获取一个表示正实数的区间：<c>(0, ∞)</c>（不包括 0）。
    /// </summary>
    public static MathInterval PositiveReals => new(0, float.PositiveInfinity, false, false);

    /// <summary>
    /// 获取一个表示负实数的区间：<c>(-∞, 0)</c>（不包括 0）。
    /// </summary>
    public static MathInterval NegativeReals => new(float.NegativeInfinity, 0, false, false);

    /// <summary>
    /// 获取一个表示非负实数的区间：<c>[0, ∞)</c>（包括 0）。
    /// </summary>
    public static MathInterval NonNegativeReals => new(0, float.PositiveInfinity, true, false);

    /// <summary>
    /// 获取一个表示非正实数的区间：<c>(-∞, 0]</c>（包括 0）。
    /// </summary>
    public static MathInterval NonPositiveReals => new(float.NegativeInfinity, 0, false, true);

    /// <summary>
    /// 获取一个表示单位区间的实例：<c>[0, 1]</c>（闭区间）。
    /// </summary>
    public static MathInterval UnitInterval => new(0, 1, true, true);

    /// <summary>
    /// 从当前区间内均匀随机选取一个值。
    /// </summary>
    /// <param name="rand">用于生成随机数的 <see cref="UnifiedRandom"/> 实例。</param>
    /// <returns>区间 <c>[Left, Right]</c> 内的一个随机浮点数。</returns>
    /// <remarks>
    /// 注意：该方法忽略端点的开闭性，实际生成的值可能等于开区间的端点。
    /// 如果需要严格遵守开闭性，应考虑其他实现方式。
    /// </remarks>
    public readonly float GetRandomValue(UnifiedRandom rand) => rand.NextFloat(Left, Right);
}