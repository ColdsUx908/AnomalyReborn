using Transoceanic.Framework.Publicizers.Terraria;

namespace Transoceanic.Framework.Helpers;

public static partial class TOExtensions
{
    extension(ref NPC.HitModifiers modifiers)
    {
        public void SetInstantKillBetter(NPC target) => modifiers.FinalDamage.Flat += target.lifeMax;

        /// <summary>
        /// 将暴击字段设为 <see langword="true"/>。
        /// <br/>不同于 <see cref="NPC.HitModifiers.SetCrit"/>，即使 <see cref="NPC.HitModifiers.DisableCrit"/> 已被调用，该方法仍会生效。
        /// </summary>
        public void ForceCrit() => TOReflectionUtils.SetStructField(ref modifiers, NPC_HitModifiers_Publicizer.i_f__critOverride, true);
    }
}