namespace Transoceanic.Core.ModIntegration;

/// <summary>
/// 指定模组在创建此记录时的加载状态；由消费模组按自身生命周期保存和释放。
/// </summary>
public sealed record ModReference
{
    /// <summary>
    /// 所引用模组的内部名称，用于解析已加载实例。
    /// </summary>
    public string Name { get; }
    /// <summary>
    /// 所引用模组是否已成功解析为加载实例。
    /// </summary>
    public bool IsLoaded { get; }
    /// <summary>
    /// 已解析的可选模组实例；未加载时为 null。
    /// </summary>
    public Mod Mod { get; }

    /// <summary>
    /// 创建指向指定模组名称的可选依赖引用。
    /// 若目标模组未加载，引用会保留缺失状态，供调用方选择回退行为。
    /// </summary>
    public ModReference(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Name = name;
        IsLoaded = ModLoader.TryGetMod(name, out Mod mod);
        Mod = mod;
    }
}
