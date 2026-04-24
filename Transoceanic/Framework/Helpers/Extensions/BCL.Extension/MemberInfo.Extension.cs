namespace Transoceanic.Framework.Helpers;

public static partial class TOExtensions
{
    extension(MemberInfo member)
    {
        /// <summary>
        /// 获取应用于该成员的第一个指定类型的特性。
        /// </summary>
        /// <typeparam name="T">要获取的特性类型。</typeparam>
        /// <param name="inherit">
        /// 如果为 <see langword="true"/>，则搜索该成员的继承链以查找特性；否则为 <see langword="false"/>。
        /// 默认值为 <see langword="true"/>。
        /// </param>
        /// <returns>
        /// 如果找到指定类型的特性，则返回该特性实例；否则返回 <see langword="null"/>。
        /// </returns>
        public T Attribute<T>(bool inherit = true) where T : Attribute => member.GetCustomAttributes(typeof(T), inherit).OfType<T>().FirstOrDefault();

        /// <summary>
        /// 确定该成员是否应用了指定类型的特性。
        /// </summary>
        /// <typeparam name="T">要检查的特性类型。</typeparam>
        /// <param name="inherit">
        /// 如果为 <see langword="true"/>，则搜索该成员的继承链以查找特性；否则为 <see langword="false"/>。
        /// 默认值为 <see langword="true"/>。
        /// </param>
        /// <returns>
        /// 如果该成员应用了指定类型的特性，则为 <see langword="true"/>；否则为 <see langword="false"/>。
        /// </returns>
        public bool HasAttribute<T>(bool inherit = true) where T : Attribute => member.IsDefined(typeof(T), inherit);

        /// <summary>
        /// 尝试获取应用于该成员的指定类型的特性。
        /// </summary>
        /// <typeparam name="T">要获取的特性类型。</typeparam>
        /// <param name="attribute">
        /// 当此方法返回时，如果找到特性，则包含该特性实例；否则包含 <see langword="null"/>。
        /// 此参数以未初始化状态传递。
        /// </param>
        /// <param name="inherit">
        /// 如果为 <see langword="true"/>，则搜索该成员的继承链以查找特性；否则为 <see langword="false"/>。
        /// 默认值为 <see langword="true"/>。
        /// </param>
        /// <returns>
        /// 如果找到指定类型的特性，则为 <see langword="true"/>；否则为 <see langword="false"/>。
        /// </returns>
        public bool TryGetAttribute<T>([NotNullWhen(true)] out T attribute, bool inherit = true) where T : Attribute => (attribute = member.Attribute<T>(inherit)) is not null;
    }
}