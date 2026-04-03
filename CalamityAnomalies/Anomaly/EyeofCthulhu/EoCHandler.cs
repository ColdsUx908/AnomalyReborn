using Transoceanic.Framework.Helpers.AbstractionHandlers;

namespace CalamityAnomalies.Anomaly.EyeofCthulhu;

public enum BehaviorCommand_Servant
{
    None = 0,

    ShootBlood,
    IncreaseFollowDistance,
    ReduceFollowDistance,
    GetToArenaPosition,
}

public enum BehaviorCommand_ArenaEye
{
    None = 0,

    ShootBlood,
    ShootBlood2,
}

public static class EoCHandler
{
    public static float MaxArenaRadius1 => 480f; //27格

    public static int NormalTeleportDuration => 90;
    public static void UpdateRotation(ref float rotation, float targetRotation, float rotationSpeed)
    {
        if (rotation != targetRotation)
        {
            float delta = TOMathUtils.ShortestDifference(rotation, targetRotation);
            if (Math.Abs(delta) < rotationSpeed)
                rotation = targetRotation;
            else
                rotation += Math.Sign(delta) * rotationSpeed;
        }
    }

    public static void ShootProjectile(NPC npc, int type, int damage, float speed, int amount, float halfRange, Action<Projectile> action = null)
    {
        float offset = npc.type == NPCID.EyeofCthulhu ? EyeofCthulhu_Anomaly.ProjectileOffset
            : npc.ModNPC is BloodlettingServant ? BloodlettingServant.ProjectileOffset
            : 0f;
        float rotationOffset = npc.type == NPCID.EyeofCthulhu ? MathHelper.PiOver2
            : npc.ModNPC is BloodlettingServant ? MathHelper.Pi
            : 0f;
        Vector2 projectileCenter = npc.Center + new PolarVector2(offset, npc.rotation + rotationOffset);
        Vector2 originalVelocity = (npc.PlayerTarget.Center - projectileCenter).ToCustomLength(speed);

        if (amount == 1)
            Projectile.NewProjectileAction(npc.GetSource_FromAI(), projectileCenter, originalVelocity, type, damage, 0f, action: action);
        else
            Projectile.RotatedProj(amount, halfRange * 2f / (amount - 1), npc.GetSource_FromAI(), projectileCenter, originalVelocity.RotatedBy(-halfRange), type, damage, 0f, action: action);
    }

    public static void SpawnOrbParticle(Vector2 center, float velocity, int lifetime, float scale)
    {
        Color color = Color.Lerp(Color.Red, Color.White, Main.rand.NextFloat(0.2f));
        ParticleHandler.SpawnParticle(new OrbParticle(center, Main.rand.NextPolarVector2(velocity), lifetime, scale, color, lifeEndRatio: 0.925f));
    }

    public static class EyeData
    {
        public static readonly float VerticalHeightMultiplier = MathF.Sqrt(49f / 75f); //7/5sqrt3 ≈ 0.80829
        public static readonly float RadiusMultiplier = MathF.Sqrt(124f / 75f);

        public static readonly float OffsetAngle = MathF.Atan(VerticalHeightMultiplier);
        public static readonly float CentralAngle = MathHelper.Pi - 2 * OffsetAngle;
    }

    public static void ShootEyeProjectile(NPC npc, int type, int damage, Vector2 originalVelocity, int projectileAmountOver4, Action<Projectile> action = null)
    {
        int amountPerArc = projectileAmountOver4 * 2;
        float singleRadian = EyeData.CentralAngle / amountPerArc;
        float singleRadianInner = MathHelper.Pi / amountPerArc;

        Vector2 baseVerticalVelocity = originalVelocity.RotatedBy(-MathHelper.PiOver2) * EyeData.VerticalHeightMultiplier;
        Vector2 adjustedVerticalVelocity = baseVerticalVelocity * (EyeData.RadiusMultiplier / EyeData.VerticalHeightMultiplier - 1f) * 0.75f;

        if (TOSharedData.GeneralClient)
        {
            for (int i = 0; i < amountPerArc; i++)
            {
                for (int j = -1; j <= 1; j += 2)
                {
                    Vector2 velocity = j * (baseVerticalVelocity + originalVelocity.RotatedBy(EyeData.OffsetAngle + singleRadian * i) * EyeData.RadiusMultiplier);
                    Vector2 velocity2 = j * adjustedVerticalVelocity.RotatedBy(i * singleRadianInner);
                    Projectile.NewProjectileAction(npc.GetSource_FromAI(), npc.Center, velocity, type, damage, 0f, action: action);
                    Projectile.NewProjectileAction(npc.GetSource_FromAI(), npc.Center, velocity2, type, damage, 0f, action: action);
                }
            }
        }
    }

    public static void SpawnEyeParticle(NPC npc, Vector2 originalVelocity)
    {
        const int particleAmountPerArc = 300;

        float singleRadian = EyeData.CentralAngle / particleAmountPerArc;
        float singleRadianInner = MathHelper.Pi / particleAmountPerArc;

        Vector2 baseVerticalVelocity = originalVelocity.RotatedBy(-MathHelper.PiOver2) * EyeData.VerticalHeightMultiplier;
        Vector2 adjustedVerticalVelocity = baseVerticalVelocity * (EyeData.RadiusMultiplier / EyeData.VerticalHeightMultiplier - 1f) * 0.75f;

        for (int i = 0; i < particleAmountPerArc; i++)
        {
            for (int j = -1; j <= 1; j += 2)
            {
                Vector2 velocity = j * (baseVerticalVelocity + originalVelocity.RotatedBy(EyeData.OffsetAngle + singleRadian * i) * EyeData.RadiusMultiplier);
                Vector2 velocity2 = j * adjustedVerticalVelocity.RotatedBy(i * singleRadianInner);
                ParticleHandler.SpawnParticle(new OrbParticle(npc.Center, velocity, 45, 0.8f, Color.Red, 0, 0.7f, true, false));
                ParticleHandler.SpawnParticle(new OrbParticle(npc.Center, velocity2, 45, 0.7f, Color.Red, 0, 0.7f, true, false));
            }
        }
    }
}
