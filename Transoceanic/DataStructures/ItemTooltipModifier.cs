namespace Transoceanic.Framework;

public class ItemTooltipDictionary
{
    public const string Tooltip = "Tooltip";

    public readonly Item _item;
    public readonly List<TooltipLine> _tooltips;
    public Dictionary<(string Mod, string Name), (int Index, TooltipLine Line)> _dictionary;

    public ItemTooltipDictionary(Item item, List<TooltipLine> tooltips)
    {
        _item = item;
        _tooltips = tooltips;
        UpdateDictionary();
    }

    public void UpdateDictionary()
    {
        _dictionary = [];
        for (int i = 0; i < _tooltips.Count; i++)
        {
            TooltipLine line = _tooltips[i];
            _dictionary[(line.Mod, line.Name)] = (i, line);
        }
    }

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

public partial class ItemTooltipModifier : ItemTooltipDictionary
{
    /// <summary>
    /// Tooltip正则表达式。
    /// </summary>
    /// <inheritdoc cref="GetTooltipRegex"/>
    public static readonly Regex _tooltipRegex = GetTooltipRegex();
    [GeneratedRegex("""^Tooltip(\d+)$""")]
    private static partial Regex GetTooltipRegex();

    public ItemTooltipModifier(Item item, List<TooltipLine> tooltips) : base(item, tooltips) { }

    public virtual ItemTooltipModifier Modify(string mod, string name, string newText)
    {
        if (TryGet(mod, name, out _, out TooltipLine line))
            line.Text = newText;
        return this;
    }

    public virtual ItemTooltipModifier Modify(string mod, string name, string newText, Color newColor)
    {
        if (TryGet(mod, name, out _, out TooltipLine line))
        {
            line.Text = newText;
            line.OverrideColor = newColor;
        }
        return this;
    }

    public virtual ItemTooltipModifier Modify(string mod, string name, Action<TooltipLine> action)
    {
        if (TryGet(mod, name, out _, out TooltipLine line))
            action(line);
        return this;
    }

    public virtual ItemTooltipModifier ModifyTooltip(int num, string newText) => Modify(null, $"{Tooltip}{num}", newText);

    public virtual ItemTooltipModifier ModifyTooltip(int num, string newText, Color newColor) => Modify(null, $"{Tooltip}{num}", newText, newColor);

    public virtual ItemTooltipModifier ModifyTooltip(int num, Action<TooltipLine> action) => Modify(null, $"{Tooltip}{num}", action);
}