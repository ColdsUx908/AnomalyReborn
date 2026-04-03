namespace Transoceanic.Framework.Helpers;

public static partial class TOExtensions
{
    extension(MethodInfo method)
    {
        /// <summary>
        /// 判定指定方法是否为指定基类型的重写方法。
        /// </summary>
        /// <param name="baseType"></param>
        /// <returns></returns>
        public bool IsOverrideOf(Type baseType)
        {
            MethodInfo baseDefinition = method.GetBaseDefinition();
            return baseDefinition.DeclaringType == baseType && !baseDefinition.DeclaringType.IsInterface;
        }

        /// <summary>
        /// 判定指定方法是否为指定基类型的重写方法。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public bool IsOverrideOf<T>() => method.IsOverrideOf(typeof(T));

        /// <summary>
        /// 判定指定方法是否为某个基类型的重写方法。
        /// </summary>
        /// <returns></returns>
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
        /// <param name="interfaceType"></param>
        /// <returns></returns>
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
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public bool IsInterfaceImplementationOf<T>() where T : class => method.IsInterfaceImplementationOf(typeof(T));

        /// <summary>
        /// 判定指定方法是否实现了某个接口类型。
        /// </summary>
        /// <returns></returns>
        public bool IsInterfaceImplementation => method.DeclaringType.GetInterfaces().Any(method.IsInterfaceImplementationOf);

        public bool IsExplicitInterfaceImplementation => method.IsInterfaceImplementation && method.Name.Contains('.');

        public bool IsImplicitInterfaceImplementation => method.IsInterfaceImplementation && !method.Name.Contains('.');

        /// <summary>
        /// 判定指定方法是否为真正的虚方法。
        /// <br/>具体判定逻辑：必须为虚方法，且不是重写方法或最终方法（sealed，用于判定逻辑上非虚的接口实现）。
        /// </summary>
        public bool IsRealVirtualOrAbstract => (method.IsVirtual && !method.IsOverride && !method.IsFinal) || method.IsAbstract;

        public bool IsRealVirtual => method.IsRealVirtualOrAbstract && !method.IsAbstract;
    }
}