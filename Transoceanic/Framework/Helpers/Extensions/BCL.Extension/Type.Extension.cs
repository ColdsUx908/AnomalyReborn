namespace Transoceanic.Framework.Helpers;

public static partial class TOExtensions
{
    extension(Type type)
    {
        public string RealName => type.IsGenericType
            ? type.Name[..type.Name.IndexOf('`')] + "<" + string.Join(", ", type.GetGenericArguments().Select(a => a.Name)) + ">"
            : type.Name;

        public string NestedName => type.DeclaringType is not null ? type.DeclaringType.NestedName + "." + type.Name : type.Name; //递归

        public string NestedRealName => type.DeclaringType is not null ? type.DeclaringType.NestedRealName + "." + type.RealName : type.RealName; //递归

        public IEnumerable<string> GetMethodNames(BindingFlags bindingAttr) => type.GetMethods(bindingAttr).Select(m => m.Name);

        public IEnumerable<string> GetMethodNamesExceptObject(BindingFlags bindingAttr)
        {
            foreach (MethodInfo method in type.GetMethods(bindingAttr))
            {
                string name = method.Name;
                if (!TOReflectionUtils.ObjectMethods.Contains(name))
                    yield return name;
            }
        }

        /// <summary>
        /// 检查指定类型中是否存在对应方法。
        /// </summary>
        public bool HasMethod(string methodName, BindingFlags bindingAttr, out MethodInfo methodInfo) =>
            (methodInfo = type.GetMethod(methodName, bindingAttr)) is not null;

        /// <summary>
        /// 获取指定类型中所有由该类型声明的方法。
        /// </summary>
        public MethodInfo[] GetRealMethods(BindingFlags bindingAttr) => type.GetMethods(bindingAttr | BindingFlags.DeclaredOnly);

        /// <summary>
        /// 检查指定类型是否声明了对应方法。
        /// </summary>
        public bool HasRealMethod(string methodName, BindingFlags bindingAttr, out MethodInfo methodInfo) =>
            type.HasMethod(methodName, bindingAttr | BindingFlags.DeclaredOnly, out methodInfo);

        /// <summary>
        /// 检查指定类型是否声明了对应方法。
        /// </summary>
        public bool HasRealMethod(string methodName, BindingFlags bindingAttr) =>
            type.HasRealMethod(methodName, bindingAttr, out _);

        /// <summary>
        /// 适用于 <paramref name="mainMethod"/> 依赖于 <paramref name="requiredMethod"/> 的情况，判定是否有正确的方法关系。
        /// </summary>
        /// <returns>仅在 <paramref name="mainMethod"/> 存在而 <paramref name="requiredMethod"/> 不存在时返回 <see langword="false"/>。</returns>
        public bool MustHaveRealMethodWith(string mainMethodName, string requiredMethodName, BindingFlags bindingAttr, out MethodInfo mainMethod, out MethodInfo requiredMethod) =>
            (type.HasRealMethod(mainMethodName, bindingAttr, out mainMethod), type.HasRealMethod(requiredMethodName, bindingAttr, out requiredMethod)) is not (true, false);

        /// <summary>
        /// 适用于 <paramref name="mainMethodName"/> 依赖于 <paramref name="requiredMethodName"/> 的情况，判定是否有正确的方法关系。
        /// </summary>
        /// <returns>仅在 <paramref name="mainMethodName"/> 存在而 <paramref name="requiredMethodName"/> 不存在时返回 <see langword="false"/>。</returns>
        public bool MustHaveRealMethodWith(string mainMethodName, string requiredMethodName, BindingFlags bindingAttr) =>
            type.MustHaveRealMethodWith(mainMethodName, requiredMethodName, bindingAttr, out _, out _);

        /// <summary>
        /// 获取指定类型中所有重写方法。
        /// </summary>
        public IEnumerable<MethodInfo> GetOverrideMethods(BindingFlags bindingAttr) => type.GetRealMethods(bindingAttr).Where(m => m.IsOverride);

        /// <summary>
        /// 获取指定类型中所有重写方法的名称。
        /// </summary>
        public IEnumerable<string> GetOverrideMethodNames(BindingFlags bindingAttr) => type.GetOverrideMethods(bindingAttr).Select(m => m.Name);

        public T Attribute<T>(bool inherit = true) where T : Attribute => type.GetCustomAttributes(typeof(T), inherit).OfType<T>().FirstOrDefault();

        public bool HasAttribute<T>(bool inherit = true) where T : Attribute => type.Attribute<T>(inherit) is not null;

        public bool TryGetAttribute<T>([NotNullWhen(true)] out T attribute, bool inherit = true) where T : Attribute => (attribute = type.Attribute<T>(inherit)) is not null;

        /// <summary>
        /// 类型是否具有无参数构造函数。
        /// </summary>
        public bool HasParameterlessConstructor => type.GetConstructor(TOReflectionUtils.InstanceBindingFlags, []) is not null;
    }
}