namespace Anomalies.Bosses.Skeletron.LegendaryNight;

public sealed partial class SkeletronLegendHead
{
    /// <summary>机制 3 的音乐绝对帧节点；从转场音乐开始累计，单位为游戏帧。</summary>
    public static class Mechanic3Timeline
    {
        /// <summary>本机制开始的音乐绝对帧（包含此帧）。</summary>
        public const int StartMusicTime = 63 * 60;
        /// <summary>本机制结束的音乐绝对帧（不包含此帧），与下一机制的开始帧衔接。</summary>
        public const int EndMusicTime = Mechanic4Timeline.StartMusicTime;
        /// <summary>扇形的音乐绝对帧，从转场音乐开始累计。</summary>
        public const int FanMusicTime = 66 * 60 + 10;
        /// <summary>首次环形的音乐绝对帧，从转场音乐开始累计。</summary>
        public const int FirstRingMusicTime = 69 * 60 + 20;
        /// <summary>第二次环形的音乐绝对帧，从转场音乐开始累计。</summary>
        public const int SecondRingMusicTime = 70 * 60 + 55;
        /// <summary>跳跃的音乐绝对帧，从转场音乐开始累计。</summary>
        public const int JumpMusicTime = 72 * 60 + 30;
        /// <summary>落地的音乐绝对帧，从转场音乐开始累计。</summary>
        public const int LandingMusicTime = JumpMusicTime + Mechanic3Parameters.JumpDuration;
    }

    /// <summary>机制 3 的攻击、运动和表现参数及纯计算；时长单位为游戏帧。</summary>
    public static class Mechanic3Parameters
    {
        /// <summary>跳跃持续时间，单位为游戏帧。</summary>
        public const int JumpDuration = 90;
        /// <summary>回归持续时间，单位为游戏帧。</summary>
        public const int ReturnDuration = 90;
        /// <summary>雨幕持续时间，单位为游戏帧。</summary>
        public const int RainDuration = 20;
        /// <summary>雨幕存活时间，单位为游戏帧。</summary>
        public const int RainLifetime = 600;
        /// <summary>雨幕层数。</summary>
        public const int RainLayers = 3;
        /// <summary>终幕雨幕层数。</summary>
        public const int FinalRainLayers = 9;
        /// <summary>雨幕生成高度，单位为世界像素。</summary>
        public const float RainSpawnHeight = 800f;
        /// <summary>雨幕层间距，单位为世界像素。</summary>
        public const float RainLayerSpacing = 125f;
        /// <summary>雨幕半宽，单位为世界像素。</summary>
        public const float RainHalfWidth = 40f;
        /// <summary>雨幕速度，以世界像素和游戏帧计量。</summary>
        public const float RainSpeed = 20f;
        /// <summary>扇形数量。</summary>
        public const int FanCount = 11;
        /// <summary>环形数量。</summary>
        public const int RingCount = 24;
        /// <summary>蓄力持续时间，单位为游戏帧。</summary>
        public const int ChargeDuration = 90;
        /// <summary>坐骑速度，以世界像素和游戏帧计量。</summary>
        public const float MountSpeed = 16f;
        /// <summary>自转或公转角速度，单位为弧度/游戏帧。</summary>
        public const float MountSpinSpeed = 0.12f;
        /// <summary>跳跃重力，以世界像素和游戏帧计量。</summary>
        public const float JumpGravity = 1.6f;
    }

    private void Mechanic3AI()
    {
        #region 场地与音乐步骤

        int time = Phase2MusicElapsedTime;
        LegendaryNightPhase2Arena arena = Phase2Arena;
        HideHands();
        AnomalyNPC.Z = 0f;
        NPC.rotation *= 0.85f;
        if (arena is null)
        {
            NPC.velocity = Vector2.Zero;
            return;
        }

        // LocalPhase：1飞行蓄力，2超级跳跃，3恢复原型回中，4等待下段。
        int localPhase = time switch
        {
            < Mechanic3Timeline.JumpMusicTime => 1,
            < Mechanic3Timeline.LandingMusicTime => 2,
            < Mechanic3Timeline.LandingMusicTime + Mechanic3Parameters.ReturnDuration => 3,
            _ => 4
        };
        if (TONetworkEnvironment.NotClient && CurrentLocalPhase != localPhase)
        {
            ChangePhase2LocalPhase(localPhase);
        }

        #endregion

        #region 史莱姆雨与目标锁定

        bool firstRain = time is >= Mechanic3Timeline.StartMusicTime
            and < Mechanic3Timeline.StartMusicTime + Mechanic3Parameters.RainDuration;
        bool finalRain = time is >= Mechanic3Timeline.LandingMusicTime and < (Mechanic3Timeline.LandingMusicTime + Mechanic3Parameters.RainDuration);
        if (time == Mechanic3Timeline.StartMusicTime)
        {
            SoundEngine.PlaySound(SoundID.ForceRoar);
            ReleaseSpellShockwave(Spell.Slime);
        }
        // AI32[8]：本轮雨目标；AI64[2/3]：跳跃起点/落底点，复用已结束的拍击槽。
        if ((time == Mechanic3Timeline.StartMusicTime || time == Mechanic3Timeline.LandingMusicTime)
            && TONetworkEnvironment.NotClient)
        {
            Mechanic3RainTargetIndex = NPC.target + 1;
            NPC.netUpdate = true;
        }
        if ((firstRain || finalRain) && TONetworkEnvironment.NotClient
            && Player.TryGetPlayerFromIndex(Mechanic3RainTargetIndex - 1, out Player rainTarget))
        {
            // 首层直接出现在玩家头顶；高层延长末轮雨，而不是让整轮在屏外等待数秒。
            int layers = finalRain ? Mechanic3Parameters.FinalRainLayers : Mechanic3Parameters.RainLayers;
            float spacing = Mechanic3Parameters.RainLayerSpacing;
            float speed = Mechanic3Parameters.RainSpeed;
            for (int i = 0; i < layers; i++)
                Projectile.NewProjectileAction<LegendSlimeRain>(SourceAI,
                    rainTarget.Center + new Vector2(Main.rand.NextFloat(-Mechanic3Parameters.RainHalfWidth, Mechanic3Parameters.RainHalfWidth),
                        -Mechanic3Parameters.RainSpawnHeight - i * spacing),
                    Vector2.UnitY * speed, SlimeMountDamage, 0f,
                    action: p =>
                    {
                        p.ai[0] = Main.rand.Next(LegendSlimeRain.VariantCount);
                        p.timeLeft = Mechanic3Parameters.RainLifetime;
                        p.netUpdate = true;
                    });
        }

        #endregion

        #region 起跳前的常态攻击

        if (time < Mechanic3Timeline.JumpMusicTime)
        {
            SkeletronKinematics.NormalMovement(NPC, Target, 20f, 8f, 0.2f, 0.08f, 300f, 20f);
            Charge(Mechanic3Timeline.FanMusicTime);
            Charge(Mechanic3Timeline.FirstRingMusicTime);
            Charge(Mechanic3Timeline.SecondRingMusicTime);
            Charge(Mechanic3Timeline.JumpMusicTime);
            switch (time)
            {
                case Mechanic3Timeline.FanMusicTime:
                    FireMounts(Mechanic3Parameters.FanCount, TOMathUtils.PiOver3 * 2 / (Mechanic3Parameters.FanCount - 1), false);
                    break;
                case Mechanic3Timeline.FirstRingMusicTime:
                case Mechanic3Timeline.SecondRingMusicTime:
                    FireMounts(Mechanic3Parameters.RingCount, MathHelper.TwoPi / Mechanic3Parameters.RingCount, true);
                    break;
            }
            return;
        }

        #endregion

        #region 起跳锁点、落底与回中

        if (time == Mechanic3Timeline.JumpMusicTime)
            SoundEngine.PlaySound(SoundID.Item92, NPC.Center);
        if (time == Mechanic3Timeline.JumpMusicTime && TONetworkEnvironment.NotClient)
        {
            float x = MathHelper.Clamp(Target.Center.X - arena.Projectile.Center.X, -arena.Radius * 0.85f, arena.Radius * 0.85f);
            Vector2 landing = arena.Projectile.Center + new Vector2(x,
                MathF.Sqrt(arena.Radius * arena.Radius - x * x) - NPC.height * 0.5f);
            Mechanic3JumpOrigin = NPC.Center;
            Mechanic3LandingPosition = landing;
            NPC.netUpdate = true;
        }
        Vector2 origin = Mechanic3JumpOrigin;
        Vector2 bottom = Mechanic3LandingPosition;
        if (time < Mechanic3Timeline.LandingMusicTime)
        {
            // Fargo超级跳跃：起跳时锁定横向落点，固定重力形成高抛物线，不在空中追踪。
            float age = time - Mechanic3Timeline.JumpMusicTime + 1f;
            float progress = age / Mechanic3Parameters.JumpDuration;
            Vector2 position = Vector2.Lerp(origin, bottom, progress);
            position.Y -= 0.5f * Mechanic3Parameters.JumpGravity * age * (Mechanic3Parameters.JumpDuration - age);
            NPC.velocity = position - NPC.Center;
        }
        else
        {
            if (time == Mechanic3Timeline.LandingMusicTime)
                SoundEngine.PlaySound(SoundID.Item14, NPC.Center);
            float progress = MathHelper.SmoothStep(0f, 1f, MathHelper.Clamp(
                (time - Mechanic3Timeline.LandingMusicTime) / (float)Mechanic3Parameters.ReturnDuration, 0f, 1f));
            NPC.velocity = Vector2.Lerp(bottom, arena.Projectile.Center, progress) - NPC.Center;
        }

        #endregion

        #region 坐骑发射与蓄力粒子

        void FireMounts(int count, float spacing, bool spin)
        {
            SoundEngine.PlaySound(SoundID.Item81, NPC.Center);
            if (!TONetworkEnvironment.NotClient)
                return;
            Projectile.NewProjectilesArc<SlimeMount>(count, spacing, SourceAI, NPC.Center,
                NPC.GetVelocityTowards(Target, Mechanic3Parameters.MountSpeed), SlimeMountDamage, 0f, action: p =>
                {
                    p.ai[1] = spin ? 3f : 2f;
                    p.rotation = p.velocity.ToRotation() - MathHelper.PiOver2;
                    p.timeLeft = 180;
                    p.netUpdate = true;
                });
        }

        void Charge(int releaseTime)
        {
            int remaining = releaseTime - time;
            if (Main.dedServ || remaining <= LegendaryNightSlimeChargeParticle.ChargeLifetime
                || remaining > Mechanic3Parameters.ChargeDuration)
                return;
            for (int i = 0; i < LegendaryNightSlimeChargeParticle.SpawnCount; i++)
            {
                Vector2 offset = Main.rand.NextPolarVector2(120f, 320f);
                AnomalyNPC.SpawnVisualParticle(new LegendaryNightSlimeChargeParticle(NPC, offset));
            }
        }

        #endregion
    }
}
