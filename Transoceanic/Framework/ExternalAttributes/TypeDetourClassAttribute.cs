// Developed by ColdsUx

namespace Transoceanic.Framework.ExternalAttributes;

/// <summary>
/// 指示源生成器为指定的泛型目标类型生成 <c>Detour</c>（方法拦截）支持类，通过重写虚方法拦截目标类型中所有公有实例虚方法和抽象方法。
/// </summary>
/// <remarks>
/// 将此特性应用于一个 <see langword="partial"/> 类（该类应继承自 <see cref="TypeDetour{T}"/> 基类以获得基础拦截框架和方法），并传入目标类型的 <see cref="Type"/> 对象（例如 <c>typeof(ModType)</c>）。
/// <br/>源生成器 <c>TypeDetourGenerator</c> 会在编译时扫描目标类型的所有公有实例虚方法和抽象方法（包括属性的 <c>get</c>/<c>set</c> 访问器），为每个方法生成对应的委托类型以及一个可被重写的虚方法，从而允许在派生类中通过重写这些方法来实现方法拦截（Detour）。
/// <para/>
/// 生成的包装类将包含以下内容：
/// <list type="bullet">
/// <item/><description/>针对每个拦截方法的委托类型 <c>Orig_{MethodName}</c>，用于保存原始方法的引用。
/// <item/><description/>针对每个拦截方法的虚方法 <c>Detour_{MethodName}</c>，其默认实现为直接调用原始方法；可在派生类中重写以自定义拦截逻辑。
/// <item/><description/>重写的 <c>ApplyDetour</c> 方法，在其中自动注册所有 <c>Detour_*</c> 方法，并调用 <c>ApplyExtraDetour</c> 分部方法以支持扩展。
/// <item/><description/>一个分部方法 <c>partial void ApplyExtraDetour()</c>，供派生类选择性实现，用于添加额外的拦截处理。
/// </list>
/// 源生成器仅处理满足以下条件的方法：非静态、非 <see langword="sealed"/>、非 <see langword="extern"/>、非泛型为虚方法或抽象方法、且具有有效的 C# 标识符名称。
/// <br/>不支持重载方法。如目标类型中有重载方法，必须手动实现 <c>Detour</c> 逻辑。属性的 <c>get</c>/<c>set</c> 访问器会被视为两个独立方法分别进行拦截。
/// <example><para/>
/// 下面的示例演示如何为泛型类 <c>TestSource&lt;T&gt;</c> 生成 Detour 支持类，并拦截其虚方法 <c>M</c>。
/// <code>
/// //目标类型：一个简单的泛型类
/// public class TestSource&lt;T&gt; { public virtual void M() { } }
/// 
/// //应用特性，生成 Detour 类
/// [TypeDetourClass(typeof(TestSource&lt;&gt;))]
/// public partial class TestSourceDetour&lt;T&gt; : TypeDetour&lt;T&gt;;
/// </code>
/// </example>
/// </remarks>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class TypeDetourClassAttribute : Attribute
{
    /// <summary>
    /// 获取要生成 Detour 的目标泛型类型。
    /// </summary>
    public Type TargetType { get; }

    /// <summary>
    /// 初始化 <see cref="TypeDetourClassAttribute"/> 的新实例，指定需要生成 Detour 的目标泛型类型。
    /// </summary>
    /// <param name="targetType">
    /// 目标泛型类型，必须是一个开放泛型类（例如 <c>typeof(List&lt;&gt;)</c>）。
    /// 源生成器 <c>TypeDetourGenerator</c> 将为此类型生成包含拦截方法的包装类。
    /// </param>
    /// <exception cref="ArgumentNullException"><paramref name="targetType"/> 为 <see langword="null"/>。</exception>
    public TypeDetourClassAttribute(Type targetType) => TargetType = targetType ?? throw new ArgumentNullException(nameof(targetType));
}