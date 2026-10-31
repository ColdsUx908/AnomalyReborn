using Transoceanic.Runtime.Publicizing.Wrappers;

namespace Transoceanic.Core.ModIntegration;

internal static class ContentRegistration<TContent> where TContent : class, IModType
{
    private static bool _CleanupRegistered;

    internal static void Register(TContent content)
    {
        // 在可能部分写入查询表之前取得清理资格，tML 不会清空第三方泛型表。
        if (!_CleanupRegistered)
        {
            TOCleanup.Register(Clear);
            _CleanupRegistered = true;
        }
        ModTypeLookup<TContent>.Register(content);
    }

    private static void Clear()
    {
        try
        {
            ModTypeLookup_Publicizer<TContent>.dict.Clear();
            ModTypeLookup_Publicizer<TContent>.tieredDict.Clear();
        }
        finally { _CleanupRegistered = false; }
    }
}
