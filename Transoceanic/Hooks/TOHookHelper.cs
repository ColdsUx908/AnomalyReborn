// Developed by ColdsUx

namespace Transoceanic.Hooks;

internal static class TOHookHelper
{
    /// <summary>
    /// 通过反射清空类中所有以 <c>_handler_</c> 开头的静态字段，
    /// 将它们重置为对应泛型列表类型的新实例。
    /// </summary>
    /// <remarks>
    /// <para>该方法会查找所有静态字段（依据 <c>TOReflectionUtils.StaticBindingFlags</c>），
    /// 对名称前缀为 <c>_handler_</c> 且类型为 <see cref="List{T}"/> 的字段，
    /// 创建新的空列表实例并赋值，从而移除所有已注册的事件处理程序。</para>
    /// <para>若字段类型是某种具体的泛型列表（如 <c>List&lt;Func&lt;NPC, bool&gt;&gt;</c>），直接创建其实例；
    /// 若字段类型是泛型定义等非具体类型，则通过反射构造对应的 <see cref="List{T}"/> 实例。</para>
    /// </remarks>
    public static void ResetHandlerFields(Type type)
    {
        foreach (FieldInfo field in type.GetFields(TOReflectionUtils.StaticBindingFlags))
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
}