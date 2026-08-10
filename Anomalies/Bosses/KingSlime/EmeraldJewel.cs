// Developed by ColdsUx

using Anomalies.DataStructures;
using Anomalies.GameContents.Dusts;

namespace Anomalies.Bosses.KingSlime;

public sealed class EmeraldJewel : JewelNPC
{
    public enum Behavior : byte
    {
        None = 0,
        Charge,
    }

    public static int ChargeTime => 60;
    public float ChargeSpeed => Ultra ? (HasEnteredPhase2 ? 24f : 28f) : (HasEnteredPhase2 ? 18f : 22f);

    private static readonly ProjectileDamageContainer _EmeraldJewelShadowDamage = new(30, 60, 90, 120, 120, 180);
    public static int EmeraldJewelShadowDamage => _EmeraldJewelShadowDamage.Value;

    public Behavior CurrentBehavior
    {
        get => (Behavior)AI_Union_0.byte0;
        set
        {
            Union32 union = AI_Union_0;
            union.byte0 = (byte)value;
            AI_Union_0 = union;
        }
    }

    public override string LocalizationCategory => "Bosses.KingSlime";

    public override void SetStaticDefaults() => NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, new NPCID.Sets.NPCBestiaryDrawModifiers() { Hide = true });

    public override void SetDefaults()
    {
        NPC.damage = 30;
        NPC.width = 30;
        NPC.height = 30;
        NPC.defense = 15;

        NPC.lifeMax = 250;
        BridgeUtils.ApplyCalamityHealthBoost(NPC);

        NPC.knockBackResist = 0.4f;
        NPC.noGravity = true;
        NPC.noTileCollide = true;
        NPC.HitSound = JewelHandler.HitSound;
        NPC.DeathSound = JewelHandler.ShatterSound;

        NPC.IsImportantBossMinion = true;
    }

    public override void ApplyDifficultyAndPlayerScaling(int numPlayers, float balance, float bossAdjustment) => NPC.lifeMax = (int)(NPC.lifeMax * balance);

    public override void AI()
    {
        if (MasterDead)
        {
            JewelHandler.Kill(NPC);
            return;
        }

        if (!NPC.TryGetMaster(NPCID.KingSlime, out NPC master))
        {
            JewelHandler.Despawn(NPC);
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

        switch (CurrentBehavior)
        {
            case Behavior.None:
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
                JewelHandler.Move(NPC, Target.Center, 15f, 12f, 0.2f, 0.125f, 350f, -350f, -200f, -400f);
            else
                JewelHandler.Move(NPC, master.Center, 15f, 15f, 0.2f, 0.175f, 150f, -150f, 0f, -200f);
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
                        CurrentBehavior = Behavior.None;
                        Timer1 = 0;
                        break;
                    }

                    NPC.velocity *= 0.94f;
                    NPC.rotation += (0.1f + Timer1 / 135f) * NPC.direction;

                    Vector2 dustVelocity = Main.rand.NextPolarVector2(10.5f, 14.5f);
                    Dust.NewDustPerfectAction<SquashDust>(NPC.Center - dustVelocity.ToCustomLength(Main.rand.NextFloat(150f, 250f)), d =>
                    {
                        d.velocity = dustVelocity;
                        d.scale = Main.rand.NextFloat(0.9f, 1.2f);
                        d.noGravity = true;
                        d.fadeIn = 0.5f;
                        d.color = JewelHandler.EmeraldColor;
                    });

                    if (Timer1 > 150) //正常情况下这里不应该被触发，因为开始冲刺由史莱姆王控制
                        CurrentAttackPhase = 1;
                    break;

                case 1: //冲刺
                    KingSlime kingSlimeBehavior = new() { _Entity = master };
                    bool validSapphire = !HasEnteredPhase2 && kingSlimeBehavior.HasSapphireBuff;
                    NPC sapphire = validSapphire ? kingSlimeBehavior.JewelSapphire : null;

                    SoundEngine.PlaySound(validSapphire ? JewelHandler.DashSoundBuff : JewelHandler.DashSoundNormal, NPC.Center);
                    JewelHandler.SpawnPointingParticle(NPC, 6, true);
                    int particleAmount = HasEnteredPhase2 ? 10 : 15;
                    if (validSapphire)
                        particleAmount += 25;
                    for (int i = 0; i < particleAmount; i++)
                        JewelHandler.SpawnOrbParticle(NPC, Main.rand.NextFloat(4f, 7f), Main.rand.Next(30, 45), Main.rand.NextFloat(0.4f, 0.7f));

                    JewelHandler.CreateDustFromJewelTo(NPC, master.Center, Aroma ? DustID.GemAmethyst : DustID.GemEmerald);
                    if (validSapphire)
                        JewelHandler.CreateDustFromJewelTo(sapphire, NPC.Center, Aroma ? DustID.GemTopaz : DustID.GemSapphire);

                    NPC.damage = NPC.defDamage;

                    float chargeSpeed = ChargeSpeed;
                    if (validSapphire)
                        chargeSpeed *= 1.2f;

                    NPC.SetVelocityandRotation(NPC.GetVelocityTowards(Target, chargeSpeed), MathHelper.PiOver2);
                    NPC.netSpam = 0;

                    if (TOSharedData.NotClient && validSapphire)
                    {
                        int type = Aroma ? ModContent.ProjectileType<RubyJewelProjectile>() : ModContent.ProjectileType<EmeraldJewelShadow>();
                        Vector2 velocityUnit = NPC.GetVelocityTowards(NPC.PlayerTarget, 1f);
                        Vector2 offset = velocityUnit.RotatedBy(MathHelper.PiOver2);
                        int amount = Ultra ? 4 : 3;
                        for (int i = -amount; i <= amount; i++)
                        {
                            Projectile.NewProjectileAction(SourceAI, NPC.Center + offset * 24f * i + velocityUnit * (60f - 20f * Math.Abs(i)), velocityUnit * chargeSpeed, type, EmeraldJewelShadowDamage, 0f, Main.myPlayer, p =>
                            {
                                if (Aroma)
                                    p.timeLeft = 60;
                                else
                                    p.VelocityToRotation(MathHelper.PiOver2);
                            });
                        }
                    }

                    Timer1 = 0;
                    CurrentAttackPhase = 2;
                    break;

                case 2: //冲刺中
                    if (CanAttack)
                        Timer1++;
                    else
                    {
                        Timer1 = 0;
                        CurrentBehavior = Behavior.None;
                        break;
                    }
                    if (Timer1 >= ChargeTime)
                    {
                        Timer1 = 0;
                        for (int i = 0; i < 15; i++)
                            JewelHandler.SpawnOrbParticle(NPC, Main.rand.NextFloat(2f, 3f), Main.rand.Next(20, 30), Main.rand.NextFloat(0.4f, 0.7f));
                        SoundEngine.PlaySound(SoundID.Item8, NPC.Center);
                        CurrentAttackPhase = 0;
                        CurrentBehavior = Behavior.None;
                        Timer1 = 0;
                        NPC.velocity = Vector2.Zero;
                        NPC.netUpdate = true;
                    }
                    else
                        NPC.damage = NPC.defDamage;
                    break;
            }
        }
    }

    public override bool CheckDead()
    {
        if (Ultra && !MasterDead)
        {
            NPC.life = 1;
            NPC.active = true;
            if (!HasEnteredPhase2)
                JewelHandler.EnterPhase2(NPC);
            return false;
        }
        return true;
    }
}
