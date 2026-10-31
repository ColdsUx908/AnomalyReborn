namespace Anomalies.Common;

/// <summary>保留给异象内部需要集中排序的加载任务；独立任务使用 tML 原生加载入口。</summary>
internal interface IAnomalyLoader
{
    /// <summary>
    /// 在本Mod加载时调用。
    /// </summary>
    internal virtual void Load() { }

    /// <summary>
    /// 在Mod卸载时调用。
    /// </summary>
    internal virtual void Unload() { }
}
