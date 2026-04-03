using System.Linq.Expressions;

namespace Transoceanic.Framework.Helpers;

public static class TOReflectionUtils
{
    public static Assembly Assembly => field ??= TOMain.Instance.Code;
    public static Assembly TerrariaAssembly => field ??= typeof(Main).Assembly;
    public static Dictionary<string, Type[]> TerrariaTypes => field ??= TerrariaAssembly.GetTypes().GroupBy(t => t.Name).ToDictionary(g => g.Key, g => g.ToArray());
    public static Dictionary<string, Type> TerrariaTypesByFullName => field ??= TerrariaAssembly.GetTypes().ToDictionary(t => t.FullName, t => t);

    public static Type GetTerrariaType(string typeName)
    {
        if (TerrariaTypes.TryGetValue(typeName, out Type[] types))
        {
            if (types.Length == 1)
                return types[0];
            else
                throw new ArgumentException($"More than one Terraria types '{typeName}' has been found.", nameof(typeName));
        }
        else
            throw new ArgumentException($"Type '{typeName}' not found in Terraria types.", nameof(typeName));
    }

    public static Type GetTerrariaTypeByFullName(string fullTypeName)
    {
        if (TerrariaTypesByFullName.TryGetValue(fullTypeName, out Type type))
            return type;
        else
            throw new ArgumentException($"Type '{fullTypeName}' not found in Terraria types.", nameof(fullTypeName));
    }

    /// <summary>
    /// 包含所有所需Flag。
    /// </summary>
    public const BindingFlags UniversalBindingFlags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public;

    /// <summary>
    /// 包含实例成员所需Flag。
    /// </summary>
    public const BindingFlags InstanceBindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

    /// <summary>
    /// 包含静态成员所需Flag。
    /// </summary>
    public const BindingFlags StaticBindingFlags = BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public;

    public static readonly string[] ObjectMethods =
    [
        nameof(Equals),
        nameof(GetHashCode),
        nameof(GetType),
        nameof(ToString),
        nameof(MemberwiseClone)
    ];

    public static object CreateInstanceSafe(Type type, bool notInitialize = false)
    {
        if (type.IsAbstract || type.IsInterface)
            return null;

        if (type.IsValueType || (type.HasParameterlessConstructor && !notInitialize))
        {
            try { return Activator.CreateInstance(type, true); }
            catch (TargetInvocationException) { return RuntimeHelpers.GetUninitializedObject(type); }
        }
        else
            return RuntimeHelpers.GetUninitializedObject(type);
    }

    public static T CreateInstanceSafe<T>() where T : class => (T)CreateInstanceSafe(typeof(T));

    public static bool TryCreateInstanceSafe(Type type, out object instance) => (instance = CreateInstanceSafe(type)) is not null;

    public static bool TryCreateInstanceSafe<T>(out T instance) where T : class
    {
        if (TryCreateInstanceSafe(typeof(T), out object obj) && obj is T t)
        {
            instance = t;
            return true;
        }
        else
        {
            instance = null;
            return false;
        }
    }

    /// <summary>
    /// 获取能被tML加载的所有类型。
    /// </summary>
    /// <returns></returns>
    public static IEnumerable<Type> GetAllTypes() =>
        from mod in ModLoader.Mods
        from type in AssemblyManager.GetLoadableTypes(mod.Code)
        select type;

    /// <summary>
    /// 获取指定基类型在指定程序集中的所有派生类或实现类。
    /// </summary>
    /// <param name="baseType"></param>
    /// <param name="assemblyToSearch"></param>
    /// <returns></returns>
    public static IEnumerable<Type> GetTypesDerivedFrom(Type baseType, Assembly assemblyToSearch) =>
        from type in AssemblyManager.GetLoadableTypes(assemblyToSearch)
        where baseType.IsAssignableTo(type) && !type.IsAbstract
        select type;

    /// <summary>
    /// 获取指定基类型的所有派生类或实现类（非抽象）。
    /// <br/>检测范围为所有已加载的Mod。
    /// <para/>使用该方法的加载器应在 <see cref="Mod.PostSetupContent"/> 中调用。
    /// </summary>
    /// <param name="baseType"></param>
    /// <returns></returns>
    public static IEnumerable<Type> GetTypesDerivedFrom(Type baseType) =>
        from type in GetAllTypes()
        where baseType.IsAssignableTo(type) && !type.IsAbstract
        select type;

    /// <summary>
    /// 获取指定基类型在指定程序集中的所有派生类或实现类（非抽象）。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="assemblyToSearch"></param>
    /// <returns></returns>
    public static IEnumerable<Type> GetTypesDerivedFrom<T>(Assembly assemblyToSearch) => GetTypesDerivedFrom(typeof(T), assemblyToSearch);

    /// <summary>
    /// 获取指定基类型的所有派生类或实现类（非抽象）。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static IEnumerable<Type> GetTypesDerivedFrom<T>() => GetTypesDerivedFrom(typeof(T));

    /// <summary>
    /// 获取指定基类型在指定程序集中的所有派生类或实现类（非抽象）的对应实例。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="assemblyToSearch"></param>
    /// <returns></returns>
    public static IEnumerable<T> GetTypeInstancesDerivedFrom<T>(Assembly assemblyToSearch, bool notInitialize = false) where T : class =>
        AssemblyManager.GetLoadableTypes(assemblyToSearch)
        .Where(type => type.IsAssignableTo(typeof(T)) && !type.IsAbstract)
        .Select(type => (T)CreateInstanceSafe(type, notInitialize))
        .Where(instance => instance is not null);

    /// <summary>
    /// 获取指定基类型的所有派生类或实现类（非抽象）的对应实例。
    /// <br/>检测范围为所有已加载的Mod。
    /// <para/>使用该方法的加载器应在 <see cref="Mod.PostSetupContent"/> 中调用。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static IEnumerable<T> GetTypeInstancesDerivedFrom<T>(bool notInitialize = false) where T : class =>
        GetAllTypes().Where(type => type.IsAssignableTo(typeof(T)) && !type.IsAbstract)
        .Select(type => (T)CreateInstanceSafe(type, notInitialize))
        .Where(instance => instance is not null);

    /// <summary>
    /// 获取指定基类型在指定程序集中的所有派生类或实现类（非抽象）及其对应实例。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="assemblyToSearch"></param>
    /// <returns></returns>
    public static IEnumerable<(Type Type, T Instance)> GetTypesAndInstancesDerivedFrom<T>(Assembly assemblyToSearch, bool notInitialize = false) where T : class =>
        AssemblyManager.GetLoadableTypes(assemblyToSearch).Where(type => type.IsAssignableTo(typeof(T)) && !type.IsAbstract)
        .Select(type => (type, instance: (T)CreateInstanceSafe(type, notInitialize)))
        .Where(pair => pair.instance is not null);

    /// <summary>
    /// 获取指定基类型的所有派生类或实现类（非抽象）及其对应实例。
    /// <br/>检测范围为所有已加载的Mod。
    /// <para/>使用该方法的加载器应在 <see cref="Mod.PostSetupContent"/> 中调用。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static IEnumerable<(Type type, T instance)> GetTypesAndInstancesDerivedFrom<T>(bool notInitialize = false) where T : class =>
        GetAllTypes().Where(type => type.IsAssignableTo(typeof(T)) && !type.IsAbstract)
        .Select(type => (type, instance: (T)CreateInstanceSafe(type, notInitialize))).Where(pair => pair.instance is not null);

    /// <summary>
    /// 获取指定程序集中所有指定特性修饰的类型及对应特性实例。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="assemblyToSearch"></param>
    /// <returns></returns>
    public static IEnumerable<(Type type, T attribute)> GetTypesWithAttribute<T>(Assembly assemblyToSearch, bool inherit = true) where T : Attribute =>
        AssemblyManager.GetLoadableTypes(assemblyToSearch)
        .Select(type => (type, attribute: type.Attribute<T>(inherit))).Where(pair => pair.attribute is not null);

    /// <summary>
    /// 获取指定程序集中所有指定特性修饰的类型及对应特性实例。
    /// <br/>检测范围为所有已加载的Mod。
    /// <para/>使用该方法的加载器应在 <see cref="Mod.PostSetupContent"/> 中调用。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static IEnumerable<(Type type, T attribute)> GetTypesWithAttribute<T>(bool inherit = true) where T : Attribute => GetAllTypes()
        .Select(type => (type, attribute: type.Attribute<T>(inherit))).Where(pair => pair.attribute is not null);

    /// <summary>
    /// 获取指定程序集中所有指定特性修饰的方法及对应特性实例。
    /// </summary>
    public static IEnumerable<(MethodInfo method, T attribute)> GetMethodsWithAttribute<T>(Assembly assemblyToSearch, bool inherit = true) where T : Attribute =>
        AssemblyManager.GetLoadableTypes(assemblyToSearch).SelectMany(type => type.GetRealMethods(UniversalBindingFlags)
        .Select(method => (method, attribute: method.Attribute<T>(inherit))).Where(pair => pair.attribute is not null));

    /// <summary>
    /// 获取指定程序集中所有指定特性修饰的方法及对应特性实例。
    /// <br/>检测范围为所有已加载的Mod。
    /// <para/>使用该方法的加载器应在 <see cref="Mod.PostSetupContent"/> 中调用。
    public static IEnumerable<(MethodInfo method, T attribute)> GetMethodsWithAttribute<T>(bool inherit = true) where T : Attribute =>
        GetAllTypes().SelectMany(type => type.GetRealMethods(UniversalBindingFlags)
        .Select(method => (method, attribute: method.Attribute<T>(inherit))).Where(pair => pair.attribute is not null));

    /// <summary>
    /// 获取指定程序集中所有指定特性修饰的成员及对应特性实例。
    /// <br/>检测范围为所有已加载的Mod。
    /// <para/>使用该方法的加载器应在 <see cref="Mod.PostSetupContent"/> 中调用。
    public static IEnumerable<(TMember member, TAttribute attribute)> GetMembersWithAttribute<TMember, TAttribute>(bool inherit = true)
        where TMember : MemberInfo
        where TAttribute : Attribute =>
        GetAllTypes().SelectMany(type => type.GetMembers(UniversalBindingFlags)).OfType<TMember>()
        .Select(member => (member, attribute: member.Attribute<TAttribute>(inherit))).Where(pair => pair.attribute is not null);

    public static void SetStructField<T>(ref T target, FieldInfo field, object value) where T : struct
    {
        object boxed = target;
        field.SetValue(boxed, value);
        target = (T)boxed;
    }

    public static Type GetDelegateType(MethodInfo method)
    {
        ParameterInfo[] parameters = method.GetParameters();
        if (method.ReturnType == typeof(void)) //Action
        {
            Type[] actionTypes = [.. parameters.Select(p => p.ParameterType)];
            if (actionTypes.Length == 0)
                return typeof(Action);
            return Expression.GetActionType(actionTypes);
        }
        else //Func
        {
            Type[] funcTypes = [.. parameters.Select(p => p.ParameterType), method.ReturnType];
            return Expression.GetFuncType(funcTypes);
        }
    }

    public static Delegate CreateMethodDelegate(MethodInfo method) => Delegate.CreateDelegate(GetDelegateType(method), method);

    public static object GetDefaultValue(Type type) => (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>)) || !type.IsValueType ? null : Activator.CreateInstance(type);

    public static bool TryAssignSingleInstance(object instance)
    {
        ArgumentNullException.ThrowIfNull(instance);

        Type instanceType = instance.GetType();

        FieldInfo instanceField = instanceType.GetField("Instance", StaticBindingFlags);
        if (instanceField is not null && instanceField.FieldType.IsAssignableFrom(instanceType))
        {
            instanceField.SetValue(null, instance);
            return true;
        }

        PropertyInfo instanceProperty = instanceType.GetProperty("Instance", StaticBindingFlags);
        if (instanceProperty is not null && instanceProperty.CanWrite && instanceProperty.PropertyType.IsAssignableFrom(instanceType))
        {
            instanceProperty.SetValue(null, instance);
            return true;
        }

        return false;
    }

    public static bool TryResetSingleInstance(Type type)
    {
        ArgumentNullException.ThrowIfNull(type);

        FieldInfo instanceField = type.GetField("Instance", StaticBindingFlags);
        if (instanceField is not null)
        {
            instanceField.SetValue(null, GetDefaultValue(instanceField.FieldType));
            return true;
        }

        PropertyInfo instanceProperty = type.GetProperty("Instance", StaticBindingFlags);
        if (instanceProperty is not null)
        {
            instanceProperty.SetValue(null, GetDefaultValue(instanceProperty.PropertyType));
            return true;
        }

        return false;
    }
}
