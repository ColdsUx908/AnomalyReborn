using MonoMod.Utils;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.UI;
using Terraria.GameInput;

namespace Transoceanic.Framework.Helpers.AbstractionHandlers;

#region Set
public class SimpleEntityBehaviorSet<TEntity, TBehavior>
    where TEntity : Entity
    where TBehavior : EntityBehavior<TEntity>
{
    public readonly ref struct BehaviorProcesser
    {
        public static BehaviorProcesser Empty => new([]);

        private readonly Span<TBehavior> _span;

        public BehaviorProcesser(Span<TBehavior> span) => _span = span;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Enumerator GetEnumerator() => new(this);

        public ref struct Enumerator
        {
            private Span<TBehavior>.Enumerator _enumerator;
            private TBehavior _current;
            public readonly TBehavior Current { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => _current; }

            public Enumerator(BehaviorProcesser processer) => _enumerator = processer._span.GetEnumerator();

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool MoveNext()
            {
                while (_enumerator.MoveNext())
                {
                    TBehavior temp = _enumerator.Current;
                    if (temp.ShouldProcess)
                    {
                        _current = temp;
                        return true;
                    }
                }
                return false;
            }
        }
    }

    public readonly ref struct ConnectedBehaviorProcesser //这个直接设置_entity字段的行为很丑陋，但是性能要求不得不这样做，下同
    {
        public static ConnectedBehaviorProcesser Empty => new([], null);

        private readonly Span<TBehavior> _span;
        private readonly TEntity _entity;

        public ConnectedBehaviorProcesser(Span<TBehavior> span, TEntity entity)
        {
            _span = span;
            _entity = entity;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Enumerator GetEnumerator() => new(this);

        public ref struct Enumerator
        {
            private Span<TBehavior>.Enumerator _enumerator;
            private readonly TEntity _entity;
            private TBehavior _current;
            public readonly TBehavior Current { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => _current; }

            public Enumerator(ConnectedBehaviorProcesser processer)
            {
                _enumerator = processer._span.GetEnumerator();
                _entity = processer._entity;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool MoveNext()
            {
                while (_enumerator.MoveNext())
                {
                    TBehavior temp = _enumerator.Current;
                    temp._entity = _entity;
                    if (temp.ShouldProcess)
                    {
                        _current = temp;
                        return true;
                    }
                }
                return false;
            }
        }
    }

    public readonly ref struct TypedBehaviorProcesser<T> where T : TBehavior
    {
        public static TypedBehaviorProcesser<T> Empty => new([]);

        private readonly Span<TBehavior> _span;

        public TypedBehaviorProcesser(Span<TBehavior> span) => _span = span;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Enumerator GetEnumerator() => new(this);

        public ref struct Enumerator
        {
            private Span<TBehavior>.Enumerator _enumerator;
            private T _current;
            public readonly T Current { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => _current; }

            public Enumerator(TypedBehaviorProcesser<T> processer) => _enumerator = processer._span.GetEnumerator();

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool MoveNext()
            {
                while (_enumerator.MoveNext())
                {
                    T temp = _enumerator.Current as T;
                    if (temp?.ShouldProcess == true)
                    {
                        _current = temp;
                        return true;
                    }
                }
                return false;
            }
        }
    }

    public readonly ref struct TypedConnectedBehaviorProcesser<T> where T : TBehavior
    {
        public static TypedConnectedBehaviorProcesser<T> Empty => new([], null);

        private readonly Span<TBehavior> _span;
        private readonly TEntity _entity;

        public TypedConnectedBehaviorProcesser(Span<TBehavior> span, TEntity entity)
        {
            _span = span;
            _entity = entity;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Enumerator GetEnumerator() => new(this);

        public ref struct Enumerator
        {
            private Span<TBehavior>.Enumerator _enumerator;
            private readonly TEntity _entity;
            private T _current;
            public readonly T Current { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => _current; }

            public Enumerator(TypedConnectedBehaviorProcesser<T> processer)
            {
                _enumerator = processer._span.GetEnumerator();
                _entity = processer._entity;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool MoveNext()
            {
                while (_enumerator.MoveNext())
                {
                    T temp = _enumerator.Current as T;
                    if (temp is not null)
                    {
                        temp._entity = _entity;
                        if (temp.ShouldProcess)
                        {
                            _current = temp;
                            return true;
                        }
                    }
                }
                return false;
            }
        }
    }

    protected internal readonly Dictionary<string, TBehavior[]> _data = [];

    public void Clear() => _data.Clear();

    public void FillSet(IEnumerable<TBehavior> behaviors) => Initialize(behaviors);

    public void FillSet() => Initialize(TOReflectionUtils.GetTypeInstancesDerivedFrom<TBehavior>());

    public void FillSet(Assembly assemblyToSearch) => Initialize(TOReflectionUtils.GetTypeInstancesDerivedFrom<TBehavior>(assemblyToSearch));

    public void FillSet<T>() where T : Mod => Initialize(TOReflectionUtils.GetTypeInstancesDerivedFrom<TBehavior>().Where(b => b.Mod is T));

    public BehaviorProcesser Enumerate([CallerMemberName] string methodName = null) =>
        _data.TryGetValue(methodName, out TBehavior[] behaviors) ? new(behaviors) : BehaviorProcesser.Empty;

    public ConnectedBehaviorProcesser Enumerate(TEntity entity, [CallerMemberName] string methodName = null) =>
        _data.TryGetValue(methodName, out TBehavior[] behaviors) ? new(behaviors, entity) : ConnectedBehaviorProcesser.Empty;

    public TypedBehaviorProcesser<T> Enumerate<T>([CallerMemberName] string methodName = null) where T : TBehavior =>
        _data.TryGetValue(methodName, out TBehavior[] behaviors) ? new(behaviors) : TypedBehaviorProcesser<T>.Empty;

    public TypedConnectedBehaviorProcesser<T> Enumerate<T>(TEntity entity, [CallerMemberName] string methodName = null) where T : TBehavior =>
        _data.TryGetValue(methodName, out TBehavior[] behaviors) ? new(behaviors, entity) : TypedConnectedBehaviorProcesser<T>.Empty;

    /// <summary>
    /// 按照 <see cref="EntityBehavior{TEntity}.Priority"/> 降序寻找通过 <see cref="SingleEntityBehavior{TEntity}.ShouldProcess"/> 检测且实现了指定方法的Override实例。
    /// </summary>
    public TBehavior GetFirstOrDefault(TEntity entity, [CallerMemberName] string methodName = null)
    {
        if (_data.TryGetValue(methodName, out TBehavior[] behaviors))
        {
            for (int i = 0; i < behaviors.Length; i++)
            {
                TBehavior behavior = behaviors[i];
                behavior._entity = entity;
                if (behavior.ShouldProcess)
                    return behavior;
            }
        }
        return null;
    }

    internal void Initialize(IEnumerable<TBehavior> behaviors)
    {
        Dictionary<string, List<TBehavior>> tempData = [];
        foreach ((TBehavior behavior, HashSet<string> behaviorMethods) in
            behaviors.Select(b =>
            {
                Type type = b.GetType();
                return (b, (type.HasAttribute<CriticalBehaviorAttribute>() ? type.GetMethodNamesExceptObject(TOReflectionUtils.UniversalBindingFlags) : type.GetOverrideMethodNames(TOReflectionUtils.UniversalBindingFlags)).ToHashSet());
            }))
        {
            foreach (string method in behaviorMethods)
                tempData.AddBetter(method, behavior);
        }
        _data.AddRange(tempData.ToDictionary(kv => kv.Key, kv => kv.Value.Distinct().OrderByDescending(b => b.Priority).ToArray()));
    }
}

public class GeneralEntityBehaviorSet<TEntity, TBehavior> : SimpleEntityBehaviorSet<TEntity, TBehavior>
    where TEntity : Entity
    where TBehavior : GeneralEntityBehavior<TEntity>;

public class GlobalEntityBehaviorSet<TEntity, TBehavior> : GeneralEntityBehaviorSet<TEntity, TBehavior>
    where TEntity : Entity
    where TBehavior : GeneralEntityBehavior<TEntity>;

public class SingleEntityBehaviorSet<TEntity, TBehavior>
    where TEntity : Entity
    where TBehavior : SingleEntityBehavior<TEntity>
{
    protected internal readonly Dictionary<int, SimpleEntityBehaviorSet<TEntity, TBehavior>> _data = [];

    /// <summary>
    /// 按照 <see cref="EntityBehavior{TEntity}.Priority"/> 降序寻找通过 <see cref="SingleEntityBehavior{TEntity}.ShouldProcess"/> 检测且实现了指定方法的Override实例。
    /// </summary>
    public bool TryGetBehavior(TEntity entity, string methodName, [NotNullWhen(true)] out TBehavior behavior) =>
        (behavior = entity is not null && _data.TryGetValue(entity.EntityType, out SimpleEntityBehaviorSet<TEntity, TBehavior> set) ? set.GetFirstOrDefault(entity, methodName) : null) is not null;

    public void FillSet() => Initialize(TOReflectionUtils.GetTypeInstancesDerivedFrom<TBehavior>());

    public void FillSet<T>() where T : Mod => Initialize(TOReflectionUtils.GetTypeInstancesDerivedFrom<TBehavior>().Where(b => b.Mod is T));

    public void FillSet(Assembly assemblyToSearch) => Initialize(TOReflectionUtils.GetTypeInstancesDerivedFrom<TBehavior>(assemblyToSearch));

    internal void Initialize(IEnumerable<TBehavior> behaviors)
    {
        foreach (IGrouping<int, TBehavior> group in behaviors.GroupBy(b => b.ApplyingType))
        {
            _data[group.Key] = new();
            _data[group.Key].Initialize(group);
        }
    }

    public void Clear()
    {
        foreach (SimpleEntityBehaviorSet<TEntity, TBehavior> set in _data.Values)
            set.Clear();
        _data.Clear();
    }
}
#endregion Set

#region General Behavior Handler
public sealed class PlayerBehaviorHandler : ModPlayer
{
    public static readonly GeneralEntityBehaviorSet<Player, PlayerBehavior> BehaviorSet = new();

    public override void SetStaticDefaults()
    {
        foreach (PlayerBehavior behavior in BehaviorSet.Enumerate())
            behavior.SetStaticDefaults();
    }

    public override void Initialize()
    {
        foreach (PlayerBehavior behavior in BehaviorSet.Enumerate(Player))
            behavior.Initialize();
    }

    public override void ResetEffects()
    {
        foreach (PlayerBehavior behavior in BehaviorSet.Enumerate(Player))
            behavior.ResetEffects();
    }

    public override void ResetInfoAccessories()
    {
        foreach (PlayerBehavior behavior in BehaviorSet.Enumerate(Player))
            behavior.ResetInfoAccessories();
    }

    public override void RefreshInfoAccessoriesFromTeamPlayers(Player otherPlayer)
    {
        foreach (PlayerBehavior behavior in BehaviorSet.Enumerate(Player))
            behavior.RefreshInfoAccessoriesFromTeamPlayers(otherPlayer);
    }

    public override void ModifyMaxStats(out StatModifier health, out StatModifier mana)
    {
        health = StatModifier.Default;
        mana = StatModifier.Default;

        foreach (PlayerBehavior behavior in BehaviorSet.Enumerate(Player))
        {
            behavior.ModifyMaxStats(out StatModifier newHealth, out StatModifier newMana);
            health = health.CombineWith(newHealth);
            mana = mana.CombineWith(newMana);
        }
    }

    public override void UpdateDead()
    {
        foreach (PlayerBehavior behavior in BehaviorSet.Enumerate(Player))
            behavior.UpdateDead();
    }

    public override void PreSaveCustomData()
    {
        foreach (PlayerBehavior behavior in BehaviorSet.Enumerate(Player))
            behavior.PreSaveCustomData();
    }

    public override void SaveData(TagCompound tag)
    {
        foreach (PlayerBehavior behavior in BehaviorSet.Enumerate(Player))
            behavior.SaveData(tag);
    }

    public override void LoadData(TagCompound tag)
    {
        foreach (PlayerBehavior behavior in BehaviorSet.Enumerate(Player))
            behavior.LoadData(tag);
    }

    public override void PreSavePlayer()
    {
        foreach (PlayerBehavior behavior in BehaviorSet.Enumerate(Player))
            behavior.PreSavePlayer();
    }

    public override void PostSavePlayer()
    {
        foreach (PlayerBehavior behavior in BehaviorSet.Enumerate(Player))
            behavior.PostSavePlayer();
    }

    public override void CopyClientState(ModPlayer targetCopy)
    {
        if (TOSharedData.SyncEnabled)
        {
            //foreach (PlayerBehavior behavior in BehaviorSet.GetBehaviors(Player))
            //    behavior.CopyClientState(targetCopy);
        }
    }

    public override void SyncPlayer(int toWho, int fromWho, bool newPlayer)
    {
        if (TOSharedData.SyncEnabled)
        {
            //foreach (PlayerBehavior behavior in BehaviorSet.GetBehaviors(Player))
            //    behavior.SyncPlayer(toWho, fromWho, newPlayer);
        }
    }

    public override void SendClientChanges(ModPlayer clientPlayer)
    {
        if (TOSharedData.SyncEnabled)
        {
            //foreach (PlayerBehavior behavior in BehaviorSet.GetBehaviors(Player))
            //    behavior.SendClientChanges(clientPlayer);
        }
    }

    public override void UpdateBadLifeRegen()
    {
        foreach (PlayerBehavior behavior in BehaviorSet.Enumerate(Player))
            behavior.UpdateBadLifeRegen();
    }

    public override void UpdateLifeRegen()
    {
        foreach (PlayerBehavior behavior in BehaviorSet.Enumerate(Player))
            behavior.UpdateLifeRegen();
    }

    public override void NaturalLifeRegen(ref float regen)
    {
        foreach (PlayerBehavior behavior in BehaviorSet.Enumerate(Player))
            behavior.NaturalLifeRegen(ref regen);
    }

    public override void UpdateAutopause()
    {
        foreach (PlayerBehavior behavior in BehaviorSet.Enumerate(Player))
            behavior.UpdateAutopause();
    }

    public override void PreUpdate()
    {
        foreach (PlayerBehavior behavior in BehaviorSet.Enumerate(Player))
            behavior.PreUpdate();
    }

    public override void ProcessTriggers(TriggersSet triggersSet)
    {
        foreach (PlayerBehavior behavior in BehaviorSet.Enumerate(Player))
            behavior.ProcessTriggers(triggersSet);
    }

    public override void ArmorSetBonusActivated()
    {
        foreach (PlayerBehavior behavior in BehaviorSet.Enumerate(Player))
            behavior.ArmorSetBonusActivated();
    }

    public override void ArmorSetBonusHeld(int holdTime)
    {
        foreach (PlayerBehavior behavior in BehaviorSet.Enumerate(Player))
            behavior.ArmorSetBonusHeld(holdTime);
    }

    public override void SetControls()
    {
        foreach (PlayerBehavior behavior in BehaviorSet.Enumerate(Player))
            behavior.SetControls();
    }

    public override void PreUpdateBuffs()
    {
        foreach (PlayerBehavior behavior in BehaviorSet.Enumerate(Player))
            behavior.PreUpdateBuffs();
    }

    public override void PostUpdateBuffs()
    {
        foreach (PlayerBehavior behavior in BehaviorSet.Enumerate(Player))
            behavior.PostUpdateBuffs();
    }

    public override void UpdateEquips()
    {
        foreach (PlayerBehavior behavior in BehaviorSet.Enumerate(Player))
            behavior.UpdateEquips();
    }

    public override void PostUpdateEquips()
    {
        foreach (PlayerBehavior behavior in BehaviorSet.Enumerate(Player))
            behavior.PostUpdateEquips();
    }

    public override void UpdateVisibleAccessories()
    {
        foreach (PlayerBehavior behavior in BehaviorSet.Enumerate(Player))
            behavior.UpdateVisibleAccessories();
    }

    public override void UpdateVisibleVanityAccessories()
    {
        foreach (PlayerBehavior behavior in BehaviorSet.Enumerate(Player))
            behavior.UpdateVisibleVanityAccessories();
    }

    public override void UpdateDyes()
    {
        foreach (PlayerBehavior behavior in BehaviorSet.Enumerate(Player))
            behavior.UpdateDyes();
    }

    public override void PostUpdateMiscEffects()
    {
        foreach (PlayerBehavior behavior in BehaviorSet.Enumerate(Player))
            behavior.PostUpdateMiscEffects();
    }

    public override void PostUpdateRunSpeeds()
    {
        foreach (PlayerBehavior behavior in BehaviorSet.Enumerate(Player))
            behavior.PostUpdateRunSpeeds();
    }

    public override void PreUpdateMovement()
    {
        foreach (PlayerBehavior behavior in BehaviorSet.Enumerate(Player))
            behavior.PreUpdateMovement();
    }

    public override void PostUpdate()
    {
        foreach (PlayerBehavior behavior in BehaviorSet.Enumerate(Player))
            behavior.PostUpdate();
    }

    public override void ModifyExtraJumpDurationMultiplier(ExtraJump jump, ref float duration)
    {
        foreach (PlayerBehavior behavior in BehaviorSet.Enumerate(Player))
            behavior.ModifyExtraJumpDurationMultiplier(jump, ref duration);
    }

    public override bool CanStartExtraJump(ExtraJump jump)
    {
        bool result = true;
        foreach (PlayerBehavior behavior in BehaviorSet.Enumerate(Player))
            result &= behavior.CanStartExtraJump(jump);
        return result;
    }

    public override void OnExtraJumpStarted(ExtraJump jump, ref bool playSound)
    {
        foreach (PlayerBehavior behavior in BehaviorSet.Enumerate(Player))
            behavior.OnExtraJumpStarted(jump, ref playSound);
    }

    public override void OnExtraJumpEnded(ExtraJump jump)
    {
        foreach (PlayerBehavior behavior in BehaviorSet.Enumerate(Player))
            behavior.OnExtraJumpEnded(jump);
    }

    public override void OnExtraJumpRefreshed(ExtraJump jump)
    {
        foreach (PlayerBehavior behavior in BehaviorSet.Enumerate(Player))
            behavior.OnExtraJumpRefreshed(jump);
    }

    public override void ExtraJumpVisuals(ExtraJump jump)
    {
        foreach (PlayerBehavior behavior in BehaviorSet.Enumerate(Player))
            behavior.ExtraJumpVisuals(jump);
    }

    public override bool CanShowExtraJumpVisuals(ExtraJump jump)
    {
        bool result = true;
        foreach (PlayerBehavior behavior in BehaviorSet.Enumerate(Player))
            result &= behavior.CanShowExtraJumpVisuals(jump);
        return result;
    }

    public override void OnExtraJumpCleared(ExtraJump jump)
    {
        foreach (PlayerBehavior behavior in BehaviorSet.Enumerate(Player))
            behavior.OnExtraJumpCleared(jump);
    }

    public override void FrameEffects()
    {
        foreach (PlayerBehavior behavior in BehaviorSet.Enumerate(Player))
            behavior.FrameEffects();
    }

    public override bool ImmuneTo(PlayerDeathReason damageSource, int cooldownCounter, bool dodgeable)
    {
        bool result = false;
        foreach (PlayerBehavior behavior in BehaviorSet.Enumerate(Player))
            result |= behavior.ImmuneTo(damageSource, cooldownCounter, dodgeable);
        return result;
    }

    public override bool FreeDodge(Player.HurtInfo info)
    {
        bool result = false;
        foreach (PlayerBehavior behavior in BehaviorSet.Enumerate(Player))
            result |= behavior.FreeDodge(info);
        return result;
    }

    public override bool ConsumableDodge(Player.HurtInfo info)
    {
        bool result = false;
        foreach (PlayerBehavior behavior in BehaviorSet.Enumerate(Player))
            result |= behavior.ConsumableDodge(info);
        return result;
    }

    public override void ModifyHurt(ref Player.HurtModifiers modifiers)
    {
        foreach (PlayerBehavior behavior in BehaviorSet.Enumerate(Player))
            behavior.ModifyHurt(ref modifiers);
    }

    public override void OnHurt(Player.HurtInfo info)
    {
        foreach (PlayerBehavior behavior in BehaviorSet.Enumerate(Player))
            behavior.OnHurt(info);
    }

    public override void PostHurt(Player.HurtInfo info)
    {
        foreach (PlayerBehavior behavior in BehaviorSet.Enumerate(Player))
            behavior.PostHurt(info);
    }

    public override bool PreKill(double damage, int hitDirection, bool pvp, ref bool playSound, ref bool genDust, ref PlayerDeathReason damageSource)
    {
        bool result = true;
        foreach (PlayerBehavior behavior in BehaviorSet.Enumerate(Player))
            result &= behavior.PreKill(damage, hitDirection, pvp, ref playSound, ref genDust, ref damageSource);
        return result;
    }

    public override void Kill(double damage, int hitDirection, bool pvp, PlayerDeathReason damageSource)
    {
        foreach (PlayerBehavior behavior in BehaviorSet.Enumerate(Player))
            behavior.Kill(damage, hitDirection, pvp, damageSource);
    }

    public override bool PreModifyLuck(ref float luck)
    {
        bool result = true;
        foreach (PlayerBehavior behavior in BehaviorSet.Enumerate(Player))
            result &= behavior.PreModifyLuck(ref luck);
        return result;
    }

    public override void ModifyLuck(ref float luck)
    {
        foreach (PlayerBehavior behavior in BehaviorSet.Enumerate(Player))
            behavior.ModifyLuck(ref luck);
    }

    public override bool PreItemCheck()
    {
        bool result = true;
        foreach (PlayerBehavior behavior in BehaviorSet.Enumerate(Player))
            result &= behavior.PreItemCheck();
        return result;
    }

    public override void PostItemCheck()
    {
        foreach (PlayerBehavior behavior in BehaviorSet.Enumerate(Player))
            behavior.PostItemCheck();
    }

    public override float UseTimeMultiplier(Item item)
    {
        float result = 1f;
        foreach (PlayerBehavior behavior in BehaviorSet.Enumerate(Player))
            result *= behavior.UseTimeMultiplier(item);
        return result;
    }

    public override float UseAnimationMultiplier(Item item)
    {
        float result = 1f;
        foreach (PlayerBehavior behavior in BehaviorSet.Enumerate(Player))
            result *= behavior.UseAnimationMultiplier(item);
        return result;
    }

    public override float UseSpeedMultiplier(Item item)
    {
        float result = 1f;
        foreach (PlayerBehavior behavior in BehaviorSet.Enumerate(Player))
            result *= behavior.UseSpeedMultiplier(item);
        return result;
    }

    public override void GetHealLife(Item item, bool quickHeal, ref int healValue)
    {
        foreach (PlayerBehavior behavior in BehaviorSet.Enumerate(Player))
            behavior.GetHealLife(item, quickHeal, ref healValue);
    }

    public override void GetHealMana(Item item, bool quickHeal, ref int healValue)
    {
        foreach (PlayerBehavior behavior in BehaviorSet.Enumerate(Player))
            behavior.GetHealMana(item, quickHeal, ref healValue);
    }

    public override void ModifyManaCost(Item item, ref float reduce, ref float mult)
    {
        foreach (PlayerBehavior behavior in BehaviorSet.Enumerate(Player))
            behavior.ModifyManaCost(item, ref reduce, ref mult);
    }

    public override void OnMissingMana(Item item, int neededMana)
    {
        foreach (PlayerBehavior behavior in BehaviorSet.Enumerate(Player))
            behavior.OnMissingMana(item, neededMana);
    }

    public override void OnConsumeMana(Item item, int manaConsumed)
    {
        foreach (PlayerBehavior behavior in BehaviorSet.Enumerate(Player))
            behavior.OnConsumeMana(item, manaConsumed);
    }

    public override void ModifyWeaponDamage(Item item, ref StatModifier damage)
    {
        foreach (PlayerBehavior behavior in BehaviorSet.Enumerate(Player))
            behavior.ModifyWeaponDamage(item, ref damage);
    }

    public override void ModifyWeaponKnockback(Item item, ref StatModifier knockback)
    {
        foreach (PlayerBehavior behavior in BehaviorSet.Enumerate(Player))
            behavior.ModifyWeaponKnockback(item, ref knockback);
    }

    public override void ModifyWeaponCrit(Item item, ref float crit)
    {
        foreach (PlayerBehavior behavior in BehaviorSet.Enumerate(Player))
            behavior.ModifyWeaponCrit(item, ref crit);
    }

    public override bool CanConsumeAmmo(Item weapon, Item ammo)
    {
        bool result = true;
        foreach (PlayerBehavior behavior in BehaviorSet.Enumerate(Player))
            result &= behavior.CanConsumeAmmo(weapon, ammo);
        return result;
    }

    public override void OnConsumeAmmo(Item weapon, Item ammo)
    {
        foreach (PlayerBehavior behavior in BehaviorSet.Enumerate(Player))
            behavior.OnConsumeAmmo(weapon, ammo);
    }

    public override bool CanShoot(Item item)
    {
        bool result = true;
        foreach (PlayerBehavior behavior in BehaviorSet.Enumerate(Player))
            result &= behavior.CanShoot(item);
        return result;
    }

    public override void ModifyShootStats(Item item, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
    {
        foreach (PlayerBehavior behavior in BehaviorSet.Enumerate(Player))
            behavior.ModifyShootStats(item, ref position, ref velocity, ref type, ref damage, ref knockback);
    }

    public override bool Shoot(Item item, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
    {
        bool result = true;
        foreach (PlayerBehavior behavior in BehaviorSet.Enumerate(Player))
            result &= behavior.Shoot(item, source, position, velocity, type, damage, knockback);
        return result;
    }

    public override void MeleeEffects(Item item, Rectangle hitbox)
    {
        foreach (PlayerBehavior behavior in BehaviorSet.Enumerate(Player))
            behavior.MeleeEffects(item, hitbox);
    }

    public override void EmitEnchantmentVisualsAt(Projectile projectile, Vector2 boxPosition, int boxWidth, int boxHeight)
    {
        foreach (PlayerBehavior behavior in BehaviorSet.Enumerate(Player))
            behavior.EmitEnchantmentVisualsAt(projectile, boxPosition, boxWidth, boxHeight);
    }

    public override bool? CanCatchNPC(NPC target, Item item)
    {
        foreach (PlayerBehavior behavior in BehaviorSet.Enumerate(Player))
        {
            bool? result = behavior.CanCatchNPC(target, item);
            if (result is not null)
                return result;
        }
        return null;
    }

    public override void OnCatchNPC(NPC npc, Item item, bool failed)
    {
        foreach (PlayerBehavior behavior in BehaviorSet.Enumerate(Player))
            behavior.OnCatchNPC(npc, item, failed);
    }

    public override void ModifyItemScale(Item item, ref float scale)
    {
        foreach (PlayerBehavior behavior in BehaviorSet.Enumerate(Player))
            behavior.ModifyItemScale(item, ref scale);
    }

    public override void OnHitAnything(float x, float y, Entity victim)
    {
        foreach (PlayerBehavior behavior in BehaviorSet.Enumerate(Player))
            behavior.OnHitAnything(x, y, victim);
    }

    /*
    public override bool CanHitNPC(NPC target)
    {
        bool result = true;
        foreach (PlayerBehavior behavior in BehaviorSet.GetBehaviors(Player))
            result &= behavior.CanHitNPC(target);
        return result;
    }

    public override bool? CanMeleeAttackCollideWithNPC(Item item, Rectangle meleeAttackHitbox, NPC target)
    {
        foreach (PlayerBehavior behavior in BehaviorSet.GetBehaviors(Player))
        {
            bool? result = behavior.CanMeleeAttackCollideWithNPC(item, meleeAttackHitbox, target);
            if (result is not null)
                return result;
        }
        return null;
    }*/

    public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
    {
        foreach (PlayerBehavior behavior in BehaviorSet.Enumerate(Player))
            behavior.ModifyHitNPC(target, ref modifiers);
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        foreach (PlayerBehavior behavior in BehaviorSet.Enumerate(Player))
            behavior.OnHitNPC(target, hit, damageDone);
    }

    /*
    public override bool? CanHitNPCWithItem(Item item, NPC target)
    {
        foreach (PlayerBehavior behavior in BehaviorSet.GetBehaviors(Player))
        {
            bool? result = behavior.CanHitNPCWithItem(item, target);
            if (result is not null)
                return result;
        }
        return null;
    }*/

    public override void ModifyHitNPCWithItem(Item item, NPC target, ref NPC.HitModifiers modifiers)
    {
        foreach (PlayerBehavior behavior in BehaviorSet.Enumerate(Player))
            behavior.ModifyHitNPCWithItem(item, target, ref modifiers);
    }

    public override void OnHitNPCWithItem(Item item, NPC target, NPC.HitInfo hit, int damageDone)
    {
        foreach (PlayerBehavior behavior in BehaviorSet.Enumerate(Player))
            behavior.OnHitNPCWithItem(item, target, hit, damageDone);
    }

    /*
    public override bool? CanHitNPCWithProj(Projectile proj, NPC target)
    {
        foreach (PlayerBehavior behavior in BehaviorSet.GetBehaviors(Player))
        {
            bool? result = behavior.CanHitNPCWithProj(proj, target);
            if (result is not null)
                return result;
        }
        return null;
    }*/

    public override void ModifyHitNPCWithProj(Projectile proj, NPC target, ref NPC.HitModifiers modifiers)
    {
        foreach (PlayerBehavior behavior in BehaviorSet.Enumerate(Player))
            behavior.ModifyHitNPCWithProj(proj, target, ref modifiers);
    }

    public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
    {
        foreach (PlayerBehavior behavior in BehaviorSet.Enumerate(Player))
            behavior.OnHitNPCWithProj(proj, target, hit, damageDone);
    }

    /*
    public override bool CanHitPvp(Item item, Player target)
    {
        bool result = true;
        foreach (PlayerBehavior behavior in BehaviorSet.GetBehaviors(Player))
            result &= behavior.CanHitPvp(item, target);
        return result;
    }

    public override bool CanHitPvpWithProj(Projectile proj, Player target)
    {
        bool result = true;
        foreach (PlayerBehavior behavior in BehaviorSet.GetBehaviors(Player))
            result &= behavior.CanHitPvpWithProj(proj, target);
        return result;
    }

    public override bool CanBeHitByNPC(NPC npc, ref int cooldownSlot)
    {
        bool result = true;
        foreach (PlayerBehavior behavior in BehaviorSet.GetBehaviors(Player))
            result &= behavior.CanBeHitByNPC(npc, ref cooldownSlot);
        return result;
    }*/

    public override void ModifyHitByNPC(NPC npc, ref Player.HurtModifiers modifiers)
    {
        foreach (PlayerBehavior behavior in BehaviorSet.Enumerate(Player))
            behavior.ModifyHitByNPC(npc, ref modifiers);
    }

    public override void OnHitByNPC(NPC npc, Player.HurtInfo hurtInfo)
    {
        foreach (PlayerBehavior behavior in BehaviorSet.Enumerate(Player))
            behavior.OnHitByNPC(npc, hurtInfo);
    }

    /*
    public override bool CanBeHitByProjectile(Projectile proj)
    {
        bool result = true;
        foreach (PlayerBehavior behavior in BehaviorSet.GetBehaviors(Player))
            result &= behavior.CanBeHitByProjectile(proj);
        return result;
    }*/

    public override void ModifyHitByProjectile(Projectile proj, ref Player.HurtModifiers modifiers)
    {
        foreach (PlayerBehavior behavior in BehaviorSet.Enumerate(Player))
            behavior.ModifyHitByProjectile(proj, ref modifiers);
    }

    public override void OnHitByProjectile(Projectile proj, Player.HurtInfo hurtInfo)
    {
        foreach (PlayerBehavior behavior in BehaviorSet.Enumerate(Player))
            behavior.OnHitByProjectile(proj, hurtInfo);
    }

    public override void ModifyFishingAttempt(ref FishingAttempt attempt)
    {
        foreach (PlayerBehavior behavior in BehaviorSet.Enumerate(Player))
            behavior.ModifyFishingAttempt(ref attempt);
    }

    public override void CatchFish(FishingAttempt attempt, ref int itemDrop, ref int npcSpawn, ref AdvancedPopupRequest sonar, ref Vector2 sonarPosition)
    {
        foreach (PlayerBehavior behavior in BehaviorSet.Enumerate(Player))
            behavior.CatchFish(attempt, ref itemDrop, ref npcSpawn, ref sonar, ref sonarPosition);
    }

    public override void ModifyCaughtFish(Item fish)
    {
        foreach (PlayerBehavior behavior in BehaviorSet.Enumerate(Player))
            behavior.ModifyCaughtFish(fish);
    }

    public override bool? CanConsumeBait(Item bait)
    {
        foreach (PlayerBehavior behavior in BehaviorSet.Enumerate(Player))
        {
            bool? result = behavior.CanConsumeBait(bait);
            if (result is not null)
                return result;
        }
        return null;
    }

    public override void GetFishingLevel(Item fishingRod, Item bait, ref float fishingLevel)
    {
        foreach (PlayerBehavior behavior in BehaviorSet.Enumerate(Player))
            behavior.GetFishingLevel(fishingRod, bait, ref fishingLevel);
    }

    public override void AnglerQuestReward(float rareMultiplier, List<Item> rewardItems)
    {
        foreach (PlayerBehavior behavior in BehaviorSet.Enumerate(Player))
            behavior.AnglerQuestReward(rareMultiplier, rewardItems);
    }

    public override void GetDyeTraderReward(List<int> rewardPool)
    {
        foreach (PlayerBehavior behavior in BehaviorSet.Enumerate(Player))
            behavior.GetDyeTraderReward(rewardPool);
    }

    public override void DrawEffects(PlayerDrawSet drawInfo, ref float r, ref float g, ref float b, ref float a, ref bool fullBright)
    {
        foreach (PlayerBehavior behavior in BehaviorSet.Enumerate(Player))
            behavior.DrawEffects(drawInfo, ref r, ref g, ref b, ref a, ref fullBright);
    }

    public override void ModifyDrawInfo(ref PlayerDrawSet drawInfo)
    {
        foreach (PlayerBehavior behavior in BehaviorSet.Enumerate(Player))
            behavior.ModifyDrawInfo(ref drawInfo);
    }

    public override void ModifyDrawLayerOrdering(IDictionary<PlayerDrawLayer, PlayerDrawLayer.Position> positions)
    {
        foreach (PlayerBehavior behavior in BehaviorSet.Enumerate(Player))
            behavior.ModifyDrawLayerOrdering(positions);
    }

    public override void HideDrawLayers(PlayerDrawSet drawInfo)
    {
        foreach (PlayerBehavior behavior in BehaviorSet.Enumerate(Player))
            behavior.HideDrawLayers(drawInfo);
    }

    public override void ModifyScreenPosition()
    {
        foreach (PlayerBehavior behavior in BehaviorSet.Enumerate(Player))
            behavior.ModifyScreenPosition();
    }

    public override void ModifyZoom(ref float zoom)
    {
        foreach (PlayerBehavior behavior in BehaviorSet.Enumerate(Player))
            behavior.ModifyZoom(ref zoom);
    }

    public override void PlayerConnect()
    {
        foreach (PlayerBehavior behavior in BehaviorSet.Enumerate(Player))
            behavior.PlayerConnect();
    }

    public override void PlayerDisconnect()
    {
        foreach (PlayerBehavior behavior in BehaviorSet.Enumerate(Player))
            behavior.PlayerDisconnect();
    }

    public override void OnEnterWorld()
    {
        foreach (PlayerBehavior behavior in BehaviorSet.Enumerate(Player))
            behavior.OnEnterWorld();
    }

    public override void OnRespawn()
    {
        foreach (PlayerBehavior behavior in BehaviorSet.Enumerate(Player))
            behavior.OnRespawn();
    }

    public override bool ShiftClickSlot(Item[] inventory, int context, int slot)
    {
        bool result = false;
        foreach (PlayerBehavior behavior in BehaviorSet.Enumerate(Player))
            result |= behavior.ShiftClickSlot(inventory, context, slot);
        return result;
    }

    public override bool HoverSlot(Item[] inventory, int context, int slot)
    {
        bool result = false;
        foreach (PlayerBehavior behavior in BehaviorSet.Enumerate(Player))
            result |= behavior.HoverSlot(inventory, context, slot);
        return result;
    }

    public override void PostSellItem(NPC vendor, Item[] shopInventory, Item item)
    {
        foreach (PlayerBehavior behavior in BehaviorSet.Enumerate(Player))
            behavior.PostSellItem(vendor, shopInventory, item);
    }

    public override bool CanSellItem(NPC vendor, Item[] shopInventory, Item item)
    {
        bool result = true;
        foreach (PlayerBehavior behavior in BehaviorSet.Enumerate(Player))
            result &= behavior.CanSellItem(vendor, shopInventory, item);
        return result;
    }

    public override void PostBuyItem(NPC vendor, Item[] shopInventory, Item item)
    {
        foreach (PlayerBehavior behavior in BehaviorSet.Enumerate(Player))
            behavior.PostBuyItem(vendor, shopInventory, item);
    }

    public override bool CanBuyItem(NPC vendor, Item[] shopInventory, Item item)
    {
        bool result = true;
        foreach (PlayerBehavior behavior in BehaviorSet.Enumerate(Player))
            result &= behavior.CanBuyItem(vendor, shopInventory, item);
        return result;
    }

    public override bool CanUseItem(Item item)
    {
        bool result = true;
        foreach (PlayerBehavior behavior in BehaviorSet.Enumerate(Player))
            result &= behavior.CanUseItem(item);
        return result;
    }

    public override bool? CanAutoReuseItem(Item item)
    {
        foreach (PlayerBehavior behavior in BehaviorSet.Enumerate(Player))
        {
            bool? result = behavior.CanAutoReuseItem(item);
            if (result is not null)
                return result;
        }
        return null;
    }

    public override bool ModifyNurseHeal(NPC nurse, ref int health, ref bool removeDebuffs, ref string chatText)
    {
        bool result = true;
        foreach (PlayerBehavior behavior in BehaviorSet.Enumerate(Player))
            result &= behavior.ModifyNurseHeal(nurse, ref health, ref removeDebuffs, ref chatText);
        return result;
    }

    public override void ModifyNursePrice(NPC nurse, int health, bool removeDebuffs, ref int price)
    {
        foreach (PlayerBehavior behavior in BehaviorSet.Enumerate(Player))
            behavior.ModifyNursePrice(nurse, health, removeDebuffs, ref price);
    }

    public override void PostNurseHeal(NPC nurse, int health, bool removeDebuffs, int price)
    {
        foreach (PlayerBehavior behavior in BehaviorSet.Enumerate(Player))
            behavior.PostNurseHeal(nurse, health, removeDebuffs, price);
    }

    public override IEnumerable<Item> AddStartingItems(bool mediumCoreDeath)
    {
        List<Item> allItems = [];
        foreach (PlayerBehavior behavior in BehaviorSet.Enumerate(Player))
        {
            IEnumerable<Item> items = behavior.AddStartingItems(mediumCoreDeath);
            if (items is not null)
                allItems.AddRange(items);
        }
        return allItems;
    }

    public override void ModifyStartingInventory(IReadOnlyDictionary<string, List<Item>> itemsByMod, bool mediumCoreDeath)
    {
        foreach (PlayerBehavior behavior in BehaviorSet.Enumerate(Player))
            behavior.ModifyStartingInventory(itemsByMod, mediumCoreDeath);
    }

    public override IEnumerable<Item> AddMaterialsForCrafting(out ItemConsumedCallback itemConsumedCallback)
    {
        itemConsumedCallback = null;
        List<Item> allItems = [];
        foreach (PlayerBehavior behavior in BehaviorSet.Enumerate(Player))
        {
            IEnumerable<Item> items = behavior.AddMaterialsForCrafting(out ItemConsumedCallback callback);
            if (items is not null)
                allItems.AddRange(items);
            itemConsumedCallback += callback;
        }
        return allItems;
    }

    public override bool OnPickup(Item item)
    {
        bool result = true;
        foreach (PlayerBehavior behavior in BehaviorSet.Enumerate(Player))
            result &= behavior.OnPickup(item);
        return result;
    }

    public override bool CanBeTeleportedTo(Vector2 teleportPosition, string context)
    {
        bool result = true;
        foreach (PlayerBehavior behavior in BehaviorSet.Enumerate(Player))
            result &= behavior.CanBeTeleportedTo(teleportPosition, context);
        return result;
    }

    public override void OnEquipmentLoadoutSwitched(int oldLoadoutIndex, int loadoutIndex)
    {
        foreach (PlayerBehavior behavior in BehaviorSet.Enumerate(Player))
            behavior.OnEquipmentLoadoutSwitched(oldLoadoutIndex, loadoutIndex);
    }
}

public sealed class GlobalNPCBehaviorHandler : GlobalNPC
{
    public override bool InstancePerEntity => true;

    public static readonly GlobalEntityBehaviorSet<NPC, GlobalNPCBehavior> BehaviorSet = new();

    public override void SetStaticDefaults()
    {
        foreach (GlobalNPCBehavior behavior in BehaviorSet.Enumerate())
            behavior.SetStaticDefaults();
    }

    public override void SetDefaults(NPC npc)
    {
        foreach (GlobalNPCBehavior behavior in BehaviorSet.Enumerate())
            behavior.SetDefaults(npc);
    }

    public override void SetDefaultsFromNetId(NPC npc)
    {
        foreach (GlobalNPCBehavior behavior in BehaviorSet.Enumerate())
            behavior.SetDefaultsFromNetId(npc);
    }

    public override void OnSpawn(NPC npc, IEntitySource source)
    {
        foreach (GlobalNPCBehavior behavior in BehaviorSet.Enumerate())
            behavior.OnSpawn(npc, source);
    }

    public override void ApplyDifficultyAndPlayerScaling(NPC npc, int numPlayers, float balance, float bossAdjustment)
    {
        foreach (GlobalNPCBehavior behavior in BehaviorSet.Enumerate())
            behavior.ApplyDifficultyAndPlayerScaling(npc, numPlayers, balance, bossAdjustment);
    }

    public override void SetBestiary(NPC npc, BestiaryDatabase database, BestiaryEntry bestiaryEntry)
    {
        foreach (GlobalNPCBehavior behavior in BehaviorSet.Enumerate())
            behavior.SetBestiary(npc, database, bestiaryEntry);
    }

    public override void ModifyTypeName(NPC npc, ref string typeName)
    {
        foreach (GlobalNPCBehavior behavior in BehaviorSet.Enumerate())
            behavior.ModifyTypeName(npc, ref typeName);
    }

    public override void ModifyHoverBoundingBox(NPC npc, ref Rectangle boundingBox)
    {
        foreach (GlobalNPCBehavior behavior in BehaviorSet.Enumerate())
            behavior.ModifyHoverBoundingBox(npc, ref boundingBox);
    }

    public override bool PreHoverInteract(NPC npc, bool mouseIntersects)
    {
        bool result = true;
        foreach (GlobalNPCBehavior behavior in BehaviorSet.Enumerate())
            result &= behavior.PreHoverInteract(npc, mouseIntersects);
        return result;
    }

    public override ITownNPCProfile ModifyTownNPCProfile(NPC npc)
    {
        foreach (GlobalNPCBehavior behavior in BehaviorSet.Enumerate())
        {
            ITownNPCProfile temp = behavior.ModifyTownNPCProfile(npc);
            if (temp is not null)
                return temp;
        }
        return null;
    }

    public override void ModifyNPCNameList(NPC npc, List<string> nameList)
    {
        foreach (GlobalNPCBehavior behavior in BehaviorSet.Enumerate())
            behavior.ModifyNPCNameList(npc, nameList);
    }

    public override void ResetEffects(NPC npc)
    {
        foreach (GlobalNPCBehavior behavior in BehaviorSet.Enumerate())
            behavior.ResetEffects(npc);
    }

    public override bool PreAI(NPC npc)
    {
        bool result = true;
        foreach (GlobalNPCBehavior behavior in BehaviorSet.Enumerate())
            result &= behavior.PreAI(npc);
        return result;
    }

    public override void AI(NPC npc)
    {
        foreach (GlobalNPCBehavior behavior in BehaviorSet.Enumerate())
            behavior.AI(npc);
    }

    public override void PostAI(NPC npc)
    {
        foreach (GlobalNPCBehavior behavior in BehaviorSet.Enumerate())
            behavior.PostAI(npc);
    }

    public override void SendExtraAI(NPC npc, BitWriter bitWriter, BinaryWriter binaryWriter)
    {
        if (TOSharedData.SyncEnabled)
        {
            //foreach (GlobalNPCBehavior behavior in BehaviorSet.GetBehaviors())
            //    behavior.SendExtraAI(npc, bitWriter, binaryWriter);
        }
    }

    public override void ReceiveExtraAI(NPC npc, BitReader bitReader, BinaryReader binaryReader)
    {
        if (TOSharedData.SyncEnabled)
        {
            //foreach (GlobalNPCBehavior behavior in BehaviorSet.GetBehaviors())
            //    behavior.ReceiveExtraAI(npc, bitReader, binaryReader);
        }
    }

    public override void FindFrame(NPC npc, int frameHeight)
    {
        foreach (GlobalNPCBehavior behavior in BehaviorSet.Enumerate())
            behavior.FindFrame(npc, frameHeight);
    }

    public override void HitEffect(NPC npc, NPC.HitInfo hit)
    {
        foreach (GlobalNPCBehavior behavior in BehaviorSet.Enumerate())
            behavior.HitEffect(npc, hit);
    }

    public override void UpdateLifeRegen(NPC npc, ref int damage)
    {
        foreach (GlobalNPCBehavior behavior in BehaviorSet.Enumerate())
            behavior.UpdateLifeRegen(npc, ref damage);
    }

    public override bool CheckActive(NPC npc)
    {
        bool result = true;
        foreach (GlobalNPCBehavior behavior in BehaviorSet.Enumerate())
            result &= behavior.CheckActive(npc);
        return result;
    }

    public override bool CheckDead(NPC npc)
    {
        bool result = true;
        foreach (GlobalNPCBehavior behavior in BehaviorSet.Enumerate())
            result &= behavior.CheckDead(npc);
        return result;
    }

    public override bool SpecialOnKill(NPC npc)
    {
        bool result = false;
        foreach (GlobalNPCBehavior behavior in BehaviorSet.Enumerate())
            result |= behavior.SpecialOnKill(npc);
        return result;
    }

    public override bool PreKill(NPC npc)
    {
        bool result = true;
        foreach (GlobalNPCBehavior behavior in BehaviorSet.Enumerate())
            result &= behavior.PreKill(npc);
        return result;
    }

    public override void OnKill(NPC npc)
    {
        foreach (GlobalNPCBehavior behavior in BehaviorSet.Enumerate())
            behavior.OnKill(npc);
    }

    public override bool? CanFallThroughPlatforms(NPC npc)
    {
        foreach (GlobalNPCBehavior behavior in BehaviorSet.Enumerate())
        {
            bool? result = behavior.CanFallThroughPlatforms(npc);
            if (result is not null)
                return result;
        }
        return null;
    }

    public override bool? CanBeCaughtBy(NPC npc, Item item, Player player)
    {
        foreach (GlobalNPCBehavior behavior in BehaviorSet.Enumerate())
        {
            bool? result = behavior.CanBeCaughtBy(npc, item, player);
            if (result is not null)
                return result;
        }
        return null;
    }

    public override void OnCaughtBy(NPC npc, Player player, Item item, bool failed)
    {
        foreach (GlobalNPCBehavior behavior in BehaviorSet.Enumerate())
            behavior.OnCaughtBy(npc, player, item, failed);
    }

    public override void ModifyNPCLoot(NPC npc, NPCLoot npcLoot)
    {
        foreach (GlobalNPCBehavior behavior in BehaviorSet.Enumerate())
            behavior.ModifyNPCLoot(npc, npcLoot);
    }

    public override void ModifyGlobalLoot(GlobalLoot globalLoot)
    {
        foreach (GlobalNPCBehavior behavior in BehaviorSet.Enumerate())
            behavior.ModifyGlobalLoot(globalLoot);
    }

    /*
    public override bool CanHitPlayer(NPC npc, Player target, ref int cooldownSlot)
    {
        bool result = true;
        foreach (GlobalNPCBehavior behavior in BehaviorSet.GetBehaviors())
            result &= behavior.CanHitPlayer(npc, target, ref cooldownSlot);
        return result;
    }*/

    public override void ModifyHitPlayer(NPC npc, Player target, ref Player.HurtModifiers modifiers)
    {
        foreach (GlobalNPCBehavior behavior in BehaviorSet.Enumerate())
            behavior.ModifyHitPlayer(npc, target, ref modifiers);
    }

    public override void OnHitPlayer(NPC npc, Player target, Player.HurtInfo hurtInfo)
    {
        foreach (GlobalNPCBehavior behavior in BehaviorSet.Enumerate())
            behavior.OnHitPlayer(npc, target, hurtInfo);
    }

    /*
    public override bool CanHitNPC(NPC npc, NPC target)
    {
        bool result = true;
        foreach (GlobalNPCBehavior behavior in BehaviorSet.GetBehaviors())
            result &= behavior.CanHitNPC(npc, target);
        return result;
    }

    public override bool CanBeHitByNPC(NPC npc, NPC attacker)
    {
        bool result = true;
        foreach (GlobalNPCBehavior behavior in BehaviorSet.GetBehaviors())
            result &= behavior.CanBeHitByNPC(npc, attacker);
        return result;
    }*/

    public override void ModifyHitNPC(NPC npc, NPC target, ref NPC.HitModifiers modifiers)
    {
        foreach (GlobalNPCBehavior behavior in BehaviorSet.Enumerate())
            behavior.ModifyHitNPC(npc, target, ref modifiers);
    }

    public override void OnHitNPC(NPC npc, NPC target, NPC.HitInfo hit)
    {
        foreach (GlobalNPCBehavior behavior in BehaviorSet.Enumerate())
            behavior.OnHitNPC(npc, target, hit);
    }

    /*
    public override bool? CanBeHitByItem(NPC npc, Player player, Item item)
    {
        foreach (GlobalNPCBehavior behavior in BehaviorSet.GetBehaviors())
        {
            bool? result = behavior.CanBeHitByItem(npc, player, item);
            if (result is not null)
                return result;
        }
        return null;
    }

    public override bool? CanCollideWithPlayerMeleeAttack(NPC npc, Player player, Item item, Rectangle meleeAttackHitbox)
    {
        foreach (GlobalNPCBehavior behavior in BehaviorSet.GetBehaviors())
        {
            bool? result = behavior.CanCollideWithPlayerMeleeAttack(npc, player, item, meleeAttackHitbox);
            if (result is not null)
                return result;
        }
        return null;
    }*/

    public override void ModifyHitByItem(NPC npc, Player player, Item item, ref NPC.HitModifiers modifiers)
    {
        foreach (GlobalNPCBehavior behavior in BehaviorSet.Enumerate())
            behavior.ModifyHitByItem(npc, player, item, ref modifiers);
    }

    public override void OnHitByItem(NPC npc, Player player, Item item, NPC.HitInfo hit, int damageDone)
    {
        foreach (GlobalNPCBehavior behavior in BehaviorSet.Enumerate())
            behavior.OnHitByItem(npc, player, item, hit, damageDone);
    }

    /*
    public override bool? CanBeHitByProjectile(NPC npc, Projectile projectile)
    {
        foreach (GlobalNPCBehavior behavior in BehaviorSet.GetBehaviors())
        {
            bool? result = behavior.CanBeHitByProjectile(npc, projectile);
            if (result is not null)
                return result;
        }
        return null;
    }*/

    public override void ModifyHitByProjectile(NPC npc, Projectile projectile, ref NPC.HitModifiers modifiers)
    {
        foreach (GlobalNPCBehavior behavior in BehaviorSet.Enumerate())
            behavior.ModifyHitByProjectile(npc, projectile, ref modifiers);
    }

    public override void OnHitByProjectile(NPC npc, Projectile projectile, NPC.HitInfo hit, int damageDone)
    {
        foreach (GlobalNPCBehavior behavior in BehaviorSet.Enumerate())
            behavior.OnHitByProjectile(npc, projectile, hit, damageDone);
    }

    public override void ModifyIncomingHit(NPC npc, ref NPC.HitModifiers modifiers)
    {
        foreach (GlobalNPCBehavior behavior in BehaviorSet.Enumerate())
            behavior.ModifyIncomingHit(npc, ref modifiers);
    }

    public override void BossHeadSlot(NPC npc, ref int index)
    {
        foreach (GlobalNPCBehavior behavior in BehaviorSet.Enumerate())
            behavior.BossHeadSlot(npc, ref index);
    }

    public override void BossHeadRotation(NPC npc, ref float rotation)
    {
        foreach (GlobalNPCBehavior behavior in BehaviorSet.Enumerate())
            behavior.BossHeadRotation(npc, ref rotation);
    }

    public override void BossHeadSpriteEffects(NPC npc, ref SpriteEffects spriteEffects)
    {
        foreach (GlobalNPCBehavior behavior in BehaviorSet.Enumerate())
            behavior.BossHeadSpriteEffects(npc, ref spriteEffects);
    }

    public override Color? GetAlpha(NPC npc, Color drawColor)
    {
        foreach (GlobalNPCBehavior behavior in BehaviorSet.Enumerate())
        {
            Color? result = behavior.GetAlpha(npc, drawColor);
            if (result is not null)
                return result;
        }
        return null;
    }

    public override void DrawEffects(NPC npc, ref Color drawColor)
    {
        foreach (GlobalNPCBehavior behavior in BehaviorSet.Enumerate())
            behavior.DrawEffects(npc, ref drawColor);
    }

    public override bool PreDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        bool result = true;
        foreach (GlobalNPCBehavior behavior in BehaviorSet.Enumerate())
            result &= behavior.PreDraw(npc, spriteBatch, screenPos, drawColor);
        return result;
    }

    public override void PostDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        foreach (GlobalNPCBehavior behavior in BehaviorSet.Enumerate())
            behavior.PostDraw(npc, spriteBatch, screenPos, drawColor);
    }

    public override void DrawBehind(NPC npc, int index)
    {
        foreach (GlobalNPCBehavior behavior in BehaviorSet.Enumerate())
            behavior.DrawBehind(npc, index);
    }

    public override bool? DrawHealthBar(NPC npc, byte hbPosition, ref float scale, ref Vector2 position)
    {
        foreach (GlobalNPCBehavior behavior in BehaviorSet.Enumerate())
        {
            bool? result = behavior.DrawHealthBar(npc, hbPosition, ref scale, ref position);
            if (result is not null)
                return result;
        }
        return null;
    }

    public override void EditSpawnRate(Player player, ref int spawnRate, ref int maxSpawns)
    {
        foreach (GlobalNPCBehavior behavior in BehaviorSet.Enumerate())
            behavior.EditSpawnRate(player, ref spawnRate, ref maxSpawns);
    }

    public override void EditSpawnRange(Player player, ref int spawnRangeX, ref int spawnRangeY, ref int safeRangeX, ref int safeRangeY)
    {
        foreach (GlobalNPCBehavior behavior in BehaviorSet.Enumerate())
            behavior.EditSpawnRange(player, ref spawnRangeX, ref spawnRangeY, ref safeRangeX, ref safeRangeY);
    }

    public override void EditSpawnPool(IDictionary<int, float> pool, NPCSpawnInfo spawnInfo)
    {
        foreach (GlobalNPCBehavior behavior in BehaviorSet.Enumerate())
            behavior.EditSpawnPool(pool, spawnInfo);
    }

    public override void SpawnNPC(int npc, int tileX, int tileY)
    {
        foreach (GlobalNPCBehavior behavior in BehaviorSet.Enumerate())
            behavior.SpawnNPC(npc, tileX, tileY);
    }

    public override bool? CanChat(NPC npc)
    {
        foreach (GlobalNPCBehavior behavior in BehaviorSet.Enumerate())
        {
            bool? result = behavior.CanChat(npc);
            if (result is not null)
                return result;
        }
        return null;
    }

    public override void GetChat(NPC npc, ref string chat)
    {
        foreach (GlobalNPCBehavior behavior in BehaviorSet.Enumerate())
            behavior.GetChat(npc, ref chat);
    }

    public override bool PreChatButtonClicked(NPC npc, bool firstButton)
    {
        bool result = true;
        foreach (GlobalNPCBehavior behavior in BehaviorSet.Enumerate())
            result &= behavior.PreChatButtonClicked(npc, firstButton);
        return result;
    }

    public override void OnChatButtonClicked(NPC npc, bool firstButton)
    {
        foreach (GlobalNPCBehavior behavior in BehaviorSet.Enumerate())
            behavior.OnChatButtonClicked(npc, firstButton);
    }

    public override void ModifyShop(NPCShop shop)
    {
        foreach (GlobalNPCBehavior behavior in BehaviorSet.Enumerate())
            behavior.ModifyShop(shop);
    }

    public override void ModifyActiveShop(NPC npc, string shopName, Item[] items)
    {
        foreach (GlobalNPCBehavior behavior in BehaviorSet.Enumerate())
            behavior.ModifyActiveShop(npc, shopName, items);
    }

    public override void SetupTravelShop(int[] shop, ref int nextSlot)
    {
        foreach (GlobalNPCBehavior behavior in BehaviorSet.Enumerate())
            behavior.SetupTravelShop(shop, ref nextSlot);
    }

    public override bool? CanGoToStatue(NPC npc, bool toKingStatue)
    {
        foreach (GlobalNPCBehavior behavior in BehaviorSet.Enumerate())
        {
            bool? result = behavior.CanGoToStatue(npc, toKingStatue);
            if (result is not null)
                return result;
        }
        return null;
    }

    public override void OnGoToStatue(NPC npc, bool toKingStatue)
    {
        foreach (GlobalNPCBehavior behavior in BehaviorSet.Enumerate())
            behavior.OnGoToStatue(npc, toKingStatue);
    }

    public override void BuffTownNPC(ref float damageMult, ref int defense)
    {
        foreach (GlobalNPCBehavior behavior in BehaviorSet.Enumerate())
            behavior.BuffTownNPC(ref damageMult, ref defense);
    }

    public override bool ModifyDeathMessage(NPC npc, ref NetworkText customText, ref Color color)
    {
        bool result = true;
        foreach (GlobalNPCBehavior behavior in BehaviorSet.Enumerate())
            result &= behavior.ModifyDeathMessage(npc, ref customText, ref color);
        return result;
    }

    public override void TownNPCAttackStrength(NPC npc, ref int damage, ref float knockback)
    {
        foreach (GlobalNPCBehavior behavior in BehaviorSet.Enumerate())
            behavior.TownNPCAttackStrength(npc, ref damage, ref knockback);
    }

    public override void TownNPCAttackCooldown(NPC npc, ref int cooldown, ref int randExtraCooldown)
    {
        foreach (GlobalNPCBehavior behavior in BehaviorSet.Enumerate())
            behavior.TownNPCAttackCooldown(npc, ref cooldown, ref randExtraCooldown);
    }

    public override void TownNPCAttackProj(NPC npc, ref int projType, ref int attackDelay)
    {
        foreach (GlobalNPCBehavior behavior in BehaviorSet.Enumerate())
            behavior.TownNPCAttackProj(npc, ref projType, ref attackDelay);
    }

    public override void TownNPCAttackProjSpeed(NPC npc, ref float multiplier, ref float gravityCorrection, ref float randomOffset)
    {
        foreach (GlobalNPCBehavior behavior in BehaviorSet.Enumerate())
            behavior.TownNPCAttackProjSpeed(npc, ref multiplier, ref gravityCorrection, ref randomOffset);
    }

    public override void TownNPCAttackShoot(NPC npc, ref bool inBetweenShots)
    {
        foreach (GlobalNPCBehavior behavior in BehaviorSet.Enumerate())
            behavior.TownNPCAttackShoot(npc, ref inBetweenShots);
    }

    public override void TownNPCAttackMagic(NPC npc, ref float auraLightMultiplier)
    {
        foreach (GlobalNPCBehavior behavior in BehaviorSet.Enumerate())
            behavior.TownNPCAttackMagic(npc, ref auraLightMultiplier);
    }

    public override void TownNPCAttackSwing(NPC npc, ref int itemWidth, ref int itemHeight)
    {
        foreach (GlobalNPCBehavior behavior in BehaviorSet.Enumerate())
            behavior.TownNPCAttackSwing(npc, ref itemWidth, ref itemHeight);
    }

    public override void DrawTownAttackGun(NPC npc, ref Texture2D item, ref Rectangle itemFrame, ref float scale, ref int horizontalHoldoutOffset)
    {
        foreach (GlobalNPCBehavior behavior in BehaviorSet.Enumerate())
            behavior.DrawTownAttackGun(npc, ref item, ref itemFrame, ref scale, ref horizontalHoldoutOffset);
    }

    public override void DrawTownAttackSwing(NPC npc, ref Texture2D item, ref Rectangle itemFrame, ref int itemSize, ref float scale, ref Vector2 offset)
    {
        foreach (GlobalNPCBehavior behavior in BehaviorSet.Enumerate())
            behavior.DrawTownAttackSwing(npc, ref item, ref itemFrame, ref itemSize, ref scale, ref offset);
    }

    public override bool ModifyCollisionData(NPC npc, Rectangle victimHitbox, ref int immunityCooldownSlot, ref MultipliableFloat damageMultiplier, ref Rectangle npcHitbox)
    {
        bool result = true;
        foreach (GlobalNPCBehavior behavior in BehaviorSet.Enumerate())
            result &= behavior.ModifyCollisionData(npc, victimHitbox, ref immunityCooldownSlot, ref damageMultiplier, ref npcHitbox);
        return result;
    }

    public override bool NeedSaving(NPC npc)
    {
        bool result = false;
        foreach (GlobalNPCBehavior behavior in BehaviorSet.Enumerate())
            result |= behavior.NeedSaving(npc);
        return result;
    }

    public override void SaveData(NPC npc, TagCompound tag)
    {
        foreach (GlobalNPCBehavior behavior in BehaviorSet.Enumerate())
            behavior.SaveData(npc, tag);
    }

    public override void LoadData(NPC npc, TagCompound tag)
    {
        foreach (GlobalNPCBehavior behavior in BehaviorSet.Enumerate())
            behavior.LoadData(npc, tag);
    }

    public override int? PickEmote(NPC npc, Player closestPlayer, List<int> emoteList, WorldUIAnchor otherAnchor)
    {
        foreach (GlobalNPCBehavior behavior in BehaviorSet.Enumerate())
        {
            int? result = behavior.PickEmote(npc, closestPlayer, emoteList, otherAnchor);
            if (result is not null)
                return result;
        }
        return null;
    }

    public override void ChatBubblePosition(NPC npc, ref Vector2 position, ref SpriteEffects spriteEffects)
    {
        foreach (GlobalNPCBehavior behavior in BehaviorSet.Enumerate())
            behavior.ChatBubblePosition(npc, ref position, ref spriteEffects);
    }

    public override void PartyHatPosition(NPC npc, ref Vector2 position, ref SpriteEffects spriteEffects)
    {
        foreach (GlobalNPCBehavior behavior in BehaviorSet.Enumerate())
            behavior.PartyHatPosition(npc, ref position, ref spriteEffects);
    }

    public override void EmoteBubblePosition(NPC npc, ref Vector2 position, ref SpriteEffects spriteEffects)
    {
        foreach (GlobalNPCBehavior behavior in BehaviorSet.Enumerate())
            behavior.EmoteBubblePosition(npc, ref position, ref spriteEffects);
    }
}

public sealed class GlobalProjectileBehaviorHandler : GlobalProjectile
{
    public override bool InstancePerEntity => true;

    public static readonly GlobalEntityBehaviorSet<Projectile, GlobalProjectileBehavior> BehaviorSet = new();

    public override void SetStaticDefaults()
    {
        foreach (GlobalProjectileBehavior behavior in BehaviorSet.Enumerate())
            behavior.SetStaticDefaults();
    }

    public override void SetDefaults(Projectile projectile)
    {
        foreach (GlobalProjectileBehavior behavior in BehaviorSet.Enumerate())
            behavior.SetDefaults(projectile);
    }

    public override void OnSpawn(Projectile projectile, IEntitySource source)
    {
        foreach (GlobalProjectileBehavior behavior in BehaviorSet.Enumerate())
            behavior.OnSpawn(projectile, source);
    }

    public override bool PreAI(Projectile projectile)
    {
        bool result = true;
        foreach (GlobalProjectileBehavior behavior in BehaviorSet.Enumerate())
            result &= behavior.PreAI(projectile);
        return result;
    }

    public override void AI(Projectile projectile)
    {
        foreach (GlobalProjectileBehavior behavior in BehaviorSet.Enumerate())
            behavior.AI(projectile);
    }

    public override void PostAI(Projectile projectile)
    {
        foreach (GlobalProjectileBehavior behavior in BehaviorSet.Enumerate())
            behavior.PostAI(projectile);
    }

    public override void SendExtraAI(Projectile projectile, BitWriter bitWriter, BinaryWriter binaryWriter)
    {
        if (TOSharedData.SyncEnabled)
        {
            //foreach (GlobalProjectileBehavior behavior in BehaviorSet.GetBehaviors())
            //    behavior.SendExtraAI(projectile, bitWriter, binaryWriter);
        }
    }

    public override void ReceiveExtraAI(Projectile projectile, BitReader bitReader, BinaryReader binaryReader)
    {
        if (TOSharedData.SyncEnabled)
        {
            //foreach (GlobalProjectileBehavior behavior in BehaviorSet.GetBehaviors())
            //    behavior.ReceiveExtraAI(projectile, bitReader, binaryReader);
        }
    }

    public override bool ShouldUpdatePosition(Projectile projectile)
    {
        bool result = true;
        foreach (GlobalProjectileBehavior behavior in BehaviorSet.Enumerate())
            result &= behavior.ShouldUpdatePosition(projectile);
        return result;
    }

    public override bool TileCollideStyle(Projectile projectile, ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac)
    {
        bool result = true;
        foreach (GlobalProjectileBehavior behavior in BehaviorSet.Enumerate())
            result &= behavior.TileCollideStyle(projectile, ref width, ref height, ref fallThrough, ref hitboxCenterFrac);
        return result;
    }

    public override bool OnTileCollide(Projectile projectile, Vector2 oldVelocity)
    {
        bool result = true;
        foreach (GlobalProjectileBehavior behavior in BehaviorSet.Enumerate())
            result &= behavior.OnTileCollide(projectile, oldVelocity);
        return result;
    }

    public override bool PreKill(Projectile projectile, int timeLeft)
    {
        bool result = true;
        foreach (GlobalProjectileBehavior behavior in BehaviorSet.Enumerate())
            result &= behavior.PreKill(projectile, timeLeft);
        return result;
    }

    public override void OnKill(Projectile projectile, int timeLeft)
    {
        foreach (GlobalProjectileBehavior behavior in BehaviorSet.Enumerate())
            behavior.OnKill(projectile, timeLeft);
    }

    public override bool? CanCutTiles(Projectile projectile)
    {
        foreach (GlobalProjectileBehavior behavior in BehaviorSet.Enumerate())
        {
            bool? result = behavior.CanCutTiles(projectile);
            if (result is not null)
                return result;
        }
        return null;
    }

    public override void CutTiles(Projectile projectile)
    {
        foreach (GlobalProjectileBehavior behavior in BehaviorSet.Enumerate())
            behavior.CutTiles(projectile);
    }

    public override bool? CanDamage(Projectile projectile)
    {
        foreach (GlobalProjectileBehavior behavior in BehaviorSet.Enumerate())
        {
            bool? result = behavior.CanDamage(projectile);
            if (result is not null)
                return result;
        }
        return null;
    }

    public override bool MinionContactDamage(Projectile projectile)
    {
        bool result = false;
        foreach (GlobalProjectileBehavior behavior in BehaviorSet.Enumerate())
            result |= behavior.MinionContactDamage(projectile);
        return result;
    }

    public override void ModifyDamageHitbox(Projectile projectile, ref Rectangle hitbox)
    {
        foreach (GlobalProjectileBehavior behavior in BehaviorSet.Enumerate())
            behavior.ModifyDamageHitbox(projectile, ref hitbox);
    }

    /*
    public override bool? CanHitNPC(Projectile projectile, NPC target)
    {
        foreach (GlobalProjectileBehavior behavior in BehaviorSet.GetBehaviors())
        {
            bool? result = behavior.CanHitNPC(projectile, target);
            if (result is not null)
                return result;
        }
        return null;
    }*/

    public override void ModifyHitNPC(Projectile projectile, NPC target, ref NPC.HitModifiers modifiers)
    {
        foreach (GlobalProjectileBehavior behavior in BehaviorSet.Enumerate())
            behavior.ModifyHitNPC(projectile, target, ref modifiers);
    }

    public override void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone)
    {
        foreach (GlobalProjectileBehavior behavior in BehaviorSet.Enumerate())
            behavior.OnHitNPC(projectile, target, hit, damageDone);
    }

    /*
    public override bool CanHitPvp(Projectile projectile, Player target)
    {
        bool result = true;
        foreach (GlobalProjectileBehavior behavior in BehaviorSet.GetBehaviors())
            result &= behavior.CanHitPvp(projectile, target);
        return result;
    }

    public override bool CanHitPlayer(Projectile projectile, Player target)
    {
        bool result = true;
        foreach (GlobalProjectileBehavior behavior in BehaviorSet.GetBehaviors())
            result &= behavior.CanHitPlayer(projectile, target);
        return result;
    }*/

    public override void ModifyHitPlayer(Projectile projectile, Player target, ref Player.HurtModifiers modifiers)
    {
        foreach (GlobalProjectileBehavior behavior in BehaviorSet.Enumerate())
            behavior.ModifyHitPlayer(projectile, target, ref modifiers);
    }

    public override void OnHitPlayer(Projectile projectile, Player target, Player.HurtInfo info)
    {
        foreach (GlobalProjectileBehavior behavior in BehaviorSet.Enumerate())
            behavior.OnHitPlayer(projectile, target, info);
    }

    /*
    public override bool? Colliding(Projectile projectile, Rectangle projHitbox, Rectangle targetHitbox)
    {
        foreach (GlobalProjectileBehavior behavior in BehaviorSet.GetBehaviors())
        {
            bool? result = behavior.Colliding(projectile, projHitbox, targetHitbox);
            if (result is not null)
                return result;
        }
        return null;
    }*/

    public override Color? GetAlpha(Projectile projectile, Color lightColor)
    {
        foreach (GlobalProjectileBehavior behavior in BehaviorSet.Enumerate())
        {
            Color? result = behavior.GetAlpha(projectile, lightColor);
            if (result is not null)
                return result;
        }
        return null;
    }

    public override bool PreDrawExtras(Projectile projectile)
    {
        bool result = true;
        foreach (GlobalProjectileBehavior behavior in BehaviorSet.Enumerate())
            result &= behavior.PreDrawExtras(projectile);
        return result;
    }

    public override bool PreDraw(Projectile projectile, ref Color lightColor)
    {
        bool result = true;
        foreach (GlobalProjectileBehavior behavior in BehaviorSet.Enumerate())
            result &= behavior.PreDraw(projectile, ref lightColor);
        return result;
    }

    public override void PostDraw(Projectile projectile, Color lightColor)
    {
        foreach (GlobalProjectileBehavior behavior in BehaviorSet.Enumerate())
            behavior.PostDraw(projectile, lightColor);
    }

    public override void DrawBehind(Projectile projectile, int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
    {
        foreach (GlobalProjectileBehavior behavior in BehaviorSet.Enumerate())
            behavior.DrawBehind(projectile, index, behindNPCsAndTiles, behindNPCs, behindProjectiles, overPlayers, overWiresUI);
    }

    public override bool? CanUseGrapple(int type, Player player)
    {
        foreach (GlobalProjectileBehavior behavior in BehaviorSet.Enumerate())
        {
            bool? result = behavior.CanUseGrapple(type, player);
            if (result is not null)
                return result;
        }
        return null;
    }

    public override void UseGrapple(Player player, ref int type)
    {
        foreach (GlobalProjectileBehavior behavior in BehaviorSet.Enumerate())
            behavior.UseGrapple(player, ref type);
    }

    public override void NumGrappleHooks(Projectile projectile, Player player, ref int numHooks)
    {
        foreach (GlobalProjectileBehavior behavior in BehaviorSet.Enumerate())
            behavior.NumGrappleHooks(projectile, player, ref numHooks);
    }

    public override void GrappleRetreatSpeed(Projectile projectile, Player player, ref float speed)
    {
        foreach (GlobalProjectileBehavior behavior in BehaviorSet.Enumerate())
            behavior.GrappleRetreatSpeed(projectile, player, ref speed);
    }

    public override void GrapplePullSpeed(Projectile projectile, Player player, ref float speed)
    {
        foreach (GlobalProjectileBehavior behavior in BehaviorSet.Enumerate())
            behavior.GrapplePullSpeed(projectile, player, ref speed);
    }

    public override void GrappleTargetPoint(Projectile projectile, Player player, ref float grappleX, ref float grappleY)
    {
        foreach (GlobalProjectileBehavior behavior in BehaviorSet.Enumerate())
            behavior.GrappleTargetPoint(projectile, player, ref grappleX, ref grappleY);
    }

    public override bool? GrappleCanLatchOnTo(Projectile projectile, Player player, int x, int y)
    {
        foreach (GlobalProjectileBehavior behavior in BehaviorSet.Enumerate())
        {
            bool? result = behavior.GrappleCanLatchOnTo(projectile, player, x, y);
            if (result is not null)
                return result;
        }
        return null;
    }

    public override void PrepareBombToBlow(Projectile projectile)
    {
        foreach (GlobalProjectileBehavior behavior in BehaviorSet.Enumerate())
            behavior.PrepareBombToBlow(projectile);
    }

    public override void EmitEnchantmentVisualsAt(Projectile projectile, Vector2 boxPosition, int boxWidth, int boxHeight)
    {
        foreach (GlobalProjectileBehavior behavior in BehaviorSet.Enumerate())
            behavior.EmitEnchantmentVisualsAt(projectile, boxPosition, boxWidth, boxHeight);
    }
}

public sealed class GlobalItemBehaviorHandler : GlobalItem
{
    public override bool InstancePerEntity => true;

    public static readonly GlobalEntityBehaviorSet<Item, GlobalItemBehavior> BehaviorSet = new();

    public override void SetStaticDefaults()
    {
        foreach (GlobalItemBehavior behavior in BehaviorSet.Enumerate())
            behavior.SetStaticDefaults();
    }

    public override void SetDefaults(Item item)
    {
        foreach (GlobalItemBehavior behavior in BehaviorSet.Enumerate())
            behavior.SetDefaults(item);
    }

    public override void OnCreated(Item item, ItemCreationContext context)
    {
        foreach (GlobalItemBehavior behavior in BehaviorSet.Enumerate())
            behavior.OnCreated(item, context);
    }

    public override void OnSpawn(Item item, IEntitySource source)
    {
        foreach (GlobalItemBehavior behavior in BehaviorSet.Enumerate())
            behavior.OnSpawn(item, source);
    }

    public override int ChoosePrefix(Item item, UnifiedRandom rand)
    {
        foreach (GlobalItemBehavior behavior in BehaviorSet.Enumerate())
        {
            int result = behavior.ChoosePrefix(item, rand);
            if (result != -1)
                return result;
        }
        return -1;
    }

    public override bool? PrefixChance(Item item, int pre, UnifiedRandom rand)
    {
        foreach (GlobalItemBehavior behavior in BehaviorSet.Enumerate())
        {
            bool? result = behavior.PrefixChance(item, pre, rand);
            if (result is not null)
                return result;
        }
        return null;
    }

    public override bool AllowPrefix(Item item, int pre)
    {
        bool result = true;
        foreach (GlobalItemBehavior behavior in BehaviorSet.Enumerate())
            result &= behavior.AllowPrefix(item, pre);
        return result;
    }

    public override bool CanUseItem(Item item, Player player)
    {
        bool result = true;
        foreach (GlobalItemBehavior behavior in BehaviorSet.Enumerate())
            result &= behavior.CanUseItem(item, player);
        return result;
    }

    public override bool? CanAutoReuseItem(Item item, Player player)
    {
        foreach (GlobalItemBehavior behavior in BehaviorSet.Enumerate())
        {
            bool? result = behavior.CanAutoReuseItem(item, player);
            if (result is not null)
                return result;
        }
        return null;
    }

    public override void UseStyle(Item item, Player player, Rectangle heldItemFrame)
    {
        foreach (GlobalItemBehavior behavior in BehaviorSet.Enumerate())
            behavior.UseStyle(item, player, heldItemFrame);
    }

    public override void HoldStyle(Item item, Player player, Rectangle heldItemFrame)
    {
        foreach (GlobalItemBehavior behavior in BehaviorSet.Enumerate())
            behavior.HoldStyle(item, player, heldItemFrame);
    }

    public override void HoldItem(Item item, Player player)
    {
        foreach (GlobalItemBehavior behavior in BehaviorSet.Enumerate())
            behavior.HoldItem(item, player);
    }

    public override float UseTimeMultiplier(Item item, Player player)
    {
        float result = 1f;
        foreach (GlobalItemBehavior behavior in BehaviorSet.Enumerate())
            result *= behavior.UseTimeMultiplier(item, player);
        return result;
    }

    public override float UseAnimationMultiplier(Item item, Player player)
    {
        float result = 1f;
        foreach (GlobalItemBehavior behavior in BehaviorSet.Enumerate())
            result *= behavior.UseAnimationMultiplier(item, player);
        return result;
    }

    public override float UseSpeedMultiplier(Item item, Player player)
    {
        float result = 1f;
        foreach (GlobalItemBehavior behavior in BehaviorSet.Enumerate())
            result *= behavior.UseSpeedMultiplier(item, player);
        return result;
    }

    public override void GetHealLife(Item item, Player player, bool quickHeal, ref int healValue)
    {
        foreach (GlobalItemBehavior behavior in BehaviorSet.Enumerate())
            behavior.GetHealLife(item, player, quickHeal, ref healValue);
    }

    public override void GetHealMana(Item item, Player player, bool quickHeal, ref int healValue)
    {
        foreach (GlobalItemBehavior behavior in BehaviorSet.Enumerate())
            behavior.GetHealMana(item, player, quickHeal, ref healValue);
    }

    public override void ModifyManaCost(Item item, Player player, ref float reduce, ref float mult)
    {
        foreach (GlobalItemBehavior behavior in BehaviorSet.Enumerate())
            behavior.ModifyManaCost(item, player, ref reduce, ref mult);
    }

    public override void OnMissingMana(Item item, Player player, int neededMana)
    {
        foreach (GlobalItemBehavior behavior in BehaviorSet.Enumerate())
            behavior.OnMissingMana(item, player, neededMana);
    }

    public override void OnConsumeMana(Item item, Player player, int manaConsumed)
    {
        foreach (GlobalItemBehavior behavior in BehaviorSet.Enumerate())
            behavior.OnConsumeMana(item, player, manaConsumed);
    }

    public override void ModifyWeaponDamage(Item item, Player player, ref StatModifier damage)
    {
        foreach (GlobalItemBehavior behavior in BehaviorSet.Enumerate())
            behavior.ModifyWeaponDamage(item, player, ref damage);
    }

    public override void ModifyResearchSorting(Item item, ref ContentSamples.CreativeHelper.ItemGroup itemGroup)
    {
        foreach (GlobalItemBehavior behavior in BehaviorSet.Enumerate())
            behavior.ModifyResearchSorting(item, ref itemGroup);
    }

    public override bool? CanConsumeBait(Player player, Item bait)
    {
        foreach (GlobalItemBehavior behavior in BehaviorSet.Enumerate())
        {
            bool? result = behavior.CanConsumeBait(player, bait);
            if (result is not null)
                return result;
        }
        return null;
    }

    public override bool CanResearch(Item item)
    {
        bool result = true;
        foreach (GlobalItemBehavior behavior in BehaviorSet.Enumerate())
            result &= behavior.CanResearch(item);
        return result;
    }

    public override void OnResearched(Item item, bool fullyResearched)
    {
        foreach (GlobalItemBehavior behavior in BehaviorSet.Enumerate())
            behavior.OnResearched(item, fullyResearched);
    }

    public override void ModifyWeaponKnockback(Item item, Player player, ref StatModifier knockback)
    {
        foreach (GlobalItemBehavior behavior in BehaviorSet.Enumerate())
            behavior.ModifyWeaponKnockback(item, player, ref knockback);
    }

    public override void ModifyWeaponCrit(Item item, Player player, ref float crit)
    {
        foreach (GlobalItemBehavior behavior in BehaviorSet.Enumerate())
            behavior.ModifyWeaponCrit(item, player, ref crit);
    }

    public override bool NeedsAmmo(Item item, Player player)
    {
        bool result = true;
        foreach (GlobalItemBehavior behavior in BehaviorSet.Enumerate())
            result &= behavior.NeedsAmmo(item, player);
        return result;
    }

    public override void PickAmmo(Item weapon, Item ammo, Player player, ref int type, ref float speed, ref StatModifier damage, ref float knockback)
    {
        foreach (GlobalItemBehavior behavior in BehaviorSet.Enumerate())
            behavior.PickAmmo(weapon, ammo, player, ref type, ref speed, ref damage, ref knockback);
    }

    public override bool? CanChooseAmmo(Item weapon, Item ammo, Player player)
    {
        foreach (GlobalItemBehavior behavior in BehaviorSet.Enumerate())
        {
            bool? result = behavior.CanChooseAmmo(weapon, ammo, player);
            if (result is not null)
                return result;
        }
        return null;
    }

    public override bool? CanBeChosenAsAmmo(Item ammo, Item weapon, Player player)
    {
        foreach (GlobalItemBehavior behavior in BehaviorSet.Enumerate())
        {
            bool? result = behavior.CanBeChosenAsAmmo(ammo, weapon, player);
            if (result is not null)
                return result;
        }
        return null;
    }

    public override bool CanConsumeAmmo(Item weapon, Item ammo, Player player)
    {
        bool result = true;
        foreach (GlobalItemBehavior behavior in BehaviorSet.Enumerate())
            result &= behavior.CanConsumeAmmo(weapon, ammo, player);
        return result;
    }

    public override bool CanBeConsumedAsAmmo(Item ammo, Item weapon, Player player)
    {
        bool result = true;
        foreach (GlobalItemBehavior behavior in BehaviorSet.Enumerate())
            result &= behavior.CanBeConsumedAsAmmo(ammo, weapon, player);
        return result;
    }

    public override void OnConsumeAmmo(Item weapon, Item ammo, Player player)
    {
        foreach (GlobalItemBehavior behavior in BehaviorSet.Enumerate())
            behavior.OnConsumeAmmo(weapon, ammo, player);
    }

    public override void OnConsumedAsAmmo(Item ammo, Item weapon, Player player)
    {
        foreach (GlobalItemBehavior behavior in BehaviorSet.Enumerate())
            behavior.OnConsumedAsAmmo(ammo, weapon, player);
    }

    public override bool CanShoot(Item item, Player player)
    {
        bool result = true;
        foreach (GlobalItemBehavior behavior in BehaviorSet.Enumerate())
            result &= behavior.CanShoot(item, player);
        return result;
    }

    public override void ModifyShootStats(Item item, Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
    {
        foreach (GlobalItemBehavior behavior in BehaviorSet.Enumerate())
            behavior.ModifyShootStats(item, player, ref position, ref velocity, ref type, ref damage, ref knockback);
    }

    public override bool Shoot(Item item, Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
    {
        bool result = true;
        foreach (GlobalItemBehavior behavior in BehaviorSet.Enumerate())
            result &= behavior.Shoot(item, player, source, position, velocity, type, damage, knockback);
        return result;
    }

    public override void UseItemHitbox(Item item, Player player, ref Rectangle hitbox, ref bool noHitbox)
    {
        foreach (GlobalItemBehavior behavior in BehaviorSet.Enumerate())
            behavior.UseItemHitbox(item, player, ref hitbox, ref noHitbox);
    }

    public override void MeleeEffects(Item item, Player player, Rectangle hitbox)
    {
        foreach (GlobalItemBehavior behavior in BehaviorSet.Enumerate())
            behavior.MeleeEffects(item, player, hitbox);
    }

    public override bool? CanCatchNPC(Item item, NPC target, Player player)
    {
        foreach (GlobalItemBehavior behavior in BehaviorSet.Enumerate())
        {
            bool? result = behavior.CanCatchNPC(item, target, player);
            if (result is not null)
                return result;
        }
        return null;
    }

    public override void OnCatchNPC(Item item, NPC npc, Player player, bool failed)
    {
        foreach (GlobalItemBehavior behavior in BehaviorSet.Enumerate())
            behavior.OnCatchNPC(item, npc, player, failed);
    }

    public override void ModifyItemScale(Item item, Player player, ref float scale)
    {
        foreach (GlobalItemBehavior behavior in BehaviorSet.Enumerate())
            behavior.ModifyItemScale(item, player, ref scale);
    }

    /*
    public override bool? CanHitNPC(Item item, Player player, NPC target)
    {
        foreach (GlobalItemBehavior behavior in BehaviorSet.GetBehaviors())
        {
            bool? result = behavior.CanHitNPC(item, player, target);
            if (result is not null)
                return result;
        }
        return null;
    }

    public override bool? CanMeleeAttackCollideWithNPC(Item item, Rectangle meleeAttackHitbox, Player player, NPC target)
    {
        foreach (GlobalItemBehavior behavior in BehaviorSet.GetBehaviors())
        {
            bool? result = behavior.CanMeleeAttackCollideWithNPC(item, meleeAttackHitbox, player, target);
            if (result is not null)
                return result;
        }
        return null;
    }*/

    public override void ModifyHitNPC(Item item, Player player, NPC target, ref NPC.HitModifiers modifiers)
    {
        foreach (GlobalItemBehavior behavior in BehaviorSet.Enumerate())
            behavior.ModifyHitNPC(item, player, target, ref modifiers);
    }

    public override void OnHitNPC(Item item, Player player, NPC target, NPC.HitInfo hit, int damageDone)
    {
        foreach (GlobalItemBehavior behavior in BehaviorSet.Enumerate())
            behavior.OnHitNPC(item, player, target, hit, damageDone);
    }

    /*
    public override bool CanHitPvp(Item item, Player player, Player target)
    {
        bool result = true;
        foreach (GlobalItemBehavior behavior in BehaviorSet.GetBehaviors())
            result &= behavior.CanHitPvp(item, player, target);
        return result;
    }*/

    public override void ModifyHitPvp(Item item, Player player, Player target, ref Player.HurtModifiers modifiers)
    {
        foreach (GlobalItemBehavior behavior in BehaviorSet.Enumerate())
            behavior.ModifyHitPvp(item, player, target, ref modifiers);
    }

    public override void OnHitPvp(Item item, Player player, Player target, Player.HurtInfo hurtInfo)
    {
        foreach (GlobalItemBehavior behavior in BehaviorSet.Enumerate())
            behavior.OnHitPvp(item, player, target, hurtInfo);
    }

    public override bool? UseItem(Item item, Player player)
    {
        foreach (GlobalItemBehavior behavior in BehaviorSet.Enumerate())
        {
            bool? result = behavior.UseItem(item, player);
            if (result is not null)
                return result;
        }
        return null;
    }

    public override void UseAnimation(Item item, Player player)
    {
        foreach (GlobalItemBehavior behavior in BehaviorSet.Enumerate())
            behavior.UseAnimation(item, player);
    }

    public override bool ConsumeItem(Item item, Player player)
    {
        bool result = true;
        foreach (GlobalItemBehavior behavior in BehaviorSet.Enumerate())
            result &= behavior.ConsumeItem(item, player);
        return result;
    }

    public override void OnConsumeItem(Item item, Player player)
    {
        foreach (GlobalItemBehavior behavior in BehaviorSet.Enumerate())
            behavior.OnConsumeItem(item, player);
    }

    public override void UseItemFrame(Item item, Player player)
    {
        foreach (GlobalItemBehavior behavior in BehaviorSet.Enumerate())
            behavior.UseItemFrame(item, player);
    }

    public override void HoldItemFrame(Item item, Player player)
    {
        foreach (GlobalItemBehavior behavior in BehaviorSet.Enumerate())
            behavior.HoldItemFrame(item, player);
    }

    public override bool AltFunctionUse(Item item, Player player)
    {
        bool result = false;
        foreach (GlobalItemBehavior behavior in BehaviorSet.Enumerate())
            result |= behavior.AltFunctionUse(item, player);
        return result;
    }

    public override void UpdateInventory(Item item, Player player)
    {
        foreach (GlobalItemBehavior behavior in BehaviorSet.Enumerate())
            behavior.UpdateInventory(item, player);
    }

    public override void UpdateInfoAccessory(Item item, Player player)
    {
        foreach (GlobalItemBehavior behavior in BehaviorSet.Enumerate())
            behavior.UpdateInfoAccessory(item, player);
    }

    public override void UpdateEquip(Item item, Player player)
    {
        foreach (GlobalItemBehavior behavior in BehaviorSet.Enumerate())
            behavior.UpdateEquip(item, player);
    }

    public override void UpdateAccessory(Item item, Player player, bool hideVisual)
    {
        foreach (GlobalItemBehavior behavior in BehaviorSet.Enumerate())
            behavior.UpdateAccessory(item, player, hideVisual);
    }

    public override void UpdateVanity(Item item, Player player)
    {
        foreach (GlobalItemBehavior behavior in BehaviorSet.Enumerate())
            behavior.UpdateVanity(item, player);
    }

    public override void UpdateVisibleAccessory(Item item, Player player, bool hideVisual)
    {
        foreach (GlobalItemBehavior behavior in BehaviorSet.Enumerate())
            behavior.UpdateVisibleAccessory(item, player, hideVisual);
    }

    public override void UpdateItemDye(Item item, Player player, int dye, bool hideVisual)
    {
        foreach (GlobalItemBehavior behavior in BehaviorSet.Enumerate())
            behavior.UpdateItemDye(item, player, dye, hideVisual);
    }

    public override string IsArmorSet(Item head, Item body, Item legs)
    {
        foreach (GlobalItemBehavior behavior in BehaviorSet.Enumerate())
        {
            string result = behavior.IsArmorSet(head, body, legs);
            if (result != "")
                return result;
        }
        return "";
    }

    public override void UpdateArmorSet(Player player, string set)
    {
        foreach (GlobalItemBehavior behavior in BehaviorSet.Enumerate())
            behavior.UpdateArmorSet(player, set);
    }

    public override string IsVanitySet(int head, int body, int legs)
    {
        foreach (GlobalItemBehavior behavior in BehaviorSet.Enumerate())
        {
            string result = behavior.IsVanitySet(head, body, legs);
            if (result != "")
                return result;
        }
        return "";
    }

    public override void PreUpdateVanitySet(Player player, string set)
    {
        foreach (GlobalItemBehavior behavior in BehaviorSet.Enumerate())
            behavior.PreUpdateVanitySet(player, set);
    }

    public override void UpdateVanitySet(Player player, string set)
    {
        foreach (GlobalItemBehavior behavior in BehaviorSet.Enumerate())
            behavior.UpdateVanitySet(player, set);
    }

    public override void ArmorSetShadows(Player player, string set)
    {
        foreach (GlobalItemBehavior behavior in BehaviorSet.Enumerate())
            behavior.ArmorSetShadows(player, set);
    }

    public override void SetMatch(int armorSlot, int type, bool male, ref int equipSlot, ref bool robes)
    {
        foreach (GlobalItemBehavior behavior in BehaviorSet.Enumerate())
            behavior.SetMatch(armorSlot, type, male, ref equipSlot, ref robes);
    }

    public override bool CanRightClick(Item item)
    {
        bool result = false;
        foreach (GlobalItemBehavior behavior in BehaviorSet.Enumerate())
            result |= behavior.CanRightClick(item);
        return result;
    }

    public override void RightClick(Item item, Player player)
    {
        foreach (GlobalItemBehavior behavior in BehaviorSet.Enumerate())
            behavior.RightClick(item, player);
    }

    public override void ModifyItemLoot(Item item, ItemLoot itemLoot)
    {
        foreach (GlobalItemBehavior behavior in BehaviorSet.Enumerate())
            behavior.ModifyItemLoot(item, itemLoot);
    }

    public override bool CanStack(Item destination, Item source)
    {
        bool result = true;
        foreach (GlobalItemBehavior behavior in BehaviorSet.Enumerate())
            result &= behavior.CanStack(destination, source);
        return result;
    }

    public override bool CanStackInWorld(Item destination, Item source)
    {
        bool result = true;
        foreach (GlobalItemBehavior behavior in BehaviorSet.Enumerate())
            result &= behavior.CanStackInWorld(destination, source);
        return result;
    }

    public override void OnStack(Item destination, Item source, int numToTransfer)
    {
        foreach (GlobalItemBehavior behavior in BehaviorSet.Enumerate())
            behavior.OnStack(destination, source, numToTransfer);
    }

    public override void SplitStack(Item destination, Item source, int numToTransfer)
    {
        foreach (GlobalItemBehavior behavior in BehaviorSet.Enumerate())
            behavior.SplitStack(destination, source, numToTransfer);
    }

    public override bool ReforgePrice(Item item, ref int reforgePrice, ref bool canApplyDiscount)
    {
        bool result = true;
        foreach (GlobalItemBehavior behavior in BehaviorSet.Enumerate())
            result &= behavior.ReforgePrice(item, ref reforgePrice, ref canApplyDiscount);
        return result;
    }

    public override bool CanReforge(Item item)
    {
        bool result = true;
        foreach (GlobalItemBehavior behavior in BehaviorSet.Enumerate())
            result &= behavior.CanReforge(item);
        return result;
    }

    public override void PreReforge(Item item)
    {
        foreach (GlobalItemBehavior behavior in BehaviorSet.Enumerate())
            behavior.PreReforge(item);
    }

    public override void PostReforge(Item item)
    {
        foreach (GlobalItemBehavior behavior in BehaviorSet.Enumerate())
            behavior.PostReforge(item);
    }

    public override void DrawArmorColor(EquipType type, int slot, Player drawPlayer, float shadow, ref Color color, ref int glowMask, ref Color glowMaskColor)
    {
        foreach (GlobalItemBehavior behavior in BehaviorSet.Enumerate())
            behavior.DrawArmorColor(type, slot, drawPlayer, shadow, ref color, ref glowMask, ref glowMaskColor);
    }

    public override void ArmorArmGlowMask(int slot, Player drawPlayer, float shadow, ref int glowMask, ref Color color)
    {
        foreach (GlobalItemBehavior behavior in BehaviorSet.Enumerate())
            behavior.ArmorArmGlowMask(slot, drawPlayer, shadow, ref glowMask, ref color);
    }

    public override void VerticalWingSpeeds(Item item, Player player, ref float ascentWhenFalling, ref float ascentWhenRising, ref float maxCanAscendMultiplier, ref float maxAscentMultiplier, ref float constantAscend)
    {
        foreach (GlobalItemBehavior behavior in BehaviorSet.Enumerate())
            behavior.VerticalWingSpeeds(item, player, ref ascentWhenFalling, ref ascentWhenRising, ref maxCanAscendMultiplier, ref maxAscentMultiplier, ref constantAscend);
    }

    public override void HorizontalWingSpeeds(Item item, Player player, ref float speed, ref float acceleration)
    {
        foreach (GlobalItemBehavior behavior in BehaviorSet.Enumerate())
            behavior.HorizontalWingSpeeds(item, player, ref speed, ref acceleration);
    }

    public override bool WingUpdate(int wings, Player player, bool inUse)
    {
        bool result = false;
        foreach (GlobalItemBehavior behavior in BehaviorSet.Enumerate())
            result |= behavior.WingUpdate(wings, player, inUse);
        return result;
    }

    public override void Update(Item item, ref float gravity, ref float maxFallSpeed)
    {
        foreach (GlobalItemBehavior behavior in BehaviorSet.Enumerate())
            behavior.Update(item, ref gravity, ref maxFallSpeed);
    }

    public override void PostUpdate(Item item)
    {
        foreach (GlobalItemBehavior behavior in BehaviorSet.Enumerate())
            behavior.PostUpdate(item);
    }

    public override void GrabRange(Item item, Player player, ref int grabRange)
    {
        foreach (GlobalItemBehavior behavior in BehaviorSet.Enumerate())
            behavior.GrabRange(item, player, ref grabRange);
    }

    public override bool GrabStyle(Item item, Player player)
    {
        bool result = false;
        foreach (GlobalItemBehavior behavior in BehaviorSet.Enumerate())
            result |= behavior.GrabStyle(item, player);
        return result;
    }

    public override bool CanPickup(Item item, Player player)
    {
        bool result = true;
        foreach (GlobalItemBehavior behavior in BehaviorSet.Enumerate())
            result &= behavior.CanPickup(item, player);
        return result;
    }

    public override bool OnPickup(Item item, Player player)
    {
        bool result = true;
        foreach (GlobalItemBehavior behavior in BehaviorSet.Enumerate())
            result &= behavior.OnPickup(item, player);
        return result;
    }

    public override bool ItemSpace(Item item, Player player)
    {
        bool result = false;
        foreach (GlobalItemBehavior behavior in BehaviorSet.Enumerate())
            result |= behavior.ItemSpace(item, player);
        return result;
    }

    public override Color? GetAlpha(Item item, Color lightColor)
    {
        foreach (GlobalItemBehavior behavior in BehaviorSet.Enumerate())
        {
            Color? result = behavior.GetAlpha(item, lightColor);
            if (result is not null)
                return result;
        }
        return null;
    }

    public override bool PreDrawInWorld(Item item, SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
    {
        bool result = true;
        foreach (GlobalItemBehavior behavior in BehaviorSet.Enumerate())
            result &= behavior.PreDrawInWorld(item, spriteBatch, lightColor, alphaColor, ref rotation, ref scale, whoAmI);
        return result;
    }

    public override void PostDrawInWorld(Item item, SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI)
    {
        foreach (GlobalItemBehavior behavior in BehaviorSet.Enumerate())
            behavior.PostDrawInWorld(item, spriteBatch, lightColor, alphaColor, rotation, scale, whoAmI);
    }

    public override bool PreDrawInInventory(Item item, SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
    {
        bool result = true;
        foreach (GlobalItemBehavior behavior in BehaviorSet.Enumerate())
            result &= behavior.PreDrawInInventory(item, spriteBatch, position, frame, drawColor, itemColor, origin, scale);
        return result;
    }

    public override void PostDrawInInventory(Item item, SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
    {
        foreach (GlobalItemBehavior behavior in BehaviorSet.Enumerate())
            behavior.PostDrawInInventory(item, spriteBatch, position, frame, drawColor, itemColor, origin, scale);
    }

    public override Vector2? HoldoutOffset(int type)
    {
        foreach (GlobalItemBehavior behavior in BehaviorSet.Enumerate())
        {
            Vector2? result = behavior.HoldoutOffset(type);
            if (result is not null)
                return result;
        }
        return null;
    }

    public override Vector2? HoldoutOrigin(int type)
    {
        foreach (GlobalItemBehavior behavior in BehaviorSet.Enumerate())
        {
            Vector2? result = behavior.HoldoutOrigin(type);
            if (result is not null)
                return result;
        }
        return null;
    }

    public override bool CanEquipAccessory(Item item, Player player, int slot, bool modded)
    {
        bool result = true;
        foreach (GlobalItemBehavior behavior in BehaviorSet.Enumerate())
            result &= behavior.CanEquipAccessory(item, player, slot, modded);
        return result;
    }

    public override bool CanAccessoryBeEquippedWith(Item equippedItem, Item incomingItem, Player player)
    {
        bool result = true;
        foreach (GlobalItemBehavior behavior in BehaviorSet.Enumerate())
            result &= behavior.CanAccessoryBeEquippedWith(equippedItem, incomingItem, player);
        return result;
    }

    public override void ExtractinatorUse(int extractType, int extractinatorBlockType, ref int resultType, ref int resultStack)
    {
        foreach (GlobalItemBehavior behavior in BehaviorSet.Enumerate())
            behavior.ExtractinatorUse(extractType, extractinatorBlockType, ref resultType, ref resultStack);
    }

    public override void CaughtFishStack(int type, ref int stack)
    {
        foreach (GlobalItemBehavior behavior in BehaviorSet.Enumerate())
            behavior.CaughtFishStack(type, ref stack);
    }

    public override bool IsAnglerQuestAvailable(int type)
    {
        bool result = true;
        foreach (GlobalItemBehavior behavior in BehaviorSet.Enumerate())
            result &= behavior.IsAnglerQuestAvailable(type);
        return result;
    }

    public override void AnglerChat(int type, ref string chat, ref string catchLocation)
    {
        foreach (GlobalItemBehavior behavior in BehaviorSet.Enumerate())
            behavior.AnglerChat(type, ref chat, ref catchLocation);
    }

    public override void AddRecipes()
    {
        foreach (GlobalItemBehavior behavior in BehaviorSet.Enumerate())
            behavior.AddRecipes();
    }

    public override bool PreDrawTooltip(Item item, ReadOnlyCollection<TooltipLine> lines, ref int x, ref int y)
    {
        bool result = true;
        foreach (GlobalItemBehavior behavior in BehaviorSet.Enumerate())
            result &= behavior.PreDrawTooltip(item, lines, ref x, ref y);
        return result;
    }

    public override void PostDrawTooltip(Item item, ReadOnlyCollection<DrawableTooltipLine> lines)
    {
        foreach (GlobalItemBehavior behavior in BehaviorSet.Enumerate())
            behavior.PostDrawTooltip(item, lines);
    }

    public override bool PreDrawTooltipLine(Item item, DrawableTooltipLine line, ref int yOffset)
    {
        bool result = true;
        foreach (GlobalItemBehavior behavior in BehaviorSet.Enumerate())
            result &= behavior.PreDrawTooltipLine(item, line, ref yOffset);
        return result;
    }

    public override void PostDrawTooltipLine(Item item, DrawableTooltipLine line)
    {
        foreach (GlobalItemBehavior behavior in BehaviorSet.Enumerate())
            behavior.PostDrawTooltipLine(item, line);
    }

    public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
    {
        foreach (GlobalItemBehavior behavior in BehaviorSet.Enumerate())
            behavior.ModifyTooltips(item, tooltips);
    }

    public override void SaveData(Item item, TagCompound tag)
    {
        foreach (GlobalItemBehavior behavior in BehaviorSet.Enumerate())
            behavior.SaveData(item, tag);
    }

    public override void LoadData(Item item, TagCompound tag)
    {
        foreach (GlobalItemBehavior behavior in BehaviorSet.Enumerate())
            behavior.LoadData(item, tag);
    }

    public override void NetSend(Item item, BinaryWriter writer)
    {
        if (TOSharedData.SyncEnabled)
        {
            //foreach (GlobalItemBehavior behavior in BehaviorSet.GetBehaviors())
            //    behavior.NetSend(item, writer);
        }
    }

    public override void NetReceive(Item item, BinaryReader reader)
    {
        if (TOSharedData.SyncEnabled)
        {
            //foreach (GlobalItemBehavior behavior in BehaviorSet.GetBehaviors())
            //    behavior.NetReceive(item, reader);
        }
    }
}

public sealed class BehaviorLoader : IContentLoader
{
    private static IEnumerable<IEntityBehavior> _allBehaviors;

    void IContentLoader.PostSetupContent()
    {
        _allBehaviors = TOReflectionUtils.GetTypeInstancesDerivedFrom<IEntityBehavior>();

        PlayerBehaviorHandler.BehaviorSet.FillSet(_allBehaviors.OfType<PlayerBehavior>());
        GlobalNPCBehaviorHandler.BehaviorSet.FillSet(_allBehaviors.OfType<GlobalNPCBehavior>());
        GlobalProjectileBehaviorHandler.BehaviorSet.FillSet(_allBehaviors.OfType<GlobalProjectileBehavior>());
        GlobalItemBehaviorHandler.BehaviorSet.FillSet(_allBehaviors.OfType<GlobalItemBehavior>());
    }
    void IContentLoader.OnModUnload()
    {
        PlayerBehaviorHandler.BehaviorSet.Clear();
        GlobalNPCBehaviorHandler.BehaviorSet.Clear();
        GlobalProjectileBehaviorHandler.BehaviorSet.Clear();
        GlobalItemBehaviorHandler.BehaviorSet.Clear();

        _allBehaviors = null;
    }
}
#endregion General Behavior Handler