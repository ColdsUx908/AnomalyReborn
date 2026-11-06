namespace Anomalies.Bosses.Skeletron.LegendaryNight;

public sealed partial class SkeletronLegendHead
{
    /// <summary>转阶段化身渐隐持续时间，单位为游戏帧。</summary>
    private const int PhaseChangeAvatarFadeDuration = 60;
    /// <summary>转阶段落星生成帧。</summary>
    private const int PhaseChangeStarSpawnTime = 60;
    /// <summary>转阶段星体下落持续时间，单位为游戏帧。</summary>
    private const int PhaseChangeStarFallDuration = 120;
    /// <summary>转阶段光晕渐隐持续时间，单位为游戏帧。</summary>
    private const int PhaseChangeGlowFadeDuration = 90;
    /// <summary>转阶段冲击波时刻。</summary>
    private const int PhaseChangeShockwaveTime = 180;
    /// <summary>转阶段回血开始。</summary>
    private const int PhaseChangeHealStart = PhaseChangeStarSpawnTime + PhaseChangeStarFallDuration;
    /// <summary>转阶段手部召唤帧。</summary>
    private const int PhaseChangeHandsTime = 290;
    /// <summary>转阶段祝福时刻。</summary>
    private const int PhaseChangeBlessingTime = 370;
    /// <summary>转阶段持续时间，单位为游戏帧。</summary>
    private const int PhaseChangeDuration = 475;
    /// <summary>转阶段冲击波缩放。</summary>
    private const float PhaseChangeShockwaveScale = 2.5f;

    private void ResetCombatStateForTransition()
    {
        CurrentBehavior = Behavior.None;
        CurrentSpell = Spell.None;
        CurrentLocalPhase = 0;
        AI_Union_2 = default;
        AI_Union_3 = default;
        Timer1 = Timer2 = Timer3 = Timer4 = 0;
        Timer6 = Timer7 = Timer8 = 0f;
        NPC.netUpdate = true;
    }

    private void BeginPhaseChange_1To2()
    {
        ResetCombatStateForTransition();
        Timer5 = 0;
        Timer2 = NPC.life; // 保存回血插值的起始血量。
        CurrentPhase = Phase.PhaseChange_1To2;
        NPC.dontTakeDamage = true;
        if (TONetworkEnvironment.NotClient)
        {
            // 化身仍在渐隐时，也必须立即取消它所驱动的延迟攻击。
            foreach (Projectile projectile in Projectile.ActiveProjectiles)
            {
                bool owned = projectile.ModProjectile switch
                {
                    CoinRoulette roulette => roulette.MasterIndex == NPC.whoAmI + 1,
                    SlimeMount slime => slime.MasterIndex == NPC.whoAmI + 1,
                    CoinJudgment or SkullWarning or DemonShadow => (int)projectile.ai[2] == NPC.whoAmI + 1,
                    _ => false
                };
                if (owned)
                    projectile.Kill();
            }
        }
    }

    private void PhaseChange_1To2AI()
    {
        Timer1++;
        NPC.dontTakeDamage = true;
        StopMovement();
        NPC.rotation *= 0.9f;
        NPC.Opacity = MathHelper.Lerp(0.5f, 1f, MathHelper.Clamp(Timer1 / (float)PhaseChangeAvatarFadeDuration, 0f, 1f));

        if (Timer1 == PhaseChangeHealStart)
        {
            Vector2 impactPosition = GetPhaseChangeStarPosition(Timer1);
            LegendaryNightHandler.SpawnBlessingBurst(impactPosition);
            if (!Main.dedServ)
                SoundEngine.PlaySound(Meteor.ExplosionSound, impactPosition);
        }

        if (Timer1 is >= PhaseChangeHealStart and <= PhaseChangeHandsTime)
        {
            float completion = (Timer1 - PhaseChangeHealStart) / (float)(PhaseChangeHandsTime - PhaseChangeHealStart);
            int newLife = Timer1 == PhaseChangeHandsTime ? NPC.lifeMax
                : (int)MathHelper.Lerp(Timer2, NPC.lifeMax, TOMathUtils.Interpolation.LogarithmicEaseOut(completion));
            int increasedLife = Math.Max(0, newLife - NPC.life);
            if (increasedLife > 0)
            {
                NPC.life = Math.Min(newLife, NPC.lifeMax);
                NPC.HealEffect(increasedLife, true);
            }
            if (TONetworkEnvironment.NotClient && (Timer1 % 10 == 0 || Timer1 == PhaseChangeHandsTime))
                NPC.netUpdate = true;
        }

        if (Timer1 == PhaseChangeShockwaveTime)
        {
            SoundEngine.PlaySound(SoundID.Roar);
            if (TONetworkEnvironment.NotClient)
            {
                Projectile.NewProjectileAction<LegendShockwave>(SourceAI, NPC.Center, Vector2.Zero, 0, 0f, action: p =>
                {
                    p.GetModProjectile<LegendShockwave>().TargetScale = PhaseChangeShockwaveScale;
                    p.netUpdate = true;
                });

            }
        }

        if (Timer1 == PhaseChangeHandsTime)
        {
            if (TONetworkEnvironment.NotClient)
            {
                IsSummoningHands = true;
                try
                {
                    NPC master = NPC;
                    for (int side = -1; side <= 1; side += 2)
                    {
                        NPC.NewNPCAction(SourceAI, master.Center, NPCID.SkeletronHand, master.whoAmI, action: hand =>
                        {
                            hand.ai[0] = side;
                            hand.ai[1] = master.whoAmI;
                            hand.Master = master;
                            hand.Anomaly.CustomAIType = 1;
                            hand.target = master.target;
                            hand.netUpdate = true;
                        });
                    }
                }
                finally
                {
                    IsSummoningHands = false;
                }
            }
        }

        if (Timer1 == PhaseChangeBlessingTime)
        {
            if (TONetworkEnvironment.NotClient)
                TOLocalizationUtils.ChatLocalizedText(AnomalySharedData.ModLocalizationPrefix + LegendaryNightHandler.LocalizationCategory + ".AstralBlessingAnnouncement", LegendaryNightHandler.StarColor);
            if (!Main.dedServ)
            {
                foreach (Player player in Main.ActivePlayers)
                {
                    if (!player.dead)
                        LegendaryNightHandler.SpawnBlessingBurst(player.Center);
                }
            }
        }

        if (Timer1 >= PhaseChangeDuration)
        {
            ResetCombatStateForTransition();
            ChangePhase2State(Phase.Phase2_1);
            AnomalyNPC.BossBar_Immune = false;
            NPC.dontTakeDamage = false;
            NPC.Opacity = 1f;
        }
    }
}