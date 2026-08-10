// Developed by ColdsUx

namespace Anomalies.Visuals;

/// <summary>
/// 针对 Anomalies 模组扩展的 <see cref="ItemTooltipModifier"/> 实现，
/// 提供对自定义提示行（以 " AnomalyTooltip" 为前缀）的插入、删除和批量修改功能。
/// 支持链式调用，并内置模组特有的渐变色样式。
/// </summary>
public sealed class AnomalyItemTooltipModifier : ItemTooltipModifier
{
    /// <summary>
    /// 本模组自定义提示行的名称前缀，用于生成 "CATooltip0"、"CATooltip1" 等标识。
    /// </summary>
    public const string AnomalyTooltip = " AnomalyTooltip";

    /// <summary>
    /// （保留字段）指向关联的 <see cref="ItemTooltipDictionary"/> 实例，当前未使用。
    /// </summary>
    public ItemTooltipDictionary _TooltipDictionary;

    /// <summary>
    /// 下一个可用的 Anomaly 提示行序号（用于命名）。
    /// </summary>
    public int _NextCATooltipNum;

    /// <summary>
    /// 下一个 Anomaly 提示行应当插入的列表索引位置。
    /// </summary>
    public int _NextCATooltipIndex;

    /// <summary>
    /// 获取一个值，该值指示当前实例是否处于有效状态（即已成功定位到插入位置）。
    /// </summary>
    public bool Valid => _NextCATooltipIndex != -1;

    /// <summary>
    /// 初始化 <see cref="CAItemTooltipModifier"/> 的新实例，并立即执行一次状态更新。
    /// </summary>
    /// <param name="item">关联的物品实例。</param>
    /// <param name="tooltips">需要管理的提示行列表。</param>
    public AnomalyItemTooltipModifier(Item item, List<TooltipLine> tooltips) : base(item, tooltips) => UpdateCA();

    /// <summary>
    /// 扫描当前提示行列表，确定下一个 Anomaly 提示行的插入位置和序号。
    /// 从列表末尾向前遍历，寻找最近的 " AnomalyTooltip" 或标准 "Tooltip" 行作为定位锚点。
    /// 若未找到任何锚点，则 <see cref="_NextCATooltipIndex"/> 被设置为 -1。
    /// </summary>
    public void UpdateCA()
    {
        for (int i = _Tooltips.Count - 1; i >= 0; i--)
        {
            TooltipLine line = _Tooltips[i];
            if (line.Mod == AnomalySharedData.ModName && line.Name.StartsWith( AnomalyTooltip) && int.TryParse(line.Name[ AnomalyTooltip.Length..], out int index))
            {
                _NextCATooltipIndex = i + 1;
                _NextCATooltipNum = index + 1;
                return;
            }
            if (line.Name.StartsWith(Tooltip))
            {
                _NextCATooltipIndex = i + 1;
                _NextCATooltipNum = 0;
                return;
            }
        }
        _NextCATooltipIndex = -1;
        return;
    }

    /// <summary>
    /// 更新内部索引字典并刷新 Anomaly 提示行的插入状态。
    /// 通常在外部修改提示行列表后调用，以保持一致性。
    /// </summary>
    public void Update()
    {
        UpdateDictionary();
        UpdateCA();
    }

    public override AnomalyItemTooltipModifier Modify(string mod, string name, string newText) => (AnomalyItemTooltipModifier)base.Modify(mod, name, newText);

    public override AnomalyItemTooltipModifier Modify(string mod, string name, string newText, Color newColor) => (AnomalyItemTooltipModifier)base.Modify(mod, name, newText, newColor);

    public override AnomalyItemTooltipModifier Modify(string mod, string name, Action<TooltipLine> action) => (AnomalyItemTooltipModifier)base.Modify(mod, name, action);

    public override AnomalyItemTooltipModifier ModifyTooltip(int num, string newText) => Modify(null, $"{Tooltip}{num}", newText);

    public override AnomalyItemTooltipModifier ModifyTooltip(int num, string newText, Color newColor) => Modify(null, $"{Tooltip}{num}", newText, newColor);

    public override AnomalyItemTooltipModifier ModifyTooltip(int num, Action<TooltipLine> action) => Modify(null, $"{Tooltip}{num}", action);

    /// <summary>
    /// 使用模组定义的渐变色（强度 0.25f）修改指定序号的标准提示行文本。
    /// </summary>
    /// <param name="num">提示行序号。</param>
    /// <param name="newText">新的文本内容。</param>
    /// <returns>当前 <see cref="CAItemTooltipModifier"/> 实例，支持链式调用。</returns>
    public AnomalyItemTooltipModifier ModifyWithCATweakColor(int num, string newText) => ModifyTooltip(num, newText, AnomalySharedData.GetGradientColor(0.25f));

    /// <summary>
    /// 通过本地化提供程序获取默认文本，并以模组渐变色修改指定序号的标准提示行。
    /// 文本键名格式为 "Tooltip{num}"。
    /// </summary>
    /// <param name="localizationPrefixProvider">提供本地化键前缀的实例。</param>
    /// <param name="num">提示行序号。</param>
    /// <returns>当前 <see cref="CAItemTooltipModifier"/> 实例，支持链式调用。</returns>
    public AnomalyItemTooltipModifier ModifyWithCATweakColorDefault(ILocalizationPrefix localizationPrefixProvider, int num) =>
        ModifyWithCATweakColor(num, localizationPrefixProvider.GetTextValue($"{Tooltip}{num}"));

    /// <summary>
    /// 通过本地化提供程序获取格式化后的默认文本，并以模组渐变色修改指定序号的标准提示行。
    /// 文本键名格式为 "Tooltip{num}"。
    /// </summary>
    /// <param name="localizationPrefixProvider">提供本地化键前缀的实例。</param>
    /// <param name="num">提示行序号。</param>
    /// <param name="args">用于格式化本地化字符串的参数。</param>
    /// <returns>当前 <see cref="CAItemTooltipModifier"/> 实例，支持链式调用。</returns>
    public AnomalyItemTooltipModifier ModifyWithCATweakColorDefault(ILocalizationPrefix localizationPrefixProvider, int num, params object[] args) =>
        ModifyWithCATweakColor(num, localizationPrefixProvider.GetTextValue($"{Tooltip}{num}", args));

    /// <summary>
    /// 移除列表中所有属于本模组且以 " AnomalyTooltip" 为前缀的提示行。
    /// </summary>
    /// <returns>当前 <see cref="CAItemTooltipModifier"/> 实例，支持链式调用。</returns>
    public AnomalyItemTooltipModifier ClearAllCATooltips()
    {
        _Tooltips.RemoveAll(line => line.Mod == AnomalySharedData.ModName && line.Name.StartsWith( AnomalyTooltip));
        return this;
    }

    /// <summary>
    /// 在当前有效插入位置添加一个具有默认颜色的 Anomaly 提示行。
    /// 调用前应确保 <see cref="Valid"/> 为 <see langword="true"/>。
    /// </summary>
    /// <param name="text">提示文本内容。</param>
    /// <returns>当前 <see cref="CAItemTooltipModifier"/> 实例，支持链式调用。</returns>
    public AnomalyItemTooltipModifier AddCATooltip(string text)
    {
        if (Valid)
        {
            _Tooltips.Insert(_NextCATooltipIndex, AnomalyUtils.CreateNewTooltipLine(_NextCATooltipNum, text));
            _NextCATooltipIndex++;
            _NextCATooltipNum++;
        }
        return this;
    }

    /// <summary>
    /// 在当前有效插入位置添加一个具有指定颜色的 Anomaly 提示行。
    /// 调用前应确保 <see cref="Valid"/> 为 <see langword="true"/>。
    /// </summary>
    /// <param name="text">提示文本内容。</param>
    /// <param name="color">提示行的覆盖颜色。</param>
    /// <returns>当前 <see cref="CAItemTooltipModifier"/> 实例，支持链式调用。</returns>
    public AnomalyItemTooltipModifier AddCATooltip(string text, Color color)
    {
        if (Valid)
        {
            _Tooltips.Insert(_NextCATooltipIndex, AnomalyUtils.CreateNewTooltipLine(_NextCATooltipNum, text, color));
            _NextCATooltipIndex++;
            _NextCATooltipNum++;
        }
        return this;
    }

    /// <summary>
    /// 在当前有效插入位置添加一个 Anomaly 提示行，并允许通过委托对其执行自定义初始化操作。
    /// 调用前应确保 <see cref="Valid"/> 为 <see langword="true"/>。
    /// </summary>
    /// <param name="action">用于配置新创建提示行的委托。</param>
    /// <returns>当前 <see cref="CAItemTooltipModifier"/> 实例，支持链式调用。</returns>
    public AnomalyItemTooltipModifier AddCATooltip(Action<TooltipLine> action)
    {
        if (Valid)
        {
            _Tooltips.Insert(_NextCATooltipIndex, AnomalyUtils.CreateNewTooltipLine(_NextCATooltipNum, action));
            _NextCATooltipIndex++;
            _NextCATooltipNum++;
        }
        return this;
    }

    /// <summary>
    /// 通过本地化提供程序获取默认文本，并添加一个具有默认颜色的 Anomaly 提示行。
    /// 文本键名格式为 " AnomalyTooltip{_NextCATooltipIndex}"。
    /// </summary>
    /// <param name="localizationPrefixProvider">提供本地化键前缀的实例。</param>
    /// <returns>当前 <see cref="CAItemTooltipModifier"/> 实例，支持链式调用。</returns>
    public AnomalyItemTooltipModifier AddCATooltipDefault(ILocalizationPrefix localizationPrefixProvider) => AddCATooltip(localizationPrefixProvider.GetTextValue($"{ AnomalyTooltip}{_NextCATooltipIndex}"));

    /// <summary>
    /// 通过本地化提供程序获取格式化后的默认文本，并添加一个具有默认颜色的 Anomaly 提示行。
    /// 文本键名格式为 " AnomalyTooltip{_NextCATooltipIndex}"。
    /// </summary>
    /// <param name="localizationPrefixProvider">提供本地化键前缀的实例。</param>
    /// <param name="args">用于格式化本地化字符串的参数。</param>
    /// <returns>当前 <see cref="CAItemTooltipModifier"/> 实例，支持链式调用。</returns>
    public AnomalyItemTooltipModifier AddCATooltipDefault(ILocalizationPrefix localizationPrefixProvider, params object[] args) => AddCATooltip(localizationPrefixProvider.GetTextValue($"{ AnomalyTooltip}{_NextCATooltipIndex}", args));

    /// <summary>
    /// 通过本地化提供程序获取默认文本，并以指定颜色添加一个 Anomaly 提示行。
    /// 文本键名格式为 " AnomalyTooltip{_NextCATooltipIndex}"。
    /// </summary>
    /// <param name="localizationPrefixProvider">提供本地化键前缀的实例。</param>
    /// <param name="newColor">提示行的覆盖颜色。</param>
    /// <returns>当前 <see cref="CAItemTooltipModifier"/> 实例，支持链式调用。</returns>
    public AnomalyItemTooltipModifier AddCATooltipDefault(ILocalizationPrefix localizationPrefixProvider, Color newColor) => AddCATooltip(localizationPrefixProvider.GetTextValue($"{ AnomalyTooltip}{_NextCATooltipIndex}"), newColor);

    /// <summary>
    /// 通过本地化提供程序获取格式化后的默认文本，并以指定颜色添加一个 Anomaly 提示行。
    /// 文本键名格式为 " AnomalyTooltip{_NextCATooltipNum}"。
    /// </summary>
    /// <param name="localizationPrefixProvider">提供本地化键前缀的实例。</param>
    /// <param name="newColor">提示行的覆盖颜色。</param>
    /// <param name="args">用于格式化本地化字符串的参数。</param>
    /// <returns>当前 <see cref="CAItemTooltipModifier"/> 实例，支持链式调用。</returns>
    public AnomalyItemTooltipModifier AddCATooltipDefault(ILocalizationPrefix localizationPrefixProvider, Color newColor, params object[] args) => AddCATooltip(localizationPrefixProvider.GetTextValue($"{ AnomalyTooltip}{_NextCATooltipNum}", args), newColor);

    /// <summary>
    /// 使用模组定义的渐变色（强度 0.25f）添加一个 Anomaly 提示行。
    /// </summary>
    /// <param name="text">提示文本内容。</param>
    /// <returns>当前 <see cref="CAItemTooltipModifier"/> 实例，支持链式调用。</returns>
    public AnomalyItemTooltipModifier AddCATweakTooltip(string text) => AddCATooltip(text, AnomalySharedData.GetGradientColor(0.25f));

    /// <summary>
    /// 添加一个 Anomaly 提示行，并在委托中自动应用模组渐变色，同时允许执行额外的自定义操作。
    /// </summary>
    /// <param name="action">用于配置新创建提示行的委托。渐变色将在调用委托前预先设置。</param>
    /// <returns>当前 <see cref="CAItemTooltipModifier"/> 实例，支持链式调用。</returns>
    public AnomalyItemTooltipModifier AddCATweakTooltip(Action<TooltipLine> action) =>
        AddCATooltip(l =>
        {
            l.OverrideColor = AnomalySharedData.GetGradientColor(0.25f);
            action?.Invoke(l);
        });

    /// <summary>
    /// 通过本地化提供程序获取默认文本，并使用模组渐变色添加一个 Anomaly 提示行。
    /// 文本键名格式为 " AnomalyTooltip{_NextCATooltipIndex}"。
    /// </summary>
    /// <param name="localizationPrefixProvider">提供本地化键前缀的实例。</param>
    /// <returns>当前 <see cref="CAItemTooltipModifier"/> 实例，支持链式调用。</returns>
    public AnomalyItemTooltipModifier AddCATweakTooltipDefault(ILocalizationPrefix localizationPrefixProvider) => AddCATweakTooltip(localizationPrefixProvider.GetTextValue($"{ AnomalyTooltip}{_NextCATooltipIndex}"));

    /// <summary>
    /// 通过本地化提供程序获取格式化后的默认文本，并使用模组渐变色添加一个 Anomaly 提示行。
    /// 文本键名格式为 " AnomalyTooltip{_NextCATooltipIndex}"。
    /// </summary>
    /// <param name="localizationPrefixProvider">提供本地化键前缀的实例。</param>
    /// <param name="args">用于格式化本地化字符串的参数。</param>
    /// <returns>当前 <see cref="CAItemTooltipModifier"/> 实例，支持链式调用。</returns>
    public AnomalyItemTooltipModifier AddCATweakTooltipDefault(ILocalizationPrefix localizationPrefixProvider, params object[] args) => AddCATweakTooltip(localizationPrefixProvider.GetTextValue($"{ AnomalyTooltip}{_NextCATooltipIndex}", args));

    /// <summary>
    /// 添加一条提示玩家按住 Shift 以展开详细信息的灰色提示行。
    /// 该行文本来自 Calamity Mod 的本地化键 "Misc.ShiftToExpand"。
    /// </summary>
    /// <returns>当前 <see cref="CAItemTooltipModifier"/> 实例，支持链式调用。</returns>
    public AnomalyItemTooltipModifier AddExpendedDisplayLine() => AddCATooltip(Language.GetTextValue( AnomalySharedData.CalamityModLocalizationPrefix + "Misc.ShiftToExpand"), new Color(0xBE, 0xBE, 0xBE));
}