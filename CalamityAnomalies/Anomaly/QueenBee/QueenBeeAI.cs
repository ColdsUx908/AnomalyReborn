using CalamityMod;
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Buffs.StatDebuffs;
using CalamityMod.NPCs.PlagueEnemies;
using CalamityMod.Projectiles.Boss;

namespace CalamityAnomalies.Anomaly.QueenBee;

public sealed partial class QueenBee_Anomaly
{
    // Vanilla values
    public static int StingerDamage = 11; // 44; Also applies to GFB stinger replacements

    // NPC.ai 属性
    public int AI0
    {
        get => (int)NPC.ai[0];
        set => NPC.ai[0] = value;
    }
    public int AI1
    {
        get => (int)NPC.ai[1];
        set => NPC.ai[1] = value;
    }
    public int AI2
    {
        get => (int)NPC.ai[2];
        set => NPC.ai[2] = value;
    }
    public float AI3
    {
        get => NPC.ai[3];
        set => NPC.ai[3] = value;
    }

    // NPC.localAI 属性
    public int LocalAI0
    {
        get => (int)NPC.localAI[0];
        set => NPC.localAI[0] = value;
    }

    // calamityGlobalNPC.newAI 属性
    public int NewAI0
    {
        get => (int)CalamityNPC.newAI[0];
        set => CalamityNPC.newAI[0] = value;
    }
    public float NewAI1
    {
        get => CalamityNPC.newAI[1];
        set => CalamityNPC.newAI[1] = value;
    }
    public int NewAI3
    {
        get => (int)CalamityNPC.newAI[3];
        set => CalamityNPC.newAI[3] = value;
    }

    public bool CalamityAI(Mod mod)
    {
        // 获取目标
        if (NPC.target < 0 || NPC.target == Main.maxPlayers || Target.dead || !Target.active)
            CalamityUtils.CalamityTargeting(NPC, CalamityTargetingParameters.BossDefaults);

        int targetTileX = (int)Target.Center.X / 16;
        int targetTileY = (int)Target.Center.Y / 16;
        Tile tile = Framing.GetTileSafely(targetTileX, targetTileY);

        float maxEnrageScale = 2f;
        float enrageScale = 0.5f;

        if (Main.getGoodWorld)
            enrageScale += 0.5f;

        if (enrageScale > maxEnrageScale)
            enrageScale = maxEnrageScale;


        // 蜜蜂生成限制
        int beeLimit = 9;
        int totalBees = 0;
        bool beeLimitReached = false;
        for (int i = 0; i < Main.maxNPCs; i++)
        {
            NPC bee = Main.npc[i];
            bool isQueenBeeBee = bee.ai[3] == 1f;
            if (bee.active && (bee.type == NPCID.Bee || bee.type == NPCID.BeeSmall) && isQueenBeeBee)
            {
                totalBees++;
                if (totalBees >= beeLimit)
                {
                    beeLimitReached = true;
                    break;
                }
            }
        }

        // 黄蜂生成限制
        int hornetLimit = 2;
        bool hornetLimitReached = false;
        int totalHornets = 0;
        for (int i = 0; i < Main.maxNPCs; i++)
        {
            NPC hornet = Main.npc[i];
            bool isQueenBeeHornet = hornet.ai[3] == 1f;
            if (hornet.active && (hornet.type == NPCID.LittleHornetHoney || hornet.type == NPCID.HornetHoney || hornet.type == NPCID.BigHornetHoney) && isQueenBeeHornet)
            {
                int hornetCountIncrement = hornet.type == NPCID.BigHornetHoney ? 3 : hornet.type == NPCID.HornetHoney ? 2 : 1;
                totalHornets += hornetCountIncrement;
                if (totalHornets >= hornetLimit)
                {
                    hornetLimitReached = true;
                    break;
                }
            }
        }

        // 阶段判定

        // 距离过远或玩家死亡的处理
        float distanceFromTarget = Vector2.Distance(NPC.Center, Target.Center);
        if (AI0 != 7)
        {
            if (NPC.timeLeft < 60)
                NPC.timeLeft = 60;
            if (distanceFromTarget > 3000f)
                AI0 = 4;
        }
        if (Target.dead)
            AI0 = 7;

        // 减速免疫调整
        bool immuneToSlowingDebuffs = AI0 == 0;
        NPC.buffImmune[ModContent.BuffType<GlacialState>()] = immuneToSlowingDebuffs;
        NPC.buffImmune[ModContent.BuffType<TemporalSadness>()] = immuneToSlowingDebuffs;
        NPC.buffImmune[ModContent.BuffType<Eutrophication>()] = immuneToSlowingDebuffs;
        NPC.buffImmune[ModContent.BuffType<TimeDistortion>()] = immuneToSlowingDebuffs;
        NPC.buffImmune[ModContent.BuffType<GalvanicCorrosion>()] = immuneToSlowingDebuffs;
        NPC.buffImmune[ModContent.BuffType<Vaporfied>()] = immuneToSlowingDebuffs;
        NPC.buffImmune[BuffID.Webbed] = immuneToSlowingDebuffs;

        // 初始化状态：始终从生成蜜蜂阶段开始
        if (NewAI3 == 0)
        {
            NewAI3 = 1;
            AI0 = 2;
            NPC.netUpdate = true;
            NPC.SyncExtraAI();
        }

        // ---------- 根据当前状态调用对应处理函数 ----------
        if (AI0 == -1)
            HandleRandomPhase();
        else if (AI0 == 0)
            HandleChargePhase();
        else if (AI0 == 1)
            HandleBeeSpawnPhase();
        else if (AI0 == 2)
            HandleBeeSpawnPreparePhase();
        else if (AI0 == 3)
            HandleStingerPhase();
        else if (AI0 == 4)
            HandleDespawnPhase();
        else if (AI0 == 5)
            HandleStingerArcPhase();
        else if (AI0 == 7)
            HandleDeathDespawnPhase();

        if (Main.dedServ)
            NPC.ForceNetUpdate();

        return false;

        // ---------- 本地函数：各阶段逻辑 ----------
        // 1. 随机选择下一个阶段
        void HandleRandomPhase()
        {
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                int phase;
                int maxRandom = Phase2 ? 5 : 4;
                do phase = Main.rand.Next(maxRandom);
                while (phase == AI1 || phase == 1 || (phase == 2 && Phase2) || (Phase2_3 && phase == 3));

                bool charging = phase == 0;
                bool stingerArcs = phase == 4;

                if (stingerArcs)
                    phase = 5;

                CalamityUtils.CalamityTargeting(NPC, CalamityTargetingParameters.BossDefaults);
                AI0 = phase;
                AI1 = 0;

                // 刺针弧线方向（左右）
                AI2 = (phase == 5 && Phase2_2) ? (Main.rand.NextBool() ? 1 : -1) : phase == 5 ? 1 : 0;

                // 冲刺速度
                AI3 = charging ? ((Phase2_3 ? 27f : Phase2_2 ? 16f : Phase2 ? 27f : Phase1_2 ? 22f : 17f) + 3f * enrageScale) : 0f;

                // 冲刺距离
                NewAI1 = charging ? ((Phase2_3 ? 700f : Phase2_2 ? 300f : Phase2 ? 600f : Phase1_2 ? 500f : 400f) - 50f * enrageScale) : 0f;

                NPC.SyncExtraAI();
            }
        }

        // 2. 冲刺阶段
        void HandleChargePhase()
        {
            int chargeDistanceX = (int)NewAI1;

            int chargeAmt = Phase2_3 ? 1 : Phase2_2 ? 3 : Phase2 ? 2 : 1;
            int deathChargeLimit = Phase2_2 ? 3 : 2;
            if (chargeAmt > deathChargeLimit)
                chargeAmt = deathChargeLimit;

            if (AI1 > (2 * chargeAmt) && AI1 % 2 == 0)
            {
                AI0 = -1;
                AI1 = 0;
                AI2 = 0;
                NPC.netUpdate = true;
                return;
            }

            float velocity = AI3;

            if (AI1 % 2 == 0)
            {
                NPC.damage = 0;
                float chargeDistanceY = Phase2_3 ? 100f : Phase2 ? 50f : 20f;
                chargeDistanceY += 50f * enrageScale;
                chargeDistanceY += MathHelper.Lerp(0f, 100f, 1f - (NPC.LifeRatio / 2));
                chargeDistanceY *= 2f;

                float distanceFromTargetX = Math.Abs(NPC.Center.X - Target.Center.X);
                float distanceFromTargetY = Math.Abs(NPC.Center.Y - Target.Center.Y);
                if (distanceFromTargetY < chargeDistanceY && distanceFromTargetX >= chargeDistanceX)
                {
                    NPC.damage = NPC.defDamage;
                    LocalAI0 = 1;
                    AI1 += 1;
                    AI2 = 0;

                    Vector2 beeLocation = NPC.Center;
                    float targetXDist = Target.Center.X - beeLocation.X;
                    float targetYDist = Target.Center.Y - beeLocation.Y;
                    float targetDistance = (float)Math.Sqrt(targetXDist * targetXDist + targetYDist * targetYDist);
                    targetDistance = velocity / targetDistance;
                    NPC.velocity.X = targetXDist * targetDistance;
                    NPC.velocity.Y = targetYDist * targetDistance;

                    float playerLocation = NPC.Center.X - Target.Center.X;
                    NPC.direction = playerLocation < 0 ? 1 : -1;
                    NPC.spriteDirection = NPC.direction;
                    SoundEngine.PlaySound(SoundID.Zombie125, NPC.Center);
                    return;
                }

                LocalAI0 = 0;
                float chargeVelocityX = (Phase2 ? 24f : Phase1_2 ? 20f : 16f) + 8f * enrageScale;
                float chargeVelocityY = (Phase2 ? 18f : Phase1_2 ? 15f : 12f) + 6f * enrageScale;
                float chargeAccelerationX = (Phase2 ? 0.7f : Phase1_2 ? 0.6f : 0.5f) + 0.25f * enrageScale;
                float chargeAccelerationY = (Phase2 ? 0.35f : Phase1_2 ? 0.3f : 0.25f) + 0.125f * enrageScale;

                chargeVelocityX += 1f;
                chargeVelocityY += 2f;
                chargeAccelerationX += 0.1f;
                chargeAccelerationY += 0.2f;

                if (NPC.Center.Y < Target.Center.Y - chargeDistanceY)
                    NPC.velocity.Y += chargeAccelerationY;
                else if (NPC.Center.Y > Target.Center.Y + chargeDistanceY)
                    NPC.velocity.Y -= chargeAccelerationY;
                else
                    NPC.velocity.Y *= 0.7f;

                if (NPC.velocity.Y < -chargeVelocityY)
                    NPC.velocity.Y = -chargeVelocityY;
                if (NPC.velocity.Y > chargeVelocityY)
                    NPC.velocity.Y = chargeVelocityY;

                float distanceXMax = 100f;
                float distanceXMin = 20f;
                if (distanceFromTargetX > chargeDistanceX + distanceXMax)
                    NPC.velocity.X += chargeAccelerationX * NPC.direction;
                else if (distanceFromTargetX < chargeDistanceX + distanceXMin)
                    NPC.velocity.X -= chargeAccelerationX * NPC.direction;
                else
                    NPC.velocity.X *= 0.7f;

                if (NPC.velocity.X < -chargeVelocityX)
                    NPC.velocity.X = -chargeVelocityX;
                if (NPC.velocity.X > chargeVelocityX)
                    NPC.velocity.X = chargeVelocityX;

                float playerLocation2 = NPC.Center.X - Target.Center.X;
                NPC.direction = playerLocation2 < 0 ? 1 : -1;
                NPC.spriteDirection = NPC.direction;
                NPC.ForceNetUpdate(false);
            }
            else
            {
                NPC.damage = NPC.defDamage;
                NPC.direction = NPC.velocity.X < 0f ? -1 : 1;
                NPC.spriteDirection = NPC.direction;

                int chargeDirection = NPC.Center.X < Target.Center.X ? -1 : 1;
                bool shouldCharge = false;
                if (NPC.direction == chargeDirection && Math.Abs(NPC.Center.X - Target.Center.X) > chargeDistanceX)
                {
                    AI2 = 1;
                    shouldCharge = true;
                }
                if (Math.Abs(NPC.Center.Y - Target.Center.Y) > chargeDistanceX * 1.5f)
                {
                    AI2 = 1;
                    shouldCharge = true;
                }
                if (enrageScale > 0f && shouldCharge)
                    NPC.velocity *= MathHelper.Lerp(0.5f, 0.9f, 1f - enrageScale / maxEnrageScale);

                if (AI2 != 1)
                {
                    if (NPC.velocity.Length() < velocity)
                        NPC.velocity.X = velocity * NPC.direction;

                    float accelerateGateValue = Phase2_3 ? 30f : Phase2_2 ? 10f : 90f;
                    if (enrageScale > 0f)
                        accelerateGateValue *= 0.75f;

                    NewAI0 += 1;
                    if (NewAI0 > accelerateGateValue)
                    {
                        NPC.SyncExtraAI();
                        float velocityXLimit = velocity * 2f;
                        if (Math.Abs(NPC.velocity.X) < velocityXLimit)
                            NPC.velocity.X *= 1.02f;
                    }

                    float beeSpawnGateValue = 20f;
                    bool spawnBee = Phase2 && NewAI0 % beeSpawnGateValue == 0f && Collision.CanHit(NPC.Center, 1, 1, Target.position, Target.width, Target.height);
                    if (spawnBee)
                    {
                        SoundEngine.PlaySound(SoundID.NPCHit18, NPC.Center);
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            int spawnType = Main.rand.Next(NPCID.Bee, NPCID.BeeSmall + 1);
                            if (Main.zenithWorld)
                            {
                                if (Phase1_3)
                                    spawnType = Main.rand.NextBool(3) ? ModContent.NPCType<PlagueChargerLarge>() : ModContent.NPCType<PlagueCharger>();
                                else
                                    spawnType = NPCID.Hellbat;
                            }
                            else
                            {
                                int random = hornetLimitReached ? 0 : beeLimitReached ? Main.rand.Next(6, 12) : Main.rand.Next(12);
                                switch (random)
                                {
                                    case 6:
                                    case 7:
                                    case 8:
                                        spawnType = NPCID.LittleHornetHoney;
                                        break;
                                    case 9:
                                    case 10:
                                        spawnType = NPCID.HornetHoney;
                                        break;
                                    case 11:
                                        spawnType = NPCID.BigHornetHoney;
                                        break;
                                }
                            }

                            if (!beeLimitReached || !hornetLimitReached)
                            {
                                int spawn = NPC.NewNPC(NPC.GetSource_FromAI(), (int)NPC.Center.X, (int)NPC.Center.Y, spawnType);
                                Vector2 beeVelocity = (Target.Center - NPC.Center).SafeNormalize(Vector2.UnitY);
                                Main.npc[spawn].velocity = beeVelocity;
                                Main.npc[spawn].velocity *= 5f;
                                if (!Main.zenithWorld)
                                {
                                    Main.npc[spawn].ai[2] = enrageScale;
                                    Main.npc[spawn].ai[3] = 1f;
                                }
                                Main.npc[spawn].timeLeft = 600;
                                Main.npc[spawn].netUpdate = true;
                            }
                        }
                    }

                    LocalAI0 = 1;
                    return;
                }

                NPC.damage = 0;
                float playerLocation = NPC.Center.X - Target.Center.X;
                NPC.direction = playerLocation < 0 ? 1 : -1;
                NPC.spriteDirection = NPC.direction;
                LocalAI0 = 0;
                NPC.velocity *= 0.8f;

                float chargeDeceleration = 0.2f;
                if (Phase1_2)
                {
                    NPC.velocity *= 0.9f;
                    chargeDeceleration += 0.05f;
                }
                if (Phase2)
                {
                    NPC.velocity *= 0.8f;
                    chargeDeceleration += 0.1f;
                }
                if (enrageScale > 0f)
                    NPC.velocity *= MathHelper.Lerp(0.7f, 1f, 1f - enrageScale / maxEnrageScale);

                if (Math.Abs(NPC.velocity.X) + Math.Abs(NPC.velocity.Y) < chargeDeceleration)
                {
                    AI2 = 0;
                    AI1 += 1;
                    NewAI0 = 0;
                    NPC.SyncExtraAI();
                }

                NPC.ForceNetUpdate(false);
            }
        }

        // 3. 飞到目标上方（准备生成蜜蜂）
        void HandleBeeSpawnPreparePhase()
        {
            NPC.damage = 0;
            float playerLocation = NPC.Center.X - Target.Center.X;
            NPC.direction = playerLocation < 0 ? 1 : -1;
            NPC.spriteDirection = NPC.direction;

            float beeAttackAccel = 0.48f;
            float beeAttackSpeed = 12f + enrageScale * 3f;
            beeAttackSpeed *= 1.35f;

            bool canHitTarget = Collision.CanHit(NPC.Center, 1, 1, Target.position, Target.width, Target.height);
            float distanceAboveTarget = !canHitTarget ? 0f : 320f;
            Vector2 hoverDestination = Target.Center - Vector2.UnitY * distanceAboveTarget;
            Vector2 idealVelocity = NPC.SafeDirectionTo(hoverDestination) * beeAttackSpeed;

            NewAI0 += 1;
            if ((Vector2.Distance(NPC.Center, hoverDestination) < 400f && canHitTarget) || NewAI0 >= 90f)
            {
                AI0 = 1;
                AI1 = 0;
                NewAI0 = 0;
                NPC.netUpdate = true;
                NPC.SyncExtraAI();
                return;
            }

            NPC.SimpleFlyMovement(idealVelocity, beeAttackAccel);
        }

        // 4. 蜜蜂生成阶段
        void HandleBeeSpawnPhase()
        {
            NPC.damage = 0;
            LocalAI0 = 0;

            float beeAttackHoverSpeed = 16f + enrageScale * 4f;
            float beeAttackHoverAccel = 0.6f;
            beeAttackHoverSpeed *= 1.35f;

            Vector2 beeSpawnLocation = new Vector2(NPC.Center.X + (Main.rand.Next(20) * NPC.direction), NPC.position.Y + NPC.height * 0.8f);
            Vector2 beeSpawnCollisionLocation = new Vector2(beeSpawnLocation.X, beeSpawnLocation.Y - 30f);
            bool canHitTarget = Collision.CanHit(beeSpawnCollisionLocation, 1, 1, Target.position, Target.width, Target.height);
            Vector2 hoverDestination = Target.Center - Vector2.UnitY * (!canHitTarget ? 0f : 320f);
            Vector2 idealVelocity = NPC.SafeDirectionTo(hoverDestination) * beeAttackHoverSpeed;

            AI1 += 1;
            int beeSpawnTimer = 0;
            for (int i = 0; i < Main.maxPlayers; i++)
            {
                if (Main.player[i].active && !Main.player[i].dead && (NPC.Center - Main.player[i].Center).Length() < 1000f)
                    beeSpawnTimer++;
            }
            AI1 += beeSpawnTimer / 2;
            if (Phase1_2)
                AI1 += 1;

            bool spawnBee = false;
            float beeSpawnCheck = 9 * enrageScale;
            if (AI1 > beeSpawnCheck)
            {
                AI1 = 0;
                AI2 += 1;
                spawnBee = true;
            }

            if (Collision.CanHit(beeSpawnLocation, 1, 1, Target.position, Target.width, Target.height) && spawnBee && (!beeLimitReached || !hornetLimitReached))
            {
                SoundEngine.PlaySound(SoundID.NPCHit18, beeSpawnLocation);
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    int spawnType = Main.rand.Next(NPCID.Bee, NPCID.BeeSmall + 1);
                    if (Main.zenithWorld)
                    {
                        if (Phase1_3)
                            spawnType = Main.rand.NextBool(3) ? ModContent.NPCType<PlagueChargerLarge>() : ModContent.NPCType<PlagueCharger>();
                        else
                            spawnType = NPCID.Hellbat;
                    }
                    else
                    {
                        int random = hornetLimitReached ? 0 : beeLimitReached ? Main.rand.Next(6, 12) : Main.rand.Next(12);
                        switch (random)
                        {
                            case 6:
                            case 7:
                            case 8:
                                spawnType = NPCID.LittleHornetHoney;
                                break;
                            case 9:
                            case 10:
                                spawnType = NPCID.HornetHoney;
                                break;
                            case 11:
                                spawnType = NPCID.BigHornetHoney;
                                break;
                        }
                    }

                    int spawn = NPC.NewNPC(NPC.GetSource_FromAI(), (int)beeSpawnLocation.X, (int)beeSpawnLocation.Y, spawnType);
                    Vector2 beeVelocity = (Target.Center - NPC.Center).SafeNormalize(Vector2.UnitY);
                    Main.npc[spawn].velocity = beeVelocity;
                    Main.npc[spawn].velocity *= 5f;
                    if (!Main.zenithWorld)
                    {
                        Main.npc[spawn].ai[2] = enrageScale;
                        Main.npc[spawn].ai[3] = 1f;
                    }
                    Main.npc[spawn].timeLeft = 600;
                    Main.npc[spawn].netUpdate = true;
                }
            }

            if (Vector2.Distance(beeSpawnLocation, hoverDestination) > 400f || !canHitTarget)
                NPC.SimpleFlyMovement(idealVelocity, beeAttackHoverAccel);
            else
                NPC.velocity *= 0.8f;

            float playerLocation = NPC.Center.X - Target.Center.X;
            NPC.direction = playerLocation < 0 ? 1 : -1;
            NPC.spriteDirection = NPC.direction;

            float numSpawns = 3f;
            if (AI2 > numSpawns || (beeLimitReached && hornetLimitReached))
            {
                AI0 = -1;
                AI1 = 2;
                AI2 = 0;
                NPC.netUpdate = true;
            }
        }

        // 5. 普通刺针射击阶段
        void HandleStingerPhase()
        {
            NPC.damage = 0;
            float playerLocation = NPC.Center.X - Target.Center.X;
            NPC.direction = playerLocation < 0 ? 1 : -1;
            NPC.spriteDirection = NPC.direction;

            float stingerAttackSpeed = 16f + enrageScale * 4f;
            float stingerAttackAccel = Phase2_3 ? 0.16f : 0.12f;
            if (enrageScale > 0f)
                stingerAttackAccel = MathHelper.Lerp(Phase2_3 ? 0.3f : 0.24f, Phase2_3 ? 0.6f : 0.48f, enrageScale / maxEnrageScale);

            stingerAttackSpeed *= 1.08f;
            stingerAttackAccel *= 1.18f;

            Vector2 stingerSpawnLocation = new Vector2(NPC.Center.X + (Main.rand.Next(20) * NPC.direction), NPC.position.Y + NPC.height * 0.8f);
            bool canHitTarget = Collision.CanHit(new Vector2(stingerSpawnLocation.X, stingerSpawnLocation.Y - 30f), 1, 1, Target.position, Target.width, Target.height);
            Vector2 hoverDestination = Target.Center - Vector2.UnitY * (!canHitTarget ? 0f : Phase2 ? 400f : Phase1_2 ? 360f : 320f);
            Vector2 idealVelocity = NPC.SafeDirectionTo(hoverDestination) * stingerAttackSpeed;

            AI1 += 1;
            int stingerAttackTimer = Phase2_3 ? 40 : Phase1_2 ? 30 : 20;
            stingerAttackTimer -= (int)Math.Ceiling((Phase2_3 ? 16f : Phase1_2 ? 12f : 8f) * enrageScale);
            if (stingerAttackTimer < 5)
                stingerAttackTimer = 5;

            if (AI1 % stingerAttackTimer == (stingerAttackTimer - 1) && NPC.Bottom.Y < Target.Top.Y && Collision.CanHit(stingerSpawnLocation, 1, 1, Target.position, Target.width, Target.height))
            {
                SoundEngine.PlaySound(SoundID.Item17, stingerSpawnLocation);
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    float stingerSpeed = (Phase1_3 ? 6f : 5f) + enrageScale;
                    stingerSpeed += 1f;

                    float stingerTargetX = Target.Center.X - stingerSpawnLocation.X;
                    float stingerTargetY = Target.Center.Y - stingerSpawnLocation.Y;
                    float stingerTargetDist = (float)Math.Sqrt(stingerTargetX * stingerTargetX + stingerTargetY * stingerTargetY);
                    stingerTargetDist = stingerSpeed / stingerTargetDist;
                    stingerTargetX *= stingerTargetDist;
                    stingerTargetY *= stingerTargetDist;
                    Vector2 stingerVelocity = new Vector2(stingerTargetX, stingerTargetY);
                    int type = Main.zenithWorld ? (Phase1_3 ? ModContent.ProjectileType<PlagueStingerGoliathV2>() : ProjectileID.FlamingWood) : ProjectileID.QueenBeeStinger;

                    int projectile = Projectile.NewProjectile(NPC.GetSource_FromAI(), stingerSpawnLocation, stingerVelocity, type, StingerDamage, 0f, Main.myPlayer, 0f, (Main.zenithWorld && Phase1_3) ? Target.position.Y : 0f);
                    Main.projectile[projectile].timeLeft = 1200;
                    Main.projectile[projectile].extraUpdates = 1;

                    if (Phase1_2)
                    {
                        int numExtraStingers = Phase2_3 ? 4 : 2;
                        for (int i = 0; i < numExtraStingers; i++)
                        {
                            projectile = Projectile.NewProjectile(NPC.GetSource_FromAI(), stingerSpawnLocation + Main.rand.NextVector2CircularEdge(16f, 16f) * (i + 1), stingerVelocity * MathHelper.Lerp(0.75f, 1f, i / (float)numExtraStingers), type, StingerDamage, 0f, Main.myPlayer, 0f, (Main.zenithWorld && Phase1_3) ? Target.position.Y : 0f);
                            Main.projectile[projectile].timeLeft = 1200;
                            Main.projectile[projectile].extraUpdates = 1;
                        }
                    }
                }
            }

            if (Vector2.Distance(stingerSpawnLocation, hoverDestination) > 40f || !canHitTarget)
                NPC.SimpleFlyMovement(idealVelocity, stingerAttackAccel);

            float numStingerShots = Phase2_3 ? 5f : Phase1_2 ? 8f : 15f;
            numStingerShots = (float)Math.Round(numStingerShots * 0.5f);

            if (AI1 > stingerAttackTimer * numStingerShots)
            {
                AI0 = -1;
                AI1 = 3;
                NPC.netUpdate = true;
            }
        }

        // 6. 远离阶段（距离过远时试图返回）
        void HandleDespawnPhase()
        {
            NPC.damage = 0;
            LocalAI0 = 1;
            float despawnVelMult = 14f;

            Vector2 despawnTargetDist = (Target.Center - NPC.Center).SafeNormalize(Vector2.UnitY);
            despawnTargetDist *= 14f;

            NPC.velocity = (NPC.velocity * despawnVelMult + despawnTargetDist) / (despawnVelMult + 1f);
            NPC.direction = NPC.velocity.X < 0f ? -1 : 1;
            NPC.spriteDirection = NPC.direction;

            if (distanceFromTarget < 2000f)
            {
                AI0 = -1;
                LocalAI0 = 0;
            }
        }

        // 7. 刺针弧线阶段
        void HandleStingerArcPhase()
        {
            NPC.damage = 0;
            float playerLocation = NPC.Center.X - Target.Center.X;
            NPC.direction = playerLocation < 0 ? 1 : -1;
            NPC.spriteDirection = NPC.direction;

            float stingerAttackSpeed = 20f + enrageScale * 4f;
            float stingerAttackAccel = Phase2_3 ? 0.7f : 0.5f;
            if (enrageScale > 0f)
                stingerAttackAccel = MathHelper.Lerp(Phase2_3 ? 0.9f : 0.7f, Phase2_3 ? 2.4f : 1.8f, enrageScale / maxEnrageScale);

            stingerAttackSpeed *= 1.1f;
            stingerAttackAccel *= 1.2f;

            int numStingerArcs = Phase2_3 ? 3 : Phase2_2 ? 2 : 1;
            numStingerArcs++;

            float phaseLimit = Phase2_3 ? 180f : Phase2_2 ? 150f : 120f;
            phaseLimit *= 1.5f;

            float stingerAttackTimer = (float)Math.Ceiling(phaseLimit / (numStingerArcs + 1));

            float maxDistance = 480f;
            float xLocationScale = MathHelper.Lerp(-maxDistance, maxDistance, AI1 / phaseLimit) * AI2;
            Vector2 stingerSpawnLocation = new Vector2(NPC.Center.X + (Main.rand.Next(20) * NPC.direction), NPC.position.Y + NPC.height * 0.8f);
            bool canHitTarget = Collision.CanHit(new Vector2(stingerSpawnLocation.X, stingerSpawnLocation.Y - 30f), 1, 1, Target.position, Target.width, Target.height);
            Vector2 hoverDestination = Target.Center + Vector2.UnitX * xLocationScale * 1.35f - Vector2.UnitY * maxDistance;
            Vector2 idealVelocity = NPC.SafeDirectionTo(hoverDestination) * stingerAttackSpeed;

            bool canFireStingers = stingerSpawnLocation.Y < Target.Top.Y - maxDistance * 0.8f || !canHitTarget;
            if (canFireStingers && AI1 < phaseLimit)
            {
                AI1 += 1;
                if (AI1 % stingerAttackTimer == 0f && AI1 != 0f && AI1 != phaseLimit)
                {
                    SoundEngine.PlaySound(SoundID.Item17, stingerSpawnLocation);
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        float stingerSpeed = (Phase2_3 ? 5f : 4f) + enrageScale;
                        stingerSpeed += 1f;

                        Vector2 projectileVelocity = (Target.Center - NPC.Center).SafeNormalize(Vector2.UnitY) * stingerSpeed;
                        int type = Main.zenithWorld ? ModContent.ProjectileType<PlagueStingerGoliathV2>() : ProjectileID.QueenBeeStinger;
                        int numProj = Phase2_3 ? 7 : Phase2_2 ? 11 : 15;
                        int spread = Phase2_3 ? 30 : Phase2_2 ? 50 : 60;
                        numProj += Phase2_3 ? 2 : Phase2_2 ? 4 : 6;
                        spread += Phase2_3 ? 10 : Phase2_2 ? 15 : 20;

                        float rotation = MathHelper.ToRadians(spread);
                        for (int i = 0; i < numProj; i++)
                        {
                            Vector2 perturbedSpeed = projectileVelocity.RotatedBy(MathHelper.Lerp(-rotation, rotation, i / (float)(numProj - 1)));
                            if (i % 2f != 0f)
                                perturbedSpeed *= 0.8f;

                            int projectile = Projectile.NewProjectile(NPC.GetSource_FromAI(), stingerSpawnLocation + perturbedSpeed.SafeNormalize(Vector2.UnitY) * 10f, perturbedSpeed, type, StingerDamage, 0f, Main.myPlayer, 0f, Target.position.Y);
                            Main.projectile[projectile].timeLeft = 1200;
                            Main.projectile[projectile].extraUpdates = 1;

                            if (!Main.zenithWorld)
                                Main.projectile[projectile].tileCollide = false;
                        }
                    }
                }
            }

            if (AI1 >= phaseLimit)
            {
                AI1 += 1;

                if (NPC.Distance(Target.Center) > 400f || !canHitTarget)
                {
                    idealVelocity = NPC.SafeDirectionTo(Target.Center) * stingerAttackSpeed;
                    NPC.SimpleFlyMovement(idealVelocity * 0.5f, stingerAttackAccel * 0.5f);
                }
                else
                    NPC.velocity *= 0.8f;

                float idleTime = 140f;
                if (AI1 >= phaseLimit + idleTime)
                {
                    AI0 = -1;
                    AI1 = 4;
                    AI2 = 0;
                    NPC.netUpdate = true;
                }
            }
            else
                NPC.SimpleFlyMovement(idealVelocity, stingerAttackAccel);
        }

        // 8. 死亡消失阶段（玩家死亡时）
        void HandleDeathDespawnPhase()
        {
            NPC.damage = 0;
            NPC.velocity.Y *= 0.98f;

            NPC.direction = NPC.velocity.X < 0f ? -1 : 1;
            NPC.spriteDirection = NPC.direction;

            if (NPC.position.X < (Main.maxTilesX * 8))
            {
                if (NPC.velocity.X > 0f)
                    NPC.velocity.X *= 0.98f;
                else
                    LocalAI0 = 1;

                NPC.velocity.X -= 0.08f;
            }
            else
            {
                if (NPC.velocity.X < 0f)
                    NPC.velocity.X *= 0.98f;
                else
                    LocalAI0 = 1;

                NPC.velocity.X += 0.08f;
            }

            if (NPC.timeLeft > 10)
                NPC.timeLeft = 10;
        }
    }
}