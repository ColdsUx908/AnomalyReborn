namespace Anomalies.Bosses.QueenBee;

public sealed class FriendlyBee : AnomalyModProjectile
{
    /*
    public static class DanceHelper
    {
        public const float TotalTime = 90f; //动作总耗时80帧

        public static Vector2 GetOffset_Round(int timer, float radius) => timer switch
        {
            <= 40 => new Vector2(0f, -radius) + new PolarVector2(Utils.Remap(timer, 0, 40, radius, radius * 0.875f), MathHelper.PiOver2 + timer / 40f * MathHelper.TwoPi), //第一段：顺时针圈
            <= 50 => new PolarVector2(radius * 0.125f, -MathHelper.PiOver2 - (timer - 40) / 10f * MathHelper.Pi), //第二段：小半圆
            <= 90 => new Vector2(0f, -radius) + new PolarVector2(Utils.Remap(timer, 50, 90, radius * 1.125f, radius), MathHelper.PiOver2 - (timer - 50) / 40f * MathHelper.TwoPi), //第三段：逆时针圈
            _ => Vector2.Zero
        };
    }
    */

    public const byte Behavior_SwarmReminder = 1;
    public const byte Behavior_Pet = byte.MaxValue;

    /* 数组使用约定
     * 
     * Projectile.ai
     * [0] 行为类型（默认值0表示无行为，立即消失）
     * [1] 特殊数据：
     *   若行为类型为1（蜂群攻击方向指示），表示蜂群攻击的方向（即 Boss 此时的攻击随机变量）
     */

    public override string LocalizationCategory => "Bosses.QueenBee";

    public override void SetStaticDefaults() => Main.projFrames[Type] = 2;

    public override void SetDefaults()
    {
        Projectile.width = 32;
        Projectile.height = 32;
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.tileCollide = false;
        Projectile.timeLeft = 1200;
        Projectile.netImportant = true;
    }

    public override void AI()
    {
        Timer1++;

        if (++Projectile.frameCounter >= 5)
        {
            Projectile.frameCounter = 0;
            if (++Projectile.frame >= 2)
                Projectile.frame = 0;
        }

        switch ((byte)Projectile.ai[0])
        {
            case Behavior_SwarmReminder:
                const float HalfReminderTime = 20f;
                float ratio = Timer1 / HalfReminderTime;
                float maxSpeed = 12f;
                float speed = maxSpeed * (1f - ratio);
                Projectile.velocity = new Vector2(speed * Projectile.ai[1] * -1f, 0f);

                if (Timer1 >= HalfReminderTime * 2 - 1)
                    Projectile.Kill();
                break;

            case Behavior_Pet:
                Projectile.damage = 0;

                Player player = Owner;
                if (!Owner.Alive)
                {
                    Projectile.timeLeft = 2;
                    break;
                }

                Projectile.FloatingPetAI(false, 0.03f);
                Projectile.timeLeft = 300;
                break;

            default:
                Projectile.Kill();
                break;
        }

        Projectile.spriteDirection = (Projectile.velocity.X >= 0f).ToDirectionInt();
    }

    public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI) => behindNPCs.Add(index);
}

