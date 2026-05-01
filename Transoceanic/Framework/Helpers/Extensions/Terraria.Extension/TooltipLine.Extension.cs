// Developed by ColdsUx

namespace Transoceanic.Framework.Helpers;

public static partial class TOExtensions
{
    extension(List<TooltipLine> tooltips)
    {
        /// <summary>
        /// 根据条件查找工具行。
        /// </summary>
        /// <param name="match">匹配条件委托。</param>
        /// <param name="index">输出匹配项的索引，未找到时为 -1。</param>
        /// <param name="tooltip">输出匹配的工具行实例，未找到时为 <c>null</c>。</param>
        /// <returns>如果找到则返回 <see langword="true"/>，否则返回 <see langword="false"/>。</returns>
        public bool TryFindTooltip(Func<TooltipLine, bool> match, out int index, out TooltipLine tooltip)
        {
            ArgumentNullException.ThrowIfNull(match);
            for (int i = 0; i < tooltips.Count; i++)
            {
                TooltipLine line = tooltips[i];
                if (match(line))
                {
                    index = i;
                    tooltip = line;
                    return true;
                }
            }
            index = -1;
            tooltip = null;
            return false;
        }

        /// <summary>
        /// 根据原版工具行名称查找工具行。
        /// </summary>
        /// <param name="name">原版工具行的名称（如 "Tooltip0"）。</param>
        /// <param name="index">输出索引。</param>
        /// <param name="tooltip">输出工具行实例。</param>
        /// <returns>如果找到则返回 <see langword="true"/>，否则返回 <see langword="false"/>。</returns>
        public bool TryFindVanillaTooltipByName(string name, out int index, out TooltipLine tooltip) =>
            tooltips.TryFindTooltip(l => l.Mod == "Terraria" && l.Name == name, out index, out tooltip);

        /// <summary>
        /// 修改匹配条件的第一个工具行。
        /// </summary>
        /// <param name="match">匹配条件。</param>
        /// <param name="action">对匹配的工具行执行的操作。</param>
        public void ModifyTooltip(Func<TooltipLine, bool> match, Action<TooltipLine> action)
        {
            ArgumentNullException.ThrowIfNull(action);
            if (tooltips.TryFindTooltip(match, out _, out TooltipLine tooltip))
                action(tooltip);
        }

        /// <summary>
        /// 修改指定名称的原版工具行。
        /// </summary>
        /// <param name="name">原版工具行名称。</param>
        /// <param name="action">修改操作。</param>
        public void ModifyVanillaTooltipByName(string name, Action<TooltipLine> action) =>
            tooltips.ModifyTooltip(l => l.Mod == "Terraria" && l.Name == name, action);

        /// <summary>
        /// 修改第 num 号原版工具行（例如 num=0 对应 "Tooltip0"）。
        /// </summary>
        /// <param name="num">工具行编号。</param>
        /// <param name="action">修改操作。</param>
        public void ModifyTooltipByNum(int num, Action<TooltipLine> action) =>
            tooltips.ModifyVanillaTooltipByName($"Tooltip{num}", action);
    }
}