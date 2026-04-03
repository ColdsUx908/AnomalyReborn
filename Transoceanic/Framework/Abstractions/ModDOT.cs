namespace Transoceanic.Framework.Abstractions;

public abstract class ModDOT : ModBuff
{
    public override void SetStaticDefaults()
    {
        Main.debuff[Type] = true;
        Main.buffNoSave[Type] = true;
    }

    public abstract bool HasBuff(Player player);

    public abstract bool HasBuff(NPC npc);

    public virtual float GetDamage(Player player) => 0f;

    public virtual float GetDamage(NPC npc) => 0f;

    public virtual int GetDamageValue(NPC npc) => 0;
}