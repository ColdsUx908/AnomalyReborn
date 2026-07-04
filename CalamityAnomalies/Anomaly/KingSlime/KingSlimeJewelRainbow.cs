// Developed by ColdsUx

using CalamityAnomalies.DataStructures;

namespace CalamityAnomalies.Anomaly.KingSlime;

public sealed class KingSlimeJewelRainbow : CAModNPC, IKingSlimeJewel
{
    public enum Behavior : byte
    {
        None = 0,
        NormalAttack,
        TriangleAttack,
        StarAttack,
        SquareAttack,
        CircleAttack,
    }

    public const float DespawnDistance = 5000f;

    private static readonly ProjectileDamageContainer _jewelProjectileRainbowDamage = new(40, 60, 90, 120, 120, 150);
    public static int JewelProjectileRainbowDamage => _jewelProjectileRainbowDamage.Value;

    public const float MaxProjectileSpeed = 18f;

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

    public override string LocalizationCategory => "Anomaly.KingSlime";

    public override void SetStaticDefaults()
    {
        NPCID.Sets.TrailingMode[Type] = 1;
    }

    public override void SetDefaults()
    {
        NPC.damage = 25;
        NPC.width = 28;
        NPC.height = 28;
        NPC.defense = 10;

        NPC.lifeMax = 700;
        NPC.ApplyCalamityBossHealthBoost();

        NPC.knockBackResist = 0.2f;
        NPC.noGravity = true;
        NPC.noTileCollide = true;
        NPC.HitSound = JewelHandler.HitSound;
        NPC.DeathSound = JewelHandler.ShatterSound;
        CalamityNPC.VulnerableToSickness = false;

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
            JewelHandler.Move(NPC, Target.Center, 15f, 15f, 0.175f, 0.125f, 250f, -250f, -250f, -400f);

            switch (CurrentBehavior)
            {
                case Behavior.NormalAttack:
                    Attack_Normal();
                    break;
                case Behavior.TriangleAttack:
                    Attack_Triangle();
                    break;
                case Behavior.StarAttack:
                    Attack_Star();
                    break;
                case Behavior.SquareAttack:
                    Attack_Square();
                    break;
                case Behavior.CircleAttack:
                    Attack_Circle();
                    break;
            }
        }
        else
        {
            JewelHandler.Move(NPC, master.Center, 15f, 15f, 0.2f, 0.15f, 150f, -150f, 0f, -200f);
            CurrentBehavior = Behavior.None;
            Timer1 = 0;
        }

        NPC.netUpdate = true;

        return;

        void Attack_Normal()
        {
            SoundEngine.PlaySound(JewelHandler.ShootSound, NPC.Center);
            for (int i = 0; i < 20; i++)
                JewelHandler.SpawnOrbParticle(NPC, Main.rand.NextFloat(3f, 6f), Main.rand.Next(30, 50), Main.rand.NextFloat(0.4f, 0.7f));
            JewelHandler.SpawnPointingParticle(NPC, 6, true);

            JewelHandler.CreateDustFromJewelTo(NPC, master.Center, -1, true);

            if (TOSharedData.NotClient)
            {
                int amount = 9;
                float totalAngle = MathHelper.TwoPi;
                float singleRadian = totalAngle / amount;
                Vector2 originalVelocity = NPC.GetVelocityTowards(Target, MaxProjectileSpeed * 0.85f);
                Projectile.NewProjectilesArc<JewelProjectileRainbow>(amount, singleRadian, SourceAI, NPC.Center, originalVelocity, JewelProjectileRainbowDamage, 0f, action: p => p.ai[0] = JewelProjectileRainbow.TextureType_Circle);
            }

            CurrentBehavior = Behavior.None;
            Timer1 = 0;
        }

        void Attack_Triangle()
        {
            SoundEngine.PlaySound(JewelHandler.ShootSound, NPC.Center);
            for (int i = 0; i < 20; i++)
                JewelHandler.SpawnOrbParticle(NPC, Main.rand.NextFloat(3f, 6f), Main.rand.Next(30, 50), Main.rand.NextFloat(0.4f, 0.7f));
            JewelHandler.SpawnPointingParticle(NPC, 6, true);

            JewelHandler.CreateDustFromJewelTo(NPC, master.Center, -1, true);

            if (TOSharedData.NotClient)
            {
                int amount = 30;
                float totalAngle = MathHelper.TwoPi;
                float singleRadian = totalAngle / amount;
                Vector2 originalVelocity = (PolarVector2)NPC.GetVelocityTowards(Target, MaxProjectileSpeed);
                Vector2 rotatedOriginalVelocity = originalVelocity.RotatedBy(TOMathUtils.PiOver3 * 2);
                Vector2 rotatedOriginalVelocity2 = rotatedOriginalVelocity.RotatedBy(TOMathUtils.PiOver3 * 2);

                List<Vector2> originalVelocityList = [originalVelocity, rotatedOriginalVelocity, rotatedOriginalVelocity2, originalVelocity];

                for (int i = 0; i < amount; i++)
                {
                    Vector2 velocity = Vector2.LerpMany(originalVelocityList, (float)i / amount);
                    Projectile.NewProjectileAction<JewelProjectileRainbow>(SourceAI, NPC.Center, velocity, JewelProjectileRainbowDamage, 0f, action: p => p.ai[0] = JewelProjectileRainbow.TextureType_Triangle);
                }
            }

            CurrentBehavior = Behavior.None;
            Timer1 = 0;
        }

        void Attack_Star()
        {
            SoundEngine.PlaySound(JewelHandler.ShootSound, NPC.Center);
            for (int i = 0; i < 20; i++)
                JewelHandler.SpawnOrbParticle(NPC, Main.rand.NextFloat(3f, 6f), Main.rand.Next(30, 50), Main.rand.NextFloat(0.4f, 0.7f));
            JewelHandler.SpawnPointingParticle(NPC, 6, true);

            JewelHandler.CreateDustFromJewelTo(NPC, master.Center, -1, true);

            if (TOSharedData.NotClient)
            {
                int amount = 60;
                float totalAngle = MathHelper.TwoPi;
                float singleRadian = totalAngle / amount;
                Vector2 originalVelocity = (PolarVector2)NPC.GetVelocityTowards(Target, MaxProjectileSpeed);
                Vector2 rotatedOriginalVelocity = originalVelocity.RotatedBy(TOMathUtils.PiOver5 * 4);
                Vector2 rotatedOriginalVelocity2 = rotatedOriginalVelocity.RotatedBy(TOMathUtils.PiOver5 * 4);
                Vector2 rotatedOriginalVelocity3 = rotatedOriginalVelocity2.RotatedBy(TOMathUtils.PiOver5 * 4);
                Vector2 rotatedOriginalVelocity4 = rotatedOriginalVelocity3.RotatedBy(TOMathUtils.PiOver5 * 4);

                List<Vector2> originalVelocityList = [originalVelocity, rotatedOriginalVelocity, rotatedOriginalVelocity2, rotatedOriginalVelocity3, rotatedOriginalVelocity4, originalVelocity];

                for (int i = 0; i < amount; i++)
                {
                    Vector2 velocity = Vector2.LerpMany(originalVelocityList, (float)i / amount);
                    Projectile.NewProjectileAction<JewelProjectileRainbow>(SourceAI, NPC.Center, velocity, JewelProjectileRainbowDamage, 0f, action: p => p.ai[0] = JewelProjectileRainbow.TextureType_Star);
                }
            }

            CurrentBehavior = Behavior.None;
            Timer1 = 0;
        }

        void Attack_Square()
        {
            Timer1++;

            if (TOSharedData.NotClient)
            {
                if (Timer1 == 1)
                    AttackCore();
                else if (Timer1 == 11)
                {
                    AttackCore(MathHelper.PiOver4);
                    CurrentBehavior = Behavior.None;
                    Timer1 = 0;
                }
            }

            void AttackCore(float offset = 0f)
            {
                SoundEngine.PlaySound(JewelHandler.ShootSound, NPC.Center);
                for (int i = 0; i < 20; i++)
                    JewelHandler.SpawnOrbParticle(NPC, Main.rand.NextFloat(3f, 6f), Main.rand.Next(30, 50), Main.rand.NextFloat(0.4f, 0.7f));
                JewelHandler.SpawnPointingParticle(NPC, 6, true);

                JewelHandler.CreateDustFromJewelTo(NPC, master.Center, -1, true);

                int amount = 32;
                float totalAngle = MathHelper.TwoPi;
                float singleRadian = totalAngle / amount;
                Vector2 originalVelocity = (PolarVector2)NPC.GetVelocityTowards(Target, MaxProjectileSpeed).RotatedBy(offset);
                Vector2 rotatedOriginalVelocity = originalVelocity.RotatedBy(MathHelper.PiOver2);
                Vector2 rotatedOriginalVelocity2 = rotatedOriginalVelocity.RotatedBy(MathHelper.PiOver2);
                Vector2 rotatedOriginalVelocity3 = rotatedOriginalVelocity2.RotatedBy(MathHelper.PiOver2);

                List<Vector2> originalVelocityList = [originalVelocity, rotatedOriginalVelocity, rotatedOriginalVelocity2, rotatedOriginalVelocity3, originalVelocity];

                for (int i = 0; i < amount; i++)
                {
                    Vector2 velocity = Vector2.LerpMany(originalVelocityList, (float)i / amount);
                    Projectile.NewProjectileAction<JewelProjectileRainbow>(SourceAI, NPC.Center, velocity, JewelProjectileRainbowDamage, 0f, action: p => p.ai[0] = JewelProjectileRainbow.TextureType_Square);
                }
            }
        }

        void Attack_Circle()
        {
            int totalAttackNum = 6;

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
                    5 => 30,
                    _ => 0
                };
                int pointingParticleAmount = attackNum switch
                {
                    0 => 2,
                    1 => 2,
                    2 => 2,
                    3 => 3,
                    4 => 4,
                    5 => 8,
                    6 => 5,
                    7 => 10,
                    _ => 0
                };
                SoundEngine.PlaySound(JewelHandler.ShootSound, NPC.Center);
                for (int i = 0; i < orbParticleAmount; i++)
                    JewelHandler.SpawnOrbParticle(NPC, Main.rand.NextFloat(3f, 6f), Main.rand.Next(30, 50), Main.rand.NextFloat(0.4f, 0.7f));
                JewelHandler.SpawnPointingParticle(NPC, pointingParticleAmount, true);

                JewelHandler.CreateDustFromJewelTo(NPC, master.Center, -1, true);

                if (TOSharedData.NotClient)
                {
                    int amount = attackNum switch
                    {
                        0 => 3,
                        1 => 6,
                        2 => 9,
                        3 => 12,
                        4 => 16,
                        5 => 36,
                        6 => 16,
                        7 => 80,
                        _ => 0
                    };
                    float singleRadian = MathHelper.TwoPi / amount;
                    float radian = singleRadian * (amount - 1);
                    float initialRotation = (Target.Center - NPC.Center).ToRotation() + attackNum * TOMathUtils.PiOver5 + Main.rand.NextFloat(TOMathUtils.PiOver12);
                    switch (attackNum)
                    {
                        case <= 5:
                            Projectile.NewProjectilesArc<JewelProjectileRainbow>(amount, singleRadian, SourceAI, NPC.Center, new PolarVector2(MaxProjectileSpeed - attackNum / 2f, initialRotation), JewelProjectileRainbowDamage, 0f, action: p => p.ai[0] = JewelProjectileRainbow.TextureType_Circle);
                            break;
                        case 6 or 7:
                            Projectile.NewProjectilesArc<JewelProjectileRainbow>(amount, singleRadian, SourceAI, NPC.Center, new PolarVector2(MaxProjectileSpeed / 2.5f - attackNum / 2f, initialRotation), JewelProjectileRainbowDamage, 0f, action: p =>
                            {
                                p.ai[0] = JewelProjectileRainbow.TextureType_Circle;
                                p.timeLeft = 450;
                            });
                            break;
                    }
                }
            }

            Timer1++;

            if (Timer1 > 4 * (totalAttackNum - 1))
            {
                CurrentBehavior = Behavior.None;
                Timer1 = 0;
            }
        }
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        DrawRainbowTrail(spriteBatch, screenPos, NPC, NPC.oldPos);
        JewelHandler.DrawJewel(spriteBatch, screenPos, NPC);
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
        JewelHandler.HitEffect(NPC);
    }

    public override void OnKill()
    {
        JewelHandler.OnKill(NPC);
    }
}
