namespace Anomalies.Bosses.Skeletron;

public sealed partial class Skull
{
    public override bool PreAI()
    {
        int headIndex = AnomalyProjectile.AnomalyAI32[SourceHeadSlot].I - 1;
        // 主体死亡后继续飞行，仅在仍存活的主体主动退场时清理。
        if ((uint)headIndex < Main.maxNPCs
            && Main.npc[headIndex] is { active: true, type: NPCID.SkeletronHead }
            && SkeletronHead.GetInstance(Main.npc[headIndex]) is
                { ShouldProcess: true, CurrentBehavior: SkeletronHead.Behavior.Despawn })
        {
            if (TONetworkEnvironment.NotClient)
            {
                Projectile.ai[2] = SuppressDeathEffectsMarker;
                Projectile.Kill();
            }
            return false;
        }
        if (Projectile.numUpdates == -1)
            Projectile.frame = (Projectile.frame + 1) % 3;
        Projectile.alpha = 0;

        switch (CurrentBehavior)
        {
            case Behavior.Homing:
                SpawnEffects(Projectile.position);
                UpdateHoming();
                Accelerate(useHomingDefaults: true);
                break;
            case Behavior.Straight:
                SpawnEffects(Projectile.position);
                Accelerate(useHomingDefaults: false);
                break;
        }

        if (!Main.dedServ)
        {
            if (HasBeamTrail)
                Projectile.localAI[1] = Math.Min(TrailLength, Projectile.localAI[1] + Projectile.velocity.Length());
            else
                EmitTrail();
        }
        Projectile.spriteDirection = Projectile.direction;
        Projectile.rotation = (Projectile.velocity * (Projectile.direction < 0 ? -1f : 1f)).ToRotation();
        return false;

        void Accelerate(bool useHomingDefaults)
        {
            AnomalyGlobalProjectile data = AnomalyProjectile;
            float maxSpeed = data.AnomalyAI32[MaxSpeedSlot].F;
            float acceleration = data.AnomalyAI32[AccelerationSlot].F;
            // 原版生成的头骨没有额外飞行参数，沿用默认追踪弹的加速。
            if (useHomingDefaults && acceleration == 0f)
            {
                maxSpeed = MaxSpeed;
                acceleration = Acceleration;
            }

            float speed = Projectile.velocity.Length();
            if (acceleration > 1f && speed > 0f && speed < maxSpeed)
                Projectile.velocity *= Math.Min(speed * acceleration, maxSpeed) / speed;
        }

        void UpdateHoming()
        {
            AnomalyGlobalProjectile data = AnomalyProjectile;
            int duration = data.AnomalyAI32[HomingDurationSlot].I;
            // Vanilla skulls have no explicit homing window.
            int startFrame = duration > 0 ? data.AnomalyAI32[HomingStartFrameSlot].I : DefaultHomingStartFrame;
            int endTime = startFrame + (duration > 0 ? duration : DefaultHomingDuration);
            if (Projectile.ai[1] >= endTime || ++Projectile.ai[1] >= endTime)
                return;

            Player target = Main.player[Player.FindClosest(Projectile.Center, 1, 1)];
            const float StopDistance = 96f;
            if (Projectile.Distance(target.Center) < StopDistance)
            {
                Projectile.ai[1] = endTime;
                if (TONetworkEnvironment.NotClient)
                    Projectile.netUpdate = true;
                return;
            }
            if (Projectile.ai[1] < startFrame)
                return;

            const float Inertia = 25f;
            float speed = Projectile.velocity.Length();
            Vector2 direction = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitY);
            Vector2 velocity = (Projectile.velocity * (Inertia - 1f) + direction * speed) / Inertia;
            Projectile.velocity = velocity.SafeNormalize(direction) * speed;
        }
    }
}
