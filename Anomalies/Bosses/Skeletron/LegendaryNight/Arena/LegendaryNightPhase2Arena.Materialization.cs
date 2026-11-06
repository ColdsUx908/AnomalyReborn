namespace Anomalies.Bosses.Skeletron.LegendaryNight;

public sealed partial class LegendaryNightPhase2Arena
{
    public bool ChargeMaterialization => MasterBehavior is { Mechanic4Active: true };
    public bool Materialized => MasterBehavior is SkeletronLegendHead head && (ChargeMaterialization
        ? SkeletronLegendHead.Mechanic4Parameters.LasersActive(head.Phase2MusicElapsedTime)
        : head.Mechanic2Active && SkeletronLegendHead.Mechanic2Parameters.LasersActive(head.Mechanic2Age));
    public float MaterializationWarning => MasterBehavior is not SkeletronLegendHead head ? 0f : ChargeMaterialization
        ? SkeletronLegendHead.Mechanic4Parameters.LaserWarning(head.Phase2MusicElapsedTime)
        : head.Mechanic2Active ? SkeletronLegendHead.Mechanic2Parameters.LaserWarning(head.Mechanic2Age) : 0f;
    public float MaterializationIntensity => MasterBehavior is not SkeletronLegendHead head ? 0f : ChargeMaterialization
        ? SkeletronLegendHead.Mechanic4Parameters.LaserIntensity(head.Phase2MusicElapsedTime)
        : head.Mechanic2Active ? SkeletronLegendHead.Mechanic2Parameters.LaserIntensity(head.Mechanic2Age) : 0f;
    public Annulus MaterializedRing => new(Projectile.Center, MainCircleRadius, Radius);

    public bool MaterializedGeometryCollides(Rectangle hitbox)
    {
        // 金币冲刺同样实体化圆周与外围圆环，但不填充中央实心星形。
        if (!ChargeMaterialization && CentralStarCollides(hitbox)
            || MaterializedRing.Collides(hitbox)
            || new Annulus(Projectile.Center, Radius - LaserWidth * 0.5f, Radius + LaserWidth * 0.5f).Collides(hitbox))
            return true;
        for (int circle = 0; circle < 3; circle++)
        {
            (Vector2 center, float radius, float rotation) = GetMagicCircle(circle);
            if (MagicCircleCollides(center, radius, rotation, hitbox))
                return true;
        }
        return false;
    }
}