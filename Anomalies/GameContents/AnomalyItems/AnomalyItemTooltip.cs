using Anomalies.Assets.Effects;

namespace Anomalies.GameContents.AnomalyItems;

public sealed class AnomalyItemTooltip : AnomalyGlobalItemBehavior
{
    private static string[] RevTooltipInsertionPositions =
    [
        "Master",
        "Expert",
        "SetBonus",
        "PrefixAccMeleeSpeed",
        "PrefixAccMoveSpeed",
        "PrefixAccDamage",
        "PrefixAccCritChance",
        "PrefixAccMaxMana",
        "PrefixAccDefense",
        "PrefixKnockback",
        "PrefixShootSpeed",
        "PrefixSize",
        "PrefixUseMana",
        "PrefixCritChance",
        "PrefixSpeed",
        "PrefixDamage",
        "OneDropLogo",
        "BuffTime",
        "WellFedExpert",
        "EtherianManaWarning",
    ];

    public override bool PreDrawTooltipLine(Item item, DrawableTooltipLine line, ref int yOffset)
    {
        if (item.rare == ModContent.RarityType<AnomalyRarity>())
        {
            if (TODrawUtils.TryDrawItemNameWithCustomShader(line, l => AnomalyEffects.Texts.AnomalyTitle.Data
                .SetCustomParameter("uScreenResolution", Main.ScreenSize.ToVector2() * Math.Max(Main.UIScale, 1f) / 2f)
                .SetCustomParameter("uScreenRatio", TODrawUtils.ScreenRatio)
                .SetCustomParameter("uPosition", new Vector2(l.X, l.Y))
                .Apply()))
            {
                return false;
            }
        }
        else if (item.rare == ModContent.RarityType<AnomalyUltramundaneRarity>())
        {
            if (TODrawUtils.TryDrawItemNameWithCustomShader(line, l => AnomalyEffects.Texts.AnomalyUltramundaneTitle.Data
                .SetCustomParameter("uScreenResolution", Main.ScreenSize.ToVector2() * Math.Max(Main.UIScale, 1f) / 2f)
                .SetCustomParameter("uScreenRatio", TODrawUtils.ScreenRatio)
                .SetCustomParameter("uPosition", new Vector2(l.X, l.Y))
                .Apply()))
            {
                return false;
            }
        }

        return true;
    }

    public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
    {
        if (item.rare == ModContent.RarityType<AnomalyRarity>())
        {
            AnomalyItemTooltipModifier modifier = new(item, tooltips);
            int difficultyIndex = -1;
            foreach (string name in RevTooltipInsertionPositions)
            {
                if (modifier.TryGet(null, name, out difficultyIndex, out _))
                    break;
            }
            if (difficultyIndex == -1)
                difficultyIndex = modifier._LastTooltipIndex;

            if (difficultyIndex == -1)
                return;

            tooltips.Insert(++difficultyIndex, new TooltipLine(Mod, "AnomalyIdentifier", Language.GetTextValue(AnomalySharedData.ModLocalizationPrefix + "AnomalyMode.Name")) { OverrideColor = AnomalySharedData.RebornColor});
        }
    }
}
