namespace Anomalies.Bosses.Skeletron.LegendaryNight;

public sealed partial class SkeletronLegendHead
{
    /// <summary>机制 5 的音乐绝对帧节点；从转场音乐开始累计，单位为游戏帧。</summary>
    public static class Mechanic5Timeline
    {
        /// <summary>本机制开始的音乐绝对帧（包含此帧）。</summary>
        public const int StartMusicTime = 86 * 60 + 40;
        /// <summary>本机制结束的音乐绝对帧（不包含此帧），与下一机制的开始帧衔接。</summary>
        public const int EndMusicTime = Mechanic6Timeline.StartMusicTime;
        /// <summary>第四次星爆的音乐绝对帧，从转场音乐开始累计。</summary>
        public const int FourthStarburstMusicTime = 97 * 60 + 45;
        /// <summary>第五次星爆的音乐绝对帧，从转场音乐开始累计。</summary>
        public const int FifthStarburstMusicTime = 100 * 60 + 55;
        /// <summary>星爆结束的音乐绝对帧，从转场音乐开始累计。</summary>
        public const int StarburstEndMusicTime = 112 * 60;
        /// <summary>公转恢复开始的音乐绝对帧，从转场音乐开始累计。</summary>
        public const int OrbitRecoveryStartMusicTime = StarburstEndMusicTime
            + LegendaryNightStarburstParameters.StarburstDamageDuration + LegendaryNightStarburstParameters.StarburstFadeDuration;
        /// <summary>雨幕结束的音乐绝对帧，从转场音乐开始累计。</summary>
        public const int RainEndMusicTime = FourthStarburstMusicTime - 30;
        /// <summary>机制五手部挥击收尾的音乐绝对帧；可跨过机制六开始帧，不作为转机制边界。</summary>
        public const int HandRecoveryEndMusicTime = StarburstEndMusicTime
            + Mechanic5Parameters.HandStrikeDelay + Mechanic5Parameters.HandStrikeDuration + Mechanic5Parameters.HandReturnDuration;
    }

    /// <summary>机制 5 的攻击、运动和表现参数及纯计算；时长单位为游戏帧。</summary>
    public static class Mechanic5Parameters
    {
        /// <summary>雨幕间隔，单位为游戏帧。</summary>
        public const int RainInterval = 30;
        /// <summary>雨幕存活时间，单位为游戏帧。</summary>
        public const int RainLifetime = Mechanic1Parameters.ProjectileLifetime;
        /// <summary>星爆初始星体数量。</summary>
        public const int StarburstInitialStarCount = 10;
        /// <summary>星爆星体数量增量。</summary>
        public const int StarburstStarCountIncrement = 2;
        /// <summary>计算星爆星体数量。</summary>
        public static int GetStarburstStarCount(int starburstIndex) => StarburstInitialStarCount + (starburstIndex - 4) * StarburstStarCountIncrement;

        /// <summary>手部重新出现持续时间，单位为游戏帧。</summary>
        public const int HandReappearanceDuration = 45;
        /// <summary>手部攻击数量。</summary>
        public const int HandAttackCount = 8;
        /// <summary>手部拍击延迟，单位为游戏帧。</summary>
        public const int HandStrikeDelay = 47;
        /// <summary>手部蓄势持续时间，单位为游戏帧。</summary>
        public const int HandWindupDuration = 24;
        /// <summary>手部拍击持续时间，单位为游戏帧。</summary>
        public const int HandStrikeDuration = 32;
        /// <summary>手部回归持续时间，单位为游戏帧。</summary>
        public const int HandReturnDuration = 36;
        /// <summary>手部蓄势距离，单位为世界像素。</summary>
        public const float HandWindupDistance = 80f;
        /// <summary>手部越界余量，单位为世界像素。</summary>
        public const float HandOvershoot = 48f;

        // 星爆采用0-based索引；第5至12次星爆各延后47帧开始前挥。
        /// <summary>计算指定手部挥击的音乐绝对帧；attack 为从零开始的攻击编号。</summary>
        public static int GetHandStrikeTime(int attack) => LegendaryNightStarburstTimeline.GetStarburstTime(4 + attack) + HandStrikeDelay;

        /// <summary>计算手部攻击索引。</summary>
        public static int GetHandAttackIndex(int musicTime)
        {
            for (int attack = 0; attack < HandAttackCount; attack++)
            {
                int age = musicTime - GetHandStrikeTime(attack);
                if (age is >= -HandWindupDuration and < HandStrikeDuration + HandReturnDuration)
                    return attack;
            }
            return -1;
        }

        /// <summary>沿锁定玩家的方向挥到法阵外沿，并留出越过边缘的余量。</summary>
        public static Vector2 GetHandStrikeDestination(Vector2 origin, Vector2 target, Vector2 center, float radius)
        {
            Vector2 direction = (target - origin).SafeNormalize(Vector2.UnitY);
            Vector2 offset = origin - center;
            float projection = Vector2.Dot(offset, direction);
            float distance = -projection + MathF.Sqrt(Math.Max(0f, projection * projection + radius * radius - offset.LengthSquared()));
            return origin + direction * (Math.Max(0f, distance) + HandOvershoot);
        }
    }

    private void Mechanic5AI()
    {
        int time = Phase2MusicElapsedTime;
        // LocalPhase：0星星雨，1公转加速，2连续星爆及手部收尾，3结束待机。
        int localPhase = time switch
        {
            < Mechanic5Timeline.FourthStarburstMusicTime => 0,
            < Mechanic5Timeline.FifthStarburstMusicTime => 1,
            < Mechanic5Timeline.HandRecoveryEndMusicTime => 2,
            _ => 3
        };
        if (TONetworkEnvironment.NotClient && CurrentLocalPhase != localPhase)
        {
            ChangePhase2LocalPhase(localPhase);
        }
        AnomalyNPC.Z = 0f;
        NPC.rotation = 0f;
        float reposition = MathHelper.SmoothStep(0f, 1f, MathHelper.Clamp(
            (time - Mechanic6Timeline.RepositionStartMusicTime) / (float)Mechanic6Parameters.RepositionDuration, 0f, 1f));
        NPC.velocity = Phase2Arena is LegendaryNightPhase2Arena arena
            ? arena.Projectile.Center - Mechanic6Parameters.SphereOffset(time) * reposition - NPC.Center : Vector2.Zero;
        if (time == Mechanic5Timeline.StartMusicTime)
        {
            SoundEngine.PlaySound(SoundID.ForceRoar);
            if (TONetworkEnvironment.NotClient)
            {
                // 复用原有手部，从本体位置重新伸出，不生成或替换NPC。
                SkeletronLegendHand.RevealOwnedHands(NPC);
            }
        }
        if (!TONetworkEnvironment.NotClient)
            return;
        if (time == Mechanic5Timeline.FifthStarburstMusicTime)
        {
            foreach (Projectile projectile in Projectile.ActiveProjectiles)
                if (projectile.ModProjectile is FallingStar && (int)projectile.ai[2] == NPC.whoAmI + 1
                    && projectile.ai[1] == 5f)
                    projectile.Kill();
        }
        int starburst = LegendaryNightStarburstTimeline.GetStarburstIndex(time);
        if (starburst >= 4 && time == LegendaryNightStarburstTimeline.GetStarburstTime(starburst)
            && Phase2Arena is LegendaryNightPhase2Arena starArena)
        {
            SpawnStarburstStars(starArena, starburst);
        }
        if (time >= Mechanic5Timeline.StartMusicTime && time < Mechanic5Timeline.RainEndMusicTime
            && (time - Mechanic5Timeline.StartMusicTime) % Mechanic5Parameters.RainInterval == 0)
        {
            Projectile.NewProjectileAction<FallingStar>(SourceAI,
                Target.Center + new Vector2(Main.rand.NextFloat(-1200f, 1200f), -1000f),
                new Vector2(Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(6f, 10f)), FallingStarDamage, 0f,
                action: p =>
                {
                    p.ai[1] = 5f; // 机制5星星雨，清场不影响其他攻击。
                    p.ai[2] = NPC.whoAmI + 1;
                    p.scale = 0.2f;
                    p.timeLeft = Mechanic5Parameters.RainLifetime;
                    p.GetModProjectile<FallingStar>().TrailType = 1;
                    p.netUpdate = true;
                });
        }

        void SpawnStarburstStars(LegendaryNightPhase2Arena arena, int starburstIndex)
        {
            // 发射点直接读取法阵中心，不依赖本体本帧是否已经移动到圆心。
            Vector2 origin = arena.Projectile.Center;
            float aimAngle = (Target.Center - origin).SafeNormalize(Vector2.UnitY).ToRotation();
            int count = Mechanic5Parameters.GetStarburstStarCount(starburstIndex);
            float speed = Mechanic2Parameters.GetFanStarSpeed(arena.Radius, 0);
            for (int i = 0; i < count; i++)
            {
                Vector2 velocity = (aimAngle + MathHelper.TwoPi * i / count).ToRotationVector2() * speed;
                Projectile.NewProjectileAction<FallingStar>(SourceAI, origin, velocity, FallingStarDamage, 0f, action: p =>
                {
                    p.ai[1] = arena.Radius;
                    p.ai[2] = NPC.whoAmI + 1;
                    p.scale = 0.2f;
                    p.Resize(16, 16);
                    p.timeLeft = Mechanic2Parameters.StrikeInterval;
                    FallingStar star = p.GetModProjectile<FallingStar>();
                    star.CurrentMotionMode = FallingStar.MotionMode.StrikeFan;
                    star.TrailType = 2;
                    p.netUpdate = true;
                });
            }
        }
    }
}