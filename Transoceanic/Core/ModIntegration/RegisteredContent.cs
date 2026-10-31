namespace Transoceanic.Core.ModIntegration;

/// <summary>
/// 自定义内容的注册基类。使用 CLR 完整类型名避免不同命名空间的同名内容冲突；
/// 注册族支持 ModContent.Find，查询表由 TOCleanup 清理。
/// </summary>
/// <typeparam name="TContent">共享名称查询表的内容基类或接口。</typeparam>
public abstract class RegisteredContent<TContent> : ModType where TContent : class, IModType
{
    /// <inheritdoc/>
    /// <remarks>
    /// 包含命名空间的内容名称；派生类可显式选择稳定的其他名称。
    /// </remarks>
    public override string Name => GetType().FullName;

    /// <inheritdoc/>
    protected sealed override void Register()
    {
        if (this is not TContent content)
            throw new InvalidOperationException($"{GetType()} is not a {typeof(TContent)}.");
        ContentRegistration<TContent>.Register(content);
        TOCleanup.Register(ClearReferences);
    }

    /// <inheritdoc/>
    public sealed override void SetupContent() => SetStaticDefaults();

    /// <summary>
    /// 卸载时释放框架保存的实例引用；不替代 ModType.Unload 或 IContentLoader 回调。
    /// </summary>
    protected virtual void ClearReferences() { }
}
