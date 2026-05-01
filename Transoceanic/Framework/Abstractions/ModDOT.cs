// Developed by ColdsUx

namespace Transoceanic.Framework.Abstractions;

/// <summary>
/// 表示一个自定义持续伤害（DOT）效果的 Buff 抽象基类。
/// 继承自 <see cref="ModBuff"/>，自动设置为 Debuff 且不随保存。
/// </summary>
public abstract class ModDOT : ModBuff
{
    /// <summary>
    /// 设置 Buff 的静态默认属性：标记为 Debuff 且不保存到角色数据中。
    /// </summary>
    public override void SetStaticDefaults()
    {
        Main.debuff[Type] = true;
        Main.buffNoSave[Type] = true;
    }

    /// <summary>
    /// 判断指定玩家当前是否拥有该持续伤害效果。
    /// </summary>
    /// <param name="player">要检查的玩家实例。</param>
    /// <returns>若玩家拥有此 Buff 则返回 <see langword="true"/>；否则返回 <see langword="false"/>。</returns>
    public abstract bool HasBuff(Player player);

    /// <summary>
    /// 判断指定 NPC 当前是否拥有该持续伤害效果。
    /// </summary>
    /// <param name="npc">要检查的 NPC 实例。</param>
    /// <returns>若 NPC 拥有此 Buff 则返回 <see langword="true"/>；否则返回 <see langword="false"/>。</returns>
    public abstract bool HasBuff(NPC npc);

    /// <summary>
    /// 获取该持续伤害对指定玩家每帧造成的伤害数值（生命回复减少量）。
    /// </summary>
    /// <param name="player">受影响的玩家实例。</param>
    /// <returns>伤害数值（正数表示减少生命回复）。</returns>
    public virtual float GetDamage(Player player) => 0f;

    /// <summary>
    /// 获取该持续伤害对指定 NPC 每帧造成的伤害数值。
    /// </summary>
    /// <param name="npc">受影响的 NPC 实例。</param>
    /// <returns>伤害数值（正数表示造成伤害）。</returns>
    public virtual float GetDamage(NPC npc) => 0f;

    /// <summary>
    /// 获取该持续伤害对指定 NPC 造成的最终伤害值（用于计算伤害数字显示等）。
    /// </summary>
    /// <param name="npc">受影响的 NPC 实例。</param>
    /// <returns>最终的伤害整数值。</returns>
    public virtual int GetDamageValue(NPC npc) => 0;
}