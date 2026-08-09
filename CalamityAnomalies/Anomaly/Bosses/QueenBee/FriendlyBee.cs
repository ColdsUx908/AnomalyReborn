// Developed by ColdsUx

namespace CalamityAnomalies.Anomaly.Bosses.QueenBee;

public sealed class FriendlyBee : CAModProjectile
{
    public const byte Behavior_SwarmReminder = 1;
    public const byte Behavior_SafeCombCellReminder = 2;

    /* 数组使用约定
     * 
     * Projectile.ai
     * [0] 行为类型（默认值0表示无行为，立即消失）
     * [1] 特殊数据：
     *   若行为类型为1（蜂群攻击方向指示），表示蜂群攻击的方向（即 Boss 此时的攻击随机变量）
     */

    public override string LocalizationCategory => "Anomaly.QueenBee";

    public override void SetStaticDefaults() => Main.projFrames[Projectile.type] = 2;

    public override void SetDefaults()
    {
        Projectile.width = 32;
        Projectile.height = 32;
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.tileCollide = false;
        Projectile.timeLeft = 1200;
    }

    public override void AI()
    {
        Timer1++;

        if (++Projectile.frameCounter >= 3)
        {
            Projectile.frameCounter = 0;
            if (++Projectile.frame >= 2)
                Projectile.frame = 0;
        }

        Projectile.spriteDirection = (Projectile.velocity.X >= 0f).ToDirectionInt();

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

            case Behavior_SafeCombCellReminder:
                Projectile.scale = 0.75f;

                Projectile.Opacity = Timer1 <= 70 ? Math.Clamp(Timer1 / 20f, 0f, 1f) : Math.Clamp((90 - Timer1) / 20f, 0f, 1f);
                Projectile.velocity *= 1.01f;
                if (Timer1 >= 90)
                    Projectile.Kill();
                break;

            default:
                Projectile.Kill();
                break;
        }
    }

    public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI) => behindNPCs.Add(index);
}
