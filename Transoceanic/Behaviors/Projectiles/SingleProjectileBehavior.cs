using Transoceanic.GlobalInstances;

namespace Transoceanic.Behaviors;

public abstract partial class SingleProjectileBehavior : SingleEntityBehavior<Projectile>
{
    public Projectile Projectile { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => _Entity; }

    public TOGlobalProjectile OceanProjectile { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => _Entity.Ocean; }

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
        set => Projectile.ai[0] = value.F;
    }

    public Union32 AI_Union_1
    {
        get => (Union32)Projectile.ai[0];
        set => Projectile.ai[0] = value.F;
    }

    public Union32 AI_Union_2
    {
        get => (Union32)Projectile.ai[0];
        set => Projectile.ai[0] = value.F;
    }

    public Union32 LocalAI_Union_0
    {
        get => (Union32)Projectile.localAI[0];
        set => Projectile.localAI[0] = value.F;
    }

    public Union32 LocalAI_Union_1
    {
        get => (Union32)Projectile.localAI[1];
        set => Projectile.localAI[1] = value.F;
    }

    public Union32 LocalAI_Union_2
    {
        get => (Union32)Projectile.localAI[2];
        set => Projectile.localAI[2] = value.F;
    }

    public IEntitySource SourceAI => Projectile.GetSource_FromAI();

    /// <inheritdoc cref="GlobalProjectile.CanCutTiles"/>
    public virtual bool? CanCutTiles() => null;

    /// <inheritdoc cref="GlobalProjectile.CanDamage"/>
    public virtual bool? CanDamage() => null;
}
