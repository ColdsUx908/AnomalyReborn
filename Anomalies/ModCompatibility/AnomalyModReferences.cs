namespace Anomalies.ModCompatibility;

/// <summary>异象自身使用的可选依赖；不向通用框架注册其他模组名称。</summary>
public sealed class AnomalyModReferences : ILoadable
{
    public static ModReference AnomaliesMusic { get => field ??= new("AnomaliesMusic"); private set; }
    public static ModReference Calamity { get => field ??= new("CalamityMod"); private set; }

    /// <summary>在 tML 内容加载时解析可选模组引用，并预先登记本轮清理。</summary>
    /// <param name="mod">由 tML 提供的所属模组。</param>
    void ILoadable.Load(Mod mod)
    {
        TOCleanup.Register(ClearReferences);
        _ = AnomaliesMusic;
        _ = Calamity;
    }

    // 清理由 TOCleanup 统一执行。
    void ILoadable.Unload() { }

    private static void ClearReferences()
    {
        AnomaliesMusic = null;
        Calamity = null;
    }
}
