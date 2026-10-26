using MonoMod.RuntimeDetour;

namespace Transoceanic.Runtime.Detours;

/// <summary>
/// 为指定类型 <typeparamref name="T"/> 提供 Detour 逻辑的抽象基类。
/// 实现 <see cref="ITODetourProvider"/> 接口，在满足 <see cref="ShouldApplyDetour"/> 条件时将已注册的 Detour 应用到目标类型上。
/// </summary>
/// <typeparam name="T">要应用 Detour 的目标类型。</typeparam>
/// <remarks>
/// 派生类可以重写 <see cref="ShouldApplyDetour"/> 属性来控制是否应用 Detour，
/// 并通过重写 <see cref="ApplyDetour"/> 方法或在其中调用 <see cref="ApplySingleDetour{TDelegate}"/> 来注册具体的 Detour 逻辑。
/// <para>
/// 当此类作为 <see cref="ITODetourProvider"/> 使用时，框架会调用显式接口实现 <see cref="ITODetourProvider.ApplyDetour"/>，
/// 该方法内部将检查 <see cref="ShouldApplyDetour"/>，若为 <see langword="true"/> 则执行 <see cref="ApplyDetour"/>。
/// </para>
/// </remarks>
public abstract class TypeDetour<T> : ITODetourProvider
{
    /// <summary>
    /// 获取当前 Detour 提供者所针对的源类型。
    /// 固定为 <c>typeof(T)</c>，用于所有后续 Detour 操作的目标类型识别。
    /// </summary>
    public static readonly Type SourceType = typeof(T);

    /// <inheritdoc />
    /// <remarks>
    /// 该方法是 <see cref="ITODetourProvider.ApplyDetour"/> 的显式接口实现，
    /// 用于统一触发 Detour 应用流程。它会先评估 <see cref="ShouldApplyDetour"/> 属性，
    /// 仅在返回 <see langword="true"/> 时调用可重写的 <see cref="ApplyDetour"/> 方法。
    /// </remarks>
    void ITODetourProvider.ApplyDetour()
    {
        if (ShouldApplyDetour)
            ApplyDetour();
    }

    /// <summary>
    /// 获取一个值，该值指示是否应该应用 Detour。
    /// 默认返回 <see langword="true"/>。派生类可以重写此属性以实现条件应用。
    /// </summary>
    /// <value>
    /// 如果应该应用 Detour，则为 <see langword="true"/>；否则为 <see langword="false"/>。
    /// </value>
    public virtual bool ShouldApplyDetour => true;

    /// <summary>
    /// 应用所有针对类型 <typeparamref name="T"/> 的 Detour。
    /// 派生类应重写此方法，并在其中调用 <see cref="ApplySingleDetour{TDelegate}"/> 等方法注册具体的 Hook。
    /// </summary>
    /// <remarks>
    /// <inheritdoc cref="ITODetourProvider.ApplyDetour" path="/summary"/>
    /// <para>
    /// 默认实现为空操作。如果需要重载方法，应使用 <c>Detour_{methodName}__{paramNames}</c> 格式的方法名来定义委托方法，
    /// 该命名规则由 <see cref="TODetourHandler.EvaluateDetourName(MethodInfo, out string)"/> 解析。
    /// </para>
    /// </remarks>
    public virtual void ApplyDetour() { }

    /// <summary>
    /// 尝试将一个由委托表示的具体 Detour 应用到 <typeparamref name="T"/> 类型的目标方法上。
    /// </summary>
    /// <typeparam name="TDelegate">委托类型，必须与目标方法签名匹配。</typeparam>
    /// <param name="detour">
    /// 表示 Detour 逻辑的委托实例。该委托必须是一个由当前类型定义的具名方法，
    /// 且其方法名必须符合 <see cref="TODetourHandler.EvaluateDetourName(MethodInfo, out string)"/> 的解析规则
    /// （通常为 <c>Detour_{方法名}__{参数类型简短名}</c> 格式）。
    /// </param>
    /// <param name="hasThis">
    /// <para>指示目标方法是否为实例方法（即是否包含 <see langword="this"/> 指针）。</para>
    /// <para>
    /// 该值会影响获取目标方法时的参数偏移量：
    /// 当 <paramref name="hasThis"/> 为 <see langword="true"/> 时，实例方法的第一个隐式参数（this）会使参数偏移量变为 2；
    /// 若为 <see langword="false"/>（静态方法），参数偏移量为 1。
    /// 同时它也会影响内部使用的绑定标志（<c>bindingAttr</c>）。
    /// </para>
    /// </param>
    /// <returns>
    /// 如果 Detour 成功应用，返回对应的 <see cref="Hook"/> 实例；
    /// 如果委托方法不满足条件（如委托未由当前类型声明、方法名无法解析出有效源方法名等），则返回 <see langword="null"/>。
    /// </returns>
    /// <remarks>
    /// 此方法的默认实现首先检查 <paramref name="detour"/> 是否确在当前类型声明，
    /// 然后尝试通过 <see cref="TODetourHandler.EvaluateDetourName(MethodInfo, out string)"/> 
    /// 从委托方法名中解析出目标源方法的名称（<paramref name="sourceName"/>）。
    /// 如果两者均满足，则调用 <see cref="TODetourHandler.Modify"/> 执行实际的 Detour 应用，并返回生成的 <see cref="Hook"/>；
    /// 否则返回 <see langword="null"/>，表示未执行任何操作。
    /// <para>
    /// 使用此方法的派生类通常应在 <see cref="ApplyDetour"/> 重写中进行调用。
    /// </para>
    /// </remarks>
    protected virtual Hook ApplySingleDetour<TDelegate>(TDelegate detour, bool hasThis = true) where TDelegate : Delegate =>
        detour.Method.DeclaringType == GetType() && TODetourHandler.EvaluateDetourName(detour.Method, out string sourceName)
        ? TODetourHandler.Modify(SourceType, sourceName, hasThis, detour)
        : null;
}
