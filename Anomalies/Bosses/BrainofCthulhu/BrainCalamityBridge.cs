namespace Anomalies.Bosses.BrainofCthulhu;

internal static class BrainCalamityBridge
{
    internal static Func<bool> BrainSpriteChanged;
    public static bool UseChangedBrainSprite => BrainSpriteChanged?.Invoke() ?? false;
    [ExtendsFromMod(CalamityModName)]
    public sealed class Support : ILoadable
    {
        /// <summary>在所属模组加载时登记灾厄 AI 策略与换皮委托，配置在运行期读取。</summary>
        /// <param name="mod">由 tML 提供的所属模组。</param>
        void ILoadable.Load(Mod mod)
        {
            TOCleanup.Register(() =>
            {
                BrainSpriteChanged = null;
                // 普通兼容事件不是 On/IL 钩子，其自有订阅需要解除。
                CalamityMod.NPCs.CalamityVanillaAIOverrideNPC.ModifyAIOverride -= SuppressOriginalAI;
            });
            CalamityMod.NPCs.CalamityVanillaAIOverrideNPC.ModifyAIOverride += SuppressOriginalAI;
            BrainSpriteChanged = () => CalamityMod.CalamityClientConfig.Instance.BoCSpriteChange;
        }

        // 自有换皮引用与普通兼容事件由 TOCleanup 清理，不重复卸载 On/IL 钩子。
        void ILoadable.Unload() { }

        private static void SuppressOriginalAI(CalamityMod.NPCs.VanillaNPCAIOverrides.VanillaAIOverrideContext context)
        {
            if (AnomalySharedData.Anomaly && context.NPCType is NPCID.BrainofCthulhu or NPCID.Creeper)
                context.OverrideToApply = null;
        }
    }
}
