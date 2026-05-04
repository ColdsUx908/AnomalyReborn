// Developed by ColdsUx

namespace CalamityAnomalies.GameContents;

public abstract class CALegendaryItem : CAModItem
{
    public int Phase = 1;
    public int SubPhase = 1;

    /// <summary>
    /// 传奇物品的更新方法。
    /// <br/>处理传奇武器的“随游戏进度成长”特性。
    /// </summary>
    public abstract void LegendaryUpdate();

    /// <summary>
    /// 传奇物品的更新方法。
    /// <br/>处理传奇饰品的“随游戏进度成长”特性，并同时更新玩家的相关状态。
    /// </summary>
    /// <param name="player"></param>
    public abstract void LegendaryUpdate(Player player);

    public override void Update(ref float gravity, ref float maxFallSpeed) => LegendaryUpdate();

    public override void UpdateInventory(Player player) => LegendaryUpdate(player);
}
