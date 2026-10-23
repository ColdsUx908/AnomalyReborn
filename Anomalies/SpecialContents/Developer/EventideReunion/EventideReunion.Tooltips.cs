using Anomalies.Assets.Effects;
using Anomalies.GameContents;

namespace Anomalies.SpecialContents.Developer.EventideReunion;

public sealed partial class EventideReunion
{
    private static readonly string[] _AspectTooltipPasses = ["Sun", "Samsara", "Life", "Death", "Sacredness", "Star"];

    public override void ModifyTooltips(List<TooltipLine> tooltips)
    {
        AnomalyItemTooltipModifier modifier = new(Item, tooltips);
        bool shift = Main.keyState.PressingShift();

        int index = modifier._LastTooltipIndex;

        IDeveloperItem.AddDeveloperItemIdentifier(Mod, tooltips, ++index);
        if (shift)
            IDeveloperItem.AddDeveloperItemOwnerIdentifier(Mod, tooltips, ++index, "VectorChaos");
        ILegendaryItem.AddLegendaryItemIdentifier(Mod, tooltips, ++index);

        modifier.Update();

        if (!shift)
            modifier.AddExpendedDisplayLine();
    }

    public override bool PreDrawTooltipLine(DrawableTooltipLine line, ref int yOffset)
    {
        if (TryDrawDeveloperName(line))
            return false;

        if (TryDrawAspectTooltip(line))
            return false;

        if (TryDrawItemName(line))
            return false;

        return true;

        static bool TryDrawDeveloperName(DrawableTooltipLine developerLine)
        {
            const string Name = "VectorChaos";
            if (developerLine.Mod != AnomalyMain.Instance.Name || developerLine.Name != "Tooltip_DeveloperItemOwnerIdentifier"
                || AnomalyEffects.Texts.EventideReunion is not { } shader)
                return false;
            int index = developerLine.Text.IndexOf(Name, StringComparison.Ordinal);
            if (index < 0)
                return false;

            SpriteBatch batch = Main.spriteBatch;
            string prefix = developerLine.Text[..index];
            string suffix = developerLine.Text[(index + Name.Length)..];
            Vector2 position = new(developerLine.X, developerLine.Y);
            Vector2 nameOffset = new(developerLine.Font.MeasureString(prefix).X, 0f);
            Vector2 suffixOffset = new(developerLine.Font.MeasureString(developerLine.Text[..(index + Name.Length)]).X, 0f);
            Vector2 namePosition = position + (nameOffset * developerLine.BaseScale).RotatedBy(developerLine.Rotation);
            Color color = developerLine.OverrideColor ?? developerLine.Color;

            // 本地化前后缀正常绘制，只有名字进入混沌 pass；不移动字形，保持可读性。
            TODrawUtils.DrawBorderString(batch, developerLine.Font, developerLine.Text, position, Color.Black,
                4, 1f, developerLine.BaseScale, developerLine.Rotation);
            batch.DrawString(developerLine.Font, prefix, position, color, developerLine.Rotation,
                Vector2.Zero, developerLine.BaseScale, SpriteEffects.None, 0f);
            batch.DrawString(developerLine.Font, suffix, position + (suffixOffset * developerLine.BaseScale).RotatedBy(developerLine.Rotation),
                color, developerLine.Rotation, Vector2.Zero, developerLine.BaseScale, SpriteEffects.None, 0f);
            using (batch.Scope(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp))
            {
                Effect effect = shader.Asset.Value;
                effect.SetCustomParameter("uPosition", namePosition * Main.UIScale)
                    .SetCustomParameter("uTextSize", developerLine.Font.MeasureString(Name))
                    .SetCustomParameter("uTextScale", developerLine.BaseScale * Main.UIScale)
                    .SetCustomParameter("uTextRotation", developerLine.Rotation)
                    .SetCustomParameter("uTime", Main.GlobalTimeWrappedHourly);
                effect.CurrentTechnique.Passes["Chaos"].Apply();
                batch.DrawString(developerLine.Font, Name, namePosition, Color.White, developerLine.Rotation,
                    Vector2.Zero, developerLine.BaseScale, SpriteEffects.None, 0f);
            }
            return true;
        }

        static bool TryDrawAspectTooltip(DrawableTooltipLine line)
        {
            if (line.Mod != "Terraria" || line.Name is not ("Tooltip0" or "Tooltip1")
                || AnomalyEffects.Texts.EventideReunion is not { } shader)
                return false;

            if (line.Name == "Tooltip1")
            {
                DrawSegment(line.Text, Vector2.Zero, "FinalSun");
                return true;
            }

            // 中英文均按六个分句分配符文，保留原标点和空格；其他结构回退原版绘制。
            Span<Range> clauses = stackalloc Range[6];
            int start = 0;
            int count = 0;
            for (int i = 0; i < line.Text.Length; i++)
            {
                if (line.Text[i] is not (',' or '，' or ';' or '；'))
                    continue;
                if (count >= 5)
                    return false;
                clauses[count++] = start..(i + 1);
                start = i + 1;
            }
            if (count != 5 || start == line.Text.Length)
                return false;
            clauses[5] = start..line.Text.Length;

            for (int i = 0; i < clauses.Length; i++)
            {
                int prefixLength = clauses[i].Start.Value;
                Vector2 offset = new(line.Font.MeasureString(line.Text[..prefixLength]).X, 0f);
                DrawSegment(line.Text[clauses[i]], offset, _AspectTooltipPasses[i]);
            }
            return true;

            void DrawSegment(string text, Vector2 offset, string pass)
            {
                SpriteBatch batch = Main.spriteBatch;
                Vector2 position = new Vector2(line.X, line.Y) + (offset * line.BaseScale).RotatedBy(line.Rotation);
                TODrawUtils.DrawBorderString(batch, line.Font, text, position, Color.Black * 0.85f, 4, 1f, line.BaseScale, line.Rotation);
                using (batch.Scope(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp))
                {
                    Effect effect = shader.Asset.Value;
                    // VPOS 是屏幕像素，先移除 UI 缩放，再还原为文字局部像素，避免宽屏或 UI 缩放拉伸纹样。
                    effect.SetCustomParameter("uPosition", position * Main.UIScale)
                        .SetCustomParameter("uTextSize", line.Font.MeasureString(text))
                        .SetCustomParameter("uTextScale", line.BaseScale * Main.UIScale)
                        .SetCustomParameter("uTextRotation", line.Rotation)
                        .SetCustomParameter("uTime", Main.GlobalTimeWrappedHourly);
                    effect.CurrentTechnique.Passes[pass].Apply();
                    batch.DrawString(line.Font, text, position, Color.White, line.Rotation, Vector2.Zero, line.BaseScale, SpriteEffects.None, 0f);
                }
            }
        }

        static bool TryDrawItemName(DrawableTooltipLine line)
        {
            if (line.Mod == "Terraria" && line.Name == "ItemName"
                && AnomalyEffects.Texts.EventideReunion is { } shader)
            {
                SpriteBatch spriteBatch = Main.spriteBatch;
                Vector2 position = new(line.X, line.Y);
                // 三秒一个呼吸周期，暗部仍保留金色轮廓。
                float breath = 0.5f + 0.5f * MathF.Sin(Main.GlobalTimeWrappedHourly * MathHelper.TwoPi / 3f);
                Color borderColor = Color.Lerp(new Color(145, 85, 20), new Color(255, 215, 80), breath);
                // 星芒先画在文字后方，避免加色亮芯覆盖笔画。
                DrawNameSparkles(line);
                using (spriteBatch.Scope(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp))
                {
                    // 金色呼吸保留在外沿，内层暗描边分隔金色文字与外沿，保持笔画清晰。
                    float borderWidth = 1f / Main.UIScale;
                    TODrawUtils.DrawBorderString(spriteBatch, line.Font, line.Text, position, borderColor * 0.7f, 8, borderWidth * 2f, line.BaseScale, line.Rotation);
                    TODrawUtils.DrawBorderString(spriteBatch, line.Font, line.Text, position, new Color(35, 20, 8), 8, borderWidth, line.BaseScale, line.Rotation);
                    shader.Data
                        .SetCustomParameter("uScreenResolution", Main.ScreenSize.ToVector2() * Math.Max(Main.UIScale, 1f) / 2f)
                        .SetCustomParameter("uScreenRatio", TODrawUtils.ScreenRatio)
                        .SetCustomParameter("uPosition", position)
                        .Apply();
                    spriteBatch.DrawString(line.Font, line.Text, position, Color.White, 0f, Vector2.Zero, line.BaseScale, SpriteEffects.None, 0f);
                }
                return true;
            }

            return false;

            static void DrawNameSparkles(DrawableTooltipLine nameLine)
            {
                Texture2D sparkle = AnomalyTextures.Extra.CrystalTextSparkle;
                Vector2 textSize = nameLine.Font.MeasureString(nameLine.Text);
                Vector2 origin = sparkle.Size() * 0.5f;
                Vector2 textPosition = new(nameLine.X, nameLine.Y);
                float time = Main.GlobalTimeWrappedHourly;

                // 固定每颗星芒的位置、相位和转速，避免重绘时跳动或影响世界随机数。
                _TooltipSparkleRandom.SetSeed(1);
                int count = Math.Max(1, (int)(textSize.X / 6f));
                for (int i = 0; i < count; i++)
                {
                    Vector2 offset = new(_TooltipSparkleRandom.NextFloat(textSize.X), _TooltipSparkleRandom.NextFloat(textSize.Y * 0.6f) + 1f);
                    float phase = (time * 4f + _TooltipSparkleRandom.NextFloat(MathHelper.TwoPi)) % MathHelper.TwoPi;
                    float rotation = time * _TooltipSparkleRandom.NextFloat(0.8f, 1.5f) + nameLine.Rotation;
                    Color gold = Color.Lerp(new Color(255, 145, 35, 0), new Color(255, 210, 75, 0), _TooltipSparkleRandom.NextFloat());
                    float brightness = Math.Max(0f, MathF.Sin(phase));
                    float progress = phase / MathHelper.TwoPi;
                    offset.Y += 2f - phase * 4.5f;
                    Vector2 position = textPosition + ((offset - nameLine.Origin) * nameLine.BaseScale).RotatedBy(nameLine.Rotation);
                    float rayWidth = progress * 0.15f;
                    float rayLength = (MathF.Sin(phase / MathHelper.PiOver2) + 1f) * 0.2f;

                    Main.spriteBatch.Draw(sparkle, position, null, gold * brightness, rotation, origin,
                        new Vector2(rayLength, rayWidth) * nameLine.BaseScale, SpriteEffects.None, 0f);
                    Main.spriteBatch.Draw(sparkle, position, null, gold * brightness, rotation, origin,
                        new Vector2(rayWidth, rayLength) * nameLine.BaseScale, SpriteEffects.None, 0f);
                    Main.spriteBatch.Draw(sparkle, position, null, new Color(255, 240, 190, 0) * brightness, rotation, origin,
                        nameLine.BaseScale * (progress * 0.3f), SpriteEffects.None, 0f);
                }
            }
        }
    }
}
