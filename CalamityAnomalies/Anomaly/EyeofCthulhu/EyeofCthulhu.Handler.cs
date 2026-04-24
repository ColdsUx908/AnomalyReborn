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

public enum BehaviorCommand_Arena
{
    None = 0,

    Charge,
    EyeSpin,
    EyeSpinLast,
}

public enum BehaviorCommand_ArenaEye
{
    None = 0,

    ShootBlood_Charge,
    ShootBlood_EyeSpin,
}

public static class EyeofCthulhu_Handler
{
    public static class EyeShapeHelper
    {
        public static readonly float VerticalHeightMultiplier = MathF.Sqrt(49f / 75f); // 7/5√3 ≈ 0.80829
        public static readonly float RadiusMultiplier = CalculateRadiusMultiplier(VerticalHeightMultiplier);
        public static readonly float ArchHeightMultiplier = CalculateArchHeightMultiplier(VerticalHeightMultiplier);
        public static readonly float OffsetAngle = CalculateOffsetAngle(VerticalHeightMultiplier);
        public static readonly float CentralAngle = CalculateCentralAngle(VerticalHeightMultiplier);

        public static float CalculateRadiusMultiplier(float verticalHeightMultiplier) => MathF.Sqrt(1f + verticalHeightMultiplier * verticalHeightMultiplier);
        public static float CalculateArchHeightMultiplier(float verticalHeightMultiplier) => CalculateRadiusMultiplier(verticalHeightMultiplier) - verticalHeightMultiplier;
        public static float CalculateOffsetAngle(float verticalHeightMultiplier) => MathF.Atan(verticalHeightMultiplier);
        public static float CalculateCentralAngle(float verticalHeightMultiplier) => MathHelper.Pi - 2 * CalculateOffsetAngle(verticalHeightMultiplier);

        public static float InnerVelocityMultiplier => ArchHeightMultiplier * 0.75f;

        public static float VerticalHeightMultiplier2 => 0.585f;

        public static float CalculateVerticalHeightMultiplier(float archHeightMultiplier) => TOMathUtils.PolarEquation.Arc_CalculateVerticalDistance(archHeightMultiplier);

        public static Vector2 GetBaseVerticalVector(Vector2 originalVelocity, float? verticalHeightMultiplierOverride = null)
        {
            float verticalHeightMultiplier = verticalHeightMultiplierOverride ?? VerticalHeightMultiplier;
            return new(originalVelocity.Y * verticalHeightMultiplier, originalVelocity.X * -verticalHeightMultiplier);
        }

        public static Vector2 GetVector(Vector2 originalVector, float rotationOffset, float? verticalHeightMultiplierOverride = null)
        {
            rotationOffset = TOMathUtils.NormalizeWithPeriod(rotationOffset);

            bool shouldNotReverse;
            if (rotationOffset > MathHelper.Pi)
            {
                shouldNotReverse = false;
                rotationOffset -= MathHelper.Pi;
            }
            else
                shouldNotReverse = true;

            Vector2 baseVerticalVector = GetBaseVerticalVector(originalVector, verticalHeightMultiplierOverride);

            bool hasOverride = verticalHeightMultiplierOverride is not null;
            float overrideValue = hasOverride ? verticalHeightMultiplierOverride.Value : 0f;
            float offsetAngle = hasOverride ? CalculateOffsetAngle(overrideValue) : OffsetAngle;
            float centralAngle = hasOverride ? CalculateCentralAngle(overrideValue) : CentralAngle;
            float radiusMultiplier = hasOverride ? CalculateRadiusMultiplier(overrideValue) : RadiusMultiplier;

            return shouldNotReverse.ToDirectionInt() * (baseVerticalVector + originalVector.RotatedBy(offsetAngle + rotationOffset * centralAngle / MathHelper.Pi) * radiusMultiplier);
        }

        public static Vector2 GetVectorDirect(Vector2 originalVector, float rotationOffset, float? archHeightMultiplierOverride = null)
        {
            rotationOffset = TOMathUtils.NormalizeWithPeriod(rotationOffset);

            bool shouldNotReverse;
            if (rotationOffset > MathHelper.Pi)
            {
                shouldNotReverse = false;
                rotationOffset -= MathHelper.Pi;
            }
            else
                shouldNotReverse = true;

            float archHeightMultiplier = archHeightMultiplierOverride ?? ArchHeightMultiplier;

            return shouldNotReverse.ToDirectionInt() * (Vector2)new PolarVector2(originalVector.Length() * TOMathUtils.PolarEquation.Arc(rotationOffset, archHeightMultiplier), originalVector.ToRotation() + (archHeightMultiplier == 0f ? 0f : rotationOffset));
        }

        public static Vector2 GetInnerVector(Vector2 originalVector) => originalVector * InnerVelocityMultiplier;

        public static float GetOuterParticleScaleMultiplier(float rotationOffset)
        {
            rotationOffset = TOMathUtils.NormalizeWithPeriod(rotationOffset);
            return Utils.Remap(TOMathUtils.Min(rotationOffset, Math.Abs(rotationOffset - MathHelper.Pi), MathHelper.TwoPi - rotationOffset), 0f, MathHelper.PiOver2, 1.2f, 1f);
        }

        public static float InnerParticleScaleMultiplier => 0.85f;
    }

    public const string AnomalyEyeofCthulhuPath = "CalamityAnomalies/Anomaly/EyeofCthulhu/";

    [LoadTexture(AnomalyEyeofCthulhuPath + "BloodOrb")]

    private static Asset<Texture2D> _bloodOrbTexture;
    public static Texture2D BloodOrbTexture => _bloodOrbTexture.Value;

    [LoadTexture(AnomalyEyeofCthulhuPath + "BloodOrb_Border")]
    private static Asset<Texture2D> _bloodOrbBorderTexture;
    public static Texture2D BloodOrbBorderTexture => _bloodOrbBorderTexture.Value;

    [LoadTexture(AnomalyEyeofCthulhuPath + "BloodOrb_BorderBig")]
    private static Asset<Texture2D> _bloodOrbBorderBigTexture;
    public static Texture2D BloodOrbBorderBigTexture => _bloodOrbBorderBigTexture.Value;

    public static float MaxArenaRadius1 => 480f; //30格
    public static float MaxArenaRadius2 => 400f; //25格

    public static int NormalTeleportDuration => 90;
    public static int EyeSpinPhase1Time => 120;
    public static int EyeSpinPhase2Time => 20;

    public static readonly Color ChargeColor = Color.Lerp(Color.Red, Color.White, 0.75f);

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

    public static void ShootEyeProjectile(NPC npc, int type, int damage, Vector2 originalVelocity, int projectileAmountOver4, Action<Projectile> action = null)
    {
        int amount = projectileAmountOver4 * 4;
        float singleRadian = MathHelper.TwoPi / amount;

        Vector2 innerVector = EyeShapeHelper.GetInnerVector(originalVelocity);

        if (TOSharedData.GeneralClient)
        {
            for (int i = 0; i < amount; i++)
            {
                float rotationOffset = i * singleRadian;
                Vector2 velocity = EyeShapeHelper.GetVector(originalVelocity, rotationOffset);
                Vector2 velocity2 = innerVector.RotatedBy(rotationOffset);
                Projectile.NewProjectileAction(npc.GetSource_FromAI(), npc.Center, velocity, type, damage, 0f, action: action);
                Projectile.NewProjectileAction(npc.GetSource_FromAI(), npc.Center, velocity2, type, damage, 0f, action: action);
            }
        }
    }

    public static void SpawnEyeParticle(NPC npc, Vector2 originalVelocity)
    {
        int particleAmount = 4 * (int)Utils.Remap(originalVelocity.Length(), 20f, 40f, 120, 180);

        float singleRadian = MathHelper.TwoPi / particleAmount;

        Vector2 innerVector = EyeShapeHelper.GetInnerVector(originalVelocity);

        for (int i = 0; i < particleAmount; i++)
        {
            float rotationOffset = i * singleRadian;
            Vector2 velocity = EyeShapeHelper.GetVectorDirect(originalVelocity, rotationOffset);
            Vector2 velocity2 = innerVector.RotatedBy(rotationOffset);
            float scale = 0.8f;
            ParticleHandler.SpawnParticle(new OrbParticle(npc.Center, velocity, 45, scale, Color.Red, 0, scale * EyeShapeHelper.GetOuterParticleScaleMultiplier(rotationOffset), true, false));
            ParticleHandler.SpawnParticle(new OrbParticle(npc.Center, velocity2, 45, 0.7f, Color.Red, 0, scale * EyeShapeHelper.InnerParticleScaleMultiplier, true, false));
        }
    }
}
