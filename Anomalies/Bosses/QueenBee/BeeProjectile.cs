// Developed by ColdsUx

namespace Anomalies.Bosses.QueenBee;

public sealed class BeeProjectile : AnomalyModProjectile
{
    public const byte Behavior_Accelerate = 1;
    public const byte Behavior_HomeIn = 2;
    public const byte Behavior_Rotate = 3;
    public const byte Behavior_KilledByHoney = 4;

    /* 数组使用约定
     * 
     * Projectile.ai
     * [0] 行为类型（默认值0表示无行为，直线运动）
     * [1] 特殊数据：
     *   若行为类型为2（追踪），表示追踪目标玩家的索引
     *   若行为类型为3（旋转），表示旋转的角速度（单位：弧度/帧）
     * [2] 特殊数据：
     *   若行为类型为3（旋转），表示旋转的角速度加速时间（单位：帧）
     */

    public override string LocalizationCategory => "Bosses.QueenBee";

    public override string Texture => TOAssetUtils.FormatVanillaProjectileTexturePath(ProjectileID.GiantBee);

    public override void SetStaticDefaults() => Main.projFrames[Projectile.type] = 4;

    public override void SetDefaults()
    {
        Projectile.width = 16;
        Projectile.height = 16;
        Projectile.hostile = true;
        Projectile.tileCollide = false;
        Projectile.timeLeft = 450;
    }

    public override void AI()
    {
        Timer1++;

        if (++Projectile.frameCounter >= 3)
        {
            Projectile.frameCounter = 0;
            if (++Projectile.frame >= 3)
                Projectile.frame = 0;
        }

        Projectile.spriteDirection = (Projectile.velocity.X >= 0f).ToDirectionInt();
        Projectile.rotation = Projectile.velocity.X * 0.03f;

        switch ((byte)Projectile.ai[0])
        {
            case Behavior_Accelerate:
                if (Timer1 == 120)
                    Projectile.Kill();
                else if (Timer1 <= 10)
                    Projectile.velocity *= 1.41f; //1.41^10 ≈ 31.06

                break;

            case Behavior_HomeIn:
                switch (Timer1)
                {
                    case < 90:
                        int index = (int)Projectile.ai[1];
                        if (index is >= 0 and < Main.maxPlayers)
                        {
                            Player target = Main.player[index];

                            if (target.Alive)
                                Projectile.HomeIn(target, homingRatio: 0.2f, maxHomingDistance: 1600f, keepVelocity: false);
                            else
                                Projectile.Timer1 = 119;
                        }
                        else
                            Projectile.ai[0] = 0; //恢复到正常行为
                        break;

                    case 120:
                        if (Projectile.velocity == Vector2.Zero)
                            Projectile.Kill();
                        break;

                    case 240:
                        Projectile.Kill();
                        break;
                }
                Projectile.velocity *= 1.008f;
                break;

            case Behavior_Rotate:
                if (Timer1 is >= 30 and <= 90)
                {
                    float rotationSpeed = Projectile.ai[1];
                    if (Projectile.ai[2] != 0f && Timer1 <= 30 + Projectile.ai[2])
                        rotationSpeed *= (Timer1 - 30) / Projectile.ai[2];
                    Projectile.velocity.Rotation += rotationSpeed;
                }

                break;

            case Behavior_KilledByHoney:
                if (Projectile.honeyWet)
                    Projectile.Kill();

                if (Timer1 >= 40)
                {
                    Projectile.ai[0] = Behavior_Rotate;
                    Timer1 = 30;
                }
                break;

            default:
                if (Timer1 >= 240)
                    Projectile.velocity *= 1.005f;
                break;
        }

        Projectile.tileCollide = Timer1 >= 100;
    }
}
