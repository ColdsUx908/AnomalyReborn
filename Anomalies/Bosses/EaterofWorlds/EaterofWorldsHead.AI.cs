namespace Anomalies.Bosses.EaterofWorlds;

public sealed partial class EaterofWorldsHead
{
    public static event Action<EaterofWorldsHead> OnRunningPreAI;

    public override bool PreAI()
    {
        if (IsFirstSegment)
        {
            if (!NPC.TargetClosestIfInvalid(true, DespawnDistance))
            {
                NPC.active = false;
                return false;
            }
        }
        else
        {
            if (!ValidHead)
            {
                NPC.active = false;
                return false;
            }

            NPC.realLife = Head.whoAmI;
            NPC.target = Head.target;

            EaterofWorldsHead headBehavior = HeadBehavior;
            CurrentPhase = headBehavior.CurrentPhase;
            CurrentBehavior = headBehavior.CurrentBehavior;
            CurrentAttackPhase = headBehavior.CurrentAttackPhase;
        }

        HandleDiggingEffect();

        switch (CurrentPhase)
        {
            case Phase.Initialize:
                Initialize();
                break;
            case Phase.Phase1 or Phase.Phase1_2:
                Phase1AI();
                break;
        }

        // 更新旋转角度（朝向速度方向）

        NPC.VelocityToRotation(MathHelper.PiOver2);

        // 触发网络同步（若状态变化）

        if (((NPC.velocity.X > 0f && NPC.oldVelocity.X < 0f) || (NPC.velocity.X < 0f && NPC.oldVelocity.X > 0f)
            || (NPC.velocity.Y > 0f && NPC.oldVelocity.Y < 0f) || (NPC.velocity.Y < 0f && NPC.oldVelocity.Y > 0f))
            && !NPC.justHit)
        {
            NPC.netUpdate = true;
        }

        OnRunningPreAI?.Invoke(this);

        return false;

        #region 行为函数
        void Initialize()
        {
            for (int i = 0; i < NPCID.Sets.TrailCacheLength[NPC.type]; i++)
                NPC.oldPos[i] = NPC.position;

            if (IsFirstSegment)
            {
                Head = LocalHead = NPC;
                SegmentIndex = 1; //头部为1号
                List<EaterofWorldsSegmentContainer> allSegments = AllSegmentsDictionary[NPC.whoAmI] = [];
                allSegments.Add(new EaterofWorldsSegmentContainer(NPC, this));

                //生成体节

                int randomXLimit = 80;
                int randomYLimit = 80;
                Vector2 additionalWormSpawnLocation = NPC.Center;

                NPC previous = NPC;

                for (int i = 0; i < BodyCount; i++)
                {
                    additionalWormSpawnLocation += new Vector2(Main.rand.NextFloat(randomXLimit, randomXLimit * 2f) * Main.rand.NextDirectionInt(), Main.rand.NextFloat(randomYLimit, randomYLimit * 2f));
                    NPC.NewNPCAction(SourceAI, additionalWormSpawnLocation, NPCID.EaterofWorldsBody, action: n =>
                    {
                        EaterofWorldsBody behavior = EaterofWorldsBody.GetInstance(n);
                        behavior.Head = behavior.LocalHead = NPC;
                        behavior.Previous = previous;
                        behavior.SegmentIndex = i + 2;
                        previous = n;

                        allSegments.Add(new EaterofWorldsSegmentContainer(n));
                    });
                }

                additionalWormSpawnLocation += new Vector2(Main.rand.NextFloat(randomXLimit, randomXLimit * 2f) * Main.rand.NextDirectionInt(), Main.rand.NextFloat(randomYLimit, randomYLimit * 2f));
                NPC.NewNPCAction(SourceAI, additionalWormSpawnLocation, NPCID.EaterofWorldsTail, action: n =>
                {
                    EaterofWorldsTail behavior = EaterofWorldsTail.GetInstance(n);
                    behavior.Head = behavior.LocalHead = NPC;
                    behavior.Previous = previous;
                    behavior.SegmentIndex = TotalSegmentCount;

                    allSegments.Add(new EaterofWorldsSegmentContainer(n));
                });

                CurrentPhase = Phase.Phase1;
                CurrentBehavior = Behavior.Phase1_Normal;
            }
            else
            {
                //同步所属头部的状态

                EaterofWorldsHead headBehavior = HeadBehavior;

                Timer1 = headBehavior.Timer1;
                Timer2 = headBehavior.Timer2;
                Timer3 = headBehavior.Timer3;
            }
        }

        void HandleDiggingEffect()
        {
            int tilePositionX = (int)(NPC.position.X / 16f) - 1;
            int tileWidthPosX = (int)((NPC.position.X + NPC.width) / 16f) + 2;
            int tilePositionY = (int)(NPC.position.Y / 16f) - 1;
            int tileWidthPosY = (int)((NPC.position.Y + NPC.height) / 16f) + 2;
            if (tilePositionX < 0)
                tilePositionX = 0;
            if (tileWidthPosX > Main.maxTilesX)
                tileWidthPosX = Main.maxTilesX;
            if (tilePositionY < 0)
                tilePositionY = 0;
            if (tileWidthPosY > Main.maxTilesY)
                tileWidthPosY = Main.maxTilesY;

            bool inTiles = false;

            for (int i = tilePositionX; i < tileWidthPosX; i++)
            {
                for (int j = tilePositionY; j < tileWidthPosY; j++)
                {
                    if (TOTileUtils.TryGetTile(i, j, 2, out Tile tile)
                        && ((tile.HasUnactuatedTile && (Main.tileSolid[tile.TileType] || (Main.tileSolidTop[tile.TileType] && tile.TileFrameY == 0)))
                        || tile.LiquidAmount > 64)
                        && NPC.Hitbox.Intersects(new Rectangle(i * 16, j * 16, 16, 16)))
                    {
                        inTiles = true;

                        if (Aroma && TileID.Sets.Platforms[tile.TileType]) //破坏平台
                            WorldGen.KillTile(i, j);
                        else if (Main.rand.NextBool(100) && tile.HasUnactuatedTile)
                            WorldGen.KillTile(i, j, true, true, false);
                    }
                }
            }

            if (inTiles && NPC.soundDelay == 0) //挖掘音效
            {
                Vector2 dest = Target.Center;
                float targetPosX = (int)(dest.X / 16f) * 16 - (int)(NPC.Center.X / 16f) * 16;
                float targetPosY = (int)(dest.Y / 16f) * 16 - (int)(NPC.Center.Y / 16f) * 16;
                float targetDistance = (float)Math.Sqrt(targetPosX * targetPosX + targetPosY * targetPosY);
                float delay = Math.Clamp(targetDistance / 40f, 10f, 20f);
                NPC.soundDelay = (int)delay;
                SoundEngine.PlaySound(SoundID.WormDig, NPC.Center);
            }
        }

        void NormalHeadMovement()
        {
            // ---- 速度与加速度参数 ----

            float velocityScale = 4f;
            float velocityBoost = velocityScale * NPC.LostLifeRatio;
            float accelerationScale = 0.05f;
            float accelerationBoost = accelerationScale * NPC.LostLifeRatio;
            float segmentVelocity = 12f + velocityBoost;
            float segmentAcceleration = 0.15f + accelerationBoost;

            segmentVelocity += NPC.justHit ? 8f : 2f;
            segmentAcceleration += NPC.justHit ? 0.16f : 0.04f;

            if (Ultra)
            {
                segmentVelocity *= 1.2f;
                segmentAcceleration *= 1.2f;
            }

            // 目标位置（按格对齐）

            Vector2 dest = Target.Center;
            float targetPosX = (int)(dest.X / 16f) * 16 - (int)(NPC.Center.X / 16f) * 16;
            float targetPosY = (int)(dest.Y / 16f) * 16 - (int)(NPC.Center.Y / 16f) * 16;
            float targetDistance = (float)Math.Sqrt(targetPosX * targetPosX + targetPosY * targetPosY);

            // 追踪玩家

            float absX = Math.Abs(targetPosX), absY = Math.Abs(targetPosY);
            float timeFactor = segmentVelocity / targetDistance;
            targetPosX *= timeFactor;
            targetPosY *= timeFactor;

            // 加速/减速到目标速度

            if ((NPC.velocity.X > 0f && targetPosX > 0f) || (NPC.velocity.X < 0f && targetPosX < 0f)
                 || (NPC.velocity.Y > 0f && targetPosY > 0f) || (NPC.velocity.Y < 0f && targetPosY < 0f))
            {
                if (NPC.velocity.X < targetPosX)
                    NPC.velocity.X += segmentAcceleration;
                else if (NPC.velocity.X > targetPosX)
                    NPC.velocity.X -= segmentAcceleration;
                if (NPC.velocity.Y < targetPosY)
                    NPC.velocity.Y += segmentAcceleration;
                else if (NPC.velocity.Y > targetPosY)
                    NPC.velocity.Y -= segmentAcceleration;

                if (Math.Abs(targetPosY) < segmentVelocity * 0.2f
                    && ((NPC.velocity.X > 0f && targetPosX < 0f) || (NPC.velocity.X < 0f && targetPosX > 0f)))
                {
                    NPC.velocity.Y += (NPC.velocity.Y > 0f ? segmentAcceleration : -segmentAcceleration) * 2f;
                }
                if (Math.Abs(targetPosX) < segmentVelocity * 0.2f
                    && ((NPC.velocity.Y > 0f && targetPosY < 0f) || (NPC.velocity.Y < 0f && targetPosY > 0f)))
                {
                    NPC.velocity.X += (NPC.velocity.X > 0f ? segmentAcceleration : -segmentAcceleration) * 2f;
                }
            }
            else if (absX > absY)
            {
                if (NPC.velocity.X < targetPosX)
                    NPC.velocity.X += segmentAcceleration * 1.1f;
                else if (NPC.velocity.X > targetPosX)
                    NPC.velocity.X -= segmentAcceleration * 1.1f;
                if ((Math.Abs(NPC.velocity.X) + Math.Abs(NPC.velocity.Y)) < segmentVelocity * 0.5)
                    NPC.velocity.Y += NPC.velocity.Y > 0f ? segmentAcceleration : -segmentAcceleration;
            }
            else
            {
                if (NPC.velocity.Y < targetPosY)
                    NPC.velocity.Y += segmentAcceleration * 1.1f;
                else if (NPC.velocity.Y > targetPosY)
                    NPC.velocity.Y -= segmentAcceleration * 1.1f;
                if ((Math.Abs(NPC.velocity.X) + Math.Abs(NPC.velocity.Y)) < segmentVelocity * 0.5)
                    NPC.velocity.X += NPC.velocity.X > 0f ? segmentAcceleration : -segmentAcceleration;
            }

            int headCount = NPC.CountNPCS(NPC.type);
            if (headCount > 0) //头部互相推挤
            {
                headCount = Math.Min(headCount - 1, 7);
                float pushDist = MathHelper.Lerp(30f, 100f, NPC.LostLifeRatio) * NPC.scale;
                const float pushSpeed = 0.25f;
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC other = Main.npc[i];
                    if (other.active && i != NPC.whoAmI && other.type == NPC.type && Vector2.Distance(NPC.Center, other.Center) < pushDist)
                    {
                        if (NPC.position.X < other.position.X)
                            NPC.velocity.X -= pushSpeed;
                        else
                            NPC.velocity.X += pushSpeed;
                        if (NPC.position.Y < other.position.Y)
                            NPC.velocity.Y -= pushSpeed;
                        else
                            NPC.velocity.Y += pushSpeed;
                    }
                }
            }
        }

        void Phase1AI()
        {
            switch (CurrentBehavior)
            {
                case Behavior.Phase1_Normal:
                    Normal();
                    break;
                case Behavior.Phase1_Split:
                    Split();
                    break;
                case Behavior.Phase1_Combine:
                    Combine();
                    break;
            }
        }

        void Normal()
        {
            NormalHeadMovement();

            //诅咒焰

            Timer1++;
            Timer2++;

            if (Timer2 >= 60
                && TOSharedData.NotClient
                && Collision.CanHitLine(NPC.Center, 1, 1, Target.Center, 1, 1)
                && Vector2.IncludedAngle(NPC.velocity, Target.Center - NPC.Center) <= TOMathUtils.PiOver3) //发射
            {
                Timer2 = 0;

                Vector2 velocity = NPC.GetVelocityTowards(Target, Ultra ? 20f : 15f) + NPC.velocity * 0.25f;
                int type = ProjectileID.CursedFlameHostile;
                Projectile.NewProjectileAction(SourceAI, NPC.Center + NPC.velocity, velocity, type, CursedFireballDamage, 0f);
            }

            if (Timer2 >= 40 && Main.rand.NextProbability(0.6f)) //预警尘埃
            {
                Vector2 dustCenter = NPC.Center + Main.rand.NextPolarVector2(10f, 30f);
                int dustType = DustID.CursedTorch;
                Dust.NewDustPerfectAction(dustCenter, dustType, d =>
                {
                    d.scale = 2.5f;
                    d.noGravity = true;
                });
            }

            if (Timer1 >= 300)
            {
                CurrentBehavior = CurrentlySplit ? Behavior.Phase1_Combine : Behavior.Phase1_Split;
                Timer1 = 0;
            }
        }

        void Split() => TransformTo(3);

        void Combine() => TransformTo(1);

        void TransformTo(int amount)
        {
            float radius = 50f;

            if (!IsFirstSegment)
                CoilingCenter = HeadBehavior.CoilingCenter;

            switch (CurrentAttackPhase)
            {
                case 0: //选定盘绕地点
                    Timer1++;

                    if (Timer1 == 1)
                    {
                        if (IsFirstSegment)
                            SoundEngine.PlaySound(SoundID.ForceRoar);

                        if (IsFirstSegment)
                        {
                            CoilingCenter = Target.Center + Main.rand.NextPolarVector2(200f, 400f);
                            ParticleHandler.SpawnParticle(new BloomParticle(CoilingCenter, Vector2.Zero, Color.Lerp(Color.Purple, Color.Magenta, 0.05f), 0f, 4.5f, 450, 0.9f));
                            ParticleHandler.SpawnParticle(new BloomParticle(CoilingCenter, Vector2.Zero, Color.Lerp(Color.White, Color.Magenta, 0.05f), 0f, 3.5f, 450, 0.9f));
                        }
                    }
                    else if (Timer1 == 60)
                    {
                        Timer1 = 0;
                        CurrentAttackPhase = 1;
                    }
                    break;

                case 1: //高度移动，进入盘绕状态
                    NPC.HomeIn(CoilingCenter, HomingAlgorithm.SmoothStep, 0.3f);
                    if (NPC.velocity.Length() < 30f)
                        NPC.velocity *= 1.05f;
                    if (NPC.Distance(CoilingCenter) < radius)
                    {
                        Vector2 direction = NPC.Center - CoilingCenter;

                        CoilingStartAngle = direction.ToRotation();

                        if (direction == Vector2.Zero)
                            direction = Main.rand.NextPolarVector2(radius);
                        else
                            direction.Modulus = radius;
                        NPC.Center = CoilingCenter + direction;

                        CurrentAttackPhase = 2;

                        goto case 2;
                    }

                    break;

                case 2: //绕中心做高速圆周运动
                    Timer1++;
                    float angleSpeed = MathHelper.TwoPi / 8f;

                    Vector2 destination = CoilingCenter + new PolarVector2(radius, CoilingStartAngle + angleSpeed * Timer1);
                    NPC.SetVelocityandRotation(destination - NPC.Center, MathHelper.PiOver2);

                    if (Timer1 >= 300)
                    {
                        CurrentAttackPhase = 0;
                        Timer1 = 0;
                        CurrentBehavior = Behavior.Phase1_Normal;

                        if (IsFirstSegment)
                        {
                            SoundEngine.PlaySound(SoundID.ForceRoar);

                            List<NPC> list = TransformToWorms(amount);
                            PolarVector2 velocity = amount == 1 ? (PolarVector2)NPC.GetVelocityTowards(Target, 40f) : Main.rand.NextPolarVector2(40f);
                            for (int i = 0; i < list.Count; i++)
                            {
                                NPC head = list[i];
                                head.Center = CoilingCenter;
                                head.velocity = velocity.RotatedBy(MathHelper.TwoPi / list.Count * i);
                            }
                        }

                        CurrentlySplit = amount > 1;
                    }

                    break;
            }
        }
        #endregion 行为函数
    }
}
