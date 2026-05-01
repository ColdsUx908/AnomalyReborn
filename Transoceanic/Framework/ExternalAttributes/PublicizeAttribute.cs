// Developed by ColdsUx

namespace Transoceanic.Framework.ExternalAttributes;

/// <summary>
/// 指示源生成器为指定的非公开类型生成公共访问包装器，通过反射暴露其非公共成员（字段、属性、方法）。
/// </summary>
/// <remarks>
/// 将此特性应用于一个 <see langword="partial"/> 类（该类可选择继承 <see cref="InstancedPublicizer"/> 类以获得 <c>Source</c> 字段），并传入目标类型的 <see cref="Type"/> 对象。
/// 源生成器 <c>PublicizerGenerator</c> 会在编译时扫描目标类型的所有非公开成员，为它们生成对应的公共包装属性/方法，从而允许在外部代码中通过反射动态访问这些成员。
/// <para/>
/// 生成的包装类将包含：
/// <list type="bullet">
/// <item/><description/>目标类型的静态字段 <c>c_TargetType</c>（保存 <c>typeof(TargetType)</c>）。
/// <item/><description/>针对实例/静态字段、属性、方法的反射信息字段和公共访问器。
/// </list>
/// 对于方法，会生成一个强类型委托并缓存，以提供接近直接调用的性能。
/// <para/>
/// 使用此特性前，请确保目标类型在当前编译上下文中可见（可通过 <c>MetadataImportOptions.All</c> 解析）。
/// 生成的代码仅依赖 <c>System.Reflection</c>，不改变原始类型的访问性。
/// 
/// <example>
/// <para/>下面的示例演示如何公开类 <c>ExampleHelper</c>（定义有私有字段 <c>private int _counter</c>）的私有成员。
/// <code>
/// [Publicize(typeof(ExampleHelper))]
/// public partial class ExampleHelper_Publicizer(object Source) : InstancedPublicizer(Source); //应用特性，指定目标类型
/// 
/// //使用生成的公共包装器
/// ExampleHelper instance = new();
/// ExampleHelper_Publicizer publicizer = new(instance);
/// publicizer._counter = 10; //访问私有成员
/// </code>
/// </example>
/// </remarks>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class PublicizeAttribute : Attribute
{
    /// <summary>
    /// 获取要公开非公共成员的目标类型。
    /// </summary>
    public Type TargetType { get; }

    /// <summary>
    /// 初始化 <see cref="PublicizeAttribute"/> 的新实例，指定需要公开其非公共成员的目标类型。
    /// </summary>
    /// <param name="targetType">需要被公开的类型。源生成器将为此类型生成反射包装器。</param>
    /// <exception cref="ArgumentNullException"><paramref name="targetType"/> 为 <see langword="null"/>。</exception>
    public PublicizeAttribute(Type targetType) => TargetType = targetType ?? throw new ArgumentNullException(nameof(targetType));
}