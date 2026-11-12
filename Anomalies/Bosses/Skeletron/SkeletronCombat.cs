namespace Anomalies.Bosses.Skeletron;

internal static class SkeletronCombat
{
    internal static void FireAcceleratingSpread(NPC source, Player target, int count, float spreadDegrees, Action<Vector2, Vector2> fire)
    {
        Vector2 baseVelocity = (target.Center - source.Center).SafeNormalize(Vector2.UnitY) * 6f;
        Vector2 position = source.Center + baseVelocity * 5f;
        float halfSpread = spreadDegrees * 0.5f;
        float centerIndex = (count - 1) * 0.5f;
        for (int i = 0; i < count; i++)
        {
            float angle = MathHelper.ToRadians(MathHelper.Lerp(-halfSpread, halfSpread, i / (count - 1f)));
            float speedMultiplier = MathHelper.Lerp(0.5f, 1.5f, MathF.Abs(centerIndex - i) / centerIndex);
            fire(position, baseVelocity.RotatedBy(angle) * speedMultiplier);
        }
    }

    public static void FireSkull(NPC npc, Player target, int amount, float radian, float skullProjSpeed, int damage, bool fireFromMouth = true, bool addNPCVelocity = true, int mode = Skull.Homing)
    {
        Vector2 velocity = npc.GetVelocityTowards(target, skullProjSpeed) + (addNPCVelocity ? npc.velocity : Vector2.Zero);
        Vector2 position = npc.Center + velocity;
        if (fireFromMouth)
            position += new Vector2(0f, 25f * npc.scale).RotatedBy(npc.rotation);

        int type = ProjectileID.Skull;

        if (amount == 1)
            Projectile.NewProjectileAction(npc.GetSource_FromAI(), position, velocity, type, damage, 0f, action: FireSkullAction);
        else
            Projectile.NewProjectilesArc(amount, radian, npc.GetSource_FromAI(), position, velocity, type, damage, 0f, action: FireSkullAction);

        void FireSkullAction(Projectile p)
        {
            Skull.InitializeFlight(p, mode, Skull.MaxSpeed, Skull.Acceleration);
            Skull.SetSource(p, npc);
            p.timeLeft = 600;
        }
    }
}
