namespace CalamityAnomalies.Visual;

public sealed class CAItemTooltipModifier : ItemTooltipModifier
{
    public const string CATooltip = "CATooltip";

    public ItemTooltipDictionary _tooltipDictionary;

    public int _nextCATooltipNum;
    public int _nextCATooltipIndex;

    public bool Valid => _nextCATooltipIndex != -1;

    public CAItemTooltipModifier(Item item, List<TooltipLine> tooltips) : base(item, tooltips) => UpdateCA();

    public void UpdateCA()
    {
        for (int i = _tooltips.Count - 1; i >= 0; i--)
        {
            TooltipLine line = _tooltips[i];
            if (line.Mod == CASharedData.ModName && line.Name.StartsWith(CATooltip) && int.TryParse(line.Name[CATooltip.Length..], out int index))
            {
                _nextCATooltipIndex = i + 1;
                _nextCATooltipNum = index + 1;
                return;
            }
            if (line.Mod == "Terraria" && line.Name.StartsWith(Tooltip))
            {
                _nextCATooltipIndex = i + 1;
                _nextCATooltipNum = 0;
                return;
            }
        }
        _nextCATooltipIndex = -1;
        return;
    }

    public void Update()
    {
        UpdateDictionary();
        UpdateCA();
    }

    public override CAItemTooltipModifier Modify(string mod, string name, string newText) => (CAItemTooltipModifier)base.Modify(mod, name, newText);

    public override CAItemTooltipModifier Modify(string mod, string name, string newText, Color newColor) => (CAItemTooltipModifier)base.Modify(mod, name, newText, newColor);

    public override CAItemTooltipModifier Modify(string mod, string name, Action<TooltipLine> action) => (CAItemTooltipModifier)base.Modify(mod, name, action);

    public override CAItemTooltipModifier ModifyTooltip(int num, string newText) => Modify(null, $"{Tooltip}{num}", newText);

    public override CAItemTooltipModifier ModifyTooltip(int num, string newText, Color newColor) => Modify(null, $"{Tooltip}{num}", newText, newColor);

    public override CAItemTooltipModifier ModifyTooltip(int num, Action<TooltipLine> action) => Modify(null, $"{Tooltip}{num}", action);

    public CAItemTooltipModifier ModifyWithCATweakColor(int num, string newText) => ModifyTooltip(num, newText, CASharedData.GetGradientColor(0.25f));

    public CAItemTooltipModifier ModifyWithCATweakColorDefault(ILocalizationPrefix localizationPrefixProvider, int num) =>
        ModifyWithCATweakColor(num, localizationPrefixProvider.GetTextValue($"{Tooltip}{num}"));

    public CAItemTooltipModifier ModifyWithCATweakColorDefault(ILocalizationPrefix localizationPrefixProvider, int num, params object[] args) =>
        ModifyWithCATweakColor(num, localizationPrefixProvider.GetTextValue($"{Tooltip}{num}", args));

    public CAItemTooltipModifier ClearAllCATooltips()
    {
        _tooltips.RemoveAll(line => line.Mod == CASharedData.ModName && line.Name.StartsWith(CATooltip));
        return this;
    }

    public CAItemTooltipModifier AddCATooltip(string text)
    {
        if (Valid)
        {
            _tooltips.Insert(_nextCATooltipIndex, CAUtils.CreateNewTooltipLine(_nextCATooltipNum, text));
            _nextCATooltipIndex++;
            _nextCATooltipNum++;
        }
        return this;
    }

    public CAItemTooltipModifier AddCATooltip(string text, Color color)
    {
        if (Valid)
        {
            _tooltips.Insert(_nextCATooltipIndex, CAUtils.CreateNewTooltipLine(_nextCATooltipNum, text, color));
            _nextCATooltipIndex++;
            _nextCATooltipNum++;
        }
        return this;
    }

    public CAItemTooltipModifier AddCATooltip(Action<TooltipLine> action)
    {
        if (Valid)
        {
            _tooltips.Insert(_nextCATooltipIndex, CAUtils.CreateNewTooltipLine(_nextCATooltipNum, action));
            _nextCATooltipIndex++;
            _nextCATooltipNum++;
        }
        return this;
    }

    public CAItemTooltipModifier AddCATooltipDefault(ILocalizationPrefix localizationPrefixProvider) => AddCATooltip(localizationPrefixProvider.GetTextValue($"{CATooltip}{_nextCATooltipIndex}"));

    public CAItemTooltipModifier AddCATooltipDefault(ILocalizationPrefix localizationPrefixProvider, params object[] args) => AddCATooltip(localizationPrefixProvider.GetTextValue($"{CATooltip}{_nextCATooltipIndex}", args));

    public CAItemTooltipModifier AddCATooltipDefault(ILocalizationPrefix localizationPrefixProvider, Color newColor) => AddCATooltip(localizationPrefixProvider.GetTextValue($"{CATooltip}{_nextCATooltipIndex}"), newColor);

    public CAItemTooltipModifier AddCATooltipDefault(ILocalizationPrefix localizationPrefixProvider, Color newColor, params object[] args) => AddCATooltip(localizationPrefixProvider.GetTextValue($"{CATooltip}{_nextCATooltipNum}", args), newColor);

    public CAItemTooltipModifier AddCATweakTooltip(string text) => AddCATooltip(text, CASharedData.GetGradientColor(0.25f));

    public CAItemTooltipModifier AddCATweakTooltip(Action<TooltipLine> action) =>
        AddCATooltip(l =>
        {
            l.OverrideColor = CASharedData.GetGradientColor(0.25f);
            action?.Invoke(l);
        });

    public CAItemTooltipModifier AddCATweakTooltipDefault(ILocalizationPrefix localizationPrefixProvider) => AddCATweakTooltip(localizationPrefixProvider.GetTextValue($"{CATooltip}{_nextCATooltipIndex}"));

    public CAItemTooltipModifier AddCATweakTooltipDefault(ILocalizationPrefix localizationPrefixProvider, params object[] args) => AddCATweakTooltip(localizationPrefixProvider.GetTextValue($"{CATooltip}{_nextCATooltipIndex}", args));

    public CAItemTooltipModifier AddExpendedDisplayLine() => AddCATooltip(Language.GetTextValue(CASharedData.CalamityModLocalizationPrefix + "Misc.ShiftToExpand"), new Color(0xBE, 0xBE, 0xBE));
}
