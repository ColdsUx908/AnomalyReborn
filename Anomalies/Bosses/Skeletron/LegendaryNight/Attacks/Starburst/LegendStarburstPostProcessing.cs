using Anomalies.Assets.Effects;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Transoceanic.Graphics.Drawing;

namespace Anomalies.Bosses.Skeletron.LegendaryNight;

/// <summary>普通圆形/圆环星爆的短时场景模糊；由当前有效实体推导，攻击结束后自动失活。</summary>
public sealed class LegendStarburstPostProcessing : ModSystem
{
    private const string FilterKey = "Anomalies:LegendStarburstRadialBlur";
    private static Filter _Filter;
    private static AnomalyScreenShaderData _Data;

    public override void PostSetupContent()
    {
        Filter filter = AnomalyEffects.ScreenFilters.LegendStarburstRadialBlur;
        if (Main.dedServ || filter?.GetShader() is not AnomalyScreenShaderData data)
            return;
        TOCleanup.Register(Clear);
        _Filter = filter;
        _Data = data;
        data.BeforeApply = ApplyParameters;
    }

    public override void PostUpdateEverything()
    {
        if (_Filter is null)
            return;
        if (AnomalyClientConfig.Instance.EnableShaders && TryGetBurst(out LegendStarburstVisuals.Burst frame))
        {
            // Arm capture at age zero, before the first nonzero blur sample. FilterManager's
            // scene fade must not gate this short impulse or omit frames after reactivation.
            _Data.UseIntensity(LegendStarburstVisuals.BlurStrength(frame.Age));
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
            Main.screenPosition -= LegendStarburstVisuals.ShakeOffset(frame.Age);
    }

    private static bool TryGetBurst(out LegendStarburstVisuals.Burst frame)
    {
        frame = default;
        if (Main.dedServ || Main.gameMenu)
            return false;
        bool found = false;
        foreach (Projectile projectile in Projectile.ActiveProjectiles)
            if (projectile.ModProjectile is LegendStarburst burst
                && burst.TryGetVisualFrame(out LegendStarburstVisuals.Burst candidate)
                && candidate.Age < LegendStarburstVisuals.BlurDuration
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
        if (_Data is not null)
            _Data.BeforeApply = null;
        _Data = null;
        _Filter = null;
    }

    private static void ApplyParameters(ScreenShaderData data)
    {
        // Read the same live music age as drawing, after the camera shake and zoom.
        if (AnomalyClientConfig.Instance.EnableShaders && TryGetBurst(out LegendStarburstVisuals.Burst frame))
        {
            WorldView2D view = new(Main.screenPosition, Main.GameViewMatrix.TransformationMatrix,
                new Vector2(Main.screenWidth, Main.screenHeight));
            data.UseDirection(view.WorldToUv(frame.Center));
            data.UseIntensity(LegendStarburstVisuals.BlurStrength(frame.Age));
        }
        else
            data.UseIntensity(0f);
    }
}
