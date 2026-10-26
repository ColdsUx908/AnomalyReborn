using Mono.Cecil.Cil;
using MonoMod.Cil;

namespace Anomalies.Bosses.Skeletron;

public sealed class SkeletronHandler : IContentLoader
{
    void IContentLoader.PostSetupContent() => IL_NPC.checkDead += RestrictHandDeathCasters;

    private static void RestrictHandDeathCasters(ILContext il)
    {
        // TODO(tModLoader 升级，尤其 1.4.5)：重新核对 checkDead 的手部死亡生成分支和生成源。
        // 当前安装的 1.4.4.9 在 type == SkeletronHand 分支内以 NaturalSpawn 生成三个暗黑法师；
        // 新版源码已改用父实体生成源，届时需调整或替换本 IL 匹配，不能沿用当前布局假设。
        ILCursor cursor = new(il);
        ILLabel end = null;
        if (!cursor.TryGotoNext(MoveType.After,
                i => i.MatchLdarg0(),
                i => i.MatchLdfld<NPC>(nameof(NPC.type)),
                i => i.MatchLdcI4(NPCID.SkeletronHand),
                i => i.MatchBneUn(out end)))
        {
            AnomalyUtils.ILFailure(nameof(SkeletronHandler), "Could not locate Skeletron hand death caster branch.");
            return;
        }

        // 在进入三次生成/每次千次选点循环之前跳过整个分支，不改世界标志或其他 NPC 的生成。
        cursor.Emit(OpCodes.Ldarg_0);
        cursor.EmitDelegate<Func<NPC, bool>>(hand => Aroma || !SkeletronHand.GetInstance(hand).ShouldProcess);
        cursor.Emit(OpCodes.Brfalse, end);
    }
}
