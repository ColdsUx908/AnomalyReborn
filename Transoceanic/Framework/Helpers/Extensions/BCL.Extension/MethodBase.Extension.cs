// Developed by ColdsUx

namespace Transoceanic.Framework.Helpers;

public static partial class TOExtensions
{
    extension(MethodBase method)
    {
        /// <summary>
        /// 获取该方法所有参数的类型数组。
        /// </summary>
        public Type[] ParameterTypes => [.. method.GetParameters().Select(p => p.ParameterType)];

        /// <summary>
        /// 获取方法的真实名称。
        /// 如果方法是泛型方法，则返回形如 "MethodName&lt;T1, T2&gt;" 的字符串；否则返回方法名。
        /// </summary>
        public string RealName => method.IsGenericMethod
            ? method.Name + "<" + string.Join(", ", method.GetGenericArguments().Select(t => t.Name)) + ">"
            : method.Name;

        /// <summary>
        /// 获取包含参数类型信息的方法完整名称。
        /// 格式示例："MethodName&lt;T&gt;(Int32, String)"。
        /// </summary>
        public string RealNameWithParameter => method.RealName + "(" + string.Join(", ", method.ParameterTypes.Select(t => t.Name)) + ")";

        /// <summary>
        /// 获取带声明类型嵌套名称前缀的方法名。
        /// 例如："OuterType.InnerType.MethodName"。
        /// </summary>
        public string TypedName => method.DeclaringType.NestedName + method.Name;

        /// <summary>
        /// 获取带声明类型嵌套真实名称前缀的方法真实名称。
        /// 例如："OuterType.InnerType.MethodName&lt;T&gt;"。
        /// </summary>
        public string TypedRealName => method.DeclaringType.NestedRealName + method.RealName;

        /// <summary>
        /// 获取带声明类型嵌套真实名称前缀且包含参数信息的方法完整名称。
        /// 例如："OuterType.InnerType.MethodName&lt;T&gt;(Int32, String)"。
        /// </summary>
        public string TypedRealNameWithParameter => method.DeclaringType.NestedRealName + method.RealNameWithParameter;

        /// <summary>
        /// 获取该方法的访问级别标志。
        /// </summary>
        public MethodAttributes AccessLevel => method.Attributes & MethodAttributes.MemberAccessMask;

        /// <summary>
        /// 获取一个值，指示该方法是否可以从定义程序集外部访问。
        /// 当访问级别为 Public、Family 或 FamORAssem 时返回 <see langword="true"/>。
        /// </summary>
        public bool CanBeAccessedOutsideAssembly => method.AccessLevel is MethodAttributes.Public or MethodAttributes.Family or MethodAttributes.FamORAssem;

        /// <summary>
        /// 尝试获取与该访问器方法关联的属性信息。
        /// </summary>
        /// <param name="property">
        /// 当此方法返回时，如果该方法是一个属性访问器（get_* 或 set_*），则包含关联的属性信息；否则包含 <see langword="null"/>。
        /// </param>
        /// <returns>
        /// 如果成功获取关联属性，则为 <see langword="true"/>；否则为 <see langword="false"/>。
        /// </returns>
        public bool TryGetPropertyAccessor(out PropertyInfo property) => (property =
            method.IsSpecialName && (method.Name.StartsWith("get_") || method.Name.StartsWith("set_"))
            ? method.DeclaringType.GetProperty(method.Name[4..], TOReflectionUtils.UniversalBindingFlags)
            : null) is not null;

        /// <summary>
        /// 获取一个值，指示该方法是否为属性访问器（get 或 set 方法）。
        /// </summary>
        public bool IsPropertyAccessor => method.TryGetPropertyAccessor(out _);

        /// <summary>
        /// 尝试获取与该访问器方法关联的事件信息。
        /// </summary>
        /// <param name="eventInfo">
        /// 当此方法返回时，如果该方法是一个事件访问器（add_* 或 remove_*），则包含关联的事件信息；否则包含 <see langword="null"/>。
        /// </param>
        /// <returns>
        /// 如果成功获取关联事件，则为 <see langword="true"/>；否则为 <see langword="false"/>。
        /// </returns>
        public bool TryGetEventAccessor(out EventInfo eventInfo) => (eventInfo =
            method.IsSpecialName ? (
                method.Name.StartsWith("add_") ? method.DeclaringType.GetEvent(method.Name[4..], TOReflectionUtils.UniversalBindingFlags)
                : method.Name.StartsWith("remove_") ? method.DeclaringType.GetEvent(method.Name[7..], TOReflectionUtils.UniversalBindingFlags)
                : null
            ) : null) is not null;

        /// <summary>
        /// 获取一个值，指示该方法是否为事件访问器（add 或 remove 方法）。
        /// </summary>
        public bool IsEventAccessor => method.TryGetEventAccessor(out _);

        /// <summary>
        /// 判定方法是否密封（无法被重写）。
        /// <br/>具体逻辑：方法非虚，或者虽是虚方法但标记为 final（即 C# 中的 <see langword="sealed override"/>，或隐式实现的接口方法）。
        /// </summary>
        public bool IsNotVirtial => !method.IsVirtual || method.IsFinal;
    }
}