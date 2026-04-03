using CalamityAnomalies.DataStructures;
using CalamityMod.NPCs.NormalNPCs;
using CalamityMod.Projectiles.Boss;

namespace CalamityAnomalies.Anomaly.KingSlime;

public sealed class KingSlimeJewelRuby_Anomaly : AnomalyNPCBehavior<KingSlimeJewelRuby>, IKingSlimeJewel
{
    public const float DespawnDistance = 5000f;

    public int ShootCooldownTime => HasEnteredPhase2 ? (Main.zenithWorld ? 240 : 120) : (Main.zenithWorld ? 180 : 90);

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
        {
            KingSlime_Handler.Move(NPC, Target.Center, 15f, 15f, 0.175f, 0.125f, 250f, -250f, -250f, -400f);
            Timer1++;
        }
        else
        {
            KingSlime_Handler.Move(NPC, master.Center, 15f, 15f, 0.2f, 0.175f, 150f, -150f, 0f, -200f);
            Timer1 -= 2;
        }

        if (Timer1 >= ShootCooldownTime)
        {
            Timer1 = 0;
            Shoot();
        }

        NPC.netUpdate = true;

        return false;

        void Shoot()
        {
            KingSlime_Anomaly kingSlimeBehavior = new() { _entity = master };
            bool validSapphire = !HasEnteredPhase2 && kingSlimeBehavior.HasSapphireBuff;
            NPC sapphire = validSapphire ? kingSlimeBehavior.JewelSapphire : null;

            SoundEngine.PlaySound(KingSlime_Handler.ShootSound, NPC.Center);
            int particleAmount = Main.zenithWorld ? 30 : 20;
            if (validSapphire)
                particleAmount += 20;
            for (int i = 0; i < particleAmount; i++)
                KingSlime_Handler.SpawnOrbParticle(NPC, Main.rand.NextFloat(3f, 6f), Main.rand.Next(30, 45), Main.rand.NextFloat(0.4f, 0.7f));
            KingSlime_Handler.SpawnPointingParticle(NPC, 6, true);

            if (!TOSharedData.GeneralClient)
                return;

            int amount = HasEnteredPhase2 ? (Main.zenithWorld ? 7 : Ultra ? 3 : 1) : (Main.zenithWorld ? 17 : Ultra ? 5 : 3);
            float singleRadian = MathHelper.ToRadians(HasEnteredPhase2 ? (Main.zenithWorld ? 18f : 9f) : (Main.zenithWorld ? 18f : Ultra ? 13.5f : 12f));
            float radian = singleRadian * (amount - 1);
            float initialRotation = (Target.Center - NPC.Center).ToRotation() - radian / 2f;
            Projectile.RotatedProj<JewelProjectile>(amount, singleRadian, SourceAI, NPC.Center, new PolarVector2(Main.zenithWorld ? 16f : 15f, initialRotation), JewelProjectileDamage, 0f, Main.myPlayer, p =>
            {
                if (Main.zenithWorld)
                {
                    p.velocity.Modulus *= Main.rand.NextFloat(0.7f, TOSharedData.LegendaryMode ? 1f : 0.85f);
                    if (TOSharedData.LegendaryMode)
                        p.velocity.Rotation += Main.rand.NextFloat(-0.15f, 0.15f);
                }
            });

            if (validSapphire)
            {
                KingSlime_Handler.CreateDustFromJewelTo(sapphire, NPC.Center, Main.zenithWorld ? DustID.GemTopaz : DustID.GemSapphire);

                int type = Main.zenithWorld ? ModContent.ProjectileType<KingSlimeJewelEmeraldShadow>() : ModContent.ProjectileType<JewelProjectile>();
                int amount1 = Ultra ? 7 : Main.zenithWorld ? 9 : 5;
                Projectile.RotatedProj(amount1, MathHelper.TwoPi / amount1, SourceAI, NPC.Center, NPC.GetVelocityTowards(NPC.PlayerTarget, Main.zenithWorld ? 13.5f : 18f), type, JewelProjectileDamage, 0f, Main.myPlayer, BuffedRubyProjectileAction);
            }

            void BuffedRubyProjectileAction(Projectile p)
            {
                if (Main.zenithWorld)
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
        float timeLeftGateValue = 30f;
        float gateValue = ShootCooldownTime - timeLeftGateValue;
        float ratio = Timer1 > gateValue ? (Timer1 - gateValue) / timeLeftGateValue : 0f;
        KingSlime_Handler.DrawAttackEffect(spriteBatch, screenPos, NPC, ratio, Main.zenithWorld ? 120f : 100f, 0.35f);
        KingSlime_Handler.DrawJewel(spriteBatch, screenPos, NPC, ratio);
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