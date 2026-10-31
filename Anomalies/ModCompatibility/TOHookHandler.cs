using CalamityMod.NPCs.SlimeGod;
using Transoceanic.Runtime.Hooks;

namespace Anomalies.ModCompatibility;

[ExtendsFromMod(CalamityModName)]
public sealed class TOHookHandler : ILoadable
{
    /// <summary>在框架事件初始化后通过 tML 内容加载订阅灾厄 Boss 判定。</summary>
    /// <param name="mod">由 tML 提供的所属模组。</param>
    void ILoadable.Load(Mod mod)
    {
        On_TOExtensions.get_IsBossEnemy += npc => npc.ModNPC is EbonianPaladin or CrimulanPaladin or SplitEbonianPaladin or SplitCrimulanPaladin;
    }
    // 事件容器由框架 TOCleanup 清空，不额外退订。
    void ILoadable.Unload() { }
}
