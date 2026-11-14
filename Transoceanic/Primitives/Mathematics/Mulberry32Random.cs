namespace Transoceanic.Primitives.Mathematics;

/// <summary>使用固定 32 位状态的 Mulberry32 序列，适用于可重现的视觉配方；不是密码学随机源。</summary>
/// <param name="seed">初始状态；零也是有效种子。</param>
/// <remarks>结构体复制会复制序列位置。每次调用消耗一个状态，不依赖游戏或系统随机源。</remarks>
public struct Mulberry32Random(uint seed)
{
    private uint _State = seed;

    /// <summary>返回下一个单精度样本；整数结果除以 2^32 后转换为 float，舍入可能得到 1。</summary>
    public float NextSingle()
    {
        unchecked
        {
            uint value = _State += 0x6D2B79F5;
            value = (value ^ (value >> 15)) * (1u | value);
            value ^= value + (value ^ (value >> 7)) * (61u | value);
            return (float)((value ^ (value >> 14)) / 4294967296.0);
        }
    }
}
