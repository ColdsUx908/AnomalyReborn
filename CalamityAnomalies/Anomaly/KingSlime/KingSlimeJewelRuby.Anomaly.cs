// Developed by ColdsUx

using CalamityAnomalies.DataStructures;
using CalamityMod.NPCs.NormalNPCs;
using CalamityMod.Projectiles.Boss;

namespace CalamityAnomalies.Anomaly.KingSlime;

public sealed class KingSlimeJewelRuby_Anomaly : AnomalyNPCBehavior<KingSlimeJewelRuby>, IKingSlimeJewel
{
    public const float DespawnDistance = 5000f;

    public int ShootCooldownTime => HasEnteredPhase2 ? (Aroma ? 240 : 150) : (Aroma ? 180 : 120);

    private static readonly ProjectileDamageContainer _jewelProjectileDamage = new(30, 52, 72, 84, 84, 102);
    public static int JewelProjectileDamage => _jewelProjectileDamage.Value;

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

    public override void SetDefaults()
    {
        NPC.lifeMax = 250;
        NPC.ApplyCalamityBossHealthBoost();
        NPC.width = 30;
        NPC.height = 30;
        NPC.knockBackResist = 0.4f;

        NPC.HitSound = KingSlime_Handler.HitSound;
        NPC.DeathSound = KingSlime_Handler.ShatterSound;
    }

    public override bool PreAI()
    {
        if (KingSlimeDead)
        {
            KingSlime_Handler.Kill(NPC);
            return false;
        }

        if (!NPC.TryGetMaster(NPCID.KingSlime, out NPC master))
        {
            KingSlime_Handler.Despawn(NPC);
            return false;
        }

        if (!NPC.TargetClosestIfInvalid(true, DespawnDistance))
        {
            NPC.Center = master.Top - new Vector2(0, master.height);
            return false;
        }

        NPC.damage = 0;
        Lighting.AddLight(NPC.Center, 1f, 0f, 0f);

        if (!HasInitialized)
        {
            CanAttack = true;
            HasInitialized = true;
        }

        if (CanAttack)
            KingSlime_Handler.Move(NPC, Target.Center, 13f, 13f, 0.15f, 0.11f, 150f, -150f, -300f, -400f);
        else
            KingSlime_Handler.Move(NPC, master.Center, 13f, 13f, 0.2f, 0.175f, 150f, -150f, 0f, -200f);

        KingSlime_Anomaly masterBehavior = KingSlime_Anomaly.GetNewInstance(master);
        if (CanAttack && CheckShoot(masterBehavior, out bool buff))
            Shoot(buff);

        NPC.netUpdate = true;

        return false;

        void Shoot(bool buff)
        {
            bool validSapphire = !HasEnteredPhase2 && masterBehavior.HasSapphireBuff;
            NPC sapphire = validSapphire ? masterBehavior.JewelSapphire : null;

            SoundEngine.PlaySound(KingSlime_Handler.ShootSound, NPC.Center);
            int particleAmount = Aroma ? 30 : 20;
            if (validSapphire)
                particleAmount += 20;
            for (int i = 0; i < particleAmount; i++)
                KingSlime_Handler.SpawnOrbParticle(NPC, Main.rand.NextFloat(3f, 6f), Main.rand.Next(30, 45), Main.rand.NextFloat(0.4f, 0.7f));
            KingSlime_Handler.SpawnPointingParticle(NPC, 6, true);

            KingSlime_Handler.CreateDustFromJewelTo(NPC, master.Center, Aroma ? DustID.IceTorch : DustID.GemRuby);
            if (validSapphire)
                KingSlime_Handler.CreateDustFromJewelTo(sapphire, NPC.Center, Aroma ? DustID.GemTopaz : DustID.GemSapphire);

            if (!TOSharedData.NotClient)
                return;

            int amount = HasEnteredPhase2 ? (Aroma ? 7 : buff && Ultra ? 3 : 1) : (Aroma ? 17 : buff ? (Ultra ? 5 : 3) : (Ultra ? 3 : 1));
            float singleRadian = MathHelper.ToRadians(HasEnteredPhase2 ? (Aroma ? 18f : 10f) : (Aroma ? 18f : 13.5f));
            float radian = singleRadian * (amount - 1);
            float initialRotation = (Target.Center - NPC.Center).ToRotation() - radian / 2f;
            Projectile.RotatedProj<JewelProjectile>(amount, singleRadian, SourceAI, NPC.Center, new PolarVector2(Aroma ? 16f : 15f, initialRotation), JewelProjectileDamage, 0f, Main.myPlayer, p =>
            {
                if (Aroma)
                {
                    p.velocity.Modulus *= Main.rand.NextFloat(0.7f, TOSharedData.LegendaryMode ? 1f : 0.85f);
                    if (TOSharedData.LegendaryMode)
                        p.velocity.Rotation += Main.rand.NextFloat(-0.15f, 0.15f);
                }
            });

            if (validSapphire)
            {
                int type = Aroma ? ModContent.ProjectileType<KingSlimeJewelEmeraldShadow>() : ModContent.ProjectileType<JewelProjectile>();
                int amount1 = Aroma ? 9 : buff ? (Ultra ? 7 : 5) : (Ultra ? 5 : 3);
                Projectile.RotatedProj(amount1, MathHelper.TwoPi / amount1, SourceAI, NPC.Center, NPC.GetVelocityTowards(NPC.PlayerTarget, Aroma ? 13.5f : 18f), type, JewelProjectileDamage, 0f, Main.myPlayer, BuffedRubyProjectileAction);
            }

            void BuffedRubyProjectileAction(Projectile p)
            {
                if (Aroma)
                {
                    p.velocity.Modulus *= Main.rand.NextFloat(1.2f, TOSharedData.LegendaryMode ? 1.5f : 1.3f);
                    if (TOSharedData.LegendaryMode)
                        p.velocity.Rotation += Main.rand.NextFloat(-0.2f, 0.2f);
                    p.VelocityToRotation(MathHelper.PiOver2);
                    p.timeLeft = (int)(p.timeLeft * (TOSharedData.LegendaryMode ? 2.25f : 1.5f));
                }
            }
        }
    }

    public static bool CheckMasterJump(KingSlime_Anomaly behavior) => 
        behavior.CurrentBehavior is KingSlime_Anomaly.Behavior.FirstJump or KingSlime_Anomaly.Behavior.NormalJump or KingSlime_Anomaly.Behavior.HighJump or KingSlime_Anomaly.Behavior.RapidJump
        && behavior.CurrentAttackPhase == 0;

    public static bool CheckShoot(KingSlime_Anomaly behavior, out bool buff)
    {
        if (CheckMasterJump(behavior) && behavior.Timer1 == KingSlime_Anomaly.JumpDelay)
        {
            buff = behavior.CurrentBehavior is KingSlime_Anomaly.Behavior.FirstJump or KingSlime_Anomaly.Behavior.HighJump;
            return true;
        }
        else
        {
            buff = false;
            return false;
        }
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        KingSlime_Handler.DrawJewel(spriteBatch, screenPos, NPC);
        return false;
    }

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
}

public sealed class KingSlimeJewelRuby_AnomalyDetour : ModNPCDetour<KingSlimeJewelRuby>
{
    public override void Detour_HitEffect(Orig_HitEffect orig, KingSlimeJewelRuby self, NPC.HitInfo hit)
    {
        if (CASharedData.Anomaly)
            KingSlime_Handler.HitEffect(self.NPC);
        else
            orig(self, hit);
    }

    public override void Detour_OnKill(Orig_OnKill orig, KingSlimeJewelRuby self)
    {
        if (CASharedData.Anomaly)
            KingSlime_Handler.OnKill(self.NPC);
        else
            orig(self);
    }
}