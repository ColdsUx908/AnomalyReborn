#pragma warning disable IDE1006 // 命名样式

namespace Transoceanic.Hooks.Framework.Helpers;

public sealed class On_TOExtensions : IContentLoader
{
    private static List<Func<NPC, bool>> _handler_get_IsBossEnemy = [];
    public static event Func<NPC, bool> get_IsBossEnemy
    {
        add => _handler_get_IsBossEnemy.Add(value);
        remove => _handler_get_IsBossEnemy.Remove(value);
    }
    internal static bool Impl_get_IsBossEnemy(NPC npc)
    {
        if (_handler_get_IsBossEnemy.Count == 0)
            return false;

        foreach (Func<NPC, bool> handler in _handler_get_IsBossEnemy)
        {
            if (handler(npc))
                return true;
        }

        return false;
    }

    private static void ResetHandlerFields()
    {
        foreach (FieldInfo field in typeof(On_TOExtensions).GetFields(TOReflectionUtils.StaticBindingFlags))
        {
            if (field.Name.StartsWith("_handler_"))
            {
                Type fieldType = field.FieldType;
                if (fieldType.IsGenericType)
                {
                    Type genericDef = fieldType.GetGenericTypeDefinition();
                    Type concreteType;
                    if (genericDef == typeof(List<>))
                        concreteType = fieldType;
                    else
                    {
                        Type elementType = fieldType.GetGenericArguments()[0];
                        concreteType = typeof(List<>).MakeGenericType(elementType);
                    }
                    field.SetValue(null, Activator.CreateInstance(concreteType));
                }
            }
        }
    }

    [LoadPriority(1e5)]
    void IContentLoader.PostSetupContent() => ResetHandlerFields();

    void IContentLoader.OnModUnload() => ResetHandlerFields();
}
