// Developed by ColdsUx

using CalamityMod.NPCs;
using CalamityMod.Projectiles;

namespace Anomalies.Common;

#region General Behavior
public abstract class AnomalyPlayerBehavior : PlayerBehavior
{
    public sealed override AnomalyMain Mod => AnomalyMain.Instance;

    public AnomalyPlayer AnomalyPlayer { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => _Entity.Anomaly; }
}

public abstract class AnomalyGlobalNPCBehavior : GlobalNPCBehavior
{
    public sealed override AnomalyMain Mod => AnomalyMain.Instance;

    /// <summary>
    /// 在更新灾厄的Boss血条之前调用。
    /// </summary>
    /// <returns>返回 <see langword="false"/> 以阻止默认的更新血条方法运行（除对 <see cref="BossHealthBar.Valid"/> 属性的更新之外）。默认返回 <see langword="true"/>。</returns>
    public virtual bool PreUpdateCalBossBar(NPC npc, BossHealthBar newBar, bool hasSingle) => true;

    /// <summary>
    /// 在更新灾厄的Boss血条之后调用。
    /// </summary>
    public virtual void PostUpdateCalBossBar(NPC npc, BossHealthBar newBar, bool hasSingle) { }

    /// <summary>
    /// 在绘制灾厄的Boss血条之前调用。
    /// </summary>
    /// <param name="x">绘制位置左上角的X坐标。</param>
    /// <param name="y">绘制位置左上角的Y坐标。</param>
    /// <returns>返回 <see langword="false"/> 以阻止默认的绘制血条方法运行。默认返回 <see langword="true"/>。</returns>
    public virtual bool PreDrawCalBossBar(NPC npc, BossHealthBar newBar, SpriteBatch spriteBatch, ref int x, ref int y, bool hasSingle) => true;

    /// <summary>
    /// 在绘制灾厄的Boss血条之后调用。
    /// </summary>
    /// <param name="x">绘制位置左上角的X坐标。</param>
    /// <param name="y">绘制位置左上角的Y坐标。</param>
    public virtual void PostDrawCalBossBar(NPC npc, BossHealthBar newBar, SpriteBatch spriteBatch, int x, int y, bool hasSingle) { }
}

public abstract class AnomalyGlobalProjectileBehavior : GlobalProjectileBehavior
{
    public sealed override AnomalyMain Mod => AnomalyMain.Instance;

    /// <summary>
    /// 编辑受击NPC的DR。
    /// </summary>
    /// <param name="baseDR">由灾厄方法计算出的基础DR。</param>
    public virtual void ModifyHitNPC_DR(Projectile projectile, NPC target, ref NPC.HitModifiers modifiers, float baseDR, ref StatModifier baseDRModifier, ref StatModifier standardDRModifier, ref StatModifier timedDRModifier) { }
}

public abstract class AnomalyGlobalItemBehavior : GlobalItemBehavior
{
    public sealed override AnomalyMain Mod => AnomalyMain.Instance;

    /// <summary>
    /// 编辑受击NPC的DR。
    /// </summary>
    /// <param name="baseDR">由灾厄方法计算出的基础DR。</param>
    public virtual void ModifyHitNPC_DR(Item item, NPC target, Player player, ref NPC.HitModifiers modifiers, float baseDR, ref StatModifier baseDRModifier, ref StatModifier standardDRModifier, ref StatModifier timedDRModifier) { }
}
#endregion General Behavior

#region Single Behavior
public enum CalamityLogicType_NPCBehavior
{
    VanillaOverrideAI,

    PreAI,
    GetAlpha,
    PreDraw,
}

public abstract class AnomalySingleNPCBehavior : SingleNPCBehavior
{
    public sealed override AnomalyMain Mod => AnomalyMain.Instance;

    public AnomalyGlobalNPC AnomalyNPC { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => _Entity.Anomaly; }

    /// <summary>
    /// 是否允许灾厄的相关逻辑执行。
    /// <br/>默认返回 <see langword="true"/>，即全部允许。
    /// </summary>
    public virtual bool AllowCalamityLogic(CalamityLogicType_NPCBehavior type) => true;

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

public abstract class AnomalySingleNPCBehavior<T> : AnomalySingleNPCBehavior where T : ModNPC
{
    public static readonly Type Type = typeof(T);

    public T ModNPC => _Entity.GetModNPC<T>();

    public override int ApplyingType => ModContent.NPCType<T>();
}

public abstract class AnomalyNPCBehavior<TBehavior> : AnomalySingleNPCBehavior where TBehavior : AnomalyNPCBehavior<TBehavior>, new()
{
    public override decimal Priority => 100m;

    public override bool ShouldProcess => AnomalySharedData.Anomaly && (AnomalyNPC?.ShouldRunAnomalyAI ?? false);

    public static TBehavior GetInstance(NPC npc) => new() { _Entity = npc };
}

public abstract class AnomalyNPCBehavior<TModNPC, TBehavior> : AnomalySingleNPCBehavior<TModNPC>
    where TModNPC : ModNPC
    where TBehavior : AnomalyNPCBehavior<TModNPC, TBehavior>, new()
{
    public override decimal Priority => 100m;

    public override bool ShouldProcess => AnomalySharedData.Anomaly && (AnomalyNPC?.ShouldRunAnomalyAI ?? false);

    public static TBehavior GetInstance(NPC npc) => new() { _Entity = npc };
}

public enum CalamityLogicType_ProjectileBehavior
{
    PreAI,
    GetAlpha,
    PreDraw,
}

public abstract class AnomalySingleProjectileBehavior : SingleProjectileBehavior
{
    public sealed override AnomalyMain Mod => AnomalyMain.Instance;

    public AnomalyGlobalProjectile AnomalyProjectile { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => _Entity.Anomaly; }

    /// <summary>
    /// 是否允许灾厄的相关逻辑执行。
    /// <br/>默认返回 <see langword="true"/>，即全部允许。
    /// </summary>
    public virtual bool AllowCalamityLogic(CalamityLogicType_ProjectileBehavior type) => true;

    /// <summary>
    /// 编辑受击NPC的DR。
    /// </summary>
    /// <param name="baseDR">由灾厄方法计算出的基础DR。</param>
    public virtual void ModifyHitNPC_DR(NPC target, ref NPC.HitModifiers modifiers, float baseDR, ref StatModifier baseDRModifier, ref StatModifier standardDRModifier, ref StatModifier timedDRModifier) { }
}

public abstract class AnomalySingleProjectileBehavior<T> : AnomalySingleProjectileBehavior where T : ModProjectile
{
    public static readonly Type Type = typeof(T);

    public T ModProjectile => _Entity.GetModProjectile<T>();

    public override int ApplyingType => ModContent.ProjectileType<T>();
}

public abstract class AnomalyProjectileBehavior<TBehavior> : AnomalySingleProjectileBehavior where TBehavior : AnomalyProjectileBehavior<TBehavior>, new()
{
    public override decimal Priority => 100m;

    public override bool ShouldProcess => AnomalySharedData.Anomaly && (AnomalyProjectile?.ShouldRunAnomalyAI ?? false);

    public static TBehavior GetInstance(Projectile projectile) => new() { _Entity = projectile };
}

public abstract class AnomalyProjectileBehavior<TModProjectile, TBehavior> : AnomalySingleProjectileBehavior<TModProjectile>
    where TModProjectile : ModProjectile
    where TBehavior : AnomalyProjectileBehavior<TModProjectile, TBehavior>, new()
{
    public override decimal Priority => 100m;

    public override bool ShouldProcess => AnomalySharedData.Anomaly && (AnomalyProjectile?.ShouldRunAnomalyAI ?? false);

    public static TBehavior GetInstance(Projectile projectile) => new() { _Entity = projectile };
}

public abstract class AnomalySingleItemBehavior : SingleItemBehavior
{
    public sealed override AnomalyMain Mod => AnomalyMain.Instance;

    public AnomalyGlobalItem AnomalyItem { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => _Entity.Anomaly; }

    /// <summary>
    /// 编辑受击NPC的DR。
    /// </summary>
    /// <param name="baseDR">由灾厄方法计算出的基础DR。</param>
    public virtual void ModifyHitNPC_DR(NPC target, Player player, ref NPC.HitModifiers modifiers, float baseDR, ref StatModifier baseDRModifier, ref StatModifier standardDRModifier, ref StatModifier timedDRModifier) { }
}

public abstract class AnomalySingleItemBehavior<T> : AnomalySingleItemBehavior where T : ModItem
{
    public static readonly Type Type = typeof(T);

    public T ModItem => _Entity.GetModItem<T>();

    public override int ApplyingType => ModContent.ItemType<T>();
}
#endregion Single Behavior

#region Tweak
public interface ICATweak
{
    public abstract void RegisterTweak();
}

public abstract class AnomalyNPCTweak : AnomalySingleNPCBehavior, IAnomalyLocalizationPrefix, ICATweak
{
    public abstract AnomalyGamePhase Phase { get; }
    public abstract string LocalizationName { get; }

    void ICATweak.RegisterTweak() => AnomalySharedData.TweakedNPCs[ApplyingType] = true;

    public override decimal Priority => 5m;
}

public abstract class AnomalyNPCTweak<T> : AnomalySingleNPCBehavior<T>, IAnomalyLocalizationPrefix, ICATweak where T : ModNPC
{
    public abstract AnomalyGamePhase Phase { get; }
    public virtual string LocalizationName => Type.Name;

    void ICATweak.RegisterTweak() => AnomalySharedData.TweakedNPCs[ApplyingType] = true;

    public override decimal Priority => 5m;
}

public abstract class AnomalyProjectileTweak : AnomalySingleProjectileBehavior, IAnomalyLocalizationPrefix, ICATweak
{
    public abstract AnomalyGamePhase Phase { get; }
    public abstract string LocalizationName { get; }

    void ICATweak.RegisterTweak() => AnomalySharedData.TweakedProjectiles[ApplyingType] = true;

    public override decimal Priority => 5m;
}

public abstract class AnomalyProjectileTweak<T> : AnomalySingleProjectileBehavior<T>, IAnomalyLocalizationPrefix, ICATweak where T : ModProjectile
{
    public abstract AnomalyGamePhase Phase { get; }
    public virtual string LocalizationName => Type.Name;

    /// <summary>
    /// 弹幕关联的NPC。将应用于显示NPC的修改标签。
    /// <br/>如无关联NPC，不要覆写该方法，如果覆写应返回空集合。
    /// </summary>
    public virtual int[] RelatedNPCs => [];
    /// <summary>
    /// 弹幕关联的物品。将应用于显示物品的修改标签。
    /// <br/>如无关联物品，不要覆写该方法，如果覆写应返回空集合。
    /// </summary>
    public virtual int[] RelatedItems => [];

    void ICATweak.RegisterTweak()
    {
        AnomalySharedData.TweakedProjectiles[ApplyingType] = true;
        foreach (int npcType in RelatedNPCs)
            AnomalySharedData.TweakedNPCs[npcType] = true;
        foreach (int itemType in RelatedItems)
            AnomalySharedData.TweakedItems[itemType] = true;
    }

    public override decimal Priority => 5m;
}

public abstract class AnomalyItemTweak : AnomalySingleItemBehavior, IAnomalyLocalizationPrefix, ICATweak
{
    public abstract AnomalyGamePhase Phase { get; }
    public abstract string LocalizationName { get; }

    void ICATweak.RegisterTweak() => AnomalySharedData.TweakedItems[ApplyingType] = true;

    public override decimal Priority => 5m;
}

public abstract class AnomalyItemTweak<T> : AnomalySingleItemBehavior<T>, IAnomalyLocalizationPrefix, ICATweak where T : ModItem
{
    public abstract AnomalyGamePhase Phase { get; }
    public virtual string LocalizationName => Type.Name;

    void ICATweak.RegisterTweak() => AnomalySharedData.TweakedItems[ApplyingType] = true;

    public override decimal Priority => 5m;
}
#endregion

#region Handler
public sealed class AnomalySingleNPCBehaviorHandler : SingleNPCBehaviorHandler<AnomalySingleNPCBehavior>
{
    public override AnomalyMain Mod => AnomalyMain.Instance;

    public override decimal Priority => 50m;

    protected override SingleEntityBehaviorSet<NPC, AnomalySingleNPCBehavior> BehaviorSet => AnomalyEntityChangeHelper.NPCBehaviors;
}

public sealed class AnomalySingleProjectileBehaviorHandler : SingleProjectileBehaviorHandler<AnomalySingleProjectileBehavior>
{
    public override AnomalyMain Mod => AnomalyMain.Instance;

    public override decimal Priority => 50m;

    protected override SingleEntityBehaviorSet<Projectile, AnomalySingleProjectileBehavior> BehaviorSet => AnomalyEntityChangeHelper.ProjectileBehaviors;
}

public sealed class AnomalySingleItemBehaviorHandler : SingleItemBehaviorHandler<AnomalySingleItemBehavior>
{
    public override AnomalyMain Mod => AnomalyMain.Instance;

    public override decimal Priority => 50m;

    protected override SingleEntityBehaviorSet<Item, AnomalySingleItemBehavior> BehaviorSet => AnomalyEntityChangeHelper.ItemBehaviors;
}

public sealed class AnomalyEntityChangeHelper : IContentLoader
{
    internal static readonly SingleEntityBehaviorSet<NPC, AnomalySingleNPCBehavior> NPCBehaviors = new();

    internal static readonly SingleEntityBehaviorSet<Projectile, AnomalySingleProjectileBehavior> ProjectileBehaviors = new();

    internal static readonly SingleEntityBehaviorSet<Item, AnomalySingleItemBehavior> ItemBehaviors = new();

    void IContentLoader.PostSetupContent()
    {
        Assembly assembly = AnomalySharedData.Assembly;
        NPCBehaviors.FillSet(assembly);
        ProjectileBehaviors.FillSet(assembly);
        ItemBehaviors.FillSet(assembly);

        foreach (ICATweak tweak in TOReflectionUtils.GetTypeInstancesDerivedFrom<ICATweak>(AnomalySharedData.Assembly))
            tweak.RegisterTweak();
    }

    void IContentLoader.OnModUnload()
    {
        NPCBehaviors.Clear();
        ProjectileBehaviors.Clear();
        ItemBehaviors.Clear();
    }
}
#endregion Handler

#region Detour
[ExtendsFromMod(CalamityModName)]
public sealed class CalamityGlobalNPCBehaviorDetour : GlobalNPCDetour<CalamityGlobalNPC>
{
    public override bool Detour_PreAI(Orig_PreAI orig, CalamityGlobalNPC self, NPC npc)
    {
        if (npc.TryGetBehavior(out AnomalySingleNPCBehavior npcBehavior, nameof(AnomalySingleNPCBehavior.PreAI))
            && !npcBehavior.AllowCalamityLogic(CalamityLogicType_NPCBehavior.PreAI))
            return true;

        return orig(self, npc);
    }

    public override Color? Detour_GetAlpha(Orig_GetAlpha orig, CalamityGlobalNPC self, NPC npc, Color drawColor)
    {
        if (npc.TryGetBehavior(out AnomalySingleNPCBehavior npcBehavior, nameof(AnomalySingleNPCBehavior.GetAlpha))
            && !npcBehavior.AllowCalamityLogic(CalamityLogicType_NPCBehavior.GetAlpha))
            return null;

        return orig(self, npc, drawColor);
    }

    public override bool Detour_PreDraw(Orig_PreDraw orig, CalamityGlobalNPC self, NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        if (npc.TryGetBehavior(out AnomalySingleNPCBehavior npcBehavior, nameof(AnomalySingleNPCBehavior.PreDraw))
            && npcBehavior.AllowCalamityLogic(CalamityLogicType_NPCBehavior.PreDraw))
            return true;

        return orig(self, npc, spriteBatch, screenPos, drawColor);
    }
}

[ExtendsFromMod(CalamityModName)]
public sealed class CalamityVanillaAIOverrideDetour : GlobalNPCDetour<CalamityVanillaAIOverrideNPC>
{
    public override bool Detour_PreAI(Orig_PreAI orig, CalamityVanillaAIOverrideNPC self, NPC npc)
    {
        if (npc.TryGetBehavior(out AnomalySingleNPCBehavior npcBehavior, nameof(AnomalySingleNPCBehavior.PreAI))
            && !npcBehavior.AllowCalamityLogic(CalamityLogicType_NPCBehavior.VanillaOverrideAI))
            return true;

        return orig(self, npc);
    }
}

[ExtendsFromMod(CalamityModName)]
public sealed class CalamityGlobalProjectileBehaviorDetour : GlobalProjectileDetour<CalamityGlobalProjectile>
{
    public override bool Detour_PreAI(Orig_PreAI orig, CalamityGlobalProjectile self, Projectile projectile)
    {
        if (projectile.TryGetBehavior(out AnomalySingleProjectileBehavior projectileBehavior, nameof(AnomalySingleProjectileBehavior.PreAI))
            && !projectileBehavior.AllowCalamityLogic(CalamityLogicType_ProjectileBehavior.PreAI))
            return true;

        return orig(self, projectile);
    }

    public override Color? Detour_GetAlpha(Orig_GetAlpha orig, CalamityGlobalProjectile self, Projectile projectile, Color lightColor)
    {
        if (projectile.TryGetBehavior(out AnomalySingleProjectileBehavior projectileBehavior, nameof(AnomalySingleProjectileBehavior.GetAlpha))
            && !projectileBehavior.AllowCalamityLogic(CalamityLogicType_ProjectileBehavior.GetAlpha))
            return null;

        return orig(self, projectile, lightColor);
    }

    public override bool Detour_PreDraw(Orig_PreDraw orig, CalamityGlobalProjectile self, Projectile projectile, ref Color lightColor)
    {
        if (projectile.TryGetBehavior(out AnomalySingleProjectileBehavior projectileBehavior, nameof(AnomalySingleProjectileBehavior.PreDraw))
            && !projectileBehavior.AllowCalamityLogic(CalamityLogicType_ProjectileBehavior.PreDraw))
            return true;

        return orig(self, projectile, ref lightColor);
    }
}
#endregion Detour