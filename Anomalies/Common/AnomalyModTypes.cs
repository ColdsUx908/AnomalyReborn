// Developed by ColdsUx

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
    public AnomalyGlobalNPC AnomalyNPC { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => NPC.Anomaly; }

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
    public AnomalyGlobalProjectile AnomalyProjectile { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => Projectile.Anomaly; }

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

    public void AddDeveloperItemIdentifier(List<TooltipLine> tooltips, int index) => tooltips.Insert(index, new TooltipLine(Mod, "Tooltip_DeveloperItemIdentifier", Language.GetTextValue(AnomalySharedData.ModLocalizationPrefix + "GameContents.DeveloperItemIdentifier")) { OverrideColor = AnomalySharedData.UltraIdentifierColor });
    public void AddContributorItemIdentifier(List<TooltipLine> tooltips, int index) => tooltips.Insert(index, new TooltipLine(Mod, "Tooltip_ContributorItemIdentifier", Language.GetTextValue(AnomalySharedData.ModLocalizationPrefix + "GameContents.ContributorItemIdentifier")) { OverrideColor = AnomalySharedData.AnomalyUltramundaneColor });
}