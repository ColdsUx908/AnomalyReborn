// Developed by ColdsUx

using CalamityMod.Items;
using CalamityMod.NPCs;
using CalamityMod.Projectiles;

namespace CalamityAnomalies.Core;

public interface ICAModNPC
{
    /// <summary>
    /// 在更新灾厄的Boss血条之前调用。
    /// </summary>
    /// <returns>返回 <see langword="false"/> 以阻止默认的更新血条方法运行（除对 <see cref="BetterBossHPUI.Valid"/> 属性的更新之外）。默认返回 <see langword="true"/>。</returns>
    public virtual bool PreUpdateCalBossBar(BetterBossHPUI newBar) => true;

    /// <summary>
    /// 在更新灾厄的Boss血条之后调用。
    /// </summary>
    public virtual void PostUpdateCalBossBar(BetterBossHPUI newBar) { }

    /// <summary>
    /// 在绘制灾厄的Boss血条之前调用。
    /// </summary>
    /// <param name="x">绘制位置左上角的X坐标。</param>
    /// <param name="y">绘制位置左上角的Y坐标。</param>
    /// <returns>返回 <see langword="false"/> 以阻止默认的绘制血条方法运行。默认返回 <see langword="true"/>。</returns>
    public virtual bool PreDrawCalBossBar(BetterBossHPUI newBar, SpriteBatch spriteBatch, ref int x, ref int y) => true;

    /// <summary>
    /// 在绘制灾厄的Boss血条之后调用。
    /// </summary>
    /// <param name="x">绘制位置左上角的X坐标。</param>
    /// <param name="y">绘制位置左上角的Y坐标。</param>
    public virtual void PostDrawCalBossBar(BetterBossHPUI newBar, SpriteBatch spriteBatch, int x, int y) { }
}

public abstract class CAModNPC : TOModNPC, ICAModNPC
{
    public static bool Ultra => CASharedData.AnomalyUltramundane;

    public CAGlobalNPC AnomalyNPC { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => NPC.Anomaly; }
    public CalamityGlobalNPC CalamityNPC { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => NPC.CalamityNPC; }

    public virtual bool PreUpdateCalBossBar(BetterBossHPUI newBar) => true;
    public virtual void PostUpdateCalBossBar(BetterBossHPUI newBar) { }
    public virtual bool PreDrawCalBossBar(BetterBossHPUI newBar, SpriteBatch spriteBatch, ref int x, ref int y) => true;
    public virtual void PostDrawCalBossBar(BetterBossHPUI newBar, SpriteBatch spriteBatch, int x, int y) { }
}

public interface ICAModProjectile
{
    public virtual void ModifyHitNPC_DR(NPC target, ref NPC.HitModifiers modifiers, float baseDR, ref StatModifier baseDRModifier, ref StatModifier standardDRModifier, ref StatModifier timedDRModifier) { }
}

public abstract class CAModProjectile : TOModProjectile, ICAModProjectile
{
    public static bool Ultra => CASharedData.AnomalyUltramundane;

    public CAGlobalProjectile AnomalyProjectile { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => Projectile.Anomaly; }
    public CalamityGlobalProjectile CalamityProjectile { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => Projectile.CalamityProjectile; }

    public virtual void ModifyHitNPC_DR(NPC target, ref NPC.HitModifiers modifiers, float baseDR, ref StatModifier baseDRModifier, ref StatModifier standardDRModifier, ref StatModifier timedDRModifier) { }
}

public interface ICAModItem
{
    public virtual void ModifyHitNPC_DR(NPC target, Player player, ref NPC.HitModifiers modifiers, float baseDR, ref StatModifier baseDRModifier, ref StatModifier standardDRModifier, ref StatModifier timedDRModifier) { }
}

public abstract class CAModItem : TOModItem, ICAModItem
{
    public static bool Ultra => CASharedData.AnomalyUltramundane;

    public CAGlobalItem AnomalyItem { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => Item.Anomaly; }
    public CalamityGlobalItem CalamityItem { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => Item.CalamityItem; }

    public virtual void ModifyHitNPC_DR(NPC target, Player player, ref NPC.HitModifiers modifiers, float baseDR, ref StatModifier baseDRModifier, ref StatModifier standardDRModifier, ref StatModifier timedDRModifier) { }
}