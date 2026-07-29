// Developed by ColdsUx

namespace CalamityAnomalies.Anomaly.QueenBee;

public sealed class KillerBeeSmall : CAModProjectile
{
    /* 数组使用约定
     * 
     * Projectile.ai
     * [0] 安全蜂巢的索引（若为-1表示无安全蜂巢）
     * [1] 旋转的角速度（单位：弧度/帧）
     */

    public override string LocalizationCategory => "Anomaly.QueenBee";

    public override void SetStaticDefaults() => Main.projFrames[Projectile.type] = 4;

    public override void SetDefaults()
    {
        Projectile.width = 32;
        Projectile.height = 32;
        Projectile.hostile = true;
        Projectile.tileCollide = false;
        Projectile.timeLeft = 300;
        Projectile.scale = 0.7f;
    }

    public override void AI()
    {
        Timer1++;

        if (++Projectile.frameCounter >= 3)
        {
            Projectile.frameCounter = 0;
            if (++Projectile.frame >= 4)
                Projectile.frame = 0;
        }

        Projectile.spriteDirection = (Projectile.velocity.X <= 0f).ToDirectionInt();
        Projectile.rotation = Projectile.velocity.X * -0.02f;

        int index = (int)Projectile.ai[0];
        if (Projectile.TryGetProjectileFromIndex(index, out Projectile safeCombCell) && safeCombCell.active && safeCombCell.ModProjectile is CombCell combCell && combCell.BehaviorType == CombCell.Behavior.BeeSwarm2_Safe)
        {
            Hexagon hitBox = combCell.HitBox;
            hitBox.CircumRadius *= 1.1f;
            if (hitBox.Collides(Projectile.Hitbox))
                Projectile.Kill();
            else if (safeCombCell.Timer2 > 10)
            {
                float length = Projectile.velocity.Length();
                Vector2 targetVelocity = safeCombCell.GetVelocityTowards(Projectile.Center, 15f);
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, targetVelocity, 0.08f);
            }
            else if (Timer1 is >= 30 and <= 120)
                Projectile.velocity.Rotation += Projectile.ai[1];
        }

        Projectile.tileCollide = Timer1 >= 100;
    }

    public override bool CanHitPlayer(Player target)
    {
        int index = (int)Projectile.ai[0];
        if (Projectile.TryGetProjectileFromIndex(index, out Projectile safeCombCell) && safeCombCell.active && safeCombCell.ModProjectile is CombCell combCell && combCell.BehaviorType == CombCell.Behavior.BeeSwarm2_Safe && combCell.HitBox.Collides(Projectile.Hitbox))
            return false;

        return true;
    }
}
