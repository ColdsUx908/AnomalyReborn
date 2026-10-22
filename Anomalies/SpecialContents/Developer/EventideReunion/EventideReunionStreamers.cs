namespace Anomalies.SpecialContents.Developer.EventideReunion;

/* 暂时禁用飘带行为，保留实现以便恢复。
// Keep cloth momentum across the short-lived holdouts of a continuous attack.
// These client-only points are cosmetic and never participate in damage or networking.
public sealed class EventideReunionStreamers : AnomalyPlayerBehavior
{
    private const int PointCount = 33;
    private const int Substeps = 4;
    private const float Gravity = 1.2f;
    private const float VelocityRetention = 0.86f;

    internal void Update(Projectile sourceProjectile)
    {
        if (AnomalyPlayer.EventideReunionStreamerInitialized && AnomalyPlayer.EventideReunionStreamerLastUpdate == Main.GameUpdateCount)
            return;

        float rotation = sourceProjectile.rotation + MathHelper.PiOver4;
        Vector2 purpleAnchor = sourceProjectile.Center + new Vector2(22f, -62f).RotatedBy(rotation) * sourceProjectile.scale;
        Vector2 orangeAnchor = sourceProjectile.Center + new Vector2(21f, -20f).RotatedBy(rotation) * sourceProjectile.scale;
        bool reset = !AnomalyPlayer.EventideReunionStreamerInitialized || Main.GameUpdateCount - AnomalyPlayer.EventideReunionStreamerLastUpdate > 2
            || Vector2.DistanceSquared(AnomalyPlayer.EventideReunionStreamerAnchors[0], purpleAnchor) > 400f * 400f;
        AnomalyPlayer.EventideReunionStreamerLastUpdate = Main.GameUpdateCount;
        AnomalyPlayer.EventideReunionStreamerInitialized = true;

        for (int ribbon = 0; ribbon < 2; ribbon++)
        {
            Vector2 target = ribbon == 0 ? purpleAnchor : orangeAnchor;
            Vector2 start = reset ? target : AnomalyPlayer.EventideReunionStreamerAnchors[ribbon];
            Vector2[] points = AnomalyPlayer.EventideReunionStreamerPositions[ribbon];
            Vector2[] old = AnomalyPlayer.EventideReunionStreamerPrevious[ribbon];
            float spacing = (ribbon == 0 ? 235f : 170f) * sourceProjectile.scale / (PointCount - 1);
            if (reset)
            {
                // Spawn hanging down in world space, independently of the blade angle.
                Vector2 tangent = Vector2.UnitY;
                for (int i = 0; i < PointCount; i++)
                    points[i] = old[i] = target + tangent * (spacing * i);
            }

            Vector2 rootMotion = (target - start) / Substeps;
            float motionRatio = MathHelper.Clamp((target - start).Length() / (24f * sourceProjectile.scale), 0f, 1f);
            float follow = MathHelper.Lerp(0.8f, 0.94f, motionRatio);
            float damping = MathHelper.Lerp(VelocityRetention, 0.78f, motionRatio);
            for (int step = 0; step < Substeps; step++)
            {
                Vector2 root = Vector2.Lerp(start, target, (step + 1f) / Substeps);
                points[0] = root;
                for (int i = 1; i < PointCount; i++)
                {
                    // Carry most of the anchor translation into both Verlet positions.
                    // This preserves the hanging silhouette without injecting velocity;
                    // the remaining motion still produces a small physical lag.
                    points[i] += rootMotion * follow;
                    old[i] += rootMotion * follow;
                    Vector2 velocity = (points[i] - old[i]) * damping;
                    old[i] = points[i];
                    // Air drag approaches the local wind velocity. Underground cloth is sheltered.
                    float wind = points[i].Y < Main.worldSurface * 16f ? Main.windSpeedCurrent : 0f;
                    Vector2 airVelocity = new(wind * 4f / Substeps, 0f);
                    Vector2 acceleration = new Vector2(0f, Gravity * sourceProjectile.scale) / (Substeps * Substeps);
                    acceleration += (airVelocity - velocity) * 0.025f;
                    points[i] += velocity + acceleration;
                }

                // Position-based distance constraints transfer motion from the pinned root
                // through the cloth, while the free end retains its world-space inertia.
                for (int iteration = 0; iteration < 16; iteration++)
                {
                    points[0] = root;
                    for (int i = 1; i < PointCount; i++)
                    {
                        Vector2 delta = points[i] - points[i - 1];
                        float length = delta.Length();
                        if (length < 0.0001f)
                            continue;
                        Vector2 correction = delta * ((length - spacing) / length);
                        if (i == 1)
                            points[i] -= correction;
                        else
                        {
                            points[i - 1] += correction * 0.5f;
                            points[i] -= correction * 0.5f;
                        }
                    }
                }
            }
            AnomalyPlayer.EventideReunionStreamerAnchors[ribbon] = target;
        }
    }

    internal void Draw(float currentScale)
    {
        if (!AnomalyPlayer.EventideReunionStreamerInitialized || Main.GameUpdateCount - AnomalyPlayer.EventideReunionStreamerLastUpdate > 1)
            return;

        Texture2D pixel = TextureAssets.MagicPixel.Value;
        // MagicPixel is a texture, not necessarily a 1x1 image. Scale only a single
        // texel so the simulated segment length and width remain world-pixel sizes.
        Rectangle pixelSource = new(0, 0, 1, 1);
        for (int ribbon = 0; ribbon < 2; ribbon++)
        {
            Vector2[] points = AnomalyPlayer.EventideReunionStreamerPositions[ribbon];
            for (int i = 1; i < PointCount; i++)
            {
                float currentProgress = (i - 0.5f) / (PointCount - 1);
                Vector2 delta = points[i] - points[i - 1];
                float angle = delta.ToRotation();
                float width = (ribbon == 0 ? 12f : 10f) * currentScale
                    * MathHelper.Lerp(1f, 0.3f, MathF.Pow(currentProgress, 3f));
                float fold = 0.82f + 0.18f * MathF.Sin(angle * 2f + currentProgress * 9f);
                width *= fold;
                Color edge = ribbon == 0 ? new Color(65, 40, 112) : new Color(213, 87, 19);
                Color face = ribbon == 0 ? new Color(174, 126, 226) : Color.Lerp(new Color(255, 174, 55), new Color(255, 248, 214), currentProgress);
                Color light = Lighting.GetColor((int)(points[i].X / 16f), (int)(points[i].Y / 16f));
                Color tint = Color.Lerp(light, Color.White, 0.55f);
                Vector2 position = points[i - 1] - Main.screenPosition;
                // Layered flat strips preserve the sprite's colored border and pale inner fold.
                Main.spriteBatch.Draw(pixel, position, pixelSource, edge.MultiplyRGBA(tint), angle,
                    new Vector2(0f, 0.5f), new Vector2(delta.Length() + 1f, width), SpriteEffects.None, 0f);
                Main.spriteBatch.Draw(pixel, position, pixelSource, face.MultiplyRGBA(tint) * fold, angle,
                    new Vector2(0f, 0.5f), new Vector2(delta.Length() + 1f, width * 0.65f), SpriteEffects.None, 0f);
            }
        }
    }
}
*/
