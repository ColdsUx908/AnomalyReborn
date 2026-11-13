using Anomalies.Assets.Effects;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;

namespace Anomalies.Bosses.Skeletron.LegendaryNight;

/// <summary>普通圆形/圆环星爆的独立短时场景模糊；由当前有效实体推导，停止测试后自动失活。</summary>
public sealed class LegendStarburstPostProcessing : ModSystem
{
    private const string FilterKey = "Anomalies:LegendStarburstRadialBlur";
    private static Filter _Filter;
    private static StarburstShaderData _Data;

    public override void Load()
    {
        if (Main.dedServ)
            return;
        TOCleanup.Register(Clear);
    }

    public override void PostSetupContent()
    {
        if (Main.dedServ || !AnomalyClientConfig.Instance.EnableShaders
            || AnomalyEffects.ScreenFilters.LegendStarburstRadialBlur is not Filter filter)
            return;
        // Replace only this dedicated filter, retaining the unified asset owner.
        // The specialized Apply needs the Asset constructor. This one-time request reuses the
        // same repository-owned asset as AnomalyEffects, without a second static asset cache.
        Asset<Effect> asset = Mod.Assets.Request<Effect>("Assets/Effects_Compiled/ScreenFilters/LegendStarburstRadialBlur");
        _Data = new StarburstShaderData(asset);
        _Filter = new Filter(_Data, EffectPriority.VeryHigh);
        filter.Deactivate();
        Filters.Scene[FilterKey] = _Filter;
        AnomalyEffects.ScreenFilters.LegendStarburstRadialBlur = _Filter;
        _Filter.Load();
    }

    public override void PostUpdateEverything()
    {
        if (_Filter is null)
            return;
        if (AnomalyClientConfig.Instance.EnableShaders && TryGetBurst(out LegendStarburstVisuals.Burst frame))
        {
            // Arm capture at age zero, before the first nonzero blur sample. FilterManager's
            // scene fade must not gate this short impulse or omit frames after reactivation.
            _Data.UseIntensity(LegendStarburstCircleVisuals.BlurStrength(frame.Age));
            if (!_Filter.IsActive())
                Filters.Scene.Activate(FilterKey, frame.Center);
            _Filter.Opacity = 1f;
            _Data.UseGlobalOpacity(1f);
        }
        else
            Deactivate();
    }

    public override void ModifyScreenPosition()
    {
        if (TryGetBurst(out LegendStarburstVisuals.Burst frame))
            Main.screenPosition -= LegendStarburstCircleVisuals.ShakeOffset(frame.Age);
    }

    private static bool TryGetBurst(out LegendStarburstVisuals.Burst frame)
    {
        frame = default;
        if (Main.dedServ || Main.gameMenu)
            return false;
        bool found = false;
        foreach (Projectile projectile in Projectile.ActiveProjectiles)
            if (projectile.ModProjectile is LegendStarburst burst
                && burst.TryGetVisualFrame(out _, out LegendStarburstVisuals.Burst candidate)
                && candidate.Age < LegendStarburstCircleVisuals.BlurDuration
                && (!found || candidate.Age < frame.Age))
            {
                // The newest impact owns both camera and blur, including its zero-strength onset.
                frame = candidate;
                found = true;
            }
        return found;
    }

    public override void OnWorldUnload() => Deactivate();

    internal static void Deactivate()
    {
        if (_Data is not null)
        {
            _Data.UseIntensity(0f);
            _Data.UseGlobalOpacity(0f);
        }
        _Filter?.Deactivate();
        if (_Filter is not null)
            _Filter.Opacity = 0f;
    }

    private static void Clear()
    {
        Deactivate();
        _Data = null;
        _Filter = null;
    }

    private sealed class StarburstShaderData(Asset<Effect> effect) : ScreenShaderData(effect, "Pass0")
    {
        public override void Apply()
        {
            // Read the same live music age as drawing, after the camera shake and zoom.
            if (AnomalyClientConfig.Instance.EnableShaders && TryGetBurst(out LegendStarburstVisuals.Burst frame))
            {
                Vector2 screen = Vector2.Transform(frame.Center - Main.screenPosition, Main.GameViewMatrix.TransformationMatrix);
                UseDirection(screen / new Vector2(Main.screenWidth, Main.screenHeight));
                UseIntensity(LegendStarburstCircleVisuals.BlurStrength(frame.Age));
            }
            else
                UseIntensity(0f);
            base.Apply();
        }
    }
}
