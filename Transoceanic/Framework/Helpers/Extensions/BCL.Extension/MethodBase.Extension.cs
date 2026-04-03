namespace Transoceanic.Framework.Helpers;

public static partial class TOExtensions
{
    extension(MethodBase method)
    {
        public Type[] ParameterTypes => [.. method.GetParameters().Select(p => p.ParameterType)];

        public string RealName => method.IsGenericMethod
            ? method.Name + "<" + string.Join(", ", method.GetGenericArguments().Select(t => t.Name)) + ">"
            : method.Name;

        public string RealNameWithParameter => method.RealName + "(" + string.Join(", ", method.ParameterTypes.Select(t => t.Name)) + ")";

        public string TypedName => method.DeclaringType.NestedName + method.Name;

        public string TypedRealName => method.DeclaringType.NestedRealName + method.RealName;

        public string TypedRealNameWithParameter => method.DeclaringType.NestedRealName + method.RealNameWithParameter;

        public MethodAttributes AccessLevel => method.Attributes & MethodAttributes.MemberAccessMask;

        public bool CanBeAccessedOutsideAssembly => method.AccessLevel is MethodAttributes.Public or MethodAttributes.Family or MethodAttributes.FamORAssem;

        public bool TryGetPropertyAccessor(out PropertyInfo property) => (property =
            method.IsSpecialName && (method.Name.StartsWith("get_") || method.Name.StartsWith("set_"))
            ? method.DeclaringType.GetProperty(method.Name[4..], TOReflectionUtils.UniversalBindingFlags)
            : null) is not null;

        public bool IsPropertyAccessor => method.TryGetPropertyAccessor(out _);

        public bool TryGetEventAccessor(out EventInfo eventInfo) => (eventInfo =
            method.IsSpecialName ? (
                method.Name.StartsWith("add_") ? method.DeclaringType.GetEvent(method.Name[4..], TOReflectionUtils.UniversalBindingFlags)
                : method.Name.StartsWith("remove_") ? method.DeclaringType.GetEvent(method.Name[7..], TOReflectionUtils.UniversalBindingFlags)
                : null
            ) : null) is not null;

        public bool IsEventAccessor => method.TryGetEventAccessor(out _);

        /// <summary>
        /// 判定方法是否密封（无法被重写）。
        /// </summary>
        public bool IsNotVirtial => !method.IsVirtual || method.IsFinal;
    }
}