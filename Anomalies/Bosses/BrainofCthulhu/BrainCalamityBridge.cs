namespace Anomalies.Bosses.BrainofCthulhu;

internal static class BrainCalamityBridge
{
    internal static Func<bool> BrainSpriteChanged;
    public static bool UseChangedBrainSprite => BrainSpriteChanged?.Invoke() ?? false;
    [ExtendsFromMod(CalamityModName)]
    public sealed class Support : IContentLoader
    {
        public void PostSetupContent()
        {
            CalamityMod.NPCs.CalamityVanillaAIOverrideNPC.ModifyAIOverride += SuppressOriginalAI;
            BrainSpriteChanged = () => CalamityMod.CalamityClientConfig.Instance.BoCSpriteChange;
        }

        public void OnModUnload()
        {
            CalamityMod.NPCs.CalamityVanillaAIOverrideNPC.ModifyAIOverride -= SuppressOriginalAI;
            BrainSpriteChanged = null;
        }

        private static void SuppressOriginalAI(CalamityMod.NPCs.VanillaNPCAIOverrides.VanillaAIOverrideContext context)
        {
            if (AnomalySharedData.Anomaly && context.NPCType is NPCID.BrainofCthulhu or NPCID.Creeper)
                context.OverrideToApply = null;
        }
    }
}
