// Developed by ColdsUx

namespace Transoceanic.Framework.Helpers;

public static partial class TOExtensions
{
    private static class Color_Extension
    {
        /// <summary>
        /// 用于生成彩虹色插值的预设颜色列表，包含红、绿、蓝、红四个颜色节点。
        /// </summary>
        public static readonly List<Color> _rainbowColors = [Color.Red, Color.Lime, Color.Blue, Color.Red];
    }

    extension(Color color)
    {
        /// <summary>
        /// 将颜色转换为六位十六进制颜色代码字符串。
        /// </summary>
        /// <returns>格式为 "RRGGBB" 的十六进制字符串，每个通道占用两位大写十六进制数字。</returns>
        public string ToHexCode() => $"{color.R:X2}{color.G:X2}{color.B:X2}";

        /// <summary>
        /// 使用当前颜色格式化输入字符串，生成带有颜色标记的文本。
        /// </summary>
        /// <param name="input">要格式化的原始字符串。</param>
        /// <returns>格式为 "[c/十六进制颜色码:输入字符串]" 的带颜色标记文本。</returns>
        public string FormatString(string input) => $"[c/{color.ToHexCode()}:{input}]";
    }

    extension(Color)
    {
        /// <summary>
        /// 获取用于彩虹色插值的预设颜色列表。
        /// </summary>
        public static List<Color> RainbowColors => Color_Extension._rainbowColors;

        /// <summary>
        /// 在彩虹色列表的整个范围内随机插值，获取一个随机彩虹色。
        /// </summary>
        /// <returns>根据随机浮点数在彩虹色之间线性插值得到的颜色。</returns>
        public static Color GetRandomRainbowColor() => Color.LerpMany(Color.RainbowColors, Main.rand.NextFloat());

        /// <summary>
        /// 在彩虹色列表的指定插值范围内随机获取一个彩虹色。
        /// </summary>
        /// <param name="minValue">插值比率的最小值，范围通常为 [0, 1]。</param>
        /// <param name="maxValue">插值比率的最大值，范围通常为 [0, 1]。</param>
        /// <returns>在指定比率区间内随机插值得到的颜色。</returns>
        public static Color GetRandomRainbowColor(float minValue, float maxValue) => Color.LerpMany(Color.RainbowColors, Main.rand.NextFloat(minValue, maxValue));

        /// <summary>
        /// 在多个颜色之间进行线性插值。
        /// </summary>
        /// <param name="colors">包含至少一个颜色的列表。若为 <see langword="null"/> 或空列表将引发异常。</param>
        /// <param name="amount">插值比率，范围 [0, 1]。0 对应第一个颜色，1 对应最后一个颜色。</param>
        /// <returns>插值后的颜色。</returns>
        /// <exception cref="ArgumentException">当 <paramref name="colors"/> 为 <see langword="null"/> 或空列表时抛出。</exception>
        public static Color LerpMany(IList<Color> colors, float amount)
        {
            ArgumentException.ThrowIfNullOrEmpty(colors);

            switch (colors.Count)
            {
                case 1:
                    return colors[0];
                case 2:
                    return Color.Lerp(colors[0], colors[1], amount);
                default:
                    if (amount <= 0f)
                        return colors[0];
                    if (amount >= 1f)
                        return colors[^1];
                    (int index, float localRatio) = TOMathUtils.SplitFloat(amount * (colors.Count - 1));
                    return Color.Lerp(colors[index], colors[index + 1], localRatio);
            }
        }
    }
}