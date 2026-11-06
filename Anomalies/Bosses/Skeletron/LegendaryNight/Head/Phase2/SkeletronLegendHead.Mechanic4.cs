namespace Anomalies.Bosses.Skeletron.LegendaryNight;

public sealed partial class SkeletronLegendHead
{
    /// <summary>机制 4 的音乐绝对帧节点；从转场音乐开始累计，单位为游戏帧。</summary>
    public static class Mechanic4Timeline
    {
        /// <summary>本机制开始的音乐绝对帧（包含此帧）。</summary>
        public const int StartMusicTime = 77 * 60 + 15;
        /// <summary>本机制结束的音乐绝对帧（不包含此帧），与下一机制的开始帧衔接。</summary>
        public const int EndMusicTime = Mechanic5Timeline.StartMusicTime;
        /// <summary>首次蓄力的音乐绝对帧，从转场音乐开始累计。</summary>
        public const int FirstChargeMusicTime = 78 * 60 + 47;
        /// <summary>首次后撤开始的音乐绝对帧，从转场音乐开始累计。</summary>
        public const int FirstRetreatStartMusicTime = FirstChargeMusicTime - Mechanic4Parameters.RetreatDuration;
        /// <summary>回归开始的音乐绝对帧，从转场音乐开始累计。</summary>
        public const int ReturnStartMusicTime = FirstChargeMusicTime + (Mechanic4Parameters.ChargeCount - 1) * Mechanic4Parameters.ChargeInterval + Mechanic4Parameters.ChargeDuration;
    }

    /// <summary>机制 4 的攻击、运动和表现参数及纯计算；时长单位为游戏帧。</summary>
    public static class Mechanic4Parameters
    {
        /// <summary>蓄力间隔，单位为游戏帧。</summary>
        public const int ChargeInterval = 95;
        /// <summary>蓄力数量。</summary>
        public const int ChargeCount = 4;
        /// <summary>蓄力持续时间，单位为游戏帧。</summary>
        public const int ChargeDuration = (ChargeInterval + 1) / 2;
        /// <summary>减速持续时间，单位为游戏帧。</summary>
        public const int DecelerationDuration = 8;
        /// <summary>后撤持续时间，单位为游戏帧。</summary>
        public const int RetreatDuration = ChargeInterval - ChargeDuration;
        /// <summary>后撤速度，以世界像素和游戏帧计量。</summary>
        public const float RetreatSpeed = 12f;
        /// <summary>蓄力速度，以世界像素和游戏帧计量。</summary>
        public const float ChargeSpeed = 32f;
        /// <summary>计算指定冲刺的音乐绝对帧；index 为从零开始的冲刺编号。</summary>
        public static int GetChargeTime(int index) => Mechanic4Timeline.FirstChargeMusicTime + index * ChargeInterval;
        /// <summary>计算蓄力速度系数。</summary>
        public static float ChargeSpeedFactor(int age) => MathHelper.Clamp(
            (ChargeDuration - age - 1f) / DecelerationDuration, 0f, 1f);

        /// <summary>判断当前帧是否处于激光伤害窗口。</summary>
        public static bool LasersActive(int time) => LaserAge(time) is >= 0 and < Mechanic2Parameters.LaserDuration;

        /// <summary>计算激光预警。</summary>
        public static float LaserWarning(int time)
        {
            int age = LaserAge(time);
            return age is < 0 and >= -ChargeDuration ? 1f + age / (float)ChargeDuration : 0f;
        }

        /// <summary>计算激光强度。</summary>
        public static float LaserIntensity(int time)
        {
            int age = LaserAge(time);
            if (age < -Mechanic2Parameters.LaserFadeInDuration)
                return 0f;
            float fadeIn = MathHelper.Clamp((age + Mechanic2Parameters.LaserFadeInDuration) / (float)Mechanic2Parameters.LaserFadeInDuration, 0f, 1f);
            float fadeOut = MathHelper.Clamp((Mechanic2Parameters.LaserDuration + Mechanic2Parameters.LaserFadeOutDuration - age)
                / (float)Mechanic2Parameters.LaserFadeOutDuration, 0f, 1f);
            return MathHelper.SmoothStep(0f, 1f, Math.Min(fadeIn, fadeOut));
        }

        /// <summary>计算当前激光波次相对伤害开始帧的年龄；窗口外返回最小整数。</summary>
        public static int LaserAge(int time)
        {
            for (int i = 0; i < ChargeCount; i++)
            {
                int age = time - GetChargeTime(i) - ChargeDuration;
                if (age is >= -ChargeDuration and < Mechanic2Parameters.LaserDuration + Mechanic2Parameters.LaserFadeOutDuration)
                    return age;
            }
            return int.MinValue;
        }
    }

    private void Mechanic4AI()
    {
        int time = Phase2MusicElapsedTime;
        LegendaryNightPhase2Arena arena = Phase2Arena;
        HideHands();
        AnomalyNPC.Z = 0f;
        NPC.rotation = 0f;
        if (arena is null)
        {
            NPC.velocity = Vector2.Zero;
            return;
        }
        if (time == Mechanic4Timeline.StartMusicTime)
        {
            SoundEngine.PlaySound(SoundID.ForceRoar);
            ReleaseSpellShockwave(Spell.PlatinumCoin);
        }

        // LocalPhase：0首次待机及后退，1/3/5/7直冲，2/4/6后退，8回中。
        int charge = Math.Clamp((time - Mechanic4Timeline.FirstChargeMusicTime) / Mechanic4Parameters.ChargeInterval,
            0, Mechanic4Parameters.ChargeCount - 1);
        int age = time - Mechanic4Parameters.GetChargeTime(charge);
        int localPhase = time switch
        {
            >= Mechanic4Timeline.ReturnStartMusicTime => 8,
            < Mechanic4Timeline.FirstChargeMusicTime => 0,
            _ => charge * 2 + (age < Mechanic4Parameters.ChargeDuration ? 1 : 2)
        };
        if (TONetworkEnvironment.NotClient && CurrentLocalPhase != localPhase)
        {
            ChangePhase2LocalPhase(localPhase);
        }
        if (time == Mechanic4Parameters.GetChargeTime(charge) + Mechanic4Parameters.ChargeDuration)
            SoundEngine.PlaySound(SoundID.Item14);
        if (time >= Mechanic4Timeline.ReturnStartMusicTime)
        {
            // AI64[2]：回中起点；AI64[3]：冲刺锁定速度。复用已结束的史莱姆跳跃槽。
            if (time == Mechanic4Timeline.ReturnStartMusicTime && TONetworkEnvironment.NotClient)
            {
                Mechanic4ReturnOrigin = NPC.Center;
                NPC.netUpdate = true;
            }
            Vector2 origin = Mechanic4ReturnOrigin;
            float progress = MathHelper.SmoothStep(0f, 1f, MathHelper.Clamp(
                (time - Mechanic4Timeline.ReturnStartMusicTime + 1f) / (Mechanic4Timeline.EndMusicTime - Mechanic4Timeline.ReturnStartMusicTime), 0f, 1f));
            NPC.velocity = Vector2.Lerp(origin, arena.Projectile.Center, progress) - NPC.Center;
            return;
        }
        if (age is >= 0 and < Mechanic4Parameters.ChargeDuration)
        {
            if (age == 0)
            {
                SoundEngine.PlaySound(SoundID.ForceRoar);
                if (TONetworkEnvironment.NotClient)
                {
                    Vector2 velocity = NPC.GetVelocityTowards(Target, Mechanic4Parameters.ChargeSpeed);
                    Mechanic4ChargeVelocity = velocity;
                    NPC.netUpdate = true;
                }
            }
            Vector2 chargeVelocity = Mechanic4ChargeVelocity;
            float speedFactor = Mechanic4Parameters.ChargeSpeedFactor(age);
            NPC.velocity = chargeVelocity * speedFactor;
        }
        else
        {
            int retreatStart = time < Mechanic4Timeline.FirstChargeMusicTime ? Mechanic4Timeline.FirstRetreatStartMusicTime
                : Mechanic4Parameters.GetChargeTime(charge) + Mechanic4Parameters.ChargeDuration;
            float speed = Mechanic4Parameters.RetreatSpeed * MathHelper.Clamp((time - retreatStart + 1f) / 12f, 0f, 1f);
            NPC.velocity = -NPC.GetVelocityTowards(Target, speed);
        }
    }
}