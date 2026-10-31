namespace Anomalies.Bosses.Skeletron.LegendaryNight;

public sealed partial class SkeletronLegendHead
{
    public static class SlimeAvatarData
    {
        public const int FanTime = 66 * 60 + 10;
        public const int Ring1Time = 69 * 60 + 20;
        public const int Ring2Time = 70 * 60 + 55;
        public const int JumpTime = 72 * 60 + 30;
        public const int JumpDuration = 90;
        public const int LandingTime = JumpTime + JumpDuration;
        public const int ReturnDuration = 90;
        public const int RainDuration = 10;
        public const int RainLifetime = 300;
        public const int FanCount = 13;
        public const int RingCount = 32;
        public const int ChargeDuration = 90;
        public const float MountSpeed = 16f;
        public const float MountSpinSpeed = 0.12f;
        public const float JumpGravity = 1.6f;
    }

    public bool Phase2SlimeAvatarActive => Mechanic2Active
        && Phase2MusicElapsedTime >= LegendaryNightPhase2Arena.FirstStarburstMusicTime
        && Phase2MusicElapsedTime < SlimeAvatarData.LandingTime;
    public bool Phase2SlimeJumpActive => Phase2SlimeAvatarActive && Phase2MusicElapsedTime >= SlimeAvatarData.JumpTime;

    private void Mechanic2SlimeAI()
    {
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
        int localPhase = time < SlimeAvatarData.JumpTime ? 1 : time < SlimeAvatarData.LandingTime ? 2
            : time < SlimeAvatarData.LandingTime + SlimeAvatarData.ReturnDuration ? 3 : 4;
        if (TONetworkEnvironment.NotClient && CurrentLocalPhase != localPhase)
        {
            CurrentLocalPhase = localPhase;
            Timer1 = 0;
            NPC.netUpdate = true;
        }
        bool firstRain = time >= LegendaryNightPhase2Arena.FirstStarburstMusicTime
            && time < LegendaryNightPhase2Arena.FirstStarburstMusicTime + SlimeAvatarData.RainDuration;
        bool finalRain = time >= SlimeAvatarData.LandingTime && time < SlimeAvatarData.LandingTime + SlimeAvatarData.RainDuration;
        if (time == LegendaryNightPhase2Arena.FirstStarburstMusicTime)
        {
            SoundEngine.PlaySound(SoundID.ForceRoar, NPC.Center);
            ReleaseSpellShockwave(Spell.Slime);
        }
        // 15：每轮雨锁定的目标；8/9：跳跃起点；10/11：落底点。复用已结束的拍击槽。
        if ((time == LegendaryNightPhase2Arena.FirstStarburstMusicTime || time == SlimeAvatarData.LandingTime)
            && TONetworkEnvironment.NotClient)
        {
            AnomalyNPC.AnomalyAI32[15].I = NPC.target + 1;
            AnomalyNPC.AIChanged32[15] = true;
            NPC.netUpdate = true;
        }
        if ((firstRain || finalRain) && TONetworkEnvironment.NotClient
            && Player.TryGetPlayerFromIndex(AnomalyNPC.AnomalyAI32[15].I - 1, out Player rainTarget))
        {
            for (int i = 0; i < 3; i++)
                Projectile.NewProjectileAction<LegendSlimeRain>(SourceAI,
                    rainTarget.Center + new Vector2(Main.rand.NextFloat(-42f, 42f), -900f - i * 80f),
                    Vector2.UnitY * (finalRain ? 5f : 16f), SlimeMountDamage, 0f,
                    action: p => { p.timeLeft = SlimeAvatarData.RainLifetime; p.netUpdate = true; });
        }
        if (time < SlimeAvatarData.JumpTime)
        {
            SkeletronHead.NormalMovement(NPC, Target, 20f, 8f, 0.2f, 0.08f, 300f, 20f);
            Charge(SlimeAvatarData.FanTime);
            Charge(SlimeAvatarData.Ring1Time);
            Charge(SlimeAvatarData.Ring2Time);
            Charge(SlimeAvatarData.JumpTime);
            if (time == SlimeAvatarData.FanTime)
                FireMounts(SlimeAvatarData.FanCount, MathHelper.PiOver2 / (SlimeAvatarData.FanCount - 1), false);
            if (time == SlimeAvatarData.Ring1Time || time == SlimeAvatarData.Ring2Time)
                FireMounts(SlimeAvatarData.RingCount, MathHelper.TwoPi / SlimeAvatarData.RingCount, true);
            return;
        }
        if (time == SlimeAvatarData.JumpTime && TONetworkEnvironment.NotClient)
        {
            float x = MathHelper.Clamp(Target.Center.X - arena.Projectile.Center.X, -arena.Radius * 0.85f, arena.Radius * 0.85f);
            Vector2 landing = arena.Projectile.Center + new Vector2(x,
                MathF.Sqrt(arena.Radius * arena.Radius - x * x) - NPC.height * 0.5f);
            AnomalyNPC.AnomalyAI32[8].F = NPC.Center.X;
            AnomalyNPC.AnomalyAI32[9].F = NPC.Center.Y;
            AnomalyNPC.AnomalyAI32[10].F = landing.X;
            AnomalyNPC.AnomalyAI32[11].F = landing.Y;
            for (int slot = 8; slot <= 11; slot++)
                AnomalyNPC.AIChanged32[slot] = true;
            NPC.netUpdate = true;
        }
        Vector2 origin = new(AnomalyNPC.AnomalyAI32[8].F, AnomalyNPC.AnomalyAI32[9].F);
        Vector2 bottom = new(AnomalyNPC.AnomalyAI32[10].F, AnomalyNPC.AnomalyAI32[11].F);
        if (time < SlimeAvatarData.LandingTime)
        {
            // Fargo超级跳跃：起跳时锁定横向落点，固定重力形成高抛物线，不在空中追踪。
            float age = time - SlimeAvatarData.JumpTime + 1f;
            float progress = age / SlimeAvatarData.JumpDuration;
            Vector2 position = Vector2.Lerp(origin, bottom, progress);
            position.Y -= 0.5f * SlimeAvatarData.JumpGravity * age * (SlimeAvatarData.JumpDuration - age);
            NPC.velocity = position - NPC.Center;
        }
        else
        {
            if (time == SlimeAvatarData.LandingTime)
                SoundEngine.PlaySound(SoundID.Item14, NPC.Center);
            float progress = MathHelper.SmoothStep(0f, 1f, MathHelper.Clamp(
                (time - SlimeAvatarData.LandingTime) / (float)SlimeAvatarData.ReturnDuration, 0f, 1f));
            NPC.velocity = Vector2.Lerp(bottom, arena.Projectile.Center, progress) - NPC.Center;
        }

        void FireMounts(int count, float spacing, bool spin)
        {
            SoundEngine.PlaySound(SoundID.Item81, NPC.Center);
            if (!TONetworkEnvironment.NotClient)
                return;
            Projectile.NewProjectilesArc<SlimeMount>(count, spacing, SourceAI, NPC.Center,
                NPC.GetVelocityTowards(Target, SlimeAvatarData.MountSpeed), SlimeMountDamage, 0f, action: p =>
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
            if (Main.dedServ || remaining <= 20 || remaining > SlimeAvatarData.ChargeDuration)
                return;
            Vector2 offset = Main.rand.NextPolarVector2(120f, 320f);
            ParticleHandler.SpawnParticle(new CustomSpriteParticle(LegendSlimeRain.RainTexture, NPC.Center + offset,
                Vector2.Zero, 20, 0f, 1f, Color.White,
                customGetFrameAction: p => p.Texture.Frame(1, 4, 0, p.Timer / 5 % 4),
                customUpdateAction: p =>
                {
                    if (!NPC.active)
                        return;
                    float progress = MathHelper.Clamp(p.Timer / 20f, 0f, 1f);
                    p.Center = NPC.Center + offset * (1f - progress);
                    p.Rotation = (-offset).ToRotation() - MathHelper.PiOver2;
                }));
        }
    }
}
