namespace Anomalies.Bosses.Skeletron;

public sealed class Skull : AnomalyProjectileBehavior<Skull>
{
    // ai[0]: 本项目的行为协议；0/1 保留传说之夜的追踪/直线模式。
    public const int LegendaryHoming = 0;
    public const int LegendaryStraight = 1;
    public const int Homing = -1;
    public const int AcceleratingCrossbone = -2;
    public const int ExtendedHoming = -3;

    public override int ApplyingType => ProjectileID.Skull;

    // AI、颜色和骨片绘制均由本 Behavior 实现，不运行灾厄的对应覆盖。
    public override bool AllowCalamityLogic(CalamityLogicType_ProjectileBehavior type) => false;

    private bool IsHoming => Projectile.ai[0] is Homing or ExtendedHoming;
    private bool IsCrossbone => Projectile.ai[0] == AcceleratingCrossbone;
    private float HomingEndTime => (Ultra ? 105f : 90f) + (Projectile.ai[0] == ExtendedHoming ? 60f : 0f);

    public override bool PreAI()
    {
        bool regular = Projectile.ai[0] < 0f;
        // 追踪头骨直接显现；骨片保留每帧 75 的淡入速度。
        Projectile.alpha = regular && !IsHoming ? Math.Max(0, Projectile.alpha - 75) : 0;
        Projectile.frame = (Projectile.frame + 1) % 3;

        if (regular)
        {
            // 骨片先加速再生成尾迹；追踪头骨先生成尾迹再更新运动。
            if (IsCrossbone)
                Accelerate(Ultra ? 18f : 15f, 1.015f, true);
            EmitTrail();
            if (IsHoming)
            {
                HomingMovement();
                SpawnEffects(Projectile.Center);
            }
        }
        else
        {
            SpawnEffects(Projectile.position);
            if (Projectile.ai[0] == LegendaryHoming)
            {
                Projectile.ai[1]++;
                if (Projectile.ai[1] is > 30f and < 110f)
                    TurnTowardsPlayer(25f);
                Accelerate(18f, 1.02f, false);
            }
            EmitTrail();
        }

        Projectile.spriteDirection = Projectile.direction;
        if (IsCrossbone)
            Projectile.rotation += MathHelper.Pi / 90f * Projectile.velocity.Length() * Projectile.direction;
        else
            Projectile.rotation = (Projectile.velocity * (Projectile.direction < 0 ? -1f : 1f)).ToRotation();
        return false;

        void HomingMovement()
        {
            bool extended = Projectile.ai[0] == ExtendedHoming;
            int targetIndex = Player.FindClosest(Projectile.Center, 1, 1);
            Player target = Main.player[targetIndex];
            Projectile.ai[1]++;
            float stopDistance = extended ? (Ultra ? 192f : 120f) : 96f;
            if (Projectile.Distance(target.Center) < stopDistance && Projectile.ai[1] < HomingEndTime)
                Projectile.ai[1] = HomingEndTime;
            if (Projectile.ai[1] > (extended ? 10f : 30f) && Projectile.ai[1] < HomingEndTime)
                TurnTowardsPlayer(Ultra ? (extended ? 20f : 25f) : (extended ? 25f : 30f));
            Accelerate(Ultra ? 18f : 15f, Ultra ? 1.02f : 1.015f, false);
        }

        void TurnTowardsPlayer(float inertia)
        {
            Player target = Main.player[Player.FindClosest(Projectile.Center, 1, 1)];
            float speed = Projectile.velocity.Length();
            Vector2 direction = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitY);
            Vector2 velocity = (Projectile.velocity * (inertia - 1f) + direction * speed) / inertia;
            Projectile.velocity = velocity.SafeNormalize(direction) * speed;
        }

        void Accelerate(float maxSpeed, float acceleration, bool clamp)
        {
            if (Projectile.velocity.Length() >= maxSpeed)
                return;
            Projectile.velocity *= acceleration;
            if (clamp && Projectile.velocity.Length() > maxSpeed)
                Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.UnitY) * maxSpeed;
        }

        void SpawnEffects(Vector2 soundPosition)
        {
            if (Projectile.localAI[0] != 0f)
                return;
            Projectile.localAI[0] = 1f;
            SoundEngine.PlaySound(SoundID.Item8, soundPosition);
            for (int i = 0; i < 10; i++)
            {
                Dust.NewDustAction(Projectile.Center, Projectile.width, Projectile.height, DustID.Blood, Projectile.velocity, dust =>
                {
                    dust.scale *= 2f;
                    dust.noGravity = true;
                    dust.velocity = (Projectile.Center - dust.position).SafeNormalize(Vector2.UnitY) * -5f + Projectile.velocity / 2f;
                });
            }
        }

        void EmitTrail()
        {
            int count = IsCrossbone ? 1 : 2;
            int dustType = IsCrossbone ? 91 : regular && !IsHoming ? DustID.Torch : DustID.Blood;
            float scale = IsCrossbone ? 1f : regular && !IsHoming ? 2f : 1.5f;
            float offset = regular && !IsHoming && !IsCrossbone ? 2f : 1f;
            for (int i = 0; i < count; i++)
            {
                Dust.NewDustAction(Projectile.Center, Projectile.width - 8, Projectile.height - 8, dustType, Projectile.velocity * 0.2f, dust =>
                {
                    dust.alpha = 100;
                    dust.scale *= scale;
                    dust.position -= Projectile.velocity * offset;
                    dust.noGravity = true;
                    dust.velocity *= 0.3f;
                });
            }
        }
    }

    public override Color? GetAlpha(Color lightColor)
    {
        if (!IsHoming)
            return null;
        return Projectile.ai[1] >= HomingEndTime
            ? new Color(184, 140, 255, Projectile.alpha)
            : new Color(255, 255, 255, (int)Utils.WrappedLerp(0f, 255f, Projectile.timeLeft % 40 / 40f));
    }

    public override bool PreDraw(ref Color lightColor)
    {
        if (!IsCrossbone)
            return true;
        Main.instance.LoadProjectile(ProjectileID.BoneGloveProj);
        Texture2D texture = TextureAssets.Projectile[ProjectileID.BoneGloveProj].Value;
        Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, null, Projectile.GetAlpha(lightColor),
            Projectile.rotation, texture.Size() / 2f, Projectile.scale,
            Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None);
        return false;
    }
}
