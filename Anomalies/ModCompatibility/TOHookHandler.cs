using CalamityMod.NPCs.SlimeGod;
using Transoceanic.Runtime.Hooks;

namespace Anomalies.ModCompatibility;

[ExtendsFromMod(CalamityModName)]
public sealed class TOHookHandler : IContentLoader
{
    void IContentLoader.PostSetupContent()
    {
        On_TOExtensions.get_IsBossEnemy += npc => npc.ModNPC is EbonianPaladin or CrimulanPaladin or SplitEbonianPaladin or SplitCrimulanPaladin;
    }
}
