// Designed by ColdsUx

namespace Transoceanic.DataStructures;

public interface ITOModNPC
{
}

public abstract class TOModNPC : ModNPC, ITOModNPC
{
    public TOGlobalNPC OceanNPC { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => NPC.Ocean; }

    public Player Target => NPC.PlayerTarget;

    public int Timer1
    {
        get => NPC.Timer1;
        set => NPC.Timer1 = value;
    }
    public int Timer2
    {
        get => NPC.Timer2;
        set => NPC.Timer2 = value;
    }
    public int Timer3
    {
        get => NPC.Timer3;
        set => NPC.Timer3 = value;
    }
    public int Timer4
    {
        get => NPC.Timer4;
        set => NPC.Timer4 = value;
    }
    public int Timer5
    {
        get => NPC.Timer5;
        set => NPC.Timer5 = value;
    }
    public float Timer6
    {
        get => NPC.Timer6;
        set => NPC.Timer6 = value;
    }
    public float Timer7
    {
        get => NPC.Timer7;
        set => NPC.Timer7 = value;
    }
    public float Timer8
    {
        get => NPC.Timer8;
        set => NPC.Timer8 = value;
    }

    public Union32 AI_Union_0
    {
        get => (Union32)NPC.ai[0];
        set => NPC.ai[0] = value.f;
    }
    public Union32 AI_Union_1
    {
        get => (Union32)NPC.ai[1];
        set => NPC.ai[1] = value.f;
    }
    public Union32 AI_Union_2
    {
        get => (Union32)NPC.ai[2];
        set => NPC.ai[2] = value.f;
    }
    public Union32 AI_Union_3
    {
        get => (Union32)NPC.ai[3];
        set => NPC.ai[3] = value.f;
    }
    public Union32 LocalAI_Union_0
    {
        get => (Union32)NPC.localAI[0];
        set => NPC.localAI[0] = value.f;
    }
    public Union32 LocalAI_Union_1
    {
        get => (Union32)NPC.localAI[1];
        set => NPC.localAI[1] = value.f;
    }
    public Union32 LocalAI_Union_2
    {
        get => (Union32)NPC.localAI[2];
        set => NPC.localAI[2] = value.f;
    }
    public Union32 LocalAI_Union_3
    {
        get => (Union32)NPC.localAI[3];
        set => NPC.localAI[3] = value.f;
    }

    public IEntitySource SourceAI => NPC.GetSource_FromAI();
}

public interface ITOModProjectile
{
}

public abstract class TOModProjectile : ModProjectile, ITOModProjectile
{
    public TOGlobalProjectile OceanProjectile { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => Projectile.Ocean; }

    public Player Owner => Projectile.Owner;

    public int Timer1
    {
        get => Projectile.Timer1;
        set => Projectile.Timer1 = value;
    }
    public int Timer2
    {
        get => Projectile.Timer2;
        set => Projectile.Timer2 = value;
    }
    public int Timer3
    {
        get => Projectile.Timer3;
        set => Projectile.Timer3 = value;
    }
    public float Timer4
    {
        get => Projectile.Timer4;
        set => Projectile.Timer4 = value;
    }
    public float Timer5
    {
        get => Projectile.Timer5;
        set => Projectile.Timer5 = value;
    }

    public Union32 AI_Union_0
    {
        get => (Union32)Projectile.ai[0];
        set => Projectile.ai[0] = value.f;
    }
    public Union32 AI_Union_1
    {
        get => (Union32)Projectile.ai[0];
        set => Projectile.ai[0] = value.f;
    }
    public Union32 AI_Union_2
    {
        get => (Union32)Projectile.ai[0];
        set => Projectile.ai[0] = value.f;
    }
    public Union32 LocalAI_Union_0
    {
        get => (Union32)Projectile.localAI[0];
        set => Projectile.localAI[0] = value.f;
    }
    public Union32 LocalAI_Union_1
    {
        get => (Union32)Projectile.localAI[1];
        set => Projectile.localAI[1] = value.f;
    }
    public Union32 LocalAI_Union_2
    {
        get => (Union32)Projectile.localAI[2];
        set => Projectile.localAI[2] = value.f;
    }

    public IEntitySource SourceAI => Projectile.GetSource_FromAI();
}

public interface ITOModItem
{
}

public abstract class TOModItem : ModItem, ITOModItem
{
    public TOGlobalItem OceanItem { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => Item.Ocean; }
}
