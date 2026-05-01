// Developed by ColdsUx

using System.Linq.Expressions;

namespace Transoceanic.Framework.Helpers;

/// <summary>
/// 提供高级反射操作、类型检索与实例化等工具方法，用于动态加载或修改游戏内容。
/// </summary>
public static class TOReflectionUtils
{
    /// <summary>
    /// 获取 Transoceanic Mod 的程序集。
    /// </summary>
    public static Assembly Assembly => field ??= TOMain.Instance.Code;
    /// <summary>
    /// 获取泰拉瑞亚主程序集（Terraria.dll）。
    /// </summary>
    public static Assembly TerrariaAssembly => field ??= typeof(Main).Assembly;
    /// <summary>
    /// 获取泰拉瑞亚程序集中按名称分组的类型字典（键为类型短名称，值为可能的类型数组，用于处理同名类型）。
    /// </summary>
    public static Dictionary<string, Type[]> TerrariaTypes => field ??= TerrariaAssembly.GetTypes().GroupBy(t => t.Name).ToDictionary(g => g.Key, g => g.ToArray());
    /// <summary>
    /// 获取泰拉瑞亚程序集中按完整名称索引的类型字典。
    /// </summary>
    public static Dictionary<string, Type> TerrariaTypesByFullName => field ??= TerrariaAssembly.GetTypes().ToDictionary(t => t.FullName, t => t);

    /// <summary>
    /// 通过类型短名称获取泰拉瑞亚程序集中的唯一类型。
    /// </summary>
    /// <param name="typeName">类型短名称（不含命名空间）。</param>
    /// <returns>对应的 <see cref="Type"/> 实例。</returns>
    /// <exception cref="ArgumentException">当未找到指定名称的类型，或存在多个同名类型时抛出。</exception>
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

    /// <summary>
    /// 通过类型完整名称获取泰拉瑞亚程序集中的类型。
    /// </summary>
    /// <param name="fullTypeName">类型的完整名称（含命名空间）。</param>
    /// <returns>对应的 <see cref="Type"/> 实例。</returns>
    /// <exception cref="ArgumentException">当未找到指定完整名称的类型时抛出。</exception>
    public static Type GetTerrariaTypeByFullName(string fullTypeName)
    {
        if (TerrariaTypesByFullName.TryGetValue(fullTypeName, out Type type))
            return type;
        else
            throw new ArgumentException($"Type '{fullTypeName}' not found in Terraria types.", nameof(fullTypeName));
    }

    /// <summary>
    /// 通用的绑定标志，包含实例、静态、非公开、公开成员。
    /// </summary>
    public const BindingFlags UniversalBindingFlags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public;

    /// <summary>
    /// 实例成员的绑定标志，包含非公开、公开。
    /// </summary>
    public const BindingFlags InstanceBindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

    /// <summary>
    /// 静态成员的绑定标志，包含非公开、公开。
    /// </summary>
    public const BindingFlags StaticBindingFlags = BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public;

    /// <summary>
    /// 基类 <see cref="object"/> 的方法名称列表，用于在反射遍历时过滤。
    /// </summary>
    public static readonly string[] ObjectMethods =
    [
        nameof(Equals),
        nameof(GetHashCode),
        nameof(GetType),
        nameof(ToString),
        nameof(MemberwiseClone)
    ];

    /// <summary>
    /// 安全地创建指定类型的实例，绕过访问修饰符限制，并处理抽象类、接口、值类型以及无参构造函数缺失的情况。
    /// </summary>
    /// <param name="type">要实例化的类型。</param>
    /// <param name="notInitialize">若为 <see langword="true"/>，则强制使用 <see cref="RuntimeHelpers.GetUninitializedObject"/> 创建未初始化实例，忽略默认构造函数。</param>
    /// <returns>创建的实例；若类型为抽象类或接口，则返回 <see langword="null"/>。</returns>
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

    /// <summary>
    /// 安全地创建指定类型的实例（泛型版本）。
    /// </summary>
    /// <typeparam name="T">要实例化的类型（必须是引用类型）。</typeparam>
    /// <returns>创建的实例；若失败则返回 <see langword="null"/>。</returns>
    public static T CreateInstanceSafe<T>() where T : class => (T)CreateInstanceSafe(typeof(T));

    /// <summary>
    /// 尝试安全地创建指定类型的实例。
    /// </summary>
    /// <param name="type">要实例化的类型。</param>
    /// <param name="instance">输出的实例。</param>
    /// <returns>若成功创建非空实例，返回 <see langword="true"/>；否则返回 <see langword="false"/>。</returns>
    public static bool TryCreateInstanceSafe(Type type, out object instance) => (instance = CreateInstanceSafe(type)) is not null;

    /// <summary>
    /// 尝试安全地创建指定类型的实例（泛型版本）。
    /// </summary>
    /// <typeparam name="T">要实例化的类型（必须是引用类型）。</typeparam>
    /// <param name="instance">输出的实例。</param>
    /// <returns>若成功创建非空实例，返回 <see langword="true"/>；否则返回 <see langword="false"/>。</returns>
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
    /// 获取能被 tModLoader 加载的所有类型（包含所有已加载 Mod 的程序集中的类型）。
    /// </summary>
    /// <returns>一个包含所有可加载类型的枚举集合。</returns>
    public static IEnumerable<Type> GetAllTypes() =>
        from mod in ModLoader.Mods
        from type in AssemblyManager.GetLoadableTypes(mod.Code)
        select type;

    /// <summary>
    /// 获取指定基类型在指定程序集中的所有派生类或实现类（非抽象）。
    /// </summary>
    /// <param name="baseType">基类型或接口类型。</param>
    /// <param name="assemblyToSearch">要检索的程序集。</param>
    /// <returns>符合条件的类型枚举。</returns>
    public static IEnumerable<Type> GetTypesDerivedFrom(Type baseType, Assembly assemblyToSearch) =>
        from type in AssemblyManager.GetLoadableTypes(assemblyToSearch)
        where baseType.IsAssignableTo(type) && !type.IsAbstract
        select type;

    /// <summary>
    /// 获取指定基类型的所有派生类或实现类（非抽象）。检索范围为所有已加载的 Mod。
    /// </summary>
    /// <param name="baseType">基类型或接口类型。</param>
    /// <returns>符合条件的类型枚举。</returns>
    /// <remarks>使用此方法的加载器应在 <see cref="Mod.PostSetupContent"/> 中调用，以确保所有 Mod 的类型均已加载。</remarks>
    public static IEnumerable<Type> GetTypesDerivedFrom(Type baseType) =>
        from type in GetAllTypes()
        where baseType.IsAssignableTo(type) && !type.IsAbstract
        select type;

    /// <summary>
    /// 获取指定基类型在指定程序集中的所有派生类或实现类（非抽象，泛型版本）。
    /// </summary>
    /// <typeparam name="T">基类型或接口类型。</typeparam>
    /// <param name="assemblyToSearch">要检索的程序集。</param>
    /// <returns>符合条件的类型枚举。</returns>
    public static IEnumerable<Type> GetTypesDerivedFrom<T>(Assembly assemblyToSearch) => GetTypesDerivedFrom(typeof(T), assemblyToSearch);

    /// <summary>
    /// 获取指定基类型的所有派生类或实现类（非抽象，泛型版本）。检索范围为所有已加载的 Mod。
    /// </summary>
    /// <typeparam name="T">基类型或接口类型。</typeparam>
    /// <returns>符合条件的类型枚举。</returns>
    /// <remarks>使用此方法的加载器应在 <see cref="Mod.PostSetupContent"/> 中调用。</remarks>
    public static IEnumerable<Type> GetTypesDerivedFrom<T>() => GetTypesDerivedFrom(typeof(T));

    /// <summary>
    /// 获取指定基类型在指定程序集中的所有派生类或实现类（非抽象）的对应实例。
    /// </summary>
    /// <typeparam name="T">基类型或接口类型（引用类型）。</typeparam>
    /// <param name="assemblyToSearch">要检索的程序集。</param>
    /// <param name="notInitialize">是否创建未初始化的实例（跳过构造函数）。</param>
    /// <returns>符合条件的类型实例枚举。</returns>
    public static IEnumerable<T> GetTypeInstancesDerivedFrom<T>(Assembly assemblyToSearch, bool notInitialize = false) where T : class =>
        AssemblyManager.GetLoadableTypes(assemblyToSearch)
        .Where(type => type.IsAssignableTo(typeof(T)) && !type.IsAbstract)
        .Select(type => (T)CreateInstanceSafe(type, notInitialize))
        .Where(instance => instance is not null);

    /// <summary>
    /// 获取指定基类型的所有派生类或实现类（非抽象）的对应实例。检索范围为所有已加载的 Mod。
    /// </summary>
    /// <typeparam name="T">基类型或接口类型（引用类型）。</typeparam>
    /// <param name="notInitialize">是否创建未初始化的实例（跳过构造函数）。</param>
    /// <returns>符合条件的类型实例枚举。</returns>
    /// <remarks>使用此方法的加载器应在 <see cref="Mod.PostSetupContent"/> 中调用。</remarks>
    public static IEnumerable<T> GetTypeInstancesDerivedFrom<T>(bool notInitialize = false) where T : class =>
        GetAllTypes().Where(type => type.IsAssignableTo(typeof(T)) && !type.IsAbstract)
        .Select(type => (T)CreateInstanceSafe(type, notInitialize))
        .Where(instance => instance is not null);

    /// <summary>
    /// 获取指定基类型在指定程序集中的所有派生类或实现类（非抽象）及其对应实例。
    /// </summary>
    /// <typeparam name="T">基类型或接口类型（引用类型）。</typeparam>
    /// <param name="assemblyToSearch">要检索的程序集。</param>
    /// <param name="notInitialize">是否创建未初始化的实例（跳过构造函数）。</param>
    /// <returns>包含类型与实例的元组枚举。</returns>
    public static IEnumerable<(Type Type, T Instance)> GetTypesAndInstancesDerivedFrom<T>(Assembly assemblyToSearch, bool notInitialize = false) where T : class =>
        AssemblyManager.GetLoadableTypes(assemblyToSearch).Where(type => type.IsAssignableTo(typeof(T)) && !type.IsAbstract)
        .Select(type => (type, instance: (T)CreateInstanceSafe(type, notInitialize)))
        .Where(pair => pair.instance is not null);

    /// <summary>
    /// 获取指定基类型的所有派生类或实现类（非抽象）及其对应实例。检索范围为所有已加载的 Mod。
    /// </summary>
    /// <typeparam name="T">基类型或接口类型（引用类型）。</typeparam>
    /// <param name="notInitialize">是否创建未初始化的实例（跳过构造函数）。</param>
    /// <returns>包含类型与实例的元组枚举。</returns>
    /// <remarks>使用此方法的加载器应在 <see cref="Mod.PostSetupContent"/> 中调用。</remarks>
    public static IEnumerable<(Type type, T instance)> GetTypesAndInstancesDerivedFrom<T>(bool notInitialize = false) where T : class =>
        GetAllTypes().Where(type => type.IsAssignableTo(typeof(T)) && !type.IsAbstract)
        .Select(type => (type, instance: (T)CreateInstanceSafe(type, notInitialize))).Where(pair => pair.instance is not null);

    /// <summary>
    /// 获取指定程序集中所有被指定特性修饰的类型及对应特性实例。
    /// </summary>
    /// <typeparam name="T">要检索的特性类型。</typeparam>
    /// <param name="assemblyToSearch">要检索的程序集。</param>
    /// <param name="inherit">是否搜索继承链上的特性。</param>
    /// <returns>包含类型与特性实例的元组枚举。</returns>
    public static IEnumerable<(Type type, T attribute)> GetTypesWithAttribute<T>(Assembly assemblyToSearch, bool inherit = true) where T : Attribute =>
        AssemblyManager.GetLoadableTypes(assemblyToSearch)
        .Select(type => (type, attribute: type.Attribute<T>(inherit))).Where(pair => pair.attribute is not null);

    /// <summary>
    /// 获取所有被指定特性修饰的类型及对应特性实例。检索范围为所有已加载的 Mod。
    /// </summary>
    /// <typeparam name="T">要检索的特性类型。</typeparam>
    /// <param name="inherit">是否搜索继承链上的特性。</param>
    /// <returns>包含类型与特性实例的元组枚举。</returns>
    /// <remarks>使用此方法的加载器应在 <see cref="Mod.PostSetupContent"/> 中调用。</remarks>
    public static IEnumerable<(Type type, T attribute)> GetTypesWithAttribute<T>(bool inherit = true) where T : Attribute => GetAllTypes()
        .Select(type => (type, attribute: type.Attribute<T>(inherit))).Where(pair => pair.attribute is not null);

    /// <summary>
    /// 获取指定程序集中所有被指定特性修饰的方法及对应特性实例。
    /// </summary>
    /// <typeparam name="T">要检索的特性类型。</typeparam>
    /// <param name="assemblyToSearch">要检索的程序集。</param>
    /// <param name="inherit">是否搜索继承链上的特性。</param>
    /// <returns>包含方法信息与特性实例的元组枚举。</returns>
    public static IEnumerable<(MethodInfo method, T attribute)> GetMethodsWithAttribute<T>(Assembly assemblyToSearch, bool inherit = true) where T : Attribute =>
        AssemblyManager.GetLoadableTypes(assemblyToSearch).SelectMany(type => type.GetRealMethods(UniversalBindingFlags)
        .Select(method => (method, attribute: method.Attribute<T>(inherit))).Where(pair => pair.attribute is not null));

    /// <summary>
    /// 获取所有被指定特性修饰的方法及对应特性实例。检索范围为所有已加载的 Mod。
    /// </summary>
    /// <typeparam name="T">要检索的特性类型。</typeparam>
    /// <param name="inherit">是否搜索继承链上的特性。</param>
    /// <returns>包含方法信息与特性实例的元组枚举。</returns>
    /// <remarks>使用此方法的加载器应在 <see cref="Mod.PostSetupContent"/> 中调用。</remarks>
    public static IEnumerable<(MethodInfo method, T attribute)> GetMethodsWithAttribute<T>(bool inherit = true) where T : Attribute =>
        GetAllTypes().SelectMany(type => type.GetRealMethods(UniversalBindingFlags)
        .Select(method => (method, attribute: method.Attribute<T>(inherit))).Where(pair => pair.attribute is not null));

    /// <summary>
    /// 获取所有被指定特性修饰的成员及对应特性实例。检索范围为所有已加载的 Mod。
    /// </summary>
    /// <typeparam name="TMember">要检索的成员类型（如 <see cref="FieldInfo"/>、<see cref="PropertyInfo"/> 等）。</typeparam>
    /// <typeparam name="TAttribute">要检索的特性类型。</typeparam>
    /// <param name="inherit">是否搜索继承链上的特性。</param>
    /// <returns>包含成员信息与特性实例的元组枚举。</returns>
    /// <remarks>使用此方法的加载器应在 <see cref="Mod.PostSetupContent"/> 中调用。</remarks>
    public static IEnumerable<(TMember member, TAttribute attribute)> GetMembersWithAttribute<TMember, TAttribute>(bool inherit = true)
        where TMember : MemberInfo
        where TAttribute : Attribute =>
        GetAllTypes().SelectMany(type => type.GetMembers(UniversalBindingFlags)).OfType<TMember>()
        .Select(member => (member, attribute: member.Attribute<TAttribute>(inherit))).Where(pair => pair.attribute is not null);

    /// <summary>
    /// 设置结构体实例中的字段值（通过装箱拆箱方式，解决结构体字段反射赋值问题）。
    /// </summary>
    /// <typeparam name="T">结构体类型。</typeparam>
    /// <param name="target">要修改的结构体实例引用。</param>
    /// <param name="field">字段信息。</param>
    /// <param name="value">要设置的值。</param>
    public static void SetStructField<T>(ref T target, FieldInfo field, object value) where T : struct
    {
        object boxed = target;
        field.SetValue(boxed, value);
        target = (T)boxed;
    }

    /// <summary>
    /// 获取与方法签名相匹配的委托类型（<see cref="Action"/> 或 <see cref="Func{TResult}"/>）。
    /// </summary>
    /// <param name="method">方法信息。</param>
    /// <returns>对应的委托类型。</returns>
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

    /// <summary>
    /// 为指定的方法创建一个对应的委托实例。
    /// </summary>
    /// <param name="method">方法信息。</param>
    /// <returns>与方法签名匹配的委托。</returns>
    public static Delegate CreateMethodDelegate(MethodInfo method) => Delegate.CreateDelegate(GetDelegateType(method), method);

    /// <summary>
    /// 获取指定类型的默认值。
    /// </summary>
    /// <param name="type">类型。</param>
    /// <returns>若为引用类型或 <see cref="Nullable{T}"/>，返回 <see langword="null"/>；若为值类型，返回该类型的默认实例。</returns>
    public static object GetDefaultValue(Type type) => (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>)) || !type.IsValueType ? null : Activator.CreateInstance(type);

    /// <summary>
    /// 尝试将给定的实例赋值给其类型中名为 "Instance" 的静态字段或属性（用于单例模式支持）。
    /// </summary>
    /// <param name="instance">要赋值的实例。</param>
    /// <returns>若成功赋值，返回 <see langword="true"/>；否则返回 <see langword="false"/>。</returns>
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

    /// <summary>
    /// 尝试将指定类型中名为 "Instance" 的静态字段或属性重置为默认值。
    /// </summary>
    /// <param name="type">要操作的类型。</param>
    /// <returns>若成功重置，返回 <see langword="true"/>；否则返回 <see langword="false"/>。</returns>
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