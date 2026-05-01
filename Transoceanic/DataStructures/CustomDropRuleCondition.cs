// Developed by ColdsUx

using Terraria.GameContent.ItemDropRules;

namespace Transoceanic.DataStructures;

/// <summary>
/// 一个可自定义行为的物品掉落规则条件，通过委托来灵活实现 <see cref="IItemDropRuleCondition"/> 接口。
/// 适用于需要在运行时动态指定掉落判定逻辑、UI显示逻辑以及条件描述的场景。
/// </summary>
public sealed class CustomDropRuleCondition : IItemDropRuleCondition
{
    private readonly Func<DropAttemptInfo, bool> _canDrop;
    private readonly Func<bool> _canShowItemDropInUI;
    private readonly Func<string> _getConditionDescription;

    /// <summary>
    /// 初始化 <see cref="CustomDropRuleCondition"/> 类的新实例。
    /// </summary>
    /// <param name="canDrop">
    /// 用于确定是否允许物品掉落的委托。该委托接收一个 <see cref="DropAttemptInfo"/> 参数并返回一个布尔值。
    /// 若为 <see langword="null"/>，则默认返回 <see langword="false"/>（即不允许掉落）。
    /// </param>
    /// <param name="canShowItemDropInUI">
    /// 用于确定是否应在UI（例如Bestiary）中显示该物品掉落信息的委托。
    /// 若为 <see langword="null"/>，则默认返回 <see langword="false"/>（即不显示）。
    /// </param>
    /// <param name="getConditionDescription">
    /// 用于获取条件描述文本的委托，该文本通常显示在UI中说明物品掉落所需的条件。
    /// 若为 <see langword="null"/>，则默认返回空字符串。
    /// </param>
    public CustomDropRuleCondition(
        Func<DropAttemptInfo, bool> canDrop = null,
        Func<bool> canShowItemDropInUI = null,
        Func<string> getConditionDescription = null)
    {
        _canDrop = canDrop;
        _canShowItemDropInUI = canShowItemDropInUI;
        _getConditionDescription = getConditionDescription;
    }

    /// <inheritdoc/>
    public bool CanDrop(DropAttemptInfo info) => _canDrop?.Invoke(info) ?? false;

    /// <inheritdoc/>
    public bool CanShowItemDropInUI() => _canShowItemDropInUI?.Invoke() ?? false;

    /// <inheritdoc/>
    public string GetConditionDescription() => _getConditionDescription?.Invoke() ?? "";
}