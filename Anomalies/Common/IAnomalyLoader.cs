namespace Anomalies.Common;

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


