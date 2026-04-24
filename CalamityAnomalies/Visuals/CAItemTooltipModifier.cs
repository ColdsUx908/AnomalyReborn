namespace CalamityAnomalies.Visuals;

/// <summary>
/// 针对 Calamity Anomalies 模组扩展的 <see cref="ItemTooltipModifier"/> 实现，
/// 提供对自定义提示行（以 "CATooltip" 为前缀）的插入、删除和批量修改功能。
/// 支持链式调用，并内置模组特有的渐变色样式。
/// </summary>
public sealed class CAItemTooltipModifier : ItemTooltipModifier
{
    /// <summary>
    /// 本模组自定义提示行的名称前缀，用于生成 "CATooltip0"、"CATooltip1" 等标识。
    /// </summary>
    public const string CATooltip = "CATooltip";

    /// <summary>
    /// （保留字段）指向关联的 <see cref="ItemTooltipDictionary"/> 实例，当前未使用。
    /// </summary>
    public ItemTooltipDictionary _tooltipDictionary;

    /// <summary>
    /// 下一个可用的 CA 提示行序号（用于命名）。
    /// </summary>
    public int _nextCATooltipNum;

    /// <summary>
    /// 下一个 CA 提示行应当插入的列表索引位置。
    /// </summary>
    public int _nextCATooltipIndex;

    /// <summary>
    /// 获取一个值，该值指示当前实例是否处于有效状态（即已成功定位到插入位置）。
    /// </summary>
    public bool Valid => _nextCATooltipIndex != -1;

    /// <summary>
    /// 初始化 <see cref="CAItemTooltipModifier"/> 的新实例，并立即执行一次状态更新。
    /// </summary>
    /// <param name="item">关联的物品实例。</param>
    /// <param name="tooltips">需要管理的提示行列表。</param>
    public CAItemTooltipModifier(Item item, List<TooltipLine> tooltips) : base(item, tooltips) => UpdateCA();

    /// <summary>
    /// 扫描当前提示行列表，确定下一个 CA 提示行的插入位置和序号。
    /// 从列表末尾向前遍历，寻找最近的 "CATooltip" 或标准 "Tooltip" 行作为定位锚点。
    /// 若未找到任何锚点，则 <see cref="_nextCATooltipIndex"/> 被设置为 -1。
    /// </summary>
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

    /// <summary>
    /// 更新内部索引字典并刷新 CA 提示行的插入状态。
    /// 通常在外部修改提示行列表后调用，以保持一致性。
    /// </summary>
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

    /// <summary>
    /// 使用模组定义的渐变色（强度 0.25f）修改指定序号的标准提示行文本。
    /// </summary>
    /// <param name="num">提示行序号。</param>
    /// <param name="newText">新的文本内容。</param>
    /// <returns>当前 <see cref="CAItemTooltipModifier"/> 实例，支持链式调用。</returns>
    public CAItemTooltipModifier ModifyWithCATweakColor(int num, string newText) => ModifyTooltip(num, newText, CASharedData.GetGradientColor(0.25f));

    /// <summary>
    /// 通过本地化提供程序获取默认文本，并以模组渐变色修改指定序号的标准提示行。
    /// 文本键名格式为 "Tooltip{num}"。
    /// </summary>
    /// <param name="localizationPrefixProvider">提供本地化键前缀的实例。</param>
    /// <param name="num">提示行序号。</param>
    /// <returns>当前 <see cref="CAItemTooltipModifier"/> 实例，支持链式调用。</returns>
    public CAItemTooltipModifier ModifyWithCATweakColorDefault(ILocalizationPrefix localizationPrefixProvider, int num) =>
        ModifyWithCATweakColor(num, localizationPrefixProvider.GetTextValue($"{Tooltip}{num}"));

    /// <summary>
    /// 通过本地化提供程序获取格式化后的默认文本，并以模组渐变色修改指定序号的标准提示行。
    /// 文本键名格式为 "Tooltip{num}"。
    /// </summary>
    /// <param name="localizationPrefixProvider">提供本地化键前缀的实例。</param>
    /// <param name="num">提示行序号。</param>
    /// <param name="args">用于格式化本地化字符串的参数。</param>
    /// <returns>当前 <see cref="CAItemTooltipModifier"/> 实例，支持链式调用。</returns>
    public CAItemTooltipModifier ModifyWithCATweakColorDefault(ILocalizationPrefix localizationPrefixProvider, int num, params object[] args) =>
        ModifyWithCATweakColor(num, localizationPrefixProvider.GetTextValue($"{Tooltip}{num}", args));

    /// <summary>
    /// 移除列表中所有属于本模组且以 "CATooltip" 为前缀的提示行。
    /// </summary>
    /// <returns>当前 <see cref="CAItemTooltipModifier"/> 实例，支持链式调用。</returns>
    public CAItemTooltipModifier ClearAllCATooltips()
    {
        _tooltips.RemoveAll(line => line.Mod == CASharedData.ModName && line.Name.StartsWith(CATooltip));
        return this;
    }

    /// <summary>
    /// 在当前有效插入位置添加一个具有默认颜色的 CA 提示行。
    /// 调用前应确保 <see cref="Valid"/> 为 <see langword="true"/>。
    /// </summary>
    /// <param name="text">提示文本内容。</param>
    /// <returns>当前 <see cref="CAItemTooltipModifier"/> 实例，支持链式调用。</returns>
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

    /// <summary>
    /// 在当前有效插入位置添加一个具有指定颜色的 CA 提示行。
    /// 调用前应确保 <see cref="Valid"/> 为 <see langword="true"/>。
    /// </summary>
    /// <param name="text">提示文本内容。</param>
    /// <param name="color">提示行的覆盖颜色。</param>
    /// <returns>当前 <see cref="CAItemTooltipModifier"/> 实例，支持链式调用。</returns>
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

    /// <summary>
    /// 在当前有效插入位置添加一个 CA 提示行，并允许通过委托对其执行自定义初始化操作。
    /// 调用前应确保 <see cref="Valid"/> 为 <see langword="true"/>。
    /// </summary>
    /// <param name="action">用于配置新创建提示行的委托。</param>
    /// <returns>当前 <see cref="CAItemTooltipModifier"/> 实例，支持链式调用。</returns>
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

    /// <summary>
    /// 通过本地化提供程序获取默认文本，并添加一个具有默认颜色的 CA 提示行。
    /// 文本键名格式为 "CATooltip{_nextCATooltipIndex}"。
    /// </summary>
    /// <param name="localizationPrefixProvider">提供本地化键前缀的实例。</param>
    /// <returns>当前 <see cref="CAItemTooltipModifier"/> 实例，支持链式调用。</returns>
    public CAItemTooltipModifier AddCATooltipDefault(ILocalizationPrefix localizationPrefixProvider) => AddCATooltip(localizationPrefixProvider.GetTextValue($"{CATooltip}{_nextCATooltipIndex}"));

    /// <summary>
    /// 通过本地化提供程序获取格式化后的默认文本，并添加一个具有默认颜色的 CA 提示行。
    /// 文本键名格式为 "CATooltip{_nextCATooltipIndex}"。
    /// </summary>
    /// <param name="localizationPrefixProvider">提供本地化键前缀的实例。</param>
    /// <param name="args">用于格式化本地化字符串的参数。</param>
    /// <returns>当前 <see cref="CAItemTooltipModifier"/> 实例，支持链式调用。</returns>
    public CAItemTooltipModifier AddCATooltipDefault(ILocalizationPrefix localizationPrefixProvider, params object[] args) => AddCATooltip(localizationPrefixProvider.GetTextValue($"{CATooltip}{_nextCATooltipIndex}", args));

    /// <summary>
    /// 通过本地化提供程序获取默认文本，并以指定颜色添加一个 CA 提示行。
    /// 文本键名格式为 "CATooltip{_nextCATooltipIndex}"。
    /// </summary>
    /// <param name="localizationPrefixProvider">提供本地化键前缀的实例。</param>
    /// <param name="newColor">提示行的覆盖颜色。</param>
    /// <returns>当前 <see cref="CAItemTooltipModifier"/> 实例，支持链式调用。</returns>
    public CAItemTooltipModifier AddCATooltipDefault(ILocalizationPrefix localizationPrefixProvider, Color newColor) => AddCATooltip(localizationPrefixProvider.GetTextValue($"{CATooltip}{_nextCATooltipIndex}"), newColor);

    /// <summary>
    /// 通过本地化提供程序获取格式化后的默认文本，并以指定颜色添加一个 CA 提示行。
    /// 文本键名格式为 "CATooltip{_nextCATooltipNum}"。
    /// </summary>
    /// <param name="localizationPrefixProvider">提供本地化键前缀的实例。</param>
    /// <param name="newColor">提示行的覆盖颜色。</param>
    /// <param name="args">用于格式化本地化字符串的参数。</param>
    /// <returns>当前 <see cref="CAItemTooltipModifier"/> 实例，支持链式调用。</returns>
    public CAItemTooltipModifier AddCATooltipDefault(ILocalizationPrefix localizationPrefixProvider, Color newColor, params object[] args) => AddCATooltip(localizationPrefixProvider.GetTextValue($"{CATooltip}{_nextCATooltipNum}", args), newColor);

    /// <summary>
    /// 使用模组定义的渐变色（强度 0.25f）添加一个 CA 提示行。
    /// </summary>
    /// <param name="text">提示文本内容。</param>
    /// <returns>当前 <see cref="CAItemTooltipModifier"/> 实例，支持链式调用。</returns>
    public CAItemTooltipModifier AddCATweakTooltip(string text) => AddCATooltip(text, CASharedData.GetGradientColor(0.25f));

    /// <summary>
    /// 添加一个 CA 提示行，并在委托中自动应用模组渐变色，同时允许执行额外的自定义操作。
    /// </summary>
    /// <param name="action">用于配置新创建提示行的委托。渐变色将在调用委托前预先设置。</param>
    /// <returns>当前 <see cref="CAItemTooltipModifier"/> 实例，支持链式调用。</returns>
    public CAItemTooltipModifier AddCATweakTooltip(Action<TooltipLine> action) =>
        AddCATooltip(l =>
        {
            l.OverrideColor = CASharedData.GetGradientColor(0.25f);
            action?.Invoke(l);
        });

    /// <summary>
    /// 通过本地化提供程序获取默认文本，并使用模组渐变色添加一个 CA 提示行。
    /// 文本键名格式为 "CATooltip{_nextCATooltipIndex}"。
    /// </summary>
    /// <param name="localizationPrefixProvider">提供本地化键前缀的实例。</param>
    /// <returns>当前 <see cref="CAItemTooltipModifier"/> 实例，支持链式调用。</returns>
    public CAItemTooltipModifier AddCATweakTooltipDefault(ILocalizationPrefix localizationPrefixProvider) => AddCATweakTooltip(localizationPrefixProvider.GetTextValue($"{CATooltip}{_nextCATooltipIndex}"));

    /// <summary>
    /// 通过本地化提供程序获取格式化后的默认文本，并使用模组渐变色添加一个 CA 提示行。
    /// 文本键名格式为 "CATooltip{_nextCATooltipIndex}"。
    /// </summary>
    /// <param name="localizationPrefixProvider">提供本地化键前缀的实例。</param>
    /// <param name="args">用于格式化本地化字符串的参数。</param>
    /// <returns>当前 <see cref="CAItemTooltipModifier"/> 实例，支持链式调用。</returns>
    public CAItemTooltipModifier AddCATweakTooltipDefault(ILocalizationPrefix localizationPrefixProvider, params object[] args) => AddCATweakTooltip(localizationPrefixProvider.GetTextValue($"{CATooltip}{_nextCATooltipIndex}", args));

    /// <summary>
    /// 添加一条提示玩家按住 Shift 以展开详细信息的灰色提示行。
    /// 该行文本来自 Calamity Mod 的本地化键 "Misc.ShiftToExpand"。
    /// </summary>
    /// <returns>当前 <see cref="CAItemTooltipModifier"/> 实例，支持链式调用。</returns>
    public CAItemTooltipModifier AddExpendedDisplayLine() => AddCATooltip(Language.GetTextValue(CASharedData.CalamityModLocalizationPrefix + "Misc.ShiftToExpand"), new Color(0xBE, 0xBE, 0xBE));
}