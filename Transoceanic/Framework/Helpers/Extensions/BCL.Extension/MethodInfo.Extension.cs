// Designed by ColdsUx

namespace Transoceanic.Framework.Helpers;

public static partial class TOExtensions
{
    extension(MethodInfo method)
    {
        /// <summary>
        /// 判定指定方法是否为指定基类型的重写方法。
        /// </summary>
        /// <param name="baseType">要检查的基类型。</param>
        /// <returns>
        /// 如果该方法是 <paramref name="baseType"/> 中定义的虚方法的重写，则为 <see langword="true"/>；
        /// 否则为 <see langword="false"/>。
        /// </returns>
        public bool IsOverrideOf(Type baseType)
        {
            MethodInfo baseDefinition = method.GetBaseDefinition();
            return baseDefinition.DeclaringType == baseType && !baseDefinition.DeclaringType.IsInterface;
        }

        /// <summary>
        /// 判定指定方法是否为指定基类型的重写方法。
        /// </summary>
        /// <typeparam name="T">要检查的基类型。</typeparam>
        /// <returns>
        /// 如果该方法是 <typeparamref name="T"/> 中定义的虚方法的重写，则为 <see langword="true"/>；
        /// 否则为 <see langword="false"/>。
        /// </returns>
        public bool IsOverrideOf<T>() => method.IsOverrideOf(typeof(T));

        /// <summary>
        /// 获取一个值，指示该方法是否为某个基类型的重写方法。
        /// </summary>
        /// <returns>
        /// 如果该方法重写了基类中的虚方法（且基类型不是接口），则为 <see langword="true"/>；
        /// 否则为 <see langword="false"/>。
        /// </returns>
        public bool IsOverride
        {
            get
            {
                MethodInfo baseDefinition = method.GetBaseDefinition();
                return baseDefinition.DeclaringType != method.DeclaringType && !baseDefinition.DeclaringType.IsInterface;
            }
        }

        /// <summary>
        /// 判定指定方法是否实现了指定接口类型。
        /// </summary>
        /// <param name="interfaceType">要检查的接口类型。</param>
        /// <returns>
        /// 如果该方法实现了 <paramref name="interfaceType"/> 中的某个方法，则为 <see langword="true"/>；
        /// 否则为 <see langword="false"/>。
        /// </returns>
        public bool IsInterfaceImplementationOf(Type interfaceType)
        {
            InterfaceMapping map;
            try
            {
                map = method.DeclaringType.GetInterfaceMap(interfaceType);
            }
            catch (NotSupportedException)
            {
                return false;
            }

            for (int i = 0; i < map.InterfaceMethods.Length; i++)
            {
                if (map.TargetMethods[i] == method)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// 判定指定方法是否实现了指定接口类型。
        /// </summary>
        /// <typeparam name="T">要检查的接口类型，必须是引用类型。</typeparam>
        /// <returns>
        /// 如果该方法实现了 <typeparamref name="T"/> 中的某个方法，则为 <see langword="true"/>；
        /// 否则为 <see langword="false"/>。
        /// </returns>
        public bool IsInterfaceImplementationOf<T>() where T : class => method.IsInterfaceImplementationOf(typeof(T));

        /// <summary>
        /// 获取一个值，指示该方法是否实现了某个接口类型。
        /// </summary>
        /// <returns>
        /// 如果该方法实现了其声明类型所实现的任何接口中的某个方法，则为 <see langword="true"/>；
        /// 否则为 <see langword="false"/>。
        /// </returns>
        public bool IsInterfaceImplementation => method.DeclaringType.GetInterfaces().Any(method.IsInterfaceImplementationOf);

        /// <summary>
        /// 获取一个值，指示该方法是否为显式接口实现。
        /// </summary>
        public bool IsExplicitInterfaceImplementation => method.IsInterfaceImplementation && method.Name.Contains('.');

        /// <summary>
        /// 获取一个值，指示该方法是否为隐式接口实现。
        /// </summary>
        public bool IsImplicitInterfaceImplementation => method.IsInterfaceImplementation && !method.Name.Contains('.');

        /// <summary>
        /// 判定指定方法是否为真正的虚方法或抽象方法。
        /// <br/>具体判定逻辑：必须为虚方法且不是重写且不是 sealed，或者是抽象方法。
        /// </summary>
        public bool IsRealVirtualOrAbstract => (method.IsVirtual && !method.IsOverride && !method.IsFinal) || method.IsAbstract;

        /// <summary>
        /// 判定指定方法是否为真正的虚方法（非抽象）。
        /// </summary>
        public bool IsRealVirtual => method.IsRealVirtualOrAbstract && !method.IsAbstract;
    }
}