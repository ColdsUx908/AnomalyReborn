using Anomalies.Assets.Effects;

namespace Anomalies.Bosses.Skeletron.LegendaryNight;

public sealed partial class LegendaryNightOverlay
{
    // Normalize attack envelopes before merging them per pixel.
    private const float PeakFlameStrength = 1f;
    private const float Unbounded = 1_000_000f;
    private BlendState ImpactUnion { get; }
    // Shape packs outer radius, inner radius, upper projection limit, lower projection limit.
    private readonly List<(Vector2 Center, Vector4 Shape, Vector2 Axis, float Strength)> _Impacts = [];

    public LegendaryNightOverlay()
    {
        if (Main.dedServ)
            return;

        // Max coverage merges the silhouettes independently of submission order.
        ImpactUnion = new BlendState
        {
            ColorBlendFunction = BlendFunction.Max,
            ColorSourceBlend = Blend.One,
            ColorDestinationBlend = Blend.One
        };
        TOCleanup.Register(ImpactUnion.Dispose);
    }

    internal static bool UseImpactFlame => AnomalyClientConfig.Instance.EnableShaders
        && AnomalyEffects.Overlays.LegendImpactFlame?.Asset.IsLoaded == true;

    private void DrawImpactFlame(SpriteBatch spriteBatch, LegendaryNightPhase2Arena arena)
    {
        if (!UseImpactFlame)
            return;

        _Impacts.Clear();
        SkeletronLegendHead head = arena?.MasterBehavior;
        foreach (Projectile projectile in Projectile.ActiveProjectiles)
        {
            switch (projectile.ModProjectile)
            {
                case CoinJudgment judgment when judgment.OverlayOpacity > 0f:
                    AddCoinImpact(judgment);
                    break;
                // Each owner lookup creates a new behavior wrapper around the same NPC.
                // 普通星爆的独立圆形/圆环绘制不并入共享火焰。
                case LegendStarburst burst when head is not null && burst.Head?.NPC == head.NPC && !burst.UsesCircleVisuals && !burst.UsesRingVisuals
                    && burst.TryGetImpact(out Vector2 center, out Vector2 radii, out float opacity):
                    AddRing(center, radii.X, radii.Y, opacity * (PeakFlameStrength / LegendStarburst.ImpactMaxOpacity));
                    break;
            }
        }

        if (head is { Mechanic2Active: true } && head.Mechanic2HandPattern != 0)
        {
            for (int strike = 0; strike < SkeletronLegendHead.Mechanic2Parameters.StrikeCount; strike++)
            {
                int age = head.Mechanic2Age - SkeletronLegendHead.Mechanic2Parameters.GetStrikeTime(strike);
                float opacity = SkeletronLegendHead.Mechanic2Parameters.HalfFieldOpacity(age);
                if (opacity > 0f)
                    _Impacts.Add((arena.Projectile.Center, new Vector4(arena.Radius, -Unbounded, Unbounded, 0f),
                        Vector2.UnitX * head.Mechanic2StrikeSide(strike),
                        opacity * (PeakFlameStrength / SkeletronLegendHead.Mechanic2Parameters.HalfFieldMaxOpacity)));
            }
        }

        if (_Impacts.Count == 0)
            return;

        Effect effect = AnomalyEffects.Overlays.LegendImpactFlame.Asset.Value;
        Matrix inverseView = Matrix.Invert(Main.GameViewMatrix.TransformationMatrix);
        Rectangle screen = new(0, 0, Main.screenWidth, Main.screenHeight);
        effect.Parameters["uInverseViewMatrix"].SetValue(inverseView);
        effect.Parameters["uScreenPosition"].SetValue(Main.screenPosition);
        effect.Parameters["uResolution"].SetValue(new Vector2(screen.Width, screen.Height));
        using RenderTargetLease lease = RenderTargetPool.Shared.Rent(Main.instance.GraphicsDevice,
            screen.Width, screen.Height, RenderTargetDescriptor.Default);
        using (lease.Scope(clearColor: Color.Transparent))
        {
            spriteBatch.Begin(SpriteSortMode.Immediate, ImpactUnion, SamplerState.LinearClamp,
                DepthStencilState.None, Main.Rasterizer, null, Matrix.Identity);
            foreach ((Vector2 center, Vector4 shape, Vector2 axis, float strength) in _Impacts)
            {
                effect.Parameters["uCenter"].SetValue(center);
                effect.Parameters["uShape"].SetValue(shape);
                effect.Parameters["uAxis"].SetValue(axis);
                effect.Parameters["uStrength"].SetValue(strength);
                effect.CurrentTechnique.Passes["Coverage"].Apply();
                spriteBatch.Draw(TOTextures.FillerTexture, screen, Color.White);
            }
            spriteBatch.End();
        }

        effect.Parameters["uStarColor"].SetValue(LegendaryNightHandler.StarColor.ToVector3());
        effect.Parameters["uFlowTime"].SetValue((float)Main.timeForVisualEffects / 60f);
        spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp,
            DepthStencilState.None, Main.Rasterizer, null, Matrix.Identity);
        effect.CurrentTechnique.Passes["Pass0"].Apply();
        spriteBatch.Draw(lease.Target, screen, Color.White);
        spriteBatch.End();

        void AddRing(Vector2 center, float outer, float inner, float opacity)
        {
            _Impacts.Add((center, new Vector4(outer, inner > 0f ? inner : -Unbounded, Unbounded, -Unbounded),
                Vector2.Zero, opacity));
        }

        void AddCoinImpact(CoinJudgment judgment)
        {
            Vector2 center = judgment.Projectile.Center;
            float opacity = judgment.OverlayOpacity * (PeakFlameStrength / CoinJudgment.OverlayMaxOpacity);
            int coin = (int)judgment.Projectile.ai[0];
            switch (coin)
            {
                case 0:
                    AddRing(center, judgment.EffectiveCopperRadius, 0f, opacity);
                    break;
                case 1:
                case 3:
                    Vector2 axis = CoinJudgment.CrossRotation(coin).ToRotationVector2();
                    float halfWidth = judgment.CrossHalfWidth;
                    Vector4 strip = new(Unbounded, -Unbounded, halfWidth, -halfWidth);
                    _Impacts.Add((center, strip, axis, opacity));
                    _Impacts.Add((center, strip, new Vector2(-axis.Y, axis.X), opacity));
                    break;
                case 2:
                    AddRing(center, CoinJudgment.GoldOuterRadius, judgment.EffectiveGoldInnerRadius, opacity);
                    break;
            }
        }
    }
}
