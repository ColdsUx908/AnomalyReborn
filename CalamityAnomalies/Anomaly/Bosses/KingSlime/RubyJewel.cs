// Developed by ColdsUx

using CalamityAnomalies.DataStructures;

namespace CalamityAnomalies.Anomaly.Bosses.KingSlime;

public sealed class RubyJewel : JewelNPC
{
    public enum Behavior : byte
    {
        None = 0,
        NormalAttack,
        BuffedAttack,
    }

    public int ShootCooldownTime => HasEnteredPhase2 ? (Aroma ? 240 : 150) : (Aroma ? 180 : 120);

    private static readonly ProjectileDamageContainer _RubyJewelProjectileDamage = new(30, 52, 72, 84, 108, 132);
    public static int RubyJewelProjectileDamage => _RubyJewelProjectileDamage.Value;

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

    public override string LocalizationCategory => "Anomaly.KingSlime";

    public override void SetStaticDefaults() => NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, new NPCID.Sets.NPCBestiaryDrawModifiers() { Hide = true });

    public override void SetDefaults()
    {
        NPC.damage = 30;
        NPC.width = 30;
        NPC.height = 30;
        NPC.defense = 15;

        NPC.lifeMax = 250;
        NPC.ApplyCalamityBossHealthBoost();

        NPC.knockBackResist = 0.4f;
        NPC.noGravity = true;
        NPC.noTileCollide = true;
        NPC.HitSound = JewelHandler.HitSound;
        NPC.DeathSound = JewelHandler.ShatterSound;
        CalamityNPC.VulnerableToSickness = false;

        NPC.IsImportantBossMinion = true;
    }

    public override void ApplyDifficultyAndPlayerScaling(int numPlayers, float balance, float bossAdjustment) => NPC.lifeMax = (int)(NPC.lifeMax * balance);

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
            KingSlime masterBehavior = KingSlime.GetInstance(master);

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
            Projectile.NewProjectilesArc<RubyJewelProjectile>(amount, singleRadian, SourceAI, NPC.Center, new PolarVector2(Aroma ? 16f : 15f, initialRotation), RubyJewelProjectileDamage, 0f, Main.myPlayer, p =>
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
                int type = Aroma ? ModContent.ProjectileType<EmeraldJewelShadow>() : ModContent.ProjectileType<RubyJewelProjectile>();
                int amount1 = Aroma ? 9 : buff ? (Ultra ? 7 : 5) : (Ultra ? 5 : 3);
                Projectile.NewProjectilesArc(amount1, MathHelper.TwoPi / amount1, SourceAI, NPC.Center, NPC.GetVelocityTowards(NPC.PlayerTarget, Aroma ? 13.5f : 18f), type, RubyJewelProjectileDamage, 0f, Main.myPlayer, BuffedRubyProjectileAction);
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