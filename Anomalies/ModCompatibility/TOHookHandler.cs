// Developed by ColdsUx

using CalamityMod.NPCs.SlimeGod;
using Transoceanic.Hooks.Framework.Helpers;

namespace Anomalies.ModCompatibility;

[ExtendsFromMod(CalamityModName)]
public sealed class TOHookHandler : IContentLoader
{
    void IContentLoader.PostSetupContent()
    {
        On_TOExtensions.get_IsBossEnemy += npc => npc.ModNPC is EbonianPaladin or CrimulanPaladin or SplitEbonianPaladin or SplitCrimulanPaladin;
    }
}
