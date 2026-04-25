// Designed by ColdsUx

namespace Transoceanic.Framework;

/// <summary>
/// 管理物品提示信息的字典容器，用于存储、索引和检索 <see cref="TooltipLine"/> 条目。
/// </summary>
public class ItemTooltipDictionary
{
    /// <summary>
    /// 标准提示行名称的前缀，用于组合生成形如 "Tooltip0"、"Tooltip1" 的行标识。
    /// </summary>
    public const string Tooltip = "Tooltip";

    /// <summary>
    /// 与此字典关联的 <see cref="Item"/> 实例。
    /// </summary>
    public readonly Item _item;

    /// <summary>
    /// 当前存储的所有提示行列表。
    /// </summary>
    public readonly List<TooltipLine> _tooltips;

    /// <summary>
    /// 以 (Mod, Name) 组合为键的快速查找字典，值为该提示行在 <see cref="_tooltips"/> 中的索引及行的引用。
    /// </summary>
    public Dictionary<(string Mod, string Name), (int Index, TooltipLine Line)> _dictionary;

    /// <summary>
    /// 初始化 <see cref="ItemTooltipDictionary"/> 的新实例，并基于传入的提示行列表构建内部索引字典。
    /// </summary>
    /// <param name="item">关联的物品实例。</param>
    /// <param name="tooltips">需要管理的提示行列表。</param>
    public ItemTooltipDictionary(Item item, List<TooltipLine> tooltips)
    {
        _item = item;
        _tooltips = tooltips;
        UpdateDictionary();
    }

    /// <summary>
    /// 重新构建内部索引字典 <see cref="_dictionary"/>。通常在 <see cref="_tooltips"/> 发生变化后调用。
    /// </summary>
    public void UpdateDictionary()
    {
        _dictionary = [];
        for (int i = 0; i < _tooltips.Count; i++)
        {
            TooltipLine line = _tooltips[i];
            _dictionary[(line.Mod, line.Name)] = (i, line);
        }
    }

    /// <summary>
    /// 尝试根据 Mod 和 Name 获取对应的提示行及其在列表中的索引。
    /// </summary>
    /// <param name="mod">提示行所属的 Mod 标识。若为 <c>null</c>，则默认视为 "Terraria"。</param>
    /// <param name="name">提示行的名称。</param>
    /// <param name="index">当方法返回 <see langword="true"/> 时，包含该提示行在 <see cref="_tooltips"/> 中的索引；否则为 -1。</param>
    /// <param name="line">当方法返回 <see langword="true"/> 时，包含找到的 <see cref="TooltipLine"/> 实例；否则为 <c>null</c>。</param>
    /// <returns>如果找到对应的提示行，则为 <see langword="true"/>；否则为 <see langword="false"/>。</returns>
    public bool TryGet(string mod, string name, out int index, out TooltipLine line)
    {
        if (_dictionary.TryGetValue((mod ?? "Terraria", name), out (int Index, TooltipLine Line) value))
        {
            (index, line) = value;
            return true;
        }
        index = -1;
        line = null;
        return false;
    }
}

/// <summary>
/// 提供对物品提示行进行便捷修改的扩展类。继承自 <see cref="ItemTooltipDictionary"/>，
/// 并提供了多个链式调用的 <c>Modify</c> 方法，允许直接修改特定提示行的文本、颜色或执行自定义操作。
/// </summary>
public partial class ItemTooltipModifier : ItemTooltipDictionary
{
    /// <summary>
    /// 用于匹配标准工具提示行名称（如 "Tooltip0"、"Tooltip1"）的正则表达式。
    /// </summary>
    /// <inheritdoc cref="GetTooltipRegex"/>
    public static readonly Regex _tooltipRegex = GetTooltipRegex();

    /// <summary>
    /// 生成匹配 "Tooltip" 后跟数字的正则表达式。模式为 <c>^Tooltip(\d+)$</c>。
    /// </summary>
    /// <returns>编译好的 <see cref="Regex"/> 实例。</returns>
    [GeneratedRegex("""^Tooltip(\d+)$""")]
    private static partial Regex GetTooltipRegex();

    /// <summary>
    /// 初始化 <see cref="ItemTooltipModifier"/> 的新实例。
    /// </summary>
    /// <param name="item">关联的物品实例。</param>
    /// <param name="tooltips">需要管理的提示行列表。</param>
    public ItemTooltipModifier(Item item, List<TooltipLine> tooltips) : base(item, tooltips) { }

    /// <summary>
    /// 修改指定 Mod 和 Name 对应的提示行文本内容。
    /// </summary>
    /// <param name="mod">提示行所属的 Mod 标识。若为 <c>null</c>，则默认视为 "Terraria"。</param>
    /// <param name="name">提示行的名称。</param>
    /// <param name="newText">新的文本内容。</param>
    /// <returns>当前 <see cref="ItemTooltipModifier"/> 实例，支持链式调用。</returns>
    public virtual ItemTooltipModifier Modify(string mod, string name, string newText)
    {
        if (TryGet(mod, name, out _, out TooltipLine line))
            line.Text = newText;
        return this;
    }

    /// <summary>
    /// 修改指定 Mod 和 Name 对应的提示行文本内容及覆盖颜色。
    /// </summary>
    /// <param name="mod">提示行所属的 Mod 标识。若为 <c>null</c>，则默认视为 "Terraria"。</param>
    /// <param name="name">提示行的名称。</param>
    /// <param name="newText">新的文本内容。</param>
    /// <param name="newColor">新的覆盖颜色。将设置 <see cref="TooltipLine.OverrideColor"/> 属性。</param>
    /// <returns>当前 <see cref="ItemTooltipModifier"/> 实例，支持链式调用。</returns>
    public virtual ItemTooltipModifier Modify(string mod, string name, string newText, Color newColor)
    {
        if (TryGet(mod, name, out _, out TooltipLine line))
        {
            line.Text = newText;
            line.OverrideColor = newColor;
        }
        return this;
    }

    /// <summary>
    /// 对指定 Mod 和 Name 对应的提示行执行自定义操作。
    /// </summary>
    /// <param name="mod">提示行所属的 Mod 标识。若为 <c>null</c>，则默认视为 "Terraria"。</param>
    /// <param name="name">提示行的名称。</param>
    /// <param name="action">要应用于找到的 <see cref="TooltipLine"/> 的委托操作。</param>
    /// <returns>当前 <see cref="ItemTooltipModifier"/> 实例，支持链式调用。</returns>
    public virtual ItemTooltipModifier Modify(string mod, string name, Action<TooltipLine> action)
    {
        if (TryGet(mod, name, out _, out TooltipLine line))
            action(line);
        return this;
    }

    /// <summary>
    /// 修改指定序号的工具提示行的文本内容。
    /// 该方法是 <see cref="Modify(string, string, string)"/> 的快捷方式，
    /// 自动将 Mod 视为 <c>null</c>（即 "Terraria"），并将 Name 组合为 "Tooltip" + 序号。
    /// </summary>
    /// <param name="num">工具提示的序号（例如 0、1、2）。</param>
    /// <param name="newText">新的文本内容。</param>
    /// <returns>当前 <see cref="ItemTooltipModifier"/> 实例，支持链式调用。</returns>
    public virtual ItemTooltipModifier ModifyTooltip(int num, string newText) => Modify(null, $"{Tooltip}{num}", newText);

    /// <summary>
    /// 修改指定序号的工具提示行的文本内容及覆盖颜色。
    /// 该方法是 <see cref="Modify(string, string, string, Color)"/> 的快捷方式，
    /// 自动将 Mod 视为 <c>null</c>（即 "Terraria"），并将 Name 组合为 "Tooltip" + 序号。
    /// </summary>
    /// <param name="num">工具提示的序号（例如 0、1、2）。</param>
    /// <param name="newText">新的文本内容。</param>
    /// <param name="newColor">新的覆盖颜色。将设置 <see cref="TooltipLine.OverrideColor"/> 属性。</param>
    /// <returns>当前 <see cref="ItemTooltipModifier"/> 实例，支持链式调用。</returns>
    public virtual ItemTooltipModifier ModifyTooltip(int num, string newText, Color newColor) => Modify(null, $"{Tooltip}{num}", newText, newColor);

    /// <summary>
    /// 对指定序号的工具提示行执行自定义操作。
    /// 该方法是 <see cref="Modify(string, string, Action{TooltipLine})"/> 的快捷方式，
    /// 自动将 Mod 视为 <c>null</c>（即 "Terraria"），并将 Name 组合为 "Tooltip" + 序号。
    /// </summary>
    /// <param name="num">工具提示的序号（例如 0、1、2）。</param>
    /// <param name="action">要应用于找到的 <see cref="TooltipLine"/> 的委托操作。</param>
    /// <returns>当前 <see cref="ItemTooltipModifier"/> 实例，支持链式调用。</returns>
    public virtual ItemTooltipModifier ModifyTooltip(int num, Action<TooltipLine> action) => Modify(null, $"{Tooltip}{num}", action);
}