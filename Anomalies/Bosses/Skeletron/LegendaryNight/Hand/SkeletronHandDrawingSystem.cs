using Mono.Cecil.Cil;
using MonoMod.Cil;

namespace Anomalies.Bosses.Skeletron.LegendaryNight;

public sealed class SkeletronHandDrawingSystem : ModSystem
{
    public override void Load()
    {
        if (!Main.dedServ)
        {
            IL_Main.DrawNPCDirect += RedirectHandBloodDust;
        }
    }
    private static void RedirectHandBloodDust(ILContext il)
    {
        ILCursor cursor = new(il);
        // 仅定位原版 type == SkeletronHand 分支内的血尘生成，不拦截其他实体或受击血尘。
        if (!cursor.TryGotoNext(MoveType.After,
                i => i.MatchLdcI4(NPCID.SkeletronHand),
                i => i.OpCode.FlowControl == FlowControl.Cond_Branch)
            || !cursor.TryGotoNext(MoveType.Before, i => i.MatchCall<Dust>(nameof(Dust.NewDust))))
        {
            AnomalyUtils.ILFailure(nameof(SkeletronHandDrawingSystem), "Could not locate Skeletron hand blood dust creation.");
            return;
        }
        ILCursor armCursor = new(il);
        if (armCursor.TryGotoNext(MoveType.Before,
                i => i.MatchLdcI4(NPCID.SkeletronHand),
                i => i.OpCode.FlowControl == FlowControl.Cond_Branch))
        {
            ILLabel skipArm = (ILLabel)armCursor.Next.Next.Operand;
            armCursor.Index += 2;
            armCursor.Emit(OpCodes.Ldarg_2);
            armCursor.EmitDelegate(SkeletronLegendHand.DetachedFromArms);
            armCursor.Emit(OpCodes.Brtrue, skipArm);
        }
        else
            AnomalyUtils.ILFailure(nameof(SkeletronHandDrawingSystem), "Could not locate Skeletron arm branch.");
        // 原版两段骨节共用此颜色入口，默认没有应用手部 alpha。
        ILCursor colorCursor = new(il);
        if (colorCursor.TryGotoNext(MoveType.After,
                i => i.MatchLdcI4(NPCID.SkeletronHand),
                i => i.OpCode.FlowControl == FlowControl.Cond_Branch)
            && colorCursor.TryGotoNext(MoveType.After, i => i.MatchCall<Lighting>(nameof(Lighting.GetColor))))
        {
            colorCursor.Emit(OpCodes.Ldarg_2);
            colorCursor.EmitDelegate((Color color, NPC npc) => npc.Anomaly.CustomAIType == 1 ? color * npc.Opacity : color);
        }
        else
            AnomalyUtils.ILFailure(nameof(SkeletronHandDrawingSystem), "Could not locate Skeletron arm color.");

        cursor.Remove();
        cursor.Emit(OpCodes.Ldarg_2); // DrawNPCDirect 的 rCurrentNPC。
        cursor.EmitDelegate(CreateHandBloodDust);
    }
    private static int CreateHandBloodDust(Vector2 position, int width, int height, int type, float speedX, float speedY,
        int alpha, Color color, float scale, NPC npc)
    {
        if (npc.type != NPCID.SkeletronHand || npc.Anomaly.CustomAIType != 1 || type != DustID.Blood)
            return Dust.NewDust(position, width, height, type, speedX, speedY, alpha, color, scale);

        SkeletronHandBloodDust blood = npc.Anomaly.Skeletron_HandBloodDust ??= new();
        blood.Spawn(position, width, height, speedX, speedY, alpha, color, scale);
        // 原版紧接着会访问返回槽位的 noGravity。使用 NewDust 自带的失败哨兵，绝不占用全局粒子槽。
        return Main.maxDust;
    }
}