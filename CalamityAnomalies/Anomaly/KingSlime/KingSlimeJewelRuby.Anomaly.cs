// Developed by ColdsUx

using CalamityAnomalies.DataStructures;
using CalamityMod.NPCs.NormalNPCs;
using CalamityMod.Projectiles.Boss;

namespace CalamityAnomalies.Anomaly.KingSlime;

public sealed class KingSlimeJewelRuby_Anomaly : AnomalyNPCBehavior<KingSlimeJewelRuby, KingSlimeJewelRuby_Anomaly>, IKingSlimeJewel
{
    public enum Behavior : byte
    {
        None = 0,
        NormalAttack,
        BuffedAttack,
    }

    public const float DespawnDistance = 5000f;

    public int ShootCooldownTime => HasEnteredPhase2 ? (Aroma ? 240 : 150) : (Aroma ? 180 : 120);

    private static readonly ProjectileDamageContainer _jewelProjectileDamage = new(30, 52, 72, 84, 108, 132);
    public static int JewelProjectileDamage => _jewelProjectileDamage.Value;

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

    public bool MasterDead
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

        NPC.HitSound = JewelHandler.HitSound;
        NPC.DeathSound = JewelHandler.ShatterSound;

        NPC.IsImportantBossMinion = true;
    }

    public override bool PreAI()
    {
        if (MasterDead)
        {
            JewelHandler.Kill(NPC);
            return false;
        }

        if (!NPC.TryGetMaster(NPCID.KingSlime, out NPC master))
        {
            JewelHandler.Despawn(NPC);
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
        {
            JewelHandler.Move(NPC, Target.Center, 13f, 13f, 0.15f, 0.11f, 150f, -150f, -300f, -400f);

            (bool shouldAttack, bool buff) = CurrentBehavior switch
            {
                Behavior.None => (false, false),
                Behavior.NormalAttack => (true, false),
                Behavior.BuffedAttack => (true, true),
                _ => (false, false),
            };

            if (shouldAttack)
            {
                Shoot(buff);
                CurrentBehavior = Behavior.None;
            }
        }
        else
        {
            JewelHandler.Move(NPC, master.Center, 13f, 13f, 0.2f, 0.175f, 150f, -150f, 0f, -200f);
            CurrentBehavior = Behavior.None;
        }

        NPC.netUpdate = true;

        return false;

        void Shoot(bool buff)
        {
            KingSlime_Anomaly masterBehavior = KingSlime_Anomaly.GetInstance(master);

            bool validSapphire = !HasEnteredPhase2 && masterBehavior.HasSapphireBuff;
            NPC sapphire = validSapphire ? masterBehavior.JewelSapphire : null;

            SoundEngine.PlaySound(JewelHandler.ShootSound, NPC.Center);
            int particleAmount = Aroma ? 30 : 20;
            if (validSapphire)
                particleAmount += 20;
            for (int i = 0; i < particleAmount; i++)
                JewelHandler.SpawnOrbParticle(NPC, Main.rand.NextFloat(3f, 6f), Main.rand.Next(30, 45), Main.rand.NextFloat(0.4f, 0.7f));
            JewelHandler.SpawnPointingParticle(NPC, 6, true);

            JewelHandler.CreateDustFromJewelTo(NPC, master.Center, Aroma ? DustID.IceTorch : DustID.GemRuby);
            if (validSapphire)
                JewelHandler.CreateDustFromJewelTo(sapphire, NPC.Center, Aroma ? DustID.GemTopaz : DustID.GemSapphire);

            if (!TOSharedData.NotClient)
                return;

            int amount = HasEnteredPhase2 ? (Aroma ? 7 : buff && Ultra ? 3 : 1) : (Aroma ? 17 : buff ? (Ultra ? 5 : 3) : (Ultra ? 3 : 1));
            float singleRadian = MathHelper.ToRadians(HasEnteredPhase2 ? (Aroma ? 18f : 10f) : (Aroma ? 18f : 13.5f));
            float initialRotation = (Target.Center - NPC.Center).ToRotation();
            Projectile.NewProjectilesArc<JewelProjectile>(amount, singleRadian, SourceAI, NPC.Center, new PolarVector2(Aroma ? 16f : 15f, initialRotation), JewelProjectileDamage, 0f, Main.myPlayer, p =>
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
                Projectile.NewProjectilesArc(amount1, MathHelper.TwoPi / amount1, SourceAI, NPC.Center, NPC.GetVelocityTowards(NPC.PlayerTarget, Aroma ? 13.5f : 18f), type, JewelProjectileDamage, 0f, Main.myPlayer, BuffedRubyProjectileAction);
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

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        JewelHandler.DrawJewel(spriteBatch, screenPos, NPC);
        return false;
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

public sealed class KingSlimeJewelRuby_AnomalyDetour : ModNPCDetour<KingSlimeJewelRuby>
{
    public override void Detour_HitEffect(Orig_HitEffect orig, KingSlimeJewelRuby self, NPC.HitInfo hit)
    {
        if (CASharedData.Anomaly)
            JewelHandler.HitEffect(self.NPC);
        else
            orig(self, hit);
    }

    public override void Detour_OnKill(Orig_OnKill orig, KingSlimeJewelRuby self)
    {
        if (CASharedData.Anomaly)
            JewelHandler.OnKill(self.NPC);
        else
            orig(self);
    }
}