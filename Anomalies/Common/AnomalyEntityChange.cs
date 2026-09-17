using Anomalies.Visuals.BossBar;
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
public interface IAnomalyTweak
{
    public abstract void RegisterTweak();
}

public enum CalamityLogicType_NPCBehavior
{
    VanillaOverrideAI,

    PreAI,
    GetAlpha,
    PreDraw,
}

public abstract class AnomalyNPCBehavior : SingleNPCBehavior, IAnomalyTweak
{
    public sealed override AnomalyMain Mod => AnomalyMain.Instance;

    public AnomalyGlobalNPC AnomalyNPC { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => _Entity.Anomaly; }

    /// <summary>
    /// 设置NPC基本属性，确保在 <see cref="NPCLoader"/> 类的 SetDefaults 方法末尾调用。
    /// </summary>
    public virtual void SetDefaultsFinal() { }

    /// <summary>
    /// 是否允许灾厄的相关逻辑执行。
    /// <br/>默认返回 <see langword="true"/>，即全部允许。
    /// </summary>
    public virtual bool AllowCalamityLogic(CalamityLogicType_NPCBehavior type) => true;

    /// <summary>
    /// 在更新Boss血条之前调用。
    /// </summary>
    /// <param name="newBar">Boss血条实例。</param>
    /// <returns>返回 <see langword="false"/> 以阻止默认的更新血条方法运行（除对 <see cref="BossHealthBar.Valid"/> 属性的更新之外）。默认返回 <see langword="true"/>。</returns>
    public virtual bool PreUpdateBossBar(BossHealthBar newBar) => true;

    /// <summary>
    /// 在更新Boss血条之后调用。
    /// </summary>
    /// <param name="newBar">Boss血条实例。</param>
    public virtual void PostUpdateBossBar(BossHealthBar newBar) { }

    /// <summary>
    /// 在绘制Boss血条之前调用。
    /// </summary>    
    /// <param name="newBar">Boss血条实例。</param>
    /// <param name="spriteBatch">SpriteBatch实例。</param>
    /// <param name="x">绘制位置左上角的X坐标。</param>
    /// <param name="y">绘制位置左上角的Y坐标。</param>
    /// <returns>返回 <see langword="false"/> 以阻止默认的绘制血条方法运行。默认返回 <see langword="true"/>。</returns>
    public virtual bool PreDrawBossBar(BossHealthBar newBar, SpriteBatch spriteBatch, ref int x, ref int y) => true;

    /// <summary>
    /// 在绘制Boss血条之后调用。
    /// </summary>
    /// <param name="x">绘制位置左上角的X坐标。</param>
    /// <param name="y">绘制位置左上角的Y坐标。</param>
    public virtual void PostDrawBossBar(BossHealthBar newBar, SpriteBatch spriteBatch, int x, int y) { }

    /// <summary>
    /// 在绘制Boss血条时应用自定义的着色器。
    /// </summary>
    /// <param name="newBar">Boss血条实例。</param>
    /// <param name="spriteBatch">SpriteBatch实例。</param>
    /// <param name="destinationRentangle">绘制目标矩形。</param>
    /// <remarks>
    /// <b>注意：</b>本方法调用前将自动改变 <paramref name="spriteBatch"/> 状态为：
    /// <list type="bullet">
    /// <item/><description/><see cref="SpriteSortMode.Immediate"/>
    /// <item/><description/><see cref="BlendState.AlphaBlend"/>
    /// <item/><description/><see cref="SamplerState.LinearClamp"/>
    /// </list>
    /// </remarks>
    public virtual void ApplyCustomMainBossBarShader(BossHealthBar newBar, SpriteBatch spriteBatch, Rectangle destinationRentangle) { }

    public delegate void Orig_SetDefaults(NPC npc, bool createModNPC = true);
    [DetourMethodTo(typeof(NPCLoader))]
    public static void Detour_SetDefaults(Orig_SetDefaults orig, NPC npc, bool createModNPC = true)
    {
        orig(npc, createModNPC);

        if (npc.TryGetBehavior(out AnomalyNPCBehavior npcBehavior, nameof(SetDefaultsFinal)))
            npcBehavior.SetDefaultsFinal();
    }

    void IAnomalyTweak.RegisterTweak() => AnomalySharedData.TweakedNPCs[ApplyingType] = true;
}

public abstract class AnomalyNPCBehavior<TBehavior> : AnomalyNPCBehavior where TBehavior : AnomalyNPCBehavior<TBehavior>, new()
{
    public override decimal Priority => 100m;

    public override bool ShouldProcess => AnomalySharedData.Anomaly && (AnomalyNPC?.ShouldRunAnomalyAI ?? false);

    public static TBehavior GetInstance(NPC npc) => new() { _Entity = npc };
}

public abstract class AnomalyNPCBehavior<TModNPC, TBehavior> : AnomalyNPCBehavior<TBehavior>
    where TModNPC : ModNPC
    where TBehavior : AnomalyNPCBehavior<TModNPC, TBehavior>, new()
{
    public static readonly Type Type = typeof(TModNPC);

    public TModNPC ModNPC => _Entity.GetModNPC<TModNPC>();

    public override int ApplyingType => ModContent.NPCType<TModNPC>();

    public override decimal Priority => 100m;

    public override bool ShouldProcess => AnomalySharedData.Anomaly && (AnomalyNPC?.ShouldRunAnomalyAI ?? false);
}

public enum CalamityLogicType_ProjectileBehavior
{
    PreAI,
    GetAlpha,
    PreDraw,
}

public abstract class AnomalyProjectileBehavior : SingleProjectileBehavior, IAnomalyTweak
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

    void IAnomalyTweak.RegisterTweak() => AnomalySharedData.TweakedProjectiles[ApplyingType] = true;
}

public abstract class AnomalyProjectileBehavior<TBehavior> : AnomalyProjectileBehavior where TBehavior : AnomalyProjectileBehavior<TBehavior>, new()
{
    public override decimal Priority => 100m;

    public override bool ShouldProcess => AnomalySharedData.Anomaly && (AnomalyProjectile?.ShouldRunAnomalyAI ?? false);

    public static TBehavior GetInstance(Projectile projectile) => new() { _Entity = projectile };
}

public abstract class AnomalyProjectileBehavior<TModProjectile, TBehavior> : AnomalyProjectileBehavior<TBehavior>
    where TModProjectile : ModProjectile
    where TBehavior : AnomalyProjectileBehavior<TModProjectile, TBehavior>, new()
{
    public static readonly Type Type = typeof(TModProjectile);

    public TModProjectile ModProjectile => _Entity.GetModProjectile<TModProjectile>();

    public override int ApplyingType => ModContent.ProjectileType<TModProjectile>();

    public override decimal Priority => 100m;

    public override bool ShouldProcess => AnomalySharedData.Anomaly && (AnomalyProjectile?.ShouldRunAnomalyAI ?? false);
}

public abstract class AnomalyItemBehavior : SingleItemBehavior, IAnomalyTweak, IContentLoader
{
    public sealed override AnomalyMain Mod => AnomalyMain.Instance;

    public AnomalyGlobalItem AnomalyItem { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => _Entity.Anomaly; }

    /// <summary>
    /// 设置物品基本属性，确保在 <see cref="Item.SetDefaults(int, bool, Terraria.GameContent.Items.ItemVariant)"/> 方法末尾调用。
    /// </summary>
    public virtual void SetDefaultsFinal() { }

    /// <summary>
    /// 编辑受击NPC的DR。
    /// </summary>
    /// <param name="baseDR">由灾厄方法计算出的基础DR。</param>
    public virtual void ModifyHitNPC_DR(NPC target, Player player, ref NPC.HitModifiers modifiers, float baseDR, ref StatModifier baseDRModifier, ref StatModifier standardDRModifier, ref StatModifier timedDRModifier) { }

    void IAnomalyTweak.RegisterTweak() => AnomalySharedData.TweakedItems[ApplyingType] = true;

    void IContentLoader.PostSetupContent() => On_Item.SetDefaults_int_bool_ItemVariant += On_Item_SetDefaults_int_bool_ItemVariant;

    private static void On_Item_SetDefaults_int_bool_ItemVariant(On_Item.orig_SetDefaults_int_bool_ItemVariant orig, Item self, int Type, bool noMatCheck, Terraria.GameContent.Items.ItemVariant variant)
    {
        orig(self, Type, noMatCheck, variant);
        if (self.TryGetBehavior(out AnomalyItemBehavior itemBehavior, nameof(SetDefaultsFinal)))
            itemBehavior.SetDefaultsFinal();
    }
}

public abstract class AnomalyItemBehavior<TBehavior> : AnomalyItemBehavior where TBehavior : AnomalyItemBehavior<TBehavior>, new()
{
    public static TBehavior GetInstance(Item item) => new() { _Entity = item };
}

public abstract class AnomalyItemBehavior<TModItem, TBehavior> : AnomalyItemBehavior<TBehavior> where TModItem : ModItem where TBehavior : AnomalyItemBehavior<TModItem, TBehavior>, new()
{
    public static readonly Type Type = typeof(TModItem);

    public TModItem ModItem => _Entity.GetModItem<TModItem>();

    public override int ApplyingType => ModContent.ItemType<TModItem>();
}
#endregion Single Behavior

#region Handler
public sealed class AnomalySingleNPCBehaviorHandler : SingleNPCBehaviorHandler<AnomalyNPCBehavior>
{
    public override AnomalyMain Mod => AnomalyMain.Instance;

    public override decimal Priority => 50m;

    protected override SingleEntityBehaviorSet<NPC, AnomalyNPCBehavior> BehaviorSet => AnomalyEntityChangeHelper.NPCBehaviors;
}

public sealed class AnomalySingleProjectileBehaviorHandler : SingleProjectileBehaviorHandler<AnomalyProjectileBehavior>
{
    public override AnomalyMain Mod => AnomalyMain.Instance;

    public override decimal Priority => 50m;

    protected override SingleEntityBehaviorSet<Projectile, AnomalyProjectileBehavior> BehaviorSet => AnomalyEntityChangeHelper.ProjectileBehaviors;
}

public sealed class AnomalySingleItemBehaviorHandler : SingleItemBehaviorHandler<AnomalyItemBehavior>
{
    public override AnomalyMain Mod => AnomalyMain.Instance;

    public override decimal Priority => 50m;

    protected override SingleEntityBehaviorSet<Item, AnomalyItemBehavior> BehaviorSet => AnomalyEntityChangeHelper.ItemBehaviors;
}

public sealed class AnomalyEntityChangeHelper : IContentLoader
{
    internal static readonly SingleEntityBehaviorSet<NPC, AnomalyNPCBehavior> NPCBehaviors = new();

    internal static readonly SingleEntityBehaviorSet<Projectile, AnomalyProjectileBehavior> ProjectileBehaviors = new();

    internal static readonly SingleEntityBehaviorSet<Item, AnomalyItemBehavior> ItemBehaviors = new();

    void IContentLoader.PostSetupContent()
    {
        Assembly assembly = AnomalySharedData.Assembly;
        NPCBehaviors.FillSet(assembly);
        ProjectileBehaviors.FillSet(assembly);
        ItemBehaviors.FillSet(assembly);

        foreach (IAnomalyTweak tweak in TOReflectionUtils.GetTypeInstancesDerivedFrom<IAnomalyTweak>(AnomalySharedData.Assembly))
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
        if (npc.TryGetBehavior(out AnomalyNPCBehavior npcBehavior, nameof(AnomalyNPCBehavior.PreAI))
            && !npcBehavior.AllowCalamityLogic(CalamityLogicType_NPCBehavior.PreAI))
            return true;

        return orig(self, npc);
    }

    public override Color? Detour_GetAlpha(Orig_GetAlpha orig, CalamityGlobalNPC self, NPC npc, Color drawColor)
    {
        if (npc.TryGetBehavior(out AnomalyNPCBehavior npcBehavior, nameof(AnomalyNPCBehavior.GetAlpha))
            && !npcBehavior.AllowCalamityLogic(CalamityLogicType_NPCBehavior.GetAlpha))
            return null;

        return orig(self, npc, drawColor);
    }

    public override bool Detour_PreDraw(Orig_PreDraw orig, CalamityGlobalNPC self, NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        if (npc.TryGetBehavior(out AnomalyNPCBehavior npcBehavior, nameof(AnomalyNPCBehavior.PreDraw))
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
        if (npc.TryGetBehavior(out AnomalyNPCBehavior npcBehavior, nameof(AnomalyNPCBehavior.PreAI))
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
        if (projectile.TryGetBehavior(out AnomalyProjectileBehavior projectileBehavior, nameof(AnomalyProjectileBehavior.PreAI))
            && !projectileBehavior.AllowCalamityLogic(CalamityLogicType_ProjectileBehavior.PreAI))
            return true;

        return orig(self, projectile);
    }

    public override Color? Detour_GetAlpha(Orig_GetAlpha orig, CalamityGlobalProjectile self, Projectile projectile, Color lightColor)
    {
        if (projectile.TryGetBehavior(out AnomalyProjectileBehavior projectileBehavior, nameof(AnomalyProjectileBehavior.GetAlpha))
            && !projectileBehavior.AllowCalamityLogic(CalamityLogicType_ProjectileBehavior.GetAlpha))
            return null;

        return orig(self, projectile, lightColor);
    }

    public override bool Detour_PreDraw(Orig_PreDraw orig, CalamityGlobalProjectile self, Projectile projectile, ref Color lightColor)
    {
        if (projectile.TryGetBehavior(out AnomalyProjectileBehavior projectileBehavior, nameof(AnomalyProjectileBehavior.PreDraw))
            && !projectileBehavior.AllowCalamityLogic(CalamityLogicType_ProjectileBehavior.PreDraw))
            return true;

        return orig(self, projectile, ref lightColor);
    }
}
#endregion Detour
