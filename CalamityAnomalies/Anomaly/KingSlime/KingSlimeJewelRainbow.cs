// Designed by ColdsUx

using CalamityAnomalies.DataStructures;

namespace CalamityAnomalies.Anomaly.KingSlime;

public sealed class KingSlimeJewelRainbow : CAModNPC, IKingSlimeJewel
{
    public enum Attack
    {
        NormalAttack1 = 1,
        NormalAttack2 = 2,
        NormalAttack3 = 3,
        RingAttack = 4,
    }

    public const float DespawnDistance = 5000f;
    public static int ShootCooldownTime => 180;

    private static readonly ProjectileDamageContainer _jewelProjectileRainbowDamage = new(40, 65, 90, 110, 105, 125);
    public static int JewelProjectileRainbowDamage => _jewelProjectileRainbowDamage.Value;

    public Attack CurrentAttack
    {
        get => (Attack)(int)NPC.ai[0];
        set => NPC.ai[0] = (int)value;
    }

    public int AttackCounter
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

    public bool ShouldUseBuffedAttack
    {
        get => AI_Union_2.bits[1];
        set
        {
            Union32 union = AI_Union_2;
            union.bits[1] = value;
            AI_Union_2 = union;
        }
    }

    public bool HasEnteredPhase2
    {
        get => AI_Union_2.bits[2];
        set
        {
            Union32 union = AI_Union_2;
            union.bits[2] = value;
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

    public bool IsAttacking
    {
        get => AI_Union_2.bits[4];
        set
        {
            Union32 union = AI_Union_2;
            union.bits[4] = value;
            AI_Union_2 = union;
        }
    }

    public override string LocalizationCategory => "Anomaly.KingSlime";

    public override void SetStaticDefaults()
    {
        NPCID.Sets.TrailingMode[Type] = 1;
    }

    public override void SetDefaults()
    {
        NPC.aiStyle = -1;
        AIType = -1;
        NPC.damage = 25;
        NPC.width = 28;
        NPC.height = 28;
        NPC.defense = 10;

        NPC.lifeMax = 700;
        NPC.ApplyCalamityBossHealthBoost();

        NPC.knockBackResist = 0.2f;
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

        KingSlime_Anomaly masterBehavior = new() { _entity = master };

        Lighting.AddLight(NPC.Center, 1f, 0f, 0f);

        NPC.damage = 0;
        CalamityNPC.CanHaveBossHealthBar = true;

        if (!HasInitialized)
        {
            Projectile.NewProjectileAction<RainbowShockwave>(SourceAI, NPC.Center, Vector2.Zero, 100, 0f, action: p =>
            {
                p.scale = 0f;
                RainbowShockwave modP = p.GetModProjectile<RainbowShockwave>();
                modP.Jewel = NPC;
                modP.Master = master;
            });

            CanAttack = true;

            HasInitialized = true;
        }

        if (CanAttack)
        {
            KingSlime_Handler.Move(NPC, Target.Center, 15f, 15f, 0.175f, 0.125f, 250f, -250f, -250f, -400f);
            if (!IsAttacking)
                Timer1++;
        }
        else
        {
            KingSlime_Handler.Move(NPC, master.Center, 15f, 15f, 0.2f, 0.15f, 150f, -150f, 0f, -200f);
            Timer2 = Math.Max(Timer2 - 1, 0);
        }

        if (Timer1 >= ShootCooldownTime)
        {
            Timer1 = 0;
            EnterNextAttack();
        }

        if (CanAttack)
        {
            if (IsAttacking)
            {
                switch (CurrentAttack)
                {
                    case Attack.NormalAttack1:
                        NormalAttack1();
                        break;
                    case Attack.NormalAttack2:
                        NormalAttack2();
                        break;
                    case Attack.NormalAttack3:
                        NormalAttack3();
                        break;
                    case Attack.RingAttack:
                        RingAttack();
                        break;
                }
            }
        }
        else
            IsAttacking = false;

        NPC.netUpdate = true;

        return;

        void EnterNextAttack()
        {
            CurrentAttack = AttackCounter switch
            {
                0 => Attack.NormalAttack1,
                1 => Attack.NormalAttack2,
                2 => Attack.NormalAttack3,
                3 => Attack.RingAttack,
                _ => Attack.NormalAttack1,
            };
            AttackCounter = (AttackCounter + 1) % 4;
            ShouldUseBuffedAttack = masterBehavior.Phase2_2;
            IsAttacking = true;
        }

        void NormalAttack1()
        {
            SoundEngine.PlaySound(KingSlime_Handler.ShootSound, NPC.Center);
            for (int i = 0; i < 20; i++)
                KingSlime_Handler.SpawnOrbParticle(NPC, Main.rand.NextFloat(3f, 6f), Main.rand.Next(30, 50), Main.rand.NextFloat(0.4f, 0.7f));
            KingSlime_Handler.SpawnPointingParticle(NPC, 6, true);

            if (TOSharedData.GeneralClient)
            {
                int amount = 30;
                float totalAngle = MathHelper.TwoPi;
                float singleRadian = totalAngle / amount;
                Vector2 originalVelocity = (PolarVector2)NPC.GetVelocityTowards(Target, 20f);
                Vector2 rotatedOriginalVelocity = originalVelocity.RotatedBy(TOMathUtils.PiOver3 * 2);
                Vector2 rotatedOriginalVelocity2 = rotatedOriginalVelocity.RotatedBy(TOMathUtils.PiOver3 * 2);

                List<Vector2> originalVelocityList = [originalVelocity, rotatedOriginalVelocity, rotatedOriginalVelocity2, originalVelocity];

                for (int i = 0; i < amount; i++)
                {
                    Vector2 velocity = Vector2.LerpMany(originalVelocityList, (float)i / amount);

                    Projectile.NewProjectileAction<JewelProjectileRainbow>(SourceAI, NPC.Center, velocity, JewelProjectileRainbowDamage, 0f);
                    if (ShouldUseBuffedAttack)
                        Projectile.NewProjectileAction<JewelProjectileRainbow>(SourceAI, NPC.Center, velocity * -1f, JewelProjectileRainbowDamage, 0f);
                }
            }

            IsAttacking = false;
        }

        void NormalAttack2()
        {
            SoundEngine.PlaySound(KingSlime_Handler.ShootSound, NPC.Center);
            for (int i = 0; i < 20; i++)
                KingSlime_Handler.SpawnOrbParticle(NPC, Main.rand.NextFloat(3f, 6f), Main.rand.Next(30, 50), Main.rand.NextFloat(0.4f, 0.7f));
            KingSlime_Handler.SpawnPointingParticle(NPC, 6, true);

            if (TOSharedData.GeneralClient)
            {
                int amount = 60;
                float totalAngle = MathHelper.TwoPi;
                float singleRadian = totalAngle / amount;
                Vector2 originalVelocity = (PolarVector2)NPC.GetVelocityTowards(Target, 20f);
                Vector2 rotatedOriginalVelocity = originalVelocity.RotatedBy(TOMathUtils.PiOver5 * 4);
                Vector2 rotatedOriginalVelocity2 = rotatedOriginalVelocity.RotatedBy(TOMathUtils.PiOver5 * 4);
                Vector2 rotatedOriginalVelocity3 = rotatedOriginalVelocity2.RotatedBy(TOMathUtils.PiOver5 * 4);
                Vector2 rotatedOriginalVelocity4 = rotatedOriginalVelocity3.RotatedBy(TOMathUtils.PiOver5 * 4);

                List<Vector2> originalVelocityList = [originalVelocity, rotatedOriginalVelocity, rotatedOriginalVelocity2, rotatedOriginalVelocity3, rotatedOriginalVelocity4, originalVelocity];

                for (int i = 0; i < amount; i++)
                {
                    Vector2 velocity = Vector2.LerpMany(originalVelocityList, (float)i / amount);

                    Projectile.NewProjectileAction<JewelProjectileRainbow>(SourceAI, NPC.Center, velocity, JewelProjectileRainbowDamage, 0f);
                    if (ShouldUseBuffedAttack)
                        Projectile.NewProjectileAction<JewelProjectileRainbow>(SourceAI, NPC.Center, velocity * -0.75f, JewelProjectileRainbowDamage, 0f);
                }
            }

            IsAttacking = false;
        }

        void NormalAttack3()
        {
            Timer1++;

            if (TOSharedData.GeneralClient)
            {
                if (Timer1 == 1)
                    NormalAttack3Core();
                else if (Timer1 == 11)
                {
                    NormalAttack3Core(MathHelper.PiOver4);
                    if (!ShouldUseBuffedAttack)
                        IsAttacking = false;
                }
                else if (ShouldUseBuffedAttack && Timer1 == 21)
                {
                    NormalAttack3Core(MathHelper.PiOver4 + Main.rand.NextFloat(-0.2f, 0.2f));
                    IsAttacking = false;
                }
            }

            void NormalAttack3Core(float offset = 0f)
            {
                SoundEngine.PlaySound(KingSlime_Handler.ShootSound, NPC.Center);
                for (int i = 0; i < 20; i++)
                    KingSlime_Handler.SpawnOrbParticle(NPC, Main.rand.NextFloat(3f, 6f), Main.rand.Next(30, 50), Main.rand.NextFloat(0.4f, 0.7f));
                KingSlime_Handler.SpawnPointingParticle(NPC, 6, true);

                int amount = 32;
                float totalAngle = MathHelper.TwoPi;
                float singleRadian = totalAngle / amount;
                Vector2 originalVelocity = (PolarVector2)NPC.GetVelocityTowards(Target, 20f).RotatedBy(offset);
                Vector2 rotatedOriginalVelocity = originalVelocity.RotatedBy(MathHelper.PiOver2);
                Vector2 rotatedOriginalVelocity2 = rotatedOriginalVelocity.RotatedBy(MathHelper.PiOver2);
                Vector2 rotatedOriginalVelocity3 = rotatedOriginalVelocity2.RotatedBy(MathHelper.PiOver2);

                List<Vector2> originalVelocityList = [originalVelocity, rotatedOriginalVelocity, rotatedOriginalVelocity2, rotatedOriginalVelocity3, originalVelocity];

                for (int i = 0; i < amount; i++)
                {
                    Vector2 velocity = Vector2.LerpMany(originalVelocityList, (float)i / amount);
                    Projectile.NewProjectileAction<JewelProjectileRainbow>(SourceAI, NPC.Center, velocity, JewelProjectileRainbowDamage, 0f);
                }
            }
        }

        void RingAttack()
        {
            int totalAttackNum = ShouldUseBuffedAttack ? 8 : 6;

            if (Timer1 % 4 == 0)
            {
                int attackNum = Timer1 / 4;
                int orbParticleAmount = attackNum switch
                {
                    0 => 5,
                    1 => 5,
                    2 => 5,
                    3 => 10,
                    4 => 15,
                    5 => ShouldUseBuffedAttack ? 20 : 30,
                    6 => 15,
                    7 => 60,
                    _ => 0
                };
                int pointingParticleAmount = attackNum switch
                {
                    0 => 2,
                    1 => 2,
                    2 => 2,
                    3 => 3,
                    4 => 4,
                    5 => ShouldUseBuffedAttack ? 4 : 8,
                    6 => 5,
                    7 => 10,
                    _ => 0
                };
                SoundEngine.PlaySound(KingSlime_Handler.ShootSound, NPC.Center);
                for (int i = 0; i < orbParticleAmount; i++)
                    KingSlime_Handler.SpawnOrbParticle(NPC, Main.rand.NextFloat(3f, 6f), Main.rand.Next(30, 50), Main.rand.NextFloat(0.4f, 0.7f));
                KingSlime_Handler.SpawnPointingParticle(NPC, pointingParticleAmount, true);


                if (TOSharedData.GeneralClient)
                {
                    int amount = attackNum switch
                    {
                        0 => 3,
                        1 => 6,
                        2 => 9,
                        3 => 12,
                        4 => 16,
                        5 => 36,
                        6 => 24,
                        7 => 60,
                        _ => 0
                    };
                    float singleRadian = MathHelper.TwoPi / amount;
                    float radian = singleRadian * (amount - 1);
                    float initialRotation = (Target.Center - NPC.Center).ToRotation() + attackNum * TOMathUtils.PiOver5 + Main.rand.NextFloat(TOMathUtils.PiOver12);
                    switch (attackNum)
                    {
                        case <= 5:
                            Projectile.RotatedProj<JewelProjectileRainbow>(amount, singleRadian, SourceAI, NPC.Center, new PolarVector2(20f - attackNum / 2f, initialRotation), JewelProjectileRainbowDamage, 0f);
                            break;
                        case 6 or 7:
                            Projectile.RotatedProj<JewelProjectileRainbow>(amount, singleRadian, SourceAI, NPC.Center, new PolarVector2(6.5f - attackNum / 2f, initialRotation), JewelProjectileRainbowDamage, 0f, action: p => p.timeLeft = 300);
                            break;
                    }
                }
            }

            Timer1++;

            if (Timer1 > 4 * (totalAttackNum - 1))
                IsAttacking = false;
        }
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        DrawRainbowTrail(spriteBatch, screenPos, NPC, NPC.oldPos);

        float timeLeftGateValue = AttackCounter switch
        {
            2 => 45,
            3 => 55,
            _ => 40
        };
        float gateValue = ShootCooldownTime - timeLeftGateValue;
        float ratio = Timer1 > gateValue ? (Timer1 - gateValue) / timeLeftGateValue : 0f;
        float radius = AttackCounter switch
        {
            2 => 145f,
            3 => 160f,
            _ => 135f
        };
        float scale = AttackCounter switch
        {
            2 => 0.375f,
            3 => 0.425f,
            _ => 0.35f
        };
        KingSlime_Handler.DrawAttackEffect(spriteBatch, screenPos, NPC, ratio, radius, scale);
        KingSlime_Handler.DrawJewel(spriteBatch, screenPos, NPC, ratio);
        return false;
    }

    public static void DrawRainbowTrail(SpriteBatch spriteBatch, Vector2 screenPos, Entity entity, Vector2[] oldPos, SpriteEffects effects = SpriteEffects.None)
    {
        Texture2D texture = TOAssetUtils.GetProjectileTexture(ProjectileID.RainbowFront);
        Vector2 origin = new(texture.Width / 2, 0f);
        Color white = Color.White with { A = 127 };
        for (int i = oldPos.Length - 1; i > 0; i--)
        {
            if (oldPos[i] != Vector2.Zero)
            {
                Vector2 old = oldPos[i - 1];
                Vector2 oldold = oldPos[i];
                float rotation = (old - oldold).ToRotation(-MathHelper.PiOver2);
                Vector2 scale = new(1f, Vector2.Distance(oldold, old) / texture.Height);
                Color color = white * (1f - (float)i / oldPos.Length);
                spriteBatch.Draw(texture, oldold + entity.Size / 2f - screenPos, null, color, rotation, origin, scale, effects, 0f);
            }
        }
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
