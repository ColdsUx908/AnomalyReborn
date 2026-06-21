// Developed by ColdsUx

using CalamityMod.CalPlayer;
using CalamityMod.Items;
using CalamityMod.NPCs;
using CalamityMod.Projectiles;

namespace CalamityAnomalies.Common;

#region General Behavior
public abstract class CAPlayerBehavior : PlayerBehavior
{
    public sealed override CAMain Mod => CAMain.Instance;

    public CAPlayer AnomalyPlayer { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => _entity.Anomaly; }
    public CalamityPlayer CalamityPlayer { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => _entity.CalamityPlayer; }
}

public abstract class CAGlobalNPCBehavior : GlobalNPCBehavior
{
    public sealed override CAMain Mod => CAMain.Instance;

    /// <summary>
    /// 在更新灾厄的Boss血条之前调用。
    /// </summary>
    /// <returns>返回 <see langword="false"/> 以阻止默认的更新血条方法运行（除对 <see cref="BetterBossHPUI.Valid"/> 属性的更新之外）。默认返回 <see langword="true"/>。</returns>
    public virtual bool PreUpdateCalBossBar(NPC npc, BetterBossHPUI newBar, bool hasSingle) => true;

    /// <summary>
    /// 在更新灾厄的Boss血条之后调用。
    /// </summary>
    public virtual void PostUpdateCalBossBar(NPC npc, BetterBossHPUI newBar, bool hasSingle) { }

    /// <summary>
    /// 在绘制灾厄的Boss血条之前调用。
    /// </summary>
    /// <param name="x">绘制位置左上角的X坐标。</param>
    /// <param name="y">绘制位置左上角的Y坐标。</param>
    /// <returns>返回 <see langword="false"/> 以阻止默认的绘制血条方法运行。默认返回 <see langword="true"/>。</returns>
    public virtual bool PreDrawCalBossBar(NPC npc, BetterBossHPUI newBar, SpriteBatch spriteBatch, ref int x, ref int y, bool hasSingle) => true;

    /// <summary>
    /// 在绘制灾厄的Boss血条之后调用。
    /// </summary>
    /// <param name="x">绘制位置左上角的X坐标。</param>
    /// <param name="y">绘制位置左上角的Y坐标。</param>
    public virtual void PostDrawCalBossBar(NPC npc, BetterBossHPUI newBar, SpriteBatch spriteBatch, int x, int y, bool hasSingle) { }
}

public abstract class CAGlobalProjectileBehavior : GlobalProjectileBehavior
{
    public sealed override CAMain Mod => CAMain.Instance;

    /// <summary>
    /// 编辑受击NPC的DR。
    /// </summary>
    /// <param name="baseDR">由灾厄方法计算出的基础DR。</param>
    public virtual void ModifyHitNPC_DR(Projectile projectile, NPC target, ref NPC.HitModifiers modifiers, float baseDR, ref StatModifier baseDRModifier, ref StatModifier standardDRModifier, ref StatModifier timedDRModifier) { }
}

public abstract class CAGlobalItemBehavior : GlobalItemBehavior
{
    public sealed override CAMain Mod => CAMain.Instance;

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

public abstract class CASingleNPCBehavior : SingleNPCBehavior
{
    public sealed override CAMain Mod => CAMain.Instance;

    public CAGlobalNPC AnomalyNPC { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => _entity.Anomaly; }
    public CalamityGlobalNPC CalamityNPC { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => _entity.CalamityNPC; }

    /// <summary>
    /// 是否允许灾厄的相关逻辑执行。
    /// <br/>默认返回 <see langword="true"/>，即全部允许。
    /// </summary>
    public virtual bool AllowCalamityLogic(CalamityLogicType_NPCBehavior method) => true;

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

public abstract class CASingleNPCBehavior<T> : CASingleNPCBehavior where T : ModNPC
{
    public static readonly Type Type = typeof(T);

    public T ModNPC => _entity.GetModNPC<T>();

    public override int ApplyingType => ModContent.NPCType<T>();
}

public abstract class AnomalyNPCBehavior : CASingleNPCBehavior
{
    public override decimal Priority => 100m;

    public override bool ShouldProcess => CASharedData.Anomaly && (AnomalyNPC?.ShouldRunAnomalyAI ?? false);
}

public abstract class AnomalyNPCBehavior<T> : CASingleNPCBehavior<T> where T : ModNPC
{
    public override decimal Priority => 100m;

    public override bool ShouldProcess => CASharedData.Anomaly && (AnomalyNPC?.ShouldRunAnomalyAI ?? false);
}

public interface ICASingleNPCBehaviorFactory<TSelf> where TSelf : CASingleNPCBehavior, ICASingleNPCBehaviorFactory<TSelf>, new()
{
    public static TSelf GetNewInstance(NPC npc) => new() { _entity = npc };
}

public enum OrigMethodType_CalamityGlobalProjectile
{
    PreAI,
    GetAlpha,
    PreDraw,
}

public abstract class CASingleProjectileBehavior : SingleProjectileBehavior
{
    public sealed override CAMain Mod => CAMain.Instance;

    public CAGlobalProjectile AnomalyProjectile { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => _entity.Anomaly; }
    public CalamityGlobalProjectile CalamityProjectile { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => _entity.CalamityProjectile; }

    /// <summary>
    /// 是否允许灾厄的相关方法执行。
    /// <br/>默认返回 <see langword="true"/>，即全部允许。
    /// </summary>
    public virtual bool AllowOrigCalMethod(OrigMethodType_CalamityGlobalProjectile type) => true;

    /// <summary>
    /// 编辑受击NPC的DR。
    /// </summary>
    /// <param name="baseDR">由灾厄方法计算出的基础DR。</param>
    public virtual void ModifyHitNPC_DR(NPC target, ref NPC.HitModifiers modifiers, float baseDR, ref StatModifier baseDRModifier, ref StatModifier standardDRModifier, ref StatModifier timedDRModifier) { }
}

public abstract class CASingleProjectileBehavior<T> : CASingleProjectileBehavior where T : ModProjectile
{
    public static readonly Type Type = typeof(T);

    public T ModProjectile => _entity.GetModProjectile<T>();

    public override int ApplyingType => ModContent.ProjectileType<T>();
}

public abstract class AnomalyProjectileBehavior : CASingleProjectileBehavior
{
    public override decimal Priority => 100m;

    public override bool ShouldProcess => CASharedData.Anomaly && (AnomalyProjectile?.ShouldRunAnomalyAI ?? false);
}

public abstract class AnomalyProjectileBehavior<T> : CASingleProjectileBehavior<T> where T : ModProjectile
{
    public override decimal Priority => 100m;

    public override bool ShouldProcess => CASharedData.Anomaly && (AnomalyProjectile?.ShouldRunAnomalyAI ?? false);
}

public abstract class CASingleItemBehavior : SingleItemBehavior
{
    public sealed override CAMain Mod => CAMain.Instance;

    public CAGlobalItem AnomalyItem { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => _entity.Anomaly; }
    public CalamityGlobalItem CalamityItem { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => _entity.CalamityItem; }

    /// <summary>
    /// 编辑受击NPC的DR。
    /// </summary>
    /// <param name="baseDR">由灾厄方法计算出的基础DR。</param>
    public virtual void ModifyHitNPC_DR(NPC target, Player player, ref NPC.HitModifiers modifiers, float baseDR, ref StatModifier baseDRModifier, ref StatModifier standardDRModifier, ref StatModifier timedDRModifier) { }
}

public abstract class CASingleItemBehavior<T> : CASingleItemBehavior where T : ModItem
{
    public static readonly Type Type = typeof(T);

    public T ModItem => _entity.GetModItem<T>();

    public override int ApplyingType => ModContent.ItemType<T>();
}
#endregion Single Behavior

#region Tweak
public interface ICATweak
{
    public abstract void RegisterTweak();
}

public abstract class CANPCTweak : CASingleNPCBehavior, ICALocalizationPrefix, ICATweak
{
    public abstract CAGamePhase Phase { get; }
    public abstract string LocalizationName { get; }

    void ICATweak.RegisterTweak() => CASharedData.TweakedNPCs[ApplyingType] = true;

    public override decimal Priority => 5m;
}

public abstract class CANPCTweak<T> : CASingleNPCBehavior<T>, ICALocalizationPrefix, ICATweak where T : ModNPC
{
    public abstract CAGamePhase Phase { get; }
    public virtual string LocalizationName => Type.Name;

    void ICATweak.RegisterTweak() => CASharedData.TweakedNPCs[ApplyingType] = true;

    public override decimal Priority => 5m;
}

public abstract class CAProjectileTweak : CASingleProjectileBehavior, ICALocalizationPrefix, ICATweak
{
    public abstract CAGamePhase Phase { get; }
    public abstract string LocalizationName { get; }

    void ICATweak.RegisterTweak() => CASharedData.TweakedProjectiles[ApplyingType] = true;

    public override decimal Priority => 5m;
}

public abstract class CAProjectileTweak<T> : CASingleProjectileBehavior<T>, ICALocalizationPrefix, ICATweak where T : ModProjectile
{
    public abstract CAGamePhase Phase { get; }
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
        CASharedData.TweakedProjectiles[ApplyingType] = true;
        foreach (int npcType in RelatedNPCs)
            CASharedData.TweakedNPCs[npcType] = true;
        foreach (int itemType in RelatedItems)
            CASharedData.TweakedItems[itemType] = true;
    }

    public override decimal Priority => 5m;
}

public abstract class CAItemTweak : CASingleItemBehavior, ICALocalizationPrefix, ICATweak
{
    public abstract CAGamePhase Phase { get; }
    public abstract string LocalizationName { get; }

    void ICATweak.RegisterTweak() => CASharedData.TweakedItems[ApplyingType] = true;

    public override decimal Priority => 5m;
}

public abstract class CAItemTweak<T> : CASingleItemBehavior<T>, ICALocalizationPrefix, ICATweak where T : ModItem
{
    public abstract CAGamePhase Phase { get; }
    public virtual string LocalizationName => Type.Name;

    void ICATweak.RegisterTweak() => CASharedData.TweakedItems[ApplyingType] = true;

    public override decimal Priority => 5m;
}
#endregion

#region Handler
public sealed class CASingleNPCBehaviorHandler : SingleNPCBehaviorHandler<CASingleNPCBehavior>
{
    public override CAMain Mod => CAMain.Instance;

    public override decimal Priority => 50m;

    protected override SingleEntityBehaviorSet<NPC, CASingleNPCBehavior> BehaviorSet => CAEntityChangeHelper.NPCBehaviors;
}

public sealed class CASingleProjectileBehaviorHandler : SingleProjectileBehaviorHandler<CASingleProjectileBehavior>
{
    public override CAMain Mod => CAMain.Instance;

    public override decimal Priority => 50m;

    protected override SingleEntityBehaviorSet<Projectile, CASingleProjectileBehavior> BehaviorSet => CAEntityChangeHelper.ProjectileBehaviors;
}

public sealed class CASingleItemBehaviorHandler : SingleItemBehaviorHandler<CASingleItemBehavior>
{
    public override CAMain Mod => CAMain.Instance;

    public override decimal Priority => 50m;

    protected override SingleEntityBehaviorSet<Item, CASingleItemBehavior> BehaviorSet => CAEntityChangeHelper.ItemBehaviors;
}

public sealed class CAEntityChangeHelper : IContentLoader
{
    internal static readonly SingleEntityBehaviorSet<NPC, CASingleNPCBehavior> NPCBehaviors = new();

    internal static readonly SingleEntityBehaviorSet<Projectile, CASingleProjectileBehavior> ProjectileBehaviors = new();

    internal static readonly SingleEntityBehaviorSet<Item, CASingleItemBehavior> ItemBehaviors = new();

    void IContentLoader.PostSetupContent()
    {
        Assembly assembly = CASharedData.Assembly;
        NPCBehaviors.FillSet(assembly);
        ProjectileBehaviors.FillSet(assembly);
        ItemBehaviors.FillSet(assembly);

        foreach (ICATweak tweak in TOReflectionUtils.GetTypeInstancesDerivedFrom<ICATweak>(CASharedData.Assembly))
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
public sealed class CalamityGlobalNPCBehaviorDetour : GlobalNPCDetour<CalamityGlobalNPC>
{
    public override bool Detour_PreAI(Orig_PreAI orig, CalamityGlobalNPC self, NPC npc)
    {
        if (npc.TryGetBehavior(out CASingleNPCBehavior npcBehavior, nameof(CASingleNPCBehavior.PreAI))
            && !npcBehavior.AllowCalamityLogic(CalamityLogicType_NPCBehavior.PreAI))
            return true;

        return orig(self, npc);
    }

    public override Color? Detour_GetAlpha(Orig_GetAlpha orig, CalamityGlobalNPC self, NPC npc, Color drawColor)
    {
        if (npc.TryGetBehavior(out CASingleNPCBehavior npcBehavior, nameof(CASingleNPCBehavior.GetAlpha))
            && !npcBehavior.AllowCalamityLogic(CalamityLogicType_NPCBehavior.GetAlpha))
            return null;

        return orig(self, npc, drawColor);
    }

    public override bool Detour_PreDraw(Orig_PreDraw orig, CalamityGlobalNPC self, NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        if (npc.TryGetBehavior(out CASingleNPCBehavior npcBehavior, nameof(CASingleNPCBehavior.PreDraw))
            && npcBehavior.AllowCalamityLogic(CalamityLogicType_NPCBehavior.PreDraw))
            return true;

        return orig(self, npc, spriteBatch, screenPos, drawColor);
    }
}

public sealed class CalamityVanillaAIOverrideDetour : GlobalNPCDetour<CalamityVanillaAIOverrideNPC>
{
    public override bool Detour_PreAI(Orig_PreAI orig, CalamityVanillaAIOverrideNPC self, NPC npc)
    {
        if (npc.TryGetBehavior(out CASingleNPCBehavior npcBehavior, nameof(CASingleNPCBehavior.PreAI))
            && !npcBehavior.AllowCalamityLogic(CalamityLogicType_NPCBehavior.VanillaOverrideAI))
            return true;

        return orig(self, npc);
    }
}

public sealed class CalamityGlobalProjectileBehaviorDetour : GlobalProjectileDetour<CalamityGlobalProjectile>
{
    public override bool Detour_PreAI(Orig_PreAI orig, CalamityGlobalProjectile self, Projectile projectile)
    {
        if (projectile.TryGetBehavior(out CASingleProjectileBehavior projectileBehavior, nameof(CASingleProjectileBehavior.PreAI))
            && !projectileBehavior.AllowOrigCalMethod(OrigMethodType_CalamityGlobalProjectile.PreAI))
            return true;

        return orig(self, projectile);
    }

    public override Color? Detour_GetAlpha(Orig_GetAlpha orig, CalamityGlobalProjectile self, Projectile projectile, Color lightColor)
    {
        if (projectile.TryGetBehavior(out CASingleProjectileBehavior projectileBehavior, nameof(CASingleProjectileBehavior.GetAlpha))
            && !projectileBehavior.AllowOrigCalMethod(OrigMethodType_CalamityGlobalProjectile.GetAlpha))
            return null;

        return orig(self, projectile, lightColor);
    }

    public override bool Detour_PreDraw(Orig_PreDraw orig, CalamityGlobalProjectile self, Projectile projectile, ref Color lightColor)
    {
        if (projectile.TryGetBehavior(out CASingleProjectileBehavior projectileBehavior, nameof(CASingleProjectileBehavior.PreDraw))
            && !projectileBehavior.AllowOrigCalMethod(OrigMethodType_CalamityGlobalProjectile.PreDraw))
            return true;

        return orig(self, projectile, ref lightColor);
    }
}
#endregion Detour