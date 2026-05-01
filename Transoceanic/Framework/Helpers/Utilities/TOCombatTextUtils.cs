// Developed by ColdsUx

namespace Transoceanic.Framework.Helpers;

/// <summary>
/// 提供操作 <see cref="CombatText"/> 对象和游戏内战斗文本的工具方法。
/// </summary>
public static class TOCombatTextUtils
{
    /// <summary>
    /// 在 <see cref="ModItem.OnHitNPC(Player, NPC, NPC.HitInfo, int)"/> 等 OnHitNPC 方法中调用，修改本次命中生成的 <see cref="CombatText"/> 实例。"
    /// </summary>
    /// <param name="action"></param>
    public static void ChangeHitNPCText(Action<CombatText> action)
    {
        if (action is null)
            return;
        CombatText text = null;
        for (int i = Main.maxCombatText - 1; i >= 0; i--)
        {
            CombatText current = Main.combatText[i];
            if (current is { crit: true, lifeTime: 120, alpha: 1f } or { crit: false, lifeTime: 60, alpha: 1f } && int.TryParse(current.text, out _))
            {
                text = current;
                break;
            }
        }
        if (text is not null)
            action(text);
    }
}
