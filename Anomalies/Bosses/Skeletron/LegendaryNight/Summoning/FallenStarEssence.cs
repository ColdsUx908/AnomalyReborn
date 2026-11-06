using Anomalies.GameContents;

namespace Anomalies.Bosses.Skeletron.LegendaryNight;

/// <summary>
/// 传说之夜召唤内容。
/// </summary>
/// <remarks>
/// <para>数据槽位：不使用 NPC/Projectile 的 ai、localAI 或框架计时槽；状态由物品/物块原版字段及调用的仪式实体管理。</para>
/// </remarks>
public sealed class FallenStarEssence : AnomalyModItem
{
    public override string Texture => LegendaryNightHandler.LegendaryNightPath + "Summoning/FallenStarEssence";

    public override string LocalizationCategory => LegendaryNightHandler.LocalizationCategory;

    public override void SetDefaults()
    {
        Item.width = 40;
        Item.height = 42;
        Item.maxStack = 5;
        Item.rare = ModContent.RarityType<AnomalyUltramundaneRarity>();
        Item.value = AnomalyRarity.AnomalyPrice;
    }

    public override void ModifyResearchSorting(ref ContentSamples.CreativeHelper.ItemGroup itemGroup) => itemGroup = ContentSamples.CreativeHelper.ItemGroup.Material;

    public override void Update(ref float gravity, ref float maxFallSpeed)
    {
        gravity = 0f;
        maxFallSpeed = 0f;
    }
}