using Anomalies.Assets.Effects;

namespace Anomalies.GameContents.AnomalyItems;

public sealed class AnomalyItemTooltip : AnomalyGlobalItemBehavior
{
    public static readonly List<string> RarityTooltipInsertionPositions =
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
        bool result = true;

        if (!AnomalySharedData.DontApplyAnomalyRarityShaderItems[item.type])
        {
            if (item.rare == ModContent.RarityType<AnomalyRarity>())
            {
                if (TODrawUtils.TryDrawItemNameWithCustomShader(line, l => AnomalyEffects.Texts.AnomalyTitle.Data
                    .SetCustomParameter("uScreenResolution", Main.ScreenSize.ToVector2() * Math.Max(Main.UIScale, 1f) / 2f)
                    .SetCustomParameter("uScreenRatio", TODrawUtils.ScreenRatio)
                    .SetCustomParameter("uPosition", new Vector2(l.X, l.Y))
                    .Apply()))
                {
                    result = false;
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
                    result = false;
                }
            }
        }

        return result;
    }

    public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
    {
        int anomalyRarity = ModContent.RarityType<AnomalyRarity>();
        int anomalyUltramundaneRarity = ModContent.RarityType<AnomalyUltramundaneRarity>();
        if (item.rare == anomalyRarity || item.rare == anomalyUltramundaneRarity)
        {
            AnomalyItemTooltipModifier modifier = new(item, tooltips);
            int difficultyIndex = -1;
            foreach (string name in RarityTooltipInsertionPositions)
            {
                if (modifier.TryGet(null, name, out difficultyIndex, out _))
                    break;
            }
            if (difficultyIndex == -1)
                difficultyIndex = modifier._LastTooltipIndex;

            if (difficultyIndex == -1)
                return;

            tooltips.Insert(++difficultyIndex, new TooltipLine(Mod,
                "Anomaly" + (item.rare == anomalyRarity ? "Identifier" : "UltramundaneIdentifier"),
                Language.GetTextValue(AnomalySharedData.ModLocalizationPrefix + "AnomalyMode." + (item.rare == anomalyRarity ? "Name" : "UltraName")))
            { OverrideColor = AnomalySharedData.RebornColor });
        }
    }
}
