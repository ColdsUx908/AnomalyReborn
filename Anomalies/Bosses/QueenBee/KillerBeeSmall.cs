namespace Anomalies.Bosses.QueenBee;

public sealed class KillerBeeSmall : AnomalyModProjectile
{
    /* 数组使用约定
     * 
     * Projectile.ai
     * [0] 安全蜂巢的索引（若为-1表示无安全蜂巢）
     * [1] 旋转的角速度（单位：弧度/帧）
     * [2] 仅在Dance攻击使用。为1表示圆圈舞，2表示摆尾舞
     */

    public override string LocalizationCategory => "Bosses.QueenBee";

    public override void SetStaticDefaults() => Main.projFrames[Projectile.type] = 4;

    public override void SetDefaults()
    {
        Projectile.width = 24;
        Projectile.height = 24;
        Projectile.hostile = true;
        Projectile.tileCollide = false;
        Projectile.timeLeft = 300;
        Projectile.scale = 0.8f;
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

        float velocityX = Projectile.velocity.X;
        float absX = Math.Abs(velocityX);
        Projectile.spriteDirection = (velocityX <= 0f).ToDirectionInt();
        Projectile.rotation = MathF.Atan2(Math.Clamp(Projectile.velocity.Y * 0.1f, -absX, absX), velocityX);
        if (Projectile.spriteDirection == 1)
            Projectile.rotation += MathHelper.Pi;

        int index = (int)Projectile.ai[0];
        if (Projectile.TryGetProjectileFromIndex(index, out Projectile safeCombCell) && safeCombCell.active && safeCombCell.ModProjectile is CombCell combCell)
        {
            if (combCell.BehaviorType == CombCell.Behavior.BeeSwarm2_Safe)
            {
                Hexagon hitBox = combCell.HitBox;
                hitBox.CircumRadius *= 1.1f;
                if (hitBox.Collides(Projectile.Hitbox))
                    Projectile.Kill();
                else if (safeCombCell.Timer2 > 10)
                {
                    Vector2 targetVelocity = safeCombCell.GetVelocityTowards(Projectile.Center, 15f);
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, targetVelocity, 0.08f);
                }
                else if (Timer1 is >= 30 and <= 120)
                    Projectile.velocity.Rotation += Projectile.ai[1];
            }
            else if (combCell.BehaviorType == CombCell.Behavior.Dance2)
            {
                Hexagon hitBox = combCell.HitBox;
                switch ((int)Projectile.ai[2])
                {
                    case 1:
                        if (!Projectile.honeyWet)
                            Projectile.Kill();
                        if (hitBox.Collides(Projectile.Hitbox))
                            Projectile.Kill();
                        return;
                    case 2:
                        if (Projectile.honeyWet)
                            Projectile.Kill();
                        hitBox.CircumRadius *= 0.9f;
                        if (hitBox.Collides(Projectile.Hitbox))
                            Projectile.Kill();
                        return;
                }

                Projectile.velocity *= 1.05f;
            }
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

    public override bool PreDraw(ref Color lightColor)
    {
        Main.spriteBatch.DrawFromCenter(Projectile.Texture, Projectile.Center - Main.screenPosition, Projectile.Texture.Frame(1, 4, 0, Projectile.frame), Color.White, Projectile.rotation, Projectile.scale, Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None);
        return false;
    }
}

