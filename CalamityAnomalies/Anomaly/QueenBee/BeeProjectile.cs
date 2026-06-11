// Developed by ColdsUx

namespace CalamityAnomalies.Anomaly.QueenBee;

public sealed class BeeProjectile : CAModProjectile
{
    public const byte Behavior_HomeIn = 1;

    /* 数组使用约定
     * 
     * Projectile.ai
     * [0] 行为类型（默认值0表示无行为，直线运动）
     * [1] 特殊数据：
     *   若行为类型为1（追踪），表示追踪目标玩家的索引
     */

    public override string LocalizationCategory => "Anomaly.QueenBee";

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

        switch ((byte)Projectile.ai[0])
        {
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

            default:
                if (Timer1 >= 240)
                    Projectile.velocity *= 1.006f;
                break;
        }
    }
}
