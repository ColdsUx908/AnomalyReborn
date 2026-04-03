namespace Transoceanic.Framework.Helpers;

public static partial class TOExtensions
{
    extension(List<TooltipLine> tooltips)
    {
        public bool TryFindTooltip(Func<TooltipLine, bool> match, out int index, out TooltipLine tooltip)
        {
            ArgumentNullException.ThrowIfNull(match);
            for (int i = 0; i < tooltips.Count; i++)
            {
                TooltipLine line = tooltips[i];
                if (match(line))
                {
                    index = i;
                    tooltip = line;
                    return true;
                }
            }
            index = -1;
            tooltip = null;
            return false;
        }

        public bool TryFindVanillaTooltipByName(string name, out int index, out TooltipLine tooltip) =>
            tooltips.TryFindTooltip(l => l.Mod == "Terraria" && l.Name == name, out index, out tooltip);

        public void ModifyTooltip(Func<TooltipLine, bool> match, Action<TooltipLine> action)
        {
            ArgumentNullException.ThrowIfNull(action);
            if (tooltips.TryFindTooltip(match, out _, out TooltipLine tooltip))
                action(tooltip);
        }

        public void ModifyVanillaTooltipByName(string name, Action<TooltipLine> action) =>
            tooltips.ModifyTooltip(l => l.Mod == "Terraria" && l.Name == name, action);

        public void ModifyTooltipByNum(int num, Action<TooltipLine> action) =>
            tooltips.ModifyVanillaTooltipByName($"Tooltip{num}", action);
    }
}