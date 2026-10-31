using Anomalies.Visuals.BossBar;

namespace Anomalies.Common;

public interface IAnomalyModNPC
{
    /// <summary>
    /// 在更新灾厄的Boss血条之前调用。
    /// </summary>
    /// <returns>返回 <see langword="false"/> 以阻止默认的更新血条方法运行（除对 <see cref="BossHealthBar.Valid"/> 属性的更新之外）。默认返回 <see langword="true"/>。</returns>
    public virtual bool PreUpdateCalBossBar(BossHealthBar newBar) => true;

    /// <summary>
    /// 在更新灾厄的Boss血条之后调用。
    /// </summary>
    public virtual void PostUpdateCalBossBar(BossHealthBar newBar) { }

    /// <summary>
    /// 在绘制灾厄的Boss血条之前调用。
    /// </summary>
    /// <param name="x">绘制位置左上角的X坐标。</param>
    /// <param name="y">绘制位置左上角的Y坐标。</param>
    /// <returns>返回 <see langword="false"/> 以阻止默认的绘制血条方法运行。默认返回 <see langword="true"/>。</returns>
    public virtual bool PreDrawCalBossBar(BossHealthBar newBar, SpriteBatch spriteBatch, ref int x, ref int y) => true;

    /// <summary>
    /// 在绘制灾厄的Boss血条之后调用。
    /// </summary>
    /// <param name="x">绘制位置左上角的X坐标。</param>
    /// <param name="y">绘制位置左上角的Y坐标。</param>
    public virtual void PostDrawCalBossBar(BossHealthBar newBar, SpriteBatch spriteBatch, int x, int y) { }
}

public abstract class AnomalyModNPC : TOModNPC, IAnomalyModNPC
{
    /// <summary>内容使用的 AI32 数据槽数，不含变化标记。</summary>
    protected virtual int AI32Capacity => 4;
    /// <summary>内容使用的 AI64 数据槽数，不含变化标记。</summary>
    protected virtual int AI64Capacity => 1;

    public AnomalyGlobalNPC AnomalyNPC
    {
        get
        {
            AnomalyGlobalNPC data = NPC.Anomaly;
            data.EnsureAICapacity(AI32Capacity, AI64Capacity);
            return data;
        }
    }

    // Seal the public pair so a derived payload cannot accidentally skip shared state.
    public sealed override void SendExtraAI(BinaryWriter writer)
    {
        AnomalyEntityNetState.WriteNPC(writer, NPC);
        SendAnomalyExtraAI(writer);
    }

    public sealed override void ReceiveExtraAI(BinaryReader reader)
    {
        AnomalyEntityNetState.ReadNPC(reader, NPC);
        ReceiveAnomalyExtraAI(reader);
    }

    protected virtual void SendAnomalyExtraAI(BinaryWriter writer) { }
    protected virtual void ReceiveAnomalyExtraAI(BinaryReader reader) { }

    public virtual bool PreUpdateCalBossBar(BossHealthBar newBar) => true;
    public virtual void PostUpdateCalBossBar(BossHealthBar newBar) { }
    public virtual bool PreDrawCalBossBar(BossHealthBar newBar, SpriteBatch spriteBatch, ref int x, ref int y) => true;
    public virtual void PostDrawCalBossBar(BossHealthBar newBar, SpriteBatch spriteBatch, int x, int y) { }
}

public interface IAnomalyModProjectile
{
    public virtual void ModifyHitNPC_DR(NPC target, ref NPC.HitModifiers modifiers, float baseDR, ref StatModifier baseDRModifier, ref StatModifier standardDRModifier, ref StatModifier timedDRModifier) { }
}

public abstract class AnomalyModProjectile : TOModProjectile, IAnomalyModProjectile
{
    /// <summary>内容使用的 AI32 数据槽数，不含变化标记。</summary>
    protected virtual int AI32Capacity => 4;
    /// <summary>内容使用的 AI64 数据槽数，不含变化标记。</summary>
    protected virtual int AI64Capacity => 1;

    public AnomalyGlobalProjectile AnomalyProjectile
    {
        get
        {
            AnomalyGlobalProjectile data = Projectile.Anomaly;
            data.EnsureAICapacity(AI32Capacity, AI64Capacity);
            return data;
        }
    }

    public sealed override void SendExtraAI(BinaryWriter writer)
    {
        AnomalyEntityNetState.WriteProjectile(writer, Projectile);
        SendAnomalyExtraAI(writer);
    }

    public sealed override void ReceiveExtraAI(BinaryReader reader)
    {
        AnomalyEntityNetState.ReadProjectile(reader, Projectile);
        ReceiveAnomalyExtraAI(reader);
    }

    protected virtual void SendAnomalyExtraAI(BinaryWriter writer) { }
    protected virtual void ReceiveAnomalyExtraAI(BinaryReader reader) { }

    public virtual void ModifyHitNPC_DR(NPC target, ref NPC.HitModifiers modifiers, float baseDR, ref StatModifier baseDRModifier, ref StatModifier standardDRModifier, ref StatModifier timedDRModifier) { }
}

public interface IAnomalyModItem
{
    public virtual void ModifyHitNPC_DR(NPC target, Player player, ref NPC.HitModifiers modifiers, float baseDR, ref StatModifier baseDRModifier, ref StatModifier standardDRModifier, ref StatModifier timedDRModifier) { }
}

public abstract class AnomalyModItem : TOModItem, IAnomalyModItem
{
    public AnomalyGlobalItem AnomalyItem { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => Item.Anomaly; }

    public virtual void ModifyHitNPC_DR(NPC target, Player player, ref NPC.HitModifiers modifiers, float baseDR, ref StatModifier baseDRModifier, ref StatModifier standardDRModifier, ref StatModifier timedDRModifier) { }
}
