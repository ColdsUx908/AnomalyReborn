using CalamityAnomalies.DataStructures;

namespace CalamityAnomalies.Anomaly.EyeofCthulhu;

public partial class BloodlettingServant : CAModNPC
{
    public enum ServantPlace
    {
        Left,
        Right
    }

    public override string Texture => TOAssetUtils.FormatVanillaNPCTexturePath(NPCID.WanderingEye);
    public override string LocalizationCategory => "Anomaly.EyeofCthulhu";

    public ServantPlace Place;
    public float PositionRotation
    {
        get;
        set => field = TOMathUtils.NormalizeWithPeriod(value);
    }

    public bool ShouldUsePhase2Frame;
    public BehaviorCommand_Servant MasterCommandReceiver;
    public float FollowDistance;
    public float ArenaRadius;

    public const int TimeToGetPosition = 50;
    public const float ProjectileOffset = 10f;

    public static float MaxFollowDistance => 64f;
    public static float MaxFollowDistanceIncreased => 360f;

    public override void SetStaticDefaults()
    {
        Main.npcFrameCount[Type] = 4;
        NPCID.Sets.TrailingMode[Type] = 3;
        NPCID.Sets.TrailCacheLength[Type] = 5;
        NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, new NPCID.Sets.NPCBestiaryDrawModifiers() { Hide = true });
    }

    public override void SetDefaults()
    {
        NPC.aiStyle = -1;
        AIType = -1;
        NPC.damage = 20;
        NPC.width = 30;
        NPC.height = 32;

        NPC.defense = 5;

        NPC.lifeMax = 96;
        NPC.ApplyCalamityBossHealthBoost();

        NPC.knockBackResist = 0f;
        NPC.noGravity = true;
        NPC.noTileCollide = true;
        NPC.HitSound = SoundID.NPCHit1;
        NPC.DeathSound = SoundID.NPCDeath1;
        CalamityNPC.VulnerableToCold = true;
        CalamityNPC.VulnerableToHeat = true;
        CalamityNPC.VulnerableToSickness = true;
    }

    public override void AI()
    {
        if (Main.zenithWorld)
        {
            AI_Zenith();
            return;
        }

        if (!NPC.TryGetMaster(NPCID.EyeofCthulhu, out NPC master))
        {
            NPC.life = 0;
            NPC.HitEffect();
            NPC.active = false;
            NPC.netUpdate = true;
            return;
        }

        NPC.target = master.target; //同步目标

        EyeofCthulhu_Anomaly masterBehavior = new() { _entity = master };

        NPC.dontTakeDamage = true;

        NPC.velocity = Vector2.Zero;

        Timer1++;
        if (ShouldUsePhase2Frame)
        {
            Timer2 = Math.Clamp(Timer2 + 1, 0, 10);
            Timer3++;
        }
        else
        {
            Timer2 = Math.Clamp(Timer2 - 1, 0, 10);
            Timer3 = Math.Max(Timer3 - 4, 0);
        }

        //更新位置和旋转

        if (masterBehavior.CurrentPhase is >= EyeofCthulhu_Anomaly.Phase.PhaseChange_1To2 and <= EyeofCthulhu_Anomaly.Phase.Phase2_3)
            MiscAI_Phase2();

        Lighting.AddLight(NPC.Center, 0.8f, 0f, 0f);

        //执行命令
        switch (MasterCommandReceiver)
        {
            case BehaviorCommand_Servant.ShootBlood:
                ShootBlood();
                break;
            case BehaviorCommand_Servant.IncreaseFollowDistance or BehaviorCommand_Servant.ReduceFollowDistance:
                ChangeFollowDistance();
                break;
            case BehaviorCommand_Servant.GetToArenaPosition:
                GetToArenaPosition();
                break;
        }

        if (ShouldUsePhase2Frame)
            NPC.SpawnAfterimage(5, NPC.GetAlpha(Color.White));

        void MiscAI_Phase2()
        {
            if (FollowDistance < MaxFollowDistance)
                FollowDistance = Utils.Remap(Timer1, 0f, TimeToGetPosition, 0f, MaxFollowDistance);
            if (FollowDistance >= MaxFollowDistance)
            {
                float newPositionRotation = PositionRotation;
                float targetPositionRotation = master.rotation;
                float acceleration = ShouldUsePhase2Frame ? 0.5f : 0.15f;
                EyeofCthulhu_Handler.UpdateRotation(ref newPositionRotation, targetPositionRotation, acceleration);
                PositionRotation = newPositionRotation;
            }
            Vector2 offset = new Vector2(FollowDistance * (Place == ServantPlace.Left ? -1 : 1), 0f).RotatedBy(PositionRotation);
            NPC.Center = master.Center + offset;

            NPC.damage = ShouldUsePhase2Frame ? NPC.defDamage : 0;

            if (ShouldUsePhase2Frame)
                NPC.rotation = master.rotation - MathHelper.PiOver2;
            else
            {
                float targetRotation = TOMathUtils.NormalizeWithPeriod((Target.Center - NPC.Center).ToRotation(MathHelper.Pi));
                EyeofCthulhu_Handler.UpdateRotation(ref NPC.rotation, targetRotation, 0.12f);
            }
        }

        void ShootBlood()
        {
            float projectileSpeed = Ultra ? 17.5f : 15f;
            int amount = Ultra ? 3 : 1;
            EyeofCthulhu_Handler.ShootProjectile(NPC, ProjectileID.BloodShot, EyeofCthulhu_Anomaly.BloodDamage, projectileSpeed, amount, MathHelper.ToRadians(15f), p => p.timeLeft = 300);

            MasterCommandReceiver = BehaviorCommand_Servant.None;
        }

        void ChangeFollowDistance()
        {
            bool increase = MasterCommandReceiver == BehaviorCommand_Servant.IncreaseFollowDistance;

            Timer4 = Math.Clamp(Timer4 + increase.ToDirectionInt(), 0, 10);
            FollowDistance = MaxFollowDistance + (MaxFollowDistanceIncreased - MaxFollowDistance) * TOMathUtils.Interpolation.QuadraticEaseInOut(Timer4 / 10f);
            Vector2 offset = new Vector2(FollowDistance * (Place == ServantPlace.Left ? -1 : 1), 0f).RotatedBy(PositionRotation);
            NPC.Center = master.Center + offset;

            if (Timer4 == (increase ? 10 : 0))
                MasterCommandReceiver = BehaviorCommand_Servant.None;
        }

        void GetToArenaPosition()
        {
            NPC.damage = 0;
            ShouldUsePhase2Frame = false;

            if (masterBehavior.Phase3)
            {
                MasterCommandReceiver = BehaviorCommand_Servant.None;
                return;
            }

            float timer = masterBehavior.Timer1;
            ArenaRadius = MathHelper.Lerp(MaxFollowDistance, EyeofCthulhu_Handler.MaxArenaRadius1, TOMathUtils.Interpolation.ExponentialEaseInOut(timer / EyeofCthulhu_Anomaly.PhaseChangeGateValue_2To3_1, 4f));

            float newPositionRotation = PositionRotation;
            float targetPositionRotation = Place == ServantPlace.Left ? MathHelper.Pi : 0f;
            EyeofCthulhu_Handler.UpdateRotation(ref newPositionRotation, targetPositionRotation, 0.2f * TOMathUtils.Interpolation.CubicEaseInOut(masterBehavior.Timer1 / 10f));
            PositionRotation = newPositionRotation;

            Vector2 offset = new Vector2(ArenaRadius, 0f).RotatedBy(PositionRotation);
            Vector2 destination = masterBehavior.Phase3ArenaCenter + offset;
            NPC.Center = Vector2.SmootherStep(NPC.Center, destination, Math.Clamp(timer / EyeofCthulhu_Anomaly.PhaseChangeGateValue_2To3_1, 0f, 1f));

            float targetRotation = Place == ServantPlace.Left ^ timer > EyeofCthulhu_Anomaly.PhaseChangeGateValue_2To3_1 ? 0f : MathHelper.Pi;
            EyeofCthulhu_Handler.UpdateRotation(ref NPC.rotation, targetRotation, 0.3f * TOMathUtils.Interpolation.CubicEaseInOut(masterBehavior.Timer1 / 10f));
        }
    }

    public override void FindFrame(int frameHeight)
    {
        if (Main.zenithWorld)
        {
            FindFrame_Zenith(frameHeight);
            return;
        }

        int frameNum;

        NPC.frameCounter += 1.0;

        switch (NPC.frameCounter)
        {
            case < 8.0:
                frameNum = 0;
                break;
            case < 16.0:
                frameNum = 1;
                break;
            default:
                NPC.frameCounter = 0.0;
                frameNum = 0;
                break;
        }

        if (ShouldUsePhase2Frame)
            frameNum += 2;

        NPC.frame.Y = frameNum * frameHeight;
    }

    public override Color? GetAlpha(Color drawColor) => Color.Lerp(Color.Red * 0.75f, EyeofCthulhu_Handler.ChargeColor, Math.Clamp(Timer2 / 10f, 0f, 1f)) with { A = NPC.GraphicAlpha };

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        if (Main.zenithWorld)
            return PreDraw_Zenith(spriteBatch, screenPos, drawColor);

        Texture2D npcTexture = NPC.Texture;
        Color color = NPC.GetAlpha(drawColor);

        spriteBatch.DrawFromCenter(npcTexture, NPC.Center - screenPos, NPC.frame, color, NPC.rotation, NPC.scale);
        return false;
    }

    public override void HitEffect(NPC.HitInfo hit)
    {
        if (NPC.life > 0)
        {
            for (int i = 0; i < hit.Damage / (double)NPC.lifeMax * 100; i++)
                Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Blood, hit.HitDirection, -1f);

            if (NPC.life < NPC.lifeMax * 0.5f && NPC.localAI[0] == 0f)
            {
                NPC.localAI[0] = 1f;
                for (int i = 0; i < 50; i++)
                    SpawnDust();
            }
        }
        else
        {
            for (int i = 0; i < 100; i++)
                SpawnDust();
        }

        void SpawnDust() => Dust.NewDustAction(NPC.Center, NPC.width, NPC.height, DustID.Blood, new Vector2(2f * hit.HitDirection, -2f), d =>
        {
            d.scale = Main.rand.NextFloat(1.6f, 2.4f);
            d.velocity *= Main.rand.NextFloat(1.6f, 2.4f);
            d.color = new Color(128, 0, 0, 255 - NPC.alpha);
        });
    }

    #region Zenith
    public const float ChargeGateValue_Zenith = 180f;
    public const float ChargeTelegraphGateValue_Zenith = 90f;
    public const float ChargeDuration_Zenith = 50f;

    private static readonly ProjectileDamageContainer _bloodDamage = new(30, 60, 75, 90, 84, 108);
    public static int BloodDamage_Zenith => _bloodDamage.Value;

    public void AI_Zenith()
    {
        Lighting.AddLight(NPC.Center, 0.8f, 0f, 0f);

        NPC.TargetClosest();
        Player target = Main.player[NPC.target];
        bool targetDead = target.dead;

        bool phase2 = NPC.life < NPC.lifeMax * 0.5;
        float enrageScale = 3f;

        float enrageScaleMaxSpeedBonus = 2f * enrageScale;
        float maxSpeedX = (phase2 ? 10f : 8f) + enrageScaleMaxSpeedBonus;
        float maxSpeedY = (phase2 ? 8f : 4f) + enrageScaleMaxSpeedBonus;
        float xAccel = maxSpeedX * 0.01f;
        float xAccelBoost1 = maxSpeedX * 0.01f;
        float xAccelBoost2 = maxSpeedX * 0.0075f;
        float yAccel = maxSpeedY * 0.01f;
        float yAccelBoost1 = maxSpeedY * 0.01f;
        float yAccelBoost2 = maxSpeedY * 0.0075f;

        float chargeVelocity = 12f + enrageScaleMaxSpeedBonus * 2f;
        bool attemptingToCharge = NPC.ai[0] >= ChargeGateValue_Zenith;
        bool farEnoughForCharge = NPC.Distance(target.Center) >= 320f;
        bool closeEnoughForCharge = NPC.Distance(target.Center) < 480f;
        bool charging = (attemptingToCharge && farEnoughForCharge && closeEnoughForCharge) || NPC.ai[1] > 0f;

        float projectileShootGateValue = 90f;

        Vector2 lookAt = target.Center - NPC.Center;
        float rateOfRotation = charging ? 0f : 0.1f;
        CalamityNPC.canBreakPlayerDefense = charging;
        if (charging)
        {
            if (NPC.ai[1] == 0f)
            {
                SoundEngine.PlaySound(SoundID.NPCDeath9, NPC.Center);
                NPC.velocity = lookAt.SafeNormalize(Vector2.UnitY) * chargeVelocity;
                NPC.spriteDirection = lookAt.X < 0f ? -1 : 1;
                NPC.rotation = NPC.velocity.ToRotation() + (lookAt.X < 0f ? MathHelper.Pi : 0f);

                NPC.ai[1] = 1f;
                if (NPC.ai[3] > projectileShootGateValue * 0.5f)
                    NPC.ai[3] = projectileShootGateValue * 0.5f;

                NPC.netUpdate = true;
                NPC.netSpam = 0;
            }

            if (NPC.ai[1] < ChargeDuration_Zenith + 1f)
            {
                NPC.ai[1] += 1f;
                if (NPC.ai[1] > ChargeDuration_Zenith + 1f - 10f)
                    NPC.velocity *= 0.8f;

                if (NPC.ai[1] >= ChargeDuration_Zenith + 1f)
                {
                    NPC.ai[0] = 0f;
                    NPC.ai[1] = 0f;
                    NPC.netUpdate = true;
                    NPC.netSpam = 0;
                }
            }
        }
        else
        {
            if (lookAt.X > 0f)
            {
                NPC.spriteDirection = 1;
                NPC.rotation = MathF.Atan2(lookAt.Y, lookAt.X);
            }
            if (lookAt.X < 0f)
            {
                NPC.spriteDirection = -1;
                NPC.rotation = MathF.Atan2(lookAt.Y, lookAt.X) + MathHelper.Pi;
            }
        }

        if (phase2)
        {
            NPC.damage = (int)Math.Round(NPC.defDamage * 1.3);

            if (NPC.ai[0] < ChargeGateValue_Zenith && !targetDead)
            {
                NPC.ai[0] += 1f;
                if (NPC.ai[0] == ChargeGateValue_Zenith)
                {
                    NPC.netUpdate = true;
                    NPC.netSpam = 0;
                }
            }
        }
        else
            NPC.damage = NPC.defDamage;

        if (targetDead)
        {
            NPC.velocity.Y -= 0.04f;

            if (NPC.timeLeft > 10)
                NPC.timeLeft = 10;
        }
        else if (!attemptingToCharge)
        {
            DemonEyeBatMovement(NPC, maxSpeedX, maxSpeedY, xAccel, xAccelBoost1, xAccelBoost2, yAccel, yAccelBoost1, yAccelBoost2);

            NPC.ai[3] += enrageScale;
            bool shootProjectile = Collision.CanHitLine(NPC.Center, 1, 1, target.Center, 1, 1) && Vector2.Distance(NPC.Center, target.Center) > 240f;
            if (NPC.ai[3] >= projectileShootGateValue && shootProjectile)
            {
                NPC.ai[3] = 0f;
                float projectileSpeed = 16f;
                Vector2 projectileVelocity = (target.Center - NPC.Center).ToCustomLength(projectileSpeed);
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    int projType = ProjectileID.BloodShot;
                    int projDamage = BloodDamage_Zenith;
                    int proj = Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center + projectileVelocity.SafeNormalize(Vector2.UnitY) * 10f, projectileVelocity, projType, projDamage, 0f, Main.myPlayer);
                    Main.projectile[proj].timeLeft = 600;
                }

                NPC.netUpdate = true;
                NPC.netSpam = 0;
            }

            float pushVelocity = 0.2f * enrageScale;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                if (Main.npc[i].active)
                {
                    if (i != NPC.whoAmI && Main.npc[i].type == NPC.type)
                    {
                        if (Vector2.Distance(NPC.Center, Main.npc[i].Center) < 48f * NPC.scale)
                        {
                            if (NPC.position.X < Main.npc[i].position.X)
                                NPC.velocity.X -= pushVelocity;
                            else
                                NPC.velocity.X += pushVelocity;

                            if (NPC.position.Y < Main.npc[i].position.Y)
                                NPC.velocity.Y -= pushVelocity;
                            else
                                NPC.velocity.Y += pushVelocity;
                        }
                    }
                }
            }
        }
        else if (!charging)
        {
            Vector2 destination = target.Center + new Vector2(-280f * NPC.spriteDirection, -280f);
            Vector2 desiredVelocity = (destination - NPC.Center).ToCustomLength(chargeVelocity);
            NPC.SimpleFlyMovement(desiredVelocity, enrageScale);
        }

        if (Main.rand.NextBool(20))
        {
            Vector2 dustSpawnTopLeft = new(NPC.position.X, NPC.position.Y + NPC.height * 0.25f);
            Dust blood = Dust.NewDustDirect(dustSpawnTopLeft, NPC.width, NPC.height / 2, DustID.Blood, NPC.velocity.X, 2f, 0, new Color(128, 0, 0, 255 - NPC.alpha), 1f);
            blood.velocity.X *= 0.5f;
            blood.velocity.Y *= 0.1f;
        }

        static void DemonEyeBatMovement(NPC npc, float maxXSpeed = 6f, float maxYSpeed = 3.5f,
            float xAccel = 0.1f, float xAccelBoost1 = 0.06f, float xAccelBoost2 = 0.25f,
            float yAccel = 0.12f, float yAccelBoost1 = 0.07f, float yAccelBoost2 = 0.2f)
        {
            if (npc.direction == -1 && npc.velocity.X > -maxXSpeed)
            {
                npc.velocity.X -= xAccel;
                if (npc.velocity.X > maxXSpeed)
                {
                    npc.velocity.X -= xAccelBoost1;
                }
                else if (npc.velocity.X > 0f)
                {
                    npc.velocity.X -= xAccelBoost2;
                }
                if (npc.velocity.X < -maxXSpeed)
                {
                    npc.velocity.X = -maxXSpeed;
                }
            }
            else if (npc.direction == 1 && npc.velocity.X < maxXSpeed)
            {
                npc.velocity.X += xAccel;
                if (npc.velocity.X < -maxXSpeed)
                {
                    npc.velocity.X += xAccelBoost1;
                }
                else if (npc.velocity.X < 0f)
                {
                    npc.velocity.X += xAccelBoost2;
                }
                if (npc.velocity.X > maxXSpeed)
                {
                    npc.velocity.X = maxXSpeed;
                }
            }
            if (npc.directionY == -1 && npc.velocity.Y > -maxYSpeed)
            {
                npc.velocity.Y -= yAccel;
                if (npc.velocity.Y > maxYSpeed)
                {
                    npc.velocity.Y -= yAccelBoost1;
                }
                else if (npc.velocity.Y > 0f)
                {
                    npc.velocity.Y -= yAccelBoost2;
                }
                if (npc.velocity.Y < -maxYSpeed)
                {
                    npc.velocity.Y = -maxYSpeed;
                }
            }
            else if (npc.directionY == 1 && npc.velocity.Y < maxYSpeed)
            {
                npc.velocity.Y += yAccel;
                if (npc.velocity.Y < -maxYSpeed)
                {
                    npc.velocity.Y += yAccelBoost1;
                }
                else if (npc.velocity.Y < 0f)
                {
                    npc.velocity.Y += yAccelBoost2;
                }
                if (npc.velocity.Y > maxYSpeed)
                {
                    npc.velocity.Y = maxYSpeed;
                }
            }
        }
    }

    public void FindFrame_Zenith(int frameHeight)
    {
        NPC.frameCounter += 1D;
        if (NPC.frameCounter >= 8D)
            NPC.frame.Y = frameHeight;
        else
            NPC.frame.Y = 0;

        if (NPC.frameCounter >= 16D)
        {
            NPC.frame.Y = 0;
            NPC.frameCounter = 0D;
        }
        if (NPC.life < NPC.lifeMax * 0.5)
            NPC.frame.Y += frameHeight * 2;
    }

    public bool PreDraw_Zenith(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        if (NPC.ai[0] >= ChargeTelegraphGateValue_Zenith)
        {
            Texture2D npcTexture = TextureAssets.Npc[NPC.type].Value;
            Color originalColor = Color.Red * 0.75f;
            Color newColor = EyeofCthulhu_Handler.ChargeColor;
            Vector2 drawPosition = NPC.Center - screenPos + new Vector2(0, NPC.gfxOffY);
            Vector2 origin = NPC.frame.Size() / 2;

            SpriteEffects spriteEffects = SpriteEffects.None;
            if (NPC.spriteDirection == 1)
                spriteEffects = SpriteEffects.FlipHorizontally;

            float telegraphScalar = MathHelper.Clamp((NPC.ai[0] - ChargeTelegraphGateValue_Zenith) / ChargeTelegraphGateValue_Zenith, 0f, 1f);
            Color telegraphColor = Color.Lerp(originalColor, newColor, telegraphScalar) with { A = NPC.GraphicAlpha };

            int afterimageAmount = 10;
            int afterImageIncrement = 2;
            for (int j = 0; j < afterimageAmount; j += afterImageIncrement)
            {
                Color afterimageColor = telegraphColor;
                afterimageColor = Color.Lerp(afterimageColor, originalColor, 0.5f);
                afterimageColor = NPC.GetAlpha(afterimageColor);
                afterimageColor *= (afterimageAmount - j) / 15f;
                Vector2 afterimagePos = NPC.oldPos[j] + new Vector2(NPC.width, NPC.height) / 2f - screenPos;
                afterimagePos -= new Vector2(npcTexture.Width, npcTexture.Height / Main.npcFrameCount[NPC.type]) * NPC.scale / 2f;
                afterimagePos += origin * NPC.scale + new Vector2(0f, NPC.gfxOffY);
                spriteBatch.Draw(npcTexture, afterimagePos, NPC.frame, afterimageColor, NPC.rotation, origin, NPC.scale, spriteEffects, 0f);
            }

            spriteBatch.Draw(npcTexture, drawPosition, NPC.frame, telegraphColor, NPC.rotation, origin, NPC.scale, spriteEffects, 0f);

            return false;
        }

        return true;
    }
    #endregion Zenith
}