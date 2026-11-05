using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod.Cil;

namespace Anomalies.Visuals;

public sealed partial class AnomalyDepthDrawSystem
{
    // CombatText 会复用实例；每次 NewText 都清除旧倍率，避免影响玩家文本。
    private static readonly ConditionalWeakTable<CombatText, CombatTextDepth> _CombatTextDepths = new();

    private sealed class CombatTextDepth(float scale)
    {
        public float Scale { get; } = scale;
    }

    private static void LoadNPCCombatText()
    {
        IL_NPC.StrikeNPC_HitInfo_bool_bool += ProjectNPCCombatText;
        MethodInfo debuffDamage = typeof(NPC).GetMethod("GetHurtByDebuff", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (debuffDamage is not null)
            MonoModHooks.Modify(debuffDamage, ProjectNPCCombatText);
        else
            IL_NPC.UpdateNPC_BuffApplyDOTs += ProjectNPCCombatText;
        On_CombatText.NewText_Rectangle_Color_string_bool_bool += ResetCombatTextDepth;
        On_CombatText.Update += UpdateProjectedCombatText;
    }

    public override void Unload() => _CombatTextDepths.Clear();

    private static int ResetCombatTextDepth(On_CombatText.orig_NewText_Rectangle_Color_string_bool_bool orig,
        Rectangle location, Color color, string text, bool dramatic, bool dot)
    {
        int index = orig(location, color, text, dramatic, dot);
        if ((uint)index < (uint)Main.combatText.Length)
            _CombatTextDepths.Remove(Main.combatText[index]);
        return index;
    }

    private static void ProjectNPCCombatText(ILContext il)
    {
        Instruction[] calls = il.Body.Instructions.Where(i => i.MatchCall<CombatText>(nameof(CombatText.NewText))
            && i.Operand is MethodReference method && method.Parameters.Count == 5
            && method.Parameters[2].ParameterType.MetadataType == MetadataType.Int32).ToArray();
        if (calls.Length == 0)
        {
            AnomalyUtils.ILFailure(nameof(ProjectNPCCombatText), $"Could not locate NPC combat text in {il.Method.Name}.");
            return;
        }
        ILCursor cursor = new(il);
        foreach (Instruction call in calls)
        {
            cursor.Goto(call, MoveType.Before);
            cursor.Remove();
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.EmitDelegate(NewProjectedNPCCombatText);
        }
    }

    private static int NewProjectedNPCCombatText(Rectangle location, Color color, int amount, bool dramatic, bool dot, NPC npc)
    {
        float z = npc.Anomaly.Z;
        if (!IsVisible(z))
            return Main.combatText.Length;
        int index = CombatText.NewText(location, color, amount, dramatic, dot);
        if ((uint)index >= (uint)Main.combatText.Length || (z == 0f && npc.Anomaly.Depth_DrawAnchor is null))
            return index;

        CombatText text = Main.combatText[index];
        float scale = GetPerspectiveScale(z);
        Vector2 pivot = GetVanishingPoint(Main.GameViewMatrix.TransformationMatrix, ViewportSize) + Main.screenPosition;
        // 原版以未缩放字体的半尺寸作为绘制中心；投影中心后再还原该偏移。
        Vector2 origin = Terraria.GameContent.FontAssets.CombatText[dramatic ? 1 : 0].Value.MeasureString(text.text) * 0.5f;
        text.position = ProjectPosition(text.position + origin, z, pivot)
            + GetNPCAnchorTranslation(npc, pivot, includeNetOffset: false) - origin;
        text.velocity *= scale;
        text.scale *= scale;
        _CombatTextDepths.Add(text, new CombatTextDepth(scale));
        return index;
    }

    private static void UpdateProjectedCombatText(On_CombatText.orig_Update orig, CombatText self)
    {
        if (!_CombatTextDepths.TryGetValue(self, out CombatTextDepth depth))
        {
            orig(self);
            return;
        }
        // 在原版空间推进缩放、暴击、DOT 和消失动画，再恢复投影倍率。
        // 不能仅在生成时乘 scale：原版 Update 会将其重新增长到 1。
        Vector2 position = self.position;
        self.scale /= depth.Scale;
        self.velocity /= depth.Scale;
        try
        {
            orig(self);
        }
        finally
        {
            self.position = position + (self.position - position) * depth.Scale;
            self.velocity *= depth.Scale;
            self.scale *= depth.Scale;
        }
    }
}