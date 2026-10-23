using Anomalies.Assets.Effects;
using Anomalies.Visuals.BossBar;

namespace Anomalies.Bosses.BrainofCthulhu;

public sealed partial class BrainofCthulhu : AnomalyNPCBehavior<BrainofCthulhu>
{
    public override int ApplyingType => NPCID.BrainofCthulhu;

    public override bool CheckDead()
    {
        if (CurrentPhase == Phase.Death)
            return true;
        TriggerDeathAnimation();
        return false;
    }

    public override void SetDefaultsFinal()
    {
        NPC.lifeMax = CalamityEnabled ? 6000 : 4500;
        BridgeUtils.ApplyCalamityHealthBoost(NPC);
    }

    public override void ApplyDifficultyAndPlayerScaling(int numPlayers, float balance, float bossAdjustment) =>
        TONPCUtils.ApplyAnomalyBossDifficultyAndPlayerScaling(NPC, bossAdjustment);

    public override bool AllowCalamityLogic(CalamityLogicType_NPCBehavior type) => type != CalamityLogicType_NPCBehavior.VanillaOverrideAI;
    private static SoundStyle StunHitSound = new("Anomalies/Bosses/BrainofCthulhu/Sounds/StunHit", 3);
    private static SoundStyle ShieldDownSound = new SoundStyle("Anomalies/Bosses/BrainofCthulhu/Sounds/ShieldDown") with
    {
        PauseBehavior = PauseBehavior.PauseWithGame
    };
    private static SoundStyle ShieldUpSound = new SoundStyle("Anomalies/Bosses/BrainofCthulhu/Sounds/ShieldUp") with
    {
        PauseBehavior = PauseBehavior.PauseWithGame
    };
    private static SoundStyle RoarSound = new SoundStyle("Anomalies/Bosses/BrainofCthulhu/Sounds/Roar") with
    {
        PauseBehavior = PauseBehavior.PauseWithGame
    };
    private static SoundStyle ShortRoarSound = new SoundStyle("Anomalies/Bosses/BrainofCthulhu/Sounds/ShortRoar") with
    {
        PauseBehavior = PauseBehavior.PauseWithGame
    };
    public static SoundStyle LaughSound = new SoundStyle("Anomalies/Bosses/BrainofCthulhu/Sounds/Laugh") with
    {
        PauseBehavior = PauseBehavior.PauseWithGame,
        MaxInstances = 5
    };
    private static SoundStyle GrowlSound = new SoundStyle("Anomalies/Bosses/BrainofCthulhu/Sounds/Growl", 2) with
    {
        PauseBehavior = PauseBehavior.PauseWithGame
    };
    private static SoundStyle DeathRoarSound = new SoundStyle("Anomalies/Bosses/BrainofCthulhu/Sounds/DeathRoar") with
    {
        PauseBehavior = PauseBehavior.PauseWithGame
    };
    private static SoundStyle BloodShotSound = new("Anomalies/Bosses/BrainofCthulhu/Sounds/BloodShot");
    private static SoundStyle BloodBombSound = new("Anomalies/Bosses/BrainofCthulhu/Sounds/BloodBomb");
    private static SoundStyle ExplosionSound = new("Anomalies/Bosses/BrainofCthulhu/Sounds/Explosion", 2);
    public bool SummonedViaItem { get => Read(15) != 0; set => Write(15, value ? 1 : 0); }
    public List<BrainOfCthulhuAfterImage> BoCAfterImages { get => AnomalyNPC.BrainOfCthulhu_AfterImages; set => AnomalyNPC.BrainOfCthulhu_AfterImages = value; }

    private static readonly ProjectileDamageContainer _BrainIllusionDamage = new(30, 60, 90, 90, 90, 90);
    public static int BrainIllusionDamage => _BrainIllusionDamage.Value;

    private static readonly ProjectileDamageContainer _BloodShotDamage = new(24, 48, 72, 72, 72, 72);
    public static int BloodShotDamage => _BloodShotDamage.Value;

    private static readonly ProjectileDamageContainer _BloodScytheDamage = new(24, 48, 72, 72, 72, 72);
    public static int BloodScytheDamage => _BloodScytheDamage.Value;

    private static readonly ProjectileDamageContainer _IchorShotDamage = new(24, 48, 72, 72, 72, 72);
    public static int IchorShotDamage => _IchorShotDamage.Value;

    private static readonly ProjectileDamageContainer _CrimsonEyeDamage = new(24, 48, 72, 72, 72, 72);
    public static int CrimsonEyeDamage => _CrimsonEyeDamage.Value;

    private static readonly ProjectileDamageContainer _TelekineticBlastDamage = new(100, 200, 300, 300, 300, 300);
    public static int TelekineticBlastDamage => _TelekineticBlastDamage.Value;

    private static readonly ProjectileDamageContainer _TelekineticEnemyGrabDamage = new(20, 40, 60, 60, 60, 60);
    public static int TelekineticEnemyGrabDamage => _TelekineticEnemyGrabDamage.Value;

    private static readonly ProjectileDamageContainer _BloodBombDamage = new(80, 160, 240, 240, 240, 240);
    public static int BloodBombDamage => _BloodBombDamage.Value;

    public static float Phase1DefenseMultiplier => 1.5f; //Multiplies BoC's default defense value by this amount when in Phase 1.
    public static float ForcedIllusionHealthGate => 0.25f; //When BoC's health % falls below this value, the next attack is forced to be the Illusion Trick
    public static float DespawnRangeSQ => 36000000f;
    public static float DespawnRange => 6000f;
    public static int IdlePeriodDuration => 180;
    public static int CreeperChargeDelayMin => 70;
    public static int CreeperChargeDelayMax => 100;
    public static int CreeperChargePositioningTime => 60;
    public static int CreeperChargeWindUpTime => 22;
    public static int StunDuration => 360;
    public static int SwipesStartupDuration => 120;
    public int SwipeDuration => 60 + SwipeDelay;
    public int SwipeDelay => AttackFlag ? 30 : AnomalySharedData.Anomaly ? 40 : 50;
    public static int SwipeAmount => 4;
    public int SwipeIchorDelay => 30 + SwipeDelay;
    public static int LightSwipeDelay => 60;
    public static int LightSwipeAmount => AnomalySharedData.Anomaly ? 8 : 6;
    public static int LightSwipeTravelTime => 30;
    public static int LightSwipeAttackDelay => 10;
    public static int LightSwipeDuration => AnomalySharedData.Anomaly ? 45 : 60;
    public static int StrongSwipeDelay => 90;
    public static int StrongSwipeAmount => AnomalySharedData.Anomaly ? 7 : 5;
    public static int StrongSwipeTravelTime => 45;
    public static int StrongSwipeAttackDelay => 15;
    public static int StrongSwipeDuration => AnomalySharedData.Anomaly ? 60 : 80;
    public static int OrbitSetupDuration => 60;
    public static int OrbitDuration => 720;
    public static int OrbitAttackInterval => 120;
    public static int OrbitAttackParticipantCount => AnomalySharedData.Anomaly ? 4 : 3;
    public static float OrbitStandardRadius => 320f;
    public static float OrbitTelegraphRadius => 480f;
    public static float BaseRotationSpeed => 0.0175f;
    public static int SpiralDuration => 720;
    public static int SpiralSetupTime => 90;
    public static int TendrilCount => 3;
    public static float TendrilLength => 512;
    public static float TendrilStartDistance => 64;
    public static float MaxCreeperSway => 64;
    public static int StartingTimePerRevolutionMax => AnomalySharedData.Anomaly ? 270 : 300;
    public static int StartingTimePerRevolutionMin => AnomalySharedData.Anomaly ? 180 : 210;
    public static int EndingTimePerRevolutionMax => AnomalySharedData.Anomaly ? 210 : 240;
    public static int EndingTimePerRevolutionMin => AnomalySharedData.Anomaly ? 120 : 150;
    public static int SpeedUpDelayTime => 120;
    public static int SpeedUpExtensionTime => 120;
    public static float TurnAroundRatio => 0.6f; //In the second creeper phase, the creeper spiral will turn around at this completion percentage of the attack;
    public static float TurnAroundDurationRatio => AnomalySharedData.Anomaly ? 0.1f : 0.125f; //The amount of time that it'll take for the creeper spiral to turn around
    public static float DefaultTeleportDistance => 360f;
    public static int ChaseTime => 160;
    public static int ChaseAmount => 2;
    public static int IdleTeleportDuration => AnomalySharedData.Anomaly ? 36 : 44;
    public static float ChaseMinSpeed => 3;
    public static float ChaseMaxSpeed => AnomalySharedData.Anomaly ? 18 : 15;
    public static int BloodlettingDuration => 675;
    public static Vector2 HoverDistance => new(420f, 300f);
    public static float HoverEndHeight => 300f;
    public static int IchorRate => AnomalySharedData.Anomaly ? 10 : 12;
    public static float IchorSpread => 1.5f;
    public static float IchorVelocity => 3f;
    public static int BloodshotRate => 90;
    public static float BloodshotVelocity => 10f;
    public static int DashPrepTime => 90;
    public static int DashReelbackTime => 20;
    public static int DashDuration => 30;
    public static float DashVelocity => 32f;
    public static int DashScytheRate => AnomalySharedData.Anomaly ? 5 : 6;
    public static int SanguineTeleportCount => 5;
    public static int SanguineScytheCount => AnomalySharedData.Anomaly ? 12 : 10;
    public static int SanguineTeleportDuration => 30;
    public static float SanguineTeleportDistance => 440f;
    public static Vector2 SanguineFinalTeleportOffset => new(720, 300);
    public static int SanguineAttackEndDelay => 30;
    public static int SanguineAttackEndDuration => 100;
    public static int SanguineAttackEndIchorRate => 10;
    public static int CrimsonEyeAttackIdleDuration => 210;
    public static int CrimsonEyeAttackSetUpDuration => 30;
    public static int CrimsonEyeAttackBuildUpDuration => 120;
    public static int CrimsonEyeRate => 60;
    public static int CrimsonEyeCap => 40;
    public static int CrimsonEyeAttackDuration => 960;
    public static int CrimsonEyeAttackEndDuration => 210;
    public static float TurnAccelerationMultiplier => 0.01f; //Base multiplier for the boss' course correction rotation
    public static float TurnAccelerationDistanceBuffer => 160f; //Determines the minimum distance it must be at before it can begin redirecting.
    public static float TurnAccelerationDistanceDivisor => 72f; //Determines how much the distance affects its turn amount. Larger number means it must get further from the player in order to correct its course
    public static float IllusionDashTeleportDistance => 300f;
    public static int IllusionDashTeleportDuration => 30;
    public static float IllusionDashCloseInDistance => 280f;
    public static float IllusionDashStartingSpinSpeed => 0.125f;
    public static int IllusionDashSpinDuration => 100;
    public static int IllusionDashFakeoutTeleportDuration => 16;
    public static float IllusionDashVelocity => 30f;
    public static int IllusionTrickAngleGroups => AnomalySharedData.Anomaly ? 8 : 6;
    public static int IllusionTrickGroupSize => AnomalySharedData.Anomaly ? 5 : 4;
    public static int IllusionTrickStunDuration => 120;
    public static int IllusionTrickTimeLimit => 960;

    public enum Behavior : byte
    {
        //Spawn Animation
        SurfaceSpawnAnimation,
        //Phase 1
        Phase1Idle,
        CreeperSwipes,
        CreeperSwings,
        CreeperOrbit,
        CreeperSpiral,
        TelekineticOnslaught,
        Stunned,
        //Phase Transition
        Phase2TransitionClosed,
        Phase2TransitionOpen,
        //Phase 2
        Phase2Idle,
        CrimsonEyes,
        SanguineScythes,
        Bloodletting,
        IllusionDash,
        IllusionTrick,
        //Defeat
        DeathAnimation
    }

    public static float CreeperHPRatio
    {
        get
        {
            float ratio = 0f;
            foreach (NPC creeper in Main.npc.Where(n => n.active && n.type == NPCID.Creeper))
                if (!creeper.GetCreeperBehavior().IsPhantom)
                    ratio += creeper.life / (float)creeper.lifeMax;
            if (ratio != 0f)
                ratio /= CreeperCount;
            return ratio;
        }
    }

    public static float CreeperAmountRatio => NPC.CountNPCS(NPCID.Creeper) / (float)CreeperCount;

    public override void SetDefaults()
    {
        NPC.damage = NPC.defDamage = 36; // 64 (1.8x expert scaling)
        BoCDrawOffset = Vector2.Zero;
        ShieldOpacity = 1f;
        ShieldScale = 1f;
        BrainOfCthulhuSystem.ScreenBlurStrength = 0f;
        NPC.AddAnomalyExtraHPIndicator(0f, 0f);
        NPC.AddAnomalyHPIndicator(ForcedIllusionHealthGate, ForcedIllusionHealthGate, true);
        if (!Main.dedServ)
        {
            int brainOfCthuluCreepersCount = CreeperCount;
            BrainOfCthulhuSystem.VerletTendrils = new (int creeper, List<VerletSimulatedSegment> tendril, int reelInTimer)[brainOfCthuluCreepersCount];
            for (int i = 0; i < brainOfCthuluCreepersCount; i++)
            {
                List<VerletSimulatedSegment> tendril = [];
                for (int j = 0; j < 28; j++)
                    tendril.Add(new(NPC.Center));
                BrainOfCthulhuSystem.VerletTendrils[i].tendril = tendril;
            }
        }
    }

    public override bool? CanBeHitByProjectile(Projectile projectile)
    {
        if (CurrentBehavior == Behavior.IllusionTrick && !AttackFlag && projectile.Anomaly.BrainOfCthulhu_IgnoreIllusions)
            return false;
        return base.CanBeHitByProjectile(projectile);
    }

    public override void ModifyHitByItem(Player player, Item item, ref NPC.HitModifiers modifiers)
    {
        if (CurrentBehavior != Behavior.DeathAnimation)
            modifiers.SetMaxDamage(NPC.life - 1);
    }

    public override void ModifyHitByProjectile(Projectile projectile, ref NPC.HitModifiers modifiers)
    {
        if (CurrentBehavior != Behavior.DeathAnimation)
            modifiers.SetMaxDamage(NPC.life - 1);
    }

    public override void HitEffect(NPC.HitInfo hit)
    {
        if (CurrentBehavior != Behavior.DeathAnimation && (NPC.life + 1) <= hit.Damage)
        {
            TriggerDeathAnimation();
            return;
        }

        if (CurrentBehavior == Behavior.IllusionTrick && Time < 960)
        {
            AttackFlag = true;
            NPC.netUpdate = true;
        }
    }

    private void TriggerDeathAnimation()
    {
        NPC.life = 1;
        NPC.lifeRegen = 0;
        NPC.BossBar = null;
        NPC.dontTakeDamage = true;
        if (CurrentBehavior is Behavior.Stunned or Behavior.IllusionTrick)
            TeleportTime = 0;
        foreach (NPC n in NPC.ActiveNPCs)
        {
            if (n.type != ModContent.NPCType<FalseBrain>())
                continue;
            n.GetModNPC<FalseBrain>().BeenHit = true;
        }

        CurrentBehavior = Behavior.DeathAnimation;
        ResetAttackValues();
        Time = 0;
        NPC.netUpdate = true;
    }

    public const int CreeperCount = 30;
    public static List<int> GetAllValidTargets(Vector2 brainPosition)
    {
        List<int> validTargets = [];
        foreach (Player p in Main.ActivePlayers)
        {
            if (ValidateTarget(p, brainPosition))
                validTargets.Add(p.whoAmI);
        }

        return validTargets;
    }

    public static bool ValidateTarget(Player p, Vector2 brainPosition) => p.active && !p.dead && p.Center.DistanceSQ(brainPosition) <= DespawnRangeSQ;
    internal static float BringOpacityTo(float currentOpacity, float goalOpacity, float changeAmount = 0.025f)
    {
        if (currentOpacity == goalOpacity)
            return goalOpacity;
        if (currentOpacity < goalOpacity)
        {
            currentOpacity += changeAmount;
            if (currentOpacity >= goalOpacity)
                return goalOpacity;
            else
                return currentOpacity;
        }
        else
        {
            currentOpacity -= changeAmount;
            if (currentOpacity <= goalOpacity)
                return goalOpacity;
            else
                return currentOpacity;
        }
    }

    public override void ApplyCustomMainBossBarShader(BossHealthBar newBar, SpriteBatch spriteBatch, Rectangle destinationRentangle)
    {
        AnomalyEffects.BossBars.BrainofCthulhu.Data
            .UseImage1(AnomalyTextures.Noise._Smear)
            .UseImage2(AnomalyTextures.Noise._Vein)
            .SetCustomParameter("uScreenResolution", Main.ScreenSize.ToVector2() * Math.Max(Main.UIScale, 1f) / 2f)
            .SetCustomParameter("uPosition", destinationRentangle.TopLeft())
            .Apply();
    }
}
