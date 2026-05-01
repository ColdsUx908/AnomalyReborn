// Developed by ColdsUx

namespace Transoceanic.Framework.Helpers;

public static partial class TOExtensions
{
    extension(ref NPC.HitModifiers modifiers)
    {
        /// <summary>
        /// 设置一击必杀效果（通过增加等于目标最大生命值的最终伤害）。
        /// </summary>
        /// <param name="target">受击目标 NPC。</param>
        public void SetInstantKillBetter(NPC target) => modifiers.FinalDamage.Flat += target.lifeMax;

        /// <summary>
        /// 强制将暴击字段设为 <see langword="true"/>。
        /// </summary>
        /// <remarks>与 <see cref="NPC.HitModifiers.SetCrit"/> 不同，即使之前调用了 <see cref="NPC.HitModifiers.DisableCrit"/>，此方法仍会生效。</remarks>
        public void ForceCrit() => TOReflectionUtils.SetStructField(ref modifiers, NPC_HitModifiers_Publicizer.i_f__critOverride, true);
    }
}