// Developed by ColdsUx

namespace CalamityAnomalies.ModCompatibility.CalamityBridge;

public static class CalamityReflectionHelper
{
    public static Dictionary<string, Type[]> CalamityTypes => field ??= CalamityMod_Publicizer.Instance.Code.GetTypes().GroupBy(t => t.Name).ToDictionary(g => g.Key, g => g.ToArray());
    public static Dictionary<string, Type> CalamityTypesByFullName => field ??= CalamityMod_Publicizer.Instance.Code.GetTypes().ToDictionary(t => t.FullName, t => t);

    public static Type GetCalamityType(string typeName)
    {
        if (CalamityTypes.TryGetValue(typeName, out Type[] types))
        {
            if (types.Length == 1)
                return types[0];
            else
                throw new ArgumentException($"More than one Calamity types '{typeName}' have been found.", nameof(typeName));
        }
        else
            throw new ArgumentException($"Type '{typeName}' is not found in Calamity types.", nameof(typeName));
    }

    public static Type GetCalamityTypeByFullName(string fullTypeName)
    {
        if (CalamityTypesByFullName.TryGetValue(fullTypeName, out Type type))
            return type;
        else
            throw new ArgumentException($"Type '{fullTypeName}' is not found in Calamity types.", nameof(fullTypeName));
    }
}
