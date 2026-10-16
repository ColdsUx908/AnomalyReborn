namespace Anomalies.Common;

/// <summary>原版实体包未携带、但模组实体会在运行中修改的公共状态。ai/Ocean/Anomaly 槽由各自的同步层处理。</summary>
public static class AnomalyEntityNetState
{
    /// <summary>写入原版实体包未覆盖的 NPC 公共状态。</summary>
    /// <param name="writer">接收状态数据的二进制写入器。</param>
    /// <param name="npc">提供状态的 NPC。</param>
    public static void WriteNPC(BinaryWriter writer, NPC npc)
    {
        writer.Write(npc.rotation);
        writer.Write(npc.scale);
        writer.Write(npc.alpha);
        writer.Write(npc.width);
        writer.Write(npc.height);
        writer.Write(npc.damage);
        writer.Write(npc.defense);
        writer.Write(npc.dontTakeDamage);
        writer.Write(npc.noGravity);
        writer.Write(npc.noTileCollide);
        writer.Write(npc.chaseable);
    }

    /// <summary>按 <see cref="WriteNPC"/> 的顺序读取并应用 NPC 公共状态。</summary>
    /// <param name="reader">提供状态数据的二进制读取器。</param>
    /// <param name="npc">接收状态的 NPC。</param>
    public static void ReadNPC(BinaryReader reader, NPC npc)
    {
        float rotation = reader.ReadSingle();
        float scale = reader.ReadSingle();
        int alpha = reader.ReadInt32();
        int width = reader.ReadInt32();
        int height = reader.ReadInt32();
        int damage = reader.ReadInt32();
        int defense = reader.ReadInt32();
        bool dontTakeDamage = reader.ReadBoolean();
        bool noGravity = reader.ReadBoolean();
        bool noTileCollide = reader.ReadBoolean();
        bool chaseable = reader.ReadBoolean();
        npc.rotation = rotation;
        npc.scale = scale;
        npc.alpha = alpha;
        npc.width = width;
        npc.height = height;
        npc.damage = damage;
        npc.defense = defense;
        npc.dontTakeDamage = dontTakeDamage;
        npc.noGravity = noGravity;
        npc.noTileCollide = noTileCollide;
        npc.chaseable = chaseable;
    }

    /// <summary>写入模组弹幕额外的公共状态，伤害使用完整的 32 位整数保存。</summary>
    /// <param name="writer">接收状态数据的二进制写入器。</param>
    /// <param name="projectile">提供状态的弹幕。</param>
    public static void WriteProjectile(BinaryWriter writer, Projectile projectile)
    {
        // scale/timeLeft/extraUpdates/tileCollide are already sent by AnomalyGlobalProjectile.
        writer.Write(projectile.rotation);
        writer.Write(projectile.direction);
        writer.Write(projectile.spriteDirection);
        writer.Write(projectile.alpha);
        writer.Write(projectile.width);
        writer.Write(projectile.height);
        // Vanilla SyncProjectile stores these as Int16; modded weapons can exceed that range.
        writer.Write(projectile.damage);
        writer.Write(projectile.originalDamage);
        writer.Write(projectile.penetrate);
        writer.Write(projectile.maxPenetrate);
        writer.Write(projectile.CritChance);
        writer.Write(projectile.ArmorPenetration);
        writer.Write(projectile.localNPCHitCooldown);
        writer.Write(projectile.friendly);
        writer.Write(projectile.hostile);
        writer.Write(projectile.ignoreWater);
        writer.Write(projectile.usesLocalNPCImmunity);
        writer.Write(projectile.usesIDStaticNPCImmunity);
    }

    /// <summary>按 <see cref="WriteProjectile"/> 的顺序读取并应用弹幕公共状态。</summary>
    /// <param name="reader">提供状态数据的二进制读取器。</param>
    /// <param name="projectile">接收状态的弹幕。</param>
    public static void ReadProjectile(BinaryReader reader, Projectile projectile)
    {
        float rotation = reader.ReadSingle();
        int direction = reader.ReadInt32();
        int spriteDirection = reader.ReadInt32();
        int alpha = reader.ReadInt32();
        int width = reader.ReadInt32();
        int height = reader.ReadInt32();
        int damage = reader.ReadInt32();
        int originalDamage = reader.ReadInt32();
        int penetrate = reader.ReadInt32();
        int maxPenetrate = reader.ReadInt32();
        int critChance = reader.ReadInt32();
        int armorPenetration = reader.ReadInt32();
        int localNPCHitCooldown = reader.ReadInt32();
        bool friendly = reader.ReadBoolean();
        bool hostile = reader.ReadBoolean();
        bool ignoreWater = reader.ReadBoolean();
        bool usesLocalNPCImmunity = reader.ReadBoolean();
        bool usesIDStaticNPCImmunity = reader.ReadBoolean();
        projectile.rotation = rotation;
        projectile.direction = direction;
        projectile.spriteDirection = spriteDirection;
        projectile.alpha = alpha;
        projectile.width = width;
        projectile.height = height;
        projectile.damage = damage;
        projectile.originalDamage = originalDamage;
        projectile.penetrate = penetrate;
        projectile.maxPenetrate = maxPenetrate;
        projectile.CritChance = critChance;
        projectile.ArmorPenetration = armorPenetration;
        projectile.localNPCHitCooldown = localNPCHitCooldown;
        projectile.friendly = friendly;
        projectile.hostile = hostile;
        projectile.ignoreWater = ignoreWater;
        projectile.usesLocalNPCImmunity = usesLocalNPCImmunity;
        projectile.usesIDStaticNPCImmunity = usesIDStaticNPCImmunity;
    }
}
