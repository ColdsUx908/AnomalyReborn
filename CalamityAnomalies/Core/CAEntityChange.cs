using System.Collections.ObjectModel;
using CalamityMod.CalPlayer;
using CalamityMod.Items;
using CalamityMod.NPCs;
using CalamityMod.Projectiles;
using MonoMod.RuntimeDetour;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.UI;
using Transoceanic.Framework.Helpers.AbstractionHandlers;
using Transoceanic.Framework.RuntimeEditing;

namespace CalamityAnomalies.Core;

#region General Behavior
public abstract class CAPlayerBehavior : PlayerBehavior
{
    public sealed override CAMain Mod => CAMain.Instance;

    public CAPlayer AnomalyPlayer { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => _entity.Anomaly; }
    public CalamityPlayer CalamityPlayer { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => _entity.Calamity; }
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
    public static bool Ultra => CASharedData.AnomalyUltramundane;

    public sealed override CAMain Mod => CAMain.Instance;

    public CAGlobalNPC AnomalyNPC { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => _entity.Anomaly; }
    public CalamityGlobalNPC CalamityNPC { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => _entity.Calamity; }

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

public enum OrigMethodType_CalamityGlobalProjectile
{
    PreAI,
    GetAlpha,
    PreDraw,
}

public abstract class CASingleProjectileBehavior : SingleProjectileBehavior
{
    public static bool Ultra => CASharedData.AnomalyUltramundane;

    public sealed override CAMain Mod => CAMain.Instance;

    public CAGlobalProjectile AnomalyProjectile { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => _entity.Anomaly; }
    public CalamityGlobalProjectile CalamityProjectile { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => _entity.Calamity; }

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

public abstract class AnomalyProjecileBehavior<T> : CASingleProjectileBehavior<T> where T : ModProjectile
{
    public override decimal Priority => 100m;

    public override bool ShouldProcess => CASharedData.Anomaly && (AnomalyProjectile?.ShouldRunAnomalyAI ?? false);
}

public abstract class CASingleItemBehavior : SingleItemBehavior
{
    public static bool Ultra => CASharedData.AnomalyUltramundane;

    public sealed override CAMain Mod => CAMain.Instance;

    public CAGlobalItem AnomalyItem { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => _entity.Anomaly; }
    public CalamityGlobalItem CalamityItem { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => _entity.Calamity; }

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

/// <summary>
/// 用于NPC覆盖的抽象类。
/// <para/>设计规范：
/// <br/>1. <strong>避免</strong>在该类中试图使用 <typeparamref name="TSource"/> 类中的任何方法。
/// <br/>2. <strong>尝试</strong>使用属性（必要时使用Publicizer）来访问 <typeparamref name="TSource"/> 中的数据。
/// <br/>3. <strong>避免</strong>在该类中使用实例字段。
/// </summary>
/// <typeparam name="TSource">待覆盖的 <see cref="ModNPC"/> 类。</typeparam>
public abstract class CANPCOverride<TSource> : ModNPCDetour<TSource>, ICALocalizationPrefix, ICATweak where TSource : ModNPC
{
    private static readonly HashSet<string> _exclusiveDetours = ["get_LocalizationCategory", "get_Texture", "get_HeadTexture", "get_BossHeadTexture"];
    private static readonly HashSet<string> _criticalDetours = [];

    public static int ContentType => ModContent.NPCType<TSource>();

    public abstract CAGamePhase Phase { get; }
    public virtual string LocalizationName => Name ?? SourceType.Name;

    void ICATweak.RegisterTweak() => CASharedData.TweakedNPCs[ContentType] = true;

    protected override Hook ApplySingleDetour<TDelegate>(TDelegate detour, bool hasThis = true)
    {
        if (TODetourHandler.EvaluateDetourName(detour.Method, out string sourceName) && SourceType.HasMethod(sourceName, hasThis ? TOReflectionUtils.InstanceBindingFlags : TOReflectionUtils.StaticBindingFlags, out MethodInfo sourceMethod))
        {
            if (_criticalDetours.Contains(sourceName))
                return TODetourHandler.Modify(sourceMethod, detour);
            switch (GetType().HasRealMethod(sourceName, TOReflectionUtils.UniversalBindingFlags), sourceMethod.DeclaringType == SourceType)
            {
                case (true, true):
                    return TODetourHandler.Modify(sourceMethod, detour);
                case (false, true):
                    if (_exclusiveDetours.Contains(sourceName))
                        return null;
                    //CAMain.Instance.Logger.Warn($"""[CA Override] Source method "{sourceName}" does not have an implemented hook, and will not be hooked.""");
                    return null;
                case (_, false):
                    return null;
            }
        }
        return null;
    }

    public TSource Self;
    public NPC NPC => Self.NPC;

    /// <inheritdoc cref="ModNPC.Type"/>
    public int Type => NPC.type;

    /// <inheritdoc cref="ModNPC.AIType"/>
    public int AIType
    {
        get => Self.AIType;
        set => Self.AIType = value;
    }

    /// <inheritdoc cref="ModNPC.AnimationType"/>
    public int AnimationType
    {
        get => Self.AnimationType;
        set => Self.AnimationType = value;
    }

    /// <inheritdoc cref="ModNPC.Music"/>
    public int Music
    {
        get => Self.Music;
        set => Self.Music = value;
    }

    /// <inheritdoc cref="ModNPC.SceneEffectPriority"/>
    public SceneEffectPriority SceneEffectPriority
    {
        get => Self.SceneEffectPriority;
        set => Self.SceneEffectPriority = value;
    }

    /// <inheritdoc cref="ModNPC.DrawOffsetY"/>
    public float DrawOffsetY
    {
        get => Self.DrawOffsetY;
        set => Self.DrawOffsetY = value;
    }

    /// <inheritdoc cref="ModNPC.Banner"/>
    public int Banner
    {
        get => Self.Banner;
        set => Self.Banner = value;
    }

    /// <inheritdoc cref="ModNPC.BannerItem"/>
    public int BannerItem
    {
        get => Self.BannerItem;
        set => Self.BannerItem = value;
    }

    /// <inheritdoc cref="ModNPC.SpawnModBiomes"/>
    public int[] SpawnModBiomes
    {
        get => Self.SpawnModBiomes;
        set => Self.SpawnModBiomes = value;
    }

    /// <inheritdoc cref="ModNPC.TownNPCStayingHomeless"/>
    public bool TownNPCStayingHomeless
    {
        get => Self.TownNPCStayingHomeless;
        set => Self.TownNPCStayingHomeless = value;
    }

    // ModType
    // Not Supported
    public sealed override void Detour_Register(Orig_Register orig, TSource self)
    {
        Self = self;
        base.Detour_Register(orig, self);
    }
    public sealed override void Detour_InitTemplateInstance(Orig_InitTemplateInstance orig, TSource self)
    {
        Self = self;
        base.Detour_InitTemplateInstance(orig, self);
    }
    public sealed override void Detour_ValidateType(Orig_ValidateType orig, TSource self)
    {
        Self = self;
        base.Detour_ValidateType(orig, self);
    }

    // Name
    public sealed override string Detour_get_Name(Orig_get_Name orig, TSource self)
    {
        Self = self;
        return Name;
    }
    /// <inheritdoc cref="ModType.Name"/>
    public virtual string Name => Self.Name;

    // Load
    public sealed override void Detour_Load(Orig_Load orig, TSource self)
    {
        Self = self;
        Load();
    }
    /// <inheritdoc cref="ModType.Load"/>
    public virtual void Load() { }

    // IsLoadingEnabled
    public sealed override bool Detour_IsLoadingEnabled(Orig_IsLoadingEnabled orig, TSource self, Mod mod)
    {
        Self = self;
        return IsLoadingEnabled(mod);
    }
    /// <inheritdoc cref="ModType.IsLoadingEnabled(Mod)"/>
    public virtual bool IsLoadingEnabled(Mod mod) => true;

    // SetupContent
    public sealed override void Detour_SetupContent(Orig_SetupContent orig, TSource self)
    {
        Self = self;
        SetupContent();
    }
    /// <inheritdoc cref="ModType.SetupContent"/>
    public virtual void SetupContent() { }

    // SetStaticDefaults
    public sealed override void Detour_SetStaticDefaults(Orig_SetStaticDefaults orig, TSource self)
    {
        Self = self;
        SetStaticDefaults();
    }
    /// <inheritdoc cref="ModType.SetStaticDefaults"/>
    public virtual void SetStaticDefaults() { }

    // Unload
    public sealed override void Detour_Unload(Orig_Unload orig, TSource self)
    {
        Self = self;
        Unload();
    }
    /// <inheritdoc cref="ModType.Unload"/>
    public virtual void Unload() { }

    // ModNPC
    // LocalizationCategory
    public sealed override string Detour_get_LocalizationCategory(Orig_get_LocalizationCategory orig, TSource self)
    {
        Self = self;
        return LocalizationCategory;
    }
    /// <inheritdoc cref="ModNPC.LocalizationCategory"/>
    public virtual string LocalizationCategory => "NPCs";

    // DisplayName
    public sealed override LocalizedText Detour_get_DisplayName(Orig_get_DisplayName orig, TSource self)
    {
        Self = self;
        return DisplayName;
    }
    /// <inheritdoc cref="ModNPC.DisplayName"/>
    public virtual LocalizedText DisplayName => Self.GetLocalization("DisplayName", Self.PrettyPrintName);

    // DeathMessage
    public sealed override LocalizedText Detour_get_DeathMessage(Orig_get_DeathMessage orig, TSource self)
    {
        Self = self;
        return DeathMessage;
    }
    /// <inheritdoc cref="ModNPC.DeathMessage"/>
    public virtual LocalizedText DeathMessage => null;

    // Texture
    public sealed override string Detour_get_Texture(Orig_get_Texture orig, TSource self)
    {
        Self = self;
        return Texture;
    }
    /// <inheritdoc cref="ModNPC.Texture"/>
    public virtual string Texture => (Self.GetType().Namespace + "." + Self.Name).Replace('.', '/');

    // HeadTexture
    public sealed override string Detour_get_HeadTexture(Orig_get_HeadTexture orig, TSource self)
    {
        Self = self;
        return HeadTexture;
    }
    /// <inheritdoc cref="ModNPC.HeadTexture"/>
    public virtual string HeadTexture => Texture + "_Head";

    // BossHeadTexture
    public sealed override string Detour_get_BossHeadTexture(Orig_get_BossHeadTexture orig, TSource self)
    {
        Self = self;
        return BossHeadTexture;
    }
    /// <inheritdoc cref="ModNPC.BossHeadTexture"/>
    public virtual string BossHeadTexture => Texture + "_Head_Boss";

    // PickEmote
    public sealed override int? Detour_PickEmote(Orig_PickEmote orig, TSource self, Player closestPlayer, List<int> emoteList, WorldUIAnchor otherAnchor)
    {
        Self = self;
        return PickEmote(closestPlayer, emoteList, otherAnchor);
    }
    /// <inheritdoc cref="ModNPC.PickEmote"/>
    public virtual int? PickEmote(Player closestPlayer, List<int> emoteList, WorldUIAnchor otherAnchor) => null;

    // SetDefaults
    public sealed override void Detour_SetDefaults(Orig_SetDefaults orig, TSource self)
    {
        Self = self;
        SetDefaults();
    }
    /// <inheritdoc cref="ModNPC.SetDefaults"/>
    public virtual void SetDefaults() { }

    // OnSpawn
    public sealed override void Detour_OnSpawn(Orig_OnSpawn orig, TSource self, IEntitySource source)
    {
        Self = self;
        OnSpawn(source);
    }
    /// <inheritdoc cref="ModNPC.OnSpawn"/>
    public virtual void OnSpawn(IEntitySource source) { }

    // AutoStaticDefaults
    public sealed override void Detour_AutoStaticDefaults(Orig_AutoStaticDefaults orig, TSource self)
    {
        Self = self;
        AutoStaticDefaults();
    }
    /// <inheritdoc cref="ModNPC.AutoStaticDefaults"/>
    public virtual void AutoStaticDefaults() { }

    // ApplyDifficultyAndPlayerScaling
    public sealed override void Detour_ApplyDifficultyAndPlayerScaling(Orig_ApplyDifficultyAndPlayerScaling orig, TSource self, int numPlayers, float balance, float bossAdjustment)
    {
        Self = self;
        ApplyDifficultyAndPlayerScaling(numPlayers, balance, bossAdjustment);
    }
    /// <inheritdoc cref="ModNPC.ApplyDifficultyAndPlayerScaling"/>
    public virtual void ApplyDifficultyAndPlayerScaling(int numPlayers, float balance, float bossAdjustment) { }

    // SetBestiary
    public sealed override void Detour_SetBestiary(Orig_SetBestiary orig, TSource self, BestiaryDatabase database, BestiaryEntry bestiaryEntry)
    {
        Self = self;
        SetBestiary(database, bestiaryEntry);
    }
    /// <inheritdoc cref="ModNPC.SetBestiary"/>
    public virtual void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) { }

    // ModifyTypeName
    public sealed override void Detour_ModifyTypeName(Orig_ModifyTypeName orig, TSource self, ref string typeName)
    {
        Self = self;
        ModifyTypeName(ref typeName);
    }
    /// <inheritdoc cref="ModNPC.ModifyTypeName"/>
    public virtual void ModifyTypeName(ref string typeName) { }

    // ModifyHoverBoundingBox
    public sealed override void Detour_ModifyHoverBoundingBox(Orig_ModifyHoverBoundingBox orig, TSource self, ref Rectangle boundingBox)
    {
        Self = self;
        ModifyHoverBoundingBox(ref boundingBox);
    }
    /// <inheritdoc cref="ModNPC.ModifyHoverBoundingBox"/>
    public virtual void ModifyHoverBoundingBox(ref Rectangle boundingBox) { }

    // PreHoverInteract
    public sealed override bool Detour_PreHoverInteract(Orig_PreHoverInteract orig, TSource self, bool mouseIntersects)
    {
        Self = self;
        return PreHoverInteract(mouseIntersects);
    }
    /// <inheritdoc cref="ModNPC.PreHoverInteract"/>
    public virtual bool PreHoverInteract(bool mouseIntersects) => true;

    // SetNPCNameList
    public sealed override List<string> Detour_SetNPCNameList(Orig_SetNPCNameList orig, TSource self)
    {
        Self = self;
        return SetNPCNameList();
    }
    /// <inheritdoc cref="ModNPC.SetNPCNameList"/>
    public virtual List<string> SetNPCNameList() => [];

    // TownNPCProfile
    public sealed override ITownNPCProfile Detour_TownNPCProfile(Orig_TownNPCProfile orig, TSource self)
    {
        Self = self;
        return TownNPCProfile();
    }
    /// <inheritdoc cref="ModNPC.TownNPCProfile"/>
    public virtual ITownNPCProfile TownNPCProfile() => null;

    // ResetEffects
    public sealed override void Detour_ResetEffects(Orig_ResetEffects orig, TSource self)
    {
        Self = self;
        ResetEffects();
    }
    /// <inheritdoc cref="ModNPC.ResetEffects"/>
    public virtual void ResetEffects() { }

    // PreAI
    public sealed override bool Detour_PreAI(Orig_PreAI orig, TSource self)
    {
        Self = self;
        return PreAI();
    }
    /// <inheritdoc cref="ModNPC.PreAI"/>
    public virtual bool PreAI() => true;

    // AI
    public sealed override void Detour_AI(Orig_AI orig, TSource self)
    {
        Self = self;
        AI();
    }
    /// <inheritdoc cref="ModNPC.AI"/>
    public virtual void AI() { }

    // PostAI
    public sealed override void Detour_PostAI(Orig_PostAI orig, TSource self)
    {
        Self = self;
        PostAI();
    }
    /// <inheritdoc cref="ModNPC.PostAI"/>
    public virtual void PostAI() { }

    // SendExtraAI
    public sealed override void Detour_SendExtraAI(Orig_SendExtraAI orig, TSource self, BinaryWriter writer)
    {
        Self = self;
        SendExtraAI(writer);
    }
    /// <inheritdoc cref="ModNPC.SendExtraAI"/>
    public virtual void SendExtraAI(BinaryWriter writer) { }

    // ReceiveExtraAI
    public sealed override void Detour_ReceiveExtraAI(Orig_ReceiveExtraAI orig, TSource self, BinaryReader reader)
    {
        Self = self;
        ReceiveExtraAI(reader);
    }
    /// <inheritdoc cref="ModNPC.ReceiveExtraAI"/>
    public virtual void ReceiveExtraAI(BinaryReader reader) { }

    // FindFrame
    public sealed override void Detour_FindFrame(Orig_FindFrame orig, TSource self, int frameHeight)
    {
        Self = self;
        FindFrame(frameHeight);
    }
    /// <inheritdoc cref="ModNPC.FindFrame"/>
    public virtual void FindFrame(int frameHeight) { }

    // HitEffect
    public sealed override void Detour_HitEffect(Orig_HitEffect orig, TSource self, NPC.HitInfo hit)
    {
        Self = self;
        HitEffect(hit);
    }
    /// <inheritdoc cref="ModNPC.HitEffect"/>
    public virtual void HitEffect(NPC.HitInfo hit) { }

    // UpdateLifeRegen
    public sealed override void Detour_UpdateLifeRegen(Orig_UpdateLifeRegen orig, TSource self, ref int damage)
    {
        Self = self;
        UpdateLifeRegen(ref damage);
    }
    /// <inheritdoc cref="ModNPC.UpdateLifeRegen"/>
    public virtual void UpdateLifeRegen(ref int damage) { }

    // CheckActive
    public sealed override bool Detour_CheckActive(Orig_CheckActive orig, TSource self)
    {
        Self = self;
        return CheckActive();
    }
    /// <inheritdoc cref="ModNPC.CheckActive"/>
    public virtual bool CheckActive() => true;

    // CheckDead
    public sealed override bool Detour_CheckDead(Orig_CheckDead orig, TSource self)
    {
        Self = self;
        return CheckDead();
    }
    /// <inheritdoc cref="ModNPC.CheckDead"/>
    public virtual bool CheckDead() => true;

    // SpecialOnKill
    public sealed override bool Detour_SpecialOnKill(Orig_SpecialOnKill orig, TSource self)
    {
        Self = self;
        return SpecialOnKill();
    }
    /// <inheritdoc cref="ModNPC.SpecialOnKill"/>
    public virtual bool SpecialOnKill() => false;

    // PreKill
    public sealed override bool Detour_PreKill(Orig_PreKill orig, TSource self)
    {
        Self = self;
        return PreKill();
    }
    /// <inheritdoc cref="ModNPC.PreKill"/>
    public virtual bool PreKill() => true;

    // OnKill
    public sealed override void Detour_OnKill(Orig_OnKill orig, TSource self)
    {
        Self = self;
        OnKill();
    }
    /// <inheritdoc cref="ModNPC.OnKill"/>
    public virtual void OnKill() { }

    // CanFallThroughPlatforms
    public sealed override bool? Detour_CanFallThroughPlatforms(Orig_CanFallThroughPlatforms orig, TSource self)
    {
        Self = self;
        return CanFallThroughPlatforms();
    }
    /// <inheritdoc cref="ModNPC.CanFallThroughPlatforms"/>
    public virtual bool? CanFallThroughPlatforms() => null;

    // CanBeCaughtBy
    public sealed override bool? Detour_CanBeCaughtBy(Orig_CanBeCaughtBy orig, TSource self, Item item, Player player)
    {
        Self = self;
        return CanBeCaughtBy(item, player);
    }
    /// <inheritdoc cref="ModNPC.CanBeCaughtBy"/>
    public virtual bool? CanBeCaughtBy(Item item, Player player) => null;

    // OnCaughtBy
    public sealed override void Detour_OnCaughtBy(Orig_OnCaughtBy orig, TSource self, Player player, Item item, bool failed)
    {
        Self = self;
        OnCaughtBy(player, item, failed);
    }
    /// <inheritdoc cref="ModNPC.OnCaughtBy"/>
    public virtual void OnCaughtBy(Player player, Item item, bool failed) { }

    // ModifyNPCLoot
    public sealed override void Detour_ModifyNPCLoot(Orig_ModifyNPCLoot orig, TSource self, NPCLoot npcLoot)
    {
        Self = self;
        ModifyNPCLoot(npcLoot);
    }
    /// <inheritdoc cref="ModNPC.ModifyNPCLoot"/>
    public virtual void ModifyNPCLoot(NPCLoot npcLoot) { }

    // BossLoot
    public sealed override void Detour_BossLoot(Orig_BossLoot orig, TSource self, ref string name, ref int potionType)
    {
        Self = self;
        BossLoot(ref name, ref potionType);
    }
    /// <inheritdoc cref="ModNPC.BossLoot"/>
    public virtual void BossLoot(ref string name, ref int potionType) { }

    // CanHitPlayer
    public sealed override bool Detour_CanHitPlayer(Orig_CanHitPlayer orig, TSource self, Player target, ref int cooldownSlot)
    {
        Self = self;
        return CanHitPlayer(target, ref cooldownSlot);
    }
    /// <inheritdoc cref="ModNPC.CanHitPlayer"/>
    public virtual bool CanHitPlayer(Player target, ref int cooldownSlot) => true;

    // ModifyHitPlayer
    public sealed override void Detour_ModifyHitPlayer(Orig_ModifyHitPlayer orig, TSource self, Player target, ref Player.HurtModifiers modifiers)
    {
        Self = self;
        ModifyHitPlayer(target, ref modifiers);
    }
    /// <inheritdoc cref="ModNPC.ModifyHitPlayer"/>
    public virtual void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers) { }

    // OnHitPlayer
    public sealed override void Detour_OnHitPlayer(Orig_OnHitPlayer orig, TSource self, Player target, Player.HurtInfo hurtInfo)
    {
        Self = self;
        OnHitPlayer(target, hurtInfo);
    }
    /// <inheritdoc cref="ModNPC.OnHitPlayer"/>
    public virtual void OnHitPlayer(Player target, Player.HurtInfo hurtInfo) { }

    // CanHitNPC
    public sealed override bool Detour_CanHitNPC(Orig_CanHitNPC orig, TSource self, NPC target)
    {
        Self = self;
        return CanHitNPC(target);
    }
    /// <inheritdoc cref="ModNPC.CanHitNPC"/>
    public virtual bool CanHitNPC(NPC target) => true;

    // CanBeHitByNPC
    public sealed override bool Detour_CanBeHitByNPC(Orig_CanBeHitByNPC orig, TSource self, NPC attacker)
    {
        Self = self;
        return CanBeHitByNPC(attacker);
    }
    /// <inheritdoc cref="ModNPC.CanBeHitByNPC"/>
    public virtual bool CanBeHitByNPC(NPC attacker) => true;

    // ModifyHitNPC
    public sealed override void Detour_ModifyHitNPC(Orig_ModifyHitNPC orig, TSource self, NPC target, ref NPC.HitModifiers modifiers)
    {
        Self = self;
        ModifyHitNPC(target, ref modifiers);
    }
    /// <inheritdoc cref="ModNPC.ModifyHitNPC"/>
    public virtual void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) { }

    // OnHitNPC
    public sealed override void Detour_OnHitNPC(Orig_OnHitNPC orig, TSource self, NPC target, NPC.HitInfo hit)
    {
        Self = self;
        OnHitNPC(target, hit);
    }
    /// <inheritdoc cref="ModNPC.OnHitNPC"/>
    public virtual void OnHitNPC(NPC target, NPC.HitInfo hit) { }

    // CanBeHitByItem
    public sealed override bool? Detour_CanBeHitByItem(Orig_CanBeHitByItem orig, TSource self, Player player, Item item)
    {
        Self = self;
        return CanBeHitByItem(player, item);
    }
    /// <inheritdoc cref="ModNPC.CanBeHitByItem"/>
    public virtual bool? CanBeHitByItem(Player player, Item item) => null;

    // CanCollideWithPlayerMeleeAttack
    public sealed override bool? Detour_CanCollideWithPlayerMeleeAttack(Orig_CanCollideWithPlayerMeleeAttack orig, TSource self, Player player, Item item, Rectangle meleeAttackHitbox)
    {
        Self = self;
        return CanCollideWithPlayerMeleeAttack(player, item, meleeAttackHitbox);
    }
    /// <inheritdoc cref="ModNPC.CanCollideWithPlayerMeleeAttack"/>
    public virtual bool? CanCollideWithPlayerMeleeAttack(Player player, Item item, Rectangle meleeAttackHitbox) => null;

    // ModifyHitByItem
    public sealed override void Detour_ModifyHitByItem(Orig_ModifyHitByItem orig, TSource self, Player player, Item item, ref NPC.HitModifiers modifiers)
    {
        Self = self;
        ModifyHitByItem(player, item, ref modifiers);
    }
    /// <inheritdoc cref="ModNPC.ModifyHitByItem"/>
    public virtual void ModifyHitByItem(Player player, Item item, ref NPC.HitModifiers modifiers) { }

    // OnHitByItem
    public sealed override void Detour_OnHitByItem(Orig_OnHitByItem orig, TSource self, Player player, Item item, NPC.HitInfo hit, int damageDone)
    {
        Self = self;
        OnHitByItem(player, item, hit, damageDone);
    }
    /// <inheritdoc cref="ModNPC.OnHitByItem"/>
    public virtual void OnHitByItem(Player player, Item item, NPC.HitInfo hit, int damageDone) { }

    // CanBeHitByProjectile
    public sealed override bool? Detour_CanBeHitByProjectile(Orig_CanBeHitByProjectile orig, TSource self, Projectile projectile)
    {
        Self = self;
        return CanBeHitByProjectile(projectile);
    }
    /// <inheritdoc cref="ModNPC.CanBeHitByProjectile"/>
    public virtual bool? CanBeHitByProjectile(Projectile projectile) => null;

    // ModifyHitByProjectile
    public sealed override void Detour_ModifyHitByProjectile(Orig_ModifyHitByProjectile orig, TSource self, Projectile projectile, ref NPC.HitModifiers modifiers)
    {
        Self = self;
        ModifyHitByProjectile(projectile, ref modifiers);
    }
    /// <inheritdoc cref="ModNPC.ModifyHitByProjectile"/>
    public virtual void ModifyHitByProjectile(Projectile projectile, ref NPC.HitModifiers modifiers) { }

    // OnHitByProjectile
    public sealed override void Detour_OnHitByProjectile(Orig_OnHitByProjectile orig, TSource self, Projectile projectile, NPC.HitInfo hit, int damageDone)
    {
        Self = self;
        OnHitByProjectile(projectile, hit, damageDone);
    }
    /// <inheritdoc cref="ModNPC.OnHitByProjectile"/>
    public virtual void OnHitByProjectile(Projectile projectile, NPC.HitInfo hit, int damageDone) { }

    // ModifyIncomingHit
    public sealed override void Detour_ModifyIncomingHit(Orig_ModifyIncomingHit orig, TSource self, ref NPC.HitModifiers modifiers)
    {
        Self = self;
        ModifyIncomingHit(ref modifiers);
    }
    /// <inheritdoc cref="ModNPC.ModifyIncomingHit"/>
    public virtual void ModifyIncomingHit(ref NPC.HitModifiers modifiers) { }

    // BossHeadSlot
    public sealed override void Detour_BossHeadSlot(Orig_BossHeadSlot orig, TSource self, ref int index)
    {
        Self = self;
        BossHeadSlot(ref index);
    }
    /// <inheritdoc cref="ModNPC.BossHeadSlot"/>
    public virtual void BossHeadSlot(ref int index) { }

    // BossHeadRotation
    public sealed override void Detour_BossHeadRotation(Orig_BossHeadRotation orig, TSource self, ref float rotation)
    {
        Self = self;
        BossHeadRotation(ref rotation);
    }
    /// <inheritdoc cref="ModNPC.BossHeadRotation"/>
    public virtual void BossHeadRotation(ref float rotation) { }

    // BossHeadSpriteEffects
    public sealed override void Detour_BossHeadSpriteEffects(Orig_BossHeadSpriteEffects orig, TSource self, ref SpriteEffects spriteEffects)
    {
        Self = self;
        BossHeadSpriteEffects(ref spriteEffects);
    }
    /// <inheritdoc cref="ModNPC.BossHeadSpriteEffects"/>
    public virtual void BossHeadSpriteEffects(ref SpriteEffects spriteEffects) { }

    // GetAlpha
    public sealed override Color? Detour_GetAlpha(Orig_GetAlpha orig, TSource self, Color drawColor)
    {
        Self = self;
        return GetAlpha(drawColor);
    }
    /// <inheritdoc cref="ModNPC.GetAlpha"/>
    public virtual Color? GetAlpha(Color drawColor) => null;

    // DrawEffects
    public sealed override void Detour_DrawEffects(Orig_DrawEffects orig, TSource self, ref Color drawColor)
    {
        Self = self;
        DrawEffects(ref drawColor);
    }
    /// <inheritdoc cref="ModNPC.DrawEffects"/>
    public virtual void DrawEffects(ref Color drawColor) { }

    // PreDraw
    public sealed override bool Detour_PreDraw(Orig_PreDraw orig, TSource self, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        Self = self;
        return PreDraw(spriteBatch, screenPos, drawColor);
    }
    /// <inheritdoc cref="ModNPC.PreDraw"/>
    public virtual bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) => true;

    // PostDraw
    public sealed override void Detour_PostDraw(Orig_PostDraw orig, TSource self, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        Self = self;
        PostDraw(spriteBatch, screenPos, drawColor);
    }
    /// <inheritdoc cref="ModNPC.PostDraw"/>
    public virtual void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) { }

    // DrawBehind
    public sealed override void Detour_DrawBehind(Orig_DrawBehind orig, TSource self, int index)
    {
        Self = self;
        DrawBehind(index);
    }
    /// <inheritdoc cref="ModNPC.DrawBehind"/>
    public virtual void DrawBehind(int index) { }

    // DrawHealthBar
    public sealed override bool? Detour_DrawHealthBar(Orig_DrawHealthBar orig, TSource self, byte hbPosition, ref float scale, ref Vector2 position)
    {
        Self = self;
        return DrawHealthBar(hbPosition, ref scale, ref position);
    }
    /// <inheritdoc cref="ModNPC.DrawHealthBar"/>
    public virtual bool? DrawHealthBar(byte hbPosition, ref float scale, ref Vector2 position) => null;

    // SpawnChance
    public sealed override float Detour_SpawnChance(Orig_SpawnChance orig, TSource self, NPCSpawnInfo spawnInfo)
    {
        Self = self;
        return SpawnChance(spawnInfo);
    }
    /// <inheritdoc cref="ModNPC.SpawnChance"/>
    public virtual float SpawnChance(NPCSpawnInfo spawnInfo) => 0f;

    // SpawnNPC
    public sealed override int Detour_SpawnNPC(Orig_SpawnNPC orig, TSource self, int tileX, int tileY)
    {
        Self = self;
        return SpawnNPC(tileX, tileY);
    }
    /// <inheritdoc cref="ModNPC.SpawnNPC"/>
    public virtual int SpawnNPC(int tileX, int tileY) => NPC.NewNPC(null, tileX * 16 + 8, tileY * 16, NPC.type);

    // CanTownNPCSpawn
    public sealed override bool Detour_CanTownNPCSpawn(Orig_CanTownNPCSpawn orig, TSource self, int numTownNPCs)
    {
        Self = self;
        return CanTownNPCSpawn(numTownNPCs);
    }
    /// <inheritdoc cref="ModNPC.CanTownNPCSpawn"/>
    public virtual bool CanTownNPCSpawn(int numTownNPCs) => false;

    // CheckConditions
    public sealed override bool Detour_CheckConditions(Orig_CheckConditions orig, TSource self, int left, int right, int top, int bottom)
    {
        Self = self;
        return CheckConditions(left, right, top, bottom);
    }
    /// <inheritdoc cref="ModNPC.CheckConditions"/>
    public virtual bool CheckConditions(int left, int right, int top, int bottom) => true;

    // UsesPartyHat
    public sealed override bool Detour_UsesPartyHat(Orig_UsesPartyHat orig, TSource self)
    {
        Self = self;
        return UsesPartyHat();
    }
    /// <inheritdoc cref="ModNPC.UsesPartyHat"/>
    public virtual bool UsesPartyHat() => true;

    // CanChat
    public sealed override bool Detour_CanChat(Orig_CanChat orig, TSource self)
    {
        Self = self;
        return CanChat();
    }
    /// <inheritdoc cref="ModNPC.CanChat"/>
    public virtual bool CanChat() => NPC.townNPC;

    // GetChat
    public sealed override string Detour_GetChat(Orig_GetChat orig, TSource self)
    {
        Self = self;
        return GetChat();
    }
    /// <inheritdoc cref="ModNPC.GetChat"/>
    public virtual string GetChat() => Language.GetTextValue("tModLoader.DefaultTownNPCChat");

    // SetChatButtons
    public sealed override void Detour_SetChatButtons(Orig_SetChatButtons orig, TSource self, ref string button, ref string button2)
    {
        Self = self;
        SetChatButtons(ref button, ref button2);
    }
    /// <inheritdoc cref="ModNPC.SetChatButtons"/>
    public virtual void SetChatButtons(ref string button, ref string button2) { }

    // OnChatButtonClicked
    public sealed override void Detour_OnChatButtonClicked(Orig_OnChatButtonClicked orig, TSource self, bool firstButton, ref string shopName)
    {
        Self = self;
        OnChatButtonClicked(firstButton, ref shopName);
    }
    /// <inheritdoc cref="ModNPC.OnChatButtonClicked"/>
    public virtual void OnChatButtonClicked(bool firstButton, ref string shopName) { }

    // AddShops
    public sealed override void Detour_AddShops(Orig_AddShops orig, TSource self)
    {
        Self = self;
        AddShops();
    }
    /// <inheritdoc cref="ModNPC.AddShops"/>
    public virtual void AddShops() { }

    // ModifyActiveShop
    public sealed override void Detour_ModifyActiveShop(Orig_ModifyActiveShop orig, TSource self, string shopName, Item[] items)
    {
        Self = self;
        ModifyActiveShop(shopName, items);
    }
    /// <inheritdoc cref="ModNPC.ModifyActiveShop"/>
    public virtual void ModifyActiveShop(string shopName, Item[] items) { }

    // CanGoToStatue
    public sealed override bool Detour_CanGoToStatue(Orig_CanGoToStatue orig, TSource self, bool toKingStatue)
    {
        Self = self;
        return CanGoToStatue(toKingStatue);
    }
    /// <inheritdoc cref="ModNPC.CanGoToStatue"/>
    public virtual bool CanGoToStatue(bool toKingStatue) => false;

    // OnGoToStatue
    public sealed override void Detour_OnGoToStatue(Orig_OnGoToStatue orig, TSource self, bool toKingStatue)
    {
        Self = self;
        OnGoToStatue(toKingStatue);
    }
    /// <inheritdoc cref="ModNPC.OnGoToStatue"/>
    public virtual void OnGoToStatue(bool toKingStatue) { }

    // ModifyDeathMessage
    public sealed override bool Detour_ModifyDeathMessage(Orig_ModifyDeathMessage orig, TSource self, ref NetworkText customText, ref Color color)
    {
        Self = self;
        return ModifyDeathMessage(ref customText, ref color);
    }
    /// <inheritdoc cref="ModNPC.ModifyDeathMessage"/>
    public virtual bool ModifyDeathMessage(ref NetworkText customText, ref Color color) => true;

    // TownNPCAttackStrength
    public sealed override void Detour_TownNPCAttackStrength(Orig_TownNPCAttackStrength orig, TSource self, ref int damage, ref float knockback)
    {
        Self = self;
        TownNPCAttackStrength(ref damage, ref knockback);
    }
    /// <inheritdoc cref="ModNPC.TownNPCAttackStrength"/>
    public virtual void TownNPCAttackStrength(ref int damage, ref float knockback) { }

    // TownNPCAttackCooldown
    public sealed override void Detour_TownNPCAttackCooldown(Orig_TownNPCAttackCooldown orig, TSource self, ref int cooldown, ref int randExtraCooldown)
    {
        Self = self;
        TownNPCAttackCooldown(ref cooldown, ref randExtraCooldown);
    }
    /// <inheritdoc cref="ModNPC.TownNPCAttackCooldown"/>
    public virtual void TownNPCAttackCooldown(ref int cooldown, ref int randExtraCooldown) { }

    // TownNPCAttackProj
    public sealed override void Detour_TownNPCAttackProj(Orig_TownNPCAttackProj orig, TSource self, ref int projType, ref int attackDelay)
    {
        Self = self;
        TownNPCAttackProj(ref projType, ref attackDelay);
    }
    /// <inheritdoc cref="ModNPC.TownNPCAttackProj"/>
    public virtual void TownNPCAttackProj(ref int projType, ref int attackDelay) { }

    // TownNPCAttackProjSpeed
    public sealed override void Detour_TownNPCAttackProjSpeed(Orig_TownNPCAttackProjSpeed orig, TSource self, ref float multiplier, ref float gravityCorrection, ref float randomOffset)
    {
        Self = self;
        TownNPCAttackProjSpeed(ref multiplier, ref gravityCorrection, ref randomOffset);
    }
    /// <inheritdoc cref="ModNPC.TownNPCAttackProjSpeed"/>
    public virtual void TownNPCAttackProjSpeed(ref float multiplier, ref float gravityCorrection, ref float randomOffset) { }

    // TownNPCAttackShoot
    public sealed override void Detour_TownNPCAttackShoot(Orig_TownNPCAttackShoot orig, TSource self, ref bool inBetweenShots)
    {
        Self = self;
        TownNPCAttackShoot(ref inBetweenShots);
    }
    /// <inheritdoc cref="ModNPC.TownNPCAttackShoot"/>
    public virtual void TownNPCAttackShoot(ref bool inBetweenShots) { }

    // TownNPCAttackMagic
    public sealed override void Detour_TownNPCAttackMagic(Orig_TownNPCAttackMagic orig, TSource self, ref float auraLightMultiplier)
    {
        Self = self;
        TownNPCAttackMagic(ref auraLightMultiplier);
    }
    /// <inheritdoc cref="ModNPC.TownNPCAttackMagic"/>
    public virtual void TownNPCAttackMagic(ref float auraLightMultiplier) { }

    // TownNPCAttackSwing
    public sealed override void Detour_TownNPCAttackSwing(Orig_TownNPCAttackSwing orig, TSource self, ref int itemWidth, ref int itemHeight)
    {
        Self = self;
        TownNPCAttackSwing(ref itemWidth, ref itemHeight);
    }
    /// <inheritdoc cref="ModNPC.TownNPCAttackSwing"/>
    public virtual void TownNPCAttackSwing(ref int itemWidth, ref int itemHeight) { }

    // DrawTownAttackGun
    public sealed override void Detour_DrawTownAttackGun(Orig_DrawTownAttackGun orig, TSource self, ref Texture2D item, ref Rectangle itemFrame, ref float scale, ref int horizontalHoldoutOffset)
    {
        Self = self;
        DrawTownAttackGun(ref item, ref itemFrame, ref scale, ref horizontalHoldoutOffset);
    }
    /// <inheritdoc cref="ModNPC.DrawTownAttackGun"/>
    public virtual void DrawTownAttackGun(ref Texture2D item, ref Rectangle itemFrame, ref float scale, ref int horizontalHoldoutOffset) { }

    // DrawTownAttackSwing
    public sealed override void Detour_DrawTownAttackSwing(Orig_DrawTownAttackSwing orig, TSource self, ref Texture2D item, ref Rectangle itemFrame, ref int itemSize, ref float scale, ref Vector2 offset)
    {
        Self = self;
        DrawTownAttackSwing(ref item, ref itemFrame, ref itemSize, ref scale, ref offset);
    }
    /// <inheritdoc cref="ModNPC.DrawTownAttackSwing"/>
    public virtual void DrawTownAttackSwing(ref Texture2D item, ref Rectangle itemFrame, ref int itemSize, ref float scale, ref Vector2 offset) { }

    // ModifyCollisionData
    public sealed override bool Detour_ModifyCollisionData(Orig_ModifyCollisionData orig, TSource self, Rectangle victimHitbox, ref int immunityCooldownSlot, ref MultipliableFloat damageMultiplier, ref Rectangle npcHitbox)
    {
        Self = self;
        return ModifyCollisionData(victimHitbox, ref immunityCooldownSlot, ref damageMultiplier, ref npcHitbox);
    }
    /// <inheritdoc cref="ModNPC.ModifyCollisionData"/>
    public virtual bool ModifyCollisionData(Rectangle victimHitbox, ref int immunityCooldownSlot, ref MultipliableFloat damageMultiplier, ref Rectangle npcHitbox) => true;

    // NeedSaving
    public sealed override bool Detour_NeedSaving(Orig_NeedSaving orig, TSource self)
    {
        Self = self;
        return NeedSaving();
    }
    /// <inheritdoc cref="ModNPC.NeedSaving"/>
    public virtual bool NeedSaving() => false;

    // SaveData
    public sealed override void Detour_SaveData(Orig_SaveData orig, TSource self, TagCompound tag)
    {
        Self = self;
        SaveData(tag);
    }
    /// <inheritdoc cref="ModNPC.SaveData"/>
    public virtual void SaveData(TagCompound tag) { }

    // LoadData
    public sealed override void Detour_LoadData(Orig_LoadData orig, TSource self, TagCompound tag)
    {
        Self = self;
        LoadData(tag);
    }
    /// <inheritdoc cref="ModNPC.LoadData"/>
    public virtual void LoadData(TagCompound tag) { }

    // ChatBubblePosition
    public sealed override void Detour_ChatBubblePosition(Orig_ChatBubblePosition orig, TSource self, ref Vector2 position, ref SpriteEffects spriteEffects)
    {
        Self = self;
        ChatBubblePosition(ref position, ref spriteEffects);
    }
    /// <inheritdoc cref="ModNPC.ChatBubblePosition"/>
    public virtual void ChatBubblePosition(ref Vector2 position, ref SpriteEffects spriteEffects) { }

    // PartyHatPosition
    public sealed override void Detour_PartyHatPosition(Orig_PartyHatPosition orig, TSource self, ref Vector2 position, ref SpriteEffects spriteEffects)
    {
        Self = self;
        PartyHatPosition(ref position, ref spriteEffects);
    }
    /// <inheritdoc cref="ModNPC.PartyHatPosition"/>
    public virtual void PartyHatPosition(ref Vector2 position, ref SpriteEffects spriteEffects) { }

    // EmoteBubblePosition
    public sealed override void Detour_EmoteBubblePosition(Orig_EmoteBubblePosition orig, TSource self, ref Vector2 position, ref SpriteEffects spriteEffects)
    {
        Self = self;
        EmoteBubblePosition(ref position, ref spriteEffects);
    }
    /// <inheritdoc cref="ModNPC.EmoteBubblePosition"/>
    public virtual void EmoteBubblePosition(ref Vector2 position, ref SpriteEffects spriteEffects) { }
}

/// <summary>
/// 用于弹幕覆盖的抽象类。
/// <para/>设计规范：
/// <br/>1. <strong>避免</strong>在该类中试图使用 <typeparamref name="TSource"/> 类中的任何方法。
/// <br/>2. <strong>尝试</strong>使用属性（必要时使用Publicizer）来访问 <typeparamref name="TSource"/> 中的数据。
/// <br/>3. <strong>避免</strong>在该类中使用实例字段。
/// </summary>
/// <typeparam name="TSource">待覆盖的 <see cref="ModProjectile"/> 类。</typeparam>
public abstract class CAProjectileOverride<TSource> : ModProjectileDetour<TSource>, ICALocalizationPrefix, ICATweak where TSource : ModProjectile
{
    private static readonly HashSet<string> _exclusiveDetours = ["get_LocalizationCategory", "get_Texture", "get_GlowTexture"];
    private static readonly HashSet<string> _criticalDetours = [];

    public static int ContentType => ModContent.ProjectileType<TSource>();

    public abstract CAGamePhase Phase { get; }
    public virtual string LocalizationName => Name ?? SourceType.Name;

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
        CASharedData.TweakedProjectiles[ContentType] = true;
        foreach (int npcType in RelatedNPCs)
            CASharedData.TweakedNPCs[npcType] = true;
        foreach (int itemType in RelatedItems)
            CASharedData.TweakedItems[itemType] = true;
    }

    protected override Hook ApplySingleDetour<TDelegate>(TDelegate detour, bool hasThis = true)
    {
        if (TODetourHandler.EvaluateDetourName(detour.Method, out string sourceName) && SourceType.HasMethod(sourceName, hasThis ? TOReflectionUtils.InstanceBindingFlags : TOReflectionUtils.StaticBindingFlags, out MethodInfo sourceMethod))
        {
            if (_criticalDetours.Contains(sourceName))
                return TODetourHandler.Modify(sourceMethod, detour);
            switch (GetType().HasRealMethod(sourceName, TOReflectionUtils.UniversalBindingFlags), sourceMethod.DeclaringType == SourceType)
            {
                case (true, true):
                    return TODetourHandler.Modify(sourceMethod, detour);
                case (false, true):
                    if (_exclusiveDetours.Contains(sourceName))
                        return null;
                    //CAMain.Instance.Logger.Warn($"""[CA Override] Source method "{sourceName}" does not have an implemented hook, and will not be hooked.""");
                    return null;
                case (_, false):
                    return null;
            }
        }
        return null;
    }

    public TSource Self;
    public Projectile Projectile => Self.Projectile;

    /// <inheritdoc cref="ModProjectile.Type"/>
    public int Type => Self.Type;

    /// <inheritdoc cref="ModProjectile.AIType"/>
    public int AIType
    {
        get => Self.AIType;
        set => Self.AIType = value;
    }

    /// <inheritdoc cref="ModProjectile.CooldownSlot"/>
    public int CooldownSlot
    {
        get => Self.CooldownSlot;
        set => Self.CooldownSlot = value;
    }

    /// <inheritdoc cref="ModProjectile.DrawOffsetX"/>
    public int DrawOffsetX
    {
        get => Self.DrawOffsetX;
        set => Self.DrawOffsetX = value;
    }

    /// <inheritdoc cref="ModProjectile.DrawOriginOffsetY"/>
    public int DrawOriginOffsetY
    {
        get => Self.DrawOriginOffsetY;
        set => Self.DrawOriginOffsetY = value;
    }

    /// <inheritdoc cref="ModProjectile.DrawOriginOffsetX"/>
    public float DrawOriginOffsetX
    {
        get => Self.DrawOriginOffsetX;
        set => Self.DrawOriginOffsetX = value;
    }

    /// <inheritdoc cref="ModProjectile.DrawHeldProjInFrontOfHeldItemAndArms"/>
    public bool DrawHeldProjInFrontOfHeldItemAndArms
    {
        get => Self.DrawHeldProjInFrontOfHeldItemAndArms;
        set => Self.DrawHeldProjInFrontOfHeldItemAndArms = value;
    }

    // ModType
    // Not Supported
    public sealed override void Detour_Register(Orig_Register orig, TSource self)
    {
        Self = self;
        base.Detour_Register(orig, self);
    }
    public sealed override void Detour_InitTemplateInstance(Orig_InitTemplateInstance orig, TSource self)
    {
        Self = self;
        base.Detour_InitTemplateInstance(orig, self);
    }
    public sealed override void Detour_ValidateType(Orig_ValidateType orig, TSource self)
    {
        Self = self;
        base.Detour_ValidateType(orig, self);
    }

    // Name
    public sealed override string Detour_get_Name(Orig_get_Name orig, TSource self)
    {
        Self = self;
        return Name;
    }
    /// <inheritdoc cref="ModType.Name"/>
    public virtual string Name => Self.Name;

    // Load
    public sealed override void Detour_Load(Orig_Load orig, TSource self)
    {
        Self = self;
        Load();
    }
    /// <inheritdoc cref="ModType.Load"/>
    public virtual void Load() { }

    // IsLoadingEnabled
    public sealed override bool Detour_IsLoadingEnabled(Orig_IsLoadingEnabled orig, TSource self, Mod mod)
    {
        Self = self;
        return IsLoadingEnabled(mod);
    }
    /// <inheritdoc cref="ModType.IsLoadingEnabled(Mod)"/>
    public virtual bool IsLoadingEnabled(Mod mod) => true;

    // SetupContent
    public sealed override void Detour_SetupContent(Orig_SetupContent orig, TSource self)
    {
        Self = self;
        SetupContent();
    }
    /// <inheritdoc cref="ModType.SetupContent"/>
    public virtual void SetupContent() { }

    // SetStaticDefaults
    public sealed override void Detour_SetStaticDefaults(Orig_SetStaticDefaults orig, TSource self)
    {
        Self = self;
        SetStaticDefaults();
    }
    /// <inheritdoc cref="ModType.SetupContent"/>
    public virtual void SetStaticDefaults() { }

    // Unload
    public sealed override void Detour_Unload(Orig_Unload orig, TSource self)
    {
        Self = self;
        Unload();
    }
    /// <inheritdoc cref="ModType.Unload"/>
    public virtual void Unload() { }

    // ModProjectile
    // LocalizationCategory
    public sealed override string Detour_get_LocalizationCategory(Orig_get_LocalizationCategory orig, TSource self)
    {
        Self = self;
        return LocalizationCategory;
    }
    /// <inheritdoc cref="ModProjectile.LocalizationCategory"/>
    public virtual string LocalizationCategory => "Projectiles";

    // DisplayName
    public sealed override LocalizedText Detour_get_DisplayName(Orig_get_DisplayName orig, TSource self)
    {
        Self = self;
        return DisplayName;
    }
    /// <inheritdoc cref="ModProjectile.DisplayName"/>
    public virtual LocalizedText DisplayName => Self.GetLocalization("DisplayName", Self.PrettyPrintName);

    // Texture
    public sealed override string Detour_get_Texture(Orig_get_Texture orig, TSource self)
    {
        Self = self;
        return Texture;
    }
    /// <inheritdoc cref="ModProjectile.Texture"/>
    public virtual string Texture => (Self.GetType().Namespace + "." + Self.Name).Replace('.', '/');

    // GlowTexture
    public sealed override string Detour_get_GlowTexture(Orig_get_GlowTexture orig, TSource self)
    {
        Self = self;
        return GlowTexture;
    }
    /// <inheritdoc cref="ModProjectile.GlowTexture"/>
    public virtual string GlowTexture => Texture + "_Glow";

    // SetDefaults
    public sealed override void Detour_SetDefaults(Orig_SetDefaults orig, TSource self)
    {
        Self = self;
        SetDefaults();
    }
    /// <inheritdoc cref="ModProjectile.SetDefaults"/>
    public virtual void SetDefaults() { }

    // OnSpawn
    public sealed override void Detour_OnSpawn(Orig_OnSpawn orig, TSource self, IEntitySource source)
    {
        Self = self;
        OnSpawn(source);
    }
    /// <inheritdoc cref="ModProjectile.OnSpawn"/>
    public virtual void OnSpawn(IEntitySource source) { }

    // AutoStaticDefaults
    public sealed override void Detour_AutoStaticDefaults(Orig_AutoStaticDefaults orig, TSource self)
    {
        Self = self;
        AutoStaticDefaults();
    }
    /// <inheritdoc cref="ModProjectile.AutoStaticDefaults"/>
    public virtual void AutoStaticDefaults() { }

    // PreAI
    public sealed override bool Detour_PreAI(Orig_PreAI orig, TSource self)
    {
        Self = self;
        return PreAI();
    }
    /// <inheritdoc cref="ModProjectile.PreAI"/>
    public virtual bool PreAI() => true;

    // AI
    public sealed override void Detour_AI(Orig_AI orig, TSource self)
    {
        Self = self;
        AI();
    }
    /// <inheritdoc cref="ModProjectile.AI"/>
    public virtual void AI() { }

    // PostAI
    public sealed override void Detour_PostAI(Orig_PostAI orig, TSource self)
    {
        Self = self;
        PostAI();
    }
    /// <inheritdoc cref="ModProjectile.PostAI"/>
    public virtual void PostAI() { }

    // SendExtraAI
    public sealed override void Detour_SendExtraAI(Orig_SendExtraAI orig, TSource self, BinaryWriter writer)
    {
        Self = self;
        SendExtraAI(writer);
    }
    /// <inheritdoc cref="ModProjectile.SendExtraAI"/>
    public virtual void SendExtraAI(BinaryWriter writer) { }

    // ReceiveExtraAI
    public sealed override void Detour_ReceiveExtraAI(Orig_ReceiveExtraAI orig, TSource self, BinaryReader reader)
    {
        Self = self;
        ReceiveExtraAI(reader);
    }
    /// <inheritdoc cref="ModProjectile.ReceiveExtraAI"/>
    public virtual void ReceiveExtraAI(BinaryReader reader) { }

    // ShouldUpdatePosition
    public sealed override bool Detour_ShouldUpdatePosition(Orig_ShouldUpdatePosition orig, TSource self)
    {
        Self = self;
        return ShouldUpdatePosition();
    }
    /// <inheritdoc cref="ModProjectile.ShouldUpdatePosition"/>
    public virtual bool ShouldUpdatePosition() => true;

    // TileCollideStyle
    public sealed override bool Detour_TileCollideStyle(Orig_TileCollideStyle orig, TSource self, ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac)
    {
        Self = self;
        return TileCollideStyle(ref width, ref height, ref fallThrough, ref hitboxCenterFrac);
    }
    /// <inheritdoc cref="ModProjectile.TileCollideStyle"/>
    public virtual bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac) => true;

    // OnTileCollide
    public sealed override bool Detour_OnTileCollide(Orig_OnTileCollide orig, TSource self, Vector2 oldVelocity)
    {
        Self = self;
        return OnTileCollide(oldVelocity);
    }
    /// <inheritdoc cref="ModProjectile.OnTileCollide"/>
    public virtual bool OnTileCollide(Vector2 oldVelocity) => true;

    // CanCutTiles
    public sealed override bool? Detour_CanCutTiles(Orig_CanCutTiles orig, TSource self)
    {
        Self = self;
        return CanCutTiles();
    }
    /// <inheritdoc cref="ModProjectile.CanCutTiles"/>
    public virtual bool? CanCutTiles() => null;

    // CutTiles
    public sealed override void Detour_CutTiles(Orig_CutTiles orig, TSource self)
    {
        Self = self;
        CutTiles();
    }
    /// <inheritdoc cref="ModProjectile.CutTiles"/>
    public virtual void CutTiles() { }

    // PreKill
    public sealed override bool Detour_PreKill(Orig_PreKill orig, TSource self, int timeLeft)
    {
        Self = self;
        return PreKill(timeLeft);
    }
    /// <inheritdoc cref="ModProjectile.PreKill"/>
    public virtual bool PreKill(int timeLeft) => true;

    // OnKill
    public sealed override void Detour_OnKill(Orig_OnKill orig, TSource self, int timeLeft)
    {
        Self = self;
        OnKill(timeLeft);
    }
    /// <inheritdoc cref="ModProjectile.OnKill"/>
    public virtual void OnKill(int timeLeft) { }

    // CanDamage
    public sealed override bool? Detour_CanDamage(Orig_CanDamage orig, TSource self)
    {
        Self = self;
        return CanDamage();
    }
    /// <inheritdoc cref="ModProjectile.CanDamage"/>
    public virtual bool? CanDamage() => null;

    // MinionContactDamage
    public sealed override bool Detour_MinionContactDamage(Orig_MinionContactDamage orig, TSource self)
    {
        Self = self;
        return MinionContactDamage();
    }
    /// <inheritdoc cref="ModProjectile.MinionContactDamage"/>
    public virtual bool MinionContactDamage() => false;

    // ModifyDamageHitbox
    public sealed override void Detour_ModifyDamageHitbox(Orig_ModifyDamageHitbox orig, TSource self, ref Rectangle hitbox)
    {
        Self = self;
        ModifyDamageHitbox(ref hitbox);
    }
    /// <inheritdoc cref="ModProjectile.ModifyDamageHitbox"/>
    public virtual void ModifyDamageHitbox(ref Rectangle hitbox) { }

    // CanHitNPC
    public sealed override bool? Detour_CanHitNPC(Orig_CanHitNPC orig, TSource self, NPC target)
    {
        Self = self;
        return CanHitNPC(target);
    }
    /// <inheritdoc cref="ModProjectile.CanHitNPC"/>
    public virtual bool? CanHitNPC(NPC target) => null;

    // ModifyHitNPC
    public sealed override void Detour_ModifyHitNPC(Orig_ModifyHitNPC orig, TSource self, NPC target, ref NPC.HitModifiers modifiers)
    {
        Self = self;
        ModifyHitNPC(target, ref modifiers);
    }
    /// <inheritdoc cref="ModProjectile.ModifyHitNPC"/>
    public virtual void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) { }

    // OnHitNPC
    public sealed override void Detour_OnHitNPC(Orig_OnHitNPC orig, TSource self, NPC target, NPC.HitInfo hit, int damageDone)
    {
        Self = self;
        OnHitNPC(target, hit, damageDone);
    }
    /// <inheritdoc cref="ModProjectile.OnHitNPC"/>
    public virtual void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) { }

    // CanHitPvp
    public sealed override bool Detour_CanHitPvp(Orig_CanHitPvp orig, TSource self, Player target)
    {
        Self = self;
        return CanHitPvp(target);
    }
    /// <inheritdoc cref="ModProjectile.CanHitPvp"/>
    public virtual bool CanHitPvp(Player target) => true;

    // CanHitPlayer
    public sealed override bool Detour_CanHitPlayer(Orig_CanHitPlayer orig, TSource self, Player target)
    {
        Self = self;
        return CanHitPlayer(target);
    }
    /// <inheritdoc cref="ModProjectile.CanHitPlayer"/>
    public virtual bool CanHitPlayer(Player target) => true;

    // ModifyHitPlayer
    public sealed override void Detour_ModifyHitPlayer(Orig_ModifyHitPlayer orig, TSource self, Player target, ref Player.HurtModifiers modifiers)
    {
        Self = self;
        ModifyHitPlayer(target, ref modifiers);
    }
    /// <inheritdoc cref="ModProjectile.ModifyHitPlayer"/>
    public virtual void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers) { }

    // OnHitPlayer
    public sealed override void Detour_OnHitPlayer(Orig_OnHitPlayer orig, TSource self, Player target, Player.HurtInfo info)
    {
        Self = self;
        OnHitPlayer(target, info);
    }
    /// <inheritdoc cref="ModProjectile.OnHitPlayer"/>
    public virtual void OnHitPlayer(Player target, Player.HurtInfo info) { }

    // Colliding
    public sealed override bool? Detour_Colliding(Orig_Colliding orig, TSource self, Rectangle projHitbox, Rectangle targetHitbox)
    {
        Self = self;
        return Colliding(projHitbox, targetHitbox);
    }
    /// <inheritdoc cref="ModProjectile.Colliding"/>
    public virtual bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) => null;

    // GetAlpha
    public sealed override Color? Detour_GetAlpha(Orig_GetAlpha orig, TSource self, Color lightColor)
    {
        Self = self;
        return GetAlpha(lightColor);
    }
    /// <inheritdoc cref="ModProjectile.GetAlpha"/>
    public virtual Color? GetAlpha(Color lightColor) => null;

    // PreDrawExtras
    public sealed override bool Detour_PreDrawExtras(Orig_PreDrawExtras orig, TSource self)
    {
        Self = self;
        return PreDrawExtras();
    }
    /// <inheritdoc cref="ModProjectile.PreDrawExtras"/>
    public virtual bool PreDrawExtras() => true;

    // PreDraw
    public sealed override bool Detour_PreDraw(Orig_PreDraw orig, TSource self, ref Color lightColor)
    {
        Self = self;
        return PreDraw(ref lightColor);
    }
    /// <inheritdoc cref="ModProjectile.PreDraw"/>
    public virtual bool PreDraw(ref Color lightColor) => true;

    // PostDraw
    public sealed override void Detour_PostDraw(Orig_PostDraw orig, TSource self, Color lightColor)
    {
        Self = self;
        PostDraw(lightColor);
    }
    /// <inheritdoc cref="ModProjectile.PostDraw"/>
    public virtual void PostDraw(Color lightColor) { }

    // CanUseGrapple
    public sealed override bool? Detour_CanUseGrapple(Orig_CanUseGrapple orig, TSource self, Player player)
    {
        Self = self;
        return CanUseGrapple(player);
    }
    /// <inheritdoc cref="ModProjectile.CanUseGrapple"/>
    public virtual bool? CanUseGrapple(Player player) => null;

    // UseGrapple
    public sealed override void Detour_UseGrapple(Orig_UseGrapple orig, TSource self, Player player, ref int type)
    {
        Self = self;
        UseGrapple(player, ref type);
    }
    /// <inheritdoc cref="ModProjectile.UseGrapple"/>
    public virtual void UseGrapple(Player player, ref int type) { }

    // GrappleRange
    public sealed override float Detour_GrappleRange(Orig_GrappleRange orig, TSource self, Player player)
    {
        Self = self;
        return GrappleRange(player);
    }
    /// <inheritdoc cref="ModProjectile.GrappleRange"/>
    public virtual float GrappleRange(Player player) => 300f;

    // NumGrappleHooks
    public sealed override void Detour_NumGrappleHooks(Orig_NumGrappleHooks orig, TSource self, Player player, ref int numHooks)
    {
        Self = self;
        NumGrappleHooks(player, ref numHooks);
    }
    /// <inheritdoc cref="ModProjectile.NumGrappleHooks"/>
    public virtual void NumGrappleHooks(Player player, ref int numHooks) { }

    // GrappleRetreatSpeed
    public sealed override void Detour_GrappleRetreatSpeed(Orig_GrappleRetreatSpeed orig, TSource self, Player player, ref float speed)
    {
        Self = self;
        GrappleRetreatSpeed(player, ref speed);
    }
    /// <inheritdoc cref="ModProjectile.GrappleRetreatSpeed"/>
    public virtual void GrappleRetreatSpeed(Player player, ref float speed) { }

    // GrapplePullSpeed
    public sealed override void Detour_GrapplePullSpeed(Orig_GrapplePullSpeed orig, TSource self, Player player, ref float speed)
    {
        Self = self;
        GrapplePullSpeed(player, ref speed);
    }
    /// <inheritdoc cref="ModProjectile.GrapplePullSpeed"/>
    public virtual void GrapplePullSpeed(Player player, ref float speed) { }

    // GrappleTargetPoint
    public sealed override void Detour_GrappleTargetPoint(Orig_GrappleTargetPoint orig, TSource self, Player player, ref float grappleX, ref float grappleY)
    {
        Self = self;
        GrappleTargetPoint(player, ref grappleX, ref grappleY);
    }
    /// <inheritdoc cref="ModProjectile.GrappleTargetPoint"/>
    public virtual void GrappleTargetPoint(Player player, ref float grappleX, ref float grappleY) { }

    // GrappleCanLatchOnTo
    public sealed override bool? Detour_GrappleCanLatchOnTo(Orig_GrappleCanLatchOnTo orig, TSource self, Player player, int x, int y)
    {
        Self = self;
        return GrappleCanLatchOnTo(player, x, y);
    }
    /// <inheritdoc cref="ModProjectile.GrappleCanLatchOnTo"/>
    public virtual bool? GrappleCanLatchOnTo(Player player, int x, int y) => null;

    // DrawBehind
    public sealed override void Detour_DrawBehind(Orig_DrawBehind orig, TSource self, int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
    {
        Self = self;
        DrawBehind(index, behindNPCsAndTiles, behindNPCs, behindProjectiles, overPlayers, overWiresUI);
    }
    /// <inheritdoc cref="ModProjectile.DrawBehind"/>
    public virtual void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI) { }

    // PrepareBombToBlow
    public sealed override void Detour_PrepareBombToBlow(Orig_PrepareBombToBlow orig, TSource self)
    {
        Self = self;
        PrepareBombToBlow();
    }
    /// <inheritdoc cref="ModProjectile.PrepareBombToBlow"/>
    public virtual void PrepareBombToBlow() { }

    // EmitEnchantmentVisualsAt
    public sealed override void Detour_EmitEnchantmentVisualsAt(Orig_EmitEnchantmentVisualsAt orig, TSource self, Vector2 boxPosition, int boxWidth, int boxHeight)
    {
        Self = self;
        EmitEnchantmentVisualsAt(boxPosition, boxWidth, boxHeight);
    }
    /// <inheritdoc cref="ModProjectile.EmitEnchantmentVisualsAt"/>
    public virtual void EmitEnchantmentVisualsAt(Vector2 boxPosition, int boxWidth, int boxHeight) { }
}

/// <summary>
/// 用于物品覆盖的抽象类。
/// <para/>设计规范：
/// <br/>1. <strong>避免</strong>在该类中试图使用 <typeparamref name="TSource"/> 类中的任何方法。
/// <br/>2. <strong>尝试</strong>使用属性（必要时使用Publicizer）来访问 <typeparamref name="TSource"/> 中的数据。
/// <br/>3. <strong>避免</strong>在该类中使用实例字段。
/// </summary>
/// <typeparam name="TSource">待覆盖的 <see cref="ModItem"/> 类。</typeparam>
public abstract class CAItemOverride<TSource> : ModItemDetour<TSource>, ICALocalizationPrefix, ICATweak where TSource : ModItem
{
    private static readonly HashSet<string> _exclusiveDetours = ["get_LocalizationCategory", "get_Texture"];
    private static readonly HashSet<string> _criticalDetours = [];

    public static int ContentType => ModContent.ItemType<TSource>();

    public abstract CAGamePhase Phase { get; }
    public virtual string LocalizationName => Name ?? SourceType.Name;

    void ICATweak.RegisterTweak() => CASharedData.TweakedItems[ContentType] = true;

    protected override Hook ApplySingleDetour<TDelegate>(TDelegate detour, bool hasThis = true)
    {
        if (TODetourHandler.EvaluateDetourName(detour.Method, out string sourceName) && SourceType.HasMethod(sourceName, hasThis ? TOReflectionUtils.InstanceBindingFlags : TOReflectionUtils.StaticBindingFlags, out MethodInfo sourceMethod))
        {
            if (_criticalDetours.Contains(sourceName))
                return TODetourHandler.Modify(sourceMethod, detour);
            switch (GetType().HasRealMethod(sourceName, TOReflectionUtils.UniversalBindingFlags), sourceMethod.DeclaringType == SourceType)
            {
                case (true, true):
                    return TODetourHandler.Modify(sourceMethod, detour);
                case (false, true):
                    if (_exclusiveDetours.Contains(sourceName))
                        return null;
                    //CAMain.Instance.Logger.Warn($"""[CA Override] Source method "{sourceName}" does not have an implemented hook, and will not be hooked.""");
                    return null;
                case (_, false):
                    return null;
            }
        }
        return null;
    }

    public TSource Self;

    /// <inheritdoc cref="ModItem.Item"/>
    public Item Item => Self.Item;

    /// <inheritdoc cref="ModItem.Type"/>
    public int Type => Self.Type;

    // ModType
    // Not Supported
    public sealed override void Detour_Register(Orig_Register orig, TSource self) => base.Detour_Register(orig, self);
    public sealed override void Detour_InitTemplateInstance(Orig_InitTemplateInstance orig, TSource self) => base.Detour_InitTemplateInstance(orig, self);
    public sealed override void Detour_ValidateType(Orig_ValidateType orig, TSource self) => base.Detour_ValidateType(orig, self);

    // Name
    public sealed override string Detour_get_Name(Orig_get_Name orig, TSource self)
    {
        Self = self;
        return Name;
    }
    /// <inheritdoc cref="ModType.Name"/>
    public virtual string Name => Self.Name;

    // Load
    public sealed override void Detour_Load(Orig_Load orig, TSource self)
    {
        Self = self;
        Load();
    }
    /// <inheritdoc cref="ModType.Load"/>
    public virtual void Load() { }

    // IsLoadingEnabled
    public sealed override bool Detour_IsLoadingEnabled(Orig_IsLoadingEnabled orig, TSource self, Mod mod)
    {
        Self = self;
        return IsLoadingEnabled(mod);
    }
    /// <inheritdoc cref="ModType.IsLoadingEnabled(Mod)"/>
    public virtual bool IsLoadingEnabled(Mod mod) => true;

    // SetupContent
    public sealed override void Detour_SetupContent(Orig_SetupContent orig, TSource self)
    {
        Self = self;
        SetupContent();
    }
    /// <inheritdoc cref="ModType.SetupContent"/>
    public virtual void SetupContent() { }

    // SetStaticDefaults
    public sealed override void Detour_SetStaticDefaults(Orig_SetStaticDefaults orig, TSource self)
    {
        Self = self;
        SetStaticDefaults();
    }
    /// <inheritdoc cref="ModType.SetStaticDefaults"/>
    public virtual void SetStaticDefaults() { }

    // Unload
    public sealed override void Detour_Unload(Orig_Unload orig, TSource self)
    {
        Self = self;
        Unload();
    }
    /// <inheritdoc cref="ModType.Unload"/>
    public virtual void Unload() { }

    // ModItem
    // LocalizationCategory
    public sealed override string Detour_get_LocalizationCategory(Orig_get_LocalizationCategory orig, TSource self)
    {
        Self = self;
        return LocalizationCategory;
    }
    /// <inheritdoc cref="ModItem.LocalizationCategory"/>
    public virtual string LocalizationCategory => "Items";

    // DisplayName
    public sealed override LocalizedText Detour_get_DisplayName(Orig_get_DisplayName orig, TSource self)
    {
        Self = self;
        return DisplayName;
    }
    /// <inheritdoc cref="ModItem.DisplayName"/>
    public virtual LocalizedText DisplayName => Self.GetLocalization("DisplayName", Self.PrettyPrintName);

    // Tooltip
    public sealed override LocalizedText Detour_get_Tooltip(Orig_get_Tooltip orig, TSource self)
    {
        Self = self;
        return Tooltip;
    }
    /// <inheritdoc cref="ModItem.Tooltip"/>
    public virtual LocalizedText Tooltip => Self.GetLocalization("Tooltip", () => "");

    // Texture
    public sealed override string Detour_get_Texture(Orig_get_Texture orig, TSource self)
    {
        Self = self;
        return Texture;
    }
    /// <inheritdoc cref="ModItem.Texture"/>
    public virtual string Texture => (Self.GetType().Namespace + "." + Self.Name).Replace('.', '/');

    // SetDefaults
    public sealed override void Detour_SetDefaults(Orig_SetDefaults orig, TSource self)
    {
        Self = self;
        SetDefaults();
    }
    /// <inheritdoc cref="ModItem.SetDefaults"/>
    public virtual void SetDefaults() { }

    // OnSpawn
    public sealed override void Detour_OnSpawn(Orig_OnSpawn orig, TSource self, IEntitySource source)
    {
        Self = self;
        OnSpawn(source);
    }
    /// <inheritdoc cref="ModItem.OnSpawn"/>
    public virtual void OnSpawn(IEntitySource source) { }

    // OnCreated
    public sealed override void Detour_OnCreated(Orig_OnCreated orig, TSource self, ItemCreationContext context)
    {
        Self = self;
        OnCreated(context);
    }
    /// <inheritdoc cref="ModItem.OnCreated"/>
    public virtual void OnCreated(ItemCreationContext context) { }

    // AutoDefaults
    public sealed override void Detour_AutoDefaults(Orig_AutoDefaults orig, TSource self)
    {
        Self = self;
        AutoDefaults();
    }
    /// <inheritdoc cref="ModItem.AutoDefaults"/>
    public virtual void AutoDefaults() { }

    // AutoStaticDefaults
    public sealed override void Detour_AutoStaticDefaults(Orig_AutoStaticDefaults orig, TSource self)
    {
        Self = self;
        AutoStaticDefaults();
    }
    /// <inheritdoc cref="ModItem.AutoStaticDefaults"/>
    public virtual void AutoStaticDefaults() { }

    // ChoosePrefix
    public sealed override int Detour_ChoosePrefix(Orig_ChoosePrefix orig, TSource self, UnifiedRandom rand)
    {
        Self = self;
        return ChoosePrefix(rand);
    }
    /// <inheritdoc cref="ModItem.ChoosePrefix"/>
    public virtual int ChoosePrefix(UnifiedRandom rand) => -1;

    // MeleePrefix
    public sealed override bool Detour_MeleePrefix(Orig_MeleePrefix orig, TSource self)
    {
        Self = self;
        return MeleePrefix();
    }
    /// <inheritdoc cref="ModItem.MeleePrefix"/>
    public virtual bool MeleePrefix() => true;

    // WeaponPrefix
    public sealed override bool Detour_WeaponPrefix(Orig_WeaponPrefix orig, TSource self)
    {
        Self = self;
        return WeaponPrefix();
    }
    /// <inheritdoc cref="ModItem.WeaponPrefix"/>
    public virtual bool WeaponPrefix() => true;

    // RangedPrefix
    public sealed override bool Detour_RangedPrefix(Orig_RangedPrefix orig, TSource self)
    {
        Self = self;
        return RangedPrefix();
    }
    /// <inheritdoc cref="ModItem.RangedPrefix"/>
    public virtual bool RangedPrefix() => true;

    // MagicPrefix
    public sealed override bool Detour_MagicPrefix(Orig_MagicPrefix orig, TSource self)
    {
        Self = self;
        return MagicPrefix();
    }
    /// <inheritdoc cref="ModItem.MagicPrefix"/>
    public virtual bool MagicPrefix() => true;

    // PrefixChance
    public sealed override bool? Detour_PrefixChance(Orig_PrefixChance orig, TSource self, int pre, UnifiedRandom rand)
    {
        Self = self;
        return PrefixChance(pre, rand);
    }
    /// <inheritdoc cref="ModItem.PrefixChance"/>
    public virtual bool? PrefixChance(int pre, UnifiedRandom rand) => null;

    // AllowPrefix
    public sealed override bool Detour_AllowPrefix(Orig_AllowPrefix orig, TSource self, int pre)
    {
        Self = self;
        return AllowPrefix(pre);
    }
    /// <inheritdoc cref="ModItem.AllowPrefix"/>
    public virtual bool AllowPrefix(int pre) => true;

    // CanUseItem
    public sealed override bool Detour_CanUseItem(Orig_CanUseItem orig, TSource self, Player player)
    {
        Self = self;
        return CanUseItem(player);
    }
    /// <inheritdoc cref="ModItem.CanUseItem"/>
    public virtual bool CanUseItem(Player player) => true;

    // CanAutoReuseItem
    public sealed override bool? Detour_CanAutoReuseItem(Orig_CanAutoReuseItem orig, TSource self, Player player)
    {
        Self = self;
        return CanAutoReuseItem(player);
    }
    /// <inheritdoc cref="ModItem.CanAutoReuseItem"/>
    public virtual bool? CanAutoReuseItem(Player player) => null;

    // UseStyle
    public sealed override void Detour_UseStyle(Orig_UseStyle orig, TSource self, Player player, Rectangle heldItemFrame)
    {
        Self = self;
        UseStyle(player, heldItemFrame);
    }
    /// <inheritdoc cref="ModItem.UseStyle"/>
    public virtual void UseStyle(Player player, Rectangle heldItemFrame) { }

    // HoldStyle
    public sealed override void Detour_HoldStyle(Orig_HoldStyle orig, TSource self, Player player, Rectangle heldItemFrame)
    {
        Self = self;
        HoldStyle(player, heldItemFrame);
    }
    /// <inheritdoc cref="ModItem.HoldStyle"/>
    public virtual void HoldStyle(Player player, Rectangle heldItemFrame) { }

    // HoldItem
    public sealed override void Detour_HoldItem(Orig_HoldItem orig, TSource self, Player player)
    {
        Self = self;
        HoldItem(player);
    }
    /// <inheritdoc cref="ModItem.HoldItem"/>
    public virtual void HoldItem(Player player) { }

    // UseTimeMultiplier
    public sealed override float Detour_UseTimeMultiplier(Orig_UseTimeMultiplier orig, TSource self, Player player)
    {
        Self = self;
        return UseTimeMultiplier(player);
    }
    /// <inheritdoc cref="ModItem.UseTimeMultiplier"/>
    public virtual float UseTimeMultiplier(Player player) => 1f;

    // UseAnimationMultiplier
    public sealed override float Detour_UseAnimationMultiplier(Orig_UseAnimationMultiplier orig, TSource self, Player player)
    {
        Self = self;
        return UseAnimationMultiplier(player);
    }
    /// <inheritdoc cref="ModItem.UseAnimationMultiplier"/>
    public virtual float UseAnimationMultiplier(Player player) => 1f;

    // UseSpeedMultiplier
    public sealed override float Detour_UseSpeedMultiplier(Orig_UseSpeedMultiplier orig, TSource self, Player player)
    {
        Self = self;
        return UseSpeedMultiplier(player);
    }
    /// <inheritdoc cref="ModItem.UseSpeedMultiplier"/>
    public virtual float UseSpeedMultiplier(Player player) => 1f;

    // GetHealLife
    public sealed override void Detour_GetHealLife(Orig_GetHealLife orig, TSource self, Player player, bool quickHeal, ref int healValue)
    {
        Self = self;
        GetHealLife(player, quickHeal, ref healValue);
    }
    /// <inheritdoc cref="ModItem.GetHealLife"/>
    public virtual void GetHealLife(Player player, bool quickHeal, ref int healValue) { }

    // GetHealMana
    public sealed override void Detour_GetHealMana(Orig_GetHealMana orig, TSource self, Player player, bool quickHeal, ref int healValue)
    {
        Self = self;
        GetHealMana(player, quickHeal, ref healValue);
    }
    /// <inheritdoc cref="ModItem.GetHealMana"/>
    public virtual void GetHealMana(Player player, bool quickHeal, ref int healValue) { }

    // ModifyManaCost
    public sealed override void Detour_ModifyManaCost(Orig_ModifyManaCost orig, TSource self, Player player, ref float reduce, ref float mult)
    {
        Self = self;
        ModifyManaCost(player, ref reduce, ref mult);
    }
    /// <inheritdoc cref="ModItem.ModifyManaCost"/>
    public virtual void ModifyManaCost(Player player, ref float reduce, ref float mult) { }

    // OnMissingMana
    public sealed override void Detour_OnMissingMana(Orig_OnMissingMana orig, TSource self, Player player, int neededMana)
    {
        Self = self;
        OnMissingMana(player, neededMana);
    }
    /// <inheritdoc cref="ModItem.OnMissingMana"/>
    public virtual void OnMissingMana(Player player, int neededMana) { }

    // OnConsumeMana
    public sealed override void Detour_OnConsumeMana(Orig_OnConsumeMana orig, TSource self, Player player, int manaConsumed)
    {
        Self = self;
        OnConsumeMana(player, manaConsumed);
    }
    /// <inheritdoc cref="ModItem.OnConsumeMana"/>
    public virtual void OnConsumeMana(Player player, int manaConsumed) { }

    // ModifyPotionDelay
    public override void Detour_ModifyPotionDelay(Orig_ModifyPotionDelay orig, TSource self, Player player, ref int baseDelay)
    {
        Self = self;
        ModifyPotionDelay(player, ref baseDelay);
    }
    /// <inheritdoc cref="ModItem.ModifyPotionDelay"/>
    public virtual void ModifyPotionDelay(Player player, ref int baseDelay) { }

    // ApplyPotionDelay
    public override bool Detour_ApplyPotionDelay(Orig_ApplyPotionDelay orig, TSource self, Player player, int potionDelay)
    {
        Self = self;
        return ApplyPotionDelay(player, potionDelay);
    }
    /// <inheritdoc cref="ModItem.ApplyPotionDelay"/>
    public virtual bool ApplyPotionDelay(Player player, int potionDelay) => true;

    // ModifyWeaponDamage
    public sealed override void Detour_ModifyWeaponDamage(Orig_ModifyWeaponDamage orig, TSource self, Player player, ref StatModifier damage)
    {
        Self = self;
        ModifyWeaponDamage(player, ref damage);
    }
    /// <inheritdoc cref="ModItem.ModifyWeaponDamage"/>
    public virtual void ModifyWeaponDamage(Player player, ref StatModifier damage) { }

    // ModifyResearchSorting
    public sealed override void Detour_ModifyResearchSorting(Orig_ModifyResearchSorting orig, TSource self, ref ContentSamples.CreativeHelper.ItemGroup itemGroup)
    {
        Self = self;
        ModifyResearchSorting(ref itemGroup);
    }
    /// <inheritdoc cref="ModItem.ModifyResearchSorting"/>
    public virtual void ModifyResearchSorting(ref ContentSamples.CreativeHelper.ItemGroup itemGroup) { }

    // CanConsumeBait
    public sealed override bool? Detour_CanConsumeBait(Orig_CanConsumeBait orig, TSource self, Player player)
    {
        Self = self;
        return CanConsumeBait(player);
    }
    /// <inheritdoc cref="ModItem.CanConsumeBait"/>
    public virtual bool? CanConsumeBait(Player player) => null;

    // CanResearch
    public sealed override bool Detour_CanResearch(Orig_CanResearch orig, TSource self)
    {
        Self = self;
        return CanResearch();
    }
    /// <inheritdoc cref="ModItem.CanResearch"/>
    public virtual bool CanResearch() => true;

    // OnResearched
    public sealed override void Detour_OnResearched(Orig_OnResearched orig, TSource self, bool fullyResearched)
    {
        Self = self;
        OnResearched(fullyResearched);
    }
    /// <inheritdoc cref="ModItem.OnResearched"/>
    public virtual void OnResearched(bool fullyResearched) { }

    // ModifyWeaponKnockback
    public sealed override void Detour_ModifyWeaponKnockback(Orig_ModifyWeaponKnockback orig, TSource self, Player player, ref StatModifier knockback)
    {
        Self = self;
        ModifyWeaponKnockback(player, ref knockback);
    }
    /// <inheritdoc cref="ModItem.ModifyWeaponKnockback"/>
    public virtual void ModifyWeaponKnockback(Player player, ref StatModifier knockback) { }

    // ModifyWeaponCrit
    public sealed override void Detour_ModifyWeaponCrit(Orig_ModifyWeaponCrit orig, TSource self, Player player, ref float crit)
    {
        Self = self;
        ModifyWeaponCrit(player, ref crit);
    }
    /// <inheritdoc cref="ModItem.ModifyWeaponCrit"/>
    public virtual void ModifyWeaponCrit(Player player, ref float crit) { }

    // NeedsAmmo
    public sealed override bool Detour_NeedsAmmo(Orig_NeedsAmmo orig, TSource self, Player player)
    {
        Self = self;
        return NeedsAmmo(player);
    }
    /// <inheritdoc cref="ModItem.NeedsAmmo"/>
    public virtual bool NeedsAmmo(Player player) => true;

    // PickAmmo
    public sealed override void Detour_PickAmmo(Orig_PickAmmo orig, TSource self, Item weapon, Player player, ref int type, ref float speed, ref StatModifier damage, ref float knockback)
    {
        Self = self;
        PickAmmo(weapon, player, ref type, ref speed, ref damage, ref knockback);
    }
    /// <inheritdoc cref="ModItem.PickAmmo"/>
    public virtual void PickAmmo(Item weapon, Player player, ref int type, ref float speed, ref StatModifier damage, ref float knockback) { }

    // CanChooseAmmo
    public sealed override bool? Detour_CanChooseAmmo(Orig_CanChooseAmmo orig, TSource self, Item ammo, Player player)
    {
        Self = self;
        return CanChooseAmmo(ammo, player);
    }
    /// <inheritdoc cref="ModItem.CanChooseAmmo"/>
    public virtual bool? CanChooseAmmo(Item ammo, Player player) => null;

    // CanBeChosenAsAmmo
    public sealed override bool? Detour_CanBeChosenAsAmmo(Orig_CanBeChosenAsAmmo orig, TSource self, Item weapon, Player player)
    {
        Self = self;
        return CanBeChosenAsAmmo(weapon, player);
    }
    /// <inheritdoc cref="ModItem.CanBeChosenAsAmmo"/>
    public virtual bool? CanBeChosenAsAmmo(Item weapon, Player player) => null;

    // CanConsumeAmmo
    public sealed override bool Detour_CanConsumeAmmo(Orig_CanConsumeAmmo orig, TSource self, Item ammo, Player player)
    {
        Self = self;
        return CanConsumeAmmo(ammo, player);
    }
    /// <inheritdoc cref="ModItem.CanConsumeAmmo"/>
    public virtual bool CanConsumeAmmo(Item ammo, Player player) => true;

    // CanBeConsumedAsAmmo
    public sealed override bool Detour_CanBeConsumedAsAmmo(Orig_CanBeConsumedAsAmmo orig, TSource self, Item weapon, Player player)
    {
        Self = self;
        return CanBeConsumedAsAmmo(weapon, player);
    }
    /// <inheritdoc cref="ModItem.CanBeConsumedAsAmmo"/>
    public virtual bool CanBeConsumedAsAmmo(Item weapon, Player player) => true;

    // OnConsumeAmmo
    public sealed override void Detour_OnConsumeAmmo(Orig_OnConsumeAmmo orig, TSource self, Item ammo, Player player)
    {
        Self = self;
        OnConsumeAmmo(ammo, player);
    }
    /// <inheritdoc cref="ModItem.OnConsumeAmmo"/>
    public virtual void OnConsumeAmmo(Item ammo, Player player) { }

    // OnConsumedAsAmmo
    public sealed override void Detour_OnConsumedAsAmmo(Orig_OnConsumedAsAmmo orig, TSource self, Item weapon, Player player)
    {
        Self = self;
        OnConsumedAsAmmo(weapon, player);
    }
    /// <inheritdoc cref="ModItem.OnConsumedAsAmmo"/>
    public virtual void OnConsumedAsAmmo(Item weapon, Player player) { }

    // CanShoot
    public sealed override bool Detour_CanShoot(Orig_CanShoot orig, TSource self, Player player)
    {
        Self = self;
        return CanShoot(player);
    }
    /// <inheritdoc cref="ModItem.CanShoot"/>
    public virtual bool CanShoot(Player player) => true;

    // ModifyShootStats
    public sealed override void Detour_ModifyShootStats(Orig_ModifyShootStats orig, TSource self, Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
    {
        Self = self;
        ModifyShootStats(player, ref position, ref velocity, ref type, ref damage, ref knockback);
    }
    /// <inheritdoc cref="ModItem.ModifyShootStats"/>
    public virtual void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) { }

    // Shoot
    public sealed override bool Detour_Shoot(Orig_Shoot orig, TSource self, Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
    {
        Self = self;
        return Shoot(player, source, position, velocity, type, damage, knockback);
    }
    /// <inheritdoc cref="ModItem.Shoot"/>
    public virtual bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) => true;

    // UseItemHitbox
    public sealed override void Detour_UseItemHitbox(Orig_UseItemHitbox orig, TSource self, Player player, ref Rectangle hitbox, ref bool noHitbox)
    {
        Self = self;
        UseItemHitbox(player, ref hitbox, ref noHitbox);
    }
    /// <inheritdoc cref="ModItem.UseItemHitbox"/>
    public virtual void UseItemHitbox(Player player, ref Rectangle hitbox, ref bool noHitbox) { }

    // MeleeEffects
    public sealed override void Detour_MeleeEffects(Orig_MeleeEffects orig, TSource self, Player player, Rectangle hitbox)
    {
        Self = self;
        MeleeEffects(player, hitbox);
    }
    /// <inheritdoc cref="ModItem.MeleeEffects"/>
    public virtual void MeleeEffects(Player player, Rectangle hitbox) { }

    // CanCatchNPC
    public sealed override bool? Detour_CanCatchNPC(Orig_CanCatchNPC orig, TSource self, NPC target, Player player)
    {
        Self = self;
        return CanCatchNPC(target, player);
    }
    /// <inheritdoc cref="ModItem.CanCatchNPC"/>
    public virtual bool? CanCatchNPC(NPC target, Player player) => null;

    // OnCatchNPC
    public sealed override void Detour_OnCatchNPC(Orig_OnCatchNPC orig, TSource self, NPC npc, Player player, bool failed)
    {
        Self = self;
        OnCatchNPC(npc, player, failed);
    }
    /// <inheritdoc cref="ModItem.OnCatchNPC"/>
    public virtual void OnCatchNPC(NPC npc, Player player, bool failed) { }

    // ModifyItemScale
    public sealed override void Detour_ModifyItemScale(Orig_ModifyItemScale orig, TSource self, Player player, ref float scale)
    {
        Self = self;
        ModifyItemScale(player, ref scale);
    }
    /// <inheritdoc cref="ModItem.ModifyItemScale"/>
    public virtual void ModifyItemScale(Player player, ref float scale) { }

    // CanHitNPC
    public sealed override bool? Detour_CanHitNPC(Orig_CanHitNPC orig, TSource self, Player player, NPC target)
    {
        Self = self;
        return CanHitNPC(player, target);
    }
    /// <inheritdoc cref="ModItem.CanHitNPC"/>
    public virtual bool? CanHitNPC(Player player, NPC target) => null;

    // CanMeleeAttackCollideWithNPC
    public sealed override bool? Detour_CanMeleeAttackCollideWithNPC(Orig_CanMeleeAttackCollideWithNPC orig, TSource self, Rectangle meleeAttackHitbox, Player player, NPC target)
    {
        Self = self;
        return CanMeleeAttackCollideWithNPC(meleeAttackHitbox, player, target);
    }
    /// <inheritdoc cref="ModItem.CanMeleeAttackCollideWithNPC"/>
    public virtual bool? CanMeleeAttackCollideWithNPC(Rectangle meleeAttackHitbox, Player player, NPC target) => null;

    // ModifyHitNPC
    public sealed override void Detour_ModifyHitNPC(Orig_ModifyHitNPC orig, TSource self, Player player, NPC target, ref NPC.HitModifiers modifiers)
    {
        Self = self;
        ModifyHitNPC(player, target, ref modifiers);
    }
    /// <inheritdoc cref="ModItem.ModifyHitNPC"/>
    public virtual void ModifyHitNPC(Player player, NPC target, ref NPC.HitModifiers modifiers) { }

    // OnHitNPC
    public sealed override void Detour_OnHitNPC(Orig_OnHitNPC orig, TSource self, Player player, NPC target, NPC.HitInfo hit, int damageDone)
    {
        Self = self;
        OnHitNPC(player, target, hit, damageDone);
    }
    /// <inheritdoc cref="ModItem.OnHitNPC"/>
    public virtual void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone) { }

    // CanHitPvp
    public sealed override bool Detour_CanHitPvp(Orig_CanHitPvp orig, TSource self, Player player, Player target)
    {
        Self = self;
        return CanHitPvp(player, target);
    }
    /// <inheritdoc cref="ModItem.CanHitPvp"/>
    public virtual bool CanHitPvp(Player player, Player target) => true;

    // ModifyHitPvp
    public sealed override void Detour_ModifyHitPvp(Orig_ModifyHitPvp orig, TSource self, Player player, Player target, ref Player.HurtModifiers modifiers)
    {
        Self = self;
        ModifyHitPvp(player, target, ref modifiers);
    }
    /// <inheritdoc cref="ModItem.ModifyHitPvp"/>
    public virtual void ModifyHitPvp(Player player, Player target, ref Player.HurtModifiers modifiers) { }

    // OnHitPvp
    public sealed override void Detour_OnHitPvp(Orig_OnHitPvp orig, TSource self, Player player, Player target, Player.HurtInfo hurtInfo)
    {
        Self = self;
        OnHitPvp(player, target, hurtInfo);
    }
    /// <inheritdoc cref="ModItem.OnHitPvp"/>
    public virtual void OnHitPvp(Player player, Player target, Player.HurtInfo hurtInfo) { }

    // UseItem
    public sealed override bool? Detour_UseItem(Orig_UseItem orig, TSource self, Player player)
    {
        Self = self;
        return UseItem(player);
    }
    /// <inheritdoc cref="ModItem.UseItem"/>
    public virtual bool? UseItem(Player player) => null;

    // UseAnimation
    public sealed override void Detour_UseAnimation(Orig_UseAnimation orig, TSource self, Player player)
    {
        Self = self;
        UseAnimation(player);
    }
    /// <inheritdoc cref="ModItem.UseAnimation"/>
    public virtual void UseAnimation(Player player) { }

    // ConsumeItem
    public sealed override bool Detour_ConsumeItem(Orig_ConsumeItem orig, TSource self, Player player)
    {
        Self = self;
        return ConsumeItem(player);
    }
    /// <inheritdoc cref="ModItem.ConsumeItem"/>
    public virtual bool ConsumeItem(Player player) => true;

    // OnConsumeItem
    public sealed override void Detour_OnConsumeItem(Orig_OnConsumeItem orig, TSource self, Player player)
    {
        Self = self;
        OnConsumeItem(player);
    }
    /// <inheritdoc cref="ModItem.OnConsumeItem"/>
    public virtual void OnConsumeItem(Player player) { }

    // UseItemFrame
    public sealed override void Detour_UseItemFrame(Orig_UseItemFrame orig, TSource self, Player player)
    {
        Self = self;
        UseItemFrame(player);
    }
    /// <inheritdoc cref="ModItem.UseItemFrame"/>
    public virtual void UseItemFrame(Player player) { }

    // HoldItemFrame
    public sealed override void Detour_HoldItemFrame(Orig_HoldItemFrame orig, TSource self, Player player)
    {
        Self = self;
        HoldItemFrame(player);
    }
    /// <inheritdoc cref="ModItem.HoldItemFrame"/>
    public virtual void HoldItemFrame(Player player) { }

    // AltFunctionUse
    public sealed override bool Detour_AltFunctionUse(Orig_AltFunctionUse orig, TSource self, Player player)
    {
        Self = self;
        return AltFunctionUse(player);
    }
    /// <inheritdoc cref="ModItem.AltFunctionUse"/>
    public virtual bool AltFunctionUse(Player player) => false;

    // UpdateInventory
    public sealed override void Detour_UpdateInventory(Orig_UpdateInventory orig, TSource self, Player player)
    {
        Self = self;
        UpdateInventory(player);
    }
    /// <inheritdoc cref="ModItem.UpdateInventory"/>
    public virtual void UpdateInventory(Player player) { }

    // UpdateInfoAccessory
    public sealed override void Detour_UpdateInfoAccessory(Orig_UpdateInfoAccessory orig, TSource self, Player player)
    {
        Self = self;
        UpdateInfoAccessory(player);
    }
    /// <inheritdoc cref="ModItem.UpdateInfoAccessory"/>
    public virtual void UpdateInfoAccessory(Player player) { }

    // UpdateEquip
    public sealed override void Detour_UpdateEquip(Orig_UpdateEquip orig, TSource self, Player player)
    {
        Self = self;
        UpdateEquip(player);
    }
    /// <inheritdoc cref="ModItem.UpdateEquip"/>
    public virtual void UpdateEquip(Player player) { }

    // UpdateAccessory
    public sealed override void Detour_UpdateAccessory(Orig_UpdateAccessory orig, TSource self, Player player, bool hideVisual)
    {
        Self = self;
        UpdateAccessory(player, hideVisual);
    }
    /// <inheritdoc cref="ModItem.UpdateAccessory"/>
    public virtual void UpdateAccessory(Player player, bool hideVisual) { }

    // UpdateVanity
    public sealed override void Detour_UpdateVanity(Orig_UpdateVanity orig, TSource self, Player player)
    {
        Self = self;
        UpdateVanity(player);
    }
    /// <inheritdoc cref="ModItem.UpdateVanity"/>
    public virtual void UpdateVanity(Player player) { }

    // UpdateVisibleAccessory
    public sealed override void Detour_UpdateVisibleAccessory(Orig_UpdateVisibleAccessory orig, TSource self, Player player, bool hideVisual)
    {
        Self = self;
        UpdateVisibleAccessory(player, hideVisual);
    }
    /// <inheritdoc cref="ModItem.UpdateVisibleAccessory"/>
    public virtual void UpdateVisibleAccessory(Player player, bool hideVisual) { }

    // UpdateItemDye
    public sealed override void Detour_UpdateItemDye(Orig_UpdateItemDye orig, TSource self, Player player, int dye, bool hideVisual)
    {
        Self = self;
        UpdateItemDye(player, dye, hideVisual);
    }
    /// <inheritdoc cref="ModItem.UpdateItemDye"/>
    public virtual void UpdateItemDye(Player player, int dye, bool hideVisual) { }

    // EquipFrameEffects
    public sealed override void Detour_EquipFrameEffects(Orig_EquipFrameEffects orig, TSource self, Player player, EquipType type)
    {
        Self = self;
        EquipFrameEffects(player, type);
    }
    /// <inheritdoc cref="ModItem.EquipFrameEffects"/>
    public virtual void EquipFrameEffects(Player player, EquipType type) { }

    // IsArmorSet
    public sealed override bool Detour_IsArmorSet(Orig_IsArmorSet orig, TSource self, Item head, Item body, Item legs)
    {
        Self = self;
        return IsArmorSet(head, body, legs);
    }
    /// <inheritdoc cref="ModItem.IsArmorSet"/>
    public virtual bool IsArmorSet(Item head, Item body, Item legs) => false;

    // UpdateArmorSet
    public sealed override void Detour_UpdateArmorSet(Orig_UpdateArmorSet orig, TSource self, Player player)
    {
        Self = self;
        UpdateArmorSet(player);
    }
    /// <inheritdoc cref="ModItem.UpdateArmorSet"/>
    public virtual void UpdateArmorSet(Player player) { }

    // IsVanitySet
    public sealed override bool Detour_IsVanitySet(Orig_IsVanitySet orig, TSource self, int head, int body, int legs)
    {
        Self = self;
        return IsVanitySet(head, body, legs);
    }
    /// <inheritdoc cref="ModItem.IsVanitySet"/>
    public virtual bool IsVanitySet(int head, int body, int legs) => false;

    // PreUpdateVanitySet
    public sealed override void Detour_PreUpdateVanitySet(Orig_PreUpdateVanitySet orig, TSource self, Player player)
    {
        Self = self;
        PreUpdateVanitySet(player);
    }
    /// <inheritdoc cref="ModItem.PreUpdateVanitySet"/>
    public virtual void PreUpdateVanitySet(Player player) { }

    // UpdateVanitySet
    public sealed override void Detour_UpdateVanitySet(Orig_UpdateVanitySet orig, TSource self, Player player)
    {
        Self = self;
        UpdateVanitySet(player);
    }
    /// <inheritdoc cref="ModItem.UpdateVanitySet"/>
    public virtual void UpdateVanitySet(Player player) { }

    // ArmorSetShadows
    public sealed override void Detour_ArmorSetShadows(Orig_ArmorSetShadows orig, TSource self, Player player)
    {
        Self = self;
        ArmorSetShadows(player);
    }
    /// <inheritdoc cref="ModItem.ArmorSetShadows"/>
    public virtual void ArmorSetShadows(Player player) { }

    // SetMatch
    public sealed override void Detour_SetMatch(Orig_SetMatch orig, TSource self, bool male, ref int equipSlot, ref bool robes)
    {
        Self = self;
        SetMatch(male, ref equipSlot, ref robes);
    }
    /// <inheritdoc cref="ModItem.SetMatch"/>
    public virtual void SetMatch(bool male, ref int equipSlot, ref bool robes) { }

    // CanRightClick
    public sealed override bool Detour_CanRightClick(Orig_CanRightClick orig, TSource self)
    {
        Self = self;
        return CanRightClick();
    }
    /// <inheritdoc cref="ModItem.CanRightClick"/>
    public virtual bool CanRightClick() => false;

    // RightClick
    public sealed override void Detour_RightClick(Orig_RightClick orig, TSource self, Player player)
    {
        Self = self;
        RightClick(player);
    }
    /// <inheritdoc cref="ModItem.RightClick"/>
    public virtual void RightClick(Player player) { }

    // ModifyItemLoot
    public sealed override void Detour_ModifyItemLoot(Orig_ModifyItemLoot orig, TSource self, ItemLoot itemLoot)
    {
        Self = self;
        ModifyItemLoot(itemLoot);
    }
    /// <inheritdoc cref="ModItem.ModifyItemLoot"/>
    public virtual void ModifyItemLoot(ItemLoot itemLoot) { }

    // CanStack
    public sealed override bool Detour_CanStack(Orig_CanStack orig, TSource self, Item source)
    {
        Self = self;
        return CanStack(source);
    }
    /// <inheritdoc cref="ModItem.CanStack"/>
    public virtual bool CanStack(Item source) => true;

    // CanStackInWorld
    public sealed override bool Detour_CanStackInWorld(Orig_CanStackInWorld orig, TSource self, Item source)
    {
        Self = self;
        return CanStackInWorld(source);
    }
    /// <inheritdoc cref="ModItem.CanStackInWorld"/>
    public virtual bool CanStackInWorld(Item source) => true;

    // OnStack
    public sealed override void Detour_OnStack(Orig_OnStack orig, TSource self, Item source, int numToTransfer)
    {
        Self = self;
        OnStack(source, numToTransfer);
    }
    /// <inheritdoc cref="ModItem.OnStack"/>
    public virtual void OnStack(Item source, int numToTransfer) { }

    // SplitStack
    public sealed override void Detour_SplitStack(Orig_SplitStack orig, TSource self, Item source, int numToTransfer)
    {
        Self = self;
        SplitStack(source, numToTransfer);
    }
    /// <inheritdoc cref="ModItem.SplitStack"/>
    public virtual void SplitStack(Item source, int numToTransfer) { }

    // ReforgePrice
    public sealed override bool Detour_ReforgePrice(Orig_ReforgePrice orig, TSource self, ref int reforgePrice, ref bool canApplyDiscount)
    {
        Self = self;
        return ReforgePrice(ref reforgePrice, ref canApplyDiscount);
    }
    /// <inheritdoc cref="ModItem.ReforgePrice"/>
    public virtual bool ReforgePrice(ref int reforgePrice, ref bool canApplyDiscount) => true;

    // CanReforge
    public sealed override bool Detour_CanReforge(Orig_CanReforge orig, TSource self)
    {
        Self = self;
        return CanReforge();
    }
    /// <inheritdoc cref="ModItem.CanReforge"/>
    public virtual bool CanReforge() => true;

    // PreReforge
    public sealed override void Detour_PreReforge(Orig_PreReforge orig, TSource self)
    {
        Self = self;
        PreReforge();
    }
    /// <inheritdoc cref="ModItem.PreReforge"/>
    public virtual void PreReforge() { }

    // PostReforge
    public sealed override void Detour_PostReforge(Orig_PostReforge orig, TSource self)
    {
        Self = self;
        PostReforge();
    }
    /// <inheritdoc cref="ModItem.PostReforge"/>
    public virtual void PostReforge() { }

    // DrawArmorColor
    public sealed override void Detour_DrawArmorColor(Orig_DrawArmorColor orig, TSource self, Player drawPlayer, float shadow, ref Color color, ref int glowMask, ref Color glowMaskColor)
    {
        Self = self;
        DrawArmorColor(drawPlayer, shadow, ref color, ref glowMask, ref glowMaskColor);
    }
    /// <inheritdoc cref="ModItem.DrawArmorColor"/>
    public virtual void DrawArmorColor(Player drawPlayer, float shadow, ref Color color, ref int glowMask, ref Color glowMaskColor) { }

    // ArmorArmGlowMask
    public sealed override void Detour_ArmorArmGlowMask(Orig_ArmorArmGlowMask orig, TSource self, Player drawPlayer, float shadow, ref int glowMask, ref Color color)
    {
        Self = self;
        ArmorArmGlowMask(drawPlayer, shadow, ref glowMask, ref color);
    }
    /// <inheritdoc cref="ModItem.ArmorArmGlowMask"/>
    public virtual void ArmorArmGlowMask(Player drawPlayer, float shadow, ref int glowMask, ref Color color) { }

    // VerticalWingSpeeds
    public sealed override void Detour_VerticalWingSpeeds(Orig_VerticalWingSpeeds orig, TSource self, Player player, ref float ascentWhenFalling, ref float ascentWhenRising, ref float maxCanAscendMultiplier, ref float maxAscentMultiplier, ref float constantAscend)
    {
        Self = self;
        VerticalWingSpeeds(player, ref ascentWhenFalling, ref ascentWhenRising, ref maxCanAscendMultiplier, ref maxAscentMultiplier, ref constantAscend);
    }
    /// <inheritdoc cref="ModItem.VerticalWingSpeeds"/>
    public virtual void VerticalWingSpeeds(Player player, ref float ascentWhenFalling, ref float ascentWhenRising, ref float maxCanAscendMultiplier, ref float maxAscentMultiplier, ref float constantAscend) { }

    // HorizontalWingSpeeds
    public sealed override void Detour_HorizontalWingSpeeds(Orig_HorizontalWingSpeeds orig, TSource self, Player player, ref float speed, ref float acceleration)
    {
        Self = self;
        HorizontalWingSpeeds(player, ref speed, ref acceleration);
    }
    /// <inheritdoc cref="ModItem.HorizontalWingSpeeds"/>
    public virtual void HorizontalWingSpeeds(Player player, ref float speed, ref float acceleration) { }

    // WingUpdate
    public sealed override bool Detour_WingUpdate(Orig_WingUpdate orig, TSource self, Player player, bool inUse)
    {
        Self = self;
        return WingUpdate(player, inUse);
    }
    /// <inheritdoc cref="ModItem.WingUpdate"/>
    public virtual bool WingUpdate(Player player, bool inUse) => false;

    // Update
    public sealed override void Detour_Update(Orig_Update orig, TSource self, ref float gravity, ref float maxFallSpeed)
    {
        Self = self;
        Update(ref gravity, ref maxFallSpeed);
    }
    /// <inheritdoc cref="ModItem.Update"/>
    public virtual void Update(ref float gravity, ref float maxFallSpeed) { }

    // PostUpdate
    public sealed override void Detour_PostUpdate(Orig_PostUpdate orig, TSource self)
    {
        Self = self;
        PostUpdate();
    }
    /// <inheritdoc cref="ModItem.PostUpdate"/>
    public virtual void PostUpdate() { }

    // GrabRange
    public sealed override void Detour_GrabRange(Orig_GrabRange orig, TSource self, Player player, ref int grabRange)
    {
        Self = self;
        GrabRange(player, ref grabRange);
    }
    /// <inheritdoc cref="ModItem.GrabRange"/>
    public virtual void GrabRange(Player player, ref int grabRange) { }

    // GrabStyle
    public sealed override bool Detour_GrabStyle(Orig_GrabStyle orig, TSource self, Player player)
    {
        Self = self;
        return GrabStyle(player);
    }
    /// <inheritdoc cref="ModItem.GrabStyle"/>
    public virtual bool GrabStyle(Player player) => false;

    // CanPickup
    public sealed override bool Detour_CanPickup(Orig_CanPickup orig, TSource self, Player player)
    {
        Self = self;
        return CanPickup(player);
    }
    /// <inheritdoc cref="ModItem.CanPickup"/>
    public virtual bool CanPickup(Player player) => true;

    // OnPickup
    public sealed override bool Detour_OnPickup(Orig_OnPickup orig, TSource self, Player player)
    {
        Self = self;
        return OnPickup(player);
    }
    /// <inheritdoc cref="ModItem.OnPickup"/>
    public virtual bool OnPickup(Player player) => true;

    // ItemSpace
    public sealed override bool Detour_ItemSpace(Orig_ItemSpace orig, TSource self, Player player)
    {
        Self = self;
        return ItemSpace(player);
    }
    /// <inheritdoc cref="ModItem.ItemSpace"/>
    public virtual bool ItemSpace(Player player) => false;

    // GetAlpha
    public sealed override Color? Detour_GetAlpha(Orig_GetAlpha orig, TSource self, Color lightColor)
    {
        Self = self;
        return GetAlpha(lightColor);
    }
    /// <inheritdoc cref="ModItem.GetAlpha"/>
    public virtual Color? GetAlpha(Color lightColor) => null;

    // PreDrawInWorld
    public sealed override bool Detour_PreDrawInWorld(Orig_PreDrawInWorld orig, TSource self, SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
    {
        Self = self;
        return PreDrawInWorld(spriteBatch, lightColor, alphaColor, ref rotation, ref scale, whoAmI);
    }
    /// <inheritdoc cref="ModItem.PreDrawInWorld"/>
    public virtual bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI) => true;

    // PostDrawInWorld
    public sealed override void Detour_PostDrawInWorld(Orig_PostDrawInWorld orig, TSource self, SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI)
    {
        Self = self;
        PostDrawInWorld(spriteBatch, lightColor, alphaColor, rotation, scale, whoAmI);
    }
    /// <inheritdoc cref="ModItem.PostDrawInWorld"/>
    public virtual void PostDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI) { }

    // PreDrawInInventory
    public sealed override bool Detour_PreDrawInInventory(Orig_PreDrawInInventory orig, TSource self, SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
    {
        Self = self;
        return PreDrawInInventory(spriteBatch, position, frame, drawColor, itemColor, origin, scale);
    }
    /// <inheritdoc cref="ModItem.PreDrawInInventory"/>
    public virtual bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale) => true;

    // PostDrawInInventory
    public sealed override void Detour_PostDrawInInventory(Orig_PostDrawInInventory orig, TSource self, SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
    {
        Self = self;
        PostDrawInInventory(spriteBatch, position, frame, drawColor, itemColor, origin, scale);
    }
    /// <inheritdoc cref="ModItem.PostDrawInInventory"/>
    public virtual void PostDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale) { }

    // HoldoutOffset
    public sealed override Vector2? Detour_HoldoutOffset(Orig_HoldoutOffset orig, TSource self)
    {
        Self = self;
        return HoldoutOffset();
    }
    /// <inheritdoc cref="ModItem.HoldoutOffset"/>
    public virtual Vector2? HoldoutOffset() => null;

    // HoldoutOrigin
    public sealed override Vector2? Detour_HoldoutOrigin(Orig_HoldoutOrigin orig, TSource self)
    {
        Self = self;
        return HoldoutOrigin();
    }
    /// <inheritdoc cref="ModItem.HoldoutOrigin"/>
    public virtual Vector2? HoldoutOrigin() => null;

    // CanEquipAccessory
    public sealed override bool Detour_CanEquipAccessory(Orig_CanEquipAccessory orig, TSource self, Player player, int slot, bool modded)
    {
        Self = self;
        return CanEquipAccessory(player, slot, modded);
    }
    /// <inheritdoc cref="ModItem.CanEquipAccessory"/>
    public virtual bool CanEquipAccessory(Player player, int slot, bool modded) => true;

    // CanAccessoryBeEquippedWith
    public sealed override bool Detour_CanAccessoryBeEquippedWith(Orig_CanAccessoryBeEquippedWith orig, TSource self, Item equippedItem, Item incomingItem, Player player)
    {
        Self = self;
        return CanAccessoryBeEquippedWith(equippedItem, incomingItem, player);
    }
    /// <inheritdoc cref="ModItem.CanAccessoryBeEquippedWith"/>
    public virtual bool CanAccessoryBeEquippedWith(Item equippedItem, Item incomingItem, Player player) => true;

    // ExtractinatorUse
    public sealed override void Detour_ExtractinatorUse(Orig_ExtractinatorUse orig, TSource self, int extractinatorBlockType, ref int resultType, ref int resultStack)
    {
        Self = self;
        ExtractinatorUse(extractinatorBlockType, ref resultType, ref resultStack);
    }
    /// <inheritdoc cref="ModItem.ExtractinatorUse"/>
    public virtual void ExtractinatorUse(int extractinatorBlockType, ref int resultType, ref int resultStack) { }

    // ModifyFishingLine
    public sealed override void Detour_ModifyFishingLine(Orig_ModifyFishingLine orig, TSource self, Projectile bobber, ref Vector2 lineOriginOffset, ref Color lineColor)
    {
        Self = self;
        ModifyFishingLine(bobber, ref lineOriginOffset, ref lineColor);
    }
    /// <inheritdoc cref="ModItem.ModifyFishingLine"/>
    public virtual void ModifyFishingLine(Projectile bobber, ref Vector2 lineOriginOffset, ref Color lineColor) { }

    // CaughtFishStack
    public sealed override void Detour_CaughtFishStack(Orig_CaughtFishStack orig, TSource self, ref int stack)
    {
        Self = self;
        CaughtFishStack(ref stack);
    }
    /// <inheritdoc cref="ModItem.CaughtFishStack"/>
    public virtual void CaughtFishStack(ref int stack) { }

    // IsQuestFish
    public sealed override bool Detour_IsQuestFish(Orig_IsQuestFish orig, TSource self)
    {
        Self = self;
        return IsQuestFish();
    }
    /// <inheritdoc cref="ModItem.IsQuestFish"/>
    public virtual bool IsQuestFish() => false;

    // IsAnglerQuestAvailable
    public sealed override bool Detour_IsAnglerQuestAvailable(Orig_IsAnglerQuestAvailable orig, TSource self)
    {
        Self = self;
        return IsAnglerQuestAvailable();
    }
    /// <inheritdoc cref="ModItem.IsAnglerQuestAvailable"/>
    public virtual bool IsAnglerQuestAvailable() => true;

    // AnglerQuestChat
    public sealed override void Detour_AnglerQuestChat(Orig_AnglerQuestChat orig, TSource self, ref string description, ref string catchLocation)
    {
        Self = self;
        AnglerQuestChat(ref description, ref catchLocation);
    }
    /// <inheritdoc cref="ModItem.AnglerQuestChat"/>
    public virtual void AnglerQuestChat(ref string description, ref string catchLocation) { }

    // SaveData
    public sealed override void Detour_SaveData(Orig_SaveData orig, TSource self, TagCompound tag)
    {
        Self = self;
        SaveData(tag);
    }
    /// <inheritdoc cref="ModItem.SaveData"/>
    public virtual void SaveData(TagCompound tag) { }

    // LoadData
    public sealed override void Detour_LoadData(Orig_LoadData orig, TSource self, TagCompound tag)
    {
        Self = self;
        LoadData(tag);
    }
    /// <inheritdoc cref="ModItem.LoadData"/>
    public virtual void LoadData(TagCompound tag) { }

    // NetSend
    public sealed override void Detour_NetSend(Orig_NetSend orig, TSource self, BinaryWriter writer)
    {
        Self = self;
        NetSend(writer);
    }
    /// <inheritdoc cref="ModItem.NetSend"/>
    public virtual void NetSend(BinaryWriter writer) { }

    // NetReceive
    public sealed override void Detour_NetReceive(Orig_NetReceive orig, TSource self, BinaryReader reader)
    {
        Self = self;
        NetReceive(reader);
    }
    /// <inheritdoc cref="ModItem.NetReceive"/>
    public virtual void NetReceive(BinaryReader reader) { }

    // AddRecipes
    public sealed override void Detour_AddRecipes(Orig_AddRecipes orig, TSource self)
    {
        Self = self;
        AddRecipes();
    }
    /// <inheritdoc cref="ModItem.AddRecipes"/>
    public virtual void AddRecipes() { }

    // PreDrawTooltip
    public sealed override bool Detour_PreDrawTooltip(Orig_PreDrawTooltip orig, TSource self, ReadOnlyCollection<TooltipLine> lines, ref int x, ref int y)
    {
        Self = self;
        return PreDrawTooltip(lines, ref x, ref y);
    }
    /// <inheritdoc cref="ModItem.PreDrawTooltip"/>
    public virtual bool PreDrawTooltip(ReadOnlyCollection<TooltipLine> lines, ref int x, ref int y) => true;

    // PostDrawTooltip
    public sealed override void Detour_PostDrawTooltip(Orig_PostDrawTooltip orig, TSource self, ReadOnlyCollection<DrawableTooltipLine> lines)
    {
        Self = self;
        PostDrawTooltip(lines);
    }
    /// <inheritdoc cref="ModItem.PostDrawTooltip"/>
    public virtual void PostDrawTooltip(ReadOnlyCollection<DrawableTooltipLine> lines) { }

    // PreDrawTooltipLine
    public sealed override bool Detour_PreDrawTooltipLine(Orig_PreDrawTooltipLine orig, TSource self, DrawableTooltipLine line, ref int yOffset)
    {
        Self = self;
        return PreDrawTooltipLine(line, ref yOffset);
    }
    /// <inheritdoc cref="ModItem.PreDrawTooltipLine"/>
    public virtual bool PreDrawTooltipLine(DrawableTooltipLine line, ref int yOffset) => true;

    // PostDrawTooltipLine
    public sealed override void Detour_PostDrawTooltipLine(Orig_PostDrawTooltipLine orig, TSource self, DrawableTooltipLine line)
    {
        Self = self;
        PostDrawTooltipLine(line);
    }
    /// <inheritdoc cref="ModItem.PostDrawTooltipLine"/>
    public virtual void PostDrawTooltipLine(DrawableTooltipLine line) { }

    // ModifyTooltips
    public sealed override void Detour_ModifyTooltips(Orig_ModifyTooltips orig, TSource self, List<TooltipLine> tooltips)
    {
        Self = self;
        ModifyTooltips(tooltips);
    }
    /// <inheritdoc cref="ModItem.ModifyTooltips"/>
    public virtual void ModifyTooltips(List<TooltipLine> tooltips) { }
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

        foreach (ICATweak caTweak in TOReflectionUtils.GetTypeInstancesDerivedFrom<ICATweak>(CASharedData.Assembly))
            caTweak.RegisterTweak();
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
public sealed class CalamityGlobalNPCBehaviorDetour : CalamityGlobalNPCDetour
{
    public override bool Detour_PreAI(Orig_PreAI orig, CalamityGlobalNPC self, NPC npc)
    {
        if (npc.TryGetBehavior(out CASingleNPCBehavior npcBehavior, nameof(CASingleNPCBehavior.PreAI)) &&
            !npcBehavior.AllowCalamityLogic(CalamityLogicType_NPCBehavior.PreAI))
            return true;

        return orig(self, npc);
    }

    public override Color? Detour_GetAlpha(Orig_GetAlpha orig, CalamityGlobalNPC self, NPC npc, Color drawColor)
    {
        if (npc.TryGetBehavior(out CASingleNPCBehavior npcBehavior, nameof(CASingleNPCBehavior.GetAlpha)) &&
            !npcBehavior.AllowCalamityLogic(CalamityLogicType_NPCBehavior.GetAlpha))
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
        if (npc.TryGetBehavior(out CASingleNPCBehavior npcBehavior, nameof(CASingleNPCBehavior.PreAI)) &&
            !npcBehavior.AllowCalamityLogic(CalamityLogicType_NPCBehavior.VanillaOverrideAI))
            return true;

        return orig(self, npc);
    }
}

public sealed class CalamityGlobalProjectileBehaviorDetour : CalamityGlobalProjectileDetour
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