using Terraria.GameContent.ItemDropRules;

namespace Transoceanic.DataStructures;

public sealed class CustomDropRuleCondition : IItemDropRuleCondition
{
    private readonly Func<DropAttemptInfo, bool> _canDrop;
    private readonly Func<bool> _canShowItemDropInUI;
    private readonly Func<string> _getConditionDescription;

    public CustomDropRuleCondition(Func<DropAttemptInfo, bool> canDrop = null, Func<bool> canShowItemDropInUI = null, Func<string> getConditionDescription = null)
    {
        _canDrop = canDrop;
        _canShowItemDropInUI = canShowItemDropInUI;
        _getConditionDescription = getConditionDescription;
    }

    public bool CanDrop(DropAttemptInfo info) => _canDrop?.Invoke(info) ?? false;
    public bool CanShowItemDropInUI() => _canShowItemDropInUI?.Invoke() ?? false;
    public string GetConditionDescription() => _getConditionDescription?.Invoke() ?? "";
}
