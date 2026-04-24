namespace Transoceanic.Framework.Helpers;

public static partial class TOExtensions
{
    extension(Type type)
    {
        /// <summary>
        /// 获取类型的真实名称。
        /// 如果类型是泛型类型，则返回形如 "List&lt;T&gt;" 的字符串；否则返回类型名。
        /// </summary>
        public string RealName => type.IsGenericType
            ? type.Name[..type.Name.IndexOf('`')] + "<" + string.Join(", ", type.GetGenericArguments().Select(a => a.Name)) + ">"
            : type.Name;

        /// <summary>
        /// 获取类型的嵌套名称，包含所有外部类型的名称，以点分隔。
        /// 例如："OuterType.InnerType"。
        /// </summary>
        public string NestedName => type.DeclaringType is not null ? type.DeclaringType.NestedName + "." + type.Name : type.Name; //递归

        /// <summary>
        /// 获取类型的嵌套真实名称，包含所有外部类型的真实名称，以点分隔。
        /// 例如："OuterType.InnerType&lt;T&gt;"。
        /// </summary>
        public string NestedRealName => type.DeclaringType is not null ? type.DeclaringType.NestedRealName + "." + type.RealName : type.RealName; //递归

        /// <summary>
        /// 获取该类型中符合指定绑定标志的所有方法的名称序列。
        /// </summary>
        /// <param name="bindingAttr">用于控制搜索方式的绑定标志。</param>
        /// <returns>方法名称的可枚举集合。</returns>
        public IEnumerable<string> GetMethodNames(BindingFlags bindingAttr) => type.GetMethods(bindingAttr).Select(m => m.Name);

        /// <summary>
        /// 获取该类型中符合指定绑定标志的所有方法的名称序列，但不包含继承自 <see cref="object"/> 的方法。
        /// </summary>
        /// <param name="bindingAttr">用于控制搜索方式的绑定标志。</param>
        /// <returns>方法名称的可枚举集合（不含 object 基类方法）。</returns>
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
        /// 检查指定类型中是否存在具有指定名称和绑定标志的方法。
        /// </summary>
        /// <param name="methodName">要搜索的方法名称。</param>
        /// <param name="bindingAttr">用于控制搜索方式的绑定标志。</param>
        /// <param name="methodInfo">
        /// 当此方法返回时，如果找到方法，则包含该方法的信息；否则包含 <see langword="null"/>。
        /// </param>
        /// <returns>如果找到方法，则为 <see langword="true"/>；否则为 <see langword="false"/>。</returns>
        public bool HasMethod(string methodName, BindingFlags bindingAttr, out MethodInfo methodInfo) =>
            (methodInfo = type.GetMethod(methodName, bindingAttr)) is not null;

        /// <summary>
        /// 获取指定类型中所有由该类型直接声明的方法（不包括继承的方法）。
        /// </summary>
        /// <param name="bindingAttr">用于控制搜索方式的绑定标志。</param>
        /// <returns>由该类型声明的方法数组。</returns>
        public MethodInfo[] GetRealMethods(BindingFlags bindingAttr) => type.GetMethods(bindingAttr | BindingFlags.DeclaredOnly);

        /// <summary>
        /// 检查指定类型是否直接声明了具有指定名称和绑定标志的方法。
        /// </summary>
        /// <param name="methodName">要搜索的方法名称。</param>
        /// <param name="bindingAttr">用于控制搜索方式的绑定标志。</param>
        /// <param name="methodInfo">
        /// 当此方法返回时，如果找到方法，则包含该方法的信息；否则包含 <see langword="null"/>。
        /// </param>
        /// <returns>如果该类型直接声明了指定方法，则为 <see langword="true"/>；否则为 <see langword="false"/>。</returns>
        public bool HasRealMethod(string methodName, BindingFlags bindingAttr, out MethodInfo methodInfo) =>
            type.HasMethod(methodName, bindingAttr | BindingFlags.DeclaredOnly, out methodInfo);

        /// <summary>
        /// 检查指定类型是否直接声明了具有指定名称和绑定标志的方法。
        /// </summary>
        /// <param name="methodName">要搜索的方法名称。</param>
        /// <param name="bindingAttr">用于控制搜索方式的绑定标志。</param>
        /// <returns>如果该类型直接声明了指定方法，则为 <see langword="true"/>；否则为 <see langword="false"/>。</returns>
        public bool HasRealMethod(string methodName, BindingFlags bindingAttr) =>
            type.HasRealMethod(methodName, bindingAttr, out _);

        /// <summary>
        /// 适用于 <paramref name="mainMethod"/> 依赖于 <paramref name="requiredMethod"/> 的情况，判定是否有正确的方法关系。
        /// </summary>
        /// <param name="mainMethodName">主方法的名称。</param>
        /// <param name="requiredMethodName">必需方法的名称。</param>
        /// <param name="bindingAttr">用于控制搜索方式的绑定标志。</param>
        /// <param name="mainMethod">
        /// 当此方法返回时，如果找到主方法，则包含该方法的信息；否则包含 <see langword="null"/>。
        /// </param>
        /// <param name="requiredMethod">
        /// 当此方法返回时，如果找到必需方法，则包含该方法的信息；否则包含 <see langword="null"/>。
        /// </param>
        /// <returns>
        /// 仅在主方法存在而必需方法不存在时返回 <see langword="false"/>；
        /// 其他情况（两者均存在或均不存在，或仅必需方法存在）返回 <see langword="true"/>。
        /// </returns>
        public bool MustHaveRealMethodWith(string mainMethodName, string requiredMethodName, BindingFlags bindingAttr, out MethodInfo mainMethod, out MethodInfo requiredMethod) =>
            (type.HasRealMethod(mainMethodName, bindingAttr, out mainMethod), type.HasRealMethod(requiredMethodName, bindingAttr, out requiredMethod)) is not (true, false);

        /// <summary>
        /// 适用于 <paramref name="mainMethodName"/> 依赖于 <paramref name="requiredMethodName"/> 的情况，判定是否有正确的方法关系。
        /// </summary>
        /// <param name="mainMethodName">主方法的名称。</param>
        /// <param name="requiredMethodName">必需方法的名称。</param>
        /// <param name="bindingAttr">用于控制搜索方式的绑定标志。</param>
        /// <returns>
        /// 仅在主方法存在而必需方法不存在时返回 <see langword="false"/>；
        /// 其他情况返回 <see langword="true"/>。
        /// </returns>
        public bool MustHaveRealMethodWith(string mainMethodName, string requiredMethodName, BindingFlags bindingAttr) =>
            type.MustHaveRealMethodWith(mainMethodName, requiredMethodName, bindingAttr, out _, out _);

        /// <summary>
        /// 获取指定类型中所有重写方法。
        /// </summary>
        /// <param name="bindingAttr">用于控制搜索方式的绑定标志。</param>
        /// <returns>重写方法的可枚举集合。</returns>
        public IEnumerable<MethodInfo> GetOverrideMethods(BindingFlags bindingAttr) => type.GetRealMethods(bindingAttr).Where(m => m.IsOverride);

        /// <summary>
        /// 获取指定类型中所有重写方法的名称。
        /// </summary>
        /// <param name="bindingAttr">用于控制搜索方式的绑定标志。</param>
        /// <returns>重写方法名称的可枚举集合。</returns>
        public IEnumerable<string> GetOverrideMethodNames(BindingFlags bindingAttr) => type.GetOverrideMethods(bindingAttr).Select(m => m.Name);

        /// <summary>
        /// 获取应用于该类型的第一个指定类型的特性。
        /// </summary>
        /// <typeparam name="T">要获取的特性类型。</typeparam>
        /// <param name="inherit">
        /// 如果为 <see langword="true"/>，则搜索该类型的继承链以查找特性；否则为 <see langword="false"/>。
        /// 默认值为 <see langword="true"/>。
        /// </param>
        /// <returns>
        /// 如果找到指定类型的特性，则返回该特性实例；否则返回 <see langword="null"/>。
        /// </returns>
        public T Attribute<T>(bool inherit = true) where T : Attribute => type.GetCustomAttributes(typeof(T), inherit).OfType<T>().FirstOrDefault();

        /// <summary>
        /// 确定该类型是否应用了指定类型的特性。
        /// </summary>
        /// <typeparam name="T">要检查的特性类型。</typeparam>
        /// <param name="inherit">
        /// 如果为 <see langword="true"/>，则搜索该类型的继承链以查找特性；否则为 <see langword="false"/>。
        /// 默认值为 <see langword="true"/>。
        /// </param>
        /// <returns>
        /// 如果该类型应用了指定类型的特性，则为 <see langword="true"/>；否则为 <see langword="false"/>。
        /// </returns>
        public bool HasAttribute<T>(bool inherit = true) where T : Attribute => type.Attribute<T>(inherit) is not null;

        /// <summary>
        /// 尝试获取应用于该类型的指定类型的特性。
        /// </summary>
        /// <typeparam name="T">要获取的特性类型。</typeparam>
        /// <param name="attribute">
        /// 当此方法返回时，如果找到特性，则包含该特性实例；否则包含 <see langword="null"/>。
        /// 此参数以未初始化状态传递。
        /// </param>
        /// <param name="inherit">
        /// 如果为 <see langword="true"/>，则搜索该类型的继承链以查找特性；否则为 <see langword="false"/>。
        /// 默认值为 <see langword="true"/>。
        /// </param>
        /// <returns>
        /// 如果找到指定类型的特性，则为 <see langword="true"/>；否则为 <see langword="false"/>。
        /// </returns>
        public bool TryGetAttribute<T>([NotNullWhen(true)] out T attribute, bool inherit = true) where T : Attribute => (attribute = type.Attribute<T>(inherit)) is not null;

        /// <summary>
        /// 获取一个值，指示该类型是否具有无参数构造函数。
        /// </summary>
        public bool HasParameterlessConstructor => type.GetConstructor(TOReflectionUtils.InstanceBindingFlags, []) is not null;
    }
}