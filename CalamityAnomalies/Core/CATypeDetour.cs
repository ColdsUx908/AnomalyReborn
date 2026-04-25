// Designed by ColdsUx

using CalamityMod.CalPlayer;
using CalamityMod.Items;
using CalamityMod.NPCs;
using CalamityMod.Projectiles;
using CalamityMod.Projectiles.BaseProjectiles;

namespace CalamityAnomalies.Core;

public abstract class BaseLaserbeamProjectileDetour<T> : ModProjectileDetour<T> where T : BaseLaserbeamProjectile
{
    // Behavior
    public delegate void Orig_Behavior(T self);
    public virtual void Detour_Behavior(Orig_Behavior orig, T self) => orig(self);

    // UpdateLaserMotion
    public delegate void Orig_UpdateLaserMotion(T self);
    public virtual void Detour_UpdateLaserMotion(Orig_UpdateLaserMotion orig, T self) => orig(self);

    // DetermineScale
    public delegate void Orig_DetermineScale(T self);
    public virtual void Detour_DetermineScale(Orig_DetermineScale orig, T self) => orig(self);

    // AttachToSomething
    public delegate void Orig_AttachToSomething(T self);
    public virtual void Detour_AttachToSomething(Orig_AttachToSomething orig, T self) => orig(self);

    // DetermineLaserLength
    public delegate float Orig_DetermineLaserLength(T self);
    public virtual float Detour_DetermineLaserLength(Orig_DetermineLaserLength orig, T self) => orig(self);

    // ExtraBehavior
    public delegate void Orig_ExtraBehavior(T self);
    public virtual void Detour_ExtraBehavior(Orig_ExtraBehavior orig, T self) => orig(self);

    // get_Lifetime
    public delegate float Orig_get_Lifetime(T self);
    public virtual float Detour_get_Lifetime(Orig_get_Lifetime orig, T self) => orig(self);

    // get_MaxScale
    public delegate float Orig_get_MaxScale(T self);
    public virtual float Detour_get_MaxScale(Orig_get_MaxScale orig, T self) => orig(self);

    // get_MaxLaserLength
    public delegate float Orig_get_MaxLaserLength(T self);
    public virtual float Detour_get_MaxLaserLength(Orig_get_MaxLaserLength orig, T self) => orig(self);

    // get_LaserBeginTexture
    public delegate Texture2D Orig_get_LaserBeginTexture(T self);
    public virtual Texture2D Detour_get_LaserBeginTexture(Orig_get_LaserBeginTexture orig, T self) => orig(self);

    // get_LaserMiddleTexture
    public delegate Texture2D Orig_get_LaserMiddleTexture(T self);
    public virtual Texture2D Detour_get_LaserMiddleTexture(Orig_get_LaserMiddleTexture orig, T self) => orig(self);

    // get_LaserEndTexture
    public delegate Texture2D Orig_get_LaserEndTexture(T self);
    public virtual Texture2D Detour_get_LaserEndTexture(Orig_get_LaserEndTexture orig, T self) => orig(self);

    // get_ScaleExpandRate
    public delegate float Orig_get_ScaleExpandRate(T self);
    public virtual float Detour_get_ScaleExpandRate(Orig_get_ScaleExpandRate orig, T self) => orig(self);

    // get_LightCastColor
    public delegate Color Orig_get_LightCastColor(T self);
    public virtual Color Detour_get_LightCastColor(Orig_get_LightCastColor orig, T self) => orig(self);

    // get_LaserOverlayColor
    public delegate Color Orig_get_LaserOverlayColor(T self);
    public virtual Color Detour_get_LaserOverlayColor(Orig_get_LaserOverlayColor orig, T self) => orig(self);

    public override void ApplyDetour()
    {
        base.ApplyDetour();
        ApplySingleDetour(Detour_Behavior);
        ApplySingleDetour(Detour_UpdateLaserMotion);
        ApplySingleDetour(Detour_DetermineScale);
        ApplySingleDetour(Detour_AttachToSomething);
        ApplySingleDetour(Detour_DetermineLaserLength);
        ApplySingleDetour(Detour_ExtraBehavior);
        ApplySingleDetour(Detour_get_Lifetime);
        ApplySingleDetour(Detour_get_MaxScale);
        ApplySingleDetour(Detour_get_MaxLaserLength);
        ApplySingleDetour(Detour_get_LaserBeginTexture);
        ApplySingleDetour(Detour_get_LaserMiddleTexture);
        ApplySingleDetour(Detour_get_LaserEndTexture);
        ApplySingleDetour(Detour_get_ScaleExpandRate);
        ApplySingleDetour(Detour_get_LightCastColor);
        ApplySingleDetour(Detour_get_LaserOverlayColor);
    }
}

public abstract class CalamityPlayerDetour : ModPlayerDetour<CalamityPlayer>
{
    // MiscEffects
    public delegate void Orig_MiscEffects(CalamityPlayer self);
    public virtual void Detour_MiscEffects(Orig_MiscEffects orig, CalamityPlayer self) { }

    // OtherBuffEffects
    public delegate void Orig_OtherBuffEffects(CalamityPlayer self);
    public virtual void Detour_OtherBuffEffects(Orig_OtherBuffEffects orig, CalamityPlayer self) { }

    public override void ApplyDetour()
    {
        base.ApplyDetour();
        ApplySingleDetour(Detour_MiscEffects);
        ApplySingleDetour(Detour_OtherBuffEffects);
    }
}

public abstract class CalamityGlobalNPCDetour : GlobalNPCDetour<CalamityGlobalNPC> { }

public abstract class CalamityGlobalProjectileDetour : GlobalProjectileDetour<CalamityGlobalProjectile> { }

public abstract class CalamityGlobalItemDetour : GlobalItemDetour<CalamityGlobalItem> { }