using Anomalies.Assets.Effects;

namespace Anomalies.Common.SingleBehaviors;

public sealed class AnomalyItemTweakTooltip : AnomalyGlobalItemBehavior, ILocalizationPrefix
{
    public string LocalizationPrefix => AnomalySharedData.TweakLocalizationPrefix;

    public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
    {
        if (AnomalySharedData.TweakedItems[item.type] && tooltips.TryFindVanillaTooltipByName("ItemName", out int index, out _))
            tooltips.Insert(index + 1, new TooltipLine(Mod, "TweakIdentifier", this.GetTextValue("TweakIdentifier")) { OverrideColor = TOColors.CelestialColor });
    }

    public override bool PreDrawTooltipLine(Item item, DrawableTooltipLine line, ref int yOffset)
    {
        bool result = true;

        if (line.Mod == Mod.Name && line.Name == "TweakIdentifier")
        {
            //查找关键词"Anomalies"
            const string Key = "Anomalies";
            const int LengthOfKey = 9; // "Anomalies"的长度

            string text = line.Text;
            int index = text.IndexOf(Key);

            if (index != -1)
            {
                string beforeKey = text[..index];
                string afterKey = text[(index + LengthOfKey)..];

                SpriteBatch spriteBatch = Main.spriteBatch;
                DynamicSpriteFont font = line.Font;
                Vector2 baseDrawPosition = new(line.X, line.Y);
                Vector2 baseScale = line.BaseScale;
                float rotation = line.Rotation;

                //先绘制黑底
                TODrawUtils.DrawBorderString(spriteBatch, font, text, baseDrawPosition, Color.Black, 4, 1f, baseScale, rotation);

                //分段绘制

                //关键词前
                if (!string.IsNullOrEmpty(beforeKey))
                {
                    Vector2 beforeKeySize = font.MeasureString(beforeKey) * baseScale;
                    spriteBatch.DrawString(font, beforeKey, baseDrawPosition, line.OverrideColor ?? Color.White, rotation, Vector2.Zero, baseScale, SpriteEffects.None, 0f);
                    baseDrawPosition.X += beforeKeySize.X;
                }

                //关键词本身
                using (spriteBatch.Scope(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp))
                {
                    Vector2 keySize = font.MeasureString(Key) * baseScale;
                    AnomalyEffects.Texts.AnomalyTitle.Data
                        .SetCustomParameter("uScreenResolution", Main.ScreenSize.ToVector2() * Math.Max(Main.UIScale, 1f) / 2f)
                        .SetCustomParameter("uScreenRatio", TODrawUtils.ScreenRatio)
                        .SetCustomParameter("uPosition", baseDrawPosition)
                        .Apply();
                    spriteBatch.DrawString(font, Key, baseDrawPosition, Color.White, 0f, Vector2.Zero, baseScale, SpriteEffects.None, 0f);
                    baseDrawPosition.X += keySize.X;
                }

                //关键词后
                if (!string.IsNullOrEmpty(afterKey))
                {
                    spriteBatch.DrawString(font, afterKey, baseDrawPosition, line.OverrideColor ?? Color.White, rotation, Vector2.Zero, baseScale, SpriteEffects.None, 0f);
                }

                result = false;
            }
        }

        return result;
    }
}
