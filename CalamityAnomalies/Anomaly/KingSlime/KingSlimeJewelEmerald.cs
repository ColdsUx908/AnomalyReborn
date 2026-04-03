using CalamityAnomalies.DataStructures;
using CalamityMod.Dusts;
using CalamityMod.Projectiles.Boss;

namespace CalamityAnomalies.Anomaly.KingSlime;

public class KingSlimeJewelEmerald : CAModNPC, IKingSlimeJewel
{
    public enum Behavior
    {
        Despawn = -1,

        FollowTarget = 0,
        Charge = 1,
    }

    public const float DespawnDistance = 5000f;
    public int ChargeCooldownTime => HasEnteredPhase2 ? 210 : 150;
    public int ChargePreparationTime => HasEnteredPhase2 ? 75 : 60;
    public static int ChargeTime => 60;
    public float ChargeSpeed => Ultra ? (HasEnteredPhase2 ? 24f : 28f) : (HasEnteredPhase2 ? 18f : 22f);

    private static readonly ProjectileDamageContainer _kingSlimeJewelEmeraldCloneDamage = new(30, 60, 90, 120, 102, 150);
    public static int KingSlimeJewelEmeraldCloneDamage => _kingSlimeJewelEmeraldCloneDamage.Value;

    public Behavior CurrentAttack
    {
        get => (Behavior)(int)NPC.ai[0];
        set => NPC.ai[0] = (int)value;
    }

    public int CurrentAttackPhase
    {
        get => (int)NPC.ai[1];
        set => NPC.ai[1] = value;
    }

    public bool HasInitialized
    {
        get => AI_Union_2.bits[0];
        set
        {
            Union32 union = AI_Union_2;
            union.bits[0] = value;
            AI_Union_2 = union;
        }
    }

    public bool HasEnteredPhase2
    {
        get => AI_Union_2.bits[1];
        set
        {
            Union32 union = AI_Union_2;
            union.bits[1] = value;
            AI_Union_2 = union;
        }
    }

    public bool CanAttack
    {
        get => AI_Union_2.bits[2];
        set
        {
            Union32 union = AI_Union_2;
            union.bits[2] = value;
            AI_Union_2 = union;
        }
    }

    public bool KingSlimeDead
    {
        get => AI_Union_2.bits[3];
        set
        {
            Union32 union = AI_Union_2;
            union.bits[3] = value;
            AI_Union_2 = union;
        }
    }

    public override string LocalizationCategory => "Anomaly.KingSlime";

    public override void SetStaticDefaults() => NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, new NPCID.Sets.NPCBestiaryDrawModifiers() { Hide = true });

    public override void SetDefaults()
    {
        NPC.aiStyle = -1;
        AIType = -1;
        NPC.damage = 30;
        NPC.width = 30;
        NPC.height = 30;
        NPC.defense = 15;

        NPC.lifeMax = 250;
        NPC.ApplyCalamityBossHealthBoost();

        NPC.knockBackResist = 0.4f;
        NPC.noGravity = true;
        NPC.noTileCollide = true;
        NPC.HitSound = SoundID.NPCHit5;
        NPC.DeathSound = SoundID.NPCDeath15;
        CalamityNPC.VulnerableToSickness = false;
    }

    public override void ApplyDifficultyAndPlayerScaling(int numPlayers, float balance, float bossAdjustment) => NPC.lifeMax = (int)(NPC.lifeMax * balance);

    public override void AI()
    {
        if (KingSlimeDead)
        {
            KingSlime_Handler.Kill(NPC);
            return;
        }

        if (!NPC.TryGetMaster(NPCID.KingSlime, out NPC master))
        {
            KingSlime_Handler.Despawn(NPC);
            return;
        }

        if (!NPC.TargetClosestIfInvalid(true, DespawnDistance))
        {
            NPC.Center = master.Top - new Vector2(0, master.height);
            return;
        }

        NPC.damage = 0;
        Lighting.AddLight(NPC.Center, 0f, 1f, 0f);

        if (!HasInitialized)
        {
            CanAttack = true;

            HasInitialized = true;
        }

        switch (CurrentAttack)
        {
            case Behavior.Despawn:
                KingSlime_Handler.Despawn(NPC);
                return;
            case Behavior.FollowTarget:
                FollowTarget();
                break;
            case Behavior.Charge:
                Charge();
                break;
        }

        NPC.netUpdate = true;

        void FollowTarget()
        {
            if (CanAttack)
            {
                KingSlime_Handler.Move(NPC, Target.Center, 15f, 12f, 0.2f, 0.125f, 350f, -350f, -200f, -400f);
                Timer1++;
            }
            else
            {
                KingSlime_Handler.Move(NPC, master.Center, 15f, 15f, 0.2f, 0.175f, 150f, -150f, 0f, -200f);
                Timer1 -= 2;
            }

            if (Timer1 >= ChargeCooldownTime)
            {
                Timer1 = 0;
                CurrentAttack = Behavior.Charge;
                NPC.netUpdate = true;
            }
        }

        void Charge()
        {
            NPC.knockBackResist = 0f;

            switch (CurrentAttackPhase)
            {
                case 0:
                    if (CanAttack)
                        Timer1++;
                    else
                    {
                        if ((Timer1 -= 2) <= ChargePreparationTime - 30)
                            CurrentAttack = Behavior.FollowTarget;
                        break;
                    }
                    if (Timer1 < ChargePreparationTime) //停止，旋转
                    {
                        NPC.velocity *= 0.94f;
                        NPC.rotation += (0.1f + (float)Timer1 / ChargePreparationTime * 0.4f) * NPC.direction;

                        Vector2 dustVelocity = Main.rand.NextPolarVector2(10.5f, 14.5f);
                        Dust.NewDustPerfectAction<SquashDust>(NPC.Center - dustVelocity.ToCustomLength(Main.rand.NextFloat(150f, 250f)), d =>
                        {
                            d.velocity = dustVelocity;
                            d.scale = Main.rand.NextFloat(0.9f, 1.2f);
                            d.noGravity = true;
                            d.fadeIn = 0.66f;
                            d.color = KingSlime_Handler.EmeraldColor;
                        });
                    }
                    else //冲刺
                    {
                        Timer1 = 0;
                        ChargeBehavior();
                    }
                    break;
                case 1: //冲刺中
                    if (CanAttack)
                        Timer1++;
                    else
                    {
                        Timer1 -= 2;
                        CurrentAttack = Behavior.FollowTarget;
                        break;
                    }
                    if (Timer1 >= ChargeTime)
                    {
                        Timer1 = 0;
                        for (int i = 0; i < 15; i++)
                            KingSlime_Handler.SpawnOrbParticle(NPC, Main.rand.NextFloat(2f, 3f), Main.rand.Next(20, 30), Main.rand.NextFloat(0.4f, 0.7f));
                        SoundEngine.PlaySound(SoundID.Item8, NPC.Center);
                        CurrentAttackPhase = 0;
                        CurrentAttack = Behavior.FollowTarget;
                        NPC.velocity = Vector2.Zero;
                        NPC.netUpdate = true;
                    }
                    else
                        NPC.damage = NPC.defDamage;
                    break;
            }

            void ChargeBehavior()
            {
                KingSlime_Anomaly kingSlimeBehavior = new() { _entity = master };
                bool validSapphire = !HasEnteredPhase2 && kingSlimeBehavior.HasSapphireBuff;
                NPC sapphire = validSapphire ? kingSlimeBehavior.JewelSapphire : null;

                SoundEngine.PlaySound(SoundID.Item38, NPC.Center);
                KingSlime_Handler.SpawnPointingParticle(NPC, 6, true);
                int particleAmount = HasEnteredPhase2 ? 10 : 15;
                if (validSapphire)
                    particleAmount += 25;
                for (int i = 0; i < particleAmount; i++)
                    KingSlime_Handler.SpawnOrbParticle(NPC, Main.rand.NextFloat(4f, 7f), Main.rand.Next(30, 45), Main.rand.NextFloat(0.4f, 0.7f));

                NPC.damage = NPC.defDamage;

                float chargeSpeed = ChargeSpeed;
                if (validSapphire)
                    chargeSpeed *= 1.2f;

                NPC.SetVelocityandRotation(NPC.GetVelocityTowards(Target, chargeSpeed), MathHelper.PiOver2);
                NPC.netSpam = 0;

                if (TOSharedData.GeneralClient && validSapphire)
                {
                    KingSlime_Handler.CreateDustFromJewelTo(sapphire, NPC.Center, Main.zenithWorld ? DustID.GemTopaz : DustID.GemSapphire);

                    int type = Main.zenithWorld ? ModContent.ProjectileType<JewelProjectile>() : ModContent.ProjectileType<KingSlimeJewelEmeraldShadow>();
                    Vector2 velocityUnit = NPC.GetVelocityTowards(NPC.PlayerTarget, 1f);
                    Vector2 offset = velocityUnit.RotatedBy(MathHelper.PiOver2);
                    int amount = Ultra ? 4 : 3;
                    for (int i = -amount; i <= amount; i++)
                    {
                        Projectile.NewProjectileAction(SourceAI, NPC.Center + offset * 24f * i + velocityUnit * (60f - 20f * Math.Abs(i)), velocityUnit * chargeSpeed, type, KingSlimeJewelEmeraldCloneDamage, 0f, Main.myPlayer, p =>
                        {
                            if (Main.zenithWorld)
                                p.timeLeft = 60;
                            else
                                p.VelocityToRotation(MathHelper.PiOver2);
                        });
                    }
                }

                CurrentAttackPhase = 1;
                NPC.netUpdate = true;
            }
        }
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        float timeLeftGateValue = 30f;
        float gateValue = ChargePreparationTime - timeLeftGateValue;
        float ratio = CurrentAttack == Behavior.Charge && CurrentAttackPhase == 0 && Timer1 > gateValue ? (Timer1 - gateValue) / timeLeftGateValue : 0f;
        KingSlime_Handler.DrawAttackEffect(spriteBatch, screenPos, NPC, ratio, 120f, 0.35f);
        KingSlime_Handler.DrawJewel(spriteBatch, screenPos, NPC, ratio);
        return false;
    }

    public override bool CheckActive() => false;

    public override bool CheckDead()
    {
        if (Ultra && !KingSlimeDead)
        {
            NPC.life = 1;
            NPC.active = true;
            if (!HasEnteredPhase2)
                KingSlime_Handler.EnterPhase2(NPC);
            return false;
        }
        return true;
    }

    public override void HitEffect(NPC.HitInfo hit)
    {
        KingSlime_Handler.HitEffect(NPC);
    }

    public override void OnKill()
    {
        KingSlime_Handler.OnKill(NPC);
    }
}
